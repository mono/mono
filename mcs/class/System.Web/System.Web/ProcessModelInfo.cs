// 
// System.Web.ProcessModelInfo.cs
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
			DateTime startTime = DateTime.Now;
			TimeSpan age = TimeSpan.Zero;
			int processID = 0;
			int requestCount = 0;
			ProcessStatus status = ProcessStatus.Terminated;
			ProcessShutdownReason shutdownReason = ProcessShutdownReason.None;
			int peakMemoryUsed = 0;

			httpContext = HttpContext.Current;
			return new ProcessInfo (startTime, age, processID, requestCount, status, shutdownReason, peakMemoryUsed);
		}

		[MonoTODO ("Retrieve process information.")]
		public static ProcessInfo[] GetHistory (int numRecords)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
