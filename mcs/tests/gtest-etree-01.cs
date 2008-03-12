using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

public struct InverseLogicalOperator
{
	bool value;
	public InverseLogicalOperator (bool value)
	{
		this.value = value;
	}

	public static bool operator true (InverseLogicalOperator u)
	{
		return u.value;
	}

	public static bool operator false (InverseLogicalOperator u)
	{
		return u.value;
	}
}

/* TODO: Add tests for every numeric expression where a type has only 1 implicit
		numeric conversion
public struct MyType<T>
{
	T value;

	public MyType (T value)
	{
		this.value = value;
	}

	public static implicit operator T (MyType<T> o)
	{
		return o.value;
	}
}
*/

public struct MyType
{
	int value;

	public MyType (int value)
	{
		this.value = value;
	}
	
	public override int GetHashCode ()
	{
		throw new NotImplementedException ();
	}

	public static implicit operator int (MyType o)
	{
		return o.value;
	}

	public static bool operator true (MyType a)
	{
		return a.value == a;
	}

	public static bool operator false (MyType a)
	{
		return a.value != a;
	}

	public static MyType operator +(MyType a, MyType b)
	{
		return new MyType (a.value + b.value);
	}

	public static MyType operator / (MyType a, MyType b)
	{
		return new MyType (a.value / b.value);
	}

	public static MyType operator &(MyType a, MyType b)
	{
		return new MyType (a.value & b.value);
	}

	public static MyType operator | (MyType a, MyType b)
	{
		return new MyType (a.value | b.value);
	}

	public static MyType operator ^ (MyType a, MyType b)
	{
		return new MyType (a.value ^ b.value);
	}

	public static bool operator == (MyType a, MyType b)
	{
		return a.value == b.value;
	}

	public static bool operator != (MyType a, MyType b)
	{
		return a.value != b.value;
	}
	
	public static bool operator > (MyType a, MyType b)
	{
		return a.value > b.value;
	}

	public static bool operator < (MyType a, MyType b)
	{
		return a.value < b.value;
	}

	public static bool operator >= (MyType a, MyType b)
	{
		return a.value >= b.value;
	}
	
	public static bool operator <= (MyType a, MyType b)
	{
		return a.value <= b.value;
	}

	public static bool operator ! (MyType a)
	{
		return a.value > 0;
	}
	
	public static int operator >> (MyType a, int b)
	{
		return a.value >> b;
	}	

	public override string ToString ()
	{
		return value.ToString ();
	}
}

// TODO: Add more nullable tests, follow AddTest pattern.

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

	static void Assert<T> (T [] expected, T [] value)
	{
		if (expected.Length != value.Length)
			throw new ApplicationException ("Array length does not match " + expected.Length + " != " + value.Length);

		for (int i = 0; i < expected.Length; ++i) {
			if (!EqualityComparer<T>.Default.Equals (expected [i], value [i]))
				throw new ApplicationException ("Index " + i + ": " + expected [i] + " != " + value [i]);
		}
	}

	void AddTest ()
	{
		Expression<Func<int, int, int>> e = (int a, int b) => a + b;
		AssertNodeType (e, ExpressionType.Add);
		Assert (50, e.Compile ().Invoke (20, 30));

		Expression<Func<int?, int?, int?>> e2 = (a, b) => a + b;
		AssertNodeType (e2, ExpressionType.Add);
		Assert (null, e2.Compile ().Invoke (null, 3));

		Expression<Func<MyType, MyType, MyType>> e3 = (MyType a, MyType b) => a + b;
		AssertNodeType (e3, ExpressionType.Add);
		Assert (10, e3.Compile ().Invoke (new MyType (-20), new MyType (30)));
		
		Expression<Func<MyType?, MyType?, MyType?>> e4 = (MyType? a, MyType? b) => a + b;
		AssertNodeType (e4, ExpressionType.Add);
		Assert (new MyType (10), e4.Compile ().Invoke (new MyType (-20), new MyType (30)));
		Assert (null, e4.Compile ().Invoke (null, new MyType (30)));

		Expression<Func<int, MyType, int>> e5 = (int a, MyType b) => a + b;
		AssertNodeType (e5, ExpressionType.Add);
		Assert (31, e5.Compile ().Invoke (1, new MyType (30)));
/*
		Expression<Func<int, MyType?, int?>> e6 = (int a, MyType? b) => a + b;
		AssertNodeType (e6, ExpressionType.Add);
		Assert (-1, e6.Compile ().Invoke (-31, new MyType (30)));		
*/
	}

	void AddCheckedTest ()
	{
		checked {
		Expression<Func<int, int, int>> e = (int a, int b) => a + b;
		AssertNodeType (e, ExpressionType.AddChecked);
		Assert (50, e.Compile ().Invoke (20, 30));

		Expression<Func<int?, int?, int?>> e2 = (a, b) => a + b;
		AssertNodeType (e2, ExpressionType.AddChecked);
		Assert (null, e2.Compile ().Invoke (null, 3));

		Expression<Func<MyType, MyType, MyType>> e3 = (MyType a, MyType b) => a + b;
		AssertNodeType (e3, ExpressionType.Add);
		Assert (10, e3.Compile ().Invoke (new MyType (-20), new MyType (30)));
		}
	}

	void AndTest ()
	{
		Expression<Func<bool, bool, bool>> e = (bool a, bool b) => a & b;

		AssertNodeType (e, ExpressionType.And);
		Func<bool, bool, bool> c = e.Compile ();

		Assert (true, c (true, true));
		Assert (false, c (true, false));
		Assert (false, c (false, true));
		Assert (false, c (false, false));

		Expression<Func<MyType, MyType, MyType>> e2 = (MyType a, MyType b) => a & b;

		AssertNodeType (e2, ExpressionType.And);
		var c2 = e2.Compile ();

		Assert (new MyType (0), c2 (new MyType (0), new MyType (1)));
		Assert (new MyType (1), c2 (new MyType (0xFF), new MyType (0x01)));
	}

	void AndNullableTest ()
	{
		Expression<Func<bool?, bool?, bool?>> e = (bool? a, bool? b) => a & b;

		AssertNodeType (e, ExpressionType.And);
		Func<bool?, bool?, bool?> c = e.Compile ();

		Assert (true, c (true, true));
		Assert (false, c (true, false));
		Assert (false, c (false, true));
		Assert (false, c (false, false));

		Assert (null, c (true, null));
		Assert (false, c (false, null));
		Assert (false, c (null, false));
		Assert (null, c (true, null));
		Assert (null, c (null, null));

		Expression<Func<MyType?, MyType?, MyType?>> e2 = (MyType? a, MyType? b) => a & b;

		AssertNodeType (e2, ExpressionType.And);
		var c2 = e2.Compile ();

		Assert (new MyType (0), c2 (new MyType (0), new MyType (1)));
		Assert (new MyType (1), c2 (new MyType (0xFF), new MyType (0x01)));
		Assert (null, c2 (new MyType (0xFF), null));
	}

	void AndAlsoTest ()
	{
		Expression<Func<bool, bool, bool>> e = (bool a, bool b) => a && b;

		AssertNodeType (e, ExpressionType.AndAlso);
		Assert (false, e.Compile ().Invoke (true, false));

		Expression<Func<MyType, MyType, MyType>> e2 = (MyType a, MyType b) => a && b;

		AssertNodeType (e2, ExpressionType.AndAlso);
		Assert (new MyType (64), e2.Compile ().Invoke (new MyType (64), new MyType (64)));
		Assert (new MyType (0), e2.Compile ().Invoke (new MyType (32), new MyType (64)));
	}

	void ArrayIndexTest ()
	{
		Expression<Func<string [], long, string>> e = (string [] a, long i) => a [i];
		AssertNodeType (e, ExpressionType.ArrayIndex);
		Assert ("b", e.Compile ().Invoke (new string [] { "a", "b", "c" }, 1));

		Expression<Func<string [], string>> e2 = (string [] a) => a [0];
		AssertNodeType (e2, ExpressionType.ArrayIndex);
		Assert ("a", e2.Compile ().Invoke (new string [] { "a", "b" }));

		Expression<Func<object [,], int, int, object>> e3 = (object [,] a, int i, int j) => a [i, j];
		AssertNodeType (e3, ExpressionType.Call);

		Assert ("z", e3.Compile ().Invoke (
			new object [,] { { 1, 2 }, { "x", "z" } }, 1, 1));

		Expression<Func<decimal [] [], byte, decimal>> e4 = (decimal [] [] a, byte b) => a [b] [1];
		AssertNodeType (e4, ExpressionType.ArrayIndex);

		decimal [] [] array = { new decimal [] { 1, 9 }, new decimal [] { 10, 90 } };
		Assert (90, e4.Compile ().Invoke (array, 1));
	}

	void ArrayLengthTest ()
	{
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

		Expression<Func<int, string, int, object>> e3 = (int index, string a, int b) => InstanceParamsMethod (index, a, b);
		AssertNodeType (e3, ExpressionType.Call);
		Assert<object> (4, e3.Compile ().Invoke (1, "a", 4));

		Expression<Func<object>> e4 = () => InstanceParamsMethod (0);
		AssertNodeType (e4, ExpressionType.Call);
		Assert<object> ("<empty>", e4.Compile ().Invoke ());

		Expression<Func<int, int>> e5 = (int a) => GenericMethod (a);
		AssertNodeType (e5, ExpressionType.Call);
		Assert (5, e5.Compile ().Invoke (5));
	}

	void CoalesceTest ()
	{
		Expression<Func<uint?, uint>> e = (uint? a) => a ?? 99;
		AssertNodeType (e, ExpressionType.Coalesce);
		var r = e.Compile ();
		Assert ((uint) 5, r.Invoke (5));
		Assert ((uint) 99, r.Invoke (null));

		Expression<Func<MyType?, int>> e2 = (MyType? a) => a ?? -3;
		AssertNodeType (e2, ExpressionType.Coalesce);
		var r2 = e2.Compile ();
		Assert (2, r2.Invoke (new MyType (2)));
		Assert (-3, r2.Invoke (null));
	}

	void ConditionTest ()
	{
		Expression<Func<bool, byte, int, int>> e = (bool a, byte b, int c) => (a ? b : c);
		AssertNodeType (e, ExpressionType.Conditional);
		var r = e.Compile ();
		Assert (3, r.Invoke (true, 3, 999999));
		Assert (999999, r.Invoke (false, 3, 999999));

		Expression<Func<int, decimal, decimal?>> e2 = (int a, decimal d) => (a > 0 ? d : a < 0 ? -d : (decimal?) null);
		AssertNodeType (e2, ExpressionType.Conditional);
		var r2 = e2.Compile ();
		Assert (null, r2.Invoke (0, 10));
		Assert (50, r2.Invoke (1, 50));
		Assert (30, r2.Invoke (-7, -30));

		Expression<Func<bool?, int?>> e3 = (bool? a) => ((bool) a ? 3 : -2);
		AssertNodeType (e3, ExpressionType.Convert);
		var r3 = e3.Compile ();
		Assert (3, r3.Invoke (true));
		Assert (-2, r3.Invoke (false));

		Expression<Func<InverseLogicalOperator, byte, byte, byte>> e4 = (InverseLogicalOperator a, byte b, byte c) => (a ? b : c);
		AssertNodeType (e4, ExpressionType.Conditional);
		var r4 = e4.Compile ();
		Assert (3, r4.Invoke (new InverseLogicalOperator (true), 3, 4));
		Assert (4, r4.Invoke (new InverseLogicalOperator (false), 3, 4));
	}

	void ConvertTest ()
	{
		Expression<Func<int, byte>> e = (int a) => ((byte) a);
		AssertNodeType (e, ExpressionType.Convert);
		Assert (100, e.Compile ().Invoke (100));

		Expression<Func<long, ushort>> e2 = (long a) => ((ushort) a);
		AssertNodeType (e2, ExpressionType.Convert);
		Assert (100, e2.Compile ().Invoke (100));

		Expression<Func<float?, float>> e3 = (float? a) => ((float) a);
		AssertNodeType (e3, ExpressionType.Convert);
		Assert (-0.456f, e3.Compile ().Invoke (-0.456f));

		Expression<Func<MyType, int>> e4 = (MyType a) => (a);
		AssertNodeType (e4, ExpressionType.Convert);
		Assert (-9, e4.Compile ().Invoke (new MyType (-9)));

		Expression<Func<MyType, MyType, bool?>> e5 = (MyType a, MyType b) => a == b;
		AssertNodeType (e5, ExpressionType.Convert);
	}

	void ConvertCheckedTest ()
	{
		Expression<Func<int, byte>> e = (int a) => checked((byte) a);
		AssertNodeType (e, ExpressionType.ConvertChecked);
		Assert (100, e.Compile ().Invoke (100));

		checked {
			Expression<Func<long, ushort>> e2 = (long a) => unchecked((ushort) a);
			AssertNodeType (e2, ExpressionType.Convert);
			Assert (100, e2.Compile ().Invoke (100));

			Expression<Func<float?, float>> e3 = (float? a) => ((float) a);
			AssertNodeType (e3, ExpressionType.ConvertChecked);
			Assert (-0.456f, e3.Compile ().Invoke (-0.456f));

			Expression<Func<MyType, int>> e4 = (MyType a) => (a);
			AssertNodeType (e4, ExpressionType.Convert);
			Assert (-9, e4.Compile ().Invoke (new MyType (-9)));
		}
	}

	void DivideTest ()
	{
		Expression<Func<int, int, int>> e = (int a, int b) => a / b;
		AssertNodeType (e, ExpressionType.Divide);
		Assert (2, e.Compile ().Invoke (60, 30));

		Expression<Func<double?, double?, double?>> e2 = (a, b) => a / b;
		AssertNodeType (e2, ExpressionType.Divide);
		Assert (null, e2.Compile ().Invoke (null, 3));
		Assert (1.5, e2.Compile ().Invoke (3, 2));

		Expression<Func<MyType, MyType, MyType>> e3 = (MyType a, MyType b) => a / b;
		AssertNodeType (e3, ExpressionType.Divide);
		Assert (1, e3.Compile ().Invoke (new MyType (-20), new MyType (-20)));

		Expression<Func<MyType?, MyType?, MyType?>> e4 = (MyType? a, MyType? b) => a / b;
		AssertNodeType (e4, ExpressionType.Divide);
		Assert (null, e4.Compile ().Invoke (null, new MyType (-20)));
		Assert (new MyType (-6), e4.Compile ().Invoke (new MyType (120), new MyType (-20)));
	}

	void EqualTest ()
	{
		Expression<Func<int, int, bool>> e = (int a, int b) => a == b;
		AssertNodeType (e, ExpressionType.Equal);
		Assert (false, e.Compile ().Invoke (60, 30));
		Assert (true, e.Compile ().Invoke (-1, -1));

		Expression<Func<double?, double?, bool>> e2 = (a, b) => a == b;
		AssertNodeType (e2, ExpressionType.Equal);
		Assert (true, e2.Compile ().Invoke (3, 3));
		Assert (false, e2.Compile ().Invoke (3, 2));

		Expression<Func<MyType, MyType, bool>> e3 = (MyType a, MyType b) => a == b;
		AssertNodeType (e3, ExpressionType.Equal);
		Assert (true, e3.Compile ().Invoke (new MyType (-20), new MyType (-20)));

		Expression<Func<MyType?, MyType?, bool>> e4 = (MyType? a, MyType? b) => a == b;
		AssertNodeType (e4, ExpressionType.Equal);
		Assert (false, e4.Compile ().Invoke (null, new MyType (-20)));
		Assert (true, e4.Compile ().Invoke (null, null));
		Assert (true, e4.Compile ().Invoke (new MyType (120), new MyType (120)));
	}
	
	delegate void EmptyDelegate ();
	
	void EqualTestDelegate ()
	{
		Expression<Func<Delegate, Delegate, bool>> e1 = (a, b) => a == b;
		AssertNodeType (e1, ExpressionType.Equal);
		Assert (true, e1.Compile ().Invoke (null, null));

		EmptyDelegate ed = delegate () {};

		Expression<Func<EmptyDelegate, EmptyDelegate, bool>> e2 = (a, b) => a == b;
		AssertNodeType (e2, ExpressionType.Equal);
		Assert (false, e2.Compile ().Invoke (delegate () {}, null));
		Assert (false, e2.Compile ().Invoke (delegate () {}, delegate {}));
		Assert (false, e2.Compile ().Invoke (ed, delegate {}));
		Assert (true, e2.Compile ().Invoke (ed, ed));
	}	

	void ExclusiveOrTest ()
	{
		Expression<Func<int, int, int>> e = (int a, int b) => a ^ b;
		AssertNodeType (e, ExpressionType.ExclusiveOr);
		Assert (34, e.Compile ().Invoke (60, 30));
/* FIXME: missing conversion
		Expression<Func<byte?, byte?, int?>> e2 = (a, b) => a ^ b;
		AssertNodeType (e2, ExpressionType.ExclusiveOr);
		Assert (null, e2.Compile ().Invoke (null, 3));
		Assert (1, e2.Compile ().Invoke (3, 2));
*/
		Expression<Func<MyType, MyType, MyType>> e3 = (MyType a, MyType b) => a ^ b;
		AssertNodeType (e3, ExpressionType.ExclusiveOr);
		Assert (0, e3.Compile ().Invoke (new MyType (-20), new MyType (-20)));

		Expression<Func<MyType?, MyType?, MyType?>> e4 = (MyType? a, MyType? b) => a ^ b;
		AssertNodeType (e4, ExpressionType.ExclusiveOr);
		Assert (null, e4.Compile ().Invoke (null, new MyType (-20)));
		Assert (new MyType (-108), e4.Compile ().Invoke (new MyType (120), new MyType (-20)));
	}

	void GreaterThanTest ()
	{
		Expression<Func<int, int, bool>> e = (int a, int b) => a > b;
		AssertNodeType (e, ExpressionType.GreaterThan);
		Assert (true, e.Compile ().Invoke (60, 30));
/*
		Expression<Func<uint?, byte?, bool>> e2 = (a, b) => a > b;
		AssertNodeType (e2, ExpressionType.GreaterThan);
		Assert (false, e2.Compile ().Invoke (null, 3));
		Assert (false, e2.Compile ().Invoke (2, 2));

		Expression<Func<MyType, MyType, bool>> e3 = (MyType a, MyType b) => a > b;
		AssertNodeType (e3, ExpressionType.GreaterThan);
		Assert (false, e3.Compile ().Invoke (new MyType (-20), new MyType (-20)));

		Expression<Func<MyType?, MyType?, bool>> e4 = (MyType? a, MyType? b) => a > b;
		AssertNodeType (e4, ExpressionType.GreaterThan);
		Assert (false, e4.Compile ().Invoke (null, new MyType (-20)));
		Assert (false, e4.Compile ().Invoke (null, null));
		Assert (true, e4.Compile ().Invoke (new MyType (120), new MyType (-20)));
*/
	}

	void GreaterThanOrEqualTest ()
	{
		Expression<Func<int, int, bool>> e = (int a, int b) => a >= b;
		AssertNodeType (e, ExpressionType.GreaterThanOrEqual);
		Assert (true, e.Compile ().Invoke (60, 30));

		Expression<Func<byte?, byte?, bool>> e2 = (a, b) => a >= b;
		AssertNodeType (e2, ExpressionType.GreaterThanOrEqual);
		Assert (false, e2.Compile ().Invoke (null, 3));
		Assert (true, e2.Compile ().Invoke (2, 2));

		Expression<Func<MyType, MyType, bool>> e3 = (MyType a, MyType b) => a >= b;
		AssertNodeType (e3, ExpressionType.GreaterThanOrEqual);
		Assert (true, e3.Compile ().Invoke (new MyType (-20), new MyType (-20)));
/*
		Expression<Func<MyType?, MyType?, bool>> e4 = (MyType? a, MyType? b) => a >= null;
		AssertNodeType (e4, ExpressionType.GreaterThanOrEqual);
		Assert (false, e4.Compile ().Invoke (null, new MyType (-20)));
		Assert (false, e4.Compile ().Invoke (null, null));
		Assert (true, e4.Compile ().Invoke (new MyType (120), new MyType (-20)));
*/
	}

	void NewArrayInitTest ()
	{
		Expression<Func<int []>> e = () => new int [0];
		AssertNodeType (e, ExpressionType.NewArrayInit);
		Assert (new int [0], e.Compile ().Invoke ());

		e = () => new int [] { };
		AssertNodeType (e, ExpressionType.NewArrayInit);
		Assert (new int [0], e.Compile ().Invoke ());

		Expression<Func<ushort, ulong? []>> e2 = (ushort a) => new ulong? [] { a };
		AssertNodeType (e2, ExpressionType.NewArrayInit);
		Assert (new ulong? [1] { ushort.MaxValue }, e2.Compile ().Invoke (ushort.MaxValue));

		Expression<Func<char [] []>> e3 = () => new char [] [] { new char [] { 'a' } };
		AssertNodeType (e3, ExpressionType.NewArrayInit);
		Assert (new char [] { 'a' }, e3.Compile ().Invoke () [0]);
	}
	
	void NotTest ()
	{
		Expression<Func<bool, bool>> e = (bool a) => !a;
		AssertNodeType (e, ExpressionType.Not);
		Assert (false, e.Compile ().Invoke (true));

		Expression<Func<MyType, bool>> e2 = (MyType a) => !a;
		AssertNodeType (e2, ExpressionType.Not);
		Assert (true, e2.Compile ().Invoke (new MyType (1)));
		Assert (false, e2.Compile ().Invoke (new MyType (-1)));
	}

	void NotNullableTest ()
	{
		Expression<Func<bool?, bool?>> e = (bool? a) => !a;
		AssertNodeType (e, ExpressionType.Not);
		Assert (false, e.Compile ().Invoke (true));
		Assert (null, e.Compile ().Invoke (null));

		Expression<Func<MyType?, bool?>> e2 = (MyType? a) => !a;
		AssertNodeType (e2, ExpressionType.Not);
		Assert (true, e2.Compile ().Invoke (new MyType (1)));
		Assert (null, e2.Compile ().Invoke (null));
	}
	
	void NotEqualTest ()
	{
		Expression<Func<int, int, bool>> e = (int a, int b) => a != b;
		AssertNodeType (e, ExpressionType.NotEqual);
		Assert (true, e.Compile ().Invoke (60, 30));
		Assert (false, e.Compile ().Invoke (-1, -1));

		Expression<Func<sbyte?, sbyte?, bool>> e2 = (a, b) => a != b;
		AssertNodeType (e2, ExpressionType.NotEqual);
		Assert (false, e2.Compile ().Invoke (3, 3));
		Assert (true, e2.Compile ().Invoke (3, 2));

		Expression<Func<MyType, MyType, bool>> e3 = (MyType a, MyType b) => a != b;
		AssertNodeType (e3, ExpressionType.NotEqual);
		Assert (false, e3.Compile ().Invoke (new MyType (-20), new MyType (-20)));

		Expression<Func<MyType?, MyType?, bool>> e4 = (MyType? a, MyType? b) => a != b;
		AssertNodeType (e4, ExpressionType.NotEqual);
		Assert (true, e4.Compile ().Invoke (null, new MyType (-20)));
		Assert (false, e4.Compile ().Invoke (null, null));
		Assert (false, e4.Compile ().Invoke (new MyType (120), new MyType (120)));
	}	

	void OrTest ()
	{
		Expression<Func<bool, bool, bool>> e = (bool a, bool b) => a | b;

		AssertNodeType (e, ExpressionType.Or);
		Func<bool, bool, bool> c = e.Compile ();

		Assert (true, c (true, true));
		Assert (true, c (true, false));
		Assert (true, c (false, true));
		Assert (false, c (false, false));

		Expression<Func<MyType, MyType, MyType>> e2 = (MyType a, MyType b) => a | b;
		AssertNodeType (e2, ExpressionType.Or);
		var c2 = e2.Compile ();
		Assert (new MyType (3), c2 (new MyType (1), new MyType (2)));
	}

	void OrNullableTest ()
	{
		Expression<Func<bool?, bool?, bool?>> e = (bool? a, bool? b) => a | b;

		AssertNodeType (e, ExpressionType.Or);
		Func<bool?, bool?, bool?> c = e.Compile ();

		Assert (true, c (true, true));
		Assert (true, c (true, false));
		Assert (true, c (false, true));
		Assert (false, c (false, false));

		Assert (true, c (true, null));
		Assert (null, c (false, null));
		Assert (null, c (null, false));
		Assert (true, c (true, null));
		Assert (null, c (null, null));

		Expression<Func<MyType?, MyType?, MyType?>> e2 = (MyType? a, MyType? b) => a | b;
		AssertNodeType (e2, ExpressionType.Or);
		var c2 = e2.Compile ();
		Assert (new MyType (3), c2 (new MyType (1), new MyType (2)));
		Assert (null, c2 (new MyType (1), null));
	}
	
	void OrElseTest ()
	{
		Expression<Func<bool, bool, bool>> e = (bool a, bool b) => a || b;
		AssertNodeType (e, ExpressionType.OrElse);
		Assert (true, e.Compile ().Invoke (true, false));

		Expression<Func<MyType, MyType, MyType>> e2 = (MyType a, MyType b) => a || b;
		AssertNodeType (e2, ExpressionType.OrElse);
		Assert (new MyType (64), e2.Compile ().Invoke (new MyType (64), new MyType (64)));
		Assert (new MyType (32), e2.Compile ().Invoke (new MyType (32), new MyType (64)));
	}
	
	void ParameterTest ()
	{
		Expression<Func<string, string>> e = (string a) => a;
		AssertNodeType (e, ExpressionType.Parameter);
		Assert ("t", e.Compile ().Invoke ("t"));

		Expression<Func<object[], object[]>> e2 = (object[] a) => a;
		AssertNodeType (e2, ExpressionType.Parameter);
		Assert (new object[0], e2.Compile ().Invoke (new object[0]));

		Expression<Func<IntPtr, IntPtr>> e3 = a => a;
		AssertNodeType (e3, ExpressionType.Parameter);
		Assert (IntPtr.Zero, e3.Compile ().Invoke (IntPtr.Zero));
	}
	
	void QuoteTest ()
	{
		Expression<Func<Expression<Func<int>>>> e = () => () => 2;
		AssertNodeType (e, ExpressionType.Quote);
		Assert (2, e.Compile ().Invoke ().Compile ().Invoke ());
	}

	void RightShiftTest ()
	{
		Expression<Func<ulong, sbyte, ulong>> e = (ulong a, sbyte b) => a >> b;
		AssertNodeType (e, ExpressionType.RightShift);
		Assert ((ulong)0x1FD940L, e.Compile ().Invoke (0xFECA0000, 11));

		Expression<Func<MyType, MyType, int>> e2 = (MyType a, MyType b) => a >> b;
		AssertNodeType (e2, ExpressionType.RightShift);
		var c2 = e2.Compile ();
		Assert (64, c2 (new MyType (256), new MyType (2)));
/*		
		Expression<Func<long?, sbyte, long?>> e3 = (long? a, sbyte b) => a >> b;
		AssertNodeType (e3, ExpressionType.RightShift);
		Assert (null, e3.Compile ().Invoke (null, 11));
		Assert (512, e3.Compile ().Invoke (1024, 1));

		Expression<Func<MyType?, MyType?, int?>> e4 = (MyType? a, MyType? b) => a >> b;
		AssertNodeType (e4, ExpressionType.RightShift);
		var c4 = e4.Compile ();
		Assert (null, c4 (new MyType (8), null));
		Assert (null, c4 (null, new MyType (8)));
		Assert (64, c4 (new MyType (256), new MyType (2)));
*/
	}	

	//
	// Test helpers
	//
	string InstanceMethod (string arg)
	{
		return arg;
	}

	object InstanceParamsMethod (int index, params object [] args)
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
		e.AndNullableTest ();
		e.AddCheckedTest ();
		e.AndTest ();
		e.AndAlsoTest ();
		e.ArrayIndexTest ();
		e.ArrayLengthTest ();
		e.CallTest ();
		e.CoalesceTest ();
		e.ConditionTest ();
		e.ConvertTest ();
		e.ConvertCheckedTest ();
		e.DivideTest ();
		e.EqualTest ();
		e.EqualTestDelegate ();
		e.ExclusiveOrTest ();
		e.GreaterThanTest ();
		e.GreaterThanOrEqualTest ();
		e.NewArrayInitTest ();
		e.NotTest ();
		e.NotNullableTest ();
		e.NotEqualTest ();
		e.OrTest ();
		e.OrNullableTest ();
		e.OrElseTest ();
		e.ParameterTest ();
		e.QuoteTest ();
		e.RightShiftTest ();

		return 0;
	}
}

