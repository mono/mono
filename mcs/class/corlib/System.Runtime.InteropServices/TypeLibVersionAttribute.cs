//
// System.Runtime.InteropServices.TypeLibVersionAttribute.cs
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
	public sealed class TypeLibVersionAttribute : Attribute
	{
		private int major;
		private int minor;

		public TypeLibVersionAttribute (int major, int minor)
		{
			this.major = major;
			this.minor = minor;
		}

		public int MajorVersion {
			get { return major; }
		}

		public int MinorVersion {
			get { return minor; }
		}
	}
}

#endif