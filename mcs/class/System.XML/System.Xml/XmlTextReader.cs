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
//   I haven't checked whether DTD parser runs correct.
//
//   More strict well-formedness checking should be done.
//
//   NameTables aren't being used completely yet.
//
//   Some thought needs to be given to performance. There's too many
//   strings being allocated.
//

using System;
using System.Collections;
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

		[MonoTODO("TODO as same as private XmlTextReader(TextReader, XmlNodeType, XmlParserContext)")]
		public XmlTextReader (string xmlFragment, XmlNodeType fragType, XmlParserContext context)
			: this (context != null ? context.BaseURI : String.Empty,
				new StringReader (xmlFragment),
				fragType,
				context)
		{
		}

		// TODO still remains as described at head of this file,
		// but it might not be TODO of the constructors...
		XmlTextReader (string url, TextReader fragment, XmlNodeType fragType, XmlParserContext context)
		{
			InitializeContext (url, context, fragment, fragType);
		}

		#endregion

		#region Properties

		public override int AttributeCount
		{
			get { return attributes.Count; }
		}

		public override string BaseURI
		{
			get { return parserContext.BaseURI; }
		}

		public override int Depth
		{
			get {
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
			get { return isEmptyElement; }
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
			get { return currentInput.LineNumber; }
		}

		public int LinePosition
		{
			get { return currentInput.LinePosition; }
		}

		public override string LocalName
		{
			get { return localName; }
		}

		public override string Name
		{
			get { return name; }
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
			get { return namespaceURI; }
		}

		public override XmlNameTable NameTable
		{
			get { return parserContext.NameTable; }
		}

		public override XmlNodeType NodeType
		{
			get { return nodeType; }
		}

		[MonoTODO]
		public bool Normalization
		{
			get { return normalization; }
			set { normalization = value; }
		}

		public override string Prefix
		{
			get { return prefix; }
		}

		public override char QuoteChar
		{
			get {
				// value string holds attribute quotation char.
				if (NodeType == XmlNodeType.Attribute)
					return value [0];
				else
					return '"';
			}
		}

		public override ReadState ReadState
		{
			get { return readState; }
		}

		public override string Value
		{
			get {
				
				string v = value;
				if (valueBuilderAvailable)
					v = valueBuilder.ToString ();
				if(NodeType == XmlNodeType.Attribute)
					return UnescapeAttributeValue(v);
				else
					return v;
			}
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
		}

		public override string GetAttribute (int i)
		{
			if (i > attributes.Count)
				throw new ArgumentOutOfRangeException ("i is smaller than AttributeCount");
			else
				return UnescapeAttributeValue (attributes [orderedAttributes [i]] as string);
		}

		// MS.NET 1.0 documentation says that this method returns String.Empty for
		// not-exist attribute, but in fact it returns null.
		// That description is corrected in MS.NET 1.1 documentation.
		public override string GetAttribute (string name)
		{
			return UnescapeAttributeValue (attributes [name] as string);
		}

		private int GetIndexOfQualifiedAttribute (string localName, string namespaceURI)
		{
			for(int i = 0; i < orderedAttributes.Count; i++)
			{
				string thisName = (string) orderedAttributes [i];

				int indexOfColon = thisName.IndexOf (':');

				if (indexOfColon != -1) {
					string thisLocalName = thisName.Substring (indexOfColon + 1);

					if (localName == thisLocalName) {
						string thisPrefix = thisName.Substring (0, indexOfColon);
						string thisNamespaceURI = LookupNamespace (thisPrefix);

						if (namespaceURI == thisNamespaceURI)
							return i;
					}
				} else if (localName == "xmlns" && namespaceURI == "http://www.w3.org/2000/xmlns/" && thisName == "xmlns")
					return i;
			}
			return -1;
		}

		public override string GetAttribute (string localName, string namespaceURI)
		{
			int idx = this.GetIndexOfQualifiedAttribute (localName, namespaceURI);
			if (idx < 0)
				return null;
			return UnescapeAttributeValue (attributes [orderedAttributes [idx]] as string);
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
			MoveToElement ();

			if (attributes == null || orderedAttributes.Count < i || i < 0)
				throw new ArgumentOutOfRangeException ("attribute index out of range.");

			if (orderedAttributesEnumerator == null) {
				SaveProperties ();
			}

			orderedAttributesEnumerator = orderedAttributes.GetEnumerator ();
			for (int n=0; n<=i; n++)
				orderedAttributesEnumerator.MoveNext();

			string name = orderedAttributes [i] as string;
			string value = attributes [name] as string;
			SetProperties (
				XmlNodeType.Attribute, // nodeType
				name, // name
				false, // isEmptyElement
				value, // value
				false // clearAttributes
				);
			attributeValuePos = 0;
		}

		public override bool MoveToAttribute (string name)
		{
			MoveToElement ();
			bool match = false;

			if (attributes == null)
				return false;

			if (orderedAttributesEnumerator == null) {
				SaveProperties ();
			}

			orderedAttributesEnumerator = orderedAttributes.GetEnumerator ();
			while (orderedAttributesEnumerator.MoveNext ()) {
				if(name == orderedAttributesEnumerator.Current as string) {
					match = true;
					break;
				}
			}

			if (match) {
				string value = attributes [name] as string;
				SetProperties (
					XmlNodeType.Attribute, // nodeType
					name, // name
					false, // isEmptyElement
					value, // value
					false // clearAttributes
				);
				attributeValuePos = 0;
			}

			return match;
		}

		public override bool MoveToAttribute (string localName, string namespaceName)
		{
			MoveToElement ();

			if (attributes == null)
				return false;

			int idx = GetIndexOfQualifiedAttribute (localName, namespaceName);
			if (idx < 0)
				return false;
			MoveToAttribute (idx);
			return true;
		}

		public override bool MoveToElement ()
		{
			if (orderedAttributesEnumerator != null) {
				orderedAttributesEnumerator = null;
				if (isPropertySaved)
					RestoreProperties ();
				return true;
			}

			return false;
		}

		public override bool MoveToFirstAttribute ()
		{
			MoveToElement ();
			return MoveToNextAttribute ();
		}

		public override bool MoveToNextAttribute ()
		{
			if (attributes == null)
				return false;

			if (orderedAttributesEnumerator == null) {
				SaveProperties ();
				orderedAttributesEnumerator = orderedAttributes.GetEnumerator ();
			}

			if (orderedAttributesEnumerator.MoveNext ()) {
				string name = orderedAttributesEnumerator.Current as string;
				string value = attributes [name] as string;
				SetProperties (
					XmlNodeType.Attribute, // nodeType
					name, // name
					false, // isEmptyElement
					value, // value
					false // clearAttributes
				);
				attributeValuePos = 0;
				return true;
			}

			return false;
		}

		public override bool Read ()
		{
			bool more = false;
			isPropertySaved = false;
			readState = ReadState.Interactive;

			// It was moved from end of ReadStartTag ().
			if (depthUp)
				++depth;
			depthUp = false;

			more = ReadContent ();

			if (depth == 0 && !allowMultipleRoot && (IsEmptyElement || NodeType == XmlNodeType.EndElement))
				currentState = XmlNodeType.EndElement;
			if (maybeTextDecl != 0)
				maybeTextDecl--;

			return more;
		}

		public override bool ReadAttributeValue ()
		{
			// 'attributeString' holds real string value (without their
			// quotation characters).
			//
			// 'attributeValuePos' holds current position
			// of 'attributeString' while iterating ReadAttribute().
			// It may be:
			//   -1 if ReadAttributeValue() has already finished.
			//    0 if ReadAttributeValue() ready to start reading.
			//   >0 if ReadAttributeValue() already got 1 or more values
			//
			// local 'refPosition' holds the position on the 
			// attributeString which may be used next time.

			if (attributeValuePos < 0)
				return false;

			// If not started, then initialize attributeString when parsing is at start.
			if (attributeValuePos == 0)
				attributeString =
					value.Substring (1, value.Length - 2);

			returnEntityReference = false;
			value = String.Empty;
			int refPosition;
			int loop = 0;

			do {
				refPosition = attributeString.IndexOf ('&', attributeValuePos);
				if (refPosition < 0) {
					// Reached to the end of value string.
					value += attributeString.Substring (attributeValuePos);
					attributeValuePos = -1;
					break;
				} else if (refPosition == attributeValuePos) {
					string parsed = ReadAttributeValueReference ();
					if (parsed != null)
						value += parsed;
					else {
						// Found that an entity reference starts from this point.
						// reset position to after '&'.
						attributeValuePos = refPosition;
						if (value.Length <= 0) {
							int endNamePos = attributeString.IndexOf (";", attributeValuePos);
							value = attributeString.Substring (attributeValuePos+1, endNamePos - attributeValuePos - 1);
							attributeValuePos += value.Length + 2;
							returnEntityReference = true;
						}
						break;
					}
				} else {
					value += attributeString.Substring (attributeValuePos,
						refPosition - attributeValuePos);
					attributeValuePos = refPosition;
					continue;
				}
			} while (++loop > 0);

			if (returnEntityReference)
				SetProperties (XmlNodeType.EntityReference,
					value,
					false,
					String.Empty,
					false);
			else
				SetProperties (XmlNodeType.Text,
					"#text",
					false,
					value,
					false);

			return true;
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
			// XmlTextReaders don't resolve entities.
			throw new InvalidOperationException ("XmlTextReader cannot resolve external entities.");
		}

		#endregion

		#region Internals
		// Parsed DTD Objects
		internal DTDObjectModel DTD;
		#endregion

		#region Privates

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

		private XmlNodeType nodeType;
		private string name;
		private string prefix;
		private string localName;
		private string namespaceURI;
		private bool isEmptyElement;
		private string value;
		private StringBuilder valueBuilder;
		private bool valueBuilderAvailable = false;

		private bool isPropertySaved;
		private XmlNodeType saveNodeType;
		private string saveName;
		private string savePrefix;
		private string saveLocalName;
		private string saveNamespaceURI;
		private bool saveIsEmptyElement;

		private Hashtable attributes;
		private ArrayList orderedAttributes;
		private IEnumerator orderedAttributesEnumerator;

		private bool returnEntityReference;
		private string entityReferenceName;

		private char [] nameBuffer;
		private int nameLength;
		private int nameCapacity;
		private const int initialNameCapacity = 256;

		private StringBuilder valueBuffer;

		// A buffer for ReadContent for ReadOuterXml
		private StringBuilder currentTag {
			get {
				return currentInput.CurrentMarkup;
			}
		}

		private string attributeString;
		private int attributeValuePos;
		// This should be only referenced(used) by ReadInnerXml(). Kind of flyweight pattern.
		private StringBuilder innerXmlBuilder;

		// Parameter entity placeholder
		private Hashtable parameterEntities;
		private int dtdIncludeSect;

		// State machine attribute.
		//	XmlDeclaration: after the first node.
		//	DocumentType: after doctypedecl
		//	Element: inside document element
		//	EndElement: after document element
		private XmlNodeType currentState;
		private int maybeTextDecl;

		// These values are never re-initialized.
		private XmlResolver resolver = new XmlUrlResolver ();
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

			nodeType = XmlNodeType.None;
			name = String.Empty;
			prefix = String.Empty;
			localName = string.Empty;
			isEmptyElement = false;
			value = String.Empty;

			attributes = new Hashtable ();
			attributeString = String.Empty;
			orderedAttributes = new ArrayList ();
			orderedAttributesEnumerator = null;

			returnEntityReference = false;
			entityReferenceName = String.Empty;

			nameBuffer = new char [initialNameCapacity];
			nameLength = 0;
			nameCapacity = initialNameCapacity;
			
			valueBuffer = new StringBuilder (8192);
			parameterEntities = new Hashtable ();
		}

		private void InitializeContext (string url, XmlParserContext context, TextReader fragment, XmlNodeType fragType)
		{
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
				value = String.Format ("{0}{1}{0}", "'", fragment.ReadToEnd ().Replace ("'", "&apos;"));
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
			StreamReader sr = fragment as StreamReader;
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
			this.nodeType = nodeType;
			this.name = name;
			this.isEmptyElement = isEmptyElement;
			this.value = value;
			this.elementDepth = depth;
			this.valueBuilderAvailable = false;

			if (clearAttributes)
				ClearAttributes ();

			if (namespaces) {
				int indexOfColon = name.IndexOf (':');

				if (indexOfColon == -1) {
					prefix = String.Empty;
					localName = name;
				} else {
					prefix = name.Substring (0, indexOfColon);
					localName = name.Substring (indexOfColon + 1);
				}
			} else {
				prefix = String.Empty;
				localName = name;
			}

			switch (nodeType) {
			case XmlNodeType.Attribute:
				if (prefix == string.Empty) namespaceURI = string.Empty;
				else namespaceURI = LookupNamespace (prefix);
				if (localName == "xmlns" && prefix == "")
					namespaceURI = "http://www.w3.org/2000/xmlns/";
				break;

			case XmlNodeType.Element:
			case XmlNodeType.EndElement:
				namespaceURI = LookupNamespace (prefix);
				break;
			default:
				namespaceURI = "";
				break;
			}
		}
		
		private void SetProperties (
			XmlNodeType nodeType,
			string name,
			bool isEmptyElement,
			StringBuilder value,
			bool clearAttributes) {
			SetProperties (nodeType, name, isEmptyElement, (string)null, clearAttributes);
			this.valueBuilderAvailable = true;
			this.valueBuilder = value;
		}

		private void SaveProperties ()
		{
			// If already saved, then return.
			if (isPropertySaved)
				return;

			saveNodeType = nodeType;
			saveName = name;
			savePrefix = prefix;
			saveLocalName = localName;
			saveNamespaceURI = namespaceURI;
			saveIsEmptyElement = isEmptyElement;
			// An element's value is always String.Empty.
			isPropertySaved = true;
		}

		private void RestoreProperties ()
		{
			nodeType = saveNodeType;
			name = saveName;
			prefix = savePrefix;
			localName = saveLocalName;
			namespaceURI = saveNamespaceURI;
			isEmptyElement = saveIsEmptyElement;
			value = String.Empty;
			isPropertySaved = false;
		}

		private void AddAttribute (string name, string value)
		{
			if (attributes.ContainsKey (name))
				throw new XmlException (this as IXmlLineInfo,
					String.Format ("Attribute {0} already exists.", name));
			attributes.Add (name, value);
			orderedAttributes.Add (name);
		}

		private void ClearAttributes ()
		{
			if (attributes.Count > 0) {
				attributes.Clear ();
				orderedAttributes.Clear ();
			}

			orderedAttributesEnumerator = null;
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

			if (returnEntityReference) {
				SetEntityReferenceProperties ();
			} else {
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
						return ReadWhitespace ();

					SkipWhitespace ();
					return ReadContent ();
				case -1:
					if (depth > 0)
						throw new XmlException ("unexpected end of file. Current depth is " + depth);
					readState = ReadState.EndOfFile;
					SetProperties (
						XmlNodeType.None, // nodeType
						String.Empty, // name
						false, // isEmptyElement
						String.Empty, // value
						true // clearAttributes
					);
					break;
				default:
					ReadText (true);
					break;
				}
			}
			return this.ReadState != ReadState.EndOfFile;
		}

		private void SetEntityReferenceProperties ()
		{
			SetProperties (
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
			if (XmlConstructs.IsNameStart (PeekChar ()))
				ReadAttributes (false);

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
				String.Empty, // value
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
				String.Empty, // value
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
		private void ReadText (bool cleanValue)
		{
			if (currentState != XmlNodeType.Element)
				throw new XmlException (this as IXmlLineInfo,
					"Text node cannot appear in this state.");

			if (cleanValue)
				ClearValueBuffer ();

			int ch = PeekChar ();

			while (ch != '<' && ch != -1) {
				if (ch == '&') {
					ReadChar ();
					if (ReadReference (false))
						break;
				} else
					AppendValueChar (ReadChar ());

				ch = PeekChar ();
			}

			if (returnEntityReference && valueBuffer.Length == 0) {
				SetEntityReferenceProperties ();
			} else {
				SetProperties (
					XmlNodeType.Text, // nodeType
					String.Empty, // name
					false, // isEmptyElement
					valueBuffer, // value
					true // clearAttributes
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

			switch (name)
			{
				case "lt":
					AppendValueChar ('<');
					break;
				case "gt":
					AppendValueChar ('>');
					break;
				case "amp":
					AppendValueChar ('&');
					break;
				case "apos":
					AppendValueChar ('\'');
					break;
				case "quot":
					AppendValueChar ('"');
					break;
				default:
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
					break;
			}
		}

		// The reader is positioned on the first character of
		// the attribute name.
		private void ReadAttributes (bool allowPIEnd)
		{
			int peekChar = -1;
			bool requireWhitespace = false;
			do {
				if (!SkipWhitespace () && requireWhitespace)
					throw new XmlException ("Unexpected token. Name is required here.");
				string name = ReadName ();
				SkipWhitespace ();
				Expect ('=');
				SkipWhitespace ();
				string value = ReadAttribute ();

				if (name == "xmlns")
					parserContext.NamespaceManager.AddNamespace (String.Empty, UnescapeAttributeValue (value));
				else if (name.StartsWith ("xmlns:"))
					parserContext.NamespaceManager.AddNamespace (name.Substring (6), UnescapeAttributeValue (value));

				AddAttribute (name, value);

				if (XmlConstructs.IsSpace (PeekChar ()))
					SkipWhitespace ();
				else
					requireWhitespace = true;
				peekChar = PeekChar ();
				if (peekChar == '?' && allowPIEnd)
					break;
			} while (peekChar != '/' && peekChar != '>' && peekChar != -1);
		}

		// The reader is positioned on the quote character.
		// *Keeps quote char* to value to get_QuoteChar() correctly.
		private string ReadAttribute ()
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
			}
			if (currentState == XmlNodeType.None)
				currentState = XmlNodeType.XmlDeclaration;

			SkipWhitespace ();

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
				valueBuffer, // value
				true // clearAttributes
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
			currentState = XmlNodeType.XmlDeclaration;

			ClearAttributes ();

			ReadAttributes (true);	// They must have "version."
			Expect ("?>");

			if (maybeTextDecl != 0)
				if (this ["standalone"] != null)
					throw new XmlException (this as IXmlLineInfo,
						"Invalid text declaration.");
			maybeTextDecl = 0;

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

				AppendValueChar ((char)ch);
			}

			SetProperties (
				XmlNodeType.Comment, // nodeType
				String.Empty, // name
				false, // isEmptyElement
				valueBuffer, // value
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
				valueBuffer, // value
				true // clearAttributes
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
			string publicId = String.Empty;
			string systemId = String.Empty;
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
				SkipWhitespace ();
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
		}

		internal void GenerateDTDObjectModel (string name, string publicId,
			string systemId, string internalSubset)
		{
			GenerateDTDObjectModel (name, publicId, systemId, internalSubset, 0, 0);
		}

		internal void GenerateDTDObjectModel (string name, string publicId,
			string systemId, string internalSubset, int intSubsetStartLine, int intSubsetStartColumn)
		{
			// now compile DTD
			DTD = new DTDObjectModel ();	// merges both internal and external subsets in the meantime,
			DTD.Name = name;
			int originalParserDepth = parserInputStack.Count;
			if (internalSubset != null && internalSubset.Length > 0) {
				XmlParserInput original = currentInput;
				currentInput = new XmlParserInput (new StringReader (internalSubset), BaseURI, intSubsetStartLine, intSubsetStartColumn);
				do {
					CompileDTDSubset ();
					if (PeekChar () == -1 && parserInputStack.Count > 0)
						PopParserInput ();
				} while (nodeType != XmlNodeType.None || parserInputStack.Count > originalParserDepth);
				if (dtdIncludeSect != 0)
					throw new XmlException (this as IXmlLineInfo,"INCLUDE section is not ended correctly.");
				currentInput = original;
			}
			if (systemId != null && systemId != String.Empty && resolver != null) {
				PushParserInput (systemId);
				do {
					this.CompileDTDSubset ();
					if (PeekChar () == -1 && parserInputStack.Count > 1)
						PopParserInput ();
				} while (nodeType != XmlNodeType.None || parserInputStack.Count > originalParserDepth + 1);
				PopParserInput ();
			}

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
		// returns a node of some nodeType or null, setting nodeType.
		//	 (if None then ']' was found.)
		private void CompileDTDSubset()
		{
			SkipWhitespace ();
			switch(PeekChar ())
			{
			case -1:
				nodeType = XmlNodeType.None;
				break;
			case '%':
				TryExpandPERef ();
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
				// End of inclusion
				Expect ("]]>");
				dtdIncludeSect--;
				SkipWhitespace ();
				break;
			default:
				throw new XmlException (this as IXmlLineInfo,String.Format ("Syntax Error inside doctypedecl markup : {0}({1})", PeekChar (), (char) PeekChar ()));
			}
		}

		private void CompileDeclaration ()
		{
			nodeType = XmlNodeType.DocumentType;	// Hack!!
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
					SkipWhitespace ();
					LOOPBACK:
					if (PeekChar () == '%') {
						ReadChar ();
						if (!XmlConstructs.IsSpace (PeekChar ())) {
							ExpandPERef ();
							goto LOOPBACK;
//							throw new XmlException (this as IXmlLineInfo,"expected whitespace between '%' and name.");
						} else {
							SkipWhitespace ();
							TryExpandPERef ();
							if (XmlConstructs.IsName (PeekChar ()))
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
			SkipWhitespace ();
			TryExpandPERef ();
			decl.Name = ReadName ();
			SkipWhitespace ();
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
				if(PeekChar () == '#') {
					// Mixed Contents. "#PCDATA" must appear first.
					decl.IsMixedContent = true;
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
						model.ElementName = ReadName ();
						model.ChildModels.Add (elem);
						SkipWhitespace ();
						TryExpandPERef ();
					}
					Expect (')');
					if (model.ChildModels.Count > 0) {
						Expect ('*');
						model.Occurence = DTDOccurence.ZeroOrMore;
					}
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
							model.OrderType = DTDContentOrderType.Or;
							ReadChar ();
							SkipWhitespace ();
							model.ChildModels.Add (ReadCP (decl));
							SkipWhitespace ();
						}
						else if(PeekChar () == ',')
						{
							// CPType=Seq
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
						model.OrderType = DTDContentOrderType.Or;
						ReadChar ();
						SkipWhitespace ();
						model.ChildModels.Add (ReadCP (elem));
						SkipWhitespace ();
					}
					else if(PeekChar () == ',') {
						// CPType=Seq
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
			SkipWhitespace ();

			if (PeekChar () == 'S' || PeekChar () == 'P') {
//				throw new NotImplementedException ("External parameter entity reference is not implemented yet.");
				// read publicId/systemId
				ReadExternalID ();
				decl.PublicId = attributes ["PUBLIC"] as string;
				decl.SystemId = attributes ["SYSTEM"] as string;
				SkipWhitespace ();
			}
			else {
				TryExpandPERef ();
				int quoteChar = ReadChar ();
				int start = currentTag.Length;
				while (true) {
					SkipWhitespace ();
					int c = PeekChar ();
					if ((int) c == -1)
						throw new XmlException ("unexpected end of stream in entity value definition.");
					switch (c) {
					case '"':
						ReadChar ();
						if (quoteChar == '"') goto SKIP;
						break;
					case '\'':
						ReadChar ();
						if (quoteChar == '\'') goto SKIP;
						break;
					case '%':
						ImportAsPERef ();
						break;
					default:
						ReadChar ();
						break;
					}
				}
				SKIP:
				decl.Value = currentTag.ToString (start, currentTag.Length - start - 1);
			}
			SkipWhitespace ();
			Expect ('>');
			if (parameterEntities [decl.Name] == null) {
                                parameterEntities.Add (decl.Name, decl);
			}
		}

		// reader is positioned on '%'
		private void ImportAsPERef ()
		{
			StringBuilder sb = null;
			int peRefStart = currentTag.Length;
			string appendStr = "";
			ReadChar ();
			string peName = ReadName ();
			Expect (';');
			DTDParameterEntityDeclaration peDecl =
				this.parameterEntities [peName] as DTDParameterEntityDeclaration;
			if (peDecl == null)
				throw new XmlException (this as IXmlLineInfo,"Parameter entity " + peName + " not found.");
			if (peDecl.SystemId != null) {
				PushParserInput (peDecl.SystemId);
				if (sb == null)
					sb = new StringBuilder ();
				else
					sb.Length = 0;
				while (PeekChar () != -1)
					sb.Append (ReadChar ());
				PopParserInput ();
				appendStr = sb.ToString ();
			} else {
				appendStr = peDecl.Value;
			}
			currentTag.Remove (peRefStart,
				currentTag.Length - peRefStart);
			currentTag.Append (Dereference (appendStr));
		}

		// The reader is positioned on the head of the name.
		private DTDEntityDeclaration ReadEntityDecl ()
		{
			DTDEntityDeclaration decl = new DTDEntityDeclaration ();
			decl.Name = ReadName ();
			SkipWhitespace ();
			TryExpandPERef ();
			SkipWhitespace ();

			if (PeekChar () == 'S' || PeekChar () == 'P') {
				// external entity
				ReadExternalID ();
				decl.PublicId = attributes ["PUBLIC"] as string;
				decl.SystemId = attributes ["SYSTEM"] as string;
				if (SkipWhitespace ()) {
					if (PeekChar () == 'N') {
						// NDataDecl
						Expect ("NDATA");
						if (SkipWhitespace ())
							decl.NotationName = ReadName ();	// ndata_name
					}
				}
			}
			else {
				// general entity
				decl.EntityValue = ReadEntityValueDecl ();
			}
			SkipWhitespace ();
			// This expanding is only allowed as a non-validating parser.
			TryExpandPERef ();
			Expect ('>');
			return decl;
		}

		private string ReadEntityValueDecl ()
		{
			SkipWhitespace ();
			// quotation char will be finally removed on unescaping
			int quoteChar = ReadChar ();
			int start = currentTag.Length;
			if (quoteChar != '\'' && quoteChar != '"')
				throw new XmlException ("quotation char was expected.");

			while (PeekChar () != quoteChar) {
				switch (PeekChar ()) {
				case '%':
					this.ImportAsPERef ();
					continue;
				case '&':
					ReadChar ();
					ReadReference (true);
					break;
				case -1:
					throw new XmlException ("unexpected end of stream.");
				default:
					ReadChar ();
					break;
				}
			}
			string value = Dereference (currentTag.ToString (start, currentTag.Length - start));
			Expect (quoteChar);
			return value;
		}

		private DTDAttListDeclaration ReadAttListDecl ()
		{
			SkipWhitespace ();
			TryExpandPERef ();
			string name = ReadName ();	// target element name
			DTDAttListDeclaration decl =
				DTD.AttListDecls [name] as DTDAttListDeclaration;
			if (decl == null)
				decl = new DTDAttListDeclaration ();
			decl.Name = name;

			SkipWhitespace ();
			TryExpandPERef ();
			SkipWhitespace ();

			while (XmlConstructs.IsName ((char) PeekChar ())) {
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
			Expect ('>');
			return decl;
		}

		private DTDAttributeDefinition ReadAttributeDefinition ()
		{
			DTDAttributeDefinition def = new DTDAttributeDefinition ();

			// attr_name
			TryExpandPERef ();
			def.Name = ReadName ();
			SkipWhitespace ();

			// attr_value
			TryExpandPERef ();
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
					SkipWhitespace ();
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
				TryExpandPERef ();
				Expect ('(');
				SkipWhitespace ();
				def.EnumeratedAttributeDeclaration.Add (ReadNmToken ());		// enum value
				SkipWhitespace ();
				while(PeekChar () == '|') {
					ReadChar ();
					SkipWhitespace ();
					def.EnumeratedAttributeDeclaration.Add (ReadNmToken ());	// enum value
					SkipWhitespace ();
				}
				Expect (')');
				break;
			}
			SkipWhitespace ();

			TryExpandPERef ();

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
					SkipWhitespace ();
					def.UnresolvedDefaultValue = ReadAttribute ();
					break;
				}
			} else {
				// one of the enumerated value
				if (PeekChar () == -1) {
					PopParserInput ();
				}
				SkipWhitespace ();
				def.UnresolvedDefaultValue = ReadAttribute ();
			}

			return def;
		}

		private DTDNotationDeclaration ReadNotationDecl()
		{
			DTDNotationDeclaration decl = new DTDNotationDeclaration ();
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
				SkipWhitespace ();
				if (PeekChar () == '\'' || PeekChar () == '"') {
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
			Expect ('>');
			return decl;
		}

		private void TryExpandPERef ()
		{
			if (PeekChar () == '%') {
				ReadChar ();
				if (!XmlConstructs.IsName (PeekChar ()))
					return;
				ExpandPERef ();
			}
		}

		// reader is positioned on the first letter of the name.
		private void ExpandPERef ()
		{
			ExpandPERef (true);
		}

		private void ExpandPERef (bool attachSpace)
		{
			string peName = ReadName ();
			Expect (";");
			ExpandNamedPERef (peName, attachSpace);
		}

		private void ExpandNamedPERef (string peName, bool attachSpace)
		{
			DTDParameterEntityDeclaration decl =
				parameterEntities [peName] as DTDParameterEntityDeclaration;
			if (decl == null)
				throw new XmlException ("undeclared parameter entity: '" + peName + "'");
			if (decl.SystemId != null) {
				PushParserInput (decl.SystemId);
			}
			// add buffer
			else
				currentInput.InsertParameterEntityBuffer (attachSpace ? " " + Dereference (decl.Value) + " " : decl.Value);
			SkipWhitespace ();	// is it ok?
//			while (PeekChar () == '%')
//				TryExpandPERef ();	// recursive
		}

		private void ReadExternalID() {
			switch(PeekChar ()) {
			case 'S':
				attributes ["PUBLIC"] = null;
				attributes ["SYSTEM"] = ReadSystemLiteral (true);
				break;
			case 'P':
				attributes ["PUBLIC"] = ReadPubidLiteral ();
				SkipWhitespace ();
				attributes ["SYSTEM"] = ReadSystemLiteral (false);
				break;
			}
		}

		// The reader is positioned on the first 'S' of "SYSTEM".
		private string ReadSystemLiteral (bool expectSYSTEM)
		{
			if(expectSYSTEM)
				Expect ("SYSTEM");
			SkipWhitespace ();
			int quoteChar = ReadChar ();	// apos or quot
			int startPos = currentTag.Length;
			int c = 0;
			while(c != quoteChar) {
				c = ReadChar ();
				if(c < 0) throw new XmlException (this as IXmlLineInfo,"Unexpected end of stream in ExternalID.");
			}
			return currentTag.ToString (startPos, currentTag.Length - 1 - startPos);
		}

		private string ReadPubidLiteral()
		{
			Expect ("PUBLIC");
			SkipWhitespace ();
			int quoteChar = ReadChar ();
			int startPos = currentTag.Length;
			int c = 0;
			while(c != quoteChar)
			{
				c = ReadChar ();
				if(c < 0) throw new XmlException (this as IXmlLineInfo,"Unexpected end of stream in ExternalID.");
				if(c != quoteChar && !XmlConstructs.IsPubid (c))
					throw new XmlException (this as IXmlLineInfo,"character '" + (char)c + "' not allowed for PUBLIC ID");
			}
			return currentTag.ToString (startPos, currentTag.Length - 1 - startPos);
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
				if (!XmlConstructs.IsName ((char) ch))
					throw new XmlException (this as IXmlLineInfo,String.Format ("a nmtoken did not start with a legal character {0} ({1})", ch, (char)ch));
			}
			else {
				if (!XmlConstructs.IsNameStart ((char) ch))
					throw new XmlException (this as IXmlLineInfo,String.Format ("a name did not start with a legal character {0} ({1})", ch, (char)ch));
			}

			nameLength = 0;

			AppendNameChar (ReadChar ());

			while (XmlConstructs.IsName (PeekChar ())) {
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
			bool skipped = XmlConstructs.IsSpace (PeekChar ());
			while (XmlConstructs.IsSpace (PeekChar ()))
				ReadChar ();
			return skipped;
		}

		private bool ReadWhitespace ()
		{
			if (currentState == XmlNodeType.None)
				currentState = XmlNodeType.XmlDeclaration;

			ClearValueBuffer ();
			int ch = PeekChar ();
			do {
				AppendValueChar (ReadChar ());
			} while ((ch = PeekChar ()) != -1 && XmlConstructs.IsSpace (ch));

			if (currentState == XmlNodeType.Element && ch != -1 && ch != '<')
				ReadText (false);
			else
				SetProperties (XmlNodeType.Whitespace,
					       String.Empty,
					       false,
					       valueBuffer,
					       true);

			return (PeekChar () != -1);
		}

		// read entity reference from attribute string and if parsable then return the value.
		private string ReadAttributeValueReference ()
		{
			int endEntityPosition = attributeString.IndexOf(';',
				attributeValuePos);
			if (endEntityPosition < 0)
				throw new XmlException ("Insufficient markup of entity reference");
			string entityName = attributeString.Substring (attributeValuePos + 1,
				endEntityPosition - attributeValuePos - 1);

			attributeValuePos = endEntityPosition + 1;

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
				return c.ToString();
			}
			else {
				switch(entityName)
				{
				case "lt": return "<";
				case "gt": return ">";
				case "amp": return "&";
				case "quot": return "\"";
				case "apos": return "'";
				default: return null;
				}
			}
		}

		private string UnescapeAttributeValue (string unresolved)
		{
			if(unresolved == null) return null;

			// trim start/end edge of quotation character.
			return Dereference (unresolved.Substring (1, unresolved.Length - 2));
		}

		private string Dereference (string unresolved)
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
					switch(entityName) {
					case "lt": resolved.Append ("<"); break;
					case "gt": resolved.Append (">"); break;
					case "amp": resolved.Append ("&"); break;
					case "quot": resolved.Append ("\""); break;
					case "apos": resolved.Append ("'"); break;
					// With respect to "Value", MS document is helpless
					// and the implemention returns inconsistent value
					// (e.g. XML: "&ent; &amp;ent;" ---> Value: "&ent; &ent;".)
					default: resolved.Append ("&" + entityName + ";"); break;
					}
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
