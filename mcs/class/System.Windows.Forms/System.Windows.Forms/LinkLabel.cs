//
// System.Windows.Forms.LinkLabel.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//   stub ammended by Jaak Simm (jaaksimm@firm.ee)
//	Dennis Hayes (dennish@raytek.com)
// (C) 2002 Ximian, Inc
//
using System.Collections;
using System.Drawing;
using System.ComponentModel;

namespace System.Windows.Forms {

	// <summary>
	// </summary>

	public class LinkLabel : Label, IButtonControl {

		//
		//  --- Constructor
		//
		[MonoTODO]
		public LinkLabel()
		{
			
		}

		//
		//  --- Public Properties
		//
		[MonoTODO]
		public Color ActiveLinkcolor {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		[MonoTODO]
		public Color DisabledLinkColor {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		[MonoTODO]
		public LinkArea LinkArea {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		[MonoTODO]
		public LinkBehavior LinkBehavior {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		[MonoTODO]
		public Color LinkColor {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		[MonoTODO]
		public LinkLabel.LinkCollection Links {
			get {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool LinkVisited {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		[MonoTODO]
		public override ISite Site {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		[MonoTODO]
		public override string Text {
			//FIXME: This is to get it to run
			get {
				return	base.Text;
			}
			set {
				base.Text = value;
			}
		}
		[MonoTODO]
		public Color VisitedLinkColor {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}

	
		// --- IButtonControl members:
		DialogResult IButtonControl.DialogResult {
			[MonoTODO] get {
						   throw new NotImplementedException ();
					   }
			[MonoTODO] set {
						   //FIXME:
					   }
		}

		[MonoTODO]
		void IButtonControl.NotifyDefault(bool value) 
		{
			//FIXME:
		}
		
		[MonoTODO]
		void IButtonControl.PerformClick() 
		{
			//FIXME:
		}
		// --- end of IButtonControl members

		
		//  --- Public Events
		//
		[MonoTODO]
		public event LinkLabelLinkClickedEventHandler LinkClicked;

		//
		//  --- Protected Properties
		//
		[MonoTODO]
		protected override ImeMode DefaultImeMode {
			get {
				//FIXME:
				return DefaultImeMode;
			}
		}

		//
		//  --- Protected Methods
		//
		[MonoTODO]
		protected override AccessibleObject CreateAccessibilityInstance()
		{
			//FIXME:
			return CreateAccessibilityInstance();
		}
		[MonoTODO]
		protected override void CreateHandle()
		{
			//FIXME: this is just to get it running
			base.CreateHandle();
		}

		[MonoTODO]
		protected override void OnEnabledChanged(EventArgs e)
		{
			//FIXME:
			base.OnEnabledChanged(e);
		}
		[MonoTODO]
		protected override void OnFontChanged( EventArgs e)
		{
			//FIXME:
			base.OnFontChanged(e);
		}
		[MonoTODO]
		protected override void OnGotFocus( EventArgs e)
		{
			//FIXME:
			base.OnGotFocus(e);
		}
		[MonoTODO]
		protected override void OnKeyDown (KeyEventArgs e)
		{
			//FIXME:
			base.OnKeyDown(e);
		}
		[MonoTODO]
		protected override void OnLostFocus (EventArgs e)
		{
			//FIXME:
			base.OnLostFocus(e);
		}
		[MonoTODO]
		protected override void OnMouseDown (MouseEventArgs e)
		{
			//FIXME:
			base.OnMouseDown(e);
		}
		// I think that this should be 'MouseEventArgs' 
		// but the documentation says EventArgs.
		[MonoTODO]
		protected override void OnMouseLeave(EventArgs e)
		{
			//FIXME:
			base.OnMouseLeave(e);
		}
		[MonoTODO]
		protected override void OnMouseMove (MouseEventArgs e)
		{
			//FIXME:
			base.OnMouseMove(e);
		}
		[MonoTODO]
		protected override void OnMouseUp (MouseEventArgs e)
		{
			//FIXME:
			base.OnMouseUp(e);
		}
		[MonoTODO]
		protected override void OnPaint (PaintEventArgs e)
		{
			//FIXME:
			base.OnPaint(e);
		}
		[MonoTODO]
		protected override void OnPaintBackground(PaintEventArgs e)
		{
			//FIXME:
			base.OnPaint(e);
		}
		[MonoTODO]
		protected override void OnTextAlignChanged( EventArgs e)
		{
			//FIXME:
			base.OnTabIndexChanged(e);
		}
		[MonoTODO]
		protected override void OnTextChanged( EventArgs e)
		{
			//FIXME:
			base.OnTabIndexChanged(e);
		}

		[MonoTODO]
		protected override bool ProcessDialogKey(Keys keyData)
		{
			//FIXME:
			return base.ProcessDialogKey(keyData);
		}

		[MonoTODO]
		protected override void Select(bool directed, bool forward)
		{
			//FIXME:
			base.Select(directed, forward);
		}
		[MonoTODO]
		protected override void SetBoundsCore(
			int x, 
			int y, 
			int width, 
			int height, 
			BoundsSpecified specified) {

			//FIXME:
			base.SetBoundsCore(x, y, width, height, specified);
		}
		[MonoTODO]
		protected override void WndProc(ref Message msg)
		{
			//FIXME:
			base.WndProc(ref msg);
		}
		
		/// System.Windows.Forms.LinkLabel.Link
		/// <summary>Represents a link within a LinkLabel control.</summary>
		///
		/// stubbed out by Jaak Simm (jaaksimm@firm.ee)
		[MonoTODO]
		public class Link {
			bool enabled;
			int length;
			object linkData;
			int start;
			bool visited;
			
			public bool Enabled {
				get { return enabled; }
				set { enabled=value; }
			}
			
			public int Length {
				get { return length; }
				set { length=value; }
			}
			
			public object LinkData {
				get { return linkData; }
				set { linkData=value; }
			}
			
			public int Start {
				get { return start; }
				set { start=value; }
			}
			
			public bool Visited {
				get { return visited; }
				set { visited=value; }
			}
		}
		
//
// System.Windows.Forms.LinkLabel.LinkCollection.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//
// (C) 2002 Ximian, Inc
//

			// <summary>
			// </summary>

		public class LinkCollection :  IList, ICollection, IEnumerable {

			//
			//  --- Constructor
			//
			[MonoTODO]
			public LinkCollection(LinkLabel owner)
			{
				//FIXME:
			}

			//
			//  --- Public Properties
			//
			[MonoTODO]
			public int Count {
				get {
					throw new NotImplementedException ();
				}
			}
			[MonoTODO]
			public bool IsReadOnly {
				get {
					throw new NotImplementedException ();
				}
			}
			[MonoTODO]
			public virtual LinkLabel.Link this[ int index]  {
				get {
					throw new NotImplementedException ();
				}
				set {
					//FIXME:
				}
			}

			//
			//  --- Public Methods
			//
			[MonoTODO]
			public Link Add(int val1, int val2)
			{
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public Link Add(int val1, int val2, object o)
			{
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public virtual void Clear()
			{
				//FIXME:
			}

			[MonoTODO]
			public IEnumerator GetEnumerator()
			{
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public int IndexOf(LinkLabel.Link link)
			{
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public void Remove(LinkLabel.Link value)
			{
				//FIXME:
			}
			[MonoTODO]
			public void RemoveAt(int index)
			{
				//FIXME:
			}
			
			/// --- LinkLabel.IList properties ---
			bool IList.IsFixedSize {
				[MonoTODO] get { throw new NotImplementedException (); }
			}
			
			object IList.this[int index] {
				[MonoTODO]
				get {
					throw new NotImplementedException ();
				}
				[MonoTODO]
				set {
					//FIXME:
				}
			}
			object ICollection.SyncRoot {
				[MonoTODO] 
				get {
					throw new NotImplementedException ();
				}
			}
	
			bool ICollection.IsSynchronized {
				[MonoTODO]
				get {
					throw new NotImplementedException ();
				}
			}
			
			[MonoTODO]
			void ICollection.CopyTo(Array dest,int index) 
			{
				throw new NotImplementedException ();
			}
			
			/// --- LinkLabel.IList methods ---
			[MonoTODO]
			int IList.Add(object control) 
			{
				throw new NotImplementedException ();
			}
		
			[MonoTODO]
			bool IList.Contains(object control) 
			{
				throw new NotImplementedException ();
			}
		
			[MonoTODO]
			int IList.IndexOf(object control) 
			{
				throw new NotImplementedException ();
			}
		
			[MonoTODO]
			void IList.Insert(int index,object value) 
			{
				//FIXME:
			}
		
			[MonoTODO]
			void IList.Remove(object control) 
			{
				//FIXME:
			}
		}//End of subclass
	}// End of Class
}
