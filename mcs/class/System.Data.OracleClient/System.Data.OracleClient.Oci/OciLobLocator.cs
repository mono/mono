// 
// OciLobLocator.cs 
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
	internal sealed class OciLobLocator : OciDescriptorHandle, IOciDescriptorHandle, IDisposable
	{
		#region Constructors

		public OciLobLocator (OciEnvironmentHandle environment, IntPtr handle)
			: base (OciDescriptorType.LobLocator, environment, handle)
		{
		}

		#endregion // Constructors

		#region Methods

		public void Dispose ()
		{
			Environment.FreeDescriptor (this);
		}

		#endregion // Methods
	}
}
