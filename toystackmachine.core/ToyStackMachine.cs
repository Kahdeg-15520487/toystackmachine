﻿using System;
using System.Collections.Generic;
using System.Linq;

using toystackmachine.core.ToyAssembly;

namespace toystackmachine.core
{

    public class ToyStackMachine
    {
        public static readonly int MinimumMemorySize = 1024;
        private readonly ToyStackMachineMemoryConfiguration config;
        private int[] memory;
        private int ip;
        private int sp;
        private Stack<int> callStack;
        private Dictionary<string, int> hostFunctionIndex;
        private List<Func<int[], int[], int>> hostFunctions;
        private ToyProgram loadedProgram;

        public bool isHalting { get; private set; }

        public ToyStackMachine(ToyStackMachineMemoryConfiguration config)
        {
            this.config = config;
            memory = new int[config.MemorySize];
            ip = config.ProgramStart;
            sp = config.StackStart;
            callStack = new Stack<int>();

            hostFunctionIndex = new Dictionary<string, int>();
            hostFunctions = new List<Func<int[], int[], int>>();
        }

        public void LoadProgram(ToyProgram program, bool debug = false)
        {
            if (program.ROM.Length >= config.StackStart - config.ProgramStart)
            {
                throw new ArgumentOutOfRangeException($"Program size must be less than {config.StackStart - config.ProgramStart}");
            }

            var availableDepedency = program.Dependency.Intersect(hostFunctionIndex.Keys).ToList();
            if (availableDepedency.Count < program.Dependency.Length)
            {
                throw new KeyNotFoundException($"Runtime missing dependency:{Environment.NewLine}{string.Join($",{Environment.NewLine}\t", program.Dependency.Except(availableDepedency))}{Environment.NewLine}");
            }

            var constants = program.Constants;
            foreach (var constant in constants.Forward)
            {
                var ptr = constant.Value;
                var value = constant.Key;
                SetArrayAt(ptr, value.Select(c => (int)c).ToArray());
            }

            this.loadedProgram = program;
            Array.Copy(program.ROM, 0, memory, config.ProgramStart, program.ROM.Length);
            ip = config.ProgramStart;
            if (debug)
            {
                Console.WriteLine("loaded:");
                Console.WriteLine(ToyAssemblyDisassembler.Diassemble(program));
            }
        }

        public void RegisterHostFuntion(string functionName, Func<int[], int[], int> hostFuntion)
        {
            if (hostFunctionIndex.ContainsKey(functionName))
            {
                throw new ArgumentException($"Host function {functionName} already registered!");
            }

            hostFunctions.Add(hostFuntion);
            hostFunctionIndex[functionName] = hostFunctions.IndexOf(hostFuntion);
        }

        public int[] EmitHostFunctionCall(string functionName, int[] args)
        {
            int[] array = new int[args.Length + 2];
            array[0] = hostFunctionIndex[functionName];
            array[1] = args.Length;
            for (int i = 0; i < args.Length; i++)
            {
                array[2 + i] = args[i];
            }
            return array;
        }

        public void Push(int value)
        {
            if (sp + 1 >= config.StackMax)
            {
                throw new StackOverflowException($"Max stack: {config.StackMax - config.StackStart}");
            }

            memory[sp] = value;
            sp++;
        }

        public void PushArray(int[] array)
        {
            if (sp + 1 + array.Length >= config.StackMax)
            {
                throw new StackOverflowException($"Exceed max stack({config.StackMax - config.StackStart}) by {sp + array.Length + 1 - config.StackMax}");
            }
            Array.Copy(array, 0, memory, sp, array.Length);
            sp += array.Length;
            Push(array.Length);
        }

        public int Pop()
        {
            if (sp - 1 < config.StackStart)
            {
                throw new Exception("StackUnderflow");
            }

            sp--;
            int value = memory[sp];
            return value;
        }

        public int[] PopArray()
        {
            int size = Pop();

            if (sp - size < config.StackStart)
            {
                throw new Exception("StackUnderflow");
            }

            sp -= size;
            int[] array = new int[size];
            Array.Copy(memory, sp, array, 0, size);
            return array;
        }

        public int GetAt(int ptr)
        {
            if (ptr < 0 || ptr > config.MemorySize)
            {
                throw new IndexOutOfRangeException($"Accessing {ptr} outside memory range (0-{config.MemorySize})");
            }

            return memory[ptr];
        }

        public int[] GetArrayAt(int ptr)
        {
            if (ptr < 0 || ptr > config.MemorySize)
            {
                throw new IndexOutOfRangeException($"Accessing {ptr} outside memory range (0-{config.MemorySize})");
            }

            int size = memory[ptr];
            int[] array = new int[size];
            Array.Copy(memory, ptr + 1, array, 0, size);
            return array;
        }

        public void SetAt(int ptr, int value)
        {
            if (ptr < 0 || ptr > config.MemorySize)
            {
                throw new IndexOutOfRangeException($"Accessing {ptr} outside memory range (0-{config.MemorySize})");
            }

            memory[ptr] = value;
        }

        public void SetArrayAt(int ptr, int[] array)
        {
            if (ptr < 0 || ptr > config.MemorySize)
            {
                throw new IndexOutOfRangeException($"Accessing {ptr} outside memory range (0-{config.MemorySize})");
            }

            memory[ptr] = array.Length;
            Array.Copy(array, 0, memory, ptr + 1, array.Length);
        }

        public void Run(string functionName = null, bool debug = false)
        {
            if (!string.IsNullOrEmpty(functionName))
            {
                ip = config.ProgramStart + this.loadedProgram.Labels.Forward[functionName];
                callStack.Push(config.ProgramStart + this.loadedProgram.ROM.Length - 1);
                isHalting = false;
            }

            if (isHalting)
            {
                throw new InvalidOperationException("Machine is halting");
            }

            while (ip < memory.Length)
            {
                OpCode opcode = (OpCode)memory[ip];
                if (debug)
                {
                    Console.WriteLine("=====");
                    Console.WriteLine($"ip: {ip - config.ProgramStart}");
                    Console.WriteLine($"sp: {sp}");
                    Console.WriteLine($"opcode: {opcode}");
                    Console.WriteLine($"callstack: {string.Join(", ", callStack.Select(c => c - config.ProgramStart))}");
                    Console.WriteLine($"stack: {string.Join(", ", GetActiveStack())}");
                    Console.WriteLine("=====");
                }
                int a, b, dest, pointer, size;
                bool isJump = false;

                switch (opcode)
                {
                    case OpCode.NOP:
                        break;
                    case OpCode.ADD:
                        b = Pop();
                        a = Pop();
                        Push(a + b);
                        break;
                    case OpCode.SUB:
                        b = Pop();
                        a = Pop();
                        Push(a - b);
                        break;
                    case OpCode.MUL:
                        b = Pop();
                        a = Pop();
                        Push(a * b);
                        break;
                    case OpCode.DIV:
                        b = Pop();
                        a = Pop();
                        Push(a / b);
                        break;
                    case OpCode.MOD:
                        b = Pop();
                        a = Pop();
                        Push(a % b);
                        break;
                    case OpCode.CMP:
                        b = Pop();
                        a = Pop();
                        Push(a > b ? 1 : a < b ? -1 : 0);
                        break;
                    case OpCode.EQ:
                        b = Pop();
                        a = Pop();
                        Push(a == b ? 1 : 0);
                        break;
                    case OpCode.NE:
                        b = Pop();
                        a = Pop();
                        Push(a != b ? 1 : 0);
                        break;
                    case OpCode.LT:
                        b = Pop();
                        a = Pop();
                        Push(a < b ? 1 : 0);
                        break;
                    case OpCode.GT:
                        b = Pop();
                        a = Pop();
                        Push(a > b ? 1 : 0);
                        break;
                    case OpCode.LE:
                        b = Pop();
                        a = Pop();
                        Push(a <= b ? 1 : 0);
                        break;
                    case OpCode.GE:
                        b = Pop();
                        a = Pop();
                        Push(a >= b ? 1 : 0);
                        break;
                    case OpCode.NOT:
                        a = Pop();
                        Push(a == 0 ? 1 : 0);
                        break;


                    case OpCode.BRANCH:
                        dest = memory[++ip];
                        ip = config.ProgramStart + dest;
                        isJump = true;
                        break;
                    case OpCode.BRANCH_IF_NOT_ZERO:
                        dest = memory[++ip];
                        if (Pop() != 0)
                        {
                            ip = config.ProgramStart + dest;
                            isJump = true;
                        }
                        break;
                    case OpCode.BRANCH_IF_ZERO:
                        dest = memory[++ip];
                        if (Pop() == 0)
                        {
                            ip = config.ProgramStart + dest;
                            isJump = true;
                        }
                        break;

                    case OpCode.CALL:
                        dest = memory[ip + 1];
                        callStack.Push(ip + 2);
                        ip = config.ProgramStart + dest;
                        isJump = true;
                        break;
                    case OpCode.RET:
                        ip = callStack.Pop();
                        isJump = true;
                        break;

                    case OpCode.PUSH_IMMEDIATE:
                        Push(memory[++ip]);
                        break;
                    case OpCode.GET:
                        {
                            pointer = memory[++ip];
                            Push(memory[pointer]);
                        }
                        break;
                    case OpCode.GETAT:
                        {
                            pointer = Pop();
                            Push(memory[pointer]);
                        }
                        break;
                    case OpCode.GETARRAY:
                        {
                            pointer = Pop();
                            PushArray(GetArrayAt(pointer));
                        }
                        break;
                    case OpCode.SET:
                        {
                            pointer = memory[++ip];
                            memory[pointer] = Pop();
                        }
                        break;
                    case OpCode.SETAT:
                        {
                            pointer = Pop();
                            a = Pop();
                            memory[pointer] = a;
                        }
                        break;
                    case OpCode.SETARRAY:
                        {
                            pointer = Pop();
                            SetArrayAt(pointer, PopArray());
                        }
                        break;
                    case OpCode.DUP:
                        {
                            var temp = Pop();
                            Push(temp);
                            Push(temp);
                        }
                        break;
                    case OpCode.TRIP:
                        {
                            var temp = Pop();
                            Push(temp);
                            Push(temp);
                            Push(temp);
                        }
                        break;
                    case OpCode.SWAP:
                        {
                            a = Pop();
                            b = Pop();
                            Push(a);
                            Push(b);
                        }
                        break;
                    case OpCode.DISCARD:
                        Pop();
                        break;
                    case OpCode.PRINT:
                        Console.WriteLine((char)Pop());
                        break;
                    case OpCode.PRINT_ARRAY:
                        {
                            var arr = PopArray();
                            Console.WriteLine(new string(arr.Select(i => (char)i).ToArray()));
                        }
                        break;
                    case OpCode.CALL_HOST_FUNCTION:
                        {
                            int functionIndex = memory[++ip];
                            int[] args = GetArrayAt(++ip);
                            ip += args.Length;
                            int result = hostFunctions[functionIndex](memory, args);
                            Push(result);
                        }
                        break;
                    case OpCode.HALT:
                        this.isHalting = true;
                        return;
                    default:
                        throw new InvalidOperationException("Invalid opcode: " + opcode);
                }

                if (!isJump) ip++;
            }
        }

        public int[] GetActiveStack()
        {
            int[] array = new int[sp - config.StackStart];
            Array.Copy(memory, config.StackStart, array, 0, sp - config.StackStart);
            return array;
        }
    }
}