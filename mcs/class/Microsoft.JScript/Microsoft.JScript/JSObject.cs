//
// JSObject.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

namespace Microsoft.JScript
{
	using System;
	using System.Reflection;
	using System.Collections;
	using System.Runtime.InteropServices.Expando;

	public class JSObject : ScriptObject, IEnumerable, IExpando
	{
		public JSObject ()
		{
			throw new NotImplementedException ();
		}

		public FieldInfo AddField (string name)
		{
			throw new NotImplementedException ();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException ();
		}

		PropertyInfo IExpando.AddProperty (string name)
		{
			throw new NotImplementedException ();
		}
		
		MethodInfo IExpando.AddMethod (String name, Delegate method)
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

		public void SetMemberValue2 (string name, Object value)
		{
			throw new NotImplementedException ();
		}

		void IExpando.RemoveMember (MemberInfo m)
		{
			throw new NotImplementedException ();
		}

		public override string ToString ()
		{
			throw new NotImplementedException ();
		}
	}
}