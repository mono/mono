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
// Copyright (c) 2004-2005 Novell, Inc.
//
// Authors:
//	Duncan Mak		duncan@ximian.com
//	Gonzalo Paniagua Javier	gonzalo@ximian.com
//	Peter Bartok		pbartok@novell.com
//	Gary Barnett		gary.barnett.mono@gmail.com
//	includes code by Mike Krüger and Lluis Sanchez

using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml;
using System.Reflection;

namespace System.Resources
{
#if INSIDE_SYSTEM_WEB
	internal
#else
	public
#endif
	class ResXResourceWriter : IResourceWriter, IDisposable
	{
		#region Local Variables
		private string		filename;
		private Stream		stream;
		private TextWriter	textwriter;
		private XmlTextWriter	writer;
		private bool		written;
		private string		base_path;
		#endregion	// Local Variables

		#region Static Fields
		public static readonly string BinSerializedObjectMimeType	= "application/x-microsoft.net.object.binary.base64";
		public static readonly string ByteArraySerializedObjectMimeType	= "application/x-microsoft.net.object.bytearray.base64";
		public static readonly string DefaultSerializedObjectMimeType	= BinSerializedObjectMimeType;
		public static readonly string ResMimeType			= "text/microsoft-resx";
		public static readonly string ResourceSchema			= schema;
		public static readonly string SoapSerializedObjectMimeType	= "application/x-microsoft.net.object.soap.base64";
		public static readonly string Version				= "2.0";
		#endregion	// Static Fields

		#region Constructors & Destructor
		public ResXResourceWriter (Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException ("stream");

			if (!stream.CanWrite)
				throw new ArgumentException ("stream is not writable.", "stream");

			this.stream = stream;
		}

		public ResXResourceWriter (TextWriter textWriter)
		{
			if (textWriter == null)
				throw new ArgumentNullException ("textWriter");

			this.textwriter = textWriter;
		}
		
		public ResXResourceWriter (string fileName)
		{
			if (fileName == null)
				throw new ArgumentNullException ("fileName");

			this.filename = fileName;
		}

		~ResXResourceWriter() {
			Dispose(false);
		}
		#endregion	// Constructors & Destructor

		void InitWriter ()
		{
			if (filename != null)
				stream = File.Open (filename, FileMode.Create);
			if (textwriter == null)
				textwriter = new StreamWriter (stream, Encoding.UTF8);

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

		void WriteNiceBase64(byte[] value, int offset, int length) {
			string		b64;
			StringBuilder	sb;
			int		pos;
			int		inc;
			string		ins;

			b64 = Convert.ToBase64String(value, offset, length);

			// Wild guess; two extra newlines, and one newline/tab pair for every 80 chars
			sb = new StringBuilder(b64, b64.Length + ((b64.Length + 160) / 80) * 3);
			pos = 0;
			inc = 80 + Environment.NewLine.Length + 1;
			ins = Environment.NewLine + "\t";
			while (pos < sb.Length) {
				sb.Insert(pos, ins);
				pos += inc;
			}
			sb.Insert(sb.Length, Environment.NewLine);
			writer.WriteString(sb.ToString());
		}
		void WriteBytes (string name, Type type, byte[] value, int offset, int length)
		{
			WriteBytes (name, type, value, offset, length, String.Empty);
		}

		void WriteBytes (string name, Type type, byte[] value, int offset, int length, string comment)
		{
			writer.WriteStartElement ("data");
			writer.WriteAttributeString ("name", name);

			if (type != null) {
				writer.WriteAttributeString ("type", type.AssemblyQualifiedName);
				// byte[] should never get a mimetype, otherwise MS.NET won't be able
				// to parse the data.
				if (type != typeof (byte[]))
					writer.WriteAttributeString ("mimetype", ByteArraySerializedObjectMimeType);
				writer.WriteStartElement ("value");
				WriteNiceBase64 (value, offset, length);
			} else {
				writer.WriteAttributeString ("mimetype", BinSerializedObjectMimeType);
				writer.WriteStartElement ("value");
				writer.WriteBase64 (value, offset, length);
			}

			writer.WriteEndElement ();

			if (!(comment == null || comment.Equals (String.Empty))) {
				writer.WriteStartElement ("comment");
				writer.WriteString (comment);
				writer.WriteEndElement ();
			}
			
			writer.WriteEndElement ();
		}

		void WriteBytes (string name, Type type, byte [] value, string comment)
		{
			WriteBytes (name, type, value, 0, value.Length, comment);
		}

		void WriteString (string name, string value)
		{
			WriteString (name, value, null);
		}
		void WriteString (string name, string value, Type type)
		{
			WriteString (name, value, type, String.Empty);
		}
		void WriteString (string name, string value, Type type, string comment)
		{
			writer.WriteStartElement ("data");
			writer.WriteAttributeString ("name", name);
			if (type != null)
				writer.WriteAttributeString ("type", type.AssemblyQualifiedName);
			writer.WriteStartElement ("value");
			writer.WriteString (value);
			writer.WriteEndElement ();
			if (!(comment == null || comment.Equals (String.Empty))) {
				writer.WriteStartElement ("comment");
				writer.WriteString (comment);
				writer.WriteEndElement ();
			}
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

			WriteBytes (name, value.GetType (), value, null);
		}

		public void AddResource (string name, object value)
		{
			AddResource (name, value, String.Empty);
		}

		private void AddResource (string name, object value, string comment)
		{
			if (value is string) {
				AddResource (name, (string) value, comment);
				return;
			}

			if (name == null)
				throw new ArgumentNullException ("name");

			if (value != null && !value.GetType ().IsSerializable)
					throw new InvalidOperationException (String.Format ("The element '{0}' of type '{1}' is not serializable.", name, value.GetType ().Name));

			if (written)
				throw new InvalidOperationException ("The resource is already generated.");

			if (writer == null)
				InitWriter ();

			if (value is byte[]) {
				WriteBytes (name, value.GetType (), (byte []) value, comment);
				return;
			}

			if (value == null) {
				// nulls written as ResXNullRef
				WriteString (name, "", typeof (ResXNullRef), comment);
				return;
			}

			TypeConverter converter = TypeDescriptor.GetConverter (value);
			if (value is ResXFileRef) {
				ResXFileRef fileRef = ProcessFileRefBasePath ((ResXFileRef) value);	
				string str = (string) converter.ConvertToInvariantString (fileRef);
				WriteString (name, str, value.GetType (), comment);
				return;
			}

			if (converter != null && converter.CanConvertTo (typeof (string)) && converter.CanConvertFrom (typeof (string))) {
				string str = (string) converter.ConvertToInvariantString (value);
				WriteString (name, str, value.GetType (), comment);
				return;
			}
			
			if (converter != null && converter.CanConvertTo (typeof (byte[])) && converter.CanConvertFrom (typeof (byte[]))) {
				byte[] b = (byte[]) converter.ConvertTo (value, typeof (byte[]));
				WriteBytes (name, value.GetType (), b, comment);
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

			WriteBytes (name, null, ms.GetBuffer (), 0, (int) ms.Length, comment);
			ms.Close ();
		}
		
		public void AddResource (string name, string value)
		{
			AddResource (name, value, string.Empty);
		}

		private void AddResource (string name, string value, string comment)
		{
			if (name == null)
				throw new ArgumentNullException ("name");

			if (value == null)
				throw new ArgumentNullException ("value");

			if (written)
				throw new InvalidOperationException ("The resource is already generated.");

			if (writer == null)
				InitWriter ();

			WriteString (name, value, null, comment);
		}

		[MonoTODO ("Stub, not implemented")]
		public virtual void AddAlias (string aliasName, AssemblyName assemblyName)
		{
		}
		
		public void AddResource (ResXDataNode node)
		{
			if (node == null)
				throw new ArgumentNullException ("node");

			if (writer == null)
				InitWriter ();

			if (node.IsWritable)
				WriteWritableNode (node);
			else if (node.FileRef != null)
				AddResource (node.Name, node.FileRef, node.Comment);
			else 
				AddResource (node.Name, node.GetValue ((AssemblyName []) null), node.Comment);
		}

		ResXFileRef ProcessFileRefBasePath (ResXFileRef fileRef)
		{
			if (String.IsNullOrEmpty (BasePath))
				return fileRef;

			string newPath = AbsoluteToRelativePath (BasePath, fileRef.FileName);
			return new ResXFileRef (newPath, fileRef.TypeName, fileRef.TextFileEncoding);
		}

		static bool IsSeparator (char ch)
		{
			return ch == Path.DirectorySeparatorChar || ch == Path.AltDirectorySeparatorChar || ch == Path.VolumeSeparatorChar;
		}
		//adapted from MonoDevelop.Core
		unsafe static string AbsoluteToRelativePath (string baseDirectoryPath, string absPath)
		{
			if (string.IsNullOrEmpty (baseDirectoryPath))
				return absPath;

			baseDirectoryPath = baseDirectoryPath.TrimEnd (Path.DirectorySeparatorChar);

			fixed (char* bPtr = baseDirectoryPath, aPtr = absPath) {
				var bEnd = bPtr + baseDirectoryPath.Length;
				var aEnd = aPtr + absPath.Length;
				char* lastStartA = aEnd;
				char* lastStartB = bEnd;
				
				int indx = 0;
				// search common base path
				var a = aPtr;
				var b = bPtr;
				while (a < aEnd) {
					if (*a != *b)
						break;
					if (IsSeparator (*a)) {
						indx++;
						lastStartA = a + 1;
						lastStartB = b; 
					}
					a++;
					b++;
					if (b >= bEnd) {
						if (a >= aEnd || IsSeparator (*a)) {
							indx++;
							lastStartA = a + 1;
							lastStartB = b;
						}
						break;
					}
				}
				if (indx == 0) 
					return absPath;
				
				if (lastStartA >= aEnd)
					return ".";
				
				// handle case a: some/path b: some/path/deeper...
				if (a >= aEnd) {
					if (IsSeparator (*b)) {
						lastStartA = a + 1;
						lastStartB = b;
					}
				}
				
				// look how many levels to go up into the base path
				int goUpCount = 0;
				while (lastStartB < bEnd) {
					if (IsSeparator (*lastStartB))
						goUpCount++;
					lastStartB++;
				}
				var size = goUpCount * 2 + goUpCount + aEnd - lastStartA;
				var result = new char [size];
				fixed (char* rPtr = result) {
					// go paths up
					var r = rPtr;
					for (int i = 0; i < goUpCount; i++) {
						*(r++) = '.';
						*(r++) = '.';
						*(r++) = Path.DirectorySeparatorChar;
					}
					// copy the remaining absulute path
					while (lastStartA < aEnd)
						*(r++) = *(lastStartA++);
				}
				return new string (result);
			}
		}

		// avoids instantiating objects
		void WriteWritableNode (ResXDataNode node)
		{
			writer.WriteStartElement ("data");
			writer.WriteAttributeString ("name", node.Name);
			if (!(node.Type == null || node.Type.Equals (String.Empty)))
				writer.WriteAttributeString ("type", node.Type);
			if (!(node.MimeType == null || node.MimeType.Equals (String.Empty)))
				writer.WriteAttributeString ("mimetype", node.MimeType);
			writer.WriteStartElement ("value");
			writer.WriteString (node.DataString);
			writer.WriteEndElement ();
			if (!(node.Comment == null || node.Comment.Equals (String.Empty))) {
				writer.WriteStartElement ("comment");
				writer.WriteString (node.Comment);
				writer.WriteEndElement ();
			}
			writer.WriteEndElement ();
			writer.WriteWhitespace ("\n  ");
		}		

		public void AddMetadata (string name, string value)
		{
			if (name == null)
				throw new ArgumentNullException ("name");

			if (value == null)
				throw new ArgumentNullException ("value");

			if (written)
				throw new InvalidOperationException ("The resource is already generated.");

			if (writer == null)
				InitWriter ();

			writer.WriteStartElement ("metadata");
			writer.WriteAttributeString ("name", name);
			writer.WriteAttributeString ("xml:space", "preserve");
			
			writer.WriteElementString ("value", value);
			
			writer.WriteEndElement ();
		}

		public void AddMetadata (string name, byte[] value)
		{
			if (name == null)
				throw new ArgumentNullException ("name");

			if (value == null)
				throw new ArgumentNullException ("value");

			if (written)
				throw new InvalidOperationException ("The resource is already generated.");

			if (writer == null)
				InitWriter ();

			writer.WriteStartElement ("metadata");
			writer.WriteAttributeString ("name", name);

			writer.WriteAttributeString ("type", value.GetType ().AssemblyQualifiedName);
			
			writer.WriteStartElement ("value");
			WriteNiceBase64 (value, 0, value.Length);
			writer.WriteEndElement ();

			writer.WriteEndElement ();
		}
		
		public void AddMetadata (string name, object value)
		{
			if (value is string) {
				AddMetadata (name, (string)value);
				return;
			}

			if (value is byte[]) {
				AddMetadata (name, (byte[])value);
				return;
			}

			if (name == null)
				throw new ArgumentNullException ("name");

			if (value == null)
				throw new ArgumentNullException ("value");

			if (!value.GetType ().IsSerializable)
				throw new InvalidOperationException (String.Format ("The element '{0}' of type '{1}' is not serializable.", name, value.GetType ().Name));

			if (written)
				throw new InvalidOperationException ("The resource is already generated.");

			if (writer == null)
				InitWriter ();

			Type type = value.GetType ();
			
			TypeConverter converter = TypeDescriptor.GetConverter (value);
			if (converter != null && converter.CanConvertTo (typeof (string)) && converter.CanConvertFrom (typeof (string))) {
				string str = (string)converter.ConvertToInvariantString (value);
				writer.WriteStartElement ("metadata");
				writer.WriteAttributeString ("name", name);
				if (type != null)
					writer.WriteAttributeString ("type", type.AssemblyQualifiedName);
				writer.WriteStartElement ("value");
				writer.WriteString (str);
				writer.WriteEndElement ();
				writer.WriteEndElement ();
				writer.WriteWhitespace ("\n  ");
				return;
			}

			if (converter != null && converter.CanConvertTo (typeof (byte[])) && converter.CanConvertFrom (typeof (byte[]))) {
				byte[] b = (byte[])converter.ConvertTo (value, typeof (byte[]));
				writer.WriteStartElement ("metadata");
				writer.WriteAttributeString ("name", name);

				if (type != null) {
					writer.WriteAttributeString ("type", type.AssemblyQualifiedName);
					writer.WriteAttributeString ("mimetype", ByteArraySerializedObjectMimeType);
					writer.WriteStartElement ("value");
					WriteNiceBase64 (b, 0, b.Length);
				} else {
					writer.WriteAttributeString ("mimetype", BinSerializedObjectMimeType);
					writer.WriteStartElement ("value");
					writer.WriteBase64 (b, 0, b.Length);
				}

				writer.WriteEndElement ();
				writer.WriteEndElement ();
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

			writer.WriteStartElement ("metadata");
			writer.WriteAttributeString ("name", name);

			if (type != null) {
				writer.WriteAttributeString ("type", type.AssemblyQualifiedName);
				writer.WriteAttributeString ("mimetype", ByteArraySerializedObjectMimeType);
				writer.WriteStartElement ("value");
				WriteNiceBase64 (ms.GetBuffer (), 0, ms.GetBuffer ().Length);
			} else {
				writer.WriteAttributeString ("mimetype", BinSerializedObjectMimeType);
				writer.WriteStartElement ("value");
				writer.WriteBase64 (ms.GetBuffer (), 0, ms.GetBuffer ().Length);
			}

			writer.WriteEndElement ();
			writer.WriteEndElement ();
			ms.Close ();
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
		
		public virtual void Dispose ()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void Generate ()
		{
			if (written)
				throw new InvalidOperationException ("The resource is already generated.");

			written = true;
			
			if (writer == null)
				InitWriter();
			
			writer.WriteEndElement ();
			writer.Flush ();
		}

		protected virtual void Dispose (bool disposing)
		{
			if (disposing)
				Close();
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

		#region Public Properties
		public string BasePath {
			get { return base_path; }
			set { base_path = value; }
		}
		#endregion
	}
}
