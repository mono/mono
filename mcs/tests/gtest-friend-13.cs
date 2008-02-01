// Compiler options: -r:gtest-friend-13-lib.dll

using System;

public class B : FriendClass
{
	protected internal override void Test ()
	{
	}
	
	internal override void Test_2 ()
	{
	}	
}

public class Test
{
	static void Main ()
	{
	}
}

