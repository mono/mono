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
	/// Summary description for XmlSchemaSimpleContentRestriction.
	/// </summary>
	public class XmlSchemaSimpleContentRestriction : XmlSchemaContent
	{
		
		private XmlSchemaAnyAttribute any;
		private XmlSchemaObjectCollection attributes;
		private XmlSchemaSimpleType baseType;
		private XmlQualifiedName baseTypeName;
		private XmlSchemaObjectCollection facets;
		const string xmlname = "restriction";

		public XmlSchemaSimpleContentRestriction()
		{
			baseTypeName = XmlQualifiedName.Empty;
			attributes	 = new XmlSchemaObjectCollection();
			facets		 = new XmlSchemaObjectCollection();
		}

		[System.Xml.Serialization.XmlAttribute("base")]
		public XmlQualifiedName BaseTypeName 
		{
			get{ return  baseTypeName; }
			set{ baseTypeName = value; }
		}

		[XmlElement("simpleType", Type=typeof (XmlSchemaSimpleType))]
		public XmlSchemaSimpleType BaseType 
		{ 
			get{ return  baseType; } 
			set{ baseType = value; } 
		}
		
		[XmlElement("minExclusive",typeof(XmlSchemaMinExclusiveFacet))]
		[XmlElement("minInclusive",typeof(XmlSchemaMinInclusiveFacet))] 
		[XmlElement("maxExclusive",typeof(XmlSchemaMaxExclusiveFacet))]
		[XmlElement("maxInclusive",typeof(XmlSchemaMaxInclusiveFacet))]
		[XmlElement("totalDigits",typeof(XmlSchemaTotalDigitsFacet))]
		[XmlElement("fractionDigits",typeof(XmlSchemaFractionDigitsFacet))]
		[XmlElement("length",typeof(XmlSchemaLengthFacet))]
		[XmlElement("minLength",typeof(XmlSchemaMinLengthFacet))]
		[XmlElement("maxLength",typeof(XmlSchemaMaxLengthFacet))]
		[XmlElement("enumeration",typeof(XmlSchemaEnumerationFacet))]
		[XmlElement("whiteSpace",typeof(XmlSchemaWhiteSpaceFacet))]
		[XmlElement("pattern",typeof(XmlSchemaPatternFacet))]
		public XmlSchemaObjectCollection Facets 
		{ 
			get{ return facets; } 
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
			if (BaseType != null)
				BaseType.SetParent (this);
			if (AnyAttribute != null)
				AnyAttribute.SetParent (this);
			foreach (XmlSchemaObject obj in Attributes)
				obj.SetParent (this);
		}

		///<remarks>
		/// 1. Base must be present and a QName
		///</remarks>
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
			}

			if(BaseTypeName == null || BaseTypeName.IsEmpty)
			{
				error(h, "base must be present, as a QName");
			}
			else if(!XmlSchemaUtil.CheckQName(BaseTypeName))
				error(h,"BaseTypeName must be a QName");

			if(BaseType != null)
			{
				errorCount += BaseType.Compile(h,schema);
			}

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
					error(h,obj.GetType() +" is not valid in this place::SimpleContentRestriction");
			}
			
			//TODO: Compile Facets: Looks like they are a part of datatypes. So we'll do them with the datatypes

			
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

			if (baseType != null) {
				baseType.Validate (h, schema);
				actualBaseSchemaType = baseType;
			}
			else if (baseTypeName != XmlQualifiedName.Empty) {
				XmlSchemaType st = schema.FindSchemaType (baseTypeName);
				if (st != null) {
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
			}

			ValidationId = schema.ValidationId;
			return errorCount;
		}

		//<restriction 
		//base = QName 
		//id = ID 
		//{any attributes with non-schema namespace . . .}>
		//Content: (annotation?, (simpleType?, (minExclusive | minInclusive | maxExclusive | maxInclusive | totalDigits | fractionDigits | length | minLength | maxLength | enumeration | whiteSpace | pattern)*)?, ((attribute | attributeGroup)*, anyAttribute?))
		//</restriction>
		internal static XmlSchemaSimpleContentRestriction Read(XmlSchemaReader reader, ValidationEventHandler h)
		{
			XmlSchemaSimpleContentRestriction restriction = new XmlSchemaSimpleContentRestriction();
			reader.MoveToElement();

			if(reader.NamespaceURI != XmlSchema.Namespace || reader.LocalName != xmlname)
			{
				error(h,"Should not happen :1: XmlSchemaComplexContentRestriction.Read, name="+reader.Name,null);
				reader.SkipToEnd();
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
			
			//Content:  1.annotation?, 
			//		    2.simpleType?, 
			//			3.(minExclusive |...| enumeration | whiteSpace | pattern)*, 
			//			4.(attribute | attributeGroup)*, 
			//			5.anyAttribute?
			int level = 1;
			while(reader.ReadNextElement())
			{
				if(reader.NodeType == XmlNodeType.EndElement)
				{
					if(reader.LocalName != xmlname)
						error(h,"Should not happen :2: XmlSchemaSimpleContentRestriction.Read, name="+reader.Name,null);
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
				if(level <=2 && reader.LocalName == "simpleType")
				{
					level = 3;
					XmlSchemaSimpleType stype = XmlSchemaSimpleType.Read(reader,h);
					if(stype != null)
						restriction.baseType = stype;
					continue;
				}
				if(level <= 3)
				{
					if(reader.LocalName == "minExclusive")
					{
						level = 3;
						XmlSchemaMinExclusiveFacet minex = XmlSchemaMinExclusiveFacet.Read(reader,h);
						if(minex != null)
							restriction.facets.Add(minex);
						continue;
					}
					else if(reader.LocalName == "minInclusive")
					{
						level = 3;
						XmlSchemaMinInclusiveFacet mini = XmlSchemaMinInclusiveFacet.Read(reader,h);
						if(mini != null)
							restriction.facets.Add(mini);
						continue;
					}
					else if(reader.LocalName == "maxExclusive")
					{
						level = 3;
						XmlSchemaMaxExclusiveFacet maxex = XmlSchemaMaxExclusiveFacet.Read(reader,h);
						if(maxex != null)
							restriction.facets.Add(maxex);
						continue;
					}
					else if(reader.LocalName == "maxInclusive")
					{
						level = 3;
						XmlSchemaMaxInclusiveFacet maxi = XmlSchemaMaxInclusiveFacet.Read(reader,h);
						if(maxi != null)
							restriction.facets.Add(maxi);
						continue;
					}
					else if(reader.LocalName == "totalDigits")
					{
						level = 3;
						XmlSchemaTotalDigitsFacet total = XmlSchemaTotalDigitsFacet.Read(reader,h);
						if(total != null)
							restriction.facets.Add(total);
						continue;
					}
					else if(reader.LocalName == "fractionDigits")
					{
						level = 3;
						XmlSchemaFractionDigitsFacet fraction = XmlSchemaFractionDigitsFacet.Read(reader,h);
						if(fraction != null)
							restriction.facets.Add(fraction);
						continue;
					}
					else if(reader.LocalName == "length")
					{
						level = 3;
						XmlSchemaLengthFacet length = XmlSchemaLengthFacet.Read(reader,h);
						if(length != null)
							restriction.facets.Add(length);
						continue;
					}
					else if(reader.LocalName == "minLength")
					{
						level = 3;
						XmlSchemaMinLengthFacet minlen = XmlSchemaMinLengthFacet.Read(reader,h);
						if(minlen != null)
							restriction.facets.Add(minlen);
						continue;
					}
					else if(reader.LocalName == "maxLength")
					{
						level = 3;
						XmlSchemaMaxLengthFacet maxlen = XmlSchemaMaxLengthFacet.Read(reader,h);
						if(maxlen != null)
							restriction.facets.Add(maxlen);
						continue;
					}
					else if(reader.LocalName == "enumeration")
					{
						level = 3;
						XmlSchemaEnumerationFacet enumeration = XmlSchemaEnumerationFacet.Read(reader,h);
						if(enumeration != null)
							restriction.facets.Add(enumeration);
						continue;
					}
					else if(reader.LocalName == "whiteSpace")
					{
						level = 3;
						XmlSchemaWhiteSpaceFacet ws = XmlSchemaWhiteSpaceFacet.Read(reader,h);
						if(ws != null)
							restriction.facets.Add(ws);
						continue;
					}
					else if(reader.LocalName == "pattern")
					{
						level = 3;
						XmlSchemaPatternFacet pattern = XmlSchemaPatternFacet.Read(reader,h);
						if(pattern != null)
							restriction.facets.Add(pattern);
						continue;
					}
				}
				if(level <= 4)
				{
					if(reader.LocalName == "attribute")
					{
						level = 4;
						XmlSchemaAttribute attr = XmlSchemaAttribute.Read(reader,h);
						if(attr != null)
							restriction.Attributes.Add(attr);
						continue;
					}
					if(reader.LocalName == "attributeGroup")
					{
						level = 4;
						XmlSchemaAttributeGroupRef attr = XmlSchemaAttributeGroupRef.Read(reader,h);
						if(attr != null)
							restriction.attributes.Add(attr);
						continue;
					}
				}
				if(level <= 5 && reader.LocalName == "anyAttribute")
				{
					level = 6;
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
