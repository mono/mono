// CS0012: The type `A1' is defined in an assembly that is not referenced. Consider adding a reference to assembly `CS0012-lib-missing, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
// Line: 13
// Compiler options: -r:CS0012-lib.dll

using System.Threading.Tasks;

class Test
{
	public static void Main ()
	{
		var b = new B ();
		var t = Task.Factory.StartNew (() => {
			b.Test ();
			b.Test ();
		});

		b.Test ();
	}
}