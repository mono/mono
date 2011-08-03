// Author: Paolo Molaro <lupus@ximian.com>
//
// Copyright (C) 2009 Novell (http://www.novell.com)
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

using System;
using System.Runtime.CompilerServices;

namespace Mono.Tasklets {

	// we may want to not expose this class at all in the API
	// and just provide a higher-level API
	public class Continuation : IDisposable
	{
		IntPtr cont;

		public Continuation ()
		{
			cont = alloc ();
		}

		~Continuation ()
		{
			Dispose ();
		}

		public void Dispose ()
		{
			if (cont != IntPtr.Zero){
				free (cont);
				cont = IntPtr.Zero;
				GC.SuppressFinalize (this);
			}
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern static IntPtr alloc ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern static void free (IntPtr cont);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern static Exception mark (IntPtr cont);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern static int store (IntPtr cont, int state, out Exception exception);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern static Exception restore (IntPtr cont, int state);

		[MethodImplAttribute (MethodImplOptions.NoInlining)]
		public void Mark ()
		{
			Exception e = mark (cont);
			if (e != null)
				throw e;
		}

		public int Store (int state)
		{
			int rstate;
			Exception e;
			rstate = store (cont, state, out e);
			if (e != null)
				throw e;
			return rstate;
		}

		public void Restore (int state)
		{
			Exception e = restore (cont, state);
			if (e != null)
				throw e;
		}
	}

}

