//
// System.Drawing.Drawing2D.Blend.cs
//
// Authors:
//   Dennis Hayes (dennish@Raytek.com)
//   Ravindra (rkumar@novell.com)
//
// (C) 2002/3 Ximian, Inc. http://www.ximian.com
// (C) 2004 Novell, Inc. http://www.novell.com
//
using System;

namespace System.Drawing.Drawing2D
{
	/// <summary>
	/// Summary description for Blend.
	/// </summary>
	public sealed class Blend
	{
		private float [] positions;
		private float [] factors;

		public Blend ()
		{
			positions = new float [1];
			factors = new float [1];
		}

		public Blend (int count)
		{
			positions = new float [count];
			factors = new float [count];
		}

		public float [] Factors {
			get {
				return factors;
			}

			set {
				factors = value;
			}
		}

		public float [] Positions {
			get {
				return positions;
			}

			set {
				positions = value;
			}
		}
	}
}
