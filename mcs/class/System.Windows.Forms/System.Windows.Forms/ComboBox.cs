//
// System.Windows.Forms.ComboBox.cs
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) Ximian, Inc., 2002
//

using System.ComponentModel;
using System.Collections;
using System.Drawing;

namespace System.Windows.Forms {

	/// <summary>
	/// Represents a Windows combo box control.
	/// ToDo note:
	///  - nothing is implemented
	/// </summary>

	[MonoTODO]
	public class ComboBox : ListControl {

		// private fields
		DrawMode drawMode;
		ComboBoxStyle dropDownStyle;
		bool droppedDown;
		bool integralHeight;
		bool sorted;
		Image backgroundImage;
		ControlStyles controlStyles;
		string text;
		int selectedLength;
		string selectedText;
		int selectedIndex;
		object selectedItem;
		int selecedStart;

		bool updateing; // true when begin update has been called. do not paint when true;
		// --- Constructor ---
		public ComboBox() : base() 
		{
			selectedLength = 0;
			selectedText = "";
			selectedIndex = 0;
			selectedItem = null;
			selecedStart = 0;
			updateing = false;
			//controlStyles = null;
			drawMode = DrawMode.Normal;
			dropDownStyle = ComboBoxStyle.DropDown;
			droppedDown = false;
			integralHeight = true;
			sorted = false;
			backgroundImage = null;
			text = "";
			
		}
		
		// --- Properties ---
		[MonoTODO]
		public override Color BackColor {
			get { 
				return base.BackColor;
			}
			set { 
				if(BackColor.A != 255){
					if(
						(controlStyles & ControlStyles.SupportsTransparentBackColor) != 
						ControlStyles.SupportsTransparentBackColor 
						){
						throw new 
							ArgumentOutOfRangeException("BackColor", BackColor, "Transparant background color not allowed.");
					}
				}
				base.BackColor = value;
			}
		}
		
		public override Image BackgroundImage {
			get {
				return backgroundImage; 
			}
			set { 
				backgroundImage = value;
			}
		}
		
		[MonoTODO]
		protected override CreateParams CreateParams {
			get {
				throw new NotImplementedException ();
			}
		}
		
		protected override Size DefaultSize {
			get {
				return new Size(100,20);
			}
		}
		
		public DrawMode DrawMode {
			get {
				return drawMode;
			}
			set {
				drawMode = value;
			}
		}
		
		public ComboBoxStyle DropDownStyle {
			get {
				return dropDownStyle;
			}
			set {
				dropDownStyle = value;
			}
		}
		
		[MonoTODO]
		public int DropDownWidth {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		public bool DroppedDown {
			get { 
				return droppedDown;
			}
			set {
				droppedDown = value; 
			}
		}
		
		[MonoTODO]
		public override bool Focused {
			get {
				throw new NotImplementedException ();
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
		
		public bool IntegralHeight {
			get {
				return integralHeight;
			}
			set {
				integralHeight=value;
			}
		}
		
		[MonoTODO]
		public int ItemHeight {
			get {
				throw new NotImplementedException (); 
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public ComboBox.ObjectCollection Items {
			get { 
				throw new NotImplementedException (); 
			}
		}
		
		[MonoTODO]
		public int MaxDropDownItems {
			get { throw new NotImplementedException ();
			}
			set { throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public int MaxLength {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public int PreferredHeight {
			get {
				return 20; //FIXME: this is the default, good as any?
			}
		}
	
		[MonoTODO]
		public override int SelectedIndex {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public object SelectedItem {
			get {
				throw new NotImplementedException ();
			}
			set { 
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public string SelectedText {
			get { 
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException (); 
			}
		}
		
		[MonoTODO]
		public int SelectionLength {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public int SelectionStart {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		public bool Sorted {
			get {
				return sorted;
			}
			set {
				sorted = value;
			}
		}
		
		[MonoTODO]
		public override string Text {
			get {
				return text;
			}
			set {
				text = value;
			}
		}
		
		
		
		
		/// --- Methods ---
		/// internal .NET framework supporting methods, not stubbed out:
		[MonoTODO]
		protected override void OnSelectedValueChanged(EventArgs e){ // .NET V1.1 Beta
			throw new NotImplementedException ();
		}

		/// - protected override void SetItemCore(int index,object value);
		[MonoTODO]
		protected virtual void AddItemsCore(object[] value) {
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void BeginUpdate() 
		{
			updateing = true;
		}
		
		[MonoTODO]
		protected override void Dispose(bool disposing) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void EndUpdate() 
		{
			updateing = false;
		}
		
		[MonoTODO]
		public int FindString(string s) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public int FindString(string s,int startIndex) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public int FindStringExact(string s) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public int FindStringExact(string s,int startIndex) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public int GetItemHeight(int index) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override bool IsInputKey(Keys keyData) 
		{
			throw new NotImplementedException ();
		}
		
		/// [methods for events]
		[MonoTODO]
		protected override void OnBackColorChanged(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void OnDataSourceChanged(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void OnDisplayMemberChanged(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnDrawItem(DrawItemEventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnDropDown(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnDropDownStyleChanged(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void OnFontChanged(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void OnForeColorChanged(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void OnHandleCreated(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void OnHandleDestroyed(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void OnKeyPress(KeyPressEventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnMeasureItem(MeasureItemEventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void OnParentBackColorChanged(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void OnResize(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void OnSelectedIndexChanged(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnSelectionChangeCommitted(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		/// end of [methods for events]
		
		
		[MonoTODO]
		protected override void RefreshItem(int index) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void Select(int start,int length) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void SelectAll() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void SetBoundsCore(int x,int y,int width,int height,BoundsSpecified specified) 
		{
			throw new NotImplementedException ();
		}
		
		// for IList interface
		// FIXME not sure how to handle this
		//[MonoTODO]
		//protected override void SetItemsCore(IList value) 
		//{
		//	throw new NotImplementedException ();
		//}
		
		[MonoTODO]
		public override string ToString() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void WndProc(ref Message m) 
		{
			throw new NotImplementedException ();
		}
		
	
		/// --- Button events ---
		/// commented out, cause it only supports the .NET Framework infrastructure
		[MonoTODO]
		public event DrawItemEventHandler DrawItem {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event EventHandler DropDown {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event EventHandler DropDownStyleChanged {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event MeasureItemEventHandler MeasureItem {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		/* only supports .NET framework
			[MonoTODO]
			public new event PaintEventHandler Paint;
		*/
		
		[MonoTODO]
		public event EventHandler SelectedIndexChanged {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event EventHandler SelectionChangeCommitted {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		
		
		
		
		/// --- public class ComboBox.ChildAccessibleObject : AccessibleObject ---
		/// the class is not stubbed, cause it's only used for .NET framework
		
		
		
		
		/// sub-class: ComboBox.ObjectCollection
		/// <summary>
		/// Represents the collection of items in a ComboBox.
		/// </summary>
		[MonoTODO]
		public class ObjectCollection : IList, ICollection, IEnumerable {
			
			/// --- ObjectCollection.constructor ---
			[MonoTODO]
			public ObjectCollection (ComboBox owner) 
			{
				throw new NotImplementedException ();
			}
			
			
			
			/// --- ObjectCollection Properties ---
			[MonoTODO]
			public int Count {
				get { throw new NotImplementedException (); }
			}
			
			[MonoTODO]
			public bool IsReadOnly {
				get { throw new NotImplementedException (); }
			}
			
			[MonoTODO]
			public int this[int index] {
				get { throw new NotImplementedException (); }
				set { throw new NotImplementedException (); }
			}

			/// --- ICollection properties ---
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
			
			/// --- methods ---
			/// --- ObjectCollection Methods ---
			/// Note: IList methods are stubbed out, otherwise IList interface cannot be implemented
			[MonoTODO]
			public int Add(object item) 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public void AddRange(object[] items) 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public void Clear() 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public bool Contains(object value) 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public void CopyTo(object[] dest,int arrayIndex) 
			{
				throw new NotImplementedException ();
			}
			
			/// for ICollection:
			[MonoTODO]
			void ICollection.CopyTo(Array dest,int index) 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public IEnumerator GetEnumerator() 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public int IndexOf(object value) 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public void Insert(int index,object item) 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public void Remove(object value) 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public void RemoveAt(int index) 
			{
				throw new NotImplementedException ();
			}
		}  // --- end of ComboBox.ObjectCollection ---
	}
}
