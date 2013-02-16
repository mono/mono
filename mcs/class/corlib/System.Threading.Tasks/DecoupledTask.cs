//
// DecoupledTask.cs
//
// Authors:
//    Marek Safar  <marek.safar@gmail.com>
//
// Copyright 2013 Xamarin Inc (http://www.xamarin.com).
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

#if NET_4_5

namespace System.Threading.Tasks
{
	//
	// A proxy used to break direct dependency to TPL mostly useful for
	// mobile profile
	//
	interface IDecoupledTask
	{
		bool IsCompleted { get; }
	}

	sealed class DecoupledTask : IDecoupledTask
	{
		public DecoupledTask (Task task)
		{
			this.Task = task;
		}

		public bool IsCompleted {
			get {
				return Task.IsCompleted;
			}
		}

		public Task Task { get; private set; }
	}

	sealed class DecoupledTask<T> : IDecoupledTask
	{
		public DecoupledTask (Task<T> task)
		{
			this.Task = task;
		}

		public bool IsCompleted {
			get {
				return Task.IsCompleted;
			}
		}

		public Task<T> Task { get; private set; }
	}	
}

#endif
