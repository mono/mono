using System;

namespace System.Xml
{
	/// <summary>
	/// 
	/// </summary>
	/// 

	/*
	 * Section 2.5 of the XML spec says...
	[Definition: Comments may appear anywhere in a document outside other markup; 	in addition, they may appear within the document type declaration at places	allowed by the grammar. They are not part of the document's character data; 	an XML processor may, but need not, make it possible for an application to retrieve 	the text of comments. For compatibility, the string "--" (double-hyphen)	must not occur within comments.] Parameter entity references are not recognized	within comments.
	
	Note that the grammar does not allow a comment ending in --->.
	Comment ::=   '<!--' ((Char - '-') | ('-' (Char	- '-')))* '-->'
	*/
	public class XmlComment : XmlCharacterData
	{
		// Private data members

		// public properties
		public override string InnerText 
		{
			get
			{
				// TODO - implement XmlComment.InnerText.get
				throw new NotImplementedException();
			}
			
			set
			{
				// TODO - implement XmlComment.InnerText.set
				throw new NotImplementedException();
			}
		}

		public override string LocalName 
		{
			get
			{
				return "#comment";
			}
		}

		public override string Name 
		{
			get
			{
				return "#comment";
			}
		}

		public override string Value 
		{
			get
			{
				return Fvalue;
			}
			
			set
			{
				// TODO - Do our well-formedness checks on Value.set? (no)
				Fvalue = value;
			}
		}
		

		// Public Methods
		public override XmlNode CloneNode(bool deep)
		{
			// TODO - implement XmlComment.CloneNode(bool)
			throw new NotImplementedException();
		}

		public override void WriteContentTo(XmlWriter w)
		{
			// TODO - implement XmlComment.WriteContentTo(XmlWriter)
			throw new NotImplementedException();
		}

		public override void WriteTo(XmlWriter w)
		{
			// TODO - implement XmlComment.WriteTo(XmlWriter)
			throw new NotImplementedException();
		}


		// Internal methods
		/// <summary>
		/// Returns an exception object if passed text is not well-formed.
		/// Text is passed without introductory syntax elements.
		/// For comments, the leading "<!--" and trailing "-->" should be stripped.
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		private XmlException wellFormed(string data, XmlInputSource src)
		{
			if (data.IndexOf("--") != -1)
				return new XmlException("Invalid characters (\"--\") in comment", src);
			if (data[0] == '-')
				return new XmlException("Invalid comment beginning (<!---)", src);
			if (data[data.Length - 1] == '-')
				return new XmlException("Invalid comment ending (--->)", src);
			return null;

		}
		// Constructors
		internal XmlComment(XmlDocument aOwner, string txt, XmlInputSource src) : base(txt, aOwner)
		{
			XmlException e = wellFormed(txt, src);

			if ( e == null )
			{
				Fvalue = txt;
			}
			else
				throw e;
		}
	}
}
