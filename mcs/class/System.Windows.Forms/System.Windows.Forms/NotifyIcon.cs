//
// System.Windows.Forms.NotifyIcon.cs
//
// Author:
//   stubbed out by Paul Osman (paul.osman@sympatico.ca)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc
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
using System.ComponentModel;
using System.Drawing;
using System.Runtime.Remoting;

namespace System.Windows.Forms {

	// <summary>
	// </summary>
    public sealed class NotifyIcon : Component {

		//
		//  --- Constructor
		//
		[MonoTODO]
		public NotifyIcon()
		{
			
		}

		[MonoTODO]
		public NotifyIcon(IContainer container) {
			
		}
		//
		//  --- Public Properties
		//

		[MonoTODO]
		public ContextMenu ContextMenu {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}

		[MonoTODO]
		public Icon Icon {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}

		internal string text; //FIXME: just to get it to run
		[MonoTODO]
		public string Text {
			get {
				return text;
			}
			set {
				text = value;
			}
		}

		internal bool visible;//FIXME: just to get it to run
		[MonoTODO]
		public bool Visible {
			get {
				return visible;
			}
			set {
				visible = value;
			}
		}

		//
		//  --- Public Events
		//
		public event EventHandler Click;
		public event EventHandler DoubleClick;
		public event MouseEventHandler MouseDown;
		public event MouseEventHandler MouseMove;
		public event MouseEventHandler MouseUp;
		//
		//  --- Protected Methods
		//
		protected override void Dispose(bool disposing) {
			base.Dispose (disposing);
		}


	 }
}
