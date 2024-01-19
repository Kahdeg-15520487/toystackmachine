using System;
using System.Collections.Generic;

namespace toystackmachine.core.ToyAssembly
{
    public class ToyEmitter
    {
        private readonly List<int> program;
        private readonly Dictionary<string, int> labels;
        private readonly Dictionary<string, List<int>> unpatchedLabels;
        private readonly List<string> dependency;
        private readonly Dictionary<string, int> constants;
        private readonly ToyStackMachineMemoryConfiguration memoryConfiguration;
        private int currentConstantPointer = 0;

        public ToyEmitter(ToyStackMachineMemoryConfiguration memoryConfiguration)
        {
            this.program = new List<int>();
            this.dependency = new List<string>();
            this.labels = new Dictionary<string, int>();
            this.unpatchedLabels = new Dictionary<string, List<int>>();
            this.constants = new Dictionary<string, int>();
            this.memoryConfiguration = memoryConfiguration;
            this.currentConstantPointer = this.memoryConfiguration.StackMax + 1024;
        }

        public void AddDepedency(string hostFunctionName)
        {
            dependency.Add(hostFunctionName);
        }

        public int AddConstant(string constant)
        {
            if (!constants.ContainsKey(constant))
            {
                constants.Add(constant, currentConstantPointer);
                currentConstantPointer += constant.Length + 1;
            }
            return constants[constant];
        }

        public void Emit(OpCode opcode)
        {
            program.Add((int)opcode);
        }

        public void Emit(OpCode opcode, int operand)
        {
            program.Add((int)opcode);
            program.Add(operand);
        }

        public void EmitPushImmediate(int imm)
        {
            program.Add((int)OpCode.PUSH_IMMEDIATE);
            program.Add(imm);
        }

        public void EmitLabel(string label)
        {
            labels.Add(label, program.Count);

            if (unpatchedLabels.ContainsKey(label))
            {
                foreach (var unpatchedJump in unpatchedLabels[label])
                {
                    program[unpatchedJump] = program.Count;
                }
                unpatchedLabels.Remove(label);
            }
        }
        public void UpdateLabel(string label)
        {
            labels[label] = program.Count;
        }

        public void EmitCall(OpCode opcode, string function)
        {
            if (opcode != OpCode.CALL)
            {
                throw new InvalidOperationException($"Expect <{OpCode.CALL}>, got {opcode}");
            }

            if (!labels.ContainsKey(function))
            {
                program.Add((int)opcode);
                if (unpatchedLabels.ContainsKey(function))
                {
                    unpatchedLabels[function].Add(program.Count);
                }
                else
                {
                    unpatchedLabels[function] = new List<int>() { program.Count };
                }
                program.Add(-1);
                return;
            }

            program.Add((int)opcode);
            program.Add(labels[function]);
        }

        public void EmitJump(OpCode opcode, string label)
        {
            if (opcode != OpCode.BRANCH
             && opcode != OpCode.BRANCH_IF_ZERO)
            {
                throw new InvalidOperationException($"Expect <{OpCode.BRANCH}|{OpCode.BRANCH_IF_ZERO}>, got {opcode}");
            }

            if (!labels.ContainsKey(label))
            {
                program.Add((int)opcode);
                if (unpatchedLabels.ContainsKey(label))
                {
                    unpatchedLabels[label].Add(program.Count);
                }
                else
                {
                    unpatchedLabels[label] = new List<int>() { program.Count };
                }
                program.Add(-1);
                return;
            }

            program.Add((int)opcode);
            program.Add(labels[label]);
        }

        public void EmitHostFunctionCall(string hostFunctionName, params int[] args)
        {
            if (dependency.Contains(hostFunctionName))
            {
                program.Add((int)OpCode.CALL_HOST_FUNCTION);
                program.Add(dependency.IndexOf(hostFunctionName));
                program.Add(args.Length);
                program.AddRange(args);
            }
            else
            {
                throw new KeyNotFoundException($"Unregistered depedency: {hostFunctionName}");
            }
        }

        public void EmitRaw(params int[] array)
        {
            program.AddRange(array);
        }

        public ToyProgram Serialize()
        {
            Emit(OpCode.HALT);
            return new ToyProgram(program.ToArray(), dependency.ToArray(), labels, constants);
        }
    }
}