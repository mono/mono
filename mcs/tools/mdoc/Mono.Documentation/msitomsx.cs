//
// msitomsx.cs: Microsoft Internal XML to Microsoft XML Documentation
//
// Arguably this doesn't belong in mdoc, but I'd rather not do some
// stand-alone tool either, especially since the primary reason it exists is
// to facilitate generating ECMA documentation via mdoc-update and
// mdoc-update-ecma-xml...
//
// Author:
//   Jonathan Pryor  <jpryor@novell.com>
//
// Copyright (c) 2010 Novell, Inc. (http://www.novell.com)
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;

using Mono.Options;

namespace Mono.Documentation {

	class MsidocToMsxdocConverter : MDocCommand {

		XslCompiledTransform msiToMsxTransform = new XslCompiledTransform ();

		public MsidocToMsxdocConverter ()
		{
			using (var r = XmlReader.Create (
						Assembly.GetExecutingAssembly ().GetManifestResourceStream ("msitomsx.xsl")))
				msiToMsxTransform.Load (r);
		}

		public override void Run (IEnumerable<string> args)
		{
			string current_library = "";

			var types = new List<string> ();
			string outdir = null;

			var options = new OptionSet () {
				{ "o|out=", 
					"{DIRECTORY} to create Microsoft XML assembly.xml documentation files.",
					v => outdir = v },
				{ "library=",
					"Ignored for compatibility with update-ecma-xml.",
					v => {} },
				{ "type=",
					"The full {TYPE} name of a type to copy into the output file.",
					v => types.Add (v) },
			};
			var sources = Parse (options, args, "export-ecma-xml", 
					"[OPTIONS]+ DIRECTORIES",
					"Convert Microsoft internal XML documentation within DIRECTORIES " +
					"into Microsoft XML documentation.\n\n");
			if (sources == null)
				return;
			if (sources.Count == 0)
				Error ("No directories specified.");
			if (outdir == null)
				Error ("No output directory specified.  Please use --out=DIRECTORY.");

			types.Sort ();

			Dictionary<string, XDocument> docs = Convert (sources, types);
			foreach (KeyValuePair<string, XDocument> e in docs) {
				using (var o = CreateWriter (Path.Combine (outdir, e.Key + ".xml")))
					e.Value.WriteTo (o);
			}
		}

		private Dictionary<string, XDocument> Convert (List<string> sources, List<string> types)
		{
			var docs = new Dictionary<string, XDocument> ();

			foreach (var source in sources) {
				foreach (var dir in Directory.GetDirectories (source)) {
					foreach (var file in Directory.GetFiles (dir, "*.xml")) {
						ConvertDocs (docs, types, file);
					}
				}
			}

			return docs;
		}

		private void ConvertDocs (Dictionary<string, XDocument> docs, List<string> types, string file)
		{
			var doc = LoadFile (file);
			var type = doc.Root.Element ("members").Element ("member").Attribute ("name").Value;

			if (type.StartsWith ("N:"))
				return;

			if (!type.StartsWith ("T:"))
				throw new InvalidOperationException ("File '" + file + "' doesn't contain type documentation, it contains docs for: " + type);

			type = type.Substring (2);
			if (types.Count > 0 && types.BinarySearch (type) < 0)
				return;

			var assembly = doc.Root.Element ("assembly").Element ("name").Value;
			XDocument asmdocs;
			if (!docs.TryGetValue (assembly, out asmdocs)) {
				docs.Add (assembly, 
						asmdocs = new XDocument (
							new XElement ("doc", 
								new XElement ("members"))));
			}

			var import = new XDocument ();
			msiToMsxTransform.Transform (doc.CreateReader (), import.CreateWriter ());

			asmdocs.Root.Element ("members").Add (import.Root.Element ("members").Elements ("member"));
		}

		static XDocument LoadFile (string file)
		{
			using (XmlReader r = XmlReader.Create (file))
				return XDocument.Load (r);
		}

		static XmlWriter CreateWriter (string file)
		{
			var settings = new XmlWriterSettings {
				Encoding            = new UTF8Encoding (false),
				Indent              = true,
				IndentChars         = "    ",
				NewLineChars        = "\r\n",
				OmitXmlDeclaration  = true,
			};

			return XmlWriter.Create (file, settings);
		}
	}
}

