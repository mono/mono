//
// System.Drawing.Drawing2D.CustomLineCap.cs
//
// Authors:
//	Dennis Hayes (dennish@Raytek.com)
//	Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//	Ravindra (rkumar@novell.com)
//
// Copyright (C) 2002/3 Ximian, Inc. http://www.ximian.com
// Copyright (C) 2004 Novell, Inc. http://www.novell.com
//

using System;

namespace System.Drawing.Drawing2D
{
	/// <summary>
	/// Summary description for CustomLineCap.
	/// </summary>
	public class CustomLineCap : MarshalByRefObject, ICloneable, IDisposable
	{
		private bool disposed;
		internal IntPtr nativeObject;

		// Constructors

		internal CustomLineCap () { }

		internal CustomLineCap (IntPtr ptr)
		{
			nativeObject = ptr;
		}

		public CustomLineCap (GraphicsPath fillPath, GraphicsPath strokePath) : this (fillPath, strokePath, LineCap.Flat, 0)
		{
		}

		public CustomLineCap (GraphicsPath fillPath, GraphicsPath strokePath, LineCap baseCap) : this (fillPath, strokePath, baseCap, 0)
		{
		}

		public CustomLineCap(GraphicsPath fillPath, GraphicsPath strokePath, LineCap baseCap, float baseInset)
		{
			IntPtr fill = IntPtr.Zero;
			IntPtr stroke = IntPtr.Zero;

			if (fillPath != null)
				fill = fillPath.nativePath;
			if (strokePath != null)
				stroke = strokePath.nativePath;

			Status status = GDIPlus.GdipCreateCustomLineCap (fill, stroke, baseCap, baseInset, out nativeObject);
			GDIPlus.CheckStatus (status);
		}

		public LineCap BaseCap {
			get {
				LineCap baseCap;
				Status status = GDIPlus.GdipGetCustomLineCapBaseCap (nativeObject, out baseCap);
				GDIPlus.CheckStatus (status);

				return baseCap;
			}

			set {
				Status status = GDIPlus.GdipSetCustomLineCapBaseCap (nativeObject, value);
				GDIPlus.CheckStatus (status);
			}
		}

		public LineJoin StrokeJoin {
			get {
				LineJoin strokeJoin;
				Status status = GDIPlus.GdipGetCustomLineCapStrokeJoin (nativeObject, out strokeJoin);
				GDIPlus.CheckStatus (status);

				return strokeJoin;
			}

			set {
				Status status = GDIPlus.GdipSetCustomLineCapStrokeJoin (nativeObject, value);
				GDIPlus.CheckStatus (status);
			}
		}

		public float BaseInset {
			get {
				float baseInset;
				Status status = GDIPlus.GdipGetCustomLineCapBaseInset (nativeObject, out baseInset);
				GDIPlus.CheckStatus (status);

				return baseInset;
			}

			set {
				Status status = GDIPlus.GdipSetCustomLineCapBaseInset (nativeObject, value);
				GDIPlus.CheckStatus (status);
			}
		}

		public float WidthScale {
			get {
				float widthScale;
				Status status = GDIPlus.GdipGetCustomLineCapWidthScale (nativeObject, out widthScale);
				GDIPlus.CheckStatus (status);

				return widthScale;
			}

			set {
				Status status = GDIPlus.GdipSetCustomLineCapWidthScale (nativeObject, value);
				GDIPlus.CheckStatus (status);
			}
		}

		// Public Methods

		public virtual object Clone ()
		{
			IntPtr clonePtr;
			Status status = GDIPlus.GdipCloneCustomLineCap (nativeObject, out clonePtr);
			GDIPlus.CheckStatus (status);

			return new CustomLineCap (clonePtr);
		}
		
		public virtual void Dispose ()
		{
			Dispose (true);
			System.GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (! disposed) {
				Status status = GDIPlus.GdipDeleteCustomLineCap (nativeObject);
				GDIPlus.CheckStatus (status);
				disposed = true;
				nativeObject = IntPtr.Zero;
			}
		}
		
		~CustomLineCap ()
		{
			Dispose (false);
		}

		public void GetStrokeCaps (out LineCap startCap, out LineCap endCap)
		{
			Status status = GDIPlus.GdipGetCustomLineCapStrokeCaps (nativeObject, out startCap, out endCap);
			GDIPlus.CheckStatus (status);
		}

		public void SetStrokeCaps(LineCap startCap, LineCap endCap)
		{
			Status status = GDIPlus.GdipSetCustomLineCapStrokeCaps (nativeObject, startCap, endCap);
			GDIPlus.CheckStatus (status);
		}
	}
}
