//
// XmlWriterEmitter.cs
//
// Authors:
//	Oleg Tkachenko (oleg@tkachenko.com)
//	Atsushi Enomoto (atsushi@ximian.com)
//	
// (C) 2003 Oleg Tkachenko
// (C) 2004 Atsushi Enomoto
//

using System;
using System.Text;
using System.Xml;

namespace Mono.Xml.Xsl 
{
	/// <summary>
	/// Emitter, which emits result tree to a XmlWriter.
	/// </summary>
	public class XmlWriterEmitter : Emitter 
	{
		XmlWriter writer;
			
		public XmlWriterEmitter (XmlWriter writer) {
			this.writer = writer;
		}

		#region # Emitter's methods implementaion			
		
		public override void WriteStartDocument (Encoding encoding, StandaloneType standalone)
		{
#if docent
			if (standalone == StandaloneType.NONE)
				writer.WriteStartDocument ();
			else
				writer.WriteStartDocument (standalone == StandaloneType.YES);
#else
			string standaloneStr = "";
			switch (standalone) {
			case StandaloneType.YES:
				standaloneStr = " standalone=\"yes\"";
				break;
			case StandaloneType.NO:
				standaloneStr = " standalone=\"no\"";
				break;
			}

			if (encoding == null)
				writer.WriteProcessingInstruction ("xml", "version=\"1.0\"" + standaloneStr);
			else
				writer.WriteProcessingInstruction ("xml", 
					"version=\"1.0\" encoding=\"" 
					+ encoding.WebName + "\"" 
					+ standaloneStr);
#endif
		}
		
		public override void WriteEndDocument ()
		{
#if docent
			writer.WriteEndDocument ();
#endif
		}

		public override void WriteDocType (string type, string publicId, string systemId)
		{
			if (publicId != null && publicId != String.Empty &&
				(systemId == null || systemId == String.Empty))
				// This is an error.
				// when PUBLIC id exists, SYSTEM id is required.
				return;
			writer.WriteDocType (type, publicId, systemId, null);
		}

		public override void WriteStartElement (string prefix, string localName, string nsURI)
		{
			writer.WriteStartElement (prefix, localName, nsURI);
		}

		public override void WriteEndElement ()
		{
			writer.WriteEndElement ();
		}

		public override void WriteFullEndElement ()
		{
			writer.WriteFullEndElement ();
		}

		public override void WriteAttributeString (string prefix, string localName, string nsURI, string value)
		{
			writer.WriteAttributeString (prefix, localName, nsURI, value);
		}		

		public override void WriteComment (string text) {
			if (text.IndexOf ("--") >= 0)
				text = text.Replace ("--", "- -");

			if (text.EndsWith ("-"))
				writer.WriteComment (text + ' ');
			else
				writer.WriteComment (text);
		}

		public override void WriteProcessingInstruction (string name, string text)
		{
			writer.WriteProcessingInstruction (name, text);
		}

		public override void WriteString (string text)
		{
			writer.WriteString (text);
		}

		public override void WriteRaw (string data)
		{
			writer.WriteRaw (data);
		}

		public override void WriteCDataSection (string text)
		{
			int index = text.IndexOf ("]]>");
			if (index >= 0) {
				writer.WriteCData (text.Substring (0, index + 2));
				WriteCDataSection (text.Substring (index + 2));
			} else 
				writer.WriteCData (text);
		}

		public override void WriteWhitespace (string value)
		{
			writer.WriteWhitespace (value);
		}

		public override void Done ()
		{
			writer.Flush ();
		}
		#endregion
	}
}
