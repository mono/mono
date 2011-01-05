// CS1624: The body of `C.Test<TSource>(System.Collections.Generic.IEnumerable<TSource>)' cannot be an iterator block because `TSource' is not an iterator interface type
// Line: 8

using System.Collections.Generic;

public class C
{
	public static TSource Test<TSource>(IEnumerable<TSource> source)
	{
		foreach (TSource element in source)
			yield return element;
	}
}
