//
// TypeReflector.cs:
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

	public sealed class TypeReflector : ScriptObject
	{
		public override MemberInfo [] GetMember (string name, BindingFlags bindAttr)
		{
			throw new NotImplementedException ();
		}


		public override MemberInfo [] GetMembers (BindingFlags bindAttr)
		{
			throw new NotImplementedException ();
		}
	}
}