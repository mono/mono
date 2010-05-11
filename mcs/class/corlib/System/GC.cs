//
// System.GC.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Security.Permissions;

namespace System
{
	public static class GC
	{

		public extern static int MaxGeneration {
			[MethodImplAttribute(MethodImplOptions.InternalCall)]
			get;
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern static void InternalCollect (int generation);
		
		public static void Collect () {
			InternalCollect (MaxGeneration);
		}

		public static void Collect (int generation) {
			if (generation < 0)
				throw new ArgumentOutOfRangeException ("generation");
			InternalCollect (generation);
		}

		[MonoDocumentationNote ("mode parameter ignored")]
		public static void Collect (int generation, GCCollectionMode mode) {
			Collect (generation);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static int GetGeneration (object obj);

		public static int GetGeneration (WeakReference wo) {
			object obj = wo.Target;
			if (obj == null)
				throw new ArgumentException ();
			return GetGeneration (obj);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static long GetTotalMemory (bool forceFullCollection);

		/* this icall has weird semantics check the docs... */
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static void KeepAlive (object obj);
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static void ReRegisterForFinalize (object obj);

		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static void SuppressFinalize (object obj);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static void WaitForPendingFinalizers ();

		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static int CollectionCount (int generation);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static void RecordPressure (long bytesAllocated);

		public static void AddMemoryPressure (long bytesAllocated) {
			RecordPressure (bytesAllocated);
		}

		public static void RemoveMemoryPressure (long bytesAllocated) {
			RecordPressure (-bytesAllocated);
		}

#if NET_4_0
		[PermissionSetAttribute (SecurityAction.LinkDemand, Name = "FullTrust")]
		[MonoTODO]
		public static GCNotificationStatus WaitForFullGCApproach () {
			throw new NotImplementedException ();
		}

		[PermissionSetAttribute (SecurityAction.LinkDemand, Name = "FullTrust")]
		[MonoTODO]
		public static GCNotificationStatus WaitForFullGCApproach (int millisecondsTimeout) {
			throw new NotImplementedException ();
		}

		[PermissionSetAttribute (SecurityAction.LinkDemand, Name = "FullTrust")]
		[MonoTODO]
		public static GCNotificationStatus WaitForFullGCComplete () {
			throw new NotImplementedException ();
		}

		[PermissionSetAttribute (SecurityAction.LinkDemand, Name = "FullTrust")]
		[MonoTODO]
		public static GCNotificationStatus WaitForFullGCComplete (int millisecondsTimeout) {
			throw new NotImplementedException ();
		}

		[PermissionSetAttribute (SecurityAction.LinkDemand, Name = "FullTrust")]
		public static void RegisterForFullGCNotification (int maxGenerationThreshold, int largeObjectHeapThreshold) {
			if (maxGenerationThreshold < 1 || maxGenerationThreshold > 99)
				throw new ArgumentOutOfRangeException ("maxGenerationThreshold", maxGenerationThreshold, "maxGenerationThreshold must be between 1 and 99 inclusive");
			if (largeObjectHeapThreshold < 1 || largeObjectHeapThreshold > 99)
				throw new ArgumentOutOfRangeException ("largeObjectHeapThreshold", largeObjectHeapThreshold, "largeObjectHeapThreshold must be between 1 and 99 inclusive");
			throw new NotImplementedException ();
		}

		[PermissionSetAttribute (SecurityAction.LinkDemand, Name = "FullTrust")]
		public static void CancelFullGCNotification () {
			throw new NotImplementedException ();
		}
#endif

#if NET_4_0 || BOOTSTRAP_NET_4_0 || MOONLIGHT
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern static void register_ephemeron_array (Ephemeron[] array);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern static object get_ephemeron_tombstone ();

		internal static readonly object EPHEMERON_TOMBSTONE = get_ephemeron_tombstone ();
#endif
	}
}