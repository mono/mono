//
// ScriptFunction.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

namespace Microsoft.JScript.Tmp
{
	using System;
	using System.Reflection;
	using System.Globalization;

	public abstract class ScriptFunction : JSObject
	{
		[JSFunctionAttribute (JSFunctionAttributeEnum.HasVarArgs)]
		public Object CreateInstance  (params Object [] args)
		{
			throw new NotImplementedException ();
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject | JSFunctionAttributeEnum.HasVarArgs)]
		public Object Invoke (Object thisOb, params Object [] args)
		{
			throw new NotImplementedException ();
		}

		public override Object InvokeMember (string name, BindingFlags invokeAttr,
						     Binder binder, Object target, Object [] args,
						     ParameterModifier [] modifiers, CultureInfo cultInfo,
						     string [] namedParams)
		{
			throw new NotImplementedException ();
		}

		public virtual int length {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
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