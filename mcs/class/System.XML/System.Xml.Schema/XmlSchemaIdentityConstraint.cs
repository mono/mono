// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
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

		[XmlElement("selector",typeof(XmlSchemaXPath),Namespace=XmlSchema.Namespace)]
		public XmlSchemaXPath Selector 
		{
			get{ return  selector; } 
			set{ selector = value; }
		}

		[XmlElement("field",typeof(XmlSchemaXPath),Namespace=XmlSchema.Namespace)]
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

		/// <remarks>
		/// 1. name must be present
		/// 2. selector and field must be present
		/// </remarks>
		internal override int Compile(ValidationEventHandler h, XmlSchema schema)
		{
			// If this is already compiled this time, simply skip.
			if (this.IsComplied (schema.CompilationId))
				return 0;

			if(Name == null)
				error(h,"Required attribute name must be present");
			else if(!XmlSchemaUtil.CheckNCName(this.name)) 
				error(h,"attribute name must be NCName");
			else {
				this.qName = new XmlQualifiedName(Name,schema.TargetNamespace);
				if (schema.NamedIdentities.Contains (qName))
					error(h,"There is already same named identity constraint in this namespace.");
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
