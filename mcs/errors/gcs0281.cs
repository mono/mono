// gcs0281.cs: Friend access was granted to `gcs0281, PublicKeyToken=43b5d2e9a794bdcb', but the output assembly is named `gcs0281, Version=0.0.0.0, Culture=neutral'. Try adding a reference to `gcs0281, PublicKeyToken=43b5d2e9a794bdcb' or change the output assembly name to match it
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

