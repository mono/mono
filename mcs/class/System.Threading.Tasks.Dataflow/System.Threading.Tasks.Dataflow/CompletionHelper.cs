// ActionBlock.cs
//
// Copyright (c) 2011 Jérémie "garuma" Laval
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//


using System;
using System.Threading.Tasks;

namespace System.Threading.Tasks.Dataflow
{
	/* This is used to implement a default behavior for Dataflow completion tracking
	 * that is the Completion property and Complete/Fault method combo
	 */
	internal struct CompletionHelper
	{
		TaskCompletionSource<object> source;

		public static CompletionHelper GetNew ()
		{
			CompletionHelper temp = new CompletionHelper ();
			temp.source = new TaskCompletionSource<object> ();
			return temp;
		}

		public Task Completion {
			get {
				return source.Task;
			}
		}

		public void Complete ()
		{
			source.TrySetResult (null);
		}

		public void Fault (Exception ex)
		{
			source.SetException (ex);
		}
	}
}
