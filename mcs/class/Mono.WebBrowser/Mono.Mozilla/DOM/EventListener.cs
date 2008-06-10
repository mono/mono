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
// Copyright (c) 2008 Novell, Inc.
//
// Authors:
//	Andreia Gaita (avidigal@novell.com)
//

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Mono.Mozilla.DOM
{
	internal class EventListener : nsIDOMEventListener
	{
		private HandleRef storage;
		private bool disposed = false;
		private object owner;

		private EventHandlerList events;
		public EventHandlerList Events {
			get { 
				if (events == null)
					events = new EventHandlerList ();
				return events;
			}
		}
		
		private nsIDOMEventTarget target;
		public nsIDOMEventTarget Target {
			get { return target; }
			set { target = value; }
		}
		
		public EventListener(nsIDOMEventTarget target, object owner)
		{
			this.target = target;
			this.owner = owner;
			IntPtr p = Base.StringInit ();
			storage = new HandleRef (this, p);
			
		}

		~EventListener ()
		{
			Dispose (false);
		}

		#region IDisposable Members

		protected virtual void Dispose (bool disposing)
		{
			if (!disposed) {
				if (disposing) {
					Base.StringFinish (storage);
				}
				disposed = true;
			}
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		#endregion

		
		public void AddHandler (EventHandler handler, string _event)
		{
			string key = String.Intern (target.GetHashCode() + ":" + _event);
			Events.AddHandler (key, handler);
			Base.StringSet (storage, _event);
			target.addEventListener (storage, this, true);
		}

		public void RemoveHandler (EventHandler handler, string _event)
		{
			string key = String.Intern (target.GetHashCode() + ":" + _event);
			Events.RemoveHandler (key, handler);
			Base.StringSet (storage, _event);
			target.removeEventListener (storage, this, true);
		}
		

		public void AddHandler (Mono.WebBrowser.DOM.NodeEventHandler handler, string _event)
		{
			string key = String.Intern (target.GetHashCode() + ":" + _event);
			Events.AddHandler (key, handler);
			Base.StringSet (storage, _event);
			target.addEventListener (storage, this, true);
		}

		public void RemoveHandler (Mono.WebBrowser.DOM.NodeEventHandler handler, string _event)
		{
			string key = String.Intern (target.GetHashCode() + ":" + _event);
			Events.RemoveHandler (key, handler);
			Base.StringSet (storage, _event);
			target.removeEventListener (storage, this, true);
		}

		public int handleEvent (nsIDOMEvent _event) 
		{
			_event.getType (storage);
			string type = Base.StringGet (storage);
			string key = String.Intern (target.GetHashCode () + ":" + type);
			EventHandler eh = Events[key] as EventHandler;
			if (eh != null) {
				eh (owner, new EventArgs ());
				return 0;
			}
			Mono.WebBrowser.DOM.NodeEventHandler eh1 = Events[key] as Mono.WebBrowser.DOM.NodeEventHandler;
			if (eh1 != null) {
				eh1 (owner, new Mono.WebBrowser.DOM.NodeEventArgs ((Mono.WebBrowser.DOM.INode)owner));
				return 0;
			}

			Mono.WebBrowser.DOM.WindowEventHandler eh2 = Events[key] as Mono.WebBrowser.DOM.WindowEventHandler;
			if (eh2 != null) {
				eh2 (owner, new Mono.WebBrowser.DOM.WindowEventArgs ((Mono.WebBrowser.DOM.IWindow)owner));
				return 0;
			}
			
			return 0;
		}
	}
}
