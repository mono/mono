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
			throw new NotImplementedException ();
		}


		[DebuggerStepThroughAttribute]
		[DebuggerHiddenAttribute]
		public object Call (object [] arguments, bool construct, bool brackets,
					VsaEngine engine)
		{
			if (construct) {
				if (brackets) {
				} else {
				}
			} else {
				if (brackets) {
				} else {
					Type type = null;

					if (obj is JSObject)
						type = SemanticAnalyser.map_to_prototype ((JSObject) obj);

					MethodInfo method = type.GetMethod (right_hand_side, BindingFlags.Public | BindingFlags.Static);
					JSFunctionAttribute [] custom_attrs = (JSFunctionAttribute [])
						method.GetCustomAttributes (typeof (JSFunctionAttribute), true);
					//
					// We need to iterate through the JSFunctionAttributes to find out whether the function wants
					// to get passed the vsaEngine or not so we can pass the right arguments to it.
					//
					object [] args = null;
					bool has_engine = false;
					bool has_var_args = false;
					foreach (JSFunctionAttribute attr in custom_attrs) {
						JSFunctionAttributeEnum flags = attr.GetAttributeValue ();
						if ((flags & JSFunctionAttributeEnum.HasEngine) != 0)
							has_engine = true;
						if ((flags & JSFunctionAttributeEnum.HasVarArgs) != 0)
							has_var_args = true;
					}

					if (has_var_args) {
						// 1 for length to last idx + 1 for thisObj = 2
						int va_idx = method.GetParameters ().Length - 2;
						if (has_engine)
							va_idx--;
						int va_count = arguments.Length - va_idx;

						ArrayList arg_list = new ArrayList (arguments);
						object [] var_args = arg_list.GetRange (va_idx, va_count).ToArray ();
						arg_list.RemoveRange (va_idx, va_count);
						arg_list.Add (var_args);
						arguments = arg_list.ToArray ();
					}

					if (has_engine)
						args = build_args (arguments, engine);
					else
						args = build_args (arguments, null);

					// TODO: Debug logging should be removed
					string arg_str = "";
					foreach (object arg in args)
						arg_str += arg.GetType ().ToString () + ", ";

					//System.Console.WriteLine("\nInvoking {0}.{1} with args {2}", obj.GetType(), method.Name, arg_str);

					return method.Invoke (type, args);
				}
			}
			throw new NotImplementedException ();
		}

		private object [] build_args (object [] arguments, VsaEngine engine)
		{
			ArrayList args = new ArrayList ();
			if (obj != null)
				args.Add (obj);
			if (engine != null)
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
					throw new NotImplementedException ();
				}
				throw new NotImplementedException ();
			} else if (brackets) {
				if (!(val is JSObject))
					throw new Exception ("val has to be a JSObject");

				JSObject js_val = (JSObject) val;
				object res = js_val.GetField (Convert.ToString (arguments [0]));
				if (res is JSFieldInfo)
					return ((JSFieldInfo) res).GetValue (arguments [0]);
				else
					throw new NotImplementedException ();
			} else {
				Console.WriteLine ("CallValue -- no brackes, no construct, this = {0}, val = {1}", thisObj.GetType(), val);
				throw new NotImplementedException ();
			}
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
			throw new NotImplementedException ();
		}


		public static bool DeleteMember (object obj, string name)
		{
			throw new NotImplementedException ();
		}

		[DebuggerStepThroughAttribute]
		[DebuggerHiddenAttribute]
		public object GetNonMissingValue ()
		{
			Type type = obj.GetType ();
			if (obj is JSObject)
				type = SemanticAnalyser.map_to_prototype ((JSObject) obj);

			MemberInfo [] members = type.GetMember (right_hand_side);
			if (members.Length > 0) {
				MemberInfo member = members [0];
				MemberTypes member_type = member.MemberType;

				switch (member_type) {
				case MemberTypes.Property:
					MethodInfo method = ((PropertyInfo) member).GetGetMethod ();
					return method.Invoke (obj, new object [] { });
				case MemberTypes.Method:
					return new FunctionObject (((MethodInfo) member).Name);
				default:
					System.Console.WriteLine ("GetNonMissingValue: type = {0}, member_type = {1}", type, member_type);
					break;
				}
			}
			throw new NotImplementedException ();
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
			if (!(obj is JSObject))
				throw new Exception ("obj should be a JSObject");

			JSObject js_obj = (JSObject) obj;
			foreach (object o in arguments)
				js_obj.AddField (o, value);
		}

		[DebuggerStepThroughAttribute]
		[DebuggerHiddenAttribute]
		public void SetValue (object value)
		{
			throw new NotImplementedException ();
		}
	}
}
