// 
// System.Data/DataSet.cs
//
// Author:
//   Stuart Caborn <stuart.caborn@virgin.net>
//
// (C) Stuart Caborn 2002

namespace System.Data 
{
///<summary>
///Constants class to hold XmlSerialisation
///strings
///</summary>
internal class XmlConstants
{
	///<summary>The namespace prefix for the xml schema namespace</summary>
	public const string SchemaPrefix = "xs";
	
	///<summary>
	/// The w3 XmlSchema namespace
	/// </summary>
	public const string SchemaNamespace = "http://www.w3.org/2001/XMLSchema";
	
	
	//xs elements and values 
	//TODO - these must exist somwhere else???
	public const string SchemaElement = "schema";
	public const string AttributeFormDefault = "attributeFormDefault";
	public const string ElementFormDefault = "elementFormDefault";
	public const string Qualified = "qualified";
	public const string Unqualified = "unqualified";
	public const string Element = "element";
	public const string Choice = "choice";
	public const string ComplexType = "complexType";
	public const string Sequence = "sequence";
	public const string MaxOccurs = "maxOccurs";
	public const string MinOccurs = "minOccurs";
	public const string Unbounded = "unbounded";
	public const string Name = "name";
	public const string Type = "type";
	public const string Id = "id";
	public const string TargetNamespace = "targetNamespace";
	public const string Form = "form";
	public const string Attribute = "attribute";
	
	//ms schema objects	
	public const string MsdataPrefix = "msdata";	
	public const string MsdataNamespace = "urn:schemas-microsoft-com:xml-msdata";
	public const string TnsPrefix = "mstns";
	public const string IsDataSet = "IsDataSet";
	public const string Locale = "Locale";
	public const string Ordinal = "Ordinal";
}

}
