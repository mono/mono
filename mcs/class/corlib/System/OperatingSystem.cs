//
// System.OperatingSystem.cs 
//
// Author:
//   Jim Richardson (develop@wtfo-guru.com)
//
// (C) 2001 Moonlight Enterprises, All Rights Reserved
//

namespace System
{
	/// <summary>
	/// Class representing a specific operating system version for a specific platform
	/// </summary>
	[Serializable]
	public sealed class OperatingSystem : ICloneable
	{
		private System.PlatformID itsPlatform;
		private Version itsVersion;

		public OperatingSystem (PlatformID platform, Version version)
		{
			if (version == null) {
				throw new ArgumentNullException ("version");
			}

			itsPlatform = platform;
			itsVersion = version;
		}

		public PlatformID Platform {
			get {
				return itsPlatform;
			}
		}

		public Version Version {
			get {
				return itsVersion;
			}
		}

		public object Clone ()
		{
			return new OperatingSystem (itsPlatform, itsVersion);
		}

		public override string ToString ()
		{
			string str;

			switch ((int) itsPlatform) {
			case (int) System.PlatformID.Win32NT:
				str = "Microsoft Windows NT";
				break;
			case (int) System.PlatformID.Win32S:
				str = "Microsoft Win32S";
				break;
			case (int) System.PlatformID.Win32Windows:
				str = "Microsoft Windows 98";
				break;
#if NET_1_1
			case (int) System.PlatformID.WinCE:
				str = "Microsoft Windows CE";
				break;
#endif
			case 128 /* PlatformID.Unix */:
				str = "Unix";
				break;
			default:
				str = Locale.GetText ("<unknown>");
				break;
			}

			return str + " " + itsVersion.ToString();
		}
	}
}
