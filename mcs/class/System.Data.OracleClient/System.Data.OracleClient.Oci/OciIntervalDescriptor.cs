//
// OciIntervalDescriptor.cs - used for an Oracle TIMESPAN/INTERVAL{YTM/DTS}
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
//     Veerapuram Varadhan  <vvaradhan@novell.com>
//
// Copyright (C) Novell Inc, 2009
//

using System;
using System.Data;
using System.Data.OracleClient;
using System.Runtime.InteropServices;

namespace System.Data.OracleClient.Oci {
	internal sealed class OciIntervalDescriptor : OciDescriptorHandle, IDisposable
	{
		#region Fields

		OciErrorHandle errorHandle;
		bool disposed = false;
		
		#endregion // Fields

		#region Constructors

		public OciIntervalDescriptor (OciHandle parent, OciHandleType type, IntPtr newHandle)
			: base (type, parent, newHandle)
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

		public TimeSpan GetDayToSecond (OciHandle handle, OciErrorHandle errorHandle)
		{
			int days = 0;
			int hours = 0;
			int mins = 0;
			int secs = 0;
			int fsec = 0;
			int fs = 0;
			
			OciCalls.OCIIntervalGetDaySecond (handle, errorHandle, out days, out hours, 
			                                  out mins, out secs, out fsec, this.handle);
			if (fsec > 0) {
				int fseci = (int) fsec;
				fs = fseci / 1000000;
			}
			return new TimeSpan (days, hours, mins, secs, fs);                             
		}
		
		public int GetYearToMonth (OciHandle handle, OciErrorHandle errorHandle)
		{
			int years = 0;
			int months = 0;
			
			OciCalls.OCIIntervalGetYearMonth (handle, errorHandle, out years, out months, this.handle);
			
			return ((years * 12) + months);
		}

		#endregion // Methods
	}
}

