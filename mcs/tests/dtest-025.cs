interface I
{
	void SetValue (int arg);
}

public struct S : I
{
	public int Value;

	public void SetValue (int v)
	{
		Value = v;
	}
}

class C
{
	static void Method<T> (ref T t) where T : struct, I
	{
		dynamic d = 25;
		t.SetValue (d);
	}
		
	public static int Main ()
	{
		int? x = null;
		dynamic y = 50;
		int v = x.GetValueOrDefault(y);
		if (v != 50)
			return 1;
		
		var s = new S ();
		dynamic d = 5;

		s.SetValue (d);
		if (s.Value != 5)
			return 2;
		
		Method (ref s);
		if (s.Value != 25)
			return 3;
		
		return 0;
	}
}
