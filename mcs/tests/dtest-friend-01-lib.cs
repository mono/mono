// Compiler options: -t:library

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("dtest-friend-01")]

public class A
{
	internal void Test ()
	{
	}
}