#if NET_4_0
// ParallelState.cs
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

using System;
using System.Threading.Tasks;

namespace System.Threading
{
	public class ParallelLoopState
	{
		internal class ExternalInfos
		{
			public AtomicBoolean IsStopped = new AtomicBoolean ();
			public AtomicBoolean IsBroken = new AtomicBoolean ();
			public volatile bool IsExceptional;
			public long? LowestBreakIteration;
		}
		
		Task[] tasks;
		ExternalInfos extInfos;
		
		internal ParallelLoopState (Task[] tasks, ExternalInfos extInfos)
		{
			this.tasks = tasks;
			this.extInfos = extInfos;
		}
		
		public bool IsStopped {
			get {
				return extInfos.IsStopped.Value;
			}
		}
		
		public bool IsExceptional {
			get {
				return extInfos.IsExceptional;
			}
		}
		
		public long? LowestBreakIteration {
			get {
				return extInfos.LowestBreakIteration;
			}
		}
		
		internal int CurrentIteration {
			get;
			set;
		}
		
		public bool ShouldExitCurrentIteration {
			get {
				return IsExceptional || IsStopped;
			}
		}
		
		public void Break ()
		{
			bool result = extInfos.IsBroken.Exchange (true);
			if (!result)
				extInfos.LowestBreakIteration = CurrentIteration;
		}
		
		public void Stop ()
		{
			bool result = extInfos.IsStopped.Exchange (true);
			if (!result) {
				foreach (var t in tasks) {
					if (t == null)
						continue;
					t.Cancel ();
				}
			}
		}
	}
	
}
#endif
