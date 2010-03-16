//
// DTDValidatingReader2.cs
//
// Author:
//   Atsushi Enomoto  atsushi@ximian.com
//
// (C)2003 Atsushi Enomoto
// (C)2004-2006 Novell Inc.
//

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

/*

Some notes:

	DTDValidatingReader requires somewhat different ResolveEntity()
	implementation because unlike other readers (XmlTextReaderImpl and
	XmlNodeReaderImpl), DTDValidatingReader manages validation state
	and it must not be held in each entity reader.
	
	Say, if there are such element and entity definitions:

		<!ELEMENT root (child)>
		<!ELEMENT child EMPTY>
		<!ENTITY foo "<child />">

	and an instance

		<root>&foo;</root>

	When the container XmlReader encounters "&foo;", it creates another
	XmlReader for resolved entity "<child/>". However, the generated
	reader must not be another standalone DTDValidatingReader since
	<child/> must be a participant of the container's validation.

	Thus, this reader handles validation, and it holds an
	EntityResolvingXmlReader as its validation source XmlReader.

TODOs:
	IsDefault messes all around the reader, so simplify it.
	isWhitespace/isText/blah mess the code too, so clear it as well.

*/

using System;
using System.Collections;
#if NET_2_0
using System.Collections.Generic;
#endif
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;

#if NET_2_0
using XmlTextReaderImpl = Mono.Xml2.XmlTextReader;
#else
using XmlTextReaderImpl = System.Xml.XmlTextReader;
#endif


namespace Mono.Xml
{
	internal class DTDValidatingReader : XmlReader, IXmlLineInfo, 
#if NET_2_0
		IXmlNamespaceResolver,
#endif
		IHasXmlParserContext, IHasXmlSchemaInfo
	{
		public DTDValidatingReader (XmlReader reader)
			: this (reader, null)
		{
		}

		internal DTDValidatingReader (XmlReader reader,
			XmlValidatingReader validatingReader)
		{
			this.reader = new EntityResolvingXmlReader (reader);
			this.sourceTextReader = reader as XmlTextReader;
			elementStack = new Stack ();
			automataStack = new Stack ();
			attributes = new AttributeSlot [10];
			nsmgr = new XmlNamespaceManager (reader.NameTable);
			this.validatingReader = validatingReader;
			valueBuilder = new StringBuilder ();
			idList = new ArrayList ();
			missingIDReferences = new ArrayList ();
			XmlTextReader xtReader = reader as XmlTextReader;
			if (xtReader != null) {
				resolver = xtReader.Resolver;
			}
			else
				resolver = new XmlUrlResolver ();
		}

		// The primary xml source
		EntityResolvingXmlReader reader;

		// This is used to acquire "Normalization" property which
		// could be dynamically changed.
		XmlTextReader sourceTextReader;

		// This field is used to get properties (such as
		// EntityHandling) and to raise events.
		XmlValidatingReader validatingReader;

		// We hold DTDObjectModel for such case that the source
		// XmlReader does not implement IHasXmlParerContext
		// (especially for non-sys.xml.dll readers).
		DTDObjectModel dtd;

		// Used to resolve entities (as expected)
		XmlResolver resolver;

		// mainly used to retrieve DTDElementDeclaration
		string currentElement;
		AttributeSlot [] attributes;
		int attributeCount;

		// Holds MoveTo*Attribute()/ReadAttributeValue() status.
		int currentAttribute = -1;
		bool consumedAttribute;

		// Ancestor and current node context for each depth.
		Stack elementStack;
		Stack automataStack;
		bool popScope;

		// Validation context.
		bool isStandalone;
		DTDAutomata currentAutomata;
		DTDAutomata previousAutomata;
		ArrayList idList;
		ArrayList missingIDReferences;

		// Holds namespace context. It must not be done in source
		// XmlReader because default attributes could affect on it.
		XmlNamespaceManager nsmgr;

		// Those fields are used to store on-constructing text value.
		// They are required to support entity-mixed text, so they
		// are likely to be moved to EntityResolvingXmlReader.
		string currentTextValue;
		string constructingTextValue;
		bool shouldResetCurrentTextValue;
		bool isSignificantWhitespace;
		bool isWhitespace;
		bool isText;

		// Utility caches.
		Stack attributeValueEntityStack = new Stack ();
		StringBuilder valueBuilder;

		class AttributeSlot
		{
			public string Name;
			public string LocalName;
			public string NS;
			public string Prefix;
			public string Value; // normalized
			public bool IsDefault;

			public void Clear ()
			{
				Prefix = String.Empty;
				LocalName = String.Empty;
				NS = String.Empty;
				Value = String.Empty;
				IsDefault = false;
			}
		}

		internal EntityResolvingXmlReader Source {
			// we cannot return EntityResolvingXmlReader.source
			// since it must check non-wellformedness error
			// (undeclared entity in use).
			get { return reader; }
		}

		public DTDObjectModel DTD {
			get { return dtd; }
		}

		public EntityHandling EntityHandling {
			get { return reader.EntityHandling; }
			set { reader.EntityHandling = value; }
		}

		public override void Close ()
		{
			reader.Close ();
		}

		int GetAttributeIndex (string name)
		{
			for (int i = 0; i < attributeCount; i++)
				if (attributes [i].Name == name)
					return i;
			return -1;
		}

		int GetAttributeIndex (string localName, string ns)
		{
			for (int i = 0; i < attributeCount; i++)
				if (attributes [i].LocalName == localName &&
				    attributes [i].NS == ns)
					return i;
			return -1;
		}

		// We had already done attribute validation, so can ignore name.
		public override string GetAttribute (int i)
		{
			if (currentTextValue != null)
				throw new IndexOutOfRangeException ("Specified index is out of range: " + i);

			if (attributeCount <= i)
				throw new IndexOutOfRangeException ("Specified index is out of range: " + i);
			return attributes [i].Value;
		}

		public override string GetAttribute (string name)
		{
			if (currentTextValue != null)
				return null;

			int i = GetAttributeIndex (name);
			return i < 0 ? null : attributes [i].Value;
		}

		public override string GetAttribute (string name, string ns)
		{
			if (currentTextValue != null)
				return null;

			int i = GetAttributeIndex (name, ns);
			return i < 0 ? null : attributes [i].Value;
		}

#if NET_2_0
		IDictionary<string, string> IXmlNamespaceResolver.GetNamespacesInScope (XmlNamespaceScope scope)
		{
			IXmlNamespaceResolver res = reader as IXmlNamespaceResolver;
			return res != null ? res.GetNamespacesInScope (scope) : new Dictionary<string, string> ();
		}
#endif

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
			string s = nsmgr.LookupNamespace (NameTable.Get (prefix));
			return s == String.Empty ? null : s;
		}

#if NET_2_0
		string IXmlNamespaceResolver.LookupPrefix (string ns)
		{
			IXmlNamespaceResolver res = reader as IXmlNamespaceResolver;
			return res != null ? res.LookupPrefix (ns) : null;
		}
#endif

		public override void MoveToAttribute (int i)
		{
			if (currentTextValue != null)
				throw new IndexOutOfRangeException ("The index is out of range.");

			if (attributeCount <= i)
				throw new IndexOutOfRangeException ("The index is out of range.");

			if (i < reader.AttributeCount) // non-default attribute
				reader.MoveToAttribute (i);
			currentAttribute = i;
			consumedAttribute = false;
			return;
		}

		public override bool MoveToAttribute (string name)
		{
			if (currentTextValue != null)
				return false;

			int i = GetAttributeIndex (name);
			if (i < 0)
				return false;
			if (i < reader.AttributeCount)
				reader.MoveToAttribute (i);
			currentAttribute = i;
			consumedAttribute = false;
			return true;
		}

		public override bool MoveToAttribute (string name, string ns)
		{
			if (currentTextValue != null)
				return false;

			int i = GetAttributeIndex (name, ns);
			if (i < 0)
				return false;
			if (i < reader.AttributeCount)
				reader.MoveToAttribute (i);
			currentAttribute = i;
			consumedAttribute = false;
			return true;
		}

		public override bool MoveToElement ()
		{
			if (currentTextValue != null)
				return false;

			bool b = reader.MoveToElement ();
			if (!b && !IsDefault)
				return false;
			currentAttribute = -1;
			consumedAttribute = false;
			return true;
		}

		public override bool MoveToFirstAttribute ()
		{
			if (currentTextValue != null)
				return false;

			if (attributeCount == 0)
				return false;
			currentAttribute = 0;
			reader.MoveToFirstAttribute ();
			consumedAttribute = false;
			return true;
		}

		public override bool MoveToNextAttribute ()
		{
			if (currentTextValue != null)
				return false;

			if (currentAttribute == -1)
				return MoveToFirstAttribute ();
			if (++currentAttribute == attributeCount) {
				currentAttribute--;
				return false;
			}

			if (currentAttribute < reader.AttributeCount)
				reader.MoveToAttribute (currentAttribute);
			consumedAttribute = false;
			return true;
		}

		public override bool Read ()
		{
			if (currentTextValue != null)
				shouldResetCurrentTextValue = true;

			if (currentAttribute >= 0)
				MoveToElement ();

			currentElement = null;
			currentAttribute = -1;
			consumedAttribute = false;
			attributeCount = 0;
			isWhitespace = false;
			isSignificantWhitespace = false;
			isText = false;

			bool b = ReadContent () || currentTextValue != null;
			if (!b &&
#if NET_2_0
			    (Settings == null || (Settings.ValidationFlags & XmlSchemaValidationFlags.ProcessIdentityConstraints) == 0) &&
#endif
			    this.missingIDReferences.Count > 0) {
				this.HandleError ("Missing ID reference was found: " +
					String.Join (",", missingIDReferences.ToArray (typeof (string)) as string []),
					XmlSeverityType.Error);
				// Don't output the same errors so many times.
				this.missingIDReferences.Clear ();
			}
			if (validatingReader != null)
				EntityHandling = validatingReader.EntityHandling;
			return b;
		}

		private bool ReadContent ()
		{
			switch (reader.ReadState) {
			case ReadState.Closed:
			case ReadState.Error:
			case ReadState.EndOfFile:
				return false;
			}
			if (popScope) {
				nsmgr.PopScope ();
				popScope = false;
				if (elementStack.Count == 0)
				// it reached to the end of document element,
				// so reset to non-validating state.
					currentAutomata = null;
			}

			bool b = !reader.EOF;
			if (shouldResetCurrentTextValue) {
				currentTextValue = null;
				shouldResetCurrentTextValue = false;
			}
			else
				b = reader.Read ();

			if (!b) {
				if (elementStack.Count != 0)
					throw new InvalidOperationException ("Unexpected end of XmlReader.");
				return false;
			}

			return ProcessContent ();
		}

		bool ProcessContent ()
		{
			switch (reader.NodeType) {
			case XmlNodeType.XmlDeclaration:
				FillAttributes ();
				if (GetAttribute ("standalone") == "yes")
					isStandalone = true;
				break;

			case XmlNodeType.DocumentType:
				ReadDoctype ();
				break;

			case XmlNodeType.Element:
				if (constructingTextValue != null) {
					currentTextValue = constructingTextValue;
					constructingTextValue = null;
					if (isWhitespace)
						ValidateWhitespaceNode ();
					return true;
				}
				ProcessStartElement ();
				break;

			case XmlNodeType.EndElement:
				if (constructingTextValue != null) {
					currentTextValue = constructingTextValue;
					constructingTextValue = null;
					return true;
				}
				ProcessEndElement ();
				break;

			case XmlNodeType.CDATA:
				isSignificantWhitespace = isWhitespace = false;
				isText = true;

				ValidateText ();

				if (currentTextValue != null) {
					currentTextValue = constructingTextValue;
					constructingTextValue = null;
					return true;
				}
				break;
			case XmlNodeType.SignificantWhitespace:
				if (!isText)
					isSignificantWhitespace = true;
				isWhitespace = false;
				goto case XmlNodeType.DocumentFragment;
			case XmlNodeType.Text:
				isWhitespace = isSignificantWhitespace = false;
				isText = true;
				goto case XmlNodeType.DocumentFragment;
			case XmlNodeType.DocumentFragment:
				// it should not happen, but in case if
				// XmlReader really returns it, just ignore.
				if (reader.NodeType == XmlNodeType.DocumentFragment)
					break;

				ValidateText ();

				break;
			case XmlNodeType.Whitespace:
				if (!isText && !isSignificantWhitespace)
					isWhitespace = true;
				goto case XmlNodeType.DocumentFragment;
			}
			if (isWhitespace)
				ValidateWhitespaceNode ();
			currentTextValue = constructingTextValue;
			constructingTextValue = null;
			return true;
		}

		private void FillAttributes ()
		{
			if (reader.MoveToFirstAttribute ()) {
				do {
					AttributeSlot slot = GetAttributeSlot ();
					slot.Name = reader.Name;
					slot.LocalName = reader.LocalName;
					slot.Prefix = reader.Prefix;
					slot.NS = reader.NamespaceURI;
					slot.Value = reader.Value;
				} while (reader.MoveToNextAttribute ());
				reader.MoveToElement ();
			}
		}

		private void ValidateText ()
		{
			if (currentAutomata == null)
				return;

			DTDElementDeclaration elem = null;
			if (elementStack.Count > 0)
				elem = dtd.ElementDecls [elementStack.Peek () as string];
			// Here element should have been already validated, so
			// if no matching declaration is found, simply ignore.
			if (elem != null && !elem.IsMixedContent && !elem.IsAny && !isWhitespace) {
				HandleError (String.Format ("Current element {0} does not allow character data content.", elementStack.Peek () as string),
					XmlSeverityType.Error);
				currentAutomata = previousAutomata;
			}
		}

		private void ValidateWhitespaceNode ()
		{
			// VC Standalone Document Declaration (2.9)
			if (this.isStandalone && DTD != null && elementStack.Count > 0) {
				DTDElementDeclaration elem = DTD.ElementDecls [elementStack.Peek () as string];
				if (elem != null && !elem.IsInternalSubset && !elem.IsMixedContent && !elem.IsAny && !elem.IsEmpty)
					HandleError ("In a standalone document, whitespace cannot appear in an element which is declared to contain only element children.", XmlSeverityType.Error);
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
			HandleError (ex, severity);
		}

		private void HandleError (XmlSchemaException ex, XmlSeverityType severity)
		{
			if (validatingReader != null &&
				validatingReader.ValidationType == ValidationType.None)
				return;

			if (validatingReader != null)
				this.validatingReader.OnValidationEvent (this,
					new ValidationEventArgs (ex, ex.Message, severity));
			else if (severity == XmlSeverityType.Error)
				throw ex;
		}

		private void ValidateAttributes (DTDAttListDeclaration decl, bool validate)
		{
			DtdValidateAttributes (decl, validate);

			for (int i = 0; i < attributeCount; i++) {
				AttributeSlot slot = attributes [i];
				if (slot.Name == "xmlns" || slot.Prefix == "xmlns")
					nsmgr.AddNamespace (
						slot.Prefix == "xmlns" ? slot.LocalName : String.Empty,
						slot.Value);
			}

			for (int i = 0; i < attributeCount; i++) {
				AttributeSlot slot = attributes [i];
				if (slot.Name == "xmlns")
					slot.NS = XmlNamespaceManager.XmlnsXmlns;
				else if (slot.Prefix.Length > 0)
					slot.NS = LookupNamespace (slot.Prefix);
				else
					slot.NS = String.Empty;
			}
		}

		AttributeSlot GetAttributeSlot ()
		{
			if (attributeCount == attributes.Length) {
				AttributeSlot [] tmp = new AttributeSlot [attributeCount << 1];
				Array.Copy (attributes, tmp, attributeCount);
				attributes = tmp;
			}
			if (attributes [attributeCount] == null)
				attributes [attributeCount] = new AttributeSlot ();
			AttributeSlot slot = attributes [attributeCount];
			slot.Clear ();
			attributeCount++;
			return slot;
		}

		private void DtdValidateAttributes (DTDAttListDeclaration decl, bool validate)
		{
			while (reader.MoveToNextAttribute ()) {
				string attrName = reader.Name;
				AttributeSlot slot = GetAttributeSlot ();
				slot.Name = reader.Name;
				slot.LocalName = reader.LocalName;
				slot.Prefix = reader.Prefix;
				XmlReader targetReader = reader;
				string attrValue = String.Empty;
				// For attribute node, it always resolves
				// entity references on attributes.
				while (attributeValueEntityStack.Count >= 0) {
					if (!targetReader.ReadAttributeValue ()) {
						if (attributeValueEntityStack.Count > 0) {
							targetReader = attributeValueEntityStack.Pop () as XmlReader;
							continue;
						} else
							break;
					}
					switch (targetReader.NodeType) {
					case XmlNodeType.EntityReference:
						DTDEntityDeclaration edecl = DTD.EntityDecls [targetReader.Name];
						if (edecl == null) {
							HandleError (String.Format ("Referenced entity {0} is not declared.", targetReader.Name),
								XmlSeverityType.Error);
						} else {
							XmlTextReader etr = new XmlTextReader (edecl.EntityValue, XmlNodeType.Attribute, ParserContext);
							attributeValueEntityStack.Push (targetReader);
							targetReader = etr;
							continue;
						}
						break;
					case XmlNodeType.EndEntity:
						break;
					default:
						attrValue += targetReader.Value;
						break;
					}
				}
				
				reader.MoveToElement ();
				reader.MoveToAttribute (attrName);
				slot.Value = FilterNormalization (attrName, attrValue);

				if (!validate)
					continue;

				// Validation

				DTDAttributeDefinition def = decl [reader.Name];
				if (def == null) {
					HandleError (String.Format ("Attribute {0} is not declared.", reader.Name),
						XmlSeverityType.Error);
					continue;
				}

				// check enumeration constraint
				if (def.EnumeratedAttributeDeclaration.Count > 0)
					if (!def.EnumeratedAttributeDeclaration.Contains (slot.Value))
						HandleError (String.Format ("Attribute enumeration constraint error in attribute {0}, value {1}.",
							reader.Name, attrValue), XmlSeverityType.Error);
				if (def.EnumeratedNotations.Count > 0)
					if (!def.EnumeratedNotations.Contains (
						slot.Value))
						HandleError (String.Format ("Attribute notation enumeration constraint error in attribute {0}, value {1}.",
							reader.Name, attrValue), XmlSeverityType.Error);

				// check type constraint
				string normalized = null;
				if (def.Datatype != null)
					normalized = FilterNormalization (def.Name, attrValue);
				else
					normalized = attrValue;
				DTDEntityDeclaration ent;

				// Common process to get list value
				string [] list = null;
				switch (def.Datatype.TokenizedType) {
				case XmlTokenizedType.IDREFS:
				case XmlTokenizedType.ENTITIES:
				case XmlTokenizedType.NMTOKENS:
					try {
						list = def.Datatype.ParseValue (normalized, NameTable, null) as string [];
					} catch (Exception) {
						HandleError ("Attribute value is invalid against its data type.", XmlSeverityType.Error);
						list = new string [0];
					}
					break;
				default:
					try {
						def.Datatype.ParseValue (normalized, NameTable, null);
					} catch (Exception ex) {
						HandleError (String.Format ("Attribute value is invalid against its data type '{0}'. {1}", def.Datatype, ex.Message), XmlSeverityType.Error);
					}
					break;
				}

				switch (def.Datatype.TokenizedType) {
				case XmlTokenizedType.ID:
					if (this.idList.Contains (normalized)) {
						HandleError (String.Format ("Node with ID {0} was already appeared.", attrValue),
							XmlSeverityType.Error);
					} else {
						if (missingIDReferences.Contains (normalized))
							missingIDReferences.Remove (normalized);
						idList.Add (normalized);
					}
					break;
				case XmlTokenizedType.IDREF:
					if (!idList.Contains (normalized))
						missingIDReferences.Add (normalized);
					break;
				case XmlTokenizedType.IDREFS:
					for (int i = 0; i < list.Length; i++) {
						string idref = list [i];
						if (!idList.Contains (idref))
							missingIDReferences.Add (idref);
					}
					break;
				case XmlTokenizedType.ENTITY:
					ent = dtd.EntityDecls [normalized];
					if (ent == null)
						HandleError ("Reference to undeclared entity was found in attribute: " + reader.Name + ".", XmlSeverityType.Error);
					else if (ent.NotationName == null)
						HandleError ("The entity specified by entity type value must be an unparsed entity. The entity definition has no NDATA in attribute: " + reader.Name + ".", XmlSeverityType.Error);
					break;
				case XmlTokenizedType.ENTITIES:
					for (int i = 0; i < list.Length; i++) {
						string entref = list [i];
						ent = dtd.EntityDecls [FilterNormalization (reader.Name, entref)];
						if (ent == null)
							HandleError ("Reference to undeclared entity was found in attribute: " + reader.Name + ".", XmlSeverityType.Error);
						else if (ent.NotationName == null)
							HandleError ("The entity specified by ENTITIES type value must be an unparsed entity. The entity definition has no NDATA in attribute: " + reader.Name + ".", XmlSeverityType.Error);
					}
					break;
//				case XmlTokenizedType.NMTOKEN: nothing to do
//				case XmlTokenizedType.NMTOKENS: nothing to do
				}

				if (isStandalone && !def.IsInternalSubset && 
					attrValue != normalized)
					HandleError ("In standalone document, attribute value characters must not be checked against external definition.", XmlSeverityType.Error);

				if (def.OccurenceType == 
					DTDAttributeOccurenceType.Fixed &&
					attrValue != def.DefaultValue)
					HandleError (String.Format ("Fixed attribute {0} in element {1} has invalid value {2}.",
						def.Name, decl.Name, attrValue),
						XmlSeverityType.Error);
			}

			if (validate)
				VerifyDeclaredAttributes (decl);

			MoveToElement ();
		}

		void ReadDoctype ()
		{
			FillAttributes ();

			IHasXmlParserContext ctx = reader as IHasXmlParserContext;
			if (ctx != null)
				dtd = ctx.ParserContext.Dtd;
			if (dtd == null) {
				XmlTextReaderImpl xmlTextReader = new XmlTextReaderImpl ("", XmlNodeType.Document, null);
				xmlTextReader.XmlResolver = resolver;
				xmlTextReader.GenerateDTDObjectModel (reader.Name,
					reader ["PUBLIC"], reader ["SYSTEM"], reader.Value);
				dtd = xmlTextReader.DTD;
			}
			currentAutomata = dtd.RootAutomata;

			// Validity Constraint Check.
			for (int i = 0; i < DTD.Errors.Length; i++)
				HandleError (DTD.Errors [i].Message, XmlSeverityType.Error);

			// NData target exists.
			foreach (DTDEntityDeclaration ent in dtd.EntityDecls.Values)
				if (ent.NotationName != null && dtd.NotationDecls [ent.NotationName] == null)
					this.HandleError ("Target notation was not found for NData in entity declaration " + ent.Name + ".",
						XmlSeverityType.Error);
			// NOTATION exists for attribute default values
			foreach (DTDAttListDeclaration attListIter in dtd.AttListDecls.Values) {
				foreach (DTDAttributeDefinition def in attListIter.Definitions) {
					if (def.Datatype.TokenizedType != XmlTokenizedType.NOTATION)
						continue;
					foreach (string notation in def.EnumeratedNotations)
						if (dtd.NotationDecls [notation] == null)
							this.HandleError ("Target notation was not found for NOTATION typed attribute default " + def.Name + ".",
								XmlSeverityType.Error);
				}
			}
		}

		void ProcessStartElement ()
		{
			nsmgr.PushScope ();
			popScope = reader.IsEmptyElement;
			elementStack.Push (reader.Name);

			currentElement = Name;

			// If no DTD, skip validation.
			if (currentAutomata == null) {
				ValidateAttributes (null, false);
				if (reader.IsEmptyElement)
					ProcessEndElement ();
				return;
			}

			// StartElementDeriv

			previousAutomata = currentAutomata;
			currentAutomata = currentAutomata.TryStartElement (reader.Name);
			if (currentAutomata == DTD.Invalid) {
				HandleError (String.Format ("Invalid start element found: {0}", reader.Name),
					XmlSeverityType.Error);
				currentAutomata = previousAutomata;
			}

			DTDElementDeclaration elem =
				DTD.ElementDecls [reader.Name];
			if (elem == null) {
				HandleError (String.Format ("Element {0} is not declared.", reader.Name),
					XmlSeverityType.Error);
				currentAutomata = previousAutomata;
			}

			automataStack.Push (currentAutomata);
			if (elem != null)	// i.e. not invalid
				currentAutomata = elem.ContentModel.GetAutomata ();

			DTDAttListDeclaration attList = dtd.AttListDecls [currentElement];
			if (attList != null) {
				// check attributes
				ValidateAttributes (attList, true);
				currentAttribute = -1;
			} else {
				if (reader.HasAttributes) {
					HandleError (String.Format (
						"Attributes are found on element {0} while it has no attribute definitions.", currentElement),
						XmlSeverityType.Error);
				}
				// SetupValidityIgnorantAttributes ();
				ValidateAttributes (null, false);
			}
			// If it is empty element then directly check end element.
			if (reader.IsEmptyElement)
				ProcessEndElement ();
		}

		void ProcessEndElement ()
		{
			popScope = true;
			elementStack.Pop ();

			// If no schema specification, then skip validation.
			if (currentAutomata == null)
				return;

			// EndElementDeriv
			DTDElementDeclaration elem =
				DTD.ElementDecls [reader.Name];
			if (elem == null) {
				HandleError (String.Format ("Element {0} is not declared.", reader.Name),
					XmlSeverityType.Error);
			}

			previousAutomata = currentAutomata;
			// Don't let currentAutomata
			DTDAutomata tmpAutomata = currentAutomata.TryEndElement ();
			if (tmpAutomata == DTD.Invalid) {
				HandleError (String.Format ("Invalid end element found: {0}", reader.Name),
					XmlSeverityType.Error);
				currentAutomata = previousAutomata;
			}

			currentAutomata = automataStack.Pop () as DTDAutomata;
		}

		void VerifyDeclaredAttributes (DTDAttListDeclaration decl)
		{
			// Check if all required attributes exist, and/or
			// if there is default values, then add them.
			for (int i = 0; i < decl.Definitions.Count; i++) {
				DTDAttributeDefinition def = (DTDAttributeDefinition) decl.Definitions [i];
				bool exists = false;
				for (int a = 0; a < attributeCount; a++) {
					if (attributes [a].Name == def.Name) {
						exists = true;
						break;
					}
				}
				if (exists)
					continue;

				if (def.OccurenceType == DTDAttributeOccurenceType.Required) {
					HandleError (String.Format ("Required attribute {0} in element {1} not found .",
						def.Name, decl.Name),
						XmlSeverityType.Error);
					continue;
				}

				else if (def.DefaultValue == null)
					continue;

				if (this.isStandalone && !def.IsInternalSubset)
					HandleError ("In standalone document, external default value definition must not be applied.", XmlSeverityType.Error);

				switch (validatingReader.ValidationType) {
				case ValidationType.Auto:
					if (validatingReader.Schemas.Count == 0)
						goto case ValidationType.DTD;
					break;
				case ValidationType.DTD:
				case ValidationType.None:
					// Other than them, ignore DTD defaults.
					AttributeSlot slot = GetAttributeSlot ();
					slot.Name = def.Name;
					int colonAt = def.Name.IndexOf (':');
					slot.LocalName = colonAt < 0 ? def.Name :
						def.Name.Substring (colonAt + 1);
					string prefix = colonAt < 0 ?
						String.Empty :
						def.Name.Substring (0, colonAt);
					slot.Prefix = prefix;
					slot.Value = def.DefaultValue;
					slot.IsDefault = true;
					break;
				}
			}
		}

#if MOONLIGHT
		internal
#else
		public
#endif
		override bool ReadAttributeValue ()
		{
			if (consumedAttribute)
				return false;
			if (NodeType == XmlNodeType.Attribute &&
			    EntityHandling == EntityHandling.ExpandEntities) {
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

#if MOONLIGHT
		internal
#else
		public
#endif
		override void ResolveEntity ()
		{
			reader.ResolveEntity ();
		}

		public override int AttributeCount {
			get {
				if (currentTextValue != null)
					return 0;

				return attributeCount;
			}
		}

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
				if (currentTextValue != null && reader.NodeType == XmlNodeType.EndElement)
					baseNum++;

				return IsDefault ? baseNum + 1 : baseNum;
			}
		}

		public override bool EOF {
			get { return reader.EOF; }
		}

#if MOONLIGHT
		internal
#else
		public
#endif
		override bool HasValue {
			get {
				return currentAttribute >= 0 ? true :
					currentTextValue != null ? true :
					reader.HasValue;
			}
		}

		public override bool IsDefault {
			get {
				if (currentTextValue != null)
					return false;
				if (currentAttribute == -1)
					return false;
				return attributes [currentAttribute].IsDefault;
			}
		}

		public override bool IsEmptyElement {
			get {
				if (currentTextValue != null)
					return false;
				return reader.IsEmptyElement;
			}
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
				if (currentTextValue != null || consumedAttribute)
					return String.Empty;
				else if (NodeType == XmlNodeType.Attribute)
					return attributes [currentAttribute].LocalName;
				else
					return reader.LocalName;
			}
		}

		public override string Name {
			get {
				if (currentTextValue != null || consumedAttribute)
					return String.Empty;
				else if (NodeType == XmlNodeType.Attribute)
					return attributes [currentAttribute].Name;
				else
					return reader.Name;
			}
		}

		public override string NamespaceURI {
			get {
				if (currentTextValue != null || consumedAttribute)
					return String.Empty;
				switch (NodeType) {
				case XmlNodeType.Attribute:
					return (string) attributes [currentAttribute].NS;
				case XmlNodeType.Element:
				case XmlNodeType.EndElement:
					return nsmgr.LookupNamespace (Prefix);
				default:
					return String.Empty;
				}
			}
		}

		public override XmlNameTable NameTable {
			get { return reader.NameTable; }
		}

		public override XmlNodeType NodeType {
			get {
				if (currentTextValue != null)
					return isSignificantWhitespace ? XmlNodeType.SignificantWhitespace :
						isWhitespace ? XmlNodeType.Whitespace :
						XmlNodeType.Text;

				// If consumedAttribute is true, then entities must be resolved.
				return consumedAttribute ? XmlNodeType.Text :
					IsDefault ? XmlNodeType.Attribute :
					reader.NodeType;
			}
		}

		public XmlParserContext ParserContext {
			get { return XmlSchemaUtil.GetParserContext (reader); }
		}

		public override string Prefix {
			get {
				if (currentTextValue != null || consumedAttribute)
					return String.Empty;
				else if (NodeType == XmlNodeType.Attribute)
					return attributes [currentAttribute].Prefix;
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
				if (reader.ReadState == ReadState.EndOfFile && currentTextValue != null)
					return ReadState.Interactive;
				return reader.ReadState;
			}
		}

		public object SchemaType {
			get {
				if (DTD == null || currentAttribute == -1 ||
				    currentElement == null)
					return null;
				DTDAttListDeclaration decl =
					DTD.AttListDecls [currentElement];
				DTDAttributeDefinition def =
					decl != null ? decl [attributes [currentAttribute].Name] : null;
				return def != null ? def.Datatype : null;
			}
		}

		char [] whitespaceChars = new char [] {' '};
		private string FilterNormalization (string attrName, string rawValue)
		{
			if (DTD == null || sourceTextReader == null ||
			    !sourceTextReader.Normalization)
				return rawValue;

			DTDAttributeDefinition def = 
				dtd.AttListDecls [currentElement].Get (attrName);
			valueBuilder.Append (rawValue);
			valueBuilder.Replace ('\r', ' ');
			valueBuilder.Replace ('\n', ' ');
			valueBuilder.Replace ('\t', ' ');
			try {
				if (def == null || def.Datatype.TokenizedType == XmlTokenizedType.CDATA)
					return valueBuilder.ToString ();
				for (int i=0; i < valueBuilder.Length; i++) {
					if (valueBuilder [i] != ' ')
						continue;
					while (++i < valueBuilder.Length && valueBuilder [i] == ' ')
						valueBuilder.Remove (i, 1);
				}
				return valueBuilder.ToString ().Trim (whitespaceChars);
			} finally {
				valueBuilder.Length = 0;
			}
		}

		// LAMESPEC: When source XmlTextReader.Normalize is true, then
		// every Attribute node is normalized. However, corresponding
		// Values of attribute value Text nodes are not.
		public override string Value {
			get {
				if (currentTextValue != null)
					return currentTextValue;
				// As to this property, MS.NET seems ignorant of EntityHandling...
				else if (NodeType == XmlNodeType.Attribute
					// It also covers default attribute text.
 					|| consumedAttribute)
					return attributes [currentAttribute].Value;
				else
					return reader.Value;
			}
		}

		public override string XmlLang {
			get {
				string val = this ["xml:lang"];
				return val != null ? val : reader.XmlLang;
			}
		}

		internal XmlResolver Resolver {
			get { return resolver; }
		}

		public XmlResolver XmlResolver {
			set {
				if (dtd != null)
					dtd.XmlResolver = value;
				resolver = value;
			}
		}

		public override XmlSpace XmlSpace {
			get {
				string val = this ["xml:space"];
				switch (val) {
				case "preserve":
					return XmlSpace.Preserve;
				case "default":
					return XmlSpace.Default;
				default:
					return reader.XmlSpace;
				}
			}
		}

	}
}
