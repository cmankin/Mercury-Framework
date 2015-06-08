Imports System.Net
Imports System.Threading
Imports System.Net.Sockets
Imports Samples.Common.Data
Imports Mercury.Messaging.Core
Imports Mercury.Messaging.Runtime
Imports Mercury.Messaging.ServiceModel
Imports Mercury.Messaging.Instrumentation

''' <summary>
''' The program entry point
''' </summary>
''' <remarks></remarks>
Module Program

    Sub Main()

        Dim server As MessagingService = Nothing

        ' Number of workers to create
        Console.WriteLine()
        Dim numWorkers As Integer = GetWorkers("How many workers would you like to create? (Min=1; Max=60)")

        ' Set instrumentation
        MessagingCoreInstrumentation.LogSource = New LogSource(TraceOutputSource.File, "C:\Users\chris.mankin\Desktop\Server-output.log", Nothing)
        MessagingCoreInstrumentation.Mode = InstrumentationMode.Debug

        ' Create environment
        Console.WriteLine()
        Console.WriteLine("Load environment...")
        Dim env As New RuntimeEnvironment("environment2")
        Console.WriteLine("done.")


        ' Host processor action
        Dim hostProcessor = New Action(Of String)(
            Sub(msg)
                ProcessServerMessage(msg, server)
            End Sub)

        ' Workers
        Console.WriteLine()
        Console.WriteLine("Create workers...")
        Dim workerList As New List(Of String)()
        For i As Integer = 0 To numWorkers - 1
            workerList.Add(env.Spawn(GetType(ServerWorker), hostProcessor, CurrentState))
        Next
        Console.WriteLine("done.")

        ' Display workers
        Console.WriteLine()
        Console.WriteLine("Show workers...")
        For Each wrk As String In workerList
            Console.WriteLine(wrk)
        Next
        Console.WriteLine("done.")

        Console.WriteLine()
        Console.WriteLine()
        Console.WriteLine()

        ' Start server
        Dim canContinue = True
        Console.WriteLine("Loading server...")
        server = GetServer(env)
        server.ListenerExceptionHandler = AddressOf _HandleListenerErrors

        Console.WriteLine()
        Console.WriteLine("Starting server...")
        Console.WriteLine("Listening...")
        Try
            server.Start()

            Do While (canContinue)
                Thread.Sleep(2000)
                If (CurrentState.WorkersQueryHandled AndAlso CurrentState.AttemptedHandleCount = numWorkers) Then
                    CurrentState.ResetAttemptedHandleCount()
                    CurrentState.SetWorkersQueryHandled(False)
                End If
                If Not (server.IsStarted) Then
                    canContinue = False
                End If
            Loop
        Catch ex As Exception
            Console.WriteLine("Exception occurred: {0}", ex.Message)
            canContinue = False
        End Try

        ' Shutdown
        MessagingCoreInstrumentation.Flush()
        server.Environment.Shutdown()
        Console.WriteLine("Stopping server...")
        Console.Write("Shutdown.")
    End Sub

    Private Sub _HandleListenerErrors(ByVal ex As Exception)
        If ex IsNot Nothing Then
            Console.WriteLine(ex.Message)
        End If
    End Sub

    Private CurrentState As ServerState = New ServerState()

    Private Sub ProcessServerMessage(ByVal msg As String, ByVal server As MessagingService)
        If Not (String.IsNullOrEmpty(msg)) Then
            msg = msg.ToLower()

            ' If kill message
            If (msg.Contains("kill ")) Then
                msg = Trim(msg.Replace("kill ", ""))
                If (msg = "[server]") Then
                    If (server IsNot Nothing) Then
                        Console.WriteLine("Received server kill message.  Stopping server...")
                        server.Stop()
                        Console.WriteLine("Server stopped.")
                    End If
                Else
                    If (server IsNot Nothing AndAlso
                        server.Environment IsNot Nothing) Then
                        Console.WriteLine("Received worker kill message: worker={0}", msg)
                        server.Environment.Kill(msg)
                    End If
                End If
            End If
        End If
    End Sub

    Public Function GetServer(ByVal env As RuntimeEnvironment) As MessagingService
        Dim ip As IPAddress = Nothing

        ' Get address
        Dim entry As IPHostEntry = Dns.GetHostEntry(Dns.GetHostName())
        For Each addr As IPAddress In entry.AddressList
            If (addr.AddressFamily = AddressFamily.InterNetwork) Then
                ip = addr
            End If
        Next

        ' Listen on port 11050
        Dim port As Integer = 11050

        Return New MessagingService(New IPEndPoint(ip, port), env)
    End Function

    Public Function GetWorkers(ByVal workerMessage As String) As Integer
        Dim numWorkers As Integer = 0

        Console.WriteLine(workerMessage)
        Dim workerArgs As String = Console.ReadLine()
        Dim hasWorkers As Boolean = False
        Do While (Not (hasWorkers))
            If IsNumeric(workerArgs) Then
                numWorkers = CInt(workerArgs)
                If (numWorkers > 0 AndAlso numWorkers < 61) Then
                    hasWorkers = True
                    Exit Do
                End If
            End If
            Console.WriteLine(workerMessage)
            workerArgs = Console.ReadLine()
        Loop

        Return numWorkers
    End Function


End Module