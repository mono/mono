// cs0579.cs : Duplicate 'AssemblyKeyName' attribute
// Line : 7
// Compiler options: CS0579-4-1.cs

using System.Reflection;

[assembly: AssemblyKeyName("")]

public class Blah {
	public static void Main () { }
}

