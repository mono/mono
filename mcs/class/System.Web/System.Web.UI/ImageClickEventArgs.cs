//
// System.Web.UI.ImageClickEventArgs.cs
//
// Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

using System;

namespace System.Web.UI {
	public sealed class ImageClickEventArgs : EventArgs
	{
		public ImageClickEventArgs (int x, int y)
		{
			X = x;
			Y = y;
		}
		
		public int X;
		public int Y;
	}
}

 
