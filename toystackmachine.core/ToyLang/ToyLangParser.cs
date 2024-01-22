using System;
using System.Collections.Generic;
using System.Linq;

namespace toystackmachine.core.ToyLang
{
    public class ToyLangParser
    {
        private readonly ToyLangLexer _lexer;
        private Token _currentToken;

        public ToyLangParser(ToyLangLexer lexer)
        {
            _lexer = lexer;
            _currentToken = _lexer.NextToken();
            SkipWhiteSpace();
        }

        public void Eat(params TokenType[] tokenTypes)
        {
            if (tokenTypes.Contains(_currentToken.Type))
            {
                _currentToken = _lexer.NextToken();
                SkipWhiteSpace();
            }
            else
            {
                throw new Exception($"Token type mismatch: expected {string.Join(", ", tokenTypes)}, got {_currentToken.Type}");
            }
        }

        private void SkipWhiteSpace()
        {
            while ((_currentToken.Type == TokenType.Whitespace || _currentToken.Type == TokenType.NewLine || _currentToken.Type == TokenType.Comment))
            {
                Eat(TokenType.Whitespace, TokenType.NewLine, TokenType.Comment);
            }
        }

        public AST Program()
        {
            // Program : (Function)* EOFs
            var programNode = new ProgramNode();
            while (_currentToken.Type != TokenType.EOF)
            {
                switch (_currentToken.Type)
                {
                    case TokenType.Function:
                        programNode.AddFunction(Function() as FunctionNode);
                        break;
                    case TokenType.Var:
                        programNode.AddGlobalVariable(VariableDeclareStatement() as VariableDeclareStatement);
                        break;
                    case TokenType.Host:
                        //todo declare expected host function
                        //programNode.AddHostFunction(HostFunction() as HostFunctionNode);
                        break;
                }
            }
            Eat(TokenType.EOF);
            return programNode;
        }

        public AST Function()
        {
            // Function : FUNCTION ID OPEN_PAREN (ID (COMMA ID)*)? CLOSE_PAREN OPEN_BRACE (Statement)* CLOSE_BRACE
            Eat(TokenType.Function);
            var functionName = _currentToken;
            Eat(TokenType.Identifier);
            Eat(TokenType.OpenParenthesis);
            var parameters = new List<Var>();
            while (_currentToken.Type != TokenType.CloseParenthesis)
            {
                parameters.Add(Variable() as Var);
                if (_currentToken.Type == TokenType.Comma)
                {
                    Eat(TokenType.Comma);
                }
            }
            Eat(TokenType.CloseParenthesis);
            Eat(TokenType.OpenBrace);
            var nodes = new List<AST>();
            while (_currentToken.Type != TokenType.CloseBrace)
            {
                nodes.Add(Statement());
            }
            Eat(TokenType.CloseBrace);
            var root = new FunctionNode(functionName, parameters, nodes);
            return root;
        }

        public AST Statement()
        {
            // Statement : AssignmentStatement | ReturnStatement | WhileStatement | ForStatement | Empty
            if (_currentToken.Type == TokenType.Identifier)
            {
                var id = Variable();
                if (_currentToken.Type == TokenType.OpenParenthesis)
                {
                    return ExpressionStatement(id);
                }
                else
                {
                    return AssignmentStatement(v: id);
                }
            }
            else if (_currentToken.Type == TokenType.Return)
            {
                return ReturnStatement();
            }
            else if (_currentToken.Type == TokenType.While)
            {
                return WhileStatement();
            }
            else if (_currentToken.Type == TokenType.For)
            {
                return ForStatement();
            }
            else if (_currentToken.Type == TokenType.If)
            {
                return IfStatement();
            }
            else if (_currentToken.Type == TokenType.Print)
            {
                return PrintStatement();
            }
            else if (_currentToken.Type == TokenType.Var)
            {
                return VariableDeclareStatement();
            }
            else if (_currentToken.Type == TokenType.OpenBrace)
            {
                return CompoundStatement();
            }
            else
            {
                return Empty();
            }
        }

        private AST ExpressionStatement(AST id)
        {
            // ExpressionStatement : Expr SEMICOLON
            AST result;
            if (id != null)
            {
                result = new ExpressionStatement(FunctionCall((id as Var).Token));
            }
            else
            {
                result = new ExpressionStatement(Expr());
            }
            Eat(TokenType.Semicolon);
            return result;
        }

        public AST CompoundStatement()
        {
            // CompoundStatement : OPEN_BRACE (Statement)* CLOSE_BRACE
            Eat(TokenType.OpenBrace);
            var nodes = new List<AST>();
            while (_currentToken.Type != TokenType.CloseBrace)
            {
                nodes.Add(Statement());
            }
            Eat(TokenType.CloseBrace);
            var root = new CompoundStatement();
            root.Children = nodes;
            return root;
        }

        public AST IfStatement()
        {
            // IfStatement : IF OPEN_PAREN Expr CLOSE_PAREN Statement (ELSE Statement)?
            Eat(TokenType.If);
            Eat(TokenType.OpenParenthesis);
            var condition = Expr();
            Eat(TokenType.CloseParenthesis);
            var trueStatement = Statement();
            AST falseStatement = null;
            if (_currentToken.Type == TokenType.Else)
            {
                Eat(TokenType.Else);
                falseStatement = Statement();
            }
            return new IfStatement(condition, trueStatement, falseStatement);
        }

        public AST PrintStatement()
        {
            // PrintStatement : PRINT OPEN_PAREN Expr CLOSE_PAREN SEMICOLON
            Eat(TokenType.Print);
            Eat(TokenType.OpenParenthesis);
            var node = Expr();
            Eat(TokenType.CloseParenthesis);
            Eat(TokenType.Semicolon);
            return new PrintStatement(node);
        }

        public AST ForStatement()
        {
            // ForStatement : FOR OPEN_PAREN VariableDeclareStatement Expr SEMICOLON AssignmentStatement CLOSE_PAREN Statement
            Eat(TokenType.For);
            Eat(TokenType.OpenParenthesis);
            var initialization = VariableDeclareStatement();
            var condition = Expr();
            Eat(TokenType.Semicolon);
            var increment = AssignmentStatement(true);
            Eat(TokenType.CloseParenthesis);
            var statement = Statement();
            return new ForStatement(initialization, condition, increment, statement);
        }

        public AST WhileStatement()
        {
            // WhileStatement : WHILE OPEN_PAREN Expr CLOSE_PAREN Statement
            Eat(TokenType.While);
            Eat(TokenType.OpenParenthesis);
            var condition = Expr();
            Eat(TokenType.CloseParenthesis);
            var statement = Statement();
            return new WhileStatement(condition, statement);
        }

        public AST ReturnStatement()
        {
            // Consume the RETURN token
            Eat(TokenType.Return);

            // Parse the expression to return
            AST expr = Expr();

            // Expect a semicolon after the return statement
            Eat(TokenType.Semicolon);

            // Return a new return statement AST node
            return new ReturnStatement(expr);
        }

        public AST AssignmentStatement(bool isExpr = false, AST v = null)
        {
            // AssignmentStatement : VARIABLE (LBRACKET Expr RBRACKET)? ASSIGN Expr SEMICOLON
            var left = v ?? Variable();
            var token = _currentToken;
            if (token.Type == TokenType.OpenBracket)
            {
                Eat(TokenType.OpenBracket);
                left = new ArrayAccessExpression(left as Var, Expr());
                Eat(TokenType.CloseBracket);
            }
            Eat(TokenType.Assign);
            var right = Expr();
            if (!isExpr) Eat(TokenType.Semicolon);
            return new Assign(left, token, right);
        }

        public AST VariableDeclareStatement()
        {
            // VariableDeclareStatement : VAR VARIABLE ((ASSIGN Expr)? SEMICOLON | LBRACKET RBRACKET ASSIGN (LBRACKET NUMBER RBRACKET | LBRACE (Expr)* RBRACE) SEMICOLON)
            Eat(TokenType.Var);
            var variable = Variable() as Var;
            AST initializer = null;
            if (_currentToken.Type == TokenType.OpenBracket)
            {
                // ArrayDeclareStatement : VAR VARIABLE LBRACKET RBRACKET ASSIGN (LBRACKET NUMBER RBRACKET | LBRACE (Expr)* RBRACE) SEMICOLON
                Eat(TokenType.OpenBracket);
                Eat(TokenType.CloseBracket);
                Eat(TokenType.Assign);
                if (_currentToken.Type == TokenType.OpenBracket)
                {
                    Eat(TokenType.OpenBracket);
                    initializer = Expr();
                    Eat(TokenType.CloseBracket);
                }
                else if (_currentToken.Type == TokenType.OpenBrace)
                {
                    Eat(TokenType.OpenBrace);
                    var nodes = new List<AST>();
                    while (_currentToken.Type != TokenType.CloseBrace)
                    {
                        nodes.Add(Expr());
                        if (_currentToken.Type == TokenType.Comma)
                        {
                            Eat(TokenType.Comma);
                        }
                    }
                    Eat(TokenType.CloseBrace);
                    initializer = new ArrayInitializerExpression(nodes);
                }
                Eat(TokenType.Semicolon);
                return new VariableDeclareStatement(variable, initializer);
            }
            if (_currentToken.Type == TokenType.Assign)
            {
                Eat(TokenType.Assign);
                initializer = Expr();
            }
            Eat(TokenType.Semicolon);
            return new VariableDeclareStatement(variable, initializer);
        }

        public AST Variable()
        {
            // Variable : ID
            var node = new Var(_currentToken);
            Eat(TokenType.Identifier);
            return node;
        }

        public AST Empty()
        {
            // An empty production
            return new NoOp();
        }

        public AST FunctionCall(Token functionName)
        {
            // FunctionCall : Identifier LPAREN (Expr (COMMA Expr)*)? RPAREN
            var function = new FunctionCallExpression(functionName);
            Eat(TokenType.OpenParenthesis);
            while (_currentToken.Type != TokenType.CloseParenthesis)
            {
                function.AddArgument(Expr());
                if (_currentToken.Type == TokenType.Comma)
                {
                    Eat(TokenType.Comma);
                }
            }
            Eat(TokenType.CloseParenthesis);
            return function;
        }

        public AST Expr()
        {
            // Expr : Comparison
            return Comparison();
        }

        public AST Comparison()
        {
            // Comparison : Additive ((EQUAL | NOT_EQUAL | LESS_THAN | GREATER_THAN) Additive)*
            var node = Additive();

            while (_currentToken.Type == TokenType.Equal || _currentToken.Type == TokenType.NotEqual || _currentToken.Type == TokenType.LessThan || _currentToken.Type == TokenType.GreaterThan)
            {
                var token = _currentToken;
                if (token.Type == TokenType.Equal)
                {
                    Eat(TokenType.Equal);
                }
                else if (token.Type == TokenType.NotEqual)
                {
                    Eat(TokenType.NotEqual);
                }
                else if (token.Type == TokenType.LessThan)
                {
                    Eat(TokenType.LessThan);
                }
                else if (token.Type == TokenType.GreaterThan)
                {
                    Eat(TokenType.GreaterThan);
                }
                node = new BinOp(node, token, Additive());
            }

            return node;
        }

        public AST Additive()
        {
            // Expr : Term ((PLUS | MINUS) Term)*
            var node = Term();

            while (_currentToken.Type == TokenType.Plus || _currentToken.Type == TokenType.Minus)
            {
                var token = _currentToken;
                if (token.Type == TokenType.Plus)
                {
                    Eat(TokenType.Plus);
                }
                else if (token.Type == TokenType.Minus)
                {
                    Eat(TokenType.Minus);
                }
                node = new BinOp(node, token, Term());
            }

            return node;
        }

        public AST Term()
        {
            // Term : Factor ((MULTIPLY | DIVIDE) Factor)*
            var node = Factor();

            while (_currentToken.Type == TokenType.Multiply || _currentToken.Type == TokenType.Divide)
            {
                var token = _currentToken;
                if (token.Type == TokenType.Multiply)
                {
                    Eat(TokenType.Multiply);
                }
                else if (token.Type == TokenType.Divide)
                {
                    Eat(TokenType.Divide);
                }
                node = new BinOp(node, token, Factor());
            }

            return node;
        }

        public AST Factor()
        {
            // Factor : PLUS Factor
            //        | MINUS Factor
            //        | INCREMENT Factor
            //        | DECREMENT Factor
            //        | INTEGER
            //        | STRING
            //        | LPAREN Expr RPAREN
            //        | Variable
            //        | Variable LBRAKET Expr RBRAKET
            //        | FunctionCall
            //        | READ
            //        | TRUE
            //        | FALSE
            //        | SIZEOF
            var token = _currentToken;
            if (token.Type == TokenType.Plus)
            {
                Eat(TokenType.Plus);
                return new UnaryOp(token, Factor());
            }
            else if (token.Type == TokenType.Minus)
            {
                Eat(TokenType.Minus);
                return new UnaryOp(token, Factor());
            }
            else if (token.Type == TokenType.Increment)
            {
                Eat(TokenType.Increment);
                return new UnaryOp(token, Factor());
            }
            else if (token.Type == TokenType.Decrement)
            {
                Eat(TokenType.Decrement);
                return new UnaryOp(token, Factor());
            }
            else if (token.Type == TokenType.Number)
            {
                Eat(TokenType.Number);
                return new Num(token);
            }
            else if (token.Type == TokenType.String)
            {
                Eat(TokenType.String);
                return new LiteralString(token);
            }
            else if (token.Type == TokenType.OpenParenthesis)
            {
                Eat(TokenType.OpenParenthesis);
                var node = Expr();
                Eat(TokenType.CloseParenthesis);
                return node;
            }
            else if (token.Type == TokenType.True)
            {
                Eat(TokenType.True);
                return new Bool(token);
            }
            else if (token.Type == TokenType.False)
            {
                Eat(TokenType.False);
                return new Bool(token);
            }
            else if (token.Type == TokenType.Read)
            {
                Eat(TokenType.Read);
                Eat(TokenType.OpenParenthesis);
                Eat(TokenType.CloseParenthesis);
                return new ReadExpression();
            }
            else if (token.Type == TokenType.Identifier)
            {
                var id = Variable() as Var;
                if (_currentToken.Type == TokenType.OpenParenthesis)
                {
                    return FunctionCall(id.Token);
                }
                else if (_currentToken.Type == TokenType.OpenBracket)
                {
                    Eat(TokenType.OpenBracket);
                    var index = Expr();
                    Eat(TokenType.CloseBracket);
                    return new ArrayAccessExpression(id, index);
                }
                else
                {
                    return id;
                }
            }
            else if (token.Type == TokenType.Sizeof)
            {
                Eat(TokenType.Sizeof);
                Eat(TokenType.OpenParenthesis);
                var id = Variable() as Var;
                Eat(TokenType.CloseParenthesis);
                return new SizeOfExpression(id);
            }
            else
            {
                return Empty();
            }
        }
    }
}
