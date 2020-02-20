using System;

public class Math { //Only append content to this class as the test suite depends on line info
	public static int IntAdd (int a, int b) {
		int c = a + b; 
		int d = c + b;
		int e = d + a;
		int f = 0;
		return e;
	}

	public static int UseComplex () {
		var complex = new Simple.Complex (10, "xx");
		var res = complex.DoStuff ();
		return res;
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
}
