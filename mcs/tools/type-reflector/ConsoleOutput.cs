//
// ConsoleOutput.cs: 
//   Finds types and (optionally) shows reflection information about 
//   the types.
//
// Author: Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002 Jonathan Pryor
//

// #define TRACE

using System;
using System.Collections;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Mono.TypeReflector
{
	public class ConsoleOutput {

		private static BooleanSwitch console = new BooleanSwitch ("console",
				"console-specific and command-line handling output");

		private static void TraceStringArray (string message, IEnumerable contents)
		{
			Trace.WriteLineIf (console.Enabled, message);
			foreach (string s in contents) {
				Trace.WriteLineIf (console.Enabled, "  " + s);
			}
		}

		private static void PrintVersion ()
		{
			Console.WriteLine ("type-reflector 0.5");
			Console.WriteLine ("Written by Jonathan Pryor.");
			Console.WriteLine ();
			Console.WriteLine ("Copyright (C) 2002 Jonathan Pryor.");
		}

		private static void InitFactory ()
		{
			// TypeDisplayerFactory.Add ("explicit", typeof(ExplicitTypeDisplayer));
			// TypeDisplayerFactory.Add ("reflection", typeof(ReflectionTypeDisplayer));
			// TypeDisplayerFactory.Add ("c#", typeof(CSharpTypeDisplayer));
			Factories.FormatterFactory.Add ("default", typeof (DefaultNodeFormatter));
			Factories.FormatterFactory.Add ("csharp", typeof (CSharpNodeFormatter));
			Factories.FinderFactory.Add ("explicit", typeof (ExplicitNodeFinder));
			Factories.FinderFactory.Add ("reflection", typeof (ReflectionNodeFinder));
		}

		public static void Main (string[] args)
		{
			InitFactory ();

			TypeReflectorOptions options = new TypeReflectorOptions ();

			try {
				options.ParseOptions (args);
			} catch (Exception e) {
				Console.WriteLine (e.Message);
				Console.WriteLine ("See `{0} --help' for more information", ProgramOptions.ProgramName);
				// Console.WriteLine ("** Full Message continues:\n" + e);
				return;
			}

			if (options.FoundHelp) {
				Console.WriteLine (options.OptionsHelp);
				return;
			}

			if (options.DefaultAssemblies) {
				Console.WriteLine ("The default search assemblies are:");
				foreach (string s in TypeReflectorOptions.GetDefaultAssemblies ()) {
					Console.WriteLine ("  {0}", s);
				}
				return;
			}

			if (options.Version) {
				PrintVersion ();
				return;
			}

			if (options.Types.Count == 0) {
				Console.WriteLine ("No types specified.");
				Console.WriteLine ("See `{0} --help' for more information", ProgramOptions.ProgramName);
				return;
			}

			TraceStringArray ("Search Assemblies: ", options.Assemblies);
			TraceStringArray ("Search for Types: ", options.Types);

			TypeLoader loader = new TypeLoader (options.Assemblies);
			loader.MatchBase = options.MatchBase;
			loader.MatchFullName = options.MatchFullName;
			loader.MatchClassName = options.MatchClassName;
			loader.MatchNamespace = options.MatchNamespace;
			loader.MatchMethodReturnType = options.MatchReturnType;

			IndentingTextWriter writer = new IndentingTextWriter (Console.Out);

			int depth = options.MaxDepth;

			INodeFormatter formatter = Factories.FormatterFactory.Create (options.Formatter);
			if (formatter == null) {
				Console.WriteLine ("Error: invalid formatter: " + options.Formatter);
				return;
			}

			NodeFinder f = (NodeFinder) Factories.FinderFactory.Create (options.Finder);
			if (f == null) {
				Console.WriteLine ("Error: invalid finder: " + options.Finder);
				return;
			}

			f.VerboseOutput = options.VerboseOutput;
			f.ShowBase = options.ShowBase;
			f.ShowConstructors = options.ShowConstructors;
			f.ShowEvents = options.ShowEvents;
			f.ShowFields = options.ShowFields;
			f.ShowInterfaces = options.ShowInterfaces;
			f.ShowMethods = options.ShowMethods;
			f.ShowProperties = options.ShowProperties;
			f.ShowTypeProperties = options.ShowTypeProperties;
			f.ShowInheritedMembers = options.ShowInheritedMembers;
			f.ShowNonPublic = options.ShowNonPublic;
			f.ShowMonoBroken = options.ShowMonoBroken;
			f.FlattenHierarchy = options.FlattenHierarchy;
			f.MaxDepth = options.MaxDepth;

			foreach (string t in options.Types) {
				try {
					ICollection typesFound = loader.LoadTypes (t);
					if (typesFound.Count > 0)
						foreach (Type type in loader.LoadTypes(t)) {
							// Console.WriteLine ("** displaying type: " + type);
							Node root = new Node (formatter, 
									f);
									// new GroupingNodeFinder (f));
									// new ExplicitNodeFinder());
							// root.Extra = new NodeInfo (null, type, NodeTypes.Type);
							root.NodeInfo = new NodeInfo (null, type);
							ShowNode (root, writer, depth);
						}
					else
						Console.WriteLine ("Unable to find type `{0}'.", t);
				} catch (Exception e) {
					Console.WriteLine ("Unable to display type `{0}': {1}.", t, e.ToString());
				}
			}
		}

		private static void ShowNode (Node root, IndentingTextWriter writer, int maxDepth)
		{
			// Console.WriteLine ("** current max depth: " + maxDepth);
			writer.WriteLine (root.Description);
			if (maxDepth > 0) {
				using (Indenter i = new Indenter (writer)) {
					foreach (Node child in root.GetChildren()) {
						ShowNode (child, writer, maxDepth-1);
					}
				}
			}
		}
	}
}

