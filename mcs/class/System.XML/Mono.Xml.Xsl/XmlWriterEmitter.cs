//
// XmlWriterEmitter.cs
//
// Authors:
//	Oleg Tkachenko (oleg@tkachenko.com)
//	
// (C) 2003 Oleg Tkachenko
//

using System;
using System.Xml;

namespace Mono.Xml.Xsl {
	/// <summary>
	/// Emitter, which emits result tree to a XmlWriter.
	/// </summary>
	public class XmlWriterEmitter : Emitter {
		XmlWriter writer;
			
		public XmlWriterEmitter (XmlWriter writer) {
			this.writer = writer;
		}

		#region # Emitter's methods implementaion			
		
		public override void WriteStartDocument (StandaloneType standalone)
		{
			if (standalone == StandaloneType.NONE)
				writer.WriteStartDocument ();
			else
				writer.WriteStartDocument (standalone == StandaloneType.YES);
		}
		
		public override void WriteEndDocument ()
		{
			writer.WriteEndDocument ();
		}

		public override void WriteDocType (string type, string publicId, string systemId)
		{
			writer.WriteWhitespace (Environment.NewLine);
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
			writer.WriteCData (text);
		}

		public override void Done ()
		{
			writer.Flush ();
		}
		#endregion
	}
}
