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
using System.Xml;
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
			//yyDebug = new yydebug.yyDebugSimple ();
			try
			{
				Expression expr = (Expression) yyparse (tok, yyDebug);
				//Console.WriteLine (expr.ToString ());
				return expr;
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

					// line 51

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
  };
  protected static  string [] yyName = {    
    "end-of-file",null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,"'$'",null,null,
    null,"'('","')'","'*'","'+'","','","'-'","'.'","'/'",null,null,null,
    null,null,null,null,null,null,null,null,null,"'<'","'='","'>'",null,
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
    "ERROR","EOF","SLASH","SLASH2","\"//\"","DOT","DOT2","\"..\"",
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
    "LITERAL","QName",
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
					// line 137 "Parser.jay"
  {
		yyVal = new ExprOR ((Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	}
  break;
case 5:
					// line 145 "Parser.jay"
  {
		yyVal = new ExprAND ((Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	}
  break;
case 7:
					// line 153 "Parser.jay"
  {
		yyVal = new ExprEQ ((Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	}
  break;
case 8:
					// line 157 "Parser.jay"
  {
		yyVal = new ExprNE ((Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	}
  break;
case 10:
					// line 165 "Parser.jay"
  {
		yyVal = new ExprLT ((Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	}
  break;
case 11:
					// line 169 "Parser.jay"
  {
		yyVal = new ExprGT ((Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	}
  break;
case 12:
					// line 173 "Parser.jay"
  {
		yyVal = new ExprLE ((Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	}
  break;
case 13:
					// line 177 "Parser.jay"
  {
		yyVal = new ExprGE ((Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	}
  break;
case 15:
					// line 185 "Parser.jay"
  {
		yyVal = new ExprPLUS ((Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	}
  break;
case 16:
					// line 189 "Parser.jay"
  {
		yyVal = new ExprMINUS ((Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	}
  break;
case 18:
					// line 197 "Parser.jay"
  {
		yyVal = new ExprMULT ((Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	}
  break;
case 19:
					// line 201 "Parser.jay"
  {
		yyVal = new ExprDIV ((Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	}
  break;
case 20:
					// line 205 "Parser.jay"
  {
		yyVal = new ExprMOD ((Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	}
  break;
case 22:
					// line 213 "Parser.jay"
  {
		yyVal = new ExprNEG ((Expression) yyVals[0+yyTop]);
	}
  break;
case 24:
					// line 221 "Parser.jay"
  {
		yyVal = new ExprUNION ((Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	}
  break;
case 26:
					// line 229 "Parser.jay"
  {
		yyVal = new ExprRoot ();
	}
  break;
case 27:
					// line 233 "Parser.jay"
  {
		yyVal = new ExprSLASH (new ExprRoot (), (NodeSet) yyVals[0+yyTop]);
	}
  break;
case 28:
					// line 237 "Parser.jay"
  {
		ExprStep exprStep = new ExprStep (new NodeTypeTest (Axes.DescendantOrSelf, XPathNodeType.All));
		yyVal = new ExprSLASH (new ExprSLASH (new ExprRoot (), exprStep), (NodeSet) yyVals[0+yyTop]);
	}
  break;
case 30:
					// line 243 "Parser.jay"
  {
		yyVal = new ExprSLASH ((Expression) yyVals[-2+yyTop], (NodeSet) yyVals[0+yyTop]);
	}
  break;
case 31:
					// line 247 "Parser.jay"
  {
		ExprStep exprStep = new ExprStep (new NodeTypeTest (Axes.DescendantOrSelf, XPathNodeType.All));
		yyVal = new ExprSLASH (new ExprSLASH ((Expression) yyVals[-2+yyTop], exprStep), (NodeSet) yyVals[0+yyTop]);
	}
  break;
case 33:
					// line 256 "Parser.jay"
  {
		yyVal = new ExprSLASH ((Expression) yyVals[-2+yyTop], (NodeSet) yyVals[0+yyTop]);
	}
  break;
case 34:
					// line 260 "Parser.jay"
  {
		ExprStep exprStep = new ExprStep (new NodeTypeTest (Axes.DescendantOrSelf, XPathNodeType.All));
		yyVal = new ExprSLASH (new ExprSLASH ((Expression) yyVals[-2+yyTop], exprStep), (NodeSet) yyVals[0+yyTop]);
	}
  break;
case 35:
					// line 268 "Parser.jay"
  {
		yyVal = new ExprStep (new NodeNameTest ((Axes) yyVals[-2+yyTop], (XmlQualifiedName) yyVals[-1+yyTop]), (ExprPredicates) yyVals[0+yyTop]);
	}
  break;
case 36:
					// line 272 "Parser.jay"
  {
		yyVal = new ExprStep (new NodeTypeTest ((Axes) yyVals[-2+yyTop]), (ExprPredicates) yyVals[0+yyTop]);
	}
  break;
case 37:
					// line 276 "Parser.jay"
  {
		yyVal = new ExprStep (new NodeTypeTest ((Axes) yyVals[-5+yyTop], (XPathNodeType) yyVals[-4+yyTop], (String) yyVals[-2+yyTop]), (ExprPredicates) yyVals[0+yyTop]);
	}
  break;
case 38:
					// line 280 "Parser.jay"
  {
		yyVal = new ExprStep (new NodeTypeTest (Axes.Self, XPathNodeType.All));
	}
  break;
case 39:
					// line 284 "Parser.jay"
  {
		yyVal = new ExprStep (new NodeTypeTest (Axes.Parent, XPathNodeType.All));
	}
  break;
case 40:
					// line 291 "Parser.jay"
  {
		yyVal = Axes.Child;
	}
  break;
case 41:
					// line 295 "Parser.jay"
  {
		yyVal = Axes.Attribute;
	}
  break;
case 42:
					// line 299 "Parser.jay"
  {
		yyVal = yyVals[-1+yyTop];
	}
  break;
case 43:
					// line 305 "Parser.jay"
  { yyVal = XPathNodeType.Comment; }
  break;
case 44:
					// line 306 "Parser.jay"
  { yyVal = XPathNodeType.Text; }
  break;
case 45:
					// line 307 "Parser.jay"
  { yyVal = XPathNodeType.ProcessingInstruction; }
  break;
case 46:
					// line 308 "Parser.jay"
  { yyVal = XPathNodeType.All; }
  break;
case 48:
					// line 315 "Parser.jay"
  {
		yyVal = new ExprFilter ((Expression) yyVals[-1+yyTop], (Expression) yyVals[0+yyTop]);
	}
  break;
case 49:
					// line 322 "Parser.jay"
  {
		yyVal = new ExprVariable ((XmlQualifiedName) yyVals[0+yyTop]);
	}
  break;
case 50:
					// line 326 "Parser.jay"
  {
		yyVal = yyVals[-1+yyTop];
	}
  break;
case 51:
					// line 330 "Parser.jay"
  {
		yyVal = new ExprLiteral ((String) yyVals[0+yyTop]);
	}
  break;
case 52:
					// line 334 "Parser.jay"
  {
		yyVal = new ExprNumber ((double) yyVals[0+yyTop]);
	}
  break;
case 54:
					// line 342 "Parser.jay"
  {
		yyVal = new ExprFunctionCall ((XmlQualifiedName) yyVals[-3+yyTop], (FunctionArguments) yyVals[-1+yyTop]);
	}
  break;
case 56:
					// line 350 "Parser.jay"
  {
		yyVal = new FunctionArguments ((Expression) yyVals[-1+yyTop], (FunctionArguments) yyVals[0+yyTop]);
	}
  break;
case 58:
					// line 358 "Parser.jay"
  {
		yyVal = new FunctionArguments ((Expression) yyVals[-1+yyTop], (FunctionArguments) yyVals[0+yyTop]);
	}
  break;
case 60:
					// line 367 "Parser.jay"
  {
		yyVal = new ExprPredicates ((Expression) yyVals[-1+yyTop], (ExprPredicates) yyVals[0+yyTop]);
	}
  break;
case 61:
					// line 374 "Parser.jay"
  {
		yyVal = yyVals[-1+yyTop];
	}
  break;
case 62:
					// line 380 "Parser.jay"
  { yyVal = Axes.Ancestor; }
  break;
case 63:
					// line 381 "Parser.jay"
  { yyVal = Axes.AncestorOrSelf; }
  break;
case 64:
					// line 382 "Parser.jay"
  { yyVal = Axes.Attribute; }
  break;
case 65:
					// line 383 "Parser.jay"
  { yyVal = Axes.Child; }
  break;
case 66:
					// line 384 "Parser.jay"
  { yyVal = Axes.Descendant; }
  break;
case 67:
					// line 385 "Parser.jay"
  { yyVal = Axes.DescendantOrSelf; }
  break;
case 68:
					// line 386 "Parser.jay"
  { yyVal = Axes.Following; }
  break;
case 69:
					// line 387 "Parser.jay"
  { yyVal = Axes.FollowingSibling; }
  break;
case 70:
					// line 388 "Parser.jay"
  { yyVal = Axes.Namespace; }
  break;
case 71:
					// line 389 "Parser.jay"
  { yyVal = Axes.Parent; }
  break;
case 72:
					// line 390 "Parser.jay"
  { yyVal = Axes.Preceding; }
  break;
case 73:
					// line 391 "Parser.jay"
  { yyVal = Axes.PrecedingSibling; }
  break;
case 74:
					// line 392 "Parser.jay"
  { yyVal = Axes.Self; }
  break;
					// line 673
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
   13,   13,   15,   15,   15,   15,   11,   11,   18,   18,
   18,   18,   18,   20,   21,   21,   22,   22,   14,   14,
   19,   17,   17,   17,   17,   17,   17,   17,   17,   17,
   17,   17,   17,   17,   16,   16,
  };
   static  short [] yyLen = {           2,
    1,    1,    3,    1,    3,    1,    3,    3,    1,    3,
    3,    3,    3,    1,    3,    3,    1,    3,    3,    3,
    1,    2,    1,    3,    1,    1,    2,    2,    1,    3,
    3,    1,    3,    3,    3,    3,    6,    1,    1,    0,
    1,    2,    1,    1,    1,    1,    1,    2,    2,    3,
    1,    1,    1,    4,    0,    2,    0,    3,    0,    2,
    3,    1,    1,    1,    1,    1,    1,    1,    1,    1,
    1,    1,    1,    1,    0,    1,
  };
   static  short [] yyDefRed = {            0,
    0,    0,   38,   39,   41,    0,    0,    0,    0,   62,
   63,   64,   65,   66,   67,   68,   69,   70,   71,   72,
   73,   74,   52,   51,    0,    0,    0,    0,    0,    0,
    0,   17,    0,   23,    0,    0,   32,    0,    0,   47,
   53,    0,    0,    0,    0,   22,   49,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,   48,    0,   43,   44,
   45,   46,    0,    0,   42,    0,    0,   50,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,   19,   20,
   18,   24,   33,   34,    0,    0,    0,   36,    0,   35,
    0,    0,   56,   54,   61,   60,   76,    0,    0,    0,
   58,   37,
  };
  protected static  short [] yyDgoto  = {            25,
   26,   27,   28,   29,   30,   31,   32,   33,   34,   35,
   36,   37,   38,   98,   74,  108,   39,   40,   99,   41,
   77,  103,
  };
  protected static  short [] yySindex = {         -254,
 -124, -124,    0,    0,    0, -272, -254, -254, -330,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0, -269, -246, -255, -204, -248,
 -258,    0, -270,    0, -165, -249,    0, -200, -229,    0,
    0, -165, -165, -254, -235,    0,    0, -254, -254, -254,
 -254, -254, -254, -254, -254, -254, -254, -254, -254, -254,
 -189, -124, -124, -124, -124, -254,    0, -230,    0,    0,
    0,    0, -230, -227,    0, -224, -226,    0, -246, -255,
 -204, -204, -248, -248, -248, -248, -258, -258,    0,    0,
    0,    0,    0,    0, -165, -165, -222,    0, -230,    0,
 -281, -254,    0,    0,    0,    0,    0, -220, -224, -230,
    0,    0,
  };
  protected static  short [] yyRindex = {         -176,
    1, -176,    0,    0,    0,    0, -176, -176,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,   19,   93,   37,   27,  357,
  276,    0,  250,    0,   85,  114,    0,    0,    0,    0,
    0,  140,  169, -192,    0,    0,    0, -176, -176, -176,
 -176, -176, -176, -176, -176, -176, -176, -176, -176, -176,
 -176, -176, -176, -176, -176, -176,    0,   59,    0,    0,
    0,    0,   59,    0,    0, -218,    0,    0,  336,  484,
  458,  476,  383,  393,  419,  429,  302,  328,    0,    0,
    0,    0,    0,    0,  195,  224,    0,    0,   59,    0,
 -216, -176,    0,    0,    0,    0,    0,    0, -218,   59,
    0,    0,
  };
  protected static  short [] yyGindex = {           -5,
    0,   15,   16,   48,  -29,   44,    9,    0,   21,   11,
    0,   40,    0,  -69,    0,    0,    0,    0,   51,    0,
    0,  -20,
  };
  protected static  short [] yyTable = {            44,
   26,   45,   47,  100,    1,    2,   48,    3,    4,   64,
   65,   42,   43,    5,    6,   61,   46,    7,    1,   58,
   66,   59,   83,   84,   85,   86,    6,   49,    8,  106,
    9,   50,   51,   56,   57,   75,    4,   78,   76,   66,
  112,   10,  102,   11,  101,   12,  104,   13,  105,   14,
  107,   15,  110,   16,   57,   17,   75,   18,   59,   19,
   97,   20,   79,   21,   80,   22,   89,   90,   91,    1,
    2,   60,    3,    4,   95,   96,   23,   24,    5,    6,
   55,   92,    7,   68,   25,   52,   67,   53,  111,   54,
   55,   40,    2,   62,   63,    9,  109,   81,   82,   87,
   88,   93,   94,    0,    0,    0,   10,   40,   11,    0,
   12,    0,   13,   29,   14,    0,   15,    0,   16,    0,
   17,   69,   18,   70,   19,   71,   20,   72,   21,   40,
   22,   40,   73,   40,    0,   40,    0,    3,    4,   27,
   40,   23,   24,    5,    0,   40,    0,   40,    0,   40,
    0,   40,    0,    0,    0,    0,   40,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,   28,    0,
    0,   10,    0,   11,    0,   12,    0,   13,    0,   14,
    0,   15,    0,   16,    0,   17,    0,   18,    0,   19,
    0,   20,    0,   21,   30,   22,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,   31,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,   21,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,   26,    0,    0,
    0,   26,    0,   26,   26,   14,   26,    0,   26,    0,
   26,    0,   26,   26,   40,    1,   26,   26,   26,    1,
   26,    1,   26,    6,   26,   26,    0,    6,    0,    6,
    6,   15,    6,    4,    0,    0,    0,    4,    0,    4,
    4,    0,    4,    6,    6,    0,    0,   59,   59,    0,
    0,    0,   40,    0,   40,   59,   40,   16,   40,   59,
   26,   59,   59,   40,   59,    3,   59,    0,   59,    0,
   59,   59,    0,    0,   59,   59,   59,    0,   59,    0,
   59,   25,   59,   59,    0,   25,    9,   25,   25,    2,
   25,    0,   25,    2,   25,    2,   25,   25,    2,    0,
   25,   25,   25,    0,   25,    0,   25,    0,   25,   25,
   29,    0,   12,    0,   29,    0,   29,   29,   59,   29,
    0,   29,   13,   29,    0,   29,   29,    0,    0,   29,
   29,   29,    0,   29,    0,   29,   27,   29,   29,    0,
   27,    0,   27,   27,   25,   27,    0,   27,   10,   27,
    0,   27,   27,    0,    0,   27,   27,   27,   11,   27,
    0,   27,    0,   27,   27,   28,    0,    0,    0,   28,
    0,   28,   28,   29,   28,    0,   28,    0,   28,    0,
   28,   28,    0,    0,   28,   28,   28,    7,   28,    0,
   28,   30,   28,   28,    0,   30,    0,   30,   30,   27,
   30,    0,   30,    0,   30,    8,   30,   30,    0,    0,
   30,   30,   30,    5,   30,    0,   30,    0,   30,   30,
   31,    0,    0,    0,   31,    0,   31,   31,   28,   31,
    0,   31,    0,   31,    0,   31,   31,    0,    0,   31,
   31,   31,    0,   31,    0,   31,   21,   31,   31,    0,
   21,    0,   21,   21,   30,   21,    0,   21,    0,   21,
    0,   21,   21,    0,    0,    0,   21,   21,    0,   21,
    0,   21,   14,   21,   21,    0,   14,    0,   14,   14,
    0,   14,    0,   31,    0,    0,    0,   14,   14,    0,
    0,    0,   14,   14,    0,   14,    0,   14,   15,   14,
   14,    0,   15,    0,   15,   15,    0,   15,    0,   21,
    0,    0,    0,   15,   15,    0,    0,    0,   15,   15,
    0,   15,    0,   15,   16,   15,   15,    0,   16,    0,
   16,   16,    3,   16,    0,    0,    3,    0,    3,   16,
   16,    3,    0,    0,   16,   16,    0,   16,    0,   16,
    0,   16,   16,    9,    0,    0,    0,    9,    0,    9,
    9,    0,    9,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    9,    9,    0,    9,    0,    9,   12,
    9,    9,    0,   12,    0,   12,   12,    0,   12,   13,
    0,    0,    0,   13,    0,   13,   13,    0,   13,   12,
   12,    0,   12,    0,   12,    0,   12,   12,    0,   13,
   13,    0,   13,    0,   13,   10,   13,   13,    0,   10,
    0,   10,   10,    0,   10,   11,    0,    0,    0,   11,
    0,   11,   11,    0,   11,   10,   10,    0,   10,    0,
   10,    0,   10,   10,    0,   11,   11,    0,   11,    0,
   11,    0,   11,   11,    7,    0,    0,    0,    7,    0,
    7,    7,    0,    7,    0,    0,    0,    0,    0,    0,
    0,    0,    8,    0,    7,    7,    8,    0,    8,    8,
    5,    8,    0,    0,    5,    0,    5,    5,    0,    5,
    0,    0,    8,    8,
  };
  protected static  short [] yyCheck = {           272,
    0,    7,  333,   73,  259,  260,  276,  262,  263,  259,
  260,    1,    2,  268,  269,  286,    8,  272,    0,  278,
  270,  280,   52,   53,   54,   55,    0,  274,  283,   99,
  285,  287,  288,  282,  283,  265,    0,  273,   44,  270,
  110,  296,  267,  298,  272,  300,  273,  302,  271,  304,
  332,  306,  273,  308,  273,  310,  273,  312,    0,  314,
   66,  316,   48,  318,   49,  320,   58,   59,   60,  259,
  260,  330,  262,  263,   64,   65,  331,  332,  268,  269,
  273,   61,  272,  284,    0,  290,   36,  292,  109,  294,
  295,  284,    0,  259,  260,  285,  102,   50,   51,   56,
   57,   62,   63,   -1,   -1,   -1,  296,  284,  298,   -1,
  300,   -1,  302,    0,  304,   -1,  306,   -1,  308,   -1,
  310,  322,  312,  324,  314,  326,  316,  328,  318,  322,
  320,  324,  333,  326,   -1,  328,   -1,  262,  263,    0,
  333,  331,  332,  268,   -1,  322,   -1,  324,   -1,  326,
   -1,  328,   -1,   -1,   -1,   -1,  333,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,    0,   -1,
   -1,  296,   -1,  298,   -1,  300,   -1,  302,   -1,  304,
   -1,  306,   -1,  308,   -1,  310,   -1,  312,   -1,  314,
   -1,  316,   -1,  318,    0,  320,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,    0,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,    0,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  267,   -1,   -1,
   -1,  271,   -1,  273,  274,    0,  276,   -1,  278,   -1,
  280,   -1,  282,  283,  284,  267,  286,  287,  288,  271,
  290,  273,  292,  267,  294,  295,   -1,  271,   -1,  273,
  274,    0,  276,  267,   -1,   -1,   -1,  271,   -1,  273,
  274,   -1,  276,  287,  288,   -1,   -1,  259,  260,   -1,
   -1,   -1,  322,   -1,  324,  267,  326,    0,  328,  271,
  330,  273,  274,  333,  276,    0,  278,   -1,  280,   -1,
  282,  283,   -1,   -1,  286,  287,  288,   -1,  290,   -1,
  292,  267,  294,  295,   -1,  271,    0,  273,  274,  267,
  276,   -1,  278,  271,  280,  273,  282,  283,  276,   -1,
  286,  287,  288,   -1,  290,   -1,  292,   -1,  294,  295,
  267,   -1,    0,   -1,  271,   -1,  273,  274,  330,  276,
   -1,  278,    0,  280,   -1,  282,  283,   -1,   -1,  286,
  287,  288,   -1,  290,   -1,  292,  267,  294,  295,   -1,
  271,   -1,  273,  274,  330,  276,   -1,  278,    0,  280,
   -1,  282,  283,   -1,   -1,  286,  287,  288,    0,  290,
   -1,  292,   -1,  294,  295,  267,   -1,   -1,   -1,  271,
   -1,  273,  274,  330,  276,   -1,  278,   -1,  280,   -1,
  282,  283,   -1,   -1,  286,  287,  288,    0,  290,   -1,
  292,  267,  294,  295,   -1,  271,   -1,  273,  274,  330,
  276,   -1,  278,   -1,  280,    0,  282,  283,   -1,   -1,
  286,  287,  288,    0,  290,   -1,  292,   -1,  294,  295,
  267,   -1,   -1,   -1,  271,   -1,  273,  274,  330,  276,
   -1,  278,   -1,  280,   -1,  282,  283,   -1,   -1,  286,
  287,  288,   -1,  290,   -1,  292,  267,  294,  295,   -1,
  271,   -1,  273,  274,  330,  276,   -1,  278,   -1,  280,
   -1,  282,  283,   -1,   -1,   -1,  287,  288,   -1,  290,
   -1,  292,  267,  294,  295,   -1,  271,   -1,  273,  274,
   -1,  276,   -1,  330,   -1,   -1,   -1,  282,  283,   -1,
   -1,   -1,  287,  288,   -1,  290,   -1,  292,  267,  294,
  295,   -1,  271,   -1,  273,  274,   -1,  276,   -1,  330,
   -1,   -1,   -1,  282,  283,   -1,   -1,   -1,  287,  288,
   -1,  290,   -1,  292,  267,  294,  295,   -1,  271,   -1,
  273,  274,  267,  276,   -1,   -1,  271,   -1,  273,  282,
  283,  276,   -1,   -1,  287,  288,   -1,  290,   -1,  292,
   -1,  294,  295,  267,   -1,   -1,   -1,  271,   -1,  273,
  274,   -1,  276,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  287,  288,   -1,  290,   -1,  292,  267,
  294,  295,   -1,  271,   -1,  273,  274,   -1,  276,  267,
   -1,   -1,   -1,  271,   -1,  273,  274,   -1,  276,  287,
  288,   -1,  290,   -1,  292,   -1,  294,  295,   -1,  287,
  288,   -1,  290,   -1,  292,  267,  294,  295,   -1,  271,
   -1,  273,  274,   -1,  276,  267,   -1,   -1,   -1,  271,
   -1,  273,  274,   -1,  276,  287,  288,   -1,  290,   -1,
  292,   -1,  294,  295,   -1,  287,  288,   -1,  290,   -1,
  292,   -1,  294,  295,  267,   -1,   -1,   -1,  271,   -1,
  273,  274,   -1,  276,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  267,   -1,  287,  288,  271,   -1,  273,  274,
  267,  276,   -1,   -1,  271,   -1,  273,  274,   -1,  276,
   -1,   -1,  287,  288,
  };

					// line 401 "Parser.jay"
	}
					// line 936
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
  public const int COLON2 = 265;
  public const int COMMA = 267;
  public const int AT = 268;
  public const int FUNCTION_NAME = 269;
  public const int BRACKET_OPEN = 270;
  public const int BRACKET_CLOSE = 271;
  public const int PAREN_OPEN = 272;
  public const int PAREN_CLOSE = 273;
  public const int AND = 274;
  public const int and = 275;
  public const int OR = 276;
  public const int or = 277;
  public const int DIV = 278;
  public const int div = 279;
  public const int MOD = 280;
  public const int mod = 281;
  public const int PLUS = 282;
  public const int MINUS = 283;
  public const int ASTERISK = 284;
  public const int DOLLAR = 285;
  public const int BAR = 286;
  public const int EQ = 287;
  public const int NE = 288;
  public const int LE = 290;
  public const int GE = 292;
  public const int LT = 294;
  public const int GT = 295;
  public const int ANCESTOR = 296;
  public const int ancestor = 297;
  public const int ANCESTOR_OR_SELF = 298;
  public const int ATTRIBUTE = 300;
  public const int attribute = 301;
  public const int CHILD = 302;
  public const int child = 303;
  public const int DESCENDANT = 304;
  public const int descendant = 305;
  public const int DESCENDANT_OR_SELF = 306;
  public const int FOLLOWING = 308;
  public const int following = 309;
  public const int FOLLOWING_SIBLING = 310;
  public const int sibling = 311;
  public const int NAMESPACE = 312;
  public const int NameSpace = 313;
  public const int PARENT = 314;
  public const int parent = 315;
  public const int PRECEDING = 316;
  public const int preceding = 317;
  public const int PRECEDING_SIBLING = 318;
  public const int SELF = 320;
  public const int self = 321;
  public const int COMMENT = 322;
  public const int comment = 323;
  public const int TEXT = 324;
  public const int text = 325;
  public const int PROCESSING_INSTRUCTION = 326;
  public const int NODE = 328;
  public const int node = 329;
  public const int MULTIPLY = 330;
  public const int NUMBER = 331;
  public const int LITERAL = 332;
  public const int QName = 333;
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
