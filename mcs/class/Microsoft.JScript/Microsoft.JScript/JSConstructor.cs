//
// JSConstructor.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

namespace Microsoft.JScript.Tmp
{
	using System;
	using System.Reflection;
	using System.Globalization;

	public sealed class JSConstructor : ConstructorInfo
	{
		public override MethodAttributes Attributes {
			get { throw new NotImplementedException (); }
		}


		public override string Name {
			get { throw new NotImplementedException (); }
		}


		public override Type DeclaringType {
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


		public override MethodImplAttributes GetMethodImplementationFlags ()
		{	
			throw new NotImplementedException ();
		}


		public override ParameterInfo [] GetParameters ()
		{
			throw new NotImplementedException ();
		}


		public override object Invoke (BindingFlags options, Binder binder,
					       object [] parameters, CultureInfo culture)
		{
			throw new NotImplementedException ();
		}


		public override object Invoke (object obj, BindingFlags options, Binder binder,
					       object [] parameters, CultureInfo culture)
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


		public override RuntimeMethodHandle MethodHandle {
			get { throw new NotImplementedException (); }
		}


		public override Type ReflectedType {
			get { throw new NotImplementedException (); }
		}
	}
}