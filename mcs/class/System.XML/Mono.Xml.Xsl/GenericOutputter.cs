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
using System.Text;

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
		// destination TextWriter,
		// which is pended until the actual output is determined.
		private TextWriter pendingTextWriter;
		// also, whitespaces before the first element are cached.
		StringBuilder pendingFirstSpaces;
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
		// Specified encoding (for TextWriter output)
		Encoding _encoding;
		//Determines whether xsl:copy can output attribute-sets or not.
		bool _canProcessAttributes;
		bool _insideCData;
		bool _isVariable;

		private GenericOutputter (Hashtable outputs, Encoding encoding)
		{
			_encoding = encoding;
			_outputs = outputs;
			_currentOutput = (XslOutput)outputs [String.Empty];
			_state = WriteState.Start;
			//TODO: Optimize using nametable
			_nt = new NameTable ();
			_nsManager = new XmlNamespaceManager (_nt);
			_currentNsPrefixes = new ArrayList ();
			_currentNamespaceDecls = new Hashtable ();
		}

		public GenericOutputter (XmlWriter writer, Hashtable outputs, Encoding encoding) 
			: this (writer, outputs, encoding, false)
		{
		}

		internal GenericOutputter (XmlWriter writer, Hashtable outputs, Encoding encoding, bool isVariable)
			: this (outputs, encoding)
		{
			_emitter = new XmlWriterEmitter (writer);
			_state = writer.WriteState;
			_isVariable = isVariable;
		}

		public GenericOutputter (TextWriter writer, Hashtable outputs, Encoding encoding)
			: this (outputs, encoding)
		{
			this.pendingTextWriter = writer;
		}

		private Emitter Emitter {
			get {
				if (_emitter == null)
					DetermineOutputMethod (null, null);
				return _emitter;
			}
		}

		private void DetermineOutputMethod (string localName, string ns)
		{
			XslOutput xslOutput = (XslOutput)_outputs [String.Empty];
			switch (xslOutput.Method) {
				case OutputMethod.Unknown: //TODO: handle xml vs html
					if (localName != null && localName.ToLower () == "html" && ns == String.Empty)
						goto case OutputMethod.HTML;
					goto case OutputMethod.XML;
				case OutputMethod.HTML:
					_emitter = new HtmlEmitter (pendingTextWriter, xslOutput);
					break;
				case OutputMethod.XML:
					XmlTextWriter w = new XmlTextWriter (pendingTextWriter);
					if (xslOutput.Indent == "yes")
						w.Formatting = Formatting.Indented;
					_emitter = new XmlWriterEmitter (w);					
					break;
				case OutputMethod.Text:
					_emitter = new TextEmitter (pendingTextWriter);
					break;
				case OutputMethod.Custom:
					throw new NotImplementedException ("Custom output method is not implemented yet.");
			}
			pendingTextWriter = null;
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
					string prefix = attr.Prefix;
					if (prefix == String.Empty)
						prefix = _nsManager.LookupPrefix (attr.Namespace);
					Emitter.WriteAttributeString (prefix, attr.LocalName, attr.Namespace, attr.Value);
				}
				foreach (string prefix in _currentNsPrefixes) {
					string uri = _currentNamespaceDecls [prefix] as string;
					if (prefix != String.Empty)
						Emitter.WriteAttributeString ("xmlns", prefix, XmlNamespaceManager.XmlnsXmlns, uri);
					else
						Emitter.WriteAttributeString (String.Empty, "xmlns", XmlNamespaceManager.XmlnsXmlns, uri);
				}
				_currentNsPrefixes.Clear ();
				_currentNamespaceDecls.Clear ();
				//Attributes flushed, state is Content now				
				_state = WriteState.Content;
			}
			_canProcessAttributes = false;
		}

		#region Outputter's methods implementation
		
		public override void WriteStartDocument ()
		{
			if (_isVariable)
				return;

			if (!_currentOutput.OmitXmlDeclaration)
				Emitter.WriteStartDocument (_encoding != null ? _encoding : _currentOutput.Encoding, _currentOutput.Standalone);
			
			_state = WriteState.Prolog;
		}
		
		public override void WriteEndDocument ()
		{
			Emitter.WriteEndDocument ();				
		}

		int _nsCount;
		public override void WriteStartElement (string prefix, string localName, string nsURI)
		{
			if (_emitter == null) {
				this.DetermineOutputMethod (localName, nsURI);
				if (pendingFirstSpaces != null) {
					WriteWhitespace (pendingFirstSpaces.ToString ());
					pendingFirstSpaces = null;
				}
			}

			if (_state == WriteState.Start)
				WriteStartDocument ();

			if (_state == WriteState.Prolog) {
				//Seems to be the first element - take care of Doctype
				// Note that HTML does not require SYSTEM identifier.
				if (_currentOutput.DoctypePublic != null || _currentOutput.DoctypeSystem != null)
					Emitter.WriteDocType (prefix + (prefix==null? ":" : "") + localName, 
						_currentOutput.DoctypePublic, _currentOutput.DoctypeSystem);
			}
			CheckState ();
			Emitter.WriteStartElement (prefix, localName, nsURI);
			_state = WriteState.Element;						
			pendingAttributesPos = 0;
			_canProcessAttributes = true;
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
				Emitter.WriteFullEndElement ();
			else
				Emitter.WriteEndElement ();
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
			if (_nsManager.LookupNamespace (prefix) == nsUri)
				return;

			if (prefix == String.Empty) {
				//Default namespace
				if (_nsManager.DefaultNamespace != nsUri)
					_nsManager.AddNamespace (prefix, nsUri);
			} else if (_nsManager.LookupPrefix (nsUri) == null)
				//That's new namespace - add it to the collection
				_nsManager.AddNamespace (prefix, nsUri);

			if (_currentNamespaceDecls [prefix] as string != nsUri) {
				if (!_currentNsPrefixes.Contains (prefix))
					_currentNsPrefixes.Add (prefix);
				_currentNamespaceDecls [prefix] = nsUri;
			}
		}
			 		
		public override void WriteComment (string text)
		{
			CheckState ();
			Emitter.WriteComment (text);
		}

		public override void WriteProcessingInstruction (string name, string text)
		{
			CheckState ();
			Emitter.WriteProcessingInstruction (name, text);
		}

		public override void WriteString (string text)
		{
			CheckState ();
			if (_insideCData)
				Emitter.WriteCDataSection (text);
			else
				Emitter.WriteString (text);
		}

		public override void WriteRaw (string data)
		{
			CheckState ();
			Emitter.WriteRaw (data);
		}

		public override void WriteWhitespace (string text)
		{
			if (_emitter == null) {
				if (pendingFirstSpaces == null)
					pendingFirstSpaces = new StringBuilder ();
				pendingFirstSpaces.Append (text);
				if (_state == WriteState.Start)
					_state = WriteState.Prolog;
			} else {
				CheckState ();
				Emitter.WriteWhitespace (text);
			}
		}

		public override void Done ()
		{
			Emitter.Done ();
			_state = WriteState.Closed;
		}

		public override bool CanProcessAttributes {
			get { return _canProcessAttributes; }
		}

		public override WriteState WriteState { get { return _state; } }

		public override bool InsideCDataSection {
			get { return _insideCData; }
			set { _insideCData = value; }
		}
		#endregion
	}
}
