//
// System.Windows.Forms.XEventQueue
//
// Author(s):
//  Jackson Harper (jackson@ximian.com)
//
// Copyright (C) Novell, Inc (http://www.novell.com)
//

using System;
using System.Threading;
using System.Collections;


namespace System.Windows.Forms {

	internal class XEventQueue {

		private XEvent [] xevents;
		private XEvent [] lxevents;   // Events inserted from threads other then the main X thread

		private int xindex;
		private int lxindex;

		private static readonly int MinXEventSize = 50;
		private static readonly int MinLXEventSize = 10;

		public XEventQueue ()
		{
			xevents = new XEvent [MinXEventSize];
			xevents = new XEvent [MinLXEventSize];
		}

		public int Count {
			get { return xindex + lxindex; }
		}

		public void Enqueue (XEvent xevent)
		{
			EnqueueArray (xevent, ref xevents, ref xindex);
		}

		public void EnqueueLocked (XEvent xevent)
		{
			lock (lxevents) {
				EnqueueArray (xevent, ref lxevents, ref lxindex);
			}
		}

		public XEvent Dequeue ()
		{
			if (xindex == -1) {
				lock (lxevents) {
					if (lxindex == -1)
						throw new Exception ("No more items in XQueue");
					return lxevents [lxindex--];
				}
			}
			return xevents [xindex--];
		}

		public void CheckSize ()
		{
			CheckArraySize (ref xevents, MinXEventSize, xindex);

			lock (lxevents) {
				CheckArraySize (ref lxevents, MinLXEventSize, lxindex);
			}
		}

		public void CheckArraySize (ref XEvent [] array, int min, int index)
		{
			if (array.Length > min && index * 3 < array.Length) {
				XEvent [] na = new XEvent [min];
				Array.Copy (array, na, index);
			}
		}

		private void EnqueueArray (XEvent xevent, ref XEvent [] array, ref int index)
		{
			index++;
			if (index == array.Length) {
				XEvent [] na = new XEvent [array.Length * 2];
				Array.Copy (array, na, array.Length);
				array = na;
			}

			array [index] = xevent;
		}
	}
}

