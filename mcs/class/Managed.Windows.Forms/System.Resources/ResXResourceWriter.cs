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
// Copyright (c) 2004 Novell, Inc.
//
// Authors:
//	Duncan Mak		duncan@ximian.com
//	Gonzalo Paniagua Javier	gonzalo@ximian.com

using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml;

namespace System.Resources
{
	public sealed class ResXResourceWriter : IResourceWriter, IDisposable
	{
		string filename;
		Stream stream;
		TextWriter textwriter;
		XmlTextWriter writer;
		bool written;

		public ResXResourceWriter (Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException ("stream");

			if (stream.CanWrite == false)
				throw new ArgumentException ("stream is not writable.", "stream");

			this.stream = stream;
		}

		public ResXResourceWriter (TextWriter textwriter)
		{
			if (textwriter == null)
				throw new ArgumentNullException ("textwriter");

			this.textwriter = textwriter;
		}
		
		public ResXResourceWriter (string fileName)
		{
			if (fileName == null)
				throw new ArgumentNullException ("fileName");

			this.filename = fileName;
		}
		
		void InitWriter ()
		{
			if (filename != null) {
				stream = File.OpenWrite (filename);
				textwriter = new StreamWriter (stream, Encoding.UTF8);
			}

			writer = new XmlTextWriter (textwriter);
			writer.Formatting = Formatting.Indented;
			writer.WriteStartDocument ();
			writer.WriteStartElement ("root");
			writer.WriteRaw (schema);
			WriteHeader ("resmimetype", "text/microsoft-resx");
			WriteHeader ("version", "1.3");
			WriteHeader ("reader", typeof (ResXResourceReader).AssemblyQualifiedName);
			WriteHeader ("writer", typeof (ResXResourceWriter).AssemblyQualifiedName);
		}

		void WriteHeader (string name, string value)
		{
			writer.WriteStartElement ("resheader");
			writer.WriteAttributeString ("name", name);
			writer.WriteStartElement ("value");
			writer.WriteString (value);
			writer.WriteEndElement ();
			writer.WriteEndElement ();
		}

		void WriteBytes (string name, string typename, byte [] value, int offset, int length)
		{
			writer.WriteStartElement ("data");
			writer.WriteAttributeString ("name", name);
			if (typename != null) {
				writer.WriteAttributeString ("type", typename);
			} else {
				writer.WriteAttributeString ("mimetype",
						"application/x-microsoft.net.object.binary.base64");
			}

			writer.WriteStartElement ("value");
			writer.WriteBase64 (value, offset, length);
			writer.WriteEndElement ();
			writer.WriteEndElement ();
		}

		void WriteBytes (string name, string typename, byte [] value)
		{
			WriteBytes (name, typename, value, 0, value.Length);
		}

		void WriteString (string name, string value)
		{
			writer.WriteStartElement ("data");
			writer.WriteAttributeString ("name", name);
			writer.WriteStartElement ("value");
			writer.WriteString (value);
			writer.WriteEndElement ();
			writer.WriteEndElement ();
			writer.WriteWhitespace ("\n  ");
		}

		public void AddResource (string name, byte [] value)
		{
			if (name == null)
				throw new ArgumentNullException ("name");

			if (value == null)
				throw new ArgumentNullException ("value");

			if (written)
				throw new InvalidOperationException ("The resource is already generated.");

			if (writer == null)
				InitWriter ();

			WriteBytes (name, value.GetType ().AssemblyQualifiedName, value);
		}

		public void AddResource (string name, object value)
		{
			if (name == null)
				throw new ArgumentNullException ("name");

			if (value == null)
				throw new ArgumentNullException ("value");

			if (written)
				throw new InvalidOperationException ("The resource is already generated.");

			if (writer == null)
				InitWriter ();

			TypeConverter converter = TypeDescriptor.GetConverter (value);
			if (converter != null && converter.CanConvertTo (typeof (string))) {
				string str = (string) converter.ConvertTo (value, typeof (string));
				WriteString (name, str);
				return;
			}
			
			MemoryStream ms = new MemoryStream ();
			BinaryFormatter fmt = new BinaryFormatter ();
			try {
				fmt.Serialize (ms, value);
			} catch (Exception e) {
				throw new InvalidOperationException ("Cannot add a " + value.GetType () +
								     "because it cannot be serialized: " +
								     e.Message);
			}

			WriteBytes (name, null, ms.GetBuffer (), 0, (int) ms.Length);
			ms.Close ();
		}
		
		public void AddResource (string name, string value)
		{
			if (name == null)
				throw new ArgumentNullException ("name");

			if (value == null)
				throw new ArgumentNullException ("value");

			if (written)
				throw new InvalidOperationException ("The resource is already generated.");

			if (writer == null)
				InitWriter ();

			WriteString (name, value);
		}

		public void Close ()
		{
			if (!written) {
				Generate ();
			}

			if (writer != null) {
				writer.Close ();
				stream = null;
				filename = null;
				textwriter = null;
			}
		}
		
		public void Dispose ()
		{
			Close ();
		}

		public void Generate ()
		{
			if (written)
				throw new InvalidOperationException ("The resource is already generated.");

			written = true;
			writer.WriteEndElement ();
			writer.Flush ();
		}

		static string schema = @"
  <xsd:schema id='root' xmlns='' xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:msdata='urn:schemas-microsoft-com:xml-msdata'>
    <xsd:element name='root' msdata:IsDataSet='true'>
      <xsd:complexType>
        <xsd:choice maxOccurs='unbounded'>
          <xsd:element name='data'>
            <xsd:complexType>
              <xsd:sequence>
                <xsd:element name='value' type='xsd:string' minOccurs='0' msdata:Ordinal='1' />
                <xsd:element name='comment' type='xsd:string' minOccurs='0' msdata:Ordinal='2' />
              </xsd:sequence>
              <xsd:attribute name='name' type='xsd:string' msdata:Ordinal='1' />
              <xsd:attribute name='type' type='xsd:string' msdata:Ordinal='3' />
              <xsd:attribute name='mimetype' type='xsd:string' msdata:Ordinal='4' />
            </xsd:complexType>
          </xsd:element>
          <xsd:element name='resheader'>
            <xsd:complexType>
              <xsd:sequence>
                <xsd:element name='value' type='xsd:string' minOccurs='0' msdata:Ordinal='1' />
              </xsd:sequence>
              <xsd:attribute name='name' type='xsd:string' use='required' />
            </xsd:complexType>
          </xsd:element>
        </xsd:choice>
      </xsd:complexType>
    </xsd:element>
  </xsd:schema>
".Replace ("'", "\"");
	}
}

