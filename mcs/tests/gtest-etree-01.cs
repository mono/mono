// Compiler options: -unsafe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

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


// TODO: Create a clone which uses +(MyType, int) pattern and an implicit conversion
// is required to do the user-conversion

public struct MyType
{
	int value;
	
	public MyType (int value) : this ()
	{
		this.value = value;
	}
	
	public short ShortProp { get; set; }
	
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

	public static MyType operator + (MyType a, MyType b)
	{
		return new MyType (a.value + b.value);
	}

	public static MyType operator - (MyType a, MyType b)
	{
		return new MyType (a.value - b.value);
	}

	public static MyType operator / (MyType a, MyType b)
	{
		return new MyType (a.value / b.value);
	}

	public static MyType operator * (MyType a, MyType b)
	{
		return new MyType (a.value * b.value);
	}

	public static MyType operator % (MyType a, MyType b)
	{
		return new MyType (a.value % b.value);
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
	
	public static int operator << (MyType a, int b)
	{
		return a.value << b;
	}
	
	public static MyType operator - (MyType a)
	{
		return new MyType (-a.value);
	}
	
	public static MyType operator + (MyType a)
	{
		return new MyType (+a.value);
	}

	public override string ToString ()
	{
		return value.ToString ();
	}
}


class MyTypeExplicit
{
	int value;
	
	public MyTypeExplicit (int value)
	{
		this.value = value;
	}
	
	public static explicit operator int (MyTypeExplicit m)
	{
		return m.value;
	}
}

struct MyTypeImplicitOnly
{
	short b;

	public MyTypeImplicitOnly (short b)
	{
		this.b = b;
	}

	public static implicit operator short (MyTypeImplicitOnly m)
	{
		return m.b;
	}
}

class MemberAccessData
{
	public bool BoolValue;
	public static decimal DecimalValue = decimal.MinValue;
	public volatile uint VolatileValue;
	public string [] StringValues;
	public List<string> ListValues;

	event Func<bool> EventField;
	public Expression<Func<Func<bool>>> GetEvent ()
	{
		return () => EventField;
	}
	
	MyType mt;
	public MyType MyTypeProperty {
		set	{
			mt = value;
		}
		get {
			return mt;
		}
	}
	
	public static string StaticProperty {
		get {
			return "alo";
		}
	}
}

enum MyEnum : byte
{
	Value_1 = 1,
	Value_2 = 2
}

enum MyEnumUlong : ulong
{
	Value_1 = 1
}


class NewTest<T>
{
	T [] t;
	public NewTest (T i)
	{
		t = new T [] { i };
	}

	public NewTest (params T [] t)
	{
		this.t = t;
	}

	public override int GetHashCode ()
	{
		return base.GetHashCode ();
	}

	public override bool Equals (object obj)
	{
		NewTest<T> obj_t = obj as NewTest<T>;
		if (obj_t == null)
			return false;

		for (int i = 0; i < t.Length; ++i) {
			if (!t [i].Equals (obj_t.t [i]))
				return false;
		}

		return true;
	}
}

class Indexer
{
	public int this [int i] { get { return i; } set { } }
	public string this [params string[] i] { get { return string.Concat (i); } }
}

class A { }
class B : A { }

// TODO: Add more nullable tests, follow AddTest pattern.

class Tester
{
	delegate void EmptyDelegate ();
	delegate int IntDelegate ();
	static int ReturnNumber ()
	{
		return 8;
	}

	static void AssertNodeType (LambdaExpression e, ExpressionType et)
	{
		if (e.Body.NodeType != et)
			throw new ApplicationException (e.Body.NodeType + " != " + et);
	}

	static void Assert<T> (T expected, T value)
	{
		Assert (expected, value, null);
	}

	static void Assert<T> (T expected, T value, string name)
	{
		if (!EqualityComparer<T>.Default.Equals (expected, value)) {
			if (!string.IsNullOrEmpty (name))
				name += ": ";
			throw new ApplicationException (name + expected + " != " + value);
		}
	}

	static void Assert<T> (T [] expected, T [] value)
	{
		if (expected == null) {
			if (value != null)
				throw new ApplicationException ("Both arrays expected to be null");
			return;
		}
	
		if (expected.Length != value.Length)
			throw new ApplicationException ("Array length does not match " + expected.Length + " != " + value.Length);

		for (int i = 0; i < expected.Length; ++i) {
			if (!EqualityComparer<T>.Default.Equals (expected [i], value [i]))
				throw new ApplicationException ("Index " + i + ": " + expected [i] + " != " + value [i]);
		}
	}

#pragma warning disable 169

	void AddTest ()
	{
		Expression<Func<int, int, int>> e = (int a, int b) => a + b;
		AssertNodeType (e, ExpressionType.Add);
		Assert (50, e.Compile ().Invoke (20, 30));
	}
	
	void AddTest_2 ()
	{
		Expression<Func<int?, int?, int?>> e2 = (a, b) => a + b;
		AssertNodeType (e2, ExpressionType.Add);
		Assert (null, e2.Compile ().Invoke (null, 3));
	}
	
	void AddTest_3 ()
	{
		Expression<Func<MyType, MyType, MyType>> e3 = (MyType a, MyType b) => a + b;
		AssertNodeType (e3, ExpressionType.Add);
		Assert (10, e3.Compile ().Invoke (new MyType (-20), new MyType (30)));
	}
	
	void AddTest_4 ()
	{
		Expression<Func<MyType?, MyType?, MyType?>> e4 = (MyType? a, MyType? b) => a + b;
		AssertNodeType (e4, ExpressionType.Add);
		Assert (new MyType (10), e4.Compile ().Invoke (new MyType (-20), new MyType (30)));
		Assert (null, e4.Compile ().Invoke (null, new MyType (30)));
	}
	
	void AddTest_5 ()
	{
		Expression<Func<int, MyType, int>> e5 = (int a, MyType b) => a + b;
		AssertNodeType (e5, ExpressionType.Add);
		Assert (31, e5.Compile ().Invoke (1, new MyType (30)));
	}
	
	void AddTest_6 ()
	{
		Expression<Func<int, MyType?, int?>> e6 = (int a, MyType? b) => a + b;
		AssertNodeType (e6, ExpressionType.Add);
		Assert (-1, e6.Compile ().Invoke (-31, new MyType (30)));
	}
	
	void AddTest_7 ()
	{
		Expression<Func<MyEnum, byte, MyEnum>> e7 = (a, b) => a + b;
		AssertNodeType (e7, ExpressionType.Convert);
		Assert (MyEnum.Value_2, e7.Compile ().Invoke (MyEnum.Value_1, 1));
	}
	
	void AddTest_8 ()
	{
		// CSC BUG: probably due to missing numeric promotion
		Expression<Func<MyEnum?, byte?, MyEnum?>> e8 = (a, b) => a + b;
		AssertNodeType (e8, ExpressionType.Convert);
		Assert<MyEnum?> (0, e8.Compile ().Invoke (MyEnum.Value_1, 255));
		Assert (null, e8.Compile ().Invoke (MyEnum.Value_1, null));
		Assert (null, e8.Compile ().Invoke (null, null));
	}
	
	void AddTest_9 ()
	{
		Expression<Func<byte, MyEnum, MyEnum>> e9 = (a, b) => a + b;
		AssertNodeType (e9, ExpressionType.Convert);
		Assert (MyEnum.Value_2, e9.Compile ().Invoke (1, MyEnum.Value_1));
	}

	void AddCheckedTest ()
	{
		checked {
		Expression<Func<int, int, int>> e = (int a, int b) => a + b;
		AssertNodeType (e, ExpressionType.AddChecked);
		Assert (50, e.Compile ().Invoke (20, 30));
		}
	}
	
	void AddCheckedTest_2 ()
	{
		checked {
		Expression<Func<int?, int?, int?>> e2 = (a, b) => a + b;
		AssertNodeType (e2, ExpressionType.AddChecked);
		Assert (null, e2.Compile ().Invoke (null, 3));
		}
	}
	
	void AddCheckedTest_3 ()
	{
		checked {
		Expression<Func<MyType, MyType, MyType>> e3 = (MyType a, MyType b) => a + b;
		AssertNodeType (e3, ExpressionType.Add);
		Assert (10, e3.Compile ().Invoke (new MyType (-20), new MyType (30)));
		}
	}
	
	void AddStringTest ()
	{
		Expression<Func<string, string>> e6 = (a) => 1 + a;
		AssertNodeType (e6, ExpressionType.Add);
		Assert ("1to", e6.Compile ().Invoke ("to"));
	}
	
	void AddStringTest_2 ()
	{
		Expression<Func<object, string, string>> e7 = (object a, string b) => a + b;
		AssertNodeType (e7, ExpressionType.Add);
		Assert ("testme", e7.Compile ().Invoke ("test", "me"));
		Assert ("test", e7.Compile ().Invoke ("test", null));
		Assert ("", e7.Compile ().Invoke (null, null));
	}
	
	void AddStringTest_3 ()
	{
		Expression<Func<string, int, string>> e8 = (a, b) => a + " " + "-" + "> " + b;
		AssertNodeType (e8, ExpressionType.Add);
		Assert ("test -> 2", e8.Compile ().Invoke ("test", 2));
	}
	
	void AddStringTest_4 ()
	{
		Expression<Func<string, ushort?, string>> e9 = (a, b) => a + b;
		AssertNodeType (e9, ExpressionType.Add);
		Assert ("test2", e9.Compile ().Invoke ("test", 2));
		Assert ("test", e9.Compile ().Invoke ("test", null));	
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
	}
	
	void AndTest_2 ()
	{
		Expression<Func<MyType, MyType, MyType>> e2 = (MyType a, MyType b) => a & b;

		AssertNodeType (e2, ExpressionType.And);
		var c2 = e2.Compile ();

		Assert (new MyType (0), c2 (new MyType (0), new MyType (1)));
		Assert (new MyType (1), c2 (new MyType (0xFF), new MyType (0x01)));
	}
	
	void AndTest_3 ()
	{
		Expression<Func<MyEnum, MyEnum, MyEnum>> e3 = (a, b) => a & b;
		AssertNodeType (e3, ExpressionType.Convert);
		Assert<MyEnum> (0, e3.Compile ().Invoke (MyEnum.Value_1, MyEnum.Value_2));
		Assert (MyEnum.Value_2, e3.Compile ().Invoke (MyEnum.Value_2, MyEnum.Value_2));
	}
	
	void AndTest_4 ()
	{
		Expression<Func<int, int>> e = (a) => a & 0;
		AssertNodeType (e, ExpressionType.And);
		var c = e.Compile ();
		
		Assert (0, c (1));
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
	}
	
	void AndNullableTest_2 ()
	{
		Expression<Func<MyType?, MyType?, MyType?>> e2 = (MyType? a, MyType? b) => a & b;

		AssertNodeType (e2, ExpressionType.And);
		var c2 = e2.Compile ();

		Assert (new MyType (0), c2 (new MyType (0), new MyType (1)));
		Assert (new MyType (1), c2 (new MyType (0xFF), new MyType (0x01)));
		Assert (null, c2 (new MyType (0xFF), null));
	}
	
	void AndNullableTest_3 ()
	{
		Expression<Func<MyEnum?, MyEnum?, MyEnum?>> e3 = (a, b) => a & b;
		AssertNodeType (e3, ExpressionType.Convert);
		Assert (null, e3.Compile ().Invoke (null, MyEnum.Value_2));
		Assert (MyEnum.Value_2, e3.Compile ().Invoke (MyEnum.Value_2, MyEnum.Value_2));
	}

	void AndAlsoTest ()
	{
		Expression<Func<bool, bool, bool>> e = (bool a, bool b) => a && b;
		AssertNodeType (e, ExpressionType.AndAlso);
		Assert (false, e.Compile ().Invoke (true, false));
	}

	void AndAlsoTest_2 ()
	{
		Expression<Func<MyType, MyType, MyType>> e2 = (MyType a, MyType b) => a && b;
		AssertNodeType (e2, ExpressionType.AndAlso);
		Assert (new MyType (64), e2.Compile ().Invoke (new MyType (64), new MyType (64)));
		Assert (new MyType (0), e2.Compile ().Invoke (new MyType (32), new MyType (64)));
	}

	void AndAlsoTest_3 ()
	{
		Expression<Func<bool, bool>> e3 = (bool a) => a && true;
		AssertNodeType (e3, ExpressionType.AndAlso);
		Assert (false, e3.Compile ().Invoke (false));
		Assert (true, e3.Compile ().Invoke (true));
	}

	void ArrayIndexTest ()
	{
		Expression<Func<string [], long, string>> e = (string [] a, long i) => a [i];
		AssertNodeType (e, ExpressionType.ArrayIndex);
		Assert ("b", e.Compile ().Invoke (new string [] { "a", "b", "c" }, 1));
	}

	void ArrayIndexTest_2 ()
	{
		Expression<Func<string [], string>> e2 = (string [] a) => a [0];
		AssertNodeType (e2, ExpressionType.ArrayIndex);
		Assert ("a", e2.Compile ().Invoke (new string [] { "a", "b" }));
	}

	void ArrayIndexTest_3 ()
	{
		Expression<Func<object [,], int, int, object>> e3 = (object [,] a, int i, int j) => a [i, j];
		AssertNodeType (e3, ExpressionType.Call);

		Assert ("z", e3.Compile ().Invoke (
			new object [,] { { 1, 2 }, { "x", "z" } }, 1, 1));
	}

	void ArrayIndexTest_4 ()
	{
		Expression<Func<decimal [] [], byte, decimal>> e4 = (decimal [] [] a, byte b) => a [b] [1];
		AssertNodeType (e4, ExpressionType.ArrayIndex);

		decimal [] [] array = { new decimal [] { 1, 9 }, new decimal [] { 10, 90 } };
		Assert (90, e4.Compile ().Invoke (array, 1));
	}

	void ArrayIndexTest_5 ()
	{
		Expression<Func<int>> e5 = () => (new int [1]) [0];
		AssertNodeType (e5, ExpressionType.ArrayIndex);
		Assert (0, e5.Compile ().Invoke ());
	}
	
	void ArrayIndexTest_6 ()
	{
		const ulong max = 3;
		
		Expression<Func<int[], int>> e = a => a [max];
		AssertNodeType (e, ExpressionType.ArrayIndex);
		Assert (4, e.Compile ().Invoke (new int [] { 1, 2, 3, 4, 5 }));
	}
	
	void ArrayIndexTest_7 ()
	{
		const ulong max = uint.MaxValue;
		
		Expression<Func<int[], int>> e = a => a [max];
		AssertNodeType (e, ExpressionType.ArrayIndex);
		
		try {
			e.Compile ().Invoke (new int [0]);
			throw new ApplicationException ("ArrayIndexTest_7");
		} catch (System.OverflowException) {
			// Check whether CheckedConversion was generated
		}
	}

	void ArrayLengthTest ()
	{
		Expression<Func<double [], int>> e = (double [] a) => a.Length;
		AssertNodeType (e, ExpressionType.ArrayLength);
		Assert (0, e.Compile ().Invoke (new double [0]));
		Assert (9, e.Compile ().Invoke (new double [9]));
	}

	void ArrayLengthTest_2 ()
	{
		Expression<Func<string [,], int>> e2 = (string [,] a) => a.Length;
		AssertNodeType (e2, ExpressionType.MemberAccess);
		Assert (0, e2.Compile ().Invoke (new string [0, 0]));
	}

	void CallTest ()
	{
		Expression<Func<int, int>> e = (int a) => Math.Max (a, 5);
		AssertNodeType (e, ExpressionType.Call);
		Assert (5, e.Compile ().Invoke (2));
		Assert (9, e.Compile ().Invoke (9));
	}

	void CallTest_2 ()
	{
		Expression<Func<string, string>> e2 = (string a) => InstanceMethod (a);
		AssertNodeType (e2, ExpressionType.Call);
		Assert ("abc", e2.Compile ().Invoke ("abc"));
	}

	void CallTest_3 ()
	{
		Expression<Func<int, string, int, object>> e3 = (int index, string a, int b) => InstanceParamsMethod (index, a, b);
		AssertNodeType (e3, ExpressionType.Call);
		Assert<object> (4, e3.Compile ().Invoke (1, "a", 4));
	}

	void CallTest_4 ()
	{
		Expression<Func<object>> e4 = () => InstanceParamsMethod (0);
		AssertNodeType (e4, ExpressionType.Call);
		Assert<object> ("<empty>", e4.Compile ().Invoke ());
	}

	void CallTest_5 ()
	{
		Expression<Func<int, int>> e5 = (int a) => GenericMethod (a);
		AssertNodeType (e5, ExpressionType.Call);
		Assert (5, e5.Compile ().Invoke (5));
	}

	void CallTest_6 ()
	{
		Expression<Action> e6 = () => Console.WriteLine ("call test");
		AssertNodeType (e6, ExpressionType.Call);
	}

	void CallTest_7 ()
	{
		Expression<Func<Indexer, int, int>> e7 = (a, b) => a [b];
		AssertNodeType (e7, ExpressionType.Call);
		Assert (3, e7.Compile ().Invoke (new Indexer (), 3));
	}

	void CallTest_8 ()
	{
		Expression<Func<Indexer, string, string, string, string>> e8 = (a, b, c , d) => a [b, c, d];
		AssertNodeType (e8, ExpressionType.Call);
		Assert ("zyb", e8.Compile ().Invoke (new Indexer (), "z", "y", "b"));
	}

	void CallTest_9 ()
	{
		Expression<Action<int>> e9 = (a) => RefMethod (ref a);
		AssertNodeType (e9, ExpressionType.Call);
		e9.Compile ().Invoke (1);
	}		

	void CoalesceTest ()
	{
		Expression<Func<uint?, uint>> e = (uint? a) => a ?? 99;
		AssertNodeType (e, ExpressionType.Coalesce);
		var r = e.Compile ();
		Assert ((uint) 5, r.Invoke (5));
		Assert ((uint) 99, r.Invoke (null));
	}		

	void CoalesceTest_2 ()
	{
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
	}

	void ConditionTest_2 ()
	{
		Expression<Func<int, decimal, decimal?>> e2 = (int a, decimal d) => (a > 0 ? d : a < 0 ? -d : (decimal?) null);
		AssertNodeType (e2, ExpressionType.Conditional);
		var r2 = e2.Compile ();
		Assert (null, r2.Invoke (0, 10));
		Assert (50, r2.Invoke (1, 50));
		Assert (30, r2.Invoke (-7, -30));
	}

	void ConditionTest_3 ()
	{
		Expression<Func<bool?, int?>> e3 = (bool? a) => ((bool) a ? 3 : -2);
		AssertNodeType (e3, ExpressionType.Convert);
		var r3 = e3.Compile ();
		Assert (3, r3.Invoke (true));
		Assert (-2, r3.Invoke (false));
	}

	void ConditionTest_4 ()
	{
		Expression<Func<InverseLogicalOperator, byte, byte, byte>> e4 = (InverseLogicalOperator a, byte b, byte c) => (a ? b : c);
		AssertNodeType (e4, ExpressionType.Conditional);
		var r4 = e4.Compile ();
		Assert (3, r4.Invoke (new InverseLogicalOperator (true), 3, 4));
		Assert (4, r4.Invoke (new InverseLogicalOperator (false), 3, 4));
	}
	
	void ConditionTest_5 ()
	{
		// CSC bug ?
		Expression<Func<int>> e = () => false ? 1 : 4;
		AssertNodeType (e, ExpressionType.Conditional);
		var r = e.Compile ();
		Assert (4, r.Invoke ());
	}
	
	void ConstantTest ()
	{
		Expression<Func<int>> e1 = () => default (int);
		AssertNodeType (e1, ExpressionType.Constant);
		Assert (0, e1.Compile ().Invoke ());
	}
	
	void ConstantTest_2 ()
	{
		Expression<Func<int?>> e2 = () => default (int?);
		AssertNodeType (e2, ExpressionType.Constant);
		Assert (null, e2.Compile ().Invoke ());
	}

	void ConstantTest_3 ()
	{
		Expression<Func<Tester>> e3 = () => default (Tester);
		AssertNodeType (e3, ExpressionType.Constant);
		Assert (null, e3.Compile ().Invoke ());
	}
	
	void ConstantTest_4 ()
	{
		Expression<Func<object>> e4 = () => null;
		AssertNodeType (e4, ExpressionType.Constant);
		Assert (null, e4.Compile ().Invoke ());
	}
	
	void ConstantTest_5 ()
	{
		Expression<Func<int>> e5 = () => 8 / 4;
		AssertNodeType (e5, ExpressionType.Constant);
		Assert (2, e5.Compile ().Invoke ());
	}
	
	void ConstantTest_6 ()
	{
		Expression<Func<int>> e6 = () => 0xFFFFFF >> 0x40;
		AssertNodeType (e6, ExpressionType.Constant);
		Assert (0xFFFFFF, e6.Compile ().Invoke ());
	}
	
	void ConstantTest_7 ()
	{
		Expression<Func<object>> e7 = () => "Alleluia";
		AssertNodeType (e7, ExpressionType.Constant);
		Assert ("Alleluia", e7.Compile ().Invoke ());		
	}
	
	void ConstantTest_8 ()
	{
		Expression<Func<Type>> e8 = () => typeof (int);
		AssertNodeType (e8, ExpressionType.Constant);
		Assert (typeof (int), e8.Compile ().Invoke ());
	}
	
	void ConstantTest_9 ()
	{
		Expression<Func<Type>> e9 = () => typeof (void);
		AssertNodeType (e9, ExpressionType.Constant);
		Assert (typeof (void), e9.Compile ().Invoke ());
	}
	
	void ConstantTest_10 ()
	{
		Expression<Func<Type>> e10 = () => typeof (Func<,>);
		AssertNodeType (e10, ExpressionType.Constant);
		Assert (typeof (Func<,>), e10.Compile ().Invoke ());
	}
	
	void ConstantTest_11 ()
	{
		Expression<Func<MyEnum>> e11 = () => MyEnum.Value_2;
		AssertNodeType (e11, ExpressionType.Constant);
		Assert (MyEnum.Value_2, e11.Compile ().Invoke ());
	}
	
	void ConstantTest_13 ()
	{
		Expression<Func<int>> e13 = () => sizeof (byte);
		AssertNodeType (e13, ExpressionType.Constant);
		Assert (1, e13.Compile ().Invoke ());
		
	}
	
	void ConstantTest_14 ()
	{
		Expression<Func<Type>> e14 = () => typeof (bool*);
		AssertNodeType (e14, ExpressionType.Constant);
		Assert (typeof (bool*), e14.Compile ().Invoke ());
	}
	
	void ConstantTest_15 ()
	{
		Expression<Func<int?>> e15 = () => null;
		AssertNodeType (e15, ExpressionType.Constant);
		Assert (null, e15.Compile ().Invoke ());
	}

	void ConvertTest ()
	{
		Expression<Func<int, byte>> e = (int a) => ((byte) a);
		AssertNodeType (e, ExpressionType.Convert);
		Assert (100, e.Compile ().Invoke (100));
	}

	void ConvertTest_2 ()
	{
		Expression<Func<long, ushort>> e2 = (long a) => ((ushort) a);
		AssertNodeType (e2, ExpressionType.Convert);
		Assert (100, e2.Compile ().Invoke (100));
	}

	void ConvertTest_3 ()
	{
		Expression<Func<float?, float>> e3 = (float? a) => ((float) a);
		AssertNodeType (e3, ExpressionType.Convert);
		Assert (-0.456f, e3.Compile ().Invoke (-0.456f));
	}

	void ConvertTest_4 ()
	{
		Expression<Func<MyType, int>> e4 = (MyType a) => (a);
		AssertNodeType (e4, ExpressionType.Convert);
		Assert (-9, e4.Compile ().Invoke (new MyType (-9)));
	}

	void ConvertTest_5 ()
	{
		Expression<Func<MyType, MyType, bool?>> e5 = (MyType a, MyType b) => a == b;
		AssertNodeType (e5, ExpressionType.Convert);
	}

	void ConvertTest_6 ()
	{
		Expression<Func<MyType?, MyType?, bool?>> e6 = (MyType? a, MyType? b) => a == b;
		AssertNodeType (e6, ExpressionType.Convert);
		Assert (false, e6.Compile ().Invoke (null, new MyType (-20)));
		Assert (true, e6.Compile ().Invoke (null, null));
		Assert (true, e6.Compile ().Invoke (new MyType (120), new MyType (120)));
	}

	void ConvertTest_7 ()
	{
		Expression<Func<MyTypeExplicit, int?>> e7 = x => (int?)x;
		AssertNodeType (e7, ExpressionType.Convert);
		Assert (33, e7.Compile ().Invoke (new MyTypeExplicit (33)));
	}

	void ConvertTest_8 ()
	{
		Expression<Func<int?, object>> e8 = x => (object)x;
		AssertNodeType (e8, ExpressionType.Convert);
		Assert (null, e8.Compile ().Invoke (null));
		Assert (-100, e8.Compile ().Invoke (-100));
	}

	unsafe void ConvertTest_9 ()
	{
		int*[] p = new int* [1];
		Expression<Func<object>> e9 = () => (object)p;
		AssertNodeType (e9, ExpressionType.Convert);
		Assert (p, e9.Compile ().Invoke ());
	}

	void ConvertTest_10 ()
	{
		Expression<Func<Func<int>, Delegate>> e10 = (a) => a + a;
		AssertNodeType (e10, ExpressionType.Convert);
		Assert (null, e10.Compile ().Invoke (null));
		Assert (new Func<int> (TestInt) + new Func<int> (TestInt), e10.Compile ().Invoke (TestInt));
	}

	void ConvertTest_11 ()
	{
		Expression<Func<Func<int>, Delegate>> e11 = (a) => a - a;
		AssertNodeType (e11, ExpressionType.Convert);
		Assert (null, e11.Compile ().Invoke (null));
	}

	void ConvertTest_12 ()
	{
		Expression<Func<Func<int>>> e12 = () => TestInt;
		AssertNodeType (e12, ExpressionType.Convert);
		Assert (29, e12.Compile ().Invoke () ());
	}

	void ConvertTest_13 ()
	{
		Expression<Func<decimal, sbyte>> e13 = a => (sbyte)a;
		AssertNodeType (e13, ExpressionType.Convert);
		Assert (6, e13.Compile ().Invoke (6));
	}

	void ConvertTest_14 ()
	{
		Expression<Func<long, decimal>> e14 = a => a;
		AssertNodeType (e14, ExpressionType.Convert);
		Assert (-66, e14.Compile ().Invoke (-66));
	}

	void ConvertTest_15 ()
	{
		Expression<Func<ulong?, decimal?>> e15 = a => a;
		AssertNodeType (e15, ExpressionType.Convert);
		Assert (null, e15.Compile ().Invoke (null));
		Assert (9, e15.Compile ().Invoke (9));
	}
	
	void ConvertTest_16 ()
	{
		Expression<Func<sbyte, sbyte>> e16 = a => (sbyte)a;
		AssertNodeType (e16, ExpressionType.Convert);
		Assert (6, e16.Compile ().Invoke (6));
	}	

	void ConvertCheckedTest ()
	{
		Expression<Func<int, byte>> e = (int a) => checked((byte) a);
		AssertNodeType (e, ExpressionType.ConvertChecked);
		Assert (100, e.Compile ().Invoke (100));
	}

	void ConvertCheckedTest_2 ()
	{
		checked {
			Expression<Func<long, ushort>> e2 = (long a) => unchecked((ushort) a);
			AssertNodeType (e2, ExpressionType.Convert);
			Assert (100, e2.Compile ().Invoke (100));
		}
	}

	void ConvertCheckedTest_3 ()
	{
		checked {
			Expression<Func<float?, float>> e3 = (float? a) => ((float) a);
			AssertNodeType (e3, ExpressionType.ConvertChecked);
			Assert (-0.456f, e3.Compile ().Invoke (-0.456f));
		}
	}

	void ConvertCheckedTest_4 ()
	{
		checked {
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
	}

	void DivideTest_2 ()
	{
		Expression<Func<double?, double?, double?>> e2 = (a, b) => a / b;
		AssertNodeType (e2, ExpressionType.Divide);
		Assert (null, e2.Compile ().Invoke (null, 3));
		Assert (1.5, e2.Compile ().Invoke (3, 2));
	}

	void DivideTest_3 ()
	{
		Expression<Func<MyType, MyType, MyType>> e3 = (MyType a, MyType b) => a / b;
		AssertNodeType (e3, ExpressionType.Divide);
		Assert (1, e3.Compile ().Invoke (new MyType (-20), new MyType (-20)));
	}

	void DivideTest_4 ()
	{
		Expression<Func<MyType?, MyType?, MyType?>> e4 = (MyType? a, MyType? b) => a / b;
		AssertNodeType (e4, ExpressionType.Divide);
		Assert (null, e4.Compile ().Invoke (null, new MyType (-20)));
		Assert (new MyType (-6), e4.Compile ().Invoke (new MyType (120), new MyType (-20)));
	}

	void DivideTest_5 ()
	{
		Expression<Func<int, MyType, int>> e5 = (int a, MyType b) => a / b;
		AssertNodeType (e5, ExpressionType.Divide);
		Assert (50, e5.Compile ().Invoke (100, new MyType (2)));
	}

	void DivideTest_6 ()
	{
		Expression<Func<int, MyType?, int?>> e6 = (int a, MyType? b) => a / b;
		AssertNodeType (e6, ExpressionType.Divide);
		Assert (50, e6.Compile ().Invoke (100, new MyType (2)));
		Assert (null, e6.Compile ().Invoke (20, null));
	}
	
	void DivideTest_7 ()
	{
		Expression<Func<float, uint?, float?>> e = (a, b) => a / b;
		AssertNodeType (e, ExpressionType.Divide);
		Assert (50, e.Compile () (100, 2));
		Assert (null, e.Compile () (20, null));
	}
	
	void EqualTest ()
	{
		Expression<Func<int, int, bool>> e = (int a, int b) => a == b;
		AssertNodeType (e, ExpressionType.Equal);
		Assert (false, e.Compile ().Invoke (60, 30));
		Assert (true, e.Compile ().Invoke (-1, -1));
	}
	
	void EqualTest_2 ()
	{
		Expression<Func<double?, double?, bool>> e2 = (a, b) => a == b;
		AssertNodeType (e2, ExpressionType.Equal);
		Assert (true, e2.Compile ().Invoke (3, 3));
		Assert (false, e2.Compile ().Invoke (3, 2));
	}

	void EqualTest_3 ()
	{
		Expression<Func<MyType, MyType, bool>> e3 = (MyType a, MyType b) => a == b;
		AssertNodeType (e3, ExpressionType.Equal);
		Assert (true, e3.Compile ().Invoke (new MyType (-20), new MyType (-20)));
	}

	void EqualTest_4 ()
	{
		Expression<Func<MyType?, MyType?, bool>> e4 = (MyType? a, MyType? b) => a == b;
		AssertNodeType (e4, ExpressionType.Equal);
		Assert (false, e4.Compile ().Invoke (null, new MyType (-20)));
		Assert (true, e4.Compile ().Invoke (null, null));
		Assert (true, e4.Compile ().Invoke (new MyType (120), new MyType (120)));
	}

	void EqualTest_5 ()
	{
		Expression<Func<bool?, bool?, bool>> e5 = (bool? a, bool? b) => a == b;
		AssertNodeType (e5, ExpressionType.Equal);
		Assert (false, e5.Compile ().Invoke (true, null));
		Assert (true, e5.Compile ().Invoke (null, null));
		Assert (true, e5.Compile ().Invoke (false, false));
	}

	void EqualTest_6 ()
	{
		Expression<Func<bool, bool>> e6 = (bool a) => a == null;
		AssertNodeType (e6, ExpressionType.Equal);
		Assert (false, e6.Compile ().Invoke (true));
		Assert (false, e6.Compile ().Invoke (false));
	}

	void EqualTest_7 ()
	{
		Expression<Func<string, string, bool>> e7 = (string a, string b) => a == b;
		AssertNodeType (e7, ExpressionType.Equal);
		Assert (true, e7.Compile ().Invoke (null, null));
		Assert (false, e7.Compile ().Invoke ("a", "A"));
		Assert (true, e7.Compile ().Invoke ("a", "a"));
	}

	void EqualTest_8 ()
	{
		Expression<Func<object, bool>> e8 = (object a) => null == a;
		AssertNodeType (e8, ExpressionType.Equal);
		Assert (true, e8.Compile ().Invoke (null));
		Assert (false, e8.Compile ().Invoke ("a"));
		Assert (false, e8.Compile ().Invoke (this));
	}

	void EqualTest_9 ()
	{
		Expression<Func<MyEnum, MyEnum, bool>> e9 = (a, b) => a == b;
		AssertNodeType (e9, ExpressionType.Equal);
		Assert (false, e9.Compile ().Invoke (MyEnum.Value_1, MyEnum.Value_2));
		Assert (true, e9.Compile ().Invoke (MyEnum.Value_2, MyEnum.Value_2));
	}

	void EqualTest_10 ()
	{
		Expression<Func<MyEnum?, MyEnum?, bool>> e10 = (a, b) => a == b;
		AssertNodeType (e10, ExpressionType.Equal);
		Assert (false, e10.Compile ().Invoke (MyEnum.Value_1, null));
		Assert (true, e10.Compile ().Invoke (MyEnum.Value_2, MyEnum.Value_2));
	}

	void EqualTest_11 ()
	{
		Expression<Func<MyEnum?, bool>> e11 = (a) => a == null;
		AssertNodeType (e11, ExpressionType.Equal);
		Assert (false, e11.Compile ().Invoke (MyEnum.Value_1));
		Assert (true, e11.Compile ().Invoke (null));
	}

	void EqualTest_12 ()
	{
		Expression<Func<MyEnumUlong, bool>> e12 = (a) => a == 0;
		AssertNodeType (e12, ExpressionType.Equal);
		Assert (false, e12.Compile ().Invoke (MyEnumUlong.Value_1));
		Assert (true, e12.Compile ().Invoke (0));
	}

	void EqualTest_13 ()
	{
		Expression<Func<MyEnum, bool>> e13 = (a) => a == MyEnum.Value_2;
		AssertNodeType (e13, ExpressionType.Equal);
		Assert (true, e13.Compile ().Invoke (MyEnum.Value_2));
		Assert (false, e13.Compile ().Invoke (0));
	}
	
	void EqualTest_14 ()
	{
		Expression<Func<MyEnum, bool>> e = (a) => a == null;
		AssertNodeType (e, ExpressionType.Equal);
		Assert (false, e.Compile ().Invoke (MyEnum.Value_1));
	}

	void EqualTest_15 ()
	{
		Expression<Func<int?, uint, bool>> e = (a, b) => a == b;
		AssertNodeType (e, ExpressionType.Equal);
		Assert (false, e.Compile ().Invoke (null, 0));
		Assert (true, e.Compile ().Invoke (4, 4));
	}

	void EqualTestDelegate ()
	{
		Expression<Func<Delegate, Delegate, bool>> e1 = (a, b) => a == b;
		AssertNodeType (e1, ExpressionType.Equal);
		Assert (true, e1.Compile ().Invoke (null, null));
	}

	void EqualTestDelegate_2 ()
	{
		EmptyDelegate ed = delegate () {};

		Expression<Func<EmptyDelegate, EmptyDelegate, bool>> e2 = (a, b) => a == b;
		AssertNodeType (e2, ExpressionType.Equal);
		Assert (false, e2.Compile ().Invoke (delegate () {}, null));
		Assert (false, e2.Compile ().Invoke (delegate () {}, delegate {}));
		Assert (false, e2.Compile ().Invoke (ed, delegate {}));
		Assert (true, e2.Compile ().Invoke (ed, ed));
	}
	
	void EqualTestDelegate_3 ()
	{
		Expression<Func<Func<int>, bool>> e1 = (a) => a == ReturnNumber;
		AssertNodeType (e1, ExpressionType.Equal);
		Assert (false, e1.Compile ().Invoke (null));
		Assert (true, e1.Compile ().Invoke (ReturnNumber));
	}

	void ExclusiveOrTest ()
	{
		Expression<Func<int, short, int>> e = (int a, short b) => a ^ b;
		AssertNodeType (e, ExpressionType.ExclusiveOr);
		Assert (34, e.Compile ().Invoke (60, 30));
	}

	void ExclusiveOrTest_2 ()
	{
		Expression<Func<byte?, byte?, int?>> e2 = (a, b) => a ^ b;
		AssertNodeType (e2, ExpressionType.ExclusiveOr);
		Assert (null, e2.Compile ().Invoke (null, 3));
		Assert (1, e2.Compile ().Invoke (3, 2));
	}

	void ExclusiveOrTest_3 ()
	{
		Expression<Func<MyType, MyType, MyType>> e3 = (MyType a, MyType b) => a ^ b;
		AssertNodeType (e3, ExpressionType.ExclusiveOr);
		Assert (0, e3.Compile ().Invoke (new MyType (-20), new MyType (-20)));
	}

	void ExclusiveOrTest_4 ()
	{
		Expression<Func<MyType?, MyType?, MyType?>> e4 = (MyType? a, MyType? b) => a ^ b;
		AssertNodeType (e4, ExpressionType.ExclusiveOr);
		Assert (null, e4.Compile ().Invoke (null, new MyType (-20)));
		Assert (new MyType (-108), e4.Compile ().Invoke (new MyType (120), new MyType (-20)));
	}

	void ExclusiveOrTest_5 ()
	{
		Expression<Func<MyType?, byte, int?>> e5 = (MyType? a, byte b) => a ^ b;
		AssertNodeType (e5, ExpressionType.ExclusiveOr);
		Assert (null, e5.Compile ().Invoke (null, 64));
		Assert (96, e5.Compile ().Invoke (new MyType (64), 32));
	}

	void ExclusiveOrTest_6 ()
	{
		Expression<Func<MyEnum, MyEnum, MyEnum>> e6 = (a, b) => a ^ b;
		AssertNodeType (e6, ExpressionType.Convert);
		Assert ((MyEnum)3, e6.Compile ().Invoke (MyEnum.Value_1, MyEnum.Value_2));
		Assert<MyEnum> (0, e6.Compile ().Invoke (MyEnum.Value_2, MyEnum.Value_2));
	}

	void ExclusiveOrTest_7 ()
	{
		Expression<Func<MyEnum?, MyEnum?, MyEnum?>> e7 = (a, b) => a ^ b;
		AssertNodeType (e7, ExpressionType.Convert);
		Assert (null, e7.Compile ().Invoke (MyEnum.Value_1, null));
		Assert<MyEnum?> (0, e7.Compile ().Invoke (MyEnum.Value_2, MyEnum.Value_2));
	}

	void ExclusiveOrTest_8 ()
	{
		Expression<Func<MyEnum?, MyEnum?>> e8 = (a) => a ^ null;
		AssertNodeType (e8, ExpressionType.Convert);
		Assert (null, e8.Compile ().Invoke (MyEnum.Value_1));
		Assert (null, e8.Compile ().Invoke (null));
	}

	void GreaterThanTest ()
	{
		Expression<Func<int, int, bool>> e = (int a, int b) => a > b;
		AssertNodeType (e, ExpressionType.GreaterThan);
		Assert (true, e.Compile ().Invoke (60, 30));
	}

	void GreaterThanTest_2 ()
	{
		Expression<Func<uint?, byte?, bool>> e2 = (a, b) => a > b;
		AssertNodeType (e2, ExpressionType.GreaterThan);
		Assert (false, e2.Compile ().Invoke (null, 3));
		Assert (false, e2.Compile ().Invoke (2, 2));
	}

	void GreaterThanTest_3 ()
	{
		Expression<Func<MyType, MyType, bool>> e3 = (MyType a, MyType b) => a > b;
		AssertNodeType (e3, ExpressionType.GreaterThan);
		Assert (false, e3.Compile ().Invoke (new MyType (-20), new MyType (-20)));
	}

	void GreaterThanTest_4 ()
	{
		Expression<Func<MyType?, MyType?, bool>> e4 = (MyType? a, MyType? b) => a > b;
		AssertNodeType (e4, ExpressionType.GreaterThan);
		Assert (false, e4.Compile ().Invoke (null, new MyType (-20)));
		Assert (false, e4.Compile ().Invoke (null, null));
		Assert (true, e4.Compile ().Invoke (new MyType (120), new MyType (-20)));
	}

	void GreaterThanTest_5 ()
	{
		Expression<Func<MyType?, sbyte, bool>> e5 = (MyType? a, sbyte b) => a > b;
		AssertNodeType (e5, ExpressionType.GreaterThan);
		Assert (false, e5.Compile ().Invoke (null, 33));
		Assert (false, e5.Compile ().Invoke (null, 0));
		Assert (true, e5.Compile ().Invoke (new MyType (120), 3));
	}

	void GreaterThanTest_6 ()
	{
		Expression<Func<ushort, bool>> e6 = (ushort a) => a > null;
		AssertNodeType (e6, ExpressionType.GreaterThan);
		Assert (false, e6.Compile ().Invoke (60));
	}

	void GreaterThanTest_7 ()
	{
		Expression<Func<MyEnum, MyEnum, bool>> e7 = (a, b) => a > b;
		AssertNodeType (e7, ExpressionType.GreaterThan);
		Assert (true, e7.Compile ().Invoke (MyEnum.Value_2, MyEnum.Value_1));
		Assert (false, e7.Compile ().Invoke (MyEnum.Value_2, MyEnum.Value_2));
	}

	void GreaterThanTest_8 ()
	{
		Expression<Func<MyEnum?, MyEnum?, bool>> e8 = (a, b) => a > b;
		AssertNodeType (e8, ExpressionType.GreaterThan);
		Assert (false, e8.Compile ().Invoke (MyEnum.Value_1, null));
		Assert (false, e8.Compile ().Invoke (MyEnum.Value_2, MyEnum.Value_2));
	}

	void GreaterThanOrEqualTest ()
	{
		Expression<Func<int, int, bool>> e = (int a, int b) => a >= b;
		AssertNodeType (e, ExpressionType.GreaterThanOrEqual);
		Assert (true, e.Compile ().Invoke (60, 30));
	}

	void GreaterThanOrEqualTest_2 ()
	{
		Expression<Func<byte?, byte?, bool>> e2 = (a, b) => a >= b;
		AssertNodeType (e2, ExpressionType.GreaterThanOrEqual);
		Assert (false, e2.Compile ().Invoke (null, 3));
		Assert (true, e2.Compile ().Invoke (2, 2));
	}

	void GreaterThanOrEqualTest_3 ()
	{
		Expression<Func<MyType, MyType, bool>> e3 = (MyType a, MyType b) => a >= b;
		AssertNodeType (e3, ExpressionType.GreaterThanOrEqual);
		Assert (true, e3.Compile ().Invoke (new MyType (-20), new MyType (-20)), "D1");
	}

	void GreaterThanOrEqualTest_4 ()
	{
		Expression<Func<MyType?, MyType?, bool>> e4 = (MyType? a, MyType? b) => a >= b;
		AssertNodeType (e4, ExpressionType.GreaterThanOrEqual);
		Assert (false, e4.Compile ().Invoke (null, new MyType (-20)));
		Assert (false, e4.Compile ().Invoke (null, null));
		Assert (true, e4.Compile ().Invoke (new MyType (120), new MyType (-20)));
	}

	void GreaterThanOrEqualTest_5 ()
	{
		Expression<Func<MyType?, sbyte, bool>> e5 = (MyType? a, sbyte b) => a >= b;
		AssertNodeType (e5, ExpressionType.GreaterThanOrEqual);
		Assert (false, e5.Compile ().Invoke (null, 33));
		Assert (false, e5.Compile ().Invoke (null, 0));
		Assert (true, e5.Compile ().Invoke (new MyType (120), 3));
	}

	void GreaterThanOrEqualTest_6 ()
	{
		Expression<Func<ushort, bool>> e6 = (ushort a) => a >= null;
		AssertNodeType (e6, ExpressionType.GreaterThanOrEqual);
		Assert (false, e6.Compile ().Invoke (60));
	}

	void GreaterThanOrEqualTest_7 ()
	{
		Expression<Func<MyEnum, MyEnum, bool>> e7 = (a, b) => a >= b;
		AssertNodeType (e7, ExpressionType.GreaterThanOrEqual);
		Assert (true, e7.Compile ().Invoke (MyEnum.Value_2, MyEnum.Value_1));
		Assert (true, e7.Compile ().Invoke (MyEnum.Value_2, MyEnum.Value_2));
	}

	void GreaterThanOrEqualTest_8 ()
	{
		Expression<Func<MyEnum?, MyEnum?, bool>> e8 = (a, b) => a >= b;
		AssertNodeType (e8, ExpressionType.GreaterThanOrEqual);
		Assert (false, e8.Compile ().Invoke (MyEnum.Value_1, null));
		Assert (true, e8.Compile ().Invoke (MyEnum.Value_2, MyEnum.Value_2));
	}
	
	void InvokeTest ()
	{
		var del = new IntDelegate (TestInt);
		Expression<Func<IntDelegate, int>> e = (a) => a ();
		AssertNodeType (e, ExpressionType.Invoke);
		Assert (29, e.Compile ().Invoke (del));
	}
	
	void InvokeTest_2 ()
	{
		Expression<Func<Func<int, string>, int, string>> e2 = (a, b) => a (b);
		AssertNodeType (e2, ExpressionType.Invoke);
		Assert ("4", e2.Compile ().Invoke ((a) => (a+1).ToString (), 3));
	}
	
	void LambdaTest ()
	{
		Expression<Func<string, Func<string>>> e = (string s) => () => s;
		AssertNodeType (e, ExpressionType.Lambda);
		Assert ("xx", e.Compile ().Invoke ("xx") ());
	}
	
	void LeftShiftTest ()
	{
		Expression<Func<ulong, short, ulong>> e = (ulong a, short b) => a << b;
		AssertNodeType (e, ExpressionType.LeftShift);
		Assert ((ulong) 0x7F000, e.Compile ().Invoke (0xFE, 11));
		Assert ((ulong) 0x1FFFFFFFE, e.Compile ().Invoke (0xFFFFFFFF, 0xA01));

		// .net produces a strange result
		// see https://bugzilla.novell.com/show_bug.cgi?id=398358
		// Assert ((ulong) 0xFFFFFFFE00000000, e.Compile ().Invoke (0xFFFFFFFF, 0xA01));
	}
	
	void LeftShiftTest_2 ()
	{
		Expression<Func<MyType, MyType, int>> e2 = (MyType a, MyType b) => a << b;
		AssertNodeType (e2, ExpressionType.LeftShift);
		var c2 = e2.Compile ();
		Assert (1024, c2 (new MyType (256), new MyType (2)));
	}
	
	void LeftShiftTest_3 ()
	{
		Expression<Func<long?, sbyte, long?>> e3 = (long? a, sbyte b) => a << b;
		AssertNodeType (e3, ExpressionType.LeftShift);
		Assert (null, e3.Compile ().Invoke (null, 11));
		Assert (2048, e3.Compile ().Invoke (1024, 1));
	}
	
	void LeftShiftTest_4 ()
	{
		Expression<Func<MyType?, MyType?, int?>> e4 = (MyType? a, MyType? b) => a << b;
		AssertNodeType (e4, ExpressionType.LeftShift);
		var c4 = e4.Compile ();
		Assert (null, c4 (new MyType (8), null));
		Assert (null, c4 (null, new MyType (8)));
		Assert (1024, c4 (new MyType (256), new MyType (2)));
	}
	
	void LeftShiftTest_5 ()
	{
		Expression<Func<ushort, int?>> e5 = (ushort a) => a << null;
		AssertNodeType (e5, ExpressionType.LeftShift);
		Assert (null, e5.Compile ().Invoke (30));
	}
	
	void LeftShiftTest_6 ()
	{
		Expression<Func<int, MyTypeImplicitOnly, int>> e = (a, b) => a << b;
		AssertNodeType (e, ExpressionType.LeftShift);
		Assert (0x7F0, e.Compile ().Invoke (0xFE, new MyTypeImplicitOnly (3)));
	}
	
	void LessThanTest ()
	{
		Expression<Func<int, int, bool>> e = (int a, int b) => a < b;
		AssertNodeType (e, ExpressionType.LessThan);
		Assert (false, e.Compile ().Invoke (60, 30));
	}
	
	void LessThanTest_2 ()
	{
		Expression<Func<uint?, byte?, bool>> e2 = (a, b) => a < b;
		AssertNodeType (e2, ExpressionType.LessThan);
		Assert (false, e2.Compile ().Invoke (null, 3));
		Assert (false, e2.Compile ().Invoke (2, 2));
	}
	
	void LessThanTest_3 ()
	{
		Expression<Func<MyType, MyType, bool>> e3 = (MyType a, MyType b) => a < b;
		AssertNodeType (e3, ExpressionType.LessThan);
		Assert (false, e3.Compile ().Invoke (new MyType (-20), new MyType (-20)));
	}
	
	void LessThanTest_4 ()
	{
		Expression<Func<MyType?, MyType?, bool>> e4 = (MyType? a, MyType? b) => a < b;
		AssertNodeType (e4, ExpressionType.LessThan);
		Assert (false, e4.Compile ().Invoke (null, new MyType (-20)));
		Assert (false, e4.Compile ().Invoke (null, null));
		Assert (false, e4.Compile ().Invoke (new MyType (120), new MyType (-20)));
	}
	
	void LessThanTest_5 ()
	{
		Expression<Func<MyType?, sbyte, bool>> e5 = (MyType? a, sbyte b) => a < b;
		AssertNodeType (e5, ExpressionType.LessThan);
		Assert (false, e5.Compile ().Invoke (null, 33));
		Assert (false, e5.Compile ().Invoke (null, 0));
		Assert (false, e5.Compile ().Invoke (new MyType (120), 3));
	}
	
	void LessThanTest_6 ()
	{
		Expression<Func<ushort, bool>> e6 = (ushort a) => a < null;
		AssertNodeType (e6, ExpressionType.LessThan);
		Assert (false, e6.Compile ().Invoke (60));
	}
	
	void LessThanTest_7 ()
	{
		Expression<Func<MyEnum, MyEnum, bool>> e7 = (a, b) => a < b;
		AssertNodeType (e7, ExpressionType.LessThan);
		Assert (false, e7.Compile ().Invoke (MyEnum.Value_2, MyEnum.Value_1));
		Assert (false, e7.Compile ().Invoke (MyEnum.Value_2, MyEnum.Value_2));
	}
	
	void LessThanTest_8 ()
	{
		Expression<Func<MyEnum?, MyEnum?, bool>> e8 = (a, b) => a < b;
		AssertNodeType (e8, ExpressionType.LessThan);
		Assert (false, e8.Compile ().Invoke (MyEnum.Value_1, null));
		Assert (false, e8.Compile ().Invoke (MyEnum.Value_2, MyEnum.Value_2));
	}

	void LessThanOrEqualTest ()
	{
		Expression<Func<int, int, bool>> e = (int a, int b) => a <= b;
		AssertNodeType (e, ExpressionType.LessThanOrEqual);
		Assert (false, e.Compile ().Invoke (60, 30));
	}

	void LessThanOrEqualTest_2 ()
	{
		Expression<Func<byte?, byte?, bool>> e2 = (a, b) => a <= b;
		AssertNodeType (e2, ExpressionType.LessThanOrEqual);
		Assert (false, e2.Compile ().Invoke (null, 3));
		Assert (true, e2.Compile ().Invoke (2, 2));
	}

	void LessThanOrEqualTest_3 ()
	{
		Expression<Func<MyType, MyType, bool>> e3 = (MyType a, MyType b) => a <= b;
		AssertNodeType (e3, ExpressionType.LessThanOrEqual);
		Assert (true, e3.Compile ().Invoke (new MyType (-20), new MyType (-20)));
	}

	void LessThanOrEqualTest_4 ()
	{
		Expression<Func<MyType?, MyType?, bool>> e4 = (MyType? a, MyType? b) => a <= b;
		AssertNodeType (e4, ExpressionType.LessThanOrEqual);
		Assert (false, e4.Compile ().Invoke (null, new MyType (-20)));
		Assert (false, e4.Compile ().Invoke (null, null));
		Assert (false, e4.Compile ().Invoke (new MyType (120), new MyType (-20)));
	}

	void LessThanOrEqualTest_5 ()
	{
		Expression<Func<MyType?, sbyte, bool>> e5 = (MyType? a, sbyte b) => a <= b;
		AssertNodeType (e5, ExpressionType.LessThanOrEqual);
		Assert (false, e5.Compile ().Invoke (null, 33));
		Assert (false, e5.Compile ().Invoke (null, 0));
		Assert (false, e5.Compile ().Invoke (new MyType (120), 3));
	}

	void LessThanOrEqualTest_6 ()
	{
		Expression<Func<ushort, bool>> e6 = (ushort a) => a <= null;
		AssertNodeType (e6, ExpressionType.LessThanOrEqual);
		Assert (false, e6.Compile ().Invoke (60));
	}

	void LessThanOrEqualTest_7 ()
	{
		Expression<Func<MyEnum, MyEnum, bool>> e7 = (a, b) => a <= b;
		AssertNodeType (e7, ExpressionType.LessThanOrEqual);
		Assert (false, e7.Compile ().Invoke (MyEnum.Value_2, MyEnum.Value_1));
		Assert (true, e7.Compile ().Invoke (MyEnum.Value_2, MyEnum.Value_2));
	}

	void LessThanOrEqualTest_8 ()
	{
		Expression<Func<MyEnum?, MyEnum?, bool>> e8 = (a, b) => a <= b;
		AssertNodeType (e8, ExpressionType.LessThanOrEqual);
		Assert (false, e8.Compile ().Invoke (MyEnum.Value_1, null));
		Assert (true, e8.Compile ().Invoke (MyEnum.Value_2, MyEnum.Value_2));
	}
	
	void ListInitTest ()
	{
		Expression<Func<List<object>>> e1 = () => new List<object> { "Hello", "", null, "World", 5 };
		AssertNodeType (e1, ExpressionType.ListInit);
		var re1 = e1.Compile ().Invoke ();
		Assert (null, re1 [2]);
		Assert ("World", re1 [3]);
		Assert (5, re1 [4]);
	}
	
	void ListInitTest_2 ()
	{
		Expression<Func<int, Dictionary<string, int>>> e2 = (int value) => new Dictionary<string, int> (3) { { "A", value }, { "B", 2 } };
		AssertNodeType (e2, ExpressionType.ListInit);
		var re2 = e2.Compile ().Invoke (3456);
		Assert (3456, re2 ["A"]);
	}
	
	void MemberAccessTest ()
	{
		MemberAccessData d = new MemberAccessData ();
		d.BoolValue = true;
		Expression<Func<bool>> e = () => d.BoolValue;
		AssertNodeType (e, ExpressionType.MemberAccess);
		Assert (true, e.Compile ().Invoke ());
		d.BoolValue = false;
		Assert (false, e.Compile ().Invoke ());
	}
	
	void MemberAccessTest_2 ()
	{
		Expression<Func<decimal>> e2 = () => MemberAccessData.DecimalValue;
		AssertNodeType (e2, ExpressionType.MemberAccess);
		Assert (decimal.MinValue, e2.Compile ().Invoke ());
	}
	
	void MemberAccessTest_3 ()
	{
		MemberAccessData d = new MemberAccessData ();
		d.VolatileValue = 492;
		Expression<Func<uint>> e3 = () => d.VolatileValue;
		AssertNodeType (e3, ExpressionType.MemberAccess);
		Assert<uint> (492, e3.Compile ().Invoke ());
	}
	
	void MemberAccessTest_4 ()
	{
		MemberAccessData d = new MemberAccessData ();	
		Expression<Func<string[]>> e4 = () => d.StringValues;
		AssertNodeType (e4, ExpressionType.MemberAccess);
		Assert (null, e4.Compile ().Invoke ());
	}
	
	void MemberAccessTest_5 ()
	{
		MemberAccessData d = new MemberAccessData ();	
		var e5 = d.GetEvent ();
		AssertNodeType (e5, ExpressionType.MemberAccess);
		Assert (null, e5.Compile ().Invoke ());
	}
	
	void MemberAccessTest_6 ()
	{
		MemberAccessData d = new MemberAccessData ();	
		Expression<Func<MyType>> e6 = () => d.MyTypeProperty;
		AssertNodeType (e6, ExpressionType.MemberAccess);
		Assert (new MyType (), e6.Compile ().Invoke ());
	}
	
	void MemberAccessTest_7 ()
	{
		MemberAccessData d = new MemberAccessData ();	
		Expression<Func<MyType, short>> e7 = a => a.ShortProp;
		AssertNodeType (e7, ExpressionType.MemberAccess);
		MyType mt = new MyType ();
		mt.ShortProp = 124;
		Assert (124, e7.Compile ().Invoke (mt));
	}
	
	void MemberAccessTest_8 ()
	{
		Expression<Func<string>> e8 = () => MemberAccessData.StaticProperty;
		AssertNodeType (e8, ExpressionType.MemberAccess);
		Assert ("alo", e8.Compile ().Invoke ());
	}
	
	void MemberAccessTest_9 ()
	{
		string s = "localvar";
		Expression<Func<string>> e9 = () => s;
		s = "changed";

		AssertNodeType (e9, ExpressionType.MemberAccess);
		Assert ("changed", e9.Compile ().Invoke ());
	}
	
	void MemberInitTest ()
	{
		Expression<Func<MemberAccessData>> e = () => new MemberAccessData { 
			VolatileValue = 2, StringValues = new string [] { "sv" }, MyTypeProperty = new MyType (692)
		};
		AssertNodeType (e, ExpressionType.MemberInit);
		var r1 = e.Compile ().Invoke ();
		Assert<uint> (2, r1.VolatileValue);
		Assert (new string[] { "sv" }, r1.StringValues);
		Assert (new MyType (692), r1.MyTypeProperty);
	}
	
	void MemberInitTest_2 ()
	{
		Expression<Func<MemberAccessData>> e2 = () => new MemberAccessData {
			ListValues = new List<string> { "a", null }
		};

		AssertNodeType (e2, ExpressionType.MemberInit);
		var r2 = e2.Compile ().Invoke ();
		Assert ("a", r2.ListValues [0]);
	}
	
	void MemberInitTest_3 ()
	{
		Expression<Func<short, MyType>> e3 = a => new MyType { ShortProp = a };
		AssertNodeType (e3, ExpressionType.MemberInit);
		var r3 = e3.Compile ().Invoke (33);
		Assert (33, r3.ShortProp);
	}
	
	void MemberInitTest_4 ()
	{
		Expression<Func<int>> e = () => new int { };
		
		AssertNodeType (e, ExpressionType.MemberInit);
		var r = e.Compile ().Invoke ();
		Assert (0, r);
	}	

	void ModuloTest ()
	{
		Expression<Func<int, int, int>> e = (int a, int b) => a % b;
		AssertNodeType (e, ExpressionType.Modulo);
		Assert (29, e.Compile ().Invoke (60, 31));
	}	

	void ModuloTest_2 ()
	{
		Expression<Func<double?, double?, double?>> e2 = (a, b) => a % b;
		AssertNodeType (e2, ExpressionType.Modulo);
		Assert (null, e2.Compile ().Invoke (null, 3));
		Assert (1.1, e2.Compile ().Invoke (3.1, 2));
	}	

	void ModuloTest_3 ()
	{
		Expression<Func<MyType, MyType, MyType>> e3 = (MyType a, MyType b) => a % b;
		AssertNodeType (e3, ExpressionType.Modulo);
		Assert (0, e3.Compile ().Invoke (new MyType (-20), new MyType (-20)));
	}	

	void ModuloTest_4 ()
	{
		Expression<Func<MyType?, MyType?, MyType?>> e4 = (MyType? a, MyType? b) => a % b;
		AssertNodeType (e4, ExpressionType.Modulo);
		Assert (null, e4.Compile ().Invoke (null, new MyType (-20)));
		Assert (new MyType (12), e4.Compile ().Invoke (new MyType (12), new MyType (-20)));
	}	

	void ModuloTest_5 ()
	{
		Expression<Func<int, MyType, int>> e5 = (int a, MyType b) => a % b;
		AssertNodeType (e5, ExpressionType.Modulo);
		Assert (1, e5.Compile ().Invoke (99, new MyType (2)));
	}	

	void ModuloTest_6 ()
	{
		Expression<Func<int, MyType?, int?>> e6 = (int a, MyType? b) => a % b;
		AssertNodeType (e6, ExpressionType.Modulo);
		Assert (100, e6.Compile ().Invoke (100, new MyType (200)));
		Assert (null, e6.Compile ().Invoke (20, null));
	}	

	void ModuloTest_7 ()
	{
		Expression<Func<ushort, int?>> e7 = (ushort a) => a % null;
		AssertNodeType (e7, ExpressionType.Modulo);
		Assert (null, e7.Compile ().Invoke (60));
	}
	
	void MultiplyTest ()
	{
		Expression<Func<int, int, int>> e = (int a, int b) => a * b;
		AssertNodeType (e, ExpressionType.Multiply);
		Assert (1860, e.Compile ().Invoke (60, 31));
		Assert (2147483617, e.Compile ().Invoke (int.MaxValue, 31));
	}
	
	void MultiplyTest_2 ()
	{
		Expression<Func<double?, double?, double?>> e2 = (a, b) => a * b;
		AssertNodeType (e2, ExpressionType.Multiply);
		Assert (null, e2.Compile ().Invoke (null, 3));
		Assert (6.2, e2.Compile ().Invoke (3.1, 2));
	}
	
	void MultiplyTest_3 ()
	{
		Expression<Func<MyType, MyType, MyType>> e3 = (MyType a, MyType b) => a * b;
		AssertNodeType (e3, ExpressionType.Multiply);
		Assert (400, e3.Compile ().Invoke (new MyType (-20), new MyType (-20)));
	}
	
	void MultiplyTest_4 ()
	{
		Expression<Func<MyType?, MyType?, MyType?>> e4 = (MyType? a, MyType? b) => a * b;
		AssertNodeType (e4, ExpressionType.Multiply);
		Assert (null, e4.Compile ().Invoke (null, new MyType (-20)));
		Assert (new MyType (-240), e4.Compile ().Invoke (new MyType (12), new MyType (-20)));
	}
	
	void MultiplyTest_5 ()
	{
		Expression<Func<int, MyType, int>> e5 = (int a, MyType b) => a * b;
		AssertNodeType (e5, ExpressionType.Multiply);
		Assert (198, e5.Compile ().Invoke (99, new MyType (2)));
	}
	
	void MultiplyTest_6 ()
	{
		Expression<Func<int, MyType?, int?>> e6 = (int a, MyType? b) => a * b;
		AssertNodeType (e6, ExpressionType.Multiply);
		Assert (0, e6.Compile ().Invoke (int.MinValue, new MyType (200)));
		Assert (null, e6.Compile ().Invoke (20, null));
	}
	
	void MultiplyTest_7 ()
	{
		Expression<Func<ushort, int?>> e7 = (ushort a) => a * null;
		AssertNodeType (e7, ExpressionType.Multiply);
		Assert (null, e7.Compile ().Invoke (60));
	}
	
	void MultiplyTest_8 ()
	{
		Expression<Func<double, ulong?, double?>> e = (a, b) => a * b;
		AssertNodeType (e, ExpressionType.Multiply);
		Assert (180, e.Compile () (60, 3));
		Assert (null, e.Compile () (60, null));
	}
	
	void MultiplyCheckedTest ()
	{
		checked {
			Expression<Func<int, int, int>> e = (int a, int b) => a * b;
			AssertNodeType (e, ExpressionType.MultiplyChecked);
			try {
				e.Compile ().Invoke (int.MaxValue, 309);
				throw new ApplicationException ("MultiplyCheckedTest #1");
			} catch (OverflowException) { }
		}
	}
	
	void MultiplyCheckedTest_2 ()
	{
		checked {
			Expression<Func<byte?, byte?, int?>> e2 = (a, b) => a * b;
			AssertNodeType (e2, ExpressionType.MultiplyChecked);
			Assert (null, e2.Compile ().Invoke (null, 3));
			Assert (14025, e2.Compile ().Invoke (byte.MaxValue, 55));
		}
	}
	
	void MultiplyCheckedTest_3 ()
	{
		checked {
			Expression<Func<MyType, MyType, MyType>> e3 = (MyType a, MyType b) => a * b;
			AssertNodeType (e3, ExpressionType.Multiply);
			Assert (-600, e3.Compile ().Invoke (new MyType (-20), new MyType (30)));
		}
	}
	
	void MultiplyCheckedTest_4 ()
	{
		checked {
			Expression<Func<double, double, double>> e4 = (a, b) => a * b;
			AssertNodeType (e4, ExpressionType.Multiply);
			Assert (double.PositiveInfinity, e4.Compile ().Invoke (double.MaxValue, int.MaxValue));
		}
	}
	
	void MultiplyCheckedTest_5 ()
	{
		checked {
			Expression<Func<float?, float?, float?>> e5 = (a, b) => b * a;
			AssertNodeType (e5, ExpressionType.MultiplyChecked);
			Assert (float.PositiveInfinity, e5.Compile ().Invoke (float.Epsilon, float.PositiveInfinity));
		}
	}
	
	void NegateTest ()
	{
		Expression<Func<int, int>> e = (a) => -a;
		AssertNodeType (e, ExpressionType.Negate);
		Assert (30, e.Compile ().Invoke (-30));
	}
	
	void NegateTest_2 ()
	{
		Expression<Func<sbyte, int>> e2 = (a) => -(-a);
		AssertNodeType (e2, ExpressionType.Negate);
		Assert (-10, e2.Compile ().Invoke (-10));
	}
	
	void NegateTest_3 ()
	{
		Expression<Func<long?, long?>> e3 = (a) => -a;
		AssertNodeType (e3, ExpressionType.Negate);
		Assert (long.MinValue + 1, e3.Compile ().Invoke (long.MaxValue));
		Assert (null, e3.Compile ().Invoke (null));
	}
	
	void NegateTest_4 ()
	{
		Expression<Func<MyType, MyType>> e4 = (a) => -a;
		AssertNodeType (e4, ExpressionType.Negate);
		Assert (new MyType (14), e4.Compile ().Invoke (new MyType (-14)));
	}
	
	void NegateTest_5 ()
	{
		Expression<Func<MyType?, MyType?>> e5 = (a) => -a;
		AssertNodeType (e5, ExpressionType.Negate);
		Assert (new MyType (-33), e5.Compile ().Invoke (new MyType (33)));
		Assert (null, e5.Compile ().Invoke (null));
	}
	
	void NegateTest_6 ()
	{
		Expression<Func<MyTypeImplicitOnly, int>> e6 = (MyTypeImplicitOnly a) => -a;
		AssertNodeType (e6, ExpressionType.Negate);
		Assert (-4, e6.Compile ().Invoke (new MyTypeImplicitOnly (4)));
	}
	
	void NegateTest_7 ()
	{
		Expression<Func<MyTypeImplicitOnly?, int?>> e7 = (MyTypeImplicitOnly? a) => -a;
		AssertNodeType (e7, ExpressionType.Negate);
		Assert (-46, e7.Compile ().Invoke (new MyTypeImplicitOnly (46)));
		
		// Another version of MS bug when predefined conversion is required on nullable user operator
		// Assert (null, e7.Compile ().Invoke (null));
	}
	
	void NegateTest_8 ()
	{
		Expression<Func<sbyte?, int?>> e8 = (a) => -a;
		AssertNodeType (e8, ExpressionType.Negate);
		Assert (11, e8.Compile ().Invoke (-11));
	}
	
	void NegateTest_9 ()
	{
		Expression<Func<uint, long>> e9 = (a) => -a;
		AssertNodeType (e9, ExpressionType.Negate);
		Assert (-2, e9.Compile ().Invoke (2));		
	}
	
	void NegateTestChecked ()
	{
		checked {
			Expression<Func<int, int>> e = (int a) => -a;
			AssertNodeType (e, ExpressionType.NegateChecked);
			try {
				e.Compile ().Invoke (int.MinValue);
				throw new ApplicationException ("NegateTestChecked #1");
			} catch (OverflowException) { }
		}
	}
	
	void NegateTestChecked_2 ()
	{
		checked {
			Expression<Func<byte?, int?>> e2 = (a) => -a;
			AssertNodeType (e2, ExpressionType.NegateChecked);
			Assert (null, e2.Compile ().Invoke (null));
			Assert (-255, e2.Compile ().Invoke (byte.MaxValue));
		}
	}
	
	void NegateTestChecked_3 ()
	{
		checked {
			Expression<Func<MyType, MyType>> e3 = (MyType a) => -a;
			AssertNodeType (e3, ExpressionType.Negate);
			Assert (20, e3.Compile ().Invoke (new MyType (-20)));
		}
	}
	
	void NegateTestChecked_4 ()
	{
		checked {
			Expression<Func<double, double>> e4 = (a) => -a;
			AssertNodeType (e4, ExpressionType.Negate);
			Assert (double.NegativeInfinity, e4.Compile ().Invoke (double.PositiveInfinity));
		}
	}	
	
	void NewArrayInitTest ()
	{
		Expression<Func<int []>> e = () => new int [1] { 5 };
		AssertNodeType (e, ExpressionType.NewArrayInit);
		Assert (new int [1] { 5 }, e.Compile ().Invoke ());
	}	
	
	void NewArrayInitTest_2 ()
	{
		Expression<Func<int []>> e1 = () => new int [] { };
		AssertNodeType (e1, ExpressionType.NewArrayInit);
		Assert (new int [0], e1.Compile ().Invoke ());
	}	
	
	void NewArrayInitTest_3 ()
	{
		Expression<Func<ushort, ulong? []>> e2 = (ushort a) => new ulong? [] { a };
		AssertNodeType (e2, ExpressionType.NewArrayInit);
		Assert (new ulong? [1] { ushort.MaxValue }, e2.Compile ().Invoke (ushort.MaxValue));
	}	
	
	void NewArrayInitTest_4 ()
	{
		Expression<Func<char [] []>> e3 = () => new char [] [] { new char [] { 'a' } };
		AssertNodeType (e3, ExpressionType.NewArrayInit);
		Assert (new char [] { 'a' }, e3.Compile ().Invoke () [0]);
	}
	
	void NewArrayInitTest_5 ()
	{
		Expression<Func<int?[]>> e = () => new int?[] { null, 3, 4 };
		AssertNodeType (e, ExpressionType.NewArrayInit);
		Assert (3, e.Compile ().Invoke ().Length);
	}

	void NewArrayInitTest_6 ()
	{
		Expression<Func<string []>> e = () => new [] { null, "a" };
		AssertNodeType (e, ExpressionType.NewArrayInit);
		Assert (2, e.Compile ().Invoke ().Length);
	}
	
	void NewArrayBoundsTest ()
	{
		Expression<Func<int [,]>> e = () => new int [2,3];
		AssertNodeType (e, ExpressionType.NewArrayBounds);
		Assert (new int [2,3].Length, e.Compile ().Invoke ().Length);
	}
	
	void NewArrayBoundsTest_2 ()
	{
		Expression<Func<int[,]>> e2 = () => new int [0,0];
		AssertNodeType (e2, ExpressionType.NewArrayBounds);
		Assert (new int [0, 0].Length, e2.Compile ().Invoke ().Length);
	}
	
	void NewArrayBoundsTest_3 ()
	{
		Expression<Func<int []>> e = () => new int [0];
		AssertNodeType (e, ExpressionType.NewArrayBounds);
		Assert (0, e.Compile ().Invoke ().Length);
	}

	void NewArrayBoundsTest_4 ()
	{
		const ulong max = ulong.MaxValue;
		
		Expression<Func<bool[]>> e = () => new bool [max];
		AssertNodeType (e, ExpressionType.NewArrayBounds);
	}
	
	void NewTest ()
	{
		Expression<Func<MyType>> e = () => new MyType (2);
		AssertNodeType (e, ExpressionType.New);
		Assert (new MyType (2), e.Compile ().Invoke ());
	}	
	
	void NewTest_2 ()
	{
		Expression<Func<MyType>> e2 = () => new MyType ();
		AssertNodeType (e2, ExpressionType.New);
		Assert (new MyType (), e2.Compile ().Invoke ());
	}	
	
	void NewTest_3 ()
	{
		Expression<Func<NewTest<bool>>> e3 = () => new NewTest<bool> (true);
		AssertNodeType (e3, ExpressionType.New);
		Assert (new NewTest<bool> (true), e3.Compile ().Invoke ());
	}	
	
	void NewTest_4 ()
	{
		Expression<Func<decimal, NewTest<decimal>>> e4 = (decimal d) => new NewTest<decimal> (1, 5, d);
		AssertNodeType (e4, ExpressionType.New);
		Assert (new NewTest<decimal> (1, 5, -9), e4.Compile ().Invoke (-9));
	}	
	
	void NewTest_5 ()
	{
		Expression<Func<object>> e5 = () => new { A = 9, Value = "a" };
		AssertNodeType (e5, ExpressionType.New);
		var ne = ((NewExpression) e5.Body);

		Assert (2, ne.Members.Count, "members count");	
	
		// Behaviour is different between .NET 3.5 and .NET 4.0
		if (ne.Members [0].MemberType == MemberTypes.Property) {
			Assert ("A", ne.Members [0].Name, "Name #1");
			Assert ("Value", ne.Members [1].Name, "Name #2");
		} else {
			Assert ("get_A", ne.Members [0].Name, "Name #1");
			Assert ("get_Value", ne.Members [1].Name, "Name #2");
		}
		
		Assert (new { A = 9, Value = "a" }, e5.Compile ().Invoke ());
	}
	
	void NewTest_6 ()
	{
		Expression<Func<object>> e5 = () => new { A = 9, Value = new MyType (5) };
		AssertNodeType (e5, ExpressionType.New);
	}	

	// CSC bug: emits new MyEnum as a constant	
	void NewTest_7 ()
	{
		Expression<Func<MyEnum>> e = () => new MyEnum ();
		AssertNodeType (e, ExpressionType.New);
		Assert<MyEnum> (0, e.Compile ().Invoke ());
	}

	void NotTest ()
	{
		Expression<Func<bool, bool>> e = (bool a) => !a;
		AssertNodeType (e, ExpressionType.Not);
		Assert (false, e.Compile ().Invoke (true));
	}	

	void NotTest_2 ()
	{
		Expression<Func<MyType, bool>> e2 = (MyType a) => !a;
		AssertNodeType (e2, ExpressionType.Not);
		Assert (true, e2.Compile ().Invoke (new MyType (1)));
		Assert (false, e2.Compile ().Invoke (new MyType (-1)));
	}	

	void NotTest_3 ()
	{
		Expression<Func<int, int>> e3 = (int a) => ~a;
		AssertNodeType (e3, ExpressionType.Not);
		Assert (-8, e3.Compile ().Invoke (7));
	}	

	void NotTest_4 ()
	{
		Expression<Func<MyType, int>> e4 = (MyType a) => ~a;
		AssertNodeType (e4, ExpressionType.Not);
		Assert (0, e4.Compile ().Invoke (new MyType (-1)));
	}	

	void NotTest_5 ()
	{
		Expression<Func<ulong, ulong>> e5 = (ulong a) => ~a;
		AssertNodeType (e5, ExpressionType.Not);
		Assert<ulong> (18446744073709551608, e5.Compile ().Invoke (7));
	}	

	void NotTest_6 ()
	{
		Expression<Func<MyEnum, MyEnum>> e6 = (MyEnum a) => ~a;
		AssertNodeType (e6, ExpressionType.Convert);
		Assert ((MyEnum)254, e6.Compile ().Invoke (MyEnum.Value_1));
	}

	void NotNullableTest ()
	{
		Expression<Func<bool?, bool?>> e = (bool? a) => !a;
		AssertNodeType (e, ExpressionType.Not);
		Assert (false, e.Compile ().Invoke (true));
		Assert (null, e.Compile ().Invoke (null));
	}

	void NotNullableTest_2 ()
	{
		Expression<Func<MyType?, bool?>> e2 = (MyType? a) => !a;
		AssertNodeType (e2, ExpressionType.Not);
		Assert (true, e2.Compile ().Invoke (new MyType (1)));
		Assert (null, e2.Compile ().Invoke (null));
	}

	void NotNullableTest_3 ()
	{
		Expression<Func<sbyte?, int?>> e3 = (sbyte? a) => ~a;
		AssertNodeType (e3, ExpressionType.Not);
		Assert (-5, e3.Compile ().Invoke (4));
		Assert (null, e3.Compile ().Invoke (null));
	}

	void NotNullableTest_4 ()
	{
		Expression<Func<MyType?, int?>> e4 = (MyType? a) => ~a;
		AssertNodeType (e4, ExpressionType.Not);
		Assert (0, e4.Compile ().Invoke (new MyType (-1)));
		Assert (null, e4.Compile ().Invoke (null));
	}

	void NotNullableTest_5 ()
	{
		Expression<Func<MyEnum?, MyEnum?>> e5 = (MyEnum? a) => ~a;
		AssertNodeType (e5, ExpressionType.Convert);
		Assert ((MyEnum) 254, e5.Compile ().Invoke (MyEnum.Value_1));
		Assert (null, e5.Compile ().Invoke (null));
	}

	void NotEqualTest ()
	{
		Expression<Func<int, int, bool>> e = (int a, int b) => a != b;
		AssertNodeType (e, ExpressionType.NotEqual);
		Assert (true, e.Compile ().Invoke (60, 30));
		Assert (false, e.Compile ().Invoke (-1, -1));
	}

	void NotEqualTest_2 ()
	{
		Expression<Func<sbyte?, sbyte?, bool>> e2 = (a, b) => a != b;
		AssertNodeType (e2, ExpressionType.NotEqual);
		Assert (false, e2.Compile ().Invoke (3, 3));
		Assert (true, e2.Compile ().Invoke (3, 2));
	}

	void NotEqualTest_3 ()
	{
		Expression<Func<MyType, MyType, bool>> e3 = (MyType a, MyType b) => a != b;
		AssertNodeType (e3, ExpressionType.NotEqual);
		Assert (false, e3.Compile ().Invoke (new MyType (-20), new MyType (-20)));
	}

	void NotEqualTest_4 ()
	{
		Expression<Func<MyType?, MyType?, bool>> e4 = (MyType? a, MyType? b) => a != b;
		AssertNodeType (e4, ExpressionType.NotEqual);
		Assert (true, e4.Compile ().Invoke (null, new MyType (-20)));
		Assert (false, e4.Compile ().Invoke (null, null));
		Assert (false, e4.Compile ().Invoke (new MyType (120), new MyType (120)));
	}

	void NotEqualTest_5 ()
	{
		Expression<Func<bool?, bool?, bool>> e5 = (bool? a, bool? b) => a != b;
		AssertNodeType (e5, ExpressionType.NotEqual);
		Assert (true, e5.Compile ().Invoke (true, null));
		Assert (false, e5.Compile ().Invoke (null, null));
		Assert (false, e5.Compile ().Invoke (false, false));
	}

	void NotEqualTest_6 ()
	{
		Expression<Func<bool, bool>> e6 = (bool a) => a != null;
		AssertNodeType (e6, ExpressionType.NotEqual);
		Assert (true, e6.Compile ().Invoke (true));
		Assert (true, e6.Compile ().Invoke (false));
	}

	void NotEqualTest_7 ()
	{
		Expression<Func<string, string, bool>> e7 = (string a, string b) => a != b;
		AssertNodeType (e7, ExpressionType.NotEqual);
		Assert (false, e7.Compile ().Invoke (null, null));
		Assert (true, e7.Compile ().Invoke ("a", "A"));
		Assert (false, e7.Compile ().Invoke ("a", "a"));
	}

	void NotEqualTest_8 ()
	{
		Expression<Func<object, bool>> e8 = (object a) => null != a;
		AssertNodeType (e8, ExpressionType.NotEqual);
		Assert (false, e8.Compile ().Invoke (null));
		Assert (true, e8.Compile ().Invoke ("a"));
		Assert (true, e8.Compile ().Invoke (this));
	}

	void NotEqualTest_9 ()
	{
		Expression<Func<MyEnum, MyEnum, bool>> e9 = (a, b) => a != b;
		AssertNodeType (e9, ExpressionType.NotEqual);
		Assert (true, e9.Compile ().Invoke (MyEnum.Value_1, MyEnum.Value_2));
		Assert (false, e9.Compile ().Invoke (MyEnum.Value_2, MyEnum.Value_2));
	}

	void NotEqualTest_10 ()
	{
		Expression<Func<MyEnum?, MyEnum?, bool>> e10 = (a, b) => a != b;
		AssertNodeType (e10, ExpressionType.NotEqual);
		Assert (true, e10.Compile ().Invoke (MyEnum.Value_1, null));
		Assert (false, e10.Compile ().Invoke (MyEnum.Value_2, MyEnum.Value_2));
	}

	void NotEqualTest_11 ()
	{
		Expression<Func<MyEnum?, bool>> e11 = (a) => a != null;
		AssertNodeType (e11, ExpressionType.NotEqual);
		Assert (true, e11.Compile ().Invoke (MyEnum.Value_1));
		Assert (false, e11.Compile ().Invoke (null));
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
	}

	void OrTest_2 ()
	{
		Expression<Func<MyType, MyType, MyType>> e2 = (MyType a, MyType b) => a | b;
		AssertNodeType (e2, ExpressionType.Or);
		var c2 = e2.Compile ();
		Assert (new MyType (3), c2 (new MyType (1), new MyType (2)));
	}

	void OrTest_3 ()
	{
		Expression<Func<MyEnum, MyEnum, MyEnum>> e3 = (a, b) => a | b;
		AssertNodeType (e3, ExpressionType.Convert);
		Assert ((MyEnum)3, e3.Compile ().Invoke (MyEnum.Value_1, MyEnum.Value_2));
		Assert (MyEnum.Value_2, e3.Compile ().Invoke (MyEnum.Value_2, MyEnum.Value_2));
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
	}

	void OrNullableTest_2 ()
	{
		Expression<Func<MyType?, MyType?, MyType?>> e2 = (MyType? a, MyType? b) => a | b;
		AssertNodeType (e2, ExpressionType.Or);
		var c2 = e2.Compile ();
		Assert (new MyType (3), c2 (new MyType (1), new MyType (2)));
		Assert (null, c2 (new MyType (1), null));
	}

	void OrNullableTest_3 ()
	{
		Expression<Func<MyType?, uint, long?>> e3 = (MyType? a, uint b) => a | b;
		AssertNodeType (e3, ExpressionType.Or);
		var c3 = e3.Compile ();
		Assert (9, c3 (new MyType (1), 8));
	}

	void OrNullableTest_4 ()
	{
		Expression<Func<MyEnum?, MyEnum?, MyEnum?>> e4 = (a, b) => a | b;
		AssertNodeType (e4, ExpressionType.Convert);
		Assert (null, e4.Compile ().Invoke (null, MyEnum.Value_2));
		Assert ((MyEnum)3, e4.Compile ().Invoke (MyEnum.Value_1, MyEnum.Value_2));
	}

	void OrElseTest ()
	{
		Expression<Func<bool, bool, bool>> e = (bool a, bool b) => a || b;
		AssertNodeType (e, ExpressionType.OrElse);
		Assert (true, e.Compile ().Invoke (true, false));
	}

	void OrElseTest_2 ()
	{
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
	}

	void ParameterTest_2 ()
	{
		Expression<Func<object[], object[]>> e2 = (object[] a) => a;
		AssertNodeType (e2, ExpressionType.Parameter);
		Assert (new object [0], e2.Compile ().Invoke (new object [0]));
	}

	void ParameterTest_3 ()
	{
		Expression<Func<IntPtr, IntPtr>> e3 = a => a;
		AssertNodeType (e3, ExpressionType.Parameter);
		Assert (IntPtr.Zero, e3.Compile ().Invoke (IntPtr.Zero));
	}

	unsafe void ParameterTest_4 ()
	{
		Expression<Func<int*[], int* []>> e4 = (a) => a;
		AssertNodeType (e4, ExpressionType.Parameter);
		Assert<int*[]> (null, e4.Compile ().Invoke (null));
		int* e4_el = stackalloc int [5];
		int*[] ptr = new int*[] { e4_el };
		Assert<int*[]> (ptr, e4.Compile ().Invoke (ptr));
	}

	void QuoteTest ()
	{
		Expression<Func<Expression<Func<int>>>> e = () => () => 2;
		AssertNodeType (e, ExpressionType.Quote);
		Assert (2, e.Compile ().Invoke ().Compile ().Invoke ());
	}

	void QuoteTest_2 ()
	{
		Expression<Func<string, Expression<Func<string>>>> e = (string s) => () => s;
		AssertNodeType (e, ExpressionType.Quote);
		
		Assert ("data", e.Compile ().Invoke ("data").Compile ().Invoke ());
	}
	
	void RightShiftTest ()
	{
		Expression<Func<ulong, short, ulong>> e = (ulong a, short b) => a >> b;
		AssertNodeType (e, ExpressionType.RightShift);
		Assert ((ulong) 0x1FD940L, e.Compile ().Invoke (0xFECA0000, 11));
		Assert ((ulong) 0x7FFFFFFF, e.Compile ().Invoke (0xFFFFFFFF, 0xA01));

		// .net produces a strange result
		// see https://bugzilla.novell.com/show_bug.cgi?id=398358		
		// Assert ((ulong)0, e.Compile ().Invoke (0xFFFFFFFF, 0xA01));
	}

	void RightShiftTest_2 ()
	{
		Expression<Func<MyType, MyType, int>> e2 = (MyType a, MyType b) => a >> b;
		AssertNodeType (e2, ExpressionType.RightShift);
		var c2 = e2.Compile ();
		Assert (64, c2 (new MyType (256), new MyType (2)));
	}

	void RightShiftTest_3 ()
	{
		Expression<Func<long?, sbyte, long?>> e3 = (long? a, sbyte b) => a >> b;
		AssertNodeType (e3, ExpressionType.RightShift);
		Assert (null, e3.Compile ().Invoke (null, 11));
		Assert (512, e3.Compile ().Invoke (1024, 1));
	}

	void RightShiftTest_4 ()
	{
		Expression<Func<MyType?, MyType?, int?>> e4 = (MyType? a, MyType? b) => a >> b;
		AssertNodeType (e4, ExpressionType.RightShift);
		var c4 = e4.Compile ();
		Assert (null, c4 (new MyType (8), null));
		Assert (null, c4 (null, new MyType (8)));
		Assert (64, c4 (new MyType (256), new MyType (2)));
	}
	
	void RightShiftTest_5 ()
	{
		Expression<Func<int, MyTypeImplicitOnly, int>> e = (a, b) => a >> b;
		AssertNodeType (e, ExpressionType.RightShift);
		Assert (31, e.Compile ().Invoke (0xFE, new MyTypeImplicitOnly (3)));
	}
	
	void RightShiftTest_6 ()
	{
		Expression<Func<ulong, byte?, ulong?>> e = (a, b) => a >> b;
		AssertNodeType (e, ExpressionType.RightShift);
		Assert (null, e.Compile () (2, null));
	}
	
	void SubtractTest ()
	{
		Expression<Func<int, int, int>> e = (int a, int b) => a - b;
		AssertNodeType (e, ExpressionType.Subtract);
		Assert (-10, e.Compile ().Invoke (20, 30));
	}
	
	void SubtractTest_2 ()
	{
		Expression<Func<int?, int?, int?>> e2 = (a, b) => a - b;
		AssertNodeType (e2, ExpressionType.Subtract);
		Assert (null, e2.Compile ().Invoke (null, 3));
	}
	
	void SubtractTest_3 ()
	{
		Expression<Func<MyType, MyType, MyType>> e3 = (MyType a, MyType b) => a - b;
		AssertNodeType (e3, ExpressionType.Subtract);
		Assert (-50, e3.Compile ().Invoke (new MyType (-20), new MyType (30)));
	}
	
	void SubtractTest_4 ()
	{
		Expression<Func<MyType?, MyType?, MyType?>> e4 = (MyType? a, MyType? b) => a - b;
		AssertNodeType (e4, ExpressionType.Subtract);
		Assert (new MyType (-50), e4.Compile ().Invoke (new MyType (-20), new MyType (30)));
		Assert (null, e4.Compile ().Invoke (null, new MyType (30)));
	}
	
	void SubtractTest_5 ()
	{
		Expression<Func<int, MyType, int>> e5 = (int a, MyType b) => a - b;
		AssertNodeType (e5, ExpressionType.Subtract);
		Assert (-29, e5.Compile ().Invoke (1, new MyType (30)));
	}
	
	void SubtractTest_6 ()
	{
		Expression<Func<int, MyType?, int?>> e6 = (int a, MyType? b) => a - b;
		AssertNodeType (e6, ExpressionType.Subtract);
		Assert (-61, e6.Compile ().Invoke (-31, new MyType (30)));
	}
	
	void SubtractTest_7 ()
	{
		Expression<Func<ushort, int?>> e7 = (ushort a) => null - a;
		AssertNodeType (e7, ExpressionType.Subtract);
		Assert (null, e7.Compile ().Invoke (690));
	}
	
	void SubtractTest_8 ()
	{
		Expression<Func<MyEnum, byte, MyEnum>> e8 = (a, b) => a - b;
		AssertNodeType (e8, ExpressionType.Convert);
		Assert ((MyEnum)255, e8.Compile ().Invoke (MyEnum.Value_1, 2));
	}
	
	void SubtractTest_9 ()
	{
		Expression<Func<MyEnum, MyEnum, byte>> e9 = (a, b) => a - b;
		AssertNodeType (e9, ExpressionType.Convert);
		Assert (1, e9.Compile ().Invoke (MyEnum.Value_2, MyEnum.Value_1));
	}

	// CSC bug
	void SubtractTest_10 ()
	{
		Expression<Func<MyEnum?, byte?, MyEnum?>> e10 = (a, b) => a - b;
		AssertNodeType (e10, ExpressionType.Convert);
		Assert ((MyEnum) 255, e10.Compile ().Invoke (MyEnum.Value_1, 2));
	}

	// CSC bug
	void SubtractTest_11 ()
	{
		Expression<Func<MyEnum?, MyEnum?, byte?>> e11 = (a, b) => a - b;
		AssertNodeType (e11, ExpressionType.Convert);
		Assert<byte?> (1, e11.Compile ().Invoke (MyEnum.Value_2, MyEnum.Value_1));
	}

	void SubtractCheckedTest ()
	{
		checked {
			Expression<Func<long, long, long>> e = (long a, long b) => a - b;
			AssertNodeType (e, ExpressionType.SubtractChecked);
			try {
				e.Compile ().Invoke (long.MinValue, 309);
				throw new ApplicationException ("SubtractCheckedTest #1");
			} catch (OverflowException) { }
		}
	}

	void SubtractCheckedTest_2 ()
	{
		checked {
			Expression<Func<byte?, byte?, int?>> e2 = (a, b) => a - b;
			AssertNodeType (e2, ExpressionType.SubtractChecked);
			Assert (null, e2.Compile ().Invoke (null, 3));
			Assert (-55, e2.Compile ().Invoke (byte.MinValue, 55));
		}
	}

	void SubtractCheckedTest_3 ()
	{
		checked {
			Expression<Func<MyType, MyType, MyType>> e3 = (MyType a, MyType b) => a - b;
			AssertNodeType (e3, ExpressionType.Subtract);
			Assert (-50, e3.Compile ().Invoke (new MyType (-20), new MyType (30)));
		}
	}

	void SubtractCheckedTest_4 ()
	{
		checked {
			Expression<Func<double, double, double>> e4 = (a, b) => a - b;
			AssertNodeType (e4, ExpressionType.Subtract);
			Assert (double.PositiveInfinity, e4.Compile ().Invoke (double.MinValue, double.NegativeInfinity));
		}
	}
	
	void TypeAsTest ()
	{
		Expression<Func<object, Tester>> e = (object a) => a as Tester;
		AssertNodeType (e, ExpressionType.TypeAs);
		Assert (this, e.Compile ().Invoke (this));
	}
	
	void TypeAsTest_2 ()
	{
		Expression<Func<object, int?>> e2 = (object a) => a as int?;
		AssertNodeType (e2, ExpressionType.TypeAs);
		Assert (null, e2.Compile ().Invoke (null));
		Assert (null, e2.Compile ().Invoke (this));
		Assert (44, e2.Compile ().Invoke (44));
	}
	
	void TypeAsTest_3 ()
	{
		Expression<Func<object, object>> e3 = (object a) => null as object;
		AssertNodeType (e3, ExpressionType.TypeAs);
		Assert (null, e3.Compile ().Invoke (null));
	}

	void TypeAsTest_4 ()
	{
		Expression<Func<int, IConvertible>> e = a => a as IConvertible;
		AssertNodeType (e, ExpressionType.TypeAs);
		Assert (ExpressionType.Parameter, ((UnaryExpression) e.Body).Operand.NodeType);
		Assert (5, e.Compile ().Invoke (5));
	}
	
	void TypeIsTest ()
	{
		Expression<Func<object, bool>> e = (object a) => a is Tester;
		AssertNodeType (e, ExpressionType.TypeIs);
		Assert (true, e.Compile ().Invoke (this));
		Assert (false, e.Compile ().Invoke (1));
	}
	
	void TypeIsTest_2 ()
	{
		Expression<Func<object, bool>> e2 = (object a) => a is int?;
		AssertNodeType (e2, ExpressionType.TypeIs);
		Assert (false, e2.Compile ().Invoke (null));
		Assert (true, e2.Compile ().Invoke (1));
	}
	
	void TypeIsTest_3 ()
	{
		Expression<Func<object, bool>> e3 = (object a) => null is object;
		AssertNodeType (e3, ExpressionType.TypeIs);
		Assert (false, e3.Compile ().Invoke (null));
	}
	
	void TypeIsTest_4 ()
	{
		Expression<Func<B, bool>> e = l => l is A;
		AssertNodeType (e, ExpressionType.TypeIs);
		Assert (false, e.Compile ().Invoke (null));
	}
	
	void TypeIsTest_5 ()
	{
		Expression<Func<bool>> e5 = () => 1 is int;
		AssertNodeType (e5, ExpressionType.TypeIs);
		Assert (true, e5.Compile ().Invoke ());
	}
	
	void TypeIsTest_6 ()
	{
		Expression<Func<int?, bool>> e6 = (a) => a is int;
		AssertNodeType (e6, ExpressionType.TypeIs);
		Assert (true, e6.Compile ().Invoke (1));
		Assert (false, e6.Compile ().Invoke (null));
	}
	
	void UnaryPlusTest ()
	{
		Expression<Func<int, int>> e = (a) => +a;
		AssertNodeType (e, ExpressionType.Parameter);
		Assert (-30, e.Compile ().Invoke (-30));
	}
	
	void UnaryPlusTest_2 ()
	{
		Expression<Func<long?, long?>> e2 = (a) => +a;
		AssertNodeType (e2, ExpressionType.Parameter);
	}
	
	void UnaryPlusTest_3 ()
	{
		Expression<Func<MyType, MyType>> e4 = (a) => +a;
		AssertNodeType (e4, ExpressionType.UnaryPlus);
		Assert (new MyType (-14), e4.Compile ().Invoke (new MyType (-14)));
	}
	
	void UnaryPlusTest_4 ()
	{
		Expression<Func<MyType?, MyType?>> e5 = (a) => +a;
		AssertNodeType (e5, ExpressionType.UnaryPlus);
		Assert (new MyType (33), e5.Compile ().Invoke (new MyType (33)));
		Assert (null, e5.Compile ().Invoke (null));
	}
	
	void UnaryPlusTest_5 ()
	{
		Expression<Func<sbyte?, long?>> e6 = (a) => +a;
		AssertNodeType (e6, ExpressionType.Convert);
		Assert (3, e6.Compile ().Invoke (3));
		Assert (null, e6.Compile ().Invoke (null));
	}	

#pragma warning restore 169

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
	
	static int TestInt ()
	{
		return 29;
	}

	T GenericMethod<T> (T t)
	{
		return t;
	}
	
	static void RefMethod (ref int i)
	{
		i = 867;
	}

	static bool RunTest (MethodInfo test)
	{
		Console.Write ("Running test {0, -25}", test.Name);
		try {
			test.Invoke (new Tester (), null);
			Console.WriteLine ("OK");
			return true;
		} catch (Exception e) {
			Console.WriteLine ("FAILED");
			Console.WriteLine (e.ToString ());
			return false;
		}
	}

	public static int Main ()
	{
		var tests = from test in typeof (Tester).GetMethods (BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
					where test.GetParameters ().Length == 0
					orderby test.Name
					select RunTest (test);

		int failures = tests.Count (a => !a);
		Console.WriteLine (failures + " tests failed");
		return failures;
	}
}

