using System.Text;

namespace emofunge
{
    class Program
    {
        static void Usage()
        {
            Console.WriteLine("Usage: emofunge [file]");
        }
        static int Main(string[] args)
        {
            Cpu cpu;
            string[] prog;

            if(args.Length == 0)
            {
                Usage();
                return 129;
            }

            try
            {
                string filename = args[0];
                prog = File.ReadAllLines(filename);
            }
            catch(Exception ex)
            {
                Console.WriteLine("error: can't read file: " + ex.Message);
                return 1;
            }

            cpu = new Cpu(prog);
            cpu.Run();

            return 0;
        }
    }
}
