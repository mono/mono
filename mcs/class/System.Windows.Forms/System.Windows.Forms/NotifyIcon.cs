//
// System.Windows.Forms.NotifyIcon.cs
//
// Author:
//   stubbed out by Paul Osman (paul.osman@sympatico.ca)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc
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
