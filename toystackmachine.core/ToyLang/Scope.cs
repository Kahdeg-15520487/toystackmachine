using System;
using System.Collections.Generic;

namespace toystackmachine.core.ToyLang
{
    public class ScopeVariable
    {
        public string Name { get; set; }
        public int Size { get; set; }
        public int Address { get; set; }
        public Scope Scope { get; set; }

        public ScopeVariable(string name, int size, int address, Scope scope)
        {
            Name = name;
            Size = size;
            Address = address;
            Scope = scope;
        }
    }

    public class Scope
    {
        private ToyStackMachineMemoryConfiguration memoryConfiguration;
        public Scope Parent { get; private set; }
        private Dictionary<string, ScopeVariable> variables { get; } = new Dictionary<string, ScopeVariable>();
        public int currentMemoryPointer = 0;
        public IEnumerable<string> defined => variables.Keys;

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

        public ScopeVariable this[string index]
        {
            get => Find(index);
        }

        private ScopeVariable Find(string name)
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
                var newVar = new ScopeVariable(value, size, currentMemoryPointer, this);
                variables.Add(value, newVar);
                if (size > 1) { currentMemoryPointer += 1; }
                currentMemoryPointer += size;
            }
        }
    }
}
