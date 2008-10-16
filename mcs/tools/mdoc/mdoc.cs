// Part of the mdoc(7) suite of tools.
using System;
using System.Collections.Generic;
using System.Linq;
using NDesk.Options;

namespace Mono.Documentation {
	class MDoc {

		private static bool debug;

		private static void Main (string[] args)
		{
			MDoc d = new MDoc ();
			try {
				d.Run (args);
			}
			catch (Exception e) {
				if (debug) {
					Console.Error.WriteLine ("mdoc: {0}", e.ToString ());
				}
				else {
					Console.Error.WriteLine ("mdoc: {0}", e.Message);
				}
				Console.Error.WriteLine ("See `mdoc help' for more information.");
			}
		}

		Dictionary<string, Action<List<string>>> subcommands;

		private void Run (string[] args)
		{
			subcommands = new Dictionary<string, Action<List<string>>> () {
				{ "assemble",         Assemble },
				{ "export-html",      ExportHtml },
				{ "export-msxdoc",    ExportMSXDoc},
				{ "help",             Help },
				{ "update",           Update },
				{ "validate",         Validate },
			};

			bool showVersion = false;
			bool showHelp    = false;
			var p = new OptionSet () {
				{ "version",  v => showVersion = v != null },
				{ "debug",    v => debug = v != null },
				{ "h|?|help", v => showHelp = v != null },
			};

			List<string> extra = p.Parse (args);

			if (showVersion) {
				Console.WriteLine ("mdoc 0.1.0");
				return;
			}
			if (extra.Count == 0) {
				Help (null);
				return;
			}
			if (showHelp) {
				extra.Add ("--help");
			}
			GetCommand (extra [0]) (extra);
		}

		private Action<List<string>> GetCommand (string command)
		{
			Action<List<string>> h;
			if (!subcommands.TryGetValue (command, out h)) {
				Error ("Unknown command: {0}.", command);
			}
			return h;
		}

		private void Help (List<string> args)
		{
			if (args != null && args.Count > 1) {
				for (int i = 1; i < args.Count; ++i) {
					GetCommand (args [i]) (new List<string>(){args [i], "--help"});
				}
				return;
			}
			Console.WriteLine (
				"usage: mdoc COMMAND [OPTIONS]\n" +
				"Use `mdoc help COMMAND' for help on a specific command.\n" +
				"\n" + 
				"Available commands:\n\n   " +
				string.Join ("\n   ", subcommands.Keys.OrderBy (v => v).ToArray()) +
				"\n\n" + 
				"mdoc is a tool for documentation management.\n" +
				"For additional information, see http://www.mono-project.com/"
			);
		}

		private static void Error (string format, params object[] args)
		{
			throw new Exception (string.Format (format, args));
		}

		private List<string> Parse (OptionSet p, List<string> args, string command,
				string prototype, string description, ref bool showHelp)
		{
			List<string> extra = null;
			if (args != null) {
				extra = p.Parse (args.Skip (1));
			}
			if (args == null || showHelp) {
				Console.WriteLine ("usage: mdoc {0} {1}", 
						args == null ? command : args [0], prototype);
				Console.WriteLine ();
				Console.WriteLine (description);
				Console.WriteLine ();
				Console.WriteLine ("Available Options:");
				p.WriteOptionDescriptions (Console.Out);
				return null;
			}
			return extra;
		}

		private void Assemble (List<string> args)
		{
			string[] validFormats = {
				"ecma", 
				"ecmaspec", 
				"error", 
				"hb", 
				"man", 
				"simple", 
				"xhtml"
			};
			var formats = new Dictionary<string, List<string>> ();
			string prefix = null;
			bool showHelp = false;
			string format = "ecma";
			var p = new OptionSet () {
				{ "f|format=",
					"The documentation {FORMAT} used in DIRECTORIES.  " + 
						"Valid formats include:\n  " +
						string.Join ("\n  ", validFormats) + "\n" +
						"If not specified, the default format is `ecma'.",
					v => {
						if (Array.IndexOf (validFormats, v) < 0)
							Error ("Invalid documentation format: {0}.", v);
						format = v;
					} },
				{ "o|out=",
					"Provides the output file prefix; the files {PREFIX}.zip and " + 
						"{PREFIX}.tree will be created.\n" +
						"If not specified, `tree' is the default PREFIX.",
					v => prefix = v },
				{ "h|?|help",
					"Show this message and exit.",
					v => showHelp = v != null },
				{ "<>", v => AddFormat (formats, format, v) },
			};
			List<string> extra = Parse (p, args, "assemble", 
					"[OPTIONS]+ DIRECTORIES",
					"Assemble documentation within DIRECTORIES for use within the monodoc browser.", 
					ref showHelp);
			if (extra == null)
				return;
			var command = new List<string> ();
			foreach (var f in formats.Keys) {
				foreach (var v in formats [f]) {
					command.Add ("--" + f);
					command.Add (v);
				}
			}
			if (prefix != null) {
				command.Add ("-o");
				command.Add (prefix);
			}
			Assembler.Main (command.ToArray ());
		}

		private static void AddFormat (Dictionary<string, List<string>> d, string format, string file)
		{
			if (format == null)
				Error ("No format specified.");
			List<string> l;
			if (!d.TryGetValue (format, out l)) {
				l = new List<string> ();
				d.Add (format, l);
			}
			l.Add (file);
		}

		private void ExportHtml (List<string> args)
		{
			bool dumptemplate = false;
			string dest = null, template = null, extension = null;
			bool showHelp = false;
			var p = new OptionSet () {
				{ "ext=",
					"The file {EXTENSION} to use for created files.  "+
						"This defaults to \"html\".",
					v => extension = v },
				{ "template=",
					"An XSLT {FILE} to use to generate the created " + 
						"files.  If not specified, uses the template generated by --dump-template.",
					v => template = v },
				{ "default-template",
					"Writes the default XSLT to stdout.",
					v => dumptemplate = v != null },
				{ "o|out=",
					"The {DIRECTORY} to place the generated files and directories.",
					v => dest = v },
				{ "h|?|help", 
					"Show this message and exit.",
					v => showHelp = v != null },
			};
			List<string> extra = Parse (p, args, "export-html", 
					"[OPTIONS]+ DIRECTORIES",
					"Export mdoc documentation within DIRECTORIES to HTML.", ref showHelp);
			if (extra == null)
				return;
			var command = new List<string> ();
			if (dumptemplate)
				command.Add ("-dumptemplate");
			if (template != null)
				command.Add ("-template:" + template);
			if (extension != null)
				command.Add ("-ext:" + extension);
			if (dest != null)
				command.Add ("-dest:" + dest);
			if (extra.Count == 0)
				Monodocs2HTML.Main (command.ToArray ());
			foreach (var source in extra) {
				command.Add ("-source:" + source);
				Monodocs2HTML.Main (command.ToArray ());
				command.RemoveAt (command.Count-1);
			}
		}

		private void ExportMSXDoc (List<string> args)
		{
			string file = null;
			bool showHelp = false;
			var p = new OptionSet () {
				{ "o|out=", 
					"The XML {FILE} to generate.\n" + 
					"If not specified, will create a set of files in the curent directory " +
					"based on the //AssemblyInfo/AssemblyName values within the documentation.\n" +
					"Use '-' to write to standard output.",
					v => file = v },
				{ "h|?|help", 
					"Show this message and exit.",
					v => showHelp = v != null },
			};
			List<string> extra = Parse (p, args, "export-slashdoc", 
					"[OPTIONS]+ DIRECTORIES",
					"Export mdoc(5) documentation within DIRECTORIES into \n" +
						"Microsoft XML Documentation format files.",
					ref showHelp);
			if (extra == null)
				return;
			if (file != null) {
				extra.Add ("-o");
				extra.Add (file);
			}
			Monodocs2SlashDoc.Main (extra.ToArray ());
		}

		private void Update (List<string> args)
		{
			string file = null;
			bool delete = false, showHelp = false;
			string path = null;
			var p = new OptionSet () {
				{ "i|import=", 
					"Import documentation from {FILE}.",
					v => file = v },
				{ "delete",
					"Delete removed members from the XML files.",
					v => delete = v != null },
				{ "o|out=",
					"Root {DIRECTORY} to generate/update documentation.",
					v => path = v },
				{ "h|?|help",
					"Show this message and exit.",
					v => showHelp = v != null },
			};
			List<string> extra = Parse (p, args, "update", 
					"[OPTIONS]+ ASSEMBLIES",
					"Create or update documentation from ASSEMBLIES.", ref showHelp);
			if (extra == null)
				return;
			if (path == null)
				Error ("Missing required parameter --out.");
			if (extra.Count == 0)
				Error ("No assemblies specified.");
			var command = new List<string> ();
			command.AddRange (extra.Select (a => "-assembly:" + a));
			if (delete)
				command.Add ("-delete");
			command.Add ("-overrides");
			command.Add ("-pretty");
			if (file != null) {
				command.Add ("-import:" + file);
			}
			if (path != null)
				command.Add ("-path:" + path);
			command.AddRange (extra);
			Updater.Main (command.ToArray ());
		}

		private void Validate (List<string> args)
		{
			string[] validFormats = {
				"ecma",
			};
			string format = "ecma";
			bool showHelp = false;
			var p = new OptionSet () {
				{ "f|format=",
					"The documentation {0:FORMAT} used within PATHS.  " + 
						"Valid formats include:\n  " +
						string.Join ("\n  ", validFormats) + "\n" +
						"If no format provided, `ecma' is used.",
					v => format = v },
				{ "h|?|help",
					"Show this message and exit.",
					v => showHelp = v != null },
			};
			List<string> extra = Parse (p, args, "validate", 
					"[OPTIONS]+ PATHS",
					"Validate PATHS against the specified format schema.", ref showHelp);
			if (extra == null)
				return;
			if (Array.IndexOf (validFormats, format) < 0)
				Error ("Invalid documentation format: {0}.", format);
			var command = new List<string> ();
			command.Add (format);
			command.AddRange (extra);
			Validater.Main (command.ToArray ());
		}
	}
}

