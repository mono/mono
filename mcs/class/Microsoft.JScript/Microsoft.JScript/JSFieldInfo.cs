//
// JSFieldInfo.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

namespace Microsoft.JScript
{
	using System;
	using System.Reflection;
	using System.Globalization;

	public sealed class JSFieldInfo : FieldInfo
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


		public override object [] GetCustomAttributes (Type t, bool inherit)
		{
			throw new NotImplementedException ();
		}

	
		public override object [] GetCustomAttributes (bool inherit)
		{
			throw new NotImplementedException ();
		}


		public override object GetValue (object obj)
		{
			throw new NotImplementedException ();
		}


		public override bool IsDefined (Type type, bool inherit)
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


		public new void SetValue (object obj, object value)
		{
			throw new NotImplementedException ();
		}


		public override void SetValue (object obj, object value, BindingFlags invokeAttr,
					       Binder binder, CultureInfo culture)
		{
			throw new NotImplementedException ();
		}
	}
}