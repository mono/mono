//
// System.Windows.Forms.DateRangeEventArgs
//
// Author:
//   stubbed out by Richard Baumann (biochem333@nyc.rr.com)
//   Implemented by Richard Baumann <biochem333@nyc.rr.com>
// (C) Ximian, Inc., 2002
//

namespace System.Windows.Forms {

	/// <summary>
	///	Provides data for the DateChanged or DateSelected events of the MonthCalendar control.
	/// </summary>
	public class DateRangeEventArgs : EventArgs {

		//
		//  --- Private Fields
		//
		private DateTime end;
		private DateTime start;

		//
		//  --- Constructors/Destructors
		//
		public DateRangeEventArgs(DateTime start, DateTime end) : base()
		{
			this.start = start;
			this.end = end;
		}

		//
		//  --- Public Properties
		//
		public DateTime End {

			get { return end; }
		}
		[MonoTODO]
		public DateTime Start {

			get { return start; }
		}
	}
}
