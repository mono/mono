using System;

namespace Mono.Debugger.Soft
{
	/*
	 * A Mirror represents a runtime object in the remote virtual machine. Calling
	 * methods/properties of mirror objects potentially involves a remoting call, 
	 * which
	 * has some overhead, and may also fail. Values of properties which are 
	 * constant (like Type.Name) are cached locally, so only the first call is 
	 * affected.
	 * FIXME: Thread safety in the api ? 
	 */
	public interface IMirror
	{
		VirtualMachine VirtualMachine {
			get;
		}
	}
}
