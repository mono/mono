// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Copyright (c) 2004 Novell, Inc.
//
// Authors:
//	Peter Bartok	pbartok@novell.com
//
//
// $Log: ImageList.cs,v $
// Revision 1.4  2004/08/11 17:43:08  pbartok
// - Removed disposing of the actual images when the list is disposed
//
// Revision 1.3  2004/08/11 13:43:13  pbartok
// - Added missing creation of the collection list
//
// Revision 1.2  2004/08/09 23:12:13  pbartok
// - Fixed several bugs Ravindra pointed out
//
// Revision 1.1  2004/07/15 20:05:28  pbartok
// - Implemented ImageList and ImageList.ImageCollection classes
// - Added ColorDepth enumeration
// - Updated SWF VS.Net project
//
//

// COMPLETE

using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;

namespace System.Windows.Forms {
	public sealed class ImageList : System.ComponentModel.Component {
		#region Local Variables
		private ColorDepth		color_depth;
		private ImageCollection		image_collection;
		private Size			size;
		private Color			transparency_color;
		private Delegate		handler;
		private ImageListStreamer	image_stream;
		#endregion	// Local Variables

		#region Sub-classes
		public sealed class ImageCollection : IList, ICollection, IEnumerable {
			#region	ImageCollection Local Variables
			private ImageList	owner;
			private ArrayList	list;
			#endregion	// ImageCollection Local Variables

			#region ImageCollection Private Constructors
			internal ImageCollection(ImageList owner) {
				this.owner=owner;
				this.list=new ArrayList();
			}
			#endregion	// ImageCollection Private Constructor

			#region	ImageCollection Public Instance Properties
			public int Count {
				get {
					return list.Count;
				}
			}

			public bool Empty {
				get {
					return list.Count==0;
				}
			}

			public bool IsReadOnly {
				get {
					return list.IsReadOnly;
				}
			}

			public Image this[int index] {
				get {
					if (index<0 || index>=list.Count) {
						throw new ArgumentOutOfRangeException("index", index, "ImageCollection does not have that many images");
					}
					return (Image)list[index];
				}

				set {
					if (index<0 || index>=list.Count) {
						throw new ArgumentOutOfRangeException("index", index, "ImageCollection does not have that many images");
					}

					if (value==null) {
						throw new ArgumentOutOfRangeException("value", value, "Image cannot be null");
					}

					list[index]=value;
					// What happens if the bitmap had a previous 'MakeTransparent' done to it?
					((Bitmap)list[index]).MakeTransparent(owner.transparency_color);
				}
			}
			#endregion	// ImageCollection Public Instance Properties

			#region	ImageCollection Private Instance Methods
			private int AddInternal(Image image) {
				int		width;
				int		height;
				PixelFormat	format;

				width=owner.ImageSize.Width;
				height=owner.ImageSize.Height;
				switch(owner.color_depth) {
					case ColorDepth.Depth4Bit:	format=PixelFormat.Format4bppIndexed; break;
					case ColorDepth.Depth8Bit:	format=PixelFormat.Format8bppIndexed; break;
					case ColorDepth.Depth16Bit:	format=PixelFormat.Format16bppRgb555; break;
					case ColorDepth.Depth24Bit:	format=PixelFormat.Format24bppRgb; break;
					case ColorDepth.Depth32Bit:	format=PixelFormat.Format32bppRgb; break;
					default:			format=PixelFormat.Format32bppRgb; break;
				}

				// Check if we can add straight or if we have to resize
				if (image.Width!=width || image.Height!=height || image.PixelFormat!=format) {
					Graphics	g;
					Bitmap		reformatted_image;

					reformatted_image = new Bitmap(width, height, format);
					g=Graphics.FromImage(reformatted_image);

					g.DrawImage(image, new Rectangle(0, 0, width, height), 0, 0, width, height, GraphicsUnit.Pixel);
					g.Dispose();

					return list.Add(reformatted_image);
				} else {
					return list.Add(image);
				}
			}

			internal void Dispose() {
#if dontwantthis
				if (list!=null) {
					for (int i=0; i<list.Count; i++) {
						((Image)list[i]).Dispose();
					}
				}
#endif
			}
			#endregion	// ImageCollection Private Instance Methods

			#region	ImageCollection Public Instance Methods
			public int Add(Image value, Color transparentColor) {
				if (value==null) {
					throw new ArgumentNullException("value", "Cannot add null image");
				}

				((Bitmap)value).MakeTransparent(owner.transparency_color);
				return AddInternal(value);
				
			}

			public void Add(Icon value) {
				Image image;

				image = value.ToBitmap();

				if (value==null || image==null) {
					throw new ArgumentNullException("value", "Cannot add null icon");
				}

				((Bitmap)image).MakeTransparent(owner.transparency_color);
				AddInternal(image);
			}

			public void Add(Image value) {
				if (value==null) {
					throw new ArgumentNullException("value", "Cannot add null image");
				}
				((Bitmap)value).MakeTransparent(owner.transparency_color);
				AddInternal(value);
			}

			public int AddStrip(Image value) {
				int		image_count;
				int		width;
				int		height;
				Bitmap		image;
				Graphics	g;

				if (value==null) {
					throw new ArgumentNullException("value", "Cannot add null images");
				}

				if ((value.Width % owner.ImageSize.Width) != 0) {
					throw new ArgumentException("Strip is not a multiple of the ImageList with", "value");
				}

				// MSDN: The number of images is inferred from the width. A strip is multiple images side-by-side
				width=owner.ImageSize.Width;
				height=owner.ImageSize.Height;
				image_count=value.Width/width;
				for (int i=0; i<image_count; i++) {
					image = new Bitmap(value, width, height);
					g = Graphics.FromImage(image);

					g.DrawImage(value, new Rectangle(0, 0, width, height), i*width, 0, width, height, GraphicsUnit.Pixel);
					AddInternal(image);

					g.Dispose();
					image.Dispose();
				}

				// FIXME - is this right? MSDN says to return the index, but we might have multiple...
				return image_count;
			}

			public void Clear() {
				list.Clear();
			}

			public bool Contains(Image image) {
				return list.Contains(image);
			}

			public IEnumerator GetEnumerator() {
				return list.GetEnumerator();
			}

			public int IndexOf(Image image) {
				return list.IndexOf(image);
			}

			public void Remove(Image image) {
				list.Remove(image);
			}

			public void RemoveAt(int index) {
				if (index<0 || index>=list.Count) {
					throw new ArgumentOutOfRangeException("index", index, "ImageCollection does not have that many images");
				}

				list.RemoveAt(index);
			}
			#endregion	// ImageCollection Public Instance Methods

			#region	ImageCollection Interface Properties
			object IList.this[int index] {
				get {
					if (index<0 || index>=list.Count) {
						throw new ArgumentOutOfRangeException("index", index, "ImageCollection does not have that many images");
					}
					return this[index];
				}

				set {
					if (!(value is Bitmap)) {
						throw new ArgumentException("Object of type Image required", "value");
					}

					this[index]=(Image)value;
				}
			}

			bool IList.IsFixedSize {
				get {
					return false;
				}
			}

			bool IList.IsReadOnly {
				get {
					return list.IsReadOnly;
				}
			}

			bool ICollection.IsSynchronized {
				get {
					return list.IsSynchronized;
				}
			}

			object ICollection.SyncRoot {
				get {
					return list.SyncRoot;
				}
			}
			#endregion	// ImageCollection Interface Properties

			#region	ImageCollection Interface Methods
			int IList.Add(object value) {
				if (value == null) {
					throw new ArgumentNullException("value", "Cannot add null images");
				}

				if (!(value is Bitmap)) {
					throw new ArgumentException("Object of type Image required", "value");
				}

				return list.Add(value);
			}

			bool IList.Contains(object value) {
				if (!(value is Bitmap)) {
					throw new ArgumentException("Object of type Image required", "value");
				}

				return this.Contains((Image) value);
			}

			int IList.IndexOf(object value) {
				if (!(value is Bitmap)) {
					throw new ArgumentException("Object of type Image required", "value");
				}

				return this.IndexOf((Image) value);
			}

			void IList.Insert(int index, object value) {
				if (!(value is Bitmap)) {
					throw new ArgumentException("Object of type Image required", "value");
				}
				list.Insert(index, value);
			}

			void IList.Remove(object value) {
				if (!(value is Bitmap)) {
					throw new ArgumentException("Object of type Image required", "value");
				}
				list.Remove(value);
			}

			void ICollection.CopyTo(Array array, int index) {
				if (list.Count>0) {
					list.CopyTo(array, index);
				}
			}
			#endregion	// ImageCollection Interface Methods
		}
		#endregion	// Sub-classes

		#region Public Constructors
		public ImageList() {
			color_depth = ColorDepth.Depth8Bit;
			transparency_color = Color.Transparent;
			size = new Size(16, 16);
			image_collection = new ImageCollection(this);
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
		public ColorDepth ColorDepth {
			get {
				return this.color_depth;
			}

			set {
				this.color_depth=value;
			}
		}

		[MonoTODO("Determine if we support HBITMAP handles, this would involve XplatUI")]
		public IntPtr Handle {
			get {
				return IntPtr.Zero;
			}
		}

		[MonoTODO("Determine if we support HBITMAP handles, this would involve XplatUI")]
		public bool HandleCreated {
			get {
				return false;
			}
		}

		public ImageCollection Images {
			get {
				return this.image_collection;
			}
		}

		public Size ImageSize {
			get {
				return this.size;
			}

			set {
				if (value.Width<1 || value.Width>256 || value.Height<1 || value.Height>256) {
					throw new ArgumentException("ImageSize width and height must be between 1 and 255", "value");
				}
				this.size=value;
			}
		}

		public ImageListStreamer ImageStream {
			get {
				return image_stream;
			}

			set {
				image_stream = value;
			}
		}

		public Color TransparentColor {
			get {
				return this.transparency_color;
			}

			set {
				this.transparency_color=value;
			}
		}
		#endregion	// Public Instance Properties

		#region Public Instance Methods
		public void Draw(Graphics g, Point pt, int index) {
			this.Draw(g, pt.X, pt.Y, this.size.Width, this.size.Height, index);
		}

		public void Draw(Graphics g, int x, int y, int width, int height, int index) {
		}

		public override string ToString() {
			return "ImageList Size "+this.size.Width.ToString()+"x"+this.size.Height.ToString()+", Depth "+this.color_depth.ToString()+", Transparency color "+this.transparency_color.ToString();
		}
		#endregion	// Public Instance Methods

		#region	Protected Instance Methods
		protected override void Dispose(bool disposing) {
			if (image_collection!=null) {
				image_collection.Dispose();
			}
		}

		#endregion	// Protected Instance Methods
	}
}
