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
            if (tokenTypes.Contains(_currentToken.type))
            {
                _currentToken = _lexer.NextToken();
                SkipWhiteSpace();
            }
            else
            {
                throw new Exception($"Token type mismatch: expected {string.Join(", ", tokenTypes)}, got {_currentToken.type}");
            }
        }

        private void SkipWhiteSpace()
        {
            while ((_currentToken.type == TokenType.Whitespace || _currentToken.type == TokenType.NewLine || _currentToken.type == TokenType.Comment))
            {
                Eat(TokenType.Whitespace, TokenType.NewLine, TokenType.Comment);
            }
        }

        public AST Program()
        {
            // Program : (Function)* EOFs
            var programNode = new ProgramNode();
            while (_currentToken.type != TokenType.EOF)
            {
                programNode.AddFunction(Function() as FunctionNode);
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
            while (_currentToken.type != TokenType.CloseParenthesis)
            {
                parameters.Add(Variable() as Var);
                if (_currentToken.type == TokenType.Comma)
                {
                    Eat(TokenType.Comma);
                }
            }
            Eat(TokenType.CloseParenthesis);
            Eat(TokenType.OpenBrace);
            var nodes = new List<AST>();
            while (_currentToken.type != TokenType.CloseBrace)
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
            if (_currentToken.type == TokenType.Identifier)
            {
                return AssignmentStatement();
            }
            else if (_currentToken.type == TokenType.Return)
            {
                return ReturnStatement();
            }
            else if (_currentToken.type == TokenType.While)
            {
                return WhileStatement();
            }
            else if (_currentToken.type == TokenType.For)
            {
                return ForStatement();
            }
            else if (_currentToken.type == TokenType.Print)
            {
                return PrintStatement();
            }
            else if (_currentToken.type == TokenType.OpenBrace)
            {
                return CompoundStatement();
            }
            else
            {
                return Empty();
            }
        }

        public AST CompoundStatement()
        {
            // CompoundStatement : OPEN_BRACE (Statement)* CLOSE_BRACE
            Eat(TokenType.OpenBrace);
            var nodes = new List<AST>();
            while (_currentToken.type != TokenType.CloseBrace)
            {
                nodes.Add(Statement());
            }
            Eat(TokenType.CloseBrace);
            var root = new Compound();
            root.Children = nodes;
            return root;
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
            // ForStatement : FOR OPEN_PAREN AssignmentStatement SEMICOLON Expr SEMICOLON AssignmentStatement CLOSE_PAREN Statement
            Eat(TokenType.For);
            Eat(TokenType.OpenParenthesis);
            var initialization = AssignmentStatement();
            Eat(TokenType.Semicolon);
            var condition = Expr();
            Eat(TokenType.Semicolon);
            var increment = AssignmentStatement();
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

        public AST AssignmentStatement()
        {
            // AssignmentStatement : VARIABLE ASSIGN Expr SEMICOLON
            var left = Variable() as Var;
            var token = _currentToken;
            Eat(TokenType.Assign);
            var right = Expr();
            Eat(TokenType.Semicolon);
            return new Assign(left, token, right);
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
        public AST Expr()
        {
            // Expr : Comparison
            return Comparison();
        }

        public AST Comparison()
        {
            // Comparison : Additive ((EQUAL | NOT_EQUAL | LESS_THAN | GREATER_THAN) Additive)*
            var node = Additive();

            while (_currentToken.type == TokenType.Equal || _currentToken.type == TokenType.NotEqual || _currentToken.type == TokenType.LessThan || _currentToken.type == TokenType.GreaterThan)
            {
                var token = _currentToken;
                if (token.type == TokenType.Equal)
                {
                    Eat(TokenType.Equal);
                }
                else if (token.type == TokenType.NotEqual)
                {
                    Eat(TokenType.NotEqual);
                }
                else if (token.type == TokenType.LessThan)
                {
                    Eat(TokenType.LessThan);
                }
                else if (token.type == TokenType.GreaterThan)
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

            while (_currentToken.type == TokenType.Plus || _currentToken.type == TokenType.Minus)
            {
                var token = _currentToken;
                if (token.type == TokenType.Plus)
                {
                    Eat(TokenType.Plus);
                }
                else if (token.type == TokenType.Minus)
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

            while (_currentToken.type == TokenType.Multiply || _currentToken.type == TokenType.Divide)
            {
                var token = _currentToken;
                if (token.type == TokenType.Multiply)
                {
                    Eat(TokenType.Multiply);
                }
                else if (token.type == TokenType.Divide)
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
            //        | INTEGER
            //        | LPAREN Expr RPAREN
            //        | Variable
            //        | TRUE
            //        | FALSE
            var token = _currentToken;
            if (token.type == TokenType.Plus)
            {
                Eat(TokenType.Plus);
                return new UnaryOp(token, Factor());
            }
            else if (token.type == TokenType.Minus)
            {
                Eat(TokenType.Minus);
                return new UnaryOp(token, Factor());
            }
            else if (token.type == TokenType.Number)
            {
                Eat(TokenType.Number);
                return new Num(token);
            }
            else if (token.type == TokenType.OpenParenthesis)
            {
                Eat(TokenType.OpenParenthesis);
                var node = Expr();
                Eat(TokenType.CloseParenthesis);
                return node;
            }
            else if (token.type == TokenType.True)
            {
                Eat(TokenType.True);
                return new Bool(token);
            }
            else if (token.type == TokenType.False)
            {
                Eat(TokenType.False);
                return new Bool(token);
            }
            else
            {
                return Variable();
            }
        }
    }
}
