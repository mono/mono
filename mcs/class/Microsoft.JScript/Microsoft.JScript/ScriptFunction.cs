//
// ScriptFunction.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
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
using System.Globalization;
using System.Collections;
using Microsoft.JScript.Vsa;
using System.Text;

namespace Microsoft.JScript {

	public abstract class ScriptFunction : JSObject {

		public string name = "";
		internal MethodInfo method;
		internal MethodAttributes attr;
		internal VsaEngine vsa_engine;
		internal Type return_type;
		internal FormalParameterList parameters;
		internal ScriptObject _prototype;
		internal int _length = -1;
		internal string source;

		[DebuggerStepThroughAttribute]
		[DebuggerHiddenAttribute]
		[JSFunctionAttribute (JSFunctionAttributeEnum.HasVarArgs)]
		public Object CreateInstance (params Object [] args)
		{
			Type t = this.GetType ();
			// Built-in constructor?
			if (t != typeof (FunctionObject)) {
				MethodInfo method = t.GetMethod ("CreateInstance");
				return method.Invoke (this, new object [] { args });
			}

			Type prototype = SemanticAnalyser.map_to_prototype (this.prototype);
			MemberInfo [] members = prototype.GetMember ("constructor");
			PropertyInfo field = members [0] as PropertyInfo;
			ScriptFunction ctr = field.GetValue (prototype, null) as ScriptFunction;
			if (ctr == null)
				Console.WriteLine ("No constructor for {0}", this);
			ScriptObject new_obj = ctr.CreateInstance (args) as ScriptObject;
			new_obj._proto = this.prototype;

			vsa_engine.PushScriptObject (new_obj);
			Invoke (new_obj, args);
			//
			// NOTE: I think the result of calling an user function as a constructor
			// should not be the return value of the user function. I think it should
			// always be the newly constructed object.
			//
			// TODO: This has yet to be verified.
			//
			return vsa_engine.PopScriptObject ();
		}

		[DebuggerStepThroughAttribute]
		[DebuggerHiddenAttribute]
		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject | JSFunctionAttributeEnum.HasVarArgs)]
		public Object Invoke (Object thisOb, params Object [] args)
		{
			if (method != null) {
				if ((attr & MethodAttributes.Static) != 0)
					return method.Invoke (null, LateBinding.assemble_args (thisOb, method, args, vsa_engine));
				else
					return method.Invoke (thisOb, LateBinding.assemble_args (null, method, args, vsa_engine));
			}

			Console.WriteLine ("Called ScriptFunction:Invoke on unknown user function");
			throw new NotImplementedException ();
		}

		[DebuggerStepThroughAttribute]
		[DebuggerHiddenAttribute]
		public override Object InvokeMember (string name, BindingFlags invokeAttr,
						     System.Reflection.Binder binder, Object target, Object [] args,
						     ParameterModifier [] modifiers, CultureInfo cultInfo,
						     string [] namedParams)
		{
			throw new NotImplementedException ();
		}

		public virtual int length {
			get {
				if (_length != -1)
					return _length;

				if (method != null)
					return LateBinding.GetRequiredArgumentCount (method);

				Console.WriteLine ("Called ScriptFunction:length on user function without known length");
				throw new NotImplementedException ();
			}
			set { throw new JScriptException (JSError.AssignmentToReadOnly); }
		}

		public int arity {
			get { return length; }
			set { length = value; }
		}

		public override string ToString ()
		{
			if (source != null)
				return source;

			StringBuilder sb = new StringBuilder ();

			sb.Append ("function");
			if (name != "") {
				sb.Append (" ");
				sb.Append (name);
			}
			sb.Append ("(");

			if (parameters != null)
				sb.Append (this.parameters.ToString ());

			sb.Append (")");
			if (return_type != null && return_type != typeof (void))
				sb.Append (" : " + return_type);
			sb.Append (" {\n");
			sb.Append ("    [native code]\n");
			sb.Append ("}");

			return sb.ToString ();
		}

		public Object prototype {
			get {
				if (_prototype == null) {
					Console.WriteLine ("No prototype for {0}", this.GetType ());
					throw new NotImplementedException ();
				} else
					return _prototype;
			}
			set {
				JSObject proto = value as JSObject;
				// TODO: Needs better checking
				if (proto != null)
					_prototype = proto;
				else {
					Console.WriteLine ("Unexpected prototype value {0} ({1})", value, value.GetType ());
					throw new NotImplementedException ();
				}
			}
		}

		public override int GetHashCode ()
		{
			return 1;
		}

		public override bool Equals (object obj)
		{
			ScriptFunction other = obj as ScriptFunction;
			if (other == null)
				return false;
			else {
				if (other.method != null && this.method != null)
					return other.method == this.method;
				else
					return this == obj;
			}
		}

		internal override object GetDefaultValue (Type hint, bool avoid_toString)
		{
			return ToString ();
		}
	}
}
