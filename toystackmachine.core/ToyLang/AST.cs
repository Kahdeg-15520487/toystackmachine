using System;
using System.Collections.Generic;

namespace toystackmachine.core.ToyLang
{
    public abstract class AST
    {
    }

    public class CompoundStatement : AST
    {
        public List<AST> Children { get; set; }
    }

    public class Assign : AST
    {
        public AST Left { get; set; }
        public Token Token { get; set; }
        public AST Right { get; set; }

        public Assign(AST left, Token token, AST right)
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
        public int Value { get; set; }

        public Num(Token token)
        {
            Token = token;
            Value = int.Parse(token.Value);
        }
    }

    public class LiteralString : AST
    {
        public Token Token { get; set; }

        public LiteralString(Token token)
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
            switch (token.Type)
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
        public List<VariableDeclareStatement> GlobalVars { get; set; }
        public List<FunctionNode> Functions { get; set; }

        public ProgramNode()
        {
            GlobalVars = new List<VariableDeclareStatement>();
            Functions = new List<FunctionNode>();
        }

        public void AddFunction(FunctionNode function)
        {
            Functions.Add(function);
        }

        internal void AddGlobalVariable(VariableDeclareStatement var)
        {
            GlobalVars.Add(var);
        }
    }

    public class VariableDeclareStatement : AST
    {
        public Var Variable { get; set; }
        public AST Initializer { get; set; }

        public VariableDeclareStatement(Var variable, AST right)
        {
            Variable = variable;
            Initializer = right;
        }
    }

    public class FunctionCallExpression : AST
    {
        public Token FunctionName { get; set; }
        public List<AST> Arguments { get; set; }

        public FunctionCallExpression(Token functionName)
        {
            FunctionName = functionName;
            Arguments = new List<AST>();
        }

        public void AddArgument(AST argument)
        {
            Arguments.Add(argument);
        }
    }

    public class ReadExpression : AST { }
    public class SizeOfExpression : AST
    {
        public Var v { get; set; }

        public SizeOfExpression(Var v)
        {
            this.v = v;
        }
    }

    public class IfStatement : AST
    {
        public AST Condition { get; set; }
        public AST TrueStatement { get; set; }
        public AST FalseStatement { get; set; }

        public IfStatement(AST condition, AST trueStatement, AST falseStatement)
        {
            Condition = condition;
            TrueStatement = trueStatement;
            FalseStatement = falseStatement;
        }
    }
    public class ArrayAccessExpression : AST
    {
        public Var Array { get; set; }
        public AST Index { get; set; }

        public ArrayAccessExpression(Var array, AST index)
        {
            Array = array;
            Index = index;
        }
    }

    public class ArrayInitializerExpression : AST
    {
        public List<AST> Elements { get; set; }

        public ArrayInitializerExpression(List<AST> elements)
        {
            Elements = elements;
        }
    }

    public class ExpressionStatement : AST
    {
        public AST Expr { get; set; }

        public ExpressionStatement(AST expr)
        {
            Expr = expr;
        }
    }
}
