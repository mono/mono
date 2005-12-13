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
using System.Text;

namespace System.Data.OracleClient.Oci {
	internal sealed class OciErrorHandle : OciHandle, IDisposable
	{
		#region Fields

		bool disposed = false;

		#endregion // Fields

		#region Constructors

		public OciErrorHandle (OciHandle parent, IntPtr newHandle)
			: base (OciHandleType.Error, parent, newHandle)
		{
		}

		#endregion // Constructors

		#region Methods

		protected override void Dispose (bool disposing)
		{
			if (!disposed) {
				disposed = true;
				base.Dispose (disposing);
			}
		}

		public OciErrorInfo HandleError () 
		{
			OciErrorInfo info;
			info.ErrorCode = 0;
			info.ErrorMessage = String.Empty;

			int errbufSize = 4096;
			IntPtr errbuf = Marshal.AllocHGlobal (errbufSize);

			OciCalls.OCIErrorGet (this, 
				1,
				IntPtr.Zero,
				out info.ErrorCode,
				errbuf,
				(uint) errbufSize,
				OciHandleType.Error);

			//object err = Marshal.PtrToStringAuto (errbuf);
			byte[] bytea = new byte[errbufSize];
			Marshal.Copy (errbuf, bytea, 0, errbufSize);
			errbufSize = 0;
			// first call to OCICharSetToUnicode gets the size
			OciCalls.OCICharSetToUnicode (Parent, null, bytea, out errbufSize);
			StringBuilder str = new StringBuilder (errbufSize);
			// second call to OCICharSetToUnicode gets the string
			OciCalls.OCICharSetToUnicode (Parent, str, bytea, out errbufSize);
			string errmsg = String.Empty;
			if (errbufSize > 0)
				errmsg = str.ToString ();
			info.ErrorMessage = String.Copy (errmsg);
			Marshal.FreeHGlobal (errbuf);

			return info;
		}

		#endregion // Methods
	}
}
