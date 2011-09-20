//
// AsyncTaskMethodBuilder.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2011 Novell, Inc (http://www.novell.com)
// Copyright (C) 2011 Xamarin, Inc (http://www.xamarin.com)
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

#if NET_4_5

using System.Threading.Tasks;

namespace System.Runtime.CompilerServices
{
	public struct AsyncTaskMethodBuilder
	{
		readonly TaskCompletionSource<object> tcs;

		private AsyncTaskMethodBuilder (TaskCompletionSource<object> tcs)
		{
			this.tcs = tcs;
		}

		public Task Task {
			get {
				return tcs.Task;
			}
		}
		
		public static AsyncTaskMethodBuilder Create ()
		{
			return new AsyncTaskMethodBuilder (new TaskCompletionSource<object> ());
		}

		public void SetException (Exception exception)
		{
			if (!tcs.TrySetException (exception))
				throw new InvalidOperationException ("The task has already completed");
		}

		public void SetResult ()
		{
			if (!tcs.TrySetResult (null))
				throw new InvalidOperationException ("The task has already completed");
		}
	}
}

#endif