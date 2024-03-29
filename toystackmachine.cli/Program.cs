﻿using System;
using System.Diagnostics.Metrics;
using System.IO;

using CommandLine;

using toystackmachine.cli;
using toystackmachine.core;
using toystackmachine.core.ToyAssembly;

namespace ToyAssemblerCLI
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<RunOptions, CompileOptions, TestOptions>(args)
            .WithParsed<RunOptions>(opts => Run(opts))
            .WithParsed<CompileOptions>(opts => Compile(opts))
            .WithParsed<TestOptions>(opts => RunTest(opts));
        }

        static void Run(RunOptions opts)
        {
            // Load the binary file
            using (FileStream fs = new FileStream(opts.BinaryFile, FileMode.Open))
            {
                ToyProgram.Deserialize(fs, out ToyProgram program);

                // Load the program into the ToyStackMachine and run it
                ToyStackMachine vm = new ToyStackMachine(new ToyStackMachineMemoryConfiguration());
                vm.RegisterHostFuntion("hostadd", (m, a) => a.Sum());
                vm.RegisterHostFuntion("hostinput", (m, a) => { Console.Write("> "); return int.TryParse(Console.ReadLine(), out int res) ? res : 0; });
                vm.RegisterHostFuntion("hostprint", (m, a) =>
                {
                    var s = new string(vm.GetArrayAt(a[0]).Select(i => (char)i).ToArray());
                    Console.Write(s);
                    return 0;
                });
                vm.LoadProgram(program);
                vm.Run();
            }
        }

        static void Compile(CompileOptions opts)
        {
            // Read the text file
            string instructions = File.ReadAllText(opts.InstructionFile);

            // Instantiate ToyAssembler and assemble the instructions
            ToyAssembler assembler = new ToyAssembler(new ToyAssemblyLexer(instructions), new ToyStackMachineMemoryConfiguration());
            ToyProgram program = assembler.Assemble();

            // Serialize the ToyProgram to a binary file
            using (Stream fs = new FileStream(opts.OutputFile, FileMode.Create))
            {
                ToyProgram.Serialize(program, fs);
            }
        }

        static void RunTest(TestOptions opts)
        {
            Test.RunTest();
        }
    }

    [Verb("run", HelpText = "Run a ToyAssembler binary.")]
    public class RunOptions
    {
        [Value(0, MetaName = "binary", HelpText = "Binary file to run.", Required = true)]
        public string BinaryFile { get; set; }
    }

    [Verb("compile", HelpText = "Compile a ToyAssembler instruction file.")]
    public class CompileOptions
    {
        [Value(0, MetaName = "instructions", HelpText = "Instruction file to compile.", Required = true)]
        public string InstructionFile { get; set; }

        [Value(1, MetaName = "output", HelpText = "Output file for the compiled binary.", Required = true)]
        public string OutputFile { get; set; }
    }

    [Verb("test", HelpText = "Run a test.")]
    public class TestOptions
    {
    }
}
