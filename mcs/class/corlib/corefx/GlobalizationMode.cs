namespace System.Globalization
{
	partial class GlobalizationMode
	{
#if DISABLE_GLOBALIZATION
		internal const bool Invariant = true;
#else
		internal static bool Invariant { get; } = GetGlobalizationInvariantMode();

		static bool GetGlobalizationInvariantMode () => false;
#endif
	}
}
