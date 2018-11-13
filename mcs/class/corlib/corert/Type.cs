using System.Reflection;

namespace System {
	partial class Type {
		public static Type GetTypeFromCLSID (Guid clsid, string server, bool throwOnError) => RuntimeType.GetTypeFromCLSIDImpl (clsid, server, throwOnError);
		public static Type GetTypeFromProgID (string progID, string server, bool throwOnError) => RuntimeType.GetTypeFromProgID (progID, server, throwOnError);

		internal bool IsRuntimeImplemented () => this.UnderlyingSystemType is RuntimeType;
	}
}
