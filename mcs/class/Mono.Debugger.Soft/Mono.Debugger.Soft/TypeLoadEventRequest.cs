using System;
using System.Collections.Generic;
using System.Linq;

namespace Mono.Debugger.Soft
{
	public sealed class TypeLoadEventRequest : EventRequest {
		string[] sourceFiles;
		string[] typeNames;

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

		public string[] TypeNameFilter {
			get {
				return typeNames;
			}
			set {
				CheckDisabled ();
				typeNames = value;
			}
		}

		public override void Enable () {
			var mods = new List <Modifier> ();
			if (SourceFileFilter != null && SourceFileFilter.Length != 0)
				mods.Add (new SourceFileModifier () { SourceFiles = SourceFileFilter });
			if (TypeNameFilter != null && TypeNameFilter.Length != 0)
				mods.Add (new TypeNameModifier () { TypeNames = TypeNameFilter });
			SendReq (mods);
		}
	}
}