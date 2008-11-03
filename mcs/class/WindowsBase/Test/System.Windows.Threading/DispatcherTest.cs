//
// Tests for System.Windows.Threading.Dispatcher.cs
//
// Author:
//    Miguel de Icaza (miguel@novell.com)
//

//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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

using NUnit.Framework;
using System;
using System.IO;
using System.Globalization;
using System.Windows.Threading;
using System.Threading;

namespace MonoTests.System.Windows.Threading
{

	delegate void Action ();
	
	[TestFixture]
	public class DispatcherTest {
		EventWaitHandle wait, wait2;
		DispatcherOperation op;
		
		[SetUp]
		public void DispatcherSetup ()
		{
			wait = new EventWaitHandle (false, EventResetMode.AutoReset);
			wait2 = new EventWaitHandle (false, EventResetMode.AutoReset);
		}

		//
		// Tests that fields in a DispatcherOperation can be issued from
		// a separate thread
		//
		[Test]
		public void TestDispatcherOpOnThread ()
		{
			Thread t = new Thread (new ThreadStart (thread));
			Dispatcher d = Dispatcher.CurrentDispatcher;
			
			t.Start ();
			op = Dispatcher.CurrentDispatcher.BeginInvoke (DispatcherPriority.Normal, (Action) delegate {
				Console.WriteLine ("Some methods");
			});
			wait.Set ();
			wait2.WaitOne ();
		}

		void thread ()
		{
			wait.WaitOne ();
			op.Priority = DispatcherPriority.DataBind;
			wait2.Set ();
		}

		[Test]
		public void TestDispatcherOrder ()
		{
			Dispatcher d = Dispatcher.CurrentDispatcher;

			DispatcherFrame frame = new DispatcherFrame ();
			bool fail = true;
			int next = 1;
			
			d.BeginInvoke (DispatcherPriority.Normal, (Action) delegate {
				if (next != 3)
					throw new Exception ("Expected state 3, got " + next.ToString ());

				next = 4;
				Console.WriteLine ("First");
			});
			d.BeginInvoke (DispatcherPriority.Normal, (Action) delegate {
				if (next != 4)
					throw new Exception ("Expected state 4, got " + next.ToString ());

				next = 5;
				Console.WriteLine ("Second");
			});
			d.BeginInvoke (DispatcherPriority.Send, (Action) delegate {
				if (next != 1)
					throw new Exception ("Expected state 1, got " + next.ToString ());
				next = 2;
				Console.WriteLine ("High Priority");
				d.BeginInvoke (DispatcherPriority.Send, (Action) delegate {
					if (next != 2)
						throw new Exception ("Expected state 2, got " + next.ToString ());

					next = 3;
					Console.WriteLine ("INSERTED");
				});
			});
			d.BeginInvoke (DispatcherPriority.SystemIdle, (Action) delegate {
				if (next != 6)
					throw new Exception ("Expected state 6, got " + next.ToString ());
				
				Console.WriteLine ("Idle");
				frame.Continue = false;
				fail = false;
			});
			
			d.BeginInvoke (DispatcherPriority.Normal, (Action) delegate {
				if (next != 5)
					throw new Exception ("Expected state 5, got " + next.ToString ());
				next = 6;
				Console.WriteLine ("Last normal");
			});
			
			Dispatcher.PushFrame (frame);

			if (fail)
				throw new Exception ("Expected all states to run");
		}
	}
}


