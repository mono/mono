using System;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyKeyFile ("InternalsVisibleTest.snk")]
[assembly: InternalsVisibleTo ("cs0281, PublicKey=0024000004800000940000000602000000240000525341310004000011000000e39d99616f48cf7d6d59f345e485e713e89b8b1265a31b1a393e9894ee3fbddaf382dcaf4083dc31ee7a40a2a25c69c6d019fba9f37ec17fd680e4f6fe3b5305f71ae9e494e3501d92508c2e98ca1e22991a217aa8ce259c9882ffdfff4fbc6fa5e6660a8ff951cd94ed011e5633651b64e8f4522519b6ec84921ee22e4840e8")]

public class FriendClass
{
	internal static void MyMethod ()
	{
	}
}

