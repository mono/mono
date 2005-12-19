// 
// OciServerHandle.cs 
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
	internal sealed class OciServerHandle : OciHandle, IDisposable
	{
		#region Fields

		bool disposed = false;
		bool attached = false;
		OciErrorHandle errorHandle;

		#endregion // Fields

		#region Constructors

		public OciServerHandle (OciHandle parent, IntPtr newHandle)
			: base (OciHandleType.Server, parent, newHandle)
		{
		}

		#endregion // Constructors

		#region Methods

		public bool Attach (string tnsname, OciErrorHandle error)
		{
			errorHandle = error;

			int status = OciCalls.OCIServerAttach (this, error, tnsname, tnsname.Length, 0);

			if (status != 0) {
				OciErrorInfo info = errorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}

			attached = true;
			return attached;
		}

		public void Detach (OciErrorHandle error)
		{
			if (!attached) 
				return;

			int status = OciCalls.OCIServerDetach (this, error, 0);

			if (status != 0) {
				OciErrorInfo info = errorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}

			attached = false;
		}

		protected override void Dispose (bool disposing)
		{
			if (!disposed) {
				try {
					//Detach (errorHandle);
					disposed = true;
				} finally {
					base.Dispose (disposing);
				}
			}
		}

		#endregion // Methods
	}
}
