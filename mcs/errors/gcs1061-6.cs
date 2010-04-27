// CS1061: Type `T' does not contain a definition for `Name' and no extension method `Name' of type `T' could be found (are you missing a using directive or an assembly reference?)
// Line: 11

using System;
using System.Collections.Generic;

public class C<T, U>
{
	public C (IEnumerable<T> t)
	{
		new List<T>(t).ConvertAll(p => p.Name);
	}
}
