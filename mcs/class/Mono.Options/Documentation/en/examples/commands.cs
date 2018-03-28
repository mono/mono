// Sub-commands with Mono.Options.CommandSet
//
// Compile as:
//   mcs -r:Mono.Options.dll commands.cs

using System;
using System.Collections.Generic;

using Mono.Options;

class CommandDemo {
	public static int Main (string[] args)
	{
		var commands = new CommandSet ("commands") {
			"usage: commands COMMAND [OPTIONS]",
			"",
			"Mono.Options.CommandSet sample app.",
			"",
			"Global options:",
			{ "v:",
			  "Output verbosity.",
			  (int? n) => Verbosity = n.HasValue ? n.Value : Verbosity + 1 },
			"",
			"Available commands:",
			new Command ("echo", "Echo arguments to the screen") {
				Run = ca => Console.WriteLine ("{0}", string.Join (" ", ca)),
			},
			new RequiresArgs (),
		};
		return commands.Run (args);
	}

	public static int Verbosity;
}

class RequiresArgs : Command {

	public RequiresArgs ()
		: base ("requires-args", "Class-based Command subclass")
	{
		Options = new OptionSet () {
			"usage: commands requires-args [OPTIONS]",
			"",
			"Class-based Command subclass example.",
			{ "name|n=",
			  "{name} of person to greet.",
			  v => Name = v },
			{ "help|h|?",
			  "Show this message and exit.",
			  v => ShowHelp = v != null },
		};
	}

	public        bool    ShowHelp    {get; private set;}
	public  new   string  Name        {get; private set;}

	public override int Invoke (IEnumerable<string> args)
	{
		try {
			var extra = Options.Parse (args);
			if (ShowHelp) {
				Options.WriteOptionDescriptions (CommandSet.Out);
				return 0;
			}
			if (string.IsNullOrEmpty (Name)) {
				Console.Error.WriteLine ("commands: Missing required argument `--name=NAME`.");
				Console.Error.WriteLine ("commands: Use `commands help requires-args` for details.");
				return 1;
			}
			Console.WriteLine ($"Hello, {Name}!");
			return 0;
		}
		catch (Exception e) {
			Console.Error.WriteLine ("commands: {0}", CommandDemo.Verbosity >= 1 ? e.ToString () : e.Message);
			return 1;
		}
	}
}
