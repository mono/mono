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

		public OciErrorHandle ErrorHandle {
			get { return errorHandle; }
			set { errorHandle = value; }
		}

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


		public bool Begin (OciCredentialType credentialType, OciSessionMode mode)
		{
			int status = 0;

			status = OciGlue.OCIAttrSetString (Handle,
						OciHandleType.Session,
						username,
						(uint) username.Length,
						OciAttributeType.Username,
						errorHandle.Handle);

			if (status != 0) 
				return false;

			status = OciGlue.OCIAttrSetString (Handle,
						OciHandleType.Session,
						password,
						(uint) password.Length,
						OciAttributeType.Password,
						errorHandle.Handle);

			if (status != 0) 
				return false;

			status = OCISessionBegin (Service.Handle,
						errorHandle.Handle,
						Handle,
						credentialType,
						mode);
			if (status != 0) 
				return false;

			begun = true;
			
			return true;
		}

		protected override void Dispose (bool disposing)
		{
			if (!disposed) {
				try {
					if (begun) {
						OCISessionEnd (Service.Handle,
								errorHandle.Handle,
								Handle,
								0);
					}
					disposed = false;
				} finally {
					base.Dispose (disposing);
				}
			}
		}

		#endregion // Methods
	}
}
