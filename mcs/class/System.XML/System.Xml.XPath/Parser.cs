// created by jay 0.7 (c) 1998 Axel.Schreiner@informatik.uni-osnabrueck.de

					// line 2 "Parser.jay"
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
		internal object yyparseDebug (Tokenizer tok)
		{
			return yyparse (tok, new yydebug.yyDebugSimple ());
		}

					// line 27 "-"

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
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,"ERROR","EOF","SLASH","SLASH2",
    "DOT","DOT2","COLON","COLON2","COMMA","AT","FUNCTION_NAME",
    "BRACKET_OPEN","BRACKET_CLOSE","PAREN_OPEN","PAREN_CLOSE","AND","OR",
    "DIV","MOD","PLUS","MINUS","ASTERISK","DOLLAR","BAR","EQ","NE","LE",
    "GE","LT","GT","ANCESTOR","ANCESTOR_OR_SELF","ATTRIBUTE","CHILD",
    "DESCENDANT","DESCENDANT_OR_SELF","FOLLOWING","FOLLOWING_SIBLING",
    "NAMESPACE","PARENT","PRECEDING","PRECEDING_SIBLING","SELF","COMMENT",
    "TEXT","PROCESSING_INSTRUCTION","NODE","NUMBER","LITERAL","NCName",
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
					// line 112 "Parser.jay"
  {
		yyVal = new ExprOR ((Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	}
  break;
case 5:
					// line 120 "Parser.jay"
  {
		yyVal = new ExprAND ((Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	}
  break;
case 7:
					// line 128 "Parser.jay"
  {
		yyVal = new ExprEQ ((Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	}
  break;
case 8:
					// line 132 "Parser.jay"
  {
		yyVal = new ExprNE ((Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	}
  break;
case 10:
					// line 140 "Parser.jay"
  {
		yyVal = new ExprLT ((Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	}
  break;
case 11:
					// line 144 "Parser.jay"
  {
		yyVal = new ExprGT ((Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	}
  break;
case 12:
					// line 148 "Parser.jay"
  {
		yyVal = new ExprLE ((Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	}
  break;
case 13:
					// line 152 "Parser.jay"
  {
		yyVal = new ExprGE ((Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	}
  break;
case 15:
					// line 160 "Parser.jay"
  {
		yyVal = new ExprPLUS ((Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	}
  break;
case 16:
					// line 164 "Parser.jay"
  {
		yyVal = new ExprMINUS ((Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	}
  break;
case 18:
					// line 172 "Parser.jay"
  {
		yyVal = new ExprMULT ((Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	}
  break;
case 19:
					// line 176 "Parser.jay"
  {
		yyVal = new ExprDIV ((Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	}
  break;
case 20:
					// line 180 "Parser.jay"
  {
		yyVal = new ExprMOD ((Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	}
  break;
case 22:
					// line 188 "Parser.jay"
  {
		yyVal = new ExprNEG ((Expression) yyVals[0+yyTop]);
	}
  break;
case 24:
					// line 196 "Parser.jay"
  {
		yyVal = new ExprUNION ((NodeSet) yyVals[-2+yyTop], (NodeSet) yyVals[0+yyTop]);
	}
  break;
case 26:
					// line 204 "Parser.jay"
  {
		yyVal = new ExprRoot ();
	}
  break;
case 27:
					// line 208 "Parser.jay"
  {
		yyVal = new ExprSLASH (new ExprRoot (), (NodeSet) yyVals[0+yyTop]);
	}
  break;
case 28:
					// line 212 "Parser.jay"
  {
		ExprStep exprStep = new ExprStep (new NodeTypeTest (Axes.DescendantOrSelf, XPathNodeType.All));
		yyVal = new ExprSLASH (new ExprSLASH (new ExprRoot (), exprStep), (NodeSet) yyVals[0+yyTop]);
	}
  break;
case 30:
					// line 218 "Parser.jay"
  {
		yyVal = new ExprSLASH ((Expression) yyVals[-2+yyTop], (NodeSet) yyVals[0+yyTop]);
	}
  break;
case 31:
					// line 222 "Parser.jay"
  {
		ExprStep exprStep = new ExprStep (new NodeTypeTest (Axes.DescendantOrSelf, XPathNodeType.All));
		yyVal = new ExprSLASH (new ExprSLASH ((Expression) yyVals[-2+yyTop], exprStep), (NodeSet) yyVals[0+yyTop]);
	}
  break;
case 33:
					// line 231 "Parser.jay"
  {
		yyVal = new ExprSLASH ((Expression) yyVals[-2+yyTop], (NodeSet) yyVals[0+yyTop]);
	}
  break;
case 34:
					// line 235 "Parser.jay"
  {
		ExprStep exprStep = new ExprStep (new NodeTypeTest (Axes.DescendantOrSelf, XPathNodeType.All));
		yyVal = new ExprSLASH (new ExprSLASH ((Expression) yyVals[-2+yyTop], exprStep), (NodeSet) yyVals[0+yyTop]);
	}
  break;
case 35:
					// line 243 "Parser.jay"
  {
		yyVal = new ExprStep (new NodeNameTest ((Axes) yyVals[-2+yyTop], (QName) yyVals[-1+yyTop]), (ExprPredicates) yyVals[0+yyTop]);
	}
  break;
case 36:
					// line 247 "Parser.jay"
  {
		yyVal = new ExprStep (new NodeTypeTest ((Axes) yyVals[-2+yyTop]), (ExprPredicates) yyVals[0+yyTop]);
	}
  break;
case 37:
					// line 251 "Parser.jay"
  {
		yyVal = new ExprStep (new NodeTypeTest ((Axes) yyVals[-5+yyTop], (XPathNodeType) yyVals[-4+yyTop], (String) yyVals[-2+yyTop]), (ExprPredicates) yyVals[0+yyTop]);
	}
  break;
case 38:
					// line 255 "Parser.jay"
  {
		yyVal = new ExprStep (new NodeTypeTest (Axes.Self, XPathNodeType.All));
	}
  break;
case 39:
					// line 259 "Parser.jay"
  {
		yyVal = new ExprStep (new NodeTypeTest (Axes.Parent, XPathNodeType.All));
	}
  break;
case 40:
					// line 266 "Parser.jay"
  {
		yyVal = Axes.Child;
	}
  break;
case 41:
					// line 270 "Parser.jay"
  {
		yyVal = Axes.Attribute;
	}
  break;
case 42:
					// line 274 "Parser.jay"
  {
		yyVal = yyVals[-1+yyTop];
	}
  break;
case 43:
					// line 280 "Parser.jay"
  { yyVal = XPathNodeType.Comment; }
  break;
case 44:
					// line 281 "Parser.jay"
  { yyVal = XPathNodeType.Text; }
  break;
case 45:
					// line 282 "Parser.jay"
  { yyVal = XPathNodeType.ProcessingInstruction; }
  break;
case 46:
					// line 283 "Parser.jay"
  { yyVal = XPathNodeType.All; }
  break;
case 48:
					// line 290 "Parser.jay"
  {
		yyVal = new ExprFilter ((Expression) yyVals[-1+yyTop], (Expression) yyVals[0+yyTop]);
	}
  break;
case 49:
					// line 297 "Parser.jay"
  {
		yyVal = new ExprVariable ((QName) yyVals[0+yyTop]);
	}
  break;
case 50:
					// line 301 "Parser.jay"
  {
		yyVal = yyVals[-1+yyTop];
	}
  break;
case 51:
					// line 305 "Parser.jay"
  {
		yyVal = new ExprLiteral ((String) yyVals[0+yyTop]);
	}
  break;
case 52:
					// line 309 "Parser.jay"
  {
		yyVal = new ExprNumber ((double) yyVals[0+yyTop]);
	}
  break;
case 54:
					// line 317 "Parser.jay"
  {
		yyVal = new ExprFunctionCall ((String) yyVals[-3+yyTop], (FunctionArguments) yyVals[-1+yyTop]);
	}
  break;
case 56:
					// line 325 "Parser.jay"
  {
		yyVal = new FunctionArguments ((Expression) yyVals[-1+yyTop], (FunctionArguments) yyVals[0+yyTop]);
	}
  break;
case 58:
					// line 333 "Parser.jay"
  {
		yyVal = new FunctionArguments ((Expression) yyVals[-1+yyTop], (FunctionArguments) yyVals[0+yyTop]);
	}
  break;
case 60:
					// line 342 "Parser.jay"
  {
		yyVal = new ExprPredicates ((Expression) yyVals[-1+yyTop], (ExprPredicates) yyVals[0+yyTop]);
	}
  break;
case 61:
					// line 349 "Parser.jay"
  {
		yyVal = yyVals[-1+yyTop];
	}
  break;
case 62:
					// line 355 "Parser.jay"
  { yyVal = Axes.Ancestor; }
  break;
case 63:
					// line 356 "Parser.jay"
  { yyVal = Axes.AncestorOrSelf; }
  break;
case 64:
					// line 357 "Parser.jay"
  { yyVal = Axes.Attribute; }
  break;
case 65:
					// line 358 "Parser.jay"
  { yyVal = Axes.Child; }
  break;
case 66:
					// line 359 "Parser.jay"
  { yyVal = Axes.Descendant; }
  break;
case 67:
					// line 360 "Parser.jay"
  { yyVal = Axes.DescendantOrSelf; }
  break;
case 68:
					// line 361 "Parser.jay"
  { yyVal = Axes.Following; }
  break;
case 69:
					// line 362 "Parser.jay"
  { yyVal = Axes.FollowingSibling; }
  break;
case 70:
					// line 363 "Parser.jay"
  { yyVal = Axes.Namespace; }
  break;
case 71:
					// line 364 "Parser.jay"
  { yyVal = Axes.Parent; }
  break;
case 72:
					// line 365 "Parser.jay"
  { yyVal = Axes.Preceding; }
  break;
case 73:
					// line 366 "Parser.jay"
  { yyVal = Axes.PrecedingSibling; }
  break;
case 74:
					// line 367 "Parser.jay"
  { yyVal = Axes.Self; }
  break;
case 77:
					// line 377 "Parser.jay"
  {
		yyVal = new NCName ((String) yyVals[0+yyTop]);
	}
  break;
case 78:
					// line 381 "Parser.jay"
  {
		yyVal = new QName ((String) yyVals[-2+yyTop], null);
	}
  break;
case 79:
					// line 385 "Parser.jay"
  {
		yyVal = new QName ((String) yyVals[-2+yyTop], (String) yyVals[0+yyTop]);
	}
  break;
					// line 662 "-"
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
  protected static  short [] yySindex = {         -188,
  -85,  -85,    0,    0,    0, -258, -188, -188, -297,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0, -215, -257, -253, -265, -245,
 -261,    0, -221,    0, -218, -252,    0, -256, -203,    0,
    0, -218, -218, -188, -209,    0, -200,    0, -188, -188,
 -188, -188, -188, -188, -188, -188, -188, -188, -188, -188,
 -188, -130,  -85,  -85,  -85,  -85, -188,    0, -204,    0,
    0,    0,    0, -204, -195,    0, -199, -194,    0, -268,
 -257, -253, -265, -265, -245, -245, -245, -245, -261, -261,
    0,    0,    0,    0,    0,    0, -218, -218, -202,    0,
 -204,    0, -229, -188,    0,    0,    0,    0,    0,    0,
    0, -191, -199, -204,    0,    0,
  };
  protected static  short [] yyRindex = {         -182,
    1, -182,    0,    0,    0,    0, -182, -182,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  253,   65,   23,  435,  319,
  244,    0,  222,    0,   90,  112,    0,    0,    0,    0,
    0,  134,  156, -267,    0,    0,   40,    0, -182, -182,
 -182, -182, -182, -182, -182, -182, -182, -182, -182, -182,
 -182, -182, -182, -182, -182, -182, -182,    0,   68,    0,
    0,    0,    0,   68,    0,    0, -190,    0,    0,    0,
  510,  501,  457,  479,  347,  369,  391,  413,  267,  289,
    0,    0,    0,    0,    0,    0,  178,  200,    0,    0,
   68,    0, -187, -182,    0,    0,    0,    0,    0,    0,
    0,    0, -190,   68,    0,    0,
  };
  protected static  short [] yyGindex = {           -7,
    0,   34,   35,   -3,  -29,   -1,   -6,    0,   24,    4,
    0,  -12,    0,   49,  -71,    0,    0,    0,    0,   52,
    0,    0,  -21,
  };
  protected static  short [] yyTable = {            45,
   26,   46,  102,   55,   42,   43,   65,   66,   47,  107,
   40,   44,   59,   60,   50,   67,   61,   53,   54,   55,
   56,   69,    4,   85,   86,   87,   88,   51,   52,  110,
   57,   58,   40,   40,   40,   40,   77,  108,   40,   77,
   63,   64,  116,   70,   71,   72,   73,   83,   84,   47,
   95,   96,   91,   92,   93,   89,   90,   49,   62,   99,
   76,   79,   80,   67,    2,  104,  109,   59,   97,   98,
    1,    2,    3,    4,  103,  111,  106,    5,    6,  114,
   57,    7,   81,   75,   82,   94,   74,   68,    8,   25,
    9,  115,    0,    0,    0,   40,  113,    0,   10,   11,
   12,   13,   14,   15,   16,   17,   18,   19,   20,   21,
   22,   29,    0,    0,    0,   23,   24,   40,   40,   40,
   40,    0,    0,   40,    0,    0,    0,    0,    1,    2,
    3,    4,    0,   27,    0,    5,    6,    0,    0,    7,
    0,    0,    0,    0,    0,    0,    0,    0,    9,    0,
    0,    0,    0,    0,    0,   28,   10,   11,   12,   13,
   14,   15,   16,   17,   18,   19,   20,   21,   22,    0,
    0,    0,    0,   23,   24,    3,    4,   30,    0,    0,
    5,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,   31,
    0,   10,   11,   12,   13,   14,   15,   16,   17,   18,
   19,   20,   21,   22,    0,    0,    0,    0,    0,    0,
    0,   21,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,   14,    0,    0,    0,    0,    0,    0,
    0,    0,    1,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,   26,   15,    0,    0,   26,
    0,   26,   26,   26,   26,   26,   26,   26,   26,    0,
   26,   26,   26,   26,   26,   26,   26,    4,   16,    0,
    0,    4,    0,    4,    4,    4,    0,    0,   77,   77,
   40,   40,   40,   40,   77,    0,   40,   77,   77,    0,
   77,   77,   77,   77,   77,   77,   77,   77,    9,   77,
   77,   77,   77,   77,   77,   77,   59,   59,    0,    2,
    0,    0,   59,    2,    0,    2,   59,    2,   59,   59,
   59,   59,   59,   59,   59,   59,   12,   59,   59,   59,
   59,   59,   59,   59,   25,    0,    0,    0,   25,    0,
   25,   25,   25,   25,   25,   25,   25,   25,   13,   25,
   25,   25,   25,   25,   25,   25,   29,    0,    0,    0,
   29,    0,   29,   29,   29,   29,   29,   29,   29,   29,
   10,   29,   29,   29,   29,   29,   29,   29,   27,    0,
    0,    0,   27,    0,   27,   27,   27,   27,   27,   27,
   27,   27,   11,   27,   27,   27,   27,   27,   27,   27,
   28,    0,    0,    0,   28,    0,   28,   28,   28,   28,
   28,   28,   28,   28,    6,   28,   28,   28,   28,   28,
   28,   28,   30,    0,    0,    0,   30,    0,   30,   30,
   30,   30,   30,   30,   30,   30,    7,   30,   30,   30,
   30,   30,   30,   30,   31,    0,    0,    0,   31,    0,
   31,   31,   31,   31,   31,   31,   31,   31,    8,   31,
   31,   31,   31,   31,   31,   31,   21,    0,    0,    0,
   21,    0,   21,   21,   21,   21,   21,   21,   21,   21,
    5,    0,   21,   21,   21,   21,   21,   21,   14,    3,
    0,    0,   14,    0,   14,   14,   14,    1,    0,   14,
   14,    1,    0,    1,   14,   14,   14,   14,   14,   14,
    0,   15,    0,    0,    0,   15,    0,   15,   15,   15,
    0,    0,   15,   15,    0,    0,    0,   15,   15,   15,
   15,   15,   15,   16,    0,    0,    0,   16,    0,   16,
   16,   16,    0,    0,   16,   16,    0,    0,    0,   16,
   16,   16,   16,   16,   16,    0,    0,    0,    0,    0,
    0,    0,    0,    9,    0,    0,    0,    9,    0,    9,
    9,    9,    0,    0,    0,    0,    0,    0,    0,    9,
    9,    9,    9,    9,    9,    0,    0,    0,    0,    0,
    0,   12,    0,    0,    0,   12,    0,   12,   12,   12,
    0,    0,    0,    0,    0,    0,    0,   12,   12,   12,
   12,   12,   12,   13,    0,    0,    0,   13,    0,   13,
   13,   13,    0,    0,    0,    0,    0,    0,    0,   13,
   13,   13,   13,   13,   13,   10,    0,    0,    0,   10,
    0,   10,   10,   10,    0,    0,    0,    0,    0,    0,
    0,   10,   10,   10,   10,   10,   10,   11,    0,    0,
    0,   11,    0,   11,   11,   11,    0,    0,    0,    0,
    0,    0,    0,   11,   11,   11,   11,   11,   11,    6,
    0,    0,    0,    6,    0,    6,    6,    6,    0,    0,
    0,    0,    0,    0,    0,    6,    6,    0,    0,    0,
    0,    7,    0,    0,    0,    7,    0,    7,    7,    7,
    0,    0,    0,    0,    0,    0,    0,    7,    7,    0,
    0,    0,    0,    8,    0,    0,    0,    8,    0,    8,
    8,    8,    0,    0,    0,    0,    0,    0,    0,    8,
    8,    0,    0,    0,    0,    5,    0,    0,    0,    5,
    0,    5,    5,    5,    3,    0,    0,    0,    3,    0,
    3,    0,    3,
  };
  protected static  short [] yyCheck = {             7,
    0,    8,   74,  271,    1,    2,  259,  260,  306,  278,
  278,  270,  274,  275,  272,  268,  278,  283,  284,  285,
  286,  278,    0,   53,   54,   55,   56,  281,  282,  101,
  276,  277,  300,  301,  302,  303,   44,  306,  306,    0,
  259,  260,  114,  300,  301,  302,  303,   51,   52,  306,
   63,   64,   59,   60,   61,   57,   58,  273,  280,   67,
  264,  271,  263,  268,    0,  265,  269,    0,   65,   66,
  259,  260,  261,  262,  270,  305,  271,  266,  267,  271,
  271,  270,   49,  271,   50,   62,   38,   36,  277,    0,
  279,  113,   -1,   -1,   -1,  278,  104,   -1,  287,  288,
  289,  290,  291,  292,  293,  294,  295,  296,  297,  298,
  299,    0,   -1,   -1,   -1,  304,  305,  300,  301,  302,
  303,   -1,   -1,  306,   -1,   -1,   -1,   -1,  259,  260,
  261,  262,   -1,    0,   -1,  266,  267,   -1,   -1,  270,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  279,   -1,
   -1,   -1,   -1,   -1,   -1,    0,  287,  288,  289,  290,
  291,  292,  293,  294,  295,  296,  297,  298,  299,   -1,
   -1,   -1,   -1,  304,  305,  261,  262,    0,   -1,   -1,
  266,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,    0,
   -1,  287,  288,  289,  290,  291,  292,  293,  294,  295,
  296,  297,  298,  299,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,    0,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,    0,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,    0,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  265,    0,   -1,   -1,  269,
   -1,  271,  272,  273,  274,  275,  276,  277,  278,   -1,
  280,  281,  282,  283,  284,  285,  286,  265,    0,   -1,
   -1,  269,   -1,  271,  272,  273,   -1,   -1,  259,  260,
  300,  301,  302,  303,  265,   -1,  306,  268,  269,   -1,
  271,  272,  273,  274,  275,  276,  277,  278,    0,  280,
  281,  282,  283,  284,  285,  286,  259,  260,   -1,  265,
   -1,   -1,  265,  269,   -1,  271,  269,  273,  271,  272,
  273,  274,  275,  276,  277,  278,    0,  280,  281,  282,
  283,  284,  285,  286,  265,   -1,   -1,   -1,  269,   -1,
  271,  272,  273,  274,  275,  276,  277,  278,    0,  280,
  281,  282,  283,  284,  285,  286,  265,   -1,   -1,   -1,
  269,   -1,  271,  272,  273,  274,  275,  276,  277,  278,
    0,  280,  281,  282,  283,  284,  285,  286,  265,   -1,
   -1,   -1,  269,   -1,  271,  272,  273,  274,  275,  276,
  277,  278,    0,  280,  281,  282,  283,  284,  285,  286,
  265,   -1,   -1,   -1,  269,   -1,  271,  272,  273,  274,
  275,  276,  277,  278,    0,  280,  281,  282,  283,  284,
  285,  286,  265,   -1,   -1,   -1,  269,   -1,  271,  272,
  273,  274,  275,  276,  277,  278,    0,  280,  281,  282,
  283,  284,  285,  286,  265,   -1,   -1,   -1,  269,   -1,
  271,  272,  273,  274,  275,  276,  277,  278,    0,  280,
  281,  282,  283,  284,  285,  286,  265,   -1,   -1,   -1,
  269,   -1,  271,  272,  273,  274,  275,  276,  277,  278,
    0,   -1,  281,  282,  283,  284,  285,  286,  265,    0,
   -1,   -1,  269,   -1,  271,  272,  273,  265,   -1,  276,
  277,  269,   -1,  271,  281,  282,  283,  284,  285,  286,
   -1,  265,   -1,   -1,   -1,  269,   -1,  271,  272,  273,
   -1,   -1,  276,  277,   -1,   -1,   -1,  281,  282,  283,
  284,  285,  286,  265,   -1,   -1,   -1,  269,   -1,  271,
  272,  273,   -1,   -1,  276,  277,   -1,   -1,   -1,  281,
  282,  283,  284,  285,  286,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  265,   -1,   -1,   -1,  269,   -1,  271,
  272,  273,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  281,
  282,  283,  284,  285,  286,   -1,   -1,   -1,   -1,   -1,
   -1,  265,   -1,   -1,   -1,  269,   -1,  271,  272,  273,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  281,  282,  283,
  284,  285,  286,  265,   -1,   -1,   -1,  269,   -1,  271,
  272,  273,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  281,
  282,  283,  284,  285,  286,  265,   -1,   -1,   -1,  269,
   -1,  271,  272,  273,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  281,  282,  283,  284,  285,  286,  265,   -1,   -1,
   -1,  269,   -1,  271,  272,  273,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  281,  282,  283,  284,  285,  286,  265,
   -1,   -1,   -1,  269,   -1,  271,  272,  273,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  281,  282,   -1,   -1,   -1,
   -1,  265,   -1,   -1,   -1,  269,   -1,  271,  272,  273,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  281,  282,   -1,
   -1,   -1,   -1,  265,   -1,   -1,   -1,  269,   -1,  271,
  272,  273,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  281,
  282,   -1,   -1,   -1,   -1,  265,   -1,   -1,   -1,  269,
   -1,  271,  272,  273,  265,   -1,   -1,   -1,  269,   -1,
  271,   -1,  273,
  };

					// line 391 "Parser.jay"
	}
					// line 929 "-"
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
  public const int DOT = 261;
  public const int DOT2 = 262;
  public const int COLON = 263;
  public const int COLON2 = 264;
  public const int COMMA = 265;
  public const int AT = 266;
  public const int FUNCTION_NAME = 267;
  public const int BRACKET_OPEN = 268;
  public const int BRACKET_CLOSE = 269;
  public const int PAREN_OPEN = 270;
  public const int PAREN_CLOSE = 271;
  public const int AND = 272;
  public const int OR = 273;
  public const int DIV = 274;
  public const int MOD = 275;
  public const int PLUS = 276;
  public const int MINUS = 277;
  public const int ASTERISK = 278;
  public const int DOLLAR = 279;
  public const int BAR = 280;
  public const int EQ = 281;
  public const int NE = 282;
  public const int LE = 283;
  public const int GE = 284;
  public const int LT = 285;
  public const int GT = 286;
  public const int ANCESTOR = 287;
  public const int ANCESTOR_OR_SELF = 288;
  public const int ATTRIBUTE = 289;
  public const int CHILD = 290;
  public const int DESCENDANT = 291;
  public const int DESCENDANT_OR_SELF = 292;
  public const int FOLLOWING = 293;
  public const int FOLLOWING_SIBLING = 294;
  public const int NAMESPACE = 295;
  public const int PARENT = 296;
  public const int PRECEDING = 297;
  public const int PRECEDING_SIBLING = 298;
  public const int SELF = 299;
  public const int COMMENT = 300;
  public const int TEXT = 301;
  public const int PROCESSING_INSTRUCTION = 302;
  public const int NODE = 303;
  public const int NUMBER = 304;
  public const int LITERAL = 305;
  public const int NCName = 306;
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
