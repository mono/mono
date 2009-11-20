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
//     Daniel Morgan <monodanmorg@yahoo.com>
//
// Copyright (C) Tim Coleman, 2003
// Copyright (C) Daniel Morgan, 2009
//

//#define ORACLE_DATA_ACCESS

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
		//OciCredentialType credentialType;

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

		internal string Username {
			get { return username; }
			set { username = value; }
		}

		internal string Password {
			get { return String.Empty; }
			set { password = value; }
		}

		#endregion // Properties

		#region Methods

		internal bool SetCredentialAttributes (OciErrorHandle error)
		{
			errorHandle = error;

			int status;

			status = OciCalls.OCIAttrSetString (this,
				OciHandleType.Session,
				username,
				(uint) username.Length,
				OciAttributeType.Username,
				errorHandle);

			if (status != 0)
				return false;

			status = OciCalls.OCIAttrSetString (this,
				OciHandleType.Session,
				password,
				(uint) password.Length,
				OciAttributeType.Password,
				errorHandle);

			if (status != 0)
				return false;
			
			return true;
		}

		public bool BeginSession (OciCredentialType credentialType, OciSessionMode mode, OciErrorHandle error)
		{
			errorHandle = error;

			int status;

			if (credentialType == OciCredentialType.RDBMS) {
				if (!SetCredentialAttributes (errorHandle))
					return false;
			}

			status = OciCalls.OCISessionBegin (Service,
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
			OciCalls.OCISessionEnd (Service, error, this, 0);
			begun = false;
		}

		protected override void Dispose (bool disposing)
		{
			if (!disposed) {
				try {
					//EndSession (errorHandle);
					disposed = false;
				} finally {
					base.Dispose (disposing);
				}
			}
		}

		#endregion // Methods
	}
}
