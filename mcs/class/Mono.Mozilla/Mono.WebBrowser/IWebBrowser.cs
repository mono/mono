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
using System.Text;
using System.Runtime.InteropServices;

namespace Mono.WebBrowser
{
	public interface IWebBrowser
	{
		void Load (IntPtr handle, int width, int height);
		void Shutdown ();
		void FocusIn (FocusOption focus);
		void FocusOut ();
		void Activate ();
		void Deactivate ();
		void Resize (int width, int height);
		void Navigate (string uri);
		bool Forward ();
		bool Back ();
		void Home ();
		void Stop ();
		void Reload ();
		void Reload (ReloadOption option);


		event EventHandler KeyDown;
		event EventHandler KeyPress;
		event EventHandler KeyUp;
		event EventHandler MouseClick;
		event EventHandler MouseDoubleClick;
		event EventHandler MouseDown;
		event EventHandler MouseEnter;
		event EventHandler MouseLeave;
		event EventHandler MouseMove;
		event EventHandler MouseUp;
		event EventHandler Focus;
		event CreateNewWindowEventHandler CreateNewWindow;

	}

	public enum ReloadOption
	{
		None = 0,
		Proxy = 1,
		Full = 2
	}

	public enum FocusOption
	{
		None = 0,
		FocusFirstElement = 1,
		FocusLastElement = 2
	}

	public delegate bool CreateNewWindowEventHandler (object sender, CreateNewWindowEventArgs e);
	public class CreateNewWindowEventArgs : EventArgs
	{
		private bool isModal;

		#region Public Constructors
		public CreateNewWindowEventArgs (bool isModal)
			: base ()
		{
			this.isModal = isModal;
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
		public bool IsModal
		{
			get { return this.isModal; }
		}
		#endregion	// Public Instance Properties
	}


}
