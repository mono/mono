namespace antlr.debug
{
	using System;
	
	public interface SyntacticPredicateListener : Listener
	{
		void  syntacticPredicateFailed		(object source, SyntacticPredicateEventArgs e);
		void  syntacticPredicateStarted		(object source, SyntacticPredicateEventArgs e);
		void  syntacticPredicateSucceeded	(object source, SyntacticPredicateEventArgs e);
	}
}