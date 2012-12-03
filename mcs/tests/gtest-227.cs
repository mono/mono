using System;
using System.Runtime.CompilerServices;

/* GenDebugConstr.cs
 * Simple test case for gmcs issue (should compile).
 * Bryan Silverthorn <bsilvert@cs.utexas.edu>
 */

public interface Indexed
{
	[IndexerName("Foo")]
	int this [int ix] {
		get;
	}
}

public class Foo<G>
	where G : Indexed
{
	public static void Bar()
	{
		int i = default(G) [0];
	}
}

class X
{
	public static void Main ()
	{ }
}
