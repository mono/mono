//
// System.Xml.XmlTextReader
//
// Author:
//   Jason Diamond (jason@injektilo.org)
//   Adam Treat (manyoso@yahoo.com)
//   Atsushi Enomoto  (ginga@kit.hi-ho.ne.jp)
//
// (C) 2001, 2002 Jason Diamond  http://injektilo.org/
//

// FIXME:
//
//   NameTables aren't being used completely yet.
//
//   Some thought needs to be given to performance. There's too many
//   strings being allocated.
//
//   If current node is on an Attribute, Prefix might be null, and
//   in several fields which uses XmlReader, it should be considered.
//

using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Xml.Schema;
using Mono.Xml;
using Mono.Xml.Native;

namespace System.Xml
{
	public class XmlTextReader : XmlReader, IXmlLineInfo
	{
		#region Constructors

		protected XmlTextReader ()
		{
		}

		public XmlTextReader (Stream input)
			: this (new XmlStreamReader (input))
		{
		}

		public XmlTextReader (string url)
			: this(url, new NameTable ())
		{
		}

		public XmlTextReader (TextReader input)
			: this (input, new NameTable ())
		{
		}

		protected XmlTextReader (XmlNameTable nt)
			: this (String.Empty, null, XmlNodeType.None, null)
		{
		}

		public XmlTextReader (Stream input, XmlNameTable nt)
			: this(new XmlStreamReader (input), nt)
 		{
		}

		public XmlTextReader (string url, Stream input)
			: this (url, new XmlStreamReader (input))
		{
		}

		public XmlTextReader (string url, TextReader input)
			: this (url, input, new NameTable ())
		{
		}

		public XmlTextReader (string url, XmlNameTable nt)
			: this (url, new XmlStreamReader (url, null, null), nt)
		{
		}

		public XmlTextReader (TextReader input, XmlNameTable nt)
			: this (String.Empty, input, nt)
		{
		}

		public XmlTextReader (Stream xmlFragment, XmlNodeType fragType, XmlParserContext context)
			: this (context != null ? context.BaseURI : String.Empty,
				new XmlStreamReader (xmlFragment),
			fragType,
			context)
		{
		}

		public XmlTextReader (string url, Stream input, XmlNameTable nt)
			: this (url, new XmlStreamReader (input), nt)
		{
		}

		public XmlTextReader (string url, TextReader input, XmlNameTable nt)
			: this (url, input, XmlNodeType.Document, null)
		{
		}

		public XmlTextReader (string xmlFragment, XmlNodeType fragType, XmlParserContext context)
			: this (context != null ? context.BaseURI : String.Empty,
				new StringReader (xmlFragment),
				fragType,
				context)
		{
		}

		XmlTextReader (string url, TextReader fragment, XmlNodeType fragType, XmlParserContext context)
		{
			InitializeContext (url, context, fragment, fragType);
		}

		#endregion

		#region Properties

		public override int AttributeCount
		{
			get { return attributeCount; }
		}

		public override string BaseURI
		{
			get { return parserContext.BaseURI; }
		}

		public override int Depth
		{
			get {
				if (currentAttributeValue >= 0)
					return elementDepth + 2; // inside attribute value.
				else if (currentAttribute >= 0)
					return elementDepth + 1;
				return elementDepth;
			}
		}

		public Encoding Encoding
		{
			get { return parserContext.Encoding; }
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
			get { 
				if (this.valueBuilderAvailable)
					return valueBuilder.Length != 0;
				else
					return cursorToken.Value != null;
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
			get { return cursorToken.IsEmptyElement; }
		}

		public override string this [int i]
		{
			get { return GetAttribute (i); }
		}

		public override string this [string name]
		{
			get { return GetAttribute (name); }
		}

		public override string this [string localName, string namespaceName]
		{
			get { return GetAttribute (localName, namespaceName); }
		}

		public int LineNumber
		{
			get {
				if (useProceedingLineInfo)
					return line;
				else
					return cursorToken.LineNumber;
			}
		}

		public int LinePosition
		{
			get {
				if (useProceedingLineInfo)
					return column;
				else
					return cursorToken.LinePosition;
			}
		}

		public override string LocalName
		{
			get { return cursorToken.LocalName; }
		}

		public override string Name
		{
			get { return cursorToken.Name; }
		}

		public bool Namespaces
		{
			get { return namespaces; }
			set { 
				if (readState != ReadState.Initial)
					throw new InvalidOperationException ("Namespaces have to be set before reading.");
				namespaces = value;
			}
		}

		public override string NamespaceURI
		{
			get { return cursorToken.NamespaceURI; }
		}

		public override XmlNameTable NameTable
		{
			get { return parserContext.NameTable; }
		}

		public override XmlNodeType NodeType
		{
			get { return cursorToken.NodeType; }
		}

		[MonoTODO]
		public bool Normalization
		{
			get { return normalization; }
			set { normalization = value; }
		}

		public override string Prefix
		{
			get { return cursorToken.Prefix; }
		}

		public override char QuoteChar
		{
			get { return cursorToken.QuoteChar; }
		}

		public override ReadState ReadState
		{
			get { return readState; }
		}

		public override string Value
		{
			get { return cursorToken.Value != null ? cursorToken.Value : String.Empty; }
		}

		public WhitespaceHandling WhitespaceHandling
		{
			get { return whitespaceHandling; }
			set { whitespaceHandling = value; }
		}

		public override string XmlLang
		{
			get { return parserContext.XmlLang; }
		}

		public XmlResolver XmlResolver
		{
			set { resolver = value; }
		}

		public override XmlSpace XmlSpace
		{
			get { return parserContext.XmlSpace; }
		}

		#endregion

		#region Methods

		public override void Close ()
		{
			readState = ReadState.Closed;

			cursorToken.Clear ();
			currentToken.Clear ();
			attributeCount = 0;
		}

		public override string GetAttribute (int i)
		{
			if (i > attributeCount)
				throw new ArgumentOutOfRangeException ("i is smaller than AttributeCount");
			else {
				return attributeTokens [i].Value;
			}
		}

		// MS.NET 1.0 msdn says that this method returns String.Empty
		// for absent attribute, but in fact it returns null.
		// This description is corrected in MS.NET 1.1 msdn.
		public override string GetAttribute (string name)
		{
			for (int i = 0; i < attributeCount; i++)
				if (attributeTokens [i].Name == name)
					return attributeTokens [i].Value;
			return null;
		}

		private int GetIndexOfQualifiedAttribute (string localName, string namespaceURI)
		{
			for (int i = 0; i < attributeCount; i++) {
				XmlAttributeTokenInfo ti = attributeTokens [i];
				if (ti.LocalName == localName && ti.NamespaceURI == namespaceURI)
					return i;
			}
			return -1;
		}

		internal XmlParserContext GetInternalParserContext ()
		{
			return parserContext;
		}

		public override string GetAttribute (string localName, string namespaceURI)
		{
			int idx = this.GetIndexOfQualifiedAttribute (localName, namespaceURI);
			if (idx < 0)
				return null;
			return attributeTokens [idx].Value;
		}

		public TextReader GetRemainder ()
		{
			StringBuilder sb = null;
			if (this.hasPeekChars) {
				sb = new StringBuilder ();
				int end = 0;
				for (; end < 6; end++)
					if (peekChars [end] <= 0)
						break;
				sb.Append (peekChars, 0, end);
			}
			if (has_peek && peek_char > 0) {
				if (sb != null)
					sb.Append ((char) peek_char);
				else
					return new StringReader (((char) peek_char) + reader.ReadToEnd ());
			}
			// As long as less memory consumptive...
			if (sb != null)
				return new StringReader (sb.ToString () + reader.ReadToEnd ());
			else
				return new StringReader (reader.ReadToEnd ());
		}

		bool IXmlLineInfo.HasLineInfo ()
		{
			return true;
		}

		public override string LookupNamespace (string prefix)
		{
			return parserContext.NamespaceManager.LookupNamespace (prefix);
		}

		public override void MoveToAttribute (int i)
		{
			if (i >= attributeCount)
				throw new ArgumentOutOfRangeException ("attribute index out of range.");

			currentAttribute = i;
			currentAttributeValue = -1;
			cursorToken = attributeTokens [i];
		}

		public override bool MoveToAttribute (string name)
		{
			for (int i = 0; i < attributeCount; i++) {
				XmlAttributeTokenInfo ti = attributeTokens [i];
				if (ti.Name == name) {
					MoveToAttribute (i);
					return true;
				}
			}
			return false;
		}

		public override bool MoveToAttribute (string localName, string namespaceName)
		{
			int idx = GetIndexOfQualifiedAttribute (localName, namespaceName);
			if (idx < 0)
				return false;
			MoveToAttribute (idx);
			return true;
		}

		public override bool MoveToElement ()
		{
			if (currentToken == null)	// for attribute .ctor()
				return false;

			if (cursorToken == currentToken)
				return false;

			if (currentAttribute >= 0) {
				currentAttribute = -1;
				currentAttributeValue = -1;
				cursorToken = currentToken;
				return true;
			}
			else
				return false;
		}

		public override bool MoveToFirstAttribute ()
		{
			if (attributeCount == 0)
				return false;
			MoveToElement ();
			return MoveToNextAttribute ();
		}

		public override bool MoveToNextAttribute ()
		{
			if (currentAttribute == 0 && attributeCount == 0)
				return false;
			if (currentAttribute + 1 < attributeCount) {
				currentAttribute++;
				currentAttributeValue = -1;
				cursorToken = attributeTokens [currentAttribute];
				return true;
			}
			else
				return false;
		}

		public override bool Read ()
		{
			if (startNodeType == XmlNodeType.Attribute) {
				if (currentAttribute == 0)
					return false;	// already read.
				ClearAttributes ();
				IncrementAttributeToken ();
				ReadAttributeValueTokens ('"');
				cursorToken = attributeTokens [0];
				currentAttributeValue = -1;
				readState = ReadState.Interactive;
				return true;
			}

			bool more = false;
			readState = ReadState.Interactive;
			currentLinkedNodeLineNumber = line;
			currentLinkedNodeLinePosition = column;
			useProceedingLineInfo = true;

			cursorToken = currentToken;
			attributeCount = 0;
			currentAttribute = currentAttributeValue = -1;
			currentToken.Clear ();

			// It was moved from end of ReadStartTag ().
			if (depthUp)
				++depth;
			depthUp = false;

			if (shouldSkipUntilEndTag) {
				shouldSkipUntilEndTag = false;
				return ReadUntilEndTag ();
			}

			more = ReadContent ();

			if (depth == 0 && !allowMultipleRoot && (IsEmptyElement || NodeType == XmlNodeType.EndElement))
				currentState = XmlNodeType.EndElement;

			if (!more && startNodeType == XmlNodeType.Document && currentState != XmlNodeType.EndElement)
				throw new XmlException ("Document element did not appear.");

			useProceedingLineInfo = false;
			return more;
		}

		public override bool ReadAttributeValue ()
		{
			if (readState == ReadState.Initial && startNodeType == XmlNodeType.Attribute) {
				Read ();
			}

			if (currentAttribute < 0)
				return false;
			XmlAttributeTokenInfo ti = attributeTokens [currentAttribute];
			if (currentAttributeValue < 0)
				currentAttributeValue = ti.ValueTokenStartIndex - 1;

			if (currentAttributeValue < ti.ValueTokenEndIndex) {
				currentAttributeValue++;
				cursorToken = attributeValueTokens [currentAttributeValue];
				return true;
			}
			else
				return false;
		}

		[MonoTODO ("It looks to keep incomplete byte block.")]
		public int ReadBase64 (byte [] buffer, int offset, int length)
		{
			if (offset < 0)
				throw new ArgumentOutOfRangeException ("offset", offset, "Offset must be non-negative integer.");
			else if (length < 0)
				throw new ArgumentOutOfRangeException ("length", length, "Length must be non-negative integer.");
			else if (buffer.Length < offset + length)
				throw new ArgumentOutOfRangeException ("buffer length is smaller than the sum of offset and length.");

			int max = (int) System.Math.Ceiling (4.0 / 3 * length);
			char [] chars = new char [max];
			int charsLength = ReadChars (chars, 0, max);

			int bufIndex = offset;
			for (int i = 0; i < charsLength - 3; i += 4) {
				buffer [bufIndex] = (byte) (GetBase64Byte (chars [i]) << 2);
				if (i + 1 == charsLength)
					break;
				byte b = GetBase64Byte (chars [i + 1]);
				buffer [bufIndex] += (byte) (b >> 4);
				bufIndex++;
				buffer [bufIndex] = (byte) ((b & 0xf) << 4);
				if (i + 2 == charsLength)
					break;
				b = GetBase64Byte (chars [i + 2]);
				buffer [bufIndex] += (byte) (b >> 2);
				bufIndex++;
				buffer [bufIndex] = (byte) ((b & 3) << 6);
				if (i + 3 == charsLength)
					break;
				buffer [bufIndex] += GetBase64Byte (chars [i + 3]);
				bufIndex++;
			}
			return (int) System.Math.Ceiling (4.0 / 3 * max);
		}

		public int ReadBinHex (byte [] buffer, int offset, int length)
		{
			if (offset < 0)
				throw new ArgumentOutOfRangeException ("offset", offset, "Offset must be non-negative integer.");
			else if (length < 0)
				throw new ArgumentOutOfRangeException ("length", length, "Length must be non-negative integer.");
			else if (buffer.Length < offset + length)
				throw new ArgumentOutOfRangeException ("buffer length is smaller than the sum of offset and length.");

			char [] chars = new char [length * 2];
			int charsLength = ReadChars (chars, 0, length * 2);
			int bufIndex = offset;
			for (int i = 0; i < charsLength - 1; i += 2) {
				buffer [bufIndex] = (chars [i] > '9' ?
						(byte) (chars [i] - 'A' + 10) :
						(byte) (chars [i] - '0'));
				buffer [bufIndex] <<= 4;
				buffer [bufIndex] += chars [i + 1] > '9' ?
						(byte) (chars [i + 1] - 'A' + 10) : 
						(byte) (chars [i + 1] - '0');
				bufIndex++;
			}
			if (charsLength %2 != 0)
				buffer [bufIndex++] = (byte)
					((chars [charsLength - 1] > '9' ?
						(byte) (chars [charsLength - 1] - 'A' + 10) :
						(byte) (chars [charsLength - 1] - '0'))
					<< 4);

			return bufIndex - offset;
		}

		public int ReadChars (char [] buffer, int offset, int length)
		{
			return ReadCharsInternal (buffer, offset, length);
		}

#if NET_1_0
		public override string ReadInnerXml ()
		{
			if (readState != ReadState.Interactive)
				return String.Empty;

			switch (NodeType) {
			case XmlNodeType.Attribute:
				return value.Substring (1, value.Length - 2);
			case XmlNodeType.Element:
				if (IsEmptyElement)
					return String.Empty;

				int startDepth = depth;

				if (innerXmlBuilder == null)
					innerXmlBuilder = new StringBuilder ();
				innerXmlBuilder.Length = 0;
				bool loop = true;
				do {
					Read ();
					if (NodeType ==XmlNodeType.None)
						throw new XmlException ("unexpected end of xml.");
					else if (NodeType == XmlNodeType.EndElement && depth == startDepth) {
						loop = false;
						Read ();
					}
					else
						innerXmlBuilder.Append (currentTag);
				} while (loop);
				string xml = innerXmlBuilder.ToString ();
				innerXmlBuilder.Length = 0;
				return xml;
			case XmlNodeType.None:
				// MS document is incorrect. Seems not to progress.
				return String.Empty;
			default:
				Read ();
				return String.Empty;
			}
		}

		public override string ReadOuterXml ()
		{
			if (readState != ReadState.Interactive)
				return String.Empty;

			switch (NodeType) {
			case XmlNodeType.Attribute:
				// strictly incompatible with MS... (it holds spaces attribute between name, value and "=" char (very trivial).
				return String.Format ("{0}={1}{2}{1}", Name, QuoteChar, ReadInnerXml ());
			case XmlNodeType.Element:
				bool isEmpty = IsEmptyElement;
				string startTag = currentTag.ToString ();
				string name = Name;

				if (NodeType == XmlNodeType.Element && !isEmpty)
					return String.Format ("{0}{1}</{2}>", startTag, ReadInnerXml (), name);
				else
					return currentTag.ToString ();
			case XmlNodeType.None:
				// MS document is incorrect. Seems not to progress.
				return String.Empty;
			default:
				Read ();
				return String.Empty;
			}
		}
#endif

		public override string ReadString ()
		{
			return ReadStringInternal ();
		}

		public void ResetState ()
		{
			Init ();
		}

		public override void ResolveEntity ()
		{
			// XmlTextReader does not resolve entities.
			throw new InvalidOperationException ("XmlTextReader cannot resolve external entities.");
		}

		#endregion

		#region Internals
		// Parsed DTD Objects
		internal DTDObjectModel DTD {
			get { return parserContext.Dtd; }
		}
		#endregion

		#region Privates
		internal class XmlTokenInfo
		{
			public XmlTokenInfo (XmlTextReader xtr)
			{
				Reader = xtr;
				Clear ();
			}

			string valueCache;

			protected XmlTextReader Reader;

			public string Name;
			public string LocalName;
			public string Prefix;
			public string NamespaceURI;
			public bool IsEmptyElement;
			public char QuoteChar;
			public int LineNumber;
			public int LinePosition;

			public XmlNodeType NodeType;

			public virtual string Value {
				get {
					if (valueCache != null)
						return valueCache;
					else if (Reader.valueBuilderAvailable) {
						valueCache = Reader.valueBuilder.ToString ();
						return valueCache;
					}
					return valueCache;
				}
				set {
					valueCache = value;
				}
			}

			public virtual void Clear ()
			{
				valueCache = null;
				NodeType = XmlNodeType.None;
				Name = LocalName = Prefix = NamespaceURI = String.Empty;
				IsEmptyElement = false;
				QuoteChar = '"';
				LineNumber = LinePosition = 0;
			}

			internal virtual void FillNames ()
			{
				if (Reader.Namespaces) {
					int indexOfColon = Name.IndexOf (':');

					if (indexOfColon == -1) {
						Prefix = String.Empty;
						LocalName = Name;
					} else {
						Prefix = Reader.NameTable.Add (Name.Substring (0, indexOfColon));
						LocalName = Reader.NameTable.Add (Name.Substring (indexOfColon + 1));
					}

					// NamespaceURI
					switch (NodeType) {
					case XmlNodeType.Attribute:
						if (Prefix == string.Empty)
							NamespaceURI = string.Empty;
						else
							NamespaceURI = Reader.LookupNamespace (Prefix);
						break;

					case XmlNodeType.Element:
					case XmlNodeType.EndElement:
						NamespaceURI = Reader.LookupNamespace (Prefix);
						break;
					default:
						NamespaceURI = "";
						break;
					}
				} else {
					Prefix = String.Empty;
					LocalName = Name;
				}
			}
		}

		internal class XmlAttributeTokenInfo : XmlTokenInfo
		{
			public XmlAttributeTokenInfo (XmlTextReader reader)
				: base (reader)
			{
				NodeType = XmlNodeType.Attribute;
			}

			public int ValueTokenStartIndex;
			public int ValueTokenEndIndex;
			string valueCache;

			public override string Value {
				get {
					if (valueCache != null)
						return valueCache;
					// An empty value should return String.Empty.
					if (ValueTokenStartIndex == ValueTokenEndIndex) {
						XmlTokenInfo ti = Reader.attributeValueTokens [ValueTokenStartIndex];
						if (ti.NodeType == XmlNodeType.Text)
							valueCache = ti.Value;
						else
							valueCache = String.Concat ("&", ti.Name, ";");
						return valueCache;
					}

					StringBuilder sb = new StringBuilder ();
					for (int i = ValueTokenStartIndex; i <= ValueTokenEndIndex; i++) {
						XmlTokenInfo ti = Reader.attributeValueTokens [i];
						if (ti.NodeType == XmlNodeType.Text)
							sb.Append (ti.Value);
						else {
							sb.Append ('&');
							sb.Append (ti.Name);
							sb.Append (';');
						}
					}

					valueCache = sb.ToString ();

					return valueCache;
				}
				set {
					valueCache = value;
				}
			}

			public override void Clear ()
			{
				base.Clear ();
				valueCache = null;
				NodeType = XmlNodeType.Attribute;
				ValueTokenStartIndex = ValueTokenEndIndex = 0;
			}

			internal override void FillNames ()
			{
				base.FillNames ();
				if (Prefix == "xmlns" || Name == "xmlns")
					NamespaceURI = XmlNamespaceManager.XmlnsXmlns;
			}
		}

		private XmlTokenInfo cursorToken;
		private XmlTokenInfo currentToken;
		private XmlAttributeTokenInfo currentAttributeToken;
		private XmlTokenInfo currentAttributeValueToken;
		private XmlAttributeTokenInfo [] attributeTokens = new XmlAttributeTokenInfo [10];
		private XmlTokenInfo [] attributeValueTokens = new XmlTokenInfo [10];
		private int currentAttribute;
		private int currentAttributeValue;
		private int attributeCount;

		private XmlParserContext parserContext;

		private ReadState readState;

		private int depth;
		private int elementDepth;
		private bool depthUp;

		private bool popScope;
		private Stack elementStack = new Stack();
		private bool allowMultipleRoot;

		private bool isStandalone;

		private StringBuilder valueBuilder;
		private bool valueBuilderAvailable = false;

		private bool returnEntityReference;
		private string entityReferenceName;

		private char [] nameBuffer;
		private int nameLength;
		private int nameCapacity;
		private const int initialNameCapacity = 256;

		private StringBuilder valueBuffer = new StringBuilder (512);

		private TextReader reader;
		private bool can_seek;
		private bool has_peek;
		private int peek_char;
		private bool hasPeekChars;
		private char [] peekChars;
		private int peekCharsIndex;

		private int line;
		private int column;
		private StringBuilder currentTag = new StringBuilder ();

		private int currentLinkedNodeLineNumber;
		private int currentLinkedNodeLinePosition;
		private bool useProceedingLineInfo;

		// A buffer for ReadContent for ReadOuterXml

		private XmlNodeType startNodeType;
		// State machine attribute.
		//	XmlDeclaration: after the first node.
		//	DocumentType: after doctypedecl
		//	Element: inside document element
		//	EndElement: after document element
		private XmlNodeType currentState;

		// For ReadChars()/ReadBase64()/ReadBinHex()
		private bool shouldSkipUntilEndTag;

		// These values are never re-initialized.
		private bool namespaces = true;
		private WhitespaceHandling whitespaceHandling = WhitespaceHandling.All;
		private XmlResolver resolver = new XmlUrlResolver ();
		private bool normalization = false;

		private void Init ()
		{
			currentToken = new XmlTokenInfo (this);
			cursorToken = currentToken;
			currentAttribute = -1;
			currentAttributeValue = -1;
			attributeCount = 0;

			readState = ReadState.Initial;
			allowMultipleRoot = false;

			depth = 0;
			elementDepth = 0;
			depthUp = false;

			popScope = allowMultipleRoot = false;
			elementStack.Clear ();

			isStandalone = false;
			valueBuilderAvailable = false;
			returnEntityReference = false;
			entityReferenceName = String.Empty;

			nameBuffer = new char [initialNameCapacity];
			nameLength = 0;
			nameCapacity = initialNameCapacity;

			can_seek = has_peek = false;
			peek_char = peekCharsIndex = 0;
			peekChars = new char [6];

			line = 1;
			column = 0;
			currentTag.Length = 0;

			valueBuffer.Length = 0;

			currentLinkedNodeLineNumber = currentLinkedNodeLinePosition = 0;
			useProceedingLineInfo = false;

			currentState = XmlNodeType.None;

			shouldSkipUntilEndTag = false;
		}

		private void InitializeContext (string url, XmlParserContext context, TextReader fragment, XmlNodeType fragType)
		{
			startNodeType = fragType;
			parserContext = context;
			if (context == null) {
				XmlNameTable nt = new NameTable ();
				parserContext = new XmlParserContext (nt,
					new XmlNamespaceManager (nt),
					String.Empty,
					XmlSpace.None);
			}

			if (url != null && url != String.Empty) {
				string path = Path.GetFullPath ("./a");
				Uri uri = new Uri (new Uri (path), url);
				parserContext.BaseURI = uri.ToString ();
			}

			Init ();

			switch (fragType) {
			case XmlNodeType.Attribute:
				fragment = new StringReader (fragment.ReadToEnd ().Replace ("\"", "&quot;"));
				break;
			case XmlNodeType.Element:
				currentState = XmlNodeType.Element;
				allowMultipleRoot = true;
				break;
			case XmlNodeType.Document:
				break;
			default:
				throw new XmlException (String.Format ("NodeType {0} is not allowed to create XmlTextReader.", fragType));
			}

			reader = fragment;
		}

		// Use this method rather than setting the properties
		// directly so that all the necessary properties can
		// be changed in harmony with each other. Maybe the
		// fields should be in a seperate class to help enforce
		// this.
		private void SetProperties (
			XmlNodeType nodeType,
			string name,
			bool isEmptyElement,
			string value,
			bool clearAttributes)
		{
			SetProperties (currentToken, nodeType, name, isEmptyElement, value, clearAttributes);
			currentToken.LineNumber = this.currentLinkedNodeLineNumber;
			currentToken.LinePosition = this.currentLinkedNodeLinePosition;
		}

		private void SetProperties (
			XmlTokenInfo token,
			XmlNodeType nodeType,
			string name,
			bool isEmptyElement,
			string value,
			bool clearAttributes)
		{
			this.valueBuilderAvailable = false;
			token.Clear ();
			token.NodeType = nodeType;
			token.Name = name;
			token.IsEmptyElement = isEmptyElement;
			token.Value = value;
			this.elementDepth = depth;

			if (clearAttributes)
				ClearAttributes ();

			token.FillNames ();
		}

		private void SetProperties (
			XmlNodeType nodeType,
			string name,
			bool isEmptyElement,
			bool clearAttributes,
			StringBuilder value) {
			SetProperties (nodeType, name, isEmptyElement, (string)null, clearAttributes);
			this.valueBuilderAvailable = true;
			this.valueBuilder = value;
		}

		private void ClearAttributes ()
		{
			for (int i = 0; i < attributeCount; i++)
				attributeTokens [i].Clear ();
			attributeCount = 0;
			currentAttribute = -1;
			currentAttributeValue = -1;
		}

		/*
		private int PeekChar ()
		{
			return currentInput.PeekChar ();
		}

		private int ReadChar ()
		{
			return currentInput.ReadChar ();
		}
		*/
		public int PeekChar ()
		{
			if (can_seek)
				return reader.Peek ();

			if (hasPeekChars)
				return peekChars [peekCharsIndex];

			if (has_peek)
				return peek_char;

			peek_char = reader.Read ();
			has_peek = true;
			return peek_char;
		}

		public int ReadChar ()
		{
			int ch;

			if (hasPeekChars) {
				ch = peekChars [peekCharsIndex++];
				if (peekChars [peekCharsIndex] == 0)
					hasPeekChars = false;
			} else if (has_peek) {
				ch = peek_char;
				has_peek = false;
			} else {
				ch = reader.Read ();
			}

			if (ch == '\n') {
				line++;
				column = 1;
			} else {
				column++;
			}
			currentTag.Append ((char) ch);
			return ch;
		}

		// This should really keep track of some state so
		// that it's not possible to have more than one document
		// element or text outside of the document element.
		private bool ReadContent ()
		{
			currentTag.Length = 0;
			if (popScope) {
				parserContext.NamespaceManager.PopScope ();
				popScope = false;
			}

			if (returnEntityReference)
				SetEntityReferenceProperties ();
			else {
    				switch (PeekChar ()) {
				case '<':
					ReadChar ();
					ReadTag ();
					break;
				case '\r': goto case ' ';
				case '\n': goto case ' ';
				case '\t': goto case ' ';
				case ' ':
					if (whitespaceHandling == WhitespaceHandling.All ||
						whitespaceHandling == WhitespaceHandling.Significant)
						ReadWhitespace ();
					else {
						SkipWhitespace ();
						return ReadContent ();
					}
					break;
				case -1:
					readState = ReadState.EndOfFile;
					SetProperties (
						XmlNodeType.None, // nodeType
						String.Empty, // name
						false, // isEmptyElement
						(string) null, // value
						true // clearAttributes
					);
					if (depth > 0)
						throw new XmlException ("unexpected end of file. Current depth is " + depth);

					return false;
				default:
					ReadText (true);
					break;
				}
			}
			return this.ReadState != ReadState.EndOfFile;
		}

		private void SetEntityReferenceProperties ()
		{
			DTDEntityDeclaration decl = DTD != null ? DTD.EntityDecls [entityReferenceName] : null;
//			if (DTD != null && resolver != null && decl == null)
//				throw new XmlException (this as IXmlLineInfo, "Entity declaration does not exist.");
			if (this.isStandalone)
				if (DTD == null || decl == null || !decl.IsInternalSubset)
					throw new XmlException (this as IXmlLineInfo,
						"Standalone document must not contain any references to an non-internally declared entity.");
			if (decl != null && decl.NotationName != null)
				throw new XmlException (this as IXmlLineInfo,
					"Reference to any unparsed entities is not allowed here.");

			SetProperties (
				XmlNodeType.EntityReference, // nodeType
				entityReferenceName, // name
				false, // isEmptyElement
				(string) null, // value
				true // clearAttributes
			);

			returnEntityReference = false;
			entityReferenceName = String.Empty;
		}

		// The leading '<' has already been consumed.
		private void ReadTag ()
		{
			switch (PeekChar ())
			{
			case '/':
				ReadChar ();
				ReadEndTag ();
				break;
			case '?':
				ReadChar ();
				ReadProcessingInstruction ();
				break;
			case '!':
				ReadChar ();
				ReadDeclaration ();
				break;
			default:
				ReadStartTag ();
				break;
			}
		}

		// The leading '<' has already been consumed.
		private void ReadStartTag ()
		{
			if (currentState == XmlNodeType.EndElement)
				throw new XmlException (this as IXmlLineInfo,
					"Element cannot appear in this state.");
			currentState = XmlNodeType.Element;

			parserContext.NamespaceManager.PushScope ();

			string name = ReadName ();
			if (currentState == XmlNodeType.EndElement)
				throw new XmlException (this as IXmlLineInfo,"document has terminated, cannot open new element");

			bool isEmptyElement = false;

			ClearAttributes ();

			SkipWhitespace ();
			if (XmlChar.IsFirstNameChar (PeekChar ()))
				ReadAttributes (false);
			cursorToken = this.currentToken;

			// fill namespaces
			for (int i = 0; i < attributeCount; i++)
				attributeTokens [i].FillNames ();

			// quick name check
			for (int i = 0; i < attributeCount; i++)
				for (int j = i + 1; j < attributeCount; j++)
					if (Object.ReferenceEquals (attributeTokens [i].Name, attributeTokens [j].Name) ||
						(Object.ReferenceEquals (attributeTokens [i].LocalName, attributeTokens [j].LocalName) &&
						Object.ReferenceEquals (attributeTokens [i].NamespaceURI, attributeTokens [j].NamespaceURI)))
						throw new XmlException (this as IXmlLineInfo,
							"Attribute name and qualified name must be identical.");

			string baseUri = GetAttribute ("xml:base");
			if (baseUri != null)
				parserContext.BaseURI = baseUri;
			string xmlLang = GetAttribute ("xml:lang");
			if (xmlLang != null)
				parserContext.XmlLang = xmlLang;
			string xmlSpaceAttr = GetAttribute ("xml:space");
			if (xmlSpaceAttr != null) {
				if (xmlSpaceAttr == "preserve")
					parserContext.XmlSpace = XmlSpace.Preserve;
				else if (xmlSpaceAttr == "default")
					parserContext.XmlSpace = XmlSpace.Default;
				else
					throw new XmlException (this as IXmlLineInfo,String.Format ("Invalid xml:space value: {0}", xmlSpaceAttr));
			}
			if (PeekChar () == '/') {
				ReadChar ();
				isEmptyElement = true;
				popScope = true;
			}
			else {
				depthUp = true;
				elementStack.Push (name);
				parserContext.PushScope ();
			}

			Expect ('>');

			SetProperties (
				XmlNodeType.Element, // nodeType
				name, // name
				isEmptyElement, // isEmptyElement
				(string) null, // value
				false // clearAttributes
			);
		}

		// The reader is positioned on the first character
		// of the element's name.
		private void ReadEndTag ()
		{
			if (currentState != XmlNodeType.Element)
				throw new XmlException (this as IXmlLineInfo,
					"End tag cannot appear in this state.");

			string name = ReadName ();
			if (elementStack.Count == 0)
				throw new XmlException (this as IXmlLineInfo,"closing element without matching opening element");
			string expected = (string)elementStack.Pop();
			if (expected != name)
				throw new XmlException (this as IXmlLineInfo,String.Format ("unmatched closing element: expected {0} but found {1}", expected, name));
			parserContext.PopScope ();

			SkipWhitespace ();
			Expect ('>');

			--depth;

			SetProperties (
				XmlNodeType.EndElement, // nodeType
				name, // name
				false, // isEmptyElement
				(string) null, // value
				true // clearAttributes
			);

			popScope = true;
		}

		private void AppendNameChar (int ch)
		{
			CheckNameCapacity ();
			nameBuffer [nameLength++] = (char)ch;
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
			return parserContext.NameTable.Add (nameBuffer, 0, nameLength);
		}

		private void AppendValueChar (int ch)
		{
			valueBuffer.Append ((char)ch);
		}

		private string CreateValueString ()
		{
			return valueBuffer.ToString ();
		}
		
		private void ClearValueBuffer ()
		{
			valueBuffer.Length = 0;
		}

		// The reader is positioned on the first character
		// of the text.
		private void ReadText (bool notWhitespace)
		{
			if (currentState != XmlNodeType.Element)
				throw new XmlException (this as IXmlLineInfo,
					"Text node cannot appear in this state.");

			if (notWhitespace)
				ClearValueBuffer ();

			int ch = PeekChar ();
			int previousCloseBracketLine = 0;
			int previousCloseBracketColumn = 0;

			while (ch != '<' && ch != -1) {
				if (ch == '&') {
					ReadChar ();
					if (ReadReference (false))
						break;
				} else {
					if (XmlConstructs.IsInvalid (ch))
						throw new XmlException (this as IXmlLineInfo,
							"Not allowed character was found.");
					AppendValueChar (ReadChar ());
					if (ch == ']') {
						if (previousCloseBracketColumn == LinePosition - 1 &&
							previousCloseBracketLine == LineNumber)
							if (PeekChar () == '>')
								throw new XmlException (this as IXmlLineInfo,
									"Inside text content, character sequence ']]>' is not allowed.");
						previousCloseBracketColumn = LinePosition;
						previousCloseBracketLine = LineNumber;
					}
				}
				ch = PeekChar ();
				notWhitespace = true;
			}

			if (returnEntityReference && valueBuffer.Length == 0) {
				SetEntityReferenceProperties ();
			} else {
				XmlNodeType nodeType = notWhitespace ? XmlNodeType.Text :
					this.XmlSpace == XmlSpace.Preserve ? XmlNodeType.SignificantWhitespace : XmlNodeType.Whitespace;
				SetProperties (
					nodeType, // nodeType
					String.Empty, // name
					false, // isEmptyElement
					true, // clearAttributes
					valueBuffer // value
				);
			}
		}

		// The leading '&' has already been consumed.
		// Returns true if the entity reference isn't a simple
		// character reference or one of the predefined entities.
		// This allows the ReadText method to break so that the
		// next call to Read will return the EntityReference node.
		private bool ReadReference (bool ignoreEntityReferences)
		{
			if (PeekChar () == '#') {
				ReadChar ();
				ReadCharacterReference ();
			} else
				ReadEntityReference (ignoreEntityReferences);

			return returnEntityReference;
		}

		private void ReadCharacterReference ()
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
						throw new XmlException (this as IXmlLineInfo,
							String.Format (
								"invalid hexadecimal digit: {0} (#x{1:X})",
								(char)ch,
								ch));
				}
			} else {
				while (PeekChar () != ';' && PeekChar () != -1) {
					int ch = ReadChar ();

					if (ch >= '0' && ch <= '9')
						value = value * 10 + ch - '0';
					else
						throw new XmlException (this as IXmlLineInfo,
							String.Format (
								"invalid decimal digit: {0} (#x{1:X})",
								(char)ch,
								ch));
				}
			}

			ReadChar (); // ';'

			// FIXME: how to handle such chars larger than 0xffff?
			if (value < 0xffff && !XmlConstructs.IsValid (value))
				throw new XmlException (this as IXmlLineInfo,
					"Referenced character was not allowed in XML.");
			AppendValueChar (value);
		}

		private void ReadEntityReference (bool ignoreEntityReferences)
		{
			nameLength = 0;

			int ch = PeekChar ();

			while (ch != ';' && ch != -1) {
				AppendNameChar (ReadChar ());
				ch = PeekChar ();
			}

			Expect (';');

			string name = CreateNameString ();
			if (!XmlChar.IsName (name))
				throw new XmlException (this as IXmlLineInfo,
					"Invalid entity reference name was found.");

			char predefined = XmlChar.GetPredefinedEntity (name);
			if (predefined != 0)
				AppendValueChar (predefined);
			else {
				if (ignoreEntityReferences) {
					AppendValueChar ('&');

					foreach (char ch2 in name) {
						AppendValueChar (ch2);
					}

					AppendValueChar (';');
				} else {
					returnEntityReference = true;
					entityReferenceName = name;
				}
			}
		}

		// The reader is positioned on the first character of
		// the attribute name.
		private void ReadAttributes (bool endsWithQuestion)
		{
			int peekChar = -1;
			bool requireWhitespace = false;
			currentAttribute = -1;
			currentAttributeValue = -1;

			do {
				if (!SkipWhitespace () && requireWhitespace)
					throw new XmlException ("Unexpected token. Name is required here.");

				IncrementAttributeToken ();
				currentAttributeToken.LineNumber = line;
				currentAttributeToken.LinePosition = column;

				currentAttributeToken.Name = ReadName ();
				SkipWhitespace ();
				Expect ('=');
				SkipWhitespace ();
				ReadAttributeValueTokens (-1);
				attributeCount++;

				if (currentAttributeToken.Name == "xmlns")
					parserContext.NamespaceManager.AddNamespace (String.Empty, GetAttribute (currentAttribute));
				else if (currentAttributeToken.Name.StartsWith ("xmlns:")) {
					string nsPrefix = NameTable.Add (currentAttributeToken.Name.Substring (6));
					parserContext.NamespaceManager.AddNamespace (nsPrefix, GetAttribute (currentAttribute));
				}

				if (!SkipWhitespace ())
					requireWhitespace = true;
				peekChar = PeekChar ();
				if (endsWithQuestion) {
					if (peekChar == '?')
						break;
				}
				else if (peekChar == '/' || peekChar == '>')
					break;
			} while (peekChar != -1);

			currentAttribute = -1;
			currentAttributeValue = -1;
		}

		private void AddAttribute (string name, string value)
		{
			IncrementAttributeToken ();
			XmlAttributeTokenInfo ati = attributeTokens [currentAttribute];
			ati.Name = "SYSTEM";
			ati.FillNames ();
			IncrementAttributeValueToken ();
			XmlTokenInfo vti = attributeValueTokens [currentAttributeValue];
			vti.Value = value;
			SetProperties (vti, XmlNodeType.Text, name, false, value, false);
			attributeCount++;
		}

		private void IncrementAttributeToken ()
		{
			currentAttribute++;
			if (attributeTokens.Length == currentAttribute) {
				XmlAttributeTokenInfo [] newArray = 
					new XmlAttributeTokenInfo [attributeTokens.Length * 2];
				attributeTokens.CopyTo (newArray, 0);
				attributeTokens = newArray;
			}
			if (attributeTokens [currentAttribute] == null)
				attributeTokens [currentAttribute] = new XmlAttributeTokenInfo (this);
			currentAttributeToken = attributeTokens [currentAttribute];
			currentAttributeToken.Clear ();
		}

		private void IncrementAttributeValueToken ()
		{
			ClearValueBuffer ();
			currentAttributeValue++;
			if (attributeValueTokens.Length == currentAttributeValue) {
				XmlTokenInfo [] newArray = new XmlTokenInfo [attributeValueTokens.Length * 2];
				attributeValueTokens.CopyTo (newArray, 0);
				attributeValueTokens = newArray;
			}
			if (attributeValueTokens [currentAttributeValue] == null)
				attributeValueTokens [currentAttributeValue] = new XmlTokenInfo (this);
			currentAttributeValueToken = attributeValueTokens [currentAttributeValue];
			currentAttributeValueToken.Clear ();
		}

		private void ReadAttributeValueTokens (int dummyQuoteChar)
		{
			int quoteChar = (dummyQuoteChar < 0) ? ReadChar () : dummyQuoteChar;

			if (quoteChar != '\'' && quoteChar != '\"')
				throw new XmlException (this as IXmlLineInfo,"an attribute value was not quoted");
			currentAttributeToken.QuoteChar = (char) quoteChar;

			IncrementAttributeValueToken ();
			currentAttributeToken.ValueTokenStartIndex = currentAttributeValue;
			currentAttributeValueToken.LineNumber = line;
			currentAttributeValueToken.LinePosition = column;

			bool incrementToken = false;
			bool isNewToken = true;
			bool loop = true;
			while (loop && PeekChar () != quoteChar) {
				if (incrementToken) {
					IncrementAttributeValueToken ();
					currentAttributeValueToken.LineNumber = line;
					currentAttributeValueToken.LinePosition = column;
					incrementToken = false;
					isNewToken = true;
				}

				int ch = ReadChar ();

				switch (ch)
				{
				case '<':
					throw new XmlException (this as IXmlLineInfo,"attribute values cannot contain '<'");
				case -1:
					if (dummyQuoteChar < 0)
						throw new XmlException (this as IXmlLineInfo,"unexpected end of file in an attribute value");
					else	// Attribute value constructor.
						loop = false;
					break;
				case '&':
					int startPosition = currentTag.Length - 1;
					if (PeekChar () == '#') {
						ReadChar ();
						this.ReadCharacterReference ();
						break;
					}
					// Check XML 1.0 section 3.1 WFC.
					string entName = ReadName ();
					Expect (';');
					int predefined = XmlChar.GetPredefinedEntity (entName);
					if (predefined == 0) {
						DTDEntityDeclaration entDecl = 
							DTD == null ? null : DTD.EntityDecls [entName];
						if (DTD != null && resolver != null && entDecl == null)
							throw new XmlException (this as IXmlLineInfo, "Entity declaration does not exist.");
						if (entDecl != null && entDecl.HasExternalReference)
							throw new XmlException (this as IXmlLineInfo,
								"Reference to external entities is not allowed in the value of an attribute.");
						if (isStandalone && !entDecl.IsInternalSubset)
							throw new XmlException (this as IXmlLineInfo,
								"Reference to external entities is not allowed in the value of an attribute.");
						if (entDecl != null && entDecl.EntityValue.IndexOf ('<') >= 0)
							throw new XmlException (this as IXmlLineInfo,
								"Attribute must not contain character '<' either directly or indirectly by way of entity references.");
						currentAttributeValueToken.Value = CreateValueString ();
						currentAttributeValueToken.NodeType = XmlNodeType.Text;
						if (!isNewToken)
							IncrementAttributeValueToken ();
						currentAttributeValueToken.Name = entName;
						currentAttributeValueToken.Value = String.Empty;
						currentAttributeValueToken.NodeType = XmlNodeType.EntityReference;
						incrementToken = true;
					}
					else
						AppendValueChar (predefined);
					break;
				default:
					AppendValueChar (ch);
					break;
				}

				isNewToken = false;
			}
			if (!incrementToken) {
				currentAttributeValueToken.Value = CreateValueString ();
				currentAttributeValueToken.NodeType = XmlNodeType.Text;
			}
			currentAttributeToken.ValueTokenEndIndex = currentAttributeValue;

			if (dummyQuoteChar < 0)
				ReadChar (); // quoteChar
		}

		// The reader is positioned on the first character
		// of the target.
		//
		// It may be xml declaration or processing instruction.
		private void ReadProcessingInstruction ()
		{
			string target = ReadName ();
			if (target == "xml") {
				ReadXmlDeclaration ();
				return;
			} else if (target.ToLower () == "xml")
				throw new XmlException (this as IXmlLineInfo,
					"Not allowed processing instruction name which starts with 'X', 'M', 'L' was found.");

			if (currentState == XmlNodeType.None)
				currentState = XmlNodeType.XmlDeclaration;

			if (!SkipWhitespace ())
				if (PeekChar () != '?')
					throw new XmlException (this as IXmlLineInfo,
						"Invalid processing instruction name was found.");

			ClearValueBuffer ();

			while (PeekChar () != -1) {
				int ch = ReadChar ();

				if (ch == '?' && PeekChar () == '>') {
					ReadChar ();
					break;
				}

				AppendValueChar ((char)ch);
			}

			SetProperties (
				XmlNodeType.ProcessingInstruction, // nodeType
				target, // name
				false, // isEmptyElement
				true, // clearAttributes
				valueBuffer // value
			);
		}

		// The reader is positioned after "<?xml "
		private void ReadXmlDeclaration ()
		{
			if (currentState != XmlNodeType.None) {
				throw new XmlException (this as IXmlLineInfo,
					"XML declaration cannot appear in this state.");
			}
			currentState = XmlNodeType.XmlDeclaration;

			ClearAttributes ();

			ReadAttributes (true);	// They must have "version."
			string version = GetAttribute ("version");

			string message = null;

			if (attributeTokens [0].Name != "version" || version != "1.0")
				message = "Version 1.0 declaration is required in XML Declaration.";
			else if (attributeCount > 1 &&
					(attributeTokens [1].Name != "encoding" &&
					attributeTokens [1].Name != "standalone"))
				message = "Invalid Xml Declaration markup was found.";
			else if (attributeCount > 2 && attributeTokens [2].Name != "standalone")
				message = "Invalid Xml Declaration markup was found.";
			string sa = GetAttribute ("standalone");
			if (sa != null && sa != "yes" && sa != "no")
				message = "Only 'yes' or 'no' is allowed for standalone.";

			this.isStandalone = (sa == "yes");

			if (message != null)
				throw new XmlException (this as IXmlLineInfo, message);

			SetProperties (
				XmlNodeType.XmlDeclaration, // nodeType
				"xml", // name
				false, // isEmptyElement
				currentTag.ToString (6, currentTag.Length - 6), // value
				false // clearAttributes
			);

			Expect ("?>");
		}

		internal void SkipTextDeclaration ()
		{
			this.currentState = XmlNodeType.Element;

			if (PeekChar () != '<')
				return;

			ReadChar ();
			peekChars [0] = '<';

			if (PeekChar () != '?') {
				hasPeekChars = true;
				return;
			}
			ReadChar ();
			peekChars [1] = '?';

			for (int i = 2; i < 6; i++) {
				if (PeekChar () == 0)
					break;
				else
					peekChars [i] = (char) ReadChar ();
			}
			if (new string (peekChars, 2, 4) != "xml ") {
				if (new string (peekChars, 2, 3).ToLower () == "xml") {
					throw new XmlException (this as IXmlLineInfo,
						"Processing instruction name must not be character sequence 'X' 'M' 'L' with case insensitivity.");
				}
				hasPeekChars = true;
				return;
			}

			for (int i = 0; i < 6; i++)
				peekChars [i] = '\0';

			SkipWhitespace ();

			// version decl
			if (PeekChar () == 'v') {
				Expect ("version");
				SkipWhitespace ();
				Expect ('=');
				SkipWhitespace ();
				int quoteChar = ReadChar ();
				char [] expect1_0 = new char [3];
				int versionLength = 0;
				switch (quoteChar) {
				case '\'':
				case '"':
					while (PeekChar () != quoteChar) {
						if (PeekChar () == -1)
							throw new XmlException (this as IXmlLineInfo,
								"Invalid version declaration inside text declaration.");
						else if (versionLength == 3)
							throw new XmlException (this as IXmlLineInfo,
								"Invalid version number inside text declaration.");
						else {
							expect1_0 [versionLength] = (char) ReadChar ();
							versionLength++;
							if (versionLength == 3 && new String (expect1_0) != "1.0")
								throw new XmlException (this as IXmlLineInfo,
									"Invalid version number inside text declaration.");
						}
					}
					ReadChar ();
					SkipWhitespace ();
					break;
				default:
					throw new XmlException (this as IXmlLineInfo,
						"Invalid version declaration inside text declaration.");
				}
			}

			if (PeekChar () == 'e') {
				Expect ("encoding");
				SkipWhitespace ();
				Expect ('=');
				SkipWhitespace ();
				int quoteChar = ReadChar ();
				switch (quoteChar) {
				case '\'':
				case '"':
					while (PeekChar () != quoteChar)
						if (ReadChar () == -1)
							throw new XmlException (this as IXmlLineInfo,
								"Invalid encoding declaration inside text declaration.");
					ReadChar ();
					SkipWhitespace ();
					break;
				default:
					throw new XmlException (this as IXmlLineInfo,
						"Invalid encoding declaration inside text declaration.");
				}
				// Encoding value should be checked inside XmlInputStream.
			}
			else
				throw new XmlException (this as IXmlLineInfo,
					"Encoding declaration is mandatory in text declaration.");

			Expect ("?>");
		}

		// The reader is positioned on the first character after
		// the leading '<!'.
		private void ReadDeclaration ()
		{
			int ch = PeekChar ();

			switch (ch)
			{
			case '-':
				Expect ("--");
				ReadComment ();
				break;
			case '[':
				ReadChar ();
				Expect ("CDATA[");
				ReadCDATA ();
				break;
			case 'D':
				Expect ("DOCTYPE");
				ReadDoctypeDecl ();
				break;
			default:
				throw new XmlException (this as IXmlLineInfo,
					"Unexpected declaration markup was found.");
			}
		}

		// The reader is positioned on the first character after
		// the leading '<!--'.
		private void ReadComment ()
		{
			if (currentState == XmlNodeType.None)
				currentState = XmlNodeType.XmlDeclaration;

			ClearValueBuffer ();

			while (PeekChar () != -1) {
				int ch = ReadChar ();

				if (ch == '-' && PeekChar () == '-') {
					ReadChar ();

					if (PeekChar () != '>')
						throw new XmlException (this as IXmlLineInfo,"comments cannot contain '--'");

					ReadChar ();
					break;
				}

				if (XmlConstructs.IsInvalid (ch))
					throw new XmlException (this as IXmlLineInfo,
						"Not allowed character was found.");

				AppendValueChar ((char)ch);
			}

			SetProperties (
				XmlNodeType.Comment, // nodeType
				String.Empty, // name
				false, // isEmptyElement
				true, // clearAttributes
				valueBuffer // value
			);
		}

		// The reader is positioned on the first character after
		// the leading '<![CDATA['.
		private void ReadCDATA ()
		{
			if (currentState != XmlNodeType.Element)
				throw new XmlException (this as IXmlLineInfo,
					"CDATA section cannot appear in this state.");

			ClearValueBuffer ();

			bool skip = false;
			int ch = 0;
			while (PeekChar () != -1) {
				if (!skip)
					ch = ReadChar ();
				skip = false;

				if (ch == ']' && PeekChar () == ']') {
					ch = ReadChar (); // ']'

					if (PeekChar () == '>') {
						ReadChar (); // '>'
						break;
					} else {
						skip = true;
//						AppendValueChar (']');
//						AppendValueChar (']');
//						ch = ReadChar ();
					}
				}

				AppendValueChar ((char)ch);
			}

			SetProperties (
				XmlNodeType.CDATA, // nodeType
				String.Empty, // name
				false, // isEmptyElement
				true, // clearAttributes
				valueBuffer // value
			);
		}

		// The reader is positioned on the first character after
		// the leading '<!DOCTYPE'.
		private void ReadDoctypeDecl ()
		{
			switch (currentState) {
			case XmlNodeType.DocumentType:
			case XmlNodeType.Element:
			case XmlNodeType.EndElement:
				throw new XmlException (this as IXmlLineInfo,
					"Document type cannot appear in this state.");
			}
			currentState = XmlNodeType.DocumentType;

			string doctypeName = null;
			string publicId = null;
			string systemId = null;
			int intSubsetStartLine = 0;
			int intSubsetStartColumn = 0;

			SkipWhitespace ();
			doctypeName = ReadName ();
			SkipWhitespace ();
			switch(PeekChar ())
			{
			case 'S':
				systemId = ReadSystemLiteral (true);
				break;
			case 'P':
				publicId = ReadPubidLiteral ();
				if (!SkipWhitespace ())
					throw new XmlException (this as IXmlLineInfo,
						"Whitespace is required between PUBLIC id and SYSTEM id.");
				systemId = ReadSystemLiteral (false);
				break;
			}
			SkipWhitespace ();


			if(PeekChar () == '[')
			{
				// read markupdecl etc. or end of decl
				ReadChar ();
				intSubsetStartLine = this.LineNumber;
				intSubsetStartColumn = this.LinePosition;
				int startPos = currentTag.Length;
				ReadInternalSubset ();
				int endPos = currentTag.Length - 1;
				parserContext.InternalSubset = currentTag.ToString (startPos, endPos - startPos);
			}
			// end of DOCTYPE decl.
			SkipWhitespace ();
			Expect ('>');

			GenerateDTDObjectModel (doctypeName, publicId,
				systemId, parserContext.InternalSubset,
				intSubsetStartLine, intSubsetStartColumn);

			// set properties for <!DOCTYPE> node
			SetProperties (
				XmlNodeType.DocumentType, // nodeType
				doctypeName, // name
				false, // isEmptyElement
				parserContext.InternalSubset, // value
				true // clearAttributes
				);

			if (publicId != null)
				AddAttribute ("PUBLIC", publicId);
			if (systemId != null)
				AddAttribute ("SYSTEM", systemId);
		}

		internal DTDObjectModel GenerateDTDObjectModel (string name, string publicId,
			string systemId, string internalSubset)
		{
			return GenerateDTDObjectModel (name, publicId, systemId, internalSubset, 0, 0);
		}

		internal DTDObjectModel GenerateDTDObjectModel (string name, string publicId,
			string systemId, string internalSubset, int intSubsetStartLine, int intSubsetStartColumn)
		{
			// now compile DTD
			parserContext.Dtd = new DTDObjectModel (this.NameTable);	// merges both internal and external subsets in the meantime,
			DTD.BaseURI = BaseURI;
			DTD.Name = name;
			DTD.PublicId = publicId;
			DTD.SystemId = systemId;
			DTD.InternalSubset = internalSubset;
			DTD.XmlResolver = resolver;
			DTD.IsStandalone = isStandalone;
			DTD.LineNumber = line;
			DTD.LinePosition = column;

			return new DTDReader (DTD, intSubsetStartLine, intSubsetStartColumn).GenerateDTDObjectModel ();
		}

		private enum DtdInputState
		{
			Free = 1,
			ElementDecl,
			AttlistDecl,
			EntityDecl,
			NotationDecl,
			PI,
			Comment,
			InsideSingleQuoted,
			InsideDoubleQuoted,
		}

		private class DtdInputStateStack
		{
			Stack intern = new Stack ();
			public DtdInputStateStack ()
			{
				Push (DtdInputState.Free);
			}

			public DtdInputState Peek ()
			{
				return (DtdInputState) intern.Peek ();
			}

			public DtdInputState Pop ()
			{
				return (DtdInputState) intern.Pop ();
			}

			public void Push (DtdInputState val)
			{
				intern.Push (val);
			}
		}


		DtdInputStateStack stateStack = new DtdInputStateStack ();
		DtdInputState State {
			get { return stateStack.Peek (); }
		}

		// Simply read but not generate any result.
		private void ReadInternalSubset ()
		{
			bool continueParse = true;

			while (continueParse) {
				switch (ReadChar ()) {
				case ']':
					switch (State) {
					case DtdInputState.Free:
						continueParse = false;
						break;
					case DtdInputState.InsideDoubleQuoted:
						continue;
					case DtdInputState.InsideSingleQuoted:
						continue;
					default:
						throw new XmlException (this as IXmlLineInfo,"unexpected end of file at DTD.");
					}
					break;
				case -1:
					throw new XmlException (this as IXmlLineInfo,"unexpected end of file at DTD.");
				case '<':
					if (State == DtdInputState.InsideDoubleQuoted ||
						State == DtdInputState.InsideSingleQuoted)
						continue;	// well-formed
					switch (ReadChar ()) {
					case '?':
						stateStack.Push (DtdInputState.PI);
						break;
					case '!':
						switch (ReadChar ()) {
						case 'E':
							switch (ReadChar ()) {
							case 'L':
								Expect ("EMENT");
								stateStack.Push (DtdInputState.ElementDecl);
								break;
							case 'N':
								Expect ("TITY");
								stateStack.Push (DtdInputState.EntityDecl);
								break;
							default:
								throw new XmlException (this as IXmlLineInfo,"unexpected token '<!E'.");
							}
							break;
						case 'A':
							Expect ("TTLIST");
							stateStack.Push (DtdInputState.AttlistDecl);
							break;
						case 'N':
							Expect ("OTATION");
							stateStack.Push (DtdInputState.NotationDecl);
							break;
						case '-':
							Expect ("-");
							stateStack.Push (DtdInputState.Comment);
							break;
						}
						break;
					default:
						throw new XmlException (this as IXmlLineInfo,"unexpected '>'.");
					}
					break;
				case '\'':
					if (State == DtdInputState.InsideSingleQuoted)
						stateStack.Pop ();
					else if (State != DtdInputState.InsideDoubleQuoted && State != DtdInputState.Comment)
						stateStack.Push (DtdInputState.InsideSingleQuoted);
					break;
				case '"':
					if (State == DtdInputState.InsideDoubleQuoted)
						stateStack.Pop ();
					else if (State != DtdInputState.InsideSingleQuoted && State != DtdInputState.Comment)
						stateStack.Push (DtdInputState.InsideDoubleQuoted);
					break;
				case '>':
					switch (State) {
					case DtdInputState.ElementDecl:
						goto case DtdInputState.NotationDecl;
					case DtdInputState.AttlistDecl:
						goto case DtdInputState.NotationDecl;
					case DtdInputState.EntityDecl:
						goto case DtdInputState.NotationDecl;
					case DtdInputState.NotationDecl:
						stateStack.Pop ();
						break;
					case DtdInputState.InsideDoubleQuoted:
						continue;
					case DtdInputState.InsideSingleQuoted:
						continue; // well-formed
					case DtdInputState.Comment:
						continue;
					default:
						throw new XmlException (this as IXmlLineInfo,"unexpected token '>'");
					}
					break;
				case '?':
					if (State == DtdInputState.PI) {
						if (ReadChar () == '>')
							stateStack.Pop ();
					}
					break;
				case '-':
					if (State == DtdInputState.Comment) {
						if (PeekChar () == '-') {
							ReadChar ();
							Expect ('>');
							stateStack.Pop ();
						}
					}
					break;
				case '%':
					if (State != DtdInputState.Free && State != DtdInputState.EntityDecl && State != DtdInputState.Comment && State != DtdInputState.InsideDoubleQuoted && State != DtdInputState.InsideSingleQuoted)
						throw new XmlException (this as IXmlLineInfo,"Parameter Entity Reference cannot appear as a part of markupdecl (see XML spec 2.8).");
					break;
				}
			}
		}

		// The reader is positioned on the first 'S' of "SYSTEM".
		private string ReadSystemLiteral (bool expectSYSTEM)
		{
			if(expectSYSTEM) {
				Expect ("SYSTEM");
				if (!SkipWhitespace ())
					throw new XmlException (this as IXmlLineInfo,
						"Whitespace is required after 'SYSTEM'.");
			}
			else
				SkipWhitespace ();
			int quoteChar = ReadChar ();	// apos or quot
			int startPos = currentTag.Length;
			int c = 0;
			ClearValueBuffer ();
			while (c != quoteChar) {
				c = ReadChar ();
				if (c < 0)
					throw new XmlException (this as IXmlLineInfo,"Unexpected end of stream in ExternalID.");
				if (c != quoteChar)
					AppendValueChar (c);
			}
			return CreateValueString (); //currentTag.ToString (startPos, currentTag.Length - 1 - startPos);
		}

		private string ReadPubidLiteral()
		{
			Expect ("PUBLIC");
			if (!SkipWhitespace ())
				throw new XmlException (this as IXmlLineInfo,
					"Whitespace is required after 'PUBLIC'.");
			int quoteChar = ReadChar ();
			int startPos = currentTag.Length;
			int c = 0;
			ClearValueBuffer ();
			while(c != quoteChar)
			{
				c = ReadChar ();
				if(c < 0) throw new XmlException (this as IXmlLineInfo,"Unexpected end of stream in ExternalID.");
				if(c != quoteChar && !XmlChar.IsPubidChar (c))
					throw new XmlException (this as IXmlLineInfo,"character '" + (char)c + "' not allowed for PUBLIC ID");
				if (c != quoteChar)
					AppendValueChar (c);
			}
			return CreateValueString (); //currentTag.ToString (startPos, currentTag.Length - 1 - startPos);
		}

		// The reader is positioned on the first character
		// of the name.
		private string ReadName ()
		{
			return ReadNameOrNmToken(false);
		}

		// The reader is positioned on the first character
		// of the name.
		private string ReadNmToken ()
		{
			return ReadNameOrNmToken(true);
		}

		private string ReadNameOrNmToken (bool isNameToken)
		{
			int ch = PeekChar ();
			if(isNameToken) {
				if (!XmlChar.IsNameChar ((char) ch))
					throw new XmlException (this as IXmlLineInfo,String.Format ("a nmtoken did not start with a legal character {0} ({1})", ch, (char)ch));
			}
			else {
				if (!XmlChar.IsFirstNameChar (ch))
					throw new XmlException (this as IXmlLineInfo,String.Format ("a name did not start with a legal character {0} ({1})", ch, (char)ch));
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
				throw new XmlException (this as IXmlLineInfo,
					String.Format (
						"expected '{0}' ({1:X}) but found '{2}' ({3:X})",
						(char)expected,
						expected,
						(char)ch,
						ch));
			}
		}

		private void Expect (string expected)
		{
			int len = expected.Length;
			for(int i=0; i< len; i++)
				Expect (expected[i]);
		}

		// Does not consume the first non-whitespace character.
		private bool SkipWhitespace ()
		{
			//FIXME: Should not skip if whitespaceHandling == WhiteSpaceHandling.None
			bool skipped = XmlChar.IsWhitespace (PeekChar ());
			while (XmlChar.IsWhitespace (PeekChar ()))
				ReadChar ();
			return skipped;
		}

		private void ReadWhitespace ()
		{
			if (currentState == XmlNodeType.None)
				currentState = XmlNodeType.XmlDeclaration;

			ClearValueBuffer ();
			int ch = PeekChar ();
			do {
				AppendValueChar (ReadChar ());
			} while ((ch = PeekChar ()) != -1 && XmlChar.IsWhitespace (ch));

			if (currentState == XmlNodeType.Element && ch != -1 && ch != '<')
				ReadText (false);
			else {
				XmlNodeType nodeType = (this.XmlSpace == XmlSpace.Preserve) ?
					XmlNodeType.SignificantWhitespace : XmlNodeType.Whitespace;
				SetProperties (nodeType,
					       String.Empty,
					       false,
					       true,
					       valueBuffer);
			}

			return; // (PeekChar () != -1);
		}

		private byte GetBase64Byte (char ch)
		{
			switch (ch) {
			case '+':
				return 62;
			case '/':
				return 63;
			case '=':
				return 0;
			default:
				if (ch >= 'A' && ch <= 'Z')
					return (byte) (ch - 'A');
				else if (ch >= 'a' && ch <= 'z')
					return (byte) (ch - 'a' + 26);
				else if (ch >= '0' && ch <= '9')
					return (byte) (ch - '0' + 52);
				else
					throw new XmlException ("Invalid Base64 character was found.");
			}
		}

		// Returns -1 if it should throw an error.
		private int ReadCharsInternal (char [] buffer, int offset, int length)
		{
			shouldSkipUntilEndTag = true;

			if (offset < 0)
				throw new ArgumentOutOfRangeException ("offset", offset, "Offset must be non-negative integer.");
			else if (length < 0)
				throw new ArgumentOutOfRangeException ("length", length, "Length must be non-negative integer.");
			else if (buffer.Length < offset + length)
				throw new ArgumentOutOfRangeException ("buffer length is smaller than the sum of offset and length.");

			if (NodeType != XmlNodeType.Element)
				return 0;

			int bufIndex = offset;
			for (int i = 0; i < length; i++) {
				int c = PeekChar ();
				switch (c) {
				case -1:
					throw new XmlException (this as IXmlLineInfo, "Unexpected end of xml.");
				case '<':
					ReadChar ();
					if (PeekChar () != '/') {
						buffer [bufIndex++] = '<';
						continue;
					}
					// Seems to skip immediate EndElement
					Expect ('/');
					string name = ReadName ();
					if (name != (string) this.elementStack.Peek ()) {
						if (i + 1 < length) {
							buffer [bufIndex++] = '<';
							buffer [bufIndex++] = '/';
						}
						for (int n = 0; n < name.Length && i + n + 1 < length; n++)
							buffer [bufIndex++] = name [n];
						continue;
					}
					Expect ('>');
					depth--;
					this.elementStack.Pop ();
					shouldSkipUntilEndTag = false;
					Read ();
					return i;
				default:
					buffer [bufIndex++] = (char) ReadChar ();
					break;
				}
			}
			return length;
		}

		private bool ReadUntilEndTag ()
		{
			int ch;
			do {
				ch = ReadChar ();
				switch (ch) {
				case -1:
					throw new XmlException (this as IXmlLineInfo,
						"Unexpected end of xml.");
				case '<':
					if (PeekChar () != '/')
						continue;
					ReadChar ();
					string name = ReadName ();
					if (name != (string) this.elementStack.Peek ())
						continue;
					Expect ('>');
					depth--;
					this.elementStack.Pop ();
					return Read ();
				}
			} while (true);
		}
		#endregion
	}
}
