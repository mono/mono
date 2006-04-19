using System;
using System.Collections;
using System.IO;
using System.Xml;

namespace Commons.Xml.Nvdl
{
	internal class NvdlFilteredXmlReader : XmlReader, IXmlLineInfo
	{
		// In this XmlReader, when AttachPlaceHolder() is called,
		// it treats the next node as to point virtual "placeholder"
		// element where IsEmptyElement is false. When it is
		// Detached, then *current* node becomes the usual reader
		// node.

		bool initial = true;
		int placeHolderDepth = 0;
		XmlNodeType nextPlaceHolder;
		XmlNodeType placeHolder = XmlNodeType.None;
		bool placeHolderLocalNameAttr;
		NvdlValidateInterp validate;
		XmlReader reader;
		IXmlLineInfo reader_as_line_info;

		AttributeInfo [] attributes = new AttributeInfo [10];
		int attributeCount = 0;

		// PlanAtt validation cache.
		Hashtable attributeValidators = new Hashtable ();

		class AttributeInfo
		{
			public string LocalName;
			public string NamespaceURI;
		}

		public NvdlFilteredXmlReader (XmlReader reader,
			NvdlValidateInterp validate)
		{
			this.reader = reader;
			reader_as_line_info = reader as IXmlLineInfo;
			this.validate = validate;
		}

		public bool HasLineInfo ()
		{
			return reader_as_line_info != null ? reader_as_line_info.HasLineInfo () : false;
		}

		public int LineNumber {
			get { return reader_as_line_info != null ? reader_as_line_info.LineNumber : 0; }
		}

		public int LinePosition {
			get { return reader_as_line_info != null ? reader_as_line_info.LinePosition : 0; }
		}

		public void AttachPlaceholder ()
		{
			placeHolderDepth++;
			nextPlaceHolder = XmlNodeType.Element;
		}

		public void DetachPlaceholder ()
		{
			placeHolderDepth--;
			placeHolder = XmlNodeType.None;
		}

		private void AddAttribute ()
		{
			if (attributes.Length == attributeCount) {
				AttributeInfo [] newArr =
					new AttributeInfo [attributeCount * 2];
				Array.Copy (attributes, newArr, attributeCount);
			}
			AttributeInfo ai = attributes [attributeCount];
			if (ai == null) {
				ai = new AttributeInfo ();
				attributes [attributeCount] = ai;
			}
			ai.LocalName = reader.LocalName;
			ai.NamespaceURI = reader.NamespaceURI;
			attributeCount++;
		}

		private SimpleRule FindAttributeRule (string ns, SimpleMode mode)
		{
			SimpleRule any = null;
			foreach (SimpleRule rule in mode.AttributeRules) {
				if (!rule.MatchNS (ns))
					continue;
				if (!rule.IsAny)
					return rule;
				any = rule;
			}
			if (any != null)
				return any;
			throw new NvdlValidationException ("NVDL internal error: should not happen. No matching rule was found.", reader as IXmlLineInfo);
		}

		// Public overrides

		public override bool Read ()
		{
			// This class itself never proceeds, just checks if
			// the reader is placed on EOF.
			if (reader.EOF)
				return false;

			MoveToElement ();
			attributeCount = 0;

			if (nextPlaceHolder != XmlNodeType.None) {
				placeHolder = nextPlaceHolder;
				nextPlaceHolder = XmlNodeType.None;
				return true;
			}

			if (placeHolder != XmlNodeType.None)
				// Inside placeHolder, ignore all contents.
				// The source XmlReader should proceed
				// regardless of this filtered reader.
				return true;

			if (!reader.MoveToFirstAttribute ())
				return true;

			// Attribute rule application
			attributeValidators.Clear ();
			do {
				if (reader.NamespaceURI == "http://www.w3.org/2000/xmlns/")
					continue;
				// FIXME: could be more efficient
				SimpleRule rule = FindAttributeRule (
					reader.NamespaceURI,
					validate.CreatedMode);
				foreach (SimpleAction a in rule.Actions) {
					SimpleResultAction ra =
						a as SimpleResultAction;
					if (ra != null &&
						ra.ResultType == NvdlResultType.Attach)
						AddAttribute ();
					if (ra != null)
						continue;
					attributeValidators [reader.NamespaceURI] = a;
				}
			} while (reader.MoveToNextAttribute ());
			reader.MoveToElement ();

			if (attributeValidators.Count > 0) {
				foreach (string ns in attributeValidators.Keys) {
					((SimpleValidate) attributeValidators [
						ns]).ValidateAttributes (reader, ns);
				}
			}

			return true;
		}

		public override int AttributeCount {
			get {
				switch (placeHolder) {
				case XmlNodeType.Element:
				case XmlNodeType.Attribute: // ns or localName attribute on placeHolder element
				case XmlNodeType.Text: // attribute value of ns or localName attribute on placeHolder element
					return 2;
				case XmlNodeType.EndElement:
					return 0;
				default:
					return attributeCount;
				}
			}
		}

		public override string BaseURI {
			get { return reader.BaseURI; }
		}

		public override int Depth {
			get {
				if (placeHolderDepth == 0)
					return reader.Depth;
				int basis = reader.Depth + 1;
				switch (placeHolder) {
				case XmlNodeType.Text:
					return basis + 2;
				case XmlNodeType.Attribute:
					return basis + 1;
				default:
					return basis;
				}
			}
		}

		public override bool EOF {
			get { return reader.EOF && placeHolder != XmlNodeType.None; }
		}

		public override bool HasValue {
			get {
				switch (placeHolder) {
				case XmlNodeType.Attribute:
				case XmlNodeType.Text:
					return true;
				case XmlNodeType.Element:
				case XmlNodeType.EndElement:
					return false;
				default:
					return reader.HasValue;
				}
			}
		}

		public override bool IsDefault {
			get {
				return placeHolder == XmlNodeType.None &&
					reader.IsDefault;
			}
		}

		public override bool IsEmptyElement {
			get {
				return placeHolder != XmlNodeType.None ||
					reader.IsEmptyElement; 
			}
		}

		public override string LocalName {
			get {
				switch (placeHolder) {
				case XmlNodeType.Element:
				case XmlNodeType.EndElement:
					return "placeholder";
				case XmlNodeType.Attribute:
					return placeHolderLocalNameAttr ?
						"localName" : "ns";
				case XmlNodeType.Text:
					return String.Empty;
				default:
					return reader.LocalName;
				}
			}
		}

		public override string Name {
			get {
				switch (placeHolder) {
				case XmlNodeType.Element:
				case XmlNodeType.EndElement:
					return "placeholder";
				case XmlNodeType.Attribute:
					return placeHolderLocalNameAttr ?
						"localName" : "ns";
				case XmlNodeType.Text:
					return String.Empty;
				default:
					return reader.Name;
				}
			}
		}

		public override string NamespaceURI {
			get {
				switch (placeHolder) {
				case XmlNodeType.Element:
				case XmlNodeType.EndElement:
					return Nvdl.InstanceNamespace;
				case XmlNodeType.Attribute:
					return String.Empty;
				case XmlNodeType.Text:
					return String.Empty;
				default:
					return reader.NamespaceURI;
				}
			}
		}

		public override XmlNameTable NameTable {
			get { return reader.NameTable; }
		}

		public override XmlNodeType NodeType {
			get {
				return placeHolder != XmlNodeType.None ?
					placeHolder : reader.NodeType; 
			}
		}

		public override string Prefix {
			get {
				return placeHolder != XmlNodeType.None ?
					String.Empty : reader.Name; 
			}
		}

		public override char QuoteChar {
			get { return reader.QuoteChar; }
		}

		public override ReadState ReadState {
			get { 
				return initial ? ReadState.Initial :
					placeHolder != XmlNodeType.None &&
					reader.ReadState != ReadState.Error ? 
					ReadState.Interactive :
					reader.ReadState; 
			}
		}

		public override string Value {
			get {
				switch (placeHolder) {
				case XmlNodeType.Element:
				case XmlNodeType.EndElement:
					return String.Empty;
				case XmlNodeType.Attribute:
				case XmlNodeType.Text:
					return placeHolderLocalNameAttr ?
						reader.LocalName :
						reader.NamespaceURI;
				default:
					return reader.Value;
				}
			}
		}

		// FIXME: xml:lang might have been filtered
		public override string XmlLang {
			get { return reader.XmlLang; }
		}

		// FIXME: xml:space might have been filtered
		public override XmlSpace XmlSpace {
			get { return reader.XmlSpace; }
		}

		public override string this [int i] {
			get { return GetAttribute (i); }
		}

		public override string this [string name] {
			get { return GetAttribute (name); }
		}

		public override string this [string localName, string ns] {
			get { return GetAttribute (localName, ns); }
		}

		public override string GetAttribute (int i)
		{
			if (placeHolder == XmlNodeType.Element)
				return placeHolderLocalNameAttr ?
					reader.LocalName : reader.NamespaceURI;
			else
				return reader.GetAttribute (
					attributes [i].LocalName,
					attributes [i].NamespaceURI);
		}

		public override string GetAttribute (string name)
		{
			if (placeHolder == XmlNodeType.Element) {
				switch (name) {
				case "localName":
					return reader.LocalName;
				case "ns":
					return reader.NamespaceURI;
				default:
					return null;
				}
			}
			return reader.GetAttribute (name);
		}

		public override string GetAttribute (string localName, string ns)
		{
			if (placeHolder == XmlNodeType.Element) {
				if (ns != String.Empty)
					return null;
				return GetAttribute (localName);
			}
			return reader.GetAttribute (localName, ns);
		}

		public override void ResolveEntity ()
		{
			if (placeHolder != XmlNodeType.None)
				throw new XmlException ("Current node is not an EntityReference.");
			reader.ResolveEntity ();
		}

		public override void Close ()
		{
			// do nothing.
		}

		public override string LookupNamespace (string prefix)
		{
			// For placeHolder element, we treat them as to have
			// empty prefix. So we have to handle empty prefix as if
			// it was mapped to the namespace.
			if (placeHolder != XmlNodeType.None && prefix == String.Empty)
				return Nvdl.InstanceNamespace;
			return reader.LookupNamespace (prefix);
		}

		public override bool MoveToElement ()
		{
			switch (placeHolder) {
			case XmlNodeType.Element:
			case XmlNodeType.EndElement:
				return false;
			case XmlNodeType.Attribute:
			case XmlNodeType.Text:
				placeHolder = XmlNodeType.Element;
				placeHolderLocalNameAttr = false;
				return true;
			default:
				return reader.MoveToElement ();
			}
		}

		public override bool MoveToFirstAttribute ()
		{
			switch (placeHolder) {
			case XmlNodeType.Element:
				placeHolder = XmlNodeType.Attribute;
				placeHolderLocalNameAttr = true;
				return true;
			case XmlNodeType.EndElement:
				return false;
			case XmlNodeType.Attribute:
			case XmlNodeType.Text:
				placeHolder = XmlNodeType.Attribute;
				placeHolderLocalNameAttr = true;
				return true;
			default:
				if (attributeCount == 0)
					return false;
				return reader.MoveToAttribute (
					attributes [0].LocalName,
					attributes [0].NamespaceURI);
			}
		}

		public override bool MoveToNextAttribute ()
		{
			switch (placeHolder) {
			case XmlNodeType.Element:
			case XmlNodeType.EndElement:
				return false;
			case XmlNodeType.Attribute:
			case XmlNodeType.Text:
				if (!placeHolderLocalNameAttr)
					return false;
				placeHolderLocalNameAttr = false;
				placeHolder = XmlNodeType.Attribute;
				return true;
			default:
				if (reader.NodeType == XmlNodeType.Element)
					return false;
				for (int i = 0; i < attributeCount - 1; i++) {
					if (attributes [i].LocalName != reader.LocalName ||
						attributes [i].NamespaceURI != reader.NamespaceURI)
						continue;
					reader.MoveToAttribute (
						attributes [i + 1].LocalName,
						attributes [i + 1].NamespaceURI);
					return true;
				}
				return false;
			}
		}

		public override bool ReadAttributeValue ()
		{
			switch (placeHolder) {
			case XmlNodeType.Element:
			case XmlNodeType.EndElement:
			case XmlNodeType.Text:
				return false;
			case XmlNodeType.Attribute:
				placeHolder = XmlNodeType.Text;
				return true;
			default:
				return reader.ReadAttributeValue ();
			}
		}

		public override void MoveToAttribute (int i)
		{
			switch (placeHolder) {
			case XmlNodeType.EndElement:
				throw new IndexOutOfRangeException ();
			case XmlNodeType.Element:
			case XmlNodeType.Attribute:
			case XmlNodeType.Text:
				switch (i) {
				case 0:
					placeHolderLocalNameAttr = true;
					return;
				case 1:
					placeHolderLocalNameAttr = false;
					return;
				}
				throw new IndexOutOfRangeException ();
			default:
				if (i < 0 || attributeCount <= i)
					throw new IndexOutOfRangeException ();
				reader.MoveToAttribute (
					attributes [i].LocalName,
					attributes [i].NamespaceURI);
				break;
			}
		}

		public override bool MoveToAttribute (string qname)
		{
			int index = qname.IndexOf (':');
			if (index < 0)
				return MoveToAttribute (qname, String.Empty);
			return MoveToAttribute (qname.Substring (index + 1),
				LookupNamespace (qname.Substring (0, index)));
		}

		public override bool MoveToAttribute (string localName, string ns)
		{
			switch (placeHolder) {
			case XmlNodeType.EndElement:
				return false;
			case XmlNodeType.Element:
			case XmlNodeType.Attribute:
			case XmlNodeType.Text:
				if (ns != String.Empty)
					return false;
				switch (localName) {
				case "localName":
					MoveToAttribute (0);
					return true;
				case "ns":
					MoveToAttribute (1);
					return true;
				default:
					return false;
				}
			default:
				for (int i = 0; i < attributeCount; i++) {
					if (attributes [i].LocalName != localName ||
						attributes [i].NamespaceURI != ns)
						continue;
					return reader.MoveToAttribute (localName, ns);
				}
				return false;
			}
		}
	}
}

