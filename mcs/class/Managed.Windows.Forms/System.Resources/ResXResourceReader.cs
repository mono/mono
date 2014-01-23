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
//	Olivier Dufour	olivier.duff@gmail.com
//	Gary Barnett	gary.barnett.mono@gmail.com

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
using System.Reflection;
using System.Drawing;
using System.Runtime.Serialization;

namespace System.Resources
{
#if INSIDE_SYSTEM_WEB
	internal
#else
	public
#endif
	class ResXResourceReader : IResourceReader, IDisposable
	{
		#region Local Variables
		private string fileName;
		private Stream stream;
		private TextReader reader;
		private Hashtable hasht;
		private ITypeResolutionService typeresolver;
		private XmlTextReader xmlReader;
		private string basepath;
		private bool useResXDataNodes;
		private AssemblyName [] assemblyNames;
		private Hashtable hashtm;
		#endregion	// Local Variables

		#region Constructors & Destructor
		public ResXResourceReader (Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException ("stream");

			if (!stream.CanRead)
				throw new ArgumentException ("Stream was not readable.");

			this.stream = stream;
		}

		public ResXResourceReader (Stream stream, ITypeResolutionService typeResolver)
			: this (stream)
		{
			this.typeresolver = typeResolver;
		}

		public ResXResourceReader (string fileName)
		{
			this.fileName = fileName;
		}

		public ResXResourceReader (string fileName, ITypeResolutionService typeResolver)
			: this (fileName)
		{
			this.typeresolver = typeResolver;
		}

		public ResXResourceReader (TextReader reader)
		{
			this.reader = reader;
		}

		public ResXResourceReader (TextReader reader, ITypeResolutionService typeResolver)
			: this (reader)
		{
			this.typeresolver = typeResolver;
		}

		public ResXResourceReader (Stream stream, AssemblyName [] assemblyNames)
			: this (stream)
		{
			this.assemblyNames = assemblyNames;
		}

		public ResXResourceReader (string fileName, AssemblyName [] assemblyNames)
			: this (fileName)
		{
			this.assemblyNames = assemblyNames;
		}

		public ResXResourceReader (TextReader reader, AssemblyName [] assemblyNames)
			: this (reader)
		{
			this.assemblyNames = assemblyNames;
		}

		~ResXResourceReader ()
		{
			Dispose (false);
		}
		#endregion	// Constructors & Destructor

		public string BasePath {
			get { return basepath; }
			set { basepath = value; }
		}

		public bool UseResXDataNodes {
			get { return useResXDataNodes; }
			set {
				if (xmlReader != null)
					throw new InvalidOperationException ();
				useResXDataNodes = value; 
			}
		}

		#region Private Methods
		private void LoadData ()
		{
			hasht = new Hashtable ();
			hashtm = new Hashtable ();
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
							ParseDataNode (false);
							break;
						case "metadata":
							ParseDataNode (true);
							break;
						}
					}
				} catch (XmlException ex) {
					throw new ArgumentException ("Invalid ResX input.", ex);
				} catch (SerializationException ex) {
					throw ex;
				} catch (TargetInvocationException ex) {
					throw ex;
				} catch (Exception ex) {
					XmlException xex = new XmlException (ex.Message, ex, 
						xmlReader.LineNumber, xmlReader.LinePosition);
					throw new ArgumentException ("Invalid ResX input.", xex);
				}
				header.Verify ();
			} finally {
				if (fileName != null) {
					stream.Close ();
					stream = null;
				}
				xmlReader = null;
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

		private string GetDataValue (bool meta, out string comment)
		{
			string value = null;
			comment = null;
			while (xmlReader.Read ()) {
				if (xmlReader.NodeType == XmlNodeType.EndElement && xmlReader.LocalName == (meta ? "metadata" : "data"))
					break;

				if (xmlReader.NodeType == XmlNodeType.Element) {
					if (xmlReader.Name.Equals ("value")) {
						xmlReader.WhitespaceHandling = WhitespaceHandling.Significant;
						value = xmlReader.ReadString ();
						xmlReader.WhitespaceHandling = WhitespaceHandling.None;
					} else if (xmlReader.Name.Equals ("comment")) {
						xmlReader.WhitespaceHandling = WhitespaceHandling.Significant;
						comment = xmlReader.ReadString ();
						xmlReader.WhitespaceHandling = WhitespaceHandling.None;
						if (xmlReader.NodeType == XmlNodeType.EndElement && xmlReader.LocalName == (meta ? "metadata" : "data"))
							break;
					}
				}
				else
					value = xmlReader.Value.Trim ();
			}
			return value;
		}

		private void ParseDataNode (bool meta)
		{
			Hashtable hashtable = ((meta && ! useResXDataNodes) ? hashtm : hasht);
			Point pos = new Point (xmlReader.LineNumber, xmlReader.LinePosition);
			string name = GetAttribute ("name");
			string type_name = GetAttribute ("type");
			string mime_type = GetAttribute ("mimetype");


			string comment = null;
			string value = GetDataValue (meta, out comment);

			ResXDataNode node = new ResXDataNode (name, mime_type, type_name, value, comment, pos, BasePath);

			if (useResXDataNodes) {
				hashtable [name] = node;
				return;
			}

			if (name == null)
				throw new ArgumentException (string.Format (CultureInfo.CurrentCulture,
							"Could not find a name for a resource. The resource value was '{0}'.",
				                        node.GetValue ((AssemblyName []) null).ToString()));

			// useResXDataNodes is false, add to dictionary of values
			if (assemblyNames != null) { 
				try {
					hashtable [name] = node.GetValue (assemblyNames);
				} catch (TypeLoadException ex) {
					// different error messages depending on type of resource, hacky solution
					if (node.handler is TypeConverterFromResXHandler)
						hashtable [name] = null;
					else 
						throw ex;
				}
			} else { // there is a typeresolver or its null
				try {
					hashtable [name] = node.GetValue (typeresolver); 
				} catch (TypeLoadException ex) {
					if (node.handler is TypeConverterFromResXHandler)
						hashtable [name] = null;
					else 
						throw ex;
				}
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

		public static ResXResourceReader FromFileContents (string fileContents, AssemblyName [] assemblyNames)
		{
			return new ResXResourceReader (new StringReader (fileContents), assemblyNames);
		}

		public IDictionaryEnumerator GetMetadataEnumerator ()
		{
			if (hashtm == null)
				LoadData ();
			return hashtm.GetEnumerator ();
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
