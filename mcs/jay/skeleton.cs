#	jay skeleton

#	character in column 1 determines outcome...
#		# is a comment
#		. is copied
#		t is copied as //t if -t is set
#	other lines are interpreted to call jay procedures

.// created by jay 0.7 (c) 1998 Axel.Schreiner@informatik.uni-osnabrueck.de
.
 prolog		## %{ ... %} prior to the first %%

.
.  /** error output stream.
.      It should be changeable.
.    */
.  public System.IO.TextWriter ErrorOutput = System.Console.Out;
.
.  /** simplified error message.
.      @see <a href="#yyerror(java.lang.String, java.lang.String[])">yyerror</a>
.    */
.  public void yyerror (string message) {
.    yyerror(message, null);
.  }
.
.  /** (syntax) error message.
.      Can be overwritten to control message format.
.      @param message text to be displayed.
.      @param expected vector of acceptable tokens, if available.
.    */
.  public void yyerror (string message, string[] expected) {
.    if ((expected != null) && (expected.Length  > 0)) {
.      ErrorOutput.Write (message+", expecting");
.      for (int n = 0; n < expected.Length; ++ n)
.        ErrorOutput.Write (" "+expected[n]);
.        ErrorOutput.WriteLine ();
.    } else
.      ErrorOutput.WriteLine (message);
.  }
.
.  /** debugging support, requires the package jay.yydebug.
.      Set to null to suppress debugging messages.
.    */
t  protected yydebug.yyDebug debug;
.
 debug			## tables for debugging support
.
.  /** index-checked interface to yyNames[].
.      @param token single character or %token value.
.      @return token name or [illegal] or [unknown].
.    */
t  public static string yyname (int token) {
t    if ((token < 0) || (token > yyNames.Length)) return "[illegal]";
t    string name;
t    if ((name = yyNames[token]) != null) return name;
t    return "[unknown]";
t  }
.
.  /** computes list of expected tokens on error by tracing the tables.
.      @param state for which to compute the list.
.      @return list of token names.
.    */
.  protected string[] yyExpecting (int state) {
.    int token, n, len = 0;
.    bool[] ok = new bool[yyNames.Length];
.
.    if ((n = yySindex[state]) != 0)
.      for (token = n < 0 ? -n : 0;
.           (token < yyNames.Length) && (n+token < yyTable.Length); ++ token)
.        if (yyCheck[n+token] == token && !ok[token] && yyNames[token] != null) {
.          ++ len;
.          ok[token] = true;
.        }
.    if ((n = yyRindex[state]) != 0)
.      for (token = n < 0 ? -n : 0;
.           (token < yyNames.Length) && (n+token < yyTable.Length); ++ token)
.        if (yyCheck[n+token] == token && !ok[token] && yyNames[token] != null) {
.          ++ len;
.          ok[token] = true;
.        }
.
.    string [] result = new string[len];
.    for (n = token = 0; n < len;  ++ token)
.      if (ok[token]) result[n++] = yyNames[token];
.    return result;
.  }
.
.  /** the generated parser, with debugging messages.
.      Maintains a state and a value stack, currently with fixed maximum size.
.      @param yyLex scanner.
.      @param yydebug debug message writer implementing yyDebug, or null.
.      @return result of the last reduction, if any.
.      @throws yyException on irrecoverable parse error.
.    */
.  public Object yyparse (yyParser.yyInput yyLex, Object yyd)
.				 {
t    this.debug = (yydebug.yyDebug)yyd;
.    return yyparse(yyLex);
.  }
.
.  /** initial size and increment of the state/value stack [default 256].
.      This is not final so that it can be overwritten outside of invocations
.      of yyparse().
.    */
.  protected int yyMax;
.
.  /** executed at the beginning of a reduce action.
.      Used as $$ = yyDefault($1), prior to the user-specified action, if any.
.      Can be overwritten to provide deep copy, etc.
.      @param first value for $1, or null.
.      @return first.
.    */
.  protected Object yyDefault (Object first) {
.    return first;
.  }
.
.  /** the generated parser.
.      Maintains a state and a value stack, currently with fixed maximum size.
.      @param yyLex scanner.
.      @return result of the last reduction, if any.
.      @throws yyException on irrecoverable parse error.
.    */
.  public Object yyparse (yyParser.yyInput yyLex)
.				{
.    if (yyMax <= 0) yyMax = 256;			// initial size
.    int yyState = 0;                                   // state stack ptr
.    int [] yyStates = new int[yyMax];	                // state stack 
.    Object yyVal = null;                               // value stack ptr
.    Object [] yyVals = new Object[yyMax];	        // value stack
.    int yyToken = -1;					// current input
.    int yyErrorFlag = 0;				// #tks to shift
.
 local		## %{ ... %} after the first %%

.    int yyTop = 0;
.    goto skip;
.    yyLoop:
.    yyTop++;
.    skip:
.    for (;; ++ yyTop) {
.      if (yyTop >= yyStates.Length) {			// dynamically increase
.        int[] i = new int[yyStates.Length+yyMax];
.        yyStates.CopyTo (i, 0);
.        yyStates = i;
.        Object[] o = new Object[yyVals.Length+yyMax];
.        yyVals.CopyTo (o, 0);
.        yyVals = o;
.      }
.      yyStates[yyTop] = yyState;
.      yyVals[yyTop] = yyVal;
t      if (debug != null) debug.push(yyState, yyVal);
.
.      yyDiscarded: for (;;) {	// discarding a token does not change stack
.        int yyN;
.        if ((yyN = yyDefRed[yyState]) == 0) {	// else [default] reduce (yyN)
.          if (yyToken < 0) {
.            yyToken = yyLex.advance() ? yyLex.token() : 0;

t            if (debug != null)
t              debug.lex(yyState, yyToken, yyname(yyToken), yyLex.value());
.          }
.          if ((yyN = yySindex[yyState]) != 0 && ((yyN += yyToken) >= 0)
.              && (yyN < yyTable.Length) && (yyCheck[yyN] == yyToken)) {
t            if (debug != null)
t              debug.shift(yyState, yyTable[yyN], yyErrorFlag-1);
.            yyState = yyTable[yyN];		// shift to yyN
.            yyVal = yyLex.value();
.            yyToken = -1;
.            if (yyErrorFlag > 0) -- yyErrorFlag;
.            goto yyLoop;
.          }
.          if ((yyN = yyRindex[yyState]) != 0 && (yyN += yyToken) >= 0
.              && yyN < yyTable.Length && yyCheck[yyN] == yyToken)
.            yyN = yyTable[yyN];			// reduce (yyN)
.          else
.            switch (yyErrorFlag) {
.  
.            case 0:
.              yyerror(String.Format ("syntax error, got token `{0}'", yyname (yyToken)), yyExpecting(yyState));
t              if (debug != null) debug.error("syntax error");
.              goto case 1;
.            case 1: case 2:
.              yyErrorFlag = 3;
.              do {
.                if ((yyN = yySindex[yyStates[yyTop]]) != 0
.                    && (yyN += Token.yyErrorCode) >= 0 && yyN < yyTable.Length
.                    && yyCheck[yyN] == Token.yyErrorCode) {
t                  if (debug != null)
t                    debug.shift(yyStates[yyTop], yyTable[yyN], 3);
.                  yyState = yyTable[yyN];
.                  yyVal = yyLex.value();
.                  goto yyLoop;
.                }
t                if (debug != null) debug.pop(yyStates[yyTop]);
.              } while (-- yyTop >= 0);
t              if (debug != null) debug.reject();
.              throw new yyParser.yyException("irrecoverable syntax error");
.  
.            case 3:
.              if (yyToken == 0) {
t                if (debug != null) debug.reject();
.                throw new yyParser.yyException("irrecoverable syntax error at end-of-file");
.              }
t              if (debug != null)
t                debug.discard(yyState, yyToken, yyname(yyToken),
t  							yyLex.value());
.              yyToken = -1;
.              goto yyDiscarded;		// leave stack alone
.            }
.        }
.        int yyV = yyTop + 1-yyLen[yyN];
t        if (debug != null)
t          debug.reduce(yyState, yyStates[yyV-1], yyN, yyRule[yyN], yyLen[yyN]);
.        yyVal = yyDefault(yyV > yyTop ? null : yyVals[yyV]);
.        switch (yyN) {

 actions		## code from the actions within the grammar

.        }
.        yyTop -= yyLen[yyN];
.        yyState = yyStates[yyTop];
.        int yyM = yyLhs[yyN];
.        if (yyState == 0 && yyM == 0) {
t          if (debug != null) debug.shift(0, yyFinal);
.          yyState = yyFinal;
.          if (yyToken < 0) {
.            yyToken = yyLex.advance() ? yyLex.token() : 0;
		
t            if (debug != null)
t               debug.lex(yyState, yyToken,yyname(yyToken), yyLex.value());
.          }
.          if (yyToken == 0) {
t            if (debug != null) debug.accept(yyVal);
.            return yyVal;
.          }
.          goto yyLoop;
.        }
.        if (((yyN = yyGindex[yyM]) != 0) && ((yyN += yyState) >= 0)
.            && (yyN < yyTable.Length) && (yyCheck[yyN] == yyState))
.          yyState = yyTable[yyN];
.        else
.          yyState = yyDgoto[yyM];
t        if (debug != null) debug.shift(yyStates[yyTop], yyState);
.	 goto yyLoop;
.      }
.    }
.  }
.
 tables			## tables for rules, default reduction, and action calls
.
 epilog			## text following second %%
.namespace yydebug {
.        using System;
.	 public interface yyDebug {
.		 void push (int state, Object value);
.		 void lex (int state, int token, string name, Object value);
.		 void shift (int from, int to, int errorFlag);
.		 void pop (int state);
.		 void discard (int state, int token, string name, Object value);
.		 void reduce (int from, int to, int rule, string text, int len);
.		 void shift (int from, int to);
.		 void accept (Object value);
.		 void error (string message);
.		 void reject ();
.	 }
.	 
.	 class yyDebugSimple : yyDebug {
.		 void println (string s){
.			 Console.Error.WriteLine (s);
.		 }
.		 
.		 public void push (int state, Object value) {
.			 println ("push\tstate "+state+"\tvalue "+value);
.		 }
.		 
.		 public void lex (int state, int token, string name, Object value) {
.			 println("lex\tstate "+state+"\treading "+name+"\tvalue "+value);
.		 }
.		 
.		 public void shift (int from, int to, int errorFlag) {
.			 switch (errorFlag) {
.			 default:				// normally
.				 println("shift\tfrom state "+from+" to "+to);
.				 break;
.			 case 0: case 1: case 2:		// in error recovery
.				 println("shift\tfrom state "+from+" to "+to
.					     +"\t"+errorFlag+" left to recover");
.				 break;
.			 case 3:				// normally
.				 println("shift\tfrom state "+from+" to "+to+"\ton error");
.				 break;
.			 }
.		 }
.		 
.		 public void pop (int state) {
.			 println("pop\tstate "+state+"\ton error");
.		 }
.		 
.		 public void discard (int state, int token, string name, Object value) {
.			 println("discard\tstate "+state+"\ttoken "+name+"\tvalue "+value);
.		 }
.		 
.		 public void reduce (int from, int to, int rule, string text, int len) {
.			 println("reduce\tstate "+from+"\tuncover "+to
.				     +"\trule ("+rule+") "+text);
.		 }
.		 
.		 public void shift (int from, int to) {
.			 println("goto\tfrom state "+from+" to "+to);
.		 }
.		 
.		 public void accept (Object value) {
.			 println("accept\tvalue "+value);
.		 }
.		 
.		 public void error (string message) {
.			 println("error\t"+message);
.		 }
.		 
.		 public void reject () {
.			 println("reject");
.		 }
.		 
.	 }
.}
.// %token constants
. class Token {
 tokens public const int
. }
. namespace yyParser {
.  using System;
.  /** thrown for irrecoverable syntax errors and stack overflow.
.    */
.  public class yyException : System.Exception {
.    public yyException (string message) : base (message) {
.    }
.  }
.
.  /** must be implemented by a scanner object to supply input to the parser.
.    */
.  public interface yyInput {
.    /** move on to next token.
.        @return false if positioned beyond tokens.
.        @throws IOException on input error.
.      */
.    bool advance (); // throws java.io.IOException;
.    /** classifies current token.
.        Should not be called if advance() returned false.
.        @return current %token or single character.
.      */
.    int token ();
.    /** associated with current token.
.        Should not be called if advance() returned false.
.        @return value for token().
.      */
.    Object value ();
.  }
. }
.} // close outermost namespace, that MUST HAVE BEEN opened in the prolog
