﻿Imports System.Net
Imports System.Net.Sockets
Imports System.Net.NetworkInformation
Imports System.IO
Imports System.Runtime.Serialization.Formatters.Binary
Imports System.Text
Imports System.Threading
''' <summary>
''' Provide a client class.
''' </summary>
''' <remarks></remarks>
Public Class client
    Private tcpClient As TcpClient = Nothing

    Private tcpClientNetStream As NetworkStream = Nothing

    Private connected As Boolean = False

    Private _closeDelay As Integer = 100

    Private lockListen As Object = New Object()

    Private lockSend As Object = New Object()

    Private clientData As New List(Of String)

    Private packets As New List(Of packet)

    Private thisClient As String = ""

    Private encryptmethod As EncryptionMethod = EncryptionMethod.none

    Private _clientrefreshdelay As Integer = 15000

    Private password As String = ""

    Private listenthread As Thread = Nothing

    Private updatethread As Thread = Nothing

    Private synclockcheckl As Boolean = False

    Private synclockchecks As Boolean = False

    Private _ip As String = ""

    Private _port As Integer = 0
    ''' <summary>
    ''' Raised when a connection is successful.
    ''' </summary>
    ''' <remarks></remarks>
    Public Event ServerConnect()
    ''' <summary>
    ''' Raised when disconnected by the server.
    ''' </summary>
    ''' <remarks></remarks>
    Public Event ServerDisconnect()
    ''' <summary>
    ''' Raised a message is received.
    ''' </summary>
    ''' <param name="message">The packet received.</param>
    ''' <remarks></remarks>
    Public Event ServerMessage(ByVal message As packet)
    ''' <summary>
    ''' Raised everytime an error occurs.
    ''' </summary>
    ''' <param name="ex">The exception that occured.</param>
    ''' <remarks></remarks>
    Public Event errEncounter(ByVal ex As Exception)

    ''' <summary>
    ''' Flushes this instance of client (Cleaning).
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub flush()
        If Not connected And Not synclockcheckl And Not synclockchecks Then
            tcpClient = Nothing

            connected = False

            _closeDelay = 100

            lockListen = New Object()

            lockSend = New Object()

            clientData = New List(Of String)

            packets = New List(Of packet)

            thisClient = ""

            encryptmethod = EncryptionMethod.none

            _clientrefreshdelay = 15000

            password = ""

            listenthread = Nothing

            updatethread = Nothing

            synclockcheckl = False

            synclockchecks = False

            _ip = ""

            _port = 0
        End If
    End Sub

    ''' <summary>
    ''' Gets or Sets the name of the client.
    ''' </summary>
    ''' <value>the name of the client.</value>
    ''' <returns>the name of the client.</returns>
    ''' <remarks></remarks>
    Public Property Name() As String
        Get
            Return thisClient
        End Get
        Set(value As String)
            Dim arex As Boolean = False
            For i As Integer = 0 To clientData.Count - 1
                If clientData(i) = value Then
                    arex = True
                    Exit For
                End If
            Next
            If Not (arex) Then
                clientData.Remove(thisClient)
                clientData.Add(value)
                Dim retyt As String = Send(New packet(0, thisClient, New List(Of String), "system", "client:" & value, password, encryptmethod))
                If retyt.ToLower = "success" Then
                    thisClient = value
                End If
            End If
        End Set
    End Property
    ''' <summary>
    ''' Returns the current connection state (True if connected).
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property isConnected() As Boolean
        Get
            Return connected
        End Get
    End Property
    ''' <summary>
    ''' Gets or Sets the delay to recieve messages before the client is disconnected.
    ''' </summary>
    ''' <value>the delay to recieve messages before the client is disconnected.</value>
    ''' <returns>the delay to recieve messages before the client is disconnected.</returns>
    ''' <remarks></remarks>
    Public Property CloseDelay() As Integer
        Get
            Return _closeDelay
        End Get
        Set(value As Integer)
            _closeDelay = value
        End Set
    End Property
    ''' <summary>
    ''' Gets or Sets the delay to refresh the local client list.
    ''' </summary>
    ''' <value>the delay to refresh the local client list.</value>
    ''' <returns>the delay to refresh the local client list.</returns>
    ''' <remarks></remarks>
    Public Property ClientRefreshDelay() As Integer
        Get
            Return _clientrefreshdelay
        End Get
        Set(value As Integer)
            _clientrefreshdelay = value
        End Set
    End Property

    ''' <summary>
    ''' Creates a new instance of client.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub New()
        tcpClient = New TcpClient()
    End Sub
    ''' <summary>
    ''' The number of bytes to send.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property SendBufferSize() As Integer
        Get
            Return tcpClient.SendBufferSize
        End Get
        Set(ByVal value As Integer)
            tcpClient.SendBufferSize = value
        End Set
    End Property
    ''' <summary>
    ''' The number of bytes to receive.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ReceiveBufferSize() As Integer
        Get
            Return tcpClient.ReceiveBufferSize
        End Get
        Set(ByVal value As Integer)
            tcpClient.ReceiveBufferSize = value
        End Set
    End Property
    ''' <summary>
    ''' Determines if the server should wait an amount of time so more data will be added to the send data buffer, if set to true, the server will send the data immediatly.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property NoDelay() As Boolean
        Get
            Return tcpClient.NoDelay
        End Get
        Set(ByVal value As Boolean)
            tcpClient.NoDelay = value
        End Set
    End Property
    ''' <summary>
    ''' Connect to a server.
    ''' </summary>
    ''' <param name="Clientname">The name of the client.</param>
    ''' <param name="ipaddress">The IP address of the server.</param>
    ''' <param name="port">The port of the server.</param>
    ''' <param name="password2">The server password to use (Optional if encrypt type is none or unicode only).</param>
    ''' <param name="encrypttype">The encrypt type to use none, unicode, ase and unicodease (ase and unicode ase require a password).</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function Connect(Clientname As String, ipaddress As String, Optional port As Integer = 100, Optional password2 As String = "", Optional encrypttype As EncryptionMethod = EncryptionMethod.none) As Boolean
        Dim result As Boolean = False
        Try
            If connected Then
                result = True
            Else
                thisClient = Clientname
                password = password2
                encrypttype = encrypttype
                _port = port
                _ip = ipaddress
                listenthread = New Thread(New ThreadStart(AddressOf Listen))
                listenthread.IsBackground = True
                listenthread.Start()
                updatethread = New Thread(New ThreadStart(AddressOf updatedata))
                updatethread.IsBackground = True
                updatethread.Start()
                result = True
            End If
        Catch ex As Exception
            result = False
            RaiseEvent errEncounter(ex)
        End Try
        Return result
    End Function

    Private Sub Listen()
        Try
            tcpClient.Connect(_ip, _port)
            If tcpClient.Connected Then
                'Send the client name
                tcpClientNetStream = tcpClient.GetStream()
                tcpClientNetStream.Write(Convert2Ascii(thisClient), 0, thisClient.Length)
                tcpClientNetStream.Flush()
                connected = True
                RaiseEvent ServerConnect()
            End If
        Catch ex As Exception
            RaiseEvent errEncounter(ex)
        End Try
        While tcpClient.Connected And connected
            Try
                Dim bytes(tcpClient.ReceiveBufferSize) As Byte
                tcpClientNetStream.Read(bytes, 0, tcpClient.ReceiveBufferSize)
                DecryptBytes(bytes)
            Catch ex As Exception
                Exit While
            End Try
            Thread.Sleep(150)
        End While
        Disconnect()
    End Sub

    Private Sub DecryptBytes(ByVal Message() As Byte)
        Dim Disconnected As Boolean = True
        For b = 0 To Message.Length - 1
            Dim Msg As intmsg = intmsg.FromBytes(Message, b)
            If Not Msg.Data Is Nothing Then
                Dim packet As packet = string2packet(ConvertFromAscii(Msg.Data))
                servermsgpr(packet)
                Disconnected = False
            End If
        Next b
        If Disconnected Then connected = False
    End Sub

    Private Sub updatedata()
        While connected
            If tcpClient.Connected Then
                Send(New packet(0, thisClient, New List(Of String), "system", "clients", password, encryptmethod))
                Send(New packet(0, thisClient, New List(Of String), "system", "client", password, encryptmethod))
            End If
            Thread.Sleep(_clientrefreshdelay)
        End While
        Thread.CurrentThread.Abort()
    End Sub
    ''' <summary>
    ''' Forces the client list to update.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub UpdateClientData()
        If connected Then
            If tcpClient.Connected Then
                Send(New packet(0, thisClient, New List(Of String), "system", "clients", password, encryptmethod))
                Send(New packet(0, thisClient, New List(Of String), "system", "client", password, encryptmethod))
            End If
        End If
    End Sub
    ''' <summary>
    ''' The currently connected clients on the server.
    ''' </summary>
    ''' <value>The currently connected clients on the server.</value>
    ''' <returns>The currently connected clients on the server.</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property connected_clients As List(Of String)
        Get
            Return clientData
        End Get
    End Property
    ''' <summary>
    ''' Check if a server is up.
    ''' </summary>
    ''' <param name="ipadress">The server IP address.</param>
    ''' <param name="port">The server port.</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function CheckServer(ipadress As String, port As Integer) As Boolean
        Try
            Dim tcpClientc As TcpClient = New TcpClient(ipadress, port)
            If tcpClientc.Connected Then
                Dim optionValue As LingerOption = New LingerOption(False, 0)
                tcpClientc.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, optionValue)
                tcpClientc.Client.Close(0)
                Return True
            End If
        Catch ex_75 As Exception
        End Try
        Return False
    End Function
    ''' <summary>
    ''' Disconnect from the server.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Disconnect()
        On Error Resume Next
        Thread.Sleep(_closeDelay)
        connected = False
        Dim SBufferSize, RBufferSize As Integer
        SBufferSize = tcpClient.SendBufferSize
        RBufferSize = tcpClient.ReceiveBufferSize
        Dim NDelay As Boolean = tcpClient.NoDelay
        If Not tcpClient Is Nothing Then tcpClient.Close()
        tcpClient = New TcpClient()
        tcpClient.SendBufferSize = SBufferSize
        tcpClient.ReceiveBufferSize = RBufferSize
        tcpClient.NoDelay = NDelay
        RaiseEvent ServerDisconnect()
    End Sub
    ''' <summary>
    ''' Kill the operating threads if they are still alive.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub kill_threads()
        If Not connected And Not synclockcheckl And Not synclockchecks Then
            While updatethread.IsAlive
                Thread.Sleep(150)
                If updatethread.ThreadState = ThreadState.AbortRequested Or updatethread.ThreadState = 132 Then
                    Exit While
                ElseIf Not synclockcheckl And Not synclockchecks Then
                    updatethread.Abort()
                End If
            End While
            While listenthread.IsAlive
                Thread.Sleep(150)
                If listenthread.ThreadState = ThreadState.AbortRequested Or listenthread.ThreadState = 132 Then
                    Exit While
                ElseIf Not synclockcheckl And Not synclockchecks Then
                    listenthread.Abort()
                End If
            End While
        End If
    End Sub
    ''' <summary>
    ''' Sends a message the server.
    ''' </summary>
    ''' <param name="message">The packet to send.</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function Send(message As packet) As Boolean
        Dim result As Boolean = False
        SyncLock lockSend
            synclockchecks = True
            Try
                If Not tcpClient.Connected Then
                    result = False
                Else
                    Dim bytes As Byte() = intmsg.GetBytes(New intmsg(MainEncoding.GetBytes(packet2string(message))))
                    tcpClientNetStream.Write(bytes, 0, bytes.Length)
                    tcpClientNetStream.Flush()
                    result = True
                End If
            Catch ex As Exception
                result = False
                RaiseEvent errEncounter(ex)
            End Try
            synclockchecks = False
        End SyncLock
        Return result
    End Function

    Private Sub servermsgpr(message As packet)
        SyncLock lockListen
            synclockcheckl = True
            Dim clientnolst As New List(Of String)
            If message.header.ToLower.StartsWith("system") Then
                If message.stringdata(password).ToLower = "disconnect" Then
                    RaiseEvent ServerDisconnect()
                ElseIf message.stringdata(password).ToLower.EndsWith(":connected") Then
                    Dim colonindx As Integer = message.stringdata(password).ToLower.IndexOf(":")
                    Dim cilname As String = message.stringdata(password).Substring(0, colonindx + 1)
                    clientData.Add(cilname)
                    Send(New packet(0, thisClient, New List(Of String), "system", "clients", password, encryptmethod))
                    Send(New packet(0, thisClient, New List(Of String), "system", "client", password, encryptmethod))
                ElseIf message.stringdata(password).ToLower.EndsWith(":disconnected") Then
                    Dim colonindx As Integer = message.stringdata(password).ToLower.IndexOf(":")
                    Dim cilname As String = message.stringdata(password).Substring(0, colonindx + 1)
                    clientData.Remove(cilname)
                    Send(New packet(0, thisClient, New List(Of String), "system", "clients", password, encryptmethod))
                    Send(New packet(0, thisClient, New List(Of String), "system", "client", password, encryptmethod))
                ElseIf message.header.ToLower.StartsWith("system:clients") Then
                    clientData = DirectCast(message.objectdata(password), List(Of String))
                ElseIf message.header.ToLower.StartsWith("system:name") Then
                    thisClient = DirectCast(message.objectdata(password), String)
                End If
            Else
                RaiseEvent ServerMessage(message)
            End If
            synclockcheckl = False
        End SyncLock
    End Sub

End Class