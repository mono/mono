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

		#region Public Methods

		/// <summary>
		///	Equality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two DateRangeEventArgs objects.
		///	The return value is based on the equivalence of
		///	start and end Property
		///	of the two DateRangeEventArgs.
		/// </remarks>
		public static bool operator == (DateRangeEventArgs DateRangeEventArgsA, DateRangeEventArgs DateRangeEventArgsB) 
		{
			return (DateRangeEventArgsA.Start == DateRangeEventArgsB.Start) && 
				   (DateRangeEventArgsA.End == DateRangeEventArgsB.End);

		}
		
		/// <summary>
		///	Inequality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two DateRangeEventArgs objects.
		///	The return value is based on the equivalence of
		///	start and end Property
		///	of the two DateRangeEventArgs.
		/// </remarks>
		public static bool operator != (DateRangeEventArgs DateRangeEventArgsA, DateRangeEventArgs DateRangeEventArgsB) 
		{
			return (DateRangeEventArgsA.Start != DateRangeEventArgsB.Start) || 
				   (DateRangeEventArgsA.End != DateRangeEventArgsB.End);

		}

		/// <summary>
		///	Equals Method
		/// </summary>
		///
		/// <remarks>
		///	Checks equivalence of this
		///	DateRangeEventArgs and another
		///	object.
		/// </remarks>
		public override bool Equals (object obj) 
		{
			if (!(obj is DateRangeEventArgs))return false;
			return (this == (DateRangeEventArgs) obj);
		}

		/// <summary>
		///	GetHashCode Method
		/// </summary>
		///
		/// <remarks>
		///	Calculates a hashing value.
		/// </remarks>
		[MonoTODO]
		public override int GetHashCode () 
		{
			//FIXME: add class specific stuff;
			return base.GetHashCode();
		}

		/// <summary>
		///	ToString Method
		/// </summary>
		///
		/// <remarks>
		///	Formats the object as a string.
		/// </remarks>
		[MonoTODO]
		public override string ToString () 
		{
			//FIXME: add class specific stuff;
			return base.ToString() + " DateRangeEventArgs";
		}


		#endregion
	}
}
