// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml.Serialization;
using System.Xml;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaSimpleType.
	/// </summary>
	public class XmlSchemaSimpleType : XmlSchemaType
	{
		private XmlSchemaSimpleTypeContent content;
		//compilation vars
		internal bool islocal = true; // Assuming local means we have to specify islocal=false only in XmlSchema
		private static string xmlname = "simpleType";

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
		[MonoTODO]
		internal int Compile(ValidationEventHandler h, XmlSchema schema)
		{
			// If this is already compiled this time, simply skip.
			if (this.IsComplied (schema.CompilationId))
				return 0;

			errorCount = 0;

			if(this.islocal) // a
			{
				if(this.Name != null) // a.1
					error(h,"Name is prohibited in a local simpletype");
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
						this.finalResolved = XmlSchemaDerivationMethod.List;
						break;
					case XmlSchemaDerivationMethod.Union:
						this.finalResolved = XmlSchemaDerivationMethod.Union;
						break;
					case XmlSchemaDerivationMethod.Restriction:
						this.finalResolved = XmlSchemaDerivationMethod.Restriction;
						break;
					default:
						error(h,"The value of final attribute is not valid for simpleType");
						goto case XmlSchemaDerivationMethod.None;
						// use assignment from finaldefault on schema.
					case XmlSchemaDerivationMethod.None: // b.5
						XmlSchemaDerivationMethod flags = 
							(XmlSchemaDerivationMethod.Restriction | XmlSchemaDerivationMethod.List |
							XmlSchemaDerivationMethod.Extension | XmlSchemaDerivationMethod.Union );
						if(schema.FinalDefault == XmlSchemaDerivationMethod.All)
							finalResolved = XmlSchemaDerivationMethod.All;
						else if(schema.FinalDefault == XmlSchemaDerivationMethod.None)
							finalResolved = flags;
						else 
							finalResolved = schema.FinalDefault & flags;
						break;
				}
			}

			XmlSchemaUtil.CompileID(Id,this,schema.IDCollection,h);

			if(this.Content == null) //a.3,b.2
				error(h,"Content is required in a simpletype");
			else if(Content is XmlSchemaSimpleTypeRestriction)
			{
				errorCount += ((XmlSchemaSimpleTypeRestriction)Content).Compile(h,schema);
			}
			else if(Content is XmlSchemaSimpleTypeList)
			{
				errorCount += ((XmlSchemaSimpleTypeList)Content).Compile(h,schema);
			}
			else if(Content is XmlSchemaSimpleTypeUnion)
			{
				errorCount += ((XmlSchemaSimpleTypeUnion)Content).Compile(h,schema);
			}
			this.CompilationId = schema.CompilationId;
			return errorCount;
		}
		
		[MonoTODO]
		internal int Validate(ValidationEventHandler h, XmlSchema schema)
		{
			if(isCompiled)
				return errorCount;

			return errorCount;
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
					stype.Final = XmlSchemaUtil.ReadDerivationAttribute(reader, out innerex, "final");
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
