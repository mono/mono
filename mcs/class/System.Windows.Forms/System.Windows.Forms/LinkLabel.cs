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
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public Color DisabledLinkColor {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public LinkArea LinkArea {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public LinkBehavior LinkBehavior {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public Color LinkColor {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
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
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public override ISite Site {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
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
				throw new NotImplementedException ();
			}
		}

		//
		//  --- Public Methods
		//
		//public IAsyncResult BeginInvoke(Delegate del)
		//{
		//	throw new NotImplementedException ();
		//}
		//public IAsyncResult BeginInvoke(Delegate del, object[] objs)
		//{
		//	throw new NotImplementedException ();
		//}
		//public void Dispose()
		//{
		//	throw new NotImplementedException ();
		//}
		[MonoTODO]
		public override bool Equals(object o)
		{
			throw new NotImplementedException ();
		}

		//public static bool Equals(object o1, object o2)
		//{
		//	throw new NotImplementedException ();
		//}
		[MonoTODO]
		public override int GetHashCode() {
			//FIXME add our proprities
			return base.GetHashCode();
		}
		//public void Invalidate()
		//{
		//	throw new NotImplementedException ();
		//}
		//public void Invalidate(bool val)
		//{
		//	throw new NotImplementedException ();
		//}
		//public void Invalidate(Rectangle rect)
		//{
		//	throw new NotImplementedException ();
		//}
		//public void Invalidate(Region reg)
		//{
		//	throw new NotImplementedException ();
		//}
		//public object Invoke(Delegate del)
		//{
		//	throw new NotImplementedException ();
		//}
		//public object Invoke(Delegate del, object[] objs)
		//{
		//	throw new NotImplementedException ();
		//}
		//public void PerformLayout()
		//{
		//	throw new NotImplementedException ();
		//}
		//public void PerformLayout(Control ctl, string str)
		//{
		//	throw new NotImplementedException ();
		//}
		//public void ResumeLayout()
		//{
		//	throw new NotImplementedException ();
		//}
		//public void ResumeLayout(bool val)
		//{
		//	throw new NotImplementedException ();
		//}
		//public void Scale(float val)
		//{
		//	throw new NotImplementedException ();
		//}
		//public void Scale(float val1, float val2)
		//{
		//	throw new NotImplementedException ();
		//}
		//public override void Select()
		//{
		//	throw new NotImplementedException ();
		//}
		//public override void Select(bool directed, bool forward)
		//{
		//	throw new NotImplementedException ();
		//}
		//public void SetBounds(int b1, int b2, int b3, int b4)
		//{
		//	throw new NotImplementedException ();
		//}
		//public void SetBounds(int b1, int b2, int b3, int b4, int b5, int b6)
		//{
		//	throw new NotImplementedException ();
		//}
		
		// --- IButtonControl members:
		DialogResult IButtonControl.DialogResult {
			[MonoTODO] get { throw new NotImplementedException (); }
			[MonoTODO] set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		void IButtonControl.NotifyDefault(bool value) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		void IButtonControl.PerformClick() 
		{
			throw new NotImplementedException ();
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
				throw new NotImplementedException ();
			}
		}

		//
		//  --- Protected Methods
		//
		[MonoTODO]
		protected override AccessibleObject CreateAccessibilityInstance()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void CreateHandle()
		{
			//FIXME: this is just to get it running
			base.CreateHandle();
		}

		//inherited
		//protected override virtual void Dispose(bool val)
		//{
		//	throw new NotImplementedException ();
		//}


		[MonoTODO]
		protected override void OnEnabledChanged(EventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void OnFontChanged( EventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void OnGotFocus( EventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void OnKeyDown (KeyEventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void OnLostFocus (EventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void OnMouseDown (MouseEventArgs e)
		{
			throw new NotImplementedException ();
		}
		// I think that this should be 'MouseEventArgs' 
		// but the documentation says EventArgs.
		[MonoTODO]
		protected override void OnMouseLeave(EventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void OnMouseMove (MouseEventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void OnMouseUp (MouseEventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void OnPaint (PaintEventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void OnPaintBackground(PaintEventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void OnTextAlignChanged( EventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void OnTextChanged( EventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override bool ProcessDialogKey(Keys keyData)
		{
			throw new NotImplementedException ();
		}
		//protected ContentAlignment RtlTranslateAlignment(ContentAlignment align)
		//{
		//	throw new NotImplementedException ();
		//}
		//protected HorizontalAlignment RtlTranslateAlignment( HorizontalAlignment align)
		//{
		//	throw new NotImplementedException ();
		//}
		//protected LeftRightAlignment RtlTranslateAlignment( LeftRightAlignment align)
		//{
		//	throw new NotImplementedException ();
		//}
		[MonoTODO]
		protected override void Select(bool val1, bool val2)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void SetBoundsCore(
			int x, 
			int y, 
			int width, 
			int height, 
			BoundsSpecified specified) {

			throw new NotImplementedException ();
		}
		//protected void UpdateBounds()
		//{
		//	throw new NotImplementedException ();
		//}
		//protected void UpdateBounds(int val1, int val2, int val3, int val4)
		//{
		//	throw new NotImplementedException ();
		//}
		//protected void UpdateBounds(int val1, int val2, int val3, int val4, int val5, int val6)
		//{
		//	throw new NotImplementedException ();
		//}
		[MonoTODO]
		protected override void WndProc(ref Message msg)
		{
			throw new NotImplementedException ();
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
			//	This is only a template.  Nothing is implemented yet.
			//
			// </summary>

		public class LinkCollection :  IList, ICollection, IEnumerable {

			//
			//  --- Constructor
			//
			[MonoTODO]
			public LinkCollection(LinkLabel owner)
			{
				throw new NotImplementedException ();
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
					throw new NotImplementedException ();
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
				throw new NotImplementedException ();
			}
			// Inherited
			//public override bool Contains(LinkLabel.Link link)
			//{
			//	throw new NotImplementedException ();
			//}
			[MonoTODO]
			public override bool Equals(object o)
			{
				throw new NotImplementedException ();
			}

			//public static bool Equals(object o1, object o2)
			//{
			//	throw new NotImplementedException ();
			//}
			[MonoTODO]
			public override int GetHashCode() {
				//FIXME add our proprities
				return base.GetHashCode();
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
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public void RemoveAt(int index)
			{
				throw new NotImplementedException ();
			}
			
			/// --- LinkLabel.IList properties ---
			bool IList.IsFixedSize {
				[MonoTODO] get { throw new NotImplementedException (); }
			}
			
			object IList.this[int index] {

				[MonoTODO] get { throw new NotImplementedException (); }
				[MonoTODO] set { throw new NotImplementedException (); }
			}
	
			object ICollection.SyncRoot {

				[MonoTODO] get { throw new NotImplementedException (); }
			}
	
			bool ICollection.IsSynchronized {

				[MonoTODO] get { throw new NotImplementedException (); }
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
				throw new NotImplementedException ();
			}
		
			[MonoTODO]
			void IList.Remove(object control) 
			{
				throw new NotImplementedException ();
			}
		}//End of subclass
	}// End of Class
}
