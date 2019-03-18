namespace System
{
	static partial class PlatformDetection
	{
		public static readonly bool IsNetNative = false;
		public static readonly bool IsNotWinRT = true;
		public static readonly bool IsWinRT = false;
		public static readonly bool IsWindowsNanoServer = false;
		public static bool IsNotWindowsNanoServer => true;
		public static bool IsNotWindowsServerCore => true;
		public static bool IsMono => true;

		public static bool IsWindows7 => false;
		public static bool IsWindows10Version1607OrGreater => false;
		public static bool IsWindows10Version1703OrGreater => false;
		public static bool IsWindows10Version1709OrGreater => false;
		public static bool IsFullFramework => false;
		public static bool IsNonZeroLowerBoundArraySupported => true;
		public static bool IsUap => false;

		//TODO: check?
		public static bool IsNotWindowsSubsystemForLinux => true;
		public static bool IsWindowsSubsystemForLinux => false;
		public static bool IsFedora => false;
		public static bool IsRedHatFamily => false;
		public static bool IsRedHatFamily6 => false;
		public static bool IsOpenSUSE => false;
		public static bool IsUbuntu1404 => false;
		public static bool IsNotRedHatFamily6 => true;
		public static bool IsMacOsHighSierraOrHigher => false;
		public static bool IsDebian8 => false;
		public static bool IsInvokingStaticConstructorsSupported => true;
		public static bool IsReflectionEmitSupported => true;

		public static bool IsNetfx462OrNewer => false;

		public static bool SupportsAlpn => false;
		public static bool SupportsClientAlpn => false;

		public static bool IsWindows {
			get {
				PlatformID id = Environment.OSVersion.Platform;
				return id == PlatformID.Win32Windows || id == PlatformID.Win32NT;
			}
		}
		public static bool IsInAppContainer => false;
		public static bool IsAlpine => false;
		public static bool IsNetCore => false;

		public static int WindowsVersion => -1;
    }
}
