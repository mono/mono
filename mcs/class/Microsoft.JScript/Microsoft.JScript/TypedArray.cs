//
// TypedArray.cs:
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

	public sealed class TypedArray : IReflect
	{
		public TypedArray (IReflect elementType, int rank)
		{
			throw new NotImplementedException ();
		}


		public override bool Equals (object obj)
		{
			throw new NotImplementedException ();
		}


		public FieldInfo GetField (string name, BindingFlags bindAttr)
		{
			throw new NotImplementedException ();
		}


		public FieldInfo [] GetFields (BindingFlags bindAttr)
		{
			throw new NotImplementedException ();
		}


		public override int GetHashCode ()
		{
			throw new NotImplementedException ();
		}


		public MemberInfo [] GetMember (string name, BindingFlags bindAttr)
		{
			throw new NotImplementedException ();
		}


		public MemberInfo [] GetMembers (BindingFlags bindAttr)
		{
			throw new NotImplementedException ();
		}


		public MethodInfo GetMethod (string name, BindingFlags bindAttr)
		{
			throw new NotImplementedException ();
		}


		public MethodInfo GetMethod (string name, BindingFlags bindAttr, Binder binder,
					     Type [] types, ParameterModifier [] modifiers)
		{
			throw new NotImplementedException ();
		}


		public MethodInfo [] GetMethods (BindingFlags bindAttr)
		{
			throw new NotImplementedException ();
		}


		public PropertyInfo GetProperty (string name, BindingFlags bindAttr)
		{
			throw new NotImplementedException ();
		}


		public PropertyInfo GetProperty (string name, BindingFlags bindAttr, Binder binder,
						 Type returnType, Type [] types, 
						 ParameterModifier [] modifiers)
		{
			throw new NotImplementedException ();
		}


		public PropertyInfo [] GetProperties (BindingFlags bindAttr)
		{
			throw new NotImplementedException ();
		}


		public object InvokeMember (string name, BindingFlags flags, Binder binder,
					    object target, object [] args, 
					    ParameterModifier [] modifiers, CultureInfo locale,
					    string [] namedParameters)
		{
			throw new NotImplementedException ();
		}


		public override string ToString ()
		{
			throw new NotImplementedException ();
		}


		public Type UnderlyingSystemType {
			get { throw new NotImplementedException (); }
		}
	}
}