//
// GlobalScope.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

namespace Microsoft.JScript.Tmp
{
	using System.Reflection;
	using System;
	using System.Runtime.InteropServices.Expando;
	using Microsoft.JScript.Vsa;

	public class GlobalScope : ActivationObject, IExpando
	{
		public GlobalScope (GlobalScope parent, VsaEngine engine)
		{
			throw new NotImplementedException ();
		}

		public FieldInfo AddField (string name)
		{
			throw new NotImplementedException ();
		}

		PropertyInfo IExpando.AddProperty (string name)
		{
			throw new NotImplementedException ();
		}

		MethodInfo IExpando.AddMethod (string name, Delegate method)
		{
			throw new NotImplementedException ();
		}

		void IExpando.RemoveMember (MemberInfo m)
		{
			throw new NotImplementedException ();
		}

		public override Object GetDefaultThisObject ()
		{
			throw new NotImplementedException ();
		}

		public override FieldInfo GetField (string name, int lexLevel)
		{
			throw new NotImplementedException ();
		}

		public override FieldInfo [] GetFields (BindingFlags bidFlags)
		{
			throw new NotImplementedException ();
		}

		public override GlobalScope GetGlobalScope ()
		{
			throw new NotImplementedException ();
		}

		public override FieldInfo GetLocalField (string name)
		{
			throw new NotImplementedException ();
		}

		public override MemberInfo [] GetMember (string name, BindingFlags bindFlags)
		{
			throw new NotImplementedException ();
		}

		public override MemberInfo [] GetMembers (BindingFlags bindFlags)
		{
			throw new NotImplementedException ();
		}

		public override MethodInfo [] GetMethods (BindingFlags bindFlags)
		{
			throw new NotImplementedException ();
		}

		public override PropertyInfo [] GetProperties (BindingFlags bindFlags)
		{
			throw new NotImplementedException ();
		}
	}
}