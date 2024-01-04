public class ToyAssembler
{
    ToyLexer lexer;
    ToyEmitter emitter;
    private Token currentToken;

    public ToyAssembler(ToyLexer lexer)
    {
        this.lexer = lexer;
        this.emitter = new ToyEmitter();
        this.currentToken = lexer.NextToken();
    }

    public bool Eat(params TokenType[] tokenTypes)
    {
        if (tokenTypes.Contains(currentToken.type))
        {
            this.currentToken = lexer.NextToken();
            return true;
        }
        else
        {
            throw new Exception($"Expect {string.Join(", ", tokenTypes)}");
        }
    }

    public (int[] binary, string[] depedency) Assemble()
    {
        while (!lexer.IsEOF)
        {
            switch (currentToken.type)
            {
                case TokenType.Directive:
                    AssembleDirective();
                    break;
                case TokenType.Label:
                    var label = currentToken;
                    Eat(TokenType.Label);
                    emitter.EmitLabel(label.value);
                    break;
                case TokenType.Identifier:
                    AssembleInstruction();
                    break;
                case TokenType.NewLine:
                case TokenType.Whitespace:
                case TokenType.Comment:
                    Eat(currentToken.type);
                    break;
                default:
                    throw new Exception($"Unexpected token {currentToken}");
            }
        }
        return emitter.Serialize();
    }

    private void AssembleInstruction()
    {
        if (ParseOpCode(currentToken.value, out var opcode))
        {
            Eat(TokenType.Identifier);
            switch (opcode)
            {
                case OpCode.BRANCH:
                case OpCode.BRANCH_IF_ZERO:
                    SkipWhiteSpace();
                    var label = currentToken.value;
                    Eat(TokenType.Identifier);
                    emitter.EmitJump(opcode, label);
                    break;
                case OpCode.PUSH_IMMEDIATE:
                case OpCode.GET:
                case OpCode.SET:
                    SkipWhiteSpace();
                    var number = currentToken;
                    Eat(TokenType.Number, TokenType.HexNumber, TokenType.BinNumber);
                    emitter.Emit(opcode, GetNumber(number));
                    break;
                case OpCode.CALL_HOST_FUNCTION:
                    SkipWhiteSpace();
                    var hostFunctionName = currentToken.value;
                    Eat(TokenType.Identifier);
                    SkipWhiteSpace();
                    if (currentToken.type == TokenType.Number ||
                        currentToken.type == TokenType.HexNumber ||
                        currentToken.type == TokenType.BinNumber)
                    {
                        var hostFunctionParameters = new List<int>();
                        while (!lexer.IsEOF && currentToken.type != TokenType.NewLine)
                        {
                            Eat(TokenType.Number, TokenType.HexNumber, TokenType.BinNumber);
                            SkipWhiteSpace();
                            hostFunctionParameters.Add(GetNumber(currentToken));
                        }
                        emitter.EmitHostFunctionCall(hostFunctionName, hostFunctionParameters.ToArray());
                    }
                    else
                    {
                        emitter.EmitHostFunctionCall(hostFunctionName);
                    }
                    break;
                default:
                    emitter.Emit(opcode);
                    break;
            }
        }
        else
        {
            throw new Exception($"Invalid opcode {currentToken.value}");
        }
    }

    private bool ParseOpCode(string token, out OpCode opcode)
    {
        switch (token)
        {
            case "nop":
                opcode = OpCode.NOP;
                return true;
            case "add":
                opcode = OpCode.ADD;
                return true;
            case "sub":
                opcode = OpCode.SUB;
                return true;
            case "mul":
                opcode = OpCode.MUL;
                return true;
            case "div":
                opcode = OpCode.DIV;
                return true;
            case "cmp":
                opcode = OpCode.CMP;
                return true;
            case "br":
                opcode = OpCode.BRANCH;
                return true;
            case "brzero":
                opcode = OpCode.BRANCH_IF_ZERO;
                return true;
            case "brnzero":
                opcode = OpCode.BRANCH_IF_NOT_ZERO;
                return true;
            case "push":
                opcode = OpCode.PUSH_IMMEDIATE;
                return true;
            case "get":
                opcode = OpCode.GET;
                return true;
            case "set":
                opcode = OpCode.SET;
                return true;
            case "dup":
                opcode = OpCode.DUP;
                return true;
            case "trip":
                opcode = OpCode.TRIP;
                return true;
            case "discard":
                opcode = OpCode.DISCARD;
                return true;
            case "print":
                opcode = OpCode.PRINT;
                return true;
            case "printarray":
                opcode = OpCode.PRINT_ARRAY;
                return true;
            case "callhost":
                opcode = OpCode.CALL_HOST_FUNCTION;
                return true;
            case "halt":
                opcode = OpCode.HALT;
                return true;
            default:
                opcode = OpCode.NOP;
                return false;
        }
    }

    private void SkipWhiteSpace()
    {
        while (!lexer.IsEOF && currentToken.type == TokenType.Whitespace)
        {
            Eat(TokenType.Whitespace);
        }
    }

    private int GetNumber(Token number)
    {
        switch (number.type)
        {
            case TokenType.Number:
                return int.Parse(number.value);
            case TokenType.HexNumber:
                return Convert.ToInt32(number.value, 16);
            case TokenType.BinNumber:
                return Convert.ToInt32(number.value, 2);
            default:
                throw new Exception($"Invalid number {number}");
        }
    }

    private void AssembleDirective()
    {
        var directive = currentToken.value.Split(" ");
        Eat(TokenType.Directive);

        switch (directive[0])
        {
            case "hostfunction":
                emitter.AddDepedency(directive[1]);
                break;
            default:
                break;
        }
    }
}
