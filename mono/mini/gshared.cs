using System;

//
// Test cases for generic sharing
// Should be run with -O=gshared,-inline
//

public struct GenStruct <T>
{
	public T t;
}

public class GenClass <T> where T : class, new ()
{
	public static T field_T;

	public T stsfld_ldsfld_T (T t) {
		field_T = t;
		return field_T;
	}

	public static T stsfld_ldsfld_T_static (T t) {
		field_T = t;
		return field_T;
	}

	public object box_struct_of_T (GenStruct <T> t) {
		return t;
	}

	public static object box_struct_of_T_static (GenStruct <T> t) {
		return t;
	}

	static T call2_T (T t) {
		string s = t as string;
		if (s != null) {
			T res = (s + "2") as T;
			return res;
		}
		return t;
	}

	public T call_static_T (T t) {
		return call2_T (t);
	}

	public static T call_static_T_static (T t) {
		return call2_T (t);
	}
}

public class Tests
{
	public static int Main () {
		return TestDriver.RunTests (typeof (Tests));
	}

	public static int test_0_ldsfld_stsfld_T () {
		object o = new object ();
		if (new GenClass <object> ().stsfld_ldsfld_T (o) != o)
			return 1;
		if (new GenClass <string> ().stsfld_ldsfld_T ("FOO") != "FOO")
			return 2;
		return 0;
	}

	public static int test_0_ldsfld_stsfld_T_static () {
		object o = new object ();
		if (GenClass <object>.stsfld_ldsfld_T_static (o) != o)
			return 1;
		if (GenClass <string>.stsfld_ldsfld_T_static ("FOO") != "FOO")
			return 2;
		return 0;
	}

	public static int test_0_box_struct_of_T () {
		var s = new GenStruct <string> ();
		s.t = "HELLO";
		object o = new GenClass <string> ().box_struct_of_T (s);

		GenStruct <string> s2 = (GenStruct <string>)o;

		if (s2.t != "HELLO")
			return 1;
		return 0;
	}

	public static int test_0_box_struct_of_T_static () {
		var s = new GenStruct <string> ();
		s.t = "HELLO";
		object o = GenClass <string>.box_struct_of_T_static (s);

		GenStruct <string> s2 = (GenStruct <string>)o;

		if (s2.t != "HELLO")
			return 1;
		return 0;
	}

	// Calling a shared static method from a shared method
	public static int test_0_call_static_T () {
		if (new GenClass <string> ().call_static_T ("FOO") != "FOO2")
			return 1;
		return 0;
	}

	public static int test_0_call_static_T_static () {
		if (GenClass <string>.call_static_T_static ("FOO") != "FOO2")
			return 1;
		return 0;
	}
}
