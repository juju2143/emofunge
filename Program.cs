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
            Console.WriteLine();
            Console.WriteLine("Supported standards:");
            Console.Write("  ");
            Console.WriteLine(String.Join(", ", Enum.GetNames(typeof(CommandSets))));
        }
        static int Main(string[] args)
        {
            Cpu cpu;
            string[] prog;
            CommandSets set = CommandSets.Emofunge;
            bool showUsage = false;
            int verbosity = 0;
            int width = 80;
            int height = 25;
            int stack = 256;

            OptionSet options = new OptionSet {
                { "b|befunge", "Run Befunge-93 program", b => { if(b != null) set = CommandSets.Befunge93; }},
                { "d|std=", "Change standard", (string d) => set = (CommandSets)Enum.Parse(typeof(CommandSets), d) },
                { "h|height=", "Set field height (0 for infinite)", (int h) => height = h },
                { "s|stack=", "Set stack size", (int s) => stack = s },
                { "v", "Show debug messages", v => { if(v != null) ++verbosity; } },
                { "w|width=", "Set field width (0 for infinite)", (int w) => width = w },
                { "?|help", "Send help", h => { showUsage = h != null; } },
            };

            List<string> extra;
            try
            {
                extra = options.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Error.WriteLine(e.Message);
                return 1;
            }
            catch (ArgumentException e)
            {
                Console.Error.WriteLine(e.Message);
                return 1;
            }

            if(showUsage)
            {
                Usage(options);
                return 0;
            }

            if(extra.Count == 0)
            {
                Console.Error.WriteLine("error: no file loaded");
                return 129;
            }

            try
            {
                string filename = extra[0];
                prog = File.ReadAllLines(filename);
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine("error: can't read file: " + ex.Message);
                return 1;
            }

            cpu = new Cpu(prog, verbosity>0, stack);
            cpu.Commands.Set = set;
            cpu.Width = width;
            cpu.Height = height;
            cpu.Run();

            return 0;
        }
    }
}
