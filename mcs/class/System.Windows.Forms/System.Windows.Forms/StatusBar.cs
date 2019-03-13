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
	[ComVisible (true)]
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[DefaultEvent("PanelClick")]
	[Designer("System.Windows.Forms.Design.StatusBarDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	[DefaultProperty("Text")]
	public class StatusBar : Control {
		#region Fields
		private StatusBarPanelCollection panels;

		private bool show_panels = false;
		private bool sizing_grip = true;
		
		// Stuff for panel Tooltips
		private Timer tooltip_timer;
		private ToolTip tooltip_window;
		private StatusBarPanel tooltip_currently_showing;
		#endregion	// Fields

		#region Public Constructors
		public StatusBar ()
		{
			Dock = DockStyle.Bottom;
			this.TabStop = false;
			this.SetStyle(ControlStyles.UserPaint | ControlStyles.Selectable, false);

			// For displaying/hiding tooltips
			MouseMove += new MouseEventHandler (StatusBar_MouseMove);
			MouseLeave += new EventHandler (StatusBar_MouseLeave);
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

		[Localizable(true)]
		[DefaultValue(DockStyle.Bottom)]
		public override DockStyle Dock {
			get { return base.Dock; }
			set { base.Dock = value; }
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		protected override bool DoubleBuffered {
			get {
				return base.DoubleBuffered;
			}
			set {
				base.DoubleBuffered = value;
			}
		}

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
			if (!IsDisposed && disposing)
			{
				if (tooltip_timer != null)
					tooltip_timer.Dispose();

				if (tooltip_window != null)
					tooltip_window.Dispose();

				if (panels != null) {
					var copiedPanels = new StatusBarPanel [panels.Count];
					((ICollection) panels).CopyTo (copiedPanels, 0);
					panels.Clear ();

					foreach (StatusBarPanel panel in copiedPanels) {
						panel.Dispose ();
					}
				}
			}

			base.Dispose (disposing);
		}

		protected virtual void OnDrawItem (StatusBarDrawItemEventArgs sbdievent) {
			StatusBarDrawItemEventHandler eh = (StatusBarDrawItemEventHandler)(Events [DrawItemEvent]);
			if (eh != null)
				eh (this, sbdievent);
		}

		protected override void OnHandleCreated (EventArgs e) {
			base.OnHandleCreated (e);
			CalcPanelSizes ();
		}

		protected override void OnHandleDestroyed (EventArgs e) {
			base.OnHandleDestroyed (e);
		}

		protected override void OnLayout (LayoutEventArgs levent) {
			base.OnLayout (levent);
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
					if (p.Icon != null) {
						len += 21;
					}
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

			if (springs != null) {
				int spring_total = springs.Count;
				int total_width = Width - taken - (SizingGrip ? ThemeEngine.Current.StatusBarSizeGripWidth : 0);
				for (int i = 0; i < spring_total; i++) {
					StatusBarPanel p = (StatusBarPanel)springs[i];
					int width = total_width / spring_total;
					p.SetWidth(width >= p.MinWidth ? width : p.MinWidth);
				}
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

		#region Stuff for ToolTips
		private void StatusBar_MouseMove (object sender, MouseEventArgs e)
		{
			if (!show_panels)
				return;
				
			StatusBarPanel p = GetPanelAtPoint (e.Location);
			
			if (p != tooltip_currently_showing)
				MouseLeftPanel (tooltip_currently_showing);
				
			if (p != null && tooltip_currently_showing == null)
				MouseEnteredPanel (p);
		}

		private void StatusBar_MouseLeave (object sender, EventArgs e)
		{
			if (tooltip_currently_showing != null)
				MouseLeftPanel (tooltip_currently_showing);
		}

		private StatusBarPanel GetPanelAtPoint (Point point)
		{
			foreach (StatusBarPanel p in Panels)
				if (point.X >= p.X && point.X <= (p.X + p.Width))
					return p;
					
			return null;
		}
		
		private void MouseEnteredPanel (StatusBarPanel item)
		{
			tooltip_currently_showing = item;
			ToolTipTimer.Start ();
		}

		private void MouseLeftPanel (StatusBarPanel item)
		{
			ToolTipTimer.Stop ();
			ToolTipWindow.Hide (this);
			tooltip_currently_showing = null;
		}

		private Timer ToolTipTimer {
			get {
				if (tooltip_timer == null) {
					tooltip_timer = new Timer ();
					tooltip_timer.Enabled = false;
					tooltip_timer.Interval = 500;
					tooltip_timer.Tick += new EventHandler (ToolTipTimer_Tick);
				}

				return tooltip_timer;
			}
		}

		private ToolTip ToolTipWindow {
			get {
				if (tooltip_window == null)
					tooltip_window = new ToolTip ();

				return tooltip_window;
			}
		}

		private void ToolTipTimer_Tick (object o, EventArgs args)
		{
			string tooltip = tooltip_currently_showing.ToolTipText;

			if (tooltip != null && tooltip.Length > 0)
				ToolTipWindow.Present (this, tooltip);

			ToolTipTimer.Stop ();
		}
		#endregion

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

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageLayoutChanged
		{
			add { base.BackgroundImageLayoutChanged += value; }
			remove { base.BackgroundImageLayoutChanged -= value; }
		}

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
		[ListBindable (false)]
		public class StatusBarPanelCollection :	 IList, ICollection, IEnumerable {
			#region Fields
			private StatusBar owner;
			private ArrayList panels = new ArrayList ();
			private int last_index_by_key;
			#endregion	// Fields

			#region UIA Framework Events
			static object UIACollectionChangedEvent = new object ();

			internal event CollectionChangeEventHandler UIACollectionChanged {
				add { owner.Events.AddHandler (UIACollectionChangedEvent, value); }
				remove { owner.Events.RemoveHandler (UIACollectionChangedEvent, value); }
			}
			
			internal void OnUIACollectionChanged (CollectionChangeEventArgs e)
			{
				CollectionChangeEventHandler eh
					= (CollectionChangeEventHandler) owner.Events [UIACollectionChangedEvent];
				if (eh != null)
					eh (owner, e);
			}
			#endregion

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

				p.SetParent (owner);
				int res = panels.Add (p);

				if (refresh) {
					owner.CalcPanelSizes ();
					owner.Refresh ();
				}

				// UIA Framework Event: Panel Added
				OnUIACollectionChanged (new CollectionChangeEventArgs (CollectionChangeAction.Add, res));

				return res;
			}

			#endregion	// Private & Internal Methods

			#region Public Instance Properties
			[Browsable(false)]
			[EditorBrowsable(EditorBrowsableState.Never)]
			public int Count {
				get { return panels.Count; }
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

					// UIA Framework Event: Panel Removed
					OnUIACollectionChanged (new CollectionChangeEventArgs (CollectionChangeAction.Remove, index));

					value.SetParent (owner);

					panels [index] = value;

					// UIA Framework Event: Panel Added
					OnUIACollectionChanged (new CollectionChangeEventArgs (CollectionChangeAction.Add, index));
				}
			}
			
			public virtual StatusBarPanel this [string key] {
				get {
					int index = IndexOfKey (key);
					if (index >= 0 && index < Count) {
						return (StatusBarPanel) panels [index];
					} 
					return null;
				}
			}

			#endregion	// Public Instance Properties

			#region Public Instance Methods
			public virtual int Add (StatusBarPanel value) {
				return AddInternal (value, true);
			}

			public virtual StatusBarPanel Add (string text) {
				StatusBarPanel res = new StatusBarPanel ();
				res.Text = text;
				Add (res);
				return res;
			}

			public virtual void AddRange (StatusBarPanel [] panels) {
				if (panels == null)
					throw new ArgumentNullException ("panels");
				if (panels.Length == 0)
					return;

				for (int i = 0; i < panels.Length; i++)
					AddInternal (panels [i], false);
				owner.Refresh ();
			}

			public virtual void Clear () {
				panels.Clear ();

				owner.Refresh ();

				// UIA Framework Event: Panel Cleared
				OnUIACollectionChanged (new CollectionChangeEventArgs (CollectionChangeAction.Refresh, -1));
			}

			public bool Contains (StatusBarPanel panel) {
				return panels.Contains (panel);
			}

			public virtual bool ContainsKey (string key)
			{
				int index = IndexOfKey (key);
				return index >= 0 && index < Count;
			}

			public IEnumerator GetEnumerator () {
				return panels.GetEnumerator ();
			}

			public int IndexOf (StatusBarPanel panel) {
				return panels.IndexOf (panel);
			}
			
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

			public virtual void Insert (int index, StatusBarPanel value) {
				if (value == null)
					throw new ArgumentNullException ("value");
				if (index > Count)
					throw new ArgumentOutOfRangeException ("index");
				// TODO: InvalidArgumentException for bad AutoSize values
				// although it seems impossible to set it to a bad value
				value.SetParent (owner);

				panels.Insert(index, value);
				owner.Refresh ();

				// UIA Framework Event: Panel Added
				OnUIACollectionChanged (new CollectionChangeEventArgs (CollectionChangeAction.Add, index));
			}

			public virtual void Remove (StatusBarPanel value) {
				int index = IndexOf (value);
				panels.Remove (value);

				// UIA Framework Event: Panel Removed
				if (index >= 0)
					OnUIACollectionChanged (new CollectionChangeEventArgs (CollectionChangeAction.Remove, index));
			}

			public virtual void RemoveAt (int index) {
				panels.RemoveAt (index);

				// UIA Framework Event: Panel Removed
				OnUIACollectionChanged (new CollectionChangeEventArgs (CollectionChangeAction.Remove, index));
			}

			public virtual void RemoveByKey (string key)
			{
				int index = IndexOfKey (key);
				if (index >= 0 && index < Count)
					RemoveAt (index);
			}

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
				get { return this[index]; }
				set { 
					if (!(value is StatusBarPanel))
						throw new ArgumentException ("Value must be of type StatusBarPanel.", "value");
						
					this[index] = (StatusBarPanel)value;
				}
			}

			int IList.Add (object value) {
				if (!(value is StatusBarPanel))
					throw new ArgumentException ("Value must be of type StatusBarPanel.", "value");
				
				return AddInternal ((StatusBarPanel)value, true);
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
				if (!(value is StatusBarPanel))
					throw new ArgumentException ("Value must be of type StatusBarPanel.", "value");

				Insert (index, (StatusBarPanel)value);
			}

			bool IList.IsFixedSize {
				get { return false; }
			}

			void IList.Remove (object value)
			{
				StatusBarPanel s = value as StatusBarPanel;
				Remove (s);
			}
			#endregion	// IList & ICollection Interfaces
		}
		#endregion	// Subclass StatusBarPanelCollection
	}

}

