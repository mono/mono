// 
// Barrier.cs
//  
// Author:
//       Jérémie "Garuma" Laval <jeremie.laval@gmail.com>
// 
// Copyright (c) 2009 Jérémie "Garuma" Laval
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

#if NET_4_0 || BOOTSTRAP_NET_4_0

using System;

namespace System.Threading
{
	public class Barrier : IDisposable
	{
		const int MaxParticipants = 32767;
		Action<Barrier> postPhaseAction;
		
		int participants;
		bool cleaned;
		CountdownEvent cntd;
		ManualResetEventSlim postPhaseEvt = new ManualResetEventSlim ();
		long phase;
		
		public Barrier (int participants) : this (participants, null)
		{
		}
		
		public Barrier (int participants, Action<Barrier> postPhaseAction)
		{
			if (participants < 0 || participants > MaxParticipants)
				throw new ArgumentOutOfRangeException ("participants");
			
			this.participants = participants;
			this.postPhaseAction = postPhaseAction;
			
			InitCountdownEvent ();
		}

		public void Dispose ()
		{
			Dispose (true);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (disposing){
				if (cntd != null){
					cntd.Dispose ();
					cntd = null;
				}
				postPhaseAction = null;
				cleaned = true;
			}
		}
			
		void InitCountdownEvent ()
		{
			postPhaseEvt = new ManualResetEventSlim (false);
			cntd = new CountdownEvent (participants);
		}
		
		public long AddParticipant ()
		{
			return AddParticipants (1);
		}

		static Exception GetDisposed ()
		{
			return new ObjectDisposedException ("Barrier");
		}
		
		public long AddParticipants (int participantCount)
		{
			if (cleaned)
				throw GetDisposed ();
			
			if (participantCount < 0)
				throw new InvalidOperationException ();
			
			// Basically, we try to add ourselves and return
			// the phase. If the call return false, we repeatdly try
			// to add ourselves for the next phase
			do {
				if (cntd.TryAddCount (participantCount)) {
					Interlocked.Add (ref participants, participantCount);
					return phase;
				}
			} while (true);
		}
		
		public void RemoveParticipant ()
		{
			RemoveParticipants (1);
		}
		
		public void RemoveParticipants (int participantCount)
		{
			if (cleaned)
				throw GetDisposed ();
			if (participantCount < 0)
				throw new ArgumentOutOfRangeException ("participantCount");
			
			if (cntd.Signal (participantCount))
				PostPhaseAction (postPhaseEvt);

			Interlocked.Add (ref participants, -participantCount);
		}
		
		public void SignalAndWait ()
		{
			if (cleaned)
				throw GetDisposed ();
			SignalAndWait ((c) => { c.Wait (); return true; });
		}
		
		public void SignalAndWait (CancellationToken token)
		{
			if (cleaned)
				throw GetDisposed ();
			SignalAndWait ((c) => { c.Wait (token); return true; });
		}

		public bool SignalAndWait (int millisecondTimeout)
		{
			if (cleaned)
				throw GetDisposed ();
			return SignalAndWait ((c) => c.Wait (millisecondTimeout));
		}

		public bool SignalAndWait (TimeSpan ts)
		{
			if (cleaned)
				throw GetDisposed ();
			return SignalAndWait ((c) => c.Wait (ts));
		}
		
		public bool SignalAndWait (int millisecondTimeout, CancellationToken token)
		{
			if (cleaned)
				throw GetDisposed ();
			return SignalAndWait ((c) => c.Wait (millisecondTimeout, token));
		}
		
		public bool SignalAndWait (TimeSpan ts, CancellationToken token)
		{
			if (cleaned)
				throw GetDisposed ();
			return SignalAndWait ((c) => c.Wait (ts, token));
		}
		
		bool SignalAndWait (Func<CountdownEvent, bool> associate)
		{
			bool result;
			CountdownEvent temp = cntd;
			ManualResetEventSlim evt = postPhaseEvt;
			
			if (!temp.Signal ()) {
				result = Wait (associate, temp, evt);
			} else {
				result = true;
				PostPhaseAction (evt);
			}
			
			return result;
		}
		
		bool Wait (Func<CountdownEvent, bool> associate, CountdownEvent temp, ManualResetEventSlim evt)
		{
			if (!associate (temp))
				return false;
			
			evt.Wait ();
			
			return true;
		}
		
		void PostPhaseAction (ManualResetEventSlim evt)
		{
			if (postPhaseAction != null) {
				try {
					postPhaseAction (this);
				} catch (Exception e) {
					throw new BarrierPostPhaseException (e);
				}
			}
			
			InitCountdownEvent ();
			phase++;

			evt.Set ();
		}
		
		public long CurrentPhaseNumber {
			get {
				return phase;
			}
		}
		
		public int ParticipantCount  {
			get {
				return participants;
			}
		}
	}
}
#endif
