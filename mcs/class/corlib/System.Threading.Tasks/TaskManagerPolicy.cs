#if NET_4_0
// TaskManagerPolicy.cs
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

namespace System.Threading.Tasks
{
	
	
	public class TaskManagerPolicy
	{
		static readonly int defaultIdealThreadsPerProcessor = 1;
		static readonly int defaultIdealProcessors = Environment.ProcessorCount;
		static readonly int defaultMinProcessors = 1;
		static readonly int defaultStackSize = 0;
		static readonly ThreadPriority defaultPriority = ThreadPriority.Normal;
		
		int idealProcessors;
		int idealThreadsPerProcessor;
		int minProcessors;
		int maxStackSize;
		ThreadPriority priority;
		
		public ThreadPriority ThreadPriority {
			get {
				return priority;
			}
		}
		
		public int MinProcessors {
			get {
				return minProcessors;
			}
		}
		
		public int MaxStackSize {
			get {
				return maxStackSize;
			}

		}
		
		public int IdealThreadsPerProcessor {
			get {
				return idealThreadsPerProcessor;
			}

		}
		
		public int IdealProcessors {
			get {
				return idealProcessors;
			}

		}
		
		public TaskManagerPolicy():
			this(defaultMinProcessors, defaultIdealProcessors, defaultIdealThreadsPerProcessor, defaultStackSize, defaultPriority) 
		{
		}
		
		public TaskManagerPolicy(int minProcessors, int idealProcessors):
			this(minProcessors, idealProcessors, defaultIdealThreadsPerProcessor, defaultStackSize, defaultPriority)
		{
			
		}
		
		public TaskManagerPolicy(int minProcessors, int idealProcessors, int idealThreadsPerProcessor):
			this(minProcessors, idealProcessors, idealThreadsPerProcessor, defaultStackSize, defaultPriority)
		{
			
		}
		
		public TaskManagerPolicy(int maxStackSize):
			this(defaultMinProcessors, defaultIdealProcessors, defaultIdealThreadsPerProcessor, maxStackSize, defaultPriority)
		{
			
		}
		
		public TaskManagerPolicy(int minProcessors, int idealProcessors, ThreadPriority priority):
			this(minProcessors, idealProcessors, defaultIdealThreadsPerProcessor, defaultStackSize, priority)
		{
			
		}
		
		public TaskManagerPolicy(int minProcessors, int idealProcessors,
		                         int idealThreadsPerProcessor, int maxStackSize, ThreadPriority priority)
		{
			this.minProcessors = minProcessors;
			this.idealProcessors = idealProcessors;
			this.idealThreadsPerProcessor = idealThreadsPerProcessor;
			this.maxStackSize = maxStackSize;
			this.priority = priority;
		}
	}
}
#endif
