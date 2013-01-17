using System;
using System.Collections.Generic;

using Monodoc;
using Mono.Options;

namespace Mono.Documentation {

	class MDocTreeDumper : MDocCommand {

		public override void Run (IEnumerable<string> args)
		{
			var validFormats = RootTree.GetSupportedFormats ();
			string cur_format = "";
			var formats = new Dictionary<string, List<string>> ();
			var options = new OptionSet () {
				{ "f|format=",
					"The documentation {FORMAT} used in FILES.  " + 
						"Valid formats include:\n  " +
						string.Join ("\n  ", validFormats) + "\n" +
						"If not specified, no HelpSource is used.  This may " +
						"impact the PublicUrls displayed for nodes.",
					v => {
						if (Array.IndexOf (validFormats, v) < 0)
							Error ("Invalid documentation format: {0}.", v);
						cur_format = v;
					} },
				{ "<>", v => AddFormat (formats, cur_format, v) },
			};
			List<string> files = Parse (options, args, "dump-tree", 
					"[OPTIONS]+ FILES",
					"Print out the nodes within the assembled .tree FILES,\n" + 
					"as produced by 'mdoc assemble'.");
			if (files == null)
				return;

			foreach (string format in formats.Keys) {
				foreach (string file in formats [format]) {
					HelpSource hs = format == ""
						? null
						: RootTree.GetHelpSource (format, file.Replace (".tree", ""));
					Tree t = new Tree (hs, file);
					TreeDumper.PrintTree (t.RootNode);
				}
			}
		}

		private void AddFormat (Dictionary<string, List<string>> d, string format, string file)
		{
			if (format == null)
				format = "";
			List<string> l;
			if (!d.TryGetValue (format, out l)) {
				l = new List<string> ();
				d.Add (format, l);
			}
			l.Add (file);
		}
	}
}
