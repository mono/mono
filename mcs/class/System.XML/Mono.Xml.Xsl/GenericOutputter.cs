//
// GenericOutputter.cs
//
// Authors:
//	Oleg Tkachenko (oleg@tkachenko.com)
//	Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//	
// (C) 2003 Oleg Tkachenko, Atsushi Enomoto
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
		// Collection of pending attributes. TODO: Can we make adding an attribute
		// O(1)? I'm not sure it is that important (this would only really make a difference
		// if elements had like 10 attributes, which is very rare).
		Attribute [] pendingAttributes = new Attribute [10];
		int pendingAttributesPos = 0;
		//Namespace manager. Subject to optimization.
		private XmlNamespaceManager _nsManager;
		private ArrayList _currentNsPrefixes;
		private Hashtable _currentNamespaceDecls;
		//Name table
		private NameTable _nt;
		//Determines whether xsl:copy can output attribute-sets or not.
		bool canProcessAttributes;
		// FIXME: This is quick hack to eliminate XML declaration for HTML output.
		bool htmlEmulation;

		private GenericOutputter (Hashtable outputs)
		{
			_outputs = outputs;
			_currentOutput = (XslOutput)outputs [String.Empty];
			_state = WriteState.Start;
			//TODO: Optimize using nametable
			_nt = new NameTable ();
			_nsManager = new XmlNamespaceManager (_nt);
			_currentNsPrefixes = new ArrayList ();
			_currentNamespaceDecls = new Hashtable ();
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
				
				case OutputMethod.HTML:
					_emitter = new HtmlEmitter (writer, xslOutput);
					break;
				case OutputMethod.Unknown: //TODO: handle xml vs html
				case OutputMethod.XML:
					XmlTextWriter w = new XmlTextWriter (writer);
					if (xslOutput.Indent == "yes")
						w.Formatting = Formatting.Indented;
					_emitter = new XmlWriterEmitter (w);					
					break;
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
			if (_state == WriteState.Start)
				WriteStartDocument ();

			if (_state == WriteState.Element) {
				//Push scope to allow to unwind namespaces scope back in WriteEndElement
				//Subject to optimization - avoid redundant push/pop by moving 
				//namespaces to WriteStartElement
				_nsManager.PushScope ();
				//Emit pending attributes
				for (int i = 0; i < pendingAttributesPos; i++) {
					Attribute attr = pendingAttributes [i];
					string prefix = _nsManager.LookupPrefix (attr.Namespace);
					if (prefix == null)
						prefix = attr.Prefix;
					_emitter.WriteAttributeString (prefix, attr.LocalName, attr.Namespace, attr.Value);
				}
				foreach (string prefix in _currentNsPrefixes) {
					string uri = _currentNamespaceDecls [prefix] as string;
					if (prefix != String.Empty)
						_emitter.WriteAttributeString ("xmlns", prefix, XmlNamespaceManager.XmlnsXmlns, uri);
					else
						_emitter.WriteAttributeString (String.Empty, "xmlns", XmlNamespaceManager.XmlnsXmlns, uri);
				}
				_currentNsPrefixes.Clear ();
				_currentNamespaceDecls.Clear ();
				//Attributes flushed, state is Content now				
				_state = WriteState.Content;
			}
			canProcessAttributes = false;
		}

		#region Outputter's methods implementation
		
		public override void WriteStartDocument ()
		{			
			if (!_currentOutput.OmitXmlDeclaration && !htmlEmulation)
				_emitter.WriteStartDocument (_currentOutput.Standalone);
			
			_state = WriteState.Prolog;
		}
		
		public override void WriteEndDocument ()
		{
			_emitter.WriteEndDocument ();				
		}

		int _nsCount;
		public override void WriteStartElement (string prefix, string localName, string nsURI)
		{
			if (_state == WriteState.Start)
				WriteStartDocument ();

			if (_state == WriteState.Prolog) {
				//Seems to be the first element - take care of Doctype
				if (_currentOutput.DoctypeSystem != null)
					_emitter.WriteDocType (prefix + (prefix==null? ":" : "") + localName, 
						_currentOutput.DoctypePublic, _currentOutput.DoctypeSystem);
			}
			CheckState ();
			_emitter.WriteStartElement (prefix, localName, nsURI);
			_state = WriteState.Element;						
			pendingAttributesPos = 0;
			canProcessAttributes = true;
		}

		public override void WriteEndElement ()
		{
			WriteEndElementInternal (false);
		}

		public override void WriteFullEndElement()
		{
			WriteEndElementInternal (true);
		}

		private void WriteEndElementInternal (bool fullEndElement)
		{
			CheckState ();
			if (fullEndElement)
				_emitter.WriteFullEndElement ();
			else
				_emitter.WriteEndElement ();
			_state = WriteState.Content;
			//Pop namespace scope
			_nsManager.PopScope ();
		}

		public override void WriteAttributeString (string prefix, string localName, string nsURI, string value)
		{
			if (prefix == String.Empty && nsURI != String.Empty) {
				prefix = "xp_" + _nsCount;
				_nsManager.AddNamespace (prefix, nsURI);
				_currentNsPrefixes.Add (prefix);
				_currentNamespaceDecls.Add (prefix, nsURI);
				_nsCount++;
			}

			//Put attribute to pending attributes collection, replacing namesake one
			for (int i = 0; i < pendingAttributesPos; i++) {
				Attribute attr = pendingAttributes [i];
				
				if (attr.LocalName == localName && attr.Namespace == nsURI) {
					pendingAttributes [i].Value = value;
					//Keep prefix (e.g. when literal attribute is overriden by xsl:attribute)
					if (attr.Prefix == String.Empty && prefix != String.Empty)
						pendingAttributes [i].Prefix = prefix;
					return;
				}
			}
			
			if (pendingAttributesPos == pendingAttributes.Length) {
				Attribute [] old = pendingAttributes;
				pendingAttributes = new Attribute [pendingAttributesPos * 2 + 1];
				if (pendingAttributesPos > 0)
					Array.Copy (old, 0, pendingAttributes, 0, pendingAttributesPos);
			}
			pendingAttributes [pendingAttributesPos].Prefix = prefix;
			pendingAttributes [pendingAttributesPos].Namespace = nsURI;
			pendingAttributes [pendingAttributesPos].LocalName = localName;
			pendingAttributes [pendingAttributesPos].Value = value;
			pendingAttributesPos++;
		}

		public override void WriteNamespaceDecl (string prefix, string nsUri)
		{
			if (prefix == String.Empty) {
				//Default namespace
				if (_nsManager.DefaultNamespace != nsUri)
					_nsManager.AddNamespace (prefix, nsUri);
			} else if (_nsManager.LookupPrefix (nsUri) == null)
				//That's new namespace - add it to the collection
				_nsManager.AddNamespace (prefix, nsUri);

			if (_currentNamespaceDecls [prefix] as string != nsUri) {
				_currentNsPrefixes.Add (prefix);
				_currentNamespaceDecls [prefix] = nsUri;
			}
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

		public override bool CanProcessAttributes {
			get { return canProcessAttributes; }
		}

		public override WriteState WriteState { get { return _state; } }
		#endregion
	}
}
