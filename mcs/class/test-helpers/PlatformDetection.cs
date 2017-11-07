namespace System
{
	static partial class PlatformDetection
	{
		public static readonly bool IsNetNative = false;
		public static readonly bool IsNotWinRT = true;
		public static readonly bool IsWinRT = false;
		public static readonly bool IsFullFramework = true;
		public static readonly bool IsWindowsNanoServer = false;
		public static bool IsNonZeroLowerBoundArraySupported => true;
	}
}