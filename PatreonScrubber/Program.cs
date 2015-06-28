using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatreonScrubber
{
    class Program
    {
        static void Main(string[] args)
        {
            Scubber t_Scrub = new Scubber();

            if (args.Count() > 0)
            {
                t_Scrub.ScrubFile(args[0]);
            }
            
        }
    }
}
