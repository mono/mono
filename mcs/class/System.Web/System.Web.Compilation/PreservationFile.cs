//
// System.Web.Compilation.AppCodeCompiler: A compiler for the App_Code folder
//
// Authors:
//   Marek Habersack (grendello@gmail.com)
//
// (C) 2006 Marek Habersack
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
#if NET_2_0
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace System.Web.Compilation 
{
	enum BuildResultTypeCode
	{
		Unknown = 0,
		AppCodeSubFolder = 1,
		Handler = 2,
		PageOrControl = 3,
		AppCode = 6,
		Global = 8,
		TopLevelAssembly = 9
	}

	//
	// The attributes of the <preserve> element in a .compiled file are described in
	//
	// http://msdn.microsoft.com/msdnmag/issues/07/01/cuttingedge/default.aspx?loc=&fig=true#fig4
	//
	// and a sample file is shown in
	//
	// http://msdn.microsoft.com/msdnmag/issues/07/01/cuttingedge/default.aspx?loc=&fig=true#fig3
	//
	internal class PreservationFile
	{
		string _filePath;
		string _assembly;
		Int32 _fileHash;
		Int32 _flags;
		Int32 _hash;
		BuildResultTypeCode _resultType = BuildResultTypeCode.Unknown;
		string _virtualPath;
		List <string> _filedeps;

		public string Assembly {
			get { return _assembly; }
			set { _assembly = value; }
		}

		public string FilePath {
			get { return _filePath; }
			set { _filePath = value; }
		}
		
		public Int32 FileHash {
			get { return _fileHash; }
			set { _fileHash = value; }
		}

		public int Flags {
			get { return _flags; }
			set { _flags = value; }
		}

		public Int32 Hash {
			get { return _hash; }
			set { _hash = value; }
		}

		public BuildResultTypeCode ResultType {
			get { return _resultType; }
			set { _resultType = value; }
		}

		public string VirtualPath {
			get { return _virtualPath; }
			set { _virtualPath = value; }
		}

		public List <string> FileDeps {
			get { return _filedeps; }
			set { _filedeps = value; }
		}

		public PreservationFile ()
		{
		}
		
		public PreservationFile (string filePath)
		{
			this._filePath = filePath;
			Parse (filePath);
		}

		public void Parse ()
		{
			if (_filePath == null)
				throw new InvalidOperationException ("File path is not defined");
			Parse (_filePath);
		}

		public void Parse (string filePath)
		{
			if (filePath == null)
				throw new ArgumentNullException ("File path is required", "filePath");
			
			XmlDocument doc = new XmlDocument ();
			doc.Load (filePath);
			
			XmlNode root = doc.DocumentElement;
			if (root.Name != "preserve")
				throw new InvalidOperationException ("Invalid assembly mapping file format");
			ParseRecursively (root);
		}

		void ParseRecursively (XmlNode root)
		{
			_assembly = GetNonEmptyRequiredAttribute (root, "assembly");

			// The rest of the values is optional for us and since we don't use them
			// at all (at least for now) we also ignore all the integer parsing errors
			try {
				_virtualPath = GetNonEmptyOptionalAttribute (root, "virtualPath");
				_fileHash = GetNonEmptyOptionalAttributeInt32 (root, "filehash");
				_hash = GetNonEmptyOptionalAttributeInt32 (root, "hash");
				_flags = GetNonEmptyOptionalAttributeInt32 (root, "flags");
				_resultType = (BuildResultTypeCode) GetNonEmptyOptionalAttributeInt32 (root, "resultType");

				foreach (XmlNode child in root.ChildNodes) {
					if (child.NodeType != XmlNodeType.Element)
						continue;
					if (child.Name != "filedeps")
						continue;
					ReadFileDeps (child);
				}
			} catch (Exception) {
			}
		}

		void ReadFileDeps (XmlNode node)
		{
			string tmp;
			if (_filedeps == null)
				_filedeps = new List <string> ();
			foreach (XmlNode child in node.ChildNodes) {
				if (child.NodeType != XmlNodeType.Element)
					continue;
				if (child.Name != "filedep")
					continue;
				tmp = GetNonEmptyRequiredAttribute (child, "name");
				_filedeps.Add (tmp);
			}
		}
		
		public void Save ()
		{
			if (_filePath == null)
				throw new InvalidOperationException ("File path is not defined");
			Save (_filePath);
		}

		public void Save (string filePath)
		{
			if (filePath == null)
				throw new ArgumentNullException ("File path is required", "filePath");

			XmlWriterSettings xmlSettings = new XmlWriterSettings ();
			xmlSettings.Indent = false;
			xmlSettings.OmitXmlDeclaration = false;
			xmlSettings.NewLineOnAttributes = false;
			
			using (XmlWriter xml = XmlWriter.Create (filePath, xmlSettings)) {
				xml.WriteStartElement ("preserve");
				xml.WriteAttributeString ("assembly", _assembly);
				if (!String.IsNullOrEmpty (_virtualPath))
					xml.WriteAttributeString ("virtualPath", _virtualPath);
				if (_fileHash != 0)
					xml.WriteAttributeString ("filehash", _fileHash.ToString ());
				if (_flags != 0)
					xml.WriteAttributeString ("flags", _flags.ToString ());
				if (_hash != 0)
					xml.WriteAttributeString ("hash", _hash.ToString ());
				if (_resultType != BuildResultTypeCode.Unknown)
					xml.WriteAttributeString ("resultType", ((int)_resultType).ToString ());
				if (_filedeps != null && _filedeps.Count > 0) {
					xml.WriteStartElement ("filedeps");
					foreach (string s in _filedeps) {
						xml.WriteStartElement ("filedep");
						xml.WriteAttributeString ("name", s);
						xml.WriteEndElement ();
					}
					xml.WriteEndElement ();
				}
				xml.WriteEndElement ();
			}
		}

		string GetNonEmptyOptionalAttribute (XmlNode n, string name)
                {
                        return System.Web.Configuration.HandlersUtil.ExtractAttributeValue (name, n, true);
                }
                
		Int32 GetNonEmptyOptionalAttributeInt32 (XmlNode n, string name)
		{
			string tmp = GetNonEmptyOptionalAttribute (n, name);
			if (tmp != null)
				return Int32.Parse (tmp);
			return 0;
		}

		string GetNonEmptyRequiredAttribute (XmlNode n, string name)
                {
                        return System.Web.Configuration.HandlersUtil.ExtractAttributeValue (name, n, false, false);
                }
	}
}
#endif
