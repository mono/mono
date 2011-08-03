// CS0182: An attribute argument must be a constant expression, typeof expression or array creation expression
// Line: 8

using System.Runtime.CompilerServices;

public class C
{
	[IndexerName ("1" + 2)]
	public string this [int i] {
		set { }
	}
}
