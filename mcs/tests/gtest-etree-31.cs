using System;
using System.Linq.Expressions;

class X
{
	class HasAction
	{
		public void Start ()
		{
		}
	}

	public static int Main ()
	{
		var expectedObject = typeof (HasAction).GetMethod("Start");

		Expression<Func<HasAction, Action>> methodToUse = r => r.Start;
		
		UnaryExpression unary = methodToUse.Body as UnaryExpression;
		MethodCallExpression methodCall = unary.Operand as MethodCallExpression;
		ConstantExpression constantExpression = methodCall.Object as ConstantExpression;

		if (expectedObject != constantExpression.Value)
			return 1;

		if (methodCall.Object == null)
			return 2;

		return 0;
	}
}