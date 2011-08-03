// CS0160: A previous catch clause already catches all exceptions of this or a super type `C<dynamic>'
// Line: 17

class D<T> : C<object>
{
}

class C<T> : System.Exception
{
}

class ClassMain
{
	public static void Main ()
	{
		try { }
		catch (C<dynamic>) { }
		catch (D<object>) { }
	}
}
