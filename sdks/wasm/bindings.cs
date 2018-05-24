using System;
using System.Text;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace WebAssembly {
	/*
	TODO:
		Expose annotated C# type to JS
		Add property fetch to JSObject
		Add typed method invoke support (get a delegate?)
		Add JS helpers to fetch wrapped methods, like to Module.cwrap
		Better Wrap C# exception when passing them as object (IE, on task failure)
		Make JSObject disposable (same for js objects)
	*/
	public sealed class Runtime {
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern string InvokeJS (string str, out int exceptional_result);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern object InvokeJSWithArgs (int js_obj_handle, string method, object[] _params, out int exceptional_result);

		public static string InvokeJS (string str)
		{
			int exception = 0;
			var res = InvokeJS (str, out exception);
			if (exception != 0)
				throw new JSException (res);
			return res;
		}

		static Dictionary<int, JSObject> bound_objects = new Dictionary<int, JSObject>();
		static Dictionary<object, JSObject> raw_to_js = new Dictionary<object, JSObject>();

		static int BindJSObject (int js_id) {
			JSObject obj;
			if (bound_objects.ContainsKey (js_id))
				obj = bound_objects [js_id];
			else
				bound_objects [js_id] = obj = new JSObject (js_id);

			return (int)(IntPtr)obj.Handle;
		}

		static object CreateTaskSource (int js_id) {
			return new TaskCompletionSource<object> ();
		}

		static void SetTaskSourceResult (TaskCompletionSource<object> tcs, object result) {
			tcs.SetResult (result);
		}

		static void SetTaskSourceFailure (TaskCompletionSource<object> tcs, string reason) {
			tcs.SetException (new JSException (reason));
		}

		static int GetTaskAndBind (TaskCompletionSource<object> tcs, int js_id) {
			return BindExistingObject (tcs.Task, js_id);
		}

		static int BindExistingObject (object raw_obj, int js_id) {
			JSObject obj;
			if (raw_obj is JSObject)
				obj =(JSObject)raw_obj;
			else if (raw_to_js.ContainsKey (raw_obj))
				obj = raw_to_js [raw_obj];
			else
				raw_to_js [raw_obj] = obj = new JSObject (js_id, raw_obj);

			return (int)(IntPtr)obj.Handle;
		}

		static int GetJSObjectId (object raw_obj) {
			JSObject obj = null;
			if (raw_obj is JSObject)
				obj =(JSObject)raw_obj;
			else if (raw_to_js.ContainsKey (raw_obj))
				obj = raw_to_js [raw_obj];

			var js_handle = obj != null ? obj.JSHandle : -1;

			return js_handle;
		}

		static object GetMonoObject (int gc_handle) {
			GCHandle h = (GCHandle)(IntPtr)gc_handle;
			JSObject o = (JSObject)h.Target;
			if (o != null && o.RawObject != null)
				return o.RawObject;
			return o;
		}

		static object BoxInt (int i) {
			return i;
		}
		static object BoxDouble (double d) {
			return d;
		}

		static object BoxBool (int b) {
			return b == 0 ? false : true;
		}

		[StructLayout(LayoutKind.Explicit)]
		internal struct IntPtrAndHandle {
			[FieldOffset(0)]
			internal IntPtr ptr;

			[FieldOffset(0)]
			internal RuntimeMethodHandle handle;
		}

		//FIXME this probably won't handle generics
		static string GetCallSignature (IntPtr method_handle) {
			IntPtrAndHandle tmp = default (IntPtrAndHandle);
			tmp.ptr = method_handle;

			var mb = MethodBase.GetMethodFromHandle (tmp.handle);

			string res = "";
			foreach (var p in mb.GetParameters ()) {
				var t = p.ParameterType;

				switch (Type.GetTypeCode (t)) {
				case TypeCode.Byte:
				case TypeCode.SByte:
				case TypeCode.Int16:
				case TypeCode.UInt16:
				case TypeCode.Int32:
				case TypeCode.UInt32:
					res += "i";
					break;
				case TypeCode.Int64:
				case TypeCode.UInt64:
					res += "l";
					break;
				case TypeCode.Single:
					res += "f";
					break;
				case TypeCode.Double:
					res += "d";
					break;
				case TypeCode.String:
					res += "s";
					break;
				default:
					if (t.IsValueType)
						throw new Exception ("Can't handle VT arguments");
					res += "o";
					break;
				}
			}
			return res;
		}

		static MethodInfo gsjsc;
		static void GenericSetupJSContinuation<T> (Task<T> task, JSObject cont_obj) {
			task.GetAwaiter().OnCompleted (() => {
				//FIXME we should dispose cont_obj after completing the Promise
				if (task.Exception != null)
					cont_obj.Invoke ("reject", task.Exception.ToString ());
				else {
					cont_obj.Invoke ("resolve", task.Result);
				}
			});
		}

		static void SetupJSContinuation (Task task, JSObject cont_obj) {
			if (task.GetType () == typeof (Task)) {
				task.GetAwaiter().OnCompleted (() => {
					//FIXME we should dispose cont_obj after completing the Promise
					if (task.Exception != null)
						cont_obj.Invoke ("reject", task.Exception.ToString ());
					else
						cont_obj.Invoke ("resolve", null);
				});
			} else {
				//FIXME this is horrible codegen, we can do better with per-method glue
				if (gsjsc == null)
					gsjsc = typeof (Runtime).GetMethod ("GenericSetupJSContinuation", BindingFlags.NonPublic | BindingFlags.Static);
				gsjsc.MakeGenericMethod (task.GetType ().GetGenericArguments()).Invoke (null, new object[] { task, cont_obj });
			}
		}
	}

	public class JSException : Exception {
		public JSException (string msg) : base (msg) {}
	}

	public class JSObject {
		internal int JSHandle;
		internal GCHandle Handle;
		internal object RawObject;

		internal JSObject (int js_handle) {
			this.JSHandle = js_handle;
			this.Handle = GCHandle.Alloc (this);
		}

		internal JSObject (int js_id, object raw_obj) {
			this.JSHandle = js_id;
			this.Handle = GCHandle.Alloc (this);
			this.RawObject = raw_obj;
		}

		public object Invoke (string method, params object[] args) {
			int exception = 0;
			var res = Runtime.InvokeJSWithArgs (JSHandle, method, args, out exception);
			if (exception != 0)
				throw new JSException ((string)res);
			return res;
		}

		public override string ToString () {
			return $"(js-obj js '{JSHandle}' mono '{(IntPtr)Handle} raw '{RawObject != null})";
		}
	}
}
