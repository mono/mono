// System/System.IOSelector.cs
//
// Authors:
//	Ludovic Henry <ludovic@xamarin.com>
//
// Copyright (C) 2015 Xamarin, Inc. (https://www.xamarin.com)
//
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
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Threading;

namespace System
{
	internal enum IOOperation : int
	{
		/* Keep in sync with MonoIOOperation in mono/metadata/threadpool-ms-io.c */

		Read  = 1 << 0,
		Write = 1 << 1,
	}

	internal delegate void IOAsyncCallback (IOAsyncResult ioares);

	[StructLayout (LayoutKind.Sequential)]
	internal abstract class IOAsyncResult : IAsyncResult
	{
		AsyncCallback async_callback;
		object async_state;

		ManualResetEvent wait_handle;
		bool completed_synchronously;
		bool completed;

		protected IOAsyncResult ()
		{
		}

		protected void Init (AsyncCallback async_callback, object async_state)
		{
			this.async_callback = async_callback;
			this.async_state = async_state;

			completed = false;
			completed_synchronously = false;

			if (wait_handle != null)
				wait_handle.Reset ();
		}

		protected IOAsyncResult (AsyncCallback async_callback, object async_state)
		{
			this.async_callback = async_callback;
			this.async_state = async_state;
		}

		public AsyncCallback AsyncCallback
		{
			get { return async_callback; }
		}

		public object AsyncState
		{
			get { return async_state; }
		}

		public WaitHandle AsyncWaitHandle
		{
			get {
				lock (this) {
					if (wait_handle == null)
						wait_handle = new ManualResetEvent (completed);
					return wait_handle;
				}
			}
		}

		public bool CompletedSynchronously
		{
			get {
				return completed_synchronously;
			}
			protected set {
				completed_synchronously = value;
			}
		}

		public bool IsCompleted
		{
			get {
				return completed;
			}
			protected set {
				completed = value;
				lock (this) {
					if (value && wait_handle != null)
						wait_handle.Set ();
				}
			}
		}

		internal abstract void CompleteDisposed();
	}

	[StructLayout (LayoutKind.Sequential)]
	internal class IOSelectorJob : IThreadPoolWorkItem
	{
		/* Keep in sync with MonoIOSelectorJob in mono/metadata/threadpool-ms-io.c */
		IOOperation operation;
		IOAsyncCallback callback;
		IOAsyncResult state;

		public IOSelectorJob (IOOperation operation, IOAsyncCallback callback, IOAsyncResult state)
		{
			this.operation = operation;
			this.callback = callback;
			this.state = state;
		}

		void IThreadPoolWorkItem.ExecuteWorkItem ()
		{
			this.callback (this.state);
		}

		void IThreadPoolWorkItem.MarkAborted (ThreadAbortException tae)
		{
		}

		public void MarkDisposed ()
		{
			state.CompleteDisposed ();
		}
	}

	internal static class IOSelector
	{
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public static extern void Add (IntPtr handle, IOSelectorJob job);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public static extern void Remove (IntPtr handle);
	}
}
