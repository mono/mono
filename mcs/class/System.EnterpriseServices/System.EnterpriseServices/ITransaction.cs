// 
// System.EnterpriseServices.ITransaction.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Runtime.InteropServices;

namespace System.EnterpriseServices {
	//[Guid ("")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	public interface ITransaction {

		#region Methods

		void Abort (ref BOID pboidReason, int fRetaining, int fAsync);
		void Commit (int fRetaining, int grfTC, int grfRM);
		void GetTransactionInfo (out XACTTRANSINFO pinfo);

		#endregion // Methods
	}
}
