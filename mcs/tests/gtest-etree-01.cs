using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

class Tester
{
	static void AssertNodeType (LambdaExpression e, ExpressionType et)
	{
		if (e.Body.NodeType != et)
			throw new ApplicationException (e.Body.NodeType + " != " + et);
	}

	static void Assert<T> (T expected, T value)
	{
		if (!EqualityComparer<T>.Default.Equals (expected, value))
			throw new ApplicationException (expected + " != " + value);
	}

	void AddTest ()
	{
		Expression<Func<int, int, int>> e = (int a, int b) => a + b;
		AssertNodeType (e, ExpressionType.Add);
		Assert (50, e.Compile ().Invoke (20, 30));

		Expression<Func<int?, int?, int?>> e2 = (a, b) => a + b;
		AssertNodeType (e2, ExpressionType.Add);
		Assert (null, e2.Compile ().Invoke (null, 3));
	}

	void AddCheckedTest ()
	{
		checked {
			Expression<Func<int, int, int>> e = (int a, int b) => a + b;

			AssertNodeType (e, ExpressionType.AddChecked);
			Assert (-10, e.Compile ().Invoke (20, -30));
		}
	}

	void AndTest ()
	{
		Expression<Func<bool, bool, bool>> e = (bool a, bool b) => a & b;

		AssertNodeType (e, ExpressionType.And);
		Assert (false, e.Compile ().Invoke (false, true));
	}

	void AndAlsoTest ()
	{
		Expression<Func<bool, bool, bool>> e = (bool a, bool b) => a && b;

		AssertNodeType (e, ExpressionType.AndAlso);
		Assert (false, e.Compile ().Invoke (true, false));
	}

	void ArrayIndexTest ()
	{
		Expression<Func<string[], long, string>> e = (string[] a, long i) => a [i];
		AssertNodeType (e, ExpressionType.ArrayIndex);
		Assert ("b", e.Compile ().Invoke (new string [] { "a", "b", "c" }, 1));

		Expression<Func<string [], string>> e2 = (string [] a) => a [0];
		AssertNodeType (e2, ExpressionType.ArrayIndex);
		Assert ("a", e2.Compile ().Invoke (new string [] { "a", "b" }));

		Expression<Func<object [,], int, int, object>> e3 = (object [,] a, int i, int j) => a [i, j];
		AssertNodeType (e3, ExpressionType.Call);
		
		Assert ("z", e3.Compile ().Invoke (
			new object [,] { { 1, 2 }, { "x", "z" } }, 1, 1));

		Expression<Func<decimal [][], byte, decimal>> e4 = (decimal [][] a, byte b) => a [b][1];
		AssertNodeType (e4, ExpressionType.ArrayIndex);

		decimal [] [] array = { new decimal [] { 1, 9 }, new decimal [] { 10, 90 } };
		Assert (90, e4.Compile ().Invoke (array, 1));
	}
	
	void ArrayLengthTest ()
	{
		int o = new int [0].Length;

		Expression<Func<double [], int>> e = (double [] a) => a.Length;
		AssertNodeType (e, ExpressionType.ArrayLength);
		Assert (0, e.Compile ().Invoke (new double [0]));
		Assert (9, e.Compile ().Invoke (new double [9]));

		// TODO: implement
		//Expression<Func<string [,], int>> e2 = (string [,] a) => a.Length;
		//AssertNodeType (e2, ExpressionType.MemberAccess);
		//Assert (0, e2.Compile ().Invoke (new string [0, 0]));
	}	
	
	void CallTest ()
	{
		Expression<Func<int, int>> e = (int a) => Math.Max (a, 5);
		AssertNodeType (e, ExpressionType.Call);
		Assert (5, e.Compile ().Invoke (2));
		Assert (9, e.Compile ().Invoke (9));
		
		Expression<Func<string, string>> e2 = (string a) => InstanceMethod (a);
		AssertNodeType (e2, ExpressionType.Call);
		Assert ("abc", e2.Compile ().Invoke ("abc"));
/*
		// TODO: implement
		Expression<Func<int, string, int, object>> e3 = (int index, string a, int b) => InstanceParamsMethod (index, a, b);
		AssertNodeType (e3, ExpressionType.Call);
		Assert<object> (4, e3.Compile ().Invoke (1, "a", 4));

		Expression<Func<object>> e4 = () => InstanceParamsMethod (0);
		AssertNodeType (e4, ExpressionType.Call);
		Assert<object> ("<empty>", e4.Compile ().Invoke ());
*/

		Expression<Func<int, int>> e5 = (int a) => GenericMethod (a);
		AssertNodeType (e5, ExpressionType.Call);
		Assert (5, e5.Compile ().Invoke (5));
	}
	
	
	//
	// Test helpers
	//
	string InstanceMethod (string arg)
	{
		return arg;
	}
	
	object InstanceParamsMethod (int index, params object[] args)
	{
		if (args == null)
			return "<null>";
		if (args.Length == 0)
			return "<empty>";
		return args [index];
	}
	
	T GenericMethod<T> (T t)
	{
		return t;
	}	


	public static int Main ()
	{
		Tester e = new Tester ();
		e.AddTest ();
		e.AddCheckedTest ();
		e.AndTest ();
		e.AndAlsoTest ();
		e.ArrayIndexTest ();
		e.CallTest ();
		
		return 0;
	}
}


