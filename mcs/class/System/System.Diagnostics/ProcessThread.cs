//
// System.Diagnostics.ProcessThread.cs
//
// Authors:
//	Dick Porter (dick@ximian.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System.ComponentModel;

namespace System.Diagnostics {
	public class ProcessThread : Component {
		[MonoTODO]
		public int BasePriority {
			get {
				return(0);
			}
		}

		[MonoTODO]
		public int CurrentPriority {
			get {
				return(0);
			}
		}

		[MonoTODO]
		public int Id {
			get {
				return(0);
			}
		}

		[MonoTODO]
		int IdealProcessor {
			set {
			}
		}

		[MonoTODO]
		public bool PriorityBoostEnabled {
			get {
				return(false);
			}
			set {
			}
		}

		[MonoTODO]
		public ThreadPriorityLevel PriorityLevel {
			get {
				return(ThreadPriorityLevel.Idle);
			}
			set {
			}
		}

		[MonoTODO]
		public TimeSpan PrivilegedProcessorTime {
			get {
				return(new TimeSpan(0));
			}
		}

		[MonoTODO]
		IntPtr ProcessorAffinity {
			set {
			}
		}

		[MonoTODO]
		public IntPtr StartAddress {
			get {
				return((IntPtr)0);
			}
		}

		[MonoTODO]
		public DateTime StartTime {
			get {
				return(new DateTime(0));
			}
		}

		[MonoTODO]
		public ThreadState ThreadState {
			get {
				return(ThreadState.Initialized);
			}
		}

		[MonoTODO]
		public TimeSpan TotalProcessorTime {
			get {
				return(new TimeSpan(0));
			}
		}

		[MonoTODO]
		public TimeSpan UserProcessorTime {
			get {
				return(new TimeSpan(0));
			}
		}

		[MonoTODO]
		public ThreadWaitReason WaitReason {
			get {
				return(ThreadWaitReason.Executive);
			}
		}

		[MonoTODO]
		public void ResetIdealProcessor() {
		}
	}
}
