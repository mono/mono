using System;
using System.Collections.Generic;

namespace Mono.Debugger.Soft
{
	public class InvocationException : Exception {

		ObjectMirror exception;

		public InvocationException (ObjectMirror exception) {
			this.exception = exception;
		}

		public ObjectMirror Exception {
			get {
				return exception;
			}
		}
	}
}