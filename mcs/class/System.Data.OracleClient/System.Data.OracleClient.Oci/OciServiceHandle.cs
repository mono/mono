// 
// OciServiceHandle.cs 
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
	internal sealed class OciServiceHandle : OciHandle, IOciHandle, IDisposable
	{
		#region Fields

		OciSessionHandle sessionHandle;
		OciServerHandle serverHandle;
		OciErrorHandle errorHandle;

		#endregion // Fields

		#region Constructors

		public OciServiceHandle (OciEnvironmentHandle environment, IntPtr handle)
			: base (OciHandleType.Service, environment, handle)
		{
		}

		#endregion // Constructors

		#region Properties

		public OciErrorHandle ErrorHandle {
			get { return errorHandle; }
			set { errorHandle = value; }
		}

		#endregion // Properties

		#region Methods

		public void Dispose ()
		{
			Environment.FreeHandle (this);
		}

		public bool SetServer (OciServerHandle handle)
		{
			serverHandle = handle;
			int status = 0;
			status = OciGlue.OCIAttrSet (Handle,
						HandleType,
						serverHandle.Handle,
						0,
						OciAttributeType.Server,
						errorHandle.Handle);
			return (status == 0);
		}

		public bool SetSession (OciSessionHandle handle)
		{
			sessionHandle = handle;
			int status = 0;
			status = OciGlue.OCIAttrSet (Handle,
						HandleType,
						sessionHandle.Handle,
						0,
						OciAttributeType.Session,
						errorHandle.Handle);
			return (status == 0);
		}

		#endregion // Methods
	}
}
