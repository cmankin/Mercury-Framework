
''' <summary>
''' Thrown when attempting to resolve a type that is not registered with the container.
''' </summary>
''' <remarks></remarks>
Public Class TypeNotRegisteredException
    Inherits Exception

    Public Sub New()
        MyBase.New("Type not registered with the container.")
    End Sub

    Public Sub New(ByVal message As String)
        MyBase.New(message)
    End Sub

End Class
