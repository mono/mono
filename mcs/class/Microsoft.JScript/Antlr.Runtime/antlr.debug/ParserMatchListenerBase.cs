namespace antlr.debug
{
	using System;
	
	/// <summary>
	/// Provides an abstract base for implementing <see cref="ParserMatchListener"/> subclasses.
	/// </summary>
	/// <remarks>
	///		<param>
	///		This abstract class is provided to make it easier to create <see cref="ParserMatchListener"/>s. 
	///		You should extend this base class rather than creating your own.
	///		</param>
	/// </remarks>
	public abstract class ParserMatchListenerBase : ParserMatchListener
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
		/// Handle the "Match" event.
		/// </summary>
		/// <param name="source">Event source object</param>
		/// <param name="e">Event data object</param>
		public virtual void  parserMatch(object source, MatchEventArgs e)
		{
		}

		/// <summary>
		/// Handle the "MatchNot" event.
		/// </summary>
		/// <param name="source">Event source object</param>
		/// <param name="e">Event data object</param>
		public virtual void  parserMatchNot(object source, MatchEventArgs e)
		{
		}
		
		/// <summary>
		/// Handle the "MisMatch" event.
		/// </summary>
		/// <param name="source">Event source object</param>
		/// <param name="e">Event data object</param>
		public virtual void  parserMismatch(object source, MatchEventArgs e)
		{
		}
		
		/// <summary>
		/// Handle the "MisMatchNot" event.
		/// </summary>
		/// <param name="source">Event source object</param>
		/// <param name="e">Event data object</param>
		public virtual void  parserMismatchNot(object source, MatchEventArgs e)
		{
		}

		public virtual void  refresh()
		{
		}
	}
}