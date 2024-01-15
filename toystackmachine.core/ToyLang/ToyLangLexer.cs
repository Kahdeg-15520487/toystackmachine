using System;
using System.Collections.Generic;

namespace toystackmachine.core.ToyLang
{
    public class ToyLangLexer
    {
        private readonly string _input;
        private int _position;
        private int _line;
        private int _column;

        private static readonly HashSet<string> _keywords = new HashSet<string>
        {
            "if", "else", "while", "do", "for", "break", "continue", "return", "var",
            "function", "print", "read", "true", "false",
        };

        public ToyLangLexer(string input)
        {
            _input = input;
            _position = 0;
            _line = 1;
            _column = 1;
        }

        public bool IsEOF
        {
            get
            {
                return _position >= _input.Length;
            }
        }

        public Token NextToken()
        {
            if (_position >= _input.Length)
            {
                return new Token(TokenType.EOF, _line, _column, null);
            }

            char current = _input[_position];
            _position++;
            _column++;

            if (char.IsWhiteSpace(current))
            {
                if (current == '\n')
                {
                    _line++;
                    _column = 1;
                    return new Token(TokenType.NewLine, _line, _column, current.ToString());
                }
                else
                {
                    return new Token(TokenType.Whitespace, _line, _column, current.ToString());
                }
            }

            if (char.IsDigit(current))
            {
                return new Token(TokenType.Number, _line, _column, current.ToString());
            }

            if (current == '0' && _input[_position] == 'x')
            {
                _position++;
                _column++;
                return this.ConvertToken(new Token(TokenType.HexNumber, _line, _column, current.ToString()));
            }

            if (current == 'b' && (_input[_position] == '0' || _input[_position] == '1'))
            {
                _position++;
                _column++;
                return this.ConvertToken(new Token(TokenType.BinNumber, _line, _column, current.ToString()));
            }

            if (char.IsLetter(current))
            {
                string identifier = current.ToString();
                while (_position < _input.Length && (char.IsLetter(_input[_position]) || char.IsDigit(_input[_position])))
                {
                    identifier += _input[_position];
                    _position++;
                    _column++;
                }

                if (_keywords.Contains(identifier))
                {
                    return new Token((TokenType)Enum.Parse(typeof(TokenType), identifier[0].ToString().ToUpper() + identifier.Substring(1)), _line, _column, identifier);
                }

                return new Token(TokenType.Identifier, _line, _column, identifier);
            }

            switch (current)
            {
                case '"':
                    return new Token(TokenType.String, _line, _column, current.ToString());
                case '\'':
                    return new Token(TokenType.Char, _line, _column, current.ToString());
                case '/':
                    if (_input[_position] == '/')
                    {
                        _position++;
                        _column++;
                        return new Token(TokenType.Comment, _line, _column, "//");
                    }
                    else
                    {
                        return new Token(TokenType.Divide, _line, _column, current.ToString());
                    }

                case ',':
                    return new Token(TokenType.Comma, _line, _column, current.ToString());
                case '(':
                    return new Token(TokenType.OpenParenthesis, _line, _column, current.ToString());
                case ')':
                    return new Token(TokenType.CloseParenthesis, _line, _column, current.ToString());
                case '{':
                    return new Token(TokenType.OpenBrace, _line, _column, current.ToString());
                case '}':
                    return new Token(TokenType.CloseBrace, _line, _column, current.ToString());
                case '[':
                    return new Token(TokenType.OpenBracket, _line, _column, current.ToString());
                case ']':
                    return new Token(TokenType.CloseBracket, _line, _column, current.ToString());
                case '+':
                    return new Token(TokenType.Plus, _line, _column, current.ToString());
                case '-':
                    return new Token(TokenType.Minus, _line, _column, current.ToString());
                case '*':
                    return new Token(TokenType.Multiply, _line, _column, current.ToString());
                case '%':
                    return new Token(TokenType.Modulo, _line, _column, current.ToString());
                case ';':
                    return new Token(TokenType.Semicolon, _line, _column, current.ToString());
                case '<':
                    if (_input[_position] == '=')
                    {
                        _position++;
                        _column++;
                        return new Token(TokenType.LessThanOrEqual, _line, _column, "<=");
                    }
                    else
                    {
                        return new Token(TokenType.LessThan, _line, _column, current.ToString());
                    }
                case '>':
                    if (_input[_position] == '=')
                    {
                        _position++;
                        _column++;
                        return new Token(TokenType.GreaterThanOrEqual, _line, _column, ">=");
                    }
                    else
                    {
                        return new Token(TokenType.GreaterThan, _line, _column, current.ToString());
                    }
                case '=':
                    if (_input[_position] == '=')
                    {
                        _position++;
                        _column++;
                        return new Token(TokenType.Equal, _line, _column, "==");
                    }
                    else
                    {
                        return new Token(TokenType.Assign, _line, _column, current.ToString());
                    }
                case '!':
                    if (_input[_position] == '=')
                    {
                        _position++;
                        _column++;
                        return new Token(TokenType.NotEqual, _line, _column, "!=");
                    }
                    else
                    {
                        return new Token(TokenType.Not, _line, _column, current.ToString());
                    }
            }

            return new Token(TokenType.EOF, _line, _column, null);
        }

        public Token ConvertToken(Token token)
        {
            if (token.Type == TokenType.HexNumber)
            {
                string value = token.Value.Substring(2); // remove '0x'
                int number = Convert.ToInt32(value, 16);
                return new Token(TokenType.Number, token.Line, token.Column, number.ToString());
            }
            else if (token.Type == TokenType.BinNumber)
            {
                string value = token.Value.Substring(1); // remove 'b'
                int number = Convert.ToInt32(value, 2);
                return new Token(TokenType.Number, token.Line, token.Column, number.ToString());
            }
            else
            {
                return token;
            }
        }
    }
}
