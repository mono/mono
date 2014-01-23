// Compiler options: -r:gtest-friend-00-lib.dll -keyfile:InternalsVisibleTest2.snk
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

