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
// Copyright (c) 2006 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Miguel de Icaza (miguel@novell.com)
//

using System;
using System.ComponentModel;
using System.Threading;

namespace System.Windows.Threading {

	public abstract class DispatcherObject {
		Thread base_thread;
		
		protected DispatcherObject ()
		{
			base_thread = Thread.CurrentThread;
		}
		
		[EditorBrowsable (EditorBrowsableState.Never)]
		public bool CheckAccess ()
		{
			return Thread.CurrentThread == base_thread;
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		public void VerifyAccess ()
		{
			throw new InvalidOperationException ("The calling thread is not the same as the creation thread");
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		public Dispatcher Dispatcher {
			get {
				return Dispatcher.FromThread (base_thread);
			}
		}
	}
}
