//
// System.Windows.Forms.ListViewItem.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//   Dennis Hayes (dennish@Raytek.com)
//	 Implemented by Jordi Mas i Hernàndez (jmas@softcatala.org)
//
// (C) 2002/3 Ximian, Inc
//
using System.Runtime.Serialization;
using System.Drawing;
using System.Collections;
using System.Runtime.InteropServices;
 
namespace System.Windows.Forms 
{
	// <summary>
	// </summary>
	[Serializable]
	public class ListViewItem :  ICloneable, ISerializable 
	{		
		private ListView container = null;
		private string  text;
		private	ListViewSubItemCollection	colSubItem = null;
		private int index;
		private bool bSelected = false;
		private bool useItemStyleForSubItems = true;
		private bool bChecked = false;
		private bool bFocused = false;
		private	Color backColor = SystemColors.Window;
		private	Color foreColor = SystemColors.WindowText;
		private object tag = null;
		

		//
		//  --- Constructor
		//					
		protected void CommonConstructor(){
			colSubItem = new 	ListViewSubItemCollection(this);
		}
		
		public ListViewItem(){			
			CommonConstructor();			
		}
		
		public ListViewItem(string str)	{
			Console.WriteLine("ListViewItem.ListViewItem str");					
			CommonConstructor();			
			text = str;
		}
		
		public ListViewItem(string[] strings){	// An array of strings that represent the subitems of the new item.
		
			Console.WriteLine("ListView.ListView strings");				
			CommonConstructor();
			
			if (strings.Length>0)			
				text = strings[0];
				
			if (strings.Length>1)			
			{
				for (int i=1; i<strings.Length; i++)
					colSubItem.Add(strings[i]);		
			}
		}

		
		public ListViewItem(ListViewItem.ListViewSubItem[] subItems){
			
			CommonConstructor();
			for (int i=0; i<subItems.Length; i++)
					colSubItem.Add(subItems[i]);		
		}

		
		public ListViewItem(string str, int imageIndex){							
				//TODO: Image index
				CommonConstructor();
				text = str;
		}

		
		public ListViewItem(string[] strings, int imageIndex){			
			//TODO: Image index
			CommonConstructor();
			
			if (strings.Length>0)			
				text = strings[0];
				
			if (strings.Length>1)			
			{
				for (int i=1; i<strings.Length; i++)
					colSubItem.Add(strings[i]);		
			}
		}

		
		public ListViewItem(string[] strings, int imageIndex,  Color fColor, Color  bColor, Font font){
			
			//TODO: Image index
			CommonConstructor();
			
			if (strings.Length>0){
				
				text = strings[0];
				BackColor = bColor;
				ForeColor = fColor;
			}
				
			if (strings.Length>1){
				ListViewSubItem subItem;
				
				for (int i=1; i<strings.Length; i++){
					subItem = colSubItem.Add(strings[i]);		
					subItem.BackColor = bColor;
					subItem.ForeColor = fColor;
				}
			}			
			
		}

		[MonoTODO]
	    public ListViewItem (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}
		

		//
		//  --- Public Properties
		//		
		public Color BackColor {
			get {return backColor;}
			set {backColor = value;}
		}

		
		public Rectangle Bounds {
			get {return	container.GetItemBoundInCtrl(Index);}
		}
		
		public bool Checked {
			get {return bChecked;}
			set {bChecked = value;}
		}

		
		public bool Focused {
			get {return bFocused;}
			set {bFocused = value;}
		}

		[MonoTODO]
		public Font Font {
			get { // see Control.DefaultFont
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}

		public Color ForeColor {
			get {return foreColor;}
			set {foreColor = value;}
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
		
		public int Index {
			get {return index;}
		}
		
						
		public ListView ListView {
			get {return container;}						
		}
		
		public bool Selected {
			get {return bSelected;}						
			set {bSelected=value;}									
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
		
		public ListViewSubItemCollection SubItems {
			get {return colSubItem;}
		}
		

		public object Tag {
			get {return tag;}
			set {tag = value;}
		}
		
		public string Text 	{
			get { return text;}
			set { text = value;}		
			
		}
		
		public bool UseItemStyleForSubItems {
			get { return useItemStyleForSubItems;}
			set { useItemStyleForSubItems = value;}		
		}
		
		//
		//  --- Private Methods
		//		
		internal ListView Container {			
			set{container=value;}
		}				
		
		internal int CtrlIndex{					
			set{index=value;}
		}		
		
		//
		//  --- Public Methods
		//		
		public void BeginEdit(){
			
			if (!container.LabelEdit)
				throw new InvalidOperationException("LabelEdit disabled");
			
			container.LabelEditInCtrl(Index);
			
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
		//	 Implemented by Jordi Mas i Hernàndez (jmas@softcatala.org)
		//
		// (C) 2002/3 Ximian, Inc
		//
		// <summary>
		// </summary>		
		[Serializable]
		public class ListViewSubItemCollection :  IList, ICollection, IEnumerable 
		{
			
			private ArrayList collection = new ArrayList();
			private ListViewItem owner = null;
			
			
			//
			//  --- Constructor
			//		
			public ListViewSubItemCollection(ListViewItem item) {
				owner = item;
			}			
			
			//
			//  --- Public Properties
			//			
			public int Count {
				get { return collection.Count; }
			}			
			
			public bool IsReadOnly 
			{
				get { return collection.IsReadOnly; }
			}		
			
			public ListViewSubItem this[int index] 
			{
				get { 
					if (index<0 || index>=Count) throw  new  ArgumentOutOfRangeException();					
						
					return (ListViewSubItem) collection[index];
				}
				set { 
					if (index<0 || index>=Count) throw  new  ArgumentOutOfRangeException();					
					collection[index] = value;
				}				
			}	
			
			/// --- ICollection properties ---
			bool IList.IsFixedSize 
			{
				get { return collection.IsFixedSize; }
			}
			
			object IList.this[int index] 
			{
				get { return collection[index]; }
				set { collection[index] = value; }
			}
	
			object ICollection.SyncRoot 
			{
				get { return collection.SyncRoot; }
			}
	
			bool ICollection.IsSynchronized 
			{
				get { return collection.IsSynchronized; }
			}
			
			//
			//  --- Public Methods
			//			
			public ListViewSubItem Add(ListViewItem.ListViewSubItem item) 
			{					
				if (item.ListViewItem==null) item.ListViewItem = owner;
				int nIdx = collection.Add(item);												
				return (ListViewSubItem)collection[nIdx]; 
			} 
			
			
			public ListViewSubItem Add(string text) 
			{				
				ListViewItem.ListViewSubItem item = new ListViewSubItem(owner, text);	 						
				return Add(item);
			}			
			
			public ListViewSubItem Add(string text,Color fColor,Color bColor,Font font) 
			{
				ListViewSubItem item = new ListViewSubItem(owner, text);
				item.ForeColor = fColor;
				item.BackColor = bColor;
				return Add(item);
			}
			
			
			public void AddRange(ListViewItem.ListViewSubItem[] values) 	{
				
				for (int i=0; i<values.Length; i++)	
				{
					if (values[i].ListViewItem==null) values[i].ListViewItem = owner;
					Add(values[i]);										
				}
			}			
			
			public void AddRange(string[] values) {
				
				for (int i=0; i<values.Length; i++)	
					Add(values[i]);										
			}
			
			
			public void AddRange(string[] items,Color fColor, Color bColor,	Font font) {
				
				for (int i=0; i<items.Length; i++)	
					Add(items[i], fColor, bColor, font);														
			}
			
			[MonoTODO]
			public void Clear() 
			{
				//FIXME:
			}			
			
			public bool Contains(ListViewItem.ListViewSubItem subItem) 	{
				
				return collection.Contains(subItem);
			}
			
			public IEnumerator GetEnumerator() 	{				
				return collection.GetEnumerator();
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
				throw new NotImplementedException();
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

	public class ListViewSubItem  
	{
		
		private string  sText;
		private ListViewItem owner = null;
		private	Color backColor = SystemColors.Window;
		private	Color foreColor = SystemColors.WindowText;
		private Font font;
		
			
		internal ListViewItem ListViewItem{				
			get{return owner;}
			set{owner=value;}
		}
			
			
		//
		//  --- Constructor
		//
		public ListViewSubItem(){
			
		}
		
		public ListViewSubItem(ListViewItem item, string str){
			
			owner = item;
			sText = str;
		}
		
		public ListViewSubItem(ListViewItem item, string str, Color foreClr, Color backClr, Font fnt){
			
			owner = item;
			sText = str;
			BackColor = backClr;
			ForeColor = foreClr;			
			font = fnt;
		}
	
		//
		//  --- Public Properties
		//
		public Color BackColor {
			get {return backColor;}
			set {backColor = value;}
		}

		
		public Font Font {
			get {return font;}
			set {font = value;}
		}
		
		public Color ForeColor {
			get {return foreColor;}
			set {foreColor = value;}
		}		
		
		public string Text {
			get {return sText;}
			set {sText=value;}
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


