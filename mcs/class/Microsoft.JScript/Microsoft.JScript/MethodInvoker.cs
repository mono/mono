//
// MethodInvoker.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

namespace Microsoft.JScript.Tmp
{
	public abstract class MethodInvoker
	{
		public abstract object Invoke (object thisObj, object [] parameters);
	}
}