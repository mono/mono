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
			IHasXmlParserContext container = reader as IHasXmlParserContext;
			this.reader = new EntityResolvingXmlReader (reader,
				container.ParserContext);
			this.sourceTextReader = reader as XmlTextReader;
			elementStack = new Stack ();
			automataStack = new Stack ();
			attributes = new ArrayList ();
			attributeValues = new Hashtable ();
			attributeLocalNames = new Hashtable ();
			attributeNamespaces = new Hashtable ();
			attributePrefixes = new Hashtable ();
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

		EntityResolvingXmlReader reader;
		XmlTextReader sourceTextReader;
		DTDObjectModel dtd;
		Stack elementStack;
		Stack automataStack;
		string currentElement;
		string currentAttribute;
		string currentTextValue;
		string constructingTextValue;
		bool shouldResetCurrentTextValue;
		bool consumedAttribute;
		bool insideContent;
		DTDAutomata currentAutomata;
		DTDAutomata previousAutomata;
		bool isStandalone;
		ArrayList attributes;
		Hashtable attributeValues;
		Hashtable attributeLocalNames;
		Hashtable attributeNamespaces;
		Hashtable attributePrefixes;
		XmlNamespaceManager nsmgr;
		StringBuilder valueBuilder;
		ArrayList idList;
		ArrayList missingIDReferences;
		XmlResolver resolver;
		bool isSignificantWhitespace;
		bool isWhitespace;
		bool isText;
		bool dontResetTextType;
		bool popScope;

		// This field is used to get properties and to raise events.
		XmlValidatingReader validatingReader;

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

		// We had already done attribute validation, so can ignore name.
		public override string GetAttribute (int i)
		{
			if (currentTextValue != null)
				throw new IndexOutOfRangeException ("Specified index is out of range: " + i);

			if (dtd == null)
				return reader.GetAttribute (i);

			if (attributes.Count <= i)
				throw new IndexOutOfRangeException ("Specified index is out of range: " + i);

			string attrName = (string) attributes [i];
			return FilterNormalization (attrName, (string) attributeValues [attrName]);
		}

		public override string GetAttribute (string name)
		{
			if (currentTextValue != null)
				return null;

			if (dtd == null)
				return reader.GetAttribute (name);

			return FilterNormalization (name, (string) attributeValues [name]);
		}

		public override string GetAttribute (string name, string ns)
		{
			if (currentTextValue != null)
				return null;

			if (dtd == null)
				return reader.GetAttribute (name, ns);

			return reader.GetAttribute ((string) attributeLocalNames [name], ns);
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

			if (dtd == null) {
				reader.MoveToAttribute (i);
				currentAttribute = reader.Name;
				consumedAttribute = false;
				return;
			}

			if (currentElement == null)
				throw new IndexOutOfRangeException ("The index is out of range.");

			if (attributes.Count > i) {
				if (reader.AttributeCount > i)
					reader.MoveToAttribute (i);
				currentAttribute = (string) attributes [i];
				consumedAttribute = false;
				return;
			} else
				throw new IndexOutOfRangeException ("The index is out of range.");
		}

		public override bool MoveToAttribute (string name)
		{
			if (currentTextValue != null)
				return false;

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
			if (currentTextValue != null)
				return false;

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

			for (int i = 0; i < attributes.Count; i++) {
				string iter = (string) attributes [i];
				if ((string) attributeLocalNames [iter] == name)
					return MoveToAttribute (iter);
			}
			return false;
		}

		public override bool MoveToElement ()
		{
			if (currentTextValue != null)
				return false;

			bool b = reader.MoveToElement ();
			if (!b && !IsDefault)
				return false;
			currentAttribute = null;
			consumedAttribute = false;
			return true;
		}

		public override bool MoveToFirstAttribute ()
		{
			if (currentTextValue != null)
				return false;

			if (dtd == null) {
				bool b = reader.MoveToFirstAttribute ();
				if (b) {
					currentAttribute = reader.Name;
					consumedAttribute = false;
				}
				return b;
			}

			if (attributes.Count == 0)
				return false;
			currentAttribute = (string) attributes [0];
			reader.MoveToAttribute (currentAttribute);
			consumedAttribute = false;
			return true;
		}

		public override bool MoveToNextAttribute ()
		{
			if (currentTextValue != null)
				return false;

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
				currentAttribute = (string) attributes [idx + 1];
				reader.MoveToAttribute (currentAttribute);
				consumedAttribute = false;
				return true;
			} else
				return false;
		}

		/*
		private void OnValidationEvent (object o, ValidationEventArgs e)
		{
			this.HandleError (e.Exception, e.Severity);
		}
		*/

		public override bool Read ()
		{
			if (currentTextValue != null)
				shouldResetCurrentTextValue = true;

			MoveToElement ();

			currentElement = null;
			currentAttribute = null;
			consumedAttribute = false;
			attributes.Clear ();
			attributeLocalNames.Clear ();
			attributeValues.Clear ();
			attributeNamespaces.Clear ();
			attributePrefixes.Clear ();
			isWhitespace = false;
			isSignificantWhitespace = false;
			isText = false;
			dontResetTextType = false;

			bool b = ReadContent () || currentTextValue != null;
			if (!b && this.missingIDReferences.Count > 0) {
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
			if (reader.EOF)
				return false;
			if (popScope) {
				nsmgr.PopScope ();
				popScope = false;
			}

			bool b = !reader.EOF;
			if (shouldResetCurrentTextValue) {
				currentTextValue = null;
				shouldResetCurrentTextValue = false;
			}
			else
				b = reader.Read ();

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

			DTDElementDeclaration elem = null;

			switch (reader.NodeType) {
			case XmlNodeType.XmlDeclaration:
				if (GetAttribute ("standalone") == "yes")
					isStandalone = true;
				ValidateAttributes (null, false);
				break;

			case XmlNodeType.DocumentType:
//				XmlTextReader xmlTextReader = reader as XmlTextReader;
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

				// Validity Constraints Check.
				if (DTD.Errors.Length > 0)
					for (int i = 0; i < DTD.Errors.Length; i++)
						HandleError (DTD.Errors [i].Message, XmlSeverityType.Error);

				// NData target exists.
				foreach (DTDEntityDeclaration ent in dtd.EntityDecls.Values)
					if (ent.NotationName != null && dtd.NotationDecls [ent.NotationName] == null)
						this.HandleError ("Target notation was not found for NData in entity declaration " + ent.Name + ".",
							XmlSeverityType.Error);
				// NOTATION exists for attribute default values
				foreach (DTDAttListDeclaration attListIter in dtd.AttListDecls.Values)
					foreach (DTDAttributeDefinition def in attListIter.Definitions)
						if (def.Datatype.TokenizedType == XmlTokenizedType.NOTATION) {
							foreach (string notation in def.EnumeratedNotations)
								if (dtd.NotationDecls [notation] == null)
									this.HandleError ("Target notation was not found for NOTATION typed attribute default " + def.Name + ".",
										XmlSeverityType.Error);
						}

				break;

			case XmlNodeType.Element:
				nsmgr.PushScope ();
				popScope = reader.IsEmptyElement;
				if (constructingTextValue != null) {
					currentTextValue = constructingTextValue;
					constructingTextValue = null;
					if (isWhitespace)
						ValidateWhitespaceNode ();
					return true;
				}
				elementStack.Push (reader.Name);
				// startElementDeriv
				// If no schema specification, then skip validation.
				if (currentAutomata == null) {
					ValidateAttributes (null, false);
					if (reader.IsEmptyElement)
						goto case XmlNodeType.EndElement;
					break;
				}

				previousAutomata = currentAutomata;
				currentAutomata = currentAutomata.TryStartElement (reader.Name);
				if (currentAutomata == DTD.Invalid) {
					HandleError (String.Format ("Invalid start element found: {0}", reader.Name),
						XmlSeverityType.Error);
					currentAutomata = previousAutomata;
				}
				elem = DTD.ElementDecls [reader.Name];
				if (elem == null) {
					HandleError (String.Format ("Element {0} is not declared.", reader.Name),
						XmlSeverityType.Error);
					currentAutomata = previousAutomata;
				}

				currentElement = Name;
				automataStack.Push (currentAutomata);
				if (elem != null)	// i.e. not invalid
					currentAutomata = elem.ContentModel.GetAutomata ();

				DTDAttListDeclaration attList = dtd.AttListDecls [currentElement];
				if (attList != null) {
					// check attributes
					ValidateAttributes (attList, true);
					currentAttribute = null;
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
					goto case XmlNodeType.EndElement;
				break;

			case XmlNodeType.EndElement:
				if (constructingTextValue != null) {
					currentTextValue = constructingTextValue;
					constructingTextValue = null;
					return true;
				}
				popScope = true;
				elementStack.Pop ();
				// endElementDeriv
				// If no schema specification, then skip validation.
				if (currentAutomata == null)
					break;

				elem = DTD.ElementDecls [reader.Name];
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
				dontResetTextType = true;
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
			MoveToElement ();
			return true;
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
					HandleError ("In standalone document, whitespace cannot appear in an element whose declaration explicitly contains child content model, not Mixed content.", XmlSeverityType.Error);
			}
		}

		private XmlException NotWFError (string message)
		{
			return new XmlException (this as IXmlLineInfo, BaseURI, message);
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

		Stack attributeValueEntityStack = new Stack ();

		private void ValidateAttributes (DTDAttListDeclaration decl, bool validate)
		{
			DtdValidateAttributes (decl, validate);

			foreach (string attr in attributes)
				if (attr == "xmlns" ||
					String.CompareOrdinal (attr, 0, "xmlns:", 0, 6) == 0)
					nsmgr.AddNamespace (
						attr == "xmlns" ? String.Empty : (string) attributeLocalNames [attr],
						(string) attributeValues [attr]);

			foreach (string attr in attributes) {
				string prefix = attr == "xmlns" ? "xmlns" : attributePrefixes [attr] as string;
				if (prefix == String.Empty)
					attributeNamespaces.Add (attr, String.Empty);
				else
					attributeNamespaces.Add (attr, LookupNamespace (prefix));
			}
		}

		private void DtdValidateAttributes (DTDAttListDeclaration decl, bool validate)
		{
			while (reader.MoveToNextAttribute ()) {
				string attrName = reader.Name;
				this.currentAttribute = attrName;
				attributes.Add (attrName);
				attributeLocalNames.Add (attrName, reader.LocalName);
				attributePrefixes.Add (attrName, reader.Prefix);
				XmlReader targetReader = reader;
				string attrValue = null;
				// It always resolves entity references on attributes (documented as such).
//				if (currentEntityHandling == EntityHandling.ExpandCharEntities)
//					attrValue = reader.Value;
//				else
				{
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
							if (attrValue != null) {
								valueBuilder.Append (attrValue);
								attrValue = null;
							}
							
							if (valueBuilder.Length != 0)
								valueBuilder.Append (targetReader.Value);
							else
								attrValue = targetReader.Value;
							
							break;
						}
					}
					
					if (attrValue == null) {
						attrValue = valueBuilder.ToString ();
						valueBuilder.Length = 0;
					}
				}
				reader.MoveToElement ();
				reader.MoveToAttribute (attrName);
				attributeValues.Add (attrName, attrValue);

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
					if (!def.EnumeratedAttributeDeclaration.Contains (
						FilterNormalization (reader.Name, attrValue)))
						HandleError (String.Format ("Attribute enumeration constraint error in attribute {0}, value {1}.",
							reader.Name, attrValue), XmlSeverityType.Error);
				if (def.EnumeratedNotations.Count > 0)
					if (!def.EnumeratedNotations.Contains (
						FilterNormalization (reader.Name, attrValue)))
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

		private void VerifyDeclaredAttributes (DTDAttListDeclaration decl)
		{
			// Check if all required attributes exist, and/or
			// if there is default values, then add them.
			for (int i = 0; i < decl.Definitions.Count; i++) {
				DTDAttributeDefinition def = (DTDAttributeDefinition) decl.Definitions [i];
				if (attributes.Contains (def.Name))
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
					attributes.Add (def.Name);
					int colonAt = def.Name.IndexOf (':');
					attributeLocalNames.Add (def.Name,
						colonAt < 0 ? def.Name :
						def.Name.Substring (colonAt + 1));
					string prefix = colonAt < 0 ?
						String.Empty :
						def.Name.Substring (0, colonAt);
					attributePrefixes.Add (def.Name, prefix);
					attributeValues.Add (def.Name, def.DefaultValue);
					break;
				}
			}
		}

		public override bool ReadAttributeValue ()
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

		public override void ResolveEntity ()
		{
			reader.ResolveEntity ();
		}

		public override int AttributeCount {
			get {
				if (currentTextValue != null)
					return 0;

				if (dtd == null || !insideContent)
					return reader.AttributeCount;

				return attributes.Count;
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

		public override bool HasValue {
			get {
				return IsDefault ? true :
					currentTextValue != null ? true :
					reader.HasValue;
			}
		}

		public override bool IsDefault {
			get {
				if (currentTextValue != null)
					return false;
				if (currentAttribute == null)
					return false;
				return reader.GetAttribute (currentAttribute) == null;
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
					return (string) attributeLocalNames [currentAttribute];
				else
					return reader.LocalName;
			}
		}

		public override string Name {
			get {
				if (currentTextValue != null || consumedAttribute)
					return String.Empty;
				else if (NodeType == XmlNodeType.Attribute)
					return currentAttribute;
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
					return (string) attributeNamespaces [currentAttribute];
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
					return (string) attributePrefixes [currentAttribute];
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
				if (currentElement == null)
					return null;
				DTDAttListDeclaration decl =
					DTD.AttListDecls [currentElement];
				DTDAttributeDefinition def =
					decl != null ? decl [currentAttribute] : null;
				return def != null ? def.Datatype : null;
			}
		}

		char [] whitespaceChars = new char [] {' '};
		private string FilterNormalization (string attrName, string rawValue)
		{
			if (DTD == null || NodeType != XmlNodeType.Attribute ||
				sourceTextReader == null ||
				!sourceTextReader.Normalization)
				return rawValue;

			DTDAttributeDefinition def = 
				dtd.AttListDecls [currentElement].Get (attrName);
			valueBuilder.Append (rawValue);
			valueBuilder.Replace ('\r', ' ');
			valueBuilder.Replace ('\n', ' ');
			valueBuilder.Replace ('\t', ' ');
			try {
				if (def.Datatype.TokenizedType == XmlTokenizedType.CDATA)
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

		public override string Value {
			get {
				if (currentTextValue != null)
					return currentTextValue;
				// As to this property, MS.NET seems ignorant of EntityHandling...
				else if (NodeType == XmlNodeType.Attribute
					// It also covers default attribute text.
 					|| consumedAttribute)
					return FilterNormalization (Name, (string) attributeValues [currentAttribute]);
				else
					return FilterNormalization (Name, reader.Value);
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
