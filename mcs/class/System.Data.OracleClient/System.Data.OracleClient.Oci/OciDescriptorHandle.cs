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

		internal OciDescriptorHandle (OciHandleType type, OciHandle parent, IntPtr newHandle)
			: base (type, parent, newHandle)
		{
		}

		#endregion // Constructors

		#region Methods

		protected override void FreeHandle () 
		{
			// Parameter handles are disposed implicitely
			if (HandleType >= OciHandleType.LobLocator) {
				switch(HandleType) {
				case OciHandleType.Parameter:
				case OciHandleType.TimeStamp:
					break;
				default:
					if (Handle != IntPtr.Zero) {
						OciCalls.OCIDescriptorFree (this, HandleType);
						SetHandle (IntPtr.Zero);
					}
					break;
				}
			}
		}

		#endregion // Methods
	}
}

