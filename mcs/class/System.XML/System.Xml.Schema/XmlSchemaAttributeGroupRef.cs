// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaAttributeGroupRef.
	/// </summary>
	public class XmlSchemaAttributeGroupRef : XmlSchemaAnnotated
	{
		private XmlQualifiedName refName;
		private int errorCount;

		public XmlSchemaAttributeGroupRef()
		{
			refName = XmlQualifiedName.Empty;
		}
		
		[System.Xml.Serialization.XmlAttribute("ref")]
		public XmlQualifiedName RefName 
		{
			get{ return  refName; }
			set{ refName = value; }
		}

		/// <remarks>
		/// 1. ref must be present
		/// 2. The element must be empty. FIXME: does it mean no id attribute too??
		/// </remarks>
		[MonoTODO]
		internal int Compile(ValidationEventHandler h, XmlSchemaInfo info)
		{
			errorCount = 0;
			if(this.refName == null || this.refName.IsEmpty)
				error(h, "ref must be present");
			if(this.Id != null && !XmlSchemaUtil.CheckID(Id))
				error(h, "id must be a valid ID");
			if(this.Annotation != null)
				error(h, "attributegroup with a ref can't have any content");
			
			return errorCount;
		}
		
		[MonoTODO]
		internal int Validate(ValidationEventHandler h)
		{
			return errorCount;
		}
				
		internal void error(ValidationEventHandler handle,string message)
		{
			this.errorCount++;
			ValidationHandler.RaiseValidationError(handle,this,message);
		}
	}
}
