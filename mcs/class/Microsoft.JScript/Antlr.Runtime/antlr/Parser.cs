using System;
using EventHandlerList			= System.ComponentModel.EventHandlerList;

using BitSet					= antlr.collections.impl.BitSet;
using AST						= antlr.collections.AST;
using ASTArray					= antlr.collections.impl.ASTArray;
using antlr.debug;

using MessageListener				= antlr.debug.MessageListener;
using ParserListener				= antlr.debug.ParserListener;
using ParserMatchListener			= antlr.debug.ParserMatchListener;
using ParserTokenListener			= antlr.debug.ParserTokenListener;
using SemanticPredicateListener		= antlr.debug.SemanticPredicateListener;
using SyntacticPredicateListener	= antlr.debug.SyntacticPredicateListener;
using TraceListener					= antlr.debug.TraceListener;

/*
	private Vector messageListeners;
	private Vector newLineListeners;
	private Vector matchListeners;
	private Vector tokenListeners;
	private Vector semPredListeners;
	private Vector synPredListeners;
	private Vector traceListeners;
*/
	
namespace antlr
{
	/*ANTLR Translator Generator
	* Project led by Terence Parr at http://www.jGuru.com
	* Software rights: http://www.antlr.org/RIGHTS.html
	*
	* $Id: Parser.cs,v 1.1 2003/04/22 04:56:13 cesar Exp $
	*/

	//
	// ANTLR C# Code Generator by Micheal Jordan
	//                            Kunle Odutola       : kunle UNDERSCORE odutola AT hotmail DOT com
	//                            Anthony Oguntimehin
	//
	// With many thanks to Eric V. Smith from the ANTLR list.
	//

	public abstract class Parser : IParserDebugSubject
	{
		// Used to store event delegates
		private EventHandlerList events_ = new EventHandlerList();

		protected internal EventHandlerList Events 
		{
			get	{ return events_;	}
		}

		// The unique keys for each event that Parser [objects] can generate
		internal static readonly object EnterRuleEventKey		= new object();
		internal static readonly object ExitRuleEventKey			= new object();
		internal static readonly object DoneEventKey				= new object();
		internal static readonly object ReportErrorEventKey		= new object();
		internal static readonly object ReportWarningEventKey	= new object();
		internal static readonly object NewLineEventKey			= new object();
		internal static readonly object MatchEventKey			= new object();
		internal static readonly object MatchNotEventKey			= new object();
		internal static readonly object MisMatchEventKey			= new object();
		internal static readonly object MisMatchNotEventKey		= new object();
		internal static readonly object ConsumeEventKey			= new object();
		internal static readonly object LAEventKey				= new object();
		internal static readonly object SemPredEvaluatedEventKey	= new object();
		internal static readonly object SynPredStartedEventKey	= new object();
		internal static readonly object SynPredFailedEventKey	= new object();
		internal static readonly object SynPredSucceededEventKey	= new object();

		protected internal ParserSharedInputState inputState;
		
		/*Nesting level of registered handlers */
		// protected int exceptionLevel = 0;
		
		/*Table of token type to token names */
		protected internal string[] tokenNames;
		
		/*AST return value for a rule is squirreled away here */
		protected internal AST returnAST;
		
		/*AST support code; parser and treeparser delegate to this object */
		protected internal ASTFactory astFactory = new ASTFactory();
		
		private bool ignoreInvalidDebugCalls = false;
		
		/*Used to keep track of indentdepth for traceIn/Out */
		protected internal int traceDepth = 0;
		
		public Parser()
		{
			inputState = new ParserSharedInputState();
		}
		
		public Parser(ParserSharedInputState state)
		{
			inputState = state;
		}
		
		/// <summary>
		/// 
		/// </summary>

		public event TraceEventHandler EnterRule
		{
			add		{	Events.AddHandler(EnterRuleEventKey, value);	}
			remove	{	Events.RemoveHandler(EnterRuleEventKey, value);	}
		}

		public event TraceEventHandler ExitRule
		{
			add		{	Events.AddHandler(ExitRuleEventKey, value);		}
			remove	{	Events.RemoveHandler(ExitRuleEventKey, value);	}
		}

		public event TraceEventHandler Done
		{
			add		{	Events.AddHandler(DoneEventKey, value);		}
			remove	{	Events.RemoveHandler(DoneEventKey, value);	}
		}

		public event MessageEventHandler ErrorReported
		{
			add		{	Events.AddHandler(ReportErrorEventKey, value);		}
			remove	{	Events.RemoveHandler(ReportErrorEventKey, value);	}
		}

		public event MessageEventHandler WarningReported
		{
			add		{	Events.AddHandler(ReportWarningEventKey, value);	}
			remove	{	Events.RemoveHandler(ReportWarningEventKey, value);	}
		}

		public event MatchEventHandler MatchedToken
		{
			add		{	Events.AddHandler(MatchEventKey, value);	}
			remove	{	Events.RemoveHandler(MatchEventKey, value);	}
		}

		public event MatchEventHandler MatchedNotToken
		{
			add		{	Events.AddHandler(MatchNotEventKey, value);		}
			remove	{	Events.RemoveHandler(MatchNotEventKey, value);	}
		}

		public event MatchEventHandler MisMatchedToken
		{
			add		{	Events.AddHandler(MisMatchEventKey, value);		}
			remove	{	Events.RemoveHandler(MisMatchEventKey, value);	}
		}

		public event MatchEventHandler MisMatchedNotToken
		{
			add		{	Events.AddHandler(MisMatchNotEventKey, value);		}
			remove	{	Events.RemoveHandler(MisMatchNotEventKey, value);	}
		}

		public event TokenEventHandler ConsumedToken
		{
			add		{	Events.AddHandler(ConsumeEventKey, value);		}
			remove	{	Events.RemoveHandler(ConsumeEventKey, value);	}
		}

		public event TokenEventHandler TokenLA
		{
			add		{	Events.AddHandler(LAEventKey, value);		}
			remove	{	Events.RemoveHandler(LAEventKey, value);	}
		}

		public event SemanticPredicateEventHandler SemPredEvaluated
		{
			add		{	Events.AddHandler(SemPredEvaluatedEventKey, value);		}
			remove	{	Events.RemoveHandler(SemPredEvaluatedEventKey, value);	}
		}

		public event SyntacticPredicateEventHandler SynPredStarted
		{
			add		{	Events.AddHandler(SynPredStartedEventKey, value);		}
			remove	{	Events.RemoveHandler(SynPredStartedEventKey, value);	}
		}

		public event SyntacticPredicateEventHandler SynPredFailed
		{
			add		{	Events.AddHandler(SynPredFailedEventKey, value);	}
			remove	{	Events.RemoveHandler(SynPredFailedEventKey, value);	}
		}

		public event SyntacticPredicateEventHandler SynPredSucceeded
		{
			add		{	Events.AddHandler(SynPredSucceededEventKey, value);		}
			remove	{	Events.RemoveHandler(SynPredSucceededEventKey, value);	}
		}

		
		public virtual void  addMessageListener(MessageListener l)
		{
			if (!ignoreInvalidDebugCalls)
				throw new System.ArgumentException("addMessageListener() is only valid if parser built for debugging");
		}
		
		public virtual void  addParserListener(ParserListener l)
		{
			if (!ignoreInvalidDebugCalls)
				throw new System.ArgumentException("addParserListener() is only valid if parser built for debugging");
		}
		
		public virtual void  addParserMatchListener(ParserMatchListener l)
		{
			if (!ignoreInvalidDebugCalls)
				throw new System.ArgumentException("addParserMatchListener() is only valid if parser built for debugging");
		}
		
		public virtual void  addParserTokenListener(ParserTokenListener l)
		{
			if (!ignoreInvalidDebugCalls)
				throw new System.ArgumentException("addParserTokenListener() is only valid if parser built for debugging");
		}
		
		public virtual void  addSemanticPredicateListener(SemanticPredicateListener l)
		{
			if (!ignoreInvalidDebugCalls)
				throw new System.ArgumentException("addSemanticPredicateListener() is only valid if parser built for debugging");
		}
		
		public virtual void  addSyntacticPredicateListener(SyntacticPredicateListener l)
		{
			if (!ignoreInvalidDebugCalls)
				throw new System.ArgumentException("addSyntacticPredicateListener() is only valid if parser built for debugging");
		}
		
		public virtual void  addTraceListener(TraceListener l)
		{
			if (!ignoreInvalidDebugCalls)
				throw new System.ArgumentException("addTraceListener() is only valid if parser built for debugging");
		}
		
		/*Get another token object from the token stream */
		public abstract void  consume();
		/*Consume tokens until one matches the given token */
		public virtual void  consumeUntil(int tokenType)
		{
			while (LA(1) != Token.EOF_TYPE && LA(1) != tokenType)
			{
				consume();
			}
		}
		/*Consume tokens until one matches the given token set */
		public virtual void  consumeUntil(BitSet bset)
		{
			while (LA(1) != Token.EOF_TYPE && !bset.member(LA(1)))
			{
				consume();
			}
		}
		protected internal virtual void  defaultDebuggingSetup(TokenStream lexer, TokenBuffer tokBuf)
		{
			// by default, do nothing -- we're not debugging
		}
		/*Get the AST return value squirreled away in the parser */
		public virtual AST getAST()
		{
			return returnAST;
		}
		public virtual ASTFactory getASTFactory()
		{
			return astFactory;
		}
		public virtual string getFilename()
		{
			return inputState.filename;
		}
		
		public virtual ParserSharedInputState getInputState()
		{
			return inputState;
		}
		
		public virtual void  setInputState(ParserSharedInputState state)
		{
			inputState = state;
		}
		
		public virtual string getTokenName(int num)
		{
			return tokenNames[num];
		}
		public virtual string[] getTokenNames()
		{
			return tokenNames;
		}
		public virtual bool isDebugMode()
		{
			return false;
		}
		/*Return the token type of the ith token of lookahead where i=1
		* is the current token being examined by the parser (i.e., it
		* has not been matched yet).
		*/
		public abstract int LA(int i);
		/*Return the ith token of lookahead */
		public abstract Token LT(int i);
		// Forwarded to TokenBuffer
		public virtual int mark()
		{
			return inputState.input.mark();
		}
		/*Make sure current lookahead symbol matches token type <tt>t</tt>.
		* Throw an exception upon mismatch, which is catch by either the
		* error handler or by the syntactic predicate.
		*/
		public virtual void  match(int t)
		{
			if (LA(1) != t)
				throw new MismatchedTokenException(tokenNames, LT(1), t, false, getFilename());
			else
				consume();
		}
		/*Make sure current lookahead symbol matches the given set
		* Throw an exception upon mismatch, which is catch by either the
		* error handler or by the syntactic predicate.
		*/
		public virtual void  match(BitSet b)
		{
			if (!b.member(LA(1)))
				throw new MismatchedTokenException(tokenNames, LT(1), b, false, getFilename());
			else
				consume();
		}
		public virtual void  matchNot(int t)
		{
			if (LA(1) == t)
				throw new MismatchedTokenException(tokenNames, LT(1), t, true, getFilename());
			else
				consume();
		}

		/// <summary>
		/// @deprecated as of 2.7.2. This method calls System.exit() and writes
		/// directly to stderr, which is usually not appropriate when
		/// a parser is embedded into a larger application. Since the method is
		/// <code>static</code>, it cannot be overridden to avoid these problems.
		/// ANTLR no longer uses this method internally or in generated code.
		/// </summary>
		/// 
		[Obsolete("De-activated since version 2.7.2.6 as it cannot be overidden.", true)]
		public static void  panic()
		{
			System.Console.Error.WriteLine("Parser: panic");
			System.Environment.Exit(1);
		}

		public virtual void  removeMessageListener(MessageListener l)
		{
			if (!ignoreInvalidDebugCalls)
				throw new System.SystemException("removeMessageListener() is only valid if parser built for debugging");
		}
		public virtual void  removeParserListener(ParserListener l)
		{
			if (!ignoreInvalidDebugCalls)
				throw new System.SystemException("removeParserListener() is only valid if parser built for debugging");
		}
		public virtual void  removeParserMatchListener(ParserMatchListener l)
		{
			if (!ignoreInvalidDebugCalls)
				throw new System.SystemException("removeParserMatchListener() is only valid if parser built for debugging");
		}
		public virtual void  removeParserTokenListener(ParserTokenListener l)
		{
			if (!ignoreInvalidDebugCalls)
				throw new System.SystemException("removeParserTokenListener() is only valid if parser built for debugging");
		}
		public virtual void  removeSemanticPredicateListener(SemanticPredicateListener l)
		{
			if (!ignoreInvalidDebugCalls)
				throw new System.ArgumentException("removeSemanticPredicateListener() is only valid if parser built for debugging");
		}
		public virtual void  removeSyntacticPredicateListener(SyntacticPredicateListener l)
		{
			if (!ignoreInvalidDebugCalls)
				throw new System.ArgumentException("removeSyntacticPredicateListener() is only valid if parser built for debugging");
		}
		public virtual void  removeTraceListener(TraceListener l)
		{
			if (!ignoreInvalidDebugCalls)
				throw new System.SystemException("removeTraceListener() is only valid if parser built for debugging");
		}
		
		/*Parser error-reporting function can be overridden in subclass */
		public virtual void reportError(RecognitionException ex)
		{
			Console.Error.WriteLine(ex);
		}
		
		/*Parser error-reporting function can be overridden in subclass */
		public virtual void reportError(string s)
		{
			if (getFilename() == null)
			{
				Console.Error.WriteLine("error: " + s);
			}
			else
			{
				Console.Error.WriteLine(getFilename() + ": error: " + s);
			}
		}
		
		/*Parser warning-reporting function can be overridden in subclass */
		public virtual void  reportWarning(string s)
		{
			if (getFilename() == null)
			{
				Console.Error.WriteLine("warning: " + s);
			}
			else
			{
				Console.Error.WriteLine(getFilename() + ": warning: " + s);
			}
		}
		
		public virtual void  rewind(int pos)
		{
			inputState.input.rewind(pos);
		}

		/// <summary>
		/// Specify an object with support code (shared by Parser and TreeParser.
		/// Normally, the programmer does not play with this, using 
		/// <see cref="setASTNodeClass"/> instead.
		/// </summary>
		/// <param name="f"></param>
		public virtual void  setASTFactory(ASTFactory f)
		{
			astFactory = f;
		}

		/// <summary>
		/// Specify the type of node to create during tree building. 
		/// </summary>
		/// <param name="cl">Fully qualified AST Node type name.</param>
		public virtual void  setASTNodeClass(string cl)
		{
			astFactory.setASTNodeType(cl);
		}

		/// <summary>
		/// Specify the type of node to create during tree building. 
		/// use <see cref="setASTNodeClass"/> now to be consistent with 
		/// Token Object Type accessor.
		/// </summary>
		/// <param name="nodeType">Fully qualified AST Node type name.</param>
		[Obsolete("Replaced by setASTNodeClass(string) since version 2.7.1", true)]
		public virtual void  setASTNodeType(string nodeType)
		{
			setASTNodeClass(nodeType);
		}

		public virtual void  setDebugMode(bool debugMode)
		{
			if (!ignoreInvalidDebugCalls)
				throw new System.SystemException("setDebugMode() only valid if parser built for debugging");
		}
		public virtual void  setFilename(string f)
		{
			inputState.filename = f;
		}
		public virtual void  setIgnoreInvalidDebugCalls(bool Value)
		{
			ignoreInvalidDebugCalls = Value;
		}
		/*Set or change the input token buffer */
		public virtual void  setTokenBuffer(TokenBuffer t)
		{
			inputState.input = t;
		}
		
		public virtual void  traceIndent()
		{
			 for (int i = 0; i < traceDepth; i++)
				Console.Out.Write(" ");
		}
		public virtual void  traceIn(string rname)
		{
			traceDepth += 1;
			traceIndent();
			Console.Out.WriteLine("> " + rname + "; LA(1)==" + LT(1).getText() + ((inputState.guessing > 0)?" [guessing]":""));
		}
		public virtual void  traceOut(string rname)
		{
			traceIndent();
			Console.Out.WriteLine("< " + rname + "; LA(1)==" + LT(1).getText() + ((inputState.guessing > 0)?" [guessing]":""));
			traceDepth -= 1;
		}
	}
}