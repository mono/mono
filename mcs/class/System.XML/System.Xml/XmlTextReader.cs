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
					return currentInput.LineNumber;
				else
					return cursorToken.LineNumber;
			}
		}

		public int LinePosition
		{
			get {
				if (useProceedingLineInfo)
					return currentInput.LinePosition;
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
			foreach (XmlParserInput input in parserInputStack.ToArray ())
				input.Close ();
			this.currentInput.Close ();

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

		[MonoTODO]
		public TextReader GetRemainder ()
		{
			throw new NotImplementedException ();
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
			currentLinkedNodeLineNumber = currentInput.LineNumber;
			currentLinkedNodeLinePosition = currentInput.LinePosition;
			useProceedingLineInfo = true;

			cursorToken = currentToken;
			attributeCount = 0;
			currentAttribute = currentAttributeValue = -1;
			currentToken.Clear ();

			// It was moved from end of ReadStartTag ().
			if (depthUp)
				++depth;
			depthUp = false;

			more = ReadContent ();

			if (depth == 0 && !allowMultipleRoot && (IsEmptyElement || NodeType == XmlNodeType.EndElement))
				currentState = XmlNodeType.EndElement;
			if (maybeTextDecl != 0)
				maybeTextDecl--;

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

		[MonoTODO]
		public int ReadBase64 (byte [] buffer, int offset, int length)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int ReadBinHex (byte [] buffer, int offset, int length)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int ReadChars (char [] buffer, int offset, int length)
		{
			throw new NotImplementedException ();
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

		internal bool MaybeTextDecl {
			set { if (value) this.maybeTextDecl = 2; }
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

		private XmlParserInput currentInput;
		private Stack parserInputStack;
		private ReadState readState;

		private int depth;
		private int elementDepth;
		private bool depthUp;

		private bool popScope;
		private Stack elementStack;
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

		private StringBuilder valueBuffer;

		private int currentLinkedNodeLineNumber;
		private int currentLinkedNodeLinePosition;
		private bool useProceedingLineInfo;

		// A buffer for ReadContent for ReadOuterXml
		private StringBuilder currentTag {
			get {
				return currentInput.CurrentMarkup;
			}
		}

		// Parameter entity placeholder
		private Hashtable parameterEntities;
		private int dtdIncludeSect;

		private XmlNodeType startNodeType;
		// State machine attribute.
		//	XmlDeclaration: after the first node.
		//	DocumentType: after doctypedecl
		//	Element: inside document element
		//	EndElement: after document element
		private XmlNodeType currentState;
		private int maybeTextDecl;

		private XmlResolver resolver = new XmlUrlResolver ();

		// These values are never re-initialized.
		private bool namespaces = true;
		private WhitespaceHandling whitespaceHandling = WhitespaceHandling.All;
		private bool normalization = false;

		private void Init ()
		{
			readState = ReadState.Initial;
			currentState = XmlNodeType.None;
			maybeTextDecl = 0;
			allowMultipleRoot = false;

			depth = 0;
			depthUp = false;

			popScope = false;
			parserInputStack = new Stack ();
			elementStack = new Stack();
			currentAttribute = -1;
			currentAttributeValue = -1;

			returnEntityReference = false;
			entityReferenceName = String.Empty;

			nameBuffer = new char [initialNameCapacity];
			nameLength = 0;
			nameCapacity = initialNameCapacity;
			
			valueBuffer = new StringBuilder (512);
			parameterEntities = new Hashtable ();

			currentToken = new XmlTokenInfo (this);
			cursorToken = currentToken;
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

			this.currentInput = new XmlParserInput (fragment, url);
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

		private int PeekChar ()
		{
			return currentInput.PeekChar ();
		}

		private int ReadChar ()
		{
			return currentInput.ReadChar ();
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
					if (depth > 0)
						throw new XmlException ("unexpected end of file. Current depth is " + depth);
					readState = ReadState.EndOfFile;
					SetProperties (
						XmlNodeType.None, // nodeType
						String.Empty, // name
						false, // isEmptyElement
						(string) null, // value
						true // clearAttributes
					);
					return false;
				default:
					ReadText (true);
					break;
				}
			}
			if (NodeType == XmlNodeType.XmlDeclaration && maybeTextDecl == 1)
				return ReadContent ();
			return this.ReadState != ReadState.EndOfFile;
		}

		private void SetEntityReferenceProperties ()
		{
/*
			if (resolver != null) {
				if (DTD == null)
					throw new XmlException (this as IXmlLineInfo,
						"Entity reference is not allowed without document type declaration.");
				else if((!DTD.InternalSubsetHasPEReference || isStandalone) &&
					DTD.EntityDecls [entityReferenceName] == null)
					throw new XmlException (this as IXmlLineInfo,
						"Required entity declaration for '" + entityReferenceName + "' was not found.");
				string dummy = DTD.EntityDecls [entityReferenceName].EntityValue;
			}
*/
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
				currentAttributeToken.LineNumber = currentInput.LineNumber;
				currentAttributeToken.LinePosition = currentInput.LinePosition;

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
			currentAttributeValueToken.LineNumber = currentInput.LineNumber;
			currentAttributeValueToken.LinePosition = currentInput.LinePosition;

			bool incrementToken = false;
			bool isNewToken = true;
			bool loop = true;
			while (loop && PeekChar () != quoteChar) {
				if (incrementToken) {
					IncrementAttributeValueToken ();
					currentAttributeValueToken.LineNumber = currentInput.LineNumber;
					currentAttributeValueToken.LinePosition = currentInput.LinePosition;
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
						if (entDecl != null && entDecl.SystemId != null)
//						if (!startNodeType == XmlNodeType.Attribute && (entDecl == null || entDecl.SystemId != null))
							throw new XmlException (this as IXmlLineInfo,
								"Reference to external entities is not allowed in the value of an attribute.");
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
				currentAttributeToken.ValueTokenEndIndex = currentAttributeValue;
			}

			if (dummyQuoteChar < 0)
				ReadChar (); // quoteChar
		}

		// The reader is positioned on the quote character.
		// *Keeps quote char* to value to get_QuoteChar() correctly.
		// Not it is used only for DTD.
		private string ReadAttribute (bool isDefaultValue)
		{
			ClearValueBuffer ();

			int quoteChar = ReadChar ();

			if (quoteChar != '\'' && quoteChar != '\"')
				throw new XmlException (this as IXmlLineInfo,"an attribute value was not quoted");

			AppendValueChar (quoteChar);

			while (PeekChar () != quoteChar) {
				int ch = ReadChar ();

				switch (ch)
				{
				case '<':
					throw new XmlException (this as IXmlLineInfo,"attribute values cannot contain '<'");
				case -1:
					throw new XmlException (this as IXmlLineInfo,"unexpected end of file in an attribute value");
/*
				case '&':
					if (isDefaultValue) {
						AppendValueChar (ch);
						break;
					}
					AppendValueChar (ch);
					if (PeekChar () == '#')
						break;
					// Check XML 1.0 section 3.1 WFC.
					string entName = ReadName ();
					Expect (';');
					if (XmlChar.GetPredefinedEntity (entName) == 0) {
						DTDEntityDeclaration entDecl = 
							DTD == null ? null : DTD.EntityDecls [entName];
						if (entDecl == null || entDecl.SystemId != null)
							throw new XmlException (this as IXmlLineInfo,
								"Reference to external entities is not allowed in attribute value.");
					}
					valueBuffer.Append (entName);
					AppendValueChar (';');
					break;
*/
				default:
					AppendValueChar (ch);
					break;
				}
			}

			ReadChar (); // quoteChar
			AppendValueChar (quoteChar);

			return CreateValueString ();
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
				if (maybeTextDecl == 0)
					throw new XmlException (this as IXmlLineInfo,
						"XML declaration cannot appear in this state.");
			}
			// Is this required?
			if (maybeTextDecl != 0)
				currentState = XmlNodeType.XmlDeclaration;

			ClearAttributes ();

			ReadAttributes (true);	// They must have "version."
			string version = GetAttribute ("version");

			string message = null;
			if (parserInputStack.Count == 0) {
				if (maybeTextDecl == 0 && (attributeTokens [0].Name != "version" || version != "1.0"))
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
			} else {
				int currentCheck = 0;
				if (attributeTokens [0].Name == "version") {
					if (version != "1.0")
						message = "Version 1.0 declaration is required in Text Declaration.";
					currentCheck = 1;
				}
				if (attributeCount <= currentCheck || attributeTokens [currentCheck].Name != "encoding")
					message = "Invalid Text Declaration markup was found. encoding specification is required.";
			}
			if (message != null)
				throw new XmlException (this as IXmlLineInfo, message);

			Expect ("?>");

			if (maybeTextDecl != 0)
				if (this ["standalone"] != null)
					throw new XmlException (this as IXmlLineInfo,
						"Invalid text declaration.");
			if (maybeTextDecl == 2)
				maybeTextDecl = 1;

			SetProperties (
				XmlNodeType.XmlDeclaration, // nodeType
				"xml", // name
				false, // isEmptyElement
				currentInput.CurrentMarkup.ToString (6, currentInput.CurrentMarkup.Length - 6), // value
				false // clearAttributes
			);
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
			parserContext.Dtd = new DTDObjectModel ();	// merges both internal and external subsets in the meantime,
			DTD.BaseURI = BaseURI;
			DTD.Name = name;
			DTD.PublicId = publicId;
			DTD.SystemId = systemId;
			DTD.InternalSubset = internalSubset;
			DTD.XmlResolver = resolver;
			int originalParserDepth = parserInputStack.Count;
			bool more;
			if (internalSubset != null && internalSubset.Length > 0) {
				XmlParserInput original = currentInput;
				currentInput = new XmlParserInput (new StringReader (internalSubset), BaseURI, intSubsetStartLine, intSubsetStartColumn);
				do {
					more = CompileDTDSubset ();
					if (PeekChar () == -1 && parserInputStack.Count > 0)
						PopParserInput ();
				} while (more || parserInputStack.Count > originalParserDepth);
				if (dtdIncludeSect != 0)
					throw new XmlException (this as IXmlLineInfo,"INCLUDE section is not ended correctly.");
				currentInput = original;
			}
			if (systemId != null && systemId != String.Empty && resolver != null) {
				PushParserInput (systemId);
				do {
					more = this.CompileDTDSubset ();
					if (PeekChar () == -1 && parserInputStack.Count > 1)
						PopParserInput ();
				} while (more || parserInputStack.Count > originalParserDepth + 1);
				PopParserInput ();
			}

			return DTD;
		}

		private void PushParserInput (string url)
		{
			Uri baseUri = null;
			try {
				baseUri = new Uri (BaseURI);
			} catch (UriFormatException) {
			}

			Uri absUri = resolver.ResolveUri (baseUri, url);
			string absPath = absUri.ToString ();

			foreach (XmlParserInput i in parserInputStack.ToArray ()) {
				if (i.BaseURI == absPath)
					throw new XmlException (this as IXmlLineInfo, "Nested inclusion is not allowed: " + url);
			}
			parserInputStack.Push (currentInput);
			currentInput = new XmlParserInput (new XmlStreamReader (url, false, resolver, BaseURI), absPath);
			parserContext.PushScope ();
			parserContext.BaseURI = absPath;

			maybeTextDecl = 2;
		}

		private void PopParserInput ()
		{
			currentInput = parserInputStack.Pop () as XmlParserInput;
			parserContext.PopScope ();
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

		// Read any one of following:
		//   elementdecl, AttlistDecl, EntityDecl, NotationDecl,
		//   PI, Comment, Parameter Entity, or doctype termination char(']')
		//
		// Returns true if it may have any more contents, or false if not.
		private bool CompileDTDSubset()
		{
			SkipWhitespace ();
			switch(PeekChar ())
			{
			case -1:
				return false;
			case '%':
				// It affects on entity references' well-formedness
				if (this.parserInputStack.Count == 0)
					DTD.InternalSubsetHasPEReference = true;
				ReadChar ();
				string peName = ReadName ();
				Expect (';');
				currentInput.InsertParameterEntityBuffer (GetPEValue (peName));
				int currentLine = currentInput.LineNumber;
				int currentColumn = currentInput.LinePosition;
				while (currentInput.HasPEBuffer)
					CompileDTDSubset ();
				if (currentInput.LineNumber != currentLine ||
					currentInput.LinePosition != currentColumn)
					throw new XmlException (this as IXmlLineInfo,
						"Incorrectly nested parameter entity.");
				break;
			case '<':
				ReadChar ();
				switch(ReadChar ())
				{
				case '?':
					// Only read, no store.
					ReadProcessingInstruction ();
					break;
				case '!':
					CompileDeclaration ();
					break;
				default:
					throw new XmlException (this as IXmlLineInfo,"Syntax Error after '<' character.");
				}
				break;
			case ']':
				if (dtdIncludeSect == 0)
					throw new XmlException (this as IXmlLineInfo, "Unbalanced end of INCLUDE/IGNORE section.");
				// End of inclusion
				Expect ("]]>");
				dtdIncludeSect--;
				SkipWhitespace ();
				return false;
			default:
				throw new XmlException (this as IXmlLineInfo,String.Format ("Syntax Error inside doctypedecl markup : {0}({1})", PeekChar (), (char) PeekChar ()));
			}
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
						throw new XmlException (this as IXmlLineInfo,
							"Whitespace is required after '<!ENTITY' in DTD entity declaration.");
					LOOPBACK:
					if (PeekChar () == '%') {
						ReadChar ();
						if (!SkipWhitespace ()) {
							ImportAsPERef ();
							goto LOOPBACK;
						} else {
							TryExpandPERef ();
							SkipWhitespace ();
							if (XmlChar.IsNameChar (PeekChar ()))
								ReadParameterEntityDecl ();
							else
								throw new XmlException (this as IXmlLineInfo,"expected name character");
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
					throw new XmlException (this as IXmlLineInfo,"Syntax Error after '<!E' (ELEMENT or ENTITY must be found)");
				}
				break;
			case 'A':
				Expect ("TTLIST");
				DTDAttListDeclaration atl = ReadAttListDecl ();
//				if (DTD.AttListDecls.ContainsKey (atl.Name))
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
				SkipWhitespace ();
				Expect ('I');
				switch (ReadChar ()) {
				case 'N':
					Expect ("CLUDE");
					SkipWhitespace ();
					Expect ('[');
					dtdIncludeSect++;
					break;
				case 'G':
					Expect ("NORE");
					ReadIgnoreSect ();
					break;
				}
				break;
			default:
				throw new XmlException (this as IXmlLineInfo,"Syntax Error after '<!' characters.");
			}
		}

		private void ReadIgnoreSect ()
		{
			bool skip = false;
			SkipWhitespace ();
			Expect ('[');
			int dtdIgnoreSect = 1;
			while (dtdIgnoreSect > 0) {
				switch (skip ? PeekChar () : ReadChar ()) {
				case -1:
					throw new XmlException (this as IXmlLineInfo,"Unexpected IGNORE section end.");
				case '<':
					if (ReadChar () == '!' && ReadChar () == '[')
						dtdIgnoreSect++;
					break;
				case ']':
					if (ReadChar () == ']') {
						if (ReadChar () == '>')
							dtdIgnoreSect--;
						else
							skip = true;
					}
					break;
				}
				skip = false;
			}
		}

		// The reader is positioned on the head of the name.
		private DTDElementDeclaration ReadElementDecl ()
		{
			DTDElementDeclaration decl = new DTDElementDeclaration (DTD);
			if (!SkipWhitespace ())
				throw new XmlException (this as IXmlLineInfo,
					"Whitespace is required between '<!ELEMENT' and name in DTD element declaration.");
			TryExpandPERef ();
			SkipWhitespace ();
			decl.Name = ReadName ();
			if (!SkipWhitespace ())
				throw new XmlException (this as IXmlLineInfo,
					"Whitespace is required between name and content in DTD element declaration.");
			TryExpandPERef ();
			ReadContentSpec (decl);
			SkipWhitespace ();
			// This expanding is only allowed as a non-validating parser.
			TryExpandPERef ();
			SkipWhitespace ();
			Expect ('>');
			return decl;
		}

		// read 'children'(BNF) of contentspec
		private void ReadContentSpec (DTDElementDeclaration decl)
		{
			TryExpandPERef ();
			SkipWhitespace ();
			switch(PeekChar ())
			{
			case 'E':
				decl.IsEmpty = true;
				Expect ("EMPTY");
				break;
			case 'A':
				decl.IsAny = true;
				Expect ("ANY");
				break;
			case '(':
				DTDContentModel model = decl.ContentModel;
				ReadChar ();
				SkipWhitespace ();
				TryExpandPERef ();
				SkipWhitespace ();
				if(PeekChar () == '#') {
					// Mixed Contents. "#PCDATA" must appear first.
					decl.IsMixedContent = true;
					model.Occurence = DTDOccurence.ZeroOrMore;
					model.OrderType = DTDContentOrderType.Or;
					Expect ("#PCDATA");
					SkipWhitespace ();
					TryExpandPERef ();
					SkipWhitespace ();
					while(PeekChar () != ')') {
						Expect('|');
						SkipWhitespace ();
						TryExpandPERef ();
						SkipWhitespace ();
						DTDContentModel elem = new DTDContentModel (DTD, decl.Name);
						elem.ElementName = ReadName ();
						model.ChildModels.Add (elem);
						SkipWhitespace ();
						TryExpandPERef ();
						SkipWhitespace ();
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
						TryExpandPERef ();
						SkipWhitespace ();
						if(PeekChar ()=='|') {
							// CPType=Or
							if (model.OrderType == DTDContentOrderType.Seq)
								throw new XmlException (this as IXmlLineInfo,
									"Inconsistent choice markup in sequence cp.");
							model.OrderType = DTDContentOrderType.Or;
							ReadChar ();
							SkipWhitespace ();
							model.ChildModels.Add (ReadCP (decl));
							SkipWhitespace ();
						}
						else if(PeekChar () == ',')
						{
							// CPType=Seq
							if (model.OrderType == DTDContentOrderType.Or)
								throw new XmlException (this as IXmlLineInfo,
									"Inconsistent sequence markup in choice cp.");
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
			}
		}

		// Read 'cp' (BNF) of contentdecl (BNF)
		private DTDContentModel ReadCP (DTDElementDeclaration elem)
		{
			DTDContentModel model = null;
			TryExpandPERef ();
			SkipWhitespace ();
			if(PeekChar () == '(') {
				model = new DTDContentModel (DTD, elem.Name);
				ReadChar ();
				SkipWhitespace ();
				model.ChildModels.Add (ReadCP (elem));
				SkipWhitespace ();
				do {
					TryExpandPERef ();
					SkipWhitespace ();
					if(PeekChar ()=='|') {
						// CPType=Or
						if (model.OrderType == DTDContentOrderType.Seq)
							throw new XmlException (this as IXmlLineInfo,
								"Inconsistent choice markup in sequence cp.");
						model.OrderType = DTDContentOrderType.Or;
						ReadChar ();
						SkipWhitespace ();
						model.ChildModels.Add (ReadCP (elem));
						SkipWhitespace ();
					}
					else if(PeekChar () == ',') {
						// CPType=Seq
						if (model.OrderType == DTDContentOrderType.Or)
							throw new XmlException (this as IXmlLineInfo,
								"Inconsistent sequence markup in choice cp.");
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
				SkipWhitespace ();
				Expect (')');
			}
			else {
				TryExpandPERef ();
				model = new DTDContentModel (DTD, elem.Name);
				SkipWhitespace ();
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

		// The reader is positioned on the first name char.
		private void ReadParameterEntityDecl ()
		{
			DTDParameterEntityDeclaration decl = 
				new DTDParameterEntityDeclaration();
			decl.BaseURI = BaseURI;

			decl.Name = ReadName ();
			if (!SkipWhitespace ())
				throw new XmlException (this as IXmlLineInfo,
					"Whitespace is required after name in DTD parameter entity declaration.");

			if (PeekChar () == 'S' || PeekChar () == 'P') {
//				throw new NotImplementedException ("External parameter entity reference is not implemented yet.");
				// read publicId/systemId
				ReadExternalID ();
				decl.PublicId = GetAttribute ("PUBLIC");
				decl.SystemId = GetAttribute ("SYSTEM");
				SkipWhitespace ();
				decl.Resolve (resolver);
			}
			else {
				TryExpandPERef ();
				int quoteChar = ReadChar ();
				int start = currentTag.Length;
				ClearValueBuffer ();
				bool loop = true;
				while (loop) {
					int c = PeekChar ();
					switch (c) {
					case -1:
						throw new XmlException ("unexpected end of stream in entity value definition.");
					case '"':
						ReadChar ();
						if (quoteChar == '"')
							loop = false;
						else
							AppendValueChar ('"');
						break;
					case '\'':
						ReadChar ();
						if (quoteChar == '\'')
							loop = false;
						else
							AppendValueChar ('\'');
						break;
					case '&':
						ReadChar ();
						if (PeekChar () == '#') {
							ReadChar ();
							ReadCharacterReference ();
						}
						else
							AppendValueChar ('&');
						break;
					case '%':
						ReadChar ();
						string peName = ReadName ();
						Expect (';');
						valueBuffer.Append (GetPEValue (peName));
						break;
					default:
						AppendValueChar (ReadChar ());
						break;
					}
				}
				decl.LiteralValue = CreateValueString (); // currentTag.ToString (start, currentTag.Length - start - 1);
				ClearValueBuffer ();
			}
			SkipWhitespace ();
			Expect ('>');
			if (parameterEntities [decl.Name] == null) {
                                parameterEntities.Add (decl.Name, decl);
			}
		}

		private string GetPEValue (string peName)
		{
			DTDParameterEntityDeclaration peDecl =
				this.parameterEntities [peName] as DTDParameterEntityDeclaration;
			if (peDecl != null)
				return peDecl.Value;
			// See XML 1.0 section 4.1 for both WFC and VC.
			if ((DTD.SystemId == null && !DTD.InternalSubsetHasPEReference) || this.isStandalone)
				throw new XmlException (this as IXmlLineInfo,
					"Parameter entity " + peName + " not found.");
			DTD.AddError (new XmlSchemaException (
				"Parameter entity " + peName + " not found.", null));
			return "";
		}

		private void TryExpandPERef ()
		{
			if (PeekChar () == '%') {
//				ReadChar ();
//				if (!XmlChar.IsNameChar (PeekChar ()))
//					return;
//				ExpandPERef ();
				ImportAsPERef ();
			}
		}

		// reader is positioned on '%'
		private void ImportAsPERef ()
		{
			ReadChar ();
			string peName = ReadName ();
			Expect (';');
			DTDParameterEntityDeclaration peDecl =
				this.parameterEntities [peName] as DTDParameterEntityDeclaration;
			if (peDecl == null) {
				DTD.AddError (new XmlSchemaException ("Parameter entity " + peName + " not found.", null));
				return;	// do nothing
			}
			currentInput.InsertParameterEntityBuffer (" " + peDecl.Value + " ");

		}

		// The reader is positioned on the head of the name.
		private DTDEntityDeclaration ReadEntityDecl ()
		{
			DTDEntityDeclaration decl = new DTDEntityDeclaration (DTD);
			decl.IsInternalSubset = (parserInputStack.Count == 0);
			TryExpandPERef ();
			SkipWhitespace ();
			decl.Name = ReadName ();
			if (!SkipWhitespace ())
				throw new XmlException (this as IXmlLineInfo,
					"Whitespace is required between name and content in DTD entity declaration.");
			TryExpandPERef ();
			SkipWhitespace ();

			if (PeekChar () == 'S' || PeekChar () == 'P') {
				// external entity
				ReadExternalID ();
				decl.PublicId = GetAttribute ("PUBLIC");
				decl.SystemId = GetAttribute ("SYSTEM");
				if (SkipWhitespace ()) {
					if (PeekChar () == 'N') {
						// NDataDecl
						Expect ("NDATA");
						if (!SkipWhitespace ())
							throw new XmlException (this as IXmlLineInfo,
								"Whitespace is required after NDATA.");
						decl.NotationName = ReadName ();	// ndata_name
					}
				}
				decl.ScanEntityValue (new StringCollection ());
			}
			else {
				// literal entity
				ReadEntityValueDecl (decl);
			}
			SkipWhitespace ();
			// This expanding is only allowed as a non-validating parser.
			TryExpandPERef ();
			SkipWhitespace ();
			Expect ('>');
			return decl;
		}

		private void ReadEntityValueDecl (DTDEntityDeclaration decl)
		{
			SkipWhitespace ();
			// quotation char will be finally removed on unescaping
			int quoteChar = ReadChar ();
			int start = currentTag.Length;
			if (quoteChar != '\'' && quoteChar != '"')
				throw new XmlException ("quotation char was expected.");

			ClearValueBuffer ();

			while (PeekChar () != quoteChar) {
				switch (PeekChar ()) {
				case '%':
					ReadChar ();
					string name = ReadName ();
					Expect (';');
					if (decl.IsInternalSubset)
						throw new XmlException (this as IXmlLineInfo,
							"Parameter entity is not allowed in internal subset entity '" + name + "'");
					valueBuffer.Append (GetPEValue (name));
					break;
				case -1:
					throw new XmlException ("unexpected end of stream.");
				default:
					AppendValueChar (ReadChar ());
					break;
				}
			}
			string value = Dereference (CreateValueString (), false);
			ClearValueBuffer ();

			Expect (quoteChar);
			decl.LiteralEntityValue = value;
		}

		private DTDAttListDeclaration ReadAttListDecl ()
		{
			SkipWhitespace ();
			TryExpandPERef ();
			SkipWhitespace ();
			string name = ReadName ();	// target element name
			DTDAttListDeclaration decl =
				DTD.AttListDecls [name] as DTDAttListDeclaration;
			if (decl == null)
				decl = new DTDAttListDeclaration (DTD);
			decl.Name = name;

			if (!SkipWhitespace ())
				if (PeekChar () != '>')
					throw new XmlException (this as IXmlLineInfo,
						"Whitespace is required between name and content in non-empty DTD attlist declaration.");

			TryExpandPERef ();
			SkipWhitespace ();

			while (XmlChar.IsNameChar ((char) PeekChar ())) {
				DTDAttributeDefinition def = ReadAttributeDefinition ();
				if (decl [def.Name] == null)
					decl.Add (def);
				SkipWhitespace ();
				TryExpandPERef ();
				SkipWhitespace ();
			}
			SkipWhitespace ();
			// This expanding is only allowed as a non-validating parser.
			TryExpandPERef ();
			SkipWhitespace ();
			Expect ('>');
			return decl;
		}

		private DTDAttributeDefinition ReadAttributeDefinition ()
		{
			DTDAttributeDefinition def = new DTDAttributeDefinition ();

			// attr_name
			TryExpandPERef ();
			SkipWhitespace ();
			def.Name = ReadName ();
			if (!SkipWhitespace ())
				throw new XmlException (this as IXmlLineInfo,
					"Whitespace is required between name and content in DTD attribute definition.");

			// attr_value
			TryExpandPERef ();
			SkipWhitespace ();
			switch(PeekChar ()) {
			case 'C':	// CDATA
				Expect ("CDATA");
				def.Datatype = XmlSchemaDatatype.FromName ("normalizedString");
				break;
			case 'I':	// ID, IDREF, IDREFS
				Expect ("ID");
				if(PeekChar () == 'R') {
					Expect ("REF");
					if(PeekChar () == 'S') {
						// IDREFS
						ReadChar ();
						def.Datatype = XmlSchemaDatatype.FromName ("IDREFS");
					}
					else	// IDREF
						def.Datatype = XmlSchemaDatatype.FromName ("IDREF");
				}
				else	// ID
					def.Datatype = XmlSchemaDatatype.FromName ("ID");
				break;
			case 'E':	// ENTITY, ENTITIES
				Expect ("ENTIT");
				switch(ReadChar ()) {
					case 'Y':	// ENTITY
						def.Datatype = XmlSchemaDatatype.FromName ("ENTITY");
						break;
					case 'I':	// ENTITIES
						Expect ("ES");
						def.Datatype = XmlSchemaDatatype.FromName ("ENTITIES");
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
						def.Datatype = XmlSchemaDatatype.FromName ("NMTOKENS");
					}
					else	// NMTOKEN
						def.Datatype = XmlSchemaDatatype.FromName ("NMTOKEN");
					break;
				case 'O':
					Expect ("OTATION");
					def.Datatype = XmlSchemaDatatype.FromName ("NOTATION");
					if (!SkipWhitespace ())
						throw new XmlException (this as IXmlLineInfo,
							"Whitespace is required between name and content in DTD attribute definition.");
					Expect ('(');
					SkipWhitespace ();
					def.EnumeratedNotations.Add (ReadName ());		// notation name
					SkipWhitespace ();
					while(PeekChar () == '|') {
						ReadChar ();
						SkipWhitespace ();
						def.EnumeratedNotations.Add (ReadName ());	// notation name
						SkipWhitespace ();
					}
					Expect (')');
					break;
				default:
					throw new XmlException ("attribute declaration syntax error.");
				}
				break;
			default:	// Enumerated Values
				def.Datatype = XmlSchemaDatatype.FromName ("NMTOKEN");
				TryExpandPERef ();
				SkipWhitespace ();
				Expect ('(');
				SkipWhitespace ();
				def.EnumeratedAttributeDeclaration.Add (
					def.Datatype.Normalize (ReadNmToken ()));	// enum value
				SkipWhitespace ();
				while(PeekChar () == '|') {
					ReadChar ();
					SkipWhitespace ();
					def.EnumeratedAttributeDeclaration.Add (
						def.Datatype.Normalize (ReadNmToken ()));	// enum value
					SkipWhitespace ();
				}
				Expect (')');
				break;
			}
			TryExpandPERef ();
			if (!SkipWhitespace ())
				throw new XmlException (this as IXmlLineInfo,
					"Whitespace is required between type and occurence in DTD attribute definition.");

			// def_value
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
						throw new XmlException (this as IXmlLineInfo,
							"Whitespace is required between FIXED and actual value in DTD attribute definition.");
					def.UnresolvedDefaultValue = ReadAttribute (true);
					break;
				}
			} else {
				// one of the enumerated value
				TryExpandPERef ();
				SkipWhitespace ();
				def.UnresolvedDefaultValue = ReadAttribute (true);
			}

			return def;
		}

		private DTDNotationDeclaration ReadNotationDecl()
		{
			DTDNotationDeclaration decl = new DTDNotationDeclaration ();
			TryExpandPERef ();
			SkipWhitespace ();
			decl.Name = ReadName ();	// notation name
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
				decl.Prefix = String.Empty;
				decl.LocalName = decl.Name;
			}

			SkipWhitespace ();
			if(PeekChar () == 'P') {
				decl.PublicId = ReadPubidLiteral ();
				bool wsSkipped = SkipWhitespace ();
				if (PeekChar () == '\'' || PeekChar () == '"') {
					if (!wsSkipped)
						throw new XmlException (this as IXmlLineInfo,
							"Whitespace is required between public id and system id.");
					decl.SystemId = ReadSystemLiteral (false);
					SkipWhitespace ();
				}
			} else if(PeekChar () == 'S') {
				decl.SystemId = ReadSystemLiteral (true);
				SkipWhitespace ();
			}
			if(decl.PublicId == null && decl.SystemId == null)
				throw new XmlException ("public or system declaration required for \"NOTATION\" declaration.");
			// This expanding is only allowed as a non-validating parser.
			TryExpandPERef ();
			SkipWhitespace ();
			Expect ('>');
			return decl;
		}

		private void ReadExternalID () {
			this.ClearAttributes ();
			switch (PeekChar ()) {
			case 'S':
				string systemId = ReadSystemLiteral (true);
				AddAttribute ("SYSTEM", systemId);
				break;
			case 'P':
				string publicId = ReadPubidLiteral ();
				if (!SkipWhitespace ())
					throw new XmlException (this as IXmlLineInfo,
						"Whitespace is required between PUBLIC id and SYSTEM id.");
				systemId = ReadSystemLiteral (false);
				AddAttribute ("PUBLIC", publicId);
				AddAttribute ("SYSTEM", systemId);
				break;
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

		private string Dereference (string unresolved, bool expandPredefined)
		{
			StringBuilder resolved = new StringBuilder();
			int pos = 0;
			int next = unresolved.IndexOf ('&');
			if(next < 0)
				return unresolved;

			while(next >= 0) {
				if(pos < next)
					resolved.Append (unresolved.Substring (pos, next - pos));// - 1);
				int endPos = unresolved.IndexOf (';', next+1);
				string entityName =
					unresolved.Substring (next + 1, endPos - next - 1);
				if(entityName [0] == '#') {
					char c;
					// character entity
					if(entityName [1] == 'x') {
						// hexadecimal
						c = (char) int.Parse ("0" + entityName.Substring (2),
							System.Globalization.NumberStyles.HexNumber);
					} else {
						// decimal
						c = (char) int.Parse (entityName.Substring (1));
					}
					resolved.Append (c);
				} else {
					char predefined = XmlChar.GetPredefinedEntity (entityName);
					if (expandPredefined && predefined != 0)
						resolved.Append (predefined);
					else
					// With respect to "Value", MS document is helpless
					// and the implemention returns inconsistent value
					// (e.g. XML: "&ent; &amp;ent;" ---> Value: "&ent; &ent;".)
						resolved.Append ("&" + entityName + ";");
				}
				pos = endPos + 1;
				if(pos > unresolved.Length)
					break;
				next = unresolved.IndexOf('&', pos);
			}
			resolved.Append (unresolved.Substring(pos));

			return resolved.ToString();
		}

		#endregion
	}
}
