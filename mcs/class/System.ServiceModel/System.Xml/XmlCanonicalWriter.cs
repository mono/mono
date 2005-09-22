using System;
using System.IO;

namespace System.Xml
{
	[MonoTODO]
	public abstract class XmlCanonicalWriter
	{
		protected XmlCanonicalWriter ()
		{
			throw new NotImplementedException ();
		}

		public abstract void Close ();

		public abstract void Flush ();

		public abstract void WriteBase64 (byte [] buffer, int index, int count);

		public abstract void WriteCharEntity (int ch);

		public abstract void WriteComment (string text);

		public abstract void WriteComment (byte [] data, int offset, int count);

		public abstract void WriteDeclaration ();

		public abstract void WriteEndAttribute ();

		public abstract void WriteEndElement (string prefix, string localName);

		public abstract void WriteEndElement (byte [] prefix, int offset1, int count1, byte [] localName, int offset2, int count2);

		public abstract void WriteEndStartElement (bool isEmpty);

		public abstract void WriteEscapedText (string text);

		public abstract void WriteEscapedText (byte [] text, int offset, int count);

		public abstract void WriteNode (XmlReader reader);

		public abstract void WriteStartAttribute (string prefix, string localName);

		public abstract void WriteStartAttribute (byte [] prefix, int offset1, int count1, byte [] localName, int offset2, int count2);

		public abstract void WriteStartElement (string prefix, string localName);

		public abstract void WriteStartElement (byte [] prefix, int offset1, int count1, byte [] localName, int offset2, int count2);

		public abstract void WriteText (string text);

		public abstract void WriteText (byte [] text, int offset, int count);

		public abstract void WriteText (int ch);

		public abstract void WriteXmlnsAttribute (
			string prefix, string namespaceUri);

		public abstract void WriteXmlnsAttribute (byte [] prefix, int offset1, int count1, byte [] namespaceUri, int offset2, int count2);
	}
}
