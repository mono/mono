//
// System.Xml.Schema.XmlSchemaDatatype.cs
//
// Authors:
//	Dwivedi, Ajay kumar <Adwiv@Yahoo.com>
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
using System;
using System.Collections;
using System.Text;
using System.Xml;
using Mono.Xml.Schema;

namespace System.Xml.Schema
{
	public abstract class XmlSchemaDatatype
	{
		protected XmlSchemaDatatype()
		{
		}
		
		internal XsdWhitespaceFacet WhitespaceValue =
			XsdWhitespaceFacet.Preserve;

		// Common Facets
		public virtual XsdWhitespaceFacet Whitespace {
			get { return WhitespaceValue; }
		}

		public abstract XmlTokenizedType TokenizedType {  get; }
		public abstract Type ValueType {  get; }

		// Methods
		public abstract object ParseValue (string s, 
			XmlNameTable nameTable, XmlNamespaceManager nsmgr);

		char [] wsChars = new char [] {' ', '\t', '\n', '\r'};

		protected string Normalize (string s)
		{
			switch (Whitespace) {
			case XsdWhitespaceFacet.Collapse:
				return String.Join (" ", s.Trim ().Split (wsChars));
			case XsdWhitespaceFacet.Replace:
				StringBuilder sb = new StringBuilder (s);
				sb.Replace ('\r', ' ');
				sb.Replace ('\n', ' ');
				sb.Replace ('\t', ' ');
				string result = sb.ToString ();
				sb.Length = 0;
				return result;
			default:
				return s;
			}
		}

		//TODO: This should return all appropriate inbuilt type
		internal static XmlSchemaDatatype GetType (XmlQualifiedName qname)
		{
			if (qname.Namespace == "http://www.w3.org/2001/XMLSchema" ||
				qname.Namespace == String.Empty)
				return FromName (qname.Name);
			throw new NotImplementedException ();
		}

		//TODO: This should return all appropriate inbuilt type
		internal static XmlSchemaDatatype FromName (string localName)
		{
			switch (localName) {
			case "string":
				return datatypeString;
			case "normalizedString":
				return datatypeNormalizedString;
			case "token":
				return datatypeToken;
			case "language":
				return datatypeLanguage;
			case "NMTOKEN":
				return datatypeNMToken;
			case "NMTOKENS":
				return datatypeNMTokens;
			case "Name":
				return datatypeName;
			case "NCname":
				return datatypeNCName;
			case "ID":
				return datatypeID;
			case "IDREF":
				return datatypeIDRef;
			case "IDREFS":
				return datatypeIDRefs;
			case "ENTITY":
				return datatypeEntity;
			case "ENTITIES":
				return datatypeEntities;
			case "NOTATION":
				return datatypeNotation;
			default:
				throw new NotImplementedException ();
			}
		}

		private static XsdString datatypeString = new XsdString ();
		private static XsdNormalizedString datatypeNormalizedString = new XsdNormalizedString ();
		private static XsdToken datatypeToken = new XsdToken ();
		private static XsdLanguage datatypeLanguage = new XsdLanguage ();
		private static XsdNMToken datatypeNMToken = new XsdNMToken ();
		private static XsdNMTokens datatypeNMTokens = new XsdNMTokens ();
		private static XsdName datatypeName = new XsdName ();
		private static XsdNCName datatypeNCName = new XsdNCName ();
		private static XsdID datatypeID = new XsdID ();
		private static XsdIDRef datatypeIDRef = new XsdIDRef ();
		private static XsdIDRefs datatypeIDRefs = new XsdIDRefs ();
		private static XsdEntity datatypeEntity = new XsdEntity ();
		private static XsdEntities datatypeEntities = new XsdEntities ();
		private static XsdNotation datatypeNotation = new XsdNotation ();

	}
}
