﻿using BidirectionalMap;

using System.Collections.Generic;
using System.IO;
using System.Linq;

using toystackmachine.core.ToyAssembly;

namespace toystackmachine.core
{
    public class ToyProgram
    {
        public int[] ROM;
        public string[] Dependency;
        public BiMap<string, int> Labels;
        public BiMap<string, int> Constants;

        public ToyProgram(int[] rom, string[] depedency, Dictionary<string, int> labels, Dictionary<string, int> constants)
        {
            this.ROM = rom;
            this.Dependency = depedency;
            this.Labels = new BiMap<string, int>(labels);
            this.Constants = new BiMap<string, int>(constants);
        }

        public static void Serialize(ToyProgram program, Stream stream)
        {
            var ROM = program.ROM;
            var Dependency = program.Dependency;
            var Labels = program.Labels;
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write("TOYASM");

            // Write the ROM
            writer.Write(ROM.Length);
            foreach (int instruction in ROM)
            {
                writer.Write(instruction);
            }

            // Write the dependency
            writer.Write(Dependency.Length);
            foreach (string dependency in Dependency)
            {
                writer.Write(dependency);
            }

            // Write the labels
            writer.Write(Labels.Forward.ToList().Count);
            foreach (KeyValuePair<string, int> label in Labels.Forward)
            {
                writer.Write(label.Key);
                writer.Write(label.Value);
            }

            // Write the constants
            writer.Write(program.Constants.Forward.ToList().Count);
            foreach (KeyValuePair<string, int> constant in program.Constants.Forward)
            {
                writer.Write(constant.Key);
                writer.Write(constant.Value);
            }
        }

        public static void Deserialize(Stream stream, out ToyProgram program)
        {
            BinaryReader reader = new BinaryReader(stream);
            string magic = reader.ReadString();
            if (magic != "TOYASM")
            {
                throw new InvalidDataException("Invalid magic number");
            }

            // Read the ROM
            int romLength = reader.ReadInt32();
            int[] rom = new int[romLength];
            for (int i = 0; i < romLength; i++)
            {
                rom[i] = reader.ReadInt32();
            }

            // Read the dependency
            int dependencyLength = reader.ReadInt32();
            string[] dependency = new string[dependencyLength];
            for (int i = 0; i < dependencyLength; i++)
            {
                dependency[i] = reader.ReadString();
            }

            // Read the labels
            int labelLength = reader.ReadInt32();
            Dictionary<string, int> labels = new Dictionary<string, int>();
            for (int i = 0; i < labelLength; i++)
            {
                string key = reader.ReadString();
                int value = reader.ReadInt32();
                labels.Add(key, value);
            }

            // Read the constants
            int constantLength = reader.ReadInt32();
            Dictionary<string, int> constants = new Dictionary<string, int>();
            for (int i = 0; i < constantLength; i++)
            {
                string key = reader.ReadString();
                int value = reader.ReadInt32();
                constants.Add(key, value);
            }

            program = new ToyProgram(rom, dependency, labels, constants);
        }

        public override string ToString()
        {
            return ToyAssemblyDisassembler.Diassemble(this);
        }
    }
}