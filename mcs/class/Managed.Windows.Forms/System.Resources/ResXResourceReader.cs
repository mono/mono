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
//

// COMPLETE

using System;
using System.Collections;
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
		private Stream			stream;
		private XmlTextReader		reader;
		private Hashtable		hasht;
		private ITypeResolutionService	typeresolver;
		#endregion	// Local Variables

		#region Constructors & Destructor
		public ResXResourceReader (Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException ("Value cannot be null.");
			
			if (!stream.CanRead)
				throw new ArgumentException ("Stream was not readable.");

			this.stream = stream;
			basic_setup ();
		}

		public ResXResourceReader (Stream stream, ITypeResolutionService typeresolver) : this(stream) {
			this.typeresolver = typeresolver;
		}
		
		public ResXResourceReader (string fileName)
		{
			stream = File.OpenRead (fileName);
			basic_setup ();
		}

		public ResXResourceReader (string fileName, ITypeResolutionService typeresolver) : this(fileName) {
			this.typeresolver = typeresolver;
		}

		public ResXResourceReader(TextReader reader) {
			this.reader = new XmlTextReader(reader);
			this.hasht = new Hashtable();

			load_data();
		}

		public ResXResourceReader (TextReader reader, ITypeResolutionService typeresolver) : this(reader) {
			this.typeresolver = typeresolver;
		}

		~ResXResourceReader() {
			Dispose(false);
		}
		#endregion	// Constructors & Destructor


		#region Private Methods
		void basic_setup () {
			reader = new XmlTextReader (stream);
			hasht = new Hashtable ();

			if (!IsStreamValid()){
				throw new ArgumentException("Stream is not a valid .resx file!  It was possibly truncated.");
			}
			load_data ();
		}
		
		static string get_attr (XmlTextReader reader, string name) {
			if (!reader.HasAttributes)
				return null;
			for (int i = 0; i < reader.AttributeCount; i++) {
				reader.MoveToAttribute (i);
				if (String.Compare (reader.Name, name, true) == 0) {
					string v = reader.Value;
					reader.MoveToElement ();
					return v;
				}
			}
			reader.MoveToElement ();
			return null;
		}


		static string get_value (XmlTextReader reader, string name)
		{
			return get_value (reader, name, true);
		}

		// Returns the value of the next XML element with the specified
		// name from the reader. canBeCdata == true specifies that
		// the element may be a CDATA node as well.
		static string get_value (XmlTextReader reader, string name, bool canBeCdata) {
			bool gotelement = false;
			while (reader.Read ()) {
				if (reader.NodeType == XmlNodeType.Element && String.Compare (reader.Name, name, true) == 0) {
					gotelement = true;
					break;
				}
				if (canBeCdata && reader.NodeType == XmlNodeType.CDATA)
				   return reader.Value;
			}
			if (!gotelement)
				return null;
			while (reader.Read ()) {
				if (reader.NodeType == XmlNodeType.Text || reader.NodeType == XmlNodeType.CDATA) {
					string v = reader.Value;
					return v;
				}
				else if (reader.NodeType == XmlNodeType.EndElement && reader.Value == string.Empty)
				{
					string v = reader.Value;
					return v;
				}
			}
			return null;
		}

		private bool IsStreamValid() {
			bool gotroot = false;
			bool gotmime = false;
			
			while (reader.Read ()) {
				if (reader.NodeType == XmlNodeType.Element && String.Compare (reader.Name, "root", true) == 0) {
					gotroot = true;
					break;
				}
			}
			if (!gotroot)
				return false;
			while (reader.Read ()) {
				if (reader.NodeType == XmlNodeType.Element && String.Compare (reader.Name, "resheader", true) == 0) {
					string v = get_attr (reader, "name");
					if (v != null && String.Compare (v, "resmimetype", true) == 0) {
						v = get_value (reader, "value");
						if (String.Compare (v, "text/microsoft-resx", true) == 0) {
							gotmime = true;
							break;
						}
					}
				} else if (reader.NodeType == XmlNodeType.Element && String.Compare (reader.Name, "data", true) == 0) {
					/* resheader apparently can appear anywhere, so we collect
					 * the data even if we haven't validated yet.
					 */
					parse_data_node ();
				}
			}
			return gotmime;
		}

		private void load_data ()
		{
			while (reader.Read ()) {
				if (reader.NodeType == XmlNodeType.Element && String.Compare (reader.Name, "data", true) == 0)
					parse_data_node ();

			}
		}

		void parse_data_node ()
		{
			string name = get_attr (reader, "name");
			string type_name = get_attr (reader, "type");
			string mime_type = get_attr (reader, "mimetype");

			if (name == null)
				return;

			Type type = type_name == null ? null : ResolveType (type_name);

			if (type_name != null && type == null)
				throw new ArgumentException (String.Format (
					"The type '{0}' of the element '{1}' could not be resolved.", type_name, name));

			if (type == typeof (ResXNullRef)) {
				hasht [name] = null;
				return;
			}

			string value = get_value (reader, "value");
			object obj = null;

			if (type != null) {
				TypeConverter c = TypeDescriptor.GetConverter (type);

				if (c == null) {
					obj = null;
				} else if (c.CanConvertFrom (typeof (string))) {
					obj = c.ConvertFromInvariantString (value);
				} else if (c.CanConvertFrom (typeof (byte[]))) {
					obj = c.ConvertFrom (Convert.FromBase64String (value));
				} else {
					// the type must be a byte[]
					obj  = Convert.FromBase64String(value);
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
			hasht [name] = obj;
		}

		private Type ResolveType (string type) {
			if (typeresolver == null) {
				return Type.GetType(type);
			} else {
				return typeresolver.GetType(type);
			}
		}
		#endregion	// Private Methods

		#region Public Methods
		public void Close ()
		{
			if (stream != null) {
				stream.Close ();
				stream = null;
			}

			if (reader != null) {
				reader.Close();
				reader = null;
			}
		}
		
		public IDictionaryEnumerator GetEnumerator () {
			if (null == stream){
				throw new InvalidOperationException("ResourceReader is closed.");
			}
			else {
				return hasht.GetEnumerator ();
			}
		}
		
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return ((IResourceReader) this).GetEnumerator();
		}
		
		void IDisposable.Dispose ()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing) {
			if (disposing) {
				Close();
			}
		}

		public static ResXResourceReader FromFileContents(string fileContents) {
			return new ResXResourceReader(new StringReader(fileContents));
		}

		public static ResXResourceReader FromFileContents(string fileContents, ITypeResolutionService typeResolver) {
			return new ResXResourceReader(new StringReader(fileContents), typeResolver);
		}

		#endregion	// Public Methods
		
	}  // public sealed class ResXResourceReader
} // namespace System.Resources
