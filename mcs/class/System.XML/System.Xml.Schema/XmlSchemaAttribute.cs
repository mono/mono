//
// System.Xml.Schema.XmlSchemaAttribute.cs
//
// Authors:
//	Dwivedi, Ajay kumar  Adwiv@Yahoo.com
//	Enomoto, Atsushi     ginga@kit.hi-ho.ne.jp
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
using System.ComponentModel;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaAttribute.
	/// </summary>
	public class XmlSchemaAttribute : XmlSchemaAnnotated
	{
		private object attributeType;
#if NET_2_0
		private XmlSchemaSimpleType attributeSchemaType;
#endif
		private string defaultValue;
		private string fixedValue;
		private string validatedDefaultValue;
		private string validatedFixedValue;
		private XmlSchemaForm form;
		private string name;
		private string targetNamespace;
		private XmlQualifiedName qualifiedName;
		private XmlQualifiedName refName;
		private XmlSchemaSimpleType schemaType;
		private XmlQualifiedName schemaTypeName;
		private XmlSchemaUse use;
		private XmlSchemaUse validatedUse;
		//Compilation fields
		internal bool ParentIsSchema = false;
		private XmlSchemaAttribute referencedAttribute;
		const string xmlname = "attribute";

		public XmlSchemaAttribute()
		{
			//LAMESPEC: Docs says the default is optional.
			//Whereas the MS implementation has default None.
			form	= XmlSchemaForm.None;
			use		= XmlSchemaUse.None;
			schemaTypeName	= XmlQualifiedName.Empty;
			qualifiedName	= XmlQualifiedName.Empty;
			refName			= XmlQualifiedName.Empty;
		}

		// Properties
		#region Properties

		[DefaultValue(null)]
		[System.Xml.Serialization.XmlAttribute("default")]
		public string DefaultValue 
		{
			get{ return defaultValue;}
			set
			{ // Default Value and fixed Value are mutually exclusive
				fixedValue = null;
				defaultValue = value;
			}
		}

		[DefaultValue(null)]
		[System.Xml.Serialization.XmlAttribute("fixed")]
		public string FixedValue 
		{
			get{ return fixedValue;}
			set
			{ // Default Value and fixed Value are mutually exclusive
				defaultValue = null;
				fixedValue = value;
			}
		}

		[DefaultValue(XmlSchemaForm.None)]
		[System.Xml.Serialization.XmlAttribute("form")]
		public XmlSchemaForm Form 
		{
			get{ return form;}
			set{ form = value;}
		}

		[System.Xml.Serialization.XmlAttribute("name")]
		public string Name 
		{
			get{ return name;}
			set
			{
				name  = value;
			}
		}

		[System.Xml.Serialization.XmlAttribute("ref")]
		public XmlQualifiedName RefName 
		{
			get{ return refName;}
			set
			{
				refName = value; 
			}
		}
		
		[System.Xml.Serialization.XmlAttribute("type")]
		public XmlQualifiedName SchemaTypeName 
		{
			get{ return schemaTypeName;}
			set{ schemaTypeName = value;}
		}

		[XmlElement("simpleType",Namespace=XmlSchema.Namespace)]
		public XmlSchemaSimpleType SchemaType 
		{
			get{ return schemaType;}
			set{ schemaType = value;}
		}

		[DefaultValue(XmlSchemaUse.None)]
		[System.Xml.Serialization.XmlAttribute("use")]
		public XmlSchemaUse Use 
		{
			get{ return use;}
			set{ use = value;}
		}

		[XmlIgnore]
		public XmlQualifiedName QualifiedName 
		{
			get{ return qualifiedName;}
		}

		[XmlIgnore]
#if NET_2_0
		[Obsolete]
#endif
		public object AttributeType 
		{
			get{
				if (referencedAttribute != null)
					return referencedAttribute.AttributeType;
				else
					return attributeType;
			}
		}

#if NET_2_0
		[XmlIgnore]
		public XmlSchemaSimpleType AttributeSchemaType
		{
			get {
				if (referencedAttribute != null)
					return referencedAttribute.AttributeSchemaType;
				else
					return attributeSchemaType;
			}
		}
#endif

		// Post compilation default value (normalized)
		internal string ValidatedDefaultValue
		{
			// DefaultValue can be overriden in case of ref.
			get { return validatedDefaultValue; }
		}

		// Post compilation fixed value (normalized)
		internal string ValidatedFixedValue 
		{
			// FixedValue can be overriden in case of ref.
			get { return validatedFixedValue; }
		}
		internal XmlSchemaUse ValidatedUse
		{
			get { return validatedUse; }
		}

		#endregion

		/// <remarks>
		/// For an attribute:
		///  a) If the parent is schema 
		///		1-5		are from <xs:complexType name="topLevelAttribute"> in the Schema for Schema
		///		6-8		are from  "Constraints on XML Representations of Attribute Declarations"
		///		9-10	are from "Attribute Declaration Schema Component"
		///		11-16	are from "Constraints on Attribute Declaration Schema Components"
		///		1. ref	must be absent
		///		2. form must be absent
		///		3. use	must be absent
		///		4. name must be present and of type NCName
		///		5. *NO CHECK REQUIRED* Only simple types and annotation are allowed as content
		///		6. default and fixed must not both be present. 
		///		7. *NO CHECK REQUIRED* If default and use are both present... (Not possible since use is absent)
		///		8. type and <simpleType> must not both be present.
		///		9. Target Namespace should be schema's targetnamespace or absent
		///		10. Type Definiton coressponds to <simpletype> element, or type value, or absent
		///		11. *TO UNDERSTAND* Missing Sub-components
		///		12. value constraint must be of the same datatype as of type
		///		13. if the type definition is ID then there should be no value constraint.
		///		14. name must not be xmlns
		///		15. Targetnamespace must not be xsi. This implies the target namespace of schema can't be xsi if toplevel attributes are used.
		///		16. *Exception to rule 15* inbuilt attributes: xsi:nil, xsi:type, xsi:schemaLocation, xsi: noNamespaceSchemaLocation
		///	b) If the parent is complextype and ref is not set
		///		1. name must be present and of type NCName.
		///		2. type and <simpleType> must not both be present.
		///		3. default and fixed must not both be present. 
		///     4. If default and use are both present, use must have the ·actual value· optional.
		///		5. name must not be xmlns
		///		6. Targetnamespace must not be xsi.
		///		7. *Exception to rule 15* inbuilt attributes: xsi:nil, xsi:type, xsi:schemaLocation, xsi: noNamespaceSchemaLocation
		///		8. If form has actual value qualified or the schema's formdefault is qualified, targetnamespace
		///		   is same as schema's target namespace, otherwise absent.
		///	c) if the parent is not schema and ref is set
		///		1. name must not be present
		///		2. all of <simpleType>, form and type must be absent. 
		///		3. default and fixed must not both be present. 
		///     4. If default and use are both present, use must have the ·actual value· optional.
		/// </remarks>
		internal override int Compile(ValidationEventHandler h, XmlSchema schema)
		{
			// If this is already compiled this time, simply skip.
			if (this.IsComplied (schema.CompilationId))
				return 0;

#if NET_2_0
			if (schemaType != null)
				schemaType.Parent = this;
#endif

			errorCount = 0;
			
			if(ParentIsSchema || isRedefineChild)//a
			{
				if(RefName!= null && !RefName.IsEmpty) // a.1
					error(h,"ref must be absent in the top level <attribute>");
				
				if(Form != XmlSchemaForm.None)	// a.2
					error(h,"form must be absent in the top level <attribute>");
				
				if(Use != XmlSchemaUse.None)		// a.3
					error(h,"use must be absent in the top level <attribute>");

				targetNamespace = schema.TargetNamespace;

				CompileCommon (h, schema, true);
			}
			else // local
			{
				// Q:How to Use of AttributeFormDefault????
				// A:Global attribute cannot be defined locally
				if(RefName == null || RefName.IsEmpty)
				{
					if(form == XmlSchemaForm.Qualified || (form == XmlSchemaForm.None && schema.AttributeFormDefault == XmlSchemaForm.Qualified))
						this.targetNamespace = schema.TargetNamespace;
					else
						this.targetNamespace = "";

					CompileCommon(h, schema, true);
				}
				else
				{
					if(this.name != null)
						error(h,"name must be absent if ref is present");
					if(this.form != XmlSchemaForm.None)
						error(h,"form must be absent if ref is present");
					if(this.schemaType != null)
						error(h,"simpletype must be absent if ref is present");
					if(this.schemaTypeName != null && !this.schemaTypeName.IsEmpty)
						error(h,"type must be absent if ref is present");

					CompileCommon(h, schema, false);
				}
			}

			this.CompilationId = schema.CompilationId;
			return errorCount;
		}
		
		private void CompileCommon(ValidationEventHandler h, XmlSchema schema, bool refIsNotPresent)
		{
			if(refIsNotPresent)
			{
				if(Name == null)	//a.4, b.1, 
					error(h,"Required attribute name must be present");
				else if(!XmlSchemaUtil.CheckNCName(Name)) // a.4.2, b1.2
					error(h,"attribute name must be NCName");
				else if(Name == "xmlns") // a.14 , b5
					error(h,"attribute name must not be xmlns");
				else
					qualifiedName = new XmlQualifiedName(Name, targetNamespace);

				if(SchemaType != null)
				{
					if(SchemaTypeName != null && !SchemaTypeName.IsEmpty) // a.8
						error(h,"attribute can't have both a type and <simpleType> content");

					errorCount += SchemaType.Compile(h, schema); 
				}

				if(SchemaTypeName != null && !XmlSchemaUtil.CheckQName(SchemaTypeName))
					error(h,SchemaTypeName+" is not a valid QName");
			}
			else
			{
				if(RefName == null || RefName.IsEmpty) 
					throw new InvalidOperationException ("Error: Should Never Happen. refname must be present");
				else
					qualifiedName = RefName;
			}

			if(schema.TargetNamespace == XmlSchema.InstanceNamespace && Name != "nil" && Name != "type" 
				&& Name != "schemaLocation" && Name != "noNamespaceSchemaLocation") // a.15, a.16
				error(h,"targetNamespace can't be " + XmlSchema.InstanceNamespace);

			if(DefaultValue != null && FixedValue != null) // a.6, b.3, c.3
				error(h,"default and fixed must not both be present in an Attribute");

			if(DefaultValue != null && Use != XmlSchemaUse.None && Use != XmlSchemaUse.Optional)
				error(h,"if default is present, use must be optional");

			XmlSchemaUtil.CompileID(Id, this, schema.IDCollection, h);
		}

		/// <summary>
		/// Schema Component: 
		///			QName, SimpleType, Scope, Default|Fixed, annotation
		/// </summary>
		internal override int Validate(ValidationEventHandler h, XmlSchema schema)
		{
			if(IsValidated (schema.ValidationId))
				return errorCount;

			// -- Attribute Declaration Schema Component --
			// {name}, {target namespace} -> QualifiedName. Already Compile()d.
			// {type definition} -> attributeType. From SchemaType or SchemaTypeName.
			// {scope} -> ParentIsSchema | isRedefineChild.
			// {value constraint} -> ValidatedFixedValue, ValidatedDefaultValue.
			// {annotation}
			// -- Attribute Use Schema Component --
			// {required}
			// {attribute declaration}
			// {value constraint}

			// First, fill type information for type reference
			if (SchemaType != null) {
				SchemaType.Validate (h, schema);
				attributeType = SchemaType;
			}
			else if (SchemaTypeName != null && SchemaTypeName != XmlQualifiedName.Empty)
			{
				// If type is null, then it is missing sub components .
				XmlSchemaType type = schema.SchemaTypes [SchemaTypeName] as XmlSchemaType;
				if (type is XmlSchemaComplexType)
					error(h,"An attribute can't have complexType Content");
				else if (type != null) {	// simple type
					errorCount += type.Validate (h, schema);
					attributeType = type;
				}
				else if (SchemaTypeName == XmlSchemaComplexType.AnyTypeName)
					attributeType = XmlSchemaComplexType.AnyType;
				else if (XmlSchemaUtil.IsBuiltInDatatypeName (SchemaTypeName)) {
					attributeType = XmlSchemaDatatype.FromName (SchemaTypeName);
					if (attributeType == null)
						error (h, "Invalid xml schema namespace datatype was specified.");
				}
				// otherwise, it might be missing sub components.
				else if (!schema.IsNamespaceAbsent (SchemaTypeName.Namespace))
					error (h, "Referenced schema type " + SchemaTypeName + " was not found in the corresponding schema.");
			}

			// Then, fill type information for the type references for the referencing attributes
			if (RefName != null && RefName != XmlQualifiedName.Empty)
			{
				referencedAttribute = schema.Attributes [RefName] as XmlSchemaAttribute;
				// If el is null, then it is missing sub components .
				if (referencedAttribute != null)
					errorCount += referencedAttribute.Validate (h, schema);
				// otherwise, it might be missing sub components.
				else if (!schema.IsNamespaceAbsent (RefName.Namespace))
					error (h, "Referenced attribute " + RefName + " was not found in the corresponding schema.");
			}

			if (attributeType == null)
				attributeType = XmlSchemaSimpleType.AnySimpleType;

			// Validate {value constraints}
			if (defaultValue != null || fixedValue != null) {
				XmlSchemaDatatype datatype = attributeType as XmlSchemaDatatype;
				if (datatype == null)
					datatype = ((XmlSchemaSimpleType) attributeType).Datatype;
				if (datatype.TokenizedType == XmlTokenizedType.QName)
					error (h, "By the defection of the W3C XML Schema specification, it is impossible to supply QName default or fixed values.");
				else {
					try {
						if (defaultValue != null) {
							validatedDefaultValue = datatype.Normalize (defaultValue);
							datatype.ParseValue (validatedDefaultValue, null, null);
						}
					} catch (Exception ex) {
						// FIXME: This is not a good way to handle exception.
						error (h, "The Attribute's default value is invalid with its type definition.", ex);
					}
					try {
						if (fixedValue != null) {
							validatedFixedValue = datatype.Normalize (fixedValue);
							datatype.ParseValue (validatedFixedValue, null, null);
						}
					} catch (Exception ex) {
						// FIXME: This is not a good way to handle exception.
						error (h, "The Attribute's fixed value is invalid with its type definition.", ex);
					}
				}
			}
			if (Use == XmlSchemaUse.None)
				validatedUse = XmlSchemaUse.Optional;
			else
				validatedUse = Use;

#if NET_2_0
			attributeSchemaType = attributeType as XmlSchemaSimpleType;
			if (attributeSchemaType == null)
				attributeSchemaType = XmlSchemaType.GetBuiltInSimpleType (((XmlSchemaDatatype) attributeType).TypeCode);
#endif

			ValidationId = schema.ValidationId;
			return errorCount;
		}

		//<attribute
		//  default = string
		//  fixed = string
		//  form = (qualified | unqualified)
		//  id = ID
		//  name = NCName
		//  ref = QName
		//  type = QName
		//  use = (optional | prohibited | required) : optional
		//  {any attributes with non-schema namespace . . .}>
		//  Content: (annotation?, (simpleType?))
		//</attribute>
		internal static XmlSchemaAttribute Read(XmlSchemaReader reader, ValidationEventHandler h)
		{
			XmlSchemaAttribute attribute = new XmlSchemaAttribute();
			reader.MoveToElement();

			if(reader.NamespaceURI != XmlSchema.Namespace || reader.LocalName != xmlname)
			{
				error(h,"Should not happen :1: XmlSchemaAttribute.Read, name="+reader.Name,null);
				reader.SkipToEnd();
				return null;
			}

			attribute.LineNumber = reader.LineNumber;
			attribute.LinePosition = reader.LinePosition;
			attribute.SourceUri = reader.BaseURI;

			while(reader.MoveToNextAttribute())
			{
				if(reader.Name == "default")
				{
					attribute.defaultValue = reader.Value;
				}
				else if(reader.Name == "fixed")
				{
					attribute.fixedValue = reader.Value;
				}
				else if(reader.Name == "form")
				{
					Exception innerex;
					attribute.form = XmlSchemaUtil.ReadFormAttribute(reader,out innerex);
					if(innerex != null)
						error(h, reader.Value + " is not a valid value for form attribute", innerex);
				}
				else if(reader.Name == "id")
				{
					attribute.Id = reader.Value;
				}
				else if(reader.Name == "name")
				{
					attribute.name = reader.Value;
				}
				else if(reader.Name == "ref")
				{
					Exception innerex;
					attribute.refName = XmlSchemaUtil.ReadQNameAttribute(reader,out innerex);
					if(innerex != null)
						error(h, reader.Value + " is not a valid value for ref attribute",innerex);
				}
				else if(reader.Name == "type")
				{
					Exception innerex;
					attribute.schemaTypeName = XmlSchemaUtil.ReadQNameAttribute(reader,out innerex);
					if(innerex != null)
						error(h, reader.Value + " is not a valid value for type attribute",innerex);
				}
				else if(reader.Name == "use")
				{
					Exception innerex;
					attribute.use = XmlSchemaUtil.ReadUseAttribute(reader,out innerex);
					if(innerex != null)
						error(h, reader.Value + " is not a valid value for use attribute", innerex);
				}
				else if((reader.NamespaceURI == "" && reader.Name != "xmlns") || reader.NamespaceURI == XmlSchema.Namespace)
				{
					error(h,reader.Name + " is not a valid attribute for attribute",null);
				}
				else
				{
					XmlSchemaUtil.ReadUnhandledAttribute(reader,attribute);
				}
			}
			
			reader.MoveToElement();
			if(reader.IsEmptyElement)
				return attribute;

			//  Content: (annotation?, (simpleType?))
			int level = 1;
			while(reader.ReadNextElement())
			{
				if(reader.NodeType == XmlNodeType.EndElement)
				{
					if(reader.LocalName != xmlname)
						error(h,"Should not happen :2: XmlSchemaAttribute.Read, name="+reader.Name,null);
					break;
				}
				if(level <= 1 && reader.LocalName == "annotation")
				{
					level = 2; //Only one annotation
					XmlSchemaAnnotation annotation = XmlSchemaAnnotation.Read(reader,h);
					if(annotation != null)
						attribute.Annotation = annotation;
					continue;
				}
				if(level <=2 && reader.LocalName == "simpleType")
				{
					level = 3;
					XmlSchemaSimpleType stype = XmlSchemaSimpleType.Read(reader,h);
					if(stype != null)
						attribute.schemaType = stype;
					continue;
				}
				reader.RaiseInvalidElementError();
			}
			return attribute;
		}
		
	}
}
