//
// System.Windows.Forms.ImageList.cs
//
// Authors:
//   Peter Bartok <pbartok@novell.com>
//   Kornél Pál <http://www.kornelpal.hu/>
//
// Copyright (C) 2004-2005 Novell, Inc.
// Copyright (C) 2005 Kornél Pál
//

//
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

// COMPLETE

//
// Differences between MS.NET ImageList and this implementation:
//
// This is a fully managed image list implementation.
//
// Images are stored as Format32bppArgb internally but ColorDepth is applied
// to the colors of images. Images[index] returns a Format32bppArgb copy of
// the image so this difference is only internal.
//
// MS.NET has no alpha channel support (except for icons in 32-bit mode with
// comctl32.dll version 6.0) but this implementation has full alpha channel
// support in 32-bit mode.
//
// Handle should be an HIMAGELIST returned by ImageList_Create. This
// implementation uses (IntPtr)(-1) that is a non-zero but invalid handle.
//
// MS.NET creates handle when images are accessed. Add methods are caching the
// original images without modification. This implementation adds images in
// Add methods so handle is created in Add methods.
//
// MS.NET 1.x shares the same HIMAGELIST between ImageLists that were
// initialized from the same ImageListStreamer and doesn't update ImageSize
// and ColorDepth that are treated as bugs.
//

using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace System.Windows.Forms
{
	[DefaultProperty("Images")]
	[Designer("System.Windows.Forms.Design.ImageListDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	[ToolboxItemFilter("System.Windows.Forms", ToolboxItemFilterType.Allow)]
	[TypeConverter("System.Windows.Forms.ImageListConverter, " + Consts.AssemblySystem_Windows_Forms)]
	public sealed class ImageList : System.ComponentModel.Component
	{
		#region Private Fields
		private readonly ImageCollection images = new ImageCollection();
		#endregion // Private Fields

		#region Sub-classes
		[Editor("System.Windows.Forms.Design.ImageCollectionEditor, " + Consts.AssemblySystem_Design, typeof(System.Drawing.Design.UITypeEditor))]
		public sealed class ImageCollection : IList, ICollection, IEnumerable
		{
			private const int AlphaMask = unchecked((int)0xFF000000);

			[StructLayout(LayoutKind.Explicit)]
			private struct ArgbColor
			{
				[FieldOffset(0)]
				internal int argb;
				[FieldOffset(0)]
				internal byte blue;
				[FieldOffset(1)]
				internal byte green;
				[FieldOffset(2)]
				internal byte red;
				[FieldOffset(3)]
				internal byte alpha;
			}

			private
#if NET_2_0
			static
#else
			sealed
#endif
			class IndexedColorDepths
			{
#if !NET_2_0
				private IndexedColorDepths()
				{
				}
#endif
				internal static readonly ArgbColor[] Palette4Bit;
				internal static readonly ArgbColor[] Palette8Bit;
				private static readonly int[] squares;

				static IndexedColorDepths()
				{
					Color[] palette;
					Bitmap bitmap;
					int index;
					int count;

					bitmap = new Bitmap(1, 1, PixelFormat.Format4bppIndexed);
					palette = bitmap.Palette.Entries;
					bitmap.Dispose();

					Palette4Bit = new ArgbColor[count = palette.Length];
					for (index = 0; index < count; index++)
						Palette4Bit[index].argb = palette[index].ToArgb();

					bitmap = new Bitmap(1, 1, PixelFormat.Format8bppIndexed);
					palette = bitmap.Palette.Entries;
					bitmap.Dispose();

					Palette8Bit = new ArgbColor[count = palette.Length];
					for (index = 0; index < count; index++)
						Palette8Bit[index].argb = palette[index].ToArgb();

					squares = new int[511];
					for (index = 0; index < 256; index++)
						squares[255 + index] = squares[255 - index] = index * index;
				}

				internal static int GetNearestColor(ArgbColor[] palette, int color)
				{
					int index;
					int count;
					int red;
					int green;
					int blue;
					int nearestColor;
					int minDistance;
					int distance;

					count = palette.Length;
					for (index = 0; index < count; index++)
						if (palette[index].argb == color)
							return color;

					red = unchecked((int)(unchecked((uint)color) >> 16) & 0xFF);
					green = unchecked((int)(unchecked((uint)color) >> 8) & 0xFF);
					blue = color & 0xFF;
					nearestColor = AlphaMask;
					minDistance = int.MaxValue;

					for (index = 0; index < count; index++)
						if ((distance = squares[255 + palette[index].red - red] + squares[255 + palette[index].green - green] + squares[255 + palette[index].blue - blue]) < minDistance) {
							nearestColor = palette[index].argb;
							minDistance = distance;
						}

					return nearestColor;
				}

			}

			#region ImageCollection Private Fields
			private ColorDepth colorDepth = ColorDepth.Depth8Bit;
			private Color transparentColor = Color.Transparent;
			private Size imageSize = new Size(16, 16);
			private bool handleCreated;
			private EventHandler recreateHandle;
			private readonly ArrayList list = new ArrayList();
			#endregion // ImageCollection Private Fields

			#region ImageCollection Internal Constructors
			// For use in ImageList
			internal ImageCollection()
			{
			}
			#endregion // ImageCollection Internal Constructor

			#region ImageCollection Internal Instance Properties
			// For use in ImageList
			internal ColorDepth ColorDepth {
				get {
					return this.colorDepth;
				}

				set {
					if (!Enum.IsDefined(typeof(ColorDepth), value))
						throw new InvalidEnumArgumentException("value", (int)value, typeof(ColorDepth));

					if (this.colorDepth != value) {
						this.colorDepth = value;
						if (handleCreated) {
							list.Clear();
							OnRecreateHandle();
						}
					}
				}
			}

			// For use in ImageList
			internal IntPtr Handle {
				get {
					this.handleCreated = true;
					return (IntPtr)(-1);
				}
			}

			// For use in ImageList
			internal bool HandleCreated {
				get {
					return this.handleCreated;
				}
			}

			// For use in ImageList
			internal Size ImageSize {
				get {
					return this.imageSize;
				}

				set {
					if (value.Width < 1 || value.Width > 256 || value.Height < 1 || value.Height > 256)
						throw new ArgumentException("ImageSize.Width and Height must be between 1 and 256", "value");

					if (this.imageSize != value) {
						this.imageSize = value;
						if (handleCreated) {
							list.Clear();
							OnRecreateHandle();
						}
					}
				}
			}

			// For use in ImageList
			internal ImageListStreamer ImageStream {
				get {
					return list.Count == 0 ? null : new ImageListStreamer(this);
				}

				set {
					int index;
					Image[] streamImages;

					if (value == null) {
#if NET_2_0
						list.Clear();
#endif
					}
					else if ((streamImages = value.Images) != null) {
						list.Clear();

						for (index = 0; index < streamImages.Length; index++)
							list.Add((Image)streamImages[index].Clone());

						this.imageSize = value.ImageSize;
						this.colorDepth = value.ColorDepth;
#if NET_2_0
						// Event is raised even when handle was not created yet.
						OnRecreateHandle();
#endif
					}
				}
			}

			// For use in ImageList
			internal Color TransparentColor {
				get {
					return this.transparentColor;
				}

				set {
					this.transparentColor = value;
				}
			}
			#endregion // ImageCollection Internal Instance Properties

			#region ImageCollection Public Instance Properties
			[Browsable(false)]
			public int Count {
				get {
					return list.Count;
				}
			}

			public bool Empty {
				get {
					return list.Count == 0;
				}
			}

			public bool IsReadOnly {
				get {
					return false;
				}
			}

			[Browsable(false)]
			[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
			public Image this[int index] {
				get {
					return (Image)GetImage(index).Clone();
				}

				set {
					if (index < 0 || index >= list.Count)
						throw new ArgumentOutOfRangeException("index");

					list[index] = CreateImage(value, this.transparentColor);
				}
			}
			#endregion // ImageCollection Public Instance Properties

			#region ImageCollection Private Instance Methods
			private Image CreateImage(Image value, Color transparentColor)
			{
				int width;
				int height;
				Bitmap bitmap;
				Graphics graphics;
				ImageAttributes imageAttributes;

				if (value == null)
					throw new ArgumentNullException("value");

				if (!(value is Bitmap))
					throw new ArgumentException("Image must be a Bitmap.");

				if (transparentColor.A == 0)
					imageAttributes = null;
				else {
					imageAttributes = new ImageAttributes();
					imageAttributes.SetColorKey(transparentColor, transparentColor);
				}

				bitmap = new Bitmap((width = this.imageSize.Width), (height = this.imageSize.Height), PixelFormat.Format32bppArgb);
				graphics = Graphics.FromImage(bitmap);
				graphics.DrawImage(value, new Rectangle(0, 0, width, height), 0, 0, value.Width, value.Height, GraphicsUnit.Pixel, imageAttributes);
				graphics.Dispose();

				return ReduceColorDepth(bitmap);
			}

			private void OnRecreateHandle()
			{
				if (recreateHandle != null)
					recreateHandle(this, EventArgs.Empty);
			}

			private unsafe Image ReduceColorDepth(Bitmap bitmap)
			{
				byte* pixelPtr;
				byte* lineEndPtr;
				byte* linePtr;
				int line;
				int pixel;
				int height;
				int widthBytes;
				int stride;
				BitmapData bitmapData;
				ArgbColor[] palette;

				if (this.colorDepth < ColorDepth.Depth32Bit) {
					bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
					try {
						linePtr = (byte*)bitmapData.Scan0;
						height = bitmapData.Height;
						widthBytes = bitmapData.Width << 2;
						stride = bitmapData.Stride;

						if (this.colorDepth < ColorDepth.Depth16Bit) {
							palette = (this.colorDepth < ColorDepth.Depth8Bit) ? IndexedColorDepths.Palette4Bit : IndexedColorDepths.Palette8Bit;

							for (line = 0; line < height; line++) {
								lineEndPtr = linePtr + widthBytes;
								for (pixelPtr = linePtr; pixelPtr < lineEndPtr; pixelPtr += 4)
									*(int*)pixelPtr = (((pixel = *(int*)pixelPtr) & AlphaMask) == 0) ? 0x00000000 : IndexedColorDepths.GetNearestColor(palette, pixel | AlphaMask);
								linePtr += stride;
							}
						}
						else if (this.colorDepth < ColorDepth.Depth24Bit) {
							for (line = 0; line < height; line++) {
								lineEndPtr = linePtr + widthBytes;
								for (pixelPtr = linePtr; pixelPtr < lineEndPtr; pixelPtr += 4)
									*(int*)pixelPtr = (((pixel = *(int*)pixelPtr) & AlphaMask) == 0) ? 0x00000000 : (pixel & 0x00F8F8F8) | AlphaMask;
								linePtr += stride;
							}
						}
						else {
							for (line = 0; line < height; line++) {
								lineEndPtr = linePtr + widthBytes;
								for (pixelPtr = linePtr; pixelPtr < lineEndPtr; pixelPtr += 4)
									*(int*)pixelPtr = (((pixel = *(int*)pixelPtr) & AlphaMask) == 0) ? 0x00000000 : pixel | AlphaMask;
								linePtr += stride;
							}
						}
					}
					finally {
						bitmap.UnlockBits(bitmapData);
					}
				}

				return bitmap;
			}
			#endregion // ImageCollection Private Instance Methods

			#region ImageCollection Internal Instance Methods
			// For use in ImageList
			internal Image GetImage(int index)
			{
				if (index < 0 || index >= list.Count)
					throw new ArgumentOutOfRangeException("index");

				return (Image)list[index];
			}

			// For use in ImageListStreamer
			internal Image[] ToArray()
			{
				Image[] images = new Image[list.Count];

				list.CopyTo(images);
				return images;
			}
			#endregion // ImageCollection Internal Instance Methods

			#region ImageCollection Public Instance Methods
			public void Add(Icon value)
			{
				Bitmap bitmap;

				if (value == null)
					throw new ArgumentNullException("value");

				bitmap = value.ToBitmap();
				Add(bitmap, Color.Transparent);
				bitmap.Dispose();
			}

			public void Add(Image value)
			{
				Add(value, this.transparentColor);
			}

			public int Add(Image value, Color transparentColor)
			{
				this.handleCreated = true;
				return list.Add(CreateImage(value, transparentColor));
			}

			public int AddStrip(Image value)
			{
				int imageX;
				int imageWidth;
				int width;
				int height;
				int index;
				Bitmap bitmap;
				Graphics graphics;
				Rectangle imageRect;
				ImageAttributes imageAttributes;

				if (value == null)
					throw new ArgumentNullException("value");

				if ((imageWidth = value.Width) == 0 || (imageWidth % (width = this.imageSize.Width)) != 0)
					throw new ArgumentException("Width of image strip must be a positive multiple of ImageSize.Width.", "value");

				if (value.Height != (height = this.imageSize.Height))
					throw new ArgumentException("Height of image strip must be equal to ImageSize.Height.", "value");

				if (!(value is Bitmap))
					throw new ArgumentException("Image must be a Bitmap.");

				this.handleCreated = true;

				imageRect = new Rectangle(0, 0, width, height);
				if (this.transparentColor.A == 0)
					imageAttributes = null;
				else {
					imageAttributes = new ImageAttributes();
					imageAttributes.SetColorKey(this.transparentColor, this.transparentColor);
				}

				index = list.Count;
				for (imageX = 0; imageX < imageWidth; imageX += width) {
					bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
					graphics = Graphics.FromImage(bitmap);
					graphics.DrawImage(value, imageRect, imageX, 0, width, height, GraphicsUnit.Pixel, imageAttributes);
					graphics.Dispose();

					list.Add(ReduceColorDepth(bitmap));
				}

				return index;
			}

			public void Clear()
			{
				list.Clear();
			}

			public bool Contains(Image image)
			{
				throw new NotSupportedException();
			}


			public IEnumerator GetEnumerator()
			{
				Image[] images = new Image[list.Count];
				int index;

				for (index = 0; index < images.Length; index++)
					images[index] = (Image)((Image)list[index]).Clone();

				return images.GetEnumerator();
			}

			public int IndexOf(Image image)
			{
				throw new NotSupportedException();
			}

			public void Remove(Image image)
			{
				throw new NotSupportedException();
			}

			public void RemoveAt(int index)
			{
				if (index < 0 || index >= list.Count)
					throw new ArgumentOutOfRangeException("index");

				list.RemoveAt(index);
			}
			#endregion // ImageCollection Public Instance Methods

			#region ImageCollection Interface Properties
			object IList.this[int index] {
				get {
					return this[index];
				}

				set {
					if (!(value is Image))
						throw new ArgumentException("value");

					this[index] = (Image)value;
				}
			}

			bool IList.IsFixedSize {
				get {
					return false;
				}
			}

			bool ICollection.IsSynchronized {
				get {
					return false;
				}
			}

			object ICollection.SyncRoot {
				get {
					return this;
				}
			}
			#endregion // ImageCollection Interface Properties

			#region ImageCollection Interface Methods
			int IList.Add(object value)
			{
				int index;

				if (!(value is Image))
					throw new ArgumentException("value");

				index = this.Count;
				this.Add((Image)value);
				return index;
			}

			bool IList.Contains(object value)
			{
				return (value is Image) ? this.Contains((Image)value) : false;
			}

			int IList.IndexOf(object value)
			{
				return (value is Image) ? this.IndexOf((Image)value) : -1;
			}

			void IList.Insert(int index, object value)
			{
				throw new NotSupportedException();
			}

			void IList.Remove(object value)
			{
				if (value is Image)
					this.Remove((Image)value);
			}

			void ICollection.CopyTo(Array array, int index)
			{
				int imageIndex;

				for (imageIndex = 0; imageIndex < this.Count; imageIndex++)
					array.SetValue(this[index], index++);
			}
			#endregion // ImageCollection Interface Methods

			#region ImageCollection Events
			// For use in ImageList
			internal event EventHandler RecreateHandle {
				add {
					recreateHandle += value;
				}

				remove {
					recreateHandle -= value;
				}
			}
			#endregion // ImageCollection Events
		}
		#endregion // Sub-classes

		#region Public Constructors
		public ImageList()
		{
		}

		public ImageList(System.ComponentModel.IContainer container)
		{
			container.Add(this);
		}
		#endregion // Public Constructors

		#region Public Instance Properties
		[DefaultValue(ColorDepth.Depth8Bit)]
		public ColorDepth ColorDepth {
			get {
				return images.ColorDepth;
			}

			set {
				images.ColorDepth = value;
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public IntPtr Handle {
			get {
				return images.Handle;
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool HandleCreated {
			get {
				return images.HandleCreated;
			}
		}

		[DefaultValue(null)]
		[MergableProperty(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ImageCollection Images {
			get {
				return this.images;
			}
		}

		[Localizable(true)]
		public Size ImageSize {
			get {
				return images.ImageSize;
			}

			set {
				images.ImageSize = value;
			}
		}

		[Browsable(false)]
		[DefaultValue(null)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public ImageListStreamer ImageStream {
			get {
				return images.ImageStream;
			}

			set {
				images.ImageStream = value;
			}
		}

		public Color TransparentColor {
			get {
				return images.TransparentColor;
			}

			set {
				images.TransparentColor = value;
			}
		}
		#endregion // Public Instance Properties

		#region Public Instance Methods
		public void Draw(Graphics g, Point pt, int index)
		{
			this.Draw(g, pt.X, pt.Y, index);
		}

		public void Draw(Graphics g, int x, int y, int index)
		{
			g.DrawImage(images.GetImage(index), x, y);
		}

		public void Draw(Graphics g, int x, int y, int width, int height, int index)
		{
			g.DrawImage(images.GetImage(index), x, y, width, height);
		}

		public override string ToString()
		{
			return base.ToString() + " Images.Count: " + images.Count.ToString() + ", ImageSize: " + images.ImageSize.ToString();
		}
		#endregion // Public Instance Methods

		#region Protected Instance Methods
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}
		#endregion // Protected Instance Methods

		#region Events
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public event EventHandler RecreateHandle {
			add {
				images.RecreateHandle += value;
			}

			remove {
				images.RecreateHandle -= value;
			}
		}
		#endregion // Events
	}
}
