using System.Collections.Generic;

class Expr
{
	public int Field;
	public Expr Next;
}

static class X
{
	public static IEnumerable<int> Test (this Expr expr)
	{
		var exprCur = expr;
		while (exprCur != null)
		{
			if (exprCur is Expr list)
			{
				yield return list.Field;
				exprCur = list.Next;
			}
			else
			{
				yield return 2;
				yield break;
			}
		}
	}

	public static void Main ()
	{
	}
}