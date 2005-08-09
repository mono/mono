//
// System.Diagnostics.ProcessThread.cs
//
// Authors:
//   Dick Porter (dick@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

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

using System.ComponentModel;
using System.ComponentModel.Design;

namespace System.Diagnostics 
{
	[Designer ("System.Diagnostics.Design.ProcessThreadDesigner, " + Consts.AssemblySystem_Design)]
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
