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

using System;
using System.Threading;
using System.ComponentModel;


// Some implementation details:
// http://msdn.microsoft.com/msdnmag/issues/06/06/NETMatters/default.aspx
namespace System.Windows.Forms
{
	public sealed class WindowsFormsSynchronizationContext : SynchronizationContext, IDisposable
	{
		private static bool auto_installed;
		private static Control invoke_control;
		private static SynchronizationContext previous_context;
		
		#region Public Constructor
		public WindowsFormsSynchronizationContext ()
		{
		}
		
		static WindowsFormsSynchronizationContext ()
		{
			invoke_control = new Control ();
			invoke_control.CreateControl ();
			auto_installed = true;
			previous_context = SynchronizationContext.Current;
		}
		#endregion

		#region Public Properties
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public static bool AutoInstall {
			get { return auto_installed; }
			set { auto_installed = value; }
		}
		#endregion

		#region Public Methods
		public override SynchronizationContext CreateCopy ()
		{
			return base.CreateCopy ();
		}
		
		public void Dispose ()
		{
		}

		public override void Post (SendOrPostCallback d, object state)
		{
			invoke_control.BeginInvoke (d, new object[] { state });
		}

		public override void Send (SendOrPostCallback d, object state)
		{
			invoke_control.Invoke (d, new object[] { state });
		}
		
		public static void Uninstall ()
		{
			if (previous_context == null)
				previous_context = new SynchronizationContext ();
				
			SynchronizationContext.SetSynchronizationContext (previous_context);
		}
		#endregion
	}
}
