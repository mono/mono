// 
// OciRowIdDescriptor.cs 
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
using System.Data.OracleClient;
using System.Runtime.InteropServices;

namespace System.Data.OracleClient.Oci {
	internal sealed class OciRowIdDescriptor : OciDescriptorHandle, IDisposable
	{
		#region Fields

		bool disposed = false;

		#endregion // Fields

		#region Constructors

		public OciRowIdDescriptor (OciHandle parent, IntPtr newHandle)
			: base (OciHandleType.RowId, parent, newHandle)
		{
		}

		#endregion // Constructors

		#region Methods

		/*
		FIXME: This method only exists in Oracle 9i

		[DllImport ("oci")] 
		static extern int OCIRowidToChar (IntPtr rowidDesc,
						IntPtr outbfp,
						ref int outbflp,
						IntPtr errhp);
		*/

		protected override void Dispose (bool disposing)
		{
			if (!disposed) {
				disposed = true;
				base.Dispose (disposing);
			}
		}

		[MonoTODO ("Find a way to get this with 8 or 9.")]
		public string GetRowId (OciErrorHandle errorHandle)
		{
			string output = String.Empty;
			/*
			int len = 64;
			IntPtr outputPtr = Marshal.AllocHGlobal (len); // FIXME: how big should this be?

			int status = 0;

			status = OCIRowidToChar (this,
						outputPtr,
						ref len,
						errorHandle);

                        if (status != 0) {
                                OciErrorInfo info = errorHandle.HandleError ();
                                throw new OracleException (info.ErrorCode, info.ErrorMessage);
                        }

                        if (outputPtr != IntPtr.Zero && len > 0) {
				object str = Marshal.PtrToStringAnsi (outputPtr, len);
				if (str != null)
					output = String.Copy ((string) str);
			}

			*/
			output = "NOT YET SUPPORTED.";

			return output;
		} 

		#endregion // Methods
	}
}
