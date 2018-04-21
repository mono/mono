using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;
using WebAssembly;

public class TestClass {
	public static int i32_res;
	public static void InvokeI32 (int a, int b) {
		i32_res = a + b;
	}

	public static float f32_res;
	public static void InvokeFloat (float f) {
		f32_res = f;
	}

	public static double f64_res;
	public static void InvokeDouble (double d) {
		f64_res = d;
	}

	public static long i64_res;
	public static void InvokeLong (long l) {
		i64_res = l;
	}

	public static string string_res;
	public static void InvokeString (string s) {
		string_res = s;
	}

	public static string mkstr;
	public static string InvokeMkString() {
		mkstr = "lalalala";
		return mkstr;
	}

	public static object obj1;
	public static object InvokeObj1(object obj)
	{
		obj1 = obj;
		return obj;
	}

	public static object obj2;
	public static object InvokeObj2(object obj)
	{
		obj2 = obj;
		return obj;
	}

	public static object mkobj;
	public static object InvokeMkobj()
	{
		mkobj = new object ();
		return mkobj;
	}

	public static int first_val, second_val;
	public static void PlayWithObj(JSObject obj) {
		first_val = (int)obj.Invoke ("inc");;
		second_val = (int)obj.Invoke("add", 20);
	}

	public static object[] js_objs;
	public static void PlayWithObjTypes (JSObject obj) {
		js_objs = new object[4];
		js_objs [0] = obj.Invoke ("return_int");
		js_objs [1] = obj.Invoke ("return_double");
		js_objs [2] = obj.Invoke ("return_string");
		js_objs [3] = obj.Invoke ("return_bool");
	}
}

[TestFixture]
public class BindingTests {
	[Test]
	public static void MarshalPrimitivesToCS ()
	{
		TestClass.i32_res = 0;
		Runtime.InvokeJS ("call_test_method(\"InvokeI32\", \"ii\", [10, 20])");
		Assert.AreEqual (TestClass.i32_res, 30);

		TestClass.f32_res = 0;
		Runtime.InvokeJS ("call_test_method(\"InvokeFloat\", \"f\", [1.5])");
		Assert.AreEqual (TestClass.f32_res, 1.5f);

		TestClass.f64_res = 0;
		Runtime.InvokeJS ("call_test_method(\"InvokeDouble\", \"d\", [4.5])");
		Assert.AreEqual (TestClass.f64_res, 4.5);

		TestClass.i64_res = 0;
		Runtime.InvokeJS ("call_test_method(\"InvokeLong\", \"l\", [99])");
		Assert.AreEqual (TestClass.i64_res, 99);
	}

	[Test]
	public static void MarshalStringToCS ()
	{
		TestClass.string_res = null;
		Runtime.InvokeJS ("call_test_method(\"InvokeString\", \"s\", [\"hello\"])");
		Assert.AreEqual (TestClass.string_res, "hello");
	}

	[Test]
	public static void MarshalStringToJS ()
	{
		TestClass.mkstr = TestClass.string_res = null;
		Runtime.InvokeJS (@"
			var str = call_test_method (""InvokeMkString"", ""o"", [ ]);
			call_test_method (""InvokeString"", ""s"", [ str ]);
		");
		Assert.IsNotNull(TestClass.mkstr);

		Assert.AreEqual (TestClass.mkstr, TestClass.string_res);

	}

	[Test]
	public static void JSObjectKeepIdentityAcrossCalls ()
	{
		TestClass.obj1 = TestClass.obj2 = null;
		Runtime.InvokeJS (@"
			var obj = { foo: 10 };
			var res = call_test_method (""InvokeObj1"", ""o"", [ obj ]);
			call_test_method (""InvokeObj2"", ""o"", [ res ]);
		");

		Assert.IsNotNull(TestClass.obj1);
		Assert.AreSame(TestClass.obj1, TestClass.obj2);
	}

	[Test]
	public static void CSObjectKeepIdentityAcrossCalls ()
	{
		TestClass.mkobj = TestClass.obj1 = TestClass.obj2 = null;
		Runtime.InvokeJS (@"
			var obj = call_test_method (""InvokeMkobj"", """", [ ]);
			var res = call_test_method (""InvokeObj1"", ""o"", [ obj ]);
			call_test_method (""InvokeObj2"", ""o"", [ res ]);
		");

		Assert.IsNotNull(TestClass.obj1);
		Assert.AreSame(TestClass.mkobj, TestClass.obj1);
		Assert.AreSame(TestClass.obj1, TestClass.obj2);
	}

	[Test]
	public static void JSInvokeInt() {
		Runtime.InvokeJS (@"
			var obj = {
				foo: 10,
				inc: function() {
					var c = this.foo;
					++this.foo;
					return c;
				},
				add: function(val){
					return this.foo + val;
				}
			};
			call_test_method (""PlayWithObj"", ""o"", [ obj ]);
		");

		Assert.AreEqual (TestClass.first_val, 10);
		Assert.AreEqual (TestClass.second_val, 31);
	}

	[Test]
	public static void JSInvokeTypes() {
		Runtime.InvokeJS (@"
			var obj = {
				return_int: function() { return 100; },
				return_double: function() { return 4.5; },
				return_string: function() { return 'qwerty'; },
				return_bool: function() { return true; },
			};
			call_test_method (""PlayWithObjTypes"", ""o"", [ obj ]);
		");

		Assert.AreEqual (TestClass.js_objs [0], 100);
		Assert.AreEqual (TestClass.js_objs [1], 4.5);
		Assert.AreEqual (TestClass.js_objs [2], "qwerty");
		Assert.AreEqual (TestClass.js_objs [3], true);
	}

}
