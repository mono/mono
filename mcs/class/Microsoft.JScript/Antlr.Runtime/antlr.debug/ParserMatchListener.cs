namespace antlr.debug
{
	using System;
	
	public interface ParserMatchListener : Listener
	{
		void  parserMatch		(object source, MatchEventArgs e);
		void  parserMatchNot	(object source, MatchEventArgs e);
		void  parserMismatch	(object source, MatchEventArgs e);
		void  parserMismatchNot	(object source, MatchEventArgs e);
	}
}