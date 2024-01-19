using System;
using System.Collections.Generic;
using System.Linq;

namespace toystackmachine.core.ToyAssembly
{
    public class ToyAssembler
    {
        ToyAssemblyLexer lexer;
        ToyEmitter emitter;
        private Token currentToken;

        public ToyAssembler(ToyAssemblyLexer lexer, ToyStackMachineMemoryConfiguration memoryConfiguration)
        {
            this.lexer = lexer;
            this.emitter = new ToyEmitter(memoryConfiguration);
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

        public ToyProgram Assemble()
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
            if (OpCodeParser.ParseOpCode(currentToken.value, out OpCode opcode))
            {
                Eat(TokenType.Identifier);
                switch (opcode)
                {
                    case OpCode.BRANCH:
                    case OpCode.BRANCH_IF_ZERO:
                    case OpCode.BRANCH_IF_NOT_ZERO:
                        SkipWhiteSpace();
                        var label = currentToken.value;
                        Eat(TokenType.Identifier);
                        emitter.EmitJump(opcode, label);
                        break;
                    case OpCode.CALL:
                        SkipWhiteSpace();
                        var function = currentToken.value;
                        Eat(TokenType.Identifier);
                        emitter.EmitCall(opcode, function);
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
                                SkipWhiteSpace();
                                hostFunctionParameters.Add(GetNumber(currentToken));
                                Eat(TokenType.Number, TokenType.HexNumber, TokenType.BinNumber);
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
            var directive = currentToken.value;
            Eat(TokenType.Directive);

            switch (directive)
            {
                case "config":
                    SkipWhiteSpace();
                    Eat(TokenType.Identifier);
                    SkipWhiteSpace();
                    Eat(TokenType.Number);
                    break;
                case "hostfunction":
                    SkipWhiteSpace();
                    var hostfunction = currentToken.value;
                    Eat(TokenType.Identifier);
                    emitter.AddDepedency(hostfunction);
                    break;
                case "data":
                    SkipWhiteSpace();
                    var dataContent = currentToken;
                    Eat(TokenType.String); //todo int array
                    switch (dataContent.type)
                    {
                        case TokenType.String:
                            {
                                emitter.AddConstant(dataContent.value);
                            }
                            break;
                        case TokenType.Char:
                            break;
                        case TokenType.Label:
                            break;
                        case TokenType.Number:
                            break;
                        case TokenType.HexNumber:
                            break;
                        case TokenType.BinNumber:
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
        }
    }
}