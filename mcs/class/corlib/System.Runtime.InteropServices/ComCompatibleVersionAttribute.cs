//
// System.Runtime.InteropServices.ComCompatibleVersionAttribute.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Andreas Nahr
//

#if (NET_1_1)

namespace System.Runtime.InteropServices
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
	public sealed class ComCompatibleVersionAttribute : Attribute
	{
		private int major;
		private int minor;
		private int build;
		private int revision;

		public ComCompatibleVersionAttribute (int major, int minor, int build, int revision)
		{
			this.major = major;
			this.minor = minor;
			this.build = build;
			this.revision = revision;
		}

		public int MajorVersion {
			get { return major; }
		}

		public int MinorVersion {
			get { return minor; }
		}

		public int BuildNumber {
			get { return build; }
		}

		public int RevisionNumber {
			get { return revision; }
		}
	}
}

#endif