//
// System.Windows.Forms.Design.WndProcRouter
//
// Authors:
//	  Ivan N. Zlatev (contact i-nZ.net)
//
// (C) 2006-2007 Ivan N. Zlatev

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
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Design;
using System.Collections;

	// Automatically reroutes Messages to the designer
	//
	
namespace System.Windows.Forms.Design
{

	internal class WndProcRouter : IWindowTarget, IDisposable
	{
		private IWindowTarget _oldTarget;
		private IMessageReceiver _receiver;
		private Control _control;
		
		public WndProcRouter (Control control, IMessageReceiver receiver)
		{
			if (control == null)
				throw new ArgumentNullException ("control");
			if (receiver == null)
				throw new ArgumentNullException ("receiver");
			
			_oldTarget = control.WindowTarget;
			_control = control;
			_receiver = receiver;
		}
		
		public Control Control {
			get { return _control; }
		}

		public IWindowTarget OldWindowTarget {
			get { return _oldTarget; }
		}

		// Route the message to the control
		//
		public void ToControl (ref Message m)
		{
			//Console.WriteLine ("Control: " + ((Native.Msg)m.Msg).ToString ());
			if (_oldTarget != null)
				_oldTarget.OnMessage (ref m);
		}
		
		public void ToSystem (ref Message m)
		{
			//Console.WriteLine ("System: " + ((Native.Msg)m.Msg).ToString ());
			Native.DefWndProc (ref m);
		}
		
		// Just pass it to the old IWindowTarget
		//
		void IWindowTarget.OnHandleChange (IntPtr newHandle)
		{
			if (_oldTarget != null)
				_oldTarget.OnHandleChange (newHandle);
		}

		// Route the msg to the designer if available, else to
		// control itself.
		//
		void IWindowTarget.OnMessage (ref Message m)
		{
			//Console.WriteLine ("Message: " + ((Native.Msg)m.Msg).ToString ());
			if (_receiver != null)
				_receiver.WndProc (ref m);
			else
				this.ToControl (ref m);
		}

		// Disposes and puts back the old IWindowTarget
		//
		public void Dispose ()
		{
			if (_control != null)
				_control.WindowTarget = _oldTarget;

			_control = null;
			_oldTarget = null;
		}

	}
}
