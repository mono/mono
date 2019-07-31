using System;
namespace WebAssembly {
	public class JSException : Exception {
		public JSException (string msg) : base (msg) { }
	}
}
