using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using toystackmachine.core.ToyAssembly;

namespace toystackmachine.core.ToyLang
{
    public class Scope
    {
        public Scope(Scope parent = null)
        {
            Parent = parent;
        }

        public Scope Parent { get; private set; }
        private Dictionary<string, int> variables { get; } = new Dictionary<string, int>();
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

        public void Define(string value)
        {
            if (variables.ContainsKey(value))
            {
                throw new Exception($"Variable {value} already defined");
            }
            else
            {
                variables.Add(value, variables.Count);
            }
        }
    }

    public class ToyLangCompiler
    {
        private List<string> _instructions = new List<string>();
        private Scope global;
        private Scope currentScope;
        private bool isInFunction;

        public List<string> Compile(AST node)
        {
            this.global = new Scope();
            currentScope = global;
            this.isInFunction = false;
            Visit(node);
            return _instructions;
        }

        private void Visit(AST node)
        {
            switch (node)
            {
                case FunctionNode functionNode:
                    isInFunction = true;
                    //declare function parameters
                    var localScope = new Scope(global);
                    currentScope = localScope;
                    foreach (var parameter in functionNode.Parameters)
                    {
                        localScope.Define(parameter.Token.value);
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
                    _instructions.Add($"set {assign.Left.Token.value}");
                    break;
                case Var variable:
                    _instructions.Add($"get {variable.Token.value}");
                    break;
                case Num number:
                    _instructions.Add($"push {number.Token.value}");
                    break;
                case BinOp binOp:
                    Visit(binOp.Left);
                    Visit(binOp.Right);
                    _instructions.Add(OpCodeParser.ToString(GetOpCode(binOp.Token.type)));
                    break;
                case UnaryOp unaryOp:
                    Visit(unaryOp.Expr);
                    _instructions.Add(OpCodeParser.ToString(GetOpCode(unaryOp.Token.type)));
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
                default:
                    throw new Exception($"Unexpected token type {tokenType}");
            }
        }
    }
}
