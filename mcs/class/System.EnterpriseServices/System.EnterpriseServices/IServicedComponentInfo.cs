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
	[Guid("8165B19E-8D3A-4d0b-80C8-97DE310DB583")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	public interface IServicedComponentInfo {

		#region Methods

		void GetComponentInfo (ref int infoMask, out string[] infoArray);

		#endregion // Methods
	}
}
