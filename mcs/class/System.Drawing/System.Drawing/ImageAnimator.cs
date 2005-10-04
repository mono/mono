//
// System.Drawing.ImageAnimator.cs
//
// Author:
//   Dennis Hayes (dennish@Raytek.com)
//   Sanjay Gupta (gsanjay@novell.com)
//
// (C) 2002 Ximian, Inc
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Drawing.Imaging;
using System.Threading;
using System.Collections;

namespace System.Drawing
{
	//AnimateEventArgs class
	class AnimateEventArgs : EventArgs 
	{  
		private int frameCount;
		private int activeFrameCount = 0;
		private Thread thread;
      
		//Constructor.
		//
		public AnimateEventArgs(Image img)
		{
			Guid[] dimensionList = img.FrameDimensionsList;
			int length = dimensionList.Length;
			for (int i=0; i<length; i++) {
				if (dimensionList [i].Equals(FrameDimension.Time.Guid))
					this.frameCount = img.GetFrameCount (FrameDimension.Time);
			}			
		}
      
		public int FrameCount {     
			get { 
				return frameCount;
			}      
		}
      
		public int ActiveFrameCount {
			get {
				return activeFrameCount;
			}

			set {
				activeFrameCount = value;
			}
		}

		public Thread RunThread{
			get {
				return thread;
			}

			set {
				thread = value;
			}
		}
	}

	/// <summary>
	/// Summary description for ImageAnimator.
	/// </summary>
	/// 
	
	public sealed class ImageAnimator
	{
		static Hashtable ht = new Hashtable (); 
		
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
			
			if (!ht.ContainsKey (img)) {
				AnimateEventArgs evtArgs = new AnimateEventArgs (img);
				WorkerThread WT = new WorkerThread(onFrameChangeHandler, evtArgs);
				ThreadStart TS = new ThreadStart(WT.LoopHandler);	
				Thread thread = new Thread(TS);
				thread.IsBackground = true;
				evtArgs.RunThread = thread;
				ht.Add (img, evtArgs);
				
				thread.Start();				
			}
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
			int frameCount;
			for (int i=0; i<length; i++) 
			{
				if (dimensionList [i].Equals(FrameDimension.Time.Guid)) 
				{
					frameCount = img.GetFrameCount (FrameDimension.Time);
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

			if (ht.ContainsKey (img)) {
				AnimateEventArgs evtArgs = (AnimateEventArgs) ht [img];
				evtArgs.RunThread.Abort ();
				ht.Remove (img);
			}				
		}

		public static void UpdateFrames ()
		{
			foreach (Image img in ht.Keys) {
				UpdateFrames (img);
			}
		}
		
		public static void UpdateFrames (Image img)
		{
			if (img == null)
				throw new System.NullReferenceException ("Object reference not set to an instance of an object.");

			if (ht.ContainsKey (img)){
				//Need a way to get the delay during animation
				AnimateEventArgs evtArgs = (AnimateEventArgs) ht [img];
				if (evtArgs.ActiveFrameCount < evtArgs.FrameCount-1){
					evtArgs.ActiveFrameCount ++;
					img.SelectActiveFrame (FrameDimension.Time, evtArgs.ActiveFrameCount);
				} 
				else
					evtArgs.ActiveFrameCount = 0;
				ht [img] = evtArgs;
			}			
		}
	}

	class WorkerThread
	{
		private EventHandler frameChangeHandler;
		private AnimateEventArgs animateEventArgs;
				
		public WorkerThread(EventHandler frmChgHandler, AnimateEventArgs aniEvtArgs)
		{
			frameChangeHandler = frmChgHandler;
			animateEventArgs = aniEvtArgs;
		}
    
		public void LoopHandler()
		{
			try
			{
				while (true) {
					//Need a way to get the delay during animation
					Thread.Sleep (100);
					frameChangeHandler (null, animateEventArgs);
				}				
			}
			catch(ThreadAbortException)
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
