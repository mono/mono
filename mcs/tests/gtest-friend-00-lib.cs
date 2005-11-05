// Compiler options: -keyfile:InternalsVisibleTest.snk -t:library
using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo ("gtest-friend-01, PublicKeyToken=818a09ab19e745bf")]
[assembly: InternalsVisibleTo ("gtest-friend-02, PublicKeyToken=818a09ab19e745bf")]
[assembly: InternalsVisibleTo ("gtest-friend-03, PublicKeyToken=818a09ab19e745bf")]
[assembly: InternalsVisibleTo ("gtest-friend-04, PublicKeyToken=818a09ab19e745bf")]

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

