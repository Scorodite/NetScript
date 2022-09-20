using System;
using System.IO;
using System.Runtime.InteropServices;
using NetScript.Compiler;

namespace NetScript
{
    static class Program
    {
        [DllImport("kernel32.dll", EntryPoint = "AllocConsole", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void AllocConsole();

        const int SW_SHOW = 5;

        static void Main(string[] args)
        {
            if (args.Length == 3 && args[0] == "-c")
            {
                string file = args[1];
                string output = args[2];
                using var fs = File.Create(output);
                NS.Compile(File.ReadAllText(file), fs);
                return;
            }
            else if (args.Length == 1)
            {
                string file = args[0];

                if (file.EndsWith(".nsc"))
                {
                    AllocConsole();
                    using FileStream fs = File.OpenRead(file);
                    NS.Run(fs);
                    return;
                }
                else if (file.EndsWith(".ns"))
                {
                    AllocConsole();
                    NS.Run(File.ReadAllText(file));
                    return;
                }
                else if (file.EndsWith(".nscw"))
                {
                    using FileStream fs = File.OpenRead(file);
                    NS.Run(fs);
                    return;
                }
                else if (file.EndsWith(".nsw"))
                {
                    NS.Run(File.ReadAllText(file));
                    return;
                }
            }
            Console.WriteLine("Usage:\n\tnetscript.exe script.ns\n\tnetscript.exe compiled.nsc\n\tnetscript.exe -c script.ns output.nsc");
        }
    }
}
