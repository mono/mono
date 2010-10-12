using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

// Dynamic binary operator, unary operators and convert tests

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

	public static bool operator true (MyType a)
	{
		return a.value != 1;
	}

	public static bool operator false (MyType a)
	{
		return a.value == 0;
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

	public static int operator ~ (MyType a)
	{
		return ~a.value;
	}

	public static MyType operator ++ (MyType a)
	{
		return new MyType (a.value * 2);
	}

	public static MyType operator -- (MyType a)
	{
		return new MyType (a.value / 2);
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
		return new MyType (334455); // magic number
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

	public static implicit operator bool (MyTypeImplicitOnly m)
	{
		return m.b != 0;
	}
}

enum MyEnum : byte
{
	Value_1 = 1,
	Value_2 = 2
}

enum MyEnumUlong : ulong
{
	Value_1 = 1,
	Value_2 = 2
}


class Tester
{
	delegate void EmptyDelegate ();
	event Action ev_assign;

	static void Assert<T> (T expected, T value, string name)
	{
		if (!EqualityComparer<T>.Default.Equals (expected, value)) {
			name += ": ";
			throw new ApplicationException (name + expected + " != " + value);
		}
	}

	static void AssertChecked<T> (Func<T> expected, T value, string name)
	{
		try {
			Assert (expected (), value, name);
			throw new ApplicationException (name + ": OverflowException expected");
		} catch (OverflowException) {
			// passed
		}
	}

	static void AssertChecked (Action expected, string name)
	{
		try {
			expected ();
			throw new ApplicationException (name + ": OverflowException expected");
		} catch (OverflowException) {
			// passed
		}
	}

#pragma warning disable 169

	void AddTest ()
	{
		dynamic d = 5;

		int v = 2;
		Assert (d + v, 7, "#1");
		double v2 = 0.5;
		Assert (d + v2, 5.5, "#1a");

		d = new MyType (5);
		MyType v3 = new MyType (30);
		Assert (d + v3, new MyType (35), "#3");
		dynamic d3 = new MyType (-7);
		Assert<MyType> (d3 + new MyType (6), new MyType (-1), "#3a");

		d3 = new MyTypeImplicitOnly (6);
		Assert (d3 + new MyTypeImplicitOnly (11), 17, "#3b");

		d = new MyTypeImplicitOnly (5);
		decimal v4 = 4m;
		Assert (d + v4, 9m, "#4");
	}

	void AddNullableTest ()
	{
		dynamic d = 5;

		int? v2 = null;
		Assert<int?> (d + v2, null, "#1");
		Assert<int?> (d + null, null, "#1a");
		Assert<int?> (null + d, null, "#1b");

		v2 = -2;
		Assert (d + v2, 3, "#2");
		dynamic d2 = (int?) -2;
		Assert (d2 + 1, -1, "#2a");

		d = new MyType (5);
		MyType? v3 = new MyType (30);
		Assert (d + v3, new MyType (35), "#3");
		dynamic d3 = new MyType? (new MyType (-7));
		Assert (d3 + new MyType (6), new MyType (-1), "#3a");
		Assert<MyType?> (d3 + null, null, "#3b");

		d = new MyTypeImplicitOnly (5);
		decimal? v4 = 4m;
		Assert (d + v4, 9m, "#4");
		v4 = null;
		Assert<decimal?> (d + v4, null, "#4a");
	}
	
	void AddEnumTest ()
	{
		dynamic d = MyEnum.Value_1;

		Assert (d + 1, MyEnum.Value_2, "#1");

		dynamic d2 = (MyEnumUlong?) MyEnumUlong.Value_1;
		Assert (d2 + (byte) 1, MyEnumUlong.Value_2, "#2");
		Assert<MyEnumUlong?> (d2 + null, null, "#2a");
		
		// CSC: Invalid System.InvalidOperationException
		Assert<MyEnum?> (d + null, null, "#1");
	}
	
	void AddCheckedTest ()
	{
		checked {
			dynamic d = 5;

			int v = int.MaxValue;
			AssertChecked (() => d + v, 7, "#1");

			int? v2 = v;
			AssertChecked (() => d + v2, null, "#2");

			d = new MyType (3);
			MyType v3 = new MyType (int.MaxValue);
			Assert (new MyType (-2147483646), d + v3, "#3");
		}
	}
	
	void AddStringTest ()
	{
		dynamic d = "foo";
		string v = "->";
		Assert (d + v, "foo->", "#1");
		Assert (d + 1, "foo1", "#1a");
		Assert (d + null, "foo", "#1b");
		Assert (d + 1 + v, "foo1->", "#1a");

		uint? v2 = 4;
		Assert (d + v2, "foo4", "#2");
	}

	void AddAssignTest ()
	{
		dynamic d = 5;

		int v = 2;
		d += v;
		Assert (d, 7, "#1");

		d = 5.0;
		double v2 = 0.5;
		d += v2;
		Assert (d, 5.5, "#1a");
		d += v;
		Assert (d, 7.5, "#1b");

		dynamic d3 = new MyType (-7);
		d3 += new MyType (6);
		Assert<MyType> (d3, new MyType (-1), "#3");

		d = 5m;
		decimal v4 = 4m;
		d += v4;
		Assert (d, 9m, "#4");
	}

	void AddAssignNullableTest ()
	{
		dynamic d = (int?) 5;

		// FEATURE
		// For now it's impossible to use nullable compound assignment
		// due to the way how DLR works. GetType () on nullable object returns
		// underlying type and not nullable type, that means that
		// C# binder is initialized with wrong operand type and any operation
		// fails to resolve
/*
		long? v2 = null;
		d += v2;
		Assert<int?> (d, null, "#1");
		d += null;
		Assert<int?> (d, null, "#1a");

		long? l = (long?) 3;
		d = l;
		v2 = -2;
		d += v2;
		Assert (d, 3, "#2");
		d = (int?) -2;
		d += 1;
		Assert (d, -1, "#2a");

		MyType? v3 = new MyType (30);
		d += v3;
		Assert (d, new MyType (35), "#3");
		dynamic d3 = new MyType? (new MyType (-7));
		Assert (d3 + new MyType (6), new MyType (-1), "#3a");
		Assert<MyType?> (d3 + null, null, "#3b");

		decimal? v4 = 4m;
		d = 2m;
		d += v4;
		Assert (d, 9m, "#4");
		d += null;
		Assert<decimal?> (d, null, "#4a");
 */
	}

	void AddAssignEnumTest ()
	{
		dynamic d = MyEnum.Value_1;

		d = MyEnum.Value_1;
		d += 1;
		Assert (d, MyEnum.Value_2, "#2");

		dynamic d2 = (MyEnumUlong?) MyEnumUlong.Value_1;
		d2 += (byte) 1;
		Assert (d2, MyEnumUlong.Value_2, "#3");
	}

	void AddAssignCheckedTest ()
	{
		checked {
			dynamic d = 5;

			int v = int.MaxValue;
			AssertChecked (() => { d += v; Assert (d, 0, "#1-"); }, "#1");

			d = new MyType (5);
			MyType v3 = new MyType (int.MaxValue);
			d += v3;
			Assert (d, new MyType (-2147483644), "#3-");
		}
	}

	void AddAssignStringTest ()
	{
		dynamic d = "foo";
		string v = "->";
		d += v;
		Assert (d, "foo->", "#1");

		d = "foo";
		d += 1;
		Assert (d, "foo1", "#1a");

		d += null;
		Assert (d, "foo1", "#1b");

		uint? v2 = 4;
		d = "foo";
		d += v2;
		Assert (d, "foo4", "#2");
	}

	void AddAssignEvent ()
	{
		dynamic d = null;
		
		// FIXME: Will have to special case events
		// ev_assign += d;
	}

	void AndTest ()
	{
		dynamic d = true;

		var v = false;
		Assert (d & v, false, "#1");
		Assert (d & true, true, "#1a");

		d = 42;
		var v2 = 62;
		Assert (d & v2, 42, "#2");
		Assert (d & 0, 0, "#2a");

		d = new MyType (10);
		MyType v3 = new MyType (30);
		Assert (d & v3, new MyType (10), "#3");
		dynamic d3 = new MyType (-7);
		Assert<MyType> (d3 & new MyType (6), new MyType (0), "#3a");

		d3 = new MyTypeImplicitOnly (6);
		Assert (d3 & 11, 2, "#3b");
	}

	void AndTestEnum ()
	{
		dynamic d = MyEnum.Value_1;

		Assert<MyEnum?> (d & null, null, "#1");

		Assert (d & d, MyEnum.Value_1, "#2");

		dynamic d2 = (MyEnumUlong?) MyEnumUlong.Value_1;
		Assert<MyEnumUlong> (d2 & MyEnumUlong.Value_2, 0, "#3");
	}

	void AndTestNullable ()
	{
		dynamic d = 5;

		int? v2 = null;
		Assert<int?> (d & v2, null, "#1");
		Assert<int?> (d & null, null, "#1a");
		Assert<int?> (null & d, null, "#1b");

		v2 = -2;
		Assert (d & v2, 4, "#2");
		dynamic d2 = (int?) -2;
		Assert (d2 & 1, 0, "#2a");

		d = new MyType (22);
		MyType? v3 = new MyType (30);
		Assert (d & v3, new MyType (22), "#3");
		dynamic d3 = new MyType? (new MyType (-7));
		Assert (d3 & new MyType (6), new MyType (0), "#3a");
		Assert<MyType?> (d3 + null, null, "#3b");
	}

	void AndAssignedTest ()
	{
		dynamic d = true;

		var v = false;
		d &= v;
		Assert (d, false, "#1");
		d = true;
		d &= true;
		Assert (d, true, "#1a");

		d = 42;
		var v2 = 62;
		d &= v2;
		Assert (d, 42, "#2");

		MyType v3 = new MyType (30);
		dynamic d3 = new MyType (-7);
		d3 &= new MyType (6);
		Assert<MyType> (d3, new MyType (0), "#3");
	}

	void AndAssignedTestEnum ()
	{
		dynamic d = MyEnum.Value_1;
		d &= MyEnum.Value_2;
		Assert<MyEnum>(d, 0, "#1");

		d = MyEnum.Value_2;
		d &= d;
		Assert (d, MyEnum.Value_2, "#2");

		dynamic d2 = (MyEnumUlong?) MyEnumUlong.Value_1;
		Assert<MyEnumUlong> (d2 & MyEnumUlong.Value_2, 0, "#3");
	}

	void AndAlsoTest ()
	{
		dynamic d = true;

		var v = false;
		Assert<bool> (d && v, false, "#1");
		
		Assert (d && true, true, "#1a");

		d = true;
		Assert (d && d, true, "#2");

		dynamic d3 = new MyType (-7);
		Assert<MyType> (d3 && new MyType (6), new MyType (0), "#3");
	}

	void DivideTest ()
	{
		dynamic d = 5;

		int v = 2;
		Assert (d / v, 2, "#1");

		d = new MyType (5);
		MyType v3 = new MyType (30);
		Assert (d / v3, new MyType (0), "#3");
		dynamic d3 = new MyType (-7);
		Assert<MyType> (d3 + new MyType (6), new MyType (-1), "#3a");

		d = new MyTypeImplicitOnly (6);
		decimal v4 = 4m;
		Assert (d / v4, 1.5m, "#4");
	}

	void DivideNullableTest ()
	{
		dynamic d = 5;

		double? v2 = null;
		Assert<double?> (d / v2, null, "#1");
		Assert<double?> (d / null, null, "#1a");
		Assert<double?> (null / d, null, "#1b");

		v2 = -2;
		Assert (d / v2, -2.5, "#2");
		dynamic d2 = (int?) -2;
		Assert (d2 / 1, -2, "#2a");

		d = new MyType (5);
		MyType? v3 = new MyType (30);
		Assert (d / v3, new MyType (0), "#3");
		dynamic d3 = new MyType? (new MyType (-7));
		Assert (d3 / new MyType (6), new MyType (-1), "#3a");
		Assert<MyType?> (d3 + null, null, "#3b");

		d = new MyTypeImplicitOnly (5);
		decimal? v4 = 4m;
		Assert (d / v4, 1.25m, "#4");
		v4 = null;
		Assert<decimal?> (d / v4, null, "#4a");
	}

	void DivideCheckedTest ()
	{
		checked {
			// TODO:
		}
	}

	void DivideAssignTest ()
	{
		dynamic d = 5;

		int v = 2;
		d /= v;
		Assert (d, 2, "#1");

		d = 5.0;
		double v2 = 0.5;
		d /= v2;
		Assert (d, 10, "#1a");
		d /= v;
		Assert (d, 5, "#1b");

		dynamic d3 = new MyType (-7);
		d3 /= new MyType (6);
		Assert<MyType> (d3, new MyType (-1), "#3");

		d = 5m;
		decimal v4 = 4m;
		d /= v4;
		Assert (d, 1.25m, "#4");
	}

	void DivideAssignCheckedTest ()
	{
		checked {
			// TODO:
		}
	}

	void ConvertImplicitTest ()
	{
		dynamic d = 3;
		decimal v1 = d;
		Assert (3m, v1, "#1");

		d = new MyTypeImplicitOnly (5);
		int v2 = d;
		Assert (5, v2, "#2");

		d = (byte) 4;
		int v3 = d;
		Assert (4, v3, "#3");

		int[] v4 = new int[] { d };
		Assert (4, v4[0], "#4");

		d = true;
		var v5 = new [] { d, 1 };
		Assert (true, v5[0], "#5");
		Assert (1, v5[1], "#5a");

		d = "aa";
		bool b = false;
		var r = b ? d : "ss";
		Assert ("ss", r, "#6");
		
		var v = new [] { d, 1 };
		Assert ("aa", v [0], "#7");
		
		dynamic [,] a = new dynamic [,] { { 1, 2 }, { 'b', 'x' } };
		Assert (2, a [0, 1], "#8");
		Assert ('x', a [1, 1], "#8a");
	}

	int ConvertImplicitReturnTest ()
	{
		dynamic d = (byte) 3;
		return d;
	}

	IEnumerable<string> ConvertImplicitReturnTest_2 ()
	{
		dynamic d = "aaa";
		yield return d;
	}

	void ConvertExplicitTest ()
	{
		dynamic d = 300;
		Assert (44, (byte) d, "#1");
		Assert<byte?> (44, (byte?) d, "#1a");

		d = 3m;
		Assert (3, d, "#2");

		d = new MyTypeImplicitOnly (5);
		Assert (5, (int) d, "#3");

		d = new MyTypeExplicit (-2);
		Assert (-2, (int) d, "#4");

		d = null;
		Assert (null, (object) d, "#5");
	}

	void ConvertExplicitCheckedTest ()
	{
		checked {
			dynamic d = 300;
			AssertChecked (() => (byte) d, 7, "#1");

			d = ulong.MaxValue;
			AssertChecked<uint?> (() => (uint?) d, 2, "#2");
		}
	}
	
	void ConvertArray ()
	{
		dynamic idx = (uint) 1;
		var arr = new int [5];
		arr [idx] = 2;
		Assert (2, arr [idx], "#1");
	}

	void EqualTest ()
	{
		dynamic d = 5;

		int v = 2;
		Assert (d == v, false, "#1");
		double v2 = 5;
		Assert (d == v2, true, "#1a");

		d = true;
		Assert (d == false, false, "#2");
		bool b2 = true;
		Assert (d == b2, true, "#2a");

		d = new MyType (30);
		MyType v3 = new MyType (30);
		Assert (d == v3, true, "#3");
		dynamic d3 = new MyTypeImplicitOnly (-7);
		Assert (d3 == 11, false, "#3b");
		
		d = 2m;
		decimal v4 = 4m;
		Assert (d == v4, false, "#4");
		Assert (d == 2m, true, "#4a");
		
		d = null;
		Assert (d == null, true, "#5");
	}

	void EqualNullableTest ()
	{
		dynamic d = 5;

		int? v2 = null;
		Assert (d == v2, false, "#1");
		Assert (d == null, false, "#1a");
		Assert (null == d, false, "#1b");

		v2 = -2;
		Assert (d == v2, false, "#2");
		dynamic d2 = (int?) -2;
		Assert (d2 == 1, false, "#2a");
		d2 = (uint?) 44;
		Assert (d2 == 44, true, "#2b");

		d = new MyType (30);
		MyType? v3 = new MyType (30);
		Assert (d == v3, true, "#3");
		dynamic d3 = new MyType? (new MyType (-7));
		Assert (d3 == new MyType (6), false, "#3a");
		Assert (d3 == null, false, "#3b");
		
		d = 4.1m;
		decimal? v4 = 4m;
		Assert (d == v4, false, "#4");
		v4 = null;
		Assert (d == v4, false, "#4a");

		d = (bool?) true;
		Assert (d == true, true, "#5");
		Assert (d == null, false, "#5a");
		Assert (d == false, false, "#5b");
	}

	void EqualEnumTest ()
	{
		dynamic d = MyEnum.Value_1;

		Assert (d == null, false, "#1");

		Assert (d == MyEnum.Value_1, true, "#2");
		Assert (d == 0, false, "#2a");

		dynamic d2 = (MyEnumUlong?) MyEnumUlong.Value_2;
		Assert (d2 == MyEnumUlong.Value_2, true, "#3");
		Assert (d2 == null, false, "#3a");
	}

	void EqualStringTest ()
	{
		dynamic d = "text";

		Assert (d == "te", false, "#1");
		Assert (d == "text", true, "#1a");
		Assert (d == null, false, "#1b");
	}

	void EqualDelegateTest ()
	{
		dynamic d = this;

//		Assert (d == delegate { }, true, "#1");

		EmptyDelegate b = EqualDelegateTest;
		d = b;

		//Assert (d == EqualDelegateTest, true, "#2");
	
/*

	void EqualTestDelegate_2 ()
	{
		EmptyDelegate ed = delegate () {};

		Expression<Func<EmptyDelegate, EmptyDelegate, bool>> e2 = (a, b) => a == b;
		AssertNodeType (e2, ExpressionType.Equal);
		Assert (false, e2.Compile ().Invoke (delegate () {}, null));
		Assert (false, e2.Compile ().Invoke (delegate () {}, delegate {}));
		Assert (false, e2.Compile ().Invoke (ed, delegate {}));
		Assert (true, e2.Compile ().Invoke (ed, ed));
*/
	}

	void ExclusiveOrTest ()
	{
		dynamic d = true;

		var v = false;
		Assert (d ^ v, true, "#1");
		Assert (d ^ true, false, "#1a");

		d = 42;
		var v2 = 62;
		Assert (d ^ v2, 20, "#2");
		Assert (d ^ 0, 42, "#2a");

		d = new MyType (42);
		MyType v3 = new MyType (30);
		Assert (d ^ v3, new MyType (52), "#3");
		dynamic d3 = new MyType (-7);
		Assert<MyType> (d3 ^ new MyType (6), new MyType (-1), "#3a");

		d3 = new MyTypeImplicitOnly (-7);
		Assert (d3 ^ 11, -14, "#3b");
	}

	void ExclusiveOrNullableTest ()
	{
		dynamic d = 5;

		int? v2 = null;
		Assert<int?> (d ^ v2, null, "#1");
		Assert<int?> (d ^ null, null, "#1a");
		Assert<int?> (null ^ d, null, "#1b");

		v2 = -2;
		Assert (d ^ v2, -5, "#2");
		dynamic d2 = (int?) -2;
		Assert (d2 ^ 1, -1, "#2a");

		d = new MyType (5);
		MyType? v3 = new MyType (30);
		Assert (d ^ v3, new MyType (27), "#3");
		dynamic d3 = new MyType? (new MyType (-7));
		Assert (d3 ^ new MyType (6), new MyType (-1), "#3a");
		Assert<MyType?> (d3 ^ null, null, "#3b");
	}

	void ExclusiveOrTestEnum ()
	{
		dynamic d = MyEnum.Value_1;

		Assert<MyEnum?> (d ^ null, null, "#1");

		Assert<MyEnum> (d ^ d, 0, "#2");

		dynamic d2 = (MyEnumUlong?) MyEnumUlong.Value_1;
		Assert<MyEnumUlong> (d2 ^ MyEnumUlong.Value_2, (MyEnumUlong) 3, "#3");
	}

	void ExclusiveOrAssignedTest ()
	{
		dynamic d = true;

		var v = false;
		d ^= v;
		Assert (d, true, "#1");
		d = true;
		d ^= true;
		Assert (d, false, "#1a");

		d = 42;
		var v2 = 62;
		d ^= v2;
		Assert (d, 20, "#2");

		MyType v3 = new MyType (30);
		dynamic d3 = new MyType (-7);
		d3 ^= new MyType (6);
		Assert (d3, new MyType (-1), "#3");
	}

	void ExclusiveOrAssignedTestEnum ()
	{
		dynamic d = MyEnum.Value_1;
		d ^= MyEnum.Value_2;
		Assert<MyEnum>(d, (MyEnum) 3, "#1");

		d = MyEnum.Value_2;
		d ^= d;
		Assert<MyEnum> (d, 0, "#2");

		dynamic d2 = (MyEnumUlong?) MyEnumUlong.Value_1;
		Assert (d2 ^ MyEnumUlong.Value_2, (MyEnumUlong)3, "#3");
	}

	void GreaterThanTest ()
	{
		dynamic d = 5;

		int v = 2;
		Assert (d > v, true, "#1");
		double v2 = 5;
		Assert (d > v2, false, "#1a");

		d = 4.6;
		Assert (d > 4.59, true, "#2");
		var b2 = 4.6;
		Assert (d > b2, false, "#2a");

		d = new MyType (30);
		MyType v3 = new MyType (30);
		Assert (d > v3, false, "#3");
		dynamic d3 = new MyType (-7);
		Assert (d3 > new MyType (6), false, "#3a");

		d3 = new MyTypeImplicitOnly (-7);
		Assert (d3 > 11, false, "#3b");

		d = 2m;
		decimal v4 = 4m;
		Assert (d > v4, false, "#4");
		Assert (d > 2m, false, "#4a");
	}

	void GreaterThanNullableTest ()
	{
		dynamic d = 5;

		int? v2 = null;
		Assert (d > v2, false, "#1");
		Assert (d > null, false, "#1a");
		Assert (null > d, false, "#1b");

		v2 = -2;
		Assert (d > v2, true, "#2");
		dynamic d2 = (int?) -2;
		Assert (d2 > 1, false, "#2a");
		d2 = (uint?) 44;
		Assert (d2 > 44, false, "#2b");

		d = new MyType (30);
		MyType? v3 = new MyType (30);
		Assert (d > v3, false, "#3");
		dynamic d3 = new MyType? (new MyType (-7));
		Assert (d3 > new MyType (6), false, "#3a");
		Assert (d3 > null, false, "#3b");

		d = 4.1m;
		decimal? v4 = 4m;
		Assert (d > v4, true, "#4");
		v4 = null;
		Assert (d > v4, false, "#4a");
	}

	void GreaterThanEnumTest ()
	{
		dynamic d = MyEnum.Value_1;

		Assert (d > null, false, "#1");

		Assert (d > MyEnum.Value_1, false, "#2");
		Assert (d > 0, true, "#2a");

		dynamic d2 = (MyEnumUlong?) MyEnumUlong.Value_2;
		Assert (d2 > MyEnumUlong.Value_2, false, "#3");
		Assert (d2 > null, false, "#3a");
	}

	void GreaterThanEqualTest ()
	{
		dynamic d = 5;

		int v = 2;
		Assert (d >= v, true, "#1");
		double v2 = 5;
		Assert (d >= v2, true, "#1a");

		d = 4.6;
		Assert (d >= 4.59, true, "#2");
		var b2 = 4.6;
		Assert (d >= b2, true, "#2a");

		d = new MyType (30);
		MyType v3 = new MyType (30);
		Assert (d >= v3, true, "#3");
		dynamic d3 = new MyType (-7);
		Assert (d3 >= new MyType (6), false, "#3a");

		d3 = new MyTypeImplicitOnly (-7);
		Assert (d3 >= 11, false, "#3b");

		d = 2m;
		decimal v4 = 4m;
		Assert (d >= v4, false, "#4");
		Assert (d >= 2m, true, "#4a");
	}

	void GreaterThanEqualNullableTest ()
	{
		dynamic d = 5;

		int? v2 = null;
		Assert (d >= v2, false, "#1");
		Assert (d >= null, false, "#1a");
		Assert (null >= d, false, "#1b");

		v2 = -2;
		Assert (d >= v2, true, "#2");
		dynamic d2 = (int?) -2;
		Assert (d2 >= 1, false, "#2a");
		d2 = (uint?) 44;
		Assert (d2 >= 44, true, "#2b");

		d = new MyType (30);
		MyType? v3 = new MyType (30);
		Assert (d >= v3, true, "#3");
		dynamic d3 = new MyType? (new MyType (-7));
		Assert (d3 >= new MyType (6), false, "#3a");
		Assert (d3 >= null, false, "#3b");

		d = 4.1m;
		decimal? v4 = 4m;
		Assert (d >= v4, true, "#4");
		v4 = null;
		Assert (d >= v4, false, "#4a");
	}

	void GreaterThanEqualEnumTest ()
	{
		dynamic d = MyEnum.Value_1;

		Assert (d >= null, false, "#1");

		Assert (d >= MyEnum.Value_1, true, "#2");
		Assert (d >= 0, true, "#2a");

		dynamic d2 = (MyEnumUlong?) MyEnumUlong.Value_2;
		Assert (d2 >= MyEnumUlong.Value_2, true, "#3");
		Assert (d2 >= null, false, "#3a");
	}

	void LeftShiftTest ()
	{
		dynamic d = (ulong) 0x7F000;

		int v = 2;
		Assert<ulong> (d << v, 0x1FC000, "#1");
		Assert<ulong> (d << 1, 0xFE000, "#1a");
		short s = 2;
		Assert<ulong> (d << s, 0x1FC000, "#1b");

		d = 0x7F000;
		MyTypeImplicitOnly v3 = new MyTypeImplicitOnly (3);
		Assert (d << v3, 0x3F8000, "#3");
		dynamic d3 = new MyType (-7);
		Assert (d3 << new MyTypeImplicitOnly (6), -448, "#3a");
		Assert (d3 << 11, -14336, "#3b");
	}

	void LeftShiftNullableTest ()
	{
		dynamic d = 5;

		int? v2 = null;
		Assert<int?> (d << v2, null, "#1");
		d = 5;
		Assert<int?> (d << null, null, "#1a");
		d = 5;
		Assert<int?> (null << d, null, "#1b");

		v2 = -2;
		Assert (d << v2, 0x40000000, "#2");
		dynamic d2 = (int?) -2;
		Assert (d2 << 1, -4, "#2a");
	}

	void LeftShiftAssignTest ()
	{
		dynamic d = 0x7F000;

		int v = 2;
		d <<= v;
		Assert (d, 0x1FC000, "#1");
		d <<= 1;
		Assert (d, 0x3F8000, "#1a");
		sbyte s = 2;
		d <<= s;
		Assert (d, 0xFE0000, "#1b");
	}

	void LeftShiftAssignNullableTest ()
	{
		dynamic d = 5;

		var v2 = -2;
		d <<= v2;
		Assert (d, 0x40000000, "#2");
		dynamic d2 = (int?) -2;
		d2 <<= 1;
		Assert (d2, -4, "#2a");
	}

	void LessThanTest ()
	{
		dynamic d = 5;

		int v = 2;
		Assert (d < v, false, "#1");
		double v2 = 5;
		Assert (d < v2, false, "#1a");

		d = 4.6;
		Assert (d < 4.59, false, "#2");
		var b2 = 4.6;
		Assert (d < b2, false, "#2a");

		d = new MyType (30);
		MyType v3 = new MyType (30);
		Assert (d < v3, false, "#3");
		dynamic d3 = new MyType (-7);
		Assert (d3 < new MyType (6), true, "#3a");

		d3 = new MyTypeImplicitOnly (-7);
		Assert (d3 < 11, true, "#3b");

		d = 2m;
		decimal v4 = 4m;
		Assert (d < v4, true, "#4");
		Assert (d < 2m, false, "#4a");
	}

	void LessThanNullableTest ()
	{
		dynamic d = 5;

		int? v2 = null;
		Assert (d < v2, false, "#1");
		Assert (d < null, false, "#1a");
		Assert (null < d, false, "#1b");

		v2 = -2;
		Assert (d < v2, false, "#2");
		dynamic d2 = (int?) -2;
		Assert (d2 < 1, true, "#2a");
		d2 = (uint?) 44;
		Assert (d2 < 44, false, "#2b");

		d = new MyType (30);
		MyType? v3 = new MyType (30);
		Assert (d < v3, false, "#3");
		dynamic d3 = new MyType? (new MyType (-7));
		Assert (d3 < new MyType (6), true, "#3a");

		d3 = new MyTypeImplicitOnly (-7);
		Assert (d3 < null, false, "#3b");

		d = 4.1m;
		decimal? v4 = 4m;
		Assert (d < v4, false, "#4");
		v4 = null;
		Assert (d < v4, false, "#4a");
	}

	void LessThanEnumTest ()
	{
		dynamic d = MyEnum.Value_1;

		Assert (d < null, false, "#1");

		Assert (d < MyEnum.Value_1, false, "#2");
		Assert (d < 0, false, "#2a");

		dynamic d2 = (MyEnumUlong?) MyEnumUlong.Value_2;
		Assert (d2 < MyEnumUlong.Value_2, false, "#3");
		Assert (d2 < null, false, "#3a");
	}

	void LessThanOrEqualTest ()
	{
		dynamic d = 5;

		int v = 2;
		Assert (d <= v, false, "#1");
		double v2 = 5;
		Assert (d <= v2, true, "#1a");

		d = 4.6;
		Assert (d <= 4.59, false, "#2");
		var b2 = 4.6;
		Assert (d <= b2, true, "#2a");

		d = new MyType (30);
		MyType v3 = new MyType (30);
		Assert (d <= v3, true, "#3");
		dynamic d3 = new MyType (-7);
		Assert (d3 <= new MyType (6), true, "#3a");

		d3 = new MyTypeImplicitOnly (-7);
		Assert (d3 <= 11, true, "#3b");

		d = 2m;
		decimal v4 = 4m;
		Assert (d <= v4, true, "#4");
		Assert (d <= 2m, true, "#4a");
	}

	void LessThanOrEqualNullableTest ()
	{
		dynamic d = 5;

		int? v2 = null;
		Assert (d <= v2, false, "#1");
		Assert (d <= null, false, "#1a");
		Assert (null <= d, false, "#1b");

		v2 = -2;
		Assert (d <= v2, false, "#2");
		dynamic d2 = (int?) -2;
		Assert (d2 <= 1, true, "#2a");
		d2 = (uint?) 44;
		Assert (d2 <= 44, true, "#2b");

		d = new MyType (30);
		MyType? v3 = new MyType (30);
		Assert (d <= v3, true, "#3");
		dynamic d3 = new MyType? (new MyType (-7));
		Assert (d3 <= new MyType (6), true, "#3a");
		Assert (d3 <= null, false, "#3b");

		d = 4.1m;
		decimal? v4 = 4m;
		Assert (d <= v4, false, "#4");
		v4 = null;
		Assert (d <= v4, false, "#4a");
	}

	void LessThanOrEqualEnumTest ()
	{
		dynamic d = MyEnum.Value_1;

		Assert (d <= null, false, "#1");

		Assert (d <= MyEnum.Value_1, true, "#2");
		Assert (d <= 0, false, "#2a");

		dynamic d2 = (MyEnumUlong?) MyEnumUlong.Value_2;
		Assert (d2 <= MyEnumUlong.Value_2, true, "#3");
		Assert (d2 <= null, false, "#3a");
	}

	void ModuloTest ()
	{
		dynamic d = 5;

		int v = 2;
		Assert (d % v, 1, "#1");

		d = new MyType (5);
		MyType v3 = new MyType (30);
		Assert (d % v3, new MyType (5), "#3");
		dynamic d3 = new MyType (-7);
		Assert<MyType> (d3 % new MyType (6), new MyType (-1), "#3a");

		d = new MyTypeImplicitOnly (5);
		decimal v4 = 4m;
		Assert (d % v4, 1m, "#4");
	}

	void ModuloNullableTest ()
	{
		dynamic d = 5;

		double? v2 = null;
		Assert<double?> (d % v2, null, "#1");
		Assert<double?> (d % null, null, "#1a");
		Assert<double?> (null % d, null, "#1b");

		v2 = -2;
		Assert (d % v2, 1, "#2");
		dynamic d2 = (int?) -2;
		Assert (d2 % 1, 0, "#2a");

		d = new MyType (-2);
		MyType? v3 = new MyType (30);
		Assert (d % v3, new MyType (-2), "#3");
		dynamic d3 = new MyType? (new MyType (-7));
		Assert (d3 % new MyType (6), new MyType (-1), "#3a");
		Assert<MyType?> (d3 + null, null, "#3b");

		d = new MyTypeImplicitOnly (5);
		decimal? v4 = 4m;
		Assert (d % v4, 1m, "#4");
		v4 = null;
		Assert<decimal?> (d % v4, null, "#4a");
	}

	void ModuloAssignTest ()
	{
		dynamic d = 5;

		int v = 2;
		d %= v;
		Assert (d, 1, "#1");

		d = 5.0;
		double v2 = 0.5;
		d %= v2;
		Assert (d, 0, "#1a");
		d %= v;
		Assert (d, 0, "#1b");

		dynamic d3 = new MyType (-7);
		d3 %= new MyType (6);
		Assert<MyType> (d3, new MyType (-1), "#3");

		d = 5m;
		decimal v4 = 4m;
		d %= v4;
		Assert (d, 1m, "#4");
	}

	void MultiplyTest ()
	{
		dynamic d = 5;

		int v = 2;
		Assert (d * v, 10, "#1");
		double v2 = 0.5;
		Assert (d * v2, 2.5, "#1a");

		d = new MyType (5);
		MyType v3 = new MyType (30);
		Assert (d * v3, new MyType (150), "#3");
		dynamic d3 = new MyType (-7);
		Assert<MyType> (d3 * new MyType (6), new MyType (-42), "#3a");

		decimal v4 = 4m;
		d = 7.9m;
		Assert (d * v4, 31.6m, "#4");
	}

	void MultiplyNullableTest ()
	{
		dynamic d = 5;

		int? v2 = null;
		Assert<int?> (d * v2, null, "#1");
		Assert<int?> (d * null, null, "#1a");
		Assert<int?> (null * d, null, "#1b");

		v2 = -2;
		Assert (d * v2, -10, "#2");
		dynamic d2 = (int?) -2;
		Assert (d2 * 1, -2, "#2a");

		d = new MyType (5);
		MyType? v3 = new MyType (30);
		Assert (d * v3, new MyType (150), "#3");
		dynamic d3 = new MyType? (new MyType (-7));
		Assert (d3 * new MyType (6), new MyType (-42), "#3a");
		Assert<MyType?> (d3 * null, null, "#3b");

		d = new MyTypeImplicitOnly (5);
		decimal? v4 = 4m;
		Assert (d * v4, 20m, "#4");
		v4 = null;
		Assert<decimal?> (d * v4, null, "#4a");
	}

	void MultiplyCheckedTest ()
	{
		checked {
			dynamic d = 5;

			int v = int.MaxValue;
			AssertChecked (() => d * v, 7, "#1");

			int? v2 = v;
			AssertChecked (() => d * v2, null, "#2");

			d = new MyType (4);
			MyType v3 = new MyType (int.MaxValue);
			Assert (d * v3, new MyType (-4), "#3");
		}
	}

	void MultiplyAssignTest ()
	{
		dynamic d = 5;

		int v = 2;
		d *= v;
		Assert (d, 10, "#1");

		d = 5.0;
		double v2 = 0.5;
		d *= v2;
		Assert (d, 2.5, "#1a");
		d *= v;
		Assert (d, 5, "#1b");

		dynamic d3 = new MyType (-7);
		d3 *= new MyType (6);
		Assert<MyType> (d3, new MyType (-42), "#3");

		d = 5m;
		decimal v4 = 4m;
		d *= v4;
		Assert (d, 20m, "#4");
		
		int i = 3;
		d = 5;
		i *= d;
		Assert (i, 15, "#5");
	}

	void MultiplyAssignCheckedTest ()
	{
		checked {
			dynamic d = 5;

			int v = int.MaxValue;
			AssertChecked (() => { d *= v; Assert (d, 0, "#1-"); }, "#1");

			d = new MyType (44);
			MyType v3 = new MyType (int.MaxValue);
			d *= v3;
			Assert (d, new MyType (-44), "#3-");
		}
	}

	void Negate ()
	{
		dynamic d = -8;
		Assert (8, -d, "#1");
		Assert (-8, -(-d), "#1a");

		d = new MyType (-14);
		Assert (new MyType (14), -d, "#2");

		d = new MyTypeImplicitOnly (4);
		Assert (-4, -d, "#3");

		d = (uint) 7;
		Assert (-7, -d, "#4");

		d = double.NegativeInfinity;
		Assert (double.PositiveInfinity, -d, "#5");
	}

	void NegateNullable ()
	{
		dynamic d = (int?) -8;
		Assert (8, -d, "#1");
		Assert (-8, -(-d), "#1a");

		MyType? n1 = new MyType (4);
		d = n1;
		Assert (new MyType (-4), -d, "#2");

		MyTypeImplicitOnly? n2 = new MyTypeImplicitOnly (4);
		d = n2;
		Assert (-4, -d, "#3");

		d = (sbyte?) 7;
		Assert (-7, -d, "#4");
	}

	void NegateChecked ()
	{
		checked {
			dynamic d = int.MinValue;
			AssertChecked (() => -d, 0, "#1");
		}
	}

	void Not ()
	{
		dynamic d = true;
		Assert (false, !d, "#1");

		var de = new MyType (-1);
		Assert (false, !d, "#2");
	}

	void NotEqualTest ()
	{
		dynamic d = 5;

		int v = 2;
		Assert (d != v, true, "#1");
		double v2 = 5;
		Assert (d != v2, false, "#1a");

		d = true;
		Assert (d != false, true, "#2");
		bool b2 = true;
		Assert (d != b2, false, "#2a");

		d = new MyType (30);
		MyType v3 = new MyType (30);
		Assert (d != v3, false, "#3");
		dynamic d3 = new MyType (-7);
		Assert (d3 != new MyType (6), true, "#3a");

		d = 2m;
		decimal v4 = 4m;
		Assert (d != v4, true, "#4");
		Assert (d != 2m, false, "#4a");

		d = null;
		Assert (d != null, false, "#5");
	}

	void NotEqualNullableTest ()
	{
		dynamic d = 5;

		int? v2 = null;
		Assert (d != v2, true, "#1");
		Assert (d != null, true, "#1a");
		Assert (null != d, true, "#1b");

		v2 = -2;
		Assert (d != v2, true, "#2");
		dynamic d2 = (int?) -2;
		Assert (d2 != 1, true, "#2a");
		d2 = (uint?) 44;
		Assert (d2 != 44, false, "#2b");

		d = new MyType (30);
		MyType? v3 = new MyType (30);
		Assert (d != v3, false, "#3");
		dynamic d3 = new MyType? (new MyType (-7));
		Assert (d3 != new MyType (6), true, "#3a");
		Assert (d3 != null, true, "#3b");

		d = 4.1m;
		decimal? v4 = 4m;
		Assert (d != v4, true, "#4");
		v4 = null;
		Assert (d != v4, true, "#4a");

		d = (bool?) true;
		Assert (d != true, false, "#5");
		Assert (d != null, true, "#5a");
		Assert (d != false, true, "#5b");
	}

	void NotEqualEnumTest ()
	{
		dynamic d = MyEnum.Value_1;

		Assert (d != null, true, "#1");

		Assert (d != MyEnum.Value_1, false, "#2");
		Assert (d != 0, true, "#2a");

		dynamic d2 = (MyEnumUlong?) MyEnumUlong.Value_2;
		Assert (d2 != MyEnumUlong.Value_2, false, "#3");
		Assert (d2 != null, true, "#3a");
	}

	void NotEqualStringTest ()
	{
		dynamic d = "text";

		Assert (d != "te", true, "#1");
		Assert (d != "text", false, "#1a");
		Assert (d != null, true, "#1b");
	}

	void OnesComplement ()
	{
		dynamic d = 7;
		Assert (-8, ~d, "#1");

		d = new MyType (-1);
		Assert (0, ~d, "#2");

		d = (ulong) 7;
		Assert (18446744073709551608, ~d, "#3");

		d = MyEnum.Value_1;
		Assert ((MyEnum) 254, ~d, "#4");
	}

	void OnesComplementNullable ()
	{
		dynamic d = (int?) 7;
		Assert (-8, ~d, "#1");

		d = (MyEnum?) MyEnum.Value_1;
		Assert ((MyEnum) 254, ~d, "#4");
	}

	void OrTest ()
	{
		dynamic d = true;

		var v = false;
		Assert (d | v, true, "#1");
		Assert (d | false, true, "#1a");

		d = 42;
		var v2 = 62;
		Assert (d | v2, 62, "#2");
		Assert (d | 0, 42, "#2a");

		d = new MyType (42);
		MyType v3 = new MyType (30);
		Assert (d | v3, new MyType (62), "#3");
		dynamic d3 = new MyType (-7);
		Assert<MyType> (d3 | new MyType (6), new MyType (-1), "#3a");

		d3 = new MyTypeImplicitOnly (-7);
		Assert (d3 | 11, -5, "#3b");
	}

	void OrTestEnum ()
	{
		dynamic d = MyEnum.Value_1;

		Assert<MyEnum?> (d | null, null, "#1");

		Assert (d | d, MyEnum.Value_1, "#2");

		dynamic d2 = (MyEnumUlong?) MyEnumUlong.Value_1;
		Assert<MyEnumUlong> (d2 | MyEnumUlong.Value_2, (MyEnumUlong) 3, "#3");
	}

	void OrTestNullable ()
	{
		dynamic d = 5;

		int? v2 = null;
		Assert<int?> (d | v2, null, "#1");
		Assert<int?> (d | null, null, "#1a");
		Assert<int?> (null | d, null, "#1b");

		v2 = -2;
		Assert (d | v2, -1, "#2");
		dynamic d2 = (int?) -2;
		Assert (d2 | 1, -1, "#2a");

		d = new MyType (-2);
		MyType? v3 = new MyType (30);
		Assert (d | v3, new MyType (-2), "#3");
		dynamic d3 = new MyType? (new MyType (-7));
		Assert (d3 | new MyType (6), new MyType (-1), "#3a");
	}

	void OrAssignedTest ()
	{
		dynamic d = true;

		var v = false;
		d |= v;
		Assert (d, true, "#1");
		d = true;
		d |= true;
		Assert (d, true, "#1a");

		d = 42;
		var v2 = 62;
		d |= v2;
		Assert (d, 62, "#2");

		MyType v3 = new MyType (30);
		dynamic d3 = new MyType (-7);
		d3 |= new MyType (6);
		Assert<MyType> (d3, new MyType (-1), "#3");
	}

	void OrAssignedTestEnum ()
	{
		dynamic d = MyEnum.Value_1;
		d |= MyEnum.Value_2;
		Assert<MyEnum> (d, (MyEnum) 3, "#1");

		d = MyEnum.Value_2;
		d |= d;
		Assert (d, MyEnum.Value_2, "#2");

		dynamic d2 = (MyEnumUlong?) MyEnumUlong.Value_1;
		Assert<MyEnumUlong> (d2 | MyEnumUlong.Value_2, (MyEnumUlong)3, "#3");
	}

	void OrElseTest ()
	{
		dynamic d = true;

		var v = false;
		Assert<bool> (d || v, true, "#1");

		Assert (d || true, true, "#1a");

		d = true;
		Assert (d || d, true, "#2");

		dynamic d3 = new MyType (-7);
		Assert<MyType> (d3 || new MyType (6), new MyType (-1), "#3");
	}

	void RightShiftTest ()
	{
		dynamic d = (ulong) 0x7F000;

		int v = 2;
		Assert<ulong> (d >> v, 0x1FC00, "#1");
		Assert<ulong> (d >> 1, 0x3F800, "#1a");
		short s = 2;
		Assert<ulong> (d >> s, 0x1FC00, "#1b");

		d = 0x7F000;
		MyTypeImplicitOnly v3 = new MyTypeImplicitOnly (3);
		Assert (d >> v3, 0xFE00, "#3");
		dynamic d3 = new MyType (-7);
		Assert (d3 >> new MyTypeImplicitOnly (11), -1, "#3a");
	}

	void RightShiftNullableTest ()
	{
		dynamic d = 5;

		int? v2 = null;
		Assert<int?> (d >> v2, null, "#1");
		d = 5;
		Assert<int?> (d >> null, null, "#1a");
		d = 5;
		Assert<int?> (null >> d, null, "#1b");

		v2 = -2;
		Assert (d >> v2, 0, "#2");
		dynamic d2 = (int?) -200;
		Assert (d2 >> 1, -100, "#2a");
	}

	void RightShiftAssignTest ()
	{
		dynamic d = 0x7F000;

		int v = 2;
		d >>= v;
		Assert (d, 0x1FC00, "#1");
		d >>= 1;
		Assert (d, 0xFE00, "#1a");
		sbyte s = 2;
		d >>= s;
		Assert (d, 0x3F80, "#1b");
	}

	void RightShiftAssignNullableTest ()
	{
		dynamic d = 0x2A0;

		var v2 = -2;
		d >>= v2;
		Assert (d, 0, "#2");
		dynamic d2 = (int?) -2;
		d2 >>= 1;
		Assert (d2, -1, "#2a");
	}

	void SubtractTest ()
	{
		dynamic d = 5;

		int v = 2;
		Assert (d - v, 3, "#1");
		double v2 = 0.5;
		Assert (d - v2, 4.5, "#1a");

		d = new MyType (5);
		MyType v3 = new MyType (30);
		Assert (d - v3, new MyType (-25), "#3");
		dynamic d3 = new MyType (-7);
		Assert (d3 - new MyType (6), new MyType (-13), "#3a");

		d = new MyTypeImplicitOnly (5);
		decimal v4 = 4m;
		Assert (d - v4, 1m, "#4");
	}

	void SubtractNullableTest ()
	{
		dynamic d = 5;

		int? v2 = null;
		Assert<int?> (d - v2, null, "#1");
		Assert<int?> (d - null, null, "#1a");
		Assert<int?> (null - d, null, "#1b");

		v2 = -2;
		Assert (d - v2, 7, "#2");
		dynamic d2 = (int?) -2;
		Assert (d2 - 1, -3, "#2a");

		d = new MyType (5);
		MyType? v3 = new MyType (30);
		Assert (d - v3, new MyType (-25), "#3");
		dynamic d3 = new MyType? (new MyType (-7));
		Assert (d3 - new MyType (6), new MyType (-13), "#3a");
		Assert<MyType?> (d3 - null, null, "#3b");

		d = new MyTypeImplicitOnly (5);
		decimal? v4 = 4m;
		Assert (d - v4, 1m, "#4");
		v4 = null;
		Assert<decimal?> (d - v4, null, "#4a");
	}

	void SubtractEnumTest ()
	{
		dynamic d = MyEnum.Value_1;

		Assert<MyEnum> (d - 1, 0, "#1");

		dynamic d2 = (MyEnumUlong?) MyEnumUlong.Value_2;
		Assert (d2 - (byte) 1, MyEnumUlong.Value_1, "#2");
		Assert<MyEnumUlong?> (d2 - null, null, "#2a");
		
		// CSC: Invalid System.InvalidOperationException
		Assert<MyEnum?> (d - null, null, "#3");
	}

	void SubtractCheckedTest ()
	{
		checked {
			dynamic d = 5;

			int v = int.MinValue;
			AssertChecked (() => d - v, 7, "#1");

			int? v2 = v;
			AssertChecked (() => d - v2, null, "#2");

			d = new MyType (5);
			MyType v3 = new MyType (int.MinValue);
			Assert (d - v3, new MyType (-2147483643), "#3");
		}
	}

	void SubtractAssignTest ()
	{
		dynamic d = 5;

		int v = 2;
		d -= v;
		Assert (d, 3, "#1");

		d = 5.0;
		double v2 = 0.5;
		d -= v2;
		Assert (d, 4.5, "#1a");
		d -= v;
		Assert (d, 2.5, "#1b");

		dynamic d3 = new MyType (-7);
		d3 -= new MyType (6);
		Assert<MyType> (d3, new MyType (-13), "#3");

		d = 5m;
		decimal v4 = 4m;
		d -= v4;
		Assert (d, 1m, "#4");
	}

	void SubtractAssignEnumTest ()
	{
		dynamic d = MyEnum.Value_1;

		d -= 1;
		Assert<MyEnum> (d, 0, "#2");

		dynamic d2 = (MyEnumUlong?) MyEnumUlong.Value_2;
		d2 -= (byte) 1;
		Assert (d2, MyEnumUlong.Value_1, "#3");
	}

	void SubtractAssignCheckedTest ()
	{
		checked {
			dynamic d = 5;

			int v = int.MinValue;
			AssertChecked (() => { d -= v; Assert (d, 0, "#1a"); }, "#1");

			d = new MyType (5);
			MyType v3 = new MyType (int.MinValue);
			d -= v3;
			Assert (d, new MyType (-2147483643), "#3a");
		}
	}

	void SubtractAssignEvent ()
	{
		Action print = () => { Console.WriteLine ("foo"); };
		dynamic d = print;
		
		// FIXME: Will have to special case events
		//ev_assign -= d;
		//ev_assign ();
	}

	void UnaryDecrement ()
	{
		dynamic d = 3;
		Assert (3, d--, "#1");
		Assert (2, d, "#1a");

		d = 3;
		Assert (2, --d, "#2");
		Assert (2, d, "#2a");

		d = new MyType (-3);
		Assert (new MyType (-3), d--, "#3");
		Assert (new MyType (-1), d, "#3a");
	}

	void UnaryDecrementCheckedTest ()
	{
		checked {
			dynamic d = int.MinValue;

			AssertChecked (() => { d--; Assert (d, -1073741824, "#1a"); }, "#1");

			d = new MyType (int.MinValue);
			d--;
			Assert (d, new MyType (-1073741824), "#2");
		}
	}

	void UnaryIncrement ()
	{
		dynamic d = 3;
		Assert (3, d++, "#1");
		Assert (4, d, "#1a");

		d = 3;
		Assert (4, ++d, "#2");
		Assert (4, d, "#2a");

		d = new MyType (-3);
		Assert (new MyType (-3), d++, "#3");
		Assert (new MyType (-6), d, "#3a");
	}

	void UnaryIncrementCheckedTest ()
	{
		checked {
			dynamic d = int.MaxValue;

			AssertChecked (() => { d++; Assert (d, 0, "#1a"); }, "#1");

			d = new MyType (int.MaxValue);
			d++;
			Assert (d, new MyType (-2), "#2");
		}
	}

	//void UnaryIsFalse ()
	//{
	//    dynamic d = this;
	//    object r = d == null;
	//    Assert (false, (bool) r, "#1");
	//    Assert<object> (true, d != null, "#1a");
	//}

	void UnaryIsTrue ()
	{
		dynamic d = true;
		Assert (3, d ? 3 : 5, "#1");

		d = 4;
		Assert (false, d < 1, "#2");

		d = new InverseLogicalOperator (true);
		Assert (1, d ? 1 : -1, "#3");
	}

	void UnaryPlus ()
	{
		dynamic d = -8;
		Assert (-8, +d, "#1");
		Assert (-8, +(+d), "#1a");

		d = new MyType (14);
		Assert (new MyType (334455), +d, "#2");

		d = new MyTypeImplicitOnly (4);
		Assert (4, +d, "#3");

		d = (uint) 7;
		Assert<uint> (7, +d, "#4");
	}

	void UnaryPlusNullable ()
	{
		dynamic d = (int?) -8;
		Assert (-8, +d, "#1");
		Assert (-8, +(+d), "#1a");

		MyType? n1 = new MyType (4);
		d = n1;
		Assert (new MyType (334455), +d, "#2");

		MyTypeImplicitOnly? n2 = new MyTypeImplicitOnly (4);
		d = n2;
		Assert (4, +d, "#3");

		d = (sbyte?) 7;
		Assert (7, +d, "#4");
	}

#pragma warning restore 169

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

