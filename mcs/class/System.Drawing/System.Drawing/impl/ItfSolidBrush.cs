//
// System.Drawing.SolidBrush.cs
//
// Author:
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc
//
using System;

namespace System.Drawing
{
	internal interface ISolidBrushFactory {
		ISolidBrush SolidBrush(Color color);
	}

	internal interface ISolidBrush : IBrush {

		Color Color {
			get ;
			set ;
		}
		
	}
}

