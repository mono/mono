using System;
namespace WebAssembly.Host {
	public class HostObject : HostObjectBase {
		public HostObject (string hostName, params object[] _params) : base (Runtime.New(hostName, _params))  
		{ }
	}
}
