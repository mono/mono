using System;

namespace Mono.Debugger.Soft
{
	public class InvalidStackFrameException : Exception {
		
		public InvalidStackFrameException () : base ("The requested operation cannot be completed because the specified stack frame is no longer valid.") {
		}
	}
}
