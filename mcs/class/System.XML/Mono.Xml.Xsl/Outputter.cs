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
	/// Transformation classes build result tree using only with this class API.
	/// Implementations of this class outputs result tree to an Emitter, which emits 
	/// it further down to real consumers.
	/// </summary>
	public abstract class Outputter {
		public abstract void WriteStartDocument ();		
		public abstract void WriteEndDocument ();
		
		public void WriteStartElement (string localName, string nsURI)
		{
			WriteStartElement (null, localName, nsURI);
		}
		
		public abstract void WriteStartElement (string prefix, string localName, string nsURI);
		public abstract void WriteEndElement ();
		public virtual void WriteFullEndElement ()
		{
			WriteEndElement ();
		}
		
		public void WriteAttributeString (string localName, string value)
		{
			WriteAttributeString ("", localName, "", value);
		}
		
		public abstract void WriteAttributeString (string prefix, string localName, string nsURI, string value);
		public abstract void WriteNamespaceDecl (string prefix, string nsUri);		
						
		public abstract void WriteComment (string text);
		
		public abstract void WriteProcessingInstruction (string name, string text);
		
		public abstract void WriteString (string text);
		public abstract void WriteRaw (string data);
		
		public abstract void Done ();

		public virtual bool CanProcessAttributes {
			get { return false; }
		}
	}
}
