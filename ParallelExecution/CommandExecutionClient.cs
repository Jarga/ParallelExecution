using System;
using System.Net.Sockets;
using System.IO;
using System.Diagnostics;

namespace ParallelExecution
{
    public class CommandExecutionClient
    {
        private int _port;
        private string _server;

        public CommandExecutionClient(int port, string server)
        {
            this._port = port;
            this._server = server;
        }

        public Process Start()
        {
            using (var client = new TcpClient(_server, _port))
            {
                var stream = client.GetStream();

                using (var writer = new StreamWriter(stream))
                using (var reader = new StreamReader(stream))
                {
                    var command = reader.ReadLine();

                    if (command == null)
                    {
                        Console.WriteLine("Failed to read TCP Stream!!");

                        return null;
                    }

                    var index = command.IndexOf(ParallelExecutionMaster.CommandDelimiter);
                    if (index > -1)
                    {
                        var fileName = command.Substring(0, index);
                        var arguments = command.Substring(index);

                        Console.WriteLine("Executing command: {0} {1} at {2}", fileName, arguments, DateTime.Now.ToString("O"));

                        Process process = null;

                        if (string.IsNullOrWhiteSpace(arguments))
                        {
                            process = Process.Start(fileName);
                        }
                        else
                        {
                            process = Process.Start(fileName, arguments);
                        }

                        writer.WriteLine("Success");
                        writer.Flush();

                        return process;
                    }

                    Console.WriteLine("Invalid Format Command Format Sent to Client!");

                    writer.WriteLine("Failed");
                }
            }
            return null;
        }
    }
}
