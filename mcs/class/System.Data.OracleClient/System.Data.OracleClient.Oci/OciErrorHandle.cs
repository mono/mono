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

		public static OciErrorInfo HandleError (OciHandle hand) 
		{
			OciErrorInfo info;
			info.ErrorCode = 0;
			info.ErrorMessage = String.Empty;

			int errbufSize = 4096;
			IntPtr errbuf = Marshal.AllocHGlobal (errbufSize);

			OciCalls.OCIErrorGet (hand, 
				1,
				IntPtr.Zero,
				out info.ErrorCode,
				errbuf,
				(uint) errbufSize,
				OciHandleType.Error);

			byte[] bytea = new byte[errbufSize];
			Marshal.Copy (errbuf, bytea, 0, errbufSize);
			errbufSize = 0;
			
			OciHandle h = hand.Parent;
			if (h == null)
				h = hand;

			// first call to OCICharSetToUnicode gets the size
			OciCalls.OCICharSetToUnicode (h, null, bytea, out errbufSize);
			StringBuilder str = new StringBuilder (errbufSize);
			
			// second call to OCICharSetToUnicode gets the string
			OciCalls.OCICharSetToUnicode (h, str, bytea, out errbufSize);
			
			string errmsg = String.Empty;
			if (errbufSize > 0)
				errmsg = str.ToString ();
			
			info.ErrorMessage = String.Copy (errmsg);
			Marshal.FreeHGlobal (errbuf);

			return info;
		}

		public OciErrorInfo HandleError () 
		{
			return HandleError (this);
		}

		#endregion // Methods
	}
}
