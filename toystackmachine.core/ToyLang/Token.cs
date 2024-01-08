namespace toystackmachine.core.ToyLang
{
    public class Token
    {
        public readonly TokenType type;
        public readonly int line;
        public readonly int column;
        public readonly string value;

        public Token(TokenType type, int line, int column, string value)
        {
            this.type = type;
            this.line = line;
            this.column = column;
            this.value = value;
        }

        public override string ToString()
        {
            return $"[{type} {line}:{column}] {value}";
        }
    }
    public enum TokenType
    {
        EOF,
        Number,
        Identifier,
        String,
        Char,
        Comment,
        Whitespace,
        NewLine,
        Directive,
        Label,
        HexNumber,
        BinNumber,

        // Keywords as individual token types
        If, Else, While, Do, For, Break, Continue, Return, Var,
        Function, Print, Input, True, False,

        OpenParenthesis,
        CloseParenthesis,
        OpenBrace,
        CloseBrace,
        OpenBracket,
        CloseBracket,

        Plus,
        Minus,
        Multiply,
        Divide,
        Modulo,

        Assign,
        Semicolon,
        Comma,
        LessThan,
        GreaterThan,
        Equal,
        NotEqual,
        Not,
    }
}