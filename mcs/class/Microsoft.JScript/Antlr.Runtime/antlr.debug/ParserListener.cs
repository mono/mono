namespace antlr.debug
{
	using System;
	
	public interface ParserListener : SemanticPredicateListener, ParserMatchListener, MessageListener, ParserTokenListener, TraceListener, SyntacticPredicateListener
	{
	}
}