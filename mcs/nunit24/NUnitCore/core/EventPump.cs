// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org/?p=license&r=2.4.
// ****************************************************************

namespace NUnit.Core
{
	using System;
	using System.Threading;

	/// <summary>
	/// EventPump pulls events out of an EventQueue and sends
	/// them to a listener. It is used to send events back to
	/// the client without using the CallContext of the test
	/// runner thread.
	/// </summary>
	public class EventPump : IDisposable
	{
		#region Instance Variables
		/// <summary>
		/// The downstream listener to which we send events
		/// </summary>
		EventListener eventListener;
		
		/// <summary>
		/// The queue that holds our events
		/// </summary>
		EventQueue events;
		
		/// <summary>
		/// Thread to do the pumping
		/// </summary>
		Thread pumpThread;

		/// <summary>
		/// Indicator that we are pumping events
		/// </summary>
		private bool pumping;
		
		/// <summary>
		/// Indicator that we are stopping
		/// </summary>
		private bool stopping;

		/// <summary>
		/// If true, stop after sending RunFinished
		/// </summary>
		private bool autostop;
		#endregion

		#region Constructor
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="eventListener">The EventListener to receive events</param>
		/// <param name="events">The event queue to pull events from</param>
		/// <param name="autostop">Set to true to stop pump after RunFinished</param>
		public EventPump( EventListener eventListener, EventQueue events, bool autostop)
		{
			this.eventListener = eventListener;
			this.events = events;
			this.autostop = autostop;
		}
		#endregion

		#region Properties
		// TODO: Replace booleans with a state?
		/// <summary>
		/// Returns true if we are pumping events
		/// </summary>
		public bool Pumping
		{
			get { return pumping; }
		}

		/// <summary>
		/// Returns true if a stop is in progress
		/// </summary>
		public bool Stopping
		{
			get { return stopping; }
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Dispose stops the pump
		/// </summary>
		public void Dispose()
		{
			Stop();
		}

		/// <summary>
		/// Start the pump
		/// </summary>
		public void Start()
		{
			if ( !this.Pumping )  // Ignore if already started
			{
				this.pumpThread = new Thread( new ThreadStart( PumpThreadProc ) );
				this.pumpThread.Name = "EventPumpThread";
				pumping = true;
				this.pumpThread.Start();
			}
		}

		/// <summary>
		/// Tell the pump to stop after emptying the queue.
		/// </summary>
		public void Stop()
		{
			if ( this.Pumping && !this.Stopping ) // Ignore extra calls
			{
				lock( events )
				{
					stopping = true;
					Monitor.Pulse( events ); // In case thread is waiting
				}
				this.pumpThread.Join();
			}
		}
		#endregion

		#region PumpThreadProc
		/// <summary>
		/// Our thread proc for removing items from the event
		/// queue and sending them on. Note that this would
		/// need to do more locking if any other thread were
		/// removing events from the queue.
		/// </summary>
		private void PumpThreadProc()
		{
			EventListener hostListeners = CoreExtensions.Host.Listeners;
			Monitor.Enter( events );
            try
            {
                while (this.events.Count > 0 || !stopping)
                {
                    while (this.events.Count > 0)
                    {
                        Event e = this.events.Dequeue();
                        e.Send(this.eventListener);
						e.Send(hostListeners);
                        if (autostop && e is RunFinishedEvent)
                            stopping = true;
                    }
                    // Will be pulsed if there are any events added
                    // or if it's time to stop the pump.
                    if (!stopping)
                        Monitor.Wait(events);
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Exception in pump thread", ex);
            }
			finally
			{
				Monitor.Exit( events );
				pumping = false;
				stopping = false;
				//pumpThread = null;
			}
		}
		#endregion
	}
}
