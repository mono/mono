//
// System.Xml.XmlWriter
//
// Authors:
//   Kral Ferch <kral_ferch@hotmail.com>
//   Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// (C) 2002 Kral Ferch
// (C) 2002-2003 Atsushi Enomoto
// (C) 2004-2007 Novell, Inc.
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
using System.IO;
using System.Text;
using System.Xml.XPath;
#if NET_4_5
using System.Threading;
using System.Threading.Tasks;
#endif

namespace System.Xml
{
	public abstract class XmlWriter : IDisposable
	{
		XmlWriterSettings settings;

		#region Constructors

		protected XmlWriter () { }

		#endregion

		#region Properties

		public virtual XmlWriterSettings Settings {
			get { return settings; }
		}

		public abstract WriteState WriteState { get; }

		public virtual string XmlLang {
			get { return null; }
		}

		public virtual XmlSpace XmlSpace {
			get { return XmlSpace.None; }
		}

		#endregion

		#region Methods

#if NET_4_5
		public virtual void Close ()
		{
			if (asyncRunning)
				throw new InvalidOperationException ("An asynchronous operation is already in progress.");
		}
#else
		public abstract void Close ();
#endif

		public static XmlWriter Create (Stream output)
		{
			return Create (output, null);
		}

		public static XmlWriter Create (string outputFileName)
		{
			return Create (outputFileName, null);
		}

		public static XmlWriter Create (TextWriter output)
		{
			return Create (output, null);
		}

		public static XmlWriter Create (XmlWriter output)
		{
			return Create (output, null);
		}

		public static XmlWriter Create (StringBuilder output)
		{
			return Create (output, null);
		}

		public static XmlWriter Create (Stream output, XmlWriterSettings settings)
		{
			Encoding enc = settings != null ? settings.Encoding : Encoding.UTF8;
			return Create (new StreamWriter (output, enc), settings);
		}

		public static XmlWriter Create (string outputFileName, XmlWriterSettings settings)
		{
			Encoding enc = settings != null ? settings.Encoding : Encoding.UTF8;
			return CreateTextWriter (new StreamWriter (outputFileName, false, enc), settings, true);
		}

		public static XmlWriter Create (StringBuilder output, XmlWriterSettings settings)
		{
			return Create (new StringWriter (output), settings);
		}

		public static XmlWriter Create (TextWriter output, XmlWriterSettings settings)
		{
			if (settings == null)
				settings = new XmlWriterSettings ();
			return CreateTextWriter (output, settings, settings.CloseOutput);
		}

		public static XmlWriter Create (XmlWriter output, XmlWriterSettings settings)
		{
			if (settings == null)
				settings = new XmlWriterSettings ();
			else
				settings = settings.Clone ();

			var src = output.Settings;
			if (src == null) {
				settings.ConformanceLevel = ConformanceLevel.Document; // Huh? Why??
				output = new DefaultXmlWriter (output);
#if NET_4_5
				settings.SetReadOnly ();
#endif
				output.settings = settings;
			} else {
				ConformanceLevel dst = src.ConformanceLevel;
				switch (src.ConformanceLevel) {
				case ConformanceLevel.Auto:
					dst = settings.ConformanceLevel;
					break;
				case ConformanceLevel.Document:
				case ConformanceLevel.Fragment:
					if (settings.ConformanceLevel != ConformanceLevel.Auto)
						dst = settings.ConformanceLevel;
					break;
				}

				settings.MergeFrom (src);

#if NET_4_5
				settings.SetReadOnly ();
#endif

				// It returns a new XmlWriter instance if 1) Settings is null, or 2) Settings ConformanceLevel (or might be other members as well) give significant difference.
				if (src.ConformanceLevel != dst) {
					output = new DefaultXmlWriter (output, false);
					output.settings = settings;
				}
			}

			return output;
		}

		private static XmlWriter CreateTextWriter (TextWriter writer, XmlWriterSettings settings, bool closeOutput)
		{
			if (settings == null)
				settings = new XmlWriterSettings ();
			XmlTextWriter xtw = new XmlTextWriter (writer, settings, closeOutput);
			return Create (xtw, settings);
		}

		protected virtual void Dispose (bool disposing)
		{
			Close ();
		}

#if NET_4_0
		public void Dispose ()
#else
		void IDisposable.Dispose() 
#endif
		{
			Dispose (false);
		}

		public abstract void Flush ();

		public abstract string LookupPrefix (string ns);

		private void WriteAttribute (XmlReader reader, bool defattr)
		{
			if (!defattr && reader.IsDefault)
				return;

			WriteStartAttribute (reader.Prefix, reader.LocalName, reader.NamespaceURI);
			while (reader.ReadAttributeValue ()) {
				switch (reader.NodeType) {
				case XmlNodeType.Text:
					WriteString (reader.Value);
					break;
				case XmlNodeType.EntityReference:
					WriteEntityRef (reader.Name);
					break;
				}
			}
			WriteEndAttribute ();
		}

		public virtual void WriteAttributes (XmlReader reader, bool defattr)
		{
			if(reader == null)
				throw new ArgumentException("null XmlReader specified.", "reader");

			switch (reader.NodeType) {
			case XmlNodeType.XmlDeclaration:
				WriteAttributeString ("version", reader ["version"]);
				if (reader ["encoding"] != null)
					WriteAttributeString ("encoding", reader ["encoding"]);
				if (reader ["standalone"] != null)
					WriteAttributeString ("standalone", reader ["standalone"]);
				break;
			case XmlNodeType.Element:
				if (reader.MoveToFirstAttribute ())
					goto case XmlNodeType.Attribute;
				break;
			case XmlNodeType.Attribute:
				do {
					WriteAttribute (reader, defattr);
				} while (reader.MoveToNextAttribute ());
				reader.MoveToElement ();
				break;
			default:
				throw new XmlException("NodeType is not one of Element, Attribute, nor XmlDeclaration.");
			}
		}

		public void WriteAttributeString (string localName, string value)
		{
			WriteAttributeString ("", localName, null, value);
		}

		public void WriteAttributeString (string localName, string ns, string value)
		{
			WriteAttributeString ("", localName, ns, value);
		}

		public void WriteAttributeString (string prefix, string localName, string ns, string value)
		{
			// In MS.NET (1.0), this check is done *here*, not at WriteStartAttribute.
			// (XmlTextWriter.WriteStartAttribute("xmlns", "anyname", null) throws an exception.

			WriteStartAttribute (prefix, localName, ns);
			if (value != null && value.Length > 0)
				WriteString (value);
			WriteEndAttribute ();
		}

		public abstract void WriteBase64 (byte[] buffer, int index, int count);

		public virtual void WriteBinHex (byte [] buffer, int index, int count)
		{
			StringWriter sw = new StringWriter ();
			XmlConvert.WriteBinHex (buffer, index, count, sw);
			WriteString (sw.ToString ());
		}

		public abstract void WriteCData (string text);

		public abstract void WriteCharEntity (char ch);

		public abstract void WriteChars (char[] buffer, int index, int count);

		public abstract void WriteComment (string text);

		public abstract void WriteDocType (string name, string pubid, string sysid, string subset);

		public void WriteElementString (string localName, string value)
		{
			WriteStartElement(localName);
			if (value != null && value.Length > 0)
				WriteString(value);
			WriteEndElement();
		}

		public void WriteElementString (string localName, string ns, string value)
		{
			WriteStartElement(localName, ns);
			if (value != null && value.Length > 0)
				WriteString(value);
			WriteEndElement();
		}

		public void WriteElementString (string prefix, string localName, string ns, string value)
		{
			WriteStartElement(prefix, localName, ns);
			if (value != null && value.Length > 0)
				WriteString(value);
			WriteEndElement();
		}

		public abstract void WriteEndAttribute ();

		public abstract void WriteEndDocument ();

		public abstract void WriteEndElement ();

		public abstract void WriteEntityRef (string name);

		public abstract void WriteFullEndElement ();

		public virtual void WriteName (string name)
		{
			WriteNameInternal (name);
		}

		public virtual void WriteNmToken (string name)
		{
			WriteNmTokenInternal (name);
		}

		public virtual void WriteQualifiedName (string localName, string ns)
		{
			WriteQualifiedNameInternal (localName, ns);
		}

		internal void WriteNameInternal (string name)
		{
			switch (Settings.ConformanceLevel) {
			case ConformanceLevel.Document:
			case ConformanceLevel.Fragment:
				XmlConvert.VerifyName (name);
				break;
			}
			WriteString (name);
		}

		internal virtual void WriteNmTokenInternal (string name)
		{
			bool valid = true;
			switch (Settings.ConformanceLevel) {
			case ConformanceLevel.Document:
			case ConformanceLevel.Fragment:
				valid = XmlChar.IsNmToken (name);
					break;
			}
			if (!valid)
				throw new ArgumentException ("Argument name is not a valid NMTOKEN.");
			WriteString (name);
		}

		internal void WriteQualifiedNameInternal (string localName, string ns)
		{
			if (localName == null || localName == String.Empty)
				throw new ArgumentException ();
			if (ns == null)
				ns = String.Empty;

			if (Settings != null) {
				switch (Settings.ConformanceLevel) {
				case ConformanceLevel.Document:
				case ConformanceLevel.Fragment:
					XmlConvert.VerifyNCName (localName);
					break;
				}
			}
			else
				XmlConvert.VerifyNCName (localName);

			string prefix = ns.Length > 0 ? LookupPrefix (ns) : String.Empty;
			if (prefix == null)
				throw new ArgumentException (String.Format ("Namespace '{0}' is not declared.", ns));

			if (prefix != String.Empty) {
				WriteString (prefix);
				WriteString (":");
				WriteString (localName);
			}
			else
				WriteString (localName);
		}

		public virtual void WriteNode (XPathNavigator navigator, bool defattr)
		{
			if (navigator == null)
				throw new ArgumentNullException ("navigator");
			switch (navigator.NodeType) {
			case XPathNodeType.Attribute:
				// no operation
				break;
			case XPathNodeType.Namespace:
				// no operation
				break;
			case XPathNodeType.Text:
				WriteString (navigator.Value);
				break;
			case XPathNodeType.SignificantWhitespace:
				WriteWhitespace (navigator.Value);
				break;
			case XPathNodeType.Whitespace:
				WriteWhitespace (navigator.Value);
				break;
			case XPathNodeType.Comment:
				WriteComment (navigator.Value);
				break;
			case XPathNodeType.ProcessingInstruction:
				WriteProcessingInstruction (navigator.Name, navigator.Value);
				break;
			case XPathNodeType.Root:
				if (navigator.MoveToFirstChild ()) {
					do {
						WriteNode (navigator, defattr);
					} while (navigator.MoveToNext ());
					navigator.MoveToParent ();
				}
				break;
			case XPathNodeType.Element:
				WriteStartElement (navigator.Prefix, navigator.LocalName, navigator.NamespaceURI);
				if (navigator.MoveToFirstNamespace (XPathNamespaceScope.Local)) {
					do {
						if (defattr || navigator.SchemaInfo == null || navigator.SchemaInfo.IsDefault)
							WriteAttributeString (navigator.Prefix,
								navigator.LocalName == String.Empty ? "xmlns" : navigator.LocalName,
								"http://www.w3.org/2000/xmlns/",
								navigator.Value);
					} while (navigator.MoveToNextNamespace (XPathNamespaceScope.Local));
					navigator.MoveToParent ();
				}
				if (navigator.MoveToFirstAttribute ()) {
					do {
						if (defattr || navigator.SchemaInfo == null || navigator.SchemaInfo.IsDefault)
							WriteAttributeString (navigator.Prefix, navigator.LocalName, navigator.NamespaceURI, navigator.Value);

					} while (navigator.MoveToNextAttribute ());
					navigator.MoveToParent ();
				}
				if (navigator.MoveToFirstChild ()) {
					do {
						WriteNode (navigator, defattr);
					} while (navigator.MoveToNext ());
					navigator.MoveToParent ();
				}
				if (navigator.IsEmptyElement)
					WriteEndElement ();
				else
					WriteFullEndElement ();
				break;
			default:
				throw new NotSupportedException ();
			}
		}

		public virtual void WriteNode (XmlReader reader, bool defattr)
		{
			if (reader == null)
				throw new ArgumentException ();

			if (reader.ReadState == ReadState.Initial) {
				reader.Read ();
				do {
					WriteNode (reader, defattr);
				} while (!reader.EOF);
				return;
			}

			switch (reader.NodeType) {
			case XmlNodeType.Element:
				WriteStartElement (reader.Prefix, reader.LocalName, reader.NamespaceURI);
#if false
				WriteAttributes (reader, defattr);
				reader.MoveToElement ();
#else
				// Well, I found that MS.NET took this way, since
				// there was a error-prone SgmlReader that fails
				// MoveToNextAttribute().
				if (reader.HasAttributes) {
					for (int i = 0; i < reader.AttributeCount; i++) {
						reader.MoveToAttribute (i);
						WriteAttribute (reader, defattr);
					}
					reader.MoveToElement ();
				}
#endif
				if (reader.IsEmptyElement)
					WriteEndElement ();
				else {
					int depth = reader.Depth;
					reader.Read ();
					if (reader.NodeType != XmlNodeType.EndElement) {
						do {
							WriteNode (reader, defattr);
						} while (depth < reader.Depth);
					}
					WriteFullEndElement ();
				}
				break;
			// In case of XmlAttribute, don't proceed reader, and it will never be written.
			case XmlNodeType.Attribute:
				return;
			case XmlNodeType.Text:
				WriteString (reader.Value);
				break;
			case XmlNodeType.CDATA:
				WriteCData (reader.Value);
				break;
			case XmlNodeType.EntityReference:
				WriteEntityRef (reader.Name);
				break;
			case XmlNodeType.XmlDeclaration:
				// LAMESPEC: It means that XmlWriter implementation _must not_ check
				// whether PI name is "xml" (it is XML error) or not.
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
				WriteFullEndElement ();
				break;
			case XmlNodeType.EndEntity:
				break;
			case XmlNodeType.None:
				break;	// Do nothing, nor reporting errors.
			default:
				throw new XmlException ("Unexpected node " + reader.Name + " of type " + reader.NodeType);
			}
			reader.Read ();
		}

		public abstract void WriteProcessingInstruction (string name, string text);

		public abstract void WriteRaw (string data);

		public abstract void WriteRaw (char[] buffer, int index, int count);

		public void WriteStartAttribute (string localName)
		{
			WriteStartAttribute (null, localName, null);
		}

		public void WriteStartAttribute (string localName, string ns)
		{
			WriteStartAttribute (null, localName, ns);
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

		public virtual void WriteValue (bool value)
		{
			WriteString (XQueryConvert.BooleanToString (value));
		}

		public virtual void WriteValue (DateTime value)
		{
			WriteString (XmlConvert.ToString (value));
		}

		public virtual void WriteValue (decimal value)
		{
			WriteString (XQueryConvert.DecimalToString (value));
		}

		public virtual void WriteValue (double value)
		{
			WriteString (XQueryConvert.DoubleToString (value));
		}

		public virtual void WriteValue (int value)
		{
			WriteString (XQueryConvert.IntToString (value));
		}

		public virtual void WriteValue (long value)
		{
			WriteString (XQueryConvert.IntegerToString (value));
		}

		public virtual void WriteValue (object value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");

			if (value is string)
				WriteString ((string) value);
			else if (value is bool)
				WriteValue ((bool) value);
			else if (value is byte)
				WriteValue ((int) value);
			else if (value is byte [])
				WriteBase64 ((byte []) value, 0, ((byte []) value).Length);
			else if (value is char [])
				WriteChars ((char []) value, 0, ((char []) value).Length);
			else if (value is DateTime)
				WriteValue ((DateTime) value);
			else if (value is decimal)
				WriteValue ((decimal) value);
			else if (value is double)
				WriteValue ((double) value);
			else if (value is short)
				WriteValue ((int) value);
			else if (value is int)
				WriteValue ((int) value);
			else if (value is long)
				WriteValue ((long) value);
			else if (value is float)
				WriteValue ((float) value);
			else if (value is TimeSpan) // undocumented
				WriteString (XmlConvert.ToString ((TimeSpan) value));
			else if (value is Uri)
				WriteString (((Uri) value).ToString ());
			else if (value is XmlQualifiedName) {
				XmlQualifiedName qname = (XmlQualifiedName) value;
				if (!qname.Equals (XmlQualifiedName.Empty)) {
					if (qname.Namespace.Length > 0 && LookupPrefix (qname.Namespace) == null)
						throw new InvalidCastException (String.Format ("The QName '{0}' cannot be written. No corresponding prefix is declared", qname));
					WriteQualifiedName (qname.Name, qname.Namespace);
				}
				else
					WriteString (String.Empty);
			}
			else if (value is IEnumerable) {
				bool follow = false;
				foreach (object obj in (IEnumerable) value) {
					if (follow)
						WriteString (" ");
					else
						follow = true;
					WriteValue (obj);
				}
			}
			else
				throw new InvalidCastException (String.Format ("Type '{0}' cannot be cast to string", value.GetType ()));
		}

		public virtual void WriteValue (float value)
		{
			WriteString (XQueryConvert.FloatToString (value));
		}

		public virtual void WriteValue (string value)
		{
			WriteString (value);
		}

#if NET_4_5
		public virtual void WriteValue (DateTimeOffset value)
		{
			WriteString (XmlConvert.ToString (value));
		}
#endif

		#endregion

#if NET_4_5
		#region .NET 4.5 Async Methods

		bool asyncRunning;

		void StartAsync ()
		{
			if (!settings.Async)
				throw new InvalidOperationException ("Set XmlWriterSettings.Async to true if you want to use Async Methods.");
			lock (this) {
				if (asyncRunning)
					throw new InvalidOperationException ("An asynchronous operation is already in progress.");
				asyncRunning = true;
			}
		}

		public virtual Task FlushAsync ()
		{
			StartAsync ();
			return Task.Run (() => {
				try {
					Flush ();
				} finally {
					asyncRunning = false;
				}
			});
		}

		public virtual Task WriteAttributesAsync (XmlReader reader, bool defattr)
		{
			StartAsync ();
			return Task.Run (() => {
				try {
					WriteAttributes (reader, defattr);
				} finally {
					asyncRunning = false;
				}
			});
		}

		public Task WriteAttributeStringAsync (string prefix, string localName,
		                                       string ns, string value)
		{
			StartAsync ();
			return Task.Run (() => {
				try {
					WriteAttributeString (prefix, localName, ns, value);
				} finally {
					asyncRunning = false;
				}
			});
		}

		public virtual Task WriteBase64Async (byte[] buffer, int index, int count)
		{
			StartAsync ();
			return Task.Run (() => {
				try {
					WriteBase64 (buffer, index, count);
				} finally {
					asyncRunning = false;
				}
			});
		}

		public virtual Task WriteBinHexAsync (byte[] buffer, int index, int count)
		{
			StartAsync ();
			return Task.Run (() => {
				try {
					WriteBinHex (buffer, index, count);
				} finally {
					asyncRunning = false;
				}
			});
		}

		public virtual Task WriteCDataAsync (string text)
		{
			StartAsync ();
			return Task.Run (() => {
				try {
					WriteCData (text);
				} finally {
					asyncRunning = false;
				}
			});
		}

		public virtual Task WriteCharEntityAsync (char ch)
		{
			StartAsync ();
			return Task.Run (() => {
				try {
					WriteCharEntity (ch);
				} finally {
					asyncRunning = false;
				}
			});
		}

		public virtual Task WriteCharsAsync (char[] buffer, int index, int count)
		{
			StartAsync ();
			return Task.Run (() => {
				try {
					WriteChars (buffer, index, count);
				} finally {
					asyncRunning = false;
				}
			});
		}

		public virtual Task WriteCommentAsync (string text)
		{
			StartAsync ();
			return Task.Run (() => {
				try {
					WriteComment (text);
				} finally {
					asyncRunning = false;
				}
			});
		}

		public virtual Task WriteDocTypeAsync (string name, string pubid, string sysid, string subset)
		{
			StartAsync ();
			return Task.Run (() => {
				try {
					WriteDocType (name, pubid, sysid, subset);
				} finally {
					asyncRunning = false;
				}
			});
		}

		public Task WriteElementStringAsync (string prefix, string localName, string ns, string value)
		{
			StartAsync ();
			return Task.Run (() => {
				try {
					WriteElementString (prefix, localName, ns, value);
				} finally {
					asyncRunning = false;
				}
			});
		}

		protected internal virtual Task WriteEndAttributeAsync ()
		{
			StartAsync ();
			return Task.Run (() => {
				try {
					WriteEndAttribute ();
				} finally {
					asyncRunning = false;
				}
			});
		}

		public virtual Task WriteEndDocumentAsync ()
		{
			StartAsync ();
			return Task.Run (() => {
				try {
					WriteEndDocument ();
				} finally {
					asyncRunning = false;
				}
			});
		}

		public virtual Task WriteEndElementAsync ()
		{
			StartAsync ();
			return Task.Run (() => {
				try {
					WriteEndElement ();
				} finally {
					asyncRunning = false;
				}
			});
		}

		public virtual Task WriteEntityRefAsync (string name)
		{
			StartAsync ();
			return Task.Run (() => {
				try {
					WriteEntityRef (name);
				} finally {
					asyncRunning = false;
				}
			});
		}

		public virtual Task WriteFullEndElementAsync ()
		{
			StartAsync ();
			return Task.Run (() => {
				try {
					WriteFullEndElement ();
				} finally {
					asyncRunning = false;
				}
			});
		}

		public virtual Task WriteNameAsync (string name)
		{
			StartAsync ();
			return Task.Run (() => {
				try {
					WriteName (name);
				} finally {
					asyncRunning = false;
				}
			});
		}

		public virtual Task WriteNmTokenAsync (string name)
		{
			StartAsync ();
			return Task.Run (() => {
				try {
					WriteNmToken (name);
				} finally {
					asyncRunning = false;
				}
			});
		}

		public virtual Task WriteNodeAsync (XmlReader reader, bool defattr)
		{
			StartAsync ();
			return Task.Run (() => {
				try {
					WriteNode (reader, defattr);
				} finally {
					asyncRunning = false;
				}
			});
		}

		public virtual Task WriteNodeAsync (XPathNavigator navigator, bool defattr)
		{
			StartAsync ();
			return Task.Run (() => {
				try {
					WriteNode (navigator, defattr);
				} finally {
					asyncRunning = false;
				}
			});
		}

		public virtual Task WriteProcessingInstructionAsync (string name, string text)
		{
			StartAsync ();
			return Task.Run (() => {
				try {
					WriteProcessingInstruction (name, text);
				} finally {
					asyncRunning = false;
				}
			});
		}

		public virtual Task WriteQualifiedNameAsync (string localName, string ns)
		{
			StartAsync ();
			return Task.Run (() => {
				try {
					WriteQualifiedName (localName, ns);
				} finally {
					asyncRunning = false;
				}
			});
		}

		public virtual Task WriteRawAsync (string data)
		{
			StartAsync ();
			return Task.Run (() => {
				try {
					WriteRaw (data);
				} finally {
					asyncRunning = false;
				}
			});

		}

		public virtual Task WriteRawAsync (char[] buffer, int index, int count)
		{
			StartAsync ();
			return Task.Run (() => {
				try {
					WriteRaw (buffer, index, count);
				} finally {
					asyncRunning = false;
				}
			});
		}

		protected internal virtual Task WriteStartAttributeAsync (
			string prefix, string localName, string ns)
		{
			StartAsync ();
			return Task.Run (() => {
				try {
					WriteStartAttribute (prefix, localName, ns);
				} finally {
					asyncRunning = false;
				}
			});
		}

		public virtual Task WriteStartDocumentAsync ()
		{
			StartAsync ();
			return Task.Run (() => {
				try {
					WriteStartDocument ();
				} finally {
					asyncRunning = false;
				}
			});
		}

		public virtual Task WriteStartDocumentAsync (bool standalone)
		{
			StartAsync ();
			return Task.Run (() => {
				try {
					WriteStartDocument (standalone);
				} finally {
					asyncRunning = false;
				}
			});
		}

		public virtual Task WriteStartElementAsync (string prefix, string localName, string ns)
		{
			StartAsync ();
			return Task.Run (() => {
				try {
					WriteStartElement (prefix, localName, ns);
				} finally {
					asyncRunning = false;
				}
			});
		}

		public virtual Task WriteStringAsync (string text)
		{
			StartAsync ();
			return Task.Run (() => {
				try {
					WriteString (text);
				} finally {
					asyncRunning = false;
				}
			});
		}

		public virtual Task WriteSurrogateCharEntityAsync (char lowChar, char highChar)
		{
			StartAsync ();
			return Task.Run (() => {
				try {
					WriteSurrogateCharEntity (lowChar, highChar);
				} finally {
					asyncRunning = false;
				}
			});
		}

		public virtual Task WriteWhitespaceAsync (string ws)
		{
			StartAsync ();
			return Task.Run (() => {
				try {
					WriteWhitespace (ws);
				} finally {
					asyncRunning = false;
				}
			});
		}

		#endregion
#endif
	}
}
