// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Collections;
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
		
		[XmlElement("appinfo",typeof(XmlSchemaAppInfo),Namespace=XmlSchema.Namespace)]
		[XmlElement("documentation",typeof(XmlSchemaDocumentation),Namespace=XmlSchema.Namespace)]
		public XmlSchemaObjectCollection Items
		{
			get{ return items; }
		}
		
		[XmlAnyAttribute]
		public XmlAttribute[] UnhandledAttributes 
		{
			get
			{
				if(unhandledAttributeList != null)
				{
					unhandledAttributes = (XmlAttribute[]) unhandledAttributeList.ToArray(typeof(XmlAttribute));
					unhandledAttributeList = null;
				}
				return unhandledAttributes;
			}
			set
			{ 
				unhandledAttributes = value; 
				unhandledAttributeList = null;
			}
		}

		[MonoTODO]
		internal int Compile(ValidationEventHandler h, XmlSchema schema)
		{
			// If this is already compiled this time, simply skip.
			if (this.IsComplied (schema.CompilationId))
				return 0;

			this.CompilationId = schema.CompilationId;
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
				else if((reader.NamespaceURI == "" && reader.Name != "xmlns") || reader.NamespaceURI == XmlSchema.Namespace)
				{
					error(h,reader.Name + " is not a valid attribute for annotation",null);
				}
				else
				{
					XmlSchemaUtil.ReadUnhandledAttribute(reader,annotation);
				}
			}
			
			reader.MoveToElement();
			if(reader.IsEmptyElement)
				return annotation;

			//Content: (appinfo | documentation)*
			bool skip = false;
			string expectedEnd = null;
			while(!reader.EOF)
			{
				if(skip) 
					skip=false;
				else 
					reader.ReadNextElement();

				if(reader.NodeType == XmlNodeType.EndElement)
				{
					bool end = true;
					string expected = xmlname;
					if(expectedEnd != null)
					{
						expected = expectedEnd;
						expectedEnd = null;
						end = false;
					}
					if(reader.LocalName != expected)
						error(h,"Should not happen :2: XmlSchemaAnnotation.Read, name="+reader.Name+",expected="+expected,null);
					if (end)
						break;
					else
						continue;
				}
				if(reader.LocalName == "appinfo")
				{
					XmlSchemaAppInfo appinfo = XmlSchemaAppInfo.Read(reader,h,out skip);
					if(appinfo != null)
						annotation.items.Add(appinfo);
					continue;
				}
				if(reader.LocalName == "documentation")
				{
					XmlSchemaDocumentation documentation = XmlSchemaDocumentation.Read(reader,h, out skip);
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
