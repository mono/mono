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
// Copyright (c) 2007, 2008 Novell, Inc.
//
// Authors:
//	Andreia Gaita (avidigal@novell.com)
//


using System;
using System.Drawing;
using System.ComponentModel;

using Mono.WebBrowser.DOM;

namespace System.Windows.Forms
{
	public sealed class HtmlWindow
	{
		private EventHandlerList event_handlers;
		private IWindow window;
		private Mono.WebBrowser.IWebBrowser webHost;
		private WebBrowser owner;
		
		internal HtmlWindow (WebBrowser owner, Mono.WebBrowser.IWebBrowser webHost, IWindow iWindow)
		{
			this.window = iWindow;
			this.webHost = webHost;
			this.owner = owner;
			this.window.Load += new EventHandler (OnLoad);
			this.window.Unload += new EventHandler (OnUnload);
		}

		private EventHandlerList Events {
			get {
				// Note: space vs. time tradeoff
				// We create the object here if it's never be accessed before.  This potentially 
				// saves space. However, we must check each time the propery is accessed to
				// determine whether we need to create the object, which increases overhead.
				// We could put the creation in the contructor, but that would waste space
				// if it were never used.  However, accessing this property would be faster.
				if (null == event_handlers)
					event_handlers = new EventHandlerList();

				return event_handlers;
			}
		}

#region Properties
		public HtmlDocument Document {
			get { return new HtmlDocument (owner, webHost, this.window.Document); }
		}
		
		public object DomWindow {
			get { throw new NotSupportedException ("Retrieving a reference to an mshtml interface is not supported. Sorry."); } 
		}

		public HtmlWindowCollection Frames {
			get { return new HtmlWindowCollection (owner, webHost, this.window.Frames); }
		}

		public HtmlHistory History {
			get { return new HtmlHistory (this.webHost, window.History); }
		}

		[MonoTODO ("Windows are always open")]
		public bool IsClosed {
			get { return false; }
		}

		public string Name {
			get { return this.window.Name; }
			set { this.window.Name = value; }
		}
		
		[MonoTODO ("Separate windows are not supported yet")]
		public HtmlWindow Opener {
			get { return null; }
		}

		public HtmlWindow Parent {
			get { return new HtmlWindow (owner, webHost, this.window.Parent); }
		}

		public Point Position {
			get { return owner.Location; }
		}

		public Size Size {
			get { return owner.Size; }
			set { }
		}
		
		public string StatusBarText {
			get { return this.window.StatusText; }
			set { }
		}

		public HtmlElement WindowFrameElement {
			get { 
				return new HtmlElement (owner, webHost, window.Document.DocumentElement);
			}
		}
		
		public Uri Url {
			get { return this.Document.Url; }
		}
#endregion

#region Methods
		public void Alert (string message) 
		{
			MessageBox.Show ("Alert", message);
		}

		public bool Confirm (string message) 
		{
			DialogResult ret = MessageBox.Show (message, "Confirm", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
			return ret == DialogResult.OK;
		}
		
		public string Prompt (string message, string defaultInputValue)
		{
			WebBrowserDialogs.Prompt prompt = new WebBrowserDialogs.Prompt ("Prompt", message, defaultInputValue);
			prompt.Show ();
			return prompt.Text;
		}
		
		public void Navigate (string urlString)
		{
			webHost.Navigation.Go (urlString);
		}

		public void Navigate (Uri url)
		{
			webHost.Navigation.Go (url.ToString ());
		}
		
		public void ScrollTo (Point point)
		{
			ScrollTo (point.X, point.Y);
		}

		public void ScrollTo (int x, int y)
		{
			this.window.ScrollTo (x, y);
		}

		[MonoTODO("Blank opens in current window at the moment. Missing media and search implementations. No options implemented")]
		public HtmlWindow Open (Uri url, string target, string windowOptions, bool replaceEntry)
		{
			return Open (url.ToString(), target, windowOptions, replaceEntry);
		}

		[MonoTODO("Blank opens in current window at the moment. Missing media and search implementations. No options implemented")]
		public HtmlWindow Open (string urlString, string target, string windowOptions, bool replaceEntry)
		{
			switch (target) {
				case "_blank":
					this.window.Open (urlString);
					break;
				case "_media":
					break;
				case "_parent":
					this.window.Parent.Open (urlString);
					break;
				case "_search":
					break;
				case "_self":
					this.window.Open (urlString);
					break;
				case "_top":
					this.window.Top.Open (urlString);
					break;
			}
			return this;
		}
		
		[MonoTODO("Opens in current window at the moment.")]
		public HtmlWindow OpenNew (string urlString, string windowOptions)
		{
			return Open (urlString, "_blank", windowOptions, false);
		}

		[MonoTODO("Opens in current window at the moment.")]
		public HtmlWindow OpenNew (Uri url, string windowOptions)
		{
			return OpenNew (url.ToString (), windowOptions);
		}

		public void AttachEventHandler (string eventName, EventHandler eventHandler)
		{
			this.window.AttachEventHandler (eventName, eventHandler);
		}
		
		public void Close ()
		{
			throw new NotImplementedException ();
		}
		
		public void DetachEventHandler (string eventName, EventHandler eventHandler)
		{
			this.window.DetachEventHandler (eventName, eventHandler);
		}
		
		public void Focus ()
		{
			this.window.Focus ();
		}
		
		public void MoveTo (Point point)
		{
			throw new NotImplementedException ();
		}
		
		public void MoveTo (int x, int y)
		{
			throw new NotImplementedException ();
		}
		
		public void RemoveFocus ()
		{
			webHost.FocusOut ();
		}
		
		public void ResizeTo (Size size)
		{
			throw new NotImplementedException ();
		}
		
		public void ResizeTo (int width, int height)
		{
			throw new NotImplementedException ();
		}
#endregion

#region Events
		static object ErrorEvent = new object ();
		public event HtmlElementErrorEventHandler Error
		{
			add { 
				Events.AddHandler (ErrorEvent, value); 
				window.Error += new EventHandler (OnError);
			}
			remove { 
				Events.RemoveHandler (ErrorEvent, value); 
				window.Error -= new EventHandler (OnError);
			}
		}

		internal void OnError (object sender, EventArgs ev)
		{
			HtmlElementErrorEventHandler eh = (HtmlElementErrorEventHandler) (Events[ErrorEvent]);
			if (eh != null) {
				HtmlElementErrorEventArgs e = new HtmlElementErrorEventArgs (String.Empty, 0, null);
				eh (this, e);
			}
		}

		static object GotFocusEvent = new object ();
		public event HtmlElementEventHandler GotFocus
		{
			add { 
				Events.AddHandler (GotFocusEvent, value);
				window.OnFocus += new EventHandler (OnGotFocus);
			}
			remove { 
				Events.RemoveHandler (GotFocusEvent, value); 
				window.OnFocus -= new EventHandler (OnGotFocus);
			}
		}

		internal void OnGotFocus (object sender, EventArgs ev)
		{
			HtmlElementEventHandler eh = (HtmlElementEventHandler) (Events[GotFocusEvent]);
			if (eh != null) {
				HtmlElementEventArgs e = new HtmlElementEventArgs ();
				eh (this, e);
			}		
		}

		static object LostFocusEvent = new object ();
		public event HtmlElementEventHandler LostFocus
		{
			add { 
				Events.AddHandler (LostFocusEvent, value); 
				window.OnBlur += new EventHandler (OnLostFocus);
			}
			remove { 
				Events.RemoveHandler (LostFocusEvent, value); 
				window.OnBlur -= new EventHandler (OnLostFocus);
			}
		}

		internal void OnLostFocus (object sender, EventArgs ev)
		{
			HtmlElementEventHandler eh = (HtmlElementEventHandler) (Events[LostFocusEvent]);
			if (eh != null) {
				HtmlElementEventArgs e = new HtmlElementEventArgs ();
				eh (this, e);
			}
		}

		static object LoadEvent = new object ();
		public event HtmlElementEventHandler Load
		{
			add { 
				Events.AddHandler (LoadEvent, value); 
				window.Load += new EventHandler (OnLoad);
			}
			remove { 
				Events.RemoveHandler (LoadEvent, value); 
				window.Load -= new EventHandler (OnLoad);
			}
		}

		internal void OnLoad (object sender, EventArgs ev)
		{
			HtmlElementEventHandler eh = (HtmlElementEventHandler) (Events[LoadEvent]);
			if (eh != null) {
				HtmlElementEventArgs e = new HtmlElementEventArgs ();
				eh (this, e);
			}
		}

		static object UnloadEvent = new object ();
		public event HtmlElementEventHandler Unload {
			add { 
				Events.AddHandler (UnloadEvent, value); 
				window.Unload += new EventHandler (OnUnload);
			}
			remove { 
				Events.RemoveHandler (UnloadEvent, value); 
				window.Unload -= new EventHandler (OnUnload);
			}
		}

		internal void OnUnload (object sender, EventArgs ev)
		{
			HtmlElementEventHandler eh = (HtmlElementEventHandler) (Events[UnloadEvent]);
			if (eh != null) {
				HtmlElementEventArgs e = new HtmlElementEventArgs ();
				eh (this, e);
			}
		}

		static object ScrollEvent = new object ();
		public event HtmlElementEventHandler Scroll {
			add { 
				Events.AddHandler (ScrollEvent, value);
				window.Scroll += new EventHandler (OnScroll);
			}
			remove { 
				Events.RemoveHandler (ScrollEvent, value);
				window.Scroll -= new EventHandler (OnScroll);
			}
		}

		internal void OnScroll (object sender, EventArgs ev)
		{
			HtmlElementEventHandler eh = (HtmlElementEventHandler) (Events[ScrollEvent]);
			if (eh != null) {
				HtmlElementEventArgs e = new HtmlElementEventArgs ();
				eh (this, e);
			}
		}

		static object ResizeEvent = new object ();
		public event HtmlElementEventHandler Resize {
			add { Events.AddHandler (ResizeEvent, value); }
			remove { Events.RemoveHandler (ResizeEvent, value); }
		}

		internal void OnResize (object sender, EventArgs ev)
		{
			HtmlElementEventHandler eh = (HtmlElementEventHandler) (Events[ResizeEvent]);
			if (eh != null) {
				HtmlElementEventArgs e = new HtmlElementEventArgs ();
				eh (this, e);
			}
		}
#endregion

#region Standard stuff
		public override int GetHashCode ()
		{
			if (window == null)
				return 0;						
			return window.GetHashCode ();
		}

		public override bool Equals (object obj)
		{
			return this == (HtmlWindow) obj;
		}
		
		public static bool operator == (HtmlWindow left, HtmlWindow right)
		{
			if ((object)left == (object)right)
				return true;

			if ((object)left == null || (object)right == null)
				return false;

			return left.window.Equals (right.window);
		}

		public static bool operator != (HtmlWindow left, HtmlWindow right)
		{
			return !(left == right);
		}
#endregion
	}
}
