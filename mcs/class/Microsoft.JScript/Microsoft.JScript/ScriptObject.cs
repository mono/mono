//
// ScriptObject.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

namespace Microsoft.JScript
{
	using System;
	using System.Globalization;
	using System.Reflection;

	public abstract class ScriptObject : IReflect
	{
		public VsaEngine engine;

		public FieldInfo GetField (string name, BindingFlags bindFlags)
		{
			throw new NotImplementedException ();
		}

		public virtual FieldInfo [] GetFields (BindingFlags bindFlags)
		{
			throw new NotImplementedException ();
		}

		public abstract MemberInfo [] GetMember (string name, BindingFlags bindFlags);		

		public abstract MemberInfo [] GetMembers (BindingFlags bindFlags);

		public MethodInfo GetMethod (string name, BindingFlags bindFlags)
		{
			throw new NotImplementedException ();
		}

		public MethodInfo GetMethod (string name, BindingFlags bindFlags, 
					     Binder binder, Type [] types, ParameterModifier [] modifiers)
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
						 Binder binder, Type returnType, Type [] types,
						 ParameterModifier [] modifiers)
		{
			throw new NotImplementedException ();
		}

		public virtual PropertyInfo [] GetProperties (BindingFlags bindFlags)
		{
			throw new NotImplementedException ();
		}

		public virtual Object InvokeMember (string name,  BindingFlags invokeAttr, 
						    Binder binder, Object target,
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
	}
}	
