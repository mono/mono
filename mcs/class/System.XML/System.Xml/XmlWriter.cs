//
// System.Xml.XmlTextWriter
//
// Author:
//   Kral Ferch <kral_ferch@hotmail.com>
//
// (C) 2002 Kral Ferch
//

using System;

namespace System.Xml
{
	public abstract class XmlWriter
	{
		#region Fields

		protected WriteState ws = WriteState.Start;
		protected XmlNamespaceManager namespaceManager = new XmlNamespaceManager (new NameTable ());

		#endregion

		#region Constructors

		protected XmlWriter () { }

		#endregion

		#region Properties

		public abstract WriteState WriteState { get; }
		
		public abstract string XmlLang { get; }

		public abstract XmlSpace XmlSpace { get; }

		#endregion

		#region Methods

		public abstract void Close ();

		public abstract void Flush ();

		public abstract string LookupPrefix (string ns);

		[MonoTODO("DTDs must be implemented to use 'defattr' parameter.")]
		public virtual void WriteAttributes (XmlReader reader, bool defattr)
		{
			if(reader == null)
				throw new ArgumentException("null XmlReader specified.", "reader");

			switch(reader.NodeType)
			{
				case XmlNodeType.XmlDeclaration:
					// this method doesn't write "<?xml " and "?>", at least MS .NET Framework as yet.
					XmlDeclaration decl = new XmlDeclaration("1.0", String.Empty, String.Empty, null);
					decl.Value = reader.Value;
					if(decl.Version != null && decl.Version != String.Empty) WriteAttributeString("version", decl.Version);
					if(decl.Encoding != null && decl.Encoding != String.Empty) WriteAttributeString("encoding", decl.Encoding);
					if(decl.Standalone != null && decl.Standalone != String.Empty) WriteAttributeString("standalone", decl.Standalone);
					break;
				case XmlNodeType.Element:
					while (reader.MoveToNextAttribute ()) 
					{
						WriteAttributeString(reader.Prefix, reader.LocalName, reader.NamespaceURI, reader.Value);
					}
					break;
				case XmlNodeType.Attribute:
					do
					{
						WriteAttributeString(reader.Prefix, reader.LocalName, reader.NamespaceURI, reader.Value);
					}
					while (reader.MoveToNextAttribute ()) ;
					break;
				default:
					throw new XmlException("NodeType is not one of Element, Attribute, nor XmlDeclaration.");
			}
		}

		public void WriteAttributeString (string localName, string value)
		{
			WriteAttributeString ("", localName, "", value);
		}

		public void WriteAttributeString (string localName, string ns, string value)
		{
			WriteAttributeString ("", localName, ns, value);
		}

		public void WriteAttributeString (string prefix, string localName, string ns, string value)
		{
			if ((prefix == "xmlns") || (localName == "xmlns"))
			  {
				ns = value;
				
				if (prefix == "xmlns" && namespaceManager.HasNamespace (localName))
				  	return;
				
				/* Users need to be able to re-declare the default namespace for subnodes
				else if (localName == "xmlns" && namespaceManager.HasNamespace (String.Empty))
				  	return;
				*/
			  }
			
			WriteStartAttribute (prefix, localName, ns);
			WriteString (value);
			WriteEndAttribute ();

			if ((prefix == "xmlns") || (localName == "xmlns")) 
			{
				if (prefix == "xmlns")
					namespaceManager.AddNamespace (localName, ns);
				else
					namespaceManager.AddNamespace ("", ns);
			}
			
		}

		public abstract void WriteBase64 (byte[] buffer, int index, int count);

		public abstract void WriteBinHex (byte[] buffer, int index, int count);

		public abstract void WriteCData (string text);

		public abstract void WriteCharEntity (char ch);

		public abstract void WriteChars (char[] buffer, int index, int count);

		public abstract void WriteComment (string text);

		public abstract void WriteDocType (string name, string pubid, string sysid, string subset);

		public void WriteElementString (string localName, string value)
		{
			WriteStartElement(localName);
			WriteString(value);
			WriteEndElement();
		}

		public void WriteElementString (string localName, string ns, string value)
		{
			WriteStartElement(localName, ns);
			WriteString(value);
			WriteEndElement();
		}

		public abstract void WriteEndAttribute ();

		public abstract void WriteEndDocument ();

		public abstract void WriteEndElement ();

		public abstract void WriteEntityRef (string name);

		public abstract void WriteFullEndElement ();

		public abstract void WriteName (string name);

		public abstract void WriteNmToken (string name);

		[MonoTODO("needs to test")]
		public virtual void WriteNode (XmlReader reader, bool defattr)
		{
			if (reader == null)
				throw new ArgumentException ();

			if (reader.ReadState == ReadState.Initial) {
				while (reader.Read ())
					WriteNode (reader, defattr);
			}
			else {
				switch (reader.NodeType) {
				case XmlNodeType.Element:
					WriteStartElement (reader.Prefix, reader.LocalName, reader.NamespaceURI);
					WriteAttributes (reader, defattr);
					if (reader.IsEmptyElement)
						WriteEndElement ();
					break;
				case XmlNodeType.Attribute:
					break;
				case XmlNodeType.Text:
					WriteString (reader.Value);
					break;
				case XmlNodeType.CDATA:
					WriteCData (reader.Value);
					break;
				case XmlNodeType.EntityReference:
					WriteEntityRef (reader.Name);
					break;
				case XmlNodeType.ProcessingInstruction:
					WriteProcessingInstruction (reader.Name, reader.Value);
					break;
				case XmlNodeType.Comment:
					WriteComment (reader.Value);
					break;
				case XmlNodeType.DocumentType:
					WriteDocType (reader.Name,
						reader ["PUBLIC"], reader ["SYSTEM"], reader.Value);
					break;
				case XmlNodeType.SignificantWhitespace:
					goto case XmlNodeType.Whitespace;
				case XmlNodeType.Whitespace:
					WriteWhitespace (reader.Value);
					break;
				case XmlNodeType.EndElement:
					break;
				case XmlNodeType.EndEntity:
					break;
				case XmlNodeType.XmlDeclaration:
					WriteStartDocument (reader.GetAttribute  ("standalone").ToLower () == "yes");
					break;
				default:
					throw new NotImplementedException ();
				}
			}
		}

		public abstract void WriteProcessingInstruction (string name, string text);

		public abstract void WriteQualifiedName (string localName, string ns);

		public abstract void WriteRaw (string data);

		public abstract void WriteRaw (char[] buffer, int index, int count);

		public void WriteStartAttribute (string localName, string ns)
		{
			WriteStartAttribute ("", localName, ns);
		}

		public abstract void WriteStartAttribute (string prefix, string localName, string ns);

		public abstract void WriteStartDocument ();

		public abstract void WriteStartDocument (bool standalone);

		public void WriteStartElement (string localName)
		{
			WriteStartElement (null, localName, null);
		}

		public void WriteStartElement (string localName, string ns)
		{
			WriteStartElement (null, localName, ns);
		}

		public abstract void WriteStartElement (string prefix, string localName, string ns);

		public abstract void WriteString (string text);

		public abstract void WriteSurrogateCharEntity (char lowChar, char highChar);

		public abstract void WriteWhitespace (string ws);

		#endregion
	}
}
