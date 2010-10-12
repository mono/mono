// Compiler options: -r:dtest-037-lib.dll

enum E
{
	Value = 9
}

class C
{
	public static int Main ()
	{
		var t = new External ();
		t.DynamicProperty = "test";
		string s = t.DynamicProperty;
		if (s != "test")
			return 1;
		
		t.Field = 's';
		if (t.Field != 's')
			return 2;
		
		if (t.Method (E.Value) != E.Value)
			return 3;
		
		dynamic d;
		t.MethodOut (out d);
		if (d != decimal.MaxValue)
			return 4;
		
		return 0;
	}
}