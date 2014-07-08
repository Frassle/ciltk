using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Weave
{
    class Program
    {
        public static string InputAssembly { get; private set; }

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

                var pdb = Path.ChangeExtension(input, "pdb");
                var mdb = Path.ChangeExtension(input, "mdb");
                ISymbolReaderProvider provider = null;
                if (File.Exists(pdb))
                {
                    provider = new Mono.Cecil.Pdb.PdbReaderProvider();
                }
                else if (File.Exists(mdb))
                {
                    provider = new Mono.Cecil.Mdb.MdbReaderProvider();
                }

                var assemblyResolver = new DefaultAssemblyResolver();
                assemblyResolver.AddSearchDirectory(System.IO.Path.GetDirectoryName(input));

                var readParameters = new ReaderParameters()
                {
                    AssemblyResolver = assemblyResolver,
                    ReadingMode = ReadingMode.Immediate,
                    ReadSymbols = true,
                    SymbolReaderProvider = provider,
                };

                var writeParameters = new WriterParameters()
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

                InputAssembly = input;
                var assembly = Mono.Cecil.AssemblyDefinition.ReadAssembly(input, readParameters);

                var labelReplace = new LabelReplacer();
                labelReplace.Visit(assembly);

                var infoReplace = new InfoReplacer();
                infoReplace.Visit(assembly);

                var cilReplacer = new CilReplacer(labelReplace);
                cilReplacer.Visit(assembly);

                //remove references to Silk
                foreach (var module in assembly.Modules)
                {
                    int index = -1;
                    for (int i = 0; i < module.AssemblyReferences.Count; ++i)
                    {
                        if (module.AssemblyReferences[i].Name == "Silk")
                        {
                            index = i;
                            break;
                        }
                    }

                    if (index != -1)
                    {
                        module.AssemblyReferences.RemoveAt(index);
                    }
                }

                //write to a temp file then copy to output to assist with debugging
                var temp = System.IO.Path.GetTempFileName();
                assembly.Write(temp, writeParameters);
                System.IO.File.Copy(temp, output, true);
            }
            catch (Mono.Options.OptionException)
            {
                options.WriteOptionDescriptions(Console.Out);
                System.Environment.Exit(1);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Weave encounted an exception: {0}", ex.GetType());
                Console.WriteLine(ex.ToString());
                System.Environment.Exit(1);
            }
        }
    }
}
