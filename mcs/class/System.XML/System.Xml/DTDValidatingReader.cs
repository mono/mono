using System;
using System.Collections.Specialized;
using System.Collections;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace Mono.Xml
{
	public class DTDValidatingReader : XmlReader, IXmlLineInfo
	{
		public DTDValidatingReader (XmlReader reader)
			: this (reader, null)
		{
		}

		public DTDValidatingReader (XmlReader reader,
			XmlValidatingReader validatingReader)
		{
			entityReaderStack = new Stack ();
			entityReaderNameStack = new Stack ();
			entityReaderDepthStack = new Stack ();
			this.reader = reader;
			this.sourceTextReader = reader as XmlTextReader;
			elementStack = new Stack ();
			automataStack = new Stack ();
			attributes = new StringCollection ();
			attributeValues = new NameValueCollection ();
			this.validatingReader = validatingReader;
			valueBuilder = new StringBuilder ();
			idList = new ArrayList ();
			missingIDReferences = new ArrayList ();
		}

		Stack entityReaderStack;
		Stack entityReaderNameStack;
		Stack entityReaderDepthStack;
		XmlReader reader;
		XmlTextReader sourceTextReader;
		XmlTextReader nextEntityReader;
		DTDObjectModel dtd;
		Stack elementStack;
		Stack automataStack;
		string currentElement;
		string currentAttribute;
		bool consumedAttribute;
		bool insideContent;
		DTDAutomata currentAutomata;
		DTDAutomata previousAutomata;
		bool isStandalone;
		StringCollection attributes;
		NameValueCollection attributeValues;
		StringBuilder valueBuilder;
		ArrayList idList;
		ArrayList missingIDReferences;

		// This field is used to get properties and to raise events.
		XmlValidatingReader validatingReader;

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

			if (nextEntityReader != null) {
				if (DTD == null || DTD.EntityDecls [Name] == null)
					throw new XmlException ("Entity '" + Name + "' was not declared.");
				entityReaderStack.Push (reader);
				entityReaderNameStack.Push (Name);
				entityReaderDepthStack.Push (Depth);
				reader = sourceTextReader = nextEntityReader;
				nextEntityReader = null;
				return Read ();
			} else if (NodeType == XmlNodeType.EndEntity) {
				reader = entityReaderStack.Pop () as XmlReader;
				entityReaderNameStack.Pop ();
				entityReaderDepthStack.Pop ();
				sourceTextReader = reader as XmlTextReader;
				return Read ();
			}

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
				if (entityReaderStack.Count > 0)
					return true;	// EndEntity

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
				if (decl != null)	// i.e. not invalid
					currentAutomata = decl.ContentModel.GetAutomata ();

				DTDAttListDeclaration attList = dtd.AttListDecls [currentElement];
				if (attList != null) {
					// check attributes
					ValidateAttributes (attList);
				} else {
					if (reader.HasAttributes) {
						HandleError (String.Format (
							"Attributes are found on element {0} while it has no attribute definitions.", currentElement),
							XmlSeverityType.Error);
						// FIXME: validation recovery code here.
					}
					SetupValidityIgnorantAttributes ();
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
				// Here element should have been already validated, so
				// if no matching declaration is found, simply ignore.
				if (elem != null && !elem.IsMixedContent) {
					HandleError (String.Format ("Current element {0} does not allow character data content.", elementStack.Peek () as string),
						XmlSeverityType.Error);
					// FIXME: validation recovery code here.
					currentAutomata = previousAutomata;
				}
				break;
			case XmlNodeType.EntityReference:
				if (validatingReader.EntityHandling == EntityHandling.ExpandEntities) {
					ResolveEntity ();
					return Read ();
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

		private void ValidateAttributes (DTDAttListDeclaration decl)
		{
			while (reader.MoveToNextAttribute ()) {
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
						}
						else
							valueBuilder.Append (edecl.EntityValue);
					}
					else
						valueBuilder.Append (reader.Value);
				}
				reader.MoveToElement ();
				reader.MoveToAttribute (attrName);
				string attrValue = valueBuilder.ToString ();
				valueBuilder.Length = 0;
				attributeValues.Add (attrName, attrValue);

				DTDAttributeDefinition def = decl [reader.Name];
				if (def == null) {
					HandleError (String.Format ("Attribute {0} is not declared.", reader.Name),
						XmlSeverityType.Error);
					// FIXME: validation recovery code here.
				} else {
					// check identity constraint
					switch (def.Datatype.TokenizedType) {
					case XmlTokenizedType.ID:
						if (this.idList.Contains (attrValue)) {
							HandleError (String.Format ("Node with ID {0} was already appeared.", attrValue),
								XmlSeverityType.Error);
							// FIXME: validation recovery code here.
						} else {
							if (missingIDReferences.Contains (attrValue))
								missingIDReferences.Remove (attrValue);
							idList.Add (attrValue);
						}
						break;
					case XmlTokenizedType.IDREF:
						if (!idList.Contains (attrValue))
							missingIDReferences.Add (attrValue);
						break;
					case XmlTokenizedType.IDREFS:
						string [] idrefs = def.Datatype.ParseValue (attrValue, NameTable, null) as string [];
						foreach (string idref in idrefs)
							if (!idList.Contains (attrValue))
								missingIDReferences.Add (attrValue);
						break;
					}

					switch (def.OccurenceType) {
						case DTDAttributeOccurenceType.Required:
						if (attrValue == String.Empty) {
							HandleError (String.Format ("Required attribute {0} in element {1} not found .",
								def.Name, decl.Name),
								XmlSeverityType.Error);
							// FIXME: validation recovery code here.
						}
						break;
					case DTDAttributeOccurenceType.Fixed:
						if (attrValue != def.DefaultValue) {
							HandleError (String.Format ("Fixed attribute {0} in element {1} has invalid value {2}.",
								def.Name, decl.Name, attrValue),
								XmlSeverityType.Error);
							// FIXME: validation recovery code here.
						}
						break;
					}
				}
			}
			// Check if all required attributes exist, and/or
			// if there is default values, then add them.
			foreach (DTDAttributeDefinition def in decl.Definitions)
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
			if (consumedAttribute)
				return false;
			if (NodeType == XmlNodeType.Attribute &&
					validatingReader.EntityHandling == EntityHandling.ExpandEntities) {
				consumedAttribute = true;
				return true;
			}
			else if (IsDefault) {
				consumedAttribute = true;
				return true;
			}
			else
				return reader.ReadAttributeValue ();
		}

#if USE_VERSION_1_0
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
#endif

		public override string ReadString ()
		{
			// It seems to be the same as ReadInnerXml(). 
			return base.ReadStringInternal ();
		}

		public override void ResolveEntity ()
		{
			if (NodeType != XmlNodeType.EntityReference)
				throw new InvalidOperationException ("The current node is not an Entity Reference");
			DTDEntityDeclaration entity = DTD != null ? DTD.EntityDecls [Name] as DTDEntityDeclaration : null;

			// MS.NET seems simply ignoring undeclared entity reference here ;-(
			string replacementText =
				(entity != null) ? entity.EntityValue : String.Empty;

			XmlNodeType xmlReaderNodeType =
				(currentAttribute != null) ? XmlNodeType.Attribute : XmlNodeType.Element;

			if (sourceTextReader == null)
				throw new NotSupportedException (
					"Entity resolution from non-XmlTextReader XmlReader could not be supported.");
			XmlParserContext ctx = sourceTextReader.GetInternalParserContext ();

			// FIXME: is seems impossible to get namespaceManager from XmlReader.
//			ctx = new XmlParserContext (document.NameTable,
//				new XmlNamespaceManager (NameTable),
//				DTD,
//				BaseURI, XmlLang, XmlSpace, Encoding.Unicode);
			nextEntityReader = new XmlTextReader (replacementText, xmlReaderNodeType, ctx);
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

		public override int Depth {
			get {
				int baseNum = reader.Depth;
				if (entityReaderDepthStack.Count > 0) {
					baseNum += (int) entityReaderDepthStack.Peek ();
					if (NodeType != XmlNodeType.EndEntity)
						baseNum++;
				}
				return IsDefault ? baseNum + 1 : baseNum;
			}
		}

		public override bool EOF {
			get { return reader.EOF && entityReaderStack.Count == 0; }
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
			get {
				return IsDefault ?
					consumedAttribute ? String.Empty : currentAttribute :
					reader.LocalName;
			}
		}

		public override string Name {
			get {
				return IsDefault ?
					consumedAttribute ? String.Empty : currentAttribute :
					reader.Name;
			}
		}

		public override string NamespaceURI {
			get {
				return IsDefault ?
					consumedAttribute ? String.Empty : String.Empty :
					reader.NamespaceURI;
			}
		}

		public override XmlNameTable NameTable {
			get { return reader.NameTable; }
		}

		public override XmlNodeType NodeType {
			get {
				if (entityReaderStack.Count > 0 && reader.EOF)
					return XmlNodeType.EndEntity;

				// If consumedAttribute is true, then entities must be resolved.
				return consumedAttribute ? XmlNodeType.Text :
					IsDefault ? XmlNodeType.Attribute :
					reader.NodeType;
			}
		}

		public override string Prefix {
			get {
				if (currentAttribute != null && NodeType != XmlNodeType.Attribute)
					return String.Empty;
				return IsDefault ? String.Empty : reader.Prefix;
			}
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
					dtd.AttListDecls [currentElement] [currentAttribute] as DTDAttributeDefinition;
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
				// This check also covers value node of default attributes.
				if (IsDefault) {
					DTDAttributeDefinition def = 
						dtd.AttListDecls [currentElement] [currentAttribute] as DTDAttributeDefinition;
					return sourceTextReader != null && sourceTextReader.Normalization ?
						def.NormalizedDefaultValue : def.DefaultValue;
				}
				// As to this property, MS.NET seems ignorant of EntityHandling...
				else if (NodeType == XmlNodeType.Attribute)// &&
					// validatingReader.EntityHandling == EntityHandling.ExpandEntities)
					return FilterNormalization (attributeValues [currentAttribute]);
				else if (consumedAttribute)
					return FilterNormalization (attributeValues [this.currentAttribute]);
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
