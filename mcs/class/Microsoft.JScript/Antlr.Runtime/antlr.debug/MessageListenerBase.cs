namespace antlr.debug
{
	using System;
	
	/// <summary>
	/// Provides an abstract base for implementing <see cref="MessageListener"/> subclasses.
	/// </summary>
	/// <remarks>
	///		<param>
	///		This abstract class is provided to make it easier to create <see cref="MessageListener"/>s. 
	///		You should extend this base class rather than creating your own.
	///		</param>
	/// </remarks>
	public class MessageListenerBase : MessageListener
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
		/// Handle the "ReportError" event.
		/// </summary>
		/// <param name="source">Event source object</param>
		/// <param name="e">Event data object</param>
		public virtual void  reportError(object source, MessageEventArgs e)
		{
		}
		
		/// <summary>
		/// Handle the "ReportWarning" event.
		/// </summary>
		/// <param name="source">Event source object</param>
		/// <param name="e">Event data object</param>
		public virtual void  reportWarning(object source, MessageEventArgs e)
		{
		}
	}
}