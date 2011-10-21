using System;
using System.Collections.Generic;
using System.Linq;

namespace Mono.Debugger.Soft
{
	public sealed class TypeLoadEventRequest : EventRequest {
		string[] sourceFiles;

		internal TypeLoadEventRequest (VirtualMachine vm) : base (vm, EventType.TypeLoad) {
		}

		public string[] SourceFileFilter {
			get {
				return sourceFiles;
			}
			set {
				CheckDisabled ();
				sourceFiles = value;
			}
		}

		public override void Enable () {
			var mods = new List <Modifier> ();
			if (SourceFileFilter != null && SourceFileFilter.Length != 0)
				mods.Add (new SourceFileModifier () { SourceFiles = SourceFileFilter });
			SendReq (mods);
		}
	}
}