//
// System.Windows.Forms.ImageList.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	Dennis Hayes (dennish@raytek.com)
//
// (C) 2002/3 Ximian, Inc
//
using System.ComponentModel;
using System.Drawing;
using System.Collections;
namespace System.Windows.Forms {

	// <summary>
	//
	// </summary>
    public sealed class ImageList : Component {
		internal ColorDepth colorDepth;
    	ImageListStreamer ImageListStreamer_;
		//
		//  --- Constructor
		//

		[MonoTODO]
		public ImageList()
		{
			colorDepth = ColorDepth.Depth8Bit;
		}

		[MonoTODO]
		public ImageList(IContainer cont) : this()
		{
			ImageListStreamer_ = new ImageListStreamer();		
		}

		//
		//  --- Public Properties
		//

		[MonoTODO]
		public ColorDepth ColorDepth {
			get {
				return colorDepth;
			}
			set {
				colorDepth = value;
			}
		}

		[MonoTODO]
		public IntPtr Handle {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public bool HandleCreated {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public ImageList.ImageCollection Images {
			get {
				throw new NotImplementedException ();
			}
		}

		internal Size imageSize; //FIXME: just to get it to run
		[MonoTODO]
		public Size ImageSize {
			get {
				return imageSize;
			}
			set {
				imageSize = value;
			}
		}

		[MonoTODO]
		public ImageListStreamer ImageStream {
			get {
				//FIXME:
				return ImageListStreamer_;
			}
			set {
				//FIXME:
			}
		}

		internal Color transparentColor; //FIXME: just to get it to run
		[MonoTODO]
		public Color TransparentColor {
			get {
				return transparentColor;
			}
			set {
				transparentColor = value;
			}
		}

		//
		//  --- Public Methods
		//

		[MonoTODO]
		public void Draw(Graphics g, Point pt, int n)
		{
			//FIXME:
		}

		[MonoTODO]
		public void Draw(Graphics g, int n1, int n2, int n3)
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
		//  --- Public Events
		//
		public event EventHandler RecreateHandle;

		//
		// System.Windows.Forms.ImageList.ImageCollection.cs
		//
		// Author:
		//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
		//
		//// (C) 2002 Ximian, Inc
		////
		// <summary>
		//
		// </summary>

		public sealed class ImageCollection : IList, ICollection, IEnumerable {


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
		public bool Empty {
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
		public Image this[int index] {
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
		public void Add(Icon icon)
		{
			//FIXME:
		}

		[MonoTODO]
		public void Add(Image img)
		{
			//FIXME:
		}

		[MonoTODO]
		public int Add(Image img, Color col)
		{
			throw new NotImplementedException ();			
		}

		[MonoTODO]
		public int AddStrip(Image value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Clear()
		{
			//FIXME:
		}

		[MonoTODO]
		public bool Contains(Image image)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public IEnumerator GetEnumerator()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int IndexOf(Image image)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Remove(Image image)
		{
			//FIXME:
		}

		[MonoTODO]
		public void RemoveAt(int index)
		{
			//FIXME:
		}

		[MonoTODO]
		public override string ToString()
		{
			//FIXME:
			return base.ToString();
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
					//FIXME:
				}
			}
		
			[MonoTODO]
			void IList.Clear(){
				//FIXME:
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
				//FIXME:
			}

			[MonoTODO]
			void IList.Remove( object value){
				//FIXME:
			}

			[MonoTODO]
			void IList.RemoveAt( int index){
				//FIXME:
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

		}// End of Subclass

	 }//End of class
}
