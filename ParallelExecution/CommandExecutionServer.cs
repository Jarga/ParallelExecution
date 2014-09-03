using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace ParallelExecution
{
    public class CommandExecutionServer
    {
        private IList<string> _clients;
        private bool _runOnServer = false;
        private int _port;
        private int _serverTimeout;
        private string _fileName;
        private string _arguments;
        private IList<string> _clientRemainingToConnect = new List<string>();
        private IList<TcpClient> _connectedClients = new List<TcpClient>();

        public CommandExecutionServer(string fileName, string arguments, IList<string> clients)
        {
            _fileName = fileName;
            _arguments = arguments;
            _clients = clients;
            _clientRemainingToConnect = _clients;
        }

        public CommandExecutionServer(int port, string fileName, string arguments, IList<string> clients)
        {
            _fileName = fileName;
            _arguments = arguments;
            _clients = clients;
            _clientRemainingToConnect = _clients;
            _port = port;
        }

        public CommandExecutionServer(int port, int serverTimeout, string fileName, string arguments, IList<string> clients)
        {
            _fileName = fileName;
            _arguments = arguments;
            _clients = clients;
            _clientRemainingToConnect = _clients;
            _port = port;
            _serverTimeout = serverTimeout;
        }

        public CommandExecutionServer(int port, int serverTimeout, string fileName, string arguments, IList<string> clients, bool runOnServer)
        {
            _fileName = fileName;
            _arguments = arguments;
            _clients = clients;
            _clientRemainingToConnect = _clients;
            _port = port;
            _serverTimeout = serverTimeout;
            _runOnServer = runOnServer;
        }

        public bool Start()
        {
            var serverListener = new TcpListener(IPAddress.Any, _port);

            var watch = new Stopwatch();

            serverListener.Start();
            watch.Start();

            while (watch.ElapsedMilliseconds < _serverTimeout)
            {
                if (serverListener.Pending())
                {
                    var connection = serverListener.AcceptTcpClient();
                    var client = (connection.Client.RemoteEndPoint as IPEndPoint).Address.ToString();
                    if (_clientRemainingToConnect.Contains(client))
                    {
                        _clientRemainingToConnect.Remove(client);
                        _connectedClients.Add(connection);
                    }
                    else
                    {
                        Console.WriteLine("Recieved unexpected connection request from {0}, closing connection.", client);
                        connection.Close();
                    }
                }

                if (_clientRemainingToConnect.Count == 0)
                {
                    watch.Stop();
                    Console.WriteLine("Server waited {0} ms for clients to connect!", watch.ElapsedMilliseconds);
                    return StartExecution();
                }

            }

            watch.Stop();
            Console.WriteLine("Server waited {0} ms for clients to connect!", watch.ElapsedMilliseconds);

            return false;
        }

        private bool StartExecution()
        {
            Console.WriteLine("All client are now connected, sending command to each client!");

            var tasks = new List<Task<bool>>();

            foreach (var connection in _connectedClients)
            {
                tasks.Add(new Task<bool>(() =>
                        {
                            Console.WriteLine("Triggering command: {0} {1} at {2} for Machine {3}", _fileName, _arguments, DateTime.Now.ToString("O"), (connection.Client.RemoteEndPoint as IPEndPoint).Address.ToString());

                            var stream = connection.GetStream();

                            using (var writer = new StreamWriter(stream))
                            using (var reader = new StreamReader(stream))
                            {
                                writer.WriteLine(string.Format("{0}{1}{2}", _fileName, ParallelExecutionMaster.CommandDelimiter, _arguments));

                                writer.Flush();

                                var returnVal = reader.ReadLine();

                                return "Success".Equals(returnVal);
                            }

                        }
                ));
            }

            tasks.ForEach(task => task.Start());

            if (_runOnServer)
            {
                Console.WriteLine("Executing command: {0} {1} at {2}", _fileName, _arguments, DateTime.Now.ToString("O"));

                if (string.IsNullOrWhiteSpace(_arguments))
                {
                    var process = Process.Start(_fileName);
                }
                else
                {
                    var process = Process.Start(_fileName, _arguments);
                }

            }

            tasks.ForEach(task => task.Wait());

            var result = tasks.All(task => task.Result);

            Console.WriteLine(result ? "Processes started!" : "At least one of the clients failed to start the process!");

            return result;
        }
    }
}
