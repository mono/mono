//
// System.Drawing.Imaging.ImageAttributes.cs
//
// Author:
//   Dennis Hayes (dennish@raytek.com)
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Drawing;

namespace System.Drawing.Imaging
{
	/// <summary>
	/// Summary description for ImageAttributes.
	/// </summary>
	public sealed class ImageAttributes : IDisposable {
		public ImageAttributes() {
		}

		//Clears the color keys for all GDI+ objects
		public void ClearColorKey(){
		}

		//Sets the color keys for all GDI+ objects
		public void SetColorKey(Color colorLow, Color colorHigh){

		}

		public void SetColorMatrix(ColorMatrix colorMatrix) {
			throw new NotImplementedException();
		}

		public void SetColorMatrix(ColorMatrix colorMatrix, ColorMatrixFlag colorMatrixFlag) {
			throw new NotImplementedException();
		}

		public void SetColorMatrix(ColorMatrix colorMatrix, ColorMatrixFlag colorMatrixFlag, ColorAdjustType colorAdjustType) {
			throw new NotImplementedException();
		}

		public void Dispose() {
		}

		~ImageAttributes() {
		}

	}
}
