//
// System.Drawing.SolidBrush.cs
//
// Author:
//   Dennis Hayes (dennish@Raytek.com)
//   Alexandre Pigolkine(pigolkine@gmx.de)
//
// (C) 2002 Ximian, Inc
//
using System;

namespace System.Drawing
{
	namespace Win32Impl {

		internal class SolidBrushFactory : ISolidBrushFactory {
			ISolidBrush ISolidBrushFactory.SolidBrush(Color color){
				return new SolidBrush(color);
			}
		}

		internal class SolidBrush	: Brush, ISolidBrush 
		{
		
			Color color;

			public SolidBrush( Color color ) 
			{
				this.Color = color;
			}

			public Color Color 
			{
				get 
				{
					return color;
				}
				set 
				{
					color = value;
					if( hbrush_ != IntPtr.Zero) Win32.DeleteObject(hbrush_);
					int clr = Win32.RGB(color);
					hbrush_ = Win32.CreateSolidBrush(Win32.RGB(color));
				}
			}
		
			public override object Clone()
			{
				return new SolidBrush( color );
			}
		
			Color IBrush.TextColor 
			{
				get 
				{
					return Color;
				}
			}
		}
	}
}

