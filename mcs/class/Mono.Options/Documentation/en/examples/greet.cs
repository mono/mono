using System;
using System.Collections.Generic;
using Mono.Options;

class Test {
	static int verbosity;

	public static void Main (string[] args)
	{
		bool show_help = false;
		List<string> names = new List<string> ();
		int repeat = 1;

		var p = new OptionSet () {
			"Usage: greet [OPTIONS]+ message",
			"Greet a list of individuals with an optional message.",
			"If no message is specified, a generic greeting is used.",
			"",
			"Options:",
			{ "n|name=", "the {NAME} of someone to greet.",
			  v => names.Add (v) },
			{ "r|repeat=", 
				"the number of {TIMES} to repeat the greeting.\n" + 
					"this must be an integer.",
			  (int v) => repeat = v },
			{ "v", "increase debug message verbosity",
			  v => { if (v != null) ++verbosity; } },
			{ "h|help",  "show this message and exit", 
			  v => show_help = v != null },
		};

		List<string> extra;
		try {
			extra = p.Parse (args);
		}
		catch (OptionException e) {
			Console.Write ("greet: ");
			Console.WriteLine (e.Message);
			Console.WriteLine ("Try `greet --help' for more information.");
			return;
		}

		if (show_help) {
			p.WriteOptionDescriptions (Console.Out);
			return;
		}

		string message;
		if (extra.Count > 0) {
			message = string.Join (" ", extra.ToArray ());
			Debug ("Using new message: {0}", message);
		}
		else {
			message = "Hello {0}!";
			Debug ("Using default message: {0}", message);
		}

		foreach (string name in names) {
			for (int i = 0; i < repeat; ++i)
				Console.WriteLine (message, name);
		}
	}

	static void Debug (string format, params object[] args)
	{
		if (verbosity > 0) {
			Console.Write ("# ");
			Console.WriteLine (format, args);
		}
	}
}

