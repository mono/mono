//
// jscript-lexer-parser.g: EcmaScript Grammar written on antlr.
//
// Author:
//	 Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

options {
	language = "CSharp";
	namespace = "Microsoft.JScript";
}

//
// Parser
//

class JScriptParser extends Parser;
options {
	defaultErrorHandler = false;
}

program
	: source_elements
	;

source_elements
	: (source_element)+
	;

source_element
	: statement
	| function_decl_or_expr
	;

function_decl_or_expr
	: "function" (IDENTIFIER | ) 
	  OPEN_PARENS formal_param_list CLOSE_PARENS 
	  OPEN_BRACE function_body CLOSE_BRACE
	;

function_body
	: source_elements	
	;

formal_param_list
	: (IDENTIFIER | ) (COMMA IDENTIFIER)*
	;

//
// Statements
//

statement
	: expr_stm
	| var_stm
	| empty_stm
	| if_stm
	| iteration_stm
	| continue_stm
	| break_stm
	| return_stm
	| with_stm
	| switch_stm
	| throw_stm
	| try_stm
	;

block
	: OPEN_BRACE (statement)* CLOSE_BRACE
	;

try_stm
	: "try" block
	  ((catch_exp (finally_exp | ) | ) | finally_exp)
	;

catch_exp
	: "catch" OPEN_PARENS IDENTIFIER CLOSE_PARENS block
	;

finally_exp
	: "finally" block
	;

throw_stm
	: "throw" expr SEMI_COLON
	;

switch_stm
	: "switch"  OPEN_PARENS expr CLOSE_PARENS case_block
	;

case_block
	: OPEN_BRACE case_clauses  default_clause case_clauses CLOSE_BRACE
	;

default_clause
	: "default" COLON statement_list
	;

case_clauses
	: (case_clause)*
	;

case_clause
	: "case" expr COLON statement_list
	;

with_stm
	: "with" OPEN_PARENS expr CLOSE_PARENS statement
	;

return_stm
	: "return" (expr | ) SEMI_COLON
	;

break_stm
	: "break" (IDENTIFIER | ) SEMI_COLON
	;
	
continue_stm
	: "continue" (IDENTIFIER | ) SEMI_COLON
	;

iteration_stm
	: "do" statement "while" OPEN_PARENS expr CLOSE_PARENS SEMI_COLON
	| "while" OPEN_PARENS expr CLOSE_PARENS statement
	| "for" OPEN_PARENS inside_for CLOSE_PARENS statement
	;

inside_for
	// We must check the NoIn restriction
	: (expr | ) SEMI_COLON (expr | ) SEMI_COLON (expr | )
	// We must keep a counter c, c tells us how many decls are
	// done, in order to interrupt if c > 1 and we are inside a "in"
	| "var" (var_decl_list 
		  ( SEMI_COLON (expr | ) SEMI_COLON (expr | )
		  | "in" expr))
	// FIXME: left_hand_side_expr in exp rule, missing
	;

if_stm
	: "if" OPEN_PARENS expr CLOSE_PARENS statement 
	  (("else")=> "else"  statement)?
	;

empty_stm
	: SEMI_COLON
	;

var_stm
	: "var" var_decl_list SEMI_COLON
	;

var_decl_list
	: var_decl (COMMA var_decl)*
	;
	

var_decl
	: IDENTIFIER (initializer | )
	;

initializer
	: ASSIGN assignment_expr
	;

expr_stm
	: expr
   	;


statement_list
	: (statement)*
	;

expr
	: assignment_expr (COMMA assignment_expr)*
	;

assignment_expr
	: ((left_hand_side_expr assignment_op)=> left_hand_side_expr assignment_op assignment_expr
	| cond_expr)
	;

member_expr
	: primary_expr member_aux
	| "new" member_expr OPEN_PARENS (arguments_list | ) CLOSE_PARENS
	;

member_aux
	: ( "." IDENTIFIER member_aux 
	  | (OPEN_BRACKET)=> OPEN_BRACKET expr CLOSE_BRACKET
	  | 
	  )
	;


call_expr
	: member_expr call_aux;

call_aux
	: (("(" (arguments_list | ) ")"
	   | "[" expr "]"
	   | DOT IDENTIFIER
	   ) call_aux 
	  |
          )
	;

arguments_list
	: assignment_expr (COMMA assignment_expr)*
	;

left_hand_side_expr
	: call_expr
	;

postfix_expr
	: left_hand_side_expr ("++" | "--" | )
	;

unary_expr
	: postfix_expr
	| unary_op unary_expr
	;

unary_op
	: "delete"
	| "void"
	| "typeof"
	| INCREMENT
	| DECREMENT
	| PLUS
	| MINUS
	| BITWISE_NOT
	| LOGICAL_NOT 
	;

multiplicative_expr
	: unary_expr multiplicative_aux
	;

multiplicative_aux
	: ((MULT | DIVISION | MODULE) unary_expr multiplicative_aux | )
	;

additive_expr
	: multiplicative_expr additive_aux
	;

additive_aux
	: ((PLUS | MINUS) multiplicative_expr additive_aux | )
	;

shift_expr
	: additive_expr shift_aux
	;

shift_aux
	: (shift_op additive_expr shift_aux | )
	;

shift_op
	: SHIFT_LEFT
	| SHIFT_RIGHT
	| UNSIGNED_SHIFT_RIGHT
	;

relational_expr
	: shift_expr relational_aux
	;

relational_aux
	: (relational_op shift_expr relational_aux | )
	;

relational_op
	: LESS_THAN
	| GREATER_THAN
	| LESS_EQ
	| GREATER_EQ
	| "instanceof"
	;


equality_expr
	: relational_expr equality_aux
	;

equality_aux
	: (equality_op relational_expr equality_aux | )
	;

equality_op
	: EQ
	| NEQ
	| STRICT_EQ
	| STRICT_NEQ
	;

bitwise_and_expr
	: equality_expr bitwise_and_aux
	;

bitwise_and_aux
	: (BITWISE_AND equality_expr bitwise_and_aux | )
	;

bitwise_xor_expr
	: bitwise_and_expr bitwise_xor_aux
	;

bitwise_xor_aux
	: (BITWISE_XOR bitwise_and_expr bitwise_xor_aux | )
	;

bitwise_or_expr
	: bitwise_xor_expr bitwise_or_aux
	;

bitwise_or_aux
	: (BITWISE_OR bitwise_xor_expr bitwise_or_aux | )
	;

logical_and_expr
	: bitwise_or_expr logical_and_aux
	;

logical_and_aux
	: (LOGICAL_AND bitwise_or_expr logical_and_aux | )
	;

logical_or_expr
	:  logical_and_expr logical_or_aux
	;

logical_or_aux
	: (LOGICAL_OR logical_and_expr logical_or_aux | )
	;

cond_expr
	: logical_or_expr (INTERR assignment_expr COLON assignment_expr | )
	;

assignment_op
	: ASSIGN
	| MULT_ASSIGN
	| DIV_ASSIGN
	| MOD_ASSIGN
	| ADD_ASSIGN
	| SUB_ASSIGN
	| SHIFT_LEFT_ASSIGN
	| SHIFT_RIGHT_ASSIGN
	| AND_ASSIGN
	| XOR_ASSIGN
	| OR_ASSIGN
	;
	

primary_expr
	:"this"
	| object_literal
	| IDENTIFIER
	| literal
	| array_literal
	| OPEN_PARENS expr CLOSE_PARENS
	; 

object_literal
	: OPEN_BRACE 
	   ((property_name COLON)=> OPEN_BRACE (property_name COLON assignment_expr)+
	   | (statement)*  // block_stm case
	   ) CLOSE_BRACE

	;

literal
	: "null"
	| "true"
	| "false"
	| STRING_LITERAL
	| numeric_literal
	;

property_name_and_value_list
	: (property_name COLON primary_expr)+
	;

property_name
	: (IDENTIFIER | STRING_LITERAL | numeric_literal) 
	;

array_literal
	: OPEN_BRACKET (primary_expr (COMMA primary_expr)* | ) CLOSE_BRACKET
	;	

numeric_literal
	: DECIMAL_LITERAL
	| HEX_INTEGER_LITERAL
	;


line_terminator
	: LINE_FEED
	| CARRIAGE_RETURN
	| LINE_SEPARATOR
	| PARAGRAPH_SEPARATOR
	;

white_space
	: TAB
	| VERTICAL_TAB
	| FORM_FEED
	| SPACE
	| NO_BREAK_SPACE
	;

//
// Lexer
//

class JScriptLexer extends Lexer;

options {
	charVocabulary = '\u0000'..'\uFFFE';
	testLiterals = false;
	k = 3;
}

DECIMAL_LITERAL: ('0'  | ('1'..'9')('0'..'9')*) (DOT ('0'..'9')* | ) (('e' | 'E') (('+' | '-' | ) ('0'..'9')+) | )
    ;

HEX_INTEGER_LITERAL: '0' ('x' | 'X') ('0'..'9' | 'a'..'f' | 'A'..'F')+
    ;

STRING_LITERAL
	: '"' (~('"' | '\\' | '\u000A' | '\u000D' | '\u2028' | '\u2029'))* '"'
	;

IDENTIFIER
options { testLiterals = true; }
	: ('a'..'z' | 'A'..'Z') ('a'..'z' | 'A'..'Z' | '0'..'9')*
	;

//
// Operators
//
DOT: '.';
COMMA: ',';

INCREMENT: "++";
DECREMENT: "--";
PLUS: '+';
MINUS: '-';
BITWISE_NOT: '~';
LOGICAL_NOT: '!';

MULT: '*';
DIVISION: '/';
MODULE: '%';
ASSIGN: '=';


SHIFT_LEFT: "<<";
SHIFT_RIGHT: ">>";
UNSIGNED_SHIFT_RIGHT: ">>>";
	
LESS_THAN: '<';
GREATER_THAN: '>';
LESS_EQ: "<=";
GREATER_EQ: ">=";

EQ: "==";
NEQ: "!=";
STRICT_EQ: "===";
STRICT_NEW: "!==";

MULT_ASSIGN: "*=";
DIV_ASSIGN: "/=";
MOD_ASSIGN: "%=";
ADD_ASSIGN: "+=";
SUB_ASSIGN: "-=";
SHIFT_LEFT_ASSIGN: "<<=";
SHIFT_RIGHT_ASSIGN: ">>=";
AND_ASSIGN: "&=";
XOR_ASSIGN: "^=";
OR_ASSIGN: "|=";
INTERR: '?';
LOGICAL_OR: "||";
LOGICAL_AND: "&&";
BITWISE_OR: '|';
BITWISE_AND: '&';
BITWISE_XOR: '^';


OPEN_PARENS: '(';
CLOSE_PARENS: ')';
OPEN_BRACKET: '[';
CLOSE_BRACKET: ']';
OPEN_BRACE: '{';
CLOSE_BRACE: '}';



//
// Punctuators
//
SEMI_COLON: ';';
COLON: ':';


//
// Comments
//

SL_COMMENT
	: "//" (~('\u000A' | '\u000D' | '\u2028' | '\u2029'))*
	  { $setType (Token.SKIP); }
	;
//
// Line terminator tokens
//
LINE_FEED
	: '\u000A' { $setType (Token.SKIP); newline (); }
	;

CARRIAGE_RETURN
	: '\u000D' { $setType (Token.SKIP); newline ();}
	;

LINE_SEPARATOR
	: '\u2028' { $setType (Token.SKIP); newline ();}
	;

PARAGRAPH_SEPARATOR
	: '\u2029' { $setType (Token.SKIP); newline ();}
	;

//
// White space tokens
//
TAB
	: '\u0009' { $setType (Token.SKIP); }
	;

VERTICAL_TAB
	: '\u000B' { $setType (Token.SKIP); }
	;

FORM_FEED
	: '\u000C' { $setType (Token.SKIP); }
	;

SPACE
	: '\u0020' { $setType (Token.SKIP); }
	;

NO_BREAK_SPACE
	: '\u00A0' { $setType (Token.SKIP); }
	;
