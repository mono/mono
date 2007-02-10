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

		static string get_attr (XmlTextReader xtr, string name)
		{
			if (!xtr.HasAttributes)
				return null;
			for (int i = 0; i < xtr.AttributeCount; i++) {
				xtr.MoveToAttribute (i);
				if (String.Compare (xtr.Name, name, true) == 0) {
					string v = xtr.Value;
					xtr.MoveToElement ();
					return v;
				}
			}
			xtr.MoveToElement ();
			return null;
		}

		private void load_data ()
		{
			if (fileName != null) {
				stream = File.OpenRead (fileName);
			}

			try {
				XmlTextReader xtr = null;
				if (stream != null) {
					xtr = new XmlTextReader (stream);
				} else if (reader != null) {
					xtr = new XmlTextReader (reader);
				}

				if (xtr == null) {
					throw new InvalidOperationException ("ResourceReader is closed.");
				}

				xtr.WhitespaceHandling = WhitespaceHandling.None;

				ResXHeader header = new ResXHeader ();
				try {
					while (xtr.Read ()) {
						if (xtr.NodeType != XmlNodeType.Element)
							continue;

						switch (xtr.LocalName) {
						case "resheader":
							parse_header_node (xtr, header);
							break;
						case "data":
							parse_data_node (xtr);
							break;
						}
					}
#if NET_2_0
				} catch (XmlException ex) {
					throw new ArgumentException ("Invalid ResX input.", ex);
				} catch (Exception ex) {
					XmlException xex = new XmlException (ex.Message, ex, 
						xtr.LineNumber, xtr.LinePosition);
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

		void parse_header_node (XmlTextReader xtr, ResXHeader header)
		{
			string v = get_attr (xtr, "name");
			if (v == null)
				return;

			if (String.Compare (v, "resmimetype", true) == 0) {
				header.ResMimeType = get_header_value (xtr);
			} else if (String.Compare (v, "reader", true) == 0) {
				header.Reader = get_header_value (xtr);
			} else if (String.Compare (v, "version", true) == 0) {
				header.Version = get_header_value (xtr);
			} else if (String.Compare (v, "writer", true) == 0) {
				header.Writer = get_header_value (xtr);
			}
		}

		string get_header_value (XmlTextReader xtr)
		{
			string value = null;
			xtr.ReadStartElement ();
			if (xtr.NodeType == XmlNodeType.Element) {
				value = xtr.ReadElementString ();
			} else {
				value = xtr.Value.Trim ();
			}
			return value;
		}

		void parse_data_node (XmlTextReader xtr)
		{
			string name = get_attr (xtr, "name");
			string type_name = get_attr (xtr, "type");
			string mime_type = get_attr (xtr, "mimetype");

			Type type = type_name == null ? null : ResolveType (type_name);

			if (type_name != null && type == null)
				throw new ArgumentException (String.Format (
					"The type '{0}' of the element '{1}' could not be resolved.", type_name, name));

			if (type == typeof (ResXNullRef)) {
				hasht [name] = null;
				return;
			}

			string value = get_data_value (xtr);
			object obj = null;

			if (type != null) {
				TypeConverter c = TypeDescriptor.GetConverter (type);

				if (c == null) {
					obj = null;
				} else if (c.CanConvertFrom (typeof (string))) {
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
				} else if (c.CanConvertFrom (typeof (byte []))) {
					obj = c.ConvertFrom (Convert.FromBase64String (value));
				} else {
					// the type must be a byte[]
					obj = Convert.FromBase64String (value);
				}
			} else if (mime_type != null && mime_type != String.Empty) {
				if (mime_type == ResXResourceWriter.BinSerializedObjectMimeType) {
					byte [] data = Convert.FromBase64String (value);
					BinaryFormatter f = new BinaryFormatter ();
					using (MemoryStream s = new MemoryStream (data)) {
						obj = f.Deserialize (s);
					}
				} else {
					// invalid mime type
#if NET_2_0
					obj = null;
#else
					obj = value;
#endif
				}
			} else {
				obj = value;
			}

			if (name == null)
				throw new ArgumentException (string.Format (CultureInfo.CurrentCulture,
					"Could not find a name for a resource. The resource value "
					+ "was '{0}'.", obj));

			hasht [name] = obj;
		}


		// Returns the value of the next XML element with the specified
		// name from the reader. canBeCdata == true specifies that
		// the element may be a CDATA node as well.
		static string get_data_value (XmlTextReader xtr)
		{
			string value = null;
#if NET_2_0
			while (xtr.Read ()) {
				if (xtr.NodeType == XmlNodeType.EndElement && xtr.LocalName == "data")
					break;
				if (xtr.NodeType == XmlNodeType.Element) {
					if (xtr.Name == "value") {
						value = xtr.ReadString ();
					}
					continue;
				}
				value = xtr.Value.Trim ();
			}
#else
			xtr.Read ();
			if (xtr.NodeType == XmlNodeType.Element) {
				value = xtr.ReadElementString ();
			} else {
				value = xtr.Value.Trim ();
			}

			if (value == null)
				value = string.Empty;
#endif
			return value;
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
				load_data ();
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

		private class ResXHeader
		{
			public string ResMimeType {
				get { return _resMimeType; }
				set { _resMimeType = value; }
			}

			public string Reader {
				get { return _reader; }
				set { _reader = value; }
			}

			public string Version {
				get { return _version; }
				set { _version = value; }
			}

			public string Writer {
				get { return _writer; }
				set { _writer = value; }
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

			private string _resMimeType;
			private string _reader;
			private string _version;
			private string _writer;
		}
	}
}
