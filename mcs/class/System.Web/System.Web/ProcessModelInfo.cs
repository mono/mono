// 
// System.Web.ProcessModelInfo
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002

using System;

namespace System.Web {
	public class ProcessModelInfo {

		#region Fields

		#endregion

		#region Constructors

		public ProcessModelInfo ()
		{
		}

		#endregion

		#region Properties

		[MonoTODO ("Retrieve appropriate variables from worker")]
		public static ProcessInfo GetCurrentProcessInfo ()
		{
			HttpContext httpContext;
			DateTime startTime;
			TimeSpan age;
			int processID;
			int requestCount;
			ProcessStatus status;
			ProcessShutdownReason shutdownReason;
			int peakMemoryUsed;

			httpContext = HttpContext.Current;
			return new ProcessInfo (startTime, age, processID, requestCount, status, shutdownReason, peakMemoryUsed);
		}

		[MonoTODO ("Retrieve process information.")]
		public static ProcessInfo[] GetHistory ()
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
