// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml.Serialization;
using System.Xml;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaAttributeGroup.
	/// </summary>
	public class XmlSchemaAttributeGroup : XmlSchemaAnnotated
	{
		private XmlSchemaAnyAttribute any;
		private XmlSchemaObjectCollection attributes;
		private string name;
		private XmlSchemaAttributeGroup redefined;
		private XmlQualifiedName qualifiedName;
		private static string xmlname = "attributeGroup";

		public XmlSchemaAttributeGroup()
		{
			attributes  = new XmlSchemaObjectCollection();
		}

		[System.Xml.Serialization.XmlAttribute("name")]
		public string Name 
		{
			get{ return name;}
			set{ name = value;}
		}

		[XmlElement("attribute",typeof(XmlSchemaAttribute),Namespace=XmlSchema.Namespace)]
		[XmlElement("attributeGroup",typeof(XmlSchemaAttributeGroupRef),Namespace=XmlSchema.Namespace)]
		public XmlSchemaObjectCollection Attributes 
		{
			get{ return attributes;}
		}

		[XmlElement("anyAttribute",Namespace=XmlSchema.Namespace)]
		public XmlSchemaAnyAttribute AnyAttribute 
		{
			get{ return any;}
			set{ any = value;}
		}

		//Undocumented property
		[XmlIgnore]
		public XmlSchemaAttributeGroup RedefinedAttributeGroup 
		{
			get{ return redefined;}
		}

		[XmlIgnore]
		internal XmlQualifiedName QualifiedName 
		{
			get{ return qualifiedName;}
		}

		/// <remarks>
		/// An Attribute group can only be defined as a child of XmlSchema or in XmlSchemaRedefine.
		/// The other attributeGroup has type XmlSchemaAttributeGroupRef.
		///  1. Name must be present
		/// </remarks>
		[MonoTODO]
		internal int Compile(ValidationEventHandler h, XmlSchema schema)
		{
			// If this is already compiled this time, simply skip.
			if (this.IsComplied (schema.CompilationId))
				return 0;

			errorCount = 0;

			XmlSchemaUtil.CompileID(Id,this, schema.IDCollection,h);

			if(this.Name == null) //1
				error(h,"Name is required in top level simpletype");
			else if(!XmlSchemaUtil.CheckNCName(this.Name)) // b.1.2
				error(h,"name attribute of a simpleType must be NCName");
			else
				this.qualifiedName = new XmlQualifiedName(this.Name, schema.TargetNamespace);
			
			if(this.AnyAttribute != null)
			{
				errorCount += this.AnyAttribute.Compile(h, schema);
			}
			
			foreach(XmlSchemaObject obj in Attributes)
			{
				if(obj is XmlSchemaAttribute)
				{
					XmlSchemaAttribute attr = (XmlSchemaAttribute) obj;
					errorCount += attr.Compile(h, schema);
				}
				else if(obj is XmlSchemaAttributeGroupRef)
				{
					XmlSchemaAttributeGroupRef gref = (XmlSchemaAttributeGroupRef) obj;
					errorCount += gref.Compile(h, schema);
				}
				else
				{
					error(h,"invalid type of object in Attributes property");
				}
			}
			this.CompilationId = schema.CompilationId;
			return errorCount;
		}
		
		[MonoTODO]
		internal int Validate(ValidationEventHandler h)
		{
			return errorCount;
		}

		//<attributeGroup
		//  id = ID
		//  name = NCName
		//  ref = QName // Not present in this class.
		//  {any attributes with non-schema namespace . . .}>
		//  Content: (annotation?, ((attribute | attributeGroup)*, anyAttribute?))
		//</attributeGroup>
		internal static XmlSchemaAttributeGroup Read(XmlSchemaReader reader, ValidationEventHandler h)
		{
			XmlSchemaAttributeGroup attrgrp = new XmlSchemaAttributeGroup();
			reader.MoveToElement();

			if(reader.NamespaceURI != XmlSchema.Namespace || reader.LocalName != xmlname)
			{
				error(h,"Should not happen :1: XmlSchemaAttributeGroup.Read, name="+reader.Name,null);
				reader.SkipToEnd();
				return null;
			}

			attrgrp.LineNumber = reader.LineNumber;
			attrgrp.LinePosition = reader.LinePosition;
			attrgrp.SourceUri = reader.BaseURI;

			while(reader.MoveToNextAttribute())
			{
				if(reader.Name == "id")
				{
					attrgrp.Id = reader.Value;
				}
				else if(reader.Name == "name")
				{
					attrgrp.name = reader.Value;
				}
				else if((reader.NamespaceURI == "" && reader.Name != "xmlns") || reader.NamespaceURI == XmlSchema.Namespace)
				{
					error(h,reader.Name + " is not a valid attribute for attributeGroup in this context",null);
				}
				else
				{
					XmlSchemaUtil.ReadUnhandledAttribute(reader,attrgrp);
				}
			}
			
			reader.MoveToElement();
			if(reader.IsEmptyElement)
				return attrgrp;

			//Content: 1.annotation?, 2.(attribute | attributeGroup)*, 3.anyAttribute?
			int level = 1;
			while(reader.ReadNextElement())
			{
				if(reader.NodeType == XmlNodeType.EndElement)
				{
					if(reader.LocalName != xmlname)
						error(h,"Should not happen :2: XmlSchemaAttributeGroup.Read, name="+reader.Name,null);
					break;
				}
				if(level <= 1 && reader.LocalName == "annotation")
				{
					level = 2; //Only one annotation
					XmlSchemaAnnotation annotation = XmlSchemaAnnotation.Read(reader,h);
					if(annotation != null)
						attrgrp.Annotation = annotation;
					continue;
				}
				if(level <= 2)
				{
					if(reader.LocalName == "attribute")
					{
						level = 2;
						XmlSchemaAttribute attr = XmlSchemaAttribute.Read(reader,h);
						if(attr != null)
							attrgrp.Attributes.Add(attr);
						continue;
					}
					if(reader.LocalName == "attributeGroup")
					{
						level = 2;
						XmlSchemaAttributeGroupRef attr = XmlSchemaAttributeGroupRef.Read(reader,h);
						if(attr != null)
							attrgrp.attributes.Add(attr);
						continue;
					}
				}
				if(level <= 3 && reader.LocalName == "anyAttribute")
				{
					level = 4;
					XmlSchemaAnyAttribute anyattr = XmlSchemaAnyAttribute.Read(reader,h);
					if(anyattr != null)
						attrgrp.AnyAttribute = anyattr;
					continue;
				}
				reader.RaiseInvalidElementError();
			}
			return attrgrp;
		}
		
	}
}
