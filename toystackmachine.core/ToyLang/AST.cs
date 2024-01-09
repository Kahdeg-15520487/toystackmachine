using System;
using System.Collections.Generic;

namespace toystackmachine.core.ToyLang
{
    public abstract class AST
    {
    }

    public class Compound : AST
    {
        public List<AST> Children { get; set; }
    }

    public class Assign : AST
    {
        public Var Left { get; set; }
        public Token Token { get; set; }
        public AST Right { get; set; }

        public Assign(Var left, Token token, AST right)
        {
            Left = left;
            Token = token;
            Right = right;
        }
    }

    public class Var : AST
    {
        public Token Token { get; set; }

        public Var(Token token)
        {
            Token = token;
        }
    }

    public class NoOp : AST
    {
    }

    public class BinOp : AST
    {
        public AST Left { get; set; }
        public Token Token { get; set; }
        public AST Right { get; set; }

        public BinOp(AST left, Token token, AST right)
        {
            Left = left;
            Token = token;
            Right = right;
        }
    }

    public class UnaryOp : AST
    {
        public Token Token { get; set; }
        public AST Expr { get; set; }

        public UnaryOp(Token token, AST expr)
        {
            Token = token;
            Expr = expr;
        }
    }

    public class Num : AST
    {
        public Token Token { get; set; }

        public Num(Token token)
        {
            Token = token;
        }
    }

    public class FunctionNode : AST
    {
        public Token FunctionName { get; set; }
        public List<Var> Parameters { get; set; }
        public List<AST> Body { get; set; }

        public FunctionNode(Token functionName, List<Var> parameters, List<AST> body)
        {
            FunctionName = functionName;
            Parameters = parameters;
            Body = body;
        }
    }

    public class ReturnStatement : AST
    {
        public AST Expr { get; set; }

        public ReturnStatement(AST expr)
        {
            Expr = expr;
        }
    }

    public class WhileStatement : AST
    {
        public AST Condition { get; set; }
        public AST Statement { get; set; }

        public WhileStatement(AST condition, AST statement)
        {
            Condition = condition;
            Statement = statement;
        }
    }

    public class ForStatement : AST
    {
        public AST Initialization { get; set; }
        public AST Condition { get; set; }
        public AST Increment { get; set; }
        public AST Statement { get; set; }

        public ForStatement(AST initialization, AST condition, AST increment, AST statement)
        {
            Initialization = initialization;
            Condition = condition;
            Increment = increment;
            Statement = statement;
        }
    }

    public class PrintStatement : AST
    {
        public AST Expr { get; set; }

        public PrintStatement(AST expr)
        {
            Expr = expr;
        }
    }

    public class Bool : AST
    {
        public bool Value { get; set; }

        public Bool(Token token)
        {
            switch (token.type)
            {
                case TokenType.True:
                    Value = true;
                    break;
                case TokenType.False:
                    Value = false;
                    break;
                default:
                    throw new Exception($"Unexpected token {token}");

            }
        }
    }

    public class ProgramNode : AST
    {
        public List<FunctionNode> Functions { get; set; }

        public ProgramNode()
        {
            Functions = new List<FunctionNode>();
        }

        public void AddFunction(FunctionNode function)
        {
            Functions.Add(function);
        }
    }
}