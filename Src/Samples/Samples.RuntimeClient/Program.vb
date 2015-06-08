Imports System.Net
Imports System.Threading
Imports System.Net.Sockets
Imports Samples.Common.Data
Imports Mercury.Messaging.Core
Imports Mercury.Messaging.Runtime
Imports Mercury.Messaging.ServiceModel
Imports Mercury.Messaging.Instrumentation

Module Program

    Public RuntimeListenerPort As Integer = 11090

    Sub Main()
        ' Setup instrumentation
        MessagingCoreInstrumentation.LogSource = New LogSource(TraceOutputSource.File, "C:\Users\chris.mankin\Desktop\Client-output.log", Nothing)
        MessagingCoreInstrumentation.Mode = InstrumentationMode.Debug
        MessagingCoreInstrumentation.SnapshotResources = False
        MessagingCoreInstrumentation.SwitchLevel = SourceLevels.All

        ' Create environment
        Console.WriteLine()
        Console.WriteLine("Load environment...")
        Dim env As New RuntimeEnvironment("runtime")
        Console.WriteLine("done.")

        ' Start server
        Dim canContinue = True
        Console.WriteLine("Specify listener port or <ENTER> to use default (11090).")
        Dim rlp As String = Console.ReadLine()
        If (IsNumeric(rlp)) Then _
            RuntimeListenerPort = CInt(rlp)

        Console.WriteLine("Loading client service...")
        Dim server As MessagingService = GetServer(env)
        server.ListenerExceptionHandler = AddressOf _HandleListenerErrors

        Console.WriteLine()
        Console.WriteLine("Starting client service...")
        Console.WriteLine("Listening...")
        Try
            server.Start()

            ' Create worker
            Dim workerId As String = env.Spawn(GetType(ClientWorker))
            Dim worker As LocalRef = env.GetRef(workerId)

            ' Get server workers
            worker.Send(New GetWorkers(ServerIP, String.Empty, GetType(ServerWorker)))

            Dim args As String = String.Empty
            Do While (canContinue)
                Console.WriteLine("Type message to send to server. {id|index, message}")
                args = Console.ReadLine()
                If (args.ToLower() = "exit") Then _
                    Exit Do

                Dim splitArgs As String() = Split(args, ",")
                If (splitArgs.Length > 0) Then
                    If (splitArgs.Length = 1) Then
                        worker.Send(New Ping("0", splitArgs(0), ServerIP.Address.ToString(), ServerIP.Port))
                    Else
                        worker.Send(New Ping(splitArgs(0), splitArgs(1), ServerIP.Address.ToString(), ServerIP.Port))
                    End If
                End If
            Loop
        Catch ex As Exception
            Console.WriteLine("Exception occurred: {0}", ex.Message)
            canContinue = False
        End Try

        ' Shutdown
        MessagingCoreInstrumentation.Flush()
        server.Environment.Shutdown()
        Console.WriteLine("Stopping client service...")
        Console.Write("Shutdown.")
    End Sub

    Private Sub _HandleListenerErrors(ByVal ex As Exception)
        If ex IsNot Nothing Then
            Console.WriteLine(ex.Message)
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

        ' Listen on port listener port (default is 11090)
        Dim port As Integer = RuntimeListenerPort

        Return New MessagingService(New IPEndPoint(ip, port), env)
    End Function

    Private _serverIp As IPEndPoint

    Public ReadOnly Property ServerIP As IPEndPoint
        Get
            If (_serverIp Is Nothing) Then
                Dim ip As IPAddress = Nothing

                ' Get address
                Dim entry As IPHostEntry = Dns.GetHostEntry(Dns.GetHostName())
                For Each addr As IPAddress In entry.AddressList
                    If (addr.AddressFamily = AddressFamily.InterNetwork) Then
                        ip = addr
                    End If
                Next

                ' Port 11050
                Dim port As Integer = 11050

                _serverIp = New IPEndPoint(ip, port)
            End If
            Return _serverIp
        End Get
    End Property
End Module
