using System;
using System.Linq;
using System.Text;

namespace toystackmachine.core.ToyAssembly
{
    public static class ToyAssemblyDisassembler
    {
        public static string Diassemble(ToyProgram program)
        {
            StringBuilder sb = new StringBuilder();
            int i = 0;
            int line = 0;
            var binary = program.ROM;
            var labels = program.Labels;
            var dependency = program.Dependency;


            sb.AppendLine("Depedency:");
            foreach (var dep in dependency)
            {
                sb.Append("  ");
                sb.AppendLine(dep);
            }
            sb.AppendLine("Constants:");
            foreach (var constant in program.Constants.Forward)
            {
                sb.AppendLine($"  {constant.Key} -> {constant.Value}");
            }
            sb.AppendLine("Labels:");
            foreach (var label in labels.ToList())
            {
                sb.AppendLine($"  {label.Key} -> {label.Value}");
            }
            sb.AppendLine("ROM:");
            while (i < program.ROM.Length)
            {
                var opcode = (OpCode)binary[i];
                sb.Append(i.ToString().PadLeft(3, ' '));
                sb.Append("\t");
                if (labels.Reverse.ContainsKey(i))
                {
                    sb.Append(labels.Reverse[i]);
                    sb.Append(":");
                    sb.AppendLine();
                    sb.Append("\t");
                }
                i++;

                switch (opcode)
                {
                    case OpCode.BRANCH:
                    case OpCode.BRANCH_IF_ZERO:
                    case OpCode.BRANCH_IF_NOT_ZERO:
                    case OpCode.CALL:
                        sb.AppendFormat("{0} {1}{2}", OpCodeParser.ToString(opcode), labels.Reverse[binary[i++]], Environment.NewLine);
                        break;
                    case OpCode.PUSH_IMMEDIATE:
                    case OpCode.GET:
                    case OpCode.SET:
                        sb.AppendFormat("{0} {1}{2}", OpCodeParser.ToString(opcode), binary[i++], Environment.NewLine);
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
                            sb.AppendFormat("{0} {1} ({2}) {3}", OpCodeParser.ToString(opcode), dependency[functionIndex], string.Join(", ", args), Environment.NewLine);
                        }
                        break;
                    default:
                        sb.AppendLine(OpCodeParser.ToString(opcode));
                        break;
                }
                line++;
            }
            return sb.ToString();
        }
    }
}