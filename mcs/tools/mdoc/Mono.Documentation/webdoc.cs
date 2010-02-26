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
			var formats = new Dictionary<string, List<string>> ();
			var formatOptions = MDocAssembler.CreateFormatOptions (this, formats);
			var sources = new List<string>();
			var options = new OptionSet () {
				{ "force-update",
					"Always generate new files.  If not specified, will only generate " +
					"files if the write time of the output directory is older than the " +
					"write time of the source .tree/.zip files.",
					v => forceUpdate = v != null },
				formatOptions [0],
				{ "o|out=",
					"The {PREFIX} to place the generated files and directories.  " + 
					"Default: \"`dirname FILE`/cache/\".\n" +
					"Underneath {PREFIX}, `basename FILE .tree` directories will be " + 
					"created which will contain the pre-generated HTML content.",
					v => dir = v },
				{ "r=",
					"A {SOURCE} file to use for reference purposes.\n" +
					"Extension methods are searched for among all {SOURCE}s which are referenced.\n" +
					"This option may be specified multiple times.",
					v => sources.Add (v) },
				formatOptions [1],
			};
			Parse (options, args, "export-html-webdoc", 
					"[OPTIONS]+ FILES",
					"Export mdoc documentation within FILES to HTML for use by ASP.NET webdoc.\n\n" +
					"FILES are .tree or .zip files as produced by 'mdoc assemble'.");
			if (formats.Values.All (files => files.Count == 0))
				Error ("No files specified.");
			HelpSource.use_css = true;
			HelpSource.FullHtml = false;
			SettingsHandler.Settings.EnableEditing = false;
			foreach (var p in formats)
				ProcessFiles (dir, forceUpdate, sources, p.Key, p.Value);
		}

		void ProcessFiles (string dir, bool forceUpdate, List<string> sources, string format, List<string> files)
		{
			foreach (var basePath in 
					files.Select (f => 
							Path.Combine (Path.GetDirectoryName (f), Path.GetFileNameWithoutExtension (f)))
					.Distinct ()) {
				string treeFile = basePath + ".tree";
				string zipFile  = basePath + ".zip";
				if (!Exists (treeFile) || !Exists (zipFile))
					continue;
				string outDir = dir != null 
					? Path.Combine (dir, Path.GetFileName (basePath))
					: XmlDocUtils.GetCacheDirectory (basePath);
				if (!forceUpdate && Directory.Exists (outDir) &&
							MaxWriteTime (treeFile, zipFile) < Directory.GetLastWriteTime (outDir))
					continue;
				Message (TraceLevel.Warning, "Processing files: {0}, {1}", treeFile, zipFile);
				Directory.CreateDirectory (outDir);
				ExtractZipFile (zipFile, outDir);
				GenerateCache (basePath, format, outDir, sources);
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

		void GenerateCache (string basePath, string format, string outDir, IEnumerable<string> sources)
		{
			var hs = RootTree.GetHelpSource (format, basePath);
			if (hs == null) {
				Error ("Unable to find a HelpSource for provider '{0}' and file '{1}.tree'.", format, basePath);
			}
			var tree = hs.Tree;
			RootTree docRoot = RootTree.LoadTree (null, null, sources);
			hs.RootTree = docRoot;
			string helpSourceName = Path.GetFileName (basePath);
			foreach (Node node in tree.TraverseDepthFirst<Node, Node> (t => t, t => t.Nodes.Cast<Node> ())) {
				var url = node.URL;
				Message (TraceLevel.Info, "\tProcessing URL: {0}", url);
				if (string.IsNullOrEmpty (url))
					continue;
				var file = XmlDocUtils.GetCachedFileName (outDir, url);
				using (var o = File.AppendText (file)) {
					Node _;
					string contents = hs.GetText (url, out _) ?? hs.RenderNamespaceLookup (url, out _);
					o.Write (contents);
				}
			}
		}
	}
}
