using System;
using System.Reflection;

public sealed class FooAttribute : Attribute
{
	public FooAttribute (int i) {
	}

	public int AProperty {
		get {
			return 0;
		}
		set {
		}
	}
}

[Foo (5, AProperty=6)]
class Program {
	public static int Main () {
		var attrs = typeof (Program).GetCustomAttributesData ();
		if (attrs.Count != 1)
			return 1;
		return 0;
	}
}
