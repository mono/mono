using System;
using System.IO;

namespace System.Xml
{
	public abstract class XmlDictionaryWriter : XmlShimWriter
	{
		protected XmlDictionaryWriter ()
		{
		}

		public bool CanCanonicalize {
			get { return false; }
		}

		public bool CanFragment {
			get { return false; }
		}

		// FIXME: add a bunch of factory methods

		public void EndCanonicalization ()
		{
			throw new NotSupportedException ();
		}

		public void EndFragment ()
		{
			throw new NotSupportedException ();
		}

		public void StartCanonicalization (XmlCanonicalWriter writer)
		{
			throw new NotSupportedException ();
		}

		public void StartFragment (Stream stream)
		{
			throw new NotSupportedException ();
		}

		// FIXME: add Write*Array() overloads.

		public void WriteAttributeString (
			XmlDictionaryString localName,
			XmlDictionaryString namespaceUri,
			string value)
		{
			throw new NotSupportedException ();
		}

		public void WriteAttributeString (string prefix,
			XmlDictionaryString localName,
			XmlDictionaryString namespaceUri,
			string value)
		{
			throw new NotSupportedException ();
		}

		public void WriteElementString (
			XmlDictionaryString localName,
			XmlDictionaryString namespaceUri,
			string value)
		{
			throw new NotSupportedException ();
		}

		public void WriteElementString (string prefix,
			XmlDictionaryString localName,
			XmlDictionaryString namespaceUri,
			string value)
		{
			throw new NotSupportedException ();
		}

		public void WriteFragment (byte [] buffer, int offset,
			int count)
		{
			throw new NotSupportedException ();
		}

		public void WriteNode (XmlDictionaryReader reader,
			bool defattr)
		{
			throw new NotSupportedException ();
		}

		public void WriteQualifiedName (
			XmlDictionaryString localName,
			XmlDictionaryString namespaceUri)
		{
			throw new NotSupportedException ();
		}

		public void WriteStartAttribute (
			XmlDictionaryString localName,
			XmlDictionaryString namespaceUri)
		{
			throw new NotSupportedException ();
		}

		public void WriteStartAttribute (string prefix,
			XmlDictionaryString localName,
			XmlDictionaryString namespaceUri)
		{
			throw new NotSupportedException ();
		}

		public void WriteStartElement (
			XmlDictionaryString localName,
			XmlDictionaryString namespaceUri)
		{
			throw new NotSupportedException ();
		}

		public void WriteStartElement (string prefix,
			XmlDictionaryString localName,
			XmlDictionaryString namespaceUri)
		{
			throw new NotSupportedException ();
		}

		public void WriteString (XmlDictionaryString value)
		{
			throw new NotSupportedException ();
		}

		public void WriteValue (UniqueId id)
		{
			throw new NotSupportedException ();
		}

		public void WriteValue (XmlDictionaryString value)
		{
			throw new NotSupportedException ();
		}

		public void WriteXmlAttribute (string localName, string value)
		{
			throw new NotSupportedException ();
		}

		public void WriteXmlAttribute (XmlDictionaryString localName,
			XmlDictionaryString value)
		{
			throw new NotSupportedException ();
		}

		public void WriteXmlnsAttribute (
			string prefix, string namespaceUri)
		{
			throw new NotSupportedException ();
		}

		public void WriteXmlnsAttribute (string prefix,
			XmlDictionaryString namespaceUri)
		{
			throw new NotSupportedException ();
		}
	}
}
