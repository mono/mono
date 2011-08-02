// Compiler options: -r:test-805-lib.dll

public class TestClass
{
	public static int Main ()
	{
		var a = new A ();
		var b = a.Test ();
		if (b.ReturnValue () != 5)
			return 1;

		a.Test2 (null);
		return 0;
	}
}

public class B
{
	internal int ReturnValue ()
	{
		return 5;
	}
}

public class C
{
}

public class G<T>
{
}
