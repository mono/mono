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
using System.IO;
using System.Text;
using System.Xml.Schema; // only required for NET_2_0 (SchemaInfo)
using System.Xml.Serialization; // only required for NET_2_0 (SchemaInfo)
using Mono.Xml; // only required for NET_2_0

namespace System.Xml
{
#if NET_2_0
	public abstract class XmlReader : IDisposable
#else
	public abstract class XmlReader
#endif
	{
		private StringBuilder readStringBuffer;
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

		public abstract bool HasValue { get; }

#if NET_2_0
		public virtual bool IsDefault {
			get { return false; }
		}

		public virtual bool IsEmptyElement {
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

		public abstract bool IsEmptyElement { get; }

		public abstract string this [int i] { get; }

		public abstract string this [string name] { get; }

		public abstract string this [string localName, string namespaceName] { get; }
#endif

		public abstract string LocalName { get; }

		public abstract string Name { get; }

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

		public abstract void Close ();

#if NET_2_0
		public static XmlReader Create (Stream stream)
		{
			return Create (stream, null, null, new XmlUrlResolver (), null);
		}

		public static XmlReader Create (string url)
		{
			return Create (url, null);
		}

		public static XmlReader Create (TextReader reader)
		{
			return Create (reader, null, new XmlUrlResolver (), null);
		}

		public static XmlReader Create (string url, XmlReaderSettings settings)
		{
			return Create (url, null, new XmlUrlResolver (), settings);
		}

		public static XmlReader Create (XmlReader reader, XmlReaderSettings settings)
		{
			return Create (reader, new XmlUrlResolver (), settings);
		}

		[MonoTODO ("ConformanceLevel, IgnoreSchemaXXX etc.")]
		public static XmlReader Create (XmlReader reader, XmlResolver resolver, XmlReaderSettings settings)
		{
			return CreateFilteredXmlReader (reader, resolver, settings);
		}

		[MonoTODO ("ConformanceLevel, IgnoreSchemaXXX etc.; Encoding")]
		public static XmlReader Create (string url, Encoding encoding, XmlResolver resolver, XmlReaderSettings settings)
		{
			return CreateCustomizedTextReader (new XmlTextReader (url, settings != null ? settings.NameTable : null), resolver, settings);
		}

		[MonoTODO ("ConformanceLevel, IgnoreSchemaXXX etc.")]
		public static XmlReader Create (TextReader reader, string baseUri, XmlResolver resolver, XmlReaderSettings settings)
		{
			return CreateCustomizedTextReader (new XmlTextReader (baseUri, reader, settings != null ? settings.NameTable : null), resolver, settings);
		}

		[MonoTODO ("ConformanceLevel, IgnoreSchemaXXX etc.")]
		public static XmlReader Create (Stream stream, string baseUri, Encoding encoding, XmlResolver resolver, XmlReaderSettings settings)
		{
			XmlNameTable nameTable = settings != null ? settings.NameTable : null;
			return CreateCustomizedTextReader (
				encoding == null ?
					new XmlTextReader (baseUri, stream, nameTable) :
					new XmlTextReader (baseUri, new StreamReader (stream, encoding), nameTable),
				resolver,
				settings);
		}

		private static XmlReader CreateCustomizedTextReader (XmlTextReader reader, XmlResolver resolver, XmlReaderSettings settings)
		{
			reader.XmlResolver = resolver;
			// Normalization is set true by default.
			reader.Normalization = true;

			if (settings == null)
				settings = new XmlReaderSettings ();

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

			// FIXME: maybe we had better create XmlParserContext.
			if (settings.NameTable != null)
				reader.SetNameTable (settings.NameTable);

			return CreateFilteredXmlReader (reader, resolver, settings);
		}

		private static XmlReader CreateFilteredXmlReader (XmlReader reader, XmlResolver resolver, XmlReaderSettings settings)
		{
			reader = CreateValidatingXmlReader (reader, settings);

			if (reader.Settings != null ||
				settings.IgnoreComments ||
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
			if (settings.DtdValidate) {
				xvr = new XmlValidatingReader (reader);
				if (!settings.XsdValidate)
					xvr.ValidationType = ValidationType.DTD;
				 // otherwise .Auto by default.
			} else if (settings.XsdValidate) {
				xvr = new XmlValidatingReader (reader);
				xvr.ValidationType = ValidationType.Schema;
			}
			if (xvr != null)
				xvr.SetSchemas (settings.Schemas);

			if (settings.IgnoreIdentityConstraints)
				throw new NotImplementedException ();
			if (!settings.IgnoreInlineSchema)
				throw new NotImplementedException ();
			if (!settings.IgnoreSchemaLocation)
				throw new NotImplementedException ();
			if (!settings.IgnoreValidationWarnings)
				throw new NotImplementedException ();

			return xvr != null ? xvr : reader;
		}
#endif

#if NET_2_0
		public virtual void Dispose ()
		{
			if (ReadState != ReadState.Closed)
				Close ();
		}
#endif

		public abstract string GetAttribute (int i);

		public abstract string GetAttribute (string name);

		public abstract string GetAttribute (
			string localName,
			string namespaceName);

		public static bool IsName (string s)
		{
			return s != null && XmlChar.IsName (s);
		}

		public static bool IsNameToken (string s)
		{
			return s != null && XmlChar.IsNmToken (s);
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

		public virtual bool IsStartElement (string localName, string namespaceName)
		{
			if (!IsStartElement ())
				return false;

			return (LocalName == localName && NamespaceURI == namespaceName);
		}

		public abstract string LookupNamespace (string prefix);

#if NET_2_0
		public virtual string LookupNamespace (string prefix, bool atomizedNames)
#else
		internal virtual string LookupNamespace (string prefix, bool atomizedNames)
#endif
		{
			return LookupNamespace (prefix);
		}

		public abstract void MoveToAttribute (int i);

		public abstract bool MoveToAttribute (string name);

		public abstract bool MoveToAttribute (
			string localName,
			string namespaceName);

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

		public virtual string ReadElementString (string localName, string namespaceName)
		{
			if (MoveToContent () != XmlNodeType.Element) {
				string error = String.Format ("'{0}' is an invalid node type.",
							      NodeType.ToString ());
				throw XmlError (error);
			}

			if (localName != LocalName || NamespaceURI != namespaceName) {
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

#if NET_1_0
		public abstract string ReadInnerXml ();

		public abstract string ReadOuterXml ();

#else
		public virtual string ReadInnerXml ()
		{
			return ReadInnerXmlInternal ();
		}

		public virtual string ReadOuterXml ()
		{
			return ReadOuterXmlInternal ();
		}
#endif

		internal string ReadInnerXmlInternal ()
		{
			if (ReadState != ReadState.Interactive || NodeType == XmlNodeType.EndElement)
				return String.Empty;

			StringWriter sw = new StringWriter ();
			XmlTextWriter xtw = new XmlTextWriter (sw);
			if (NodeType == XmlNodeType.Element) {
				if (IsEmptyElement) {
					Read ();
					return String.Empty;
				}
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

		internal string ReadOuterXmlInternal ()
		{
			if (ReadState != ReadState.Interactive || NodeType == XmlNodeType.EndElement)
				return String.Empty;

			StringWriter sw = new StringWriter ();
			XmlTextWriter xtw = new XmlTextWriter (sw);
			xtw.WriteNode (this, false);
			return sw.ToString ();
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

		public virtual void ReadStartElement (string localName, string namespaceName)
		{
			if (MoveToContent () != XmlNodeType.Element) {
				string error = String.Format ("'{0}' is an invalid node type.",
							      NodeType.ToString ());
				throw XmlError (error);
			}

			if (localName != LocalName || NamespaceURI != namespaceName) {
				string error = String.Format ("Expecting {0} tag from namespace {1}, got {2} and {3} instead",
							      localName, namespaceName,
							      LocalName, NamespaceURI);
				throw XmlError (error);
			}

			Read ();
		}

#if NET_1_0
		public abstract string ReadString ();
#else
		public virtual string ReadString ()
		{
			return ReadStringInternal ();
		}
#endif

		internal string ReadStringInternal ()
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

		[MonoTODO]
		public virtual bool ReadToDescendant (string name)
		{
			if (NodeType != XmlNodeType.Element || IsEmptyElement)
				return false;
			int depth = Depth;
			for (Read (); depth < Depth; Read ())
				if (NodeType == XmlNodeType.Element && name == Name)
					return true;
			return false;
		}

		[MonoTODO]
		public virtual bool ReadToDescendant (string localName, string namespaceURI)
		{
			if (NodeType != XmlNodeType.Element || IsEmptyElement)
				return false;
			int depth = Depth;
			for (Read (); depth < Depth; Read ())
				if (NodeType == XmlNodeType.Element && localName == LocalName && namespaceURI == NamespaceURI)
					return true;
			return false;
		}

		[MonoTODO]
		public virtual bool ReadToNextSibling (string name)
		{
			if (NodeType != XmlNodeType.Element || IsEmptyElement)
				return false;
			int depth = Depth;
			for (Skip (); depth < Depth; Skip ())
				if (NodeType == XmlNodeType.Element && name == Name)
					return true;
			return false;
		}

		[MonoTODO]
		public virtual bool ReadToNextSibling (string localName, string namespaceURI)
		{
			if (NodeType != XmlNodeType.Element || IsEmptyElement)
				return false;
			int depth = Depth;
			for (Skip (); depth < Depth; Skip ())
				if (NodeType == XmlNodeType.Element && localName == LocalName && namespaceURI == NamespaceURI)
					return true;
			return false;
		}

		[MonoTODO]
		public virtual XmlReader ReadSubtree ()
		{
			return new SubtreeXmlReader (this);
		}

		[MonoTODO]
		[Obsolete]
		public virtual object ReadTypedValue ()
		{
			if (NodeType == XmlNodeType.Element)
				return ReadElementContentAs (ValueType, this as IXmlNamespaceResolver);
			else
				return ReadContentAs (ValueType, this as IXmlNamespaceResolver);
		}

		private string ReadContentString ()
		{
			switch (NodeType) {
			case XmlNodeType.Text:
			case XmlNodeType.CDATA:
			case XmlNodeType.SignificantWhitespace:
			case XmlNodeType.Whitespace:
				break;
			default:
				throw new InvalidOperationException (String.Format ("This method does not support node type {0}.", NodeType));
			}
			return ReadString ();
		}

		[MonoTODO]
		public virtual object ReadElementContentAs (Type type, IXmlNamespaceResolver resolver)
		{
			return ValueAs (ReadElementString (), type, resolver);
		}

		[MonoTODO]
		public virtual object ReadElementContentAs (Type type, IXmlNamespaceResolver resolver, string localName, string namespaceURI)
		{
			return ValueAs (ReadElementString (localName, namespaceURI), type, resolver);
		}

		[MonoTODO]
		public virtual object ReadContentAs (Type type, IXmlNamespaceResolver resolver)
		{
			return ValueAs (ReadContentString (), type, resolver);
		}

		private object ValueAs (string text, Type type, IXmlNamespaceResolver resolver)
		{
			try {
				if (type == typeof (XmlQualifiedName))
					return XmlQualifiedName.Parse (text, resolver);

				switch (Type.GetTypeCode (type)) {
				case TypeCode.Boolean:
					return ReadContentAsBoolean ();
				case TypeCode.DateTime:
					return ReadContentAsDateTime ();
				case TypeCode.Double:
					return ReadContentAsDouble ();
				case TypeCode.Int32:
					return ReadContentAsInt ();
				case TypeCode.String:
					return ReadContentAsString ();
				}
			} catch (Exception ex) {
				return new FormatException (String.Format ("Current text value '{0}' is not acceptable for specified type '{1}'.", text, type));
			}
			throw new ArgumentException (String.Format ("Specified type '{0}' is not supported.", type));
		}

		[MonoTODO]
		public virtual bool ReadElementContentAsBoolean ()
		{
			return XQueryConvert.StringToBoolean (ReadElementString ());
		}

		[MonoTODO]
		public virtual DateTime ReadElementContentAsDateTime ()
		{
			return XQueryConvert.StringToDateTime (ReadElementString ());
		}

		[MonoTODO]
		public virtual double ReadElementContentAsDouble ()
		{
			return XQueryConvert.StringToDouble (ReadElementString ());
		}

		[MonoTODO]
		public virtual int ReadElementContentAsInt ()
		{
			return XQueryConvert.StringToInt (ReadElementString ());
		}

		[MonoTODO]
		public virtual string ReadElementContentAsString ()
		{
			return ReadElementString ();
		}

		[MonoTODO]
		public virtual bool ReadElementContentAsBoolean (string localName, string namespaceURI)
		{
			return XQueryConvert.StringToBoolean (ReadElementString (localName, namespaceURI));
		}

		[MonoTODO]
		public virtual DateTime ReadElementContentAsDateTime (string localName, string namespaceURI)
		{
			return XQueryConvert.StringToDateTime (ReadElementString (localName, namespaceURI));
		}

		[MonoTODO]
		public virtual double ReadElementContentAsDouble (string localName, string namespaceURI)
		{
			return XQueryConvert.StringToDouble (ReadElementString (localName, namespaceURI));
		}

		[MonoTODO]
		public virtual int ReadElementContentAsInt (string localName, string namespaceURI)
		{
			return XQueryConvert.StringToInt (ReadElementString (localName, namespaceURI));
		}

		[MonoTODO]
		public virtual string ReadElementContentAsString (string localName, string namespaceURI)
		{
			return ReadElementString (localName, namespaceURI);
		}

		[MonoTODO]
		public virtual bool ReadContentAsBoolean ()
		{
			return XQueryConvert.StringToBoolean (ReadContentString ());
		}

		[MonoTODO]
		public virtual DateTime ReadContentAsDateTime ()
		{
			return XQueryConvert.StringToDateTime (ReadContentString ());
		}

		[MonoTODO]
		public virtual double ReadContentAsDouble ()
		{
			return XQueryConvert.StringToDouble (ReadContentString ());
		}

		[MonoTODO]
		public virtual int ReadContentAsInt ()
		{
			return XQueryConvert.StringToInt (ReadContentString ());
		}

		[MonoTODO]
		public virtual string ReadContentAsString ()
		{
			return ReadContentString ();
		}
#endif

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

		#endregion
	}
}
