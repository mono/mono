namespace antlr.debug
{
	using System;
	using System.Threading;
	using antlr;

	using BitSet	= antlr.collections.impl.BitSet;
	
	public abstract class DebuggingCharScanner : CharScanner, DebuggingParser
	{
		private void  InitBlock()
		{
			eventSupport = new ScannerEventSupport(this);
		}
		public virtual void setDebugMode(bool mode)
		{
			_notDebugMode = !mode;
		}

		private ScannerEventSupport eventSupport;
		private bool _notDebugMode = false;
		protected internal string[] ruleNames;
		protected internal string[] semPredNames;
		
		
		public DebuggingCharScanner(InputBuffer cb) : base(cb)
		{
			InitBlock();
		}
		public DebuggingCharScanner(LexerSharedInputState state) : base(state)
		{
			InitBlock();
		}
		public virtual void  addMessageListener(MessageListener l)
		{
			eventSupport.addMessageListener(l);
		}
		public virtual void  addNewLineListener(NewLineListener l)
		{
			eventSupport.addNewLineListener(l);
		}
		public virtual void  addParserListener(ParserListener l)
		{
			eventSupport.addParserListener(l);
		}
		public virtual void  addParserMatchListener(ParserMatchListener l)
		{
			eventSupport.addParserMatchListener(l);
		}
		public virtual void  addParserTokenListener(ParserTokenListener l)
		{
			eventSupport.addParserTokenListener(l);
		}
		public virtual void  addSemanticPredicateListener(SemanticPredicateListener l)
		{
			eventSupport.addSemanticPredicateListener(l);
		}
		public virtual void  addSyntacticPredicateListener(SyntacticPredicateListener l)
		{
			eventSupport.addSyntacticPredicateListener(l);
		}
		public virtual void  addTraceListener(TraceListener l)
		{
			eventSupport.addTraceListener(l);
		}
		public override void  consume()
		{
			int la_1 = - 99;
			try
			{
				la_1 = LA(1);
			}
			catch (CharStreamException)
			{
			}
			base.consume();
			eventSupport.fireConsume(la_1);
		}
		protected internal virtual void  fireEnterRule(int num, int data)
		{
			if (isDebugMode())
				eventSupport.fireEnterRule(num, inputState.guessing, data);
		}
		protected internal virtual void  fireExitRule(int num, int ttype)
		{
			if (isDebugMode())
				eventSupport.fireExitRule(num, inputState.guessing, ttype);
		}
		protected internal virtual bool fireSemanticPredicateEvaluated(int type, int num, bool condition)
		{
			if (isDebugMode())
				return eventSupport.fireSemanticPredicateEvaluated(type, num, condition, inputState.guessing);
			else
				return condition;
		}
		protected internal virtual void  fireSyntacticPredicateFailed()
		{
			if (isDebugMode())
				eventSupport.fireSyntacticPredicateFailed(inputState.guessing);
		}
		protected internal virtual void  fireSyntacticPredicateStarted()
		{
			if (isDebugMode())
				eventSupport.fireSyntacticPredicateStarted(inputState.guessing);
		}
		protected internal virtual void  fireSyntacticPredicateSucceeded()
		{
			if (isDebugMode())
				eventSupport.fireSyntacticPredicateSucceeded(inputState.guessing);
		}
		public virtual string getRuleName(int num)
		{
			return ruleNames[num];
		}
		public virtual string getSemPredName(int num)
		{
			return semPredNames[num];
		}
		public virtual void  goToSleep()
		{
			lock(this)
			{
				try
				{
					Monitor.Wait(this);
				}
				catch (System.Threading.ThreadInterruptedException)
				{
				}
			}
		}
		public virtual bool isDebugMode()
		{
			return !_notDebugMode;
		}
		public override char LA(int i)
		{
			char la = base.LA(i);
			eventSupport.fireLA(i, la);
			return la;
		}
		protected internal override Token makeToken(int t)
		{
			// do something with char buffer???
			//		try {
			//			Token tok = (Token)tokenObjectClass.newInstance();
			//			tok.setType(t);
			//			// tok.setText(getText()); done in generated lexer now
			//			tok.setLine(line);
			//			return tok;
			//		}
			//		catch (InstantiationException ie) {
			//			panic("can't instantiate a Token");
			//		}
			//		catch (IllegalAccessException iae) {
			//			panic("Token class is not accessible");
			//		}
			return base.makeToken(t);
		}
		public override void  match(int c)
		{
			char la_1 = LA(1);
			try
			{
				base.match(c);
				eventSupport.fireMatch(Convert.ToChar(c), inputState.guessing);
			}
			catch (MismatchedCharException e)
			{
				if (inputState.guessing == 0)
					eventSupport.fireMismatch(la_1, Convert.ToChar(c), inputState.guessing);
				throw e;
			}
		}
		public override void  match(BitSet b)
		{
			string text = this.text.ToString();
			char la_1 = LA(1);
			try
			{
				base.match(b);
				eventSupport.fireMatch(la_1, b, text, inputState.guessing);
			}
			catch (MismatchedCharException e)
			{
				if (inputState.guessing == 0)
					eventSupport.fireMismatch(la_1, b, text, inputState.guessing);
				throw e;
			}
		}
		public override void  match(string s)
		{
			System.Text.StringBuilder la_s = new System.Text.StringBuilder("");
			int len = s.Length;
			// peek at the next len worth of characters
			try
			{
				 for (int i = 1; i <= len; i++)
				{
					la_s.Append(base.LA(i));
				}
			}
			catch (System.Exception)
			{
			}
			
			try
			{
				base.match(s);
				eventSupport.fireMatch(s, inputState.guessing);
			}
			catch (MismatchedCharException e)
			{
				if (inputState.guessing == 0)
					eventSupport.fireMismatch(la_s.ToString(), s, inputState.guessing);
				throw e;
			}
			
		}
		public override void  matchNot(int c)
		{
			char la_1 = LA(1);
			try
			{
				base.matchNot(c);
				eventSupport.fireMatchNot(la_1, Convert.ToChar(c), inputState.guessing);
			}
			catch (MismatchedCharException e)
			{
				if (inputState.guessing == 0)
					eventSupport.fireMismatchNot(la_1, Convert.ToChar(c), inputState.guessing);
				throw e;
			}
			
		}
		public override void  matchRange(int c1, int c2)
		{
			char la_1 = LA(1);
			try
			{
				base.matchRange(c1, c2);
				eventSupport.fireMatch(la_1, "" + c1 + c2, inputState.guessing);
			}
			catch (MismatchedCharException e)
			{
				if (inputState.guessing == 0)
					eventSupport.fireMismatch(la_1, "" + c1 + c2, inputState.guessing);
				throw e;
			}
			
		}
		public override void  newline()
		{
			base.newline();
			eventSupport.fireNewLine(getLine());
		}
		public virtual void  removeMessageListener(MessageListener l)
		{
			eventSupport.removeMessageListener(l);
		}
		public virtual void  removeNewLineListener(NewLineListener l)
		{
			eventSupport.removeNewLineListener(l);
		}
		public virtual void  removeParserListener(ParserListener l)
		{
			eventSupport.removeParserListener(l);
		}
		public virtual void  removeParserMatchListener(ParserMatchListener l)
		{
			eventSupport.removeParserMatchListener(l);
		}
		public virtual void  removeParserTokenListener(ParserTokenListener l)
		{
			eventSupport.removeParserTokenListener(l);
		}
		public virtual void  removeSemanticPredicateListener(SemanticPredicateListener l)
		{
			eventSupport.removeSemanticPredicateListener(l);
		}
		public virtual void  removeSyntacticPredicateListener(SyntacticPredicateListener l)
		{
			eventSupport.removeSyntacticPredicateListener(l);
		}
		public virtual void  removeTraceListener(TraceListener l)
		{
			eventSupport.removeTraceListener(l);
		}
		/// <summary>Report exception errors caught in nextToken() 
		/// </summary>
		public virtual void  reportError(MismatchedCharException e)
		{
			eventSupport.fireReportError(e);
			base.reportError(e);
		}
		/// <summary>Parser error-reporting function can be overridden in subclass 
		/// </summary>
		public override void  reportError(string s)
		{
			eventSupport.fireReportError(s);
			base.reportError(s);
		}
		/// <summary>Parser warning-reporting function can be overridden in subclass 
		/// </summary>
		public override void  reportWarning(string s)
		{
			eventSupport.fireReportWarning(s);
			base.reportWarning(s);
		}
		public virtual void  setupDebugging()
		{
		}

		public virtual void  wakeUp()
		{
			lock(this)
			{
				Monitor.Pulse(this);
			}
		}
	}
}