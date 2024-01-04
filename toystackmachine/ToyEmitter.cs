public class ToyEmitter
{
    private readonly List<int> program;
    private readonly Dictionary<string, int> labels;
    private readonly Dictionary<string, List<int>> unpatchedLabels;
    private readonly List<string> dependency;

    public ToyEmitter()
    {
        this.program = new List<int>();
        this.dependency = new List<string>();
        this.labels = new Dictionary<string, int>();
        this.unpatchedLabels = new Dictionary<string, List<int>>();
    }

    public void AddDepedency(string hostFunctionName)
    {
        dependency.Add(hostFunctionName);
    }

    public void Emit(OpCode opcode)
    {
        program.Add((int)opcode);
    }

    public void Emit(OpCode opcode, int operand)
    {
        program.Add((int)opcode);
        program.Add(operand);
    }

    public void EmitPushImmediate(int imm)
    {
        program.Add((int)OpCode.PUSH_IMMEDIATE);
        program.Add(imm);
    }

    public void EmitLabel(string label)
    {
        labels.Add(label, program.Count);

        if (unpatchedLabels.ContainsKey(label))
        {
            foreach (var unpatchedJump in unpatchedLabels[label])
            {
                program[unpatchedJump] = program.Count - 1 - unpatchedJump;
            }
            unpatchedLabels.Remove(label);
        }
    }
    public void UpdateLabel(string label)
    {
        labels[label] = program.Count;
    }

    public void EmitJump(OpCode opcode, string label)
    {
        if (opcode != OpCode.BRANCH
         && opcode != OpCode.BRANCH_IF_ZERO)
        {
            throw new InvalidOperationException($"Expect <{OpCode.BRANCH}|{OpCode.BRANCH_IF_ZERO}>, got {opcode}");
        }

        if (!labels.ContainsKey(label))
        {
            program.Add((int)opcode);
            if (unpatchedLabels.ContainsKey(label))
            {
                unpatchedLabels[label].Add(program.Count);
            }
            else
            {
                unpatchedLabels[label] = new List<int>() { program.Count };
            }
            program.Add(-1);
            return;
        }

        program.Add((int)opcode);
        program.Add(labels[label] - 1 - program.Count);
    }

    public void EmitHostFunctionCall(string hostFunctionName, params int[] args)
    {
        if (dependency.Contains(hostFunctionName))
        {
            program.Add((int)OpCode.CALL_HOST_FUNCTION);
            program.Add(dependency.IndexOf(hostFunctionName));
            program.Add(args.Length);
            program.AddRange(args);
        }
        else
        {
            throw new KeyNotFoundException($"Unregistered depedency: {hostFunctionName}");
        }
    }

    public void EmitRaw(params int[] array)
    {
        program.AddRange(array);
    }

    public (int[] binary, string[] dependency) Serialize()
    {
        Emit(OpCode.HALT);
        return (program.ToArray(), dependency.ToArray());
    }
}
