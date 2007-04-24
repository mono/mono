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
using System.Collections;
using System.Xml;
using System.Xml.Serialization;
using Mono.Xml.Schema;

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

		private XsdIdentitySelector compiledSelector;
//		ArrayList compiledFields;

		public XmlSchemaIdentityConstraint()
		{
			fields = new XmlSchemaObjectCollection();
			qName = XmlQualifiedName.Empty;
		}
		
		[System.Xml.Serialization.XmlAttribute("name")]
		public string Name 
		{
			get{ return  name; } 
			set{ name = value; }
		}

		[XmlElement("selector",typeof(XmlSchemaXPath))]
		public XmlSchemaXPath Selector 
		{
			get{ return  selector; } 
			set{ selector = value; }
		}

		[XmlElement("field",typeof(XmlSchemaXPath))]
		public XmlSchemaObjectCollection Fields 
		{
			get{ return fields; }
		}
		
		[XmlIgnore]
		public XmlQualifiedName QualifiedName 
		{
			get{ return  qName; }
		}

		internal XsdIdentitySelector CompiledSelector {
			get { return compiledSelector; }
		}

		internal override void SetParent (XmlSchemaObject parent)
		{
			base.SetParent (parent);
			if (Selector != null)
				Selector.SetParent (this);
			foreach (XmlSchemaObject obj in Fields)
				obj.SetParent (this);
		}

		/// <remarks>
		/// 1. name must be present
		/// 2. selector and field must be present
		/// </remarks>
		internal override int Compile(ValidationEventHandler h, XmlSchema schema)
		{
			// If this is already compiled this time, simply skip.
			if (CompilationId == schema.CompilationId)
				return 0;

			if(Name == null)
				error(h,"Required attribute name must be present");
			else if(!XmlSchemaUtil.CheckNCName(this.name)) 
				error(h,"attribute name must be NCName");
			else {
				this.qName = new XmlQualifiedName(Name, AncestorSchema.TargetNamespace);
				if (schema.NamedIdentities.Contains (qName)) {
					XmlSchemaIdentityConstraint existing =
						schema.NamedIdentities [qName] as XmlSchemaIdentityConstraint;
					error(h, String.Format ("There is already same named identity constraint in this namespace. Existing item is at {0}({1},{2})", existing.SourceUri, existing.LineNumber, existing.LinePosition));
				}
				else
					schema.NamedIdentities.Add (qName, this);
			}

			if(Selector == null)
				error(h,"selector must be present");
			else
			{
				Selector.isSelector = true;
				errorCount += Selector.Compile(h,schema);
				if (selector.errorCount == 0)
					compiledSelector = new XsdIdentitySelector (Selector);
			}
			if (errorCount > 0)
				return errorCount; // fatal

			if(Fields.Count == 0)
				error(h,"atleast one field value must be present");
			else
			{
				for (int i = 0; i < Fields.Count; i++)
				{
					XmlSchemaXPath field = Fields [i] as XmlSchemaXPath;
					if(field != null)
					{
						errorCount += field.Compile(h,schema);
						if (field.errorCount == 0)
							this.compiledSelector.AddField (new XsdIdentityField (field, i));
					}
					else
						error (h, "Object of type " + Fields [i].GetType() + " is invalid in the Fields Collection");
				}
			}
			XmlSchemaUtil.CompileID(Id,this,schema.IDCollection,h);

			this.CompilationId = schema.CompilationId;
			return errorCount;
		}
	}
}
