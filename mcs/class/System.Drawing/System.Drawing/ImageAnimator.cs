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
using System.Threading;

namespace System.Drawing
{
	/// <summary>
	/// Summary description for ImageAnimator.
	/// </summary>
	public sealed class ImageAnimator
	{
		static private bool isAnimating = false;
		static private Thread thread;
		static private int activeFrame;

		private ImageAnimator ()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		public static void Animate (Image img, EventHandler onFrameChangeHandler)
		{
			if (img == null)
				throw new System.NullReferenceException ("Object reference not set to an instance of an object.");

			if (!isAnimating) {
				int frameCount = img.GetFrameCount (FrameDimension.Time);
				if (frameCount>1){
					isAnimating = true;
					//Start a new thread for looping within multiple frames of the image
					WorkerThread WT = new WorkerThread(img, new ActiveFrameCountCallBack(ResultCallback));
					ThreadStart TS = new ThreadStart(WT.LoopHandler);	
					thread = new Thread(TS);
					//Console.WriteLine ("Starting Thread");
					//MessageBox.Show ("Starting Thread");
					thread.Start();					
				}
							
			}
			//Console.WriteLine ("Started Thread");
			//MessageBox.Show ("Started Thread");
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
			for (int i=0; i<length; i++){
				if (dimensionList [i].Equals(FrameDimension.Time.Guid)){
					int frameCount = img.GetFrameCount (FrameDimension.Time);
					if (frameCount > 1)
						return true;
				}
			}			

			return false; 
		}

		public static void StopAnimate (Image img, EventHandler onFrameChangeHandler)
		{
			if (img == null)
				throw new System.NullReferenceException ("Object reference not set to an instance of an object.");
			//Console.WriteLine ("Stopping animation");
			//MessageBox.Show ("Stopping animation");
			if (isAnimating) {
				isAnimating = false;
				thread.Abort ();
				//Thread.CurrentThread.Join ();
				//Console.WriteLine ("Thread Joined");
				//MessageBox.Show ("Thread Joined");
			}
			
		}

		[MonoTODO ("Implement")]
		public static void UpdateFrames ()
		{
			throw new NotImplementedException (); 
		}
		
		[MonoTODO ("Implement")]
		public static void UpdateFrames (Image img)
		{
			/* Not sure as to what else needs to be done here.
			  I have updated the frame thats what i can think of
			  It surely requires something else also, as my application
			  shows only a static image */

			throw new NotImplementedException();
		}	

		// The callback method must match the signature of the
		// callback delegate.
		//
		static void ResultCallback(int activeFrameCount) 
		{
			activeFrame = activeFrameCount;
		}

	}

	// Delegate that defines the signature for the callback method.
	//
	delegate void ActiveFrameCountCallBack(int lineCount);

	class WorkerThread
	{
		private Image image;
		private int activeFrameCount;
		private int frameCount;
		private ActiveFrameCountCallBack afc;
				
		public WorkerThread(Image img, ActiveFrameCountCallBack afCount)
		{
			//Console.WriteLine ("Worker Thread Constructor");
			//MessageBox.Show ("Worker Thread Constructor");
			image = img;			
			frameCount = img.GetFrameCount (FrameDimension.Time);
			afc = afCount;
		}
    
		public void LoopHandler()
		{
			try
			{
				//Console.WriteLine ("Came in loop handler");
				//MessageBox.Show ("Came in loop handler");
				while (true)
				{
					//Need a way to get the delay during animation
					Thread.Sleep (100);
					activeFrameCount++;
					if (activeFrameCount > frameCount)
						activeFrameCount = 0;
					if (afc != null)
						afc (activeFrameCount);

				}
				//Console.WriteLine ("Exiting loop handler");
				//MessageBox.Show ("Exiting loop handler");
			}
			catch(ThreadAbortException tae)
			{ 
				//lets not bother ourselves with tae
				//it will be thrown anyway
			}
			catch(Exception er)
			{
				throw er;
			}

		}
	}
}
