using CommandLine;
using System;
using System.Linq;

namespace ParallelExecution
{
    public class ParallelExecutionMaster
    {
        /// <summary>
        /// Delimiter to use between the filename and arguments when sending the execution request from the server so the client can parse it easier, needs to be an uncommon series of characters
        /// </summary>
        public const string CommandDelimiter = "||";

        static void Main(string[] args)
        {
            var options = new Options();

            var parser = new Parser();

            
            //If parsing was successful verify either a server is given or the file and receivers are given
            if (parser.ParseArguments(args, options) && (!string.IsNullOrWhiteSpace(options.Server) || (!string.IsNullOrWhiteSpace(options.File) && options.Clients != null && options.Clients.Any())))
            {
                //If No Server is Given assume Current Box is the Server otherwise process act as if you are a client
                if (string.IsNullOrWhiteSpace(options.Server))
                {
                    var server = new CommandExecutionServer(options.Port, options.Timeout, options.File, options.Arguments, options.Clients, options.UseServer);

                    var result = server.Start();

                    Console.WriteLine(result ? "Execution Successful! Processes Running." : "An Error Occured while attempting to execute the processes!");
                }
                else
                {
                    var client = new CommandExecutionClient(options.Port, options.Server);

                    var process = client.Start();

                    if (process != null)
                    {
                        Console.WriteLine("Process Started!");
                        process.WaitForExit();
                        Console.WriteLine("Process Complete!");
                    }
                    else
                    {
                        Console.WriteLine("Error occured when attempting to execute process!");
                    }

                }
            }
            else
            {
                Console.WriteLine(options.GetUsage());
                return;
            }

            Console.WriteLine("Press Enter To Close The Console!");
            Console.ReadLine();
        }
    }
}
