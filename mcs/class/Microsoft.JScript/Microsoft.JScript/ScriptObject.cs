//
// ScriptObject.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
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
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using Microsoft.JScript.Vsa;

namespace Microsoft.JScript {

	public abstract class ScriptObject : IReflect {
		public object proto {
			get {
				if (_proto != null)
					return _proto;

				if (this == ObjectPrototype.Proto)
					return DBNull.Value;

				Type prototype = SemanticAnalyser.map_to_prototype (this);
				if (prototype == null)
					Console.WriteLine ("No prototype for {0}", this.GetType ());
				FieldInfo field = prototype.GetField ("Proto", BindingFlags.NonPublic | BindingFlags.Static);
				if (field != null) {
					ScriptObject result = field.GetValue (prototype) as ScriptObject;
					if (result != null)
						return result;
				}

				throw new NotImplementedException ();
			}
		}
		
		public VsaEngine engine;
		protected GlobalScope parent;
		internal Hashtable elems;
		internal Hashtable property_cache;
		internal object _proto = null;

		public FieldInfo AddField (string name)
		{
			JSFieldInfo fi = new JSFieldInfo (name);
			elems.Add (name, fi);
			return fi;
		}

		internal void AddField (object name, object value)
		{
			string str_name = Convert.ToString (name);

			if (proper_array_index (str_name)) {
				JSFieldInfo field = (JSFieldInfo) AddField (str_name);
				field.SetValue (str_name, value);
			} else
				throw new Exception ("Not a valid index array");
		}

		public FieldInfo GetField (string name, BindingFlags bindFlags)
		{
			throw new NotImplementedException ();
		}

		internal JSFieldInfo GetField (string name)
		{
			if (elems == null)
				return null;

			object res = elems [name];

			if (res is JSFieldInfo)
				return (JSFieldInfo) res;
			return null;
		}

		public virtual FieldInfo [] GetFields (BindingFlags bindFlags)
		{
			throw new NotImplementedException ();
		}

		public abstract MemberInfo [] GetMember (string name, BindingFlags bindFlags);		

		public abstract MemberInfo [] GetMembers (BindingFlags bindFlags);

		public MethodInfo GetMethod (string name, BindingFlags bindFlags)
		{
			Type prototype = SemanticAnalyser.map_to_prototype (this);
			if (prototype != null)
				return prototype.GetMethod (name, bindFlags | BindingFlags.Static);
			else
				throw new NotImplementedException ();
		}

		public MethodInfo GetMethod (string name, BindingFlags bindFlags, 
					     System.Reflection.Binder binder, Type [] types, ParameterModifier [] modifiers)
		{
			throw new NotImplementedException ();
		}

		public virtual MethodInfo[] GetMethods (BindingFlags bindFlags)
		{
			throw new NotImplementedException ();
		}

		public virtual MethodInfo GetMethods (string name, BindingFlags bindFlags)
		{
			throw new NotImplementedException ();
		}

		public ScriptObject GetParent ()
		{
			throw new NotImplementedException ();
		}

		public PropertyInfo GetProperty (string name, BindingFlags bindFlags)
		{
			throw new NotImplementedException ();
		}

		public PropertyInfo GetProperty (string name, BindingFlags bindFlags,
						 System.Reflection.Binder binder, Type returnType, Type [] types,
						 ParameterModifier [] modifiers)
		{
			throw new NotImplementedException ();
		}

		public virtual PropertyInfo [] GetProperties (BindingFlags bindFlags)
		{
			throw new NotImplementedException ();
		}

		[DebuggerStepThroughAttribute]
		[DebuggerHiddenAttribute]
		public virtual Object InvokeMember (string name,  BindingFlags invokeAttr, 
						    System.Reflection.Binder binder, Object target,
						    Object[] args, ParameterModifier [] modifiers, 
						    CultureInfo locale, string[] namedParameters)
		{
			throw new NotImplementedException ();
		}

		public Object this [double index] {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException ();}
		}

		public Object this [int index] {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public Object this [string name] {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		/*
		public Object this [params Object [] pars] {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		*/

		public virtual Type UnderlyingSystemType {
			get { throw new NotImplementedException (); }
		}

		internal bool HasMethod (string name)
		{
			Type prototype = SemanticAnalyser.map_to_prototype (this);
			MethodInfo method = prototype.GetMethod (name, BindingFlags.Public | BindingFlags.Static);
			return method != null;
		}

		internal object CallMethod (string name, params object [] args)
		{
			Type prototype = SemanticAnalyser.map_to_prototype (this);
			MethodInfo method = prototype.GetMethod (name, BindingFlags.Public | BindingFlags.Static);
			if (method == null)
				method = typeof (ObjectPrototype).GetMethod (name, BindingFlags.Public | BindingFlags.Static);
			if (method == null)
				Console.WriteLine ("CallMethod: method is null! this = {0}, prototype = {1}, name = {2}", this, prototype, name);
			return CallMethod (method, args);
		}

		internal object CallMethod (MethodInfo method, params object [] args)
		{
			return method.Invoke (null, LateBinding.assemble_args (this, method, args, engine));
		}


		internal string ClassName {
			get {
				if (this is GlobalScope)
					return "global";
				else if (this is ObjectPrototype)
					return "Object";
				else if (this is ArrayObject)
					return "Array";
				else if (this is FunctionObject)
					return "Function";
				else if (this is BooleanObject)
					return "Boolean";
				else if (this is StringObject)
					return "String";
				else if (this is NumberObject)
					return "Number";
				else if (this is DateObject)
					return "Date";
				else if (this is EvalErrorObject)
					return "EvalError";
				else if (this is RangeErrorObject)
					return "RangeError";
				else if (this is ReferenceErrorObject)
					return "ReferenceError";
				else if (this is RegExpObject)
					return "RegExp";
				else if (this is SyntaxErrorObject)
					return "SyntaxError";
				else if (this is TypeErrorObject)
					return "TypeError";
				else if (this is URIErrorObject)
					return "URIError";
				else if (this is ErrorObject)
					return "Error";
				else
					return this.GetType ().ToString ();
			}
		}

		//
		// FIXME: 
		//
		private bool proper_array_index (object name)
		{
			return true;
		}
	}
}	
