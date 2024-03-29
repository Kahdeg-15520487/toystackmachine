using System;
using System.Collections.Generic;
using System.Linq;

using toystackmachine.core.ToyAssembly;

namespace toystackmachine.core.ToyLang
{
    public class ToyLangCompiler
    {
        private List<string> _headers = new List<string>();
        private List<string> _instructions = new List<string>();
        private Dictionary<string, int> _functions = new Dictionary<string, int>();
        private ToyStackMachineMemoryConfiguration memoryConfiguration;
        private Scope global;
        private Scope currentScope;
        private Scope constants;
        private bool isInFunction;
        private Random randomLabelCounter;
        private bool isLastVariableArray;

        public string Compile(AST node, ToyStackMachineMemoryConfiguration memoryConfiguration)
        {
            _headers.Clear();
            _instructions.Clear();
            _functions.Clear();
            randomLabelCounter = new Random();
            this.memoryConfiguration = memoryConfiguration;
            this.global = new Scope(memoryConfiguration: memoryConfiguration);
            this.constants = new Scope(memoryConfiguration: memoryConfiguration);
            this.constants.currentMemoryPointer = memoryConfiguration.StackMax + 1024;
            currentScope = global;
            this.isInFunction = false;
            Visit(node);
            GenerateHeader();
            return string.Join(Environment.NewLine, _instructions);
        }

        private void GenerateHeader()
        {
            _headers.Add("//declare machine's spec");
            _headers.Add($"#config memsize {memoryConfiguration.MemorySize}");
            _headers.Add($"#config programstart {memoryConfiguration.ProgramStart}");
            _headers.Add($"#config stackstart {memoryConfiguration.StackStart}");
            _headers.Add($"#config stackmax {memoryConfiguration.StackMax}");

            _headers.Add("//declare host functions");
            _headers.Add("#hostfunction hostadd");
            _headers.Add("#hostfunction hostinput");
            _headers.Add("#hostfunction hostprint");

            foreach (var s in constants.defined)
            {
                _headers.Add($"#data \"{s}\"");
            }

            _instructions = _headers.Concat(_instructions).ToList();
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
                        _instructions.Add($"set {localScope[functionNode.Parameters[i].Token.Value].Address}");
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
                                currentScope.Define(variableDeclareStatement.Variable.Token.Value);
                                _instructions.Add($"push {num.Token.Value}");
                                _instructions.Add($"set {currentScope[variableDeclareStatement.Variable.Token.Value].Address}");
                                break;
                            case ArrayInitializerExpression arrayInitializerExpression:
                                currentScope.Define(variableDeclareStatement.Variable.Token.Value, arrayInitializerExpression.Elements.Count);
                                foreach (var expr in arrayInitializerExpression.Elements)
                                {
                                    Visit(expr);
                                }
                                _instructions.Add($"push {arrayInitializerExpression.Elements.Count}");
                                _instructions.Add($"push {currentScope[variableDeclareStatement.Variable.Token.Value].Address}");
                                _instructions.Add("setarray");
                                break;
                            case LiteralString literalString:
                                Visit(literalString);
                                currentScope.Define(variableDeclareStatement.Variable.Token.Value, literalString.Token.Value.Length);
                                _instructions.Add($"push {currentScope[variableDeclareStatement.Variable.Token.Value].Address}");
                                _instructions.Add("setarray");
                                break;
                            default:
                                currentScope.Define(variableDeclareStatement.Variable.Token.Value);
                                Visit(variableDeclareStatement.Initializer);
                                _instructions.Add($"set {currentScope[variableDeclareStatement.Variable.Token.Value].Address}");
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
                            _instructions.Add($"push {currentScope[arrayAccessExpression.Array.Token.Value].Address}");
                            _instructions.Add($"add");
                            _instructions.Add($"setat");
                            break;
                        default:
                            _instructions.Add($"set {currentScope[(assign.Left as Var).Token.Value].Address}");
                            break;
                    }
                    break;
                case Var variable:
                    {
                        var v = currentScope[variable.Token.Value];
                        if (v.Size > 1)
                        {
                            _instructions.Add($"push {v.Address}");
                            _instructions.Add("getarray");
                            this.isLastVariableArray = v.Size > 1;
                        }
                        else
                        {
                            _instructions.Add($"get {v.Address}");
                        }
                    }
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
                    _instructions.Add($"push {currentScope[arrayAccessExpression.Array.Token.Value].Address}");
                    _instructions.Add($"add");
                    _instructions.Add($"getat");
                    break;
                case LiteralString literalString:
                    this.constants.Define(literalString.Token.Value, literalString.Token.Value.Length);
                    _instructions.Add($"push {this.constants[literalString.Token.Value].Address}");
                    _instructions.Add($"getarray");
                    isLastVariableArray = true;
                    break;
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
                    isLastVariableArray = false;
                    Visit(printStatement.Expr);
                    if (isLastVariableArray)
                    {
                        _instructions.Add("printarray");
                    }
                    else
                    {
                        _instructions.Add("print");
                    }
                    break;
                case CompoundStatement compoundStatement:
                    foreach (var statement in compoundStatement.Children)
                    {
                        Visit(statement);
                    }
                    break;
                case ExpressionStatement expressionStatement:
                    Visit(expressionStatement.Expr);
                    _instructions.Add("discard");
                    break;
                case ReadExpression _:
                    _instructions.Add("callhost hostinput");
                    break;
                case SizeOfExpression sizeOfExpression:
                    _instructions.Add($"push {currentScope[sizeOfExpression.v.Token.Value].Size}");
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
