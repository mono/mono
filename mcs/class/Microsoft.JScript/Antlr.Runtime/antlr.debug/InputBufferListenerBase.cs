namespace antlr.debug
{
	using System;
	
	/// <summary>
	/// Provides an abstract base for implementing <see cref="InputBufferListener"/> subclasses.
	/// </summary>
	/// <remarks>
	///		<param>
	///		This abstract class is provided to make it easier to create <see cref="InputBufferListener"/>s. 
	///		You should extend this base class rather than creating your own.
	///		</param>
	/// </remarks>
	public abstract class InputBufferListenerBase : InputBufferListener
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
		/// Handle the "CharConsumed" event.
		/// </summary>
		/// <param name="source">Event source object</param>
		/// <param name="e">Event data object</param>
		public virtual void  inputBufferConsume(object source, InputBufferEventArgs e)
		{
		}

		/// <summary>
		/// Handle the "CharLA" event.
		/// </summary>
		/// <param name="source">Event source object</param>
		/// <param name="e">Event data object</param>
		public virtual void  inputBufferLA(object source, InputBufferEventArgs e)
		{
		}

		/// <summary>
		/// Handle the "Mark" event.
		/// </summary>
		/// <param name="source">Event source object</param>
		/// <param name="e">Event data object</param>
		public virtual void  inputBufferMark(object source, InputBufferEventArgs e)
		{
		}

		/// <summary>
		/// Handle the "Rewind" event.
		/// </summary>
		/// <param name="source">Event source object</param>
		/// <param name="e">Event data object</param>
		public virtual void  inputBufferRewind(object source, InputBufferEventArgs e)
		{
		}

		public virtual void  refresh()
		{
		}
	}
}