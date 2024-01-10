namespace toystackmachine.core.ToyLang
{
    public class Token
    {
        public readonly TokenType Type;
        public readonly int Line;
        public readonly int Column;
        public readonly string Value;

        public Token(TokenType type, int line, int column, string value)
        {
            this.Type = type;
            this.Line = line;
            this.Column = column;
            this.Value = value;
        }

        public override string ToString()
        {
            return $"[{Type} {Line}:{Column}] {Value}";
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
        Function, Print, Read, True, False,
        Host,

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
        LessThan,
        LessThanOrEqual,
        GreaterThan,
        GreaterThanOrEqual,
        Equal,
        NotEqual,
        Not,

        Assign,
        Semicolon,
        Comma,
    }
}