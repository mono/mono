//
// System.Drawing.Region.cs
//
// Author:
//	Dennis Hayes (dennish@raytek.com)
// (C) 2002 Ximian, Inc
//
using System;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace System.Drawing
{
	[ComVisible(false)]
	public sealed class Region : MarshalByRefObject, IDisposable
	{
		public Region()
		{
			
		}
		
		public Region (Rectangle rect)
		{

		}

		[ComVisible(false)]
		public Region Clone()
		{
			return this;
		}

		public void Dispose ()
		{
		}
	}
}
