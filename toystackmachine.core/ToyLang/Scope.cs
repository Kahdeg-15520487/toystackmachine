using System;
using System.Collections.Generic;

namespace toystackmachine.core.ToyLang
{
    public class Scope
    {
        private ToyStackMachineMemoryConfiguration memoryConfiguration;
        public Scope Parent { get; private set; }
        private Dictionary<string, int> variables { get; } = new Dictionary<string, int>();
        protected int currentMemoryPointer = 0;

        public Scope(ToyStackMachineMemoryConfiguration memoryConfiguration, Scope parent = null)
        {
            this.memoryConfiguration = memoryConfiguration;
            this.currentMemoryPointer = memoryConfiguration.StackStart + 1;
            if (parent != null)
            {
                this.currentMemoryPointer += parent.currentMemoryPointer;
            }
            Parent = parent;
        }

        public int this[string index]
        {
            get => Find(index);
        }

        private int Find(string name)
        {
            if (variables.ContainsKey(name))
            {
                return variables[name];
            }
            else if (Parent != null)
            {
                return Parent.Find(name);
            }
            else
            {
                throw new Exception($"Variable {name} not found");
            }
        }

        public void Define(string value, int size = 1)
        {
            if (variables.ContainsKey(value))
            {
                throw new Exception($"Variable {value} already defined");
            }
            else
            {
                if (currentMemoryPointer + size > memoryConfiguration.MemorySize)
                {
                    //todo check if there is enough space in the heap
                }
                variables.Add(value, currentMemoryPointer);
                currentMemoryPointer += size;
            }
        }
    }
}
