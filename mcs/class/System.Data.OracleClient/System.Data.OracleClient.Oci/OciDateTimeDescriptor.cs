//
// OciDateTimeDescriptor.cs - used for an Oracle TIMESTAMP
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
//     Daniel Morgan <danielmorgan@verizon.net>
//
// Copyright (C) Daniel Morgan, 2005
//

using System;
using System.Data;
using System.Data.OracleClient;
using System.Runtime.InteropServices;

namespace System.Data.OracleClient.Oci {
	internal sealed class OciDateTimeDescriptor : OciDescriptorHandle, IDisposable
	{
		#region Fields

		OciErrorHandle errorHandle;
		//OciServiceHandle service;
		bool disposed = false;

		#endregion // Fields

		#region Constructors

		public OciDateTimeDescriptor (OciHandle parent, IntPtr newHandle)
			: base (OciHandleType.TimeStamp, parent, newHandle)
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
				disposed = true;
				base.Dispose (disposing);
			}
		}

		public void SetDateTime (OciHandle handle, OciErrorHandle errorHandle,
					short year, byte month, byte day,
					byte hour, byte min, byte sec, uint fsec, string timezone)
		{
			// Get size of buffer
			int rsize = 0;
			int status = OciCalls.OCIUnicodeToCharSet (handle, null, timezone, out rsize);

			// Fill buffer
			byte[] bytes = new byte[rsize];
			if (status == 0 && rsize > 0)
				OciCalls.OCIUnicodeToCharSet (handle, bytes, timezone, out rsize);

			if (fsec > 0)
				fsec = fsec * 1000000;

			uint timezoneSize = (uint) bytes.Length;
			OciCalls.OCIDateTimeConstruct (handle,
				errorHandle, this.Handle,
				year, month, day,
				hour, min, sec, fsec,
				bytes, timezoneSize);

			//uint valid = 0;
			//int result = OciCalls.OCIDateTimeCheck (handle,
			//	errorHandle, this.Handle, out valid);
		}

		public DateTime GetDateTime (OciHandle handle, OciErrorHandle errorHandle)
		{
			short year = 0;
			byte month = 0;
			byte day = 0;
			byte hour = 0;
			byte min = 0;
			byte sec = 0;
			uint fsec = 0;
			int fs = 0;

			OciCalls.OCIDateTimeGetDate (handle, errorHandle, this.Handle,
				out year, out month, out day);
			OciCalls.OCIDateTimeGetTime (handle, errorHandle, this.Handle,
				out hour, out min, out sec, out fsec);

			// a TIMESTAMP can have up to 9 digits of millisecond but DateTime only
			// can be up to 999.
			if (fsec > 0) {
				int fseci = (int) fsec;
				fs = fseci / 1000000;
			}

			return new DateTime(year, month, day, hour, min, sec, fs);
		}

		#endregion // Methods
	}
}

