

Public Class Ack

    Public Sub New()
        Me.New(String.Empty, String.Empty, 0)
    End Sub

    Public Sub New(ByVal senderAddress As String, ByVal ip As String, ByVal port As Integer)
        Me.SenderAddress = senderAddress
        Me.IP = ip
        Me.Port = port
    End Sub

    Public Property SenderAddress As String
    Public Property IP As String
    Public Property Port As Integer

End Class