//
// Mono Runtime gateway functions
//
//

using System;
using System.Runtime.CompilerServices;

namespace Mono {

	public class Runtime
	{
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private static extern void mono_runtime_install_handlers ();
		
		public void InstallSignalHandlers ()
		{
			mono_runtime_install_handlers ();
		}
	}
	
}
