// 
// OciErrorHandle.cs 
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
	internal sealed class OciErrorHandle : OciHandle, IOciHandle, IDisposable
	{
		#region Constructors

		public OciErrorHandle (OciEnvironmentHandle environment, IntPtr handle)
			: base (OciHandleType.Error, environment, handle)
		{
		}

		#endregion // Constructors

		#region Methods

		public void Dispose ()
		{
			Environment.FreeHandle (this);
		}

		public OciErrorInfo HandleError ()
		{
			OciErrorInfo info;
			info.ErrorCode = 0;
			info.ErrorMessage = String.Empty;

			int errbufSize = 512;
			IntPtr errbuf = Marshal.AllocHGlobal (errbufSize);

			OciGlue.OCIErrorGet (Handle, 
						1,
						IntPtr.Zero,
						out info.ErrorCode,
						errbuf,
						(uint) errbufSize,
						OciHandleType.Error);
	
			object err = Marshal.PtrToStringAnsi (errbuf);
			if (err != null) {
				string errmsg = (string) err;
				info.ErrorMessage = String.Copy (errmsg);
				Marshal.FreeHGlobal (errbuf);
			}

			return info;
		}

		#endregion // Methods
	}
}
