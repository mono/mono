// CS1616: Option `keyfile' overrides attribute `System.Reflection.AssemblyKeyFileAttribute' given in a source file or added module
// Line: 0
// Compiler options: -keyfile:CS1616.snk -warnaserror

using System.Reflection;

[assembly: AssemblyKeyFile("mono.snk")]

class C
{
    public static void Main () {}
}