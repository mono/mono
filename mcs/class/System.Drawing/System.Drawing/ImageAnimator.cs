//
// System.Drawing.ImageAnimator.cs
//
// Author:
//   Dennis Hayes (dennish@Raytek.com)
//   Sanjay Gupta (gsanjay@novell.com)
//
// (C) 2002 Ximian, Inc
//
using System;

namespace System.Drawing
{
	/// <summary>
	/// Summary description for ImageAnimator.
	/// </summary>
	public sealed class ImageAnimator
	{
		private ImageAnimator ()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		[MonoTODO ("Implement")]
		public static void Animate (Image img, EventHandler onFrameChangeHandler)
		{
			throw new NotImplementedException (); 
		}

		[MonoTODO ("Implement")]
		public static bool CanAnimate (Image img)
		{
			throw new NotImplementedException (); 
		}

		[MonoTODO ("Implement")]
		public static void StopAnimate (Image img, EventHandler onFrameChangeHandler)
		{
			throw new NotImplementedException (); 
		}

		[MonoTODO ("Implement")]
		public static void UpdateFrames ()
		{
			throw new NotImplementedException (); 
		}
		
		[MonoTODO ("Implement")]
		public static void UpdateFrames (Image img)
		{
			throw new NotImplementedException (); 
		}	
	}
}
