using System;
using System.Linq;
using System.Collections.Generic;
using Mono.Options;

class Test {
	public static void Main (string[] args)
	{
		var show_help = false;
		var macros = new Dictionary<string, string>();
		bool create = false, extract = false, list = false;
		string output = null, input = null;
		string color  = null;

		var p = new OptionSet () {
			// gcc-like options
			{ "D:", "Predefine a macro with an (optional) value.",
				(m, v) => {
					if (m == null)
						throw new OptionException ("Missing macro name for option -D.", 
								"-D");
					macros.Add (m, v);
				} },
			{ "d={-->}{=>}", "Alternate macro syntax.", 
				(m, v) => macros.Add (m, v) },
			{ "o=", "Specify the output file", v => output = v },

			// tar-like options
			{ "f=", "The input file",   v => input = v },
			{ "x",  "Extract the file", v => extract = v != null },
			{ "c",  "Create the file",  v => create = v != null },
			{ "t",  "List the file",    v => list = v != null },

			// ls-like optional values
			{ "color:", "control whether and when color is used", 
				v => color = v },

			// other...
			{ "h|help",  "show this message and exit", 
			  v => show_help = v != null },
			// default
			{ "<>",
				v => Console.WriteLine ("def handler: color={0}; arg={1}", color, v)},
		};

		try {
			p.Parse (args);
		}
		catch (OptionException e) {
			Console.Write ("bundling: ");
			Console.WriteLine (e.Message);
			Console.WriteLine ("Try `greet --help' for more information.");
			return;
		}

		if (show_help) {
			ShowHelp (p);
			return;
		}

		Console.WriteLine ("Macros:");
		foreach (var m in (from k in macros.Keys orderby k select k)) {
			Console.WriteLine ("\t{0}={1}", m, macros [m] ?? "<null>");
		}
		Console.WriteLine ("Options:");
		Console.WriteLine ("\t Input File: {0}", input);
		Console.WriteLine ("\tOuptut File: {0}", output);
		Console.WriteLine ("\t     Create: {0}", create);
		Console.WriteLine ("\t    Extract: {0}", extract);
		Console.WriteLine ("\t       List: {0}", list);
		Console.WriteLine ("\t      Color: {0}", color ?? "<null>");
	}

	static void ShowHelp (OptionSet p)
	{
		Console.WriteLine ("Usage: bundling [OPTIONS]+");
		Console.WriteLine ("Demo program to show the effects of bundling options and their values");
		Console.WriteLine ();
		Console.WriteLine ("Options:");
		p.WriteOptionDescriptions (Console.Out);
	}
}

