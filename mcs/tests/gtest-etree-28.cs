// Compiler options: -warnaserror

using System;
using System.Linq.Expressions;

class Repro
{
	int i = 2;

	void UseField ()
	{
		TakeExpression (() => Console.Write (i));
	}

	void TakeExpression (Expression<Action> expr)
	{
	}

	public static void Main ()
	{
	}
}
