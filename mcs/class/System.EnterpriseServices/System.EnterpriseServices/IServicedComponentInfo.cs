// 
// System.EnterpriseServices.IServicedComponentInfo.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Runtime.InteropServices;

namespace System.EnterpriseServices {
	[Guid ("")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	public interface IServicedComponentInfo {

		#region Methods

		void GetComponentInfo (ref int infoMask, out string[] infoArray);

		#endregion // Methods
	}
}
