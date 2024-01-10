using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;

using toystackmachine.core.ToyAssembly;

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
            this.currentMemoryPointer = memoryConfiguration.StackMax + 1;
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

    public class ToyLangCompiler
    {
        private List<string> _instructions = new List<string>();
        private Dictionary<string, int> _functions = new Dictionary<string, int>();
        private ToyStackMachineMemoryConfiguration memoryConfiguration;
        private Scope global;
        private Scope currentScope;
        private bool isInFunction;

        public List<string> Compile(AST node, ToyStackMachineMemoryConfiguration memoryConfiguration)
        {
            _instructions.Clear();
            _functions.Clear();
            this.memoryConfiguration = memoryConfiguration;
            this.global = new Scope(memoryConfiguration: memoryConfiguration);
            currentScope = global;
            this.isInFunction = false;
            GenerateHeader();
            Visit(node);
            return _instructions;
        }

        private void GenerateHeader()
        {
            _instructions.Add("//declare machine's spec");
            _instructions.Add($"#config memsize {memoryConfiguration.MemorySize}");
            _instructions.Add($"#config programstart {memoryConfiguration.ProgramStart}");
            _instructions.Add($"#config stackstart {memoryConfiguration.StackStart}");
            _instructions.Add($"#config stackmax {memoryConfiguration.StackMax}");

            _instructions.Add("//declare host functions");
            _instructions.Add("#hostfunction hostadd");
            _instructions.Add("#hostfunction hostinput");
            _instructions.Add("#hostfunction hostprint");
        }

        private void Visit(AST node)
        {
            switch (node)
            {
                case ProgramNode program:
                    foreach (var variable in program.GlobalVars)
                    {
                        Visit(variable);
                    }
                    foreach (var function in program.Functions)
                    {
                        Visit(function);
                    }
                    break;
                case FunctionNode functionNode:
                    isInFunction = true;
                    //declare function name
                    _instructions.Add($"{functionNode.FunctionName.Value}:");
                    //declare function parameters
                    var localScope = new Scope(memoryConfiguration, global);
                    currentScope = localScope;
                    foreach (var parameter in functionNode.Parameters)
                    {
                        localScope.Define(parameter.Token.Value);
                    }
                    //pop parameters value from stack into local variables
                    for (int i = functionNode.Parameters.Count - 1; i >= 0; i--)
                    {
                        _instructions.Add($"pop {localScope[functionNode.Parameters[i].Token.Value]}");
                    }
                    //declare function body
                    foreach (var stmt in functionNode.Body)
                    {
                        Visit(stmt);
                    }
                    isInFunction = false;
                    currentScope = global;
                    break;
                case Assign assign:
                    Visit(assign.Right);
                    _instructions.Add($"set {currentScope[assign.Left.Token.Value]}");
                    break;
                case Var variable:
                    _instructions.Add($"get {currentScope[variable.Token.Value]}");
                    break;
                case Num number:
                    _instructions.Add($"push {number.Token.Value}");
                    break;
                case Bool boolean:
                    _instructions.Add($"push {(boolean.Value ? 1 : 0)}");
                    break;
                //todo constant string
                case BinOp binOp:
                    Visit(binOp.Left);
                    Visit(binOp.Right);
                    _instructions.Add(OpCodeParser.ToString(GetOpCode(binOp.Token.Type)));
                    break;
                case UnaryOp unaryOp:
                    Visit(unaryOp.Expr);
                    _instructions.Add(OpCodeParser.ToString(GetOpCode(unaryOp.Token.Type)));
                    break;
                case WhileStatement whileNode:
                    string startLabel = Guid.NewGuid().ToString();
                    string endLabel = Guid.NewGuid().ToString();
                    _instructions.Add($"{startLabel}:");
                    Visit(whileNode.Condition);
                    _instructions.Add($"brzero {endLabel}");
                    Visit(whileNode.Statement);
                    _instructions.Add($"br {startLabel}:");
                    _instructions.Add($"{endLabel}:");
                    break;
                case ReturnStatement returnStatement:
                    Visit(returnStatement.Expr);
                    _instructions.Add("ret");
                    break;
                case PrintStatement printStatement:
                    Visit(printStatement.Expr);
                    _instructions.Add("print");
                    break;
                case NoOp _:
                    break;
                default:
                    throw new Exception($"Unexpected node type {node.GetType()}");
            }
        }

        private OpCode GetOpCode(TokenType tokenType)
        {
            switch (tokenType)
            {
                case TokenType.Plus:
                    return OpCode.ADD;
                case TokenType.Minus:
                    return OpCode.SUB;
                case TokenType.Multiply:
                    return OpCode.MUL;
                case TokenType.Divide:
                    return OpCode.DIV;
                case TokenType.Modulo:
                    return OpCode.MOD;
                case TokenType.LessThan:
                    return OpCode.LT;
                case TokenType.GreaterThan:
                    return OpCode.GT;
                case TokenType.Equal:
                    return OpCode.EQ;
                case TokenType.NotEqual:
                    return OpCode.NE;
                case TokenType.Not:
                    return OpCode.NOT;
                case TokenType.GreaterThanOrEqual:
                    return OpCode.GE;
                case TokenType.LessThanOrEqual:
                    return OpCode.LE;
                default:
                    throw new Exception($"Unexpected token type {tokenType}");
            }
        }
    }
}
