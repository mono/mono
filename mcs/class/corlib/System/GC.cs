//
// System.GC.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.CompilerServices;

namespace System
{
	public sealed class GC
	{
		private GC ()
		{
		}

		// TODO: as long as we use Boehm leave 0...
		public static int MaxGeneration {
			get {return 0;}
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern static void InternalCollect (int generation);
		
		public static void Collect () {
			InternalCollect (MaxGeneration);
		}

		public static void Collect (int generation) {
			if (generation < 0 || generation > MaxGeneration)
				throw new ArgumentOutOfRangeException ("generation");
			InternalCollect (generation);
		}

		public static int GetGeneration (object obj) {
			return 0;
		}

		public static int GetGeneration (WeakReference wo) {
			if (!wo.IsAlive)
				throw new ArgumentException ();
			return 0;
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static long GetTotalMemory (bool forceFullCollection);

		/* this icall has weird semantics check the docs... */
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static void KeepAlive (object obj);
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static void ReRegisterForFinalize (object obj);
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static void SuppressFinalize (object obj);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static void WaitForPendingFinalizers ();
		
	}
}
