//
// System.Resources.ResXResourceReader.cs
//
// Authors: 
// 	Duncan Mak <duncan@ximian.com>
//	Nick Drochak <ndrochak@gol.com>
//	Paolo Molaro <lupus@ximian.com>
//
// 2001 (C) Ximian Inc, http://www.ximian.com
//
// TODO: Finish this

using System.Collections;
using System.Resources;
using System.IO;
using System.Xml;

namespace System.Resources
{
	public sealed class ResXResourceReader : IResourceReader, IDisposable
	{
		Stream stream;
		XmlTextReader reader;
		Hashtable hasht;

		// Constructors
		public ResXResourceReader (Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException ("Value cannot be null.");
			
			if (!stream.CanRead)
				throw new ArgumentException ("Stream was not readable.");

			this.stream = stream;
			reader = new XmlTextReader (stream);
			
			if (!IsStreamValid()){
				throw new ArgumentException("Stream is not a valid .resources file!  It was possibly truncated.");
			}
			load_data ();
		}
		
		public ResXResourceReader (string fileName)
		{
			if (fileName == null)
				throw new ArgumentException ("Path cannot be null.");
			
			if (String.Empty == fileName)
				throw new ArgumentException("Empty path name is not legal.");

			if (!System.IO.File.Exists (fileName)) 
				throw new FileNotFoundException ("Could not find file " + Path.GetFullPath(fileName));

			stream = new FileStream (fileName, FileMode.Open);
			reader = new XmlTextReader (stream);

			if (!IsStreamValid()){
				throw new ArgumentException("Stream is not a valid .resources file!  It was possibly truncated.");
			}
			load_data ();
		}
		
		static string get_attr (XmlTextReader reader, string name) {
			if (!reader.HasAttributes)
				return null;
			for (int i = 0; i < reader.AttributeCount; i++) {
				reader.MoveToAttribute (i);
				if (reader.Name == name) {
					string v = reader.Value;
					reader.MoveToElement ();
					return v;
				}
			}
			reader.MoveToElement ();
			return null;
		}

		static string get_value (XmlTextReader reader, string name) {
			bool gotelement = false;
			while (reader.Read ()) {
				if (reader.NodeType == XmlNodeType.Element && reader.Name == name) {
					gotelement = true;
					break;
				}
			}
			if (!gotelement)
				return null;
			while (reader.Read ()) {
				if (reader.NodeType == XmlNodeType.Text) {
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
				if (reader.NodeType == XmlNodeType.Element && reader.Name == "root") {
					gotroot = true;
					break;
				}
			}
			if (!gotroot)
				return false;
			while (reader.Read ()) {
				if (reader.NodeType == XmlNodeType.Element && reader.Name == "resheader") {
					string v = get_attr (reader, "name");
					if (v != null && v == "resmimetype") {
						v = get_value (reader, "value");
						if (v == "text/microsoft-resx") {
							gotmime = true;
							break;
						}
					}
				}
			}
			return gotmime;
		}

		private void load_data ()
		{
			hasht = new Hashtable ();
			while (reader.Read ()) {
				if (reader.NodeType == XmlNodeType.Element && reader.Name == "data") {
					string n = get_attr (reader, "name");
					if (n != null) {
						string v = get_value (reader, "value");
						hasht [n] = v;
					}
				}
			}
		}

		public void Close ()
		{
			stream.Close ();
			stream = null;
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
		
		[MonoTODO]
		void IDisposable.Dispose ()
		{
			// FIXME: is this all we need to do?
			Close();
		}
		
	}  // public sealed class ResXResourceReader
} // namespace System.Resources
