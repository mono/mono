//
// System.Drawing.Drawing2D.AdjustableArrowCap.cs
//
// Authors:
//	Dennis Hayes (dennish@Raytek.com)
//	Ravindra (rkumar@novell.com)
//
// Copyright (C) 2002/3 Ximian, Inc. http://www.ximian.com
// Copyright (C) 2004 Novell, Inc. http://www.novell.com
//

using System;

namespace System.Drawing.Drawing2D
{
	/// <summary>
	/// Summary description for AdjustableArrowCap.
	/// </summary>
	public sealed class AdjustableArrowCap : CustomLineCap
	{
		// Constructors

		internal AdjustableArrowCap (IntPtr ptr) : base (ptr)
		{
		}

		public AdjustableArrowCap (float width, float height) : this (width, height, true)
		{
		}

		public AdjustableArrowCap (float width, float height, bool isFilled)
		{
			Status status = GDIPlus.GdipCreateAdjustableArrowCap (height, width, isFilled, out nativeObject);
			GDIPlus.CheckStatus (status);
		}

		// Public Properities

		public bool Filled {
			get {
				bool isFilled;
				Status status = GDIPlus.GdipGetAdjustableArrowCapFillState (nativeObject, out isFilled);
				GDIPlus.CheckStatus (status);

				return isFilled;
			}

			set {
				Status status = GDIPlus.GdipSetAdjustableArrowCapFillState (nativeObject, value);
				GDIPlus.CheckStatus (status);
			}
		}

		public float Width {
			get {
				float width;
				Status status = GDIPlus.GdipGetAdjustableArrowCapWidth (nativeObject, out width);
				GDIPlus.CheckStatus (status);

				return width;
			}

			set {
				Status status = GDIPlus.GdipSetAdjustableArrowCapWidth (nativeObject, value);
				GDIPlus.CheckStatus (status);
			}
		}

		public float Height {
			get {
				float height;
				Status status = GDIPlus.GdipGetAdjustableArrowCapHeight (nativeObject, out height);
				GDIPlus.CheckStatus (status);

				return height;
			}

			set {
				Status status = GDIPlus.GdipSetAdjustableArrowCapHeight (nativeObject, value);
				GDIPlus.CheckStatus (status);
			}
		}

		public float MiddleInset {
			get {
				float middleInset;
				Status status = GDIPlus.GdipGetAdjustableArrowCapMiddleInset (nativeObject, out middleInset);
				GDIPlus.CheckStatus (status);

				return middleInset;
			}

			set {
				Status status = GDIPlus.GdipSetAdjustableArrowCapMiddleInset (nativeObject, value);
				GDIPlus.CheckStatus (status);
			}
		}
	}
}
