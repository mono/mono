using System;
using System.Collections.Specialized;
using System.Collections;
using System.Xml;
using System.Xml.Schema;

namespace Mono.Xml
{
	public class DTDValidatingReader : /*XmlValidatingReader*/XmlReader, IXmlLineInfo
	{
		public DTDValidatingReader (XmlReader reader)
			: this (reader, null)
		{
		}

		public DTDValidatingReader (XmlReader reader,
			XmlValidatingReader validatingReader)
//			: base (reader)
		{
			this.reader = reader;
			elementStack = new Stack ();
			automataStack = new Stack ();
			attributes = new StringCollection ();
			this.validatingReader = validatingReader;
		}

		XmlReader reader;
		DTDObjectModel dtd;
		Stack elementStack;
		Stack automataStack;
		string currentElement;
		string currentAttribute;
		bool consumedDefaultAttribute;
		bool inContent;
		DTDAutomata currentAutomata;
		DTDAutomata previousAutomata;
		bool isStandalone;
		StringCollection attributes;

		XmlValidatingReader validatingReader;
		ValidationEventHandler handler;

		public DTDObjectModel DTD {
			get { return dtd; }
		}

		public override void Close ()
		{
			reader.Close ();
		}

		// We had already done attribute validation, so can ignore name.
		public override string GetAttribute (int i)
		{
			if (dtd == null)
				return reader.GetAttribute (i);

			if (NodeType != XmlNodeType.Element)
				return String.Empty;
			if (attributes.Count <= i)
				throw new IndexOutOfRangeException ("Specified index is out of range: " + i);
			if (reader.AttributeCount > i)
				return reader [i];
			// Otherwise, the value is default.
			DTDAttributeDefinition def = dtd.ElementDecls [currentElement]
				.Attributes [i] as DTDAttributeDefinition;
			return def.DefaultValue;
		}

		public override string GetAttribute (string name)
		{
			if (dtd == null)
				return reader.GetAttribute (name);

			if (NodeType != XmlNodeType.Element)
				return String.Empty;

			string specified = reader.GetAttribute (name);
			if (specified != null)
				return specified;

			DTDAttributeDefinition def = dtd.ElementDecls [currentElement]
				.Attributes [name] as DTDAttributeDefinition;
			return def.DefaultValue;
		}

		public override string GetAttribute (string name, string ns)
		{
			if (dtd == null)
				return reader.GetAttribute (name, ns);

			if (NodeType != XmlNodeType.Element)
				return String.Empty;

			string specified = reader.GetAttribute (name, ns);
			if (specified != null)
				return specified;

			if (ns != String.Empty)
				throw new InvalidOperationException ("DTD validating reader does not support namespace.");
			return GetAttribute (name);
		}

		bool IXmlLineInfo.HasLineInfo ()
		{
			IXmlLineInfo ixli = reader as IXmlLineInfo;
			if (ixli != null)
				return ixli.HasLineInfo ();
			else
				return false;
		}

		public override string LookupNamespace (string prefix)
		{
			// Does it mean anything with DTD?
			return reader.LookupNamespace (prefix);
		}

		public override void MoveToAttribute (int i)
		{
			if (dtd == null) {
				reader.MoveToAttribute (i);
				currentAttribute = reader.Name;
				consumedDefaultAttribute = false;
				return;
			}

			if (currentElement == null)
				return;

			if (attributes.Count > i) {
				currentAttribute = attributes [i];
				consumedDefaultAttribute = false;
				return;
			} else
				throw new IndexOutOfRangeException ("The index is out of range.");
		}

		public override bool MoveToAttribute (string name)
		{
			if (dtd == null) {
				bool b = reader.MoveToAttribute (name);
				if (b) {
					currentAttribute = reader.Name;
					consumedDefaultAttribute = false;
				}
				return b;
			}

			if (currentElement == null)
				return false;

			int idx = attributes.IndexOf (name);
			if (idx >= 0) {
				currentAttribute = name;
				consumedDefaultAttribute = false;
				return true;
			}
			return false;
		}

		public override bool MoveToAttribute (string name, string ns)
		{
			if (dtd == null) {
				bool b = reader.MoveToAttribute (name, ns);
				if (b) {
					currentAttribute = reader.Name;
					consumedDefaultAttribute = false;
				}
				return b;
			}

			if (reader.MoveToAttribute (name, ns)) {
				currentAttribute = reader.Name;
				consumedDefaultAttribute = false;
				return true;
			}

			if (ns != String.Empty)
				throw new InvalidOperationException ("DTD validating reader does not support namespace.");
			return MoveToAttribute (name);
		}

		public override bool MoveToElement ()
		{
			bool b = reader.MoveToElement ();
			if (!b)
				return false;
			currentAttribute = null;
			consumedDefaultAttribute = false;
			return true;
		}

		public override bool MoveToFirstAttribute ()
		{
			if (dtd == null) {
				bool b = reader.MoveToFirstAttribute ();
				if (b) {
					currentAttribute = reader.Name;
					consumedDefaultAttribute = false;
				}
				return b;
			}

			// It should access attributes by *defined* order.
			if (NodeType != XmlNodeType.Element)
				return false;

			if (attributes.Count == 0)
				return false;
			reader.MoveToFirstAttribute ();
			currentAttribute = attributes [0];
			consumedDefaultAttribute = false;
			return true;
		}

		public override bool MoveToNextAttribute ()
		{
			if (dtd == null) {
				bool b = reader.MoveToNextAttribute ();
				if (b) {
					currentAttribute = reader.Name;
					consumedDefaultAttribute = false;
				}
				return b;
			}

			if (currentAttribute == null)
				return false;

			int idx = attributes.IndexOf (currentAttribute);
			if (idx + 1 < attributes.Count) {
				reader.MoveToNextAttribute ();
				currentAttribute = attributes [idx + 1];
				consumedDefaultAttribute = false;
				return true;
			} else
				return false;
		}

		[MonoTODO]
		public override bool Read ()
		{
			MoveToElement ();

			bool b = reader.Read ();
			currentElement = null;
			currentAttribute = null;
			consumedDefaultAttribute = false;
			attributes.Clear ();

			if (!inContent && reader.NodeType == XmlNodeType.Element) {
				inContent = true;
				if (dtd == null)
					currentAutomata = null;
				else
					currentAutomata = dtd.RootAutomata;
			}

			if (!b) {
				if (elementStack.Count != 0)
					throw new InvalidOperationException ("Unexpected end of XmlReader.");
				return false;
			}

			switch (reader.NodeType) {
			case XmlNodeType.XmlDeclaration:
				if (GetAttribute ("standalone") == "yes")
					isStandalone = true;
				break;

			case XmlNodeType.DocumentType:
				XmlTextReader xmlTextReader = reader as XmlTextReader;
				if (xmlTextReader == null) {
					xmlTextReader = new XmlTextReader ("", XmlNodeType.Document, null);
					xmlTextReader.GenerateDTDObjectModel (reader.Name,
						reader ["PUBLIC"], reader ["SYSTEM"], reader.Value);
				}
				this.dtd = xmlTextReader.DTD;
				break;

			case XmlNodeType.Element:	// startElementDeriv
				// If no schema specification, then skip validation.
				if (currentAutomata == null)
					break;

				previousAutomata = currentAutomata;
				currentAutomata = currentAutomata.TryStartElement (reader.Name);
				if (currentAutomata == DTD.Invalid) {
					HandleError (String.Format ("Invalid start element found: {0}", reader.Name),
						XmlSeverityType.Error);
					// FIXME: validation recovery code here.
					currentAutomata = previousAutomata;
				}
				DTDElementDeclaration decl = DTD.ElementDecls [reader.Name];
				if (decl == null) {
					HandleError (String.Format ("Element {0} is not declared.", reader.Name),
						XmlSeverityType.Error);
					// FIXME: validation recovery code here.
					currentAutomata = previousAutomata;
				}

				currentElement = Name;
				elementStack.Push (reader.Name);
				automataStack.Push (currentAutomata);
				if (decl != null) {	// i.e. not invalid
					currentAutomata = decl.ContentModel.GetAutomata ();

					// check attributes
					if (decl.Attributes == null) {
						if (reader.HasAttributes) {
							HandleError (String.Format ("Attributes are found on element {0} while it has no attribute definitions.",decl.Name),
								XmlSeverityType.Error);
							// FIXME: validation recovery code here.
						}
					}
					else
						ValidateAttributes (decl);
				} else if (reader.MoveToFirstAttribute ()) {
					// If it was invalid, simply add specified attributes.
					do {
						attributes.Add (reader.Name);
					} while (reader.MoveToNextAttribute ());
					reader.MoveToElement ();
				}

				// If it is empty element then directly check end element.
				if (reader.IsEmptyElement)
					goto case XmlNodeType.EndElement;
				break;

			case XmlNodeType.EndElement:	// endElementDeriv
				// If no schema specification, then skip validation.
				if (currentAutomata == null)
					break;

				decl = DTD.ElementDecls [reader.Name];
				if (decl == null) {
					HandleError (String.Format ("Element {0} is not declared.", reader.Name),
						XmlSeverityType.Error);
					// FIXME: validation recovery code here.
				}

				previousAutomata = currentAutomata;
				// Don't let currentAutomata
				DTDAutomata tmpAutomata = currentAutomata.TryEndElement ();
				if (tmpAutomata == DTD.Invalid) {
					HandleError (String.Format ("Invalid end element found: {0}", reader.Name),
						XmlSeverityType.Error);
					// FIXME: validation recovery code here.
					currentAutomata = previousAutomata;
				}

				elementStack.Pop ();
				currentAutomata = automataStack.Pop () as DTDAutomata;
				break;

			case XmlNodeType.CDATA:
			case XmlNodeType.SignificantWhitespace:
			case XmlNodeType.Text:
				// If no schema specification, then skip validation.
				if (currentAutomata == null)
					break;

				DTDElementDeclaration elem = dtd.ElementDecls [elementStack.Peek () as string];
				if (!elem.IsMixedContent) {
					HandleError (String.Format ("Current element {0} does not allow character data content.", elementStack.Peek () as string),
						XmlSeverityType.Error);
					// FIXME: validation recovery code here.
					currentAutomata = previousAutomata;
				}
				break;
			}
			return true;
		}

		private void HandleError (string message, XmlSeverityType severity)
		{
			IXmlLineInfo info = this as IXmlLineInfo;
			bool hasLine = info.HasLineInfo ();
			XmlSchemaException ex = new XmlSchemaException (
				message,
				hasLine ? info.LineNumber : 0,
				hasLine ? info.LinePosition : 0, 
				null,
				BaseURI, 
				null);

			if (validatingReader != null)
				this.validatingReader.OnValidationEvent (this,
					new ValidationEventArgs (ex, message, severity));
			else
				throw ex;
		}

		private void ValidateAttributes (DTDElementDeclaration decl)
		{
			if (reader.MoveToFirstAttribute ()) {
				do {
					attributes.Add (reader.Name);
					DTDAttributeDefinition def = decl.Attributes [reader.Name];
					if (def == null) {
						HandleError (String.Format ("Attribute {0} is not declared.", reader.Name),
							XmlSeverityType.Error);
						// FIXME: validation recovery code here.
					}
					switch (def.OccurenceType) {
					case DTDAttributeOccurenceType.Required:
						if (reader.Value == String.Empty) {
							HandleError (String.Format ("Required attribute {0} in element {1} not found .",
								def.Name, decl.Name),
								XmlSeverityType.Error);
							// FIXME: validation recovery code here.
						}
						break;
					case DTDAttributeOccurenceType.Fixed:
						if (reader.Value != def.DefaultValue) {
							HandleError (String.Format ("Fixed attribute {0} in element {1} has invalid value {2}.",
								def.Name, decl.Name, reader.Value),
								XmlSeverityType.Error);
							// FIXME: validation recovery code here.
						}
						break;
					}
				} while (reader.MoveToNextAttribute ());
			}
			// Check if all required attributes exist, and/or
			// if there is default values, then add them.
			foreach (DTDAttributeDefinition def in decl.Attributes.Definitions)
				if (!attributes.Contains (def.Name)) {
					if (def.OccurenceType == DTDAttributeOccurenceType.Required) {
						HandleError (String.Format ("Required attribute {0} was not found.", decl.Name),
							XmlSeverityType.Error);
						// FIXME: validation recovery code here.
					}
					else if (def.DefaultValue != null)
						attributes.Add (def.Name);
				}

			reader.MoveToElement ();
		}

		public override bool ReadAttributeValue ()
		{
			if (this.IsDefault) {
				if (consumedDefaultAttribute)
					return false;
				consumedDefaultAttribute = true;
				return true;
			}
			return reader.ReadAttributeValue ();
		}

		public override string ReadInnerXml ()
		{
			// MS.NET 1.0 has a serious bug here. It skips validation.
			return reader.ReadInnerXml ();
		}

		public override string ReadOuterXml ()
		{
			// MS.NET 1.0 has a serious bug here. It skips validation.
			return reader.ReadOuterXml ();
		}

		public override string ReadString ()
		{
			// It seems to be the same as ReadInnerXml(). 
			return reader.ReadString ();
		}

		[MonoTODO]
		public override void ResolveEntity ()
		{
			throw new NotImplementedException ();
		}

		public override int AttributeCount {
			get {
				if (dtd == null || !inContent)
					return reader.AttributeCount;

				return attributes.Count;
			}
		}

		[MonoTODO ("Should consider general entities.")]
		public override string BaseURI {
			get {
				return reader.BaseURI;
			}
		}

		public override bool CanResolveEntity {
			get { return true; }
		}

		[MonoTODO ("Should consider general entities' depth")]
		public override int Depth {
			get { return IsDefault ? reader.Depth + 1 : reader.Depth; }
		}

		[MonoTODO]
		public override bool EOF {
			get { return reader.EOF; }
		}

		public override bool HasValue {
			get { return IsDefault ? true : reader.HasValue; }
		}

		public override bool IsDefault {
			get {
				if (currentAttribute == null)
					return false;
				return reader.GetAttribute (currentAttribute) == null;
			}
		}

		public override bool IsEmptyElement {
			get { return reader.IsEmptyElement; }
		}

		public override string this [int i] {
			get { return GetAttribute (i); }
		}

		public override string this [string name] {
			get { return GetAttribute (name); }
		}

		public override string this [string name, string ns] {
			get { return GetAttribute (name, ns); }
		}

		public int LineNumber {
			get {
				IXmlLineInfo info = reader as IXmlLineInfo;
				return (info != null) ? info.LineNumber : 0;
			}
		}

		public int LinePosition {
			get {
				IXmlLineInfo info = reader as IXmlLineInfo;
				return (info != null) ? info.LinePosition : 0;
			}
		}

		public override string LocalName {
			get { return IsDefault ? currentAttribute : reader.LocalName; }
		}

		public override string Name {
			get { return IsDefault ? currentAttribute : reader.Name; }
		}

		public override string NamespaceURI {
			get { return IsDefault ? String.Empty : reader.NamespaceURI; }
		}

		public override XmlNameTable NameTable {
			get { return reader.NameTable; }
		}

		public override XmlNodeType NodeType {
			get { return IsDefault ? XmlNodeType.Attribute : reader.NodeType; }
		}

		public override string Prefix {
			get { return IsDefault ? String.Empty : reader.Prefix; }
		}
		
		public override char QuoteChar {
			get {
				// If it is not actually on an attribute, then it returns
				// undefined value or '"'.
				return reader.QuoteChar;
			}
		}

		public override ReadState ReadState {
			get {
				return reader.ReadState;
			}
		}

		public override string Value {
			get {
				if (IsDefault) {
					DTDAttributeDefinition def = 
						dtd.ElementDecls [currentElement]
						.Attributes [currentAttribute]
						as DTDAttributeDefinition;
					return def.DefaultValue;
				}
				else
					return reader.Value;
			}
		}

		[MonoTODO ("Should consider default xml:lang values.")]
		public override string XmlLang {
			get { return reader.XmlLang; }
		}

		[MonoTODO ("Should consider default xml:space values.")]
		public override XmlSpace XmlSpace {
			get { return reader.XmlSpace; }
		}

	}
}
