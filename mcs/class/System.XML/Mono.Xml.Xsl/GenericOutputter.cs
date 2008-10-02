//
// GenericOutputter.cs
//
// Authors:
//	Oleg Tkachenko (oleg@tkachenko.com)
//	Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//	
// (C) 2003 Oleg Tkachenko, Atsushi Enomoto
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
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
	internal class GenericOutputter : Outputter {	
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
		private ListDictionary _currentNamespaceDecls;
		// See CheckState(). This is just a cache.
		private ArrayList newNamespaces = new ArrayList();
		//Name table
		private NameTable _nt;
		// Specified encoding (for TextWriter output)
		Encoding _encoding;
		//Determines whether xsl:copy can output attribute-sets or not.
		bool _canProcessAttributes;
		bool _insideCData;
//		bool _isVariable;
		bool _omitXmlDeclaration;
		int _xpCount;

		private GenericOutputter (Hashtable outputs, Encoding encoding)
		{
			_encoding = encoding;
			_outputs = outputs;
			_currentOutput = (XslOutput)outputs [String.Empty];
			_state = WriteState.Prolog;
			//TODO: Optimize using nametable
			_nt = new NameTable ();
			_nsManager = new XmlNamespaceManager (_nt);
			_currentNamespaceDecls = new ListDictionary ();
			_omitXmlDeclaration = false;
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
//			_isVariable = isVariable;
			_omitXmlDeclaration = true; // .Net never writes XML declaration via XmlWriter
		}

		public GenericOutputter (TextWriter writer, Hashtable outputs, Encoding encoding)
			: this (outputs, encoding)
		{
			this.pendingTextWriter = writer;
		}

                
                internal GenericOutputter (TextWriter writer, Hashtable outputs)
                        : this (writer, outputs, null)
                {
                }

                internal GenericOutputter (XmlWriter writer, Hashtable outputs)
                        : this (writer, outputs, null)
                {
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
				default: // .Custom format is not supported, only handled as unknown
				case OutputMethod.Unknown:
					if (localName != null && String.Compare (localName, "html", true, CultureInfo.InvariantCulture) == 0 && ns == String.Empty)
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
					if (!_omitXmlDeclaration && !xslOutput.OmitXmlDeclaration)
						_emitter.WriteStartDocument (
							_encoding != null ? _encoding : xslOutput.Encoding,
							xslOutput.Standalone);

					break;
				case OutputMethod.Text:
					_emitter = new TextEmitter (pendingTextWriter);
					break;
			}
			pendingTextWriter = null;
		}

		/// <summary>
		/// Checks output state and flushes pending attributes and namespaces 
		/// when it's appropriate.
		/// </summary>
		private void CheckState ()
		{
			if (_state == WriteState.Element) {
				//Emit pending attributes
				_nsManager.PushScope ();
				foreach (string prefix in _currentNamespaceDecls.Keys)
				{
					string uri = _currentNamespaceDecls [prefix] as string;
					
					if (_nsManager.LookupNamespace (prefix, false) == uri)
						continue;

					newNamespaces.Add (prefix);
					_nsManager.AddNamespace (prefix, uri);
				}
				for (int i = 0; i < pendingAttributesPos; i++) 
				{
					Attribute attr = pendingAttributes [i];
					string prefix = attr.Prefix;
					if (prefix == XmlNamespaceManager.PrefixXml &&
						attr.Namespace != XmlNamespaceManager.XmlnsXml)
						// don't allow mapping from "xml" to other namespaces.
						prefix = String.Empty;
					string existing = _nsManager.LookupPrefix (attr.Namespace, false);
					if (prefix.Length == 0 && attr.Namespace.Length > 0)
						prefix = existing;
					if (attr.Namespace.Length > 0) {
						if (prefix == null || prefix == String.Empty)
						{ // ADD
							// empty prefix is not allowed
							// for non-local attributes.
							prefix = "xp_" + _xpCount++;
						//if (existing != prefix) {
							while (_nsManager.LookupNamespace (prefix) != null)
								prefix = "xp_" + _xpCount++;
							newNamespaces.Add (prefix);
							_currentNamespaceDecls.Add (prefix, attr.Namespace);
							_nsManager.AddNamespace (prefix, attr.Namespace);
						//}
						} // ADD
					}
					Emitter.WriteAttributeString (prefix, attr.LocalName, attr.Namespace, attr.Value);
				}
				for (int i = 0; i < newNamespaces.Count; i++)
				{
					string prefix = (string) newNamespaces [i];
					string uri = _currentNamespaceDecls [prefix] as string;
					
					if (prefix != String.Empty)
						Emitter.WriteAttributeString ("xmlns", prefix, XmlNamespaceManager.XmlnsXmlns, uri);
					else
						Emitter.WriteAttributeString (String.Empty, "xmlns", XmlNamespaceManager.XmlnsXmlns, uri);
				}
				_currentNamespaceDecls.Clear ();
				//Attributes flushed, state is Content now				
				_state = WriteState.Content;
				newNamespaces.Clear ();
			}
			_canProcessAttributes = false;
		}

		#region Outputter's methods implementation

		public override void WriteStartElement (string prefix, string localName, string nsURI)
		{
			if (_emitter == null) {
				this.DetermineOutputMethod (localName, nsURI);
				if (pendingFirstSpaces != null) {
					WriteWhitespace (pendingFirstSpaces.ToString ());
					pendingFirstSpaces = null;
				}
			}

			if (_state == WriteState.Prolog) {
				//Seems to be the first element - take care of Doctype
				// Note that HTML does not require SYSTEM identifier.
				if (_currentOutput.DoctypePublic != null || _currentOutput.DoctypeSystem != null)
					Emitter.WriteDocType (prefix + (prefix==null? ":" : "") + localName, 
						_currentOutput.DoctypePublic, _currentOutput.DoctypeSystem);
			}
			CheckState ();
			if (nsURI == String.Empty)
				prefix = String.Empty;
			Emitter.WriteStartElement (prefix, localName, nsURI);
			_state = WriteState.Element;
			if (_nsManager.LookupNamespace (prefix, false) != nsURI)
//				_nsManager.AddNamespace (prefix, nsURI);
				_currentNamespaceDecls [prefix] = nsURI;
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
			//Put attribute to pending attributes collection, replacing namesake one
			for (int i = 0; i < pendingAttributesPos; i++) {
				Attribute attr = pendingAttributes [i];
				
				if (attr.LocalName == localName && attr.Namespace == nsURI) {
					pendingAttributes [i].Value = value;
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
			if (_nsManager.LookupNamespace (prefix, false) == nsUri)
				return; // do nothing

			for (int i = 0; i < pendingAttributesPos; i++) {
				Attribute attr = pendingAttributes [i];
				if (attr.Prefix == prefix || attr.Namespace == nsUri)
					return; //don't touch explicitly declared attributes
			}
			if (_currentNamespaceDecls [prefix] as string != nsUri)
				_currentNamespaceDecls [prefix] = nsUri;
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
			// This weird check is required to reject Doctype
			// after non-whitespace nodes but also to allow 
			// Doctype after whitespace nodes. It especially
			// happens when there is an xsl:text before the
			// document element (e.g. BVTs_bvt066 testcase).
			else if (_state != WriteState.Content &&
				 text.Length > 0 && XmlChar.IsWhitespace (text))
				Emitter.WriteWhitespace (text);
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

		public override bool InsideCDataSection {
			get { return _insideCData; }
			set { _insideCData = value; }
		}
		#endregion
	}
}
