// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaKey.
	/// </summary>
	public class XmlSchemaKey : XmlSchemaIdentityConstraint
	{
		private int errorCount;

		public XmlSchemaKey()
		{
		}
		/// <remarks>
		/// 1. name must be present
		/// 2. selector and field must be present
		/// </remarks>
		[MonoTODO]
		internal new int Compile(ValidationEventHandler h, XmlSchemaInfo info)
		{
			return base.Compile(h,info);
		}
		
		[MonoTODO]
		internal int Validate(ValidationEventHandler h)
		{
			return errorCount;
		}

		internal new void error(ValidationEventHandler handle, string message)
		{
			errorCount++;
			ValidationHandler.RaiseValidationError(handle, this, message);
		}
	}
}
