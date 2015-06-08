
''' <summary>
''' Thrown when an invalid file path is encountered.
''' </summary>
''' <remarks></remarks>
Public Class InvalidFilePathException
    Inherits Exception

    Public Sub New()
        Me.New(My.Resources.Invalid_File_Path)
    End Sub

    Public Sub New(ByVal message As String)
        Me.New(message, Nothing)
    End Sub

    Public Sub New(ByVal message As String, ByVal innerException As Exception)
        MyBase.New(message, innerException)
    End Sub

End Class