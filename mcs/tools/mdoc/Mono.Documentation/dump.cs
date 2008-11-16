using System;
using System.Collections.Generic;

using Monodoc;
using Mono.Options;

namespace Mono.Documentation {

	class MDocTreeDumper : MDocCommand {

		public override void Run (IEnumerable<string> args)
		{
			var options = new OptionSet () {
			};
			List<string> files = Parse (options, args, "dump-tree", 
					"[OPTIONS]+ FILES",
					"Print out the nodes within the assembled .tree file FILES.");
			if (files == null)
				return;

			foreach (var file in files) {
				Tree t = new Tree (null, file);
				Node.PrintTree (t);
			}
		}
	}
}
