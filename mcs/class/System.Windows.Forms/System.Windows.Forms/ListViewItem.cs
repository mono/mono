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

 
namespace System.Windows.Forms 
{
	// <summary>
	// </summary>
	[Serializable]
	public class ListViewItem :  ICloneable, ISerializable 
	{		
		private ListView container = null;
		private string  m_sText;
		private	ListViewSubItemCollection	m_colSubItem = null;
		private int index;
		private bool bSelected = false;
		private bool useItemStyleForSubItems = true;
		private bool bChecked = false;
		private bool bFocused = false;
		private	Color backColor = SystemColors.Window;
		private	Color foreColor = SystemColors.WindowText;
		private	System.Drawing.Rectangle m_Bounds;

		//
		//  --- Constructor
		//					
		protected void CommonConstructor(){
			m_colSubItem = new 	ListViewSubItemCollection(this);
		}
		
		public ListViewItem(){			
			CommonConstructor();			
		}
		
		public ListViewItem(string str)	{
			Console.WriteLine("ListViewItem.ListViewItem str");					
			CommonConstructor();			
			m_sText = str;
		}
		
		public ListViewItem(string[] strings){	// An array of strings that represent the subitems of the new item.
		
			Console.WriteLine("ListView.ListView strings");				
			CommonConstructor();
			
			if (strings.Length>0)			
				m_sText = strings[0];
				
			if (strings.Length>1)			
			{
				for (int i=1; i<strings.Length; i++)
					m_colSubItem.Add(strings[i]);		
			}
		}

		
		public ListViewItem(ListViewItem.ListViewSubItem[] subItems){
			
			CommonConstructor();
			for (int i=0; i<subItems.Length; i++)
					m_colSubItem.Add(subItems[i]);		
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

		[MonoTODO]
		public Rectangle Bounds 
		{
			get 
			{
				// TODO: Windows Win32 api call to calculate the bound
				return m_Bounds;
			}
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
			get {
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
		
		public ListViewSubItemCollection SubItems 
		{
			get 
			{
				return m_colSubItem;
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
		
		public string Text 	{
			get { return m_sText;}
			set { m_sText = value;}		
			
		}
		
		public bool UseItemStyleForSubItems {
			get { return useItemStyleForSubItems;}
			set { useItemStyleForSubItems = value;}		
		}
		
		//
		//  --- Private Methods
		//		
		public ListView Container {			
			set{container=value;}
		}				
		
		public int CtrlIndex{					
			set{index=value;}
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
			
			private ArrayList m_collection = new ArrayList();
			private ListViewItem m_owner = null;
			
			
			//
			//  --- Constructor
			//		
			public ListViewSubItemCollection(ListViewItem owner) {
				m_owner = owner;
			}			
			
			//
			//  --- Public Properties
			//			
			public int Count {
				get { return m_collection.Count; }
			}			
			
			public bool IsReadOnly 
			{
				get { return m_collection.IsReadOnly; }
			}		
			
			public ListViewSubItem this[int index] 
			{
				get { return (ListViewSubItem) m_collection[index];}
				set { m_collection[index] = value;}				
			}	
			
			/// --- ICollection properties ---
			bool IList.IsFixedSize 
			{
				get { return m_collection.IsFixedSize; }
			}
			
			object IList.this[int index] 
			{
				get { return m_collection[index]; }
				set { m_collection[index] = value; }
			}
	
			object ICollection.SyncRoot 
			{
				get { return m_collection.SyncRoot; }
			}
	
			bool ICollection.IsSynchronized 
			{
				get { return m_collection.IsSynchronized; }
			}
			
			//
			//  --- Public Methods
			//			
			public ListViewSubItem Add(ListViewItem.ListViewSubItem item) 
			{			
				
				Console.WriteLine("ListViewSubItem.Add " +  item.Text);											
				int nIdx = m_collection.Add(item);				
								
				return (ListViewSubItem)m_collection[nIdx]; // TODO: Check this in .Net?
			} 
			
			
			public ListViewSubItem Add(string text) 
			{
				
				Console.WriteLine("ListViewSubItem.Add " +  text);											
				ListViewItem.ListViewSubItem item = new ListViewSubItem(m_owner, text);	 
						
				int nIdx = m_collection.Add(item); 
				return (ListViewSubItem)m_collection[nIdx]; 
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
		
		
		public ListViewItem ListViewItem{
			get{return owner;}
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


