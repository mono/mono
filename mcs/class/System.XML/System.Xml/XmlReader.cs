//
// XmlReader.cs
//
// Authors:
// 	Jason Diamond (jason@injektilo.org)
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2001, 2002 Jason Diamond  http://injektilo.org/
// (c) 2002 Ximian, Inc. (http://www.ximian.com)
//
using System.Text;

namespace System.Xml
{
	public abstract class XmlReader
	{
		private StringBuilder readStringBuffer;

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

		public abstract bool IsDefault { get; }

		public abstract bool IsEmptyElement { get; }

		public abstract string this[int i] { get; }

		public abstract string this[string name] { get; }

		public abstract string this[
			string localName,
			string namespaceName]
		{ get; }

		public abstract string LocalName { get; }

		public abstract string Name { get; }

		public abstract string NamespaceURI { get; }

		public abstract XmlNameTable NameTable { get; }

		public abstract XmlNodeType NodeType { get; }

		public abstract string Prefix { get; }

		public abstract char QuoteChar { get; }

		public abstract ReadState ReadState { get; }

		public abstract string Value { get; }

		public abstract string XmlLang { get; }

		public abstract XmlSpace XmlSpace { get; }

		#endregion

		#region Methods

		public abstract void Close ();

		public abstract string GetAttribute (int i);

		public abstract string GetAttribute (string name);

		public abstract string GetAttribute (
			string localName,
			string namespaceName);

		public static bool IsName (string s)
		{
			bool result = false;

			if (s != null && s.Length > 0) {
				char[] chars = s.ToCharArray ();

				if (XmlConstructs.IsNameStart (chars[0])) {
					int i = 1;
					int n = chars.Length;

					while (i < n && XmlConstructs.IsName (chars[i]))
						++i;

					result = i == n;
				}
			}

			return result;
		}

		public static bool IsNameToken (string s)
		{
			bool result = false;

			if (s != null && s.Length > 0) {
				char[] chars = s.ToCharArray ();

				int i = 0;
				int n = chars.Length;

				while (i < n && XmlConstructs.IsName (chars[i]))
					++i;

				result = i == n;
			}

			return result;
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

		public abstract string ReadInnerXml ();

		public abstract string ReadOuterXml ();

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
				string error = String.Format ("The {0} tag from namespace {1} is expected.",
							      LocalName, NamespaceURI);
				throw new XmlException (this as IXmlLineInfo, error);
			}

			Read ();
		}

		public abstract string ReadString ();

		internal protected string ReadStringInternal ()
		{
			if (readStringBuffer == null)
				readStringBuffer = new StringBuilder ();
			readStringBuffer.Length = 0;

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
