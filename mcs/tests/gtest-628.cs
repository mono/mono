using System;
using System.Threading;

class Impl<T> where T : class
{
	public static object CompareExchangeImpl (TypedReference tr, object value, object comparand)
	{
		return Interlocked.CompareExchange (ref __refvalue(tr, T), (T) value, (T) comparand);
	}
}

class X
{
	public static void Main ()
	{
		var obj = "obj";
		var tr = __makeref (obj);
		Impl<string>.CompareExchangeImpl (tr, "foo", null);
	}
}