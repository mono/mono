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

namespace Microsoft.JScript {

	public abstract class ScriptFunction : JSObject {

		internal MethodInfo method;
		internal MethodAttributes attr;

		[DebuggerStepThroughAttribute]
		[DebuggerHiddenAttribute]
		[JSFunctionAttribute (JSFunctionAttributeEnum.HasVarArgs)]
		public Object CreateInstance  (params Object [] args)
		{
			throw new NotImplementedException ();
		}

		[DebuggerStepThroughAttribute]
		[DebuggerHiddenAttribute]
		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject | JSFunctionAttributeEnum.HasVarArgs)]
		public Object Invoke (Object thisOb, params Object [] args)
		{
			if (method != null) {
				if ((attr & MethodAttributes.Static) != 0)
					return method.Invoke (null, LateBinding.assemble_args (thisOb, method, args, null));
				else
					return method.Invoke (thisOb, LateBinding.assemble_args (null, method, args, null));
			}

			Console.WriteLine ("Called ScriptFunction:Invoke on user function");
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
				if (method != null)
					return LateBinding.GetRequiredArgumentCount (method);

				Console.WriteLine ("Called ScriptFunction:length on user function");
				throw new NotImplementedException ();
			}
			set { throw new JScriptException (JSError.AssignmentToReadOnly); }
		}

		public override string ToString ()
		{
			throw new NotImplementedException ();
		}

		public Object prototype {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
	}
}
