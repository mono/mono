//
// System.Windows.Forms.Form
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//   Joel Basson  (jstrike@mweb.co.za)
// (C) 2002 Ximian, Inc
//

using System;
using System.Drawing;
using Gtk;
using GtkSharp;

namespace System.Windows.Forms {

	public class Form : ContainerControl {
		internal Window win;
		string caption;
		bool firsttimeactivated=false;
		Size csize;
		Control menu = null;
		Button acceptbutton, cancelbutton; 
		bool autoscroll, autoscale, controlbox;
		Size autoscalecasesize = new Size ();
		
		
		public Form () : base ()
		{
		}

		static Form ()
		{
			// this happens to late (added this to Control's static constructor)
			Gtk.Application.Init ();
		}
		
		void delete_cb (object o, DeleteEventArgs args)
		{
			if (Closed != null)
				Closed (o, args);
		}
		
		void load_cb (object o, EventArgs args)
		{
			if (Load != null)
				Load (this, EventArgs.Empty);
		}

		void activate_cb (object o, EventArgs args)
		{
			if (Activated != null)
				Activated (o, args);
		}

		internal void ConnectEvents ()
		{
			win.DefaultActivated += new EventHandler (activate_cb);
			win.DeleteEvent += new DeleteEventHandler (delete_cb);
			win.Realized += new EventHandler (load_cb);
		}

		internal override Widget CreateWidget ()
		{
			Widget contents = base.CreateWidget ();
			win = new Window (WindowType.Toplevel);
			win.Title = Text;
			win.Add(contents);
			this.ConnectEvents();
			return (Widget) win;
		}

		//  --- Public Properties
		public Button AcceptButton 
		{
			get {
				return this.acceptbutton;
			}
			set {
				this.acceptbutton = value;
			}
		}

		// [MonoTODO]
		// public static Form ActiveForm {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//}
		// [MonoTODO]
		// public Form ActiveMdiChild {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//}

		[MonoTODO]
		public bool AutoScale {
			get {
				return this.autoscale;
			}
			set {
				// autoscalecasesize
				this.autoscale = value;
			}
		}

		[MonoTODO]
		public virtual Size AutoScaleBaseSize {
			get {
				return this.autoscalecasesize;
			}
			set {
				this.autoscalecasesize = value;
			}
		}
		[MonoTODO]
		public bool AutoScroll {
			get {
				return this.autoscroll;
			}
			set {
				this.autoscroll = value;
			}
		}

		// [MonoTODO]
		// public virtual Color BackColor {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//	set {
		//		throw new NotImplementedException ();
		//	}
		//}

		[MonoTODO]
		public Button CancelButton {
			get {
				return this.cancelbutton;
			}
			set {
				this.cancelbutton = value;
			}
		}

		[MonoTODO]
		public Size ClientSize {
			get {
				return csize;
			}
			set {
				csize = value;
				Widget.SetSizeRequest (value.Width,value.Height);
			}
		}
		[MonoTODO]
		public bool ControlBox {
			get {
				return this.controlbox;
			}
			set {
				this.controlbox = value;
			}
		}

		// [MonoTODO]
		// public Rectangle DesktopBounds {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//	set {
		//		throw new NotImplementedException ();
		//	}
		//}
		// [MonoTODO]
		// public Point DesktopLocation {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//	set {
		//		throw new NotImplementedException ();
		//	}
		//}
		// [MonoTODO]
		// public DialogResult DialogResult {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//	set {
		//		throw new NotImplementedException ();
		//	}
		//}
		// [MonoTODO]
		// public FormBorderStyle FormBorderStyle {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//	set {
		//		throw new NotImplementedException ();
		//	}
		//}
		[MonoTODO]
		public bool HelpButton {
			get {
				return false;
			}
			set {
				// Whatever
			}
		}

		[MonoTODO]
		public Icon Icon {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		 public bool IsMidiChild {
			get {
				return false;
			}
			set {
				// Whatever
			}
		}

		[MonoTODO]
		public bool IsMidiContainer {
			get {
				return false;
			}
			set {
				// Whatever
			}
		}

		[MonoTODO]
		public bool KeyPreview {
			get {
				return false;
			}
			set {
				// Whatever
			}
		}

		[MonoTODO]
		public bool MaximizeBox {
			get {
				return false;
			}
			set {
				// Whatever
			}
		}

		[MonoTODO]
		public Size MaximumSize {
			get {
				// TODO, return size of the screen here ?
				return new Size ();
			}
			set {
				// Whatever
			}
		}

		private Form[] mdichildren;
		[MonoTODO]
		public Form[] MdiChildren {
			get {
				return mdichildren;
			}
			set {
				this.mdichildren = value;
			}
		}

		private Form mdiparent;
		[MonoTODO]
		public Form MdiParent {
			get {
				return mdiparent;
			}
			set {
				mdiparent = value;
			}
		}

		public Control Menu {
			get {
				return this.menu;
			}
			set {
				this.menu = value;
				Control.Controls.Add(this.menu);
			}
		}

		 [MonoTODO]
		 public MainMenu MergedMenu {
			get {
				// What is this?
				return new MainMenu ();
			}
		}

		[MonoTODO]
		public bool MinimizeBox {
			get {
				return true;
			}
			set {
				// Whatever
			}
		}

		Size minimumsize = new Size();
		[MonoTODO]
		public Size MinimumSize {
			get {
				return this.minimumsize;
			}
			set {
				this.minimumsize = value;
			}
		}

		public bool Modal {
			get {
				return win.Modal;
			}
		}

		[MonoTODO]
		public double Opacity {
			get {
				return 1;
			}
			set {
				// whatever
			}
		}

		[MonoTODO]
		public Form[] OwnedForms {
			get {
				throw new NotImplementedException ();
			}
		}

		private Form owner;
		[MonoTODO]
		public Form Owner {
			get {
				return this.owner;
			}
			set {
				this.owner = value;
			}
		}

		bool showintaskbar;
		[MonoTODO]
		public bool ShowInTaskbar {
			get {
				return this.showintaskbar;
			}
			set {
				// We might need libegg for this
				this.showintaskbar = value;
			}
		}

		// [MonoTODO]
		// public override ISite Site {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//	set {
		//		throw new NotImplementedException ();
		//	}
		//}
		// [MonoTODO]
		// public SizeGripStyle SizeGripStyle {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//	set {
		//		throw new NotImplementedException ();
		//	}
		//}
		// [MonoTODO]
		// public FormStartPosition StartPosition {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//	set {
		//		throw new NotImplementedException ();
		//	}
		//}
		// [MonoTODO]
		// public bool TopLevel {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//	set {
		//		throw new NotImplementedException ();
		//	}
		//}
		// [MonoTODO]
		// public bool TopMost {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//	set {
		//		throw new NotImplementedException ();
		//	}
		//}
		// [MonoTODO]
		// public Color TransparencyKey {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//	set {
		//		throw new NotImplementedException ();
		//	}
		//}
		// [MonoTODO]
		// public FormWindowState WindowState {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//	set {
		//		throw new NotImplementedException ();
		//	}
		//}

		//
		//  --- Public Methods
		//
		// [MonoTODO]
		// public void Activate()
		// {
		//		throw new NotImplementedException ();
		// }
		// [MonoTODO]
		// public void AddOwnedForm(Form ownedForm)
		// {
		//		throw new NotImplementedException ();
		// }
		[MonoTODO]
		public void Close()
		{
			win.Destroy();
		}
		// [MonoTODO]
		// public void Dispose()
		// {
		//		throw new NotImplementedException ();
		// }
		// [MonoTODO]
		// public virtual bool Equals(object o);
		// {
		//		throw new NotImplementedException ();
		// }
		// [MonoTODO]
		// public static bool Equals(object o1, object o2);
		// {
		//		throw new NotImplementedException ();
		// }
		// [MonoTODO]
		// public static SizeF GetAutoScaleSize(Font font)
		// {
		//		throw new NotImplementedException ();
		// }
		// [MonoTODO]
		// public void Invalidate()
		// {
		//		throw new NotImplementedException ();
		// }
		// [MonoTODO]
		// public object Invoke()
		// {
		//		throw new NotImplementedException ();
		// }
		// [MonoTODO]
		// public void LayoutMdi(MdiLayout value)
		// {
		//		throw new NotImplementedException ();
		// }
		// [MonoTODO]
		// public void PerformLayout()
		// {
		//		throw new NotImplementedException ();
		// }
		// [MonoTODO]
		// public void RemoveOwnedForm(Form ownedForm)
		// {
		//		throw new NotImplementedException ();
		// }
		// [MonoTODO]
		 public void SuspendLayout()
		 {
		 }
		// [MonoTODO]
		 public void ResumeLayout()
		 {
		 }

		public void ResumeLayout (bool performLayout)
		{
		}

		// [MonoTODO]
		// public void Scale(float f)
		// {
		//		throw new NotImplementedException ();
		// }
		// [MonoTODO]
		// public void Select()
		// {
		//		throw new NotImplementedException ();
		// }
		// [MonoTODO]
		// public void SetBounds(int, int, int, int)
		// {
		//		throw new NotImplementedException ();
		// }
		// [MonoTODO]
		// public void SetDesktopLocation(int x, int y)
		// {
		//		throw new NotImplementedException ();
		// }
		// [MonoTODO]
		// public DialogResult ShowDialog()
		// {
		//		throw new NotImplementedException ();
		// }
		// [MonoTODO]
		// public override string ToString()
		// {
		//		throw new NotImplementedException ();
		// }

		//
		//  --- Public Events
		//
		[MonoTODO]
		public event EventHandler Activated;
		public event EventHandler Closed;
		
		// public event CancelEventHandler Closing;
		
		// [MonoTODO]
		// public event EventHandler Deactivate {
		//	add {
		//		throw new NotImplementedException ();
		//	}
		//	remove {
		//		throw new NotImplementedException ();
		//	}
		//}
		// [MonoTODO]
		// public event InputLanguageChangedEventHandler InputLanguageChanged {
		//	add {
		//		throw new NotImplementedException ();
		//	}
		//	remove {
		//		throw new NotImplementedException ();
		//	}
		//}
		// [MonoTODO]
		// public event InputLanguageChangingEventHandler InputLanguageChanging {
		//	add {
		//		throw new NotImplementedException ();
		//	}
		//	remove {
		//		throw new NotImplementedException ();
		//	}
		//}

		public event EventHandler Load;

		// [MonoTODO]
		// public event EventHandler  MaximizedBoundsChanged {
		//	add {
		//		throw new NotImplementedException ();
		//	}
		//	remove {
		//		throw new NotImplementedException ();
		//	}
		//}
		// [MonoTODO]
		// public event EventHandler  MaximumSizeChanged {
		//	add {
		//		throw new NotImplementedException ();
		//	}
		//	remove {
		//		throw new NotImplementedException ();
		//	}
		//}
		// [MonoTODO]
		// public event EventHandler  MdiChildActivate {
		//	add {
		//		throw new NotImplementedException ();
		//	}
		//	remove {
		//		throw new NotImplementedException ();
		//	}
		//}
		// [MonoTODO]
		// public event EventHandler  MenuComplete {
		//	add {
		//		throw new NotImplementedException ();
		//	}
		//	remove {
		//		throw new NotImplementedException ();
		//	}
		//}
		// [MonoTODO]
		// public event EventHandler  MenuStart {
		//	add {
		//		throw new NotImplementedException ();
		//	}
		//	remove {
		//		throw new NotImplementedException ();
		//	}
		//}
		// [MonoTODO]
		// public event EventHandler  MinimumSizedChanged {
		//	add {
		//		throw new NotImplementedException ();
		//	}
		//	remove {
		//		throw new NotImplementedException ();
		//	}
		//}

		//
		//  --- Protected Properties
		//
		// [MonoTODO]
		// protected override CreateParams CreateParams {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//}
		// [MonoTODO]
		// protected override ImeMode DefaultImeMode {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//}
		// [MonoTODO]
		// protected override Size DefaultSize {
		//}
		// [MonoTODO]
		// protected Rectangle MaximizedBounds {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//	set {
		//		throw new NotImplementedException ();
		//	}
		//}

		//
		//  --- Protected Methods
		//
		// [MonoTODO]
		// protected override void AdjustFormScrollbars(bool displayScrollbars)
		// {
		//		throw new NotImplementedException ();
		// }
		// [MonoTODO]
		// protected override ControlCollection CreateControlsInstnace()
		// {
		//		throw new NotImplementedException ();
		// }
		// [MonoTODO]
		// protected override void CreateHandle()
		// {
		//		throw new NotImplementedException ();
		// }
		// [MonoTODO]
		// protected override void DefWndProc(ref Message m)
		// {
		//		throw new NotImplementedException ();
		// }
		// [MonoTODO]
		// protected override void Dispose(bool b)
		// {
		//		throw new NotImplementedException ();
		// }

		// [MonoTODO]
		// protected virtual void  OnClosed(EventArgs e)
		// {
		//		throw new NotImplementedException ();
		// }
		// [MonoTODO]
		// protected virtual void  OnClosing(CancelEventArgs e)
		// {
		//		throw new NotImplementedException ();
		// }

		// [MonoTODO]
		// protected override void OnCreateControl()
		// {
		//		throw new NotImplementedException ();
		// }
		// [MonoTODO]
		// protected override void OnFontChanged(EventArgs e)
		// {
		//		throw new NotImplementedException ();
		// }
		// [MonoTODO]
		// protected override void OnHandleCreated(EventArgs e)
		// {
		//		throw new NotImplementedException ();
		// }
		// [MonoTODO]
		// protected override void OnHandleDestroyed(EventArgs e)
		// {
		//		throw new NotImplementedException ();
		// }

		// [MonoTODO]
		// protected virtual void OnInputLanguageChanged( OnInputLanguageChangedEventArgs e)
		// {
		//		throw new NotImplementedException ();
		// }
		// [MonoTODO]
		// protected virtual void OnInputLanguagedChanging( OnInputLanguagedChangingEventArgs e)
		// {
		//		throw new NotImplementedException ();
		// }
		// [MonoTODO]
		// protected virtual void OnLoad(EventArgs e)
		// {
		//		throw new NotImplementedException ();
		// }
		// [MonoTODO]
		// protected virtual void OnMaximizedBoundsChanged(EventArgs e)
		// {
		//		throw new NotImplementedException ();
		// }
		// [MonoTODO]
		// protected virtual void OnMaximumSizedChanged(EventArgs e)
		// {
		//		throw new NotImplementedException ();
		// }
		// [MonoTODO]
		// protected virtual void OnMdiChildActive(EventArgs e)
		// {
		//		throw new NotImplementedException ();
		// }
		// [MonoTODO]
		// protected virtual void OnMenuComplete(EventArgs e)
		// {
		//		throw new NotImplementedException ();
		// }
		// [MonoTODO]
		// protected virtual void OnMenuStart(EventArgs e)
		// {
		//		throw new NotImplementedException ();
		// }
		// [MonoTODO]
		// protected virtual void OnMinimumSizeChanged(EventArgs e)
		// {
		//		throw new NotImplementedException ();
		// }

		// [MonoTODO]
		// protected override void  OnPaint(EventArgs e)
		// {
		//		throw new NotImplementedException ();
		// }
		// [MonoTODO]
		// protected override void  OnResize(EventArgs e)
		// {
		//		throw new NotImplementedException ();
		// }
		// [MonoTODO]
		// protected override void  OnStyleChanged(EventArgs e)
		// {
		//		throw new NotImplementedException ();
		// }
		
		protected override void  OnTextChanged(EventArgs e)
		{
			if (win != null)
				win.Title = Text;
		}

		// [MonoTODO]
		// protected override void  OnVisibleChanged(EventArgs e)
		// {
		//		throw new NotImplementedException ();
		// }

		// [MonoTODO]
		// protected override bool ProcessCmdKey( ref Message msg, Keys keyData)
		// {
		//		throw new NotImplementedException ();
		// }
		// [MonoTODO]
		// protected override bool ProcessDialogKey(Keys keyData)
		// {
		//		throw new NotImplementedException ();
		// }
		// [MonoTODO]
		// protected override bool ProcessKeyPreview(ref Message m)
		// {
		//		throw new NotImplementedException ();
		// }
		// [MonoTODO]
		// protected override bool ProcessTabKey(bool forward)
		// {
		//		throw new NotImplementedException ();
		// }

		// [MonoTODO]
		// protected override void ScaleScore(float x, float y)
		// {
		//		throw new NotImplementedException ();
		// }
		// [MonoTODO]
		// protected override void Select(bool b1, bool b2)
		// {
		//		throw new NotImplementedException ();
		// }
		// [MonoTODO]
		// protected override void SetBoundsCore(int x, int y,  int width,  int height,  BoundsSpecified specified)
		// {
		//		throw new NotImplementedException ();
		// }
		// [MonoTODO]
		// protected override void SelectClientSizeCore(int x, int y)
		// {
		//		throw new NotImplementedException ();
		// }
		// [MonoTODO]
		// protected override void SetVisibleCore(bool value)
		// {
		//		throw new NotImplementedException ();
		// }

		// [MonoTODO]
		// protected void UpdateBounds()
		// {
		//		throw new NotImplementedException ();
		// }
		// [MonoTODO]
		// protected override void WndProc(ref Message m)
		// {
		//		throw new NotImplementedException ();
		// }
	}
}
