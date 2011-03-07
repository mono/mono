// Compiler options: -r:gtest-friend-13-lib.dll

using System;

public class B : FriendClass
{
	protected internal override void Test ()
	{
	}
	
	internal override void Test_2 ()
	{
		new FriendClass().Test ();
	}
}

public class Test
{
	static void Main ()
	{
		var b = new B ();
		b.Test_2 ();
	}
}

