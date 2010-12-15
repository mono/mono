using System;
using System.Linq.Expressions;

public static class InferArrayType
{
	public static void foo (Func<Expression, bool>[] args)
	{
	}

	public static void bar (Action<Expression> seq, Func<Expression, bool> action)
	{
		foo (new[] { p => { seq (p); return true; }, action });
	}

	public static void Main ()
	{
	}
}
