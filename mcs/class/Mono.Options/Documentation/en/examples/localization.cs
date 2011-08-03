// Localization with NDesk.Options.OptionSet.
//
// Compile as:
//   gmcs -r:Mono.Posix.dll -r:NDesk.Options.dll code-localization.cs
using System;
using System.IO;
using Mono.Options;
using Mono.Unix;

class LocalizationDemo {
	public static void Main (string[] args)
	{
		bool with_gettext = false;
		string useLocalizer = null;
		var p = new OptionSet () {
			{ "with-gettext", v => { useLocalizer = "gettext"; } },
			{ "with-hello",   v => { useLocalizer = "hello"; } },
			{ "with-default", v => { /* do nothing */ } },
		};
		p.Parse (args);

		Converter<string, string> localizer = f => f;
		switch (useLocalizer) {
			case "gettext":
				Catalog.Init ("localization", 
						Path.Combine (AppDomain.CurrentDomain.BaseDirectory,
							"locale"));
				localizer = f => { return Catalog.GetString (f); };
				break;
			case "hello":
				localizer = f => { return "hello:" + f; };;
				break;
		}

		bool help = false;
		int verbose = 0;
		bool version = false;
		p = new OptionSet (localizer) {
			{ "h|?|help", "show this message and exit.", 
				v => help = v != null },
			{ "v|verbose", "increase message verbosity.",
				v => { ++verbose; } },
			{ "n=", "must be an int",
				(int n) => { /* ignore */ } },
			{ "V|version", "output version information and exit.",
				v => version = v != null },
		};
		try {
			p.Parse (args);
		}
		catch (OptionException e) {
			Console.Write ("localization: ");
			Console.WriteLine (e.Message);
			return;
		}
		if (help)
			p.WriteOptionDescriptions (Console.Out);
		if (version)
			Console.WriteLine ("NDesk.Options Localizer Demo 1.0");
		if (verbose > 0)
			Console.WriteLine ("Message level: {0}", verbose);
	}
}
