//
// FieldAccessor.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

namespace Microsoft.JScript
{
	public abstract class FieldAccessor
	{
		public abstract object GetValue (object thisObj);


		public abstract void SetValue (object thisObj, object value);
	}
}
		