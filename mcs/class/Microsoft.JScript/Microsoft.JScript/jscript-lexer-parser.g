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
    namespace =  "Microsoft.JScript.Tmp";
}


// Parser
class JScriptParser extends Parser;

// Program, see section 14 from Ecma-262, page 75.
program [ASTList astList]
    : 
        source_elements [astList]
    ;


source_elements	[ASTList astList]
{ AST ast = null; }
    : 
	ast = source_element { if (ast != null) astList.Add (ast); } (source_elements [astList] | )
    ;


// See Section 14 from Ecma-262, page 75.
source_element returns [AST ast]
{
	ast = null;
	Statement stm = null;
	FunctionDeclaration fd = null;
}
    : 
	stm = statement
	{ ast = stm; }
    |
	fd = global_function_declaration
	{ ast = fd; }
    |
	conditional_compilation_directive		
    ;


global_function_declaration returns [FunctionDeclaration fd]
{ fd = new FunctionDeclaration (); }
    :
	"function" 
	id:IDENTIFIER { fd.id = id.getText (); }
   	LPAREN (formal_parameter_list [fd.parameters] | ) RPAREN 
   	(COLON rType:IDENTIFIER { fd.returnType = rType.getText (); } | ) 
   	LBRACE (function_body [fd.funcBody] | ) RBRACE
       ;

	
// Statement, see section 12 from Ecma-262, page 61.
statement returns [Statement stm]
{ 
	stm = null; 
	VariableStatement varStm = null;
	Print printStatement = null;
	Enum enumStatement = null;
}
    : 
	block	    
    |
	modifiers ( class_statement
		  | const_statement
		  | enumStatement = enum_statement
		  | interface_statement
                  | varStm = variable_statement { stm = (Statement) varStm; }
		  )			
    |
	empty_statement
    |
	debugger_statement
    |
	if_statement	    
    |
	import_statement
    |
	iteration_statement
    |
	continue_statement
    |
	break_statement
    |
	package_statement
    |
	return_statement
    |
	with_statement
    |
	super_statement
    |
	switch_statement
    |
	throw_statement	    
    |
	labelled_statement
    |
	try_statement
    |
	cc_on_statement
    |
	printStatement = print_statement
	{ stm = (Statement) printStatement; }
    ;


block: LBRACE (statement_list | ) RBRACE 
    ;


// FIXME: find out to what other values can be assigned to it.
const_statement:	 
	"const" name:IDENTIFIER (COLON type:IDENTIFIER | ) 
	ASSIGNMENT
	numeric_literal
	SEMI_COLON
	;


class_statement: 
	"class" 
	classname:IDENTIFIER
	("extends" baseclass:IDENTIFIER | ) 
	("implements" interfaces_list | )
	LBRACE class_members RBRACE
	;


interfaces_list: IDENTIFIER (COMMA IDENTIFIER)* ;


class_members:
	((static_statement)=> static_statement 
	    		| (modifiers
			        ( type_function_declaration
				| const_statement
				| variable_statement
				| enum_statement					
				| class_statement
				)
			       )
	)*
    ;


type_function_declaration returns [FunctionDeclaration fd]
{ fd = new FunctionDeclaration (); }
      :
   	"function"
   	("get" | "set" | ) // If present, denotes a property declaration.
   	functionName:IDENTIFIER { fd.id = functionName.getText (); }
   	LPAREN (formal_parameter_list [fd.parameters] | ) RPAREN
   	(COLON type:IDENTIFIER | )
	(LBRACE (function_body [fd.funcBody] | ) RBRACE | SEMI_COLON)
   	;

debugger_statement: "debugger" ;


empty_statement: SEMI_COLON ;


if_statement
    :
	"if" LPAREN expression RPAREN statement (("else")=> "else" statement)?    
    ;


import_statement: "import" IDENTIFIER SEMI_COLON ;


interface_statement: 
	"interface" 
	intfName:IDENTIFIER 
	("implements" IDENTIFIER | )
	LBRACE interface_members RBRACE
	;


interface_members: (modifiers type_function_declaration)* ;


// See, Ecma-262 3d. Edition, page 64.
// FIXME: more options left to implement.
iteration_statement
    :
	"do" statement "while" LPAREN expression RPAREN SEMI_COLON
    |
	"while" LPAREN expression RPAREN statement
    |
	"for" LPAREN left_hand_side_expression "in" expression RPAREN statement
    ;


// ContinueStatement, Ecma-262, section 12.7
// FIXME: Make sure that no LineSeparator appears between the continue keyword and the identifier or semicolon
continue_statement: "continue" (IDENTIFIER | ) SEMI_COLON ;


// BreakStatement, Ecma-262, section 12.8
// FIXME: Make sure that no LineSeparator appears between the break keyword and the identifier or semicolon
break_statement: "break" (IDENTIFIER | ) SEMI_COLON ;


package_statement returns [Package pack]
{ pack = new Package (); }
    : 
	"package" IDENTIFIER LBRACE package_members RBRACE;


package_members: (package_member)* ;

package_member: (modifiers (class_statement | interface_statement | enum_statement)) ;


// ReturnStatement, Ecma-262, section 12.9
// FIXME: Make sure that no LineSeparator appears between the return keyword and the identifier or semicolon
return_statement: "return" (expression | ) SEMI_COLON ;


// WithStatement, see Ecma-262 3d. Edition, section 12.8, page 67.
with_statement
    : 
	"with" LPAREN expression RPAREN statement 
    ;


// FIXME: we are missing the super.member rule.
super_statement: "super" (arguments) ;


switch_statement
    :
	"switch" LPAREN expression RPAREN case_block
    ;


case_block
    :
	LBRACE (case_clauses | ) (default_clause (case_clauses | ) | ) RBRACE
    ;


case_clauses: (case_clause)+ ;

case_clause
    : 
	"case" expression COLON (statement_list | )
    ;


default_clause
    : 
	"default" COLON (statement_list | )
    ;


enum_statement returns [Enum enm]
{ 
    enm = new Enum (); 
}
    : 
	"enum" name:IDENTIFIER { enm.Name = name.getText (); }
	(COLON type:IDENTIFIER { enm.Type = type.getText (); } | ) 
	LBRACE
	(enumVal:IDENTIFIER
	 (ASSIGNMENT nl:numeric_literal | ) 
	 (COMMA IDENTIFIER (ASSIGNMENT numeric_literal | ))* 
	| 
	)
	RBRACE
	;


labelled_statement
    :
	IDENTIFIER COLON statement
    ;


// FIXME: what does [body] means? ([body] must be between LBRACE and RBRACE)
static_statement: "static" IDENTIFIER LBRACE RBRACE ;


// ThrowStatement, Ecma-262, section 12.13
// FIXME: Make sure no LineSeparator appears between the throw keyword and the expression or semicolon.
throw_statement: "throw" expression SEMI_COLON ;


// See section 12.14 from Ecma-262, 3d. Edition.
try_statement
    : 
	"try" block ((catch_exp (finally_exp | )) | finally_exp)
    ;


cc_on_statement: CC_ON ;


print_statement returns [Print p]
{ 
    p = new Print (); 
    AST exp = null;
}
    :
	"print" LPAREN (exp = primary_expression { p.Exp = exp; } | ) RPAREN ;


// FIXME: more options left to implement
conditional_compilation_directive:
	COND_SET ((COND_DEBUG LPAREN ("on" | "off") RPAREN)
		 |(COND_POSITION LPAREN ("end" | 
					("file" ASSIGNMENT STRING_LITERAL | ))
				 RPAREN))
	;


// NOTE: I call it catch_exp, to avoid confusion on antlr.
catch_exp: "catch" LPAREN IDENTIFIER RPAREN block ;

// NOTE: I call it finally_exp, to avoid confusion on the Java/C# side.
finally_exp: "finally" block ;

    
statement_list
    :
	statement (statement_list | )
    ;

// VariableStatement, see section 12.2 from Ecma-262 3td Edition, page 74.
variable_statement returns [VariableStatement varStm]
{ varStm = new VariableStatement (); }
    :
        "var" variable_declaration_list [varStm] SEMI_COLON
    ;
        

variable_declaration_list [VariableStatement varStm]
{ VariableDeclaration varDecl = null; }
    :
        varDecl = variable_declaration { varStm.Add (varDecl); } (COMMA variable_declaration_list [varStm] | )
    ;

variable_declaration returns [VariableDeclaration varDecl]
{ varDecl = new VariableDeclaration (); }
    :
	id:IDENTIFIER { varDecl.Id = id.getText (); } 
        (COLON type:IDENTIFIER { varDecl.Type = type.getText (); } | ) (initialiser | )
    ;
    

initialiser
    :
        ASSIGNMENT assignment_expression
    ;                           

// FIXME: a conditional_expression can be reduced to a postfixExpression wich 
// is reduced to a left_hand_side_expression
// AssignmentExpression, see section 11.13 from Ecma-262 3td. Edition, page 59.
assignment_expression
    :
        conditional_expression
//    |
//	left_hand_side_expression assignment_operator assignment_expression
    ;

        
assignment_operator
    :
	ASSIGNMENT 
    |
	MULTIPLICATION_ASSIGN 
    |
	DIVISION_ASSIGN 
    |
	REMAINDER_ASSIGN 
    |
	ADDITION_ASSIGN 
    |
	SUBSTRACTION_ASSIGN 
    |
	SIGNED_LEFT_SHIFT_ASSIGN 
    |
	SIGNED_RIGHT_SHIFT_ASSIGN 
    |
	UNSIGNED_RIGHT_SHIFT_ASSIGN 
    |
	BITWISE_AND_ASSIGN 
    |
	BITWISE_XOR_ASSIGN 
    |
	BITWISE_OR_ASSIGN
    ;


// ConditionalExpression, see section 11.12 from Ecma-262, page 58.
conditional_expression
    :
        logical_or_expression (CONDITIONAL assignment_expression COLON assignment_expression | )
    ;
        


// Binary Logical Operators, section 11.11 from Ecma-262 spec, page 58.
logical_or_expression
    :
        logical_and_expression (LOGICAL_OR logical_or_expression | )
    ;


logical_and_expression
    :
        bitwise_or_expression (LOGICAL_AND logical_and_expression | )
    ;


// Binary Bitwise Operators, section 11.10 from Ecma-262 spec, page 57.
bitwise_or_expression
    :
        bitwise_xor_expression (BITWISE_OR bitwise_or_expression | )
    ;

bitwise_xor_expression
    :
        bitwise_and_expression (BITWISE_XOR bitwise_xor_expression | )
    ;


bitwise_and_expression
    :
        equality_expression (BITWISE_AND bitwise_and_expression | )       
    ;


// Equality Operators, section 11.9 from Ecma-262 spec, page 54.
// FIXME: more options left to implement
equality_expression
    :
        relational_expression ((EQUALS | DOES_NOT_EQUALS | STRICT_EQUALS | STRICT_DOES_NOT_EQUALS) equality_expression | )
    ;


// Relational Operators, section 11.4 from Ecma-262 spec, page 52.
// FIXME: more options left to implement
relational_expression
    :
	shift_expression ((L_THAN | G_THAN | LE_THAN | GE_THAN | "instanceof" | "in") relational_expression | )
    ;


// Bitwise Shift Operators, section 11.7 from Ecma-262, page 51.
// FIXME: more options left to implement
shift_expression
    :
        additive_expression ((SIGNED_RIGHT_SHIFT | SIGNED_LEFT_SHIFT) shift_expression | )
    ;



// Additive Operators, section 11.6 from Ecma-262, page 50.
// FIXME: more options left to implement
additive_expression
    :
        multiplicative_expression ((PLUS | MINUS) additive_expression | )
    ;



// Multiplicative Operators, section 11.5 from Ecma-262, page 48.
multiplicative_expression
    :
        unary_expression ((TIMES | DIVISION  | REMAINDER) multiplicative_expression | ) 
    ;


// Unary Operators,  Section 11.4 from Ecma-262, page 46.
unary_expression
    :
        postfix_expression
    |
        ("delete" | "void" | "typeof" | INCREMENT | DECREMENT | PLUS | MINUS | BITWISE_NOT | LOGICAL_NOT) unary_expression
    ;


// Postfix Expressions, section 11.3 from Ecma-262, page 45.
// FIXME: ensure that no LineTerminator appears between LeftHandSideExpression and INCREMENT and DECREMENT.
postfix_expression
    :
        left_hand_side_expression (INCREMENT | DECREMENT | )
    ;


// FIXME: there's a problem with the NEW member_expression arguments rule from member expression
// section 11.2 from Ecma-262 3td Edition, page 43.
left_hand_side_expression
    :
        new_expression
//    |
//        call_expression
    ;


// FIXME: there's a problem with the NEW member_expression arguments rule from member expression
new_expression
    :
        member_expression
    |
        "new" new_expression
    ;

// FIXME: more options left to implement
call_expression
    :
	member_expression arguments (arguments | LSQUARE expression RSQUARE | DOT IDENTIFIER)*
    ;


// See Ecma-262, section 11.2, page 43.
// FIXME: more options left to implement
member_expression
    :
	(primary_expression | function_expression) (LSQUARE expression RSQUARE | DOT IDENTIFIER)*
//    |
//        "new" member_expression arguments
    ;



arguments
    :
        LPAREN (argument_list | ) RPAREN
    ;
        

argument_list
    :
        assignment_expression (COMMA argument_list | )
    ;

// Expressions, section 11, from Ecma-262 3d Edition, page 40.
// FIXME: more options left to implement
primary_expression returns [AST pExp]
{
    pExp = null;
    Literal lit;
}
    :
        "this"
    |
        IDENTIFIER
    |
	lit = literal
	{ pExp = lit; }	    
    |
	array_literal
    |
	object_literal
    |
	LPAREN expression RPAREN
    ;


// Literals, section 7.8 from Ecma-262 3d Edition, page 16.
// FIXME: more options left to implement
literal returns [Literal lit]
{ 
    lit = null;
    StringLiteral strl = new StringLiteral (); 
}
    :
	boolean_literal
    |
	numeric_literal
    |
	null_literal
    |
	s:STRING_LITERAL
	{ 
	    strl.Str = s.getText (); 
	    lit = strl; 
	}
    ;


// FIXME: more options left to implement.
array_literal
    :
	LSQUARE (elision | ) RSQUARE
    ;


elision: (COMMA)+ ;


// ObjectLiteral, see Ecma-262 3d. Edition, page 41 and 42.
object_literal
    : 
	LBRACE (property_name_and_value_list | ) RBRACE ;


property_name_and_value_list
    :
	property_name COLON assignment_expression (COMMA property_name_and_value_list | )
    ;


// PropertyName
// FIXME:  NumericLiteral missing.
property_name
    :
	IDENTIFIER
    |
	STRING_LITERAL
    ;


//  Expression, see Ecma-262 spec, section 11.14, page 60. 
expression: assignment_expression (COMMA  expression | ) ;


function_expression
    :
	"function" (IDENTIFIER | ) LPAREN (formal_parameter_list [null] | ) RPAREN LBRACE function_body [null] RBRACE
    ;


formal_parameter_list [FormalParameterList param]
    :
        id:IDENTIFIER { param.Add (id.getText ()); } (COMMA formal_parameter_list [param] | )
    ;


function_body [ASTList funcBody]
    :
        source_elements [funcBody]
    ;


boolean_literal
    :
    	"true"
    |
    	"false"
    ;

null_literal
    :
	"null"
    ;


numeric_literal
    : DECIMAL_LITERAL
    | HEX_INTEGER_LITERAL
    ;


modifiers: (modifier)* ;

modifier
    : "public" 
    | "private"
    | "protected"
    | "internal"
    | "expando"
    | "static"
    | "abstract"
    | "final"
    ;

version_modifiers
    : "hide"
    | "override"
    ;	

// Lexer
class JScriptLexer extends Lexer;
options {
    charVocabulary='\u0000'..'\uFFFE';
    testLiterals=false;
    k = 4;
}


TAB 
    : 
        '\u0009'
        { _ttype = Token.SKIP; }
    ;
        
   
VERTICAL_TAB
    : 
        '\u000B' 
    ;


FORM_FEED
    :
        '\u000C' 
    ;


SPACE
    :
        '\u0020'
        { _ttype =Token.SKIP; }
    ;


NO_BREAK_SPACE
    :
        '\u00A0'
    ;    

// FIXME: find out possibles Unicode "space separator"
// USP: 


LINE_FEED
    :
        '\u000A'
        { newline (); { _ttype =Token.SKIP; }}
    ;


CARRIGE_RETURN
    :
        '\u000D'
        { newline (); { _ttype =Token.SKIP; }}
    ;


LINE_SEPARATOR
    :
        '\u2028'
        { newline (); { _ttype =Token.SKIP; }}
    ;


PARAGRAPH_SEPARATOR
    :
        '\u2029'
        { newline (); { _ttype =Token.SKIP; }}
    ;




// Punctuators

LBRACE: '{' ;

RBRACE: '}' ;

LPAREN: '(' ;

RPAREN: ')' ;

LSQUARE: '[' ;

RSQUARE: ']' ;

DOT: '.' ;

SEMI_COLON: ';' ;

COMMA: ',' ;

L_THAN: '<' ;

G_THAN: '>' ;

LE_THAN: "<=" ;

GE_THAN: ">=" ;

EQUALS: "==" ;

DOES_NOT_EQUALS: "!=" ;

STRICT_EQUALS: "===" ;

STRICT_DOES_NOT_EQUALS: "!==" ;

PLUS: '+' ;

MINUS: '-' ;

TIMES: '*' ;

REMAINDER: '%' ;

INCREMENT: "++" ;

DECREMENT: "--";

SIGNED_LEFT_SHIFT: "<<" ;

SIGNED_RIGHT_SHIFT: ">>" ;

UNSIGNED_RIGHT_SHIFT: ">>>" ;

BITWISE_AND: '&' ;

BITWISE_OR: '|' ;

BITWISE_XOR: '^' ;

LOGICAL_NOT: '!' ;

BITWISE_NOT: '~' ;

LOGICAL_AND: "&&" ;

LOGICAL_OR: "||" ;

CONDITIONAL: '?' ;

COLON: ':' ;

ASSIGNMENT: '=' ;

ADDITION_ASSIGN: "+=" ;

SUBSTRACTION_ASSIGN: "-=" ;

MULTIPLICATION_ASSIGN: "*=" ;

REMAINDER_ASSIGN: "%=" ;

SIGNED_LEFT_SHIFT_ASSIGN: "<<=" ;

SIGNED_RIGHT_SHIFT_ASSIGN: ">>=" ;

UNSIGNED_RIGHT_SHIFT_ASSIGN: ">>>=" ;

BITWISE_AND_ASSIGN: "&=" ;

BITWISE_OR_ASSIGN: "|=" ;

BITWISE_XOR_ASSIGN: "^=" ;

DIVISION: '/' ;

DIVISION_ASSIGN: "/=" ;

COND_SET: "@set" ;
 
COND_DEBUG: "@debug" ;

COND_POSITION: "@position" ;

COND_IF: "@if" ;

COND_ELIF: "@elif" ;

COND_ELSE: "@else" ;

COND_END: "@end" ;

CC_ON: "@cc_on" ;


// literals
DECIMAL_LITERAL: ('0'  | ('1'..'9')('0'..'9')*) (DOT ('0'..'9')* | ) (('e' | 'E') (('+' | '-' | ) ('0'..'9')+) | )
    ;

HEX_INTEGER_LITERAL: '0' 'x' ('0'..'9' | 'a'..'f' | 'A'..'F')+
    ;

// FIXME: this just temporal, in order to get into parsing
STRING_LITERAL
    : 
        '"'!('a'..'z' | 'A'..'Z' | '\u0020')+'"'!
    ;


// FIXME: this a temporal definition.
//        We must handle the UNICODE charset, see section 7.6 of the Ecma-262 spec
IDENTIFIER
options { testLiterals=true; }
    : 
	('a'..'z' | 'A'..'Z') ('a'..'z' | 'A'..'Z' | '0'..'9')*
    ;


SL_COMMENT
    :
	"//" (~('\u000A' | '\u000D' | '\u2028' | '\u2029'))* { $setType (Token.SKIP); newline (); }
    ;
