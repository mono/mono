using System;
using System.Collections.Generic;
using System.Linq;

namespace Mono.Debugger.Soft
{
	public sealed class AssemblyLoadEventRequest : EventRequest {		
		internal AssemblyLoadEventRequest (VirtualMachine vm) : base (vm, EventType.AssemblyLoad) {
		}
	}
}
