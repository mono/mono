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
using Test.Xml.XPath;

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

#line 48 "-"

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
    "AndExpr : AndExpr \"and\" EqualityExpr",
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
    "MultiplicativeExpr : MultiplicativeExpr ASTERISK UnaryExpr",
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
    "\"processing-instruction\"","NODE","\"node\"","NUMBER","LITERAL",
    "NCName",
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
#line 133 "Parser.jay"
  {
		yyVal = new ExprOR ((Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	}
  break;
case 5:
#line 141 "Parser.jay"
  {
		yyVal = new ExprAND ((Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	}
  break;
case 7:
#line 149 "Parser.jay"
  {
		yyVal = new ExprEQ ((Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	}
  break;
case 8:
#line 153 "Parser.jay"
  {
		yyVal = new ExprNE ((Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	}
  break;
case 10:
#line 161 "Parser.jay"
  {
		yyVal = new ExprLT ((Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	}
  break;
case 11:
#line 165 "Parser.jay"
  {
		yyVal = new ExprGT ((Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	}
  break;
case 12:
#line 169 "Parser.jay"
  {
		yyVal = new ExprLE ((Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	}
  break;
case 13:
#line 173 "Parser.jay"
  {
		yyVal = new ExprGE ((Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	}
  break;
case 15:
#line 181 "Parser.jay"
  {
		yyVal = new ExprPLUS ((Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	}
  break;
case 16:
#line 185 "Parser.jay"
  {
		yyVal = new ExprMINUS ((Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	}
  break;
case 18:
#line 193 "Parser.jay"
  {
		yyVal = new ExprMULT ((Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	}
  break;
case 19:
#line 197 "Parser.jay"
  {
		yyVal = new ExprDIV ((Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	}
  break;
case 20:
#line 201 "Parser.jay"
  {
		yyVal = new ExprMOD ((Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	}
  break;
case 22:
#line 209 "Parser.jay"
  {
		yyVal = new ExprNEG ((Expression) yyVals[0+yyTop]);
	}
  break;
case 24:
#line 217 "Parser.jay"
  {
		yyVal = new ExprUNION ((NodeSet) yyVals[-2+yyTop], (NodeSet) yyVals[0+yyTop]);
	}
  break;
case 26:
#line 225 "Parser.jay"
  {
		yyVal = new ExprRoot ();
	}
  break;
case 27:
#line 229 "Parser.jay"
  {
		yyVal = new ExprSLASH (new ExprRoot (), (NodeSet) yyVals[0+yyTop]);
	}
  break;
case 28:
#line 233 "Parser.jay"
  {
		ExprStep exprStep = new ExprStep (new NodeTypeTest (Axes.DescendantOrSelf, XPathNodeType.All));
		yyVal = new ExprSLASH (new ExprSLASH (new ExprRoot (), exprStep), (NodeSet) yyVals[0+yyTop]);
	}
  break;
case 30:
#line 239 "Parser.jay"
  {
		yyVal = new ExprSLASH ((Expression) yyVals[-2+yyTop], (NodeSet) yyVals[0+yyTop]);
	}
  break;
case 31:
#line 243 "Parser.jay"
  {
		ExprStep exprStep = new ExprStep (new NodeTypeTest (Axes.DescendantOrSelf, XPathNodeType.All));
		yyVal = new ExprSLASH (new ExprSLASH ((Expression) yyVals[-2+yyTop], exprStep), (NodeSet) yyVals[0+yyTop]);
	}
  break;
case 33:
#line 252 "Parser.jay"
  {
		yyVal = new ExprSLASH ((Expression) yyVals[-2+yyTop], (NodeSet) yyVals[0+yyTop]);
	}
  break;
case 34:
#line 256 "Parser.jay"
  {
		ExprStep exprStep = new ExprStep (new NodeTypeTest (Axes.DescendantOrSelf, XPathNodeType.All));
		yyVal = new ExprSLASH (new ExprSLASH ((Expression) yyVals[-2+yyTop], exprStep), (NodeSet) yyVals[0+yyTop]);
	}
  break;
case 35:
#line 264 "Parser.jay"
  {
		yyVal = new ExprStep (new NodeNameTest ((Axes) yyVals[-2+yyTop], (QName) yyVals[-1+yyTop]), (ExprPredicates) yyVals[0+yyTop]);
	}
  break;
case 36:
#line 268 "Parser.jay"
  {
		yyVal = new ExprStep (new NodeTypeTest ((Axes) yyVals[-2+yyTop]), (ExprPredicates) yyVals[0+yyTop]);
	}
  break;
case 37:
#line 272 "Parser.jay"
  {
		yyVal = new ExprStep (new NodeTypeTest ((Axes) yyVals[-5+yyTop], (XPathNodeType) yyVals[-4+yyTop], (String) yyVals[-2+yyTop]), (ExprPredicates) yyVals[0+yyTop]);
	}
  break;
case 38:
#line 276 "Parser.jay"
  {
		yyVal = new ExprStep (new NodeTypeTest (Axes.Self, XPathNodeType.All));
	}
  break;
case 39:
#line 280 "Parser.jay"
  {
		yyVal = new ExprStep (new NodeTypeTest (Axes.Parent, XPathNodeType.All));
	}
  break;
case 40:
#line 287 "Parser.jay"
  {
		yyVal = Axes.Child;
	}
  break;
case 41:
#line 291 "Parser.jay"
  {
		yyVal = Axes.Attribute;
	}
  break;
case 42:
#line 295 "Parser.jay"
  {
		yyVal = yyVals[-1+yyTop];
	}
  break;
case 43:
#line 301 "Parser.jay"
  { yyVal = XPathNodeType.Comment; }
  break;
case 44:
#line 302 "Parser.jay"
  { yyVal = XPathNodeType.Text; }
  break;
case 45:
#line 303 "Parser.jay"
  { yyVal = XPathNodeType.ProcessingInstruction; }
  break;
case 46:
#line 304 "Parser.jay"
  { yyVal = XPathNodeType.All; }
  break;
case 48:
#line 311 "Parser.jay"
  {
		yyVal = new ExprFilter ((Expression) yyVals[-1+yyTop], (Expression) yyVals[0+yyTop]);
	}
  break;
case 49:
#line 318 "Parser.jay"
  {
		yyVal = new ExprVariable ((QName) yyVals[0+yyTop]);
	}
  break;
case 50:
#line 322 "Parser.jay"
  {
		yyVal = yyVals[-1+yyTop];
	}
  break;
case 51:
#line 326 "Parser.jay"
  {
		yyVal = new ExprLiteral ((String) yyVals[0+yyTop]);
	}
  break;
case 52:
#line 330 "Parser.jay"
  {
		yyVal = new ExprNumber ((double) yyVals[0+yyTop]);
	}
  break;
case 54:
#line 338 "Parser.jay"
  {
		yyVal = new ExprFunctionCall ((String) yyVals[-3+yyTop], (FunctionArguments) yyVals[-1+yyTop]);
	}
  break;
case 56:
#line 346 "Parser.jay"
  {
		yyVal = new FunctionArguments ((Expression) yyVals[-1+yyTop], (FunctionArguments) yyVals[0+yyTop]);
	}
  break;
case 58:
#line 354 "Parser.jay"
  {
		yyVal = new FunctionArguments ((Expression) yyVals[-1+yyTop], (FunctionArguments) yyVals[0+yyTop]);
	}
  break;
case 60:
#line 363 "Parser.jay"
  {
		yyVal = new ExprPredicates ((Expression) yyVals[-1+yyTop], (ExprPredicates) yyVals[0+yyTop]);
	}
  break;
case 61:
#line 370 "Parser.jay"
  {
		yyVal = yyVals[-1+yyTop];
	}
  break;
case 62:
#line 376 "Parser.jay"
  { yyVal = Axes.Ancestor; }
  break;
case 63:
#line 377 "Parser.jay"
  { yyVal = Axes.AncestorOrSelf; }
  break;
case 64:
#line 378 "Parser.jay"
  { yyVal = Axes.Attribute; }
  break;
case 65:
#line 379 "Parser.jay"
  { yyVal = Axes.Child; }
  break;
case 66:
#line 380 "Parser.jay"
  { yyVal = Axes.Descendant; }
  break;
case 67:
#line 381 "Parser.jay"
  { yyVal = Axes.DescendantOrSelf; }
  break;
case 68:
#line 382 "Parser.jay"
  { yyVal = Axes.Following; }
  break;
case 69:
#line 383 "Parser.jay"
  { yyVal = Axes.FollowingSibling; }
  break;
case 70:
#line 384 "Parser.jay"
  { yyVal = Axes.Namespace; }
  break;
case 71:
#line 385 "Parser.jay"
  { yyVal = Axes.Parent; }
  break;
case 72:
#line 386 "Parser.jay"
  { yyVal = Axes.Preceding; }
  break;
case 73:
#line 387 "Parser.jay"
  { yyVal = Axes.PrecedingSibling; }
  break;
case 74:
#line 388 "Parser.jay"
  { yyVal = Axes.Self; }
  break;
case 77:
#line 398 "Parser.jay"
  {
		yyVal = new NCName ((String) yyVals[0+yyTop]);
	}
  break;
case 78:
#line 402 "Parser.jay"
  {
		yyVal = new QName ((String) yyVals[-2+yyTop], null);
	}
  break;
case 79:
#line 406 "Parser.jay"
  {
		yyVal = new QName ((String) yyVals[-2+yyTop], (String) yyVals[0+yyTop]);
	}
  break;
#line 691 "-"
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
  protected static  short [] yySindex = {         -231,
  -97,  -97,    0,    0,    0, -263, -231, -231, -318,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0, -256, -250, -282, -273, -259,
 -268,    0, -257,    0, -219, -255,    0, -252, -230,    0,
    0, -219, -219, -231, -223,    0, -206,    0, -231, -231,
 -231, -231, -231, -231, -231, -231, -231, -231, -231, -231,
 -231, -157,  -97,  -97,  -97,  -97, -231,    0, -204,    0,
    0,    0,    0, -204, -194,    0, -199, -183,    0, -276,
 -250, -282, -273, -273, -259, -259, -259, -259, -268, -268,
    0,    0,    0,    0,    0,    0, -219, -219, -176,    0,
 -204,    0, -234, -231,    0,    0,    0,    0,    0,    0,
    0, -170, -199, -204,    0,    0,
  };
  protected static  short [] yyRindex = {         -240,
    1, -240,    0,    0,    0,    0, -240, -240,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,   19,    2,   92,   27,  391,
  307,    0,  281,    0,  125,  151,    0,    0,    0,    0,
    0,  177,  203, -271,    0,    0,   61,    0, -240, -240,
 -240, -240, -240, -240, -240, -240, -240, -240, -240, -240,
 -240, -240, -240, -240, -240, -240, -240,    0,   99,    0,
    0,    0,    0,   99,    0,    0, -165,    0,    0,    0,
  424,  398,   34,  521,  417,  443,  469,  495,  336,  362,
    0,    0,    0,    0,    0,    0,  229,  255,    0,    0,
   99,    0, -164, -240,    0,    0,    0,    0,    0,    0,
    0,    0, -165,   99,    0,    0,
  };
  protected static  short [] yyGindex = {           -7,
    0,   62,   65,   -5,   64,   -8,    4,    0,   52,   42,
    0,   31,    0,   83,  -66,    0,    0,    0,    0,   86,
    0,    0,   10,
  };
  protected static  short [] yyTable = {            45,
   26,    2,   55,   65,   66,   51,   52,  102,  107,   44,
   59,   46,   60,   40,   47,   67,   61,   53,    1,   54,
   49,   55,   56,   57,   58,   50,    6,    1,    2,   62,
    3,    4,   69,    7,  110,   76,   77,    5,    6,   63,
   64,    7,   42,   43,   40,   83,   84,  116,   89,   90,
   79,   40,    8,   40,    9,   40,  108,   40,   80,   99,
   77,   40,   91,   92,   93,   10,   67,   11,  104,   12,
   70,   13,   71,   14,   72,   15,   73,   16,  103,   17,
   47,   18,   40,   19,   40,   20,   40,   21,   40,   22,
  106,    4,   40,   95,   96,  109,  113,  111,   59,   23,
   24,    1,    2,  114,    3,    4,   97,   98,   57,   75,
   81,    5,    6,   94,   82,    7,   85,   86,   87,   88,
   74,   68,  115,    0,   25,    0,    0,    0,    9,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,   10,
    0,   11,    0,   12,    0,   13,    0,   14,    0,   15,
   29,   16,    0,   17,    0,   18,    0,   19,    0,   20,
    0,   21,    0,   22,    3,    4,    0,    0,    0,    0,
    0,    5,    0,   23,   24,    0,   27,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,   10,
    0,   11,   28,   12,    0,   13,    0,   14,    0,   15,
    0,   16,    0,   17,    0,   18,    0,   19,    0,   20,
    0,   21,    0,   22,    0,    0,    0,    0,   30,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,   31,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,   26,    2,
    0,    0,   26,    2,   26,    2,   26,   26,    2,   26,
   21,   26,    0,   26,   26,   26,    1,   26,   26,   26,
    1,   26,    1,   26,    6,   26,   26,    0,    6,    0,
    6,    7,    6,    6,    0,    7,   14,    7,    0,    7,
    7,    0,    0,    0,    6,    6,    0,    0,    0,   77,
   77,    7,    7,   40,    0,   40,    0,   40,   77,   40,
    0,   77,   77,   40,   77,   15,   77,   77,    0,   77,
    0,   77,    0,   77,   77,   77,    0,   77,   77,   77,
    0,   77,    0,   77,    0,   77,   77,   59,   59,    4,
    0,   16,    0,    4,    0,    4,   59,    4,    4,    0,
   59,    0,   59,    0,   59,   59,    0,   59,    0,   59,
    0,   59,   59,   59,    0,   59,   59,   59,    0,   59,
    9,   59,   25,   59,   59,    0,   25,    5,   25,    0,
   25,   25,    0,   25,    0,   25,    0,   25,   25,   25,
    0,   25,   25,   25,    0,   25,   12,   25,   29,   25,
   25,    0,   29,    3,   29,    0,   29,   29,    0,   29,
    0,   29,    0,   29,   29,   29,    0,   29,   29,   29,
    0,   29,   13,   29,   27,   29,   29,    0,   27,    0,
   27,    0,   27,   27,    0,   27,    0,   27,    0,   27,
   27,   27,    0,   27,   27,   27,    0,   27,   10,   27,
   28,   27,   27,    0,   28,    0,   28,    0,   28,   28,
    0,   28,    0,   28,    0,   28,   28,   28,    0,   28,
   28,   28,    0,   28,   11,   28,   30,   28,   28,    0,
   30,    0,   30,    0,   30,   30,    0,   30,    0,   30,
    0,   30,   30,   30,    0,   30,   30,   30,    0,   30,
    8,   30,   31,   30,   30,    0,   31,    0,   31,    0,
   31,   31,    0,   31,    0,   31,    0,   31,   31,   31,
    0,   31,   31,   31,    0,   31,    0,   31,   21,   31,
   31,    0,   21,    0,   21,    0,   21,   21,    0,   21,
    0,   21,    0,   21,   21,   21,    0,    0,   21,   21,
    0,   21,    0,   21,   14,   21,   21,    0,   14,    0,
   14,    0,   14,   14,    0,    0,    0,    0,    0,   14,
   14,    0,    0,    0,   14,   14,    0,   14,    0,   14,
    0,   14,   14,   15,    0,    0,    0,   15,    0,   15,
    0,   15,   15,    0,    0,    0,    0,    0,   15,   15,
    0,    0,    0,   15,   15,    0,   15,    0,   15,   16,
   15,   15,    0,   16,    0,   16,    0,   16,   16,    0,
    0,    0,    0,    0,   16,   16,    0,    0,    0,   16,
   16,    0,   16,    0,   16,    0,   16,   16,    9,    0,
    0,    0,    9,    0,    9,    5,    9,    9,    0,    5,
    0,    5,    0,    5,    5,    0,    0,    0,    9,    9,
    0,    9,    0,    9,   12,    9,    9,    0,   12,    0,
   12,    3,   12,   12,    0,    3,    0,    3,    0,    0,
    3,    0,    0,    0,   12,   12,    0,   12,    0,   12,
   13,   12,   12,    0,   13,    0,   13,    0,   13,   13,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
   13,   13,    0,   13,    0,   13,   10,   13,   13,    0,
   10,    0,   10,    0,   10,   10,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,   10,   10,    0,   10,
    0,   10,   11,   10,   10,    0,   11,    0,   11,    0,
   11,   11,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,   11,   11,    0,   11,    0,   11,    8,   11,
   11,    0,    8,    0,    8,    0,    8,    8,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    8,    8,
  };
  protected static  short [] yyCheck = {             7,
    0,    0,  274,  259,  260,  288,  289,   74,  285,  273,
  279,    8,  281,  285,  333,  271,  285,  291,    0,  293,
  277,  295,  296,  283,  284,  276,    0,  259,  260,  287,
  262,  263,  285,    0,  101,  266,   44,  269,  270,  259,
  260,  273,    1,    2,  285,   51,   52,  114,   57,   58,
  274,  323,  284,  325,  286,  327,  333,  329,  265,   67,
    0,  333,   59,   60,   61,  297,  271,  299,  268,  301,
  323,  303,  325,  305,  327,  307,  329,  309,  273,  311,
  333,  313,  323,  315,  325,  317,  327,  319,  329,  321,
  274,    0,  333,   63,   64,  272,  104,  332,    0,  331,
  332,  259,  260,  274,  262,  263,   65,   66,  274,  274,
   49,  269,  270,   62,   50,  273,   53,   54,   55,   56,
   38,   36,  113,   -1,    0,   -1,   -1,   -1,  286,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  297,
   -1,  299,   -1,  301,   -1,  303,   -1,  305,   -1,  307,
    0,  309,   -1,  311,   -1,  313,   -1,  315,   -1,  317,
   -1,  319,   -1,  321,  262,  263,   -1,   -1,   -1,   -1,
   -1,  269,   -1,  331,  332,   -1,    0,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  297,
   -1,  299,    0,  301,   -1,  303,   -1,  305,   -1,  307,
   -1,  309,   -1,  311,   -1,  313,   -1,  315,   -1,  317,
   -1,  319,   -1,  321,   -1,   -1,   -1,   -1,    0,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,    0,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  268,  268,
   -1,   -1,  272,  272,  274,  274,  276,  277,  277,  279,
    0,  281,   -1,  283,  284,  285,  268,  287,  288,  289,
  272,  291,  274,  293,  268,  295,  296,   -1,  272,   -1,
  274,  268,  276,  277,   -1,  272,    0,  274,   -1,  276,
  277,   -1,   -1,   -1,  288,  289,   -1,   -1,   -1,  259,
  260,  288,  289,  323,   -1,  325,   -1,  327,  268,  329,
   -1,  271,  272,  333,  274,    0,  276,  277,   -1,  279,
   -1,  281,   -1,  283,  284,  285,   -1,  287,  288,  289,
   -1,  291,   -1,  293,   -1,  295,  296,  259,  260,  268,
   -1,    0,   -1,  272,   -1,  274,  268,  276,  277,   -1,
  272,   -1,  274,   -1,  276,  277,   -1,  279,   -1,  281,
   -1,  283,  284,  285,   -1,  287,  288,  289,   -1,  291,
    0,  293,  268,  295,  296,   -1,  272,    0,  274,   -1,
  276,  277,   -1,  279,   -1,  281,   -1,  283,  284,  285,
   -1,  287,  288,  289,   -1,  291,    0,  293,  268,  295,
  296,   -1,  272,    0,  274,   -1,  276,  277,   -1,  279,
   -1,  281,   -1,  283,  284,  285,   -1,  287,  288,  289,
   -1,  291,    0,  293,  268,  295,  296,   -1,  272,   -1,
  274,   -1,  276,  277,   -1,  279,   -1,  281,   -1,  283,
  284,  285,   -1,  287,  288,  289,   -1,  291,    0,  293,
  268,  295,  296,   -1,  272,   -1,  274,   -1,  276,  277,
   -1,  279,   -1,  281,   -1,  283,  284,  285,   -1,  287,
  288,  289,   -1,  291,    0,  293,  268,  295,  296,   -1,
  272,   -1,  274,   -1,  276,  277,   -1,  279,   -1,  281,
   -1,  283,  284,  285,   -1,  287,  288,  289,   -1,  291,
    0,  293,  268,  295,  296,   -1,  272,   -1,  274,   -1,
  276,  277,   -1,  279,   -1,  281,   -1,  283,  284,  285,
   -1,  287,  288,  289,   -1,  291,   -1,  293,  268,  295,
  296,   -1,  272,   -1,  274,   -1,  276,  277,   -1,  279,
   -1,  281,   -1,  283,  284,  285,   -1,   -1,  288,  289,
   -1,  291,   -1,  293,  268,  295,  296,   -1,  272,   -1,
  274,   -1,  276,  277,   -1,   -1,   -1,   -1,   -1,  283,
  284,   -1,   -1,   -1,  288,  289,   -1,  291,   -1,  293,
   -1,  295,  296,  268,   -1,   -1,   -1,  272,   -1,  274,
   -1,  276,  277,   -1,   -1,   -1,   -1,   -1,  283,  284,
   -1,   -1,   -1,  288,  289,   -1,  291,   -1,  293,  268,
  295,  296,   -1,  272,   -1,  274,   -1,  276,  277,   -1,
   -1,   -1,   -1,   -1,  283,  284,   -1,   -1,   -1,  288,
  289,   -1,  291,   -1,  293,   -1,  295,  296,  268,   -1,
   -1,   -1,  272,   -1,  274,  268,  276,  277,   -1,  272,
   -1,  274,   -1,  276,  277,   -1,   -1,   -1,  288,  289,
   -1,  291,   -1,  293,  268,  295,  296,   -1,  272,   -1,
  274,  268,  276,  277,   -1,  272,   -1,  274,   -1,   -1,
  277,   -1,   -1,   -1,  288,  289,   -1,  291,   -1,  293,
  268,  295,  296,   -1,  272,   -1,  274,   -1,  276,  277,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  288,  289,   -1,  291,   -1,  293,  268,  295,  296,   -1,
  272,   -1,  274,   -1,  276,  277,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  288,  289,   -1,  291,
   -1,  293,  268,  295,  296,   -1,  272,   -1,  274,   -1,
  276,  277,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  288,  289,   -1,  291,   -1,  293,  268,  295,
  296,   -1,  272,   -1,  274,   -1,  276,  277,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  288,  289,
  };

#line 412 "Parser.jay"
	}
#line 962 "-"
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
  public const int NUMBER = 331;
  public const int LITERAL = 332;
  public const int NCName = 333;
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
