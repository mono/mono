//
// GenericOutputter.cs
//
// Authors:
//	Oleg Tkachenko (oleg@tkachenko.com)
//	
// (C) 2003 Oleg Tkachenko
//

using System;
using System.Collections;
using System.Xml;
using System.IO;

namespace Mono.Xml.Xsl
{
	/// <summary>
	/// Generic implemenatation of the Outputter.
	/// Works as a buffer between Transformation classes and an Emitter.
	/// Implements attributes dublicate checking, nemaspace stuff and
	/// choosing of right Emitter implementation.
	/// </summary>
	public class GenericOutputter : Outputter {	
		private Hashtable _outputs;
		//Current xsl:output 
		private XslOutput _currentOutput;
		//Underlying emitter
		private Emitter _emitter;
		//Outputting state
		private WriteState _state;
		//Collection of pending attributes. Key is attribute's QName, 
		//value is Attribute object, which encapsulates prefix, QName and string
		//value of the attribute. Subject to optimization.
		private Hashtable _pendingAttrs;
		//Namespace manager. Subject to optimization.
		private XmlNamespaceManager _nsManager;
		//Name table
		private NameTable _nt;
		
		private GenericOutputter (Hashtable outputs)
		{
			_outputs = outputs;
			_currentOutput = (XslOutput)outputs [String.Empty];
			_state = WriteState.Start;
			_pendingAttrs = new Hashtable ();
			//TODO: Optimize using nametable
			_nt = new NameTable ();
			_nsManager = new XmlNamespaceManager (_nt);
		}

		public GenericOutputter (XmlWriter writer, Hashtable outputs) 
			: this (outputs)
		{
			_emitter = new XmlWriterEmitter (writer);			
		}

		public GenericOutputter (TextWriter writer, Hashtable outputs)
			: this (outputs)
		{			
			XslOutput xslOutput = (XslOutput)outputs [String.Empty];
			switch (xslOutput.Method) {
				case OutputMethod.Unknown: //TODO: handle xml vs html
				case OutputMethod.XML:
					//TODO: XmlTextEmitter goes here
					//_emitter = new XmlTextEmitter (writer);
					_emitter = new XmlWriterEmitter (new XmlTextWriter (writer));					
					break;
				case OutputMethod.HTML:
					throw new NotImplementedException ("HTML output method is not implemented yet.");
				case OutputMethod.Text:
					_emitter = new TextEmitter (writer);
					break;
				case OutputMethod.Custom:
					throw new NotImplementedException ("Custom output method is not implemented yet.");
			}						
		}

		/// <summary>
		/// Checks output state and flushes pending attributes and namespaces 
		/// when it's appropriate.
		/// </summary>
		private void CheckState ()
		{
		// this isnt being called at the right place
		// <a (1)> <b /> (2)</a>
		// This should be called at 1, but is really being called at 2.
		#if false
			if (_state == WriteState.Element) {
				//Push scope to allow to unwind namespaces scope back in WriteEndElement
				//Subject to optimization - avoid redundant push/pop by moving 
				//namespaces to WriteStartElement
				_nsManager.PushScope ();
				//Emit pending attributes
				foreach (XmlQualifiedName qName in _pendingAttrs.Keys) {
					Attribute attr = (Attribute)_pendingAttrs [qName];
					_emitter.WriteAttributeString (attr.Prefix, qName.Name, qName.Namespace, attr.Value);
				}					
			}
		#endif
		}

		#region Outputter's methods implementation
		
		public override void WriteStartDocument ()
		{			
			if (!_currentOutput.OmitXmlDeclaration)
				_emitter.WriteStartDocument (_currentOutput.Standalone);
			
			_state = WriteState.Prolog;
		}
		
		public override void WriteEndDocument ()
		{
			_emitter.WriteEndDocument ();				
		}

		public override void WriteStartElement (string prefix, string localName, string nsURI)
		{
			if (_state == WriteState.Prolog) {
				//Seems to be the first element - take care of Doctype
				if (_currentOutput.DoctypeSystem != null)
					_emitter.WriteDocType (prefix + (prefix==null? ":" : "") + localName, 
						_currentOutput.DoctypePublic, _currentOutput.DoctypeSystem);
			}
			CheckState ();
			_emitter.WriteStartElement (prefix, localName, nsURI);
			_state = WriteState.Element;						
			_pendingAttrs.Clear ();
		}

		public override void WriteEndElement ()
		{
			CheckState ();
			_emitter.WriteEndElement ();
			_state = WriteState.Content;
			//Pop namespace scope
			_nsManager.PopScope ();
		}

		public override void WriteAttributeString (string prefix, string localName, string nsURI, string value)
		{
			
			_emitter.WriteAttributeString (prefix, localName, nsURI, value);
		
		// See CheckState for why i commented this out
		#if false
			//Put attribute to pending attributes collection, replacing namesake one
		 	XmlQualifiedName qName = new XmlQualifiedName (localName, nsURI);
		 	Attribute attr = (Attribute)_pendingAttrs [qName];
		 	if (attr == null) {
		 		attr = new Attribute (prefix, qName, value);
		 		_pendingAttrs.Add (qName, attr);
		 	} else {
		 		attr.Value = value;
		 		//Keep prefix (e.g. when literal attribute is overriden by xsl:attribute)
		 		if (attr.Prefix == String.Empty && prefix != String.Empty)
		 			attr.Prefix = prefix;
		 	}
		#endif
		}

		public override void WriteNamespaceDecl (string prefix, string nsUri)
		{
			if (prefix == String.Empty) {
				//Default namespace
				if (_nsManager.DefaultNamespace != nsUri) {
					_nsManager.AddNamespace (prefix, nsUri);
					_emitter.WriteAttributeString ("", "xmlns", "", nsUri);
				}
			} else if (_nsManager.LookupPrefix (nsUri) == null) {
				//That's new namespace - add it to the collection and emit
				_nsManager.AddNamespace (prefix, nsUri);
				_emitter.WriteAttributeString ("xmlns", prefix, null, nsUri);
			}			
		}
			 
		public override void WriteStartAttribute (string prefix, string localName, string nsURI)
		{
			_emitter.WriteStartAttribute (prefix, localName, nsURI);
			_state = WriteState.Attribute;
		}

		public override void WriteEndAttribute ()
		{
			_emitter.WriteEndAttribute ();
			_state = WriteState.Element;
		}

		public override void WriteComment (string text)
		{
			CheckState ();
			_emitter.WriteComment (text);
		}

		public override void WriteProcessingInstruction (string name, string text)
		{
			CheckState ();
			_emitter.WriteProcessingInstruction (name, text);
		}

		public override void WriteString (string text)
		{
			CheckState ();
			_emitter.WriteString (text);
		}

		public override void WriteRaw (string data)
		{
			CheckState ();
			_emitter.WriteRaw (data);
		}

		public override void Done ()
		{
			_emitter.Done ();
			_state = WriteState.Closed;
		}
		#endregion
	}
}
