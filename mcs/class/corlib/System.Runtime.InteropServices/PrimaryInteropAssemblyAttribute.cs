//
// System.Runtime.InteropServices.PrimaryInteropAssemblyAttribute.cs
//
// Name: Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

using System;

namespace System.Runtime.InteropServices {

	[AttributeUsage (AttributeTargets.Assembly)]
	public sealed class PrimaryInteropAssemblyAttribute : Attribute
	{
		int major, minor;
		
		public PrimaryInteropAssemblyAttribute (int major, int minor)
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
