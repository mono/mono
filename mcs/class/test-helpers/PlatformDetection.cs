namespace System
{
	static partial class PlatformDetection
	{
		public static readonly bool IsNetNative = false;

		public static readonly bool IsFullFramework = true;

		public static bool IsNonZeroLowerBoundArraySupported => true;
	}
}