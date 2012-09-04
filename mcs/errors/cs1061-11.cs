// CS1061: Type `object' does not contain a definition for `Foo' and no extension method `Foo' of type `object' could be found. Are you missing an assembly reference?
// Line: 12

using System.Collections.Generic;

public class C
{
	void M (IEnumerable<KeyValuePair<string, dynamic>> arg)
	{
		foreach (KeyValuePair<string, object> o in arg)
		{
			o.Value.Foo ();
		}
	}
}
