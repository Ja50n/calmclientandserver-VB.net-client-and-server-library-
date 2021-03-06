﻿Imports System.IO
Imports System.Runtime.Serialization.Formatters.Binary
Imports System.Security
Imports System.Security.Cryptography
Imports System.Text
''' <summary>
''' The packet class.
''' </summary>
''' <remarks></remarks>
<Serializable()>
Public Class packet
    Private _refnumber As Integer = 0
    Private _sender As String = ""
    Private _receivers As New List(Of String)
    Private _header As String = ""
    Private _data As String = ""
    Private _isobj As Boolean = False
    Private _isencrypted As Boolean = False
    Private _encryptmethod As EncryptionMethod = EncryptionMethod.none
    Private _ispacketvalid As Boolean = False

    'useful for ping packets
    ''' <summary>
    ''' Creates a new invalid packet.
    ''' </summary>
    ''' <remarks>(Useful for ping packets)</remarks>
    Public Sub New()
        _ispacketvalid = False
    End Sub

    'string construction
    ''' <summary>
    ''' Creates a new valid packet with basic data and no encryption [string data].
    ''' </summary>
    ''' <param name="refnumber">Reference Number.</param>
    ''' <param name="sender">The Sender Name.</param>
    ''' <param name="receivers">The Name(s) of the Receivers.</param>
    ''' <param name="header">The Header Data.</param>
    ''' <param name="data">The Data to Send [String].</param>
    ''' <remarks></remarks>
    Public Sub New(refnumber As Integer, sender As String, receivers As List(Of String), header As String, data As String)
        Try
            _refnumber = refnumber
            _ispacketvalid = True
            _sender = sender
            _receivers = receivers
            _header = header
            _data = data
            _isobj = False
        Catch ex As Exception
        End Try
    End Sub

    'object construction
    ''' <summary>
    ''' Creates a new valid packet with basic data and no encryption [encapsulated data].
    ''' </summary>
    ''' <param name="refnumber">Reference Number.</param>
    ''' <param name="sender">The Sender Name.</param>
    ''' <param name="receivers">The Name(s) of the Receivers.</param>
    ''' <param name="header">The Header Data.</param>
    ''' <param name="data">The Data to Send [Encapsulation].</param>
    ''' <remarks></remarks>
    Public Sub New(refnumber As Integer, sender As String, receivers As List(Of String), header As String, data As encapsulation)
        Try
            _refnumber = refnumber
            _ispacketvalid = True
            _sender = sender
            _receivers = receivers
            _header = header
            _data = data.data
            _isobj = True
        Catch ex As Exception
        End Try
    End Sub

    'string construction with encryption
    ''' <summary>
    ''' Creates a new valid packet with basic data and encryption [string data].
    ''' </summary>
    ''' <param name="refnumber">Reference Number.</param>
    ''' <param name="sender">The Sender Name.</param>
    ''' <param name="receivers">The Name(s) of the Receivers.</param>
    ''' <param name="header">The Header Data.</param>
    ''' <param name="data">The Data to Send [String].</param>
    ''' <param name="password">The password to protect the data (Optional if using no encryption or unicode only encryption).</param>
    ''' <param name="encryptmethod">The method to encrypt the data (No and Unicode Only encryption do not require a password while Ase and Unicode and Ase encryption do require a password).</param>
    ''' <remarks></remarks>
    Public Sub New(refnumber As Integer, sender As String, receivers As List(Of String), header As String, data As String, password As String, encryptmethod As EncryptionMethod)
        Try
            _refnumber = refnumber
            _ispacketvalid = True
            _sender = sender
            _receivers = receivers
            _header = header
            _data = data
            _isobj = False
            If encryptmethod > 0 And encryptmethod < 4 Then
                _encryptmethod = encryptmethod
                _isencrypted = True
            End If
            If _isencrypted Then
                If encryptmethod = EncryptionMethod.unicode Then
                    Dim oldstring As String = _data
                    _data = ""
                    For i As Integer = 0 To oldstring.Length - 1 Step 1
                        _data = _data & AscW(oldstring.Substring(i, 1)).ToString() & " "
                    Next
                ElseIf encryptmethod = EncryptionMethod.ase Then
                    Dim oldstring As String = _data
                    _data = ""
                    _data = EncryptString(oldstring, password)
                ElseIf encryptmethod = EncryptionMethod.unicodease Then
                    Dim oldstring As String = _data
                    _data = ""
                    Dim oldstring2 As String = EncryptString(oldstring, password)
                    For i As Integer = 0 To oldstring2.Length - 1 Step 1
                        _data = _data & AscW(oldstring2.Substring(i, 1)).ToString() & " "
                    Next
                End If
            End If
        Catch ex As Exception
        End Try
    End Sub

    'object construction with encryption
    ''' <summary>
    ''' Creates a new valid packet with basic data and encryption [encapsulated data].
    ''' </summary>
    ''' <param name="refnumber">Reference Number.</param>
    ''' <param name="sender">The Sender Name.</param>
    ''' <param name="receivers">The Name(s) of the Receivers.</param>
    ''' <param name="header">The Header Data.</param>
    ''' <param name="data">The Data to Send [Encapsulation].</param>
    ''' <param name="password">The password to protect the data (Optional if using no encryption or unicode only encryption).</param>
    ''' <param name="encryptmethod">The method to encrypt the data (No and Unicode Only encryption do not require a password while Ase and Unicode and Ase encryption do require a password).</param>
    ''' <remarks></remarks>
    Public Sub New(refnumber As Integer, sender As String, receivers As List(Of String), header As String, data As encapsulation, password As String, encryptmethod As EncryptionMethod)
        Try
            _refnumber = refnumber
            _ispacketvalid = True
            _sender = sender
            _receivers = receivers
            _header = header
            _data = data.data
            _isobj = True
            If encryptmethod > 0 And encryptmethod < 4 Then
                _encryptmethod = encryptmethod
                _isencrypted = True
            End If
            If _isencrypted Then
                If encryptmethod = EncryptionMethod.unicode Then
                    Dim oldstring As String = _data
                    _data = ""
                    For i As Integer = 0 To oldstring.Length - 1 Step 1
                        _data = _data & AscW(oldstring.Substring(i, 1)).ToString() & " "
                    Next
                ElseIf encryptmethod = EncryptionMethod.ase Then
                    Dim oldstring As String = _data
                    _data = ""
                    _data = EncryptString(oldstring, password)
                ElseIf encryptmethod = EncryptionMethod.unicodease Then
                    Dim oldstring As String = _data
                    _data = ""
                    Dim oldstring2 As String = EncryptString(oldstring, password)
                    For i As Integer = 0 To oldstring2.Length - 1 Step 1
                        _data = _data & AscW(oldstring2.Substring(i, 1)).ToString() & " "
                    Next
                End If
            End If
        Catch ex As Exception
        End Try
    End Sub

    'from other packet
    ''' <summary>
    ''' Creates a packet from another packet.
    ''' </summary>
    ''' <param name="packet">The packet to duplicate.</param>
    ''' <remarks></remarks>
    Public Sub New(packet As packet)
        Try
            If packet.isvalidpacket Then
                _ispacketvalid = True
                _sender = packet.sender
                _receivers = packet.receivers
                _header = packet.header
                _data = packet.data
                If packet.encryptmethod = 0 Then
                    _isencrypted = False
                    _encryptmethod = EncryptionMethod.none
                Else
                    If packet.encryptmethod > 0 And packet.encryptmethod < 4 Then
                        _encryptmethod = packet.encryptmethod
                        _isencrypted = True
                    Else
                        _isencrypted = False
                        _encryptmethod = EncryptionMethod.none
                    End If
                End If
                _refnumber = packet.referencenumber
                _isobj = packet.hasobject
            Else
                _ispacketvalid = False
            End If
        Catch ex As Exception
        End Try
    End Sub
    ''' <summary>
    ''' Returns if the packet is valid.
    ''' </summary>
    ''' <value>Returns if the packet is valid.</value>
    ''' <returns>Returns if the packet is valid.</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property isvalidpacket As Boolean
        Get
            Return _ispacketvalid
        End Get
    End Property
    ''' <summary>
    ''' Returns the sender's name.
    ''' </summary>
    ''' <value>Returns the sender's name.</value>
    ''' <returns>Returns the sender's name.</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property sender As String
        Get
            Return _sender
        End Get
    End Property
    ''' <summary>
    ''' Returns the names of the receivers.
    ''' </summary>
    ''' <value>Returns the names of the receivers.</value>
    ''' <returns>Returns the names of the receivers.</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property receivers As List(Of String)
        Get
            Return _receivers
        End Get
    End Property
    ''' <summary>
    ''' Gets the header of the packet.
    ''' </summary>
    ''' <value>Gets the header of the packet.</value>
    ''' <returns>Gets the header of the packet.</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property header As String
        Get
            Return _header
        End Get
    End Property
    ''' <summary>
    ''' Returns the encryption method.
    ''' </summary>
    ''' <value>Returns the encryption method.</value>
    ''' <returns>Returns the encryption method.</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property encryptmethod As Integer
        Get
            Return _encryptmethod
        End Get
    End Property
    ''' <summary>
    ''' Returns the packet's reference number.
    ''' </summary>
    ''' <value>Returns the packet's reference number.</value>
    ''' <returns>Returns the packet's reference number.</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property referencenumber As Integer
        Get
            Return _refnumber
        End Get
    End Property
    ''' <summary>
    ''' Returns the data of the packet [string form].
    ''' </summary>
    ''' <value>Returns the data of the packet [string form].</value>
    ''' <returns>Returns the data of the packet [string form].</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property data As String
        Get
            Return _data
        End Get
    End Property
    ''' <summary>
    ''' Returns if the packet contains an object.
    ''' </summary>
    ''' <value>Returns if the packet contains an object.</value>
    ''' <returns>Returns if the packet contains an object.</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property hasobject As Boolean
        Get
            Return _isobj
        End Get
    End Property
    ''' <summary>
    ''' Returns the data held by the packet as a string.
    ''' </summary>
    ''' <param name="password">The password (If the data is encrypted with ase of unicodease).</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function stringdata(Optional password As String = "") As String
        Try
            If isvalidpacket Then
                Dim data As String = ""
                If _encryptmethod = EncryptionMethod.unicode Or _encryptmethod = EncryptionMethod.unicodease Then
                    Dim chars As String = ""
                    For i As Integer = 0 To _data.Length - 1 Step 1
                        Dim cchr As String = _data.Substring(i, 1)
                        If cchr = " " Then
                            Dim conint As Integer = 0
                            Try
                                conint = CInt(chars)
                            Catch ex As Exception
                                conint = 0
                            End Try
                            data = data & ChrW(conint)
                            chars = ""
                        Else
                            chars = chars & cchr
                        End If
                    Next
                Else
                    data = _data
                End If
                Dim returned As String = ""
                If (password <> "" Or password <> Nothing) And (_encryptmethod = EncryptionMethod.ase Or _encryptmethod = EncryptionMethod.unicodease) Then
                    returned = DecryptString(data, password)
                Else
                    returned = data
                End If
                Return returned
            End If
        Catch ex As Exception
        End Try
        Return ""
    End Function
    ''' <summary>
    ''' Returns the data held by the packet as an object.
    ''' </summary>
    ''' <param name="password">The password (If the data is encrypted with ase of unicodease).</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function objectdata(Optional password As String = "") As Object
        Try
            If _isobj And isvalidpacket Then
                Dim data As String = ""
                If _encryptmethod = EncryptionMethod.unicode Or _encryptmethod = EncryptionMethod.unicodease Then
                    Dim chars As String = ""
                    For i As Integer = 0 To _data.Length - 1 Step 1
                        Dim cchr As String = _data.Substring(i, 1)
                        If cchr = " " Then
                            Dim conint As Integer = 0
                            Try
                                conint = CInt(chars)
                            Catch ex As Exception
                                conint = 0
                            End Try
                            data = data & ChrW(conint)
                            chars = ""
                        Else
                            chars = chars & cchr
                        End If
                    Next
                Else
                    data = _data
                End If
                Dim returned As String = ""
                If password <> "" Or password <> Nothing Then
                    returned = DecryptString(data, password)
                Else
                    returned = data
                End If
                Dim retobj As Object = convertstringtoobject(returned)
                Return retobj
            End If
        Catch ex As Exception
        End Try
        Return Nothing
    End Function
End Class
''' <summary>
''' Packet Encryption Method.
''' </summary>
''' <remarks></remarks>
<Serializable()>
Public Enum EncryptionMethod As Integer
    none = 0
    unicode = 1
    ase = 2
    unicodease = 3
End Enum
