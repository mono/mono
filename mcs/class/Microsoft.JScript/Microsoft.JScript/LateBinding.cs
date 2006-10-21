//
// LateBinding.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
// (C) 2005, Novell Inc, (http://novell.com)
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Reflection;
using System.Diagnostics;
using Microsoft.JScript.Vsa;
using System.Collections;
using System.Text.RegularExpressions;

namespace Microsoft.JScript {

	public sealed class LateBinding {

		public object obj;
		private static BindingFlags bind_flags = BindingFlags.Public;
		private string right_hand_side;

		public LateBinding (string name)
		{
			this.right_hand_side = name;
		}


		public LateBinding (string name, object obj)
		{
			this.right_hand_side = name;
			this.obj = obj;
		}

		[DebuggerStepThroughAttribute]
		[DebuggerHiddenAttribute]
		public object Call (object [] arguments, bool construct, bool brackets,
					VsaEngine engine)
		{
			object fun = GetObjectProperty (obj, right_hand_side);

			if (fun == null)
				Console.WriteLine ("Call: No function for {0} ({1}).{2}", obj, obj.GetType (), right_hand_side);
			return CallValue (obj, fun, arguments, construct, brackets, engine);
		}

		internal static string MapToInternalName (string name)
		{
			switch (name) {
			case "lastIndexOf":
				return "lastIndexOfGood";
			case "__proto__":
				return "proto";
			default:
				return name.Replace ("$", "dollar_");
			}
		}

		internal static object MapToExternalName (string name)
		{
			switch (name) {
			case "lastIndexOfGood":
				return "lastIndexOf";
			case "proto":
				return "__proto__";
			default:
				return name.Replace ("dollar_", "$");
			}
		}

		internal static void GetMethodFlags (MethodInfo method, out bool has_engine, out bool has_var_args, out bool has_this)
		{
			//
			// Very hackish. It would be better if the
			// anonymous methods could be decorated with the right attributes.
			//
			if (method.DeclaringType.IsSubclassOf (typeof (GlobalScope))) {
				has_engine = true;
				has_var_args = false;
				has_this = true;
				return;
			}

			JSFunctionAttribute [] custom_attrs = (JSFunctionAttribute [])
				method.GetCustomAttributes (typeof (JSFunctionAttribute), true);
			//
			// We need to iterate through the JSFunctionAttributes to find out whether the function wants
			// to get passed the vsaEngine or not so we can pass the right arguments to it.
			//
			has_engine = false;
			has_var_args = false;
			has_this = false;
			foreach (JSFunctionAttribute attr in custom_attrs) {
				JSFunctionAttributeEnum flags = attr.GetAttributeValue ();
				if ((flags & JSFunctionAttributeEnum.HasEngine) != 0)
					has_engine = true;
				if ((flags & JSFunctionAttributeEnum.HasVarArgs) != 0)
					has_var_args = true;
				if ((flags & JSFunctionAttributeEnum.HasThisObject) != 0)
					has_this = true;
			}
		}

		internal static int GetRequiredArgumentCount (MethodInfo method)
		{
			bool has_engine, has_var_args, has_this;
			GetMethodFlags (method, out has_engine, out has_var_args, out has_this);
			return GetRequiredArgumentCount (method.GetParameters ().Length, has_engine, has_var_args, has_this);
		}

		private static int GetRequiredArgumentCount (int argc, bool has_engine, bool has_var_args, bool has_this)
		{
			if (has_this)
				argc--;
			if (has_engine)
				argc--;
			if (has_var_args)
				argc--;
			return argc;
		}

		internal static object [] assemble_args (object obj, MethodInfo method, object [] arguments, VsaEngine engine)
		{
			bool has_engine, has_var_args, has_this;
			GetMethodFlags (method, out has_engine, out has_var_args, out has_this);

			ParameterInfo [] args = method.GetParameters ();
			int total_argc = args.Length;
			int req_argc = GetRequiredArgumentCount (total_argc, has_engine, has_var_args, has_this);
			Type [] arg_types = new Type [req_argc];
			// var args are not at the beginning of the argument list so we need to adjust the index
			int j = total_argc - req_argc - (has_var_args ? 1 : 0);

			for (int i = 0; i < req_argc; i++, j++)
				arg_types [i] = args [j].ParameterType;

			return assemble_args (obj, has_engine, has_var_args, has_this, arg_types, arguments, engine);
		}

		internal static object [] assemble_args (object obj, bool has_engine, bool has_var_args, bool has_this,
			Type [] arg_types, object [] arguments, VsaEngine engine)
		{
			ArrayList arg_list = new ArrayList (arguments);
			int req_argc = arg_types.Length;
			int missing_args = req_argc - arg_list.Count;

			// Add missing args
			for (int i = 0; i < missing_args; i++)
				arg_list.Add (null);

			// Convert types of argument to match method signature if necessary
			for (int i = 0; i < req_argc; i++) {
				Type arg_type = arg_types [i];
				object arg = arg_list [i];
				if (!arg_type.IsInstanceOfType (arg)) {
					object new_arg = null;
					if (arg_type == typeof (object)) {
						if (arg != null && arg != DBNull.Value)
							new_arg = Convert.ToObject (arg, engine);
						else
							new_arg = arg;
					} else if (arg_type == typeof (double))
						new_arg = Convert.ToNumber (arg);
					else if (arg_type == typeof (string))
						new_arg = Convert.ToString (arg);
					else {
						Console.WriteLine ("assemble_args: Can not convert to type {0}", arg_type);
						throw new NotImplementedException ();
					}

					arg_list [i] = new_arg;
				}
			}

			if (!has_var_args) {
				// Remove unneeded args
				int added_args = -missing_args;
				/*if (added_args > 0)
					Console.WriteLine ("warning JS1148: There are too many arguments. The extra arguments will be ignored");*/
				for (int i = 0; i < added_args; i++)
					arg_list.RemoveAt (arg_list.Count - 1);
			} else {
				int va_idx = req_argc;
				if (!has_this)
					va_idx--;
				int va_count = arg_list.Count - va_idx;

				object [] var_args = new object [va_count];

				int j = va_idx;
				object arg;
				for (int i = 0; i < va_count; i++, j++) {
					arg = arg_list [j];
					if (arg != null)
						var_args [i] = arg;
				}

				arg_list.RemoveRange (va_idx, va_count);
				arg_list.Add (var_args);
			}

			return build_args (obj, arg_list.ToArray (), engine, has_engine, has_this);
		}

		internal static object [] build_args (object obj, object [] arguments, VsaEngine engine,
			bool has_engine, bool has_this)
		{
			ArrayList args = new ArrayList ();
			if (has_this)
				args.Add (obj);
			if (has_engine)
				args.Add (engine);
			foreach (object o in arguments)
				args.Add (o);
			return args.ToArray ();
		}

		[DebuggerStepThroughAttribute]
		[DebuggerHiddenAttribute]
		public static object CallValue (object thisObj, object val, object [] arguments,
						bool construct, bool brackets, VsaEngine engine)
		{
			if (construct) {
				if (brackets) {
				}

				if (val is Closure)
					return ((Closure) val).func.CreateInstance (arguments);
				else if (val is FunctionObject)
					return ((FunctionObject) val).CreateInstance (arguments);
			} else if (brackets) {
				object first_arg = arguments.Length > 0 ? arguments [0] : null;
				return GetObjectProperty ((ScriptObject) Convert.ToObject (val, engine), Convert.ToString (first_arg));
			} else {
				if (val is Closure)
					return ((Closure) val).func.Invoke (thisObj, arguments);
				else if (val is FunctionObject)
					return ((FunctionObject) val).Invoke (thisObj, arguments);
				else if (val is RegExpObject) {
					object first_arg = arguments.Length > 0 ? arguments [0] : null;
					return RegExpPrototype.exec (val, first_arg);
				} else
					return null;
			}

			Console.WriteLine ("CallValue: construct = {0}, brackets = {1}, this = {2}, val = {3} ({4}), arg[0] = {5}",
				construct, brackets, thisObj.GetType (), val, val.GetType (), arguments [0]);
			throw new NotImplementedException ();
		}

		[DebuggerStepThroughAttribute]
		[DebuggerHiddenAttribute]
		public static object CallValue2 (object val, object thisObj, object [] arguments,
						 bool construct, bool brackets, VsaEngine engine)
		{
			throw new NotImplementedException ();
		}


		public bool Delete ()
		{
			// We are currently only calling LateBinding.Delete where we know that the
			// properties can't be deleted.
			return false;
		}


		public static bool DeleteMember (object obj, string name)
		{
			if (obj is Closure)
				obj = ((Closure) obj).func;

			ScriptObject js_obj = obj as ScriptObject;
			if (js_obj != null) {
				// Numeric index on Array?
				uint index;
				if (js_obj is ArrayObject && IsArrayIndex (name, out index)) {
					if (js_obj.elems.ContainsKey (index)) {
						js_obj.elems.Remove (index);
						// Note: It appears that deleting an element should never update length
						return true;
					}
				}

				if (js_obj.elems.ContainsKey (name)) {
					InvalidateCacheEntry (js_obj, name);
					js_obj.elems.Remove (name);
					return true;
				}
			}
 
			return false;
		}

		[DebuggerStepThroughAttribute]
		[DebuggerHiddenAttribute]
		public object GetNonMissingValue ()
		{
			return GetObjectProperty ((ScriptObject) obj, right_hand_side);
		}

		[DebuggerStepThroughAttribute]
		[DebuggerHiddenAttribute]
		public object GetValue2 ()
		{
			throw new NotImplementedException ();
		}

		[DebuggerStepThroughAttribute]
		[DebuggerHiddenAttribute]
		public static void SetIndexedPropertyValueStatic (object obj, object [] arguments,
								  object value)
		{
			foreach (object key in arguments)
				DirectSetObjectProperty (obj, Convert.ToString (key), value);
		}

		[DebuggerStepThroughAttribute]
		[DebuggerHiddenAttribute]
		public void SetValue (object value)
		{
			//
			// FIXME: We should look for native properties in the prototype chain before we
			// create a new property in the object itself.
			//
			DirectSetObjectProperty ((ScriptObject) obj, right_hand_side, value);
		}

		private static void DirectSetObjectProperty (object obj, string rhs, object value)
		{
			ArrayObject ary = obj as ArrayObject;
			StringObject str = obj as StringObject;
			ScriptObject js_obj = obj as ScriptObject;
			bool is_js_obj = js_obj != null;

			if (is_js_obj)
				InvalidateCacheEntry (js_obj, rhs);

			// Numeric index?
			uint index;
			if (IsArrayIndex (rhs, out index)) {
				if (ary != null) {
					Hashtable elems = ary.elems;
					bool had_value = elems.ContainsKey (index);
					elems [index] = value;
					if (!had_value)
						AdjustArrayLength (ary, index);
					return;
				} else if (str != null) {
					return;
				}
			}
			if (!TrySetNativeProperty (obj, rhs, value) && is_js_obj) {
				FieldInfo field = js_obj.GetField (rhs);
				if (field == null)
					field = js_obj.AddField (rhs);
				field.SetValue (rhs, value);
			}
		}

		private static void AdjustArrayLength (ArrayObject ary, uint index)
		{
			uint old_len = (uint) ary.length;
			if (index > 0 && index != 4294967295 && index > old_len)
				ary.length = index + 1;
		}

		private static bool TrySetNativeProperty (object obj, string key, object value)
		{
			key = MapToInternalName (key);
			// * seems to be a wilcard inside member names
			if (key.IndexOf ('*') != -1)
				return false;

			if (obj is Closure)
				obj = ((Closure) obj).func;

			Type type = (obj is GlobalScope) ? typeof (GlobalObject) : obj.GetType ();
			MemberInfo [] members = type.GetMember (key);

			if (members.Length != 0) {
				MemberInfo member = members [0];

				switch (member.MemberType) {
				case MemberTypes.Property:
					MethodInfo method = ((PropertyInfo) member).GetSetMethod ();
					if (method == null) {
						return true;
						// Strict behavior: /fast+
						//throw new JScriptException (JSError.AssignmentToReadOnly, type + ":" + key);
					}
					method.Invoke (obj, new object [] { value });
					return true;

				case MemberTypes.Method:
					break;

				default:
					Console.WriteLine ("TrySetNativeProperty: Unsupported member type {0} for {1}.{2}",
						member.MemberType, obj, key);
					break;
				}
			}

			return false;
		}

		private static object GetObjectProperty (object obj, string key)
		{
			return GetObjectProperty (obj, key, 0);
		}

		private static object GetObjectProperty (object obj, string key, int nesting)
		{
			object result;

			ScriptObject prop_obj;
			ScriptObject js_obj = obj as ScriptObject;
			bool is_js_obj = js_obj != null;
			if (is_js_obj && nesting > 0 && GetCacheEntry (out prop_obj, js_obj, key)) {
				if (prop_obj == null)
					return null;

				if (TryDirectGetObjectProperty (out result, prop_obj, key))
					return result;
			}

			if (TryDirectGetObjectProperty (out result, obj, key)) {
				if (is_js_obj && nesting > 0)
					SetCacheEntry (js_obj, key, js_obj);
				return result;
			}

			if (is_js_obj) {
				ScriptObject cur_obj = js_obj.proto as ScriptObject;
				if (cur_obj != null) {
					result = GetObjectProperty (cur_obj, key, nesting + 1);
					if (nesting > 0)
						SetCacheEntry (js_obj, key, cur_obj);
					return result;
				}

				if (nesting > 0)
					SetCacheEntry (js_obj, key, null);
			}
			return null;
		}

		internal static bool HasObjectProperty (object obj, string key)
		{
			return HasObjectProperty (obj, key, 0);
		}

		private static bool HasObjectProperty (object obj, string key, int nesting)
		{
			ScriptObject prop_obj;
			ScriptObject js_obj = obj as ScriptObject;
			bool is_js_obj = js_obj != null;

			if (is_js_obj && nesting > 0 && GetCacheEntry (out prop_obj, js_obj, key))
				return prop_obj != null;

			if (DirectHasObjectProperty (obj, key)) {
				if (is_js_obj && nesting > 0)
					SetCacheEntry (js_obj, key, js_obj);
				return true;
			}

			if (is_js_obj) {
				ScriptObject cur_obj = js_obj.proto as ScriptObject;
				if (cur_obj != null) {
					bool success = HasObjectProperty (cur_obj, key, nesting + 1);
					if (nesting > 0)
						SetCacheEntry (js_obj, key, cur_obj);
					return success;
				}

				if (nesting > 0)
					SetCacheEntry (js_obj, key, null);
			}
			return false;
		}

		internal static bool DirectHasObjectProperty (object obj, string key)
		{
			object result;
			return TryDirectGetObjectProperty (out result, obj, key);
		}

		private static bool TryDirectGetObjectProperty (out object result, object obj, string rhs)
		{
			ArrayObject ary = obj as ArrayObject;
			StringObject str = obj as StringObject;
			ScriptObject js_obj = obj as ScriptObject;

			if (js_obj != null) {
				Hashtable elems = js_obj.elems;

				// Numeric index?
				uint index;
				if (IsArrayIndex (rhs, out index)) {
					if (ary != null) {
						result = elems [index];
						return true;
					} else if (str != null) {
						string str_val = str.value;
						if (index < str_val.Length)
							result = str_val.Substring ((int) index, 1);
						else
							result = null;
						return true;
					}
				}

				if (elems.ContainsKey (rhs)) {
					object val = elems [rhs];
					JSFieldInfo field = val as JSFieldInfo;
					if (field != null)
						result = field.GetValue (rhs);
					else
						result = val;
					return true;
				}
			}

			bool success = TryGetNativeProperty (out result, obj, rhs);
			return success;
		}

		private static bool TryGetNativeProperty (out object result, object obj, string key)
		{
			// * seems to be a wilcard inside member names
			if (key.IndexOf ('*') != -1) {
				result = null;
				return false;
			}
			key = MapToInternalName (key);
			
			if (obj is Closure)
				obj = ((Closure) obj).func;

			Type type = (obj is GlobalScope) ? typeof (GlobalObject) : obj.GetType ();
			MemberInfo [] members = type.GetMember (key,
				BindingFlags.FlattenHierarchy | BindingFlags.GetField | BindingFlags.GetProperty |
				BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);

			if (members.Length != 0) {
				MemberInfo member = members [0];

				switch (member.MemberType) {
				case MemberTypes.Field:
					result = ((FieldInfo) member).GetValue (obj);
					return true;

				case MemberTypes.Property:
					MethodInfo method = ((PropertyInfo) member).GetGetMethod ();
					result = method.Invoke (obj, new object [] { });
					return true;

				case MemberTypes.Method:
					result = new FunctionObject ((MethodInfo) member);
					return true;

				default:
					Console.WriteLine ("TryGetNativeProperty: Unsupported member type {0} for {1}.{2}",
						member.MemberType, obj, key);
					break;
				}
			}

			result = null;
			return false;
		}

		#region Cache Helpers
		static private bool USE_CACHE = true;

		private static bool GetCacheEntry (out ScriptObject prop_obj, ScriptObject obj, string key)
		{
			if (USE_CACHE) {
				//Console.WriteLine ("Getting cache for {0} ({1}) property {2}", obj, obj.GetType (), key);
				Hashtable property_cache = obj.property_cache;
				if (property_cache.ContainsKey (key)) {
					//Console.WriteLine ("\t=> in cache: {0} ({1})", property_cache [cache_key],
					//	property_cache [cache_key].GetType ());
					prop_obj = property_cache [key] as ScriptObject;
					return true;
				}
			}
			//Console.WriteLine ("\t=> not in cache");
			prop_obj = null;
			return false;
		}

		private static void SetCacheEntry (ScriptObject obj, string key, ScriptObject prop_obj)
		{
			if (!USE_CACHE)
				return;

			ArrayObject ary = obj as ArrayObject;
			// Ignore numeric indices on arrays
			if (ary != null && IsArrayIndex (key))
				return;

			//Console.WriteLine ("Setting cache for {0} ({1}) property {2} to {3} ({4})", obj, obj.GetType (),
			//	key, prop_obj, prop_obj.GetType ());

			Hashtable property_cache = obj.property_cache;
			property_cache.Remove (key);
			property_cache.Add (key, prop_obj);
		}

		private static void InvalidateCacheEntry (ScriptObject obj, string key)
		{
			if (!USE_CACHE)
				return;

			//Console.WriteLine ("Invalidating cache for {0} ({1}) property {2}", obj, obj.GetType (), key);

			obj.property_cache.Remove (key);
		}
		#endregion

		private static readonly Regex NonDigitRegex = new Regex (@"\D",
			RegexOptions.Compiled | RegexOptions.ECMAScript);

		internal static bool IsArrayIndex (string key, out uint index)
		{
			if (key == "" || NonDigitRegex.IsMatch (key)) {
				index = 0;
				return false;
			}
			index = Convert.ToUint32 (key);
			return Convert.ToString (index) == key;
		}

		internal static bool IsArrayIndex (string key)
		{
			if (key == "" || NonDigitRegex.IsMatch (key))
				return false;
			uint index = Convert.ToUint32 (key);
			return Convert.ToString (index) == key;
		}
	}
}
