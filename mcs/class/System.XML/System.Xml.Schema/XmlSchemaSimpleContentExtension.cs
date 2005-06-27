//
// System.Xml.Schema.XmlSchemaSimpleContentExtension.cs
//
// Author:
//	Dwivedi, Ajay kumar  Adwiv@Yahoo.com
//	Atsushi Enomoto  ginga@kit.hi-ho.ne.jp
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
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
		const string xmlname = "extension";

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

		// internal properties
		internal override bool IsExtension {
			get { return true; }
		}

		///<remarks>
		/// 1. Base must be present and a QName
		///</remarks>
		internal override int Compile(ValidationEventHandler h, XmlSchema schema)
		{
			// If this is already compiled this time, simply skip.
			if (this.IsComplied (schema.CompilationId))
				return 0;

#if NET_2_0
			if (AnyAttribute != null)
				AnyAttribute.Parent = this;
			foreach (XmlSchemaObject obj in Attributes)
				obj.Parent = this;
#endif

			if (this.isRedefinedComponent) {
				if (Annotation != null)
					Annotation.isRedefinedComponent = true;
				if (AnyAttribute != null)
					AnyAttribute.isRedefinedComponent = true;
				foreach (XmlSchemaObject obj in Attributes)
					obj.isRedefinedComponent = true;
			}

			if(BaseTypeName == null || BaseTypeName.IsEmpty)
			{
				error(h, "base must be present, as a QName");
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
		
		internal override XmlQualifiedName GetBaseTypeName ()
		{
			return baseTypeName;
		}

		internal override XmlSchemaParticle GetParticle ()
		{
			return null;
		}

		internal override int Validate(ValidationEventHandler h, XmlSchema schema)
		{
			if (IsValidated (schema.ValidationId))
				return errorCount;

			XmlSchemaType st = schema.SchemaTypes [baseTypeName] as XmlSchemaType;
			if (st != null) {
				XmlSchemaComplexType ct = st as XmlSchemaComplexType;
				if (ct != null && ct.ContentModel is XmlSchemaComplexContent)
					error (h, "Specified type is complex type which contains complex content.");
				st.Validate (h, schema);
				actualBaseSchemaType = st;
			} else if (baseTypeName == XmlSchemaComplexType.AnyTypeName) {
				actualBaseSchemaType = XmlSchemaComplexType.AnyType;
			} else if (XmlSchemaUtil.IsBuiltInDatatypeName (baseTypeName)) {
				actualBaseSchemaType = XmlSchemaDatatype.FromName (baseTypeName);
				if (actualBaseSchemaType == null)
					error (h, "Invalid schema datatype name is specified.");
			}
			// otherwise, it might be missing sub components.
			else if (!schema.IsNamespaceAbsent (baseTypeName.Namespace))
				error (h, "Referenced base schema type " + baseTypeName + " was not found in the corresponding schema.");

			ValidationId = schema.ValidationId;
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
