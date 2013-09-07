using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Weave
{
    class Program
    {
        static void Main(string[] args)
        {
            string input = null;
            string output = null;
            string keyPairContainer = null;

            var options = new Mono.Options.OptionSet()
            {
                { "i|input=", i => input = i },
                { "o|output:", o => output = o },
                { "k|key:", k => keyPairContainer = k },
            };

            try
            {
                var extra = options.Parse(args);

                var readParameters = new Mono.Cecil.ReaderParameters()
                {
                    ReadingMode = Mono.Cecil.ReadingMode.Immediate,
                    ReadSymbols = true,
                };

                var writeParameters = new Mono.Cecil.WriterParameters()
                {
                    WriteSymbols = true,
                };

                if (keyPairContainer != null)
                {
                    writeParameters.StrongNameKeyPair = new System.Reflection.StrongNameKeyPair(keyPairContainer);
                }
                if (output == null)
                {
                    output = input;
                }

                Console.WriteLine("CilTK Rewriter");
                Console.WriteLine("Reading assembly from {0}", input);
                Console.WriteLine("Writing assembly to {0}", output);

                var assembly = Mono.Cecil.AssemblyDefinition.ReadAssembly(input, readParameters);

                var labelReplace = new LabelReplacer();
                labelReplace.Visit(assembly);

                var cilReplacer = new CilReplacer(labelReplace);
                cilReplacer.Visit(assembly);

                assembly.Write(output, writeParameters);
            }
            catch (Mono.Options.OptionException ex)
            {
                options.WriteOptionDescriptions(Console.Out);
            }
        }
    }
}
