// -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*-
//
// System.Xml.XmlTextReader.cs
//
// Author:
//   Jason Diamond (jason@injektilo.org)
//
// (C) 2001 Jason Diamond  http://injektilo.org/
//

// FIXME:
//   This can only parse basic XML: elements, attributes, processing
//   instructions, and comments are OK.
//
//   It barfs on DOCTYPE declarations.
//
//   There's also no checking being done for either well-formedness
//   or validity.
//
//   ParserContext and NameTables aren't being used yet.
//
//   Some thought needs to be given to performance. There's too many
//   strings being allocated.
//
//   None of the MoveTo methods have been implemented yet.
//
//   LineNumber and LinePosition aren't being tracked.
//
//   xml:space, xml:lang, and xml:base aren't being tracked.
//
//   Depth isn't being tracked.

using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Text;

namespace System.Xml
{
	public class XmlTextReader : XmlReader, IXmlLineInfo
	{
		// constructors

		protected XmlTextReader()
		{
			Init();
		}

		public XmlTextReader(Stream input)
		{
			Init();
			reader = new StreamReader(
				input,
				Encoding.UTF8,
				true);
		}

		public XmlTextReader(string url)
		{
			Init();
			WebClient client = new WebClient();
			reader = new StreamReader(
				client.OpenRead(url),
				Encoding.UTF8,
				true);
		}

		public XmlTextReader(TextReader input)
		{
			Init();
			reader = input;
		}

 		public XmlTextReader(Stream input, XmlNameTable nameTable)
 		{
			this.nameTable = nameTable;

			// TODO: implement me.
			throw new NotImplementedException();
		}

		public XmlTextReader(string baseURI, Stream input)
		{
			// TODO: implement me.
			throw new NotImplementedException();
		}

		public XmlTextReader(string baseURI, TextReader input)
		{
			// TODO: implement me.
			throw new NotImplementedException();
		}

		public XmlTextReader(string url, XmlNameTable nameTable)
		{
			this.nameTable = nameTable;

			// TODO: implement me.
			throw new NotImplementedException();
		}

		public XmlTextReader(
			TextReader input,
			XmlNameTable nameTable)
		{
			this.nameTable = nameTable;

			// TODO: implement me.
			throw new NotImplementedException();
		}

		public XmlTextReader(
			Stream inputFragment,
			XmlNodeType fragmentType,
			XmlParserContext context)
		{
			// TODO: implement me.
			throw new NotImplementedException();
		}

		public XmlTextReader(
			string baseURI,
			Stream input,
			XmlNameTable nameTable)
		{
			this.nameTable = nameTable;

			// TODO: implement me.
			throw new NotImplementedException();
		}

		public XmlTextReader(
			string baseURI,
			TextReader input,
			XmlNameTable nameTable)
		{
			this.nameTable = nameTable;
			// TODO: implement me.
			throw new NotImplementedException();
		}

		public XmlTextReader(
			string fragment,
			XmlNodeType fragmentType,
			XmlParserContext context)
		{
			// TODO: implement me.
			throw new NotImplementedException();
		}

		// properties

		public override int AttributeCount
		{
			get
			{
				return attributes.Count;
			}
		}

		public override string BaseURI
		{
			get
			{
				// TODO: implement me.
				return null;
			}
		}

		public override bool CanResolveEntity
		{
			get
			{
				// XmlTextReaders don't resolve entities.
				return false;
			}
		}

		public override int Depth
		{
			get
			{
				// TODO: implement me.
				return depth > 0 ? depth : 0;
			}
		}

		public Encoding Encoding
		{
			get
			{
				// TODO: implement me.
				return null;
			}
		}

		public override bool EOF
		{
			get
			{
				return
					readState == ReadState.EndOfFile ||
					readState == ReadState.Closed;
			}
		}

		public override bool HasValue
		{
			get
			{
				return value != String.Empty;
			}
		}

		public override bool IsDefault
		{
			get
			{
				// XmlTextReader does not expand default attributes.
				return false;
			}
		}

		public override bool IsEmptyElement
		{
			get
			{
				return isEmptyElement;
			}
		}

		public override string this[int i]
		{
			get
			{
				return GetAttribute(i);
			}
		}

		public override string this[string name]
		{
			get
			{
				return GetAttribute(name);
			}
		}

		public override string this[
			string localName,
			string namespaceName]
		{
			get
			{
				return GetAttribute(localName, namespaceName);
			}
		}

		public int LineNumber
		{
			get
			{
				// TODO: implement me.
				return 0;
			}
		}

		public int LinePosition
		{
			get
			{
				// TODO: implement me.
				return 0;
			}
		}

		public override string LocalName
		{
			get
			{
				return localName;
			}
		}

		public override string Name
		{
			get
			{
				return name;
			}
		}

		public bool Namespaces
		{
			get
			{
				// TODO: implement me.
				return false;
			}

			set
			{
				// TODO: implement me.
			}
		}

		public override string NamespaceURI
		{
			get
			{
				return namespaceURI;
			}
		}

		public override XmlNameTable NameTable
		{
			get
			{
				// TODO: implement me.
				return null;
			}
		}

		public override XmlNodeType NodeType
		{
			get
			{
				return nodeType;
			}
		}

		public bool Normalization
		{
			get
			{
				// TODO: implement me.
				return false;
			}

			set
			{
				// TODO: implement me.
			}
		}

		public override string Prefix
		{
			get
			{
				return prefix;
			}
		}

		public override char QuoteChar
		{
			get
			{
				// TODO: implement me.
				return '"';
			}
		}

		public override ReadState ReadState
		{
			get
			{
				return readState;
			}
		}

		public override string Value
		{
			get
			{
				return value;
			}
		}

		public WhitespaceHandling WhitespaceHandling
		{
			get
			{
				// TODO: implement me.
				return WhitespaceHandling.All;
			}

			set
			{
				// TODO: implement me.
			}
		}

		public override string XmlLang
		{
			get
			{
				// TODO: implement me.
				return null;
			}
		}

		public XmlResolver XmlResolver
		{
			set
			{
				// TODO: implement me.
			}
		}

		public override XmlSpace XmlSpace
		{
			get
			{
				// TODO: implement me.
				return XmlSpace.Default;
			}
		}

		// methods

		public override void Close()
		{
			readState = ReadState.Closed;
		}

		public override string GetAttribute(int i)
		{
			// TODO: implement me.
			return null;
		}

		public override string GetAttribute(string name)
		{
			return (string)attributes[name];
		}

		public override string GetAttribute(
			string localName,
			string namespaceURI)
		{
			foreach (DictionaryEntry entry in attributes)
			{
				string thisName = (string)entry.Key;

				int indexOfColon = thisName.IndexOf(':');

				if (indexOfColon != -1)
				{
					string thisLocalName = thisName.Substring(indexOfColon + 1);

					if (localName == thisLocalName)
					{
						string thisPrefix = thisName.Substring(0, indexOfColon);
						string thisNamespaceURI = LookupNamespace(thisPrefix);

						if (namespaceURI == thisNamespaceURI)
						{
							return (string)attributes[thisName];
						}
					}
				}
			}

			return String.Empty;
		}

		public TextReader GetRemainder()
		{
			// TODO: implement me.
			return null;
		}

		// Why does this use explicit interface implementation?
		bool IXmlLineInfo.HasLineInfo()
		{
			// TODO: implement me.
			return false;
		}

		public override string LookupNamespace(string prefix)
		{
			return namespaceManager.LookupNamespace(prefix);
		}

		public override void MoveToAttribute(int i)
		{
			// TODO: implement me.
		}

		public override bool MoveToAttribute(string name)
		{
			// TODO: implement me.
			return false;
		}

		public override bool MoveToAttribute(
			string localName,
			string namespaceName)
		{
			// TODO: implement me.
			return false;
		}

		public override bool MoveToElement()
		{
			// TODO: implement me.
			return false;
		}

		public override bool MoveToFirstAttribute()
		{
			// TODO: implement me.
			return false;
		}

		public override bool MoveToNextAttribute()
		{
			// TODO: implement me.
			return false;
		}

		public override bool Read()
		{
			bool more = false;

			readState = ReadState.Interactive;

			more = ReadContent();

			return more;
		}

		public override bool ReadAttributeValue()
		{
			// TODO: implement me.
			return false;
		}

		public int ReadBase64(byte[] buffer, int offset, int length)
		{
			// TODO: implement me.
			return 0;
		}

		public int ReadBinHex(byte[] buffer, int offset, int length)
		{
			// TODO: implement me.
			return 0;
		}

		public int ReadChars(char[] buffer, int offset, int length)
		{
			// TODO: implement me.
			return 0;
		}

		public override string ReadInnerXml()
		{
			// TODO: implement me.
			return null;
		}

		public override string ReadOuterXml()
		{
			// TODO: implement me.
			return null;
		}

		public override string ReadString()
		{
			// TODO: implement me.
			return null;
		}

		public override void ResolveEntity()
		{
			// XmlTextReaders don't resolve entities.
			throw new InvalidOperationException("XmlTextReaders don't resolve entities.");
		}

		// privates

		private TextReader reader;
		private ReadState readState;

		private int depth;
		private bool depthDown;

		private XmlNameTable nameTable;
		private XmlNamespaceManager namespaceManager;
		private bool popScope;

		private XmlNodeType nodeType;
		private string name;
		private string prefix;
		private string localName;
		private string namespaceURI;
		private bool isEmptyElement;
		private string value;
		private Hashtable attributes;

		private bool returnEntityReference;
		private string entityReferenceName;

		private char[] nameBuffer;
		private int nameLength;
		private int nameCapacity;
		private const int initialNameCapacity = 256;

		private char[] valueBuffer;
		private int valueLength;
		private int valueCapacity;
		private const int initialValueCapacity = 8192;

		private void Init()
		{
			namespaceManager = new XmlNamespaceManager(nameTable);
			popScope = false;

			readState = ReadState.Initial;

			depth = -1;
			depthDown = false;

			nodeType = XmlNodeType.None;
			name = String.Empty;
			prefix = String.Empty;
			localName = string.Empty;
			isEmptyElement = false;
			value = String.Empty;
			attributes = new Hashtable();
			
			returnEntityReference = false;
			entityReferenceName = String.Empty;

			nameBuffer = new char[initialNameCapacity];
			nameLength = 0;
			nameCapacity = initialNameCapacity;

			valueBuffer = new char[initialValueCapacity];
			valueLength = 0;
			valueCapacity = initialValueCapacity;
		}

		// Use this method rather than setting the properties
		// directly so that all the necessary properties can
		// be changed in harmony with each other. Maybe the
		// fields should be in a seperate class to help enforce
		// this.
		private void SetProperties(
			XmlNodeType nodeType,
			string name,
			bool isEmptyElement,
			string value,
			bool clearAttributes)
		{
			this.nodeType = nodeType;
			this.name = name;
			this.isEmptyElement = isEmptyElement;
			this.value = value;

			if (clearAttributes)
			{
				ClearAttributes();
			}

			int indexOfColon = name.IndexOf(':');

			if (indexOfColon == -1)
			{
				prefix = String.Empty;
				localName = name;
			}
			else
			{
				prefix = name.Substring(0, indexOfColon);
				localName = name.Substring(indexOfColon + 1);
			}

			namespaceURI = LookupNamespace(prefix);
		}

		private void AddAttribute(string name, string value)
		{
			attributes.Add(name, value);
		}

		private void ClearAttributes()
		{
			if (attributes.Count > 0)
			{
				attributes.Clear();
			}
		}

		private int PeekChar()
		{
			return reader.Peek();
		}

		private int ReadChar()
		{
			return reader.Read();
		}

		// This should really keep track of some state so
		// that it's not possible to have more than one document
		// element or text outside of the document element.
		private bool ReadContent()
		{
			bool more = false;

			if (popScope)
			{
				namespaceManager.PopScope();
				popScope = false;
			}

			if (depthDown)
			{
				--depth;
			}

			if (returnEntityReference)
			{
				++depth;
				SetEntityReferenceProperties();
				more = true;
			}
			else
			{
				switch (PeekChar())
				{
				case '<':
					ReadChar();
					ReadTag();
					more = true;
					break;
				case -1:
					readState = ReadState.EndOfFile;
					SetProperties(
						XmlNodeType.None, // nodeType
						String.Empty, // name
						false, // isEmptyElement
						String.Empty, // value
						true // clearAttributes
					);
					more = false;
					break;
				default:
					ReadText();
					more = true;
					break;
				}
			}

			return more;
		}

		private void SetEntityReferenceProperties()
		{
			SetProperties(
				XmlNodeType.EntityReference, // nodeType
				entityReferenceName, // name
				false, // isEmptyElement
				String.Empty, // value
				true // clearAttributes
			);

			returnEntityReference = false;
			entityReferenceName = String.Empty;
		}

		// The leading '<' has already been consumed.
		private void ReadTag()
		{
			switch (PeekChar())
			{
			case '/':
				ReadChar();
				ReadEndTag();
				break;
			case '?':
				ReadChar();
				ReadProcessingInstruction();
				break;
			case '!':
				ReadChar();
				ReadDeclaration();
				break;
			default:
				ReadStartTag();
				break;
			}
		}

		// The leading '<' has already been consumed.
		private void ReadStartTag()
		{
			namespaceManager.PushScope();

			string name = ReadName();
			SkipWhitespace();

			bool isEmptyElement = false;

			ClearAttributes();

			if (XmlChar.IsFirstNameChar(PeekChar()))
			{
				ReadAttributes();
			}

			if (PeekChar() == '/')
			{
				ReadChar();
				isEmptyElement = true;
				depthDown = true;
				popScope = true;
			}

			Expect('>');

			++depth;

			SetProperties(
				XmlNodeType.Element, // nodeType
				name, // name
				isEmptyElement, // isEmptyElement
				String.Empty, // value
				false // clearAttributes
			);
		}

		// The reader is positioned on the first character
		// of the element's name.
		private void ReadEndTag()
		{
			string name = ReadName();
			SkipWhitespace();
			Expect('>');

			--depth;

			SetProperties(
				XmlNodeType.EndElement, // nodeType
				name, // name
				false, // isEmptyElement
				String.Empty, // value
				true // clearAttributes
			);

			popScope = true;
		}

		private void AppendNameChar(int ch)
		{
			CheckNameCapacity();
			nameBuffer[nameLength++] = (char)ch;
		}

		private void CheckNameCapacity()
		{
			if (nameLength == nameCapacity)
			{
				nameCapacity = nameCapacity * 2;
				char[] oldNameBuffer = nameBuffer;
				nameBuffer = new char[nameCapacity];
				Array.Copy(oldNameBuffer, nameBuffer, nameLength);
			}
		}

		private string CreateNameString()
		{
			return new String(nameBuffer, 0, nameLength);
		}

		private void AppendValueChar(int ch)
		{
			CheckValueCapacity();
			valueBuffer[valueLength++] = (char)ch;
		}

		private void CheckValueCapacity()
		{
			if (valueLength == valueCapacity)
			{
				valueCapacity = valueCapacity * 2;
				char[] oldValueBuffer = valueBuffer;
				valueBuffer = new char[valueCapacity];
				Array.Copy(oldValueBuffer, valueBuffer, valueLength);
			}
		}

		private string CreateValueString()
		{
			return new String(valueBuffer, 0, valueLength);
		}

		// The reader is positioned on the first character
		// of the text.
		private void ReadText()
		{
			valueLength = 0;

			int ch = PeekChar();

			while (ch != '<' && ch != -1)
			{
				if (ch == '&')
				{
					ReadChar();

					if (ReadReference(false))
					{
						break;
					}
				}
				else
				{
					AppendValueChar(ReadChar());
				}

				ch = PeekChar();
			}

			if (returnEntityReference && valueLength == 0)
			{
				++depth;
				SetEntityReferenceProperties();
			}
			else
			{
				if (depth >= 0)
				{
					++depth;
					depthDown = true;
				}

				SetProperties(
					XmlNodeType.Text, // nodeType
					String.Empty, // name
					false, // isEmptyElement
					CreateValueString(), // value
					true // clearAttributes
				);
			}
		}

		// The leading '&' has already been consumed.
		// Returns true if the entity reference isn't a simple
		// character reference or one of the predefined entities.
		// This allows the ReadText method to break so that the
		// next call to Read will return the EntityReference node.
		private bool ReadReference(bool ignoreEntityReferences)
		{
			if (PeekChar() == '#')
			{
				ReadChar();
				ReadCharacterReference();
			}
			else
			{
				ReadEntityReference(ignoreEntityReferences);
			}

			return returnEntityReference;
		}

		private void ReadCharacterReference()
		{
			int value = 0;

			if (PeekChar() == 'x')
			{
				ReadChar();

				while (PeekChar() != ';' && PeekChar() != -1)
				{
					int ch = ReadChar();

					if (ch >= '0' && ch <= '9')
					{
						value = (value << 4) + ch - '0';
					}
					else if (ch >= 'A' && ch <= 'F')
					{
						value = (value << 4) + ch - 'A' + 10;
					}
					else if (ch >= 'a' && ch <= 'f')
					{
						value = (value << 4) + ch - 'a' + 10;
					}
					else
					{
						throw new Exception(
							String.Format(
								"invalid hexadecimal digit: {0} (#x{1:X})",
								(char)ch,
								ch));
					}
				}
			}
			else
			{
				while (PeekChar() != ';' && PeekChar() != -1)
				{
					int ch = ReadChar();

					if (ch >= '0' && ch <= '9')
					{
						value = value * 10 + ch - '0';
					}
					else
					{
						throw new Exception(
							String.Format(
								"invalid decimal digit: {0} (#x{1:X})",
								(char)ch,
								ch));
					}
				}
			}

			ReadChar(); // ';'

			AppendValueChar(value);
		}

		private void ReadEntityReference(bool ignoreEntityReferences)
		{
			nameLength = 0;

			int ch = PeekChar();

			while (ch != ';' && ch != -1)
			{
				AppendNameChar(ReadChar());
				ch = PeekChar();
			}

			Expect(';');

			string name = CreateNameString();

			switch (name)
			{
				case "lt":
					AppendValueChar('<');
					break;
				case "gt":
					AppendValueChar('>');
					break;
				case "amp":
					AppendValueChar('&');
					break;
				case "apos":
					AppendValueChar('\'');
					break;
				case "quot":
					AppendValueChar('"');
					break;
				default:
					if (ignoreEntityReferences)
					{
						AppendValueChar('&');

						foreach (char ch2 in name)
						{
							AppendValueChar(ch2);
						}

						AppendValueChar(';');
					}
					else
					{
						returnEntityReference = true;
						entityReferenceName = name;
					}
					break;
			}
		}

		// The reader is positioned on the first character of
		// the attribute name.
		private void ReadAttributes()
		{
			do
			{
				string name = ReadName();
				SkipWhitespace();
				Expect('=');
				SkipWhitespace();
				string value = ReadAttribute();
				SkipWhitespace();

				if (name == "xmlns")
				{
					namespaceManager.AddNamespace(String.Empty, value);
				}
				else if (name.StartsWith("xmlns:"))
				{
					namespaceManager.AddNamespace(name.Substring(6), value);
				}
				else
				{
					AddAttribute(name, value);
				}
			}
			while (PeekChar() != '/' && PeekChar() != '>' && PeekChar() != -1);
		}

		// The reader is positioned on the quote character.
		private string ReadAttribute()
		{
			int quoteChar = ReadChar();

			if (quoteChar != '\'' && quoteChar != '\"')
			{
				throw new Exception("an attribute value was not quoted");
			}

			valueLength = 0;

			while (PeekChar() != quoteChar)
			{
				int ch = ReadChar();

				switch (ch)
				{
				case '<':
					throw new Exception("attribute values cannot contain '<'");
				case '&':
					ReadReference(true);
					break;
				case -1:
					throw new Exception("unexpected end of file in an attribute value");
				default:
					AppendValueChar(ch);
					break;
				}
			}

			ReadChar(); // quoteChar

			return CreateValueString();
		}

		// The reader is positioned on the first character
		// of the target.
		private void ReadProcessingInstruction()
		{
			string target = ReadName();
			SkipWhitespace();

			valueLength = 0;

			while (PeekChar() != -1)
			{
				int ch = ReadChar();

				if (ch == '?' && PeekChar() == '>')
				{
					ReadChar();
					break;
				}

				AppendValueChar((char)ch);
			}

			SetProperties(
				XmlNodeType.ProcessingInstruction, // nodeType
				target, // name
				false, // isEmptyElement
				CreateValueString(), // value
				true // clearAttributes
			);
		}

		// The reader is positioned on the first character after
		// the leading '<!'.
		private void ReadDeclaration()
		{
			int ch = PeekChar();

			switch (ch)
			{
			case '-':
				Expect('-');
				Expect('-');
				ReadComment();
				break;
			case '[':
				ReadChar();
				Expect('C');
				Expect('D');
				Expect('A');
				Expect('T');
				Expect('A');
				Expect('[');
				ReadCDATA();
				break;
			}
		}

		// The reader is positioned on the first character after
		// the leading '<!--'.
		private void ReadComment()
		{
			valueLength = 0;

			while (PeekChar() != -1)
			{
				int ch = ReadChar();

				if (ch == '-' && PeekChar() == '-')
				{
					ReadChar();

					if (PeekChar() != '>')
					{
						throw new Exception("comments cannot contain '--'");
					}

					ReadChar();
					break;
				}

				AppendValueChar((char)ch);
			}

			SetProperties(
				XmlNodeType.Comment, // nodeType
				String.Empty, // name
				false, // isEmptyElement
				CreateValueString(), // value
				true // clearAttributes
			);
		}

		// The reader is positioned on the first character after
		// the leading '<![CDATA['.
		private void ReadCDATA()
		{
			valueLength = 0;

			while (PeekChar() != -1)
			{
				int ch = ReadChar();

				if (ch == ']' && PeekChar() == ']')
				{
					ch = ReadChar(); // ']'

					if (PeekChar() == '>')
					{
						ReadChar(); // '>'
						break;
					}
					else
					{
						AppendValueChar(']');
						AppendValueChar(']');
						ch = ReadChar();
					}
				}

				AppendValueChar((char)ch);
			}

			++depth;

			SetProperties(
				XmlNodeType.CDATA, // nodeType
				String.Empty, // name
				false, // isEmptyElement
				CreateValueString(), // value
				true // clearAttributes
			);
		}

		// The reader is positioned on the first character
		// of the name.
		private string ReadName()
		{
			if (!XmlChar.IsFirstNameChar(PeekChar()))
			{
				throw new Exception("a name did not start with a legal character");
			}

			nameLength = 0;

			AppendNameChar(ReadChar());

			while (XmlChar.IsNameChar(PeekChar()))
			{
				AppendNameChar(ReadChar());
			}

			return CreateNameString();
		}

		// Read the next character and compare it against the
		// specified character.
		private void Expect(int expected)
		{
			int ch = ReadChar();

			if (ch != expected)
			{
				throw new Exception(String.Format(
					"expected '{0}' ({1:X}) but found '{2}' ({3:X})",
					(char)expected,
					expected,
					(char)ch,
					ch));
			}
		}

		// Does not consume the first non-whitespace character.
		private void SkipWhitespace()
		{
			while (XmlChar.IsWhitespace(PeekChar()))
			{
				ReadChar();
			}
		}
	}
}
