//
// JSLocalField.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

using System;
using System.Reflection;
using System.Globalization;

namespace Microsoft.JScript {

	public class JSLocalField : JSVariableField {

		public JSLocalField (string name, RuntimeTypeHandle handle, int number)
		{
		}

		public override Type FieldType {
			get { throw new NotImplementedException (); }
		}

		public override Object GetValue (Object obj)
		{
			throw new NotImplementedException ();
		}

		public override void SetValue (Object obj, Object value, BindingFlags invokeAttr,
				       Binder binder, CultureInfo locale)
		{
			throw new NotImplementedException ();
		}
	}
}
					      
