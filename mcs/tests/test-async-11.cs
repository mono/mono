// Compiler options: -langversion:future

using System;
using System.Threading.Tasks;

// Async stack spilling tests

struct S
{
	public int value;
}

enum E : byte
{
	E_1,
	E_2
}

class G<T>
{
	public async Task<int> TestStack_1 (T t)
	{
		T[] a = new T[] { t };
		return Call (t, a[0], out t,
			await Task.Factory.StartNew (() => 3));
	}
	
	int Call (T t1, T t2, out T t3, int i)
	{
		t3 = t2;
		return 0;
	}
}

class C
{
	int field;

	int TestCall (ref int a, Type type, object o, ulong ul, int b)
	{
		field = a;
		
		if (type != typeof (string)) {
			return 1;
		}
		
		S s = (S) o;
		if (s.value != 4)
			return 2;
		
		if (ul != ulong.MaxValue)
			return 3;
		
		return 0;
	}
	
	static async Task<int> TestStack_1 ()
	{
		int v = 9;
		var array = new ulong[] { ulong.MaxValue };
		return new C ().TestCall (ref v, typeof (string), new S () { value = 4 }, array [0],
			await Task.Factory.StartNew (() => 3));
	}

	int TestCall2<T1, T2, T3, T4, T5, T6, T7> (T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7)
	{
		return 0;
	}
	
	static async Task<int> TestStack_2 (ulong value)
	{
		short v = 2;
		return new C ().TestCall2 ((byte) 1, v, value = 9999, float.MaxValue, double.MaxValue, decimal.MaxValue,
			await Task.Factory.StartNew (() => 3));
	}

	static async Task<int> TestStack_3 ()
	{
		var s = new S [] { new S () };
		s[0].value = 6;
		
		var s2 = new S [,] { { new S () }, {} };
		s2[0, 0].value = 3;
		
		TestCall3 (ref s [0], ref s2 [0, 0], s [0].value++,
			await Task.Factory.StartNew (() => 3));
		
		if (s [0].value != 10)
			return 1;
		
		if (s2 [0, 0].value != 20)
			return 1;

		return 0;
	}
	
	static int TestCall3 (ref S value, ref S value2, int value3, int i)
	{
		value.value = 10;
		value2.value = 20;
		return 0;
	}

	static async Task<int> TestStack_4 ()
	{
		var a1 = new [] { E.E_2 };
		var a2 = new [] { new S () { value = 5 } };
		var a3 = new [] { new C () };
		
		return TestCall4 (a1[0], a2[0], a3[0],
			await Task.Factory.StartNew (() => 3));
	}
	
	static int TestCall4 (E e, S s, C c, int i)
	{
		if (e != E.E_2)
			return 100;
		
		if (s.value != 5)
			return 101;
		
		if (i != 3)
			return 102;
		
		return 0;
	}

	public static int Main ()
	{
		Task<int> t;

		t = TestStack_1 ();
		if (!Task.WaitAll (new[] { t }, 1000))
			return 1;
		
		if (t.Result != 0)
			return 2;
		
		t = TestStack_2 (ulong.MaxValue);
		if (!Task.WaitAll (new[] { t }, 1000))
			return 3;
		
		if (t.Result != 0)
			return 4;
		
		t = TestStack_3 ();
		if (!Task.WaitAll (new[] { t }, 1000))
			return 4;
		
		if (t.Result != 0)
			return 5;
		
		t = TestStack_4 ();
		if (!Task.WaitAll (new[] { t }, 1000))
			return 6;
		
		if (t.Result != 0)
			return 7;
		
		var g = new G<sbyte> ();
		t = g.TestStack_1 (9);
		if (!Task.WaitAll (new[] { t }, 1000))
			return 8;
		
		if (t.Result != 0)
			return 9;
		
		Console.WriteLine ("ok");
		return 0;
	}
}
