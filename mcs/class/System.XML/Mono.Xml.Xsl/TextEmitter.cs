//
// TextEmitter.cs
//
// Authors:
//	Oleg Tkachenko (oleg@tkachenko.com)
//	
// (C) 2003 Oleg Tkachenko
//

using System;
using System.IO;

namespace Mono.Xml.Xsl {
	/// <summary>
	/// Emitetr, which emits result tree according to "text" output method.
	/// </summary>
	public class TextEmitter : Emitter {
		TextWriter writer;
			
		public TextEmitter (TextWriter writer) {
			this.writer = writer;
		}

		#region # Emitter's methods implementaion			
		
		public override void WriteStartDocument (StandaloneType standalone) {
			//Do nothing
		}
		
		public override void WriteEndDocument () {
			//Do nothing
		}

		public override void WriteDocType (string type, string publicId, string systemId) {
			//Do nothing
		}

		public override void WriteStartElement (string prefix, string localName, string nsURI) {
			//Do nothing
		}

		public override void WriteEndElement () {
			//Do nothing
		}

		public override void WriteAttributeString (string prefix, string localName, string nsURI, string value) {
			//Do nothing
		}
		
		public override void WriteComment (string text) {
			//Do nothing
		}

		public override void WriteProcessingInstruction (string name, string text) {
			//Do nothing
		}

		public override void WriteString (string text) {
			writer.Write (text);
		}

		public override void WriteRaw (string data) {
			writer.Write (data);
		}

		public override void WriteCDataSection (string text) {
			writer.Write (text);
		}

		public override void Done () {
			//Do nothing
		}
		#endregion
	}
}
