// 
// System.Web.ProcessInfo
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Security.Permissions;

namespace System.Web {

	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
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
