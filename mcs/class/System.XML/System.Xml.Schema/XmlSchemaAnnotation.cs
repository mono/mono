// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaAnnotation.
	/// </summary>
	public class XmlSchemaAnnotation : XmlSchemaObject
	{
		private string id;
		private XmlSchemaObjectCollection items;
		private XmlAttribute[] unhandledAttributes;
		private static string xmlname = "annotation";
		public XmlSchemaAnnotation()
		{
			items = new XmlSchemaObjectCollection();
		}

		[System.Xml.Serialization.XmlAttribute("id")]
		public string Id 
		{
			get{ return  id; } 
			set{ id = value; }
		}
		
		[XmlElement("appinfo",typeof(XmlSchemaAppInfo),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("documentation",typeof(XmlSchemaDocumentation),Namespace="http://www.w3.org/2001/XMLSchema")]
		public XmlSchemaObjectCollection Items
		{
			get{ return items; }
		}
		
		[XmlAnyAttribute]
		public XmlAttribute[] UnhandledAttributes 
		{
			get{ return  unhandledAttributes; } 
			set{ unhandledAttributes = value; }
		}

		[MonoTODO]
		internal int Compile(ValidationEventHandler h, XmlSchemaInfo info)
		{
			return 0;
		}
		
		[MonoTODO]
		internal int Validate(ValidationEventHandler h)
		{
			return 0;
		}

		//<annotation
		//  id = ID
		//  {any attributes with non-schema namespace . . .}>
		//  Content: (appinfo | documentation)*
		//</annotation>
		internal static XmlSchemaAnnotation Read(XmlSchemaReader reader, ValidationEventHandler h)
		{
			XmlSchemaAnnotation annotation = new XmlSchemaAnnotation();
			reader.MoveToElement();

			if(reader.NamespaceURI != XmlSchema.Namespace || reader.LocalName != xmlname)
			{
				error(h,"Should not happen :1: XmlSchemaAnnotation.Read, name="+reader.Name,null);
				reader.SkipToEnd();
				return null;
			}

			annotation.LineNumber = reader.LineNumber;
			annotation.LinePosition = reader.LinePosition;
			annotation.SourceUri = reader.BaseURI;

			//Read Attributes
			while(reader.MoveToNextAttribute())
			{
				if(reader.Name == "id")
				{
					annotation.Id = reader.Value;
				}
				else if(reader.NamespaceURI == "" || reader.NamespaceURI == XmlSchema.Namespace)
				{
					error(h,reader.Name + " is not a valid attribute for annotation",null);
				}
				else
				{
											if(reader.Prefix == "xmlns")
												annotation.Namespaces.Add(reader.LocalName, reader.Value);
											else if(reader.Name == "xmlns")
												annotation.Namespaces.Add("",reader.Value);
						//TODO: Add to Unhandled attributes
				}
			}
			
			reader.MoveToElement();
			if(reader.IsEmptyElement)
				return annotation;

			//Content: (appinfo | documentation)*
			while(reader.ReadNextElement())
			{
				if(reader.NodeType == XmlNodeType.EndElement)
				{
					if(reader.LocalName != xmlname)
						error(h,"Should not happen :2: XmlSchemaAnnotation.Read, name="+reader.Name,null);
					break;
				}
				if(reader.LocalName == "appinfo")
				{
					XmlSchemaAppInfo appinfo = XmlSchemaAppInfo.Read(reader,h);
					if(appinfo != null)
						annotation.items.Add(appinfo);
					continue;
				}
				if(reader.LocalName == "documentation")
				{
					XmlSchemaDocumentation documentation = XmlSchemaDocumentation.Read(reader,h);
					if(documentation != null)
						annotation.items.Add(documentation);
					continue;
				}
				reader.RaiseInvalidElementError();
			}
			return annotation;
		}
	}
}
