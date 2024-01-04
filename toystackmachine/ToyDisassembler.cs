using System.Text;

public class ToyDisassembler
{
    public static string Diassemble(int[] binary)
    {
        StringBuilder stringBuilder = new StringBuilder();
        int i = 0;
        int line = 0;
        while (i < binary.Length)
        {
            var opcode = (OpCode)binary[i++];
            stringBuilder.Append(i.ToString().PadLeft(3, ' '));
            stringBuilder.Append("\t");
            switch (opcode)
            {
                case OpCode.BRANCH:
                case OpCode.BRANCH_IF_ZERO:
                case OpCode.PUSH_IMMEDIATE:
                case OpCode.GET:
                case OpCode.SET:
                    stringBuilder.AppendFormat("{0} {1}{2}", opcode, binary[i++], Environment.NewLine);
                    break;
                case OpCode.CALL_HOST_FUNCTION:
                    {
                        var functionIndex = binary[i++];
                        var argsLength = binary[i++];
                        var args = new int[argsLength];
                        for (int j = 0; j < argsLength; j++)
                        {
                            args[j] = binary[i + j];
                        }
                        i += argsLength;
                        stringBuilder.AppendFormat("{0} {1} ({2}) {3}", opcode, functionIndex, string.Join(", ", args), Environment.NewLine);
                    }
                    break;
                default:
                    stringBuilder.AppendLine(opcode.ToString());
                    break;
            }
            line++;
        }
        return stringBuilder.ToString();
    }
}
