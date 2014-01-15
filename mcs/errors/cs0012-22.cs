// CS0012: The type `Base.IQueryExpr`1<System.Collections.Generic.IEnumerable<double>>' is defined in an assembly that is not referenced. Consider adding a reference to assembly `CS0012-lib-missing, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
// Line: 9
// Compiler options: -r:CS0012-22-lib.dll

public class C
{
	public static void Main ()
	{
		B.Sum (null);
	}
}