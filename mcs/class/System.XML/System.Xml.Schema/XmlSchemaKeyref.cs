// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaKeyref.
	/// </summary>
	public class XmlSchemaKeyref : XmlSchemaIdentityConstraint
	{
		private XmlQualifiedName refer;
		private int errorCount;

		public XmlSchemaKeyref()
		{
			refer = XmlQualifiedName.Empty;
		}

		[System.Xml.Serialization.XmlAttribute("refer")]
		public XmlQualifiedName Refer
		{
			get{ return  refer; } 
			set{ refer = value; }
		}
		/// <remarks>
		/// 1. name must be present
		/// 2. selector and field must be present
		/// 3. refer must be present
		/// </remarks>
		[MonoTODO]
		internal new int Compile(ValidationEventHandler h, XmlSchemaInfo info)
		{
			errorCount = base.Compile(h,info);
			if(refer == null || refer.IsEmpty)
				error(h,"refer must be present");
			return errorCount;
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
