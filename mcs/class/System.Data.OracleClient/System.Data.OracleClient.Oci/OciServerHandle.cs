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
		string tnsname;

		#endregion // Fields

		#region Constructors

		public OciServerHandle (OciHandle parent, IntPtr handle)
			: base (OciHandleType.Server, parent, handle)
		{
		}

		#endregion // Constructors

		#region Properties

		public OciErrorHandle ErrorHandle {
			get { return errorHandle; }
			set { errorHandle = value; }
		}

		public string TNSName {
			get { return tnsname; }
			set { tnsname = value; }
		}

		#endregion // Properties

		#region Methods

		[DllImport ("oci")]
		public static extern int OCIServerAttach (IntPtr srvhp,
							IntPtr errhp,
							string dblink,
							[MarshalAs (UnmanagedType.U4)] int dblink_len,
							uint mode);

		[DllImport ("oci")]
		public static extern int OCIServerDetach (IntPtr srvhp,
							IntPtr errhp,
							uint mode);

		public bool Attach ()
		{
			int status = 0;
			status = OCIServerAttach (Handle,
						errorHandle.Handle,
						tnsname,
						tnsname.Length,
						0);
			attached = (status == 0);
			return attached;
		}

		protected override void Dispose (bool disposing)
		{
			if (!disposed) {
				try {
					if (attached)
						OCIServerDetach (Handle, errorHandle, 0);
					disposed = true;
				} finally {
					base.Dispose (disposing);
				}
			}
		}

		#endregion // Methods
	}
}
