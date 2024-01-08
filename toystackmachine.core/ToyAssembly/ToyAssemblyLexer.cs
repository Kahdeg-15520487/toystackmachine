using System;
using System.Text;

namespace toystackmachine.core.ToyAssembly
{
    public class ToyAssemblyLexer
    {
        string source;
        int index;
        int line;
        int column;

        public ToyAssemblyLexer(string source)
        {
            this.source = source;
            this.index = 0;
            this.line = 1;
            this.column = 1;
        }

        public bool IsEOF { get => index >= source.Length; }

        public Token NextToken()
        {
            if (index >= source.Length)
            {
                return new Token(TokenType.EOF, line, column, "");
            }

            char c = source[index];
            if (char.IsDigit(c) || (c == '-' && index + 1 < source.Length && char.IsDigit(source[index + 1])))
            {
                return NextNumber();
            }
            else if (c == '0' && index + 2 < source.Length && source[index + 1] == 'x' && char.IsDigit(source[index + 2]))
            {
                return NextHexNumber();
            }
            else if (c == 'b' && index + 1 < source.Length && (source[index + 1] == '0' || source[index + 1] == '1'))
            {
                return NextBinNumber();
            }
            else if (char.IsLetter(c))
            {
                return NextIdentifier();
            }
            else if (c == '#')
            {
                return NextDirective();
            }
            else if (c == '"')
            {
                return NextString();
            }
            else if (c == '\'')
            {
                return NextChar();
            }
            else if (c == '/' && index + 1 < source.Length && source[index + 1] == '/')
            {
                return NextComment();
            }
            else if (c == ' ' || c == '\t')
            {
                return NextWhitespace();
            }
            else if (c == '\n' || c == '\r')
            {
                return NextNewLine();
            }
            else
            {
                return NextSymbol();
            }
        }

        private Token NextBinNumber()
        {
            StringBuilder binNumber = new StringBuilder();
            int startLine = line;
            int startColumn = column;

            // Skip the opening 'b'
            index++;
            column++;

            while (index < source.Length && (source[index] == '0' || source[index] == '1'))
            {
                binNumber.Append(source[index]);
                index++;
                column++;
            }

            return new Token(TokenType.BinNumber, startLine, startColumn, binNumber.ToString());
        }

        private Token NextHexNumber()
        {
            StringBuilder hexNumber = new StringBuilder();
            int startLine = line;
            int startColumn = column;

            // Skip the opening '0x'
            index += 2;
            column += 2;

            while (index < source.Length && ((source[index] >= '0' && source[index] <= '9') || (source[index] >= 'a' && source[index] <= 'f') || (source[index] >= 'A' && source[index] <= 'F')))
            {
                hexNumber.Append(source[index]);
                index++;
                column++;
            }

            return new Token(TokenType.HexNumber, startLine, startColumn, hexNumber.ToString());
        }

        private Token NextDirective()
        {
            int startLine = line;
            int startColumn = column;

            // Skip the opening "#"
            index++;
            column++;

            StringBuilder directive = new StringBuilder();
            while (index < source.Length && char.IsLetter(source[index]))
            {
                directive.Append(source[index]);
                index++;
                column++;
            }

            return new Token(TokenType.Directive, startLine, startColumn, directive.ToString().Trim());
        }

        private Token NextSymbol()
        {
            throw new NotImplementedException();
        }

        private Token NextNewLine()
        {
            int startLine = line;
            int startColumn = column;

            if (index < source.Length && (source[index] == '\n' || source[index] == '\r' && source[index + 1] == '\n'))
            {
                if (source[index] == '\r')
                {
                    // Skip the next character '\n'
                    index++;
                }
                index++;
                line++;
                column = 0;
            }

            return new Token(TokenType.NewLine, startLine, startColumn, "\n");
        }

        private Token NextWhitespace()
        {
            int startLine = line;
            int startColumn = column;

            while (index < source.Length && char.IsWhiteSpace(source[index]))
            {
                index++;
                column++;
            }

            return new Token(TokenType.Whitespace, startLine, startColumn, " ");
        }

        private Token NextComment()
        {
            StringBuilder comment = new StringBuilder();
            int startLine = line;
            int startColumn = column;

            // Skip the opening "//"
            index += 2;
            column += 2;

            while (index < source.Length && source[index] != '\n')
            {
                comment.Append(source[index]);
                index++;
                column++;
            }

            return new Token(TokenType.Comment, startLine, startColumn, comment.ToString().Trim());
        }

        private Token NextChar()
        {
            int startLine = line;
            int startColumn = column;

            // Skip the opening quote
            index++;
            column++;

            // Get the character
            char ch = source[index];
            index++;
            column++;

            // Skip the closing quote
            if (index < source.Length)
            {
                index++;
                column++;
            }
            else
            {
                // report error if no closing quote
                throw new Exception("Missing closing quote");
            }

            return new Token(TokenType.Char, startLine, startColumn, ch.ToString());
        }

        private Token NextString()
        {
            StringBuilder str = new StringBuilder();
            int startLine = line;
            int startColumn = column;

            // Skip the opening quote
            index++;
            column++;

            while (index < source.Length && source[index] != '\"')
            {
                str.Append(source[index]);
                index++;
                column++;
            }

            // Skip the closing quote
            if (index < source.Length)
            {
                index++;
                column++;
            }
            else
            {
                // report error if no closing quote
                throw new Exception("Missing closing quote");
            }

            return new Token(TokenType.String, startLine, startColumn, str.ToString());
        }

        private Token NextIdentifier()
        {
            StringBuilder identifier = new StringBuilder();
            int startLine = line;
            int startColumn = column;

            while (index < source.Length && (char.IsLetter(source[index]) || char.IsDigit(source[index]) || source[index] == '_'))
            {
                identifier.Append(source[index]);
                index++;
                column++;
            }

            // Check for colon
            if (index < source.Length && source[index] == ':')
            {
                // Skip the colon
                index++;
                column++;
            }
            else
            {
                // If there's no colon, it's not a label
                return new Token(TokenType.Identifier, startLine, startColumn, identifier.ToString().Trim());
            }

            return new Token(TokenType.Label, startLine, startColumn, identifier.ToString().Trim());
        }

        private Token NextNumber()
        {
            StringBuilder number = new StringBuilder();
            int startLine = line;
            int startColumn = column;

            while (index < source.Length && char.IsDigit(source[index]))
            {
                number.Append(source[index]);
                index++;
                column++;
            }

            return new Token(TokenType.Number, startLine, startColumn, number.ToString());
        }
    }
}