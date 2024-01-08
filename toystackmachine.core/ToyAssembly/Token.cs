namespace toystackmachine.core.ToyAssembly
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
    }
}