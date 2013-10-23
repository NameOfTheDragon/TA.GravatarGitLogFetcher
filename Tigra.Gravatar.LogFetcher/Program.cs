using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tigra.Gravatar.LogFetcher
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
             * 1. Open the log file.
             * 2. Read each line in the log and extract the committer name and email address
             * 3. Build a set of unique committers.
             * 4. Download the Gravatar icon for each committer. [Parallel/Async]
             * 5. Save the image with the correct file name
             */
            var options = new Options();
            var result = CommandLine.Parser.Default.ParseArguments(args, options);
            var log = new GitLog(options.Repository);
            var committers = log.GetListOfUniqueCommitters();
            var fetcher = new GravatarFetcher(committers);
            fetcher.FetchGravatars(options.OutputDirectory);
        }
    }
}
