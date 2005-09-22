#if NET_2_0
using System;
using System.IO;

namespace System.Xml
{
	[MonoTODO]
	public class XmlC14NWriter : XmlCanonicalWriter
	{
		bool include_comments;

		public XmlC14NWriter (Stream stream)
		{
			throw new NotImplementedException ();
		}

		public bool IncludeComments {
			get { return include_comments; }
			set {
				throw new NotImplementedException ();
			}
		}

		public override void Close ()
		{
			Flush ();
		}

		public override void Flush ()
		{
			throw new NotImplementedException ();
		}

		public virtual void SetOutput (Stream stream)
		{
			throw new NotImplementedException ();
		}

		public override void WriteBase64 (byte [] buffer, int index, int count)
		{
			throw new NotImplementedException ();
		}

		public override void WriteCharEntity (int ch)
		{
			throw new NotImplementedException ();
		}

		public override void WriteComment (string text)
		{
			throw new NotImplementedException ();
		}

		public override void WriteComment (byte [] data, int offset, int count)
		{
			throw new NotImplementedException ();
		}

		public override void WriteDeclaration ()
		{
			throw new NotImplementedException ();
		}

		public override void WriteEndAttribute ()
		{
			throw new NotImplementedException ();
		}

		public override void WriteEndElement (string prefix, string localName)
		{
			throw new NotImplementedException ();
		}

		public override void WriteEndElement (byte [] prefix, int offset1, int count1, byte [] localName, int offset2, int count2)
		{
			throw new NotImplementedException ();
		}

		public override void WriteEndStartElement (bool isEmpty)
		{
			throw new NotImplementedException ();
		}

		public override void WriteEscapedText (string text)
		{
			throw new NotImplementedException ();
		}

		public override void WriteEscapedText (byte [] text, int offset, int count)
		{
			throw new NotImplementedException ();
		}

		public override void WriteNode (XmlReader reader)
		{
			throw new NotImplementedException ();
		}

		public override void WriteStartAttribute (string prefix, string localName)
		{
			throw new NotImplementedException ();
		}

		public override void WriteStartAttribute (byte [] prefix, int offset1, int count1, byte [] localName, int offset2, int count2)
		{
			throw new NotImplementedException ();
		}

		public override void WriteStartElement (string prefix, string localName)
		{
			throw new NotImplementedException ();
		}

		public override void WriteStartElement (byte [] prefix, int offset1, int count1, byte [] localName, int offset2, int count2)
		{
			throw new NotImplementedException ();
		}

		public override void WriteText (string text)
		{
			throw new NotImplementedException ();
		}

		public override void WriteText (int ch)
		{
			throw new NotImplementedException ();
		}

		public override void WriteText (byte [] text, int offset, int count)
		{
			throw new NotImplementedException ();
		}

		public override void WriteXmlnsAttribute (
			string prefix, string namespaceUri)
		{
			throw new NotImplementedException ();
		}

		public override void WriteXmlnsAttribute (byte [] prefix, int offset1, int count1, byte [] namespaceUri, int offset2, int count2)
		{
			throw new NotImplementedException ();
		}
	}
}
#endif
