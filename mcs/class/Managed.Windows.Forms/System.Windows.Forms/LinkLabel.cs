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
//
//
// $Revision: 1.7 $
// $Modtime: $
// $Log: LinkLabel.cs,v $
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

			public Piece ()
			{
				start = end = 0;
				link = null;
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

		#region Events
		public event LinkLabelLinkClickedEventHandler LinkClicked;
		#endregion // Events

		public LinkLabel ()
		{
			link_area = new LinkArea ();
			link_behavior = LinkBehavior.SystemDefault;
			link_collection = new LinkCollection (this);
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
				if (value.Start <0 || value.Length >0)
					throw new ArgumentException();
				
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
			set { link_collection = value;}
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

		[MonoTODO]
		DialogResult IButtonControl.DialogResult {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException ();  }
		}

		[MonoTODO]
		void IButtonControl.NotifyDefault (bool value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IButtonControl.PerformClick ()
		{
			throw new NotImplementedException ();
		}

		#region Protected Instance Methods
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
		}

		protected override void OnFontChanged (EventArgs e)
		{
			base.OnFontChanged (e);
			CreateLinkFont ();
		}

		protected override void OnGotFocus( EventArgs e)
		{
			base.OnGotFocus(e);
		}

		protected override void OnKeyDown (KeyEventArgs e)
		{
			base.OnKeyDown(e);
		}

		protected override void OnLostFocus (EventArgs e)
		{
			base.OnLostFocus (e);
		}

		protected override void OnMouseDown (MouseEventArgs e)
		{
			if (!Enabled) return;

			base.OnMouseDown(e);
			Point pnt = new Point (e.X, e.Y);

			if (Links.Count == 0)  {
				if (paint_area.Contains (pnt)) {
					link_click = true;
					Refresh ();
				}
			}
			else {
				for (int i = 0; i < pieces.Length; i++) {
					if (pieces[i].rect.Contains (pnt)) {
						link_click = true;
						Refresh ();
						break;
					}
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
			Point pnt = new Point (e.X, e.Y);

			if (Links.Count == 0)  {
				if (paint_area.Contains (pnt)) {
					link_click = false;

					if (LinkClicked != null)
						LinkClicked (this, new LinkLabelLinkClickedEventArgs (new Link ()));

					Refresh ();
				}
			}
			else {
				for (int i = 0; i < pieces.Length; i++) {
					if (pieces[i].rect.Contains (pnt)) {
						link_click = false;

						if ((LinkClicked != null) && (pieces[i].link != null))
							LinkClicked (this, new LinkLabelLinkClickedEventArgs (pieces[i].link));

						Refresh ();
						break;
					}
				}
			}

			if (paint_area.Contains (new Point (e.X, e.Y))) {


				link_click = false;
				Refresh ();
			}
		}

		protected override void OnPaint (PaintEventArgs e)
		{
			base.OnPaint (e);
		}

		protected override void OnPaintBackground(PaintEventArgs e)
		{
			base.OnPaint (e);
		}

		protected override void OnTextAlignChanged (EventArgs e)
		{
			base.OnTabIndexChanged(e);
		}

		protected override void OnTextChanged (EventArgs e)
		{
			base.OnTabIndexChanged(e);
		}
		#endregion	// Protected instance methods
	
		internal void CreateLinkPieces ()
		{
			//Console.WriteLine ("CreateLinkPieces:" + Links.Count);

			if (Links.Count == 0)
				return;

			int num_pieces = (Links.Count * 2) + 1;
			pieces = new Piece [num_pieces];
			int cur_piece = 0;

			//Console.WriteLine ("pieces: " + num_pieces);

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
							pieces[cur_piece] = new Piece();
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
			charRegions = DeviceContext.MeasureCharacterRanges (Text, Font, paint_area, string_format);

			for (int i = 0; i < pieces.Length; i++)
				pieces[i].rect = charRegions[i].GetBounds (DeviceContext);

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

		internal void Draw ()
		{
			Color color;

			if (Visible == false) return;

			if (Enabled == false)
				color = DisabledLinkColor;
			else
				if (link_click == true)
					color = ActiveLinkColor;
				else
					if (LinkVisited == true)
						color = VisitedLinkColor;
					else
						color = LinkColor;

			if (Links.Count == 0 || pieces == null) {

				ThemeEngine.Current.DrawLabel (DeviceContext, paint_area, BorderStyle, Text,
					color, BackColor, link_font, string_format,
					true /* We paint ourselfs the disabled status*/);

				DrawImage (DeviceContext, Image, paint_area, image_align);
				return;
			}


			for (int i = 0; i < pieces.Length; i++)	{

				if (pieces[i].link == null)
					DeviceContext.DrawString (pieces[i].text, Font, new SolidBrush (Color.Black),
						pieces[i].rect.X, pieces[i].rect.Y, string_format);
				else
					DeviceContext.DrawString (pieces[i].text, link_font, new SolidBrush (color),
						pieces[i].rect.X, pieces[i].rect.Y, string_format);
			}

			DrawImage (DeviceContext, Image, paint_area, image_align);
		}

		private void CreateLinkFont ()
		{
			link_font  = new Font (Font.FontFamily, Font.Size, Font.Style | FontStyle.Underline,
				 Font.Unit);
		}

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
				this.owner = owner;
			}

			public int Count {
				get { return collection.Count; }
			}

			public bool IsReadOnly {
				get { return collection.IsReadOnly; }
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
				Link link = new Link ();
				int idx;

				link.Length = length;
				link.Start = start;

				idx = collection.Add (link);
				owner.CheckLinks ();
				owner.CreateLinkPieces ();
				return (Link)collection[idx];
			}


			public Link Add (int start, int length, object o)
			{
				Link link = new Link ();
				int idx;

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
				get {return collection.IsFixedSize;}
			}

			object IList.this[int index] {
				get { return collection[index]; }
				set { collection[index] = value; }
			}


			object ICollection.SyncRoot {
				[MonoTODO] get {
					throw new NotImplementedException ();
				}
			}

			bool ICollection.IsSynchronized {
				[MonoTODO] get {
					throw new NotImplementedException ();
				}
			}

			[MonoTODO]
			void ICollection.CopyTo (Array dest,int index)
			{
				throw new NotImplementedException ();
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
