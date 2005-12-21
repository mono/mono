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
	internal sealed class OciServiceHandle : OciHandle
	{
		#region Fields

		bool disposed = false;
		OciSessionHandle session;
		OciServerHandle server;

		OciErrorHandle errorHandle;

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

		#endregion // Methods
	}
}
