// 
// System.EnterpriseServices.XACTTRANSINFO.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Runtime.InteropServices;

namespace System.EnterpriseServices {

	[ComVisible(false)]
	public struct XACTTRANSINFO {

		#region Fields

		public int grfRMSupported;
		public int grfRMSupportedRetaining;
		public int grfTCSupported;
		public int grfTCSupportedRetaining;
		public int isoFlags;
		public int isoLevel;
		public BOID uow;

		#endregion // Fields
	}
}
