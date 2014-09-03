using CommandLine;
using CommandLine.Text;
using System.Collections.Generic;
using System.Text;

namespace ParallelExecution
{
    public class Options
    {
        [Option(shortName: 's', longName: "server", HelpText = "Server to use as the controller of the processes when running as a client.")]
        public string Server { get; set; }

        [Option(shortName: 'p', longName: "port", HelpText = "Port to connect to when running as a client or port to activly listen to connections on if running as the server.", DefaultValue = 8888)]
        public int Port { get; set; }

        [Option(shortName: 't', longName: "timeout", HelpText = "Timeout on for the server to stop accepting connections (in milliseconds).", DefaultValue = 180000)]
        public int Timeout { get; set; }

        [Option(shortName: 'u', longName: "useserver", HelpText = "Use this flag to execute the file on the server as well.")]
        public bool UseServer { get; set; }

        [OptionList('r', "receivers", Separator = ',', HelpText = "All the receiving clients that will be used to execute the processes, separated by a comma.")]
        public IList<string> Clients { get; set; }

        [Option(shortName: 'f', longName: "fileName", HelpText = "File to execute.")]
        public string File { get; set; }

        [Option(shortName: 'a', longName: "arguments", HelpText = "Arguments to pass to the file.")]
        public string Arguments { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            var help = new HelpText
            {
                Heading = new HeadingInfo("\r\nParallel Execution Application", "1.0"),
                Copyright = new CopyrightInfo("Sean McAdams", 2014),
                AdditionalNewLineAfterOption = true,
                AddDashesToOption = true,
            };
            help.AddPreOptionsLine("\r\nExample Usage: ParallelExecution.exe -u -f notepad.exe -r \"172.0.0.0\"");
            help.AddPreOptionsLine(    "        Usage: ParallelExecution.exe -s \"172.0.0.0\"");
            help.AddOptions(this); 
            return help;
        }
    }
}
