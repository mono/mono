//
// System.Windows.Forms.ToolBar
//
// Author:
//   stubbed out by Jackson Harper (jackson@latitudegeo.com)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc
//
using System.Drawing;
using System.Collections;
using System.ComponentModel;

namespace System.Windows.Forms {

	// <summary>
	//
	// </summary>

    public class ToolBar : Control {
		private Size buttonSize;
		private bool dropDownArrows;
		private bool showToolTips;

		//
		//  --- Public Constructors
		//
		[MonoTODO]
		public ToolBar() 
		{
			Dock = DockStyle.Top;
			buttonSize = new Size ( 24, 22 );
			dropDownArrows = false;
			showToolTips = false;
		}
		//
		// --- Public Properties
		//
		[MonoTODO]
		public ToolBarAppearance Appearance {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		[MonoTODO]
		public bool AutoSize {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}

		[EditorBrowsable (EditorBrowsableState.Never)]	 
		public override Color BackColor {
			get { return base.BackColor;  }
			set { base.BackColor = value; }
		}

		[EditorBrowsable (EditorBrowsableState.Never)]	 
		public override Image BackgroundImage{
			get { return base.BackgroundImage;  }
			set { base.BackgroundImage = value; }
		}

		[MonoTODO]
		public BorderStyle BorderStyle{
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public ToolBar.ToolBarButtonCollection Buttons {
			get {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public Size ButtonSize {
			get {
				return buttonSize;
			}
			set {
				if ( value.Width < 0 || value.Height < 0 )
					throw new ArgumentOutOfRangeException( "value" );

				buttonSize = value;
			}
		}
		[MonoTODO]
		public bool Divider {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public override DockStyle Dock{
			get {
				if ( base.Dock == DockStyle.Fill )
					return DockStyle.Top;
				return base.Dock;
			}
			set {
				base.Dock = value;
			}
		}

		[MonoTODO]
		public bool DropDownArrows {
			get {
				return dropDownArrows;
			}
			set {
				dropDownArrows = value;
			}
		}

		[EditorBrowsable (EditorBrowsableState.Never)]	 
		public override Color ForeColor {
			get { return base.ForeColor;  }
			set { base.ForeColor = value; }
		}
		[MonoTODO]
		public ImageList ImageList {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public Size ImageSize {
			get {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public new ImeMode ImeMode {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public override RightToLeft RightToLeft {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public bool ShowToolTips {
			get {
				return showToolTips;
			}
			set {
				showToolTips = value;
			}
		}
		[MonoTODO]
		public override string Text {
			get {
				return base.Text;
			}
			set {
				base.Text = value;
			}
		}
		[MonoTODO]
		public ToolBarTextAlign TextAlign {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool Wrappable{
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		// --- Public Methods
		
		[MonoTODO]
		public override string ToString() 
		{
			throw new NotImplementedException ();
		}
		
		// --- Public Events
		
		[MonoTODO]
		public event ToolBarButtonClickEventHandler ButtonClick;
		[MonoTODO]
		public event ToolBarButtonClickEventHandler ButtonDropDown;
		//
		// --- Protected Properties
		//
		[MonoTODO]
		protected override CreateParams CreateParams {
			get {
				CreateParams createParams = base.CreateParams;

				createParams.ClassName = Win32.TOOLBAR_CLASS;
				createParams.Style |= (int) WindowStyles.WS_CHILD;
				return createParams;
			}		
		}
		[MonoTODO]
		protected override ImeMode DefaultImeMode {
			get {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		protected override Size DefaultSize {
			get {
				return new Size ( 100, 22 );
			}
		}
		
		// --- Protected Methods
		
		[MonoTODO]
		protected override void CreateHandle() 
		{
			initCommonControlsLibrary ( );
			base.CreateHandle();
		}

		[MonoTODO]
		protected virtual void OnButtonClick(ToolBarButtonClickEventArgs e) 
		{
			//FIXME:
		}
		[MonoTODO]
		protected virtual void OnButtonDropDown(ToolBarButtonClickEventArgs e) 
		{
			//FIXME:
		}
		[MonoTODO]
		protected override void OnFontChanged(EventArgs e) 
		{
			//FIXME:
		}
		[MonoTODO]
		protected override void OnHandleCreated(EventArgs e) 
		{
			base.OnHandleCreated ( e );
			UpdateBounds ( );
		}
		[MonoTODO]
		protected override void OnResize(EventArgs e) 
		{
			base.OnResize ( e );
		}
		[MonoTODO]
		protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified) 
		{
			base.SetBoundsCore ( x, y, width, height, specified );
		}
		[MonoTODO]
		protected override void WndProc(ref Message m) 
		{
		}

		private void initCommonControlsLibrary ( ) {
			if ( !RecreatingHandle ) {
				INITCOMMONCONTROLSEX	initEx = new INITCOMMONCONTROLSEX();
				initEx.dwICC = CommonControlInitFlags.ICC_TAB_CLASSES;
				Win32.InitCommonControlsEx(initEx);
			}
		}

		public class ToolBarButtonCollection : IList, ICollection, IEnumerable {
			//
			// --- Public Constructor
			//
			[MonoTODO]
			public ToolBarButtonCollection(ToolBar owner)
			{
				
			}
			//
			// --- Public Properties
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
			public virtual ToolBarButton this[int index] {
				get {
					throw new NotImplementedException ();
				}
				set {
					throw new NotImplementedException ();
				}
			}
			//
			// --- Public Methods
			//
			[MonoTODO]
			public int Add(string text) 
			{
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public int Add(ToolBarButton button) 
			{
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public void AddRange(ToolBarButton[] buttons) 
			{
				//FIXME:
			}
			[MonoTODO]
			public void Clear() 
			{
				//FIXME:
			}
			[MonoTODO]
			public bool Contains(ToolBarButton button) 
			{
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public IEnumerator GetEnumerator() 
			{
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public int IndexOf(ToolBarButton button) 
			{
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public void Insert(int index, ToolBarButton button) 
			{
				throw new NotImplementedException ();
			}
			//[MonoTODO]
			//public void Insert(int index, ToolBarButton button) {
			//	throw new NotImplementedException ();
			//}
			[MonoTODO]
			public void Remove(ToolBarButton button) 
			{
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public void RemoveAt(int index) 
			{
				throw new NotImplementedException ();
			}
			/// <summary>
			/// IList Interface implmentation.
			/// </summary>
			bool IList.IsReadOnly{
				get{
					// We allow addition, removeal, and editing of items after creation of the list.
					return false;
				}
			}
			bool IList.IsFixedSize{
				get{
					// We allow addition and removeal of items after creation of the list.
					return false;
				}
			}

			//[MonoTODO]
			object IList.this[int index]{
				get{
					throw new NotImplementedException ();
				}
				set{
					throw new NotImplementedException ();
				}
			}
		
			[MonoTODO]
			void IList.Clear(){
				throw new NotImplementedException ();
			}
		
			[MonoTODO]
			int IList.Add( object value){
				throw new NotImplementedException ();
			}

			[MonoTODO]
			bool IList.Contains( object value){
				throw new NotImplementedException ();
			}

			[MonoTODO]
			int IList.IndexOf( object value){
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IList.Insert(int index, object value){
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IList.Remove( object value){
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IList.RemoveAt( int index){
				throw new NotImplementedException ();
			}
			// End of IList interface
			/// <summary>
			/// ICollection Interface implmentation.
			/// </summary>
			int ICollection.Count{
				get{
					throw new NotImplementedException ();
				}
			}
			bool ICollection.IsSynchronized{
				get{
					throw new NotImplementedException ();
				}
			}
			object ICollection.SyncRoot{
				get{
					throw new NotImplementedException ();
				}
			}
			void ICollection.CopyTo(Array array, int index){
				throw new NotImplementedException ();
			}
			// End Of ICollection
		}
	}
}

