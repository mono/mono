//
// JSField.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

namespace Microsoft.JScript
{
	using System;
	using System.Reflection;

	public abstract class JSField : FieldInfo 
	{
		public override FieldAttributes Attributes {
			get { throw new NotImplementedException (); }
		}

		public override Type DeclaringType {
			get { throw new NotImplementedException (); }
		}

		public override RuntimeFieldHandle FieldHandle {
			get { throw new NotImplementedException (); }
		}

		public override Type FieldType {
			get { throw new NotImplementedException (); }
		}

		public override Object [] GetCustomAttributes (Type t, bool inherit)
		{
			throw new NotImplementedException ();
		}

		public override Object [] GetCustomAttributes (bool inherit)
		{
			throw new NotImplementedException ();
		}

		public override bool IsDefined (Type type, bool  inherit)
		{
			throw new NotImplementedException ();
		}

		public override MemberTypes MemberType {
			get { throw new NotImplementedException (); }
		}

		public override string Name {
			get { throw new NotImplementedException (); }
		}

		public override Type ReflectedType {
			get { throw new NotImplementedException (); }
		}
	}
}