//
// System.Drawing.Imaging.ImageAttributes.cs
//
// Author:
//   Dennis Hayes (dennish@raytek.com) (stubbed out)
//	 Jordi Mas i Hernàndez (jmas@softcatala.org)
// (C) 2002-4 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Drawing;

namespace System.Drawing.Imaging
{
	/// <summary>
	/// Summary description for ImageAttributes.
	/// </summary>
	public sealed class ImageAttributes : ICloneable, IDisposable {
		
		private IntPtr nativeImageAttr = IntPtr.Zero;
		
		internal IntPtr NativeObject{
			get{
				return nativeImageAttr;
			}
		}
		
		public ImageAttributes() {	
			
			
			Status status = GDIPlus.GdipCreateImageAttributes(out nativeImageAttr);
						
			if (status != Status.Ok)
				throw new Exception ("Error calling GDIPlus.GdipCreateImageAttributes:" +status);
				
			Console.WriteLine("ImageAttributes().ImageAttributes()" + nativeImageAttr);
		}

		//Clears the color keys for all GDI+ objects
		public void ClearColorKey(){			                    
			
			Status status = GDIPlus.GdipSetImageAttributesColorKeys(nativeImageAttr,  ColorAdjustType.Default,  false, 0,0);
			
			if (status != Status.Ok)
				throw new Exception ("Error calling GDIPlus.ClearColorKey:" +status);
		}

		//Sets the color keys for all GDI+ objects
		public void SetColorKey(Color colorLow, Color colorHigh){
			
			Status status = GDIPlus.GdipSetImageAttributesColorKeys(nativeImageAttr,
            				ColorAdjustType.Default, true,  colorLow.ToArgb(), colorHigh.ToArgb());

			if (status != Status.Ok)
				throw new Exception ("Error calling GDIPlus.GdipSetImageAttributesColorKeys:" +status);
		}

		public void SetColorMatrix(ColorMatrix colorMatrix) {
			
			Status status = GDIPlus.GdipSetImageAttributesColorMatrix(nativeImageAttr, ColorAdjustType.Default,
                                            true, colorMatrix, (ColorMatrix)null, ColorMatrixFlag.Default);
			if (status != Status.Ok)
				throw new Exception ("Error calling GDIPlus.SetColorMatrix:" +status);                                           
		}

		public void SetColorMatrix(ColorMatrix colorMatrix, ColorMatrixFlag colorMatrixFlag) {
			
			Status status = GDIPlus.GdipSetImageAttributesColorMatrix(nativeImageAttr, ColorAdjustType.Default,
                                            true, colorMatrix, (ColorMatrix)null, colorMatrixFlag);
			if (status != Status.Ok)
				throw new Exception ("Error calling GDIPlus.SetColorMatrix:" +status);                                                                                       
                                            
		}

		public void SetColorMatrix(ColorMatrix colorMatrix, ColorMatrixFlag colorMatrixFlag, ColorAdjustType colorAdjustType) {
			
			Status status = GDIPlus.GdipSetImageAttributesColorMatrix(nativeImageAttr,colorAdjustType,
                                            true,  colorMatrix,  (ColorMatrix)null,  colorMatrixFlag);
                                            
			if (status != Status.Ok)
				throw new Exception ("Error calling GDIPlus.SetColorMatrix:" +status);                                                                                       
		}
		
		void Dispose (bool disposing)
		{
			if (!disposing) return;
			
			Status status = GDIPlus.GdipDisposeImageAttributes(nativeImageAttr);
			
			if (status != Status.Ok)
				throw new Exception ("Error calling GDIPlus.GdipDisposeImageAttributes:" +status);
			else
				nativeImageAttr = IntPtr.Zero;
		}


		public void Dispose() {
			
			Dispose (true);
			System.GC.SuppressFinalize (this);
		}

		~ImageAttributes() {
			
			Dispose (false);
		}

		[MonoTODO]
		object ICloneable.Clone()
		{
			throw new NotImplementedException ();
		}
	}
}
