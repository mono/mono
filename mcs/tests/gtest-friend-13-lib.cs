// Compiler options: -t:library
using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo ("gtest-friend-13")]

public class FriendClass
{
	protected internal virtual void Test ()
	{
	}
	
	internal virtual void Test_2 ()
	{
	}
}

