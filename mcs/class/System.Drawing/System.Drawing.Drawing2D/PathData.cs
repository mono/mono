//
// System.Drawing.Drawing2D.PathData.cs
//
// Authors:
//   Dennis Hayes (dennish@Raytek.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002/3 Ximian, Inc
//

using System;

namespace System.Drawing.Drawing2D
{
	/// <summary>
	/// Summary description for PathData.
	/// </summary>
	public sealed class PathData
	{

		PointF[] points = null;
		byte[] types = null;

		public PathData()
		{
		}

		public PointF[] Points {
			get { return points; } 
			set { points = value; }
		}

		public byte[] Types {
			get { return types; }
			set { types = value; }
		}
	}
}
