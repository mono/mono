//
// System.Windows.Forms.ListViewItem.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc
//
using System.Runtime.Serialization;
using System.Drawing;
using System.Collections;

namespace System.Windows.Forms {

	// <summary>
	// </summary>

	public class ListViewItem :  ICloneable, ISerializable {

		//
		//  --- Constructor
		//
		[MonoTODO]
		public ListViewItem()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public ListViewItem(string str)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public ListViewItem(string[] strings)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public ListViewItem(ListViewItem.ListViewSubItem[] subItems)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public ListViewItem(string str, int val)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public ListViewItem(string[] strings, int val)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public ListViewItem(string[] strings, int val,  Color color1, Color color2, Font font)
		{
			throw new NotImplementedException ();
		}

		//
		//  --- Public Properties
		//
		[MonoTODO]
		public Color BackColor {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public Rectangle Bounds {
			get {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool Checked {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool Focused {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public Font Font {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public Color Forecolor {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public int ImageIndex {
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
		}
		[MonoTODO]
		public int Index {
			get {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public ListView ListView {
			get {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool Selected {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public int StateImageIndex  {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public ListViewSubItemCollection SubItems {
			get {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public object Tag {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public string Text {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool UseItemStyleForSubItems {
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
		public void BeginEdit()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public object Clone()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public virtual void EnsureVisible()
		{
			throw new NotImplementedException ();
		}
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
		public Rectangle GetBounds(ItemBoundsPortion portion)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void Remove()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public override string ToString()
		{
			throw new NotImplementedException ();
		}

		//
		//  --- Protected Methods
		//
		[MonoTODO]
		protected virtual void Deserialize(SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual void Serialize(SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}
		
		// ISerializable.method:
		[MonoTODO]
		void ISerializable.GetObjectData(SerializationInfo info,StreamingContext context) 
		{
			throw new NotImplementedException ();
		}

		//
		// System.Windows.Forms.ListViewItem.ListViewSubItemCollection.cs
		//
		// Author:
		//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
		//   stub ammended by Jaak Simm (jaaksimm@firm.ee)
		//
		// (C) 2002 Ximian, Inc
		//
		// <summary>
		//	This is only a template.  Nothing is implemented yet.
		//
		// </summary>
		[MonoTODO]
		[Serializable]
		public class ListViewSubItemCollection :  IList, ICollection, IEnumerable {
			/// Constructors
			[MonoTODO]
			public ListViewSubItemCollection(ListViewItem owner) 
			{
				throw new NotImplementedException ();
			}
			
			
			/// Properties
			[MonoTODO]
			public int Count {
				get { throw new NotImplementedException (); }
			}
			
			[MonoTODO]
			public bool IsReadOnly {
				get { throw new NotImplementedException (); }
			}
		
			[MonoTODO]
			public ListViewSubItem this[int index] {
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
			
			
			/// Methods
			[MonoTODO]
			public ListViewSubItem Add(ListViewItem.ListViewSubItem item) 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public ListViewSubItem Add(string text) 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public ListViewSubItem Add(string text,Color foreColor,Color backColor,Font font) 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public void AddRange(ListViewItem.ListViewSubItem[] items) 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public void AddRange(string[] items) 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public void AddRange(
				string[] items,
				Color foreColor,
				Color backColor,
				Font font) {

				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public void Clear() 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public bool Contains(ListViewItem.ListViewSubItem subItem) 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public IEnumerator GetEnumerator() 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			void ICollection.CopyTo(Array dest,int index) 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			int IList.Add(object item) 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			bool IList.Contains(object subItem) 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			int IList.IndexOf(object subItem) 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			void IList.Insert(int index,object item) 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			void IList.Remove(object item) 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public int IndexOf(ListViewItem.ListViewSubItem subItem) 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public void Insert(int index,ListViewItem.ListViewSubItem item) 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public void Remove(ListViewItem.ListViewSubItem item) 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public void RemoveAt(int index) 
			{
				throw new NotImplementedException ();
			}
		}
	//
	// System.Windows.Forms.ListViewItem.ListViewSubItem.cs
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

	public class ListViewSubItem {
	
		//
		//  --- Constructor
		//
		[MonoTODO]
		public ListViewSubItem()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public ListViewSubItem(ListViewItem item, string str)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public ListViewSubItem(ListViewItem item, string str, Color color1, Color color2, Font font)
		{
			throw new NotImplementedException ();
		}
	
		//
		//  --- Public Properties
		//
		[MonoTODO]
		public Color BackColor {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public Font Font {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public Color ForeColor {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public string Text {
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
		public override bool Equals(object o)
		{
			throw new NotImplementedException ();
		}
		//inherited
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
		public void ResetStyle()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public override string ToString()
		{
			throw new NotImplementedException ();
		}
		}
	}
}


