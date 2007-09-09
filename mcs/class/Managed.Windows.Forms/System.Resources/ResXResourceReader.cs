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
// 	Duncan Mak	duncan@ximian.com
//	Nick Drochak	ndrochak@gol.com
//	Paolo Molaro	lupus@ximian.com
//	Peter Bartok	pbartok@novell.com
//	Gert Driesen	drieseng@users.sourceforge.net
//

using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;

namespace System.Resources
{
	public class ResXResourceReader : IResourceReader, IDisposable
	{
		#region Local Variables
		private string fileName;
		private Stream stream;
		private TextReader reader;
		private Hashtable hasht;
		private ITypeResolutionService typeresolver;

		private XmlTextReader xmlReader;

#if NET_2_0
		private string basepath;
#endif
		#endregion	// Local Variables

		#region Constructors & Destructor
		public ResXResourceReader (Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException ("Value cannot be null.");

			if (!stream.CanRead)
				throw new ArgumentException ("Stream was not readable.");

			this.stream = stream;
		}

		public ResXResourceReader (Stream stream, ITypeResolutionService typeresolver)
			: this (stream)
		{
			this.typeresolver = typeresolver;
		}

		public ResXResourceReader (string fileName)
		{
			this.fileName = fileName;
		}

		public ResXResourceReader (string fileName, ITypeResolutionService typeresolver)
			: this (fileName)
		{
			this.typeresolver = typeresolver;
		}

		public ResXResourceReader (TextReader reader)
		{
			this.reader = reader;
		}

		public ResXResourceReader (TextReader reader, ITypeResolutionService typeresolver)
			: this (reader)
		{
			this.typeresolver = typeresolver;
		}

		~ResXResourceReader ()
		{
			Dispose (false);
		}
		#endregion	// Constructors & Destructor

#if NET_2_0
		public string BasePath {
			get { return basepath; }
			set { basepath = value; }
		}
#endif

		#region Private Methods
		private void LoadData ()
		{
			if (fileName != null) {
				stream = File.OpenRead (fileName);
			}

			try {
				xmlReader = null;
				if (stream != null) {
					xmlReader = new XmlTextReader (stream);
				} else if (reader != null) {
					xmlReader = new XmlTextReader (reader);
				}

				if (xmlReader == null) {
					throw new InvalidOperationException ("ResourceReader is closed.");
				}

				xmlReader.WhitespaceHandling = WhitespaceHandling.None;

				ResXHeader header = new ResXHeader ();
				try {
					while (xmlReader.Read ()) {
						if (xmlReader.NodeType != XmlNodeType.Element)
							continue;

						switch (xmlReader.LocalName) {
						case "resheader":
							ParseHeaderNode (header);
							break;
						case "data":
							ParseDataNode ();
							break;
						}
					}
#if NET_2_0
				} catch (XmlException ex) {
					throw new ArgumentException ("Invalid ResX input.", ex);
				} catch (Exception ex) {
					XmlException xex = new XmlException (ex.Message, ex, 
						xmlReader.LineNumber, xmlReader.LinePosition);
					throw new ArgumentException ("Invalid ResX input.", xex);
				}
#else
				} catch (Exception ex) {
					throw new ArgumentException ("Invalid ResX input.", ex);
				}
#endif

				header.Verify ();
			} finally {
				if (fileName != null) {
					stream.Close ();
					stream = null;
				}
			}
		}

		private void ParseHeaderNode (ResXHeader header)
		{
			string v = GetAttribute ("name");
			if (v == null)
				return;

			if (String.Compare (v, "resmimetype", true) == 0) {
				header.ResMimeType = GetHeaderValue ();
			} else if (String.Compare (v, "reader", true) == 0) {
				header.Reader = GetHeaderValue ();
			} else if (String.Compare (v, "version", true) == 0) {
				header.Version = GetHeaderValue ();
			} else if (String.Compare (v, "writer", true) == 0) {
				header.Writer = GetHeaderValue ();
			}
		}

		private string GetHeaderValue ()
		{
			string value = null;
			xmlReader.ReadStartElement ();
			if (xmlReader.NodeType == XmlNodeType.Element) {
				value = xmlReader.ReadElementString ();
			} else {
				value = xmlReader.Value.Trim ();
			}
			return value;
		}

		private string GetAttribute (string name)
		{
			if (!xmlReader.HasAttributes)
				return null;
			for (int i = 0; i < xmlReader.AttributeCount; i++) {
				xmlReader.MoveToAttribute (i);
				if (String.Compare (xmlReader.Name, name, true) == 0) {
					string v = xmlReader.Value;
					xmlReader.MoveToElement ();
					return v;
				}
			}
			xmlReader.MoveToElement ();
			return null;
		}

		private string GetDataValue ()
		{
			string value = null;
#if NET_2_0
			while (xmlReader.Read ()) {
				if (xmlReader.NodeType == XmlNodeType.EndElement && xmlReader.LocalName == "data")
					break;

				if (xmlReader.NodeType == XmlNodeType.Element) {
					if (xmlReader.Name.Equals ("value")) {
						xmlReader.WhitespaceHandling = WhitespaceHandling.Significant;
						value = xmlReader.ReadString ();
						xmlReader.WhitespaceHandling = WhitespaceHandling.None;
					} else if (xmlReader.Name.Equals ("comment")) {
						xmlReader.Skip ();
						if (xmlReader.NodeType == XmlNodeType.EndElement && xmlReader.LocalName == "data")
							break;
					}
				}
				else
					value = xmlReader.Value.Trim ();
			}
#else
			xmlReader.Read ();
			if (xmlReader.NodeType == XmlNodeType.Element) {
				value = xmlReader.ReadElementString ();
			} else {
				value = xmlReader.Value.Trim ();
			}

			if (value == null)
				value = string.Empty;
#endif
			return value;
		}

		private void ParseDataNode ()
		{
			string name = GetAttribute ("name");
			string type_name = GetAttribute ("type");
			string mime_type = GetAttribute ("mimetype");

			Type type = type_name == null ? null : ResolveType (type_name);

			if (type_name != null && type == null)
				throw new ArgumentException (String.Format (
					"The type '{0}' of the element '{1}' could not be resolved.", type_name, name));

			if (type == typeof (ResXNullRef)) {
				hasht [name] = null;
				return;
			}

			string value = GetDataValue ();
			object obj = null;

			if (mime_type != null && mime_type.Length > 0) {
				if (mime_type == ResXResourceWriter.BinSerializedObjectMimeType) {
					byte [] data = Convert.FromBase64String (value);
					BinaryFormatter f = new BinaryFormatter ();
					using (MemoryStream s = new MemoryStream (data)) {
						obj = f.Deserialize (s);
					}
				} else if (mime_type == ResXResourceWriter.ByteArraySerializedObjectMimeType) {
					if (type != null) {
						TypeConverter c = TypeDescriptor.GetConverter (type);
						if (c.CanConvertFrom (typeof (byte [])))
							obj = c.ConvertFrom (Convert.FromBase64String (value));
					}
				}
			} else if (type != null) {
				if (type == typeof (byte [])) {
					obj = Convert.FromBase64String (value);
				} else {
					TypeConverter c = TypeDescriptor.GetConverter (type);
					if (c.CanConvertFrom (typeof (string))) {
#if NET_2_0
						if (BasePath != null && type == typeof (ResXFileRef)) {
							string [] parts = ResXFileRef.Parse (value);
							parts [0] = Path.Combine (BasePath, parts [0]);
							obj = c.ConvertFromInvariantString (string.Join (";", parts));
						} else {
							obj = c.ConvertFromInvariantString (value);
						}
#else
						obj = c.ConvertFromInvariantString (value);
#endif
					}
				}
			} else {
				obj = value;
			}

#if ONLY_1_1
			if (obj == null)
				obj = value;
#endif

			if (name == null)
				throw new ArgumentException (string.Format (CultureInfo.CurrentCulture,
					"Could not find a name for a resource. The resource value "
					+ "was '{0}'.", obj));

			hasht [name] = obj;
		}

		private Type ResolveType (string type)
		{
			if (typeresolver == null) {
				return Type.GetType (type);
			} else {
				return typeresolver.GetType (type);
			}
		}
		#endregion	// Private Methods

		#region Public Methods
		public void Close ()
		{
			if (reader != null) {
				reader.Close ();
				reader = null;
			}
		}

		public IDictionaryEnumerator GetEnumerator ()
		{
			if (hasht == null) {
				hasht = new Hashtable ();
				LoadData ();
			}
			return hasht.GetEnumerator ();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return ((IResourceReader) this).GetEnumerator ();
		}

		void IDisposable.Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (disposing) {
				Close ();
			}
		}

		public static ResXResourceReader FromFileContents (string fileContents)
		{
			return new ResXResourceReader (new StringReader (fileContents));
		}

		public static ResXResourceReader FromFileContents (string fileContents, ITypeResolutionService typeResolver)
		{
			return new ResXResourceReader (new StringReader (fileContents), typeResolver);
		}

		#endregion	// Public Methods

		#region Internal Classes
		private class ResXHeader
		{
			private string resMimeType;
			private string reader;
			private string version;
			private string writer;

			public string ResMimeType
			{
				get { return resMimeType; }
				set { resMimeType = value; }
			}

			public string Reader {
				get { return reader; }
				set { reader = value; }
			}

			public string Version {
				get { return version; }
				set { version = value; }
			}

			public string Writer {
				get { return writer; }
				set { writer = value; }
			}

			public void Verify ()
			{
				if (!IsValid)
					throw new ArgumentException ("Invalid ResX input.  Could "
						+ "not find valid \"resheader\" tags for the ResX "
						+ "reader & writer type names.");
			}

			public bool IsValid {
				get {
					if (string.Compare (ResMimeType, ResXResourceWriter.ResMimeType) != 0)
						return false;
					if (Reader == null || Writer == null)
						return false;
					string readerType = Reader.Split (',') [0].Trim ();
					if (readerType != typeof (ResXResourceReader).FullName)
						return false;
					string writerType = Writer.Split (',') [0].Trim ();
					if (writerType != typeof (ResXResourceWriter).FullName)
						return false;
					return true;
				}
			}
		}
		#endregion
	}
}
