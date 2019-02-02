using CommandLine;

namespace TripAdvisorScapage
{
    public class Options
    {
        [Option('p', "page", Required = true, HelpText = "Page to start scaping reviews from")]
        public string StartPage { get; set; }

        [Option('m', "maxPages", Required = false, HelpText = "Maximum pages to scape for a smaller data set.")]
        public int MaxPages { get; set; }

        [Option('s', "saveTo", Required = true, HelpText = "File path to save output")]
        public string SavePath { get; set; }
    }
}
