//
// System.Drawing.Imaging.ImageAttributes.cs
//
// Author:
//   Dennis Hayes (dennish@raytek.com) (stubbed out)
//	 Jordi Mas i Hernàndez (jmas@softcatala.org)
//	 Sanjay Gupta (gsanjay@novell.com)
//
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

		public void ClearBrushRemapTable()
		{
			ClearRemapTable (ColorAdjustType.Brush);			
		}

		//Clears the color keys for all GDI+ objects
		public void ClearColorKey() 
		{			                 
			ClearColorKey (ColorAdjustType.Default);
		}

		public void ClearColorKey(ColorAdjustType type)
		{
			Status status = GDIPlus.GdipSetImageAttributesColorKeys (nativeImageAttr, 
																	type, false, 0, 0);
			
			GDIPlus.CheckStatus (status);			
		}

		public void ClearColorMatrix()
		{
			ClearColorMatrix (ColorAdjustType.Default);
		}
		
		public void ClearColorMatrix(ColorAdjustType type)
		{
			Status status = GDIPlus.GdipSetImageAttributesColorMatrix (nativeImageAttr, 
										type, false, null, null, ColorMatrixFlag.Default);
			
			GDIPlus.CheckStatus (status);			
		}
		
		public void ClearGamma()
		{
			ClearGamma (ColorAdjustType.Default);
		}
		
		public void ClearGamma(ColorAdjustType type)
		{
			Status status = GDIPlus.GdipSetImageAttributesGamma (nativeImageAttr, 
																	type, false, 0);
			
			GDIPlus.CheckStatus (status);
		}
		
		public void ClearNoOp()
		{
			ClearNoOp (ColorAdjustType.Default);
		}
		
		public void ClearNoOp(ColorAdjustType type)
		{
			Status status = GDIPlus.GdipSetImageAttributesNoOp (nativeImageAttr, 
																		type, false);
			
			GDIPlus.CheckStatus (status);
		}

		public void ClearOutputChannel()
		{
			ClearOutputChannel (ColorAdjustType.Default);
		}
		
		public void ClearOutputChannel(ColorAdjustType type)
		{
			Status status = GDIPlus.GdipSetImageAttributesOutputChannel (nativeImageAttr, 
																		type, false, ColorChannelFlag.ColorChannelLast );
			
			GDIPlus.CheckStatus (status);
		}

		public void ClearOutputChannelColorProfile()
		{
			ClearOutputChannelColorProfile (ColorAdjustType.Default);
		}
		
		public void ClearOutputChannelColorProfile(ColorAdjustType type)
		{
			Status status = GDIPlus.GdipSetImageAttributesOutputChannelColorProfile (nativeImageAttr, 
																	type, false, null);
			
			GDIPlus.CheckStatus (status);
		}

		public void ClearRemapTable()
		{
			ClearRemapTable (ColorAdjustType.Default);
		}
		
		public void ClearRemapTable(ColorAdjustType type)
		{
			Status status = GDIPlus.GdipSetImageAttributesRemapTable (nativeImageAttr, 
															type, false, 0, IntPtr.Zero);
			
			GDIPlus.CheckStatus (status);
		}

		public void ClearThreshold()
		{
			ClearThreshold (ColorAdjustType.Default);
		}
		
		public void ClearThreshold(ColorAdjustType type)
		{
			Status status = GDIPlus.GdipSetImageAttributesThreshold (nativeImageAttr, 
																		type, false, 0);
			
			GDIPlus.CheckStatus (status);
		}

		//Sets the color keys for all GDI+ objects
		public void SetColorKey(Color colorLow, Color colorHigh)
		{
			
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
