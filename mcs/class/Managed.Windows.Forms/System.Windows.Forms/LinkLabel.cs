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
//
// Based on work by:
//	Daniel Carrera, dcarrera@math.toronto.edu (stubbed out)
//	Jaak Simm (jaaksimm@firm.ee) (stubbed out)
//

// COMPLETE


using System.ComponentModel;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace System.Windows.Forms
{
	[DefaultEvent("LinkClicked")]
	public class LinkLabel : Label, IButtonControl
	{
		/* Encapsulates a piece of text (regular or link)*/
		internal class Piece
		{
			public string		text;
			public int		start;
			public int		end;
			public LinkLabel.Link	link;	// Empty link indicates regular text
			public Rectangle	rect;
			public bool		clicked;
			public bool		focused;

			public Piece ()
			{
				start = end = 0;
				link = null;
				clicked = false;
				focused = false;				
			}
		}

		private Color active_link;
		private Color disabled_link;
		private Color link_color;
		private Color visited_color;
		private LinkArea link_area;
		private LinkBehavior link_behavior;
		private LinkCollection link_collection;
		private bool link_visited;		
		private bool link_click;
		internal Piece[] pieces;
		internal int num_pieces;
		internal Font link_font;		
		private Cursor override_cursor;
		private DialogResult dialog_result;

		#region Events
		public event LinkLabelLinkClickedEventHandler LinkClicked;
		#endregion // Events

		public LinkLabel ()
		{
			LinkArea = new LinkArea (0, -1);
			link_behavior = LinkBehavior.SystemDefault;
			link_visited = false;
			link_click = false;
			pieces = null;
			num_pieces = 0;
			link_font = null;			

			ActiveLinkColor = Color.Red;
			DisabledLinkColor = ThemeEngine.Current.ColorGrayText;
			LinkColor = Color.FromArgb (255, 0, 0, 255);
			VisitedLinkColor = Color.FromArgb (255, 128, 0, 128);
			SetStyle (ControlStyles.Selectable, true);			
		}

		#region Public Properties

		public Color ActiveLinkColor {
			get { return active_link;}
			set {
				if (active_link == value)
					return;

				active_link = value;
				Refresh ();
			}
		}

		public Color DisabledLinkColor {

			get { return disabled_link;}
			set {
				if (disabled_link == value)
					return;

				disabled_link = value;
				Refresh ();
			}
		}

		public Color LinkColor {
			get { return link_color;}
			set {
				if (link_color == value)
					return;

				link_color = value;
				Refresh ();
			}
		}

		public Color VisitedLinkColor {
			get { return visited_color;}
			set {
				if (visited_color == value)
					return;

				visited_color = value;
				Refresh ();
			}
		}

		[Localizable (true)]
		[Editor ("System.Windows.Forms.Design.LinkAreaEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]				
		public LinkArea LinkArea {
			get { return link_area;}
			set {

				if (value.Start <0 || value.Length < -1)
					throw new ArgumentException ();

				if (!value.IsEmpty)
					Links.Add (value.Start, value.Length);

				link_area = value;
				Refresh ();
			}
		}
				
		[DefaultValue (LinkBehavior.SystemDefault)]
		public LinkBehavior LinkBehavior {

			get { return link_behavior;}
			set {
				if (link_behavior == value)
					return;

				link_behavior = value;
				Refresh ();
			}
		}
	
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
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
				Refresh ();
			}
		}
		
		protected Cursor OverrideCursor {
			get { return override_cursor;}
			set { override_cursor = value;}
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
			Refresh ();
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
			for (int i = 0; i < num_pieces; i++) {
				if (pieces[i].link != null && pieces[i].link.Enabled) {
					 pieces[i].focused = true;
					 Invalidate (pieces[i].rect);
					 break;
				}
			}			
		}

		protected override void OnKeyDown (KeyEventArgs e)
		{	
			base.OnKeyDown(e);		
			
			// Set focus to the next link piece
			if (e.KeyCode == Keys.Tab || e.KeyCode == Keys.Right) {
				for (int i = 0; i < num_pieces; i++) {
					if (pieces[i].focused) {
						pieces[i].focused = false;
						Invalidate (pieces[i].rect);
						
						for (int n = i + 1; n < num_pieces; n++) {
							if (pieces[n].link != null && pieces[n].link.Enabled) {							
								pieces[n].focused = true;
								e.Handled = true;
								Invalidate (pieces[n].rect);
								return;
							}		
						}
					}
				}
			} else if (e.KeyCode == Keys.Return) {											
				for (int i = 0; i < num_pieces; i++) {
					if (pieces[i].focused && pieces[i].link != null) {
						OnLinkClicked (new LinkLabelLinkClickedEventArgs (pieces[i].link));
						break;
					}
				}
			}
		}

		protected virtual void OnLinkClicked (LinkLabelLinkClickedEventArgs e)
		{
			if (LinkClicked != null)
				LinkClicked (this, e);
		}

		protected override void OnLostFocus (EventArgs e)
		{
			base.OnLostFocus (e);			
			
			// Clean focus in link pieces
			for (int i = 0; i < num_pieces; i++) {
				if (pieces[i].focused) {
					pieces[i].focused = false;					
				}
			}
			
			Refresh ();
		}

		protected override void OnMouseDown (MouseEventArgs e)
		{
			if (!Enabled) return;

			base.OnMouseDown (e);
			this.Capture = true;

			for (int i = 0; i < num_pieces; i++) {
				if (pieces[i].rect.Contains (e.X, e.Y)) {
					if (pieces[i].link!= null) {
						pieces[i].clicked = true;
						Invalidate (pieces[i].rect);
					}
					break;
				}
			}
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			if (!Enabled) return;

			base.OnMouseLeave (e);
		}

		protected override void OnMouseMove (MouseEventArgs e)
		{
			base.OnMouseMove (e);
			
			Link link = PointInLink (e.X, e.Y);
			
			if (link == null) {
				Cursor = Cursors.Default;
				bool changed = false;
				if (link_behavior == LinkBehavior.HoverUnderline) {
					for (int i = 0; i < Links.Count; i++) {
						if (Links[i].Hoovered == true) 	{
							changed = true;
							Links[i].Hoovered = false;
						}
					}

					if (changed == true)
						Refresh ();
				}
				return;
			}
			
			if (link_behavior == LinkBehavior.HoverUnderline) {
				if (link.Hoovered != true) {
					link.Hoovered = true;
					Refresh ();
				}
			}
			
			Cursor = Cursors.Hand;
		}

		protected override void OnMouseUp (MouseEventArgs e)
		{
			if (!Enabled) return;

			base.OnMouseUp (e);
			this.Capture = false;

			for (int i = 0; i < num_pieces; i++) {
				if (pieces[i].link!= null && pieces[i].clicked == true) {
					OnLinkClicked (new LinkLabelLinkClickedEventArgs (pieces[i].link));					
					pieces[i].clicked = false;
					Invalidate (pieces[i].rect);
					break;
				}
			}
		}

		protected override void OnPaint (PaintEventArgs pevent)
		{
			ThemeEngine.Current.DrawLinkLabel (pevent.Graphics, pevent.ClipRectangle, this);
			DrawImage (pevent.Graphics, Image, ClientRectangle, image_align);
			base.OnPaint(pevent);
		}

		protected override void OnPaintBackground (PaintEventArgs e)
		{
			base.OnPaintBackground (e);
		}

		protected override void OnTextAlignChanged (EventArgs e)
		{
			base.OnTextAlignChanged (e);
			CreateLinkPieces ();			
		}

		protected override void OnTextChanged (EventArgs e)
		{
			base.OnTextChanged (e);			
		}
		
		protected Link PointInLink (int x, int y)
		{
			for (int i = 0; i < num_pieces; i++) {
				if (pieces[i].rect.Contains (x,y) && pieces[i].link != null)
					return pieces[i].link;
			}

			return null;
		}

		protected override bool ProcessDialogKey (Keys keyData)
		{
			return base.ProcessDialogKey (keyData);
		}

		protected override void Select (bool directed, bool forward)
		{
			base.Select (directed, forward);
		}

		protected override void SetBoundsCore (int x, int y, int width, int height, BoundsSpecified specified)
		{
			base.SetBoundsCore (x, y, width, height, specified);			
		}

		protected override void WndProc (ref Message m)
    		{
			base.WndProc (ref m);
    		}

		#endregion //Public Methods

		#region Private Methods

		internal void CreateLinkPieces ()
		{
			if (Links.Count == 0 || IsHandleCreated == false || Text.Length == 0)
				return;

			int cur_piece = 0;
			num_pieces = 0;

			if (Links.Count == 1 && Links[0].Start == 0 &&	Links[0].Length == -1) {
				Links[0].Length = Text.Length;				
			}

			pieces = new Piece [(Links.Count * 2) + 1];
			pieces[cur_piece] = new Piece();
			pieces[cur_piece].start = 0;

			for (int i = 0; i < Text.Length; i++) { /* Every char on the text*/
				for (int l = 0; l < Links.Count; l++)	{ /* Every link that we know of*/
					if (Links[l].Start == i) {
						if (i > 0) {							
							/*Push prev. regular text*/
							pieces[cur_piece].end = i;
							pieces[cur_piece].text = Text.Substring (pieces[cur_piece].start,
								pieces[cur_piece].end - pieces[cur_piece].start);

							cur_piece++;

							/* New link*/
							pieces[cur_piece] = new Piece ();							
						}
						
						int end;
						
						if (Links[l].Start + Links[l].Length > Text.Length) {
							end = Text.Length - Links[l].Start;
						}
						else {
							end = Links[l].Length;
						}						
						
						pieces[cur_piece].start = Links[l].Start;
						pieces[cur_piece].end = Links[l].Start + end;
						pieces[cur_piece].link = Links[l];
						
						pieces[cur_piece].text = Text.Substring (pieces[cur_piece].start, end);

						cur_piece++; /* Push link*/
						pieces[cur_piece] = new Piece();
						i+= Links[l].Length;
						pieces[cur_piece].start = i;
					}
				}
			}			

			if (pieces[cur_piece].end == 0 && pieces[cur_piece].start < Text.Length) {
				pieces[cur_piece].end = Text.Length;
				pieces[cur_piece].text = Text.Substring (pieces[cur_piece].start, pieces[cur_piece].end - pieces[cur_piece].start);
				cur_piece++;
			}
			
			num_pieces = cur_piece;

			CharacterRange[] charRanges = new CharacterRange [num_pieces];

			for (int i = 0; i < num_pieces; i++)
				charRanges[i] = new CharacterRange (pieces[i].start, pieces[i].end - pieces[i].start);

			Region[] charRegions = new Region [num_pieces];			
			string_format.SetMeasurableCharacterRanges (charRanges);

			// BUG: This sizes do not match the ones used later when drawing
			charRegions = DeviceContext.MeasureCharacterRanges (Text, link_font, ClientRectangle, string_format);
	
			RectangleF rect;
			for (int i = 0; i < num_pieces; i++)  {				
				rect = charRegions[i].GetBounds (DeviceContext);
				pieces[i].rect = Rectangle.Ceiling (rect);
				charRegions[i].Dispose ();
			}

			if (Visible && IsHandleCreated)
				Refresh ();

		}

		/* Check if the links overlap */
		internal void CheckLinks ()
		{
			for (int i = 0; i < Links.Count; i++) {
				for (int l = 0; l < Links.Count; l++) {
					if (i==l) continue;

					if (((Links[i].Start + Links[i].Length) >= Links[l].Start &&
						Links[i].Start + Links[i].Length <= Links[l].Start + Links[l].Length) ||
						(Links[i].Start  >= Links[l].Start &&
						Links[i].Start  <= Links[l].Start + Links[l].Length))
						throw new InvalidOperationException ("Overlapping link regions.");
				}
			}
		}
		
		internal Font GetPieceFont (Piece piece)
		{
			switch (link_behavior) {				
				case LinkBehavior.AlwaysUnderline:
				case LinkBehavior.SystemDefault: // Depends on IE configuration
				{
					if (piece.link == null) {
						return Font;
					} else {
						return link_font;
					}
					break;
				}				
				case LinkBehavior.HoverUnderline:
				{
					if (piece.link != null && piece.link.Hoovered) {
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
		

		internal Color GetLinkColor (Piece piece, int i)
		{
			Color color;

			if (Enabled == false ||
				(piece.link != null && piece.link.Enabled == false))
				color = DisabledLinkColor;
			else
				if (piece.clicked == true)
					color = ActiveLinkColor;
				else
					if ((LinkVisited == true && i == 0) ||
						(piece.link != null && piece.link.Visited == true))
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
		public class Link
		{
			private bool enabled;
			internal int length;
			private object linkData;
			private int start;
			private bool visited;			
			private LinkLabel owner;
			private bool hoovered;

			internal Link ()
			{
				enabled = true;
				visited = false;
				length = start = 0;
				linkData = null;
				owner = null;				
			}

			internal Link (LinkLabel owner)
			{
				enabled = true;
				visited = false;
				length = start = 0;
				linkData = null;
				this.owner = owner;
			}

			public bool Enabled {
				get { return enabled; }
				set {
					if (enabled == value)
						return;

					enabled = value;	
					
					if (owner != null)
						owner.Refresh ();
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

					if (owner != null)
						owner.CreateLinkPieces ();
				}
			}

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

					if (owner != null)
						owner.CreateLinkPieces ();
				}
			}

			public bool Visited {
				get { return visited; }
				set {
					if (visited == value)
						return;

					visited = value;
					
					if (owner != null)
						owner.Refresh ();
				}
			}
			
			internal bool Hoovered {
				get { return hoovered; }
				set { hoovered = value; }
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


			public Link Add (int start, int length, object o)
			{
				Link link = new Link (owner);
				int idx;

				if (Count == 1 && this[0].Start == 0
					&& this[0].length == -1) {
					Clear ();
				}

				link.Length = length;
				link.Start = start;
				link.LinkData = o;
				idx = collection.Add (link);

				owner.CheckLinks ();
				owner.CreateLinkPieces ();
				return (Link) collection[idx];
			}

			public virtual void Clear ()
			{
				collection.Clear();
				owner.CreateLinkPieces ();
			}

			public bool Contains (LinkLabel.Link link)
			{
				return collection.Contains (link);
			}

			public IEnumerator GetEnumerator ()
			{
				return collection.GetEnumerator ();
			}

			public int IndexOf (LinkLabel.Link link)
			{
				return collection.IndexOf (link);
			}

			public void Remove (LinkLabel.Link value)
			{
				collection.Remove (value);
				owner.CreateLinkPieces ();
			}

			public void RemoveAt (int index)
			{
				if (index >= Count)
					throw new ArgumentOutOfRangeException ("Invalid value for array index");

				collection.Remove (collection[index]);
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
				owner.CheckLinks ();
				owner.CreateLinkPieces ();
				return idx;
			}

			bool IList.Contains (object control)
			{
				return collection.Contains (control);
			}

			int IList.IndexOf (object control)
			{
				return collection.IndexOf (control);
			}

			void IList.Insert (int index, object value)
			{
				collection.Insert (index, value);
				owner.CheckLinks ();
				owner.CreateLinkPieces ();
			}

			void IList.Remove (object control)
			{
				collection.Remove (control);
				owner.CreateLinkPieces ();
			}
		}
	}
}
