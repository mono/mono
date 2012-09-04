//
// TraceEventCache.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc.
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

using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace System.Diagnostics
{
	public class TraceEventCache
	{
		DateTime started;
		CorrelationManager manager;
		string callstack, thread;
		int process;
		long timestamp;

		public TraceEventCache ()
		{
			started = DateTime.Now;
			manager = Trace.CorrelationManager;
			callstack = Environment.StackTrace;
			timestamp = Stopwatch.GetTimestamp ();
			thread = Thread.CurrentThread.Name;
			process = Process.GetCurrentProcess ().Id;
		}

		public string Callstack {
			get { return callstack; }
		}

		public DateTime DateTime {
			get { return started; }
		}

		public Stack LogicalOperationStack {
			get { return manager.LogicalOperationStack; }
		}

		public int ProcessId {
			get { return process; }
		}

		public string ThreadId {
			get { return thread; }
		}

		public long Timestamp {
			get { return timestamp; }
		}
	}
}
