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
// Copyright (c) 2004 Novell, Inc.
//
// Authors:
//	Jackson Harper (jackson@ximian.com)

//
// TODO:
//  - Change cursor when mouse is over grip
//

using System.Collections;
using System.Drawing;
using System.Drawing.Text;
using System.Drawing.Imaging;

namespace System.Windows.Forms {

	public class StatusBar : Control {

		private StatusBarPanelCollection panels;

		private bool show_panels = false;
		private bool sizing_grip = true;

		private Rectangle paint_area = new Rectangle ();

		public StatusBar ()
		{
			base.Dock = DockStyle.Bottom;
			Anchor = AnchorStyles.Top | AnchorStyles.Left;
		}

		public string Text {
			get { return base.Text; }
			set {
				if (value == Text)
					return;
				base.Text = value;
				Refresh ();
			}
			
		}

		public bool ShowPanels {
			get { return show_panels; }
			set {
				if (show_panels == value)
					return;
				show_panels = value;
			}
		}

		public bool SizingGrip {
			get { return sizing_grip; }
			set {
				if (sizing_grip == value)
					return;
				sizing_grip = value;
			}
		}

		public Color ForeColor {
			get { return base.ForeColor; }
			set {
				if (value == ForeColor)
					return;
				if (ForeColorChanged != null)
					ForeColorChanged (this, EventArgs.Empty);
				Refresh ();
			}
		}

		public Color BackColor {
			get { return base.BackColor; }
			set {
				if (value == BackColor)
					return;
				base.BackColor = value;
				if (BackColorChanged != null)
					BackColorChanged (this, EventArgs.Empty);
				Refresh ();
			}
		}

		public Image BackgroundImage {
			get { return base.BackgroundImage; }
			set {
				if (value == BackgroundImage)
					return;
				base.BackgroundImage = value;
				if (BackgroundImageChanged != null)
					BackgroundImageChanged (this, EventArgs.Empty);
			}
		}

		public override DockStyle Dock {
			get { return base.Dock; }
			set {
				if (value == Dock)
					return;
				base.Dock = value;
				Refresh ();
			}
		}

		public override Font Font {
			get { return base.Font; }
			set {
				if (value == Font)
					return;
				base.Font = value;
				Refresh ();
			}
		}

		public new ImeMode ImeMode {
			get { return base.ImeMode; }
			set {
				if (value == ImeMode)
					return;
				base.ImeMode = value;
				if (ImeModeChanged != null)
					ImeModeChanged (this, EventArgs.Empty);
			}
		}

		public new bool TabStop {
			get { return base.TabStop; }
			set { base.TabStop = value; }
		}

		public StatusBarPanelCollection Panels {
			get {
				if (panels == null)
					panels = new StatusBarPanelCollection (this);
				return panels;
			}
		}

		public override string ToString ()
		{
			return base.ToString () + ", Panels.Count: " + Panels.Count +
				(Panels.Count > 0 ? ", Panels[0]: " + Panels [0] : String.Empty);
		}

		public new event EventHandler BackColorChanged;
		public new event EventHandler ForeColorChanged;
		public new event EventHandler BackgroundImageChanged;
		public event StatusBarDrawItemEventHandler DrawItem;
		public new event EventHandler ImeModeChanged;
		public new event PaintEventHandler Paint;
		public event StatusBarPanelClickEventHandler PanelClick;
		
		protected override CreateParams CreateParams {
			get {
				return base.CreateParams;
			}
		}

		protected override Size DefaultSize {
			get { return new Size (100, 22); }
		}

		protected override ImeMode DefaultImeMode {
			get { return ImeMode.Disable; }
		}

		protected override void WndProc(ref Message m)
		{
			switch ((Msg) m.Msg) {
			case Msg.WM_PAINT: {
				Rectangle	rect;
				PaintEventArgs	paint_event;

				paint_event = XplatUI.PaintEventStart (Handle);
				DoPaint (paint_event);
				XplatUI.PaintEventEnd (Handle);
				return;
			}
			}
			base.WndProc (ref m);
		}

		protected override void CreateHandle ()
		{
			base.CreateHandle ();
		}

		protected override void OnResize (EventArgs e)
		{
			base.OnResize (e);

			if (Width <= 0 || Height <= 0)
				return;

			UpdateArea ();
			CreateBuffers (Width, Height);
			CalcPanelSizes ();
			Draw ();
		}

		protected override void OnHandleCreated (EventArgs e)
		{
			base.OnHandleCreated (e);

			UpdateArea ();
			CreateBuffers (Width, Height);
			Draw();
		}

		protected override void OnHandleDestroyed (EventArgs e)
		{
			base.OnHandleDestroyed (e);
		}

		protected override void OnLayout (LayoutEventArgs e)
		{
			base.OnLayout (e);
		}

		protected override void OnMouseDown (MouseEventArgs e)
		{
                        if (panels == null)
                                return;

                        float prev_x = 0;
                        float gap = ThemeEngine.Current.StatusBarHorzGapWidth;
                        for (int i = 0; i < panels.Count; i++) {
                                float x = panels [i].Width + prev_x + (i == panels.Count - 1 ? gap : gap / 2);
                                if (e.X >= prev_x && e.X <= x) {
                                        OnPanelClick (new StatusBarPanelClickEventArgs (panels [i],
                                                                      e.Button, e.Clicks, e.X, e.Y));
                                        break;
                                }
                                prev_x = x;
                        }

			base.OnMouseDown (e);
		}

		protected virtual void OnPanelClick (StatusBarPanelClickEventArgs e)
		{
			if (PanelClick != null)
				PanelClick (this, e);
		}

		internal void OnDrawItemInternal (StatusBarDrawItemEventArgs e)
		{
			OnDrawItem (e);
		}

		protected virtual void OnDrawItem (StatusBarDrawItemEventArgs e)
		{
			if (DrawItem != null)
				DrawItem (this, e);
		}

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
		}

		private void DoPaint (PaintEventArgs pevent)
		{
		       if (Width <= 0 || Height <=  0 || Visible == false)
			       return;

		       UpdateArea ();
		       CalcPanelSizes ();
		       Draw();
		       pevent.Graphics.DrawImage (ImageBuffer, 0, 0);
		}

		private void CalcPanelSizes ()
		{
			if (panels == null || !show_panels)
				return;

			if (Width == 0 || Height == 0)
				return;

			int gap = ThemeEngine.Current.StatusBarHorzGapWidth;
			int taken = 0;
			ArrayList springs = null;
			for (int i = 0; i < panels.Count; i++) {
				StatusBarPanel p = panels [i];
				if (p.AutoSize == StatusBarPanelAutoSize.None) {
					taken += p.Width;
					taken += gap;
					continue;
				}
				if (p.AutoSize == StatusBarPanelAutoSize.Contents) {
					if (DeviceContext == null)
						CreateBuffers (Width, Height);
					int len = (int) (DeviceContext.MeasureString (p.Text, Font).Width + 0.5F);
					p.Width = (int) (len * 1.5F);
					taken += p.Width;
					taken += gap;
					continue;
				}
				if (p.AutoSize == StatusBarPanelAutoSize.Spring) {
					if (springs == null)
						springs = new ArrayList ();
					springs.Add (p);
					taken += gap;
					continue;
				}
			}

			if (springs == null)
				return;

			int spring_total = springs.Count;
			int total_width = Width - taken - ThemeEngine.Current.SizeGripWidth;
			for (int i = 0; i < spring_total; i++) {
				StatusBarPanel p = (StatusBarPanel) springs [i];
				p.Width = total_width / spring_total;
			}
		}

		private void UpdateArea ()
		{
			paint_area.X = paint_area.Y = 0;
			paint_area.Width = Width;
			paint_area.Height = Height;
		}

		private void Draw ()
		{
			ThemeEngine.Current.DrawStatusBar (DeviceContext, paint_area, this);
		}

		public class StatusBarPanelCollection :	 IList, ICollection, IEnumerable {

			private StatusBar owner;

			private ArrayList panels;

			public StatusBarPanelCollection (StatusBar owner)
			{
				this.owner = owner;
			}

			public virtual int Count {
				get {
					if (panels == null)
						return 0;
					return panels.Count;
				}
			}

			public virtual bool IsReadOnly {
				get { return false; }
			}

			bool IList.IsFixedSize {
				get { return false; }
			}

			bool ICollection.IsSynchronized {
				get { return panels.IsSynchronized; }
			}

			object ICollection.SyncRoot {
				get { return panels.SyncRoot; }
			}

			public virtual StatusBarPanel this [int index] {
				get {
					if (index < 0 || index >= Count)
						throw new ArgumentOutOfRangeException ("index");
					return (StatusBarPanel) panels [index];
				}
				set {
					if (value == null)
						throw new ArgumentNullException ("index");
					if (index < 0 || index >= Count)
						throw new ArgumentOutOfRangeException ("index");
					panels [index] = value;
				}
			}

			public virtual int Add (StatusBarPanel p)
			{
				return AddInternal (p, true);
			}

			public virtual StatusBarPanel Add (string text)
			{
				StatusBarPanel res = new StatusBarPanel ();
				res.Text = text;
				Add (res);
				return res;
			}

			private int AddInternal (StatusBarPanel p, bool refresh)
			{
				if (p == null)
					throw new ArgumentNullException ("value");
				if (panels == null)
					panels = new ArrayList ();

				int res = panels.Add (p);
				p.SetParent (owner);

				if (refresh) {
					owner.CalcPanelSizes ();
					owner.Refresh ();
				}

				return res;
			}

			public virtual void AddRange (StatusBarPanel [] range)
			{
				if (range == null)
					throw new ArgumentNullException ("panels");
				if (range.Length == 0)
					return;
				if (panels == null)
					panels = new ArrayList (range.Length);

				for (int i = 0; i < range.Length; i++)
					AddInternal (range [i], false);
				owner.Refresh ();
			}

			public virtual void Insert (int index, StatusBarPanel value)
			{
				if (value == null)
					throw new ArgumentNullException ("value");
				if (index > Count)
					throw new ArgumentOutOfRangeException ("index");
				// TODO: InvalidArgumentException for bad AutoSize values
				// although it seems impossible to set it to a bad value
				value.SetParent (owner);
				panels [index] = value;

				owner.Refresh ();
			}

			public virtual void Clear ()
			{
				panels.Clear ();

				owner.Refresh ();
			}

			public virtual bool Contains (StatusBarPanel panel)
			{
				return panels.Contains (panel);
			}

			public virtual IEnumerator GetEnumerator ()
			{
				return panels.GetEnumerator ();
			}

			public virtual int IndexOf (StatusBarPanel panel)
			{
				return panels.IndexOf (panel);
			}

			public virtual void Remove (StatusBarPanel panel)
			{
				panels.Remove (panel);
			}

			public virtual void RemoveAt (int index)
			{
				panels.RemoveAt (index);
			}

			void ICollection.CopyTo (Array dest, int index)
			{
				panels.CopyTo (dest, index);
			}

			object IList.this [int index] {
				get { return panels [index]; }
				set { panels [index] = value; }
			}

			int IList.Add (object value)
			{
				return panels.Add (value);
			}

			bool IList.Contains (object panel)
			{
				return panels.Contains (panel);
			}

			int IList.IndexOf (object panel)
			{
				return panels.IndexOf (panel);
			}

			void IList.Insert (int index, object value)
			{
				panels.Insert (index, value);
			}

			void IList.Remove (object value)
			{
				panels.Remove (value);
			}
		}
	}

}

