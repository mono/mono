//
// System.Drawing.Imaging.ColorMatrix.cs
//
// Authors:
//   Everaldo Canuto (everaldo.canuto@bol.com.br)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System;

namespace System.Drawing.Imaging {

	public sealed class ColorMatrix {

		float[] colors;

		// constructors
		public ColorMatrix() 
		{
			colors = new float[25];
			// Set identity matrix by default
			colors[0]  = 1;
			colors[6]  = 1;
			colors[12] = 1;
			colors[18] = 1;
			colors[24] = 1;
		}
		
		[CLSCompliant(false)]
		public ColorMatrix(float[][] newColorMatrix)
		{
			colors = new float[25];
			for (int x = 0; x < 5; x++) {
				for (int y = 0; y < 5; y++) {
					colors[x * 5 + y] = newColorMatrix[x][y];
				}
			}
		}
		
		// properties
		public float this[int row, int column] {
			get { return colors[row * 5 + column]; }
			set { colors[row * 5 + column] = value; }
		}
		

		public float Matrix00 {
			get { return colors[0]; }
			set { colors[0] = value; }
		}

		public float Matrix01 {
			get { return colors[1]; }
			set { colors[1] = value; }
		}

		public float Matrix02 {
			get { return colors[2]; }
			set { colors[2] = value; }
		}

		public float Matrix03 {
			get { return colors[3]; }
			set { colors[3] = value; }
		}

		public float Matrix04 {
			get { return colors[4]; }
			set { colors[4] = value; }
		}


		public float Matrix10 {
			get { return colors[5]; }
			set { colors[5] = value; }
		}

		public float Matrix11 {
			get { return colors[6]; }
			set { colors[6] = value; }
		}

		public float Matrix12 {
			get { return colors[7]; }
			set { colors[7] = value; }
		}

		public float Matrix13 {
			get { return colors[8]; }
			set { colors[8] = value; }
		}

		public float Matrix14 {
			get { return colors[9]; }
			set { colors[9] = value; }
		}


		public float Matrix20 {
			get { return colors[10]; }
			set { colors[10] = value; }
		}

		public float Matrix21 {
			get { return colors[11]; }
			set { colors[11] = value; }
		}

		public float Matrix22 {
			get { return colors[12]; }
			set { colors[12] = value; }
		}

		public float Matrix23 {
			get { return colors[13]; }
			set { colors[13] = value; }
		}

		public float Matrix24 {
			get { return colors[14]; }
			set { colors[14] = value; }
		}


		public float Matrix30 {
			get { return colors[15]; }
			set { colors[15] = value; }
		}

		public float Matrix31 {
			get { return colors[16]; }
			set { colors[16] = value; }
		}

		public float Matrix32 {
			get { return colors[17]; }
			set { colors[17] = value; }
		}

		public float Matrix33 {
			get { return colors[18]; }
			set { colors[18] = value; }
		}

		public float Matrix34 {
			get { return colors[19]; }
			set { colors[19] = value; }
		}


		public float Matrix40 {
			get { return colors[20]; }
			set { colors[20] = value; }
		}

		public float Matrix41 {
			get { return colors[21]; }
			set { colors[21] = value; }
		}

		public float Matrix42 {
			get { return colors[22]; }
			set { colors[22] = value; }
		}

		public float Matrix43 {
			get { return colors[23]; }
			set { colors[23] = value; }
		}

		public float Matrix44 {
			get { return colors[24]; }
			set { colors[24] = value; }
		}

	}

}
