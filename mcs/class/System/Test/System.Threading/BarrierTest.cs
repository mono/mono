// BarrierTest.cs
//
// Copyright (c) 2010 Jérémie "Garuma" Laval
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

using NUnit.Framework;

using System;
using System.Threading;

namespace MonoTests.System.Threading {

	[TestFixture]
	public class BarrierTests {
		Barrier barrier;
		const int participants = 10;
		bool triggered;
		
		[SetUp]
		public void Setup ()
		{
			barrier = new Barrier (participants, PostPhaseAction);
			triggered = false;
		}
		
		void PostPhaseAction (Barrier b)
		{
			Assert.AreEqual (barrier, b, "postphase");
			triggered = true;
		}
		
		[Test]
		public void AddParticipantTest ()
		{
			barrier.AddParticipant ();
			Assert.AreEqual (participants + 1, barrier.ParticipantCount, "#1");
			barrier.AddParticipants (3);
			Assert.AreEqual (participants + 4, barrier.ParticipantCount, "#2");
		}
		
		[Test]
		public void RemoveParticipantTest ()
		{
			barrier.RemoveParticipant ();
			Assert.AreEqual (participants - 1, barrier.ParticipantCount, "#1");
			barrier.RemoveParticipants (3);
			Assert.AreEqual (participants - 4, barrier.ParticipantCount, "#2");
		}
		
		[Test]
		public void SignalTest ()
		{
			barrier.RemoveParticipants (participants - 2);
			Assert.IsFalse (barrier.SignalAndWait (1), "#1");
			barrier.SignalAndWait ();
			Assert.IsTrue (triggered, "#3");
			Assert.AreEqual (1, barrier.CurrentPhaseNumber, "#4");
		}
		
		[Test]
		public void RemoveTriggeringTest ()
		{
			barrier.RemoveParticipants (participants - 2);
			barrier.RemoveParticipants (2);
			Assert.IsTrue (triggered, "#1");
			Assert.AreEqual (1, barrier.CurrentPhaseNumber, "#2");
		}
	}
}

#endif
