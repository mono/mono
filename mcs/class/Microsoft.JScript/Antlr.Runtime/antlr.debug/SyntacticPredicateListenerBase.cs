namespace antlr.debug
{
	using System;
	
	/// <summary>
	/// Provides an abstract base for implementing <see cref="SyntacticPredicateListener"/> subclasses.
	/// </summary>
	/// <remarks>
	///		<param>
	///		This abstract class is provided to make it easier to create <see cref="SyntacticPredicateListener"/>s. 
	///		You should extend this base class rather than creating your own.
	///		</param>
	/// </remarks>
	public abstract class SyntacticPredicateListenerBase : SyntacticPredicateListener
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
		/// Handle the "SynPredFailed" event.
		/// </summary>
		/// <param name="source">Event source object</param>
		/// <param name="e">Event data object</param>
		public virtual void  syntacticPredicateFailed(object source, SyntacticPredicateEventArgs e)
		{
		}

		/// <summary>
		/// Handle the "SynPredStarted" event.
		/// </summary>
		/// <param name="source">Event source object</param>
		/// <param name="e">Event data object</param>
		public virtual void  syntacticPredicateStarted(object source, SyntacticPredicateEventArgs e)
		{
		}
		
		/// <summary>
		/// Handle the "SynPredSucceeded" event.
		/// </summary>
		/// <param name="source">Event source object</param>
		/// <param name="e">Event data object</param>
		public virtual void  syntacticPredicateSucceeded(object source, SyntacticPredicateEventArgs e)
		{
		}
	}
}