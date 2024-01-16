using System;
using System.Collections.Generic;

using toystackmachine.core.ToyAssembly;

namespace toystackmachine.core.ToyLang
{
    public class ToyLangCompiler
    {
        private List<string> _instructions = new List<string>();
        private Dictionary<string, int> _functions = new Dictionary<string, int>();
        private ToyStackMachineMemoryConfiguration memoryConfiguration;
        private Scope global;
        private Scope currentScope;
        private bool isInFunction;
        private Random randomLabelCounter;

        public string Compile(AST node, ToyStackMachineMemoryConfiguration memoryConfiguration)
        {
            _instructions.Clear();
            _functions.Clear();
            randomLabelCounter = new Random();
            this.memoryConfiguration = memoryConfiguration;
            this.global = new Scope(memoryConfiguration: memoryConfiguration);
            currentScope = global;
            this.isInFunction = false;
            GenerateHeader();
            Visit(node);
            return string.Join(Environment.NewLine, _instructions);
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
                        _instructions.Add($"set {localScope[functionNode.Parameters[i].Token.Value]}");
                    }
                    //declare function body
                    foreach (var stmt in functionNode.Body)
                    {
                        Visit(stmt);
                    }
                    _instructions.Add("ret");
                    isInFunction = false;
                    currentScope = global;
                    break;
                case VariableDeclareStatement variableDeclareStatement:
                    if (variableDeclareStatement.Initializer != null)
                    {
                        switch (variableDeclareStatement.Initializer)
                        {
                            case Num num:
                                currentScope.Define(variableDeclareStatement.Variable.Token.Value, num.Value);
                                break;
                            case ArrayInitializerExpression arrayInitializerExpression:
                                currentScope.Define(variableDeclareStatement.Variable.Token.Value, arrayInitializerExpression.Elements.Count);
                                foreach (var expr in arrayInitializerExpression.Elements)
                                {
                                    Visit(expr);
                                }
                                _instructions.Add($"push {arrayInitializerExpression.Elements.Count}");
                                _instructions.Add($"push {currentScope[variableDeclareStatement.Variable.Token.Value]}");
                                _instructions.Add($"setarray");
                                break;
                            default:
                                Visit(variableDeclareStatement.Initializer);
                                _instructions.Add($"set {currentScope[variableDeclareStatement.Variable.Token.Value]}");
                                break;
                        }
                    }
                    break;
                case IfStatement ifStatement:
                    {
                        Visit(ifStatement.Condition);
                        string elseLabel = GenerateRandomLabel("else");
                        string endLabel = GenerateRandomLabel("endif");
                        _instructions.Add($"brzero {elseLabel}");
                        Visit(ifStatement.TrueStatement);
                        _instructions.Add($"br {endLabel}");
                        _instructions.Add($"{elseLabel}:");
                        if (ifStatement.FalseStatement != null)
                        {
                            Visit(ifStatement.FalseStatement);
                        }
                        else
                        {
                            _instructions.Add($"nop");
                        }
                        _instructions.Add($"{endLabel}:");
                    }
                    break;
                case Assign assign:
                    Visit(assign.Right);
                    switch (assign.Left)
                    {
                        case ArrayAccessExpression arrayAccessExpression:
                            Visit(arrayAccessExpression.Index);
                            _instructions.Add($"push {currentScope[arrayAccessExpression.Array.Token.Value]}");
                            _instructions.Add($"add");
                            _instructions.Add($"setat");
                            break;
                        default:
                            _instructions.Add($"set {currentScope[(assign.Left as Var).Token.Value]}");
                            break;
                    }
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
                case FunctionCallExpression functionCallExpression:
                    foreach (var argument in functionCallExpression.Arguments)
                    {
                        Visit(argument);
                    }
                    _instructions.Add($"call {functionCallExpression.FunctionName.Value}");
                    break;
                case ArrayAccessExpression arrayAccessExpression:
                    Visit(arrayAccessExpression.Index);
                    _instructions.Add($"push {currentScope[arrayAccessExpression.Array.Token.Value]}");
                    _instructions.Add($"add");
                    _instructions.Add($"getat");
                    break;
                //todo constant string
                case BinOp binOp:
                    Visit(binOp.Left);
                    Visit(binOp.Right);
                    _instructions.Add(OpCodeParser.ToString(GetOpCode(binOp.Token.Type)));
                    break;
                case UnaryOp unaryOp:
                    Visit(unaryOp.Expr);
                    if (unaryOp.Token.Type == TokenType.Increment || unaryOp.Token.Type == TokenType.Decrement)
                    {
                        _instructions.Add("push 1");
                    }
                    _instructions.Add(OpCodeParser.ToString(GetOpCode(unaryOp.Token.Type)));
                    break;
                case WhileStatement whileNode:
                    {
                        string startLabel = GenerateRandomLabel("whilestart");
                        string endLabel = GenerateRandomLabel("whileend");
                        _instructions.Add($"{startLabel}:");
                        Visit(whileNode.Condition);
                        _instructions.Add($"brzero {endLabel}");
                        Visit(whileNode.Statement);
                        _instructions.Add($"br {startLabel}");
                        _instructions.Add($"{endLabel}:");
                    }
                    break;
                case ForStatement forStatement:
                    {
                        string startLabel = GenerateRandomLabel("forstart");
                        string endLabel = GenerateRandomLabel("forend");
                        Visit(forStatement.Initialization);
                        _instructions.Add($"{startLabel}:");
                        Visit(forStatement.Condition);
                        _instructions.Add($"brzero {endLabel}");
                        Visit(forStatement.Statement);
                        Visit(forStatement.Increment);
                        _instructions.Add($"br {startLabel}");
                        _instructions.Add($"{endLabel}:");
                    }
                    break;
                case ReturnStatement returnStatement:
                    Visit(returnStatement.Expr);
                    _instructions.Add("ret");
                    break;
                case PrintStatement printStatement:
                    Visit(printStatement.Expr);
                    _instructions.Add("print");
                    break;
                case CompoundStatement compoundStatement:
                    foreach (var statement in compoundStatement.Children)
                    {
                        Visit(statement);
                    }
                    break;
                case ReadExpression _:
                    _instructions.Add("callhost hostinput");
                    break;
                case NoOp _:
                    break;
                default:
                    throw new Exception($"Unexpected node type {node.GetType()}");
            }
        }

        private string GenerateRandomLabel(string prefix = "L")
        {
            byte[] bytes = BitConverter.GetBytes(randomLabelCounter.Next());
            string base64Label = Convert.ToBase64String(bytes).Replace('+', '_').Replace('/', '_');
            return prefix + "_" + base64Label.Substring(0, 6);
        }

        private OpCode GetOpCode(TokenType tokenType)
        {
            switch (tokenType)
            {
                case TokenType.Plus:
                case TokenType.Increment:
                    return OpCode.ADD;
                case TokenType.Minus:
                case TokenType.Decrement:
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
