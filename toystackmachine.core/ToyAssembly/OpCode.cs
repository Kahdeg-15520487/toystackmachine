namespace toystackmachine.core.ToyAssembly
{
    public enum OpCode
    {
        NOP,

        ADD,
        SUB,
        MUL,
        DIV,
        MOD,
        LT,
        GT,
        EQ,
        NE,
        LE,
        GE,
        NOT,

        BRANCH,
        BRANCH_IF_NOT_ZERO,
        BRANCH_IF_ZERO,

        CALL,
        RET,

        PUSH_IMMEDIATE,
        GET,
        GETAT,
        GETARRAY,
        SET,
        SETAT,
        SETARRAY,

        DUP,
        SWAP,
        TRIP,
        DISCARD,

        PRINT,
        PRINT_ARRAY,
        CALL_HOST_FUNCTION,

        HALT,
        CMP
    }

    public static class OpCodeParser
    {
        public static bool ParseOpCode(string token, out OpCode opcode)
        {
            switch (token)
            {
                case "nop":
                    opcode = OpCode.NOP;
                    return true;
                case "add":
                    opcode = OpCode.ADD;
                    return true;
                case "sub":
                    opcode = OpCode.SUB;
                    return true;
                case "mul":
                    opcode = OpCode.MUL;
                    return true;
                case "div":
                    opcode = OpCode.DIV;
                    return true;
                case "mod":
                    opcode = OpCode.MOD;
                    return true;
                case "lt":
                    opcode = OpCode.LT;
                    return true;
                case "gt":
                    opcode = OpCode.GT;
                    return true;
                case "eq":
                    opcode = OpCode.EQ;
                    return true;
                case "ne":
                    opcode = OpCode.NE;
                    return true;
                case "le":
                    opcode = OpCode.LE;
                    return true;
                case "ge":
                    opcode = OpCode.GE;
                    return true;
                case "not":
                    opcode = OpCode.NOT;
                    return true;
                case "cmp":
                    opcode = OpCode.CMP;
                    return true;
                case "br":
                    opcode = OpCode.BRANCH;
                    return true;
                case "brzero":
                    opcode = OpCode.BRANCH_IF_ZERO;
                    return true;
                case "brnzero":
                    opcode = OpCode.BRANCH_IF_NOT_ZERO;
                    return true;
                case "call":
                    opcode = OpCode.CALL;
                    return true;
                case "ret":
                    opcode = OpCode.RET;
                    return true;
                case "push":
                    opcode = OpCode.PUSH_IMMEDIATE;
                    return true;
                case "get":
                    opcode = OpCode.GET;
                    return true;
                case "getat":
                    opcode = OpCode.GETAT;
                    return true;
                case "getarray":
                    opcode = OpCode.GETARRAY;
                    return true;
                case "set":
                    opcode = OpCode.SET;
                    return true;
                case "setat":
                    opcode = OpCode.SETAT;
                    return true;
                case "setarray":
                    opcode = OpCode.SETARRAY;
                    return true;
                case "dup":
                    opcode = OpCode.DUP;
                    return true;
                case "trip":
                    opcode = OpCode.TRIP;
                    return true;
                case "swap":
                    opcode = OpCode.SWAP;
                    return true;
                case "discard":
                    opcode = OpCode.DISCARD;
                    return true;
                case "print":
                    opcode = OpCode.PRINT;
                    return true;
                case "printarray":
                    opcode = OpCode.PRINT_ARRAY;
                    return true;
                case "callhost":
                    opcode = OpCode.CALL_HOST_FUNCTION;
                    return true;
                case "halt":
                    opcode = OpCode.HALT;
                    return true;
                default:
                    opcode = OpCode.NOP;
                    return false;
            }
        }

        public static string ToString(OpCode opcode)
        {
            switch (opcode)
            {
                case OpCode.NOP:
                    return "nop";
                case OpCode.ADD:
                    return "add";
                case OpCode.SUB:
                    return "sub";
                case OpCode.MUL:
                    return "mul";
                case OpCode.DIV:
                    return "div";
                case OpCode.MOD:
                    return "mod";
                case OpCode.LT:
                    return "lt";
                case OpCode.GT:
                    return "gt";
                case OpCode.EQ:
                    return "eq";
                case OpCode.NE:
                    return "ne";
                case OpCode.LE:
                    return "le";
                case OpCode.GE:
                    return "ge";
                case OpCode.NOT:
                    return "not";
                case OpCode.CMP:
                    return "cmp";
                case OpCode.BRANCH:
                    return "br";
                case OpCode.BRANCH_IF_ZERO:
                    return "brzero";
                case OpCode.BRANCH_IF_NOT_ZERO:
                    return "brnzero";
                case OpCode.CALL:
                    return "call";
                case OpCode.RET:
                    return "ret";
                case OpCode.PUSH_IMMEDIATE:
                    return "push";
                case OpCode.GET:
                    return "get";
                case OpCode.GETAT:
                    return "getat";
                case OpCode.GETARRAY:
                    return "getarray";
                case OpCode.SET:
                    return "set";
                case OpCode.SETAT:
                    return "setat";
                case OpCode.SETARRAY:
                    return "setarray";
                case OpCode.DUP:
                    return "dup";
                case OpCode.TRIP:
                    return "trip";
                case OpCode.SWAP:
                    return "swap";
                case OpCode.DISCARD:
                    return "discard";
                case OpCode.PRINT:
                    return "print";
                case OpCode.PRINT_ARRAY:
                    return "printarray";
                case OpCode.CALL_HOST_FUNCTION:
                    return "callhost";
                case OpCode.HALT:
                    return "halt";
                default:
                    return "nop";
            }
        }
    }
}