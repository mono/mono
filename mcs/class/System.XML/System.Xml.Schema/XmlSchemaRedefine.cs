// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com

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
	/// Summary description for XmlSchemaRedefine.
	/// </summary>
	public class XmlSchemaRedefine : XmlSchemaExternal
	{
		private XmlSchemaObjectTable attributeGroups;
		private XmlSchemaObjectTable groups;
		private XmlSchemaObjectCollection items;
		private XmlSchemaObjectTable schemaTypes;
		const string xmlname = "redefine";

		public XmlSchemaRedefine()
		{
			attributeGroups = new XmlSchemaObjectTable();
			groups = new XmlSchemaObjectTable();
			items = new XmlSchemaObjectCollection(this);
			schemaTypes = new XmlSchemaObjectTable();
		}
		
		[XmlElement("annotation",typeof(XmlSchemaAnnotation))]
		[XmlElement("simpleType",typeof(XmlSchemaSimpleType))]
		[XmlElement("complexType",typeof(XmlSchemaComplexType))]
		[XmlElement("group",typeof(XmlSchemaGroup))]
			//NOTE: AttributeGroup and not AttributeGroupRef
		[XmlElement("attributeGroup",typeof(XmlSchemaAttributeGroup))]
		public XmlSchemaObjectCollection Items 
		{
			get{ return items; }
		}

		[XmlIgnore]
		public XmlSchemaObjectTable AttributeGroups 
		{
			get{ return attributeGroups; }
		}
		
		[XmlIgnore]
		public XmlSchemaObjectTable SchemaTypes 
		{
			get{ return schemaTypes; }
		}

		[XmlIgnore]
		public XmlSchemaObjectTable Groups 
		{
			get{ return groups; }
		}

		internal override void SetParent (XmlSchemaObject parent)
		{
			base.SetParent (parent);
			foreach (XmlSchemaObject i in Items) {
				i.SetParent (this);
				i.isRedefinedComponent = true;
				i.isRedefineChild = true;
			}
		}

//<redefine 
//  id = ID 
//  schemaLocation = anyURI 
//  {any attributes with non-schema namespace . . .}>
//  Content: (annotation | (simpleType | complexType | group | attributeGroup))*
//</redefine>
		internal static XmlSchemaRedefine Read(XmlSchemaReader reader, ValidationEventHandler h)
		{
			XmlSchemaRedefine redefine = new XmlSchemaRedefine();
			reader.MoveToElement();

			if(reader.NamespaceURI != XmlSchema.Namespace || reader.LocalName != xmlname)
			{
				error(h,"Should not happen :1: XmlSchemaRedefine.Read, name="+reader.Name,null);
				reader.Skip();
				return null;
			}

			redefine.LineNumber = reader.LineNumber;
			redefine.LinePosition = reader.LinePosition;
			redefine.SourceUri = reader.BaseURI;

			while(reader.MoveToNextAttribute())
			{
				if(reader.Name == "id")
				{
					redefine.Id = reader.Value;
				}
				else if(reader.Name == "schemaLocation")
				{
					redefine.SchemaLocation = reader.Value;
				}
				else if((reader.NamespaceURI == "" && reader.Name != "xmlns") || reader.NamespaceURI == XmlSchema.Namespace)
				{
					error(h,reader.Name + " is not a valid attribute for redefine",null);
				}
				else
				{
					XmlSchemaUtil.ReadUnhandledAttribute(reader,redefine);
				}
			}

			reader.MoveToElement();
			if(reader.IsEmptyElement)
				return redefine;

			//(annotation | (simpleType | complexType | group | attributeGroup))*
			while(reader.ReadNextElement())
			{
				if(reader.NodeType == XmlNodeType.EndElement)
				{
					if(reader.LocalName != xmlname)
						error(h,"Should not happen :2: XmlSchemaRedefine.Read, name="+reader.Name,null);
					break;
				}
				if(reader.LocalName == "annotation")
				{
					XmlSchemaAnnotation annotation = XmlSchemaAnnotation.Read(reader,h);
					if(annotation != null)
						redefine.items.Add(annotation);
					continue;
				}
				if(reader.LocalName == "simpleType")
				{
					XmlSchemaSimpleType simpleType = XmlSchemaSimpleType.Read(reader,h);
					if(simpleType != null)
						redefine.items.Add(simpleType);
					continue;
				}
				if(reader.LocalName == "complexType")
				{
					XmlSchemaComplexType complexType = XmlSchemaComplexType.Read(reader,h);
					if(complexType != null)
						redefine.items.Add(complexType);
					continue;
				}
				if(reader.LocalName == "group")
				{
					XmlSchemaGroup group = XmlSchemaGroup.Read(reader,h);
					if(group != null)
						redefine.items.Add(group);
					continue;
				}
				if(reader.LocalName == "attributeGroup")
				{
					XmlSchemaAttributeGroup attributeGroup = XmlSchemaAttributeGroup.Read(reader,h);
					if(attributeGroup != null)
						redefine.items.Add(attributeGroup);
					continue;
				}
				reader.RaiseInvalidElementError();
			}
			return redefine;
		}
	}
}
