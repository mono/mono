// created by jay 0.7 (c) 1998 Axel.Schreiner@informatik.uni-osnabrueck.de

#line 2 "C:\Users\mattl_000.SURFACE\Projects\mono\mcs\class\System.Data\Mono.Data.SqlExpressions\parser.jay"
//
// Parser.jay
//
// Author:
//   Juraj Skripsky (juraj@hotfeet.ch)
//
// (C) 2004 HotFeet GmbH (http://www.hotfeet.ch)
// Copyright 2011 Xamarin Inc (http://www.xamarin.com)
//

using System;
using System.Collections;
using System.Data;

#if WINDOWS_PHONE || NETFX_CORE
using MarshalByRefObject = System.Object;
#endif

namespace Mono.Data.SqlExpressions {

	internal class Parser {
		static Parser ()
		{
#if !WINDOWS_PHONE && !NETFX_CORE		
			if (Environment.GetEnvironmentVariable ("MONO_DEBUG_SQLEXPRESSIONS") != null)
				yacc_verbose_flag = 2;
#endif				
		}

		bool cacheAggregationResults = false;
		DataRow[] aggregationRows = null;
		static int yacc_verbose_flag;
		
		//called by DataTable.Select
		//called by DataColumn.set_Expression //FIXME: enable cache in this case?
		public Parser () {
			ErrorOutput = System.IO.TextWriter.Null;
			cacheAggregationResults = true;
		}
		
		//called by DataTable.Compute
		public Parser (DataRow[] aggregationRows)
		{
			ErrorOutput = System.IO.TextWriter.Null;
			this.aggregationRows = aggregationRows;
		}
		
		public IExpression Compile (string sqlExpr)
		{
			try {
				Tokenizer tokenizer = new Tokenizer (sqlExpr);
				if (yacc_verbose_flag > 1)
					return (IExpression) yyparse (tokenizer,
						new yydebug.yyDebugSimple ());
				else
					return (IExpression) yyparse (tokenizer);
			} catch (yyParser.yyException) {
				throw new SyntaxErrorException (String.Format ("Expression '{0}' is invalid.", sqlExpr));
			}
		}
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

  protected const int yyFinal = 25;
 // Put this array into a separate class so it is only initialized if debugging is actually used
 // Use MarshalByRefObject to disable inlining
 class YYRules : MarshalByRefObject {
  public static readonly string [] yyRule = {
    "$accept : Expr",
    "Expr : BoolExpr",
    "Expr : ArithExpr",
    "BoolExpr : PAROPEN BoolExpr PARCLOSE",
    "BoolExpr : BoolExpr AND BoolExpr",
    "BoolExpr : BoolExpr OR BoolExpr",
    "BoolExpr : NOT BoolExpr",
    "BoolExpr : Predicate",
    "Predicate : CompPredicate",
    "Predicate : IsPredicate",
    "Predicate : LikePredicate",
    "Predicate : InPredicate",
    "CompPredicate : ArithExpr CompOp ArithExpr",
    "CompOp : EQ",
    "CompOp : NE",
    "CompOp : LT",
    "CompOp : GT",
    "CompOp : LE",
    "CompOp : GE",
    "LE : LT EQ",
    "NE : LT GT",
    "GE : GT EQ",
    "ArithExpr : PAROPEN ArithExpr PARCLOSE",
    "ArithExpr : ArithExpr MUL ArithExpr",
    "ArithExpr : ArithExpr DIV ArithExpr",
    "ArithExpr : ArithExpr MOD ArithExpr",
    "ArithExpr : ArithExpr PLUS ArithExpr",
    "ArithExpr : ArithExpr MINUS ArithExpr",
    "ArithExpr : MINUS ArithExpr",
    "ArithExpr : Function",
    "ArithExpr : Value",
    "Value : LiteralValue",
    "Value : SingleColumnValue",
    "LiteralValue : StringLiteral",
    "LiteralValue : NumberLiteral",
    "LiteralValue : DateLiteral",
    "LiteralValue : BoolLiteral",
    "LiteralValue : NULL",
    "BoolLiteral : TRUE",
    "BoolLiteral : FALSE",
    "SingleColumnValue : LocalColumnValue",
    "SingleColumnValue : ParentColumnValue",
    "MultiColumnValue : LocalColumnValue",
    "MultiColumnValue : ChildColumnValue",
    "LocalColumnValue : ColumnName",
    "ParentColumnValue : PARENT DOT ColumnName",
    "ParentColumnValue : PARENT PAROPEN RelationName PARCLOSE DOT ColumnName",
    "ChildColumnValue : CHILD DOT ColumnName",
    "ChildColumnValue : CHILD PAROPEN RelationName PARCLOSE DOT ColumnName",
    "ColumnName : Identifier",
    "ColumnName : ColumnName DOT Identifier",
    "RelationName : Identifier",
    "Function : CalcFunction",
    "Function : AggFunction",
    "Function : StringFunction",
    "AggFunction : AggFunctionName PAROPEN MultiColumnValue PARCLOSE",
    "AggFunctionName : COUNT",
    "AggFunctionName : SUM",
    "AggFunctionName : AVG",
    "AggFunctionName : MAX",
    "AggFunctionName : MIN",
    "AggFunctionName : STDEV",
    "AggFunctionName : VAR",
    "StringExpr : SingleColumnValue",
    "StringExpr : StringLiteral",
    "StringExpr : Function",
    "StringFunction : TRIM PAROPEN ArithExpr PARCLOSE",
    "StringFunction : SUBSTRING PAROPEN ArithExpr COMMA ArithExpr COMMA ArithExpr PARCLOSE",
    "CalcFunction : IIF PAROPEN Expr COMMA Expr COMMA Expr PARCLOSE",
    "CalcFunction : ISNULL PAROPEN Expr COMMA Expr PARCLOSE",
    "CalcFunction : LEN PAROPEN Expr PARCLOSE",
    "CalcFunction : CONVERT PAROPEN Expr COMMA TypeSpecifier PARCLOSE",
    "TypeSpecifier : StringLiteral",
    "TypeSpecifier : Identifier",
    "IsPredicate : ArithExpr IS NULL",
    "IsPredicate : ArithExpr IS NOT NULL",
    "LikePredicate : StringExpr LIKE StringExpr",
    "LikePredicate : StringExpr NOT_LIKE StringExpr",
    "InPredicate : ArithExpr IN InPredicateValue",
    "InPredicate : ArithExpr NOT_IN InPredicateValue",
    "InPredicateValue : PAROPEN InValueList PARCLOSE",
    "InValueList : LiteralValue",
    "InValueList : InValueList COMMA LiteralValue",
  };
 public static string getRule (int index) {
    return yyRule [index];
 }
}
  protected static readonly string [] yyNames = {    
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
    null,null,null,null,null,null,null,"PAROPEN","PARCLOSE","AND","OR",
    "NOT","TRUE","FALSE","NULL","PARENT","CHILD","EQ","LT","GT","PLUS",
    "MINUS","MUL","DIV","MOD","DOT","COMMA","IS","IN","NOT_IN","LIKE",
    "NOT_LIKE","COUNT","SUM","AVG","MAX","MIN","STDEV","VAR","IIF",
    "SUBSTRING","ISNULL","LEN","TRIM","CONVERT","StringLiteral",
    "NumberLiteral","DateLiteral","Identifier","FunctionName","UMINUS",
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
case 3:
#line 114 "C:\Users\mattl_000.SURFACE\Projects\mono\mcs\class\System.Data\Mono.Data.SqlExpressions\parser.jay"
  {
		yyVal = (IExpression)yyVals[-1+yyTop];
	}
  break;
case 4:
#line 118 "C:\Users\mattl_000.SURFACE\Projects\mono\mcs\class\System.Data\Mono.Data.SqlExpressions\parser.jay"
  {
		yyVal = new BoolOperation (Operation.AND, (IExpression)yyVals[-2+yyTop], (IExpression)yyVals[0+yyTop]);
	}
  break;
case 5:
#line 122 "C:\Users\mattl_000.SURFACE\Projects\mono\mcs\class\System.Data\Mono.Data.SqlExpressions\parser.jay"
  {
		yyVal = new BoolOperation (Operation.OR, (IExpression)yyVals[-2+yyTop], (IExpression)yyVals[0+yyTop]);
	}
  break;
case 6:
#line 126 "C:\Users\mattl_000.SURFACE\Projects\mono\mcs\class\System.Data\Mono.Data.SqlExpressions\parser.jay"
  {
		yyVal = new Negation ((IExpression)yyVals[0+yyTop]);
	}
  break;
case 12:
#line 141 "C:\Users\mattl_000.SURFACE\Projects\mono\mcs\class\System.Data\Mono.Data.SqlExpressions\parser.jay"
  {
		yyVal = new Comparison ((Operation)yyVals[-1+yyTop], (IExpression)yyVals[-2+yyTop], (IExpression)yyVals[0+yyTop]);
	}
  break;
case 13:
#line 145 "C:\Users\mattl_000.SURFACE\Projects\mono\mcs\class\System.Data\Mono.Data.SqlExpressions\parser.jay"
  { yyVal = Operation.EQ; }
  break;
case 14:
#line 146 "C:\Users\mattl_000.SURFACE\Projects\mono\mcs\class\System.Data\Mono.Data.SqlExpressions\parser.jay"
  { yyVal = Operation.NE; }
  break;
case 15:
#line 147 "C:\Users\mattl_000.SURFACE\Projects\mono\mcs\class\System.Data\Mono.Data.SqlExpressions\parser.jay"
  { yyVal = Operation.LT; }
  break;
case 16:
#line 148 "C:\Users\mattl_000.SURFACE\Projects\mono\mcs\class\System.Data\Mono.Data.SqlExpressions\parser.jay"
  { yyVal = Operation.GT; }
  break;
case 17:
#line 149 "C:\Users\mattl_000.SURFACE\Projects\mono\mcs\class\System.Data\Mono.Data.SqlExpressions\parser.jay"
  { yyVal = Operation.LE; }
  break;
case 18:
#line 150 "C:\Users\mattl_000.SURFACE\Projects\mono\mcs\class\System.Data\Mono.Data.SqlExpressions\parser.jay"
  { yyVal = Operation.GE; }
  break;
case 22:
#line 161 "C:\Users\mattl_000.SURFACE\Projects\mono\mcs\class\System.Data\Mono.Data.SqlExpressions\parser.jay"
  {
		yyVal = (IExpression)yyVals[-1+yyTop];
	}
  break;
case 23:
#line 165 "C:\Users\mattl_000.SURFACE\Projects\mono\mcs\class\System.Data\Mono.Data.SqlExpressions\parser.jay"
  {
		yyVal = new ArithmeticOperation (Operation.MUL, (IExpression)yyVals[-2+yyTop], (IExpression)yyVals[0+yyTop]);
	}
  break;
case 24:
#line 169 "C:\Users\mattl_000.SURFACE\Projects\mono\mcs\class\System.Data\Mono.Data.SqlExpressions\parser.jay"
  {
		yyVal = new ArithmeticOperation (Operation.DIV, (IExpression)yyVals[-2+yyTop], (IExpression)yyVals[0+yyTop]);
	}
  break;
case 25:
#line 173 "C:\Users\mattl_000.SURFACE\Projects\mono\mcs\class\System.Data\Mono.Data.SqlExpressions\parser.jay"
  {
		yyVal = new ArithmeticOperation (Operation.MOD, (IExpression)yyVals[-2+yyTop], (IExpression)yyVals[0+yyTop]);
	}
  break;
case 26:
#line 177 "C:\Users\mattl_000.SURFACE\Projects\mono\mcs\class\System.Data\Mono.Data.SqlExpressions\parser.jay"
  {
		yyVal = new ArithmeticOperation (Operation.ADD, (IExpression)yyVals[-2+yyTop], (IExpression)yyVals[0+yyTop]);
	}
  break;
case 27:
#line 181 "C:\Users\mattl_000.SURFACE\Projects\mono\mcs\class\System.Data\Mono.Data.SqlExpressions\parser.jay"
  {
		yyVal = new ArithmeticOperation (Operation.SUB, (IExpression)yyVals[-2+yyTop], (IExpression)yyVals[0+yyTop]);
	}
  break;
case 28:
#line 185 "C:\Users\mattl_000.SURFACE\Projects\mono\mcs\class\System.Data\Mono.Data.SqlExpressions\parser.jay"
  {
		yyVal = new Negative ((IExpression)yyVals[0+yyTop]);
	}
  break;
case 33:
#line 196 "C:\Users\mattl_000.SURFACE\Projects\mono\mcs\class\System.Data\Mono.Data.SqlExpressions\parser.jay"
  { yyVal = new Literal (yyVals[0+yyTop]); }
  break;
case 34:
#line 197 "C:\Users\mattl_000.SURFACE\Projects\mono\mcs\class\System.Data\Mono.Data.SqlExpressions\parser.jay"
  { yyVal = new Literal (yyVals[0+yyTop]); }
  break;
case 35:
#line 198 "C:\Users\mattl_000.SURFACE\Projects\mono\mcs\class\System.Data\Mono.Data.SqlExpressions\parser.jay"
  { yyVal = new Literal (yyVals[0+yyTop]); }
  break;
case 37:
#line 200 "C:\Users\mattl_000.SURFACE\Projects\mono\mcs\class\System.Data\Mono.Data.SqlExpressions\parser.jay"
  { yyVal = new Literal (null); }
  break;
case 38:
#line 204 "C:\Users\mattl_000.SURFACE\Projects\mono\mcs\class\System.Data\Mono.Data.SqlExpressions\parser.jay"
  { yyVal = new Literal (true); }
  break;
case 39:
#line 205 "C:\Users\mattl_000.SURFACE\Projects\mono\mcs\class\System.Data\Mono.Data.SqlExpressions\parser.jay"
  { yyVal = new Literal (false); }
  break;
case 44:
#line 222 "C:\Users\mattl_000.SURFACE\Projects\mono\mcs\class\System.Data\Mono.Data.SqlExpressions\parser.jay"
  {
		yyVal = new ColumnReference ((string)yyVals[0+yyTop]);
	}
  break;
case 45:
#line 229 "C:\Users\mattl_000.SURFACE\Projects\mono\mcs\class\System.Data\Mono.Data.SqlExpressions\parser.jay"
  {
		yyVal = new ColumnReference (ReferencedTable.Parent, null, (string)yyVals[0+yyTop]);
	}
  break;
case 46:
#line 233 "C:\Users\mattl_000.SURFACE\Projects\mono\mcs\class\System.Data\Mono.Data.SqlExpressions\parser.jay"
  {
		yyVal = new ColumnReference (ReferencedTable.Parent, (string)yyVals[-3+yyTop], (string)yyVals[0+yyTop]);
	}
  break;
case 47:
#line 240 "C:\Users\mattl_000.SURFACE\Projects\mono\mcs\class\System.Data\Mono.Data.SqlExpressions\parser.jay"
  {
		yyVal = new ColumnReference (ReferencedTable.Child, null, (string)yyVals[0+yyTop]);
	}
  break;
case 48:
#line 244 "C:\Users\mattl_000.SURFACE\Projects\mono\mcs\class\System.Data\Mono.Data.SqlExpressions\parser.jay"
  {
		yyVal = new ColumnReference (ReferencedTable.Child, (string)yyVals[-3+yyTop], (string)yyVals[0+yyTop]);
	}
  break;
case 50:
#line 252 "C:\Users\mattl_000.SURFACE\Projects\mono\mcs\class\System.Data\Mono.Data.SqlExpressions\parser.jay"
  {
		yyVal = (string)yyVals[-2+yyTop] + "." + (string)yyVals[0+yyTop];
	}
  break;
case 55:
#line 268 "C:\Users\mattl_000.SURFACE\Projects\mono\mcs\class\System.Data\Mono.Data.SqlExpressions\parser.jay"
  {
		yyVal = new Aggregation (cacheAggregationResults, aggregationRows, (AggregationFunction)yyVals[-3+yyTop], (ColumnReference)yyVals[-1+yyTop]);
	}
  break;
case 56:
#line 272 "C:\Users\mattl_000.SURFACE\Projects\mono\mcs\class\System.Data\Mono.Data.SqlExpressions\parser.jay"
  { yyVal = AggregationFunction.Count; }
  break;
case 57:
#line 273 "C:\Users\mattl_000.SURFACE\Projects\mono\mcs\class\System.Data\Mono.Data.SqlExpressions\parser.jay"
  { yyVal = AggregationFunction.Sum; }
  break;
case 58:
#line 274 "C:\Users\mattl_000.SURFACE\Projects\mono\mcs\class\System.Data\Mono.Data.SqlExpressions\parser.jay"
  { yyVal = AggregationFunction.Avg; }
  break;
case 59:
#line 275 "C:\Users\mattl_000.SURFACE\Projects\mono\mcs\class\System.Data\Mono.Data.SqlExpressions\parser.jay"
  { yyVal = AggregationFunction.Max; }
  break;
case 60:
#line 276 "C:\Users\mattl_000.SURFACE\Projects\mono\mcs\class\System.Data\Mono.Data.SqlExpressions\parser.jay"
  { yyVal = AggregationFunction.Min; }
  break;
case 61:
#line 277 "C:\Users\mattl_000.SURFACE\Projects\mono\mcs\class\System.Data\Mono.Data.SqlExpressions\parser.jay"
  { yyVal = AggregationFunction.StDev; }
  break;
case 62:
#line 278 "C:\Users\mattl_000.SURFACE\Projects\mono\mcs\class\System.Data\Mono.Data.SqlExpressions\parser.jay"
  { yyVal = AggregationFunction.Var; }
  break;
case 64:
#line 283 "C:\Users\mattl_000.SURFACE\Projects\mono\mcs\class\System.Data\Mono.Data.SqlExpressions\parser.jay"
  { yyVal = new Literal (yyVals[0+yyTop]); }
  break;
case 66:
#line 291 "C:\Users\mattl_000.SURFACE\Projects\mono\mcs\class\System.Data\Mono.Data.SqlExpressions\parser.jay"
  {
		yyVal = new TrimFunction ((IExpression)yyVals[-1+yyTop]);
	}
  break;
case 67:
#line 295 "C:\Users\mattl_000.SURFACE\Projects\mono\mcs\class\System.Data\Mono.Data.SqlExpressions\parser.jay"
  {
		yyVal = new SubstringFunction ((IExpression)yyVals[-5+yyTop], (IExpression)yyVals[-3+yyTop], (IExpression)yyVals[-1+yyTop]);
	}
  break;
case 68:
#line 302 "C:\Users\mattl_000.SURFACE\Projects\mono\mcs\class\System.Data\Mono.Data.SqlExpressions\parser.jay"
  {
		yyVal = new IifFunction ((IExpression)yyVals[-5+yyTop], (IExpression)yyVals[-3+yyTop], (IExpression)yyVals[-1+yyTop]);
	}
  break;
case 69:
#line 306 "C:\Users\mattl_000.SURFACE\Projects\mono\mcs\class\System.Data\Mono.Data.SqlExpressions\parser.jay"
  {
		yyVal = new IsNullFunction ((IExpression)yyVals[-3+yyTop], (IExpression)yyVals[-1+yyTop]);
	}
  break;
case 70:
#line 310 "C:\Users\mattl_000.SURFACE\Projects\mono\mcs\class\System.Data\Mono.Data.SqlExpressions\parser.jay"
  {
		yyVal = new LenFunction ((IExpression)yyVals[-1+yyTop]);
	}
  break;
case 71:
#line 314 "C:\Users\mattl_000.SURFACE\Projects\mono\mcs\class\System.Data\Mono.Data.SqlExpressions\parser.jay"
  {
		yyVal = new ConvertFunction ((IExpression)yyVals[-3+yyTop], (string)yyVals[-1+yyTop]);
	}
  break;
case 74:
#line 326 "C:\Users\mattl_000.SURFACE\Projects\mono\mcs\class\System.Data\Mono.Data.SqlExpressions\parser.jay"
  {
		yyVal = new Comparison (Operation.EQ, (IExpression)yyVals[-2+yyTop], new Literal (null));
	}
  break;
case 75:
#line 330 "C:\Users\mattl_000.SURFACE\Projects\mono\mcs\class\System.Data\Mono.Data.SqlExpressions\parser.jay"
  {
		yyVal = new Comparison (Operation.NE, (IExpression)yyVals[-3+yyTop], new Literal (null));
	}
  break;
case 76:
#line 337 "C:\Users\mattl_000.SURFACE\Projects\mono\mcs\class\System.Data\Mono.Data.SqlExpressions\parser.jay"
  {
		yyVal = new Like ((IExpression)yyVals[-2+yyTop], (IExpression)yyVals[0+yyTop]);
	}
  break;
case 77:
#line 341 "C:\Users\mattl_000.SURFACE\Projects\mono\mcs\class\System.Data\Mono.Data.SqlExpressions\parser.jay"
  {
		yyVal = new Negation (new Like ((IExpression)yyVals[-2+yyTop], (IExpression)yyVals[0+yyTop]));
	}
  break;
case 78:
#line 348 "C:\Users\mattl_000.SURFACE\Projects\mono\mcs\class\System.Data\Mono.Data.SqlExpressions\parser.jay"
  {
		yyVal = new In ((IExpression)yyVals[-2+yyTop], (IList)yyVals[0+yyTop]);
	}
  break;
case 79:
#line 352 "C:\Users\mattl_000.SURFACE\Projects\mono\mcs\class\System.Data\Mono.Data.SqlExpressions\parser.jay"
  {
		yyVal = new Negation (new In ((IExpression)yyVals[-2+yyTop], (IList)yyVals[0+yyTop]));
	}
  break;
case 80:
#line 356 "C:\Users\mattl_000.SURFACE\Projects\mono\mcs\class\System.Data\Mono.Data.SqlExpressions\parser.jay"
  { yyVal = yyVals[-1+yyTop]; }
  break;
case 81:
  case_81();
  break;
case 82:
#line 368 "C:\Users\mattl_000.SURFACE\Projects\mono\mcs\class\System.Data\Mono.Data.SqlExpressions\parser.jay"
  {
		((IList)(yyVal = yyVals[-2+yyTop])).Add (yyVals[0+yyTop]);
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
void case_81()
#line 361 "C:\Users\mattl_000.SURFACE\Projects\mono\mcs\class\System.Data\Mono.Data.SqlExpressions\parser.jay"
{
		yyVal = new ArrayList();
		((IList)yyVal).Add (yyVals[0+yyTop]);
	}

#line default
   static readonly short [] yyLhs  = {              -1,
    0,    0,    1,    1,    1,    1,    1,    3,    3,    3,
    3,    4,    8,    8,    8,    8,    8,    8,   10,    9,
   11,    2,    2,    2,    2,    2,    2,    2,    2,    2,
   13,   13,   14,   14,   14,   14,   14,   16,   16,   15,
   15,   19,   19,   17,   18,   18,   20,   20,   21,   21,
   22,   12,   12,   12,   24,   26,   26,   26,   26,   26,
   26,   26,   27,   27,   27,   25,   25,   23,   23,   23,
   23,   28,   28,    5,    5,    6,    6,    7,    7,   29,
   30,   30,
  };
   static readonly short [] yyLen = {           2,
    1,    1,    3,    3,    3,    2,    1,    1,    1,    1,
    1,    3,    1,    1,    1,    1,    1,    1,    2,    2,
    2,    3,    3,    3,    3,    3,    3,    2,    1,    1,
    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,
    1,    1,    1,    1,    3,    6,    3,    6,    1,    3,
    1,    1,    1,    1,    4,    1,    1,    1,    1,    1,
    1,    1,    1,    1,    1,    4,    8,    8,    6,    4,
    6,    1,    1,    3,    4,    3,    3,    3,    3,    3,
    1,    3,
  };
   static readonly short [] yyDefRed = {            0,
    0,    0,   38,   39,   37,    0,    0,   56,   57,   58,
   59,   60,   61,   62,    0,    0,    0,    0,    0,    0,
    0,   34,   35,   49,    0,    0,    0,    7,    8,    9,
   10,   11,    0,   30,   31,    0,   36,   40,   41,    0,
   52,   53,   54,    0,    0,    0,    0,    6,    0,    0,
    0,    0,   33,   28,   29,   32,    0,    0,    0,    0,
    0,    0,    0,    0,   13,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,   14,   17,   18,    0,
    0,    0,    0,    3,   22,   51,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    4,    0,   19,   20,   21,
    0,    0,   23,   24,   25,    0,   74,    0,   78,   79,
    0,   50,    0,   42,    0,   43,   64,   65,   63,   76,
   77,    0,    0,    0,    0,   70,   66,    0,   75,   81,
    0,    0,    0,   55,    0,    0,    0,    0,   72,   73,
    0,   80,    0,    0,    0,    0,    0,    0,   69,   71,
   82,    0,    0,    0,    0,   68,   67,    0,
  };
  protected static readonly short [] yyDgoto  = {            25,
   26,   27,   28,   29,   30,   31,   32,   76,   77,   78,
   79,   33,   34,   35,   36,   37,   38,   39,  115,  116,
   40,   87,   41,   42,   43,   44,   45,  141,  109,  131,
  };
  protected static readonly short [] yySindex = {         -104,
 -104, -104,    0,    0,    0, -251,  -62,    0,    0,    0,
    0,    0,    0,    0, -242, -230, -224, -213, -192, -189,
    0,    0,    0,    0,    0, -219,  231,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0, -229,
    0,    0,    0, -184, -200, -146,  -21,    0,  231, -209,
 -207,  -62,    0,    0,    0,    0, -104,  -62, -104, -104,
  -62, -104, -104, -104,    0, -246, -180,  -62,  -62,  -62,
  -62,  -62, -222, -164, -164,  -62,    0,    0,    0, -197,
 -255,  232,  232,    0,    0,    0, -151, -229, -211, -161,
 -254, -159, -135, -131, -147,    0, -134,    0,    0,    0,
 -137, -137,    0,    0,    0, -133,    0, -259,    0,    0,
 -102,    0, -245,    0, -114,    0,    0,    0,    0,    0,
    0, -130, -104,  -62, -104,    0,    0, -250,    0,    0,
 -244, -209, -207,    0, -207, -121, -220,  -96,    0,    0,
  -95,    0, -259,  -94, -229, -229, -104,  -62,    0,    0,
    0, -119,  -93, -124, -207,    0,    0, -229,
  };
  protected static readonly short [] yyRindex = {            0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
   66,    0,    0,    0,    0,   90,  152,    0,    0,    0,
    0,    0,   83,    0,    0,  100,    0,    0,    0,    1,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  156,  198,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,   25,    0,    0,
    0,    0,    0,    0,    0,    0,  128,    0,    0,    0,
  124,  138,    0,    0,    0,    0,    0,    0,    0,    0,
   55,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  -92,   49,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  -85,
  };
  protected static readonly short [] yyGindex = {          -31,
    8,    6,    0,    0,    0,    0,    0,    0,    0,    0,
    0,   27,    0, -108,   50,    0,   95,    0,    0,    0,
  -49,   45,    0,    0,    0,    0,   22,    0,  121,    0,
  };
  protected static readonly short [] yyTable = {           130,
   44,   88,    3,    4,    5,   50,   47,   49,   46,   48,
  113,  132,   54,  142,   57,   68,   69,   70,   71,   72,
   98,  124,   99,   51,   45,   90,   58,   92,   93,  133,
   95,  143,   59,   55,  151,   53,   22,   23,  106,   63,
   64,  107,   24,   60,  139,   80,   85,  140,   46,   68,
   69,   70,   71,   72,   12,  148,   56,   89,   68,   69,
   70,   71,   72,   91,   61,   33,   94,   62,   49,   49,
   96,   97,   81,  101,  102,  103,  104,  105,   55,   82,
   83,  111,   29,  145,   55,  146,  100,   55,   86,    1,
   24,  136,  108,  138,   55,   55,   55,   55,   55,   32,
  112,   56,   55,  120,  121,  158,  122,   56,  118,  118,
   56,   84,   63,   64,  123,  153,  125,   56,   56,   56,
   56,   56,  126,   26,   63,   56,  127,    5,  128,  137,
  129,  119,  119,  157,   70,   71,   72,   27,   68,   69,
   70,   71,   72,  134,  135,   68,   69,   70,   71,   72,
   55,    2,    1,  154,  147,  155,    2,    3,    4,    5,
    6,  149,  150,  152,  156,   47,    7,   68,   69,   70,
   71,   72,   48,   56,   55,  114,  144,    8,    9,   10,
   11,   12,   13,   14,   15,   16,   17,   18,   19,   20,
   21,   22,   23,   24,   52,  110,    0,   56,    0,    3,
    4,    5,    6,    0,    0,    0,    0,    0,    7,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    8,
    9,   10,   11,   12,   13,   14,   15,   16,   17,   18,
   19,   20,   53,   22,   23,   24,   85,    0,    0,    0,
    0,    0,    0,    0,    0,   65,   66,   67,   68,   69,
   70,   71,   72,    0,    0,   73,   74,   75,   44,   44,
   44,    0,    0,    0,    0,    0,    0,   44,   44,   44,
   44,   44,   44,   44,   44,    0,   44,   44,   44,   44,
   44,   44,   45,   45,   45,    0,    0,    0,    0,    0,
    0,   45,   45,   45,   45,   45,   45,   45,   45,    0,
   45,   45,   45,   45,   45,   45,   46,   46,   46,    0,
    0,    0,   12,   12,   12,   46,   46,   46,   46,   46,
   46,   46,   46,   33,   46,   46,   46,   46,   46,   46,
   12,    0,   33,   33,   33,   33,   33,   33,   33,   33,
   29,   33,   33,   33,   33,   64,   64,    1,    0,   29,
   29,   29,   29,   29,   29,   29,   29,   32,   29,   29,
   29,   29,   65,   65,    0,    1,   32,   32,   32,   32,
   32,   32,   32,   32,    0,   32,   32,   32,   32,   63,
   63,   26,   26,   26,    0,    5,    0,    5,    0,    0,
   26,   26,   26,   26,   26,   27,   27,   27,    0,   26,
   26,   26,   26,    5,   27,   27,   27,   27,   27,    2,
    0,    0,   15,   27,   27,   27,   27,   15,   15,   15,
   15,    0,    0,    0,    0,    0,   15,    2,    0,    0,
    0,    0,    0,    0,    0,    0,    0,   15,   15,   15,
   15,   15,   15,   15,   15,   15,   15,   15,   15,   15,
   15,   15,   15,   15,   16,    0,    0,    0,    0,   16,
   16,   16,   16,    0,    0,    0,    0,    0,   16,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,   16,
   16,   16,   16,   16,   16,   16,   16,   16,   16,   16,
   16,   16,   16,   16,   16,   16,    6,   65,   66,   67,
   68,   69,   70,   71,   72,    0,    0,   73,   74,   75,
    0,    0,    0,    8,    9,   10,   11,   12,   13,   14,
   15,   16,   17,   18,   19,   20,  117,    0,    0,   24,
  };
  protected static readonly short [] yyCheck = {           108,
    0,   51,  262,  263,  264,  257,    1,    2,    1,    2,
  266,  257,    7,  258,  257,  270,  271,  272,  273,  274,
  267,  276,  269,  275,    0,   57,  257,   59,   60,  275,
   62,  276,  257,    7,  143,  295,  296,  297,  261,  259,
  260,  264,  298,  257,  295,  275,  258,  298,    0,  270,
  271,  272,  273,  274,    0,  276,    7,   52,  270,  271,
  272,  273,  274,   58,  257,    0,   61,  257,   63,   64,
   63,   64,  257,   68,   69,   70,   71,   72,   52,  280,
  281,   76,    0,  133,   58,  135,  267,   61,  298,    0,
  298,  123,  257,  125,   68,   69,   70,   71,   72,    0,
  298,   52,   76,   82,   83,  155,  258,   58,   82,   83,
   61,  258,  259,  260,  276,  147,  276,   68,   69,   70,
   71,   72,  258,    0,  259,   76,  258,    0,  276,  124,
  264,   82,   83,  258,  272,  273,  274,    0,  270,  271,
  272,  273,  274,  258,  275,  270,  271,  272,  273,  274,
  124,    0,  257,  148,  276,  275,  261,  262,  263,  264,
  265,  258,  258,  258,  258,  258,  271,  270,  271,  272,
  273,  274,  258,  124,  148,   81,  132,  282,  283,  284,
  285,  286,  287,  288,  289,  290,  291,  292,  293,  294,
  295,  296,  297,  298,  257,   75,   -1,  148,   -1,  262,
  263,  264,  265,   -1,   -1,   -1,   -1,   -1,  271,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  282,
  283,  284,  285,  286,  287,  288,  289,  290,  291,  292,
  293,  294,  295,  296,  297,  298,  258,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  267,  268,  269,  270,  271,
  272,  273,  274,   -1,   -1,  277,  278,  279,  258,  259,
  260,   -1,   -1,   -1,   -1,   -1,   -1,  267,  268,  269,
  270,  271,  272,  273,  274,   -1,  276,  277,  278,  279,
  280,  281,  258,  259,  260,   -1,   -1,   -1,   -1,   -1,
   -1,  267,  268,  269,  270,  271,  272,  273,  274,   -1,
  276,  277,  278,  279,  280,  281,  258,  259,  260,   -1,
   -1,   -1,  258,  259,  260,  267,  268,  269,  270,  271,
  272,  273,  274,  258,  276,  277,  278,  279,  280,  281,
  276,   -1,  267,  268,  269,  270,  271,  272,  273,  274,
  258,  276,  277,  278,  279,  280,  281,  258,   -1,  267,
  268,  269,  270,  271,  272,  273,  274,  258,  276,  277,
  278,  279,  280,  281,   -1,  276,  267,  268,  269,  270,
  271,  272,  273,  274,   -1,  276,  277,  278,  279,  280,
  281,  258,  259,  260,   -1,  258,   -1,  260,   -1,   -1,
  267,  268,  269,  270,  271,  258,  259,  260,   -1,  276,
  277,  278,  279,  276,  267,  268,  269,  270,  271,  258,
   -1,   -1,  257,  276,  277,  278,  279,  262,  263,  264,
  265,   -1,   -1,   -1,   -1,   -1,  271,  276,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  282,  283,  284,
  285,  286,  287,  288,  289,  290,  291,  292,  293,  294,
  295,  296,  297,  298,  257,   -1,   -1,   -1,   -1,  262,
  263,  264,  265,   -1,   -1,   -1,   -1,   -1,  271,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  282,
  283,  284,  285,  286,  287,  288,  289,  290,  291,  292,
  293,  294,  295,  296,  297,  298,  265,  267,  268,  269,
  270,  271,  272,  273,  274,   -1,   -1,  277,  278,  279,
   -1,   -1,   -1,  282,  283,  284,  285,  286,  287,  288,
  289,  290,  291,  292,  293,  294,  295,   -1,   -1,  298,
  };

#line 372 "C:\Users\mattl_000.SURFACE\Projects\mono\mcs\class\System.Data\Mono.Data.SqlExpressions\parser.jay"
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
  public const int PAROPEN = 257;
  public const int PARCLOSE = 258;
  public const int AND = 259;
  public const int OR = 260;
  public const int NOT = 261;
  public const int TRUE = 262;
  public const int FALSE = 263;
  public const int NULL = 264;
  public const int PARENT = 265;
  public const int CHILD = 266;
  public const int EQ = 267;
  public const int LT = 268;
  public const int GT = 269;
  public const int PLUS = 270;
  public const int MINUS = 271;
  public const int MUL = 272;
  public const int DIV = 273;
  public const int MOD = 274;
  public const int DOT = 275;
  public const int COMMA = 276;
  public const int IS = 277;
  public const int IN = 278;
  public const int NOT_IN = 279;
  public const int LIKE = 280;
  public const int NOT_LIKE = 281;
  public const int COUNT = 282;
  public const int SUM = 283;
  public const int AVG = 284;
  public const int MAX = 285;
  public const int MIN = 286;
  public const int STDEV = 287;
  public const int VAR = 288;
  public const int IIF = 289;
  public const int SUBSTRING = 290;
  public const int ISNULL = 291;
  public const int LEN = 292;
  public const int TRIM = 293;
  public const int CONVERT = 294;
  public const int StringLiteral = 295;
  public const int NumberLiteral = 296;
  public const int DateLiteral = 297;
  public const int Identifier = 298;
  public const int FunctionName = 299;
  public const int UMINUS = 300;
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
