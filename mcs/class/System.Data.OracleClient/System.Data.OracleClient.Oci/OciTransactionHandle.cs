// 
// OciTransactionHandle.cs 
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
	internal sealed class OciTransactionHandle : OciHandle, IOciHandle, IDisposable
	{
		#region Fields
		
		OciErrorHandle errorHandle;
		OciServiceHandle serviceHandle;

		#endregion // Fields

		#region Constructors

		public OciTransactionHandle (OciEnvironmentHandle environment, IntPtr handle)
			: base (OciHandleType.Transaction, environment, handle)
		{
		}

		#endregion // Constructors

		#region Properties

		public OciErrorHandle ErrorHandle {
			get { return errorHandle; }
			set { errorHandle = value; }
		}

		public OciServiceHandle Service {
			get { return serviceHandle; }
			set { serviceHandle = value; }
		}

		#endregion // Properties

		#region Methods

		[DllImport ("oci")]
		public static extern int OCITransStart (IntPtr svchp,
							IntPtr errhp,
							uint timeout,
							[MarshalAs (UnmanagedType.U4)] OciTransactionFlags flags);

		public void Begin ()
		{
			int status = 0;
			status = OciGlue.OCIAttrSet (Service.Handle,
							OciHandleType.Service,
							Handle,
							0,
							OciAttributeType.Transaction,
							ErrorHandle.Handle);
			if (status != 0) {
				OciErrorInfo info = ErrorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}

			status = OCITransStart (Service.Handle,
							ErrorHandle.Handle,
							60,
							OciTransactionFlags.New);

			if (status != 0) {
				OciErrorInfo info = ErrorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}
		}

		public void Commit ()
		{
		}

		public void Dispose ()
		{
			Environment.FreeHandle (this);
		}

		public void Rollback ()
		{
		}

		#endregion // Methods
	}
}
