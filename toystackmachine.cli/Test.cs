using toystackmachine.core.ToyAssembly;
using toystackmachine.core.ToyLang;

namespace toystackmachine.cli
{
    static class Test
    {
        public static void RunTest()
        {
            testtoyLangCompilerFor();

            //testToyLangCompilerWhile();

            //testToyLangCompilerIf();

            //testToyLangCompilerRecursive();

            //testToyLangCompiler3();

            //testToyLangCompiler2();

            //testToyAssemblerCallRet();

            //testToyLangCompiler();

            //testToyLangParser();

            //testToyLangLexer();

            //testToyAssembler2();

            //testToyAssembler();

            //testToyLexer();

            //testToyStackMachine();
        }

        static void testtoyLangCompilerFor()
        {
            string input = @"
function main(){
    var n = read;
    for(var i=0;i<n;i=i+1){
        print(i+1);
    }
}
";
            string asm = Compile(input);
            ToyProgram prog = Assemble(asm);
            Run(prog);
        }

        static void testToyLangCompilerWhile()
        {
            string input = @"
function main(){
    var n = read;
    while(n>0){
        print(n);
        n = n - 1;
    }
}
";
            string asm = Compile(input);
            ToyProgram prog = Assemble(asm);
            Run(prog);
        }

        static void testToyLangCompilerIf()
        {
            string input = @"
function main(){
    var a = read;
    var b = read;
    print(min(a,b));
}
function min(a,b){
    if(a<b){
        return a;
    }
    return b;
}
";
            string asm = Compile(input);
            ToyProgram prog = Assemble(asm);
            Run(prog);
        }

        static void testToyLangCompilerRecursive()
        {
            string input = @"
function main(){
    var n = read;
    print(factorial(n));
}
function factorial(n){
    if(n==0){
        return 1;
    }
    return n * factorial(n-1);
}
";
            string asm = Compile(input);
            ToyProgram prog = Assemble(asm);
            Run(prog);
        }

        private static ToyProgram Assemble(string asm)
        {
            ToyAssembler assembler = new ToyAssembler(new ToyAssemblyLexer(asm));
            var prog = assembler.Assemble();

            Console.WriteLine(ToyAssemblyDisassembler.Diassemble(prog));
            return prog;
        }

        private static string Compile(string input)
        {
            ToyLangCompiler compiler = new ToyLangCompiler();
            ToyLangParser parser = new ToyLangParser(new ToyLangLexer(input));
            var ast = parser.Program();
            var asm = compiler.Compile(ast, new ToyStackMachineMemoryConfiguration());
            Console.WriteLine("Compiled program:");
            Console.WriteLine(string.Join(Environment.NewLine, asm));
            Console.WriteLine("=====");
            return asm;
        }

        private static void Run(ToyProgram prog, string entryFunction = "main")
        {
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
            vm.Run(entryFunction);
        }

        static void testToyLangCompiler3()
        {
            string input = @"
function main(){
    var a = read;
    var n = read;
    print(add(a,n));
}
function add(a,b){
    return a+b;
}
";
            ToyLangCompiler compiler = new ToyLangCompiler();
            ToyLangParser parser = new ToyLangParser(new ToyLangLexer(input));
            var ast = parser.Program();
            var asm = compiler.Compile(ast, new ToyStackMachineMemoryConfiguration());
            Console.WriteLine("Compiled program:");
            Console.WriteLine(string.Join(Environment.NewLine, asm));
            Console.WriteLine("=====");

            ToyAssembler assembler = new ToyAssembler(new ToyAssemblyLexer(asm));
            var prog = assembler.Assemble();

            Console.WriteLine(ToyAssemblyDisassembler.Diassemble(prog));

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
            vm.Run("main");
        }

        static void testToyLangCompiler2()
        {
            string input = @"
function main(){
    var a = 5;
    var n = read;
    print(a+n);
}
";
            ToyLangCompiler compiler = new ToyLangCompiler();
            ToyLangParser parser = new ToyLangParser(new ToyLangLexer(input));
            var ast = parser.Program();
            var asm = compiler.Compile(ast, new ToyStackMachineMemoryConfiguration());
            Console.WriteLine("Compiled program:");
            Console.WriteLine(string.Join(Environment.NewLine, asm));
            Console.WriteLine("=====");

            ToyAssembler assembler = new ToyAssembler(new ToyAssemblyLexer(asm));
            var prog = assembler.Assemble();

            Console.WriteLine(ToyAssemblyDisassembler.Diassemble(prog));

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
            vm.Run("main");
        }

        static void testToyAssemblerCallRet()
        {
            string input = @"
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
call add1
print
halt

add1:
	add
	ret
";
            ToyAssembler assembler = new ToyAssembler(new ToyAssemblyLexer(input));
            var prog = assembler.Assemble();

            Console.WriteLine(ToyAssemblyDisassembler.Diassemble(prog));

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

        static void testToyLangCompiler()
        {
            string input = @"
function add(a, b) {
    return a + b;
}
function countDown(n) {
    while (n > 0) {
        print(n);
        n = n - 1;
    }
}
";
            ToyLangCompiler compiler = new ToyLangCompiler();
            ToyLangParser parser = new ToyLangParser(new ToyLangLexer(input));
            var ast = parser.Program();
            var prog = compiler.Compile(ast, new ToyStackMachineMemoryConfiguration());
            Console.WriteLine(string.Join(Environment.NewLine, prog));
        }

        static void testToyLangParser()
        {
            string input = @"
function add(a, b) {
    return a + b;
}
function countDown(n) {
    while (n > 0) {
        print(n);
        n = n - 1;
    }
}
";
            ToyLangLexer lexer = new ToyLangLexer(input);
            ToyLangParser parser = new ToyLangParser(lexer);
            var ast = parser.Program();
            Console.WriteLine(ast);

        }

        static void testToyLangLexer()
        {
            string input = @"
function add(a, b) {
    return a + b;
}
";
            ToyLangLexer lexer = new ToyLangLexer(input);
            while (!lexer.IsEOF)
            {
                Console.WriteLine(lexer.NextToken());
            }
        }

        static void testToyAssembler2()
        {
            ToyAssemblyLexer lexer = new ToyAssemblyLexer(
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

            Console.WriteLine(ToyAssemblyDisassembler.Diassemble(prog));

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

            ToyAssemblyLexer lexer = new ToyAssemblyLexer(
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

            Console.WriteLine(ToyAssemblyDisassembler.Diassemble(prog));

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
            ToyAssemblyLexer lexer = new ToyAssemblyLexer(
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
            e.EmitJump(OpCode.BRANCH_IF_ZERO, "loopend");       // brzero loopend
            e.EmitJump(OpCode.BRANCH, "loopstart"); // br loopstart
            e.EmitLabel("loopend");
            e.Emit(OpCode.HALT);

            vm.LoadProgram(e.Serialize());

            vm.Run();
        }
    }
}
