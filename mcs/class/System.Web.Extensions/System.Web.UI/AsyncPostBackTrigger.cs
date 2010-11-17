//
// AsyncPostBackTrigger.cs
//
// Author:
//   Igor Zelmanovich <igorz@mainsoft.com>
//
// (C) 2007 Mainsoft, Inc.  http://www.mainsoft.com
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
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Reflection;

namespace System.Web.UI
{
	public class AsyncPostBackTrigger : UpdatePanelControlTrigger
	{
		static readonly MethodInfo _eventHandler = typeof (AsyncPostBackTrigger).GetMethod ("OnEvent");

		string _eventName;

		public new string ControlID {
			get {
				return base.ControlID;
			}
			set {
				base.ControlID = value;
			}
		}

		[DefaultValue ("")]
		[Category ("Behavior")]
		public string EventName {
			get {
				if (_eventName == null)
					return String.Empty;
				return _eventName;
			}
			set {
				_eventName = value;
			}
		}

		protected internal override bool HasTriggered ()
		{
			Control ctrl = FindTargetControl (true);
			string ctrlUniqueID = ctrl != null ? ctrl.UniqueID : null;
			if (ctrlUniqueID == null)
				return false;

			string asyncPostBackElementID = Owner.ScriptManager.AsyncPostBackSourceElementID;
			if (String.Compare (asyncPostBackElementID, ctrlUniqueID, StringComparison.Ordinal) == 0)
				return true;
			else if (asyncPostBackElementID.StartsWith (ctrlUniqueID + "$", StringComparison.Ordinal))
				return true;
			
			return false;
		}

		// LAME SPEC: it seems DefaultEventAttribute is never queried for the event name.
		protected internal override void Initialize ()
		{
			Control c = FindTargetControl (true);
			ScriptManager sm = Owner.ScriptManager;
			string eventName = EventName;

			if (!String.IsNullOrEmpty (eventName)) {
				EventInfo evi = c.GetType ().GetEvent (eventName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
				if (evi == null)
					throw new InvalidOperationException (String.Format ("Could not find an event named '{0}' on associated control '{1}' for the trigger in UpdatePanel '{2}'.", eventName, c.ID, Owner.ID));

				Delegate d = null;
				try {
					d = Delegate.CreateDelegate (evi.EventHandlerType, this, _eventHandler);
				}
				catch (ArgumentException) {
					throw new InvalidOperationException (String.Format ("The event '{0}' in '{1}' for the control '{2}' does not match a standard event handler signature.", eventName, c.GetType (), c.ID));
				}

				evi.AddEventHandler (c, d);
			}
			
			sm.RegisterAsyncPostBackControl (c);
		}

		public void OnEvent (object sender, EventArgs e)
		{
			UpdatePanel owner = Owner;
			if (owner != null && owner.UpdateMode != UpdatePanelUpdateMode.Always)
				owner.Update ();
		}

		public override string ToString () {
			return String.Format ("AsyncPostBackTrigger: {0}.{1}", ControlID, EventName);
		}
	}
}