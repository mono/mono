using System;

//
// parser conditional and cast expression tests
//

class A<T>
{
	public static int Value;
}

public class ConditionalParsing
{
	class T
	{
		public T (string path, bool mode)
			: this (path, mode, (mode == true ? 1 : 2), 1, int.MaxValue)
		{
		}
		
		public T (string s, bool m, int i, int i2, int i3)
		{
		}
	}
	
	class Const
	{
		const int c = true ? 1 : 2;
	}
	
	struct S
	{
	}

	struct MyTestStruct : IDisposable
	{
		public void Dispose ()
		{
		}

		public static implicit operator MyTestStruct (int i)
		{
			return new MyTestStruct ();
		}
	}
	
	void Test_1 (bool a)
	{
		int? b = a ? 3 : 4;
	}
	
	void Test_2 ()
	{
		string mp = "";
		int a = 1;
		int _provider = mp.Length == a ? _provider = 4 : 5;
	}
	
	T? Test_3<T> (Func<T, T, T> result, T t) where T : struct
	{
		return new T? (result (t, t));
	}
	
	void Test_4 (bool x, bool y)
	{
		int s = x ? (y ? 2 : 4) : (y ? 5 : 7);
	}
	
	void Test_5 (bool a, IDisposable fs)
	{
		using (a ? fs : null) {
			Console.WriteLine ("");
		}
	}
	
	void Test_6 (bool a)
	{
		char[] escaped_text_chars =
			a != false ?
			new char [] {'&', '<', '>', '\r', '\n'} :
			new char [] {'&', '<', '>'};
	}
	
	void Test_7 (object o)
	{
		object a = (S?[]) o;
	}

	void Test_8 (DateTime value)
	{
		var	_endDate = value > DateTime.MinValue ? new DateTime ? (value) : null;
	}
	
	void Test_9 ()
	{
		bool b = (1 == 2);
		bool c = (1 == 1);
		string a = (b ? (c ? "#" : "#") : "");
	}

	void Test_10 ()
	{
		int i = new int [] { 1, 2, 3 } [1];
	}
	
	void Test_11 ()
	{
		int a = (int)(A<int>.Value);
	}
	
	static int Test_12 (bool arg)
	{
		return arg ? Foo (() => { return 1; }) : 1;
	}
	
	static int Foo (Func<int> arg)
	{
		return 1;
	}

	void Test_13 (object param)
	{
		if (param as bool? ?? false) {} else {}
	}

	int? Test_14 ()
	{
		bool a = false, b = false;
		object c = null;

		return a ? (b ? c as int? : null) : null;
	}

	Action<int> Test_15 (Action<int> arg)
	{
		return arg ?? (Helper<int>);
	}

	void Test_16 ()
	{
		bool? b = Test (1, arg:2);
	}

	void Test_17 ()
	{
		{
			using (MyTestStruct? mts = (int?) 1)
			{
			}
		}
	}

	void Test_18 (bool b, Action a)
	{
		var e = b ? () => { } : a;
	}

	void Test_19 (int[,] table)
	{
		var x = 1 > 0  ? table[5, 1] : 0;
	}

	void Test_20 ()
	{
		var t = (Object)string.Empty;
	}

	static void Helper<T> (T arg)
	{
	}

	static bool Test (object b, int arg)
	{
		return false;
	}

	public static void Main ()
	{
	}
}

