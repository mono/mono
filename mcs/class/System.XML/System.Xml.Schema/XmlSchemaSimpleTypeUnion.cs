// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaSimpleTypeUnion.
	/// </summary>
	public class XmlSchemaSimpleTypeUnion : XmlSchemaSimpleTypeContent
	{
		private XmlSchemaObjectCollection baseTypes;
		private XmlQualifiedName[] memberTypes;
		private bool errorOccured;

		public XmlSchemaSimpleTypeUnion()
		{
			baseTypes = new XmlSchemaObjectCollection();
		}

		[XmlElement("simpleType",typeof(XmlSchemaSimpleType),Namespace="http://www.w3.org/2001/XMLSchema")]
		public XmlSchemaObjectCollection BaseTypes 
		{
			get{ return baseTypes; }
		}

		[System.Xml.Serialization.XmlAttribute("memberTypes")]
		public XmlQualifiedName[] MemberTypes
		{
			get{ return  memberTypes; } 
			set{ memberTypes = value; }
		}
		/// <remarks>
		/// 1. Circular union type definition is disallowed. (WTH is this?)
		/// 2. id must be a valid ID
		/// </remarks>
		[MonoTODO]
		internal bool Compile(ValidationEventHandler h, XmlSchemaInfo info)
		{
			if(BaseTypes.Count + MemberTypes.Length == 0)
				error(h, "Atleast one simpletype or membertype must be present");

			foreach(XmlSchemaObject obj in baseTypes)
			{
				if(obj != null && obj is XmlSchemaSimpleType)
				{
					XmlSchemaSimpleType stype = (XmlSchemaSimpleType) obj;
					stype.Compile(h,info);
				}
				else
				{
					error(h, "baseTypes can't have objects other than a simpletype");
				}
			}
			for(int i=0; i< memberTypes.Length; i++)
			{
				if(memberTypes[i] == null)
				{
					warn(h,"memberTypes should not have a null value");
					memberTypes[i] = XmlQualifiedName.Empty;
				}
			}
			if(!XmlSchemaUtil.CheckID(this.Id))
				error(h,"id must be a valid ID");

			return !errorOccured;
		}
		
		[MonoTODO]
		internal bool Validate(ValidationEventHandler h)
		{
			return false;
		}
		
		internal void error(ValidationEventHandler handle,string message)
		{
			this.errorOccured = true;
			ValidationHandler.RaiseValidationError(handle,this,message);
		}

		internal void warn(ValidationEventHandler handle,string message)
		{
			this.errorOccured = true;
			ValidationHandler.RaiseValidationWarning(handle,this,message);
		}
	}
}
