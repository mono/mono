using System;
using Mono.CodeContracts.Static;
using Mono.Options;

namespace cccheck {
	internal class Program {
		private static void Main (string[] args)
		{
			var options = new CheckOptions ();
			bool showOptions = false;
			string showMsg = null;

			var optionSet = new OptionSet {
			                              	{"help", "Show this help.", v => showOptions = v != null},
			                              	{"assembly=", "Assembly to check.", v => options.Assembly = v},
			                              	{"method=", "Method name (if you want to check only it).", v => options.Method = v},
							{"debug=", "Show debug information", v=> options.ShowDebug = v != null}
			                              };

			try {
				optionSet.Parse (args);
			} catch (OptionException e) {
				showOptions = true;
				showMsg = e.Message;
			}

			if (showOptions) {
				Console.WriteLine ("cccheck");
				Console.WriteLine ();
				Console.WriteLine ("Options:");
				optionSet.WriteOptionDescriptions (Console.Out);
				Console.WriteLine ();
				if (showMsg != null) {
					Console.WriteLine (showMsg);
					Console.WriteLine ();
				}
				return;
			}

			CheckResults results = Checker.Check (options);
			Console.WriteLine ();
			if (results.AnyErrors) {
				foreach (string error in results.Errors)
					Console.WriteLine ("Error: " + error);
			}

			if (results.AnyWarnings) {
				foreach (string warning in results.Warnings)
					Console.WriteLine ("Warning: " + warning);
			}

			if (results.Results != null) {
				foreach (var methodValidationResults in results.Results) {
					string methodName = methodValidationResults.Key;
					Console.WriteLine ("Method: " + methodName);
					foreach (string result in methodValidationResults.Value)
						Console.WriteLine ("  " + result);
					Console.WriteLine ();
				}
			}

			Console.WriteLine ();
			Console.WriteLine ("*** done ***");
		}
	}
}
