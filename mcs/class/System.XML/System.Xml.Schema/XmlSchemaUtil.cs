
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
using System.Collections;
using System.Text;
using Mono.Xml;
using Mono.Xml.Schema;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	///  All Methods in this class should use XmlConvert. Some Methods are not present in the
	///  MS Implementation. We should provide them.
	/// </summary>
	internal class XmlSchemaUtil
	{
		static XmlSchemaUtil ()
		{
			FinalAllowed = XmlSchemaDerivationMethod.Restriction | 
				XmlSchemaDerivationMethod.Extension;
			ComplexTypeBlockAllowed = FinalAllowed;
			ElementBlockAllowed = XmlSchemaDerivationMethod.Substitution | 
				FinalAllowed;
		}

		internal static XmlSchemaDerivationMethod FinalAllowed;
		internal static XmlSchemaDerivationMethod ElementBlockAllowed;
		internal static XmlSchemaDerivationMethod ComplexTypeBlockAllowed;
		internal static readonly bool StrictMsCompliant = Environment.GetEnvironmentVariable ("MONO_STRICT_MS_COMPLIANT") == "yes";


		public static void AddToTable (XmlSchemaObjectTable table, XmlSchemaObject obj,
			XmlQualifiedName qname, ValidationEventHandler h)
		{
			if (table.Contains (qname)) {
				// FIXME: This logic unexpectedly allows 
				// one redefining item and two or more redefining items.
				// FIXME: redefining item is not simple replacement,
				// but much more complex stuff.
				if (obj.isRedefineChild) {	// take precedence.
					if (obj.redefinedObject != null)
						obj.error (h, String.Format ("Named item {0} was already contained in the schema object table.", qname));
					else
						obj.redefinedObject = table [qname];
					table.Set (qname, obj);
				}
				else if (table [qname].isRedefineChild) {
					if (table [qname].redefinedObject != null)
						obj.error (h, String.Format ("Named item {0} was already contained in the schema object table.", qname));
					else
						table [qname].redefinedObject = obj;
					return;	// never add to the table.
				}
				else if (StrictMsCompliant) {
					table.Set (qname, obj);
				}
				else
					obj.error (h, String.Format ("Named item {0} was already contained in the schema object table. {1}",
					                             qname, "Consider setting MONO_STRICT_MS_COMPLIANT to 'yes' to mimic MS implementation."));
			}
			else
				table.Set (qname, obj);
		}

		public static void CompileID (string id,  XmlSchemaObject xso, Hashtable idCollection, ValidationEventHandler h)
		{
			//check if the string conforms to http://www.w3.org/TR/2001/REC-xmlschema-2-20010502/datatypes.html#ID
			// 1. ID must be a NCName
			// 2. ID must be unique in the schema
			if(id == null)
				return;
			if(!CheckNCName(id)) 
				xso.error(h,id+" is not a valid id attribute");
			else if(idCollection.ContainsKey(id))
				xso.error(h,"Duplicate id attribute "+id);
			else
				idCollection.Add(id,xso);
		}

		public static bool CheckAnyUri (string uri)
		{
			if (uri.StartsWith ("##"))
				return false;
			return true;
		}

		public static bool CheckNormalizedString (string token)
		{
			return true;
		}

		public static bool CheckNCName (string name)
		{
			//check if the string conforms to http://www.w3.org/TR/2001/REC-xmlschema-2-20010502/datatypes.html#NCName
			return XmlChar.IsNCName (name);
		}

		public static bool CheckQName (XmlQualifiedName qname)
		{
			// What is this doing?
			return true;
		}

		public static XmlParserContext GetParserContext (XmlReader reader)
		{
			IHasXmlParserContext xctx = reader as IHasXmlParserContext;
			if (xctx != null)
				return xctx.ParserContext;

			return null;
		}

		public static bool IsBuiltInDatatypeName (XmlQualifiedName qname)
		{
			if (qname.Namespace == XmlSchema.XdtNamespace) {
				switch (qname.Name) {
				case "anyAtomicType":
				case "untypedAtomic":
				case "dayTimeDuration":
				case "yearMonthDuration":
					return true;
				default:
					return false;
				}
			}
			if (qname.Namespace != XmlSchema.Namespace)
				return false;
			switch (qname.Name) {
			case "anySimpleType":
			case "duration": case "dateTime": case "time":
			case "date": case "gYearMonth": case "gYear":
			case "gMonthDay": case "gDay": case "gMonth":
			case "boolean":
			case "base64Binary": case "hexBinary":
			case "float": case "double":
			case "anyURI":
			case "QName":
			case "NOTATION":
			case "string": case "normalizedString": case "token":
			case "language": case "Name": case "NCName":
			case "ID": case "IDREF": case "IDREFS":
			case "ENTITY": case "ENTITIES":
			case "NMTOKEN": case "NMTOKENS":
			case "decimal": case "integer":
			case "nonPositiveInteger": case "negativeInteger":
			case "nonNegativeInteger":
			case "unsignedLong": case "unsignedInt":
			case "unsignedShort": case "unsignedByte":
			case "positiveInteger":
			case "long": case "int": case "short": case "byte":
				return true;
			}
			return false;
		}

		public static bool AreSchemaDatatypeEqual (XmlSchemaSimpleType st1, object v1, XmlSchemaSimpleType st2, object v2)
		{
			if (st1.Datatype is XsdAnySimpleType)
				return AreSchemaDatatypeEqual (st1.Datatype as XsdAnySimpleType, v1, st2.Datatype as XsdAnySimpleType, v2);
			// otherwise the types are lists of strings.
			string [] a1 = v1 as string [];
			string [] a2 = v2 as string [];
			if (st1 != st2 || a1 == null || a2 == null || a1.Length != a2.Length)
				return false;
			for (int i = 0; i < a1.Length; i++)
				if (a1 [i] != a2 [i])
					return false;
			return true;
		}

		public static bool AreSchemaDatatypeEqual (XsdAnySimpleType st1, object v1,
			XsdAnySimpleType st2, object v2)
		{
			if (v1 == null || v2 == null)
				return false;

			if (st1 == null)
				st1 = XmlSchemaSimpleType.AnySimpleType;
			if (st2 == null)
				st2 = XmlSchemaSimpleType.AnySimpleType;

			Type t = st2.GetType ();
			if (st1 is XsdFloat) {
				return st2 is XsdFloat && Convert.ToSingle (v1) == Convert.ToSingle (v2);
			} else if (st1 is XsdDouble) {
				return st2 is XsdDouble && Convert.ToDouble (v1) == Convert.ToDouble (v2);
			} else if (st1 is XsdDecimal) {
				if (!(st2 is XsdDecimal) || Convert.ToDecimal (v1) != Convert.ToDecimal (v2))
					return false;
				if (st1 is XsdNonPositiveInteger)
					return st2 is XsdNonPositiveInteger || t == typeof (XsdDecimal) || t == typeof (XsdInteger);
				else if (st1 is XsdPositiveInteger)
					return st2 is XsdPositiveInteger || t == typeof (XsdDecimal) || 
						t == typeof (XsdInteger) || t == typeof (XsdNonNegativeInteger);
				else if (st1 is XsdUnsignedLong)
					return st2 is XsdUnsignedLong || t == typeof (XsdDecimal) || 
						t == typeof (XsdInteger) || t == typeof (XsdNonNegativeInteger);
				else if (st1 is XsdNonNegativeInteger)
					return st2 is XsdNonNegativeInteger || t == typeof (XsdDecimal) || t == typeof (XsdInteger);
				else if (st1 is XsdLong)
					return st2 is XsdLong || t == typeof (XsdDecimal) || t == typeof (XsdInteger);
				return true;
			}
			else if (!v1.Equals (v2))
				return false;
			if (st1 is XsdString) {
				if (!(st2 is XsdString))
					return false;
				if (st1 is XsdNMToken && (st2 is XsdLanguage || st2 is XsdName))
					return false;
				if (st2 is XsdNMToken && (st1 is XsdLanguage || st1 is XsdName))
					return false;
				if (st1 is XsdName && (st2 is XsdLanguage || st2 is XsdNMToken))
					return false;
				if (st2 is XsdName && (st1 is XsdLanguage || st1 is XsdNMToken))
					return false;
				if (st1 is XsdID && st2 is XsdIDRef)
					return false;
				if (st1 is XsdIDRef && st2 is XsdID)
					return false;
			}
			else if (st1 != st2)
				return false;
			return true;
		}

		public static bool IsValidQName(string qname)
		{
			foreach(string part in qname.Split(new char[]{':'},2))
			{
				if(!CheckNCName(part))
					return false;
			}
			return true;
		}

		//FIXME: First remove all the multiple instances of whitespace and then return the strings.
		//The current method returns empty strings if there are two or more consecutive whitespaces.
		public static string[] SplitList(string list)
		{
			if(list == null || list == string.Empty)
				return new string [0];

			ArrayList al = null;
			int start = 0;
			bool wait = true;
			for (int i = 0; i < list.Length; i++) {
				switch (list [i]) {
				case ' ':
				case '\r':
				case '\n':
				case '\t':
					if (!wait) {
						if (al == null)
							al = new ArrayList ();
						al.Add (list.Substring (start, i - start));
					}
					wait = true;
					break;
				default:
					if (wait) {
						wait = false;
						start = i;
					}
					break;
				}
			}

			if (!wait && start == 0)
				return new string [] {list};

			if (!wait && start < list.Length)
				al.Add (start == 0 ? list : list.Substring (start));
			return al.ToArray (typeof (string)) as string [];
		}

		public static void ReadUnhandledAttribute(XmlReader reader, XmlSchemaObject xso)
		{
			if(reader.Prefix == "xmlns")
				xso.Namespaces.Add(reader.LocalName, reader.Value);
			else if(reader.Name == "xmlns")
				xso.Namespaces.Add("",reader.Value);
			else
			{
				if(xso.unhandledAttributeList == null)
					xso.unhandledAttributeList = new System.Collections.ArrayList();
				XmlAttribute attr = new XmlDocument().CreateAttribute(reader.LocalName,reader.NamespaceURI);
				attr.Value = reader.Value;
				ParseWsdlArrayType (reader, attr);
				xso.unhandledAttributeList.Add(attr);
			}
		}
		
		static void ParseWsdlArrayType (XmlReader reader, XmlAttribute attr)
		{
			if (attr.NamespaceURI == XmlSerializer.WsdlNamespace && attr.LocalName == "arrayType")
			{
				string ns = "", type, dimensions;
				TypeTranslator.ParseArrayType (attr.Value, out type, out ns, out dimensions);
				if (ns != "") ns = reader.LookupNamespace (ns) + ":";
				attr.Value = ns + type + dimensions;
			}
		}

		public static bool ReadBoolAttribute(XmlReader reader, out Exception innerExcpetion)
		{
			innerExcpetion = null;
			try
			{
				bool val = XmlConvert.ToBoolean(reader.Value);
				return val;
			}
			catch(Exception ex)
			{
				innerExcpetion = ex;
				return false;
			}
		}
		public static decimal ReadDecimalAttribute(XmlReader reader,  out Exception innerExcpetion)
		{
			innerExcpetion = null;
			try
			{
				decimal val = XmlConvert.ToDecimal(reader.Value);
				return val;
			}
			catch(Exception ex)
			{
				innerExcpetion = ex;
				return decimal.Zero;
			}
		}

		// Is some value is read, return it.
		// If no values return empty.
		// If exception, return none
		public static XmlSchemaDerivationMethod ReadDerivationAttribute(XmlReader reader, out Exception innerExcpetion, string name, XmlSchemaDerivationMethod allowed)
		{
			innerExcpetion = null;
			try
			{
				string list = reader.Value;
				string warn = "";
				XmlSchemaDerivationMethod val = 0;
				
				if(list.IndexOf("#all") != -1 && list.Trim() != "#all")
				{
					innerExcpetion = new Exception(list+" is not a valid value for "+ name +". #all if present must be the only value");
					return XmlSchemaDerivationMethod.All;
				}
				foreach(string xsdm in XmlSchemaUtil.SplitList(list))
				{
					switch(xsdm)
					{
						case "":
							val = AddFlag (val, XmlSchemaDerivationMethod.Empty, allowed); break;
						case "#all":
							val = AddFlag (val,XmlSchemaDerivationMethod.All, allowed); break;
						case "substitution":
							val = AddFlag (val,XmlSchemaDerivationMethod.Substitution, allowed); break;
						case "extension":
							val = AddFlag (val,XmlSchemaDerivationMethod.Extension, allowed); break;
						case "restriction":
							val = AddFlag (val,XmlSchemaDerivationMethod.Restriction, allowed); break;
						case "list":
							val = AddFlag (val,XmlSchemaDerivationMethod.List, allowed); break;
						case "union":
							val = AddFlag (val,XmlSchemaDerivationMethod.Union, allowed); break;
						default:
							warn += xsdm + " "; break;
					}
				}
				if(warn != "")
						innerExcpetion = new Exception(warn + "is/are not valid values for " + name);
				return val;
			}
			catch(Exception ex)
			{
				innerExcpetion = ex;
				return XmlSchemaDerivationMethod.None;
			}
		}

		private static XmlSchemaDerivationMethod AddFlag (XmlSchemaDerivationMethod dst,
			XmlSchemaDerivationMethod add, XmlSchemaDerivationMethod allowed)
		{
			if ((add & allowed) == 0 && allowed != XmlSchemaDerivationMethod.All)
				throw new ArgumentException (add + " is not allowed in this attribute.");
			if ((dst & add) != 0)
				throw new ArgumentException (add + " is already specified in this attribute.");
			return dst | add;
		}

		public static XmlSchemaForm ReadFormAttribute(XmlReader reader, out Exception innerExcpetion)
		{
			innerExcpetion = null;
			XmlSchemaForm val = XmlSchemaForm.None;
			switch(reader.Value != null ? reader.Value.Trim () : null)
			{
				case "qualified":
					val = XmlSchemaForm.Qualified; break;
				case "unqualified":
					val = XmlSchemaForm.Unqualified; break;
				default:
					innerExcpetion = new Exception("only qualified or unqulified is a valid value"); break;
			}
			return val;
		}

		public static XmlSchemaContentProcessing ReadProcessingAttribute(XmlReader reader, out Exception innerExcpetion)
		{
			innerExcpetion = null;
			XmlSchemaContentProcessing val = XmlSchemaContentProcessing.None;
			switch(reader.Value != null ? reader.Value.Trim () : null)
			{
				case "lax":
					val = XmlSchemaContentProcessing.Lax; break;
				case "strict":
					val = XmlSchemaContentProcessing.Strict; break;
				case "skip":
					val = XmlSchemaContentProcessing.Skip; break;
				default:
					innerExcpetion = new Exception("only lax , strict or skip are valid values for processContents");
					break;
			}
			return val;
		}

		public static XmlSchemaUse ReadUseAttribute(XmlReader reader, out Exception innerExcpetion)
		{
			innerExcpetion = null;
			XmlSchemaUse val = XmlSchemaUse.None;
			switch(reader.Value != null ? reader.Value.Trim () : null)
			{
				case "optional":
					val = XmlSchemaUse.Optional; break;
				case "prohibited":
					val = XmlSchemaUse.Prohibited; break;
				case "required":
					val = XmlSchemaUse.Required; break;
				default:
					innerExcpetion = new Exception("only optional , prohibited or required are valid values for use");
					break;
			}
			return val;
		}
		public static XmlQualifiedName ReadQNameAttribute(XmlReader reader, out Exception innerEx)
		{
			return ToQName(reader, reader.Value, out innerEx);
		}

		//While Creating a XmlQualifedName, we should check:
		// 1. If a prefix is present, its namespace should be resolvable.
		// 2. If a prefix is not present, and if the defaultNamespace is set, 
		public static XmlQualifiedName ToQName(XmlReader reader, string qnamestr, out Exception innerEx)
		{

			string ns;
			string name;
			XmlQualifiedName qname;
			innerEx = null;
			
			if(!IsValidQName(qnamestr))
			{
				innerEx = new Exception(qnamestr + " is an invalid QName. Either name or namespace is not a NCName");
				return XmlQualifiedName.Empty;
			}

			string[] values = qnamestr.Split(new char[]{':'},2);

			if(values.Length == 2)
			{
				ns = reader.LookupNamespace(values[0]);
				if(ns == null)
				{
					innerEx = new Exception("Namespace Prefix '"+values[0]+"could not be resolved");
					return XmlQualifiedName.Empty;
				}
				name = values[1];
			}
			else
			{
				//Default Namespace
				ns = reader.LookupNamespace("");
				name = values[0];
			}

			qname = new XmlQualifiedName(name,ns);
			return qname;
		}

		public static int ValidateAttributesResolved (
			XmlSchemaObjectTable attributesResolved,
			ValidationEventHandler h,
			XmlSchema schema,
			XmlSchemaObjectCollection attributes,
			XmlSchemaAnyAttribute anyAttribute,
			ref XmlSchemaAnyAttribute anyAttributeUse,
			XmlSchemaAttributeGroup redefined,
			bool skipEquivalent)
		{
			int errorCount = 0;
			if (anyAttribute != null && anyAttributeUse == null)
				anyAttributeUse = anyAttribute;

			ArrayList newAttrNames = new ArrayList ();

			foreach (XmlSchemaObject xsobj in attributes) {
				XmlSchemaAttributeGroupRef grpRef = xsobj as XmlSchemaAttributeGroupRef;
				if (grpRef != null) {
					// Resolve attributeGroup redefinition.
					XmlSchemaAttributeGroup grp = null;
					if (redefined != null && grpRef.RefName == redefined.QualifiedName)
						grp = redefined;
					else
						grp = schema.FindAttributeGroup (grpRef.RefName);
					// otherwise, it might be missing sub components.
					if (grp == null) {
						if (!schema.missedSubComponents)// && schema.Schemas [grpRef.RefName.Namespace] != null)
							grpRef.error (h, "Referenced attribute group " + grpRef.RefName + " was not found in the corresponding schema.");
						continue;
					}
					if (grp.AttributeGroupRecursionCheck) {
						grp.error (h, "Attribute group recursion was found: " + grpRef.RefName);
						continue;
					}
					try {
						grp.AttributeGroupRecursionCheck = true;
						errorCount += grp.Validate (h, schema);
					} finally {
						grp.AttributeGroupRecursionCheck = false;
					}
					if (grp.AnyAttributeUse != null) {
						if (anyAttribute == null)
							anyAttributeUse = grp.AnyAttributeUse;
					}
					foreach (DictionaryEntry entry in grp.AttributeUses) {
						XmlSchemaAttribute attr = (XmlSchemaAttribute) entry.Value;

						if (StrictMsCompliant && attr.Use == XmlSchemaUse.Prohibited)
							continue;

						if (attr.RefName != null && attr.RefName != XmlQualifiedName.Empty && (!skipEquivalent || !AreAttributesEqual (attr, attributesResolved [attr.RefName] as XmlSchemaAttribute)))
							AddToTable (attributesResolved, attr, attr.RefName, h);
						else if (!skipEquivalent || !AreAttributesEqual (attr, attributesResolved [attr.QualifiedName] as XmlSchemaAttribute))
							AddToTable (attributesResolved, attr, attr.QualifiedName, h);
					}
				} else {
					XmlSchemaAttribute attr = xsobj as XmlSchemaAttribute;
					if (attr != null) {
						errorCount += attr.Validate (h, schema);

						if (newAttrNames.Contains (attr.QualifiedName))
							attr.error (h, String.Format ("Duplicate attributes was found for '{0}'", attr.QualifiedName));
						newAttrNames.Add (attr.QualifiedName);


						if (StrictMsCompliant && attr.Use == XmlSchemaUse.Prohibited)
							continue;

						if (attr.RefName != null && attr.RefName != XmlQualifiedName.Empty && (!skipEquivalent || !AreAttributesEqual (attr, attributesResolved [attr.RefName] as XmlSchemaAttribute)))
							AddToTable (attributesResolved, attr, attr.RefName, h);
						else if (!skipEquivalent || !AreAttributesEqual (attr, attributesResolved [attr.QualifiedName] as XmlSchemaAttribute))
							AddToTable (attributesResolved, attr, attr.QualifiedName, h);
					} else {
						if (anyAttribute != null) {
							anyAttributeUse = (XmlSchemaAnyAttribute) xsobj;
							anyAttribute.Validate (h, schema);
						}
					}
				}
			}
			return errorCount;
		}

		internal static bool AreAttributesEqual (XmlSchemaAttribute one,
			XmlSchemaAttribute another)
		{
			if (one == null || another == null)
				return false;
			return one.AttributeType == another.AttributeType &&
				one.Form == another.Form &&
				one.ValidatedUse == another.ValidatedUse &&
				one.ValidatedDefaultValue == another.ValidatedDefaultValue &&
				one.ValidatedFixedValue == another.ValidatedFixedValue;
		}

#if NET_2_0
		public static object ReadTypedValue (XmlReader reader,
			object type, IXmlNamespaceResolver nsResolver,
			StringBuilder tmpBuilder)
#else
		public static object ReadTypedValue (XmlReader reader,
			object type, XmlNamespaceManager nsResolver,
			StringBuilder tmpBuilder)
#endif
		{
			if (tmpBuilder == null)
				tmpBuilder = new StringBuilder ();
			XmlSchemaDatatype dt = type as XmlSchemaDatatype;
			XmlSchemaSimpleType st = type as XmlSchemaSimpleType;
			if (st != null)
				dt = st.Datatype;
			if (dt == null)
				return null;

			switch (reader.NodeType) {
			case XmlNodeType.Element:
				if (reader.IsEmptyElement)
					return null;

				tmpBuilder.Length = 0;
				bool loop = true;
				do {
					reader.Read ();
					switch (reader.NodeType) {
					case XmlNodeType.SignificantWhitespace:
					case XmlNodeType.Text:
					case XmlNodeType.CDATA:
						tmpBuilder.Append (reader.Value);
						break;
					case XmlNodeType.Comment:
						break;
					default:
						loop = false;
						break;
					}
				} while (loop && !reader.EOF && reader.ReadState == ReadState.Interactive);
				return dt.ParseValue (tmpBuilder.ToString (), reader.NameTable, nsResolver);
			case XmlNodeType.Attribute:
				return dt.ParseValue (reader.Value, reader.NameTable, nsResolver);
			}
			return null;
		}

		public static XmlSchemaObject FindAttributeDeclaration (
			string ns,
			XmlSchemaSet schemas,
			XmlSchemaComplexType cType,
			XmlQualifiedName qname)
		{
			XmlSchemaObject result = cType.AttributeUses [qname];
			if (result != null)
				return result;
			if (cType.AttributeWildcard == null)
				return null;

			if (!AttributeWildcardItemValid (cType.AttributeWildcard, qname, ns))
				return null;

			if (cType.AttributeWildcard.ResolvedProcessContents == XmlSchemaContentProcessing.Skip)
				return cType.AttributeWildcard;
			XmlSchemaAttribute attr = schemas.GlobalAttributes [qname] as XmlSchemaAttribute;
			if (attr != null)
				return attr;
			if (cType.AttributeWildcard.ResolvedProcessContents == XmlSchemaContentProcessing.Lax)
				return cType.AttributeWildcard;
			else
				return null;
		}

		// Spec 3.10.4 Item Valid (Wildcard)
		private static bool AttributeWildcardItemValid (XmlSchemaAnyAttribute anyAttr, XmlQualifiedName qname, string ns)
		{
			if (anyAttr.HasValueAny)
				return true;
			if (anyAttr.HasValueOther && (anyAttr.TargetNamespace == "" || ns != anyAttr.TargetNamespace))
				return true;
			if (anyAttr.HasValueTargetNamespace && ns == anyAttr.TargetNamespace)
				return true;
			if (anyAttr.HasValueLocal && ns == "")
				return true;
			for (int i = 0; i < anyAttr.ResolvedNamespaces.Count; i++)
				if (anyAttr.ResolvedNamespaces [i] == ns)
					return true;
			return false;
		}
	}
}
