using System;

public class Math { //Only append content to this class as the test suite depends on line info
	public static int IntAdd (int a, int b) {
		int c = a + b; 
		int d = c + b;
		int e = d + a;
		int f = 0;
		return e;
	}

	public static int UseComplex (int a, int b) {
		var complex = new Simple.Complex (10, "xx");
		int c = a + b; 
		int d = c + b;
		int e = d + a;
		int f = 0;
		e += complex.DoStuff ();
		return e;
	}

	delegate bool IsMathNull (Math m);

	public static int DelegatesTest () {
		Func<Math, bool> fn_func = (Math m) => m == null;
		Func<Math, bool> fn_func_null = null;
		Func<Math, bool>[] fn_func_arr = new Func<Math, bool>[] { (Math m) => m == null };

		Math.IsMathNull fn_del = Math.IsMathNullDelegateTarget;
		var fn_del_arr = new Math.IsMathNull[] { Math.IsMathNullDelegateTarget };
		var m_obj = new Math ();
		Math.IsMathNull fn_del_null = null;
		bool res = fn_func (m_obj) && fn_del (m_obj) && fn_del_arr[0] (m_obj) && fn_del_null == null && fn_func_null == null && fn_func_arr[0] != null;

		// Unused locals

		Func<Math, bool> fn_func_unused = (Math m) => m == null;
		Func<Math, bool> fn_func_null_unused = null;
		Func<Math, bool>[] fn_func_arr_unused = new Func<Math, bool>[] { (Math m) => m == null };

		Math.IsMathNull fn_del_unused = Math.IsMathNullDelegateTarget;
		Math.IsMathNull fn_del_null_unused = null;
		var fn_del_arr_unused = new Math.IsMathNull[] { Math.IsMathNullDelegateTarget };
		Console.WriteLine ("Just a test message, ignore");
		return res ? 0 : 1;
	}

	public static int GenericTypesTest () {
		var list = new System.Collections.Generic.Dictionary<Math[], IsMathNull> ();
		System.Collections.Generic.Dictionary<Math[], IsMathNull> list_null = null;

		var list_arr = new System.Collections.Generic.Dictionary<Math[], IsMathNull>[] { new System.Collections.Generic.Dictionary<Math[], IsMathNull> () };
		System.Collections.Generic.Dictionary<Math[], IsMathNull>[] list_arr_null = null;

		Console.WriteLine ($"list_arr.Length: {list_arr.Length}, list.Count: {list.Count}");

		// Unused locals

		var list_unused = new System.Collections.Generic.Dictionary<Math[], IsMathNull> ();
		System.Collections.Generic.Dictionary<Math[], IsMathNull> list_null_unused = null;

		var list_arr_unused = new System.Collections.Generic.Dictionary<Math[], IsMathNull>[] { new System.Collections.Generic.Dictionary<Math[], IsMathNull> () };
		System.Collections.Generic.Dictionary<Math[], IsMathNull>[] list_arr_null_unused = null;

		Console.WriteLine ("Just a test message, ignore");
		return 0;
	}

	static bool IsMathNullDelegateTarget (Math m) => m == null;

	public static void OuterMethod ()
	{
		Console.WriteLine ($"OuterMethod called");
		var nim = new Math.NestedInMath ();
		var i = 5;
		var text = "Hello";
		var new_i = nim.InnerMethod (i);
		Console.WriteLine ($"i: {i}");
		Console.WriteLine ($"-- InnerMethod returned: {new_i}, nim: {nim}, text: {text}");
		int k = 19;
		new_i = InnerMethod2 ("test string", new_i, out k);
		Console.WriteLine ($"-- InnerMethod2 returned: {new_i}, and k: {k}");
	}

	static int InnerMethod2 (string s, int i, out int k)
	{
		k = i + 10;
		Console.WriteLine ($"s: {s}, i: {i}, k: {k}");
		return i - 2;
	}

	class NestedInMath
	{
		public int InnerMethod (int i)
		{
			SimpleStructProperty = new SimpleStruct () { dt = new DateTime (2020, 1, 2, 3, 4, 5) };
			int j = i + 10;
			string foo_str = "foo";
			Console.WriteLine ($"i: {i} and j: {j}, foo_str: {foo_str} ");
			j += 9;
			Console.WriteLine ($"i: {i} and j: {j}");
			return j;
		}

		Math m = new Math ();
		public async System.Threading.Tasks.Task<bool> AsyncMethod0 (string s, int i)
		{
			string local0 = "value0";
			await System.Threading.Tasks.Task.Delay (1);
			Console.WriteLine ($"* time for the second await, local0: {local0}");
			await AsyncMethodNoReturn ();
			return true;
		}

		public async System.Threading.Tasks.Task AsyncMethodNoReturn ()
		{
			var ss = new SimpleStruct () { dt = new DateTime (2020, 1, 2, 3, 4, 5) };
			var ss_arr = new SimpleStruct [] {};
			//ss.gs.StringField = "field in GenericStruct";

			//Console.WriteLine ($"Using the struct: {ss.dt}, {ss.gs.StringField}, ss_arr: {ss_arr.Length}");
			string str = "AsyncMethodNoReturn's local";
			//Console.WriteLine ($"* field m: {m}");
			await System.Threading.Tasks.Task.Delay (1);
			Console.WriteLine ($"str: {str}");
		}

		public static async System.Threading.Tasks.Task<bool> AsyncTest (string s, int i)
		{
			return await new NestedInMath().AsyncMethod0 (s, i);
		}

		public SimpleStruct SimpleStructProperty { get; set; }
	}

	struct SimpleStruct
	{
		public DateTime dt;
		public GenericStruct<DateTime> gs;
	}

	public struct GenericStruct<T>
	{
		public System.Collections.Generic.List<T> List;
		public string StringField;
	}

}
