//
// System.Configuration.ConfigXmlDocument
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

using System;
using System.IO;
using System.Xml;

namespace System.Configuration
{
	public sealed class ConfigXmlDocument : XmlDocument, IConfigXmlNode
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
			LoadSingleElement (filename, new XmlTextReader (filename));
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
				return fileName;
			}
		}

		public int LineNumber
		{
			get {
				return lineNumber;
			}
		}

		//
		// Wrappers for Xml* that just provide file name and line number addition
		//
		class ConfigXmlAttribute : XmlAttribute, IConfigXmlNode
		{
			string fileName;
			int lineNumber;

			public ConfigXmlAttribute (ConfigXmlDocument document,
						   string prefix,
						   string localName,
						   string namespaceUri)
				: base (prefix, localName, namespaceUri, document)
			{
				fileName = document.Filename;
				lineNumber = document.LineNumber;
			}

			public string Filename
			{
				get {
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
		{
			string fileName;
			int lineNumber;

			public ConfigXmlCDataSection (ConfigXmlDocument document, string data)
				: base (data, document)
			{
				fileName = document.Filename;
				lineNumber = document.LineNumber;
			}

			public string Filename
			{
				get {
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
				fileName = document.Filename;
				lineNumber = document.LineNumber;
			}

			public string Filename
			{
				get {
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
		{
			string fileName;
			int lineNumber;

			public ConfigXmlElement (ConfigXmlDocument document,
						 string prefix,
						 string localName,
						 string namespaceUri)
				: base (prefix, localName, namespaceUri, document)
			{
				fileName = document.Filename;
				lineNumber = document.LineNumber;
			}

			public string Filename
			{
				get {
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
		{
			string fileName;
			int lineNumber;

			public ConfigXmlText (ConfigXmlDocument document, string data)
				: base (data, document)
			{
				fileName = document.Filename;
				lineNumber = document.LineNumber;
			}

			public string Filename
			{
				get {
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

