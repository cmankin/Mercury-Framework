using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using Mercury.Messaging.Core;
using Mercury.Messaging.Runtime;
using Mercury.Messaging.Messages;
using Samples.Common.Data;

namespace Samples.AgentCounting
{
    class Program
    {
        static void Main(string[] args)
        {
            // Get runtime
            RuntimeEnvironment env = new RuntimeEnvironment("env");
            Console.WriteLine("Initialize runtime environment.");

            // Switch on verbose updates
            bool verbose = false;
            Console.WriteLine("Display verbose output during test? (y|n)");
            string arg = Console.ReadLine();
            if (arg.ToLower() == "y" || arg.ToLower() == "yes")
                verbose = true;

            List<Fault> faults = new List<Fault>();
            // Get fault agent
            LocalRef faultAgent = AnonymousAgent.New(env, (port) =>
            {
                port.Receive<Fault>((fault) =>
                {
                    faults.Add(fault);
                });
            });

            // Specify the number of iterations & agents to use
            int count = 2000000;
            int numAgents = 10000;
            List<LocalRef> agents = new List<LocalRef>();

            // Specify number of times to repeat test
            int repeat = 0;
            Console.WriteLine("Specify the number of times to repeat the test.  Default is 0.");
            arg = Console.ReadLine();
            if (!string.IsNullOrEmpty(arg))
                int.TryParse(arg, out repeat);

            List<TestResult> overallResults = new List<TestResult>();
            for (int rIndex = 0; rIndex < repeat + 1; rIndex++)
            {
                // Start performance timer
                Console.WriteLine();
                Console.WriteLine("Begin testing...");
                Stopwatch timer = new Stopwatch();
                timer.Start();

                // Iterate
                for (int i = 0; i < numAgents; i++)
                {
                    LocalRef current = env.SpawnLink<Counter>(faultAgent.ResId);
                    agents.Add(current);
                    current.Send<Tuple<string, int>>(new Tuple<string, int>("count", count));

                    // Verbose display
                    if (verbose)
                        Console.WriteLine("Begin counting on agent [{0}]...", agents[i].ResId);
                }

                while (env.ResourceCount > 1)
                {
                    Thread.Sleep(500);
                }

                // Timer stop - test completed
                timer.Stop();
                TimeFormat tf = new TimeFormat(timer.Elapsed);
                Console.WriteLine();
                Console.WriteLine("Completed sequential counting from 0 to {0} across {1} agents in approximately {2}.", count, numAgents, tf.ToString());

                // Faults
                Console.WriteLine();
                Console.WriteLine("{0} faults found.", faults.Count);
                Console.WriteLine();
                foreach (Fault f in faults)
                    Console.WriteLine(string.Format("{0}{1}", f.Exception.Message, Environment.NewLine));

                // Add overall results
                overallResults.Add(new TestResult(faults, tf, numAgents));
                // Clear agents list
                agents.Clear();
            }

            // Overall results
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("******************************************************");
            Console.WriteLine("RESULTS:");
            Console.WriteLine();

            int totalFaults = 0;
            TimeSpan totalTime = new TimeSpan(0);
            for (int j = 0; j < overallResults.Count; j++)
            {
                TestResult result = overallResults[j];
                Console.WriteLine("Iteration {0}: Test executed across {1} agents in approximately {2} with {3} faults.",
                    j, result.AgentCount, result.Elapsed.ToString(), result.Faults.Count);

                // Get total faults
                totalFaults += result.Faults.Count;
                totalTime = totalTime.Add(result.Elapsed.Time);
            }
            
            // Get final totals
            long tickCount = totalTime.Ticks;
            long avgTickCount = (long)(tickCount / overallResults.Count);
            TimeFormat avgTime = new TimeFormat(new TimeSpan(avgTickCount));
            Console.WriteLine();
            Console.WriteLine("******************************************************");
            Console.WriteLine("FINAL RESULTS:");
            Console.WriteLine("{0} iterations counting from 0 to {1} across {2} agents with an average execution time of {3} and a total of {4} faults.",
                overallResults.Count, count, numAgents, avgTime.ToString(), totalFaults);
            Console.WriteLine();
            Console.WriteLine("END AGENT TESTING");
            Console.WriteLine("******************************************************");

            // Test synchronously?
            bool testSynchronous = false;
            Console.WriteLine();
            Console.WriteLine("Run synchronous test for comparison? (y|n)");
            arg = Console.ReadLine();
            if (arg.ToLower() == "y" || arg.ToLower() == "yes")
                testSynchronous = true;

            if (testSynchronous)
            {
                // Start synchronous testing
                Console.WriteLine();
                Console.WriteLine("Begin testing...");
                Stopwatch timer = new Stopwatch();
                timer.Start();

                // Synchronous loop
                for (int i = 0; i < numAgents; i++)
                {
                    for (int j = 0; j < count; j++)
                    {
                        // Do nothing
                    }
                }

                // Timer stop - test completed
                timer.Stop();
                TimeFormat tf = new TimeFormat(timer.Elapsed);
                Console.WriteLine();
                Console.WriteLine("Completed synchronous counting from 0 to {0} executed sequentially {1} times in approximately {2}.", count, numAgents, tf.ToString());
            }
        }
    }
}
