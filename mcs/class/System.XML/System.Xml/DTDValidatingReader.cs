using System;
using System.Collections;
using System.Xml;
using System.Xml.Schema;

namespace Mono.Xml
{
	public class DTDValidatingReader : XmlValidatingReader, IXmlLineInfo
	{
		public DTDValidatingReader (XmlReader reader)
			: base (reader)
		{
			this.reader = reader;
			elementStack = new Stack ();
			automataStack = new Stack ();
		}

		XmlReader reader;
		DTDObjectModel dtd;
		Stack elementStack;
		Stack automataStack;
		string currentAttribute;

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

			// It should access attributes by *defined* order.
			if (elementStack.Count == 0)
				return String.Empty;
			DTDAttributeDefinition def = dtd.ElementDecls [elementStack.Peek () as string]
				.Attributes [i] as DTDAttributeDefinition;
			string specified = reader.GetAttribute (def.Name);
			return (specified != null) ? specified : def.DefaultValue;
		}

		public override string GetAttribute (string name)
		{
			if (dtd == null)
				return reader.GetAttribute (name);

			if (elementStack.Count == 0)
				return String.Empty;
			string specified = reader.GetAttribute (name);
			if (specified != null)
				return specified;

			DTDAttributeDefinition def = dtd.ElementDecls [elementStack.Peek () as string]
				.Attributes [name] as DTDAttributeDefinition;
			return def.DefaultValue;
		}

		public override string GetAttribute (string name, string ns)
		{
			if (dtd == null)
				return reader.GetAttribute (name, ns);

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
				return;
			}

			// It should access attributes by *defined* order.
			if (elementStack.Count == 0)
				return;

			DTDAttListDeclaration decl = dtd.ElementDecls [elementStack.Peek () as string].Attributes;
			if (decl.Count <= i)
				throw new ArgumentOutOfRangeException ("i");
			DTDAttributeDefinition def = decl [i] as DTDAttributeDefinition;
			currentAttribute = def.Name;
			// We can ignore return value here.
			reader.MoveToAttribute (def.Name);
		}

		public override bool MoveToAttribute (string name)
		{
			if (dtd == null) {
				bool b = reader.MoveToAttribute (name);
				if (b)
					currentAttribute = reader.Name;
				return b;
			}

			if (elementStack.Count == 0)
				return false;

			DTDAttributeDefinition def = dtd.ElementDecls [elementStack.Peek () as string]
				.Attributes [name] as DTDAttributeDefinition;
			if (def == null)
				return false;
			reader.MoveToAttribute (name);
			currentAttribute = name;
			return true;
		}

		public override bool MoveToAttribute (string name, string ns)
		{
			if (dtd == null) {
				bool b = reader.MoveToAttribute (name, ns);
				if (b)
					currentAttribute = reader.Name;
				return b;
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
			return true;
		}

		public override bool MoveToFirstAttribute ()
		{
			if (dtd == null) {
				bool b = reader.MoveToFirstAttribute ();
				if (b)
					currentAttribute = reader.Name;
				return b;
			}

			// It should access attributes by *defined* order.
			if (elementStack.Count == 0)
				return false;

			DTDAttListDeclaration decl = dtd.ElementDecls [elementStack.Peek () as string].Attributes;
			if (decl.Count == 0)
				return false;
			return reader.MoveToAttribute (decl [0].Name);
		}

		public override bool MoveToNextAttribute ()
		{
			if (dtd == null) {
				bool b = reader.MoveToNextAttribute ();
				if (b)
					currentAttribute = reader.Name;
				return b;
			}

			if (currentAttribute == null)
				return false;

			DTDAttListDeclaration decl = dtd.ElementDecls [elementStack.Peek () as string].Attributes;
			int pos = 0;
			for (; pos < decl.Count; pos++) {
				if (decl [pos].Name == currentAttribute)
					break;
			}
			if (pos == decl.Count)
				return false;

			currentAttribute = decl [pos].Name;
			reader.MoveToAttribute (currentAttribute);
			return true;
		}

		bool inContent;
		DTDAutomata currentAutomata;
		DTDAutomata previousAutomata;
		bool isStandalone;

		[MonoTODO]
		public override bool Read ()
		{
			MoveToElement ();

			bool b = reader.Read ();

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
#if true
				XmlTextReader xmlTextReader = reader as XmlTextReader;
				this.dtd = xmlTextReader.currentSubset;
#else
				// It will support external DTD reader in the future.
				this.dtd = new DTDReader (reader).DTD;
#endif
				break;

			case XmlNodeType.Element:	// startElementDeriv
				// If no schema specification, then skip validation.
				if (currentAutomata == null)
					break;

				previousAutomata = currentAutomata;
				currentAutomata = currentAutomata.TryStartElement (reader.Name);
				if (currentAutomata == DTD.Invalid)
					throw new XmlException (reader as IXmlLineInfo,
						String.Format ("Invalid start element found: {0}", reader.Name));
				DTDElementDeclaration decl = DTD.ElementDecls [reader.Name];
				if (decl == null)
					throw new XmlException (reader as IXmlLineInfo,
						String.Format ("Element {0} is not declared.", reader.Name));
				elementStack.Push (reader.Name);
				automataStack.Push (currentAutomata);
				currentAutomata = decl.ContentModel.GetAutomata ();

				// TODO: check attributes
				if (decl.Attributes == null) {
					if (reader.HasAttributes)
						throw new XmlException (reader as IXmlLineInfo,
							String.Format ("Attributes are found on element {0} while it has no attribute definitions.",
							decl.Name));
				}
				else
					ValidateAttributes (decl);

				// If it is empty element then directly check end element.
				if (reader.IsEmptyElement)
					goto case XmlNodeType.EndElement;
				break;

			case XmlNodeType.EndElement:	// endTagDeriv
				// If no schema specification, then skip validation.
				if (currentAutomata == null)
					break;

				decl = DTD.ElementDecls [reader.Name];
				if (decl == null)
					throw new XmlException (reader as IXmlLineInfo,
						String.Format ("Element {0} is not declared.", reader.Name));

				previousAutomata = currentAutomata;
				// Don't let currentAutomata
				DTDAutomata tmpAutomata = currentAutomata.TryEndElement ();
				if (tmpAutomata == DTD.Invalid)
					throw new XmlException (reader as IXmlLineInfo,
						String.Format ("Invalid end element found: {0}", reader.Name));

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
				if (!elem.IsMixedContent)
					throw new XmlException (reader as IXmlLineInfo,
						String.Format ("Current element {0} does not allow character data content.", elementStack.Peek ()));
				break;
			}
			return true;
		}

		private void ValidateAttributes (DTDElementDeclaration decl)
		{
			Hashtable atts = new Hashtable ();
			if (reader.MoveToFirstAttribute ()) {
				do {
					atts.Add (reader.Name, reader.Value);
				} while (reader.MoveToNextAttribute ());
			}
			foreach (DTDAttributeDefinition def in decl.Attributes.Definitions) {
				string value = atts [def.Name] as string;
				switch (def.OccurenceType) {
				case DTDAttributeOccurenceType.Required:
					if (value == null)
						throw new XmlException (reader as IXmlLineInfo,
							String.Format ("Required attribute {0} in element {1} not found .",
							def.Name, decl.Name));
					break;
				case DTDAttributeOccurenceType.Fixed:
					if (value != def.DefaultValue)
						throw new XmlException (reader as IXmlLineInfo,
							String.Format ("Fixed attribute {0} in element {1} has invalid value {2}.",
							def.Name, decl.Name, reader.Value));
					break;
				}
				atts.Remove (def.Name);
			}
			if (atts.Count > 0) {
				string [] extraneous = new string [atts.Count];
				int i=0;
				foreach (string attribute in atts.Keys)
					extraneous [i++] = attribute;
				throw new XmlException (reader as IXmlLineInfo,
					String.Format ("These attributes are not declared in element {0}: {1}",
					decl.Name, String.Join (",", extraneous)));
			}
			reader.MoveToElement ();
		}

		public override bool ReadAttributeValue ()
		{
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

//		public override void ResolveEntity ()
//		{
//		}

		public override int AttributeCount {
			get {
				if (dtd == null || !inContent)
					return reader.AttributeCount;

				if (elementStack.Count == 0)
					return 0;
				return dtd.ElementDecls [elementStack.Peek () as string]
					.Attributes.Count;
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
			get {
				return (currentAttribute != null
					|| reader.NodeType != XmlNodeType.Attribute) ?
					reader.Depth : reader.Depth + 1;
			}
		}

		[MonoTODO]
		public override bool EOF {
			get { return reader.EOF; }
		}

		public override bool HasValue {
			get {
				return (currentAttribute != null
					|| reader.NodeType != XmlNodeType.Attribute) ?
					reader.HasValue : true;
			}
		}

		public override bool IsDefault {
			get {
				if (currentAttribute == null)
					return false;
				return reader.GetAttribute (currentAttribute) != null;
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

		public override string LocalName {
			get {
				return (currentAttribute != null
					|| reader.NodeType != XmlNodeType.Attribute) ?
					reader.LocalName : currentAttribute;
			}
		}

		public override string Name {
			get {
				return (currentAttribute != null
					|| reader.NodeType != XmlNodeType.Attribute) ?
					reader.Name : currentAttribute;
			}
		}

		public override string NamespaceURI {
			get {
				if (currentAttribute != null)
					return String.Empty;
				return reader.NamespaceURI;
			}
		}

		public override XmlNameTable NameTable {
			get { return reader.NameTable; }
		}

		public override XmlNodeType NodeType {
			get {
				return (currentAttribute != null 
					|| reader.NodeType != XmlNodeType.Attribute) ?
					reader.NodeType : XmlNodeType.Attribute;
			}
		}

		public override string Prefix {
			get {
				if (currentAttribute != null) {
					int colon = currentAttribute.IndexOf (':');
					return colon < 0 ?
						currentAttribute :
						currentAttribute.Substring (0, colon - 1);
				}
				else
					return reader.Prefix;
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

		public override string Value {
			get {
				if (currentAttribute != null || reader.NodeType != XmlNodeType.Attribute)
					return reader.Value;
				return reader.GetAttribute (currentAttribute);
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
