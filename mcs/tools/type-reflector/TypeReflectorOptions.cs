//
// TypeReflectorOptions.cs: Handles `type-reflector' program options
//
// Author: Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002 Jonathan Pryor
//

using System;
using System.Collections;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Mono.TypeReflector
{
	public class TypeReflectorOptions : ProgramOptions {

		private static char onlyAssemblies      = 'A';
		private static char addAssemblies       = 'a';
		private static string defaultAssemblies = "default-assemblies";
		private static char matchAll            = 'M';
		private static char matchFullName       = 'F';
		private static char matchClass          = 'C';
		private static char matchBase           = 'B';
		private static char matchNamespace      = 'N';
		private static char matchReturnType     = 'R';
		private static char showBase            = 'b';
		private static char showConstructors    = 'c';
		private static char showEvents          = 'e';
		private static char showFields          = 'f';
		private static char showInterfaces      = 'i';
		private static char showMethods         = 'm';
		private static char showProperties      = 'p';
		private static char showTypeProperties  = 't';
		private static char showNonPublic       = 'U';
		private static char showMonoBroken      = 'K';
		private static char showAll             = 'S';
		private static char showInheritedMembers= 'r';
		private static char verboseOutput       = 'v';
		private static char flattenHierarchy    = 'l';
		private static char invokeMethods       = 'n';
		private static string version           = "version";
		private static string formatter         = "formatter";
		private static string finder            = "finder";
		private static string displayer         = "displayer";
		private static string maxDepth          = "max-depth";

		public TypeReflectorOptions ()
		{
			AddArgumentOption (onlyAssemblies,  "only-assemblies",
				"Only search for types within the specified " +
				"assemblies.  Assemblies are searched for " +
				"within the directories specified with " +
				"--only-directories.", 
				"<assembly-list>");
			AddArgumentOption (addAssemblies,   "add-assemblies",
				"Add the specified assemblies to the list of " +
				"assemblies searched for types.", 
				"<assembly-list>");
			AddOption (matchAll,                "match-all",
				"Type names should be matched in all locations"
				);
			AddOption (matchFullName,           "match-full-name",
				"Match type names against the full type name " +
				"(Namespace + Class Name).\n" +
				"This is the default.");
			AddOption (matchClass,              "match-class",
				"Match type names against only the class name");
			AddOption (matchNamespace,          "match-namespace",
				"Match the type's namespace.");
			AddOption (matchBase,               "match-base",
				"Match type names against the base class " + 
				"name.\nMatching of the base name is " +
				"identical to top-level type matches--it " +
				"matches the namespace, class name, or full " +
				"type name.");
			/*
			AddOption (matchReturnType,         "match-return-type",
				"Match the return type of methods");
			 */
			AddOption (showBase,                "show-base",
				"Show the base class.");
			AddOption (showConstructors,        "show-constructors",
				"Show the type's constructors.");
			AddOption (showEvents,              "show-events",
				"Show the type's events.");
			AddOption (showFields,              "show-fields",
				"Show the type's fields.");
			AddOption (showInterfaces,          "show-interfaces",
				"Show the type's interfaces");
			AddOption (showMethods,             "show-methods",
				"Show the type's methods.");
			AddOption (showProperties,          "show-properties",
				"Show the type's properties.");
			AddOption (showTypeProperties,      "show-type-properties",
				"Show the properties of the type's System.Type"+
				"object.\nThis is not set by -S.");
			AddOption (showInheritedMembers,    "show-inherited-members",
				"Show inherited members (members declared by " +
				"base classes).\nThis is not set by -S.");
			AddOption (showNonPublic,           "show-non-public",
				"Show non-public members.\n" + 
				"This is not set by -S.");
			AddOption (showMonoBroken,          "show-mono-broken",
				"Some attributes shown in verbose output " +
				"cause exceptions when run under Mono.  " +
				"These attributes are not shown by default.  "+
				"This option shows these disabled attributes."+
				"\nThis is not set by -S.");
			AddOption (showAll,                 "show-all",
				"Show everything except System.Type "+
				"properties, inherited members, non-public "+
				"members, and \"broken\" Mono attributes.  " +
				"Equivalent to -bcefimp.");
			AddOption (flattenHierarchy,        "flatten-hierarchy",
				"Static members of base types should be " + 
				"displayed.");
			AddOption (invokeMethods,           "invoke-methods",
				"Invoke static methods that accept no arguments " +
				"and display the return value in the method " +
				"description (e.g. -m).\n" +
				"This is not set by -S.");
			StringBuilder formatterDescription = new StringBuilder ();
			formatterDescription.Append ("Experimental.  Specify the output style to use.  Available values are:");
			foreach (object o in Factories.Formatter.Keys)
				formatterDescription.Append (string.Format ("\n{0}", o));
			AddArgumentOption (formatter, formatterDescription.ToString(), "<formatter>");

			StringBuilder finderDescription = new StringBuilder ();
			finderDescription.Append ("Experimental.  Specify how nodes are found.  Available values are:");
			foreach (object o in Factories.Finder.Keys)
				finderDescription.Append (string.Format ("\n{0}", o));
			AddArgumentOption (finder, finderDescription.ToString(), "<finder>");

			StringBuilder displayerDescription = new StringBuilder ();
			displayerDescription.Append ("Experimental.  Specify where output should be displayed.  Available values are:");
			foreach (object o in Factories.Displayer.Keys)
				displayerDescription.Append (string.Format ("\n{0}", o));
			AddArgumentOption (displayer, displayerDescription.ToString(), "<displayer>");

			AddOption (verboseOutput,           "verbose-output",
				"Print the contents of all the public " + 
				"attributes of the reflection information " +
				"classes.");
			AddOption (defaultAssemblies, 
				"Print the default search assemblies and exit.");
			AddArgumentOption (maxDepth, "Specify how deep the verbose output tree should display output.\nDefault is 10.", "INTEGER");
			AddOption (version, "Output version information and exit.");
			AddHelpOption ();
		}

		public override void ParseOptions (string[] options)
		{
			base.ParseOptions (options);

			_showAll = base.FoundOption (showAll);
			_matchAll = base.FoundOption (matchAll);
		}

		public IList Types {
			get {return base.UnmatchedOptions;}
		}

		private bool _matchAll;

		public bool MatchAll {
			get {return _matchAll;}
		}

		// default: true;
		public bool MatchFullName {
			get {
				if (!MatchClassName && !MatchNamespace && !MatchBase && 
				    !MatchReturnType)
					return true;
				return MatchAll || base.FoundOption (matchFullName);
			}
		}

		public bool MatchClassName {
			get {
				return MatchAll || base.FoundOption (matchClass);
			}
		}

		public bool MatchNamespace {
			get {
				return MatchAll || base.FoundOption (matchNamespace);
			}
		}

		public bool MatchBase {
			get {
				return MatchAll || base.FoundOption (matchBase);
			}
		}

		public bool MatchReturnType {
			get {
				return MatchAll || base.FoundOption (matchReturnType);
			}
		}

		private bool _showAll;

		public bool ShowAll {
			get {
				return _showAll;
			}
		}

		public bool ShowBase {
			get {
				return ShowAll || base.FoundOption (showBase);
			}
		}

		public bool ShowConstructors {
			get {
				return ShowAll || base.FoundOption (showConstructors);
			}
		}

		public bool ShowEvents {
			get {
				return ShowAll || base.FoundOption (showEvents);
			}
		}

		public bool ShowFields {
			get {
				return ShowAll || base.FoundOption (showFields);
			}
		}

		public bool ShowInterfaces {
			get {
				return ShowAll || base.FoundOption (showInterfaces);
			}
		}

		public bool ShowMethods {
			get {
				return ShowAll || base.FoundOption (showMethods);
			}
		}

		public bool ShowProperties {
			get {
				return ShowAll || base.FoundOption (showProperties);
			}
		}

		public bool ShowTypeProperties {
			get {
				return base.FoundOption (showTypeProperties);
			}
		}

		public bool ShowInheritedMembers {
			get {
				return base.FoundOption (showInheritedMembers);
			}
		}

		public bool ShowNonPublic {
			get {
				return base.FoundOption (showNonPublic);
			}
		}

		public bool ShowMonoBroken {
			get {
				return base.FoundOption (showMonoBroken);
			}
		}

		public bool FlattenHierarchy {
			get {
				return base.FoundOption (flattenHierarchy);
			}
		}

		public bool VerboseOutput {
			get {
				return base.FoundOption (verboseOutput);
			}
		}

		public bool Version {
			get {
				return base.FoundOption (version);
			}
		}

		public int MaxDepth {
			get {
				string v = base.FoundOptionValue (maxDepth);
				if (v != null) {
					try {
						return Convert.ToInt32 (v);
					} catch {
					}
				}
				return 10;
			}
		}

		public bool InvokeMethods {
			get {return base.FoundOption (invokeMethods);}
		}

		public bool DefaultAssemblies {
			get {
				return base.FoundOption (defaultAssemblies);
			}
		}

		public static string[] GetDefaultAssemblies ()
		{
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies ();

			string sysdir = null;

			foreach (Assembly a in assemblies) {
				string codebase = a.CodeBase;
				if (codebase.EndsWith ("corlib.dll")) {
					sysdir = codebase.Substring (0, codebase.LastIndexOf ("/"));
					break;
				}
			}

			return Directory.GetFiles (new Uri (sysdir).LocalPath, "*.dll");
		}

		public ICollection Assemblies {
			get {
				string o = base.FoundOptionValue (onlyAssemblies);
				string a = base.FoundOptionValue (addAssemblies);
				ArrayList r = new ArrayList ();

				if (o == null) {
					r.AddRange (GetDefaultAssemblies ());
				}
				else {
					r.AddRange (o.Split (Path.PathSeparator));
				}

				if (a != null)
					r.AddRange (a.Split (Path.PathSeparator));

				return r;
			}
		}

		public string Formatter {
			get {
				string s = base.FoundOptionValue (formatter);
				if (s == null)
					return "default";
				return s;
			}
		}

		public string Finder {
			get {
				string s = base.FoundOptionValue (finder);
				if (s == null)
					return "explicit";
				return s;
			}
		}

		public string Displayer {
			get {
				string s = base.FoundOptionValue (displayer);
				if (s == null)
					return string.Empty;
				return s;
			}
		}

		public override string OptionsHelp {
			get {
				StringBuilder sb = new StringBuilder ();
				TextFormatter tg0 = new TextFormatter ();
				TextFormatter tg4 = new TextFormatter (4, 80, 0);
				sb.Append (
					"Prints out type information\n" +
					"\n" +
					"Usage: " + ProgramName + " [options] <types>\n" +
					"\n" +
					"Where [options] can include:\n");
				sb.Append (base.OptionsHelp);
				sb.Append (
					"\n" + 
					tg0.Group (
						"<types> is interpreted as a regular expression.  As regular expression " + 
						"meta-characters are seldom used in class names, specifying a type name " +
						"looks for all types that have the specified type name as a substring.  " +
						"To get a listing of all available types, pass '.' as the type.  (Since " +
						"regular expressions are used, '.' will match any character, thus matching " +
						"all possible types.)") +
					"\n\n" +
					tg0.Group (
						"<assembly-list> is a `" + Path.PathSeparator + "'-delimited list.  " + 
						"For example, `" + 
						String.Format ("foo{0}bar{0}baz", 
							Path.PathSeparator) + "' is a valid list.\n") +
					"\n"
					);
				sb.Append (String.Format (
					"Examples:\n" +
					"  {0} Type\n", ProgramName));
				sb.Append (String.Format ("    {0}", tg4.Group ("Finds all types that have `Type' (case-sensitive) as part of their name.")));
				sb.Append (String.Format (
						"\n\n" +
						"  {0} [Tt][Yy][Pp][Ee]\n", ProgramName));
				sb.Append (String.Format ("    {0}", tg4.Group ("Finds all types that have `Type' (case-insensitive) as part of their name.")));
				sb.Append (String.Format (
						"\n\n" +
						"  {0} -A my-assembly.dll MyType\n", ProgramName));
				sb.Append (String.Format ("    {0}", tg4.Group ("Finds all types that have `MyType' as part of their name within the assembly `my-assembly'.")));
				sb.Append (String.Format (
						"\n\n" +
						"  {0} -SKt MyType\n", ProgramName));
				sb.Append (String.Format ("    {0}", tg4.Group ("Find all types that have `MyType' as part of their name, and for those types show all information (-S) including information Mono generates exceptions for (-K) and show the values of the public attributes of the System.Type object for the type (-t).")));
				sb.Append ("\n\nReport bugs to <jonpryor@vt.edu>.");
				return sb.ToString ();
			}
		}
	}
}

