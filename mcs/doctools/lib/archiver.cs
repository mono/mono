// archiver.cs - Mono Documentation Lib
//
// Author: Adam Treat <manyoso@yahoo.com>
// (c) 2002 Adam Treat
// Licensed under the terms of the GNU GPL

using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Collections;

namespace Mono.Document.Library {

	public class DocArchiver {

		public static void Archive (DocType document)
		{
			if (!Directory.Exists (document.FileNamespace))
				if (!Directory.Exists (document.FileLanguage))
					Directory.CreateDirectory (document.FileLanguage);
				else
					Directory.CreateDirectory (document.FileNamespace);

			XmlTextWriter writer = new XmlTextWriter (document.FilePath, new UTF8Encoding());
			writer.Formatting = Formatting.Indented;
			writer.Indentation = 4;
			writer.WriteStartDocument ();
				writer.WriteStartElement ("monodoc");
				writer.WriteAttributeString ("language", document.Language);
					writer.WriteStartElement (document.Type);
					writer.WriteAttributeString ("name", document.Name);
					writer.WriteAttributeString ("namespace", document.Namespace);
						writer.WriteStartElement ("summary");
							writer.WriteString (document.Summary);
						writer.WriteEndElement();
						writer.WriteStartElement ("remarks");
							writer.WriteString (document.Remarks);
						writer.WriteEndElement();
					writer.WriteEndElement();
				writer.WriteEndElement();
			writer.WriteEndDocument ();
			writer.Close();
		}
	}
}
