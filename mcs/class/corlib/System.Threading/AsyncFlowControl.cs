//
// System.Threading.AsyncFlowControl.cs
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

using System.Globalization;
using System.Runtime.InteropServices;
using System.Security;

namespace System.Threading {

	internal enum AsyncFlowControlType {
		None,
		Execution,
		Security
	}

	public struct AsyncFlowControl : IDisposable {
		private Thread _t;
		private AsyncFlowControlType _type;

		internal AsyncFlowControl (Thread t, AsyncFlowControlType type)
		{
			_t = t;
			_type = type;
		}

		public void Undo ()
		{
			if (_t == null) {
				throw new InvalidOperationException (Locale.GetText (
					"Can only be called once."));
			}
			switch (_type) {
			case AsyncFlowControlType.Execution:
				ExecutionContext.RestoreFlow ();
				break;
			case AsyncFlowControlType.Security:
				SecurityContext.RestoreFlow ();
				break;
			}
			_t = null;
		}

#if NET_4_0 || MOBILE
		public void Dispose ()
#else
		void IDisposable.Dispose ()
#endif
		{
			if (_t != null) {
				Undo ();
				_t = null;
				_type = AsyncFlowControlType.None;
			}
		}

		public override int GetHashCode ()
		{
			return(base.GetHashCode ());
		}
		
		public override bool Equals (object obj)
		{
			if (!(obj is AsyncFlowControl)) {
				return(false);
			}
			
			return(obj.Equals (this));
		}
		
		public bool Equals (AsyncFlowControl obj)
		{
			if (this._t == obj._t &&
			    this._type == obj._type) {
				return(true);
			} else {
				return(false);
			}
		}

		public static bool operator == (AsyncFlowControl a, AsyncFlowControl b)
		{
			return a.Equals (b);
		}

		public static bool operator != (AsyncFlowControl a, AsyncFlowControl b)
		{
			return !a.Equals (b);
		}
	}
}
