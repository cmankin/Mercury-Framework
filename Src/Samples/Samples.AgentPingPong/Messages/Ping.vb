
''' <summary>
''' Represents a ping message.
''' </summary>
''' <remarks></remarks>
Public Class Ping

    Public Sub New(ByVal senderId As String, ByVal counter As Integer)
        Me.SenderId = senderId
        Me.Counter = counter
    End Sub

    Public Property SenderId As String
    Public Property Counter As Integer

End Class