//
// System.Windows.Forms.TabControl
//
// Author:
//   stubbed out by Jackson Harper (jackson@latitudegeo.com)
//   Dennis Hayes (dennish@Raytek.com)
//   implemented by Aleksey Ryabchuk (ryabchuk@yahoo.com)
// (C) 2002 Ximian, Inc
//
using System.ComponentModel;
using System.Collections;
using System.Drawing;
using System.Runtime.InteropServices;

namespace System.Windows.Forms {

	public class TabControl : Control  {

		public class TabPageControlCollection : ControlCollection {

			public TabPageControlCollection ( Control owner ): base( owner ){ }

			public override void Add( Control c ) {
				if ( !( c is TabPage ) ) {
					throw new ArgumentException();
				}
				base.Add(c);
			}
		}

		private int selectedIndex;
		private TabAlignment tabAlignment;
		private bool multiline;
		private TabAppearance appearance;
		private TabDrawMode tabDrawMode;
		private bool hotTrack;
		private Point padding;
		private Size  itemSize;
		private TabSizeMode sizeMode;
		private bool showTooltips;

		[MonoTODO]
		public TabControl() {
			selectedIndex = -1;
			multiline = false;
			tabAlignment = TabAlignment.Top;
			appearance   = TabAppearance.Normal;
			tabDrawMode  = TabDrawMode.Normal;
			hotTrack = false;
			padding = new Point ( 6, 3 );
			itemSize = Size.Empty;
			sizeMode = TabSizeMode.Normal;
			showTooltips = false;
		}

		[MonoTODO]
		public TabAlignment Alignment  {
			get {
				return tabAlignment;
			}
			set {
				if ( !Enum.IsDefined ( typeof(TabAlignment), value ) )
					throw new InvalidEnumArgumentException( "Alignment",
						(int)value,
						typeof(TabAlignment));

				if ( tabAlignment != value ) {
					if ( value == TabAlignment.Right ||
						 value == TabAlignment.Left )
							Multiline = true;
				
					tabAlignment = value;

					if ( IsHandleCreated )
						RecreateHandle( );
				}
			}
		}

		[MonoTODO]
		public TabAppearance Appearance  {
			get {	return appearance; }
			set {
				if ( !Enum.IsDefined ( typeof(TabAppearance), value ) )
					throw new InvalidEnumArgumentException( "Appearance",
						(int)value,
						typeof(TabAppearance));

				if ( appearance != value ) {
					appearance = value;
					
					if ( IsHandleCreated )
						RecreateHandle( );
				}
			}
		}

		[EditorBrowsable (EditorBrowsableState.Never)]	 
		public override Color BackColor  {
			get {	return base.BackColor;	}
			set {	base.BackColor = value;	}
		}

		[EditorBrowsable (EditorBrowsableState.Never)]	 
		public override Image BackgroundImage  {
			get {	return base.BackgroundImage; }
			set {	base.BackgroundImage = value;}
		}

		[MonoTODO]
		public override Rectangle DisplayRectangle  {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public TabDrawMode DrawMode  {
			get {	return tabDrawMode; }
			set {
				if ( !Enum.IsDefined ( typeof(TabDrawMode), value ) )
					throw new InvalidEnumArgumentException( "DrawMode",
						(int)value,
						typeof(TabDrawMode));

				if ( tabDrawMode != value ) {
					tabDrawMode = value;

					if ( IsHandleCreated )
						RecreateHandle ( );
				}
			}
		}

		[EditorBrowsable (EditorBrowsableState.Never)]	 
		public override Color ForeColor  {
			get {	return base.ForeColor;	}
			set {	base.ForeColor = value; }
		}

		public bool HotTrack  {
			get {	return hotTrack; }
			set {
				if ( hotTrack != value ) {
					hotTrack = value;

					if ( IsHandleCreated )
						RecreateHandle ( );
				}
			}
		}

		[MonoTODO]
		public ImageList ImageList  {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public Size ItemSize  {
			get {	return itemSize;  } // FIXME: don't know how to get size initially
			set {
				if ( itemSize != value ) {
					if ( value.Width < 0 || value.Height < 0 )
						throw new ArgumentException ( ); // FIXME: message

					itemSize = value;
					
					if ( IsHandleCreated )
						setItemSize ( );
				}
			}
		}

		[MonoTODO]
		public bool Multiline  {
			get {	return multiline; }
			set {
				if ( multiline != value ) {
					multiline = value;

					if ( multiline == false && ( Alignment == TabAlignment.Left ||
						Alignment == TabAlignment.Right ) )
							Alignment = TabAlignment.Top;

					if ( IsHandleCreated )
						RecreateHandle( );
				}
			}
		}

		[MonoTODO]
		public Point Padding  {
			get {	return padding;	}
			set {
				if ( padding != value ) {
					if ( value.X < 0 || value.Y < 0 )
						throw new ArgumentException ( ); // FIXME: message

					padding = value;
					
					if ( IsHandleCreated )
						setPadding ( );
				}
			}
		}

		[MonoTODO]
		public int RowCount {
			get {
				if ( TabCount == 0)
					return 0;
				if ( Multiline == false )
					return 1;
				// referencing this property creates handle in ms.swf
				return Win32.SendMessage ( Handle, (int) TabControlMessages.TCM_GETROWCOUNT, 0, 0);
			}
		}

		[MonoTODO]
		public int SelectedIndex {
			get {	return selectedIndex;  }
			set {
				if ( selectedIndex != value ) {
					if ( value < -1 )
						throw new ArgumentException ( ); // FIXME: message

					selectedIndex = value;

					if ( IsHandleCreated )
						selectPage ( selectedIndex );
				}
			}
		}

		[MonoTODO]
		public TabPage SelectedTab  {
			get {	
				if ( SelectedIndex >= 0 )
					return TabPages[ SelectedIndex ];
				return null;
			}
			set {
				int index = TabPages.IndexOf ( value );
				if ( index >= 0 )
					SelectedIndex = index;
			}
		}

		public bool ShowToolTips  {
			get {	return showTooltips;  }
			set {	
				if ( showTooltips != value ) {
					showTooltips = value;

					if ( IsHandleCreated )
						RecreateHandle ( );
				}
			}
		}

		public TabSizeMode SizeMode {
			get {	return sizeMode; }
			set {
				if ( !Enum.IsDefined ( typeof(TabSizeMode), value ) )
					throw new InvalidEnumArgumentException( "SizeMode",
						(int)value,
						typeof(TabSizeMode));

				if ( sizeMode != value ) {
					sizeMode = value;

					if ( IsHandleCreated )
						RecreateHandle ( );
				}
			}
		}

		public int TabCount  {
			get {	return Controls.Count;	}
		}

		public TabControl.TabPageCollection TabPages  {
			get {	return new TabPageCollection ( this );	}
		}

		[EditorBrowsable (EditorBrowsableState.Never)]	 
		public override string Text  {
			get {	return base.Text; }
			set {	base.Text = value;}
		}
		 
		[MonoTODO]
		public Rectangle GetTabRect(int index) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override string ToString() {
			throw new NotImplementedException ();
		}
		
		// --- Public Events
		
		[MonoTODO]
		public event DrawItemEventHandler DrawItem;
		public event EventHandler SelectedIndexChanged;		
		// --- Protected Properties
		
		[MonoTODO]
		protected override CreateParams CreateParams  {
			get {
				CreateParams createParams = base.CreateParams;
				createParams.ClassName = Win32.TABCONTROL;
				createParams.Style = (int) ( WindowStyles.WS_CHILD | WindowStyles.WS_VISIBLE );
				
				if ( Multiline )
					createParams.Style |= (int) TabControlStyles.TCS_MULTILINE;

				if ( DrawMode == TabDrawMode.OwnerDrawfixed )
					createParams.Style |= (int) TabControlStyles.TCS_OWNERDRAWFIXED;

				if ( HotTrack )
					createParams.Style |= (int) TabControlStyles.TCS_HOTTRACK;

				if ( ShowToolTips )
					createParams.Style |= (int) TabControlStyles.TCS_TOOLTIPS;

				switch ( Alignment ) {
				case TabAlignment.Bottom:
					createParams.Style |= (int) TabControlStyles.TCS_BOTTOM;
				break;
				case TabAlignment.Left:
					createParams.Style |= (int) TabControlStyles.TCS_VERTICAL;
				break;
				case TabAlignment.Right:
					createParams.Style |= (int) TabControlStyles.TCS_RIGHT;
				break;
				}

				switch ( Appearance ) {
				case TabAppearance.Buttons:
					createParams.Style |= (int) TabControlStyles.TCS_BUTTONS;
				break;
				case TabAppearance.FlatButtons:
					createParams.Style |= (int) TabControlStyles.TCS_FLATBUTTONS;
				break;
				}

				switch ( SizeMode ) {
				case TabSizeMode.Fixed:
					createParams.Style |= (int) TabControlStyles.TCS_FIXEDWIDTH;
				break;
				case TabSizeMode.FillToRight:
					createParams.Style |= (int) TabControlStyles.TCS_RIGHTJUSTIFY;
				break;
				}

				return createParams;
			}		
		}

		[MonoTODO]
		protected override Size DefaultSize {
			get {
				return new System.Drawing.Size(200,100);
			}
		}
		
		// --- Protected Methods
		
		protected override Control.ControlCollection CreateControlsInstance() {
			return new TabPageControlCollection ( this );
		}

		[MonoTODO]
		protected override void CreateHandle() {
			initCommonControlsLibrary ( );
			base.CreateHandle();
		}

		[MonoTODO]
		protected override void Dispose(bool disposing) { // .NET V1.1 Beta. .needs implmentation
			base.Dispose(disposing);
		}

		[MonoTODO]
		protected override bool IsInputKey(Keys keyData) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnDrawItem(DrawItemEventArgs e) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnFontChanged(EventArgs e) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnHandleCreated(EventArgs e) {
			base.OnHandleCreated ( e );
			setPages ( );
			setPadding ( );
			setItemSize ( );
			selectPage ( SelectedIndex );
		}

		[MonoTODO]
		protected override void OnHandleDestroyed(EventArgs e) {
			base.OnHandleDestroyed ( e );
			//throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnKeyDown(KeyEventArgs ke) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnResize(EventArgs e) {
			//throw new NotImplementedException ();
		}

		protected virtual void OnSelectedIndexChanged(EventArgs e) {
			if ( SelectedIndexChanged != null )
				SelectedIndexChanged ( this, e );
		}

		[MonoTODO]
		protected override void OnStyleChanged(EventArgs e) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override bool ProcessKeyPreview(ref Message m) {
			throw new NotImplementedException ();
		}

		protected void RemoveAll() {
			Controls.Clear ( );
		}

		[MonoTODO]
		protected override void WndProc(ref Message m) {
			switch ( m.Msg ) {
			case Msg.WM_NOTIFY:
				NMHDR nmhdr = (NMHDR)Marshal.PtrToStructure ( m.LParam,	typeof ( NMHDR ) );
				switch ( nmhdr.code ) {
				case (int)TabControlNotifications.TCN_SELCHANGE:
					selectedIndex =	Win32.SendMessage ( Handle, (int) TabControlMessages.TCM_GETCURSEL, 0, 0);
					OnSelectedIndexChanged ( EventArgs.Empty );
				break;
				}
			break;
			}
		}

		private void initCommonControlsLibrary ( ) {
			if ( !RecreatingHandle ) {
				INITCOMMONCONTROLSEX	initEx = new INITCOMMONCONTROLSEX();
				initEx.dwICC = CommonControlInitFlags.ICC_TAB_CLASSES;
				Win32.InitCommonControlsEx(initEx);
			}
		}
		
		private void update ( ) {
		}

		private void setPages ( ) {
			for (int i = 0; i < Controls.Count; i++ ) {
				TCITEMHEADER header = new TCITEMHEADER();
				header.mask = (uint) TabControlItemFlags.TCIF_TEXT;
				header.pszText = Controls[i].Text;
				
				sendMessageHelper ( TabControlMessages.TCM_INSERTITEM, i, ref header );
			}
		}

		internal void pageTextChanged ( TabPage page ) {
			if ( IsHandleCreated ) {
				int index = Controls.IndexOf ( page );
				if ( index != -1 ) {
					TCITEMHEADER header = new TCITEMHEADER();
					header.mask = (uint) TabControlItemFlags.TCIF_TEXT;
					header.pszText = page.Text;
				
					sendMessageHelper ( TabControlMessages.TCM_SETITEM, index, ref header );
				}
			}
		}

		private void sendMessageHelper ( TabControlMessages mes, int index, ref TCITEMHEADER hdr ) {
			if ( IsHandleCreated ) {
				IntPtr ptr	= Marshal.AllocHGlobal ( Marshal.SizeOf ( hdr ) );
				Marshal.StructureToPtr( hdr, ptr, false );
				Win32.SendMessage ( Handle , (int)mes, index, ptr.ToInt32() );
				Marshal.FreeHGlobal ( ptr );
			}
		}

		private void setPadding ( ) {
			Win32.SendMessage ( Handle, (int) TabControlMessages.TCM_SETPADDING, 0, Win32.MAKELONG ( Padding.X, Padding.Y ) );
		}

		private void setItemSize ( ) {
			if ( ItemSize != Size.Empty ) {
				Win32.SendMessage ( Handle, (int) TabControlMessages.TCM_SETPADDING, 0, Win32.MAKELONG ( ItemSize.Width, ItemSize.Height ) );
			}
		}

		private void selectPage ( int selectedIndex ) {
			if ( Win32.SendMessage ( Handle, (int) TabControlMessages.TCM_SETCURSEL, selectedIndex, 0 ) != -1 )
				OnSelectedIndexChanged ( EventArgs.Empty );
		}

		public class TabPageCollection : IList, ICollection, IEnumerable {
			TabControl owner;
			Control.ControlCollection collection;
			
			public TabPageCollection( TabControl owner ) {
				this.owner = owner;
				collection = owner.Controls;
			}

			public int Count {
				get { return collection.Count; }
			}

			public bool IsReadOnly {
				get {	return collection.IsReadOnly; }
			}

			[MonoTODO]
			public virtual TabPage this[int index] {
				get {	return collection[ index ] as TabPage; }
				set {	
					( (IList)collection )[ index ] = value;
					owner.update ( );
				}
			}
			
			public void Add(TabPage value) {
				collection.Add ( value );
				owner.update ( );
			}

			public void AddRange( TabPage[] pages ) {
				collection.AddRange ( pages );
				owner.update ( );
			}

			public virtual void Clear() {
				collection.Clear ( );
				owner.update ( );
			}

			public bool Contains( TabPage page ) {
				return collection.Contains ( page );
			}

			public IEnumerator GetEnumerator() {
				return collection.GetEnumerator ( );
			}

			public int IndexOf( TabPage page ) {
				return collection.IndexOf ( page );
			}

			public void Remove( TabPage value ) {
				collection.Remove ( value );
				owner.update ( );
			}

			public void RemoveAt(int index) {
				collection.RemoveAt ( index );
				owner.update ( );
			}

			/// <summary>
			/// IList Interface implmentation.
			/// </summary>
			bool IList.IsReadOnly{
				get{	return this.IsReadOnly; }
			}

			bool IList.IsFixedSize{
				get{	return (( IList )collection).IsFixedSize; }
			}

			object IList.this[int index]{
				get{	return collection [ index ]; }
				set{	if ( ! (value is TabPage) )
						throw new ArgumentException ( );
					this[ index ] = (TabPage) value;
					owner.update ( );
				}
			}
		
			void IList.Clear(){
				this.Clear ( );
			}
		
			[MonoTODO]
			int IList.Add( object value ) {
				TabPage page = value as TabPage;
				if ( page == null )
					throw new ArgumentException ( );
				this.Add ( page );
				return this.IndexOf ( page );
			}

			[MonoTODO]
			bool IList.Contains( object value ){
				return this.Contains ( value as TabPage );
			}

			[MonoTODO]
			int IList.IndexOf( object value ){
				return this.IndexOf ( value as TabPage );
			}

			[MonoTODO]
			void IList.Insert(int index, object value){
				if ( ! (value is TabPage) )
					throw new ArgumentException ( );

				(( IList )collection).Insert ( index, value );
				owner.update ( );
			}

			void IList.Remove( object value ){
				this.Remove ( value as TabPage );
			}

			void IList.RemoveAt( int index){
				this.RemoveAt ( index );
			}
			// End of IList interface

			/// <summary>
			/// ICollection Interface implmentation.
			/// </summary>
			int ICollection.Count{
				get{ return this.Count;	}
			}

			bool ICollection.IsSynchronized{
				get{ return ( (ICollection) collection).IsSynchronized;	}
			}

			object ICollection.SyncRoot{
				get{ return ( (ICollection) collection).SyncRoot; }
			}

			void ICollection.CopyTo(Array array, int index){
				( (ICollection) collection ).CopyTo ( array, index );
			}
			// End Of ICollection
		}
	}
}

