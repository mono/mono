//
// System.Diagnostics.ProcessThread.cs
//
// Authors:
//   Dick Porter (dick@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System.ComponentModel;
using System.ComponentModel.Design;

namespace System.Diagnostics 
{
	[Designer ("System.Diagnostics.Design.ProcessThreadDesigner, " + Consts.AssemblySystem_Design, typeof (IDesigner))]
	public class ProcessThread : Component 
	{

		[MonoTODO ("Parse parameters")]
		internal ProcessThread() 
		{
		}

		[MonoTODO]
		[MonitoringDescription ("The base priority of this thread.")]
		public int BasePriority {
			get {
				return(0);
			}
		}

		[MonoTODO]
		[MonitoringDescription ("The current priority of this thread.")]
		public int CurrentPriority {
			get {
				return(0);
			}
		}

		[MonoTODO]
		[MonitoringDescription ("The ID of this thread.")]
		public int Id {
			get {
				return(0);
			}
		}

		[MonoTODO]
		[Browsable (false)]
		public int IdealProcessor {
			set {
			}
		}

		[MonoTODO]
		[MonitoringDescription ("Thread gets a priority boot when interactively used by a user.")]
		public bool PriorityBoostEnabled {
			get {
				return(false);
			}
			set {
			}
		}

		[MonoTODO]
		[MonitoringDescription ("The priority level of this thread.")]
		public ThreadPriorityLevel PriorityLevel {
			get {
				return(ThreadPriorityLevel.Idle);
			}
			set {
			}
		}

		[MonoTODO]
		[MonitoringDescription ("The amount of CPU time used in privileged mode.")]
		public TimeSpan PrivilegedProcessorTime {
			get {
				return(new TimeSpan(0));
			}
		}

		[MonoTODO]
		[Browsable (false)]
		public IntPtr ProcessorAffinity {
			set {
			}
		}

		[MonoTODO]
		[MonitoringDescription ("The start address in memory of this thread.")]
		public IntPtr StartAddress {
			get {
				return((IntPtr)0);
			}
		}

		[MonoTODO]
		[MonitoringDescription ("The time this thread was started.")]
		public DateTime StartTime {
			get {
				return(new DateTime(0));
			}
		}

		[MonoTODO]
		[MonitoringDescription ("The current state of this thread.")]
		public ThreadState ThreadState {
			get {
				return(ThreadState.Initialized);
			}
		}

		[MonoTODO]
		[MonitoringDescription ("The total amount of CPU time used.")]
		public TimeSpan TotalProcessorTime {
			get {
				return(new TimeSpan(0));
			}
		}

		[MonoTODO]
		[MonitoringDescription ("The amount of CPU time used in user mode.")]
		public TimeSpan UserProcessorTime {
			get {
				return(new TimeSpan(0));
			}
		}

		[MonoTODO]
		[MonitoringDescription ("The reason why this thread is waiting.")]
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
