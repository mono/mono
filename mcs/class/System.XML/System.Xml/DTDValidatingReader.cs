//
// DTDValidatingReader.cs
//
// Author:
//   Atsushi Enomoto  (ginga@kit.hi-ho.ne.jp)
//
// (C)2003 Atsushi Enomoto
// (C)2004 Novell Inc.
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
using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace Mono.Xml
{
#if NET_2_0
	internal class DTDValidatingReader : XmlReader, IXmlLineInfo, IHasXmlParserContext, IHasXmlSchemaInfo, IXmlNamespaceResolver
#else
	internal class DTDValidatingReader : XmlReader, IXmlLineInfo, IHasXmlParserContext, IHasXmlSchemaInfo
#endif
	{
		public DTDValidatingReader (XmlReader reader)
			: this (reader, null)
		{
		}

		internal DTDValidatingReader (XmlReader reader,
			XmlValidatingReader validatingReader)
		{
			entityReaderStack = new Stack ();
			entityReaderNameStack = new Stack ();
			entityReaderDepthStack = new Stack ();
			this.reader = reader;
			this.sourceTextReader = reader as XmlTextReader;
			elementStack = new Stack ();
			automataStack = new Stack ();
			attributes = new ArrayList ();
			attributeValues = new Hashtable ();
			attributeLocalNames = new Hashtable ();
			attributeNamespaces = new Hashtable ();
			this.validatingReader = validatingReader;
			valueBuilder = new StringBuilder ();
			idList = new ArrayList ();
			missingIDReferences = new ArrayList ();
			XmlTextReader xtReader = reader as XmlTextReader;
			if (xtReader != null) {
#if DTD_HANDLE_EVENTS
				if (validatingReader != null)
					xtReader.ValidationEventHandler += new ValidationEventHandler (OnValidationEvent);
#endif
				resolver = xtReader.Resolver;
			}
			else
				resolver = new XmlUrlResolver ();
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
		StringBuilder valueBuilder;
		ArrayList idList;
		ArrayList missingIDReferences;
		XmlResolver resolver;
		EntityHandling currentEntityHandling;
		bool isSignificantWhitespace;
		bool isWhitespace;
		bool isText;
		bool nextMaybeSignificantWhitespace;

		// This field is used to get properties and to raise events.
		XmlValidatingReader validatingReader;

		public DTDObjectModel DTD {
			get { return dtd; }
		}

		public EntityHandling EntityHandling {
			get { return currentEntityHandling; }
			set { currentEntityHandling = value; }
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
		IDictionary IXmlNamespaceResolver.GetNamespacesInScope (XmlNamespaceScope scope)
		{
			IXmlNamespaceResolver res = reader as IXmlNamespaceResolver;
			return res != null ? res.GetNamespacesInScope (scope) : new Hashtable ();
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
			// Does it mean anything with DTD?
			return reader.LookupNamespace (prefix);
		}

#if NET_2_0
		string IXmlNamespaceResolver.LookupPrefix (string ns)
		{
			IXmlNamespaceResolver res = reader as IXmlNamespaceResolver;
			return res != null ? res.LookupPrefix (ns) : null;
		}

		string IXmlNamespaceResolver.LookupPrefix (string ns, bool atomizedNames)
		{
			IXmlNamespaceResolver res = reader as IXmlNamespaceResolver;
			return res != null ? res.LookupPrefix (ns, atomizedNames) : null;
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
				return;

			if (attributes.Count > i) {
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

		private void OnValidationEvent (object o, ValidationEventArgs e)
		{
//			if (validatingReader.HasValidationEvent)
//				validatingReader.OnValidationEvent (this, e);
			this.HandleError (e.Exception, e.Severity);
		}

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
			isWhitespace = false;
			isSignificantWhitespace = false;
			isText = false;
			nextMaybeSignificantWhitespace = false;

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
			if (nextEntityReader != null) {
				if (DTD == null || DTD.EntityDecls [reader.Name] == null)
					throw new XmlException ("Entity '" + reader.Name + "' was not declared.");
				entityReaderStack.Push (reader);
				entityReaderNameStack.Push (reader.Name);
				entityReaderDepthStack.Push (Depth);
				reader = sourceTextReader = nextEntityReader;
				nextEntityReader = null;
				return ReadContent ();
			} else if (reader.EOF && entityReaderStack.Count > 0) {
				reader = entityReaderStack.Pop () as XmlReader;
				entityReaderNameStack.Pop ();
				entityReaderDepthStack.Pop ();
				sourceTextReader = reader as XmlTextReader;
				return ReadContent ();
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
				if (entityReaderStack.Count > 0) {
					if (validatingReader.EntityHandling == EntityHandling.ExpandEntities)
						return ReadContent ();
					else
						return true;	// EndEntity
				}

				if (elementStack.Count != 0)
					throw new InvalidOperationException ("Unexpected end of XmlReader.");
				return false;
			}

			bool dontResetTextType = false;
			DTDElementDeclaration elem = null;

			switch (reader.NodeType) {
			case XmlNodeType.XmlDeclaration:
				if (GetAttribute ("standalone") == "yes")
					isStandalone = true;
				ValidateAttributes (null, false);
				break;

			case XmlNodeType.DocumentType:
				XmlTextReader xmlTextReader = reader as XmlTextReader;
				if (xmlTextReader == null) {
					xmlTextReader = new XmlTextReader ("", XmlNodeType.Document, null);
					xmlTextReader.XmlResolver = resolver;
					xmlTextReader.GenerateDTDObjectModel (reader.Name,
						reader ["PUBLIC"], reader ["SYSTEM"], reader.Value);
				}
				this.dtd = xmlTextReader.DTD;

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
				if (currentTextValue != null) {
					currentTextValue = constructingTextValue;
					constructingTextValue = null;
					return true;
				}
				goto case XmlNodeType.Text;
			case XmlNodeType.SignificantWhitespace:
				if (!isText)
					isSignificantWhitespace = true;
				dontResetTextType = true;
				goto case XmlNodeType.Text;
			case XmlNodeType.Text:
				isText = true;
				if (!dontResetTextType) {
					isWhitespace = isSignificantWhitespace = false;
				}
				// If no schema specification, then skip validation.
				if (currentAutomata == null)
					break;

				elem = dtd.ElementDecls [elementStack.Peek () as string];
				// Here element should have been already validated, so
				// if no matching declaration is found, simply ignore.
				if (elem != null && !elem.IsMixedContent && !elem.IsAny) {
					HandleError (String.Format ("Current element {0} does not allow character data content.", elementStack.Peek () as string),
						XmlSeverityType.Error);
					currentAutomata = previousAutomata;
				}
				if (validatingReader.EntityHandling == EntityHandling.ExpandEntities) {
					constructingTextValue += reader.Value;
					return ReadContent ();
				}
				break;
			case XmlNodeType.Whitespace:
				if (nextMaybeSignificantWhitespace) {
					currentTextValue = reader.Value;
					nextMaybeSignificantWhitespace = false;
					goto case XmlNodeType.SignificantWhitespace;
				}
				if (!isText && !isSignificantWhitespace)
					isWhitespace = true;
				if (validatingReader.EntityHandling == EntityHandling.ExpandEntities) {
					constructingTextValue += reader.Value;
					return ReadContent ();
				}
				ValidateWhitespaceNode ();
				break;
			case XmlNodeType.EntityReference:
				if (validatingReader.EntityHandling == EntityHandling.ExpandEntities) {
					ResolveEntity ();
					return ReadContent ();
				}
				break;
			}
			constructingTextValue = null;
			MoveToElement ();
			return true;
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
			while (reader.MoveToNextAttribute ()) {
				string attrName = reader.Name;
				this.currentAttribute = attrName;
				attributes.Add (attrName);
				attributeLocalNames.Add (attrName, reader.LocalName);
				attributeNamespaces.Add (attrName, reader.NamespaceURI);
				XmlReader targetReader = reader;
				string attrValue = null;
				if (currentEntityHandling == EntityHandling.ExpandCharEntities)
					attrValue = reader.Value;
				else {
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
				} else {
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
						} catch (Exception) {
							HandleError ("Attribute value is invalid against its data type.", XmlSeverityType.Error);
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
//					case XmlTokenizedType.NMTOKEN: nothing to do
//					case XmlTokenizedType.NMTOKENS: nothing to do
					}
					if (isStandalone && !def.IsInternalSubset && attrValue != normalized)
						HandleError ("In standalone document, attribute value characters must not be checked against external definition.", XmlSeverityType.Error);

					if (def.OccurenceType == DTDAttributeOccurenceType.Fixed &&
							attrValue != def.DefaultValue) {
						HandleError (String.Format ("Fixed attribute {0} in element {1} has invalid value {2}.",
							def.Name, decl.Name, attrValue),
							XmlSeverityType.Error);
					}
				}
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
				if (!attributes.Contains (def.Name)) {
					if (def.OccurenceType == DTDAttributeOccurenceType.Required) {
						HandleError (String.Format ("Required attribute {0} in element {1} not found .",
							def.Name, decl.Name),
							XmlSeverityType.Error);
					}
					else if (def.DefaultValue != null) {
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
							attributeLocalNames.Add (def.Name, colonAt < 0 ? def.Name : def.Name.Substring (colonAt + 1));
							attributeNamespaces.Add (def.Name, colonAt < 0 ? def.Name : def.Name.Substring (0, colonAt));
							attributeValues.Add (def.Name, def.DefaultValue);
							break;
						}
					}
				}
			}
		}

		public override bool ReadAttributeValue ()
		{
			if (consumedAttribute)
				return false;
			if (NodeType == XmlNodeType.Attribute &&
					currentEntityHandling == EntityHandling.ExpandEntities) {
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

#if NET_1_0
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
			if (resolver == null)
				return;

			// "reader." is required since NodeType must not be entityref by nature.
			if (reader.NodeType != XmlNodeType.EntityReference)
				throw new InvalidOperationException ("The current node is not an Entity Reference");
			DTDEntityDeclaration entity = DTD != null ? DTD.EntityDecls [reader.Name] as DTDEntityDeclaration : null;

			XmlNodeType xmlReaderNodeType =
				(currentAttribute != null) ? XmlNodeType.Attribute : XmlNodeType.Element;

			// MS.NET seems simply ignoring undeclared entity reference here ;-(
			if (entity != null && entity.SystemId != null) {
				Uri baseUri = entity.BaseURI == null ? null : new Uri (entity.BaseURI);
				Stream stream = resolver.GetEntity (resolver.ResolveUri (baseUri, entity.SystemId), null, typeof (Stream)) as Stream;
				nextEntityReader = new XmlTextReader (stream, xmlReaderNodeType, ParserContext);
			} else {
				string replacementText =
					(entity != null) ? entity.EntityValue : String.Empty;
				nextEntityReader = new XmlTextReader (replacementText, xmlReaderNodeType, ParserContext);
			}
			nextEntityReader.XmlResolver = resolver;
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
				if (entityReaderDepthStack.Count > 0) {
					baseNum += (int) entityReaderDepthStack.Peek ();
					if (NodeType != XmlNodeType.EndEntity)
						baseNum++;
				}
				if (currentTextValue != null && reader.NodeType == XmlNodeType.EndElement)
					baseNum++;

				return IsDefault ? baseNum + 1 : baseNum;
			}
		}

		public override bool EOF {
			get { return reader.EOF && entityReaderStack.Count == 0; }
		}

		public override bool HasValue {
			get {
				return IsDefault ? true :
					currentTextValue != null ? true :
					reader.HasValue; }
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
				if (currentTextValue != null)
					return String.Empty;
				return IsDefault ?
					consumedAttribute ? String.Empty : currentAttribute :
					reader.LocalName;
			}
		}

		public override string Name {
			get {
				if (currentTextValue != null)
					return String.Empty;
				return IsDefault ?
					consumedAttribute ? String.Empty : currentAttribute :
					reader.Name;
			}
		}

		public override string NamespaceURI {
			get {
				if (currentTextValue != null)
					return String.Empty;
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
				if (currentTextValue != null)
					return isSignificantWhitespace ? XmlNodeType.SignificantWhitespace :
						isWhitespace ? XmlNodeType.Whitespace :
						XmlNodeType.Text;

				if (entityReaderStack.Count > 0 && reader.EOF)
					return XmlNodeType.EndEntity;

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
				if (currentTextValue != null)
					return String.Empty;
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
			if (DTD != null &&
					NodeType == XmlNodeType.Attribute &&
					sourceTextReader != null && 
					sourceTextReader.Normalization) {
				DTDAttributeDefinition def = 
					dtd.AttListDecls [currentElement].Get (attrName);
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
				if (currentTextValue != null)
					return currentTextValue;
				// This check also covers value node of default attributes.
				if (IsDefault) {
					DTDAttributeDefinition def = 
						dtd.AttListDecls [currentElement] [currentAttribute] as DTDAttributeDefinition;
					return sourceTextReader != null && sourceTextReader.Normalization ?
						def.NormalizedDefaultValue : def.DefaultValue;
				}
				// As to this property, MS.NET seems ignorant of EntityHandling...
				else if (NodeType == XmlNodeType.Attribute)// &&
					return FilterNormalization (Name, (string) attributeValues [currentAttribute]);
				else if (consumedAttribute)
					return FilterNormalization (Name, (string) attributeValues [this.currentAttribute]);
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

		public XmlResolver XmlResolver {
			set {
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
