// Compiler options: -r:gtest-friend-02-lib.dll -keyfile:InternalsVisibleTest2.snk
using System;

public class Test
{
	public static void Main ()
	{
		FriendClass fc = new FriendClass ();
		
		// We should be able to access them
		FriendClass.StaticFriendMethod ();
		fc.InstanceFriendMethod ();
	}
}

