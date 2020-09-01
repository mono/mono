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
// Copyright (c) 2007 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Jackson Harper (jackson@ximian.com)
//


using System;
using System.IO;
using System.Drawing;

namespace System.Windows.Forms.RTF {

	internal class Picture {

		private Minor image_type;
		private Image image;
		private MemoryStream data;
		private float width = -1;
		private float height = -1;

		private readonly static float dpix;

		static Picture ()
		{
			dpix = TextRenderer.GetDpi ().Width;
		}

		public Picture ()
		{
			
		}

		public Minor ImageType {
			get { return image_type; }
			set { image_type = value; }
		}

		public MemoryStream Data {
			get {
				if (data == null)
					data = new MemoryStream ();
				return data;
			}
		}

		public float Width {
			get {
				float w = width;
				if (w < 0) {
					if (image == null)
						image = ToImage ();
					w = image.Width;
				}
				return w;
				
			}
		}

		public float Height {
			get {
				float h = height;
				if (h < 0) {
					if (image == null)
						image = ToImage ();
					h = image.Height;
				}
				return h;
			}
		}

		public SizeF Size {
			get {	
				return new SizeF (Width, Height);
			}
		}

		public void SetWidthFromTwips (int twips)
		{
			width = (int) (((float) twips / 1440.0F) * dpix + 0.5F);
		}

		public void SetHeightFromTwips (int twips)
		{
			height = (int) (((float) twips / 1440.0F) * dpix + 0.5F);
		}

		//
		// Makes sure that we got enough information to actually use the image
		//
		public bool IsValid ()
		{
			if  (data == null)
				return false;
			switch (image_type) {
			case Minor.PngBlip:
			case Minor.JpegBlip:
			case Minor.WinMetafile:
			case Minor.EnhancedMetafile:
				break;
			default:
				return false;
			}

			return true;
		}

		public void DrawImage (Graphics dc, float x, float y, bool selected)
		{
			if (image == null)
				image = ToImage ();
			dc.DrawImage (image, x, y, Width, Height);
		}

		public Image ToImage ()
		{
			// Reset the data stream position to the beginning
			data.Position = 0;
			return Image.FromStream (data);
		}
	}

}

