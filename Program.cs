using Mono.Options;

namespace emofunge
{
    class Program
    {
        static void Usage(OptionSet p)
        {
            Console.WriteLine("Emofunge 0.0");
            Console.WriteLine("Copyright 2021 J. P. Savard");
            Console.WriteLine("Usage: emofunge [options] file");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }
        static int Main(string[] args)
        {
            Cpu cpu;
            string[] prog;
            CommandSets set = CommandSets.Emofunge;
            bool showUsage = false;
            int verbosity = 0;

            OptionSet options = new OptionSet {
                { "b|befunge", "Run Befunge-93 program", b => { if(b != null) set = CommandSets.Befunge93; }},
                { "v", "Show debug messages", v => { if(v != null) ++verbosity; } },
                { "h|help", "Send help", h => { showUsage = h != null; } },
            };

            List<string> extra;
            try
            {
                extra = options.Parse(args);
            }
            catch (OptionException e)
            {
                Console.WriteLine(e.Message);
                return 1;
            }

            if(showUsage)
            {
                Usage(options);
                return 0;
            }

            if(extra.Count == 0)
            {
                Console.WriteLine("error: no file loaded");
                return 129;
            }

            try
            {
                string filename = extra[0];
                prog = File.ReadAllLines(filename);
            }
            catch(Exception ex)
            {
                Console.WriteLine("error: can't read file: " + ex.Message);
                return 1;
            }

            cpu = new Cpu(prog, verbosity>0);
            cpu.Commands.Set = set;
            cpu.Run();

            return 0;
        }
    }
}
