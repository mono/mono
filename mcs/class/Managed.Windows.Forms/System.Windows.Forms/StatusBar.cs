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
			Dock = DockStyle.Bottom;
			Anchor = AnchorStyles.Top | AnchorStyles.Left;
		}

		public bool ShowPanels {
			get { return show_panels; }
			set {
				bool refresh = (show_panels != value);
				show_panels = value;
				if (refresh)
					Refresh ();
			}
		}

		public bool SizingGrip {
			get { return sizing_grip; }
			set {
				bool refresh = (sizing_grip != value);
				sizing_grip = value;
				if (refresh)
					Refresh ();
			}
		}

		public StatusBarPanelCollection Panels {
			get {
				if (panels == null)
					panels = new StatusBarPanelCollection (this);
				return panels;
			}
		}

		protected override Size DefaultSize {
			get { return new Size (100, 22); }
		}

		protected override void WndProc(ref Message m)
		{
			switch ((Msg) m.Msg) {
			case Msg.WM_PAINT: {
				Rectangle	rect;
				PaintEventArgs	paint_event;

				paint_event = XplatUI.PaintEventStart (Handle);
				Paint (paint_event);
				XplatUI.PaintEventEnd (Handle);
				return;
			}
			}
			base.WndProc (ref m);
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

		private void Paint (PaintEventArgs pevent)
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

