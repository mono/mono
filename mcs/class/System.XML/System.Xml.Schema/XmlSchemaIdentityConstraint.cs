// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaIdentityConstraint.
	/// </summary>
	public class XmlSchemaIdentityConstraint : XmlSchemaAnnotated
	{
		private XmlSchemaObjectCollection fields;
		private string name;
		private XmlQualifiedName qName;
		private XmlSchemaXPath selector;

		public XmlSchemaIdentityConstraint()
		{
			fields = new XmlSchemaObjectCollection();
			qName = XmlQualifiedName.Empty;
		}

		[XmlElement("field",typeof(XmlSchemaXPath),Namespace="http://www.w3.org/2001/XMLSchema")]
		public XmlSchemaObjectCollection Fields 
		{
			get{ return fields; }
		}

		[XmlElement("selector",typeof(XmlSchemaXPath),Namespace="http://www.w3.org/2001/XMLSchema")]
		public XmlSchemaXPath Selector 
		{
			get{ return  selector; } 
			set{ selector = value; }
		}
		
		[System.Xml.Serialization.XmlAttribute("name")]
		public string Name 
		{
			get{ return  name; } 
			set{ name = value; }
		}
		
		[XmlIgnore]
		public XmlQualifiedName QualifiedName 
		{
			get{ return  qName; }
		}
		/// <remarks>
		/// 1. name must be present
		/// 2. selector and field must be present
		/// </remarks>
		[MonoTODO]
		internal int Compile(ValidationEventHandler h, XmlSchemaInfo info)
		{
			if(Name == null)
				error(h,"Required attribute name must be present");
			else if(!XmlSchemaUtil.CheckNCName(this.name)) 
				error(h,"attribute name must be NCName");
			else
				this.qName = new XmlQualifiedName(Name,info.TargetNamespace);

			//TODO: Compile Xpath. 
			if(Selector == null)
				error(h,"selector must be present");
			else
			{
				errorCount += Selector.Compile(h,info);
			}

			if(Fields.Count == 0)
				error(h,"atleast one field value must be present");
			else
			{
				foreach(XmlSchemaObject obj in Fields)
				{
					if(obj is XmlSchemaXPath)
					{
						XmlSchemaXPath field = (XmlSchemaXPath)obj;
						errorCount += field.Compile(h,info);
					}
					else
						error(h,"Object of type "+obj.GetType()+" is invalid in the Fields Collection");
				}
			}
			XmlSchemaUtil.CompileID(Id,this,info.IDCollection,h);

			return errorCount;
		}
	}
}
