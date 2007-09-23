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

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using Mono.Mozilla;

namespace Mono.WebBrowser
{
	/// <summary>
	/// Summary description for WebBrowser.
	/// </summary>
	public class WebBrowser : Component, IWebBrowser, ICallback
	{
		private bool loaded;

		public WebBrowser()
		{
			loaded = false;
			Base.Init (this);
		}

		public void Load (IntPtr handle, int width, int height)
		{			
			Base.Bind (this, handle, width, height);
		}

		public void Shutdown ()
		{
			Base.Shutdown (this);			
		}

		#region Layout
		public void FocusIn (FocusOption focus)
		{
			Base.Focus (this, focus);
		}
		public void FocusOut ()
		{
			Base.Blur (this);
		}

		public void Activate ()
		{
			Base.Activate (this);
		}
		public void Deactivate ()
		{
			Base.Deactivate (this);
		}

		public void Resize (int width, int height)
		{
			Base.Resize (this, width, height);
		}

		#endregion

		#region Navigation
		public void Navigate (string uri)
		{
			Base.Navigate (this, uri);
		}

		/// <summary>
		/// Navigate to the next page in history
		/// </summary>
		/// <returns>False if it can't navigate forward</returns>
		public bool Forward ()
		{
			return Base.Forward (this);
		}

		/// <summary>
		/// Navigate to the previous page in history
		/// </summary>
		/// <returns>False if it can't navigate back</returns>
		public bool Back ()
		{
			return Base.Back (this);
		}

		public void Home ()
		{
			Base.Home (this);
		}

		/// <summary>
		/// Stop all activity
		/// </summary>
		public void Stop ()
		{
			Base.Stop (this);
		}

		/// <summary>
		/// Reload the current page in the browser
		/// </summary>
		public void Reload ()
		{
			Base.Reload (this, ReloadOption.None);
		}

		public void Reload (ReloadOption option)
		{
			Base.Reload (this, option);
		}
		#endregion



		#region Events
		static object KeyDownEvent = new object ();
		public event EventHandler KeyDown
		{
			add { Events.AddHandler (KeyDownEvent, value); }
			remove { Events.RemoveHandler (KeyDownEvent, value); }
		}

		static object KeyPressEvent = new object ();
		public event EventHandler KeyPress
		{
			add { Events.AddHandler (KeyPressEvent, value); }
			remove { Events.RemoveHandler (KeyPressEvent, value); }
		}
		static object KeyUpEvent = new object ();
		public event EventHandler KeyUp
		{
			add { Events.AddHandler (KeyUpEvent, value); }
			remove { Events.RemoveHandler (KeyUpEvent, value); }
		}
		static object MouseClickEvent = new object ();
		public event EventHandler MouseClick
		{
			add { Events.AddHandler (MouseClickEvent, value); }
			remove { Events.RemoveHandler (MouseClickEvent, value); }
		}
		static object MouseDoubleClickEvent = new object ();
		public event EventHandler MouseDoubleClick
		{
			add { Events.AddHandler (MouseDoubleClickEvent, value); }
			remove { Events.RemoveHandler (MouseDoubleClickEvent, value); }
		}
		static object MouseDownEvent = new object ();
		public event EventHandler MouseDown
		{
			add { Events.AddHandler (MouseDownEvent, value); }
			remove { Events.RemoveHandler (MouseDownEvent, value); }
		}
		static object MouseEnterEvent = new object ();
		public event EventHandler MouseEnter
		{
			add { Events.AddHandler (MouseEnterEvent, value); }
			remove { Events.RemoveHandler (MouseEnterEvent, value); }
		}
		static object MouseLeaveEvent = new object ();
		public event EventHandler MouseLeave
		{
			add { Events.AddHandler (MouseLeaveEvent, value); }
			remove { Events.RemoveHandler (MouseLeaveEvent, value); }
		}
		static object MouseMoveEvent = new object ();
		public event EventHandler MouseMove
		{
			add { Events.AddHandler (MouseMoveEvent, value); }
			remove { Events.RemoveHandler (MouseMoveEvent, value); }
		}
		static object MouseUpEvent = new object ();
		public event EventHandler MouseUp
		{
			add { Events.AddHandler (MouseUpEvent, value); }
			remove { Events.RemoveHandler (MouseUpEvent, value); }
		}

		static object FocusEvent = new object ();
		public event EventHandler Focus
		{
			add { Events.AddHandler (FocusEvent, value); }
			remove { Events.RemoveHandler (FocusEvent, value); }
		}

		static object CreateNewWindowEvent = new object ();
		public event CreateNewWindowEventHandler CreateNewWindow
		{
			add { Events.AddHandler (CreateNewWindowEvent, value); }
			remove { Events.RemoveHandler (CreateNewWindowEvent, value); }
		}

		static object AlertEvent = new object ();
		public event AlertEventHandler Alert
		{
			add { Events.AddHandler (AlertEvent, value); }
			remove { Events.RemoveHandler (AlertEvent, value); }
		}
		#endregion


		#region ICallback

		public void OnWidgetLoaded ()
		{
//			loaded = true;
		}


		public void GetControlSize (ref SizeInfo sz)
		{
			// TODO:  Add WebBrowser.GetControlSize implementation
		}

		public void OnJSStatus ()
		{
			// TODO:  Add WebBrowser.OnJSStatus implementation
		}

		public void OnLinkStatus ()
		{
			// TODO:  Add WebBrowser.OnLinkStatus implementation
		}

		public void OnDestroyBrowser ()
		{
			// TODO:  Add WebBrowser.OnDestroyBrowser implementation
		}

		public void OnClientSizeTo (Int32 width, Int32 height)
		{
			// TODO:  Add WebBrowser.OnClientSizeTo implementation
		}

		public void OnFocusNext ()
		{
			// TODO:  Add WebBrowser.OnFocusNext implementation
		}

		public void OnFocusPrev ()
		{
			// TODO:  Add WebBrowser.OnFocusPrev implementation
		}

		public void OnTitleChanged ()
		{
			// TODO:  Add WebBrowser.OnTitleChanged implementation
		}

		public void OnShowTooltipWindow (string tiptext, Int32 x, Int32 y)
		{
			// TODO:  Add WebBrowser.OnShowTooltipWindow implementation
		}

		public void OnHideTooltipWindow ()
		{
			// TODO:  Add WebBrowser.OnHideTooltipWindow implementation
		}

		public void OnStateNetStart ()
		{
			// TODO:  Add WebBrowser.OnStateNetStart implementation
		}

		public void OnStateNetStop ()
		{
			// TODO:  Add WebBrowser.OnStateNetStop implementation
		}

		public void OnStateSpecial (UInt32 stateFlags, Int32 status)
		{
			// TODO:  Add WebBrowser.OnStateSpecial implementation
		}

		public void OnStateChange (string URI, Int32 status, UInt32 stateFlags)
		{
			// TODO:  Add WebBrowser.OnStateChange implementation
		}

		public void OnProgress (Int32 currentTotalProgress, Int32 maxTotalProgress)
		{
			// TODO:  Add WebBrowser.OnProgress implementation
		}

		public void OnProgressAll (string URI, Int32 currentTotalProgress, Int32 maxTotalProgress)
		{
			// TODO:  Add WebBrowser.OnProgressAll implementation
		}

		public void OnLocationChanged ()
		{
			// TODO:  Add WebBrowser.OnLocationChanged implementation
		}

		public void OnStatusChange (string message, Int32 status)
		{
			// TODO:  Add WebBrowser.OnStatusChange implementation
		}

		public void OnSecurityChange (UInt32 state)
		{
			// TODO:  Add WebBrowser.OnSecurityChange implementation
		}

		public void OnVisibility (bool val)
		{
			// TODO:  Add WebBrowser.OnVisibility implementation
		}

		public bool OnClientDomKeyDown (KeyInfo keyInfo, ModifierKeys modifiers)
		{
			EventHandler eh = (EventHandler) (Events[KeyDownEvent]);
			if (eh != null) {
				EventArgs e = new EventArgs ();
				eh (this, e);
				return true;
			}
			return false;
		}

		public bool OnClientDomKeyUp (KeyInfo keyInfo, ModifierKeys modifiers)
		{
			EventHandler eh = (EventHandler) (Events[KeyUpEvent]);
			if (eh != null) {
				EventArgs e = new EventArgs ();
				eh (this, e);
				return true;
			}
			return false;
		}

		public bool OnClientDomKeyPress (KeyInfo keyInfo, ModifierKeys modifiers)
		{
			EventHandler eh = (EventHandler) (Events[KeyPressEvent]);
			if (eh != null) {
				EventArgs e = new EventArgs ();
				eh (this, e);
				return true;
			}
			return false;
		}

		public bool OnClientMouseDown (MouseInfo mouseInfo, ModifierKeys modifiers)
		{
			EventHandler eh = (EventHandler) (Events[MouseDownEvent]);
			if (eh != null) {
				EventArgs e = new EventArgs ();
				eh (this, e);
				return true;
			}
			return false;
		}

		public bool OnClientMouseUp (MouseInfo mouseInfo, ModifierKeys modifiers)
		{
			EventHandler eh = (EventHandler) (Events[MouseUpEvent]);
			if (eh != null) {
				EventArgs e = new EventArgs ();
				eh (this, e);
				return true;
			}
			return false;
		}

		public bool OnClientMouseClick (MouseInfo mouseInfo, ModifierKeys modifiers)
		{
			EventHandler eh = (EventHandler) (Events[MouseClickEvent]);
			if (eh != null) {
				EventArgs e = new EventArgs ();
				eh (this, e);
				return true;
			}
			return false;
		}

		public bool OnClientMouseDoubleClick (MouseInfo mouseInfo, ModifierKeys modifiers)
		{
			EventHandler eh = (EventHandler) (Events[MouseDoubleClickEvent]);
			if (eh != null) {
				EventArgs e = new EventArgs ();
				eh (this, e);
				return true;
			}
			return false;
		}

		public bool OnClientMouseOver (MouseInfo mouseInfo, ModifierKeys modifiers)
		{
			EventHandler eh = (EventHandler) (Events[MouseEnterEvent]);
			if (eh != null) {
				EventArgs e = new EventArgs ();
				eh (this, e);
				return true;
			}
			return false;
		}

		public bool OnClientMouseOut (MouseInfo mouseInfo, ModifierKeys modifiers)
		{
			EventHandler eh = (EventHandler) (Events[MouseLeaveEvent]);
			if (eh != null) {
				EventArgs e = new EventArgs ();
				eh (this, e);
				return true;
			}
			return false;
		}

		public bool OnClientActivate ()
		{
			// TODO:  Add WebBrowser.OnClientActivate implementation
			return false;
		}

		public bool OnClientFocusIn ()
		{
			// TODO:  Add WebBrowser.OnClientFocusIn implementation
			return false;
		}

		public bool OnClientFocusOut ()
		{
			// TODO:  Add WebBrowser.OnClientFocusOut implementation
			return false;
		}

		public bool OnBeforeURIOpen (string URL)
		{
			// TODO:  Add WebBrowser.OnBeforeURIOpen implementation
			return false;
		}

		public void OnFocus ()
		{
			EventHandler eh = (EventHandler) (Events[FocusEvent]);
			if (eh != null) {
				EventArgs e = new EventArgs ();
				eh (this, e);
			}
		}

		public bool OnCreateNewWindow ()
		{
			bool ret = false;
			CreateNewWindowEventHandler eh = (CreateNewWindowEventHandler) (Events[CreateNewWindowEvent]);
			if (eh != null) {
				CreateNewWindowEventArgs e = new CreateNewWindowEventArgs (false);
				ret = eh (this, e);
			}
			return ret;
		}

		public void OnAlert (IntPtr title, IntPtr text)
		{
			AlertEventHandler eh = (AlertEventHandler) (Events[AlertEvent]);
			if (eh != null) {
				AlertEventArgs e = new AlertEventArgs ();
				if (title != IntPtr.Zero)
					e.Title = Marshal.PtrToStringUni (title);
				if (text != IntPtr.Zero)
					e.Text = Marshal.PtrToStringUni (text);
				eh (this, e);
			}
		}

		public bool OnAlertCheck (IntPtr title, IntPtr text, IntPtr chkMsg, ref bool chkState)
		{
			AlertEventHandler eh = (AlertEventHandler) (Events[AlertEvent]);
			if (eh != null) {
				AlertEventArgs e = new AlertEventArgs ();
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
			AlertEventHandler eh = (AlertEventHandler) (Events[AlertEvent]);
			if (eh != null) {
				AlertEventArgs e = new AlertEventArgs ();
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
			AlertEventHandler eh = (AlertEventHandler) (Events[AlertEvent]);
			if (eh != null) {
				AlertEventArgs e = new AlertEventArgs ();
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

		public bool OnConfirmEx (IntPtr title, IntPtr text, DialogButtonFlags flags, 
								IntPtr title0, IntPtr title1, IntPtr title2, 
								IntPtr chkMsg, ref bool chkState, out Int32 retVal)
		{
			retVal = -1;

			AlertEventHandler eh = (AlertEventHandler) (Events[AlertEvent]);
			if (eh != null) {
				AlertEventArgs e = new AlertEventArgs ();
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

		public bool OnPrompt (IntPtr title, IntPtr text, IntPtr chkMsg, ref bool chkState, StringBuilder retVal)
		{
			retVal = new StringBuilder();
			AlertEventHandler eh = (AlertEventHandler) (Events[AlertEvent]);
			if (eh != null) {
				AlertEventArgs e = new AlertEventArgs ();
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

		public bool OnPromptUsernameAndPassword (IntPtr title, IntPtr text, IntPtr chkMsg, ref bool chkState, out IntPtr username, out IntPtr password)
		{
			username = IntPtr.Zero;
			password = IntPtr.Zero;
			AlertEventHandler eh = (AlertEventHandler) (Events[AlertEvent]);
			if (eh != null) {
				AlertEventArgs e = new AlertEventArgs ();
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
			password = IntPtr.Zero;
			AlertEventHandler eh = (AlertEventHandler) (Events[AlertEvent]);
			if (eh != null) {
				AlertEventArgs e = new AlertEventArgs ();
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
			retVal = 0;
			AlertEventHandler eh = (AlertEventHandler) (Events[AlertEvent]);
			if (eh != null) {
				AlertEventArgs e = new AlertEventArgs ();
				if (title != IntPtr.Zero)
					e.Title = Marshal.PtrToStringUni (title);
				if (text != IntPtr.Zero)
					e.Text = Marshal.PtrToStringUni (text);
				eh (this, e);
				return e.BoolReturn;
			}
			return false;
		}



		public void OnGeneric (IntPtr type)
		{
			string t = Marshal.PtrToStringUni (type);
			Trace.WriteLine (t);
			Trace.Flush ();

		}
		#endregion
	}
}
