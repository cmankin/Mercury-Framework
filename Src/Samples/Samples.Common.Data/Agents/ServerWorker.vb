Imports System.Net
Imports Mercury.Messaging.Core
Imports Mercury.Messaging.Runtime
Imports Mercury.Messaging.Messages

''' <summary>
''' Generic worker agent
''' </summary>
''' <remarks></remarks>
Public Class ServerWorker
    Implements Agent

    Public Sub New(ByVal port As AgentPort, ByVal hostProcessor As Action(Of String), ByVal currentState As ServerState)
        Me.Environment = port.Environment
        Me.HostProcessor = hostProcessor
        Me.State = currentState

        port.Receive(Of Ping)(
            Sub(msg)
                Try
                    Console.WriteLine(String.Format("Received data from {0} at {1}:{2}", msg.SenderAddress, msg.IP, msg.Port))
                    Console.WriteLine(String.Format("Data={0}", msg.Data))

                    ' If routing to host processor
                    If (msg.Data.Contains("-host")) Then
                        If Me.HostProcessor IsNot Nothing Then
                            Me.HostProcessor(Replace(msg.Data, "-host", ""))
                        End If
                    Else
                        ' Get channel for ack and send
                        Dim channel As LocalRef = Environment.GetRef(msg.SenderAddress, msg.IP, msg.Port)
                        If (channel IsNot Nothing) Then
                            Console.WriteLine("Sending ack...")
                            channel.Send(Of Ack)(New Ack(port.Id, Environment.EndPoint.Address.ToString(), Environment.EndPoint.Port))
                        End If
                    End If

                    Console.WriteLine()
                Catch
                End Try
            End Sub)

        port.Receive(Of IList(Of Ping))(
            Sub(msg)
                If msg IsNot Nothing AndAlso msg.Count > 0 Then
                    For Each png As Ping In msg
                        Console.WriteLine(String.Format("Received data from {0} at {1}:{2}", png.SenderAddress, png.IP, png.Port))
                        Console.WriteLine(String.Format("Data={0}", png.Data))
                        Console.WriteLine()
                    Next

                    Dim first = msg(0)
                    ' If routing to host processor
                    If (first.Data.Contains("-host")) Then
                        If Me.HostProcessor IsNot Nothing Then
                            Me.HostProcessor(Replace(first.Data, "-host", ""))
                        End If
                    Else
                        ' Get channel for ack and send
                        Dim channel As LocalRef = Environment.GetRef(first.SenderAddress, first.IP, first.Port)
                        If (channel IsNot Nothing) Then
                            Console.WriteLine("Sending ack...")
                            channel.Send(Of Ack)(New Ack(port.Id, Environment.EndPoint.Address.ToString(), Environment.EndPoint.Port))
                        End If
                    End If
                End If
            End Sub)

        port.Receive(Of GetWorkers)(
            Sub(msg)
                Try
                    Dim workers As String() = GetWorkerReferences()

                    ' Get channel and send
                    Dim channel As LocalRef = Environment.GetRef(msg.Destination, msg.IP, msg.Port)
                    channel.Send(New SendWorkers(workers))
                Catch
                End Try
            End Sub)

        port.Receive(Of Request(Of GetWorkers))(
            Sub(msg)
                Try
                    If Me.State.HandleQuery() Then
                        Dim workers As String() = GetWorkerReferences()

                        ' Send response
                        msg.ResponseChannel.Send(Of String())(workers)
                    End If
                Catch
                End Try
            End Sub)
    End Sub

    Protected State As ServerState
    Protected Environment As RuntimeEnvironment
    Protected HostProcessor As Action(Of String)

    Protected Function GetWorkerReferences() As String()
        Dim workerRefs As LocalRef() = Me.Environment.GetAgentRefsByType(GetType(ServerWorker)).ToArray()
        If workerRefs IsNot Nothing AndAlso workerRefs.Length > 0 Then
            Dim workers As String() = New String(workerRefs.Length - 1) {}
            For i As Integer = 0 To workers.Length - 1
                workers(i) = workerRefs(i).ResId
            Next
            Return workers
        End If
        Return Nothing
    End Function
End Class