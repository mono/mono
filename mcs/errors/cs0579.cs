// CS0579: The attribute `System.Reflection.AssemblyKeyNameAttribute' cannot be applied multiple times
// Line : 

using System.Reflection;
using System.Runtime.CompilerServices;


[assembly: AssemblyKeyName("")]
[assembly: AssemblyKeyName("")]

public class Blah {

	public static void Main () { }
}
