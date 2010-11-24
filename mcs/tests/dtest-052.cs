// Compiler options: -r:dtest-052-lib.dll

// Importing of complex dynamic arguments

class A
{
	public void Method (DynamicReference d)
	{
		d.DynType.Value.AnyMethod ();
		d.DynArray.Value[0][0].AnyMethod ();
	}
	
	public static void Main ()
	{
	}
}
