// CS0281: Friend access was granted to `cs0281, PublicKeyToken=27576a8182a18822', but the output assembly is named `cs0281, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'. Try adding a reference to `cs0281, PublicKeyToken=27576a8182a18822' or change the output assembly name to match it
// Line: 0
// Compiler options: -r:CSFriendAssembly-lib.dll

using System;

public class Test
{
	static void Main ()
	{
		FriendClass.MyMethod ();
	}
}

