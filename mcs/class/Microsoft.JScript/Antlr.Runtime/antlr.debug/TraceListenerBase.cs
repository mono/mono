namespace antlr.debug
{
	using System;

	/// <summary>
	/// Provides an abstract base for implementing <see cref="TraceListener"/> subclasses.
	/// </summary>
	/// <remarks>
	///		<param>
	///		This abstract class is provided to make it easier to create <see cref="TraceListener"/>s. 
	///		You should extend this base class rather than creating your own.
	///		</param>
	/// </remarks>
	public abstract class TraceListenerBase : TraceListener
	{
		/// <summary>
		/// Handle the "Done" event.
		/// </summary>
		/// <param name="source">Event source object</param>
		/// <param name="e">Event data object</param>
		public virtual void  doneParsing(object source, TraceEventArgs e)
		{
		}

		/// <summary>
		/// Handle the "EnterRule" event
		/// </summary>
		/// <param name="source">Event source object</param>
		/// <param name="e">Event data object</param>
		public virtual void  enterRule(object source, TraceEventArgs e)
		{
		}

		/// <summary>
		/// Handle the "ExitRule" event
		/// </summary>
		/// <param name="source">Event source object</param>
		/// <param name="e">Event data object</param>
		public virtual void  exitRule(object source, TraceEventArgs e)
		{
		}

		public virtual void  refresh()
		{
		}
	}
}