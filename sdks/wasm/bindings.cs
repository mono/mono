using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using NUnitLite.Runner;
using NUnit.Framework.Internal;
using NUnit.Framework.Api;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace WebAssembly {
	/*
	TODO:
		Expose annotated C# type to JS
		Add property fetch to JSObject
		Add typed method invoke support (get a delegate?)
		Task marshalling
		Add JS helpers to fetch wrapped methods, like to Module.cwrap
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
