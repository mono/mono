//
// Microsoft.Win32.SystemEvents.cs
//
// Authors:
//   Johannes Roith (johannes@jroith.de)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002 Johannes Roith
// (C) 2003 Andreas Nahr
//

using System;
using System.Timers;
using System.Collections;

namespace Microsoft.Win32 
{
	public sealed class SystemEvents 
	{
		private static Hashtable TimerStore = new Hashtable ();

		private SystemEvents ()
		{
		}

		// You can use timers using the CreateTimer, KillTimer methods and the Timerelapsed event
		// This is only a partial solution, as it only works in managed code.
		// TODO implement this on OS level
		// Till done this solution should work if you are only using the mentioned members

		public static IntPtr CreateTimer (int interval)
		{
			Guid Ident = Guid.NewGuid ();
			int IdentValue = Ident.GetHashCode ();
			Timer t = new System.Timers.Timer (interval);
			t.Elapsed += new ElapsedEventHandler (InternalTimerElapsed);
			TimerStore.Add (IdentValue, t);
			return new IntPtr (IdentValue);
		}

		public static void KillTimer (IntPtr timerId)
		{
			Timer t = (Timer) TimerStore[timerId.GetHashCode()];
			t.Stop ();
			t.Elapsed -= new ElapsedEventHandler (InternalTimerElapsed);
			t.Dispose ();
			TimerStore.Remove (timerId.GetHashCode());
		}

		private static void InternalTimerElapsed (object e, ElapsedEventArgs args)
		{
			if (TimerElapsed != null)
				TimerElapsed (null, new TimerElapsedEventArgs (new IntPtr(0)));
		}

		[MonoTODO]
		public static void InvokeOnEventsThread(Delegate method)
		{
			throw new System.NotImplementedException ();
		}

		[MonoTODO]
		public static event System.EventHandler DisplaySettingsChanged 
		{
			add 	{ throw new System.NotImplementedException ();}
			remove 	{ throw new System.NotImplementedException ();}
		}

		[MonoTODO]
		public static event System.EventHandler EventsThreadShutdown 
		{
			add 	{ throw new System.NotImplementedException ();}
			remove 	{ throw new System.NotImplementedException ();}
		}

		[MonoTODO]
		public static event System.EventHandler InstalledFontsChanged 
		{
			add 	{ throw new System.NotImplementedException ();}
			remove 	{ throw new System.NotImplementedException ();}
		}

		[MonoTODO]
		public static event System.EventHandler LowMemory 
		{
			add 	{ throw new System.NotImplementedException ();}
			remove 	{ throw new System.NotImplementedException ();}
		}

		[MonoTODO]
		public static event System.EventHandler PaletteChanged 
		{
			add 	{ throw new System.NotImplementedException ();}
			remove 	{ throw new System.NotImplementedException ();}
		}

		[MonoTODO]
		public static event PowerModeChangedEventHandler PowerModeChanged 
		{
			add 	{ throw new System.NotImplementedException ();}
			remove 	{ throw new System.NotImplementedException ();}
		}

		[MonoTODO]
		public static event SessionEndedEventHandler SessionEnded 
		{
			add 	{ throw new System.NotImplementedException ();}
			remove 	{ throw new System.NotImplementedException ();}
		}

		[MonoTODO]
		public static event SessionEndingEventHandler SessionEnding 
		{
			add 	{ throw new System.NotImplementedException ();}
			remove 	{ throw new System.NotImplementedException ();}
		}

		[MonoTODO]
		public static event System.EventHandler TimeChanged 
		{
			add 	{ throw new System.NotImplementedException ();}
			remove 	{ throw new System.NotImplementedException ();}
		}

		public static event TimerElapsedEventHandler TimerElapsed;

		[MonoTODO]
		public static event UserPreferenceChangedEventHandler UserPreferenceChanged 
		{
			add 	{ throw new System.NotImplementedException ();}
			remove 	{ throw new System.NotImplementedException ();}
		}

		[MonoTODO]
		public static event UserPreferenceChangingEventHandler UserPreferenceChanging 
		{
			add 	{ throw new System.NotImplementedException ();}
			remove 	{ throw new System.NotImplementedException ();}
		}
	}
}
