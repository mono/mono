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
//   instructions, and comments are OK but there's no support for
//   entity/character references or namespaces yet.
//
//   It barfs on DOCTYPE declarations and CDATA sections.
//
//   There's also no checking being done for either well-formedness
//   or validity.
//
//   ParserContext and NameTables aren't being used yet.
//
//   The XmlTextReader-specific properties and methods have yet to
//   be added or implemented.
//
//   Some thought needs to be given to performance. There's too many
//   strings and string builders being allocated.
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
	public class XmlTextReader : XmlReader
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
			// TODO: implement me.
			throw new NotImplementedException();
		}

		public XmlTextReader(
			TextReader input,
			XmlNameTable nameTable)
		{
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
			// TODO: implement me.
			throw new NotImplementedException();
		}

		public XmlTextReader(
			string baseURI,
			TextReader input,
			XmlNameTable nameTable)
		{
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
				// TODO: implement me.
				return false;
			}
		}

		public override int Depth
		{
			get
			{
				// TODO: implement me.
				return 0;
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
				// TODO: implement me.
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

		public override string LocalName
		{
			get
			{
				// TODO: implement me.
				return null;
			}
		}

		public override string Name
		{
			get
			{
				return name;
			}
		}

		public override string NamespaceURI
		{
			get
			{
				// TODO: implement me.
				return null;
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

		public override string Prefix
		{
			get
			{
				// TODO: implement me.
				return null;
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

		public override string XmlLang
		{
			get
			{
				// TODO: implement me.
				return null;
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
			string namespaceName)
		{
			// TODO: implement me.
			return null;
		}

		public override string LookupNamespace(string prefix)
		{
			// TODO: implement me.
			return null;
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
			// TODO: implement me.
		}

		// privates

		private TextReader reader;
		private ReadState readState;

		private XmlNodeType nodeType;
		private string name;
		private bool isEmptyElement;
		private string value;
		private Hashtable attributes;

		private void Init()
		{
			readState = ReadState.Initial;

			nodeType = XmlNodeType.None;
			name = String.Empty;
			isEmptyElement = false;
			value = String.Empty;
			attributes = new Hashtable();
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
		}

		private void AddAttribute(string name, string value)
		{
			attributes.Add(name, value);
		}

		private void ClearAttributes()
		{
			attributes.Clear();
		}

		// This should really keep track of some state so
		// that it's not possible to have more than one document
		// element or text outside of the document element.
		private bool ReadContent()
		{
			bool more = false;

			switch (reader.Peek())
			{
			case '<':
				reader.Read();
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

			return more;
		}

		// The leading '<' has already been consumed.
		private void ReadTag()
		{
			switch (reader.Peek())
			{
			case '/':
				reader.Read();
				ReadEndTag();
				break;
			case '?':
				reader.Read();
				ReadProcessingInstruction();
				break;
			case '!':
				reader.Read();
				ReadComment();
				break;
			default:
				ReadStartTag();
				break;
			}
		}

		// The leading '<' has already been consumed.
		private void ReadStartTag()
		{
			string name = ReadName();
			SkipWhitespace();

			bool isEmptyElement = false;

			ClearAttributes();

			if (XmlChar.IsFirstNameChar(reader.Peek()))
			{
				ReadAttributes();
			}

			if (reader.Peek() == '/')
			{
				reader.Read();
				isEmptyElement = true;
			}

			Expect('>');

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

			SetProperties(
				XmlNodeType.EndElement, // nodeType
				name, // name
				false, // isEmptyElement
				String.Empty, // value
				true // clearAttributes
			);
		}

		// The reader is positioned on the first character
		// of the text.
		private void ReadText()
		{
			StringBuilder text = new StringBuilder();
			text.Append((char)reader.Read());

			while (reader.Peek() != '<' && reader.Peek() != -1)
			{
				text.Append((char)reader.Read());
			}

			SetProperties(
				XmlNodeType.Text, // nodeType
				String.Empty, // name
				false, // isEmptyElement
				text.ToString(), // value
				true // clearAttributes
			);
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

				AddAttribute(name, value);
			}
			while (reader.Peek() != '/' && reader.Peek() != '>' && reader.Peek() != -1);
		}

		// The reader is positioned on the quote character.
		private string ReadAttribute()
		{
			int quoteChar = reader.Read();

			if (quoteChar != '\'' && quoteChar != '\"')
			{
				throw new Exception("an attribute value was not quoted");
			}

			StringBuilder valueBuilder = new StringBuilder();

			while (reader.Peek() != quoteChar)
			{
				int ch = reader.Read();

				switch (ch)
				{
				case '<':
					throw new Exception("attribute values cannot contain '<'");
				case -1:
					throw new Exception("unexpected end of file in an attribute value");
				}

				valueBuilder.Append((char)ch);
			}

			reader.Read();

			return valueBuilder.ToString();
		}

		// The reader is positioned on the first character
		// of the target.
		private void ReadProcessingInstruction()
		{
			string target = ReadName();
			SkipWhitespace();

			StringBuilder valueBuilder = new StringBuilder();

			while (reader.Peek() != -1)
			{
				int ch = reader.Read();

				if (ch == '?' && reader.Peek() == '>')
				{
					reader.Read();
					break;
				}

				valueBuilder.Append((char)ch);
			}

			SetProperties(
				XmlNodeType.ProcessingInstruction, // nodeType
				target, // name
				false, // isEmptyElement
				valueBuilder.ToString(), // value
				true // clearAttributes
			);
		}

		// The reader is positioned on the first character after
		// the leading '<!'.
		private void ReadComment()
		{
			Expect('-');
			Expect('-');

			StringBuilder valueBuilder = new StringBuilder();

			while (reader.Peek() != -1)
			{
				int ch = reader.Read();

				if (ch == '-' && reader.Peek() == '-')
				{
					reader.Read();

					if (reader.Peek() != '>')
					{
						throw new Exception("comments cannot contain '--'");
					}

					reader.Read();
					break;
				}

				valueBuilder.Append((char)ch);
			}

			SetProperties(
				XmlNodeType.Comment, // nodeType
				String.Empty, // name
				false, // isEmptyElement
				valueBuilder.ToString(), // value
				true // clearAttributes
			);
		}

		// The reader is positioned on the first character
		// of the name.
		private string ReadName()
		{
			if (!XmlChar.IsFirstNameChar(reader.Peek()))
			{
				throw new Exception("a name did not start with a legal character");
			}

			StringBuilder nameBuilder = new StringBuilder();

			nameBuilder.Append((char)reader.Read());

			while (XmlChar.IsNameChar(reader.Peek()))
			{
				nameBuilder.Append((char)reader.Read());
			}

			return nameBuilder.ToString();
		}

		// Read the next character and compare it against the
		// specified character.
		private void Expect(int expected)
		{
			int ch = reader.Read();

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
			while (XmlChar.IsWhitespace(reader.Peek()))
			{
				reader.Read();
			}
		}
	}
}
