//
// System.Xml.DTDReader
//
// Author:
//   Atsushi Enomoto  (ginga@kit.hi-ho.ne.jp)
//
// (C)2003 Atsushi Enomoto
// (C)2004 Novell Inc.
//
// FIXME:
//	When a parameter entity contains cp section, it should be closed 
//	within that declaration.
//
//	Resolution to external entities from different BaseURI fails (it is
//	the same as MS.NET 1.1, but should be fixed in the future).
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
using System.Globalization;
using System.IO;
using System.Text;
using Mono.Xml;
#if NET_2_1
using XmlSchemaException = System.Xml.XmlException;
#else
using System.Xml.Schema;
#endif

namespace System.Xml
{
	internal class DTDReader : IXmlLineInfo
	{
		private XmlParserInput currentInput;
		private Stack parserInputStack;

		private char [] nameBuffer;
		private int nameLength;
		private int nameCapacity;
		private const int initialNameCapacity = 256;

		private StringBuilder valueBuffer;

		private int currentLinkedNodeLineNumber;
		private int currentLinkedNodeLinePosition;

		// Parameter entity placeholder
		private int dtdIncludeSect;

		private bool normalization;

		private bool processingInternalSubset;

		string cachedPublicId;
		string cachedSystemId;

		DTDObjectModel DTD;

#if DTD_HANDLE_EVENTS
		public event ValidationEventHandler ValidationEventHandler;
#endif

		// .ctor()

		public DTDReader (DTDObjectModel dtd,
			int startLineNumber, 
			int startLinePosition)
		{
			this.DTD = dtd;
			currentLinkedNodeLineNumber = startLineNumber;
			currentLinkedNodeLinePosition = startLinePosition;
			Init ();
		}

		// Properties

		public string BaseURI {
			get { return currentInput.BaseURI; }
		}

		public bool Normalization {
			get { return normalization; }
			set { normalization = value; }
		}

		public int LineNumber {
			get { return currentInput.LineNumber; }
		}

		public int LinePosition {
			get { return currentInput.LinePosition; }
		}

		public bool HasLineInfo ()
		{
			return true;
		}

		// Methods

		private XmlException NotWFError (string message)
		{
			return new XmlException (this as IXmlLineInfo, BaseURI, message);
		}

		private void Init ()
		{
			parserInputStack = new Stack ();

			nameBuffer = new char [initialNameCapacity];
			nameLength = 0;
			nameCapacity = initialNameCapacity;
			
			valueBuffer = new StringBuilder (512);
		}

		internal DTDObjectModel GenerateDTDObjectModel ()
		{
			// now compile DTD
			int originalParserDepth = parserInputStack.Count;
			bool more;
			if (DTD.InternalSubset != null && DTD.InternalSubset.Length > 0) {
				this.processingInternalSubset = true;
				XmlParserInput original = currentInput;

				currentInput = new XmlParserInput (
					new StringReader (DTD.InternalSubset),
					DTD.BaseURI,
					currentLinkedNodeLineNumber,
					currentLinkedNodeLinePosition);
				currentInput.AllowTextDecl = false;
				do {
					more = ProcessDTDSubset ();
					if (PeekChar () == -1 && parserInputStack.Count > 0)
						PopParserInput ();
				} while (more || parserInputStack.Count > originalParserDepth);
				if (dtdIncludeSect != 0)
					throw NotWFError ("INCLUDE section is not ended correctly.");

				currentInput = original;
				this.processingInternalSubset = false;
			}
			if (DTD.SystemId != null && DTD.SystemId != String.Empty && DTD.Resolver != null) {
				PushParserInput (DTD.SystemId);
				do {
					more = ProcessDTDSubset ();
					if (PeekChar () == -1 && parserInputStack.Count > 1)
						PopParserInput ();
				} while (more || parserInputStack.Count > originalParserDepth + 1);
				if (dtdIncludeSect != 0)
					throw NotWFError ("INCLUDE section is not ended correctly.");

				PopParserInput ();
			}
			ArrayList sc = new ArrayList ();

			// Entity recursion check.
			foreach (DTDEntityDeclaration ent in DTD.EntityDecls.Values) {
				if (ent.NotationName != null) {
					ent.ScanEntityValue (sc);
					sc.Clear ();
				}
			}
			// release unnecessary memory usage
			DTD.ExternalResources.Clear ();

			return DTD;
		}

		// Read any one of following:
		//   elementdecl, AttlistDecl, EntityDecl, NotationDecl,
		//   PI, Comment, Parameter Entity, or doctype termination char(']')
		//
		// Returns true if it may have any more contents, or false if not.
		private bool ProcessDTDSubset ()
		{
			SkipWhitespace ();
			int c2 = ReadChar ();
			switch(c2)
			{
			case -1:
				return false;
			case '%':
				// It affects on entity references' well-formedness
				if (this.processingInternalSubset)
					DTD.InternalSubsetHasPEReference = true;
				string peName = ReadName ();
				Expect (';');
				DTDParameterEntityDeclaration peDecl = GetPEDecl (peName);
				if (peDecl == null)
					break;
				currentInput.PushPEBuffer (peDecl);
//				int currentLine = currentInput.LineNumber;
//				int currentColumn = currentInput.LinePosition;
				while (currentInput.HasPEBuffer)
					ProcessDTDSubset ();
				SkipWhitespace ();
				// FIXME: Implement correct nest-level check.
				// Don't depend on lineinfo (might not be supplied)
//				if (currentInput.LineNumber != currentLine ||
//					currentInput.LinePosition != currentColumn)
//					throw NotWFError ("Incorrectly nested parameter entity.");
				break;
			case '<':
				int c = ReadChar ();
				switch(c)
				{
				case '?':
					// Only read, no store.
					ReadProcessingInstruction ();
					break;
				case '!':
					CompileDeclaration ();
					break;
				case -1:
					throw NotWFError ("Unexpected end of stream.");
				default:
					throw NotWFError ("Syntax Error after '<' character: " + (char) c);
				}
				break;
			case ']':
				if (dtdIncludeSect == 0)
					throw NotWFError ("Unbalanced end of INCLUDE/IGNORE section.");
				// End of inclusion
				Expect ("]>");
				dtdIncludeSect--;
				SkipWhitespace ();
				break;
			default:
				throw NotWFError (String.Format ("Syntax Error inside doctypedecl markup : {0}({1})", c2, (char) c2));
			}
			currentInput.AllowTextDecl = false;
			return true;
		}

		private void CompileDeclaration ()
		{
			switch(ReadChar ())
			{
			case '-':
				Expect ('-');
				// Only read, no store.
				ReadComment ();
				break;
			case 'E':
				switch(ReadChar ())
				{
				case 'N':
					Expect ("TITY");
					if (!SkipWhitespace ())
						throw NotWFError (
							"Whitespace is required after '<!ENTITY' in DTD entity declaration.");
					LOOPBACK:
					if (PeekChar () == '%') {
						ReadChar ();
						if (!SkipWhitespace ()) {
							ExpandPERef ();
							goto LOOPBACK;
						} else {
							// FIXME: Is this allowed? <!ENTITY % %name; ...> 
							// (i.e. Can PE name be replaced by another PE?)
							TryExpandPERef ();
							if (XmlChar.IsNameChar (PeekChar ()))
								ReadParameterEntityDecl ();
							else
								throw NotWFError ("expected name character");
						}
						break;
					}
					DTDEntityDeclaration ent = ReadEntityDecl ();
					if (DTD.EntityDecls [ent.Name] == null)
						DTD.EntityDecls.Add (ent.Name, ent);
					break;
				case 'L':
					Expect ("EMENT");
					DTDElementDeclaration el = ReadElementDecl ();
					DTD.ElementDecls.Add (el.Name, el);
					break;
				default:
					throw NotWFError ("Syntax Error after '<!E' (ELEMENT or ENTITY must be found)");
				}
				break;
			case 'A':
				Expect ("TTLIST");
				DTDAttListDeclaration atl = ReadAttListDecl ();
				DTD.AttListDecls.Add (atl.Name, atl);
				break;
			case 'N':
				Expect ("OTATION");
				DTDNotationDeclaration not = ReadNotationDecl ();
				DTD.NotationDecls.Add (not.Name, not);
				break;
			case '[':
				// conditional sections
				SkipWhitespace ();
				TryExpandPERef ();
				Expect ('I');
				switch (ReadChar ()) {
				case 'N':
					Expect ("CLUDE");
					ExpectAfterWhitespace ('[');
					dtdIncludeSect++;
					break;
				case 'G':
					Expect ("NORE");
					ReadIgnoreSect ();
					break;
				}
				break;
			default:
				throw NotWFError ("Syntax Error after '<!' characters.");
			}
		}

		private void ReadIgnoreSect ()
		{
			ExpectAfterWhitespace ('[');
			int dtdIgnoreSect = 1;

			while (dtdIgnoreSect > 0) {
				switch (ReadChar ()) {
				case -1:
					throw NotWFError ("Unexpected IGNORE section end.");
				case '<':
					if (PeekChar () != '!')
						break;
					ReadChar ();
					if (PeekChar () != '[')
						break;
					ReadChar ();
					dtdIgnoreSect++;
					break;
				case ']':
					if (PeekChar () != ']')
						break;
					ReadChar ();
					if (PeekChar () != '>')
						break;
					ReadChar ();
						dtdIgnoreSect--;
					break;
				}
			}
			if (dtdIgnoreSect != 0)
				throw NotWFError ("IGNORE section is not ended correctly.");
		}

		// The reader is positioned on the head of the name.
		private DTDElementDeclaration ReadElementDecl ()
		{
			DTDElementDeclaration decl = new DTDElementDeclaration (DTD);
			decl.IsInternalSubset = this.processingInternalSubset;

			if (!SkipWhitespace ())
				throw NotWFError ("Whitespace is required between '<!ELEMENT' and name in DTD element declaration.");
			TryExpandPERef ();
			decl.Name = ReadName ();
			if (!SkipWhitespace ())
				throw NotWFError ("Whitespace is required between name and content in DTD element declaration.");
			TryExpandPERef ();
			ReadContentSpec (decl);
			SkipWhitespace ();
			// This expanding is only allowed as a non-validating parser.
			TryExpandPERef ();
			Expect ('>');
			return decl;
		}

		// read 'children'(BNF) of contentspec
		private void ReadContentSpec (DTDElementDeclaration decl)
		{
			TryExpandPERef ();
			switch(ReadChar ())
			{
			case 'E':
				decl.IsEmpty = true;
				Expect ("MPTY");
				break;
			case 'A':
				decl.IsAny = true;
				Expect ("NY");
				break;
			case '(':
				DTDContentModel model = decl.ContentModel;
				SkipWhitespace ();
				TryExpandPERef ();
				if(PeekChar () == '#') {
					// Mixed Contents. "#PCDATA" must appear first.
					decl.IsMixedContent = true;
					model.Occurence = DTDOccurence.ZeroOrMore;
					model.OrderType = DTDContentOrderType.Or;
					Expect ("#PCDATA");
					SkipWhitespace ();
					TryExpandPERef ();
					while(PeekChar () != ')') {
						SkipWhitespace ();
						if (PeekChar () == '%') {
							TryExpandPERef ();
							continue;
						}
						Expect('|');
						SkipWhitespace ();
						TryExpandPERef ();
						DTDContentModel elem = new DTDContentModel (DTD, decl.Name);
//						elem.LineNumber = currentInput.LineNumber;
//						elem.LinePosition = currentInput.LinePosition;
						elem.ElementName = ReadName ();
						this.AddContentModel (model.ChildModels, elem);
						SkipWhitespace ();
						TryExpandPERef ();
					}
					Expect (')');
					if (model.ChildModels.Count > 0)
						Expect ('*');
					else if (PeekChar () == '*')
						Expect ('*');
				} else {
					// Non-Mixed Contents
					model.ChildModels.Add (ReadCP (decl));
					SkipWhitespace ();

					do {	// copied from ReadCP() ...;-)
						if (PeekChar () == '%') {
							TryExpandPERef ();
							continue;
						}
						if(PeekChar ()=='|') {
							// CPType=Or
							if (model.OrderType == DTDContentOrderType.Seq)
								throw NotWFError ("Inconsistent choice markup in sequence cp.");
							model.OrderType = DTDContentOrderType.Or;
							ReadChar ();
							SkipWhitespace ();
							AddContentModel (model.ChildModels, ReadCP (decl));
							SkipWhitespace ();
						}
						else if(PeekChar () == ',')
						{
							// CPType=Seq
							if (model.OrderType == DTDContentOrderType.Or)
								throw NotWFError ("Inconsistent sequence markup in choice cp.");
							model.OrderType = DTDContentOrderType.Seq;
							ReadChar ();
							SkipWhitespace ();
							model.ChildModels.Add (ReadCP (decl));
							SkipWhitespace ();
						}
						else
							break;
					}
					while(true);

					Expect (')');
					switch(PeekChar ())
					{
					case '?':
						model.Occurence = DTDOccurence.Optional;
						ReadChar ();
						break;
					case '*':
						model.Occurence = DTDOccurence.ZeroOrMore;
						ReadChar ();
						break;
					case '+':
						model.Occurence = DTDOccurence.OneOrMore;
						ReadChar ();
						break;
					}
					SkipWhitespace ();
				}
				SkipWhitespace ();
				break;
			default:
				throw NotWFError ("ContentSpec is missing.");
			}
		}

		// Read 'cp' (BNF) of contentdecl (BNF)
		private DTDContentModel ReadCP (DTDElementDeclaration elem)
		{
			DTDContentModel model = null;
			TryExpandPERef ();
			if(PeekChar () == '(') {
				model = new DTDContentModel (DTD, elem.Name);
				ReadChar ();
				SkipWhitespace ();
				model.ChildModels.Add (ReadCP (elem));
				SkipWhitespace ();
				do {
					if (PeekChar () == '%') {
						TryExpandPERef ();
						continue;
					}
					if(PeekChar ()=='|') {
						// CPType=Or
						if (model.OrderType == DTDContentOrderType.Seq)
							throw NotWFError ("Inconsistent choice markup in sequence cp.");
						model.OrderType = DTDContentOrderType.Or;
						ReadChar ();
						SkipWhitespace ();
						AddContentModel (model.ChildModels, ReadCP (elem));
						SkipWhitespace ();
					}
					else if(PeekChar () == ',') {
						// CPType=Seq
						if (model.OrderType == DTDContentOrderType.Or)
							throw NotWFError ("Inconsistent sequence markup in choice cp.");
						model.OrderType = DTDContentOrderType.Seq;
						ReadChar ();
						SkipWhitespace ();
						model.ChildModels.Add (ReadCP (elem));
						SkipWhitespace ();
					}
					else
						break;
				}
				while(true);
				ExpectAfterWhitespace (')');
			}
			else {
				TryExpandPERef ();
				model = new DTDContentModel (DTD, elem.Name);
				model.ElementName = ReadName ();
			}

			switch(PeekChar ()) {
			case '?':
				model.Occurence = DTDOccurence.Optional;
				ReadChar ();
				break;
			case '*':
				model.Occurence = DTDOccurence.ZeroOrMore;
				ReadChar ();
				break;
			case '+':
				model.Occurence = DTDOccurence.OneOrMore;
				ReadChar ();
				break;
			}
			return model;
		}

		private void AddContentModel (DTDContentModelCollection cmc, DTDContentModel cm)
		{
			if (cm.ElementName != null) {
				for (int i = 0; i < cmc.Count; i++) {
					if (cmc [i].ElementName == cm.ElementName) {
						HandleError (new XmlSchemaException ("Element content must be unique inside mixed content model.",
							this.LineNumber,
							this.LinePosition,
							null,
							this.BaseURI,
							null));
						return;
					}
				}
			}
			cmc.Add (cm);
		}

		// The reader is positioned on the first name char.
		private void ReadParameterEntityDecl ()
		{
			DTDParameterEntityDeclaration decl = 
				new DTDParameterEntityDeclaration (DTD);
			decl.BaseURI = BaseURI;
			decl.XmlResolver = DTD.Resolver;

			decl.Name = ReadName ();
			if (!SkipWhitespace ())
				throw NotWFError ("Whitespace is required after name in DTD parameter entity declaration.");

			if (PeekChar () == 'S' || PeekChar () == 'P') {
				// read publicId/systemId
				ReadExternalID ();
				decl.PublicId = cachedPublicId;
				decl.SystemId = cachedSystemId;
				SkipWhitespace ();
				decl.Resolve ();

				ResolveExternalEntityReplacementText (decl);
			} else {
				TryExpandPERef ();
				int quoteChar = ReadChar ();
				if (quoteChar != '\'' && quoteChar != '"')
					throw NotWFError ("quotation char was expected.");
				ClearValueBuffer ();
				bool loop = true;
				while (loop) {
					int c = ReadChar ();
					switch (c) {
					case -1:
						throw NotWFError ("unexpected end of stream in entity value definition.");
					case '"':
						if (quoteChar == '"')
							loop = false;
						else
							AppendValueChar ('"');
						break;
					case '\'':
						if (quoteChar == '\'')
							loop = false;
						else
							AppendValueChar ('\'');
						break;
					default:
						if (XmlChar.IsInvalid (c))
							throw NotWFError ("Invalid character was used to define parameter entity.");
						AppendValueChar (c);
						break;
					}
				}
				decl.LiteralEntityValue = CreateValueString ();
				ClearValueBuffer ();
				ResolveInternalEntityReplacementText (decl);
			}
			ExpectAfterWhitespace ('>');


			if (DTD.PEDecls [decl.Name] == null) {
                                DTD.PEDecls.Add (decl.Name, decl);
			}
		}

		private void ResolveExternalEntityReplacementText (DTDEntityBase decl)
		{
			if (decl.SystemId != null && decl.SystemId.Length > 0) {
				// FIXME: not always it should be read in Element context
				XmlTextReader xtr = new XmlTextReader (decl.LiteralEntityValue, XmlNodeType.Element, null);
				xtr.SkipTextDeclaration ();
				if (decl is DTDEntityDeclaration && DTD.EntityDecls [decl.Name] == null) {
					// GE - also checked as valid contents
					StringBuilder sb = new StringBuilder ();
					xtr.Normalization = this.Normalization;
					xtr.Read ();
					while (!xtr.EOF)
						sb.Append (xtr.ReadOuterXml ());
					decl.ReplacementText = sb.ToString ();
				}
				else
					// PE
					decl.ReplacementText = xtr.GetRemainder ().ReadToEnd ();
			}
			else
				decl.ReplacementText = decl.LiteralEntityValue;
		}

		private void ResolveInternalEntityReplacementText (DTDEntityBase decl)
		{
			string value = decl.LiteralEntityValue;
			int len = value.Length;
			ClearValueBuffer ();
			for (int i = 0; i < len; i++) {
				int ch = value [i];
				int end = 0;
				string name;
				switch (ch) {
				case '&':
					i++;
					end = value.IndexOf (';', i);
					if (end < i + 1)
						throw new XmlException (decl, decl.BaseURI, "Invalid reference markup.");
					// expand charref
					if (value [i] == '#') {
						i++;
						ch = GetCharacterReference (decl, value, ref i, end);
						if (XmlChar.IsInvalid (ch))
							throw NotWFError ("Invalid character was used to define parameter entity.");

					} else {
						name = value.Substring (i, end - i);
						if (!XmlChar.IsName (name))
							throw NotWFError (String.Format ("'{0}' is not a valid entity reference name.", name));
						// don't expand "general" entity.
						AppendValueChar ('&');
						valueBuffer.Append (name);
						AppendValueChar (';');
						i = end;
						break;
					}
					if (XmlChar.IsInvalid (ch))
						throw new XmlException (decl, decl.BaseURI, "Invalid character was found in the entity declaration.");
					AppendValueChar (ch);
					break;
				case '%':
					i++;
					end = value.IndexOf (';', i);
					if (end < i + 1)
						throw new XmlException (decl, decl.BaseURI, "Invalid reference markup.");
					name = value.Substring (i, end - i);
					valueBuffer.Append (GetPEValue (name));
					i = end;
					break;
				default:
					AppendValueChar (ch);
					break;
				}
			}
			decl.ReplacementText = CreateValueString ();

			ClearValueBuffer ();
		}

		private int GetCharacterReference (DTDEntityBase li, string value, ref int index, int end)
		{
			int ret = 0;
			if (value [index] == 'x') {
				try {
					ret = int.Parse (value.Substring (index + 1, end - index - 1), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
				} catch (FormatException) {
					throw new XmlException (li, li.BaseURI, "Invalid number for a character reference.");
				}
			} else {
				try {
					ret = int.Parse (value.Substring (index, end - index), CultureInfo.InvariantCulture);
				} catch (FormatException) {
					throw new XmlException (li, li.BaseURI, "Invalid number for a character reference.");
				}
			}
			index = end;
			return ret;
		}

		private string GetPEValue (string peName)
		{
			DTDParameterEntityDeclaration peDecl = GetPEDecl (peName);
			return peDecl != null ? 
				peDecl.ReplacementText : String.Empty;
		}

		private DTDParameterEntityDeclaration GetPEDecl (string peName)
		{
			DTDParameterEntityDeclaration peDecl =
				DTD.PEDecls [peName] as DTDParameterEntityDeclaration;
			if (peDecl != null) {
				if (peDecl.IsInternalSubset)
					throw NotWFError ("Parameter entity is not allowed in internal subset entity '" + peName + "'");
				return peDecl;
			}
			// See XML 1.0 section 4.1 for both WFC and VC.
			if ((DTD.SystemId == null && !DTD.InternalSubsetHasPEReference) || DTD.IsStandalone)
				throw NotWFError (String.Format ("Parameter entity '{0}' not found.",peName));
			HandleError (new XmlSchemaException (
				"Parameter entity " + peName + " not found.", null));
			return null;
		}

		private bool TryExpandPERef ()
		{
			if (PeekChar () != '%')
				return false;
			while (PeekChar () == '%') {
				TryExpandPERefSpaceKeep ();
				SkipWhitespace ();
			}
			return true;
		}

		// Tries to expand parameter entities, but it should not skip spaces
		private bool TryExpandPERefSpaceKeep ()
		{
			if (PeekChar () == '%') {
				if (this.processingInternalSubset)
					throw NotWFError ("Parameter entity reference is not allowed inside internal subset.");
				ReadChar ();
				ExpandPERef ();
				return true;
			}
			else
				return false;
		}

		// reader is positioned after '%'
		private void ExpandPERef ()
		{
			string peName = ReadName ();
			Expect (';');
			DTDParameterEntityDeclaration peDecl =
				DTD.PEDecls [peName] as DTDParameterEntityDeclaration;
			if (peDecl == null) {
				HandleError (new XmlSchemaException ("Parameter entity " + peName + " not found.", null));
				return;	// do nothing
			}
			currentInput.PushPEBuffer (peDecl);
		}

		// The reader is positioned on the head of the name.
		private DTDEntityDeclaration ReadEntityDecl ()
		{
			DTDEntityDeclaration decl = new DTDEntityDeclaration (DTD);
			decl.BaseURI = BaseURI;
			decl.XmlResolver = DTD.Resolver;
			decl.IsInternalSubset = this.processingInternalSubset;
			TryExpandPERef ();
			decl.Name = ReadName ();
			if (!SkipWhitespace ())
				throw NotWFError ("Whitespace is required between name and content in DTD entity declaration.");
			TryExpandPERef ();

			if (PeekChar () == 'S' || PeekChar () == 'P') {
				// external entity
				ReadExternalID ();
				decl.PublicId = cachedPublicId;
				decl.SystemId = cachedSystemId;
				if (SkipWhitespace ()) {
					if (PeekChar () == 'N') {
						// NDataDecl
						Expect ("NDATA");
						if (!SkipWhitespace ())
							throw NotWFError ("Whitespace is required after NDATA.");
						decl.NotationName = ReadName ();	// ndata_name
					}
				}
				if (decl.NotationName == null) {
					decl.Resolve ();
					ResolveExternalEntityReplacementText (decl);
				} else {
					// Unparsed entity.
					decl.LiteralEntityValue = String.Empty;
					decl.ReplacementText = String.Empty;
				}
			}
			else {
				// literal entity
				ReadEntityValueDecl (decl);
				ResolveInternalEntityReplacementText (decl);
			}
			SkipWhitespace ();
			// This expanding is only allowed as a non-validating parser.
			TryExpandPERef ();
			Expect ('>');
			return decl;
		}

		private void ReadEntityValueDecl (DTDEntityDeclaration decl)
		{
			SkipWhitespace ();
			// quotation char will be finally removed on unescaping
			int quoteChar = ReadChar ();
			if (quoteChar != '\'' && quoteChar != '"')
				throw NotWFError ("quotation char was expected.");
			ClearValueBuffer ();

			while (PeekChar () != quoteChar) {
				int ch = ReadChar ();
				switch (ch) {
				case '%':
					string name = ReadName ();
					Expect (';');
					if (decl.IsInternalSubset)
						throw NotWFError (String.Format ("Parameter entity is not allowed in internal subset entity '{0}'", name));
					valueBuffer.Append (GetPEValue (name));
					break;
				case -1:
					throw NotWFError ("unexpected end of stream.");
				default:
					if (this.normalization && XmlChar.IsInvalid (ch))
						throw NotWFError ("Invalid character was found in the entity declaration.");
					AppendValueChar (ch);
					break;
				}
			}
//			string value = Dereference (CreateValueString (), false);
			string value = CreateValueString ();
			ClearValueBuffer ();

			Expect (quoteChar);
			decl.LiteralEntityValue = value;
		}

		private DTDAttListDeclaration ReadAttListDecl ()
		{
			TryExpandPERefSpaceKeep ();
			if (!SkipWhitespace ())
				throw NotWFError ("Whitespace is required between ATTLIST and name in DTD attlist declaration.");
			TryExpandPERef ();
			string name = ReadName ();	// target element name
			DTDAttListDeclaration decl =
				DTD.AttListDecls [name] as DTDAttListDeclaration;
			if (decl == null)
				decl = new DTDAttListDeclaration (DTD);
			decl.IsInternalSubset = this.processingInternalSubset;
			decl.Name = name;

			if (!SkipWhitespace ())
				if (PeekChar () != '>')
					throw NotWFError ("Whitespace is required between name and content in non-empty DTD attlist declaration.");

			TryExpandPERef ();

			while (XmlChar.IsNameChar (PeekChar ())) {
				DTDAttributeDefinition def = ReadAttributeDefinition ();
				// There must not be two or more ID attributes.
				if (def.Datatype.TokenizedType == XmlTokenizedType.ID) {
					for (int i = 0; i < decl.Definitions.Count; i++) {
						DTDAttributeDefinition d = decl [i];
						if (d.Datatype.TokenizedType == XmlTokenizedType.ID) {
							HandleError (new XmlSchemaException ("AttList declaration must not contain two or more ID attributes.",
								def.LineNumber, def.LinePosition, null, def.BaseURI, null));
							break;
						}
					}
				}
				if (decl [def.Name] == null)
					decl.Add (def);
				SkipWhitespace ();
				TryExpandPERef ();
			}
			SkipWhitespace ();
			// This expanding is only allowed as a non-validating parser.
			TryExpandPERef ();
			Expect ('>');
			return decl;
		}

		private DTDAttributeDefinition ReadAttributeDefinition ()
		{
#if NET_2_1_HACK
			throw new NotImplementedException ();
#else
			DTDAttributeDefinition def = new DTDAttributeDefinition (DTD);
			def.IsInternalSubset = this.processingInternalSubset;

			// attr_name
			TryExpandPERef ();
			def.Name = ReadName ();
			if (!SkipWhitespace ())
				throw NotWFError ("Whitespace is required between name and content in DTD attribute definition.");

			// attr_value
			TryExpandPERef ();
			switch(PeekChar ()) {
			case 'C':	// CDATA
				Expect ("CDATA");
				def.Datatype = XmlSchemaDatatype.FromName ("normalizedString", XmlSchema.Namespace);
				break;
			case 'I':	// ID, IDREF, IDREFS
				Expect ("ID");
				if(PeekChar () == 'R') {
					Expect ("REF");
					if(PeekChar () == 'S') {
						// IDREFS
						ReadChar ();
						def.Datatype = XmlSchemaDatatype.FromName ("IDREFS", XmlSchema.Namespace);
					}
					else	// IDREF
						def.Datatype = XmlSchemaDatatype.FromName ("IDREF", XmlSchema.Namespace);
				}
				else	// ID
					def.Datatype = XmlSchemaDatatype.FromName ("ID", XmlSchema.Namespace);
				break;
			case 'E':	// ENTITY, ENTITIES
				Expect ("ENTIT");
				switch(ReadChar ()) {
					case 'Y':	// ENTITY
						def.Datatype = XmlSchemaDatatype.FromName ("ENTITY", XmlSchema.Namespace);
						break;
					case 'I':	// ENTITIES
						Expect ("ES");
						def.Datatype = XmlSchemaDatatype.FromName ("ENTITIES", XmlSchema.Namespace);
						break;
				}
				break;
			case 'N':	// NMTOKEN, NMTOKENS, NOTATION
				ReadChar ();
				switch(PeekChar ()) {
				case 'M':
					Expect ("MTOKEN");
					if(PeekChar ()=='S') {	// NMTOKENS
						ReadChar ();
						def.Datatype = XmlSchemaDatatype.FromName ("NMTOKENS", XmlSchema.Namespace);
					}
					else	// NMTOKEN
						def.Datatype = XmlSchemaDatatype.FromName ("NMTOKEN", XmlSchema.Namespace);
					break;
				case 'O':
					Expect ("OTATION");
					def.Datatype = XmlSchemaDatatype.FromName ("NOTATION", XmlSchema.Namespace);
					TryExpandPERefSpaceKeep ();
					if (!SkipWhitespace ())
						throw NotWFError ("Whitespace is required after notation name in DTD attribute definition.");
					Expect ('(');
					SkipWhitespace ();
					TryExpandPERef ();
					def.EnumeratedNotations.Add (ReadName ());		// notation name
					SkipWhitespace ();
					TryExpandPERef ();
					while(PeekChar () == '|') {
						ReadChar ();
						SkipWhitespace ();
						TryExpandPERef ();
						def.EnumeratedNotations.Add (ReadName ());	// notation name
						SkipWhitespace ();
						TryExpandPERef ();
					}
					Expect (')');
					break;
				default:
					throw NotWFError ("attribute declaration syntax error.");
				}
				break;
			default:	// Enumerated Values
				def.Datatype = XmlSchemaDatatype.FromName ("NMTOKEN", XmlSchema.Namespace);
				TryExpandPERef ();
				Expect ('(');
				SkipWhitespace ();
				TryExpandPERef ();
				def.EnumeratedAttributeDeclaration.Add (
					def.Datatype.Normalize (ReadNmToken ()));	// enum value
				SkipWhitespace ();
				while(PeekChar () == '|') {
					ReadChar ();
					SkipWhitespace ();
					TryExpandPERef ();
					def.EnumeratedAttributeDeclaration.Add (
						def.Datatype.Normalize (ReadNmToken ()));	// enum value
					SkipWhitespace ();
					TryExpandPERef ();
				}
				Expect (')');
				break;
			}
			TryExpandPERefSpaceKeep ();
			if (!SkipWhitespace ())
				throw NotWFError ("Whitespace is required between type and occurence in DTD attribute definition.");

			// def_value
			ReadAttributeDefaultValue (def);

			return def;
#endif
		}

		private void ReadAttributeDefaultValue (DTDAttributeDefinition def)
		{
			if(PeekChar () == '#')
			{
				ReadChar ();
				switch(PeekChar ())
				{
				case 'R':
					Expect ("REQUIRED");
					def.OccurenceType = DTDAttributeOccurenceType.Required;
					break;
				case 'I':
					Expect ("IMPLIED");
					def.OccurenceType = DTDAttributeOccurenceType.Optional;
					break;
				case 'F':
					Expect ("FIXED");
					def.OccurenceType = DTDAttributeOccurenceType.Fixed;
					if (!SkipWhitespace ())
						throw NotWFError ("Whitespace is required between FIXED and actual value in DTD attribute definition.");
					def.UnresolvedDefaultValue = ReadDefaultAttribute ();
					break;
				}
			} else {
				// one of the enumerated value
				SkipWhitespace ();
				TryExpandPERef ();
				def.UnresolvedDefaultValue = ReadDefaultAttribute ();
			}

			// VC: If default value exists, it should be valid.
			if (def.DefaultValue != null) {
				string normalized = def.Datatype.Normalize (def.DefaultValue);
				bool breakup = false;
				object parsed = null;

				// enumeration validity
				if (def.EnumeratedAttributeDeclaration.Count > 0) {
					if (!def.EnumeratedAttributeDeclaration.Contains (normalized)) {
						HandleError (new XmlSchemaException ("Default value is not one of the enumerated values.",
							def.LineNumber, def.LinePosition, null, def.BaseURI, null));
						breakup = true;
					}
				}
				if (def.EnumeratedNotations.Count > 0) {
					if (!def.EnumeratedNotations.Contains (normalized)) {
						HandleError (new XmlSchemaException ("Default value is not one of the enumerated notation values.",
							def.LineNumber, def.LinePosition, null, def.BaseURI, null));
						breakup = true;
					}
				}

				// type based validity
				if (!breakup) {
					try {
						parsed = def.Datatype.ParseValue (normalized, DTD.NameTable, null);
					} catch (Exception ex) { // FIXME: (wishlist) bad catch ;-(
						HandleError (new XmlSchemaException ("Invalid default value for ENTITY type.",
							def.LineNumber, def.LinePosition, null, def.BaseURI, ex));
						breakup = true;
					}
				}
				if (!breakup) {
					switch (def.Datatype.TokenizedType) {
					case XmlTokenizedType.ENTITY:
						if (DTD.EntityDecls [normalized] == null)
							HandleError (new XmlSchemaException ("Specified entity declaration used by default attribute value was not found.",
								def.LineNumber, def.LinePosition, null, def.BaseURI, null));
						break;
					case XmlTokenizedType.ENTITIES:
						string [] entities = parsed as string [];
						for (int i = 0; i < entities.Length; i++) {
							string entity = entities [i];
							if (DTD.EntityDecls [entity] == null)
								HandleError (new XmlSchemaException ("Specified entity declaration used by default attribute value was not found.",
									def.LineNumber, def.LinePosition, null, def.BaseURI, null));
						}
						break;
					}
				}
			}
			// Extra ID attribute validity check.
			if (def.Datatype != null && def.Datatype.TokenizedType == XmlTokenizedType.ID)
				if (def.UnresolvedDefaultValue != null)
					HandleError (new XmlSchemaException ("ID attribute must not have fixed value constraint.",
						def.LineNumber, def.LinePosition, null, def.BaseURI, null));

		}

		private DTDNotationDeclaration ReadNotationDecl()
		{
			DTDNotationDeclaration decl = new DTDNotationDeclaration (DTD);
			if (!SkipWhitespace ())
				throw NotWFError ("Whitespace is required between NOTATION and name in DTD notation declaration.");
			TryExpandPERef ();
			decl.Name = ReadName ();	// notation name
			/*
			if (namespaces) {	// copy from SetProperties ;-)
				int indexOfColon = decl.Name.IndexOf (':');

				if (indexOfColon == -1) {
					decl.Prefix = String.Empty;
					decl.LocalName = decl.Name;
				} else {
					decl.Prefix = decl.Name.Substring (0, indexOfColon);
					decl.LocalName = decl.Name.Substring (indexOfColon + 1);
				}
			} else {
			*/
				decl.Prefix = String.Empty;
				decl.LocalName = decl.Name;
//			}

			SkipWhitespace ();
			if(PeekChar () == 'P') {
				decl.PublicId = ReadPubidLiteral ();
				bool wsSkipped = SkipWhitespace ();
				if (PeekChar () == '\'' || PeekChar () == '"') {
					if (!wsSkipped)
						throw NotWFError ("Whitespace is required between public id and system id.");
					decl.SystemId = ReadSystemLiteral (false);
					SkipWhitespace ();
				}
			} else if(PeekChar () == 'S') {
				decl.SystemId = ReadSystemLiteral (true);
				SkipWhitespace ();
			}
			if(decl.PublicId == null && decl.SystemId == null)
				throw NotWFError ("public or system declaration required for \"NOTATION\" declaration.");
			// This expanding is only allowed as a non-validating parser.
			TryExpandPERef ();
			Expect ('>');
			return decl;
		}

		private void ReadExternalID () {
			switch (PeekChar ()) {
			case 'S':
				cachedSystemId = ReadSystemLiteral (true);
				break;
			case 'P':
				cachedPublicId = ReadPubidLiteral ();
				if (!SkipWhitespace ())
					throw NotWFError ("Whitespace is required between PUBLIC id and SYSTEM id.");
				cachedSystemId = ReadSystemLiteral (false);
				break;
			}
		}

		// The reader is positioned on the first 'S' of "SYSTEM".
		private string ReadSystemLiteral (bool expectSYSTEM)
		{
			if(expectSYSTEM) {
				Expect ("SYSTEM");
				if (!SkipWhitespace ())
					throw NotWFError ("Whitespace is required after 'SYSTEM'.");
			}
			else
				SkipWhitespace ();
			int quoteChar = ReadChar ();	// apos or quot
			int c = 0;
			ClearValueBuffer ();
			while (c != quoteChar) {
				c = ReadChar ();
				if (c < 0)
					throw NotWFError ("Unexpected end of stream in ExternalID.");
				if (c != quoteChar)
					AppendValueChar (c);
			}
			return CreateValueString (); //currentTag.ToString (startPos, currentTag.Length - 1 - startPos);
		}

		private string ReadPubidLiteral()
		{
			Expect ("PUBLIC");
			if (!SkipWhitespace ())
				throw NotWFError ("Whitespace is required after 'PUBLIC'.");
			int quoteChar = ReadChar ();
			int c = 0;
			ClearValueBuffer ();
			while(c != quoteChar)
			{
				c = ReadChar ();
				if(c < 0) throw NotWFError ("Unexpected end of stream in ExternalID.");
				if(c != quoteChar && !XmlChar.IsPubidChar (c))
					throw NotWFError (String.Format ("character '{0}' not allowed for PUBLIC ID", (char) c));
				if (c != quoteChar)
					AppendValueChar (c);
			}
			return CreateValueString (); //currentTag.ToString (startPos, currentTag.Length - 1 - startPos);
		}

		// The reader is positioned on the first character
		// of the name.
		internal string ReadName ()
		{
			return ReadNameOrNmToken(false);
		}

		// The reader is positioned on the first character
		// of the name.
		private string ReadNmToken ()
		{
			return ReadNameOrNmToken(true);
		}

		private string ReadNameOrNmToken(bool isNameToken)
		{
			int ch = PeekChar ();
			if(isNameToken) {
				if (!XmlChar.IsNameChar (ch))
					throw NotWFError (String.Format ("a nmtoken did not start with a legal character {0} ({1})", ch, (char) ch));
			}
			else {
				if (!XmlChar.IsFirstNameChar (ch))
					throw NotWFError (String.Format ("a name did not start with a legal character {0} ({1})", ch, (char) ch));
			}

			nameLength = 0;

			AppendNameChar (ReadChar ());

			while (XmlChar.IsNameChar (PeekChar ())) {
				AppendNameChar (ReadChar ());
			}

			return CreateNameString ();
		}

		// Read the next character and compare it against the
		// specified character.
		private void Expect (int expected)
		{
			int ch = ReadChar ();

			if (ch != expected) {
				throw NotWFError (String.Format (CultureInfo.InvariantCulture, 
						"expected '{0}' ({1:X}) but found '{2}' ({3:X})",
						(char) expected,
						expected,
						(char) ch,
						ch));
			}
		}

		private void Expect (string expected)
		{
			int len = expected.Length;
			for (int i=0; i< len; i++)
				Expect (expected [i]);
		}

		private void ExpectAfterWhitespace (char c)
		{
			while (true) {
				int i = ReadChar ();
				if (XmlChar.IsWhitespace (i))
					continue;
				if (c != i)
					throw NotWFError (String.Format (CultureInfo.InvariantCulture, "Expected {0} but found {1} [{2}].", c, (char) i, i));
				break;
			}
		}

		// Does not consume the first non-whitespace character.
		private bool SkipWhitespace ()
		{
			bool skipped = XmlChar.IsWhitespace (PeekChar ());
			while (XmlChar.IsWhitespace (PeekChar ()))
				ReadChar ();
			return skipped;
		}

		private int PeekChar ()
		{
			return currentInput.PeekChar ();
		}

		private int ReadChar ()
		{
			return currentInput.ReadChar ();
		}

		// The reader is positioned on the first character after
		// the leading '<!--'.
		private void ReadComment ()
		{
			currentInput.AllowTextDecl = false;

			while (PeekChar () != -1) {
				int ch = ReadChar ();

				if (ch == '-' && PeekChar () == '-') {
					ReadChar ();

					if (PeekChar () != '>')
						throw NotWFError ("comments cannot contain '--'");

					ReadChar ();
					break;
				}

				if (XmlChar.IsInvalid (ch))
					throw NotWFError ("Not allowed character was found.");
			}
		}

		// The reader is positioned on the first character
		// of the target.
		//
		// It may be xml declaration or processing instruction.
		private void ReadProcessingInstruction ()
		{
			string target = ReadName ();
			if (target == "xml") {
				ReadTextDeclaration ();
				return;
			} else if (CultureInfo.InvariantCulture.CompareInfo.Compare (target, "xml", CompareOptions.IgnoreCase) == 0)
				throw NotWFError ("Not allowed processing instruction name which starts with 'X', 'M', 'L' was found.");

			currentInput.AllowTextDecl = false;

			if (!SkipWhitespace ())
				if (PeekChar () != '?')
					throw NotWFError ("Invalid processing instruction name was found.");

			while (PeekChar () != -1) {
				int ch = ReadChar ();

				if (ch == '?' && PeekChar () == '>') {
					ReadChar ();
					break;
				}
			}
		}

		// The reader is positioned after "<?xml "
		private void ReadTextDeclaration ()
		{
			if (!currentInput.AllowTextDecl)
				throw NotWFError ("Text declaration cannot appear in this state.");

			currentInput.AllowTextDecl = false;

			SkipWhitespace ();

			// version decl
			if (PeekChar () == 'v') {
				Expect ("version");
				ExpectAfterWhitespace ('=');
				SkipWhitespace ();
				int quoteChar = ReadChar ();
				char [] expect1_0 = new char [3];
				int versionLength = 0;
				switch (quoteChar) {
				case '\'':
				case '"':
					while (PeekChar () != quoteChar) {
						if (PeekChar () == -1)
							throw NotWFError ("Invalid version declaration inside text declaration.");
						else if (versionLength == 3)
							throw NotWFError ("Invalid version number inside text declaration.");
						else {
							expect1_0 [versionLength] = (char) ReadChar ();
							versionLength++;
							if (versionLength == 3 && new String (expect1_0) != "1.0")
								throw NotWFError ("Invalid version number inside text declaration.");
						}
					}
					ReadChar ();
					SkipWhitespace ();
					break;
				default:
					throw NotWFError ("Invalid version declaration inside text declaration.");
				}
			}

			if (PeekChar () == 'e') {
				Expect ("encoding");
				ExpectAfterWhitespace ('=');
				SkipWhitespace ();
				int quoteChar = ReadChar ();
				switch (quoteChar) {
				case '\'':
				case '"':
					while (PeekChar () != quoteChar)
						if (ReadChar () == -1)
							throw NotWFError ("Invalid encoding declaration inside text declaration.");
					ReadChar ();
					SkipWhitespace ();
					break;
				default:
					throw NotWFError ("Invalid encoding declaration inside text declaration.");
				}
				// Encoding value should be checked inside XmlInputStream.
			}
			else
				throw NotWFError ("Encoding declaration is mandatory in text declaration.");

			Expect ("?>");
		}

		// Note that now this method behaves differently from
		// XmlTextReader's one. It calles AppendValueChar() internally.
		/*
		private int ReadCharacterReference ()
		{
			int value = 0;

			if (PeekChar () == 'x') {
				ReadChar ();

				while (PeekChar () != ';' && PeekChar () != -1) {
					int ch = ReadChar ();

					if (ch >= '0' && ch <= '9')
						value = (value << 4) + ch - '0';
					else if (ch >= 'A' && ch <= 'F')
						value = (value << 4) + ch - 'A' + 10;
					else if (ch >= 'a' && ch <= 'f')
						value = (value << 4) + ch - 'a' + 10;
					else
						throw NotWFError (String.Format (
								CultureInfo.InvariantCulture,
								"invalid hexadecimal digit: {0} (#x{1:X})",
								(char) ch,
								ch));
				}
			} else {
				while (PeekChar () != ';' && PeekChar () != -1) {
					int ch = ReadChar ();

					if (ch >= '0' && ch <= '9')
						value = value * 10 + ch - '0';
					else
						throw NotWFError (String.Format (
								CultureInfo.InvariantCulture,
								"invalid decimal digit: {0} (#x{1:X})",
								(char) ch,
								ch));
				}
			}

			ReadChar (); // ';'

			// There is no way to save surrogate pairs...
			if (XmlChar.IsInvalid (value))
				throw NotWFError ("Referenced character was not allowed in XML.");
			AppendValueChar (value);
			return value;
		}
		*/

		private void AppendNameChar (int ch)
		{
			CheckNameCapacity ();
			if (ch <= Char.MaxValue)
				nameBuffer [nameLength++] = (char) ch;
			else {
				nameBuffer [nameLength++] = (char) (ch / 0x10000 + 0xD800 - 1);
				CheckNameCapacity ();
				nameBuffer [nameLength++] = (char) (ch % 0x10000 + 0xDC00);
			}
		}

		private void CheckNameCapacity ()
		{
			if (nameLength == nameCapacity) {
				nameCapacity = nameCapacity * 2;
				char [] oldNameBuffer = nameBuffer;
				nameBuffer = new char [nameCapacity];
				Array.Copy (oldNameBuffer, nameBuffer, nameLength);
			}
		}

		private string CreateNameString ()
		{
			return DTD.NameTable.Add (nameBuffer, 0, nameLength);
		}

		private void AppendValueChar (int ch)
		{
			//See http://www.faqs.org/rfcs/rfc2781.html for used algorithm
			if (ch < 0x10000) {
				valueBuffer.Append ((char) ch);
				return;
			}
			if (ch > 0x10FFFF)
				throw new XmlException ("The numeric entity value is too large", null, LineNumber, LinePosition);
			else
			{
				int utag = ch - 0x10000;
				valueBuffer.Append((char) ((utag >> 10) + 0xD800));
				valueBuffer.Append((char) ((utag & 0x3FF) + 0xDC00));
			}
		}

		private string CreateValueString ()
		{
			return valueBuffer.ToString ();
		}
		
		private void ClearValueBuffer ()
		{
			valueBuffer.Length = 0;
		}

		// The reader is positioned on the quote character.
		// *Keeps quote char* to value to get_QuoteChar() correctly.
		private string ReadDefaultAttribute ()
		{
			ClearValueBuffer ();

			TryExpandPERef ();

			int quoteChar = ReadChar ();

			if (quoteChar != '\'' && quoteChar != '\"')
				throw NotWFError ("an attribute value was not quoted");

			AppendValueChar (quoteChar);

			while (PeekChar () != quoteChar) {
				int ch = ReadChar ();

				switch (ch)
				{
				case '<':
					throw NotWFError ("attribute values cannot contain '<'");
				case -1:
					throw NotWFError ("unexpected end of file in an attribute value");
				case '&':
					AppendValueChar (ch);
					if (PeekChar () == '#')
						break;
					// Check XML 1.0 section 3.1 WFC.
					string entName = ReadName ();
					Expect (';');
					if (XmlChar.GetPredefinedEntity (entName) < 0) {
						DTDEntityDeclaration entDecl = 
							DTD == null ? null : DTD.EntityDecls [entName];
						if (entDecl == null || entDecl.SystemId != null)
							// WFC: Entity Declared (see 4.1)
							if (DTD.IsStandalone || (DTD.SystemId == null && !DTD.InternalSubsetHasPEReference))
								throw NotWFError ("Reference to external entities is not allowed in attribute value.");
					}
					valueBuffer.Append (entName);
					AppendValueChar (';');
					break;
				default:
					AppendValueChar (ch);
					break;
				}
			}

			ReadChar (); // quoteChar
			AppendValueChar (quoteChar);

			return CreateValueString ();
		}

		private void PushParserInput (string url)
		{
			Uri baseUri = null;
			try {
				if (DTD.BaseURI != null && DTD.BaseURI.Length > 0)
					baseUri = new Uri (DTD.BaseURI);
			} catch (UriFormatException) {
			}

			Uri absUri = url != null && url.Length > 0 ?
				DTD.Resolver.ResolveUri (baseUri, url) : baseUri;
			string absPath = absUri != null ? absUri.ToString () : String.Empty;

			foreach (XmlParserInput i in parserInputStack.ToArray ()) {
				if (i.BaseURI == absPath)
					throw NotWFError ("Nested inclusion is not allowed: " + url);
			}
			parserInputStack.Push (currentInput);
			Stream s = null;
			MemoryStream ms = new MemoryStream ();
			try {
				s = DTD.Resolver.GetEntity (absUri, null, typeof (Stream)) as Stream;
				int size;
				byte [] buf = new byte [4096];
				do {
					size = s.Read (buf, 0, buf.Length);
					ms.Write (buf, 0, size);
				} while (size > 0);
				s.Close ();
				ms.Position = 0;
				currentInput = new XmlParserInput (new XmlStreamReader (ms), absPath);
			} catch (Exception ex) { // FIXME: (wishlist) Bad exception catch ;-(
				if (s != null)
					s.Close ();
				int line = currentInput == null ? 0 : currentInput.LineNumber;
				int col = currentInput == null ? 0 : currentInput.LinePosition;
				string bu = (currentInput == null) ? String.Empty : currentInput.BaseURI;
				HandleError (new XmlSchemaException ("Specified external entity not found. Target URL is " + url + " .",
					line, col, null, bu, ex));
				currentInput = new XmlParserInput (new StringReader (String.Empty), absPath);
			}
		}

		private void PopParserInput ()
		{
			currentInput.Close ();
			currentInput = parserInputStack.Pop () as XmlParserInput;
		}

		private void HandleError (XmlSchemaException ex)
		{
#if DTD_HANDLE_EVENTS
			if (this.ValidationEventHandler != null)
				ValidationEventHandler (this, new ValidationEventArgs (ex, ex.Message, XmlSeverityType.Error));
#else
			DTD.AddError (ex);
#endif
		}
	}
}
