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
				//FIXME:
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
				//FIXME:
			}
		}

		[MonoTODO]
		public bool Focused {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}

		[MonoTODO]
		public Font Font {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}

		[MonoTODO]
		public Color Forecolor {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		[MonoTODO]
		public int ImageIndex {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
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
				//FIXME:
			}
		}
		[MonoTODO]
		public int StateImageIndex  {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
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
				//FIXME:
			}
		}
		[MonoTODO]
		public string Text {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		[MonoTODO]
		public bool UseItemStyleForSubItems {
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
		public void BeginEdit()
		{
			//FIXME:
		}
		[MonoTODO]
		public object Clone()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public virtual void EnsureVisible()
		{
			//FIXME:
		}
		[MonoTODO]
		public override bool Equals(object obj)
		{
			//FIXME:
			return base.Equals(obj);
		}
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
			//FIXME:
		}
		[MonoTODO]
		public override string ToString()
		{
			//FIXME:
			return base.ToString();
		}

		//
		//  --- Protected Methods
		//
		[MonoTODO]
		protected virtual void Deserialize(SerializationInfo info, StreamingContext context)
		{
			//FIXME:
		}
		[MonoTODO]
		protected virtual void Serialize(SerializationInfo info, StreamingContext context)
		{
			//FIXME:
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
		// </summary>
		[MonoTODO]
		[Serializable]
		public class ListViewSubItemCollection :  IList, ICollection, IEnumerable {
			/// Constructors
			[MonoTODO]
			public ListViewSubItemCollection(ListViewItem owner) 
			{
				//FIXME:
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
				set {
					//FIXME:
				}
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
				//FIXME:
			}
			
			[MonoTODO]
			public void AddRange(string[] items) 
			{
				//FIXME:
			}
			
			[MonoTODO]
			public void AddRange(
				string[] items,
				Color foreColor,
				Color backColor,
				Font font) {

				//FIXME:
			}
			
			[MonoTODO]
			public void Clear() 
			{
				//FIXME:
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
				//FIXME:
			}
			
			[MonoTODO]
			void IList.Remove(object item) 
			{
				//FIXME:
			}
			
			[MonoTODO]
			public int IndexOf(ListViewItem.ListViewSubItem subItem) 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public void Insert(int index,ListViewItem.ListViewSubItem item) 
			{
				//FIXME:
			}
			
			[MonoTODO]
			public void Remove(ListViewItem.ListViewSubItem item) 
			{
				//FIXME:
			}
			
			[MonoTODO]
			public void RemoveAt(int index) 
			{
				//FIXME:
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
	// </summary>

	public class ListViewSubItem {
	
		//
		//  --- Constructor
		//
		[MonoTODO]
		public ListViewSubItem()
		{
		}
		[MonoTODO]
		public ListViewSubItem(ListViewItem item, string str)
		{
			//FIXME:
		}
		[MonoTODO]
		public ListViewSubItem(ListViewItem item, string str, Color color1, Color color2, Font font)
		{
			//FIXME:
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
				//FIXME:
			}
		}
		[MonoTODO]
		public Font Font {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		[MonoTODO]
		public Color ForeColor {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		[MonoTODO]
		public string Text {
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
		public override bool Equals(object obj)
		{
			//FIXME:
			return base.Equals(obj);
		}
		[MonoTODO]
		public override int GetHashCode() {
			//FIXME add our proprities
			return base.GetHashCode();
		}
		[MonoTODO]
		public void ResetStyle()
		{
			//FIXME:
		}
		[MonoTODO]
		public override string ToString()
		{
			//FIXME:
			return base.ToString();
		}
		}
	}
}


