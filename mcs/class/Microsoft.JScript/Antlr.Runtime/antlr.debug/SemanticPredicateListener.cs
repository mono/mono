namespace antlr.debug
{
	using System;
	
	public interface SemanticPredicateListener : Listener
	{
		void  semanticPredicateEvaluated(object source, SemanticPredicateEventArgs e);
	}
}