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

using System;
using System.Globalization;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;

namespace System.Threading {

	[ComVisibleAttribute (false)]
	public struct CompressedStackSwitcher : IDisposable {

		private bool _undo;

		[MonoTODO]
		public override bool Equals (object obj)
		{
			return false;
		}

		[MonoTODO]
		public override int GetHashCode ()
		{
			return 0;
		}

		[MonoTODO]
		[ReliabilityContract (Consistency.WillNotCorruptState, CER.MayFail)]
		public void Undo ()
		{
			_undo = true;
		}

// LAMESPEC: documented but not implemented (not shown by corcompare)
#if false
		[MonoTODO]
		public static bool op_Equality (CompressedStackSwitcher c1, CompressedStackSwitcher c2)
		{
			return false;
		}

		[MonoTODO]
		public static bool op_Inequality (CompressedStackSwitcher c1, CompressedStackSwitcher c2)
		{
			return false;
		}
#endif

		void IDisposable.Dispose () 
		{
			if (!_undo)
				Undo ();
		}
	}
}

#endif
