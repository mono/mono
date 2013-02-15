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
using System.Xml.Linq;

using Monodoc;
using Monodoc.Generators;
using Monodoc.Caches;
using Mono.Documentation;

using Mono.Options;
using Mono.Rocks;

using ICSharpCode.SharpZipLib.Zip;

namespace Mono.Documentation
{
	public class MDocExportWebdocHtml : MDocCommand
	{
		class Options {
			public Dictionary<string, List<string>> Formats = new Dictionary<string, List<string>>();
			public List<string> Sources = new List<string>();
			public bool UseSystemSources = true;
			public bool ForceUpdate = false;
			public string OutputDirectory = null;
		}

		public override void Run (IEnumerable<string> args)
		{
			var opts = new Options ();
			var formatOptions = MDocAssembler.CreateFormatOptions (this, opts.Formats);
			var options = new OptionSet () {
				{ "force-update",
					"Always generate new files.  If not specified, will only generate " +
					"files if the write time of the output directory is older than the " +
					"write time of the source .tree/.zip files.",
					v => opts.ForceUpdate = v != null },
				formatOptions [0],
				formatOptions [1],
				{ "o|out=",
					"The {PREFIX} to place the generated files and directories.  " +
					"Default: \"`dirname FILE`/cache/\".\n" +
					"Underneath {PREFIX}, `basename FILE .tree` directories will be " +
					"created which will contain the pre-generated HTML content.",
					v => opts.OutputDirectory = v },
				{ "r=",
					"A {SOURCE} file to use for reference purposes.\n" +
					"Extension methods are searched for among all {SOURCE}s which are referenced.\n" +
					"This option may be specified multiple times.",
					v => opts.Sources.Add (v) },
				{ "use-system-sources",
					"Use the system-wide .source files for reference purposes. " +
					"Default is " + (opts.UseSystemSources ? "enabled" : "disabled") + ".",
					v => opts.UseSystemSources = v != null },
			};
			Parse (options, args, "export-html-webdoc",
					"[OPTIONS]+ FILES",
					"Export mdoc documentation within FILES to HTML for use by ASP.NET webdoc.\n\n" +
					"FILES are .tree or .zip files as produced by 'mdoc assemble', or .source files\n" +
					"which reference .tree and .zip files produced by 'mdoc assemble'.\n\n" +
					"See mdoc(5) or mdoc-assemble(1) for information about the .source file format.");
			if (opts.Formats.Values.All (files => files.Count == 0))
				Error ("No files specified.");
			ProcessSources (opts);
			foreach (var p in opts.Formats)
				ProcessFiles (opts, p.Key, p.Value);
		}

		void ProcessSources (Options opts)
		{
			foreach (var p in opts.Formats) {
				var files = p.Value;
				foreach (var f in files.Where (f => f.EndsWith (".source")).ToList ()) {
					files.Remove (f);
					foreach (var tfi in GetTreeFilesFromSource (f)) {
						List<string> treeFiles;
						if (!opts.Formats.TryGetValue (tfi.Key, out treeFiles))
							opts.Formats.Add (tfi.Key, treeFiles = new List<string> ());
						treeFiles.Add (tfi.Value);
					}
				}
			}
		}

		IEnumerable<KeyValuePair<string, string>> GetTreeFilesFromSource (string sourceFile)
		{
			try {
				var source = XElement.Load (sourceFile);
				return source.Descendants ("source")
					.Select (e => new KeyValuePair<string, string>(e.Attribute ("provider").Value,
								Path.Combine (Path.GetDirectoryName (sourceFile), e.Attribute ("basefile").Value + ".tree")));
			}
			catch (Exception e) {
				Message (TraceLevel.Error, "mdoc: error parsing file {0}: {1}", sourceFile, e.Message);
				return new KeyValuePair<string, string>[0];
			}
		}

		void ProcessFiles (Options opts, string format, List<string> files)
		{
			foreach (var basePath in
					files.Select (f =>
							Path.Combine (Path.GetDirectoryName (f), Path.GetFileNameWithoutExtension (f)))
					.Distinct ()) {
				string treeFile = basePath + ".tree";
				string zipFile  = basePath + ".zip";
				if (!Exists (treeFile) || !Exists (zipFile))
					continue;
				string outDir = opts.OutputDirectory != null
					? Path.Combine (opts.OutputDirectory, Path.GetFileName (basePath))
					: XmlDocUtils.GetCacheDirectory (basePath);
				if (!opts.ForceUpdate && Directory.Exists (outDir) &&
							MaxWriteTime (treeFile, zipFile) < Directory.GetLastWriteTime (outDir))
					continue;
				Message (TraceLevel.Warning, "Processing files: {0}, {1}", treeFile, zipFile);
				Directory.CreateDirectory (outDir);
				ExtractZipFile (zipFile, outDir);
				GenerateCache (opts, basePath, format, outDir);
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

		void GenerateCache (Options opts, string basePath, string format, string outDir)
		{
			var hs = RootTree.GetHelpSource (format, basePath);
			if (hs == null) {
				Error ("Unable to find a HelpSource for provider '{0}' and file '{1}.tree'.", format, basePath);
			}
			var tree = hs.Tree;
			RootTree docRoot = null;
			if (!opts.UseSystemSources)
				docRoot = RootTree.LoadTree (null, null, opts.Sources);
			else {
				docRoot = RootTree.LoadTree ();
				foreach (var source in opts.Sources)
					docRoot.AddSourceFile (source);
			}
			hs.RootTree = docRoot;
			var generator = new HtmlGenerator (new NullCache ());
			foreach (Node node in tree.RootNode.TraverseDepthFirst<Node, Node> (t => t, t => t.ChildNodes)) {
				var url = node.PublicUrl;
				Message (TraceLevel.Info, "\tProcessing URL: {0}", url);
				if (string.IsNullOrEmpty (url))
					continue;
				var file = XmlDocUtils.GetCachedFileName (outDir, url);
				using (var o = File.AppendText (file)) {
					Node _;
					string contents = docRoot.RenderUrl (url, generator, out _);
					o.Write (contents);
				}
			}
		}
	}
}
