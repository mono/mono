using System;
using System.Runtime.CompilerServices;

interface Iface
{
	[IndexerName ("AA")]
	bool this [int i] { set; }
}

public class MySubClass : Iface
{
	public static int Main ()
	{
		MySubClass m = new MySubClass ();
		m [1] = true;

		Iface i = new MySubClass ();
		i [1] = true;
		return 0;
	}

	[IndexerName ("BB")]
	public bool this [int i] { set { } }
}
