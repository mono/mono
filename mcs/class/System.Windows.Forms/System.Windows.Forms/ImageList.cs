//
// System.Windows.Forms.ImageList.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	Dennis Hayes (dennish@raytek.com)
//   Aleksey Ryabchuk (ryabchuk@yahoo.com)
//
// (C) 2002/3 Ximian, Inc
//
using System.ComponentModel;
using System.Drawing;
using System.Collections;
using System.Runtime.Serialization;

namespace System.Windows.Forms {

	// <summary>
	//
	// </summary>
    public sealed class ImageList : Component {
		ColorDepth colorDepth;
		Size       size;
		Color      transparentColor;
		IntPtr     handle;
		ImageCollection images;

    		ImageListStreamer imageListStreamer;

		[MonoTODO]
		public ImageList() {
			colorDepth = ColorDepth.Depth4Bit;
			size = new Size ( 16, 16 );
			transparentColor = Color.Transparent;
		}

		[MonoTODO]
		public ImageList(IContainer cont) : this() {
			cont.Add ( this );
		}


		[MonoTODO]
		public ColorDepth ColorDepth {
			get {	return colorDepth; }
			set {
				if ( !Enum.IsDefined ( typeof( ColorDepth ), value ) )
					throw new InvalidEnumArgumentException( "ColorDepth",
						(int)value,
						typeof( ColorDepth ) );

				if ( colorDepth != value ) {
					colorDepth = value;
					if ( HandleCreated )
						recreateHandle ( );
				}
			}
		}

		[MonoTODO]
		public IntPtr Handle {
			get {
				if ( !HandleCreated )
					createHandle ( );
				return handle;
			}
		}

		[MonoTODO]
		public bool HandleCreated {
			get {	return handle != IntPtr.Zero;	}
		}

		[MonoTODO]
		public ImageList.ImageCollection Images {
			get {
				if ( images == null )
					images = new ImageCollection ( this );
				return images;
			}
		}

		[MonoTODO]
		public Size ImageSize {
			get {   return size;  }
			set {
				if ( value.IsEmpty || value.Width  <= 0 || value.Height <= 0 || 
					value.Width  > 256 || value.Height > 256 )
					throw new ArgumentException( ); // FIXME: message

				if ( size != value ) {
					size = value;
					if ( HandleCreated )
						recreateHandle ( );
					
				}
			}
		}

		[MonoTODO]
		public ImageListStreamer ImageStream {
			get {
				return imageListStreamer;
			}
			set {
				imageListStreamer = value;
				destroyHandle ( );
				handle = imageListStreamer.Handle;
			}
		}

		[MonoTODO]
		public Color TransparentColor {
			get {	return transparentColor; }
			set {	transparentColor = value;}
		}

		//
		//  --- Public Methods
		//

		[MonoTODO]
		public void Draw(Graphics g, Point pt, int index)
		{
			//FIXME:
		}

		[MonoTODO]
		public void Draw(Graphics g, int x, int y, int index)
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

		private void createHandle ( )
		{
			ImageListStreamer.initCommonControlsLibrary( );
		}

		private void recreateHandle ( ) {
		}

		private void destroyHandle ( )
		{
			if ( HandleCreated )
				Win32.ImageList_Destroy ( handle );
		}

		//
		// System.Windows.Forms.ImageList.ImageCollection.cs
		//
		// Author:
		//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
		//   Aleksey Ryabchuk (ryabchuk@yahoo.com)
		//// (C) 2002 Ximian, Inc
		////
		// <summary>
		//
		// </summary>

		public sealed class ImageCollection : IList, ICollection, IEnumerable {
			private ArrayList list;
			private ImageList owner;


		public ImageCollection( ImageList owner )  {
			list = new ArrayList();
			this.owner = owner;
		}

		[MonoTODO]
		public int Count {
			get { return list.Count; }
		}

		[MonoTODO]
		public bool Empty {
			get { return list.Count == 0; }
		}

		[MonoTODO]
		public bool IsReadOnly {
			get { return list.IsReadOnly; }
		}

		[MonoTODO]
		public Image this[int index] {
			get {	return ( Image ) list[ index ];	}
			set {	list[ index ] = value;	}
		}

		[MonoTODO]
		public void Add(Icon icon) {
			if ( icon == null )
				throw new ArgumentNullException("value");
			
			//list.Add( Bitmap.FromHicon ( icon.Handle ) );
		}

		[MonoTODO]
		public void Add(Image img) {
			if ( img == null )
				throw new ArgumentNullException("value");

			list.Add( img );
		}

		[MonoTODO]
		public int Add(Image img, Color col) {
			if ( img == null )
				throw new ArgumentNullException("value");

			return list.Add( img );
		}

		[MonoTODO]
		public int AddStrip( Image value ) {
			return -1;
		}

		[MonoTODO]
		public void Clear() {
			list.Clear ( );
		}

		[MonoTODO]
		public bool Contains(Image image) {
			return list.Contains( image );
		}

		[MonoTODO]
		public IEnumerator GetEnumerator() {
			return list.GetEnumerator();
		}

		[MonoTODO]
		public int IndexOf(Image image)	{
			return list.IndexOf( image );
		}

		[MonoTODO]
		public void Remove(Image image)	{
			list.Remove( image );
		}

		[MonoTODO]
		public void RemoveAt(int index)	{
			if (index < 0 || index > Count )
				throw new ArgumentOutOfRangeException( "index" );

			list.RemoveAt( index );
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
				get{	return list.IsReadOnly; }
			}
			bool IList.IsFixedSize{
				get{	return list.IsFixedSize; }
			}

			object IList.this[int index]{
				get { return this[index]; }
				set { this[index]= (Image) value; }
			}
		
			[MonoTODO]
			void IList.Clear(){
				Clear ( );
			}
		
			[MonoTODO]
			int IList.Add( object value ){
				if (!(value is Image))
					throw new ArgumentException();//FIXME: message

				Add( (Image) value );
				return Count;
			}

			[MonoTODO]
			bool IList.Contains( object value ){
				if (!(value is Image))
					return false;
				return Contains( (Image) value );
			}

			[MonoTODO]
			int IList.IndexOf( object value ){
				if ( !( value is Image))
					return -1;
				return IndexOf( (Image) value );
			}

			[MonoTODO]
			void IList.Insert( int index, object value ){
				if ( !( value is Image ) )
					throw new ArgumentException();//FIXME: message
				
				list.Insert ( index, value );
			}

			[MonoTODO]
			void IList.Remove( object value ){
				if ( !(value is Image) )
					throw new ArgumentException(); //FIXME: message

				Remove( (Image) value);
			}

			[MonoTODO]
			void IList.RemoveAt( int index ){
				RemoveAt ( index );
			}
			// End of IList interface

			/// <summary>
			/// ICollection Interface implmentation.
			/// </summary>
			int ICollection.Count{
				get{	return Count;  }
			}
			bool ICollection.IsSynchronized{
				get{	return list.IsSynchronized; }
			}
			object ICollection.SyncRoot{
				get{	return list.SyncRoot;	}
			}
			void ICollection.CopyTo(Array array, int index){
				list.CopyTo ( array, index );
			}
			// End Of ICollection

		}// End of Subclass

	 }//End of class
}
