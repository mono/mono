//
// System.Threading.CompressedStackSwitcher structure
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
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

#if NET_2_0

using System.Globalization;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;

namespace System.Threading {

	[ComVisibleAttribute (false)]
	public struct CompressedStackSwitcher : IDisposable {

		private CompressedStack _cs;
		private Thread _t;

		internal CompressedStackSwitcher (CompressedStack cs, Thread t)
		{
			_cs = cs;
			_t = t;
		}

		public override bool Equals (object obj)
		{
			if (obj == null)
				return false;

			if (obj is CompressedStackSwitcher)
				return op_Equality (this, (CompressedStackSwitcher)obj);

			return false;
		}

		public override int GetHashCode ()
		{
			// documented as always the same for all instances
			return typeof (CompressedStackSwitcher).GetHashCode ();
			// Microsoft seems to return 1404280835 every time 
			// (even between executions).
		}

		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.MayFail)]
		public void Undo ()
		{
			if ((_cs != null) && (_t != null)) {
				lock (_cs) {
					if ((_cs != null) && (_t != null)) {
						_t.SetCompressedStack (_cs);
					}
					_t = null;
					_cs = null;
				}
			}
		}

		public static bool op_Equality (CompressedStackSwitcher c1, CompressedStackSwitcher c2)
		{
			if (c1._cs == null)
				return (c2._cs == null);
			if (c2._cs == null)
				return false;

			if (c1._t.ManagedThreadId != c2._t.ManagedThreadId)
				return false;
			return c1._cs.Equals (c2._cs);
		}

		public static bool op_Inequality (CompressedStackSwitcher c1, CompressedStackSwitcher c2)
		{
			return !op_Equality (c1, c2);
		}

		void IDisposable.Dispose () 
		{
			Undo ();
		}
	}
}

#endif
