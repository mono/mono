//
// IActivationObject.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

namespace Microsoft.JScript.Tmp
{
	using System;
	using System.Reflection;

	public interface IActivationObject
	{
		Object GetDefaultThisObject ();
		GlobalScope GetGlobalScope ();
		FieldInfo GetLocalField (string name);
		Object GetMemberValue (string name, int lexLevel);
		FieldInfo GetField (string name, int lexLevel);
	}
}