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
using System.IO;
using System.Security.Policy;
using System.Text;

using System.Xml.Schema; // only required for NET_2_0 (SchemaInfo)

namespace System.Xml
{
#if NET_2_0
	public abstract class XmlReader : IDisposable, IXmlDataEvidence
#else
	public abstract class XmlReader
#endif
	{
		private StringBuilder readStringBuffer;
		private Evidence evidence;
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

#if NET_2_0
		[MonoTODO]
		public virtual Evidence Evidence {
			get { return evidence; }
		}
#endif

		public virtual bool HasAttributes
		{
			get { return AttributeCount > 0; }
		}

		public abstract bool HasValue { get; }

		public abstract bool IsDefault { get; }

		public abstract bool IsEmptyElement { get; }

#if NET_2_0
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
		[MonoTODO]
		public virtual IXmlSchemaInfo SchemaInfo {
			get {
				throw new NotImplementedException ();
			}
		}

		public virtual XmlReaderSettings Settings {
			get { return settings; }
		}
#endif

		public abstract string Value { get; }

		public abstract string XmlLang { get; }

		public abstract XmlSpace XmlSpace { get; }

		#endregion

		#region Methods

		public abstract void Close ();

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
				throw new XmlException (this as IXmlLineInfo, error);
			}

			string result = String.Empty;
			if (!IsEmptyElement) {
				Read ();
				result = ReadString ();
				if (NodeType != XmlNodeType.EndElement) {
					string error = String.Format ("'{0}' is an invalid node type.",
								      NodeType.ToString ());
					throw new XmlException (this as IXmlLineInfo, error);
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
				throw new XmlException (this as IXmlLineInfo, error);
			}

			if (name != Name) {
				string error = String.Format ("The {0} tag from namespace {1} is expected.",
							      Name, NamespaceURI);
				throw new XmlException (this as IXmlLineInfo, error);
			}

			string result = String.Empty;
			if (!IsEmptyElement) {
				Read ();
				result = ReadString ();
				if (NodeType != XmlNodeType.EndElement) {
					string error = String.Format ("'{0}' is an invalid node type.",
								      NodeType.ToString ());
					throw new XmlException (this as IXmlLineInfo, error);
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
				throw new XmlException (this as IXmlLineInfo, error);
			}

			if (localName != LocalName || NamespaceURI != namespaceName) {
				string error = String.Format ("The {0} tag from namespace {1} is expected.",
							      LocalName, NamespaceURI);
				throw new XmlException (this as IXmlLineInfo, error);
			}

			string result = String.Empty;
			if (!IsEmptyElement) {
				Read ();
				result = ReadString ();
				if (NodeType != XmlNodeType.EndElement) {
					string error = String.Format ("'{0}' is an invalid node type.",
								      NodeType.ToString ());
					throw new XmlException (this as IXmlLineInfo, error);
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
				throw new XmlException (this as IXmlLineInfo, error);
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
						throw new XmlException ("Unexpected end of the XML reader.");
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
				throw new XmlException (this as IXmlLineInfo, error);
			}

			Read ();
		}

		public virtual void ReadStartElement (string name)
		{
			if (MoveToContent () != XmlNodeType.Element) {
				string error = String.Format ("'{0}' is an invalid node type.",
							      NodeType.ToString ());
				throw new XmlException (this as IXmlLineInfo, error);
			}

			if (name != Name) {
				string error = String.Format ("The {0} tag from namespace {1} is expected.",
							      Name, NamespaceURI);
				throw new XmlException (this as IXmlLineInfo, error);
			}

			Read ();
		}

		public virtual void ReadStartElement (string localName, string namespaceName)
		{
			if (MoveToContent () != XmlNodeType.Element) {
				string error = String.Format ("'{0}' is an invalid node type.",
							      NodeType.ToString ());
				throw new XmlException (this as IXmlLineInfo, error);
			}

			if (localName != LocalName || NamespaceURI != namespaceName) {
				string error = String.Format ("Expecting {0} tag from namespace {1}, got {2} and {3} instead",
							      localName, namespaceName,
							      LocalName, NamespaceURI);
				throw new XmlException (this as IXmlLineInfo, error);
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
			while (Read() && depth < Depth);
			if (NodeType == XmlNodeType.EndElement)
				Read ();
		}

		#endregion
	}
}
