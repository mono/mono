using System;
using System.Collections.Generic;
using System.Linq;

namespace Mono.Debugger.Soft
{
	public sealed class BreakpointEventRequest : EventRequest {

		MethodMirror method;
		long location;
		
		internal BreakpointEventRequest (VirtualMachine vm, MethodMirror method, long location) : base (vm, EventType.Breakpoint) {
			if (method == null)
				throw new ArgumentNullException ("method");
			CheckMirror (vm, method);
			if (method.Locations.Count > 0 && !method.Locations.Any (l => l.ILOffset == location))
				throw new ArgumentException ("A breakpoint can only be set at an IL offset which is equal to the ILOffset property of one of the locations in method.Locations", "location");
			this.method = method;
			this.location = location;
		}

		public override void Enable () {
			var mods = new List <Modifier> ();
			mods.Add (new LocationModifier () { Method = method.Id, Location = location });
			SendReq (mods);
		}
	}
}