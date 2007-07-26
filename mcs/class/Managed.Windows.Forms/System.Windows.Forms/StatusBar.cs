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
// Copyright (c) 2004-2005 Novell, Inc.
//
// Authors:
//	Jackson Harper (jackson@ximian.com)

//
// TODO:
//  - Change cursor when mouse is over grip
//

using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Text;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace System.Windows.Forms {
#if NET_2_0
	[ComVisible (true)]
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
#endif
	[DefaultEvent("PanelClick")]
	[Designer("System.Windows.Forms.Design.StatusBarDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	[DefaultProperty("Text")]
	public class StatusBar : Control {
		#region Fields
		private StatusBarPanelCollection panels;

		private bool show_panels = false;
		private bool sizing_grip = true;

		#endregion	// Fields

		#region Public Constructors
		public StatusBar ()
		{
			Dock = DockStyle.Bottom;
			this.TabStop = false;
			this.SetStyle(ControlStyles.UserPaint | ControlStyles.Selectable, false);
		}
		#endregion	// Public Constructors

		#region	Public Instance Properties
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override Color BackColor {
			get { return base.BackColor; }
			set { base.BackColor = value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override Image BackgroundImage {
			get { return base.BackgroundImage; }
			set { base.BackgroundImage = value; }
		}

#if NET_2_0
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override ImageLayout BackgroundImageLayout {
			get {
				return base.BackgroundImageLayout;
			}
			set {
				base.BackgroundImageLayout = value;
			}
		}
#endif

		[Localizable(true)]
		[DefaultValue(DockStyle.Bottom)]
		public override DockStyle Dock {
			get { return base.Dock; }
			set { base.Dock = value; }
		}

#if NET_2_0
		[EditorBrowsable (EditorBrowsableState.Never)]
		protected override bool DoubleBuffered {
			get {
				return base.DoubleBuffered;
			}
			set {
				base.DoubleBuffered = value;
			}
		}
#endif
		[Localizable(true)]
		public override Font Font {
			get { return base.Font; }
			set {
				if (value == Font)
					return;
				base.Font = value;
				UpdateStatusBar ();
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override Color ForeColor {
			get { return base.ForeColor; }
			set { base.ForeColor = value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new ImeMode ImeMode {
			get { return base.ImeMode; }
			set { base.ImeMode = value; }
		}

		[MergableProperty(false)]
		[Localizable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public StatusBarPanelCollection Panels {
			get {
				if (panels == null)
					panels = new StatusBarPanelCollection (this);
				return panels;
			}
		}

		[DefaultValue(false)]
		public bool ShowPanels {
			get { return show_panels; }
			set {
				if (show_panels == value)
					return;
				show_panels = value;
				UpdateStatusBar ();
			}
		}

		[DefaultValue(true)]
		public bool SizingGrip {
			get { return sizing_grip; }
			set {
				if (sizing_grip == value)
					return;
				sizing_grip = value;
				UpdateStatusBar ();
			}
		}

		[DefaultValue(false)]
		public new bool TabStop {
			get { return base.TabStop; }
			set { base.TabStop = value; }
		}

		[Localizable(true)]
		public override string Text {
			get { return base.Text; }
			set {
				if (value == Text)
					return;
				base.Text = value;
				UpdateStatusBar ();
			}
			
		}

		#endregion Public Instance Properties

		#region Protected Instance Properties
		protected override CreateParams CreateParams {
			get {
				return base.CreateParams;
			}
		}

		protected override ImeMode DefaultImeMode {
			get { return ImeMode.Disable; }
		}

		protected override Size DefaultSize {
			get { return ThemeEngine.Current.StatusBarDefaultSize; }
		}

		#endregion	// Protected Instance Properties

		#region Public Instance Methods
		public override string ToString () {
			return base.ToString () + ", Panels.Count: " + Panels.Count +
				(Panels.Count > 0 ? ", Panels[0]: " + Panels [0] : String.Empty);
		}

		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		protected override void CreateHandle ()
		{
			base.CreateHandle ();
		}

		protected override void Dispose (bool disposing) {
			base.Dispose (disposing);
		}

		protected virtual void OnDrawItem (StatusBarDrawItemEventArgs e) {
			StatusBarDrawItemEventHandler eh = (StatusBarDrawItemEventHandler)(Events [DrawItemEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override void OnHandleCreated (EventArgs e) {
			base.OnHandleCreated (e);
			CalcPanelSizes ();
		}

		protected override void OnHandleDestroyed (EventArgs e) {
			base.OnHandleDestroyed (e);
		}

		protected override void OnLayout (LayoutEventArgs e) {
			base.OnLayout (e);
		}

		protected override void OnMouseDown (MouseEventArgs e) {
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

		protected virtual void OnPanelClick (StatusBarPanelClickEventArgs e) {
			StatusBarPanelClickEventHandler eh = (StatusBarPanelClickEventHandler)(Events [PanelClickEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override void OnResize (EventArgs e)
		{
			base.OnResize (e);

			if (Width <= 0 || Height <= 0)
				return;

			UpdateStatusBar ();
		}

		protected override void WndProc(ref Message m) {
			base.WndProc (ref m);
		}

		#endregion	// Methods


		#region Internal Methods
		internal void OnDrawItemInternal (StatusBarDrawItemEventArgs e)
		{
			OnDrawItem (e);
		}

		internal void UpdatePanel (StatusBarPanel panel)
		{
			if (panel.AutoSize == StatusBarPanelAutoSize.Contents) {
				UpdateStatusBar ();
				return;
			}

			UpdateStatusBar ();
		}

		internal void UpdatePanelContents (StatusBarPanel panel)
		{
			if (panel.AutoSize == StatusBarPanelAutoSize.Contents) {
				UpdateStatusBar ();
				Invalidate ();
				return;
			}

			Invalidate (new Rectangle (panel.X + 2, 2, panel.Width - 4, bounds.Height - 4));
		}

		void UpdateStatusBar ()
		{
			CalcPanelSizes ();
			Refresh ();
		}

		internal override void OnPaintInternal (PaintEventArgs pevent)
		{
			Draw (pevent.Graphics, pevent.ClipRectangle);
		}

		private void CalcPanelSizes ()
		{
			if (panels == null || !show_panels)
				return;

			if (Width == 0 || Height == 0)
				return;

			int border = 2;
			int gap = ThemeEngine.Current.StatusBarHorzGapWidth;
			int taken = 0;
			ArrayList springs = null;

			taken = border;
			for (int i = 0; i < panels.Count; i++) {
				StatusBarPanel p = panels [i];

				if (p.AutoSize == StatusBarPanelAutoSize.None) {
					taken += p.Width;
					taken += gap;
					continue;
				}
				if (p.AutoSize == StatusBarPanelAutoSize.Contents) {
					int len = (int)(TextRenderer.MeasureString (p.Text, Font).Width + 0.5F);
					p.SetWidth (len + 8);
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
			int total_width = Width - taken - (SizingGrip ? ThemeEngine.Current.StatusBarSizeGripWidth : 0);
			for (int i = 0; i < spring_total; i++) {
				StatusBarPanel p = (StatusBarPanel) springs [i];
				int width = total_width / spring_total;
				p.SetWidth (width >= p.MinWidth ? width : p.MinWidth);
			}

			taken = border;
			for (int i = 0; i < panels.Count; i++) {
				StatusBarPanel p = panels [i];
				p.X = taken;
				taken += p.Width + gap;
			}
		}

		private void Draw (Graphics dc, Rectangle clip)
		{
			ThemeEngine.Current.DrawStatusBar (dc, clip, this);
			
		}
		#endregion	// Internal Methods


		#region Events
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler BackColorChanged {
			add { base.BackColorChanged += value; }
			remove { base.BackColorChanged -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageChanged {
			add { base.BackgroundImageChanged += value; }
			remove { base.BackgroundImageChanged -= value; }
		}

#if NET_2_0
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageLayoutChanged
		{
			add { base.BackgroundImageLayoutChanged += value; }
			remove { base.BackgroundImageLayoutChanged -= value; }
		}
#endif	

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler ForeColorChanged {
			add { base.ForeColorChanged += value; }
			remove { base.ForeColorChanged -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler ImeModeChanged {
			add { base.ImeModeChanged += value; }
			remove { base.ImeModeChanged -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event PaintEventHandler Paint {
			add { base.Paint += value; }
			remove { base.Paint -= value; }
		}

		static object DrawItemEvent = new object ();
		static object PanelClickEvent = new object ();

		public event StatusBarDrawItemEventHandler DrawItem {
			add { Events.AddHandler (DrawItemEvent, value); }
			remove { Events.RemoveHandler (DrawItemEvent, value); }
		}

		public event StatusBarPanelClickEventHandler PanelClick {
			add { Events.AddHandler (PanelClickEvent, value); }
			remove { Events.RemoveHandler (PanelClickEvent, value); }
		}
		#endregion	// Events
		

		#region Subclass StatusBarPanelCollection
#if NET_2_0
		[ListBindable (false)]
#endif
		public class StatusBarPanelCollection :	 IList, ICollection, IEnumerable {
			#region Fields
			private StatusBar owner;
			private ArrayList panels;
#if NET_2_0
			private int last_index_by_key;
#endif
			#endregion	// Fields

			#region Public Constructors
			public StatusBarPanelCollection (StatusBar owner)
			{
				this.owner = owner;
			}

			#endregion	// Public Constructors

			#region Private & Internal Methods
			private int AddInternal (StatusBarPanel p, bool refresh) {
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

			#endregion	// Private & Internal Methods

			#region Public Instance Properties
			[Browsable(false)]
			[EditorBrowsable(EditorBrowsableState.Never)]
			public int Count {
				get {
					if (panels == null)
						return 0;
					return panels.Count;
				}
			}

			public bool IsReadOnly {
				get { return false; }
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
			
#if NET_2_0

			public virtual StatusBarPanel this [string key] {
				get {
					int index = IndexOfKey (key);
					if (index >= 0 && index < Count) {
						return (StatusBarPanel) panels [index];
					} 
					return null;
				}
			}
#endif

			#endregion	// Public Instance Properties

			#region Public Instance Methods
			public virtual int Add (StatusBarPanel p) {
				return AddInternal (p, true);
			}

			public virtual StatusBarPanel Add (string text) {
				StatusBarPanel res = new StatusBarPanel ();
				res.Text = text;
				Add (res);
				return res;
			}

			public virtual void AddRange (StatusBarPanel [] range) {
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

			public virtual void Clear () {
				panels.Clear ();

				owner.Refresh ();
			}

			public bool Contains (StatusBarPanel panel) {
				return panels.Contains (panel);
			}

#if NET_2_0
			public virtual bool ContainsKey (string key)
			{
				int index = IndexOfKey (key);
				return index >= 0 && index < Count;
			}
#endif

			public IEnumerator GetEnumerator () {
				return panels.GetEnumerator ();
			}

			public int IndexOf (StatusBarPanel panel) {
				return panels.IndexOf (panel);
			}
			
#if NET_2_0
			public virtual int IndexOfKey (string key)
			{
				if (key == null || key == string.Empty)
					return -1;
				
				if (last_index_by_key >= 0 && last_index_by_key < Count &&
					String.Compare (((StatusBarPanel)panels [last_index_by_key]).Name, key, StringComparison.OrdinalIgnoreCase) == 0) {
					return last_index_by_key;
				}
					
				for (int i = 0; i < Count; i++) {
					StatusBarPanel item;
					item = panels [i] as StatusBarPanel;
					if (item != null && String.Compare (item.Name, key, StringComparison.OrdinalIgnoreCase) == 0) {
						last_index_by_key = i;
						return i;
					}
				}
				
				return -1;
			}
#endif

			public virtual void Insert (int index, StatusBarPanel value) {
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

			public virtual void Remove (StatusBarPanel panel) {
				panels.Remove (panel);
			}

			public virtual void RemoveAt (int index) {
				panels.RemoveAt (index);
			}

#if NET_2_0
			public virtual void RemoveByKey (string key)
			{
				int index = IndexOfKey (key);
				if (index >= 0 && index < Count)
					RemoveAt (index);
			}
#endif

			#endregion	// Public Instance Methods

			#region IList & ICollection Interfaces
			bool ICollection.IsSynchronized {
				get { return panels.IsSynchronized; }
			}

			object ICollection.SyncRoot {
				get { return panels.SyncRoot; }
			}

			void ICollection.CopyTo (Array dest, int index)
			{
				panels.CopyTo (dest, index);
			}


			object IList.this [int index] {
				get { return panels [index]; }
				set { panels [index] = value; }
			}

			int IList.Add (object value) {
				return panels.Add (value);
			}

			bool IList.Contains (object panel) {
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

			bool IList.IsFixedSize {
				get { return false; }
			}

			void IList.Remove (object value)
			{
				panels.Remove (value);
			}
			#endregion	// IList & ICollection Interfaces
		}
		#endregion	// Subclass StatusBarPanelCollection
	}

}

