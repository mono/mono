using System;
using System.Collections;
using System.Collections.Generic;

namespace System.Xml
{
	internal class XmlSimpleDictionaryReader :
		XmlDictionaryReader, IXmlLineInfo, IXmlNamespaceResolver
	{
		XmlDictionary dict;
		XmlReader reader;
		XmlDictionaryReader as_dict_reader;
		IXmlLineInfo as_line_info;
		OnXmlDictionaryReaderClose onClose;

		public XmlSimpleDictionaryReader (XmlReader reader)
			: this (reader, null)
		{
		}

		public XmlSimpleDictionaryReader (XmlReader reader,
			XmlDictionary dictionary)
			: this (reader, dictionary, null)
		{
		}

		public XmlSimpleDictionaryReader (XmlReader reader,
			XmlDictionary dictionary, OnXmlDictionaryReaderClose onClose)
		{
			this.reader = reader;
			this.onClose = onClose;
			as_line_info = reader as IXmlLineInfo;
			as_dict_reader = reader as XmlDictionaryReader;

			if (dictionary == null)
				dictionary = new XmlDictionary ();
			dict = dictionary;
		}

		#region IXmlLineInfo
		public int LineNumber {
			get { return as_line_info != null ? as_line_info.LineNumber : 0; }
		}

		public int LinePosition {
			get { return as_line_info != null ? as_line_info.LinePosition : 0; }
		}

		public bool HasLineInfo ()
		{
			return as_line_info != null ? as_line_info.HasLineInfo () : false;
		}
		#endregion

		#region XmlDictionaryReader

		public override bool CanCanonicalize {
			get { return as_dict_reader != null ? as_dict_reader.CanCanonicalize : false; }
		}

		public override void EndCanonicalization ()
		{
			if (as_dict_reader != null)
				as_dict_reader.EndCanonicalization ();
			else
				throw new NotSupportedException ();
		}

		// no need to override for GetAttribute(), IndexOfLocalName(),
		// IsLocalName(), IsNamespaceUri(), IsStartElement()

		public override bool TryGetLocalNameAsDictionaryString (
			out XmlDictionaryString localName)
		{
			// FIXME: find out when it returns true.
			localName = null;
			return false;
//			if (!dict.TryLookup (LocalName, out localName))
//				return false;
//			return true;
		}

		public override bool TryGetNamespaceUriAsDictionaryString (
			out XmlDictionaryString namespaceUri)
		{
			// FIXME: find out when it returns true.
			namespaceUri = null;
			return false;
//			if (!dict.TryLookup (NamespaceURI, out namespaceUri))
//				return false;
//			return true;
		}
		#endregion

		#region IXmlNamespaceResolver

		public IDictionary<string,string> GetNamespacesInScope (
			XmlNamespaceScope scope)
		{
			IXmlNamespaceResolver nsr = reader as IXmlNamespaceResolver;
			return nsr.GetNamespacesInScope (scope);
		}

		public string LookupPrefix (string ns)
		{
			IXmlNamespaceResolver nsr = reader as IXmlNamespaceResolver;
			return nsr.LookupPrefix (NameTable.Get (ns));
		}

		#endregion

		#region XmlReader

		public override int AttributeCount {
			get { return reader.AttributeCount; }
		}

		public override string BaseURI {
			get { return reader.BaseURI; }
		}

		public override int Depth {
			get { return reader.Depth; }
		}

		public override XmlNodeType NodeType 
		{
			get { return reader.NodeType; }
		}

		public override string Name {
			get { return reader.Name; }
		}

		public override string LocalName {
			get { return reader.LocalName; }
		}

		public override string NamespaceURI {
			get { return reader.NamespaceURI; }
		}

		public override string Prefix {
			get { return reader.Prefix; }
		}

		public override bool HasValue {
			get { return reader.HasValue; }
		}

		public override string Value {
			get { return reader.Value; }
		}

		public override bool IsEmptyElement {
			get { return reader.IsEmptyElement; }
		}

		public override bool IsDefault {
			get { return reader.IsDefault; }
		}

		public override char QuoteChar {
			get { return reader.QuoteChar; }
		}

		public override string XmlLang {
			get { return reader.XmlLang; }
		}

		public override XmlSpace XmlSpace {
			get { return reader.XmlSpace; }
		}

		public override string this [int i] {
			get { return reader [i]; }
		}

		public override string this [string name] {
			get { return reader [name]; }
		}

		public override string this [string localName, string namespaceURI] {
			get { return reader [localName, namespaceURI]; }
		}

		public override bool EOF {
			get { return reader.EOF; }
		}

		public override ReadState ReadState {
			get { return reader.ReadState; }
		}

		public override XmlNameTable NameTable {
			get { return reader.NameTable; }
		}

		public override string GetAttribute (string name)
		{
			return reader.GetAttribute (name);
		}

		public override string GetAttribute (string localName, string namespaceURI)
		{
			return reader.GetAttribute (localName, namespaceURI);
		}

		public override string GetAttribute (int i)
		{
			return reader.GetAttribute (i);
		}

		public override bool MoveToAttribute (string name)
		{
			return reader.MoveToAttribute (name);
		}

		public override bool MoveToAttribute (string localName, string namespaceURI)
		{
			return reader.MoveToAttribute (localName, namespaceURI);
		}

		public override void MoveToAttribute (int i)
		{
			reader.MoveToAttribute (i);
		}

		public override bool MoveToFirstAttribute ()
		{
			return reader.MoveToFirstAttribute ();
		}

		public override bool MoveToNextAttribute ()
		{
			return reader.MoveToNextAttribute ();
		}

		public override bool MoveToElement ()
		{
			return reader.MoveToElement ();
		}

		public override void Close ()
		{
			reader.Close ();
			if (onClose != null)
				onClose (this);
		}

		public override bool Read ()
		{
			if (!reader.Read ())
				return false;
			dict.Add (reader.Prefix);
			dict.Add (reader.LocalName);
			dict.Add (reader.NamespaceURI);
			if (reader.MoveToFirstAttribute ()) {
				do {
					dict.Add (reader.Prefix);
					dict.Add (reader.LocalName);
					dict.Add (reader.NamespaceURI);
					dict.Add (reader.Value);
				} while (reader.MoveToNextAttribute ());
				reader.MoveToElement ();
			}
			return true;
		}

		public override string ReadString ()
		{
			return reader.ReadString ();
		}

		public override string ReadInnerXml ()
		{
			return reader.ReadInnerXml ();
		}

		public override string ReadOuterXml ()
		{
			return reader.ReadOuterXml ();
		}

		public override string LookupNamespace (string prefix)
		{
			return reader.LookupNamespace (prefix);
		}

		public override void ResolveEntity ()
		{
			reader.ResolveEntity ();
		}
		public override bool ReadAttributeValue ()
		{
			return reader.ReadAttributeValue ();
		}
		#endregion
	}
}
