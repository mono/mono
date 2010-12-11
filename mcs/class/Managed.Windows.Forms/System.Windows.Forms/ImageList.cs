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
// MS.NET destroys handles using the garbage collector this implementation
// does the same with Image objects stored in an ArrayList.
//
// MS.NET 1.x shares the same HIMAGELIST between ImageLists that were
// initialized from the same ImageListStreamer and doesn't update ImageSize
// and ColorDepth that are treated as bugs and MS.NET 2.0 behavior is
// implemented.
//
// MS.NET 2.0 does not clear keys when handle is destroyed that is treated as
// a bug.
//

using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Imaging;
using System.Globalization;
using System.Runtime.InteropServices;

namespace System.Windows.Forms
{
	[DefaultProperty("Images")]
	[Designer("System.Windows.Forms.Design.ImageListDesigner, " + Consts.AssemblySystem_Design)]
#if NET_2_0
	[DesignerSerializer("System.Windows.Forms.Design.ImageListCodeDomSerializer, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.Serialization.CodeDomSerializer, " + Consts.AssemblySystem_Design)]
#endif
	[ToolboxItemFilter("System.Windows.Forms")]
	[TypeConverter(typeof(ImageListConverter))]
	public sealed class ImageList : System.ComponentModel.Component
	{
		#region Private Fields
		private const ColorDepth DefaultColorDepth = ColorDepth.Depth8Bit;
		private static readonly Size DefaultImageSize = new Size(16, 16);
		private static readonly Color DefaultTransparentColor = Color.Transparent;

#if NET_2_0
		private object tag;
#endif
		private readonly ImageCollection images;
		#endregion // Private Fields

		#region Sub-classes
		[Editor("System.Windows.Forms.Design.ImageCollectionEditor, " + Consts.AssemblySystem_Design, typeof(UITypeEditor))]
		public sealed class ImageCollection : IList, ICollection, IEnumerable
		{
			private const int AlphaMask = unchecked((int)0xFF000000);

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
				internal static readonly ColorPalette Palette4Bit;
				internal static readonly ColorPalette Palette8Bit;
				private static readonly int[] squares;

				static IndexedColorDepths()
				{
					Bitmap bitmap;
					int index;

					bitmap = new Bitmap(1, 1, PixelFormat.Format4bppIndexed);
					Palette4Bit = bitmap.Palette;
					bitmap.Dispose();

					bitmap = new Bitmap(1, 1, PixelFormat.Format8bppIndexed);
					Palette8Bit = bitmap.Palette;
					bitmap.Dispose();

					squares = new int[511];
					for (index = 0; index < 256; index++)
						squares[255 + index] = squares[255 - index] = index * index;
				}

				internal static int GetNearestColor(Color[] palette, int color)
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
						if (palette[index].ToArgb() == color)
							return color;

					red = unchecked((int)(unchecked((uint)color) >> 16) & 0xFF);
					green = unchecked((int)(unchecked((uint)color) >> 8) & 0xFF);
					blue = color & 0xFF;
					nearestColor = AlphaMask;
					minDistance = int.MaxValue;

					for (index = 0; index < count; index++)
						if ((distance = squares[255 + palette[index].R - red] + squares[255 + palette[index].G - green] + squares[255 + palette[index].B - blue]) < minDistance) {
							nearestColor = palette[index].ToArgb();
							minDistance = distance;
						}

					return nearestColor;
				}
			}

			[Flags()]
			private enum ItemFlags
			{
				None = 0,
				UseTransparentColor = 1,
				ImageStrip = 2
			}

			private sealed class ImageListItem
			{
				internal readonly object Image;
				internal readonly ItemFlags Flags;
				internal readonly Color TransparentColor;
				internal readonly int ImageCount = 1;

				internal ImageListItem(Icon value)
				{
					if (value == null)
						throw new ArgumentNullException("value");

					// Icons are cloned.
					this.Image = (Icon)value.Clone();
				}

				internal ImageListItem(Image value)
				{
					if (value == null)
						throw new ArgumentNullException("value");

					if (!(value is Bitmap))
						throw new ArgumentException("Image must be a Bitmap.");

					// Images are not cloned.
					this.Image = value;
				}

				internal ImageListItem(Image value, Color transparentColor) : this(value)
				{
					this.Flags = ItemFlags.UseTransparentColor;
					this.TransparentColor = transparentColor;
				}

				internal ImageListItem(Image value, int imageCount) : this(value)
				{
					this.Flags = ItemFlags.ImageStrip;
					this.ImageCount = imageCount;
				}
			}

			#region ImageCollection Private Fields
			private ColorDepth colorDepth = DefaultColorDepth;
			private Size imageSize = DefaultImageSize;
			private Color transparentColor = DefaultTransparentColor;
			private ArrayList list = new ArrayList();
#if NET_2_0
			private ArrayList keys = new ArrayList();
#endif
			private int count;
			private bool handleCreated;
#if NET_2_0
			private int lastKeyIndex = -1;
#endif
			private readonly ImageList owner;
			#endregion // ImageCollection Private Fields

			#region ImageCollection Internal Constructors
			// For use in ImageList
			internal ImageCollection(ImageList owner)
			{
				this.owner = owner;
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
						RecreateHandle();
					}
				}
			}

			// For use in ImageList
			internal IntPtr Handle {
				get {
					CreateHandle();
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
						RecreateHandle();
					}
				}
			}

			// For use in ImageList
			internal ImageListStreamer ImageStream {
				get {
					return this.Empty ? null : new ImageListStreamer(this);
				}

				set {
					int index;
					Image[] streamImages;

					if (value == null) {
#if NET_2_0
						if (this.handleCreated)
							DestroyHandle();
						else
							this.Clear();
#endif
					}
					// Only deserialized ImageListStreamers are used.
					else if ((streamImages = value.Images) != null) {
						this.list = new ArrayList(streamImages.Length);
						this.count = 0;
						this.handleCreated = true;
#if NET_2_0
						this.keys = new ArrayList(streamImages.Length);
#endif

						for (index = 0; index < streamImages.Length; index++) {
							list.Add((Image)streamImages[index].Clone());
#if NET_2_0
							keys.Add(null);
#endif
						}

						// Invalid ColorDepth values are ignored.
						if (Enum.IsDefined(typeof(ColorDepth), value.ColorDepth))
							this.colorDepth = (ColorDepth)value.ColorDepth;
						this.imageSize = value.ImageSize;

#if NET_2_0
						// Event is raised even when handle was not created yet.
						owner.OnRecreateHandle();
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
					return this.handleCreated ? list.Count : this.count;
				}
			}

			public bool Empty {
				get {
					return this.Count == 0;
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
					Image image;

					if (index < 0 || index >= this.Count)
						throw new ArgumentOutOfRangeException("index");

					if (value == null)
						throw new ArgumentNullException("value");

					if (!(value is Bitmap))
						throw new ArgumentException("Image must be a Bitmap.");

					image = CreateImage(value, this.transparentColor);
					CreateHandle();
					list[index] = image;
				}
			}

#if NET_2_0
			public Image this[string key] {
				get {
					int index;

					return (index = IndexOfKey(key)) == -1 ? null : this[index];
				}
			}

			public StringCollection Keys {
				get {
					int index;
					string key;
					StringCollection keyCollection;

					// Returns all keys even when there are more keys than
					// images. Null keys are returned as empty strings.

					keyCollection = new StringCollection();
					for (index = 0; index < keys.Count; index++)
						keyCollection.Add(((key = (string)keys[index]) == null || key.Length == 0) ? string.Empty : key);

					return keyCollection;
				}
			}
#endif
			#endregion // ImageCollection Public Instance Properties

			#region ImageCollection Private Static Methods
#if NET_2_0
			private static bool CompareKeys(string key1, string key2)
			{
				// Keys are case-insensitive and keys with different length
				// are not equal even when string.Compare treats them equal.

				if (key1 == null || key2 == null || key1.Length != key2.Length)
					return false;

				return string.Compare(key1, key2, true, CultureInfo.InvariantCulture) == 0;
			}
#endif
			#endregion // ImageCollection Private Static Methods

			#region ImageCollection Private Instance Methods
#if NET_2_0
			private int AddItem(string key, ImageListItem item)
#else
			private int AddItem(ImageListItem item)
#endif
			{
				int itemIndex;
#if NET_2_0
				int index;
#endif

				if (this.handleCreated)
					itemIndex = AddItemInternal(item);
				else {
					// Image strips are counted as a single item in the return
					// value of Add and AddStrip until handle is created.

					itemIndex = list.Add(item);
					this.count += item.ImageCount;
				}

#if NET_2_0
				if ((item.Flags & ItemFlags.ImageStrip) == 0)
					keys.Add(key);
				else
					for (index = 0; index < item.ImageCount; index++)
						keys.Add(null);
#endif

				return itemIndex;
			}

			internal event EventHandler Changed;

			private int AddItemInternal(ImageListItem item)
			{
				if (Changed != null)
					Changed (this, EventArgs.Empty);

				if (item.Image is Icon) {
					int imageWidth;
					int imageHeight;
					Bitmap bitmap;
					Graphics graphics;

					bitmap = new Bitmap(imageWidth = this.imageSize.Width, imageHeight = this.imageSize.Height, PixelFormat.Format32bppArgb);
					graphics = Graphics.FromImage(bitmap);
					graphics.DrawIcon((Icon)item.Image, new Rectangle(0, 0, imageWidth, imageHeight));
					graphics.Dispose();

					ReduceColorDepth(bitmap);
					return list.Add(bitmap);
				}
				else if ((item.Flags & ItemFlags.ImageStrip) == 0)
					return list.Add(CreateImage((Image)item.Image, (item.Flags & ItemFlags.UseTransparentColor) == 0 ? this.transparentColor : item.TransparentColor));
				else {
					int imageX;
					int width;
					int imageWidth;
					int imageHeight;
					int index;
					Image image;
					Bitmap bitmap;
					Graphics graphics;
					Rectangle imageRect;
					ImageAttributes imageAttributes;

					// When ImageSize was changed after adding image strips
					// Count will return invalid values based on old ImageSize
					// but when creating handle either ArgumentException will
					// be thrown or image strip will be added according to the
					// new ImageSize. This can result in image count
					// difference that can result in exceptions in methods
					// that use Count before creating handle. In addition this
					// can result in the loss of sync with keys. When doing
					// the same after handle was created there are no problems
					// as handle will be recreated after changing ImageSize
					// that results in the loss of images added previously.

					if ((width = (image = (Image)item.Image).Width) == 0 || (width % (imageWidth = this.imageSize.Width)) != 0)
						throw new ArgumentException("Width of image strip must be a positive multiple of ImageSize.Width.", "value");

					if (image.Height != (imageHeight = this.imageSize.Height))
						throw new ArgumentException("Height of image strip must be equal to ImageSize.Height.", "value");

					imageRect = new Rectangle(0, 0, imageWidth, imageHeight);
					if (this.transparentColor.A == 0)
						imageAttributes = null;
					else {
						imageAttributes = new ImageAttributes();
						imageAttributes.SetColorKey(this.transparentColor, this.transparentColor);
					}

					index = list.Count;
					for (imageX = 0; imageX < width; imageX += imageWidth) {
						bitmap = new Bitmap(imageWidth, imageHeight, PixelFormat.Format32bppArgb);
						graphics = Graphics.FromImage(bitmap);
						graphics.DrawImage(image, imageRect, imageX, 0, imageWidth, imageHeight, GraphicsUnit.Pixel, imageAttributes);
						graphics.Dispose();

						ReduceColorDepth(bitmap);
						list.Add(bitmap);
					}

					if (imageAttributes != null)
						imageAttributes.Dispose();

					return index;
				}
			}

			private void CreateHandle()
			{
				int index;
				ArrayList items;

				if (!this.handleCreated) {
					items = this.list;
					this.list = new ArrayList(this.count);
					this.count = 0;
					this.handleCreated = true;

					for (index = 0; index < items.Count; index++)
						AddItemInternal((ImageListItem)items[index]);
				}
			}

			private Image CreateImage(Image value, Color transparentColor)
			{
				int imageWidth;
				int imageHeight;
				ImageAttributes imageAttributes;

				if (transparentColor.A == 0)
					imageAttributes = null;
				else {
					imageAttributes = new ImageAttributes();
					imageAttributes.SetColorKey (transparentColor, transparentColor);
				}

				var bitmap = new Bitmap (imageWidth = this.imageSize.Width, imageHeight = this.imageSize.Height, PixelFormat.Format32bppArgb);
				using (var graphics = Graphics.FromImage (bitmap))
					graphics.DrawImage (value, new Rectangle(0, 0, imageWidth, imageHeight), 0, 0, value.Width, value.Height, GraphicsUnit.Pixel, imageAttributes);

				if (imageAttributes != null)
					imageAttributes.Dispose ();

				ReduceColorDepth (bitmap);
				return bitmap;
			}

			private void RecreateHandle()
			{
				if (this.handleCreated) {
					DestroyHandle();
					this.handleCreated = true;
					owner.OnRecreateHandle();
				}
			}

			private unsafe void ReduceColorDepth(Bitmap bitmap)
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
				Color[] palette;

				if (this.colorDepth < ColorDepth.Depth32Bit) {
					bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
					try {
						linePtr = (byte*)bitmapData.Scan0;
						height = bitmapData.Height;
						widthBytes = bitmapData.Width << 2;
						stride = bitmapData.Stride;

						if (this.colorDepth < ColorDepth.Depth16Bit) {
							palette = (this.colorDepth < ColorDepth.Depth8Bit ? IndexedColorDepths.Palette4Bit : IndexedColorDepths.Palette8Bit).Entries;

							for (line = 0; line < height; line++) {
								lineEndPtr = linePtr + widthBytes;
								for (pixelPtr = linePtr; pixelPtr < lineEndPtr; pixelPtr += 4)
									*(int*)pixelPtr = ((pixel = *(int*)pixelPtr) & AlphaMask) == 0 ? 0x00000000 : IndexedColorDepths.GetNearestColor(palette, pixel | AlphaMask);
								linePtr += stride;
							}
						}
						else if (this.colorDepth < ColorDepth.Depth24Bit) {
							for (line = 0; line < height; line++) {
								lineEndPtr = linePtr + widthBytes;
								for (pixelPtr = linePtr; pixelPtr < lineEndPtr; pixelPtr += 4)
									*(int*)pixelPtr = ((pixel = *(int*)pixelPtr) & AlphaMask) == 0 ? 0x00000000 : (pixel & 0x00F8F8F8) | AlphaMask;
								linePtr += stride;
							}
						}
						else {
							for (line = 0; line < height; line++) {
								lineEndPtr = linePtr + widthBytes;
								for (pixelPtr = linePtr; pixelPtr < lineEndPtr; pixelPtr += 4)
									*(int*)pixelPtr = ((pixel = *(int*)pixelPtr) & AlphaMask) == 0 ? 0x00000000 : pixel | AlphaMask;
								linePtr += stride;
							}
						}
					}
					finally {
						bitmap.UnlockBits(bitmapData);
					}
				}
			}
			#endregion // ImageCollection Private Instance Methods

			#region ImageCollection Internal Instance Methods
			// For use in ImageList
			internal void DestroyHandle()
			{
				if (this.handleCreated) {
					this.list = new ArrayList();
					this.count = 0;
					this.handleCreated = false;
#if NET_2_0
					keys = new ArrayList();
#endif
				}
			}

			// For use in ImageList
			internal Image GetImage(int index)
			{
				if (index < 0 || index >= this.Count)
					throw new ArgumentOutOfRangeException("index");

				CreateHandle();
				return (Image)list[index];
			}

			// For use in ImageListStreamer
			internal Image[] ToArray()
			{
				Image[] images;

				// Handle is created even when the list is empty.
				CreateHandle();
				images = new Image[list.Count];
				list.CopyTo(images);
				return images;
			}
			#endregion // ImageCollection Internal Instance Methods

			#region ImageCollection Public Instance Methods
			public void Add(Icon value)
			{
#if NET_2_0
				Add(null, value);
#else
				AddItem(new ImageListItem(value));
#endif
			}

			public void Add(Image value)
			{
#if NET_2_0
				Add(null, value);
#else
				AddItem(new ImageListItem(value));
#endif
			}

			public int Add(Image value, Color transparentColor)
			{
#if NET_2_0
				return AddItem(null, new ImageListItem(value, transparentColor));
#else
				return AddItem(new ImageListItem(value, transparentColor));
#endif
			}

#if NET_2_0
			public void Add(string key, Icon icon)
			{
				// Argument has name icon but exceptions use name value.
				AddItem(key, new ImageListItem(icon));
			}

			public void Add(string key, Image image)
			{
				// Argument has name image but exceptions use name value.
				AddItem(key, new ImageListItem(image));
			}

			public void AddRange(Image[] images)
			{
				int index;

				if (images == null)
					throw new ArgumentNullException("images");

				for (index = 0; index < images.Length; index++)
					Add(images[index]);
			}
#endif

			public int AddStrip(Image value)
			{
				int width;
				int imageWidth;

				if (value == null)
					throw new ArgumentNullException("value");

				if ((width = value.Width) == 0 || (width % (imageWidth = this.imageSize.Width)) != 0)
					throw new ArgumentException("Width of image strip must be a positive multiple of ImageSize.Width.", "value");

				if (value.Height != this.imageSize.Height)
					throw new ArgumentException("Height of image strip must be equal to ImageSize.Height.", "value");

#if NET_2_0
				return AddItem(null, new ImageListItem(value, width / imageWidth));
#else
				return AddItem(new ImageListItem(value, width / imageWidth));
#endif
			}

			public void Clear()
			{
				list.Clear();
				if (this.handleCreated)
					this.count = 0;
#if NET_2_0
				keys.Clear();
#endif
			}

#if NET_2_0
			[EditorBrowsable(EditorBrowsableState.Never)]
#endif
			public bool Contains(Image image)
			{
				throw new NotSupportedException();
			}

#if NET_2_0
			public bool ContainsKey(string key)
			{
				return IndexOfKey(key) != -1;
			}
#endif

			public IEnumerator GetEnumerator()
			{
				Image[] images = new Image[this.Count];
				int index;

				if (images.Length != 0) {
					// Handle is created only when there are images.
					CreateHandle();

					for (index = 0; index < images.Length; index++)
						images[index] = (Image)((Image)list[index]).Clone();
				}

				return images.GetEnumerator();
			}

#if NET_2_0
			[EditorBrowsable(EditorBrowsableState.Never)]
#endif
			public int IndexOf(Image image)
			{
				throw new NotSupportedException();
			}

#if NET_2_0
			public int IndexOfKey(string key)
			{
				int index;

				if (key != null && key.Length != 0) {
					// When last IndexOfKey was successful and the same key was
					// assigned to an image with a lower index than the last
					// result and the key of the last result equals to key
					// argument the last result is returned.

					if (this.lastKeyIndex >= 0 && this.lastKeyIndex < this.Count && CompareKeys((string)keys[this.lastKeyIndex], key))
						return this.lastKeyIndex;

					// Duplicate keys are allowed and first match is returned.
					for (index = 0; index < this.Count; index++)
						if (CompareKeys((string)keys[index], key))
							return this.lastKeyIndex = index;
				}

				return this.lastKeyIndex = -1;
			}
#endif

#if NET_2_0
			[EditorBrowsable(EditorBrowsableState.Never)]
#endif
			public void Remove(Image image)
			{
				throw new NotSupportedException();
			}

			public void RemoveAt(int index)
			{
				if (index < 0 || index >= this.Count)
					throw new ArgumentOutOfRangeException("index");

				CreateHandle();
				list.RemoveAt(index);
#if NET_2_0
				keys.RemoveAt(index);
#endif
				if (Changed != null)
					Changed (this, EventArgs.Empty);
			}

#if NET_2_0
			public void RemoveByKey(string key)
			{
				int index;

				if ((index = IndexOfKey(key)) != -1)
					RemoveAt(index);
			}

			public void SetKeyName(int index, string name)
			{
				// Only SetKeyName throws IndexOutOfRangeException.
				if (index < 0 || index >= this.Count)
					throw new IndexOutOfRangeException();

				keys[index] = name;
			}
#endif
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

			bool IList.Contains(object image)
			{
				return image is Image ? this.Contains ((Image) image) : false;
			}

			int IList.IndexOf (object image)
			{
				return image is Image ? this.IndexOf ((Image) image) : -1;
			}

			void IList.Insert(int index, object value)
			{
				throw new NotSupportedException();
			}

			void IList.Remove (object image)
			{
				if (image is Image)
					this.Remove ((Image) image);
			}

			void ICollection.CopyTo(Array dest, int index)
			{
				for (int imageIndex = 0; imageIndex < this.Count; imageIndex++)
					dest.SetValue (this[imageIndex], index++);
			}
			#endregion // ImageCollection Interface Methods
		}
		#endregion // Sub-classes

		#region Public Constructors
		public ImageList()
		{
			images = new ImageCollection(this);
		}

		public ImageList(System.ComponentModel.IContainer container) : this()
		{
			container.Add(this);
		}
		#endregion // Public Constructors

		#region Private Instance Methods
		private void OnRecreateHandle()
		{
			EventHandler eh = (EventHandler)(Events [RecreateHandleEvent]);
			if (eh != null)
				eh (this, EventArgs.Empty);
		}

		// MS's TypeDescriptor stuff apparently uses
		// non-public ShouldSerialize* methods, because it
		// picks up this behavior even though the methods
		// aren't public.  we can't make them private, though,
		// without adding compiler warnings.  so, make then
		// internal instead.

		internal bool ShouldSerializeTransparentColor ()
		{
			return this.TransparentColor != Color.LightGray;
		}

		internal bool ShouldSerializeColorDepth()
		{
			// ColorDepth is serialized in ImageStream when non-empty.
			// It is serialized even if it has its default value when empty.
			return images.Empty;
		}               

		internal bool ShouldSerializeImageSize()
		{
			// ImageSize is serialized in ImageStream when non-empty.
			// It is serialized even if it has its default value when empty.
#if NET_2_0
			return images.Empty;
#else
			return this.ImageSize != DefaultImageSize;
#endif
		}


		internal void ResetColorDepth ()
		{
			this.ColorDepth = DefaultColorDepth;
		}

#if NET_2_0
		internal void ResetImageSize ()
		{
			this.ImageSize = DefaultImageSize;
		}

		internal void ResetTransparentColor ()
		{
			this.TransparentColor = Color.LightGray;
		}
#endif
		#endregion // Private Instance Methods

		#region Public Instance Properties
#if !NET_2_0
		[DefaultValue(DefaultColorDepth)]
#endif
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

#if NET_2_0
		[Bindable(true)]
		[DefaultValue(null)]
		[Localizable(false)]
		[TypeConverter(typeof(StringConverter))]
		public object Tag {
			get {
				return this.tag;
			}

			set {
				this.tag = value;
			}
		}
#endif

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
			return base.ToString() + " Images.Count: " + images.Count.ToString() + ", ImageSize: " + this.ImageSize.ToString();
		}
		#endregion // Public Instance Methods

		#region Protected Instance Methods
		protected override void Dispose(bool disposing)
		{
			if (disposing)
				images.DestroyHandle();

			base.Dispose(disposing);
		}
		#endregion // Protected Instance Methods

		#region Events
		static object RecreateHandleEvent = new object ();

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public event EventHandler RecreateHandle {
			add { Events.AddHandler (RecreateHandleEvent, value); }
			remove { Events.RemoveHandler (RecreateHandleEvent, value); }
		}
		#endregion // Events
	}
}
