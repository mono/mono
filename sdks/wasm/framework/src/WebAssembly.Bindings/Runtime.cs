using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace WebAssembly {
	/// <summary>
	///   Provides access to the Mono/WebAssembly runtime to perform tasks like invoking JavaScript functions and retrieving global variables.
	/// </summary>
	public sealed class Runtime {
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern string InvokeJS (string str, out int exceptional_result);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern object InvokeJSWithArgs (int js_obj_handle, string method, object [] _params, out int exceptional_result);
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern object GetObjectProperty (int js_obj_handle, string propertyName, out int exceptional_result);
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern object SetObjectProperty (int js_obj_handle, string propertyName, object value, bool createIfNotExists, bool hasOwnProperty, out int exceptional_result);
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern object GetByIndex (int js_obj_handle, int index, out int exceptional_result);
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern object SetByIndex (int js_obj_handle, int index, object value, out int exceptional_result);
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern object GetGlobalObject (string globalName, out int exceptional_result);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern object ReleaseHandle (int js_obj_handle, out int exceptional_result);
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern object ReleaseObject (int js_obj_handle, out int exceptional_result);
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern object BindCoreObject (int js_obj_handle, int gc_handle, out int exceptional_result);
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern object BindHostObject (int js_obj_handle, int gc_handle, out int exceptional_result);
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern object New (string className, object [] _params, out int exceptional_result);
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern object TypedArrayToArray (int js_obj_handle, out int exceptional_result);
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern object TypedArrayCopyTo (int js_obj_handle, int array_ptr, int begin, int end, int bytes_per_element, out int exceptional_result);
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern object TypedArrayFrom (int array_ptr, int begin, int end, int bytes_per_element, int type, out int exceptional_result);
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern object TypedArrayCopyFrom (int js_obj_handle, int array_ptr, int begin, int end, int bytes_per_element, out int exceptional_result);

		/// <summary>
		/// Execute the provided string in the JavaScript context
		/// </summary>
		/// <returns>The js.</returns>
		/// <param name="str">String.</param>
		public static string InvokeJS (string str)
		{
			var res = InvokeJS (str, out int exception);
			if (exception != 0)
				throw new JSException (res);
			return res;
		}

		static readonly Dictionary<int, WeakReference> bound_objects = new Dictionary<int, WeakReference> ();

		// weak_delegate_table is a ConditionalWeakTable with the Delegate and associated JSObject:
		// Key Lifetime:
		//	Once the key dies, the dictionary automatically removes
		//	    the key/value entry.
		// No need to lock as it is thread safe.
		static readonly ConditionalWeakTable<Delegate, JSObject> weak_delegate_table = new ConditionalWeakTable<Delegate, JSObject> ();
		static Dictionary<object, JSObject> raw_to_js = new Dictionary<object, JSObject> ();

		static Runtime ()
		{ }

		/// <summary>
		/// Creates a new JavaScript object of the specified type
		/// </summary>
		/// <returns>The new.</returns>
		/// <param name="_params">Parameters.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static int New<T> (params object [] _params)
		{
			var res = New (typeof(T).Name, _params, out int exception);
			if (exception != 0)
				throw new JSException ((string)res);
			return (int)res;
		}

		/// <summary>
		/// Create a new JavaScript object of the host class name
		/// </summary>
		/// <param name="hostClassName">The name of the host class name of the object to create.</param>
		/// <param name="_params">Parameters</param>
		/// <returns></returns>
		public static int New (string hostClassName, params object [] _params)
		{
			var res = New (hostClassName, _params, out int exception);
			if (exception != 0)
				throw new JSException ((string)res);
			return (int)res;
		}

		static int BindJSObject (int js_id, bool ownsHandle, Type mappedType)
		{
			lock (bound_objects) {
				if (!bound_objects.TryGetValue (js_id, out var obj)) {
					if (mappedType != null) {
						return BindJSType (js_id, ownsHandle, mappedType);
					} else {
						bound_objects [js_id] = obj = new WeakReference (new JSObject ((IntPtr)js_id, ownsHandle), true);
					}
				}
				return (int)(IntPtr)((JSObject)obj.Target).AnyRefHandle;
			}
		}

		static int BindCoreCLRObject (int js_id, int gcHandle)
		{
			GCHandle h = (GCHandle)(IntPtr)gcHandle;
			JSObject obj = (JSObject)h.Target;

			lock (bound_objects) {
				if (bound_objects.TryGetValue (js_id, out var existingObj)) {
					var instance = existingObj.Target as JSObject;
					if (instance.AnyRefHandle != h && h.IsAllocated)
						throw new JSException ($"Multiple handles pointing at js_id: {js_id}");

					obj = instance;
				} else
					bound_objects [js_id] = new WeakReference (obj, true);

				return (int)(IntPtr)obj.AnyRefHandle;
			}
		}

		static int BindJSType (int js_id, bool ownsHandle, Type mappedType)
		{
			lock (bound_objects) {
				if (!bound_objects.TryGetValue (js_id, out var reference)) {
					var jsobjectnew = mappedType.GetConstructor (BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.ExactBinding,
						    null, new Type [] { typeof (IntPtr), typeof (bool) }, null);
					bound_objects [js_id] = reference = new WeakReference ((JSObject)jsobjectnew.Invoke (new object [] { (IntPtr)js_id, ownsHandle }), true);
				}
				return (int)(IntPtr)((JSObject)reference.Target).AnyRefHandle;
			}
		}

		static int UnBindJSObject (int js_id)
		{
			lock (bound_objects) {
				if (bound_objects.TryGetValue (js_id, out var reference)) {
					bound_objects.Remove (js_id);
					return (int)(IntPtr)((JSObject)reference.Target).AnyRefHandle;
				}
				return 0;
			}
		}

		static void UnBindJSObjectAndFree (int js_id)
		{
			lock (bound_objects) {
				if (bound_objects.TryGetValue (js_id, out var reference)) {
					var instance = reference.Target;
					if (instance == null) {
						bound_objects.Remove (js_id);
					} else {
						((JSObject)bound_objects [js_id].Target).RawObject = null;
						((JSObject)bound_objects [js_id].Target).WeakRawObject = null;
						bound_objects.Remove (js_id);
						var instanceJS = reference.Target as JSObject;
						instanceJS.SetHandleAsInvalid ();
						instanceJS.IsDisposed = true;
						instanceJS.RawObject = null;
						instanceJS.AnyRefHandle.Free ();
					}

				}
				
			}
		}


		internal static bool ReleaseJSObject (JSObject objToRelease)
		{
			Runtime.ReleaseHandle (objToRelease.JSHandle, out int exception);
			if (exception != 0)
				throw new JSException ($"Error releasing handle on (js-obj js '{objToRelease.JSHandle}' mono '{(IntPtr)objToRelease.AnyRefHandle} raw '{objToRelease.RawObject != null}' weak raw '{objToRelease.WeakRawObject?.Target != null}'   )");

			lock (bound_objects) {
				bound_objects.Remove (objToRelease.JSHandle);
				objToRelease.SetHandleAsInvalid ();
				objToRelease.IsDisposed = true;
				objToRelease.RawObject = null;
				objToRelease.WeakRawObject = null;
				objToRelease.AnyRefHandle.Free ();
			}
			return true;
		}

		static void UnBindRawJSObjectAndFree (int gcHandle)
		{

			GCHandle h = (GCHandle)(IntPtr)gcHandle;
			JSObject obj = (JSObject)h.Target;
			if (obj != null && obj.RawObject != null) {
				raw_to_js.Remove (obj.RawObject);

				int exception;
				ReleaseHandle (obj.JSHandle, out exception);
				if (exception != 0)
					throw new JSException ($"Error releasing handle on (js-obj js '{obj.JSHandle}' mono '{(IntPtr)obj.AnyRefHandle} raw '{obj.RawObject != null})");

				// Calling Release Handle above only removes the reference from the JavaScript side but does not 
				// release the bridged JSObject associated with the raw object so we have to do that ourselves.
				obj.SetHandleAsInvalid ();
				obj.IsDisposed = true;
				obj.RawObject = null;

				obj.AnyRefHandle.Free ();
			}

		}

		public static void FreeObject(object obj)
		{
			// We no longer need to free on delegates.
			// Leave this here for now so it does not break code.
			if (obj.GetType().IsSubclassOf(typeof(Delegate)))
			{
				return;
			}
			if (raw_to_js.TryGetValue (obj, out JSObject jsobj)) {
				raw_to_js [obj].RawObject = null;
				raw_to_js.Remove (obj);

				int exception;
				Runtime.ReleaseObject (jsobj.JSHandle, out exception);
				if (exception != 0)
					throw new JSException ($"Error releasing object on (raw-obj)");

				jsobj.SetHandleAsInvalid ();
				jsobj.RawObject = null;
				jsobj.IsDisposed = true;
				jsobj.AnyRefHandle.Free ();

			} else {
				throw new JSException ($"Error releasing object on (obj)");
			}
		}

		static object CreateTaskSource (int js_id)
		{
			return new TaskCompletionSource<object> ();
		}

		static void SetTaskSourceResult (TaskCompletionSource<object> tcs, object result)
		{
			tcs.SetResult (result);
		}

		static void SetTaskSourceFailure (TaskCompletionSource<object> tcs, string reason)
		{
			tcs.SetException (new JSException (reason));
		}

		static int GetTaskAndBind (TaskCompletionSource<object> tcs, int js_id)
		{
			return BindExistingObject (tcs.Task, js_id);
		}

		static int BindExistingObject (object raw_obj, int js_id)
		{
			JSObject obj = raw_obj as JSObject;
			if (raw_obj.GetType().IsSubclassOf(typeof(Delegate))) {
				var dele = raw_obj as Delegate;
				if (obj == null && !weak_delegate_table.TryGetValue(dele, out obj)) {

					obj = new JSObject (js_id, true);
					bound_objects [js_id] = new WeakReference (obj);
					weak_delegate_table.Add(dele, obj);
					obj.WeakRawObject = new WeakReference (dele, false);
				}

			} else {
				if (obj == null && !raw_to_js.TryGetValue (raw_obj, out obj)) {
					raw_to_js [raw_obj] = obj = new JSObject (js_id, raw_obj);
				}


			}

			return (int)(IntPtr)obj.AnyRefHandle;
		}

		internal static void DumpExistingObjects ()
		{
			Console.WriteLine ($"  DumpExistingObjects:: bound_objects ");
			foreach (var bo in bound_objects) {
				Console.WriteLine ($"bound map: {bo}");
			}

			Console.WriteLine ($"  DumpExistingObjects:: raw_objects ");
			foreach (var rr in raw_to_js) {
				Console.WriteLine ($"raw map: {rr}");
			}
		}

		static int GetJSObjectId (object raw_obj)
		{
			JSObject obj = raw_obj as JSObject;

			if (obj == null && raw_obj.GetType ().IsSubclassOf (typeof (Delegate))) {
				var dele = raw_obj as Delegate;
				weak_delegate_table.TryGetValue (dele, out obj);
			}
			if (obj == null && !raw_to_js.TryGetValue (raw_obj, out obj))
				return -1;

			return obj != null ? obj.JSHandle : -1;
		}

		static object GetMonoObject (int gc_handle)
		{
			GCHandle h = (GCHandle)(IntPtr)gc_handle;
			JSObject o = (JSObject)h.Target;

			if (o != null && o.WeakRawObject != null) {
				var target = o.WeakRawObject.Target;
				if (target != null) {
					return target;
				}
			}

			if (o != null && o.RawObject != null)
				return o.RawObject;
			return o;
		}

		static object BoxInt (int i)
		{
			return i;
		}
		static object BoxDouble (double d)
		{
			return d;
		}

		static object BoxBool (int b)
		{
			return b == 0 ? false : true;
		}

		static bool IsSimpleArray (object a)
		{
			if (a is Array arr) {
				if (arr.Rank == 1 && arr.GetLowerBound (0) == 0)
					return true;
			}
			return false;
		
		}

		static object GetCoreType (string coreObj)
		{
			Assembly asm = typeof (Runtime).Assembly;
			Type type = asm.GetType (coreObj);
			return type;

		}

		[StructLayout (LayoutKind.Explicit)]
		internal struct IntPtrAndHandle {
			[FieldOffset (0)]
			internal IntPtr ptr;

			[FieldOffset (0)]
			internal RuntimeMethodHandle handle;
		}

		//FIXME this probably won't handle generics
		static string GetCallSignature (IntPtr method_handle)
		{
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
				case TypeCode.Boolean:
					// Enums types have the same code as their underlying numeric types
					if (t.IsEnum)
						res += "j";
					else
						res += "i";
					break;
				case TypeCode.Int64:
				case TypeCode.UInt64:
					// Enums types have the same code as their underlying numeric types
					if (t.IsEnum)
						res += "k";
					else
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
					if (t == typeof(IntPtr)) { 
 						res += "i";
					} else if (t == typeof(SafeHandle)) {
						res += "h";

					} else {
 						if (t.IsValueType)
 							throw new Exception("Can't handle VT arguments");
						res += "o";
					}
					break;
				}
			}

			return res;
		}

		static object ObjectToEnum (IntPtr method_handle, int parm, object obj)
		{
			IntPtrAndHandle tmp = default (IntPtrAndHandle);
			tmp.ptr = method_handle;

			var mb = MethodBase.GetMethodFromHandle (tmp.handle);
			var parmType = mb.GetParameters () [parm].ParameterType;
			if (parmType.IsEnum)
				return Runtime.EnumFromExportContract (parmType, obj);
			else
				return null;

		}


		static MethodInfo gsjsc;
		static void GenericSetupJSContinuation<T> (Task<T> task, JSObject cont_obj)
		{
			task.GetAwaiter ().OnCompleted ((Action)(() => {

				if (task.Exception != null)
					cont_obj.Invoke ((string)"reject", task.Exception.ToString ());
				else {
					cont_obj.Invoke ((string)"resolve", task.Result);
				}

				cont_obj.Dispose ();
				FreeObject (task);

			}));
		}

		static void SetupJSContinuation (Task task, JSObject cont_obj)
		{
			if (task.GetType () == typeof (Task)) {
				task.GetAwaiter ().OnCompleted ((Action)(() => {

					if (task.Exception != null)
						cont_obj.Invoke ((string)"reject", task.Exception.ToString ());
					else
						cont_obj.Invoke ((string)"resolve", (object [])null);

					cont_obj.Dispose ();
					FreeObject (task);
				}));
			} else {
				//FIXME this is horrible codegen, we can do better with per-method glue
				if (gsjsc == null)
					gsjsc = typeof (Runtime).GetMethod ("GenericSetupJSContinuation", BindingFlags.NonPublic | BindingFlags.Static);
				gsjsc.MakeGenericMethod (task.GetType ().GetGenericArguments ()).Invoke (null, new object [] { task, cont_obj });
			}
		}


		/// <summary>
		///   Fetches a global object from the Javascript world, either from the current brower window or from the node.js global context.
		/// </summary>
		/// <remarks>
		///   This method returns the value of a global object marshalled for consumption in C#.
		/// </remarks>
		/// <returns>
		///   <para>
		///     The return value can either be a primitive (string, int, double), a 
		///     <see cref="T:WebAssembly.JSObject"/> for JavaScript objects, a 
		///     <see cref="T:System.Threading.Tasks.Task"/>(object) for JavaScript promises, an array of
		///     a byte, int or double (for Javascript objects typed as ArrayBuffer) or a 
		///     <see cref="T:System.Func"/> to represent JavaScript functions.  The specific version of
		///     the Func that will be returned depends on the parameters of the Javascript function
		///     and return value.
		///   </para>
		///   <para>
		///     The value of a returned promise (The Task(object) return) can in turn be any of the above
		///     valuews.
		///   </para>
		/// </returns>
		/// <param name="str">The name of the global object, or null if you want to retrieve the 'global' object itself.
		/// On a browser, this is the 'window' object, on node.js it is the 'global' object.
		/// </param>
		public static object GetGlobalObject (string str = null)
		{
			int exception;
			var globalHandle = Runtime.GetGlobalObject (str, out exception);

			if (exception != 0)
				throw new JSException ($"Error obtaining a handle to global {str}");

			return globalHandle;
		}

		static string ObjectToString (object o)
		{

			if (o is Enum)
				return EnumToExportContract ((Enum)o).ToString ();

			return o.ToString ();
		}

		static double GetDateValue (object dtv)
		{
			if (dtv == null)
				throw new ArgumentNullException (nameof (dtv), "Value can not be null");
			if (!(dtv is DateTime)) {
				throw new InvalidCastException ($"Unable to cast object of type {dtv.GetType()} to type DateTime.");
			}
			var dt = (DateTime)dtv;
			if (dt.Kind == DateTimeKind.Local)
				dt = dt.ToUniversalTime ();
			else if (dt.Kind == DateTimeKind.Unspecified)
				dt = new DateTime (dt.Ticks, DateTimeKind.Utc);
			return new DateTimeOffset(dt).ToUnixTimeMilliseconds();
		}

		static DateTime CreateDateTime (double ticks)
		{
			var unixTime = DateTimeOffset.FromUnixTimeMilliseconds((Int64)ticks);
			return unixTime.DateTime;
		}

		static bool SafeHandleAddRef (SafeHandle safeHandle)
		{
			bool b = false;
#if DEBUG_HANDLE
			var anyref = safeHandle as AnyRef;
#endif
			try {
				safeHandle.DangerousAddRef (ref b);
#if DEBUG_HANDLE
				if (b && anyref != null)
					anyref.AddRef ();
#endif
					
			} catch {
				if (b) {
					safeHandle.DangerousRelease ();
#if DEBUG_HANDLE

					if (anyref != null)
					    anyref.Release ();
#endif
					b = false;
				}
			}
#if DEBUG_HANDLE
			Console.WriteLine($"\tSafeHandleAddRef: {safeHandle.DangerousGetHandle()} / RefCount: {((anyref == null) ? 0 : anyref.RefCount)}");
#endif
			return b;
		}

		static void SafeHandleRelease (SafeHandle safeHandle)
		{
			safeHandle.DangerousRelease ();
#if DEBUG_HANDLE
			var anyref = safeHandle as AnyRef;
			if (anyref != null) {
				anyref.Release ();
				Console.WriteLine ($"\tSafeHandleRelease: {safeHandle.DangerousGetHandle ()} / RefCount: {anyref.RefCount}");
			}
#endif
		}

		static void SafeHandleReleaseByHandle (int js_id)
		{
#if DEBUG_HANDLE
			Console.WriteLine ($"SafeHandleReleaseByHandle: {js_id}");
#endif
			lock (bound_objects) {
				if (bound_objects.TryGetValue (js_id, out var reference)) {
					if (reference.Target != null) {
						SafeHandleRelease ((AnyRef)reference.Target);
					} else {
						Console.WriteLine ($"\tSafeHandleReleaseByHandle: did not find active target {js_id} / target: {reference.Target}");
					}

				} else {
					Console.WriteLine ($"\tSafeHandleReleaseByHandle: did not find reference for {js_id}");
				}
			}

		}

		static IntPtr SafeHandleGetHandle (SafeHandle safeHandle, bool addRef)
		{
#if DEBUG_HANDLE
			Console.WriteLine ($"SafeHandleGetHandle: {safeHandle.DangerousGetHandle ()} / addRef {addRef}");
#endif
			if (addRef)
				if (SafeHandleAddRef (safeHandle))
					return safeHandle.DangerousGetHandle ();
				else
					return IntPtr.Zero;
			return safeHandle.DangerousGetHandle ();
		}

		// This is simple right now and will include FlagsAttribute later.
		public static Enum EnumFromExportContract (Type enumType, object value)
		{

			if (!enumType.IsEnum) {
				throw new ArgumentException ("Type provided must be an Enum.", nameof (enumType));
			}

			if (value is string) {

				var fields = enumType.GetFields ();
				foreach (var fi in fields) {
					// Do not process special names
					if (fi.IsSpecialName)
						continue;

					ExportAttribute [] attributes =
					    (ExportAttribute [])fi.GetCustomAttributes (typeof (ExportAttribute), false);

					var enumConversionType = ConvertEnum.Default;

					object contractName = null;

					if (attributes != null && attributes.Length > 0) {
						enumConversionType = attributes [0].EnumValue;
						if (enumConversionType != ConvertEnum.Numeric)
							contractName = attributes [0].ContractName;

					}

					if (contractName == null)
						contractName = fi.Name;

					switch (enumConversionType) {
					case ConvertEnum.ToLower:
						contractName = contractName.ToString ().ToLower ();
						break;
					case ConvertEnum.ToUpper:
						contractName = contractName.ToString ().ToUpper ();
						break;
					case ConvertEnum.Numeric:
						contractName = (int)Enum.Parse (value.GetType (), contractName.ToString ());
						break;
					default:
						contractName = contractName.ToString ();
						break;
					}

					if (contractName.ToString () == value.ToString ()) {
						return (Enum)Enum.Parse (enumType, fi.Name);
					}

				}

				throw new ArgumentException ($"Value is a name, but not one of the named constants defined for the enum of type: {enumType}.", nameof (value));
			} else {
				return (Enum)Enum.ToObject (enumType, value);
			}

		}

		// This is simple right now and will include FlagsAttribute later.
		public static object EnumToExportContract (Enum value)
		{

			FieldInfo fi = value.GetType ().GetField (value.ToString ());

			ExportAttribute [] attributes =
			    (ExportAttribute [])fi.GetCustomAttributes (typeof (ExportAttribute), false);

			var enumConversionType = ConvertEnum.Default;

			object contractName = null;

			if (attributes != null && attributes.Length > 0) {
				enumConversionType = attributes [0].EnumValue;
				if (enumConversionType != ConvertEnum.Numeric)
					contractName = attributes [0].ContractName;

			}

			if (contractName == null)
				contractName = value;

			switch (enumConversionType) {
			case ConvertEnum.ToLower:
				contractName = contractName.ToString ().ToLower ();
				break;
			case ConvertEnum.ToUpper:
				contractName = contractName.ToString ().ToUpper ();
				break;
			case ConvertEnum.Numeric:
				contractName = (int)Enum.Parse (value.GetType (), contractName.ToString ());
				break;
			default:
				contractName = contractName.ToString ();
				break;
			}

			return contractName;
		}

		//
		// Can be called by the app to stop profiling
		//
		[MethodImplAttribute (MethodImplOptions.NoInlining)]
		public static void StopProfile () {
		}

		// Called by the AOT profiler to save profile data into Module.aot_profile_data
		internal unsafe static void DumpAotProfileData (ref byte buf, int len, string s) {
			var arr = new byte [len];
			fixed (void *p = &buf) {
				var span = new ReadOnlySpan<byte> (p, len);

				// Send it to JS
				var js_dump = (JSObject)Runtime.GetGlobalObject ("Module");
				js_dump.SetObjectProperty ("aot_profile_data", WebAssembly.Core.Uint8Array.From (span));
			}
		}
	}
}
