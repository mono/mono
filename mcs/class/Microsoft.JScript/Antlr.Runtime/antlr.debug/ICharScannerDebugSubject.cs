namespace antlr.debug
{
	using System;
	
	public interface ICharScannerDebugSubject : IDebugSubject
	{
		event NewLineEventHandler				HitNewLine;
		event MatchEventHandler					MatchedChar;
		event MatchEventHandler					MatchedNotChar;
		event MatchEventHandler					MisMatchedChar;
		event MatchEventHandler					MisMatchedNotChar;
		event TokenEventHandler					ConsumedChar;
		event TokenEventHandler					CharLA;
	}
}
