// TaskCanceledException.cs
//
// Copyright (c) 2008 Jérémie "Garuma" Laval
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

#if NET_4_0 || BOOTSTRAP_NET_4_0
using System;
using System.Runtime.Serialization;

namespace System.Threading.Tasks
{
	[Serializable]
	public class TaskCanceledException : OperationCanceledException
	{
		Task task;
		
		public TaskCanceledException (): base ()
		{
		}
		
		public TaskCanceledException (string message): base (message)
		{
		}
		
		public TaskCanceledException (string message, Exception inner): base (message, inner)
		{
		}
		
		public TaskCanceledException (Task task): base ("The Task was canceled")
		{
			this.task = task;
		}
		
		protected TaskCanceledException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
			
		}
		
		public Task Task {
			get {
				return task;
			}
		}
	}
}
#endif
