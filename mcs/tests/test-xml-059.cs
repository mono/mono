// Compiler options: -doc:xml-059.xml

using System.Collections.Generic;

// <see cref="int?" /> - this is invalid 1584/1658
// <see cref="List" />
/// <see cref="M:System.Web.Services.Protocols.SoapHttpClientProtocol.Invoke2 ( )" />
/// <see cref="Bar" />
/// <see cref="ListBase(string)" />
/// <see cref="T:ListBase(string)" />
/// <see cref="T:ListBase&lt;string)" /><!-- it somehow passes -->
/// <see cref="T:List!$%Base()" /><!-- it somehow passes -->
/// <see cref="T:$%!" />
/// <see cref=".:Bar" />
/// <see cref="T:List(int)" />
public class Foo
{
	static void Main ()
	{
	}

	/// hogehoge
	public string Bar;

	/// fugafuga
	public void ListBase (string s)
	{
	}
}

// <see cref="System.Nullable&lt;System.Int32&gt;" /> - cs1658/1574
/// <see cref="T:System.Nullable&lt;System.Int32&gt;" />
/// <see cref="T:System.Nullable(System.Int32)" />
public class ListBase<T>
{
}
