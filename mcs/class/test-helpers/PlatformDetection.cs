namespace System
{
	static partial class PlatformDetection
	{
		public static readonly bool IsNetNative = false;
		public static readonly bool IsNotWinRT = true;
		public static readonly bool IsWinRT = false;
		public static readonly bool IsWindowsNanoServer = false;
		public static readonly bool IsNotWindowsNanoServer = true;
		public static readonly bool IsNotWindowsServerCore = true;

		public static bool IsWindows7 => false;
		public static bool IsFullFramework => false;
		public static bool IsNonZeroLowerBoundArraySupported => true;
		public static bool IsUap => false;

		//TODO: check?
		public static bool IsNotWindowsSubsystemForLinux => true;
		public static bool IsWindowsSubsystemForLinux => false;
		public static bool IsFedora => false;
		public static bool IsRedHatFamily => false;
		public static bool IsOpenSUSE => false;
		public static bool IsUbuntu1404 => false;

		public static bool IsWindows {
			get {
				PlatformID id = Environment.OSVersion.Platform;
				return id == PlatformID.Win32Windows || id == PlatformID.Win32NT;
			}
		}
		public static bool IsInAppContainer => false;
	}
}