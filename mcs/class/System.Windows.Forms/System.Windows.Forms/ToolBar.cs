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
namespace System.Windows.Forms {

	// <summary>
	//
	// </summary>

    public class ToolBar : Control {

		//
		//  --- Public Constructors
		//
		[MonoTODO]
		public ToolBar() 
		{
			
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
		[MonoTODO]
		public override Color BackColor {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public override Image BackgroundImage{
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
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
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
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
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		internal bool dropDownArrows; //FIXME: Just to get it to run
		[MonoTODO]
		public bool DropDownArrows {
			get {
				return dropDownArrows;
			}
			set {
				dropDownArrows = value;
			}
		}
		[MonoTODO]
		public override Color ForeColor {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
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
		
		internal bool showToolTips;//FIXME: just to get it to run
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

				createParams.ClassName = "TOOLBAR";
				createParams.Style = (int) (
					WindowStyles.WS_CHILD | 
					WindowStyles.WS_VISIBLE);
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
				throw new NotImplementedException ();
			}
		}
		
		// --- Protected Methods
		
		[MonoTODO]
		protected override void CreateHandle() 
		{
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
			//FIXME:
		}
		[MonoTODO]
		protected override void OnResize(EventArgs e) 
		{
			//FIXME:
		}
		[MonoTODO]
		protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified) 
		{
			//FIXME:
		}
		[MonoTODO]
		protected override void WndProc(ref Message m) 
		{
			//FIXME:
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

