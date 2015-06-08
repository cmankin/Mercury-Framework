Imports Ionic.Zip
Imports System.IO

Namespace Document

    ''' <summary>
    ''' Represents a set of information for a packaged (ZIP) file.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class PackageFileInfo

        ' Save event
        Public Event FileContentSaved(ByVal sender As Object, ByVal e As EventArgs)

        ' Attributes
        Private _parentPackage As FilePackage
        Private _relativePath As String
        Private _name As String

        ' Constants
        Private Const EXT_DOT As String = "."

#Region "Constructors"

        ''' <summary>
        ''' Initializes a default instance of the <c>PackageFileInfo</c> 
        ''' with the specified <c>FilePackage</c> instance.
        ''' </summary>
        ''' <param name="parent">Owning <c>FilePackage</c> instance.</param>
        ''' <remarks></remarks>
        Public Sub New(ByVal parent As FilePackage)
            Me._parentPackage = parent
        End Sub

        ''' <summary>
        ''' Initializes a default instance of the <c>PackageFileInfo</c> 
        ''' with the specified <c>FilePackage</c> instance and resource 
        ''' file relative system path.
        ''' </summary>
        ''' <param name="parent">Owning <c>FilePackage</c> instance.</param>
        ''' <param name="path">Relative path from package root to resource file.</param>
        Public Sub New(ByVal parent As FilePackage, ByVal path As String)
            Me._parentPackage = parent
            Me.RelativePath = path
        End Sub

#End Region

#Region "Properties"

        ''' <summary>
        ''' Gets the parent package to which this <c>PackageDirectoryInfo</c> belongs.
        ''' </summary>
        ''' <returns>the parent package to which this <c>PackageDirectoryInfo</c> belongs.</returns>
        Public ReadOnly Property ParentPackage() As FilePackage
            Get
                Return Me._parentPackage
            End Get
        End Property

        ''' <summary>
        ''' Gets the path to the parent package.
        ''' </summary>
        ''' <returns>The path to the parent package.</returns>
        Public ReadOnly Property ParentPath() As String
            Get
                Return Me.ParentPackage.Path
            End Get
        End Property

        ''' <summary>
        ''' Gets the full system path to the file.
        ''' </summary>
        ''' <returns>The full system path to the file.</returns>
        Public ReadOnly Property FullPath() As String
            Get
                Return String.Format("{0}\{1}", Me.ParentPackage.Path, _
                                     Replace(Me._relativePath, "/", "\"))
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the relative path from the package root to the file.
        ''' </summary>
        ''' <value>The relative path from the package root to the file.</value>
        ''' <returns>The relative path from the package root to the file.</returns>
        ''' <remarks></remarks>
        Public Property RelativePath() As String
            Get
                Return Me._relativePath
            End Get
            Set(ByVal value As String)
                Me._relativePath = value
                Me._name = GetNameFromPath(Me._relativePath, "/")
            End Set
        End Property

        ''' <summary>
        ''' Gets the name of the file.
        ''' </summary>
        ''' <returns>The name of the file.</returns>
        Public ReadOnly Property Name() As String
            Get
                Return Me._name
            End Get
        End Property

#End Region

#Region "Methods"

        ''' <summary>
        ''' Extracts the contents of the file specified by this 
        ''' <c>PackageFileInfo</c> into the provided <c>StreamReader</c>.
        ''' </summary>
        ''' <param name="reader"><c>StreamReader</c> to fill with file contents.</param>
        Public Sub Extract(ByRef reader As StreamReader)
            Dim mStream As Stream = New MemoryStream()
            Using zip As New ZipFile(Me.ParentPath)
                For Each e As ZipEntry In zip.Entries
                    If e.FileName = Me.RelativePath Then
                        ' Extract into memory stream
                        e.Extract(mStream)
                        ' Seek to beginning and create reader
                        mStream.Seek(0, SeekOrigin.Begin)
                        reader = New StreamReader(mStream)
                        Return
                    End If
                Next
            End Using
        End Sub

        ''' <summary>
        ''' Returns the file name without the extension.
        ''' </summary>
        ''' <returns>The file name without the extension.</returns>
        ''' <remarks></remarks>
        Public Function GetNameWithoutExtension() As String
            Return PackageFileInfo.GetNameWithoutExtension(Me.Name)
        End Function

        ''' <summary>
        ''' Saves the specified content string to the file 
        ''' specified by this <c>PackageFileInfo</c> instance.
        ''' </summary>
        ''' <param name="content">Text content to save to file.</param>
        Public Sub Save(ByVal content As String)
            Using zip As New ZipFile(Me.ParentPath)
                zip.UpdateEntry(Me.RelativePath, content)
                zip.Save()
            End Using
            OnFileContentSaved(Nothing)
        End Sub

        Protected Overridable Sub OnFileContentSaved(ByVal e As EventArgs)
            RaiseEvent FileContentSaved(Me, e)
        End Sub

#End Region

#Region "Static Methods"

        ''' <summary>
        ''' Returns the file name from a system file path.
        ''' </summary>
        ''' <param name="path">Full system path to file.</param>
        ''' <param name="delim">Optional delimiter used in file path. Default is '\'.</param>
        ''' <returns>The file name from a system file path.</returns>
        Public Shared Function GetNameFromPath(ByVal path As String, _
                                               Optional ByVal delim As String = "\") As String
            If Not (String.IsNullOrEmpty(path)) Then
                Dim s() As String = Split(path, delim)
                If UBound(s) > -1 Then
                    Return s(s.Count - 1)
                End If
            End If
            Return String.Empty
        End Function

        ''' <summary>
        ''' Returns only the directory path from a system file path.
        ''' </summary>
        ''' <param name="path">Full system path to file.</param>
        ''' <param name="delim">Optional delimiter used in file path. Default is '\'.</param>
        ''' <returns>The directory path from a system file path.</returns>
        Public Shared Function GetDirectoryPath(ByVal path As String, _
                                                Optional ByVal delim As String = "\") As String
            If Not (String.IsNullOrEmpty(path)) Then
                Dim filePos As Integer = InStrRev(path, delim)
                If filePos > 0 Then
                    Return Left(path, filePos - 1)
                End If
            End If
            Return String.Empty
        End Function

        ''' <summary>
        ''' Gets the file name of the specified path string without the extension.
        ''' </summary>
        ''' <param name="path">The path of the file.</param>
        ''' <returns>The file name of the specified path string without the extension.</returns>
        ''' <remarks></remarks>
        Public Shared Function GetNameWithoutExtension(ByVal path As String) As String
            Dim name As String = GetNameFromPath(path)
            Dim sp() As String = name.Split(EXT_DOT.ToCharArray())
            Return sp(0)
        End Function

#End Region

    End Class

End Namespace