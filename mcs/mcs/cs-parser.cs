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

		// <summary>
		//   Used to record all types defined
		// </summary>
		CIR.Tree tree;
#line 86 "-"

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
    "opt_attributes : attribute_section opt_attributes",
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
    "rank_specifiers : rank_specifier rank_specifiers",
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
    "unary_expression : OP_INC unary_expression",
    "unary_expression : OP_DEC unary_expression",
    "unary_expression : cast_expression",
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
    "\"string literal\"","IDENTIFIER","pre_increment_expression",
    "pre_decrement_expression",
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
#line 245 "cs-parser.jay"
  {
		/* At some point check that using only comes *before* any namespaces*/
	  }
  break;
case 6:
#line 262 "cs-parser.jay"
  {
	  }
  break;
case 7:
#line 268 "cs-parser.jay"
  {
		current_namespace.Using ((string) yyVals[-1+yyTop]);
          }
  break;
case 10:
#line 279 "cs-parser.jay"
  {
		current_namespace = new Namespace (current_namespace, (string) yyVals[0+yyTop]); 
	  }
  break;
case 11:
#line 283 "cs-parser.jay"
  { 
		current_namespace = current_namespace.Parent;
	  }
  break;
case 17:
#line 300 "cs-parser.jay"
  { 
	    yyVal = ((yyVals[-2+yyTop]).ToString ()) + "." + (yyVals[0+yyTop].ToString ()); }
  break;
case 19:
#line 313 "cs-parser.jay"
  {
	  }
  break;
case 26:
#line 334 "cs-parser.jay"
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
case 39:
#line 392 "cs-parser.jay"
  { 
	     /* if (Collection.Contains ($$))... FIXME*/
	     note  ("Allows: assembly, field, method, module, param, property, type"); 
	}
  break;
case 46:
#line 411 "cs-parser.jay"
  { /* reserved attribute name or identifier: 17.4 */ }
  break;
case 70:
#line 467 "cs-parser.jay"
  { 
		Struct new_struct;
		string full_struct_name = MakeName ((string) yyVals[0+yyTop]);

		new_struct = new Struct (current_container, full_struct_name, (int) yyVals[-2+yyTop]);
		current_container = new_struct;
		current_container.Namespace = current_namespace;
		tree.RecordType (full_class_name, new_struct);
	  }
  break;
case 71:
#line 479 "cs-parser.jay"
  {
		Struct new_struct = (Struct) current_container;

		current_container = current_container.Parent;
		CheckDef (current_container.AddStruct (new_struct), new_struct.Name);
		yyVal = new_struct;
	  }
  break;
case 91:
#line 535 "cs-parser.jay"
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
case 92:
#line 550 "cs-parser.jay"
  {
		ArrayList constants = new ArrayList ();
		constants.Add (yyVals[0+yyTop]);
		yyVal = constants;
	  }
  break;
case 93:
#line 556 "cs-parser.jay"
  {
		ArrayList constants = (ArrayList) yyVals[-2+yyTop];

		constants.Add (yyVals[0+yyTop]);
	  }
  break;
case 94:
#line 564 "cs-parser.jay"
  {
		yyVal = new DictionaryEntry (yyVals[-2+yyTop], yyVals[0+yyTop]);
	  }
  break;
case 95:
#line 575 "cs-parser.jay"
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
case 96:
#line 591 "cs-parser.jay"
  {
		ArrayList decl = new ArrayList ();
		yyVal = decl;
		decl.Add (yyVals[0+yyTop]);
	  }
  break;
case 97:
#line 597 "cs-parser.jay"
  {
		ArrayList decls = (ArrayList) yyVals[-2+yyTop];
		decls.Add (yyVals[0+yyTop]);
		yyVal = yyVals[-2+yyTop];
	  }
  break;
case 98:
#line 606 "cs-parser.jay"
  {
		yyVal = new VariableDeclaration ((string) yyVals[-2+yyTop], yyVals[0+yyTop]);
	  }
  break;
case 99:
#line 610 "cs-parser.jay"
  {
		yyVal = new VariableDeclaration ((string) yyVals[0+yyTop], null);
	  }
  break;
case 102:
#line 623 "cs-parser.jay"
  {
		Method method = (Method) yyVals[-1+yyTop];

		method.Block = (Block) yyVals[0+yyTop];
		CheckDef (current_container.AddMethod (method), method.Name);

		current_local_parameters = null;
	  }
  break;
case 103:
#line 639 "cs-parser.jay"
  {
		Method method = new Method ((TypeRef) yyVals[-4+yyTop], (int) yyVals[-5+yyTop], (string) yyVals[-3+yyTop], (Parameters) yyVals[-1+yyTop]);

		current_local_parameters = (Parameters) yyVals[-1+yyTop];

		yyVal = method;
	  }
  break;
case 104:
#line 651 "cs-parser.jay"
  {
		Method method = new Method (type ("void"), (int) yyVals[-5+yyTop], (string) yyVals[-3+yyTop], (Parameters) yyVals[-1+yyTop]);

		current_local_parameters = (Parameters) yyVals[-1+yyTop];
		yyVal = method;
	  }
  break;
case 106:
#line 661 "cs-parser.jay"
  { yyVal = null; }
  break;
case 107:
#line 665 "cs-parser.jay"
  { yyVal = new Parameters (null, null); }
  break;
case 109:
#line 671 "cs-parser.jay"
  { 
	  	yyVal = new Parameters ((ParameterCollection) yyVals[0+yyTop], null); 
	  }
  break;
case 110:
#line 675 "cs-parser.jay"
  {
		yyVal = new Parameters ((ParameterCollection) yyVals[-2+yyTop], (Parameter) yyVals[0+yyTop]); 
	  }
  break;
case 111:
#line 679 "cs-parser.jay"
  {
		yyVal = new Parameters (null, (Parameter) yyVals[0+yyTop]);
	  }
  break;
case 112:
#line 686 "cs-parser.jay"
  {
		ParameterCollection pars = new ParameterCollection ();
		pars.Add ((Parameter) yyVals[0+yyTop]);
		yyVal = pars;
	  }
  break;
case 113:
#line 692 "cs-parser.jay"
  {
		ParameterCollection pars = (ParameterCollection) yyVals[-2+yyTop];
		pars.Add ((Parameter) yyVals[0+yyTop]);
		yyVal = yyVals[-2+yyTop];
	  }
  break;
case 114:
#line 704 "cs-parser.jay"
  {
		yyVal = new Parameter ((TypeRef) yyVals[-1+yyTop], (string) yyVals[0+yyTop], (Parameter.Modifier) yyVals[-2+yyTop]);
	  }
  break;
case 115:
#line 710 "cs-parser.jay"
  { yyVal = Parameter.Modifier.NONE; }
  break;
case 117:
#line 715 "cs-parser.jay"
  { yyVal = Parameter.Modifier.REF; }
  break;
case 118:
#line 716 "cs-parser.jay"
  { yyVal = Parameter.Modifier.OUT; }
  break;
case 119:
#line 721 "cs-parser.jay"
  { 
		yyVal = new Parameter ((TypeRef) yyVals[-1+yyTop], (string) yyVals[0+yyTop], Parameter.Modifier.PARAMS);
		note ("type must be a single-dimension array type"); 
	  }
  break;
case 120:
#line 728 "cs-parser.jay"
  { yyVal = yyVals[0+yyTop].ToString (); }
  break;
case 121:
#line 729 "cs-parser.jay"
  { yyVal = yyVals[-2+yyTop].ToString () + "." + yyVals[0+yyTop].ToString (); }
  break;
case 122:
#line 737 "cs-parser.jay"
  {
		Parameter implicit_value_parameter;
		implicit_value_parameter = new Parameter ((TypeRef) yyVals[-2+yyTop], "value", Parameter.Modifier.NONE);

		lexer.properties = true;
		
		implicit_value_parameters = new ParameterCollection ();
		implicit_value_parameters.Add (implicit_value_parameter);
	  }
  break;
case 123:
#line 747 "cs-parser.jay"
  {
		lexer.properties = false;
	  }
  break;
case 124:
#line 751 "cs-parser.jay"
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
case 125:
#line 771 "cs-parser.jay"
  { 
		yyVal = new DictionaryEntry (yyVals[-1+yyTop], yyVals[0+yyTop]);
	  }
  break;
case 126:
#line 775 "cs-parser.jay"
  {
		yyVal = new DictionaryEntry (yyVals[0+yyTop], yyVals[-1+yyTop]);
	  }
  break;
case 127:
#line 781 "cs-parser.jay"
  { yyVal = null; }
  break;
case 129:
#line 786 "cs-parser.jay"
  { yyVal = null; }
  break;
case 131:
#line 792 "cs-parser.jay"
  {
		yyVal = yyVals[0+yyTop];
	  }
  break;
case 132:
#line 799 "cs-parser.jay"
  { 
		current_local_parameters = new Parameters (implicit_value_parameters, null);
	  }
  break;
case 133:
#line 803 "cs-parser.jay"
  {
		yyVal = yyVals[0+yyTop];
		current_local_parameters = null;
	  }
  break;
case 135:
#line 811 "cs-parser.jay"
  { yyVal = new Block (null); }
  break;
case 136:
#line 818 "cs-parser.jay"
  {
		Interface new_interface;
		string full_interface_name = MakeName ((string) yyVals[0+yyTop]);

		new_interface = new Interface (current_container, full_interface_name, (int) yyVals[-2+yyTop]);
		if (current_interface != null)
			error (-2, "Internal compiler error: interface inside interface");
		current_interface = new_interface;
		tree.RecordType (full_class_name, new_interface);
	  }
  break;
case 137:
#line 830 "cs-parser.jay"
  { 
		Interface new_interface = (Interface) current_interface;

		if (yyVals[-1+yyTop] != null)
			new_interface.Bases = (ArrayList) yyVals[-1+yyTop];

		current_interface = null;
		CheckDef (current_container.AddInterface (new_interface), new_interface.Name);
	  }
  break;
case 138:
#line 842 "cs-parser.jay"
  { yyVal = null; }
  break;
case 140:
#line 847 "cs-parser.jay"
  { yyVal = yyVals[0+yyTop]; }
  break;
case 141:
#line 852 "cs-parser.jay"
  {
		ArrayList interfaces = new ArrayList ();

		interfaces.Add (yyVals[0+yyTop]);
	  }
  break;
case 142:
#line 858 "cs-parser.jay"
  {
		ArrayList interfaces = (ArrayList) yyVals[-2+yyTop];
		interfaces.Add (yyVals[0+yyTop]);
		yyVal = interfaces;
	  }
  break;
case 148:
#line 883 "cs-parser.jay"
  { 
		InterfaceMethod m = (InterfaceMethod) yyVals[0+yyTop];

		CheckDef (current_interface.AddMethod (m), m.Name);
	  }
  break;
case 149:
#line 889 "cs-parser.jay"
  { 
		InterfaceProperty p = (InterfaceProperty) yyVals[0+yyTop];

		CheckDef (current_interface.AddProperty (p), p.Name);
          }
  break;
case 150:
#line 895 "cs-parser.jay"
  { 
		InterfaceEvent e = (InterfaceEvent) yyVals[0+yyTop];

		CheckDef (current_interface.AddEvent (e), e.Name);
	  }
  break;
case 151:
#line 901 "cs-parser.jay"
  { 
		InterfaceIndexer i = (InterfaceIndexer) yyVals[0+yyTop];

		CheckDef (current_interface.AddIndexer (i), "indexer");
	  }
  break;
case 152:
#line 909 "cs-parser.jay"
  { yyVal = false; }
  break;
case 153:
#line 910 "cs-parser.jay"
  { yyVal = true; }
  break;
case 154:
#line 917 "cs-parser.jay"
  {
		yyVal = new InterfaceMethod ((TypeRef) yyVals[-5+yyTop], (string) yyVals[-4+yyTop], (bool) yyVals[-6+yyTop], (Parameters) yyVals[-2+yyTop]);
	  }
  break;
case 155:
#line 923 "cs-parser.jay"
  {
		yyVal = new InterfaceMethod (type ("void"), (string) yyVals[-4+yyTop], (bool) yyVals[-6+yyTop], (Parameters) yyVals[-2+yyTop]);
	  }
  break;
case 156:
#line 933 "cs-parser.jay"
  { lexer.properties = true; }
  break;
case 157:
#line 935 "cs-parser.jay"
  { lexer.properties = false; }
  break;
case 158:
#line 937 "cs-parser.jay"
  {
	        int gs = (int) yyVals[-2+yyTop];

		yyVal = new InterfaceProperty ((TypeRef) yyVals[-6+yyTop], (string) yyVals[-5+yyTop], (bool) yyVals[-7+yyTop], 
					    (gs & 1) == 1, (gs & 2) == 2);
	  }
  break;
case 159:
#line 946 "cs-parser.jay"
  { yyVal = 1; }
  break;
case 160:
#line 947 "cs-parser.jay"
  { yyVal = 2; }
  break;
case 161:
#line 949 "cs-parser.jay"
  { yyVal = 3; }
  break;
case 162:
#line 951 "cs-parser.jay"
  { yyVal = 3; }
  break;
case 163:
#line 956 "cs-parser.jay"
  {
		yyVal = new InterfaceEvent ((TypeRef) yyVals[-2+yyTop], (string) yyVals[-1+yyTop], (bool) yyVals[-4+yyTop]);
	  }
  break;
case 164:
#line 965 "cs-parser.jay"
  { lexer.properties = true; }
  break;
case 165:
#line 967 "cs-parser.jay"
  { lexer.properties = false; }
  break;
case 166:
#line 969 "cs-parser.jay"
  {
		int a_flags = (int) yyVals[-2+yyTop];

	  	bool do_get = (a_flags & 1) == 1;
		bool do_set = (a_flags & 2) == 2;

		yyVal = new InterfaceIndexer ((TypeRef) yyVals[-9+yyTop], (Parameters) yyVals[-6+yyTop], do_get, do_set, (bool) yyVals[-10+yyTop]);
	  }
  break;
case 167:
#line 981 "cs-parser.jay"
  {
		/* FIXME: validate that opt_modifiers is exactly: PUBLIC and STATIC*/
	  }
  break;
case 168:
#line 989 "cs-parser.jay"
  {
		/* FIXME: since reduce/reduce on this*/
	 	/* rule, validate overloadable_operator is unary*/
	  }
  break;
case 169:
#line 998 "cs-parser.jay"
  {
		/* FIXME: because of the reduce/reduce on PLUS and MINUS*/
		/* validate overloadable_operator is binary*/
	  }
  break;
case 195:
#line 1043 "cs-parser.jay"
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
case 196:
#line 1063 "cs-parser.jay"
  {
		ConstructorInitializer i = null;

		if (yyVals[0+yyTop] != null)
			i = (ConstructorInitializer) yyVals[0+yyTop];

		yyVal = new Constructor ((string) yyVals[-4+yyTop], (Parameters) yyVals[-2+yyTop], i);
	
		current_local_parameters = (Parameters) yyVals[-2+yyTop];
	  }
  break;
case 197:
#line 1076 "cs-parser.jay"
  { yyVal = null; }
  break;
case 199:
#line 1082 "cs-parser.jay"
  {
		yyVal = new ConstructorBaseInitializer ((ArrayList) yyVals[-1+yyTop]);
	  }
  break;
case 200:
#line 1086 "cs-parser.jay"
  {
		yyVal = new ConstructorThisInitializer ((ArrayList) yyVals[-1+yyTop]);
	  }
  break;
case 201:
#line 1093 "cs-parser.jay"
  {
		Method d = new Method (type ("void"), 0, "Finalize", new Parameters (null, null));

		d.Block = (Block) yyVals[0+yyTop];
		CheckDef (current_container.AddMethod (d), d.Name);
	  }
  break;
case 202:
#line 1105 "cs-parser.jay"
  { note ("validate that the flags only contain new public protected internal private static virtual sealed override abstract"); }
  break;
case 203:
#line 1110 "cs-parser.jay"
  { note ("validate that the flags only contain new public protected internal private static virtual sealed override abstract"); }
  break;
case 208:
#line 1129 "cs-parser.jay"
  { 
		/* The signature is computed from the signature of the indexer.  Look*/
	 	/* at section 3.6 on the spec*/
		note ("verify modifiers are NEW PUBLIC PROTECTED INTERNAL PRIVATE VIRTUAL SEALED OVERRIDE ABSTRACT"); 
	  }
  break;
case 211:
#line 1148 "cs-parser.jay"
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
case 212:
#line 1165 "cs-parser.jay"
  { yyVal = type ("System.Int32"); }
  break;
case 213:
#line 1166 "cs-parser.jay"
  { yyVal = yyVals[0+yyTop];   }
  break;
case 214:
#line 1171 "cs-parser.jay"
  {
		yyVal = yyVals[-1+yyTop];
	  }
  break;
case 215:
#line 1175 "cs-parser.jay"
  {
		yyVal = yyVals[-2+yyTop];
	  }
  break;
case 216:
#line 1181 "cs-parser.jay"
  { yyVal = new ArrayList (); }
  break;
case 217:
#line 1182 "cs-parser.jay"
  { yyVal = yyVals[0+yyTop]; }
  break;
case 218:
#line 1187 "cs-parser.jay"
  {
		ArrayList l = new ArrayList ();

		l.Add (yyVals[0+yyTop]);
		yyVal = l;
	  }
  break;
case 219:
#line 1194 "cs-parser.jay"
  {
		ArrayList l = (ArrayList) yyVals[-2+yyTop];

		l.Add (yyVals[0+yyTop]);

		yyVal = l;
	  }
  break;
case 220:
#line 1205 "cs-parser.jay"
  {
		yyVal = new VariableDeclaration ((string) yyVals[0+yyTop], null);
	  }
  break;
case 221:
#line 1209 "cs-parser.jay"
  { 
		yyVal = new VariableDeclaration ((string) yyVals[-2+yyTop], yyVals[0+yyTop]);
	  }
  break;
case 222:
#line 1222 "cs-parser.jay"
  { note ("validate that modifiers only contains NEW PUBLIC PROTECTED INTERNAL PRIVATE"); }
  break;
case 225:
#line 1240 "cs-parser.jay"
  {  	/* class_type */
		/* 
	           This does interfaces, delegates, struct_types, class_types, 
	           parent classes, and more! 4.2 
	         */
		yyVal = type ((string) yyVals[0+yyTop]); 
	  }
  break;
case 228:
#line 1253 "cs-parser.jay"
  {
		ArrayList types = new ArrayList ();

		types.Add (yyVals[0+yyTop]);
		yyVal = types;
	  }
  break;
case 229:
#line 1260 "cs-parser.jay"
  {
		ArrayList types = new ArrayList ();
		types.Add (yyVals[0+yyTop]);
		yyVal = types;
	  }
  break;
case 230:
#line 1272 "cs-parser.jay"
  { yyVal = type ("System.Object"); }
  break;
case 231:
#line 1273 "cs-parser.jay"
  { yyVal = type ("System.String"); }
  break;
case 232:
#line 1274 "cs-parser.jay"
  { yyVal = type ("System.Boolean"); }
  break;
case 233:
#line 1275 "cs-parser.jay"
  { yyVal = type ("System.Decimal"); }
  break;
case 234:
#line 1276 "cs-parser.jay"
  { yyVal = type ("System.Single"); }
  break;
case 235:
#line 1277 "cs-parser.jay"
  { yyVal = type ("System.Double"); }
  break;
case 237:
#line 1282 "cs-parser.jay"
  { yyVal = type ("System.SByte"); }
  break;
case 238:
#line 1283 "cs-parser.jay"
  { yyVal = type ("System.Byte"); }
  break;
case 239:
#line 1284 "cs-parser.jay"
  { yyVal = type ("System.Int16"); }
  break;
case 240:
#line 1285 "cs-parser.jay"
  { yyVal = type ("System.UInt16"); }
  break;
case 241:
#line 1286 "cs-parser.jay"
  { yyVal = type ("System.Int32"); }
  break;
case 242:
#line 1287 "cs-parser.jay"
  { yyVal = type ("System.UInt32"); }
  break;
case 243:
#line 1288 "cs-parser.jay"
  { yyVal = type ("System.Int64"); }
  break;
case 244:
#line 1289 "cs-parser.jay"
  { yyVal = type ("System.UInt64"); }
  break;
case 245:
#line 1290 "cs-parser.jay"
  { yyVal = type ("System.Char"); }
  break;
case 247:
#line 1299 "cs-parser.jay"
  {
		yyVal = yyVals[-1+yyTop];
		/* FIXME: We need to create a type for the nested thing.*/
	  }
  break;
case 248:
#line 1310 "cs-parser.jay"
  {
		/* 7.5.1: Literals*/
		
	  }
  break;
case 249:
#line 1316 "cs-parser.jay"
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
case 266:
#line 1358 "cs-parser.jay"
  { yyVal = new CharLiteral ((char) lexer.Value); }
  break;
case 267:
#line 1359 "cs-parser.jay"
  { yyVal = new StringLiteral ((string) lexer.Value); }
  break;
case 268:
#line 1360 "cs-parser.jay"
  { yyVal = new NullLiteral (); }
  break;
case 269:
#line 1364 "cs-parser.jay"
  { yyVal = new FloatLiteral ((float) lexer.Value); }
  break;
case 270:
#line 1365 "cs-parser.jay"
  { yyVal = new DoubleLiteral ((double) lexer.Value); }
  break;
case 271:
#line 1366 "cs-parser.jay"
  { yyVal = new DecimalLiteral ((decimal) lexer.Value); }
  break;
case 272:
#line 1370 "cs-parser.jay"
  { yyVal = new IntLiteral ((Int32) lexer.Value); }
  break;
case 273:
#line 1374 "cs-parser.jay"
  { yyVal = new BoolLiteral (true); }
  break;
case 274:
#line 1375 "cs-parser.jay"
  { yyVal = new BoolLiteral (false); }
  break;
case 275:
#line 1380 "cs-parser.jay"
  { yyVal = yyVals[-1+yyTop]; }
  break;
case 276:
#line 1385 "cs-parser.jay"
  {
		yyVal = new MemberAccess ((Expression) yyVals[-2+yyTop], (string) yyVals[0+yyTop]);
	  }
  break;
case 277:
#line 1389 "cs-parser.jay"
  {
		yyVal = new BuiltinTypeAccess ((TypeRef) yyVals[-2+yyTop], (string) yyVals[0+yyTop]);
	  }
  break;
case 279:
#line 1400 "cs-parser.jay"
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
case 280:
#line 1414 "cs-parser.jay"
  { yyVal = new ArrayList (); }
  break;
case 282:
#line 1420 "cs-parser.jay"
  { 
		ArrayList list = new ArrayList ();
		list.Add (yyVals[0+yyTop]);
		yyVal = list;
	  }
  break;
case 283:
#line 1426 "cs-parser.jay"
  {
		ArrayList list = (ArrayList) yyVals[-2+yyTop];
		list.Add (yyVals[0+yyTop]);
		yyVal = list;
	  }
  break;
case 284:
#line 1435 "cs-parser.jay"
  {
		yyVal = new Argument ((Expression) yyVals[0+yyTop], Argument.AType.Expression);
	  }
  break;
case 285:
#line 1439 "cs-parser.jay"
  { 
		yyVal = new Argument ((Expression) yyVals[0+yyTop], Argument.AType.Ref);
	  }
  break;
case 286:
#line 1443 "cs-parser.jay"
  { 
		yyVal = new Argument ((Expression) yyVals[0+yyTop], Argument.AType.Out);
	  }
  break;
case 287:
#line 1449 "cs-parser.jay"
  { note ("section 5.4"); yyVal = yyVals[0+yyTop]; }
  break;
case 291:
#line 1464 "cs-parser.jay"
  {
		yyVal = new This ();
	  }
  break;
case 294:
#line 1478 "cs-parser.jay"
  {
		yyVal = new Unary (Unary.Operator.PostIncrement, (Expression) yyVals[-1+yyTop]);
	  }
  break;
case 295:
#line 1485 "cs-parser.jay"
  {
		yyVal = new Unary (Unary.Operator.PostDecrement, (Expression) yyVals[-1+yyTop]);
	  }
  break;
case 298:
#line 1497 "cs-parser.jay"
  {
		yyVal = new New ((TypeRef) yyVals[-3+yyTop], (ArrayList) yyVals[-1+yyTop]);
	  }
  break;
case 316:
#line 1557 "cs-parser.jay"
  {
		yyVal = new TypeOf ((TypeRef) yyVals[-1+yyTop]);
	  }
  break;
case 317:
#line 1563 "cs-parser.jay"
  { 
		yyVal = new SizeOf ((TypeRef) yyVals[-1+yyTop]);

		note ("Verify type is unmanaged"); 
		note ("if (5.8) builtin, yield constant expression");
	  }
  break;
case 321:
#line 1581 "cs-parser.jay"
  { 
	  	yyVal = new Unary (Unary.Operator.Plus, (Expression) yyVals[0+yyTop]);
	  }
  break;
case 322:
#line 1585 "cs-parser.jay"
  { 
		yyVal = new Unary (Unary.Operator.Minus, (Expression) yyVals[0+yyTop]);
	  }
  break;
case 323:
#line 1589 "cs-parser.jay"
  {
		yyVal = new Unary (Unary.Operator.Negate, (Expression) yyVals[0+yyTop]);
	  }
  break;
case 324:
#line 1593 "cs-parser.jay"
  {
		yyVal = new Unary (Unary.Operator.BitComplement, (Expression) yyVals[0+yyTop]);
	  }
  break;
case 325:
#line 1597 "cs-parser.jay"
  {
		yyVal = new Unary (Unary.Operator.Indirection, (Expression) yyVals[0+yyTop]);
	  }
  break;
case 326:
#line 1601 "cs-parser.jay"
  {
		yyVal = new Unary (Unary.Operator.AddressOf, (Expression) yyVals[0+yyTop]);
	  }
  break;
case 327:
#line 1605 "cs-parser.jay"
  {
		yyVal = new Unary (Unary.Operator.PreIncrement, (Expression) yyVals[0+yyTop]);
	  }
  break;
case 328:
#line 1609 "cs-parser.jay"
  {
		yyVal = new Unary (Unary.Operator.PreDecrement, (Expression) yyVals[0+yyTop]);
	  }
  break;
case 330:
#line 1627 "cs-parser.jay"
  {
		yyVal = new Cast (type ((string) yyVals[-2+yyTop]), (Expression) yyVals[0+yyTop]);
	  }
  break;
case 331:
#line 1631 "cs-parser.jay"
  {
		yyVal = new Cast ((TypeRef) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 333:
#line 1639 "cs-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.Multiply, 
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 334:
#line 1644 "cs-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.Divide, 
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 335:
#line 1649 "cs-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.Modulo, 
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 337:
#line 1658 "cs-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.Add, 
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 338:
#line 1663 "cs-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.Substract, 
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 340:
#line 1672 "cs-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.ShiftLeft, 
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 341:
#line 1677 "cs-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.ShiftRight, 
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 343:
#line 1686 "cs-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.LessThan, 
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 344:
#line 1691 "cs-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.GreatherThan, 
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 345:
#line 1696 "cs-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.LessOrEqual, 
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 346:
#line 1701 "cs-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.GreatherOrEqual, 
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 347:
#line 1706 "cs-parser.jay"
  {
		yyVal = new Probe (Probe.Operator.Is, 
			         (Expression) yyVals[-2+yyTop], (TypeRef) yyVals[0+yyTop]);
	  }
  break;
case 348:
#line 1711 "cs-parser.jay"
  {
		yyVal = new Probe (Probe.Operator.As, 
			         (Expression) yyVals[-2+yyTop], (TypeRef) yyVals[0+yyTop]);
	  }
  break;
case 350:
#line 1720 "cs-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.Equal, 
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 351:
#line 1725 "cs-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.NotEqual, 
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 353:
#line 1734 "cs-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.BitwiseAnd, 
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 355:
#line 1743 "cs-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.ExclusiveOr, 
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 357:
#line 1752 "cs-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.BitwiseOr, 
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 359:
#line 1761 "cs-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.LogicalAnd, 
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 361:
#line 1770 "cs-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.LogicalOr, 
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 363:
#line 1779 "cs-parser.jay"
  {
		yyVal = new Conditional ((Expression) yyVals[-4+yyTop], (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 364:
#line 1786 "cs-parser.jay"
  {
		yyVal = new Assign ((Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 365:
#line 1790 "cs-parser.jay"
  {
		yyVal = new Assign ((Expression) yyVals[-2+yyTop],
				 new Binary (Binary.Operator.Multiply, 
					     (Expression) yyVals[-2+yyTop],
					     (Expression) yyVals[0+yyTop]));
	  }
  break;
case 366:
#line 1797 "cs-parser.jay"
  {
		yyVal = new Assign ((Expression) yyVals[-2+yyTop],
				 new Binary (Binary.Operator.Divide, 
					     (Expression) yyVals[-2+yyTop],
					     (Expression) yyVals[0+yyTop]));
	  }
  break;
case 367:
#line 1804 "cs-parser.jay"
  {
		yyVal = new Assign ((Expression) yyVals[-2+yyTop],
				 new Binary (Binary.Operator.Modulo, 
					     (Expression) yyVals[-2+yyTop],
					     (Expression) yyVals[0+yyTop]));
	  }
  break;
case 368:
#line 1811 "cs-parser.jay"
  {
		yyVal = new Assign ((Expression) yyVals[-2+yyTop],
				 new Binary (Binary.Operator.Add, 
					     (Expression) yyVals[-2+yyTop],
					     (Expression) yyVals[0+yyTop]));
	  }
  break;
case 369:
#line 1818 "cs-parser.jay"
  {
		yyVal = new Assign ((Expression) yyVals[-2+yyTop],
				 new Binary (Binary.Operator.Substract, 
					     (Expression) yyVals[-2+yyTop],
					     (Expression) yyVals[0+yyTop]));
	  }
  break;
case 370:
#line 1825 "cs-parser.jay"
  {
		yyVal = new Assign ((Expression) yyVals[-2+yyTop],
				 new Binary (Binary.Operator.ShiftLeft, 
					     (Expression) yyVals[-2+yyTop],
					     (Expression) yyVals[0+yyTop]));
	  }
  break;
case 371:
#line 1832 "cs-parser.jay"
  {
		yyVal = new Assign ((Expression) yyVals[-2+yyTop],
				 new Binary (Binary.Operator.ShiftRight, 
					     (Expression) yyVals[-2+yyTop],
					     (Expression) yyVals[0+yyTop]));
	  }
  break;
case 372:
#line 1839 "cs-parser.jay"
  {
		yyVal = new Assign ((Expression) yyVals[-2+yyTop],
				 new Binary (Binary.Operator.BitwiseAnd, 
					     (Expression) yyVals[-2+yyTop],
					     (Expression) yyVals[0+yyTop]));
	  }
  break;
case 373:
#line 1846 "cs-parser.jay"
  {
		yyVal = new Assign ((Expression) yyVals[-2+yyTop],
				 new Binary (Binary.Operator.BitwiseOr, 
					     (Expression) yyVals[-2+yyTop],
					     (Expression) yyVals[0+yyTop]));
	  }
  break;
case 374:
#line 1853 "cs-parser.jay"
  {
		yyVal = new Assign ((Expression) yyVals[-2+yyTop],
				 new Binary (Binary.Operator.ExclusiveOr, 
					     (Expression) yyVals[-2+yyTop],
					     (Expression) yyVals[0+yyTop]));
	  }
  break;
case 378:
#line 1871 "cs-parser.jay"
  { CheckBoolean ((Expression) yyVals[0+yyTop]); yyVal = yyVals[0+yyTop]; }
  break;
case 379:
#line 1881 "cs-parser.jay"
  {
		Class new_class;
		string full_class_name = MakeName ((string) yyVals[0+yyTop]);

		new_class = new Class (current_container, full_class_name, (int) yyVals[-2+yyTop]);
		current_container = new_class;
		current_container.Namespace = current_namespace;
		tree.RecordType (full_class_name, new_class);
	  }
  break;
case 380:
#line 1893 "cs-parser.jay"
  {
		Class new_class = (Class) current_container;

		if (yyVals[-2+yyTop] != null)
			new_class.Bases = (ArrayList) yyVals[-2+yyTop];

		current_container = current_container.Parent;
		CheckDef (current_container.AddClass (new_class), new_class.Name);

		yyVal = new_class;
	  }
  break;
case 381:
#line 1907 "cs-parser.jay"
  { yyVal = (int) 0; }
  break;
case 384:
#line 1913 "cs-parser.jay"
  { 
		int m1 = (int) yyVals[-1+yyTop];
		int m2 = (int) yyVals[0+yyTop];

		if ((m1 & m2) != 0)
			error (1002, "Duplicate modifier: `" + Modifiers.Name (m2) + "'");

		yyVal = (int) (m1 | m2);
	  }
  break;
case 385:
#line 1925 "cs-parser.jay"
  { yyVal = Modifiers.NEW; }
  break;
case 386:
#line 1926 "cs-parser.jay"
  { yyVal = Modifiers.PUBLIC; }
  break;
case 387:
#line 1927 "cs-parser.jay"
  { yyVal = Modifiers.PROTECTED; }
  break;
case 388:
#line 1928 "cs-parser.jay"
  { yyVal = Modifiers.INTERNAL; }
  break;
case 389:
#line 1929 "cs-parser.jay"
  { yyVal = Modifiers.PRIVATE; }
  break;
case 390:
#line 1930 "cs-parser.jay"
  { yyVal = Modifiers.ABSTRACT; }
  break;
case 391:
#line 1931 "cs-parser.jay"
  { yyVal = Modifiers.SEALED; }
  break;
case 392:
#line 1932 "cs-parser.jay"
  { yyVal = Modifiers.STATIC; }
  break;
case 393:
#line 1933 "cs-parser.jay"
  { yyVal = Modifiers.READONLY; }
  break;
case 394:
#line 1934 "cs-parser.jay"
  { yyVal = Modifiers.VIRTUAL; }
  break;
case 395:
#line 1935 "cs-parser.jay"
  { yyVal = Modifiers.OVERRIDE; }
  break;
case 396:
#line 1936 "cs-parser.jay"
  { yyVal = Modifiers.EXTERN; }
  break;
case 397:
#line 1940 "cs-parser.jay"
  { yyVal = null; }
  break;
case 398:
#line 1941 "cs-parser.jay"
  { yyVal = yyVals[0+yyTop]; }
  break;
case 399:
#line 1945 "cs-parser.jay"
  { yyVal = yyVals[0+yyTop]; }
  break;
case 400:
#line 1963 "cs-parser.jay"
  {
		current_block = new Block (current_block);
	  }
  break;
case 401:
#line 1967 "cs-parser.jay"
  { 
		while (current_block.Implicit)
			current_block = current_block.Parent;
		yyVal = current_block;
		current_block = current_block.Parent;
	  }
  break;
case 406:
#line 1987 "cs-parser.jay"
  {
		if ((Block) yyVals[0+yyTop] != current_block){
			current_block.AddStatement ((Statement) yyVals[0+yyTop]);
			current_block = (Block) yyVals[0+yyTop];
		}
	  }
  break;
case 407:
#line 1994 "cs-parser.jay"
  {
		current_block.AddStatement ((Statement) yyVals[0+yyTop]);
	  }
  break;
case 408:
#line 1998 "cs-parser.jay"
  {
		current_block.AddStatement ((Statement) yyVals[0+yyTop]);
	  }
  break;
case 420:
#line 2019 "cs-parser.jay"
  {
		  yyVal = new EmptyStatement ();
	  }
  break;
case 421:
#line 2026 "cs-parser.jay"
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
case 424:
#line 2054 "cs-parser.jay"
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
case 425:
#line 2075 "cs-parser.jay"
  {
		/* FIXME: Do something smart with the type here.*/
		yyVal = yyVals[-1+yyTop]; 
	  }
  break;
case 426:
#line 2080 "cs-parser.jay"
  {
		yyVal = type ("VOID SOMETHING TYPE");
	  }
  break;
case 432:
#line 2102 "cs-parser.jay"
  {
		yyVal = declare_local_variables ((TypeRef) yyVals[-1+yyTop], (ArrayList) yyVals[0+yyTop]);
	  }
  break;
case 434:
#line 2114 "cs-parser.jay"
  {
		yyVal = yyVals[-1+yyTop];
	  }
  break;
case 435:
#line 2124 "cs-parser.jay"
  { yyVal = new StatementExpression ((Expression) yyVals[0+yyTop]); }
  break;
case 436:
#line 2125 "cs-parser.jay"
  { yyVal = new StatementExpression ((Expression) yyVals[0+yyTop]); }
  break;
case 437:
#line 2126 "cs-parser.jay"
  { yyVal = new StatementExpression ((Expression) yyVals[0+yyTop]); }
  break;
case 438:
#line 2127 "cs-parser.jay"
  { yyVal = new StatementExpression ((Expression) yyVals[0+yyTop]); }
  break;
case 439:
#line 2128 "cs-parser.jay"
  { yyVal = new StatementExpression ((Expression) yyVals[0+yyTop]); }
  break;
case 440:
#line 2129 "cs-parser.jay"
  { yyVal = new StatementExpression ((Expression) yyVals[0+yyTop]); }
  break;
case 441:
#line 2130 "cs-parser.jay"
  { yyVal = new StatementExpression ((Expression) yyVals[0+yyTop]); }
  break;
case 442:
#line 2135 "cs-parser.jay"
  { note ("complain if this is a delegate maybe?"); }
  break;
case 445:
#line 2146 "cs-parser.jay"
  { 
		yyVal = new If ((Expression) yyVals[-2+yyTop], (Statement) yyVals[0+yyTop]);
	  }
  break;
case 446:
#line 2151 "cs-parser.jay"
  {
		yyVal = new If ((Expression) yyVals[-4+yyTop], (Statement) yyVals[-2+yyTop], (Statement) yyVals[0+yyTop]);
	  }
  break;
case 447:
#line 2159 "cs-parser.jay"
  {
		yyVal = new Switch ((Expression) yyVals[-2+yyTop], (ArrayList) yyVals[0+yyTop]);
	  }
  break;
case 448:
#line 2168 "cs-parser.jay"
  {
		yyVal = yyVals[-1+yyTop];
	  }
  break;
case 449:
#line 2174 "cs-parser.jay"
  { yyVal = new ArrayList (); }
  break;
case 451:
#line 2180 "cs-parser.jay"
  {
		ArrayList sections = new ArrayList ();

		sections.Add (yyVals[0+yyTop]);
		yyVal = sections;
	  }
  break;
case 452:
#line 2187 "cs-parser.jay"
  {
		ArrayList sections = (ArrayList) yyVals[-1+yyTop];

		sections.Add (yyVals[0+yyTop]);
		yyVal = sections;
	  }
  break;
case 453:
#line 2197 "cs-parser.jay"
  {
		current_block = new Block (current_block);
	  }
  break;
case 454:
#line 2201 "cs-parser.jay"
  {
		while (current_block.Implicit)
			current_block = current_block.Parent;
		yyVal = new SwitchSection ((ArrayList) yyVals[-2+yyTop], current_block);
		current_block = current_block.Parent;
	  }
  break;
case 455:
#line 2211 "cs-parser.jay"
  {
		ArrayList labels = new ArrayList ();

		labels.Add (yyVals[0+yyTop]);
		yyVal = labels;
	  }
  break;
case 456:
#line 2218 "cs-parser.jay"
  {
		ArrayList labels = (ArrayList) (yyVals[-1+yyTop]);
		labels.Add (yyVals[0+yyTop]);

		yyVal = labels;
	  }
  break;
case 457:
#line 2227 "cs-parser.jay"
  { yyVal = new SwitchLabel ((Expression) yyVals[-1+yyTop]); }
  break;
case 458:
#line 2228 "cs-parser.jay"
  { yyVal = new SwitchLabel (null); }
  break;
case 463:
#line 2240 "cs-parser.jay"
  {
		yyVal = new While ((Expression) yyVals[-2+yyTop], (Statement) yyVals[0+yyTop]);
	}
  break;
case 464:
#line 2248 "cs-parser.jay"
  {
		yyVal = new Do ((Statement) yyVals[-5+yyTop], (Expression) yyVals[-2+yyTop]);
	  }
  break;
case 465:
#line 2259 "cs-parser.jay"
  {
		yyVal = new For ((Statement) yyVals[-6+yyTop], (Expression) yyVals[-4+yyTop], (Statement) yyVals[-2+yyTop], (Statement) yyVals[0+yyTop]);
	  }
  break;
case 466:
#line 2265 "cs-parser.jay"
  { yyVal = new EmptyStatement (); }
  break;
case 470:
#line 2275 "cs-parser.jay"
  { yyVal = new BoolLiteral (true); }
  break;
case 472:
#line 2280 "cs-parser.jay"
  { yyVal = new EmptyStatement (); }
  break;
case 475:
#line 2290 "cs-parser.jay"
  {
		Block b = new Block (null, true);

		b.AddStatement ((Statement) yyVals[0+yyTop]);
		yyVal = b;
	  }
  break;
case 476:
#line 2297 "cs-parser.jay"
  {
		Block b = (Block) yyVals[-2+yyTop];

		b.AddStatement ((Statement) yyVals[0+yyTop]);
		yyVal = yyVals[-2+yyTop];
	  }
  break;
case 477:
#line 2308 "cs-parser.jay"
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
case 483:
#line 2351 "cs-parser.jay"
  {
		yyVal = new Break ();
	  }
  break;
case 484:
#line 2358 "cs-parser.jay"
  {
		yyVal = new Continue ();
	  }
  break;
case 485:
#line 2365 "cs-parser.jay"
  {
		yyVal = new Goto ((string) yyVals[-1+yyTop]);
	  }
  break;
case 488:
#line 2374 "cs-parser.jay"
  {
		yyVal = new Return ((Expression) yyVals[-1+yyTop]);
	  }
  break;
case 489:
#line 2381 "cs-parser.jay"
  {
		yyVal = new Throw ((Expression) yyVals[-1+yyTop]);
	  }
  break;
case 492:
#line 2393 "cs-parser.jay"
  {
		DictionaryEntry cc = (DictionaryEntry) yyVals[0+yyTop];
		ArrayList s = null;

		if (cc.Key != null)
			s = (ArrayList) cc.Key;

		yyVal = new Try ((Block) yyVals[-1+yyTop], s, (Catch) cc.Value, null);
	  }
  break;
case 493:
#line 2403 "cs-parser.jay"
  {
		yyVal = new Try ((Block) yyVals[-1+yyTop], null, null, (Block) yyVals[0+yyTop]);
	  }
  break;
case 494:
#line 2407 "cs-parser.jay"
  {
		DictionaryEntry cc = (DictionaryEntry) yyVals[-1+yyTop];
		ArrayList s = null;

		if (cc.Key != null)
			s = (ArrayList) cc.Key;

		yyVal = new Try ((Block) yyVals[-2+yyTop], s, (Catch) cc.Value, (Block) yyVals[0+yyTop]);
	  }
  break;
case 495:
#line 2420 "cs-parser.jay"
  {
		DictionaryEntry pair = new DictionaryEntry ();

		pair.Key = yyVals[-1+yyTop]; 
		pair.Value = yyVals[0+yyTop];

		yyVal = pair;
	  }
  break;
case 496:
#line 2429 "cs-parser.jay"
  {
		DictionaryEntry pair = new DictionaryEntry ();
		pair.Key = yyVals[-1+yyTop];
		pair.Value = yyVals[-1+yyTop];

		yyVal = pair;
	  }
  break;
case 497:
#line 2439 "cs-parser.jay"
  { yyVal = null; }
  break;
case 499:
#line 2444 "cs-parser.jay"
  { yyVal = null; }
  break;
case 501:
#line 2450 "cs-parser.jay"
  {
		ArrayList l = new ArrayList ();

		l.Add (yyVals[0+yyTop]);
		yyVal = l;
	  }
  break;
case 502:
#line 2457 "cs-parser.jay"
  {
		ArrayList l = (ArrayList) yyVals[-1+yyTop];

		l.Add (yyVals[0+yyTop]);
		yyVal = l;
	  }
  break;
case 503:
#line 2467 "cs-parser.jay"
  {
		string id = null;

		if (yyVals[-2+yyTop] != null)
			id = (string) yyVals[-2+yyTop];

		yyVal = new Catch ((TypeRef) yyVals[-3+yyTop], id, (Block) yyVals[0+yyTop]);
	  }
  break;
case 504:
#line 2478 "cs-parser.jay"
  { yyVal = null; }
  break;
case 506:
#line 2484 "cs-parser.jay"
  {
		yyVal = new Catch (null, null, (Block) yyVals[0+yyTop]);
	  }
  break;
case 507:
#line 2491 "cs-parser.jay"
  {
		yyVal = yyVals[0+yyTop];
	  }
  break;
case 508:
#line 2498 "cs-parser.jay"
  {
		yyVal = new Checked ((Block) yyVals[0+yyTop]);
	  }
  break;
case 509:
#line 2505 "cs-parser.jay"
  {
		yyVal = new Unchecked ((Block) yyVals[0+yyTop]);
	  }
  break;
case 510:
#line 2512 "cs-parser.jay"
  {
		yyVal = new Lock ((Expression) yyVals[-2+yyTop], (Statement) yyVals[0+yyTop]);
	  }
  break;
#line 2677 "-"
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
   19,   19,    2,    2,   25,   26,   26,   28,   29,   29,
   29,   27,   27,   30,   30,   31,   32,   32,   34,   34,
   36,   36,   37,   37,   38,   39,   39,   40,   40,   41,
   41,   41,   41,   41,   41,   41,   41,   41,   41,   53,
   21,   52,   52,   55,   55,   56,   54,   58,   58,   59,
   59,   60,   60,   60,   60,   60,   60,   60,   60,   60,
   42,   62,   62,   63,   43,   65,   65,   66,   66,   67,
   67,   44,   69,   69,   70,   70,   72,   72,   74,   74,
   74,   75,   75,   77,   78,   78,   79,   79,   76,   71,
   71,   82,   83,   45,   81,   81,   87,   87,   85,   85,
   84,   89,   86,   88,   88,   91,   22,   90,   90,   93,
   94,   94,   92,   95,   95,   96,   96,   97,   97,   97,
   97,  102,  102,   98,   98,  104,  105,   99,  103,  103,
  103,  103,  100,  106,  107,  101,   48,  108,  108,  108,
  109,  109,  109,  109,  109,  109,  109,  109,  109,  109,
  109,  109,  109,  109,  109,  109,  109,  109,  109,  109,
  109,  109,  110,  110,   49,  111,  112,  112,  113,  113,
   50,   46,   46,  115,  115,  116,  117,   47,  118,  118,
   23,  119,  119,  120,  120,  122,  122,  123,  123,  124,
  124,   24,   33,    8,   61,   61,   61,   57,   57,  125,
  125,  125,  125,  125,  125,  125,  121,  121,  121,  121,
  121,  121,  121,  121,  121,   80,  126,  128,  128,  128,
  128,  128,  128,  128,  128,  128,  128,  128,  128,  128,
  128,  128,  129,  129,  129,  129,  129,  129,  145,  145,
  145,  144,  143,  143,  130,  131,  131,  146,  132,  114,
  114,  147,  147,  148,  148,  148,  149,  133,  150,  150,
  134,  135,  135,  136,  137,  138,  138,  151,  152,  153,
  153,  127,  127,  155,  156,  156,  157,  157,  154,  154,
   68,   68,   68,  158,  158,  139,  140,  141,  142,  159,
  159,  159,  159,  159,  159,  159,  159,  159,  159,  160,
  160,  161,  161,  161,  161,  162,  162,  162,  163,  163,
  163,  164,  164,  164,  164,  164,  164,  164,  165,  165,
  165,  166,  166,  167,  167,  168,  168,  169,  169,  170,
  170,  171,  171,  172,  172,  172,  172,  172,  172,  172,
  172,  172,  172,  172,   35,   35,   64,  173,  175,   20,
   51,   51,  176,  176,  177,  177,  177,  177,  177,  177,
  177,  177,  177,  177,  177,  177,  174,  174,  178,  180,
   73,  179,  179,  181,  181,  182,  182,  182,  184,  184,
  184,  184,  184,  184,  184,  184,  184,  184,  184,  186,
  185,  183,  183,  198,  198,  198,  199,  199,  200,  200,
  201,  196,  197,  187,  202,  202,  202,  202,  202,  202,
  202,  203,  188,  188,  204,  204,  205,  206,  207,  207,
  208,  208,  211,  209,  210,  210,  212,  212,  189,  189,
  189,  189,  213,  214,  215,  217,  217,  220,  220,  218,
  218,  219,  219,  222,  221,  221,  216,  190,  190,  190,
  190,  190,  223,  224,  225,  225,  225,  226,  227,  228,
  228,  191,  191,  191,  229,  229,  232,  232,  233,  233,
  231,  231,  235,  236,  236,  234,  230,  192,  193,  194,
  195,  237,
  };
   static  short [] yyLen = {           2,
    4,    1,    2,    1,    1,    5,    3,    1,    2,    0,
    5,    0,    1,    0,    1,    1,    3,    1,    4,    0,
    1,    0,    1,    1,    2,    1,    1,    1,    1,    1,
    1,    1,    0,    2,    4,    0,    1,    2,    1,    1,
    1,    1,    3,    1,    1,    1,    0,    3,    1,    3,
    0,    1,    1,    2,    3,    0,    1,    1,    2,    1,
    1,    1,    1,    1,    1,    1,    1,    1,    1,    0,
    8,    0,    1,    1,    2,    2,    3,    0,    1,    1,
    2,    1,    1,    1,    1,    1,    1,    1,    1,    1,
    6,    1,    3,    3,    5,    1,    3,    3,    1,    1,
    1,    2,    7,    7,    1,    1,    0,    1,    1,    3,
    1,    1,    3,    4,    0,    1,    1,    1,    4,    1,
    3,    0,    0,    9,    2,    2,    0,    1,    0,    1,
    3,    0,    4,    1,    1,    0,    7,    0,    1,    2,
    1,    3,    3,    0,    1,    1,    2,    1,    1,    1,
    1,    0,    1,    8,    8,    0,    0,    9,    3,    3,
    6,    6,    6,    0,    0,   12,    4,    7,   10,    1,
    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,
    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,
    1,    1,    7,    7,    4,    5,    0,    1,    5,    5,
    6,    6,    9,    2,    2,    3,    3,    6,    5,    7,
    7,    0,    2,    3,    4,    0,    1,    1,    3,    2,
    4,    9,    1,    1,    1,    1,    1,    1,    2,    1,
    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,
    1,    1,    1,    1,    1,    1,    2,    1,    1,    1,
    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,
    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,
    1,    1,    1,    1,    3,    3,    3,    1,    4,    0,
    1,    1,    3,    1,    2,    2,    1,    4,    1,    3,
    1,    3,    4,    2,    2,    1,    1,    5,    7,    0,
    1,    1,    2,    3,    0,    1,    1,    2,    0,    1,
    2,    3,    4,    1,    3,    4,    4,    4,    4,    1,
    2,    2,    2,    2,    2,    2,    2,    2,    1,    4,
    4,    1,    3,    3,    3,    1,    3,    3,    1,    3,
    3,    1,    3,    3,    3,    3,    3,    3,    1,    3,
    3,    1,    3,    1,    3,    1,    3,    1,    3,    1,
    3,    1,    5,    3,    3,    3,    3,    3,    3,    3,
    3,    3,    3,    3,    1,    1,    1,    1,    0,    8,
    0,    1,    1,    2,    1,    1,    1,    1,    1,    1,
    1,    1,    1,    1,    1,    1,    0,    1,    2,    0,
    4,    0,    1,    1,    2,    1,    1,    1,    1,    1,
    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,
    3,    2,    2,    2,    2,    2,    0,    1,    1,    2,
    3,    2,    3,    2,    1,    1,    1,    1,    1,    1,
    1,    1,    1,    1,    5,    7,    5,    3,    0,    1,
    1,    2,    0,    3,    1,    2,    3,    2,    1,    1,
    1,    1,    5,    7,    9,    0,    1,    1,    1,    0,
    1,    0,    1,    1,    1,    3,    8,    1,    1,    1,
    1,    1,    2,    2,    3,    4,    3,    3,    3,    0,
    1,    3,    3,    4,    2,    2,    0,    1,    0,    1,
    1,    2,    6,    0,    1,    2,    2,    2,    2,    5,
    5,    2,
  };
   static  short [] yyDefRed = {            0,
    0,    0,    0,    0,    2,    4,    5,    0,   18,    0,
    0,    0,    0,    0,    3,    0,    7,    0,   40,   41,
   39,    0,   37,    0,    0,    0,    0,   27,    0,   24,
   26,   28,   29,   30,   31,   32,   34,   16,    0,   17,
    0,  223,    0,   42,   44,   45,   46,   38,    0,  390,
  396,  388,  385,  395,  389,  387,  386,  393,  391,  392,
  394,    0,    0,  383,    1,   25,    6,    0,  232,  238,
  245,    0,  233,  235,  274,  234,  241,  243,    0,  268,
  230,  237,  239,    0,  231,  291,  273,    0,  242,  244,
    0,  240,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  272,  269,  270,  271,  266,  267,    0,    0,   49,
  236,  278,    0,  248,  250,  251,  252,  253,  254,  255,
  256,  257,  258,  259,  260,  261,  262,  263,  264,  265,
    0,  296,  297,    0,  329,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  375,  376,   35,    0,    0,
    0,    0,    0,    0,    0,  384,    0,    0,    0,  225,
    0,  226,  227,    0,    0,    0,    0,    0,    0,  324,
  321,  322,  323,  326,  325,  327,  328,   48,    0,    0,
    0,    0,  294,  295,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,   43,    0,    0,
  379,    0,    0,  136,   70,  289,    0,  292,    0,    0,
    0,  247,    0,    0,    0,    0,    0,  275,    0,   50,
    0,    0,    0,  284,    0,    0,  282,  276,  277,  364,
  365,  366,  367,  368,  369,  370,  371,  372,  374,  373,
  333,  335,  334,  332,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,   13,   11,    0,    0,    0,    0,    0,
    0,    0,  293,    0,  318,  307,    0,    0,    0,    0,
  303,  317,  316,  319,  330,  331,  288,  287,  286,  285,
  279,    0,    0,    0,    0,    0,  398,    0,  213,    0,
    0,    0,    0,  139,    0,    0,    0,   74,  290,    0,
  304,  308,  298,  283,  363,   19,    0,    0,    0,    0,
    0,    0,    0,  111,  112,    0,    0,    0,  218,  211,
  246,  141,    0,    0,  137,    0,    0,    0,   75,  301,
    0,    0,    0,   69,    0,    0,   58,   60,   61,   62,
   63,   64,   65,   66,   67,   68,    0,  380,  118,    0,
  117,    0,  116,    0,    0,    0,  214,    0,    0,    0,
    0,    0,  146,  148,  149,  150,  151,    0,   90,   82,
   83,   84,   85,   86,   87,   88,   89,    0,    0,   80,
   71,    0,  310,  299,    0,    0,   55,   59,  400,  106,
  102,  105,    0,    0,  222,  110,  113,    0,  215,  219,
  142,  153,    0,  143,  147,   77,   81,  311,  100,  314,
  101,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  170,    0,    0,    0,  119,  114,  221,    0,    0,
    0,  312,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,   96,    0,    0,  167,
  195,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  420,    0,  440,  441,  409,    0,    0,    0,    0,    0,
    0,    0,  437,    0,    0,  404,  406,  407,  408,  410,
  411,  412,  413,  414,  415,  416,  417,  418,  419,    0,
    0,    0,    0,  436,  443,  444,  459,  460,  461,  462,
  478,  479,  480,  481,  482,    0,    0,    0,    0,  313,
  315,    0,    0,    0,   92,    0,    0,    0,    0,    0,
    0,    0,  108,  176,  175,  172,  177,  178,  171,  190,
  189,  182,  183,  179,  181,  180,  184,  173,  174,  185,
  186,  192,  191,  187,  188,    0,    0,    0,    0,   95,
  122,    0,    0,    0,    0,    0,    0,  483,  508,    0,
  484,    0,    0,    0,    0,    0,    0,    0,    0,  491,
    0,    0,    0,    0,  509,    0,    0,  426,    0,  429,
    0,    0,  425,    0,  424,  401,  405,  422,  423,    0,
    0,  434,    0,    0,    0,  156,    0,  201,    0,    0,
   91,  202,    0,    0,    0,    0,  121,    0,    0,    0,
   98,   97,    0,    0,    0,    0,  132,  208,    0,  125,
  130,    0,  128,  126,  433,    0,  468,  475,    0,  467,
    0,    0,  377,    0,  487,  485,  378,    0,    0,  488,
    0,  489,    0,    0,    0,  493,    0,    0,  501,    0,
    0,    0,    0,    0,  430,    0,  421,  163,    0,    0,
    0,    0,   94,   93,    0,    0,    0,    0,    0,    0,
  104,    0,  196,  198,    0,  209,  123,  103,    0,  135,
  134,  131,    0,    0,    0,    0,    0,  486,    0,    0,
    0,    0,  507,  494,    0,  495,  498,  502,    0,  496,
  512,    0,  431,    0,    0,    0,    0,  157,    0,    0,
    0,    0,    0,  204,    0,  205,    0,    0,    0,    0,
    0,    0,    0,  133,    0,  471,    0,  476,    0,    0,
  510,    0,  447,    0,  506,  511,  463,  155,  164,    0,
    0,    0,  154,  206,  207,  203,  194,  193,    0,    0,
  168,    0,  124,  210,    0,    0,    0,    0,    0,    0,
    0,    0,  451,    0,  455,  505,    0,    0,    0,    0,
  158,    0,    0,    0,  464,    0,    0,  473,    0,  446,
    0,  458,  448,  452,    0,  456,    0,  165,    0,    0,
  199,  200,    0,    0,  477,  457,    0,  503,    0,    0,
    0,  169,  465,  166,  161,  162,
  };
  protected static  short [] yyDgoto  = {             2,
    3,  341,   27,    4,    5,    6,    7,   42,   10,    0,
   28,  108,  220,  150,  285,    0,   29,   30,   31,   32,
   33,   34,   35,   36,   14,   22,   43,   23,   24,   44,
   45,   46,  160,  109,  244,    0,    0,  340,  365,  366,
  367,  368,  369,  370,  371,  372,  373,  374,  375,  376,
  416,  326,  292,  358,  327,  328,  337,  408,  409,  410,
  338,  554,  555,  674,  476,  477,  440,  441,  377,  421,
  470,  562,  505,  563,  343,  344,  345,  382,  383,  471,
  595,  653,  762,  596,  660,  597,  664,  722,  723,  323,
  291,  355,  324,  353,  391,  392,  393,  394,  395,  396,
  397,  433,  748,  701,  782,  808,  839,  451,  586,  452,
  453,  713,  714,  245,  706,  707,  708,  454,  290,  321,
  111,  347,  348,  349,  112,  163,  232,  113,  114,  115,
  116,  117,  118,  119,  120,  121,  122,  123,  124,  125,
  126,  127,  128,  129,  130,  131,  246,  247,  309,  241,
  132,  133,  361,  414,  233,  298,  299,  442,  134,  135,
  136,  137,  138,  139,  140,  141,  142,  143,  144,  145,
  146,  147,  678,  316,  286,   63,   64,  317,  514,  455,
  515,  516,  517,  518,  519,  520,  521,  522,  523,  524,
  525,  526,  527,  528,  529,  530,  531,  532,  625,  619,
  620,  533,  534,  535,  536,  773,  801,  802,  803,  804,
  825,  805,  537,  538,  539,  540,  669,  767,  816,  670,
  671,  818,  541,  542,  543,  544,  545,  611,  685,  686,
  687,  736,  688,  737,  689,  807,  693,
  };
  protected static  short [] yySindex = {         -287,
 -349,    0, -262, -287,    0,    0,    0, -169,    0, -173,
 -200, -275, -259, -262,    0, -168,    0, -143,    0,    0,
    0, -326,    0, -130, -168, 2204,   48,    0, -259,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  -65,    0,
 8167,    0,  -74,    0,    0,    0,    0,    0, -200,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,   -5, 2204,    0,    0,    0,    0,  -34,    0,    0,
    0,  -11,    0,    0,    0,    0,    0,    0, 8778,    0,
    0,    0,    0,   41,    0,    0,    0,   50,    0,    0,
   63,    0, 8167, 8167, 8167, 8167, 8167, 8167, 8167, 8167,
 8167,    0,    0,    0,    0,    0,    0, -200,   19,    0,
    0,    0,  118,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  -35,    0,    0,  382,    0,   31,  107,  -28, -178,   33,
   62,  -39,   67,   59, -339,    0,    0,    0, -326,  103,
   36, 8778,   53,   58,   81,    0, 8167,   98, 8167,    0,
   64,    0,    0, 8778, 8778, 8167,  168,  104,  123,    0,
    0,    0,    0,    0,    0,    0,    0,    0, 8167, 8167,
 7229,   99,    0,    0,  109, 8167, 8167, 8167, 8167, 8167,
 8167, 8167, 8167, 8167, 8167, 8167, 8167, 8167, 8167, 8167,
 8167, 8167, 8167, 8778, 8778, 8167, 8167, 8167, 8167, 8167,
 8167, 8167, 8167, 8167, 8167, 8167, 8167,    0, -287,  137,
    0, -328,  177,    0,    0,    0,  -30,    0,  186, 7765,
 7229,    0,  180,  -48,   28,  187, 8167,    0, 8167,    0,
  -13, 8167, 8167,    0,  189,  190,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,   31,   31,  107,  107,  180,  180,
  -28,  -28,  -28,  -28, -178, -178,   33,   62,  -39,   67,
  185,   59, -259,    0,    0,  191,  195,  199,  163,  196,
  198,  201,    0, 8167,    0,    0,   -4,  204,  197,  206,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0, 7229, 8167,  194, 8778,  212,    0, -262,    0, -262,
  137, -168,  214,    0, 8778,  216,  201,    0,    0,  180,
    0,    0,    0,    0,    0,    0, 8778,  180, -262,  137,
    3,  215,  210,    0,    0,  143,  222,  217,    0,    0,
    0,    0,  219, -262,    0, 8778, -262,  137,    0,    0,
  227,  180,  188,    0,  229, -262,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0, -283,    0,    0, 8778,
    0, 8778,    0,  223, -262,  220,    0,    7, -168,  268,
  232, -262,    0,    0,    0,    0,    0, 2204,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  234, -262,    0,
    0, 7363,    0,    0,  162, -221,    0,    0,    0,    0,
    0,    0, -321, -316,    0,    0,    0, 8167,    0,    0,
    0,    0,  624,    0,    0,    0,    0,    0,    0,    0,
    0, -161,  237, 8778, 8778,  273,  275,  166,  241, -281,
  247,    0,  247,  254, 2766,    0,    0,    0, 8778,  173,
 -271,    0, 7497,  255, -314, -309, 8778, 8778,    0,  252,
  253, -262, 1992,  259,  248,   68,    0,    1,  257,    0,
    0, -262,  258,   11, 8778,  260, 4876,  264,  266, -263,
  270,  271, 8167,  272, 8167,  247,   12,  276,  262,  277,
    0,  278,    0,    0,    0,  262,  184,    0,    0,    0,
    0,  382,    0,  282, 2766,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  263,
  280,  209,  281,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0, -280,  293,  300,   14,    0,
    0,  247,  265,   71,    0,   75,  284,   90,  102, -262,
  226,  301,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  304, -262, 7899,  209,    0,
    0, -262, -306,   97,  313, -262, -262,    0,    0, -314,
    0,  322, 5035, 8778, 8167,  296,  312, 8167, 8167,    0,
  314, 8167,  316,  -21,    0, 8301,  195,    0,  262,    0,
 8167, 2766,    0, 7765,    0,    0,    0,    0,    0,  248,
  315,    0,  318, -262, -262,    0, -262,    0, 8167,  250,
    0,    0, -262, 8778, 8778,  324,    0,  323, 8778,  327,
    0,    0, -262,  329,  334,  -68,    0,    0,  342,    0,
    0,  346,    0,    0,    0,  340,    0,    0,  337,    0,
  341, -268,    0,  345,    0,    0,    0,  357,  361,    0,
  364,    0,  367,  247,  406,    0,  447,  448,    0, 8167,
  184, 8167,  371,  374,    0,  373,    0,    0,  375,  376,
 -262,  377,    0,    0, -172,  380, -262, -262, -266, -237,
    0, -235,    0,    0, -234,    0,    0,    0, -262,    0,
    0,    0,  -68, 8167, 8167, 5194,  431,    0, 4876, 4876,
  384, 8778,    0,    0,   17,    0,    0,    0,  247,    0,
    0, 4876,    0, 4876,  378,  385,  112,    0,  379,  247,
  247,  381,  413,    0,  468,    0,  386,  388,  390,  391,
  140,  396,  397,    0,  399,    0,  392,    0, 8167,  464,
    0,  -60,    0, -208,    0,    0,    0,    0,    0,  398,
  400,  404,    0,    0,    0,    0,    0,    0, 7229, 7229,
    0, 8778,    0,    0,  401, 5194,  403, 4876, 8167,  405,
  411,  -60,    0,  -60,    0,    0,  414, -262, -262, -262,
    0,  415,  416, -203,    0,  418,  341,    0, 4876,    0,
  408,    0,    0,    0, 2766,    0,  247,    0,  420,  410,
    0,    0,  426, 4876,    0,    0, 2766,    0,  424,  430,
  432,    0,    0,    0,    0,    0,
  };
  protected static  short [] yyRindex = {         9086,
    0,    0, 9233, 8976,    0,    0,    0,   30,    0,    0,
 2609,  -16, 9288,  703,    0,    0,    0,    0,    0,    0,
    0,   22,    0,    0,    0,   16,    0,    0, 9141,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  435,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0, 8371,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0, 1657,    0,    0,
    0,    0, 1814,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0, 2291,    0, 2927, 3513, 3647, 5353, 5755,
 6023, 6157, 6559, 6827, 7095,    0,    0,    0,   22,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0, 8829,    0,  437,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  441,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0, 9163, 9031,
    0,    0,  449,    0,    0,    0,    0,    0,    0,  444,
  441,    0, 1186,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  445,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0, 3061, 3195, 3781, 3915, 4049, 4183,
 4317, 4451, 4585, 4719, 5487, 5621, 5889, 6291, 6425, 6693,
    0, 6961, 9216,    0,    0,  452,  444,    0,    0,    0,
  453,  454,    0,    0,    0,    0,    0,    0,  455,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0, 8615,    0, -322,
  784,    0,    0,    0,    0,    0,  456,    0,    0, 1343,
    0,    0,    0,    0,    0,    0,  457, 8667,  865,  784,
 8835,    0,  120,    0,    0,    0,    0,  460,    0,    0,
    0,    0,  462, 8477,    0,  -52, 1027,  784,    0,    0,
 1500, 8689, 8453,    0,    0,  946,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0, 8615, -128,    0,  389,    0, 8759,
    0, 8529,    0,    0,    0,    0,    0, 8453,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0, 1106,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0, -267,    0,
    0,    0,    0,    0,  467,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  158,    0,
    0, 8596,    0,    0,  -64,    0,    0,    0,    0,    0,
    0,  169,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  446,    0,  446,    0,    0,    0,  393,    0,
    0, 5014,    0,    0,    0, -282, 1167,  289, 9245, 9268,
 9327,    0,    0,    0,  469,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0, 8596,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0, 8615,    0,    0,    0,
    0, 8596,    0,    0,    0,  -17,  -88,    0,    0,    0,
    0,    0,  463,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  444,    0,  395,    0,
    0,    0,    0,  444,    0,    0,    0,    0,    0, 7631,
 8033,    0,    0, 8596, 8615,    0, 8596,    0,    0,    0,
    0,    0, -113,    0,    0,    0,    0,  474,    0,    0,
    0,    0,  169,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  466,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0, 2130,    0, 1971,    0,    0,    0,
  393,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  169,    0,    0,    0,    0,    0,  496,  563,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0, 8615,    0,
    0,    0,    0,    0,  476,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0, 2448,
    0,  485,    0,  482,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  441,  441,
    0,    0,    0,    0,    0,  483,    0,    0,    0,    0,
    0,  488,    0, 3352,    0,    0,    0,  169,   42,  -38,
    0,    0,    0,    0,    0,    0,  486,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  -83,    0,    0,    0,
    0,    0,    0,    0,    0,    0,
  };
  protected static  short [] yyGindex = {            0,
  612,   -3,  551,    0,  832,    0,    0,  251,    0,    0,
    0,    8,    0,    0,  -90,    0,    0,  809, -109,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  690,
    0,    0,  -20,    0,  -25,    0,    0,    0,    0,    0,
  477, -199, -167, -164, -163, -154, -153, -151, -149,    0,
  816,    0,    0,    0,    0,  517,  521,    0,    0,  438,
  174,    0, -561, -625, -417,  261, -455,  487,    0,    0,
 -291, -506, -308, -315,    0,  471,  472,    0,    0, -304,
  205,    0,    0,  267,    0,  256,    0,  131,    0,    0,
    0,    0,    0,    0,    0,    0,  475,    0,    0,    0,
    0,    0,   51,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0, -230,    0,  153,  161,    0,    0,    0,
  580,    0,    0,  484,   57,    0, -212, -420,    0,    0,
    0, -423,    0,    0,    0,  -33,   91,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  558,  628, -126,
  111,    0,    0,    0,    0, -577,    0,    0,   26,    0,
  160,  307,  192,  310,  661,  662,  660,  664,  659,    0,
    0,  136, -608,    0,    0,    0,  814,    0,    0,    0,
   56, -510,    0, -451,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0, -514,    0,    0, -205,    0,
  285, -596,    0,    0,    0,    0,    0,    0,   82,    0,
    0,   79,    0,    0,    0,    0,    0,    0,    0,    0,
   89,    0,    0,    0,    0,    0,    0,  402,    0,  207,
    0,    0,    0,  203,  208,    0,    0,
  };
  protected static  short [] yyTable = {            13,
  300,   47,  342,  605,  627,   19,  668,  551,   11,   26,
   37,  606,  696,  703,  287,  110,  655,  352,   41,  216,
  301,  287,  473,   11,  216,   26,  287,  759,  287,   11,
  227,  508,   49,  287,  507,  602,   16,   20,  665,  694,
   25,  474,   69,  217,   70,    1,  694,   71,  556,  151,
  444,  548,   73,  646,  152,   16,   74,  419,  153,  445,
  446,  287,  287,  508,  278,   76,  420,  168,  422,    8,
  447,  287,   77,  154,  287,   16,  287,   78,  704,   16,
   12,   81,  204,   12,  431,  654,   11,  760,  667,  750,
  288,  508,   38,   82,  507,   83,   33,  456,   85,  155,
  167,  692,  457,  297,  553,  287,   89,   90,  287,  475,
   92,  697,  647,  448,  631,  765,  766,  360,  205,  170,
  171,  172,  173,  174,  175,  176,  177,  699,   47,  768,
  702,  226,  651,  229,  287,  162,  427,  475,  633,  287,
  236,  751,  480,   21,  481,  479,   18,  549,   33,  169,
  727,   16,  757,  240,  226,  607,   11,  400,  478,   11,
  250,  251,  252,  253,  254,  255,  256,  257,  258,  259,
  260,   11,   11,  821,  557,  599,   17,  206,  207,  508,
  462,  758,  507,  454,  761,   16,  463,  614,  615,  401,
  281,  454,  402,  403,  208,  691,  209,  449,  508,  668,
   33,  507,  404,  405,  226,  406,  799,  407,  162,  400,
  806,   11,   11,  220,  800,  833,  308,  308,   48,  220,
  162,  162,  261,  262,  263,  264,  264,  264,  264,  364,
  350,  264,  264,  264,  264,  264,  264,  264,  264,  264,
  264,  401,  264,  638,  402,  403,  683,  399,   33,  378,
   38,    9,  161,  127,  404,  405,  364,  406,  454,  407,
  162,  162,  305,  684,  306,  151,   39,  411,  329,  148,
  152,  650,  419,  149,  153,   40,  120,  770,  771,   26,
  120,  720,   16,   99,   67,   99,  381,  335,   76,  154,
  776,  381,  777,  618,  287,  381,   76,  302,   33,  399,
  623,  351,  508,  160,   65,  508,  508,  379,  157,  380,
  381,  185,  158,  293,  381,  155,  346,  294,  508,  700,
  508,   33,   11,  213,  129,  222,  627,   36,   36,   11,
  307,   36,   11,  159,  294,  363,  381,  234,  235,  330,
  202,  591,  203,  294,   11,  592,  820,  721,  429,   12,
  390,  419,  419,  398,  636,  159,  166,  419,  637,  265,
  266,  732,  363,   11,  178,   47,  179,  835,  351,   47,
  287,  162,  508,  303,  508,  733,   16,  269,  270,   16,
   33,  162,  843,  159,  346,  164,  439,   11,  390,   11,
  197,  198,  199,  162,  165,  508,   11,  271,  272,  273,
  274,  508,  458,  763,  507,  398,  230,  166,  231,  210,
  508,  211,  162,  508,  721,  589,  507,  590,  640,  212,
  641,  509,  589,   11,  642,  214,  775,  351,   70,  351,
  775,   71,  287,  656,  644,  657,  162,  439,  162,  215,
   11,  784,  785,  219,  287,  351,  645,   50,  780,  238,
  781,   11,   11,  509,  221,   11,   77,   11,  200,  201,
  180,   78,  181,  109,  182,  109,   11,  610,  239,  610,
   51,  223,  162,   11,   11,   11,  224,   82,  594,   83,
  512,  509,  183,   52,  184,  791,  284,  792,   53,  162,
   89,   90,   11,   54,   92,   55,   56,   57,   58,  225,
  162,  162,  120,   59,   16,   33,   60,   33,  267,  268,
  362,  506,  512,  237,   18,  162,  228,  248,  838,  275,
  276,   61,  287,  162,  162,  289,  624,  249,  181,  362,
  182,  295,  304,  313,  311,  336,  320,  312,  415,  315,
  512,  162,  296,  318,  332,  510,  322,  331,  183,  325,
  184,  333,  339,  423,  354,  424,  357,  385,  812,  813,
  384,  386,  439,  387,  388,  511,  389,  412,  432,  509,
  417,  506,  425,  434,  428,  436,  467,  510,  468,  673,
  443,  464,  677,  679,  469,  472,  681,  419,  509,  450,
  513,  547,  659,  662,  482,  677,  560,  511,  226,  561,
  552,  587,  588,  593,  617,  510,  461,  598,  603,  601,
  604,   11,  628,  673,  608,  609,  612,  465,  466,  639,
  616,  621,  513,  626,  643,  511,  622,  630,  512,  629,
  632,  252,  546,  252,  435,  252,  435,  634,  435,  705,
  558,  559,  635,  252,  647,  675,  648,  512,  649,  594,
  513,   11,   11,  252,  658,  252,   11,  666,  600,  506,
  162,  676,  589,  680,  168,  682,  741,  698,  553,  711,
  716,  712,  506,  252,  718,  252,  719,  252,  506,  252,
  657,  252,  656,  252,  724,  252,  725,  252,  726,  252,
  684,  252,  509,  510,  728,  509,  509,  747,  677,  677,
  162,  162,  729,  753,  755,  162,  730,  252,  509,  731,
  509,  732,  510,  511,  735,  739,  742,  743,  744,  746,
  745,  752,  749,  769,  772,  779,  751,  778,  783,  750,
  786,  787,  511,  788,  789,  790,  186,  793,  513,   11,
  794,  796,  798,  797,  795,  811,  841,  809,  819,  810,
  815,  512,  823,  822,  512,  512,  836,  513,  840,  827,
  831,  832,  509,  834,  509,  844,  187,  512,  188,  512,
  189,  842,  190,  673,  191,   10,  192,  672,  193,  845,
  194,  846,  195,  278,  196,  509,  280,  305,  162,  212,
  281,  509,  397,  138,   72,  490,   73,  399,  306,   11,
  509,  217,  140,  509,  747,  829,  830,   33,  402,   33,
  403,  427,  466,  428,  197,  469,  510,  709,  710,  510,
  510,  512,  715,  512,   33,  470,  449,  504,  472,  450,
  283,  474,  510,  314,  510,   15,  511,   66,  218,  511,
  511,   62,  418,  359,  512,  356,  437,  413,  162,  652,
  512,  661,  511,  764,  511,  426,  427,  717,  828,  512,
  756,  513,  512,  663,  513,  513,  435,  754,  319,  334,
  310,  430,  277,  279,  278,  282,  156,  513,  280,  513,
  837,  506,  826,  824,  817,    0,  510,   69,  510,   70,
  740,  734,   71,  506,  738,    0,  613,   73,    0,    0,
    0,   74,    0,  695,  459,  774,  511,    0,  511,  510,
   76,    0,    0,    0,    0,  510,    0,   77,    0,    0,
    0,    0,   78,    0,  510,    0,   81,  510,    0,  511,
    0,  513,    0,  513,    0,  511,    0,    0,   82,    0,
   83,    0,    0,   85,  511,    0,    0,  511,    0,    0,
    0,   89,   90,    0,  513,   92,    0,    0,  460,   33,
  513,    0,   33,    0,   33,  814,   33,    0,   33,  513,
    0,   33,  513,   33,   33,    0,   33,    0,   33,    0,
   33,    0,   33,   33,   33,   33,    0,    0,    0,   33,
    0,    0,    0,    0,   33,    0,   33,   33,   33,    0,
    0,   33,   33,   33,    0,   33,    0,   33,   33,   33,
   33,   33,   33,   33,   33,    0,   33,   33,   33,   33,
    0,   33,   33,   33,    0,    0,    0,    0,    0,    0,
   33,   33,    0,    0,   33,    0,   33,   33,    0,   33,
   12,   33,   38,   12,    0,    0,    0,   12,    0,   12,
    0,    0,   12,   33,   12,   12,    0,   12,    0,   12,
    0,   12,    0,   12,   12,   12,   12,    0,    0,    0,
   12,    0,    0,    0,    0,   12,    0,   12,   12,   12,
    0,    0,   12,   12,   12,    0,   12,    0,    0,   12,
    0,   12,   12,   12,   12,    0,    0,    0,   12,   12,
   12,    0,   12,   12,   12,    0,    0,    0,    0,    0,
    0,   12,   12,    0,    0,   12,    0,   12,   12,    0,
    0,   33,    0,    0,   33,   12,   12,    0,   33,    0,
   33,    0,    0,   33,   12,   33,   33,    0,   33,    0,
   33,    0,   33,    0,   33,   33,   33,   33,    0,    0,
    0,   33,    0,    0,    0,    0,   33,    0,   33,   33,
   33,    0,    0,   33,    0,   33,    0,   33,    0,    0,
   33,    0,   33,   33,   33,   33,    0,    0,    0,   33,
   33,   33,    0,   33,   33,   33,    0,    0,    0,    0,
    0,    0,   33,   33,    0,    0,   33,    0,   33,   33,
    0,    0,   12,    0,    0,   33,   56,    0,    0,   33,
    0,   33,    0,    0,   33,   33,   33,   33,    0,   33,
    0,   33,    0,   33,    0,   33,   33,   33,   33,    0,
    0,    0,   33,    0,    0,    0,    0,   33,    0,   33,
   33,   33,    0,    0,   33,    0,   33,    0,   33,    0,
    0,   33,    0,   33,   33,   33,   33,    0,    0,    0,
   33,   33,   33,    0,   33,   33,   33,    0,    0,    0,
    0,    0,    0,   33,   33,    0,    0,   33,    0,   33,
   33,    0,    0,   33,    0,    0,   33,   57,    0,    0,
   33,    0,   33,    0,    0,   33,   33,   33,   33,    0,
   33,    0,   33,    0,   33,    0,   33,   33,   33,   33,
    0,    0,    0,   33,    0,    0,    0,    0,   33,    0,
   33,   33,   33,    0,    0,   33,    0,   33,    0,   33,
    0,    0,   33,    0,   33,   33,   33,   33,    0,    0,
    0,   33,   33,   33,    0,   33,   33,   33,    0,    0,
    0,    0,    0,    0,   33,   33,    0,    0,   33,    0,
   33,   33,    0,    0,   33,   33,    0,    0,   78,   33,
    0,   33,    0,    0,   33,    0,   33,   33,    0,   33,
    0,   33,    0,   33,    0,   33,   33,   33,   33,    0,
    0,    0,   33,    0,    0,    0,    0,   33,    0,   33,
   33,   33,    0,    0,   33,    0,   33,    0,   33,    0,
    0,   33,    0,   33,   33,   33,   33,    0,    0,    0,
   33,   33,   33,    0,   33,   33,   33,    0,    0,    0,
    0,    0,    0,   33,   33,    0,    0,   33,    0,   33,
   33,    0,    0,    0,    0,   33,  302,   79,  302,  302,
    0,  302,    0,    0,  302,  302,    0,    0,    0,  302,
    0,    0,    0,  302,    0,    0,    0,    0,    0,  302,
    0,    0,  302,    0,    0,    0,    0,    0,    0,  302,
    0,    0,  302,    0,  302,    0,  302,  302,  302,  302,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  302,    0,  302,  302,    0,  302,    0,    0,  302,    0,
  302,    0,  302,  302,  302,  302,    0,  302,    0,    0,
    0,  320,    0,    0,   33,    0,  302,  302,    0,  302,
  302,  302,  302,  302,  302,  302,  302,  302,  302,  302,
  302,  302,  302,  302,  302,  302,  302,  302,  302,  302,
  302,  320,  302,  320,  302,  320,  302,  320,  302,  320,
  302,  320,  302,  320,  302,  320,  302,  320,  302,  320,
  302,    0,  302,    0,  302,    0,  302,    0,  302,    0,
  302,    0,  302,    0,  302,  427,  302,    0,  302,    0,
    0,    0,  302,    0,  302,    0,  302,    0,  302,    0,
  302,    0,  302,  300,  302,  300,  300,    0,  300,    0,
    0,  300,  300,    0,    0,    0,  300,    0,    0,    0,
  300,    0,    0,    0,    0,    0,  300,    0,    0,  300,
    0,    0,    0,    0,    0,    0,  300,    0,    0,  300,
    0,  300,    0,  300,  300,  300,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  300,    0,  300,
  300,    0,  300,    0,    0,  300,    0,  300,    0,  300,
  300,  300,  300,    0,  300,    0,    0,    0,    0,    0,
    0,    0,    0,  300,  300,    0,  300,  300,  300,  300,
  300,  300,  300,  300,  300,  300,  300,  300,  300,  300,
  300,  300,  300,  300,  300,  300,  300,  300,    0,  300,
    0,  300,    0,  300,    0,  300,    0,  300,    0,  300,
    0,  300,    0,  300,    0,  300,    0,  300,    0,  300,
    0,  300,    0,  300,    0,  300,    0,  300,    0,  300,
    0,  300,    0,  300,    0,  300,    0,    0,    0,  300,
    0,  300,    0,  300,    0,  300,    0,  300,    0,  300,
  309,  300,  309,  309,    0,  309,    0,    0,  309,  309,
    0,    0,    0,  309,    0,    0,    0,  309,    0,    0,
    0,    0,    0,  309,    0,    0,  309,    0,    0,    0,
    0,    0,    0,  309,    0,    0,  309,    0,  309,    0,
  309,  309,  309,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  309,    0,  309,  309,    0,  309,
    0,    0,  309,    0,  309,    0,  309,  309,  309,  309,
    0,  309,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  309,  309,  309,  309,  309,  309,  309,  309,  309,
  309,  309,  309,  309,  309,  309,  309,  309,  309,  309,
  309,  309,  309,  309,  309,    0,  309,    0,  309,    0,
  309,    0,  309,    0,  309,    0,  309,    0,  309,    0,
  309,    0,  309,    0,  309,    0,  309,    0,  309,    0,
  309,    0,  309,    0,  309,    0,  309,    0,  309,    0,
  309,    0,  309,    0,    0,    0,  309,    0,  309,    0,
  309,    0,  309,    0,  309,    0,  309,  249,  309,  249,
  249,    0,  249,    0,    0,  249,  249,    0,    0,    0,
  249,    0,    0,    0,  249,    0,    0,    0,    0,    0,
  249,    0,    0,  249,    0,    0,    0,    0,    0,    0,
  249,    0,    0,  249,    0,  249,    0,  249,  249,  249,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  249,    0,  249,  249,    0,  249,    0,    0,  249,
    0,  249,    0,  249,  249,  249,  249,    0,  249,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  249,  249,
  249,  249,  249,    0,  249,  249,  249,  249,  249,  249,
  249,  249,  249,  249,  249,  249,  249,  249,  249,  249,
  249,  249,    0,  249,    0,  249,    0,  249,    0,  249,
    0,  249,    0,  249,    0,  249,    0,  249,    0,  249,
    0,  249,    0,  249,    0,  249,    0,  249,    0,  249,
    0,  249,    0,  249,    0,  249,    0,  249,    0,  249,
    0,    0,    0,  249,    0,  249,    0,  249,    0,  249,
    0,  249,    0,  249,  320,  249,  320,  320,    0,  320,
    0,    0,  320,  320,    0,    0,    0,  320,    0,    0,
    0,  320,    0,    0,    0,    0,    0,  320,    0,    0,
  320,    0,    0,    0,    0,    0,    0,  320,    0,    0,
  320,    0,  320,    0,  320,  320,  320,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  320,    0,
  320,  320,    0,  320,    0,    0,  320,    0,  320,    0,
  320,  320,  320,  320,    0,  320,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  320,    0,  320,    0,  320,
    0,  320,  320,  320,  320,  320,  320,  320,  320,  320,
  320,  320,  320,  320,  320,  320,  320,  320,    0,    0,
    0,    0,  320,    0,  320,    0,  320,    0,  320,    0,
  320,    0,  320,    0,  320,    0,  320,    0,  320,    0,
  320,    0,  320,    0,  320,    0,  320,    0,  320,    0,
  320,    0,  320,    0,  320,    0,  320,    0,    0,    0,
  320,    0,  320,    0,  320,    0,  320,    0,  320,    0,
  320,    0,  320,  497,  497,  497,  497,  497,    0,  497,
  497,    0,  497,  497,  497,  497,    0,  497,  497,  497,
    0,    0,    0,    0,  497,  497,    0,  497,  497,  497,
  497,  497,    0,    0,  497,    0,    0,    0,  497,  497,
    0,  497,  497,  497,    0,  564,    0,    0,    0,    0,
    0,    0,    0,  497,    0,  497,    0,  497,  497,    0,
  497,    0,  497,  497,  497,  497,  497,  497,  497,  497,
  497,    0,  497,  497,    0,  497,  497,    0,    0,    0,
    0,  497,  497,    0,    0,  497,  565,    0,    0,    0,
  497,  497,  497,  497,  497,    0,    0,    0,  497,    0,
  497,    0,    0,    0,    0,  497,    0,  497,    0,    0,
    0,    0,  566,  567,  568,  569,    0,  570,  571,  572,
  573,  574,  575,  576,  577,    0,  578,    0,  579,    0,
  580,    0,  581,    0,  582,    0,  583,    0,  584,    0,
  585,    0,    0,    0,    0,    0,    0,  497,    0,  497,
    0,  497,    0,  497,    0,  497,    0,  497,    0,  497,
  497,  497,  492,  492,  492,  492,  492,    0,  492,  492,
    0,  492,  492,  492,  492,    0,  492,  492,  492,    0,
    0,    0,    0,  492,    0,    0,  492,  492,  492,  492,
  492,    0,    0,  492,    0,    0,    0,  492,  492,    0,
  492,  492,  492,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  492,    0,  492,    0,  492,  492,    0,  492,
    0,  492,  492,  492,  492,  492,  492,  492,  492,  492,
    0,  492,  492,   50,  492,  492,    0,    0,    0,    0,
  492,  492,    0,    0,  492,    0,    0,    0,    0,  492,
  492,  492,  492,  492,    0,    0,   51,  492,    0,  492,
    0,    0,    0,    0,  492,    0,  492,    0,    0,   52,
    0,    0,    0,    0,   53,    0,    0,    0,    0,   54,
    0,   55,   56,   57,   58,    0,    0,    0,    0,   59,
    0,    0,   60,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  492,   61,  492,    0,
  492,    0,  492,    0,  492,    0,  492,    0,  492,  492,
  492,  332,    0,  332,  332,    0,  332,    0,    0,  332,
  332,    0,    0,    0,  332,    0,    0,    0,  332,    0,
    0,    0,    0,    0,  332,    0,    0,  332,    0,    0,
    0,    0,    0,    0,  332,    0,    0,  332,    0,  332,
    0,  332,  332,  332,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  332,    0,  332,  332,    0,
  332,    0,    0,  332,    0,  332,    0,  332,  332,  332,
  332,    0,  332,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  332,    0,  332,  332,  332,    0,  332,  332,
  332,  332,  332,  332,  332,    0,  332,  332,  332,  332,
  332,  332,  332,  332,  332,  332,    0,  332,    0,  332,
    0,  332,    0,  332,    0,  332,    0,  332,    0,  332,
    0,  332,    0,  332,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  332,    0,  332,
    0,  332,    0,  332,    0,  332,    0,  332,    0,  332,
  445,  445,  445,  445,  445,    0,  445,  445,    0,  445,
  445,  445,  445,    0,  445,  445,    0,    0,    0,    0,
    0,  445,    0,    0,  445,  445,  445,  445,  445,    0,
    0,  445,    0,    0,    0,  445,  445,    0,  445,  445,
  445,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  445,    0,  445,    0,  445,  445,    0,  445,    0,  445,
  445,  445,  445,  445,  445,  445,  445,  445,    0,  445,
  445,    0,  445,  445,    0,    0,    0,    0,  445,  445,
    0,    0,  445,    0,    0,    0,    0,  445,  445,  445,
  445,  445,    0,    0,    0,  445,    0,  445,    0,    0,
    0,    0,  445,    0,  445,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  445,    0,  445,    0,  445,    0,
  445,    0,  445,    0,  445,    0,  445,  445,  445,  224,
    0,  224,  224,    0,  224,    0,    0,  224,  224,    0,
    0,    0,  224,    0,    0,    0,  224,    0,    0,    0,
    0,    0,  224,    0,    0,  224,    0,    0,    0,    0,
    0,    0,  224,    0,    0,  224,    0,  224,    0,  224,
  224,  224,  224,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  224,    0,  224,  224,    0,  224,    0,
    0,  224,    0,  224,    0,  224,  224,  224,  224,    0,
  224,    0,    0,    0,    0,    0,    0,    0,    0,  224,
  224,  224,  224,  224,  224,    0,  224,  224,  224,  224,
  224,  224,  224,    0,  224,  224,  224,  224,  224,    0,
    0,  224,  224,  224,    0,  224,    0,    0,    0,    0,
    0,  224,    0,  224,    0,  224,    0,  224,    0,  224,
    0,  224,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  224,    0,  224,    0,  224,
    0,  224,    0,  224,    0,  224,    0,  224,   68,   69,
  483,   70,    0,    0,   71,  484,    0,  485,  486,   73,
    0,    0,  487,   74,    0,    0,    0,    0,    0,   75,
    0,    0,   76,  488,  489,  490,  491,    0,    0,   77,
    0,    0,    0,  492,   78,    0,   79,   80,   81,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  493,    0,
   82,    0,   83,   84,    0,   85,    0,  494,   86,  495,
   87,  496,   88,   89,   90,  497,    0,   92,  498,    0,
  499,  500,    0,    0,    0,    0,  419,    0,    0,    0,
   93,    0,    0,    0,    0,  501,   94,   95,   96,   97,
    0,    0,    0,   98,    0,   99,    0,    0,    0,    0,
  100,    0,  101,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  102,    0,  103,    0,  104,    0,  105,    0,
  106,    0,  107,    0,  502,  503,  504,  336,    0,  336,
  336,    0,  336,    0,    0,  336,  336,    0,    0,    0,
  336,    0,    0,    0,  336,    0,    0,    0,    0,    0,
  336,    0,    0,  336,    0,    0,    0,    0,    0,    0,
  336,    0,    0,  336,    0,  336,    0,  336,  336,  336,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  336,    0,  336,  336,    0,  336,    0,    0,  336,
    0,  336,    0,  336,  336,  336,  336,    0,  336,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  336,    0,
  336,  336,  336,    0,  336,  336,  336,  336,  336,  336,
  336,    0,  336,  336,  336,  336,    0,    0,    0,  336,
  336,  336,    0,  336,    0,  336,    0,  336,    0,  336,
    0,  336,    0,  336,    0,  336,    0,  336,    0,  336,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  337,    0,  337,  337,    0,  337,    0,    0,  337,
  337,    0,    0,  336,  337,  336,    0,  336,  337,  336,
    0,  336,    0,  336,  337,  336,    0,  337,    0,    0,
    0,    0,    0,    0,  337,    0,    0,  337,    0,  337,
    0,  337,  337,  337,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  337,    0,  337,  337,    0,
  337,    0,    0,  337,    0,  337,    0,  337,  337,  337,
  337,    0,  337,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  337,    0,  337,  337,  337,    0,  337,  337,
  337,  337,  337,  337,  337,    0,  337,  337,  337,  337,
    0,    0,    0,  337,  337,  337,    0,  337,    0,  337,
    0,  337,    0,  337,    0,  337,    0,  337,    0,  337,
    0,  337,    0,  337,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  338,    0,  338,  338,    0,
  338,    0,    0,  338,  338,    0,    0,  337,  338,  337,
    0,  337,  338,  337,    0,  337,    0,  337,  338,  337,
    0,  338,    0,    0,    0,    0,    0,    0,  338,    0,
    0,  338,    0,  338,    0,  338,  338,  338,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  338,
    0,  338,  338,    0,  338,    0,    0,  338,    0,  338,
    0,  338,  338,  338,  338,    0,  338,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  338,    0,  338,  338,
  338,    0,  338,  338,  338,  338,  338,  338,  338,    0,
  338,  338,  338,  338,    0,    0,    0,  338,  338,  338,
    0,  338,    0,  338,    0,  338,    0,  338,    0,  338,
    0,  338,    0,  338,    0,  338,    0,  338,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  338,    0,  338,    0,  338,    0,  338,    0,  338,
    0,  338,    0,  338,  453,  453,  453,  453,    0,    0,
  453,  453,    0,  453,  453,  453,    0,    0,  453,  453,
    0,    0,    0,    0,    0,  453,    0,    0,  453,  453,
  453,  453,  453,    0,    0,  453,    0,    0,    0,  453,
  453,    0,  453,  453,  453,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  453,    0,  453,    0,  453,  453,
    0,  453,    0,  453,  453,  453,  453,  453,  453,  453,
  453,  453,    0,  453,  453,    0,  453,  453,    0,    0,
    0,    0,  453,    0,    0,    0,  453,    0,    0,    0,
    0,  453,  453,  453,  453,  453,    0,    0,    0,  453,
    0,  453,    0,    0,    0,    0,  453,    0,  453,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  453,    0,
  453,    0,  453,    0,  453,    0,  453,    0,  453,    0,
  453,  453,  453,  339,    0,  339,  339,    0,  339,    0,
    0,  339,  339,    0,    0,    0,  339,    0,    0,    0,
  339,    0,    0,    0,    0,    0,  339,    0,    0,  339,
    0,    0,    0,    0,    0,    0,  339,    0,    0,  339,
    0,  339,    0,  339,  339,  339,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  339,    0,  339,
  339,    0,  339,    0,    0,  339,    0,  339,    0,  339,
  339,  339,  339,    0,  339,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  339,    0,  339,  339,  339,    0,
  339,  339,  339,  339,    0,    0,  339,    0,  339,  339,
  339,  339,  339,    0,    0,  339,  339,  339,    0,  339,
    0,  339,    0,  339,    0,  339,    0,  339,    0,  339,
    0,  339,    0,  339,    0,  339,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  342,    0,  342,
  342,    0,  342,    0,    0,  342,  342,    0,    0,  339,
  342,  339,    0,  339,  342,  339,    0,  339,    0,  339,
  342,  339,    0,  342,    0,    0,    0,    0,    0,    0,
  342,    0,    0,  342,    0,  342,    0,  342,  342,  342,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  342,    0,  342,  342,    0,  342,    0,    0,  342,
    0,  342,    0,  342,  342,  342,  342,    0,  342,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  342,    0,
  342,  342,  342,    0,  342,  342,  342,  342,  342,  342,
  342,    0,  342,  342,  342,  342,  342,    0,    0,  342,
  342,  342,    0,  342,    0,    0,    0,    0,    0,  342,
    0,  342,    0,  342,    0,  342,    0,  342,    0,  342,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  340,    0,  340,  340,    0,  340,    0,    0,  340,
  340,    0,    0,  342,  340,  342,    0,  342,  340,  342,
    0,  342,    0,  342,  340,  342,    0,  340,    0,    0,
    0,    0,    0,    0,  340,    0,    0,  340,    0,  340,
    0,  340,  340,  340,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  340,    0,  340,  340,    0,
  340,    0,    0,  340,    0,  340,    0,  340,  340,  340,
  340,    0,  340,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  340,    0,  340,  340,  340,    0,  340,  340,
  340,  340,    0,    0,  340,    0,  340,  340,  340,  340,
  340,    0,    0,  340,  340,  340,    0,  340,    0,  340,
    0,  340,    0,  340,    0,  340,    0,  340,    0,  340,
    0,  340,    0,  340,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  341,    0,  341,  341,    0,
  341,    0,    0,  341,  341,    0,    0,  340,  341,  340,
    0,  340,  341,  340,    0,  340,    0,  340,  341,  340,
    0,  341,    0,    0,    0,    0,    0,    0,  341,    0,
    0,  341,    0,  341,    0,  341,  341,  341,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  341,
    0,  341,  341,    0,  341,    0,    0,  341,    0,  341,
    0,  341,  341,  341,  341,    0,  341,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  341,    0,  341,  341,
  341,    0,  341,  341,  341,  341,    0,    0,  341,    0,
  341,  341,  341,  341,  341,    0,    0,  341,  341,  341,
    0,  341,    0,  341,    0,  341,    0,  341,    0,  341,
    0,  341,    0,  341,    0,  341,    0,  341,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  348,
    0,  348,  348,    0,  348,    0,    0,  348,  348,    0,
    0,  341,  348,  341,    0,  341,  348,  341,    0,  341,
    0,  341,  348,  341,    0,  348,    0,    0,    0,    0,
    0,    0,  348,    0,    0,  348,    0,  348,    0,  348,
  348,  348,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  348,    0,  348,  348,    0,  348,    0,
    0,  348,    0,  348,    0,  348,  348,  348,  348,    0,
  348,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  348,    0,  348,  348,  348,    0,  348,  348,  348,  348,
  348,  348,  348,    0,  348,  348,  348,  348,  348,    0,
    0,  348,  348,  348,    0,  348,    0,    0,    0,    0,
    0,  348,    0,  348,    0,  348,    0,  348,    0,  348,
    0,  348,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  347,    0,  347,  347,    0,  347,    0,
    0,  347,  347,    0,    0,  348,  347,  348,    0,  348,
  347,  348,    0,  348,    0,  348,  347,  348,    0,  347,
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
    0,    0,    0,    0,    0,    0,    0,  343,    0,  343,
  343,    0,  343,    0,    0,  343,  343,    0,    0,  347,
  343,  347,    0,  347,  343,  347,    0,  347,    0,  347,
  343,  347,    0,  343,    0,    0,    0,    0,    0,    0,
  343,    0,    0,  343,    0,  343,    0,  343,  343,  343,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  343,    0,  343,  343,    0,  343,    0,    0,  343,
    0,  343,    0,  343,  343,  343,  343,    0,  343,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  343,    0,
  343,  343,  343,    0,  343,  343,  343,  343,  343,  343,
  343,    0,  343,  343,  343,  343,  343,    0,    0,  343,
  343,  343,    0,  343,    0,    0,    0,    0,    0,  343,
    0,  343,    0,  343,    0,  343,    0,  343,    0,  343,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  344,    0,  344,  344,    0,  344,    0,    0,  344,
  344,    0,    0,  343,  344,  343,    0,  343,  344,  343,
    0,  343,    0,  343,  344,  343,    0,  344,    0,    0,
    0,    0,    0,    0,  344,    0,    0,  344,    0,  344,
    0,  344,  344,  344,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  344,    0,  344,  344,    0,
  344,    0,    0,  344,    0,  344,    0,  344,  344,  344,
  344,    0,  344,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  344,    0,  344,  344,  344,    0,  344,  344,
  344,  344,  344,  344,  344,    0,  344,  344,  344,  344,
  344,    0,    0,  344,  344,  344,    0,  344,    0,    0,
    0,    0,    0,  344,    0,  344,    0,  344,    0,  344,
    0,  344,    0,  344,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  345,    0,  345,  345,    0,
  345,    0,    0,  345,  345,    0,    0,  344,  345,  344,
    0,  344,  345,  344,    0,  344,    0,  344,  345,  344,
    0,  345,    0,    0,    0,    0,    0,    0,  345,    0,
    0,  345,    0,  345,    0,  345,  345,  345,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  345,
    0,  345,  345,    0,  345,    0,    0,  345,    0,  345,
    0,  345,  345,  345,  345,    0,  345,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  345,    0,  345,  345,
  345,    0,  345,  345,  345,  345,  345,  345,  345,    0,
  345,  345,  345,  345,  345,    0,    0,  345,  345,  345,
    0,  345,    0,    0,    0,    0,    0,  345,    0,  345,
    0,  345,    0,  345,    0,  345,    0,  345,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  346,
    0,  346,  346,    0,  346,    0,    0,  346,  346,    0,
    0,  345,  346,  345,    0,  345,  346,  345,    0,  345,
    0,  345,  346,  345,    0,  346,    0,    0,    0,    0,
    0,    0,  346,    0,    0,  346,    0,  346,    0,  346,
  346,  346,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  346,    0,  346,  346,    0,  346,    0,
    0,  346,    0,  346,    0,  346,  346,  346,  346,    0,
  346,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  346,    0,  346,  346,  346,    0,  346,  346,  346,  346,
  346,  346,  346,    0,  346,  346,  346,  346,  346,    0,
    0,  346,  346,  346,    0,  346,    0,    0,    0,    0,
    0,  346,    0,  346,    0,  346,    0,  346,    0,  346,
    0,  346,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  346,    0,  346,    0,  346,
    0,  346,    0,  346,    0,  346,    0,  346,   68,   69,
  483,   70,    0,    0,   71,  484,    0,    0,  486,   73,
    0,    0,  487,   74,    0,    0,    0,    0,    0,   75,
    0,    0,   76,  488,  489,  490,  491,    0,    0,   77,
    0,    0,    0,  492,   78,    0,   79,   80,   81,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  493,    0,
   82,    0,   83,   84,    0,   85,    0,  494,   86,  495,
   87,  496,   88,   89,   90,  497,    0,   92,  498,    0,
    0,  500,    0,    0,    0,    0,  419,    0,    0,    0,
   93,    0,    0,    0,    0,  501,   94,   95,   96,   97,
    0,    0,    0,   98,    0,   99,    0,    0,    0,    0,
  100,    0,  101,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  102,    0,  103,    0,  104,    0,  105,    0,
  106,    0,  107,    0,   38,  503,  504,   68,   69,    0,
   70,    0,    0,   71,   72,    0,    0,    0,   73,    0,
    0,    0,   74,    0,    0,    0,    0,    0,   75,    0,
    0,   76,    0,    0,    0,    0,    0,    0,   77,    0,
    0,    0,    0,   78,    0,   79,   80,   81,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,   82,
    0,   83,   84,    0,   85,    0,   16,   86,   16,   87,
   16,   88,   89,   90,   91,    0,   92,    0,   16,  499,
    0,    0,    0,    0,    0,    0,    0,    0,   16,   93,
   16,    0,    0,    0,    0,   94,   95,   96,   97,    0,
    0,    0,   98,    0,   99,    0,    0,    0,   16,  100,
   16,  101,   16,    0,   16,    0,   16,    0,   16,    0,
   16,    0,   16,    0,   16,    0,   16,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,   16,    0,    0,    0,    0,    0,    0,    0,
    0,  102,    0,  103,    0,  104,    0,  105,    0,  106,
    0,  107,    0,   38,  503,  504,   68,   69,    0,   70,
    0,    0,   71,   72,    0,    0,    0,   73,    0,    0,
    0,   74,    0,    0,    0,    0,    0,   75,    0,    0,
   76,    0,    0,    0,    0,    0,    0,   77,    0,    0,
    0,    0,   78,    0,   79,   80,   81,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,   82,    0,
   83,   84,    0,   85,    0,    0,   86,    0,   87,    0,
   88,   89,   90,   91,    0,   92,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,   93,    0,
    0,    0,    0,    0,   94,   95,   96,   97,    0,    0,
    0,   98,    0,   99,    0,    0,    0,    0,  100,    0,
  101,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  102,    0,  103,    0,  104,    0,  105,    0,  106,    0,
  107,    0,   38,  503,  504,  349,  349,    0,  349,    0,
    0,  349,  349,    0,    0,    0,  349,    0,    0,    0,
  349,    0,    0,    0,    0,    0,  349,    0,    0,  349,
    0,    0,    0,    0,    0,    0,  349,    0,    0,    0,
    0,  349,    0,  349,  349,  349,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  349,    0,  349,
  349,    0,  349,    0,    0,  349,    0,  349,    0,  349,
  349,  349,  349,    0,  349,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  349,    0,  349,  349,  349,    0,
  349,  349,  349,  349,  349,  349,  349,    0,    0,    0,
  349,  349,  349,    0,    0,  349,  349,  349,    0,  349,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  349,
    0,  349,    0,  349,    0,  349,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  350,
  350,    0,  350,    0,    0,  350,  350,    0,    0,  349,
  350,  349,    0,  349,  350,  349,    0,  349,    0,  349,
  350,  349,    0,  350,    0,    0,    0,    0,    0,    0,
  350,    0,    0,    0,    0,  350,    0,  350,  350,  350,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  350,    0,  350,  350,    0,  350,    0,    0,  350,
    0,  350,    0,  350,  350,  350,  350,    0,  350,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  350,    0,
  350,  350,  350,    0,  350,  350,  350,  350,  350,  350,
  350,    0,    0,    0,  350,  350,  350,    0,    0,  350,
  350,  350,    0,  350,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  350,    0,  350,    0,  350,    0,  350,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  351,  351,    0,  351,    0,    0,  351,
  351,    0,    0,  350,  351,  350,    0,  350,  351,  350,
    0,  350,    0,  350,  351,  350,    0,  351,    0,    0,
    0,    0,    0,    0,  351,    0,    0,    0,    0,  351,
    0,  351,  351,  351,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  351,    0,  351,  351,    0,
  351,    0,    0,  351,    0,  351,    0,  351,  351,  351,
  351,    0,  351,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  351,    0,  351,  351,  351,    0,  351,  351,
  351,  351,  351,  351,  351,    0,    0,    0,  351,  351,
  351,    0,    0,  351,  351,  351,    0,  351,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  351,    0,  351,
    0,  351,    0,  351,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  352,  352,    0,
  352,    0,    0,  352,  352,    0,    0,  351,  352,  351,
    0,  351,  352,  351,    0,  351,    0,  351,  352,  351,
    0,  352,    0,    0,    0,    0,    0,    0,  352,    0,
    0,    0,    0,  352,    0,  352,  352,  352,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  352,
    0,  352,  352,    0,  352,    0,    0,  352,    0,  352,
    0,  352,  352,  352,  352,    0,  352,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  352,    0,  352,  352,
  352,    0,  352,  352,  352,  352,  352,  352,  352,    0,
    0,    0,  352,  352,  352,    0,    0,  352,  352,  352,
    0,  352,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  352,    0,  352,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  353,  353,    0,  353,    0,    0,  353,  353,    0,
    0,  352,  353,  352,    0,  352,  353,  352,    0,  352,
    0,  352,  353,  352,    0,  353,    0,    0,    0,    0,
    0,    0,  353,    0,    0,    0,    0,  353,    0,  353,
  353,  353,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  353,    0,  353,  353,    0,  353,    0,
    0,  353,    0,  353,    0,  353,  353,  353,  353,    0,
  353,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  353,    0,  353,  353,  353,    0,  353,  353,  353,  353,
  353,  353,  353,    0,    0,    0,  353,  353,  353,    0,
    0,  353,  353,  353,    0,  353,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  353,
    0,  353,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  354,  354,    0,  354,    0,
    0,  354,  354,    0,    0,  353,  354,  353,    0,  353,
  354,  353,    0,  353,    0,  353,  354,  353,    0,  354,
    0,    0,    0,    0,    0,    0,  354,    0,    0,    0,
    0,  354,    0,  354,  354,  354,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  354,    0,  354,
  354,    0,  354,    0,    0,  354,    0,  354,    0,  354,
  354,  354,  354,    0,  354,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  354,    0,  354,  354,  354,    0,
  354,  354,  354,  354,  354,  354,  354,    0,    0,    0,
    0,  354,  354,    0,    0,  354,  354,  354,    0,  354,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  354,    0,  354,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  356,
  356,    0,  356,    0,    0,  356,  356,    0,    0,  354,
  356,  354,    0,  354,  356,  354,    0,  354,    0,  354,
  356,  354,    0,  356,    0,    0,    0,    0,    0,    0,
  356,    0,    0,    0,    0,  356,    0,  356,  356,  356,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  356,    0,  356,  356,    0,  356,    0,    0,  356,
    0,  356,    0,  356,  356,  356,  356,    0,  356,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  356,    0,
  356,  356,  356,    0,  356,  356,  356,  356,  356,  356,
  356,    0,    0,    0,  356,  356,  356,    0,    0,    0,
  356,  356,    0,  356,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  356,    0,  356,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  355,  355,    0,  355,    0,    0,  355,
  355,    0,    0,  356,  355,  356,    0,  356,  355,  356,
    0,  356,    0,  356,  355,  356,    0,  355,    0,    0,
    0,    0,    0,    0,  355,    0,    0,    0,    0,  355,
    0,  355,  355,  355,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  355,    0,  355,  355,    0,
  355,    0,    0,  355,    0,  355,    0,  355,  355,  355,
  355,    0,  355,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  355,    0,  355,  355,  355,    0,  355,  355,
  355,  355,  355,  355,  355,    0,    0,    0,    0,  355,
  355,    0,    0,  355,  355,  355,    0,  355,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  355,    0,  355,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  357,  357,    0,
  357,    0,    0,  357,  357,    0,    0,  355,  357,  355,
    0,  355,  357,  355,    0,  355,    0,  355,  357,  355,
    0,  357,    0,    0,    0,    0,    0,    0,  357,    0,
    0,    0,    0,  357,    0,  357,  357,  357,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  357,
    0,  357,  357,    0,  357,    0,    0,  357,    0,  357,
    0,  357,  357,  357,  357,    0,  357,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  357,    0,  357,  357,
  357,    0,  357,  357,  357,  357,  357,  357,  357,    0,
    0,    0,  357,  357,  357,    0,    0,    0,  357,  357,
    0,  357,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  357,    0,  357,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  358,  358,    0,  358,    0,    0,  358,  358,    0,
    0,  357,  358,  357,    0,  357,  358,  357,    0,  357,
    0,  357,  358,  357,    0,  358,    0,    0,    0,    0,
    0,    0,  358,    0,    0,    0,    0,  358,    0,  358,
  358,  358,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  358,    0,  358,  358,    0,  358,    0,
    0,  358,    0,  358,    0,  358,  358,  358,  358,    0,
  358,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  358,    0,  358,  358,  358,    0,  358,  358,  358,  358,
  358,  358,  358,    0,    0,    0,  358,    0,  358,    0,
    0,    0,  358,  358,    0,  358,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  358,
    0,  358,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  359,  359,    0,  359,    0,
    0,  359,  359,    0,    0,  358,  359,  358,    0,  358,
  359,  358,    0,  358,    0,  358,  359,  358,    0,  359,
    0,    0,    0,    0,    0,    0,  359,    0,    0,    0,
    0,  359,    0,  359,  359,  359,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  359,    0,  359,
  359,    0,  359,    0,    0,  359,    0,  359,    0,  359,
  359,  359,  359,    0,  359,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  359,    0,  359,  359,  359,    0,
  359,  359,  359,  359,  359,  359,  359,    0,    0,    0,
  359,    0,  359,    0,    0,    0,  359,  359,    0,  359,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  359,    0,  359,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  360,
  360,    0,  360,    0,    0,  360,  360,    0,    0,  359,
  360,  359,    0,  359,  360,  359,    0,  359,    0,  359,
  360,  359,    0,  360,    0,    0,    0,    0,    0,    0,
  360,    0,    0,    0,    0,  360,    0,  360,  360,  360,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  360,    0,  360,  360,    0,  360,    0,    0,  360,
    0,  360,    0,  360,  360,  360,  360,    0,  360,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  360,    0,
  360,  360,  360,    0,  360,  360,  360,  360,  360,  360,
  360,    0,    0,    0,  360,    0,  360,    0,    0,    0,
  360,  360,    0,  360,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  360,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  361,  361,    0,  361,    0,    0,  361,
  361,    0,    0,  360,  361,  360,    0,  360,  361,  360,
    0,  360,    0,  360,  361,  360,    0,  361,    0,    0,
    0,    0,    0,    0,  361,    0,    0,    0,    0,  361,
    0,  361,  361,  361,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  361,    0,  361,  361,    0,
  361,    0,    0,  361,    0,  361,    0,  361,  361,  361,
  361,    0,  361,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  361,    0,  361,  361,  361,    0,  361,  361,
  361,  361,  361,  361,  361,    0,    0,    0,  361,    0,
  361,    0,    0,    0,  361,  361,    0,  361,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  361,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  362,  362,    0,
  362,    0,    0,  362,  362,    0,    0,  361,  362,  361,
    0,  361,  362,  361,    0,  361,    0,  361,  362,  361,
    0,  362,    0,    0,    0,    0,    0,    0,  362,    0,
    0,    0,    0,  362,    0,  362,  362,  362,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  362,
    0,  362,  362,    0,  362,    0,    0,  362,    0,  362,
    0,  362,  362,  362,  362,    0,  362,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  362,    0,  362,  362,
  362,    0,  362,  362,  362,  362,  362,  362,  362,    0,
    0,    0,  362,    0,  362,    0,    0,    0,    0,  362,
    0,  362,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,   68,   69,    0,   70,    0,    0,   71,   72,    0,
    0,  362,   73,  362,    0,  362,   74,  362,    0,  362,
    0,  362,   75,  362,    0,   76,    0,    0,    0,    0,
    0,    0,   77,    0,    0,    0,    0,   78,    0,   79,
   80,   81,    0,  242,    0,    0,    0,    0,    0,    0,
  243,    0,    0,   82,    0,   83,   84,    0,   85,    0,
    0,   86,    0,   87,    0,   88,   89,   90,   91,    0,
   92,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,   93,    0,    0,    0,    0,    0,   94,
   95,   96,   97,    0,    0,    0,   98,    0,   99,    0,
    0,    0,    0,  100,    0,  101,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,   68,   69,    0,   70,    0,
    0,   71,   72,    0,    0,  102,   73,  103,    0,  104,
   74,  105,    0,  106,    0,  107,   75,   38,    0,   76,
    0,    0,    0,    0,    0,    0,   77,    0,    0,    0,
    0,   78,    0,   79,   80,   81,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,   82,    0,   83,
   84,    0,   85,    0,    0,   86,    0,   87,    0,   88,
   89,   90,   91,    0,   92,    0,    0,    0,    0,    0,
    0,    0,    0,  412,  438,    0,    0,   93,    0,    0,
    0,    0,    0,   94,   95,   96,   97,    0,    0,    0,
   98,    0,   99,    0,    0,    0,    0,  100,    0,  101,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,   68,
   69,    0,   70,    0,    0,   71,   72,    0,    0,  102,
   73,  103,    0,  104,   74,  105,    0,  106,    0,  107,
   75,   38,    0,   76,    0,    0,    0,    0,    0,    0,
   77,    0,    0,    0,    0,   78,    0,   79,   80,   81,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,   82,    0,   83,   84,    0,   85,    0,    0,   86,
    0,   87,    0,   88,   89,   90,   91,    0,   92,    0,
    0,    0,    0,    0,    0,    0,    0,  412,  550,    0,
    0,   93,    0,    0,    0,    0,    0,   94,   95,   96,
   97,    0,    0,    0,   98,    0,   99,    0,    0,    0,
    0,  100,    0,  101,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,   99,   99,    0,   99,    0,    0,   99,
   99,    0,    0,  102,   99,  103,    0,  104,   99,  105,
    0,  106,    0,  107,   99,   38,    0,   99,    0,    0,
    0,    0,    0,    0,   99,    0,    0,    0,    0,   99,
    0,   99,   99,   99,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,   99,    0,   99,   99,    0,
   99,    0,    0,   99,    0,   99,    0,   99,   99,   99,
   99,    0,   99,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,   99,    0,    0,   99,    0,
   99,   99,   99,   99,   99,    0,    0,    0,   99,    0,
   99,    0,    0,    0,    0,   99,    0,   99,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,   68,   69,    0,
   70,    0,    0,   71,   72,    0,    0,   99,   73,   99,
    0,   99,   74,   99,    0,   99,    0,   99,   75,   99,
    0,   76,    0,    0,    0,    0,    0,    0,   77,    0,
    0,    0,    0,   78,    0,   79,   80,   81,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,   82,
    0,   83,   84,    0,   85,    0,    0,   86,    0,   87,
    0,   88,   89,   90,   91,    0,   92,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,   93,
    0,    0,  296,    0,    0,   94,   95,   96,   97,    0,
    0,    0,   98,    0,   99,    0,    0,    0,    0,  100,
    0,  101,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,   68,   69,    0,   70,    0,    0,   71,   72,    0,
    0,  102,   73,  103,    0,  104,   74,  105,    0,  106,
    0,  107,   75,   38,    0,   76,    0,    0,    0,    0,
    0,    0,   77,    0,    0,    0,    0,   78,    0,   79,
   80,   81,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,   82,    0,   83,   84,    0,   85,    0,
    0,   86,    0,   87,    0,   88,   89,   90,   91,    0,
   92,    0,    0,    0,    0,    0,    0,    0,    0,  412,
    0,    0,    0,   93,    0,    0,    0,    0,    0,   94,
   95,   96,   97,    0,    0,    0,   98,    0,   99,    0,
    0,    0,    0,  100,    0,  101,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  432,  432,    0,  432,    0,
    0,  432,  432,    0,    0,  102,  432,  103,    0,  104,
  432,  105,    0,  106,    0,  107,  432,   38,    0,  432,
    0,    0,    0,    0,    0,    0,  432,    0,    0,    0,
    0,  432,    0,  432,  432,  432,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  432,    0,  432,
  432,    0,  432,    0,    0,  432,    0,  432,    0,  432,
  432,  432,  432,    0,  432,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  432,    0,    0,
    0,    0,  432,  432,  432,  432,  432,    0,    0,    0,
  432,    0,  432,    0,    0,    0,    0,  432,    0,  432,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,   68,
   69,    0,   70,    0,    0,   71,   72,    0,    0,  432,
   73,  432,    0,  432,   74,  432,    0,  432,    0,  432,
   75,  432,    0,   76,    0,    0,    0,    0,    0,    0,
   77,    0,    0,    0,    0,   78,    0,   79,   80,   81,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,   82,    0,   83,   84,    0,   85,    0,    0,   86,
    0,   87,    0,   88,   89,   90,   91,    0,   92,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,   93,    0,    0,    0,    0,    0,   94,   95,   96,
   97,    0,    0,    0,   98,    0,   99,    0,    0,    0,
    0,  100,    0,  101,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,   68,   69,    0,   70,    0,    0,   71,
   72,    0,    0,  102,   73,  103,    0,  104,   74,  105,
    0,  106,    0,  107,   75,   38,    0,   76,    0,    0,
    0,    0,    0,    0,   77,    0,    0,    0,    0,   78,
    0,   79,   80,   81,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,   82,    0,   83,   84,    0,
   85,    0,    0,   86,    0,   87,    0,   88,   89,   90,
   91,    0,   92,    0,  382,  499,  382,    0,    0,  382,
    0,  382,  382,    0,  382,  690,  382,    0,  382,    0,
  382,  382,  382,    0,    0,    0,    0,  382,    0,    0,
    0,    0,  382,    0,  382,  382,    0,    0,    0,  382,
    0,    0,    0,  382,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  382,    0,  382,    0,    0,
  382,  382,    0,    0,    0,    0,    0,    0,  382,  382,
    0,    0,  382,    0,    0,  382,    0,  102,    0,  103,
    0,  104,    0,  105,    0,  106,  381,  107,  381,   38,
    0,  381,    0,  381,  381,    0,  381,    0,  381,    0,
  381,    0,  381,  381,  381,    0,    0,    0,    0,  381,
   33,    0,   33,    0,  381,   33,  381,  381,    0,    0,
   33,  381,    0,    0,   33,  381,    0,   33,    0,    0,
    0,    0,    0,   33,    0,    0,    0,  381,    0,  381,
   33,    0,  381,  381,    0,   33,    0,   33,    0,   33,
  381,  381,    0,    0,  381,    0,    0,  381,    0,  382,
    0,   33,   33,   33,   33,    0,   33,   33,    0,    0,
    0,    0,   33,    0,   33,   33,   33,    0,   33,   33,
    0,   33,    0,    0,    0,   33,    0,    0,  144,    0,
    0,    0,   33,    0,    0,    0,    0,   33,    0,   33,
    0,   33,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,   33,    0,   33,    0,    0,   33,    0,
    0,    0,    0,    0,    0,    0,   33,   33,    0,   33,
   33,   33,    0,   33,   33,    0,    0,    0,    0,   33,
  145,  381,    0,   33,    0,    0,    0,    0,   33,    0,
   33,    0,   33,   33,    0,    0,    0,    0,   33,   33,
    0,    0,   33,    0,   33,   33,    0,    0,   33,    0,
   33,   33,   33,    0,    0,    0,    0,   33,   33,    0,
   33,    0,   33,   33,    0,   33,    0,   33,    0,   33,
    0,   33,    0,   33,   33,    0,   33,   33,    0,   33,
  228,   33,  228,    0,   33,  228,    0,    0,    0,    0,
  228,  107,   33,   33,  228,    0,   33,   33,    0,    0,
    0,    0,  229,  228,  229,    0,    0,  229,    0,    0,
  228,    0,  229,    0,    0,  228,  229,    0,    0,  228,
    0,    0,    0,    0,    0,  229,    0,    0,    0,    0,
    0,  228,  229,  228,    0,    0,  228,  229,    0,    0,
    0,  229,    0,    0,  228,  228,    0,    0,  228,    0,
    0,    0,    0,  229,    0,  229,    0,  228,  229,    0,
    0,    0,    0,    0,   33,  228,  229,  229,    0,    0,
  229,    0,  152,    0,  152,    0,    0,  152,    0,  229,
    0,    0,  152,   33,    0,    0,  152,  229,    0,  152,
    0,   69,    0,   70,    0,  152,   71,    0,    0,    0,
    0,   73,  152,    0,    0,   74,    0,  152,    0,    0,
    0,  152,    0,    0,   76,    0,    0,    0,    0,    0,
    0,   77,    0,  152,    0,  152,   78,    0,  152,    0,
   81,    0,    0,    0,    0,  228,  152,  152,    0,  249,
  152,    0,   82,  152,   83,    0,    0,   85,  115,    0,
  115,    0,    0,  115,    0,   89,   90,  229,  115,   92,
    0,    0,  115,    0,    0,    0,    0,    0,    0,    0,
    0,  115,    0,    0,    0,  249,    0,    0,  115,    0,
    0,    0,    0,  115,    0,    0,    0,  115,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  115,
    0,  115,    0,    0,  115,    0,    0,    0,    0,    0,
    0,    0,  115,  115,    0,    0,  115,    0,    0,    0,
    0,  249,    0,  249,    0,    0,    0,  152,    0,    0,
  249,  249,    0,  249,  249,  249,  249,  249,  249,  249,
  249,  249,  249,  249,    0,  249,   38,  249,    0,  249,
    0,  249,    0,  249,    0,  249,    0,  249,    0,  249,
    0,  249,    0,  249,    0,  249,    0,  249,    0,  249,
    0,  249,    0,  249,    0,  249,    0,  249,    0,  249,
    0,  249,   21,    0,    0,   21,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,   21,    0,    0,    0,
    0,   21,    0,  115,    0,   21,    0,    0,   21,    0,
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
    0,    0,   33,   33,    0,    0,    0,  256,   33,  256,
  438,  256,  438,   33,  438,   33,   33,   33,   33,  256,
    0,    0,    0,   33,    0,    0,   33,    0,   33,  256,
  257,  256,  257,  439,  257,  439,    0,  439,    0,    0,
    0,   33,  257,    0,    0,    0,    0,    0,    0,  256,
    0,  256,  257,  256,  257,  256,    0,  256,    0,  256,
    0,  256,    0,  256,    0,  256,    0,  256,    0,    0,
    0,    0,  257,    0,  257,    0,  257,    0,  257,    0,
  257,    0,  257,  256,  257,    0,  257,    0,  257,  296,
  257,  296,  442,  296,  442,    0,  442,    0,    0,    0,
    0,  296,    0,    0,    0,    0,  257,    0,    0,    0,
    0,  296,    0,  296,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  296,    0,  296,    0,  296,    0,  296,    0,  296,
    0,  296,    0,  296,    0,  296,    0,  296,    0,  296,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  296,
  };
  protected static  short [] yyCheck = {             3,
  231,   22,  318,  267,  515,  281,  603,  463,    1,   13,
   14,  275,  621,  639,  343,   41,  323,  322,  345,  342,
  233,  343,  304,   16,  364,   29,  343,  263,  343,   22,
  157,  455,   25,  343,  455,  487,  304,  313,  600,  617,
  300,  323,  264,  383,  266,  333,  624,  269,  466,  271,
  272,  323,  274,  560,  276,  323,  278,  341,  280,  281,
  282,  343,  343,  487,  347,  287,  350,   93,  377,  419,
  292,  343,  294,  295,  343,  343,  343,  299,  640,  347,
  343,  303,  261,  343,  389,  592,   79,  323,  603,  262,
  419,  515,  419,  315,  515,  317,  419,  419,  320,  321,
   93,  616,  419,  230,  419,  343,  328,  329,  343,  419,
  332,  622,  419,  335,  532,  724,  725,  330,  297,   94,
   95,   96,   97,   98,   99,  100,  101,  634,  149,  726,
  637,  157,  588,  159,  343,   79,  419,  419,  419,  343,
  166,  314,  451,  419,  453,  450,  347,  419,  262,   93,
  419,  419,  419,  179,  180,  419,  149,  357,  450,  152,
  186,  187,  188,  189,  190,  191,  192,  193,  194,  195,
  196,  164,  165,  799,  466,  484,  350,  356,  357,  603,
  342,  419,  603,  267,  419,  355,  348,  496,  497,  357,
  216,  275,  357,  357,  373,  616,  375,  419,  622,  796,
  314,  622,  357,  357,  230,  357,  267,  357,  152,  409,
  419,  204,  205,  342,  275,  419,  242,  243,  349,  348,
  164,  165,  197,  198,  199,  200,  201,  202,  203,  339,
  321,  206,  207,  208,  209,  210,  211,  212,  213,  214,
  215,  409,  217,  552,  409,  409,  268,  357,  337,  340,
  419,    1,   79,  342,  409,  409,  366,  409,  342,  409,
  204,  205,  237,  285,  239,  271,   16,  358,  294,  344,
  276,  587,  341,  348,  280,  419,  341,  729,  730,  283,
  345,  350,  347,  348,  350,  350,  271,  313,  341,  295,
  742,  276,  744,  499,  343,  280,  349,  346,  337,  409,
  506,  322,  726,  342,  257,  729,  730,  305,  343,  307,
  295,  347,  347,  344,  312,  321,  320,  348,  742,  635,
  744,  339,  315,  363,  342,  152,  837,  344,  345,  322,
  344,  348,  325,  345,  348,  339,  321,  164,  165,  344,
  369,  341,  371,  348,  337,  345,  798,  656,  342,  343,
  354,  341,  341,  357,  341,  345,  345,  341,  345,  200,
  201,  345,  366,  356,  346,  344,  348,  819,  389,  348,
  343,  315,  796,  346,  798,  684,  347,  204,  205,  350,
  339,  325,  834,  342,  388,  345,  412,  380,  392,  382,
  360,  361,  362,  337,  345,  819,  389,  206,  207,  208,
  209,  825,  428,  719,  825,  409,  343,  345,  345,  377,
  834,  379,  356,  837,  723,  348,  837,  350,  348,  358,
  350,  455,  348,  416,  350,  359,  735,  448,  266,  450,
  739,  269,  343,  337,  345,  339,  380,  463,  382,  381,
  433,  750,  751,  341,  343,  466,  345,  260,  337,  346,
  339,  444,  445,  487,  419,  448,  294,  450,  352,  353,
  343,  299,  345,  344,  347,  346,  459,  493,  346,  495,
  283,  419,  416,  466,  467,  468,  419,  315,  482,  317,
  455,  515,  365,  296,  367,  346,  350,  348,  301,  433,
  328,  329,  485,  306,  332,  308,  309,  310,  311,  419,
  444,  445,  345,  316,  347,  337,  319,  339,  202,  203,
  337,  455,  487,  346,  347,  459,  419,  419,  827,  210,
  211,  334,  343,  467,  468,  349,  343,  419,  345,  356,
  347,  346,  346,  349,  346,  342,  341,  348,  351,  349,
  515,  485,  348,  345,  348,  455,  349,  344,  365,  349,
  367,  346,  341,  380,  341,  382,  341,  348,  789,  790,
  346,  419,  588,  342,  348,  455,  348,  341,  301,  603,
  342,  515,  350,  342,  355,  342,  304,  487,  304,  605,
  419,  345,  608,  609,  419,  345,  612,  341,  622,  416,
  455,  419,  596,  597,  341,  621,  345,  487,  624,  347,
  346,  343,  355,  347,  343,  515,  433,  350,  345,  350,
  345,  604,  350,  639,  345,  345,  345,  444,  445,  355,
  345,  345,  487,  342,  341,  515,  349,  419,  603,  350,
  350,  343,  459,  345,  346,  347,  348,  345,  350,  643,
  467,  468,  343,  355,  419,  350,  346,  622,  345,  653,
  515,  644,  645,  365,  342,  367,  649,  336,  485,  603,
  604,  350,  348,  350,  690,  350,  692,  350,  419,  346,
  344,  349,  616,  385,  346,  387,  343,  389,  622,  391,
  339,  393,  337,  395,  345,  397,  350,  399,  348,  401,
  285,  403,  726,  603,  350,  729,  730,  701,  724,  725,
  644,  645,  346,  707,  708,  649,  346,  419,  742,  346,
  744,  345,  622,  603,  268,  268,  346,  344,  346,  344,
  346,  342,  346,  293,  341,  341,  314,  350,  350,  262,
  350,  346,  622,  346,  345,  345,  355,  342,  603,  732,
  344,  350,  279,  769,  346,  342,  337,  350,  346,  350,
  350,  726,  342,  349,  729,  730,  349,  622,  339,  346,
  346,  346,  796,  346,  798,  342,  385,  742,  387,  744,
  389,  346,  391,  799,  393,  341,  395,  604,  397,  350,
  399,  350,  401,  347,  403,  819,  346,  344,  732,  341,
  346,  825,  341,  341,  341,  350,  341,  341,  344,  792,
  834,  342,  341,  837,  808,  809,  810,  419,  342,  314,
  342,  419,  350,  419,  341,  350,  726,  644,  645,  729,
  730,  796,  649,  798,  262,  350,  342,  346,  346,  342,
  219,  346,  742,  283,  744,    4,  726,   29,  149,  729,
  730,   26,  366,  327,  819,  325,  409,  361,  792,  589,
  825,  596,  742,  723,  744,  385,  385,  653,  808,  834,
  708,  726,  837,  597,  729,  730,  392,  707,  289,  312,
  243,  388,  212,  214,  213,  217,   63,  742,  215,  744,
  825,  825,  804,  802,  796,   -1,  796,  264,  798,  266,
  688,  685,  269,  837,  687,   -1,  495,  274,   -1,   -1,
   -1,  278,   -1,  619,  281,  732,  796,   -1,  798,  819,
  287,   -1,   -1,   -1,   -1,  825,   -1,  294,   -1,   -1,
   -1,   -1,  299,   -1,  834,   -1,  303,  837,   -1,  819,
   -1,  796,   -1,  798,   -1,  825,   -1,   -1,  315,   -1,
  317,   -1,   -1,  320,  834,   -1,   -1,  837,   -1,   -1,
   -1,  328,  329,   -1,  819,  332,   -1,   -1,  335,  257,
  825,   -1,  260,   -1,  262,  792,  264,   -1,  266,  834,
   -1,  269,  837,  271,  272,   -1,  274,   -1,  276,   -1,
  278,   -1,  280,  281,  282,  283,   -1,   -1,   -1,  287,
   -1,   -1,   -1,   -1,  292,   -1,  294,  295,  296,   -1,
   -1,  299,  300,  301,   -1,  303,   -1,  305,  306,  307,
  308,  309,  310,  311,  312,   -1,  314,  315,  316,  317,
   -1,  319,  320,  321,   -1,   -1,   -1,   -1,   -1,   -1,
  328,  329,   -1,   -1,  332,   -1,  334,  335,   -1,  337,
  257,  339,  419,  260,   -1,   -1,   -1,  264,   -1,  266,
   -1,   -1,  269,  351,  271,  272,   -1,  274,   -1,  276,
   -1,  278,   -1,  280,  281,  282,  283,   -1,   -1,   -1,
  287,   -1,   -1,   -1,   -1,  292,   -1,  294,  295,  296,
   -1,   -1,  299,  300,  301,   -1,  303,   -1,   -1,  306,
   -1,  308,  309,  310,  311,   -1,   -1,   -1,  315,  316,
  317,   -1,  319,  320,  321,   -1,   -1,   -1,   -1,   -1,
   -1,  328,  329,   -1,   -1,  332,   -1,  334,  335,   -1,
   -1,  419,   -1,   -1,  260,  342,  343,   -1,  264,   -1,
  266,   -1,   -1,  269,  351,  271,  272,   -1,  274,   -1,
  276,   -1,  278,   -1,  280,  281,  282,  283,   -1,   -1,
   -1,  287,   -1,   -1,   -1,   -1,  292,   -1,  294,  295,
  296,   -1,   -1,  299,   -1,  301,   -1,  303,   -1,   -1,
  306,   -1,  308,  309,  310,  311,   -1,   -1,   -1,  315,
  316,  317,   -1,  319,  320,  321,   -1,   -1,   -1,   -1,
   -1,   -1,  328,  329,   -1,   -1,  332,   -1,  334,  335,
   -1,   -1,  419,   -1,   -1,  260,  342,   -1,   -1,  264,
   -1,  266,   -1,   -1,  269,  351,  271,  272,   -1,  274,
   -1,  276,   -1,  278,   -1,  280,  281,  282,  283,   -1,
   -1,   -1,  287,   -1,   -1,   -1,   -1,  292,   -1,  294,
  295,  296,   -1,   -1,  299,   -1,  301,   -1,  303,   -1,
   -1,  306,   -1,  308,  309,  310,  311,   -1,   -1,   -1,
  315,  316,  317,   -1,  319,  320,  321,   -1,   -1,   -1,
   -1,   -1,   -1,  328,  329,   -1,   -1,  332,   -1,  334,
  335,   -1,   -1,  419,   -1,   -1,  260,  342,   -1,   -1,
  264,   -1,  266,   -1,   -1,  269,  351,  271,  272,   -1,
  274,   -1,  276,   -1,  278,   -1,  280,  281,  282,  283,
   -1,   -1,   -1,  287,   -1,   -1,   -1,   -1,  292,   -1,
  294,  295,  296,   -1,   -1,  299,   -1,  301,   -1,  303,
   -1,   -1,  306,   -1,  308,  309,  310,  311,   -1,   -1,
   -1,  315,  316,  317,   -1,  319,  320,  321,   -1,   -1,
   -1,   -1,   -1,   -1,  328,  329,   -1,   -1,  332,   -1,
  334,  335,   -1,   -1,  419,  260,   -1,   -1,  342,  264,
   -1,  266,   -1,   -1,  269,   -1,  271,  272,   -1,  274,
   -1,  276,   -1,  278,   -1,  280,  281,  282,  283,   -1,
   -1,   -1,  287,   -1,   -1,   -1,   -1,  292,   -1,  294,
  295,  296,   -1,   -1,  299,   -1,  301,   -1,  303,   -1,
   -1,  306,   -1,  308,  309,  310,  311,   -1,   -1,   -1,
  315,  316,  317,   -1,  319,  320,  321,   -1,   -1,   -1,
   -1,   -1,   -1,  328,  329,   -1,   -1,  332,   -1,  334,
  335,   -1,   -1,   -1,   -1,  419,  261,  342,  263,  264,
   -1,  266,   -1,   -1,  269,  270,   -1,   -1,   -1,  274,
   -1,   -1,   -1,  278,   -1,   -1,   -1,   -1,   -1,  284,
   -1,   -1,  287,   -1,   -1,   -1,   -1,   -1,   -1,  294,
   -1,   -1,  297,   -1,  299,   -1,  301,  302,  303,  304,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  315,   -1,  317,  318,   -1,  320,   -1,   -1,  323,   -1,
  325,   -1,  327,  328,  329,  330,   -1,  332,   -1,   -1,
   -1,  355,   -1,   -1,  419,   -1,  341,  342,   -1,  344,
  345,  346,  347,  348,  349,  350,  351,  352,  353,  354,
  355,  356,  357,  358,  359,  360,  361,  362,  363,  364,
  365,  385,  367,  387,  369,  389,  371,  391,  373,  393,
  375,  395,  377,  397,  379,  399,  381,  401,  383,  403,
  385,   -1,  387,   -1,  389,   -1,  391,   -1,  393,   -1,
  395,   -1,  397,   -1,  399,  419,  401,   -1,  403,   -1,
   -1,   -1,  407,   -1,  409,   -1,  411,   -1,  413,   -1,
  415,   -1,  417,  261,  419,  263,  264,   -1,  266,   -1,
   -1,  269,  270,   -1,   -1,   -1,  274,   -1,   -1,   -1,
  278,   -1,   -1,   -1,   -1,   -1,  284,   -1,   -1,  287,
   -1,   -1,   -1,   -1,   -1,   -1,  294,   -1,   -1,  297,
   -1,  299,   -1,  301,  302,  303,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  315,   -1,  317,
  318,   -1,  320,   -1,   -1,  323,   -1,  325,   -1,  327,
  328,  329,  330,   -1,  332,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  341,  342,   -1,  344,  345,  346,  347,
  348,  349,  350,  351,  352,  353,  354,  355,  356,  357,
  358,  359,  360,  361,  362,  363,  364,  365,   -1,  367,
   -1,  369,   -1,  371,   -1,  373,   -1,  375,   -1,  377,
   -1,  379,   -1,  381,   -1,  383,   -1,  385,   -1,  387,
   -1,  389,   -1,  391,   -1,  393,   -1,  395,   -1,  397,
   -1,  399,   -1,  401,   -1,  403,   -1,   -1,   -1,  407,
   -1,  409,   -1,  411,   -1,  413,   -1,  415,   -1,  417,
  261,  419,  263,  264,   -1,  266,   -1,   -1,  269,  270,
   -1,   -1,   -1,  274,   -1,   -1,   -1,  278,   -1,   -1,
   -1,   -1,   -1,  284,   -1,   -1,  287,   -1,   -1,   -1,
   -1,   -1,   -1,  294,   -1,   -1,  297,   -1,  299,   -1,
  301,  302,  303,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  315,   -1,  317,  318,   -1,  320,
   -1,   -1,  323,   -1,  325,   -1,  327,  328,  329,  330,
   -1,  332,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  342,  343,  344,  345,  346,  347,  348,  349,  350,
  351,  352,  353,  354,  355,  356,  357,  358,  359,  360,
  361,  362,  363,  364,  365,   -1,  367,   -1,  369,   -1,
  371,   -1,  373,   -1,  375,   -1,  377,   -1,  379,   -1,
  381,   -1,  383,   -1,  385,   -1,  387,   -1,  389,   -1,
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
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  342,  343,
  344,  345,  346,   -1,  348,  349,  350,  351,  352,  353,
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
   -1,   -1,   -1,   -1,   -1,  342,   -1,  344,   -1,  346,
   -1,  348,  349,  350,  351,  352,  353,  354,  355,  356,
  357,  358,  359,  360,  361,  362,  363,  364,   -1,   -1,
   -1,   -1,  369,   -1,  371,   -1,  373,   -1,  375,   -1,
  377,   -1,  379,   -1,  381,   -1,  383,   -1,  385,   -1,
  387,   -1,  389,   -1,  391,   -1,  393,   -1,  395,   -1,
  397,   -1,  399,   -1,  401,   -1,  403,   -1,   -1,   -1,
  407,   -1,  409,   -1,  411,   -1,  413,   -1,  415,   -1,
  417,   -1,  419,  263,  264,  265,  266,  267,   -1,  269,
  270,   -1,  272,  273,  274,  275,   -1,  277,  278,  279,
   -1,   -1,   -1,   -1,  284,  285,   -1,  287,  288,  289,
  290,  291,   -1,   -1,  294,   -1,   -1,   -1,  298,  299,
   -1,  301,  302,  303,   -1,  284,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  313,   -1,  315,   -1,  317,  318,   -1,
  320,   -1,  322,  323,  324,  325,  326,  327,  328,  329,
  330,   -1,  332,  333,   -1,  335,  336,   -1,   -1,   -1,
   -1,  341,  342,   -1,   -1,  345,  325,   -1,   -1,   -1,
  350,  351,  352,  353,  354,   -1,   -1,   -1,  358,   -1,
  360,   -1,   -1,   -1,   -1,  365,   -1,  367,   -1,   -1,
   -1,   -1,  351,  352,  353,  354,   -1,  356,  357,  358,
  359,  360,  361,  362,  363,   -1,  365,   -1,  367,   -1,
  369,   -1,  371,   -1,  373,   -1,  375,   -1,  377,   -1,
  379,   -1,   -1,   -1,   -1,   -1,   -1,  407,   -1,  409,
   -1,  411,   -1,  413,   -1,  415,   -1,  417,   -1,  419,
  420,  421,  263,  264,  265,  266,  267,   -1,  269,  270,
   -1,  272,  273,  274,  275,   -1,  277,  278,  279,   -1,
   -1,   -1,   -1,  284,   -1,   -1,  287,  288,  289,  290,
  291,   -1,   -1,  294,   -1,   -1,   -1,  298,  299,   -1,
  301,  302,  303,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  313,   -1,  315,   -1,  317,  318,   -1,  320,
   -1,  322,  323,  324,  325,  326,  327,  328,  329,  330,
   -1,  332,  333,  260,  335,  336,   -1,   -1,   -1,   -1,
  341,  342,   -1,   -1,  345,   -1,   -1,   -1,   -1,  350,
  351,  352,  353,  354,   -1,   -1,  283,  358,   -1,  360,
   -1,   -1,   -1,   -1,  365,   -1,  367,   -1,   -1,  296,
   -1,   -1,   -1,   -1,  301,   -1,   -1,   -1,   -1,  306,
   -1,  308,  309,  310,  311,   -1,   -1,   -1,   -1,  316,
   -1,   -1,  319,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  407,  334,  409,   -1,
  411,   -1,  413,   -1,  415,   -1,  417,   -1,  419,  420,
  421,  261,   -1,  263,  264,   -1,  266,   -1,   -1,  269,
  270,   -1,   -1,   -1,  274,   -1,   -1,   -1,  278,   -1,
   -1,   -1,   -1,   -1,  284,   -1,   -1,  287,   -1,   -1,
   -1,   -1,   -1,   -1,  294,   -1,   -1,  297,   -1,  299,
   -1,  301,  302,  303,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  315,   -1,  317,  318,   -1,
  320,   -1,   -1,  323,   -1,  325,   -1,  327,  328,  329,
  330,   -1,  332,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  342,   -1,  344,  345,  346,   -1,  348,  349,
  350,  351,  352,  353,  354,   -1,  356,  357,  358,  359,
  360,  361,  362,  363,  364,  365,   -1,  367,   -1,  369,
   -1,  371,   -1,  373,   -1,  375,   -1,  377,   -1,  379,
   -1,  381,   -1,  383,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  407,   -1,  409,
   -1,  411,   -1,  413,   -1,  415,   -1,  417,   -1,  419,
  263,  264,  265,  266,  267,   -1,  269,  270,   -1,  272,
  273,  274,  275,   -1,  277,  278,   -1,   -1,   -1,   -1,
   -1,  284,   -1,   -1,  287,  288,  289,  290,  291,   -1,
   -1,  294,   -1,   -1,   -1,  298,  299,   -1,  301,  302,
  303,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  313,   -1,  315,   -1,  317,  318,   -1,  320,   -1,  322,
  323,  324,  325,  326,  327,  328,  329,  330,   -1,  332,
  333,   -1,  335,  336,   -1,   -1,   -1,   -1,  341,  342,
   -1,   -1,  345,   -1,   -1,   -1,   -1,  350,  351,  352,
  353,  354,   -1,   -1,   -1,  358,   -1,  360,   -1,   -1,
   -1,   -1,  365,   -1,  367,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  407,   -1,  409,   -1,  411,   -1,
  413,   -1,  415,   -1,  417,   -1,  419,  420,  421,  261,
   -1,  263,  264,   -1,  266,   -1,   -1,  269,  270,   -1,
   -1,   -1,  274,   -1,   -1,   -1,  278,   -1,   -1,   -1,
   -1,   -1,  284,   -1,   -1,  287,   -1,   -1,   -1,   -1,
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
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  407,   -1,  409,   -1,  411,
   -1,  413,   -1,  415,   -1,  417,   -1,  419,  263,  264,
  265,  266,   -1,   -1,  269,  270,   -1,  272,  273,  274,
   -1,   -1,  277,  278,   -1,   -1,   -1,   -1,   -1,  284,
   -1,   -1,  287,  288,  289,  290,  291,   -1,   -1,  294,
   -1,   -1,   -1,  298,  299,   -1,  301,  302,  303,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  313,   -1,
  315,   -1,  317,  318,   -1,  320,   -1,  322,  323,  324,
  325,  326,  327,  328,  329,  330,   -1,  332,  333,   -1,
  335,  336,   -1,   -1,   -1,   -1,  341,   -1,   -1,   -1,
  345,   -1,   -1,   -1,   -1,  350,  351,  352,  353,  354,
   -1,   -1,   -1,  358,   -1,  360,   -1,   -1,   -1,   -1,
  365,   -1,  367,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  407,   -1,  409,   -1,  411,   -1,  413,   -1,
  415,   -1,  417,   -1,  419,  420,  421,  261,   -1,  263,
  264,   -1,  266,   -1,   -1,  269,  270,   -1,   -1,   -1,
  274,   -1,   -1,   -1,  278,   -1,   -1,   -1,   -1,   -1,
  284,   -1,   -1,  287,   -1,   -1,   -1,   -1,   -1,   -1,
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
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  407,   -1,  409,   -1,  411,   -1,  413,   -1,  415,
   -1,  417,   -1,  419,  263,  264,  265,  266,   -1,   -1,
  269,  270,   -1,  272,  273,  274,   -1,   -1,  277,  278,
   -1,   -1,   -1,   -1,   -1,  284,   -1,   -1,  287,  288,
  289,  290,  291,   -1,   -1,  294,   -1,   -1,   -1,  298,
  299,   -1,  301,  302,  303,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  313,   -1,  315,   -1,  317,  318,
   -1,  320,   -1,  322,  323,  324,  325,  326,  327,  328,
  329,  330,   -1,  332,  333,   -1,  335,  336,   -1,   -1,
   -1,   -1,  341,   -1,   -1,   -1,  345,   -1,   -1,   -1,
   -1,  350,  351,  352,  353,  354,   -1,   -1,   -1,  358,
   -1,  360,   -1,   -1,   -1,   -1,  365,   -1,  367,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  407,   -1,
  409,   -1,  411,   -1,  413,   -1,  415,   -1,  417,   -1,
  419,  420,  421,  261,   -1,  263,  264,   -1,  266,   -1,
   -1,  269,  270,   -1,   -1,   -1,  274,   -1,   -1,   -1,
  278,   -1,   -1,   -1,   -1,   -1,  284,   -1,   -1,  287,
   -1,   -1,   -1,   -1,   -1,   -1,  294,   -1,   -1,  297,
   -1,  299,   -1,  301,  302,  303,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  315,   -1,  317,
  318,   -1,  320,   -1,   -1,  323,   -1,  325,   -1,  327,
  328,  329,  330,   -1,  332,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  342,   -1,  344,  345,  346,   -1,
  348,  349,  350,  351,   -1,   -1,  354,   -1,  356,  357,
  358,  359,  360,   -1,   -1,  363,  364,  365,   -1,  367,
   -1,  369,   -1,  371,   -1,  373,   -1,  375,   -1,  377,
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
  346,   -1,  348,  349,  350,  351,   -1,   -1,  354,   -1,
  356,  357,  358,  359,  360,   -1,   -1,  363,  364,  365,
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
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  407,   -1,  409,   -1,  411,
   -1,  413,   -1,  415,   -1,  417,   -1,  419,  263,  264,
  265,  266,   -1,   -1,  269,  270,   -1,   -1,  273,  274,
   -1,   -1,  277,  278,   -1,   -1,   -1,   -1,   -1,  284,
   -1,   -1,  287,  288,  289,  290,  291,   -1,   -1,  294,
   -1,   -1,   -1,  298,  299,   -1,  301,  302,  303,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  313,   -1,
  315,   -1,  317,  318,   -1,  320,   -1,  322,  323,  324,
  325,  326,  327,  328,  329,  330,   -1,  332,  333,   -1,
   -1,  336,   -1,   -1,   -1,   -1,  341,   -1,   -1,   -1,
  345,   -1,   -1,   -1,   -1,  350,  351,  352,  353,  354,
   -1,   -1,   -1,  358,   -1,  360,   -1,   -1,   -1,   -1,
  365,   -1,  367,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  407,   -1,  409,   -1,  411,   -1,  413,   -1,
  415,   -1,  417,   -1,  419,  420,  421,  263,  264,   -1,
  266,   -1,   -1,  269,  270,   -1,   -1,   -1,  274,   -1,
   -1,   -1,  278,   -1,   -1,   -1,   -1,   -1,  284,   -1,
   -1,  287,   -1,   -1,   -1,   -1,   -1,   -1,  294,   -1,
   -1,   -1,   -1,  299,   -1,  301,  302,  303,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  315,
   -1,  317,  318,   -1,  320,   -1,  343,  323,  345,  325,
  347,  327,  328,  329,  330,   -1,  332,   -1,  355,  335,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  365,  345,
  367,   -1,   -1,   -1,   -1,  351,  352,  353,  354,   -1,
   -1,   -1,  358,   -1,  360,   -1,   -1,   -1,  385,  365,
  387,  367,  389,   -1,  391,   -1,  393,   -1,  395,   -1,
  397,   -1,  399,   -1,  401,   -1,  403,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  419,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  407,   -1,  409,   -1,  411,   -1,  413,   -1,  415,
   -1,  417,   -1,  419,  420,  421,  263,  264,   -1,  266,
   -1,   -1,  269,  270,   -1,   -1,   -1,  274,   -1,   -1,
   -1,  278,   -1,   -1,   -1,   -1,   -1,  284,   -1,   -1,
  287,   -1,   -1,   -1,   -1,   -1,   -1,  294,   -1,   -1,
   -1,   -1,  299,   -1,  301,  302,  303,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  315,   -1,
  317,  318,   -1,  320,   -1,   -1,  323,   -1,  325,   -1,
  327,  328,  329,  330,   -1,  332,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  345,   -1,
   -1,   -1,   -1,   -1,  351,  352,  353,  354,   -1,   -1,
   -1,  358,   -1,  360,   -1,   -1,   -1,   -1,  365,   -1,
  367,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  407,   -1,  409,   -1,  411,   -1,  413,   -1,  415,   -1,
  417,   -1,  419,  420,  421,  263,  264,   -1,  266,   -1,
   -1,  269,  270,   -1,   -1,   -1,  274,   -1,   -1,   -1,
  278,   -1,   -1,   -1,   -1,   -1,  284,   -1,   -1,  287,
   -1,   -1,   -1,   -1,   -1,   -1,  294,   -1,   -1,   -1,
   -1,  299,   -1,  301,  302,  303,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  315,   -1,  317,
  318,   -1,  320,   -1,   -1,  323,   -1,  325,   -1,  327,
  328,  329,  330,   -1,  332,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  342,   -1,  344,  345,  346,   -1,
  348,  349,  350,  351,  352,  353,  354,   -1,   -1,   -1,
  358,  359,  360,   -1,   -1,  363,  364,  365,   -1,  367,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  377,
   -1,  379,   -1,  381,   -1,  383,   -1,   -1,   -1,   -1,
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
   -1,   -1,   -1,  377,   -1,  379,   -1,  381,   -1,  383,
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
  352,  353,  354,   -1,   -1,   -1,  358,  359,  360,   -1,
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
   -1,  359,  360,   -1,   -1,  363,  364,  365,   -1,  367,
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
  354,   -1,   -1,   -1,  358,  359,  360,   -1,   -1,   -1,
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
  352,  353,  354,   -1,   -1,   -1,  358,   -1,  360,   -1,
   -1,   -1,  364,  365,   -1,  367,   -1,   -1,   -1,   -1,
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
  358,   -1,  360,   -1,   -1,   -1,  364,  365,   -1,  367,
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
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  383,
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
   -1,   -1,   -1,  383,   -1,   -1,   -1,   -1,   -1,   -1,
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
   -1,   -1,  358,   -1,  360,   -1,   -1,   -1,   -1,  365,
   -1,  367,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  263,  264,   -1,  266,   -1,   -1,  269,  270,   -1,
   -1,  407,  274,  409,   -1,  411,  278,  413,   -1,  415,
   -1,  417,  284,  419,   -1,  287,   -1,   -1,   -1,   -1,
   -1,   -1,  294,   -1,   -1,   -1,   -1,  299,   -1,  301,
  302,  303,   -1,  305,   -1,   -1,   -1,   -1,   -1,   -1,
  312,   -1,   -1,  315,   -1,  317,  318,   -1,  320,   -1,
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
  328,  329,  330,   -1,  332,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  341,  342,   -1,   -1,  345,   -1,   -1,
   -1,   -1,   -1,  351,  352,  353,  354,   -1,   -1,   -1,
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
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  341,  342,   -1,
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
   -1,   -1,   -1,   -1,   -1,  345,   -1,   -1,  348,   -1,
  350,  351,  352,  353,  354,   -1,   -1,   -1,  358,   -1,
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
   -1,   -1,  348,   -1,   -1,  351,  352,  353,  354,   -1,
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
  332,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  341,
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
  328,  329,  330,   -1,  332,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  345,   -1,   -1,
   -1,   -1,  350,  351,  352,  353,  354,   -1,   -1,   -1,
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
  330,   -1,  332,   -1,  264,  335,  266,   -1,   -1,  269,
   -1,  271,  272,   -1,  274,  345,  276,   -1,  278,   -1,
  280,  281,  282,   -1,   -1,   -1,   -1,  287,   -1,   -1,
   -1,   -1,  292,   -1,  294,  295,   -1,   -1,   -1,  299,
   -1,   -1,   -1,  303,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  315,   -1,  317,   -1,   -1,
  320,  321,   -1,   -1,   -1,   -1,   -1,   -1,  328,  329,
   -1,   -1,  332,   -1,   -1,  335,   -1,  407,   -1,  409,
   -1,  411,   -1,  413,   -1,  415,  264,  417,  266,  419,
   -1,  269,   -1,  271,  272,   -1,  274,   -1,  276,   -1,
  278,   -1,  280,  281,  282,   -1,   -1,   -1,   -1,  287,
  264,   -1,  266,   -1,  292,  269,  294,  295,   -1,   -1,
  274,  299,   -1,   -1,  278,  303,   -1,  281,   -1,   -1,
   -1,   -1,   -1,  287,   -1,   -1,   -1,  315,   -1,  317,
  294,   -1,  320,  321,   -1,  299,   -1,  301,   -1,  303,
  328,  329,   -1,   -1,  332,   -1,   -1,  335,   -1,  419,
   -1,  315,  264,  317,  266,   -1,  320,  269,   -1,   -1,
   -1,   -1,  274,   -1,  328,  329,  278,   -1,  332,  281,
   -1,  335,   -1,   -1,   -1,  287,   -1,   -1,  342,   -1,
   -1,   -1,  294,   -1,   -1,   -1,   -1,  299,   -1,  301,
   -1,  303,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  315,   -1,  317,   -1,   -1,  320,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  328,  329,   -1,  264,
  332,  266,   -1,  335,  269,   -1,   -1,   -1,   -1,  274,
  342,  419,   -1,  278,   -1,   -1,   -1,   -1,  264,   -1,
  266,   -1,  287,  269,   -1,   -1,   -1,   -1,  274,  294,
   -1,   -1,  278,   -1,  299,  419,   -1,   -1,  303,   -1,
  305,  287,  307,   -1,   -1,   -1,   -1,  312,  294,   -1,
  315,   -1,  317,  299,   -1,  320,   -1,  303,   -1,  305,
   -1,  307,   -1,  328,  329,   -1,  312,  332,   -1,  315,
  264,  317,  266,   -1,  320,  269,   -1,   -1,   -1,   -1,
  274,  346,  328,  329,  278,   -1,  332,  419,   -1,   -1,
   -1,   -1,  264,  287,  266,   -1,   -1,  269,   -1,   -1,
  294,   -1,  274,   -1,   -1,  299,  278,   -1,   -1,  303,
   -1,   -1,   -1,   -1,   -1,  287,   -1,   -1,   -1,   -1,
   -1,  315,  294,  317,   -1,   -1,  320,  299,   -1,   -1,
   -1,  303,   -1,   -1,  328,  329,   -1,   -1,  332,   -1,
   -1,   -1,   -1,  315,   -1,  317,   -1,  341,  320,   -1,
   -1,   -1,   -1,   -1,  419,  349,  328,  329,   -1,   -1,
  332,   -1,  264,   -1,  266,   -1,   -1,  269,   -1,  341,
   -1,   -1,  274,  419,   -1,   -1,  278,  349,   -1,  281,
   -1,  264,   -1,  266,   -1,  287,  269,   -1,   -1,   -1,
   -1,  274,  294,   -1,   -1,  278,   -1,  299,   -1,   -1,
   -1,  303,   -1,   -1,  287,   -1,   -1,   -1,   -1,   -1,
   -1,  294,   -1,  315,   -1,  317,  299,   -1,  320,   -1,
  303,   -1,   -1,   -1,   -1,  419,  328,  329,   -1,  261,
  332,   -1,  315,  335,  317,   -1,   -1,  320,  264,   -1,
  266,   -1,   -1,  269,   -1,  328,  329,  419,  274,  332,
   -1,   -1,  278,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  287,   -1,   -1,   -1,  297,   -1,   -1,  294,   -1,
   -1,   -1,   -1,  299,   -1,   -1,   -1,  303,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  315,
   -1,  317,   -1,   -1,  320,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  328,  329,   -1,   -1,  332,   -1,   -1,   -1,
   -1,  343,   -1,  345,   -1,   -1,   -1,  419,   -1,   -1,
  352,  353,   -1,  355,  356,  357,  358,  359,  360,  361,
  362,  363,  364,  365,   -1,  367,  419,  369,   -1,  371,
   -1,  373,   -1,  375,   -1,  377,   -1,  379,   -1,  381,
   -1,  383,   -1,  385,   -1,  387,   -1,  389,   -1,  391,
   -1,  393,   -1,  395,   -1,  397,   -1,  399,   -1,  401,
   -1,  403,  257,   -1,   -1,  260,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  271,   -1,   -1,   -1,
   -1,  276,   -1,  419,   -1,  280,   -1,   -1,  283,   -1,
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
   -1,  355,   -1,   -1,   -1,   -1,  419,   -1,   -1,   -1,
   -1,  365,   -1,  367,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  385,   -1,  387,   -1,  389,   -1,  391,   -1,  393,
   -1,  395,   -1,  397,   -1,  399,   -1,  401,   -1,  403,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  419,
  };

#line 2528 "cs-parser.jay"


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
	this.tree = tree;
	current_container = tree.Types;
	current_container.Namespace = current_namespace;

	lexer = new Tokenizer (input, name);
	type_references = tree.TypeRefManager; 
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

#line 5298 "-"
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
  public const int pre_increment_expression = 420;
  public const int pre_decrement_expression = 421;
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
