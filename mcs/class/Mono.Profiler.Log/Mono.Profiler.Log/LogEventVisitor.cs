// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Mono.Profiler.Log {

	public abstract class LogEventVisitor {

		public virtual void VisitBefore (LogEvent ev)
		{
		}

		public virtual void VisitAfter (LogEvent ev)
		{
		}

		public virtual void Visit (AppDomainLoadEvent ev)
		{
		}

		public virtual void Visit (AppDomainUnloadEvent ev)
		{
		}

		public virtual void Visit (AppDomainNameEvent ev)
		{
		}

		public virtual void Visit (ContextLoadEvent ev)
		{
		}

		public virtual void Visit (ContextUnloadEvent ev)
		{
		}

		public virtual void Visit (ThreadStartEvent ev)
		{
		}

		public virtual void Visit (ThreadEndEvent ev)
		{
		}

		public virtual void Visit (ThreadNameEvent ev)
		{
		}

		public virtual void Visit (ImageLoadEvent ev)
		{
		}

		public virtual void Visit (ImageUnloadEvent ev)
		{
		}

		public virtual void Visit (AssemblyLoadEvent ev)
		{
		}

		public virtual void Visit (AssemblyUnloadEvent ev)
		{
		}

		public virtual void Visit (ClassLoadEvent ev)
		{
		}

		public virtual void Visit (VTableLoadEvent ev)
		{
		}

		public virtual void Visit (JitEvent ev)
		{
		}

		public virtual void Visit (JitHelperEvent ev)
		{
		}

		public virtual void Visit (AllocationEvent ev)
		{
		}

		public virtual void Visit (HeapBeginEvent ev)
		{
		}

		public virtual void Visit (HeapEndEvent ev)
		{
		}

		public virtual void Visit (HeapObjectEvent ev)
		{
		}

		public virtual void Visit (HeapRootsEvent ev)
		{
		}

		public virtual void Visit (HeapRootRegisterEvent ev)
		{
		}

		public virtual void Visit (HeapRootUnregisterEvent ev)
		{
		}

		public virtual void Visit (GCEvent ev)
		{
		}

		public virtual void Visit (GCResizeEvent ev)
		{
		}

		public virtual void Visit (GCMoveEvent ev)
		{
		}

		public virtual void Visit (GCHandleCreationEvent ev)
		{
		}

		public virtual void Visit (GCHandleDeletionEvent ev)
		{
		}

		public virtual void Visit (GCFinalizeBeginEvent ev)
		{
		}

		public virtual void Visit (GCFinalizeEndEvent ev)
		{
		}

		public virtual void Visit (GCFinalizeObjectBeginEvent ev)
		{
		}

		public virtual void Visit (GCFinalizeObjectEndEvent ev)
		{
		}

		public virtual void Visit (ThrowEvent ev)
		{
		}

		public virtual void Visit (ExceptionClauseEvent ev)
		{
		}

		public virtual void Visit (EnterEvent ev)
		{
		}

		public virtual void Visit (LeaveEvent ev)
		{
		}

		public virtual void Visit (ExceptionalLeaveEvent ev)
		{
		}

		public virtual void Visit (MonitorEvent ev)
		{
		}

		public virtual void Visit (SampleHitEvent ev)
		{
		}

		public virtual void Visit (CounterSamplesEvent ev)
		{
		}

		public virtual void Visit (CounterDescriptionsEvent ev)
		{
		}

		public virtual void Visit (UnmanagedBinaryEvent ev)
		{
		}

		public virtual void Visit (UnmanagedSymbolEvent ev)
		{
		}

		public virtual void Visit (SynchronizationPointEvent ev)
		{
		}
	}
}
