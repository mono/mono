// created by jay 0.7 (c) 1998 Axel.Schreiner@informatik.uni-osnabrueck.de

#line 1 "mb-parser.jay"

//
// Mono.MonoBASIC.Parser.cs (from .jay): The Parser for the MonoBASIC compiler
//
// Author: A Rafael D Teixeira (rafaelteixeirabr@hotmail.com)
//
// Licensed under the terms of the GNU GPL
//
// Copyright (C) 2001 A Rafael D Teixeira
//
// TODO:
//	Nearly everything
//

namespace Mono.MonoBASIC
{
	using System.Text;
	using System;
	using System.Collections;
	using Mono.Languages;
	using Mono.CSharp;

	/// <summary>
	///    The MonoBASIC Parser
	/// </summary>
	public class Parser : GenericParser 
	{
		Namespace     current_namespace;
		TypeContainer current_container;
	
/*
		/// <summary>
		///   Current block is used to add statements as we find
		///   them.  
		/// </summary>
		Block      current_block;

		/// <summary>
		///   Current interface is used by the various declaration
		///   productions in the interface declaration to "add"
		///   the interfaces as we find them.
		/// </summary>
		Interface  current_interface;

		/// <summary>
		///   This is used by the unary_expression code to resolve
		///   a name against a parameter.  
		/// </summary>
		Parameters current_local_parameters;

		/// <summary>
		///   Using during property parsing to describe the implicit
		///   value parameter that is passed to the "set" accessor
		///   method
		/// </summary>
		Parameter [] implicit_value_parameters;

*/
		bool UseExtendedSyntax; // for ".mbs" files

		public override string[] extensions()
		{
			string [] list = { ".vb", ".mbs" };
			return list;
		}

#line 71 "-"

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
  protected yydebug.yyDebug yydebug;

  protected static  int yyFinal = 2;
  public static  string [] yyRule = {
    "$accept : compilation_unit",
    "compilation_unit : opt_imports_directives opt_attributes opt_namespace_member_declarations EOF",
    "qualified_identifier : IDENTIFIER",
    "qualified_identifier : qualified_identifier DOT IDENTIFIER",
    "opt_imports_directives :",
    "opt_imports_directives : imports_directives",
    "imports_directives : imports_directive",
    "imports_directives : imports_directives imports_directive",
    "imports_directive : imports_namespace_directive",
    "imports_namespace_directive : IMPORTS namespace_name EOL",
    "opt_attributes :",
    "$$1 :",
    "namespace_declaration : NAMESPACE qualified_identifier EOL $$1 opt_imports_directives opt_namespace_member_declarations END NAMESPACE EOL",
    "namespace_name : qualified_identifier",
    "opt_namespace_member_declarations :",
    "opt_namespace_member_declarations : namespace_member_declarations",
    "namespace_member_declarations : namespace_member_declaration",
    "namespace_member_declarations : namespace_member_declarations namespace_member_declaration",
    "namespace_member_declaration : namespace_declaration",
    "namespace_member_declaration : type_declaration",
    "type_declaration : class_declaration",
    "$$2 :",
    "class_declaration : CLASS IDENTIFIER EOL $$2 opt_class_member_declarations END CLASS EOL",
    "opt_class_member_declarations :",
    "opt_class_member_declarations : class_member_declarations",
    "class_member_declarations : class_member_declaration",
    "class_member_declarations : class_member_declarations class_member_declaration",
    "class_member_declaration : type_declaration",
    "class_member_declaration : sub_declaration",
    "sub_declaration : SUB qualified_identifier OPEN_PARENS opt_formal_parameters CLOSE_PARENS EOL opt_statements END SUB EOL",
    "opt_statements :",
    "opt_statements : qualified_identifier OPEN_PARENS opt_actual_parameters CLOSE_PARENS EOL",
    "opt_formal_parameters :",
    "opt_formal_parameters : qualified_identifier AS qualified_identifier",
    "opt_actual_parameters :",
    "opt_actual_parameters : qualified_identifier",
    "opt_actual_parameters : LITERAL_STRING",
  };
  protected static  string [] yyName = {    
    "end-of-file",null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,"'%'","'&'",
    null,"'('","')'","'*'","'+'","','","'-'","'.'","'/'",null,null,null,
    null,null,null,null,null,null,null,"':'",null,"'<'","'='","'>'","'?'",
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,
    "'['","'\\\\'","']'","'^'",null,null,null,null,null,null,null,null,
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
    "EOF","NONE","ERROR","ADDHANDLER","ADDRESSOF","ALIAS","AND","ANDALSO",
    "ANSI","AS","ASSEMBLY","AUTO","BOOLEAN","BYREF","BYTE","BYVAL","CALL",
    "CASE","CATCH","CBOOL","CBYTE","CCHAR","CDATE","CDEC","CDBL","CHAR",
    "CINT","CLASS","CLNG","COBJ","CONST","CSHORT","CSNG","CSTR","CTYPE",
    "DATE","DECIMAL","DECLARE","DEFAULT","DELEGATE","DESCRIPTION","DIM",
    "DO","DOUBLE","EACH","ELSE","ELSEIF","END","ENUM","EOL","ERASE",
    "EVENT","EXIT","FALSE","FINALLY","FOR","FRIEND","FUNCTION","GET",
    "GETTYPE","GOTO","HANDLES","IF","IMPLEMENTS","IMPORTS","IN",
    "INHERITS","INTEGER","INTERFACE","IS","LET","LIB","LIKE","LONG",
    "LOOP","ME","MOD","MODULE","MUSTINHERIT","MUSTOVERRIDE","MYBASE",
    "MYCLASS","NAMESPACE","NEW","NEXT","NOT","NOTHING","NOTINHERITABLE",
    "NOTOVERRIDABLE","OBJECT","ON","OPTION","OPTIONAL","OR","ORELSE",
    "OVERLOADS","OVERRIDABLE","OVERRIDES","PARAMETER","PARAM_ARRAY",
    "PRESERVE","PRIVATE","PROPERTY","PROTECTED","PUBLIC","RAISEEVENT",
    "READONLY","REDIM","REM","REMOVEHANDLER","RESUME","RETURN","SELECT",
    "SET","SHADOWS","SHARED","SHORT","SINGLE","SIZEOF","STATIC","STEP",
    "STOP","STRING","STRUCTURE","SUB","SUMMARY","SYNCLOCK","THEN","THROW",
    "TO","TRUE","TRY","TYPEOF","UNICODE","UNTIL","VARIANT","WHEN","WHILE",
    "WITH","WITHEVENTS","WRITEONLY","XOR","OPEN_BRACKET","CLOSE_BRACKET",
    "OPEN_PARENS","CLOSE_PARENS","DOT","COMMA","COLON","PLUS","MINUS",
    "ASSIGN","OP_LT","OP_GT","STAR","PERCENT","DIV","OP_EXP","INTERR",
    "OP_IDIV","OP_CONCAT","OP_LE","\"<=\"","OP_GE","\">=\"","OP_EQ",
    "\"==\"","OP_NE","\"<>\"","OP_AND","OP_OR","OP_XOR","OP_MODULUS",
    "OP_MULT_ASSIGN","\"*=\"","OP_DIV_ASSIGN","\"/=\"","OP_IDIV_ASSIGN",
    "\"\\\\=\"","OP_ADD_ASSIGN","\"+=\"","OP_SUB_ASSIGN","\"-=\"",
    "OP_CONCAT_ASSIGN","\"&=\"","OP_EXP_ASSIGN","\"^=\"",
    "LITERAL_INTEGER","\"int literal\"","LITERAL_SINGLE",
    "\"float literal\"","LITERAL_DOUBLE","\"double literal\"",
    "LITERAL_DECIMAL","\"decimal literal\"","LITERAL_CHARACTER",
    "\"character literal\"","LITERAL_STRING","\"string literal\"",
    "IDENTIFIER","LOWPREC","BITWISE_OR","BITWISE_AND","OP_SHIFT_LEFT",
    "OP_SHIFT_RIGHT","BITWISE_NOT","CARRET","UMINUS","OP_INC","OP_DEC",
    "OPEN_BRACE","HIGHPREC",
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
  public Object yyparse (yyParser.yyInput yyLex, Object yydebug)
				 {
    this.yydebug = (yydebug.yyDebug)yydebug;
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
        System.Array.Copy(yyStates, i, 0);
        yyStates = i;
        Object[] o = new Object[yyVals.Length+yyMax];
        System.Array.Copy(yyVals, o, 0);
        yyVals = o;
      }
      yyStates[yyTop] = yyState;
      yyVals[yyTop] = yyVal;
      if (yydebug != null) yydebug.push(yyState, yyVal);

      yyDiscarded: for (;;) {	// discarding a token does not change stack
        int yyN;
        if ((yyN = yyDefRed[yyState]) == 0) {	// else [default] reduce (yyN)
          if (yyToken < 0) {
            yyToken = yyLex.advance() ? yyLex.token() : 0;
            if (yydebug != null)
              yydebug.lex(yyState, yyToken, yyname(yyToken), yyLex.value());
          }
          if ((yyN = yySindex[yyState]) != 0 && ((yyN += yyToken) >= 0)
              && (yyN < yyTable.Length) && (yyCheck[yyN] == yyToken)) {
            if (yydebug != null)
              yydebug.shift(yyState, yyTable[yyN], yyErrorFlag-1);
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
              if (yydebug != null) yydebug.error("syntax error");
              goto case 1;
            case 1: case 2:
              yyErrorFlag = 3;
              do {
                if ((yyN = yySindex[yyStates[yyTop]]) != 0
                    && (yyN += Token.yyErrorCode) >= 0 && yyN < yyTable.Length
                    && yyCheck[yyN] == Token.yyErrorCode) {
                  if (yydebug != null)
                    yydebug.shift(yyStates[yyTop], yyTable[yyN], 3);
                  yyState = yyTable[yyN];
                  yyVal = yyLex.value();
                  goto yyLoop;
                }
                if (yydebug != null) yydebug.pop(yyStates[yyTop]);
              } while (-- yyTop >= 0);
              if (yydebug != null) yydebug.reject();
              throw new yyParser.yyException("irrecoverable syntax error");
  
            case 3:
              if (yyToken == 0) {
                if (yydebug != null) yydebug.reject();
                throw new yyParser.yyException("irrecoverable syntax error at end-of-file");
              }
              if (yydebug != null)
                yydebug.discard(yyState, yyToken, yyname(yyToken),
  							yyLex.value());
              yyToken = -1;
              goto yyDiscarded;		// leave stack alone
            }
        }
        int yyV = yyTop + 1-yyLen[yyN];
        if (yydebug != null)
          yydebug.reduce(yyState, yyStates[yyV-1], yyN, yyRule[yyN], yyLen[yyN]);
        yyVal = yyDefault(yyV > yyTop ? null : yyVals[yyV]);
        switch (yyN) {
case 1:
#line 296 "mb-parser.jay"
  {
		yyVal = yyVals[-1+yyTop];
	  }
  break;
case 3:
#line 304 "mb-parser.jay"
  { 
	    yyVal = ((yyVals[-2+yyTop]).ToString ()) + "." + (yyVals[0+yyTop].ToString ()); 
	  }
  break;
case 9:
#line 326 "mb-parser.jay"
  {
		current_namespace.Using ((string) yyVals[-1+yyTop]);
	  }
  break;
case 11:
#line 337 "mb-parser.jay"
  {
		current_namespace = new Namespace (current_namespace, (string) yyVals[-1+yyTop]); 
	  }
  break;
case 12:
#line 343 "mb-parser.jay"
  { 
		current_namespace = current_namespace.Parent;
	  }
  break;
case 19:
#line 367 "mb-parser.jay"
  {
		int mod_flags = 0;
		string name = "";

		if (yyVals[0+yyTop] is Class){
			Class c = (Class) yyVals[0+yyTop];
			mod_flags = c.ModFlags;
			name = c.Name;
		} else if (yyVals[0+yyTop] is Struct){
			Struct s = (Struct) yyVals[0+yyTop];
			mod_flags = s.ModFlags;
			name = s.Name;
		} else
			break;

	  }
  break;
case 21:
#line 394 "mb-parser.jay"
  { 
	  }
  break;
case 22:
#line 398 "mb-parser.jay"
  {
	  }
  break;
#line 419 "-"
        }
        yyTop -= yyLen[yyN];
        yyState = yyStates[yyTop];
        int yyM = yyLhs[yyN];
        if (yyState == 0 && yyM == 0) {
          if (yydebug != null) yydebug.shift(0, yyFinal);
          yyState = yyFinal;
          if (yyToken < 0) {
            yyToken = yyLex.advance() ? yyLex.token() : 0;
            if (yydebug != null)
               yydebug.lex(yyState, yyToken,yyname(yyToken), yyLex.value());
          }
          if (yyToken == 0) {
            if (yydebug != null) yydebug.accept(yyVal);
            return yyVal;
          }
          goto yyLoop;
        }
        if (((yyN = yyGindex[yyM]) != 0) && ((yyN += yyState) >= 0)
            && (yyN < yyTable.Length) && (yyCheck[yyN] == yyState))
          yyState = yyTable[yyN];
        else
          yyState = yyDgoto[yyM];
        if (yydebug != null) yydebug.shift(yyStates[yyTop], yyState);
	 goto yyLoop;
      }
    }
  }

   static  short [] yyLhs  = {              -1,
    0,    4,    4,    1,    1,    5,    5,    6,    7,    2,
   10,    9,    8,    3,    3,   11,   11,   12,   12,   13,
   16,   14,   15,   15,   17,   17,   18,   18,   19,   21,
   21,   20,   20,   22,   22,   22,
  };
   static  short [] yyLen = {           2,
    4,    1,    3,    0,    1,    1,    2,    1,    3,    0,
    0,    9,    1,    0,    1,    1,    2,    1,    1,    1,
    0,    8,    0,    1,    1,    2,    1,    1,   10,    0,
    5,    0,    3,    0,    1,    1,
  };
   static  short [] yyDefRed = {            0,
    0,    0,   10,    0,    6,    8,    2,    0,    0,    0,
    7,    0,    9,    0,    0,    0,   18,    0,   16,   19,
   20,    3,    0,    0,    1,   17,   21,   11,    0,    0,
    0,   27,    0,    0,   25,   28,    0,    0,    0,   26,
    0,    0,    0,    0,    0,    0,   22,    0,    0,    0,
   12,    0,    0,    0,    0,    0,    0,   36,    0,    0,
    0,    0,   29,   31,
  };
  protected static  short [] yyDgoto  = {             2,
    3,   10,   16,    8,    4,    5,    6,    9,   17,   30,
   18,   19,   20,   21,   33,   29,   34,   35,   36,   46,
   55,   60,
  };
  protected static  short [] yySindex = {         -303,
 -440,    0,    0, -303,    0,    0,    0, -384, -286, -278,
    0, -435,    0, -434, -440, -234,    0, -278,    0,    0,
    0,    0, -280, -305,    0,    0,    0,    0, -282, -303,
 -440,    0, -277, -282,    0,    0, -278, -391, -256,    0,
 -275, -440, -274, -306, -266, -368,    0, -270, -440, -269,
    0, -384, -440, -390, -265, -439, -343,    0, -384, -362,
 -263, -262,    0,    0,
  };
  protected static  short [] yyRindex = {         -253,
    0,    0,    0, -254,    0,    0,    0, -286,    0, -216,
    0,    0,    0,    0,    0,    0,    0, -250,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0, -259, -279,
    0,    0,    0, -257,    0,    0, -252,    0,    0,    0,
    0, -354,    0,    0,    0,    0,    0,    0,    0,    0,
    0, -351, -251,    0,    0, -347,    0,    0, -346,    0,
    0,    0,    0,    0,
  };
  protected static  short [] yyGindex = {            0,
   27,    0,   21,   -7,    0,   55,    0,    0,    0,    0,
    0,   44,  -20,    0,    0,    0,    0,   29,    0,    0,
    0,    0,
  };
  protected static  short [] yyTable = {            49,
   28,   14,    5,    4,    4,   14,   15,   24,   32,   42,
   56,   12,   12,   32,   58,    7,    7,    1,   12,   13,
   22,   23,   25,   38,    4,   27,   39,   43,   44,    5,
    4,   47,   48,   50,   45,   51,   53,   61,   57,   62,
   14,   52,   63,   64,   23,   54,   24,   32,   59,    5,
   33,   14,   30,   15,   34,   35,   37,   41,   11,    4,
   15,   26,   40,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    5,    4,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,   12,   31,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,   12,
  };
  protected static  short [] yyCheck = {           266,
  306,  284,  257,  257,  284,  284,  257,   15,   29,  401,
  401,  403,  403,   34,  454,  456,  456,  321,  403,  306,
  456,  456,  257,   31,  304,  306,  304,  284,  304,  284,
  284,  306,  339,  402,   42,  306,  306,  381,  304,  402,
  257,   49,  306,  306,  304,   53,  304,  402,   56,  304,
  402,  304,  304,  304,  402,  402,   30,   37,    4,  339,
  339,   18,   34,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  339,  339,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  403,  381,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  403,
  };

#line 439 "mb-parser.jay"



Tokenizer lexer;

public Tokenizer Lexer {
	get {
		return lexer;
	}
}		   

public override int parse ()
{
	current_namespace = new Namespace (null, "");
	current_container = RootContext.Tree.Types;
	current_container.Namespace = current_namespace;

	UseExtendedSyntax = name.EndsWith(".mbs");

	lexer = new Tokenizer (input, name, defines);
	StringBuilder value = new StringBuilder ();

	global_errors = 0;
	try 
	{
		if (yacc_verbose_flag)
			yyparse (lexer, new yydebug.yyDebugSimple ());
		else
			yyparse (lexer);
	} 
	catch (Exception e)
	{
		Console.WriteLine (lexer.location + "  : Parsing error ");
		Console.WriteLine (e);
		global_errors++;
	}

	return global_errors;
}

/* end end end */
}


#line 576 "-"
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
  public const int EOF = 257;
  public const int NONE = 258;
  public const int ERROR = 259;
  public const int ADDHANDLER = 260;
  public const int ADDRESSOF = 261;
  public const int ALIAS = 262;
  public const int AND = 263;
  public const int ANDALSO = 264;
  public const int ANSI = 265;
  public const int AS = 266;
  public const int ASSEMBLY = 267;
  public const int AUTO = 268;
  public const int BOOLEAN = 269;
  public const int BYREF = 270;
  public const int BYTE = 271;
  public const int BYVAL = 272;
  public const int CALL = 273;
  public const int CASE = 274;
  public const int CATCH = 275;
  public const int CBOOL = 276;
  public const int CBYTE = 277;
  public const int CCHAR = 278;
  public const int CDATE = 279;
  public const int CDEC = 280;
  public const int CDBL = 281;
  public const int CHAR = 282;
  public const int CINT = 283;
  public const int CLASS = 284;
  public const int CLNG = 285;
  public const int COBJ = 286;
  public const int CONST = 287;
  public const int CSHORT = 288;
  public const int CSNG = 289;
  public const int CSTR = 290;
  public const int CTYPE = 291;
  public const int DATE = 292;
  public const int DECIMAL = 293;
  public const int DECLARE = 294;
  public const int DEFAULT = 295;
  public const int DELEGATE = 296;
  public const int DESCRIPTION = 297;
  public const int DIM = 298;
  public const int DO = 299;
  public const int DOUBLE = 300;
  public const int EACH = 301;
  public const int ELSE = 302;
  public const int ELSEIF = 303;
  public const int END = 304;
  public const int ENUM = 305;
  public const int EOL = 306;
  public const int ERASE = 307;
  public const int EVENT = 308;
  public const int EXIT = 309;
  public const int FALSE = 310;
  public const int FINALLY = 311;
  public const int FOR = 312;
  public const int FRIEND = 313;
  public const int FUNCTION = 314;
  public const int GET = 315;
  public const int GETTYPE = 316;
  public const int GOTO = 317;
  public const int HANDLES = 318;
  public const int IF = 319;
  public const int IMPLEMENTS = 320;
  public const int IMPORTS = 321;
  public const int IN = 322;
  public const int INHERITS = 323;
  public const int INTEGER = 324;
  public const int INTERFACE = 325;
  public const int IS = 326;
  public const int LET = 327;
  public const int LIB = 328;
  public const int LIKE = 329;
  public const int LONG = 330;
  public const int LOOP = 331;
  public const int ME = 332;
  public const int MOD = 333;
  public const int MODULE = 334;
  public const int MUSTINHERIT = 335;
  public const int MUSTOVERRIDE = 336;
  public const int MYBASE = 337;
  public const int MYCLASS = 338;
  public const int NAMESPACE = 339;
  public const int NEW = 340;
  public const int NEXT = 341;
  public const int NOT = 342;
  public const int NOTHING = 343;
  public const int NOTINHERITABLE = 344;
  public const int NOTOVERRIDABLE = 345;
  public const int OBJECT = 346;
  public const int ON = 347;
  public const int OPTION = 348;
  public const int OPTIONAL = 349;
  public const int OR = 350;
  public const int ORELSE = 351;
  public const int OVERLOADS = 352;
  public const int OVERRIDABLE = 353;
  public const int OVERRIDES = 354;
  public const int PARAMETER = 355;
  public const int PARAM_ARRAY = 356;
  public const int PRESERVE = 357;
  public const int PRIVATE = 358;
  public const int PROPERTY = 359;
  public const int PROTECTED = 360;
  public const int PUBLIC = 361;
  public const int RAISEEVENT = 362;
  public const int READONLY = 363;
  public const int REDIM = 364;
  public const int REM = 365;
  public const int REMOVEHANDLER = 366;
  public const int RESUME = 367;
  public const int RETURN = 368;
  public const int SELECT = 369;
  public const int SET = 370;
  public const int SHADOWS = 371;
  public const int SHARED = 372;
  public const int SHORT = 373;
  public const int SINGLE = 374;
  public const int SIZEOF = 375;
  public const int STATIC = 376;
  public const int STEP = 377;
  public const int STOP = 378;
  public const int STRING = 379;
  public const int STRUCTURE = 380;
  public const int SUB = 381;
  public const int SUMMARY = 382;
  public const int SYNCLOCK = 383;
  public const int THEN = 384;
  public const int THROW = 385;
  public const int TO = 386;
  public const int TRUE = 387;
  public const int TRY = 388;
  public const int TYPEOF = 389;
  public const int UNICODE = 390;
  public const int UNTIL = 391;
  public const int VARIANT = 392;
  public const int WHEN = 393;
  public const int WHILE = 394;
  public const int WITH = 395;
  public const int WITHEVENTS = 396;
  public const int WRITEONLY = 397;
  public const int XOR = 398;
  public const int OPEN_BRACKET = 399;
  public const int CLOSE_BRACKET = 400;
  public const int OPEN_PARENS = 401;
  public const int CLOSE_PARENS = 402;
  public const int DOT = 403;
  public const int COMMA = 404;
  public const int COLON = 405;
  public const int PLUS = 406;
  public const int MINUS = 407;
  public const int ASSIGN = 408;
  public const int OP_LT = 409;
  public const int OP_GT = 410;
  public const int STAR = 411;
  public const int PERCENT = 412;
  public const int DIV = 413;
  public const int OP_EXP = 414;
  public const int INTERR = 415;
  public const int OP_IDIV = 416;
  public const int OP_CONCAT = 417;
  public const int OP_LE = 418;
  public const int OP_GE = 420;
  public const int OP_EQ = 422;
  public const int OP_NE = 424;
  public const int OP_AND = 426;
  public const int OP_OR = 427;
  public const int OP_XOR = 428;
  public const int OP_MODULUS = 429;
  public const int OP_MULT_ASSIGN = 430;
  public const int OP_DIV_ASSIGN = 432;
  public const int OP_IDIV_ASSIGN = 434;
  public const int OP_ADD_ASSIGN = 436;
  public const int OP_SUB_ASSIGN = 438;
  public const int OP_CONCAT_ASSIGN = 440;
  public const int OP_EXP_ASSIGN = 442;
  public const int LITERAL_INTEGER = 444;
  public const int LITERAL_SINGLE = 446;
  public const int LITERAL_DOUBLE = 448;
  public const int LITERAL_DECIMAL = 450;
  public const int LITERAL_CHARACTER = 452;
  public const int LITERAL_STRING = 454;
  public const int IDENTIFIER = 456;
  public const int LOWPREC = 457;
  public const int BITWISE_OR = 458;
  public const int BITWISE_AND = 459;
  public const int OP_SHIFT_LEFT = 460;
  public const int OP_SHIFT_RIGHT = 461;
  public const int BITWISE_NOT = 462;
  public const int CARRET = 463;
  public const int UMINUS = 464;
  public const int OP_INC = 465;
  public const int OP_DEC = 466;
  public const int OPEN_BRACE = 467;
  public const int HIGHPREC = 468;
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
