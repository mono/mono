//
// Emitter.cs
//
// Authors:
//	Oleg Tkachenko (oleg@tkachenko.com)
//	
// (C) 2003 Oleg Tkachenko
//

using System;

namespace Mono.Xml.Xsl {
	/// <summary>
	/// Abstract emitter. Implementations of this class deals with outputting
	/// result tree to specific output format, such as XML, HTML, Text. 
	/// Implementations for additional formats (e.g. XHTML) as well as custom 
	/// implementations may be supported either.
	/// </summary>
	public abstract class Emitter {
		public abstract void WriteStartDocument (StandaloneType standalone);		
		public abstract void WriteEndDocument ();						
		public abstract void WriteDocType (string type, string publicId, string systemId);
		public abstract void WriteStartElement (string prefix, string localName, string nsURI);
		public abstract void WriteEndElement ();
		public virtual void WriteFullEndElement ()
		{
			WriteEndElement ();
		}

		public abstract void WriteAttributeString (string prefix, string localName, string nsURI, string value);					
		public abstract void WriteComment (string text);		
		public abstract void WriteProcessingInstruction (string name, string text);		
		public abstract void WriteString (string text);
		public abstract void WriteCDataSection (string text);
		public abstract void WriteRaw (string data);
		public abstract void Done ();
	}
}
