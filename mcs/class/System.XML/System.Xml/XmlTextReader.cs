//
// System.Xml.XmlTextReader
//
// Author:
//   Jason Diamond (jason@injektilo.org)
//   Adam Treat (manyoso@yahoo.com)
//   Atsushi Enomoto  (ginga@kit.hi-ho.ne.jp)
//
// (C) 2001, 2002 Jason Diamond  http://injektilo.org/
// Copyright (C) 2005-2006 Novell, Inc (http://www.novell.com)
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
//#define USE_NAME_BUFFER

//
// Optimization TODOs:
//
//	- support PushbackChar() which reverts one character read.
//		- ReadTextReader() should always keep one pushback buffer
//		  as pushback safety net.
//	- Replace (peek,read) * n -> read * n + pushback
//

using System;
using System.Collections;
#if NET_2_0
using System.Collections.Generic;
#endif
using System.Globalization;
using System.IO;
using System.Security.Permissions;
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
	[PermissionSet (SecurityAction.InheritanceDemand, Unrestricted = true)]
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
			string uriString;
			Stream stream = GetStreamFromUrl (url, out uriString);
			XmlParserContext ctx = new XmlParserContext (nt,
				new XmlNamespaceManager (nt),
				String.Empty,
				XmlSpace.None);
			this.InitializeContext (uriString, ctx, new XmlStreamReader (stream), XmlNodeType.Document);
		}

		public XmlTextReader (TextReader input, XmlNameTable nt)
			: this (String.Empty, input, nt)
		{
		}

		// This is used in XmlReader.Create() to indicate that string
		// argument is uri, not an xml fragment.
		internal XmlTextReader (bool dummy, XmlResolver resolver, string url, XmlNodeType fragType, XmlParserContext context)
		{
			if (resolver == null) {
#if MOONLIGHT
				resolver = new XmlXapResolver ();
#else
				resolver = new XmlUrlResolver ();
#endif
			}
			this.XmlResolver = resolver;
			string uriString;
			Stream stream = GetStreamFromUrl (url, out uriString);
			this.InitializeContext (uriString, context, new XmlStreamReader (stream), fragType);
		}

		public XmlTextReader (Stream xmlFragment, XmlNodeType fragType, XmlParserContext context)
			: this (context != null ? context.BaseURI : String.Empty,
				new XmlStreamReader (xmlFragment),
			fragType,
			context)
		{
			disallowReset = true;
		}

		internal XmlTextReader (string baseURI, TextReader xmlFragment, XmlNodeType fragType)
			: this (baseURI, xmlFragment, fragType, null)
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
			disallowReset = true;
		}

		internal XmlTextReader (string url, TextReader fragment, XmlNodeType fragType, XmlParserContext context)
		{
			InitializeContext (url, context, fragment, fragType);
		}

		private Stream GetStreamFromUrl (string url, out string absoluteUriString)
		{
#if NET_2_1
			if (url == null)
				throw new ArgumentNullException ("url");
			if (url.Length == 0)
				throw new ArgumentException ("url");
#endif
			Uri uri = resolver.ResolveUri (null, url);
			absoluteUriString = uri != null ? uri.ToString () : String.Empty;
			return resolver.GetEntity (uri, null, typeof (Stream)) as Stream;
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

#if NET_2_0
		public override bool CanReadBinaryContent {
			get { return true; }
		}

		public override bool CanReadValueChunk {
			get { return true; }
		}
#else
		internal override bool CanReadBinaryContent {
			get { return true; }
		}

		internal override bool CanReadValueChunk {
			get { return true; }
		}
#endif

		internal bool CharacterChecking {
			get { return checkCharacters; }
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
			get { return nameTable; }
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

		public bool ProhibitDtd {
			get { return prohibitDtd; }
			set { prohibitDtd = value; }
		}

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
		public IDictionary<string, string> GetNamespacesInScope (XmlNamespaceScope scope)
		{
			return nsmgr.GetNamespacesInScope (scope);
		}

		IDictionary<string, string> IXmlNamespaceResolver.GetNamespacesInScope (XmlNamespaceScope scope)
		{
			return GetNamespacesInScope (scope);
		}
#endif

		public TextReader GetRemainder ()
		{
			if (peekCharsLength < 0)
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

		private string LookupNamespace (string prefix, bool atomizedNames)
		{
			string s = nsmgr.LookupNamespace (
				prefix, atomizedNames);
			return s == String.Empty ? null : s;
		}

#if NET_2_0
		string IXmlNamespaceResolver.LookupPrefix (string ns)
		{
			return LookupPrefix (ns, false);
		}

		public string LookupPrefix (string ns, bool atomizedName)
		{
			return nsmgr.LookupPrefix (ns, atomizedName);
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
			if (readState == ReadState.Closed)
				return false;
			curNodePeekIndex = peekCharsIndex;
			preserveCurrentTag = true;
			nestLevel = 0;
			ClearValueBuffer ();

			if (startNodeType == XmlNodeType.Attribute) {
				if (currentAttribute == 0)
					return false;	// already read.
				SkipTextDeclaration ();
				ClearAttributes ();
				IncrementAttributeToken ();
				ReadAttributeValueTokens ('"');
				cursorToken = attributeTokens [0];
				currentAttributeValue = -1;
				readState = ReadState.Interactive;
				return true;
			}
			if (readState == ReadState.Initial && currentState == XmlNodeType.Element)
				SkipTextDeclaration ();

			if (Binary != null)
				Binary.Reset ();

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

			if (readCharsInProgress) {
				readCharsInProgress = false;
				return ReadUntilEndTag ();
			}

			more = ReadContent ();

			if (!more && startNodeType == XmlNodeType.Document && currentState != XmlNodeType.EndElement)
				throw NotWFError ("Document element did not appear.");

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

		public int ReadBase64 (byte [] buffer, int offset, int length)
		{
			BinaryCharGetter = binaryCharGetter;
			try {
				return Binary.ReadBase64 (buffer, offset, length);
			} finally {
				BinaryCharGetter = null;
			}
		}

		public int ReadBinHex (byte [] buffer, int offset, int length)
		{
			BinaryCharGetter = binaryCharGetter;
			try {
				return Binary.ReadBinHex (buffer, offset, length);
			} finally {
				BinaryCharGetter = null;
			}
		}

		public int ReadChars (char [] buffer, int offset, int length)
		{
			if (offset < 0) {
				throw new ArgumentOutOfRangeException (
#if !NET_2_1
					"offset", offset,
#endif
					"Offset must be non-negative integer.");

			} else if (length < 0) {
				throw new ArgumentOutOfRangeException (
#if !NET_2_1
					"length", length,
#endif
					"Length must be non-negative integer.");

			} else if (buffer.Length < offset + length)
				throw new ArgumentOutOfRangeException ("buffer length is smaller than the sum of offset and length.");

			if (IsEmptyElement) {
				Read ();
				return 0;
			}

			if (!readCharsInProgress && NodeType != XmlNodeType.Element)
				return 0;

			preserveCurrentTag = false;
			readCharsInProgress = true;
			useProceedingLineInfo = true;

			return ReadCharsInternal (buffer, offset, length);
		}

		public void ResetState ()
		{
			if (disallowReset)
				throw new InvalidOperationException ("Cannot call ResetState when parsing an XML fragment.");
			Clear ();
		}

		public override void ResolveEntity ()
		{
			// XmlTextReader does not resolve entities.
			throw new InvalidOperationException ("XmlTextReader cannot resolve external entities.");
		}

#if NET_2_0
		[MonoTODO] // FIXME: Implement, for performance improvement
		public override void Skip ()
		{
			base.Skip ();
		}
#endif
		#endregion

		#region Internals
		// Parsed DTD Objects
		// Note that thgis property must be kept since dtd2xsd uses it.
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
			public int ValueBufferStart;
			public int ValueBufferEnd;

			public XmlNodeType NodeType;

			public virtual string Value {
				get {
					if (valueCache != null)
						return valueCache;
					if (ValueBufferStart >= 0) {
//Console.WriteLine (NodeType + " / " + ValueBuffer.Length + " / " + ValueBufferStart + " / " + ValueBufferEnd);
						valueCache = Reader.valueBuffer.ToString (ValueBufferStart, ValueBufferEnd - ValueBufferStart);
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
				: base (reader)
			{
				NodeType = XmlNodeType.Attribute;
			}

			public int ValueTokenStartIndex;
			public int ValueTokenEndIndex;
			string valueCache;
			StringBuilder tmpBuilder = new StringBuilder ();

			public override string Value {
				get {
					if (valueCache != null)
						return valueCache;

					// An empty value should return String.Empty.
					if (ValueTokenStartIndex == ValueTokenEndIndex) {
						XmlTokenInfo ti = Reader.attributeValueTokens [ValueTokenStartIndex];
						if (ti.NodeType == XmlNodeType.EntityReference)
							valueCache = String.Concat ("&", ti.Name, ";");
						else
							valueCache = ti.Value;
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

					valueCache = tmpBuilder.ToString (0, tmpBuilder.Length);
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
					Reader.nsmgr.AddNamespace (LocalName, Value);
				else if (Object.ReferenceEquals (Name, XmlNamespaceManager.PrefixXmlns))
					Reader.nsmgr.AddNamespace (String.Empty, Value);
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
		private XmlNameTable nameTable;
		private XmlNamespaceManager nsmgr;

		private ReadState readState;
		private bool disallowReset;

		private int depth;
		private int elementDepth;
		private bool depthUp;

		private bool popScope;

		struct TagName
		{
			public TagName (string n, string l, string p)
			{
				Name = n;
				LocalName = l;
				Prefix = p;
			}

			public readonly string Name;
			public readonly string LocalName;
			public readonly string Prefix;
		}

		private TagName [] elementNames;
		int elementNameStackPos;

		private bool allowMultipleRoot;

		private bool isStandalone;

		private bool returnEntityReference;
		private string entityReferenceName;

#if USE_NAME_BUFFER
		private char [] nameBuffer;
		private int nameLength;
		private int nameCapacity;
		private const int initialNameCapacity = 32;
#endif

		private StringBuilder valueBuffer;

		private TextReader reader;
		private char [] peekChars;
		private int peekCharsIndex;
		private int peekCharsLength;
		private int curNodePeekIndex;
		private bool preserveCurrentTag;
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
		private int nestLevel;
		private bool readCharsInProgress;
		XmlReaderBinarySupport.CharGetter binaryCharGetter;

		// These values are never re-initialized.
		private bool namespaces = true;
		private WhitespaceHandling whitespaceHandling = WhitespaceHandling.All;
#if MOONLIGHT
		private XmlResolver resolver = new XmlXapResolver ();
#else
		private XmlResolver resolver = new XmlUrlResolver ();
#endif
		private bool normalization = false;

		private bool checkCharacters;
		private bool prohibitDtd = false;
		private bool closeInput = true;
		private EntityHandling entityHandling; // 2.0

		private NameTable whitespacePool;
		private char [] whitespaceCache;

		private XmlException NotWFError (string message)
		{
			return new XmlException (this as IXmlLineInfo, BaseURI, message);
		}

		private void Init ()
		{
			allowMultipleRoot = false;
			elementNames = new TagName [10];
			valueBuffer = new StringBuilder ();
			binaryCharGetter = new XmlReaderBinarySupport.CharGetter (ReadChars);
#if USE_NAME_BUFFER
			nameBuffer = new char [initialNameCapacity];
#endif

			checkCharacters = true;
#if NET_2_0
			if (Settings != null)
				checkCharacters = Settings.CheckCharacters;
#endif
			prohibitDtd = false;
			closeInput = true;
			entityHandling = EntityHandling.ExpandCharEntities;

			peekCharsIndex = 0;
			if (peekChars == null)
				peekChars = new char [peekCharCapacity];
			peekCharsLength = -1;
			curNodePeekIndex = -1; // read from start

			line = 1;
			column = 1;

			currentLinkedNodeLineNumber = currentLinkedNodeLinePosition = 0;

			Clear ();
		}

		private void Clear ()
		{
			currentToken = new XmlTokenInfo (this);
			cursorToken = currentToken;
			currentAttribute = -1;
			currentAttributeValue = -1;
			attributeCount = 0;

			readState = ReadState.Initial;

			depth = 0;
			elementDepth = 0;
			depthUp = false;

			popScope = allowMultipleRoot = false;
			elementNameStackPos = 0;

			isStandalone = false;
			returnEntityReference = false;
			entityReferenceName = String.Empty;

#if USE_NAME_BUFFER
			nameLength = 0;
			nameCapacity = initialNameCapacity;
#endif
			useProceedingLineInfo = false;

			currentState = XmlNodeType.None;

			readCharsInProgress = false;
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
			nameTable = parserContext.NameTable;
			nameTable = nameTable != null ? nameTable : new NameTable ();
			nsmgr = parserContext.NamespaceManager;
			nsmgr = nsmgr != null ? nsmgr : new XmlNamespaceManager (nameTable);

			if (url != null && url.Length > 0) {
#if NET_2_1
				Uri uri = new Uri (url, UriKind.RelativeOrAbsolute);
#else
				Uri uri = null;
				try {
#if NET_2_0
					uri = new Uri (url, UriKind.RelativeOrAbsolute);
#else
					uri = new Uri (url);
#endif
				} catch (Exception) {
					string path = Path.GetFullPath ("./a");
					uri = new Uri (new Uri (path), url);
				}
#endif
				parserContext.BaseURI = uri.ToString ();
			}

			Init ();

			reader = fragment;

			switch (fragType) {
			case XmlNodeType.Attribute:
				reader = new StringReader (fragment.ReadToEnd ().Replace ("\"", "&quot;"));
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
		}

#if NET_2_0
		internal ConformanceLevel Conformance {
			get { return allowMultipleRoot ? ConformanceLevel.Fragment : ConformanceLevel.Document; }
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
			//for (int i = 0; i < attributeCount; i++)
			//	attributeTokens [i].Clear ();
			attributeCount = 0;
			currentAttribute = -1;
			currentAttributeValue = -1;
		}

		private int PeekSurrogate (int c)
		{
			if (peekCharsLength <= peekCharsIndex + 1) {
				if (!ReadTextReader (c))
					//FIXME: copy MS.NET behaviour when unpaired surrogate found
					return c;
			}

			int highhalfChar = peekChars [peekCharsIndex];
			int lowhalfChar = peekChars [peekCharsIndex+1];

			if (((highhalfChar & 0xFC00) != 0xD800) || ((lowhalfChar & 0xFC00) != 0xDC00))
				//FIXME: copy MS.NET behaviour when unpaired surrogate found
				return highhalfChar;
			return 0x10000 + (highhalfChar-0xD800)*0x400 + (lowhalfChar-0xDC00);
		}

		private int PeekChar ()
		{
			if (peekCharsIndex < peekCharsLength) {
				int c = peekChars [peekCharsIndex];
				if (c == 0)
					return -1;
				if (c < 0xD800 || c >= 0xDFFF)
					return c;
				return PeekSurrogate (c);
			} else {
				if (!ReadTextReader (-1))
					return -1;
				return PeekChar ();
			}
		}

		private int ReadChar ()
		{
			int ch = PeekChar ();
			peekCharsIndex++;

			if (ch >= 0x10000)
				peekCharsIndex++; //Increment by 2 when a compound UCS-4 character was found

			if (ch == '\n') {
				line++;
				column = 1;
			} else if (ch != -1) {
				column++;
			}
			return ch;
		}

		private void Advance (int ch) {
			peekCharsIndex++;

			if (ch >= 0x10000)
				peekCharsIndex++; //Increment by 2 when a compound UCS-4 character was found

			if (ch == '\n') {
				line++;
				column = 1;
			} else if (ch != -1) {
				column++;
			}
		}

		private bool ReadTextReader (int remained)
		{
			if (peekCharsLength < 0) {	// initialized buffer
				peekCharsLength = reader.Read (peekChars, 0, peekChars.Length);
				return peekCharsLength > 0;
			}
			int offset = remained >= 0 ? 1 : 0;
			int copysize = peekCharsLength - curNodePeekIndex;

			// It must assure that current tag content always exists
			// in peekChars.
			if (!preserveCurrentTag) {
				curNodePeekIndex = 0;
				peekCharsIndex = 0;
				//copysize = 0;
			} else if (peekCharsLength < peekChars.Length) {
				// NonBlockingStreamReader returned less bytes
				// than the size of the buffer. In that case,
				// just refill the buffer.
			} else if (curNodePeekIndex <= (peekCharsLength >> 1)) {
				// extend the buffer
				char [] tmp = new char [peekChars.Length * 2];
				Array.Copy (peekChars, curNodePeekIndex,
					tmp, 0, copysize);
				peekChars = tmp;
				curNodePeekIndex = 0;
				peekCharsIndex = copysize;
			} else {
				Array.Copy (peekChars, curNodePeekIndex,
					peekChars, 0, copysize);
				curNodePeekIndex = 0;
				peekCharsIndex = copysize;
			}
			if (remained >= 0)
				peekChars [peekCharsIndex] = (char) remained;
			int count = peekChars.Length - peekCharsIndex - offset;
			if (count > peekCharCapacity)
				count = peekCharCapacity;
			int read = reader.Read (
				peekChars, peekCharsIndex + offset, count);
			int remainingSize = offset + read;
			peekCharsLength = peekCharsIndex + remainingSize;

			return (remainingSize != 0);
		}

		private bool ReadContent ()
		{
			if (popScope) {
				nsmgr.PopScope ();
				parserContext.PopScope ();
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
						throw NotWFError ("unexpected end of file. Current depth is " + depth);

					return false;
				} else {
 	   				switch (c) {
					case '<':
						Advance (c);
						switch (PeekChar ())
						{
						case '/':
							Advance ('/');
							ReadEndTag ();
							break;
						case '?':
							Advance ('?');
							ReadProcessingInstruction ();
							break;
						case '!':
							Advance ('!');
							ReadDeclaration ();
							break;
						default:
							ReadStartTag ();
							break;
						}
						break;
					case '\r':
					case '\n':
					case '\t':
					case ' ':
						if (!ReadWhitespace ())
							// skip
							return ReadContent ();
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
					throw NotWFError ("Standalone document must not contain any references to an non-internally declared entity.");
			if (decl != null && decl.NotationName != null)
				throw NotWFError ("Reference to any unparsed entities is not allowed here.");

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
				throw NotWFError ("Multiple document element was detected.");
			currentState = XmlNodeType.Element;

			nsmgr.PushScope ();

			currentLinkedNodeLineNumber = line;
			currentLinkedNodeLinePosition = column;

			string prefix, localName;
			string name = ReadName (out prefix, out localName);
			if (currentState == XmlNodeType.EndElement)
				throw NotWFError ("document has terminated, cannot open new element");

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
			if (namespaces)
				for (int i = 0; i < attributeCount; i++)
					if (attributeTokens [i].Prefix == "xmlns" &&
						attributeTokens [i].Value == String.Empty)
						throw NotWFError ("Empty namespace URI cannot be mapped to non-empty prefix.");

			for (int i = 0; i < attributeCount; i++) {
				for (int j = i + 1; j < attributeCount; j++)
					if (Object.ReferenceEquals (attributeTokens [i].Name, attributeTokens [j].Name) ||
						(Object.ReferenceEquals (attributeTokens [i].LocalName, attributeTokens [j].LocalName) &&
						Object.ReferenceEquals (attributeTokens [i].NamespaceURI, attributeTokens [j].NamespaceURI)))
						throw NotWFError ("Attribute name and qualified name must be identical.");
			}

			if (PeekChar () == '/') {
				Advance ('/');
				isEmptyElement = true;
				popScope = true;
			}
			else {
				depthUp = true;
				PushElementName (name, localName, prefix);
			}
			parserContext.PushScope ();

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
				currentToken.NamespaceURI = nsmgr.DefaultNamespace;

			if (namespaces) {
				if (NamespaceURI == null)
					throw NotWFError (String.Format ("'{0}' is undeclared namespace.", Prefix));
				try {
					for (int i = 0; i < attributeCount; i++) {
						MoveToAttribute (i);
						if (NamespaceURI == null)
							throw NotWFError (String.Format ("'{0}' is undeclared namespace.", Prefix));
					}
				} finally {
					MoveToElement ();
				}
			}

			for (int i = 0; i < attributeCount; i++) {
				if (!Object.ReferenceEquals (attributeTokens [i].Prefix, XmlNamespaceManager.PrefixXml))
					continue;
				string aname = attributeTokens [i].LocalName;
				string value = attributeTokens [i].Value;
				switch (aname) {
				case "base":
					if (this.resolver != null) {
						Uri buri =
							BaseURI != String.Empty ?
							new Uri (BaseURI) : null;
						// xml:base="" without any base URI -> pointless. However there are
						// some people who use such xml:base. Seealso bug #608391.
						if (buri == null && String.IsNullOrEmpty (value))
							break;
						Uri uri = resolver.ResolveUri (
							buri, value);
						parserContext.BaseURI =
							uri != null ?
							uri.ToString () :
							String.Empty;
					}
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
						throw NotWFError (String.Format ("Invalid xml:space value: {0}", value));
					}
					break;
				}
			}

			if (IsEmptyElement)
				CheckCurrentStateUpdate ();
		}

		private void PushElementName (string name, string local, string prefix)
		{
			if (elementNames.Length == elementNameStackPos) {
				TagName [] newArray = new TagName [elementNames.Length * 2];
				Array.Copy (elementNames, 0, newArray, 0, elementNameStackPos);
				elementNames = newArray;
			}
			elementNames [elementNameStackPos++] =
				new TagName (name, local, prefix);
		}

		// The reader is positioned on the first character
		// of the element's name.
		private void ReadEndTag ()
		{
			if (currentState != XmlNodeType.Element)
				throw NotWFError ("End tag cannot appear in this state.");

			currentLinkedNodeLineNumber = line;
			currentLinkedNodeLinePosition = column;

			if (elementNameStackPos == 0)
				throw NotWFError ("closing element without matching opening element");
			TagName expected = elementNames [--elementNameStackPos];
			Expect (expected.Name);

			ExpectAfterWhitespace ('>');

			--depth;

			SetProperties (
				XmlNodeType.EndElement, // nodeType
				expected.Name, // name
				expected.Prefix, // prefix
				expected.LocalName, // localName
				false, // isEmptyElement
				null, // value
				true // clearAttributes
			);
			if (expected.Prefix.Length > 0)
				currentToken.NamespaceURI = LookupNamespace (expected.Prefix, true);
			else if (namespaces)
				currentToken.NamespaceURI = nsmgr.DefaultNamespace;

			popScope = true;

			CheckCurrentStateUpdate ();
		}

		private void CheckCurrentStateUpdate ()
		{
			if (depth == 0 && !allowMultipleRoot && (IsEmptyElement || NodeType == XmlNodeType.EndElement))
				currentState = XmlNodeType.EndElement;
		}

#if USE_NAME_BUFFER
		private void AppendSurrogatePairNameChar (int ch)
		{
			nameBuffer [nameLength++] = (char) ((ch - 0x10000) / 0x400 + 0xD800);
			if (nameLength == nameCapacity)
				ExpandNameCapacity ();
			nameBuffer [nameLength++] = (char) ((ch - 0x10000) % 0x400 + 0xDC00);
		}

		private void ExpandNameCapacity ()
		{
			nameCapacity = nameCapacity * 2;
			char [] oldNameBuffer = nameBuffer;
			nameBuffer = new char [nameCapacity];
			Array.Copy (oldNameBuffer, nameBuffer, nameLength);
		}
#endif

		private void AppendValueChar (int ch)
		{
			if (ch <= Char.MaxValue)
				valueBuffer.Append ((char) ch);
			else
				AppendSurrogatePairValueChar (ch);
		}

		private void AppendSurrogatePairValueChar (int ch)
		{
			valueBuffer.Append ((char) ((ch - 0x10000) / 0x400 + 0xD800));
			valueBuffer.Append ((char) ((ch - 0x10000) % 0x400 + 0xDC00));
		}

		private string CreateValueString ()
		{
			// Since whitespace strings are mostly identical
			// depending on the Depth, we make use of NameTable
			// to atomize whitespace strings.
			switch (NodeType) {
			case XmlNodeType.Whitespace:
			case XmlNodeType.SignificantWhitespace:
				int len = valueBuffer.Length;
				if (whitespaceCache == null)
					whitespaceCache = new char [32];
				if (len >= whitespaceCache.Length)
					break;
				if (whitespacePool == null)
					whitespacePool = new NameTable ();
#if NET_2_0 && !NET_2_1
				valueBuffer.CopyTo (0, whitespaceCache, 0, len);
#else
				for (int i = 0; i < len; i++)
					whitespaceCache [i] = valueBuffer [i];
#endif
				return whitespacePool.Add (whitespaceCache, 0, valueBuffer.Length);
			}
			return (valueBuffer.Capacity < 100) ?
				valueBuffer.ToString (0, valueBuffer.Length) :
				valueBuffer.ToString ();
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
				throw NotWFError ("Text node cannot appear in this state.");
			preserveCurrentTag = false;

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
					ch = PeekChar ();
					if (ch != '\n')
						// append '\n' instead of '\r'.
						AppendValueChar ('\n');
					// and in case of "\r\n", discard '\r'.
					continue;
				} else {
					if (CharacterChecking && XmlChar.IsInvalid (ch))
						throw NotWFError ("Not allowed character was found.");
					ch = ReadChar ();
				}

				// FIXME: it might be optimized by the JIT later,
//				AppendValueChar (ch);
				{
					if (ch <= Char.MaxValue)
						valueBuffer.Append ((char) ch);
					else
						AppendSurrogatePairValueChar (ch);
				}

				// Block "]]>"
				if (ch == ']') {
					if (previousWasCloseBracket)
						if (PeekChar () == '>')
							throw NotWFError ("Inside text content, character sequence ']]>' is not allowed.");
					previousWasCloseBracket = true;
				}
				else if (previousWasCloseBracket)
					previousWasCloseBracket = false;
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
				Advance ('#');
				return ReadCharacterReference ();
			} else
				return ReadEntityReference (ignoreEntityReferences);
		}

		private int ReadCharacterReference ()
		{
			int value = 0;
			int ch;

			if (PeekChar () == 'x') {
				Advance ('x');

				while ((ch = PeekChar ()) != ';' && ch != -1) {
					Advance (ch);

					if (ch >= '0' && ch <= '9')
						value = (value << 4) + ch - '0';
					else if (ch >= 'A' && ch <= 'F')
						value = (value << 4) + ch - 'A' + 10;
					else if (ch >= 'a' && ch <= 'f')
						value = (value << 4) + ch - 'a' + 10;
					else
						throw NotWFError (String.Format (CultureInfo.InvariantCulture, 
								"invalid hexadecimal digit: {0} (#x{1:X})",
								(char) ch,
								ch));
				}
			} else {
				while ((ch = PeekChar ()) != ';' && ch != -1) {
					Advance (ch);

					if (ch >= '0' && ch <= '9')
						value = value * 10 + ch - '0';
					else
						throw NotWFError (String.Format (CultureInfo.InvariantCulture, 
								"invalid decimal digit: {0} (#x{1:X})",
								(char) ch,
								ch));
				}
			}

			ReadChar (); // ';'

			// There is no way to save surrogate pairs...
			if (CharacterChecking && Normalization &&
				XmlChar.IsInvalid (value))
				throw NotWFError ("Referenced character was not allowed in XML. Normalization is " + normalization + ", checkCharacters = " + checkCharacters);
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
					throw NotWFError ("Unexpected token. Name is required here.");

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

		private void AddAttributeWithValue (string name, string value)
		{
			IncrementAttributeToken ();
			XmlAttributeTokenInfo ati = attributeTokens [currentAttribute];
			ati.Name = NameTable.Add (name);
			ati.Prefix = String.Empty;
			ati.NamespaceURI = String.Empty;
			IncrementAttributeValueToken ();
			XmlTokenInfo vti = attributeValueTokens [currentAttributeValue];
			SetTokenProperties (vti,
				XmlNodeType.Text,
				String.Empty,
				String.Empty,
				String.Empty,
				false,
				value,
				false);
			ati.Value = value;
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
				attributeValueTokens [currentAttributeValue] = new XmlTokenInfo (this);
			currentAttributeValueToken = attributeValueTokens [currentAttributeValue];
			currentAttributeValueToken.Clear ();
		}

		// LAMESPEC: Orthodox XML reader should normalize attribute values
		private void ReadAttributeValueTokens (int dummyQuoteChar)
		{
			int quoteChar = (dummyQuoteChar < 0) ? ReadChar () : dummyQuoteChar;

			if (quoteChar != '\'' && quoteChar != '\"')
				throw NotWFError ("an attribute value was not quoted");
			currentAttributeToken.QuoteChar = (char) quoteChar;

			IncrementAttributeValueToken ();
			currentAttributeToken.ValueTokenStartIndex = currentAttributeValue;
			currentAttributeValueToken.LineNumber = line;
			currentAttributeValueToken.LinePosition = column;

			bool incrementToken = false;
			bool isNewToken = true;
			bool loop = true;
			int ch = 0;
			currentAttributeValueToken.ValueBufferStart = valueBuffer.Length;
			while (loop) {
				ch = ReadChar ();
				if (ch == quoteChar)
					break;

				if (incrementToken) {
					IncrementAttributeValueToken ();
					currentAttributeValueToken.ValueBufferStart = valueBuffer.Length;
					currentAttributeValueToken.LineNumber = line;
					currentAttributeValueToken.LinePosition = column;
					incrementToken = false;
					isNewToken = true;
				}

				switch (ch)
				{
				case '<':
					throw NotWFError ("attribute values cannot contain '<'");
				case -1:
					if (dummyQuoteChar < 0)
						throw NotWFError ("unexpected end of file in an attribute value");
					else	// Attribute value constructor.
						loop = false;
					break;
				case '\r':
					if (!normalization)
						goto default;
					if (PeekChar () == '\n')
						continue; // skip '\r'.
					//
					// The csc in MS.NET 2.0 beta 1 barfs on this goto, so work around that
					//
					//goto case '\n';
					if (!normalization)
						goto default;
					ch = ' ';
					goto default;					
				case '\n':
				case '\t':
					// When Normalize = true, then replace
					// all spaces to ' '
					if (!normalization)
						goto default;
					ch = ' ';
					goto default;
				case '&':
					if (PeekChar () == '#') {
						Advance ('#');
						ch = ReadCharacterReference ();
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
							foreach (char c in (IEnumerable<char>) value)
								AppendValueChar (c);
						} else
#endif
						{
							currentAttributeValueToken.ValueBufferEnd = valueBuffer.Length;
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
						throw NotWFError ("Invalid character was found.");
					// FIXME: it might be optimized by the JIT later,
//					AppendValueChar (ch);
					{
						if (ch <= Char.MaxValue)
							valueBuffer.Append ((char) ch);
						else
							AppendSurrogatePairValueChar (ch);
					}
					break;
				}

				isNewToken = false;
			}
			if (!incrementToken) {
				currentAttributeValueToken.ValueBufferEnd = valueBuffer.Length;
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
					throw NotWFError (String.Format ("Referenced entity '{0}' does not exist.", entName));
				else
					return;
			}

			if (entDecl.HasExternalReference)
				throw NotWFError ("Reference to external entities is not allowed in the value of an attribute.");
			if (isStandalone && !entDecl.IsInternalSubset)
				throw NotWFError ("Reference to external entities is not allowed in the internal subset.");
			if (entDecl.EntityValue.IndexOf ('<') >= 0)
				throw NotWFError ("Attribute must not contain character '<' either directly or indirectly by way of entity references.");
		}

		// The reader is positioned on the first character
		// of the target.
		//
		// It may be xml declaration or processing instruction.
		private void ReadProcessingInstruction ()
		{
			string target = ReadName ();
			if (target != "xml" && target.ToLower (CultureInfo.InvariantCulture) == "xml")
				throw NotWFError ("Not allowed processing instruction name which starts with 'X', 'M', 'L' was found.");

			if (!SkipWhitespace ())
				if (PeekChar () != '?')
					throw NotWFError ("Invalid processing instruction name was found.");

			ClearValueBuffer ();

			int ch;
			while ((ch = PeekChar ()) != -1) {
				Advance (ch);

				if (ch == '?' && PeekChar () == '>') {
					Advance ('>');
					break;
				}

				if (CharacterChecking && XmlChar.IsInvalid (ch))
					throw NotWFError ("Invalid character was found.");
				AppendValueChar (ch);
			}

			if (Object.ReferenceEquals (target, XmlNamespaceManager.PrefixXml))
				VerifyXmlDeclaration ();
			else {
				if (currentState == XmlNodeType.None)
					currentState = XmlNodeType.XmlDeclaration;

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
		}

		void VerifyXmlDeclaration ()
		{
			if (!allowMultipleRoot && currentState != XmlNodeType.None)
				throw NotWFError ("XML declaration cannot appear in this state.");

			currentState = XmlNodeType.XmlDeclaration;

			string text = CreateValueString ();

			ClearAttributes ();

			int idx = 0;

			string encoding = null, standalone = null;
			string name, value;
			ParseAttributeFromString (text, ref idx, out name, out value);
			if (name != "version" || value != "1.0")
				throw NotWFError ("'version' is expected.");
			name = String.Empty;
			if (SkipWhitespaceInString (text, ref idx) && idx < text.Length)
				ParseAttributeFromString (text, ref idx, out name, out value);
			if (name == "encoding") {
				if (!XmlChar.IsValidIANAEncoding (value))
					throw NotWFError ("'encoding' must be a valid IANA encoding name.");
				if (reader is XmlStreamReader)
					parserContext.Encoding = ((XmlStreamReader) reader).Encoding;
				else
					parserContext.Encoding = Encoding.Unicode;
				encoding = value;
				name = String.Empty;
				if (SkipWhitespaceInString (text, ref idx) && idx < text.Length)
					ParseAttributeFromString (text, ref idx, out name, out value);
			}
			if (name == "standalone") {
				this.isStandalone = value == "yes";
				if (value != "yes" && value != "no")
					throw NotWFError ("Only 'yes' or 'no' is allow for 'standalone'");
				standalone = value;
				SkipWhitespaceInString (text, ref idx);
			}
			else if (name.Length != 0)
				throw NotWFError (String.Format ("Unexpected token: '{0}'", name));

			if (idx < text.Length)
				throw NotWFError ("'?' is expected.");

			AddAttributeWithValue ("version", "1.0");
			if (encoding != null)
				AddAttributeWithValue ("encoding", encoding);
			if (standalone != null)
				AddAttributeWithValue ("standalone", standalone);
			currentAttribute = currentAttributeValue = -1;

			SetProperties (
				XmlNodeType.XmlDeclaration, // nodeType
				"xml", // name
				String.Empty, // prefix
				"xml", // localName
				false, // isEmptyElement
				text, // value
				false // clearAttributes
			);
		}

		bool SkipWhitespaceInString (string text, ref int idx)
		{
			int start = idx;
			while (idx < text.Length && XmlChar.IsWhitespace (text [idx]))
				idx++;
			return idx - start > 0;
		}

		private void ParseAttributeFromString (string src,
			ref int idx, out string name, out string value)
		{
			while (idx < src.Length && XmlChar.IsWhitespace (src [idx]))
				idx++;

			int start = idx;
			while (idx < src.Length && XmlChar.IsNameChar (src [idx]))
				idx++;
			name = src.Substring (start, idx - start);

			while (idx < src.Length && XmlChar.IsWhitespace (src [idx]))
				idx++;
			if (idx == src.Length || src [idx] != '=')
				throw NotWFError (String.Format ("'=' is expected after {0}", name));
			idx++;

			while (idx < src.Length && XmlChar.IsWhitespace (src [idx]))
				idx++;

			if (idx == src.Length || src [idx] != '"' && src [idx] != '\'')
				throw NotWFError ("'\"' or '\'' is expected.");

			char quote = src [idx];
			idx++;
			start = idx;

			while (idx < src.Length && src [idx] != quote)
				idx++;
			idx++;

			value = src.Substring (start, idx - start - 1);
		}

		internal void SkipTextDeclaration ()
		{
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
				if (new string (peekChars, 2, 4).ToLower (CultureInfo.InvariantCulture) == "xml ") {
					throw NotWFError ("Processing instruction name must not be character sequence 'X' 'M' 'L' with case insensitivity.");
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
#if NET_2_0
			// this condition is to check if this instance is
			// not created by XmlReader.Create() (which just
			// omits strict text declaration check).
			else if (Conformance == ConformanceLevel.Auto)
				throw NotWFError ("Encoding declaration is mandatory in text declaration.");
#endif

			Expect ("?>");

			curNodePeekIndex = peekCharsIndex; // without this it causes incorrect value start indication.
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
				throw NotWFError ("Unexpected declaration markup was found.");
			}
		}

		// The reader is positioned on the first character after
		// the leading '<!--'.
		private void ReadComment ()
		{
			if (currentState == XmlNodeType.None)
				currentState = XmlNodeType.XmlDeclaration;

			preserveCurrentTag = false;

			ClearValueBuffer ();

			int ch;
			while ((ch = PeekChar ()) != -1) {
				Advance (ch);

				if (ch == '-' && PeekChar () == '-') {
					Advance ('-');

					if (PeekChar () != '>')
						throw NotWFError ("comments cannot contain '--'");

					Advance ('>');
					break;
				}

				if (XmlChar.IsInvalid (ch))
					throw NotWFError ("Not allowed character was found.");

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
				throw NotWFError ("CDATA section cannot appear in this state.");
			preserveCurrentTag = false;

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
					throw NotWFError ("Invalid character was found.");

				// FIXME: it might be optimized by the JIT later,
//				AppendValueChar (ch);
				{
					if (ch <= Char.MaxValue)
						valueBuffer.Append ((char) ch);
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
				throw NotWFError ("Document Type Declaration (DTD) is prohibited in this XML.");
			switch (currentState) {
			case XmlNodeType.DocumentType:
			case XmlNodeType.Element:
			case XmlNodeType.EndElement:
				throw NotWFError ("Document type cannot appear in this state.");
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
					throw NotWFError ("Whitespace is required between PUBLIC id and SYSTEM id.");
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
				ClearValueBuffer ();
				ReadInternalSubset ();
				parserContext.InternalSubset = CreateValueString ();
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
				AddAttributeWithValue ("PUBLIC", publicId);
			if (systemId != null)
				AddAttributeWithValue ("SYSTEM", systemId);
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

		private int ReadValueChar ()
		{
			int ret = ReadChar ();
			AppendValueChar (ret);
			return ret;
		}

		private void ExpectAndAppend (string s)
		{
			Expect (s);
			valueBuffer.Append (s);
		}

		// Simply read but not generate any result.
		private void ReadInternalSubset ()
		{
			bool continueParse = true;

			while (continueParse) {
				switch (ReadValueChar ()) {
				case ']':
					switch (State) {
					case DtdInputState.Free:
						// chop extra ']'
						valueBuffer.Remove (valueBuffer.Length - 1, 1);
						continueParse = false;
						break;
					case DtdInputState.InsideDoubleQuoted:
					case DtdInputState.InsideSingleQuoted:
					case DtdInputState.Comment:
						continue;
					default:
						throw NotWFError ("unexpected end of file at DTD.");
					}
					break;
				case -1:
					throw NotWFError ("unexpected end of file at DTD.");
				case '<':
					switch (State) {
					case DtdInputState.InsideDoubleQuoted:
					case DtdInputState.InsideSingleQuoted:
					case DtdInputState.Comment:
						continue;	// well-formed
					}
					int c = ReadValueChar ();
					switch (c) {
					case '?':
						stateStack.Push (DtdInputState.PI);
						break;
					case '!':
						switch (ReadValueChar ()) {
						case 'E':
							switch (ReadValueChar ()) {
							case 'L':
								ExpectAndAppend ("EMENT");
								stateStack.Push (DtdInputState.ElementDecl);
								break;
							case 'N':
								ExpectAndAppend ("TITY");
								stateStack.Push (DtdInputState.EntityDecl);
								break;
							default:
								throw NotWFError ("unexpected token '<!E'.");
							}
							break;
						case 'A':
							ExpectAndAppend ("TTLIST");
							stateStack.Push (DtdInputState.AttlistDecl);
							break;
						case 'N':
							ExpectAndAppend ("OTATION");
							stateStack.Push (DtdInputState.NotationDecl);
							break;
						case '-':
							ExpectAndAppend ("-");
							stateStack.Push (DtdInputState.Comment);
							break;
						}
						break;
					default:
						throw NotWFError (String.Format ("unexpected '<{0}'.", (char) c));
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
						throw NotWFError ("unexpected token '>'");
					}
					break;
				case '?':
					if (State == DtdInputState.PI) {
						if (ReadValueChar () == '>')
							stateStack.Pop ();
					}
					break;
				case '-':
					if (State == DtdInputState.Comment) {
						if (PeekChar () == '-') {
							ReadValueChar ();
							ExpectAndAppend (">");
							stateStack.Pop ();
						}
					}
					break;
				case '%':
					if (State != DtdInputState.Free && State != DtdInputState.EntityDecl && State != DtdInputState.Comment && State != DtdInputState.InsideDoubleQuoted && State != DtdInputState.InsideSingleQuoted)
						throw NotWFError ("Parameter Entity Reference cannot appear as a part of markupdecl (see XML spec 2.8).");
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
			return CreateValueString ();
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
					throw NotWFError (String.Format ("character '{0}' not allowed for PUBLIC ID", (char)c ));
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
#if !USE_NAME_BUFFER
			bool savePreserve = preserveCurrentTag;
			preserveCurrentTag = true;

			int startOffset = peekCharsIndex - curNodePeekIndex;
			int ch = PeekChar ();
			if (!XmlChar.IsFirstNameChar (ch))
				throw NotWFError (String.Format (CultureInfo.InvariantCulture, "a name did not start with a legal character {0} ({1})", ch, (char) ch));
			Advance (ch);
			int length = 1;
			int colonAt = -1;

			while (XmlChar.IsNameChar ((ch = PeekChar ()))) {
				Advance (ch);
				if (ch == ':' && namespaces && colonAt < 0)
					colonAt = length;
				length++;
			}

			int start = curNodePeekIndex + startOffset;

			string name = NameTable.Add (
				peekChars, start, length);

			if (colonAt > 0) {
				prefix = NameTable.Add (
					peekChars, start, colonAt);
				localName = NameTable.Add (
					peekChars, start + colonAt + 1, length - colonAt - 1);
			} else {
				prefix = String.Empty;
				localName = name;
			}

			preserveCurrentTag = savePreserve;

			return name;
#else
			int ch = PeekChar ();
			if (!XmlChar.IsFirstNameChar (ch))
				throw NotWFError (String.Format (CultureInfo.InvariantCulture, "a name did not start with a legal character {0} ({1})", ch, (char) ch));

			nameLength = 0;

			Advance (ch);
			// AppendNameChar (ch);
			{
				// nameBuffer.Length is always non-0 so no need to ExpandNameCapacity () here
				if (ch <= Char.MaxValue)
					nameBuffer [nameLength++] = (char) ch;
				else
					AppendSurrogatePairNameChar (ch);
			}

			int colonAt = -1;

			while (XmlChar.IsNameChar ((ch = PeekChar ()))) {
				Advance (ch);

				if (ch == ':' && namespaces && colonAt < 0)
					colonAt = nameLength;
				// AppendNameChar (ch);
				{
					if (nameLength == nameCapacity)
						ExpandNameCapacity ();
					if (ch <= Char.MaxValue)
						nameBuffer [nameLength++] = (char) ch;
					else
						AppendSurrogatePairNameChar (ch);
				}
			}

			string name = NameTable.Add (nameBuffer, 0, nameLength);

			if (colonAt > 0) {
				prefix = NameTable.Add (nameBuffer, 0, colonAt);
				localName = NameTable.Add (nameBuffer, colonAt + 1, nameLength - colonAt - 1);
			} else {
				prefix = String.Empty;
				localName = name;
			}

			return name;
#endif
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
						ch < 0 ? (object) "EOF" : (char) ch,
						ch));
			}
		}

		private void Expect (string expected)
		{
			for (int i = 0; i < expected.Length; i++)
				if (ReadChar () != expected [i])
					throw NotWFError (String.Format (CultureInfo.InvariantCulture, 
						"'{0}' is expected", expected));
		}

		private void ExpectAfterWhitespace (char c)
		{
			while (true) {
				int i = ReadChar ();
				if (i < 0x21 && XmlChar.IsWhitespace (i))
					continue;
				if (c != i)
					throw NotWFError (String.Format (CultureInfo.InvariantCulture, "Expected {0}, but found {1} [{2}]", c, i < 0 ? (object) "EOF" : (char) i, i));
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
			Advance (ch);
			// FIXME: It should be inlined by the JIT.
//			while (XmlChar.IsWhitespace (PeekChar ()))
//				ReadChar ();
			while ((ch = PeekChar ()) == 0x20 || ch == 0x9 || ch == 0xA || ch == 0xD)
				Advance (ch);
			return skipped;
		}

		private bool ReadWhitespace ()
		{
			if (currentState == XmlNodeType.None)
				currentState = XmlNodeType.XmlDeclaration;

			bool savePreserve = preserveCurrentTag;
			preserveCurrentTag = true;
			int startOffset = peekCharsIndex - curNodePeekIndex; // it should be 0 for now though.

			int ch = PeekChar ();
			do {
				Advance (ch);
				ch = PeekChar ();
			// FIXME: It should be inlined by the JIT.
//			} while ((ch = PeekChar ()) != -1 && XmlChar.IsWhitespace (ch));
			} while (ch == 0x20 || ch == 0x9 || ch == 0xA || ch == 0xD);

			bool isText = currentState == XmlNodeType.Element && ch != -1 && ch != '<';

			if (!isText && (whitespaceHandling == WhitespaceHandling.None ||
				    whitespaceHandling == WhitespaceHandling.Significant && XmlSpace != XmlSpace.Preserve))
				return false;

			ClearValueBuffer ();
			valueBuffer.Append (peekChars, curNodePeekIndex, peekCharsIndex - curNodePeekIndex - startOffset);
			preserveCurrentTag = savePreserve;

			if (isText) {
				ReadText (false);
			} else {
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

			return true;
		}

		// Returns -1 if it should throw an error.
		private int ReadCharsInternal (char [] buffer, int offset, int length)
		{
			int bufIndex = offset;
			for (int i = 0; i < length; i++) {
				int c = PeekChar ();
				switch (c) {
				case -1:
					throw NotWFError ("Unexpected end of xml.");
				case '<':
					if (i + 1 == length)
						// if it does not end here,
						// it cannot store another
						// character, so stop here.
						return i;
					Advance (c);
					if (PeekChar () != '/') {
						nestLevel++;
						buffer [bufIndex++] = '<';
						continue;
					}
					else if (nestLevel-- > 0) {
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
					readCharsInProgress = false;
					Read (); // move to the next node
					return i;
				default:
					Advance (c);
					if (c <= Char.MaxValue)
						buffer [bufIndex++] = (char) c;
					else {
						buffer [bufIndex++] = (char) ((c - 0x10000) / 0x400 + 0xD800);
						buffer [bufIndex++] = (char) ((c - 0x10000) % 0x400 + 0xDC00);
					}
					break;
				}
			}
			return length;
		}

		private bool ReadUntilEndTag ()
		{
			if (Depth == 0)
				currentState = XmlNodeType.EndElement;
			int ch;
			do {
				ch = ReadChar ();
				switch (ch) {
				case -1:
					throw NotWFError ("Unexpected end of xml.");
				case '<':
					if (PeekChar () != '/') {
						nestLevel++;
						continue;
					}
					else if (--nestLevel > 0)
						continue;
					ReadChar ();
					string name = ReadName ();
					if (name != elementNames [elementNameStackPos - 1].Name)
						continue;
					Expect ('>');
					depth--;
					return Read ();
				}
			} while (true);
		}
		#endregion
	}
}
