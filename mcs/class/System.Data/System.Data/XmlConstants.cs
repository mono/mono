// 
// System.Data/DataSet.cs
//
// Author:
//   Stuart Caborn <stuart.caborn@virgin.net>
//
// (C) Stuart Caborn 2002

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System.Xml;
using System.Xml.Schema;

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
	public const string SimpleType = "simpleType";
	public const string Restriction = "restriction";
	public const string MaxLength = "maxLength";
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
	public const string Default = "default";
	public const string Caption = "Caption";
	public const string Base = "base";
	public const string Value = "value";
	public const string DataType = "DataType";
	public const string AutoIncrement = "AutoIncrement";
	public const string AutoIncrementSeed = "AutoIncrementSeed";

	//ms schema objects	
	public const string MsdataPrefix = "msdata";	
	public const string MsdataNamespace = "urn:schemas-microsoft-com:xml-msdata";
	public const string MspropPrefix = "msprop";	
	public const string MspropNamespace = "urn:schemas-microsoft-com:xml-msprop";
	public const string DiffgrPrefix = "diffgr";
	public const string DiffgrNamespace = "urn:schemas-microsoft-com:xml-diffgram-v1";
	public const string TnsPrefix = "mstns";
	public const string IsDataSet = "IsDataSet";
	public const string Locale = "Locale";
	public const string Ordinal = "Ordinal";
	public const string IsNested = "IsNested";
	public const string RelationName = "RelationName";
	public const string ConstraintName = "ConstraintName";
	public const string PrimaryKey = "PrimaryKey";
	public const string ColumnName = "ColumnName";

	public static XmlQualifiedName QnString = new XmlQualifiedName ("string", XmlSchema.Namespace);
	public static XmlQualifiedName QnShort = new XmlQualifiedName ("short", XmlSchema.Namespace);
	public static XmlQualifiedName QnInt = new XmlQualifiedName ("int", XmlSchema.Namespace);
	public static XmlQualifiedName QnLong = new XmlQualifiedName ("long", XmlSchema.Namespace);
	public static XmlQualifiedName QnBoolean = new XmlQualifiedName ("boolean", XmlSchema.Namespace);
	public static XmlQualifiedName QnUnsignedByte = new XmlQualifiedName ("unsignedByte", XmlSchema.Namespace);
	public static XmlQualifiedName QnChar = new XmlQualifiedName ("char", XmlSchema.Namespace);
	public static XmlQualifiedName QnDateTime = new XmlQualifiedName ("dateTime", XmlSchema.Namespace);
	public static XmlQualifiedName QnDecimal = new XmlQualifiedName ("decimal", XmlSchema.Namespace);
	public static XmlQualifiedName QnDouble = new XmlQualifiedName ("double", XmlSchema.Namespace);
	public static XmlQualifiedName QnSbyte = new XmlQualifiedName ("byte", XmlSchema.Namespace);
	public static XmlQualifiedName QnFloat = new XmlQualifiedName ("float", XmlSchema.Namespace);
	public static XmlQualifiedName QnDuration = new XmlQualifiedName ("duration", XmlSchema.Namespace);
	public static XmlQualifiedName QnUsignedShort = new XmlQualifiedName ("usignedShort", XmlSchema.Namespace);
	public static XmlQualifiedName QnUnsignedInt = new XmlQualifiedName ("unsignedInt", XmlSchema.Namespace);
	public static XmlQualifiedName QnUnsignedLong = new XmlQualifiedName ("unsignedLong", XmlSchema.Namespace);
	public static XmlQualifiedName QnUri = new XmlQualifiedName ("anyURI", XmlSchema.Namespace);
	public static XmlQualifiedName QnBase64Binary = new XmlQualifiedName ("base64Binary", XmlSchema.Namespace);
	public static XmlQualifiedName QnXmlQualifiedName = new XmlQualifiedName ("QName", XmlSchema.Namespace);
}

}
