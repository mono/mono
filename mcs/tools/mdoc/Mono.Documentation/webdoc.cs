//
// webdoc.cs
//
// Author:
//   Jonathan Pryor  <jpryor@novell.com>
//
// Copyright (c) 2009 Novell, Inc. (http://www.novell.com)
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

using Monodoc;
using Mono.Documentation;

using Mono.Options;
using Mono.Rocks;

using ICSharpCode.SharpZipLib.Zip;

namespace Mono.Documentation
{
	public class MDocExportWebdocHtml : MDocCommand
	{
		public override void Run (IEnumerable<string> args)
		{
			string dir = null;
			bool forceUpdate = false;
			var options = new OptionSet () {
				{ "force-update",
					"Always generate new files.  If not specified, will only generate " +
					"files if the write time of the output directory is older than the " +
					"write time of the source .tree/.zip files.",
					v => forceUpdate = v != null },
				{ "o|out=",
					"The {DIRECTORY} to place the generated files and directories.\n\n" +
					"If not specified, defaults to\n`dirname FILE`/cache/`basename FILE .tree`.",
					v => dir = v },
			};
			List<string> files = Parse (options, args, "export-html-webdoc", 
					"[OPTIONS]+ FILES",
					"Export mdoc documentation within FILES to HTML for use by ASP.NET webdoc.\n\n" +
					"FILES are .tree or .zip files as produced by 'mdoc assemble'.");
			if (files == null)
				return;
			if (files.Count == 0)
				Error ("No files specified.");
			HelpSource.use_css = true;
			HelpSource.FullHtml = false;
			SettingsHandler.Settings.EnableEditing = false;
			foreach (var basePath in 
					files.Select (f => 
							Path.Combine (Path.GetDirectoryName (f), Path.GetFileNameWithoutExtension (f)))
					.Distinct ()) {
				string treeFile = basePath + ".tree";
				string zipFile  = basePath + ".zip";
				if (!Exists (treeFile) || !Exists (zipFile))
					continue;
				string outDir = dir ?? XmlDocUtils.GetCacheDirectory (basePath);
				if (!forceUpdate && Directory.Exists (outDir) &&
							MaxWriteTime (treeFile, zipFile) < Directory.GetLastWriteTime (outDir))
					continue;
				Message (TraceLevel.Warning, "Processing files: {0}, {1}", treeFile, zipFile);
				Directory.CreateDirectory (outDir);
				ExtractZipFile (zipFile, outDir);
				GenerateCache (basePath, treeFile, outDir);
			}
		}

		bool Exists (string file)
		{
			if (!File.Exists (file)) {
					Message (TraceLevel.Error,
							"mdoc: Could not find file: {0}", file);
					return false;
			}
			return true;
		}

		DateTime MaxWriteTime (params string[] files)
		{
			return files.Select (f => File.GetLastWriteTime (f)).Max ();
		}

		void ExtractZipFile (string zipFile, string outDir)
		{
			ZipInputStream zip = new ZipInputStream (File.OpenRead (zipFile));

			ZipEntry entry;
			while ((entry = zip.GetNextEntry ()) != null) {
				string file = Path.Combine (outDir, entry.Name);
				Directory.CreateDirectory (Path.GetDirectoryName (file));
				using (var output = File.OpenWrite (file))
					zip.WriteTo (output);
			}
		}

		void GenerateCache (string basePath, string treeFile, string outDir)
		{
			Tree tree = new Tree (null, treeFile);
			RootTree docRoot = RootTree.LoadTree ();
			string helpSourceName = Path.GetFileName (basePath);
			HelpSource hs = docRoot.HelpSources.Cast<HelpSource> ()
				.FirstOrDefault (h => h.Name == helpSourceName);
			if (hs == null) {
				throw new Exception ("Only installed .tree and .zip files are supported.");
			}
			foreach (Node node in tree.TraverseDepthFirst<Node, Node> (t => t, t => t.Nodes.Cast<Node> ())) {
				var url = node.URL;
				Message (TraceLevel.Info, "\tProcessing URL: {0}", url);
				if (string.IsNullOrEmpty (url))
					continue;
				var file = XmlDocUtils.GetCachedFileName (outDir, url);
				using (var o = File.AppendText (file)) {
					Node _;
					// Sometimes the HelpSource won't directly support a url.
					// Case in point: the Tree will contain N:Enter.Namespace.Here nodes
					// which aren't supported by HelpSource.GetText.
					// If this happens, docRoot.RenderUrl() works.
					// (And no, we can't always use docRoot.RenderUrl() for URLs like
					// "ecma:0#Foo/", as that'll just grab the 0th stream contents from
					// the first EcmaHelpSource found...
					string contents = hs.GetText (url, out _) ?? docRoot.RenderUrl (url, out _);
					o.Write (contents);
				}
			}
		}
	}
}
