//
// Outputter.cs
//
// Authors:
//	Oleg Tkachenko (oleg@tkachenko.com)
//	
// (C) 2003 Oleg Tkachenko
//

using System;

namespace Mono.Xml.Xsl {
	/// <summary>
	/// Abstract XSLT outputter. 
	/// Implementations of this class outputs result tree according output method.
	/// </summary>
	public abstract class Outputter {		
		public abstract void WriteStartDocument();			
		public abstract void WriteEndDocument();
		public abstract void WriteStartElement(string localName, string nsURI);
		public abstract void WriteStartElement(string prefix, string localName, string nsURI);
		public abstract void WriteEndElement();
		public abstract void WriteAttributeString(string localName, string value);
		public abstract void WriteAttributeString(string prefix, string localName, string nsURI, string value);
		public abstract void WriteStartAttribute(string localName, string nsURI);
		public abstract void WriteStartAttribute(string prefix, string localName, string nsURI);
		public abstract void WriteEndAttribute();		
		public abstract void WriteComment(string text);
		public abstract void WriteProcessingInstruction(string name, string text);
		public abstract void WriteString(string text);
		public abstract void WriteRaw(string data);
		public abstract void Close();
	}
}
