namespace antlr.debug
{
	using System;
	
	/// <summary>
	/// Provides an abstract base for implementing <see cref="ParserListener"/> subclasses.
	/// </summary>
	/// <remarks>
	///		<param>
	///		This abstract class is provided to make it easier to create <see cref="ParserListener"/>s. 
	///		You should extend this base class rather than creating your own.
	///		</param>
	/// </remarks>
	public class ParserListenerBase : ParserListener
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
		
		/// <summary>
		/// Handle the "SemPreEvaluated" event.
		/// </summary>
		/// <param name="source">Event source object</param>
		/// <param name="e">Event data object</param>
		public virtual void  semanticPredicateEvaluated(object source, SemanticPredicateEventArgs e)
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

		public virtual void  refresh()
		{
		}
	}
}