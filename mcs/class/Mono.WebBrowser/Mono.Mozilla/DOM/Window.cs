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
using System.Text;
using System.Runtime.InteropServices;
using System.Collections;
using Mono.WebBrowser;
using Mono.WebBrowser.DOM;

namespace Mono.Mozilla.DOM
{
	internal class Window : DOMObject, IWindow
	{
		internal nsIDOMWindow window;
		private EventListener eventListener;
		int hashcode;
		
		public Window(WebBrowser control, nsIDOMWindow domWindow) : base (control)
		{
			this.hashcode = domWindow.GetHashCode ();
			this.window = domWindow;
		}

#region IDisposable Members
		protected override  void Dispose (bool disposing)
		{
			if (!disposed) {
				if (disposing) {
					this.resources.Clear ();
					this.window = null;
				}
			}
			base.Dispose(disposing);
		}		
#endregion

		internal static bool FindDocument (ref nsIDOMWindow window, int docHashcode) {
			nsIDOMDocument doc;
			window.getDocument (out doc);
			
			if (doc.GetHashCode () == docHashcode) {
				return true;
			} else {
				uint len = 1;
				nsIDOMWindowCollection col;
	
				window.getFrames (out col);
				col.getLength (out len);

				for (uint i = 0; i < len; ++i) {
					col.item (i, out window);
					if (Window.FindDocument (ref window, docHashcode))
						return true;
				}
			}
			return false;			
		}
		
#region Properties
		public IDocument Document {
			get {
				nsIDOMDocument doc;
				this.window.getDocument (out doc);
				if (!control.documents.ContainsKey (doc.GetHashCode ()))
				    control.documents.Add (doc.GetHashCode (), new Document (control, (nsIDOMHTMLDocument) doc));
				return control.documents[doc.GetHashCode ()] as IDocument;
			}
		}
		
		public IWindowCollection Frames {
			get {
				nsIDOMWindowCollection windows;
				this.window.getFrames (out windows);
				return new WindowCollection (control, windows);
			}
		}

		public string Name {
			get {
				this.window.getName (storage);
				return Base.StringGet (storage);
			}
			set {
				Base.StringSet (storage, value);
				this.window.setName (storage);
			}
		}
		
		public IWindow Parent {
			get {
				nsIDOMWindow parent;
				this.window.getParent (out parent);
				return new Window (control, parent);
			}
		}
		
		public IWindow Top {
			get {
				nsIDOMWindow top;
				this.window.getTop (out top);
				return new Window (control, top);
			}
		}
		
		public string StatusText {
			get {
				return control.StatusText;
				
			}
		}
		
		public IHistory History {
			get {
				Navigation nav = new Navigation (this.control, window as nsIWebNavigation);
				return new History (this.control, nav);
			}
		}
#endregion

#region Methods
		private EventListener EventListener {
			get {
				if (eventListener == null)
					eventListener = new EventListener (this.window as nsIDOMEventTarget, this);
				return eventListener;
			}
		}
		
		public void AttachEventHandler (string eventName, EventHandler handler) 
		{
			EventListener.AddHandler (handler, eventName);			
		}
		
		public void DetachEventHandler (string eventName, EventHandler handler) 
		{
			EventListener.RemoveHandler (handler, eventName);
		}
		
		public void Focus () {
			nsIWebBrowserFocus focus = (nsIWebBrowserFocus) this.window;
			focus.setFocusedWindow (this.window);
		}
		
		public void Open (string url)
		{
			nsIWebNavigation webnav = (nsIWebNavigation) this.window;			
			webnav.loadURI (url, (uint)LoadFlags.None, null, null, null);
		}
		
		public void ScrollTo (int x, int y)
		{
			this.window.scrollTo (x, y);
		}
		
		public override bool Equals (object obj)
		{
			return this == obj as Window;
		}

		public static bool operator == (Window left, Window right)
		{
			if ((object)left == (object)right)
				return true;

			if ((object)left == null || (object)right == null)
				return false;

			return left.hashcode == right.hashcode; 
		}

		public static bool operator != (Window left, Window right)
		{
			return !(left == right);
		}

		public override int GetHashCode () {
			return hashcode;
		}		

#endregion

#region Events
		static object LoadEvent = new object ();
		public event EventHandler Load {
			add { 
				Events.AddHandler (LoadEvent, value); 
				AttachEventHandler ("load", value);
			}
			remove { 
				Events.RemoveHandler (LoadEvent, value); 
				DetachEventHandler ("load", value);
			}
		}

		static object UnloadEvent = new object ();
		public event EventHandler Unload {
			add { 
				Events.AddHandler (UnloadEvent, value); 
				AttachEventHandler ("unload", value);
			}
			remove { 
				Events.RemoveHandler (UnloadEvent, value); 
				DetachEventHandler ("unload", value);
			}
		}

		public event EventHandler OnFocus {
			add { AttachEventHandler ("focus", value); }
			remove { DetachEventHandler ("focus", value); }
		}

		public event EventHandler OnBlur {
			add { AttachEventHandler ("blur", value); }
			remove { DetachEventHandler ("blur", value); }
		}
		
		public event EventHandler Error {
			add { AttachEventHandler ("error", value); }
			remove { DetachEventHandler ("error", value); }
		}
		
		public event EventHandler Scroll {
			add { AttachEventHandler ("scroll", value); }
			remove { DetachEventHandler ("scroll", value); }
		}
		
		public void OnLoad ()
		{
			EventHandler eh = (EventHandler) (Events[LoadEvent]);
			if (eh != null) {
				EventArgs e = new EventArgs ();
				eh (this, e);
			}		
		}

		public void OnUnload ()
		{		
			EventHandler eh = (EventHandler) (Events[UnloadEvent]);
			if (eh != null) {
				EventArgs e = new EventArgs ();
				eh (this, e);
			}
		}
		
#endregion		
	}
}
