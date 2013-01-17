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
// Copyright (c) 2007 Novell, Inc.
//
// Authors:
//	Andreia Gaita (avidigal@novell.com)
//

#undef debug

using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Mono.Mozilla {

	using Mono.WebBrowser;
	using Mono.WebBrowser.DOM;
	
	internal class Callback
	{
		WebBrowser owner;
		string currentUri;
		bool calledLoadStarted;
		
		public Callback (WebBrowser owner) 
		{
			this.owner = owner;
			
		}

		#region Events

		public void OnWidgetLoaded ()
		{
#if debug
			OnGeneric ("OnWidgetLoaded");
#endif
		}


		public void OnStateChange (nsIWebProgress progress, nsIRequest request, Int32 status, UInt32 state)
		{
			if (!owner.created)
				owner.created = true;

#if debug
			//OnGeneric ("OnStateChange");

			System.Text.StringBuilder s = new System.Text.StringBuilder ();
			if ((state & (uint) StateFlags.Start) != 0) {
				s.Append ("Start\t");
			}
			if ((state & (uint) StateFlags.Redirecting) != 0) {
				s.Append ("Redirecting\t");
			}
			if ((state & (uint) StateFlags.Transferring) != 0) {
				s.Append ("Transferring\t");
			}
			if ((state & (uint) StateFlags.Negotiating) != 0) {
				s.Append ("Negotiating\t");
			}
			if ((state & (uint) StateFlags.Stop) != 0) {
				s.Append ("Stop\t");
			}
			if ((state & (uint) StateFlags.IsRequest) != 0) {
				s.Append ("Request\t");
			}
			if ((state & (uint) StateFlags.IsDocument) != 0) {
				s.Append ("Document\t");
			}
			if ((state & (uint) StateFlags.IsNetwork) != 0) {
				s.Append ("Network\t");
			}
			if ((state & (uint) StateFlags.IsWindow) != 0) {
				s.Append ("Window\t");
			}
			Console.Error.WriteLine (s.ToString ());
#endif

			bool _start = (state & (uint) StateFlags.Start) != 0;
			bool _transferring = (state & (uint) StateFlags.Transferring) != 0;
			bool _redirecting = (state & (uint) StateFlags.Redirecting) != 0;
			bool _stop = (state & (uint) StateFlags.Stop) != 0;
			bool _request = (state & (uint) StateFlags.IsRequest) != 0;
			bool _document = (state & (uint) StateFlags.IsDocument) != 0;
			bool _network = (state & (uint) StateFlags.IsNetwork) != 0;
			bool _window = (state & (uint) StateFlags.IsWindow) != 0;

			if (_start && _request && _document && !calledLoadStarted) {
				nsIDOMWindow win;
				progress.getDOMWindow (out win);
				nsIChannel channel = (nsIChannel) request;
				nsIURI uri;
				channel.getURI (out uri);
				if (uri == null)
					currentUri = "about:blank";
				else {
					AsciiString spec = new AsciiString (String.Empty);
					uri.getSpec (spec.Handle);
					currentUri = spec.ToString ();
				}

				calledLoadStarted = true;
				LoadStartedEventHandler eh = (LoadStartedEventHandler) (owner.Events [WebBrowser.LoadStartedEvent]);
				if (eh != null) {

					AsciiString name = new AsciiString (String.Empty);
					win.getName (name.Handle);

					LoadStartedEventArgs e = new LoadStartedEventArgs (currentUri, name.ToString ());
					eh (this, e);
					if (e.Cancel)
						request.cancel (2152398850); //NS_BINDING_ABORTED
				}
				return;

			}

			if (_document && _request && _transferring) {
				nsIDOMWindow win;
				progress.getDOMWindow (out win);
				nsIChannel channel = (nsIChannel) request;
				nsIURI uri;
				channel.getURI (out uri);
				if (uri == null)
					currentUri = "about:blank";
				else {
					AsciiString spec = new AsciiString (String.Empty);
					uri.getSpec (spec.Handle);
					currentUri = spec.ToString ();
				}

				nsIDOMWindow topWin;
				win.getTop (out topWin);
				if (topWin == null || topWin.GetHashCode () == win.GetHashCode ()) {
					owner.Reset ();
					nsIDOMDocument doc;
					win.getDocument (out doc);
					if (doc != null)
						owner.document = new Mono.Mozilla.DOM.Document (owner, doc);
				}

				LoadCommitedEventHandler eh = (LoadCommitedEventHandler) (owner.Events[WebBrowser.LoadCommitedEvent]);
				if (eh != null) {
					LoadCommitedEventArgs e = new LoadCommitedEventArgs (currentUri);
					eh (this, e);
				}
				return;
			}

			if (_document && _request && _redirecting) {
				nsIDOMWindow win;
				progress.getDOMWindow (out win);
				nsIChannel channel = (nsIChannel) request;
				nsIURI uri;
				channel.getURI (out uri);
				if (uri == null)
					currentUri = "about:blank";
				else {
					AsciiString spec = new AsciiString (String.Empty);
					uri.getSpec (spec.Handle);
					currentUri = spec.ToString ();
				}
				return;
			}

			if (_stop && !_request && !_document && _network && _window) {
				calledLoadStarted = false;
			    LoadFinishedEventHandler eh1 = (LoadFinishedEventHandler) (owner.Events[WebBrowser.LoadFinishedEvent]);
			    if (eh1 != null) {

 					nsIDOMWindow win;
					progress.getDOMWindow (out win);
			        LoadFinishedEventArgs e = new LoadFinishedEventArgs (currentUri);
			        eh1 (this, e);

			    }
				return;
			}

			if (_stop && !_request && _document && !_network && !_window) {
				nsIDOMWindow win;
				progress.getDOMWindow (out win);
				nsIDOMDocument doc;
				win.getDocument (out doc);
				if (doc != null) {
					int hash = doc.GetHashCode ();
					if (owner.documents.ContainsKey (hash)) {
						DOM.Document document = owner.documents[hash] as DOM.Document;
						
						EventHandler eh1 = (EventHandler)(document.Events[DOM.Document.LoadStoppedEvent]);
						if (eh1 != null)
							eh1 (this, null);
				    }
				}
				calledLoadStarted = false;
				return;
			} 
#if debug
			Console.Error.WriteLine ("{0} completed", s.ToString ());
#endif
		}

		public void OnProgress (nsIWebProgress progress, nsIRequest request, Int32 currentTotalProgress, Int32 maxTotalProgress)
		{
#if debug
			OnGeneric ("OnProgress");
#endif			
			ProgressChangedEventHandler eh = (ProgressChangedEventHandler) (owner.Events [Mono.Mozilla.WebBrowser.ProgressChangedEvent]);
		    if (eh != null) {
		        Mono.WebBrowser.ProgressChangedEventArgs e = new Mono.WebBrowser.ProgressChangedEventArgs (currentTotalProgress, maxTotalProgress);
		        eh (this, e);
		    }
		}

		public void OnLocationChanged (nsIWebProgress progress, nsIRequest request, nsIURI uri)
		{
#if debug
			OnGeneric ("OnLocationChanged");
#endif
		}

		public void OnStatusChange (nsIWebProgress progress, nsIRequest request, string message, Int32 status)
		{
			StatusChangedEventHandler eh = (StatusChangedEventHandler) (owner.Events[WebBrowser.StatusChangedEvent]);
			if (eh != null) {
				StatusChangedEventArgs e = new StatusChangedEventArgs (message, status);
				eh (this, e);
			}
		}

		public void OnSecurityChange (nsIWebProgress progress, nsIRequest request, uint status)
		{
			SecurityChangedEventHandler eh = (SecurityChangedEventHandler) (owner.Events[WebBrowser.SecurityChangedEvent]);
			if (eh != null) {
				SecurityLevel state = SecurityLevel.Insecure;
				switch (status) {
				case 4: 
					state = SecurityLevel.Insecure;
					break;
				case 1:
					state = SecurityLevel.Mixed;
					break;
				case 2:
					state = SecurityLevel.Secure;
					break;
				}

				SecurityChangedEventArgs e = new SecurityChangedEventArgs (state);
				eh (this, e);
			}
		}

		public bool OnClientDomKeyDown (KeyInfo keyInfo, ModifierKeys modifiers, nsIDOMNode target)
		{
#if debug
			OnGeneric ("OnClientDomKeyDown");
			Console.Error.WriteLine ("OnClientDomKeyDown");
#endif
			INode node = new Mono.Mozilla.DOM.Node (owner, target);			
			string key = String.Intern (node.GetHashCode () + ":keydown");
			EventHandler eh1 = (EventHandler) owner.DomEvents[key];
			if (eh1 != null) {
				EventArgs e1 = new EventArgs ();
				eh1 (node, e1);
			}
			
			NodeEventHandler eh = (NodeEventHandler) (owner.Events[WebBrowser.KeyDownEvent]);
			if (eh != null) {
				NodeEventArgs e = new NodeEventArgs (node);
				eh (this, e);
				return true;
			}
			return false;
		}

		public bool OnClientDomKeyUp (KeyInfo keyInfo, ModifierKeys modifiers, nsIDOMNode target)
		{
#if debug
			OnGeneric ("OnClientDomKeyUp");
			Console.Error.WriteLine ("OnClientDomKeyUp");
#endif
			INode node = new Mono.Mozilla.DOM.Node (owner, target);			
			string key = String.Intern (node.GetHashCode () + ":keyup");
			EventHandler eh1 = (EventHandler) owner.DomEvents[key];
			if (eh1 != null) {
				EventArgs e1 = new EventArgs ();
				eh1 (node, e1);
			}
			
			NodeEventHandler eh = (NodeEventHandler) (owner.Events[WebBrowser.KeyUpEvent]);
			if (eh != null) {
				NodeEventArgs e = new NodeEventArgs (node);
				eh (this, e);
				return true;
			}
			return false;
		}

		public bool OnClientDomKeyPress (KeyInfo keyInfo, ModifierKeys modifiers, nsIDOMNode target)
		{
#if debug
			OnGeneric ("OnClientDomKeyPress");
			Console.Error.WriteLine ("OnClientDomKeyPress");
#endif
			INode node = new Mono.Mozilla.DOM.Node (owner, target);			
			string key = String.Intern (node.GetHashCode () + ":keypress");
			EventHandler eh1 = (EventHandler) owner.DomEvents[key];
			if (eh1 != null) {
				EventArgs e1 = new EventArgs ();
				eh1 (node, e1);
			}
			
			NodeEventHandler eh = (NodeEventHandler) (owner.Events[WebBrowser.KeyPressEvent]);
			if (eh != null) {
				NodeEventArgs e = new NodeEventArgs (node);
				eh (this, e);
				return true;
			}
			return false;
		}

		public bool OnClientMouseDown (MouseInfo mouseInfo, ModifierKeys modifiers, nsIDOMNode target)
		{
#if debug
			OnGeneric ("OnClientMouseDown");
			Console.Error.WriteLine ("OnClientMouseDown");
#endif
			INode node = new Mono.Mozilla.DOM.Node (owner, target);			
			string key = String.Intern (node.GetHashCode () + ":mousedown");
			EventHandler eh1 = (EventHandler) owner.DomEvents[key];
			if (eh1 != null) {
				EventArgs e1 = new EventArgs ();
				eh1 (node, e1);
			}
			
			NodeEventHandler eh = (NodeEventHandler) (owner.Events[WebBrowser.MouseDownEvent]);
			if (eh != null) {
				NodeEventArgs e = new NodeEventArgs (node);
				eh (this, e);
				return true;
			}
			return false;
		}

		public bool OnClientMouseUp (MouseInfo mouseInfo, ModifierKeys modifiers, nsIDOMNode target)
		{
#if debug
			OnGeneric ("OnClientMouseUp");
			Console.Error.WriteLine ("OnClientMouseUp");
#endif
			INode node = new Mono.Mozilla.DOM.Node (owner, target);			
			string key = String.Intern (node.GetHashCode () + ":mouseup");
			EventHandler eh1 = (EventHandler) owner.DomEvents[key];
			if (eh1 != null) {
				EventArgs e1 = new EventArgs ();
				eh1 (node, e1);
			}
			
			NodeEventHandler eh = (NodeEventHandler) (owner.Events[WebBrowser.MouseUpEvent]);
			if (eh != null) {
				NodeEventArgs e = new NodeEventArgs (node);
				eh (this, e);
				return true;
			}
			return false;
		}

		public bool OnClientMouseClick (MouseInfo mouseInfo, ModifierKeys modifiers, nsIDOMNode target)
		{
#if debug
			OnGeneric ("OnClientMouseClick");
			Console.Error.WriteLine ("OnClientMouseClick");
#endif
			INode node = new Mono.Mozilla.DOM.Node (owner, target);			
			string key = String.Intern (node.GetHashCode () + ":click");
			EventHandler eh1 = (EventHandler) owner.DomEvents[key];
			if (eh1 != null) {
				EventArgs e1 = new EventArgs ();
				eh1 (node, e1);
			}
			
			NodeEventHandler eh = (NodeEventHandler) (owner.Events[WebBrowser.MouseClickEvent]);
			if (eh != null) {
				NodeEventArgs e = new NodeEventArgs (node);
				eh (this, e);
				return true;
			}
			return false;
		}

		public bool OnClientMouseDoubleClick (MouseInfo mouseInfo, ModifierKeys modifiers, nsIDOMNode target)
		{
#if debug
			OnGeneric ("OnClientMouseDoubleClick");
			Console.Error.WriteLine ("OnClientMouseDoubleClick");
#endif
			INode node = new Mono.Mozilla.DOM.Node (owner, target);			
			string key = String.Intern (node.GetHashCode () + ":dblclick");
			EventHandler eh1 = (EventHandler) owner.DomEvents[key];
			if (eh1 != null) {
				EventArgs e1 = new EventArgs ();
				eh1 (node, e1);
			}
			
			NodeEventHandler eh = (NodeEventHandler) (owner.Events[WebBrowser.MouseDoubleClickEvent]);
			if (eh != null) {
				NodeEventArgs e = new NodeEventArgs (node);
				eh (this, e);
				return true;
			}
			return false;
		}

		public bool OnClientMouseOver (MouseInfo mouseInfo, ModifierKeys modifiers, nsIDOMNode target)
		{
#if debug
			OnGeneric ("OnClientMouseOver");
			Console.Error.WriteLine ("OnClientMouseOver");
#endif
			DOM.DOMObject helper = new DOM.DOMObject(this.owner);
			INode node = helper.GetTypedNode  (target);
			string key = String.Intern (node.GetHashCode () + ":mouseover");
			EventHandler eh1 = (EventHandler) owner.DomEvents[key];
			if (eh1 != null) {
				EventArgs e1 = new EventArgs ();
				eh1 (node, e1);
			}
			
			NodeEventHandler eh = (NodeEventHandler) (owner.Events[WebBrowser.MouseEnterEvent]);
			if (eh != null) {
				NodeEventArgs e = new NodeEventArgs (node);
				eh (node, e);
				return true;
			}
			return false;
		}

		public bool OnClientMouseOut (MouseInfo mouseInfo, ModifierKeys modifiers, nsIDOMNode target)
		{
#if debug
			OnGeneric ("OnClientMouseOut");
			Console.Error.WriteLine ("OnClientMouseOut");
#endif
			INode node = new Mono.Mozilla.DOM.Node (owner, target);			
			string key = String.Intern (node.GetHashCode () + ":mouseout");
			EventHandler eh1 = (EventHandler) owner.DomEvents[key];
			if (eh1 != null) {
				EventArgs e1 = new EventArgs ();
				eh1 (node, e1);
			}
			
			NodeEventHandler eh = (NodeEventHandler) (owner.Events[WebBrowser.MouseLeaveEvent]);
			if (eh != null) {
				NodeEventArgs e = new NodeEventArgs (node);
				eh (this, e);
				return true;
			}
			return false;
		}

		public bool OnClientActivate ()
		{
#if debug
			OnGeneric ("OnClientActivate");
			Console.Error.WriteLine ("OnClientActivate");
#endif
			// TODO:  Add WebBrowser.OnClientActivate implementation
			return false;
		}

		public bool OnClientFocus ()
		{
#if debug
			OnGeneric ("OnClientFocus");
			Console.Error.WriteLine ("OnClientFocus");
#endif
			EventHandler eh = (EventHandler) (owner.Events[WebBrowser.FocusEvent]);
			if (eh != null) {
				EventArgs e = new EventArgs ();
				eh (this, e);
			}
			return false;
		}

		public bool OnClientBlur ()
		{
#if debug
			OnGeneric ("OnClientBlur");
			Console.Error.WriteLine ("OnClientBlur");
#endif
			EventHandler eh = (EventHandler) (owner.Events[WebBrowser.BlurEvent]);
			if (eh != null) {
				EventArgs e = new EventArgs ();
				eh (this, e);
			}
			return false;
		}

		public bool OnCreateNewWindow ()
		{
			bool ret = false;

#if debug
			OnGeneric ("OnCreateNewWindow");
			Console.Error.WriteLine ("OnCreateNewWindow");
#endif
			CreateNewWindowEventHandler eh = (CreateNewWindowEventHandler) (owner.Events[WebBrowser.CreateNewWindowEvent]);
			if (eh != null) {
				CreateNewWindowEventArgs e = new CreateNewWindowEventArgs (false);
				ret = eh (this, e);
			}
			return ret;
		}

		public void OnAlert (IntPtr title, IntPtr text)
		{
#if debug
			OnGeneric ("OnAlert");
			Console.Error.WriteLine ("OnAlert");
#endif
			AlertEventHandler eh = (AlertEventHandler) (owner.Events[WebBrowser.AlertEvent]);
			if (eh != null) {
				AlertEventArgs e = new AlertEventArgs ();
				e.Type = DialogType.Alert;
				if (title != IntPtr.Zero)
					e.Title = Marshal.PtrToStringUni (title);
				if (text != IntPtr.Zero)
					e.Text = Marshal.PtrToStringUni (text);
				eh (this, e);
			}
		}

		public bool OnAlertCheck (IntPtr title, IntPtr text, IntPtr chkMsg, ref bool chkState)
		{
#if debug
			OnGeneric ("OnAlertCheck");
			Console.Error.WriteLine ("OnAlertCheck");
#endif
			AlertEventHandler eh = (AlertEventHandler) (owner.Events[WebBrowser.AlertEvent]);
			if (eh != null) {
				AlertEventArgs e = new AlertEventArgs ();
				e.Type = DialogType.AlertCheck;
				if (title != IntPtr.Zero)
					e.Title = Marshal.PtrToStringUni (title);
				if (text != IntPtr.Zero)
					e.Text = Marshal.PtrToStringUni (text);
				if (chkMsg != IntPtr.Zero)
					e.CheckMessage = Marshal.PtrToStringUni (chkMsg);
				e.CheckState = chkState;
				eh (this, e);
				return e.BoolReturn;
			}
			return false;
		}

		public bool OnConfirm (IntPtr title, IntPtr text)
		{
#if debug
			OnGeneric ("OnConfirm");
			Console.Error.WriteLine ("OnConfirm");
#endif
			AlertEventHandler eh = (AlertEventHandler) (owner.Events[WebBrowser.AlertEvent]);
			if (eh != null) {
				AlertEventArgs e = new AlertEventArgs ();
				e.Type = DialogType.Confirm;
				if (title != IntPtr.Zero)
					e.Title = Marshal.PtrToStringUni (title);
				if (text != IntPtr.Zero)
					e.Text = Marshal.PtrToStringUni (text);
				eh (this, e);
				return e.BoolReturn;
			}
			return false;
		}

		public bool OnConfirmCheck (IntPtr title, IntPtr text, IntPtr chkMsg, ref bool chkState)
		{
#if debug
			OnGeneric ("OnConfirmCheck");
			Console.Error.WriteLine ("OnConfirmCheck");
#endif
			AlertEventHandler eh = (AlertEventHandler) (owner.Events[WebBrowser.AlertEvent]);
			if (eh != null) {
				AlertEventArgs e = new AlertEventArgs ();
				e.Type = DialogType.ConfirmCheck;
				if (title != IntPtr.Zero)
					e.Title = Marshal.PtrToStringUni (title);
				if (text != IntPtr.Zero)
					e.Text = Marshal.PtrToStringUni (text);
				if (chkMsg != IntPtr.Zero)
					e.CheckMessage = Marshal.PtrToStringUni (chkMsg);
				e.CheckState = chkState;
				eh (this, e);
				chkState = e.CheckState;
				return e.BoolReturn;
			}
			return false;
		}

		public bool OnConfirmEx (IntPtr title, IntPtr text, DialogButtonFlags flags,
								IntPtr title0, IntPtr title1, IntPtr title2,
								IntPtr chkMsg, ref bool chkState, out Int32 retVal)
		{
#if debug
			OnGeneric ("OnConfirmEx");
			Console.Error.WriteLine ("OnConfirmEx");
#endif
			retVal = -1;

			AlertEventHandler eh = (AlertEventHandler) (owner.Events[WebBrowser.AlertEvent]);
			if (eh != null) {
				AlertEventArgs e = new AlertEventArgs ();
				e.Type = DialogType.ConfirmEx;
				if (title != IntPtr.Zero)
					e.Title = Marshal.PtrToStringUni (title);
				if (text != IntPtr.Zero)
					e.Text = Marshal.PtrToStringUni (text);
				if (chkMsg != IntPtr.Zero)
					e.CheckMessage = Marshal.PtrToStringUni (chkMsg);
				e.CheckState = chkState;
				eh (this, e);
				chkState = e.CheckState;
				return e.BoolReturn;
			}
			return false;
		}

		public bool OnPrompt (IntPtr title, IntPtr text, ref IntPtr retVal)
		{
#if debug
			OnGeneric ("OnPrompt");
			Console.Error.WriteLine ("OnPrompt");
#endif
			AlertEventHandler eh = (AlertEventHandler) (owner.Events[WebBrowser.AlertEvent]);
			if (eh != null) {
				AlertEventArgs e = new AlertEventArgs ();
				e.Type = DialogType.Prompt;
				if (title != IntPtr.Zero)
					e.Title = Marshal.PtrToStringUni (title);
				if (text != IntPtr.Zero)
					e.Text = Marshal.PtrToStringUni (text);
				if (retVal != IntPtr.Zero)
					e.Text2 = Marshal.PtrToStringUni (retVal);
				eh (this, e);
				retVal = Marshal.StringToHGlobalUni (e.StringReturn);
				return e.BoolReturn;
			}
			return false;
		}

		public bool OnPromptUsernameAndPassword (IntPtr title, IntPtr text, IntPtr chkMsg, ref bool chkState, out IntPtr username, out IntPtr password)
		{
#if debug
			OnGeneric ("OnPromptUsernameAndPassword");
			Console.Error.WriteLine ("OnPromptUsernameAndPassword");
#endif
			username = IntPtr.Zero;
			password = IntPtr.Zero;
			AlertEventHandler eh = (AlertEventHandler) (owner.Events[WebBrowser.AlertEvent]);
			if (eh != null) {
				AlertEventArgs e = new AlertEventArgs ();
				e.Type = DialogType.PromptUsernamePassword;
				if (title != IntPtr.Zero)
					e.Title = Marshal.PtrToStringUni (title);
				if (text != IntPtr.Zero)
					e.Text = Marshal.PtrToStringUni (text);
				if (chkMsg != IntPtr.Zero)
					e.CheckMessage = Marshal.PtrToStringUni (chkMsg);
				e.CheckState = chkState;
				eh (this, e);
				return e.BoolReturn;
			}
			return false;
		}

		public bool OnPromptPassword (IntPtr title, IntPtr text, IntPtr chkMsg, ref bool chkState, out IntPtr password)
		{
#if debug
			OnGeneric ("OnPromptPassword");
			Console.Error.WriteLine ("OnPromptPassword");
#endif
			password = IntPtr.Zero;
			AlertEventHandler eh = (AlertEventHandler) (owner.Events[WebBrowser.AlertEvent]);
			if (eh != null) {
				AlertEventArgs e = new AlertEventArgs ();
				e.Type = DialogType.PromptPassword;
				if (title != IntPtr.Zero)
					e.Title = Marshal.PtrToStringUni (title);
				if (text != IntPtr.Zero)
					e.Text = Marshal.PtrToStringUni (text);
				if (chkMsg != IntPtr.Zero)
					e.CheckMessage = Marshal.PtrToStringUni (chkMsg);
				e.CheckState = chkState;
				eh (this, e);
				return e.BoolReturn;
			}
			return false;
		}

		public bool OnSelect (IntPtr title, IntPtr text, uint count, IntPtr list, out int retVal)
		{
#if debug
			OnGeneric ("OnSelect");
			Console.Error.WriteLine ("OnSelect");
#endif
			retVal = 0;
			AlertEventHandler eh = (AlertEventHandler) (owner.Events[WebBrowser.AlertEvent]);
			if (eh != null) {
				AlertEventArgs e = new AlertEventArgs ();
				e.Type = DialogType.Select;
				if (title != IntPtr.Zero)
					e.Title = Marshal.PtrToStringUni (title);
				if (text != IntPtr.Zero)
					e.Text = Marshal.PtrToStringUni (text);
				eh (this, e);
				return e.BoolReturn;
			}
			return false;
		}

		public void OnLoad ()
		{
#if debug
			OnGeneric ("OnLoad");
			Console.Error.WriteLine ("OnLoad");
#endif
			((DOM.Window)owner.Window).OnLoad ();
		}

		public void OnUnload ()
		{
#if debug
			OnGeneric ("OnUnload");
			Console.Error.WriteLine ("OnUnload");
#endif
			((DOM.Window)owner.Window).OnUnload ();
		}
		
		public void OnShowContextMenu (UInt32 contextFlags, 
		                               [MarshalAs (UnmanagedType.Interface)] nsIDOMEvent eve, 
		                               [MarshalAs (UnmanagedType.Interface)] nsIDOMNode node)
		{
#if debug
			OnGeneric ("OnShowContextMenu");
			Console.Error.WriteLine ("OnShowContextMenu");
#endif
			ContextMenuEventHandler eh = (ContextMenuEventHandler) (owner.Events[WebBrowser.ContextMenuEvent]);

			if (eh != null) {
				nsIDOMMouseEvent mouseEvent = (nsIDOMMouseEvent) eve;
				int x, y;
				mouseEvent.getClientX (out x);
				mouseEvent.getClientY (out y);
				ContextMenuEventArgs args = new ContextMenuEventArgs(x, y);
				eh (owner, args);
			}
			
		}
		
		public void OnGeneric (string type)
		{
#if debug
//			string t = Marshal.PtrToStringUni (type);
			Console.Error.WriteLine ("Callback Generic:{0}", type);
#endif
			EventHandler eh = (EventHandler) (owner.Events[WebBrowser.GenericEvent]);
			if (eh != null) {
				EventArgs e = new EventArgs ();
				eh (type, e);
				return;
			}
			return;

		}
		#endregion
		
	}

#region Delegates

	internal delegate void CallbackVoid ();

	internal delegate void CallbackString (string arg1);
	internal delegate void CallbackWString ([MarshalAs(UnmanagedType.LPWStr)] string arg1);
	
	internal delegate void CallbackStringString (string arg1, string arg2);
	internal delegate void CallbackStringInt		(string arg1, Int32 arg2);
	internal delegate void CallbackWStringInt ([MarshalAs (UnmanagedType.LPWStr)] string arg1, Int32 arg2);
	internal delegate void CallbackStringIntInt	(string arg1, Int32 arg2, Int32 arg3);
	internal delegate void CallbackStringIntUint	(string arg1, Int32 arg2, UInt32 arg3);


	internal delegate void CallbackIntInt			(Int32 arg1, Int32 arg2);
	internal delegate void CallbackIntUint			(Int32 arg2, UInt32 arg3);

	internal delegate void CallbackUint			(UInt32 arg1);
	internal delegate void CallbackUintInt			(UInt32 arg1, Int32 arg2);

	internal delegate void CallbackPtrPtr			(IntPtr arg1, IntPtr arg2);

	//Don't have to worry about marshalling bool, PRBool seems very constant and uses 4 bit int underneath
	internal delegate void CallbackBool			(bool val);
	
	internal delegate bool KeyCallback			(KeyInfo keyInfo, ModifierKeys modifiers, [MarshalAs (UnmanagedType.Interface)] nsIDOMNode target);
	internal delegate bool MouseCallback			(MouseInfo mouseInfo, ModifierKeys modifiers, [MarshalAs (UnmanagedType.Interface)] nsIDOMNode target);

	internal delegate void GenericCallback			(IntPtr type);
	

	internal delegate bool Callback2				();
	internal delegate bool Callback2String			(string arg1);


	internal delegate bool CallbackOnAlertCheck	(IntPtr title, IntPtr text, IntPtr chkMsg, ref bool chkState);
	internal delegate bool CallbackOnConfirm		(IntPtr title, IntPtr text);
	internal delegate bool CallbackOnConfirmCheck	(IntPtr title, IntPtr text, IntPtr chkMsg, ref bool chkState);
	internal delegate bool CallbackOnConfirmEx	(IntPtr title, IntPtr text, Mono.WebBrowser.DialogButtonFlags flags, 
														 	 IntPtr title0, IntPtr title1, IntPtr title2,
														 	 IntPtr chkMsg, ref bool chkState, out Int32 retVal);
	internal delegate bool CallbackOnPrompt		(IntPtr title, IntPtr text,
														 	 ref IntPtr retVal);
	internal delegate bool CallbackOnPromptUsernameAndPassword 
															(IntPtr title, IntPtr text,
														 	 IntPtr chkMsg, ref bool chkState, 
														 	 out IntPtr username, out IntPtr password);
	internal delegate bool CallbackOnPromptPassword
															(IntPtr title, IntPtr text,
														 	 IntPtr chkMsg, ref bool chkState, 
														 	 out IntPtr password);
	internal delegate bool CallbackOnSelect		(IntPtr title, IntPtr text, 
														 	 UInt32 count, IntPtr list, 
														 	 out Int32 retVal);
	
	internal delegate void CallbackOnLocationChanged ([MarshalAs (UnmanagedType.Interface)] nsIWebProgress progress,
	                                                  [MarshalAs (UnmanagedType.Interface)] nsIRequest request,
	                                                  [MarshalAs (UnmanagedType.Interface)] nsIURI uri);

	internal delegate void CallbackOnStatusChange ([MarshalAs (UnmanagedType.Interface)] nsIWebProgress progress,
	                                               [MarshalAs (UnmanagedType.Interface)] nsIRequest request,
	                                               [MarshalAs (UnmanagedType.LPWStr)] string message, Int32 status);
	
	internal delegate void CallbackOnSecurityChange ([MarshalAs (UnmanagedType.Interface)] nsIWebProgress progress,
	                                               [MarshalAs (UnmanagedType.Interface)] nsIRequest request,
	                                               uint status);

	internal delegate void CallbackOnStateChange ([MarshalAs (UnmanagedType.Interface)] nsIWebProgress progress,
	                                               [MarshalAs (UnmanagedType.Interface)] nsIRequest request,
	                                               Int32 arg2, UInt32 arg3);
	internal delegate void CallbackOnProgress ([MarshalAs (UnmanagedType.Interface)] nsIWebProgress progress,
	                                               [MarshalAs (UnmanagedType.Interface)] nsIRequest request,
	                                               Int32 arg2, Int32 arg3);
	
	internal delegate void CallbackOnShowContextMenu (UInt32 contextFlags, 
	                                                  [MarshalAs (UnmanagedType.Interface)] nsIDOMEvent eve, 
	                                                  [MarshalAs (UnmanagedType.Interface)] nsIDOMNode node);
	


#endregion


#region Structs

	[StructLayout (LayoutKind.Sequential)]
	internal struct CallbackBinder {
		
		public CallbackVoid			OnWidgetLoaded;

		public CallbackOnStateChange 		OnStateChange;
		public CallbackOnProgress		OnProgress;
		public CallbackOnLocationChanged		OnLocationChanged;

		public CallbackOnStatusChange	OnStatusChange;
		public CallbackOnSecurityChange	OnSecurityChange;

		public KeyCallback			OnKeyDown;
		public KeyCallback			OnKeyUp;
		public KeyCallback			OnKeyPress;

		public MouseCallback		OnMouseDown;
		public MouseCallback		OnMouseUp;
		public MouseCallback		OnMouseClick;
		public MouseCallback		OnMouseDoubleClick;
		public MouseCallback		OnMouseOver;
		public MouseCallback		OnMouseOut;

		public Callback2			OnActivate;
		public Callback2			OnFocus;
		public Callback2			OnBlur;

		public CallbackPtrPtr							OnAlert;
		public CallbackOnAlertCheck						OnAlertCheck;
		public CallbackOnConfirm 						OnConfirm;
		public CallbackOnConfirmCheck 					OnConfirmCheck;
		public CallbackOnConfirmEx 						OnConfirmEx;
		public CallbackOnPrompt 						OnPrompt;
		public CallbackOnPromptUsernameAndPassword 		OnPromptUsernameAndPassword;
		public CallbackOnPromptPassword 				OnPromptPassword;
		public CallbackOnSelect 						OnSelect;

		public CallbackVoid				OnLoad;
		public CallbackVoid				OnUnload;
		
		public CallbackOnShowContextMenu OnShowContextMenu;

		public CallbackWString		OnGeneric;
		
		internal CallbackBinder (Callback callback) {
			this.OnWidgetLoaded			= new CallbackVoid (callback.OnWidgetLoaded);

			this.OnStateChange			= new CallbackOnStateChange (callback.OnStateChange);

			this.OnProgress				= new CallbackOnProgress (callback.OnProgress);
			this.OnLocationChanged		= new CallbackOnLocationChanged (callback.OnLocationChanged);
			this.OnStatusChange			= new CallbackOnStatusChange (callback.OnStatusChange);
			this.OnSecurityChange		= new CallbackOnSecurityChange (callback.OnSecurityChange);

			this.OnKeyDown				= new KeyCallback (callback.OnClientDomKeyDown);
			this.OnKeyUp				= new KeyCallback (callback.OnClientDomKeyUp);
			this.OnKeyPress				= new KeyCallback (callback.OnClientDomKeyPress);

			this.OnMouseDown			= new MouseCallback (callback.OnClientMouseDown);
			this.OnMouseUp				= new MouseCallback (callback.OnClientMouseUp);
			this.OnMouseClick			= new MouseCallback (callback.OnClientMouseClick);
			this.OnMouseDoubleClick		= new MouseCallback (callback.OnClientMouseDoubleClick);
			this.OnMouseOver			= new MouseCallback (callback.OnClientMouseOver);
			this.OnMouseOut				= new MouseCallback (callback.OnClientMouseOut);

			this.OnActivate				= new Callback2 (callback.OnClientActivate);
			this.OnFocus				= new Callback2 (callback.OnClientFocus);
			this.OnBlur					= new Callback2 (callback.OnClientBlur);

			this.OnAlert				= new CallbackPtrPtr (callback.OnAlert);
			this.OnAlertCheck			= new CallbackOnAlertCheck (callback.OnAlertCheck);
			this.OnConfirm 				= new CallbackOnConfirm (callback.OnConfirm);
			this.OnConfirmCheck 		= new CallbackOnConfirmCheck (callback.OnConfirmCheck);
			this.OnConfirmEx 			= new CallbackOnConfirmEx (callback.OnConfirmEx);
			this.OnPrompt 				= new CallbackOnPrompt (callback.OnPrompt);
			this.OnPromptUsernameAndPassword = new CallbackOnPromptUsernameAndPassword (callback.OnPromptUsernameAndPassword);
			this.OnPromptPassword 		= new CallbackOnPromptPassword (callback.OnPromptPassword);
			this.OnSelect 				= new CallbackOnSelect (callback.OnSelect);

			this.OnLoad 				= new CallbackVoid (callback.OnLoad);
			this.OnUnload 				= new CallbackVoid (callback.OnUnload);
			
			this.OnShowContextMenu		= new CallbackOnShowContextMenu (callback.OnShowContextMenu);
			
			this.OnGeneric				= new CallbackWString (callback.OnGeneric);
		}
	}

 
	[StructLayout (LayoutKind.Sequential)]
	internal struct SizeInfo
	{
		public UInt32 width;
		public UInt32 height;
	}

	[StructLayout (LayoutKind.Sequential)]
	internal struct ModifierKeys
	{
		public Int32 altKey;
		public Int32 ctrlKey;
		public Int32 metaKey;
		public Int32 shiftKey;
	}

	[StructLayout (LayoutKind.Sequential)]
	internal struct MouseInfo
	{
		public UInt16 button;
		public Int32 clientX;
		public Int32 clientY;
		public Int32 screenX;
		public Int32 screenY;
	}

	[StructLayout (LayoutKind.Sequential)]
	internal struct KeyInfo
	{
		public UInt32 charCode;
		public UInt32 keyCode;
	}
	
	[Flags]
	internal enum StateFlags
	{
		Start = 1,
		Redirecting = 2,
		Transferring = 4,
		Negotiating = 8,
		Stop = 16,
		IsRequest = 65536,
		IsDocument = 	131072,
		IsNetwork = 262144,
		IsWindow = 524288,
		Restoring = 16777216,
		IsInsecure = 4,
		IsBroken = 1,
		IsSecure = 2,
		SecureHigh = 262144,
		SecureMed = 65536,
		SecureLow = 131072
	}
#endregion
}
