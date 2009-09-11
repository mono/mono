using System;
using System.Dynamic;

public class MyObject : DynamicObject
{
	public static int Get, Invoke;

	public override bool TryGetMember (GetMemberBinder binder, out object result)
	{
		Console.WriteLine ("Get");
		Get++;
		result = null;
		return true;
	}

	public override bool TryInvokeMember (InvokeMemberBinder binder, object[] args, out object result)
	{
		Console.WriteLine ("Invoke");
		Invoke++;
		result = null;
		return true;
	}
}

public class Tests
{
	public static int Main ()
	{
		dynamic d = new MyObject ();

		var g = d.GetMe;
		if (MyObject.Get != 1 && MyObject.Invoke != 0)
			return 1;

		d.printf ("Hello, World!");
		if (MyObject.Get != 1 && MyObject.Invoke != 1)
			return 2;

		Console.WriteLine ("ok");
		return 0;
	}
}
