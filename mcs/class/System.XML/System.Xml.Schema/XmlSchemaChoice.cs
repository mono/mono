// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaAll.
	/// </summary>
	public class XmlSchemaChoice : XmlSchemaGroupBase
	{
		private XmlSchemaObjectCollection items;
		private int errorCount=0;
		public XmlSchemaChoice()
		{
			items = new XmlSchemaObjectCollection();
		}

		[XmlElement("element",typeof(XmlSchemaElement),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("group",typeof(XmlSchemaGroupRef),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("choice",typeof(XmlSchemaChoice),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("sequence",typeof(XmlSchemaSequence),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("any",typeof(XmlSchemaAny),Namespace="http://www.w3.org/2001/XMLSchema")]
		public override XmlSchemaObjectCollection Items 
		{
			get{ return items; }
		}

		[MonoTODO]
		internal int Compile(ValidationEventHandler h, XmlSchemaInfo info)
		{
			foreach(XmlSchemaObject obj in Items)
			{
				if(obj is XmlSchemaElement)
				{
					errorCount += ((XmlSchemaElement)obj).Compile(h,info);
				}
				else if(obj is XmlSchemaGroupRef)
				{
					errorCount += ((XmlSchemaGroupRef)obj).Compile(h,info);
				}
				else if(obj is XmlSchemaChoice)
				{
					errorCount += ((XmlSchemaChoice)obj).Compile(h,info);
				}
				else if(obj is XmlSchemaSequence)
				{
					errorCount += ((XmlSchemaSequence)obj).Compile(h,info);
				}
				else if(obj is XmlSchemaAny)
				{
					errorCount += ((XmlSchemaAny)obj).Compile(h,info);
				}
			}
			return errorCount;
		}
		
		[MonoTODO]
		internal int Validate(ValidationEventHandler h)
		{
			return errorCount;
		}

		internal void error(ValidationEventHandler handle,string message)
		{
			errorCount++;
			ValidationHandler.RaiseValidationError(handle,this,message);
		}
	}
}
