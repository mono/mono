using System.Dynamic;

public class TestConvert : DynamicObject
{
	public override bool TryConvert (ConvertBinder binder, out object result)
	{
		result = null;
		return true;
	}
}

public class Test : DynamicObject
{
	public override bool TryInvokeMember (InvokeMemberBinder binder, object [] args, out object result)
	{
		result = new TestConvert ();
		return true;
	}
}

public class XX
{
	public static void Main ()
	{
		dynamic t = new Test ();
		string result = t.SomeMethod ();
	}
}