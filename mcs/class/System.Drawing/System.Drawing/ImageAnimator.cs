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
using System.Drawing.Imaging;

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

		public static bool CanAnimate (Image img)
		{
			//An image can animate if it has multiple frame in
			//time based FrameDimension else return false
			//Doubt what if the image has multiple frame in page
			//based FrameDimension
			if (img == null)
				return false;

			//Need to check whether i can do this without iterating
			//within the FrameDimensionsList, ie just call GetFrameCount
			//with parameter FrameDimension.Time
			Guid[] dimensionList = img.FrameDimensionsList;
			int length = dimensionList.Length;
			for (int i=0; i<length; i++)
			{
				Guid dimension = dimensionList[i];
				if (dimension.Equals (FrameDimension.Time))
				{
					int frameCount = img.GetFrameCount (FrameDimension.Time);
					if (frameCount > 1)
						return true;
				}
			}			

			return false; 
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
