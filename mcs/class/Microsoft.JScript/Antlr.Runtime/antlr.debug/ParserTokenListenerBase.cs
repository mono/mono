namespace antlr.debug
{
	using System;
	
	/// <summary>
	/// Provides an abstract base for implementing <see cref="ParserTokenListener"/> subclasses.
	/// </summary>
	/// <remarks>
	///		<param>
	///		This abstract class is provided to make it easier to create <see cref="ParserTokenListener"/>s. 
	///		You should extend this base class rather than creating your own.
	///		</param>
	/// </remarks>
	public abstract class ParserTokenListenerBase : ParserTokenListener
	{
		/// <summary>
		/// Handle the "Done" event.
		/// </summary>
		/// <param name="source">Event source object</param>
		/// <param name="e">Event data object</param>
		public virtual void  doneParsing(object source, TraceEventArgs e)
		{
		}

		public virtual void  refresh()
		{
		}

		/// <summary>
		/// Handle the "Consume" event.
		/// </summary>
		/// <param name="source">Event source object</param>
		/// <param name="e">Event data object</param>
		public virtual void  parserConsume(object source, TokenEventArgs e)
		{
		}

		/// <summary>
		/// Handle the "ParserLA" event.
		/// </summary>
		/// <param name="source">Event source object</param>
		/// <param name="e">Event data object</param>
		public virtual void  parserLA(object source, TokenEventArgs e)
		{
		}
	}
}