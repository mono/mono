// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml;
using System.Xml.Serialization;


namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaSimpleTypeList.
	/// </summary>
	public class XmlSchemaSimpleTypeList : XmlSchemaSimpleTypeContent
	{
		private XmlSchemaSimpleType itemType;
		private XmlQualifiedName itemTypeName;
		private static string xmlname = "list";

		public XmlSchemaSimpleTypeList()
		{
			this.ItemTypeName = XmlQualifiedName.Empty;
		}

		[System.Xml.Serialization.XmlAttribute("itemType")]
		public XmlQualifiedName ItemTypeName
		{
			get{ return itemTypeName; } 
			set
			{
				itemTypeName = value;
			}
		}

		[XmlElement("simpleType",Namespace=XmlSchema.Namespace)]
		public XmlSchemaSimpleType ItemType 
		{
			get{ return itemType; } 
			set
			{
				itemType = value;
			}
		}
		/// <remarks>
		/// 1. One of itemType or a <simpleType> must be present, but not both.
		/// 2. id must be of type ID
		/// </remarks>
		[MonoTODO]
		internal int Compile(ValidationEventHandler h, XmlSchema schema)
		{
			// If this is already compiled this time, simply skip.
			if (this.IsComplied (schema.CompilationId))
				return 0;

			errorCount = 0;

			if(ItemType != null && !ItemTypeName.IsEmpty)
				error(h, "both itemType and simpletype can't be present");
			if(ItemType == null && ItemTypeName.IsEmpty)
				error(h, "one of itemType or simpletype must be present");
			if(ItemType != null)
			{
				errorCount += ItemType.Compile(h,schema);
			}
			if(!XmlSchemaUtil.CheckQName(ItemTypeName))
				error(h,"BaseTypeName must be a XmlQualifiedName");
			
			XmlSchemaUtil.CompileID(Id,this,schema.IDCollection,h);

			this.CompilationId = schema.CompilationId;
			return errorCount;
		}
		
		[MonoTODO]
		internal int Validate(ValidationEventHandler h)
		{
			return errorCount;
		}
		//<list 
		//  id = ID 
		//  itemType = QName 
		//  {any attributes with non-schema namespace . . .}>
		//  Content: (annotation?, (simpleType?))
		//</list>
		internal static XmlSchemaSimpleTypeList Read(XmlSchemaReader reader, ValidationEventHandler h)
		{
			XmlSchemaSimpleTypeList list = new XmlSchemaSimpleTypeList();
			reader.MoveToElement();

			if(reader.NamespaceURI != XmlSchema.Namespace || reader.LocalName != xmlname)
			{
				error(h,"Should not happen :1: XmlSchemaSimpleTypeList.Read, name="+reader.Name,null);
				reader.Skip();
				return null;
			}

			list.LineNumber = reader.LineNumber;
			list.LinePosition = reader.LinePosition;
			list.SourceUri = reader.BaseURI;

			while(reader.MoveToNextAttribute())
			{
				if(reader.Name == "id")
				{
					list.Id = reader.Value;
				}
				else if(reader.Name == "itemType")
				{
					Exception innerex;
					list.ItemTypeName = XmlSchemaUtil.ReadQNameAttribute(reader,out innerex);
					if(innerex != null)
						error(h, reader.Value + " is not a valid value for itemType attribute",innerex);
				}
				else if((reader.NamespaceURI == "" && reader.Name != "xmlns") || reader.NamespaceURI == XmlSchema.Namespace)
				{
					error(h,reader.Name + " is not a valid attribute for list",null);
				}
				else
				{
					XmlSchemaUtil.ReadUnhandledAttribute(reader,list);
				}
			}
			
			reader.MoveToElement();
			if(reader.IsEmptyElement)
				return list;
			//  Content: annotation?, simpleType?
			int level = 1;
			while(reader.ReadNextElement())
			{
				if(reader.NodeType == XmlNodeType.EndElement)
				{
					if(reader.LocalName != xmlname)
						error(h,"Should not happen :2: XmlSchemaSimpleTypeList.Read, name="+reader.Name,null);
					break;
				}
				if(level <= 1 && reader.LocalName == "annotation")
				{
					level = 2; //Only one annotation
					XmlSchemaAnnotation annotation = XmlSchemaAnnotation.Read(reader,h);
					if(annotation != null)
						list.Annotation = annotation;
					continue;
				}
				if(level <= 2 && reader.LocalName == "simpleType")
				{
					level = 3;
					XmlSchemaSimpleType stype = XmlSchemaSimpleType.Read(reader,h);
					if(stype != null)
						list.itemType = stype;
					continue;
				}
				reader.RaiseInvalidElementError();
			}
			return list;
		}
	}
}
