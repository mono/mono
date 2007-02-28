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
//	Jordi Mas i Hernandez, jordi@ximian.com
//	Chris Toshok  <toshok@ximian.com>
//
// Based on work by:
//	Daniel Carrera, dcarrera@math.toronto.edu (stubbed out)
//	Jaak Simm (jaaksimm@firm.ee) (stubbed out)
//

using System.ComponentModel;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace System.Windows.Forms
{
	[DefaultEvent("LinkClicked")]
#if NET_2_0
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[ComVisible (true)]
#endif
	public class LinkLabel : Label, IButtonControl
	{
		/* Encapsulates a piece of text (regular or link)*/
		internal class Piece
		{
			public string		text;
			public int		start;
			public int		length;
			public LinkLabel.Link	link;	// Empty link indicates regular text
			public Region           region;

			public Piece (int start, int length, string text, Link link)
			{
				this.start = start;
				this.length = length;
				this.text = text;
				this.link = link;
			}
		}

		private Color active_link_color;
		private Color disabled_link_color;
		private Color link_color;
		private Color visited_color;
		private LinkArea link_area;
		private LinkBehavior link_behavior;
		private LinkCollection link_collection;
		internal Link[] sorted_links;
		private bool link_visited;
		internal Piece[] pieces;
		internal Font link_font;
		private Cursor override_cursor;
		private DialogResult dialog_result;
		internal Rectangle factor;

		private Link active_link;
		private Link hovered_link;
		/* this is an index instead of a Link because we have
		 * to search through sorted links for the new one */
		private int focused_index;

		#region Events
		static object LinkClickedEvent = new object ();

		public event LinkLabelLinkClickedEventHandler LinkClicked {
			add { Events.AddHandler (LinkClickedEvent, value); }
			remove { Events.RemoveHandler (LinkClickedEvent, value); }
		}
		#endregion // Events

		public LinkLabel ()
		{
			LinkArea = new LinkArea (0, -1);
			link_behavior = LinkBehavior.SystemDefault;
			link_visited = false;
			pieces = null;
			link_font = null;			
			focused_index = -1;

			string_format.FormatFlags = StringFormatFlags.NoClip;
			
			ActiveLinkColor = Color.Red;
			DisabledLinkColor = ThemeEngine.Current.ColorGrayText;
			LinkColor = Color.FromArgb (255, 0, 0, 255);
			VisitedLinkColor = Color.FromArgb (255, 128, 0, 128);
			SetStyle (ControlStyles.Selectable, false);
			SetStyle (ControlStyles.ResizeRedraw | 
				ControlStyles.UserPaint | 
				ControlStyles.AllPaintingInWmPaint |
				ControlStyles.SupportsTransparentBackColor | 
				ControlStyles.Opaque 
#if NET_2_0
				| ControlStyles.OptimizedDoubleBuffer
#else
				| ControlStyles.DoubleBuffer
#endif
				, true);

		}

		#region Public Properties

		public Color ActiveLinkColor {
			get { return active_link_color; }
			set {
				if (active_link_color == value)
					return;

				active_link_color = value;
				Invalidate ();
			}
		}

		public Color DisabledLinkColor {

			get { return disabled_link_color; }
			set {
				if (disabled_link_color == value)
					return;

				disabled_link_color = value;
				Invalidate ();
			}
		}

		public Color LinkColor {
			get { return link_color; }
			set {
				if (link_color == value)
					return;

				link_color = value;
				Invalidate ();
			}
		}

		public Color VisitedLinkColor {
			get { return visited_color;}
			set {
				if (visited_color == value)
					return;

				visited_color = value;
				Invalidate ();
			}
		}

		[Localizable (true)]
		[Editor ("System.Windows.Forms.Design.LinkAreaEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]				
#if NET_2_0
		[RefreshProperties (RefreshProperties.Repaint)]
#endif
		public LinkArea LinkArea {
			get { return link_area;}
			set {

				if (value.Start <0 || value.Length < -1)
					throw new ArgumentException ();

				if (Links.IsDefault)
					Links.Clear ();

				if (!value.IsEmpty) {
					Links.Add (value.Start, value.Length);

					link_area = value;
					Invalidate ();
				}
			}
		}
				
		[DefaultValue (LinkBehavior.SystemDefault)]
		public LinkBehavior LinkBehavior {

			get { return link_behavior;}
			set {
				if (link_behavior == value)
					return;

				link_behavior = value;
				Invalidate ();
			}
		}
	
		[Browsable (false)]
#if !NET_2_0
		[EditorBrowsable (EditorBrowsableState.Advanced)]
#endif
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public LinkLabel.LinkCollection Links {
			get {
                                if (link_collection == null)
                                        link_collection = new LinkCollection (this);
                                        
                                return link_collection;
                        }
		}

		[DefaultValue (false)]
		public bool LinkVisited {
			get { return link_visited;}
			set {
				if (link_visited == value)
					return;

				link_visited = value;
				Invalidate ();
			}
		}
		
		protected Cursor OverrideCursor {
			get {
				if (override_cursor == null)
					override_cursor = Cursors.Hand;
				return override_cursor;
			}
			set { override_cursor = value; }
		}

		[RefreshProperties(RefreshProperties.Repaint)]
		public override string Text {
			get { return base.Text;	}
			set {
				if (base.Text == value)
					return;

				base.Text = value;
				CreateLinkPieces ();				
			}
		}

		#endregion // Public Properties

		DialogResult IButtonControl.DialogResult {
			get { return dialog_result; }
			set { dialog_result = value; }
		}


		void IButtonControl.NotifyDefault (bool value)
		{
		}

		void IButtonControl.PerformClick ()
		{
		}

		#region Public Methods
		protected override AccessibleObject CreateAccessibilityInstance ()
		{
			return base.CreateAccessibilityInstance();
		}

		protected override void CreateHandle ()
		{
			base.CreateHandle ();
			CreateLinkFont ();
			CreateLinkPieces ();			
		}

		protected override void OnEnabledChanged (EventArgs e)
		{
			base.OnEnabledChanged (e);
			Invalidate ();
		}

		protected override void OnFontChanged (EventArgs e)
		{
			base.OnFontChanged (e);
			CreateLinkFont ();
			CreateLinkPieces ();
		}

		protected override void OnGotFocus (EventArgs e)
		{
			base.OnGotFocus (e);			

			// Set focus to the first enabled link piece
			if (focused_index == -1) {
				if ((Control.ModifierKeys & Keys.Shift) == 0) {
					for (int i = 0; i < sorted_links.Length; i ++) {
						if (sorted_links[i].Enabled) {
							focused_index = i;
							break;
						}
					}
				} else {
					if (focused_index == -1)
						focused_index = sorted_links.Length;

					for (int n = focused_index - 1; n >= 0; n--) {
						if (sorted_links[n].Enabled) {
							sorted_links[n].Focused = true;
							focused_index = n;
							return;
						}
					}
				}
			}

			if (focused_index != -1)
				sorted_links[focused_index].Focused = true;
		}

		protected override void OnKeyDown (KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Return) {
				if (focused_index != -1)
					OnLinkClicked (new LinkLabelLinkClickedEventArgs (sorted_links[focused_index]));
			}

			base.OnKeyDown(e);
		}

		protected virtual void OnLinkClicked (LinkLabelLinkClickedEventArgs e)
		{
			LinkLabelLinkClickedEventHandler eh = (LinkLabelLinkClickedEventHandler)(Events [LinkClickedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override void OnLostFocus (EventArgs e)
		{
			base.OnLostFocus (e);			
			
			// Clean focus in link pieces
			if (focused_index != -1)
				sorted_links[focused_index].Focused = false;
		}

		protected override void OnMouseDown (MouseEventArgs e)
		{
			if (!Enabled) return;

			base.OnMouseDown (e);

			for (int i = 0; i < sorted_links.Length; i ++) {
				if (sorted_links[i].Contains (e.X, e.Y) && sorted_links[i].Enabled) {
					sorted_links[i].Active = true;
					if (focused_index != -1)
						sorted_links[focused_index].Focused = false;
					active_link = sorted_links[i];
					focused_index = i;
					sorted_links[focused_index].Focused = true;
					break;
				}
			}
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			if (!Enabled) return;
			base.OnMouseLeave (e);
			UpdateHover (null);
		}

		private void UpdateHover (Link link)
		{
			if (link == hovered_link)
				return;

			if (hovered_link != null)
				hovered_link.Hovered = false;

			hovered_link = link;

			if (hovered_link != null)
				hovered_link.Hovered = true;

			Cursor = (hovered_link != null) ? OverrideCursor : Cursors.Default;

			/* XXX this shouldn't be here.  the
			 * Link.Invalidate machinery should be enough,
			 * but it seems the piece regions don't
			 * contain the area with the underline.  this
			 * can be seen easily when you click on a link
			 * and the focus rectangle shows up (it's too
			 * far up), and also the bottom few pixels of
			 * a linklabel aren't active when it comes to
			 * hovering */
			Invalidate ();
		}

		protected override void OnMouseMove (MouseEventArgs e)
		{
			UpdateHover (PointInLink (e.X, e.Y));
			base.OnMouseMove (e);
		}

		protected override void OnMouseUp (MouseEventArgs e)
		{
			if (!Enabled) return;

			base.OnMouseUp (e);

			if (active_link == null)
				return;

			Link clicked_link = (PointInLink (e.X, e.Y) == active_link) ? active_link : null;

			active_link.Active = false;
			active_link = null;

			if (clicked_link != null)
				OnLinkClicked (new LinkLabelLinkClickedEventArgs (clicked_link));
		}

		protected override void OnPaint (PaintEventArgs pevent)
		{
			ThemeEngine.Current.DrawLinkLabel (pevent.Graphics, pevent.ClipRectangle, this);
			DrawImage (pevent.Graphics, Image, ClientRectangle, image_align);
			// Do not call base.OnPaint since it's the Label class 
		}

		protected override void OnPaintBackground (PaintEventArgs e)
		{
			base.OnPaintBackground (e);
		}

		protected override void OnTextAlignChanged (EventArgs e)
		{
			CreateLinkPieces ();			
			base.OnTextAlignChanged (e);
		}

		protected override void OnTextChanged (EventArgs e)
		{
			CreateLinkPieces ();
			base.OnTextChanged (e);
		}
		
		protected Link PointInLink (int x, int y)
		{
			for (int i = 0; i < sorted_links.Length; i ++)
				if (sorted_links[i].Contains (x, y))
					return sorted_links[i];

			return null;
		}

		protected override bool ProcessDialogKey (Keys keyData)
		{
			if ((keyData & Keys.KeyCode) ==  Keys.Tab) {
				Select (true, (keyData & Keys.Shift) == 0);
				return true;
			}
			return base.ProcessDialogKey (keyData);
		}

		protected override void Select (bool directed, bool forward)
		{
			if (directed) {
				if (focused_index != -1)
					sorted_links[focused_index].Focused = false;

				if (forward) {
					for (int n = focused_index + 1; n < sorted_links.Length; n++) {
						if (sorted_links[n].Enabled) {
							sorted_links[n].Focused = true;
							focused_index = n;
							base.Select (directed, forward);
							return;
						}
					}
				}
				else {
					if (focused_index == -1)
						focused_index = sorted_links.Length;

					for (int n = focused_index - 1; n >= 0; n--) {
						if (sorted_links[n].Enabled) {
							sorted_links[n].Focused = true;
							focused_index = n;
							base.Select (directed, forward);
							return;
						}
					}
				}

				focused_index = -1;

				if (Parent != null)
					Parent.SelectNextControl (this, forward, false, true, true);
			}
		}

		protected override void SetBoundsCore (int x, int y, int width, int height, BoundsSpecified specified)
		{
			base.SetBoundsCore (x, y, width, height, specified);
			CreateLinkFont ();
			CreateLinkPieces();
		}

		protected override void WndProc (ref Message m)
		{
			base.WndProc (ref m);
		}

		#endregion //Public Methods

		#region Private Methods

		private ArrayList CreatePiecesFromText (int start, int len, Link link)
		{
			ArrayList rv = new ArrayList ();

			if (start + len > Text.Length)
				len = Text.Length - start;
			if (len < 0)
				return rv;

			string t = Text.Substring (start, len);

			int ps = 0;
			for (int i = 0; i < t.Length; i ++) {
				if (t[i] == '\n') {
					if (i != 0) {
						Piece p = new Piece (start + ps, i + 1 - ps, t.Substring (ps, i+1-ps), link);
						rv.Add (p);
					}
					ps = i+1;
				}
			}
			if (ps < t.Length) {
				Piece p = new Piece (start + ps, t.Length - ps, t.Substring (ps, t.Length-ps), link);
				rv.Add (p);
			}

			return rv;
		}

		private void CalcMeasurementFactor ()
		{
			const string text = "X";
			
			// Measure total area including padding area.
			SizeF size = DeviceContext.MeasureString (text, link_font);
			Rectangle rect = new Rectangle (0, 0, (int) size.Width, (int) size.Height);
			
			// Measure only font area without padding area.
			CharacterRange[] ranges = { new CharacterRange(0, 1) };
			string_format.SetMeasurableCharacterRanges (ranges);
			Region[] regions = DeviceContext.MeasureCharacterRanges (text, link_font, rect, string_format);
			
			// Calculate diference.
			RectangleF rectf = regions [0].GetBounds (DeviceContext);
			
			factor = new Rectangle ((int) rectf.X, (int) rectf.Y, 
							rect.Width - (int) rectf.Width - ((int) rectf.X * 2), 
							rect.Height - (int) rectf.Height - ((int) rectf.Y * 2));
		}

		private void CreateLinkPieces ()
		{
			if (Text.Length == 0) {
				SetStyle (ControlStyles.Selectable, false);
				return;
			}

			SetStyle (ControlStyles.Selectable, Links.Count > 0);

			/* don't bother doing the rest if our handle hasn't been created */
			if (!IsHandleCreated)
				return;

			if (Links.Count == 1 && Links[0].Start == 0 &&	Links[0].Length == -1)
				Links[0].Length = Text.Length;

			SortLinks ();

			ArrayList pieces_list = new ArrayList ();

			int current_end = 0;

			for (int l = 0; l < sorted_links.Length; l ++) {
				int new_link_start = sorted_links[l].Start;

				if (new_link_start > current_end) {
					/* create/push a piece
					 * containing the text between
					 * the previous/new link */
					ArrayList text_pieces = CreatePiecesFromText (current_end, new_link_start - current_end, null);
					pieces_list.AddRange (text_pieces);
				}

				/* now create a group of pieces for
				 * the new link (split up by \n's) */
				ArrayList link_pieces = CreatePiecesFromText (new_link_start, sorted_links[l].Length, sorted_links[l]);
				pieces_list.AddRange (link_pieces);
				sorted_links[l].pieces.AddRange (link_pieces);

				current_end = sorted_links[l].Start + sorted_links[l].Length;
			}
			if (current_end < Text.Length) {
				ArrayList text_pieces = CreatePiecesFromText (current_end, Text.Length - current_end, null);
				pieces_list.AddRange (text_pieces);
			}

			pieces = new Piece[pieces_list.Count];
			pieces_list.CopyTo (pieces, 0);

			CharacterRange[] ranges = new CharacterRange[pieces.Length];

			for(int i = 0; i < pieces.Length; i++)
				ranges[i] = new CharacterRange (pieces[i].start, pieces[i].length);

			string_format.SetMeasurableCharacterRanges (ranges);

			Region[] regions = DeviceContext.MeasureCharacterRanges (Text,
										 link_font,
										 ClientRectangle,
										 string_format);

			for (int i = 0; i < pieces.Length; i ++)
				pieces[i].region = regions[i];

			CalcMeasurementFactor ();
			Invalidate ();
		}

		private void SortLinks ()
		{
			if (sorted_links != null)
				return;

			sorted_links = new Link [Links.Count];
			((ICollection)Links).CopyTo (sorted_links, 0);

			Array.Sort (sorted_links, new LinkComparer ());
		}

		/* Check if the links overlap */
		private void CheckLinks ()
		{
			SortLinks ();

			int current_end = 0;

			for (int i = 0; i < sorted_links.Length; i++) {
				if (sorted_links[i].Start < current_end)
					throw new InvalidOperationException ("Overlapping link regions.");
				current_end = sorted_links[i].Start + sorted_links[i].Length;
			}
		}
		
		internal Font GetPieceFont (Piece piece)
		{
			if (piece.link == null)
				return Font;

			switch (link_behavior) {				
				case LinkBehavior.AlwaysUnderline:
				case LinkBehavior.SystemDefault: // Depends on IE configuration
				{
					return link_font;
				}				
				case LinkBehavior.HoverUnderline:
				{
					if (piece.link.Hovered) {
						return link_font;
					} else {
						return Font;
					}								
				}
				
				case LinkBehavior.NeverUnderline:				
				default:
					return Font;					
			}
			
		}		
		

		internal Color GetPieceColor (Piece piece, int i)
		{
			Color color;

			if (Enabled == false)
				return DisabledLinkColor;

			if (piece.link == null)
				return ForeColor;

			if (!piece.link.Enabled)
				color = DisabledLinkColor;
			else if (piece.link.Active)
				color = ActiveLinkColor;
			else if ((LinkVisited && i == 0) || piece.link.Visited == true)
				color = VisitedLinkColor;
			else
				color = LinkColor;

			return color;
		}

		private void CreateLinkFont ()
		{
			if (link_font != null)
				link_font.Dispose ();
				
			link_font  = new Font (Font.FontFamily, Font.Size, Font.Style | FontStyle.Underline,
					       Font.Unit);
		}

		#endregion // Private Methods

		//
		// System.Windows.Forms.LinkLabel.Link
		//
#if NET_2_0
		// XXX [TypeConverter (typeof (LinkConverter))]
#endif
		public class Link
		{
			private bool enabled;
			internal int length;
			private object linkData;
			private int start;
			private bool visited;			
			private LinkLabel owner;
			private bool hovered;
			internal ArrayList pieces;
			private bool focused;
			private bool active;

			internal Link (LinkLabel owner)
			{
				focused = false;
				enabled = true;
				visited = false;
				length = start = 0;
				linkData = null;
				this.owner = owner;
				pieces = new ArrayList ();
			}

#if NET_2_0
			[DefaultValue (true)]
#endif
			public bool Enabled {
				get { return enabled; }
				set {
					if (enabled != value)
						Invalidate ();

					enabled = value;
				}
			}

			public int Length {
				get { 
					if (length == -1) {
						return owner.Text.Length;
					}
					
					return length; 
				}
				set {
					if (length == value)
						return;
					
					length = value;

					owner.CreateLinkPieces ();
				}
			}

#if NET_2_0
			[DefaultValue (null)]
#endif
			public object LinkData {
				get { return linkData; }
				set { linkData = value; }
			}

			public int Start {
				get { return start; }
				set {
					if (start == value)
						return;

					start = value;

					owner.sorted_links = null;
					owner.CreateLinkPieces ();
				}
			}

#if NET_2_0
			[DefaultValue (false)]
#endif
			public bool Visited {
				get { return visited; }
				set {
					if (visited != value)
						Invalidate ();

					visited = value;
				}
			}
			
			internal bool Hovered {
				get { return hovered; }
				set {
					if (hovered != value)
						Invalidate ();
					hovered = value;
				}
			}

			internal bool Focused {
				get { return focused; }
				set {
					if (focused != value)
						Invalidate ();
					focused = value;
				}
			}

			internal bool Active {
				get { return active; }
				set {
					if (active != value)
						Invalidate ();
					active = value;
				}
			}

			private void Invalidate ()
			{
				for (int i = 0; i < pieces.Count; i ++)
					owner.Invalidate (((Piece)pieces[i]).region);
			}

			internal bool Contains (int x, int y)
			{
				foreach (Piece p in pieces) {
					if (p.region.IsVisible (new Point (x,y)))
						return true;
				}
				return false;
			}
		}

		class LinkComparer : IComparer
		{
			public int Compare (object x, object y)
			{
				Link l1 = (Link)x;
				Link l2 = (Link)y;

				return l1.Start - l2.Start;
			}
		}

		//
		// System.Windows.Forms.LinkLabel.Link
		//
		public class LinkCollection :  IList, ICollection, IEnumerable
		{
			private LinkLabel owner;
			private ArrayList collection = new ArrayList();

			public LinkCollection (LinkLabel owner)
			{
				if (owner==null)
					throw new ArgumentNullException ();

				this.owner = owner;
			}

			[Browsable (false)]
			public int Count {
				get { return collection.Count; }
			}

			public bool IsReadOnly {
				get { return false; }
			}

			public virtual LinkLabel.Link this[int index]  {
				get {
					if (index < 0 || index >= Count)
						throw  new  ArgumentOutOfRangeException();

					return (LinkLabel.Link) collection[index];
				}
				set {
					if (index < 0 || index >= Count)
						throw new  ArgumentOutOfRangeException();

					collection[index] = value;
				}
			}

			public Link Add (int start, int length)
			{
				return Add (start, length, null);
			}
			
			internal bool IsDefault {
				get {
					return (Count == 1
						&& this[0].Start == 0
						&& this[0].length == -1);
				}
			}

			public Link Add (int start, int length, object o)
			{
				Link link = new Link (owner);
				int idx;

				/* remove the default 0,-1 link */
				if (IsDefault) {
					/* don't call Clear() here to save the additional CreateLinkPieces */
					collection.Clear ();
				}

				link.Length = length;
				link.Start = start;
				link.LinkData = o;
				idx = collection.Add (link);

				owner.sorted_links = null;
				owner.CheckLinks ();
				owner.CreateLinkPieces ();
				return (Link) collection[idx];
			}

			public virtual void Clear ()
			{
				collection.Clear();
				owner.sorted_links = null;
				owner.CreateLinkPieces ();
			}

			public bool Contains (Link link)
			{
				return collection.Contains (link);
			}

			public IEnumerator GetEnumerator ()
			{
				return collection.GetEnumerator ();
			}

			public int IndexOf (Link link)
			{
				return collection.IndexOf (link);
			}

			public void Remove (LinkLabel.Link value)
			{
				collection.Remove (value);
				owner.sorted_links = null;
				owner.CreateLinkPieces ();
			}

			public void RemoveAt (int index)
			{
				if (index >= Count)
					throw new ArgumentOutOfRangeException ("Invalid value for array index");

				collection.Remove (collection[index]);
				owner.sorted_links = null;
				owner.CreateLinkPieces ();
			}

			bool IList.IsFixedSize {
				get {return false;}
			}

			object IList.this[int index] {
				get { return collection[index]; }
				set { collection[index] = value; }
			}

			object ICollection.SyncRoot {
				get {return this;}
			}

			bool ICollection.IsSynchronized {
				get {return false;}
			}

			void ICollection.CopyTo (Array dest, int index)
			{
				collection.CopyTo (dest, index);
			}

			int IList.Add (object control)
			{
				int idx = collection.Add (control);
				owner.sorted_links = null;
				owner.CheckLinks ();
				owner.CreateLinkPieces ();
				return idx;
			}

			bool IList.Contains (object control)
			{
				return Contains ((Link)control);
			}

			int IList.IndexOf (object control)
			{
				return collection.IndexOf (control);
			}

			void IList.Insert (int index, object value)
			{
				collection.Insert (index, value);
				owner.sorted_links = null;
				owner.CheckLinks ();
				owner.CreateLinkPieces ();
			}

			void IList.Remove (object control)
			{
				Remove ((Link)control);
			}
		}
#if NET_2_0

		[RefreshProperties (RefreshProperties.Repaint)]
		public new bool UseCompatibleTextRendering {
			get {
				return use_compatible_text_rendering;
			}

			set {
				use_compatible_text_rendering = value;
			}
		}
#endif
	}
}
