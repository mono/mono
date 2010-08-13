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

#if NET_4_0
using System;
using System.Threading;

namespace System.Threading.Tasks
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
		
		ExternalInfos extInfos;
		
		internal ParallelLoopState (ExternalInfos extInfos)
		{
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
			extInfos.IsStopped.Exchange (true);
		}
	}
	
}
#endif
