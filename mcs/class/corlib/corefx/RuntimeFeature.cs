namespace System.Runtime.CompilerServices
{
	partial class RuntimeFeature
	{
		// https://github.com/dotnet/coreclr/blob/397aaccb5104844998c3bcf6e9245cc81127e1e2/src/System.Private.CoreLib/src/System/Runtime/CompilerServices/RuntimeFeature.CoreCLR.cs#L9-L10
#if WASM || (FULL_AOT_RUNTIME && !MONOTOUCH)
		public const bool IsDynamicCodeSupported = false;
		public const bool IsDynamicCodeCompiled = false;
#else
		public static bool IsDynamicCodeSupported => true;
		public static bool IsDynamicCodeCompiled => true;
#endif
	}
}
