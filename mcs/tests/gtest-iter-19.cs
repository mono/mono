using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public static class IEnumerableTransform
{

	public static IEnumerable<TOut> Transform<TOut> (this IEnumerable<int> input, EmitterFunc<TOut> rule)
	{
		foreach (var v in input) {
			TOut output;
			rule (out output);
			yield return output;
		}
	}

	public static EmitterFunc<TOut> Emit<TOut> (TOut result)
	{
		return delegate (out TOut output) {
			output = result;
		};
	}

	public delegate void EmitterFunc<TOut> (out TOut output);

	public static int Main ()
	{
		IEnumerable<int> arr = new int[3];
		if (!arr.Transform<char> (IEnumerableTransform.Emit<char> ('t')).SequenceEqual(new char[] { 't', 't', 't'}))
			return 1;

		return 0;
	}
}
