//
// System.Drawing.Drawing2D.GraphicsContainer.cs
//
// Authors:
//   Dennis Hayes (dennish@Raytek.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002/3 Ximian, Inc
//
using System;

namespace System.Drawing.Drawing2D {
	/// <summary>
	/// Summary description for GraphicsContainer.
	/// </summary>
	public sealed class GraphicsContainer : MarshalByRefObject {
		
		internal int nativeState = 0;
		
		private GraphicsContainer ()
		{
			
		}
		
		internal GraphicsContainer (int state)
		{
			nativeState = state;
		}
		
		internal int NativeObject{            
				get{
						return nativeState;
				}
				set	{
						nativeState = value;
				}
			}
	}
}
