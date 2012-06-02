//
// Microsoft.Win32.SystemEvents.cs
//
// Authors:
//   Johannes Roith (johannes@jroith.de)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002 Johannes Roith
// (C) 2003 Andreas Nahr
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.ComponentModel;
using System.Security.Permissions;
using System.Timers;

namespace Microsoft.Win32 {

	[PermissionSet (SecurityAction.LinkDemand, Unrestricted = true)]
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
				TimerElapsed (null, new TimerElapsedEventArgs (IntPtr.Zero));
		}

		[MonoTODO]
		public static void InvokeOnEventsThread(Delegate method)
		{
			throw new System.NotImplementedException ();
		}

		[MonoTODO]
		public static event System.EventHandler DisplaySettingsChanged 
		{
			add 	{ }
			remove 	{ }
		}
		[MonoTODO("Currently does nothing on Mono")]
		public static event EventHandler DisplaySettingsChanging {
			add {  }
			remove { }
		}
		[MonoTODO("Currently does nothing on Mono")]
		public static event System.EventHandler EventsThreadShutdown 
		{
			add 	{ }
			remove 	{ }
		}

		[MonoTODO("Currently does nothing on Mono")]
		public static event System.EventHandler InstalledFontsChanged 
		{
			add 	{ }
			remove 	{ }
		}

		[MonoTODO("Currently does nothing on Mono")]
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("")]
		public static event System.EventHandler LowMemory 
		{
			add 	{ }
			remove 	{ }
		}

		[MonoTODO("Currently does nothing on Mono")]
		public static event System.EventHandler PaletteChanged 
		{
			add 	{ }
			remove 	{ }
		}

		[MonoTODO("Currently does nothing on Mono")]
		public static event PowerModeChangedEventHandler PowerModeChanged 
		{
			add 	{ }
			remove 	{ }
		}

		[MonoTODO("Currently does nothing on Mono")]
		public static event SessionEndedEventHandler SessionEnded 
		{
			add 	{ }
			remove 	{ }
		}

		[MonoTODO("Currently does nothing on Mono")]
		public static event SessionEndingEventHandler SessionEnding 
		{
			add 	{ }
			remove 	{ }
		}
		[MonoTODO("Currently does nothing on Mono")]
		public static event SessionSwitchEventHandler SessionSwitch {
			add    { }
			remove { }
		}

		[MonoTODO("Currently does nothing on Mono")]
		public static event System.EventHandler TimeChanged 
		{
			add 	{ }
			remove 	{ }
		}

		public static event TimerElapsedEventHandler TimerElapsed;

		[MonoTODO("Currently does nothing on Mono")]
		public static event UserPreferenceChangedEventHandler UserPreferenceChanged 
		{
			add 	{ }
			remove 	{ }
		}

		[MonoTODO("Currently does nothing on Mono")]
		public static event UserPreferenceChangingEventHandler UserPreferenceChanging 
		{
			add 	{ }
			remove 	{ }
		}
	}
}
