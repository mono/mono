// created by jay 0.7 (c) 1998 Axel.Schreiner@informatik.uni-osnabrueck.de

#line 2 "Parser.jay"
// XPath parser
//
// Author - Piers Haken <piersh@friskit.com>
//

// TODO: FUNCTION_CALL should be a QName, not just a NCName
// TODO: PROCESSING_INSTRUCTION's optional parameter
// TODO: flatten argument/predicate lists in place

using System;
using System.Xml.XPath;

namespace Mono.Xml.XPath
{
	public class XPathParser
	{
		internal object yyparseSafe (Tokenizer tok)
		{
			return yyparseSafe (tok, null);
		}

		internal object yyparseSafe (Tokenizer tok, object yyDebug)
		{
			try
			{
				return yyparse (tok, yyDebug);
			}
			catch (XPathException e)
			{
				throw e;
			}
			catch (Exception e)
			{
				throw new XPathException ("Error during parse", e);
			}
		}

		internal object yyparseDebug (Tokenizer tok)
		{
			return yyparseSafe (tok, new yydebug.yyDebugSimple ());
		}

#line default

  /** simplified error message.
      @see <a href="#yyerror(java.lang.String, java.lang.String[])">yyerror</a>
    */
  public void yyerror (string message) {
    yyerror(message, null);
  }

  /** (syntax) error message.
      Can be overwritten to control message format.
      @param message text to be displayed.
      @param expected vector of acceptable tokens, if available.
    */
  public void yyerror (string message, string[] expected) {
    if ((expected != null) && (expected.Length  > 0)) {
      System.Console.Write (message+", expecting");
      for (int n = 0; n < expected.Length; ++ n)
        System.Console.Write (" "+expected[n]);
        System.Console.WriteLine ();
    } else
      System.Console.WriteLine (message);
  }

  /** debugging support, requires the package jay.yydebug.
      Set to null to suppress debugging messages.
    */
  protected yydebug.yyDebug debug;

  protected static  int yyFinal = 25;
  public static  string [] yyRule = {
    "$accept : Expr",
    "Expr : OrExpr",
    "OrExpr : AndExpr",
    "OrExpr : OrExpr OR AndExpr",
    "AndExpr : EqualityExpr",
    "AndExpr : AndExpr AND EqualityExpr",
    "EqualityExpr : RelationalExpr",
    "EqualityExpr : EqualityExpr EQ RelationalExpr",
    "EqualityExpr : EqualityExpr NE RelationalExpr",
    "RelationalExpr : AdditiveExpr",
    "RelationalExpr : RelationalExpr LT AdditiveExpr",
    "RelationalExpr : RelationalExpr GT AdditiveExpr",
    "RelationalExpr : RelationalExpr LE AdditiveExpr",
    "RelationalExpr : RelationalExpr GE AdditiveExpr",
    "AdditiveExpr : MultiplicativeExpr",
    "AdditiveExpr : AdditiveExpr PLUS MultiplicativeExpr",
    "AdditiveExpr : AdditiveExpr MINUS MultiplicativeExpr",
    "MultiplicativeExpr : UnaryExpr",
    "MultiplicativeExpr : MultiplicativeExpr MULTIPLY UnaryExpr",
    "MultiplicativeExpr : MultiplicativeExpr DIV UnaryExpr",
    "MultiplicativeExpr : MultiplicativeExpr MOD UnaryExpr",
    "UnaryExpr : UnionExpr",
    "UnaryExpr : MINUS UnaryExpr",
    "UnionExpr : PathExpr",
    "UnionExpr : UnionExpr BAR PathExpr",
    "PathExpr : RelativeLocationPath",
    "PathExpr : SLASH",
    "PathExpr : SLASH RelativeLocationPath",
    "PathExpr : SLASH2 RelativeLocationPath",
    "PathExpr : FilterExpr",
    "PathExpr : FilterExpr SLASH RelativeLocationPath",
    "PathExpr : FilterExpr SLASH2 RelativeLocationPath",
    "RelativeLocationPath : Step",
    "RelativeLocationPath : RelativeLocationPath SLASH Step",
    "RelativeLocationPath : RelativeLocationPath SLASH2 Step",
    "Step : AxisSpecifier QName ZeroOrMorePredicates",
    "Step : AxisSpecifier ASTERISK ZeroOrMorePredicates",
    "Step : AxisSpecifier NodeType PAREN_OPEN OptionalLiteral PAREN_CLOSE ZeroOrMorePredicates",
    "Step : DOT",
    "Step : DOT2",
    "AxisSpecifier :",
    "AxisSpecifier : AT",
    "AxisSpecifier : AxisName COLON2",
    "NodeType : COMMENT",
    "NodeType : TEXT",
    "NodeType : PROCESSING_INSTRUCTION",
    "NodeType : NODE",
    "FilterExpr : PrimaryExpr",
    "FilterExpr : FilterExpr Predicate",
    "PrimaryExpr : DOLLAR QName",
    "PrimaryExpr : PAREN_OPEN Expr PAREN_CLOSE",
    "PrimaryExpr : LITERAL",
    "PrimaryExpr : NUMBER",
    "PrimaryExpr : FunctionCall",
    "FunctionCall : FUNCTION_NAME PAREN_OPEN OptionalArgumentList PAREN_CLOSE",
    "OptionalArgumentList :",
    "OptionalArgumentList : Expr OptionalArgumentListTail",
    "OptionalArgumentListTail :",
    "OptionalArgumentListTail : COMMA Expr OptionalArgumentListTail",
    "ZeroOrMorePredicates :",
    "ZeroOrMorePredicates : Predicate ZeroOrMorePredicates",
    "Predicate : BRACKET_OPEN Expr BRACKET_CLOSE",
    "AxisName : ANCESTOR",
    "AxisName : ANCESTOR_OR_SELF",
    "AxisName : ATTRIBUTE",
    "AxisName : CHILD",
    "AxisName : DESCENDANT",
    "AxisName : DESCENDANT_OR_SELF",
    "AxisName : FOLLOWING",
    "AxisName : FOLLOWING_SIBLING",
    "AxisName : NAMESPACE",
    "AxisName : PARENT",
    "AxisName : PRECEDING",
    "AxisName : PRECEDING_SIBLING",
    "AxisName : SELF",
    "OptionalLiteral :",
    "OptionalLiteral : LITERAL",
    "QName : NCName",
    "QName : NCName COLON ASTERISK",
    "QName : NCName COLON NCName",
  };
  protected static  string [] yyName = {    
    "end-of-file",null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,"'$'",null,null,
    null,"'('","')'","'*'","'+'","','","'-'","'.'","'/'",null,null,null,
    null,null,null,null,null,null,null,"':'",null,"'<'","'='","'>'",null,
    "'@'",null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    "'['",null,"']'",null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,"'|'",null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    "ERROR","EOF","SLASH","SLASH2","\"//\"","DOT","DOT2","\"..\"","COLON",
    "COLON2","\"::\"","COMMA","AT","FUNCTION_NAME","BRACKET_OPEN",
    "BRACKET_CLOSE","PAREN_OPEN","PAREN_CLOSE","AND","\"and\"","OR",
    "\"or\"","DIV","\"div\"","MOD","\"mod\"","PLUS","MINUS","ASTERISK",
    "DOLLAR","BAR","EQ","NE","\"!=\"","LE","\"<=\"","GE","\">=\"","LT",
    "GT","ANCESTOR","\"ancestor\"","ANCESTOR_OR_SELF",
    "\"ancstor-or-self\"","ATTRIBUTE","\"attribute\"","CHILD","\"child\"",
    "DESCENDANT","\"descendant\"","DESCENDANT_OR_SELF",
    "\"descendant-or-self\"","FOLLOWING","\"following\"",
    "FOLLOWING_SIBLING","\"sibling\"","NAMESPACE","\"NameSpace\"",
    "PARENT","\"parent\"","PRECEDING","\"preceding\"","PRECEDING_SIBLING",
    "\"preceding-sibling\"","SELF","\"self\"","COMMENT","\"comment\"",
    "TEXT","\"text\"","PROCESSING_INSTRUCTION",
    "\"processing-instruction\"","NODE","\"node\"","MULTIPLY","NUMBER",
    "LITERAL","NCName",
  };

  /** index-checked interface to yyName[].
      @param token single character or %token value.
      @return token name or [illegal] or [unknown].
    */
  public static string yyname (int token) {
    if ((token < 0) || (token > yyName.Length)) return "[illegal]";
    string name;
    if ((name = yyName[token]) != null) return name;
    return "[unknown]";
  }

  /** computes list of expected tokens on error by tracing the tables.
      @param state for which to compute the list.
      @return list of token names.
    */
  protected string[] yyExpecting (int state) {
    int token, n, len = 0;
    bool[] ok = new bool[yyName.Length];

    if ((n = yySindex[state]) != 0)
      for (token = n < 0 ? -n : 0;
           (token < yyName.Length) && (n+token < yyTable.Length); ++ token)
        if (yyCheck[n+token] == token && !ok[token] && yyName[token] != null) {
          ++ len;
          ok[token] = true;
        }
    if ((n = yyRindex[state]) != 0)
      for (token = n < 0 ? -n : 0;
           (token < yyName.Length) && (n+token < yyTable.Length); ++ token)
        if (yyCheck[n+token] == token && !ok[token] && yyName[token] != null) {
          ++ len;
          ok[token] = true;
        }

    string [] result = new string[len];
    for (n = token = 0; n < len;  ++ token)
      if (ok[token]) result[n++] = yyName[token];
    return result;
  }

  /** the generated parser, with debugging messages.
      Maintains a state and a value stack, currently with fixed maximum size.
      @param yyLex scanner.
      @param yydebug debug message writer implementing yyDebug, or null.
      @return result of the last reduction, if any.
      @throws yyException on irrecoverable parse error.
    */
  public Object yyparse (yyParser.yyInput yyLex, Object yyd)
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

  /** the generated parser.
      Maintains a state and a value stack, currently with fixed maximum size.
      @param yyLex scanner.
      @return result of the last reduction, if any.
      @throws yyException on irrecoverable parse error.
    */
  public Object yyparse (yyParser.yyInput yyLex)
				{
    if (yyMax <= 0) yyMax = 256;			// initial size
    int yyState = 0;                                   // state stack ptr
    int [] yyStates = new int[yyMax];	                // state stack 
    Object yyVal = null;                               // value stack ptr
    Object [] yyVals = new Object[yyMax];	        // value stack
    int yyToken = -1;					// current input
    int yyErrorFlag = 0;				// #tks to shift

    int yyTop = 0;
    goto skip;
    yyLoop:
    yyTop++;
    skip:
    for (;; ++ yyTop) {
      if (yyTop >= yyStates.Length) {			// dynamically increase
        int[] i = new int[yyStates.Length+yyMax];
        yyStates.CopyTo (i, 0);
        yyStates = i;
        Object[] o = new Object[yyVals.Length+yyMax];
        yyVals.CopyTo (o, 0);
        yyVals = o;
      }
      yyStates[yyTop] = yyState;
      yyVals[yyTop] = yyVal;
      if (debug != null) debug.push(yyState, yyVal);

      yyDiscarded: for (;;) {	// discarding a token does not change stack
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
            goto yyLoop;
          }
          if ((yyN = yyRindex[yyState]) != 0 && (yyN += yyToken) >= 0
              && yyN < yyTable.Length && yyCheck[yyN] == yyToken)
            yyN = yyTable[yyN];			// reduce (yyN)
          else
            switch (yyErrorFlag) {
  
            case 0:
              yyerror("syntax error", yyExpecting(yyState));
              if (debug != null) debug.error("syntax error");
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
                  goto yyLoop;
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
              goto yyDiscarded;		// leave stack alone
            }
        }
        int yyV = yyTop + 1-yyLen[yyN];
        if (debug != null)
          debug.reduce(yyState, yyStates[yyV-1], yyN, yyRule[yyN], yyLen[yyN]);
        yyVal = yyDefault(yyV > yyTop ? null : yyVals[yyV]);
        switch (yyN) {
case 3:
#line 134 "Parser.jay"
  {
		yyVal = new ExprOR ((Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	}
  break;
case 5:
#line 142 "Parser.jay"
  {
		yyVal = new ExprAND ((Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	}
  break;
case 7:
#line 150 "Parser.jay"
  {
		yyVal = new ExprEQ ((Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	}
  break;
case 8:
#line 154 "Parser.jay"
  {
		yyVal = new ExprNE ((Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	}
  break;
case 10:
#line 162 "Parser.jay"
  {
		yyVal = new ExprLT ((Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	}
  break;
case 11:
#line 166 "Parser.jay"
  {
		yyVal = new ExprGT ((Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	}
  break;
case 12:
#line 170 "Parser.jay"
  {
		yyVal = new ExprLE ((Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	}
  break;
case 13:
#line 174 "Parser.jay"
  {
		yyVal = new ExprGE ((Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	}
  break;
case 15:
#line 182 "Parser.jay"
  {
		yyVal = new ExprPLUS ((Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	}
  break;
case 16:
#line 186 "Parser.jay"
  {
		yyVal = new ExprMINUS ((Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	}
  break;
case 18:
#line 194 "Parser.jay"
  {
		yyVal = new ExprMULT ((Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	}
  break;
case 19:
#line 198 "Parser.jay"
  {
		yyVal = new ExprDIV ((Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	}
  break;
case 20:
#line 202 "Parser.jay"
  {
		yyVal = new ExprMOD ((Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	}
  break;
case 22:
#line 210 "Parser.jay"
  {
		yyVal = new ExprNEG ((Expression) yyVals[0+yyTop]);
	}
  break;
case 24:
#line 218 "Parser.jay"
  {
		yyVal = new ExprUNION ((NodeSet) yyVals[-2+yyTop], (NodeSet) yyVals[0+yyTop]);
	}
  break;
case 26:
#line 226 "Parser.jay"
  {
		yyVal = new ExprRoot ();
	}
  break;
case 27:
#line 230 "Parser.jay"
  {
		yyVal = new ExprSLASH (new ExprRoot (), (NodeSet) yyVals[0+yyTop]);
	}
  break;
case 28:
#line 234 "Parser.jay"
  {
		ExprStep exprStep = new ExprStep (new NodeTypeTest (Axes.DescendantOrSelf, XPathNodeType.All));
		yyVal = new ExprSLASH (new ExprSLASH (new ExprRoot (), exprStep), (NodeSet) yyVals[0+yyTop]);
	}
  break;
case 30:
#line 240 "Parser.jay"
  {
		yyVal = new ExprSLASH ((Expression) yyVals[-2+yyTop], (NodeSet) yyVals[0+yyTop]);
	}
  break;
case 31:
#line 244 "Parser.jay"
  {
		ExprStep exprStep = new ExprStep (new NodeTypeTest (Axes.DescendantOrSelf, XPathNodeType.All));
		yyVal = new ExprSLASH (new ExprSLASH ((Expression) yyVals[-2+yyTop], exprStep), (NodeSet) yyVals[0+yyTop]);
	}
  break;
case 33:
#line 253 "Parser.jay"
  {
		yyVal = new ExprSLASH ((Expression) yyVals[-2+yyTop], (NodeSet) yyVals[0+yyTop]);
	}
  break;
case 34:
#line 257 "Parser.jay"
  {
		ExprStep exprStep = new ExprStep (new NodeTypeTest (Axes.DescendantOrSelf, XPathNodeType.All));
		yyVal = new ExprSLASH (new ExprSLASH ((Expression) yyVals[-2+yyTop], exprStep), (NodeSet) yyVals[0+yyTop]);
	}
  break;
case 35:
#line 265 "Parser.jay"
  {
		yyVal = new ExprStep (new NodeNameTest ((Axes) yyVals[-2+yyTop], (QName) yyVals[-1+yyTop]), (ExprPredicates) yyVals[0+yyTop]);
	}
  break;
case 36:
#line 269 "Parser.jay"
  {
		yyVal = new ExprStep (new NodeTypeTest ((Axes) yyVals[-2+yyTop]), (ExprPredicates) yyVals[0+yyTop]);
	}
  break;
case 37:
#line 273 "Parser.jay"
  {
		yyVal = new ExprStep (new NodeTypeTest ((Axes) yyVals[-5+yyTop], (XPathNodeType) yyVals[-4+yyTop], (String) yyVals[-2+yyTop]), (ExprPredicates) yyVals[0+yyTop]);
	}
  break;
case 38:
#line 277 "Parser.jay"
  {
		yyVal = new ExprStep (new NodeTypeTest (Axes.Self, XPathNodeType.All));
	}
  break;
case 39:
#line 281 "Parser.jay"
  {
		yyVal = new ExprStep (new NodeTypeTest (Axes.Parent, XPathNodeType.All));
	}
  break;
case 40:
#line 288 "Parser.jay"
  {
		yyVal = Axes.Child;
	}
  break;
case 41:
#line 292 "Parser.jay"
  {
		yyVal = Axes.Attribute;
	}
  break;
case 42:
#line 296 "Parser.jay"
  {
		yyVal = yyVals[-1+yyTop];
	}
  break;
case 43:
#line 302 "Parser.jay"
  { yyVal = XPathNodeType.Comment; }
  break;
case 44:
#line 303 "Parser.jay"
  { yyVal = XPathNodeType.Text; }
  break;
case 45:
#line 304 "Parser.jay"
  { yyVal = XPathNodeType.ProcessingInstruction; }
  break;
case 46:
#line 305 "Parser.jay"
  { yyVal = XPathNodeType.All; }
  break;
case 48:
#line 312 "Parser.jay"
  {
		yyVal = new ExprFilter ((Expression) yyVals[-1+yyTop], (Expression) yyVals[0+yyTop]);
	}
  break;
case 49:
#line 319 "Parser.jay"
  {
		yyVal = new ExprVariable ((QName) yyVals[0+yyTop]);
	}
  break;
case 50:
#line 323 "Parser.jay"
  {
		yyVal = yyVals[-1+yyTop];
	}
  break;
case 51:
#line 327 "Parser.jay"
  {
		yyVal = new ExprLiteral ((String) yyVals[0+yyTop]);
	}
  break;
case 52:
#line 331 "Parser.jay"
  {
		yyVal = new ExprNumber ((double) yyVals[0+yyTop]);
	}
  break;
case 54:
#line 339 "Parser.jay"
  {
		yyVal = new ExprFunctionCall ((String) yyVals[-3+yyTop], (FunctionArguments) yyVals[-1+yyTop]);
	}
  break;
case 56:
#line 347 "Parser.jay"
  {
		yyVal = new FunctionArguments ((Expression) yyVals[-1+yyTop], (FunctionArguments) yyVals[0+yyTop]);
	}
  break;
case 58:
#line 355 "Parser.jay"
  {
		yyVal = new FunctionArguments ((Expression) yyVals[-1+yyTop], (FunctionArguments) yyVals[0+yyTop]);
	}
  break;
case 60:
#line 364 "Parser.jay"
  {
		yyVal = new ExprPredicates ((Expression) yyVals[-1+yyTop], (ExprPredicates) yyVals[0+yyTop]);
	}
  break;
case 61:
#line 371 "Parser.jay"
  {
		yyVal = yyVals[-1+yyTop];
	}
  break;
case 62:
#line 377 "Parser.jay"
  { yyVal = Axes.Ancestor; }
  break;
case 63:
#line 378 "Parser.jay"
  { yyVal = Axes.AncestorOrSelf; }
  break;
case 64:
#line 379 "Parser.jay"
  { yyVal = Axes.Attribute; }
  break;
case 65:
#line 380 "Parser.jay"
  { yyVal = Axes.Child; }
  break;
case 66:
#line 381 "Parser.jay"
  { yyVal = Axes.Descendant; }
  break;
case 67:
#line 382 "Parser.jay"
  { yyVal = Axes.DescendantOrSelf; }
  break;
case 68:
#line 383 "Parser.jay"
  { yyVal = Axes.Following; }
  break;
case 69:
#line 384 "Parser.jay"
  { yyVal = Axes.FollowingSibling; }
  break;
case 70:
#line 385 "Parser.jay"
  { yyVal = Axes.Namespace; }
  break;
case 71:
#line 386 "Parser.jay"
  { yyVal = Axes.Parent; }
  break;
case 72:
#line 387 "Parser.jay"
  { yyVal = Axes.Preceding; }
  break;
case 73:
#line 388 "Parser.jay"
  { yyVal = Axes.PrecedingSibling; }
  break;
case 74:
#line 389 "Parser.jay"
  { yyVal = Axes.Self; }
  break;
case 77:
#line 399 "Parser.jay"
  {
		yyVal = new NCName ((String) yyVals[0+yyTop]);
	}
  break;
case 78:
#line 403 "Parser.jay"
  {
		yyVal = new QName ((String) yyVals[-2+yyTop], null);
	}
  break;
case 79:
#line 407 "Parser.jay"
  {
		yyVal = new QName ((String) yyVals[-2+yyTop], (String) yyVals[0+yyTop]);
	}
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
          goto yyLoop;
        }
        if (((yyN = yyGindex[yyM]) != 0) && ((yyN += yyState) >= 0)
            && (yyN < yyTable.Length) && (yyCheck[yyN] == yyState))
          yyState = yyTable[yyN];
        else
          yyState = yyDgoto[yyM];
        if (debug != null) debug.shift(yyStates[yyTop], yyState);
	 goto yyLoop;
      }
    }
  }

   static  short [] yyLhs  = {              -1,
    0,    1,    1,    2,    2,    3,    3,    3,    4,    4,
    4,    4,    4,    5,    5,    5,    6,    6,    6,    6,
    7,    7,    8,    8,    9,    9,    9,    9,    9,    9,
    9,   10,   10,   10,   12,   12,   12,   12,   12,   13,
   13,   13,   16,   16,   16,   16,   11,   11,   19,   19,
   19,   19,   19,   21,   22,   22,   23,   23,   15,   15,
   20,   18,   18,   18,   18,   18,   18,   18,   18,   18,
   18,   18,   18,   18,   17,   17,   14,   14,   14,
  };
   static  short [] yyLen = {           2,
    1,    1,    3,    1,    3,    1,    3,    3,    1,    3,
    3,    3,    3,    1,    3,    3,    1,    3,    3,    3,
    1,    2,    1,    3,    1,    1,    2,    2,    1,    3,
    3,    1,    3,    3,    3,    3,    6,    1,    1,    0,
    1,    2,    1,    1,    1,    1,    1,    2,    2,    3,
    1,    1,    1,    4,    0,    2,    0,    3,    0,    2,
    3,    1,    1,    1,    1,    1,    1,    1,    1,    1,
    1,    1,    1,    1,    0,    1,    1,    3,    3,
  };
   static  short [] yyDefRed = {            0,
    0,    0,   38,   39,   41,    0,    0,    0,    0,   62,
   63,   64,   65,   66,   67,   68,   69,   70,   71,   72,
   73,   74,   52,   51,    0,    0,    0,    0,    0,    0,
    0,   17,    0,   23,    0,    0,   32,    0,    0,   47,
   53,    0,    0,    0,    0,   22,    0,   49,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,   48,    0,   43,
   44,   45,   46,    0,    0,   42,    0,    0,   50,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
   19,   20,   18,   24,   33,   34,    0,    0,    0,   36,
    0,   35,    0,    0,   56,   54,   78,   79,   61,   60,
   76,    0,    0,    0,   58,   37,
  };
  protected static  short [] yyDgoto  = {            25,
   26,   27,   28,   29,   30,   31,   32,   33,   34,   35,
   36,   37,   38,   48,  100,   75,  112,   39,   40,  101,
   41,   78,  105,
  };
  protected static  short [] yySindex = {         -252,
 -126, -126,    0,    0,    0, -270, -252, -252, -329,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0, -265, -249, -260, -271, -241,
 -277,    0, -264,    0, -220, -244,    0, -221, -235,    0,
    0, -220, -220, -252, -239,    0, -217,    0, -252, -252,
 -252, -252, -252, -252, -252, -252, -252, -252, -252, -252,
 -252, -187, -126, -126, -126, -126, -252,    0, -227,    0,
    0,    0,    0, -227, -223,    0, -212, -216,    0, -266,
 -249, -260, -271, -271, -241, -241, -241, -241, -277, -277,
    0,    0,    0,    0,    0,    0, -220, -220, -210,    0,
 -227,    0, -267, -252,    0,    0,    0,    0,    0,    0,
    0, -195, -212, -227,    0,    0,
  };
  protected static  short [] yyRindex = {         -176,
    1, -176,    0,    0,    0,    0, -176, -176,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    9,  338,   38,   30,  409,
  325,    0,  299,    0,  129,  155,    0,    0,    0,    0,
    0,  184,  210, -233,    0,    0,   74,    0, -176, -176,
 -176, -176, -176, -176, -176, -176, -176, -176, -176, -176,
 -176, -176, -176, -176, -176, -176, -176,    0,  100,    0,
    0,    0,    0,  100,    0,    0, -183,    0,    0,    0,
  547,  291,  510,  529,  419,  445,  455,  481,  354,  380,
    0,    0,    0,    0,    0,    0,  239,  265,    0,    0,
  100,    0, -181, -176,    0,    0,    0,    0,    0,    0,
    0,    0, -183,  100,    0,    0,
  };
  protected static  short [] yyGindex = {           -7,
    0,   46,   48,   19,   85,   27,   28,    0,   41,   12,
    0,   93,    0,   67,  -68,    0,    0,    0,    0,   71,
    0,    0,   -2,
  };
  protected static  short [] yyTable = {            45,
   26,   59,   44,   60,   47,  102,    1,    2,    1,    3,
    4,   49,   42,   43,   65,   66,    5,    6,  107,   53,
    7,   54,   62,   55,   56,   50,   67,   51,   52,    6,
   76,    8,  110,    9,   79,   46,   77,    4,   63,   64,
   55,   57,   58,   67,   10,  116,   11,   80,   12,  103,
   13,   40,   14,   61,   15,  104,   16,  106,   17,   99,
   18,  109,   19,   69,   20,  111,   21,  108,   22,   83,
   84,    1,    2,   77,    3,    4,   97,   98,  114,   23,
   24,    5,    6,   89,   90,    7,   91,   92,   93,   40,
   57,   40,   75,   40,   81,   40,  113,   82,    9,   59,
   40,   70,   94,   71,   74,   72,   68,   73,   40,   10,
  115,   11,   47,   12,    0,   13,    0,   14,    0,   15,
    0,   16,    0,   17,    0,   18,    0,   19,   25,   20,
    0,   21,    0,   22,    0,    3,    4,   85,   86,   87,
   88,    0,    5,    0,   23,   24,   40,    0,   40,    0,
   40,    0,   40,    0,   29,   95,   96,   40,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
   10,    0,   11,    0,   12,    0,   13,    0,   14,    0,
   15,    0,   16,   27,   17,    0,   18,    0,   19,    0,
   20,    0,   21,    0,   22,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,   28,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,   30,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,   31,    0,    0,    0,   26,    0,
    0,    0,   26,    0,   26,   26,    1,   26,    0,   26,
    1,   26,    1,   26,   26,   40,    0,   26,   26,   26,
    5,   26,    0,   26,    0,   26,   26,    6,   21,    0,
    0,    6,    0,    6,    6,    4,    6,    0,    0,    4,
    0,    4,    4,    0,    4,    0,    0,    6,    6,    0,
    0,    0,    0,   40,   14,   40,    0,   40,    0,   40,
    0,   26,   77,   77,   40,    0,    0,    2,    0,    0,
    0,   77,    0,    0,   77,   77,    0,   77,   77,    0,
   77,    0,   77,   15,   77,    0,   77,   77,   59,   59,
   77,   77,   77,    0,   77,    0,   77,   59,   77,   77,
    0,   59,    0,   59,   59,    0,   59,    0,   59,   16,
   59,    0,   59,   59,    0,    0,   59,   59,   59,    0,
   59,    0,   59,    0,   59,   59,   25,    0,    0,    0,
   25,    0,   25,   25,   77,   25,    0,   25,    9,   25,
    0,   25,   25,    0,    0,   25,   25,   25,   12,   25,
    0,   25,   29,   25,   25,    0,   29,    0,   29,   29,
   59,   29,    0,   29,    0,   29,    0,   29,   29,    0,
    0,   29,   29,   29,   13,   29,    0,   29,    0,   29,
   29,   27,    0,    0,   10,   27,    0,   27,   27,   25,
   27,    0,   27,    0,   27,    0,   27,   27,    0,    0,
   27,   27,   27,    0,   27,    0,   27,   28,   27,   27,
   11,   28,    0,   28,   28,   29,   28,    0,   28,    0,
   28,    0,   28,   28,    0,    0,   28,   28,   28,    0,
   28,    0,   28,    0,   28,   28,   30,    0,    0,    7,
   30,    0,   30,   30,   27,   30,    0,   30,    0,   30,
    0,   30,   30,    0,    0,   30,   30,   30,    8,   30,
    0,   30,   31,   30,   30,    0,   31,    0,   31,   31,
   28,   31,    0,   31,    0,   31,    3,   31,   31,    0,
    0,   31,   31,   31,    0,   31,    0,   31,    5,   31,
   31,    0,    5,    0,    5,    5,   21,    5,    0,   30,
   21,    0,   21,   21,    0,   21,    0,   21,    0,   21,
    0,   21,   21,    0,    0,    0,   21,   21,    0,   21,
    0,   21,   14,   21,   21,   31,   14,    0,   14,   14,
    0,   14,    0,    0,    0,    2,    0,   14,   14,    2,
    0,    2,   14,   14,    2,   14,    0,   14,    0,   14,
   14,   15,    0,    0,    0,   15,    0,   15,   15,   21,
   15,    0,    0,    0,    0,    0,   15,   15,    0,    0,
    0,   15,   15,    0,   15,    0,   15,   16,   15,   15,
    0,   16,    0,   16,   16,    0,   16,    0,    0,    0,
    0,    0,   16,   16,    0,    0,    0,   16,   16,    0,
   16,    0,   16,    0,   16,   16,    9,    0,    0,    0,
    9,    0,    9,    9,    0,    9,   12,    0,    0,    0,
   12,    0,   12,   12,    0,   12,    9,    9,    0,    9,
    0,    9,    0,    9,    9,    0,   12,   12,    0,   12,
    0,   12,   13,   12,   12,    0,   13,    0,   13,   13,
    0,   13,   10,    0,    0,    0,   10,    0,   10,   10,
    0,   10,   13,   13,    0,   13,    0,   13,    0,   13,
   13,    0,   10,   10,    0,   10,    0,   10,   11,   10,
   10,    0,   11,    0,   11,   11,    0,   11,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,   11,   11,
    0,   11,    0,   11,    0,   11,   11,    7,    0,    0,
    0,    7,    0,    7,    7,    0,    7,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    8,    7,    7,    0,
    8,    0,    8,    8,    0,    8,    0,    0,    0,    0,
    0,    0,    0,    0,    3,    0,    8,    8,    3,    0,
    3,    0,    0,    3,
  };
  protected static  short [] yyCheck = {             7,
    0,  279,  273,  281,  334,   74,  259,  260,    0,  262,
  263,  277,    1,    2,  259,  260,  269,  270,  285,  291,
  273,  293,  287,  295,  296,  275,  271,  288,  289,    0,
  266,  284,  101,  286,  274,    8,   44,    0,  259,  260,
  274,  283,  284,  271,  297,  114,  299,  265,  301,  273,
  303,  285,  305,  331,  307,  268,  309,  274,  311,   67,
  313,  272,  315,  285,  317,  333,  319,  334,  321,   51,
   52,  259,  260,    0,  262,  263,   65,   66,  274,  332,
  333,  269,  270,   57,   58,  273,   59,   60,   61,  323,
  274,  325,  274,  327,   49,  329,  104,   50,  286,    0,
  334,  323,   62,  325,   38,  327,   36,  329,  285,  297,
  113,  299,  334,  301,   -1,  303,   -1,  305,   -1,  307,
   -1,  309,   -1,  311,   -1,  313,   -1,  315,    0,  317,
   -1,  319,   -1,  321,   -1,  262,  263,   53,   54,   55,
   56,   -1,  269,   -1,  332,  333,  323,   -1,  325,   -1,
  327,   -1,  329,   -1,    0,   63,   64,  334,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  297,   -1,  299,   -1,  301,   -1,  303,   -1,  305,   -1,
  307,   -1,  309,    0,  311,   -1,  313,   -1,  315,   -1,
  317,   -1,  319,   -1,  321,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,    0,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,    0,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,    0,   -1,   -1,   -1,  268,   -1,
   -1,   -1,  272,   -1,  274,  275,  268,  277,   -1,  279,
  272,  281,  274,  283,  284,  285,   -1,  287,  288,  289,
    0,  291,   -1,  293,   -1,  295,  296,  268,    0,   -1,
   -1,  272,   -1,  274,  275,  268,  277,   -1,   -1,  272,
   -1,  274,  275,   -1,  277,   -1,   -1,  288,  289,   -1,
   -1,   -1,   -1,  323,    0,  325,   -1,  327,   -1,  329,
   -1,  331,  259,  260,  334,   -1,   -1,    0,   -1,   -1,
   -1,  268,   -1,   -1,  271,  272,   -1,  274,  275,   -1,
  277,   -1,  279,    0,  281,   -1,  283,  284,  259,  260,
  287,  288,  289,   -1,  291,   -1,  293,  268,  295,  296,
   -1,  272,   -1,  274,  275,   -1,  277,   -1,  279,    0,
  281,   -1,  283,  284,   -1,   -1,  287,  288,  289,   -1,
  291,   -1,  293,   -1,  295,  296,  268,   -1,   -1,   -1,
  272,   -1,  274,  275,  331,  277,   -1,  279,    0,  281,
   -1,  283,  284,   -1,   -1,  287,  288,  289,    0,  291,
   -1,  293,  268,  295,  296,   -1,  272,   -1,  274,  275,
  331,  277,   -1,  279,   -1,  281,   -1,  283,  284,   -1,
   -1,  287,  288,  289,    0,  291,   -1,  293,   -1,  295,
  296,  268,   -1,   -1,    0,  272,   -1,  274,  275,  331,
  277,   -1,  279,   -1,  281,   -1,  283,  284,   -1,   -1,
  287,  288,  289,   -1,  291,   -1,  293,  268,  295,  296,
    0,  272,   -1,  274,  275,  331,  277,   -1,  279,   -1,
  281,   -1,  283,  284,   -1,   -1,  287,  288,  289,   -1,
  291,   -1,  293,   -1,  295,  296,  268,   -1,   -1,    0,
  272,   -1,  274,  275,  331,  277,   -1,  279,   -1,  281,
   -1,  283,  284,   -1,   -1,  287,  288,  289,    0,  291,
   -1,  293,  268,  295,  296,   -1,  272,   -1,  274,  275,
  331,  277,   -1,  279,   -1,  281,    0,  283,  284,   -1,
   -1,  287,  288,  289,   -1,  291,   -1,  293,  268,  295,
  296,   -1,  272,   -1,  274,  275,  268,  277,   -1,  331,
  272,   -1,  274,  275,   -1,  277,   -1,  279,   -1,  281,
   -1,  283,  284,   -1,   -1,   -1,  288,  289,   -1,  291,
   -1,  293,  268,  295,  296,  331,  272,   -1,  274,  275,
   -1,  277,   -1,   -1,   -1,  268,   -1,  283,  284,  272,
   -1,  274,  288,  289,  277,  291,   -1,  293,   -1,  295,
  296,  268,   -1,   -1,   -1,  272,   -1,  274,  275,  331,
  277,   -1,   -1,   -1,   -1,   -1,  283,  284,   -1,   -1,
   -1,  288,  289,   -1,  291,   -1,  293,  268,  295,  296,
   -1,  272,   -1,  274,  275,   -1,  277,   -1,   -1,   -1,
   -1,   -1,  283,  284,   -1,   -1,   -1,  288,  289,   -1,
  291,   -1,  293,   -1,  295,  296,  268,   -1,   -1,   -1,
  272,   -1,  274,  275,   -1,  277,  268,   -1,   -1,   -1,
  272,   -1,  274,  275,   -1,  277,  288,  289,   -1,  291,
   -1,  293,   -1,  295,  296,   -1,  288,  289,   -1,  291,
   -1,  293,  268,  295,  296,   -1,  272,   -1,  274,  275,
   -1,  277,  268,   -1,   -1,   -1,  272,   -1,  274,  275,
   -1,  277,  288,  289,   -1,  291,   -1,  293,   -1,  295,
  296,   -1,  288,  289,   -1,  291,   -1,  293,  268,  295,
  296,   -1,  272,   -1,  274,  275,   -1,  277,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  288,  289,
   -1,  291,   -1,  293,   -1,  295,  296,  268,   -1,   -1,
   -1,  272,   -1,  274,  275,   -1,  277,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  268,  288,  289,   -1,
  272,   -1,  274,  275,   -1,  277,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  268,   -1,  288,  289,  272,   -1,
  274,   -1,   -1,  277,
  };

#line 413 "Parser.jay"
	}
#line default
namespace yydebug {
        using System;
	 public interface yyDebug {
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
			 Console.WriteLine (s);
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
  public const int ERROR = 257;
  public const int EOF = 258;
  public const int SLASH = 259;
  public const int SLASH2 = 260;
  public const int DOT = 262;
  public const int DOT2 = 263;
  public const int COLON = 265;
  public const int COLON2 = 266;
  public const int COMMA = 268;
  public const int AT = 269;
  public const int FUNCTION_NAME = 270;
  public const int BRACKET_OPEN = 271;
  public const int BRACKET_CLOSE = 272;
  public const int PAREN_OPEN = 273;
  public const int PAREN_CLOSE = 274;
  public const int AND = 275;
  public const int and = 276;
  public const int OR = 277;
  public const int or = 278;
  public const int DIV = 279;
  public const int div = 280;
  public const int MOD = 281;
  public const int mod = 282;
  public const int PLUS = 283;
  public const int MINUS = 284;
  public const int ASTERISK = 285;
  public const int DOLLAR = 286;
  public const int BAR = 287;
  public const int EQ = 288;
  public const int NE = 289;
  public const int LE = 291;
  public const int GE = 293;
  public const int LT = 295;
  public const int GT = 296;
  public const int ANCESTOR = 297;
  public const int ancestor = 298;
  public const int ANCESTOR_OR_SELF = 299;
  public const int ATTRIBUTE = 301;
  public const int attribute = 302;
  public const int CHILD = 303;
  public const int child = 304;
  public const int DESCENDANT = 305;
  public const int descendant = 306;
  public const int DESCENDANT_OR_SELF = 307;
  public const int FOLLOWING = 309;
  public const int following = 310;
  public const int FOLLOWING_SIBLING = 311;
  public const int sibling = 312;
  public const int NAMESPACE = 313;
  public const int NameSpace = 314;
  public const int PARENT = 315;
  public const int parent = 316;
  public const int PRECEDING = 317;
  public const int preceding = 318;
  public const int PRECEDING_SIBLING = 319;
  public const int SELF = 321;
  public const int self = 322;
  public const int COMMENT = 323;
  public const int comment = 324;
  public const int TEXT = 325;
  public const int text = 326;
  public const int PROCESSING_INSTRUCTION = 327;
  public const int NODE = 329;
  public const int node = 330;
  public const int MULTIPLY = 331;
  public const int NUMBER = 332;
  public const int LITERAL = 333;
  public const int NCName = 334;
  public const int yyErrorCode = 256;
 }
 namespace yyParser {
  using System;
  /** thrown for irrecoverable syntax errors and stack overflow.
    */
  public class yyException : System.Exception {
    public yyException (string message) : base (message) {
    }
  }

  /** must be implemented by a scanner object to supply input to the parser.
    */
  public interface yyInput {
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
