// 
// OciSessionHandle.cs 
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
	internal sealed class OciSessionHandle : OciHandle, IDisposable
	{
		#region Fields

		OciErrorHandle errorHandle;
		OciServiceHandle serviceHandle;
		bool begun = false;
		bool disposed = false;
		string username;
		string password;

		#endregion // Fields

		#region Constructors

		public OciSessionHandle (OciHandle parent, IntPtr handle)
			: base (OciHandleType.Session, parent, handle)
		{
		}

		#endregion // Constructors

		#region Properties

		public OciServiceHandle Service {
			get { return serviceHandle; }
			set { serviceHandle = value; }
		}

		public string Username {
			get { return username; }
			set { username = value; }
		}

		public string Password {
			get { return password; }
			set { password = value; }
		}

		#endregion // Properties

		#region Methods

		[DllImport ("oci")]
		public static extern int OCISessionBegin (IntPtr svchp,
							IntPtr errhp,
							IntPtr usrhp,
							[MarshalAs (UnmanagedType.U4)] OciCredentialType credt,
							[MarshalAs (UnmanagedType.U4)] OciSessionMode mode);

		[DllImport ("oci")]
		public static extern int OCISessionEnd (IntPtr svchp,
							IntPtr errhp,
							IntPtr usrhp,
							uint mode);


		public bool BeginSession (OciCredentialType credentialType, OciSessionMode mode, OciErrorHandle error)
		{
			errorHandle = error;

			int status = 0;

			status = OciGlue.OCIAttrSetString (this,
							OciHandleType.Session,
							username,
							(uint) username.Length,
							OciAttributeType.Username,
							errorHandle);

			if (status != 0) 
				return false;

			status = OciGlue.OCIAttrSetString (this,
							OciHandleType.Session,
							password,
							(uint) password.Length,
							OciAttributeType.Password,
							errorHandle);

			if (status != 0) 
				return false;

			status = OCISessionBegin (Service,
						errorHandle,
						Handle,
						credentialType,
						mode);

			if (status != 0) 
				return false;

			begun = true;
			
			return true;
		}

		public void EndSession (OciErrorHandle error)
		{
			if (!begun)
				return;
			OCISessionEnd (Service, error, this, 0);
		}

		protected override void Dispose (bool disposing)
		{
			if (!disposed) {
				try {
					EndSession (errorHandle);
					disposed = false;
				} finally {
					base.Dispose (disposing);
				}
			}
		}

		#endregion // Methods
	}
}
