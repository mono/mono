using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

	public static int int_val;
	public static void InvokeInt (int i) {
		int_val = i;
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

	public static int do_add;
	public static void UseFunction (JSObject obj) {
		do_add = (int)obj.Invoke("call", null, 10, 20);
	}

	public static int dele_res;
	public static Func<int, int, int> MkDelegate () {
		return (a, b) => {
			dele_res = a + b;
			return dele_res;
		};
	}

	public static TaskCompletionSource<int> tcs;
	public static Task<int> task;
	public static object MkTask () {
		tcs = new TaskCompletionSource<int> ();
		task = tcs.Task;
		return task;
	}

	public static TaskCompletionSource<object> tcs3;
	public static Task task3;
	public static object MkTaskNull () {
		tcs3 = new TaskCompletionSource<object> ();
		task3 = tcs3.Task;
		return task3;
	}

	public static Task<object> taskString;
	public static object MkTaskString () {
		tcs3 = new TaskCompletionSource<object> ();
		taskString = tcs3.Task;
		return taskString;
	}

	public static Task<object> the_promise;
	public static void InvokePromise (object obj) {
		the_promise = (Task<object>)obj;
		the_promise.ContinueWith((t,o) => {
			Console.WriteLine ("Promise result is {0}", t.Result);
		}, null, TaskContinuationOptions.ExecuteSynchronously); //must be sync cuz the mainloop pump is gone
	}

	public static List<JSObject> js_objs_to_dispose = new List<JSObject>();
	public static void DisposeObject(JSObject obj)
	{
		js_objs_to_dispose.Add(obj);
		obj.Dispose();
	}	

	public static object[] js_props;
	public static void RetrieveObjectProperties (JSObject obj) {
		js_props = new object[4];
		js_props [0] = obj.GetObjectProperty ("myInt");
		js_props [1] = obj.GetObjectProperty ("myDouble");
		js_props [2] = obj.GetObjectProperty ("myString");
		js_props [3] = obj.GetObjectProperty ("myBoolean");
	}	

	public static void PopulateObjectProperties (JSObject obj, bool createIfNotExist) {
		js_props = new object[4];
		obj.SetObjectProperty ("myInt", 100, createIfNotExist);
		obj.SetObjectProperty ("myDouble", 4.5, createIfNotExist);
		obj.SetObjectProperty ("myString", "qwerty", createIfNotExist);
		obj.SetObjectProperty ("myBoolean", true, createIfNotExist);
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

	[Test]
	public static void JSObjectApply() {
		Runtime.InvokeJS (@"
			var do_add = function(a, b) { return a + b};
			call_test_method (""UseFunction"", ""o"", [ do_add ]);
		");
		Assert.AreEqual (TestClass.do_add, 30);
	}

	[Test]
	public static void MarshalDelegate() {
		TestClass.obj1 = null;
		Runtime.InvokeJS (@"
			var dele = call_test_method (""MkDelegate"", """", [ ]);
			var res = dele (10, 20);
			call_test_method (""InvokeI32"", ""ii"", [ res, res ]);
		");

		Assert.AreEqual (TestClass.dele_res, 30);
		Assert.AreEqual (TestClass.i32_res, 60);
	}

	[Test]
	public static void PassTaskToJS () {
		TestClass.int_val = 0;
		Runtime.InvokeJS (@"
			var tsk = call_test_method (""MkTask"", """", [ ]);
			tsk.then (function (value) {
				Module.print ('PassTaskToJS cont with value ' + value);
			});
		");
		Assert.AreEqual (0, TestClass.int_val);
		TestClass.tcs.SetResult (99);
		//FIXME our test harness doesn't suppport async tests.
		// So manually verify it for now by checking stdout for `PassTaskToJS cont with value 99`
		//Assert.AreEqual (99, TestClass.int_val);
	}


	[Test]
	public static void PassTaskToJS2 () {
		TestClass.int_val = 0;
		Runtime.InvokeJS (@"
			var tsk = call_test_method (""MkTask"", """", [ ]);
			tsk.then (function (value) {},
			function (reason) {
				Module.print ('PassTaskToJS2 cont failed due to ' + reason);
			});
		");
		Assert.AreEqual (0, TestClass.int_val);
		TestClass.tcs.SetException (new Exception ("it failed"));
		//FIXME our test harness doesn't suppport async tests.
		// So manually verify it for now by checking stdout for `PassTaskToJS2 cont failed due to System.AggregateException...
		// Assert.AreEqual (99, TestClass.int_val);
	}

	[Test]
	public static void PassTaskToJS3 () {
		TestClass.int_val = 0;
		Runtime.InvokeJS (@"
			var tsk = call_test_method (""MkTaskNull"", """", [ ]);
			tsk.then( () => {
  				Module.print('PassTaskToJS3 cont without value '); // Success!
			}, reason => {
  				Module.print('PassTaskToJS3 cont failed due to ' + reason); // Error!
			} );
		");
		Assert.AreEqual (0, TestClass.int_val);
		TestClass.tcs3.SetResult (null);
	}

	[Test]
	public static void PassTaskToJS4 () {
		TestClass.int_val = 0;
		Runtime.InvokeJS (@"
			var tsk = call_test_method (""MkTaskNull"", """", [ ]);
			tsk.then( value => {
  				Module.print(value); // Success!
			}, reason => {
  				Module.print('PassTaskToJS4 cont failed due to ' + reason); // Error!
			} );
		");
		Assert.AreEqual (0, TestClass.int_val);
		TestClass.tcs3.SetException (new Exception ("it failed"));
	}	
	
	[Test]
	public static void PassTaskToJS5 () {
		TestClass.int_val = 0;
		Runtime.InvokeJS (@"
			var tsk = call_test_method (""MkTaskString"", """", [ ]);
			tsk.then( success => {
  				Module.print('PassTaskToJS5 cont with value ' + success); // Success!
			}, reason => {
  				Module.print('PassTaskToJS5 cont failed due to ' + reason); // Error!
			} );
		");
		Assert.AreEqual (0, TestClass.int_val);
		TestClass.tcs3.SetResult ("Success");
	}

	[Test]
	public static void PassTaskToJS6 () {
		TestClass.int_val = 0;
		Runtime.InvokeJS (@"
			var tsk = call_test_method (""MkTaskString"", """", [ ]);
			tsk.then( success => {
  				Module.print('PassTaskToJS6 cont with value ' + success); // Success!
			}, reason => {
  				Module.print('PassTaskToJS6 cont failed due to ' + reason); // Error!
			} );
		");
		Assert.AreEqual (0, TestClass.int_val);
		TestClass.tcs3.SetException (new Exception ("it failed"));
	}	

	[Test]
	public static void PassPromiseToCS () {
		TestClass.int_val = 0;
		Runtime.InvokeJS (@"
			var resolve_func = null;
			var promise = new Promise(function (resolve, reject) {
				resolve_func = resolve;
			});
			call_test_method (""InvokePromise"", ""o"", [ promise ]);
			resolve_func (111);
		");
		//FIXME our test harness doesn't suppport async tests.
		// So manually verify it for now by checking stdout for `Promise result is 111`
		// Assert.AreEqual (99, TestClass.int_val);
	}

	[Test]
	public static void BindStaticMethod () {
		TestClass.int_val = 0;
		Runtime.InvokeJS (@"
			var invoke_int = Module.mono_bind_static_method (""[binding_tests]TestClass:InvokeInt"");
			invoke_int (200);
		");

		Assert.AreEqual (200, TestClass.int_val);
	}

	[Test]
	public static void InvokeStaticMethod () {
		TestClass.int_val = 0;
		Runtime.InvokeJS (@"
			Module.mono_call_static_method (""[binding_tests]TestClass:InvokeInt"", [ 300 ]);
		");

		Assert.AreEqual (300, TestClass.int_val);
	}

	[Test]
	public static void ResolveMethod () {
		TestClass.int_val = 0;
		Runtime.InvokeJS (@"
			var invoke_int = Module.mono_method_resolve (""[binding_tests]TestClass:InvokeInt"");
			call_test_method (""InvokeInt"", ""i"", [ invoke_int ]);
		");

		Assert.AreNotEqual (0, TestClass.int_val);
	}

	[Test]
	public static void DisposeObject () {
		Runtime.InvokeJS (@"
			var obj1 = {
			};
			var obj2 = {
			};
			var obj3 = {
			};
			call_test_method (""DisposeObject"", ""o"", [ obj3 ]);
			call_test_method (""DisposeObject"", ""o"", [ obj2 ]);
			call_test_method (""DisposeObject"", ""o"", [ obj1 ]);
		");

		Assert.AreEqual (-1, TestClass.js_objs_to_dispose [0].JSHandle);
		Assert.AreEqual (-1, TestClass.js_objs_to_dispose [1].JSHandle);		
		Assert.AreEqual (-1, TestClass.js_objs_to_dispose [2].JSHandle);		
	}

	[Test]
	public static void GetObjectProperties () {
		Runtime.InvokeJS (@"
			var obj = {myInt: 100, myDouble: 4.5, myString: ""qwerty"", myBoolean: true};
			call_test_method (""RetrieveObjectProperties"", ""o"", [ obj ]);		
		");

		Assert.AreEqual (100, TestClass.js_props [0]);
		Assert.AreEqual (4.5, TestClass.js_props [1]);
		Assert.AreEqual ("qwerty", TestClass.js_props [2]);
		Assert.AreEqual (true, TestClass.js_props [3]);
	}

	[Test]
	public static void SetObjectProperties () {
		Runtime.InvokeJS (@"
			var obj = {myInt: 200, myDouble: 0, myString: ""foo"", myBoolean: false};
			call_test_method (""PopulateObjectProperties"", ""oi"", [ obj, false ]);		
			call_test_method (""RetrieveObjectProperties"", ""o"", [ obj ]);		
		");

		Assert.AreEqual (100, TestClass.js_props [0]);
		Assert.AreEqual (4.5, TestClass.js_props [1]);
		Assert.AreEqual ("qwerty", TestClass.js_props [2]);
		Assert.AreEqual (true, TestClass.js_props [3]);
	}

	[Test]
	public static void SetObjectPropertiesIfNotExistsFalse () {
		// This test will not create the properties if they do not already exist
		Runtime.InvokeJS (@"
			var obj = {myInt: 200};
			call_test_method (""PopulateObjectProperties"", ""oi"", [ obj, false ]);		
			call_test_method (""RetrieveObjectProperties"", ""o"", [ obj ]);		
		");

		Assert.AreEqual (100, TestClass.js_props [0]);
		Assert.AreEqual (null, TestClass.js_props [1]);
		Assert.AreEqual (null, TestClass.js_props [2]);
		Assert.AreEqual (null, TestClass.js_props [3]);
	}

	[Test]
	public static void SetObjectPropertiesIfNotExistsTrue () {
		// This test will set the value of the property if it exists and will create and 
		// set the value if it does not exists
		Runtime.InvokeJS (@"
			var obj = {myInt: 200};
			call_test_method (""PopulateObjectProperties"", ""oi"", [ obj, true ]);		
			call_test_method (""RetrieveObjectProperties"", ""o"", [ obj ]);		
		");

		Assert.AreEqual (100, TestClass.js_props [0]);
		Assert.AreEqual (4.5, TestClass.js_props [1]);
		Assert.AreEqual ("qwerty", TestClass.js_props [2]);
		Assert.AreEqual (true, TestClass.js_props [3]);
	}


}
