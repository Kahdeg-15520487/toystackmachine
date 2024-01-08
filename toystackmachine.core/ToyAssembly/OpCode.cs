namespace toystackmachine.core.ToyAssembly
{
    public enum OpCode
    {
        NOP,
        ADD,
        SUB,
        MUL,
        DIV,
        BRANCH,
        BRANCH_IF_NOT_ZERO,
        BRANCH_IF_ZERO,
        PUSH_IMMEDIATE,
        GET,
        SET,
        SETARRAY,
        DUP,
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
                case "push":
                    opcode = OpCode.PUSH_IMMEDIATE;
                    return true;
                case "get":
                    opcode = OpCode.GET;
                    return true;
                case "set":
                    opcode = OpCode.SET;
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
                case OpCode.CMP:
                    return "cmp";
                case OpCode.BRANCH:
                    return "br";
                case OpCode.BRANCH_IF_ZERO:
                    return "brzero";
                case OpCode.BRANCH_IF_NOT_ZERO:
                    return "brnzero";
                case OpCode.PUSH_IMMEDIATE:
                    return "push";
                case OpCode.GET:
                    return "get";
                case OpCode.SET:
                    return "set";
                case OpCode.SETARRAY:
                    return "setarray";
                case OpCode.DUP:
                    return "dup";
                case OpCode.TRIP:
                    return "trip";
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