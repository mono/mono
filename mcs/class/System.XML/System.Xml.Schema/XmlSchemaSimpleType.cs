//
// System.Xml.Schema.XmlSchemaSimpleType.cs
//
// Author:
//	Dwivedi, Ajay kumar  Adwiv@Yahoo.com
//	Atsushi Enomoto  ginga@kit.hi-ho.ne.jp
//
using System;
using System.Xml.Serialization;
using System.Xml;
using Mono.Xml.Schema;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaSimpleType.
	/// </summary>
	public class XmlSchemaSimpleType : XmlSchemaType
	{
		private static string xmlname = "simpleType";
		private static XmlSchemaSimpleType schemaLocationType;

		private XmlSchemaSimpleTypeContent content;
		//compilation vars
		internal bool islocal = true; // Assuming local means we have to specify islocal=false only in XmlSchema
		private bool recursed;
		private XmlSchemaDerivationMethod variety;

		static XmlSchemaSimpleType ()
		{
			// This is not used in the meantime.
			XmlSchemaSimpleType st = new XmlSchemaSimpleType ();
			XmlSchemaSimpleTypeList list = new XmlSchemaSimpleTypeList ();
			list.ItemTypeName = new XmlQualifiedName ("anyURI", XmlSchema.Namespace);
			st.Content = list;
			st.BaseXmlSchemaTypeInternal = null;
			st.variety = XmlSchemaDerivationMethod.List;
			schemaLocationType = st;
		}

		internal static XsdAnySimpleType AnySimpleType {
			get { return XsdAnySimpleType.Instance; }
		}

		internal static XmlSchemaSimpleType SchemaLocationType {
			get { return schemaLocationType; }
		}

		public XmlSchemaSimpleType()
		{
		}

		[XmlElement("restriction",typeof(XmlSchemaSimpleTypeRestriction),Namespace=XmlSchema.Namespace)]
		[XmlElement("list",typeof(XmlSchemaSimpleTypeList),Namespace=XmlSchema.Namespace)]
		[XmlElement("union",typeof(XmlSchemaSimpleTypeUnion),Namespace=XmlSchema.Namespace)]
		public XmlSchemaSimpleTypeContent Content
		{
			get{ return  content; } 
			set{ content = value; }
		}

		internal XmlSchemaDerivationMethod Variety
		{
			get{ return variety; }
		}

		/// <remarks>
		/// For a simple Type:
		///		1. Content must be present
		///		2. id if present, must have be a valid ID
		///		a) If the simpletype is local
		///			1-	are from <xs:complexType name="simpleType"> and <xs:complexType name="localSimpleType">
		///			1. name  is prohibited
		///			2. final is prohibited
		///		b) If the simpletype is toplevel
		///			1-  are from <xs:complexType name="simpleType"> and <xs:complexType name="topLevelSimpleType">
		///			1. name is required, type must be NCName
		///			2. Content is required
		///			3. final can have values : #all | (list | union | restriction)
		///			4. If final is set, finalResolved is same as final (but within the values of b.3)
		///			5. If final is not set, the finalDefault of the schema (ie. only #all and restriction)
		///			6. Base type is:
		///				4.1 If restriction is chosen,the base type of restriction or elements
		///				4.2 otherwise simple ur-type
		/// </remarks>
		internal override int Compile(ValidationEventHandler h, XmlSchema schema)
		{
			// If this is already compiled this time, simply skip.
			if (this.IsComplied (schema.CompilationId))
				return 0;

			errorCount = 0;

			if(this.islocal) // a
			{
				if(this.Name != null) // a.1
					error(h,"Name is prohibited in a local simpletype");
				else
					this.QNameInternal = new XmlQualifiedName(this.Name,schema.TargetNamespace);
				if(this.Final != XmlSchemaDerivationMethod.None) //a.2
					error(h,"Final is prohibited in a local simpletype");
			}
			else //b
			{
				if(this.Name == null) //b.1
					error(h,"Name is required in top level simpletype");
				else if(!XmlSchemaUtil.CheckNCName(this.Name)) // b.1.2
					error(h,"name attribute of a simpleType must be NCName");
				else
					this.QNameInternal = new XmlQualifiedName(this.Name,schema.TargetNamespace);
				
				//NOTE: Although the FinalResolved can be Empty, it is not a valid value for Final
				//DEVIATION: If an error occurs, the finaldefault is always consulted. This deviates
				//			 from the way MS implementation works.
				switch(this.Final) //b.3, b.4
				{
					case XmlSchemaDerivationMethod.All:
						this.finalResolved = XmlSchemaDerivationMethod.All;
						break;
					case XmlSchemaDerivationMethod.List:
					case XmlSchemaDerivationMethod.Union:
					case XmlSchemaDerivationMethod.Restriction:
						this.finalResolved = Final;
						break;
					default:
						error(h,"The value of final attribute is not valid for simpleType");
						goto case XmlSchemaDerivationMethod.None;
						// use assignment from finaldefault on schema.
					case XmlSchemaDerivationMethod.None: // b.5
						XmlSchemaDerivationMethod flags = 
							(XmlSchemaDerivationMethod.Restriction | XmlSchemaDerivationMethod.List |
							XmlSchemaDerivationMethod.Extension | XmlSchemaDerivationMethod.Union );
						switch (schema.FinalDefault) {
						case XmlSchemaDerivationMethod.All:
							finalResolved = XmlSchemaDerivationMethod.All;
							break;
						case XmlSchemaDerivationMethod.None:
							finalResolved = XmlSchemaDerivationMethod.Empty;
							break;
						default:
							finalResolved = schema.FinalDefault & flags;
							break;
						}
						break;
				}
			}

			XmlSchemaUtil.CompileID(Id,this,schema.IDCollection,h);

			if (Content != null)
				Content.OwnerType = this;

			if(this.Content == null) //a.3,b.2
				error(h,"Content is required in a simpletype");
			else if(Content is XmlSchemaSimpleTypeRestriction)
			{
				this.resolvedDerivedBy = XmlSchemaDerivationMethod.Restriction;
				errorCount += ((XmlSchemaSimpleTypeRestriction)Content).Compile(h,schema);
			}
			else if(Content is XmlSchemaSimpleTypeList)
			{
				this.resolvedDerivedBy = XmlSchemaDerivationMethod.List;
				errorCount += ((XmlSchemaSimpleTypeList)Content).Compile(h,schema);
			}
			else if(Content is XmlSchemaSimpleTypeUnion)
			{
				this.resolvedDerivedBy = XmlSchemaDerivationMethod.Union;
				errorCount += ((XmlSchemaSimpleTypeUnion)Content).Compile(h,schema);
			}

			this.CompilationId = schema.CompilationId;
			return errorCount;
		}

		internal void CollectBaseType (ValidationEventHandler h, XmlSchema schema)
		{
			if (Content is XmlSchemaSimpleTypeRestriction) {
				object o = ((XmlSchemaSimpleTypeRestriction) Content).GetActualType (h, schema, false);
				BaseXmlSchemaTypeInternal = o as XmlSchemaSimpleType;
				DatatypeInternal = o as XmlSchemaDatatype;
			}
			// otherwise, actualBaseSchemaType is null
			else
				DatatypeInternal = XmlSchemaSimpleType.AnySimpleType;
		}
		
		internal override int Validate(ValidationEventHandler h, XmlSchema schema)
		{
			// 3.14.6 Properties Correct.
			// 
			// 1. Post Compilation Properties
			// {name}, {target namespace} => QNameInternal. Already Compile()d.
			// {base type definition} => baseSchemaTypeInternal
			// {final} => finalResolved. Already Compile()d.
			// {variety} => resolvedDerivedBy. Already Compile()d.
			//
			// 2. Should be checked by "recursed" field.

			if(IsValidated (schema.ValidationId))
				return errorCount;

			if (recursed) {
				error (h, "Circular type reference was found.");
				return errorCount;
			}
			recursed = true;

			CollectBaseType (h, schema);

			if (content != null)
				errorCount += content.Validate (h, schema);

/*
			// BaseSchemaType property
			BaseXmlSchemaTypeInternal = content.ActualBaseSchemaType as XmlSchemaType;
			if (this.BaseXmlSchemaTypeInternal == null)
				this.DatatypeInternal = content.ActualBaseSchemaType as XmlSchemaDatatype;
*/

			// Datatype property
			XmlSchemaSimpleType simple = BaseXmlSchemaType as XmlSchemaSimpleType;
			if (simple != null)
				this.DatatypeInternal = simple.Datatype;
//			else
//				DatatypeInternal = BaseSchemaType as XmlSchemaDatatype;

			// 3.
			XmlSchemaSimpleType baseSType = BaseXmlSchemaType as XmlSchemaSimpleType;
			if (baseSType != null) {
				if ((baseSType.FinalResolved & this.resolvedDerivedBy) != 0)
					error (h, "Specified derivation is prohibited by the base simple type.");
			}

			// {variety}
			if (this.resolvedDerivedBy == XmlSchemaDerivationMethod.Restriction &&
				baseSType != null)
				this.variety = baseSType.Variety;
			else
				this.variety = this.resolvedDerivedBy;

			// 3.14.6 Derivation Valid (Restriction, Simple)
			XmlSchemaSimpleTypeRestriction r = Content as XmlSchemaSimpleTypeRestriction;
			object baseType = BaseXmlSchemaType != null ? (object) BaseXmlSchemaType : Datatype;
			if (r != null)
				ValidateDerivationValid (baseType, r.Facets, h, schema);

			// TODO: describe which validation term this belongs to.
			XmlSchemaSimpleTypeList l = Content as XmlSchemaSimpleTypeList;
			if (l != null) {
				XmlSchemaSimpleType itemSimpleType = l.ValidatedListItemType as XmlSchemaSimpleType;
				if (itemSimpleType != null && itemSimpleType.Content is XmlSchemaSimpleTypeList)
					error (h, "List type must not be derived from another list type.");
			}

			recursed = false;
			ValidationId = schema.ValidationId;
			return errorCount;
		}

		// 3.14.6 Derivation Valid (RestrictionSimple)
		internal void ValidateDerivationValid (object baseType, XmlSchemaObjectCollection facets,
			ValidationEventHandler h, XmlSchema schema)
		{
			// TODO
			XmlSchemaSimpleType baseSimpleType = baseType as XmlSchemaSimpleType;
			switch (this.Variety) {
			// 1. atomic type
			case XmlSchemaDerivationMethod.Restriction:
				// 1.1
				if (baseSimpleType != null && baseSimpleType.resolvedDerivedBy != XmlSchemaDerivationMethod.Restriction)
					error (h, "Base schema type is not either atomic type or primitive type.");
				// 1.2
				if (baseSimpleType != null && 
					(baseSimpleType.FinalResolved & XmlSchemaDerivationMethod.Restriction) != 0)
					error (h, "Derivation by restriction is prohibited by the base simple type.");
				// TODO: 1.3 facet restriction valid.
				break;
			case XmlSchemaDerivationMethod.List:
				XmlSchemaSimpleTypeList thisList = Content as XmlSchemaSimpleTypeList;
				/*
				// 2.1 item list type not allowed
				if (baseSimpleType != null && baseSimpleType.resolvedDerivedBy == XmlSchemaDerivationMethod.List)
					error (h, "Base list schema type is not allowed.");
				XmlSchemaSimpleTypeUnion baseUnion = baseSimpleType.Content as XmlSchemaSimpleTypeUnion;
				if (baseUnion != null) {
					bool errorFound = false;
					foreach (object memberType in baseUnion.ValidatedTypes) {
						XmlSchemaSimpleType memberST = memberType as XmlSchemaSimpleType;
						if (memberST != null && memberST.resolvedDerivedBy == XmlSchemaDerivationMethod.List)
							errorFound = true;
					}
					if (errorFound)
						error (h, "Base union schema type should not contain list types.");
				}
				*/
				// 2.2 facets limited
				if (facets != null)
					foreach (XmlSchemaFacet facet in facets) {
						if (facet is XmlSchemaLengthFacet ||
							facet is XmlSchemaMaxLengthFacet ||
							facet is XmlSchemaMinLengthFacet ||
							facet is XmlSchemaEnumerationFacet ||
							facet is XmlSchemaPatternFacet)
							continue;
						else
							error (h, "Not allowed facet was found on this simple type which derives list type.");
					}
				break;
			case XmlSchemaDerivationMethod.Union:
				// 3.1

				// 3.2
				if (facets != null)
					foreach (XmlSchemaFacet facet in facets) {
						if (facet is XmlSchemaEnumerationFacet ||
							facet is XmlSchemaPatternFacet)
							continue;
						else
							error (h, "Not allowed facet was found on this simple type which derives list type.");
					}
				break;
			}
		}

		// 3.14.6 Type Derivation OK (Simple)
		internal bool ValidateTypeDerivationOK (object baseType,
			ValidationEventHandler h, XmlSchema schema, bool raiseError)
		{
			// 1
			// Note that anyType should also be allowed as anySimpleType.
			if (this == baseType || baseType == XmlSchemaSimpleType.AnySimpleType ||
				baseType == XmlSchemaComplexType.AnyType)
				return true;

			// 2.1
			XmlSchemaSimpleType baseSimpleType = baseType as XmlSchemaSimpleType;
			if (baseSimpleType != null && 
				(baseSimpleType.FinalResolved & resolvedDerivedBy) != 0) {
				if (raiseError)
					error (h, "Specified derivation is prohibited by the base type.");
				return false;
			}

			// 2.2.1
			if (BaseXmlSchemaType == baseType || Datatype == baseType)
				return true;

			// 2.2.2
			XmlSchemaSimpleType thisBaseSimpleType = BaseXmlSchemaType as XmlSchemaSimpleType;
			if (thisBaseSimpleType != null) {
				if (thisBaseSimpleType.ValidateTypeDerivationOK (baseType, h, schema, false))
					return true;
			}

			// 2.2.3
			switch (Variety) {
			case XmlSchemaDerivationMethod.Union:
			case XmlSchemaDerivationMethod.List:
				if (baseType == XmlSchemaSimpleType.AnySimpleType)
					return true;
				break;
			}

			// 2.2.4 validly derived from one of the union member type.
			if (baseSimpleType != null && baseSimpleType.Variety == XmlSchemaDerivationMethod.Union) {
				foreach (object memberType in ((XmlSchemaSimpleTypeUnion) baseSimpleType.Content).ValidatedTypes)
					if (this.ValidateTypeDerivationOK (memberType, h, schema, false))
						return true;
			}

			if (raiseError)
				error(h, "Invalid simple type derivation was found.");
			return false;
		}

		internal string Normalize (string s, XmlNameTable nt, XmlNamespaceManager nsmgr)
		{
			return Content.Normalize (s, nt, nsmgr);
		}

		//<simpleType 
		//  final = (#all | (list | union | restriction)) 
		//  id = ID 
		//  name = NCName 
		//  {any attributes with non-schema namespace . . .}>
		//  Content: (annotation?, (restriction | list | union))
		//</simpleType>
		internal static XmlSchemaSimpleType Read(XmlSchemaReader reader, ValidationEventHandler h)
		{
			XmlSchemaSimpleType stype = new XmlSchemaSimpleType();
			reader.MoveToElement();

			if(reader.NamespaceURI != XmlSchema.Namespace || reader.LocalName != xmlname)
			{
				error(h,"Should not happen :1: XmlSchemaGroup.Read, name="+reader.Name,null);
				reader.Skip();
				return null;
			}

			stype.LineNumber = reader.LineNumber;
			stype.LinePosition = reader.LinePosition;
			stype.SourceUri = reader.BaseURI;

			while(reader.MoveToNextAttribute())
			{
				if(reader.Name == "final")
				{
					Exception innerex;
					stype.Final = XmlSchemaUtil.ReadDerivationAttribute(reader, out innerex, "final",
						XmlSchemaUtil.FinalAllowed);
					if(innerex != null)
						error(h, "some invalid values not a valid value for final", innerex);
				}
				else if(reader.Name == "id")
				{
					stype.Id = reader.Value;
				}
				else if(reader.Name == "name")
				{
					stype.Name = reader.Value;
				}
				else if((reader.NamespaceURI == "" && reader.Name != "xmlns") || reader.NamespaceURI == XmlSchema.Namespace)
				{
					error(h,reader.Name + " is not a valid attribute for simpleType",null);
				}
				else
				{
					XmlSchemaUtil.ReadUnhandledAttribute(reader,stype);
				}
			}
			
			reader.MoveToElement();
			if(reader.IsEmptyElement)
				return stype;

			//	Content: (annotation?, (restriction | list | union))
			int level = 1;
			while(reader.ReadNextElement())
			{
				if(reader.NodeType == XmlNodeType.EndElement)
				{
					if(reader.LocalName != xmlname)
						error(h,"Should not happen :2: XmlSchemaSimpleType.Read, name="+reader.Name,null);
					break;
				}
				if(level <= 1 && reader.LocalName == "annotation")
				{
					level = 2; //Only one annotation
					XmlSchemaAnnotation annotation = XmlSchemaAnnotation.Read(reader,h);
					if(annotation != null)
						stype.Annotation = annotation;
					continue;
				}
				if(level <= 2)
				{
					if(reader.LocalName == "restriction")
					{
						level = 3;
						XmlSchemaSimpleTypeRestriction restriction = XmlSchemaSimpleTypeRestriction.Read(reader,h);
						if(restriction != null)
							stype.content = restriction;
						continue;
					}
					if(reader.LocalName == "list")
					{
						level = 3;
						XmlSchemaSimpleTypeList list = XmlSchemaSimpleTypeList.Read(reader,h);
						if(list != null)
							stype.content = list;
						continue;
					}
					if(reader.LocalName == "union")
					{
						level = 3;
						XmlSchemaSimpleTypeUnion union = XmlSchemaSimpleTypeUnion.Read(reader,h);
						if(union != null)
							stype.content = union;
						continue;
					}
				}
				reader.RaiseInvalidElementError();
			}
			return stype;
		}


	}
}
