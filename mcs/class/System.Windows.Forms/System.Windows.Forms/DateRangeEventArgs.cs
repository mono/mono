//
// System.Windows.Forms.DateRangeEventArgs
//
// Author:
//   stubbed out by Richard Baumann (biochem333@nyc.rr.com)
//   Implemented by Richard Baumann <biochem333@nyc.rr.com>
//   Dennis Hayes (dennish@Raytek.com)
//	 Gianandrea Terzi (gianandrea.terzi@lario.com)
//
// (C) Ximian, Inc., 2002
//

namespace System.Windows.Forms {

	/// <summary>
	///	Provides data for the DateChanged or DateSelected events of the MonthCalendar control.
	/// </summary>
	public class DateRangeEventArgs : EventArgs {

		#region Fields

		private DateTime end;
		private DateTime start;
		
		#endregion

		//
		//  --- Constructors/Destructors
		//
		public DateRangeEventArgs(DateTime start, DateTime end) : base()
		{
			this.start = start;
			this.end = end;
		}

		#region Public Properties

		public DateTime End 
		{
			get { 
					return end; 
			}
		}

		public DateTime Start {
			get { 
					return start; 
			}
		}

		#endregion
	}
}
