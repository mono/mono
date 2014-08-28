//
// XmlReader.cs
//
// Authors:
// 	Jason Diamond (jason@injektilo.org)
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//	Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//
// (C) 2001, 2002 Jason Diamond  http://injektilo.org/
// (c) 2002 Ximian, Inc. (http://www.ximian.com)
// (C) 2003 Atsushi Enomoto
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
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml.Schema; // only required for NET_2_0 (SchemaInfo)
using System.Xml.Serialization; // only required for NET_2_0 (SchemaInfo)
using Mono.Xml.Schema; // only required for NET_2_0
using Mono.Xml; // only required for NET_2_0
#if NET_4_5
using System.Threading;
using System.Threading.Tasks;
#endif

namespace System.Xml
{
#if NET_2_0
	public abstract class XmlReader : IDisposable
#else
	public abstract class XmlReader
#endif
	{
		private StringBuilder readStringBuffer;
		private XmlReaderBinarySupport binary;
#if NET_2_0
		private XmlReaderSettings settings;
#endif

		#region Constructor

		protected XmlReader ()
		{
		}

		#endregion

		#region Properties

		public abstract int AttributeCount { get; }

		public abstract string BaseURI { get; }

		internal XmlReaderBinarySupport Binary {
			get { return binary; }
		}

		internal XmlReaderBinarySupport.CharGetter BinaryCharGetter {
			get { return binary != null ? binary.Getter : null; }
			set {
				if (binary == null)
					binary = new XmlReaderBinarySupport (this);
				binary.Getter = value;
			}
		}

#if NET_2_0
		// To enable it internally in sys.xml, just insert these
		// two lines into Read():
		//
		//	#if NET_2_0
		//	if (Binary != null)
		//		Binary.Reset ();
		//	#endif
		//
		public virtual bool CanReadBinaryContent {
			get { return false; }
		}

		public virtual bool CanReadValueChunk {
			get { return false; }
		}
#else
		internal virtual bool CanReadBinaryContent {
			get { return false; }
		}

		internal virtual bool CanReadValueChunk {
			get { return false; }
		}
#endif

		public virtual bool CanResolveEntity
		{
			get	{ return false; }
		}

		public abstract int Depth { get; }

		public abstract bool EOF { get; }

		public virtual bool HasAttributes
		{
			get { return AttributeCount > 0; }
		}

#if NET_4_0
		public virtual bool HasValue {
			get {
				switch (NodeType) {
				case XmlNodeType.Attribute:
				case XmlNodeType.Comment:
				case XmlNodeType.ProcessingInstruction:
				case XmlNodeType.SignificantWhitespace:
				case XmlNodeType.CDATA:
				case XmlNodeType.Text:
				case XmlNodeType.Whitespace:
				case XmlNodeType.XmlDeclaration:
					return true;
				}
				return false;
			}
		}
#else
		public abstract bool HasValue { get; }
#endif

		public abstract bool IsEmptyElement { get; }

#if NET_2_0
		public virtual bool IsDefault {
			get { return false; }
		}

		public virtual string this [int i] {
			get { return GetAttribute (i); }
		}

		public virtual string this [string name] {
			get { return GetAttribute (name); }
		}

		public virtual string this [string name, string namespaceURI] {
			get { return GetAttribute (name, namespaceURI); }
		}
#else
		public abstract bool IsDefault { get; }

		public abstract string this [int i] { get; }

		public abstract string this [string name] { get; }

		public abstract string this [string localName, string namespaceName] { get; }
#endif

		public abstract string LocalName { get; }

#if NET_2_0
		public virtual string Name {
			get {
				return Prefix.Length > 0 ?
					String.Concat (Prefix, ":", LocalName) :
					LocalName;
			}
		}
#else
		public abstract string Name { get; }
#endif

		public abstract string NamespaceURI { get; }

		public abstract XmlNameTable NameTable { get; }

		public abstract XmlNodeType NodeType { get; }

		public abstract string Prefix { get; }

#if NET_2_0
		public virtual char QuoteChar {
			get { return '\"'; }
		}
#else
		public abstract char QuoteChar { get; }
#endif

		public abstract ReadState ReadState { get; }

#if NET_2_0
		public virtual IXmlSchemaInfo SchemaInfo {
			get { return null; }
		}

		public virtual XmlReaderSettings Settings {
			get { return settings; }
		}
#endif

		public abstract string Value { get; }

#if NET_2_0
		public virtual string XmlLang {
			get { return String.Empty; }
		}

		public virtual XmlSpace XmlSpace {
			get { return XmlSpace.None; }
		}
#else
		public abstract string XmlLang { get; }

		public abstract XmlSpace XmlSpace { get; }
#endif

		#endregion

		#region Methods

#if NET_4_5
		public virtual void Close ()
		{
			if (asyncRunning)
				throw new InvalidOperationException ("An asynchronous operation is already in progress.");
		}
#else
		public abstract void Close ();
#endif

#if NET_2_0
		private static XmlNameTable PopulateNameTable (
			XmlReaderSettings settings)
		{
			XmlNameTable nameTable = settings.NameTable;
			if (nameTable == null)
				nameTable = new NameTable ();
			return nameTable;
		}

		private static XmlParserContext PopulateParserContext (
			XmlReaderSettings settings, string baseUri)
		{
			XmlNameTable nt = PopulateNameTable (settings);
			return new XmlParserContext (nt,
				new XmlNamespaceManager (nt),
				null,
				null,
				null,
				null,
				baseUri,
				null,
				XmlSpace.None,
				null);
		}

		private static XmlNodeType GetNodeType (
			XmlReaderSettings settings)
		{
			ConformanceLevel level = settings != null ? settings.ConformanceLevel : ConformanceLevel.Auto;
			return
				level == ConformanceLevel.Fragment ?
				XmlNodeType.Element :
				XmlNodeType.Document;
		}

		public static XmlReader Create (Stream input)
		{
			return Create (input, null);
		}

		public static XmlReader Create (string inputUri)
		{
			return Create (inputUri, null);
		}

		public static XmlReader Create (TextReader input)
		{
			return Create (input, null);
		}

		public static XmlReader Create (string inputUri, XmlReaderSettings settings)
		{
			return Create (inputUri, settings, null);
		}

		public static XmlReader Create (Stream input, XmlReaderSettings settings)
		{
			return Create (input, settings, String.Empty);
		}

		public static XmlReader Create (TextReader input, XmlReaderSettings settings)
		{
			return Create (input, settings, String.Empty);
		}

		static XmlReaderSettings PopulateSettings (XmlReaderSettings src)
		{
			XmlReaderSettings copy;
			if (src == null)
				copy = new XmlReaderSettings ();
			else
				copy = src.Clone ();
#if NET_4_5
			copy.SetReadOnly ();
#endif
			return copy;
		}

		static XmlReaderSettings PopulateSettings (XmlReader reader, XmlReaderSettings src)
		{
			XmlReaderSettings copy;
			if (src == null)
				copy = new XmlReaderSettings ();
			else
				copy = src.Clone ();
#if NET_4_5
			if (reader.Settings != null)
				copy.Async = reader.Settings.Async;
			copy.SetReadOnly ();
#endif
			return copy;
		}

		public static XmlReader Create (Stream input, XmlReaderSettings settings, string baseUri)
		{
			settings = PopulateSettings (settings);
			return Create (input, settings,
				PopulateParserContext (settings, baseUri));
		}

		public static XmlReader Create (TextReader input, XmlReaderSettings settings, string baseUri)
		{
			settings = PopulateSettings (settings);
			return Create (input, settings,
				PopulateParserContext (settings, baseUri));
		}

		public static XmlReader Create (XmlReader reader, XmlReaderSettings settings)
		{
			settings = PopulateSettings (reader, settings);
			XmlReader r = CreateFilteredXmlReader (reader, settings);
			r.settings = settings;
			return r;
		}

		public static XmlReader Create (string inputUri, XmlReaderSettings settings, XmlParserContext inputContext)
		{
			settings = PopulateSettings (settings);
			bool closeInputBak = settings.CloseInput;
			try {
				settings.CloseInput = true; // forced. See XmlReaderCommonTests.CreateFromUrlClose().
				if (inputContext == null)
					inputContext = PopulateParserContext (settings, inputUri);
				XmlTextReader xtr = new XmlTextReader (false, settings.XmlResolver, inputUri, GetNodeType (settings), inputContext);
				XmlReader ret = CreateCustomizedTextReader (xtr, settings);
				return ret;
			} finally {
				settings.CloseInput = closeInputBak;
			}
		}

		public static XmlReader Create (Stream input, XmlReaderSettings settings, XmlParserContext inputContext)
		{
			settings = PopulateSettings (settings);
			if (inputContext == null)
				inputContext = PopulateParserContext (settings, String.Empty);
			return CreateCustomizedTextReader (new XmlTextReader (input, GetNodeType (settings), inputContext), settings);
		}

		public static XmlReader Create (TextReader input, XmlReaderSettings settings, XmlParserContext inputContext)
		{
			settings = PopulateSettings (settings);
			if (inputContext == null)
				inputContext = PopulateParserContext (settings, String.Empty);
			return CreateCustomizedTextReader (new XmlTextReader (inputContext.BaseURI, input, GetNodeType (settings), inputContext), settings);
		}

		private static XmlReader CreateCustomizedTextReader (XmlTextReader reader, XmlReaderSettings settings)
		{
			reader.XmlResolver = settings.XmlResolver;
			// Normalization is set true by default.
			reader.Normalization = true;
			reader.EntityHandling = EntityHandling.ExpandEntities;

			if (settings.ProhibitDtd)
				reader.ProhibitDtd = true;

			if (!settings.CheckCharacters)
				reader.CharacterChecking = false;

			// I guess it might be changed in 2.0 RTM to set true
			// as default, or just disappear. It goes against
			// XmlTextReader's default usage and users will have 
			// to close input manually (that's annoying). Moreover,
			// MS XmlTextReader consumes text input more than 
			// actually read and users can acquire those extra
			// consumption by GetRemainder() that returns different
			// TextReader.
			reader.CloseInput = settings.CloseInput;

			// I would like to support it in detail later;
			// MSDN description looks source of confusion. We don't
			// need examples, but precise list of how it works.
			reader.Conformance = settings.ConformanceLevel;

			reader.AdjustLineInfoOffset (settings.LineNumberOffset,
				settings.LinePositionOffset);

			if (settings.NameTable != null)
				reader.SetNameTable (settings.NameTable);

			XmlReader r = CreateFilteredXmlReader (reader, settings);
			r.settings = settings;
			return r;
		}

		private static XmlReader CreateFilteredXmlReader (XmlReader reader, XmlReaderSettings settings)
		{
			ConformanceLevel conf = ConformanceLevel.Auto;
			if (reader is XmlTextReader)
				conf = ((XmlTextReader) reader).Conformance;
			else if (reader.Settings != null)
				conf = reader.Settings.ConformanceLevel;
			else
				conf = settings.ConformanceLevel;
			if (settings.ConformanceLevel != ConformanceLevel.Auto &&
				conf != settings.ConformanceLevel)
				throw new InvalidOperationException (String.Format ("ConformanceLevel cannot be overwritten by a wrapping XmlReader. The source reader has {0}, while {1} is specified.", conf, settings.ConformanceLevel));
			settings.ConformanceLevel = conf;

			reader = CreateValidatingXmlReader (reader, settings);

			if ( settings.IgnoreComments ||
			     settings.IgnoreProcessingInstructions ||
			     settings.IgnoreWhitespace)
				return new XmlFilterReader (reader, settings);
			else {
				reader.settings = settings;
				return reader;
			}
		}

		private static XmlReader CreateValidatingXmlReader (XmlReader reader, XmlReaderSettings settings)
		{
			XmlValidatingReader xvr = null;
			switch (settings.ValidationType) {
			// Auto and XDR are obsoleted in 2.0 and therefore ignored.
			default:
				return reader;
			case ValidationType.DTD:
				xvr = new XmlValidatingReader (reader);
				xvr.XmlResolver = settings.XmlResolver;
				xvr.ValidationType = ValidationType.DTD;
				break;
			case ValidationType.Schema:
				return new XmlSchemaValidatingReader (reader, settings);
			}

			// Actually I don't think they are treated in DTD validation though...
			if ((settings.ValidationFlags & XmlSchemaValidationFlags.ProcessIdentityConstraints) == 0)
				throw new NotImplementedException ();
			//if ((settings.ValidationFlags & XmlSchemaValidationFlags.ProcessInlineSchema) != 0)
			//	throw new NotImplementedException ();
			//if ((settings.ValidationFlags & XmlSchemaValidationFlags.ProcessSchemaLocation) != 0)
			//	throw new NotImplementedException ();
			//if ((settings.ValidationFlags & XmlSchemaValidationFlags.ReportValidationWarnings) == 0)
			//	throw new NotImplementedException ();

			return xvr != null ? xvr : reader;
		}

#if NET_4_0
		public void Dispose ()
#else
		void IDisposable.Dispose() 
#endif
		{
			Dispose (true);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (disposing && ReadState != ReadState.Closed)
				Close ();
		}
#endif

		public abstract string GetAttribute (int i);

		public abstract string GetAttribute (string name);

		public abstract string GetAttribute (string name, string namespaceURI);

		public static bool IsName (string str)
		{
			return str != null && XmlChar.IsName (str);
		}

		public static bool IsNameToken (string str)
		{
			return str != null && XmlChar.IsNmToken (str);
		}

		public virtual bool IsStartElement ()
		{
			return (MoveToContent () == XmlNodeType.Element);
		}

		public virtual bool IsStartElement (string name)
		{
			if (!IsStartElement ())
				return false;

			return (Name == name);
		}

		public virtual bool IsStartElement (string localname, string ns)
		{
			if (!IsStartElement ())
				return false;

			return (LocalName == localname && NamespaceURI == ns);
		}

		public abstract string LookupNamespace (string prefix);

#if NET_2_0
		public virtual void MoveToAttribute (int i)
		{
			if (i >= AttributeCount)
				throw new ArgumentOutOfRangeException ();
			MoveToFirstAttribute ();
			for (int a = 0; a < i; a++)
				MoveToNextAttribute ();
		}
#else
		public abstract void MoveToAttribute (int i);
#endif

		public abstract bool MoveToAttribute (string name);

		public abstract bool MoveToAttribute (string name, string ns);

		private bool IsContent (XmlNodeType nodeType)
		{
			/* MS doc says:
			 * (non-white space text, CDATA, Element, EndElement, EntityReference, or EndEntity)
			 */
			switch (nodeType) {
			case XmlNodeType.Text:
				return true;
			case XmlNodeType.CDATA:
				return true;
			case XmlNodeType.Element:
				return true;
			case XmlNodeType.EndElement:
				return true;
			case XmlNodeType.EntityReference:
				return true;
			case XmlNodeType.EndEntity:
				return true;
			}

			return false;
		}

		public virtual XmlNodeType MoveToContent ()
		{
			switch (ReadState) {
			case ReadState.Initial:
			case ReadState.Interactive:
				break;
			default:
				return NodeType;
			}

			if (NodeType == XmlNodeType.Attribute)
				MoveToElement ();

			do {
				if (IsContent (NodeType))
					return NodeType;
				Read ();
			} while (!EOF);
			return XmlNodeType.None;
		}

		public abstract bool MoveToElement ();

		public abstract bool MoveToFirstAttribute ();

		public abstract bool MoveToNextAttribute ();

		public abstract bool Read ();

		public abstract bool ReadAttributeValue ();

		public virtual string ReadElementString ()
		{
			if (MoveToContent () != XmlNodeType.Element) {
				string error = String.Format ("'{0}' is an invalid node type.",
							      NodeType.ToString ());
				throw XmlError (error);
			}

			string result = String.Empty;
			if (!IsEmptyElement) {
				Read ();
				result = ReadString ();
				if (NodeType != XmlNodeType.EndElement) {
					string error = String.Format ("'{0}' is an invalid node type.",
								      NodeType.ToString ());
					throw XmlError (error);
				}
			}

			Read ();
			return result;
		}

		public virtual string ReadElementString (string name)
		{
			if (MoveToContent () != XmlNodeType.Element) {
				string error = String.Format ("'{0}' is an invalid node type.",
							      NodeType.ToString ());
				throw XmlError (error);
			}

			if (name != Name) {
				string error = String.Format ("The {0} tag from namespace {1} is expected.",
							      Name, NamespaceURI);
				throw XmlError (error);
			}

			string result = String.Empty;
			if (!IsEmptyElement) {
				Read ();
				result = ReadString ();
				if (NodeType != XmlNodeType.EndElement) {
					string error = String.Format ("'{0}' is an invalid node type.",
								      NodeType.ToString ());
					throw XmlError (error);
				}
			}

			Read ();
			return result;
		}

		public virtual string ReadElementString (string localname, string ns)
		{
			if (MoveToContent () != XmlNodeType.Element) {
				string error = String.Format ("'{0}' is an invalid node type.",
							      NodeType.ToString ());
				throw XmlError (error);
			}

			if (localname != LocalName || NamespaceURI != ns) {
				string error = String.Format ("The {0} tag from namespace {1} is expected.",
							      LocalName, NamespaceURI);
				throw XmlError (error);
			}

			string result = String.Empty;
			if (!IsEmptyElement) {
				Read ();
				result = ReadString ();
				if (NodeType != XmlNodeType.EndElement) {
					string error = String.Format ("'{0}' is an invalid node type.",
								      NodeType.ToString ());
					throw XmlError (error);
				}
			}

			Read ();
			return result;
		}

		public virtual void ReadEndElement ()
		{
			if (MoveToContent () != XmlNodeType.EndElement) {
				string error = String.Format ("'{0}' is an invalid node type.",
							      NodeType.ToString ());
				throw XmlError (error);
			}

			Read ();
		}

		public virtual string ReadInnerXml ()
		{
			if (ReadState != ReadState.Interactive || NodeType == XmlNodeType.EndElement)
				return String.Empty;

			if (IsEmptyElement) {
				Read ();
				return String.Empty;
			}
			StringWriter sw = new StringWriter ();
			XmlTextWriter xtw = new XmlTextWriter (sw);
			if (NodeType == XmlNodeType.Element) {
				int startDepth = Depth;
				Read ();
				while (startDepth < Depth) {
					if (ReadState != ReadState.Interactive)
						throw XmlError ("Unexpected end of the XML reader.");
					xtw.WriteNode (this, false);
				}
				// reader is now end element, then proceed once more.
				Read ();
			}
			else
				xtw.WriteNode (this, false);

			return sw.ToString ();
		}

		public virtual string ReadOuterXml ()
		{
			if (ReadState != ReadState.Interactive || NodeType == XmlNodeType.EndElement)
				return String.Empty;

			switch (NodeType) {
			case XmlNodeType.Element:
			case XmlNodeType.Attribute:
				StringWriter sw = new StringWriter ();
				XmlTextWriter xtw = new XmlTextWriter (sw);
				xtw.WriteNode (this, false);
				return sw.ToString ();
			default:
				Skip ();
				return String.Empty;
			}
		}

		public virtual void ReadStartElement ()
		{
			if (MoveToContent () != XmlNodeType.Element) {
				string error = String.Format ("'{0}' is an invalid node type.",
							      NodeType.ToString ());
				throw XmlError (error);
			}

			Read ();
		}

		public virtual void ReadStartElement (string name)
		{
			if (MoveToContent () != XmlNodeType.Element) {
				string error = String.Format ("'{0}' is an invalid node type.",
							      NodeType.ToString ());
				throw XmlError (error);
			}

			if (name != Name) {
				string error = String.Format ("The {0} tag from namespace {1} is expected.",
							      Name, NamespaceURI);
				throw XmlError (error);
			}

			Read ();
		}

		public virtual void ReadStartElement (string localname, string ns)
		{
			if (MoveToContent () != XmlNodeType.Element) {
				string error = String.Format ("'{0}' is an invalid node type.",
							      NodeType.ToString ());
				throw XmlError (error);
			}

			if (localname != LocalName || NamespaceURI != ns) {
				string error = String.Format ("Expecting {0} tag from namespace {1}, got {2} and {3} instead",
							      localname, ns,
							      LocalName, NamespaceURI);
				throw XmlError (error);
			}

			Read ();
		}

		public virtual string ReadString ()
		{
			if (readStringBuffer == null)
				readStringBuffer = new StringBuilder ();
			readStringBuffer.Length = 0;

			MoveToElement ();

			switch (NodeType) {
			default:
				return String.Empty;
			case XmlNodeType.Element:
				if (IsEmptyElement)
					return String.Empty;
				do {
					Read ();
					switch (NodeType) {
					case XmlNodeType.Text:
					case XmlNodeType.CDATA:
					case XmlNodeType.Whitespace:
					case XmlNodeType.SignificantWhitespace:
						readStringBuffer.Append (Value);
						continue;
					}
					break;
				} while (true);
				break;
			case XmlNodeType.Text:
			case XmlNodeType.CDATA:
			case XmlNodeType.Whitespace:
			case XmlNodeType.SignificantWhitespace:
				do {
					switch (NodeType) {
					case XmlNodeType.Text:
					case XmlNodeType.CDATA:
					case XmlNodeType.Whitespace:
					case XmlNodeType.SignificantWhitespace:
						readStringBuffer.Append (Value);
						Read ();
						continue;
					}
					break;
				} while (true);
				break;
			}
			string ret = readStringBuffer.ToString ();
			readStringBuffer.Length = 0;
			return ret;
		}

#if NET_2_0
		public virtual Type ValueType {
			get { return typeof (string); }
		}

		public virtual bool ReadToDescendant (string name)
		{
			if (ReadState == ReadState.Initial) {
				MoveToContent ();
				if (IsStartElement (name))
					return true;
			}
			if (NodeType != XmlNodeType.Element || IsEmptyElement)
				return false;
			int depth = Depth;
			for (Read (); depth < Depth; Read ())
				if (NodeType == XmlNodeType.Element && name == Name)
					return true;
			return false;
		}

		public virtual bool ReadToDescendant (string localName, string namespaceURI)
		{
			if (ReadState == ReadState.Initial) {
				MoveToContent ();
				if (IsStartElement (localName, namespaceURI))
					return true;
			}
			if (NodeType != XmlNodeType.Element || IsEmptyElement)
				return false;
			int depth = Depth;
			for (Read (); depth < Depth; Read ())
				if (NodeType == XmlNodeType.Element && localName == LocalName && namespaceURI == NamespaceURI)
					return true;
			return false;
		}

		public virtual bool ReadToFollowing (string name)
		{
			while (Read ())
				if (NodeType == XmlNodeType.Element && name == Name)
					return true;
			return false;
		}

		public virtual bool ReadToFollowing (string localName, string namespaceURI)
		{
			while (Read ())
				if (NodeType == XmlNodeType.Element && localName == LocalName && namespaceURI == NamespaceURI)
					return true;
			return false;
		}

		public virtual bool ReadToNextSibling (string name)
		{
			if (ReadState != ReadState.Interactive)
				return false;
			MoveToElement ();
			int depth = Depth;
			Skip ();
			for (; !EOF && depth <= Depth; Skip ())
				if (NodeType == XmlNodeType.Element && name == Name)
					return true;
			return false;
		}

		public virtual bool ReadToNextSibling (string localName, string namespaceURI)
		{
			if (ReadState != ReadState.Interactive)
				return false;
			int depth = Depth;
			Skip ();
			for (; !EOF && depth <= Depth; Skip ())
				if (NodeType == XmlNodeType.Element && localName == LocalName && namespaceURI == NamespaceURI)
					return true;
			return false;
		}

		public virtual XmlReader ReadSubtree ()
		{
			if (NodeType != XmlNodeType.Element)
				throw new InvalidOperationException (String.Format ("ReadSubtree() can be invoked only when the reader is positioned on an element. Current node is {0}. {1}", NodeType, GetLocation ()));
			return new SubtreeXmlReader (this);
		}

		private string ReadContentString ()
		{
			// The latter condition indicates that this XmlReader is on an attribute value
			// (HasAttributes is to indicate it is on attribute value).
			if (NodeType == XmlNodeType.Attribute || NodeType != XmlNodeType.Element && HasAttributes)
				return Value;
			return ReadContentString (true);
		}

		private string ReadContentString (bool isText)
		{
			if (isText) {
				switch (NodeType) {
				case XmlNodeType.Text:
				case XmlNodeType.SignificantWhitespace:
				case XmlNodeType.Whitespace:
				case XmlNodeType.CDATA:
					break;
				case XmlNodeType.Element:
					throw new InvalidOperationException (String.Format ("Node type {0} is not supported in this operation.{1}", NodeType, GetLocation ()));
				default:
					return String.Empty;
				}
			}

			string value = String.Empty;
			do {
				switch (NodeType) {
				case XmlNodeType.Element:
					if (isText)
						return value;
					throw XmlError ("Child element is not expected in this operation.");
				case XmlNodeType.EndElement:
					return value;
				case XmlNodeType.Text:
				case XmlNodeType.CDATA:
				case XmlNodeType.SignificantWhitespace:
				case XmlNodeType.Whitespace:
					value += Value;
					break;
				}
			} while (Read ());
			throw XmlError ("Unexpected end of document.");
		}

		string GetLocation ()
		{
			IXmlLineInfo li = this as IXmlLineInfo;
			return li != null && li.HasLineInfo () ?
				String.Format (" {0} (line {1}, column {2})", BaseURI, li.LineNumber, li.LinePosition) : String.Empty;
		}

		[MonoTODO]
		public virtual object ReadElementContentAsObject ()
		{
			return ReadElementContentAs (ValueType, null);
		}

		[MonoTODO]
		public virtual object ReadElementContentAsObject (string localName, string namespaceURI)
		{
			return ReadElementContentAs (ValueType, null, localName, namespaceURI);
		}

		[MonoTODO]
		public virtual object ReadContentAsObject ()
		{
			return ReadContentAs (ValueType, null);
		}

#if NET_4_5
		public virtual DateTimeOffset ReadContentAsDateTimeOffset ()
		{
			try {
				return XmlConvert.ToDateTimeOffset (ReadContentString ());
			} catch (Exception e) {
				throw XmlError ("Typed value is invalid.", e);
			}
		}
#endif

		public virtual object ReadElementContentAs (Type returnType, IXmlNamespaceResolver namespaceResolver)
		{
			bool isEmpty = IsEmptyElement;
			ReadStartElement ();
			object obj = ValueAs (isEmpty ? String.Empty : ReadContentString (false), returnType, namespaceResolver, false);
			if (!isEmpty)
				ReadEndElement ();
			return obj;
		}

		public virtual object ReadElementContentAs (Type returnType, IXmlNamespaceResolver namespaceResolver, string localName, string namespaceURI)
		{
			if (localName == null)
				throw new ArgumentNullException ("localName");
			if (namespaceURI == null)
				throw new ArgumentNullException ("namespaceURI");

			bool isEmpty = IsEmptyElement;
			ReadStartElement (localName, namespaceURI);
			if (isEmpty)
				return ValueAs (String.Empty, returnType, namespaceResolver, false);
			object obj = ReadContentAs (returnType, namespaceResolver);
			ReadEndElement ();
			return obj;
		}

		public virtual object ReadContentAs (Type returnType, IXmlNamespaceResolver namespaceResolver)
		{
			return ValueAs (ReadContentString (), returnType, namespaceResolver, false);
		}

		private object ValueAs (string text, Type type, IXmlNamespaceResolver resolver, bool isArrayItem)
		{
			try {
				if (type == typeof (object))
					return text;
				if (type.IsArray && !isArrayItem) {
					var elemType = type.GetElementType ();
					var sarr = text.Split ((string []) null, StringSplitOptions.RemoveEmptyEntries);
					var ret = Array.CreateInstance (elemType, sarr.Length);
					for (int i = 0; i < ret.Length; i++)
						ret.SetValue (ValueAs (sarr [i], elemType, resolver, true), i);
					return ret;
				}

				if (type == typeof (XmlQualifiedName)) {
					if (resolver != null)
						return XmlQualifiedName.Parse (text, resolver, true);
					else
						return XmlQualifiedName.Parse (text, this, true);
				}
				if (type == typeof (Uri))
					return XmlConvert.ToUri (text);
				if (type == typeof (TimeSpan))
					return XmlConvert.ToTimeSpan (text);
				if (type == typeof (DateTimeOffset))
					return XmlConvert.ToDateTimeOffset (text);

				switch (Type.GetTypeCode (type)) {
				case TypeCode.Boolean:
					return XQueryConvert.StringToBoolean (text);
				case TypeCode.Byte:
					return XmlConvert.ToByte (text);
				case TypeCode.SByte:
					return XmlConvert.ToSByte (text);
				case TypeCode.Int16:
					return XmlConvert.ToInt16 (text);
				case TypeCode.UInt16:
					return XQueryConvert.StringToUnsignedShort (text);
				case TypeCode.Int32:
					return XQueryConvert.StringToInt (text);
				case TypeCode.UInt32:
					return XQueryConvert.StringToUnsignedInt (text);
				case TypeCode.Int64:
					return XQueryConvert.StringToInteger (text);
				case TypeCode.UInt64:
					return XQueryConvert.StringToUnsignedLong (text);
				case TypeCode.DateTime:
					return XQueryConvert.StringToDateTime (text);
				case TypeCode.Decimal:
					return XQueryConvert.StringToDecimal (text);
				case TypeCode.Double:
					return XQueryConvert.StringToDouble (text);
				case TypeCode.Single:
					return XQueryConvert.StringToFloat (text);
				case TypeCode.String:
					return text;
				}
			} catch (Exception ex) {
				throw XmlError (String.Format ("Current text value '{0}' is not acceptable for specified type '{1}'. {2}", text, type, ex != null ? ex.Message : String.Empty), ex);
			}
			throw new XmlException (String.Format ("Specified type '{0}' is not supported.", type));
		}

		public virtual bool ReadElementContentAsBoolean ()
		{
			try {
				return XQueryConvert.StringToBoolean (ReadElementContentAsString ());
			} catch (FormatException ex) {
				throw XmlError ("Typed value is invalid.", ex);
			}
		}

		public virtual DateTime ReadElementContentAsDateTime ()
		{
			try {
				return XQueryConvert.StringToDateTime (ReadElementContentAsString ());
			} catch (FormatException ex) {
				throw XmlError ("Typed value is invalid.", ex);
			}
		}

		public virtual decimal ReadElementContentAsDecimal ()
		{
			try {
				return XQueryConvert.StringToDecimal (ReadElementContentAsString ());
			} catch (FormatException ex) {
				throw XmlError ("Typed value is invalid.", ex);
			}
		}

		public virtual double ReadElementContentAsDouble ()
		{
			try {
				return XQueryConvert.StringToDouble (ReadElementContentAsString ());
			} catch (FormatException ex) {
				throw XmlError ("Typed value is invalid.", ex);
			}
		}

		public virtual float ReadElementContentAsFloat ()
		{
			try {
				return XQueryConvert.StringToFloat (ReadElementContentAsString ());
			} catch (FormatException ex) {
				throw XmlError ("Typed value is invalid.", ex);
			}
		}

		public virtual int ReadElementContentAsInt ()
		{
			try {
				return XQueryConvert.StringToInt (ReadElementContentAsString ());
			} catch (FormatException ex) {
				throw XmlError ("Typed value is invalid.", ex);
			}
		}

		public virtual long ReadElementContentAsLong ()
		{
			try {
				return XQueryConvert.StringToInteger (ReadElementContentAsString ());
			} catch (FormatException ex) {
				throw XmlError ("Typed value is invalid.", ex);
			}
		}

		public virtual string ReadElementContentAsString ()
		{
			bool isEmpty = IsEmptyElement;
			// unlike ReadStartElement() it rejects non-content nodes (this check is done before MoveToContent())
			if (NodeType != XmlNodeType.Element)
				throw new InvalidOperationException (String.Format ("'{0}' is an element node.", NodeType));
			ReadStartElement ();
			if (isEmpty)
				return String.Empty;
			string s = ReadContentString (false);
			ReadEndElement ();
			return s;
		}

		public virtual bool ReadElementContentAsBoolean (string localName, string namespaceURI)
		{
			try {
				return XQueryConvert.StringToBoolean (ReadElementContentAsString (localName, namespaceURI));
			} catch (FormatException ex) {
				throw XmlError ("Typed value is invalid.", ex);
			}
		}

		public virtual DateTime ReadElementContentAsDateTime (string localName, string namespaceURI)
		{
			try {
				return XQueryConvert.StringToDateTime (ReadElementContentAsString (localName, namespaceURI));
			} catch (FormatException ex) {
				throw XmlError ("Typed value is invalid.", ex);
			}
		}

		public virtual decimal ReadElementContentAsDecimal (string localName, string namespaceURI)
		{
			try {
				return XQueryConvert.StringToDecimal (ReadElementContentAsString (localName, namespaceURI));
			} catch (FormatException ex) {
				throw XmlError ("Typed value is invalid.", ex);
			}
		}

		public virtual double ReadElementContentAsDouble (string localName, string namespaceURI)
		{
			try {
				return XQueryConvert.StringToDouble (ReadElementContentAsString (localName, namespaceURI));
			} catch (FormatException ex) {
				throw XmlError ("Typed value is invalid.", ex);
			}
		}

		public virtual float ReadElementContentAsFloat (string localName, string namespaceURI)
		{
			try {
				return XQueryConvert.StringToFloat (ReadElementContentAsString (localName, namespaceURI));
			} catch (FormatException ex) {
				throw XmlError ("Typed value is invalid.", ex);
			}
		}

		public virtual int ReadElementContentAsInt (string localName, string namespaceURI)
		{
			try {
				return XQueryConvert.StringToInt (ReadElementContentAsString (localName, namespaceURI));
			} catch (FormatException ex) {
				throw XmlError ("Typed value is invalid.", ex);
			}
		}

		public virtual long ReadElementContentAsLong (string localName, string namespaceURI)
		{
			try {
				return XQueryConvert.StringToInteger (ReadElementContentAsString (localName, namespaceURI));
			} catch (FormatException ex) {
				throw XmlError ("Typed value is invalid.", ex);
			}
		}

		public virtual string ReadElementContentAsString (string localName, string namespaceURI)
		{
			bool isEmpty = IsEmptyElement;
			// unlike ReadStartElement() it rejects non-content nodes (this check is done before MoveToContent())
			if (NodeType != XmlNodeType.Element)
				throw new InvalidOperationException (String.Format ("'{0}' is an element node.", NodeType));
			ReadStartElement (localName, namespaceURI);
			if (isEmpty)
				return String.Empty;
			string s = ReadContentString (false);
			ReadEndElement ();
			return s;
		}

		public virtual bool ReadContentAsBoolean ()
		{
			try {
				return XQueryConvert.StringToBoolean (ReadContentString ());
			} catch (FormatException ex) {
				throw XmlError ("Typed value is invalid.", ex);
			}
		}

		public virtual DateTime ReadContentAsDateTime ()
		{
			try {
				return XQueryConvert.StringToDateTime (ReadContentString ());
			} catch (FormatException ex) {
				throw XmlError ("Typed value is invalid.", ex);
			}
		}

		public virtual decimal ReadContentAsDecimal ()
		{
			try {
				return XQueryConvert.StringToDecimal (ReadContentString ());
			} catch (FormatException ex) {
				throw XmlError ("Typed value is invalid.", ex);
			}
		}

		public virtual double ReadContentAsDouble ()
		{
			try {
				return XQueryConvert.StringToDouble (ReadContentString ());
			} catch (FormatException ex) {
				throw XmlError ("Typed value is invalid.", ex);
			}
		}

		public virtual float ReadContentAsFloat ()
		{
			try {
				return XQueryConvert.StringToFloat (ReadContentString ());
			} catch (FormatException ex) {
				throw XmlError ("Typed value is invalid.", ex);
			}
		}

		public virtual int ReadContentAsInt ()
		{
			try {
				return XQueryConvert.StringToInt (ReadContentString ());
			} catch (FormatException ex) {
				throw XmlError ("Typed value is invalid.", ex);
			}
		}

		public virtual long ReadContentAsLong ()
		{
			try {
				return XQueryConvert.StringToInteger (ReadContentString ());
			} catch (FormatException ex) {
				throw XmlError ("Typed value is invalid.", ex);
			}
		}

		public virtual string ReadContentAsString ()
		{
			return ReadContentString ();
		}

		public virtual int ReadContentAsBase64 (
			byte [] buffer, int index, int count)
		{
			CheckSupport ();
			return binary.ReadContentAsBase64 (
				buffer, index, count);
		}

		public virtual int ReadContentAsBinHex (
			byte [] buffer, int index, int count)
		{
			CheckSupport ();
			return binary.ReadContentAsBinHex (
				buffer, index, count);
		}

		public virtual int ReadElementContentAsBase64 (
			byte [] buffer, int index, int count)
		{
			CheckSupport ();
			return binary.ReadElementContentAsBase64 (
				buffer, index, count);
		}

		public virtual int ReadElementContentAsBinHex (
			byte [] buffer, int index, int count)
		{
			CheckSupport ();
			return binary.ReadElementContentAsBinHex (
				buffer, index, count);
		}

		private void CheckSupport ()
		{
			// Default implementation expects both.
			if (!CanReadBinaryContent || !CanReadValueChunk)
				throw new NotSupportedException ();
			if (binary == null)
				binary = new XmlReaderBinarySupport (this);
		}
		
#endif

		public virtual int ReadValueChunk (char [] buffer, int index, int count)
		{
			if (!CanReadValueChunk)
				throw new NotSupportedException ();
			if (binary == null)
				binary = new XmlReaderBinarySupport (this);
			return binary.ReadValueChunk (buffer, index, count);
		}

		public abstract void ResolveEntity ();

		public virtual void Skip ()
		{
			if (ReadState != ReadState.Interactive)
				return;

			MoveToElement ();
			if (NodeType != XmlNodeType.Element || IsEmptyElement) {
				Read ();
				return;
			}
				
			int depth = Depth;
			while (Read () && depth < Depth)
				;
			if (NodeType == XmlNodeType.EndElement)
				Read ();
		}

		private XmlException XmlError (string message)
		{
			return new XmlException (this as IXmlLineInfo, BaseURI, message);
		}
#if NET_2_0
		private XmlException XmlError (string message, Exception innerException)
		{
			return new XmlException (this as IXmlLineInfo, BaseURI, message);
		}
#endif
		#endregion

#if NET_4_5
		#region .NET 4.5 Async Methods

		bool asyncRunning;

		void StartAsync ()
		{
			if (!settings.Async)
				throw new InvalidOperationException ("Set XmlReaderSettings.Async to true if you want to use Async Methods.");
			lock (this) {
				if (asyncRunning)
					throw new InvalidOperationException ("An asynchronous operation is already in progress.");
				asyncRunning = true;
			}
		}

		public virtual Task<bool> ReadAsync ()
		{
			StartAsync ();
			return Task.Run (() => {
				try {
					return Read ();
				} finally {
					asyncRunning = false;
				}
			});
		}

		public virtual Task<string> GetValueAsync ()
		{
			StartAsync ();
			return Task.Run (() => {
				try {
					return Value;
				} finally {
					asyncRunning = false;
				}
			});
		}

		public virtual Task<string> ReadInnerXmlAsync ()
		{
			StartAsync ();
			return Task.Run (() => {
				try {
					return ReadInnerXml ();
				} finally {
					asyncRunning = false;
				}
			});
		}

		public virtual Task<string> ReadOuterXmlAsync ()
		{
			StartAsync ();
			return Task.Run (() => {
				try {
					return ReadOuterXml ();
				} finally {
					asyncRunning = false;
				}
			});
		}

		public virtual Task<string> ReadContentAsStringAsync ()
		{
			StartAsync ();
			return Task.Run (() => {
				try {
					return ReadContentAsString ();
				} finally {
					asyncRunning = false;
				}
			});
		}

		public virtual Task<int> ReadContentAsBase64Async (byte[] buffer, int index, int count)
		{
			StartAsync ();
			return Task.Run (() => {
				try {
					return ReadContentAsBase64 (buffer, index, count);
				} finally {
					asyncRunning = false;
				}
			});
		}

		public virtual Task<int> ReadContentAsBinHexAsync (byte[] buffer, int index, int count)
		{
			StartAsync ();
			return Task.Run (() => {
				try {
					return ReadContentAsBinHex (buffer, index, count);
				} finally {
					asyncRunning = false;
				}
			});
		}

		public virtual Task<int> ReadElementContentAsBase64Async (byte[] buffer, int index, int count)
		{
			StartAsync ();
			return Task.Run (() => {
				try {
					return ReadElementContentAsBase64 (buffer, index, count);
				} finally {
					asyncRunning = false;
				}
			});
		}

		public virtual Task<int> ReadElementContentAsBinHexAsync (byte[] buffer, int index, int count)
		{
			StartAsync ();
			return Task.Run (() => {
				try {
					return ReadElementContentAsBinHex (buffer, index, count);
				} finally {
					asyncRunning = false;
				}
			});
		}

		public virtual Task<int> ReadValueChunkAsync (char[] buffer, int index, int count)
		{
			StartAsync ();
			return Task.Run (() => {
				try {
					return ReadValueChunk (buffer, index, count);
				} finally {
					asyncRunning = false;
				}
			});
		}

		public virtual Task<object> ReadContentAsAsync (Type returnType, IXmlNamespaceResolver namespaceResolver)
		{
			StartAsync ();
			return Task.Run (() => {
				try {
					return ReadContentAs (returnType, namespaceResolver);
				} finally {
					asyncRunning = false;
				}
			});
		}

		public virtual Task<object> ReadContentAsObjectAsync ()
		{
			StartAsync ();
			return Task.Run (() => {
				try {
					return ReadContentAsObject ();
				} finally {
					asyncRunning = false;
				}
			});
		}

		public virtual Task<object> ReadElementContentAsAsync (Type returnType, IXmlNamespaceResolver namespaceResolver)
		{
			StartAsync ();
			return Task.Run (() => {
				try {
					return ReadElementContentAs (returnType, namespaceResolver);
				} finally {
					asyncRunning = false;
				}
			});
		}

		public virtual Task<object> ReadElementContentAsObjectAsync ()
		{
			StartAsync ();
			return Task.Run (() => {
				try {
					return ReadElementContentAsObject ();
				} finally {
					asyncRunning = false;
				}
			});
		}

		public virtual Task<string> ReadElementContentAsStringAsync ()
		{
			StartAsync ();
			return Task.Run (() => {
				try {
					return ReadElementContentAsString ();
				} finally {
					asyncRunning = false;
				}
			});
		}

		public virtual Task<XmlNodeType> MoveToContentAsync ()
		{
			StartAsync ();
			return Task.Run (() => {
				try {
					return MoveToContent ();
				} finally {
					asyncRunning = false;
				}
			});
		}

		public virtual Task SkipAsync ()
		{
			StartAsync ();
			return Task.Run (() => {
				try {
					Skip ();
				} finally {
					asyncRunning = false;
				}
			});
		}

		#endregion
#endif
	}
}
