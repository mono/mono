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
	/// Summary description for XmlSchemaKeyref.
	/// </summary>
	public class XmlSchemaKeyref : XmlSchemaIdentityConstraint
	{
		private XmlQualifiedName refer;
		const string xmlname = "keyref";
		private XmlSchemaIdentityConstraint target;

		public XmlSchemaKeyref()
		{
			refer = XmlQualifiedName.Empty;
		}

		[System.Xml.Serialization.XmlAttribute("refer")]
		public XmlQualifiedName Refer
		{
			get{ return  refer; } 
			set{ refer = value; }
		}

		internal XmlSchemaIdentityConstraint Target
		{
			get { return target; }
		}

		/// <remarks>
		/// 1. name must be present
		/// 2. selector and field must be present
		/// 3. refer must be present
		/// </remarks>
		internal override int Compile(ValidationEventHandler h, XmlSchema schema)
		{
			base.Compile(h, schema);

			if(refer == null || refer.IsEmpty)
				error(h,"refer must be present");
			else if(!XmlSchemaUtil.CheckQName(refer))
				error(h,"Refer is not a valid XmlQualifiedName");

			return errorCount;
		}
		
		internal override int Validate (ValidationEventHandler h, XmlSchema schema)
		{
			// Find target key
			XmlSchemaIdentityConstraint target = schema.NamedIdentities [this.Refer] as XmlSchemaIdentityConstraint;
			if (target == null)
				error (h, "Target key was not found.");
			else if (target is XmlSchemaKeyref)
				error (h, "Target identity constraint was keyref.");
			else if (target.Fields.Count != this.Fields.Count)
				error (h, "Target identity constraint has different number of fields.");
			else
				this.target = target;
			return errorCount;
		}

		/*
		internal new void error(ValidationEventHandler handle, string message)
		{
			errorCount++;
			ValidationHandler.RaiseValidationError(handle, this, message);
		}
		*/
		//<key 
		//  id = ID 
		//  name = NCName 
		//  refer = QName 
		//  {any attributes with non-schema namespace . . .}>
		//  Content: (annotation?, (selector, field+))
		//</key>
		internal static XmlSchemaKeyref Read(XmlSchemaReader reader, ValidationEventHandler h)
		{
			XmlSchemaKeyref keyref = new XmlSchemaKeyref();
			reader.MoveToElement();

			if(reader.NamespaceURI != XmlSchema.Namespace || reader.LocalName != xmlname)
			{
				error(h,"Should not happen :1: XmlSchemaKeyref.Read, name="+reader.Name,null);
				reader.Skip();
				return null;
			}

			keyref.LineNumber = reader.LineNumber;
			keyref.LinePosition = reader.LinePosition;
			keyref.SourceUri = reader.BaseURI;

			while(reader.MoveToNextAttribute())
			{
				if(reader.Name == "id")
				{
					keyref.Id = reader.Value;
				}
				else if(reader.Name == "name")
				{
					keyref.Name = reader.Value;
				}
				else if(reader.Name == "refer")
				{
					Exception innerex;
					keyref.refer = XmlSchemaUtil.ReadQNameAttribute(reader,out innerex);
					if(innerex != null)
						error(h, reader.Value + " is not a valid value for refer attribute",innerex);
				}
				else if((reader.NamespaceURI == "" && reader.Name != "xmlns") || reader.NamespaceURI == XmlSchema.Namespace)
				{
					error(h,reader.Name + " is not a valid attribute for keyref",null);
				}
				else
				{
					XmlSchemaUtil.ReadUnhandledAttribute(reader,keyref);
				}
			}
			
			reader.MoveToElement();
			if(reader.IsEmptyElement)
				return keyref;

			//  Content: annotation?, selector, field+
			int level = 1;
			while(reader.ReadNextElement())
			{
				if(reader.NodeType == XmlNodeType.EndElement)
				{
					if(reader.LocalName != xmlname)
						error(h,"Should not happen :2: XmlSchemaKeyref.Read, name="+reader.Name,null);
					break;
				}
				if(level <= 1 && reader.LocalName == "annotation")
				{
					level = 2; //Only one annotation
					XmlSchemaAnnotation annotation = XmlSchemaAnnotation.Read(reader,h);
					if(annotation != null)
						keyref.Annotation = annotation;
					continue;
				}
				if(level <= 2 && reader.LocalName == "selector")
				{
					level = 3;
					XmlSchemaXPath selector = XmlSchemaXPath.Read(reader,h,"selector");
					if(selector != null)
						keyref.Selector = selector;
					continue;
				}
				if(level <= 3 && reader.LocalName == "field")
				{
					level = 3;
					if(keyref.Selector == null)
						error(h,"selector must be defined before field declarations",null);
					XmlSchemaXPath field = XmlSchemaXPath.Read(reader,h,"field");
					if(field != null)
						keyref.Fields.Add(field);
					continue;
				}
				reader.RaiseInvalidElementError();
			}
			return keyref;
		}
	}
}
