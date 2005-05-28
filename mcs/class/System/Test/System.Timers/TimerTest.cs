//
// TimerTest.cs - NUnit Test Cases for System.Timers.Timer
//
// Author:
//   Kornél Pál <http://www.kornelpal.hu/>
//
// Copyright (C) 2005 Kornél Pál
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

using NUnit.Framework;
using System;
using System.Timers;

namespace MonoTests.System.Timers
{
	[TestFixture]
	public class TimerTest : Assertion
	{
		Timer timer;

		[SetUp]
		public void SetUp ()
		{
			timer = new Timer();
		}

		[TearDown]
		public void TearDown ()
		{
		}

		[Test]
		public void StartStopEnabled ()
		{
			timer.Start();
			Assert ("#A01 !Enabled after Start()", timer.Enabled);
			timer.Stop();
			Assert ("#A02 Enabled after Stop()", !timer.Enabled);
		}

		[Test]
		public void CloseEnabled () {
			Assert ("#A01 Enabled after created", !timer.Enabled);
			timer.Enabled = true;
			Assert ("#A02 !Enabled after Enabled = true", timer.Enabled);
			timer.Close();
			Assert ("#A02 Enabled after Close()", !timer.Enabled);
		}
	}
}