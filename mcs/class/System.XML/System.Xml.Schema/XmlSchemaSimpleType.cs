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
		internal bool islocal = false;
		private  bool errorOccured = false;

		public XmlSchemaSimpleType()
		{
		}

		[XmlElement("restriction",typeof(XmlSchemaSimpleTypeRestriction),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("list",typeof(XmlSchemaSimpleTypeList),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("union",typeof(XmlSchemaSimpleTypeUnion),Namespace="http://www.w3.org/2001/XMLSchema")]
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
		internal bool Compile(ValidationEventHandler h, XmlSchemaInfo info)
		{
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
					this.qName = new XmlQualifiedName(this.Name,info.targetNS);
				
				XmlSchemaDerivationMethod finaltmp;

				// The possible values of finalDefault on schema are #all | List of (extension | restriction)
				// Of these, the only possible values for us are #all | restriction.
				if(this.Final != XmlSchemaDerivationMethod.None)
					finaltmp = this.Final;
				else
					finaltmp = info.finalDefault;// & XmlSchemaDerivationMethod.Restriction;
				
				switch(finaltmp) //b.3, b.4, b.5
				{
					case XmlSchemaDerivationMethod.Substitution:
						error(h,"substition is not a valid value for final in a simpletype");
						break;
					case XmlSchemaDerivationMethod.Extension:
						error(h,"extension is not a valid value for final in a simpletype");
						break;
					case XmlSchemaDerivationMethod.Union:
						error(h,"union is not a valid value for final in simpletype");
						break;
					case XmlSchemaDerivationMethod.Empty:
						this.finalResolved = XmlSchemaDerivationMethod.Empty;
						break;
					case XmlSchemaDerivationMethod.List:
						this.finalResolved = XmlSchemaDerivationMethod.List;
						break;
					case XmlSchemaDerivationMethod.Restriction:
						this.finalResolved = XmlSchemaDerivationMethod.Restriction;
						break;
					case XmlSchemaDerivationMethod.All:
						this.finalResolved = XmlSchemaDerivationMethod.All;
						break;
					case XmlSchemaDerivationMethod.None: // Default is empty
						this.finalResolved = XmlSchemaDerivationMethod.Empty;
						break;
					default:
						error(h,"simpletype can't have more than one value for final");
						break;
				}
			}
			if(!XmlSchemaUtil.CheckID(this.Id))
				error(h,"id must be a valid ID");

			if(this.Content == null) //a.3,b.2
				error(h,"Content is required in a simpletype");
			else if(Content is XmlSchemaSimpleTypeRestriction)
			{
				((XmlSchemaSimpleTypeRestriction)Content).Compile(h,info);
			}
			else if(Content is XmlSchemaSimpleTypeList)
			{
				((XmlSchemaSimpleTypeList)Content).Compile(h,info);
			}
			else if(Content is XmlSchemaSimpleTypeUnion)
			{
				((XmlSchemaSimpleTypeUnion)Content).Compile(h,info);
			}
			return !errorOccured;
		}
		
		[MonoTODO]
		internal bool Validate(ValidationEventHandler h)
		{
			return false;
		}
		
		internal void error(ValidationEventHandler handle,string message)
		{
			this.errorOccured = true;
			ValidationHandler.RaiseValidationError(handle,this,message);
		}
	}
}
