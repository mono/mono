//
// The assembler: Help compiler.
//
// Author:
//   Miguel de Icaza (miguel@gnome.org)
//
// (C) 2003 Ximian, Inc.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Monodoc;
using Monodoc.Providers;
using Mono.Options;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;
using Monodoc.Ecma;

namespace Mono.Documentation {

public class MDocAssembler : MDocCommand {
	static readonly string[] ValidFormats = {
		"ecma", 
		"ecmaspec", 
		"error", 
		"hb", 
		"man", 
		"simple", 
		"xhtml"
	};

	string droppedNamespace = null;

	public static Option[] CreateFormatOptions (MDocCommand self, Dictionary<string, List<string>> formats)
	{
		string cur_format = "ecma";
		var options = new OptionSet () {
			{ "f|format=",
				"The documentation {FORMAT} used in DIRECTORIES.  " + 
					"Valid formats include:\n  " +
					string.Join ("\n  ", ValidFormats) + "\n" +
					"If not specified, the default format is `ecma'.",
				v => {
					if (Array.IndexOf (ValidFormats, v) < 0)
						self.Error ("Invalid documentation format: {0}.", v);
					cur_format = v;
				} },
			{ "<>", v => AddFormat (self, formats, cur_format, v) },
		};
		return new Option[]{options[0], options[1]};
	}

	public override void Run (IEnumerable<string> args)
	{
		bool replaceNTypes = false;
		var formats = new Dictionary<string, List<string>> ();
		string prefix = "tree";
		var formatOptions = CreateFormatOptions (this, formats);
		var options = new OptionSet () {
			formatOptions [0],
			{ "o|out=",
				"Provides the output file prefix; the files {PREFIX}.zip and " + 
					"{PREFIX}.tree will be created.\n" +
					"If not specified, `tree' is the default PREFIX.",
				v => prefix = v },
			formatOptions [1],
			{"dropns=","The namespace that has been dropped from this version of the assembly.", v => droppedNamespace = v },
			{"ntypes","Replace references to native types with their original types.", v => replaceNTypes=true },
		};
		List<string> extra = Parse (options, args, "assemble", 
				"[OPTIONS]+ DIRECTORIES",
				"Assemble documentation within DIRECTORIES for use within the monodoc browser.");
		if (extra == null)
			return;

		List<Provider> list = new List<Provider> ();
		EcmaProvider ecma = null;
		bool sort = false;
		
		foreach (string format in formats.Keys) {
			switch (format) {
			case "ecma":
				if (ecma == null) {
					ecma = new EcmaProvider ();
					list.Add (ecma);
					sort = true;
				}
				ecma.FileSource = new MDocFileSource(droppedNamespace, string.IsNullOrWhiteSpace(droppedNamespace) ? ApiStyle.Unified : ApiStyle.Classic) {
					ReplaceNativeTypes = replaceNTypes
				};
				foreach (string dir in formats [format])
					ecma.AddDirectory (dir);
				break;

			case "xhtml":
			case "hb":
				list.AddRange (formats [format].Select (d => (Provider) new XhtmlProvider (d)));
				break;

			case "man":
				list.Add (new ManProvider (formats [format].ToArray ()));
				break;

			case "error":
				list.AddRange (formats [format].Select (d => (Provider) new ErrorProvider (d)));
				break;

			case "ecmaspec":
				list.AddRange (formats [format].Select (d => (Provider) new EcmaSpecProvider (d)));
				break;

			case "addins":
				list.AddRange (formats [format].Select (d => (Provider) new AddinsProvider (d)));
				break;
			}
		}

		HelpSource hs = new HelpSource (prefix, true);
		hs.TraceLevel = TraceLevel;

		foreach (Provider p in list) {
			p.PopulateTree (hs.Tree);
		}

		if (sort && hs.Tree != null)
			hs.Tree.RootNode.Sort ();
			      
		//
		// Flushes the EcmaProvider
		//
		foreach (Provider p in list)
			p.CloseTree (hs, hs.Tree);

		hs.Save ();
	}

	private static void AddFormat (MDocCommand self, Dictionary<string, List<string>> d, string format, string file)
	{
		if (format == null)
			self.Error ("No format specified.");
		List<string> l;
		if (!d.TryGetValue (format, out l)) {
			l = new List<string> ();
			d.Add (format, l);
		}
		l.Add (file);
	}
}

	/// <summary>
	/// A custom provider file source that lets us modify the source files before they are processed by monodoc.
	/// </summary>
	internal class MDocFileSource : IEcmaProviderFileSource {
		private string droppedNamespace;
		private bool shouldDropNamespace = false;
		private ApiStyle styleToDrop;

		public bool ReplaceNativeTypes { get; set; }

		/// <param name="ns">The namespace that is being dropped.</param>
		/// <param name="style">The style that is being dropped.</param>
		public MDocFileSource(string ns, ApiStyle style) 
		{
			droppedNamespace = ns;
			shouldDropNamespace = !string.IsNullOrWhiteSpace (ns);
			styleToDrop = style;
		}

		public XmlReader GetIndexReader(string path) 
		{
			XDocument doc = XDocument.Load (path);

			DropApiStyle (doc, path);
			DropNSFromDocument (doc);

			// now put the modified contents into a stream for the XmlReader that monodoc will use.
			MemoryStream io = new MemoryStream ();
			using (var writer = XmlWriter.Create (io)) {
				doc.WriteTo (writer);
			}
			io.Seek (0, SeekOrigin.Begin);

			return XmlReader.Create (io);
		}

		public XElement GetNamespaceElement(string path) 
		{
			var element = XElement.Load (path);

			var attributes = element.Descendants ().Concat(new XElement[] { element }).SelectMany (n => n.Attributes ());
			var textNodes = element.Nodes ().OfType<XText> ();

			DropNS (attributes, textNodes);

			return element;
		}

		void DropApiStyle(XDocument doc, string path) 
		{
			string styleString = styleToDrop.ToString ().ToLower ();
			var items = doc
				.Descendants ()
				.Where (n => n.Attributes ()
					.Any (a => a.Name.LocalName == "apistyle" && a.Value == styleString))
				.ToArray ();

			foreach (var element in items) {
				element.Remove ();
			}

			if (styleToDrop == ApiStyle.Classic && ReplaceNativeTypes) {
				RewriteCrefsIfNecessary (doc, path);
			}
		}

		void RewriteCrefsIfNecessary (XDocument doc, string path)
		{
			// we also have to rewrite crefs
			var sees = doc.Descendants ().Where (d => d.Name.LocalName == "see").ToArray ();
			foreach (var see in sees) {
				var cref = see.Attribute ("cref");
				if (cref == null) {
					continue;
				}
				EcmaUrlParser parser = new EcmaUrlParser ();
				EcmaDesc reference;
				if (!parser.TryParse (cref.Value, out reference)) {
					continue;
				}
				if ((new EcmaDesc.Kind[] {
					EcmaDesc.Kind.Constructor,
					EcmaDesc.Kind.Method
				}).Any (k => k == reference.DescKind)) {
					string ns = reference.Namespace;
					string type = reference.TypeName;
					string memberName = reference.MemberName;
					if (reference.MemberArguments != null) {
						XDocument refDoc = FindReferenceDoc (path, doc, ns, type);
						if (refDoc == null) {
							continue;
						}
						// look in the refDoc for the memberName, and match on parameters and # of type parameters
						var overloads = refDoc.XPathSelectElements ("//Member[@MemberName='" + memberName + "']").ToArray ();
						// Do some initial filtering to find members that could potentially match (based on parameter and typeparam counts)
						var members = overloads.Where (e => reference.MemberArgumentsCount == e.XPathSelectElements ("Parameters/Parameter[not(@apistyle) or @apistyle='classic']").Count () && reference.GenericMemberArgumentsCount == e.XPathSelectElements ("TypeParameters/TypeParameter[not(@apistyle) or @apistyle='classic']").Count ()).Select (m => new {
							Node = m,
							AllParameters = m.XPathSelectElements ("Parameters/Parameter").ToArray (),
							Parameters = m.XPathSelectElements ("Parameters/Parameter[not(@apistyle) or @apistyle='classic']").ToArray (),
							NewParameters = m.XPathSelectElements ("Parameters/Parameter[@apistyle='unified']").ToArray ()
						}).ToArray ();
						// now find the member that matches on types
						var member = members.FirstOrDefault (m => reference.MemberArguments.All (r => m.Parameters.Any (mp => mp.Attribute ("Type").Value.Contains (r.TypeName))));
						if (member == null || member.NewParameters.Length == 0)
							continue;
						foreach (var arg in reference.MemberArguments) {
							// find the "classic" parameter
							var oldParam = member.Parameters.First (p => p.Attribute ("Type").Value.Contains (arg.TypeName));
							var newParam = member.NewParameters.FirstOrDefault (p => oldParam.Attribute ("Name").Value == p.Attribute ("Name").Value);
							if (newParam != null) {
								// this means there was a change made, and we should try to convert this cref
								arg.TypeName = NativeTypeManager.ConvertToNativeType (arg.TypeName);
							}
						}
						var rewrittenReference = reference.ToEcmaCref ();
						Console.WriteLine ("From {0} to {1}", cref.Value, rewrittenReference);
						cref.Value = rewrittenReference;
					}
				}
			}
		}

		XDocument FindReferenceDoc (string currentPath, XDocument currentDoc, string ns, string type)
		{
			if (currentPath.Length <= 1) {
				return null;
			}
			// build up the supposed path to the doc
			string dir = Path.GetDirectoryName (currentPath);
			if (dir.Equals (currentPath)) {
				return null;
			}

			string supposedPath = Path.Combine (dir, ns, type + ".xml");

			// if it's the current path, return currentDoc
			if (supposedPath == currentPath) {
				return currentDoc;
			}

			if (!File.Exists (supposedPath)) {
				// hmm, file not there, look one directory up
				return FindReferenceDoc (dir, currentDoc, ns, type);
			}

			// otherwise, load the XDoc and return
			return XDocument.Load (supposedPath);
		}

		void DropNSFromDocument (XDocument doc)
		{
			var attributes = doc.Descendants ().SelectMany (n => n.Attributes ());
			var textNodes = doc.DescendantNodes().OfType<XText> ().ToArray();

			DropNS (attributes, textNodes);
		}

		void DropNS(IEnumerable<XAttribute> attributes, IEnumerable<XText> textNodes) 
		{
			if (!shouldDropNamespace) {
				return;
			}

			string nsString = string.Format ("{0}.", droppedNamespace);
			foreach (var attr in attributes) {
				if (attr.Value.Contains (nsString)) {
					attr.Value = attr.Value.Replace (nsString, string.Empty);
				}
			}

			foreach (var textNode in textNodes) {
				if (textNode.Value.Contains (nsString)) {
 					textNode.Value = textNode.Value.Replace (nsString, string.Empty);
				}
			}
		}
			

		/// <param name="nsName">This is the type's name in the processed XML content. 
		/// If dropping the namespace, we'll need to append it so that it's found in the source.</param>
		/// <param name="typeName">Type name.</param>
		public string GetTypeXmlPath(string basePath, string nsName, string typeName) 
		{
			string nsNameToUse = nsName;
			if (shouldDropNamespace) {
				nsNameToUse = string.Format ("{0}.{1}", droppedNamespace, nsName);

				var droppedPath = BuildTypeXmlPath (basePath, typeName, nsNameToUse);
				var origPath = BuildTypeXmlPath (basePath, typeName, nsName);

				if (!File.Exists (droppedPath)) {
					if (File.Exists (origPath)) {
						return origPath;
					}
				}

				return droppedPath;
			} else {

				var finalPath = BuildTypeXmlPath (basePath, typeName, nsNameToUse);

				return finalPath;
			}
		}

		static string BuildTypeXmlPath (string basePath, string typeName, string nsNameToUse)
		{
			string finalPath = Path.Combine (basePath, nsNameToUse, Path.ChangeExtension (typeName, ".xml"));
			return finalPath;
		}

		static string BuildNamespaceXmlPath (string basePath, string ns)
		{
			var nsFileName = Path.Combine (basePath, String.Format ("ns-{0}.xml", ns));
			return nsFileName;
		}

		/// <returns>The namespace for path, with the dropped namespace so it can be used to pick the right file if we're dropping it.</returns>
		/// <param name="ns">This namespace will already have "dropped" the namespace.</param>
		public string GetNamespaceXmlPath(string basePath, string ns) 
		{
			string nsNameToUse = ns;
			if (shouldDropNamespace) {
				nsNameToUse = string.Format ("{0}.{1}", droppedNamespace, ns);

				var droppedPath = BuildNamespaceXmlPath (basePath, nsNameToUse);
				var origPath = BuildNamespaceXmlPath (basePath, ns);

				if (!File.Exists (droppedPath) && File.Exists(origPath)) {
					return origPath;
				}

				return droppedPath;
			} else {
				var path = BuildNamespaceXmlPath (basePath, ns); 
				return path;
			}
		}

		public XDocument GetTypeDocument(string path) 
		{
			var doc = XDocument.Load (path);
			DropApiStyle (doc, path);
			DropNSFromDocument (doc);

			return doc;
		}

		public XElement ExtractNamespaceSummary (string path)
		{
			using (var reader = GetIndexReader (path)) {
				reader.ReadToFollowing ("Namespace");
				var name = reader.GetAttribute ("Name");
				var summary = reader.ReadToFollowing ("summary") ? XElement.Load (reader.ReadSubtree ()) : new XElement ("summary");
				var remarks = reader.ReadToFollowing ("remarks") ? XElement.Load (reader.ReadSubtree ()) : new XElement ("remarks");

				return new XElement ("namespace",
					new XAttribute ("ns", name ?? string.Empty),
					summary,
					remarks);
			}
		}
	}
}
