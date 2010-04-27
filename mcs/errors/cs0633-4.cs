// CS0633: The argument to the `System.Runtime.CompilerServices.IndexerNameAttribute' attribute must be a valid identifier
// Line: 8

using System.Runtime.CompilerServices;

public class C
{
	[IndexerName ("class")]
	public string this [int i] {
		set { }
	}
}
