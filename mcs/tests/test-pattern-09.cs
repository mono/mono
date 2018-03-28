using System.Collections.Generic;

class Expr
{
	public int Field;
}

static class X
{
	public static IEnumerable<int> Test (Expr expr)
	{
		object exprCur = expr;
		if (exprCur is Expr list) {
			yield return list.Field;
		}
	}

	public static IEnumerable<string> Test2 (int? expr)
	{
		int? exprCur = expr;
		while (exprCur != null) {
			if (exprCur is int list) {
				yield return list.ToString ();
			}
		}
	}	

	public static void Main ()
	{
		Test (null);
		Test2 (3);
	}
}