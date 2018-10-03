using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Reflection;

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

	public static byte[] byteBuffer;
	public static void MarshalByteBuffer (byte[] buffer) {
		byteBuffer = buffer;
	}	

	public static int[] intBuffer;
	public static void MarshalInt32Array (int[] buffer) {
		intBuffer = buffer;		
	}	

	public static void MarshalByteBufferToInts (byte[] buffer) {
        intBuffer = new int[buffer.Length / sizeof(int)];
        for (int i = 0; i < buffer.Length; i += sizeof(int))
	        intBuffer[i / sizeof(int)] = BitConverter.ToInt32(buffer, i);
	}	

	public static float[] floatBuffer;
	public static void MarshalFloat32Array (float[] buffer) {
		floatBuffer = buffer;
	}	

	public static void MarshalByteBufferToFloats (byte[] buffer) {
        floatBuffer = new float[buffer.Length / sizeof(float)];
        for (int i = 0; i < buffer.Length; i += sizeof(float))
	        floatBuffer[i / sizeof(float)] = BitConverter.ToSingle(buffer, i);
	}	


	public static double[] doubleBuffer;
	public static void MarshalFloat64Array (double[] buffer) {
		doubleBuffer = buffer;
	}	

	public static void MarshalByteBufferToDoubles (byte[] buffer) {
        doubleBuffer = new double[buffer.Length / sizeof(double)];
        for (int i = 0; i < buffer.Length; i += sizeof(double))
	        doubleBuffer[i / sizeof(double)] = BitConverter.ToDouble(buffer, i);
	}	

	public static void SetTypedArraySByte (JSObject obj) {
		sbyte[] buffer = Enumerable.Repeat((sbyte)0x20, 11).ToArray();
		obj.SetObjectProperty ("typedArray", buffer);
	}	

	public static sbyte[] taSByte;
	public static void GetTypedArraySByte (JSObject obj) {
		taSByte = (sbyte[])obj.GetObjectProperty ("typedArray");
	}	

	public static void SetTypedArrayByte (JSObject obj) {
		var dragons = "hic sunt dracones";
		byte[] buffer = System.Text.Encoding.ASCII.GetBytes(dragons);
		obj.SetObjectProperty ("dracones", buffer);
	}	

	public static byte[] taByte;
	public static void GetTypedArrayByte (JSObject obj) {
		taByte = (byte[])obj.GetObjectProperty ("dracones");
	}	

	public static void SetTypedArrayShort (JSObject obj) {
		short[] buffer = Enumerable.Repeat((short)0x20, 13).ToArray();
		obj.SetObjectProperty ("typedArray", buffer);
	}	

	public static short[] taShort;
	public static void GetTypedArrayShort (JSObject obj) {
		taShort = (short[])obj.GetObjectProperty ("typedArray");
	}	

	public static void SetTypedArrayUShort (JSObject obj) {
		ushort[] buffer = Enumerable.Repeat((ushort)0x20, 14).ToArray();
		obj.SetObjectProperty ("typedArray", buffer);
	}	

	public static ushort[] taUShort;
	public static void GetTypedArrayUShort (JSObject obj) {
		taUShort = (ushort[])obj.GetObjectProperty ("typedArray");
	}	


	public static void SetTypedArrayInt (JSObject obj) {
		int[] buffer = Enumerable.Repeat((int)0x20, 15).ToArray();
		obj.SetObjectProperty ("typedArray", buffer);
	}	

	public static int[] taInt;
	public static void GetTypedArrayInt (JSObject obj) {
		taInt = (int[])obj.GetObjectProperty ("typedArray");
	}	

	public static void SetTypedArrayUInt (JSObject obj) {
		uint[] buffer = Enumerable.Repeat((uint)0x20, 16).ToArray();
		obj.SetObjectProperty ("typedArray", buffer);
	}	

	public static uint[] taUInt;
	public static void GetTypedArrayUInt (JSObject obj) {
		taUInt = (uint[])obj.GetObjectProperty ("typedArray");
	}	

	public static void SetTypedArrayFloat (JSObject obj) {
		float[] buffer = Enumerable.Repeat(3.14f, 17).ToArray();
		obj.SetObjectProperty ("typedArray", buffer);
	}	

	public static float[] taFloat;
	public static void GetTypedArrayFloat (JSObject obj) {
		taFloat = (float[])obj.GetObjectProperty ("typedArray");
	}	


	public static void SetTypedArrayDouble (JSObject obj) {
		double[] buffer = Enumerable.Repeat(3.14d, 18).ToArray();
		obj.SetObjectProperty ("typedArray", buffer);
	}	

	public static double[] taDouble;
	public static void GetTypedArrayDouble (JSObject obj) {
		taDouble = (double[])obj.GetObjectProperty ("typedArray");
	}	

	public static HttpClient client;
	public static string fakeClientHandlerString;
	public static HttpClientHandler fakeClientHandler;
	public static void SetMessageHandler () {
		
		var httpMessageHandler = typeof(HttpClient).GetField("GetHttpMessageHandler", 
                            BindingFlags.Static | 
                            BindingFlags.NonPublic);

        httpMessageHandler.SetValue(null, (Func<HttpClientHandler>) (() => {
			return new FakeHttpClientHandler ();
		}));

		client = new HttpClient();
	}	

	public static RequestCache[] requestEnums;

	public static void SetRequestEnums (RequestCache dflt, RequestCache nostore, RequestCache reload, RequestCache nocache, RequestCache force, RequestCache onlyif) 
	{
		requestEnums = new RequestCache[6];
		requestEnums[0] = dflt;
		requestEnums[1] = nostore;
		requestEnums[2] = reload;
		requestEnums[3] = nocache;
		requestEnums[4] = force;
		requestEnums[5] = onlyif;
	}

	public static void SetRequestEnumsProperties (JSObject obj) 
	{
		obj.SetObjectProperty("dflt", RequestCache.Default);
		obj.SetObjectProperty("nostore", RequestCache.NoStore);
		obj.SetObjectProperty("reload", RequestCache.Reload);
		obj.SetObjectProperty("nocache", RequestCache.NoCache);
		obj.SetObjectProperty("force", RequestCache.ForceCache);
		obj.SetObjectProperty("onlyif", RequestCache.OnlyIfCached);
	}

}


public class FakeHttpClientHandler : HttpClientHandler
{
	public FakeHttpClientHandler () : base()
	{
		TestClass.fakeClientHandlerString = "Fake HttpClientHandler";
		TestClass.fakeClientHandler = this;
	}
}

public enum RequestCache
{
	[Export(EnumValue = ConvertEnum.Default)]
	Default = -1,
	[Export("no-store")]
	NoStore,
	[Export(EnumValue = ConvertEnum.ToUpper)]
	Reload,
	[Export(EnumValue = ConvertEnum.ToLower)]
	NoCache,
	[Export("force-cache")]
	ForceCache,
	OnlyIfCached = -3636,
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


	[Test]
	public static void MarshalArrayBuffer () {
		Runtime.InvokeJS (@"
			var buffer = new ArrayBuffer(16);
			call_test_method (""MarshalByteBuffer"", ""o"", [ buffer ]);		
		");

		Assert.AreEqual (16, TestClass.byteBuffer.Length);
	}

	[Test]
	public static void MarshalArrayBuffer2Int () {
		// This really does not work to be honest
		// The length of the marshalled array is 16 ints but 
		// the first 4 ints will be correct and the rest will 
		// probably be trash from memory
		Runtime.InvokeJS (@"
			var buffer = new ArrayBuffer(16);
			var int32View = new Int32Array(buffer);
			for (var i = 0; i < int32View.length; i++) {
  				int32View[i] = i * 2;
			}
			call_test_method (""MarshalInt32Array"", ""o"", [ buffer ]);		
		");

		Assert.AreEqual (16, TestClass.intBuffer.Length);
		Assert.AreEqual (0, TestClass.intBuffer[0]);
		Assert.AreEqual (2, TestClass.intBuffer[1]);
		Assert.AreEqual (4, TestClass.intBuffer[2]);
		Assert.AreEqual (6, TestClass.intBuffer[3]);
	}

	[Test]
	public static void MarshalArrayBuffer2Int2 () {

		Runtime.InvokeJS (@"
			var buffer = new ArrayBuffer(16);
			var int32View = new Int32Array(buffer);
			for (var i = 0; i < int32View.length; i++) {
  				int32View[i] = i * 2;
			}
			call_test_method (""MarshalByteBufferToInts"", ""o"", [ buffer ]);		
		");

		Assert.AreEqual (4, TestClass.intBuffer.Length);
		Assert.AreEqual (0, TestClass.intBuffer[0]);
		Assert.AreEqual (2, TestClass.intBuffer[1]);
		Assert.AreEqual (4, TestClass.intBuffer[2]);
		Assert.AreEqual (6, TestClass.intBuffer[3]);
	}

	
	[Test]
	public static void MarshalTypedArray () {
		Runtime.InvokeJS (@"
			var buffer = new ArrayBuffer(16);
			var uint8View = new Uint8Array(buffer);
			call_test_method (""MarshalByteBuffer"", ""o"", [ uint8View ]);		
		");

		Assert.AreEqual (16, TestClass.byteBuffer.Length);
	}

	[Test]
	public static void MarshalTypedArray2Int () {
		Runtime.InvokeJS (@"
			var buffer = new ArrayBuffer(16);
			var int32View = new Int32Array(buffer);
			for (var i = 0; i < int32View.length; i++) {
  				int32View[i] = i * 2;
			}
			call_test_method (""MarshalInt32Array"", ""o"", [ int32View ]);		
		");

		Assert.AreEqual (4, TestClass.intBuffer.Length);
		Assert.AreEqual (0, TestClass.intBuffer[0]);
		Assert.AreEqual (2, TestClass.intBuffer[1]);
		Assert.AreEqual (4, TestClass.intBuffer[2]);
		Assert.AreEqual (6, TestClass.intBuffer[3]);
	}

	[Test]
	public static void MarshalTypedArray2Float () {
		Runtime.InvokeJS (@"
			var typedArray = new Float32Array([1, 2.1334, 3, 4.2, 5]);
			call_test_method (""MarshalFloat32Array"", ""o"", [ typedArray ]);		
		");

		Assert.AreEqual (1, TestClass.floatBuffer[0]);
		Assert.AreEqual (2.1334f, TestClass.floatBuffer[1]);
		Assert.AreEqual (3, TestClass.floatBuffer[2]);
		Assert.AreEqual (4.2f, TestClass.floatBuffer[3]);
		Assert.AreEqual (5, TestClass.floatBuffer[4]);
	}

	[Test]
	public static void MarshalArrayBuffer2Float () {
		Runtime.InvokeJS (@"
			var buffer = new ArrayBuffer(16);
			var float32View = new Float32Array(buffer);
			for (var i = 0; i < float32View.length; i++) {
  				float32View[i] = i * 2.5;
			}
			call_test_method (""MarshalByteBufferToFloats"", ""o"", [ buffer ]);		
		");

		Assert.AreEqual (4, TestClass.floatBuffer.Length);
		Assert.AreEqual (0, TestClass.floatBuffer[0]);
		Assert.AreEqual (2.5f, TestClass.floatBuffer[1]);
		Assert.AreEqual (5, TestClass.floatBuffer[2]);
		Assert.AreEqual (7.5f, TestClass.floatBuffer[3]);
	}

	[Test]
	public static void MarshalArrayBuffer2Float2 () {
		// This really does not work to be honest
		// The length of the marshalled array is 16 floats but 
		// the first 4 floats will be correct and the rest will 
		// probably be trash from memory
		Runtime.InvokeJS (@"
			var buffer = new ArrayBuffer(16);
			var float32View = new Float32Array(buffer);
			for (var i = 0; i < float32View.length; i++) {
  				float32View[i] = i * 2.5;
			}
			call_test_method (""MarshalFloat32Array"", ""o"", [ buffer ]);		
		");

		Assert.AreEqual (16, TestClass.floatBuffer.Length);
		Assert.AreEqual (0, TestClass.floatBuffer[0]);
		Assert.AreEqual (2.5f, TestClass.floatBuffer[1]);
		Assert.AreEqual (5, TestClass.floatBuffer[2]);
		Assert.AreEqual (7.5f, TestClass.floatBuffer[3]);
	}

	[Test]
	public static void MarshalTypedArray2Double () {
		Runtime.InvokeJS (@"
			var typedArray = new Float64Array([1, 2.1334, 3, 4.2, 5]);
			call_test_method (""MarshalFloat64Array"", ""o"", [ typedArray ]);		
		");

		Assert.AreEqual (1, TestClass.doubleBuffer[0]);
		Assert.AreEqual (2.1334d, TestClass.doubleBuffer[1]);
		Assert.AreEqual (3, TestClass.doubleBuffer[2]);
		Assert.AreEqual (4.2d, TestClass.doubleBuffer[3]);
		Assert.AreEqual (5, TestClass.doubleBuffer[4]);
	}

	[Test]
	public static void MarshalArrayBuffer2Double () {
		Runtime.InvokeJS (@"
			var buffer = new ArrayBuffer(32);
			var float64View = new Float64Array(buffer);
			for (var i = 0; i < float64View.length; i++) {
  				float64View[i] = i * 2.5;
			}
			call_test_method (""MarshalByteBufferToDoubles"", ""o"", [ buffer ]);		
		");

		Assert.AreEqual (4, TestClass.doubleBuffer.Length);
		Assert.AreEqual (0, TestClass.doubleBuffer[0]);
		Assert.AreEqual (2.5d, TestClass.doubleBuffer[1]);
		Assert.AreEqual (5, TestClass.doubleBuffer[2]);
		Assert.AreEqual (7.5d, TestClass.doubleBuffer[3]);
	}

	[Test]
	public static void MarshalArrayBuffer2Double2 () {
		// This really does not work to be honest
		// The length of the marshalled array is 32 doubles but 
		// the first 4 doubles will be correct and the rest will 
		// probably be trash from memory
		Runtime.InvokeJS (@"
			var buffer = new ArrayBuffer(32);
			var float64View = new Float64Array(buffer);
			for (var i = 0; i < float64View.length; i++) {
  				float64View[i] = i * 2.5;
			}
			call_test_method (""MarshalFloat64Array"", ""o"", [ buffer ]);		
		");

		Assert.AreEqual (32, TestClass.doubleBuffer.Length);
		Assert.AreEqual (0, TestClass.doubleBuffer[0]);
		Assert.AreEqual (2.5f, TestClass.doubleBuffer[1]);
		Assert.AreEqual (5, TestClass.doubleBuffer[2]);
		Assert.AreEqual (7.5f, TestClass.doubleBuffer[3]);
	}

	[Test]
	public static void MarshalTypedArraySByte () {
		TestClass.int_val = 0;
		Runtime.InvokeJS (@"
			var obj = { };
			call_test_method (""SetTypedArraySByte"", ""o"", [ obj ]);
			call_test_method (""GetTypedArraySByte"", ""o"", [ obj ]);
		");
		Assert.AreEqual (11, TestClass.taSByte.Length);
		Assert.AreEqual (32, TestClass.taSByte[0]);
		Assert.AreEqual (32, TestClass.taSByte[TestClass.taSByte.Length - 1]);
	}

	[Test]
	public static void MarshalTypedArrayByte () {
		TestClass.int_val = 0;
		Runtime.InvokeJS (@"
			var obj = { };
			call_test_method (""SetTypedArrayByte"", ""o"", [ obj ]);
			call_test_method (""GetTypedArrayByte"", ""o"", [ obj ]);
		");
		Assert.AreEqual (17, TestClass.taByte.Length);
		Assert.AreEqual (104, TestClass.taByte[0]);
		Assert.AreEqual (115, TestClass.taByte[TestClass.taByte.Length - 1]);
		Assert.AreEqual ("hic sunt dracones", System.Text.Encoding.Default.GetString(TestClass.taByte));
	}

	[Test]
	public static void MarshalTypedArrayShort () {
		TestClass.int_val = 0;
		Runtime.InvokeJS (@"
			var obj = { };
			call_test_method (""SetTypedArrayShort"", ""o"", [ obj ]);
			call_test_method (""GetTypedArrayShort"", ""o"", [ obj ]);
		");
		Assert.AreEqual (13, TestClass.taShort.Length);
		Assert.AreEqual (32, TestClass.taShort[0]);
		Assert.AreEqual (32, TestClass.taShort[TestClass.taShort.Length - 1]);
	}

	[Test]
	public static void MarshalTypedArrayUShort () {
		TestClass.int_val = 0;
		Runtime.InvokeJS (@"
			var obj = { };
			call_test_method (""SetTypedArrayUShort"", ""o"", [ obj ]);
			call_test_method (""GetTypedArrayUShort"", ""o"", [ obj ]);
		");
		Assert.AreEqual (14, TestClass.taUShort.Length);
		Assert.AreEqual (32, TestClass.taUShort[0]);
		Assert.AreEqual (32, TestClass.taUShort[TestClass.taUShort.Length - 1]);
	}


	[Test]
	public static void MarshalTypedArrayInt () {
		TestClass.int_val = 0;
		Runtime.InvokeJS (@"
			var obj = { };
			call_test_method (""SetTypedArrayInt"", ""o"", [ obj ]);
			call_test_method (""GetTypedArrayInt"", ""o"", [ obj ]);
		");
		Assert.AreEqual (15, TestClass.taInt.Length);
		Assert.AreEqual (32, TestClass.taInt[0]);
		Assert.AreEqual (32, TestClass.taInt[TestClass.taInt.Length - 1]);
	}

	[Test]
	public static void MarshalTypedArrayUInt () {
		TestClass.int_val = 0;
		Runtime.InvokeJS (@"
			var obj = { };
			call_test_method (""SetTypedArrayUInt"", ""o"", [ obj ]);
			call_test_method (""GetTypedArrayUInt"", ""o"", [ obj ]);
		");
		Assert.AreEqual (16, TestClass.taUInt.Length);
		Assert.AreEqual (32, TestClass.taUInt[0]);
		Assert.AreEqual (32, TestClass.taUInt[TestClass.taUInt.Length - 1]);
	}

	[Test]
	public static void MarshalTypedArrayFloat () {
		TestClass.int_val = 0;
		Runtime.InvokeJS (@"
			var obj = { };
			call_test_method (""SetTypedArrayFloat"", ""o"", [ obj ]);
			call_test_method (""GetTypedArrayFloat"", ""o"", [ obj ]);
		");
		Assert.AreEqual (17, TestClass.taFloat.Length);
		Assert.AreEqual (3.14f, TestClass.taFloat[0]);
		Assert.AreEqual (3.14f, TestClass.taFloat[TestClass.taFloat.Length - 1]);
	}


	[Test]
	public static void MarshalTypedArrayDouble () {
		TestClass.int_val = 0;
		Runtime.InvokeJS (@"
			var obj = { };
			call_test_method (""SetTypedArrayDouble"", ""o"", [ obj ]);
			call_test_method (""GetTypedArrayDouble"", ""o"", [ obj ]);
		");
		Assert.AreEqual (18, TestClass.taDouble.Length);
		Assert.AreEqual (3.14d, TestClass.taDouble[0]);
		Assert.AreEqual (3.14d, TestClass.taDouble[TestClass.taDouble.Length - 1]);
	}


	[Test]
	public static void HttpMessageHandler () {
		TestClass.fakeClientHandlerString = string.Empty;
		TestClass.fakeClientHandler = null;
		TestClass.client = null;
		Runtime.InvokeJS (@"
			call_test_method (""SetMessageHandler"", ""o"", [  ]);
		");
		Assert.AreEqual ("Fake HttpClientHandler", TestClass.fakeClientHandlerString);
		Assert.AreNotEqual (null, TestClass.fakeClientHandler);
		Assert.AreEqual (typeof(FakeHttpClientHandler), TestClass.fakeClientHandler.GetType());
		Assert.AreNotEqual (null, TestClass.client);
	}

	[Test]
	public static void MarshalRequestEnums () {
		Runtime.InvokeJS (@"
		    var dflt = ""Default"";
			var nostore = ""no-store"";
			var reload = ""RELOAD"";
			var nocache = ""nocache"";
			var force = 3;
			var onlyif = -3636;
			Module.mono_call_static_method (""[binding_tests]TestClass:SetRequestEnums"", [ dflt, nostore, reload, nocache, force, onlyif ]);
		");
		Assert.AreEqual (RequestCache.Default, TestClass.requestEnums[0]);
		Assert.AreEqual (RequestCache.NoStore, TestClass.requestEnums[1]);
		Assert.AreEqual (RequestCache.Reload, TestClass.requestEnums[2]);
		Assert.AreEqual (RequestCache.NoCache, TestClass.requestEnums[3]);
		Assert.AreEqual (RequestCache.ForceCache, TestClass.requestEnums[4]);
		Assert.AreEqual (RequestCache.OnlyIfCached, TestClass.requestEnums[5]);
	}

	[Test]
	public static void MarshalRequestEnumProps () {
		Runtime.InvokeJS (@"
		    var obj = {};
			Module.mono_call_static_method (""[binding_tests]TestClass:SetRequestEnumsProperties"", [ obj ]);
			Module.mono_call_static_method (""[binding_tests]TestClass:SetRequestEnums"", [ obj.dflt, obj.nostore, obj.reload, obj.nocache, obj.force, obj.onlyif ]);
		");
		Assert.AreEqual (RequestCache.Default, TestClass.requestEnums[0]);
		Assert.AreEqual (RequestCache.NoStore, TestClass.requestEnums[1]);
		Assert.AreEqual (RequestCache.Reload, TestClass.requestEnums[2]);
		Assert.AreEqual (RequestCache.NoCache, TestClass.requestEnums[3]);
		Assert.AreEqual (RequestCache.ForceCache, TestClass.requestEnums[4]);
		Assert.AreEqual (RequestCache.OnlyIfCached, TestClass.requestEnums[5]);
	}

}
