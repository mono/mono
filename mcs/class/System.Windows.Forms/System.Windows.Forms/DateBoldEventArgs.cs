//
// System.Windows.Forms.DateBoldEventArgs
//
// Author:
//   stubbed out by Richard Baumann (biochem333@nyc.rr.com)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) Ximian, Inc., 2002
//

namespace System.Windows.Forms {

	// <summary>
	//	This class is internal to the .NET framework.
	// </summary>
	public class DateBoldEventArgs : EventArgs {

		private int size;
		private DateTime start;
		private int [] daysToBold;

		//
		//  --- Public Properties
		//

		DateBoldEventArgs(DateTime start, int size, int [] daysToBold){
			this.size = size;
			this.start = start;
			this.daysToBold = daysToBold;
		}

		public int[] DaysToBold {

			get { return daysToBold; }
			set { daysToBold = value; }
		}

		public int Size {
			get { return size; }
		}

		public DateTime StartDate {

			get { return start; }
		}
	}
}
