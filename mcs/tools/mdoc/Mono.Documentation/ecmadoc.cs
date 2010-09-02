//
// ecmadoc.cs
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
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

using Monodoc;
using Mono.Documentation;

using Mono.Options;
using Mono.Rocks;

namespace Mono.Documentation
{
	public class MDocUpdateEcmaXml : MDocCommand
	{
		string file = "CLILibraryTypes.xml";
		List<string> directories;
		Dictionary<string, HashSet<string>> libraries = new Dictionary<string, HashSet<string>>();

		public override void Run (IEnumerable<string> args)
		{
			string current_library = "";

			var options = new OptionSet () {
				{ "o|out=", 
					"{FILE} to generate/update documentation within.\n" + 
					"If not specified, will process " + file + ".\n" +
					"Set to '-' to skip updates and write to standard output.",
					v => file = v },
				{ "library=",
					"The {LIBRARY} that the following --type=TYPE types should be a part of.",
					v => current_library = v },
				{ "type=",
					"The full {TYPE} name of a type to copy into the output file.",
					v => AddTypeToLibrary (current_library, v) },
			};
			directories = Parse (options, args, "export-ecma-xml", 
					"[OPTIONS]+ DIRECTORIES",
					"Export mdoc documentation within DIRECTORIES into ECMA-format XML.\n\n" +
					"DIRECTORIES are mdoc(5) directories as produced by 'mdoc update'.");
			if (directories == null || directories.Count == 0)
				return;

			Update ();
		}

		void AddTypeToLibrary (string library, string type)
		{
			HashSet<string> types;
			if (!libraries.TryGetValue (library, out types))
				libraries.Add (library, types = new HashSet<string> ());
			types.Add (type.Replace ('/', '.').Replace ('+', '.'));
		}

		void Update ()
		{
			Action<string> creator = file => {
				XDocument docs = LoadFile (this.file);

				var seenLibraries = new HashSet<string> ();

				UpdateExistingLibraries (docs, seenLibraries);
				GenerateMissingLibraries (docs, seenLibraries);

				SortLibraries (docs.Root);
				SortTypes (docs.Root);

				using (var output = CreateWriter (file)) {
					foreach (var node in docs.Nodes ()) {
						if (node.NodeType == XmlNodeType.Element || node.NodeType == XmlNodeType.Text)
							continue;
						node.WriteTo (output);
					}
					docs.Root.WriteTo (output);
					output.WriteWhitespace ("\r\n");
				}
			};
			MdocFile.UpdateFile (this.file, creator);
		}

		static XDocument LoadFile (string file)
		{
			if (file == "-" || !File.Exists (file))
				return CreateDefaultDocument ();

			var settings = new XmlReaderSettings {
				ProhibitDtd = false,
			};
			using (var reader = XmlReader.Create (file, settings))
				return XDocument.Load (reader);
		}

		static XDocument CreateDefaultDocument ()
		{
			return new XDocument (
					new XComment (" ====================================================================== "),
					new XComment (" This XML is a description of the Common Language Infrastructure (CLI) library. "),
					new XComment (" This file is a normative part of Partition IV of the following standards: ISO/IEC 23271 and ECMA 335 "),
					new XComment (" ====================================================================== "),
					new XDocumentType ("Libraries", null, "CLILibraryTypes.dtd", null),
					new XElement ("Libraries"));
		}

		static XmlWriter CreateWriter (string file)
		{
			var settings = new XmlWriterSettings {
				Indent              = true,
				IndentChars         = "\t",
				NewLineChars        = "\r\n",
				OmitXmlDeclaration  = true,
			};

			if (file == "-")
				return XmlWriter.Create (Console.Out, settings);

			settings.Encoding = new UTF8Encoding (false);
			return XmlWriter.Create (file, settings);
		}

		void UpdateExistingLibraries (XDocument docs, HashSet<string> seenLibraries)
		{
			foreach (XElement types in docs.Root.Elements ("Types")) {
				XAttribute library = types.Attribute ("Library");
				HashSet<string> libraryTypes;
				if (library == null || !libraries.TryGetValue (library.Value, out libraryTypes)) {
					continue;
				}
				seenLibraries.Add (library.Value);
				var seenTypes = new HashSet<string> ();
				foreach (XElement type in types.Elements ("Type").ToList ()) {
					XAttribute fullName = type.Attribute ("FullName");
					string typeName = fullName == null
						? null
						: XmlDocUtils.ToEscapedTypeName (fullName.Value);
					if (typeName == null || !libraryTypes.Contains (typeName)) {
						continue;
					}
					type.Remove ();
					seenTypes.Add (typeName);
					types.Add (LoadType (typeName, library.Value));
				}
				foreach (string typeName in libraryTypes.Except (seenTypes))
					types.Add (LoadType (typeName, library.Value));
			}
		}

		void GenerateMissingLibraries (XDocument docs, HashSet<string> seenLibraries)
		{
			foreach (KeyValuePair<string, HashSet<string>> lib in libraries) {
				if (seenLibraries.Contains (lib.Key))
					continue;
				seenLibraries.Add (lib.Key);
				docs.Root.Add (new XElement ("Types", new XAttribute ("Library", lib.Key),
							lib.Value.Select (type => LoadType (type, lib.Key))));
			}
		}

		XElement LoadType (string type, string library)
		{
			foreach (KeyValuePair<string, string> permutation in GetTypeDirectoryFilePermutations (type)) {
				foreach (string root in directories) {
					string path = Path.Combine (root, Path.Combine (permutation.Key, permutation.Value + ".xml"));
					if (File.Exists (path))
						return FixupType (path, library);
				}
			}
			throw new FileNotFoundException ("Unable to find documentation file for type: " + type + ".");
		}

		// type has been "normalized", which (alas) means we have ~no clue which
		// part is the namespace and which is the type name, particularly
		// problematic as types may be nested to any level.
		// Try ~all permutations. :-)
		static IEnumerable<KeyValuePair<string, string>> GetTypeDirectoryFilePermutations (string type)
		{
			int end = type.Length;
			int dot;
			while ((dot = type.LastIndexOf ('.', end-1)) >= 0) {
				yield return new KeyValuePair<string, string> (
						type.Substring (0, dot), 
						type.Substring (dot+1).Replace ('.', '+'));
				end = dot;
			}
			yield return new KeyValuePair<string, string> ("", type.Replace ('.', '+'));
		}

		static XElement FixupType (string path, string library)
		{
			var type = XElement.Load (path);

			XAttribute fullName   = type.Attribute ("FullName");
			XAttribute fullNameSp = type.Attribute ("FullNameSP");
			if (fullNameSp == null && fullName != null) {
				type.Add (new XAttribute ("FullNameSP", fullName.Value.Replace ('.', '_')));
			}
			if (type.Element ("TypeExcluded") == null)
				type.Add (new XElement ("TypeExcluded", "0"));
			if (type.Element ("MemberOfLibrary") == null) {
				XElement member = new XElement ("MemberOfLibrary", library);
				XElement assemblyInfo = type.Element ("AssemblyInfo");
				if (assemblyInfo != null)
					assemblyInfo.AddBeforeSelf (member);
				else
					type.Add (member);
			}

			XElement ai = type.Element ("AssemblyInfo");

			XElement assembly = 
				XElement.Load (
						Path.Combine (
							Path.Combine (Path.GetDirectoryName (path), ".."), 
							"index.xml"))
				.Element ("Assemblies")
				.Elements ("Assembly")
				.FirstOrDefault (a => a.Attribute ("Name").Value == ai.Element ("AssemblyName").Value &&
						a.Attribute ("Version").Value == ai.Element ("AssemblyVersion").Value);
			if (assembly == null)
				return type;

			if (assembly.Element ("AssemblyPublicKey") != null)
				ai.Add (assembly.Element ("AssemblyPublicKey"));

			if (assembly.Element ("AssemblyCulture") != null)
				ai.Add (assembly.Element ("AssemblyCulture"));
			else
				ai.Add (new XElement ("AssemblyCulture", "neutral"));

			// TODO: assembly attributes?
			// The problem is that .NET mscorlib.dll v4.0 has ~26 attributes, and
			// importing these for every time seems like some serious bloat...
			var clsAttr = assembly.Elements ("Attributes").Elements ("Attribute")
				.FirstOrDefault (a => a.Element ("AttributeName").Value.StartsWith ("System.CLSCompliant"));
			if (clsAttr != null) {
				var dest = ai.Element ("Attributes");
				if (dest == null)
					ai.Add (dest = new XElement ("Attributes"));
				dest.Add (clsAttr);
			}

			return type;
		}

		static void SortLibraries (XContainer libraries)
		{
			SortElements (libraries, (x, y) => x.Attribute ("Library").Value.CompareTo (y.Attribute ("Library").Value));
		}

		static void SortElements (XContainer container, Comparison<XElement> comparison)
		{
			var items = new List<XElement> ();
			foreach (var e in container.Elements ())
				items.Add (e);
			items.Sort (comparison);
			for (int i = items.Count - 1; i > 0; --i) {
				items [i-1].Remove ();
				items [i].AddBeforeSelf (items [i-1]);
			}
		}

		static void SortTypes (XContainer libraries)
		{
			foreach (var types in libraries.Elements ("Types")) {
				SortElements (types, (x, y) => {
						string xName, yName;
						int xCount, yCount;

						GetTypeSortName (x, out xName, out xCount);
						GetTypeSortName (y, out yName, out yCount);

						int c = xName.CompareTo (yName);
						if (c != 0)
							return c;
						if (xCount < yCount)
							return -1;
						if (yCount < xCount)
							return 1;
						return 0;
				});
			}
		}

		static void GetTypeSortName (XElement element, out string name, out int typeParamCount)
		{
			typeParamCount = 0;
			name = element.Attribute ("Name").Value;

			int lt = name.IndexOf ('<');
			if (lt >= 0) {
				int gt = name.IndexOf ('>', lt);
				if (gt >= 0) {
					typeParamCount = name.Substring (lt, gt-lt).Count (c => c == ',') + 1;
				}
				name = name.Substring (0, lt);
			}
		}
	}
}
