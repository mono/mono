// Compiler options: -keyfile:key.snk -t:library
using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo ("gtest-friend-09, PublicKey=00240000048000009400000006020000002400005253413100040000110000007331bd7b6ac6f8db65c505462a9599dcf02ebd376eef7d99b6c4471c52a5a0f55eb33b24cd684c2a78ec06b216cfc0171da1927dc786b48fda8132f4afad465bae616d007f5b68e9779d901761d4d709494f65e5fe56df45492d26ead6541226b93d8b7f932b5fcad8b0c4f421401391bff163f23349276eca2b4c1805e8e2d3")]


[assembly: InternalsVisibleTo ("gtest-friend-10, PublicKey=00240000048000009400000006020000002400005253413100040000110000007331bd7b6ac6f8db65c505462a9599dcf02ebd376eef7d99b6c4471c52a5a0f55eb33b24cd684c2a78ec06b216cfc0171da1927dc786b48fda8132f4afad465bae616d007f5b68e9779d901761d4d709494f65e5fe56df45492d26ead6541226b93d8b7f932b5fcad8b0c4f421401391bff163f23349276eca2b4c1805e8e2d3")]

[assembly: InternalsVisibleTo ("gtest-friend-11, PublicKey=00240000048000009400000006020000002400005253413100040000110000007331bd7b6ac6f8db65c505462a9599dcf02ebd376eef7d99b6c4471c52a5a0f55eb33b24cd684c2a78ec06b216cfc0171da1927dc786b48fda8132f4afad465bae616d007f5b68e9779d901761d4d709494f65e5fe56df45492d26ead6541226b93d8b7f932b5fcad8b0c4f421401391bff163f23349276eca2b4c1805e8e2d3")]

[assembly: InternalsVisibleTo ("gtest-friend-12, PublicKey=00240000048000009400000006020000002400005253413100040000110000007331bd7b6ac6f8db65c505462a9599dcf02ebd376eef7d99b6c4471c52a5a0f55eb33b24cd684c2a78ec06b216cfc0171da1927dc786b48fda8132f4afad465bae616d007f5b68e9779d901761d4d709494f65e5fe56df45492d26ead6541226b93d8b7f932b5fcad8b0c4f421401391bff163f23349276eca2b4c1805e8e2d3")]

public class FriendClass
{
	// Static members
	
	internal static int StaticFriendField;
	
	internal static int StaticFriendProperty {
		get {
			return 1;
		}
	}

	internal static int StaticFriendMethod ()
	{
		return 2;
	}

	// Instance members
	
	internal int InstanceFriendField;
	
	internal int InstanceFriendProperty {
		get {
			return 1;
		}
	}

	internal int InstanceFriendMethod () 
	{
		return 2;
	}

	// Nested classes
	internal class NestedInternalClass
	{
	}

	protected internal class NestedProtectedInternalClass
	{
	}
}

//
// This is an internal class
//
class InternalFriendClass
{
}

