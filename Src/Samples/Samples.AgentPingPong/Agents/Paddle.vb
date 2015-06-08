Imports Mercury.Messaging.Core
Imports Mercury.Messaging.Runtime
Imports Mercury.Messaging.Messages

''' <summary>
''' Represents a paddle used return a ping or pong.
''' </summary>
''' <remarks></remarks>
Public Class Paddle
    Implements Agent

    Public Sub New(ByVal port As AgentPort)
        Dim environment As RuntimeEnvironment = port.Environment

        port.Receive(Of Ping)(
            Sub(msg)
                Dim sender As LocalRef = environment.GetRef(msg.SenderId)
                If (msg.Counter = 0) Then
                    sender.Send(Of [Stop])(New [Stop]())
                    Console.Write(String.Format("Shutdown:{0}", port.Id))
                    port.Shutdown(Nothing)
                Else
                    Dim counter As Integer = msg.Counter - 1
                    Console.WriteLine(String.Format("{0} received Ping {1}. Sent Pong.", port.Id, msg.Counter))
                    sender.Send(Of Pong)(New Pong(port.Id, counter))
                End If
            End Sub)

        port.Receive(Of Pong)(
            Sub(msg)
                Dim sender As LocalRef = environment.GetRef(msg.SenderId)
                'Console.WriteLine(String.Format("{0} received Pong {1}.", port.Id, msg.Counter))
                Console.WriteLine(String.Format("Received Pong. Sent Ping.", port.Id))
                sender.Send(Of Ping)(New Ping(port.Id, msg.Counter))
            End Sub)
    End Sub
End Class