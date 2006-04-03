//
// System.Drawing.Icon.cs
//
// Authors:
//   Andrew Skiba (andrews@mainsoft.com)
//   Dennis Hayes (dennish@Raytek.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Sanjay Gupta (gsanjay@novell.com)
//
// Copyright (C) 2005 Mainsoft, Corp. http://mainsoft.com
// Copyright (C) 2002 Ximian, Inc. http://www.ximian.com
// Copyright (C) 2004 Novell, Inc. http://www.novell.com
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

using System;
using System.IO;
using System.Drawing.Imaging;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace System.Drawing
{
	[Serializable]
	[ComVisible (false)]
	[TypeConverter(typeof(IconConverter))]
	public sealed class Icon 
		: MarshalByRefObject, ISerializable, ICloneable, IDisposable
	{
		private System.Drawing.Bitmap _bitmap;

		#region Ctors
		private void SelectSize (int width, int height) {
			int count = _bitmap.GetFrameCount (FrameDimension.Resolution);
			bool sizeObtained = false;
			for (int i=0; i<count; i++){
				_bitmap.SelectActiveFrame (
					System.Drawing.Imaging.FrameDimension.Resolution, i);
				if (!sizeObtained)
					if (_bitmap.Height==height && _bitmap.Width==width) {
						sizeObtained = true;
						break;
					}
			}

			if (!sizeObtained){
				uint largestSize = 0;
				Bitmap tmpBmp = _bitmap;
				for (int j=0; j<count; j++){
					tmpBmp.SelectActiveFrame (FrameDimension.Resolution, j);
					uint thisSize = (uint)_bitmap.Height * (uint)_bitmap.Width;
					if (thisSize >= largestSize){
						largestSize = thisSize;
						_bitmap = tmpBmp;
					}
				}
			}
		}
			
		private Icon () {
		}
		
		internal Icon (Bitmap bitmap) {
			_bitmap = bitmap;
		}

		public Icon (Icon original, int width, int height) {			
			_bitmap = original._bitmap;
			SelectSize (width, height);
		}

		public Icon (Icon original, Size size)
			:this (original, size.Width, size.Height) {
		}

		public Icon (Stream stream) 
			: this (stream, 32, 32) {
		}

		public Icon (Stream stream, int width, int height)
		{
			_bitmap = new Bitmap (stream, false, ImageFormat.Icon);
			SelectSize (width, height);
		}

		public Icon (string fileName) {
			_bitmap = new Bitmap (fileName, false, ImageFormat.Icon);
		}

		public Icon (Type type, string resource)
		{
			using (Stream s = type.Assembly.GetManifestResourceStream (resource)) {
				if (s == null)
					throw new FileNotFoundException ("Resource name was not found: `" + resource + "'");
				_bitmap = new Bitmap (s, false, ImageFormat.Icon);
			}
		}

		[MonoTODO]
   		private Icon (SerializationInfo info, StreamingContext context)
		{
			//FIXME, need to check how MS stores Icon structure
			//Will serialized form help
			throw new NotImplementedException ();
		}
		#endregion

		[MonoTODO]
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}

		public void Dispose ()
		{
#if INTPTR_SUPPORT
			if (winHandle!=IntPtr.Zero)
				winHandle = IntPtr.Zero;
#endif
		}

		public object Clone ()
		{
			Icon newIcon = new Icon ();
			newIcon._bitmap = (Bitmap)_bitmap.Clone ();
			return newIcon;
		}

#if INTPTR_SUPPORT
		[MonoTODO]
		public static Icon FromHandle (IntPtr handle)
		{
			throw new NotImplementedException ();
		}
#endif
		public void Save (Stream outputStream)	{
			_bitmap.Save (outputStream, System.Drawing.Imaging.ImageFormat.Icon);
		}

		public Bitmap ToBitmap () {
			return _bitmap;
		}

		public override string ToString ()
		{
			//is this correct, this is what returned by .Net
			return "<Icon>";			
		}

#if INTPTR_SUPPORT
		[Browsable (false)]
		public IntPtr Handle {
			get { 
				return winHandle;
			}
		}
#endif

		[Browsable (false)]
		public int Height {
			get {
				return _bitmap.Height;
			}
		}

		public Size Size {
			get {
				return _bitmap.Size;
			}
		}

		[Browsable (false)]
		public int Width {
			get {
				return _bitmap.Width;
			}
		}
	}
}
