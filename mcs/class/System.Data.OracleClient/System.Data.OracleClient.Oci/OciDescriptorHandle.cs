// 
// OciDescriptorHandle.cs 
//  
// Part of managed C#/.NET library System.Data.OracleClient.dll
//
// Part of the Mono class libraries at
// mcs/class/System.Data.OracleClient/System.Data.OracleClient.Oci
//
// Assembly: System.Data.OracleClient.dll
// Namespace: System.Data.OracleClient.Oci
// 
// Author: 
//     Tim Coleman <tim@timcoleman.com>
//         
// Copyright (C) Tim Coleman, 2003
// 

using System;
using System.Runtime.InteropServices;

namespace System.Data.OracleClient.Oci {
	internal abstract class OciDescriptorHandle : OciHandle
	{
		#region Constructors

		public OciDescriptorHandle (OciHandleType type, OciHandle parent, IntPtr newHandle)
			: base (type, parent, newHandle)
		{
		}

		#endregion // Constructors

		#region Methods

		[DllImport ("oci")]
		static extern int OCIDescriptorFree (IntPtr hndlp,
						[MarshalAs (UnmanagedType.U4)] OciHandleType type);

		protected override void FreeHandle ()
		{
			int status = 0;
			status = OCIDescriptorFree (this, HandleType);
		}

		#endregion // Methods
	}
}
