//
// SystemEventsCas.cs - CAS unit tests for Microsoft.Win32.SystemEvents
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
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

using NUnit.Framework;

using System;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using Microsoft.Win32;

namespace MonoCasTests.Microsoft.Win32 {

	[TestFixture]
	[Category ("CAS")]
	public class SystemEventsCas {

		[SetUp]
		public virtual void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
		}

		private void TimerCallback (object o, TimerElapsedEventArgs args)
		{
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Methods_Deny_Unrestricted ()
		{
			IntPtr timer = SystemEvents.CreateTimer (5000);
			SystemEvents.KillTimer (timer);

			try {
				SystemEvents.InvokeOnEventsThread (new TimerElapsedEventHandler (TimerCallback));
			}
			catch (NotImplementedException) {
				// mono
			}
		}

		private void EventCallback (object o, EventArgs args)
		{
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void DisplaySettingsChanged_Deny_Unrestricted ()
		{
			try {
				SystemEvents.DisplaySettingsChanged += new EventHandler (EventCallback);
				SystemEvents.DisplaySettingsChanged -= new EventHandler (EventCallback);
			}
			catch (NotImplementedException) {
				// mono
			}
		}
#if NET_2_0
		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void DisplaySettingsChanging_Deny_Unrestricted ()
		{
			try {
				SystemEvents.DisplaySettingsChanging += new EventHandler (EventCallback);
				SystemEvents.DisplaySettingsChanging -= new EventHandler (EventCallback);
			}
			catch (NotImplementedException) {
				// mono
			}
		}
#endif
		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void EventsThreadShutdown_Deny_Unrestricted ()
		{
			try {
				SystemEvents.EventsThreadShutdown += new EventHandler (EventCallback);
				SystemEvents.EventsThreadShutdown -= new EventHandler (EventCallback);
			}
			catch (NotImplementedException) {
				// mono
			}
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void InstalledFontsChanged_Deny_Unrestricted ()
		{
			try {
				SystemEvents.InstalledFontsChanged += new EventHandler (EventCallback);
				SystemEvents.InstalledFontsChanged -= new EventHandler (EventCallback);
			}
			catch (NotImplementedException) {
				// mono
			}
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void LowMemory_Deny_Unrestricted ()
		{
			try {
				SystemEvents.LowMemory += new EventHandler (EventCallback);
				SystemEvents.LowMemory -= new EventHandler (EventCallback);
			}
			catch (NotImplementedException) {
				// mono
			}
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void PaletteChanged_Deny_Unrestricted ()
		{
			try {
				SystemEvents.PaletteChanged += new EventHandler (EventCallback);
				SystemEvents.PaletteChanged -= new EventHandler (EventCallback);
			}
			catch (NotImplementedException) {
				// mono
			}
		}

		private void PowerModeChangedCallback (object o, PowerModeChangedEventArgs args)
		{
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void PowerModeChanged_Deny_Unrestricted ()
		{
			try {
				SystemEvents.PowerModeChanged += new PowerModeChangedEventHandler (PowerModeChangedCallback);
				SystemEvents.PowerModeChanged -= new PowerModeChangedEventHandler (PowerModeChangedCallback);
			}
			catch (NotImplementedException) {
				// mono
			}
		}

		private void SessionEndedCallback (object o, SessionEndedEventArgs args)
		{
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void SessionEnded_Deny_Unrestricted ()
		{
			try {
				SystemEvents.SessionEnded += new SessionEndedEventHandler (SessionEndedCallback);
				SystemEvents.SessionEnded -= new SessionEndedEventHandler (SessionEndedCallback);
			}
			catch (NotImplementedException) {
				// mono
			}
		}

		private void SessionEndingCallback (object o, SessionEndingEventArgs args)
		{
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void SessionEnding_Deny_Unrestricted ()
		{
			try {
				SystemEvents.SessionEnding += new SessionEndingEventHandler (SessionEndingCallback);
				SystemEvents.SessionEnding -= new SessionEndingEventHandler (SessionEndingCallback);
			}
			catch (NotImplementedException) {
				// mono
			}
		}
#if NET_2_0
		private void SessionSwitchCallback (object o, SessionSwitchEventArgs args)
		{
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void SessionSwitch_Deny_Unrestricted ()
		{
			try {
				SystemEvents.SessionSwitch += new SessionSwitchEventHandler (SessionSwitchCallback);
				SystemEvents.SessionSwitch -= new SessionSwitchEventHandler (SessionSwitchCallback);
			}
			catch (NotImplementedException) {
				// mono
			}
		}
#endif
		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void TimeChanged_Deny_Unrestricted ()
		{
			try {
				SystemEvents.TimeChanged += new EventHandler (EventCallback);
				SystemEvents.TimeChanged -= new EventHandler (EventCallback);
			}
			catch (NotImplementedException) {
				// mono
			}
		}

		private void TimerElapsedCallback (object o, TimerElapsedEventArgs args)
		{
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void TimerElapsed_Deny_Unrestricted ()
		{
			SystemEvents.TimerElapsed += new TimerElapsedEventHandler (TimerElapsedCallback);
			SystemEvents.TimerElapsed -= new TimerElapsedEventHandler (TimerElapsedCallback);
		}

		private void UserPreferenceChangedCallback (object o, UserPreferenceChangedEventArgs args)
		{
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void UserPreferenceChanged_Deny_Unrestricted ()
		{
			SystemEvents.UserPreferenceChanged += new UserPreferenceChangedEventHandler (UserPreferenceChangedCallback);
			SystemEvents.UserPreferenceChanged -= new UserPreferenceChangedEventHandler (UserPreferenceChangedCallback);
		}

		private void UserPreferenceChangingCallback (object o, UserPreferenceChangingEventArgs args)
		{
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void UserPreferenceChanging_Deny_Unrestricted ()
		{
			try {
				SystemEvents.UserPreferenceChanging += new UserPreferenceChangingEventHandler (UserPreferenceChangingCallback);
				SystemEvents.UserPreferenceChanging -= new UserPreferenceChangingEventHandler (UserPreferenceChangingCallback);
			}
			catch (NotImplementedException) {
				// mono
			}
		}

		// LinkDemand

		// we use reflection to call this class as it is protected by a LinkDemand 
		// (which will be converted into full demand, i.e. a stack walk) when 
		// reflection is used (i.e. it gets testable).

		public virtual object Create ()
		{
			MethodInfo mi = typeof (SystemEvents).GetMethod ("CreateTimer");
			Assert.IsNotNull (mi, "CreateTimer");
			return mi.Invoke (null, new object[1] { 5000 });
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void LinkDemand_Deny_Unrestricted ()
		{
			Assert.IsNotNull (Create ());
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Read = "MONO")]
		[ExpectedException (typeof (SecurityException))]
		public void LinkDemand_Deny_Anything ()
		{
			// denying any permissions -> not full trust!
			Assert.IsNotNull (Create ());
		}

		[Test]
		[PermissionSet (SecurityAction.PermitOnly, Unrestricted = true)]
		public void LinkDemand_PermitOnly_Unrestricted ()
		{
			Assert.IsNotNull (Create ());
		}
	}
}
