//
// ActivationObject.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

namespace Microsoft.JScript.Tmp
{
	using System;
	using System.Reflection;

	public abstract class ActivationObject : ScriptObject, IActivationObject
	{
		public virtual FieldInfo GetField (string name, int lexLevel)
		{
			throw new NotImplementedException ();
		}

		public virtual Object GetDefaultThisObject ()
		{
			throw new NotImplementedException ();
		}

		public virtual GlobalScope GetGlobalScope ()
		{
			throw new NotImplementedException ();
		}

		public virtual FieldInfo GetLocalField (string name)
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

		public Object GetMemberValue (string name, int lexLevel)
		{
			throw new NotImplementedException ();
		}
	}
}