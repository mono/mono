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

		// LAMESPEC: MS documentation says that it raises an error
		// when the argument generation is greater than MaxGeneration,
		// but it doesn't. (The documentation is fixed in 2.0.)
		public static void Collect (int generation) {
			if (generation < 0)
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
