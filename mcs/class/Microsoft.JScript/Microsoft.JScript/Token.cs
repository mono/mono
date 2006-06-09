//
// Token.cs: C# Port of Mozilla's Rhino Token.java
//	     Implement's the JScript scanner

/*
 *
 * The contents of this file are subject to the Netscape Public
 * License Version 1.1 (the "License"); you may not use this file
 * except in compliance with the License. You may obtain a copy of
 * the License at http://www.mozilla.org/NPL/
 *
 * Software distributed under the License is distributed on an "AS
 * IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or
 * implied. See the License for the specific language governing
 * rights and limitations under the License.
 *
 * The Original Code is Rhino code, released
 * May 6, 1999.
 *
 * The Initial Developer of the Original Code is Netscape
 * Communications Corporation.  Portions created by Netscape are
 * Copyright (C) 1997-1999 Netscape Communications Corporation. All
 * Rights Reserved.
 *
 * Contributor(s):
 * Roger Lawrence
 * Mike McCabe
 * Igor Bukanov
 * Milen Nankov
 *
 * Alternatively, the contents of this file may be used under the
 * terms of the GNU Public License (the "GPL"), in which case the
 * provisions of the GPL are applicable instead of those above.
 * If you wish to allow use of your version of this file only
 * under the terms of the GPL and not to allow others to use your
 * version of this file under the NPL, indicate your decision by
 * deleting the provisions above and replace them with the notice
 * and other provisions required by the GPL.  If you do not delete
 * the provisions above, a recipient may use your version of this
 * file under either the NPL or the GPL.
 */

//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2004, Cesar Lopez Nataren
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;

namespace Microsoft.JScript {
internal class Token  {

	internal static bool PrintNames = true;

	internal static int
		ERROR = -1,
		EOF = 0,
		EOL = 1,
		
		FIRST_BYTECODE_TOKEN = 2,
	
		ENTERWITH = 3,
		LEAVEWITH = 4,
		RETURN = 5,
		GOTO = 6,
		IFEQ = 7,
		IFNE = 8, 
		SETNAME = 9,
		BITOR = 10,
		BITXOR = 11,
		BITAND = 12,
		EQ = 13,
		NE = 14, 
		LT = 15,
		LE = 16,
		GT = 17,
		GE = 18,
		LSH = 19,
		RSH = 20,
		URSH = 21,
		ADD = 22,
		SUB = 23,
		MUL = 24,
		DIV = 25,
		MOD = 26,
		NOT = 27,
		BITNOT = 28,
		POS = 29,
		NEG = 30,
		NEW = 31,
		DELPROP = 32,
		TYPEOF = 33,
		GETPROP = 34,
		SETPROP= 35,
		GETELEM     = 36,
		SETELEM     = 37,
		CALL        = 38,
		NAME        = 39,
		NUMBER      = 40,
		STRING      = 41,
		ZERO        = 42,
		ONE         = 43,
		NULL        = 44,
		THIS        = 45,
		FALSE       = 46,
		TRUE        = 47,
		SHEQ        = 48,   // shallow equality (===)
		SHNE        = 49,   // shallow inequality (!==)
		REGEXP      = 50,

		// XXX removed unused POP
		BINDNAME    = 52,
		THROW       = 53,
		IN          = 54,
		INSTANCEOF  = 55,
		LOCAL_SAVE  = 56,
		LOCAL_LOAD  = 57,
		GETVAR      = 58,
		SETVAR      = 59,

		// XXX removed unused UNDEFINED
		CATCH_SCOPE = 61,
		ENUM_INIT   = 62,
		ENUM_NEXT   = 63,
		ENUM_ID     = 64,
		THISFN      = 65,
		RETURN_RESULT = 66, // to return result stored as popv in functions
		ARRAYLIT    = 67, // array literal
		OBJECTLIT   = 68, // object literal
		GET_REF     = 69, // *reference
		SET_REF     = 70, // *reference = something
		REF_CALL    = 71, // f(args) = something or f(args)++
		SPECIAL_REF = 72, // reference for special properties like __proto
		GENERIC_REF = 73, // generic reference to generate runtime ref errors

		LAST_BYTECODE_TOKEN = 73,
		// End of interpreter bytecodes

		TRY         = 74,
		SEMI        = 75,  // semicolon
		LB          = 76,  // left and right brackets
		RB          = 77,
		LC          = 78,  // left and right curlies (braces)
		RC          = 79,
		LP          = 80,  // left and right parentheses
		RP          = 81,
		COMMA       = 82,  // comma operator
		ASSIGN      = 83, // simple assignment  (=)
		ASSIGNOP    = 84, // assignment with operation (+= -= etc.)
		HOOK        = 85, // conditional (?:)
		COLON       = 86,
		OR          = 87, // logical or (||)
		AND         = 88, // logical and (&&)
		INC         = 89, // increment/decrement (++ --)
		DEC         = 90,
		DOT         = 91, // member operator (.)
		FUNCTION    = 92, // function keyword
		EXPORT      = 93, // export keyword
		IMPORT      = 94, // import keyword
		IF          = 95, // if keyword
		ELSE        = 96, // else keyword
		SWITCH      = 97, // switch keyword
		CASE        = 98, // case keyword
		DEFAULT     = 99, // default keyword
		WHILE       = 100, // while keyword
		DO          = 101, // do keyword
		FOR         = 102, // for keyword
		BREAK       = 103, // break keyword
		CONTINUE    = 104, // continue keyword
		VAR         = 105, // var keyword
		WITH        = 106, // with keyword
		CATCH       = 107, // catch keyword
		FINALLY     = 108, // finally keyword
		VOID        = 109, // void keyword
		RESERVED    = 110, // reserved keywords

		EMPTY       = 111,

		/* types used for the parse tree - these never get returned
		 * by the scanner.
		 */

		BLOCK       = 112, // statement block
		LABEL       = 113, // label
		TARGET      = 114,
		LOOP        = 115,
		EXPR_VOID   = 116,
		EXPR_RESULT = 117,
		JSR         = 118,
		SCRIPT      = 119,   // top-level node for entire script
		TYPEOFNAME  = 120,  // for typeof(simple-name)
		USE_STACK   = 121,
		SETPROP_OP  = 122, // x.y op= something
		SETELEM_OP  = 123, // x[y] op= something
		// XXX removed unused INIT_LIST
		LOCAL_BLOCK = 125,
		SET_REF_OP  = 126, // *reference op= something

		LAST_TOKEN  = 126;

	public static string Name (int token, bool ignore_error)
	{
		if (!(-1 <= token && token <= LAST_TOKEN)) {
			if (ignore_error)
				return null;
			else
				throw new Exception ("Invalid argument = " + token.ToString ());
		}

		if (token == ERROR)
			return "ERROR";
		else if (token == EOF)
			return "EOF";
		else if (token == EOL)
			return "EOL";
		else if (token == ENTERWITH)
			return "ENTERWITH";
		else if (token == LEAVEWITH)
			return "LEAVEWITH";
		else if (token ==  RETURN)
			return "RETURN";
		else if (token ==  GOTO)
			return "GOTO";
		else if (token ==  IFEQ)
			return "IFEQ";
		else if (token ==  IFNE)
			return "IFNE";
		else if (token ==  SETNAME)
			return "SETNAME";
		else if (token ==  BITOR)
			return "BITOR";
		else if (token ==  BITXOR)
			return "BITXOR";
		else if (token ==  BITAND)
			return "BITAND";
		else if (token ==  EQ)
			return "EQ";
		else if (token ==  NE)
			return "NE";
		else if (token ==  LT)
			return "LT";
		else if (token ==  LE)
			return "LE";
		else if (token ==  GT)
			return "GT";
		else if (token ==  GE)
			return "GE";
		else if (token ==  LSH)
			return "LSH";
		else if (token ==  RSH)
			return "RSH";
		else if (token ==  URSH)
			return "URSH";
		else if (token ==  ADD)
			return "ADD";
		else if (token ==  SUB)
			return "SUB";
		else if (token ==  MUL)
			return "MUL";
		else if (token ==  DIV)
			return "DIV";
		else if (token ==  MOD)
			return "MOD";
		else if (token ==  NOT)
			return "NOT";
		else if (token ==  BITNOT)
			return "BITNOT";
		else if (token ==  POS)
			return "POS";
		else if (token ==  NEG)
			return "NEG";
		else if (token ==  NEW)
			return "NEW";
		else if (token ==  DELPROP)
			return "DELPROP";
		else if (token ==  TYPEOF)
			return "TYPEOF";
		else if (token ==  GETPROP)
			return "GETPROP";
		else if (token ==  SETPROP)
			return "SETPROP";
		else if (token ==  GETELEM)
			return "GETELEM";
		else if (token ==  SETELEM)
			return "SETELEM";
		else if (token ==  CALL)
			return "CALL";
		else if (token ==  NAME)
			return "NAME";
		else if (token ==  NUMBER)
			return "NUMBER";
		else if (token ==  STRING)
			return "STRING";
		else if (token ==  ZERO)
			return "ZERO";
		else if (token ==  ONE)
			return "ONE";
		else if (token ==  NULL)
			return "NULL";
		else if (token ==  THIS)
			return "THIS";
		else if (token ==  FALSE)
			return "FALSE";
		else if (token ==  TRUE)
			return "TRUE";
		else if (token ==  SHEQ)
			return "SHEQ";
		else if (token ==  SHNE)
			return "SHNE";
		else if (token ==  REGEXP)
			return "OBJECT";
		else if (token ==  BINDNAME)
			return "BINDNAME";
		else if (token ==  THROW)
			return "THROW";
		else if (token ==  IN)
			return "IN";
		else if (token ==  INSTANCEOF)
			return "INSTANCEOF";
		else if (token ==  LOCAL_SAVE)
			return "LOCAL_SAVE";
		else if (token ==  LOCAL_LOAD)
			return "LOCAL_LOAD";
		else if (token ==  GETVAR)
			return "GETVAR";
		else if (token ==  SETVAR)
			return "SETVAR";
		else if (token ==  CATCH_SCOPE)
			return "CATCH_SCOPE";
		else if (token ==  ENUM_INIT)
			return "ENUM_INIT";
		else if (token ==  ENUM_NEXT)
			return "ENUM_NEXT";
		else if (token ==  ENUM_ID)
			return "ENUM_ID";
		else if (token ==  THISFN)
			return "THISFN";
		else if (token ==  RETURN_RESULT)
			return "RETURN_RESULT";
		else if (token ==  ARRAYLIT)
			return "ARRAYLIT";
		else if (token ==  OBJECTLIT)
			return "OBJECTLIT";
		else if (token ==  GET_REF)
			return "GET_REF";
		else if (token ==  SET_REF)
			return "SET_REF";
		else if (token ==  REF_CALL)
			return "REF_CALL";
		else if (token ==  SPECIAL_REF)
			return "SPECIAL_REF";
		else if (token ==  GENERIC_REF)
			return "GENERIC_REF";
		else if (token ==  TRY)
			return "TRY";
		else if (token ==  SEMI)
			return "SEMI";
		else if (token ==  LB)
			return "LB";
		else if (token ==  RB)
			return "RB";
		else if (token ==  LC)
			return "LC";
		else if (token ==  RC)
			return "RC";
		else if (token ==  LP)
			return "LP";
		else if (token ==  RP)
			return "RP";
		else if (token ==  COMMA)
			return "COMMA";
		else if (token ==  ASSIGN)
			return "ASSIGN";
		else if (token ==  ASSIGNOP)
			return "ASSIGNOP";
		else if (token ==  HOOK)
			return "HOOK";
		else if (token ==  COLON)
			return "COLON";
		else if (token ==  OR)
			return "OR";
		else if (token ==  AND)
			return "AND";
		else if (token ==  INC)
			return "INC";
		else if (token ==  DEC)
			return "DEC";
		else if (token ==  DOT)
			return "DOT";
		else if (token ==  FUNCTION)
			return "FUNCTION";
		else if (token ==  EXPORT)
			return "EXPORT";
		else if (token ==  IMPORT)
			return "IMPORT";
		else if (token ==  IF)
			return "IF";
		else if (token ==  ELSE)
			return "ELSE";
		else if (token ==  SWITCH)
			return "SWITCH";
		else if (token ==  CASE)
			return "CASE";
		else if (token ==  DEFAULT)
			return "DEFAULT";
		else if (token ==  WHILE)
			return "WHILE";
		else if (token ==  DO)
			return "DO";
		else if (token ==  FOR)
			return "FOR";
		else if (token ==  BREAK)
			return "BREAK";
		else if (token ==  CONTINUE)
			return "CONTINUE";
		else if (token ==  VAR)
			return "VAR";
		else if (token ==  WITH)
			return "WITH";
		else if (token ==  CATCH)
			return "CATCH";
		else if (token ==  FINALLY)
			return "FINALLY";
		else if (token ==  RESERVED)
			return "RESERVED";
		else if (token ==  EMPTY)
			return "EMPTY";
		else if (token ==  BLOCK)
			return "BLOCK";
		else if (token ==  LABEL)
			return "LABEL";
		else if (token ==  TARGET)
			return "TARGET";
		else if (token ==  LOOP)
			return "LOOP";
		else if (token ==  EXPR_VOID)
			return "EXPR_VOID";
		else if (token ==  EXPR_RESULT)
			return "EXPR_RESULT";
		else if (token ==  JSR)
			return "JSR";
		else if (token ==  SCRIPT)
			return "SCRIPT";
		else if (token ==  TYPEOFNAME)
			return "TYPEOFNAME";
		else if (token ==  USE_STACK)
			return "USE_STACK";
		else if (token ==  SETPROP_OP)
			return "SETPROP_OP";
		else if (token ==  SETELEM_OP)
			return "SETELEM_OP";
		else if (token ==  LOCAL_BLOCK)
			return "LOCAL_BLOCK";
		else if (token ==  SET_REF_OP)
			return "SET_REF_OP";

		// Token without name
		if (ignore_error)
			return null;
		else
			throw new Exception("Illegal state, " + token.ToString ());
	}
}
}
