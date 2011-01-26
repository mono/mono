// CS0266: Cannot implicitly convert type `System.Collections.Generic.IList<dynamic>' to `string[]'. An explicit conversion exists (are you missing a cast?)
// Line: 10
// Compiler options: -r:DCS0266-lib.dll -noconfig

public class C
{
	public static void Main ()
	{
		var t = new Test ();
		string[] s = t.DynField;
	}
}
