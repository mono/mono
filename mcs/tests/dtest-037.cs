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

		t.FieldArray = new dynamic [2,2];
		t.FieldArray [1,1] = 'b';
		if (t.FieldArray[1,1] != 'b')
			return 21;
		
		if (t.Method (E.Value) != E.Value)
			return 3;
		
		dynamic d;
		t.MethodOut (out d);
		if (d != decimal.MaxValue)
			return 4;

		I<dynamic>[] r = t.Method2 (1);
		int res = r [0].Value;
		r = t.Method3 (null);
		
		CI<dynamic> ci2 = new CI2 ();
		ci2.Value = 'v';
		if (ci2.Value != 'v')
			return 5;
		
		return 0;
	}
}