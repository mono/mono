//
// JSVariableField.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

namespace Microsoft.JScript.Tmp
{
	using System.Reflection;
	using System;

	public abstract class JSVariableField : JSField
	{
		public override FieldAttributes Attributes {
			get { throw new NotImplementedException (); }
		}

		public override Type DeclaringType {
			get { throw new NotImplementedException (); }
		}

		public override Type FieldType {
			get { throw new NotImplementedException (); }
		}

		public override Object [] GetCustomAttributes (bool inherit)
		{
			throw new NotImplementedException ();
		}

		public override string Name {
			get { throw new NotImplementedException (); }
		}
	}
}