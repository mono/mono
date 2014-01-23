// Compiler options: -r:gtest-friend-00-lib.dll -keyfile:InternalsVisibleTest2.snk
using System;

public class Test
{
	public static void Main ()
	{
		FriendClass fc = new FriendClass ();
		
		// We should be able to access them
		int a = FriendClass.StaticFriendProperty;
		int b = fc.InstanceFriendProperty;
	}
}

