//
// System.Configuration.ConfigXmlDocument
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

#if CONFIGURATION_DEP
using System.Configuration.Internal;
#endif
using System.IO;
using System.Security;
using System.Security.Permissions;

#if (XML_DEP)
using System.Xml;

namespace System.Configuration
{
	[PermissionSet (SecurityAction.LinkDemand, Unrestricted = true)]
	public sealed class ConfigXmlDocument : XmlDocument, IConfigXmlNode
#if CONFIGURATION_DEP
		, IConfigErrorInfo
#endif
	{
		XmlTextReader reader;
		string fileName;
		int lineNumber;

		public override XmlAttribute CreateAttribute (string prefix,
							      string localName,
							      string namespaceUri)
		{
			return new ConfigXmlAttribute (this, prefix, localName, namespaceUri);
		}

		public override XmlCDataSection CreateCDataSection (string data)
		{
			return new ConfigXmlCDataSection (this, data);
		}

		public override XmlComment CreateComment (string comment)
		{
			return new ConfigXmlComment (this, comment);
		}

		public override XmlElement CreateElement (string prefix, string localName, string namespaceUri)
		{
			return new ConfigXmlElement (this, prefix, localName, namespaceUri);
		}

		public override XmlSignificantWhitespace CreateSignificantWhitespace (string data)
		{
			return base.CreateSignificantWhitespace (data);
		}

		public override XmlText CreateTextNode (string text)
		{
			return new ConfigXmlText (this, text);
		}

		public override XmlWhitespace CreateWhitespace (string data)
		{
			return base.CreateWhitespace (data);
		}

		public override void Load (string filename)
		{
			XmlTextReader rd = new XmlTextReader (filename);
			try {
				rd.MoveToContent ();
				LoadSingleElement (filename, rd);
			} finally {
				rd.Close ();
			}
		}

		public void LoadSingleElement (string filename, XmlTextReader sourceReader)
		{
			fileName = filename;
			lineNumber = sourceReader.LineNumber;
			string xml = sourceReader.ReadOuterXml();
			reader = new XmlTextReader (new StringReader (xml), sourceReader.NameTable);
			Load (reader);
			reader.Close ();
		}

		public string Filename
		{
			get {
				if ((fileName != null) && (fileName.Length > 0) && SecurityManager.SecurityEnabled) {
					new FileIOPermission (FileIOPermissionAccess.PathDiscovery, fileName).Demand ();
				}
				return fileName;
			}
		}

		public int LineNumber
		{
			get {
				return lineNumber;
			}
		}

#if CONFIGURATION_DEP
		string System.Configuration.Internal.IConfigErrorInfo.Filename {
			get { return Filename; }
		}

		int System.Configuration.Internal.IConfigErrorInfo.LineNumber {
			get { return LineNumber; }
		}
#endif

		string IConfigXmlNode.Filename {
			get { return Filename; }
		}

		int IConfigXmlNode.LineNumber {
			get { return LineNumber; }
		}

		//
		// Wrappers for Xml* that just provide file name and line number addition
		//
		class ConfigXmlAttribute : XmlAttribute, IConfigXmlNode
#if CONFIGURATION_DEP
			, IConfigErrorInfo
#endif
		{
			string fileName;
			int lineNumber;

			public ConfigXmlAttribute (ConfigXmlDocument document,
						   string prefix,
						   string localName,
						   string namespaceUri)
				: base (prefix, localName, namespaceUri, document)
			{
				fileName = document.fileName;
				lineNumber = document.LineNumber;
			}

			public string Filename
			{
				get {
					if ((fileName != null) && (fileName.Length > 0) && SecurityManager.SecurityEnabled) {
						new FileIOPermission (FileIOPermissionAccess.PathDiscovery, fileName).Demand ();
					}
					return fileName;
				}
			}

			public int LineNumber
			{
				get {
					return lineNumber;
				}
			}
		}
		
		class ConfigXmlCDataSection : XmlCDataSection, IConfigXmlNode
#if CONFIGURATION_DEP
			, IConfigErrorInfo
#endif
		{
			string fileName;
			int lineNumber;

			public ConfigXmlCDataSection (ConfigXmlDocument document, string data)
				: base (data, document)
			{
				fileName = document.fileName;
				lineNumber = document.LineNumber;
			}

			public string Filename
			{
				get {
					if ((fileName != null) && (fileName.Length > 0) && SecurityManager.SecurityEnabled) {
						new FileIOPermission (FileIOPermissionAccess.PathDiscovery, fileName).Demand ();
					}
					return fileName;
				}
			}

			public int LineNumber
			{
				get {
					return lineNumber;
				}
			}
		}
		
		class ConfigXmlComment : XmlComment, IConfigXmlNode
		{
			string fileName;
			int lineNumber;

			public ConfigXmlComment (ConfigXmlDocument document, string comment)
				: base (comment, document)
			{
				fileName = document.fileName;
				lineNumber = document.LineNumber;
			}

			public string Filename
			{
				get {
					if ((fileName != null) && (fileName.Length > 0) && SecurityManager.SecurityEnabled) {
						new FileIOPermission (FileIOPermissionAccess.PathDiscovery, fileName).Demand ();
					}
					return fileName;
				}
			}

			public int LineNumber
			{
				get {
					return lineNumber;
				}
			}
		}
	
		class ConfigXmlElement : XmlElement, IConfigXmlNode
#if CONFIGURATION_DEP
			, IConfigErrorInfo
#endif
		{
			string fileName;
			int lineNumber;

			public ConfigXmlElement (ConfigXmlDocument document,
						 string prefix,
						 string localName,
						 string namespaceUri)
				: base (prefix, localName, namespaceUri, document)
			{
				fileName = document.fileName;
				lineNumber = document.LineNumber;
			}

			public string Filename
			{
				get {
					if ((fileName != null) && (fileName.Length > 0) && SecurityManager.SecurityEnabled) {
						new FileIOPermission (FileIOPermissionAccess.PathDiscovery, fileName).Demand ();
					}
					return fileName;
				}
			}

			public int LineNumber
			{
				get {
					return lineNumber;
				}
			}
		}

		class ConfigXmlText : XmlText, IConfigXmlNode
#if CONFIGURATION_DEP
			, IConfigErrorInfo
#endif
		{
			string fileName;
			int lineNumber;

			public ConfigXmlText (ConfigXmlDocument document, string data)
				: base (data, document)
			{
				fileName = document.fileName;
				lineNumber = document.LineNumber;
			}

			public string Filename
			{
				get {
					if ((fileName != null) && (fileName.Length > 0) && SecurityManager.SecurityEnabled) {
						new FileIOPermission (FileIOPermissionAccess.PathDiscovery, fileName).Demand ();
					}
					return fileName;
				}
			}

			public int LineNumber
			{
				get {
					return lineNumber;
				}
			}
		}
	}
}

#endif
