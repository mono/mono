using System;

class X
{
	public static void Main ()
	{
		string s = null;

		_ = 1;
		{
			char _ = '4';
		}

		_ = TestValue ();

		_ = _ = s;

		byte k1;
		var s1 = (k1, _) = (1, s);

		Func<object> l1 = () => _ = (_, _) = (1, s);

		TryGetValue (out _);
	}

	static bool TryGetValue (out int arg)
	{
		arg = 3;
		return true;
	}

	static int TestValue ()
	{
		return 4;
	}
}