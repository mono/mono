using System;
using System.Collections.Specialized;
using System.Collections;
using System.Text;
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
			this.sourceTextReader = reader as XmlTextReader;
			elementStack = new Stack ();
			automataStack = new Stack ();
			attributes = new StringCollection ();
			attributeValues = new NameValueCollection ();
			this.validatingReader = validatingReader;
		}

		XmlReader reader;
		XmlTextReader sourceTextReader;
		DTDObjectModel dtd;
		Stack elementStack;
		Stack automataStack;
		string currentElement;
		string currentAttribute;
		bool consumedAttribute;
		bool insideContent;
//		bool insideAttributeValue;
		DTDAutomata currentAutomata;
		DTDAutomata previousAutomata;
		bool isStandalone;
		StringCollection attributes;
		NameValueCollection attributeValues;

		XmlValidatingReader validatingReader;
//		ValidationEventHandler handler;

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

			if (attributes.Count <= i)
				throw new IndexOutOfRangeException ("Specified index is out of range: " + i);

			return FilterNormalization (attributeValues [i]);
		}

		public override string GetAttribute (string name)
		{
			if (dtd == null)
				return reader.GetAttribute (name);

			return FilterNormalization (attributeValues [name]);
		}

		public override string GetAttribute (string name, string ns)
		{
			if (dtd == null)
				return reader.GetAttribute (name, ns);

			// FIXME: check whether this way is correct.
			if (ns == String.Empty)
				return GetAttribute (name);
			else
				return FilterNormalization (reader.GetAttribute (name, ns));
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
				consumedAttribute = false;
				return;
			}

			if (currentElement == null)
				return;

			if (attributes.Count > i) {
				currentAttribute = attributes [i];
				consumedAttribute = false;
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
					consumedAttribute = false;
				}
				return b;
			}

			if (currentElement == null)
				return false;

			int idx = attributes.IndexOf (name);
			if (idx >= 0) {
				currentAttribute = name;
				consumedAttribute = false;
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
					consumedAttribute = false;
				}
				return b;
			}

			if (reader.MoveToAttribute (name, ns)) {
				currentAttribute = reader.Name;
				consumedAttribute = false;
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
			consumedAttribute = false;
			return true;
		}

		public override bool MoveToFirstAttribute ()
		{
			if (dtd == null) {
				bool b = reader.MoveToFirstAttribute ();
				if (b) {
					currentAttribute = reader.Name;
					consumedAttribute = false;
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
			consumedAttribute = false;
			return true;
		}

		public override bool MoveToNextAttribute ()
		{
			if (dtd == null) {
				bool b = reader.MoveToNextAttribute ();
				if (b) {
					currentAttribute = reader.Name;
					consumedAttribute = false;
				}
				return b;
			}

			if (currentAttribute == null)
				return MoveToFirstAttribute ();

			int idx = attributes.IndexOf (currentAttribute);
			if (idx + 1 < attributes.Count) {
				reader.MoveToNextAttribute ();
				currentAttribute = attributes [idx + 1];
				consumedAttribute = false;
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
			consumedAttribute = false;
			attributes.Clear ();
			attributeValues.Clear ();

			if (!insideContent && reader.NodeType == XmlNodeType.Element) {
				insideContent = true;
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
				if (currentAutomata == null) {
					SetupValidityIgnorantAttributes ();
					break;
				}

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
				} else
					SetupValidityIgnorantAttributes ();

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

		private void SetupValidityIgnorantAttributes ()
		{
			if (reader.MoveToFirstAttribute ()) {
				// If it was invalid, simply add specified attributes.
				do {
					attributes.Add (reader.Name);
					attributeValues.Add (reader.Name, reader.Value);
				} while (reader.MoveToNextAttribute ());
				reader.MoveToElement ();
			}
		}

		private void HandleError (string message, XmlSeverityType severity)
		{
			if (validatingReader != null &&
				validatingReader.ValidationType == ValidationType.None)
				return;

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

		StringBuilder valueBuilder = new StringBuilder ();
		private void ValidateAttributes (DTDElementDeclaration decl)
		{
			if (reader.MoveToFirstAttribute ()) {
				do {
					string attrName = reader.Name;
					attributes.Add (attrName);
					bool hasError = false;
					while (reader.ReadAttributeValue ()) {
						if (reader.NodeType == XmlNodeType.EntityReference) {
							DTDEntityDeclaration edecl = DTD.EntityDecls [reader.Name];
							if (edecl == null) {
								HandleError (String.Format ("Referenced entity {0} is not declared.", reader.Name),
									XmlSeverityType.Error);
								hasError = true;
								break;
							}
							valueBuilder.Append (edecl.EntityValue);
						}
						else
							valueBuilder.Append (reader.Value);
					}
					reader.MoveToElement ();
					reader.MoveToAttribute (attrName);
					if (hasError) {
						attributeValues.Add (reader.Name, "");
						break;
					}
					attributeValues.Add (reader.Name, valueBuilder.ToString ());
					valueBuilder.Length = 0;

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
					else if (def.DefaultValue != null) {
						attributes.Add (def.Name);
						attributeValues.Add (def.Name, def.DefaultValue);
					}
				}

			reader.MoveToElement ();
		}

		public override bool ReadAttributeValue ()
		{
			if (NodeType == XmlNodeType.Attribute) {
				if (consumedAttribute)
					return false;
				consumedAttribute = true;
				return true;
			}
			else
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
				if (dtd == null || !insideContent)
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
			get {
				// If consumedAttribute is true, then entities must be resolved.
				return consumedAttribute ? XmlNodeType.Text :
					IsDefault ? XmlNodeType.Attribute :
					reader.NodeType;
			}
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

		char [] whitespaceChars = new char [] {' '};
		private string FilterNormalization (string rawValue)
		{
			if (DTD != null &&
					NodeType == XmlNodeType.Attribute &&
					sourceTextReader != null && 
					sourceTextReader.Normalization) {
				DTDAttributeDefinition def = 
					dtd.ElementDecls [currentElement]
					.Attributes [currentAttribute]
					as DTDAttributeDefinition;
				valueBuilder.Append (rawValue);
				valueBuilder.Replace ('\r', ' ');
				valueBuilder.Replace ('\n', ' ');
				valueBuilder.Replace ('\t', ' ');
				try {
					if (def.Datatype.TokenizedType != XmlTokenizedType.CDATA) {
						for (int i=0; i < valueBuilder.Length; i++) {
							if (valueBuilder [i] == ' ') {
								while (++i < valueBuilder.Length && valueBuilder [i] == ' ')
									valueBuilder.Remove (i, 1);
							}
						}
						return valueBuilder.ToString ().Trim (whitespaceChars);
					}
					else
						return valueBuilder.ToString ();
				} finally {
					valueBuilder.Length = 0;
				}
			}
			else
				return rawValue;
		}

		public override string Value {
			get {
				if (IsDefault) {
					DTDAttributeDefinition def = 
						dtd.ElementDecls [currentElement]
						.Attributes [currentAttribute]
						as DTDAttributeDefinition;
					return sourceTextReader != null && sourceTextReader.Normalization ?
						def.NormalizedDefaultValue : def.DefaultValue;
				}
				else if (NodeType == XmlNodeType.Attribute)
					return FilterNormalization (attributeValues [currentAttribute]);
				else
					return FilterNormalization (reader.Value);
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
