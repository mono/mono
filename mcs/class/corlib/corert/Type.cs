using System.Reflection;

namespace System {
	partial class Type {
		public static Type GetTypeFromCLSID (Guid clsid, string server, bool throwOnError) => RuntimeType.GetTypeFromCLSIDImpl (clsid, server, throwOnError);
		public static Type GetTypeFromProgID (string progID, string server, bool throwOnError) => RuntimeType.GetTypeFromProgIDImpl (progID, server, throwOnError);

		internal const string DefaultTypeNameWhenMissingMetadata = "UnknownType";		
		
		internal string FullNameOrDefault {
			get {
				// First, see if Type.Name is available. If Type.Name is available, then we can be reasonably confident that it is safe to call Type.FullName.
				// We'll still wrap the call in a try-catch as a failsafe.
				if (InternalNameIfAvailable == null)
					return DefaultTypeNameWhenMissingMetadata;
				try {
					return FullName;
				} catch (MissingMetadataException) {
					return DefaultTypeNameWhenMissingMetadata;
				}
			}
		}

		internal bool IsRuntimeImplemented () => this.UnderlyingSystemType is RuntimeType;

		internal virtual string InternalGetNameIfAvailable (ref Type rootCauseForFailure) => Name;
		
		internal string InternalNameIfAvailable {
			get {
				Type ignore = null;
				return InternalGetNameIfAvailable (ref ignore);
			}
		}
		
 		internal string NameOrDefault {
			get  {
				return InternalNameIfAvailable ?? DefaultTypeNameWhenMissingMetadata;
			}
		}
	}
}