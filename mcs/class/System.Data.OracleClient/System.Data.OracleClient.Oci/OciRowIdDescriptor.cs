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
//     Daniel Morgan <monodanmorg@yahoo.com>
//
// Copyright (C) Tim Coleman, 2003
// Copyright (C) Daniel Morgan, 2008
//

using System;
using System.Data.OracleClient;
using System.Runtime.InteropServices;
using System.Text;

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

		protected override void Dispose (bool disposing)
		{
			if (!disposed) {
				disposed = true;
				base.Dispose (disposing);
			}
		}

		//FIXME: This method only exists in Oracle 9i client and above
		[DllImport ("oci")]
		static extern int OCIRowidToChar (IntPtr rowidDesc,
						IntPtr outbfp,
						ref ushort outbflp,
						IntPtr errhp);

		[MonoTODO ("Only will work with 9i and above. Get it to work for 8i as well.")]
		internal string GetRowIdToString (OciErrorHandle errorHandle)
		{
			string output = String.Empty;

			int len = 18; // Universal ROWID has a length of 18
			int maxByteCount = Encoding.UTF8.GetMaxByteCount (len);
			IntPtr outputPtr = OciCalls.AllocateClear (maxByteCount); 

			int status = 0;

			ushort u = (ushort) maxByteCount;

			status = OCIRowidToChar (Handle,
						outputPtr,
						ref u,
						errorHandle);

                        if (status != 0) {
                                OciErrorInfo info = errorHandle.HandleError ();
                                throw new OracleException (info.ErrorCode, info.ErrorMessage);
                        }

                        if (outputPtr != IntPtr.Zero && maxByteCount > 0) {
				object str = Marshal.PtrToStringAnsi (outputPtr, len);
				if (str != null)
					output = String.Copy ((string) str);
			}

			return output;
		}

		#endregion // Methods
	}
}


