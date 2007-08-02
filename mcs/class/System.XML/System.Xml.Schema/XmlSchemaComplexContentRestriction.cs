//
// System.Xml.Schema.XmlSchemaComplexContentRestriction.cs
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
	/// Summary description for XmlSchemaComplexContentRestriction.
	/// </summary>
	public class XmlSchemaComplexContentRestriction : XmlSchemaContent
	{
		private XmlSchemaAnyAttribute any;
		private XmlSchemaObjectCollection attributes;
		private XmlQualifiedName baseTypeName;
		private XmlSchemaParticle particle;
		const string xmlname = "restriction";

		public XmlSchemaComplexContentRestriction()
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

		[XmlElement("group",typeof(XmlSchemaGroupRef))]
		[XmlElement("all",typeof(XmlSchemaAll))]
		[XmlElement("choice",typeof(XmlSchemaChoice))]
		[XmlElement("sequence",typeof(XmlSchemaSequence))]
		public XmlSchemaParticle Particle 
		{
			get{ return  particle; }
			set{ particle = value; }
		}

		[XmlElement("attribute",typeof(XmlSchemaAttribute))]
		[XmlElement("attributeGroup",typeof(XmlSchemaAttributeGroupRef))]
		public XmlSchemaObjectCollection Attributes 
		{
			get{ return attributes; }
		}

		[XmlElement("anyAttribute")]
		public XmlSchemaAnyAttribute AnyAttribute 
		{
			get{ return  any; }
			set{ any = value; }
		}

		// internal properties
		internal override bool IsExtension {
			get { return false; }
		}

		internal override void SetParent (XmlSchemaObject parent)
		{
			base.SetParent (parent);
			if (Particle != null)
				Particle.SetParent (this);
			if (AnyAttribute != null)
				AnyAttribute.SetParent (this);
			foreach (XmlSchemaObject obj in Attributes)
				obj.SetParent (this);
		}

		/// <remarks>
		/// 1. base must be present
		/// </remarks>
		internal override int Compile(ValidationEventHandler h, XmlSchema schema)
		{
			// If this is already compiled this time, simply skip.
			if (CompilationId == schema.CompilationId)
				return 0;

			if (this.isRedefinedComponent) {
				if (Annotation != null)
					Annotation.isRedefinedComponent = true;
				if (AnyAttribute != null)
					AnyAttribute.isRedefinedComponent = true;
				foreach (XmlSchemaObject obj in Attributes)
					obj.isRedefinedComponent = true;
				if (Particle != null)
					Particle.isRedefinedComponent = true;
			}

			if(BaseTypeName == null || BaseTypeName.IsEmpty)
			{
				error(h, "base must be present, as a QName");
			}
			else if(!XmlSchemaUtil.CheckQName(BaseTypeName))
				error(h,"BaseTypeName is not a valid XmlQualifiedName");

			if(this.AnyAttribute != null)
			{
				errorCount += AnyAttribute.Compile(h, schema);
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
					XmlSchemaAttributeGroupRef atgrp = (XmlSchemaAttributeGroupRef) obj;
					errorCount += atgrp.Compile(h, schema);
				}
				else
					error(h,obj.GetType() +" is not valid in this place::ComplexContentRestriction");
			}
			
			if(Particle != null)
			{
				if(Particle is XmlSchemaGroupRef)
				{
					errorCount += ((XmlSchemaGroupRef)Particle).Compile(h, schema);
				}
				else if(Particle is XmlSchemaAll)
				{
					errorCount += ((XmlSchemaAll)Particle).Compile(h, schema);
				}
				else if(Particle is XmlSchemaChoice)
				{
					errorCount += ((XmlSchemaChoice)Particle).Compile(h, schema);
				}
				else if(Particle is XmlSchemaSequence)
				{
					errorCount += ((XmlSchemaSequence)Particle).Compile(h, schema);
				}
				else
					error (h, "Particle of a restriction is limited only to group, sequence, choice and all.");
			}
			
			XmlSchemaUtil.CompileID(Id,this, schema.IDCollection,h);

			this.CompilationId = schema.CompilationId;
			return errorCount;
		}
		
		internal override XmlQualifiedName GetBaseTypeName ()
		{
			return baseTypeName;
		}

		internal override XmlSchemaParticle GetParticle ()
		{
			return particle;
		}

		internal override int Validate (ValidationEventHandler h, XmlSchema schema)
		{
			if (Particle != null)
				Particle.Validate (h, schema);
			return errorCount;
		}

		//<restriction
		//  base = QName
		//  id = ID
		//  {any attributes with non-schema namespace . . .}>
		//  Content: (annotation?, ((group | all | choice | sequence)?, ((attribute | attributeGroup)*, anyAttribute?)))
		//</restriction>
		internal static XmlSchemaComplexContentRestriction Read(XmlSchemaReader reader, ValidationEventHandler h)
		{
			XmlSchemaComplexContentRestriction restriction = new XmlSchemaComplexContentRestriction();
			reader.MoveToElement();

			if(reader.NamespaceURI != XmlSchema.Namespace || reader.LocalName != xmlname)
			{
				error(h,"Should not happen :1: XmlSchemaComplexContentRestriction.Read, name="+reader.Name,null);
				reader.Skip();
				return null;
			}

			restriction.LineNumber = reader.LineNumber;
			restriction.LinePosition = reader.LinePosition;
			restriction.SourceUri = reader.BaseURI;

			while(reader.MoveToNextAttribute())
			{
				if(reader.Name == "base")
				{
					Exception innerex;
					restriction.baseTypeName = XmlSchemaUtil.ReadQNameAttribute(reader,out innerex);
					if(innerex != null)
						error(h, reader.Value + " is not a valid value for base attribute",innerex);
				}
				else if(reader.Name == "id")
				{
					restriction.Id = reader.Value;
				}
				else if((reader.NamespaceURI == "" && reader.Name != "xmlns") || reader.NamespaceURI == XmlSchema.Namespace)
				{
					error(h,reader.Name + " is not a valid attribute for restriction",null);
				}
				else
				{
					XmlSchemaUtil.ReadUnhandledAttribute(reader,restriction);
				}
			}
			
			reader.MoveToElement();
			if(reader.IsEmptyElement)
				return restriction;
			//Content: 1. annotation?, 
			//			(2.(group | all | choice | sequence)?, (3.(attribute | attributeGroup)*, 4.anyAttribute?)))
			int level = 1;
			while(reader.ReadNextElement())
			{
				if(reader.NodeType == XmlNodeType.EndElement)
				{
					if(reader.LocalName != xmlname)
						error(h,"Should not happen :2: XmlSchemaComplexContentRestriction.Read, name="+reader.Name,null);
					break;
				}
				if(level <= 1 && reader.LocalName == "annotation")
				{
					level = 2; //Only one annotation
					XmlSchemaAnnotation annotation = XmlSchemaAnnotation.Read(reader,h);
					if(annotation != null)
						restriction.Annotation = annotation;
					continue;
				}
				if(level <= 2)
				{
					if(reader.LocalName == "group")
					{
						level = 3;
						XmlSchemaGroupRef group = XmlSchemaGroupRef.Read(reader,h);
						if(group != null)
							restriction.particle = group;
						continue;
					}
					if(reader.LocalName == "all")
					{
						level = 3;
						XmlSchemaAll all = XmlSchemaAll.Read(reader,h);
						if(all != null)
							restriction.particle = all;
						continue;
					}
					if(reader.LocalName == "choice")
					{
						level = 3;
						XmlSchemaChoice choice = XmlSchemaChoice.Read(reader,h);
						if(choice != null)
							restriction.particle = choice;
						continue;
					}
					if(reader.LocalName == "sequence")
					{
						level = 3;
						XmlSchemaSequence sequence = XmlSchemaSequence.Read(reader,h);
						if(sequence != null)
							restriction.particle = sequence;
						continue;
					}
				}
				if(level <= 3)
				{
					if(reader.LocalName == "attribute")
					{
						level = 3;
						XmlSchemaAttribute attr = XmlSchemaAttribute.Read(reader,h);
						if(attr != null)
							restriction.Attributes.Add(attr);
						continue;
					}
					if(reader.LocalName == "attributeGroup")
					{
						level = 3;
						XmlSchemaAttributeGroupRef attr = XmlSchemaAttributeGroupRef.Read(reader,h);
						if(attr != null)
							restriction.attributes.Add(attr);
						continue;
					}
				}
				if(level <= 4 && reader.LocalName == "anyAttribute")
				{
					level = 5;
					XmlSchemaAnyAttribute anyattr = XmlSchemaAnyAttribute.Read(reader,h);
					if(anyattr != null)
						restriction.AnyAttribute = anyattr;
					continue;
				}
				reader.RaiseInvalidElementError();
			}
			return restriction;
		}
	}
}
