//
// TraceFilter.cs: Internal generic (Input/Output) trace filter
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.IO;
using System.Reflection;
using System.Xml;

namespace Microsoft.Web.Services.Diagnostics {

	internal class TraceFilter {

		public static string GetCompleteFilename (string filename) 
		{
			return AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + filename;
		}

		public static void WriteEnvelope (string filename, SoapEnvelope envelope) 
		{
			if (envelope == null)
				throw new ArgumentNullException ("envelope");

			FileStream fs = null;
			XmlDocument log = null;
			try {
				// load and lock xml document
				if (!File.Exists (filename)) {
					fs = File.Open (filename, FileMode.Create, FileAccess.Write, FileShare.Read);
					log = new XmlDocument ();
					log.LoadXml (String.Format ("<?xml version=\"1.0\" encoding=\"utf-8\"?>{0}<log/>", Environment.NewLine));
				}
				else {
					fs = File.Open (filename, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
					log = new XmlDocument ();
					log.Load (fs);
				}
				// add new entry
				XmlNode xn = log.ImportNode (envelope.DocumentElement, true);
				log.DocumentElement.AppendChild (xn);
				fs.Position = 0;
				log.Save (fs);
			}
			finally {
				fs.Close ();
			}
		}
	}
}
