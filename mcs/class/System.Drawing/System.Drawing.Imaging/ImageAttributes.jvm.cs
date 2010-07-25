
using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace System.Drawing.Imaging
{
	/// <summary>
	/// Summary description for ImageAttributes.
	/// </summary>
	/// 
	[MonoTODO]
	public sealed class ImageAttributes : ICloneable, IDisposable
	{
		[MonoTODO]
		public ImageAttributes()
		{
		}

		public  void Dispose()
		{
		}

		public Object Clone()
		{
			ImageAttributes imgAttr = new ImageAttributes();
			imgAttr.clrMatrix = clrMatrix;
			imgAttr.clrMatrixFlag = clrMatrixFlag;
			imgAttr.clrAdjustType = clrAdjustType;
			imgAttr.gMatrix = gMatrix;
			imgAttr.thresh = thresh;
			imgAttr.gamma = gamma;
			imgAttr.clrChannelFlags = clrChannelFlags;
			imgAttr.clrProfileFilename = clrProfileFilename;
			imgAttr.clrLow = clrLow;
			imgAttr.clrHigh = clrHigh;
			imgAttr.clrMap = clrMap;
			imgAttr.wrapMode = wrapMode;
			imgAttr.col = col;
			imgAttr.bClamp = bClamp;
			imgAttr.clrPalette = clrPalette;
			imgAttr.bNoOp = bNoOp;
			return imgAttr;
		}


		public void SetColorMatrix(ColorMatrix newColorMatrix)
		{
			SetColorMatrix(newColorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Default);
		}

		public void SetColorMatrix(ColorMatrix newColorMatrix, ColorMatrixFlag flags)
		{
			SetColorMatrix(newColorMatrix, flags, ColorAdjustType.Default);
		}

		public void SetColorMatrix(ColorMatrix newColorMatrix, ColorMatrixFlag mode, ColorAdjustType type)
		{
			clrMatrix = newColorMatrix;
			clrMatrixFlag = mode;
			clrAdjustType = type;
		}

		public void ClearColorMatrix()
		{
			ClearColorMatrix(ColorAdjustType.Default);
		}

		public void ClearColorMatrix(ColorAdjustType type)
		{
			ColorMatrix cm = new ColorMatrix();
			clrMatrix = cm;
			clrAdjustType = type;
		}

		public void SetColorMatrices(ColorMatrix newColorMatrix, ColorMatrix gMatrix)
		{
			SetColorMatrices(newColorMatrix, gMatrix, ColorMatrixFlag.Default, ColorAdjustType.Default);
		}

		public void SetColorMatrices(ColorMatrix newColorMatrix, ColorMatrix gMatrix, ColorMatrixFlag flags)
		{
			SetColorMatrices(newColorMatrix, gMatrix, flags, ColorAdjustType.Default);
		}

		public void SetColorMatrices(ColorMatrix newColorMatrix, ColorMatrix gMatrix, ColorMatrixFlag mode, ColorAdjustType type)
		{
			clrMatrix = newColorMatrix;
			this.gMatrix = gMatrix;
			clrMatrixFlag = mode;
			clrAdjustType = type;
		}

		public void SetThreshold(float thresh)
		{
			SetThreshold(thresh, ColorAdjustType.Default);
		}

		public void SetThreshold(float thresh, ColorAdjustType type)
		{
			this.thresh = thresh;
			clrAdjustType = type;
		}

		public void ClearThreshold()
		{
			ClearThreshold(ColorAdjustType.Default);
		}

		public void ClearThreshold(ColorAdjustType type)
		{
			thresh = 1.0F;
			clrAdjustType = type;
		}

		public void SetGamma(float gamma)
		{
			SetGamma(gamma, ColorAdjustType.Default);
		}

		public void SetGamma(float gamma, ColorAdjustType type)
		{
			this.gamma = gamma;
			clrAdjustType = type;
			return;
		}

		public void ClearGamma()
		{
			ClearGamma(ColorAdjustType.Default);
		}

		public void ClearGamma(ColorAdjustType type)
		{
			gamma = 1;
			clrAdjustType = type;
		}

		public void SetNoOp()
		{
			SetNoOp(ColorAdjustType.Default);
		}

		public void SetNoOp(ColorAdjustType type)
		{
			bNoOp = true;
			clrAdjustType = type;
		}

		public void ClearNoOp()
		{
			ClearNoOp(ColorAdjustType.Default);
		}

		public void ClearNoOp(ColorAdjustType type)
		{
			bNoOp = false;
			clrAdjustType = type;
		}

		public void SetColorKey(Color clrLow, Color clrHigh)
		{
			SetColorKey(clrLow, clrHigh, ColorAdjustType.Default);
		}

		public void SetColorKey(Color clrLow, Color clrHigh, ColorAdjustType type)
		{
			this.clrLow = clrLow;
			this.clrHigh = clrHigh;
			clrAdjustType = type;
		}

		public void ClearColorKey()
		{
			ClearColorKey(ColorAdjustType.Default);
		}

		public void ClearColorKey(ColorAdjustType type)
		{
			clrAdjustType = type;
		}

		public void SetOutputChannel(ColorChannelFlag flags)
		{
			SetOutputChannel(flags, ColorAdjustType.Default);
		}

		public void SetOutputChannel(ColorChannelFlag flags, ColorAdjustType type)
		{
			clrChannelFlags = flags;
			clrAdjustType = type;
		}

		public void ClearOutputChannel()
		{
			ClearOutputChannel(ColorAdjustType.Default);
		}

		public void ClearOutputChannel(ColorAdjustType type)
		{
			clrAdjustType = type;
		}

		public void SetOutputChannelColorProfile(String clrProfileFilename)
		{
			SetOutputChannelColorProfile(clrProfileFilename, ColorAdjustType.Default);
		}

		public void SetOutputChannelColorProfile(String clrProfileFilename, ColorAdjustType type)
		{
			this.clrProfileFilename = clrProfileFilename;
			clrAdjustType = type;
		}

		public void ClearOutputChannelColorProfile()
		{
			ClearOutputChannel(ColorAdjustType.Default);
		}

		public void ClearOutputChannelColorProfile(ColorAdjustType type)
		{
			clrProfileFilename = null;
			clrAdjustType = type;
		}

		public void SetRemapTable(ColorMap[] map)
		{
			SetRemapTable(map, ColorAdjustType.Default);
		}

		public void SetRemapTable(ColorMap[] map, ColorAdjustType type)
		{
			clrMap = map;
			clrAdjustType = type;
		}

		public void ClearRemapTable()
		{
			ClearRemapTable(ColorAdjustType.Default);
		}

		public void ClearRemapTable(ColorAdjustType type)
		{
			clrMap = null;
			clrAdjustType = type;
		}

		public void SetBrushRemapTable(ColorMap []map)
		{
			SetRemapTable(map, ColorAdjustType.Brush);
		}

		public void ClearBrushRemapTable()
		{
			ClearRemapTable(ColorAdjustType.Brush);
		}

		public void SetWrapMode(WrapMode mode)
		{
			SetWrapMode(mode, new Color(), false);
		}

		public void SetWrapMode(WrapMode mode, Color clr)
		{
			SetWrapMode(mode, clr, false);
		}

		public void SetWrapMode(WrapMode mode, Color clr, bool bClamp)
		{
			wrapMode = mode;
			col = clr;
			this.bClamp = bClamp;
		}

		public void GetAdjustedPalette(ColorPalette palette, ColorAdjustType type)
		{
			clrPalette = palette;
			clrAdjustType = type;
		}

		public ColorMatrix clrMatrix;
		public ColorMatrixFlag clrMatrixFlag;
		public ColorAdjustType clrAdjustType;
		public ColorMatrix gMatrix;
		public float thresh;
		public float gamma;
		public ColorChannelFlag clrChannelFlags;
		public string clrProfileFilename;
		public Color clrLow;
		public Color clrHigh;
		public ColorMap[] clrMap;
		public WrapMode wrapMode;
		public Color col;
		public bool bClamp;
		public ColorPalette clrPalette;
		public bool bNoOp;
	}
}