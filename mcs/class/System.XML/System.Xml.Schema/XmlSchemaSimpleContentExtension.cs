// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaSimpleContentExtension.
	/// </summary>
	public class XmlSchemaSimpleContentExtension : XmlSchemaContent
	{

		private XmlSchemaAnyAttribute any;
		private XmlSchemaObjectCollection attributes;
		private XmlQualifiedName baseTypeName;
		private static string xmlname = "extension";

		public XmlSchemaSimpleContentExtension()
		{
			baseTypeName = XmlQualifiedName.Empty;
			attributes	 = new XmlSchemaObjectCollection();
		}

		[System.Xml.Serialization.XmlAttribute("base")]
		public XmlQualifiedName BaseTypeName 
		{
			get{ return  baseTypeName; }
			set{ baseTypeName = value; }
		}

		[XmlElement("attribute",typeof(XmlSchemaAttribute),Namespace=XmlSchema.Namespace)]
		[XmlElement("attributeGroup",typeof(XmlSchemaAttributeGroupRef),Namespace=XmlSchema.Namespace)]
		public XmlSchemaObjectCollection Attributes 
		{
			get{ return attributes; }
		}

		[XmlElement("anyAttribute",Namespace=XmlSchema.Namespace)]
		public XmlSchemaAnyAttribute AnyAttribute 
		{
			get{ return  any; }
			set{ any = value; }
		}

		///<remarks>
		/// 1. Base must be present and a QName
		///</remarks>
		[MonoTODO]
		internal int Compile(ValidationEventHandler h, XmlSchema schema)
		{
			// If this is already compiled this time, simply skip.
			if (this.IsComplied (schema.CompilationId))
				return 0;

			if(BaseTypeName == null || BaseTypeName.IsEmpty)
			{
				error(h, "base must be present and a QName");
			}
			else if(!XmlSchemaUtil.CheckQName(BaseTypeName))
				error(h,"BaseTypeName must be a QName");

			if(this.AnyAttribute != null)
			{
				errorCount += AnyAttribute.Compile(h,schema);
			}

			foreach(XmlSchemaObject obj in Attributes)
			{
				if(obj is XmlSchemaAttribute)
				{
					XmlSchemaAttribute attr = (XmlSchemaAttribute) obj;
					errorCount += attr.Compile(h,schema);
				}
				else if(obj is XmlSchemaAttributeGroupRef)
				{
					XmlSchemaAttributeGroupRef atgrp = (XmlSchemaAttributeGroupRef) obj;
					errorCount += atgrp.Compile(h,schema);
				}
				else
					error(h,obj.GetType() +" is not valid in this place::SimpleConentExtension");
			}
			
			XmlSchemaUtil.CompileID(Id,this,schema.IDCollection,h);

			this.CompilationId = schema.CompilationId;
			return errorCount;
		}
		
		[MonoTODO]
		internal int Validate(ValidationEventHandler h)
		{
			return errorCount;
		}
		//<extension 
		//base = QName 
		//id = ID 
		//{any attributes with non-schema namespace . . .}>
		//Content: (annotation?, ((attribute | attributeGroup)*, anyAttribute?))
		//</extension>
		internal static XmlSchemaSimpleContentExtension Read(XmlSchemaReader reader, ValidationEventHandler h)
		{
			XmlSchemaSimpleContentExtension extension = new XmlSchemaSimpleContentExtension();
			reader.MoveToElement();

			if(reader.NamespaceURI != XmlSchema.Namespace || reader.LocalName != xmlname)
			{
				error(h,"Should not happen :1: XmlSchemaAttributeGroup.Read, name="+reader.Name,null);
				reader.Skip();
				return null;
			}

			extension.LineNumber = reader.LineNumber;
			extension.LinePosition = reader.LinePosition;
			extension.SourceUri = reader.BaseURI;

			while(reader.MoveToNextAttribute())
			{
				if(reader.Name == "base")
				{
					Exception innerex;
					extension.baseTypeName= XmlSchemaUtil.ReadQNameAttribute(reader,out innerex);
					if(innerex != null)
						error(h, reader.Value + " is not a valid value for base attribute",innerex);
				}
				else if(reader.Name == "id")
				{
					extension.Id = reader.Value;
				}
				else if((reader.NamespaceURI == "" && reader.Name != "xmlns") || reader.NamespaceURI == XmlSchema.Namespace)
				{
					error(h,reader.Name + " is not a valid attribute for extension in this context",null);
				}
				else
				{
					XmlSchemaUtil.ReadUnhandledAttribute(reader,extension);
				}
			}
			
			reader.MoveToElement();
			if(reader.IsEmptyElement)
				return extension;

			//Content: 1.annotation?, 2.(attribute | attributeGroup)*, 3.anyAttribute?
			int level = 1;
			while(reader.ReadNextElement())
			{
				if(reader.NodeType == XmlNodeType.EndElement)
				{
					if(reader.LocalName != xmlname)
						error(h,"Should not happen :2: XmlSchemaSimpleContentExtension.Read, name="+reader.Name,null);
					break;
				}
				if(level <= 1 && reader.LocalName == "annotation")
				{
					level = 2; //Only one annotation
					XmlSchemaAnnotation annotation = XmlSchemaAnnotation.Read(reader,h);
					if(annotation != null)
						extension.Annotation = annotation;
					continue;
				}
				if(level <= 2)
				{
					if(reader.LocalName == "attribute")
					{
						level = 2;
						XmlSchemaAttribute attr = XmlSchemaAttribute.Read(reader,h);
						if(attr != null)
							extension.Attributes.Add(attr);
						continue;
					}
					if(reader.LocalName == "attributeGroup")
					{
						level = 2;
						XmlSchemaAttributeGroupRef attr = XmlSchemaAttributeGroupRef.Read(reader,h);
						if(attr != null)
							extension.attributes.Add(attr);
						continue;
					}
				}
				if(level <= 3 && reader.LocalName == "anyAttribute")
				{
					level = 4;
					XmlSchemaAnyAttribute anyattr = XmlSchemaAnyAttribute.Read(reader,h);
					if(anyattr != null)
						extension.AnyAttribute = anyattr;
					continue;
				}
				reader.RaiseInvalidElementError();
			}
			return extension;
		}
	}
}
