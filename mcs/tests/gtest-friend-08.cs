// Compiler options: -r:gtest-friend-01-lib.dll
using System;

public class Test
{
	public static void Main ()
	{
		// We should be able to access them
		new InternalFriendClass ();
		new FriendClass.NestedInternalClass ();
		new FriendClass.NestedProtectedInternalClass ();
	}
}

