namespace antlr.debug
{
	using System;
	using System.Threading;
	using antlr.collections.impl;
	
	public class LLkDebuggingParser : LLkParser, DebuggingParser
	{
		private void  InitBlock()
		{
			parserEventSupport = new ParserEventSupport(this);
		}
		public override void setDebugMode(bool mode)
		{
			_notDebugMode = !mode;
		}
		protected internal ParserEventSupport parserEventSupport;
		
		private bool _notDebugMode = false;
		protected internal string[] ruleNames;
		protected internal string[] semPredNames;
		
		
		public LLkDebuggingParser(int k_):base(k_)
		{
			InitBlock();
		}
		public LLkDebuggingParser(ParserSharedInputState state, int k_):base(state, k_)
		{
			InitBlock();
		}
		public LLkDebuggingParser(TokenBuffer tokenBuf, int k_):base(tokenBuf, k_)
		{
			InitBlock();
		}
		public LLkDebuggingParser(TokenStream lexer, int k_):base(lexer, k_)
		{
			InitBlock();
		}
		public override void  addMessageListener(MessageListener l)
		{
			parserEventSupport.addMessageListener(l);
		}
		public override void  addParserListener(ParserListener l)
		{
			parserEventSupport.addParserListener(l);
		}
		public override void  addParserMatchListener(ParserMatchListener l)
		{
			parserEventSupport.addParserMatchListener(l);
		}
		public override void  addParserTokenListener(ParserTokenListener l)
		{
			parserEventSupport.addParserTokenListener(l);
		}
		public override void  addSemanticPredicateListener(SemanticPredicateListener l)
		{
			parserEventSupport.addSemanticPredicateListener(l);
		}
		public override void  addSyntacticPredicateListener(SyntacticPredicateListener l)
		{
			parserEventSupport.addSyntacticPredicateListener(l);
		}
		public override void  addTraceListener(TraceListener l)
		{
			parserEventSupport.addTraceListener(l);
		}
		/// <summary>Get another token object from the token stream 
		/// </summary>
		public override void  consume()
		{
			int la_1 = - 99;
			try
			{
				la_1 = LA(1);
			}
			catch (TokenStreamException)
			{
			}
			base.consume();
			parserEventSupport.fireConsume(la_1);
		}
		protected internal virtual void  fireEnterRule(int num, int data)
		{
			if (isDebugMode())
				parserEventSupport.fireEnterRule(num, inputState.guessing, data);
		}
		protected internal virtual void  fireExitRule(int num, int data)
		{
			if (isDebugMode())
				parserEventSupport.fireExitRule(num, inputState.guessing, data);
		}
		protected internal virtual bool fireSemanticPredicateEvaluated(int type, int num, bool condition)
		{
			if (isDebugMode())
				return parserEventSupport.fireSemanticPredicateEvaluated(type, num, condition, inputState.guessing);
			else
				return condition;
		}
		protected internal virtual void  fireSyntacticPredicateFailed()
		{
			if (isDebugMode())
				parserEventSupport.fireSyntacticPredicateFailed(inputState.guessing);
		}
		protected internal virtual void  fireSyntacticPredicateStarted()
		{
			if (isDebugMode())
				parserEventSupport.fireSyntacticPredicateStarted(inputState.guessing);
		}
		protected internal virtual void  fireSyntacticPredicateSucceeded()
		{
			if (isDebugMode())
				parserEventSupport.fireSyntacticPredicateSucceeded(inputState.guessing);
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
		public override bool isDebugMode()
		{
			return !_notDebugMode;
		}
		public virtual bool isGuessing()
		{
			return inputState.guessing > 0;
		}
		/// <summary>Return the token type of the ith token of lookahead where i=1
		/// is the current token being examined by the parser (i.e., it
		/// has not been matched yet).
		/// </summary>
		public override int LA(int i)
		{
			int la = base.LA(i);
			parserEventSupport.fireLA(i, la);
			return la;
		}
		/// <summary>Make sure current lookahead symbol matches token type <tt>t</tt>.
		/// Throw an exception upon mismatch, which is catch by either the
		/// error handler or by the syntactic predicate.
		/// </summary>
		public override void  match(int t)
		{
			string text = LT(1).getText();
			int la_1 = LA(1);
			try
			{
				base.match(t);
				parserEventSupport.fireMatch(t, text, inputState.guessing);
			}
			catch (MismatchedTokenException e)
			{
				if (inputState.guessing == 0)
					parserEventSupport.fireMismatch(la_1, t, text, inputState.guessing);
				throw e;
			}
		}
		/// <summary>Make sure current lookahead symbol matches the given set
		/// Throw an exception upon mismatch, which is catch by either the
		/// error handler or by the syntactic predicate.
		/// </summary>
		public override void  match(BitSet b)
		{
			string text = LT(1).getText();
			int la_1 = LA(1);
			try
			{
				base.match(b);
				parserEventSupport.fireMatch(la_1, b, text, inputState.guessing);
			}
			catch (MismatchedTokenException e)
			{
				if (inputState.guessing == 0)
					parserEventSupport.fireMismatch(la_1, b, text, inputState.guessing);
				throw e;
			}
		}
		public override void  matchNot(int t)
		{
			string text = LT(1).getText();
			int la_1 = LA(1);
			try
			{
				base.matchNot(t);
				parserEventSupport.fireMatchNot(la_1, t, text, inputState.guessing);
			}
			catch (MismatchedTokenException e)
			{
				if (inputState.guessing == 0)
					parserEventSupport.fireMismatchNot(la_1, t, text, inputState.guessing);
				throw e;
			}
		}
		public override void  removeMessageListener(MessageListener l)
		{
			parserEventSupport.removeMessageListener(l);
		}
		public override void  removeParserListener(ParserListener l)
		{
			parserEventSupport.removeParserListener(l);
		}
		public override void  removeParserMatchListener(ParserMatchListener l)
		{
			parserEventSupport.removeParserMatchListener(l);
		}
		public override void  removeParserTokenListener(ParserTokenListener l)
		{
			parserEventSupport.removeParserTokenListener(l);
		}
		public override void  removeSemanticPredicateListener(SemanticPredicateListener l)
		{
			parserEventSupport.removeSemanticPredicateListener(l);
		}
		public override void  removeSyntacticPredicateListener(SyntacticPredicateListener l)
		{
			parserEventSupport.removeSyntacticPredicateListener(l);
		}
		public override void  removeTraceListener(TraceListener l)
		{
			parserEventSupport.removeTraceListener(l);
		}
		/// <summary>Parser error-reporting function can be overridden in subclass 
		/// </summary>
		public override void  reportError(RecognitionException ex)
		{
			parserEventSupport.fireReportError(ex);
			base.reportError(ex);
		}
		/// <summary>Parser error-reporting function can be overridden in subclass 
		/// </summary>
		public override void  reportError(string s)
		{
			parserEventSupport.fireReportError(s);
			base.reportError(s);
		}
		/// <summary>Parser warning-reporting function can be overridden in subclass 
		/// </summary>
		public override void  reportWarning(string s)
		{
			parserEventSupport.fireReportWarning(s);
			base.reportWarning(s);
		}
		public virtual void  setupDebugging(TokenBuffer tokenBuf)
		{
			setupDebugging(null, tokenBuf);
		}
		public virtual void  setupDebugging(TokenStream lexer)
		{
			setupDebugging(lexer, null);
		}
		/// <summary>User can override to do their own debugging 
		/// </summary>
		protected internal virtual void  setupDebugging(TokenStream lexer, TokenBuffer tokenBuf)
		{
			setDebugMode(true);
			// default parser debug setup is ParseView
			try
			{
				try
				{
					System.Type.GetType("javax.swing.JButton");
				}
				catch (System.Exception)
				{
					System.Console.Error.WriteLine("Swing is required to use ParseView, but is not present in your CLASSPATH");
					System.Environment.Exit(1);
				}
				System.Type c = System.Type.GetType("antlr.parseview.ParseView");
				System.Reflection.ConstructorInfo constructor = c.GetConstructor(new System.Type[]{typeof(LLkDebuggingParser), typeof(TokenStream), typeof(TokenBuffer)});
				constructor.Invoke(new object[]{this, lexer, tokenBuf});
			}
			catch (System.Exception e)
			{
				System.Console.Error.WriteLine("Error initializing ParseView: " + e);
				System.Console.Error.WriteLine("Please report this to Scott Stanchfield, thetick@magelang.com");
				System.Environment.Exit(1);
			}
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