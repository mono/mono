// Compiler options: -t:library
using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo ("gtest-friend-05")]
[assembly: InternalsVisibleTo ("gtest-friend-06")]
[assembly: InternalsVisibleTo ("gtest-friend-07")]
[assembly: InternalsVisibleTo ("gtest-friend-08")]

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

