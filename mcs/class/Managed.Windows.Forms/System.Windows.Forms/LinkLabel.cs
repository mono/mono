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
//	Jordi Mas i Hernandez, jordi@ximian.com
//
// Based on work by:
//	Daniel Carrera, dcarrera@math.toronto.edu (stubbed out)
//	Jaak Simm (jaaksimm@firm.ee) (stubbed out)
//
// TODO:
//	- Change the cursor to a hand cursor when you are over a link (when cursors are available)
//	- Focus handeling
//
// $Revision: 1.8 $
// $Modtime: $
// $Log: LinkLabel.cs,v $
// Revision 1.8  2004/09/07 09:40:15  jordi
// LinkLabel fixes, methods, multiple links
//
// Revision 1.7  2004/08/21 22:32:14  pbartok
// - Signature Fixes
//
// Revision 1.6  2004/08/10 15:24:35  jackson
// Let Control handle buffering.
//
// Revision 1.5  2004/08/08 17:52:12  jordi
// *** empty log message ***
//
// Revision 1.4  2004/08/07 23:31:15  jordi
// fixes label bug and draw method name
//
// Revision 1.3  2004/08/07 19:16:31  jordi
// throw exceptions, fixes events, missing methods
//
// Revision 1.2  2004/07/22 15:22:19  jordi
// link label: check link overlapping, implement events, and fixes
//
// Revision 1.1  2004/07/21 16:19:17  jordi
// LinkLabel control implementation
//
//
// INCOMPLETE


using System.ComponentModel;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace System.Windows.Forms
{
	public class LinkLabel : Label, IButtonControl
	{
		/* Encapsulates a piece of text (regular or link)*/
		internal class Piece
		{
			public string		text;
			public int		start;
			public int		end;
			public LinkLabel.Link	link;	// Empty link indicates regular text
			public RectangleF	rect;
			public bool		clicked;

			public Piece ()
			{
				start = end = 0;
				link = null;
				clicked = false;
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
		private Font link_font;
		private bool link_click;
		private Piece[] pieces;
		private Cursor override_cursor;
		private DialogResult dialog_result;

		#region Events
		public event LinkLabelLinkClickedEventHandler LinkClicked;
		#endregion // Events

		public LinkLabel ()
		{
			link_collection = new LinkCollection (this);
			LinkArea = new LinkArea (0, -1);
			link_behavior = LinkBehavior.SystemDefault;
			link_visited = false;
			link_click = false;
			pieces = null;

			ActiveLinkColor = Color.Red;
			DisabledLinkColor = ThemeEngine.Current.ColorGrayText;
			LinkColor = Color.FromArgb (255, 0, 0, 255);
			VisitedLinkColor = Color.FromArgb (255, 128, 0, 128);
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

		public LinkArea LinkArea {
			get { return link_area;}
			set {

				if (value.Start <0 || value.Length > 0)
					throw new ArgumentException ();

				if (!value.IsEmpty)
					Links.Add (value.Start, value.Length);

				link_area = value;
				Refresh ();
			}
		}

		public LinkBehavior LinkBehavior {

			get { return link_behavior;}
			set {
				if (link_behavior == value)
					return;

				link_behavior = value;
				Refresh ();
			}
		}

		public LinkLabel.LinkCollection Links {
			get { return link_collection;}
		}

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

		public override string Text {
			get { return base.Text;	}
			set {
				if (base.Text == value)
					return;

				base.Text = value;
				Refresh ();
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
			throw new NotImplementedException ();
		}

		#region Public Methods
		protected override AccessibleObject CreateAccessibilityInstance()
		{
			return base.CreateAccessibilityInstance();
		}

		protected override void CreateHandle ()
		{
			CreateLinkFont ();
			CreateLinkPieces ();
			base.CreateHandle();
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
			Refresh ();
		}

		protected override void OnGotFocus (EventArgs e)
		{
			base.OnGotFocus(e);
		}

		protected override void OnKeyDown (KeyEventArgs e)
		{
			base.OnKeyDown(e);
		}

		protected virtual void OnLinkClicked (LinkLabelLinkClickedEventArgs e)
		{
			if (LinkClicked != null)
				LinkClicked (this, e);
		}

		protected override void OnLostFocus (EventArgs e)
		{
			base.OnLostFocus (e);
		}

		protected override void OnMouseDown (MouseEventArgs e)
		{
			if (!Enabled) return;

			base.OnMouseDown(e);
			this.Capture = true;

			for (int i = 0; i < pieces.Length; i++) {
				if (pieces[i].rect.Contains (e.X, e.Y)) {
					if (pieces[i].link!= null) {
						pieces[i].clicked = true;
						Refresh ();
					}
					break;
				}
			}
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			if (!Enabled) return;

			base.OnMouseLeave(e);
		}

		protected override void OnMouseMove (MouseEventArgs e)
		{
			base.OnMouseMove (e);
		}

		protected override void OnMouseUp (MouseEventArgs e)
		{
			if (!Enabled) return;

			base.OnMouseUp (e);
			this.Capture = false;

			for (int i = 0; i < pieces.Length; i++) {
				if (pieces[i].link!= null && pieces[i].clicked == true) {

					if (LinkClicked != null)
						LinkClicked (this, new LinkLabelLinkClickedEventArgs (pieces[i].link));

					pieces[i].clicked = false;
					Refresh ();
				}
			}
		}

		protected override void OnPaint (PaintEventArgs pevent)
    		{
			if (Width <= 0 || Height <=  0 || Visible == false)
    				return;

			Draw ();
			pevent.Graphics.DrawImage (ImageBuffer, 0, 0);
		}

		protected override void OnPaintBackground(PaintEventArgs e)
		{

		}

		protected override void OnTextAlignChanged (EventArgs e)
		{
			base.OnTextAlignChanged (e);
			Refresh ();
		}

		protected override void OnTextChanged (EventArgs e)
		{
			base.OnTextChanged (e);
			Refresh ();
		}
		
		protected Link PointInLink (int x, int y)
		{
			for (int i = 0; i < pieces.Length; i++) {
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

		public void Select ()
		{
			base.Select ();
		}
		
		protected override void SetBoundsCore (int x, int y, int width, int height, BoundsSpecified specified)
		{
			base.SetBoundsCore (x, y, width, height, specified);
			Refresh ();
		}

		protected override void WndProc (ref Message m)
    		{
			base.WndProc (ref m);
    		}

		#endregion //Public Methods

		#region Private Methods

		internal void CreateLinkPieces ()
		{
			if (Links.Count == 0)
				return;

			int cur_piece = 0;

			if (Links.Count == 1 && Links[0].Start == 0 &&	Links[0].Length == -1) {
				pieces = new Piece [1];
				pieces[cur_piece] = new Piece();
				pieces[cur_piece].start = 0;
				pieces[cur_piece].end = Text.Length;
				pieces[cur_piece].link = Links[0];
				pieces[cur_piece].text = Text;
				pieces[cur_piece].rect = ClientRectangle;
				return;
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

						pieces[cur_piece].start = Links[l].Start;
						pieces[cur_piece].end = Links[l].Start + Links[l].Length;
						pieces[cur_piece].link = Links[l];
						pieces[cur_piece].text = Text.Substring (pieces[cur_piece].start,
						pieces[cur_piece].end - pieces[cur_piece].start);

						cur_piece++; /* Push link*/
						pieces[cur_piece] = new Piece();
						i+= Links[l].Length;
						pieces[cur_piece].start = i;
					}
				}
			}

			if (pieces[cur_piece].end == 0) {
				pieces[cur_piece].end = Text.Length;
				pieces[cur_piece].text = Text.Substring (pieces[cur_piece].start, pieces[cur_piece].end - pieces[cur_piece].start);
			}

			CharacterRange[] charRanges = new CharacterRange [pieces.Length];

			for (int i = 0; i < pieces.Length; i++)
				charRanges[i] = new CharacterRange (pieces[i].start, pieces[i].end - pieces[i].start);

			Region[] charRegions = new Region [pieces.Length];
			string_format.SetMeasurableCharacterRanges (charRanges);

			charRegions = DeviceContext.MeasureCharacterRanges (Text, Font, ClientRectangle, string_format);

			for (int i = 0; i < pieces.Length; i++)  {
				//RectangleF[] f = charRegions[i].GetRegionScans (new Matrix());
				pieces[i].rect = charRegions[i].GetBounds (DeviceContext);
				Console.WriteLine (pieces[i].rect);
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

		private Color GetLinkColor (Piece piece, int i)
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

		internal void Draw ()
		{
			Color color;

			//dc.FillRectangle (label_br_back_color, area);
			ThemeEngine.Current.DrawBorderStyle (DeviceContext, ClientRectangle, BorderStyle);

			if (Links.Count == 1 && Links[0].Start == 0 &&	Links[0].Length == -1) {

				color = GetLinkColor (pieces[0], 0);
				DeviceContext.DrawString (Text, Font, new SolidBrush (color),
					ClientRectangle, string_format);
				return;
			}

			for (int i = 0; i < pieces.Length; i++)	{

				color = GetLinkColor (pieces[i], i);

				if (pieces[i].link == null)
					DeviceContext.DrawString (pieces[i].text, Font, new SolidBrush (Color.Black),
						pieces[i].rect.X, pieces[i].rect.Y, string_format);
				else
					DeviceContext.DrawString (pieces[i].text, link_font, new SolidBrush (color),
						pieces[i].rect.X, pieces[i].rect.Y, string_format);
			}

			DrawImage (DeviceContext, Image, ClientRectangle, image_align);
		}

		private void CreateLinkFont ()
		{
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
			private int length;
			private object linkData;
			private int start;
			private bool visited;
			private LinkLabel owner;

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
						owner.CreateLinkPieces ();
				}
			}

			public int Length {
				get { return length; }
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
						owner.CreateLinkPieces ();
				}
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
				Link link = new Link ();
				int idx;

				if (Count == 1 && this[0].Start == 0
					&& this[0].Length == -1) {
					Console.WriteLine ("Clear list");
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
				return collection.Add (control);
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
			}

			void IList.Remove (object control)
			{
				collection.Remove (control);
			}
		}
	}
}
