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

#if NET_2_0
using System.Runtime.ConstrainedExecution;
#endif

namespace System
{
	public
#if NET_2_0
	static
#else
	sealed
#endif
	class GC
	{

#if !NET_2_0
		private GC ()
		{
		}
#endif

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

#if NET_2_0
		[MonoDocumentationNote ("mode parameter ignored")]
		public static void Collect (int generation, GCCollectionMode mode) {
			Collect (generation);
		}
#endif

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
#if NET_2_0
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
#endif
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static void KeepAlive (object obj);
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static void ReRegisterForFinalize (object obj);

#if NET_2_0
		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
#endif
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static void SuppressFinalize (object obj);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static void WaitForPendingFinalizers ();

#if NET_2_0
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
#endif
	}
}
