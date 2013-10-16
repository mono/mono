// created by jay 0.7 (c) 1998 Axel.Schreiner@informatik.uni-osnabrueck.de

#line 2 "ExpressionParser.jay"

using System;
using System.Text;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Exceptions;
using Microsoft.Build.Framework;

/*

Pseudo formal syntax for .NET 4.0 expression:

Condition = Expression
Include = Expression*

 Expression
	BooleanLiteral
		TrueLiteral
		FalseLiteral
	BinaryExpression
		Expression "==" Expression
		Expression "!=" Expression
		Expression ">" Expression
		Expression ">=" Expression
		Expression "<" Expression
		Expression "<=" Expression
		Expression "And" Expression
		Expression "Or" Expression
	UnaryExpression
		"!" Expression
	PropertyExpression
		"$(" PropertyApplication ")"
	ItemExpression
		"@(" ItemApplication ")"
	MetadataBatchingExpression
		"%(" MetadataBatchingApplication ")"
  StringLiteralOrFunction
		StringLiteralOrFunctionName ( "(" FunctionArguments ")" )?

.NET error messages are so detailed which is something like "you forgot '(' after '$' ?" - so
it is likely that the MS tokenizer is hand-written.

*/

namespace Microsoft.Build.Internal
{
#if false
	class ExpressionParser
	{
		public static string EvaluateExpression (string source, Project project, ITaskItem [] inputs)
		{
			var head = new StringBuilder ();
			var tail = new StringBuilder ();
			int start = 0;
			int end = source.Length;
			while (start < end) {
				switch (source [start]) {
				case '$':
				case '@':
					int last = source.LastIndexOf (')', end);
					if (last < 0)
						throw new InvalidProjectFileException (string.Format ("expression did not have matching ')' since index {0} in \"{1}\"", start, source));
					if (start + 1 == end || source [start] != '(')
						throw new InvalidProjectFileException (string.Format ("missing '(' after '{0}' at {1} in \"{2}\"", source [start], start, source));
					tail.Insert (0, source.Substring (last + 1, end - last));
					start += 2;
					end = last - 1;
					if (source [start] == '$')
						head.Append (EvaluatePropertyExpression (source, project, inputs, start, end));
					else
						head.Append (EvaluateItemExpression (source, project, inputs, start, end));
					break;
				default:
					head.Append (source.Substring (start, end - start));
					start = end;
					break;
				}
			}
			return head.ToString () + tail.ToString ();
		}
		
		public static string EvaluatePropertyExpression (string source, Project project, ITaskItem [] inputs, int start, int end)
		{
			int idx = source.IndexOf ("::", start, StringComparison.Ordinal);
			if (idx >= 0) {
				string type = source.Substring (start, idx - start);
				if (type.Length < 2 || type [0] != '[' || type [type.Length - 1] != ']')
					throw new InvalidProjectFileException (string.Format ("Static function call misses appropriate type name surrounded by '[' and ']' at {0} in \"{1}\"", start, source));
				int start2 = idx + 2;
				int idx2 = source.IndexOf ('(', idx + 2, end - start2);
				if (idx2 < 0) {
					// access to static property
					string member = source.Substring (start2, end - start2);
				} else {
					// access to static method
					string member = source.Substring (start2, idx2 - start2);
				}
			} // the result could be context for further property access...
			
			idx = source.IndexOf ('.', start);
			if (idx > 0) {
				string name = source.Substring (start, idx - start);
				var prop = project.GetProperty (name);
				if (prop == null)
					throw new InvalidProjectFileException (string.Format ("Property \"{0}\" was not found", name));
			}
		}
		
		public static string EvaluateItemExpression (string source, Project project, ITaskItem [] inputs, int start, int end)
		{
			// using property as context and evaluate
			int idx = source.IndexOf ("->", start, StringComparison.Ordinal);
			if (idx > 0) {
				string name = source.Substring (start, idx - start);
			}
			
		}
	}
	
	class ExpressionNode
	{
	}
	
	enum ExpressionNodeType
	{
		Item,
		Property,
		Transform,
		Invocation
	}

#endif

	class ExpressionParser
	{
		int yacc_verbose_flag = 1;
#line default

  /** error output stream.
      It should be changeable.
    */
  public System.IO.TextWriter ErrorOutput = System.Console.Out;

  /** simplified error message.
      @see <a href="#yyerror(java.lang.String, java.lang.String[])">yyerror</a>
    */
  public void yyerror (string message) {
    yyerror(message, null);
  }
#pragma warning disable 649
  /* An EOF token */
  public int eof_token;
#pragma warning restore 649
  /** (syntax) error message.
      Can be overwritten to control message format.
      @param message text to be displayed.
      @param expected vector of acceptable tokens, if available.
    */
  public void yyerror (string message, string[] expected) {
    if ((yacc_verbose_flag > 0) && (expected != null) && (expected.Length  > 0)) {
      ErrorOutput.Write (message+", expecting");
      for (int n = 0; n < expected.Length; ++ n)
        ErrorOutput.Write (" "+expected[n]);
        ErrorOutput.WriteLine ();
    } else
      ErrorOutput.WriteLine (message);
  }

  /** debugging support, requires the package jay.yydebug.
      Set to null to suppress debugging messages.
    */
  internal yydebug.yyDebug debug;

  protected const int yyFinal = 9;
 // Put this array into a separate class so it is only initialized if debugging is actually used
 // Use MarshalByRefObject to disable inlining
 class YYRules : MarshalByRefObject {
  public static readonly string [] yyRule = {
    "$accept : Expression",
    "ExpressionList : Expression",
    "ExpressionList : ExpressionList Expression",
    "Expression : BooleanLiteral",
    "Expression : BinaryExpression",
    "Expression : UnaryExpression",
    "Expression : PropertyAccessExpression",
    "Expression : ItemAccessExpression",
    "Expression : MetadataAccessExpression",
    "Expression : StringLiteralOrFunction",
    "Expression : ParenthesizedExpression",
    "BooleanLiteral : TRUE_LITERAL",
    "BooleanLiteral : FALSE_LITERAL",
    "BinaryExpression : Expression EQ Expression",
    "BinaryExpression : Expression NE Expression",
    "BinaryExpression : Expression GT Expression",
    "BinaryExpression : Expression GE Expression",
    "BinaryExpression : Expression LT Expression",
    "BinaryExpression : Expression LE Expression",
    "BinaryExpression : Expression AND Expression",
    "BinaryExpression : Expression OR Expression",
    "UnaryExpression : NOT Expression",
    "PropertyAccessExpression : PROP_OPEN PropertyAccess PAREN_CLOSE",
    "PropertyAccess : NAME",
    "PropertyAccess : Expression DOT NAME",
    "ItemAccessExpression : ITEM_OPEN ItemApplication PAREN_CLOSE",
    "ItemApplication : NAME",
    "ItemApplication : NAME ARROW ExpressionList",
    "MetadataAccessExpression : METADATA_OPEN MetadataAccess PAREN_CLOSE",
    "MetadataAccess : NAME",
    "MetadataAccess : NAME DOT NAME",
    "StringLiteralOrFunction : NAME",
    "StringLiteralOrFunction : NAME PAREN_OPEN FunctionCallArguments PAREN_CLOSE",
    "FunctionCallArguments : Expression",
    "FunctionCallArguments : FunctionCallArguments COMMA Expression",
    "ParenthesizedExpression : PAREN_OPEN Expression PAREN_CLOSE",
  };
 public static string getRule (int index) {
    return yyRule [index];
 }
}
  protected static readonly string [] yyNames = {    
    "end-of-file",null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,"'!'",null,null,null,null,null,
    null,"'('","')'",null,null,"','",null,"'.'",null,null,null,null,null,
    null,null,null,null,null,null,null,null,"'<'",null,"'>'",null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,"TRUE_LITERAL",
    "FALSE_LITERAL","EQ","\"==\"","NE","\"!=\"","GT","GE","\">=\"","LT",
    "LE","\"<=\"","AND","OR","NOT","DOT","COMMA","PROP_OPEN","\"$(\"",
    "ITEM_OPEN","\"@(\"","METADATA_OPEN","\"%(\"","PAREN_OPEN",
    "PAREN_CLOSE","COLON2","\"::\"","ARROW","\"->\"","NAME","ERROR",
  };

  /** index-checked interface to yyNames[].
      @param token single character or %token value.
      @return token name or [illegal] or [unknown].
    */
  public static string yyname (int token) {
    if ((token < 0) || (token > yyNames.Length)) return "[illegal]";
    string name;
    if ((name = yyNames[token]) != null) return name;
    return "[unknown]";
  }

#pragma warning disable 414
  int yyExpectingState;
#pragma warning restore 414
  /** computes list of expected tokens on error by tracing the tables.
      @param state for which to compute the list.
      @return list of token names.
    */
  protected int [] yyExpectingTokens (int state){
    int token, n, len = 0;
    bool[] ok = new bool[yyNames.Length];
    if ((n = yySindex[state]) != 0)
      for (token = n < 0 ? -n : 0;
           (token < yyNames.Length) && (n+token < yyTable.Length); ++ token)
        if (yyCheck[n+token] == token && !ok[token] && yyNames[token] != null) {
          ++ len;
          ok[token] = true;
        }
    if ((n = yyRindex[state]) != 0)
      for (token = n < 0 ? -n : 0;
           (token < yyNames.Length) && (n+token < yyTable.Length); ++ token)
        if (yyCheck[n+token] == token && !ok[token] && yyNames[token] != null) {
          ++ len;
          ok[token] = true;
        }
    int [] result = new int [len];
    for (n = token = 0; n < len;  ++ token)
      if (ok[token]) result[n++] = token;
    return result;
  }
  protected string[] yyExpecting (int state) {
    int [] tokens = yyExpectingTokens (state);
    string [] result = new string[tokens.Length];
    for (int n = 0; n < tokens.Length;  n++)
      result[n++] = yyNames[tokens [n]];
    return result;
  }

  /** the generated parser, with debugging messages.
      Maintains a state and a value stack, currently with fixed maximum size.
      @param yyLex scanner.
      @param yydebug debug message writer implementing yyDebug, or null.
      @return result of the last reduction, if any.
      @throws yyException on irrecoverable parse error.
    */
  internal Object yyparse (yyParser.yyInput yyLex, Object yyd)
				 {
    this.debug = (yydebug.yyDebug)yyd;
    return yyparse(yyLex);
  }

  /** initial size and increment of the state/value stack [default 256].
      This is not final so that it can be overwritten outside of invocations
      of yyparse().
    */
  protected int yyMax;

  /** executed at the beginning of a reduce action.
      Used as $$ = yyDefault($1), prior to the user-specified action, if any.
      Can be overwritten to provide deep copy, etc.
      @param first value for $1, or null.
      @return first.
    */
  protected Object yyDefault (Object first) {
    return first;
  }

	static int[] global_yyStates;
	static object[] global_yyVals;
#pragma warning disable 649
	protected bool use_global_stacks;
#pragma warning restore 649
	object[] yyVals;					// value stack
	object yyVal;						// value stack ptr
	int yyToken;						// current input
	int yyTop;

  /** the generated parser.
      Maintains a state and a value stack, currently with fixed maximum size.
      @param yyLex scanner.
      @return result of the last reduction, if any.
      @throws yyException on irrecoverable parse error.
    */
  internal Object yyparse (yyParser.yyInput yyLex)
  {
    if (yyMax <= 0) yyMax = 256;		// initial size
    int yyState = 0;                   // state stack ptr
    int [] yyStates;               	// state stack 
    yyVal = null;
    yyToken = -1;
    int yyErrorFlag = 0;				// #tks to shift
	if (use_global_stacks && global_yyStates != null) {
		yyVals = global_yyVals;
		yyStates = global_yyStates;
   } else {
		yyVals = new object [yyMax];
		yyStates = new int [yyMax];
		if (use_global_stacks) {
			global_yyVals = yyVals;
			global_yyStates = yyStates;
		}
	}

    /*yyLoop:*/ for (yyTop = 0;; ++ yyTop) {
      if (yyTop >= yyStates.Length) {			// dynamically increase
        global::System.Array.Resize (ref yyStates, yyStates.Length+yyMax);
        global::System.Array.Resize (ref yyVals, yyVals.Length+yyMax);
      }
      yyStates[yyTop] = yyState;
      yyVals[yyTop] = yyVal;
      if (debug != null) debug.push(yyState, yyVal);

      /*yyDiscarded:*/ while (true) {	// discarding a token does not change stack
        int yyN;
        if ((yyN = yyDefRed[yyState]) == 0) {	// else [default] reduce (yyN)
          if (yyToken < 0) {
            yyToken = yyLex.advance() ? yyLex.token() : 0;
            if (debug != null)
              debug.lex(yyState, yyToken, yyname(yyToken), yyLex.value());
          }
          if ((yyN = yySindex[yyState]) != 0 && ((yyN += yyToken) >= 0)
              && (yyN < yyTable.Length) && (yyCheck[yyN] == yyToken)) {
            if (debug != null)
              debug.shift(yyState, yyTable[yyN], yyErrorFlag-1);
            yyState = yyTable[yyN];		// shift to yyN
            yyVal = yyLex.value();
            yyToken = -1;
            if (yyErrorFlag > 0) -- yyErrorFlag;
            goto continue_yyLoop;
          }
          if ((yyN = yyRindex[yyState]) != 0 && (yyN += yyToken) >= 0
              && yyN < yyTable.Length && yyCheck[yyN] == yyToken)
            yyN = yyTable[yyN];			// reduce (yyN)
          else
            switch (yyErrorFlag) {
  
            case 0:
              yyExpectingState = yyState;
              // yyerror(String.Format ("syntax error, got token `{0}'", yyname (yyToken)), yyExpecting(yyState));
              if (debug != null) debug.error("syntax error");
              if (yyToken == 0 /*eof*/ || yyToken == eof_token) throw new yyParser.yyUnexpectedEof ();
              goto case 1;
            case 1: case 2:
              yyErrorFlag = 3;
              do {
                if ((yyN = yySindex[yyStates[yyTop]]) != 0
                    && (yyN += Token.yyErrorCode) >= 0 && yyN < yyTable.Length
                    && yyCheck[yyN] == Token.yyErrorCode) {
                  if (debug != null)
                    debug.shift(yyStates[yyTop], yyTable[yyN], 3);
                  yyState = yyTable[yyN];
                  yyVal = yyLex.value();
                  goto continue_yyLoop;
                }
                if (debug != null) debug.pop(yyStates[yyTop]);
              } while (-- yyTop >= 0);
              if (debug != null) debug.reject();
              throw new yyParser.yyException("irrecoverable syntax error");
  
            case 3:
              if (yyToken == 0) {
                if (debug != null) debug.reject();
                throw new yyParser.yyException("irrecoverable syntax error at end-of-file");
              }
              if (debug != null)
                debug.discard(yyState, yyToken, yyname(yyToken),
  							yyLex.value());
              yyToken = -1;
              goto continue_yyDiscarded;		// leave stack alone
            }
        }
        int yyV = yyTop + 1-yyLen[yyN];
        if (debug != null)
          debug.reduce(yyState, yyStates[yyV-1], yyN, YYRules.getRule (yyN), yyLen[yyN]);
        yyVal = yyV > yyTop ? null : yyVals[yyV]; // yyVal = yyDefault(yyV > yyTop ? null : yyVals[yyV]);
        switch (yyN) {
case 1:
#line 168 "ExpressionParser.jay"
  { yyVal = new ExpressionList ((Expression) yyVals[0+yyTop]); }
  break;
case 2:
#line 170 "ExpressionParser.jay"
  { yyVal = ((ExpressionList) yyVals[-1+yyTop]).Append ((Expression) yyVals[0+yyTop]); }
  break;
case 11:
#line 185 "ExpressionParser.jay"
  { yyVal = new BooleanLiteral () { Value = true, Location = (ILocation) yyVals[0+yyTop] }; }
  break;
case 12:
#line 187 "ExpressionParser.jay"
  { yyVal = new BooleanLiteral () { Value = false, Location = (ILocation) yyVals[0+yyTop] }; }
  break;
case 21:
#line 203 "ExpressionParser.jay"
  { yyVal = new NotExpression () { Negated = (Expression) yyVals[0+yyTop], Location = (ILocation) yyVals[-1+yyTop] }; }
  break;
case 22:
#line 208 "ExpressionParser.jay"
  { yyVal = new PropertyAccessExpression () { Access = (PropertyAccess) yyVals[-1+yyTop], Location = (ILocation) yyVals[-2+yyTop] }; }
  break;
case 23:
#line 213 "ExpressionParser.jay"
  { yyVal = new PropertyAccess () { Name = (NameToken) yyVals[0+yyTop], Location = (NameToken) yyVals[0+yyTop] }; }
  break;
case 24:
#line 215 "ExpressionParser.jay"
  { yyVal = new PropertyAccess () { Name = (NameToken) yyVals[0+yyTop], Target = (Expression) yyVals[-2+yyTop], Location = (ILocation) yyVals[-2+yyTop] }; }
  break;
case 25:
#line 220 "ExpressionParser.jay"
  { yyVal = new ItemAccessExpression () { Application = (ItemApplication) yyVals[-1+yyTop], Location = (ILocation) yyVals[-2+yyTop] }; }
  break;
case 27:
#line 227 "ExpressionParser.jay"
  { yyVal = new ItemApplication () { Name = (NameToken) yyVals[-2+yyTop], Expressions = (ExpressionList) yyVals[0+yyTop], Location = (ILocation) yyVals[-2+yyTop] }; }
  break;
case 28:
#line 232 "ExpressionParser.jay"
  { yyVal = new MetadataAccessExpression () { Access = (MetadataAccess) yyVals[-1+yyTop], Location = (ILocation) yyVals[-2+yyTop] }; }
  break;
case 29:
#line 238 "ExpressionParser.jay"
  { yyVal = new MetadataAccess () { Metadata = (NameToken) yyVals[0+yyTop], Location = (ILocation) yyVals[0+yyTop] }; }
  break;
case 30:
#line 240 "ExpressionParser.jay"
  { yyVal = new MetadataAccess () { Item = (NameToken) yyVals[-2+yyTop], Metadata = (NameToken) yyVals[0+yyTop], Location = (ILocation) yyVals[-2+yyTop] }; }
  break;
case 31:
#line 245 "ExpressionParser.jay"
  { yyVal = new StringLiteral () { Value = (NameToken) yyVals[0+yyTop], Location = (ILocation) yyVals[0+yyTop] }; }
  break;
case 32:
#line 247 "ExpressionParser.jay"
  { yyVal = new FunctionCallExpression () { Name = (NameToken) yyVals[-3+yyTop], Arguments = (ExpressionList) yyVals[-1+yyTop], Location = (ILocation) yyVals[-3+yyTop] }; }
  break;
case 33:
#line 252 "ExpressionParser.jay"
  { yyVal = new ExpressionList ((Expression) yyVals[0+yyTop]); }
  break;
case 34:
#line 254 "ExpressionParser.jay"
  { yyVal = ((ExpressionList) yyVals[-2+yyTop]).Append ((Expression) yyVals[0+yyTop]); }
  break;
case 35:
#line 259 "ExpressionParser.jay"
  { yyVal = (Expression) yyVals[-1+yyTop]; }
  break;
#line default
        }
        yyTop -= yyLen[yyN];
        yyState = yyStates[yyTop];
        int yyM = yyLhs[yyN];
        if (yyState == 0 && yyM == 0) {
          if (debug != null) debug.shift(0, yyFinal);
          yyState = yyFinal;
          if (yyToken < 0) {
            yyToken = yyLex.advance() ? yyLex.token() : 0;
            if (debug != null)
               debug.lex(yyState, yyToken,yyname(yyToken), yyLex.value());
          }
          if (yyToken == 0) {
            if (debug != null) debug.accept(yyVal);
            return yyVal;
          }
          goto continue_yyLoop;
        }
        if (((yyN = yyGindex[yyM]) != 0) && ((yyN += yyState) >= 0)
            && (yyN < yyTable.Length) && (yyCheck[yyN] == yyState))
          yyState = yyTable[yyN];
        else
          yyState = yyDgoto[yyM];
        if (debug != null) debug.shift(yyStates[yyTop], yyState);
	 goto continue_yyLoop;
      continue_yyDiscarded: ;	// implements the named-loop continue: 'continue yyDiscarded'
      }
    continue_yyLoop: ;		// implements the named-loop continue: 'continue yyLoop'
    }
  }

/*
 All more than 3 lines long rules are wrapped into a method
*/
#line default
   static readonly short [] yyLhs  = {              -1,
    1,    1,    0,    0,    0,    0,    0,    0,    0,    0,
    2,    2,    3,    3,    3,    3,    3,    3,    3,    3,
    4,    5,   10,   10,    6,   11,   11,    7,   12,   12,
    8,    8,   13,   13,    9,
  };
   static readonly short [] yyLen = {           2,
    1,    2,    1,    1,    1,    1,    1,    1,    1,    1,
    1,    1,    3,    3,    3,    3,    3,    3,    3,    3,
    2,    3,    1,    3,    3,    1,    3,    3,    1,    3,
    1,    4,    1,    3,    3,
  };
   static readonly short [] yyDefRed = {            0,
   11,   12,    0,    0,    0,    0,    0,   31,    0,    3,
    4,    5,    6,    7,    8,    9,   10,   21,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,   22,    0,   25,    0,
   28,   35,    0,    0,    0,    0,    0,    0,    0,    0,
    0,   20,   24,    0,    0,   30,    0,   32,    0,    0,
  };
  protected static readonly short [] yyDgoto  = {             9,
   55,   10,   11,   12,   13,   14,   15,   16,   17,   21,
   23,   25,   44,
  };
  protected static readonly short [] yySindex = {         -180,
    0,    0, -180, -156, -286, -284, -180,    0, -203,    0,
    0,    0,    0,    0,    0,    0,    0,    0, -276, -251,
 -274, -267, -243, -231, -238, -126, -180, -180, -180, -180,
 -180, -180, -180, -180, -180, -241,    0, -180,    0, -239,
    0,    0, -203, -270, -153, -118, -141, -113, -109, -246,
 -220,    0,    0, -203, -180,    0, -180,    0, -203, -203,
  };
  protected static readonly short [] yyRindex = {            0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0, -177,    0,
    0, -226,    0, -219,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0, -259,    0,  142,  131,  105,   79,   53,   27,
    1,    0,    0, -232, -212,    0,    0,    0, -206, -253,
  };
  protected static readonly short [] yyGindex = {            2,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,
  };
  protected static readonly short [] yyTable = {            22,
   19,   24,   57,   27,   18,   20,   37,   28,   26,   29,
   58,   30,   31,   33,   32,   33,   38,   34,   35,   34,
   36,   33,   34,   35,    1,    1,   18,   34,   43,   45,
   46,   47,   48,   49,   50,   51,   52,   39,    1,   54,
   40,    1,   41,    1,   53,    1,   56,    1,    1,   35,
    2,    2,   17,    1,   26,   28,   59,   29,   60,   30,
   31,   29,   32,   33,    2,   34,   35,    2,   27,    2,
    0,    2,    0,    2,    2,    0,    1,    2,   16,    2,
    0,   31,    0,   31,    0,   31,   31,    0,   31,   31,
    3,   31,   31,    4,   31,    5,    0,    6,    0,    7,
    1,    2,    0,   23,   15,    8,    0,   29,    0,   30,
   31,    0,   32,   33,    3,   34,   35,    4,    0,    5,
    0,    6,   31,    7,   32,   33,    0,   34,   35,   19,
   14,    0,   28,    0,   29,    0,   30,   31,    0,   32,
   33,   13,   34,   35,   30,   31,    0,   32,   33,    0,
   34,   35,   32,   33,   42,   34,   35,   33,    0,   34,
   35,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,   19,   19,   19,
    0,   19,    0,   19,   19,    0,   19,   19,    0,   19,
    0,   19,   19,   19,   19,    0,   19,    0,   19,    0,
   19,   19,    0,   18,   18,   18,   19,   18,    0,   18,
   18,    0,   18,   18,    0,    0,    0,   18,   18,   18,
   18,    0,   18,    0,   18,    0,   18,   18,    0,   17,
   17,   17,   18,   17,    0,   17,   17,    0,   17,    0,
    0,    0,    0,   17,   17,   17,   17,    0,   17,    0,
   17,    0,   17,   17,    0,   16,   16,   16,   17,   16,
    0,   16,   16,    0,    0,    0,    0,    0,    0,   16,
   16,   16,   16,    0,   16,    0,   16,    0,   16,   16,
    0,   15,   15,   15,   16,   15,    0,   15,    0,    0,
    0,    0,    0,    0,    0,   15,   15,   15,   15,    0,
   15,    0,   15,    0,   15,   15,    0,   14,   14,   14,
   15,   14,    0,    0,    0,    0,    0,    0,   13,   13,
   13,   14,   14,   14,   14,    0,   14,    0,   14,    0,
   14,   14,   13,   13,   13,   13,   14,   13,    0,   13,
    0,   13,   13,    0,    0,    0,    0,   13,
  };
  protected static readonly short [] yyCheck = {           286,
    0,  286,  273,  280,    3,    4,  281,  259,    7,  261,
  281,  263,  264,  273,  266,  267,  284,  269,  270,  273,
  272,  281,  269,  270,  257,  258,    0,  281,   27,   28,
   29,   30,   31,   32,   33,   34,   35,  281,  271,   38,
  272,  274,  281,  276,  286,  278,  286,  280,  281,  270,
  257,  258,    0,  286,  281,  259,   55,  261,   57,  263,
  264,  281,  266,  267,  271,  269,  270,  274,  281,  276,
   -1,  278,   -1,  280,  281,   -1,  257,  258,    0,  286,
   -1,  259,   -1,  261,   -1,  263,  264,   -1,  266,  267,
  271,  269,  270,  274,  272,  276,   -1,  278,   -1,  280,
  257,  258,   -1,  281,    0,  286,   -1,  261,   -1,  263,
  264,   -1,  266,  267,  271,  269,  270,  274,   -1,  276,
   -1,  278,  264,  280,  266,  267,   -1,  269,  270,  286,
    0,   -1,  259,   -1,  261,   -1,  263,  264,   -1,  266,
  267,    0,  269,  270,  263,  264,   -1,  266,  267,   -1,
  269,  270,  266,  267,  281,  269,  270,  267,   -1,  269,
  270,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  257,  258,  259,
   -1,  261,   -1,  263,  264,   -1,  266,  267,   -1,  269,
   -1,  271,  272,  273,  274,   -1,  276,   -1,  278,   -1,
  280,  281,   -1,  257,  258,  259,  286,  261,   -1,  263,
  264,   -1,  266,  267,   -1,   -1,   -1,  271,  272,  273,
  274,   -1,  276,   -1,  278,   -1,  280,  281,   -1,  257,
  258,  259,  286,  261,   -1,  263,  264,   -1,  266,   -1,
   -1,   -1,   -1,  271,  272,  273,  274,   -1,  276,   -1,
  278,   -1,  280,  281,   -1,  257,  258,  259,  286,  261,
   -1,  263,  264,   -1,   -1,   -1,   -1,   -1,   -1,  271,
  272,  273,  274,   -1,  276,   -1,  278,   -1,  280,  281,
   -1,  257,  258,  259,  286,  261,   -1,  263,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  271,  272,  273,  274,   -1,
  276,   -1,  278,   -1,  280,  281,   -1,  257,  258,  259,
  286,  261,   -1,   -1,   -1,   -1,   -1,   -1,  257,  258,
  259,  271,  272,  273,  274,   -1,  276,   -1,  278,   -1,
  280,  281,  271,  272,  273,  274,  286,  276,   -1,  278,
   -1,  280,  281,   -1,   -1,   -1,   -1,  286,
  };

#line 263 "ExpressionParser.jay"

	}
#line default
namespace yydebug {
        using System;
	 internal interface yyDebug {
		 void push (int state, Object value);
		 void lex (int state, int token, string name, Object value);
		 void shift (int from, int to, int errorFlag);
		 void pop (int state);
		 void discard (int state, int token, string name, Object value);
		 void reduce (int from, int to, int rule, string text, int len);
		 void shift (int from, int to);
		 void accept (Object value);
		 void error (string message);
		 void reject ();
	 }
	 
	 class yyDebugSimple : yyDebug {
		 void println (string s){
			 Console.Error.WriteLine (s);
		 }
		 
		 public void push (int state, Object value) {
			 println ("push\tstate "+state+"\tvalue "+value);
		 }
		 
		 public void lex (int state, int token, string name, Object value) {
			 println("lex\tstate "+state+"\treading "+name+"\tvalue "+value);
		 }
		 
		 public void shift (int from, int to, int errorFlag) {
			 switch (errorFlag) {
			 default:				// normally
				 println("shift\tfrom state "+from+" to "+to);
				 break;
			 case 0: case 1: case 2:		// in error recovery
				 println("shift\tfrom state "+from+" to "+to
					     +"\t"+errorFlag+" left to recover");
				 break;
			 case 3:				// normally
				 println("shift\tfrom state "+from+" to "+to+"\ton error");
				 break;
			 }
		 }
		 
		 public void pop (int state) {
			 println("pop\tstate "+state+"\ton error");
		 }
		 
		 public void discard (int state, int token, string name, Object value) {
			 println("discard\tstate "+state+"\ttoken "+name+"\tvalue "+value);
		 }
		 
		 public void reduce (int from, int to, int rule, string text, int len) {
			 println("reduce\tstate "+from+"\tuncover "+to
				     +"\trule ("+rule+") "+text);
		 }
		 
		 public void shift (int from, int to) {
			 println("goto\tfrom state "+from+" to "+to);
		 }
		 
		 public void accept (Object value) {
			 println("accept\tvalue "+value);
		 }
		 
		 public void error (string message) {
			 println("error\t"+message);
		 }
		 
		 public void reject () {
			 println("reject");
		 }
		 
	 }
}
// %token constants
 class Token {
  public const int TRUE_LITERAL = 257;
  public const int FALSE_LITERAL = 258;
  public const int EQ = 259;
  public const int NE = 261;
  public const int GT = 263;
  public const int GE = 264;
  public const int LT = 266;
  public const int LE = 267;
  public const int AND = 269;
  public const int OR = 270;
  public const int NOT = 271;
  public const int DOT = 272;
  public const int COMMA = 273;
  public const int PROP_OPEN = 274;
  public const int ITEM_OPEN = 276;
  public const int METADATA_OPEN = 278;
  public const int PAREN_OPEN = 280;
  public const int PAREN_CLOSE = 281;
  public const int COLON2 = 282;
  public const int ARROW = 284;
  public const int NAME = 286;
  public const int ERROR = 287;
  public const int yyErrorCode = 256;
 }
 namespace yyParser {
  using System;
  /** thrown for irrecoverable syntax errors and stack overflow.
    */
  internal class yyException : System.Exception {
    public yyException (string message) : base (message) {
    }
  }
  internal class yyUnexpectedEof : yyException {
    public yyUnexpectedEof (string message) : base (message) {
    }
    public yyUnexpectedEof () : base ("") {
    }
  }

  /** must be implemented by a scanner object to supply input to the parser.
    */
  internal interface yyInput {
    /** move on to next token.
        @return false if positioned beyond tokens.
        @throws IOException on input error.
      */
    bool advance (); // throws java.io.IOException;
    /** classifies current token.
        Should not be called if advance() returned false.
        @return current %token or single character.
      */
    int token ();
    /** associated with current token.
        Should not be called if advance() returned false.
        @return value for token().
      */
    Object value ();
  }
 }
} // close outermost namespace, that MUST HAVE BEEN opened in the prolog
