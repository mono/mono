//
// System.Drawing.Drawing2D.AdjustableArrowCap.cs
//
// Author:
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002/3 Ximian, Inc
//
using System;

namespace System.Drawing.Drawing2D
{
	/// <summary>
	/// Summary description for AdjustableArrowCap.
	/// </summary>
	public sealed class AdjustableArrowCap : CustomLineCap
	{
		private bool isFilled;
		private float height;
		private float width;
		private float middleInset;
		//Constructors
		public AdjustableArrowCap(float width, float height, bool isFilled) {
			this.isFilled = isFilled;
			this.height = height;
			this.width = width;
			middleInset = 0;
		}

		public AdjustableArrowCap(float width, float height) {
			isFilled = true;
			this.height = height;
			this.width = width;
			middleInset = 0;
			//AdjustableArrowCap(width, height, true);
		}

		//Public Properities
		[MonoTODO] //redraw on set
		public bool Filled {
			get {
				return isFilled;
			}
			set {
				isFilled = value;
				//Redraw!
			}
		}
		[MonoTODO] //redraw on set
		public float Width {
			get {
				return width;
			}
			set {
				width = value;
				//Redraw!
			}
		}
		[MonoTODO] //redraw on set
		public float Height {
			get {
				return height;
			}
			set {
				height = value;
				//Redraw!
			}
		}
		[MonoTODO] //redraw on set
		public float MiddleInset {
			get {
				return middleInset;
			}
			set {
				middleInset = value;
				//Redraw!
			}
		}

	}
}
