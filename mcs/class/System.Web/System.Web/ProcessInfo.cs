// 
// System.Web.ProcessInfo
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002

namespace System.Web {
	public class ProcessInfo {

		#region Fields

		TimeSpan age;
		int peakMemoryUsed;
		int processID;
		int requestCount;
		ProcessShutdownReason shutdownReason;
		DateTime startTime;
		ProcessStatus status;

		#endregion

		#region Constructors

		public ProcessInfo ()
		{
		}

		public ProcessInfo (DateTime startTime, TimeSpan age, int processID, int requestCount, ProcessStatus status, ProcessShutdownReason shutdownReason, int peakMemoryUsed)
		{
			this.age = age;
			this.peakMemoryUsed = peakMemoryUsed;
			this.processID = processID;
			this.requestCount = requestCount;
			this.shutdownReason = shutdownReason;
			this.startTime = startTime;
			this.status = status;
		}

		#endregion

		#region Properties

		public TimeSpan Age {
			get { return age; }
		}

		public int PeakMemoryUsed {
			get { return peakMemoryUsed; }
		}

		public int ProcessID {
			get { return processID; }
		}

		public int RequestCount {
			get { return requestCount; }
		}

		public ProcessShutdownReason ShutdownReason {
			get { return shutdownReason; }
		}

		public DateTime StartTime {
			get { return startTime; }
		}

		public ProcessStatus Status {
			get { return status; }
		}

		#endregion // Properties

		#region Methods

		public void SetAll (DateTime startTime, TimeSpan age, int processID, int requestCount, ProcessStatus status, ProcessShutdownReason shutdownReason, int peakMemoryUsed)
		{
			this.age = age;
			this.peakMemoryUsed = peakMemoryUsed;
			this.processID = processID;
			this.requestCount = requestCount;
			this.shutdownReason = shutdownReason;
			this.startTime = startTime;
			this.status = status;
		}

		#endregion // Methods
	}
}
