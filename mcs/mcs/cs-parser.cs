// created by jay 0.7 (c) 1998 Axel.Schreiner@informatik.uni-osnabrueck.de

#line 1 "cs-parser.jay"

//
// cs-parser.jay: The Parser for the C# compiler
//
// Author: Miguel de Icaza (miguel@gnu.org)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2001 Ximian, Inc (http://www.ximian.com)
//
// TODO:
//   (1) Get rid of the *Collections.cs, that is an idea I took from System.CodeDOM
//       And come to think of it, it is not that great, it duplicates a lot of code
//       for something which is not really needed.  We still have piles of typecasts
//       anwyays (due to the nature of the stack being a collection of Objects).
//
//   (2) Figure out why error productions dont work.  `type-declaration' is a
//       great spot to put an `error' because you can reproduce it with this input:
//	 "public X { }"
//
//   (3) Move Modifier checking from each object into the parser itself, that will
//       get rid of the global "error" symbol that we use now to report errors. 
//       We still need to pass a pointer to the tree.ErrorHandler, but that is a 
//	 separate problem
//
using System.Text;
using CSC;
using System;

namespace CSC
{
	using System.Collections;
	using Compiler;
	using CSC;
	using CIR;

	/// <summary>
	///    The C# Parser
	/// </summary>
	public class CSharpParser : Parser {
		static int global_errors;

		Namespace     current_namespace;
		TypeContainer current_container;
	
		// <summary>
		//   Current block is used to add statements as we find
		//   them.  
		// </summary>

		Block      current_block;

		// <summary>
		//   Current interface is used by the various declaration
		//   productions in the interface declaration to "add"
		//   the interfaces as we find them.
		// </summary>
		Interface  current_interface;

		// <summary>
		//   This is used by the unary_expression code to resolve
		//   a name against a parameter.  
		// </summary>
		Parameters current_local_parameters;

		// <summary>
		//   Using during property parsing to describe the implicit
		//   value parameter that is passed to the "set" accesor
		//   method
		// </summary>
		ParameterCollection implicit_value_parameters;

		// <summary>
		//   Here we keep track of type references. 
		// </summary>
		TypeRefManager type_references;
#line 81 "-"

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
    string res;
    if ((expected != null) && (expected.Length  > 0)) {
      res = message+", expecting";
      for (int n = 0; n < expected.Length; ++ n)
        res += " "+expected[n];
    } else
        res = message;
    throw new Exception (res);
  }

  /** debugging support, requires the package jay.yydebug.
      Set to null to suppress debugging messages.
    */
  protected yydebug.yyDebug yydebug;

  protected static  int yyFinal = 2;
  public static  string [] yyRule = {
    "$accept : compilation_unit",
    "compilation_unit : opt_using_directives opt_attributes opt_namespace_member_declarations EOF",
    "using_directives : using_directive",
    "using_directives : using_directives using_directive",
    "using_directive : using_alias_directive",
    "using_directive : using_namespace_directive",
    "using_alias_directive : USING IDENTIFIER ASSIGN namespace_or_type_name SEMICOLON",
    "using_namespace_directive : USING namespace_name SEMICOLON",
    "namespace_declarations : namespace_declaration",
    "namespace_declarations : namespace_declarations namespace_declaration",
    "$$1 :",
    "namespace_declaration : NAMESPACE qualified_identifier $$1 namespace_body opt_semicolon",
    "opt_semicolon :",
    "opt_semicolon : SEMICOLON",
    "opt_comma :",
    "opt_comma : COMMA",
    "qualified_identifier : IDENTIFIER",
    "qualified_identifier : qualified_identifier DOT IDENTIFIER",
    "namespace_name : namespace_or_type_name",
    "namespace_body : OPEN_BRACE opt_using_directives opt_namespace_member_declarations CLOSE_BRACE",
    "opt_using_directives :",
    "opt_using_directives : using_directives",
    "opt_namespace_member_declarations :",
    "opt_namespace_member_declarations : namespace_member_declarations",
    "namespace_member_declarations : namespace_member_declaration",
    "namespace_member_declarations : namespace_member_declarations namespace_member_declaration",
    "namespace_member_declaration : type_declaration",
    "namespace_member_declaration : namespace_declaration",
    "type_declaration : class_declaration",
    "type_declaration : struct_declaration",
    "type_declaration : interface_declaration",
    "type_declaration : enum_declaration",
    "type_declaration : delegate_declaration",
    "opt_attributes :",
    "opt_attributes : attributes",
    "attributes : attribute_sections",
    "attribute_sections : attribute_section",
    "attribute_sections : attribute_sections attribute_section",
    "attribute_section : OPEN_BRACKET opt_attribute_target_specifier attribute_list CLOSE_BRACKET",
    "opt_attribute_target_specifier :",
    "opt_attribute_target_specifier : attribute_target_specifier",
    "attribute_target_specifier : attribute_target COLON",
    "attribute_target : IDENTIFIER",
    "attribute_target : EVENT",
    "attribute_target : RETURN",
    "attribute_list : attribute",
    "attribute_list : attribute_list COMMA attribute",
    "attribute : attribute_name",
    "attribute : opt_attribute_arguments",
    "attribute_name : type_name",
    "opt_attribute_arguments :",
    "opt_attribute_arguments : OPEN_PARENS attribute_arguments CLOSE_PARENS",
    "attribute_arguments : expression",
    "attribute_arguments : attribute_arguments COMMA expression",
    "opt_dimension_separators :",
    "opt_dimension_separators : dimension_separators",
    "dimension_separators : COMMA",
    "dimension_separators : dimension_separators COMMA",
    "class_body : OPEN_BRACE opt_class_member_declarations CLOSE_BRACE",
    "opt_class_member_declarations :",
    "opt_class_member_declarations : class_member_declarations",
    "class_member_declarations : class_member_declaration",
    "class_member_declarations : class_member_declarations class_member_declaration",
    "class_member_declaration : constant_declaration",
    "class_member_declaration : field_declaration",
    "class_member_declaration : method_declaration",
    "class_member_declaration : property_declaration",
    "class_member_declaration : event_declaration",
    "class_member_declaration : indexer_declaration",
    "class_member_declaration : operator_declaration",
    "class_member_declaration : constructor_declaration",
    "class_member_declaration : destructor_declaration",
    "class_member_declaration : type_declaration",
    "$$2 :",
    "struct_declaration : opt_attributes opt_modifiers STRUCT IDENTIFIER $$2 opt_struct_interfaces struct_body opt_semicolon",
    "opt_struct_interfaces :",
    "opt_struct_interfaces : struct_interfaces",
    "struct_interfaces : struct_interface",
    "struct_interfaces : struct_interfaces struct_interface",
    "struct_interface : COLON type_list",
    "struct_body : OPEN_BRACE opt_struct_member_declarations CLOSE_BRACE",
    "opt_struct_member_declarations :",
    "opt_struct_member_declarations : struct_member_declarations",
    "struct_member_declarations : struct_member_declaration",
    "struct_member_declarations : struct_member_declarations struct_member_declaration",
    "struct_member_declaration : constant_declaration",
    "struct_member_declaration : field_declaration",
    "struct_member_declaration : method_declaration",
    "struct_member_declaration : property_declaration",
    "struct_member_declaration : event_declaration",
    "struct_member_declaration : indexer_declaration",
    "struct_member_declaration : operator_declaration",
    "struct_member_declaration : constructor_declaration",
    "struct_member_declaration : type_declaration",
    "constant_declaration : opt_attributes opt_modifiers CONST type constant_declarators SEMICOLON",
    "constant_declarators : constant_declarator",
    "constant_declarators : constant_declarators COMMA constant_declarator",
    "constant_declarator : IDENTIFIER ASSIGN constant_expression",
    "field_declaration : opt_attributes opt_modifiers type variable_declarators SEMICOLON",
    "variable_declarators : variable_declarator",
    "variable_declarators : variable_declarators COMMA variable_declarator",
    "variable_declarator : IDENTIFIER ASSIGN variable_initializer",
    "variable_declarator : IDENTIFIER",
    "variable_initializer : expression",
    "variable_initializer : array_initializer",
    "method_declaration : method_header method_body",
    "method_header : opt_attributes opt_modifiers type member_name OPEN_PARENS opt_formal_parameter_list CLOSE_PARENS",
    "method_header : opt_attributes opt_modifiers VOID member_name OPEN_PARENS opt_formal_parameter_list CLOSE_PARENS",
    "method_body : block",
    "method_body : SEMICOLON",
    "opt_formal_parameter_list :",
    "opt_formal_parameter_list : formal_parameter_list",
    "formal_parameter_list : fixed_parameters",
    "formal_parameter_list : fixed_parameters COMMA parameter_array",
    "formal_parameter_list : parameter_array",
    "fixed_parameters : fixed_parameter",
    "fixed_parameters : fixed_parameters COMMA fixed_parameter",
    "fixed_parameter : opt_attributes opt_parameter_modifier type IDENTIFIER",
    "opt_parameter_modifier :",
    "opt_parameter_modifier : parameter_modifier",
    "parameter_modifier : REF",
    "parameter_modifier : OUT",
    "parameter_array : opt_attributes PARAMS type IDENTIFIER",
    "member_name : IDENTIFIER",
    "member_name : interface_type DOT IDENTIFIER",
    "$$3 :",
    "$$4 :",
    "property_declaration : opt_attributes opt_modifiers type member_name OPEN_BRACE $$3 accessor_declarations $$4 CLOSE_BRACE",
    "accessor_declarations : get_accessor_declaration opt_set_accessor_declaration",
    "accessor_declarations : set_accessor_declaration opt_get_accessor_declaration",
    "opt_get_accessor_declaration :",
    "opt_get_accessor_declaration : get_accessor_declaration",
    "opt_set_accessor_declaration :",
    "opt_set_accessor_declaration : set_accessor_declaration",
    "get_accessor_declaration : opt_attributes GET accessor_body",
    "$$5 :",
    "set_accessor_declaration : opt_attributes SET $$5 accessor_body",
    "accessor_body : block",
    "accessor_body : SEMICOLON",
    "$$6 :",
    "interface_declaration : opt_attributes opt_modifiers INTERFACE IDENTIFIER $$6 opt_interface_base interface_body",
    "opt_interface_base :",
    "opt_interface_base : interface_base",
    "interface_base : COLON interface_type_list",
    "interface_type_list : interface_type",
    "interface_type_list : interface_type_list COMMA interface_type",
    "interface_body : OPEN_BRACE opt_interface_member_declarations CLOSE_BRACE",
    "opt_interface_member_declarations :",
    "opt_interface_member_declarations : interface_member_declarations",
    "interface_member_declarations : interface_member_declaration",
    "interface_member_declarations : interface_member_declarations interface_member_declaration",
    "interface_member_declaration : interface_method_declaration",
    "interface_member_declaration : interface_property_declaration",
    "interface_member_declaration : interface_event_declaration",
    "interface_member_declaration : interface_indexer_declaration",
    "opt_new :",
    "opt_new : NEW",
    "interface_method_declaration : opt_attributes opt_new type IDENTIFIER OPEN_PARENS opt_formal_parameter_list CLOSE_PARENS SEMICOLON",
    "interface_method_declaration : opt_attributes opt_new VOID IDENTIFIER OPEN_PARENS opt_formal_parameter_list CLOSE_PARENS SEMICOLON",
    "$$7 :",
    "$$8 :",
    "interface_property_declaration : opt_attributes opt_new type IDENTIFIER OPEN_BRACE $$7 interface_accesors $$8 CLOSE_BRACE",
    "interface_accesors : opt_attributes GET SEMICOLON",
    "interface_accesors : opt_attributes SET SEMICOLON",
    "interface_accesors : opt_attributes GET SEMICOLON opt_attributes SET SEMICOLON",
    "interface_accesors : opt_attributes SET SEMICOLON opt_attributes GET SEMICOLON",
    "interface_event_declaration : opt_attributes opt_new EVENT type IDENTIFIER SEMICOLON",
    "$$9 :",
    "$$10 :",
    "interface_indexer_declaration : opt_attributes opt_new type THIS OPEN_BRACKET formal_parameter_list CLOSE_BRACKET OPEN_BRACE $$9 interface_accesors $$10 CLOSE_BRACE",
    "operator_declaration : opt_attributes opt_modifiers operator_declarator block",
    "operator_declarator : type OPERATOR overloadable_operator OPEN_PARENS type IDENTIFIER CLOSE_PARENS",
    "operator_declarator : type OPERATOR overloadable_operator OPEN_PARENS type IDENTIFIER COMMA type IDENTIFIER CLOSE_PARENS",
    "operator_declarator : conversion_operator_declarator",
    "overloadable_operator : BANG",
    "overloadable_operator : TILDE",
    "overloadable_operator : OP_INC",
    "overloadable_operator : OP_DEC",
    "overloadable_operator : TRUE",
    "overloadable_operator : FALSE",
    "overloadable_operator : PLUS",
    "overloadable_operator : MINUS",
    "overloadable_operator : STAR",
    "overloadable_operator : DIV",
    "overloadable_operator : PERCENT",
    "overloadable_operator : BITWISE_AND",
    "overloadable_operator : BITWISE_OR",
    "overloadable_operator : CARRET",
    "overloadable_operator : OP_SHIFT_LEFT",
    "overloadable_operator : OP_SHIFT_RIGHT",
    "overloadable_operator : OP_EQ",
    "overloadable_operator : OP_NE",
    "overloadable_operator : OP_GT",
    "overloadable_operator : OP_LT",
    "overloadable_operator : OP_GE",
    "overloadable_operator : OP_LE",
    "conversion_operator_declarator : IMPLICIT OPERATOR type OPEN_PARENS type IDENTIFIER CLOSE_PARENS",
    "conversion_operator_declarator : EXPLICIT OPERATOR type OPEN_PARENS type IDENTIFIER CLOSE_PARENS",
    "constructor_declaration : opt_attributes opt_modifiers constructor_declarator block",
    "constructor_declarator : IDENTIFIER OPEN_PARENS opt_formal_parameter_list CLOSE_PARENS opt_constructor_initializer",
    "opt_constructor_initializer :",
    "opt_constructor_initializer : constructor_initializer",
    "constructor_initializer : COLON BASE OPEN_PARENS opt_argument_list CLOSE_PARENS",
    "constructor_initializer : COLON THIS OPEN_PARENS opt_argument_list CLOSE_PARENS",
    "destructor_declaration : opt_attributes TILDE IDENTIFIER OPEN_PARENS CLOSE_PARENS block",
    "event_declaration : opt_attributes opt_modifiers EVENT type variable_declarators SEMICOLON",
    "event_declaration : opt_attributes opt_modifiers EVENT type member_name OPEN_BRACE event_accesor_declarations CLOSE_BRACE SEMICOLON",
    "event_accesor_declarations : add_accessor_declaration remove_accessor_declaration",
    "event_accesor_declarations : remove_accessor_declaration add_accessor_declaration",
    "add_accessor_declaration : opt_attributes ADD block",
    "remove_accessor_declaration : opt_attributes REMOVE block",
    "indexer_declaration : opt_attributes opt_modifiers indexer_declarator OPEN_BRACE accessor_declarations CLOSE_BRACE",
    "indexer_declarator : type THIS OPEN_BRACKET formal_parameter_list CLOSE_BRACKET",
    "indexer_declarator : type interface_type DOT THIS OPEN_BRACKET formal_parameter_list CLOSE_BRACKET",
    "enum_declaration : opt_attributes opt_modifiers ENUM IDENTIFIER opt_enum_base enum_body opt_semicolon",
    "opt_enum_base :",
    "opt_enum_base : COLON integral_type",
    "enum_body : OPEN_BRACE opt_enum_member_declarations CLOSE_BRACE",
    "enum_body : OPEN_BRACE enum_member_declarations COMMA CLOSE_BRACE",
    "opt_enum_member_declarations :",
    "opt_enum_member_declarations : enum_member_declarations",
    "enum_member_declarations : enum_member_declaration",
    "enum_member_declarations : enum_member_declarations COMMA enum_member_declaration",
    "enum_member_declaration : opt_attributes IDENTIFIER",
    "enum_member_declaration : opt_attributes IDENTIFIER ASSIGN expression",
    "delegate_declaration : opt_attributes opt_modifiers DELEGATE type IDENTIFIER OPEN_PARENS formal_parameter_list CLOSE_PARENS SEMICOLON",
    "type_name : namespace_or_type_name",
    "namespace_or_type_name : qualified_identifier",
    "type : type_name",
    "type : builtin_types",
    "type : array_type",
    "type_list : type",
    "type_list : type_list type",
    "builtin_types : OBJECT",
    "builtin_types : STRING",
    "builtin_types : BOOL",
    "builtin_types : DECIMAL",
    "builtin_types : FLOAT",
    "builtin_types : DOUBLE",
    "builtin_types : integral_type",
    "integral_type : SBYTE",
    "integral_type : BYTE",
    "integral_type : SHORT",
    "integral_type : USHORT",
    "integral_type : INT",
    "integral_type : UINT",
    "integral_type : LONG",
    "integral_type : ULONG",
    "integral_type : CHAR",
    "interface_type : type_name",
    "array_type : type rank_specifiers",
    "primary_expression : literal",
    "primary_expression : qualified_identifier",
    "primary_expression : parenthesized_expression",
    "primary_expression : member_access",
    "primary_expression : invocation_expression",
    "primary_expression : element_access",
    "primary_expression : this_access",
    "primary_expression : base_access",
    "primary_expression : post_increment_expression",
    "primary_expression : post_decrement_expression",
    "primary_expression : new_expression",
    "primary_expression : typeof_expression",
    "primary_expression : sizeof_expression",
    "primary_expression : checked_expression",
    "primary_expression : unchecked_expression",
    "literal : boolean_literal",
    "literal : integer_literal",
    "literal : real_literal",
    "literal : LITERAL_CHARACTER",
    "literal : LITERAL_STRING",
    "literal : NULL",
    "real_literal : LITERAL_FLOAT",
    "real_literal : LITERAL_DOUBLE",
    "real_literal : LITERAL_DECIMAL",
    "integer_literal : LITERAL_INTEGER",
    "boolean_literal : TRUE",
    "boolean_literal : FALSE",
    "parenthesized_expression : OPEN_PARENS expression CLOSE_PARENS",
    "member_access : primary_expression DOT IDENTIFIER",
    "member_access : predefined_type DOT IDENTIFIER",
    "predefined_type : builtin_types",
    "invocation_expression : primary_expression OPEN_PARENS opt_argument_list CLOSE_PARENS",
    "opt_argument_list :",
    "opt_argument_list : argument_list",
    "argument_list : argument",
    "argument_list : argument_list COMMA argument",
    "argument : expression",
    "argument : REF variable_reference",
    "argument : OUT variable_reference",
    "variable_reference : expression",
    "element_access : primary_expression OPEN_BRACKET expression_list CLOSE_BRACKET",
    "expression_list : expression",
    "expression_list : expression_list COMMA expression",
    "this_access : THIS",
    "base_access : BASE DOT IDENTIFIER",
    "base_access : BASE OPEN_BRACKET expression_list CLOSE_BRACKET",
    "post_increment_expression : primary_expression OP_INC",
    "post_decrement_expression : primary_expression OP_DEC",
    "new_expression : object_or_delegate_creation_expression",
    "new_expression : array_creation_expression",
    "object_or_delegate_creation_expression : NEW type OPEN_PARENS opt_argument_list CLOSE_PARENS",
    "array_creation_expression : NEW type OPEN_BRACKET expression_list CLOSE_BRACKET opt_rank_specifier opt_array_initializer",
    "opt_rank_specifier :",
    "opt_rank_specifier : rank_specifiers",
    "rank_specifiers : rank_specifier",
    "rank_specifiers : rank_specifiers rank_specifier",
    "rank_specifier : OPEN_BRACKET opt_dim_separators CLOSE_BRACKET",
    "opt_dim_separators :",
    "opt_dim_separators : dim_separators",
    "dim_separators : COMMA",
    "dim_separators : dim_separators COMMA",
    "opt_array_initializer :",
    "opt_array_initializer : array_initializer",
    "array_initializer : OPEN_BRACE CLOSE_BRACE",
    "array_initializer : OPEN_BRACE variable_initializer_list CLOSE_BRACE",
    "array_initializer : OPEN_BRACE variable_initializer_list COMMA CLOSE_BRACE",
    "variable_initializer_list : variable_initializer",
    "variable_initializer_list : variable_initializer_list COMMA variable_initializer",
    "typeof_expression : TYPEOF OPEN_PARENS type CLOSE_PARENS",
    "sizeof_expression : SIZEOF OPEN_PARENS type CLOSE_PARENS",
    "checked_expression : CHECKED OPEN_PARENS expression CLOSE_PARENS",
    "unchecked_expression : UNCHECKED OPEN_PARENS expression CLOSE_PARENS",
    "unary_expression : primary_expression",
    "unary_expression : PLUS unary_expression",
    "unary_expression : MINUS unary_expression",
    "unary_expression : BANG unary_expression",
    "unary_expression : TILDE unary_expression",
    "unary_expression : STAR unary_expression",
    "unary_expression : BITWISE_AND unary_expression",
    "unary_expression : pre_increment_expression",
    "unary_expression : pre_decrement_expression",
    "unary_expression : cast_expression",
    "pre_increment_expression : OP_INC unary_expression",
    "pre_decrement_expression : OP_DEC unary_expression",
    "cast_expression : OPEN_PARENS qualified_identifier CLOSE_PARENS unary_expression",
    "cast_expression : OPEN_PARENS builtin_types CLOSE_PARENS unary_expression",
    "multiplicative_expression : unary_expression",
    "multiplicative_expression : multiplicative_expression STAR unary_expression",
    "multiplicative_expression : multiplicative_expression DIV unary_expression",
    "multiplicative_expression : multiplicative_expression PERCENT unary_expression",
    "additive_expression : multiplicative_expression",
    "additive_expression : additive_expression PLUS multiplicative_expression",
    "additive_expression : additive_expression MINUS multiplicative_expression",
    "shift_expression : additive_expression",
    "shift_expression : shift_expression OP_SHIFT_LEFT additive_expression",
    "shift_expression : shift_expression OP_SHIFT_RIGHT additive_expression",
    "relational_expression : shift_expression",
    "relational_expression : relational_expression OP_LT shift_expression",
    "relational_expression : relational_expression OP_GT shift_expression",
    "relational_expression : relational_expression OP_LE shift_expression",
    "relational_expression : relational_expression OP_GE shift_expression",
    "relational_expression : relational_expression IS type",
    "relational_expression : relational_expression AS type",
    "equality_expression : relational_expression",
    "equality_expression : equality_expression OP_EQ relational_expression",
    "equality_expression : equality_expression OP_NE relational_expression",
    "and_expression : equality_expression",
    "and_expression : and_expression BITWISE_AND equality_expression",
    "exclusive_or_expression : and_expression",
    "exclusive_or_expression : exclusive_or_expression CARRET and_expression",
    "inclusive_or_expression : exclusive_or_expression",
    "inclusive_or_expression : inclusive_or_expression BITWISE_OR exclusive_or_expression",
    "conditional_and_expression : inclusive_or_expression",
    "conditional_and_expression : conditional_and_expression OP_AND inclusive_or_expression",
    "conditional_or_expression : conditional_and_expression",
    "conditional_or_expression : conditional_or_expression OP_OR conditional_and_expression",
    "conditional_expression : conditional_or_expression",
    "conditional_expression : conditional_or_expression INTERR expression COLON expression",
    "assignment_expression : unary_expression ASSIGN expression",
    "assignment_expression : unary_expression OP_MULT_ASSIGN expression",
    "assignment_expression : unary_expression OP_DIV_ASSIGN expression",
    "assignment_expression : unary_expression OP_MOD_ASSIGN expression",
    "assignment_expression : unary_expression OP_ADD_ASSIGN expression",
    "assignment_expression : unary_expression OP_SUB_ASSIGN expression",
    "assignment_expression : unary_expression OP_SHIFT_LEFT_ASSIGN expression",
    "assignment_expression : unary_expression OP_SHIFT_RIGHT_ASSIGN expression",
    "assignment_expression : unary_expression OP_AND_ASSIGN expression",
    "assignment_expression : unary_expression OP_OR_ASSIGN expression",
    "assignment_expression : unary_expression OP_XOR_ASSIGN expression",
    "expression : conditional_expression",
    "expression : assignment_expression",
    "constant_expression : expression",
    "boolean_expression : expression",
    "$$11 :",
    "class_declaration : opt_attributes opt_modifiers CLASS IDENTIFIER $$11 opt_class_base class_body opt_semicolon",
    "opt_modifiers :",
    "opt_modifiers : modifiers",
    "modifiers : modifier",
    "modifiers : modifiers modifier",
    "modifier : NEW",
    "modifier : PUBLIC",
    "modifier : PROTECTED",
    "modifier : INTERNAL",
    "modifier : PRIVATE",
    "modifier : ABSTRACT",
    "modifier : SEALED",
    "modifier : STATIC",
    "modifier : READONLY",
    "modifier : VIRTUAL",
    "modifier : OVERRIDE",
    "modifier : EXTERN",
    "opt_class_base :",
    "opt_class_base : class_base",
    "class_base : COLON type_list",
    "$$12 :",
    "block : OPEN_BRACE $$12 opt_statement_list CLOSE_BRACE",
    "opt_statement_list :",
    "opt_statement_list : statement_list",
    "statement_list : statement",
    "statement_list : statement_list statement",
    "statement : declaration_statement",
    "statement : embedded_statement",
    "statement : labeled_statement",
    "embedded_statement : block",
    "embedded_statement : empty_statement",
    "embedded_statement : expression_statement",
    "embedded_statement : selection_statement",
    "embedded_statement : iteration_statement",
    "embedded_statement : jump_statement",
    "embedded_statement : try_statement",
    "embedded_statement : checked_statement",
    "embedded_statement : unchecked_statement",
    "embedded_statement : lock_statement",
    "embedded_statement : using_statement",
    "empty_statement : SEMICOLON",
    "labeled_statement : IDENTIFIER COLON statement",
    "declaration_statement : local_variable_declaration SEMICOLON",
    "declaration_statement : local_constant_declaration SEMICOLON",
    "local_variable_type : primary_expression type_suffixes",
    "local_variable_type : builtin_types type_suffixes",
    "local_variable_type : VOID type_suffixes",
    "type_suffixes :",
    "type_suffixes : type_suffix_list",
    "type_suffix_list : type_suffix",
    "type_suffix_list : type_suffix_list type_suffix",
    "type_suffix : OPEN_BRACKET opt_dim_separators CLOSE_BRACKET",
    "local_variable_declaration : local_variable_type variable_declarators",
    "local_constant_declaration : CONST type constant_declarator",
    "expression_statement : statement_expression SEMICOLON",
    "statement_expression : invocation_expression",
    "statement_expression : object_creation_expression",
    "statement_expression : assignment_expression",
    "statement_expression : post_increment_expression",
    "statement_expression : post_decrement_expression",
    "statement_expression : pre_increment_expression",
    "statement_expression : pre_decrement_expression",
    "object_creation_expression : object_or_delegate_creation_expression",
    "selection_statement : if_statement",
    "selection_statement : switch_statement",
    "if_statement : IF OPEN_PARENS boolean_expression CLOSE_PARENS embedded_statement",
    "if_statement : IF OPEN_PARENS boolean_expression CLOSE_PARENS embedded_statement ELSE embedded_statement",
    "switch_statement : SWITCH OPEN_PARENS expression CLOSE_PARENS switch_block",
    "switch_block : OPEN_BRACE opt_switch_sections CLOSE_BRACE",
    "opt_switch_sections :",
    "opt_switch_sections : switch_sections",
    "switch_sections : switch_section",
    "switch_sections : switch_sections switch_section",
    "$$13 :",
    "switch_section : switch_labels $$13 statement_list",
    "switch_labels : switch_label",
    "switch_labels : switch_labels switch_label",
    "switch_label : CASE constant_expression COLON",
    "switch_label : DEFAULT COLON",
    "iteration_statement : while_statement",
    "iteration_statement : do_statement",
    "iteration_statement : for_statement",
    "iteration_statement : foreach_statement",
    "while_statement : WHILE OPEN_PARENS boolean_expression CLOSE_PARENS embedded_statement",
    "do_statement : DO embedded_statement WHILE OPEN_PARENS boolean_expression CLOSE_PARENS SEMICOLON",
    "for_statement : FOR OPEN_PARENS opt_for_initializer SEMICOLON opt_for_condition SEMICOLON opt_for_iterator CLOSE_PARENS embedded_statement",
    "opt_for_initializer :",
    "opt_for_initializer : for_initializer",
    "for_initializer : local_variable_declaration",
    "for_initializer : statement_expression_list",
    "opt_for_condition :",
    "opt_for_condition : boolean_expression",
    "opt_for_iterator :",
    "opt_for_iterator : for_iterator",
    "for_iterator : statement_expression_list",
    "statement_expression_list : statement_expression",
    "statement_expression_list : statement_expression_list COMMA statement_expression",
    "foreach_statement : FOREACH OPEN_PARENS type IDENTIFIER IN expression CLOSE_PARENS embedded_statement",
    "jump_statement : break_statement",
    "jump_statement : continue_statement",
    "jump_statement : goto_statement",
    "jump_statement : return_statement",
    "jump_statement : throw_statement",
    "break_statement : BREAK SEMICOLON",
    "continue_statement : CONTINUE SEMICOLON",
    "goto_statement : GOTO IDENTIFIER SEMICOLON",
    "goto_statement : GOTO CASE constant_expression SEMICOLON",
    "goto_statement : GOTO DEFAULT SEMICOLON",
    "return_statement : RETURN opt_expression SEMICOLON",
    "throw_statement : THROW opt_expression SEMICOLON",
    "opt_expression :",
    "opt_expression : expression",
    "try_statement : TRY block catch_clauses",
    "try_statement : TRY block finalize_clause",
    "try_statement : TRY block catch_clauses finalize_clause",
    "catch_clauses : specific_catch_clauses opt_general_catch_clause",
    "catch_clauses : opt_specific_catch_clauses general_catch_clause",
    "opt_general_catch_clause :",
    "opt_general_catch_clause : general_catch_clause",
    "opt_specific_catch_clauses :",
    "opt_specific_catch_clauses : specific_catch_clauses",
    "specific_catch_clauses : specific_catch_clause",
    "specific_catch_clauses : specific_catch_clauses specific_catch_clause",
    "specific_catch_clause : CATCH OPEN_PARENS type opt_identifier CLOSE_PARENS block",
    "opt_identifier :",
    "opt_identifier : IDENTIFIER",
    "general_catch_clause : CATCH block",
    "finalize_clause : FINALLY block",
    "checked_statement : CHECKED block",
    "unchecked_statement : UNCHECKED block",
    "lock_statement : LOCK OPEN_PARENS expression CLOSE_PARENS embedded_statement",
    "using_statement : USING OPEN_PARENS resource_acquisition CLOSE_PARENS embedded_statement",
    "resource_acquisition : local_variable_declaration expression",
  };
  protected static  string [] yyName = {    
    "end-of-file",null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,"'!'",null,null,null,"'%'","'&'",
    null,"'('","')'","'*'","'+'","','","'-'","'.'","'/'",null,null,null,
    null,null,null,null,null,null,null,"':'","';'","'<'","'='","'>'",
    "'?'",null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,"'['",null,"']'","'^'",null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,"'{'","'|'","'}'","'~'",null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,"EOF","NONE","ERROR","ABSTRACT","AS","ADD","BASE","BOOL","BREAK",
    "BYTE","CASE","CATCH","CHAR","CHECKED","CLASS","CONST","CONTINUE",
    "DECIMAL","DEFAULT","DELEGATE","DO","DOUBLE","ELSE","ENUM","EVENT",
    "EXPLICIT","EXTERN","FALSE","FINALLY","FIXED","FLOAT","FOR","FOREACH",
    "GOTO","IF","IMPLICIT","IN","INT","INTERFACE","INTERNAL","IS","LOCK",
    "LONG","NAMESPACE","NEW","NULL","OBJECT","OPERATOR","OUT","OVERRIDE",
    "PARAMS","PRIVATE","PROTECTED","PUBLIC","READONLY","REF","RETURN",
    "REMOVE","SBYTE","SEALED","SHORT","SIZEOF","STATIC","STRING","STRUCT",
    "SWITCH","THIS","THROW","TRUE","TRY","TYPEOF","UINT","ULONG",
    "UNCHECKED","UNSAFE","USHORT","USING","VIRTUAL","VOID","WHILE","GET",
    "\"get\"","SET","\"set\"","OPEN_BRACE","CLOSE_BRACE","OPEN_BRACKET",
    "CLOSE_BRACKET","OPEN_PARENS","CLOSE_PARENS","DOT","COMMA","COLON",
    "SEMICOLON","TILDE","PLUS","MINUS","BANG","ASSIGN","OP_LT","OP_GT",
    "BITWISE_AND","BITWISE_OR","STAR","PERCENT","DIV","CARRET","INTERR",
    "OP_INC","\"++\"","OP_DEC","\"--\"","OP_SHIFT_LEFT","\"<<\"",
    "OP_SHIFT_RIGHT","\">>\"","OP_LE","\"<=\"","OP_GE","\">=\"","OP_EQ",
    "\"==\"","OP_NE","\"!=\"","OP_AND","\"&&\"","OP_OR","\"||\"",
    "OP_MULT_ASSIGN","\"*=\"","OP_DIV_ASSIGN","\"/=\"","OP_MOD_ASSIGN",
    "\"%=\"","OP_ADD_ASSIGN","\"+=\"","OP_SUB_ASSIGN","\"-=\"",
    "OP_SHIFT_LEFT_ASSIGN","\"<<=\"","OP_SHIFT_RIGHT_ASSIGN","\">>=\"",
    "OP_AND_ASSIGN","\"&=\"","OP_XOR_ASSIGN","\"^=\"","OP_OR_ASSIGN",
    "\"|=\"","OP_PTR","\"->\"","LITERAL_INTEGER","\"int literal\"",
    "LITERAL_FLOAT","\"float literal\"","LITERAL_DOUBLE",
    "\"double literal\"","LITERAL_DECIMAL","\"decimal literal\"",
    "LITERAL_CHARACTER","\"character literal\"","LITERAL_STRING",
    "\"string literal\"","IDENTIFIER",
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
#line 240 "cs-parser.jay"
  {
		/* At some point check that using only comes *before* any namespaces*/
	  }
  break;
case 6:
#line 257 "cs-parser.jay"
  {
	  }
  break;
case 7:
#line 263 "cs-parser.jay"
  {
		current_namespace.Using ((string) yyVals[-1+yyTop]);
          }
  break;
case 10:
#line 274 "cs-parser.jay"
  {
		current_namespace = new Namespace (current_namespace, (string) yyVals[0+yyTop]); 
	  }
  break;
case 11:
#line 278 "cs-parser.jay"
  { 
		current_namespace = current_namespace.Parent;
	  }
  break;
case 17:
#line 295 "cs-parser.jay"
  { 
	    yyVal = ((yyVals[-2+yyTop]).ToString ()) + "." + (yyVals[0+yyTop].ToString ()); }
  break;
case 19:
#line 308 "cs-parser.jay"
  {
	  }
  break;
case 26:
#line 329 "cs-parser.jay"
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

		/**/
		/* We remove this error until we can */
		/*if ((mod_flags & (Modifiers.PRIVATE|Modifiers.PROTECTED)) != 0){*/
		/*	error (1527, "Namespace elements cant be explicitly " +*/
		/*	             "declared private or protected in `" + name + "'");*/
		/*}*/
	  }
  break;
case 42:
#line 396 "cs-parser.jay"
  { 
	     /* if (Collection.Contains ($$))... FIXME*/
	     note  ("Allows: assembly, field, method, module, param, property, type"); 
	}
  break;
case 49:
#line 415 "cs-parser.jay"
  { /* reserved attribute name or identifier: 17.4 */ }
  break;
case 73:
#line 471 "cs-parser.jay"
  { 
		Struct new_struct;
		string full_struct_name = MakeName ((string) yyVals[0+yyTop]);

		new_struct = new Struct (current_container, full_struct_name, (int) yyVals[-2+yyTop]);
		current_container = new_struct;
		current_container.Namespace = current_namespace;
	  }
  break;
case 74:
#line 482 "cs-parser.jay"
  {
		Struct new_struct = (Struct) current_container;

		current_container = current_container.Parent;
		CheckDef (current_container.AddStruct (new_struct), new_struct.Name);
		yyVal = new_struct;
	  }
  break;
case 94:
#line 538 "cs-parser.jay"
  { 
		Modifiers.Check (Constant.AllowedModifiers, (int) yyVals[-4+yyTop], Modifiers.PRIVATE);

		foreach (DictionaryEntry constant in (ArrayList) yyVals[-1+yyTop]){
			Constant c = new Constant (
				(TypeRef) yyVals[-2+yyTop], (string) constant.Key, 
				(Expression) constant.Value);

			CheckDef (current_container.AddConstant (c), c.Name);
		}
	  }
  break;
case 95:
#line 553 "cs-parser.jay"
  {
		ArrayList constants = new ArrayList ();
		constants.Add (yyVals[0+yyTop]);
		yyVal = constants;
	  }
  break;
case 96:
#line 559 "cs-parser.jay"
  {
		ArrayList constants = (ArrayList) yyVals[-2+yyTop];

		constants.Add (yyVals[0+yyTop]);
	  }
  break;
case 97:
#line 567 "cs-parser.jay"
  {
		yyVal = new DictionaryEntry (yyVals[-2+yyTop], yyVals[0+yyTop]);
	  }
  break;
case 98:
#line 578 "cs-parser.jay"
  { 
		TypeRef typeref = (TypeRef) yyVals[-2+yyTop];
		int mod = (int) yyVals[-3+yyTop];

		foreach (VariableDeclaration var in (ArrayList) yyVals[-1+yyTop]){
			Field field = new Field (typeref, mod, var.identifier, 
						 var.expression_or_array_initializer);

			CheckDef (current_container.AddField (field), field.Name);
		}
	}
  break;
case 99:
#line 594 "cs-parser.jay"
  {
		ArrayList decl = new ArrayList ();
		yyVal = decl;
		decl.Add (yyVals[0+yyTop]);
	  }
  break;
case 100:
#line 600 "cs-parser.jay"
  {
		ArrayList decls = (ArrayList) yyVals[-2+yyTop];
		decls.Add (yyVals[0+yyTop]);
		yyVal = yyVals[-2+yyTop];
	  }
  break;
case 101:
#line 609 "cs-parser.jay"
  {
		yyVal = new VariableDeclaration ((string) yyVals[-2+yyTop], yyVals[0+yyTop]);
	  }
  break;
case 102:
#line 613 "cs-parser.jay"
  {
		yyVal = new VariableDeclaration ((string) yyVals[0+yyTop], null);
	  }
  break;
case 105:
#line 626 "cs-parser.jay"
  {
		Method method = (Method) yyVals[-1+yyTop];

		method.Block = (Block) yyVals[0+yyTop];
		CheckDef (current_container.AddMethod (method), method.Name);

		current_local_parameters = null;
	  }
  break;
case 106:
#line 642 "cs-parser.jay"
  {
		Method method = new Method ((TypeRef) yyVals[-4+yyTop], (int) yyVals[-5+yyTop], (string) yyVals[-3+yyTop], (Parameters) yyVals[-1+yyTop]);

		current_local_parameters = (Parameters) yyVals[-1+yyTop];

		yyVal = method;
	  }
  break;
case 107:
#line 654 "cs-parser.jay"
  {
		Method method = new Method (type ("void"), (int) yyVals[-5+yyTop], (string) yyVals[-3+yyTop], (Parameters) yyVals[-1+yyTop]);

		current_local_parameters = (Parameters) yyVals[-1+yyTop];
		yyVal = method;
	  }
  break;
case 109:
#line 664 "cs-parser.jay"
  { yyVal = null; }
  break;
case 110:
#line 668 "cs-parser.jay"
  { yyVal = new Parameters (null, null); }
  break;
case 112:
#line 674 "cs-parser.jay"
  { 
	  	yyVal = new Parameters ((ParameterCollection) yyVals[0+yyTop], null); 
	  }
  break;
case 113:
#line 678 "cs-parser.jay"
  {
		yyVal = new Parameters ((ParameterCollection) yyVals[-2+yyTop], (Parameter) yyVals[0+yyTop]); 
	  }
  break;
case 114:
#line 682 "cs-parser.jay"
  {
		yyVal = new Parameters (null, (Parameter) yyVals[0+yyTop]);
	  }
  break;
case 115:
#line 689 "cs-parser.jay"
  {
		ParameterCollection pars = new ParameterCollection ();
		pars.Add ((Parameter) yyVals[0+yyTop]);
		yyVal = pars;
	  }
  break;
case 116:
#line 695 "cs-parser.jay"
  {
		ParameterCollection pars = (ParameterCollection) yyVals[-2+yyTop];
		pars.Add ((Parameter) yyVals[0+yyTop]);
		yyVal = yyVals[-2+yyTop];
	  }
  break;
case 117:
#line 707 "cs-parser.jay"
  {
		yyVal = new Parameter ((TypeRef) yyVals[-1+yyTop], (string) yyVals[0+yyTop], (Parameter.Modifier) yyVals[-2+yyTop]);
	  }
  break;
case 118:
#line 713 "cs-parser.jay"
  { yyVal = Parameter.Modifier.NONE; }
  break;
case 120:
#line 718 "cs-parser.jay"
  { yyVal = Parameter.Modifier.REF; }
  break;
case 121:
#line 719 "cs-parser.jay"
  { yyVal = Parameter.Modifier.OUT; }
  break;
case 122:
#line 724 "cs-parser.jay"
  { 
		yyVal = new Parameter ((TypeRef) yyVals[-1+yyTop], (string) yyVals[0+yyTop], Parameter.Modifier.PARAMS);
		note ("type must be a single-dimension array type"); 
	  }
  break;
case 123:
#line 731 "cs-parser.jay"
  { yyVal = yyVals[0+yyTop].ToString (); }
  break;
case 124:
#line 732 "cs-parser.jay"
  { yyVal = yyVals[-2+yyTop].ToString () + "." + yyVals[0+yyTop].ToString (); }
  break;
case 125:
#line 740 "cs-parser.jay"
  {
		Parameter implicit_value_parameter;
		implicit_value_parameter = new Parameter ((TypeRef) yyVals[-2+yyTop], "value", Parameter.Modifier.NONE);

		lexer.properties = true;
		
		implicit_value_parameters = new ParameterCollection ();
		implicit_value_parameters.Add (implicit_value_parameter);
	  }
  break;
case 126:
#line 750 "cs-parser.jay"
  {
		lexer.properties = false;
	  }
  break;
case 127:
#line 754 "cs-parser.jay"
  { 
		Property prop;
		DictionaryEntry pair = (DictionaryEntry) yyVals[-2+yyTop];
		Block get_block = null;
		Block set_block = null;

		if (pair.Key != null)
			get_block = (Block) pair.Key;
		if (pair.Value != null)
			set_block = (Block) pair.Value;

		prop = new Property ((TypeRef) yyVals[-6+yyTop], (string) yyVals[-5+yyTop], (int) yyVals[-7+yyTop], get_block, set_block);
		
		CheckDef (current_container.AddProperty (prop), prop.Name);
		implicit_value_parameters = null;
	  }
  break;
case 128:
#line 774 "cs-parser.jay"
  { 
		yyVal = new DictionaryEntry (yyVals[-1+yyTop], yyVals[0+yyTop]);
	  }
  break;
case 129:
#line 778 "cs-parser.jay"
  {
		yyVal = new DictionaryEntry (yyVals[0+yyTop], yyVals[-1+yyTop]);
	  }
  break;
case 130:
#line 784 "cs-parser.jay"
  { yyVal = null; }
  break;
case 132:
#line 789 "cs-parser.jay"
  { yyVal = null; }
  break;
case 134:
#line 795 "cs-parser.jay"
  {
		yyVal = yyVals[0+yyTop];
	  }
  break;
case 135:
#line 802 "cs-parser.jay"
  { 
		current_local_parameters = new Parameters (implicit_value_parameters, null);
	  }
  break;
case 136:
#line 806 "cs-parser.jay"
  {
		yyVal = yyVals[0+yyTop];
		current_local_parameters = null;
	  }
  break;
case 138:
#line 814 "cs-parser.jay"
  { yyVal = new Block (null); }
  break;
case 139:
#line 821 "cs-parser.jay"
  {
		Interface new_interface;
		string full_interface_name = MakeName ((string) yyVals[0+yyTop]);

		new_interface = new Interface (current_container, full_interface_name, (int) yyVals[-2+yyTop]);
		if (current_interface != null)
			error (-2, "Internal compiler error: interface inside interface");
		current_interface = new_interface;
	  }
  break;
case 140:
#line 832 "cs-parser.jay"
  { 
		Interface new_interface = (Interface) current_interface;

		if (yyVals[-1+yyTop] != null)
			new_interface.Bases = (ArrayList) yyVals[-1+yyTop];

		current_interface = null;
		CheckDef (current_container.AddInterface (new_interface), new_interface.Name);
	  }
  break;
case 141:
#line 844 "cs-parser.jay"
  { yyVal = null; }
  break;
case 143:
#line 849 "cs-parser.jay"
  { yyVal = yyVals[0+yyTop]; }
  break;
case 144:
#line 854 "cs-parser.jay"
  {
		ArrayList interfaces = new ArrayList ();

		interfaces.Add (yyVals[0+yyTop]);
	  }
  break;
case 145:
#line 860 "cs-parser.jay"
  {
		ArrayList interfaces = (ArrayList) yyVals[-2+yyTop];
		interfaces.Add (yyVals[0+yyTop]);
		yyVal = interfaces;
	  }
  break;
case 151:
#line 885 "cs-parser.jay"
  { 
		InterfaceMethod m = (InterfaceMethod) yyVals[0+yyTop];

		CheckDef (current_interface.AddMethod (m), m.Name);
	  }
  break;
case 152:
#line 891 "cs-parser.jay"
  { 
		InterfaceProperty p = (InterfaceProperty) yyVals[0+yyTop];

		CheckDef (current_interface.AddProperty (p), p.Name);
          }
  break;
case 153:
#line 897 "cs-parser.jay"
  { 
		InterfaceEvent e = (InterfaceEvent) yyVals[0+yyTop];

		CheckDef (current_interface.AddEvent (e), e.Name);
	  }
  break;
case 154:
#line 903 "cs-parser.jay"
  { 
		InterfaceIndexer i = (InterfaceIndexer) yyVals[0+yyTop];

		CheckDef (current_interface.AddIndexer (i), "indexer");
	  }
  break;
case 155:
#line 911 "cs-parser.jay"
  { yyVal = false; }
  break;
case 156:
#line 912 "cs-parser.jay"
  { yyVal = true; }
  break;
case 157:
#line 919 "cs-parser.jay"
  {
		yyVal = new InterfaceMethod ((TypeRef) yyVals[-5+yyTop], (string) yyVals[-4+yyTop], (bool) yyVals[-6+yyTop], (Parameters) yyVals[-2+yyTop]);
	  }
  break;
case 158:
#line 925 "cs-parser.jay"
  {
		yyVal = new InterfaceMethod (type ("void"), (string) yyVals[-4+yyTop], (bool) yyVals[-6+yyTop], (Parameters) yyVals[-2+yyTop]);
	  }
  break;
case 159:
#line 935 "cs-parser.jay"
  { lexer.properties = true; }
  break;
case 160:
#line 937 "cs-parser.jay"
  { lexer.properties = false; }
  break;
case 161:
#line 939 "cs-parser.jay"
  {
	        int gs = (int) yyVals[-2+yyTop];

		yyVal = new InterfaceProperty ((TypeRef) yyVals[-6+yyTop], (string) yyVals[-5+yyTop], (bool) yyVals[-7+yyTop], 
					    (gs & 1) == 1, (gs & 2) == 2);
	  }
  break;
case 162:
#line 948 "cs-parser.jay"
  { yyVal = 1; }
  break;
case 163:
#line 949 "cs-parser.jay"
  { yyVal = 2; }
  break;
case 164:
#line 951 "cs-parser.jay"
  { yyVal = 3; }
  break;
case 165:
#line 953 "cs-parser.jay"
  { yyVal = 3; }
  break;
case 166:
#line 958 "cs-parser.jay"
  {
		yyVal = new InterfaceEvent ((TypeRef) yyVals[-2+yyTop], (string) yyVals[-1+yyTop], (bool) yyVals[-4+yyTop]);
	  }
  break;
case 167:
#line 967 "cs-parser.jay"
  { lexer.properties = true; }
  break;
case 168:
#line 969 "cs-parser.jay"
  { lexer.properties = false; }
  break;
case 169:
#line 971 "cs-parser.jay"
  {
		int a_flags = (int) yyVals[-2+yyTop];

	  	bool do_get = (a_flags & 1) == 1;
		bool do_set = (a_flags & 2) == 2;

		yyVal = new InterfaceIndexer ((TypeRef) yyVals[-9+yyTop], (Parameters) yyVals[-6+yyTop], do_get, do_set, (bool) yyVals[-10+yyTop]);
	  }
  break;
case 170:
#line 983 "cs-parser.jay"
  {
		/* FIXME: validate that opt_modifiers is exactly: PUBLIC and STATIC*/
	  }
  break;
case 171:
#line 991 "cs-parser.jay"
  {
		/* FIXME: since reduce/reduce on this*/
	 	/* rule, validate overloadable_operator is unary*/
	  }
  break;
case 172:
#line 1000 "cs-parser.jay"
  {
		/* FIXME: because of the reduce/reduce on PLUS and MINUS*/
		/* validate overloadable_operator is binary*/
	  }
  break;
case 198:
#line 1045 "cs-parser.jay"
  { 
		Constructor c = (Constructor) yyVals[-1+yyTop];
		c.Block = (Block) yyVals[0+yyTop];
		c.ModFlags = (int) yyVals[-2+yyTop];

		if ((c.ModFlags & Modifiers.STATIC) != 0){
			if ((c.ModFlags & Modifiers.Accessibility) != 0){
				error (515, "Access modifiers are not allowed on static constructors");
			}
		}
		CheckDef (current_container.AddConstructor (c), c.Name);

		current_local_parameters = null;
	  }
  break;
case 199:
#line 1065 "cs-parser.jay"
  {
		ConstructorInitializer i = null;

		if (yyVals[0+yyTop] != null)
			i = (ConstructorInitializer) yyVals[0+yyTop];

		yyVal = new Constructor ((string) yyVals[-4+yyTop], (Parameters) yyVals[-2+yyTop], i);
	
		current_local_parameters = (Parameters) yyVals[-2+yyTop];
	  }
  break;
case 200:
#line 1078 "cs-parser.jay"
  { yyVal = null; }
  break;
case 202:
#line 1084 "cs-parser.jay"
  {
		yyVal = new ConstructorBaseInitializer ((ArrayList) yyVals[-1+yyTop]);
	  }
  break;
case 203:
#line 1088 "cs-parser.jay"
  {
		yyVal = new ConstructorThisInitializer ((ArrayList) yyVals[-1+yyTop]);
	  }
  break;
case 204:
#line 1095 "cs-parser.jay"
  {
		Method d = new Method (type ("void"), 0, "Finalize", new Parameters (null, null));

		d.Block = (Block) yyVals[0+yyTop];
		CheckDef (current_container.AddMethod (d), d.Name);
	  }
  break;
case 205:
#line 1107 "cs-parser.jay"
  { note ("validate that the flags only contain new public protected internal private static virtual sealed override abstract"); }
  break;
case 206:
#line 1112 "cs-parser.jay"
  { note ("validate that the flags only contain new public protected internal private static virtual sealed override abstract"); }
  break;
case 211:
#line 1131 "cs-parser.jay"
  { 
		/* The signature is computed from the signature of the indexer.  Look*/
	 	/* at section 3.6 on the spec*/
		note ("verify modifiers are NEW PUBLIC PROTECTED INTERNAL PRIVATE VIRTUAL SEALED OVERRIDE ABSTRACT"); 
	  }
  break;
case 214:
#line 1150 "cs-parser.jay"
  { 
		string name = (string) yyVals[-3+yyTop];
		Enum e = new Enum ((TypeRef) yyVals[-2+yyTop], (int) yyVals[-5+yyTop], name);

		foreach (VariableDeclaration ev in (ArrayList) yyVals[-1+yyTop]){
			CheckDef (
				e.AddEnum (
					ev.identifier, 
					(Expression) ev.expression_or_array_initializer),
				ev.identifier);
		}

		CheckDef (current_container.AddEnum (e), name);
	  }
  break;
case 215:
#line 1167 "cs-parser.jay"
  { yyVal = type ("System.Int32"); }
  break;
case 216:
#line 1168 "cs-parser.jay"
  { yyVal = yyVals[0+yyTop];   }
  break;
case 217:
#line 1173 "cs-parser.jay"
  {
		yyVal = yyVals[-1+yyTop];
	  }
  break;
case 218:
#line 1177 "cs-parser.jay"
  {
		yyVal = yyVals[-2+yyTop];
	  }
  break;
case 219:
#line 1183 "cs-parser.jay"
  { yyVal = new ArrayList (); }
  break;
case 220:
#line 1184 "cs-parser.jay"
  { yyVal = yyVals[0+yyTop]; }
  break;
case 221:
#line 1189 "cs-parser.jay"
  {
		ArrayList l = new ArrayList ();

		l.Add (yyVals[0+yyTop]);
		yyVal = l;
	  }
  break;
case 222:
#line 1196 "cs-parser.jay"
  {
		ArrayList l = (ArrayList) yyVals[-2+yyTop];

		l.Add (yyVals[0+yyTop]);

		yyVal = l;
	  }
  break;
case 223:
#line 1207 "cs-parser.jay"
  {
		yyVal = new VariableDeclaration ((string) yyVals[0+yyTop], null);
	  }
  break;
case 224:
#line 1211 "cs-parser.jay"
  { 
		yyVal = new VariableDeclaration ((string) yyVals[-2+yyTop], yyVals[0+yyTop]);
	  }
  break;
case 225:
#line 1224 "cs-parser.jay"
  { note ("validate that modifiers only contains NEW PUBLIC PROTECTED INTERNAL PRIVATE"); }
  break;
case 228:
#line 1242 "cs-parser.jay"
  {  	/* class_type */
		/* 
	           This does interfaces, delegates, struct_types, class_types, 
	           parent classes, and more! 4.2 
	         */
		yyVal = type ((string) yyVals[0+yyTop]); 
	  }
  break;
case 231:
#line 1255 "cs-parser.jay"
  {
		ArrayList types = new ArrayList ();

		types.Add (yyVals[0+yyTop]);
		yyVal = types;
	  }
  break;
case 232:
#line 1262 "cs-parser.jay"
  {
		ArrayList types = new ArrayList ();
		types.Add (yyVals[0+yyTop]);
		yyVal = types;
	  }
  break;
case 233:
#line 1274 "cs-parser.jay"
  { yyVal = type ("System.Object"); }
  break;
case 234:
#line 1275 "cs-parser.jay"
  { yyVal = type ("System.String"); }
  break;
case 235:
#line 1276 "cs-parser.jay"
  { yyVal = type ("System.Boolean"); }
  break;
case 236:
#line 1277 "cs-parser.jay"
  { yyVal = type ("System.Decimal"); }
  break;
case 237:
#line 1278 "cs-parser.jay"
  { yyVal = type ("System.Single"); }
  break;
case 238:
#line 1279 "cs-parser.jay"
  { yyVal = type ("System.Double"); }
  break;
case 240:
#line 1284 "cs-parser.jay"
  { yyVal = type ("System.SByte"); }
  break;
case 241:
#line 1285 "cs-parser.jay"
  { yyVal = type ("System.Byte"); }
  break;
case 242:
#line 1286 "cs-parser.jay"
  { yyVal = type ("System.Int16"); }
  break;
case 243:
#line 1287 "cs-parser.jay"
  { yyVal = type ("System.UInt16"); }
  break;
case 244:
#line 1288 "cs-parser.jay"
  { yyVal = type ("System.Int32"); }
  break;
case 245:
#line 1289 "cs-parser.jay"
  { yyVal = type ("System.UInt32"); }
  break;
case 246:
#line 1290 "cs-parser.jay"
  { yyVal = type ("System.Int64"); }
  break;
case 247:
#line 1291 "cs-parser.jay"
  { yyVal = type ("System.UInt64"); }
  break;
case 248:
#line 1292 "cs-parser.jay"
  { yyVal = type ("System.Char"); }
  break;
case 250:
#line 1301 "cs-parser.jay"
  {
		yyVal = yyVals[-1+yyTop];
		/* FIXME: We need to create a type for the nested thing.*/
	  }
  break;
case 251:
#line 1312 "cs-parser.jay"
  {
		/* 7.5.1: Literals*/
		
	  }
  break;
case 252:
#line 1318 "cs-parser.jay"
  {
		string name = (string) yyVals[0+yyTop];

		yyVal = null;
		if (name.IndexOf ('.') == -1){
			/**/
			/* we need to check against current_block not being null*/
			/* as `expression' is allowed in argument_lists, which */
			/* do not exist inside a block.  */
			/**/
			if (current_block != null){
				if (current_block.IsVariableDefined (name))
					yyVal = new LocalVariableReference (current_block, name);
			}
			if ((yyVal == null) && (current_local_parameters != null)){
				Parameter par = current_local_parameters.GetParameterByName (name);
				if (par != null)
					yyVal = new ParameterReference (current_local_parameters, name);
			}
		}
		if (yyVal == null)
			yyVal = new SimpleName (name);
	  }
  break;
case 269:
#line 1360 "cs-parser.jay"
  { yyVal = new CharLiteral ((char) lexer.Value); }
  break;
case 270:
#line 1361 "cs-parser.jay"
  { yyVal = new StringLiteral ((string) lexer.Value); }
  break;
case 271:
#line 1362 "cs-parser.jay"
  { yyVal = new NullLiteral (); }
  break;
case 272:
#line 1366 "cs-parser.jay"
  { yyVal = new FloatLiteral ((float) lexer.Value); }
  break;
case 273:
#line 1367 "cs-parser.jay"
  { yyVal = new DoubleLiteral ((double) lexer.Value); }
  break;
case 274:
#line 1368 "cs-parser.jay"
  { yyVal = new DecimalLiteral ((decimal) lexer.Value); }
  break;
case 275:
#line 1372 "cs-parser.jay"
  { yyVal = new IntLiteral ((Int32) lexer.Value); }
  break;
case 276:
#line 1376 "cs-parser.jay"
  { yyVal = new BoolLiteral (true); }
  break;
case 277:
#line 1377 "cs-parser.jay"
  { yyVal = new BoolLiteral (false); }
  break;
case 278:
#line 1382 "cs-parser.jay"
  { yyVal = yyVals[-1+yyTop]; }
  break;
case 279:
#line 1387 "cs-parser.jay"
  {
		yyVal = new MemberAccess ((Expression) yyVals[-2+yyTop], (string) yyVals[0+yyTop]);
	  }
  break;
case 280:
#line 1391 "cs-parser.jay"
  {
		yyVal = new BuiltinTypeAccess ((TypeRef) yyVals[-2+yyTop], (string) yyVals[0+yyTop]);
	  }
  break;
case 282:
#line 1402 "cs-parser.jay"
  {
		/* FIXME:*/
		/* if $1 is MethodGroup*/
		/*	$$ = new Call ($1, $3);*/
		/* else */
		/* 	$$ = new DelegateCall ($1, $3);*/
		if (yyVals[-3+yyTop] == null)
			error (1, "THIS IS CRAZY");

		yyVal = new Invocation ((Expression) yyVals[-3+yyTop], (ArrayList) yyVals[-1+yyTop]);
	  }
  break;
case 283:
#line 1416 "cs-parser.jay"
  { yyVal = new ArrayList (); }
  break;
case 285:
#line 1422 "cs-parser.jay"
  { 
		ArrayList list = new ArrayList ();
		list.Add (yyVals[0+yyTop]);
		yyVal = list;
	  }
  break;
case 286:
#line 1428 "cs-parser.jay"
  {
		ArrayList list = (ArrayList) yyVals[-2+yyTop];
		list.Add (yyVals[0+yyTop]);
		yyVal = list;
	  }
  break;
case 287:
#line 1437 "cs-parser.jay"
  {
		yyVal = new Argument ((Expression) yyVals[0+yyTop], Argument.AType.Expression);
	  }
  break;
case 288:
#line 1441 "cs-parser.jay"
  { 
		yyVal = new Argument ((Expression) yyVals[0+yyTop], Argument.AType.Ref);
	  }
  break;
case 289:
#line 1445 "cs-parser.jay"
  { 
		yyVal = new Argument ((Expression) yyVals[0+yyTop], Argument.AType.Out);
	  }
  break;
case 290:
#line 1451 "cs-parser.jay"
  { note ("section 5.4"); yyVal = yyVals[0+yyTop]; }
  break;
case 294:
#line 1466 "cs-parser.jay"
  {
		yyVal = new This ();
	  }
  break;
case 297:
#line 1480 "cs-parser.jay"
  {
		yyVal = new Unary (Unary.Operator.PostIncrement, (Expression) yyVals[-1+yyTop]);
	  }
  break;
case 298:
#line 1487 "cs-parser.jay"
  {
		yyVal = new Unary (Unary.Operator.PostDecrement, (Expression) yyVals[-1+yyTop]);
	  }
  break;
case 301:
#line 1499 "cs-parser.jay"
  {
		yyVal = new New ((TypeRef) yyVals[-3+yyTop], (ArrayList) yyVals[-1+yyTop]);
	  }
  break;
case 319:
#line 1559 "cs-parser.jay"
  {
		yyVal = new TypeOf ((TypeRef) yyVals[-1+yyTop]);
	  }
  break;
case 320:
#line 1565 "cs-parser.jay"
  { 
		yyVal = new SizeOf ((TypeRef) yyVals[-1+yyTop]);

		note ("Verify type is unmanaged"); 
		note ("if (5.8) builtin, yield constant expression");
	  }
  break;
case 324:
#line 1583 "cs-parser.jay"
  { 
	  	yyVal = new Unary (Unary.Operator.Plus, (Expression) yyVals[0+yyTop]);
	  }
  break;
case 325:
#line 1587 "cs-parser.jay"
  { 
		yyVal = new Unary (Unary.Operator.Minus, (Expression) yyVals[0+yyTop]);
	  }
  break;
case 326:
#line 1591 "cs-parser.jay"
  {
		yyVal = new Unary (Unary.Operator.Negate, (Expression) yyVals[0+yyTop]);
	  }
  break;
case 327:
#line 1595 "cs-parser.jay"
  {
		yyVal = new Unary (Unary.Operator.BitComplement, (Expression) yyVals[0+yyTop]);
	  }
  break;
case 328:
#line 1599 "cs-parser.jay"
  {
		yyVal = new Unary (Unary.Operator.Indirection, (Expression) yyVals[0+yyTop]);
	  }
  break;
case 329:
#line 1603 "cs-parser.jay"
  {
		yyVal = new Unary (Unary.Operator.AddressOf, (Expression) yyVals[0+yyTop]);
	  }
  break;
case 333:
#line 1618 "cs-parser.jay"
  {
		yyVal = new Unary (Unary.Operator.PreIncrement, (Expression) yyVals[0+yyTop]);
	  }
  break;
case 334:
#line 1625 "cs-parser.jay"
  {
		yyVal = new Unary (Unary.Operator.PreDecrement, (Expression) yyVals[0+yyTop]);
	  }
  break;
case 335:
#line 1635 "cs-parser.jay"
  {
		yyVal = new Cast (type ((string) yyVals[-2+yyTop]), (Expression) yyVals[0+yyTop]);
	  }
  break;
case 336:
#line 1639 "cs-parser.jay"
  {
		yyVal = new Cast ((TypeRef) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 338:
#line 1647 "cs-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.Multiply, 
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 339:
#line 1652 "cs-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.Divide, 
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 340:
#line 1657 "cs-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.Modulo, 
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 342:
#line 1666 "cs-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.Add, 
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 343:
#line 1671 "cs-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.Substract, 
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 345:
#line 1680 "cs-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.ShiftLeft, 
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 346:
#line 1685 "cs-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.ShiftRight, 
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 348:
#line 1694 "cs-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.LessThan, 
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 349:
#line 1699 "cs-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.GreatherThan, 
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 350:
#line 1704 "cs-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.LessOrEqual, 
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 351:
#line 1709 "cs-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.GreatherOrEqual, 
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 352:
#line 1714 "cs-parser.jay"
  {
		yyVal = new Probe (Probe.Operator.Is, 
			         (Expression) yyVals[-2+yyTop], (TypeRef) yyVals[0+yyTop]);
	  }
  break;
case 353:
#line 1719 "cs-parser.jay"
  {
		yyVal = new Probe (Probe.Operator.As, 
			         (Expression) yyVals[-2+yyTop], (TypeRef) yyVals[0+yyTop]);
	  }
  break;
case 355:
#line 1728 "cs-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.Equal, 
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 356:
#line 1733 "cs-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.NotEqual, 
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 358:
#line 1742 "cs-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.BitwiseAnd, 
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 360:
#line 1751 "cs-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.ExclusiveOr, 
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 362:
#line 1760 "cs-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.BitwiseOr, 
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 364:
#line 1769 "cs-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.LogicalAnd, 
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 366:
#line 1778 "cs-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.LogicalOr, 
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 368:
#line 1787 "cs-parser.jay"
  {
		yyVal = new Conditional ((Expression) yyVals[-4+yyTop], (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 369:
#line 1794 "cs-parser.jay"
  {
		yyVal = new Assign ((Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 370:
#line 1798 "cs-parser.jay"
  {
		yyVal = new Assign ((Expression) yyVals[-2+yyTop],
				 new Binary (Binary.Operator.Multiply, 
					     (Expression) yyVals[-2+yyTop],
					     (Expression) yyVals[0+yyTop]));
	  }
  break;
case 371:
#line 1805 "cs-parser.jay"
  {
		yyVal = new Assign ((Expression) yyVals[-2+yyTop],
				 new Binary (Binary.Operator.Divide, 
					     (Expression) yyVals[-2+yyTop],
					     (Expression) yyVals[0+yyTop]));
	  }
  break;
case 372:
#line 1812 "cs-parser.jay"
  {
		yyVal = new Assign ((Expression) yyVals[-2+yyTop],
				 new Binary (Binary.Operator.Modulo, 
					     (Expression) yyVals[-2+yyTop],
					     (Expression) yyVals[0+yyTop]));
	  }
  break;
case 373:
#line 1819 "cs-parser.jay"
  {
		yyVal = new Assign ((Expression) yyVals[-2+yyTop],
				 new Binary (Binary.Operator.Add, 
					     (Expression) yyVals[-2+yyTop],
					     (Expression) yyVals[0+yyTop]));
	  }
  break;
case 374:
#line 1826 "cs-parser.jay"
  {
		yyVal = new Assign ((Expression) yyVals[-2+yyTop],
				 new Binary (Binary.Operator.Substract, 
					     (Expression) yyVals[-2+yyTop],
					     (Expression) yyVals[0+yyTop]));
	  }
  break;
case 375:
#line 1833 "cs-parser.jay"
  {
		yyVal = new Assign ((Expression) yyVals[-2+yyTop],
				 new Binary (Binary.Operator.ShiftLeft, 
					     (Expression) yyVals[-2+yyTop],
					     (Expression) yyVals[0+yyTop]));
	  }
  break;
case 376:
#line 1840 "cs-parser.jay"
  {
		yyVal = new Assign ((Expression) yyVals[-2+yyTop],
				 new Binary (Binary.Operator.ShiftRight, 
					     (Expression) yyVals[-2+yyTop],
					     (Expression) yyVals[0+yyTop]));
	  }
  break;
case 377:
#line 1847 "cs-parser.jay"
  {
		yyVal = new Assign ((Expression) yyVals[-2+yyTop],
				 new Binary (Binary.Operator.BitwiseAnd, 
					     (Expression) yyVals[-2+yyTop],
					     (Expression) yyVals[0+yyTop]));
	  }
  break;
case 378:
#line 1854 "cs-parser.jay"
  {
		yyVal = new Assign ((Expression) yyVals[-2+yyTop],
				 new Binary (Binary.Operator.BitwiseOr, 
					     (Expression) yyVals[-2+yyTop],
					     (Expression) yyVals[0+yyTop]));
	  }
  break;
case 379:
#line 1861 "cs-parser.jay"
  {
		yyVal = new Assign ((Expression) yyVals[-2+yyTop],
				 new Binary (Binary.Operator.ExclusiveOr, 
					     (Expression) yyVals[-2+yyTop],
					     (Expression) yyVals[0+yyTop]));
	  }
  break;
case 383:
#line 1879 "cs-parser.jay"
  { CheckBoolean ((Expression) yyVals[0+yyTop]); yyVal = yyVals[0+yyTop]; }
  break;
case 384:
#line 1889 "cs-parser.jay"
  {
		Class new_class;
		string full_class_name = MakeName ((string) yyVals[0+yyTop]);

		new_class = new Class (current_container, full_class_name, (int) yyVals[-2+yyTop]);
		current_container = new_class;
		current_container.Namespace = current_namespace;
	  }
  break;
case 385:
#line 1900 "cs-parser.jay"
  {
		Class new_class = (Class) current_container;

		if (yyVals[-2+yyTop] != null)
			new_class.Bases = (ArrayList) yyVals[-2+yyTop];

		current_container = current_container.Parent;
		CheckDef (current_container.AddClass (new_class), new_class.Name);

		yyVal = new_class;
	  }
  break;
case 386:
#line 1914 "cs-parser.jay"
  { yyVal = (int) 0; }
  break;
case 389:
#line 1920 "cs-parser.jay"
  { 
		int m1 = (int) yyVals[-1+yyTop];
		int m2 = (int) yyVals[0+yyTop];

		if ((m1 & m2) != 0)
			error (1002, "Duplicate modifier: `" + Modifiers.Name (m2) + "'");

		yyVal = (int) (m1 | m2);
	  }
  break;
case 390:
#line 1932 "cs-parser.jay"
  { yyVal = Modifiers.NEW; }
  break;
case 391:
#line 1933 "cs-parser.jay"
  { yyVal = Modifiers.PUBLIC; }
  break;
case 392:
#line 1934 "cs-parser.jay"
  { yyVal = Modifiers.PROTECTED; }
  break;
case 393:
#line 1935 "cs-parser.jay"
  { yyVal = Modifiers.INTERNAL; }
  break;
case 394:
#line 1936 "cs-parser.jay"
  { yyVal = Modifiers.PRIVATE; }
  break;
case 395:
#line 1937 "cs-parser.jay"
  { yyVal = Modifiers.ABSTRACT; }
  break;
case 396:
#line 1938 "cs-parser.jay"
  { yyVal = Modifiers.SEALED; }
  break;
case 397:
#line 1939 "cs-parser.jay"
  { yyVal = Modifiers.STATIC; }
  break;
case 398:
#line 1940 "cs-parser.jay"
  { yyVal = Modifiers.READONLY; }
  break;
case 399:
#line 1941 "cs-parser.jay"
  { yyVal = Modifiers.VIRTUAL; }
  break;
case 400:
#line 1942 "cs-parser.jay"
  { yyVal = Modifiers.OVERRIDE; }
  break;
case 401:
#line 1943 "cs-parser.jay"
  { yyVal = Modifiers.EXTERN; }
  break;
case 402:
#line 1947 "cs-parser.jay"
  { yyVal = null; }
  break;
case 403:
#line 1948 "cs-parser.jay"
  { yyVal = yyVals[0+yyTop]; }
  break;
case 404:
#line 1952 "cs-parser.jay"
  { yyVal = yyVals[0+yyTop]; }
  break;
case 405:
#line 1970 "cs-parser.jay"
  {
		current_block = new Block (current_block);
	  }
  break;
case 406:
#line 1974 "cs-parser.jay"
  { 
		while (current_block.Implicit)
			current_block = current_block.Parent;
		yyVal = current_block;
		current_block = current_block.Parent;
	  }
  break;
case 411:
#line 1994 "cs-parser.jay"
  {
		if ((Block) yyVals[0+yyTop] != current_block){
			current_block.AddStatement ((Statement) yyVals[0+yyTop]);
			current_block = (Block) yyVals[0+yyTop];
		}
	  }
  break;
case 412:
#line 2001 "cs-parser.jay"
  {
		current_block.AddStatement ((Statement) yyVals[0+yyTop]);
	  }
  break;
case 413:
#line 2005 "cs-parser.jay"
  {
		current_block.AddStatement ((Statement) yyVals[0+yyTop]);
	  }
  break;
case 425:
#line 2026 "cs-parser.jay"
  {
		  yyVal = new EmptyStatement ();
	  }
  break;
case 426:
#line 2033 "cs-parser.jay"
  {
		string lab = (String) yyVals[-2+yyTop];
		Block block;

		block = new Block (current_block, lab);
		block.AddStatement ((Statement) yyVals[0+yyTop]);
		yyVal = block;

		if (!current_block.AddLabel (lab, block)){
			error (140, "The label '" + lab + "' is a duplicate");
			yyVal = yyVals[0+yyTop];
		}	
	  }
  break;
case 429:
#line 2061 "cs-parser.jay"
  { 
		/* FIXME: Do something smart here regarding the composition of the type.*/
		/**/

		/* Ok, the above "primary_expression" is there to get rid of*/
		/* both reduce/reduce and shift/reduces in the grammar, it should*/
		/* really just be "type_name".  If you use type_name, a reduce/reduce*/
		/* creeps up.  If you use qualified_identifier (which is all we need*/
		/* really) two shift/reduces appear.*/
		/* */
		/* So, instead we do a super trick: we just allow ($1) to be a */
		/* SimpleName Expression.*/
		/**/
		if (((Expression) yyVals[-1+yyTop]) is SimpleName)
			yyVal = type (((SimpleName) yyVals[-1+yyTop]).Name);
		else {
			error (-1, "Invalid Type definition");
			yyVal = type ("object");
		}
	  }
  break;
case 430:
#line 2082 "cs-parser.jay"
  {
		/* FIXME: Do something smart with the type here.*/
		yyVal = yyVals[-1+yyTop]; 
	  }
  break;
case 431:
#line 2087 "cs-parser.jay"
  {
		yyVal = type ("VOID SOMETHING TYPE");
	  }
  break;
case 437:
#line 2109 "cs-parser.jay"
  {
		yyVal = declare_local_variables ((TypeRef) yyVals[-1+yyTop], (ArrayList) yyVals[0+yyTop]);
	  }
  break;
case 439:
#line 2121 "cs-parser.jay"
  {
		yyVal = yyVals[-1+yyTop];
	  }
  break;
case 440:
#line 2131 "cs-parser.jay"
  { yyVal = new StatementExpression ((Expression) yyVals[0+yyTop]); }
  break;
case 441:
#line 2132 "cs-parser.jay"
  { yyVal = new StatementExpression ((Expression) yyVals[0+yyTop]); }
  break;
case 442:
#line 2133 "cs-parser.jay"
  { yyVal = new StatementExpression ((Expression) yyVals[0+yyTop]); }
  break;
case 443:
#line 2134 "cs-parser.jay"
  { yyVal = new StatementExpression ((Expression) yyVals[0+yyTop]); }
  break;
case 444:
#line 2135 "cs-parser.jay"
  { yyVal = new StatementExpression ((Expression) yyVals[0+yyTop]); }
  break;
case 445:
#line 2136 "cs-parser.jay"
  { yyVal = new StatementExpression ((Expression) yyVals[0+yyTop]); }
  break;
case 446:
#line 2137 "cs-parser.jay"
  { yyVal = new StatementExpression ((Expression) yyVals[0+yyTop]); }
  break;
case 447:
#line 2142 "cs-parser.jay"
  { note ("complain if this is a delegate maybe?"); }
  break;
case 450:
#line 2153 "cs-parser.jay"
  { 
		yyVal = new If ((Expression) yyVals[-2+yyTop], (Statement) yyVals[0+yyTop]);
	  }
  break;
case 451:
#line 2158 "cs-parser.jay"
  {
		yyVal = new If ((Expression) yyVals[-4+yyTop], (Statement) yyVals[-2+yyTop], (Statement) yyVals[0+yyTop]);
	  }
  break;
case 452:
#line 2166 "cs-parser.jay"
  {
		yyVal = new Switch ((Expression) yyVals[-2+yyTop], (ArrayList) yyVals[0+yyTop]);
	  }
  break;
case 453:
#line 2175 "cs-parser.jay"
  {
		yyVal = yyVals[-1+yyTop];
	  }
  break;
case 454:
#line 2181 "cs-parser.jay"
  { yyVal = new ArrayList (); }
  break;
case 456:
#line 2187 "cs-parser.jay"
  {
		ArrayList sections = new ArrayList ();

		sections.Add (yyVals[0+yyTop]);
		yyVal = sections;
	  }
  break;
case 457:
#line 2194 "cs-parser.jay"
  {
		ArrayList sections = (ArrayList) yyVals[-1+yyTop];

		sections.Add (yyVals[0+yyTop]);
		yyVal = sections;
	  }
  break;
case 458:
#line 2204 "cs-parser.jay"
  {
		current_block = new Block (current_block);
	  }
  break;
case 459:
#line 2208 "cs-parser.jay"
  {
		while (current_block.Implicit)
			current_block = current_block.Parent;
		yyVal = new SwitchSection ((ArrayList) yyVals[-2+yyTop], current_block);
		current_block = current_block.Parent;
	  }
  break;
case 460:
#line 2218 "cs-parser.jay"
  {
		ArrayList labels = new ArrayList ();

		labels.Add (yyVals[0+yyTop]);
		yyVal = labels;
	  }
  break;
case 461:
#line 2225 "cs-parser.jay"
  {
		ArrayList labels = (ArrayList) (yyVals[-1+yyTop]);
		labels.Add (yyVals[0+yyTop]);

		yyVal = labels;
	  }
  break;
case 462:
#line 2234 "cs-parser.jay"
  { yyVal = new SwitchLabel ((Expression) yyVals[-1+yyTop]); }
  break;
case 463:
#line 2235 "cs-parser.jay"
  { yyVal = new SwitchLabel (null); }
  break;
case 468:
#line 2247 "cs-parser.jay"
  {
		yyVal = new While ((Expression) yyVals[-2+yyTop], (Statement) yyVals[0+yyTop]);
	}
  break;
case 469:
#line 2255 "cs-parser.jay"
  {
		yyVal = new Do ((Statement) yyVals[-5+yyTop], (Expression) yyVals[-2+yyTop]);
	  }
  break;
case 470:
#line 2266 "cs-parser.jay"
  {
		yyVal = new For ((Statement) yyVals[-6+yyTop], (Expression) yyVals[-4+yyTop], (Statement) yyVals[-2+yyTop], (Statement) yyVals[0+yyTop]);
	  }
  break;
case 471:
#line 2272 "cs-parser.jay"
  { yyVal = new EmptyStatement (); }
  break;
case 475:
#line 2282 "cs-parser.jay"
  { yyVal = new BoolLiteral (true); }
  break;
case 477:
#line 2287 "cs-parser.jay"
  { yyVal = new EmptyStatement (); }
  break;
case 480:
#line 2297 "cs-parser.jay"
  {
		Block b = new Block (null, true);

		b.AddStatement ((Statement) yyVals[0+yyTop]);
		yyVal = b;
	  }
  break;
case 481:
#line 2304 "cs-parser.jay"
  {
		Block b = (Block) yyVals[-2+yyTop];

		b.AddStatement ((Statement) yyVals[0+yyTop]);
		yyVal = yyVals[-2+yyTop];
	  }
  break;
case 482:
#line 2315 "cs-parser.jay"
  {
		string temp_id = current_block.MakeInternalID ();
		Expression assign_e, ma;
		Statement getcurrent;
		Block foreach_block, child_block;

		foreach_block = new Block (current_block, true);

		foreach_block.AddVariable (type ("IEnumerator"), temp_id);
		foreach_block.AddVariable ((TypeRef) yyVals[-5+yyTop], (string) yyVals[-4+yyTop]);
		assign_e = new Assign (new LocalVariableReference (foreach_block, temp_id), 
				       new Invocation (
						new MemberAccess ((Expression) yyVals[-2+yyTop], "GetEnumerator"), null));
		current_block.AddStatement (new StatementExpression (assign_e));
		ma = new MemberAccess (new LocalVariableReference (foreach_block, temp_id), "MoveNext");
		child_block = new Block (current_block);

		getcurrent = new StatementExpression (
			new Assign (
				new LocalVariableReference (foreach_block, (string) yyVals[-4+yyTop]),
				new Cast (
					(TypeRef) yyVals[-5+yyTop], 
					new MemberAccess (
						new LocalVariableReference (foreach_block, temp_id), "Current"))));

		child_block.AddStatement (getcurrent);
		child_block.AddStatement ((Statement) yyVals[0+yyTop]);
	 	foreach_block.AddStatement (new While (ma, (Statement) child_block));

		yyVal = foreach_block;
	  }
  break;
case 488:
#line 2358 "cs-parser.jay"
  {
		yyVal = new Break ();
	  }
  break;
case 489:
#line 2365 "cs-parser.jay"
  {
		yyVal = new Continue ();
	  }
  break;
case 490:
#line 2372 "cs-parser.jay"
  {
		yyVal = new Goto ((string) yyVals[-1+yyTop]);
	  }
  break;
case 493:
#line 2381 "cs-parser.jay"
  {
		yyVal = new Return ((Expression) yyVals[-1+yyTop]);
	  }
  break;
case 494:
#line 2388 "cs-parser.jay"
  {
		yyVal = new Throw ((Expression) yyVals[-1+yyTop]);
	  }
  break;
case 497:
#line 2400 "cs-parser.jay"
  {
		DictionaryEntry cc = (DictionaryEntry) yyVals[0+yyTop];
		ArrayList s = null;

		if (cc.Key != null)
			s = (ArrayList) cc.Key;

		yyVal = new Try ((Block) yyVals[-1+yyTop], s, (Catch) cc.Value, null);
	  }
  break;
case 498:
#line 2410 "cs-parser.jay"
  {
		yyVal = new Try ((Block) yyVals[-1+yyTop], null, null, (Block) yyVals[0+yyTop]);
	  }
  break;
case 499:
#line 2414 "cs-parser.jay"
  {
		DictionaryEntry cc = (DictionaryEntry) yyVals[-1+yyTop];
		ArrayList s = null;

		if (cc.Key != null)
			s = (ArrayList) cc.Key;

		yyVal = new Try ((Block) yyVals[-2+yyTop], s, (Catch) cc.Value, (Block) yyVals[0+yyTop]);
	  }
  break;
case 500:
#line 2427 "cs-parser.jay"
  {
		DictionaryEntry pair = new DictionaryEntry ();

		pair.Key = yyVals[-1+yyTop]; 
		pair.Value = yyVals[0+yyTop];

		yyVal = pair;
	  }
  break;
case 501:
#line 2436 "cs-parser.jay"
  {
		DictionaryEntry pair = new DictionaryEntry ();
		pair.Key = yyVals[-1+yyTop];
		pair.Value = yyVals[-1+yyTop];

		yyVal = pair;
	  }
  break;
case 502:
#line 2446 "cs-parser.jay"
  { yyVal = null; }
  break;
case 504:
#line 2451 "cs-parser.jay"
  { yyVal = null; }
  break;
case 506:
#line 2457 "cs-parser.jay"
  {
		ArrayList l = new ArrayList ();

		l.Add (yyVals[0+yyTop]);
		yyVal = l;
	  }
  break;
case 507:
#line 2464 "cs-parser.jay"
  {
		ArrayList l = (ArrayList) yyVals[-1+yyTop];

		l.Add (yyVals[0+yyTop]);
		yyVal = l;
	  }
  break;
case 508:
#line 2474 "cs-parser.jay"
  {
		string id = null;

		if (yyVals[-2+yyTop] != null)
			id = (string) yyVals[-2+yyTop];

		yyVal = new Catch ((TypeRef) yyVals[-3+yyTop], id, (Block) yyVals[0+yyTop]);
	  }
  break;
case 509:
#line 2485 "cs-parser.jay"
  { yyVal = null; }
  break;
case 511:
#line 2491 "cs-parser.jay"
  {
		yyVal = new Catch (null, null, (Block) yyVals[0+yyTop]);
	  }
  break;
case 512:
#line 2498 "cs-parser.jay"
  {
		yyVal = yyVals[0+yyTop];
	  }
  break;
case 513:
#line 2505 "cs-parser.jay"
  {
		yyVal = new Checked ((Block) yyVals[0+yyTop]);
	  }
  break;
case 514:
#line 2512 "cs-parser.jay"
  {
		yyVal = new Unchecked ((Block) yyVals[0+yyTop]);
	  }
  break;
case 515:
#line 2519 "cs-parser.jay"
  {
		yyVal = new Lock ((Expression) yyVals[-2+yyTop], (Statement) yyVals[0+yyTop]);
	  }
  break;
#line 2673 "-"
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
    0,    4,    4,    5,    5,    6,    7,   10,   10,   14,
   11,   15,   15,   16,   16,   12,   12,    9,   13,    1,
    1,    3,    3,   17,   17,   18,   18,   19,   19,   19,
   19,   19,    2,    2,   25,   26,   26,   27,   28,   28,
   30,   31,   31,   31,   29,   29,   32,   32,   33,   34,
   34,   36,   36,   38,   38,   39,   39,   40,   41,   41,
   42,   42,   43,   43,   43,   43,   43,   43,   43,   43,
   43,   43,   55,   21,   54,   54,   57,   57,   58,   56,
   60,   60,   61,   61,   62,   62,   62,   62,   62,   62,
   62,   62,   62,   44,   64,   64,   65,   45,   67,   67,
   68,   68,   69,   69,   46,   71,   71,   72,   72,   74,
   74,   76,   76,   76,   77,   77,   79,   80,   80,   81,
   81,   78,   73,   73,   84,   85,   47,   83,   83,   89,
   89,   87,   87,   86,   91,   88,   90,   90,   93,   22,
   92,   92,   95,   96,   96,   94,   97,   97,   98,   98,
   99,   99,   99,   99,  104,  104,  100,  100,  106,  107,
  101,  105,  105,  105,  105,  102,  108,  109,  103,   50,
  110,  110,  110,  111,  111,  111,  111,  111,  111,  111,
  111,  111,  111,  111,  111,  111,  111,  111,  111,  111,
  111,  111,  111,  111,  111,  112,  112,   51,  113,  114,
  114,  115,  115,   52,   48,   48,  117,  117,  118,  119,
   49,  120,  120,   23,  121,  121,  122,  122,  124,  124,
  125,  125,  126,  126,   24,   35,    8,   63,   63,   63,
   59,   59,  127,  127,  127,  127,  127,  127,  127,  123,
  123,  123,  123,  123,  123,  123,  123,  123,   82,  128,
  130,  130,  130,  130,  130,  130,  130,  130,  130,  130,
  130,  130,  130,  130,  130,  131,  131,  131,  131,  131,
  131,  147,  147,  147,  146,  145,  145,  132,  133,  133,
  148,  134,  116,  116,  149,  149,  150,  150,  150,  151,
  135,  152,  152,  136,  137,  137,  138,  139,  140,  140,
  153,  154,  155,  155,  129,  129,  157,  158,  158,  159,
  159,  156,  156,   70,   70,   70,  160,  160,  141,  142,
  143,  144,  161,  161,  161,  161,  161,  161,  161,  161,
  161,  161,  162,  163,  164,  164,  165,  165,  165,  165,
  166,  166,  166,  167,  167,  167,  168,  168,  168,  168,
  168,  168,  168,  169,  169,  169,  170,  170,  171,  171,
  172,  172,  173,  173,  174,  174,  175,  175,  176,  176,
  176,  176,  176,  176,  176,  176,  176,  176,  176,   37,
   37,   66,  177,  179,   20,   53,   53,  180,  180,  181,
  181,  181,  181,  181,  181,  181,  181,  181,  181,  181,
  181,  178,  178,  182,  184,   75,  183,  183,  185,  185,
  186,  186,  186,  188,  188,  188,  188,  188,  188,  188,
  188,  188,  188,  188,  190,  189,  187,  187,  202,  202,
  202,  203,  203,  204,  204,  205,  200,  201,  191,  206,
  206,  206,  206,  206,  206,  206,  207,  192,  192,  208,
  208,  209,  210,  211,  211,  212,  212,  215,  213,  214,
  214,  216,  216,  193,  193,  193,  193,  217,  218,  219,
  221,  221,  224,  224,  222,  222,  223,  223,  226,  225,
  225,  220,  194,  194,  194,  194,  194,  227,  228,  229,
  229,  229,  230,  231,  232,  232,  195,  195,  195,  233,
  233,  236,  236,  237,  237,  235,  235,  239,  240,  240,
  238,  234,  196,  197,  198,  199,  241,
  };
   static  short [] yyLen = {           2,
    4,    1,    2,    1,    1,    5,    3,    1,    2,    0,
    5,    0,    1,    0,    1,    1,    3,    1,    4,    0,
    1,    0,    1,    1,    2,    1,    1,    1,    1,    1,
    1,    1,    0,    1,    1,    1,    2,    4,    0,    1,
    2,    1,    1,    1,    1,    3,    1,    1,    1,    0,
    3,    1,    3,    0,    1,    1,    2,    3,    0,    1,
    1,    2,    1,    1,    1,    1,    1,    1,    1,    1,
    1,    1,    0,    8,    0,    1,    1,    2,    2,    3,
    0,    1,    1,    2,    1,    1,    1,    1,    1,    1,
    1,    1,    1,    6,    1,    3,    3,    5,    1,    3,
    3,    1,    1,    1,    2,    7,    7,    1,    1,    0,
    1,    1,    3,    1,    1,    3,    4,    0,    1,    1,
    1,    4,    1,    3,    0,    0,    9,    2,    2,    0,
    1,    0,    1,    3,    0,    4,    1,    1,    0,    7,
    0,    1,    2,    1,    3,    3,    0,    1,    1,    2,
    1,    1,    1,    1,    0,    1,    8,    8,    0,    0,
    9,    3,    3,    6,    6,    6,    0,    0,   12,    4,
    7,   10,    1,    1,    1,    1,    1,    1,    1,    1,
    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,
    1,    1,    1,    1,    1,    7,    7,    4,    5,    0,
    1,    5,    5,    6,    6,    9,    2,    2,    3,    3,
    6,    5,    7,    7,    0,    2,    3,    4,    0,    1,
    1,    3,    2,    4,    9,    1,    1,    1,    1,    1,
    1,    2,    1,    1,    1,    1,    1,    1,    1,    1,
    1,    1,    1,    1,    1,    1,    1,    1,    1,    2,
    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,
    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,
    1,    1,    1,    1,    1,    1,    1,    3,    3,    3,
    1,    4,    0,    1,    1,    3,    1,    2,    2,    1,
    4,    1,    3,    1,    3,    4,    2,    2,    1,    1,
    5,    7,    0,    1,    1,    2,    3,    0,    1,    1,
    2,    0,    1,    2,    3,    4,    1,    3,    4,    4,
    4,    4,    1,    2,    2,    2,    2,    2,    2,    1,
    1,    1,    2,    2,    4,    4,    1,    3,    3,    3,
    1,    3,    3,    1,    3,    3,    1,    3,    3,    3,
    3,    3,    3,    1,    3,    3,    1,    3,    1,    3,
    1,    3,    1,    3,    1,    3,    1,    5,    3,    3,
    3,    3,    3,    3,    3,    3,    3,    3,    3,    1,
    1,    1,    1,    0,    8,    0,    1,    1,    2,    1,
    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,
    1,    0,    1,    2,    0,    4,    0,    1,    1,    2,
    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,
    1,    1,    1,    1,    1,    3,    2,    2,    2,    2,
    2,    0,    1,    1,    2,    3,    2,    3,    2,    1,
    1,    1,    1,    1,    1,    1,    1,    1,    1,    5,
    7,    5,    3,    0,    1,    1,    2,    0,    3,    1,
    2,    3,    2,    1,    1,    1,    1,    5,    7,    9,
    0,    1,    1,    1,    0,    1,    0,    1,    1,    1,
    3,    8,    1,    1,    1,    1,    1,    2,    2,    3,
    4,    3,    3,    3,    0,    1,    3,    3,    4,    2,
    2,    0,    1,    0,    1,    1,    2,    6,    0,    1,
    2,    2,    2,    2,    5,    5,    2,
  };
   static  short [] yyDefRed = {            0,
    0,    0,    0,    0,    2,    4,    5,    0,   18,    0,
    0,    0,    0,   34,    0,   36,    3,    0,    7,    0,
   43,   44,   42,    0,   40,    0,    0,    0,    0,   27,
    0,   24,   26,   28,   29,   30,   31,   32,   37,   16,
    0,   17,    0,  226,    0,   45,   47,   48,   49,   41,
    0,  395,  401,  393,  390,  400,  394,  392,  391,  398,
  396,  397,  399,    0,    0,  388,    1,   25,    6,    0,
  235,  241,  248,    0,  236,  238,  277,  237,  244,  246,
    0,  271,  233,  240,  242,    0,  234,  294,  276,    0,
  245,  247,    0,  243,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  275,  272,  273,  274,  269,  270,    0,
    0,   52,  239,  281,    0,  251,  253,  254,  255,  256,
  257,  258,  259,  260,  261,  262,  263,  264,  265,  266,
  267,  268,    0,  299,  300,    0,  330,  331,  332,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  380,
  381,   38,    0,    0,    0,    0,    0,    0,    0,  389,
    0,    0,    0,  228,    0,  229,  230,    0,    0,    0,
    0,    0,    0,  327,  324,  325,  326,  329,  328,  333,
  334,   51,    0,    0,    0,    0,  297,  298,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,   46,    0,    0,  384,    0,    0,  139,   73,  292,
    0,  295,    0,    0,    0,    0,  305,    0,    0,    0,
    0,  278,    0,   53,    0,    0,    0,  287,    0,    0,
  285,  279,  280,  369,  370,  371,  372,  373,  374,  375,
  376,  377,  379,  378,  338,  340,  339,  337,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,   13,   11,    0,
    0,    0,    0,    0,    0,    0,  296,    0,  321,  310,
    0,    0,    0,    0,  306,  320,  319,  322,  335,  336,
  291,  290,  289,  288,  282,    0,    0,    0,    0,    0,
  403,    0,  216,    0,    0,    0,    0,  142,    0,    0,
    0,   77,  293,    0,  307,  311,  301,  286,  368,   19,
    0,    0,    0,    0,    0,    0,    0,  114,  115,    0,
    0,    0,  221,  214,  249,  144,    0,    0,  140,    0,
    0,    0,   78,    0,    0,    0,    0,   72,    0,    0,
   61,   63,   64,   65,   66,   67,   68,   69,   70,   71,
    0,  385,  121,    0,  120,    0,  119,    0,    0,    0,
  217,    0,    0,    0,    0,    0,  149,  151,  152,  153,
  154,    0,   93,   85,   86,   87,   88,   89,   90,   91,
   92,    0,    0,   83,   74,    0,  313,  302,    0,    0,
   58,   62,  405,  109,  105,  108,    0,    0,  225,  113,
  116,    0,  218,  222,  145,  156,    0,  146,  150,   80,
   84,  314,  103,  317,  104,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  173,    0,    0,    0,  122,
  117,  224,    0,    0,    0,  315,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
   99,    0,    0,  170,  198,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  425,    0,  414,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  442,    0,    0,  409,
  411,  412,  413,  415,  416,  417,  418,  419,  420,  421,
  422,  423,  424,    0,    0,    0,    0,  441,  448,  449,
  464,  465,  466,  467,  483,  484,  485,  486,  487,    0,
    0,    0,    0,  316,  318,    0,    0,    0,   95,    0,
    0,    0,    0,    0,    0,    0,  111,  179,  178,  175,
  180,  181,  174,  193,  192,  185,  186,  182,  184,  183,
  187,  176,  177,  188,  189,  195,  194,  190,  191,    0,
    0,    0,    0,   98,  125,    0,    0,    0,    0,    0,
    0,  488,  513,    0,  489,    0,    0,    0,    0,    0,
    0,    0,    0,  496,    0,    0,    0,    0,  514,    0,
    0,  431,    0,  434,    0,    0,  430,    0,  429,  406,
  410,  427,  428,    0,    0,  439,    0,    0,    0,  159,
    0,  204,    0,    0,   94,  205,    0,    0,    0,    0,
  124,    0,    0,    0,  101,  100,    0,    0,    0,    0,
  135,  211,    0,  128,  133,    0,  131,  129,  438,    0,
  473,  480,    0,  472,    0,    0,  382,    0,  492,  490,
  383,    0,    0,  493,    0,  494,    0,    0,    0,  498,
    0,    0,  506,    0,    0,    0,    0,    0,  435,    0,
  426,  166,    0,    0,    0,    0,   97,   96,    0,    0,
    0,    0,    0,    0,  107,    0,  199,  201,    0,  212,
  126,  106,    0,  138,  137,  134,    0,    0,    0,    0,
    0,  491,    0,    0,    0,    0,  512,  499,    0,  500,
  503,  507,    0,  501,  517,    0,  436,    0,    0,    0,
    0,  160,    0,    0,    0,    0,    0,  207,    0,  208,
    0,    0,    0,    0,    0,    0,    0,  136,    0,  476,
    0,  481,    0,    0,  515,    0,  452,    0,  511,  516,
  468,  158,  167,    0,    0,    0,  157,  209,  210,  206,
  197,  196,    0,    0,  171,    0,  127,  213,    0,    0,
    0,    0,    0,    0,    0,    0,  456,    0,  460,  510,
    0,    0,    0,    0,  161,    0,    0,    0,  469,    0,
    0,  478,    0,  451,    0,  463,  453,  457,    0,  461,
    0,  168,    0,    0,  202,  203,    0,    0,  482,  462,
    0,  508,    0,    0,    0,  172,  470,  169,  164,  165,
  };
  protected static  short [] yyDgoto  = {             2,
    3,  345,   29,    4,    5,    6,    7,   44,   10,    0,
   30,  110,  224,  154,  289,    0,   31,   32,   33,   34,
   35,   36,   37,   38,   14,   15,   16,   24,   45,   25,
   26,   46,   47,   48,  164,  111,  248,    0,    0,  344,
  369,  370,  371,  372,  373,  374,  375,  376,  377,  378,
  379,  380,  420,  330,  296,  362,  331,  332,  341,  412,
  413,  414,  342,  558,  559,  678,  480,  481,  444,  445,
  381,  425,  474,  566,  507,  567,  347,  348,  349,  386,
  387,  475,  599,  657,  766,  600,  664,  601,  668,  726,
  727,  327,  295,  359,  328,  357,  395,  396,  397,  398,
  399,  400,  401,  437,  752,  705,  786,  812,  843,  455,
  590,  456,  457,  717,  718,  249,  710,  711,  712,  458,
  294,  325,  113,  351,  352,  353,  114,  167,  236,  115,
  116,  117,  118,  119,  120,  121,  122,  123,  124,  125,
  126,  127,  128,  129,  130,  131,  132,  133,  250,  251,
  313,  245,  134,  135,  365,  418,  237,  302,  303,  446,
  136,  137,  138,  139,  140,  141,  142,  143,  144,  145,
  146,  147,  148,  149,  150,  151,  682,  320,  290,   65,
   66,  321,  518,  459,  519,  520,  521,  522,  523,  524,
  525,  526,  527,  528,  529,  530,  531,  532,  533,  534,
  535,  536,  629,  623,  624,  537,  538,  539,  540,  777,
  805,  806,  807,  808,  829,  809,  541,  542,  543,  544,
  673,  771,  820,  674,  675,  822,  545,  546,  547,  548,
  549,  615,  689,  690,  691,  740,  692,  741,  693,  811,
  697,
  };
  protected static  short [] yySindex = {         -312,
 -236,    0, -116, -312,    0,    0,    0,  -97,    0,  -56,
  -15, -273,  -90,    0, -116,    0,    0,  -49,    0,  -44,
    0,    0,    0, -328,    0,   69,  -49, 4393,  166,    0,
  -90,    0,    0,    0,    0,    0,    0,    0,    0,    0,
   63,    0, 8029,    0,  -58,    0,    0,    0,    0,    0,
  -15,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,   36, 4393,    0,    0,    0,    0,   19,
    0,    0,    0,   86,    0,    0,    0,    0,    0,    0,
  468,    0,    0,    0,    0,   90,    0,    0,    0,   95,
    0,    0,   98,    0, 8029, 8029, 8029, 8029, 8029, 8029,
 8029, 8029, 8029,    0,    0,    0,    0,    0,    0,  -15,
   88,    0,    0,    0,   74,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  111,    0,    0,  429,    0,    0,    0,   -9,
  -51,   78, -155,   73,  112,  118,  126,   96, -295,    0,
    0,    0, -328,  158,   83,  468,   87,   91,  100,    0,
 8029,  101, 8029,    0,  116,    0,    0,  468,  468, 8029,
 -293,  168,  171,    0,    0,    0,    0,    0,    0,    0,
    0,    0, 8029, 8029, 6957,  102,    0,    0,  103, 8029,
 8029, 8029, 8029, 8029, 8029, 8029, 8029, 8029, 8029, 8029,
 8029, 8029, 8029, 8029, 8029, 8029, 8029,  468,  468, 8029,
 8029, 8029, 8029, 8029, 8029, 8029, 8029, 8029, 8029, 8029,
 8029,    0, -312,  173,    0, -310,  175,    0,    0,    0,
   33,    0,  179, 7493, 6957,  184,    0, -186,  -87,  182,
 8029,    0, 8029,    0,   34, 8029, 8029,    0,  183,  185,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,   -9,   -9,
  -51,  -51,  184,  184,   78,   78,   78,   78, -155, -155,
   73,  112,  118,  126,  193,   96,  -90,    0,    0,  194,
  186,  187,   31,  190,  196,  199,    0, 8029,    0,    0,
   35,  200,  201,  204,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0, 6957, 8029,  210,  468,  213,
    0, -116,    0, -116,  173,  -49,  216,    0,  468,  217,
  199,    0,    0,  184,    0,    0,    0,    0,    0,    0,
  468,  184, -116,  173,   38,  215,  218,    0,    0,  149,
  222,  221,    0,    0,    0,    0,  223, -116,    0,  468,
 -116,  173,    0,  184,  224,  184, 4236,    0,  230, -116,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
 -299,    0,    0,  468,    0,  468,    0,  225, -116,  219,
    0,  -19,  -49,  275,  235, -116,    0,    0,    0,    0,
    0, 4393,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  236, -116,    0,    0, 7091,    0,    0,  162, 8233,
    0,    0,    0,    0,    0,    0, -305, -287,    0,    0,
    0, 8029,    0,    0,    0,    0, 8585,    0,    0,    0,
    0,    0,    0,    0,    0,  -60,  234,  468,  468,  276,
  278,  164,  240, -208,  249,    0,  249,  251, 4633,    0,
    0,    0,  468,  174, -265,    0, 7225,  250, -276, -266,
  468,  468,    0,  254,  247, -116,  661,  252,  246,  119,
    0,   43,  255,    0,    0, -116,  256,   54,  468,  258,
 4947,  259,  266, -263,  267,  269, 8029,  272, 8029,  249,
   64,  274,  279,  280,    0,  277,    0,  279,   79,    0,
    0,    0,    0,  429,    0,    0,    0,  281, 4633,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  271,  282,  209,  285,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0, -256,
  284,  288,   70,    0,    0,  249,  283,  125,    0,  134,
  286,  145,  146, -116,  227,  291,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  295,
 -116, 7627,  209,    0,    0, -116, -308,  155,  299, -116,
 -116,    0,    0, -276,    0,  311, 7761,  468, 8029,  300,
  301, 8029, 8029,    0,  305, 8029,  308, -241,    0, 8163,
  186,    0,  279,    0, 8029, 4633,    0, 7493,    0,    0,
    0,    0,    0,  246,  312,    0,  309, -116, -116,    0,
 -116,    0, 8029,  243,    0,    0, -116,  468,  468,  317,
    0,  315,  468,  321,    0,    0, -116,  322,  329,  -36,
    0,    0,  334,    0,    0,  339,    0,    0,    0,  336,
    0,    0,  332,    0,  331, -239,    0,  333,    0,    0,
    0,  338,  340,    0,  342,    0,  344,  249,  400,    0,
  422,  423,    0, 8029,   79, 8029,  348,  351,    0,  350,
    0,    0,  352,  353, -116,  359,    0,    0, -212,  364,
 -116, -116, -238, -233,    0, -240,    0,    0, -231,    0,
    0,    0, -116,    0,    0,    0,  -36, 8029, 8029, 8029,
  414,    0, 4947, 4947,  369,  468,    0,    0,   75,    0,
    0,    0,  249,    0,    0, 4947,    0, 4947,  361,  371,
  156,    0,  363,  249,  249,  367,  405,    0,  452,    0,
  377,  378,  380,  381,  152,  386,  385,    0,  384,    0,
  383,    0, 8029,  456,    0,  -61,    0, -226,    0,    0,
    0,    0,    0,  388,  389,  394,    0,    0,    0,    0,
    0,    0, 6957, 6957,    0,  468,    0,    0,  391, 8029,
  397, 4947, 8029,  382,  403,  -61,    0,  -61,    0,    0,
  406, -116, -116, -116,    0,  407,  410, -194,    0,  413,
  331,    0, 4947,    0,  415,    0,    0,    0, 4633,    0,
  249,    0,  427,  412,    0,    0,  424, 4947,    0,    0,
 4633,    0,  430,  419,  426,    0,    0,    0,    0,    0,
  };
  protected static  short [] yyRindex = {         8996,
    0,    0, 9143, 8886,    0,    0,    0,  -76,    0,    0,
 2129,   28, 9198,    0,  813,    0,    0,    0,    0,    0,
    0,    0,    0,   81,    0,    0,    0,   85,    0,    0,
 9051,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  433,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0, 8315,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0, 1681,
    0,    0,    0,    0, 1838,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0, 1995,    0,    0,    0, 2397,
 2799, 2933, 5081, 5483, 5751, 5885, 6287, 6555, 6823,    0,
    0,    0,   81,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
 8741,    0,  432,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  434,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0, 9073, 8941,    0,    0,  440,    0,    0,    0,
    0,    0,    0,  442,  434, 2263,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  443,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0, 2531, 2665,
 3067, 3201, 3335, 3469, 3603, 3737, 3871, 4005, 5215, 5349,
 5617, 6019, 6153, 6421,    0, 6689, 9126,    0,    0,  449,
  442,    0,    0,    0,  450,  451,    0,    0,    0,    0,
    0,    0,  454,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0, 8553,    0, -318, -235,    0,    0,    0,    0,    0,
  453,    0,    0, 1210,    0,    0,    0,    0,    0,    0,
  458, 8632,  889, -235, 8742,    0,  157,    0,    0,    0,
    0,  459,    0,    0,    0,    0,  461, 8426,    0,  -80,
 1051, -235,    0, 1367, 1524, 8668, 8382,    0,    0,  970,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0, 8553,  -29,
    0,  374,    0, 8720,    0, 8478,    0,    0,    0,    0,
    0, 8382,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0, 1130,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0, -225,    0,    0,    0,    0,    0,  463,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  160,    0,    0, 8509,    0,    0,  -12,    0,
    0,    0,    0,    0,    0,  172,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  445,    0,  445,    0,
    0,    0,  387,    0,    0, 9299,    0, -327, -108, 9155,
 9178, 9237, 9248,    0,  584, 1195,    0,    0,  465,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0, 8509,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
 8553,    0,    0,    0,    0, 8509,    0,    0,    0,    2,
 -150,    0,    0,    0,    0,    0,  462,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  442,    0,  402,    0,    0,    0,    0,  442,    0,    0,
    0,    0,    0, 7359, 7895,    0,    0, 8509, 8553,    0,
 8509,    0,    0,    0,    0,    0, -115,    0,    0,    0,
    0,  467,    0,    0,    0,    0,  172,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  473,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0, 4319,    0,
 4162,    0,    0,    0,  387,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  172,    0,    0,    0,    0,    0,
  499,  563,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0, 8553,    0,    0,    0,    0,    0,  481,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0, 4476,    0,  491,    0,  489,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  434,  434,    0,    0,    0,    0,    0,  490,
    0,    0,    0,    0,    0,  496,    0, 4790,    0,    0,
    0,  172,   48,    5,    0,    0,    0,    0,    0,    0,
  493,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
 -123,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  };
  protected static  short [] yyGindex = {            0,
  618,   -3,  555,    0,  839,    0,    0,  195,    0,    0,
    0,    8,    0,    0,  -45,    0,    0,  814, -158,    0,
    0,    0,    0,    0,    0,    0,  834,    0,    0,    0,
    0,  698,    0,    0,  -22,    0,  -25,    0,    0,    0,
    0,    0,  482, -206, -161, -156, -153, -146, -135, -110,
 -107,    0,  825,    0,    0,    0,    0,  523,  528,    0,
    0,  446,  229,    0, -454, -624, -415,  268, -453,  501,
    0,    0, -272, -501, -192, -319,    0,  475,  478,    0,
    0, -321,  212,    0,    0,  270,    0,  273,    0,  147,
    0,    0,    0,    0,    0,    0,    0,    0,  476,    0,
    0,    0,    0,    0,   68,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0, -234,    0,  163,  165,    0,
    0,    0,  586,    0,    0,  494,   67,    0,  549, -429,
    0,    0,    0, -193,    0,    0,    0,   17,  114,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  572,
  643,  -10,  151,    0,    0,    0, -223, -306,    0,    0,
   27,  161,  208,    0,  198,  248,  253,  298,  676,  681,
  675,  682,  679,    0,    0,  263, -609,    0,    0,    0,
  837,    0,    0,    0,   76, -513,    0, -484,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0, -558,
    0,    0, -134,    0,  287, -596,    0,    0,    0,    0,
    0,    0,   97,    0,    0,  104,    0,    0,    0,    0,
    0,    0,    0,    0,  106,    0,    0,    0,    0,    0,
    0,  416,    0,  228,    0,    0,    0,  226,  220,    0,
    0,
  };
  protected static  short [] yyTable = {            13,
  304,   49,  346,  609,  356,  631,  606,   21,   11,   28,
  672,  610,  305,  555,  659,  700,   43,  112,  707,  281,
    1,   12,  763,  219,   12,   11,  687,   28,   12,  509,
   12,   11,  291,   12,   51,   12,   12,  291,   12,   22,
   12,  423,   12,  688,   12,   12,   12,   12,  671,  754,
  424,   12,  241,   20,  560,  291,   12,  552,   12,   12,
   12,  696,  650,   12,   12,   12,  291,   12,  220,  172,
   12,  435,   12,   12,   12,   12,  291,  291,   16,   12,
   12,   12,  764,   12,   12,   12,  291,  221,   11,  509,
   40,  432,   12,   12,  658,  477,   12,   16,   12,   12,
   33,  755,  171,  291,  291,  208,   12,   12,  292,  291,
  651,  291,  701,  460,  478,   12,  291,   16,  769,  770,
  635,   16,  174,  175,  176,  177,  178,  179,  180,  181,
   49,  461,  483,  772,  291,  230,  703,  233,  655,  706,
  305,  209,  557,  459,  240,   23,   33,  166,  291,  669,
  231,  459,  479,  553,  404,  611,  291,  244,  230,  306,
   11,  173,  637,   11,  254,  255,  256,  257,  258,  259,
  260,  261,  262,  263,  264,   11,   11,  509,  825,  731,
  761,  482,    8,   12,  368,  762,   33,  765,  426,  708,
  695,  130,  810,   16,  285,    9,  509,  561,   33,  405,
  210,  211,  403,  672,  406,  803,  404,  407,  230,   27,
  479,  368,   41,  804,  408,   11,   11,  212,  459,  213,
  312,  312,  166,  301,  837,  409,   12,  265,  266,  267,
  268,  268,  268,  268,  166,  166,  268,  268,  268,  268,
  268,  268,  268,  268,  268,  268,  323,  268,  774,  775,
  410,  405,   12,  411,  403,  291,  406,   18,  307,  407,
   79,  780,  484,  781,  485,  510,  408,  309,   79,  310,
   16,  654,  333,   16,  166,  166,  323,  409,  323,  354,
  323,  466,  323,   28,  323,  152,  323,  467,  323,  153,
  323,  339,  323,   19,  323,  603,   72,  510,  382,   73,
  204,  205,  410,  355,  423,  411,  155,  618,  619,  165,
  432,  156,  223,  724,  698,  157,  415,  824,  223,  704,
  350,  698,  433,   12,   79,  510,   11,  631,  123,   80,
  158,   20,  123,   11,   16,  102,   11,  102,  839,  367,
   33,   33,  383,  132,  384,   84,  163,   85,   11,  385,
  201,  202,  203,  847,  394,  386,  159,  402,   91,   92,
  386,  161,   94,  642,  386,  162,  367,   11,  622,   40,
  355,   39,   39,  627,   42,   39,  297,  311,  334,  386,
  298,  298,  298,  595,  226,  166,   33,  596,  350,  162,
  443,   11,  394,   11,  423,  166,  238,  239,  163,  509,
   11,  269,  270,  767,  423,  386,  462,  166,  170,  402,
  640,  509,   69,  510,  641,  423,  184,   50,  185,  736,
  186,  628,   67,  185,   50,  186,  166,   11,   50,  355,
  163,  355,  510,  182,  168,  183,  273,  274,  187,  169,
  188,  443,  170,  187,   11,  188,  206,  355,  207,  214,
  166,  215,  166,  271,  272,   11,   11,  189,  234,   11,
  235,   11,  275,  276,  277,  278,  593,  725,  594,  216,
   11,  614,  644,  614,  645,  511,  219,   11,   11,   11,
  217,  593,  598,  646,  218,  514,  166,  291,  291,  648,
  649,  660,  784,  661,  785,  737,   11,  795,  223,  796,
  112,  225,  112,  166,  123,  227,   16,  511,   33,  228,
   33,  279,  280,  242,  166,  166,  243,  514,  229,  232,
  252,  253,  288,  293,  299,  508,  291,  308,  315,  166,
  324,  322,  316,  300,  725,  511,  510,  166,  166,  510,
  510,  317,  319,  335,  326,  514,  779,  329,  336,  337,
  779,  340,  510,  343,  510,  166,  358,  361,  816,  817,
  388,  788,  789,  391,  416,  389,  443,  390,  392,  366,
  393,  421,  512,  432,  429,  436,  438,  440,  468,  471,
  447,  472,  473,  677,  476,  508,  681,  683,  366,  423,
  685,  486,  551,  565,  591,  556,  663,  666,  564,  681,
  592,  597,  230,  607,  512,  602,  510,  605,  510,  513,
  608,  612,  427,  613,  428,   11,  616,  677,  620,  515,
  632,  621,  630,  511,  625,  626,  647,  634,  638,  510,
  639,  633,  512,  514,  636,  510,  652,  643,  842,  653,
  662,  513,  511,  709,  510,  651,  670,  510,  454,  679,
  680,  515,  514,  598,  684,   11,   11,  686,  702,  593,
   11,  557,  715,  716,  720,  465,  516,  722,  172,  513,
  745,  723,  661,  508,  166,  660,  469,  470,  730,  515,
  728,  729,  732,  733,  688,  734,  508,  735,  736,  739,
  743,  550,  508,  746,  747,  748,  750,  749,  516,  562,
  563,  751,  681,  681,  753,  756,  773,  757,  759,  776,
  782,  783,  787,  754,  166,  166,  790,  604,  755,  166,
  512,  517,  791,  792,  793,  794,  516,  797,  798,  799,
  826,   71,  800,   72,  802,  815,   73,  813,  814,  512,
  819,   75,  823,   11,  827,   76,  511,  801,  845,  511,
  511,  831,  835,  517,   78,  836,  514,  513,  838,  514,
  514,   79,  511,  840,  511,  844,   80,  515,  849,  846,
   83,  848,  514,   10,  514,  850,  513,  677,  281,  283,
  215,  517,   84,  190,   85,  308,  515,   87,  284,  402,
  141,   75,   33,   76,  495,   91,   92,  309,  404,   94,
  220,  143,  166,   11,  407,  432,  408,  200,  751,  833,
  834,  471,   33,  191,  516,  192,  511,  193,  511,  194,
  433,  195,  474,  196,   33,  197,  514,  198,  514,  199,
  475,  200,  454,  516,  509,  477,  676,  455,  479,  511,
  287,  318,   17,  512,   68,  511,  512,  512,   39,  514,
  222,  422,   64,  363,  511,  514,  360,  511,  441,  512,
  656,  512,  166,  430,  514,  417,  431,  514,  721,  517,
  667,  439,  665,  768,  760,  758,  713,  714,  323,  832,
  513,  719,  364,  513,  513,  434,   40,  338,  517,  314,
  515,  281,  283,  515,  515,  508,  513,  282,  513,  286,
  284,  160,  828,    0,  841,  821,  515,  508,  515,  699,
  742,  830,    0,  512,  617,  512,  738,  744,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  445,
    0,  445,    0,  445,    0,    0,  512,  516,  330,    0,
  516,  516,  512,    0,  568,    0,    0,    0,    0,    0,
  513,  512,  513,  516,  512,  516,    0,    0,    0,    0,
  515,    0,  515,    0,  778,    0,    0,    0,  330,    0,
  330,    0,  330,  513,  330,    0,  330,    0,  330,  513,
  330,    0,  330,  515,  330,  569,  330,    0,  513,  515,
    0,  513,  517,    0,    0,  517,  517,    0,  515,    0,
    0,  515,    0,    0,    0,    0,    0,  516,  517,  516,
  517,  570,  571,  572,  573,    0,  574,  575,  576,  577,
  578,  579,  580,  581,  818,  582,    0,  583,    0,  584,
  516,  585,    0,  586,    0,  587,  516,  588,    0,  589,
    0,    0,    0,    0,    0,  516,    0,    0,  516,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  517,    0,  517,    0,    0,    0,    0,   35,
    0,    0,   35,    0,   35,    0,   35,    0,   35,    0,
    0,   35,    0,   35,   35,  517,   35,    0,   35,    0,
   35,  517,   35,   35,   35,   35,    0,    0,    0,   35,
  517,    0,    0,  517,   35,    0,   35,   35,   35,    0,
    0,   35,   35,   35,    0,   35,    0,   35,   35,   35,
   35,   35,   35,   35,   35,    0,   35,   35,   35,   35,
    0,   35,   35,   35,    0,    0,    0,    0,    0,    0,
   35,   35,    0,    0,   35,    0,   35,   35,   33,   35,
    0,   35,   33,    0,   33,    0,    0,   33,    0,   33,
   33,    0,   33,   35,   33,    0,   33,    0,   33,   33,
   33,   33,    0,    0,    0,   33,    0,    0,    0,    0,
   33,    0,   33,   33,   33,    0,    0,   33,    0,   33,
    0,   33,    0,    0,   33,    0,   33,   33,   33,   33,
    0,    0,    0,   33,   33,   33,    0,   33,   33,   33,
    0,    0,    0,    0,    0,    0,   33,   33,    0,    0,
   33,    0,   33,   33,    0,    0,    0,    0,    0,   33,
   59,   35,    0,   33,    0,   33,    0,    0,   33,   33,
   33,   33,    0,   33,    0,   33,    0,   33,    0,   33,
   33,   33,   33,    0,    0,    0,   33,    0,    0,    0,
    0,   33,    0,   33,   33,   33,    0,    0,   33,    0,
   33,    0,   33,    0,    0,   33,    0,   33,   33,   33,
   33,    0,    0,    0,   33,   33,   33,    0,   33,   33,
   33,    0,    0,    0,    0,    0,    0,   33,   33,    0,
    0,   33,    0,   33,   33,    0,    0,   33,    0,    0,
   33,   60,    0,    0,   33,    0,   33,    0,    0,   33,
   33,   33,   33,    0,   33,    0,   33,    0,   33,    0,
   33,   33,   33,   33,    0,    0,    0,   33,    0,    0,
    0,    0,   33,    0,   33,   33,   33,    0,    0,   33,
    0,   33,    0,   33,    0,    0,   33,    0,   33,   33,
   33,   33,    0,    0,    0,   33,   33,   33,    0,   33,
   33,   33,    0,    0,    0,    0,    0,    0,   33,   33,
    0,    0,   33,    0,   33,   33,    0,    0,   33,   33,
    0,    0,   81,   33,    0,   33,    0,    0,   33,    0,
   33,   33,    0,   33,    0,   33,    0,   33,    0,   33,
   33,   33,   33,    0,    0,    0,   33,    0,    0,    0,
    0,   33,    0,   33,   33,   33,    0,    0,   33,    0,
   33,    0,   33,    0,    0,   33,    0,   33,   33,   33,
   33,    0,    0,    0,   33,   33,   33,    0,   33,   33,
   33,    0,    0,    0,    0,    0,    0,   33,   33,    0,
    0,   33,    0,   33,   33,    0,    0,    0,    0,   33,
  303,   82,  303,  303,    0,  303,    0,    0,  303,  303,
    0,    0,    0,  303,    0,    0,    0,  303,    0,    0,
    0,    0,    0,  303,    0,    0,  303,    0,    0,    0,
    0,    0,    0,  303,    0,    0,  303,    0,  303,    0,
  303,  303,  303,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  303,    0,  303,  303,    0,  303,
    0,    0,  303,    0,  303,    0,  303,  303,  303,  303,
  446,  303,  446,    0,  446,    0,    0,    0,   33,  331,
  303,  303,    0,  303,  303,  303,  303,  303,  303,  303,
  303,  303,  303,  303,  303,  303,  303,  303,  303,  303,
  303,  303,  303,  303,  303,    0,  303,    0,  303,  331,
  303,  331,  303,  331,  303,  331,  303,  331,  303,  331,
  303,  331,  303,  331,  303,  331,  303,  331,  303,    0,
  303,    0,  303,    0,  303,    0,  303,    0,  303,    0,
  303,    0,  303,    0,    0,    0,  303,    0,  303,    0,
  303,    0,  303,    0,  303,    0,  303,  304,  303,  304,
  304,    0,  304,    0,    0,  304,  304,    0,    0,    0,
  304,    0,    0,    0,  304,    0,    0,    0,    0,    0,
  304,    0,    0,  304,    0,    0,    0,    0,    0,    0,
  304,    0,    0,  304,    0,  304,    0,  304,  304,  304,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  304,    0,  304,  304,    0,  304,    0,    0,  304,
    0,  304,    0,  304,  304,  304,  304,    0,  304,    0,
    0,    0,    0,    0,    0,    0,    0,  304,  304,    0,
  304,  304,  304,  304,  304,  304,  304,  304,  304,  304,
  304,  304,  304,  304,  304,  304,  304,  304,  304,  304,
  304,  304,    0,  304,    0,  304,    0,  304,    0,  304,
    0,  304,    0,  304,    0,  304,    0,  304,    0,  304,
    0,  304,    0,  304,    0,  304,    0,  304,    0,  304,
    0,  304,    0,  304,    0,  304,    0,  304,    0,  304,
    0,    0,    0,  304,    0,  304,    0,  304,    0,  304,
    0,  304,    0,  304,  312,  304,  312,  312,    0,  312,
    0,    0,  312,  312,    0,    0,    0,  312,    0,    0,
    0,  312,    0,    0,    0,    0,    0,  312,    0,    0,
  312,    0,    0,    0,    0,    0,    0,  312,    0,    0,
  312,    0,  312,    0,  312,  312,  312,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  312,    0,
  312,  312,    0,  312,    0,    0,  312,    0,  312,    0,
  312,  312,  312,  312,    0,  312,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  312,  312,  312,  312,  312,
  312,  312,  312,  312,  312,  312,  312,  312,  312,  312,
  312,  312,  312,  312,  312,  312,  312,  312,  312,    0,
  312,    0,  312,    0,  312,    0,  312,    0,  312,    0,
  312,    0,  312,    0,  312,    0,  312,    0,  312,    0,
  312,    0,  312,    0,  312,    0,  312,    0,  312,    0,
  312,    0,  312,    0,  312,    0,  312,    0,    0,    0,
  312,    0,  312,    0,  312,    0,  312,    0,  312,    0,
  312,  252,  312,  252,  252,    0,  252,    0,    0,  252,
  252,    0,    0,    0,  252,    0,    0,    0,  252,    0,
    0,    0,    0,    0,  252,    0,    0,  252,    0,    0,
    0,    0,    0,    0,  252,    0,    0,  252,    0,  252,
    0,  252,  252,  252,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  252,    0,  252,  252,    0,
  252,    0,    0,  252,    0,  252,    0,  252,  252,  252,
  252,    0,  252,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  252,  252,  252,  252,  252,    0,  252,  252,
  252,  252,  252,  252,  252,  252,  252,  252,  252,  252,
  252,  252,  252,  252,  252,  252,    0,  252,    0,  252,
    0,  252,    0,  252,    0,  252,    0,  252,    0,  252,
    0,  252,    0,  252,    0,  252,    0,  252,    0,  252,
    0,  252,    0,  252,    0,  252,    0,  252,    0,  252,
    0,  252,    0,  252,    0,    0,    0,  252,    0,  252,
    0,  252,    0,  252,    0,  252,    0,  252,  323,  252,
  323,  323,    0,  323,    0,    0,  323,  323,    0,    0,
    0,  323,    0,    0,    0,  323,    0,    0,    0,    0,
    0,  323,    0,    0,  323,    0,    0,    0,    0,    0,
    0,  323,    0,    0,  323,    0,  323,    0,  323,  323,
  323,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  323,    0,  323,  323,    0,  323,    0,    0,
  323,    0,  323,    0,  323,  323,  323,  323,    0,  323,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  323,
    0,  323,    0,  323,    0,  323,  323,  323,  323,  323,
  323,  323,  323,  323,  323,  323,  323,  323,  323,  323,
  323,  323,    0,    0,    0,    0,  323,    0,  323,    0,
  323,    0,  323,    0,  323,    0,  323,    0,  323,    0,
  323,    0,  323,    0,  323,    0,  323,    0,  323,    0,
  323,    0,  323,    0,  323,    0,  323,    0,  323,    0,
  323,    0,    0,    0,  323,    0,  323,    0,  323,    0,
  323,    0,  323,    0,  323,  337,  323,  337,  337,    0,
  337,    0,    0,  337,  337,    0,    0,    0,  337,    0,
    0,    0,  337,    0,    0,    0,    0,    0,  337,    0,
    0,  337,    0,    0,    0,    0,    0,    0,  337,    0,
    0,  337,    0,  337,    0,  337,  337,  337,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  337,
    0,  337,  337,    0,  337,    0,    0,  337,    0,  337,
    0,  337,  337,  337,  337,    0,  337,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  337,    0,  337,  337,
  337,    0,  337,  337,  337,  337,  337,  337,  337,    0,
  337,  337,  337,  337,  337,  337,  337,  337,  337,  337,
    0,  337,    0,  337,    0,  337,    0,  337,    0,  337,
    0,  337,    0,  337,    0,  337,    0,  337,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  227,
    0,  227,  227,    0,  227,    0,    0,  227,  227,    0,
    0,  337,  227,  337,    0,  337,  227,  337,    0,  337,
    0,  337,  227,  337,    0,  227,    0,    0,    0,    0,
    0,    0,  227,    0,    0,  227,    0,  227,    0,  227,
  227,  227,  227,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  227,    0,  227,  227,    0,  227,    0,
    0,  227,    0,  227,    0,  227,  227,  227,  227,    0,
  227,    0,    0,    0,    0,    0,    0,    0,    0,  227,
  227,  227,  227,  227,  227,    0,  227,  227,  227,  227,
  227,  227,  227,    0,  227,  227,  227,  227,  227,    0,
    0,  227,  227,  227,    0,  227,    0,    0,    0,    0,
    0,  227,    0,  227,    0,  227,    0,  227,    0,  227,
    0,  227,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  250,    0,  250,  250,    0,  250,    0,
    0,  250,  250,    0,    0,  227,  250,  227,    0,  227,
  250,  227,    0,  227,    0,  227,  250,  227,    0,  250,
    0,    0,    0,    0,    0,    0,  250,    0,    0,  250,
    0,  250,    0,  250,  250,  250,  250,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  250,    0,  250,
  250,    0,  250,    0,    0,  250,    0,  250,    0,  250,
  250,  250,  250,    0,  250,    0,    0,    0,    0,    0,
    0,    0,    0,  250,  250,    0,  250,  250,  250,    0,
  250,  250,  250,  250,  250,  250,  250,    0,  250,  250,
  250,  250,  250,    0,    0,  250,  250,  250,    0,  250,
    0,    0,    0,    0,    0,  250,    0,  250,    0,  250,
    0,  250,    0,  250,    0,  250,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  341,    0,  341,
  341,    0,  341,    0,    0,  341,  341,    0,    0,  250,
  341,  250,    0,  250,  341,  250,    0,  250,    0,  250,
  341,  250,    0,  341,    0,    0,    0,    0,    0,    0,
  341,    0,    0,  341,    0,  341,    0,  341,  341,  341,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  341,    0,  341,  341,    0,  341,    0,    0,  341,
    0,  341,    0,  341,  341,  341,  341,    0,  341,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  341,    0,
  341,  341,  341,    0,  341,  341,  341,  341,  341,  341,
  341,    0,  341,  341,  341,  341,    0,    0,    0,  341,
  341,  341,    0,  341,    0,  341,    0,  341,    0,  341,
    0,  341,    0,  341,    0,  341,    0,  341,    0,  341,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  342,    0,  342,  342,    0,  342,    0,    0,  342,
  342,    0,    0,  341,  342,  341,    0,  341,  342,  341,
    0,  341,    0,  341,  342,  341,    0,  342,    0,    0,
    0,    0,    0,    0,  342,    0,    0,  342,    0,  342,
    0,  342,  342,  342,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  342,    0,  342,  342,    0,
  342,    0,    0,  342,    0,  342,    0,  342,  342,  342,
  342,    0,  342,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  342,    0,  342,  342,  342,    0,  342,  342,
  342,  342,  342,  342,  342,    0,  342,  342,  342,  342,
    0,    0,    0,  342,  342,  342,    0,  342,    0,  342,
    0,  342,    0,  342,    0,  342,    0,  342,    0,  342,
    0,  342,    0,  342,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  343,    0,  343,  343,    0,
  343,    0,    0,  343,  343,    0,    0,  342,  343,  342,
    0,  342,  343,  342,    0,  342,    0,  342,  343,  342,
    0,  343,    0,    0,    0,    0,    0,    0,  343,    0,
    0,  343,    0,  343,    0,  343,  343,  343,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  343,
    0,  343,  343,    0,  343,    0,    0,  343,    0,  343,
    0,  343,  343,  343,  343,    0,  343,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  343,    0,  343,  343,
  343,    0,  343,  343,  343,  343,  343,  343,  343,    0,
  343,  343,  343,  343,    0,    0,    0,  343,  343,  343,
    0,  343,    0,  343,    0,  343,    0,  343,    0,  343,
    0,  343,    0,  343,    0,  343,    0,  343,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  344,
    0,  344,  344,    0,  344,    0,    0,  344,  344,    0,
    0,  343,  344,  343,    0,  343,  344,  343,    0,  343,
    0,  343,  344,  343,    0,  344,    0,    0,    0,    0,
    0,    0,  344,    0,    0,  344,    0,  344,    0,  344,
  344,  344,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  344,    0,  344,  344,    0,  344,    0,
    0,  344,    0,  344,    0,  344,  344,  344,  344,    0,
  344,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  344,    0,  344,  344,  344,    0,  344,  344,  344,  344,
    0,    0,  344,    0,  344,  344,  344,  344,  344,    0,
    0,  344,  344,  344,    0,  344,    0,  344,    0,  344,
    0,  344,    0,  344,    0,  344,    0,  344,    0,  344,
    0,  344,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  347,    0,  347,  347,    0,  347,    0,
    0,  347,  347,    0,    0,  344,  347,  344,    0,  344,
  347,  344,    0,  344,    0,  344,  347,  344,    0,  347,
    0,    0,    0,    0,    0,    0,  347,    0,    0,  347,
    0,  347,    0,  347,  347,  347,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  347,    0,  347,
  347,    0,  347,    0,    0,  347,    0,  347,    0,  347,
  347,  347,  347,    0,  347,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  347,    0,  347,  347,  347,    0,
  347,  347,  347,  347,  347,  347,  347,    0,  347,  347,
  347,  347,  347,    0,    0,  347,  347,  347,    0,  347,
    0,    0,    0,    0,    0,  347,    0,  347,    0,  347,
    0,  347,    0,  347,    0,  347,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  345,    0,  345,
  345,    0,  345,    0,    0,  345,  345,    0,    0,  347,
  345,  347,    0,  347,  345,  347,    0,  347,    0,  347,
  345,  347,    0,  345,    0,    0,    0,    0,    0,    0,
  345,    0,    0,  345,    0,  345,    0,  345,  345,  345,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  345,    0,  345,  345,    0,  345,    0,    0,  345,
    0,  345,    0,  345,  345,  345,  345,    0,  345,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  345,    0,
  345,  345,  345,    0,  345,  345,  345,  345,    0,    0,
  345,    0,  345,  345,  345,  345,  345,    0,    0,  345,
  345,  345,    0,  345,    0,  345,    0,  345,    0,  345,
    0,  345,    0,  345,    0,  345,    0,  345,    0,  345,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  346,    0,  346,  346,    0,  346,    0,    0,  346,
  346,    0,    0,  345,  346,  345,    0,  345,  346,  345,
    0,  345,    0,  345,  346,  345,    0,  346,    0,    0,
    0,    0,    0,    0,  346,    0,    0,  346,    0,  346,
    0,  346,  346,  346,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  346,    0,  346,  346,    0,
  346,    0,    0,  346,    0,  346,    0,  346,  346,  346,
  346,    0,  346,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  346,    0,  346,  346,  346,    0,  346,  346,
  346,  346,    0,    0,  346,    0,  346,  346,  346,  346,
  346,    0,    0,  346,  346,  346,    0,  346,    0,  346,
    0,  346,    0,  346,    0,  346,    0,  346,    0,  346,
    0,  346,    0,  346,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  353,    0,  353,  353,    0,
  353,    0,    0,  353,  353,    0,    0,  346,  353,  346,
    0,  346,  353,  346,    0,  346,    0,  346,  353,  346,
    0,  353,    0,    0,    0,    0,    0,    0,  353,    0,
    0,  353,    0,  353,    0,  353,  353,  353,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  353,
    0,  353,  353,    0,  353,    0,    0,  353,    0,  353,
    0,  353,  353,  353,  353,    0,  353,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  353,    0,  353,  353,
  353,    0,  353,  353,  353,  353,  353,  353,  353,    0,
  353,  353,  353,  353,  353,    0,    0,  353,  353,  353,
    0,  353,    0,    0,    0,    0,    0,  353,    0,  353,
    0,  353,    0,  353,    0,  353,    0,  353,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  352,
    0,  352,  352,    0,  352,    0,    0,  352,  352,    0,
    0,  353,  352,  353,    0,  353,  352,  353,    0,  353,
    0,  353,  352,  353,    0,  352,    0,    0,    0,    0,
    0,    0,  352,    0,    0,  352,    0,  352,    0,  352,
  352,  352,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  352,    0,  352,  352,    0,  352,    0,
    0,  352,    0,  352,    0,  352,  352,  352,  352,    0,
  352,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  352,    0,  352,  352,  352,    0,  352,  352,  352,  352,
  352,  352,  352,    0,  352,  352,  352,  352,  352,    0,
    0,  352,  352,  352,    0,  352,    0,    0,    0,    0,
    0,  352,    0,  352,    0,  352,    0,  352,    0,  352,
    0,  352,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  348,    0,  348,  348,    0,  348,    0,
    0,  348,  348,    0,    0,  352,  348,  352,    0,  352,
  348,  352,    0,  352,    0,  352,  348,  352,    0,  348,
    0,    0,    0,    0,    0,    0,  348,    0,    0,  348,
    0,  348,    0,  348,  348,  348,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  348,    0,  348,
  348,    0,  348,    0,    0,  348,    0,  348,    0,  348,
  348,  348,  348,    0,  348,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  348,    0,  348,  348,  348,    0,
  348,  348,  348,  348,  348,  348,  348,    0,  348,  348,
  348,  348,  348,    0,    0,  348,  348,  348,    0,  348,
    0,    0,    0,    0,    0,  348,    0,  348,    0,  348,
    0,  348,    0,  348,    0,  348,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  349,    0,  349,
  349,    0,  349,    0,    0,  349,  349,    0,    0,  348,
  349,  348,    0,  348,  349,  348,    0,  348,    0,  348,
  349,  348,    0,  349,    0,    0,    0,    0,    0,    0,
  349,    0,    0,  349,    0,  349,    0,  349,  349,  349,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  349,    0,  349,  349,    0,  349,    0,    0,  349,
    0,  349,    0,  349,  349,  349,  349,    0,  349,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  349,    0,
  349,  349,  349,    0,  349,  349,  349,  349,  349,  349,
  349,    0,  349,  349,  349,  349,  349,    0,    0,  349,
  349,  349,    0,  349,    0,    0,    0,    0,    0,  349,
    0,  349,    0,  349,    0,  349,    0,  349,    0,  349,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  350,    0,  350,  350,    0,  350,    0,    0,  350,
  350,    0,    0,  349,  350,  349,    0,  349,  350,  349,
    0,  349,    0,  349,  350,  349,    0,  350,    0,    0,
    0,    0,    0,    0,  350,    0,    0,  350,    0,  350,
    0,  350,  350,  350,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  350,    0,  350,  350,    0,
  350,    0,    0,  350,    0,  350,    0,  350,  350,  350,
  350,    0,  350,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  350,    0,  350,  350,  350,    0,  350,  350,
  350,  350,  350,  350,  350,    0,  350,  350,  350,  350,
  350,    0,    0,  350,  350,  350,    0,  350,    0,    0,
    0,    0,    0,  350,    0,  350,    0,  350,    0,  350,
    0,  350,    0,  350,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  351,    0,  351,  351,    0,
  351,    0,    0,  351,  351,    0,    0,  350,  351,  350,
    0,  350,  351,  350,    0,  350,    0,  350,  351,  350,
    0,  351,    0,    0,    0,    0,    0,    0,  351,    0,
    0,  351,    0,  351,    0,  351,  351,  351,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  351,
    0,  351,  351,    0,  351,    0,    0,  351,    0,  351,
    0,  351,  351,  351,  351,    0,  351,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  351,    0,  351,  351,
  351,    0,  351,  351,  351,  351,  351,  351,  351,    0,
  351,  351,  351,  351,  351,    0,    0,  351,  351,  351,
    0,  351,    0,    0,    0,    0,    0,  351,    0,  351,
    0,  351,    0,  351,    0,  351,    0,  351,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  351,    0,  351,    0,  351,    0,  351,    0,  351,
    0,  351,    0,  351,  502,  502,  502,  502,  502,    0,
  502,  502,    0,  502,  502,  502,  502,    0,  502,  502,
  502,    0,    0,    0,    0,  502,  502,    0,  502,  502,
  502,  502,  502,    0,    0,  502,    0,    0,    0,  502,
  502,    0,  502,  502,  502,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  502,    0,  502,    0,  502,  502,
    0,  502,    0,  502,  502,  502,  502,  502,  502,  502,
  502,  502,    0,  502,  502,   52,  502,  502,    0,    0,
    0,    0,  502,  502,    0,    0,  502,    0,    0,    0,
    0,  502,  502,  502,  502,  502,    0,    0,   53,  502,
    0,  502,    0,    0,    0,    0,  502,    0,  502,    0,
    0,   54,    0,    0,    0,    0,   55,    0,    0,    0,
    0,   56,    0,   57,   58,   59,   60,    0,    0,    0,
    0,   61,    0,    0,   62,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  502,   63,
  502,    0,  502,    0,  502,    0,  502,    0,  502,    0,
  502,  497,  497,  497,  497,  497,  419,  497,  497,    0,
  497,  497,  497,  497,    0,  497,  497,  497,    0,    0,
    0,    0,  497,    0,    0,  497,  497,  497,  497,  497,
    0,    0,  497,    0,    0,    0,  497,  497,    0,  497,
  497,  497,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  497,    0,  497,    0,  497,  497,    0,  497,    0,
  497,  497,  497,  497,  497,  497,  497,  497,  497,    0,
  497,  497,   52,  497,  497,    0,    0,    0,    0,  497,
  497,    0,    0,  497,    0,    0,    0,    0,  497,  497,
  497,  497,  497,    0,    0,   53,  497,    0,  497,    0,
    0,    0,    0,  497,    0,  497,    0,    0,   54,    0,
    0,    0,    0,   55,    0,    0,    0,    0,   56,    0,
   57,   58,   59,   60,    0,    0,    0,    0,   61,    0,
    0,   62,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  497,   63,  497,    0,  497,
    0,  497,    0,  497,    0,  497,    0,  497,  450,  450,
  450,  450,  450,    0,  450,  450,    0,  450,  450,  450,
  450,    0,  450,  450,    0,    0,    0,    0,    0,  450,
    0,    0,  450,  450,  450,  450,  450,    0,    0,  450,
    0,    0,    0,  450,  450,    0,  450,  450,  450,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  450,    0,
  450,    0,  450,  450,    0,  450,    0,  450,  450,  450,
  450,  450,  450,  450,  450,  450,    0,  450,  450,    0,
  450,  450,    0,    0,    0,    0,  450,  450,    0,    0,
  450,    0,    0,    0,    0,  450,  450,  450,  450,  450,
    0,    0,    0,  450,    0,  450,    0,    0,    0,    0,
  450,    0,  450,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  450,    0,  450,    0,  450,    0,  450,    0,
  450,    0,  450,    0,  450,   70,   71,  487,   72,    0,
    0,   73,  488,    0,  489,  490,   75,    0,    0,  491,
   76,    0,    0,    0,    0,    0,   77,    0,    0,   78,
  492,  493,  494,  495,    0,    0,   79,    0,    0,    0,
  496,   80,    0,   81,   82,   83,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  497,    0,   84,    0,   85,
   86,    0,   87,    0,  498,   88,  499,   89,  500,   90,
   91,   92,  501,    0,   94,  502,    0,  503,  504,    0,
    0,    0,    0,  423,    0,    0,    0,   95,    0,    0,
    0,    0,  505,   96,   97,   98,   99,    0,    0,    0,
  100,    0,  101,    0,    0,    0,    0,  102,    0,  103,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  104,
    0,  105,    0,  106,    0,  107,    0,  108,    0,  109,
    0,  506,  458,  458,  458,  458,    0,    0,  458,  458,
    0,  458,  458,  458,    0,    0,  458,  458,    0,    0,
    0,    0,    0,  458,    0,    0,  458,  458,  458,  458,
  458,    0,    0,  458,    0,    0,    0,  458,  458,    0,
  458,  458,  458,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  458,    0,  458,    0,  458,  458,    0,  458,
    0,  458,  458,  458,  458,  458,  458,  458,  458,  458,
    0,  458,  458,    0,  458,  458,    0,    0,    0,    0,
  458,    0,    0,    0,  458,    0,    0,    0,    0,  458,
  458,  458,  458,  458,    0,    0,    0,  458,    0,  458,
    0,    0,    0,    0,  458,    0,  458,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  458,    0,  458,    0,
  458,    0,  458,    0,  458,    0,  458,    0,  458,   70,
   71,  487,   72,    0,    0,   73,  488,    0,    0,  490,
   75,    0,    0,  491,   76,    0,    0,    0,    0,    0,
   77,    0,    0,   78,  492,  493,  494,  495,    0,    0,
   79,    0,    0,    0,  496,   80,    0,   81,   82,   83,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  497,
    0,   84,    0,   85,   86,    0,   87,    0,  498,   88,
  499,   89,  500,   90,   91,   92,  501,    0,   94,  502,
    0,    0,  504,    0,    0,    0,    0,  423,    0,    0,
    0,   95,    0,    0,    0,    0,  505,   96,   97,   98,
   99,    0,    0,    0,  100,    0,  101,    0,    0,    0,
    0,  102,    0,  103,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  354,  354,    0,  354,    0,    0,  354,
  354,    0,    0,  104,  354,  105,    0,  106,  354,  107,
    0,  108,    0,  109,  354,   40,    0,  354,    0,    0,
    0,    0,    0,    0,  354,    0,    0,    0,    0,  354,
    0,  354,  354,  354,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  354,    0,  354,  354,    0,
  354,    0,    0,  354,    0,  354,    0,  354,  354,  354,
  354,    0,  354,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  354,    0,  354,  354,  354,    0,  354,  354,
  354,  354,  354,  354,  354,    0,    0,    0,  354,  354,
  354,    0,    0,  354,  354,  354,    0,  354,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  354,    0,  354,
    0,  354,    0,  354,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  355,  355,    0,
  355,    0,    0,  355,  355,    0,    0,  354,  355,  354,
    0,  354,  355,  354,    0,  354,    0,  354,  355,  354,
    0,  355,    0,    0,    0,    0,    0,    0,  355,    0,
    0,    0,    0,  355,    0,  355,  355,  355,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  355,
    0,  355,  355,    0,  355,    0,    0,  355,    0,  355,
    0,  355,  355,  355,  355,    0,  355,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  355,    0,  355,  355,
  355,    0,  355,  355,  355,  355,  355,  355,  355,    0,
    0,    0,  355,  355,  355,    0,    0,  355,  355,  355,
    0,  355,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  355,    0,  355,    0,  355,    0,  355,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  356,  356,    0,  356,    0,    0,  356,  356,    0,
    0,  355,  356,  355,    0,  355,  356,  355,    0,  355,
    0,  355,  356,  355,    0,  356,    0,    0,    0,    0,
    0,    0,  356,    0,    0,    0,    0,  356,    0,  356,
  356,  356,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  356,    0,  356,  356,    0,  356,    0,
    0,  356,    0,  356,    0,  356,  356,  356,  356,    0,
  356,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  356,    0,  356,  356,  356,    0,  356,  356,  356,  356,
  356,  356,  356,    0,    0,    0,  356,  356,  356,    0,
    0,  356,  356,  356,    0,  356,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  356,    0,  356,    0,  356,
    0,  356,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  357,  357,    0,  357,    0,
    0,  357,  357,    0,    0,  356,  357,  356,    0,  356,
  357,  356,    0,  356,    0,  356,  357,  356,    0,  357,
    0,    0,    0,    0,    0,    0,  357,    0,    0,    0,
    0,  357,    0,  357,  357,  357,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  357,    0,  357,
  357,    0,  357,    0,    0,  357,    0,  357,    0,  357,
  357,  357,  357,    0,  357,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  357,    0,  357,  357,  357,    0,
  357,  357,  357,  357,  357,  357,  357,    0,    0,    0,
  357,  357,  357,    0,    0,  357,  357,  357,    0,  357,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  357,    0,  357,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  358,
  358,    0,  358,    0,    0,  358,  358,    0,    0,  357,
  358,  357,    0,  357,  358,  357,    0,  357,    0,  357,
  358,  357,    0,  358,    0,    0,    0,    0,    0,    0,
  358,    0,    0,    0,    0,  358,    0,  358,  358,  358,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  358,    0,  358,  358,    0,  358,    0,    0,  358,
    0,  358,    0,  358,  358,  358,  358,    0,  358,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  358,    0,
  358,  358,  358,    0,  358,  358,  358,  358,  358,  358,
  358,    0,    0,    0,  358,  358,  358,    0,    0,  358,
  358,  358,    0,  358,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  358,    0,  358,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  359,  359,    0,  359,    0,    0,  359,
  359,    0,    0,  358,  359,  358,    0,  358,  359,  358,
    0,  358,    0,  358,  359,  358,    0,  359,    0,    0,
    0,    0,    0,    0,  359,    0,    0,    0,    0,  359,
    0,  359,  359,  359,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  359,    0,  359,  359,    0,
  359,    0,    0,  359,    0,  359,    0,  359,  359,  359,
  359,    0,  359,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  359,    0,  359,  359,  359,    0,  359,  359,
  359,  359,  359,  359,  359,    0,    0,    0,    0,  359,
  359,    0,    0,  359,  359,  359,    0,  359,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  359,    0,  359,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  361,  361,    0,
  361,    0,    0,  361,  361,    0,    0,  359,  361,  359,
    0,  359,  361,  359,    0,  359,    0,  359,  361,  359,
    0,  361,    0,    0,    0,    0,    0,    0,  361,    0,
    0,    0,    0,  361,    0,  361,  361,  361,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  361,
    0,  361,  361,    0,  361,    0,    0,  361,    0,  361,
    0,  361,  361,  361,  361,    0,  361,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  361,    0,  361,  361,
  361,    0,  361,  361,  361,  361,  361,  361,  361,    0,
    0,    0,  361,  361,  361,    0,    0,    0,  361,  361,
    0,  361,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  361,    0,  361,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  360,  360,    0,  360,    0,    0,  360,  360,    0,
    0,  361,  360,  361,    0,  361,  360,  361,    0,  361,
    0,  361,  360,  361,    0,  360,    0,    0,    0,    0,
    0,    0,  360,    0,    0,    0,    0,  360,    0,  360,
  360,  360,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  360,    0,  360,  360,    0,  360,    0,
    0,  360,    0,  360,    0,  360,  360,  360,  360,    0,
  360,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  360,    0,  360,  360,  360,    0,  360,  360,  360,  360,
  360,  360,  360,    0,    0,    0,    0,  360,  360,    0,
    0,  360,  360,  360,    0,  360,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  360,
    0,  360,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  362,  362,    0,  362,    0,
    0,  362,  362,    0,    0,  360,  362,  360,    0,  360,
  362,  360,    0,  360,    0,  360,  362,  360,    0,  362,
    0,    0,    0,    0,    0,    0,  362,    0,    0,    0,
    0,  362,    0,  362,  362,  362,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  362,    0,  362,
  362,    0,  362,    0,    0,  362,    0,  362,    0,  362,
  362,  362,  362,    0,  362,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  362,    0,  362,  362,  362,    0,
  362,  362,  362,  362,  362,  362,  362,    0,    0,    0,
  362,  362,  362,    0,    0,    0,  362,  362,    0,  362,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  362,    0,  362,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  363,
  363,    0,  363,    0,    0,  363,  363,    0,    0,  362,
  363,  362,    0,  362,  363,  362,    0,  362,    0,  362,
  363,  362,    0,  363,    0,    0,    0,    0,    0,    0,
  363,    0,    0,    0,    0,  363,    0,  363,  363,  363,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  363,    0,  363,  363,    0,  363,    0,    0,  363,
    0,  363,    0,  363,  363,  363,  363,    0,  363,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  363,    0,
  363,  363,  363,    0,  363,  363,  363,  363,  363,  363,
  363,    0,    0,    0,  363,    0,  363,    0,    0,    0,
  363,  363,    0,  363,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  363,    0,  363,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  364,  364,    0,  364,    0,    0,  364,
  364,    0,    0,  363,  364,  363,    0,  363,  364,  363,
    0,  363,    0,  363,  364,  363,    0,  364,    0,    0,
    0,    0,    0,    0,  364,    0,    0,    0,    0,  364,
    0,  364,  364,  364,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  364,    0,  364,  364,    0,
  364,    0,    0,  364,    0,  364,    0,  364,  364,  364,
  364,    0,  364,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  364,    0,  364,  364,  364,    0,  364,  364,
  364,  364,  364,  364,  364,    0,    0,    0,  364,    0,
  364,    0,    0,    0,  364,  364,    0,  364,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  364,    0,  364,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  365,  365,    0,
  365,    0,    0,  365,  365,    0,    0,  364,  365,  364,
    0,  364,  365,  364,    0,  364,    0,  364,  365,  364,
    0,  365,    0,    0,    0,    0,    0,    0,  365,    0,
    0,    0,    0,  365,    0,  365,  365,  365,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  365,
    0,  365,  365,    0,  365,    0,    0,  365,    0,  365,
    0,  365,  365,  365,  365,    0,  365,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  365,    0,  365,  365,
  365,    0,  365,  365,  365,  365,  365,  365,  365,    0,
    0,    0,  365,    0,  365,    0,    0,    0,  365,  365,
    0,  365,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  365,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  366,  366,    0,  366,    0,    0,  366,  366,    0,
    0,  365,  366,  365,    0,  365,  366,  365,    0,  365,
    0,  365,  366,  365,    0,  366,    0,    0,    0,    0,
    0,    0,  366,    0,    0,    0,    0,  366,    0,  366,
  366,  366,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  366,    0,  366,  366,    0,  366,    0,
    0,  366,    0,  366,    0,  366,  366,  366,  366,    0,
  366,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  366,    0,  366,  366,  366,    0,  366,  366,  366,  366,
  366,  366,  366,    0,    0,    0,  366,    0,  366,    0,
    0,    0,  366,  366,    0,  366,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  366,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  367,  367,    0,  367,    0,
    0,  367,  367,    0,    0,  366,  367,  366,    0,  366,
  367,  366,    0,  366,    0,  366,  367,  366,    0,  367,
    0,    0,    0,    0,    0,    0,  367,    0,    0,    0,
    0,  367,    0,  367,  367,  367,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  367,    0,  367,
  367,    0,  367,    0,    0,  367,    0,  367,    0,  367,
  367,  367,  367,    0,  367,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  367,    0,  367,  367,  367,    0,
  367,  367,  367,  367,  367,  367,  367,    0,    0,    0,
  367,    0,  367,    0,    0,    0,    0,  367,    0,  367,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,   70,
   71,    0,   72,    0,    0,   73,   74,    0,    0,  367,
   75,  367,    0,  367,   76,  367,    0,  367,    0,  367,
   77,  367,    0,   78,    0,    0,    0,    0,    0,    0,
   79,    0,    0,    0,    0,   80,    0,   81,   82,   83,
    0,  246,    0,    0,    0,    0,    0,    0,  247,    0,
    0,   84,    0,   85,   86,    0,   87,    0,    0,   88,
    0,   89,    0,   90,   91,   92,   93,    0,   94,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,   95,    0,    0,    0,    0,    0,   96,   97,   98,
   99,    0,    0,    0,  100,    0,  101,    0,    0,    0,
    0,  102,    0,  103,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,   70,   71,    0,   72,    0,    0,   73,
   74,    0,    0,  104,   75,  105,    0,  106,   76,  107,
    0,  108,    0,  109,   77,   40,    0,   78,    0,    0,
    0,    0,    0,    0,   79,    0,    0,    0,    0,   80,
    0,   81,   82,   83,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,   84,    0,   85,   86,    0,
   87,    0,    0,   88,    0,   89,    0,   90,   91,   92,
   93,    0,   94,    0,    0,    0,    0,    0,    0,    0,
    0,  416,  442,    0,    0,   95,    0,    0,    0,    0,
    0,   96,   97,   98,   99,    0,    0,    0,  100,    0,
  101,    0,    0,    0,    0,  102,    0,  103,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,   70,   71,    0,
   72,    0,    0,   73,   74,    0,    0,  104,   75,  105,
    0,  106,   76,  107,    0,  108,    0,  109,   77,   40,
    0,   78,    0,    0,    0,    0,    0,    0,   79,    0,
    0,    0,    0,   80,    0,   81,   82,   83,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,   84,
    0,   85,   86,    0,   87,    0,    0,   88,    0,   89,
    0,   90,   91,   92,   93,    0,   94,    0,    0,    0,
    0,    0,    0,    0,    0,  416,  554,    0,    0,   95,
    0,    0,    0,    0,    0,   96,   97,   98,   99,    0,
    0,    0,  100,    0,  101,    0,    0,    0,    0,  102,
    0,  103,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  102,  102,    0,  102,    0,    0,  102,  102,    0,
    0,  104,  102,  105,    0,  106,  102,  107,    0,  108,
    0,  109,  102,   40,    0,  102,    0,    0,    0,    0,
    0,    0,  102,    0,    0,    0,    0,  102,    0,  102,
  102,  102,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  102,    0,  102,  102,    0,  102,    0,
    0,  102,    0,  102,    0,  102,  102,  102,  102,    0,
  102,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  102,    0,    0,  102,    0,  102,  102,
  102,  102,  102,    0,    0,    0,  102,    0,  102,    0,
    0,    0,    0,  102,    0,  102,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,   70,   71,    0,   72,    0,
    0,   73,   74,    0,    0,  102,   75,  102,    0,  102,
   76,  102,    0,  102,    0,  102,   77,  102,    0,   78,
    0,    0,    0,    0,    0,    0,   79,    0,    0,    0,
    0,   80,    0,   81,   82,   83,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,   84,    0,   85,
   86,    0,   87,    0,    0,   88,    0,   89,    0,   90,
   91,   92,   93,    0,   94,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,   95,    0,    0,
  300,    0,    0,   96,   97,   98,   99,    0,    0,    0,
  100,    0,  101,    0,    0,    0,    0,  102,    0,  103,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,   70,
   71,    0,   72,    0,    0,   73,   74,    0,    0,  104,
   75,  105,    0,  106,   76,  107,    0,  108,    0,  109,
   77,   40,    0,   78,    0,    0,    0,    0,    0,    0,
   79,    0,    0,    0,    0,   80,    0,   81,   82,   83,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,   84,    0,   85,   86,    0,   87,    0,    0,   88,
    0,   89,    0,   90,   91,   92,   93,    0,   94,    0,
    0,    0,    0,    0,    0,    0,    0,  416,    0,    0,
    0,   95,    0,    0,    0,    0,    0,   96,   97,   98,
   99,    0,    0,    0,  100,    0,  101,    0,    0,    0,
    0,  102,    0,  103,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,   70,   71,    0,   72,    0,    0,   73,
   74,    0,    0,  104,   75,  105,    0,  106,   76,  107,
    0,  108,    0,  109,   77,   40,    0,   78,    0,    0,
    0,    0,    0,    0,   79,    0,    0,    0,    0,   80,
    0,   81,   82,   83,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,   84,    0,   85,   86,    0,
   87,    0,    0,   88,    0,   89,    0,   90,   91,   92,
   93,    0,   94,    0,    0,  503,    0,    0,    0,    0,
    0,    0,    0,    0,    0,   95,    0,    0,    0,    0,
    0,   96,   97,   98,   99,    0,    0,    0,  100,    0,
  101,    0,    0,    0,    0,  102,    0,  103,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  437,  437,    0,
  437,    0,    0,  437,  437,    0,    0,  104,  437,  105,
    0,  106,  437,  107,    0,  108,    0,  109,  437,   40,
    0,  437,    0,    0,    0,    0,    0,    0,  437,    0,
    0,    0,    0,  437,    0,  437,  437,  437,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  437,
    0,  437,  437,    0,  437,    0,    0,  437,    0,  437,
    0,  437,  437,  437,  437,    0,  437,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  437,
    0,    0,    0,    0,  437,  437,  437,  437,  437,    0,
    0,    0,  437,    0,  437,    0,    0,    0,    0,  437,
    0,  437,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,   70,   71,    0,   72,    0,    0,   73,   74,    0,
    0,  437,   75,  437,    0,  437,   76,  437,    0,  437,
    0,  437,   77,  437,    0,   78,    0,    0,    0,    0,
    0,    0,   79,    0,    0,    0,    0,   80,    0,   81,
   82,   83,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,   84,    0,   85,   86,    0,   87,    0,
    0,   88,    0,   89,    0,   90,   91,   92,   93,    0,
   94,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,   95,    0,    0,    0,    0,    0,   96,
   97,   98,   99,    0,    0,    0,  100,    0,  101,    0,
    0,    0,    0,  102,    0,  103,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,   70,   71,    0,   72,    0,
    0,   73,   74,    0,    0,  104,   75,  105,    0,  106,
   76,  107,    0,  108,    0,  109,   77,   40,    0,   78,
    0,    0,    0,    0,    0,    0,   79,    0,    0,    0,
    0,   80,    0,   81,   82,   83,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,   84,    0,   85,
   86,    0,   87,    0,    0,   88,    0,   89,    0,   90,
   91,   92,   93,    0,   94,    0,   71,  503,   72,    0,
    0,   73,    0,  155,  448,    0,   75,  694,  156,    0,
   76,    0,  157,  449,  450,    0,    0,    0,    0,   78,
    0,    0,    0,    0,  451,    0,   79,  158,    0,    0,
    0,   80,    0,    0,    0,   83,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,   84,    0,   85,
    0,    0,   87,  159,    0,    0,    0,    0,    0,    0,
   91,   92,    0,    0,   94,    0,    0,  452,    0,  104,
    0,  105,    0,  106,    0,  107,    0,  108,  387,  109,
  387,   40,    0,  387,    0,  387,  387,    0,  387,    0,
  387,    0,  387,    0,  387,  387,  387,    0,    0,    0,
    0,  387,    0,    0,    0,    0,  387,    0,  387,  387,
    0,    0,    0,  387,    0,    0,    0,  387,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  387,
    0,  387,    0,    0,  387,  387,    0,    0,    0,    0,
    0,    0,  387,  387,    0,  386,  387,  386,    0,  387,
  386,  453,  386,  386,    0,  386,    0,  386,    0,  386,
    0,  386,  386,  386,    0,    0,    0,    0,  386,    0,
    0,    0,    0,  386,    0,  386,  386,    0,    0,    0,
  386,    0,    0,    0,  386,    0,    0,    0,    0,   33,
    0,   33,    0,    0,   33,    0,  386,    0,  386,   33,
    0,  386,  386,   33,    0,    0,   33,    0,    0,  386,
  386,    0,   33,  386,    0,    0,  386,    0,    0,   33,
    0,    0,    0,    0,   33,    0,   33,    0,   33,    0,
    0,    0,    0,  387,    0,    0,    0,    0,    0,    0,
   33,   33,   33,   33,    0,   33,   33,    0,    0,    0,
    0,   33,    0,   33,   33,   33,    0,   33,   33,    0,
   33,    0,    0,    0,   33,    0,    0,  147,    0,    0,
    0,   33,   33,    0,   33,    0,   33,   33,   33,    0,
   33,    0,   33,    0,    0,    0,   33,    0,    0,    0,
    0,    0,   33,    0,   33,   33,    0,   33,    0,    0,
  386,    0,   33,    0,    0,   33,   33,   33,    0,   33,
    0,   33,   33,   33,    0,   33,   33,    0,   33,  148,
   33,   33,    0,   33,    0,   33,   33,    0,   33,    0,
   33,    0,    0,    0,    0,    0,   33,   33,    0,   33,
   33,    0,    0,    0,   33,    0,   33,    0,   71,    0,
   72,   33,    0,   73,  110,   33,    0,   33,   75,   33,
    0,    0,   76,    0,   33,  463,    0,   33,    0,   33,
    0,   78,   33,    0,    0,    0,    0,    0,   79,    0,
   33,   33,    0,   80,   33,    0,    0,   83,    0,    0,
    0,    0,    0,    0,    0,  231,   33,  231,    0,   84,
  231,   85,    0,    0,   87,  231,    0,    0,    0,  231,
    0,    0,   91,   92,    0,    0,   94,    0,  231,  464,
    0,    0,    0,    0,    0,  231,    0,   33,    0,    0,
  231,  232,    0,  232,  231,    0,  232,    0,    0,    0,
    0,  232,    0,    0,    0,  232,  231,    0,  231,    0,
    0,  231,    0,    0,  232,    0,    0,    0,    0,  231,
  231,  232,    0,  231,    0,    0,  232,    0,    0,    0,
  232,   33,  231,    0,    0,    0,    0,    0,    0,    0,
  231,    0,  232,  155,  232,  155,    0,  232,  155,    0,
    0,    0,    0,  155,    0,  232,  232,  155,    0,  232,
  155,  252,    0,   40,    0,  118,  155,  118,  232,    0,
  118,    0,    0,  155,    0,  118,  232,    0,  155,  118,
    0,    0,  155,    0,    0,    0,    0,    0,  118,    0,
    0,    0,    0,    0,  155,  118,  155,  252,    0,  155,
  118,    0,    0,    0,  118,    0,    0,  155,  155,    0,
  231,  155,    0,    0,  155,    0,  118,    0,  118,    0,
    0,  118,    0,    0,    0,    0,    0,    0,    0,  118,
  118,    0,    0,  118,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  252,    0,  252,  232,    0,    0,    0,
    0,    0,  252,  252,    0,  252,  252,  252,  252,  252,
  252,  252,  252,  252,  252,  252,    0,  252,    0,  252,
    0,  252,    0,  252,    0,  252,    0,  252,    0,  252,
    0,  252,    0,  252,    0,  252,    0,  252,    0,  252,
    0,  252,    0,  252,    0,  252,    0,  252,  155,  252,
    0,  252,   21,  252,    0,   21,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,   21,    0,    0,    0,
  118,   21,    0,    0,    0,   21,    0,    0,   21,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
   21,   21,    0,    0,    0,   21,   21,    0,    0,    0,
    0,   21,    0,   21,   21,   21,   21,   12,    0,    0,
   12,   21,    0,    0,   21,    0,   21,    0,    0,    0,
    0,   12,    0,    0,    0,    0,   12,    0,    0,   21,
   12,    0,    0,   12,    0,    0,    0,   21,   21,    0,
    0,    0,    0,    0,    0,   12,   12,    0,    0,    0,
   12,   12,    0,    0,    0,    0,   12,    0,   12,   12,
   12,   12,   20,    0,    0,   20,   12,    0,    0,   12,
    0,   12,    0,    0,    0,    0,   20,    0,    0,    0,
    0,   20,    0,    0,   12,   20,    0,    0,   20,    0,
    0,    0,   12,   12,    0,    0,    0,    0,    0,    0,
   20,   20,    0,    0,    0,   20,   20,    0,    0,    0,
    0,   20,    0,   20,   20,   20,   20,   23,    0,    0,
   33,   20,    0,    0,   20,    0,   20,    0,    0,    0,
    0,   33,    0,    0,    0,    0,   33,    0,    0,   20,
   33,    0,   20,   33,    0,    0,    0,    0,   20,    0,
    0,    0,    0,   20,    0,   33,   33,    0,   20,    0,
    0,   33,   20,    0,    0,   20,   33,    0,   33,   33,
   33,   33,    0,    0,    0,    0,   33,   20,   20,   33,
    0,   33,   20,   20,    0,    0,    0,    0,   20,    0,
   20,   20,   20,   20,   33,   33,    0,    0,   20,    0,
    0,   20,   23,   20,    0,    0,   33,    0,    0,   33,
    0,   33,   33,    0,    0,   33,   20,    0,   33,    0,
    0,    0,    0,   33,   20,   20,    0,    0,   33,    0,
   33,   33,   33,    0,    0,   33,   33,    0,    0,    0,
    0,   33,    0,   33,   33,   33,   33,   33,   33,    0,
    0,   33,   33,   33,   33,    0,   33,    0,   33,    0,
   33,   33,   33,   33,   22,    0,    0,   33,   33,   33,
    0,   33,    0,   33,    0,    0,    0,   22,   33,    0,
    0,    0,    0,   33,    0,    0,   33,   33,    0,    0,
   33,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,   33,   33,    0,    0,    0,  255,   33,  255,
  440,  255,  440,   33,  440,   33,   33,   33,   33,  255,
    0,    0,    0,   33,    0,    0,   33,    0,   33,  255,
  259,  255,  259,  443,  259,  443,    0,  443,    0,    0,
    0,   33,  259,    0,    0,    0,    0,    0,    0,  255,
    0,  255,  259,  255,  259,  255,    0,  255,    0,  255,
    0,  255,    0,  255,    0,  255,    0,  255,    0,    0,
    0,    0,  259,    0,  259,    0,  259,    0,  259,    0,
  259,    0,  259,  255,  259,    0,  259,    0,  259,  260,
  259,  260,  444,  260,  444,    0,  444,    0,    0,    0,
  299,  260,  299,  447,  299,  447,  259,  447,    0,    0,
    0,  260,  299,  260,    0,    0,    0,    0,    0,    0,
    0,    0,  299,    0,  299,    0,    0,    0,    0,    0,
    0,  260,    0,  260,    0,  260,    0,  260,    0,  260,
    0,  260,  299,  260,  299,  260,  299,  260,  299,  260,
  299,   16,  299,   16,  299,   16,  299,    0,  299,    0,
  299,    0,    0,   16,    0,  260,    0,    0,    0,    0,
    0,    0,    0,   16,    0,   16,  299,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,   16,    0,   16,    0,   16,    0,   16,
    0,   16,    0,   16,    0,   16,    0,   16,    0,   16,
    0,   16,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,   16,
  };
  protected static  short [] yyCheck = {             3,
  235,   24,  322,  267,  326,  519,  491,  281,    1,   13,
  607,  275,  236,  467,  323,  625,  345,   43,  643,  347,
  333,  257,  263,  342,  260,   18,  268,   31,  264,  459,
  266,   24,  343,  269,   27,  271,  272,  343,  274,  313,
  276,  341,  278,  285,  280,  281,  282,  283,  607,  262,
  350,  287,  346,  347,  470,  343,  292,  323,  294,  295,
  296,  620,  564,  299,  300,  301,  343,  303,  364,   95,
  306,  393,  308,  309,  310,  311,  343,  343,  304,  315,
  316,  317,  323,  319,  320,  321,  343,  383,   81,  519,
  419,  419,  328,  329,  596,  304,  332,  323,  334,  335,
  419,  314,   95,  343,  343,  261,  342,  343,  419,  343,
  419,  343,  626,  419,  323,  351,  343,  343,  728,  729,
  536,  347,   96,   97,   98,   99,  100,  101,  102,  103,
  153,  419,  454,  730,  343,  161,  638,  163,  592,  641,
  364,  297,  419,  267,  170,  419,  262,   81,  343,  604,
  161,  275,  419,  419,  361,  419,  343,  183,  184,  346,
  153,   95,  419,  156,  190,  191,  192,  193,  194,  195,
  196,  197,  198,  199,  200,  168,  169,  607,  803,  419,
  419,  454,  419,  419,  343,  419,  337,  419,  381,  644,
  620,  342,  419,  419,  220,    1,  626,  470,  314,  361,
  356,  357,  361,  800,  361,  267,  413,  361,  234,  300,
  419,  370,   18,  275,  361,  208,  209,  373,  342,  375,
  246,  247,  156,  234,  419,  361,  343,  201,  202,  203,
  204,  205,  206,  207,  168,  169,  210,  211,  212,  213,
  214,  215,  216,  217,  218,  219,  355,  221,  733,  734,
  361,  413,  343,  361,  413,  343,  413,  355,  346,  413,
  341,  746,  455,  748,  457,  459,  413,  241,  349,  243,
  347,  591,  298,  350,  208,  209,  385,  413,  387,  325,
  389,  342,  391,  287,  393,  344,  395,  348,  397,  348,
  399,  317,  401,  350,  403,  488,  266,  491,  344,  269,
  352,  353,  413,  326,  341,  413,  271,  500,  501,   81,
  419,  276,  342,  350,  621,  280,  362,  802,  348,  639,
  324,  628,  342,  343,  294,  519,  319,  841,  341,  299,
  295,  347,  345,  326,  347,  348,  329,  350,  823,  343,
  339,  337,  305,  342,  307,  315,  342,  317,  341,  312,
  360,  361,  362,  838,  358,  271,  321,  361,  328,  329,
  276,  343,  332,  556,  280,  347,  370,  360,  503,  419,
  393,  344,  345,  508,  419,  348,  344,  344,  344,  295,
  348,  348,  348,  341,  156,  319,  339,  345,  392,  342,
  416,  384,  396,  386,  341,  329,  168,  169,  345,  829,
  393,  204,  205,  723,  341,  321,  432,  341,  345,  413,
  341,  841,  350,  607,  345,  341,  343,  349,  345,  345,
  347,  343,  257,  345,  344,  347,  360,  420,  348,  452,
  345,  454,  626,  346,  345,  348,  208,  209,  365,  345,
  367,  467,  345,  365,  437,  367,  369,  470,  371,  377,
  384,  379,  386,  206,  207,  448,  449,  347,  343,  452,
  345,  454,  210,  211,  212,  213,  348,  660,  350,  358,
  463,  497,  348,  499,  350,  459,  381,  470,  471,  472,
  363,  348,  486,  350,  359,  459,  420,  343,  343,  345,
  345,  337,  337,  339,  339,  688,  489,  346,  341,  348,
  344,  419,  346,  437,  345,  419,  347,  491,  337,  419,
  339,  214,  215,  346,  448,  449,  346,  491,  419,  419,
  419,  419,  350,  349,  346,  459,  343,  346,  346,  463,
  341,  345,  348,  348,  727,  519,  730,  471,  472,  733,
  734,  349,  349,  344,  349,  519,  739,  349,  348,  346,
  743,  342,  746,  341,  748,  489,  341,  341,  793,  794,
  346,  754,  755,  342,  341,  348,  592,  419,  348,  341,
  348,  342,  459,  355,  350,  301,  342,  342,  345,  304,
  419,  304,  419,  609,  345,  519,  612,  613,  360,  341,
  616,  341,  419,  347,  343,  346,  600,  601,  345,  625,
  355,  347,  628,  345,  491,  350,  800,  350,  802,  459,
  345,  345,  384,  345,  386,  608,  345,  643,  345,  459,
  350,  343,  342,  607,  345,  349,  341,  419,  345,  823,
  343,  350,  519,  607,  350,  829,  346,  355,  831,  345,
  342,  491,  626,  647,  838,  419,  336,  841,  420,  350,
  350,  491,  626,  657,  350,  648,  649,  350,  350,  348,
  653,  419,  346,  349,  344,  437,  459,  346,  694,  519,
  696,  343,  339,  607,  608,  337,  448,  449,  348,  519,
  345,  350,  350,  346,  285,  346,  620,  346,  345,  268,
  268,  463,  626,  346,  344,  346,  344,  346,  491,  471,
  472,  705,  728,  729,  346,  342,  293,  711,  712,  341,
  350,  341,  350,  262,  648,  649,  350,  489,  314,  653,
  607,  459,  346,  346,  345,  345,  519,  342,  344,  346,
  349,  264,  350,  266,  279,  342,  269,  350,  350,  626,
  350,  274,  346,  736,  342,  278,  730,  773,  337,  733,
  734,  346,  346,  491,  287,  346,  730,  607,  346,  733,
  734,  294,  746,  349,  748,  339,  299,  607,  350,  346,
  303,  342,  746,  341,  748,  350,  626,  803,  347,  346,
  341,  519,  315,  355,  317,  344,  626,  320,  346,  341,
  341,  341,  419,  341,  350,  328,  329,  344,  341,  332,
  342,  341,  736,  796,  342,  419,  342,  341,  812,  813,
  814,  350,  314,  385,  607,  387,  800,  389,  802,  391,
  419,  393,  350,  395,  262,  397,  800,  399,  802,  401,
  350,  403,  342,  626,  346,  346,  608,  342,  346,  823,
  223,  287,    4,  730,   31,  829,  733,  734,   15,  823,
  153,  370,   28,  331,  838,  829,  329,  841,  413,  746,
  593,  748,  796,  389,  838,  365,  389,  841,  657,  607,
  601,  396,  600,  727,  712,  711,  648,  649,  293,  812,
  730,  653,  334,  733,  734,  392,  419,  316,  626,  247,
  730,  216,  218,  733,  734,  829,  746,  217,  748,  221,
  219,   65,  806,   -1,  829,  800,  746,  841,  748,  623,
  691,  808,   -1,  800,  499,  802,  689,  692,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  346,
   -1,  348,   -1,  350,   -1,   -1,  823,  730,  355,   -1,
  733,  734,  829,   -1,  284,   -1,   -1,   -1,   -1,   -1,
  800,  838,  802,  746,  841,  748,   -1,   -1,   -1,   -1,
  800,   -1,  802,   -1,  736,   -1,   -1,   -1,  385,   -1,
  387,   -1,  389,  823,  391,   -1,  393,   -1,  395,  829,
  397,   -1,  399,  823,  401,  325,  403,   -1,  838,  829,
   -1,  841,  730,   -1,   -1,  733,  734,   -1,  838,   -1,
   -1,  841,   -1,   -1,   -1,   -1,   -1,  800,  746,  802,
  748,  351,  352,  353,  354,   -1,  356,  357,  358,  359,
  360,  361,  362,  363,  796,  365,   -1,  367,   -1,  369,
  823,  371,   -1,  373,   -1,  375,  829,  377,   -1,  379,
   -1,   -1,   -1,   -1,   -1,  838,   -1,   -1,  841,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  800,   -1,  802,   -1,   -1,   -1,   -1,  257,
   -1,   -1,  260,   -1,  262,   -1,  264,   -1,  266,   -1,
   -1,  269,   -1,  271,  272,  823,  274,   -1,  276,   -1,
  278,  829,  280,  281,  282,  283,   -1,   -1,   -1,  287,
  838,   -1,   -1,  841,  292,   -1,  294,  295,  296,   -1,
   -1,  299,  300,  301,   -1,  303,   -1,  305,  306,  307,
  308,  309,  310,  311,  312,   -1,  314,  315,  316,  317,
   -1,  319,  320,  321,   -1,   -1,   -1,   -1,   -1,   -1,
  328,  329,   -1,   -1,  332,   -1,  334,  335,  260,  337,
   -1,  339,  264,   -1,  266,   -1,   -1,  269,   -1,  271,
  272,   -1,  274,  351,  276,   -1,  278,   -1,  280,  281,
  282,  283,   -1,   -1,   -1,  287,   -1,   -1,   -1,   -1,
  292,   -1,  294,  295,  296,   -1,   -1,  299,   -1,  301,
   -1,  303,   -1,   -1,  306,   -1,  308,  309,  310,  311,
   -1,   -1,   -1,  315,  316,  317,   -1,  319,  320,  321,
   -1,   -1,   -1,   -1,   -1,   -1,  328,  329,   -1,   -1,
  332,   -1,  334,  335,   -1,   -1,   -1,   -1,   -1,  260,
  342,  419,   -1,  264,   -1,  266,   -1,   -1,  269,  351,
  271,  272,   -1,  274,   -1,  276,   -1,  278,   -1,  280,
  281,  282,  283,   -1,   -1,   -1,  287,   -1,   -1,   -1,
   -1,  292,   -1,  294,  295,  296,   -1,   -1,  299,   -1,
  301,   -1,  303,   -1,   -1,  306,   -1,  308,  309,  310,
  311,   -1,   -1,   -1,  315,  316,  317,   -1,  319,  320,
  321,   -1,   -1,   -1,   -1,   -1,   -1,  328,  329,   -1,
   -1,  332,   -1,  334,  335,   -1,   -1,  419,   -1,   -1,
  260,  342,   -1,   -1,  264,   -1,  266,   -1,   -1,  269,
  351,  271,  272,   -1,  274,   -1,  276,   -1,  278,   -1,
  280,  281,  282,  283,   -1,   -1,   -1,  287,   -1,   -1,
   -1,   -1,  292,   -1,  294,  295,  296,   -1,   -1,  299,
   -1,  301,   -1,  303,   -1,   -1,  306,   -1,  308,  309,
  310,  311,   -1,   -1,   -1,  315,  316,  317,   -1,  319,
  320,  321,   -1,   -1,   -1,   -1,   -1,   -1,  328,  329,
   -1,   -1,  332,   -1,  334,  335,   -1,   -1,  419,  260,
   -1,   -1,  342,  264,   -1,  266,   -1,   -1,  269,   -1,
  271,  272,   -1,  274,   -1,  276,   -1,  278,   -1,  280,
  281,  282,  283,   -1,   -1,   -1,  287,   -1,   -1,   -1,
   -1,  292,   -1,  294,  295,  296,   -1,   -1,  299,   -1,
  301,   -1,  303,   -1,   -1,  306,   -1,  308,  309,  310,
  311,   -1,   -1,   -1,  315,  316,  317,   -1,  319,  320,
  321,   -1,   -1,   -1,   -1,   -1,   -1,  328,  329,   -1,
   -1,  332,   -1,  334,  335,   -1,   -1,   -1,   -1,  419,
  261,  342,  263,  264,   -1,  266,   -1,   -1,  269,  270,
   -1,   -1,   -1,  274,   -1,   -1,   -1,  278,   -1,   -1,
   -1,   -1,   -1,  284,   -1,   -1,  287,   -1,   -1,   -1,
   -1,   -1,   -1,  294,   -1,   -1,  297,   -1,  299,   -1,
  301,  302,  303,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  315,   -1,  317,  318,   -1,  320,
   -1,   -1,  323,   -1,  325,   -1,  327,  328,  329,  330,
  346,  332,  348,   -1,  350,   -1,   -1,   -1,  419,  355,
  341,  342,   -1,  344,  345,  346,  347,  348,  349,  350,
  351,  352,  353,  354,  355,  356,  357,  358,  359,  360,
  361,  362,  363,  364,  365,   -1,  367,   -1,  369,  385,
  371,  387,  373,  389,  375,  391,  377,  393,  379,  395,
  381,  397,  383,  399,  385,  401,  387,  403,  389,   -1,
  391,   -1,  393,   -1,  395,   -1,  397,   -1,  399,   -1,
  401,   -1,  403,   -1,   -1,   -1,  407,   -1,  409,   -1,
  411,   -1,  413,   -1,  415,   -1,  417,  261,  419,  263,
  264,   -1,  266,   -1,   -1,  269,  270,   -1,   -1,   -1,
  274,   -1,   -1,   -1,  278,   -1,   -1,   -1,   -1,   -1,
  284,   -1,   -1,  287,   -1,   -1,   -1,   -1,   -1,   -1,
  294,   -1,   -1,  297,   -1,  299,   -1,  301,  302,  303,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  315,   -1,  317,  318,   -1,  320,   -1,   -1,  323,
   -1,  325,   -1,  327,  328,  329,  330,   -1,  332,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  341,  342,   -1,
  344,  345,  346,  347,  348,  349,  350,  351,  352,  353,
  354,  355,  356,  357,  358,  359,  360,  361,  362,  363,
  364,  365,   -1,  367,   -1,  369,   -1,  371,   -1,  373,
   -1,  375,   -1,  377,   -1,  379,   -1,  381,   -1,  383,
   -1,  385,   -1,  387,   -1,  389,   -1,  391,   -1,  393,
   -1,  395,   -1,  397,   -1,  399,   -1,  401,   -1,  403,
   -1,   -1,   -1,  407,   -1,  409,   -1,  411,   -1,  413,
   -1,  415,   -1,  417,  261,  419,  263,  264,   -1,  266,
   -1,   -1,  269,  270,   -1,   -1,   -1,  274,   -1,   -1,
   -1,  278,   -1,   -1,   -1,   -1,   -1,  284,   -1,   -1,
  287,   -1,   -1,   -1,   -1,   -1,   -1,  294,   -1,   -1,
  297,   -1,  299,   -1,  301,  302,  303,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  315,   -1,
  317,  318,   -1,  320,   -1,   -1,  323,   -1,  325,   -1,
  327,  328,  329,  330,   -1,  332,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  342,  343,  344,  345,  346,
  347,  348,  349,  350,  351,  352,  353,  354,  355,  356,
  357,  358,  359,  360,  361,  362,  363,  364,  365,   -1,
  367,   -1,  369,   -1,  371,   -1,  373,   -1,  375,   -1,
  377,   -1,  379,   -1,  381,   -1,  383,   -1,  385,   -1,
  387,   -1,  389,   -1,  391,   -1,  393,   -1,  395,   -1,
  397,   -1,  399,   -1,  401,   -1,  403,   -1,   -1,   -1,
  407,   -1,  409,   -1,  411,   -1,  413,   -1,  415,   -1,
  417,  261,  419,  263,  264,   -1,  266,   -1,   -1,  269,
  270,   -1,   -1,   -1,  274,   -1,   -1,   -1,  278,   -1,
   -1,   -1,   -1,   -1,  284,   -1,   -1,  287,   -1,   -1,
   -1,   -1,   -1,   -1,  294,   -1,   -1,  297,   -1,  299,
   -1,  301,  302,  303,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  315,   -1,  317,  318,   -1,
  320,   -1,   -1,  323,   -1,  325,   -1,  327,  328,  329,
  330,   -1,  332,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  342,  343,  344,  345,  346,   -1,  348,  349,
  350,  351,  352,  353,  354,  355,  356,  357,  358,  359,
  360,  361,  362,  363,  364,  365,   -1,  367,   -1,  369,
   -1,  371,   -1,  373,   -1,  375,   -1,  377,   -1,  379,
   -1,  381,   -1,  383,   -1,  385,   -1,  387,   -1,  389,
   -1,  391,   -1,  393,   -1,  395,   -1,  397,   -1,  399,
   -1,  401,   -1,  403,   -1,   -1,   -1,  407,   -1,  409,
   -1,  411,   -1,  413,   -1,  415,   -1,  417,  261,  419,
  263,  264,   -1,  266,   -1,   -1,  269,  270,   -1,   -1,
   -1,  274,   -1,   -1,   -1,  278,   -1,   -1,   -1,   -1,
   -1,  284,   -1,   -1,  287,   -1,   -1,   -1,   -1,   -1,
   -1,  294,   -1,   -1,  297,   -1,  299,   -1,  301,  302,
  303,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  315,   -1,  317,  318,   -1,  320,   -1,   -1,
  323,   -1,  325,   -1,  327,  328,  329,  330,   -1,  332,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  342,
   -1,  344,   -1,  346,   -1,  348,  349,  350,  351,  352,
  353,  354,  355,  356,  357,  358,  359,  360,  361,  362,
  363,  364,   -1,   -1,   -1,   -1,  369,   -1,  371,   -1,
  373,   -1,  375,   -1,  377,   -1,  379,   -1,  381,   -1,
  383,   -1,  385,   -1,  387,   -1,  389,   -1,  391,   -1,
  393,   -1,  395,   -1,  397,   -1,  399,   -1,  401,   -1,
  403,   -1,   -1,   -1,  407,   -1,  409,   -1,  411,   -1,
  413,   -1,  415,   -1,  417,  261,  419,  263,  264,   -1,
  266,   -1,   -1,  269,  270,   -1,   -1,   -1,  274,   -1,
   -1,   -1,  278,   -1,   -1,   -1,   -1,   -1,  284,   -1,
   -1,  287,   -1,   -1,   -1,   -1,   -1,   -1,  294,   -1,
   -1,  297,   -1,  299,   -1,  301,  302,  303,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  315,
   -1,  317,  318,   -1,  320,   -1,   -1,  323,   -1,  325,
   -1,  327,  328,  329,  330,   -1,  332,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  342,   -1,  344,  345,
  346,   -1,  348,  349,  350,  351,  352,  353,  354,   -1,
  356,  357,  358,  359,  360,  361,  362,  363,  364,  365,
   -1,  367,   -1,  369,   -1,  371,   -1,  373,   -1,  375,
   -1,  377,   -1,  379,   -1,  381,   -1,  383,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  261,
   -1,  263,  264,   -1,  266,   -1,   -1,  269,  270,   -1,
   -1,  407,  274,  409,   -1,  411,  278,  413,   -1,  415,
   -1,  417,  284,  419,   -1,  287,   -1,   -1,   -1,   -1,
   -1,   -1,  294,   -1,   -1,  297,   -1,  299,   -1,  301,
  302,  303,  304,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  315,   -1,  317,  318,   -1,  320,   -1,
   -1,  323,   -1,  325,   -1,  327,  328,  329,  330,   -1,
  332,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  341,
  342,  343,  344,  345,  346,   -1,  348,  349,  350,  351,
  352,  353,  354,   -1,  356,  357,  358,  359,  360,   -1,
   -1,  363,  364,  365,   -1,  367,   -1,   -1,   -1,   -1,
   -1,  373,   -1,  375,   -1,  377,   -1,  379,   -1,  381,
   -1,  383,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  261,   -1,  263,  264,   -1,  266,   -1,
   -1,  269,  270,   -1,   -1,  407,  274,  409,   -1,  411,
  278,  413,   -1,  415,   -1,  417,  284,  419,   -1,  287,
   -1,   -1,   -1,   -1,   -1,   -1,  294,   -1,   -1,  297,
   -1,  299,   -1,  301,  302,  303,  304,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  315,   -1,  317,
  318,   -1,  320,   -1,   -1,  323,   -1,  325,   -1,  327,
  328,  329,  330,   -1,  332,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  341,  342,   -1,  344,  345,  346,   -1,
  348,  349,  350,  351,  352,  353,  354,   -1,  356,  357,
  358,  359,  360,   -1,   -1,  363,  364,  365,   -1,  367,
   -1,   -1,   -1,   -1,   -1,  373,   -1,  375,   -1,  377,
   -1,  379,   -1,  381,   -1,  383,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  261,   -1,  263,
  264,   -1,  266,   -1,   -1,  269,  270,   -1,   -1,  407,
  274,  409,   -1,  411,  278,  413,   -1,  415,   -1,  417,
  284,  419,   -1,  287,   -1,   -1,   -1,   -1,   -1,   -1,
  294,   -1,   -1,  297,   -1,  299,   -1,  301,  302,  303,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  315,   -1,  317,  318,   -1,  320,   -1,   -1,  323,
   -1,  325,   -1,  327,  328,  329,  330,   -1,  332,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  342,   -1,
  344,  345,  346,   -1,  348,  349,  350,  351,  352,  353,
  354,   -1,  356,  357,  358,  359,   -1,   -1,   -1,  363,
  364,  365,   -1,  367,   -1,  369,   -1,  371,   -1,  373,
   -1,  375,   -1,  377,   -1,  379,   -1,  381,   -1,  383,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  261,   -1,  263,  264,   -1,  266,   -1,   -1,  269,
  270,   -1,   -1,  407,  274,  409,   -1,  411,  278,  413,
   -1,  415,   -1,  417,  284,  419,   -1,  287,   -1,   -1,
   -1,   -1,   -1,   -1,  294,   -1,   -1,  297,   -1,  299,
   -1,  301,  302,  303,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  315,   -1,  317,  318,   -1,
  320,   -1,   -1,  323,   -1,  325,   -1,  327,  328,  329,
  330,   -1,  332,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  342,   -1,  344,  345,  346,   -1,  348,  349,
  350,  351,  352,  353,  354,   -1,  356,  357,  358,  359,
   -1,   -1,   -1,  363,  364,  365,   -1,  367,   -1,  369,
   -1,  371,   -1,  373,   -1,  375,   -1,  377,   -1,  379,
   -1,  381,   -1,  383,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  261,   -1,  263,  264,   -1,
  266,   -1,   -1,  269,  270,   -1,   -1,  407,  274,  409,
   -1,  411,  278,  413,   -1,  415,   -1,  417,  284,  419,
   -1,  287,   -1,   -1,   -1,   -1,   -1,   -1,  294,   -1,
   -1,  297,   -1,  299,   -1,  301,  302,  303,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  315,
   -1,  317,  318,   -1,  320,   -1,   -1,  323,   -1,  325,
   -1,  327,  328,  329,  330,   -1,  332,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  342,   -1,  344,  345,
  346,   -1,  348,  349,  350,  351,  352,  353,  354,   -1,
  356,  357,  358,  359,   -1,   -1,   -1,  363,  364,  365,
   -1,  367,   -1,  369,   -1,  371,   -1,  373,   -1,  375,
   -1,  377,   -1,  379,   -1,  381,   -1,  383,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  261,
   -1,  263,  264,   -1,  266,   -1,   -1,  269,  270,   -1,
   -1,  407,  274,  409,   -1,  411,  278,  413,   -1,  415,
   -1,  417,  284,  419,   -1,  287,   -1,   -1,   -1,   -1,
   -1,   -1,  294,   -1,   -1,  297,   -1,  299,   -1,  301,
  302,  303,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  315,   -1,  317,  318,   -1,  320,   -1,
   -1,  323,   -1,  325,   -1,  327,  328,  329,  330,   -1,
  332,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  342,   -1,  344,  345,  346,   -1,  348,  349,  350,  351,
   -1,   -1,  354,   -1,  356,  357,  358,  359,  360,   -1,
   -1,  363,  364,  365,   -1,  367,   -1,  369,   -1,  371,
   -1,  373,   -1,  375,   -1,  377,   -1,  379,   -1,  381,
   -1,  383,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  261,   -1,  263,  264,   -1,  266,   -1,
   -1,  269,  270,   -1,   -1,  407,  274,  409,   -1,  411,
  278,  413,   -1,  415,   -1,  417,  284,  419,   -1,  287,
   -1,   -1,   -1,   -1,   -1,   -1,  294,   -1,   -1,  297,
   -1,  299,   -1,  301,  302,  303,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  315,   -1,  317,
  318,   -1,  320,   -1,   -1,  323,   -1,  325,   -1,  327,
  328,  329,  330,   -1,  332,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  342,   -1,  344,  345,  346,   -1,
  348,  349,  350,  351,  352,  353,  354,   -1,  356,  357,
  358,  359,  360,   -1,   -1,  363,  364,  365,   -1,  367,
   -1,   -1,   -1,   -1,   -1,  373,   -1,  375,   -1,  377,
   -1,  379,   -1,  381,   -1,  383,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  261,   -1,  263,
  264,   -1,  266,   -1,   -1,  269,  270,   -1,   -1,  407,
  274,  409,   -1,  411,  278,  413,   -1,  415,   -1,  417,
  284,  419,   -1,  287,   -1,   -1,   -1,   -1,   -1,   -1,
  294,   -1,   -1,  297,   -1,  299,   -1,  301,  302,  303,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  315,   -1,  317,  318,   -1,  320,   -1,   -1,  323,
   -1,  325,   -1,  327,  328,  329,  330,   -1,  332,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  342,   -1,
  344,  345,  346,   -1,  348,  349,  350,  351,   -1,   -1,
  354,   -1,  356,  357,  358,  359,  360,   -1,   -1,  363,
  364,  365,   -1,  367,   -1,  369,   -1,  371,   -1,  373,
   -1,  375,   -1,  377,   -1,  379,   -1,  381,   -1,  383,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  261,   -1,  263,  264,   -1,  266,   -1,   -1,  269,
  270,   -1,   -1,  407,  274,  409,   -1,  411,  278,  413,
   -1,  415,   -1,  417,  284,  419,   -1,  287,   -1,   -1,
   -1,   -1,   -1,   -1,  294,   -1,   -1,  297,   -1,  299,
   -1,  301,  302,  303,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  315,   -1,  317,  318,   -1,
  320,   -1,   -1,  323,   -1,  325,   -1,  327,  328,  329,
  330,   -1,  332,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  342,   -1,  344,  345,  346,   -1,  348,  349,
  350,  351,   -1,   -1,  354,   -1,  356,  357,  358,  359,
  360,   -1,   -1,  363,  364,  365,   -1,  367,   -1,  369,
   -1,  371,   -1,  373,   -1,  375,   -1,  377,   -1,  379,
   -1,  381,   -1,  383,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  261,   -1,  263,  264,   -1,
  266,   -1,   -1,  269,  270,   -1,   -1,  407,  274,  409,
   -1,  411,  278,  413,   -1,  415,   -1,  417,  284,  419,
   -1,  287,   -1,   -1,   -1,   -1,   -1,   -1,  294,   -1,
   -1,  297,   -1,  299,   -1,  301,  302,  303,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  315,
   -1,  317,  318,   -1,  320,   -1,   -1,  323,   -1,  325,
   -1,  327,  328,  329,  330,   -1,  332,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  342,   -1,  344,  345,
  346,   -1,  348,  349,  350,  351,  352,  353,  354,   -1,
  356,  357,  358,  359,  360,   -1,   -1,  363,  364,  365,
   -1,  367,   -1,   -1,   -1,   -1,   -1,  373,   -1,  375,
   -1,  377,   -1,  379,   -1,  381,   -1,  383,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  261,
   -1,  263,  264,   -1,  266,   -1,   -1,  269,  270,   -1,
   -1,  407,  274,  409,   -1,  411,  278,  413,   -1,  415,
   -1,  417,  284,  419,   -1,  287,   -1,   -1,   -1,   -1,
   -1,   -1,  294,   -1,   -1,  297,   -1,  299,   -1,  301,
  302,  303,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  315,   -1,  317,  318,   -1,  320,   -1,
   -1,  323,   -1,  325,   -1,  327,  328,  329,  330,   -1,
  332,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  342,   -1,  344,  345,  346,   -1,  348,  349,  350,  351,
  352,  353,  354,   -1,  356,  357,  358,  359,  360,   -1,
   -1,  363,  364,  365,   -1,  367,   -1,   -1,   -1,   -1,
   -1,  373,   -1,  375,   -1,  377,   -1,  379,   -1,  381,
   -1,  383,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  261,   -1,  263,  264,   -1,  266,   -1,
   -1,  269,  270,   -1,   -1,  407,  274,  409,   -1,  411,
  278,  413,   -1,  415,   -1,  417,  284,  419,   -1,  287,
   -1,   -1,   -1,   -1,   -1,   -1,  294,   -1,   -1,  297,
   -1,  299,   -1,  301,  302,  303,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  315,   -1,  317,
  318,   -1,  320,   -1,   -1,  323,   -1,  325,   -1,  327,
  328,  329,  330,   -1,  332,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  342,   -1,  344,  345,  346,   -1,
  348,  349,  350,  351,  352,  353,  354,   -1,  356,  357,
  358,  359,  360,   -1,   -1,  363,  364,  365,   -1,  367,
   -1,   -1,   -1,   -1,   -1,  373,   -1,  375,   -1,  377,
   -1,  379,   -1,  381,   -1,  383,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  261,   -1,  263,
  264,   -1,  266,   -1,   -1,  269,  270,   -1,   -1,  407,
  274,  409,   -1,  411,  278,  413,   -1,  415,   -1,  417,
  284,  419,   -1,  287,   -1,   -1,   -1,   -1,   -1,   -1,
  294,   -1,   -1,  297,   -1,  299,   -1,  301,  302,  303,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  315,   -1,  317,  318,   -1,  320,   -1,   -1,  323,
   -1,  325,   -1,  327,  328,  329,  330,   -1,  332,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  342,   -1,
  344,  345,  346,   -1,  348,  349,  350,  351,  352,  353,
  354,   -1,  356,  357,  358,  359,  360,   -1,   -1,  363,
  364,  365,   -1,  367,   -1,   -1,   -1,   -1,   -1,  373,
   -1,  375,   -1,  377,   -1,  379,   -1,  381,   -1,  383,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  261,   -1,  263,  264,   -1,  266,   -1,   -1,  269,
  270,   -1,   -1,  407,  274,  409,   -1,  411,  278,  413,
   -1,  415,   -1,  417,  284,  419,   -1,  287,   -1,   -1,
   -1,   -1,   -1,   -1,  294,   -1,   -1,  297,   -1,  299,
   -1,  301,  302,  303,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  315,   -1,  317,  318,   -1,
  320,   -1,   -1,  323,   -1,  325,   -1,  327,  328,  329,
  330,   -1,  332,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  342,   -1,  344,  345,  346,   -1,  348,  349,
  350,  351,  352,  353,  354,   -1,  356,  357,  358,  359,
  360,   -1,   -1,  363,  364,  365,   -1,  367,   -1,   -1,
   -1,   -1,   -1,  373,   -1,  375,   -1,  377,   -1,  379,
   -1,  381,   -1,  383,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  261,   -1,  263,  264,   -1,
  266,   -1,   -1,  269,  270,   -1,   -1,  407,  274,  409,
   -1,  411,  278,  413,   -1,  415,   -1,  417,  284,  419,
   -1,  287,   -1,   -1,   -1,   -1,   -1,   -1,  294,   -1,
   -1,  297,   -1,  299,   -1,  301,  302,  303,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  315,
   -1,  317,  318,   -1,  320,   -1,   -1,  323,   -1,  325,
   -1,  327,  328,  329,  330,   -1,  332,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  342,   -1,  344,  345,
  346,   -1,  348,  349,  350,  351,  352,  353,  354,   -1,
  356,  357,  358,  359,  360,   -1,   -1,  363,  364,  365,
   -1,  367,   -1,   -1,   -1,   -1,   -1,  373,   -1,  375,
   -1,  377,   -1,  379,   -1,  381,   -1,  383,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  407,   -1,  409,   -1,  411,   -1,  413,   -1,  415,
   -1,  417,   -1,  419,  263,  264,  265,  266,  267,   -1,
  269,  270,   -1,  272,  273,  274,  275,   -1,  277,  278,
  279,   -1,   -1,   -1,   -1,  284,  285,   -1,  287,  288,
  289,  290,  291,   -1,   -1,  294,   -1,   -1,   -1,  298,
  299,   -1,  301,  302,  303,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  313,   -1,  315,   -1,  317,  318,
   -1,  320,   -1,  322,  323,  324,  325,  326,  327,  328,
  329,  330,   -1,  332,  333,  260,  335,  336,   -1,   -1,
   -1,   -1,  341,  342,   -1,   -1,  345,   -1,   -1,   -1,
   -1,  350,  351,  352,  353,  354,   -1,   -1,  283,  358,
   -1,  360,   -1,   -1,   -1,   -1,  365,   -1,  367,   -1,
   -1,  296,   -1,   -1,   -1,   -1,  301,   -1,   -1,   -1,
   -1,  306,   -1,  308,  309,  310,  311,   -1,   -1,   -1,
   -1,  316,   -1,   -1,  319,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  407,  334,
  409,   -1,  411,   -1,  413,   -1,  415,   -1,  417,   -1,
  419,  263,  264,  265,  266,  267,  351,  269,  270,   -1,
  272,  273,  274,  275,   -1,  277,  278,  279,   -1,   -1,
   -1,   -1,  284,   -1,   -1,  287,  288,  289,  290,  291,
   -1,   -1,  294,   -1,   -1,   -1,  298,  299,   -1,  301,
  302,  303,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  313,   -1,  315,   -1,  317,  318,   -1,  320,   -1,
  322,  323,  324,  325,  326,  327,  328,  329,  330,   -1,
  332,  333,  260,  335,  336,   -1,   -1,   -1,   -1,  341,
  342,   -1,   -1,  345,   -1,   -1,   -1,   -1,  350,  351,
  352,  353,  354,   -1,   -1,  283,  358,   -1,  360,   -1,
   -1,   -1,   -1,  365,   -1,  367,   -1,   -1,  296,   -1,
   -1,   -1,   -1,  301,   -1,   -1,   -1,   -1,  306,   -1,
  308,  309,  310,  311,   -1,   -1,   -1,   -1,  316,   -1,
   -1,  319,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  407,  334,  409,   -1,  411,
   -1,  413,   -1,  415,   -1,  417,   -1,  419,  263,  264,
  265,  266,  267,   -1,  269,  270,   -1,  272,  273,  274,
  275,   -1,  277,  278,   -1,   -1,   -1,   -1,   -1,  284,
   -1,   -1,  287,  288,  289,  290,  291,   -1,   -1,  294,
   -1,   -1,   -1,  298,  299,   -1,  301,  302,  303,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  313,   -1,
  315,   -1,  317,  318,   -1,  320,   -1,  322,  323,  324,
  325,  326,  327,  328,  329,  330,   -1,  332,  333,   -1,
  335,  336,   -1,   -1,   -1,   -1,  341,  342,   -1,   -1,
  345,   -1,   -1,   -1,   -1,  350,  351,  352,  353,  354,
   -1,   -1,   -1,  358,   -1,  360,   -1,   -1,   -1,   -1,
  365,   -1,  367,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  407,   -1,  409,   -1,  411,   -1,  413,   -1,
  415,   -1,  417,   -1,  419,  263,  264,  265,  266,   -1,
   -1,  269,  270,   -1,  272,  273,  274,   -1,   -1,  277,
  278,   -1,   -1,   -1,   -1,   -1,  284,   -1,   -1,  287,
  288,  289,  290,  291,   -1,   -1,  294,   -1,   -1,   -1,
  298,  299,   -1,  301,  302,  303,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  313,   -1,  315,   -1,  317,
  318,   -1,  320,   -1,  322,  323,  324,  325,  326,  327,
  328,  329,  330,   -1,  332,  333,   -1,  335,  336,   -1,
   -1,   -1,   -1,  341,   -1,   -1,   -1,  345,   -1,   -1,
   -1,   -1,  350,  351,  352,  353,  354,   -1,   -1,   -1,
  358,   -1,  360,   -1,   -1,   -1,   -1,  365,   -1,  367,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  407,
   -1,  409,   -1,  411,   -1,  413,   -1,  415,   -1,  417,
   -1,  419,  263,  264,  265,  266,   -1,   -1,  269,  270,
   -1,  272,  273,  274,   -1,   -1,  277,  278,   -1,   -1,
   -1,   -1,   -1,  284,   -1,   -1,  287,  288,  289,  290,
  291,   -1,   -1,  294,   -1,   -1,   -1,  298,  299,   -1,
  301,  302,  303,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  313,   -1,  315,   -1,  317,  318,   -1,  320,
   -1,  322,  323,  324,  325,  326,  327,  328,  329,  330,
   -1,  332,  333,   -1,  335,  336,   -1,   -1,   -1,   -1,
  341,   -1,   -1,   -1,  345,   -1,   -1,   -1,   -1,  350,
  351,  352,  353,  354,   -1,   -1,   -1,  358,   -1,  360,
   -1,   -1,   -1,   -1,  365,   -1,  367,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  407,   -1,  409,   -1,
  411,   -1,  413,   -1,  415,   -1,  417,   -1,  419,  263,
  264,  265,  266,   -1,   -1,  269,  270,   -1,   -1,  273,
  274,   -1,   -1,  277,  278,   -1,   -1,   -1,   -1,   -1,
  284,   -1,   -1,  287,  288,  289,  290,  291,   -1,   -1,
  294,   -1,   -1,   -1,  298,  299,   -1,  301,  302,  303,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  313,
   -1,  315,   -1,  317,  318,   -1,  320,   -1,  322,  323,
  324,  325,  326,  327,  328,  329,  330,   -1,  332,  333,
   -1,   -1,  336,   -1,   -1,   -1,   -1,  341,   -1,   -1,
   -1,  345,   -1,   -1,   -1,   -1,  350,  351,  352,  353,
  354,   -1,   -1,   -1,  358,   -1,  360,   -1,   -1,   -1,
   -1,  365,   -1,  367,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  263,  264,   -1,  266,   -1,   -1,  269,
  270,   -1,   -1,  407,  274,  409,   -1,  411,  278,  413,
   -1,  415,   -1,  417,  284,  419,   -1,  287,   -1,   -1,
   -1,   -1,   -1,   -1,  294,   -1,   -1,   -1,   -1,  299,
   -1,  301,  302,  303,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  315,   -1,  317,  318,   -1,
  320,   -1,   -1,  323,   -1,  325,   -1,  327,  328,  329,
  330,   -1,  332,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  342,   -1,  344,  345,  346,   -1,  348,  349,
  350,  351,  352,  353,  354,   -1,   -1,   -1,  358,  359,
  360,   -1,   -1,  363,  364,  365,   -1,  367,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  377,   -1,  379,
   -1,  381,   -1,  383,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  263,  264,   -1,
  266,   -1,   -1,  269,  270,   -1,   -1,  407,  274,  409,
   -1,  411,  278,  413,   -1,  415,   -1,  417,  284,  419,
   -1,  287,   -1,   -1,   -1,   -1,   -1,   -1,  294,   -1,
   -1,   -1,   -1,  299,   -1,  301,  302,  303,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  315,
   -1,  317,  318,   -1,  320,   -1,   -1,  323,   -1,  325,
   -1,  327,  328,  329,  330,   -1,  332,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  342,   -1,  344,  345,
  346,   -1,  348,  349,  350,  351,  352,  353,  354,   -1,
   -1,   -1,  358,  359,  360,   -1,   -1,  363,  364,  365,
   -1,  367,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  377,   -1,  379,   -1,  381,   -1,  383,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  263,  264,   -1,  266,   -1,   -1,  269,  270,   -1,
   -1,  407,  274,  409,   -1,  411,  278,  413,   -1,  415,
   -1,  417,  284,  419,   -1,  287,   -1,   -1,   -1,   -1,
   -1,   -1,  294,   -1,   -1,   -1,   -1,  299,   -1,  301,
  302,  303,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  315,   -1,  317,  318,   -1,  320,   -1,
   -1,  323,   -1,  325,   -1,  327,  328,  329,  330,   -1,
  332,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  342,   -1,  344,  345,  346,   -1,  348,  349,  350,  351,
  352,  353,  354,   -1,   -1,   -1,  358,  359,  360,   -1,
   -1,  363,  364,  365,   -1,  367,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  377,   -1,  379,   -1,  381,
   -1,  383,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  263,  264,   -1,  266,   -1,
   -1,  269,  270,   -1,   -1,  407,  274,  409,   -1,  411,
  278,  413,   -1,  415,   -1,  417,  284,  419,   -1,  287,
   -1,   -1,   -1,   -1,   -1,   -1,  294,   -1,   -1,   -1,
   -1,  299,   -1,  301,  302,  303,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  315,   -1,  317,
  318,   -1,  320,   -1,   -1,  323,   -1,  325,   -1,  327,
  328,  329,  330,   -1,  332,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  342,   -1,  344,  345,  346,   -1,
  348,  349,  350,  351,  352,  353,  354,   -1,   -1,   -1,
  358,  359,  360,   -1,   -1,  363,  364,  365,   -1,  367,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  381,   -1,  383,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  263,
  264,   -1,  266,   -1,   -1,  269,  270,   -1,   -1,  407,
  274,  409,   -1,  411,  278,  413,   -1,  415,   -1,  417,
  284,  419,   -1,  287,   -1,   -1,   -1,   -1,   -1,   -1,
  294,   -1,   -1,   -1,   -1,  299,   -1,  301,  302,  303,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  315,   -1,  317,  318,   -1,  320,   -1,   -1,  323,
   -1,  325,   -1,  327,  328,  329,  330,   -1,  332,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  342,   -1,
  344,  345,  346,   -1,  348,  349,  350,  351,  352,  353,
  354,   -1,   -1,   -1,  358,  359,  360,   -1,   -1,  363,
  364,  365,   -1,  367,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  381,   -1,  383,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  263,  264,   -1,  266,   -1,   -1,  269,
  270,   -1,   -1,  407,  274,  409,   -1,  411,  278,  413,
   -1,  415,   -1,  417,  284,  419,   -1,  287,   -1,   -1,
   -1,   -1,   -1,   -1,  294,   -1,   -1,   -1,   -1,  299,
   -1,  301,  302,  303,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  315,   -1,  317,  318,   -1,
  320,   -1,   -1,  323,   -1,  325,   -1,  327,  328,  329,
  330,   -1,  332,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  342,   -1,  344,  345,  346,   -1,  348,  349,
  350,  351,  352,  353,  354,   -1,   -1,   -1,   -1,  359,
  360,   -1,   -1,  363,  364,  365,   -1,  367,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  381,   -1,  383,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  263,  264,   -1,
  266,   -1,   -1,  269,  270,   -1,   -1,  407,  274,  409,
   -1,  411,  278,  413,   -1,  415,   -1,  417,  284,  419,
   -1,  287,   -1,   -1,   -1,   -1,   -1,   -1,  294,   -1,
   -1,   -1,   -1,  299,   -1,  301,  302,  303,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  315,
   -1,  317,  318,   -1,  320,   -1,   -1,  323,   -1,  325,
   -1,  327,  328,  329,  330,   -1,  332,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  342,   -1,  344,  345,
  346,   -1,  348,  349,  350,  351,  352,  353,  354,   -1,
   -1,   -1,  358,  359,  360,   -1,   -1,   -1,  364,  365,
   -1,  367,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  381,   -1,  383,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  263,  264,   -1,  266,   -1,   -1,  269,  270,   -1,
   -1,  407,  274,  409,   -1,  411,  278,  413,   -1,  415,
   -1,  417,  284,  419,   -1,  287,   -1,   -1,   -1,   -1,
   -1,   -1,  294,   -1,   -1,   -1,   -1,  299,   -1,  301,
  302,  303,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  315,   -1,  317,  318,   -1,  320,   -1,
   -1,  323,   -1,  325,   -1,  327,  328,  329,  330,   -1,
  332,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  342,   -1,  344,  345,  346,   -1,  348,  349,  350,  351,
  352,  353,  354,   -1,   -1,   -1,   -1,  359,  360,   -1,
   -1,  363,  364,  365,   -1,  367,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  381,
   -1,  383,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  263,  264,   -1,  266,   -1,
   -1,  269,  270,   -1,   -1,  407,  274,  409,   -1,  411,
  278,  413,   -1,  415,   -1,  417,  284,  419,   -1,  287,
   -1,   -1,   -1,   -1,   -1,   -1,  294,   -1,   -1,   -1,
   -1,  299,   -1,  301,  302,  303,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  315,   -1,  317,
  318,   -1,  320,   -1,   -1,  323,   -1,  325,   -1,  327,
  328,  329,  330,   -1,  332,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  342,   -1,  344,  345,  346,   -1,
  348,  349,  350,  351,  352,  353,  354,   -1,   -1,   -1,
  358,  359,  360,   -1,   -1,   -1,  364,  365,   -1,  367,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  381,   -1,  383,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  263,
  264,   -1,  266,   -1,   -1,  269,  270,   -1,   -1,  407,
  274,  409,   -1,  411,  278,  413,   -1,  415,   -1,  417,
  284,  419,   -1,  287,   -1,   -1,   -1,   -1,   -1,   -1,
  294,   -1,   -1,   -1,   -1,  299,   -1,  301,  302,  303,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  315,   -1,  317,  318,   -1,  320,   -1,   -1,  323,
   -1,  325,   -1,  327,  328,  329,  330,   -1,  332,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  342,   -1,
  344,  345,  346,   -1,  348,  349,  350,  351,  352,  353,
  354,   -1,   -1,   -1,  358,   -1,  360,   -1,   -1,   -1,
  364,  365,   -1,  367,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  381,   -1,  383,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  263,  264,   -1,  266,   -1,   -1,  269,
  270,   -1,   -1,  407,  274,  409,   -1,  411,  278,  413,
   -1,  415,   -1,  417,  284,  419,   -1,  287,   -1,   -1,
   -1,   -1,   -1,   -1,  294,   -1,   -1,   -1,   -1,  299,
   -1,  301,  302,  303,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  315,   -1,  317,  318,   -1,
  320,   -1,   -1,  323,   -1,  325,   -1,  327,  328,  329,
  330,   -1,  332,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  342,   -1,  344,  345,  346,   -1,  348,  349,
  350,  351,  352,  353,  354,   -1,   -1,   -1,  358,   -1,
  360,   -1,   -1,   -1,  364,  365,   -1,  367,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  381,   -1,  383,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  263,  264,   -1,
  266,   -1,   -1,  269,  270,   -1,   -1,  407,  274,  409,
   -1,  411,  278,  413,   -1,  415,   -1,  417,  284,  419,
   -1,  287,   -1,   -1,   -1,   -1,   -1,   -1,  294,   -1,
   -1,   -1,   -1,  299,   -1,  301,  302,  303,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  315,
   -1,  317,  318,   -1,  320,   -1,   -1,  323,   -1,  325,
   -1,  327,  328,  329,  330,   -1,  332,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  342,   -1,  344,  345,
  346,   -1,  348,  349,  350,  351,  352,  353,  354,   -1,
   -1,   -1,  358,   -1,  360,   -1,   -1,   -1,  364,  365,
   -1,  367,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  383,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  263,  264,   -1,  266,   -1,   -1,  269,  270,   -1,
   -1,  407,  274,  409,   -1,  411,  278,  413,   -1,  415,
   -1,  417,  284,  419,   -1,  287,   -1,   -1,   -1,   -1,
   -1,   -1,  294,   -1,   -1,   -1,   -1,  299,   -1,  301,
  302,  303,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  315,   -1,  317,  318,   -1,  320,   -1,
   -1,  323,   -1,  325,   -1,  327,  328,  329,  330,   -1,
  332,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  342,   -1,  344,  345,  346,   -1,  348,  349,  350,  351,
  352,  353,  354,   -1,   -1,   -1,  358,   -1,  360,   -1,
   -1,   -1,  364,  365,   -1,  367,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  383,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  263,  264,   -1,  266,   -1,
   -1,  269,  270,   -1,   -1,  407,  274,  409,   -1,  411,
  278,  413,   -1,  415,   -1,  417,  284,  419,   -1,  287,
   -1,   -1,   -1,   -1,   -1,   -1,  294,   -1,   -1,   -1,
   -1,  299,   -1,  301,  302,  303,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  315,   -1,  317,
  318,   -1,  320,   -1,   -1,  323,   -1,  325,   -1,  327,
  328,  329,  330,   -1,  332,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  342,   -1,  344,  345,  346,   -1,
  348,  349,  350,  351,  352,  353,  354,   -1,   -1,   -1,
  358,   -1,  360,   -1,   -1,   -1,   -1,  365,   -1,  367,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  263,
  264,   -1,  266,   -1,   -1,  269,  270,   -1,   -1,  407,
  274,  409,   -1,  411,  278,  413,   -1,  415,   -1,  417,
  284,  419,   -1,  287,   -1,   -1,   -1,   -1,   -1,   -1,
  294,   -1,   -1,   -1,   -1,  299,   -1,  301,  302,  303,
   -1,  305,   -1,   -1,   -1,   -1,   -1,   -1,  312,   -1,
   -1,  315,   -1,  317,  318,   -1,  320,   -1,   -1,  323,
   -1,  325,   -1,  327,  328,  329,  330,   -1,  332,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  345,   -1,   -1,   -1,   -1,   -1,  351,  352,  353,
  354,   -1,   -1,   -1,  358,   -1,  360,   -1,   -1,   -1,
   -1,  365,   -1,  367,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  263,  264,   -1,  266,   -1,   -1,  269,
  270,   -1,   -1,  407,  274,  409,   -1,  411,  278,  413,
   -1,  415,   -1,  417,  284,  419,   -1,  287,   -1,   -1,
   -1,   -1,   -1,   -1,  294,   -1,   -1,   -1,   -1,  299,
   -1,  301,  302,  303,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  315,   -1,  317,  318,   -1,
  320,   -1,   -1,  323,   -1,  325,   -1,  327,  328,  329,
  330,   -1,  332,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  341,  342,   -1,   -1,  345,   -1,   -1,   -1,   -1,
   -1,  351,  352,  353,  354,   -1,   -1,   -1,  358,   -1,
  360,   -1,   -1,   -1,   -1,  365,   -1,  367,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  263,  264,   -1,
  266,   -1,   -1,  269,  270,   -1,   -1,  407,  274,  409,
   -1,  411,  278,  413,   -1,  415,   -1,  417,  284,  419,
   -1,  287,   -1,   -1,   -1,   -1,   -1,   -1,  294,   -1,
   -1,   -1,   -1,  299,   -1,  301,  302,  303,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  315,
   -1,  317,  318,   -1,  320,   -1,   -1,  323,   -1,  325,
   -1,  327,  328,  329,  330,   -1,  332,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  341,  342,   -1,   -1,  345,
   -1,   -1,   -1,   -1,   -1,  351,  352,  353,  354,   -1,
   -1,   -1,  358,   -1,  360,   -1,   -1,   -1,   -1,  365,
   -1,  367,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  263,  264,   -1,  266,   -1,   -1,  269,  270,   -1,
   -1,  407,  274,  409,   -1,  411,  278,  413,   -1,  415,
   -1,  417,  284,  419,   -1,  287,   -1,   -1,   -1,   -1,
   -1,   -1,  294,   -1,   -1,   -1,   -1,  299,   -1,  301,
  302,  303,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  315,   -1,  317,  318,   -1,  320,   -1,
   -1,  323,   -1,  325,   -1,  327,  328,  329,  330,   -1,
  332,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  345,   -1,   -1,  348,   -1,  350,  351,
  352,  353,  354,   -1,   -1,   -1,  358,   -1,  360,   -1,
   -1,   -1,   -1,  365,   -1,  367,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  263,  264,   -1,  266,   -1,
   -1,  269,  270,   -1,   -1,  407,  274,  409,   -1,  411,
  278,  413,   -1,  415,   -1,  417,  284,  419,   -1,  287,
   -1,   -1,   -1,   -1,   -1,   -1,  294,   -1,   -1,   -1,
   -1,  299,   -1,  301,  302,  303,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  315,   -1,  317,
  318,   -1,  320,   -1,   -1,  323,   -1,  325,   -1,  327,
  328,  329,  330,   -1,  332,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  345,   -1,   -1,
  348,   -1,   -1,  351,  352,  353,  354,   -1,   -1,   -1,
  358,   -1,  360,   -1,   -1,   -1,   -1,  365,   -1,  367,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  263,
  264,   -1,  266,   -1,   -1,  269,  270,   -1,   -1,  407,
  274,  409,   -1,  411,  278,  413,   -1,  415,   -1,  417,
  284,  419,   -1,  287,   -1,   -1,   -1,   -1,   -1,   -1,
  294,   -1,   -1,   -1,   -1,  299,   -1,  301,  302,  303,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  315,   -1,  317,  318,   -1,  320,   -1,   -1,  323,
   -1,  325,   -1,  327,  328,  329,  330,   -1,  332,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  341,   -1,   -1,
   -1,  345,   -1,   -1,   -1,   -1,   -1,  351,  352,  353,
  354,   -1,   -1,   -1,  358,   -1,  360,   -1,   -1,   -1,
   -1,  365,   -1,  367,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  263,  264,   -1,  266,   -1,   -1,  269,
  270,   -1,   -1,  407,  274,  409,   -1,  411,  278,  413,
   -1,  415,   -1,  417,  284,  419,   -1,  287,   -1,   -1,
   -1,   -1,   -1,   -1,  294,   -1,   -1,   -1,   -1,  299,
   -1,  301,  302,  303,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  315,   -1,  317,  318,   -1,
  320,   -1,   -1,  323,   -1,  325,   -1,  327,  328,  329,
  330,   -1,  332,   -1,   -1,  335,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  345,   -1,   -1,   -1,   -1,
   -1,  351,  352,  353,  354,   -1,   -1,   -1,  358,   -1,
  360,   -1,   -1,   -1,   -1,  365,   -1,  367,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  263,  264,   -1,
  266,   -1,   -1,  269,  270,   -1,   -1,  407,  274,  409,
   -1,  411,  278,  413,   -1,  415,   -1,  417,  284,  419,
   -1,  287,   -1,   -1,   -1,   -1,   -1,   -1,  294,   -1,
   -1,   -1,   -1,  299,   -1,  301,  302,  303,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  315,
   -1,  317,  318,   -1,  320,   -1,   -1,  323,   -1,  325,
   -1,  327,  328,  329,  330,   -1,  332,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  345,
   -1,   -1,   -1,   -1,  350,  351,  352,  353,  354,   -1,
   -1,   -1,  358,   -1,  360,   -1,   -1,   -1,   -1,  365,
   -1,  367,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  263,  264,   -1,  266,   -1,   -1,  269,  270,   -1,
   -1,  407,  274,  409,   -1,  411,  278,  413,   -1,  415,
   -1,  417,  284,  419,   -1,  287,   -1,   -1,   -1,   -1,
   -1,   -1,  294,   -1,   -1,   -1,   -1,  299,   -1,  301,
  302,  303,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  315,   -1,  317,  318,   -1,  320,   -1,
   -1,  323,   -1,  325,   -1,  327,  328,  329,  330,   -1,
  332,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  345,   -1,   -1,   -1,   -1,   -1,  351,
  352,  353,  354,   -1,   -1,   -1,  358,   -1,  360,   -1,
   -1,   -1,   -1,  365,   -1,  367,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  263,  264,   -1,  266,   -1,
   -1,  269,  270,   -1,   -1,  407,  274,  409,   -1,  411,
  278,  413,   -1,  415,   -1,  417,  284,  419,   -1,  287,
   -1,   -1,   -1,   -1,   -1,   -1,  294,   -1,   -1,   -1,
   -1,  299,   -1,  301,  302,  303,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  315,   -1,  317,
  318,   -1,  320,   -1,   -1,  323,   -1,  325,   -1,  327,
  328,  329,  330,   -1,  332,   -1,  264,  335,  266,   -1,
   -1,  269,   -1,  271,  272,   -1,  274,  345,  276,   -1,
  278,   -1,  280,  281,  282,   -1,   -1,   -1,   -1,  287,
   -1,   -1,   -1,   -1,  292,   -1,  294,  295,   -1,   -1,
   -1,  299,   -1,   -1,   -1,  303,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  315,   -1,  317,
   -1,   -1,  320,  321,   -1,   -1,   -1,   -1,   -1,   -1,
  328,  329,   -1,   -1,  332,   -1,   -1,  335,   -1,  407,
   -1,  409,   -1,  411,   -1,  413,   -1,  415,  264,  417,
  266,  419,   -1,  269,   -1,  271,  272,   -1,  274,   -1,
  276,   -1,  278,   -1,  280,  281,  282,   -1,   -1,   -1,
   -1,  287,   -1,   -1,   -1,   -1,  292,   -1,  294,  295,
   -1,   -1,   -1,  299,   -1,   -1,   -1,  303,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  315,
   -1,  317,   -1,   -1,  320,  321,   -1,   -1,   -1,   -1,
   -1,   -1,  328,  329,   -1,  264,  332,  266,   -1,  335,
  269,  419,  271,  272,   -1,  274,   -1,  276,   -1,  278,
   -1,  280,  281,  282,   -1,   -1,   -1,   -1,  287,   -1,
   -1,   -1,   -1,  292,   -1,  294,  295,   -1,   -1,   -1,
  299,   -1,   -1,   -1,  303,   -1,   -1,   -1,   -1,  264,
   -1,  266,   -1,   -1,  269,   -1,  315,   -1,  317,  274,
   -1,  320,  321,  278,   -1,   -1,  281,   -1,   -1,  328,
  329,   -1,  287,  332,   -1,   -1,  335,   -1,   -1,  294,
   -1,   -1,   -1,   -1,  299,   -1,  301,   -1,  303,   -1,
   -1,   -1,   -1,  419,   -1,   -1,   -1,   -1,   -1,   -1,
  315,  264,  317,  266,   -1,  320,  269,   -1,   -1,   -1,
   -1,  274,   -1,  328,  329,  278,   -1,  332,  281,   -1,
  335,   -1,   -1,   -1,  287,   -1,   -1,  342,   -1,   -1,
   -1,  294,  264,   -1,  266,   -1,  299,  269,  301,   -1,
  303,   -1,  274,   -1,   -1,   -1,  278,   -1,   -1,   -1,
   -1,   -1,  315,   -1,  317,  287,   -1,  320,   -1,   -1,
  419,   -1,  294,   -1,   -1,  328,  329,  299,   -1,  332,
   -1,  303,  335,  305,   -1,  307,  264,   -1,  266,  342,
  312,  269,   -1,  315,   -1,  317,  274,   -1,  320,   -1,
  278,   -1,   -1,   -1,   -1,   -1,  328,  329,   -1,  287,
  332,   -1,   -1,   -1,  419,   -1,  294,   -1,  264,   -1,
  266,  299,   -1,  269,  346,  303,   -1,  305,  274,  307,
   -1,   -1,  278,   -1,  312,  281,   -1,  315,   -1,  317,
   -1,  287,  320,   -1,   -1,   -1,   -1,   -1,  294,   -1,
  328,  329,   -1,  299,  332,   -1,   -1,  303,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  264,  419,  266,   -1,  315,
  269,  317,   -1,   -1,  320,  274,   -1,   -1,   -1,  278,
   -1,   -1,  328,  329,   -1,   -1,  332,   -1,  287,  335,
   -1,   -1,   -1,   -1,   -1,  294,   -1,  419,   -1,   -1,
  299,  264,   -1,  266,  303,   -1,  269,   -1,   -1,   -1,
   -1,  274,   -1,   -1,   -1,  278,  315,   -1,  317,   -1,
   -1,  320,   -1,   -1,  287,   -1,   -1,   -1,   -1,  328,
  329,  294,   -1,  332,   -1,   -1,  299,   -1,   -1,   -1,
  303,  419,  341,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  349,   -1,  315,  264,  317,  266,   -1,  320,  269,   -1,
   -1,   -1,   -1,  274,   -1,  328,  329,  278,   -1,  332,
  281,  261,   -1,  419,   -1,  264,  287,  266,  341,   -1,
  269,   -1,   -1,  294,   -1,  274,  349,   -1,  299,  278,
   -1,   -1,  303,   -1,   -1,   -1,   -1,   -1,  287,   -1,
   -1,   -1,   -1,   -1,  315,  294,  317,  297,   -1,  320,
  299,   -1,   -1,   -1,  303,   -1,   -1,  328,  329,   -1,
  419,  332,   -1,   -1,  335,   -1,  315,   -1,  317,   -1,
   -1,  320,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  328,
  329,   -1,   -1,  332,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  343,   -1,  345,  419,   -1,   -1,   -1,
   -1,   -1,  352,  353,   -1,  355,  356,  357,  358,  359,
  360,  361,  362,  363,  364,  365,   -1,  367,   -1,  369,
   -1,  371,   -1,  373,   -1,  375,   -1,  377,   -1,  379,
   -1,  381,   -1,  383,   -1,  385,   -1,  387,   -1,  389,
   -1,  391,   -1,  393,   -1,  395,   -1,  397,  419,  399,
   -1,  401,  257,  403,   -1,  260,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  271,   -1,   -1,   -1,
  419,  276,   -1,   -1,   -1,  280,   -1,   -1,  283,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  295,  296,   -1,   -1,   -1,  300,  301,   -1,   -1,   -1,
   -1,  306,   -1,  308,  309,  310,  311,  257,   -1,   -1,
  260,  316,   -1,   -1,  319,   -1,  321,   -1,   -1,   -1,
   -1,  271,   -1,   -1,   -1,   -1,  276,   -1,   -1,  334,
  280,   -1,   -1,  283,   -1,   -1,   -1,  342,  343,   -1,
   -1,   -1,   -1,   -1,   -1,  295,  296,   -1,   -1,   -1,
  300,  301,   -1,   -1,   -1,   -1,  306,   -1,  308,  309,
  310,  311,  257,   -1,   -1,  260,  316,   -1,   -1,  319,
   -1,  321,   -1,   -1,   -1,   -1,  271,   -1,   -1,   -1,
   -1,  276,   -1,   -1,  334,  280,   -1,   -1,  283,   -1,
   -1,   -1,  342,  343,   -1,   -1,   -1,   -1,   -1,   -1,
  295,  296,   -1,   -1,   -1,  300,  301,   -1,   -1,   -1,
   -1,  306,   -1,  308,  309,  310,  311,  257,   -1,   -1,
  260,  316,   -1,   -1,  319,   -1,  321,   -1,   -1,   -1,
   -1,  271,   -1,   -1,   -1,   -1,  276,   -1,   -1,  334,
  280,   -1,  260,  283,   -1,   -1,   -1,   -1,  343,   -1,
   -1,   -1,   -1,  271,   -1,  295,  296,   -1,  276,   -1,
   -1,  301,  280,   -1,   -1,  283,  306,   -1,  308,  309,
  310,  311,   -1,   -1,   -1,   -1,  316,  295,  296,  319,
   -1,  321,  300,  301,   -1,   -1,   -1,   -1,  306,   -1,
  308,  309,  310,  311,  334,  260,   -1,   -1,  316,   -1,
   -1,  319,  342,  321,   -1,   -1,  271,   -1,   -1,  257,
   -1,  276,  260,   -1,   -1,  280,  334,   -1,  283,   -1,
   -1,   -1,   -1,  271,  342,  343,   -1,   -1,  276,   -1,
  295,  296,  280,   -1,   -1,  283,  301,   -1,   -1,   -1,
   -1,  306,   -1,  308,  309,  310,  311,  295,  296,   -1,
   -1,  316,  300,  301,  319,   -1,  321,   -1,  306,   -1,
  308,  309,  310,  311,  257,   -1,   -1,  260,  316,  334,
   -1,  319,   -1,  321,   -1,   -1,   -1,  342,  271,   -1,
   -1,   -1,   -1,  276,   -1,   -1,  334,  280,   -1,   -1,
  283,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  295,  296,   -1,   -1,   -1,  343,  301,  345,
  346,  347,  348,  306,  350,  308,  309,  310,  311,  355,
   -1,   -1,   -1,  316,   -1,   -1,  319,   -1,  321,  365,
  343,  367,  345,  346,  347,  348,   -1,  350,   -1,   -1,
   -1,  334,  355,   -1,   -1,   -1,   -1,   -1,   -1,  385,
   -1,  387,  365,  389,  367,  391,   -1,  393,   -1,  395,
   -1,  397,   -1,  399,   -1,  401,   -1,  403,   -1,   -1,
   -1,   -1,  385,   -1,  387,   -1,  389,   -1,  391,   -1,
  393,   -1,  395,  419,  397,   -1,  399,   -1,  401,  343,
  403,  345,  346,  347,  348,   -1,  350,   -1,   -1,   -1,
  343,  355,  345,  346,  347,  348,  419,  350,   -1,   -1,
   -1,  365,  355,  367,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  365,   -1,  367,   -1,   -1,   -1,   -1,   -1,
   -1,  385,   -1,  387,   -1,  389,   -1,  391,   -1,  393,
   -1,  395,  385,  397,  387,  399,  389,  401,  391,  403,
  393,  343,  395,  345,  397,  347,  399,   -1,  401,   -1,
  403,   -1,   -1,  355,   -1,  419,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  365,   -1,  367,  419,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  385,   -1,  387,   -1,  389,   -1,  391,
   -1,  393,   -1,  395,   -1,  397,   -1,  399,   -1,  401,
   -1,  403,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  419,
  };

#line 2535 "cs-parser.jay"


// <summary>
//   A class used to pass around variable declarations and constants
// </summary>
public class VariableDeclaration {
	public string identifier;
	public object expression_or_array_initializer;

	public VariableDeclaration (string id, object eoai){
		this.identifier = id;
		this.expression_or_array_initializer = eoai;
	}
}

// <summary>
//   Given the @class_name name, it creates a fully qualified name
//   based on the containing declaration space
// </summary>
string 
MakeName (string class_name)
{
	string ns = current_namespace.Name;
	string container_name = current_container.Name;

	if (container_name == ""){
		if (ns != "")
			return ns + "." + class_name;
		else
			return class_name;
	} else
		return container_name + "." + class_name;
}

// <summary>
//   Used to report back to the user the result of a declaration
//   in the current declaration space
// </summary>
void 
CheckDef (DeclSpace.AdditionResult result, string name)
{
	if (result == DeclSpace.AdditionResult.Success)
		return;

	switch (result){
	case DeclSpace.AdditionResult.NameExists:
		error (102, "The namespace `" + current_container.Name + 
			    "' already contains a definition for `"+
			    name + "'");
		break;

/*
	NEED TO HANDLE THIS IN SEMANTIC ANALYSIS:

	case DeclSpace.AdditionResult.MethodDuplicated:
		error (111, "Class `"+current_container.Name+
			    "' already defines a member called '" + 
			    name + "' with the same parameter types");
		break;
*/
	case DeclSpace.AdditionResult.EnclosingClash:
		error (542, "Member names cannot be the same as their enclosing type");
		break;

	case DeclSpace.AdditionResult.NotAConstructor:
		error (1520, "Class, struct, or interface method must have a return type");
		break;
	}
}

void 
CheckDef (bool result, string name)
{
	if (result)
		return;
	CheckDef (DeclSpace.AdditionResult.NameExists, name);
}

Block declare_local_variables (TypeRef typeref, ArrayList variable_declarators)
{
	Block implicit_block;
	ArrayList inits = null;

	//
	// We use the `Used' property to check whether statements
	// have been added to the current block.  If so, we need
	// to create another block to contain the new declaration
	// otherwise, as an optimization, we use the same block to
	// add the declaration.
	//
	// FIXME: A further optimization is to check if the statements
	// that were added were added as part of the initialization
	// below.  In which case, no other statements have been executed
	// and we might be able to reduce the number of blocks for
	// situations like this:
	//
	// int j = 1;  int k = j + 1;
	//
	if (current_block.Used)
		implicit_block = new Block (current_block, true);
	else
		implicit_block = new Block (current_block, true);

	foreach (VariableDeclaration decl in variable_declarators){
		if (implicit_block.AddVariable (typeref, decl.identifier)){
			if (decl.expression_or_array_initializer != null){
				if (inits == null)
					inits = new ArrayList ();
				inits.Add (decl);
			}
		} else {
			error (128, "A local variable `" + decl.identifier +
				    "' is already defined in this scope");
		}
	}

	if (inits == null)
		return implicit_block;

	foreach (VariableDeclaration decl in inits){
		if (decl.expression_or_array_initializer is Expression){
			Expression expr = (Expression) decl.expression_or_array_initializer;
			Assign assign;
			
			assign = new Assign (new LocalVariableReference (implicit_block, decl.identifier), expr);
			implicit_block.AddStatement (new StatementExpression (assign));
		} else {
		}
	}
			
	return implicit_block;
}

void CheckConstant (Expression expr)
{
	// finishme
}

void CheckBoolean (Expression expr)
{
	// finishme
}

static public void error (int code, string desc)
{
	Console.WriteLine ("Error CS"+code+": "+desc);
	global_errors++;
}

void output (string s)
{
	Console.WriteLine (s);
}

void note (string s)
{
	// Used to put annotations
}

TypeRef type (string type_name)
{
	return type_references.GetTypeRef (current_container, type_name);
}

Tokenizer lexer;

public CSharpParser(CIR.Tree tree, string name, System.IO.Stream input) 
	: base (tree, name, input)
{
	current_namespace = new Namespace (null, "");
	current_container = tree.Types;
	current_container.Namespace = current_namespace;

	lexer = new Tokenizer (input, name);
	type_references = new TypeRefManager ();
}

public override int parse ()
{
	StringBuilder value = new StringBuilder ();

	global_errors = 0;
	try {
		if (yacc_verbose_flag)
			yyparse (lexer, new yydebug.yyDebugSimple ());
		else
			yyparse (lexer);
	} catch (Exception e){
		Console.WriteLine ("Fatal error: "+name);
		Console.WriteLine (e);
		Console.WriteLine (lexer.location);
		global_errors++;
	}
	
	return global_errors;
}

bool yacc_verbose_flag = false;

public bool yacc_verbose {
	set {
		yacc_verbose_flag = value;
	}

	get {
		return yacc_verbose_flag;
	}
}

/* end end end */
}
}

#line 5289 "-"
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
  public const int ABSTRACT = 260;
  public const int AS = 261;
  public const int ADD = 262;
  public const int BASE = 263;
  public const int BOOL = 264;
  public const int BREAK = 265;
  public const int BYTE = 266;
  public const int CASE = 267;
  public const int CATCH = 268;
  public const int CHAR = 269;
  public const int CHECKED = 270;
  public const int CLASS = 271;
  public const int CONST = 272;
  public const int CONTINUE = 273;
  public const int DECIMAL = 274;
  public const int DEFAULT = 275;
  public const int DELEGATE = 276;
  public const int DO = 277;
  public const int DOUBLE = 278;
  public const int ELSE = 279;
  public const int ENUM = 280;
  public const int EVENT = 281;
  public const int EXPLICIT = 282;
  public const int EXTERN = 283;
  public const int FALSE = 284;
  public const int FINALLY = 285;
  public const int FIXED = 286;
  public const int FLOAT = 287;
  public const int FOR = 288;
  public const int FOREACH = 289;
  public const int GOTO = 290;
  public const int IF = 291;
  public const int IMPLICIT = 292;
  public const int IN = 293;
  public const int INT = 294;
  public const int INTERFACE = 295;
  public const int INTERNAL = 296;
  public const int IS = 297;
  public const int LOCK = 298;
  public const int LONG = 299;
  public const int NAMESPACE = 300;
  public const int NEW = 301;
  public const int NULL = 302;
  public const int OBJECT = 303;
  public const int OPERATOR = 304;
  public const int OUT = 305;
  public const int OVERRIDE = 306;
  public const int PARAMS = 307;
  public const int PRIVATE = 308;
  public const int PROTECTED = 309;
  public const int PUBLIC = 310;
  public const int READONLY = 311;
  public const int REF = 312;
  public const int RETURN = 313;
  public const int REMOVE = 314;
  public const int SBYTE = 315;
  public const int SEALED = 316;
  public const int SHORT = 317;
  public const int SIZEOF = 318;
  public const int STATIC = 319;
  public const int STRING = 320;
  public const int STRUCT = 321;
  public const int SWITCH = 322;
  public const int THIS = 323;
  public const int THROW = 324;
  public const int TRUE = 325;
  public const int TRY = 326;
  public const int TYPEOF = 327;
  public const int UINT = 328;
  public const int ULONG = 329;
  public const int UNCHECKED = 330;
  public const int UNSAFE = 331;
  public const int USHORT = 332;
  public const int USING = 333;
  public const int VIRTUAL = 334;
  public const int VOID = 335;
  public const int WHILE = 336;
  public const int GET = 337;
  public const int get = 338;
  public const int SET = 339;
  public const int set = 340;
  public const int OPEN_BRACE = 341;
  public const int CLOSE_BRACE = 342;
  public const int OPEN_BRACKET = 343;
  public const int CLOSE_BRACKET = 344;
  public const int OPEN_PARENS = 345;
  public const int CLOSE_PARENS = 346;
  public const int DOT = 347;
  public const int COMMA = 348;
  public const int COLON = 349;
  public const int SEMICOLON = 350;
  public const int TILDE = 351;
  public const int PLUS = 352;
  public const int MINUS = 353;
  public const int BANG = 354;
  public const int ASSIGN = 355;
  public const int OP_LT = 356;
  public const int OP_GT = 357;
  public const int BITWISE_AND = 358;
  public const int BITWISE_OR = 359;
  public const int STAR = 360;
  public const int PERCENT = 361;
  public const int DIV = 362;
  public const int CARRET = 363;
  public const int INTERR = 364;
  public const int OP_INC = 365;
  public const int OP_DEC = 367;
  public const int OP_SHIFT_LEFT = 369;
  public const int OP_SHIFT_RIGHT = 371;
  public const int OP_LE = 373;
  public const int OP_GE = 375;
  public const int OP_EQ = 377;
  public const int OP_NE = 379;
  public const int OP_AND = 381;
  public const int OP_OR = 383;
  public const int OP_MULT_ASSIGN = 385;
  public const int OP_DIV_ASSIGN = 387;
  public const int OP_MOD_ASSIGN = 389;
  public const int OP_ADD_ASSIGN = 391;
  public const int OP_SUB_ASSIGN = 393;
  public const int OP_SHIFT_LEFT_ASSIGN = 395;
  public const int OP_SHIFT_RIGHT_ASSIGN = 397;
  public const int OP_AND_ASSIGN = 399;
  public const int OP_XOR_ASSIGN = 401;
  public const int OP_OR_ASSIGN = 403;
  public const int OP_PTR = 405;
  public const int LITERAL_INTEGER = 407;
  public const int LITERAL_FLOAT = 409;
  public const int LITERAL_DOUBLE = 411;
  public const int LITERAL_DECIMAL = 413;
  public const int LITERAL_CHARACTER = 415;
  public const int LITERAL_STRING = 417;
  public const int IDENTIFIER = 419;
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
