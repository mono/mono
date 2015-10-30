// Command to preserve member documentation for types that are changing in a subsequent version
// By Joel Martinez <joel.martinez@xamarin.com
using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Options;

namespace Mono.Documentation
{
	[Obsolete ("This functionality is no longer supported.")]
	public class MDocPreserve : MDocCommand
	{
		MDocUpdater updater;

		public MDocPreserve ()
		{
			updater = new MDocUpdater ();
		}

		public override void Run (IEnumerable<string> args)
		{
			Message (System.Diagnostics.TraceLevel.Warning, "This functionality is no longer supported, and will be removed in a future release.");

			string preserveName = string.Empty;
			var p = new OptionSet () { { "name=",
					"Root {DIRECTORY} to generate/update documentation.",
					v => preserveName = v
				}
			};

			updater.PreserveTag = preserveName;

			updater.Run (args);
		}
	}
}

