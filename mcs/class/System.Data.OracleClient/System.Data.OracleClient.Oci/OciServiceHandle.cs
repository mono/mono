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
//     Daniel Morgan <monodanmorg@yahoo.com>
//         
// Copyright (C) Tim Coleman, 2003
// Copyright (C) Daniel Morgan, 2009
// 

//#define ORACLE_DATA_ACCESS

using System;
using System.Runtime.InteropServices;

namespace System.Data.OracleClient.Oci {
	internal sealed class OciServiceHandle : OciHandle
	{
		#region Fields

		bool disposed = false;
		OciSessionHandle session;
		OciServerHandle server;

		OciErrorHandle errorHandle;

#if ORACLE_DATA_ACCESS
		static readonly uint OCI_AUTH = 8;
#endif

		#endregion // Fields

		#region Constructors

		public OciServiceHandle (OciHandle parent, IntPtr handle)
			: base (OciHandleType.Service, parent, handle)
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

		protected override void Dispose (bool disposing)
		{
			if (!disposed) {
				try {
					if (disposing) {
						//if (server != null)
						//	server.Dispose ();
						//if (session != null)
						//	session.Dispose ();
					}
					disposed = true;
				} finally {
					base.Dispose (disposing);
				}
			}
		}

		public bool SetServer (OciServerHandle handle)
		{
			server = handle;
			int status = OciCalls.OCIAttrSet (this,
							HandleType,
							server,
							0,
							OciAttributeType.Server,
							ErrorHandle);
			return (status == 0);
		}

		public bool SetSession (OciSessionHandle handle)
		{
			session = handle;
			int status = OciCalls.OCIAttrSet (this,
							HandleType,
							session,
							0,
							OciAttributeType.Session,
							ErrorHandle);
			return (status == 0);
		}

#if ORACLE_DATA_ACCESS
		byte[] UnicodeToCharSet (string s)
		{
			int rsize = 0;
			byte [] buffer;
			
			// Get size of buffer
			OciCalls.OCIUnicodeToCharSet (Parent, null, s, out rsize);
			
			// Fill buffer
			buffer = new byte[rsize];
			OciCalls.OCIUnicodeToCharSet (Parent, buffer, s, out rsize);

			return buffer;
		}

		internal bool ChangePassword (string new_password, OciErrorHandle error) 
		{			
			if (!session.SetCredentialAttributes (error))
				return false;

			byte[] ub = UnicodeToCharSet (session.Username
			byte[] opb = UnicodeToCharSet (session.Password);
			byte[] npb = UnicodeToCharSet (new_password);

			int status = OciCalls.OCIPasswordChange (this, error, ub, ub.Length, opb, opb.Length, npb, npb.Length, OCI_AUTH);
			
			if (status == 0) {
				session.Password = new_password;
				return true;
			}
				
			return false;
		}
#endif

		#endregion // Methods
	}
}

