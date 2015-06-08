Imports System.IO
Imports Ionic.Zip

Namespace Document

    ''' <summary>
    ''' A file wrapper that can be used to represent a packaged (ZIP) file.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class FilePackage

        ' Attributes
        Private _pathToPackage As String
        Private _name As String

        ' Contents dictionary
        Private _contents As IDictionary(Of String, PackageFileInfo) = _
            New Dictionary(Of String, PackageFileInfo)

#Region "Constructors"

        ''' <summary>
        ''' Initializes a default instance of the <c>FilePackage</c> class.
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
        End Sub

        ''' <summary>
        ''' Initializes a default instance of the <c>FilePackage</c> 
        ''' class with the specified system path to the file.
        ''' </summary>
        ''' <param name="path">Full system path to file.</param>
        ''' <remarks></remarks>
        Public Sub New(ByVal path As String)
            Me.New(PackageFileInfo.GetNameFromPath(path), PackageFileInfo.GetDirectoryPath(path))
        End Sub

        ''' <summary>
        ''' Initializes a default instance of the <c>FilePackage</c> class with 
        ''' the specified package name and path to the package directory.
        ''' </summary>
        ''' <param name="name">The name of the package.</param>
        ''' <param name="directoryPath">The path to the package directory.</param>
        Public Sub New(ByVal name As String, ByVal directoryPath As String)
            Me._name = name
            Me._pathToPackage = directoryPath
        End Sub

#End Region

#Region "Properties"

        ''' <summary>
        ''' Gets the full system file path to the package.
        ''' </summary>
        ''' <returns>The full system file path to the package.</returns>
        Public Overridable ReadOnly Property Path() As String
            Get
                Return String.Format("{0}\{1}", Me._pathToPackage, Me._name)
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the system directory path to the package.
        ''' </summary>
        ''' <value>The system directory path to the package.</value>
        ''' <returns>The system directory path to the package.</returns>
        Public Overridable Property DirectoryPath() As String
            Get
                Return Me._pathToPackage
            End Get
            Set(ByVal value As String)
                Me._pathToPackage = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the name of the package.
        ''' </summary>
        ''' <value>The name of the package.</value>
        ''' <returns>The name of the package.</returns>
        Public Overridable Property Name() As String
            Get
                Return Me._name
            End Get
            Set(ByVal value As String)
                Me._name = value
            End Set
        End Property

        ''' <summary>
        ''' Gets a collection of <c>PackageFileInfo</c> objects 
        ''' for content contained within the <c>FilePackage</c>.
        ''' </summary>
        ''' <returns>A collection of <c>PackageFileInfo</c> objects.</returns>
        Public Overridable ReadOnly Property Contents() As ICollection(Of PackageFileInfo)
            Get
                Return Me._contents.Values
            End Get
        End Property

#End Region

#Region "Methods"

        ''' <summary>
        ''' Adds the relative path resource to the <c>FilePackage</c> if it does not already exist.
        ''' </summary>
        ''' <param name="path">Relative path from the package root to the file resource.</param>
        Public Sub AddContentInfo(ByVal path As String)
            If Not (Me._contents.ContainsKey(path)) Then
                Me._contents.Add(path, New PackageFileInfo(Me, path))
            End If
        End Sub

        ''' <summary>
        ''' Removes the content <c>PackageFileInfo</c> at the specified path.
        ''' </summary>
        ''' <param name="path">Relative path from the package root to the file resource.</param>
        Public Sub RemoveContentInfo(ByVal path As String)
            If Me._contents.ContainsKey(path) Then
                Me._contents.Remove(path)
            End If
        End Sub

        ''' <summary>
        ''' Clears all <c>PackageFileInfo</c> objects 
        ''' associated with the <c>FilePackage</c>.
        ''' </summary>
        Public Sub ClearContentInfo()
            Me._contents.Clear()
        End Sub

        ''' <summary>
        ''' Extracts packaged file content information into 
        ''' a collection of <c>PackageFileInfo</c> objects.
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub ExtractContentInfo()
            ClearContentInfo()
            Using zip As New ZipFile(Me.Path)
                For Each e As ZipEntry In zip.Entries
                    AddContentInfo(e.FileName)
                Next
            End Using
        End Sub

        ''' <summary>
        ''' Returns the <c>String</c> content of the file at the specified file path.
        ''' </summary>
        ''' <param name="path">Relative path from the package root to the file resource.</param>
        ''' <returns>The <c>String</c> content of the file at the specified file path.</returns>
        Public Function ExtractContent(ByVal path As String) As String
            Dim reader As StreamReader = Nothing
            ExtractContent(path, reader)
            If reader IsNot Nothing Then
                Return reader.ReadToEnd
            Else    ' Return an empty string
                Return String.Empty
            End If
        End Function

        ''' <summary>
        ''' Extracts the content from the file at the 
        ''' specified path into a <c>StreamReader</c>.
        ''' </summary>
        ''' <param name="path">Relative path from the package root to the file resource.</param>
        ''' <param name="reader">A <c>StreamReader</c> into 
        ''' which the file contents will be extracted.</param>
        Public Sub ExtractContent(ByVal path As String, ByRef reader As StreamReader)
            If Me._contents.ContainsKey(path) Then
                Me._contents(path).Extract(reader)
            End If
        End Sub

        ''' <summary>
        ''' Determines whether the specified file name exists in the package.
        ''' </summary>
        ''' <param name="name">The file name to check for existence.</param>
        ''' <returns>True if the file exists in the package; otherwise, false.</returns>
        ''' <remarks></remarks>
        Public Function FileExists(ByVal name As String) As Boolean
            Using zip As New ZipFile(Me.Path)
                For Each e As ZipEntry In zip.Entries
                    If e.FileName = name Then
                        Return True
                    End If
                Next
            End Using
            Return False
        End Function

        ''' <summary>
        ''' Returns a collection of <c>PackageFileInfo</c> objects that are children of the specified directory.
        ''' </summary>
        ''' <param name="dirPath">The path to the directory to retrieve entries.</param>
        ''' <returns>A collection of <c>PackageFileInfo</c> objects 
        ''' that are children of the specified directory.</returns>
        ''' <remarks></remarks>
        Public Function GetFileEntries(ByVal dirPath As String) As ICollection(Of PackageFileInfo)
            Dim coll As ICollection(Of PackageFileInfo) = New List(Of PackageFileInfo)
            For Each k As String In Me._contents.Keys
                If IsInDirectoryPath(k, dirPath) Then
                    coll.Add(Me._contents(k))
                End If
            Next
            Return coll
        End Function

        ''' <summary>
        ''' Saves the specified content <c>String</c> to the file resource at the specified path.
        ''' </summary>
        ''' <param name="path">Relative path from the package root to the file resource.</param>
        ''' <param name="content">Text content to save to file.</param>
        Public Sub SaveContent(ByVal path As String, ByVal content As String)
            ' If new content
            If Not (Me._contents.ContainsKey(path)) Then
                Dim info As New PackageFileInfo(Me, path)
                info.Save(content)
                ExtractContentInfo()
            Else    ' Save/update content
                Me._contents(path).Save(content)
            End If
        End Sub

        ''' <summary>
        ''' Removes the file resource at the specified path.  This method 
        ''' also removes the associated <c>PackageFileInfo</c> object.
        ''' </summary>
        ''' <param name="path">Relative path from the package root to the file resource.</param>
        Public Sub RemoveContent(ByVal path As String)
            Using zip As ZipFile = New ZipFile(Me.Path)
                zip.RemoveEntry(path)
                zip.Save()
            End Using
        End Sub

#End Region

        ''' <summary>
        ''' Determines whether a specified file path is contained within the specified directory path.
        ''' </summary>
        ''' <param name="path">The file path to be evaluated.</param>
        ''' <param name="dirPath">The directory path to search for.</param>
        ''' <returns>True if the file path is contained within the specified directory path; otherwise, false.</returns>
        ''' <remarks></remarks>
        Protected Function IsInDirectoryPath(ByVal path As String, ByVal dirPath As String) As Boolean
            Dim origPath() As String = SplitPath(path)
            Dim comDirPath() As String = SplitPath(dirPath)
            If origPath.Length = (comDirPath.Length + 1) Then
                For i As Integer = 0 To UBound(comDirPath)
                    If origPath(i) <> comDirPath(i) Then _
                        Return False
                Next
                Return True
            End If
            Return False
        End Function

        Protected Function SplitPath(ByVal path As String, Optional ByVal delim As String = "/")
            Return path.Split(delim.ToCharArray())
        End Function

    End Class

End Namespace