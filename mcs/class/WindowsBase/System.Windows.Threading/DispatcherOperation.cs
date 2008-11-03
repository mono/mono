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
// Copyright (c) 2006 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Miguel de Icaza (miguel@novell.com)
//
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security;
using System.Threading;

namespace System.Windows.Threading {

	public sealed class DispatcherOperation {
		DispatcherOperationStatus status;
		DispatcherPriority priority;
		Dispatcher dispatcher;
		object result;
		Delegate delegate_method;
		object [] delegate_args;

		internal DispatcherOperation (Dispatcher dis, DispatcherPriority prio)
		{
			dispatcher = dis;
			priority = prio;
			if (Dispatcher.HasShutdownFinished)
				status = DispatcherOperationStatus.Aborted;
			else
				status = DispatcherOperationStatus.Pending;
		}
		
		internal DispatcherOperation (Dispatcher dis, DispatcherPriority prio, Delegate d)
			: this (dis, prio)
		{
			delegate_method = d;
		}

		internal DispatcherOperation (Dispatcher dis, DispatcherPriority prio, Delegate d, object arg)
			: this (dis, prio)
		{
			delegate_method = d;
			delegate_args = new object [1];
			delegate_args [0] = arg;
		}

		internal DispatcherOperation (Dispatcher dis, DispatcherPriority prio, Delegate d, object arg, object [] args)
			: this (dis, prio)
		{
			delegate_method = d;
			delegate_args = new object [args.Length + 1];
			delegate_args [0] = arg;
			Array.Copy (args, 1, delegate_args, 0, args.Length);
		}

		internal void Invoke ()
		{
			status = DispatcherOperationStatus.Executing;
			result = delegate_method.DynamicInvoke (delegate_args);
				
			status = DispatcherOperationStatus.Completed;

			if (Completed != null)
				Completed (this, EventArgs.Empty);
		}
		
		public bool Abort ()
		{
			status = DispatcherOperationStatus.Aborted;
			throw new NotImplementedException ();
		}

		public DispatcherOperationStatus Status {
			get {
				return status;
			}

			internal set {
				status = value;
			}
		}

		public Dispatcher Dispatcher {
			get {
				return dispatcher;
			}
		}

		public DispatcherPriority Priority {
			get {
				return priority;
			}

			set {
				if (priority != value){
					DispatcherPriority old = priority;
					priority = value;
					dispatcher.Reprioritize (this, old);
				}
			}
		}

		public object Result {
			get {
				return result;
			}
		}

		public DispatcherOperationStatus Wait ()
		{
			if (status == DispatcherOperationStatus.Executing)
				throw new InvalidOperationException ("Already executing");

			throw new NotImplementedException ();
		}

		[SecurityCritical]
		public DispatcherOperationStatus Wait (TimeSpan timeout)
		{
			if (status == DispatcherOperationStatus.Executing)
				throw new InvalidOperationException ("Already executing");

			throw new NotImplementedException ();
		}
		
		public event EventHandler Aborted;
		public event EventHandler Completed;
	}
}
