using CommandLine;
using CommandLine.Text;

namespace Queuing_Simulation
{
    public class Options
    {
        [Option('i', "input", Required = true, HelpText = "Directory to use to read input.")]
        public string InputDirectory { get; set; }

        [Option('o', "output", Required = false, HelpText = "Directory to use to create output.")]
        public string OutputDirectory { get; set; }

        [Option('r', "runs", Required = false, DefaultValue = 1, HelpText = "Number of runs.")]
        public int Runs { get; set; }

        [Option('s', "step size", Required = false, DefaultValue = 0.05, HelpText = "Utilization step size.")]
        public double Stepsize { get; set; }

        [Option('d', "departures to simulate", Required = false, DefaultValue = 1E6, HelpText = "Number of departures to simulate.")]
        public double DeparturesToSimulate { get; set; }

        [Option('t', "threads", Required = false, DefaultValue = 1, HelpText = "Maximum number of threads to use.")]
        public int Threads { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}
