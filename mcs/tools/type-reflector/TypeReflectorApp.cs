//
// TypeReflectorApp.cs: 
//   Finds types and sends them to a displayer.
//
// Author: Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002 Jonathan Pryor
//

// #define TRACE

using System;
using System.Collections;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Mono.TypeReflector
{
	public class TypeReflectorApp
	{
		private static BooleanSwitch console = new BooleanSwitch ("console",
				"console-specific and command-line handling output");

		private static void TraceStringArray (string message, IEnumerable contents)
		{
			Trace.WriteLineIf (console.Enabled, message);
			foreach (string s in contents) {
				Trace.WriteLineIf (console.Enabled, "  " + s);
			}
		}

		public static void PrintVersion ()
		{
			Console.WriteLine ("type-reflector 0.6");
			Console.WriteLine ("Written by Jonathan Pryor.");
			Console.WriteLine ();
			Console.WriteLine ("Copyright (C) 2002 Jonathan Pryor.");
		}

		private static void InitFactories ()
		{
			InitFactory ("displayers", Factories.Displayer);
			InitFactory ("finders",    Factories.Finder);
			InitFactory ("formatters", Factories.Formatter);
		}

		private static void InitFactory (string section, TypeFactory factory)
		{
			try {
				IDictionary d = (IDictionary) ConfigurationSettings.GetConfig (section);
				foreach (DictionaryEntry de in d) {
					try {
						factory.Add (
								de.Key.ToString(), 
								Type.GetType (de.Value.ToString(), true));
					}
					catch (Exception e) {
						Trace.WriteLineIf (console.Enabled, 
								string.Format ("Error adding {0} ({1}): {2}",
									de.Key, de.Value, e.Message));
					}
				}
			}
			catch {
				Trace.WriteLineIf (console.Enabled, 
						string.Format ("Unable to open section: {0}", section));
			}
		}

		public static void Main (string[] args)
		{
			InitFactories ();

			TypeReflectorOptions options = new TypeReflectorOptions ();

			bool quit = false;

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
				quit = true;
			}

			if (options.DefaultAssemblies) {
				Console.WriteLine ("The default search assemblies are:");
				foreach (string s in TypeReflectorOptions.GetDefaultAssemblies ()) {
					Console.WriteLine ("  {0}", s);
				}
				quit = true;
			}

			if (options.Version) {
				PrintVersion ();
				quit = true;
			}

			if (quit)
				return;

			TraceStringArray ("Search Assemblies: ", options.Assemblies);
			TraceStringArray ("Search for Types: ", options.Types);

			TypeLoader loader = CreateLoader (options);

			INodeFormatter formatter = CreateFormatter (options);
			if (formatter == null) {
				Console.WriteLine ("Error: invalid formatter: " + options.Formatter);
				return;
			}

			INodeFinder finder = CreateFinder (options);
			if (finder == null) {
				Console.WriteLine ("Error: invalid finder: " + options.Finder);
				return;
			}

			ITypeDisplayer displayer = CreateDisplayer (options);
			if (displayer == null) {
				Console.WriteLine ("Error: invalid displayer: " + options.Displayer);
				return;
			}

			if (options.Types.Count == 0) {
				Console.WriteLine ("No types specified.");
				Console.WriteLine ("See `{0} --help' for more information", ProgramOptions.ProgramName);
				return;
			}

			displayer.Finder = finder;
			displayer.Formatter = formatter;
			displayer.Options = options;

			// Find the requested types and display them.
			FindTypes (displayer, loader, options.Types);

			displayer.Run ();
		}
		
		public static void FindTypes (ITypeDisplayer displayer, TypeLoader loader, IList types)
		{
			try {
				ICollection typesFound = loader.LoadTypes (types);
				if (typesFound.Count > 0)
					foreach (Type type in typesFound) {
						displayer.AddType (type);
					}
				else
					displayer.ShowError ("Unable to find types.");
			} catch (Exception e) {
				displayer.ShowError (string.Format ("Unable to display type: {0}.", 
					e.ToString()));
			}
		}

		public static TypeLoader CreateLoader (TypeReflectorOptions options)
		{
			TypeLoader loader = new TypeLoader (options.Assemblies);
			loader.MatchBase = options.MatchBase;
			loader.MatchFullName = options.MatchFullName;
			loader.MatchClassName = options.MatchClassName;
			loader.MatchNamespace = options.MatchNamespace;
			loader.MatchMethodReturnType = options.MatchReturnType;
			return loader;
		}

		public static ITypeDisplayer CreateDisplayer (TypeReflectorOptions options)
		{
			ITypeDisplayer d = Factories.Displayer.Create (options.Displayer);

			if (d != null) {
				d.MaxDepth = options.MaxDepth;
			}

			return d;
		}

		public static INodeFinder CreateFinder (TypeReflectorOptions options)
		{
			INodeFinder finder = Factories.Finder.Create (options.Finder);
			NodeFinder f = finder as NodeFinder;

			if (f != null) {
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
			}

			return finder;
		}

		public static INodeFormatter CreateFormatter (TypeReflectorOptions options)
		{
			INodeFormatter formatter = Factories.Formatter.Create (options.Formatter);
			NodeFormatter f = formatter as NodeFormatter;

			if (f != null) {
				f.InvokeMethods = options.InvokeMethods;
			}

			return formatter;
		}
	}
}

