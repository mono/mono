// CS0579: The attribute `System.Reflection.AssemblyKeyNameAttribute' cannot be applied multiple times
// Line : 7
// Compiler options: CS0579-4-1.cs

using System.Reflection;

[assembly: AssemblyKeyName("")]

public class Blah {
	public static void Main () { }
}

