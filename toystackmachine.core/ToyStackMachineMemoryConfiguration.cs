﻿public class ToyStackMachineMemoryConfiguration
{
    public readonly int MemorySize = 2048;
    public readonly int ProgramStart = 64;
    public readonly int StackStart = 512;
    public readonly int StackMax = 1024;

    public ToyStackMachineMemoryConfiguration(int memorySize = 2048, int programStart = 64, int stackStart = 512, int stackMax = 1024)
    {
        MemorySize = memorySize;
        ProgramStart = programStart;
        StackStart = stackStart;
        StackMax = stackMax;
    }
}
