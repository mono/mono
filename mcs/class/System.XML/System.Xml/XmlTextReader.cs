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
using System.Xml.Schema;
using Mono.Xml;

#if NET_2_0
using System.Xml;

namespace Mono.Xml2
#else
namespace System.Xml
#endif
{

#if NET_2_0
	internal class XmlTextReader : XmlReader,
		IXmlLineInfo, IXmlNamespaceResolver, IHasXmlParserContext
#else
	public class XmlTextReader : XmlReader, IXmlLineInfo, IHasXmlParserContext
#endif
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
		{
			Uri uri = resolver.ResolveUri (null, url);
			Stream s = resolver.GetEntity (uri, null, typeof (Stream)) as Stream;
			XmlParserContext ctx = new XmlParserContext (nt,
				new XmlNamespaceManager (nt),
				String.Empty,
				XmlSpace.None);
			this.InitializeContext (uri.ToString(), ctx, new XmlStreamReader (s), XmlNodeType.Document);
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

		internal bool CharacterChecking {
			get { return checkCharacters && normalization; }
			set { checkCharacters = value; }
		}

		// for XmlReaderSettings.CloseInput support
		internal bool CloseInput {
			get { return closeInput; }
			set { closeInput = value; }
		}

		public override int Depth
		{
			get {
				int nodeTypeMod = currentToken.NodeType == XmlNodeType.Element  ? 0 : -1;
				if (currentAttributeValue >= 0)
					return nodeTypeMod + elementDepth + 2; // inside attribute value.
				else if (currentAttribute >= 0)
					return nodeTypeMod + elementDepth + 1;
				return elementDepth;
			}
		}

		public Encoding Encoding
		{
			get { return parserContext.Encoding; }
		}
#if NET_2_0
		public EntityHandling EntityHandling {
			get { return entityHandling; }
			set { entityHandling = value; }
		}
#endif

		public override bool EOF {
			get { return readState == ReadState.EndOfFile; }
		}

		public override bool HasValue {
			get { return cursorToken.Value != null; }
		}

		public override bool IsDefault {
			// XmlTextReader does not expand default attributes.
			get { return false; }
		}

		public override bool IsEmptyElement {
			get { return cursorToken.IsEmptyElement; }
		}

#if NET_2_0
#else
		public override string this [int i] {
			get { return GetAttribute (i); }
		}

		public override string this [string name] {
			get { return GetAttribute (name); }
		}

		public override string this [string localName, string namespaceName] {
			get { return GetAttribute (localName, namespaceName); }
		}
#endif

		public int LineNumber {
			get {
				if (useProceedingLineInfo)
					return line;
				else
					return cursorToken.LineNumber;
			}
		}

		public int LinePosition {
			get {
				if (useProceedingLineInfo)
					return column;
				else
					return cursorToken.LinePosition;
			}
		}

		public override string LocalName {
			get { return cursorToken.LocalName; }
		}

		public override string Name {
			get { return cursorToken.Name; }
		}

		public bool Namespaces {
			get { return namespaces; }
			set { 
				if (readState != ReadState.Initial)
					throw new InvalidOperationException ("Namespaces have to be set before reading.");
				namespaces = value;
			}
		}

		public override string NamespaceURI {
			get { return cursorToken.NamespaceURI; }
		}

		public override XmlNameTable NameTable {
			get { return parserContext.NameTable; }
		}

		public override XmlNodeType NodeType {
			get { return cursorToken.NodeType; }
		}

		public bool Normalization {
			get { return normalization; }
			set { normalization = value; }
		}

		public override string Prefix {
			get { return cursorToken.Prefix; }
		}

#if NET_2_0
		public bool ProhibitDtd {
			get { return prohibitDtd; }
			set { prohibitDtd = value; }
		}
#endif

		public override char QuoteChar {
			get { return cursorToken.QuoteChar; }
		}

		public override ReadState ReadState {
			get { return readState; }
		}

#if NET_2_0
		public override XmlReaderSettings Settings {
			get { return base.Settings; }
		}
#endif

		public override string Value {
			get { return cursorToken.Value != null ? cursorToken.Value : String.Empty; }
		}

		public WhitespaceHandling WhitespaceHandling {
			get { return whitespaceHandling; }
			set { whitespaceHandling = value; }
		}

		public override string XmlLang {
			get { return parserContext.XmlLang; }
		}

		public XmlResolver XmlResolver {
			set { resolver = value; }
		}

		public override XmlSpace XmlSpace {
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
			if (closeInput && reader != null)
				reader.Close ();
		}

		public override string GetAttribute (int i)
		{
			if (i >= attributeCount)
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

		XmlParserContext IHasXmlParserContext.ParserContext {
			get { return parserContext; }
		}

		public override string GetAttribute (string localName, string namespaceURI)
		{
			int idx = this.GetIndexOfQualifiedAttribute (localName, namespaceURI);
			if (idx < 0)
				return null;
			return attributeTokens [idx].Value;
		}

#if NET_2_0
		public IDictionary GetNamespacesInScope (XmlNamespaceScope scope)
		{
			return parserContext.NamespaceManager.GetNamespacesInScope (scope);
		}
#endif

		public TextReader GetRemainder ()
		{
			if (peekCharsIndex == peekCharsLength)
				return reader;
			return new StringReader (new string (peekChars, peekCharsIndex, peekCharsLength - peekCharsIndex) + reader.ReadToEnd ());
		}

#if NET_2_0
		public bool HasLineInfo ()
#else
		bool IXmlLineInfo.HasLineInfo ()
#endif
		{
			return true;
		}

		public override string LookupNamespace (string prefix)
		{
			return LookupNamespace (prefix, false);
		}

#if NET_2_0
		public override string LookupNamespace (string prefix, bool atomizedName)
#else
		internal override string LookupNamespace (string prefix, bool atomizedName)
#endif
		{
			return parserContext.NamespaceManager.LookupNamespace (prefix, atomizedName);
		}

#if NET_2_0
		string IXmlNamespaceResolver.LookupPrefix (string ns)
		{
			return LookupPrefix (ns, false);
		}

		public string LookupPrefix (string ns, bool atomizedName)
		{
			return parserContext.NamespaceManager.LookupPrefix (ns, atomizedName);
		}
#endif

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
			if (depthUp) {
				++depth;
				depthUp = false;
			}

			if (shouldSkipUntilEndTag) {
				shouldSkipUntilEndTag = false;
				return ReadUntilEndTag ();
			}

			base64CacheStartsAt = -1;

			more = ReadContent ();

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

		private int SkipIgnorableBase64Chars (char [] chars, int charsLength, int i)
		{
			while (chars [i] == '=' || XmlChar.IsWhitespace (chars [i]))
				if (charsLength == ++i)
					break;
			return i;
		}

		public int ReadBase64 (byte [] buffer, int offset, int length)
		{
			if (offset < 0)
				throw new ArgumentOutOfRangeException ("offset", offset, "Offset must be non-negative integer.");
			else if (length < 0)
				throw new ArgumentOutOfRangeException ("length", length, "Length must be non-negative integer.");
			else if (buffer.Length < offset + length)
				throw new ArgumentOutOfRangeException ("buffer length is smaller than the sum of offset and length.");

			if (length == 0)	// It does not raise an error.
				return 0;

			int bufIndex = offset;
			int bufLast = offset + length;

			if (base64CacheStartsAt >= 0) {
				for (int i = base64CacheStartsAt; i < 3; i++) {
					buffer [bufIndex++] = base64Cache [base64CacheStartsAt++];
					if (bufIndex == bufLast)
						return bufLast - offset;
				}
			}

			for (int i = 0; i < 3; i++)
				base64Cache [i] = 0;
			base64CacheStartsAt = -1;

			int max = (int) System.Math.Ceiling (4.0 / 3 * length);
			int additional = max % 4;
			if (additional > 0)
				max += 4 - additional;
			char [] chars = new char [max];
			int charsLength = ReadChars (chars, 0, max);

			byte b = 0;
			byte work = 0;
			bool loop = true;
			for (int i = 0; i < charsLength - 3; i++) {
				if ((i = SkipIgnorableBase64Chars (chars, charsLength, i)) == charsLength)
					break;
				b = (byte) (GetBase64Byte (chars [i]) << 2);
				if (bufIndex < bufLast)
					buffer [bufIndex] = b;
				else {
					if (base64CacheStartsAt < 0)
						base64CacheStartsAt = 0;
					base64Cache [0] = b;
				}
				// charsLength mod 4 might not equals to 0.
				if (++i == charsLength)
					break;
				if ((i = SkipIgnorableBase64Chars (chars, charsLength, i))  == charsLength)
					break;
				b = GetBase64Byte (chars [i]);
				work = (byte) (b >> 4);
				if (bufIndex < bufLast) {
					buffer [bufIndex] += work;
					bufIndex++;
				}
				else
					base64Cache [0] += work;

				work = (byte) ((b & 0xf) << 4);
				if (bufIndex < bufLast) {
					buffer [bufIndex] = work;
				}
				else {
					if (base64CacheStartsAt < 0)
						base64CacheStartsAt = 1;
					base64Cache [1] = work;
				}

				if (++i == charsLength)
					break;
				if ((i = SkipIgnorableBase64Chars (chars, charsLength, i)) == charsLength)
					break;
				b = GetBase64Byte (chars [i]);
				work = (byte) (b >> 2);
				if (bufIndex < bufLast) {
					buffer [bufIndex] += work;
					bufIndex++;
				}
				else
					base64Cache [1] += work;

				work = (byte) ((b & 3) << 6);
				if (bufIndex < bufLast)
					buffer [bufIndex] = work;
				else {
					if (base64CacheStartsAt < 0)
						base64CacheStartsAt = 2;
					base64Cache [2] = work;
				}
				if (++i == charsLength)
					break;
				if ((i = SkipIgnorableBase64Chars (chars, charsLength, i)) == charsLength)
					break;
				work = GetBase64Byte (chars [i]);
				if (bufIndex < bufLast) {
					buffer [bufIndex] += work;
					bufIndex++;
				}
				else
					base64Cache [2] += work;
			}
			return System.Math.Min (bufLast - offset, bufIndex - offset);
		}

		public int ReadBinHex (byte [] buffer, int offset, int length)
		{
			if (offset < 0)
				throw new ArgumentOutOfRangeException ("offset", offset, "Offset must be non-negative integer.");
			else if (length < 0)
				throw new ArgumentOutOfRangeException ("length", length, "Length must be non-negative integer.");
			else if (buffer.Length < offset + length)
				throw new ArgumentOutOfRangeException ("buffer length is smaller than the sum of offset and length.");

			if (length == 0)
				return 0;

			char [] chars = new char [length * 2];
			int charsLength = ReadChars (chars, 0, length * 2);
			return XmlConvert.FromBinHexString (chars, offset, charsLength, buffer);
		}

		public int ReadChars (char [] buffer, int offset, int length)
		{
			return ReadCharsInternal (buffer, offset, length);
		}

#if NET_1_0
		public override string ReadInnerXml ()
		{
			return ReadInnerXmlInternal ();
		}

		public override string ReadOuterXml ()
		{
			return ReadOuterXmlInternal ();
		}

		public override string ReadString ()
		{
			return ReadStringInternal ();
		}
#endif

		public void ResetState ()
		{
			throw new InvalidOperationException ("Cannot call ResetState when parsing an XML fragment.");
			Init ();
		}

#if NET_2_0
		[MonoTODO]
		public override bool ReadValueAsBoolean ()
		{
			return base.ReadValueAsBoolean ();
		}

		[MonoTODO]
		public override DateTime ReadValueAsDateTime ()
		{
			return base.ReadValueAsDateTime ();
		}

		[MonoTODO]
		public override decimal ReadValueAsDecimal ()
		{
			return base.ReadValueAsDecimal ();
		}

		[MonoTODO]
		public override double ReadValueAsDouble ()
		{
			return base.ReadValueAsDouble ();
		}

		[MonoTODO]
		public override int ReadValueAsInt32 ()
		{
			return base.ReadValueAsInt32 ();
		}

		[MonoTODO]
		public override long ReadValueAsInt64 ()
		{
			return base.ReadValueAsInt64 ();
		}

		[MonoTODO]
		public override ICollection ReadValueAsList ()
		{
			return base.ReadValueAsList ();
		}

		[MonoTODO]
		public override float ReadValueAsSingle ()
		{
			return base.ReadValueAsSingle ();
		}

		[MonoTODO]
		public override string ReadValueAsString ()
		{
			return ReadString ();
		}

		[MonoTODO]
		public override object ReadValueAs (Type type)
		{
			return base.ReadValueAs (type);
		}

		[MonoTODO]
		public override object ReadValueAs (Type type, IXmlNamespaceResolver resolver)
		{
			return base.ReadValueAs (type, resolver);
		}
#endif

		public override void ResolveEntity ()
		{
			// XmlTextReader does not resolve entities.
			throw new InvalidOperationException ("XmlTextReader cannot resolve external entities.");
		}

#if NET_2_0
		[MonoTODO ("Implement for performance reason")]
		public override void Skip ()
		{
			base.Skip ();
		}
#endif
		#endregion

		#region Internals
		// Parsed DTD Objects
		internal DTDObjectModel DTD {
			get { return parserContext.Dtd; }
		}

		internal XmlResolver Resolver {
			get { return resolver; }
		}
		#endregion

		#region Privates
		internal class XmlTokenInfo
		{
			public XmlTokenInfo (XmlTextReader xtr, bool isPrimaryToken)
			{
				this.isPrimaryToken = isPrimaryToken;
				Reader = xtr;
				Clear ();
			}

			bool isPrimaryToken;
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
			public int ValueBufferStart;
			public int ValueBufferEnd;

			public XmlNodeType NodeType;

			public virtual string Value {
				get {
					if (valueCache != null)
						return valueCache;
					if (ValueBufferStart >= 0) {
//Console.WriteLine (NodeType + " / " + ValueBuffer.Length + " / " + ValueBufferStart + " / " + ValueBufferEnd);
						valueCache = new string (Reader.valueBuffer, ValueBufferStart, ValueBufferEnd - ValueBufferStart);
						return valueCache;
					}
					switch (NodeType) {
					case XmlNodeType.Text:
					case XmlNodeType.SignificantWhitespace:
					case XmlNodeType.Whitespace:
					case XmlNodeType.Comment:
					case XmlNodeType.CDATA:
					case XmlNodeType.ProcessingInstruction:
						valueCache = Reader.CreateValueString ();
						return valueCache;
					}
					return null;
				}
				set { valueCache = value; }
			}

			public virtual void Clear ()
			{
				ValueBufferStart = -1;
				valueCache = null;
				NodeType = XmlNodeType.None;
				Name = LocalName = Prefix = NamespaceURI = String.Empty;
				IsEmptyElement = false;
				QuoteChar = '"';
				LineNumber = LinePosition = 0;
			}
		}

		internal class XmlAttributeTokenInfo : XmlTokenInfo
		{
			public XmlAttributeTokenInfo (XmlTextReader reader)
				: base (reader, false)
			{
				NodeType = XmlNodeType.Attribute;
			}

			public int ValueTokenStartIndex;
			public int ValueTokenEndIndex;
			string valueCache;
			bool cachedNormalization;
			StringBuilder tmpBuilder = new StringBuilder ();

			public override string Value {
				get {
					if (cachedNormalization != Reader.Normalization)
						valueCache = null;
					if (valueCache != null)
						return valueCache;

					cachedNormalization = Reader.Normalization;

					// An empty value should return String.Empty.
					if (ValueTokenStartIndex == ValueTokenEndIndex) {
						XmlTokenInfo ti = Reader.attributeValueTokens [ValueTokenStartIndex];
						if (ti.NodeType == XmlNodeType.EntityReference)
							valueCache = String.Concat ("&", ti.Name, ";");
						else
							valueCache = ti.Value;
						if (cachedNormalization)
							NormalizeSpaces ();
						return valueCache;
					}

					tmpBuilder.Length = 0;
					for (int i = ValueTokenStartIndex; i <= ValueTokenEndIndex; i++) {
						XmlTokenInfo ti = Reader.attributeValueTokens [i];
						if (ti.NodeType == XmlNodeType.Text)
							tmpBuilder.Append (ti.Value);
						else {
							tmpBuilder.Append ('&');
							tmpBuilder.Append (ti.Name);
							tmpBuilder.Append (';');
						}
					}

					valueCache = tmpBuilder.ToString ();
					if (cachedNormalization)
						NormalizeSpaces ();
					return valueCache;
				}

				set { valueCache = value; }
			}

			public override void Clear ()
			{
				base.Clear ();
				valueCache = null;
				NodeType = XmlNodeType.Attribute;
				ValueTokenStartIndex = ValueTokenEndIndex = 0;
			}

			internal void FillXmlns ()
			{
				if (Object.ReferenceEquals (Prefix, XmlNamespaceManager.PrefixXmlns))
					Reader.parserContext.NamespaceManager.AddNamespace (LocalName, Value);
				else if (Object.ReferenceEquals (Name, XmlNamespaceManager.PrefixXmlns))
					Reader.parserContext.NamespaceManager.AddNamespace (String.Empty, Value);
			}

			internal void FillNamespace ()
			{
				if (Object.ReferenceEquals (Prefix, XmlNamespaceManager.PrefixXmlns) ||
					Object.ReferenceEquals (Name, XmlNamespaceManager.PrefixXmlns))
					NamespaceURI = XmlNamespaceManager.XmlnsXmlns;
				else if (Prefix.Length == 0)
					NamespaceURI = string.Empty;
				else
					NamespaceURI = Reader.LookupNamespace (Prefix, true);
			}

			private void NormalizeSpaces ()
			{
				tmpBuilder.Length = 0;
				for (int i = 0; i < valueCache.Length; i++)
					switch (valueCache [i]) {
					case '\r':
						if (i + 1 < valueCache.Length && valueCache [i + 1] == '\n')
							i++;
						goto case '\n';
					case '\t':
					case '\n':
						tmpBuilder.Append (' ');
						break;
					default:
						tmpBuilder.Append (valueCache [i]);
						break;
					}
				valueCache = tmpBuilder.ToString ();
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

		private string [] elementNames;
		int elementNameStackPos;

		private bool allowMultipleRoot;

		private bool isStandalone;

		private bool returnEntityReference;
		private string entityReferenceName;

		private char [] nameBuffer;
		private int nameLength;
		private int nameCapacity;
		private const int initialNameCapacity = 32;

		private char [] valueBuffer;
		private int valueLength;
		private int valueCapacity;
		private const int initialValueCapacity = 256;

		private char [] currentTagBuffer;
		private int currentTagLength;
		private int currentTagCapacity;
		private const int initialCurrentTagCapacity = 256;

		private TextReader reader;
		private char [] peekChars;
		private int peekCharsIndex;
		private int peekCharsLength;
		private const int peekCharCapacity = 1024;

		private int line;
		private int column;

		private int currentLinkedNodeLineNumber;
		private int currentLinkedNodeLinePosition;
		private bool useProceedingLineInfo;

		private XmlNodeType startNodeType;
		// State machine attribute.
		//	XmlDeclaration: after the first node.
		//	DocumentType: after doctypedecl
		//	Element: inside document element
		//	EndElement: after document element
		private XmlNodeType currentState;

		// For ReadChars()/ReadBase64()/ReadBinHex()
		private bool shouldSkipUntilEndTag;
		private byte [] base64Cache = new byte [3];
		private int base64CacheStartsAt;

		// These values are never re-initialized.
		private bool namespaces = true;
		private WhitespaceHandling whitespaceHandling = WhitespaceHandling.All;
		private XmlResolver resolver = new XmlUrlResolver ();
		private bool normalization = false;

		private bool checkCharacters;
		private bool prohibitDtd = false;
		private bool closeInput = true;
		private EntityHandling entityHandling; // 2.0

		private void Init ()
		{
			currentToken = new XmlTokenInfo (this, true);
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
			elementNames = new string [10];
			elementNameStackPos = 0;

			isStandalone = false;
			returnEntityReference = false;
			entityReferenceName = String.Empty;

			nameBuffer = new char [initialNameCapacity];
			nameLength = 0;
			nameCapacity = initialNameCapacity;

			valueBuffer = new char [initialValueCapacity];
			valueLength = 0;
			valueCapacity = initialValueCapacity;

			currentTagBuffer = new char [initialCurrentTagCapacity];
			currentTagLength = 0;
			currentTagCapacity = initialCurrentTagCapacity;

			peekCharsIndex = 0;
			peekCharsLength = 0;
			if (peekChars == null)
				peekChars = new char [peekCharCapacity];

			line = 1;
			column = 1;

			currentLinkedNodeLineNumber = currentLinkedNodeLinePosition = 0;
			useProceedingLineInfo = false;

			currentState = XmlNodeType.None;

			shouldSkipUntilEndTag = false;
			base64CacheStartsAt = -1;

			checkCharacters = true;
#if NET_2_0
			if (Settings != null)
				checkCharacters = Settings.CheckCharacters;
#endif
			prohibitDtd = false;
			closeInput = true;
			entityHandling = EntityHandling.ExpandCharEntities;
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

			if (url != null && url.Length > 0) {
				Uri uri = null;
				try {
					uri = new Uri (url);
				} catch (Exception) {
					string path = Path.GetFullPath ("./a");
					uri = new Uri (new Uri (path), url);
				}
				parserContext.BaseURI = uri.ToString ();
			}

			Init ();

			reader = fragment;

			switch (fragType) {
			case XmlNodeType.Attribute:
				reader = new StringReader (fragment.ReadToEnd ().Replace ("\"", "&quot;"));
				SkipTextDeclaration ();
				break;
			case XmlNodeType.Element:
				currentState = XmlNodeType.Element;
				allowMultipleRoot = true;
				SkipTextDeclaration ();
				break;
			case XmlNodeType.Document:
				break;
			default:
				throw new XmlException (String.Format ("NodeType {0} is not allowed to create XmlTextReader.", fragType));
			}
		}

#if NET_2_0
		[MonoTODO ("Test")]
		internal ConformanceLevel Conformance {
			set {
				if (value == ConformanceLevel.Fragment) {
					currentState = XmlNodeType.Element;
					allowMultipleRoot = true;
				}
			}
		}

		internal void AdjustLineInfoOffset (int lineNumberOffset, int linePositionOffset)
		{
			line += lineNumberOffset;
			column += linePositionOffset;
		}

		internal void SetNameTable (XmlNameTable nameTable)
		{
			parserContext.NameTable = nameTable;
		}
#endif

		// Use this method rather than setting the properties
		// directly so that all the necessary properties can
		// be changed in harmony with each other. Maybe the
		// fields should be in a seperate class to help enforce
		// this.
		//
		// Namespace URI could not be provided here.
		private void SetProperties (
			XmlNodeType nodeType,
			string name,
			string prefix,
			string localName,
			bool isEmptyElement,
			string value,
			bool clearAttributes)
		{
			SetTokenProperties (currentToken, nodeType, name, prefix, localName, isEmptyElement, value, clearAttributes);
			currentToken.LineNumber = this.currentLinkedNodeLineNumber;
			currentToken.LinePosition = this.currentLinkedNodeLinePosition;
		}

		private void SetTokenProperties (
			XmlTokenInfo token,
			XmlNodeType nodeType,
			string name,
			string prefix,
			string localName,
			bool isEmptyElement,
			string value,
			bool clearAttributes)
		{
			token.NodeType = nodeType;
			token.Name = name;
			token.Prefix = prefix;
			token.LocalName = localName;
			token.IsEmptyElement = isEmptyElement;
			token.Value = value;
			this.elementDepth = depth;

			if (clearAttributes)
				ClearAttributes ();
		}

		private void ClearAttributes ()
		{
			for (int i = 0; i < attributeCount; i++)
				attributeTokens [i].Clear ();
			attributeCount = 0;
			currentAttribute = -1;
			currentAttributeValue = -1;
		}

		private int PeekChar ()
		{
			if (peekCharsLength == peekCharsIndex) {
				if (!ReadTextReader ())
					return -1;
				else
					return PeekChar ();
			}
			else
				return peekChars [peekCharsIndex] != 0 ?
					peekChars [peekCharsIndex] : -1;
		}

		private int ReadChar ()
		{
			int ch;

			if (peekCharsLength == peekCharsIndex) {
				if (!ReadTextReader ())
					return -1;
			}
			ch = peekChars [peekCharsIndex++];

			if (ch == '\n') {
				line++;
				column = 1;
			} else if (ch == 0) {
				return -1;
			} else {
				column++;
			}
			if (currentState != XmlNodeType.Element)
				AppendCurrentTagChar (ch);
			return ch;
		}

		private bool ReadTextReader ()
		{
			peekCharsIndex = 0;
			peekCharsLength = reader.Read (peekChars, 0, peekCharCapacity);
			if (peekCharsLength == 0)
				return false;
			// set EOF
			if (peekCharsLength < peekCharCapacity)
				peekChars [peekCharsLength] = (char) 0;
			return true;
		}

		private string ExpandSurrogateChar (int ch)
		{
			if (ch < Char.MaxValue)
				return ((char) ch).ToString ();
			else {
				char [] tmp = new char [] {(char) (ch / 0x10000 + 0xD800 - 1), (char) (ch % 0x10000 + 0xDC00)};
				return new string (tmp);
			}
		}

		private bool ReadContent ()
		{
			currentTagLength = 0;
			if (popScope) {
				parserContext.NamespaceManager.PopScope ();
				popScope = false;
			}

			if (returnEntityReference)
				SetEntityReferenceProperties ();
			else {
				int c = PeekChar ();
				if (c == -1) {
					readState = ReadState.EndOfFile;
					ClearValueBuffer ();
					SetProperties (
						XmlNodeType.None, // nodeType
						String.Empty, // name
						String.Empty, // prefix
						String.Empty, // localName
						false, // isEmptyElement
						null, // value
						true // clearAttributes
					);
					if (depth > 0)
						throw new XmlException ("unexpected end of file. Current depth is " + depth);

					return false;
				} else {
 	   				switch (c) {
					case '<':
						ReadChar ();
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
					default:
						ReadText (true);
						break;
					}
				}
			}
			return this.ReadState != ReadState.EndOfFile;
		}

		private void SetEntityReferenceProperties ()
		{
			DTDEntityDeclaration decl = DTD != null ? DTD.EntityDecls [entityReferenceName] : null;
			if (this.isStandalone)
				if (DTD == null || decl == null || !decl.IsInternalSubset)
					throw new XmlException (this as IXmlLineInfo,
						"Standalone document must not contain any references to an non-internally declared entity.");
			if (decl != null && decl.NotationName != null)
				throw new XmlException (this as IXmlLineInfo,
					"Reference to any unparsed entities is not allowed here.");

			ClearValueBuffer ();
			SetProperties (
				XmlNodeType.EntityReference, // nodeType
				entityReferenceName, // name
				String.Empty, // prefix
				entityReferenceName, // localName
				false, // isEmptyElement
				null, // value
				true // clearAttributes
			);

			returnEntityReference = false;
			entityReferenceName = String.Empty;
		}

		// The leading '<' has already been consumed.
		private void ReadStartTag ()
		{
			if (currentState == XmlNodeType.EndElement)
				throw new XmlException (this as IXmlLineInfo,
					"Multiple document element was detected.");
			currentState = XmlNodeType.Element;

			parserContext.NamespaceManager.PushScope ();

			currentLinkedNodeLineNumber = line;
			currentLinkedNodeLinePosition = column;

			string prefix, localName;
			string name = ReadName (out prefix, out localName);
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
				attributeTokens [i].FillXmlns ();
			for (int i = 0; i < attributeCount; i++)
				attributeTokens [i].FillNamespace ();

			// quick name check
			for (int i = 0; i < attributeCount; i++) {
				for (int j = i + 1; j < attributeCount; j++)
					if (Object.ReferenceEquals (attributeTokens [i].Name, attributeTokens [j].Name) ||
						(Object.ReferenceEquals (attributeTokens [i].LocalName, attributeTokens [j].LocalName) &&
						Object.ReferenceEquals (attributeTokens [i].NamespaceURI, attributeTokens [j].NamespaceURI)))
						throw new XmlException (this as IXmlLineInfo,
							"Attribute name and qualified name must be identical.");
			}

			if (PeekChar () == '/') {
				ReadChar ();
				isEmptyElement = true;
				popScope = true;
			}
			else {
				depthUp = true;
				PushElementName (name);
				parserContext.PushScope ();
			}

			Expect ('>');

			SetProperties (
				XmlNodeType.Element, // nodeType
				name, // name
				prefix, // prefix
				localName, // name
				isEmptyElement, // isEmptyElement
				null, // value
				false // clearAttributes
			);
			if (prefix.Length > 0)
				currentToken.NamespaceURI = LookupNamespace (prefix, true);
			else if (namespaces)
				currentToken.NamespaceURI = parserContext.NamespaceManager.DefaultNamespace;

			if (namespaces) {
				if (NamespaceURI == null)
					throw new XmlException (String.Format ("'{0}' is undeclared namespace.", Prefix));
				try {
					for (int i = 0; i < attributeCount; i++) {
						MoveToAttribute (i);
						if (NamespaceURI == null)
							throw new XmlException (String.Format ("'{0}' is undeclared namespace.", Prefix));
					}
				} finally {
					MoveToElement ();
				}
			}

			for (int i = 0; i < attributeCount; i++) {
				if (Object.ReferenceEquals (attributeTokens [i].Prefix, XmlNamespaceManager.PrefixXml)) {
					string aname = attributeTokens [i].LocalName;
					string value = attributeTokens [i].Value;
					switch (aname) {
					case "base":
						if (this.resolver != null)
							parserContext.BaseURI = resolver.ResolveUri (new Uri (BaseURI), value).ToString ();
						else
							parserContext.BaseURI = value;
						break;
					case "lang":
						parserContext.XmlLang = value;
						break;
					case "space":
						switch (value) {
						case "preserve":
							parserContext.XmlSpace = XmlSpace.Preserve;
							break;
						case "default":
							parserContext.XmlSpace = XmlSpace.Default;
							break;
						default:
							throw new XmlException (this as IXmlLineInfo, String.Format ("Invalid xml:space value: {0}", value));
						}
						break;
					}
				}
			}

			if (IsEmptyElement)
				CheckCurrentStateUpdate ();
		}

		private void PushElementName (string name)
		{
			if (elementNames.Length == elementNameStackPos) {
				string [] newArray = new string [elementNames.Length * 2];
				Array.Copy (elementNames, 0, newArray, 0, elementNameStackPos);
				elementNames = newArray;
			}
			elementNames [elementNameStackPos++] = name;
		}

		// The reader is positioned on the first character
		// of the element's name.
		private void ReadEndTag ()
		{
			if (currentState != XmlNodeType.Element)
				throw new XmlException (this as IXmlLineInfo,
					"End tag cannot appear in this state.");

			currentLinkedNodeLineNumber = line;
			currentLinkedNodeLinePosition = column;

			string prefix, localName;
			string name = ReadName (out prefix, out localName);
			if (elementNameStackPos == 0)
				throw new XmlException (this as IXmlLineInfo,"closing element without matching opening element");
			string expected = elementNames [--elementNameStackPos];
			if (expected != name)
				throw new XmlException (this as IXmlLineInfo,String.Format ("unmatched closing element: expected {0} but found {1}", expected, name));
			parserContext.PopScope ();

			ExpectAfterWhitespace ('>');

			--depth;

			SetProperties (
				XmlNodeType.EndElement, // nodeType
				name, // name
				prefix, // prefix
				localName, // localName
				false, // isEmptyElement
				null, // value
				true // clearAttributes
			);
			if (prefix.Length > 0)
				currentToken.NamespaceURI = LookupNamespace (prefix, true);
			else if (namespaces)
				currentToken.NamespaceURI = parserContext.NamespaceManager.DefaultNamespace;

			popScope = true;

			CheckCurrentStateUpdate ();
		}

		private void CheckCurrentStateUpdate ()
		{
			if (depth == 0 && !allowMultipleRoot && (IsEmptyElement || NodeType == XmlNodeType.EndElement))
				currentState = XmlNodeType.EndElement;
		}

		private void AppendSurrogatePairNameChar (int ch)
		{
			nameBuffer [nameLength++] = (char) (ch / 0x10000 + 0xD800 - 1);
			if (nameLength == nameCapacity)
				ExpandNameCapacity ();
			nameBuffer [nameLength++] = (char) (ch % 0x10000 + 0xDC00);
		}

		private void ExpandNameCapacity ()
		{
			nameCapacity = nameCapacity * 2;
			char [] oldNameBuffer = nameBuffer;
			nameBuffer = new char [nameCapacity];
			Array.Copy (oldNameBuffer, nameBuffer, nameLength);
		}

		private void AppendValueChar (int ch)
		{
			if (valueLength == valueCapacity)
				ExpandValueCapacity ();
			if (ch < Char.MaxValue)
				valueBuffer [valueLength++] = (char) ch;
			else
				AppendSurrogatePairValueChar (ch);
		}

		private void AppendSurrogatePairValueChar (int ch)
		{
			valueBuffer [valueLength++] = (char) (ch / 0x10000 + 0xD800 - 1);
			if (valueLength == valueCapacity)
				ExpandValueCapacity ();
			valueBuffer [valueLength++] = (char) (ch % 0x10000 + 0xDC00);
		}

		private void ExpandValueCapacity ()
		{
			valueCapacity = valueCapacity * 2;
			char [] oldValueBuffer = valueBuffer;
			valueBuffer = new char [valueCapacity];
			Array.Copy (oldValueBuffer, valueBuffer, valueLength);
		}

		private string CreateValueString ()
		{
			return new string (valueBuffer, 0, valueLength);
		}

		private void ClearValueBuffer ()
		{
			valueLength = 0;
		}

		private void AppendCurrentTagChar (int ch)
		{
			if (currentTagLength == currentTagCapacity)
				ExpandCurrentTagCapacity ();
			if (ch < Char.MaxValue)
				currentTagBuffer [currentTagLength++] = (char) ch;
			else {
				currentTagBuffer [currentTagLength++] = (char) (ch / 0x10000 + 0xD800 - 1);
				if (currentTagLength == currentTagCapacity)
					ExpandCurrentTagCapacity ();
				currentTagBuffer [currentTagLength++] = (char) (ch % 0x10000 + 0xDC00);
			}
		}

		private void ExpandCurrentTagCapacity ()
		{
			currentTagCapacity = currentTagCapacity * 2;
			char [] oldCurrentTagBuffer = currentTagBuffer;
			currentTagBuffer = new char [currentTagCapacity];
			Array.Copy (oldCurrentTagBuffer, currentTagBuffer, currentTagLength);
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
			bool previousWasCloseBracket = false;

			while (ch != '<' && ch != -1) {
				if (ch == '&') {
					ReadChar ();
					ch = ReadReference (false);
					if (returnEntityReference) // Returns -1 if char validation should not be done
						break;
				} else if (normalization && ch == '\r') {
					ReadChar ();
					ch = ReadChar ();
					if (ch != '\n')
						// append '\n' instead of '\r'.
						AppendValueChar ('\n');
					// and in case of "\r\n", discard '\r'.
				} else {
					if (CharacterChecking && XmlChar.IsInvalid (ch))
						throw new XmlException (this, "Not allowed character was found.");
					ch = ReadChar ();
				}

				// FIXME: it might be optimized by the JIT later,
//				AppendValueChar (ch);
				{
					if (valueLength == valueCapacity)
						ExpandValueCapacity ();
					if (ch < Char.MaxValue)
						valueBuffer [valueLength++] = (char) ch;
					else
						AppendSurrogatePairValueChar (ch);
				}

				// Block "]]>"
				if (ch == ']') {
					if (previousWasCloseBracket)
						if (PeekChar () == '>')
							throw new XmlException (this as IXmlLineInfo,
								"Inside text content, character sequence ']]>' is not allowed.");
					previousWasCloseBracket = true;
				}
				else if (previousWasCloseBracket)
					previousWasCloseBracket = false;
				ch = PeekChar ();
				notWhitespace = true;
			}

			if (returnEntityReference && valueLength == 0) {
				SetEntityReferenceProperties ();
			} else {
				XmlNodeType nodeType = notWhitespace ? XmlNodeType.Text :
					this.XmlSpace == XmlSpace.Preserve ? XmlNodeType.SignificantWhitespace : XmlNodeType.Whitespace;
				SetProperties (
					nodeType, // nodeType
					String.Empty, // name
					String.Empty, // prefix
					String.Empty, // localName
					false, // isEmptyElement
					null, // value: create only when required
					true // clearAttributes
				);
			}
		}

		// The leading '&' has already been consumed.
		// Returns true if the entity reference isn't a simple
		// character reference or one of the predefined entities.
		// This allows the ReadText method to break so that the
		// next call to Read will return the EntityReference node.
		private int ReadReference (bool ignoreEntityReferences)
		{
			if (PeekChar () == '#') {
				ReadChar ();
				return ReadCharacterReference ();
			} else
				return ReadEntityReference (ignoreEntityReferences);
		}

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
						throw new XmlException (this as IXmlLineInfo,
							String.Format (CultureInfo.InvariantCulture, 
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
						throw new XmlException (this as IXmlLineInfo,
							String.Format (CultureInfo.InvariantCulture, 
								"invalid decimal digit: {0} (#x{1:X})",
								(char) ch,
								ch));
				}
			}

			ReadChar (); // ';'

			// There is no way to save surrogate pairs...
			if (CharacterChecking && XmlChar.IsInvalid (value))
				throw new XmlException (this as IXmlLineInfo,
					"Referenced character was not allowed in XML. Normalization is " + normalization + ", checkCharacters = " + checkCharacters);
			return value;
		}

		// Returns -1 if it should not be validated.
		// Real EOF must not be detected here.
		private int ReadEntityReference (bool ignoreEntityReferences)
		{
			string name = ReadName ();
			Expect (';');

			int predefined = XmlChar.GetPredefinedEntity (name);
			if (predefined >= 0)
				return predefined;
			else {
				if (ignoreEntityReferences) {
					AppendValueChar ('&');
					for (int i = 0; i < name.Length; i++)
						AppendValueChar (name [i]);
					AppendValueChar (';');
				} else {
					returnEntityReference = true;
					entityReferenceName = name;
				}
			}
			return -1;
		}

		// The reader is positioned on the first character of
		// the attribute name.
		private void ReadAttributes (bool isXmlDecl)
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

				string prefix, localName;
				currentAttributeToken.Name = ReadName (out prefix, out localName);
				currentAttributeToken.Prefix = prefix;
				currentAttributeToken.LocalName = localName;
				ExpectAfterWhitespace ('=');
				SkipWhitespace ();
				ReadAttributeValueTokens (-1);
				// This hack is required for xmldecl which has
				// both effective attributes and Value.
				string dummyValue;
				if (isXmlDecl)
					dummyValue = currentAttributeToken.Value;

				attributeCount++;

				if (!SkipWhitespace ())
					requireWhitespace = true;
				peekChar = PeekChar ();
				if (isXmlDecl) {
					if (peekChar == '?')
						break;
				}
				else if (peekChar == '/' || peekChar == '>')
					break;
			} while (peekChar != -1);

			currentAttribute = -1;
			currentAttributeValue = -1;
		}

		private void AddDtdAttribute (string name, string value)
		{
			IncrementAttributeToken ();
			XmlAttributeTokenInfo ati = attributeTokens [currentAttribute];
			ati.Name = parserContext.NameTable.Add (name);
			ati.Prefix = String.Empty;
			ati.NamespaceURI = String.Empty;
			IncrementAttributeValueToken ();
			XmlTokenInfo vti = attributeValueTokens [currentAttributeValue];
			vti.Value = value;
			SetTokenProperties (vti,
				XmlNodeType.Text,
				String.Empty,
				String.Empty,
				String.Empty,
				false,
				value,
				false);
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
			currentAttributeValue++;
			if (attributeValueTokens.Length == currentAttributeValue) {
				XmlTokenInfo [] newArray = new XmlTokenInfo [attributeValueTokens.Length * 2];
				attributeValueTokens.CopyTo (newArray, 0);
				attributeValueTokens = newArray;
			}
			if (attributeValueTokens [currentAttributeValue] == null)
				attributeValueTokens [currentAttributeValue] = new XmlTokenInfo (this, false);
			currentAttributeValueToken = attributeValueTokens [currentAttributeValue];
			currentAttributeValueToken.Clear ();
		}

		// LAMESPEC: Orthodox XML reader should normalize attribute values
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
			int ch = 0;
			currentAttributeValueToken.ValueBufferStart = valueLength;
			while (loop) {
				ch = ReadChar ();
				if (ch == quoteChar)
					break;

				if (incrementToken) {
					IncrementAttributeValueToken ();
					currentAttributeValueToken.ValueBufferStart = valueLength;
					currentAttributeValueToken.LineNumber = line;
					currentAttributeValueToken.LinePosition = column;
					incrementToken = false;
					isNewToken = true;
				}

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
					int startPosition = currentTagLength - 1;
					if (PeekChar () == '#') {
						ReadChar ();
						ch = ReadCharacterReference ();
						if (CharacterChecking && XmlChar.IsInvalid (ch))
							throw new XmlException (this as IXmlLineInfo,
								"Not allowed character was found.");
						AppendValueChar (ch);
						break;
					}
					// Check XML 1.0 section 3.1 WFC.
					string entName = ReadName ();
					Expect (';');
					int predefined = XmlChar.GetPredefinedEntity (entName);
					if (predefined < 0) {
						CheckAttributeEntityReferenceWFC (entName);
#if NET_2_0
						if (entityHandling == EntityHandling.ExpandEntities) {
							string value = DTD.GenerateEntityAttributeText (entName);
							foreach (char c in value)
								AppendValueChar (c);
						} else
#endif
						{
							currentAttributeValueToken.ValueBufferEnd = valueLength;
							currentAttributeValueToken.NodeType = XmlNodeType.Text;
							if (!isNewToken)
								IncrementAttributeValueToken ();
							currentAttributeValueToken.Name = entName;
							currentAttributeValueToken.Value = String.Empty;
							currentAttributeValueToken.NodeType = XmlNodeType.EntityReference;
							incrementToken = true;
						}
					}
					else
						AppendValueChar (predefined);
					break;
				default:
					if (CharacterChecking && XmlChar.IsInvalid (ch))
						throw new XmlException (this, "Invalid character was found.");
					// FIXME: it might be optimized by the JIT later,
//					AppendValueChar (ch);
					{
						if (valueLength == valueCapacity)
							ExpandValueCapacity ();
						if (ch < Char.MaxValue)
							valueBuffer [valueLength++] = (char) ch;
						else
							AppendSurrogatePairValueChar (ch);
					}
					break;
				}

				isNewToken = false;
			}
			if (!incrementToken) {
				currentAttributeValueToken.ValueBufferEnd = valueLength;
				currentAttributeValueToken.NodeType = XmlNodeType.Text;
			}
			currentAttributeToken.ValueTokenEndIndex = currentAttributeValue;

		}

		private void CheckAttributeEntityReferenceWFC (string entName)
		{
			DTDEntityDeclaration entDecl = 
				DTD == null ? null : DTD.EntityDecls [entName];
			if (entDecl == null) {
				if (entityHandling == EntityHandling.ExpandEntities
					|| (DTD != null && resolver != null && entDecl == null))
					throw new XmlException (this,
						String.Format ("Referenced entity '{0}' does not exist.", entName));
				else
					return;
			}

			if (entDecl.HasExternalReference)
				throw new XmlException (this, "Reference to external entities is not allowed in the value of an attribute.");
			if (isStandalone && !entDecl.IsInternalSubset)
				throw new XmlException (this, "Reference to external entities is not allowed in the internal subset.");
			if (entDecl.EntityValue.IndexOf ('<') >= 0)
				throw new XmlException (this, "Attribute must not contain character '<' either directly or indirectly by way of entity references.");
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
			} else if (target.ToLower (CultureInfo.InvariantCulture) == "xml")
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

				if (CharacterChecking && XmlChar.IsInvalid (ch))
					throw new XmlException (this, "Invalid character was found.");
				AppendValueChar (ch);
			}

			SetProperties (
				XmlNodeType.ProcessingInstruction, // nodeType
				target, // name
				String.Empty, // prefix
				target, // localName
				false, // isEmptyElement
				null, // value: create only when required
				true // clearAttributes
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
				message = String.Format ("Only 'yes' or 'no' is allowed for standalone. Value was '{0}'", sa);

			this.isStandalone = (sa == "yes");

			if (message != null)
				throw new XmlException (this as IXmlLineInfo, message);

			SetProperties (
				XmlNodeType.XmlDeclaration, // nodeType
				"xml", // name
				String.Empty, // prefix
				"xml", // localName
				false, // isEmptyElement
				new string (currentTagBuffer, 6, currentTagLength - 6), // value
				false // clearAttributes
			);

			Expect ("?>");
		}

		private void SkipTextDeclaration ()
		{
			this.currentState = XmlNodeType.Element;

			if (PeekChar () != '<')
				return;

			ReadChar ();

			if (PeekChar () != '?') {
				peekCharsIndex = 0;
				return;
			}
			ReadChar ();

			while (peekCharsIndex < 6) {
				if (PeekChar () < 0)
					break;
				else
					ReadChar ();
			}
			if (new string (peekChars, 2, 4) != "xml ") {
				if (new string (peekChars, 2, 3).ToLower (CultureInfo.InvariantCulture) == "xml") {
					throw new XmlException (this as IXmlLineInfo,
						"Processing instruction name must not be character sequence 'X' 'M' 'L' with case insensitivity.");
				}
				peekCharsIndex = 0;
				return;
			}

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
				ExpectAfterWhitespace ('=');
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

				if (XmlChar.IsInvalid (ch))
					throw new XmlException (this as IXmlLineInfo,
						"Not allowed character was found.");

				AppendValueChar (ch);
			}

			SetProperties (
				XmlNodeType.Comment, // nodeType
				String.Empty, // name
				String.Empty, // prefix
				String.Empty, // localName
				false, // isEmptyElement
				null, // value: create only when required
				true // clearAttributes
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
					}
				}
				if (normalization && ch == '\r') {
					ch = PeekChar ();
					if (ch != '\n')
						// append '\n' instead of '\r'.
						AppendValueChar ('\n');
					// otherwise, discard '\r'.
					continue;
				}
				if (CharacterChecking && XmlChar.IsInvalid (ch))
					throw new XmlException (this, "Invalid character was found.");

				// FIXME: it might be optimized by the JIT later,
//				AppendValueChar (ch);
				{
					if (valueLength == valueCapacity)
						ExpandValueCapacity ();
					if (ch < Char.MaxValue)
						valueBuffer [valueLength++] = (char) ch;
					else
						AppendSurrogatePairValueChar (ch);
				}
			}

			SetProperties (
				XmlNodeType.CDATA, // nodeType
				String.Empty, // name
				String.Empty, // prefix
				String.Empty, // localName
				false, // isEmptyElement
				null, // value: create only when required
				true // clearAttributes
			);
		}

		// The reader is positioned on the first character after
		// the leading '<!DOCTYPE'.
		private void ReadDoctypeDecl ()
		{
			if (prohibitDtd)
				throw new XmlException (this as IXmlLineInfo,
					"Document Type Declaration (DTD) is prohibited in this XML.");
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
				int startPos = currentTagLength;
				ReadInternalSubset ();
				int endPos = currentTagLength - 1;
				parserContext.InternalSubset = new string (currentTagBuffer, startPos, endPos - startPos);
			}
			// end of DOCTYPE decl.
			ExpectAfterWhitespace ('>');

			GenerateDTDObjectModel (doctypeName, publicId,
				systemId, parserContext.InternalSubset,
				intSubsetStartLine, intSubsetStartColumn);

			// set properties for <!DOCTYPE> node
			SetProperties (
				XmlNodeType.DocumentType, // nodeType
				doctypeName, // name
				String.Empty, // prefix
				doctypeName, // localName
				false, // isEmptyElement
				parserContext.InternalSubset, // value
				true // clearAttributes
				);

			if (publicId != null)
				AddDtdAttribute ("PUBLIC", publicId);
			if (systemId != null)
				AddDtdAttribute ("SYSTEM", systemId);
			currentAttribute = currentAttributeValue = -1;
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

			DTDReader dr = new DTDReader (DTD, intSubsetStartLine, intSubsetStartColumn);
			dr.Normalization = this.normalization;
			return dr.GenerateDTDObjectModel ();
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
					switch (State) {
					case DtdInputState.InsideDoubleQuoted:
					case DtdInputState.InsideSingleQuoted:
					case DtdInputState.Comment:
						continue;	// well-formed
					}
					int c = ReadChar ();
					switch (c) {
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
							Expect ('-');
							stateStack.Push (DtdInputState.Comment);
							break;
						}
						break;
					default:
						throw new XmlException (this as IXmlLineInfo, String.Format ("unexpected '<{0}'.", (char) c));
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
					case DtdInputState.InsideSingleQuoted:
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
			int startPos = currentTagLength;
			int c = 0;
			ClearValueBuffer ();
			while (c != quoteChar) {
				c = ReadChar ();
				if (c < 0)
					throw new XmlException (this as IXmlLineInfo,"Unexpected end of stream in ExternalID.");
				if (c != quoteChar)
					AppendValueChar (c);
			}
			return CreateValueString ();
		}

		private string ReadPubidLiteral()
		{
			Expect ("PUBLIC");
			if (!SkipWhitespace ())
				throw new XmlException (this as IXmlLineInfo,
					"Whitespace is required after 'PUBLIC'.");
			int quoteChar = ReadChar ();
			int startPos = currentTagLength;
			int c = 0;
			ClearValueBuffer ();
			while(c != quoteChar)
			{
				c = ReadChar ();
				if(c < 0) throw new XmlException (this as IXmlLineInfo,"Unexpected end of stream in ExternalID.");
				if(c != quoteChar && !XmlChar.IsPubidChar (c))
					throw new XmlException (this as IXmlLineInfo,"character '" + (char) c + "' not allowed for PUBLIC ID");
				if (c != quoteChar)
					AppendValueChar (c);
			}
			return CreateValueString ();
		}

		// The reader is positioned on the first character
		// of the name.
		private string ReadName ()
		{
			string prefix, local;
			return ReadName (out prefix, out local);
		}

		private string ReadName (out string prefix, out string localName)
		{
			// FIXME: need to reject non-QName names?

			int ch = PeekChar ();
			if (!XmlChar.IsFirstNameChar (ch))
				throw new XmlException (this as IXmlLineInfo,String.Format (CultureInfo.InvariantCulture, "a name did not start with a legal character {0} ({1})", ch, (char) ch));

			nameLength = 0;

			ch = ReadChar ();
			// AppendNameChar (ch);
			{
				if (nameLength == nameCapacity)
					ExpandNameCapacity ();
				if (ch < Char.MaxValue)
					nameBuffer [nameLength++] = (char) ch;
				else
					AppendSurrogatePairNameChar (ch);
			}

			int colonAt = -1;

			while (XmlChar.IsNameChar (PeekChar ())) {
				ch = ReadChar ();

				if (namespaces && colonAt < 0 && ch == ':')
					colonAt = nameLength;
				// AppendNameChar (ch);
				{
					if (nameLength == nameCapacity)
						ExpandNameCapacity ();
					if (ch < Char.MaxValue)
						nameBuffer [nameLength++] = (char) ch;
					else
						AppendSurrogatePairNameChar (ch);
				}
			}

			string name = parserContext.NameTable.Add (nameBuffer, 0, nameLength);

			if (namespaces && colonAt > 0) {
				prefix = parserContext.NameTable.Add (nameBuffer, 0, colonAt);
				localName = parserContext.NameTable.Add (nameBuffer, colonAt + 1, nameLength - colonAt - 1);
			}
			else {
				prefix = String.Empty;
				localName = name;
			}

			return name;
		}

		// Read the next character and compare it against the
		// specified character.
		private void Expect (int expected)
		{
			int ch = ReadChar ();

			if (ch != expected) {
				throw new XmlException (this as IXmlLineInfo,
					String.Format (CultureInfo.InvariantCulture, 
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
			for(int i=0; i< len; i++)
				Expect (expected[i]);
		}

		private void ExpectAfterWhitespace (char c)
		{
			while (true) {
				int i = ReadChar ();
				if (i < 0x21 && XmlChar.IsWhitespace (i))
					continue;
				if (c != i)
					throw new XmlException (this, String.Format (CultureInfo.InvariantCulture, "Expected {0}, but found {1} [{2}]", c, (char) i, i));
				break;
			}
		}

		// Does not consume the first non-whitespace character.
		private bool SkipWhitespace ()
		{
			// FIXME: It should be inlined by the JIT.
//			bool skipped = XmlChar.IsWhitespace (PeekChar ());
			int ch = PeekChar ();
			bool skipped = (ch == 0x20 || ch == 0x9 || ch == 0xA || ch == 0xD);
			if (!skipped)
				return false;
			ReadChar ();
			// FIXME: It should be inlined by the JIT.
//			while (XmlChar.IsWhitespace (PeekChar ()))
//				ReadChar ();
			while ((ch = PeekChar ()) == 0x20 || ch == 0x9 || ch == 0xA || ch == 0xD)
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
				// FIXME: it might be optimized by the JIT later,
//				AppendValueChar (ReadChar ());
				{
					ch = ReadChar ();
					if (valueLength == valueCapacity)
						ExpandValueCapacity ();
					if (ch < Char.MaxValue)
						valueBuffer [valueLength++] = (char) ch;
					else
						AppendSurrogatePairValueChar (ch);
				}
			// FIXME: It should be inlined by the JIT.
//			} while ((ch = PeekChar ()) != -1 && XmlChar.IsWhitespace (ch));
				ch = PeekChar ();
			} while (ch == 0x20 || ch == 0x9 || ch == 0xA || ch == 0xD);

			if (currentState == XmlNodeType.Element && ch != -1 && ch != '<')
				ReadText (false);
			else {
				XmlNodeType nodeType = (this.XmlSpace == XmlSpace.Preserve) ?
					XmlNodeType.SignificantWhitespace : XmlNodeType.Whitespace;
				SetProperties (nodeType,
					       String.Empty,
					       String.Empty,
					       String.Empty,
					       false,
					       null, // value: create only when required
					       true);
			}

			return;
		}

		// Since ReadBase64() is processed for every 4 chars, it does
		// not handle '=' here.
		private byte GetBase64Byte (char ch)
		{
			switch (ch) {
			case '+':
				return 62;
			case '/':
				return 63;
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
			if (IsEmptyElement) {
				Read ();
				return 0;
			}

			if (offset < 0)
				throw new ArgumentOutOfRangeException ("offset", offset, "Offset must be non-negative integer.");
			else if (length < 0)
				throw new ArgumentOutOfRangeException ("length", length, "Length must be non-negative integer.");
			else if (buffer.Length < offset + length)
				throw new ArgumentOutOfRangeException ("buffer length is smaller than the sum of offset and length.");

			if (NodeType != XmlNodeType.Element)
				return 0;

			shouldSkipUntilEndTag = true;

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
					if (depthUp) {
						depth++;
						depthUp = false;
					}
					ReadEndTag ();
					shouldSkipUntilEndTag = false;
					Read (); // move to the next node
					return i;
				default:
					ReadChar ();
					if (c < Char.MaxValue)
						buffer [bufIndex++] = (char) c;
					else {
						buffer [bufIndex++] = (char) (c / 0x10000 + 0xD800 - 1);
						buffer [bufIndex++] = (char) (c % 0x10000 + 0xDC00);
					}
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
					if (name != elementNames [elementNameStackPos - 1])
						continue;
					Expect ('>');
					depth--;
					elementNames [--elementNameStackPos] = null;
					return Read ();
				}
			} while (true);
		}
		#endregion
	}
}
