// cs0579.cs : Duplicate 'AssemblyKeyName' attribute
// Line : 

using System.Reflection;
using System.Runtime.CompilerServices;


[assembly: AssemblyKeyName("")]
[assembly: AssemblyKeyName("")]

public class Blah {

	public static void Main () { }
}
