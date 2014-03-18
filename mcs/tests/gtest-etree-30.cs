using System;
using System.Linq.Expressions;

public sealed class C
{
	public C ()
	{
	}

	public C (Action<object, object> tappedCallback)
	{
	}

	public readonly string TappedCallbackProperty = Create<C, Action<object, object>> (o => o.TappedCallback);

	public Action<object, object> TappedCallback {
		get;
		set;
	}

	public static string Create<T1, T2> (Expression<Func<T1, T2>> getter)
	{
		return null;
	}

	public static void Main ()
	{
		new C (null);
	}
}
