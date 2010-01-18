using System;

namespace Mono.Debugger.Soft
{
	public class ObjectCollectedException : Exception {
		
		public ObjectCollectedException () : base ("The requested operation cannot be completed because the object has been garbage collected.") {
		}
	}
}
