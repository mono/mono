namespace antlr.debug
{
	using System;
	using System.Reflection;
	using Hashtable		= System.Collections.Hashtable;
	using ArrayList		= System.Collections.ArrayList;

	using antlr.collections.impl;
	
	
	/// <summary>A class to assist in firing parser events
	/// NOTE: I intentionally _did_not_ synchronize the event firing and
	/// add/remove listener methods.  This is because the add/remove should
	/// _only_ be called by the parser at its start/end, and the _same_thread_
	/// should be performing the parsing.  This should help performance a tad...
	/// </summary>
	public class ScannerEventSupport
	{
		private object source;
		private Hashtable listeners;
		private MatchEventArgs matchEvent;
		private MessageEventArgs messageEvent;
		private TokenEventArgs tokenEvent;
		private SemanticPredicateEventArgs semPredEvent;
		private SyntacticPredicateEventArgs synPredEvent;
		private TraceEventArgs traceEvent;
		private NewLineEventArgs newLineEvent;
		//private ParserController controller;

		private int ruleDepth = 0;
		
		
		public ScannerEventSupport(object source)
		{
			matchEvent		= new MatchEventArgs();
			messageEvent	= new MessageEventArgs();
			tokenEvent		= new TokenEventArgs();
			traceEvent		= new TraceEventArgs();
			semPredEvent	= new SemanticPredicateEventArgs();
			synPredEvent	= new SyntacticPredicateEventArgs();
			newLineEvent	= new NewLineEventArgs();
			listeners		= new Hashtable();
			this.source		= source;
		}

		public virtual void checkController()
		{
			//if (controller != null)
			//	controller.checkBreak();
		}

		public virtual void  addDoneListener(Listener l)
		{
			((CharScanner)source).Done += new TraceEventHandler(l.doneParsing);
			listeners[l] = l;
		}
		public virtual void  addMessageListener(MessageListener l)
		{
			((CharScanner)source).ErrorReported   += new MessageEventHandler(l.reportError);
			((CharScanner)source).WarningReported += new MessageEventHandler(l.reportWarning);
			addDoneListener(l);
		}
		public virtual void  addNewLineListener(NewLineListener l)
		{
			((CharScanner)source).HitNewLine   += new NewLineEventHandler(l.hitNewLine);
			addDoneListener(l);
		}
		public virtual void  addParserListener(ParserListener l)
		{
			if (l is ParserController)
			{
				//((ParserController) l).ParserEventSupport = this;
				//controller = (ParserController) l;
			}
			addParserMatchListener(l);
			addParserTokenListener(l);
			
			addMessageListener(l);
			addTraceListener(l);
			addSemanticPredicateListener(l);
			addSyntacticPredicateListener(l);
		}
		public virtual void  addParserMatchListener(ParserMatchListener l)
		{
			((CharScanner)source).MatchedChar		+= new MatchEventHandler(l.parserMatch);
			((CharScanner)source).MatchedNotChar	+= new MatchEventHandler(l.parserMatchNot);
			((CharScanner)source).MisMatchedChar	+= new MatchEventHandler(l.parserMismatch);
			((CharScanner)source).MisMatchedNotChar	+= new MatchEventHandler(l.parserMismatchNot);
			addDoneListener(l);
		}
		public virtual void  addParserTokenListener(ParserTokenListener l)
		{
			((CharScanner)source).ConsumedChar	+= new TokenEventHandler(l.parserConsume);
			((CharScanner)source).CharLA		+= new TokenEventHandler(l.parserLA);
			addDoneListener(l);
		}
		public virtual void  addSemanticPredicateListener(SemanticPredicateListener l)
		{
			((CharScanner)source).SemPredEvaluated	+= new SemanticPredicateEventHandler(l.semanticPredicateEvaluated);
			addDoneListener(l);
		}
		public virtual void  addSyntacticPredicateListener(SyntacticPredicateListener l)
		{
			((CharScanner)source).SynPredStarted		+= new SyntacticPredicateEventHandler(l.syntacticPredicateStarted);
			((CharScanner)source).SynPredFailed		+= new SyntacticPredicateEventHandler(l.syntacticPredicateFailed);
			((CharScanner)source).SynPredSucceeded	+= new SyntacticPredicateEventHandler(l.syntacticPredicateSucceeded);
			addDoneListener(l);
		}
		public virtual void  addTraceListener(TraceListener l)
		{
			((CharScanner)source).EnterRule	+= new TraceEventHandler(l.enterRule);
			((CharScanner)source).ExitRule	+= new TraceEventHandler(l.exitRule);
			addDoneListener(l);
		}
		public virtual void  fireConsume(int c)
		{
			TokenEventHandler eventDelegate = (TokenEventHandler)((CharScanner)source).Events[Parser.LAEventKey];
			if (eventDelegate != null) 
			{
				tokenEvent.setValues(TokenEventArgs.CONSUME, 1, c);
				eventDelegate(source, tokenEvent);
			}
			checkController();
		}
		public virtual void  fireDoneParsing()
		{
			TraceEventHandler eventDelegate = (TraceEventHandler)((CharScanner)source).Events[Parser.DoneEventKey];
			if (eventDelegate != null) 
			{
				traceEvent.setValues(TraceEventArgs.DONE_PARSING, 0, 0, 0);
				eventDelegate(source, traceEvent);
			}
			checkController();
		}
		public virtual void  fireEnterRule(int ruleNum, int guessing, int data)
		{
			ruleDepth++;
			TraceEventHandler eventDelegate = (TraceEventHandler)((CharScanner)source).Events[Parser.EnterRuleEventKey];
			if (eventDelegate != null) 
			{
				traceEvent.setValues(TraceEventArgs.ENTER, ruleNum, guessing, data);
				eventDelegate(source, traceEvent);
			}
			checkController();
		}
		public virtual void  fireExitRule(int ruleNum, int guessing, int data)
		{
			TraceEventHandler eventDelegate = (TraceEventHandler)((CharScanner)source).Events[Parser.ExitRuleEventKey];
			if (eventDelegate != null) 
			{
				traceEvent.setValues(TraceEventArgs.EXIT, ruleNum, guessing, data);
				eventDelegate(source, traceEvent);
			} 
			checkController();

			ruleDepth--;
			if (ruleDepth == 0)
				fireDoneParsing();

		}
		public virtual void  fireLA(int k, int la)
		{
			TokenEventHandler eventDelegate = (TokenEventHandler)((CharScanner)source).Events[Parser.LAEventKey];
			if (eventDelegate != null) 
			{
				tokenEvent.setValues(TokenEventArgs.LA, k, la);
				eventDelegate(source, tokenEvent);
			}
			checkController();
		}
		public virtual void  fireMatch(char c, int guessing)
		{
			MatchEventHandler eventDelegate = (MatchEventHandler)((CharScanner)source).Events[Parser.MatchEventKey];
			if (eventDelegate != null) 
			{
				matchEvent.setValues(MatchEventArgs.CHAR, c, c, null, guessing, false, true);
				eventDelegate(source, matchEvent);
			}
			checkController();
		}
		public virtual void  fireMatch(char c, BitSet b, int guessing)
		{
			MatchEventHandler eventDelegate = (MatchEventHandler)((CharScanner)source).Events[Parser.MatchEventKey];
			if (eventDelegate != null) 
			{
				matchEvent.setValues(MatchEventArgs.CHAR_BITSET, c, b, null, guessing, false, true);
				eventDelegate(source, matchEvent);
			}
			checkController();
		}
		public virtual void  fireMatch(char c, string target, int guessing)
		{
			MatchEventHandler eventDelegate = (MatchEventHandler)((CharScanner)source).Events[Parser.MatchEventKey];
			if (eventDelegate != null) 
			{
				matchEvent.setValues(MatchEventArgs.CHAR_RANGE, c, target, null, guessing, false, true);
				eventDelegate(source, matchEvent);
			}
			checkController();
		}
		public virtual void  fireMatch(int c, BitSet b, string text, int guessing)
		{
			MatchEventHandler eventDelegate = (MatchEventHandler)((CharScanner)source).Events[Parser.MatchEventKey];
			if (eventDelegate != null) 
			{
				matchEvent.setValues(MatchEventArgs.BITSET, c, b, text, guessing, false, true);
				eventDelegate(source, matchEvent);
			}
			checkController();
		}
		public virtual void  fireMatch(int n, string text, int guessing)
		{
			MatchEventHandler eventDelegate = (MatchEventHandler)((CharScanner)source).Events[Parser.MatchEventKey];
			if (eventDelegate != null) 
			{
				matchEvent.setValues(MatchEventArgs.TOKEN, n, n, text, guessing, false, true);
				eventDelegate(source, matchEvent);
			}
			checkController();
		}
		public virtual void  fireMatch(string s, int guessing)
		{
			MatchEventHandler eventDelegate = (MatchEventHandler)((CharScanner)source).Events[Parser.MatchEventKey];
			if (eventDelegate != null) 
			{
				matchEvent.setValues(MatchEventArgs.STRING, 0, s, null, guessing, false, true);
				eventDelegate(source, matchEvent);
			}
			checkController();
		}
		public virtual void  fireMatchNot(char c, char n, int guessing)
		{
			MatchEventHandler eventDelegate = (MatchEventHandler)((CharScanner)source).Events[Parser.MatchNotEventKey];
			if (eventDelegate != null) 
			{
				matchEvent.setValues(MatchEventArgs.CHAR, c, n, null, guessing, true, true);
				eventDelegate(source, matchEvent);
			}
			checkController();
		}
		public virtual void  fireMatchNot(int c, int n, string text, int guessing)
		{
			MatchEventHandler eventDelegate = (MatchEventHandler)((CharScanner)source).Events[Parser.MatchNotEventKey];
			if (eventDelegate != null) 
			{
				matchEvent.setValues(MatchEventArgs.TOKEN, c, n, text, guessing, true, true);
				eventDelegate(source, matchEvent);
			}
			checkController();
		}
		public virtual void  fireMismatch(char c, char n, int guessing)
		{
			MatchEventHandler eventDelegate = (MatchEventHandler)((CharScanner)source).Events[Parser.MisMatchEventKey];
			if (eventDelegate != null) 
			{
				matchEvent.setValues(MatchEventArgs.CHAR, c, n, null, guessing, false, false);
				eventDelegate(source, matchEvent);
			}
			checkController();
		}
		public virtual void  fireMismatch(char c, BitSet b, int guessing)
		{
			MatchEventHandler eventDelegate = (MatchEventHandler)((CharScanner)source).Events[Parser.MisMatchEventKey];
			if (eventDelegate != null) 
			{
				matchEvent.setValues(MatchEventArgs.CHAR_BITSET, c, b, null, guessing, false, true);
				eventDelegate(source, matchEvent);
			}
			checkController();
		}
		public virtual void  fireMismatch(char c, string target, int guessing)
		{
			MatchEventHandler eventDelegate = (MatchEventHandler)((CharScanner)source).Events[Parser.MisMatchEventKey];
			if (eventDelegate != null) 
			{
				matchEvent.setValues(MatchEventArgs.CHAR_RANGE, c, target, null, guessing, false, true);
				eventDelegate(source, matchEvent);
			}
			checkController();
		}
		public virtual void  fireMismatch(int i, int n, string text, int guessing)
		{
			MatchEventHandler eventDelegate = (MatchEventHandler)((CharScanner)source).Events[Parser.MisMatchEventKey];
			if (eventDelegate != null) 
			{
				matchEvent.setValues(MatchEventArgs.TOKEN, i, n, text, guessing, false, false);
				eventDelegate(source, matchEvent);
			}
			checkController();
		}
		public virtual void  fireMismatch(int i, BitSet b, string text, int guessing)
		{
			MatchEventHandler eventDelegate = (MatchEventHandler)((CharScanner)source).Events[Parser.MisMatchEventKey];
			if (eventDelegate != null) 
			{
				matchEvent.setValues(MatchEventArgs.BITSET, i, b, text, guessing, false, true);
				eventDelegate(source, matchEvent);
			}
			checkController();
		}
		public virtual void  fireMismatch(string s, string text, int guessing)
		{
			MatchEventHandler eventDelegate = (MatchEventHandler)((CharScanner)source).Events[Parser.MisMatchEventKey];
			if (eventDelegate != null) 
			{
				matchEvent.setValues(MatchEventArgs.STRING, 0, text, s, guessing, false, true);
				eventDelegate(source, matchEvent);
			}
			checkController();
		}
		public virtual void  fireMismatchNot(char v, char c, int guessing)
		{
			MatchEventHandler eventDelegate = (MatchEventHandler)((CharScanner)source).Events[Parser.MisMatchNotEventKey];
			if (eventDelegate != null) 
			{
				matchEvent.setValues(MatchEventArgs.CHAR, v, c, null, guessing, true, true);
				eventDelegate(source, matchEvent);
			}
			checkController();
		}
		public virtual void  fireMismatchNot(int i, int n, string text, int guessing)
		{
			MatchEventHandler eventDelegate = (MatchEventHandler)((CharScanner)source).Events[Parser.MisMatchNotEventKey];
			if (eventDelegate != null) 
			{
				matchEvent.setValues(MatchEventArgs.TOKEN, i, n, text, guessing, true, true);
				eventDelegate(source, matchEvent);
			}
			checkController();
		}
		public virtual void  fireNewLine(int line)
		{
			NewLineEventHandler eventDelegate = (NewLineEventHandler)((CharScanner)source).Events[Parser.NewLineEventKey];
			if (eventDelegate != null) 
			{
				newLineEvent.Line = line;
				eventDelegate(source, newLineEvent);
			}
			checkController();
		}
		public virtual void  fireReportError(System.Exception e)
		{
			MessageEventHandler eventDelegate = (MessageEventHandler)((CharScanner)source).Events[Parser.ReportErrorEventKey];
			if (eventDelegate != null) 
			{
				messageEvent.setValues(MessageEventArgs.ERROR, e.ToString());
				eventDelegate(source, messageEvent);
			}
			checkController();
		}
		public virtual void  fireReportError(string s)
		{
			MessageEventHandler eventDelegate = (MessageEventHandler)((CharScanner)source).Events[Parser.ReportErrorEventKey];
			if (eventDelegate != null) 
			{
				messageEvent.setValues(MessageEventArgs.ERROR, s);
				eventDelegate(source, messageEvent);
			}
			checkController();
		}
		public virtual void  fireReportWarning(string s)
		{
			MessageEventHandler eventDelegate = (MessageEventHandler)((CharScanner)source).Events[Parser.ReportWarningEventKey];
			if (eventDelegate != null) 
			{
				messageEvent.setValues(MessageEventArgs.WARNING, s);
				eventDelegate(source, messageEvent);
			}
			checkController();
		}
		public virtual bool fireSemanticPredicateEvaluated(int type, int condition, bool result, int guessing)
		{
			SemanticPredicateEventHandler eventDelegate = (SemanticPredicateEventHandler)((CharScanner)source).Events[Parser.SemPredEvaluatedEventKey];
			if (eventDelegate != null) 
			{
				semPredEvent.setValues(type, condition, result, guessing);
				eventDelegate(source, semPredEvent);
			}
			checkController();

			return result;
		}
		public virtual void  fireSyntacticPredicateFailed(int guessing)
		{
			SyntacticPredicateEventHandler eventDelegate = (SyntacticPredicateEventHandler)((CharScanner)source).Events[Parser.SynPredFailedEventKey];
			if (eventDelegate != null) 
			{
				synPredEvent.setValues(0, guessing);
				eventDelegate(source, synPredEvent);
			}
			checkController();
		}
		public virtual void  fireSyntacticPredicateStarted(int guessing)
		{
			SyntacticPredicateEventHandler eventDelegate = (SyntacticPredicateEventHandler)((CharScanner)source).Events[Parser.SynPredStartedEventKey];
			if (eventDelegate != null) 
			{
				synPredEvent.setValues(0, guessing);
				eventDelegate(source, synPredEvent);
			}
			checkController();
		}
		public virtual void  fireSyntacticPredicateSucceeded(int guessing)
		{
			SyntacticPredicateEventHandler eventDelegate = (SyntacticPredicateEventHandler)((CharScanner)source).Events[Parser.SynPredSucceededEventKey];
			if (eventDelegate != null) 
			{
				synPredEvent.setValues(0, guessing);
				eventDelegate(source, synPredEvent);
			}
			checkController();
		}
		public virtual void  refreshListeners()
		{
			Hashtable clonedTable;

			lock(listeners.SyncRoot)
			{
				clonedTable = (Hashtable)listeners.Clone();
			}
			foreach (Listener l in clonedTable)
			{
				l.refresh();
			}
		}
		public virtual void  removeDoneListener(Listener l)
		{
			((CharScanner)source).Done -= new TraceEventHandler(l.doneParsing);
			listeners.Remove(l);
		}
		public virtual void  removeMessageListener(MessageListener l)
		{
			((CharScanner)source).ErrorReported   -= new MessageEventHandler(l.reportError);
			((CharScanner)source).WarningReported -= new MessageEventHandler(l.reportWarning);
			removeDoneListener(l);
		}
		public virtual void  removeNewLineListener(NewLineListener l)
		{
			((CharScanner)source).HitNewLine   -= new NewLineEventHandler(l.hitNewLine);
			removeDoneListener(l);
		}
		public virtual void  removeParserListener(ParserListener l)
		{
			removeParserMatchListener(l);
			removeMessageListener(l);
			removeParserTokenListener(l);
			removeTraceListener(l);
			removeSemanticPredicateListener(l);
			removeSyntacticPredicateListener(l);
		}
		public virtual void  removeParserMatchListener(ParserMatchListener l)
		{
			((CharScanner)source).MatchedChar		-= new MatchEventHandler(l.parserMatch);
			((CharScanner)source).MatchedNotChar	-= new MatchEventHandler(l.parserMatchNot);
			((CharScanner)source).MisMatchedChar	-= new MatchEventHandler(l.parserMismatch);
			((CharScanner)source).MisMatchedNotChar	-= new MatchEventHandler(l.parserMismatchNot);
			removeDoneListener(l);
		}
		public virtual void  removeParserTokenListener(ParserTokenListener l)
		{
			((CharScanner)source).ConsumedChar	-= new TokenEventHandler(l.parserConsume);
			((CharScanner)source).CharLA		-= new TokenEventHandler(l.parserLA);
			removeDoneListener(l);
		}
		public virtual void  removeSemanticPredicateListener(SemanticPredicateListener l)
		{
			((CharScanner)source).SemPredEvaluated	-= new SemanticPredicateEventHandler(l.semanticPredicateEvaluated);
			removeDoneListener(l);
		}
		public virtual void  removeSyntacticPredicateListener(SyntacticPredicateListener l)
		{
			((CharScanner)source).SynPredStarted	-= new SyntacticPredicateEventHandler(l.syntacticPredicateStarted);
			((CharScanner)source).SynPredFailed		-= new SyntacticPredicateEventHandler(l.syntacticPredicateFailed);
			((CharScanner)source).SynPredSucceeded	-= new SyntacticPredicateEventHandler(l.syntacticPredicateSucceeded);
			removeDoneListener(l);
		}
		public virtual void  removeTraceListener(TraceListener l)
		{
			((CharScanner)source).EnterRule	-= new TraceEventHandler(l.enterRule);
			((CharScanner)source).ExitRule	-= new TraceEventHandler(l.exitRule);
			removeDoneListener(l);
		}
	}
}