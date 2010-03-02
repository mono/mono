// Compiler options: -r:test-741-lib.dll

class Test
{
	void test ()
	{
		IFoo f = null;
		int v = f.Prop;
		f.NestedProp = 4;		
	}
	
	public static void Main ()
	{
    }
}
