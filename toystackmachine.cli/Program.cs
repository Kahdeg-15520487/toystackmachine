

testToyAssembler2();

//testToyAssembler();

//testToyLexer();

//testToyStackMachine();

static void testToyAssembler2()
{
    ToyLexer lexer = new ToyLexer(
@"
// declare machine's spec
#config memsize 2048
#config programstart 64
#config stackstart 512
#config stackmax 1024
#config screenstart 1024
#config screenwidth 32
#config screenheight 32

#hostfunction hostadd
#hostfunction hostinput
#hostfunction hostprint

callhost hostinput
set 700
callhost hostinput
set 701
get 700
get 701
add
print
halt
");

    ToyAssembler assembler = new ToyAssembler(lexer);
    var prog = assembler.Assemble();

    Console.WriteLine(ToyDisassembler.Diassemble(prog));

    ToyStackMachine vm = new ToyStackMachine(new ToyStackMachineMemoryConfiguration() { });

    vm.RegisterHostFuntion("hostadd", (m, a) => a.Sum());
    vm.RegisterHostFuntion("hostinput", (m, a) => { Console.Write("> "); return int.TryParse(Console.ReadLine(), out int res) ? res : 0; });
    vm.RegisterHostFuntion("hostprint", (m, a) =>
    {
        var s = new string(vm.GetArrayAt(a[0]).Select(i => (char)i).ToArray());
        Console.Write(s);
        return 0;
    });
    vm.LoadProgram(prog);
    vm.Run();
}


static void testToyAssembler()
{

    ToyLexer lexer = new ToyLexer(
@"
// declare machine's spec
#config memsize 2048
#config programstart 64
#config stackstart 512
#config stackmax 1024
#config screenstart 1024
#config screenwidth 32
#config screenheight 32

#hostfunction hostadd
#hostfunction hostexp
#hostfunction hostinput
#hostfunction hostprint

#data 900 ""count down from""

callhost hostprint 900
discard
callhost hostinput
push 1
add
set 700
loopstart:
get 700
push 1
sub
trip
set 700
print
brzero loopend
br loopstart
loopend:
halt
");

    ToyAssembler assembler = new ToyAssembler(lexer);
    var prog = assembler.Assemble();

    Console.WriteLine(ToyDisassembler.Diassemble(prog));

    ToyStackMachine vm = new ToyStackMachine(new ToyStackMachineMemoryConfiguration() { });

    vm.RegisterHostFuntion("hostadd", (m, a) => a.Sum());
    vm.RegisterHostFuntion("hostexp", (m, a) => (int)Math.Pow(a[0], a[1]));
    vm.RegisterHostFuntion("hostinput", (m, a) => { Console.Write("> "); return int.TryParse(Console.ReadLine(), out int res) ? res : 0; });
    vm.RegisterHostFuntion("hostprint", (m, a) =>
    {
        var s = new string(vm.GetArrayAt(a[0]).Select(i => (char)i).ToArray());
        Console.Write(s);
        return 0;
    });
    vm.LoadProgram(prog);
    vm.Run();
}

static void testToyLexer()
{
    ToyLexer lexer = new ToyLexer(
@"
// declare machine's spec
#memsize 2048
#programstart 64
#stackstart 512
#stackmax 1024
#screenstart 1024
#screenwidth 32
#screenheight 32

#hostfunction hostadd
#hostfunction hostexp
#hostfunction hostinput

#data 700 ""test""

hostadd 5 2 10
print

callhost hostinput
set 700
loopstart:
get 700
push 1
sub
trip
set 700
print
brzero loopend
br loopstart
loopend:
halt
");
    while (!lexer.IsEOF)
    {
        Console.WriteLine(lexer.NextToken());
    }
}

static void testToyStackMachine()
{
    ToyStackMachine vm = new ToyStackMachine(new ToyStackMachineMemoryConfiguration() { });
    ToyEmitter e = new ToyEmitter();

    vm.RegisterHostFuntion("hostadd", (m, a) => a.Sum());
    vm.RegisterHostFuntion("hostexp", (m, a) => (int)Math.Pow(a[0], a[1]));
    vm.RegisterHostFuntion("hostinput", (m, a) => int.TryParse(Console.ReadLine(), out int res) ? res : 0);

    e.AddDepedency("hostadd");
    e.AddDepedency("hostexp");
    e.AddDepedency("hostinput");

    e.EmitHostFunctionCall("hostadd", new int[] { 5, 2, 10 });
    e.Emit(OpCode.PRINT);
    e.EmitHostFunctionCall("hostexp", new int[] { 2, 10 });
    e.Emit(OpCode.PRINT);

    e.EmitHostFunctionCall("hostinput");    // callhost hostinput
    e.Emit(OpCode.SET, 700);                // pop and store at 700
    e.EmitLabel("loopstart");               // loopstart:
    e.Emit(OpCode.GET, 700);                // load from 700
    e.EmitPushImmediate(1);                 // push 1
    e.Emit(OpCode.SUB);                     // sub
    e.Emit(OpCode.TRIP);                    // trip
    e.Emit(OpCode.SET, 700);                // pop and store at 700
    e.Emit(OpCode.PRINT);                   // print
    e.Emit(OpCode.BRANCH_IF_ZERO, 2);       // brzero loopend
    e.EmitJump(OpCode.BRANCH, "loopstart"); // br loopstart
    e.EmitLabel("loopend");
    e.Emit(OpCode.HALT);

    vm.LoadProgram(e.Serialize());

    vm.Run();
}