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
	using CSC;
	using CIR;

	/// <summary>
	///    The C# Parser
	/// </summary>
	public class CSharpParser {
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

		// Name of the file we are parsing
		public string name;

		// Input stream to parse from.
		public System.IO.Stream input;

#line 92 "-"

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
#line 251 "cs-parser.jay"
  {
		/* At some point check that using only comes *before* any namespaces*/
	  }
  break;
case 6:
#line 268 "cs-parser.jay"
  {
	  }
  break;
case 7:
#line 274 "cs-parser.jay"
  {
		current_namespace.Using ((string) yyVals[-1+yyTop]);
          }
  break;
case 10:
#line 285 "cs-parser.jay"
  {
		current_namespace = new Namespace (current_namespace, (string) yyVals[0+yyTop]); 
	  }
  break;
case 11:
#line 289 "cs-parser.jay"
  { 
		current_namespace = current_namespace.Parent;
	  }
  break;
case 17:
#line 306 "cs-parser.jay"
  { 
	    yyVal = ((yyVals[-2+yyTop]).ToString ()) + "." + (yyVals[0+yyTop].ToString ()); }
  break;
case 19:
#line 319 "cs-parser.jay"
  {
	  }
  break;
case 26:
#line 340 "cs-parser.jay"
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
#line 398 "cs-parser.jay"
  { 
	     /* if (Collection.Contains ($$))... FIXME*/
	     note  ("Allows: assembly, field, method, module, param, property, type"); 
	}
  break;
case 46:
#line 417 "cs-parser.jay"
  { /* reserved attribute name or identifier: 17.4 */ }
  break;
case 70:
#line 473 "cs-parser.jay"
  { 
		Struct new_struct;
		string full_struct_name = MakeName ((string) yyVals[0+yyTop]);

		new_struct = new Struct (current_container, full_struct_name, (int) yyVals[-2+yyTop]);
		current_container = new_struct;
		current_container.Namespace = current_namespace;
		tree.RecordType (full_struct_name, new_struct);
	  }
  break;
case 71:
#line 485 "cs-parser.jay"
  {
		Struct new_struct = (Struct) current_container;

		current_container = current_container.Parent;
		CheckDef (current_container.AddStruct (new_struct), new_struct.Name);
		yyVal = new_struct;
	  }
  break;
case 91:
#line 541 "cs-parser.jay"
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
#line 556 "cs-parser.jay"
  {
		ArrayList constants = new ArrayList ();
		constants.Add (yyVals[0+yyTop]);
		yyVal = constants;
	  }
  break;
case 93:
#line 562 "cs-parser.jay"
  {
		ArrayList constants = (ArrayList) yyVals[-2+yyTop];

		constants.Add (yyVals[0+yyTop]);
	  }
  break;
case 94:
#line 570 "cs-parser.jay"
  {
		yyVal = new DictionaryEntry (yyVals[-2+yyTop], yyVals[0+yyTop]);
	  }
  break;
case 95:
#line 581 "cs-parser.jay"
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
#line 597 "cs-parser.jay"
  {
		ArrayList decl = new ArrayList ();
		yyVal = decl;
		decl.Add (yyVals[0+yyTop]);
	  }
  break;
case 97:
#line 603 "cs-parser.jay"
  {
		ArrayList decls = (ArrayList) yyVals[-2+yyTop];
		decls.Add (yyVals[0+yyTop]);
		yyVal = yyVals[-2+yyTop];
	  }
  break;
case 98:
#line 612 "cs-parser.jay"
  {
		yyVal = new VariableDeclaration ((string) yyVals[-2+yyTop], yyVals[0+yyTop]);
	  }
  break;
case 99:
#line 616 "cs-parser.jay"
  {
		yyVal = new VariableDeclaration ((string) yyVals[0+yyTop], null);
	  }
  break;
case 102:
#line 629 "cs-parser.jay"
  {
		Method method = (Method) yyVals[-1+yyTop];

		method.Block = (Block) yyVals[0+yyTop];
		CheckDef (current_container.AddMethod (method), method.Name);

		current_local_parameters = null;
	  }
  break;
case 103:
#line 645 "cs-parser.jay"
  {
		Method method = new Method ((TypeRef) yyVals[-4+yyTop], (int) yyVals[-5+yyTop], (string) yyVals[-3+yyTop], (Parameters) yyVals[-1+yyTop]);

		current_local_parameters = (Parameters) yyVals[-1+yyTop];

		yyVal = method;
	  }
  break;
case 104:
#line 657 "cs-parser.jay"
  {
		Method method = new Method (type ("void"), (int) yyVals[-5+yyTop], (string) yyVals[-3+yyTop], (Parameters) yyVals[-1+yyTop]);

		current_local_parameters = (Parameters) yyVals[-1+yyTop];
		yyVal = method;
	  }
  break;
case 106:
#line 667 "cs-parser.jay"
  { yyVal = null; }
  break;
case 107:
#line 671 "cs-parser.jay"
  { yyVal = new Parameters (null, null); }
  break;
case 109:
#line 677 "cs-parser.jay"
  { 
	  	yyVal = new Parameters ((ParameterCollection) yyVals[0+yyTop], null); 
	  }
  break;
case 110:
#line 681 "cs-parser.jay"
  {
		yyVal = new Parameters ((ParameterCollection) yyVals[-2+yyTop], (Parameter) yyVals[0+yyTop]); 
	  }
  break;
case 111:
#line 685 "cs-parser.jay"
  {
		yyVal = new Parameters (null, (Parameter) yyVals[0+yyTop]);
	  }
  break;
case 112:
#line 692 "cs-parser.jay"
  {
		ParameterCollection pars = new ParameterCollection ();
		pars.Add ((Parameter) yyVals[0+yyTop]);
		yyVal = pars;
	  }
  break;
case 113:
#line 698 "cs-parser.jay"
  {
		ParameterCollection pars = (ParameterCollection) yyVals[-2+yyTop];
		pars.Add ((Parameter) yyVals[0+yyTop]);
		yyVal = yyVals[-2+yyTop];
	  }
  break;
case 114:
#line 710 "cs-parser.jay"
  {
		yyVal = new Parameter ((TypeRef) yyVals[-1+yyTop], (string) yyVals[0+yyTop], (Parameter.Modifier) yyVals[-2+yyTop]);
	  }
  break;
case 115:
#line 716 "cs-parser.jay"
  { yyVal = Parameter.Modifier.NONE; }
  break;
case 117:
#line 721 "cs-parser.jay"
  { yyVal = Parameter.Modifier.REF; }
  break;
case 118:
#line 722 "cs-parser.jay"
  { yyVal = Parameter.Modifier.OUT; }
  break;
case 119:
#line 727 "cs-parser.jay"
  { 
		yyVal = new Parameter ((TypeRef) yyVals[-1+yyTop], (string) yyVals[0+yyTop], Parameter.Modifier.PARAMS);
		note ("type must be a single-dimension array type"); 
	  }
  break;
case 120:
#line 734 "cs-parser.jay"
  { yyVal = yyVals[0+yyTop].ToString (); }
  break;
case 121:
#line 735 "cs-parser.jay"
  { yyVal = yyVals[-2+yyTop].ToString () + "." + yyVals[0+yyTop].ToString (); }
  break;
case 122:
#line 743 "cs-parser.jay"
  {
		Parameter implicit_value_parameter;
		implicit_value_parameter = new Parameter ((TypeRef) yyVals[-2+yyTop], "value", Parameter.Modifier.NONE);

		lexer.properties = true;
		
		implicit_value_parameters = new ParameterCollection ();
		implicit_value_parameters.Add (implicit_value_parameter);
	  }
  break;
case 123:
#line 753 "cs-parser.jay"
  {
		lexer.properties = false;
	  }
  break;
case 124:
#line 757 "cs-parser.jay"
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
#line 777 "cs-parser.jay"
  { 
		yyVal = new DictionaryEntry (yyVals[-1+yyTop], yyVals[0+yyTop]);
	  }
  break;
case 126:
#line 781 "cs-parser.jay"
  {
		yyVal = new DictionaryEntry (yyVals[0+yyTop], yyVals[-1+yyTop]);
	  }
  break;
case 127:
#line 787 "cs-parser.jay"
  { yyVal = null; }
  break;
case 129:
#line 792 "cs-parser.jay"
  { yyVal = null; }
  break;
case 131:
#line 798 "cs-parser.jay"
  {
		yyVal = yyVals[0+yyTop];
	  }
  break;
case 132:
#line 805 "cs-parser.jay"
  { 
		current_local_parameters = new Parameters (implicit_value_parameters, null);
	  }
  break;
case 133:
#line 809 "cs-parser.jay"
  {
		yyVal = yyVals[0+yyTop];
		current_local_parameters = null;
	  }
  break;
case 135:
#line 817 "cs-parser.jay"
  { yyVal = new Block (null); }
  break;
case 136:
#line 824 "cs-parser.jay"
  {
		Interface new_interface;
		string full_interface_name = MakeName ((string) yyVals[0+yyTop]);

		new_interface = new Interface (current_container, full_interface_name, (int) yyVals[-2+yyTop]);
		if (current_interface != null)
			error (-2, "Internal compiler error: interface inside interface");
		current_interface = new_interface;
		tree.RecordType (full_interface_name, new_interface);
	  }
  break;
case 137:
#line 836 "cs-parser.jay"
  { 
		Interface new_interface = (Interface) current_interface;

		if (yyVals[-1+yyTop] != null)
			new_interface.Bases = (ArrayList) yyVals[-1+yyTop];

		current_interface = null;
		CheckDef (current_container.AddInterface (new_interface), new_interface.Name);
	  }
  break;
case 138:
#line 848 "cs-parser.jay"
  { yyVal = null; }
  break;
case 140:
#line 853 "cs-parser.jay"
  { yyVal = yyVals[0+yyTop]; }
  break;
case 141:
#line 858 "cs-parser.jay"
  {
		ArrayList interfaces = new ArrayList ();

		interfaces.Add (yyVals[0+yyTop]);
	  }
  break;
case 142:
#line 864 "cs-parser.jay"
  {
		ArrayList interfaces = (ArrayList) yyVals[-2+yyTop];
		interfaces.Add (yyVals[0+yyTop]);
		yyVal = interfaces;
	  }
  break;
case 148:
#line 889 "cs-parser.jay"
  { 
		InterfaceMethod m = (InterfaceMethod) yyVals[0+yyTop];

		CheckDef (current_interface.AddMethod (m), m.Name);
	  }
  break;
case 149:
#line 895 "cs-parser.jay"
  { 
		InterfaceProperty p = (InterfaceProperty) yyVals[0+yyTop];

		CheckDef (current_interface.AddProperty (p), p.Name);
          }
  break;
case 150:
#line 901 "cs-parser.jay"
  { 
		InterfaceEvent e = (InterfaceEvent) yyVals[0+yyTop];

		CheckDef (current_interface.AddEvent (e), e.Name);
	  }
  break;
case 151:
#line 907 "cs-parser.jay"
  { 
		InterfaceIndexer i = (InterfaceIndexer) yyVals[0+yyTop];

		CheckDef (current_interface.AddIndexer (i), "indexer");
	  }
  break;
case 152:
#line 915 "cs-parser.jay"
  { yyVal = false; }
  break;
case 153:
#line 916 "cs-parser.jay"
  { yyVal = true; }
  break;
case 154:
#line 923 "cs-parser.jay"
  {
		yyVal = new InterfaceMethod ((TypeRef) yyVals[-5+yyTop], (string) yyVals[-4+yyTop], (bool) yyVals[-6+yyTop], (Parameters) yyVals[-2+yyTop]);
	  }
  break;
case 155:
#line 929 "cs-parser.jay"
  {
		yyVal = new InterfaceMethod (type ("void"), (string) yyVals[-4+yyTop], (bool) yyVals[-6+yyTop], (Parameters) yyVals[-2+yyTop]);
	  }
  break;
case 156:
#line 939 "cs-parser.jay"
  { lexer.properties = true; }
  break;
case 157:
#line 941 "cs-parser.jay"
  { lexer.properties = false; }
  break;
case 158:
#line 943 "cs-parser.jay"
  {
	        int gs = (int) yyVals[-2+yyTop];

		yyVal = new InterfaceProperty ((TypeRef) yyVals[-6+yyTop], (string) yyVals[-5+yyTop], (bool) yyVals[-7+yyTop], 
					    (gs & 1) == 1, (gs & 2) == 2);
	  }
  break;
case 159:
#line 952 "cs-parser.jay"
  { yyVal = 1; }
  break;
case 160:
#line 953 "cs-parser.jay"
  { yyVal = 2; }
  break;
case 161:
#line 955 "cs-parser.jay"
  { yyVal = 3; }
  break;
case 162:
#line 957 "cs-parser.jay"
  { yyVal = 3; }
  break;
case 163:
#line 962 "cs-parser.jay"
  {
		yyVal = new InterfaceEvent ((TypeRef) yyVals[-2+yyTop], (string) yyVals[-1+yyTop], (bool) yyVals[-4+yyTop]);
	  }
  break;
case 164:
#line 971 "cs-parser.jay"
  { lexer.properties = true; }
  break;
case 165:
#line 973 "cs-parser.jay"
  { lexer.properties = false; }
  break;
case 166:
#line 975 "cs-parser.jay"
  {
		int a_flags = (int) yyVals[-2+yyTop];

	  	bool do_get = (a_flags & 1) == 1;
		bool do_set = (a_flags & 2) == 2;

		yyVal = new InterfaceIndexer ((TypeRef) yyVals[-9+yyTop], (Parameters) yyVals[-6+yyTop], do_get, do_set, (bool) yyVals[-10+yyTop]);
	  }
  break;
case 167:
#line 987 "cs-parser.jay"
  {
		/* FIXME: validate that opt_modifiers is exactly: PUBLIC and STATIC*/
	  }
  break;
case 168:
#line 995 "cs-parser.jay"
  {
		/* FIXME: since reduce/reduce on this*/
	 	/* rule, validate overloadable_operator is unary*/
	  }
  break;
case 169:
#line 1004 "cs-parser.jay"
  {
		/* FIXME: because of the reduce/reduce on PLUS and MINUS*/
		/* validate overloadable_operator is binary*/
	  }
  break;
case 195:
#line 1049 "cs-parser.jay"
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
#line 1069 "cs-parser.jay"
  {
		ConstructorInitializer i = null;

		if (yyVals[0+yyTop] != null)
			i = (ConstructorInitializer) yyVals[0+yyTop];

		yyVal = new Constructor ((string) yyVals[-4+yyTop], (Parameters) yyVals[-2+yyTop], i);
	
		current_local_parameters = (Parameters) yyVals[-2+yyTop];
	  }
  break;
case 197:
#line 1082 "cs-parser.jay"
  { yyVal = null; }
  break;
case 199:
#line 1088 "cs-parser.jay"
  {
		yyVal = new ConstructorBaseInitializer ((ArrayList) yyVals[-1+yyTop]);
	  }
  break;
case 200:
#line 1092 "cs-parser.jay"
  {
		yyVal = new ConstructorThisInitializer ((ArrayList) yyVals[-1+yyTop]);
	  }
  break;
case 201:
#line 1099 "cs-parser.jay"
  {
		Method d = new Method (type ("void"), 0, "Finalize", new Parameters (null, null));

		d.Block = (Block) yyVals[0+yyTop];
		CheckDef (current_container.AddMethod (d), d.Name);
	  }
  break;
case 202:
#line 1111 "cs-parser.jay"
  { note ("validate that the flags only contain new public protected internal private static virtual sealed override abstract"); }
  break;
case 203:
#line 1116 "cs-parser.jay"
  { note ("validate that the flags only contain new public protected internal private static virtual sealed override abstract"); }
  break;
case 208:
#line 1135 "cs-parser.jay"
  { 
		/* The signature is computed from the signature of the indexer.  Look*/
	 	/* at section 3.6 on the spec*/
		note ("verify modifiers are NEW PUBLIC PROTECTED INTERNAL PRIVATE VIRTUAL SEALED OVERRIDE ABSTRACT"); 
	  }
  break;
case 211:
#line 1154 "cs-parser.jay"
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
#line 1171 "cs-parser.jay"
  { yyVal = type ("System.Int32"); }
  break;
case 213:
#line 1172 "cs-parser.jay"
  { yyVal = yyVals[0+yyTop];   }
  break;
case 214:
#line 1177 "cs-parser.jay"
  {
		yyVal = yyVals[-1+yyTop];
	  }
  break;
case 215:
#line 1181 "cs-parser.jay"
  {
		yyVal = yyVals[-2+yyTop];
	  }
  break;
case 216:
#line 1187 "cs-parser.jay"
  { yyVal = new ArrayList (); }
  break;
case 217:
#line 1188 "cs-parser.jay"
  { yyVal = yyVals[0+yyTop]; }
  break;
case 218:
#line 1193 "cs-parser.jay"
  {
		ArrayList l = new ArrayList ();

		l.Add (yyVals[0+yyTop]);
		yyVal = l;
	  }
  break;
case 219:
#line 1200 "cs-parser.jay"
  {
		ArrayList l = (ArrayList) yyVals[-2+yyTop];

		l.Add (yyVals[0+yyTop]);

		yyVal = l;
	  }
  break;
case 220:
#line 1211 "cs-parser.jay"
  {
		yyVal = new VariableDeclaration ((string) yyVals[0+yyTop], null);
	  }
  break;
case 221:
#line 1215 "cs-parser.jay"
  { 
		yyVal = new VariableDeclaration ((string) yyVals[-2+yyTop], yyVals[0+yyTop]);
	  }
  break;
case 222:
#line 1228 "cs-parser.jay"
  { note ("validate that modifiers only contains NEW PUBLIC PROTECTED INTERNAL PRIVATE"); }
  break;
case 225:
#line 1246 "cs-parser.jay"
  {  	/* class_type */
		/* 
	           This does interfaces, delegates, struct_types, class_types, 
	           parent classes, and more! 4.2 
	         */
		yyVal = type ((string) yyVals[0+yyTop]); 
	  }
  break;
case 228:
#line 1259 "cs-parser.jay"
  {
		ArrayList types = new ArrayList ();

		types.Add (yyVals[0+yyTop]);
		yyVal = types;
	  }
  break;
case 229:
#line 1266 "cs-parser.jay"
  {
		ArrayList types = new ArrayList ();
		types.Add (yyVals[0+yyTop]);
		yyVal = types;
	  }
  break;
case 230:
#line 1278 "cs-parser.jay"
  { yyVal = type ("System.Object"); }
  break;
case 231:
#line 1279 "cs-parser.jay"
  { yyVal = type ("System.String"); }
  break;
case 232:
#line 1280 "cs-parser.jay"
  { yyVal = type ("System.Boolean"); }
  break;
case 233:
#line 1281 "cs-parser.jay"
  { yyVal = type ("System.Decimal"); }
  break;
case 234:
#line 1282 "cs-parser.jay"
  { yyVal = type ("System.Single"); }
  break;
case 235:
#line 1283 "cs-parser.jay"
  { yyVal = type ("System.Double"); }
  break;
case 237:
#line 1288 "cs-parser.jay"
  { yyVal = type ("System.SByte"); }
  break;
case 238:
#line 1289 "cs-parser.jay"
  { yyVal = type ("System.Byte"); }
  break;
case 239:
#line 1290 "cs-parser.jay"
  { yyVal = type ("System.Int16"); }
  break;
case 240:
#line 1291 "cs-parser.jay"
  { yyVal = type ("System.UInt16"); }
  break;
case 241:
#line 1292 "cs-parser.jay"
  { yyVal = type ("System.Int32"); }
  break;
case 242:
#line 1293 "cs-parser.jay"
  { yyVal = type ("System.UInt32"); }
  break;
case 243:
#line 1294 "cs-parser.jay"
  { yyVal = type ("System.Int64"); }
  break;
case 244:
#line 1295 "cs-parser.jay"
  { yyVal = type ("System.UInt64"); }
  break;
case 245:
#line 1296 "cs-parser.jay"
  { yyVal = type ("System.Char"); }
  break;
case 247:
#line 1305 "cs-parser.jay"
  {
		yyVal = yyVals[-1+yyTop];
		/* FIXME: We need to create a type for the nested thing.*/
	  }
  break;
case 248:
#line 1316 "cs-parser.jay"
  {
		/* 7.5.1: Literals*/
		
	  }
  break;
case 249:
#line 1322 "cs-parser.jay"
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
#line 1364 "cs-parser.jay"
  { yyVal = new CharLiteral ((char) lexer.Value); }
  break;
case 267:
#line 1365 "cs-parser.jay"
  { yyVal = new StringLiteral ((string) lexer.Value); }
  break;
case 268:
#line 1366 "cs-parser.jay"
  { yyVal = new NullLiteral (); }
  break;
case 269:
#line 1370 "cs-parser.jay"
  { yyVal = new FloatLiteral ((float) lexer.Value); }
  break;
case 270:
#line 1371 "cs-parser.jay"
  { yyVal = new DoubleLiteral ((double) lexer.Value); }
  break;
case 271:
#line 1372 "cs-parser.jay"
  { yyVal = new DecimalLiteral ((decimal) lexer.Value); }
  break;
case 272:
#line 1376 "cs-parser.jay"
  { yyVal = new IntLiteral ((Int32) lexer.Value); }
  break;
case 273:
#line 1380 "cs-parser.jay"
  { yyVal = new BoolLiteral (true); }
  break;
case 274:
#line 1381 "cs-parser.jay"
  { yyVal = new BoolLiteral (false); }
  break;
case 275:
#line 1386 "cs-parser.jay"
  { yyVal = yyVals[-1+yyTop]; }
  break;
case 276:
#line 1391 "cs-parser.jay"
  {
		yyVal = new MemberAccess ((Expression) yyVals[-2+yyTop], (string) yyVals[0+yyTop]);
	  }
  break;
case 277:
#line 1395 "cs-parser.jay"
  {
		yyVal = new BuiltinTypeAccess ((TypeRef) yyVals[-2+yyTop], (string) yyVals[0+yyTop]);
	  }
  break;
case 279:
#line 1406 "cs-parser.jay"
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
#line 1420 "cs-parser.jay"
  { yyVal = new ArrayList (); }
  break;
case 282:
#line 1426 "cs-parser.jay"
  { 
		ArrayList list = new ArrayList ();
		list.Add (yyVals[0+yyTop]);
		yyVal = list;
	  }
  break;
case 283:
#line 1432 "cs-parser.jay"
  {
		ArrayList list = (ArrayList) yyVals[-2+yyTop];
		list.Add (yyVals[0+yyTop]);
		yyVal = list;
	  }
  break;
case 284:
#line 1441 "cs-parser.jay"
  {
		yyVal = new Argument ((Expression) yyVals[0+yyTop], Argument.AType.Expression);
	  }
  break;
case 285:
#line 1445 "cs-parser.jay"
  { 
		yyVal = new Argument ((Expression) yyVals[0+yyTop], Argument.AType.Ref);
	  }
  break;
case 286:
#line 1449 "cs-parser.jay"
  { 
		yyVal = new Argument ((Expression) yyVals[0+yyTop], Argument.AType.Out);
	  }
  break;
case 287:
#line 1455 "cs-parser.jay"
  { note ("section 5.4"); yyVal = yyVals[0+yyTop]; }
  break;
case 291:
#line 1470 "cs-parser.jay"
  {
		yyVal = new This ();
	  }
  break;
case 294:
#line 1484 "cs-parser.jay"
  {
		yyVal = new Unary (Unary.Operator.PostIncrement, (Expression) yyVals[-1+yyTop]);
	  }
  break;
case 295:
#line 1491 "cs-parser.jay"
  {
		yyVal = new Unary (Unary.Operator.PostDecrement, (Expression) yyVals[-1+yyTop]);
	  }
  break;
case 298:
#line 1503 "cs-parser.jay"
  {
		yyVal = new New ((TypeRef) yyVals[-3+yyTop], (ArrayList) yyVals[-1+yyTop]);
	  }
  break;
case 316:
#line 1563 "cs-parser.jay"
  {
		yyVal = new TypeOf ((TypeRef) yyVals[-1+yyTop]);
	  }
  break;
case 317:
#line 1569 "cs-parser.jay"
  { 
		yyVal = new SizeOf ((TypeRef) yyVals[-1+yyTop]);

		note ("Verify type is unmanaged"); 
		note ("if (5.8) builtin, yield constant expression");
	  }
  break;
case 321:
#line 1587 "cs-parser.jay"
  { 
	  	yyVal = new Unary (Unary.Operator.Plus, (Expression) yyVals[0+yyTop]);
	  }
  break;
case 322:
#line 1591 "cs-parser.jay"
  { 
		yyVal = new Unary (Unary.Operator.Minus, (Expression) yyVals[0+yyTop]);
	  }
  break;
case 323:
#line 1595 "cs-parser.jay"
  {
		yyVal = new Unary (Unary.Operator.Negate, (Expression) yyVals[0+yyTop]);
	  }
  break;
case 324:
#line 1599 "cs-parser.jay"
  {
		yyVal = new Unary (Unary.Operator.BitComplement, (Expression) yyVals[0+yyTop]);
	  }
  break;
case 325:
#line 1603 "cs-parser.jay"
  {
		yyVal = new Unary (Unary.Operator.Indirection, (Expression) yyVals[0+yyTop]);
	  }
  break;
case 326:
#line 1607 "cs-parser.jay"
  {
		yyVal = new Unary (Unary.Operator.AddressOf, (Expression) yyVals[0+yyTop]);
	  }
  break;
case 330:
#line 1622 "cs-parser.jay"
  {
		yyVal = new Unary (Unary.Operator.PreIncrement, (Expression) yyVals[0+yyTop]);
	  }
  break;
case 331:
#line 1629 "cs-parser.jay"
  {
		yyVal = new Unary (Unary.Operator.PreDecrement, (Expression) yyVals[0+yyTop]);
	  }
  break;
case 332:
#line 1639 "cs-parser.jay"
  {
		yyVal = new Cast (type ((string) yyVals[-2+yyTop]), (Expression) yyVals[0+yyTop]);
	  }
  break;
case 333:
#line 1643 "cs-parser.jay"
  {
		yyVal = new Cast ((TypeRef) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 335:
#line 1651 "cs-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.Multiply, 
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 336:
#line 1656 "cs-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.Divide, 
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 337:
#line 1661 "cs-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.Modulo, 
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 339:
#line 1670 "cs-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.Add, 
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 340:
#line 1675 "cs-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.Substract, 
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 342:
#line 1684 "cs-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.ShiftLeft, 
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 343:
#line 1689 "cs-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.ShiftRight, 
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 345:
#line 1698 "cs-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.LessThan, 
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 346:
#line 1703 "cs-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.GreatherThan, 
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 347:
#line 1708 "cs-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.LessOrEqual, 
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 348:
#line 1713 "cs-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.GreatherOrEqual, 
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 349:
#line 1718 "cs-parser.jay"
  {
		yyVal = new Probe (Probe.Operator.Is, 
			         (Expression) yyVals[-2+yyTop], (TypeRef) yyVals[0+yyTop]);
	  }
  break;
case 350:
#line 1723 "cs-parser.jay"
  {
		yyVal = new Probe (Probe.Operator.As, 
			         (Expression) yyVals[-2+yyTop], (TypeRef) yyVals[0+yyTop]);
	  }
  break;
case 352:
#line 1732 "cs-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.Equal, 
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 353:
#line 1737 "cs-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.NotEqual, 
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 355:
#line 1746 "cs-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.BitwiseAnd, 
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 357:
#line 1755 "cs-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.ExclusiveOr, 
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 359:
#line 1764 "cs-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.BitwiseOr, 
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 361:
#line 1773 "cs-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.LogicalAnd, 
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 363:
#line 1782 "cs-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.LogicalOr, 
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 365:
#line 1791 "cs-parser.jay"
  {
		yyVal = new Conditional ((Expression) yyVals[-4+yyTop], (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 366:
#line 1798 "cs-parser.jay"
  {
		yyVal = new Assign ((Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop]);
	  }
  break;
case 367:
#line 1802 "cs-parser.jay"
  {
		yyVal = new Assign ((Expression) yyVals[-2+yyTop],
				 new Binary (Binary.Operator.Multiply, 
					     (Expression) yyVals[-2+yyTop],
					     (Expression) yyVals[0+yyTop]));
	  }
  break;
case 368:
#line 1809 "cs-parser.jay"
  {
		yyVal = new Assign ((Expression) yyVals[-2+yyTop],
				 new Binary (Binary.Operator.Divide, 
					     (Expression) yyVals[-2+yyTop],
					     (Expression) yyVals[0+yyTop]));
	  }
  break;
case 369:
#line 1816 "cs-parser.jay"
  {
		yyVal = new Assign ((Expression) yyVals[-2+yyTop],
				 new Binary (Binary.Operator.Modulo, 
					     (Expression) yyVals[-2+yyTop],
					     (Expression) yyVals[0+yyTop]));
	  }
  break;
case 370:
#line 1823 "cs-parser.jay"
  {
		yyVal = new Assign ((Expression) yyVals[-2+yyTop],
				 new Binary (Binary.Operator.Add, 
					     (Expression) yyVals[-2+yyTop],
					     (Expression) yyVals[0+yyTop]));
	  }
  break;
case 371:
#line 1830 "cs-parser.jay"
  {
		yyVal = new Assign ((Expression) yyVals[-2+yyTop],
				 new Binary (Binary.Operator.Substract, 
					     (Expression) yyVals[-2+yyTop],
					     (Expression) yyVals[0+yyTop]));
	  }
  break;
case 372:
#line 1837 "cs-parser.jay"
  {
		yyVal = new Assign ((Expression) yyVals[-2+yyTop],
				 new Binary (Binary.Operator.ShiftLeft, 
					     (Expression) yyVals[-2+yyTop],
					     (Expression) yyVals[0+yyTop]));
	  }
  break;
case 373:
#line 1844 "cs-parser.jay"
  {
		yyVal = new Assign ((Expression) yyVals[-2+yyTop],
				 new Binary (Binary.Operator.ShiftRight, 
					     (Expression) yyVals[-2+yyTop],
					     (Expression) yyVals[0+yyTop]));
	  }
  break;
case 374:
#line 1851 "cs-parser.jay"
  {
		yyVal = new Assign ((Expression) yyVals[-2+yyTop],
				 new Binary (Binary.Operator.BitwiseAnd, 
					     (Expression) yyVals[-2+yyTop],
					     (Expression) yyVals[0+yyTop]));
	  }
  break;
case 375:
#line 1858 "cs-parser.jay"
  {
		yyVal = new Assign ((Expression) yyVals[-2+yyTop],
				 new Binary (Binary.Operator.BitwiseOr, 
					     (Expression) yyVals[-2+yyTop],
					     (Expression) yyVals[0+yyTop]));
	  }
  break;
case 376:
#line 1865 "cs-parser.jay"
  {
		yyVal = new Assign ((Expression) yyVals[-2+yyTop],
				 new Binary (Binary.Operator.ExclusiveOr, 
					     (Expression) yyVals[-2+yyTop],
					     (Expression) yyVals[0+yyTop]));
	  }
  break;
case 380:
#line 1883 "cs-parser.jay"
  { CheckBoolean ((Expression) yyVals[0+yyTop]); yyVal = yyVals[0+yyTop]; }
  break;
case 381:
#line 1893 "cs-parser.jay"
  {
		Class new_class;
		string full_class_name = MakeName ((string) yyVals[0+yyTop]);

		new_class = new Class (current_container, full_class_name, (int) yyVals[-2+yyTop]);
		current_container = new_class;
		current_container.Namespace = current_namespace;
		tree.RecordType (full_class_name, new_class);
	  }
  break;
case 382:
#line 1905 "cs-parser.jay"
  {
		Class new_class = (Class) current_container;

		if (yyVals[-2+yyTop] != null)
			new_class.Bases = (ArrayList) yyVals[-2+yyTop];

		current_container = current_container.Parent;
		CheckDef (current_container.AddClass (new_class), new_class.Name);

		yyVal = new_class;
	  }
  break;
case 383:
#line 1919 "cs-parser.jay"
  { yyVal = (int) 0; }
  break;
case 386:
#line 1925 "cs-parser.jay"
  { 
		int m1 = (int) yyVals[-1+yyTop];
		int m2 = (int) yyVals[0+yyTop];

		if ((m1 & m2) != 0)
			error (1002, "Duplicate modifier: `" + Modifiers.Name (m2) + "'");

		yyVal = (int) (m1 | m2);
	  }
  break;
case 387:
#line 1937 "cs-parser.jay"
  { yyVal = Modifiers.NEW; }
  break;
case 388:
#line 1938 "cs-parser.jay"
  { yyVal = Modifiers.PUBLIC; }
  break;
case 389:
#line 1939 "cs-parser.jay"
  { yyVal = Modifiers.PROTECTED; }
  break;
case 390:
#line 1940 "cs-parser.jay"
  { yyVal = Modifiers.INTERNAL; }
  break;
case 391:
#line 1941 "cs-parser.jay"
  { yyVal = Modifiers.PRIVATE; }
  break;
case 392:
#line 1942 "cs-parser.jay"
  { yyVal = Modifiers.ABSTRACT; }
  break;
case 393:
#line 1943 "cs-parser.jay"
  { yyVal = Modifiers.SEALED; }
  break;
case 394:
#line 1944 "cs-parser.jay"
  { yyVal = Modifiers.STATIC; }
  break;
case 395:
#line 1945 "cs-parser.jay"
  { yyVal = Modifiers.READONLY; }
  break;
case 396:
#line 1946 "cs-parser.jay"
  { yyVal = Modifiers.VIRTUAL; }
  break;
case 397:
#line 1947 "cs-parser.jay"
  { yyVal = Modifiers.OVERRIDE; }
  break;
case 398:
#line 1948 "cs-parser.jay"
  { yyVal = Modifiers.EXTERN; }
  break;
case 399:
#line 1952 "cs-parser.jay"
  { yyVal = null; }
  break;
case 400:
#line 1953 "cs-parser.jay"
  { yyVal = yyVals[0+yyTop]; }
  break;
case 401:
#line 1957 "cs-parser.jay"
  { yyVal = yyVals[0+yyTop]; }
  break;
case 402:
#line 1975 "cs-parser.jay"
  {
		current_block = new Block (current_block);
	  }
  break;
case 403:
#line 1979 "cs-parser.jay"
  { 
		while (current_block.Implicit)
			current_block = current_block.Parent;
		yyVal = current_block;
		current_block = current_block.Parent;
	  }
  break;
case 408:
#line 1999 "cs-parser.jay"
  {
		if ((Block) yyVals[0+yyTop] != current_block){
			current_block.AddStatement ((Statement) yyVals[0+yyTop]);
			current_block = (Block) yyVals[0+yyTop];
		}
	  }
  break;
case 409:
#line 2006 "cs-parser.jay"
  {
		current_block.AddStatement ((Statement) yyVals[0+yyTop]);
	  }
  break;
case 410:
#line 2010 "cs-parser.jay"
  {
		current_block.AddStatement ((Statement) yyVals[0+yyTop]);
	  }
  break;
case 422:
#line 2031 "cs-parser.jay"
  {
		  yyVal = new EmptyStatement ();
	  }
  break;
case 423:
#line 2038 "cs-parser.jay"
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
case 426:
#line 2066 "cs-parser.jay"
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
case 427:
#line 2087 "cs-parser.jay"
  {
		/* FIXME: Do something smart with the type here.*/
		yyVal = yyVals[-1+yyTop]; 
	  }
  break;
case 428:
#line 2092 "cs-parser.jay"
  {
		yyVal = type ("VOID SOMETHING TYPE");
	  }
  break;
case 434:
#line 2114 "cs-parser.jay"
  {
		yyVal = declare_local_variables ((TypeRef) yyVals[-1+yyTop], (ArrayList) yyVals[0+yyTop]);
	  }
  break;
case 436:
#line 2126 "cs-parser.jay"
  {
		yyVal = yyVals[-1+yyTop];
	  }
  break;
case 437:
#line 2136 "cs-parser.jay"
  { yyVal = new StatementExpression ((Expression) yyVals[0+yyTop]); }
  break;
case 438:
#line 2137 "cs-parser.jay"
  { yyVal = new StatementExpression ((Expression) yyVals[0+yyTop]); }
  break;
case 439:
#line 2138 "cs-parser.jay"
  { yyVal = new StatementExpression ((Expression) yyVals[0+yyTop]); }
  break;
case 440:
#line 2139 "cs-parser.jay"
  { yyVal = new StatementExpression ((Expression) yyVals[0+yyTop]); }
  break;
case 441:
#line 2140 "cs-parser.jay"
  { yyVal = new StatementExpression ((Expression) yyVals[0+yyTop]); }
  break;
case 442:
#line 2141 "cs-parser.jay"
  { yyVal = new StatementExpression ((Expression) yyVals[0+yyTop]); }
  break;
case 443:
#line 2142 "cs-parser.jay"
  { yyVal = new StatementExpression ((Expression) yyVals[0+yyTop]); }
  break;
case 444:
#line 2147 "cs-parser.jay"
  { note ("complain if this is a delegate maybe?"); }
  break;
case 447:
#line 2158 "cs-parser.jay"
  { 
		yyVal = new If ((Expression) yyVals[-2+yyTop], (Statement) yyVals[0+yyTop]);
	  }
  break;
case 448:
#line 2163 "cs-parser.jay"
  {
		yyVal = new If ((Expression) yyVals[-4+yyTop], (Statement) yyVals[-2+yyTop], (Statement) yyVals[0+yyTop]);
	  }
  break;
case 449:
#line 2171 "cs-parser.jay"
  {
		yyVal = new Switch ((Expression) yyVals[-2+yyTop], (ArrayList) yyVals[0+yyTop]);
	  }
  break;
case 450:
#line 2180 "cs-parser.jay"
  {
		yyVal = yyVals[-1+yyTop];
	  }
  break;
case 451:
#line 2186 "cs-parser.jay"
  { yyVal = new ArrayList (); }
  break;
case 453:
#line 2192 "cs-parser.jay"
  {
		ArrayList sections = new ArrayList ();

		sections.Add (yyVals[0+yyTop]);
		yyVal = sections;
	  }
  break;
case 454:
#line 2199 "cs-parser.jay"
  {
		ArrayList sections = (ArrayList) yyVals[-1+yyTop];

		sections.Add (yyVals[0+yyTop]);
		yyVal = sections;
	  }
  break;
case 455:
#line 2209 "cs-parser.jay"
  {
		current_block = new Block (current_block);
	  }
  break;
case 456:
#line 2213 "cs-parser.jay"
  {
		while (current_block.Implicit)
			current_block = current_block.Parent;
		yyVal = new SwitchSection ((ArrayList) yyVals[-2+yyTop], current_block);
		current_block = current_block.Parent;
	  }
  break;
case 457:
#line 2223 "cs-parser.jay"
  {
		ArrayList labels = new ArrayList ();

		labels.Add (yyVals[0+yyTop]);
		yyVal = labels;
	  }
  break;
case 458:
#line 2230 "cs-parser.jay"
  {
		ArrayList labels = (ArrayList) (yyVals[-1+yyTop]);
		labels.Add (yyVals[0+yyTop]);

		yyVal = labels;
	  }
  break;
case 459:
#line 2239 "cs-parser.jay"
  { yyVal = new SwitchLabel ((Expression) yyVals[-1+yyTop]); }
  break;
case 460:
#line 2240 "cs-parser.jay"
  { yyVal = new SwitchLabel (null); }
  break;
case 465:
#line 2252 "cs-parser.jay"
  {
		yyVal = new While ((Expression) yyVals[-2+yyTop], (Statement) yyVals[0+yyTop]);
	}
  break;
case 466:
#line 2260 "cs-parser.jay"
  {
		yyVal = new Do ((Statement) yyVals[-5+yyTop], (Expression) yyVals[-2+yyTop]);
	  }
  break;
case 467:
#line 2271 "cs-parser.jay"
  {
		yyVal = new For ((Statement) yyVals[-6+yyTop], (Expression) yyVals[-4+yyTop], (Statement) yyVals[-2+yyTop], (Statement) yyVals[0+yyTop]);
	  }
  break;
case 468:
#line 2277 "cs-parser.jay"
  { yyVal = new EmptyStatement (); }
  break;
case 472:
#line 2287 "cs-parser.jay"
  { yyVal = new BoolLiteral (true); }
  break;
case 474:
#line 2292 "cs-parser.jay"
  { yyVal = new EmptyStatement (); }
  break;
case 477:
#line 2302 "cs-parser.jay"
  {
		Block b = new Block (null, true);

		b.AddStatement ((Statement) yyVals[0+yyTop]);
		yyVal = b;
	  }
  break;
case 478:
#line 2309 "cs-parser.jay"
  {
		Block b = (Block) yyVals[-2+yyTop];

		b.AddStatement ((Statement) yyVals[0+yyTop]);
		yyVal = yyVals[-2+yyTop];
	  }
  break;
case 479:
#line 2320 "cs-parser.jay"
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
case 485:
#line 2363 "cs-parser.jay"
  {
		yyVal = new Break ();
	  }
  break;
case 486:
#line 2370 "cs-parser.jay"
  {
		yyVal = new Continue ();
	  }
  break;
case 487:
#line 2377 "cs-parser.jay"
  {
		yyVal = new Goto ((string) yyVals[-1+yyTop]);
	  }
  break;
case 490:
#line 2386 "cs-parser.jay"
  {
		yyVal = new Return ((Expression) yyVals[-1+yyTop]);
	  }
  break;
case 491:
#line 2393 "cs-parser.jay"
  {
		yyVal = new Throw ((Expression) yyVals[-1+yyTop]);
	  }
  break;
case 494:
#line 2405 "cs-parser.jay"
  {
		DictionaryEntry cc = (DictionaryEntry) yyVals[0+yyTop];
		ArrayList s = null;

		if (cc.Key != null)
			s = (ArrayList) cc.Key;

		yyVal = new Try ((Block) yyVals[-1+yyTop], s, (Catch) cc.Value, null);
	  }
  break;
case 495:
#line 2415 "cs-parser.jay"
  {
		yyVal = new Try ((Block) yyVals[-1+yyTop], null, null, (Block) yyVals[0+yyTop]);
	  }
  break;
case 496:
#line 2419 "cs-parser.jay"
  {
		DictionaryEntry cc = (DictionaryEntry) yyVals[-1+yyTop];
		ArrayList s = null;

		if (cc.Key != null)
			s = (ArrayList) cc.Key;

		yyVal = new Try ((Block) yyVals[-2+yyTop], s, (Catch) cc.Value, (Block) yyVals[0+yyTop]);
	  }
  break;
case 497:
#line 2432 "cs-parser.jay"
  {
		DictionaryEntry pair = new DictionaryEntry ();

		pair.Key = yyVals[-1+yyTop]; 
		pair.Value = yyVals[0+yyTop];

		yyVal = pair;
	  }
  break;
case 498:
#line 2441 "cs-parser.jay"
  {
		DictionaryEntry pair = new DictionaryEntry ();
		pair.Key = yyVals[-1+yyTop];
		pair.Value = yyVals[-1+yyTop];

		yyVal = pair;
	  }
  break;
case 499:
#line 2451 "cs-parser.jay"
  { yyVal = null; }
  break;
case 501:
#line 2456 "cs-parser.jay"
  { yyVal = null; }
  break;
case 503:
#line 2462 "cs-parser.jay"
  {
		ArrayList l = new ArrayList ();

		l.Add (yyVals[0+yyTop]);
		yyVal = l;
	  }
  break;
case 504:
#line 2469 "cs-parser.jay"
  {
		ArrayList l = (ArrayList) yyVals[-1+yyTop];

		l.Add (yyVals[0+yyTop]);
		yyVal = l;
	  }
  break;
case 505:
#line 2479 "cs-parser.jay"
  {
		string id = null;

		if (yyVals[-2+yyTop] != null)
			id = (string) yyVals[-2+yyTop];

		yyVal = new Catch ((TypeRef) yyVals[-3+yyTop], id, (Block) yyVals[0+yyTop]);
	  }
  break;
case 506:
#line 2490 "cs-parser.jay"
  { yyVal = null; }
  break;
case 508:
#line 2496 "cs-parser.jay"
  {
		yyVal = new Catch (null, null, (Block) yyVals[0+yyTop]);
	  }
  break;
case 509:
#line 2503 "cs-parser.jay"
  {
		yyVal = yyVals[0+yyTop];
	  }
  break;
case 510:
#line 2510 "cs-parser.jay"
  {
		yyVal = new Checked ((Block) yyVals[0+yyTop]);
	  }
  break;
case 511:
#line 2517 "cs-parser.jay"
  {
		yyVal = new Unchecked ((Block) yyVals[0+yyTop]);
	  }
  break;
case 512:
#line 2524 "cs-parser.jay"
  {
		yyVal = new Lock ((Expression) yyVals[-2+yyTop], (Statement) yyVals[0+yyTop]);
	  }
  break;
#line 2684 "-"
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
  161,  162,  162,  163,  163,  163,  163,  164,  164,  164,
  165,  165,  165,  166,  166,  166,  166,  166,  166,  166,
  167,  167,  167,  168,  168,  169,  169,  170,  170,  171,
  171,  172,  172,  173,  173,  174,  174,  174,  174,  174,
  174,  174,  174,  174,  174,  174,   35,   35,   64,  175,
  177,   20,   51,   51,  178,  178,  179,  179,  179,  179,
  179,  179,  179,  179,  179,  179,  179,  179,  176,  176,
  180,  182,   73,  181,  181,  183,  183,  184,  184,  184,
  186,  186,  186,  186,  186,  186,  186,  186,  186,  186,
  186,  188,  187,  185,  185,  200,  200,  200,  201,  201,
  202,  202,  203,  198,  199,  189,  204,  204,  204,  204,
  204,  204,  204,  205,  190,  190,  206,  206,  207,  208,
  209,  209,  210,  210,  213,  211,  212,  212,  214,  214,
  191,  191,  191,  191,  215,  216,  217,  219,  219,  222,
  222,  220,  220,  221,  221,  224,  223,  223,  218,  192,
  192,  192,  192,  192,  225,  226,  227,  227,  227,  228,
  229,  230,  230,  193,  193,  193,  231,  231,  234,  234,
  235,  235,  233,  233,  237,  238,  238,  236,  232,  194,
  195,  196,  197,  239,
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
    2,    2,    2,    2,    2,    2,    1,    1,    1,    2,
    2,    4,    4,    1,    3,    3,    3,    1,    3,    3,
    1,    3,    3,    1,    3,    3,    3,    3,    3,    3,
    1,    3,    3,    1,    3,    1,    3,    1,    3,    1,
    3,    1,    3,    1,    5,    3,    3,    3,    3,    3,
    3,    3,    3,    3,    3,    3,    1,    1,    1,    1,
    0,    8,    0,    1,    1,    2,    1,    1,    1,    1,
    1,    1,    1,    1,    1,    1,    1,    1,    0,    1,
    2,    0,    4,    0,    1,    1,    2,    1,    1,    1,
    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,
    1,    1,    3,    2,    2,    2,    2,    2,    0,    1,
    1,    2,    3,    2,    3,    2,    1,    1,    1,    1,
    1,    1,    1,    1,    1,    1,    5,    7,    5,    3,
    0,    1,    1,    2,    0,    3,    1,    2,    3,    2,
    1,    1,    1,    1,    5,    7,    9,    0,    1,    1,
    1,    0,    1,    0,    1,    1,    1,    3,    8,    1,
    1,    1,    1,    1,    2,    2,    3,    4,    3,    3,
    3,    0,    1,    3,    3,    4,    2,    2,    0,    1,
    0,    1,    1,    2,    6,    0,    1,    2,    2,    2,
    2,    5,    5,    2,
  };
   static  short [] yyDefRed = {            0,
    0,    0,    0,    0,    2,    4,    5,    0,   18,    0,
    0,    0,    0,    0,    3,    0,    7,    0,   40,   41,
   39,    0,   37,    0,    0,    0,    0,   27,    0,   24,
   26,   28,   29,   30,   31,   32,   34,   16,    0,   17,
    0,  223,    0,   42,   44,   45,   46,   38,    0,  392,
  398,  390,  387,  397,  391,  389,  388,  395,  393,  394,
  396,    0,    0,  385,    1,   25,    6,    0,  232,  238,
  245,    0,  233,  235,  274,  234,  241,  243,    0,  268,
  230,  237,  239,    0,  231,  291,  273,    0,  242,  244,
    0,  240,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  272,  269,  270,  271,  266,  267,    0,    0,   49,
  236,  278,    0,  248,  250,  251,  252,  253,  254,  255,
  256,  257,  258,  259,  260,  261,  262,  263,  264,  265,
    0,  296,  297,    0,  327,  328,  329,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  377,  378,   35,
    0,    0,    0,    0,    0,    0,    0,  386,    0,    0,
    0,  225,    0,  226,  227,    0,    0,    0,    0,    0,
    0,  324,  321,  322,  323,  326,  325,  330,  331,   48,
    0,    0,    0,    0,  294,  295,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,   43,
    0,    0,  381,    0,    0,  136,   70,  289,    0,  292,
    0,    0,    0,  247,    0,    0,    0,    0,    0,  275,
    0,   50,    0,    0,    0,  284,    0,    0,  282,  276,
  277,  366,  367,  368,  369,  370,  371,  372,  373,  374,
  376,  375,  335,  337,  336,  334,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,   13,   11,    0,    0,    0,
    0,    0,    0,    0,  293,    0,  318,  307,    0,    0,
    0,    0,  303,  317,  316,  319,  332,  333,  288,  287,
  286,  285,  279,    0,    0,    0,    0,    0,  400,    0,
  213,    0,    0,    0,    0,  139,    0,    0,    0,   74,
  290,    0,  304,  308,  298,  283,  365,   19,    0,    0,
    0,    0,    0,    0,    0,  111,  112,    0,    0,    0,
  218,  211,  246,  141,    0,    0,  137,    0,    0,    0,
   75,  301,    0,    0,    0,   69,    0,    0,   58,   60,
   61,   62,   63,   64,   65,   66,   67,   68,    0,  382,
  118,    0,  117,    0,  116,    0,    0,    0,  214,    0,
    0,    0,    0,    0,  146,  148,  149,  150,  151,    0,
   90,   82,   83,   84,   85,   86,   87,   88,   89,    0,
    0,   80,   71,    0,  310,  299,    0,    0,   55,   59,
  402,  106,  102,  105,    0,    0,  222,  110,  113,    0,
  215,  219,  142,  153,    0,  143,  147,   77,   81,  311,
  100,  314,  101,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  170,    0,    0,    0,  119,  114,  221,
    0,    0,    0,  312,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,   96,    0,
    0,  167,  195,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  422,    0,  411,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  439,    0,    0,  406,  408,  409,
  410,  412,  413,  414,  415,  416,  417,  418,  419,  420,
  421,    0,    0,    0,    0,  438,  445,  446,  461,  462,
  463,  464,  480,  481,  482,  483,  484,    0,    0,    0,
    0,  313,  315,    0,    0,    0,   92,    0,    0,    0,
    0,    0,    0,    0,  108,  176,  175,  172,  177,  178,
  171,  190,  189,  182,  183,  179,  181,  180,  184,  173,
  174,  185,  186,  192,  191,  187,  188,    0,    0,    0,
    0,   95,  122,    0,    0,    0,    0,    0,    0,  485,
  510,    0,  486,    0,    0,    0,    0,    0,    0,    0,
    0,  493,    0,    0,    0,    0,  511,    0,    0,  428,
    0,  431,    0,    0,  427,    0,  426,  403,  407,  424,
  425,    0,    0,  436,    0,    0,    0,  156,    0,  201,
    0,    0,   91,  202,    0,    0,    0,    0,  121,    0,
    0,    0,   98,   97,    0,    0,    0,    0,  132,  208,
    0,  125,  130,    0,  128,  126,  435,    0,  470,  477,
    0,  469,    0,    0,  379,    0,  489,  487,  380,    0,
    0,  490,    0,  491,    0,    0,    0,  495,    0,    0,
  503,    0,    0,    0,    0,    0,  432,    0,  423,  163,
    0,    0,    0,    0,   94,   93,    0,    0,    0,    0,
    0,    0,  104,    0,  196,  198,    0,  209,  123,  103,
    0,  135,  134,  131,    0,    0,    0,    0,    0,  488,
    0,    0,    0,    0,  509,  496,    0,  497,  500,  504,
    0,  498,  514,    0,  433,    0,    0,    0,    0,  157,
    0,    0,    0,    0,    0,  204,    0,  205,    0,    0,
    0,    0,    0,    0,    0,  133,    0,  473,    0,  478,
    0,    0,  512,    0,  449,    0,  508,  513,  465,  155,
  164,    0,    0,    0,  154,  206,  207,  203,  194,  193,
    0,    0,  168,    0,  124,  210,    0,    0,    0,    0,
    0,    0,    0,    0,  453,    0,  457,  507,    0,    0,
    0,    0,  158,    0,    0,    0,  466,    0,    0,  475,
    0,  448,    0,  460,  450,  454,    0,  458,    0,  165,
    0,    0,  199,  200,    0,    0,  479,  459,    0,  505,
    0,    0,    0,  169,  467,  166,  161,  162,
  };
  protected static  short [] yyDgoto  = {             2,
    3,  343,   27,    4,    5,    6,    7,   42,   10,    0,
   28,  108,  222,  152,  287,    0,   29,   30,   31,   32,
   33,   34,   35,   36,   14,   22,   43,   23,   24,   44,
   45,   46,  162,  109,  246,    0,    0,  342,  367,  368,
  369,  370,  371,  372,  373,  374,  375,  376,  377,  378,
  418,  328,  294,  360,  329,  330,  339,  410,  411,  412,
  340,  556,  557,  676,  478,  479,  442,  443,  379,  423,
  472,  564,  505,  565,  345,  346,  347,  384,  385,  473,
  597,  655,  764,  598,  662,  599,  666,  724,  725,  325,
  293,  357,  326,  355,  393,  394,  395,  396,  397,  398,
  399,  435,  750,  703,  784,  810,  841,  453,  588,  454,
  455,  715,  716,  247,  708,  709,  710,  456,  292,  323,
  111,  349,  350,  351,  112,  165,  234,  113,  114,  115,
  116,  117,  118,  119,  120,  121,  122,  123,  124,  125,
  126,  127,  128,  129,  130,  131,  248,  249,  311,  243,
  132,  133,  363,  416,  235,  300,  301,  444,  134,  135,
  136,  137,  138,  139,  140,  141,  142,  143,  144,  145,
  146,  147,  148,  149,  680,  318,  288,   63,   64,  319,
  516,  457,  517,  518,  519,  520,  521,  522,  523,  524,
  525,  526,  527,  528,  529,  530,  531,  532,  533,  534,
  627,  621,  622,  535,  536,  537,  538,  775,  803,  804,
  805,  806,  827,  807,  539,  540,  541,  542,  671,  769,
  818,  672,  673,  820,  543,  544,  545,  546,  547,  613,
  687,  688,  689,  738,  690,  739,  691,  809,  695,
  };
  protected static  short [] yySindex = {         -287,
 -347,    0, -253, -287,    0,    0,    0, -202,    0,  -91,
  -69, -269, -250, -253,    0, -114,    0,  -83,    0,    0,
    0, -325,    0,  -54, -114, 4292,   98,    0, -250,    0,
    0,    0,    0,    0,    0,    0,    0,    0,   -5,    0,
 7928,    0,  -97,    0,    0,    0,    0,    0,  -69,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,   12, 4292,    0,    0,    0,    0,  -13,    0,    0,
    0,   17,    0,    0,    0,    0,    0,    0, 8595,    0,
    0,    0,    0,   24,    0,    0,    0,   41,    0,    0,
   93,    0, 7928, 7928, 7928, 7928, 7928, 7928, 7928, 7928,
 7928,    0,    0,    0,    0,    0,    0,  -69,   76,    0,
    0,    0,  -68,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
   31,    0,    0, 1411,    0,    0,    0,   40, -130,   58,
 -165,   51,   65,  105,   82,   75, -167,    0,    0,    0,
 -325,  120,   64, 8595,   74,   79,   80,    0, 7928,   81,
 7928,    0,   88,    0,    0, 8595, 8595, 7928,  106,  125,
  133,    0,    0,    0,    0,    0,    0,    0,    0,    0,
 7928, 7928, 6856,   84,    0,    0,   85, 7928, 7928, 7928,
 7928, 7928, 7928, 7928, 7928, 7928, 7928, 7928, 7928, 7928,
 7928, 7928, 7928, 7928, 7928, 8595, 8595, 7928, 7928, 7928,
 7928, 7928, 7928, 7928, 7928, 7928, 7928, 7928, 7928,    0,
 -287,  151,    0, -324,  157,    0,    0,    0,    2,    0,
  162, 7392, 6856,    0,  164, -321,   33,  163, 7928,    0,
 7928,    0,    4, 7928, 7928,    0,  165,  167,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,   40,   40, -130, -130,
  164,  164,   58,   58,   58,   58, -165, -165,   51,   65,
  105,   82,  170,   75, -250,    0,    0,  171,  173,  178,
   43,  177,  176,  180,    0, 7928,    0,    0,   13,  166,
  179,  185,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0, 6856, 7928,  184, 8595,  193,    0, -253,
    0, -253,  151, -114,  194,    0, 8595,  197,  180,    0,
    0,  164,    0,    0,    0,    0,    0,    0, 8595,  164,
 -253,  151, -170,  186,  192,    0,    0,  122,  201,  199,
    0,    0,    0,    0,  200, -253,    0, 8595, -253,  151,
    0,    0,  198,  164, 4135,    0,  203, -253,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  -96,    0,
    0, 8595,    0, 8595,    0,  202, -253,  195,    0, -157,
 -114,  211,  209, -253,    0,    0,    0,    0,    0, 4292,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  213,
 -253,    0,    0, 6990,    0,    0,  143, -217,    0,    0,
    0,    0,    0,    0, -322, -312,    0,    0,    0, 7928,
    0,    0,    0,    0, 8433,    0,    0,    0,    0,    0,
    0,    0,    0, -124,  216, 8595, 8595,  259,  260,  147,
  224, -270,  230,    0,  230,  232, 4532,    0,    0,    0,
 8595,  155, -305,    0, 7124,  234, -306, -303, 8595, 8595,
    0,  225,  228, -253,  587,  235,  226,   89,    0,   23,
  236,    0,    0, -253,  238,   29, 8595,  240, 4846,  248,
  249, -240,  254,  255, 7928,  258, 7928,  230,   36,  263,
  266,  274,    0,  271,    0,  266,  -16,    0,    0,    0,
    0, 1411,    0,    0,    0,  280, 4532,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  273,  275,  205,  277,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0, -302,  283,  288,
   52,    0,    0,  230,  278,   94,    0,   97,  294,  103,
  114, -253,  217,  291,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  298, -253, 7526,
  205,    0,    0, -253, -309,  127,  297, -253, -253,    0,
    0, -306,    0,  305, 7660, 8595, 7928,  295,  299, 7928,
 7928,    0,  303, 7928,  306,  -87,    0, 8062,  173,    0,
  266,    0, 7928, 4532,    0, 7392,    0,    0,    0,    0,
    0,  226,  309,    0,  310, -253, -253,    0, -253,    0,
 7928,  227,    0,    0, -253, 8595, 8595,  315,    0,  313,
 8595,  300,    0,    0, -253,  317,  323,  -80,    0,    0,
  329,    0,    0,  333,    0,    0,    0,  326,    0,    0,
  324,    0,  331, -300,    0,  327,    0,    0,    0,  335,
  336,    0,  337,    0,  342,  230,  403,    0,  421,  424,
    0, 7928,  -16, 7928,  347,  350,    0,  349,    0,    0,
  351,  352, -253,  357,    0,    0, -230,  362, -253, -253,
 -274, -267,    0, -234,    0,    0, -264,    0,    0,    0,
 -253,    0,    0,    0,  -80, 7928, 7928, 7928,  412,    0,
 4846, 4846,  368, 8595,    0,    0,   66,    0,    0,    0,
  230,    0,    0, 4846,    0, 4846,  360,  370,  128,    0,
  365,  230,  230,  367,  398,    0,  457,    0,  374,  375,
  377,  378,  134,  382,  381,    0,  380,    0,  379,    0,
 7928,  448,    0,   48,    0, -258,    0,    0,    0,    0,
    0,  384,  385,  386,    0,    0,    0,    0,    0,    0,
 6856, 6856,    0, 8595,    0,    0,  388, 7928,  393, 4846,
 7928,  383,  389,   48,    0,   48,    0,    0,  394, -253,
 -253, -253,    0,  395,  397, -241,    0,  399,  331,    0,
 4846,    0,  401,    0,    0,    0, 4532,    0,  230,    0,
  391,  415,    0,    0,  407, 4846,    0,    0, 4532,    0,
  414,  404,  411,    0,    0,    0,    0,    0,
  };
  protected static  short [] yyRindex = {         8851,
    0,    0, 8994, 8741,    0,    0,    0,   38,    0,    0,
 2162,   -4, 9036,  760,    0,    0,    0,    0,    0,    0,
    0,   19,    0,    0,    0,   22,    0,    0,  296,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  416,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0, 8132,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0, 1714,    0,    0,
    0,    0, 1871,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0, 2028,    0,    0,    0, 2296, 2698, 2832,
 4980, 5382, 5650, 5784, 6186, 6454, 6722,    0,    0,    0,
   19,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0, 8593,    0,
  417,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  419,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
 8895, 8796,    0,    0,  422,    0,    0,    0,    0,    0,
    0,  423,  419,    0, 1243,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  426,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0, 2430, 2564, 2966, 3100,
 3234, 3368, 3502, 3636, 3770, 3904, 5114, 5248, 5516, 5918,
 6052, 6320,    0, 6588, 8939,    0,    0,  427,  423,    0,
    0,    0,  428,  429,    0,    0,    0,    0,    0,    0,
  431,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0, 8376,
    0, -327,  841,    0,    0,    0,    0,    0,  436,    0,
    0, 1400,    0,    0,    0,    0,    0,    0,  437, 8452,
  922,  841, 8601,    0,  142,    0,    0,    0,    0,  438,
    0,    0,    0,    0,  440, 8238,    0, -161, 1084,  841,
    0,    0, 1557, 8509, 8214,    0,    0, 1003,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0, 8376,  -48,    0,  363,
    0, 8528,    0, 8290,    0,    0,    0,    0,    0, 8214,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
 1163,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
 -281,    0,    0,    0,    0,    0,  441,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  145,    0,    0, 8357,    0,    0,  -61,    0,    0,    0,
    0,    0,    0,  150,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  434,    0,  434,    0,    0,    0,
  371,    0,    0, 9137,    0, -299, 1224, 8993, 9016, 9075,
 9086,    0, 1385, 1542,    0,    0,  444,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0, 8357,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0, 8376,    0,
    0,    0,    0, 8357,    0,    0,    0,   77,  -41,    0,
    0,    0,    0,    0,  439,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  423,    0,
  376,    0,    0,    0,    0,  423,    0,    0,    0,    0,
    0, 7258, 7794,    0,    0, 8357, 8376,    0, 8357,    0,
    0,    0,    0,    0, -206,    0,    0,    0,    0,  446,
    0,    0,    0,    0,  150,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  447,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0, 4218,    0, 4061,    0,
    0,    0,  371,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  150,    0,    0,    0,    0,    0,  465,  532,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
 8376,    0,    0,    0,    0,    0,  449,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0, 4375,    0,  456,    0,  454,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  419,  419,    0,    0,    0,    0,    0,  464,    0,    0,
    0,    0,    0,  469,    0, 4689,    0,    0,    0,  150,
   78,  -26,    0,    0,    0,    0,    0,    0,  466,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0, -184,    0,
    0,    0,    0,    0,    0,    0,    0,    0,
  };
  protected static  short [] yyGindex = {            0,
  592,   -3,  533,    0,  813,    0,    0,   44,    0,    0,
    0,    8,    0,    0,  -75,    0,    0,  790, -159,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  669,
    0,    0,  -18,    0,  -25,    0,    0,    0,    0,    0,
  453, -215, -158, -156, -155, -154, -148, -146, -142,    0,
  796,    0,    0,    0,    0,  494,  497,    0,    0,  418,
  229,    0, -448, -624, -387,  237, -460,  467,    0,    0,
 -401, -488, -195, -317,    0,  451,  452,    0,    0, -311,
  181,    0,    0,  241,    0,  233,    0,  108,    0,    0,
    0,    0,    0,    0,    0,    0,  455,    0,    0,    0,
    0,    0,   32,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0, -232,    0,  124,  136,    0,    0,    0,
  555,    0,    0,  460,   67,    0, -227, -429,    0,    0,
    0, -418,    0,    0,    0,    5,   16,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  537,  602, -101,
   60,    0,    0,    0,    0, -436,    0,    0,   27,  161,
  169,    0,  119,  270,  204,  284,  639,  641,  643,  640,
  645,    0,    0,  191, -587,    0,    0,    0,  799,    0,
    0,    0,   42, -515,    0, -482,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0, -393,    0,    0,
 -225,    0,  244, -599,    0,    0,    0,    0,    0,    0,
   63,    0,    0,   62,    0,    0,    0,    0,    0,    0,
    0,    0,   72,    0,    0,    0,    0,    0,    0,  387,
    0,  187,    0,    0,    0,  182,  188,    0,    0,
  };
  protected static  short [] yyTable = {            13,
  302,  629,  344,   47,  553,  670,  604,  303,   11,   26,
   37,   19,  354,  657,  216,  110,  705,  550,  289,   41,
  289,  289,   16,   11,  304,   26,  607,  507,  761,   11,
  289,  752,   49,  475,  608,  698,  289,  289,  508,  289,
  289,   16,  289,   20,    9,    1,   69,  278,   70,   25,
  480,   71,  476,  153,  446,   33,   73,  229,  154,   39,
   74,   16,  155,  447,  448,   16,  559,  170,  289,   76,
  508,    8,  289,  648,  449,  289,   77,  156,  289,  433,
  558,   78,  456,  753,  289,   81,   11,  507,  762,   12,
  456,   33,   12,   38,  290,  206,  458,   82,  508,   83,
  169,  289,   85,  157,  362,  656,  459,   33,  699,  649,
   89,   90,  555,  551,   92,  477,  635,  450,  729,  429,
  172,  173,  174,  175,  176,  177,  178,  179,  770,  653,
  299,  207,   47,  228,  381,  231,  382,   16,  767,  768,
  481,  383,  238,  402,  759,  164,  633,  701,  477,   21,
  704,  760,   16,  667,  763,  242,  228,  456,   11,  171,
  808,   11,  252,  253,  254,  255,  256,  257,  258,  259,
  260,  261,  262,   11,   11,  507,  823,  835,  609,   76,
  685,  366,  696,  424,  431,   12,  508,   76,  693,  696,
  208,  209,  283,  706,  507,  402,  218,  686,  670,  401,
  403,  451,  404,  405,  406,  508,  228,  210,  366,  211,
  407,  669,  408,   11,   11,  219,  409,  464,  310,  310,
  164,  202,  203,  465,  694,  263,  264,  265,  266,  266,
  266,  266,  164,  164,  266,  266,  266,  266,  266,  266,
  266,  266,  266,  266,  421,  266,  150,  352,  772,  773,
  151,  401,  403,  422,  404,  405,  406,  482,   17,  483,
  421,  778,  407,  779,  408,  307,  380,  308,  409,  722,
  331,  652,  164,  164,  182,  620,  183,   18,  184,  120,
  625,   26,  153,  120,  413,   16,   99,  154,   99,  337,
  601,  155,  383,  220,   48,   33,  185,  383,  186,  220,
  127,  383,  616,  617,   38,  353,  156,  163,   70,  508,
   33,   71,  508,  508,  801,  160,  383,  822,  348,  702,
  267,  268,  802,  629,   11,  508,  626,  508,  183,  159,
  184,   11,  157,  160,   11,   40,   77,  365,  837,   36,
   36,   78,  383,   36,   67,  295,   11,  309,  185,  296,
  186,  296,  392,  845,   65,  400,  332,   82,  640,   83,
  296,  161,   47,  593,  365,   11,   47,  594,  166,  421,
   89,   90,  353,  161,   92,  289,  421,  187,  305,  508,
  168,  508,  224,  164,   16,  167,  348,   16,  441,   11,
  392,   11,  638,  164,  236,  237,  639,  507,   11,  199,
  200,  201,  508,  765,  460,  164,  421,  400,  508,  507,
  734,  273,  274,  275,  276,   33,   33,  508,  129,  159,
  508,  180,  214,  181,  164,   11,  204,  212,  205,  213,
  232,  353,  233,  353,  271,  272,  591,  168,  592,  441,
  216,  642,   11,  643,  591,  289,  644,  646,  164,  353,
  164,  239,   18,   11,   11,  217,  289,   11,  647,   11,
  221,  509,  723,  658,  782,  659,  783,  215,   11,  612,
  240,  612,  510,  269,  270,   11,   11,   11,  241,  793,
  596,  794,  223,  512,  164,  109,   33,  109,   33,  120,
  735,   16,  225,  509,   11,  277,  278,  226,  227,  230,
  286,  164,  250,  251,  510,  291,  289,  297,  306,  333,
  313,  434,  164,  164,  314,  512,  511,  322,  315,  317,
  298,  509,  320,  506,  324,  338,  334,  164,  327,  723,
  335,  386,  510,  341,  356,  164,  164,  359,  414,  387,
  388,  777,  389,  512,  419,  777,  390,  391,  511,  430,
  436,  427,   23,  164,  438,   33,  786,  787,  814,  815,
  466,  445,  469,  470,  441,  471,   33,  364,  474,  562,
  421,   33,  484,  549,  563,   33,  511,  589,   33,  554,
  590,  675,  595,  506,  679,  681,  364,  600,  683,  603,
   33,   33,  605,  606,  661,  664,   33,  679,  610,  611,
  228,   33,  614,   33,   33,   33,   33,  618,  619,  509,
  425,   33,  426,   11,   33,  675,   33,  513,  623,  624,
  510,  628,  630,  632,  631,  514,  634,  636,  509,   33,
  637,  512,  641,  840,  645,  649,  650,   23,  660,  510,
  668,  707,  651,  718,  677,  555,  452,  515,  678,  513,
  512,  596,  682,   11,   11,  684,  591,  514,   11,  700,
  713,  714,  720,  463,  511,  721,  170,  659,  743,  658,
  726,  506,  164,  727,  467,  468,  730,  513,  728,  515,
  731,  732,  733,  511,  506,  514,  734,  686,  737,  548,
  506,  741,  744,  745,  746,  748,  747,  560,  561,  749,
  679,  679,  751,  754,  771,  755,  757,  515,  774,  780,
  781,  753,  164,  164,  785,  602,  788,  164,  752,  789,
  790,  791,  792,  795,  796,  797,  800,  813,  798,  842,
  825,  824,  509,  811,  812,  509,  509,  817,  821,  829,
  833,   11,  834,  510,  836,  799,  510,  510,  509,  838,
  509,  843,  844,  847,  512,  846,   10,  512,  512,  510,
  848,  510,  212,  278,  280,  513,  305,  399,  138,   72,
  512,  281,  512,  514,  306,  675,   73,  401,   33,  217,
  140,   33,  404,  492,  513,  405,  197,  511,  468,  429,
  511,  511,  514,   33,  430,  515,  471,  451,  472,  506,
  164,   11,  509,  511,  509,  511,  749,  831,  832,  474,
  452,  476,  285,  510,  515,  510,   15,  316,   66,  220,
  420,   62,  361,  358,  512,  509,  512,  654,  439,  415,
  663,  509,  766,  758,  674,  719,  510,  428,  429,  665,
  509,  830,  510,  509,  756,  321,  312,  512,  437,  432,
  336,  510,  279,  512,  510,  280,  282,  511,  281,  511,
  164,  158,  512,  284,  697,  512,  826,  828,  839,  819,
  566,  742,    0,  736,  711,  712,  740,    0,    0,  717,
  511,    0,    0,  615,    0,    0,  511,    0,  513,    0,
    0,  513,  513,  506,    0,  511,  514,    0,  511,  514,
  514,    0,    0,    0,  513,  506,  513,    0,    0,    0,
    0,  567,  514,    0,  514,    0,    0,    0,  515,    0,
    0,  515,  515,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  515,    0,  515,  568,  569,  570,
  571,    0,  572,  573,  574,  575,  576,  577,  578,  579,
    0,  580,    0,  581,    0,  582,    0,  583,  513,  584,
  513,  585,  776,  586,    0,  587,  514,    0,  514,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  513,    0,    0,    0,    0,    0,  513,  515,  514,
  515,    0,    0,    0,    0,  514,  513,    0,    0,  513,
    0,    0,    0,    0,  514,    0,    0,  514,    0,    0,
    0,  515,    0,    0,    0,    0,   33,  515,    0,   33,
    0,   33,  816,   33,    0,   33,  515,    0,   33,  515,
   33,   33,    0,   33,    0,   33,    0,   33,    0,   33,
   33,   33,   33,    0,    0,    0,   33,    0,    0,    0,
    0,   33,    0,   33,   33,   33,    0,    0,   33,   33,
   33,    0,   33,    0,   33,   33,   33,   33,   33,   33,
   33,   33,    0,   33,   33,   33,   33,    0,   33,   33,
   33,    0,    0,    0,    0,    0,    0,   33,   33,    0,
    0,   33,    0,   33,   33,    0,   33,   12,   33,    0,
   12,    0,    0,    0,   12,    0,   12,    0,    0,   12,
   33,   12,   12,    0,   12,    0,   12,    0,   12,    0,
   12,   12,   12,   12,    0,    0,    0,   12,    0,    0,
    0,    0,   12,    0,   12,   12,   12,    0,    0,   12,
   12,   12,    0,   12,    0,    0,   12,    0,   12,   12,
   12,   12,    0,    0,    0,   12,   12,   12,    0,   12,
   12,   12,    0,    0,    0,    0,    0,    0,   12,   12,
    0,    0,   12,    0,   12,   12,    0,    0,   33,    0,
    0,   33,   12,   12,    0,   33,    0,   33,    0,    0,
   33,   12,   33,   33,    0,   33,    0,   33,    0,   33,
    0,   33,   33,   33,   33,    0,    0,    0,   33,    0,
    0,    0,    0,   33,    0,   33,   33,   33,    0,    0,
   33,    0,   33,    0,   33,    0,    0,   33,    0,   33,
   33,   33,   33,    0,    0,    0,   33,   33,   33,    0,
   33,   33,   33,    0,    0,    0,    0,    0,    0,   33,
   33,    0,    0,   33,    0,   33,   33,    0,    0,   12,
    0,    0,   33,   56,    0,    0,   33,    0,   33,    0,
    0,   33,   33,   33,   33,    0,   33,    0,   33,    0,
   33,    0,   33,   33,   33,   33,    0,    0,    0,   33,
    0,    0,    0,    0,   33,    0,   33,   33,   33,    0,
    0,   33,    0,   33,    0,   33,    0,    0,   33,    0,
   33,   33,   33,   33,    0,    0,    0,   33,   33,   33,
    0,   33,   33,   33,    0,    0,    0,    0,    0,    0,
   33,   33,    0,    0,   33,    0,   33,   33,    0,    0,
   33,    0,    0,   33,   57,    0,    0,   33,    0,   33,
    0,    0,   33,   33,   33,   33,    0,   33,    0,   33,
    0,   33,    0,   33,   33,   33,   33,    0,    0,    0,
   33,    0,    0,    0,    0,   33,    0,   33,   33,   33,
    0,    0,   33,    0,   33,    0,   33,    0,    0,   33,
    0,   33,   33,   33,   33,    0,    0,    0,   33,   33,
   33,    0,   33,   33,   33,    0,    0,    0,    0,    0,
    0,   33,   33,    0,    0,   33,    0,   33,   33,    0,
    0,   33,   33,    0,    0,   78,   33,    0,   33,    0,
    0,   33,    0,   33,   33,    0,   33,    0,   33,    0,
   33,    0,   33,   33,   33,   33,    0,    0,    0,   33,
    0,    0,    0,    0,   33,    0,   33,   33,   33,    0,
    0,   33,    0,   33,    0,   33,    0,    0,   33,    0,
   33,   33,   33,   33,    0,    0,    0,   33,   33,   33,
    0,   33,   33,   33,    0,    0,    0,    0,    0,    0,
   33,   33,    0,    0,   33,    0,   33,   33,    0,    0,
    0,    0,   33,  302,   79,  302,  302,    0,  302,    0,
    0,  302,  302,    0,    0,    0,  302,    0,    0,    0,
  302,    0,    0,    0,    0,    0,  302,    0,    0,  302,
    0,    0,    0,    0,    0,    0,  302,    0,    0,  302,
    0,  302,    0,  302,  302,  302,  302,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  302,    0,  302,
  302,    0,  302,    0,    0,  302,    0,  302,    0,  302,
  302,  302,  302,    0,  302,    0,    0,    0,  320,    0,
    0,   33,    0,  302,  302,    0,  302,  302,  302,  302,
  302,  302,  302,  302,  302,  302,  302,  302,  302,  302,
  302,  302,  302,  302,  302,  302,  302,  302,  320,  302,
  320,  302,  320,  302,  320,  302,  320,  302,  320,  302,
  320,  302,  320,  302,  320,  302,  320,  302,    0,  302,
    0,  302,    0,  302,    0,  302,    0,  302,    0,  302,
    0,  302,  429,  302,    0,  302,    0,    0,    0,  302,
    0,  302,    0,  302,    0,  302,    0,  302,    0,  302,
  300,  302,  300,  300,    0,  300,    0,    0,  300,  300,
    0,    0,    0,  300,    0,    0,    0,  300,    0,    0,
    0,    0,    0,  300,    0,    0,  300,    0,    0,    0,
    0,    0,    0,  300,    0,    0,  300,    0,  300,    0,
  300,  300,  300,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  300,    0,  300,  300,    0,  300,
    0,    0,  300,    0,  300,    0,  300,  300,  300,  300,
  442,  300,  442,    0,  442,    0,    0,    0,    0,  327,
  300,  300,    0,  300,  300,  300,  300,  300,  300,  300,
  300,  300,  300,  300,  300,  300,  300,  300,  300,  300,
  300,  300,  300,  300,  300,  188,  300,    0,  300,  327,
  300,  327,  300,  327,  300,  327,  300,  327,  300,  327,
  300,  327,  300,  327,  300,  327,  300,  327,  300,    0,
  300,    0,  300,    0,  300,  189,  300,  190,  300,  191,
  300,  192,  300,  193,    0,  194,  300,  195,  300,  196,
  300,  197,  300,  198,  300,    0,  300,  309,  300,  309,
  309,    0,  309,    0,    0,  309,  309,    0,    0,    0,
  309,    0,    0,    0,  309,    0,    0,    0,    0,    0,
  309,    0,    0,  309,    0,    0,    0,    0,    0,    0,
  309,    0,    0,  309,    0,  309,    0,  309,  309,  309,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  309,    0,  309,  309,    0,  309,    0,    0,  309,
    0,  309,    0,  309,  309,  309,  309,  443,  309,  443,
    0,  443,    0,    0,    0,    0,  328,    0,  309,  309,
  309,  309,  309,  309,  309,  309,  309,  309,  309,  309,
  309,  309,  309,  309,  309,  309,  309,  309,  309,  309,
  309,  309,    0,  309,    0,  309,  328,  309,  328,  309,
  328,  309,  328,  309,  328,  309,  328,  309,  328,  309,
  328,  309,  328,  309,  328,  309,    0,  309,    0,  309,
    0,  309,    0,  309,    0,  309,    0,  309,    0,  309,
    0,    0,    0,  309,    0,  309,    0,  309,    0,  309,
    0,  309,    0,  309,  249,  309,  249,  249,    0,  249,
    0,    0,  249,  249,    0,    0,    0,  249,    0,    0,
    0,  249,    0,    0,    0,    0,    0,  249,    0,    0,
  249,    0,    0,    0,    0,    0,    0,  249,    0,    0,
  249,    0,  249,    0,  249,  249,  249,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  249,    0,
  249,  249,    0,  249,    0,    0,  249,    0,  249,    0,
  249,  249,  249,  249,    0,  249,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  249,  249,  249,  249,  249,
    0,  249,  249,  249,  249,  249,  249,  249,  249,  249,
  249,  249,  249,  249,  249,  249,  249,  249,  249,    0,
  249,    0,  249,    0,  249,    0,  249,    0,  249,    0,
  249,    0,  249,    0,  249,    0,  249,    0,  249,    0,
  249,    0,  249,    0,  249,    0,  249,    0,  249,    0,
  249,    0,  249,    0,  249,    0,  249,    0,    0,    0,
  249,    0,  249,    0,  249,    0,  249,    0,  249,    0,
  249,  320,  249,  320,  320,    0,  320,    0,    0,  320,
  320,    0,    0,    0,  320,    0,    0,    0,  320,    0,
    0,    0,    0,    0,  320,    0,    0,  320,    0,    0,
    0,    0,    0,    0,  320,    0,    0,  320,    0,  320,
    0,  320,  320,  320,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  320,    0,  320,  320,    0,
  320,    0,    0,  320,    0,  320,    0,  320,  320,  320,
  320,    0,  320,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  320,    0,  320,    0,  320,    0,  320,  320,
  320,  320,  320,  320,  320,  320,  320,  320,  320,  320,
  320,  320,  320,  320,  320,    0,    0,    0,    0,  320,
    0,  320,    0,  320,    0,  320,    0,  320,    0,  320,
    0,  320,    0,  320,    0,  320,    0,  320,    0,  320,
    0,  320,    0,  320,    0,  320,    0,  320,    0,  320,
    0,  320,    0,  320,    0,    0,    0,  320,    0,  320,
    0,  320,    0,  320,    0,  320,    0,  320,  334,  320,
  334,  334,    0,  334,    0,    0,  334,  334,    0,    0,
    0,  334,    0,    0,    0,  334,    0,    0,    0,    0,
    0,  334,    0,    0,  334,    0,    0,    0,    0,    0,
    0,  334,    0,    0,  334,    0,  334,    0,  334,  334,
  334,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  334,    0,  334,  334,    0,  334,    0,    0,
  334,    0,  334,    0,  334,  334,  334,  334,    0,  334,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  334,
    0,  334,  334,  334,    0,  334,  334,  334,  334,  334,
  334,  334,    0,  334,  334,  334,  334,  334,  334,  334,
  334,  334,  334,    0,  334,    0,  334,    0,  334,    0,
  334,    0,  334,    0,  334,    0,  334,    0,  334,    0,
  334,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  224,    0,  224,  224,    0,  224,    0,    0,
  224,  224,    0,    0,  334,  224,  334,    0,  334,  224,
  334,    0,  334,    0,  334,  224,  334,    0,  224,    0,
    0,    0,    0,    0,    0,  224,    0,    0,  224,    0,
  224,    0,  224,  224,  224,  224,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  224,    0,  224,  224,
    0,  224,    0,    0,  224,    0,  224,    0,  224,  224,
  224,  224,    0,  224,    0,    0,    0,    0,    0,    0,
    0,    0,  224,  224,  224,  224,  224,  224,    0,  224,
  224,  224,  224,  224,  224,  224,    0,  224,  224,  224,
  224,  224,    0,    0,  224,  224,  224,    0,  224,    0,
    0,    0,    0,    0,  224,    0,  224,    0,  224,    0,
  224,    0,  224,    0,  224,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  338,    0,  338,  338,
    0,  338,    0,    0,  338,  338,    0,    0,  224,  338,
  224,    0,  224,  338,  224,    0,  224,    0,  224,  338,
  224,    0,  338,    0,    0,    0,    0,    0,    0,  338,
    0,    0,  338,    0,  338,    0,  338,  338,  338,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  338,    0,  338,  338,    0,  338,    0,    0,  338,    0,
  338,    0,  338,  338,  338,  338,    0,  338,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  338,    0,  338,
  338,  338,    0,  338,  338,  338,  338,  338,  338,  338,
    0,  338,  338,  338,  338,    0,    0,    0,  338,  338,
  338,    0,  338,    0,  338,    0,  338,    0,  338,    0,
  338,    0,  338,    0,  338,    0,  338,    0,  338,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  339,    0,  339,  339,    0,  339,    0,    0,  339,  339,
    0,    0,  338,  339,  338,    0,  338,  339,  338,    0,
  338,    0,  338,  339,  338,    0,  339,    0,    0,    0,
    0,    0,    0,  339,    0,    0,  339,    0,  339,    0,
  339,  339,  339,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  339,    0,  339,  339,    0,  339,
    0,    0,  339,    0,  339,    0,  339,  339,  339,  339,
    0,  339,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  339,    0,  339,  339,  339,    0,  339,  339,  339,
  339,  339,  339,  339,    0,  339,  339,  339,  339,    0,
    0,    0,  339,  339,  339,    0,  339,    0,  339,    0,
  339,    0,  339,    0,  339,    0,  339,    0,  339,    0,
  339,    0,  339,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  340,    0,  340,  340,    0,  340,
    0,    0,  340,  340,    0,    0,  339,  340,  339,    0,
  339,  340,  339,    0,  339,    0,  339,  340,  339,    0,
  340,    0,    0,    0,    0,    0,    0,  340,    0,    0,
  340,    0,  340,    0,  340,  340,  340,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  340,    0,
  340,  340,    0,  340,    0,    0,  340,    0,  340,    0,
  340,  340,  340,  340,    0,  340,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  340,    0,  340,  340,  340,
    0,  340,  340,  340,  340,  340,  340,  340,    0,  340,
  340,  340,  340,    0,    0,    0,  340,  340,  340,    0,
  340,    0,  340,    0,  340,    0,  340,    0,  340,    0,
  340,    0,  340,    0,  340,    0,  340,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  341,    0,
  341,  341,    0,  341,    0,    0,  341,  341,    0,    0,
  340,  341,  340,    0,  340,  341,  340,    0,  340,    0,
  340,  341,  340,    0,  341,    0,    0,    0,    0,    0,
    0,  341,    0,    0,  341,    0,  341,    0,  341,  341,
  341,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  341,    0,  341,  341,    0,  341,    0,    0,
  341,    0,  341,    0,  341,  341,  341,  341,    0,  341,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  341,
    0,  341,  341,  341,    0,  341,  341,  341,  341,    0,
    0,  341,    0,  341,  341,  341,  341,  341,    0,    0,
  341,  341,  341,    0,  341,    0,  341,    0,  341,    0,
  341,    0,  341,    0,  341,    0,  341,    0,  341,    0,
  341,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  344,    0,  344,  344,    0,  344,    0,    0,
  344,  344,    0,    0,  341,  344,  341,    0,  341,  344,
  341,    0,  341,    0,  341,  344,  341,    0,  344,    0,
    0,    0,    0,    0,    0,  344,    0,    0,  344,    0,
  344,    0,  344,  344,  344,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  344,    0,  344,  344,
    0,  344,    0,    0,  344,    0,  344,    0,  344,  344,
  344,  344,    0,  344,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  344,    0,  344,  344,  344,    0,  344,
  344,  344,  344,  344,  344,  344,    0,  344,  344,  344,
  344,  344,    0,    0,  344,  344,  344,    0,  344,    0,
    0,    0,    0,    0,  344,    0,  344,    0,  344,    0,
  344,    0,  344,    0,  344,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  342,    0,  342,  342,
    0,  342,    0,    0,  342,  342,    0,    0,  344,  342,
  344,    0,  344,  342,  344,    0,  344,    0,  344,  342,
  344,    0,  342,    0,    0,    0,    0,    0,    0,  342,
    0,    0,  342,    0,  342,    0,  342,  342,  342,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  342,    0,  342,  342,    0,  342,    0,    0,  342,    0,
  342,    0,  342,  342,  342,  342,    0,  342,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  342,    0,  342,
  342,  342,    0,  342,  342,  342,  342,    0,    0,  342,
    0,  342,  342,  342,  342,  342,    0,    0,  342,  342,
  342,    0,  342,    0,  342,    0,  342,    0,  342,    0,
  342,    0,  342,    0,  342,    0,  342,    0,  342,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  343,    0,  343,  343,    0,  343,    0,    0,  343,  343,
    0,    0,  342,  343,  342,    0,  342,  343,  342,    0,
  342,    0,  342,  343,  342,    0,  343,    0,    0,    0,
    0,    0,    0,  343,    0,    0,  343,    0,  343,    0,
  343,  343,  343,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  343,    0,  343,  343,    0,  343,
    0,    0,  343,    0,  343,    0,  343,  343,  343,  343,
    0,  343,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  343,    0,  343,  343,  343,    0,  343,  343,  343,
  343,    0,    0,  343,    0,  343,  343,  343,  343,  343,
    0,    0,  343,  343,  343,    0,  343,    0,  343,    0,
  343,    0,  343,    0,  343,    0,  343,    0,  343,    0,
  343,    0,  343,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  350,    0,  350,  350,    0,  350,
    0,    0,  350,  350,    0,    0,  343,  350,  343,    0,
  343,  350,  343,    0,  343,    0,  343,  350,  343,    0,
  350,    0,    0,    0,    0,    0,    0,  350,    0,    0,
  350,    0,  350,    0,  350,  350,  350,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  350,    0,
  350,  350,    0,  350,    0,    0,  350,    0,  350,    0,
  350,  350,  350,  350,    0,  350,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  350,    0,  350,  350,  350,
    0,  350,  350,  350,  350,  350,  350,  350,    0,  350,
  350,  350,  350,  350,    0,    0,  350,  350,  350,    0,
  350,    0,    0,    0,    0,    0,  350,    0,  350,    0,
  350,    0,  350,    0,  350,    0,  350,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  349,    0,
  349,  349,    0,  349,    0,    0,  349,  349,    0,    0,
  350,  349,  350,    0,  350,  349,  350,    0,  350,    0,
  350,  349,  350,    0,  349,    0,    0,    0,    0,    0,
    0,  349,    0,    0,  349,    0,  349,    0,  349,  349,
  349,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  349,    0,  349,  349,    0,  349,    0,    0,
  349,    0,  349,    0,  349,  349,  349,  349,    0,  349,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  349,
    0,  349,  349,  349,    0,  349,  349,  349,  349,  349,
  349,  349,    0,  349,  349,  349,  349,  349,    0,    0,
  349,  349,  349,    0,  349,    0,    0,    0,    0,    0,
  349,    0,  349,    0,  349,    0,  349,    0,  349,    0,
  349,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  345,    0,  345,  345,    0,  345,    0,    0,
  345,  345,    0,    0,  349,  345,  349,    0,  349,  345,
  349,    0,  349,    0,  349,  345,  349,    0,  345,    0,
    0,    0,    0,    0,    0,  345,    0,    0,  345,    0,
  345,    0,  345,  345,  345,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  345,    0,  345,  345,
    0,  345,    0,    0,  345,    0,  345,    0,  345,  345,
  345,  345,    0,  345,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  345,    0,  345,  345,  345,    0,  345,
  345,  345,  345,  345,  345,  345,    0,  345,  345,  345,
  345,  345,    0,    0,  345,  345,  345,    0,  345,    0,
    0,    0,    0,    0,  345,    0,  345,    0,  345,    0,
  345,    0,  345,    0,  345,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  346,    0,  346,  346,
    0,  346,    0,    0,  346,  346,    0,    0,  345,  346,
  345,    0,  345,  346,  345,    0,  345,    0,  345,  346,
  345,    0,  346,    0,    0,    0,    0,    0,    0,  346,
    0,    0,  346,    0,  346,    0,  346,  346,  346,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  346,    0,  346,  346,    0,  346,    0,    0,  346,    0,
  346,    0,  346,  346,  346,  346,    0,  346,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  346,    0,  346,
  346,  346,    0,  346,  346,  346,  346,  346,  346,  346,
    0,  346,  346,  346,  346,  346,    0,    0,  346,  346,
  346,    0,  346,    0,    0,    0,    0,    0,  346,    0,
  346,    0,  346,    0,  346,    0,  346,    0,  346,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  347,    0,  347,  347,    0,  347,    0,    0,  347,  347,
    0,    0,  346,  347,  346,    0,  346,  347,  346,    0,
  346,    0,  346,  347,  346,    0,  347,    0,    0,    0,
    0,    0,    0,  347,    0,    0,  347,    0,  347,    0,
  347,  347,  347,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  347,    0,  347,  347,    0,  347,
    0,    0,  347,    0,  347,    0,  347,  347,  347,  347,
    0,  347,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  347,    0,  347,  347,  347,    0,  347,  347,  347,
  347,  347,  347,  347,    0,  347,  347,  347,  347,  347,
    0,    0,  347,  347,  347,    0,  347,    0,    0,    0,
    0,    0,  347,    0,  347,    0,  347,    0,  347,    0,
  347,    0,  347,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  348,    0,  348,  348,    0,  348,
    0,    0,  348,  348,    0,    0,  347,  348,  347,    0,
  347,  348,  347,    0,  347,    0,  347,  348,  347,    0,
  348,    0,    0,    0,    0,    0,    0,  348,    0,    0,
  348,    0,  348,    0,  348,  348,  348,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  348,    0,
  348,  348,    0,  348,    0,    0,  348,    0,  348,    0,
  348,  348,  348,  348,    0,  348,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  348,    0,  348,  348,  348,
    0,  348,  348,  348,  348,  348,  348,  348,    0,  348,
  348,  348,  348,  348,    0,    0,  348,  348,  348,    0,
  348,    0,    0,    0,    0,    0,  348,    0,  348,    0,
  348,    0,  348,    0,  348,    0,  348,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  348,    0,  348,    0,  348,    0,  348,    0,  348,    0,
  348,    0,  348,  499,  499,  499,  499,  499,    0,  499,
  499,    0,  499,  499,  499,  499,    0,  499,  499,  499,
    0,    0,    0,    0,  499,  499,    0,  499,  499,  499,
  499,  499,    0,    0,  499,    0,    0,    0,  499,  499,
    0,  499,  499,  499,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  499,    0,  499,    0,  499,  499,    0,
  499,    0,  499,  499,  499,  499,  499,  499,  499,  499,
  499,    0,  499,  499,   50,  499,  499,    0,    0,    0,
    0,  499,  499,    0,    0,  499,    0,    0,    0,    0,
  499,  499,  499,  499,  499,    0,    0,   51,  499,    0,
  499,    0,    0,    0,    0,  499,    0,  499,    0,    0,
   52,    0,    0,    0,    0,   53,    0,    0,    0,    0,
   54,    0,   55,   56,   57,   58,    0,    0,    0,    0,
   59,    0,    0,   60,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  499,   61,  499,
    0,  499,    0,  499,    0,  499,    0,  499,    0,  499,
  494,  494,  494,  494,  494,  417,  494,  494,    0,  494,
  494,  494,  494,    0,  494,  494,  494,    0,    0,    0,
    0,  494,    0,    0,  494,  494,  494,  494,  494,    0,
    0,  494,    0,    0,    0,  494,  494,    0,  494,  494,
  494,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  494,    0,  494,    0,  494,  494,    0,  494,    0,  494,
  494,  494,  494,  494,  494,  494,  494,  494,    0,  494,
  494,   50,  494,  494,    0,    0,    0,    0,  494,  494,
    0,    0,  494,    0,    0,    0,    0,  494,  494,  494,
  494,  494,    0,    0,   51,  494,    0,  494,    0,    0,
    0,    0,  494,    0,  494,    0,    0,   52,    0,    0,
    0,    0,   53,    0,    0,    0,    0,   54,    0,   55,
   56,   57,   58,    0,    0,    0,    0,   59,    0,    0,
   60,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  494,   61,  494,    0,  494,    0,
  494,    0,  494,    0,  494,    0,  494,  447,  447,  447,
  447,  447,    0,  447,  447,    0,  447,  447,  447,  447,
    0,  447,  447,    0,    0,    0,    0,    0,  447,    0,
    0,  447,  447,  447,  447,  447,    0,    0,  447,    0,
    0,    0,  447,  447,    0,  447,  447,  447,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  447,    0,  447,
    0,  447,  447,    0,  447,    0,  447,  447,  447,  447,
  447,  447,  447,  447,  447,    0,  447,  447,    0,  447,
  447,    0,    0,    0,    0,  447,  447,    0,    0,  447,
    0,    0,    0,    0,  447,  447,  447,  447,  447,    0,
    0,    0,  447,    0,  447,    0,    0,    0,    0,  447,
    0,  447,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  447,    0,  447,    0,  447,    0,  447,    0,  447,
    0,  447,    0,  447,   68,   69,  485,   70,    0,    0,
   71,  486,    0,  487,  488,   73,    0,    0,  489,   74,
    0,    0,    0,    0,    0,   75,    0,    0,   76,  490,
  491,  492,  493,    0,    0,   77,    0,    0,    0,  494,
   78,    0,   79,   80,   81,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  495,    0,   82,    0,   83,   84,
    0,   85,    0,  496,   86,  497,   87,  498,   88,   89,
   90,  499,    0,   92,  500,    0,  501,  502,    0,    0,
    0,    0,  421,    0,    0,    0,   93,    0,    0,    0,
    0,  503,   94,   95,   96,   97,    0,    0,    0,   98,
    0,   99,    0,    0,    0,    0,  100,    0,  101,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  102,    0,
  103,    0,  104,    0,  105,    0,  106,    0,  107,    0,
  504,  455,  455,  455,  455,    0,    0,  455,  455,    0,
  455,  455,  455,    0,    0,  455,  455,    0,    0,    0,
    0,    0,  455,    0,    0,  455,  455,  455,  455,  455,
    0,    0,  455,    0,    0,    0,  455,  455,    0,  455,
  455,  455,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  455,    0,  455,    0,  455,  455,    0,  455,    0,
  455,  455,  455,  455,  455,  455,  455,  455,  455,    0,
  455,  455,    0,  455,  455,    0,    0,    0,    0,  455,
    0,    0,    0,  455,    0,    0,    0,    0,  455,  455,
  455,  455,  455,    0,    0,    0,  455,    0,  455,    0,
    0,    0,    0,  455,    0,  455,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  455,    0,  455,    0,  455,
    0,  455,    0,  455,    0,  455,    0,  455,   68,   69,
  485,   70,    0,    0,   71,  486,    0,    0,  488,   73,
    0,    0,  489,   74,    0,    0,    0,    0,    0,   75,
    0,    0,   76,  490,  491,  492,  493,    0,    0,   77,
    0,    0,    0,  494,   78,    0,   79,   80,   81,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  495,    0,
   82,    0,   83,   84,    0,   85,    0,  496,   86,  497,
   87,  498,   88,   89,   90,  499,    0,   92,  500,    0,
    0,  502,    0,    0,    0,    0,  421,    0,    0,    0,
   93,    0,    0,    0,    0,  503,   94,   95,   96,   97,
    0,    0,    0,   98,    0,   99,    0,    0,    0,    0,
  100,    0,  101,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  351,  351,    0,  351,    0,    0,  351,  351,
    0,    0,  102,  351,  103,    0,  104,  351,  105,    0,
  106,    0,  107,  351,   38,    0,  351,    0,    0,    0,
    0,    0,    0,  351,    0,    0,    0,    0,  351,    0,
  351,  351,  351,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  351,    0,  351,  351,    0,  351,
    0,    0,  351,    0,  351,    0,  351,  351,  351,  351,
    0,  351,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  351,    0,  351,  351,  351,    0,  351,  351,  351,
  351,  351,  351,  351,    0,    0,    0,  351,  351,  351,
    0,    0,  351,  351,  351,    0,  351,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  351,    0,  351,    0,
  351,    0,  351,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  352,  352,    0,  352,
    0,    0,  352,  352,    0,    0,  351,  352,  351,    0,
  351,  352,  351,    0,  351,    0,  351,  352,  351,    0,
  352,    0,    0,    0,    0,    0,    0,  352,    0,    0,
    0,    0,  352,    0,  352,  352,  352,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  352,    0,
  352,  352,    0,  352,    0,    0,  352,    0,  352,    0,
  352,  352,  352,  352,    0,  352,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  352,    0,  352,  352,  352,
    0,  352,  352,  352,  352,  352,  352,  352,    0,    0,
    0,  352,  352,  352,    0,    0,  352,  352,  352,    0,
  352,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  352,    0,  352,    0,  352,    0,  352,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  353,  353,    0,  353,    0,    0,  353,  353,    0,    0,
  352,  353,  352,    0,  352,  353,  352,    0,  352,    0,
  352,  353,  352,    0,  353,    0,    0,    0,    0,    0,
    0,  353,    0,    0,    0,    0,  353,    0,  353,  353,
  353,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  353,    0,  353,  353,    0,  353,    0,    0,
  353,    0,  353,    0,  353,  353,  353,  353,    0,  353,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  353,
    0,  353,  353,  353,    0,  353,  353,  353,  353,  353,
  353,  353,    0,    0,    0,  353,  353,  353,    0,    0,
  353,  353,  353,    0,  353,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  353,    0,  353,    0,  353,    0,
  353,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  354,  354,    0,  354,    0,    0,
  354,  354,    0,    0,  353,  354,  353,    0,  353,  354,
  353,    0,  353,    0,  353,  354,  353,    0,  354,    0,
    0,    0,    0,    0,    0,  354,    0,    0,    0,    0,
  354,    0,  354,  354,  354,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  354,    0,  354,  354,
    0,  354,    0,    0,  354,    0,  354,    0,  354,  354,
  354,  354,    0,  354,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  354,    0,  354,  354,  354,    0,  354,
  354,  354,  354,  354,  354,  354,    0,    0,    0,  354,
  354,  354,    0,    0,  354,  354,  354,    0,  354,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  354,    0,  354,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  355,  355,
    0,  355,    0,    0,  355,  355,    0,    0,  354,  355,
  354,    0,  354,  355,  354,    0,  354,    0,  354,  355,
  354,    0,  355,    0,    0,    0,    0,    0,    0,  355,
    0,    0,    0,    0,  355,    0,  355,  355,  355,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  355,    0,  355,  355,    0,  355,    0,    0,  355,    0,
  355,    0,  355,  355,  355,  355,    0,  355,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  355,    0,  355,
  355,  355,    0,  355,  355,  355,  355,  355,  355,  355,
    0,    0,    0,  355,  355,  355,    0,    0,  355,  355,
  355,    0,  355,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  355,    0,  355,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  356,  356,    0,  356,    0,    0,  356,  356,
    0,    0,  355,  356,  355,    0,  355,  356,  355,    0,
  355,    0,  355,  356,  355,    0,  356,    0,    0,    0,
    0,    0,    0,  356,    0,    0,    0,    0,  356,    0,
  356,  356,  356,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  356,    0,  356,  356,    0,  356,
    0,    0,  356,    0,  356,    0,  356,  356,  356,  356,
    0,  356,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  356,    0,  356,  356,  356,    0,  356,  356,  356,
  356,  356,  356,  356,    0,    0,    0,    0,  356,  356,
    0,    0,  356,  356,  356,    0,  356,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  356,    0,  356,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  358,  358,    0,  358,
    0,    0,  358,  358,    0,    0,  356,  358,  356,    0,
  356,  358,  356,    0,  356,    0,  356,  358,  356,    0,
  358,    0,    0,    0,    0,    0,    0,  358,    0,    0,
    0,    0,  358,    0,  358,  358,  358,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  358,    0,
  358,  358,    0,  358,    0,    0,  358,    0,  358,    0,
  358,  358,  358,  358,    0,  358,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  358,    0,  358,  358,  358,
    0,  358,  358,  358,  358,  358,  358,  358,    0,    0,
    0,  358,  358,  358,    0,    0,    0,  358,  358,    0,
  358,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  358,    0,  358,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  357,  357,    0,  357,    0,    0,  357,  357,    0,    0,
  358,  357,  358,    0,  358,  357,  358,    0,  358,    0,
  358,  357,  358,    0,  357,    0,    0,    0,    0,    0,
    0,  357,    0,    0,    0,    0,  357,    0,  357,  357,
  357,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  357,    0,  357,  357,    0,  357,    0,    0,
  357,    0,  357,    0,  357,  357,  357,  357,    0,  357,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  357,
    0,  357,  357,  357,    0,  357,  357,  357,  357,  357,
  357,  357,    0,    0,    0,    0,  357,  357,    0,    0,
  357,  357,  357,    0,  357,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  357,    0,
  357,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  359,  359,    0,  359,    0,    0,
  359,  359,    0,    0,  357,  359,  357,    0,  357,  359,
  357,    0,  357,    0,  357,  359,  357,    0,  359,    0,
    0,    0,    0,    0,    0,  359,    0,    0,    0,    0,
  359,    0,  359,  359,  359,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  359,    0,  359,  359,
    0,  359,    0,    0,  359,    0,  359,    0,  359,  359,
  359,  359,    0,  359,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  359,    0,  359,  359,  359,    0,  359,
  359,  359,  359,  359,  359,  359,    0,    0,    0,  359,
  359,  359,    0,    0,    0,  359,  359,    0,  359,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  359,    0,  359,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  360,  360,
    0,  360,    0,    0,  360,  360,    0,    0,  359,  360,
  359,    0,  359,  360,  359,    0,  359,    0,  359,  360,
  359,    0,  360,    0,    0,    0,    0,    0,    0,  360,
    0,    0,    0,    0,  360,    0,  360,  360,  360,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  360,    0,  360,  360,    0,  360,    0,    0,  360,    0,
  360,    0,  360,  360,  360,  360,    0,  360,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  360,    0,  360,
  360,  360,    0,  360,  360,  360,  360,  360,  360,  360,
    0,    0,    0,  360,    0,  360,    0,    0,    0,  360,
  360,    0,  360,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  360,    0,  360,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  361,  361,    0,  361,    0,    0,  361,  361,
    0,    0,  360,  361,  360,    0,  360,  361,  360,    0,
  360,    0,  360,  361,  360,    0,  361,    0,    0,    0,
    0,    0,    0,  361,    0,    0,    0,    0,  361,    0,
  361,  361,  361,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  361,    0,  361,  361,    0,  361,
    0,    0,  361,    0,  361,    0,  361,  361,  361,  361,
    0,  361,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  361,    0,  361,  361,  361,    0,  361,  361,  361,
  361,  361,  361,  361,    0,    0,    0,  361,    0,  361,
    0,    0,    0,  361,  361,    0,  361,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  361,    0,  361,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  362,  362,    0,  362,
    0,    0,  362,  362,    0,    0,  361,  362,  361,    0,
  361,  362,  361,    0,  361,    0,  361,  362,  361,    0,
  362,    0,    0,    0,    0,    0,    0,  362,    0,    0,
    0,    0,  362,    0,  362,  362,  362,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  362,    0,
  362,  362,    0,  362,    0,    0,  362,    0,  362,    0,
  362,  362,  362,  362,    0,  362,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  362,    0,  362,  362,  362,
    0,  362,  362,  362,  362,  362,  362,  362,    0,    0,
    0,  362,    0,  362,    0,    0,    0,  362,  362,    0,
  362,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  362,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  363,  363,    0,  363,    0,    0,  363,  363,    0,    0,
  362,  363,  362,    0,  362,  363,  362,    0,  362,    0,
  362,  363,  362,    0,  363,    0,    0,    0,    0,    0,
    0,  363,    0,    0,    0,    0,  363,    0,  363,  363,
  363,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  363,    0,  363,  363,    0,  363,    0,    0,
  363,    0,  363,    0,  363,  363,  363,  363,    0,  363,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  363,
    0,  363,  363,  363,    0,  363,  363,  363,  363,  363,
  363,  363,    0,    0,    0,  363,    0,  363,    0,    0,
    0,  363,  363,    0,  363,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  363,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  364,  364,    0,  364,    0,    0,
  364,  364,    0,    0,  363,  364,  363,    0,  363,  364,
  363,    0,  363,    0,  363,  364,  363,    0,  364,    0,
    0,    0,    0,    0,    0,  364,    0,    0,    0,    0,
  364,    0,  364,  364,  364,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  364,    0,  364,  364,
    0,  364,    0,    0,  364,    0,  364,    0,  364,  364,
  364,  364,    0,  364,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  364,    0,  364,  364,  364,    0,  364,
  364,  364,  364,  364,  364,  364,    0,    0,    0,  364,
    0,  364,    0,    0,    0,    0,  364,    0,  364,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,   68,   69,
    0,   70,    0,    0,   71,   72,    0,    0,  364,   73,
  364,    0,  364,   74,  364,    0,  364,    0,  364,   75,
  364,    0,   76,    0,    0,    0,    0,    0,    0,   77,
    0,    0,    0,    0,   78,    0,   79,   80,   81,    0,
  244,    0,    0,    0,    0,    0,    0,  245,    0,    0,
   82,    0,   83,   84,    0,   85,    0,    0,   86,    0,
   87,    0,   88,   89,   90,   91,    0,   92,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
   93,    0,    0,    0,    0,    0,   94,   95,   96,   97,
    0,    0,    0,   98,    0,   99,    0,    0,    0,    0,
  100,    0,  101,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,   68,   69,    0,   70,    0,    0,   71,   72,
    0,    0,  102,   73,  103,    0,  104,   74,  105,    0,
  106,    0,  107,   75,   38,    0,   76,    0,    0,    0,
    0,    0,    0,   77,    0,    0,    0,    0,   78,    0,
   79,   80,   81,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,   82,    0,   83,   84,    0,   85,
    0,    0,   86,    0,   87,    0,   88,   89,   90,   91,
    0,   92,    0,    0,    0,    0,    0,    0,    0,    0,
  414,  440,    0,    0,   93,    0,    0,    0,    0,    0,
   94,   95,   96,   97,    0,    0,    0,   98,    0,   99,
    0,    0,    0,    0,  100,    0,  101,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,   68,   69,    0,   70,
    0,    0,   71,   72,    0,    0,  102,   73,  103,    0,
  104,   74,  105,    0,  106,    0,  107,   75,   38,    0,
   76,    0,    0,    0,    0,    0,    0,   77,    0,    0,
    0,    0,   78,    0,   79,   80,   81,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,   82,    0,
   83,   84,    0,   85,    0,    0,   86,    0,   87,    0,
   88,   89,   90,   91,    0,   92,    0,    0,    0,    0,
    0,    0,    0,    0,  414,  552,    0,    0,   93,    0,
    0,    0,    0,    0,   94,   95,   96,   97,    0,    0,
    0,   98,    0,   99,    0,    0,    0,    0,  100,    0,
  101,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
   99,   99,    0,   99,    0,    0,   99,   99,    0,    0,
  102,   99,  103,    0,  104,   99,  105,    0,  106,    0,
  107,   99,   38,    0,   99,    0,    0,    0,    0,    0,
    0,   99,    0,    0,    0,    0,   99,    0,   99,   99,
   99,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,   99,    0,   99,   99,    0,   99,    0,    0,
   99,    0,   99,    0,   99,   99,   99,   99,    0,   99,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,   99,    0,    0,   99,    0,   99,   99,   99,
   99,   99,    0,    0,    0,   99,    0,   99,    0,    0,
    0,    0,   99,    0,   99,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,   68,   69,    0,   70,    0,    0,
   71,   72,    0,    0,   99,   73,   99,    0,   99,   74,
   99,    0,   99,    0,   99,   75,   99,    0,   76,    0,
    0,    0,    0,    0,    0,   77,    0,    0,    0,    0,
   78,    0,   79,   80,   81,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,   82,    0,   83,   84,
    0,   85,    0,    0,   86,    0,   87,    0,   88,   89,
   90,   91,    0,   92,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,   93,    0,    0,  298,
    0,    0,   94,   95,   96,   97,    0,    0,    0,   98,
    0,   99,    0,    0,    0,    0,  100,    0,  101,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,   68,   69,
    0,   70,    0,    0,   71,   72,    0,    0,  102,   73,
  103,    0,  104,   74,  105,    0,  106,    0,  107,   75,
   38,    0,   76,    0,    0,    0,    0,    0,    0,   77,
    0,    0,    0,    0,   78,    0,   79,   80,   81,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
   82,    0,   83,   84,    0,   85,    0,    0,   86,    0,
   87,    0,   88,   89,   90,   91,    0,   92,    0,    0,
    0,    0,    0,    0,    0,    0,  414,    0,    0,    0,
   93,    0,    0,    0,    0,    0,   94,   95,   96,   97,
    0,    0,    0,   98,    0,   99,    0,    0,    0,    0,
  100,    0,  101,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,   68,   69,    0,   70,    0,    0,   71,   72,
    0,    0,  102,   73,  103,    0,  104,   74,  105,    0,
  106,    0,  107,   75,   38,    0,   76,    0,    0,    0,
    0,    0,    0,   77,    0,    0,    0,    0,   78,    0,
   79,   80,   81,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,   82,    0,   83,   84,    0,   85,
    0,    0,   86,    0,   87,    0,   88,   89,   90,   91,
    0,   92,    0,    0,  501,    0,    0,    0,    0,    0,
    0,    0,    0,    0,   93,    0,    0,    0,    0,    0,
   94,   95,   96,   97,    0,    0,    0,   98,    0,   99,
    0,    0,    0,    0,  100,    0,  101,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  434,  434,    0,  434,
    0,    0,  434,  434,    0,    0,  102,  434,  103,    0,
  104,  434,  105,    0,  106,    0,  107,  434,   38,    0,
  434,    0,    0,    0,    0,    0,    0,  434,    0,    0,
    0,    0,  434,    0,  434,  434,  434,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  434,    0,
  434,  434,    0,  434,    0,    0,  434,    0,  434,    0,
  434,  434,  434,  434,    0,  434,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  434,    0,
    0,    0,    0,  434,  434,  434,  434,  434,    0,    0,
    0,  434,    0,  434,    0,    0,    0,    0,  434,    0,
  434,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
   68,   69,    0,   70,    0,    0,   71,   72,    0,    0,
  434,   73,  434,    0,  434,   74,  434,    0,  434,    0,
  434,   75,  434,    0,   76,    0,    0,    0,    0,    0,
    0,   77,    0,    0,    0,    0,   78,    0,   79,   80,
   81,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,   82,    0,   83,   84,    0,   85,    0,    0,
   86,    0,   87,    0,   88,   89,   90,   91,    0,   92,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,   93,    0,    0,    0,    0,    0,   94,   95,
   96,   97,    0,    0,    0,   98,    0,   99,    0,    0,
    0,    0,  100,    0,  101,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,   68,   69,    0,   70,    0,    0,
   71,   72,    0,    0,  102,   73,  103,    0,  104,   74,
  105,    0,  106,    0,  107,   75,   38,    0,   76,    0,
    0,    0,    0,    0,    0,   77,    0,    0,    0,    0,
   78,    0,   79,   80,   81,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,   82,    0,   83,   84,
    0,   85,    0,    0,   86,    0,   87,    0,   88,   89,
   90,   91,    0,   92,    0,  384,  501,  384,    0,    0,
  384,    0,  384,  384,    0,  384,  692,  384,    0,  384,
    0,  384,  384,  384,    0,    0,    0,    0,  384,    0,
    0,    0,    0,  384,    0,  384,  384,    0,    0,    0,
  384,    0,    0,    0,  384,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  384,    0,  384,    0,
    0,  384,  384,    0,    0,    0,    0,    0,    0,  384,
  384,    0,    0,  384,    0,    0,  384,    0,  102,    0,
  103,    0,  104,    0,  105,    0,  106,  383,  107,  383,
   38,    0,  383,    0,  383,  383,    0,  383,    0,  383,
    0,  383,    0,  383,  383,  383,    0,    0,    0,    0,
  383,   33,    0,   33,    0,  383,   33,  383,  383,    0,
    0,   33,  383,    0,    0,   33,  383,    0,   33,    0,
    0,    0,    0,    0,   33,    0,    0,    0,  383,    0,
  383,   33,    0,  383,  383,    0,   33,    0,   33,    0,
   33,  383,  383,    0,    0,  383,    0,    0,  383,    0,
  384,    0,   33,   33,   33,   33,    0,   33,   33,    0,
    0,    0,    0,   33,    0,   33,   33,   33,    0,   33,
   33,    0,   33,    0,    0,    0,   33,    0,    0,  144,
    0,    0,    0,   33,    0,    0,    0,    0,   33,    0,
   33,    0,   33,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,   33,    0,   33,    0,    0,   33,
    0,    0,    0,    0,    0,    0,    0,   33,   33,    0,
   33,   33,   33,    0,   33,   33,    0,    0,    0,    0,
   33,  145,  383,    0,   33,    0,    0,    0,    0,   33,
    0,   33,    0,   33,   33,    0,    0,    0,    0,   33,
   33,    0,    0,   33,    0,   33,   33,    0,    0,   33,
    0,   33,   33,   33,    0,    0,    0,    0,   33,   33,
    0,   33,    0,   33,   33,    0,   33,    0,   33,    0,
   33,    0,   33,    0,   33,   33,    0,   33,   33,    0,
   33,    0,   33,    0,    0,   33,   69,    0,   70,    0,
    0,   71,  107,   33,   33,    0,   73,   33,   33,    0,
   74,    0,    0,  461,    0,  228,    0,  228,    0,   76,
  228,    0,    0,    0,    0,  228,   77,    0,    0,  228,
    0,   78,    0,    0,    0,   81,    0,    0,  228,    0,
    0,    0,    0,    0,    0,  228,    0,   82,    0,   83,
  228,    0,   85,    0,  228,    0,    0,    0,    0,    0,
   89,   90,    0,    0,   92,    0,  228,  462,  228,    0,
    0,  228,  229,    0,  229,   33,    0,  229,    0,  228,
  228,    0,  229,  228,    0,    0,  229,    0,    0,    0,
    0,  152,  228,  152,   33,  229,  152,    0,    0,    0,
  228,  152,  229,    0,    0,  152,    0,  229,  152,    0,
    0,  229,    0,    0,  152,    0,    0,    0,    0,    0,
    0,  152,    0,  229,    0,  229,  152,    0,  229,    0,
  152,    0,    0,    0,    0,    0,  229,  229,    0,    0,
  229,    0,  152,    0,  152,    0,    0,  152,    0,  229,
    0,   38,    0,  249,    0,  152,  152,  229,   69,  152,
   70,    0,  152,   71,  115,    0,  115,    0,   73,  115,
  228,    0,   74,    0,  115,    0,    0,    0,  115,    0,
    0,   76,    0,    0,    0,    0,    0,  115,   77,  249,
    0,    0,    0,   78,  115,    0,    0,   81,    0,  115,
    0,    0,    0,  115,    0,    0,    0,    0,    0,   82,
    0,   83,    0,    0,   85,  115,    0,  115,    0,    0,
  115,    0,   89,   90,    0,    0,   92,  229,  115,  115,
    0,    0,  115,    0,    0,  249,    0,  249,    0,    0,
    0,    0,    0,    0,  249,  249,  152,  249,  249,  249,
  249,  249,  249,  249,  249,  249,  249,  249,    0,  249,
    0,  249,    0,  249,    0,  249,    0,  249,    0,  249,
    0,  249,    0,  249,    0,  249,    0,  249,    0,  249,
    0,  249,    0,  249,    0,  249,    0,  249,    0,  249,
    0,  249,    0,  249,    0,  249,    0,   21,    0,    0,
   21,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,   21,    0,   38,    0,    0,   21,    0,    0,  115,
   21,    0,    0,   21,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,   21,   21,    0,    0,    0,
   21,   21,    0,    0,    0,    0,   21,    0,   21,   21,
   21,   21,   12,    0,    0,   12,   21,    0,    0,   21,
    0,   21,    0,    0,    0,    0,   12,    0,    0,    0,
    0,   12,    0,    0,   21,   12,    0,    0,   12,    0,
    0,    0,   21,   21,    0,    0,    0,    0,    0,    0,
   12,   12,    0,    0,    0,   12,   12,    0,    0,    0,
    0,   12,    0,   12,   12,   12,   12,   20,    0,    0,
   20,   12,    0,    0,   12,    0,   12,    0,    0,    0,
    0,   20,    0,    0,    0,    0,   20,    0,    0,   12,
   20,    0,    0,   20,    0,    0,    0,   12,   12,    0,
    0,    0,    0,    0,    0,   20,   20,    0,    0,    0,
   20,   20,    0,    0,   20,    0,   20,    0,   20,   20,
   20,   20,    0,    0,    0,   20,   20,    0,    0,   20,
   20,   20,    0,    0,   20,    0,    0,   20,    0,    0,
    0,    0,    0,    0,   20,    0,    0,    0,    0,   20,
   20,    0,    0,   20,   20,   20,    0,    0,   33,    0,
   20,    0,   20,   20,   20,   20,    0,    0,    0,   33,
   20,    0,    0,   20,   33,   20,    0,    0,   33,    0,
    0,   33,    0,    0,    0,    0,    0,    0,   20,    0,
    0,    0,    0,   33,   33,    0,   20,   20,    0,   33,
    0,    0,    0,    0,   33,    0,   33,   33,   33,   33,
   33,    0,    0,   33,   33,    0,    0,   33,    0,   33,
    0,    0,    0,    0,   33,    0,    0,    0,    0,   33,
    0,    0,   33,   33,    0,    0,   33,    0,    0,    0,
   22,    0,    0,    0,    0,    0,    0,    0,   33,   33,
    0,    0,   22,   33,   33,   33,    0,    0,    0,   33,
    0,   33,   33,   33,   33,    0,   33,    0,    0,   33,
    0,   33,   33,    0,   33,   33,    0,    0,   33,    0,
    0,    0,    0,    0,    0,    0,    0,   33,    0,    0,
   33,   33,    0,    0,    0,  252,   33,  252,  437,  252,
  437,   33,  437,   33,   33,   33,   33,  252,    0,    0,
    0,   33,    0,    0,   33,    0,   33,  252,  256,  252,
  256,  440,  256,  440,    0,  440,    0,    0,    0,   33,
  256,    0,    0,    0,    0,    0,    0,  252,    0,  252,
  256,  252,  256,  252,    0,  252,    0,  252,    0,  252,
    0,  252,    0,  252,    0,  252,    0,    0,    0,    0,
  256,    0,  256,    0,  256,    0,  256,    0,  256,    0,
  256,  252,  256,    0,  256,    0,  256,  257,  256,  257,
  441,  257,  441,    0,  441,    0,    0,    0,  296,  257,
  296,  444,  296,  444,  256,  444,    0,    0,    0,  257,
  296,  257,    0,    0,    0,    0,    0,    0,    0,    0,
  296,    0,  296,    0,    0,    0,    0,    0,    0,  257,
    0,  257,    0,  257,    0,  257,    0,  257,    0,  257,
  296,  257,  296,  257,  296,  257,  296,  257,  296,   16,
  296,   16,  296,   16,  296,    0,  296,    0,  296,    0,
    0,   16,    0,  257,    0,    0,    0,    0,    0,    0,
    0,   16,    0,   16,  296,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,   16,    0,   16,    0,   16,    0,   16,    0,   16,
    0,   16,    0,   16,    0,   16,    0,   16,    0,   16,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,   16,
  };
  protected static  short [] yyCheck = {             3,
  233,  517,  320,   22,  465,  605,  489,  235,    1,   13,
   14,  281,  324,  323,  342,   41,  641,  323,  343,  345,
  343,  343,  304,   16,  346,   29,  267,  457,  263,   22,
  343,  262,   25,  304,  275,  623,  343,  343,  457,  343,
  343,  323,  343,  313,    1,  333,  264,  347,  266,  300,
  452,  269,  323,  271,  272,  262,  274,  159,  276,   16,
  278,  343,  280,  281,  282,  347,  468,   93,  343,  287,
  489,  419,  343,  562,  292,  343,  294,  295,  343,  391,
  468,  299,  267,  314,  343,  303,   79,  517,  323,  343,
  275,  419,  343,  419,  419,  261,  419,  315,  517,  317,
   93,  343,  320,  321,  332,  594,  419,  314,  624,  419,
  328,  329,  419,  419,  332,  419,  419,  335,  419,  419,
   94,   95,   96,   97,   98,   99,  100,  101,  728,  590,
  232,  297,  151,  159,  305,  161,  307,  419,  726,  727,
  452,  312,  168,  359,  419,   79,  534,  636,  419,  419,
  639,  419,  355,  602,  419,  181,  182,  342,  151,   93,
  419,  154,  188,  189,  190,  191,  192,  193,  194,  195,
  196,  197,  198,  166,  167,  605,  801,  419,  419,  341,
  268,  341,  619,  379,  342,  343,  605,  349,  618,  626,
  356,  357,  218,  642,  624,  411,  364,  285,  798,  359,
  359,  419,  359,  359,  359,  624,  232,  373,  368,  375,
  359,  605,  359,  206,  207,  383,  359,  342,  244,  245,
  154,  352,  353,  348,  618,  199,  200,  201,  202,  203,
  204,  205,  166,  167,  208,  209,  210,  211,  212,  213,
  214,  215,  216,  217,  341,  219,  344,  323,  731,  732,
  348,  411,  411,  350,  411,  411,  411,  453,  350,  455,
  341,  744,  411,  746,  411,  239,  342,  241,  411,  350,
  296,  589,  206,  207,  343,  501,  345,  347,  347,  341,
  506,  285,  271,  345,  360,  347,  348,  276,  350,  315,
  486,  280,  271,  342,  349,  337,  365,  276,  367,  348,
  342,  280,  498,  499,  419,  324,  295,   79,  266,  728,
  337,  269,  731,  732,  267,  342,  295,  800,  322,  637,
  202,  203,  275,  839,  317,  744,  343,  746,  345,  343,
  347,  324,  321,  347,  327,  419,  294,  341,  821,  344,
  345,  299,  321,  348,  350,  344,  339,  344,  365,  348,
  367,  348,  356,  836,  257,  359,  344,  315,  554,  317,
  348,  345,  344,  341,  368,  358,  348,  345,  345,  341,
  328,  329,  391,  345,  332,  343,  341,  347,  346,  798,
  345,  800,  154,  317,  347,  345,  390,  350,  414,  382,
  394,  384,  341,  327,  166,  167,  345,  827,  391,  360,
  361,  362,  821,  721,  430,  339,  341,  411,  827,  839,
  345,  208,  209,  210,  211,  339,  339,  836,  342,  342,
  839,  346,  358,  348,  358,  418,  369,  377,  371,  379,
  343,  450,  345,  452,  206,  207,  348,  345,  350,  465,
  359,  348,  435,  350,  348,  343,  350,  345,  382,  468,
  384,  346,  347,  446,  447,  381,  343,  450,  345,  452,
  341,  457,  658,  337,  337,  339,  339,  363,  461,  495,
  346,  497,  457,  204,  205,  468,  469,  470,  346,  346,
  484,  348,  419,  457,  418,  344,  337,  346,  339,  345,
  686,  347,  419,  489,  487,  212,  213,  419,  419,  419,
  350,  435,  419,  419,  489,  349,  343,  346,  346,  344,
  346,  301,  446,  447,  348,  489,  457,  341,  349,  349,
  348,  517,  345,  457,  349,  342,  348,  461,  349,  725,
  346,  346,  517,  341,  341,  469,  470,  341,  341,  348,
  419,  737,  342,  517,  342,  741,  348,  348,  489,  355,
  342,  350,  257,  487,  342,  260,  752,  753,  791,  792,
  345,  419,  304,  304,  590,  419,  271,  339,  345,  345,
  341,  276,  341,  419,  347,  280,  517,  343,  283,  346,
  355,  607,  347,  517,  610,  611,  358,  350,  614,  350,
  295,  296,  345,  345,  598,  599,  301,  623,  345,  345,
  626,  306,  345,  308,  309,  310,  311,  345,  343,  605,
  382,  316,  384,  606,  319,  641,  321,  457,  345,  349,
  605,  342,  350,  419,  350,  457,  350,  345,  624,  334,
  343,  605,  355,  829,  341,  419,  346,  342,  342,  624,
  336,  645,  345,  344,  350,  419,  418,  457,  350,  489,
  624,  655,  350,  646,  647,  350,  348,  489,  651,  350,
  346,  349,  346,  435,  605,  343,  692,  339,  694,  337,
  345,  605,  606,  350,  446,  447,  350,  517,  348,  489,
  346,  346,  346,  624,  618,  517,  345,  285,  268,  461,
  624,  268,  346,  344,  346,  344,  346,  469,  470,  703,
  726,  727,  346,  342,  293,  709,  710,  517,  341,  350,
  341,  314,  646,  647,  350,  487,  350,  651,  262,  346,
  346,  345,  345,  342,  344,  346,  279,  342,  350,  339,
  342,  349,  728,  350,  350,  731,  732,  350,  346,  346,
  346,  734,  346,  728,  346,  771,  731,  732,  744,  349,
  746,  337,  346,  350,  728,  342,  341,  731,  732,  744,
  350,  746,  341,  347,  346,  605,  344,  341,  341,  341,
  744,  346,  746,  605,  344,  801,  341,  341,  314,  342,
  341,  419,  342,  350,  624,  342,  341,  728,  350,  419,
  731,  732,  624,  262,  419,  605,  350,  342,  350,  346,
  734,  794,  798,  744,  800,  746,  810,  811,  812,  346,
  342,  346,  221,  798,  624,  800,    4,  285,   29,  151,
  368,   26,  329,  327,  798,  821,  800,  591,  411,  363,
  598,  827,  725,  710,  606,  655,  821,  387,  387,  599,
  836,  810,  827,  839,  709,  291,  245,  821,  394,  390,
  314,  836,  214,  827,  839,  215,  217,  798,  216,  800,
  794,   63,  836,  219,  621,  839,  804,  806,  827,  798,
  284,  690,   -1,  687,  646,  647,  689,   -1,   -1,  651,
  821,   -1,   -1,  497,   -1,   -1,  827,   -1,  728,   -1,
   -1,  731,  732,  827,   -1,  836,  728,   -1,  839,  731,
  732,   -1,   -1,   -1,  744,  839,  746,   -1,   -1,   -1,
   -1,  325,  744,   -1,  746,   -1,   -1,   -1,  728,   -1,
   -1,  731,  732,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  744,   -1,  746,  351,  352,  353,
  354,   -1,  356,  357,  358,  359,  360,  361,  362,  363,
   -1,  365,   -1,  367,   -1,  369,   -1,  371,  798,  373,
  800,  375,  734,  377,   -1,  379,  798,   -1,  800,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  821,   -1,   -1,   -1,   -1,   -1,  827,  798,  821,
  800,   -1,   -1,   -1,   -1,  827,  836,   -1,   -1,  839,
   -1,   -1,   -1,   -1,  836,   -1,   -1,  839,   -1,   -1,
   -1,  821,   -1,   -1,   -1,   -1,  257,  827,   -1,  260,
   -1,  262,  794,  264,   -1,  266,  836,   -1,  269,  839,
  271,  272,   -1,  274,   -1,  276,   -1,  278,   -1,  280,
  281,  282,  283,   -1,   -1,   -1,  287,   -1,   -1,   -1,
   -1,  292,   -1,  294,  295,  296,   -1,   -1,  299,  300,
  301,   -1,  303,   -1,  305,  306,  307,  308,  309,  310,
  311,  312,   -1,  314,  315,  316,  317,   -1,  319,  320,
  321,   -1,   -1,   -1,   -1,   -1,   -1,  328,  329,   -1,
   -1,  332,   -1,  334,  335,   -1,  337,  257,  339,   -1,
  260,   -1,   -1,   -1,  264,   -1,  266,   -1,   -1,  269,
  351,  271,  272,   -1,  274,   -1,  276,   -1,  278,   -1,
  280,  281,  282,  283,   -1,   -1,   -1,  287,   -1,   -1,
   -1,   -1,  292,   -1,  294,  295,  296,   -1,   -1,  299,
  300,  301,   -1,  303,   -1,   -1,  306,   -1,  308,  309,
  310,  311,   -1,   -1,   -1,  315,  316,  317,   -1,  319,
  320,  321,   -1,   -1,   -1,   -1,   -1,   -1,  328,  329,
   -1,   -1,  332,   -1,  334,  335,   -1,   -1,  419,   -1,
   -1,  260,  342,  343,   -1,  264,   -1,  266,   -1,   -1,
  269,  351,  271,  272,   -1,  274,   -1,  276,   -1,  278,
   -1,  280,  281,  282,  283,   -1,   -1,   -1,  287,   -1,
   -1,   -1,   -1,  292,   -1,  294,  295,  296,   -1,   -1,
  299,   -1,  301,   -1,  303,   -1,   -1,  306,   -1,  308,
  309,  310,  311,   -1,   -1,   -1,  315,  316,  317,   -1,
  319,  320,  321,   -1,   -1,   -1,   -1,   -1,   -1,  328,
  329,   -1,   -1,  332,   -1,  334,  335,   -1,   -1,  419,
   -1,   -1,  260,  342,   -1,   -1,  264,   -1,  266,   -1,
   -1,  269,  351,  271,  272,   -1,  274,   -1,  276,   -1,
  278,   -1,  280,  281,  282,  283,   -1,   -1,   -1,  287,
   -1,   -1,   -1,   -1,  292,   -1,  294,  295,  296,   -1,
   -1,  299,   -1,  301,   -1,  303,   -1,   -1,  306,   -1,
  308,  309,  310,  311,   -1,   -1,   -1,  315,  316,  317,
   -1,  319,  320,  321,   -1,   -1,   -1,   -1,   -1,   -1,
  328,  329,   -1,   -1,  332,   -1,  334,  335,   -1,   -1,
  419,   -1,   -1,  260,  342,   -1,   -1,  264,   -1,  266,
   -1,   -1,  269,  351,  271,  272,   -1,  274,   -1,  276,
   -1,  278,   -1,  280,  281,  282,  283,   -1,   -1,   -1,
  287,   -1,   -1,   -1,   -1,  292,   -1,  294,  295,  296,
   -1,   -1,  299,   -1,  301,   -1,  303,   -1,   -1,  306,
   -1,  308,  309,  310,  311,   -1,   -1,   -1,  315,  316,
  317,   -1,  319,  320,  321,   -1,   -1,   -1,   -1,   -1,
   -1,  328,  329,   -1,   -1,  332,   -1,  334,  335,   -1,
   -1,  419,  260,   -1,   -1,  342,  264,   -1,  266,   -1,
   -1,  269,   -1,  271,  272,   -1,  274,   -1,  276,   -1,
  278,   -1,  280,  281,  282,  283,   -1,   -1,   -1,  287,
   -1,   -1,   -1,   -1,  292,   -1,  294,  295,  296,   -1,
   -1,  299,   -1,  301,   -1,  303,   -1,   -1,  306,   -1,
  308,  309,  310,  311,   -1,   -1,   -1,  315,  316,  317,
   -1,  319,  320,  321,   -1,   -1,   -1,   -1,   -1,   -1,
  328,  329,   -1,   -1,  332,   -1,  334,  335,   -1,   -1,
   -1,   -1,  419,  261,  342,  263,  264,   -1,  266,   -1,
   -1,  269,  270,   -1,   -1,   -1,  274,   -1,   -1,   -1,
  278,   -1,   -1,   -1,   -1,   -1,  284,   -1,   -1,  287,
   -1,   -1,   -1,   -1,   -1,   -1,  294,   -1,   -1,  297,
   -1,  299,   -1,  301,  302,  303,  304,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  315,   -1,  317,
  318,   -1,  320,   -1,   -1,  323,   -1,  325,   -1,  327,
  328,  329,  330,   -1,  332,   -1,   -1,   -1,  355,   -1,
   -1,  419,   -1,  341,  342,   -1,  344,  345,  346,  347,
  348,  349,  350,  351,  352,  353,  354,  355,  356,  357,
  358,  359,  360,  361,  362,  363,  364,  365,  385,  367,
  387,  369,  389,  371,  391,  373,  393,  375,  395,  377,
  397,  379,  399,  381,  401,  383,  403,  385,   -1,  387,
   -1,  389,   -1,  391,   -1,  393,   -1,  395,   -1,  397,
   -1,  399,  419,  401,   -1,  403,   -1,   -1,   -1,  407,
   -1,  409,   -1,  411,   -1,  413,   -1,  415,   -1,  417,
  261,  419,  263,  264,   -1,  266,   -1,   -1,  269,  270,
   -1,   -1,   -1,  274,   -1,   -1,   -1,  278,   -1,   -1,
   -1,   -1,   -1,  284,   -1,   -1,  287,   -1,   -1,   -1,
   -1,   -1,   -1,  294,   -1,   -1,  297,   -1,  299,   -1,
  301,  302,  303,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  315,   -1,  317,  318,   -1,  320,
   -1,   -1,  323,   -1,  325,   -1,  327,  328,  329,  330,
  346,  332,  348,   -1,  350,   -1,   -1,   -1,   -1,  355,
  341,  342,   -1,  344,  345,  346,  347,  348,  349,  350,
  351,  352,  353,  354,  355,  356,  357,  358,  359,  360,
  361,  362,  363,  364,  365,  355,  367,   -1,  369,  385,
  371,  387,  373,  389,  375,  391,  377,  393,  379,  395,
  381,  397,  383,  399,  385,  401,  387,  403,  389,   -1,
  391,   -1,  393,   -1,  395,  385,  397,  387,  399,  389,
  401,  391,  403,  393,   -1,  395,  407,  397,  409,  399,
  411,  401,  413,  403,  415,   -1,  417,  261,  419,  263,
  264,   -1,  266,   -1,   -1,  269,  270,   -1,   -1,   -1,
  274,   -1,   -1,   -1,  278,   -1,   -1,   -1,   -1,   -1,
  284,   -1,   -1,  287,   -1,   -1,   -1,   -1,   -1,   -1,
  294,   -1,   -1,  297,   -1,  299,   -1,  301,  302,  303,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  315,   -1,  317,  318,   -1,  320,   -1,   -1,  323,
   -1,  325,   -1,  327,  328,  329,  330,  346,  332,  348,
   -1,  350,   -1,   -1,   -1,   -1,  355,   -1,  342,  343,
  344,  345,  346,  347,  348,  349,  350,  351,  352,  353,
  354,  355,  356,  357,  358,  359,  360,  361,  362,  363,
  364,  365,   -1,  367,   -1,  369,  385,  371,  387,  373,
  389,  375,  391,  377,  393,  379,  395,  381,  397,  383,
  399,  385,  401,  387,  403,  389,   -1,  391,   -1,  393,
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
   -1,  348,  349,  350,  351,  352,  353,  354,  355,  356,
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
   -1,   -1,  342,   -1,  344,   -1,  346,   -1,  348,  349,
  350,  351,  352,  353,  354,  355,  356,  357,  358,  359,
  360,  361,  362,  363,  364,   -1,   -1,   -1,   -1,  369,
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
   -1,  344,  345,  346,   -1,  348,  349,  350,  351,  352,
  353,  354,   -1,  356,  357,  358,  359,  360,  361,  362,
  363,  364,  365,   -1,  367,   -1,  369,   -1,  371,   -1,
  373,   -1,  375,   -1,  377,   -1,  379,   -1,  381,   -1,
  383,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  261,   -1,  263,  264,   -1,  266,   -1,   -1,
  269,  270,   -1,   -1,  407,  274,  409,   -1,  411,  278,
  413,   -1,  415,   -1,  417,  284,  419,   -1,  287,   -1,
   -1,   -1,   -1,   -1,   -1,  294,   -1,   -1,  297,   -1,
  299,   -1,  301,  302,  303,  304,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  315,   -1,  317,  318,
   -1,  320,   -1,   -1,  323,   -1,  325,   -1,  327,  328,
  329,  330,   -1,  332,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  341,  342,  343,  344,  345,  346,   -1,  348,
  349,  350,  351,  352,  353,  354,   -1,  356,  357,  358,
  359,  360,   -1,   -1,  363,  364,  365,   -1,  367,   -1,
   -1,   -1,   -1,   -1,  373,   -1,  375,   -1,  377,   -1,
  379,   -1,  381,   -1,  383,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  261,   -1,  263,  264,
   -1,  266,   -1,   -1,  269,  270,   -1,   -1,  407,  274,
  409,   -1,  411,  278,  413,   -1,  415,   -1,  417,  284,
  419,   -1,  287,   -1,   -1,   -1,   -1,   -1,   -1,  294,
   -1,   -1,  297,   -1,  299,   -1,  301,  302,  303,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  315,   -1,  317,  318,   -1,  320,   -1,   -1,  323,   -1,
  325,   -1,  327,  328,  329,  330,   -1,  332,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  342,   -1,  344,
  345,  346,   -1,  348,  349,  350,  351,  352,  353,  354,
   -1,  356,  357,  358,  359,   -1,   -1,   -1,  363,  364,
  365,   -1,  367,   -1,  369,   -1,  371,   -1,  373,   -1,
  375,   -1,  377,   -1,  379,   -1,  381,   -1,  383,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  261,   -1,  263,  264,   -1,  266,   -1,   -1,  269,  270,
   -1,   -1,  407,  274,  409,   -1,  411,  278,  413,   -1,
  415,   -1,  417,  284,  419,   -1,  287,   -1,   -1,   -1,
   -1,   -1,   -1,  294,   -1,   -1,  297,   -1,  299,   -1,
  301,  302,  303,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  315,   -1,  317,  318,   -1,  320,
   -1,   -1,  323,   -1,  325,   -1,  327,  328,  329,  330,
   -1,  332,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  342,   -1,  344,  345,  346,   -1,  348,  349,  350,
  351,  352,  353,  354,   -1,  356,  357,  358,  359,   -1,
   -1,   -1,  363,  364,  365,   -1,  367,   -1,  369,   -1,
  371,   -1,  373,   -1,  375,   -1,  377,   -1,  379,   -1,
  381,   -1,  383,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  261,   -1,  263,  264,   -1,  266,
   -1,   -1,  269,  270,   -1,   -1,  407,  274,  409,   -1,
  411,  278,  413,   -1,  415,   -1,  417,  284,  419,   -1,
  287,   -1,   -1,   -1,   -1,   -1,   -1,  294,   -1,   -1,
  297,   -1,  299,   -1,  301,  302,  303,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  315,   -1,
  317,  318,   -1,  320,   -1,   -1,  323,   -1,  325,   -1,
  327,  328,  329,  330,   -1,  332,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  342,   -1,  344,  345,  346,
   -1,  348,  349,  350,  351,  352,  353,  354,   -1,  356,
  357,  358,  359,   -1,   -1,   -1,  363,  364,  365,   -1,
  367,   -1,  369,   -1,  371,   -1,  373,   -1,  375,   -1,
  377,   -1,  379,   -1,  381,   -1,  383,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  261,   -1,
  263,  264,   -1,  266,   -1,   -1,  269,  270,   -1,   -1,
  407,  274,  409,   -1,  411,  278,  413,   -1,  415,   -1,
  417,  284,  419,   -1,  287,   -1,   -1,   -1,   -1,   -1,
   -1,  294,   -1,   -1,  297,   -1,  299,   -1,  301,  302,
  303,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  315,   -1,  317,  318,   -1,  320,   -1,   -1,
  323,   -1,  325,   -1,  327,  328,  329,  330,   -1,  332,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  342,
   -1,  344,  345,  346,   -1,  348,  349,  350,  351,   -1,
   -1,  354,   -1,  356,  357,  358,  359,  360,   -1,   -1,
  363,  364,  365,   -1,  367,   -1,  369,   -1,  371,   -1,
  373,   -1,  375,   -1,  377,   -1,  379,   -1,  381,   -1,
  383,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  261,   -1,  263,  264,   -1,  266,   -1,   -1,
  269,  270,   -1,   -1,  407,  274,  409,   -1,  411,  278,
  413,   -1,  415,   -1,  417,  284,  419,   -1,  287,   -1,
   -1,   -1,   -1,   -1,   -1,  294,   -1,   -1,  297,   -1,
  299,   -1,  301,  302,  303,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  315,   -1,  317,  318,
   -1,  320,   -1,   -1,  323,   -1,  325,   -1,  327,  328,
  329,  330,   -1,  332,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  342,   -1,  344,  345,  346,   -1,  348,
  349,  350,  351,  352,  353,  354,   -1,  356,  357,  358,
  359,  360,   -1,   -1,  363,  364,  365,   -1,  367,   -1,
   -1,   -1,   -1,   -1,  373,   -1,  375,   -1,  377,   -1,
  379,   -1,  381,   -1,  383,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  261,   -1,  263,  264,
   -1,  266,   -1,   -1,  269,  270,   -1,   -1,  407,  274,
  409,   -1,  411,  278,  413,   -1,  415,   -1,  417,  284,
  419,   -1,  287,   -1,   -1,   -1,   -1,   -1,   -1,  294,
   -1,   -1,  297,   -1,  299,   -1,  301,  302,  303,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  315,   -1,  317,  318,   -1,  320,   -1,   -1,  323,   -1,
  325,   -1,  327,  328,  329,  330,   -1,  332,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  342,   -1,  344,
  345,  346,   -1,  348,  349,  350,  351,   -1,   -1,  354,
   -1,  356,  357,  358,  359,  360,   -1,   -1,  363,  364,
  365,   -1,  367,   -1,  369,   -1,  371,   -1,  373,   -1,
  375,   -1,  377,   -1,  379,   -1,  381,   -1,  383,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  261,   -1,  263,  264,   -1,  266,   -1,   -1,  269,  270,
   -1,   -1,  407,  274,  409,   -1,  411,  278,  413,   -1,
  415,   -1,  417,  284,  419,   -1,  287,   -1,   -1,   -1,
   -1,   -1,   -1,  294,   -1,   -1,  297,   -1,  299,   -1,
  301,  302,  303,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  315,   -1,  317,  318,   -1,  320,
   -1,   -1,  323,   -1,  325,   -1,  327,  328,  329,  330,
   -1,  332,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  342,   -1,  344,  345,  346,   -1,  348,  349,  350,
  351,   -1,   -1,  354,   -1,  356,  357,  358,  359,  360,
   -1,   -1,  363,  364,  365,   -1,  367,   -1,  369,   -1,
  371,   -1,  373,   -1,  375,   -1,  377,   -1,  379,   -1,
  381,   -1,  383,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  261,   -1,  263,  264,   -1,  266,
   -1,   -1,  269,  270,   -1,   -1,  407,  274,  409,   -1,
  411,  278,  413,   -1,  415,   -1,  417,  284,  419,   -1,
  287,   -1,   -1,   -1,   -1,   -1,   -1,  294,   -1,   -1,
  297,   -1,  299,   -1,  301,  302,  303,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  315,   -1,
  317,  318,   -1,  320,   -1,   -1,  323,   -1,  325,   -1,
  327,  328,  329,  330,   -1,  332,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  342,   -1,  344,  345,  346,
   -1,  348,  349,  350,  351,  352,  353,  354,   -1,  356,
  357,  358,  359,  360,   -1,   -1,  363,  364,  365,   -1,
  367,   -1,   -1,   -1,   -1,   -1,  373,   -1,  375,   -1,
  377,   -1,  379,   -1,  381,   -1,  383,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  261,   -1,
  263,  264,   -1,  266,   -1,   -1,  269,  270,   -1,   -1,
  407,  274,  409,   -1,  411,  278,  413,   -1,  415,   -1,
  417,  284,  419,   -1,  287,   -1,   -1,   -1,   -1,   -1,
   -1,  294,   -1,   -1,  297,   -1,  299,   -1,  301,  302,
  303,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  315,   -1,  317,  318,   -1,  320,   -1,   -1,
  323,   -1,  325,   -1,  327,  328,  329,  330,   -1,  332,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  342,
   -1,  344,  345,  346,   -1,  348,  349,  350,  351,  352,
  353,  354,   -1,  356,  357,  358,  359,  360,   -1,   -1,
  363,  364,  365,   -1,  367,   -1,   -1,   -1,   -1,   -1,
  373,   -1,  375,   -1,  377,   -1,  379,   -1,  381,   -1,
  383,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  261,   -1,  263,  264,   -1,  266,   -1,   -1,
  269,  270,   -1,   -1,  407,  274,  409,   -1,  411,  278,
  413,   -1,  415,   -1,  417,  284,  419,   -1,  287,   -1,
   -1,   -1,   -1,   -1,   -1,  294,   -1,   -1,  297,   -1,
  299,   -1,  301,  302,  303,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  315,   -1,  317,  318,
   -1,  320,   -1,   -1,  323,   -1,  325,   -1,  327,  328,
  329,  330,   -1,  332,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  342,   -1,  344,  345,  346,   -1,  348,
  349,  350,  351,  352,  353,  354,   -1,  356,  357,  358,
  359,  360,   -1,   -1,  363,  364,  365,   -1,  367,   -1,
   -1,   -1,   -1,   -1,  373,   -1,  375,   -1,  377,   -1,
  379,   -1,  381,   -1,  383,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  261,   -1,  263,  264,
   -1,  266,   -1,   -1,  269,  270,   -1,   -1,  407,  274,
  409,   -1,  411,  278,  413,   -1,  415,   -1,  417,  284,
  419,   -1,  287,   -1,   -1,   -1,   -1,   -1,   -1,  294,
   -1,   -1,  297,   -1,  299,   -1,  301,  302,  303,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  315,   -1,  317,  318,   -1,  320,   -1,   -1,  323,   -1,
  325,   -1,  327,  328,  329,  330,   -1,  332,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  342,   -1,  344,
  345,  346,   -1,  348,  349,  350,  351,  352,  353,  354,
   -1,  356,  357,  358,  359,  360,   -1,   -1,  363,  364,
  365,   -1,  367,   -1,   -1,   -1,   -1,   -1,  373,   -1,
  375,   -1,  377,   -1,  379,   -1,  381,   -1,  383,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  261,   -1,  263,  264,   -1,  266,   -1,   -1,  269,  270,
   -1,   -1,  407,  274,  409,   -1,  411,  278,  413,   -1,
  415,   -1,  417,  284,  419,   -1,  287,   -1,   -1,   -1,
   -1,   -1,   -1,  294,   -1,   -1,  297,   -1,  299,   -1,
  301,  302,  303,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  315,   -1,  317,  318,   -1,  320,
   -1,   -1,  323,   -1,  325,   -1,  327,  328,  329,  330,
   -1,  332,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  342,   -1,  344,  345,  346,   -1,  348,  349,  350,
  351,  352,  353,  354,   -1,  356,  357,  358,  359,  360,
   -1,   -1,  363,  364,  365,   -1,  367,   -1,   -1,   -1,
   -1,   -1,  373,   -1,  375,   -1,  377,   -1,  379,   -1,
  381,   -1,  383,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  261,   -1,  263,  264,   -1,  266,
   -1,   -1,  269,  270,   -1,   -1,  407,  274,  409,   -1,
  411,  278,  413,   -1,  415,   -1,  417,  284,  419,   -1,
  287,   -1,   -1,   -1,   -1,   -1,   -1,  294,   -1,   -1,
  297,   -1,  299,   -1,  301,  302,  303,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  315,   -1,
  317,  318,   -1,  320,   -1,   -1,  323,   -1,  325,   -1,
  327,  328,  329,  330,   -1,  332,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  342,   -1,  344,  345,  346,
   -1,  348,  349,  350,  351,  352,  353,  354,   -1,  356,
  357,  358,  359,  360,   -1,   -1,  363,  364,  365,   -1,
  367,   -1,   -1,   -1,   -1,   -1,  373,   -1,  375,   -1,
  377,   -1,  379,   -1,  381,   -1,  383,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  407,   -1,  409,   -1,  411,   -1,  413,   -1,  415,   -1,
  417,   -1,  419,  263,  264,  265,  266,  267,   -1,  269,
  270,   -1,  272,  273,  274,  275,   -1,  277,  278,  279,
   -1,   -1,   -1,   -1,  284,  285,   -1,  287,  288,  289,
  290,  291,   -1,   -1,  294,   -1,   -1,   -1,  298,  299,
   -1,  301,  302,  303,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  313,   -1,  315,   -1,  317,  318,   -1,
  320,   -1,  322,  323,  324,  325,  326,  327,  328,  329,
  330,   -1,  332,  333,  260,  335,  336,   -1,   -1,   -1,
   -1,  341,  342,   -1,   -1,  345,   -1,   -1,   -1,   -1,
  350,  351,  352,  353,  354,   -1,   -1,  283,  358,   -1,
  360,   -1,   -1,   -1,   -1,  365,   -1,  367,   -1,   -1,
  296,   -1,   -1,   -1,   -1,  301,   -1,   -1,   -1,   -1,
  306,   -1,  308,  309,  310,  311,   -1,   -1,   -1,   -1,
  316,   -1,   -1,  319,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  407,  334,  409,
   -1,  411,   -1,  413,   -1,  415,   -1,  417,   -1,  419,
  263,  264,  265,  266,  267,  351,  269,  270,   -1,  272,
  273,  274,  275,   -1,  277,  278,  279,   -1,   -1,   -1,
   -1,  284,   -1,   -1,  287,  288,  289,  290,  291,   -1,
   -1,  294,   -1,   -1,   -1,  298,  299,   -1,  301,  302,
  303,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  313,   -1,  315,   -1,  317,  318,   -1,  320,   -1,  322,
  323,  324,  325,  326,  327,  328,  329,  330,   -1,  332,
  333,  260,  335,  336,   -1,   -1,   -1,   -1,  341,  342,
   -1,   -1,  345,   -1,   -1,   -1,   -1,  350,  351,  352,
  353,  354,   -1,   -1,  283,  358,   -1,  360,   -1,   -1,
   -1,   -1,  365,   -1,  367,   -1,   -1,  296,   -1,   -1,
   -1,   -1,  301,   -1,   -1,   -1,   -1,  306,   -1,  308,
  309,  310,  311,   -1,   -1,   -1,   -1,  316,   -1,   -1,
  319,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  407,  334,  409,   -1,  411,   -1,
  413,   -1,  415,   -1,  417,   -1,  419,  263,  264,  265,
  266,  267,   -1,  269,  270,   -1,  272,  273,  274,  275,
   -1,  277,  278,   -1,   -1,   -1,   -1,   -1,  284,   -1,
   -1,  287,  288,  289,  290,  291,   -1,   -1,  294,   -1,
   -1,   -1,  298,  299,   -1,  301,  302,  303,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  313,   -1,  315,
   -1,  317,  318,   -1,  320,   -1,  322,  323,  324,  325,
  326,  327,  328,  329,  330,   -1,  332,  333,   -1,  335,
  336,   -1,   -1,   -1,   -1,  341,  342,   -1,   -1,  345,
   -1,   -1,   -1,   -1,  350,  351,  352,  353,  354,   -1,
   -1,   -1,  358,   -1,  360,   -1,   -1,   -1,   -1,  365,
   -1,  367,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
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
  419,  263,  264,  265,  266,   -1,   -1,  269,  270,   -1,
  272,  273,  274,   -1,   -1,  277,  278,   -1,   -1,   -1,
   -1,   -1,  284,   -1,   -1,  287,  288,  289,  290,  291,
   -1,   -1,  294,   -1,   -1,   -1,  298,  299,   -1,  301,
  302,  303,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  313,   -1,  315,   -1,  317,  318,   -1,  320,   -1,
  322,  323,  324,  325,  326,  327,  328,  329,  330,   -1,
  332,  333,   -1,  335,  336,   -1,   -1,   -1,   -1,  341,
   -1,   -1,   -1,  345,   -1,   -1,   -1,   -1,  350,  351,
  352,  353,  354,   -1,   -1,   -1,  358,   -1,  360,   -1,
   -1,   -1,   -1,  365,   -1,  367,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
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
   -1,   -1,  263,  264,   -1,  266,   -1,   -1,  269,  270,
   -1,   -1,  407,  274,  409,   -1,  411,  278,  413,   -1,
  415,   -1,  417,  284,  419,   -1,  287,   -1,   -1,   -1,
   -1,   -1,   -1,  294,   -1,   -1,   -1,   -1,  299,   -1,
  301,  302,  303,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  315,   -1,  317,  318,   -1,  320,
   -1,   -1,  323,   -1,  325,   -1,  327,  328,  329,  330,
   -1,  332,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  342,   -1,  344,  345,  346,   -1,  348,  349,  350,
  351,  352,  353,  354,   -1,   -1,   -1,  358,  359,  360,
   -1,   -1,  363,  364,  365,   -1,  367,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  377,   -1,  379,   -1,
  381,   -1,  383,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  263,  264,   -1,  266,
   -1,   -1,  269,  270,   -1,   -1,  407,  274,  409,   -1,
  411,  278,  413,   -1,  415,   -1,  417,  284,  419,   -1,
  287,   -1,   -1,   -1,   -1,   -1,   -1,  294,   -1,   -1,
   -1,   -1,  299,   -1,  301,  302,  303,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  315,   -1,
  317,  318,   -1,  320,   -1,   -1,  323,   -1,  325,   -1,
  327,  328,  329,  330,   -1,  332,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  342,   -1,  344,  345,  346,
   -1,  348,  349,  350,  351,  352,  353,  354,   -1,   -1,
   -1,  358,  359,  360,   -1,   -1,  363,  364,  365,   -1,
  367,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  377,   -1,  379,   -1,  381,   -1,  383,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  263,  264,   -1,  266,   -1,   -1,  269,  270,   -1,   -1,
  407,  274,  409,   -1,  411,  278,  413,   -1,  415,   -1,
  417,  284,  419,   -1,  287,   -1,   -1,   -1,   -1,   -1,
   -1,  294,   -1,   -1,   -1,   -1,  299,   -1,  301,  302,
  303,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  315,   -1,  317,  318,   -1,  320,   -1,   -1,
  323,   -1,  325,   -1,  327,  328,  329,  330,   -1,  332,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  342,
   -1,  344,  345,  346,   -1,  348,  349,  350,  351,  352,
  353,  354,   -1,   -1,   -1,  358,  359,  360,   -1,   -1,
  363,  364,  365,   -1,  367,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  377,   -1,  379,   -1,  381,   -1,
  383,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  263,  264,   -1,  266,   -1,   -1,
  269,  270,   -1,   -1,  407,  274,  409,   -1,  411,  278,
  413,   -1,  415,   -1,  417,  284,  419,   -1,  287,   -1,
   -1,   -1,   -1,   -1,   -1,  294,   -1,   -1,   -1,   -1,
  299,   -1,  301,  302,  303,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  315,   -1,  317,  318,
   -1,  320,   -1,   -1,  323,   -1,  325,   -1,  327,  328,
  329,  330,   -1,  332,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  342,   -1,  344,  345,  346,   -1,  348,
  349,  350,  351,  352,  353,  354,   -1,   -1,   -1,  358,
  359,  360,   -1,   -1,  363,  364,  365,   -1,  367,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  381,   -1,  383,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  263,  264,
   -1,  266,   -1,   -1,  269,  270,   -1,   -1,  407,  274,
  409,   -1,  411,  278,  413,   -1,  415,   -1,  417,  284,
  419,   -1,  287,   -1,   -1,   -1,   -1,   -1,   -1,  294,
   -1,   -1,   -1,   -1,  299,   -1,  301,  302,  303,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  315,   -1,  317,  318,   -1,  320,   -1,   -1,  323,   -1,
  325,   -1,  327,  328,  329,  330,   -1,  332,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  342,   -1,  344,
  345,  346,   -1,  348,  349,  350,  351,  352,  353,  354,
   -1,   -1,   -1,  358,  359,  360,   -1,   -1,  363,  364,
  365,   -1,  367,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  381,   -1,  383,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  263,  264,   -1,  266,   -1,   -1,  269,  270,
   -1,   -1,  407,  274,  409,   -1,  411,  278,  413,   -1,
  415,   -1,  417,  284,  419,   -1,  287,   -1,   -1,   -1,
   -1,   -1,   -1,  294,   -1,   -1,   -1,   -1,  299,   -1,
  301,  302,  303,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  315,   -1,  317,  318,   -1,  320,
   -1,   -1,  323,   -1,  325,   -1,  327,  328,  329,  330,
   -1,  332,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  342,   -1,  344,  345,  346,   -1,  348,  349,  350,
  351,  352,  353,  354,   -1,   -1,   -1,   -1,  359,  360,
   -1,   -1,  363,  364,  365,   -1,  367,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  381,   -1,  383,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  263,  264,   -1,  266,
   -1,   -1,  269,  270,   -1,   -1,  407,  274,  409,   -1,
  411,  278,  413,   -1,  415,   -1,  417,  284,  419,   -1,
  287,   -1,   -1,   -1,   -1,   -1,   -1,  294,   -1,   -1,
   -1,   -1,  299,   -1,  301,  302,  303,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  315,   -1,
  317,  318,   -1,  320,   -1,   -1,  323,   -1,  325,   -1,
  327,  328,  329,  330,   -1,  332,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  342,   -1,  344,  345,  346,
   -1,  348,  349,  350,  351,  352,  353,  354,   -1,   -1,
   -1,  358,  359,  360,   -1,   -1,   -1,  364,  365,   -1,
  367,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  381,   -1,  383,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  263,  264,   -1,  266,   -1,   -1,  269,  270,   -1,   -1,
  407,  274,  409,   -1,  411,  278,  413,   -1,  415,   -1,
  417,  284,  419,   -1,  287,   -1,   -1,   -1,   -1,   -1,
   -1,  294,   -1,   -1,   -1,   -1,  299,   -1,  301,  302,
  303,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  315,   -1,  317,  318,   -1,  320,   -1,   -1,
  323,   -1,  325,   -1,  327,  328,  329,  330,   -1,  332,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  342,
   -1,  344,  345,  346,   -1,  348,  349,  350,  351,  352,
  353,  354,   -1,   -1,   -1,   -1,  359,  360,   -1,   -1,
  363,  364,  365,   -1,  367,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  381,   -1,
  383,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  263,  264,   -1,  266,   -1,   -1,
  269,  270,   -1,   -1,  407,  274,  409,   -1,  411,  278,
  413,   -1,  415,   -1,  417,  284,  419,   -1,  287,   -1,
   -1,   -1,   -1,   -1,   -1,  294,   -1,   -1,   -1,   -1,
  299,   -1,  301,  302,  303,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  315,   -1,  317,  318,
   -1,  320,   -1,   -1,  323,   -1,  325,   -1,  327,  328,
  329,  330,   -1,  332,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  342,   -1,  344,  345,  346,   -1,  348,
  349,  350,  351,  352,  353,  354,   -1,   -1,   -1,  358,
  359,  360,   -1,   -1,   -1,  364,  365,   -1,  367,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  381,   -1,  383,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  263,  264,
   -1,  266,   -1,   -1,  269,  270,   -1,   -1,  407,  274,
  409,   -1,  411,  278,  413,   -1,  415,   -1,  417,  284,
  419,   -1,  287,   -1,   -1,   -1,   -1,   -1,   -1,  294,
   -1,   -1,   -1,   -1,  299,   -1,  301,  302,  303,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  315,   -1,  317,  318,   -1,  320,   -1,   -1,  323,   -1,
  325,   -1,  327,  328,  329,  330,   -1,  332,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  342,   -1,  344,
  345,  346,   -1,  348,  349,  350,  351,  352,  353,  354,
   -1,   -1,   -1,  358,   -1,  360,   -1,   -1,   -1,  364,
  365,   -1,  367,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  381,   -1,  383,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  263,  264,   -1,  266,   -1,   -1,  269,  270,
   -1,   -1,  407,  274,  409,   -1,  411,  278,  413,   -1,
  415,   -1,  417,  284,  419,   -1,  287,   -1,   -1,   -1,
   -1,   -1,   -1,  294,   -1,   -1,   -1,   -1,  299,   -1,
  301,  302,  303,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  315,   -1,  317,  318,   -1,  320,
   -1,   -1,  323,   -1,  325,   -1,  327,  328,  329,  330,
   -1,  332,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  342,   -1,  344,  345,  346,   -1,  348,  349,  350,
  351,  352,  353,  354,   -1,   -1,   -1,  358,   -1,  360,
   -1,   -1,   -1,  364,  365,   -1,  367,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  381,   -1,  383,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  263,  264,   -1,  266,
   -1,   -1,  269,  270,   -1,   -1,  407,  274,  409,   -1,
  411,  278,  413,   -1,  415,   -1,  417,  284,  419,   -1,
  287,   -1,   -1,   -1,   -1,   -1,   -1,  294,   -1,   -1,
   -1,   -1,  299,   -1,  301,  302,  303,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  315,   -1,
  317,  318,   -1,  320,   -1,   -1,  323,   -1,  325,   -1,
  327,  328,  329,  330,   -1,  332,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  342,   -1,  344,  345,  346,
   -1,  348,  349,  350,  351,  352,  353,  354,   -1,   -1,
   -1,  358,   -1,  360,   -1,   -1,   -1,  364,  365,   -1,
  367,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  383,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  263,  264,   -1,  266,   -1,   -1,  269,  270,   -1,   -1,
  407,  274,  409,   -1,  411,  278,  413,   -1,  415,   -1,
  417,  284,  419,   -1,  287,   -1,   -1,   -1,   -1,   -1,
   -1,  294,   -1,   -1,   -1,   -1,  299,   -1,  301,  302,
  303,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  315,   -1,  317,  318,   -1,  320,   -1,   -1,
  323,   -1,  325,   -1,  327,  328,  329,  330,   -1,  332,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  342,
   -1,  344,  345,  346,   -1,  348,  349,  350,  351,  352,
  353,  354,   -1,   -1,   -1,  358,   -1,  360,   -1,   -1,
   -1,  364,  365,   -1,  367,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  383,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  263,  264,   -1,  266,   -1,   -1,
  269,  270,   -1,   -1,  407,  274,  409,   -1,  411,  278,
  413,   -1,  415,   -1,  417,  284,  419,   -1,  287,   -1,
   -1,   -1,   -1,   -1,   -1,  294,   -1,   -1,   -1,   -1,
  299,   -1,  301,  302,  303,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  315,   -1,  317,  318,
   -1,  320,   -1,   -1,  323,   -1,  325,   -1,  327,  328,
  329,  330,   -1,  332,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  342,   -1,  344,  345,  346,   -1,  348,
  349,  350,  351,  352,  353,  354,   -1,   -1,   -1,  358,
   -1,  360,   -1,   -1,   -1,   -1,  365,   -1,  367,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  263,  264,
   -1,  266,   -1,   -1,  269,  270,   -1,   -1,  407,  274,
  409,   -1,  411,  278,  413,   -1,  415,   -1,  417,  284,
  419,   -1,  287,   -1,   -1,   -1,   -1,   -1,   -1,  294,
   -1,   -1,   -1,   -1,  299,   -1,  301,  302,  303,   -1,
  305,   -1,   -1,   -1,   -1,   -1,   -1,  312,   -1,   -1,
  315,   -1,  317,  318,   -1,  320,   -1,   -1,  323,   -1,
  325,   -1,  327,  328,  329,  330,   -1,  332,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  345,   -1,   -1,   -1,   -1,   -1,  351,  352,  353,  354,
   -1,   -1,   -1,  358,   -1,  360,   -1,   -1,   -1,   -1,
  365,   -1,  367,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  263,  264,   -1,  266,   -1,   -1,  269,  270,
   -1,   -1,  407,  274,  409,   -1,  411,  278,  413,   -1,
  415,   -1,  417,  284,  419,   -1,  287,   -1,   -1,   -1,
   -1,   -1,   -1,  294,   -1,   -1,   -1,   -1,  299,   -1,
  301,  302,  303,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  315,   -1,  317,  318,   -1,  320,
   -1,   -1,  323,   -1,  325,   -1,  327,  328,  329,  330,
   -1,  332,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  341,  342,   -1,   -1,  345,   -1,   -1,   -1,   -1,   -1,
  351,  352,  353,  354,   -1,   -1,   -1,  358,   -1,  360,
   -1,   -1,   -1,   -1,  365,   -1,  367,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  263,  264,   -1,  266,
   -1,   -1,  269,  270,   -1,   -1,  407,  274,  409,   -1,
  411,  278,  413,   -1,  415,   -1,  417,  284,  419,   -1,
  287,   -1,   -1,   -1,   -1,   -1,   -1,  294,   -1,   -1,
   -1,   -1,  299,   -1,  301,  302,  303,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  315,   -1,
  317,  318,   -1,  320,   -1,   -1,  323,   -1,  325,   -1,
  327,  328,  329,  330,   -1,  332,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  341,  342,   -1,   -1,  345,   -1,
   -1,   -1,   -1,   -1,  351,  352,  353,  354,   -1,   -1,
   -1,  358,   -1,  360,   -1,   -1,   -1,   -1,  365,   -1,
  367,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  263,  264,   -1,  266,   -1,   -1,  269,  270,   -1,   -1,
  407,  274,  409,   -1,  411,  278,  413,   -1,  415,   -1,
  417,  284,  419,   -1,  287,   -1,   -1,   -1,   -1,   -1,
   -1,  294,   -1,   -1,   -1,   -1,  299,   -1,  301,  302,
  303,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  315,   -1,  317,  318,   -1,  320,   -1,   -1,
  323,   -1,  325,   -1,  327,  328,  329,  330,   -1,  332,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  345,   -1,   -1,  348,   -1,  350,  351,  352,
  353,  354,   -1,   -1,   -1,  358,   -1,  360,   -1,   -1,
   -1,   -1,  365,   -1,  367,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  263,  264,   -1,  266,   -1,   -1,
  269,  270,   -1,   -1,  407,  274,  409,   -1,  411,  278,
  413,   -1,  415,   -1,  417,  284,  419,   -1,  287,   -1,
   -1,   -1,   -1,   -1,   -1,  294,   -1,   -1,   -1,   -1,
  299,   -1,  301,  302,  303,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  315,   -1,  317,  318,
   -1,  320,   -1,   -1,  323,   -1,  325,   -1,  327,  328,
  329,  330,   -1,  332,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  345,   -1,   -1,  348,
   -1,   -1,  351,  352,  353,  354,   -1,   -1,   -1,  358,
   -1,  360,   -1,   -1,   -1,   -1,  365,   -1,  367,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  263,  264,
   -1,  266,   -1,   -1,  269,  270,   -1,   -1,  407,  274,
  409,   -1,  411,  278,  413,   -1,  415,   -1,  417,  284,
  419,   -1,  287,   -1,   -1,   -1,   -1,   -1,   -1,  294,
   -1,   -1,   -1,   -1,  299,   -1,  301,  302,  303,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  315,   -1,  317,  318,   -1,  320,   -1,   -1,  323,   -1,
  325,   -1,  327,  328,  329,  330,   -1,  332,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  341,   -1,   -1,   -1,
  345,   -1,   -1,   -1,   -1,   -1,  351,  352,  353,  354,
   -1,   -1,   -1,  358,   -1,  360,   -1,   -1,   -1,   -1,
  365,   -1,  367,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  263,  264,   -1,  266,   -1,   -1,  269,  270,
   -1,   -1,  407,  274,  409,   -1,  411,  278,  413,   -1,
  415,   -1,  417,  284,  419,   -1,  287,   -1,   -1,   -1,
   -1,   -1,   -1,  294,   -1,   -1,   -1,   -1,  299,   -1,
  301,  302,  303,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  315,   -1,  317,  318,   -1,  320,
   -1,   -1,  323,   -1,  325,   -1,  327,  328,  329,  330,
   -1,  332,   -1,   -1,  335,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  345,   -1,   -1,   -1,   -1,   -1,
  351,  352,  353,  354,   -1,   -1,   -1,  358,   -1,  360,
   -1,   -1,   -1,   -1,  365,   -1,  367,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  263,  264,   -1,  266,
   -1,   -1,  269,  270,   -1,   -1,  407,  274,  409,   -1,
  411,  278,  413,   -1,  415,   -1,  417,  284,  419,   -1,
  287,   -1,   -1,   -1,   -1,   -1,   -1,  294,   -1,   -1,
   -1,   -1,  299,   -1,  301,  302,  303,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  315,   -1,
  317,  318,   -1,  320,   -1,   -1,  323,   -1,  325,   -1,
  327,  328,  329,  330,   -1,  332,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  345,   -1,
   -1,   -1,   -1,  350,  351,  352,  353,  354,   -1,   -1,
   -1,  358,   -1,  360,   -1,   -1,   -1,   -1,  365,   -1,
  367,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  263,  264,   -1,  266,   -1,   -1,  269,  270,   -1,   -1,
  407,  274,  409,   -1,  411,  278,  413,   -1,  415,   -1,
  417,  284,  419,   -1,  287,   -1,   -1,   -1,   -1,   -1,
   -1,  294,   -1,   -1,   -1,   -1,  299,   -1,  301,  302,
  303,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  315,   -1,  317,  318,   -1,  320,   -1,   -1,
  323,   -1,  325,   -1,  327,  328,  329,  330,   -1,  332,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  345,   -1,   -1,   -1,   -1,   -1,  351,  352,
  353,  354,   -1,   -1,   -1,  358,   -1,  360,   -1,   -1,
   -1,   -1,  365,   -1,  367,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  263,  264,   -1,  266,   -1,   -1,
  269,  270,   -1,   -1,  407,  274,  409,   -1,  411,  278,
  413,   -1,  415,   -1,  417,  284,  419,   -1,  287,   -1,
   -1,   -1,   -1,   -1,   -1,  294,   -1,   -1,   -1,   -1,
  299,   -1,  301,  302,  303,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  315,   -1,  317,  318,
   -1,  320,   -1,   -1,  323,   -1,  325,   -1,  327,  328,
  329,  330,   -1,  332,   -1,  264,  335,  266,   -1,   -1,
  269,   -1,  271,  272,   -1,  274,  345,  276,   -1,  278,
   -1,  280,  281,  282,   -1,   -1,   -1,   -1,  287,   -1,
   -1,   -1,   -1,  292,   -1,  294,  295,   -1,   -1,   -1,
  299,   -1,   -1,   -1,  303,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  315,   -1,  317,   -1,
   -1,  320,  321,   -1,   -1,   -1,   -1,   -1,   -1,  328,
  329,   -1,   -1,  332,   -1,   -1,  335,   -1,  407,   -1,
  409,   -1,  411,   -1,  413,   -1,  415,  264,  417,  266,
  419,   -1,  269,   -1,  271,  272,   -1,  274,   -1,  276,
   -1,  278,   -1,  280,  281,  282,   -1,   -1,   -1,   -1,
  287,  264,   -1,  266,   -1,  292,  269,  294,  295,   -1,
   -1,  274,  299,   -1,   -1,  278,  303,   -1,  281,   -1,
   -1,   -1,   -1,   -1,  287,   -1,   -1,   -1,  315,   -1,
  317,  294,   -1,  320,  321,   -1,  299,   -1,  301,   -1,
  303,  328,  329,   -1,   -1,  332,   -1,   -1,  335,   -1,
  419,   -1,  315,  264,  317,  266,   -1,  320,  269,   -1,
   -1,   -1,   -1,  274,   -1,  328,  329,  278,   -1,  332,
  281,   -1,  335,   -1,   -1,   -1,  287,   -1,   -1,  342,
   -1,   -1,   -1,  294,   -1,   -1,   -1,   -1,  299,   -1,
  301,   -1,  303,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  315,   -1,  317,   -1,   -1,  320,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  328,  329,   -1,
  264,  332,  266,   -1,  335,  269,   -1,   -1,   -1,   -1,
  274,  342,  419,   -1,  278,   -1,   -1,   -1,   -1,  264,
   -1,  266,   -1,  287,  269,   -1,   -1,   -1,   -1,  274,
  294,   -1,   -1,  278,   -1,  299,  419,   -1,   -1,  303,
   -1,  305,  287,  307,   -1,   -1,   -1,   -1,  312,  294,
   -1,  315,   -1,  317,  299,   -1,  320,   -1,  303,   -1,
  305,   -1,  307,   -1,  328,  329,   -1,  312,  332,   -1,
  315,   -1,  317,   -1,   -1,  320,  264,   -1,  266,   -1,
   -1,  269,  346,  328,  329,   -1,  274,  332,  419,   -1,
  278,   -1,   -1,  281,   -1,  264,   -1,  266,   -1,  287,
  269,   -1,   -1,   -1,   -1,  274,  294,   -1,   -1,  278,
   -1,  299,   -1,   -1,   -1,  303,   -1,   -1,  287,   -1,
   -1,   -1,   -1,   -1,   -1,  294,   -1,  315,   -1,  317,
  299,   -1,  320,   -1,  303,   -1,   -1,   -1,   -1,   -1,
  328,  329,   -1,   -1,  332,   -1,  315,  335,  317,   -1,
   -1,  320,  264,   -1,  266,  419,   -1,  269,   -1,  328,
  329,   -1,  274,  332,   -1,   -1,  278,   -1,   -1,   -1,
   -1,  264,  341,  266,  419,  287,  269,   -1,   -1,   -1,
  349,  274,  294,   -1,   -1,  278,   -1,  299,  281,   -1,
   -1,  303,   -1,   -1,  287,   -1,   -1,   -1,   -1,   -1,
   -1,  294,   -1,  315,   -1,  317,  299,   -1,  320,   -1,
  303,   -1,   -1,   -1,   -1,   -1,  328,  329,   -1,   -1,
  332,   -1,  315,   -1,  317,   -1,   -1,  320,   -1,  341,
   -1,  419,   -1,  261,   -1,  328,  329,  349,  264,  332,
  266,   -1,  335,  269,  264,   -1,  266,   -1,  274,  269,
  419,   -1,  278,   -1,  274,   -1,   -1,   -1,  278,   -1,
   -1,  287,   -1,   -1,   -1,   -1,   -1,  287,  294,  297,
   -1,   -1,   -1,  299,  294,   -1,   -1,  303,   -1,  299,
   -1,   -1,   -1,  303,   -1,   -1,   -1,   -1,   -1,  315,
   -1,  317,   -1,   -1,  320,  315,   -1,  317,   -1,   -1,
  320,   -1,  328,  329,   -1,   -1,  332,  419,  328,  329,
   -1,   -1,  332,   -1,   -1,  343,   -1,  345,   -1,   -1,
   -1,   -1,   -1,   -1,  352,  353,  419,  355,  356,  357,
  358,  359,  360,  361,  362,  363,  364,  365,   -1,  367,
   -1,  369,   -1,  371,   -1,  373,   -1,  375,   -1,  377,
   -1,  379,   -1,  381,   -1,  383,   -1,  385,   -1,  387,
   -1,  389,   -1,  391,   -1,  393,   -1,  395,   -1,  397,
   -1,  399,   -1,  401,   -1,  403,   -1,  257,   -1,   -1,
  260,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  271,   -1,  419,   -1,   -1,  276,   -1,   -1,  419,
  280,   -1,   -1,  283,   -1,   -1,   -1,   -1,   -1,   -1,
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
  280,   -1,   -1,  283,   -1,   -1,   -1,  342,  343,   -1,
   -1,   -1,   -1,   -1,   -1,  295,  296,   -1,   -1,   -1,
  300,  301,   -1,   -1,  260,   -1,  306,   -1,  308,  309,
  310,  311,   -1,   -1,   -1,  271,  316,   -1,   -1,  319,
  276,  321,   -1,   -1,  280,   -1,   -1,  283,   -1,   -1,
   -1,   -1,   -1,   -1,  334,   -1,   -1,   -1,   -1,  295,
  296,   -1,   -1,  343,  300,  301,   -1,   -1,  260,   -1,
  306,   -1,  308,  309,  310,  311,   -1,   -1,   -1,  271,
  316,   -1,   -1,  319,  276,  321,   -1,   -1,  280,   -1,
   -1,  283,   -1,   -1,   -1,   -1,   -1,   -1,  334,   -1,
   -1,   -1,   -1,  295,  296,   -1,  342,  343,   -1,  301,
   -1,   -1,   -1,   -1,  306,   -1,  308,  309,  310,  311,
  257,   -1,   -1,  260,  316,   -1,   -1,  319,   -1,  321,
   -1,   -1,   -1,   -1,  271,   -1,   -1,   -1,   -1,  276,
   -1,   -1,  334,  280,   -1,   -1,  283,   -1,   -1,   -1,
  342,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  295,  296,
   -1,   -1,  257,  300,  301,  260,   -1,   -1,   -1,  306,
   -1,  308,  309,  310,  311,   -1,  271,   -1,   -1,  316,
   -1,  276,  319,   -1,  321,  280,   -1,   -1,  283,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  334,   -1,   -1,
  295,  296,   -1,   -1,   -1,  343,  301,  345,  346,  347,
  348,  306,  350,  308,  309,  310,  311,  355,   -1,   -1,
   -1,  316,   -1,   -1,  319,   -1,  321,  365,  343,  367,
  345,  346,  347,  348,   -1,  350,   -1,   -1,   -1,  334,
  355,   -1,   -1,   -1,   -1,   -1,   -1,  385,   -1,  387,
  365,  389,  367,  391,   -1,  393,   -1,  395,   -1,  397,
   -1,  399,   -1,  401,   -1,  403,   -1,   -1,   -1,   -1,
  385,   -1,  387,   -1,  389,   -1,  391,   -1,  393,   -1,
  395,  419,  397,   -1,  399,   -1,  401,  343,  403,  345,
  346,  347,  348,   -1,  350,   -1,   -1,   -1,  343,  355,
  345,  346,  347,  348,  419,  350,   -1,   -1,   -1,  365,
  355,  367,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  365,   -1,  367,   -1,   -1,   -1,   -1,   -1,   -1,  385,
   -1,  387,   -1,  389,   -1,  391,   -1,  393,   -1,  395,
  385,  397,  387,  399,  389,  401,  391,  403,  393,  343,
  395,  345,  397,  347,  399,   -1,  401,   -1,  403,   -1,
   -1,  355,   -1,  419,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  365,   -1,  367,  419,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  385,   -1,  387,   -1,  389,   -1,  391,   -1,  393,
   -1,  395,   -1,  397,   -1,  399,   -1,  401,   -1,  403,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  419,
  };

#line 2540 "cs-parser.jay"


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
{
	current_namespace = new Namespace (null, "");
	this.tree = tree;
	this.name = name;
	this.input = input;
	current_container = tree.Types;
	current_container.Namespace = current_namespace;

	lexer = new Tokenizer (input, name);
	type_references = new TypeRefManager (); 
}

public int parse ()
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

#line 5268 "-"
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
