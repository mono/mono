using System;

namespace ByteFX.Data.Common
{
	/// <summary>
	/// Summary description for Version.
	/// </summary>
	internal struct Version
	{
		private int	major;
		private int minor;
		private int build;

		public Version( int major, int minor, int build)
		{
			this.major = major;
			this.minor = minor;
			this.build = build;
		}

		public static Version Parse( string versionString )
		{
			int start = 0;
			int index = versionString.IndexOf('.', start);
			if (index == -1) throw new Exception("Version string not in acceptable format");
			int major = Convert.ToInt32( versionString.Substring(start, index-start).Trim());

			start = index+1;
			index = versionString.IndexOf('.', start);
			if (index == -1) throw new Exception("Version string not in acceptable format");
			int minor = Convert.ToInt32( versionString.Substring(start, index-start).Trim());

			start = index+1;
			int i = start;
			while (i < versionString.Length && Char.IsDigit( versionString, i ))
				i++;
			int build = Convert.ToInt32( versionString.Substring(start, i-start).Trim());

			return new Version( major, minor, build );
		}

		public bool isAtLeast(int major, int minor, int build)
		{
			if (major > this.major) return false;
			if (minor > this.minor) return false;
			if (build > this.build) return false;
			return true;
		}

	}
}
