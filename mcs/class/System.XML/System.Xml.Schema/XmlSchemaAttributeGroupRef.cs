//
// System.Xml.Schema.XmlSchemaAttributeGroupRef.cs
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
	/// Summary description for XmlSchemaAttributeGroupRef.
	/// </summary>
	public class XmlSchemaAttributeGroupRef : XmlSchemaAnnotated
	{
		private XmlQualifiedName refName;
		const string xmlname = "attributeGroup";

		public XmlSchemaAttributeGroupRef()
		{
			refName = XmlQualifiedName.Empty;
		}
		
		[System.Xml.Serialization.XmlAttribute("ref")]
		public XmlQualifiedName RefName 
		{
			get{ return  refName; }
			set{ refName = value; }
		}

		/// <remarks>
		/// 1. ref must be present
		/// </remarks>
		internal override int Compile(ValidationEventHandler h, XmlSchema schema)
		{
			// If this is already compiled this time, simply skip.
			if (CompilationId == schema.CompilationId)
				return 0;

			errorCount = 0;
			if(RefName == null || RefName.IsEmpty)
				error(h, "ref must be present");
			else if(!XmlSchemaUtil.CheckQName(RefName))
				error(h, "ref must be a valid qname");

			XmlSchemaUtil.CompileID(Id,this, schema.IDCollection,h);

			this.CompilationId = schema.CompilationId;
			return errorCount;
		}
		
		internal override int Validate(ValidationEventHandler h, XmlSchema schema)
		{
			return errorCount;
		}

		//<attributeGroup
		//  id = ID
		//  ref = QName
		//  {any attributes with non-schema namespace . . .}>
		//  Content: (annotation?)
		//</attributeGroup>
		internal static XmlSchemaAttributeGroupRef Read(XmlSchemaReader reader, ValidationEventHandler h)
		{
			XmlSchemaAttributeGroupRef attrgrp = new XmlSchemaAttributeGroupRef();
			reader.MoveToElement();

			if(reader.NamespaceURI != XmlSchema.Namespace || reader.LocalName != xmlname)
			{
				error(h,"Should not happen :1: XmlSchemaAttributeGroupRef.Read, name="+reader.Name,null);
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
				else if(reader.Name == "ref")
				{
					Exception innerex;
					attrgrp.refName = XmlSchemaUtil.ReadQNameAttribute(reader,out innerex);
					if(innerex != null)
						error(h, reader.Value + " is not a valid value for ref attribute",innerex);
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
			int level = 1;

			while(reader.ReadNextElement())
			{
				if(reader.NodeType == XmlNodeType.EndElement)
				{
					if(reader.LocalName != xmlname)
						error(h,"Should not happen :2: XmlSchemaAttributeGroupRef.Read, name="+reader.Name,null);
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
				reader.RaiseInvalidElementError();
			}
			return attrgrp;
		}
	}
}
