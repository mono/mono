// created by jay 0.7 (c) 1998 Axel.Schreiner@informatik.uni-osnabrueck.de

#line 2 "mb-parser.jay"
//
// Mono.MonoBASIC.Parser.cs (from .jay): The Parser for the MonoBASIC compiler
//
// Authors: A Rafael D Teixeira (rafaelteixeirabr@hotmail.com)
//	    Anirban Bhattacharjee (banirban@novell.com)
//          Jambunathan K (kjambunathan@novell.com)
//
// Licensed under the terms of the GNU GPL
//
// Copyright (C) 2001, 2002, 2003, 2004 A Rafael D Teixeira
// Copyright (C) 2003, 2004 Novell
//
//

namespace Mono.CSharp
{
	using System.Text;
	using System;
	using System.Reflection;
	using System.Collections;
	using Mono.CSharp;

	/// <summary>
	///    The MonoBASIC Parser
	/// </summary>
// 	[DefaultParser]
// 	public class Parser : GenericParser
// 	{
	

// 		/// <summary>
// 		///   Current block is used to add statements as we find
// 		///   them.  
// 		/// </summary>
// 		Block      current_block;
		
// 		/// <summary>
// 		///   Tmp block is used to store block endings in if/select's
// 		/// </summary>
// 		Block      tmp_block;		

// 		/// <summary>
// 		///   Tmp block is used to store tmp copies of expressions
// 		/// </summary>
// 		Expression      tmp_expr;	
		
// 		/// <summary>
// 		///   Tmp catch is used to store catch clauses in try..catch..finally
// 		/// </summary>
// 		ArrayList      tmp_catch_clauses;			
		
// 		/// <summary>
// 		///   Current interface is used by the various declaration
// 		///   productions in the interface declaration to "add"
// 		///   the interfaces as we find them.
// 		/// </summary>
// 		Interface  current_interface;

// 		/// <summary>
// 		///   This is used by the unary_expression code to resolve
// 		///   a name against a parameter.  
// 		/// </summary>
// 		Parameters current_local_parameters;
		
// 		/// <summary>
// 		///   This are used when parsing parameters in property
// 		///   declarations.
// 		/// </summary>		
// 		Parameters set_parameters;
// 		Parameters get_parameters;
		
// 		/// <summary>
// 		///   This is used by the sub_header parser to store modifiers
// 		///   to be passed to sub/constructor  
// 		/// </summary>
// 		int current_modifiers;		
			
// 		/// <summary>
// 		///   This is used by the sub_header parser to store attributes
// 		///   to be passed to sub/constructor  
// 		/// </summary>
// 		Attributes current_attributes;				

// 		/// <summary>
// 		///   Using during property parsing to describe the implicit
// 		///   value parameter that is passed to the "set" accessor
// 		///   method
// 		/// </summary>
// 		string get_implicit_value_parameter_name;
		
// 		// <summary>
// 		//   Using during property parsing to describe the implicit
// 		//   value parameter that is passed to the "set" and "get"accesor
// 		//   methods (properties and indexers).
// 		// </summary>
// 		Expression get_implicit_value_parameter_type;
		
// 		/// <summary>
// 		///   Using during property parsing to describe the implicit
// 		///   value parameter that is passed to the "set" accessor
// 		///   method
// 		/// </summary>
// 		string set_implicit_value_parameter_name;
		
// 		// <summary>
// 		//   Using during property parsing to describe the implicit
// 		//   value parameter that is passed to the "set" and "get"accesor
// 		//   methods (properties and indexers).
// 		// </summary>
// 		Expression set_implicit_value_parameter_type;		
		
// 		Location member_location;
		
// 		// An out-of-band stack.
// 		//
// 		Stack oob_stack;
		
// 		ArrayList current_rank_specifiers;

// 		DoOptions do_type;
// 		//
// 		// Switch stack.
// 		//
// 		Stack switch_stack;
		
// 		// Expression stack for nested ifs
// 		Stack expr_stack; 
		
// 		Stack tmp_blocks;
// 	    	Stack statement_stack;

// 		// A stack for With expressions.
// 		//
// 		Stack with_stack;
	
		
// 		static public bool InitialOptionExplicit = false;
// 		static public bool InitialOptionStrict = false;
// 		static public bool InitialOptionCompareBinary = true;
// 		static public ArrayList ImportsList = null;

// 		bool OptionExplicit;
// 		bool OptionStrict;
// 		bool OptionCompareBinary;

// 		static public bool UseExtendedSyntax; // for ".mbs" files

// 		bool implicit_modifiers;
		
// 		public override string[] extensions()
// 		{
// 			string [] list = { ".vb", ".mbs" };
// 			return list;
// 		}


	/// <summary>
	///    The C# Parser
	/// </summary>
	public class CSharpParser {
		NamespaceEntry  current_namespace;
		TypeContainer   current_container;
		TypeContainer	current_class;
	
		IIteratorContainer iterator_container;

		/// <summary>
		///   Current block is used to add statements as we find
		///   them.  
		/// </summary>
		Block      current_block, top_current_block;

		/// <summary>
		///   This is used by the unary_expression code to resolve
		///   a name against a parameter.  
		/// </summary>
		Parameters current_local_parameters;

		/// <summary>
		///   Using during property parsing to describe the implicit
		///   value parameter that is passed to the "set" and "get"accesor
		///   methods (properties and indexers).
		/// </summary>
		Expression implicit_value_parameter_type;
		Parameters indexer_parameters;

		/// <summary>
		///   Used to determine if we are parsing the get/set pair
		///   of an indexer or a property
		/// </summmary>
		bool  parsing_indexer;

		///
		/// An out-of-band stack.
		///
		Stack oob_stack;

		///
		/// Switch stack.
		///
		Stack switch_stack;

		static public int yacc_verbose_flag;

		// Name of the file we are parsing
		public string name;

		///
		/// The current file.
		///
		SourceFile file;
		
		///   This is used by the sub_header parser to store modifiers
		///   to be passed to sub/constructor  
		int current_modifiers;		
			
		///   This is used by the sub_header parser to store attributes
		///   to be passed to sub/constructor  
		Attributes current_attributes;				

		///   This is used by the attributes parser to syntactically
		///   validate the attribute rules  
		bool allow_global_attribs = true;

		bool expecting_global_attribs = false;
		bool expecting_local_attribs = false;

		bool local_attrib_section_added = false;

		///FIXME
		ArrayList current_rank_specifiers;

		/// <summary>
		///   Using during property parsing to describe the implicit
		///   value parameter that is passed to the "set" accessor
		///   method
		/// </summary>
		string get_implicit_value_parameter_name;
		
		// <summary>
		//   Using during property parsing to describe the implicit
		//   value parameter that is passed to the "set" and "get"accesor
		//   methods (properties and indexers).
		// </summary>
		Expression get_implicit_value_parameter_type;
		
		/// <summary>
		///   Using during property parsing to describe the implicit
		///   value parameter that is passed to the "set" accessor
		///   method
		/// </summary>
		string set_implicit_value_parameter_name;
		
		// <summary>
		//   Using during property parsing to describe the implicit
		//   value parameter that is passed to the "set" and "get"accesor
		//   methods (properties and indexers).
		// </summary>
		Expression set_implicit_value_parameter_type;		

		/// <summary>
		///   This are used when parsing parameters in property
		///   declarations.
		/// </summary>		
		Parameters set_parameters;
		Parameters get_parameters;

// 		static public ArrayList ImportsList = null;

		bool implicit_modifiers;


		/// <summary>
		///   This is used as a helper class for handling of
		///   pre-processor statements.
		/// </summary>		

		// FIXME: This class MBASException is actually a kludge 
		// It can be done away with a more elegant replacement.

		public class MBASException : ApplicationException
		{
			public int code;
			public Location loc;

			public MBASException(int code, Location loc, string text) : base(text)
			{
				this.code = code;
				this.loc = loc;
			}
		}



		public class IfElseStateMachine {
			
			public enum State {
				START,
				IF_SEEN,
				ELSEIF_SEEN,
				ELSE_SEEN,
				ENDIF_SEEN,
				MAX
			}
		
			public enum Token {
				START,
				IF,
				ELSEIF,
				ELSE,
				ENDIF,
				EOF,
				MAX
			}

			State state;
			Stack stateStack;

			public static Hashtable errStrings = new Hashtable();

			int err=0;
			static int[,] errTable = new int[(int)State.MAX, (int)Token.MAX];
		
			static IfElseStateMachine()
			{
				// FIXME: Fix both the error nos and the error strings. 
				// Currently the error numbers and the error strings are 
				// just placeholders for getting the state-machine going.

				errStrings.Add(0, "");
				errStrings.Add(30012, "#If must end with a matching #End If");
				errStrings.Add(30013, "#ElseIf, #Else or #End If must be preceded by a matching #If");
				errStrings.Add(30014, "#ElseIf must be preceded by a matching #If or #ElseIf");
				errStrings.Add(30028, "#Else must be preceded by a matching #If or #ElseIf");
				errStrings.Add(32030, "#ElseIf cannot follow #Else as part of #If block");

				errTable[(int)State.START, (int)Token.IF] = 0;
				errTable[(int)State.START, (int)Token.ELSEIF] = 30014;
				errTable[(int)State.START, (int)Token.ELSE] = 30028;
				errTable[(int)State.START, (int)Token.ENDIF] = 30013;
				errTable[(int)State.START, (int)Token.EOF] = 0;

				errTable[(int)State.IF_SEEN, (int)Token.IF] = 0;
				errTable[(int)State.IF_SEEN, (int)Token.ELSEIF] = 0;
				errTable[(int)State.IF_SEEN, (int)Token.ELSE] = 0;
				errTable[(int)State.IF_SEEN, (int)Token.ENDIF] = 0;
				errTable[(int)State.IF_SEEN, (int)Token.EOF] = 30012;

				errTable[(int)State.ELSEIF_SEEN, (int)Token.IF] = 0;
				errTable[(int)State.ELSEIF_SEEN, (int)Token.ELSEIF] = 0;
				errTable[(int)State.ELSEIF_SEEN, (int)Token.ELSE] = 0;
				errTable[(int)State.ELSEIF_SEEN, (int)Token.ENDIF] = 0;
				errTable[(int)State.ELSEIF_SEEN, (int)Token.EOF] = 30012;

				errTable[(int)State.ELSE_SEEN, (int)Token.IF] = 0;
				errTable[(int)State.ELSE_SEEN, (int)Token.ELSEIF] = 32030;
				errTable[(int)State.ELSE_SEEN, (int)Token.ELSE] = 32030;
				errTable[(int)State.ELSE_SEEN, (int)Token.ENDIF] = 0;
				errTable[(int)State.ELSE_SEEN, (int)Token.EOF] = 30012;

				errTable[(int)State.ENDIF_SEEN, (int)Token.IF] = 0;
				errTable[(int)State.ENDIF_SEEN, (int)Token.ELSEIF] = 30014;
				errTable[(int)State.ENDIF_SEEN, (int)Token.ELSE] = 30028;
				errTable[(int)State.ENDIF_SEEN, (int)Token.ENDIF] = 30013;
				errTable[(int)State.ENDIF_SEEN, (int)Token.EOF] = 0;
			}

			public IfElseStateMachine()
			{
				state = State.START;

				stateStack = new Stack();
				stateStack.Push(state);
			}

			// The parameter here need not be qualified with IfElseStateMachine
			// But it hits a bug in mcs. So temporarily scoping it so that builds
			// are not broken.

			public void HandleToken(IfElseStateMachine.Token tok)
			{	
				err = (int) errTable[(int)state, (int)tok];

				if(err != 0)
					throw new ApplicationException("Unexpected pre-processor directive #"+tok); 
				
				if(tok == Token.IF) {
					stateStack.Push(state);
					state = (State) tok;
				}
				else if(tok == Token.ENDIF) {
					state = (State)stateStack.Pop();
				}
				else
					state = (State)tok;
			}

			public int Error {
				get {
					return err;
				}
			}

			public string ErrString {
				get {
					return (string) errStrings[err];
				}
			}
		}

		
		public class TokenizerController {
			
			struct State
			{
				public bool CanAcceptTokens;
				public bool CanSelectBlock;

			}

			State currentState;
			Stack stateStack;
			Tokenizer lexer;

			public TokenizerController(Tokenizer lexer)
			{
				this.lexer = lexer;
				stateStack = new Stack();

				currentState.CanAcceptTokens = true;
				currentState.CanSelectBlock = true;

				stateStack.Push(currentState);
			}

			State parentState {
				get {
					return (State)stateStack.Peek();
				}
			}

			public bool IsAcceptingTokens {
				get {
					return currentState.CanAcceptTokens;
				}
			}

			public void PositionCursorAtNextPreProcessorDirective()
			{
				lexer.PositionCursorAtNextPreProcessorDirective();
			}

			public void PositionTokenizerCursor(IfElseStateMachine.Token tok, BoolLiteral expr)
			{
				if(tok == IfElseStateMachine.Token.ENDIF) {
					currentState = (State)stateStack.Pop();

					if(currentState.CanAcceptTokens)
						return;
					else {
						PositionCursorAtNextPreProcessorDirective();
						return;
					}
				}
				
				if(tok == IfElseStateMachine.Token.IF) {
					stateStack.Push(currentState);
					
					currentState.CanAcceptTokens = parentState.CanAcceptTokens;
					currentState.CanSelectBlock = true;
				}
			
				if(parentState.CanAcceptTokens && 
				   currentState.CanSelectBlock && (bool)(expr.GetValue()) ) {
				    
					currentState.CanAcceptTokens = true;
					currentState.CanSelectBlock = false; 
					return;
				}
				else {
					currentState.CanAcceptTokens = false;
					PositionCursorAtNextPreProcessorDirective();
					return;
				}
			}
		}

		bool in_external_source = false;
		int in_marked_region = 0;

		TokenizerController tokenizerController;
		IfElseStateMachine ifElseStateMachine;



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

  protected static  int yyFinal = 2;
  public static  string [] yyRule = {
    "$accept : compilation_unit",
    "end_of_stmt : logical_end_of_line",
    "end_of_stmt : COLON",
    "logical_end_of_line : EOL",
    "logical_end_of_line : logical_end_of_line pp_directive",
    "compilation_unit : logical_end_of_line opt_option_directives opt_imports_directives declarations EOF",
    "compilation_unit : logical_end_of_line opt_option_directives opt_imports_directives opt_attributes EOF",
    "opt_option_directives :",
    "opt_option_directives : option_directives",
    "option_directives : option_directive",
    "option_directives : option_directives option_directive",
    "option_directive : option_explicit_directive",
    "option_directive : option_strict_directive",
    "option_directive : option_compare_directive",
    "on_off :",
    "on_off : ON",
    "on_off : OFF",
    "text_or_binary : BINARY",
    "text_or_binary : TEXT",
    "option_explicit_directive : OPTION EXPLICIT on_off logical_end_of_line",
    "option_strict_directive : OPTION STRICT on_off logical_end_of_line",
    "option_compare_directive : OPTION COMPARE text_or_binary logical_end_of_line",
    "opt_declarations :",
    "opt_declarations : declarations",
    "declarations : declaration",
    "declarations : declarations declaration",
    "$$1 :",
    "declaration : declaration_qualifiers $$1 namespace_declaration",
    "$$2 :",
    "declaration : declaration_qualifiers $$2 type_spec_declaration",
    "identifier : IDENTIFIER",
    "identifier : BINARY",
    "identifier : TEXT",
    "identifier : COMPARE",
    "identifier : EXPLICIT",
    "identifier : OFF",
    "type_character : PERCENT",
    "type_character : LONGTYPECHAR",
    "type_character : AT_SIGN",
    "type_character : SINGLETYPECHAR",
    "type_character : NUMBER_SIGN",
    "type_character : DOLAR_SIGN",
    "opt_type_character :",
    "opt_type_character : type_character",
    "qualified_identifier : identifier",
    "qualified_identifier : qualified_identifier DOT identifier",
    "opt_imports_directives :",
    "opt_imports_directives : imports_directives",
    "imports_directives : imports_directive",
    "imports_directives : imports_directives imports_directive",
    "imports_directive : IMPORTS imports_terms logical_end_of_line",
    "imports_terms : imports_term",
    "imports_terms : imports_terms COMMA imports_term",
    "imports_term : namespace_or_type_name",
    "imports_term : identifier ASSIGN namespace_or_type_name",
    "opt_params :",
    "opt_params : OPEN_PARENS CLOSE_PARENS",
    "opt_params : OPEN_PARENS opt_formal_parameter_list CLOSE_PARENS",
    "opt_attributes :",
    "opt_attributes : attribute_sections",
    "attribute_sections : attribute_section",
    "$$3 :",
    "attribute_sections : attribute_sections $$3 attribute_section",
    "attribute_section : OP_LT attribute_list OP_GT opt_end_of_stmt",
    "opt_end_of_stmt :",
    "opt_end_of_stmt : end_of_stmt",
    "attribute_list : attribute",
    "attribute_list : attribute_list COMMA attribute",
    "$$4 :",
    "attribute : namespace_or_type_name $$4 opt_attribute_arguments",
    "$$5 :",
    "$$6 :",
    "attribute : attribute_target_specifier $$5 COLON namespace_or_type_name $$6 opt_attribute_arguments",
    "attribute_target_specifier : ASSEMBLY",
    "attribute_target_specifier : MODULE",
    "attribute_target_specifier : namespace_or_type_name",
    "opt_attribute_arguments :",
    "opt_attribute_arguments : OPEN_PARENS opt_attribute_arguments_list CLOSE_PARENS",
    "opt_attribute_arguments_list :",
    "opt_attribute_arguments_list : attribute_arguments_list",
    "attribute_arguments_list : positional_argument_list",
    "attribute_arguments_list : positional_argument_list COMMA named_argument_list",
    "attribute_arguments_list : named_argument_list",
    "positional_argument_list : constant_expression",
    "positional_argument_list : positional_argument_list COMMA constant_expression",
    "named_argument_list : named_argument",
    "named_argument_list : named_argument_list COMMA named_argument",
    "named_argument : identifier ATTR_ASSIGN constant_expression",
    "$$7 :",
    "namespace_declaration : NAMESPACE qualified_identifier logical_end_of_line $$7 opt_declarations END NAMESPACE logical_end_of_line",
    "declaration_qualifiers : opt_attributes opt_modifiers",
    "type_spec_declaration : class_declaration",
    "type_spec_declaration : module_declaration",
    "type_spec_declaration : interface_declaration",
    "type_spec_declaration : delegate_declaration",
    "type_spec_declaration : struct_declaration",
    "type_spec_declaration : enum_declaration",
    "$$8 :",
    "$$9 :",
    "class_declaration : CLASS identifier logical_end_of_line $$8 opt_inherits opt_implements $$9 opt_class_member_declarations END CLASS logical_end_of_line",
    "opt_inherits :",
    "opt_inherits : INHERITS type_list logical_end_of_line",
    "opt_implements :",
    "opt_implements : IMPLEMENTS type_list logical_end_of_line",
    "opt_modifiers :",
    "opt_modifiers : modifiers",
    "modifiers : modifier",
    "modifiers : modifiers modifier",
    "modifier : PUBLIC",
    "modifier : PROTECTED",
    "modifier : PRIVATE",
    "modifier : SHARED",
    "modifier : FRIEND",
    "modifier : NOTINHERITABLE",
    "modifier : OVERRIDABLE",
    "modifier : NOTOVERRIDABLE",
    "modifier : OVERRIDES",
    "modifier : OVERLOADS",
    "modifier : SHADOWS",
    "modifier : MUSTINHERIT",
    "modifier : READONLY",
    "modifier : DEFAULT",
    "modifier : WRITEONLY",
    "$$10 :",
    "module_declaration : MODULE identifier logical_end_of_line $$10 opt_module_member_declarations END MODULE logical_end_of_line",
    "opt_module_member_declarations :",
    "opt_module_member_declarations : module_member_declarations",
    "module_member_declarations : module_member_declaration",
    "module_member_declarations : module_member_declarations module_member_declaration",
    "$$11 :",
    "module_member_declaration : opt_attributes opt_modifiers $$11 module_member_declarator",
    "module_member_declarator : constructor_declaration",
    "module_member_declarator : method_declaration",
    "module_member_declarator : field_declaration",
    "module_member_declarator : constant_declaration",
    "module_member_declarator : property_declaration",
    "module_member_declarator : event_declaration",
    "module_member_declarator : type_spec_declaration",
    "constant_declaration : CONST constant_declarators logical_end_of_line",
    "opt_class_member_declarations :",
    "opt_class_member_declarations : class_member_declarations",
    "class_member_declarations : class_member_declaration",
    "class_member_declarations : class_member_declarations class_member_declaration",
    "class_member_declaration : opt_attributes opt_modifiers class_member_declarator",
    "class_member_declarator : field_declaration",
    "class_member_declarator : constant_declaration",
    "class_member_declarator : method_declaration",
    "class_member_declarator : constructor_declaration",
    "class_member_declarator : property_declaration",
    "class_member_declarator : event_declaration",
    "class_member_declarator : type_spec_declaration",
    "method_declaration : sub_declaration",
    "method_declaration : func_declaration",
    "$$12 :",
    "sub_declaration : SUB identifier opt_params $$12 opt_evt_handler opt_implement_clause logical_end_of_line begin_block opt_statement_list end_block END SUB logical_end_of_line",
    "$$13 :",
    "$$14 :",
    "func_declaration : FUNCTION identifier opt_type_character opt_params opt_type_with_ranks $$13 opt_implement_clause logical_end_of_line begin_block $$14 opt_statement_list end_block END FUNCTION logical_end_of_line",
    "$$15 :",
    "$$16 :",
    "struct_declaration : STRUCTURE identifier logical_end_of_line opt_implement_clause $$15 opt_struct_member_declarations $$16 END STRUCTURE logical_end_of_line",
    "opt_logical_end_of_line :",
    "opt_logical_end_of_line : logical_end_of_line",
    "opt_struct_member_declarations :",
    "opt_struct_member_declarations : struct_member_declarations",
    "struct_member_declarations : struct_member_declaration",
    "struct_member_declarations : struct_member_declarations struct_member_declaration",
    "struct_member_declaration : opt_modifiers struct_member_declarator",
    "struct_member_declarator : field_declaration",
    "struct_member_declarator : constant_declaration",
    "struct_member_declarator : constructor_declaration",
    "struct_member_declarator : method_declaration",
    "struct_member_declarator : event_declaration",
    "struct_member_declarator : type_spec_declaration",
    "event_declaration : EVENT identifier AS type opt_implement_clause logical_end_of_line",
    "$$17 :",
    "enum_declaration : ENUM identifier opt_type_spec logical_end_of_line opt_enum_member_declarations $$17 END ENUM logical_end_of_line",
    "opt_enum_member_declarations :",
    "opt_enum_member_declarations : enum_member_declarations",
    "enum_member_declarations : enum_member_declaration",
    "enum_member_declarations : enum_member_declarations enum_member_declaration",
    "enum_member_declaration : opt_attributes identifier logical_end_of_line",
    "$$18 :",
    "enum_member_declaration : opt_attributes identifier $$18 ASSIGN expression logical_end_of_line",
    "interface_property_declaration : PROPERTY identifier opt_type_character opt_property_parameters opt_type_with_ranks logical_end_of_line",
    "$$19 :",
    "$$20 :",
    "$$21 :",
    "interface_declaration : INTERFACE identifier logical_end_of_line $$19 opt_interface_base $$20 interface_body $$21 END INTERFACE logical_end_of_line",
    "opt_interface_base :",
    "opt_interface_base : interface_bases",
    "interface_bases : interface_base",
    "interface_bases : interface_bases interface_base",
    "interface_base : INHERITS type_list logical_end_of_line",
    "interface_body : opt_interface_member_declarations",
    "opt_interface_member_declarations :",
    "opt_interface_member_declarations : interface_member_declarations",
    "interface_member_declarations : interface_member_declaration",
    "interface_member_declarations : interface_member_declarations interface_member_declaration",
    "interface_member_declaration : opt_attributes opt_modifiers interface_member_declarator",
    "interface_member_declarator : interface_method_declaration",
    "interface_member_declarator : interface_property_declaration",
    "interface_method_declaration : SUB identifier opt_params logical_end_of_line",
    "interface_method_declaration : FUNCTION identifier opt_type_character opt_params opt_type_with_ranks logical_end_of_line",
    "property_declaration : non_abstract_propery_declaration",
    "$$22 :",
    "non_abstract_propery_declaration : PROPERTY identifier opt_type_character opt_property_parameters opt_type_with_ranks opt_implement_clause logical_end_of_line $$22 accessor_declarations END PROPERTY logical_end_of_line",
    "opt_property_parameters :",
    "opt_property_parameters : OPEN_PARENS opt_formal_parameter_list CLOSE_PARENS",
    "opt_implement_clause :",
    "opt_implement_clause : IMPLEMENTS implement_clause_list",
    "implement_clause_list : qualified_identifier",
    "implement_clause_list : implement_clause_list COMMA qualified_identifier",
    "accessor_declarations : get_accessor_declaration opt_set_accessor_declaration",
    "accessor_declarations : set_accessor_declaration opt_get_accessor_declaration",
    "opt_get_accessor_declaration :",
    "opt_get_accessor_declaration : get_accessor_declaration",
    "opt_set_accessor_declaration :",
    "opt_set_accessor_declaration : set_accessor_declaration",
    "$$23 :",
    "$$24 :",
    "get_accessor_declaration : opt_attributes GET logical_end_of_line $$23 begin_block $$24 opt_statement_list end_block END GET logical_end_of_line",
    "$$25 :",
    "set_accessor_declaration : opt_attributes SET opt_set_parameter logical_end_of_line $$25 begin_block opt_statement_list end_block END SET logical_end_of_line",
    "opt_set_parameter :",
    "opt_set_parameter : OPEN_PARENS CLOSE_PARENS",
    "opt_set_parameter : OPEN_PARENS opt_parameter_modifier opt_identifier opt_type_with_ranks CLOSE_PARENS",
    "field_declaration : opt_dim_stmt variable_declarators logical_end_of_line",
    "opt_dim_stmt :",
    "opt_dim_stmt : DIM",
    "delegate_declaration : DELEGATE SUB identifier OPEN_PARENS opt_formal_parameter_list CLOSE_PARENS logical_end_of_line",
    "delegate_declaration : DELEGATE FUNCTION identifier OPEN_PARENS opt_formal_parameter_list CLOSE_PARENS opt_type_with_ranks logical_end_of_line",
    "opt_evt_handler :",
    "$$26 :",
    "$$27 :",
    "constructor_declaration : SUB NEW opt_params logical_end_of_line $$26 begin_block opt_statement_list end_block $$27 END SUB logical_end_of_line",
    "opt_formal_parameter_list :",
    "opt_formal_parameter_list : formal_parameter_list",
    "formal_parameter_list : parameters",
    "parameters : parameter",
    "parameters : parameters COMMA parameter",
    "parameter : opt_attributes opt_parameter_modifier identifier opt_type_character opt_rank_specifiers opt_type_with_ranks opt_variable_initializer",
    "opt_parameter_modifier :",
    "opt_parameter_modifier : parameter_modifiers",
    "parameter_modifiers : parameter_modifiers parameter_modifier",
    "parameter_modifiers : parameter_modifier",
    "parameter_modifier : BYREF",
    "parameter_modifier : BYVAL",
    "parameter_modifier : OPTIONAL",
    "parameter_modifier : PARAM_ARRAY",
    "opt_statement_list :",
    "opt_statement_list : statement_list end_of_stmt",
    "statement_list : statement",
    "statement_list : statement_list end_of_stmt statement",
    "block : begin_block opt_statement_list end_block",
    "begin_block :",
    "end_block :",
    "statement : declaration_statement",
    "statement : embedded_statement",
    "statement : ADDHANDLER prefixed_unary_expression COMMA ADDRESSOF expression",
    "statement : REMOVEHANDLER prefixed_unary_expression COMMA ADDRESSOF expression",
    "statement : RAISEEVENT identifier opt_raise_event_args",
    "opt_raise_event_args :",
    "opt_raise_event_args : OPEN_PARENS opt_argument_list CLOSE_PARENS",
    "embedded_statement : expression_statement",
    "embedded_statement : selection_statement",
    "embedded_statement : try_statement",
    "embedded_statement : synclock_statement",
    "embedded_statement : jump_statement",
    "jump_statement : return_statement",
    "jump_statement : goto_statement",
    "jump_statement : throw_statement",
    "goto_statement : GOTO label_name",
    "throw_statement : THROW opt_expression",
    "return_statement : RETURN opt_expression",
    "synclock_statement : SYNCLOCK expression end_of_stmt block END SYNCLOCK",
    "try_statement : try_catch",
    "try_statement : try_catch_finally",
    "try_catch : TRY end_of_stmt block opt_catch_clauses END TRY",
    "try_catch_finally : TRY end_of_stmt block opt_catch_clauses FINALLY end_of_stmt block END TRY",
    "opt_catch_clauses :",
    "opt_catch_clauses : catch_clauses",
    "catch_clauses : catch_clause",
    "catch_clauses : catch_clauses catch_clause",
    "opt_identifier :",
    "opt_identifier : identifier",
    "$$28 :",
    "catch_clause : CATCH opt_catch_args end_of_stmt $$28 block",
    "opt_catch_args :",
    "opt_catch_args : catch_args",
    "catch_args : identifier AS type",
    "$$29 :",
    "while_statement : WHILE $$29 boolean_expression end_of_stmt begin_block opt_statement_list end_block END WHILE",
    "selection_statement : if_statement",
    "if_statement : IF boolean_expression opt_then end_of_stmt block END IF",
    "if_statement : IF boolean_expression opt_then end_of_stmt block ELSE end_of_stmt block END IF",
    "if_statement : IF boolean_expression opt_then end_of_stmt block else_if_statement_rest",
    "else_if_statement_rest : ELSEIF boolean_expression opt_then end_of_stmt block END IF",
    "else_if_statement_rest : ELSEIF boolean_expression opt_then end_of_stmt block ELSE end_of_stmt block END IF",
    "else_if_statement_rest : ELSEIF boolean_expression opt_then end_of_stmt block else_if_statement_rest",
    "opt_then :",
    "opt_then : THEN",
    "case_clauses : case_clause",
    "case_clauses : case_clauses COMMA case_clause",
    "case_clause : opt_is comparison_operator expression",
    "case_clause : expression",
    "opt_is :",
    "opt_is : IS",
    "comparison_operator : OP_LT",
    "comparison_operator : OP_GT",
    "comparison_operator : OP_LE",
    "comparison_operator : OP_NE",
    "opt_case :",
    "opt_case : CASE",
    "expression_statement : statement_expression",
    "statement_expression : invocation_expression",
    "statement_expression : object_creation_expression",
    "statement_expression : assignment_expression",
    "object_creation_expression : NEW type OPEN_PARENS opt_argument_list CLOSE_PARENS",
    "object_creation_expression : NEW type",
    "new_expression : object_creation_expression",
    "declaration_statement : local_variable_declaration",
    "declaration_statement : local_constant_declaration",
    "local_variable_declaration : DIM variable_declarators",
    "local_constant_declaration : CONST constant_declarators",
    "constant_declarators : constant_declarator",
    "constant_declarators : constant_declarators COMMA constant_declarator",
    "constant_declarator : variable_name opt_type_decl opt_variable_initializer",
    "variable_declarators : variable_declarator",
    "variable_declarators : variable_declarators COMMA variable_declarator",
    "variable_declarator : variable_names opt_type_decl opt_variable_initializer",
    "variable_names : variable_name",
    "variable_names : variable_names COMMA variable_name",
    "variable_name : identifier opt_type_character opt_array_name_modifier",
    "opt_type_spec :",
    "opt_type_spec : AS type",
    "opt_type_with_ranks : opt_type_spec",
    "opt_type_decl : opt_type_with_ranks",
    "opt_type_decl : AS NEW type",
    "opt_type_decl : AS NEW type OPEN_PARENS opt_argument_list CLOSE_PARENS",
    "opt_array_name_modifier :",
    "opt_variable_initializer :",
    "opt_variable_initializer : ASSIGN variable_initializer",
    "variable_initializer : expression",
    "variable_initializer : array_initializer",
    "array_initializer : OPEN_BRACE CLOSE_BRACE",
    "array_initializer : OPEN_BRACE variable_initializer_list CLOSE_BRACE",
    "variable_initializer_list : variable_initializer",
    "variable_initializer_list : variable_initializer_list COMMA variable_initializer",
    "opt_rank_specifiers :",
    "opt_rank_specifiers : rank_specifiers",
    "rank_specifiers : rank_specifier",
    "rank_specifiers : rank_specifiers rank_specifier",
    "rank_specifier : OPEN_PARENS opt_dim_specifiers CLOSE_PARENS",
    "opt_dim_specifiers :",
    "opt_dim_specifiers : expression",
    "opt_dim_specifiers : opt_dim_specifiers COMMA expression",
    "opt_dim_specifiers : opt_dim_specifiers COMMA",
    "primary_expression : literal",
    "primary_expression : parenthesized_expression",
    "primary_expression : this_access",
    "primary_expression : base_access",
    "primary_expression : qualified_identifier",
    "primary_expression : get_type_expression",
    "primary_expression : member_access",
    "primary_expression : invocation_expression",
    "primary_expression : new_expression",
    "primary_expression : cast_expression",
    "literal : boolean_literal",
    "literal : integer_literal",
    "literal : real_literal",
    "literal : LITERAL_CHARACTER",
    "literal : LITERAL_STRING",
    "literal : NOTHING",
    "real_literal : LITERAL_SINGLE",
    "real_literal : LITERAL_DOUBLE",
    "real_literal : LITERAL_DECIMAL",
    "integer_literal : LITERAL_INTEGER",
    "boolean_literal : TRUE",
    "boolean_literal : FALSE",
    "parenthesized_expression : OPEN_PARENS expression CLOSE_PARENS",
    "member_access : primary_expression DOT identifier",
    "member_access : predefined_type DOT identifier",
    "predefined_type : builtin_types",
    "invocation_expression : primary_expression OPEN_PARENS opt_argument_list CLOSE_PARENS",
    "invocation_expression : CALL primary_expression OPEN_PARENS opt_argument_list CLOSE_PARENS",
    "base_access : MYBASE DOT IDENTIFIER",
    "opt_argument_list : argument_list",
    "argument_list : argument",
    "argument_list : argument_list COMMA argument",
    "argument : expression",
    "argument : BYREF variable_reference",
    "argument :",
    "argument : ADDRESSOF expression",
    "variable_reference : expression",
    "expression : conditional_xor_expression",
    "opt_expression :",
    "opt_expression : expression",
    "this_access : ME",
    "this_access : MYCLASS",
    "cast_expression : DIRECTCAST OPEN_PARENS expression COMMA type CLOSE_PARENS",
    "cast_expression : CTYPE OPEN_PARENS expression COMMA type CLOSE_PARENS",
    "cast_expression : cast_operator OPEN_PARENS expression CLOSE_PARENS",
    "cast_operator : CBOOL",
    "cast_operator : CBYTE",
    "cast_operator : CCHAR",
    "cast_operator : CDBL",
    "cast_operator : CDEC",
    "cast_operator : CINT",
    "cast_operator : CLNG",
    "cast_operator : COBJ",
    "cast_operator : CSHORT",
    "cast_operator : CSNG",
    "cast_operator : CSTR",
    "get_type_expression : GETTYPE OPEN_PARENS type CLOSE_PARENS",
    "exponentiation_expression : primary_expression",
    "exponentiation_expression : exponentiation_expression OP_EXP primary_expression",
    "prefixed_unary_expression : exponentiation_expression",
    "prefixed_unary_expression : PLUS prefixed_unary_expression",
    "prefixed_unary_expression : MINUS prefixed_unary_expression",
    "multiplicative_expression : prefixed_unary_expression",
    "multiplicative_expression : multiplicative_expression STAR prefixed_unary_expression",
    "multiplicative_expression : multiplicative_expression DIV prefixed_unary_expression",
    "integer_division_expression : multiplicative_expression",
    "integer_division_expression : integer_division_expression OP_IDIV multiplicative_expression",
    "mod_expression : integer_division_expression",
    "mod_expression : mod_expression MOD integer_division_expression",
    "additive_expression : mod_expression",
    "additive_expression : additive_expression PLUS mod_expression",
    "additive_expression : additive_expression MINUS mod_expression",
    "concat_expression : additive_expression",
    "concat_expression : concat_expression OP_CONCAT additive_expression",
    "shift_expression : concat_expression",
    "shift_expression : shift_expression OP_SHIFT_LEFT concat_expression",
    "shift_expression : shift_expression OP_SHIFT_RIGHT concat_expression",
    "relational_expression : shift_expression",
    "relational_expression : relational_expression ASSIGN shift_expression",
    "relational_expression : relational_expression OP_NE shift_expression",
    "relational_expression : relational_expression OP_LT shift_expression",
    "relational_expression : relational_expression OP_GT shift_expression",
    "relational_expression : relational_expression OP_LE shift_expression",
    "relational_expression : relational_expression OP_GE shift_expression",
    "relational_expression : relational_expression IS shift_expression",
    "relational_expression : TYPEOF shift_expression IS type",
    "negation_expression : relational_expression",
    "negation_expression : NOT negation_expression",
    "conditional_and_expression : negation_expression",
    "conditional_and_expression : conditional_and_expression AND negation_expression",
    "conditional_and_expression : conditional_and_expression ANDALSO negation_expression",
    "conditional_or_expression : conditional_and_expression",
    "conditional_or_expression : conditional_or_expression OR conditional_and_expression",
    "conditional_or_expression : conditional_or_expression ORELSE conditional_and_expression",
    "conditional_xor_expression : conditional_or_expression",
    "conditional_xor_expression : conditional_xor_expression XOR conditional_or_expression",
    "assignment_expression : prefixed_unary_expression ASSIGN expression",
    "assignment_expression : prefixed_unary_expression STAR ASSIGN expression",
    "assignment_expression : prefixed_unary_expression DIV ASSIGN expression",
    "assignment_expression : prefixed_unary_expression PLUS ASSIGN expression",
    "assignment_expression : prefixed_unary_expression MINUS ASSIGN expression",
    "assignment_expression : prefixed_unary_expression OP_SHIFT_LEFT ASSIGN expression",
    "assignment_expression : prefixed_unary_expression OP_SHIFT_RIGHT ASSIGN expression",
    "assignment_expression : prefixed_unary_expression OP_CONCAT ASSIGN expression",
    "assignment_expression : prefixed_unary_expression OP_EXP ASSIGN expression",
    "assignment_expression : prefixed_unary_expression ASSIGN ADDRESSOF expression",
    "constant_expression : expression",
    "boolean_expression : expression",
    "type : namespace_or_type_name",
    "type : builtin_types",
    "type_list : type",
    "type_list : type_list COMMA type",
    "namespace_or_type_name : qualified_identifier",
    "builtin_types : OBJECT",
    "builtin_types : primitive_type",
    "primitive_type : numeric_type",
    "primitive_type : BOOLEAN",
    "primitive_type : CHAR",
    "primitive_type : STRING",
    "numeric_type : integral_type",
    "numeric_type : floating_point_type",
    "numeric_type : DECIMAL",
    "integral_type :",
    "integral_type : BYTE",
    "integral_type : SHORT",
    "integral_type : INTEGER",
    "integral_type : LONG",
    "floating_point_type : SINGLE",
    "floating_point_type : DOUBLE",
    "pp_directive : HASH IDENTIFIER OPEN_PARENS LITERAL_STRING COMMA LITERAL_INTEGER CLOSE_PARENS EOL",
    "pp_directive : HASH IDENTIFIER LITERAL_STRING EOL",
    "pp_directive : HASH END IDENTIFIER EOL",
    "pp_directive : HASH CONST IDENTIFIER ASSIGN boolean_literal EOL",
    "$$30 :",
    "pp_directive : HASH IF $$30 boolean_literal opt_then EOL",
    "$$31 :",
    "pp_directive : HASH ELSEIF $$31 boolean_literal opt_then EOL",
    "$$32 :",
    "pp_directive : HASH ELSE $$32 EOL",
    "$$33 :",
    "pp_directive : HASH END IF $$33 EOL",
    "pp_directive : HASH error EOL",
  };
  protected static  string [] yyNames = {    
    "end-of-file",null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,"'!'",null,"'#'","'$'","'%'","'&'",
    null,"'('","')'","'*'","'+'","','","'-'","'.'","'/'",null,null,null,
    null,null,null,null,null,null,null,"':'",null,"'<'","'='","'>'","'?'",
    "'@'",null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    "'['","'\\\\'","']'","'^'",null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,"'{'",null,"'}'",null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,"EOF","NONE","ERROR","ADDHANDLER","ADDRESSOF","ALIAS","AND",
    "ANDALSO","ANSI","AS","ASSEMBLY","AUTO","BINARY","BOOLEAN","BYREF",
    "BYTE","BYVAL","CALL","CASE","CATCH","CBOOL","CBYTE","CCHAR","CDATE",
    "CDEC","CDBL","CHAR","CINT","CLASS","CLNG","COBJ","COMPARE","CONST",
    "CSHORT","CSNG","CSTR","CTYPE","DATE","DECIMAL","DECLARE","DEFAULT",
    "DELEGATE","DIM","DIRECTCAST","DO","DOUBLE","EACH","ELSE","ELSEIF",
    "END","ENDIF","ENUM","EOL","ERASE","EVENT","EXIT","EXPLICIT","FALSE",
    "FINALLY","FOR","FRIEND","FUNCTION","GET","GETTYPE","GOSUB","GOTO",
    "HANDLES","IF","IMPLEMENTS","IMPORTS","IN","INHERITS","INTEGER",
    "INTERFACE","IS","LET","LIB","LIKE","LONG","LOOP","ME","MOD","MODULE",
    "MUSTINHERIT","MUSTOVERRIDE","MYBASE","MYCLASS","NAMESPACE","NEW",
    "NEXT","NOT","NOTHING","NOTINHERITABLE","NOTOVERRIDABLE","OBJECT",
    "OFF","ON","OPTION","OPTIONAL","OR","ORELSE","OVERLOADS",
    "OVERRIDABLE","OVERRIDES","PARAM_ARRAY","PRESERVE","PRIVATE",
    "PROPERTY","PROTECTED","PUBLIC","RAISEEVENT","READONLY","REDIM","REM",
    "REMOVEHANDLER","RESUME","RETURN","SELECT","SET","SHADOWS","SHARED",
    "SHORT","SINGLE","SIZEOF","STATIC","STEP","STOP","STRICT","STRING",
    "STRUCTURE","SUB","SYNCLOCK","TEXT","THEN","THROW","TO","TRUE","TRY",
    "TYPEOF","UNICODE","UNTIL","VARIANT","WEND","WHEN","WHILE","WITH",
    "WITHEVENTS","WRITEONLY","XOR","YIELD","HASH","OPEN_BRACKET",
    "CLOSE_BRACKET","OPEN_PARENS","OPEN_BRACE","CLOSE_BRACE",
    "CLOSE_PARENS","DOT","COMMA","COLON","PLUS","MINUS","ASSIGN","OP_LT",
    "OP_GT","STAR","DIV","OP_EXP","INTERR","OP_IDIV","OP_CONCAT",
    "EXCLAMATION","PERCENT","LONGTYPECHAR","AT_SIGN","SINGLETYPECHAR",
    "NUMBER_SIGN","DOLAR_SIGN","ATTR_ASSIGN","\":=\"","OP_LE","\"<=\"",
    "OP_GE","\">=\"","OP_NE","\"<>\"","OP_XOR","\"xor\"","OP_SHIFT_LEFT",
    "\"<<\"","OP_SHIFT_RIGHT","\">>\"","LITERAL_INTEGER",
    "\"int literal\"","LITERAL_SINGLE","\"float literal\"",
    "LITERAL_DOUBLE","\"double literal\"","LITERAL_DECIMAL",
    "\"decimal literal\"","LITERAL_CHARACTER","\"character literal\"",
    "LITERAL_STRING","\"string literal\"","LITERAL_DATE",
    "\"datetime literal\"","IDENTIFIER","LOWPREC","OP_OR","OP_AND",
    "BITWISE_OR","BITWISE_AND","BITWISE_NOT","CARRET","UMINUS","OP_INC",
    "OP_DEC","HIGHPREC","label_name",
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

  /** computes list of expected tokens on error by tracing the tables.
      @param state for which to compute the list.
      @return list of token names.
    */
  protected string[] yyExpecting (int state) {
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

    string [] result = new string[len];
    for (n = token = 0; n < len;  ++ token)
      if (ok[token]) result[n++] = yyNames[token];
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

  /** the generated parser.
      Maintains a state and a value stack, currently with fixed maximum size.
      @param yyLex scanner.
      @return result of the last reduction, if any.
      @throws yyException on irrecoverable parse error.
    */
  internal Object yyparse (yyParser.yyInput yyLex)
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
              yyerror(String.Format ("syntax error, got token `{0}'", yyname (yyToken)), yyExpecting(yyState));
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
case 5:
#line 753 "mb-parser.jay"
  {
		yyVal=yyVals[-1+yyTop];
	  }
  break;
case 6:
#line 761 "mb-parser.jay"
  {
		/* ????? */ ;
	  }
  break;
case 14:
#line 784 "mb-parser.jay"
  {
		  yyVal = (object)true;
	  }
  break;
case 15:
#line 788 "mb-parser.jay"
  {
		  yyVal = (object)true;
	  }
  break;
case 16:
#line 792 "mb-parser.jay"
  {
		  yyVal = (object)false;
	  }
  break;
case 17:
#line 799 "mb-parser.jay"
  {
		  yyVal = (object)true;
	  }
  break;
case 18:
#line 803 "mb-parser.jay"
  {
		  yyVal = (object)false;
	  }
  break;
case 19:
#line 810 "mb-parser.jay"
  {
/* 		if (!UseExtendedSyntax)*/
/* 			OptionExplicit = (bool)$3;*/
/* 		else*/
/* 			Report.Warning (*/
/* 				9999, lexer.Location, */
/* 				"In MonoBASIC extended syntax explicit declaration is always required. So OPTION EXPLICIT is deprecated");*/
	  }
  break;
case 20:
#line 823 "mb-parser.jay"
  {
/* 		if (!UseExtendedSyntax)*/
/* 			OptionStrict = (bool)$3;*/
/* 		else*/
/* 			Report.Warning (*/
/* 				9999, lexer.Location, */
/* 				"In MonoBASIC extended syntax strict assignability is always required. So OPTION STRICT is deprecated");*/
	  }
  break;
case 21:
#line 835 "mb-parser.jay"
  {
/* 		OptionCompareBinary = (bool)$3;*/
	  }
  break;
case 26:
#line 852 "mb-parser.jay"
  {
/* 	  	FIXME: Need to check declaration qualifiers for multi-file compilation*/
/* 	  	FIXME: Qualifiers cannot be applied to namespaces*/
	  	allow_global_attribs = false;
	  }
  break;
case 27:
#line 858 "mb-parser.jay"
  {
		current_namespace.DeclarationFound = true;
	  }
  break;
case 28:
#line 862 "mb-parser.jay"
  {
		  /* FIXME: Need to check declaration qualifiers for multi-file compilation*/
		  allow_global_attribs = false;
	  }
  break;
case 29:
#line 867 "mb-parser.jay"
  {
		string name = "";
		int mod_flags;

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

		if ((mod_flags & (Modifiers.PRIVATE|Modifiers.PROTECTED)) != 0){
			Report.Error (
				1527, lexer.Location, 
				"Namespace elements cant be explicitly " +
			        "declared private or protected in `" + name + "'");
		}
		current_namespace.DeclarationFound = true;
	  }
  break;
case 36:
#line 902 "mb-parser.jay"
  { yyVal = TypeManager.system_int32_expr; }
  break;
case 37:
#line 903 "mb-parser.jay"
  { yyVal = TypeManager.system_int64_expr; }
  break;
case 38:
#line 904 "mb-parser.jay"
  { yyVal = TypeManager.system_decimal_expr; }
  break;
case 39:
#line 905 "mb-parser.jay"
  { yyVal = TypeManager.system_single_expr; }
  break;
case 40:
#line 906 "mb-parser.jay"
  { yyVal = TypeManager.system_double_expr; }
  break;
case 41:
#line 907 "mb-parser.jay"
  { yyVal = TypeManager.system_string_expr; }
  break;
case 42:
#line 911 "mb-parser.jay"
  { yyVal = null; }
  break;
case 43:
#line 912 "mb-parser.jay"
  { yyVal = yyVals[0+yyTop]; }
  break;
case 44:
#line 918 "mb-parser.jay"
  {
		yyVal = new MemberName ((string) yyVals[0+yyTop]);
	}
  break;
case 45:
#line 922 "mb-parser.jay"
  {
		yyVal = new MemberName ((MemberName) yyVals[-2+yyTop], (string) yyVals[0+yyTop], null);
	  }
  break;
case 53:
#line 948 "mb-parser.jay"
  {
		string name = ((MemberName) yyVals[0+yyTop]).GetName ();
	  	current_namespace.Using (name, lexer.Location);
	}
  break;
case 54:
#line 953 "mb-parser.jay"
  {
		  current_namespace.UsingAlias ((string) yyVals[-2+yyTop], (MemberName) yyVals[0+yyTop], lexer.Location);
	}
  break;
case 55:
#line 960 "mb-parser.jay"
  { yyVal = Parameters.EmptyReadOnlyParameters; }
  break;
case 56:
#line 961 "mb-parser.jay"
  { yyVal = Parameters.EmptyReadOnlyParameters; }
  break;
case 57:
#line 962 "mb-parser.jay"
  { yyVal = yyVals[-1+yyTop]; }
  break;
case 58:
#line 967 "mb-parser.jay"
  { 
		  current_attributes = null;
	  }
  break;
case 59:
#line 971 "mb-parser.jay"
  { 
	  	yyVal = yyVals[0+yyTop]; 
		local_attrib_section_added = false;
		current_attributes = (Attributes) yyVals[0+yyTop];
	  }
  break;
case 60:
#line 980 "mb-parser.jay"
  { 
	  	yyVal = yyVals[0+yyTop];
		if (yyVals[0+yyTop] == null) {
			expecting_local_attribs = false;
			expecting_global_attribs = false;
			break;
		}
		
		if (expecting_local_attribs) {
			local_attrib_section_added = true;
			allow_global_attribs = false;

			yyVal = new Attributes ((ArrayList) yyVals[0+yyTop]);
		}	

		if (expecting_global_attribs) {
			yyVal = null;
			CodeGen.AddGlobalAttributes ((ArrayList) yyVals[0+yyTop]);
		}

		expecting_local_attribs = false;
		expecting_global_attribs = false;
	  }
  break;
case 61:
#line 1004 "mb-parser.jay"
  {
	   	yyVal = lexer.Location;
	   }
  break;
case 62:
#line 1008 "mb-parser.jay"
  {
	  	yyVal = yyVals[-2+yyTop];
	  	if (yyVals[0+yyTop] != null) {
			ArrayList attrs = (ArrayList) yyVals[0+yyTop];

			if (expecting_local_attribs) {
				if (local_attrib_section_added) {
					expecting_local_attribs = false;
					expecting_global_attribs = false;
					Report.Error (30205, (Location) yyVals[-1+yyTop], "Multiple attribute sections may not be used; Coalesce multiple attribute sections in to a single attribute section");
					break;
				}

				if (yyVals[-2+yyTop] == null)
					yyVal = new Attributes (attrs);
				else 
					((Attributes) yyVals[-2+yyTop]).AddAttributes (attrs);

				local_attrib_section_added = true;
				allow_global_attribs = false;
			}

			if (expecting_global_attribs) {
				yyVal = null;
				CodeGen.AddGlobalAttributes ((ArrayList) yyVals[0+yyTop]);
			}
  		}	

		expecting_local_attribs = false;
		expecting_global_attribs = false;
	  }
  break;
case 63:
#line 1043 "mb-parser.jay"
  {
 	  	yyVal = null;
		if (yyVals[-2+yyTop] != null) {
			if (expecting_global_attribs && !(bool) yyVals[0+yyTop]) {
				Report.Error (30205, lexer.Location, "End of statement expected");
				break;
			}
			
			if (expecting_local_attribs)  {
				if ((bool) yyVals[0+yyTop]) {
					Report.Error (32035, lexer.Location, "Use a line continuation after the attribute specifier to apply it to the following statement.");
					break;
				}
			}

			yyVal = yyVals[-2+yyTop];
		}
 	  }
  break;
case 64:
#line 1064 "mb-parser.jay"
  { yyVal = false; }
  break;
case 65:
#line 1065 "mb-parser.jay"
  { yyVal = true; }
  break;
case 66:
#line 1070 "mb-parser.jay"
  {
 	  	ArrayList attrs = null;
 	  	if (yyVals[0+yyTop] != null) {
	 		attrs = new ArrayList ();
 			attrs.Add (yyVals[0+yyTop]);
  		}
 		yyVal = attrs;
 	  }
  break;
case 67:
#line 1079 "mb-parser.jay"
  {
 	  	ArrayList attrs = null;
		
 	  	if (yyVals[0+yyTop] != null) {
	 		attrs = (yyVals[-2+yyTop] == null) ? new ArrayList () : (ArrayList) yyVals[-2+yyTop];
 			attrs.Add (yyVals[0+yyTop]);
  		}

 		yyVal = attrs;
 	  }
  break;
case 68:
#line 1093 "mb-parser.jay"
  {
 		yyVal = lexer.Location;
 	   }
  break;
case 69:
#line 1097 "mb-parser.jay"
  {
 	   	yyVal = null;
		
 	   	if (expecting_global_attribs)
			Report.Error (32015, (Location) yyVals[-1+yyTop], "Expecting Assembly or Module attribute specifiers");
		else {
			expecting_local_attribs = true;
			MemberName mname = (MemberName) yyVals[-2+yyTop];
			string name = mname.GetName ();

			yyVal = new Attribute (null, name, (ArrayList) yyVals[0+yyTop],
					    (Location) yyVals[-1+yyTop]);
		}
 	   }
  break;
case 70:
#line 1112 "mb-parser.jay"
  {
	    	yyVal = lexer.Location;
	    }
  break;
case 71:
#line 1117 "mb-parser.jay"
  {
		  yyVal = lexer.Location;
	   }
  break;
case 72:
#line 1121 "mb-parser.jay"
  {
	   	yyVal = null;

	   	string attribute_target = (string) yyVals[-5+yyTop];
	   	if (attribute_target != "assembly" && attribute_target != "module") {
			Report.Error (29999, lexer.Location, "`" + (string)yyVals[-5+yyTop] + "' is an invalid attribute modifier");
			break;
   		}
   		if (!allow_global_attribs) {
			Report.Error (30637, (Location) yyVals[-4+yyTop], "Global attribute statements must precede any declarations in a file");
			break;
		}

		if (expecting_local_attribs) {
			Report.Error (30183, (Location) yyVals[-4+yyTop], "Global attributes cannot be combined with local attributes");
			break;
		}			

		expecting_global_attribs = true;

		MemberName mname = (MemberName) yyVals[-2+yyTop];
		string aname = mname.GetName ();

		yyVal = new Attribute (attribute_target, aname, (ArrayList) yyVals[0+yyTop], (Location) yyVals[-1+yyTop]);
	    }
  break;
case 73:
#line 1150 "mb-parser.jay"
  { yyVal = "assembly"; }
  break;
case 74:
#line 1151 "mb-parser.jay"
  { yyVal = "module"; }
  break;
case 76:
#line 1157 "mb-parser.jay"
  { yyVal = null; }
  break;
case 77:
#line 1159 "mb-parser.jay"
  {
		yyVal = yyVals[-1+yyTop];
	  }
  break;
case 80:
#line 1171 "mb-parser.jay"
  {
		ArrayList args = new ArrayList (4);
		args.Add (yyVals[0+yyTop]);
	
		yyVal = args;
	  }
  break;
case 81:
#line 1178 "mb-parser.jay"
  {
		ArrayList args = new ArrayList (4);
		args.Add (yyVals[-2+yyTop]);
		args.Add (yyVals[0+yyTop]);

		yyVal = args;
	  }
  break;
case 82:
#line 1186 "mb-parser.jay"
  {
		ArrayList args = new ArrayList (4);
		args.Add (null);
		args.Add (yyVals[0+yyTop]);
		
		yyVal = args;
	  }
  break;
case 83:
#line 1197 "mb-parser.jay"
  {
		ArrayList args = new ArrayList ();
		args.Add (new Argument ((Expression) yyVals[0+yyTop], Argument.AType.Expression));

		yyVal = args;
	  }
  break;
case 84:
#line 1204 "mb-parser.jay"
  {
		ArrayList args = (ArrayList) yyVals[-2+yyTop];
		args.Add (new Argument ((Expression) yyVals[0+yyTop], Argument.AType.Expression));

		yyVal = args;
	 }
  break;
case 85:
#line 1214 "mb-parser.jay"
  {
		ArrayList args = new ArrayList ();
		args.Add (yyVals[0+yyTop]);

		yyVal = args;
	  }
  break;
case 86:
#line 1221 "mb-parser.jay"
  {	  
		ArrayList args = (ArrayList) yyVals[-2+yyTop];
		args.Add (yyVals[0+yyTop]);

		yyVal = args;
	  }
  break;
case 87:
#line 1231 "mb-parser.jay"
  {
		yyVal = new DictionaryEntry (
			(string) yyVals[-2+yyTop], 
			new Argument ((Expression) yyVals[0+yyTop], Argument.AType.Expression));
	  }
  break;
case 88:
#line 1240 "mb-parser.jay"
  {
		if (current_attributes != null) {
			Report.Error(1518, Lexer.Location, "Attributes cannot be applied to namespaces."
					+ " Expected class, delegate, enum, interface, or struct");
		}

		MemberName name = (MemberName) yyVals[-1+yyTop];

		if ((current_namespace.Parent != null) && (name.Left != null)) {
			Report.Error (134, lexer.Location,
				      "Cannot use qualified namespace names in nested " +
				      "namespace declarations");
		}

		current_namespace = new NamespaceEntry (
			current_namespace, file, name.GetName (), lexer.Location);
	  }
  break;
case 89:
#line 1259 "mb-parser.jay"
  { 
		current_namespace = current_namespace.Parent;
	  }
  break;
case 97:
#line 1280 "mb-parser.jay"
  {
		if (implicit_modifiers && ((current_modifiers & Modifiers.STATIC) != 0))
			current_modifiers = (current_modifiers & ~Modifiers.STATIC);

		MemberName name = MakeName (new MemberName ((string) yyVals[-1+yyTop]));
		int mod_flags = current_modifiers;


		current_class = new Class (current_namespace, current_container, name,
					   mod_flags, (Attributes) current_attributes, lexer.Location);


		current_container = current_class;
		RootContext.Tree.RecordDecl (name.GetName (true), current_class);
	  }
  break;
case 98:
#line 1297 "mb-parser.jay"
  {
		ArrayList bases = (ArrayList) yyVals[-1+yyTop];
		ArrayList ifaces = (ArrayList) yyVals[0+yyTop];

		if (ifaces != null){
			if (bases != null)	
				bases.AddRange(ifaces);
			else
				bases = ifaces;
		}

		if (bases != null) {
			if (current_class.Name == "System.Object") {
				Report.Error (537, current_class.Location,
					      "The class System.Object cannot have a base " +
					      "class or implement an interface.");
			}
			current_class.Bases = (ArrayList) bases;
		}

		current_class.Register ();
	  }
  break;
case 99:
#line 1321 "mb-parser.jay"
  {
		yyVal = current_class;

		current_container = current_container.Parent;
		current_class = current_container;
	  }
  break;
case 100:
#line 1330 "mb-parser.jay"
  { yyVal = null; }
  break;
case 101:
#line 1331 "mb-parser.jay"
  { yyVal = yyVals[-1+yyTop]; }
  break;
case 102:
#line 1335 "mb-parser.jay"
  { yyVal = null; }
  break;
case 103:
#line 1336 "mb-parser.jay"
  { yyVal = yyVals[-1+yyTop]; }
  break;
case 104:
#line 1341 "mb-parser.jay"
  { 
		yyVal = (int) 0; 
		current_modifiers = 0; 
	}
  break;
case 105:
#line 1346 "mb-parser.jay"
  { 
		yyVal = yyVals[0+yyTop]; 
		current_modifiers = (int) yyVals[0+yyTop]; 
	}
  break;
case 107:
#line 1355 "mb-parser.jay"
  { 
		int m1 = (int) yyVals[-1+yyTop];
		int m2 = (int) yyVals[0+yyTop];

		if ((m1 & m2) != 0) {
			Location l = lexer.Location;
			Report.Error (1004, l, "Duplicate modifier: `" + Modifiers.Name (m2) + "'");
		}
		yyVal = (int) (m1 | m2);
	  }
  break;
case 108:
#line 1368 "mb-parser.jay"
  { yyVal = Modifiers.PUBLIC; }
  break;
case 109:
#line 1369 "mb-parser.jay"
  { yyVal = Modifiers.PROTECTED; }
  break;
case 110:
#line 1370 "mb-parser.jay"
  { yyVal = Modifiers.PRIVATE; }
  break;
case 111:
#line 1371 "mb-parser.jay"
  { yyVal = Modifiers.STATIC; }
  break;
case 112:
#line 1372 "mb-parser.jay"
  { yyVal = Modifiers.INTERNAL; }
  break;
case 113:
#line 1373 "mb-parser.jay"
  { yyVal = Modifiers.SEALED; }
  break;
case 114:
#line 1374 "mb-parser.jay"
  { yyVal = Modifiers.VIRTUAL; }
  break;
case 115:
#line 1375 "mb-parser.jay"
  { yyVal = Modifiers.NONVIRTUAL; }
  break;
case 116:
#line 1376 "mb-parser.jay"
  { yyVal = Modifiers.OVERRIDE; }
  break;
case 117:
#line 1377 "mb-parser.jay"
  { yyVal = Modifiers.NEW; }
  break;
case 118:
#line 1378 "mb-parser.jay"
  { yyVal = Modifiers.SHADOWS; }
  break;
case 119:
#line 1379 "mb-parser.jay"
  { yyVal = Modifiers.ABSTRACT; }
  break;
case 120:
#line 1380 "mb-parser.jay"
  { yyVal = Modifiers.READONLY; }
  break;
case 121:
#line 1381 "mb-parser.jay"
  { yyVal = Modifiers.DEFAULT; }
  break;
case 122:
#line 1382 "mb-parser.jay"
  { yyVal = Modifiers.WRITEONLY; }
  break;
case 123:
#line 1387 "mb-parser.jay"
  { 
		MemberName name = MakeName(new MemberName ((string) yyVals[-1+yyTop]));
		current_class = new VBModule (current_namespace, current_container, name, 
					    current_modifiers, current_attributes, lexer.Location);

		current_container = current_class;
		RootContext.Tree.RecordDecl(name.GetName (true), current_class);

		current_class.Register ();
	  }
  break;
case 124:
#line 1399 "mb-parser.jay"
  {
		yyVal = current_class;
/* 	  	FIXME: ?????*/
/* 		TypeManager.AddStandardModule (current_class);*/

		current_container = current_container.Parent;
		current_class = current_container;
	  }
  break;
case 129:
#line 1422 "mb-parser.jay"
  { 
	   	current_modifiers = ((int)yyVals[0+yyTop]) | Modifiers.STATIC; 
		bool explicit_static = (((int) yyVals[0+yyTop] & Modifiers.STATIC) > 0);
		implicit_modifiers = (!explicit_static);
	   }
  break;
case 130:
#line 1429 "mb-parser.jay"
  {
	   	implicit_modifiers = false;
	   	yyVal = yyVals[-1+yyTop];
	   }
  break;
case 138:
#line 1449 "mb-parser.jay"
  {
		if (implicit_modifiers && ((current_modifiers & Modifiers.STATIC) != 0))
			current_modifiers = (current_modifiers & ~Modifiers.STATIC);
		
		int modflags = (int) current_modifiers;
		
		/* Structure members are Public by default			*/
		if ((current_container is Struct) && (modflags == 0))
			modflags = Modifiers.PUBLIC;			

		ArrayList consts = (ArrayList) yyVals[-1+yyTop];
		if(consts.Count > 0) 
		{
			VariableDeclaration.FixupTypes ((ArrayList) yyVals[-1+yyTop]);
			VariableDeclaration.FixupArrayTypes ((ArrayList) yyVals[-1+yyTop]);

			foreach (VariableDeclaration constant in (ArrayList) yyVals[-1+yyTop]){
				Location l = constant.Location;
				Const c = new Const (current_class, 
						     (Expression) constant.type, 
						     (String) constant.identifier, 
						     (Expression) constant.expression_or_array_initializer, 
						     modflags, current_attributes, l);

				current_container.AddConstant (c);
			}
		}
	}
  break;
case 143:
#line 1493 "mb-parser.jay"
  {
	   	yyVal = yyVals[0+yyTop];
	   }
  break;
case 153:
#line 1571 "mb-parser.jay"
  {
		MemberName name = new MemberName ((string) yyVals[-1+yyTop]);

		if ((current_container is Struct) && (current_modifiers == 0))
			current_modifiers = Modifiers.PUBLIC;		


		GenericMethod generic = null;
		Method method = new Method (current_class, generic, TypeManager.system_void_expr,
					    (int) current_modifiers, false, name, 
					    (Parameters) yyVals[0+yyTop], (Attributes) current_attributes, 
					    lexer.Location);

		current_local_parameters = (Parameters) yyVals[0+yyTop];
		yyVal = method;

		iterator_container = (IIteratorContainer) method;
	  }
  break;
case 154:
#line 1595 "mb-parser.jay"
  {
		Method method = (Method) yyVals[-9+yyTop];
		Block b = (Block) yyVals[-3+yyTop];
		const int extern_abstract = (Modifiers.EXTERN | Modifiers.ABSTRACT);

		if (b == null){
			if ((method.ModFlags & extern_abstract) == 0){
				Report.Error (
					501, lexer.Location,  current_container.MakeName (method.Name) +
				        "must declare a body because it is not marked abstract or extern");
			}
		} else {
			if ((method.ModFlags & Modifiers.EXTERN) != 0){
				Report.Error (
					179, lexer.Location, current_container.MakeName (method.Name) +
					" is declared extern, but has a body");
			}
		}

		method.Block = (ToplevelBlock) yyVals[-3+yyTop];
		current_container.AddMethod (method);

		current_local_parameters = null;
		iterator_container = null;
	  }
  break;
case 155:
#line 1625 "mb-parser.jay"
  {
		MemberName name =  new MemberName ((string) yyVals[-3+yyTop]);
		Expression rettype = (yyVals[0+yyTop] == null) ? ((yyVals[-2+yyTop] == null) ? TypeManager.system_object_expr : (Expression) yyVals[-2+yyTop] ) : (Expression) yyVals[0+yyTop];

		GenericMethod generic = null;

		Method method = new Method (current_class, generic, rettype, current_modifiers,
					    false, name,  (Parameters) yyVals[-1+yyTop], current_attributes,
					    lexer.Location);

		current_local_parameters = (Parameters) yyVals[-1+yyTop];

		yyVal = method;
		iterator_container = method;
	  }
  break;
case 156:
#line 1643 "mb-parser.jay"
  { 
		Method method = (Method) yyVals[-3+yyTop];

		ArrayList retval = new ArrayList ();
		retval.Add (new VariableDeclaration ((string) yyVals[-7+yyTop], method.Type, lexer.Location));
		declare_local_variables (method.Type, retval, lexer.Location);
	  }
  break;
case 157:
#line 1653 "mb-parser.jay"
  {
		Method method = (Method) yyVals[-9+yyTop];
		Block b = (Block) yyVals[-3+yyTop];
		const int extern_abstract = (Modifiers.EXTERN | Modifiers.ABSTRACT);

		if (b == null){
			if ((method.ModFlags & extern_abstract) == 0){
				Report.Error (
					501, lexer.Location,  current_container.MakeName (method.Name) +
				        "must declare a body because it is not marked abstract or extern");
			}
		} else {
			if ((method.ModFlags & Modifiers.EXTERN) != 0){
				Report.Error (
					179, lexer.Location, current_container.MakeName (method.Name) +
					" is declared extern, but has a body");
			}
		}

		method.Block = (ToplevelBlock) b;
		current_container.AddMethod (method);

		current_local_parameters = null;
		iterator_container = null;
	  }
  break;
case 158:
#line 1683 "mb-parser.jay"
  { 
		MemberName name = MakeName (new MemberName ((string) yyVals[-2+yyTop]));

		if (implicit_modifiers && ((current_modifiers & Modifiers.STATIC) != 0))
			current_modifiers = (current_modifiers & ~Modifiers.STATIC);

		current_class = new Struct (current_namespace, current_container, name, current_modifiers,
					    current_attributes, lexer.Location);

		current_container = current_class;
		RootContext.Tree.RecordDecl (name.GetName (true), current_class);

		if (yyVals[0+yyTop] != null)
			current_class.Bases = (ArrayList) yyVals[0+yyTop];

		current_class.Register ();
	  }
  break;
case 159:
#line 1701 "mb-parser.jay"
  {
		yyVal = current_class;

		current_container = current_container.Parent;
		current_class = current_container;
	  }
  break;
case 174:
#line 1806 "mb-parser.jay"
  {
	  	VariableDeclaration var = new VariableDeclaration ((string) yyVals[-4+yyTop], (Expression) yyVals[-2+yyTop], lexer.Location);

		MemberName name = new MemberName ((string) yyVals[-4+yyTop]);

		Event e = new EventField (current_class, (Expression) yyVals[-2+yyTop], current_modifiers, false, name,
					  var.expression_or_array_initializer, current_attributes,
					  lexer.Location);

		current_container.AddEvent (e);
	  }
  break;
case 175:
#line 1860 "mb-parser.jay"
  { 
		Location enum_location = lexer.Location;

		Expression base_type = TypeManager.system_int32_expr;
		if ((Expression) yyVals[-2+yyTop] != null)
			base_type = (Expression) yyVals[-2+yyTop];

		ArrayList enum_members = (ArrayList) yyVals[0+yyTop];
		if (enum_members.Count == 0)
			Report.Error (30280, enum_location,
				      "Enum can not have empty member list");


		if (implicit_modifiers && ((current_modifiers & Modifiers.STATIC) != 0))
			current_modifiers = (current_modifiers & ~Modifiers.STATIC);
			
		MemberName full_name = MakeName (new MemberName ((string) yyVals[-3+yyTop]));
		Enum e = new Enum (current_namespace, current_container, base_type, 
				   (int) current_modifiers, full_name, 
				   (Attributes) current_attributes, enum_location);
		
		foreach (VariableDeclaration ev in (ArrayList) yyVals[0+yyTop]) {
			e.AddEnumMember (ev.identifier, 
					 (Expression) ev.expression_or_array_initializer,
					 ev.Location, ev.OptAttributes, ev.DocComment);
		}

		string name = full_name.GetName ();
		current_container.AddEnum (e);
		RootContext.Tree.RecordDecl (name, e);

	  }
  break;
case 177:
#line 1896 "mb-parser.jay"
  { yyVal = new ArrayList (4); }
  break;
case 178:
#line 1897 "mb-parser.jay"
  { yyVal = yyVals[0+yyTop]; }
  break;
case 179:
#line 1902 "mb-parser.jay"
  {
		ArrayList l = new ArrayList ();

		l.Add (yyVals[0+yyTop]);
		yyVal = l;
	  }
  break;
case 180:
#line 1909 "mb-parser.jay"
  {
		ArrayList l = (ArrayList) yyVals[-1+yyTop];

		l.Add (yyVals[0+yyTop]);

		yyVal = l;
	  }
  break;
case 181:
#line 1920 "mb-parser.jay"
  {
		yyVal = new VariableDeclaration ((string) yyVals[-1+yyTop], null, lexer.Location, (Attributes) yyVals[-2+yyTop]);
	  }
  break;
case 182:
#line 1924 "mb-parser.jay"
  {
		  yyVal = lexer.Location;
	  }
  break;
case 183:
#line 1928 "mb-parser.jay"
  { 
		yyVal = new VariableDeclaration ((string) yyVals[-4+yyTop], yyVals[-1+yyTop], lexer.Location, (Attributes) yyVals[-5+yyTop]);
	  }
  break;
case 184:
#line 2019 "mb-parser.jay"
  {
		get_implicit_value_parameter_type  = 
			(yyVals[-1+yyTop] == null) ? ((yyVals[-3+yyTop] == null) ? 
				TypeManager.system_object_expr : (Expression) yyVals[-3+yyTop] ) : (Expression) yyVals[-1+yyTop];

		current_local_parameters = (Parameters) yyVals[-2+yyTop];
		if (current_local_parameters != Parameters.EmptyReadOnlyParameters) { 
			get_parameters = current_local_parameters.Copy (lexer.Location);
			set_parameters = current_local_parameters.Copy (lexer.Location);
			
			Parameter implicit_value_parameter = new Parameter (
					get_implicit_value_parameter_type, "Value", Parameter.Modifier.NONE, null);
			
			set_parameters.AppendParameter (implicit_value_parameter);
		}
		else
		{
			get_parameters = Parameters.EmptyReadOnlyParameters;
			set_parameters = new Parameters (null, null ,lexer.Location); 
			
			Parameter implicit_value_parameter = new Parameter (
					get_implicit_value_parameter_type, "Value", Parameter.Modifier.NONE, null);
			
			set_parameters.AppendParameter (implicit_value_parameter);
		}
		lexer.PropertyParsing = true;
		
		Location loc = lexer.Location;
		MemberName name = new MemberName ((string) yyVals[-4+yyTop]);

		Accessor get_block = new Accessor (null, 0, null, loc);	
		Accessor set_block = new Accessor (null, 0, null, loc);	

		Property prop = new Property (current_class, get_implicit_value_parameter_type, 
				     (int) current_modifiers, true,
				     name, current_attributes, 
				     get_parameters, get_block, 
				     set_parameters, set_block, lexer.Location);
		
		current_container.AddProperty (prop);
		
		get_implicit_value_parameter_type = null;
		set_implicit_value_parameter_type = null;
		get_parameters = null;
		set_parameters = null;
		current_local_parameters = null;			
	  }
  break;
case 185:
#line 2105 "mb-parser.jay"
  {
		MemberName name = new MemberName ((string) yyVals[-1+yyTop]);

		current_class = new Interface (current_namespace, current_container, 
					       name, (int) current_modifiers, 
					       (Attributes) current_attributes, lexer.Location);

		current_container = current_class;
		RootContext.Tree.RecordDecl (name.GetName (true), current_class);

	  }
  break;
case 186:
#line 2117 "mb-parser.jay"
  {
		current_class.Bases = (ArrayList) yyVals[0+yyTop];
		current_class.Register ();
	  }
  break;
case 187:
#line 2122 "mb-parser.jay"
  {
		yyVal = current_class;

		current_container = current_container.Parent;
		current_class = current_container;
	  }
  break;
case 189:
#line 2132 "mb-parser.jay"
  { yyVal = null; }
  break;
case 190:
#line 2133 "mb-parser.jay"
  { yyVal = yyVals[0+yyTop]; }
  break;
case 192:
#line 2139 "mb-parser.jay"
  {
		ArrayList bases = (ArrayList) yyVals[-1+yyTop];
		bases.AddRange ((ArrayList) yyVals[0+yyTop]);
		yyVal = bases;
	  }
  break;
case 193:
#line 2147 "mb-parser.jay"
  { yyVal = yyVals[-1+yyTop]; }
  break;
case 200:
#line 2170 "mb-parser.jay"
  { 
		Method m = (Method) yyVals[0+yyTop];

		current_container.AddMethod (m);
	  }
  break;
case 202:
#line 2181 "mb-parser.jay"
  {
		MemberName name = (MemberName) new MemberName ((string) yyVals[-2+yyTop]);

		GenericMethod generic = null;

		yyVal = new Method (current_class, generic, TypeManager.system_void_expr, 
				 (int) current_modifiers, true, name, (Parameters) yyVals[-1+yyTop],  
				 (Attributes) current_attributes, lexer.Location);
	  }
  break;
case 203:
#line 2192 "mb-parser.jay"
  {
		MemberName name = new MemberName ((string) yyVals[-4+yyTop]);
		Expression return_type = (yyVals[-1+yyTop] == null) ? 
			((yyVals[-3+yyTop] == null) ? TypeManager.system_object_expr : (Expression) yyVals[-3+yyTop] ) 
			: (Expression) yyVals[-1+yyTop];

		GenericMethod generic = null;
		yyVal = new Method (current_class, generic, return_type, (int) current_modifiers, 
				 true, name, (Parameters) yyVals[-2+yyTop], (Attributes) current_attributes,
				 lexer.Location);
	  }
  break;
case 205:
#line 2268 "mb-parser.jay"
  {
		get_implicit_value_parameter_type  = 
			(yyVals[-2+yyTop] == null) ? ((yyVals[-4+yyTop] == null) ? 
				TypeManager.system_object_expr : (Expression) yyVals[-4+yyTop] ) : (Expression) yyVals[-2+yyTop];
		get_implicit_value_parameter_name = (string) yyVals[-5+yyTop];
		
		current_local_parameters = (Parameters) yyVals[-3+yyTop];
		if (current_local_parameters != Parameters.EmptyReadOnlyParameters) { 
			get_parameters = current_local_parameters.Copy (lexer.Location);
			set_parameters = current_local_parameters.Copy (lexer.Location);
		}
		else
		{
			get_parameters = Parameters.EmptyReadOnlyParameters;
			set_parameters = new Parameters (null, null ,lexer.Location);		
		}
		lexer.PropertyParsing = true;

		yyVal = lexer.Location;
	  }
  break;
case 206:
#line 2290 "mb-parser.jay"
  {
		lexer.PropertyParsing = false;

		Property prop;
		Pair pair = (Pair) yyVals[-3+yyTop];
		
		Accessor get_block = (Accessor) pair.First; 
		Accessor set_block = (Accessor) pair.Second; 
		
		Location loc = lexer.Location;
		MemberName name = new MemberName ((string) yyVals[-10+yyTop]);

		/* FIXME: Implements Clause needs to be taken care of.*/

		if ((current_container is Struct) && (current_modifiers == 0))
			current_modifiers = Modifiers.PUBLIC;				


		prop = new Property (current_class, get_implicit_value_parameter_type, 
				     (int) current_modifiers, false,
				     name, (Attributes) current_attributes, 
				     get_parameters, get_block, 
				     set_parameters, set_block, lexer.Location);
		
		current_container.AddProperty (prop);
		get_implicit_value_parameter_type = null;
		set_implicit_value_parameter_type = null;
		get_parameters = null;
		set_parameters = null;
		current_local_parameters = null;
	  }
  break;
case 207:
#line 2325 "mb-parser.jay"
  {
	  	yyVal = Parameters.EmptyReadOnlyParameters;
	  }
  break;
case 208:
#line 2329 "mb-parser.jay"
  {
	  	yyVal = yyVals[-1+yyTop];
	  }
  break;
case 209:
#line 2336 "mb-parser.jay"
  {
	  	yyVal = null;
	  }
  break;
case 210:
#line 2340 "mb-parser.jay"
  {
		yyVal = yyVals[0+yyTop];
	  }
  break;
case 211:
#line 2347 "mb-parser.jay"
  {
		  MemberName mname = (MemberName) yyVals[0+yyTop];
		  ArrayList impl_list = new ArrayList ();
		  impl_list.Add (mname.GetTypeExpression (lexer.Location));
		  yyVal = impl_list;
	  }
  break;
case 212:
#line 2354 "mb-parser.jay"
  {
		  MemberName mname = (MemberName) yyVals[0+yyTop];
		  ArrayList impl_list = (ArrayList) yyVals[-2+yyTop];
		  impl_list.Add (mname.GetTypeExpression (lexer.Location));
		  yyVal = impl_list;
	  }
  break;
case 213:
#line 2364 "mb-parser.jay"
  { 
		yyVal = new Pair (yyVals[-1+yyTop], yyVals[0+yyTop]);
	  }
  break;
case 214:
#line 2368 "mb-parser.jay"
  {
		yyVal = new Pair (yyVals[0+yyTop], yyVals[-1+yyTop]);
	  }
  break;
case 215:
#line 2374 "mb-parser.jay"
  { yyVal = null; }
  break;
case 217:
#line 2379 "mb-parser.jay"
  { yyVal = null; }
  break;
case 219:
#line 2385 "mb-parser.jay"
  {
		if ((current_modifiers & Modifiers.WRITEONLY) != 0)
	  		Report.Error (30023, "'WriteOnly' properties cannot have a 'Get' accessor");
	  
		current_local_parameters = get_parameters;
		
		lexer.PropertyParsing = false;
		
	  }
  break;
case 220:
#line 2395 "mb-parser.jay"
  {
		ArrayList retval = new ArrayList ();
		retval.Add (new VariableDeclaration (get_implicit_value_parameter_name, get_implicit_value_parameter_type, lexer.Location));
		declare_local_variables (get_implicit_value_parameter_type, retval, lexer.Location);	
	  }
  break;
case 221:
#line 2403 "mb-parser.jay"
  {
		yyVal = new Accessor ((ToplevelBlock) yyVals[-3+yyTop], (int) current_modifiers, 
				   (Attributes) yyVals[-10+yyTop], lexer.Location);

		current_local_parameters = null;
		lexer.PropertyParsing = true;
	  }
  break;
case 222:
#line 2416 "mb-parser.jay"
  {
        if ((current_modifiers & Modifiers.READONLY) != 0)
	  		Report.Error (30022, "'ReadOnly' properties cannot have a 'Set' accessor");
	  		
		Parameter implicit_value_parameter = new Parameter (
			set_implicit_value_parameter_type, 
			set_implicit_value_parameter_name, 
			Parameter.Modifier.NONE, null);

		set_parameters.AppendParameter (implicit_value_parameter);
		current_local_parameters = set_parameters;

		lexer.PropertyParsing = false;
	  }
  break;
case 223:
#line 2434 "mb-parser.jay"
  {
		yyVal = new Accessor ((ToplevelBlock) yyVals[-3+yyTop], (int) current_modifiers, 
				   (Attributes) yyVals[-10+yyTop], lexer.Location);
		current_local_parameters = null;
		lexer.PropertyParsing = true;
	  }
  break;
case 224:
#line 2444 "mb-parser.jay"
  {
		set_implicit_value_parameter_type = (Expression) get_implicit_value_parameter_type; /* TypeManager.system_object_expr;*/
		set_implicit_value_parameter_name = "Value";
	}
  break;
case 225:
#line 2449 "mb-parser.jay"
  {
		set_implicit_value_parameter_type = (Expression) get_implicit_value_parameter_type;
		set_implicit_value_parameter_name = "Value";
	}
  break;
case 226:
#line 2454 "mb-parser.jay"
  {
		Parameter.Modifier pm = (Parameter.Modifier)yyVals[-3+yyTop];
		if ((pm | Parameter.Modifier.VAL) != 0)
			Report.Error (31065, 
				lexer.Location, 
				"Set cannot have a paremeter modifier other than 'ByVal'");
				
		set_implicit_value_parameter_type = (Expression) yyVals[-1+yyTop];
		
		if (set_implicit_value_parameter_type.ToString () != get_implicit_value_parameter_type.ToString ())
			Report.Error (31064, 
				lexer.Location, 
				"Set value parameter type can not be different from property type");
				
		if (yyVals[-3+yyTop] != null)
			set_implicit_value_parameter_name = (string) yyVals[-2+yyTop];
		else
			set_implicit_value_parameter_name = "Value";
	}
  break;
case 227:
#line 2478 "mb-parser.jay"
  {   		  
		int mod = (int) current_modifiers;

		VariableDeclaration.FixupTypes ((ArrayList) yyVals[-1+yyTop]);
		VariableDeclaration.FixupArrayTypes ((ArrayList) yyVals[-1+yyTop]);
		
/* 		if (current_container is Module)*/
/* 			mod = mod | Modifiers.STATIC;*/
			
		/* Structure members are Public by default			*/
		if ((current_container is Struct) && (mod == 0))
			mod = Modifiers.PUBLIC;			
		
		if ((mod & Modifiers.Accessibility) == 0)
			mod |= Modifiers.PRIVATE;
					
		foreach (VariableDeclaration var in (ArrayList) yyVals[-1+yyTop]){
			Location l = var.Location;
			Field field = new Field (current_class, var.type, mod, 
						 var.identifier, var.expression_or_array_initializer, 
						 (Attributes) null, l);

			current_container.AddField (field);
		}
	  }
  break;
case 230:
#line 2555 "mb-parser.jay"
  {
		Location l = lexer.Location;
		MemberName name = MakeName (new MemberName ((string) yyVals[-4+yyTop]));

		if (implicit_modifiers && ((current_modifiers & Modifiers.STATIC) != 0))
			current_modifiers = (current_modifiers & ~Modifiers.STATIC);

		Delegate del = new Delegate (current_namespace, current_container, TypeManager.system_void_expr,
					     current_modifiers, name, (Parameters) yyVals[-2+yyTop], current_attributes, l);

		current_container.AddDelegate (del);
		RootContext.Tree.RecordDecl (name.GetName (true), del);
	  }
  break;
case 231:
#line 2573 "mb-parser.jay"
  {
		Location l = lexer.Location;
		MemberName name = MakeName (new MemberName ((string) yyVals[-5+yyTop]));

		if (implicit_modifiers && ((current_modifiers & Modifiers.STATIC) != 0))
			current_modifiers = (current_modifiers & ~Modifiers.STATIC);

		Expression rettype = (yyVals[-1+yyTop] == null) ? TypeManager.system_object_expr : (Expression) yyVals[-1+yyTop];
		Delegate del = new Delegate (current_namespace, current_container, rettype,
					     current_modifiers, name, (Parameters) yyVals[-3+yyTop], current_attributes, l);

		current_container.AddDelegate (del);
		RootContext.Tree.RecordDecl (name.GetName (true), del);
	  }
  break;
case 232:
#line 2592 "mb-parser.jay"
  { 	yyVal = null; }
  break;
case 233:
#line 2619 "mb-parser.jay"
  {
	  	current_local_parameters = (Parameters) yyVals[-1+yyTop];
		yyVal = new Constructor (current_class, current_container.Basename, 0, (Parameters) yyVals[-1+yyTop], 
				      (ConstructorInitializer) null, lexer.Location);
	  }
  break;
case 234:
#line 2627 "mb-parser.jay"
  { 
		Constructor c = (Constructor) yyVals[-3+yyTop];
		c.Block = (ToplevelBlock) yyVals[0+yyTop];
		c.ModFlags = (int) current_modifiers;
		c.OptAttributes = current_attributes;

		/* FIXME: Some more error checking from mcs needs to be merged here ???*/
		
		c.Initializer = CheckConstructorInitializer (ref c.Block.statements);

		current_container.AddConstructor(c);
		current_local_parameters = null;
	  }
  break;
case 236:
#line 2645 "mb-parser.jay"
  { 
		yyVal = Parameters.EmptyReadOnlyParameters; 
	  }
  break;
case 237:
#line 2649 "mb-parser.jay"
  { 
	  	yyVal = yyVals[0+yyTop];	
	  }
  break;
case 238:
#line 2656 "mb-parser.jay"
  { 
		ArrayList pars_list = (ArrayList) yyVals[0+yyTop];
		Parameter [] pars = null; 
		Parameter array_parameter = null;
		int non_array_count = pars_list.Count;
		if (pars_list.Count > 0 && (((Parameter) pars_list [pars_list.Count - 1]).ModFlags & Parameter.Modifier.PARAMS) != 0) {
			array_parameter = (Parameter) pars_list [pars_list.Count - 1];
			non_array_count = pars_list.Count - 1;
		}
		foreach (Parameter par in pars_list)
			if (par != array_parameter && (par.ModFlags & Parameter.Modifier.PARAMS) != 0) {
		  		Report.Error (30192, lexer.Location, "ParamArray parameters must be last");
			  	non_array_count = 0; 
				array_parameter = null;
				break;
			}
		if (non_array_count > 0) {
			pars = new Parameter [non_array_count];
			pars_list.CopyTo (0, pars, 0, non_array_count);
		}
	  	yyVal = new Parameters (pars, array_parameter, lexer.Location); 
	  }
  break;
case 239:
#line 2682 "mb-parser.jay"
  {
		ArrayList pars = new ArrayList ();

		pars.Add (yyVals[0+yyTop]);
		yyVal = pars;
	  }
  break;
case 240:
#line 2689 "mb-parser.jay"
  {
		ArrayList pars = (ArrayList) yyVals[-2+yyTop];

		pars.Add (yyVals[0+yyTop]);
		yyVal = yyVals[-2+yyTop];
	  }
  break;
case 241:
#line 2701 "mb-parser.jay"
  {
	  	Parameter.Modifier pm = (Parameter.Modifier)yyVals[-5+yyTop];
	  	bool opt_parm = ((pm & Parameter.Modifier.OPTIONAL) != 0);
	  	Expression ptype;
	  	
	  	if (opt_parm && (yyVals[0+yyTop] == null))
	  		Report.Error (30812, lexer.Location, "Optional parameters must have a default value");

	  	if (!opt_parm && (yyVals[0+yyTop] != null))
	  		Report.Error (32024, lexer.Location, "Non-Optional parameters should not have a default value");

	  	if ((pm & Parameter.Modifier.PARAMS) != 0) {
		  	if ((pm & ~Parameter.Modifier.PARAMS) != 0)
	  			Report.Error (30667, lexer.Location, "ParamArray parameters must be ByVal");
		}
	  	
  		if ((pm & Parameter.Modifier.REF) !=0)
  			pm |= Parameter.Modifier.ISBYREF;
		
	  	if (yyVals[-3+yyTop] != null && yyVals[-1+yyTop] != null && yyVals[-3+yyTop] != yyVals[-1+yyTop])
			Report.Error (30302, lexer.Location, "Type character conflicts with declared type."); /* TODO: Correct error number and message text*/

		ptype = (Expression)((yyVals[-1+yyTop] == null) ? ((yyVals[-3+yyTop] == null) ? TypeManager.system_object_expr : yyVals[-3+yyTop]) : yyVals[-1+yyTop]);
		if (yyVals[-2+yyTop] != null)	{
	  		string t = ptype.ToString ();
	  		if (t.IndexOf('[') >= 0)
				Report.Error (31087, lexer.Location, "Array types specified in too many places");
			else	
	  			ptype = DecomposeQI (t + VariableDeclaration.BuildRanks ((ArrayList) yyVals[-2+yyTop], true, lexer.Location), lexer.Location);
	  	}
		if ((pm & Parameter.Modifier.PARAMS) != 0 && ptype.ToString ().IndexOf('[') < 0)
	  		Report.Error (30050, lexer.Location, "ParamArray parameters must be an array type");
		yyVal = new Parameter (ptype, (string) yyVals[-4+yyTop], pm,
					(Attributes) yyVals[-6+yyTop], (Expression) yyVals[0+yyTop], opt_parm);
	  }
  break;
case 242:
#line 2739 "mb-parser.jay"
  { yyVal = Parameter.Modifier.VAL; 	}
  break;
case 243:
#line 2740 "mb-parser.jay"
  { yyVal = yyVals[0+yyTop];			}
  break;
case 244:
#line 2744 "mb-parser.jay"
  { yyVal = (Parameter.Modifier)yyVals[-1+yyTop] | (Parameter.Modifier)yyVals[0+yyTop];	}
  break;
case 245:
#line 2745 "mb-parser.jay"
  { yyVal = yyVals[0+yyTop];	}
  break;
case 246:
#line 2749 "mb-parser.jay"
  { yyVal = Parameter.Modifier.REF | Parameter.Modifier.ISBYREF; }
  break;
case 247:
#line 2750 "mb-parser.jay"
  { yyVal = Parameter.Modifier.VAL; }
  break;
case 248:
#line 2751 "mb-parser.jay"
  { yyVal = Parameter.Modifier.OPTIONAL; }
  break;
case 249:
#line 2752 "mb-parser.jay"
  { yyVal = Parameter.Modifier.PARAMS; }
  break;
case 254:
#line 2768 "mb-parser.jay"
  { yyVal = yyVals[0+yyTop]; }
  break;
case 255:
#line 2773 "mb-parser.jay"
  {
		  if (current_block == null){
			  current_block = new ToplevelBlock ((ToplevelBlock) top_current_block, current_local_parameters, lexer.Location);
			  top_current_block = current_block;
		  } else {
		  current_block = new Block (current_block, current_local_parameters,
						 lexer.Location, Location.Null);
		  }
	}
  break;
case 256:
#line 2787 "mb-parser.jay"
  { 
		while (current_block.Implicit)
			current_block = current_block.Parent;
		yyVal = current_block;
		current_block.SetEndLocation (lexer.Location);
		current_block = current_block.Parent;
		if (current_block == null)
			top_current_block = null;
	  }
  break;
case 257:
#line 2800 "mb-parser.jay"
  {
		  if (yyVals[0+yyTop] != null && (Block) yyVals[0+yyTop] != current_block){
			current_block.AddStatement ((Statement) yyVals[0+yyTop]);
			current_block = (Block) yyVals[0+yyTop];
		  }
	    }
  break;
case 258:
#line 2807 "mb-parser.jay"
  {
		  Statement s = (Statement) yyVals[0+yyTop];

		  current_block.AddStatement ((Statement) yyVals[0+yyTop]);
	    }
  break;
case 259:
#line 2814 "mb-parser.jay"
  {
		Location loc = lexer.Location;

		ExpressionStatement expr = new CompoundAssign (Binary.Operator.Addition, 
					 (Expression) yyVals[-3+yyTop], (Expression) yyVals[0+yyTop], loc);

		Statement stmt = new StatementExpression (expr, loc); 

		current_block.AddStatement (stmt);

	    }
  break;
case 260:
#line 2827 "mb-parser.jay"
  {
		Location loc = lexer.Location;

		ExpressionStatement expr = new CompoundAssign (Binary.Operator.Subtraction, 
					 (Expression) yyVals[-3+yyTop], (Expression) yyVals[0+yyTop], loc);

		Statement stmt = new StatementExpression (expr, loc); 

		current_block.AddStatement (stmt);

	    }
  break;
case 261:
#line 2839 "mb-parser.jay"
  {
	      Location loc = lexer.Location;
	      MemberName mname = new MemberName ((string) yyVals[-1+yyTop]);
	      Expression expr = mname.GetTypeExpression (loc);

	      Invocation inv_expr = new Invocation (expr, (ArrayList) yyVals[0+yyTop], loc);
	      Statement stmt = new StatementExpression (inv_expr, loc); 
	      current_block.AddStatement (stmt);	
	    }
  break;
case 262:
#line 2860 "mb-parser.jay"
  { yyVal = null; }
  break;
case 263:
#line 2862 "mb-parser.jay"
  {
		yyVal = yyVals[-1+yyTop];
	  }
  break;
case 272:
#line 3027 "mb-parser.jay"
  {
		yyVal = new Goto (current_block, (string) yyVals[0+yyTop], lexer.Location);
	  }
  break;
case 273:
#line 3034 "mb-parser.jay"
  {
		yyVal = new Throw ((Expression) yyVals[0+yyTop], lexer.Location);
	  }
  break;
case 274:
#line 3059 "mb-parser.jay"
  {	  
		yyVal = new Return ((Expression) yyVals[0+yyTop], lexer.Location);
	  }
  break;
case 275:
#line 3185 "mb-parser.jay"
  {
		yyVal = new Lock ((Expression) yyVals[-4+yyTop], (Statement) yyVals[-2+yyTop], lexer.Location);
	  }
  break;
case 278:
#line 3198 "mb-parser.jay"
  {
		Catch g = null;
		
		ArrayList c = (ArrayList)yyVals[-2+yyTop];
		for (int i = 0; i < c.Count; ++i) {
			Catch cc = (Catch) c [i];
			if (cc.IsGeneral) {
				if (i != c.Count - 1)
					Report.Error (1017, cc.loc, "Empty catch block must be the last in a series of catch blocks");
				g = cc;
				c.RemoveAt (i);
				i--;
			}
		}

		/* Now s contains the list of specific catch clauses*/
		/* and g contains the general one.*/
		
		yyVal = new Try ((Block) yyVals[-3+yyTop], c, g, null, ((Block) yyVals[-3+yyTop]).loc);
	  }
  break;
case 279:
#line 3227 "mb-parser.jay"
  {
		Catch g = null;
		ArrayList s = new ArrayList (4);
		ArrayList catch_list = (ArrayList) yyVals[-5+yyTop];

		if (catch_list != null){
			foreach (Catch cc in catch_list) {
				if (cc.IsGeneral)
					g = cc;
				else
					s.Add (cc);
			}
		}

		yyVal = new Try ((Block) yyVals[-6+yyTop], s, g, (Block) yyVals[-2+yyTop], ((Block) yyVals[-6+yyTop]).loc);
	  }
  break;
case 280:
#line 3246 "mb-parser.jay"
  {  yyVal = null;  }
  break;
case 282:
#line 3252 "mb-parser.jay"
  {
		ArrayList l = new ArrayList (4);

		l.Add (yyVals[0+yyTop]);
		yyVal = l;
	  }
  break;
case 283:
#line 3259 "mb-parser.jay"
  {
		ArrayList l = (ArrayList) yyVals[-1+yyTop];

		l.Add (yyVals[0+yyTop]);
		yyVal = l;
	  }
  break;
case 284:
#line 3268 "mb-parser.jay"
  {  yyVal = null;  }
  break;
case 286:
#line 3281 "mb-parser.jay"
  {
		/* FIXME: opt_when needs to be hnadled*/
		Expression type = null;
		string id = null;
		
		if (yyVals[-1+yyTop] != null) {
			DictionaryEntry cc = (DictionaryEntry) yyVals[-1+yyTop];
			type = (Expression) cc.Key;
			id   = (string) cc.Value;

			if (id != null){
				ArrayList one = new ArrayList (4);
				Location loc = lexer.Location;

				one.Add (new VariableDeclaration (id, type, loc));

				yyVals[-2+yyTop] = current_block;
				current_block = new Block (current_block);
				Block b = declare_local_variables (type, one, loc);
				current_block = b;
			}
		}
	}
  break;
case 287:
#line 3305 "mb-parser.jay"
  {
		Expression type = null;
		string id = null;

		if (yyVals[-3+yyTop] != null){
			DictionaryEntry cc = (DictionaryEntry) yyVals[-3+yyTop];
			type = (Expression) cc.Key;
			id   = (string) cc.Value;

			if (yyVals[-4+yyTop] != null){
				/**/
				/* FIXME: I can change this for an assignment.*/
				/**/
				while (current_block != (Block) yyVals[-4+yyTop])
					current_block = current_block.Parent;
			}
		}


		yyVal = new Catch (type, id , (Block) yyVals[0+yyTop], ((Block) yyVals[0+yyTop]).loc);
	}
  break;
case 288:
#line 3329 "mb-parser.jay"
  {  yyVal = null; }
  break;
case 290:
#line 3335 "mb-parser.jay"
  {
		 yyVal = new DictionaryEntry (yyVals[0+yyTop], yyVals[-2+yyTop]); 
	}
  break;
case 291:
#line 3388 "mb-parser.jay"
  {
		oob_stack.Push (lexer.Location);
	}
  break;
case 292:
#line 3396 "mb-parser.jay"
  {
		Location l = (Location) oob_stack.Pop ();
		yyVal = new While ((Expression) yyVals[-6+yyTop], (Statement) yyVals[-2+yyTop], l);
	}
  break;
case 294:
#line 3471 "mb-parser.jay"
  { 
		oob_stack.Push (lexer.Location);	  
		Location l = (Location) oob_stack.Pop ();

		yyVal = new If ((Expression) yyVals[-5+yyTop], (Statement) yyVals[-2+yyTop], l);

		if (RootContext.WarningLevel >= 3){
			if (yyVals[-2+yyTop] == EmptyStatement.Value)
				Report.Warning (642, lexer.Location, "Possible mistaken empty statement");
		}

	  }
  break;
case 295:
#line 3489 "mb-parser.jay"
  {
		oob_stack.Push (lexer.Location);	  
		Location l = (Location) oob_stack.Pop ();

		yyVal = new If ((Expression) yyVals[-8+yyTop], (Statement) yyVals[-5+yyTop], (Statement) yyVals[-2+yyTop], l);
	  }
  break;
case 296:
#line 3499 "mb-parser.jay"
  {
		oob_stack.Push (lexer.Location);	  
		Location l = (Location) oob_stack.Pop ();

		yyVal = new If ((Expression) yyVals[-4+yyTop], (Statement) yyVals[-1+yyTop], (Statement) yyVals[0+yyTop], l);
	  }
  break;
case 297:
#line 3511 "mb-parser.jay"
  { 
		oob_stack.Push (lexer.Location);	  
		Location l = (Location) oob_stack.Pop ();

		yyVal = new If ((Expression) yyVals[-5+yyTop], (Statement) yyVals[-2+yyTop], l);

		if (RootContext.WarningLevel >= 3){
			if (yyVals[-2+yyTop] == EmptyStatement.Value)
				Report.Warning (642, lexer.Location, "Possible mistaken empty statement");
		}

	  }
  break;
case 298:
#line 3529 "mb-parser.jay"
  {
		oob_stack.Push (lexer.Location);	  
		Location l = (Location) oob_stack.Pop ();

		yyVal = new If ((Expression) yyVals[-8+yyTop], (Statement) yyVals[-5+yyTop], (Statement) yyVals[-2+yyTop], l);
	  }
  break;
case 299:
#line 3539 "mb-parser.jay"
  {
		oob_stack.Push (lexer.Location);	  
		Location l = (Location) oob_stack.Pop ();

		yyVal = new If ((Expression) yyVals[-4+yyTop], (Statement) yyVals[-1+yyTop], (Statement) yyVals[0+yyTop], l);
	  }
  break;
case 302:
#line 3667 "mb-parser.jay"
  {
		ArrayList labels = new ArrayList ();

		labels.Add (yyVals[0+yyTop]);
		yyVal = labels;
	  }
  break;
case 303:
#line 3674 "mb-parser.jay"
  {
		ArrayList labels = (ArrayList) (yyVals[-2+yyTop]);
		labels.Add (yyVals[-1+yyTop]);

		yyVal = labels;
	  }
  break;
case 305:
#line 3685 "mb-parser.jay"
  {
	  	yyVal = new SwitchLabel ((Expression) yyVals[0+yyTop], lexer.Location);
	  }
  break;
case 314:
#line 3710 "mb-parser.jay"
  {
		 yyVal = yyVals[0+yyTop]; 
	  }
  break;
case 315:
#line 3717 "mb-parser.jay"
  { yyVal = new StatementExpression ((ExpressionStatement) yyVals[0+yyTop], lexer.Location);  }
  break;
case 316:
#line 3718 "mb-parser.jay"
  { yyVal = new StatementExpression ((ExpressionStatement) yyVals[0+yyTop], lexer.Location);  }
  break;
case 317:
#line 3719 "mb-parser.jay"
  { yyVal = new StatementExpression ((ExpressionStatement) yyVals[0+yyTop], lexer.Location);  }
  break;
case 318:
#line 3724 "mb-parser.jay"
  {
		yyVal = new New ((Expression) yyVals[-3+yyTop], (ArrayList) yyVals[-1+yyTop], lexer.Location);
	  }
  break;
case 319:
#line 3728 "mb-parser.jay"
  {
		yyVal = new New ((Expression) yyVals[0+yyTop], new ArrayList(), lexer.Location);
	  }
  break;
case 321:
#line 3775 "mb-parser.jay"
  {
		if (yyVals[0+yyTop] != null){
			DictionaryEntry de = (DictionaryEntry) yyVals[0+yyTop];

			yyVal = declare_local_variables ((Expression) de.Key, (ArrayList) de.Value, lexer.Location);
		}
	  }
  break;
case 322:
#line 3783 "mb-parser.jay"
  {
		if (yyVals[0+yyTop] != null){
			DictionaryEntry de = (DictionaryEntry) yyVals[0+yyTop];

			yyVal = declare_local_constant ((Expression) de.Key, (ArrayList) de.Value);
		}
	  }
  break;
case 323:
#line 3794 "mb-parser.jay"
  {
		yyVal = new DictionaryEntry (DecomposeQI("_local_vars_", lexer.Location), yyVals[0+yyTop]);		
	  }
  break;
case 324:
#line 3802 "mb-parser.jay"
  {
		if (yyVals[0+yyTop] != null)
			yyVal = new DictionaryEntry (DecomposeQI("_local_consts_", lexer.Location), yyVals[0+yyTop]);
		else
			yyVal = null;
	  }
  break;
case 325:
#line 3812 "mb-parser.jay"
  {
		ArrayList decl = new ArrayList ();
		if (yyVals[0+yyTop] != null) 
			decl.Add (yyVals[0+yyTop]);
			
		yyVal = decl;
	  }
  break;
case 326:
#line 3820 "mb-parser.jay"
  {
	  	ArrayList decls = (ArrayList) yyVals[-2+yyTop];
		if (yyVals[0+yyTop] != null)
			decls.Add (yyVals[0+yyTop]);

		yyVal = yyVals[-2+yyTop];
	  }
  break;
case 327:
#line 3831 "mb-parser.jay"
  {
		VarName vname = (VarName) yyVals[-2+yyTop];
		string varname = (string) vname.Name;
		current_rank_specifiers = (ArrayList) vname.Rank;
		object varinit = yyVals[0+yyTop];
		ArrayList a_dims = null;

		if (varinit == null)
			Report.Error (
				30438, lexer.Location, "Constant should have a value"
				);

		if (vname.Type != null && yyVals[-1+yyTop] != null)
			Report.Error (
				30302, lexer.Location, 
				"Type character cannot be used with explicit type declaration" );

		Expression vartype = (yyVals[-1+yyTop] == null) ? ((vname.Type == null) ? TypeManager.system_object_expr : (Expression) vname.Type ) : (Expression) yyVals[-1+yyTop];

		if (current_rank_specifiers != null) 
		{
			Report.Error (30424, lexer.Location, "Constant doesn't support array");
			yyVal = null;
	  	}
	  	else
	  	  	yyVal = new VariableDeclaration (varname, vartype, varinit, lexer.Location, null);
	  }
  break;
case 328:
#line 3862 "mb-parser.jay"
  {
		ArrayList decl = new ArrayList ();
		decl.AddRange ((ArrayList) yyVals[0+yyTop]);
		yyVal = decl;
	  }
  break;
case 329:
#line 3868 "mb-parser.jay"
  {
		ArrayList decls = (ArrayList) yyVals[-2+yyTop];
		decls.AddRange ((ArrayList) yyVals[0+yyTop]);
		yyVal = yyVals[-2+yyTop];
	  }
  break;
case 330:
#line 3877 "mb-parser.jay"
  {
	    ArrayList names = (ArrayList) yyVals[-2+yyTop];
		object varinit = yyVals[0+yyTop];
		ArrayList VarDeclarations = new ArrayList();
	  	Expression vartype;
	  	ArrayList a_dims = null;

		if ((names.Count > 1) && (varinit != null)) 
			Report.Error (
				30671, lexer.Location, 
				"Multiple variables with single type can not have " +
				"a explicit initialization" );

				
		foreach (VarName vname in names)
		{
			string varname = (string) vname.Name;
			current_rank_specifiers = (ArrayList) vname.Rank;
			a_dims = null;
			varinit = yyVals[0+yyTop];

	  		if(vname.Type != null && yyVals[-1+yyTop] != null)
				Report.Error (
					30302, lexer.Location, 
					"Type character cannot be used with explicit type declaration" );

	  		/* Some checking is required for particularly weird declarations*/
	  		/* like Dim a As Integer(,)*/
	  		if (yyVals[-1+yyTop] is Pair) {
	  			vartype = (Expression) ((Pair) yyVals[-1+yyTop]).First;
				
	  			/*if ($3 != null && $3 is ArrayList)
	  				Report.Error (205, "End of statement expected.");*/
		  			
				ArrayList args = (ArrayList) ((Pair) yyVals[-1+yyTop]).Second;
				if (current_rank_specifiers != null)
					Report.Error (31087, lexer.Location,
						 "Array types specified in too many places");	
				
				if (VariableDeclaration.IndexesSpecifiedInRank (args))	  	  
					Report.Error (30638, "Array bounds cannot appear in type specifiers.");	
				
				current_rank_specifiers = new ArrayList ();
				current_rank_specifiers.Add (args);				
	  		}
	  		else
				vartype = (yyVals[-1+yyTop] == null) ? ((vname.Type == null) ? TypeManager.system_object_expr : (Expression) vname.Type ) : (Expression) yyVals[-1+yyTop];

			/* if the variable is an array with explicit bound*/
			/* and having explicit initialization throw exception*/
			if (current_rank_specifiers != null && varinit != null) 
			{
				bool broken = false;
				foreach (ArrayList exprs in current_rank_specifiers)
				{
					foreach (Expression expr in exprs)
					{
						if (!((Expression)expr is EmptyExpression ))
						{
							Report.Error (
								30672, lexer.Location, 
								"Array declared with explicit bound " +
								" can not have explicit initialization");
							broken = true;
							break;
						}
					}
					if (broken)
						break;
				}
	  		}
			
	  		/*
	  		Check for a declaration like Dim a(2) or Dim a(2,3)
	  		If this is the case, we must generate an ArrayCreationExpression
	  		and, in case, add the initializer after the array has been created.
	  		*/
/* 	  		if (VariableDeclaration.IsArrayDecl (this)) {	*/
/* 				if (VariableDeclaration.IndexesSpecified(current_rank_specifiers)) {   */
/* 					a_dims = (ArrayList) current_rank_specifiers;*/
/* 					VariableDeclaration.VBFixIndexLists (ref a_dims);*/
/* 					varinit = VariableDeclaration.BuildArrayCreator(vartype, a_dims, (ArrayList) varinit, lexer.Location);*/
/* 				}*/
/* 				vartype = DecomposeQI (vartype.ToString() + VariableDeclaration.BuildRanks (current_rank_specifiers, false, lexer.Location), lexer.Location);*/
/* 			}*/

			if (vartype is New) {
				if (varinit != null) {
					Report.Error (30205, lexer.Location, "End of statement expected");
					yyVal = null;
				}
				else
				{
					varinit = vartype;
					vartype = ((New)vartype).RequestedType;
				}
			}
	  		VarDeclarations.Add (new VariableDeclaration (varname, vartype, varinit, lexer.Location, null));
	    }/* end of for*/
	    yyVal = VarDeclarations;
	  }
  break;
case 331:
#line 3982 "mb-parser.jay"
  {
		ArrayList list = new ArrayList ();
		list.Add (yyVals[0+yyTop]);
		yyVal = list;
	  }
  break;
case 332:
#line 3988 "mb-parser.jay"
  {
		ArrayList list = (ArrayList) yyVals[-2+yyTop];
		list.Add (yyVals[0+yyTop]);
		yyVal = list;
	  }
  break;
case 333:
#line 3997 "mb-parser.jay"
  {
		yyVal = new VarName (yyVals[-2+yyTop], yyVals[-1+yyTop], yyVals[0+yyTop]);
	  }
  break;
case 334:
#line 4004 "mb-parser.jay"
  { 
	  	yyVal = null; 		
	  }
  break;
case 335:
#line 4008 "mb-parser.jay"
  { 
	  	yyVal = (Expression) yyVals[0+yyTop];
	  }
  break;
case 337:
#line 4023 "mb-parser.jay"
  {
	  	yyVal = yyVals[0+yyTop];
	  }
  break;
case 338:
#line 4027 "mb-parser.jay"
  {
	  	New n = new New ((Expression)yyVals[0+yyTop], null, lexer.Location);
	  	yyVal = (Expression) n;
	  }
  break;
case 339:
#line 4032 "mb-parser.jay"
  {
	  	New n = new New ((Expression)yyVals[-3+yyTop], (ArrayList) yyVals[-1+yyTop], lexer.Location);
	  	yyVal = (Expression) n;
	  }
  break;
case 340:
#line 4048 "mb-parser.jay"
  { yyVal = null; }
  break;
case 341:
#line 4057 "mb-parser.jay"
  { yyVal = null; }
  break;
case 342:
#line 4058 "mb-parser.jay"
  { yyVal = yyVals[0+yyTop]; }
  break;
case 343:
#line 4063 "mb-parser.jay"
  {
		yyVal = yyVals[0+yyTop];
	  }
  break;
case 344:
#line 4067 "mb-parser.jay"
  {
		yyVal = yyVals[0+yyTop];
	  }
  break;
case 345:
#line 4075 "mb-parser.jay"
  {
		ArrayList list = new ArrayList ();
		yyVal = list;
	  }
  break;
case 346:
#line 4080 "mb-parser.jay"
  {
		yyVal = (ArrayList) yyVals[-1+yyTop];
	  }
  break;
case 347:
#line 4087 "mb-parser.jay"
  {
		ArrayList list = new ArrayList ();
		list.Add (yyVals[0+yyTop]);
		yyVal = list;
	  }
  break;
case 348:
#line 4093 "mb-parser.jay"
  {
		ArrayList list = (ArrayList) yyVals[-2+yyTop];
		list.Add (yyVals[0+yyTop]);
		yyVal = list;
	  }
  break;
case 349:
#line 4102 "mb-parser.jay"
  {
		  /* $$ = "";*/
		  yyVal = null;
	  }
  break;
case 350:
#line 4107 "mb-parser.jay"
  {
			yyVal = yyVals[0+yyTop];
	  }
  break;
case 351:
#line 4114 "mb-parser.jay"
  {
		  ArrayList rs = new ArrayList();
		  rs.Add (yyVals[0+yyTop]);
		  yyVal = rs;
	  }
  break;
case 352:
#line 4120 "mb-parser.jay"
  {
		  ArrayList rs = (ArrayList) yyVals[-1+yyTop];
		  rs.Add (yyVals[0+yyTop]);
		  yyVal = rs;
	  }
  break;
case 353:
#line 4129 "mb-parser.jay"
  {
		yyVal = yyVals[-1+yyTop];
	  }
  break;
case 354:
#line 4136 "mb-parser.jay"
  {
	  	ArrayList ds = new ArrayList();
	  	ds.Add (new EmptyExpression());
	  	yyVal = ds;
	  }
  break;
case 355:
#line 4142 "mb-parser.jay"
  {
	  	ArrayList ds = new ArrayList();
	  	ds.Add ((Expression) yyVals[0+yyTop]);
	  	yyVal = ds;
	  }
  break;
case 356:
#line 4148 "mb-parser.jay"
  {
		ArrayList ds = (ArrayList) yyVals[-2+yyTop];
	  	ds.Add ((Expression) yyVals[0+yyTop]);
	  	yyVal = ds;		
	  }
  break;
case 357:
#line 4154 "mb-parser.jay"
  {
		ArrayList ds = (ArrayList) yyVals[-1+yyTop];
	  	ds.Add (new EmptyExpression());
	  	yyVal = ds;		
	  }
  break;
case 358:
#line 4184 "mb-parser.jay"
  {
	  	/*TODO*/
	  }
  break;
case 362:
#line 4191 "mb-parser.jay"
  {
		yyVal = ((MemberName) yyVals[0+yyTop]).GetTypeExpression (lexer.Location);
	  }
  break;
case 371:
#line 4209 "mb-parser.jay"
  { yyVal = new CharLiteral ((char) lexer.Value); }
  break;
case 372:
#line 4210 "mb-parser.jay"
  { yyVal = new StringLiteral ((string) lexer.Value); }
  break;
case 373:
#line 4211 "mb-parser.jay"
  { yyVal = NullLiteral.Null; }
  break;
case 374:
#line 4215 "mb-parser.jay"
  { yyVal = new FloatLiteral ((float) lexer.Value); }
  break;
case 375:
#line 4216 "mb-parser.jay"
  { yyVal = new DoubleLiteral ((double) lexer.Value); }
  break;
case 376:
#line 4217 "mb-parser.jay"
  { yyVal = new DecimalLiteral ((decimal) lexer.Value); }
  break;
case 377:
#line 4221 "mb-parser.jay"
  {
		object v = lexer.Value;

		if (v is int)
			yyVal = new IntLiteral ((Int32)v); 
/* 		else if (v is short)*/
/* 			$$ = new ShortLiteral ((Int16)v);*/
		else if (v is long)
			yyVal = new LongLiteral ((Int64)v);
		else
			Console.WriteLine ("OOPS.  Unexpected result from scanner");
			
	  }
  break;
case 378:
#line 4237 "mb-parser.jay"
  { yyVal = new BoolLiteral (true); }
  break;
case 379:
#line 4238 "mb-parser.jay"
  { yyVal = new BoolLiteral (false); }
  break;
case 380:
#line 4243 "mb-parser.jay"
  { yyVal = yyVals[-1+yyTop]; }
  break;
case 381:
#line 4248 "mb-parser.jay"
  {
	  	if (yyVals[-2+yyTop] != null) {
	  		string id_name = (string)yyVals[0+yyTop];
	  		if (id_name.ToUpper() == "NEW")
	  			id_name = ".ctor";
			yyVal = new MemberAccess ((Expression) yyVals[-2+yyTop], id_name, lexer.Location);
		}
		else
		{
/* 			if (with_stack.Count > 0) {*/
/* 				Expression e = (Expression) with_stack.Peek();*/
/* 				$$ = new MemberAccess (e, (string) $3, lexer.Location);*/
/* 			}*/
/* 			else*/
/* 			{*/
/* 				// OOps*/
/* 			}*/
		}
	  }
  break;
case 382:
#line 4272 "mb-parser.jay"
  {
	  	if (yyVals[-2+yyTop] != null)
			yyVal = new MemberAccess ((Expression) yyVals[-2+yyTop], (string) yyVals[0+yyTop], lexer.Location);
		else
		{
/* 			if (with_stack.Count > 0) {*/
/* 				Expression e = (Expression) with_stack.Peek();*/
/* 				$$ = new MemberAccess (e, (string) $3, lexer.Location);*/
/* 			}*/
/* 			else*/
/* 			{*/
/* 				// OOps*/
/* 			}*/
		}
	  }
  break;
case 384:
#line 4295 "mb-parser.jay"
  {
		if (yyVals[-3+yyTop] == null) {
			Location l = lexer.Location;
			Report.Error (1, l, "Parse error");
		}
		yyVal = new Invocation ((Expression) yyVals[-3+yyTop], (ArrayList) yyVals[-1+yyTop], lexer.Location);
	  }
  break;
case 385:
#line 4303 "mb-parser.jay"
  {
		if (yyVals[-3+yyTop] == null) {
			Location l = lexer.Location;
			Report.Error (1, l, "THIS IS CRAZY");
		}
		yyVal = new Invocation ((Expression) yyVals[-3+yyTop], (ArrayList) yyVals[-2+yyTop], lexer.Location);
/*		Console.WriteLine ("Invocation: {0} with {1} arguments", $2, ($3 != null) ? ((ArrayList) $3).Count : 0);*/
	  }
  break;
case 386:
#line 4315 "mb-parser.jay"
  {
		string id_name = (string) yyVals[0+yyTop];
		if (id_name.ToUpper() == "NEW")
			id_name = "New";
		yyVal = new BaseAccess (id_name, lexer.Location);
	  }
  break;
case 387:
#line 4329 "mb-parser.jay"
  { 
		/*
		   The 'argument' rule returns an 'empty' argument
		   of type NoArg (used for default arguments in invocations)
		   if no arguments are actually passed.

		   If there is only one argument and it is o type NoArg,
		   we return a null (empty) list
		*/
		ArrayList args = (ArrayList) yyVals[0+yyTop];
		if (args.Count == 1 &&
		    ((Argument)args[0]).ArgType == Argument.AType.NoArg)
			yyVal = null;
		else
			yyVal = yyVals[0+yyTop];
	  }
  break;
case 388:
#line 4349 "mb-parser.jay"
  {
		ArrayList list = new ArrayList ();
		list.Add (yyVals[0+yyTop]);
		yyVal = list;
	  }
  break;
case 389:
#line 4355 "mb-parser.jay"
  {
		ArrayList list = (ArrayList) yyVals[-2+yyTop];
		list.Add (yyVals[0+yyTop]);
		yyVal = list;
	  }
  break;
case 390:
#line 4364 "mb-parser.jay"
  {
		yyVal = new Argument ((Expression) yyVals[0+yyTop], Argument.AType.Expression);
	  }
  break;
case 391:
#line 4368 "mb-parser.jay"
  {
		yyVal = new Argument ((Expression) yyVals[0+yyTop], Argument.AType.Ref);
	  }
  break;
case 392:
#line 4372 "mb-parser.jay"
  {
	  	yyVal = new Argument (new EmptyExpression (), Argument.AType.NoArg);
	  }
  break;
case 393:
#line 4376 "mb-parser.jay"
  {
		yyVal = new Argument ((Expression) yyVals[0+yyTop], Argument.AType.AddressOf);
	  }
  break;
case 394:
#line 4382 "mb-parser.jay"
  {/* note ("section 5.4"); */  yyVal = yyVals[0+yyTop];  }
  break;
case 395:
#line 4387 "mb-parser.jay"
  { yyVal = yyVals[0+yyTop]; }
  break;
case 398:
#line 4398 "mb-parser.jay"
  {
		yyVal = new This (current_block, lexer.Location);
	  }
  break;
case 399:
#line 4402 "mb-parser.jay"
  {
		/* FIXME: This is actually somewhat different from Me*/
		/* because it is for accessing static (classifier) methods/properties/fields*/
		yyVal = new This (current_block, lexer.Location);
	  }
  break;
case 400:
#line 4411 "mb-parser.jay"
  {
	  	/* TODO*/
	  }
  break;
case 401:
#line 4415 "mb-parser.jay"
  {
		  yyVal = new Cast ((Expression) yyVals[-1+yyTop], (Expression) yyVals[-3+yyTop], lexer.Location);
	  }
  break;
case 402:
#line 4419 "mb-parser.jay"
  {
		  yyVal = new Cast ((Expression) yyVals[-3+yyTop], (Expression) yyVals[-1+yyTop], lexer.Location);
	  }
  break;
case 403:
#line 4425 "mb-parser.jay"
  { yyVal = TypeManager.system_boolean_expr; 	}
  break;
case 404:
#line 4426 "mb-parser.jay"
  { yyVal = TypeManager.system_byte_expr;	 	}
  break;
case 405:
#line 4427 "mb-parser.jay"
  { yyVal = TypeManager.system_char_expr; 		}
  break;
case 406:
#line 4429 "mb-parser.jay"
  { yyVal = TypeManager.system_double_expr; 		}
  break;
case 407:
#line 4430 "mb-parser.jay"
  { yyVal = TypeManager.system_decimal_expr; 	}
  break;
case 408:
#line 4431 "mb-parser.jay"
  { yyVal = TypeManager.system_int32_expr; 		}
  break;
case 409:
#line 4432 "mb-parser.jay"
  { yyVal = TypeManager.system_int64_expr; 		}
  break;
case 410:
#line 4433 "mb-parser.jay"
  { yyVal = TypeManager.system_object_expr; 		}
  break;
case 411:
#line 4434 "mb-parser.jay"
  { yyVal = TypeManager.system_int16_expr; 		}
  break;
case 412:
#line 4435 "mb-parser.jay"
  { yyVal = TypeManager.system_single_expr; 		}
  break;
case 413:
#line 4436 "mb-parser.jay"
  { yyVal = TypeManager.system_string_expr; 	}
  break;
case 414:
#line 4441 "mb-parser.jay"
  {
		yyVal = new TypeOf ((Expression) yyVals[-1+yyTop], lexer.Location);
  	  }
  break;
case 416:
#line 4449 "mb-parser.jay"
  {
	  	/*TODO*/
	  }
  break;
case 418:
#line 4457 "mb-parser.jay"
  {
	  	/*FIXME: Is this rule correctly defined ?*/
	  	yyVal = new Unary (Unary.Operator.UnaryPlus, (Expression) yyVals[0+yyTop], lexer.Location);
	  }
  break;
case 419:
#line 4462 "mb-parser.jay"
  {
	  	/*FIXME: Is this rule correctly defined ?*/
		yyVal = new Unary (Unary.Operator.UnaryNegation, (Expression) yyVals[0+yyTop], lexer.Location);
	  }
  break;
case 421:
#line 4471 "mb-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.Multiply,
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop], lexer.Location);
	  }
  break;
case 422:
#line 4476 "mb-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.Division,
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop], lexer.Location);
	  }
  break;
case 424:
#line 4485 "mb-parser.jay"
  {
	  	/*FIXME: Is this right ?*/
		yyVal = new Binary (Binary.Operator.Division,
			   (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop], lexer.Location);
          }
  break;
case 426:
#line 4495 "mb-parser.jay"
  {
	      yyVal = new Binary (Binary.Operator.Modulus,
			       (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop], lexer.Location);
	  }
  break;
case 428:
#line 4504 "mb-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.Addition,
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop], lexer.Location);
	  }
  break;
case 429:
#line 4509 "mb-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.Subtraction,
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop], lexer.Location);
	  }
  break;
case 431:
#line 4518 "mb-parser.jay"
  {
	      /* FIXME: This should only work for String expressions*/
	      /* We probably need to use something from the runtime*/
	      yyVal = new Binary (Binary.Operator.Addition,
			       (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop], lexer.Location);
	  }
  break;
case 433:
#line 4529 "mb-parser.jay"
  {
	  	/* TODO*/
	  }
  break;
case 434:
#line 4533 "mb-parser.jay"
  {
	  	/*TODO*/
	  }
  break;
case 436:
#line 4541 "mb-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.Equality,
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop], lexer.Location);
	  }
  break;
case 437:
#line 4546 "mb-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.Inequality, 
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop], lexer.Location);
	  }
  break;
case 438:
#line 4551 "mb-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.LessThan,
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop], lexer.Location);
	  }
  break;
case 439:
#line 4556 "mb-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.GreaterThan,
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop], lexer.Location);
	  }
  break;
case 440:
#line 4561 "mb-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.LessThanOrEqual,
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop], lexer.Location);
	  }
  break;
case 441:
#line 4566 "mb-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.GreaterThanOrEqual,
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop], lexer.Location);
	  }
  break;
case 442:
#line 4571 "mb-parser.jay"
  {
	  	/*FIXME: Should be a different op for reference equality but allows tests to use Is*/
		yyVal = new Binary (Binary.Operator.Equality,
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop], lexer.Location);
	  }
  break;
case 443:
#line 4577 "mb-parser.jay"
  {
		/*FIXME: Is this rule correctly defined ?*/
		yyVal = new Is ((Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop], lexer.Location);
	  }
  break;
case 445:
#line 4586 "mb-parser.jay"
  {
	  	/*FIXME: Is this rule correctly defined ?*/
		yyVal = new Unary (Unary.Operator.LogicalNot, (Expression) yyVals[0+yyTop], lexer.Location);
	  }
  break;
case 447:
#line 4595 "mb-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.LogicalAnd,
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop], lexer.Location);
	  }
  break;
case 448:
#line 4600 "mb-parser.jay"
  {	/* FIXME: this is likely to be broken*/
		yyVal = new Binary (Binary.Operator.LogicalAnd,
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop], lexer.Location);
	  }
  break;
case 450:
#line 4609 "mb-parser.jay"
  {
		yyVal = new Binary (Binary.Operator.LogicalOr,
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop], lexer.Location);
	  }
  break;
case 451:
#line 4614 "mb-parser.jay"
  {	/* FIXME: this is likely to be broken*/
		yyVal = new Binary (Binary.Operator.LogicalOr,
			         (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop], lexer.Location);
	  }
  break;
case 453:
#line 4623 "mb-parser.jay"
  {
	      yyVal = new Binary (Binary.Operator.ExclusiveOr,
			       (Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop], lexer.Location);
	}
  break;
case 454:
#line 4631 "mb-parser.jay"
  { 
		yyVal = new Assign ((Expression) yyVals[-2+yyTop], (Expression) yyVals[0+yyTop], lexer.Location);
	  }
  break;
case 455:
#line 4635 "mb-parser.jay"
  {
		Location l = lexer.Location;

		yyVal = new CompoundAssign (
			Binary.Operator.Multiply, (Expression) yyVals[-3+yyTop], (Expression) yyVals[0+yyTop], l);
	  }
  break;
case 456:
#line 4642 "mb-parser.jay"
  {
		Location l = lexer.Location;

		yyVal = new CompoundAssign (
			Binary.Operator.Division, (Expression) yyVals[-3+yyTop], (Expression) yyVals[0+yyTop], l);
	  }
  break;
case 457:
#line 4649 "mb-parser.jay"
  {
		Location l = lexer.Location;

		yyVal = new CompoundAssign (
			Binary.Operator.Addition, (Expression) yyVals[-3+yyTop], (Expression) yyVals[0+yyTop], l);
	  }
  break;
case 458:
#line 4656 "mb-parser.jay"
  {
		Location l = lexer.Location;

		yyVal = new CompoundAssign (
			Binary.Operator.Subtraction, (Expression) yyVals[-3+yyTop], (Expression) yyVals[0+yyTop], l);
	  }
  break;
case 459:
#line 4663 "mb-parser.jay"
  {
		Location l = lexer.Location;

		yyVal = new CompoundAssign (
			Binary.Operator.LeftShift, (Expression) yyVals[-3+yyTop], (Expression) yyVals[0+yyTop], l);
	  }
  break;
case 460:
#line 4670 "mb-parser.jay"
  {
		Location l = lexer.Location;

		yyVal = new CompoundAssign (
			Binary.Operator.RightShift, (Expression) yyVals[-3+yyTop], (Expression) yyVals[0+yyTop], l);
	  }
  break;
case 461:
#line 4677 "mb-parser.jay"
  {
		Location l = lexer.Location;

		/* FIXME should be strings only*/
		yyVal = new CompoundAssign (
			Binary.Operator.Addition, (Expression) yyVals[-3+yyTop], (Expression) yyVals[0+yyTop], l);
	  }
  break;
case 462:
#line 4685 "mb-parser.jay"
  {
		Location l = lexer.Location;

		/* TODO: $$ = new CompoundAssign (
			Binary.Operator.ExclusiveOr, (Expression) $1, (Expression) $4, l); */
	  }
  break;
case 463:
#line 4692 "mb-parser.jay"
  { 
/* 		ArrayList args = new ArrayList();*/
/* 		Argument arg = new Argument ((Expression) $4, Argument.AType.Expression);*/
/* 		args.Add (arg);*/
		
/* 		New n = new New ((Expression) $1, (ArrayList) args, lexer.Location);*/
/* 		n.isDelegate = true;*/
		yyVal = new Assign ((Expression) yyVals[-3+yyTop], (Expression) yyVals[0+yyTop], lexer.Location);
	  }
  break;
case 466:
#line 4713 "mb-parser.jay"
  {  	
		yyVal = ((MemberName) yyVals[0+yyTop]).GetTypeExpression (lexer.Location);
	  }
  break;
case 468:
#line 4722 "mb-parser.jay"
  {
		ArrayList types = new ArrayList ();

		types.Add (yyVals[0+yyTop]);
		yyVal = types;
	  }
  break;
case 469:
#line 4729 "mb-parser.jay"
  {
		ArrayList types = (ArrayList) yyVals[-2+yyTop];

		types.Add (yyVals[0+yyTop]);
		yyVal = types;
	  }
  break;
case 471:
#line 4742 "mb-parser.jay"
  { yyVal = TypeManager.system_object_expr; }
  break;
case 474:
#line 4748 "mb-parser.jay"
  { yyVal = TypeManager.system_boolean_expr; }
  break;
case 475:
#line 4750 "mb-parser.jay"
  { yyVal = TypeManager.system_char_expr; }
  break;
case 476:
#line 4751 "mb-parser.jay"
  { yyVal = TypeManager.system_string_expr; }
  break;
case 479:
#line 4758 "mb-parser.jay"
  { yyVal = TypeManager.system_decimal_expr; }
  break;
case 481:
#line 4763 "mb-parser.jay"
  { yyVal = TypeManager.system_byte_expr; }
  break;
case 482:
#line 4764 "mb-parser.jay"
  { yyVal = TypeManager.system_int16_expr; }
  break;
case 483:
#line 4765 "mb-parser.jay"
  { yyVal = TypeManager.system_int32_expr; }
  break;
case 484:
#line 4766 "mb-parser.jay"
  { yyVal = TypeManager.system_int64_expr; }
  break;
case 485:
#line 4770 "mb-parser.jay"
  { yyVal = TypeManager.system_single_expr; }
  break;
case 486:
#line 4771 "mb-parser.jay"
  { yyVal = TypeManager.system_double_expr; }
  break;
case 487:
#line 4776 "mb-parser.jay"
  { 
/* 	  	if(tokenizerController.IsAcceptingTokens)*/
/* 	  	{*/
/* 	  		if(in_external_source) */
/* 				Report.Error (30580, lexer.Location, "#ExternalSource directives may not be nested");*/
/* 			else {*/
/* 	  			in_external_source = true;*/
			
/* 				lexer.EffectiveSource = (string) $4;*/
/* 				lexer.EffectiveLine = (int) $6;*/
/* 			}*/
/* 	  	}*/
	  }
  break;
case 488:
#line 4790 "mb-parser.jay"
  {
	  	if(tokenizerController.IsAcceptingTokens) 
	  	{
	  		string id = (yyVals[-2+yyTop] as string);
		
		  	if(!(yyVals[-2+yyTop] as string).ToLower().Equals("region"))
				Report.Error (30205, lexer.Location, "Invalid Pre-processor directive");
			else
			{
				++in_marked_region;
			}
	  	}
	  }
  break;
case 489:
#line 4804 "mb-parser.jay"
  {
	  	if(tokenizerController.IsAcceptingTokens)
	  	{
/* 	  		if( ($3 as string).ToLower().Equals("externalsource")) {*/
/* 				if(!in_external_source)*/
/* 					Report.Error (30578, lexer.Location, "'#End ExternalSource' must be preceded by a matching '#ExternalSource'");*/
/* 				else {*/
/* 					in_external_source = false;*/
/* 					lexer.EffectiveSource = lexer.Source;*/
/* 					lexer.EffectiveLine = lexer.Line;*/
/* 				}*/
/* 	  		}*/
			/* else */if((yyVals[-1+yyTop] as string).ToLower().Equals("region")) {
				if(in_marked_region > 0)
					--in_marked_region;
				else
					Report.Error (30205, lexer.Location, "'#End Region' must be preceded  by a matching '#Region'");
			}
			else {
				Report.Error (29999, lexer.Location, "Unrecognized Pre-Processor statement");
			}	
	  	}
	  }
  break;
case 490:
#line 4828 "mb-parser.jay"
  {
	  	if(tokenizerController.IsAcceptingTokens)
	  	{
			/*TODO;*/
	  	}
	  }
  break;
case 491:
#line 4835 "mb-parser.jay"
  {
	      	IfElseStateMachine.Token tok = IfElseStateMachine.Token.IF;

		try {
			ifElseStateMachine.HandleToken(tok);
		}
		catch(ApplicationException) {
			throw new MBASException(ifElseStateMachine.Error, lexer.Location, ifElseStateMachine.ErrString);
		}
	  }
  break;
case 492:
#line 4846 "mb-parser.jay"
  {
	  	HandleConditionalDirective(IfElseStateMachine.Token.IF, (BoolLiteral)yyVals[-2+yyTop]);
	  }
  break;
case 493:
#line 4850 "mb-parser.jay"
  {
		      IfElseStateMachine.Token tok = IfElseStateMachine.Token.ELSEIF;
		      try {
			      ifElseStateMachine.HandleToken(tok);
		      }
		      catch(ApplicationException) {
			      throw new MBASException(ifElseStateMachine.Error, lexer.Location, ifElseStateMachine.ErrString);
		      }
  	  }
  break;
case 494:
#line 4860 "mb-parser.jay"
  { 
		  HandleConditionalDirective(IfElseStateMachine.Token.ELSEIF, (BoolLiteral)yyVals[-2+yyTop]);
	  }
  break;
case 495:
#line 4864 "mb-parser.jay"
  {
		    IfElseStateMachine.Token tok = IfElseStateMachine.Token.ELSE;
		    try {
			    ifElseStateMachine.HandleToken(tok);
		    }
		    catch(ApplicationException) {
			    throw new MBASException(ifElseStateMachine.Error, lexer.Location, ifElseStateMachine.ErrString);
		    }
	  }
  break;
case 496:
#line 4874 "mb-parser.jay"
  { 
	  	HandleConditionalDirective(IfElseStateMachine.Token.ELSE, new BoolLiteral(true));
	  }
  break;
case 497:
#line 4878 "mb-parser.jay"
  {
		  IfElseStateMachine.Token tok = IfElseStateMachine.Token.ENDIF;
		  try {
			  ifElseStateMachine.HandleToken(tok);
		  }
		  catch(ApplicationException) {
			  throw new MBASException(ifElseStateMachine.Error, lexer.Location, ifElseStateMachine.ErrString);
		  }
	  }
  break;
case 498:
#line 4888 "mb-parser.jay"
  { 
		HandleConditionalDirective(IfElseStateMachine.Token.ENDIF, new BoolLiteral(false));
	  }
  break;
case 499:
#line 4892 "mb-parser.jay"
  {
		if(tokenizerController.IsAcceptingTokens)
			Report.Error(2999, lexer.Location, "Unrecognized Pre-Processor statement");
		else
			Report.Warning (9999, lexer.Location, 	"Unrecognized Pre-Processor statement");
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
    1,    1,    2,    2,    0,    0,    4,    4,    8,    8,
    9,    9,    9,   13,   13,   13,   14,   14,   10,   11,
   12,   15,   15,    6,    6,   19,   16,   21,   16,   22,
   22,   22,   22,   22,   22,   23,   23,   23,   23,   23,
   23,   24,   24,   25,   25,    5,    5,   26,   26,   27,
   28,   28,   29,   29,   31,   31,   31,    7,    7,   33,
   35,   33,   34,   37,   37,   36,   36,   40,   38,   42,
   43,   38,   41,   41,   41,   39,   39,   44,   44,   45,
   45,   45,   46,   46,   47,   47,   49,   50,   18,   17,
   20,   20,   20,   20,   20,   20,   59,   62,   52,   58,
   58,   60,   60,   51,   51,   64,   64,   65,   65,   65,
   65,   65,   65,   65,   65,   65,   65,   65,   65,   65,
   65,   65,   67,   53,   66,   66,   68,   68,   71,   69,
   70,   70,   70,   70,   70,   70,   70,   75,   61,   61,
   79,   79,   80,   81,   81,   81,   81,   81,   81,   81,
   73,   73,   85,   82,   91,   92,   83,   94,   95,   56,
   96,   96,   93,   93,   97,   97,   98,   99,   99,   99,
   99,   99,   99,   77,  103,   57,  102,  102,  104,  104,
  105,  106,  105,  108,  111,  113,  114,   54,  110,  110,
  115,  115,  116,  112,  117,  117,  118,  118,  119,  120,
  120,  121,  121,   76,  124,  122,  109,  109,   86,   86,
  125,  125,  123,  123,  129,  129,  127,  127,  130,  131,
  126,  133,  128,  132,  132,  132,   74,  136,  136,   55,
   55,   84,  138,  139,   72,   32,   32,  140,  141,  141,
  142,  134,  134,  145,  145,  146,  146,  146,  146,   88,
   88,  147,  147,  149,   87,   89,  148,  148,  148,  148,
  148,  153,  153,  151,  151,  151,  151,  151,  159,  159,
  159,  161,  162,  160,  158,  157,  157,  164,  165,  166,
  166,  167,  167,  135,  135,  170,  168,  169,  169,  171,
  174,  172,  156,  175,  175,  175,  177,  177,  177,  176,
  176,  178,  178,  179,  179,  180,  180,  181,  181,  181,
  181,  182,  182,  155,  183,  183,  183,  185,  185,  187,
  150,  150,  188,  189,   78,   78,  190,  137,  137,  193,
  194,  194,  191,  101,  101,   90,  192,  192,  192,  195,
  144,  144,  196,  196,  197,  197,  198,  198,  143,  143,
  199,  199,  200,  201,  201,  201,  201,  202,  202,  202,
  202,  202,  202,  202,  202,  202,  202,  203,  203,  203,
  203,  203,  203,  212,  212,  212,  211,  210,  210,  204,
  208,  208,  213,  184,  184,  206,  154,  215,  215,  216,
  216,  216,  216,  217,  107,  163,  163,  205,  205,  209,
  209,  209,  219,  219,  219,  219,  219,  219,  219,  219,
  219,  219,  219,  207,  220,  220,  152,  152,  152,  221,
  221,  221,  222,  222,  223,  223,  224,  224,  224,  225,
  225,  226,  226,  226,  227,  227,  227,  227,  227,  227,
  227,  227,  227,  228,  228,  229,  229,  229,  230,  230,
  230,  218,  218,  186,  186,  186,  186,  186,  186,  186,
  186,  186,  186,   48,  173,  100,  100,   63,   63,   30,
  214,  214,  231,  231,  231,  231,  232,  232,  232,  233,
  233,  233,  233,  233,  234,  234,    3,    3,    3,    3,
  235,    3,  236,    3,  237,    3,  238,    3,    3,
  };
   static  short [] yyLen = {           2,
    1,    1,    1,    2,    5,    5,    0,    1,    1,    2,
    1,    1,    1,    0,    1,    1,    1,    1,    4,    4,
    4,    0,    1,    1,    2,    0,    3,    0,    3,    1,
    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,
    1,    0,    1,    1,    3,    0,    1,    1,    2,    3,
    1,    3,    1,    3,    0,    2,    3,    0,    1,    1,
    0,    3,    4,    0,    1,    1,    3,    0,    3,    0,
    0,    6,    1,    1,    1,    0,    3,    0,    1,    1,
    3,    1,    1,    3,    1,    3,    3,    0,    8,    2,
    1,    1,    1,    1,    1,    1,    0,    0,   11,    0,
    3,    0,    3,    0,    1,    1,    2,    1,    1,    1,
    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,
    1,    1,    0,    8,    0,    1,    1,    2,    0,    4,
    1,    1,    1,    1,    1,    1,    1,    3,    0,    1,
    1,    2,    3,    1,    1,    1,    1,    1,    1,    1,
    1,    1,    0,   13,    0,    0,   15,    0,    0,   10,
    0,    1,    0,    1,    1,    2,    2,    1,    1,    1,
    1,    1,    1,    6,    0,    9,    0,    1,    1,    2,
    3,    0,    6,    6,    0,    0,    0,   11,    0,    1,
    1,    2,    3,    1,    0,    1,    1,    2,    3,    1,
    1,    4,    6,    1,    0,   12,    0,    3,    0,    2,
    1,    3,    2,    2,    0,    1,    0,    1,    0,    0,
   11,    0,   11,    0,    2,    5,    3,    0,    1,    7,
    8,    0,    0,    0,   12,    0,    1,    1,    1,    3,
    7,    0,    1,    2,    1,    1,    1,    1,    1,    0,
    2,    1,    3,    3,    0,    0,    1,    1,    5,    5,
    3,    0,    3,    1,    1,    1,    1,    1,    1,    1,
    1,    2,    2,    2,    6,    1,    1,    6,    9,    0,
    1,    1,    2,    0,    1,    0,    5,    0,    1,    3,
    0,    9,    1,    7,   10,    6,    7,   10,    6,    0,
    1,    1,    3,    3,    1,    0,    1,    1,    1,    1,
    1,    0,    1,    1,    1,    1,    1,    5,    2,    1,
    1,    1,    2,    2,    1,    3,    3,    1,    3,    3,
    1,    3,    3,    0,    2,    1,    1,    3,    6,    0,
    0,    2,    1,    1,    2,    3,    1,    3,    0,    1,
    1,    2,    3,    0,    1,    3,    2,    1,    1,    1,
    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,
    1,    1,    1,    1,    1,    1,    1,    1,    1,    3,
    3,    3,    1,    4,    5,    3,    1,    1,    3,    1,
    2,    0,    2,    1,    1,    0,    1,    1,    1,    6,
    6,    4,    1,    1,    1,    1,    1,    1,    1,    1,
    1,    1,    1,    4,    1,    3,    1,    2,    2,    1,
    3,    3,    1,    3,    1,    3,    1,    3,    3,    1,
    3,    1,    3,    3,    1,    3,    3,    3,    3,    3,
    3,    3,    4,    1,    2,    1,    3,    3,    1,    3,
    3,    1,    3,    3,    4,    4,    4,    4,    4,    4,
    4,    4,    4,    1,    1,    1,    1,    1,    3,    1,
    1,    1,    1,    1,    1,    1,    1,    1,    1,    0,
    1,    1,    1,    1,    1,    1,    8,    4,    4,    6,
    0,    6,    0,    6,    0,    4,    0,    5,    3,
  };
   static  short [] yyDefRed = {            0,
    3,    0,    0,    0,    0,    4,    0,    0,    9,   11,
   12,   13,    0,    0,    0,    0,    0,  495,  493,    0,
  491,    0,    0,    0,    0,   48,   10,   17,   18,    0,
   16,   15,    0,    0,  499,    0,    0,    0,  497,    0,
    0,    0,    0,   31,   33,   34,   35,   32,   30,    0,
    0,    0,   51,   53,    0,    0,    0,   24,    0,    0,
   60,   49,    0,    0,    0,    0,  496,  379,  378,    0,
    0,  489,    0,    0,  488,    0,    0,    0,    0,   73,
   74,   44,    0,    0,   66,   70,    5,    0,   25,    6,
  121,  112,  119,  113,  115,  117,  114,  116,  110,  109,
  108,  120,  118,  111,  122,   90,    0,  106,    0,    0,
    0,    0,  301,    0,  498,    0,    0,   54,   45,   52,
    0,    0,    0,    0,  107,    0,   27,    0,    0,    0,
    0,    0,    0,   29,   91,   92,   93,   94,   95,   96,
   62,  490,  494,  492,    0,    0,   69,   67,    2,   65,
    0,   63,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  474,  481,    0,  403,  404,  405,  407,  406,
  475,  408,  409,  410,  411,  412,  413,    0,  479,    0,
  486,    0,  483,  484,  398,    0,  399,    0,    0,  373,
  471,  482,  485,  476,    0,    0,    0,    0,  377,  374,
  375,  376,  371,  372,    0,    0,    0,   79,    0,    0,
   83,   85,  464,  420,  365,  320,  366,    0,  358,  359,
  360,  361,  363,  364,  367,  368,  369,  370,    0,  383,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  446,    0,    0,  472,  473,  477,  478,   71,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  487,    0,    0,
    0,    0,    0,  466,    0,  467,  445,    0,    0,  418,
  419,    0,   77,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  335,    0,    0,    0,
    0,  158,    0,    0,    0,    0,  386,    0,    0,  380,
   87,    0,   84,    0,   86,    0,    0,  390,    0,    0,
  388,  381,  382,    0,    0,    0,  421,  422,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  447,  448,    0,    0,   72,    0,    0,    0,
    0,    0,    0,  237,    0,  239,    0,    0,  175,    0,
  179,    0,  186,    0,  191,    0,    0,    0,  127,    0,
    0,    0,    0,    0,    0,  414,    0,  443,  393,  394,
  391,  384,    0,  402,    0,    0,  468,    0,   98,  246,
  247,  248,  249,    0,    0,  245,    0,    0,    0,    0,
    0,  180,    0,    0,  192,  129,    0,  128,    0,    0,
  159,    0,  165,    0,    0,    0,  318,  389,    0,    0,
    0,    0,    0,    0,  244,    0,  336,  240,    0,    0,
    0,    0,    0,    0,  187,  194,    0,  197,    0,    0,
    0,    0,  229,    0,    0,    0,  173,  170,  171,  168,
  169,  172,  151,  152,  167,    0,    0,  166,  401,  400,
    0,  469,    0,    0,    0,    0,  141,   36,   37,   38,
   39,   40,   41,   43,    0,    0,    0,    0,    0,    0,
  198,    0,  137,  130,  131,  132,  133,  134,  135,  136,
  204,    0,    0,    0,  325,    0,    0,    0,    0,    0,
    0,  331,  328,    0,    0,    0,    0,  142,    0,    0,
    0,  351,    0,    0,    0,    0,    0,  201,  199,  200,
    0,    0,  340,    0,    0,    0,  337,    0,    0,    0,
    0,    0,  153,    0,    0,    0,    0,    0,  150,  147,
  146,  144,  145,  148,  149,  143,    0,  355,    0,    0,
  352,    0,    0,    0,    0,    0,    0,  333,  326,    0,
    0,  327,    0,    0,   56,    0,    0,  232,  329,  332,
  330,    0,    0,  353,    0,  241,    0,    0,    0,    0,
    0,    0,    0,    0,  343,  342,  344,    0,  155,   57,
  255,    0,  356,    0,    0,    0,    0,    0,    0,  345,
  347,    0,    0,    0,    0,    0,    0,    0,  208,    0,
    0,  346,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  256,    0,  252,  257,  258,
    0,  264,  265,  266,  267,  268,  269,  270,  271,  276,
  277,  293,  314,    0,    0,  317,  321,  322,    0,    0,
    0,    0,  339,  348,    0,    0,    0,    0,  272,  465,
    0,    0,    0,  397,  274,    0,  273,  255,  234,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  156,    0,    0,    0,  261,    0,  255,    0,    0,
    0,  253,    0,    0,    0,  454,    0,    0,    0,    0,
    0,    0,  256,    0,    0,    0,    0,    0,    0,  255,
    0,    0,    0,  256,    0,    0,    0,  282,    0,  457,
  458,  463,  455,  456,  462,  461,  459,  460,    0,    0,
    0,    0,    0,  213,  218,    0,  216,  214,  256,  259,
    0,  263,  260,    0,  254,    0,    0,  289,    0,    0,
  283,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  296,  275,    0,  286,  278,  255,    0,    0,  255,
  225,    0,    0,    0,    0,  255,    0,  294,  290,  255,
    0,    0,  220,  285,    0,  255,    0,    0,    0,  287,
    0,    0,    0,    0,    0,    0,  255,  279,  256,  226,
  256,  295,    0,    0,    0,    0,    0,  299,    0,    0,
  255,  297,    0,    0,    0,    0,    0,    0,  298,
  };
  protected static  short [] yyDgoto  = {             2,
  150,  151,    6,    7,   24,   56,  362,    8,    9,   10,
   11,   12,   33,   30,  359,   58,   59,  127,  109,  134,
  110,   82,  484,  485,  206,   25,   26,   52,   53,  264,
  542,  363,   60,   61,  111,   84,  152,   85,  147,  121,
   86,  124,  302,  207,  208,  209,  210,  211,  212,  303,
  106,  135,  136,  137,  138,  139,  140,  361,  304,  399,
  475,  433,  396,  107,  108,  377,  310,  378,  379,  494,
  449,  458,  459,  460,  461,  499,  462,  504,  476,  477,
  556,  463,  464,  602,  578,  312,  699,  636,  679,  537,
  614,  718,  421,  382,  467,    0,  422,  423,  465,  397,
  437,  369,  411,  370,  371,  441,  328,  528,  592,  373,
  309,  445,  414,  490,  374,  375,  446,  447,  448,  529,
  530,  501,  715,  691,  381,  716,  744,  717,  748,  780,
  802,  766,  796,  404,  795,  466,  511,  601,  701,  364,
  365,  366,  520,  572,  405,  406,  637,  638,  700,  639,
  640,  214,  696,  329,  642,  643,  644,  645,  646,  647,
  648,  649,  675,  650,  651,  726,  727,  728,  757,  790,
  758,    0,  671,    0,  652,  114,  772,    0,    0,    0,
    0,    0,  653,  215,  216,  656,  217,  657,  658,  505,
  506,  538,  513,  514,  568,  596,  597,  612,  521,  522,
  559,  218,  219,  220,  221,  222,  223,  224,  225,  226,
  227,  228,  229,  230,  330,  331,  391,  231,  232,  233,
  234,  235,  236,  237,  238,  239,  240,  241,  242,  243,
  244,  245,  246,  247,   41,   38,   37,   71,
  };
  protected static  short [] yySindex = {         -277,
    0,    0,  -42, -263, -240,    0, -236, -244,    0,    0,
    0,    0, -254, -100, -100, -207, -165,    0,    0, -312,
    0, -383,    9,  -51, -236,    0,    0,    0,    0, -277,
    0,    0, -277, -277,    0,  -33,   92, -274,    0,  100,
 -274,  -41,  125,    0,    0,    0,    0,    0,    0,   29,
   59, -250,    0,    0, -230, -247, 3240,    0,    0,    0,
    0,    0,   86,   86,   86, -274,    0,    0,    0,   88,
  188,    0,   88,   93,    0,    9,    9,    9,   86,    0,
    0,    0,    0,  -78,    0,    0,    0, 2192,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0, 2192,    0,  177,  736,
  -51,  223,    0,  243,    0,  256,  119,    0,    0,    0,
  165, -230, -223,  179,    0,    9,    0,    9,  -72,    9,
    9,    9,    9,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  187, 3081,    0,    0,    0,    0,
   86,    0,    9,  -91, -277,    9,    9,  343, -277, -277,
 -277,  304,    0,    0, 3385,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  206,    0,  231,
    0,  258,    0,    0,    0,  248,    0, 1614, 3081,    0,
    0,    0,    0,    0, 3233, 3081, 3233, 3233,    0,    0,
    0,    0,    0,    0,  238,   59,  265,    0,  270,  277,
    0,    0,    0,    0,    0,    0,    0, -354,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  288,    0,
  310,  309,  298, -113,  303,  413,   97,  312,   34,  323,
    0,  373,  289,    0,    0,    0,    0,    0,   86,   86,
  347,  348, 1614, -277,   86,   86, -299,    0, -133, 3081,
 3081, 1614,  290,    0,  359,    0,    0, -235,  358,    0,
    0, 3081,    0, 3081,    9, 2473,    9,    9, 3081, 3081,
 3385, 3233, 3233, 3233, 3233, 3233, 3233, 3233, 3233, 3233,
 3233, 3233, 3233, 3233, 3233, 3233, 3233, 3081, 3081, 3081,
 3081,  165,  -51,  449,  -51,  -51,    0, -239,  450,  -51,
    9,    0, 2473,  372,  381,  375,    0, 2473, 1614,    0,
    0,  277,    0,  238,    0, 3081, 3081,    0,  386,  385,
    0,    0,    0,  289,  390, -354,    0,    0, -113,  303,
  413,  413,   97,  312,  312,   34,   34,   34,   34,   34,
   34,   34,    0,    0,  373,  373,    0,  -51,  500, 1614,
  482,  195,  397,    0,  398,    0,  399,    9,    0,  -51,
    0, 1614,    0,  450,    0, 2192,  517,  -51,    0,   59,
  417, 2192,  424, 1614, 1614,    0,  438,    0,    0,    0,
    0,    0, 2473,    0,  509, -217,    0, 1614,    0,    0,
    0,    0,    0,    9,  195,    0,  343,  -51, -277, -277,
  554,    0, -217,  -51,    0,    0,  525,    0,    9, 2149,
    0, 2192,    0,    0,  453,  454,    0,    0, -277, 1614,
   86, -217,  -51,  731,    0, -277,    0,    0,   86,   86,
  463,  561,   86, 2192,    0,    0,  -51,    0, 1957, -277,
   59,    9,    0,    9,    9, -116,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    9,  567,    0,    0,    0,
   86,    0,   86, 2192,  574,  -51,    0,    0,    0,    0,
    0,    0,    0,    0,  478,   86, 3081, -277,   99,  584,
    0,    9,    0,    0,    0,    0,    0,    0,    0,    0,
    0,   86,  731, -184,    0,  625,  629,  731,  488,  488,
 -161,    0,    0, -252,  518, 1957,  618,    0, 3081,  343,
  478,    0, -277,   86,    9,    9,    9,    0,    0,    0,
  581,  731,    0,    9,   86,  218,    0,  491, 1614,  488,
   87, -277,    0,    9,   86,    9,  491, -277,    0,    0,
    0,    0,    0,    0,    0,    0, -277,    0,  114,  491,
    0,   86,  731,  731,  488, -277,  503,    0,    0, 1614,
 2929,    0,  589,  343,    0,  506,   86,    0,    0,    0,
    0,   86,   86,    0, 3081,    0,  488,  503, -277,   86,
  -51,  343,  519, 2777,    0,    0,    0, -277,    0,    0,
    0,  589,    0,  343,  343,   86,  513,  589, 2473,    0,
    0,  -80,   86,  589, 2321, -277, -277, -277,    0, -277,
  514,    0, 2929, -277, 3233,    9,    9,  456, 3081,    9,
 3233, 3081, 3081, 3081, -223,    0, -223,    0,    0,    0,
  919,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,   86,   86,
   86,   86,    0,    0,   86,  522,  523,  526,    0,    0,
   88,  530,  532,    0,    0, -223,    0,    0,    0, 2321,
  538,  541, 2625,  547,  551,  553,  560,  563,  577, 2321,
  -51,    0,  688, -223, 2473,    0,  712,    0, 2321,  704,
  691,    0, 3081, 3081, 3081,    0, 3081, 3081, 3081, 3081,
 3081, 3081,    0, -272,  692,  -51,  -51, 2321, 3081,    0,
  591, 3081,  699,    0,    9,  126,  704,    0,  620,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  705, -277,
  613,  661,  656,    0,    0,  713,    0,    0,    0,    0,
  486,    0,    0,  645,    0,  769, -223,    0,  643, -223,
    0, -277,  652,   86,  -31, -277, -277,  739, -223, 3081,
  718,    0,    0, 1614,    0,    0,    0,   86, -277,    0,
    0,    9,   86,   86,  733,    0,   88,    0,    0,    0,
  740,   86,    0,    0,  343,    0, -277,  744, -223,    0,
  658, 2321,  640, 2321,   86,  737,    0,    0,    0,    0,
    0,    0,  516,  749,  757, -223,  741,    0,  745,  693,
    0,    0, -277, -277,  767,   86,   86,  753,    0,
  };
  protected static  short [] yyRindex = {            0,
    0,    0, 7141,    0,    0,    0, 7429, 7213,    0,    0,
    0,    0,    0,  772,  772,    0,    0,    0,    0,    0,
    0,    0,    0, 3449, 7501,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0, -288,
 5251,    0,    0,    0,    0, 8024,  728,    0,  732,  730,
    0,    0, 6925, 6997, 7069,    0,    0,    0,    0,  774,
    0,    0,  774,    0,    0,    0,    0,    0, 7285,    0,
    0,    0,  468,    0,    0,    0,    0,  728,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  -19,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
   44,    0,  840,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  252,    0,    0,    0,    0,
  557,    0,    0,    0,    0,    0,    0,  775,    0,    0,
    0,    0,    0,    0,  664,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0, 5324,  664,    0,
    0,    0,    0,    0,  664,  664,  664,  664,    0,    0,
    0,    0,    0,    0, 5776, 5447,    0,    0,  673,  674,
    0,    0,    0,    0,    0,    0,    0, 5582,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  403,    0, 5729, 5826, 5948, 6043, 6212, 6331, 6468,  360,
    0,  333,  189,    0,    0,    0,    0,    0, 7573, 3558,
    0,    0,  122,    0, 7602, 3807, 5253,    0,    0,  664,
  664,  679,    0,    0, 5544,    0,    0,    0,    0,    0,
    0,  664,    0,  664,    0,  539,    0,    0,  664,  664,
  664,  664,  664,  664,  664,  664,  664,  664,  664,  664,
  664,  664,  664,  664,  664,  664,  664,  664,  664,  664,
  664,   44, 7912, 3640,   91,   91,    0, -122, 7697, 4834,
    0,    0,  539,    0,    0,    0,    0,  539, 6503,    0,
    0,  686,    0,    0,    0,  664,  664,    0,    0,  687,
    0,    0,    0,  389,    0, 5679,    0,    0, 5876, 5991,
 6093, 6162, 6262, 6381, 6431, 6540, 6590, 6627, 6640, 6706,
 6749, 6819,    0,    0, 1572, 1686,    0, 7952,    0, -159,
 3892,   60,    0,    0,  689,    0,    0,    0,    0, -117,
    0, -159,    0, 7732,    0,  395,    0, 4919,    0, 3974,
 5001,  422,    0,  679,  679,    0,    0,    0,    0,    0,
    0,    0,  539,    0,    0,    0,    0, -159,    0,    0,
    0,    0,    0,    0, -246,    0,  775,  103,    0,  682,
    0,    0,    0, 1808,    0,    0,    0,    0,    0,   71,
    0,  670,    0, 5374,    0,    0,    0,    0,    0, -159,
 3725,    0, 5086, -233,    0,    0,    0,    0,  925, -206,
    0,    0, 7667,  152,    0,    0, 2000,    0,   71,    0,
 4084,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
 7357,    0, 4166,  395,    0, 5168,    0,    0,    0,    0,
    0,    0,    0,    0, -181, 1009,  664,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0, 1093,  -88,    0,    0,  -38,    0, -186,  794,  -65,
    0,    0,    0, -114,    0,   71,    0,    0,  550,  286,
 -112,    0,    0, 1177,    0,    0,    0,    0,    0,    0,
    0, -186,    0,    0, 4251,  -23,    0, -189,   30,  319,
  103,    0,    0,    0, 4333,    0, -189,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  207,
    0, -195, -218, -218,  794,    0,  461,    0,    0,   10,
  664,    0,  795,   90,    0,    0, 1553,    0,    0,    0,
    0, 1261, 1345,    0,  571,    0, -237,   48,    0, 1429,
   91,   90,   80,  664,    0,    0,    0,    0,    0,    0,
    0,  795,    0,  775,  775, 7762,    0,  795,  539,    0,
    0,    0, 4418,  795, -282,    0,    0,    0,    0,    0,
    0,    0,  664,    0,  664,    0,    0,    0,  664,    0,
  664, -151,  664, -151,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  216,  835,    0,    0,    0, 1745, 7797,
 7827,   25,    0,    0, 1745,    0,  -90,  -45,    0,    0,
  -14,  -13,    0,    0,    0,    0,    0,    0,    0,  268,
    0,    0,  664,    0,    0,    0,    0,    0,    0, -282,
 -262,    0,    0,    0,  539,    0,    0,    0,  371,  240,
    0,    0,  664,  664,  664,    0,  664,  664,  664,  664,
  664,  664,    0,    0,    0, -251,  156, -282,  664,    0,
    0,  664,    0,    0,    8,    0,  245,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  799,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0, 1937, -215,    0,    0,    0,    0,  664,
    0,    0,    0,   12,    0,    0,    0, 4500,    0,    0,
    0, -249, 2129, 4585,    0,    0,  -14,    0,    0,    0,
    0, 4667,    0,    0,  697,    0,    0,    0,    0,    0,
    0, -282,    0, -282, 4752,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0, -287, -278,    0,    0,
  };
  protected static  short [] yyGindex = {            0,
 -394,    1,    0,    0,    0,  809,  -20,    0, 1107,    0,
    0,    0, 1105,    0,    0,  -50,    0,    0,    0, -136,
    0,  -16,    0,  192,   23,    0, 1096,    0, 1045,  462,
  139, -304,    0, 1013,    0,    0,    0, 1004,  825,    0,
    0,    0,    0,    0,    0,    0,  856,  355,  857,    0,
  580,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0, -327,    0, 1024,    0,    0,    0,  758,    0,
    0, -419, -411, -210, -200,  619, -198,  515,    0,  666,
    0,    0,    0,    0,    0,  181, -590, -216, -137, -389,
    0,    0,    0,    0,    0,    0,    0,  721,    0, -183,
  982,    0,    0,    0,  776,    0,  -52,    0,  559,    0,
    0,    0,    0,    0,    0,  771,    0,    0,  702,    0,
    0,    0,    0,    0,    0,  435,    0,  439,    0,    0,
    0,    0,    0,  391,    0,    0,  527,    0,    0,    0,
    0,  759,    0, -211,    0,  761,    0,  489, -229,    0,
    0, -154,    0, -305,    0,    0,    0,    0,    0,    0,
    0,    0,  534,    0,    0,    0,    0,  444,    0,    0,
    0,    0,  402,    0,    0,  -73,  361,    0,    0,    0,
    0,    0,    0, -511, -196,    0,    0,    0,    0,  639,
 -457,  662,  631,    0,    0, -542,    0,    0,    0,  657,
    0, -143,    0,    0,    0,    0,    0,    0,    0,  416,
    0,    0,    0, -185,    0,  784,    0,    0,    0,    0,
  897,  898,  401,  899,  442,  698,    0,  -94,  436,  906,
    0,    0,    0,    0,    0,    0,    0,    0,
  };
  protected static  short [] yyTable = {           116,
    3,  367,  266,   57,  265,   89,   50,  383,  512,   87,
  615,   39,  387,  536,   28,   16,  284,  436,  221,  243,
   44,  259,  243,  250,   13,  311,   42,  223,   55,  495,
   63,    1,   42,   64,   65,   88,   80,  496,   44,   68,
  223,  243,  270,  271,  413,   51,  740,   42,   17,   14,
  242,  611,   79,  242,  217,  276,   58,   45,    1,  277,
  119,   50,  181,   18,   19,   20,  243,  266,  690,  307,
  432,   55,  242,  183,  692,   43,  266,   51,  316,   42,
  664,  181,   46,   21,  349,    1,  512,  221,  580,   23,
   42,    1,  183,  213,  267,  319,  550,  242,   51,  181,
   51,   35,  741,  654,  551,  243,  181,    5,   81,    4,
  183,  155,   58,  158,  159,  160,  161,  183,   69,  341,
   15,   47,   42,   58,    1,   44,   44,  337,  338,  205,
  560,  480,  221,  266,   29,  388,  242,  336,   42,  251,
  252,  223,  243,  269,   51,  181,   58,    1,  154,  480,
   40,   58,   44,  350,  249,  250,  183,  396,   48,  255,
  256,  257,  546,  284,   78,   58,  243,    5,  654,  512,
   58,   45,   55,  242,  266,   51,   42,   42,  654,   42,
   55,   42,  181,  177,  599,   42,  266,  654,  178,  793,
   58,   42,  149,  183,  334,   58,   46,  430,  266,  266,
  425,  426,  608,  353,  354,  804,  654,  314,  315,  289,
   51,  290,  266,  181,  617,  618,  243,    1,  324,  213,
   42,  213,   22,   42,  183,  341,  341,  335,  509,   58,
  534,  349,   49,  349,   58,   47,  576,  349,  497,  400,
  678,  401,  680,   55,  266,  156,  472,  242,  498,  105,
  500,   31,   32,  544,  308,  480,  181,  205,  324,   55,
  332,  333,  480,  323,  396,  105,   58,  183,  105,  105,
  334,   58,   48,  389,  390,   51,  313,   44,  105,  105,
  277,  698,   88,  457,   51,  480,  607,  368,  105,  376,
  654,  105,  654,  105,  300,  262,   45,   36,  105,  720,
  350,  334,  350,  621,  334,  552,  350,   89,  282,  283,
  105,    4,  493,  207,  157,  553,  288,  555,  480,  105,
  480,   46,   77,  402,  105,  324,   42,   42,  242,  403,
   42,  622,  105,  380,  623,  581,  122,   88,  480,  228,
   58,   51,  123,  205,  105,   58,   49,  242,  586,  368,
  266,  410,  307,  266,  480,  573,  207,  376,  228,   58,
   47,   58,  775,   58,    5,  777,  105,  105,   55,  105,
  323,   58,  242,   58,  786,   58,  334,  334,   58,  549,
  334,  781,   51,  228,  266,   66,  593,  434,  338,  721,
   58,  480,  480,  444,   51,  480,  431,   48,  334,  205,
   67,  300,  262,   58,  807,  803,   51,   51,   72,  439,
  440,  242,  474,  443,  334,   58,  525,   74,  655,  480,
   51,  821,  228,  288,  480,  480,  444,  480,  480,  471,
  480,  759,  473,   75,  523,  503,  486,  507,  508,  510,
  760,  451,   58,  105,  205,   58,  480,   76,  242,  503,
  502,   58,   51,   70,   58,  474,   73,   58,   76,  228,
  641,  215,  526,   58,   76,  400,  558,  401,  723,  104,
  666,   49,   77,  713,   58,  532,  673,  113,  289,   58,
  290,  112,  724,  655,   54,  527,   44,  163,  524,  164,
  751,   58,    5,  655,  338,  338,  115,  452,  338,  575,
  171,  749,  655,  236,  535,   45,   55,  117,  563,  564,
  565,  545,  179,  286,  287,  104,   83,  503,  595,  181,
  126,  655,  242,  562,  315,  641,  584,  503,  585,  503,
   46,  142,  603,  228,  480,  641,  480,  118,  104,   54,
  480,  595,  577,  251,  641,  280,  183,  791,  582,  402,
  281,  143,  184,   58,  280,  403,  798,  583,   51,  281,
  800,   51,  570,  641,  144,   58,  590,  145,  191,   47,
  595,  251,  251,  251,  146,  739,  670,  813,  452,  674,
  676,  674,  251,   83,   55,  809,  755,  811,  266,  606,
  789,  825,   51,  452,  153,  192,  193,  694,  613,  162,
  452,  452,  194,  452,  452,  655,   48,  655,  253,  503,
  503,  768,  258,  672,  248,  260,  659,  660,  661,  341,
  662,  341,  444,  444,  665,  365,  321,   55,  323,  365,
  706,  315,  365,  365,  365,  298,  299,  365,  365,  365,
  261,  449,  365,   55,  300,  301,  250,  641,  543,  641,
  730,  731,  732,  291,  733,  734,  735,  736,  737,  738,
  365,  263,  365,  104,   78,  480,  750,  262,  444,  753,
  714,  814,  272,  815,  250,  250,  250,  273,  574,  104,
   49,  480,  104,  104,  274,  250,  341,  342,  449,  449,
  104,  275,  104,  104,  533,  743,  746,  453,  334,  540,
  334,  278,  104,  589,  334,  104,  104,  104,  756,  104,
  104,  395,  104,  799,  279,  444,  444,  670,  280,  104,
  104,  281,  449,  567,  104,  604,  207,  163,  284,  104,
  344,  345,  104,  104,  104,  355,  356,  449,  288,  104,
  764,  292,  293,  294,  449,  449,  104,  449,  449,  444,
  285,  104,  317,  598,  587,  588,  305,  306,  104,  295,
  104,  296,  778,  297,  444,  794,  783,  784,  318,  207,
  320,  444,  444,  104,  444,  444,  360,  372,  453,  792,
  104,  104,  616,  104,  480,  207,  384,  386,  620,  769,
  770,  771,  395,  453,  624,  385,   51,  805,  392,  393,
  453,  453,  394,  453,  453,  395,  398,  104,  104,  407,
  104,  409,  408,    1,  395,  395,    1,  395,  395,  816,
  770,  817,  417,  826,  827,    1,    1,    1,    1,    1,
    1,  419,    1,    1,    1,    1,  424,    1,    1,    1,
    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,
  427,    1,  429,    1,    1,    1,    1,  104,    1,  442,
    1,    1,    1,  450,    1,  469,  470,    1,  488,    1,
    1,    1,  515,    1,    1,    1,    1,   68,    1,  517,
    1,  487,   68,   75,  104,    1,    1,  519,   68,  531,
  536,    1,  268,    1,  539,    1,    1,  541,    1,    1,
    1,    1,  557,  548,    1,    1,    1,    1,    1,  571,
  566,    1,  591,  311,    1,    1,    1,    1,  600,    1,
    1,    1,    1,    1,    1,  619,  663,    1,  609,    1,
  669,    1,    1,    1,    1,    1,  693,  534,  104,  695,
  544,    1,    1,    1,    1,    1,  697,    1,  719,    1,
    1,  392,  480,  392,  104,  416,  703,  104,  104,  704,
    1,  420,  354,  480,  354,  707,    1,  104,  104,  708,
    1,  709,  722,    1,    1,  164,    1,  104,  710,  725,
  104,  711,  104,  357,  480,  357,   59,  104,  346,  347,
  348,  349,  350,  351,  352,  712,  729,  742,   59,  104,
   59,  420,   59,  752,  754,    1,  762,    1,  104,    1,
  763,    1,  104,    1,   59,    1,   28,   59,   59,    1,
  128,  104,  765,  489,  767,  104,   59,   59,   59,   28,
  741,  740,  773,  129,  774,  104,  776,   59,  779,   28,
   59,  788,   59,  130,  785,  801,   59,   59,   59,  806,
  797,  808,  810,  516,  819,  104,  104,  104,  104,   59,
  812,   28,  820,  823,  822,  131,  104,  824,   59,   59,
   28,  104,  828,   59,  132,   26,  829,  480,   59,   59,
   14,   59,  300,  334,   59,   80,   82,   59,   59,   59,
   59,  480,   59,   59,   59,   59,   64,   59,   81,  387,
  182,  238,   55,  209,   59,   59,   59,  224,   64,  334,
   64,  358,   64,  104,   27,   59,   59,   28,   59,   34,
   62,  133,  120,  141,   64,  148,  357,   64,   64,  322,
  125,  325,  104,   59,  554,  418,   64,   64,   64,  254,
  667,  518,  468,  316,  415,  412,  605,   64,  491,   61,
   64,  747,   64,  668,  745,  782,   64,   64,   64,  478,
  479,  480,  481,  482,  483,  435,  438,  677,  702,   64,
  761,  787,  569,  818,  579,  547,  428,  561,   64,   64,
  339,  230,  340,   64,  334,    0,  343,    0,   64,   64,
    0,   64,   59,  230,   64,    0,    0,   64,   64,   64,
   64,    0,   64,   64,   64,   64,    0,   64,    0,  230,
    0,    0,  230,  230,   64,   64,   64,    0,    0,    0,
    0,  230,  230,  230,    0,   64,   64,    0,   64,    0,
  230,    0,  230,    0,    0,  230,    0,  230,    0,    0,
    0,  230,  230,   64,  320,    0,    0,    0,  320,    0,
  316,  320,  320,  320,  230,    0,  320,  320,  320,   64,
    0,  320,    0,  230,  230,  231,    0,    0,  230,    0,
    0,    0,    0,  230,  230,    0,  230,  231,    0,  320,
    0,  320,  230,  230,  230,    0,    0,  230,  230,  230,
  230,    0,  230,  231,    0,    0,  231,  231,    0,    0,
  230,  230,   64,    0,    0,  231,  231,  231,    0,    0,
  230,  230,    0,  230,  231,    0,  231,    0,    0,  231,
    0,  231,    0,    0,    0,  231,  231,    0,  230,    0,
    0,    0,    0,    0,    0,  681,  682,  683,  231,    0,
  684,  685,  686,    0,  230,  687,    0,  231,  231,  124,
    0,    0,  231,    0,    0,    0,    0,  231,  231,    0,
  231,  124,    0,  688,    0,  689,  231,  231,  231,    0,
    0,  231,  231,  231,  231,    0,  231,  124,    0,    0,
  124,  124,    0,    0,  231,  231,    0,  230,    0,  124,
  124,  124,    0,    0,  231,  231,    0,  231,  124,    0,
  124,    0,    0,  124,    0,  124,    0,    0,    0,  124,
  124,    0,  231,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  124,    0,    0,    0,    0,    0,  231,    0,
    0,  124,  124,  176,    0,    0,  124,    0,    0,    0,
    0,  124,  124,    0,  124,  176,    0,    0,    0,    0,
  124,  124,  124,    0,    0,  124,  124,  124,  124,    0,
  124,  176,    0,    0,  176,  176,    0,    0,  124,  124,
    0,  231,    0,  176,  176,  176,    0,    0,  124,  124,
    0,  124,  176,    0,  176,    0,    0,  176,    0,  176,
    0,    0,    0,  176,  176,    0,  124,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  176,    0,    0,    0,
    0,    0,  124,    0,    0,  176,  176,  160,    0,    0,
  176,    0,    0,    0,    0,  176,  176,    0,  176,  160,
    0,    0,    0,    0,  176,  176,  176,    0,    0,  176,
  176,  176,  176,    0,  176,  160,    0,    0,  160,  160,
    0,    0,  176,  176,    0,  124,    0,  160,  160,  160,
    0,    0,  176,  176,    0,  176,  160,    0,  160,    0,
    0,  160,    0,  160,    0,    0,    0,  160,  160,    0,
  176,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  160,    0,    0,    0,    0,    0,  176,    0,    0,  160,
  160,   99,    0,    0,  160,    0,    0,    0,    0,  160,
  160,    0,  160,   99,    0,    0,    0,    0,  160,  160,
  160,    0,    0,  160,  160,  160,  160,    0,  160,   99,
    0,    0,   99,   99,    0,    0,  160,  160,    0,  176,
    0,   99,   99,   99,    0,    0,  160,  160,    0,  160,
   99,    0,   99,    0,    0,   99,    0,   99,    0,    0,
    0,   99,   99,    0,  160,    0,    0,    0,    0,    0,
    0,    0,    0,    0,   99,    0,    0,    0,    0,    0,
  160,    0,    0,   99,   99,  188,    0,    0,   99,    0,
    0,    0,    0,   99,   99,    0,   99,  188,    0,    0,
    0,    0,   99,   99,   99,    0,    0,   99,   99,   99,
   99,    0,   99,  188,    0,    0,  188,  188,    0,    0,
   99,   99,    0,  160,    0,  188,  188,  188,    0,    0,
   99,   99,    0,   99,  188,    0,  188,    0,    0,  188,
    0,  188,    0,    0,    0,  188,  188,    0,   99,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  188,    0,
    0,    0,    0,    0,   99,    0,    0,  188,  188,    0,
    0,    0,  188,    0,    0,    0,    0,  188,  188,    0,
  188,    0,    0,    0,    0,    0,  188,  188,  188,    0,
    0,  188,  188,  188,  188,    0,  188,    0,    0,    0,
    0,    0,    0,    0,  188,  188,    0,   99,    0,    0,
    0,    0,  233,    0,  188,  188,    0,  188,    0,    0,
    0,  233,  233,    0,  233,    0,  233,    0,    0,  233,
  233,  233,  188,  233,  233,  233,  233,    0,  233,  233,
  233,  233,  233,  233,  233,  233,    0,  233,  188,    0,
    0,  233,  233,    0,  233,    0,    0,    0,  233,    0,
    0,    0,    0,    0,    0,  233,  233,    0,    0,    0,
    0,    0,  233,    0,  233,    0,  233,    0,    0,    0,
  450,  233,   44,  163,    0,  164,    0,  233,    0,  233,
    0,  188,    0,    0,  233,  233,  171,  233,    0,    0,
  233,   45,    0,  233,  233,    0,    0,    0,  179,    0,
    0,    0,    0,    0,    0,  181,    0,    0,    0,  233,
    0,    0,    0,  233,    0,  233,   46,  450,  450,    0,
  233,  233,    0,    0,    0,    0,    0,  233,    0,    0,
  233,  233,  183,  233,    0,  233,  233,    0,  184,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  450,  233,    0,  191,   47,  233,    0,    0,  233,
  233,    0,    0,    0,    0,    0,  450,    0,    0,    0,
    0,    0,    0,  450,  450,    0,  450,  450,    0,    0,
    0,  192,  193,    0,  451,    0,    0,    0,  194,    0,
    0,  233,   48,  233,  255,  233,    0,  233,    0,  233,
    0,  233,    0,  255,  255,  233,  255,    0,  255,    0,
    0,  255,  255,  255,    0,  255,  255,  255,  255,    0,
  255,  255,  255,  255,  255,  255,  255,  255,    0,  255,
    0,  451,  451,  255,  255,    0,  255,    0,    0,    0,
  255,    0,    0,    0,    0,    0,    0,  255,  255,    0,
    0,    0,    0,    0,  255,    0,  255,    0,  255,    0,
    0,    0,    0,  255,    0,  451,   49,    0,    0,  255,
    0,  255,    0,    0,    0,    0,  255,  255,    0,  255,
  451,    0,  255,    0,    0,  255,  255,  451,  451,    0,
  451,  451,    0,    0,   58,    0,    0,    0,    0,    0,
    0,  255,    0,  195,    0,  255,    0,  255,    0,    0,
    0,    0,  255,  255,   58,   58,    0,    0,    0,  255,
    0,    0,  255,  255,    0,  255,    0,  255,  255,    0,
    0,    0,    0,    0,    0,    0,    0,   58,    0,    0,
    0,    0,    0,    0,  255,    0,   58,   58,  255,    0,
    0,  255,  255,    0,    0,   58,   58,   58,    0,    0,
   58,   58,   58,   58,    0,   58,    0,    0,    0,    0,
    0,    0,    0,   58,   58,    0,    0,    0,    0,    0,
    0,    0,    0,  255,   58,  255,  219,  255,    0,  255,
    0,  255,    0,  255,    0,  219,  219,  255,  219,    0,
  219,   58,    0,  219,  219,  219,    0,  219,  219,  219,
  219,    0,  219,  219,  219,  219,  219,  219,  219,  219,
    0,  219,    0,    0,    0,  219,  219,    0,  219,    0,
    0,  128,  219,    0,    0,  452,    0,    0,    0,  219,
  219,    0,    0,    0,  129,  453,  219,    0,  219,    0,
  219,    0,    0,    0,  130,  219,    0,  454,    0,    0,
    0,  219,    0,  219,  455,    0,    0,    0,  219,  219,
    0,  219,    0,    0,  219,    0,  131,  219,  219,    0,
    0,    0,    0,    0,    0,  132,   58,    0,    0,    0,
    0,    0,    0,  219,    0,  196,    0,  219,    0,  219,
    0,    0,    0,    0,  219,  219,   58,   58,    0,    0,
  492,  219,    0,    0,  219,  219,    0,  219,    0,  219,
  219,    0,    0,    0,    0,    0,    0,    0,    0,   58,
    0,    0,  133,  456,    0,    0,  219,    0,   58,   58,
  219,    0,    0,  219,  219,    0,    0,   58,   58,   58,
    0,    0,   58,   58,   58,   58,    0,   58,    0,    0,
    0,    0,    0,    0,    0,   58,   58,    0,    0,    0,
    0,    0,    0,    0,    0,  219,   58,  219,  222,  219,
    0,  219,    0,  219,    0,  219,    0,  222,  222,  219,
  222,    0,  222,   58,    0,  222,  222,  222,    0,  222,
  222,  222,  222,    0,  222,  222,  222,  222,  222,  222,
  222,  222,    0,  222,    0,    0,    0,  222,  222,    0,
  222,    0,    0,  128,  222,    0,    0,  452,    0,    0,
    0,  222,  222,    0,    0,    0,  129,  453,  222,    0,
  222,    0,  222,    0,    0,    0,  130,  222,    0,  454,
    0,    0,    0,  222,    0,  222,  455,    0,    0,    0,
  222,  222,    0,  222,    0,    0,  222,    0,  131,  222,
  222,    0,    0,    0,    0,    0,    0,  132,   91,    0,
    0,    0,    0,    0,    0,  222,    0,    0,    0,  222,
    0,  222,    0,    0,    0,    0,  222,  222,   92,    0,
    0,    0,    0,  222,    0,    0,  222,  222,    0,  222,
    0,  222,  222,    0,    0,    0,    0,    0,    0,    0,
    0,   93,    0,    0,  133,  456,    0,    0,  222,    0,
   94,   95,  222,    0,    0,  222,  222,    0,    0,   96,
   97,   98,    0,    0,   99,    0,  100,  101,    0,  102,
    0,    0,    0,    0,    0,    0,    0,  103,  104,    0,
    0,    0,    0,    0,    0,    0,    0,  222,    0,  222,
  625,  222,    0,  222,    0,  222,    0,  222,    0,   44,
  163,  222,  164,    0,  165,  105,    0,  166,  167,  168,
    0,  169,  170,  171,  172,    0,  173,  174,   45,  626,
  175,  176,  177,  178,    0,  179,    0,    0,    0,  627,
  180,    0,  181,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,   46,   68,    0,    0,    0,    0,    0,
  182,    0,  628,    0,  629,    0,    0,    0,    0,  183,
    0,    0,    0,    0,    0,  184,    0,  185,    0,    0,
    0,    0,  186,  187,    0,  188,    0,    0,  190,    0,
    0,  191,   47,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  630,    0,    0,
    0,  631,    0,  632,    0,    0,    0,    0,  192,  193,
    0,    0,    0,    0,    0,  194,    0,    0,  633,   48,
    0,  634,    0,   69,  635,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  196,    0,    0,  326,    0,    0,    0,  197,  198,    0,
    0,   44,  163,  327,  164,    0,  165,    0,    0,  166,
  167,  168,    0,  169,  170,  171,  172,    0,  173,  174,
   45,    0,  175,  176,  177,  178,    0,  179,    0,  199,
    0,  200,  180,  201,  181,  202,    0,  203,    0,  204,
    0,    0,    0,   49,    0,   46,   68,    0,    0,    0,
    0,    0,  182,    0,    0,    0,    0,    0,    0,    0,
    0,  183,    0,    0,    0,    0,    0,  184,    0,  185,
    0,    0,    0,    0,  186,  187,    0,  188,    0,  189,
  190,    0,    0,  191,   47,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  192,  193,    0,    0,    0,    0,    0,  194,    0,    0,
    0,   48,    0,    0,    0,   69,    0,  195,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  196,    0,    0,  705,    0,    0,    0,  197,
  198,    0,    0,   44,  163,    0,  164,    0,  165,    0,
    0,  166,  167,  168,    0,  169,  170,  171,  172,    0,
  173,  174,   45,    0,  175,  176,  177,  178,    0,  179,
    0,  199,    0,  200,  180,  201,  181,  202,    0,  203,
    0,  204,    0,    0,    0,   49,    0,   46,   68,    0,
    0,    0,    0,    0,  182,    0,    0,    0,    0,    0,
    0,    0,    0,  183,    0,    0,    0,    0,    0,  184,
    0,  185,    0,    0,    0,    0,  186,  187,    0,  188,
    0,  189,  190,    0,    0,  191,   47,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  192,  193,    0,    0,    0,    0,    0,  194,
    0,    0,    0,   48,    0,    0,    0,   69,    0,  195,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  196,    0,    0,    0,    0,    0,
    0,  197,  198,    0,    0,   44,  163,    0,  164,    0,
  165,    0,    0,  166,  167,  168,    0,  169,  170,  171,
  172,    0,  173,  174,   45,    0,  175,  176,  177,  178,
    0,  179,    0,  199,    0,  200,  180,  201,  181,  202,
    0,  203,    0,  204,    0,    0,    0,   49,    0,   46,
   68,    0,    0,    0,    0,    0,  182,    0,    0,    0,
    0,    0,    0,    0,    0,  183,    0,    0,    0,    0,
    0,  184,    0,  185,    0,    0,    0,    0,  186,  187,
    0,  188,    0,  189,  190,    0,    0,  191,   47,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  192,  193,    0,    0,    0,    0,
    0,  194,    0,    0,    0,   48,    0,    0,    0,   69,
    0,  195,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  196,  594,  610,    0,
    0,    0,    0,  197,  198,    0,    0,   44,  163,    0,
  164,    0,  165,    0,    0,  166,  167,  168,    0,  169,
  170,  171,  172,    0,  173,  174,   45,    0,  175,  176,
  177,  178,    0,  179,    0,  199,    0,  200,  180,  201,
  181,  202,    0,  203,    0,  204,    0,    0,    0,   49,
    0,   46,   68,    0,    0,    0,    0,    0,  182,    0,
    0,    0,    0,    0,    0,    0,    0,  183,    0,    0,
    0,    0,    0,  184,    0,  185,    0,    0,    0,    0,
  186,  187,    0,  188,    0,  189,  190,    0,    0,  191,
   47,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  192,  193,    0,    0,
    0,    0,    0,  194,    0,    0,    0,   48,    0,    0,
    0,   69,    0,  195,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  196,  594,
    0,    0,    0,    0,    0,  197,  198,    0,    0,   44,
  163,    0,  164,    0,  165,    0,    0,  166,  167,  168,
    0,  169,  170,  171,  172,    0,  173,  174,   45,    0,
  175,  176,  177,  178,    0,  179,    0,  199,    0,  200,
  180,  201,  181,  202,    0,  203,    0,  204,    0,    0,
    0,   49,    0,   46,   68,    0,    0,    0,    0,    0,
  182,    0,    0,    0,    0,    0,    0,    0,    0,  183,
    0,    0,    0,    0,    0,  184,    0,  185,    0,    0,
    0,    0,  186,  187,    0,  188,    0,  189,  190,    0,
    0,  191,   47,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  192,  193,
    0,    0,    0,    0,    0,  194,    0,    0,    0,   48,
    0,    0,    0,   69,    0,  195,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  196,    0,    0,    0,    0,    0,   90,  197,  198,    0,
    0,   44,  163,    0,  164,    0,  165,    0,    0,  166,
  167,  168,    0,  169,  170,  171,  172,    0,  173,  174,
   45,    0,  175,  176,  177,  178,    0,  179,    0,  199,
    0,  200,  180,  201,  181,  202,   91,  203,    0,  204,
    0,    0,    0,   49,    0,   46,   68,    0,    0,    0,
    0,    0,  182,    0,    0,    0,   92,    0,    0,    0,
    0,  183,    0,    0,    0,    0,    0,  184,    0,  185,
    0,    0,    0,    0,  186,  187,    0,  188,    0,   93,
  190,    0,    0,  191,   47,    0,    0,    0,   94,   95,
    0,    0,    0,    0,    0,    0,    0,   96,   97,   98,
    0,    0,   99,    0,  100,  101,    0,  102,    0,    0,
  192,  193,    0,    0,    0,  103,  104,  194,    0,    0,
    0,   48,    0,    0,    0,   69,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  196,  105,    0,    0,    0,    0,    0,  197,
  198,    0,    0,   44,  163,    0,  164,    0,  165,    0,
    0,  166,  167,  168,    0,  169,  170,  171,  172,    0,
  173,  174,   45,    0,  175,  176,  177,  178,    0,  179,
    0,  199,    0,  200,  180,  201,  181,  202,    0,  203,
    0,  204,    0,    0,    0,   49,    0,   46,   68,    0,
    0,    0,    0,    0,  182,   58,    0,    0,    0,    0,
    0,    0,    0,  183,    0,    0,    0,    0,    0,  184,
    0,  185,    0,    0,    0,    0,  186,  187,    0,  188,
    0,    0,  190,   58,    0,  191,   47,    0,    0,    0,
    0,    0,    0,    0,    0,   58,   58,    0,    0,    0,
    0,    0,    0,    0,    0,    0,   58,    0,    0,    0,
    0,    0,  192,  193,    0,   58,    0,    0,    0,  194,
    0,    0,    0,   48,    0,    0,    0,   69,   58,    0,
    0,    0,    0,    0,    0,    0,    0,   58,   58,    0,
    0,    0,   58,    0,  196,    0,    0,   58,   58,    0,
    0,    0,    0,    0,    0,    0,   58,   58,   58,    0,
    0,   58,    0,   58,   58,    0,   58,    0,    0,    0,
    0,    0,    0,    0,   58,   58,   97,    0,    0,    0,
    0,    0,    0,  199,   58,  200,    0,  201,    0,  202,
    0,  203,   97,  204,    0,   97,   97,   49,    0,    0,
    0,    0,   58,    0,   97,   97,   97,    0,    0,    0,
    0,    0,    0,   97,    0,   97,    0,    0,   97,    0,
   97,    0,    0,    0,   97,   97,    0,    0,    0,    0,
    0,    0,   97,    0,    0,   97,    0,   97,    0,    0,
    0,    0,    0,    0,    0,    0,   97,   97,    0,    0,
    0,    0,    0,    0,    0,    0,   97,   97,  100,   97,
    0,    0,    0,    0,    0,   97,   97,   97,    0,    0,
   97,   97,   97,   97,  100,   97,    0,  100,  100,    0,
    0,    0,    0,   97,   97,    0,  100,  100,  100,    0,
    0,    0,    0,   97,   97,  100,   97,  100,    0,    0,
  100,    0,  100,    0,    0,    0,  100,  100,    0,    0,
    0,   97,    0,    0,  100,    0,    0,    0,    0,  100,
    0,    0,    0,    0,    0,    0,    0,   97,  100,  100,
    0,    0,    0,    0,    0,    0,    0,    0,  100,  100,
    0,  100,    0,  101,    0,    0,    0,  100,  100,  100,
    0,    0,  100,  100,  100,  100,    0,  100,    0,  101,
    0,    0,  101,  101,    0,  100,  100,    0,    0,    0,
   97,  101,  101,  101,    0,  100,  100,    0,  100,    0,
  101,    0,  101,    0,    0,  101,    0,  101,    0,    0,
    0,  101,  101,  100,    0,    0,    0,    0,    0,  101,
    0,    0,    0,    0,  101,    0,    0,    0,    0,  100,
    0,    0,    0,  101,  101,    0,    0,    0,    0,    0,
    0,    0,    0,  101,  101,  123,  101,    0,    0,    0,
    0,    0,  101,  101,  101,    0,    0,  101,  101,  101,
  101,  123,  101,    0,  123,  123,    0,    0,    0,    0,
  101,  101,  100,  123,  123,  123,    0,    0,    0,    0,
  101,  101,  123,  101,  123,    0,    0,  123,    0,  123,
    0,    0,    0,  123,  123,    0,    0,    0,  101,    0,
    0,    0,    0,    0,    0,    0,  123,    0,    0,    0,
    0,    0,    0,    0,  101,  123,  123,    0,    0,    0,
    0,    0,    0,    0,    0,  123,  123,    0,  123,    0,
  102,    0,    0,    0,  123,  123,  123,    0,    0,  123,
  123,  123,  123,    0,  123,    0,  102,    0,    0,  102,
  102,    0,  123,  123,    0,    0,    0,  101,  102,  102,
  102,    0,  123,  123,    0,  123,    0,  102,    0,  102,
    0,    0,  102,    0,  102,    0,    0,    0,  102,  102,
  123,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  102,    0,    0,    0,    0,  123,    0,    0,    0,
  102,  102,    0,    0,    0,    0,    0,    0,    0,    0,
  102,  102,  211,  102,    0,    0,    0,    0,    0,  102,
  102,  102,    0,    0,  102,  102,  102,  102,  211,  102,
    0,  211,  211,    0,    0,    0,    0,  102,  102,  123,
  211,  211,  211,    0,    0,    0,    0,  102,  102,  211,
  102,  211,  211,    0,  211,    0,  211,    0,    0,    0,
  211,  211,    0,    0,    0,  102,    0,    0,    0,    0,
    0,    0,    0,  211,    0,    0,    0,    0,    0,    0,
    0,  102,  211,  211,    0,    0,    0,    0,    0,    0,
    0,    0,  211,  211,    0,  211,    0,    0,    0,    0,
    0,  211,  211,  211,    0,    0,  211,    0,  211,  211,
    0,  211,    0,    0,    0,    0,    0,    0,    0,  211,
  211,    0,  212,    0,  102,    0,    0,    0,    0,  211,
  211,    0,  211,    0,    0,    0,    0,    0,  212,    0,
    0,  212,  212,    0,    0,    0,    0,  211,    0,    0,
  212,  212,  212,    0,    0,    0,    0,    0,  211,  212,
    0,  212,  212,    0,  212,    0,  212,    0,    0,    0,
  212,  212,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  212,    0,    0,    0,    0,    0,    0,
    0,    0,  212,  212,    0,    0,    0,    0,    0,    0,
    0,    0,  212,  212,  103,  212,  211,    0,    0,    0,
    0,  212,  212,  212,    0,    0,  212,    0,  212,  212,
  103,  212,    0,  103,  103,    0,    0,    0,    0,  212,
  212,    0,  103,  103,  103,    0,    0,    0,    0,  212,
  212,  103,  212,  103,    0,    0,  103,    0,  103,    0,
    0,    0,  103,  103,    0,    0,    0,  212,    0,    0,
    0,    0,    0,    0,    0,  103,    0,    0,  212,    0,
    0,    0,    0,    0,  103,  103,    0,    0,    0,    0,
    0,    0,    0,    0,  103,  103,    0,  103,    0,  138,
    0,    0,    0,  103,  103,  103,    0,    0,  103,  103,
  103,  103,    0,  103,    0,  138,    0,    0,  138,  138,
    0,  103,  103,    0,    0,    0,  212,  138,  138,  138,
    0,  103,  103,    0,  103,    0,  138,    0,  138,    0,
    0,  138,    0,  138,    0,    0,    0,  138,  138,  103,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  138,    0,    0,    0,    0,  103,    0,    0,    0,  138,
  138,    0,    0,    0,    0,    0,    0,    0,    0,  138,
  138,  227,  138,    0,    0,    0,    0,    0,  138,  138,
  138,    0,    0,  138,  138,  138,  138,  227,  138,    0,
  227,  227,    0,    0,    0,    0,  138,  138,  103,  227,
  227,  227,    0,    0,    0,    0,  138,  138,  227,  138,
  227,    0,    0,  227,    0,  227,    0,    0,    0,  227,
  227,    0,    0,    0,  138,    0,    0,    0,    0,    0,
    0,    0,  227,    0,    0,    0,    0,    0,    0,    0,
  138,  227,  227,    0,    0,    0,    0,    0,    0,    0,
    0,  227,  227,    0,  227,    0,  174,    0,    0,    0,
  227,  227,  227,    0,    0,  227,  227,  227,  227,    0,
  227,    0,  174,    0,    0,  174,  174,    0,  227,  227,
    0,    0,    0,  138,  174,  174,  174,    0,  227,  227,
    0,  227,    0,  174,    0,  174,    0,    0,  174,    0,
  174,    0,    0,    0,  174,  174,  227,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  174,    0,    0,
    0,    0,  227,    0,    0,    0,  174,  174,    0,    0,
    0,    0,    0,    0,    0,    0,  174,  174,  235,  174,
    0,    0,    0,    0,    0,  174,  174,  174,    0,    0,
  174,  174,  174,  174,  235,  174,    0,  235,  235,    0,
    0,    0,    0,  174,  174,  227,  235,  235,  235,    0,
    0,    0,    0,  174,  174,  235,  174,  235,    0,    0,
  235,    0,  235,    0,    0,    0,  235,  235,    0,    0,
    0,  174,    0,    0,    0,    0,    0,    0,    0,  235,
    0,    0,    0,    0,    0,    0,    0,  174,  235,  235,
    0,    0,    0,    0,    0,    0,    0,    0,  235,  235,
    0,  235,    0,  206,    0,    0,    0,  235,  235,  235,
    0,    0,  235,  235,  235,  235,    0,  235,    0,  206,
    0,    0,  206,  206,    0,  235,  235,    0,    0,    0,
  174,  206,  206,  206,    0,  235,  235,    0,  235,    0,
  206,    0,  206,    0,    0,  206,    0,  206,    0,    0,
    0,  206,  206,  235,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  206,    0,    0,    0,    0,  235,
    0,    0,    0,  206,  206,    0,    0,    0,    0,    0,
    0,    0,    0,  206,  206,  154,  206,    0,    0,    0,
    0,    0,  206,  206,  206,    0,    0,  206,  206,  206,
  206,  154,  206,    0,  154,  154,    0,    0,    0,    0,
  206,  206,  235,  154,  154,  154,    0,    0,    0,    0,
  206,  206,  154,  206,  154,    0,    0,  154,    0,  154,
    0,    0,    0,  154,  154,    0,    0,    0,  206,    0,
    0,    0,    0,    0,    0,    0,  154,    0,    0,    0,
    0,    0,    0,    0,  206,  154,  154,    0,    0,    0,
    0,    0,    0,    0,    0,  154,  154,    0,  154,    0,
  157,    0,    0,    0,  154,  154,  154,    0,    0,  154,
  154,  154,  154,    0,  154,    0,  157,    0,    0,  157,
  157,    0,  154,  154,    0,    0,    0,  206,  157,  157,
  157,    0,  154,  154,    0,  154,    0,  157,    0,  157,
    0,    0,  157,    0,  157,    0,    0,    0,  157,  157,
  154,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  157,    0,    0,    0,    0,  154,    0,    0,    0,
  157,  157,    0,    0,    0,    0,    0,    0,    0,    0,
  157,  157,   58,  157,    0,    0,    0,    0,    0,  157,
  157,  157,    0,    0,  157,  157,  157,  157,   58,  157,
    0,   58,   58,    0,    0,    0,    0,  157,  157,  154,
   58,   58,   58,    0,    0,    0,    0,  157,  157,  125,
  157,   58,    0,    0,   58,    0,   58,    0,    0,    0,
   58,   58,    0,    0,    0,  157,    0,    0,    0,    0,
    0,    0,    0,   58,    0,    0,    0,    0,    0,    0,
    0,  157,   58,   58,    0,    0,    0,    0,    0,    0,
    0,    0,   58,   58,    0,   58,    0,   58,    0,    0,
    0,   58,   58,   58,    0,    0,   58,   58,   58,   58,
    0,   58,    0,   58,    0,    0,   58,   58,    0,   58,
   58,    0,    0,    0,  157,   58,   58,   58,    0,   58,
   58,    0,   58,    0,  126,    0,   58,    0,    0,   58,
    0,   58,    0,    0,    0,   58,   58,   58,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,   58,    0,
    0,    0,    0,    0,    0,    0,    0,   58,   58,    0,
    0,    0,    0,    0,    0,    0,    0,   58,   58,  210,
   58,    0,    0,    0,    0,    0,   58,   58,   58,    0,
    0,   58,   58,   58,   58,  210,   58,    0,  210,  210,
    0,    0,    0,    0,   58,   58,   58,  210,  210,  210,
    0,    0,    0,    0,   58,   58,  210,   58,  210,  210,
    0,  210,    0,  210,    0,    0,    0,  210,  210,    0,
    0,    0,   58,    0,    0,    0,    0,    0,    0,    0,
  210,    0,    0,    0,    0,    0,    0,    0,    0,  210,
  210,    0,    0,    0,    0,    0,    0,    0,    0,  210,
  210,    0,  210,    0,   58,    0,    0,    0,  210,  210,
  210,    0,    0,  210,    0,  210,  210,    0,  210,    0,
   58,    0,    0,   58,   58,    0,  210,  210,    0,    0,
    0,   58,   58,   58,   58,    0,  210,  210,    0,  210,
    0,  139,    0,   58,    0,    0,   58,    0,   58,    0,
    0,    0,   58,   58,  210,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,   58,    0,    0,    0,    0,
    0,    0,    0,    0,   58,   58,    0,    0,    0,    0,
    0,    0,    0,    0,   58,   58,   58,   58,    0,    0,
    0,    0,    0,   58,   58,   58,    0,    0,   58,   58,
   58,   58,   58,   58,    0,   58,   58,    0,    0,    0,
    0,   58,   58,  210,   58,   58,   58,    0,    0,    0,
    0,   58,   58,  140,   58,   58,    0,    0,   58,    0,
   58,    0,    0,    0,   58,   58,    0,    0,    0,   58,
    0,    0,    0,    0,    0,    0,    0,   58,    0,    0,
    0,    0,    0,    0,    0,    0,   58,   58,    0,    0,
    0,    0,    0,  470,  470,    0,   58,   58,    0,   58,
    0,  209,    0,    0,    0,   58,   58,   58,    0,    0,
   58,   58,   58,   58,    0,   58,    0,  209,    0,    0,
  209,  209,    0,   58,   58,    0,    0,    0,   58,  209,
  209,  209,    0,   58,   58,    0,   58,    0,  209,  470,
  209,    0,    0,  209,    0,  209,    0,    0,    0,  209,
  209,   58,    0,    0,    0,  470,    0,    0,    0,    0,
    0,  470,  209,    0,    0,    0,  480,  480,  470,    0,
    0,  209,  209,    0,    0,    0,    0,    0,    0,    0,
    0,  209,  209,    0,  209,    0,  470,  470,    0,    0,
  209,  209,  209,    0,    0,  209,    0,  209,  209,    0,
  209,    0,    0,    0,    0,    0,    0,    0,  209,  209,
   58,    0,  480,    0,    0,    0,  385,  385,  209,  209,
  470,  209,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  480,  470,  209,    0,    0,    0,
  470,  480,  470,  470,    0,  470,  470,  470,  470,  470,
  470,  470,  470,  470,  470,    0,  470,  470,    0,  480,
  480,    0,  385,    0,    0,    0,    0,  470,    0,  470,
    0,  470,    0,    0,    0,  470,    0,  470,    0,    0,
    0,    0,    0,    0,  385,    0,    0,    0,    0,  362,
  362,  385,    0,  480,    0,  209,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  480,  385,
  385,    0,    0,  480,    0,  480,  480,  480,  480,  480,
  480,  480,  480,  480,  480,  480,  480,  480,    0,  480,
  480,    0,    0,    0,    0,  362,    0,    0,    0,    0,
  480,    0,  480,  385,  480,    0,    0,    0,  480,    0,
  480,    0,    0,    0,    0,    0,    0,  362,  385,    0,
    0,    0,    0,  384,  362,  385,  385,  384,  385,  385,
  385,  385,  385,  385,  385,  385,  385,  385,    0,  385,
  385,    0,  362,  362,    0,    0,  319,  319,    0,    0,
  385,    0,  385,    0,  385,    0,    0,    0,  385,    0,
  385,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  362,    0,    0,    0,
    0,    0,    0,    0,  415,  415,    0,    0,    0,    0,
    0,  362,  319,    0,    0,    0,  362,    0,  362,  362,
    0,  362,  362,  362,  362,  362,  362,  362,  362,  362,
  362,    0,  362,  362,  319,    0,    0,    0,    0,    0,
    0,  319,    0,  362,    0,  362,    0,  362,    0,    0,
  415,  362,    0,  362,    0,    0,    0,    0,    0,  319,
  319,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,  415,    0,    0,    0,    0,    0,    0,  415,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  319,    0,    0,    0,  415,  415,    0,
    0,  416,  416,    0,    0,    0,    0,    0,  319,    0,
    0,    0,    0,    0,    0,  319,  319,  319,  319,  319,
  319,  319,  319,  319,  319,  319,  319,  319,    0,  319,
  319,  415,    0,    0,    0,    0,    0,    0,    0,    0,
  319,    0,  319,    0,  319,    0,  415,  416,  319,    0,
  319,  417,  417,  415,  415,    0,  415,  415,  415,  415,
  415,  415,  415,  415,  415,  415,    0,  415,  415,  416,
    0,    0,    0,    0,    0,    0,  416,    0,  415,    0,
  415,    0,  415,    0,    0,    0,  415,    0,  415,    0,
    0,    0,    0,    0,  416,  416,    0,  417,   44,   44,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,  417,
    0,    0,    0,    0,    0,    0,  417,    0,  416,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,  416,  417,  417,    0,    0,  423,  423,
  416,  416,    0,  416,  416,  416,  416,  416,  416,  416,
  416,  416,  416,    0,  416,  416,   44,    0,    0,    0,
    0,    0,    0,   44,    0,  416,    0,  416,  417,  416,
    0,    0,    0,  416,    0,  416,    0,    0,    0,    0,
    0,   44,   44,  417,  423,    0,    0,    0,  424,  424,
  417,  417,    0,  417,  417,  417,  417,  417,  417,  417,
  417,  417,    0,    0,  417,  417,  423,    0,    0,    0,
    0,    0,    0,  423,    0,  417,    0,  417,    0,  417,
    0,    0,    0,  417,    0,  417,    0,    0,    0,    0,
   44,  423,  423,    0,  424,   44,    0,    0,   44,   44,
   44,    0,   44,   44,   44,   44,   44,   44,   44,   44,
    0,   44,   44,    0,    0,    0,  424,    0,    0,    0,
  425,  425,   44,  424,   44,  423,   44,    0,    0,    0,
   44,    0,   44,    0,    0,    0,    0,    0,    0,    0,
  423,  424,  424,    0,    0,    0,    0,  423,  423,    0,
  423,  423,  423,  423,  423,  423,  423,    0,    0,    0,
    0,  423,  423,  426,  426,    0,  425,    0,    0,    0,
    0,    0,  423,    0,  423,  424,  423,    0,    0,    0,
  423,    0,  423,    0,    0,    0,    0,    0,  425,    0,
  424,    0,    0,    0,    0,  425,    0,  424,  424,    0,
  424,  424,  424,  424,  424,  424,  424,    0,    0,  426,
    0,  424,  424,  425,  425,  427,  427,    0,    0,    0,
    0,    0,  424,    0,  424,    0,  424,    0,    0,    0,
  424,  426,  424,    0,    0,    0,    0,    0,  426,    0,
    0,    0,    0,    0,    0,    0,    0,  425,    0,    0,
    0,    0,    0,    0,    0,    0,  426,  426,    0,    0,
    0,  427,  425,    0,    0,  428,  428,    0,    0,  425,
  425,    0,  425,  425,  425,  425,  425,  425,  425,    0,
    0,    0,    0,  427,  425,    0,    0,    0,    0,    0,
  426,    0,    0,    0,  425,    0,  425,    0,  425,    0,
    0,    0,  425,    0,  425,  426,    0,    0,  427,  427,
    0,  428,  426,  426,    0,  426,  426,  426,  426,  426,
  426,  426,    0,    0,    0,    0,    0,  426,    0,    0,
    0,    0,    0,  428,  429,  429,    0,  426,    0,  426,
    0,  426,  427,    0,    0,  426,    0,  426,    0,    0,
    0,    0,    0,    0,    0,    0,    0,  427,  428,  428,
    0,    0,    0,    0,  427,  427,    0,  427,  427,  427,
  427,  427,  427,  427,    0,    0,    0,    0,    0,  427,
  429,    0,    0,    0,  430,  430,    0,    0,    0,  427,
    0,  427,  428,  427,    0,    0,    0,  427,    0,  427,
    0,    0,  429,    0,    0,    0,    0,  428,    0,    0,
    0,    0,    0,    0,  428,  428,    0,  428,  428,  428,
  428,  428,  428,  428,    0,    0,    0,  429,  429,  428,
  430,    0,    0,    0,  431,  431,    0,    0,    0,  428,
    0,  428,    0,  428,    0,    0,    0,  428,    0,  428,
    0,    0,  430,    0,    0,    0,    0,    0,    0,    0,
    0,  429,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  429,  430,  430,    0,
  431,    0,    0,  429,  429,    0,  429,  429,  429,  429,
  429,  429,  429,    0,    0,    0,    0,    0,  429,    0,
    0,    0,  431,  432,  432,    0,    0,    0,  429,    0,
  429,  430,  429,    0,    0,    0,  429,    0,  429,    0,
    0,    0,    0,    0,    0,    0,  430,  431,  431,    0,
    0,    0,    0,  430,  430,    0,  430,  430,    0,    0,
  430,  430,  430,    0,    0,    0,    0,    0,  430,  432,
    0,    0,    0,  433,  433,    0,    0,    0,  430,    0,
  430,  431,  430,    0,    0,    0,  430,    0,  430,    0,
    0,  432,    0,    0,    0,    0,  431,    0,    0,    0,
    0,    0,    0,  431,  431,    0,  431,  431,    0,    0,
  431,  431,  431,    0,    0,    0,  432,  432,  431,  433,
    0,    0,    0,  434,  434,    0,    0,    0,  431,    0,
  431,    0,  431,    0,    0,    0,  431,    0,  431,    0,
    0,  433,    0,    0,    0,    0,    0,    0,    0,    0,
  432,    0,    0,    0,    0,    0,    0,    0,    0,    0,
  435,  435,    0,    0,    0,  432,  433,  433,    0,  434,
    0,    0,  432,  432,    0,  432,  432,    0,    0,  432,
  432,  432,    0,    0,    0,    0,    0,    0,    0,    0,
    0,  434,    0,    0,    0,  480,  480,  432,    0,  432,
  433,  432,    0,    0,    0,  432,  435,  432,    0,    0,
    0,    0,    0,    0,    0,  433,  434,  434,    0,    0,
    0,    0,  433,  433,    0,  433,  433,    0,  435,  433,
  433,  433,  442,  442,    0,    0,    0,    0,    0,    0,
    0,  480,    0,    0,    0,    0,    0,  433,    0,  433,
  434,  433,    0,  435,  435,  433,    0,  433,    0,    0,
    0,    0,    0,  480,    0,  434,    0,    0,    0,    0,
    0,    0,  434,  434,    0,  434,  434,    0,  442,  434,
  434,  434,  436,  436,    0,    0,    0,  435,  480,  480,
    0,    0,    0,    0,    0,    0,    0,  434,    0,  434,
  442,  434,  435,    0,    0,  434,    0,  434,    0,  435,
  435,    0,  435,  435,    0,    0,  435,  435,  435,  438,
  438,    0,  480,    0,    0,  442,  442,    0,  436,    0,
    0,    0,  439,  439,  435,    0,  435,  480,  435,    0,
    0,    0,    0,    0,  480,  480,    0,  480,  480,    0,
  436,  480,  480,  480,    0,    0,    0,    0,    0,  442,
    0,    0,    0,    0,    0,  438,    0,    0,    0,  480,
    0,  480,    0,  480,  442,  436,  436,    0,  439,    0,
    0,  442,  442,    0,  442,  442,    0,  438,  442,  442,
  442,    0,    0,    0,    0,    0,    0,    0,  440,  440,
  439,    0,    0,    0,    0,    0,  442,    0,  442,  436,
  442,    0,  438,  438,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,  436,  439,  439,    0,    0,    0,
    0,  436,  436,    0,  436,  436,    0,    0,  436,  436,
  436,  441,  441,    0,  440,    0,  438,    0,    0,    0,
    0,    0,    0,    0,    0,    0,  436,    0,  436,  439,
  436,  438,    0,    0,    0,    0,  440,    0,  438,  438,
    0,  438,  438,    0,  439,  438,  438,  438,    0,    0,
    0,  439,  439,    0,  439,  439,    0,  441,  439,  439,
  439,  440,  440,  438,    0,  438,    0,  438,    0,    0,
    0,    0,    0,    0,    0,    0,  439,    0,  439,  441,
  439,  437,  437,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,  440,    0,    0,    0,    0,
    0,    0,    0,    0,  441,  441,    0,    0,    0,    0,
  440,    0,    0,    0,    0,    0,    0,  440,  440,    0,
  440,  440,    0,    0,  440,  440,  440,  437,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  441,    0,
    0,    0,  440,    0,  440,    0,  440,    0,    0,  437,
    0,    0,    0,  441,    0,    0,    0,    0,    0,    0,
  441,  441,    0,  441,  441,    0,    0,  441,  441,  441,
    0,    0,    0,    0,  437,  437,    0,    0,    0,    0,
    0,   21,    0,    0,    0,  441,    0,  441,    0,  441,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,  437,   21,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,   21,   21,  437,    0,    0,    0,    0,    0,    0,
  437,  437,   21,  437,  437,    0,    0,  437,  437,  437,
    0,   21,    0,    0,    0,    0,    0,    0,    0,    0,
   21,    0,    0,   19,   21,  437,    0,  437,    0,  437,
    0,    0,    0,   21,   21,    0,    0,    0,   21,    0,
    0,    0,    0,   21,   21,    0,    0,    0,   21,    0,
    0,   19,   21,   21,   21,    0,    0,   21,    0,   21,
   21,    0,   21,   19,   19,    0,    0,    0,    0,    0,
   21,   21,    0,    0,   19,    0,    0,    0,    0,    0,
   21,    0,    0,   19,    0,    0,    0,    0,    0,    0,
    0,    0,   19,    0,    0,   20,   19,    0,   21,    0,
    0,    0,    0,    0,    0,   19,   19,    0,    0,    0,
   19,    0,    0,    0,   21,   19,   19,    0,    0,    0,
   19,    0,    0,   20,   19,   19,   19,    0,    0,   19,
    0,   19,   19,    0,   19,   20,   20,    0,    0,    0,
    0,    0,   19,   19,    0,    0,   20,    0,    0,    0,
    0,    0,   19,    0,    0,   20,    0,    0,    0,    0,
    0,    0,    0,    0,   20,    0,    0,    7,   20,    0,
   19,    0,    0,    0,    0,    0,    0,   20,   20,    0,
    0,    0,   20,    0,    0,    0,   19,   20,   20,    0,
    0,    0,   20,    0,    0,    7,   20,   20,   20,    0,
    0,   20,    0,   20,   20,    0,   20,    7,    7,    0,
    0,    0,    0,    0,   20,   20,    0,    0,    7,    0,
    0,    0,    0,    0,   20,    0,    0,    7,    0,    0,
    0,    0,    0,    0,    0,    0,    7,    0,    0,    8,
    7,    0,   20,    0,    0,    0,    0,    0,    0,    7,
    7,    0,    0,    0,    7,    0,    0,    0,   20,    7,
    7,    0,    0,    0,    0,    0,    0,    8,    7,    7,
    7,    0,    0,    7,    0,    7,    7,    0,    7,    8,
    8,    0,    0,    0,    0,    0,    7,    7,    0,    0,
    8,    0,    0,    0,    0,    0,    7,    0,    0,    8,
    0,    0,    0,    0,    0,    0,    0,    0,    8,    0,
    0,   50,    8,    0,    7,    0,    0,    0,    0,    0,
    0,    8,    8,    0,    0,    0,    8,    0,    0,    0,
    7,    8,    8,    0,    0,    0,    0,    0,    0,   50,
    8,    8,    8,    0,    0,    8,    0,    8,    8,    0,
    8,   50,   50,    0,    0,    0,    0,    0,    8,    8,
    0,    0,   50,    0,    0,    0,    0,    0,    8,    0,
    0,   50,    0,    0,    0,    0,    0,    0,    0,    0,
   50,    0,    0,   89,   50,    0,    8,    0,    0,    0,
    0,    0,    0,   50,   50,    0,    0,    0,   50,    0,
    0,    0,    8,   50,   50,    0,    0,    0,    0,    0,
    0,   89,   50,   50,   50,    0,    0,   50,    0,   50,
   50,    0,   50,   89,   89,    0,    0,    0,    0,    0,
   50,   50,   89,    0,   89,    0,    0,    0,    0,    0,
   50,    0,    0,   89,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,   46,   89,    0,   50,    0,
    0,    0,    0,    0,    0,   89,   89,    0,    0,    0,
   89,    0,    0,    0,   50,   89,   89,    0,    0,    0,
    0,    0,    0,   46,   89,   89,   89,    0,    0,   89,
    0,   89,   89,    0,   89,   46,   46,    0,    0,    0,
    0,    0,   89,   89,    0,    0,   46,    0,    0,    0,
    0,    0,   89,    0,    0,   46,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,   47,   46,    0,
   89,    0,    0,    0,    0,    0,    0,   46,   46,    0,
    0,    0,   46,    0,    0,    0,   89,   46,   46,    0,
    0,    0,    0,    0,    0,   47,   46,   46,   46,    0,
    0,   46,    0,   46,   46,    0,   46,   47,   47,    0,
    0,    0,    0,    0,   46,   46,    0,    0,   47,    0,
    0,    0,    0,    0,   46,    0,    0,   47,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
   47,    0,   46,    0,    0,    0,    0,    0,    0,   47,
   47,    0,    0,    0,   47,    0,    0,    0,   46,   47,
   47,    0,    0,    0,    0,    0,    0,   88,   47,   47,
   47,    0,    0,   47,    0,   47,   47,    0,   47,   88,
   88,    0,    0,    0,    0,    0,   47,   47,   88,    0,
   88,    0,    0,    0,    0,    0,   47,    0,    0,   88,
    0,    0,    0,    0,    0,    0,    0,    0,  185,    0,
    0,    0,   88,    0,   47,    0,    0,  185,    0,    0,
    0,   88,   88,    0,    0,    0,   88,    0,  185,  185,
   47,   88,   88,    0,    0,    0,    0,    0,    0,  185,
   88,   88,   88,    0,    0,   88,    0,   88,   88,    0,
   88,  185,    0,    0,    0,    0,    0,    0,   88,   88,
  185,  185,    0,    0,    0,    0,    0,    0,   88,  185,
  185,  185,    0,  193,  185,  185,  185,  185,    0,  185,
    0,    0,  193,    0,    0,    0,   88,  185,  185,    0,
    0,    0,    0,  193,  193,    0,    0,    0,  185,    0,
    0,    0,   88,  189,  193,    0,    0,    0,    0,    0,
    0,    0,  189,    0,    0,  185,  193,    0,    0,    0,
    0,    0,    0,  189,  189,  193,  193,    0,    0,    0,
    0,  185,    0,    0,  193,  193,  193,    0,  190,  193,
  193,  193,  193,    0,  193,    0,  189,  190,    0,    0,
    0,    0,  193,  193,    0,  189,  189,    0,  190,  190,
    0,    0,    0,  193,  189,  189,  189,    0,  202,  189,
  189,  189,  189,    0,  189,    0,    0,  202,    0,    0,
  193,  190,  189,  189,    0,    0,    0,    0,  202,  202,
  190,  190,    0,  189,    0,    0,  193,    0,    0,  190,
  190,  190,    0,  203,  190,  190,  190,  190,    0,  190,
  189,  202,  203,    0,    0,    0,    0,  190,  190,    0,
  202,  202,    0,  203,  203,    0,  189,    0,  190,  202,
  202,  202,    0,  184,  202,  202,  202,  202,    0,  202,
    0,    0,  184,    0,    0,  190,  203,  202,  202,    0,
    0,    0,    0,  184,  184,  203,  203,    0,  202,    0,
    0,  190,    0,    0,  203,  203,  203,    0,    0,  203,
  203,  203,  203,    0,  203,  202,  184,    0,    0,    0,
    0,    0,  203,  203,    0,  184,  184,    0,    0,    0,
    0,  202,    0,  203,  184,  184,  184,    0,    0,  184,
  184,  184,  184,    0,  184,    0,   58,    0,    0,    0,
  203,    0,  184,  184,    0,    0,    0,    0,   58,   58,
    0,    0,    0,  184,    0,    0,  203,   22,    0,   58,
    0,    0,    0,    0,    0,    0,    0,    0,   58,    0,
  184,    0,    0,    0,    0,    0,   58,    0,    0,    0,
    0,   58,    0,    0,    0,    0,  184,    0,   58,   58,
   58,   58,    0,    0,    0,   58,    0,   23,    0,   58,
   58,   58,    0,    0,    0,    0,    0,    0,   58,   58,
   58,   58,    0,    0,   58,    0,   58,   58,    0,   58,
    0,   58,    0,    0,    0,    0,    0,   58,   58,    0,
   58,   58,    0,    0,    0,   58,    0,   58,    0,    0,
   58,   58,    0,    0,    0,    0,    0,    0,   58,   58,
   58,   58,    0,    0,   58,   58,   58,   58,    0,   58,
   58,   58,    0,    0,    0,    0,    0,   58,   58,    0,
    0,   58,    0,    0,    0,    0,    0,   58,    0,    0,
   58,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,   58,    0,   58,    0,    0,    0,    0,
    0,    0,   58,   58,    0,    0,    0,   58,    0,    0,
    0,    0,   58,   58,    0,    0,    0,    0,    0,    0,
    0,   58,   58,   58,    0,    0,   58,    0,   58,   58,
    0,   58,    0,    0,    0,    0,    0,    0,    0,   58,
   58,    0,    0,    0,    0,    0,    0,    0,    0,   58,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,   58,
  };
  protected static  short [] yyCheck = {            73,
    0,  306,  188,   24,  188,   56,   23,  313,  466,  257,
  601,  324,  318,  266,  269,  256,  266,  407,  306,  266,
  309,  165,  269,  306,  288,  325,  410,  306,  266,  449,
   30,  309,  266,   33,   34,   56,  267,  449,  269,  314,
  319,  288,  197,  198,  372,   23,  319,  266,  289,  313,
  266,  594,   52,  269,  306,  410,  319,  288,  309,  414,
   77,   78,  269,  304,  305,  306,  313,  253,  659,  253,
  398,  309,  288,  269,  665,  459,  262,   55,  262,  266,
  623,  288,  313,  324,  266,  309,  544,  375,  546,  326,
  309,  309,  288,  146,  189,  331,  516,  313,   76,  306,
   78,  309,  375,  615,  516,  352,  313,  407,  339,  354,
  306,  128,  375,  130,  131,  132,  133,  313,  393,  309,
  384,  352,  309,  375,  309,  414,  415,  282,  283,  146,
  520,  414,  420,  319,  389,  319,  352,  281,  325,  156,
  157,  420,  389,  196,  122,  352,  269,  309,  126,  309,
  463,  269,  269,  266,  154,  155,  352,  309,  389,  159,
  160,  161,  415,  413,  415,  288,  413,  407,  680,  627,
  288,  288,  420,  389,  360,  153,  410,  266,  690,  413,
  420,  415,  389,  306,  574,  419,  372,  699,  306,  780,
  313,  410,  416,  389,  309,  313,  313,  415,  384,  385,
  384,  385,  592,  298,  299,  796,  718,  260,  261,  445,
  188,  447,  398,  420,  604,  605,  463,  309,  309,  272,
  309,  274,  463,  410,  420,  415,  416,  280,  345,  352,
  415,  413,  463,  415,  352,  352,  541,  419,  449,  271,
  635,  273,  637,  309,  430,  318,  430,  463,  449,  269,
  449,  352,  353,  415,  254,  415,  463,  274,  275,  325,
  277,  278,  414,  309,  416,  285,  389,  463,  288,  289,
  309,  389,  389,  326,  327,  253,  410,  269,  298,  299,
  414,  676,  303,  420,  262,  309,  591,  308,  308,  310,
  802,  311,  804,  313,  309,  309,  288,  463,  318,  694,
  413,  416,  415,  609,  419,  516,  419,  358,  422,  423,
  330,  354,  449,  266,  387,  516,  309,  516,  309,  339,
  309,  313,  414,  355,  344,  416,  415,  416,  269,  361,
  419,  412,  352,  311,  415,  547,  415,  358,  309,  269,
  463,  319,  421,  319,  364,  463,  463,  288,  560,  370,
  536,  368,  536,  539,  325,  539,  309,  378,  288,  269,
  352,  271,  757,  273,  407,  760,  386,  387,  420,  389,
  416,  269,  313,  271,  769,  273,  415,  416,  288,  516,
  419,  413,  360,  313,  570,  419,  570,  404,  309,  695,
  288,  415,  416,  414,  372,  419,  396,  389,  309,  375,
  309,  416,  416,  313,  799,  795,  384,  385,  309,  409,
  410,  352,  433,  413,  325,  313,  318,  459,  615,  410,
  398,  816,  352,  416,  415,  416,  447,  416,  419,  429,
  309,  306,  432,  309,  487,  452,  436,  454,  455,  456,
  315,  419,  352,  463,  420,  355,  325,  419,  389,  466,
  450,  361,  430,   38,  352,  476,   41,  355,  415,  389,
  615,  306,  364,  361,  421,  271,  519,  273,  698,  318,
  625,  463,  414,  690,  319,  492,  631,  390,  445,  389,
  447,   66,  699,  680,   23,  387,  269,  270,  488,  272,
  720,  389,  407,  690,  415,  416,  309,  309,  419,  413,
  283,  718,  699,  413,  504,  288,  420,  415,  525,  526,
  527,  511,  295,  417,  418,  364,   55,  534,  571,  302,
  344,  718,  463,  523,  309,  680,  413,  544,  415,  546,
  313,  309,  585,  463,  413,  690,  415,   76,  387,   78,
  419,  594,  542,  276,  699,  306,  329,  777,  548,  355,
  306,  309,  335,  463,  315,  361,  786,  557,  536,  315,
  790,  539,  345,  718,  309,  463,  566,  449,  351,  352,
  623,  304,  305,  306,  410,  713,  629,  807,  390,  632,
  633,  634,  315,  122,  266,  802,  724,  804,  774,  589,
  774,  821,  570,  405,  416,  378,  379,  671,  598,  413,
  412,  413,  385,  415,  416,  802,  389,  804,  266,  626,
  627,  749,  309,  630,  153,  410,  616,  617,  618,  413,
  620,  415,  263,  264,  624,  410,  272,  309,  274,  414,
  683,  416,  417,  418,  419,  263,  264,  422,  423,  424,
  410,  309,  427,  325,  356,  357,  276,  802,  510,  804,
  703,  704,  705,  331,  707,  708,  709,  710,  711,  712,
  445,  414,  447,  269,  413,  414,  719,  410,  309,  722,
  691,  809,  435,  811,  304,  305,  306,  413,  540,  285,
  463,  414,  288,  289,  415,  315,  286,  287,  356,  357,
  269,  415,  298,  299,  503,  716,  717,  309,  413,  508,
  415,  414,  308,  565,  419,  311,  285,  313,  725,  288,
  289,  309,  318,  787,  405,  356,  357,  770,  410,  298,
  299,  424,  390,  532,  330,  587,  266,  306,  426,  308,
  289,  290,  311,  339,  313,  300,  301,  405,  427,  318,
  740,  419,  420,  421,  412,  413,  352,  415,  416,  390,
  338,  330,  463,  573,  563,  564,  410,  410,  364,  437,
  339,  439,  762,  441,  405,  782,  766,  767,  410,  309,
  413,  412,  413,  352,  415,  416,  328,  328,  390,  779,
  386,  387,  602,  389,  414,  325,  415,  413,  608,  304,
  305,  306,  390,  405,  614,  415,  774,  797,  413,  415,
  412,  413,  413,  415,  416,  306,  325,  386,  387,  413,
  389,  413,  415,  257,  412,  413,  260,  415,  416,  304,
  305,  306,  306,  823,  824,  269,  270,  271,  272,  273,
  274,  415,  276,  277,  278,  279,  413,  281,  282,  283,
  284,  285,  286,  287,  288,  289,  290,  291,  292,  293,
  413,  295,  344,  297,  298,  299,  300,  463,  302,  306,
  304,  305,  306,  339,  308,  413,  413,  311,  308,  313,
  314,  315,  306,  317,  318,  319,  320,  410,  322,  306,
  324,  419,  415,  416,  463,  329,  330,  410,  421,  306,
  266,  335,  195,  337,  266,  339,  340,  410,  342,  343,
  344,  345,  285,  386,  348,  349,  350,  351,  352,  419,
  330,  355,  410,  325,  358,  359,  360,  361,  413,  363,
  364,  365,  366,  367,  368,  413,  413,  371,  410,  373,
  475,  375,  376,  377,  378,  379,  415,  415,  269,  410,
  415,  385,  386,  387,  388,  389,  415,  391,  261,  393,
  394,  413,  414,  415,  285,  376,  419,  288,  289,  419,
  404,  382,  413,  414,  415,  419,  410,  298,  299,  419,
  414,  419,  261,  417,  418,  306,  420,  308,  419,  276,
  311,  419,  313,  413,  414,  415,  257,  318,  291,  292,
  293,  294,  295,  296,  297,  419,  306,  306,  269,  330,
  271,  422,  273,  413,  306,  449,  387,  451,  339,  453,
  306,  455,  285,  457,  285,  459,  285,  288,  289,  463,
  285,  352,  410,  444,  364,  298,  297,  298,  299,  298,
  375,  319,  388,  298,  266,  308,  394,  308,  387,  308,
  311,  324,  313,  308,  306,  306,  317,  318,  319,  306,
  318,  394,  413,  474,  306,  386,  387,  330,  389,  330,
  324,  330,  306,  319,  324,  330,  339,  375,  339,  340,
  339,  344,  306,  344,  339,  344,  324,  414,  349,  350,
  309,  352,  309,  309,  355,  413,  413,  358,  359,  360,
  361,  413,  363,  364,  365,  366,  257,  368,  413,  413,
  419,  413,  309,  309,  375,  376,  377,  309,  269,  413,
  271,  303,  273,  386,    8,  386,  387,  386,  389,   15,
   25,  386,   78,  111,  285,  122,  302,  288,  289,  274,
  107,  275,  463,  404,  516,  378,  297,  298,  299,  158,
  626,  476,  422,  309,  374,  370,  588,  308,  447,  420,
  311,  717,  313,  627,  716,  765,  317,  318,  319,  429,
  430,  431,  432,  433,  434,  405,  408,  634,  680,  330,
  727,  770,  534,  813,  544,  514,  393,  521,  339,  340,
  284,  257,  285,  344,  279,   -1,  288,   -1,  349,  350,
   -1,  352,  463,  269,  355,   -1,   -1,  358,  359,  360,
  361,   -1,  363,  364,  365,  366,   -1,  368,   -1,  285,
   -1,   -1,  288,  289,  375,  376,  377,   -1,   -1,   -1,
   -1,  297,  298,  299,   -1,  386,  387,   -1,  389,   -1,
  306,   -1,  308,   -1,   -1,  311,   -1,  313,   -1,   -1,
   -1,  317,  318,  404,  410,   -1,   -1,   -1,  414,   -1,
  416,  417,  418,  419,  330,   -1,  422,  423,  424,  420,
   -1,  427,   -1,  339,  340,  257,   -1,   -1,  344,   -1,
   -1,   -1,   -1,  349,  350,   -1,  352,  269,   -1,  445,
   -1,  447,  358,  359,  360,   -1,   -1,  363,  364,  365,
  366,   -1,  368,  285,   -1,   -1,  288,  289,   -1,   -1,
  376,  377,  463,   -1,   -1,  297,  298,  299,   -1,   -1,
  386,  387,   -1,  389,  306,   -1,  308,   -1,   -1,  311,
   -1,  313,   -1,   -1,   -1,  317,  318,   -1,  404,   -1,
   -1,   -1,   -1,   -1,   -1,  417,  418,  419,  330,   -1,
  422,  423,  424,   -1,  420,  427,   -1,  339,  340,  257,
   -1,   -1,  344,   -1,   -1,   -1,   -1,  349,  350,   -1,
  352,  269,   -1,  445,   -1,  447,  358,  359,  360,   -1,
   -1,  363,  364,  365,  366,   -1,  368,  285,   -1,   -1,
  288,  289,   -1,   -1,  376,  377,   -1,  463,   -1,  297,
  298,  299,   -1,   -1,  386,  387,   -1,  389,  306,   -1,
  308,   -1,   -1,  311,   -1,  313,   -1,   -1,   -1,  317,
  318,   -1,  404,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  330,   -1,   -1,   -1,   -1,   -1,  420,   -1,
   -1,  339,  340,  257,   -1,   -1,  344,   -1,   -1,   -1,
   -1,  349,  350,   -1,  352,  269,   -1,   -1,   -1,   -1,
  358,  359,  360,   -1,   -1,  363,  364,  365,  366,   -1,
  368,  285,   -1,   -1,  288,  289,   -1,   -1,  376,  377,
   -1,  463,   -1,  297,  298,  299,   -1,   -1,  386,  387,
   -1,  389,  306,   -1,  308,   -1,   -1,  311,   -1,  313,
   -1,   -1,   -1,  317,  318,   -1,  404,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  330,   -1,   -1,   -1,
   -1,   -1,  420,   -1,   -1,  339,  340,  257,   -1,   -1,
  344,   -1,   -1,   -1,   -1,  349,  350,   -1,  352,  269,
   -1,   -1,   -1,   -1,  358,  359,  360,   -1,   -1,  363,
  364,  365,  366,   -1,  368,  285,   -1,   -1,  288,  289,
   -1,   -1,  376,  377,   -1,  463,   -1,  297,  298,  299,
   -1,   -1,  386,  387,   -1,  389,  306,   -1,  308,   -1,
   -1,  311,   -1,  313,   -1,   -1,   -1,  317,  318,   -1,
  404,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  330,   -1,   -1,   -1,   -1,   -1,  420,   -1,   -1,  339,
  340,  257,   -1,   -1,  344,   -1,   -1,   -1,   -1,  349,
  350,   -1,  352,  269,   -1,   -1,   -1,   -1,  358,  359,
  360,   -1,   -1,  363,  364,  365,  366,   -1,  368,  285,
   -1,   -1,  288,  289,   -1,   -1,  376,  377,   -1,  463,
   -1,  297,  298,  299,   -1,   -1,  386,  387,   -1,  389,
  306,   -1,  308,   -1,   -1,  311,   -1,  313,   -1,   -1,
   -1,  317,  318,   -1,  404,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  330,   -1,   -1,   -1,   -1,   -1,
  420,   -1,   -1,  339,  340,  257,   -1,   -1,  344,   -1,
   -1,   -1,   -1,  349,  350,   -1,  352,  269,   -1,   -1,
   -1,   -1,  358,  359,  360,   -1,   -1,  363,  364,  365,
  366,   -1,  368,  285,   -1,   -1,  288,  289,   -1,   -1,
  376,  377,   -1,  463,   -1,  297,  298,  299,   -1,   -1,
  386,  387,   -1,  389,  306,   -1,  308,   -1,   -1,  311,
   -1,  313,   -1,   -1,   -1,  317,  318,   -1,  404,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  330,   -1,
   -1,   -1,   -1,   -1,  420,   -1,   -1,  339,  340,   -1,
   -1,   -1,  344,   -1,   -1,   -1,   -1,  349,  350,   -1,
  352,   -1,   -1,   -1,   -1,   -1,  358,  359,  360,   -1,
   -1,  363,  364,  365,  366,   -1,  368,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  376,  377,   -1,  463,   -1,   -1,
   -1,   -1,  260,   -1,  386,  387,   -1,  389,   -1,   -1,
   -1,  269,  270,   -1,  272,   -1,  274,   -1,   -1,  277,
  278,  279,  404,  281,  282,  283,  284,   -1,  286,  287,
  288,  289,  290,  291,  292,  293,   -1,  295,  420,   -1,
   -1,  299,  300,   -1,  302,   -1,   -1,   -1,  306,   -1,
   -1,   -1,   -1,   -1,   -1,  313,  314,   -1,   -1,   -1,
   -1,   -1,  320,   -1,  322,   -1,  324,   -1,   -1,   -1,
  309,  329,  269,  270,   -1,  272,   -1,  335,   -1,  337,
   -1,  463,   -1,   -1,  342,  343,  283,  345,   -1,   -1,
  348,  288,   -1,  351,  352,   -1,   -1,   -1,  295,   -1,
   -1,   -1,   -1,   -1,   -1,  302,   -1,   -1,   -1,  367,
   -1,   -1,   -1,  371,   -1,  373,  313,  356,  357,   -1,
  378,  379,   -1,   -1,   -1,   -1,   -1,  385,   -1,   -1,
  388,  389,  329,  391,   -1,  393,  394,   -1,  335,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  390,  410,   -1,  351,  352,  414,   -1,   -1,  417,
  418,   -1,   -1,   -1,   -1,   -1,  405,   -1,   -1,   -1,
   -1,   -1,   -1,  412,  413,   -1,  415,  416,   -1,   -1,
   -1,  378,  379,   -1,  309,   -1,   -1,   -1,  385,   -1,
   -1,  449,  389,  451,  260,  453,   -1,  455,   -1,  457,
   -1,  459,   -1,  269,  270,  463,  272,   -1,  274,   -1,
   -1,  277,  278,  279,   -1,  281,  282,  283,  284,   -1,
  286,  287,  288,  289,  290,  291,  292,  293,   -1,  295,
   -1,  356,  357,  299,  300,   -1,  302,   -1,   -1,   -1,
  306,   -1,   -1,   -1,   -1,   -1,   -1,  313,  314,   -1,
   -1,   -1,   -1,   -1,  320,   -1,  322,   -1,  324,   -1,
   -1,   -1,   -1,  329,   -1,  390,  463,   -1,   -1,  335,
   -1,  337,   -1,   -1,   -1,   -1,  342,  343,   -1,  345,
  405,   -1,  348,   -1,   -1,  351,  352,  412,  413,   -1,
  415,  416,   -1,   -1,  297,   -1,   -1,   -1,   -1,   -1,
   -1,  367,   -1,  306,   -1,  371,   -1,  373,   -1,   -1,
   -1,   -1,  378,  379,  317,  318,   -1,   -1,   -1,  385,
   -1,   -1,  388,  389,   -1,  391,   -1,  393,  394,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  340,   -1,   -1,
   -1,   -1,   -1,   -1,  410,   -1,  349,  350,  414,   -1,
   -1,  417,  418,   -1,   -1,  358,  359,  360,   -1,   -1,
  363,  364,  365,  366,   -1,  368,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  376,  377,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  449,  387,  451,  260,  453,   -1,  455,
   -1,  457,   -1,  459,   -1,  269,  270,  463,  272,   -1,
  274,  404,   -1,  277,  278,  279,   -1,  281,  282,  283,
  284,   -1,  286,  287,  288,  289,  290,  291,  292,  293,
   -1,  295,   -1,   -1,   -1,  299,  300,   -1,  302,   -1,
   -1,  285,  306,   -1,   -1,  289,   -1,   -1,   -1,  313,
  314,   -1,   -1,   -1,  298,  299,  320,   -1,  322,   -1,
  324,   -1,   -1,   -1,  308,  329,   -1,  311,   -1,   -1,
   -1,  335,   -1,  337,  318,   -1,   -1,   -1,  342,  343,
   -1,  345,   -1,   -1,  348,   -1,  330,  351,  352,   -1,
   -1,   -1,   -1,   -1,   -1,  339,  297,   -1,   -1,   -1,
   -1,   -1,   -1,  367,   -1,  306,   -1,  371,   -1,  373,
   -1,   -1,   -1,   -1,  378,  379,  317,  318,   -1,   -1,
  364,  385,   -1,   -1,  388,  389,   -1,  391,   -1,  393,
  394,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  340,
   -1,   -1,  386,  387,   -1,   -1,  410,   -1,  349,  350,
  414,   -1,   -1,  417,  418,   -1,   -1,  358,  359,  360,
   -1,   -1,  363,  364,  365,  366,   -1,  368,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  376,  377,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  449,  387,  451,  260,  453,
   -1,  455,   -1,  457,   -1,  459,   -1,  269,  270,  463,
  272,   -1,  274,  404,   -1,  277,  278,  279,   -1,  281,
  282,  283,  284,   -1,  286,  287,  288,  289,  290,  291,
  292,  293,   -1,  295,   -1,   -1,   -1,  299,  300,   -1,
  302,   -1,   -1,  285,  306,   -1,   -1,  289,   -1,   -1,
   -1,  313,  314,   -1,   -1,   -1,  298,  299,  320,   -1,
  322,   -1,  324,   -1,   -1,   -1,  308,  329,   -1,  311,
   -1,   -1,   -1,  335,   -1,  337,  318,   -1,   -1,   -1,
  342,  343,   -1,  345,   -1,   -1,  348,   -1,  330,  351,
  352,   -1,   -1,   -1,   -1,   -1,   -1,  339,  297,   -1,
   -1,   -1,   -1,   -1,   -1,  367,   -1,   -1,   -1,  371,
   -1,  373,   -1,   -1,   -1,   -1,  378,  379,  317,   -1,
   -1,   -1,   -1,  385,   -1,   -1,  388,  389,   -1,  391,
   -1,  393,  394,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  340,   -1,   -1,  386,  387,   -1,   -1,  410,   -1,
  349,  350,  414,   -1,   -1,  417,  418,   -1,   -1,  358,
  359,  360,   -1,   -1,  363,   -1,  365,  366,   -1,  368,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  376,  377,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  449,   -1,  451,
  260,  453,   -1,  455,   -1,  457,   -1,  459,   -1,  269,
  270,  463,  272,   -1,  274,  404,   -1,  277,  278,  279,
   -1,  281,  282,  283,  284,   -1,  286,  287,  288,  289,
  290,  291,  292,  293,   -1,  295,   -1,   -1,   -1,  299,
  300,   -1,  302,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  313,  314,   -1,   -1,   -1,   -1,   -1,
  320,   -1,  322,   -1,  324,   -1,   -1,   -1,   -1,  329,
   -1,   -1,   -1,   -1,   -1,  335,   -1,  337,   -1,   -1,
   -1,   -1,  342,  343,   -1,  345,   -1,   -1,  348,   -1,
   -1,  351,  352,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  367,   -1,   -1,
   -1,  371,   -1,  373,   -1,   -1,   -1,   -1,  378,  379,
   -1,   -1,   -1,   -1,   -1,  385,   -1,   -1,  388,  389,
   -1,  391,   -1,  393,  394,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  410,   -1,   -1,  261,   -1,   -1,   -1,  417,  418,   -1,
   -1,  269,  270,  271,  272,   -1,  274,   -1,   -1,  277,
  278,  279,   -1,  281,  282,  283,  284,   -1,  286,  287,
  288,   -1,  290,  291,  292,  293,   -1,  295,   -1,  449,
   -1,  451,  300,  453,  302,  455,   -1,  457,   -1,  459,
   -1,   -1,   -1,  463,   -1,  313,  314,   -1,   -1,   -1,
   -1,   -1,  320,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  329,   -1,   -1,   -1,   -1,   -1,  335,   -1,  337,
   -1,   -1,   -1,   -1,  342,  343,   -1,  345,   -1,  347,
  348,   -1,   -1,  351,  352,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  378,  379,   -1,   -1,   -1,   -1,   -1,  385,   -1,   -1,
   -1,  389,   -1,   -1,   -1,  393,   -1,  395,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  410,   -1,   -1,  261,   -1,   -1,   -1,  417,
  418,   -1,   -1,  269,  270,   -1,  272,   -1,  274,   -1,
   -1,  277,  278,  279,   -1,  281,  282,  283,  284,   -1,
  286,  287,  288,   -1,  290,  291,  292,  293,   -1,  295,
   -1,  449,   -1,  451,  300,  453,  302,  455,   -1,  457,
   -1,  459,   -1,   -1,   -1,  463,   -1,  313,  314,   -1,
   -1,   -1,   -1,   -1,  320,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  329,   -1,   -1,   -1,   -1,   -1,  335,
   -1,  337,   -1,   -1,   -1,   -1,  342,  343,   -1,  345,
   -1,  347,  348,   -1,   -1,  351,  352,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  378,  379,   -1,   -1,   -1,   -1,   -1,  385,
   -1,   -1,   -1,  389,   -1,   -1,   -1,  393,   -1,  395,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  410,   -1,   -1,   -1,   -1,   -1,
   -1,  417,  418,   -1,   -1,  269,  270,   -1,  272,   -1,
  274,   -1,   -1,  277,  278,  279,   -1,  281,  282,  283,
  284,   -1,  286,  287,  288,   -1,  290,  291,  292,  293,
   -1,  295,   -1,  449,   -1,  451,  300,  453,  302,  455,
   -1,  457,   -1,  459,   -1,   -1,   -1,  463,   -1,  313,
  314,   -1,   -1,   -1,   -1,   -1,  320,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  329,   -1,   -1,   -1,   -1,
   -1,  335,   -1,  337,   -1,   -1,   -1,   -1,  342,  343,
   -1,  345,   -1,  347,  348,   -1,   -1,  351,  352,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  378,  379,   -1,   -1,   -1,   -1,
   -1,  385,   -1,   -1,   -1,  389,   -1,   -1,   -1,  393,
   -1,  395,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  410,  411,  412,   -1,
   -1,   -1,   -1,  417,  418,   -1,   -1,  269,  270,   -1,
  272,   -1,  274,   -1,   -1,  277,  278,  279,   -1,  281,
  282,  283,  284,   -1,  286,  287,  288,   -1,  290,  291,
  292,  293,   -1,  295,   -1,  449,   -1,  451,  300,  453,
  302,  455,   -1,  457,   -1,  459,   -1,   -1,   -1,  463,
   -1,  313,  314,   -1,   -1,   -1,   -1,   -1,  320,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  329,   -1,   -1,
   -1,   -1,   -1,  335,   -1,  337,   -1,   -1,   -1,   -1,
  342,  343,   -1,  345,   -1,  347,  348,   -1,   -1,  351,
  352,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  378,  379,   -1,   -1,
   -1,   -1,   -1,  385,   -1,   -1,   -1,  389,   -1,   -1,
   -1,  393,   -1,  395,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  410,  411,
   -1,   -1,   -1,   -1,   -1,  417,  418,   -1,   -1,  269,
  270,   -1,  272,   -1,  274,   -1,   -1,  277,  278,  279,
   -1,  281,  282,  283,  284,   -1,  286,  287,  288,   -1,
  290,  291,  292,  293,   -1,  295,   -1,  449,   -1,  451,
  300,  453,  302,  455,   -1,  457,   -1,  459,   -1,   -1,
   -1,  463,   -1,  313,  314,   -1,   -1,   -1,   -1,   -1,
  320,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  329,
   -1,   -1,   -1,   -1,   -1,  335,   -1,  337,   -1,   -1,
   -1,   -1,  342,  343,   -1,  345,   -1,  347,  348,   -1,
   -1,  351,  352,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  378,  379,
   -1,   -1,   -1,   -1,   -1,  385,   -1,   -1,   -1,  389,
   -1,   -1,   -1,  393,   -1,  395,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  410,   -1,   -1,   -1,   -1,   -1,  257,  417,  418,   -1,
   -1,  269,  270,   -1,  272,   -1,  274,   -1,   -1,  277,
  278,  279,   -1,  281,  282,  283,  284,   -1,  286,  287,
  288,   -1,  290,  291,  292,  293,   -1,  295,   -1,  449,
   -1,  451,  300,  453,  302,  455,  297,  457,   -1,  459,
   -1,   -1,   -1,  463,   -1,  313,  314,   -1,   -1,   -1,
   -1,   -1,  320,   -1,   -1,   -1,  317,   -1,   -1,   -1,
   -1,  329,   -1,   -1,   -1,   -1,   -1,  335,   -1,  337,
   -1,   -1,   -1,   -1,  342,  343,   -1,  345,   -1,  340,
  348,   -1,   -1,  351,  352,   -1,   -1,   -1,  349,  350,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  358,  359,  360,
   -1,   -1,  363,   -1,  365,  366,   -1,  368,   -1,   -1,
  378,  379,   -1,   -1,   -1,  376,  377,  385,   -1,   -1,
   -1,  389,   -1,   -1,   -1,  393,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  410,  404,   -1,   -1,   -1,   -1,   -1,  417,
  418,   -1,   -1,  269,  270,   -1,  272,   -1,  274,   -1,
   -1,  277,  278,  279,   -1,  281,  282,  283,  284,   -1,
  286,  287,  288,   -1,  290,  291,  292,  293,   -1,  295,
   -1,  449,   -1,  451,  300,  453,  302,  455,   -1,  457,
   -1,  459,   -1,   -1,   -1,  463,   -1,  313,  314,   -1,
   -1,   -1,   -1,   -1,  320,  257,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  329,   -1,   -1,   -1,   -1,   -1,  335,
   -1,  337,   -1,   -1,   -1,   -1,  342,  343,   -1,  345,
   -1,   -1,  348,  285,   -1,  351,  352,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  297,  298,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  308,   -1,   -1,   -1,
   -1,   -1,  378,  379,   -1,  317,   -1,   -1,   -1,  385,
   -1,   -1,   -1,  389,   -1,   -1,   -1,  393,  330,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  339,  340,   -1,
   -1,   -1,  344,   -1,  410,   -1,   -1,  349,  350,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  358,  359,  360,   -1,
   -1,  363,   -1,  365,  366,   -1,  368,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  376,  377,  269,   -1,   -1,   -1,
   -1,   -1,   -1,  449,  386,  451,   -1,  453,   -1,  455,
   -1,  457,  285,  459,   -1,  288,  289,  463,   -1,   -1,
   -1,   -1,  404,   -1,  297,  298,  299,   -1,   -1,   -1,
   -1,   -1,   -1,  306,   -1,  308,   -1,   -1,  311,   -1,
  313,   -1,   -1,   -1,  317,  318,   -1,   -1,   -1,   -1,
   -1,   -1,  325,   -1,   -1,  328,   -1,  330,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  339,  340,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  349,  350,  269,  352,
   -1,   -1,   -1,   -1,   -1,  358,  359,  360,   -1,   -1,
  363,  364,  365,  366,  285,  368,   -1,  288,  289,   -1,
   -1,   -1,   -1,  376,  377,   -1,  297,  298,  299,   -1,
   -1,   -1,   -1,  386,  387,  306,  389,  308,   -1,   -1,
  311,   -1,  313,   -1,   -1,   -1,  317,  318,   -1,   -1,
   -1,  404,   -1,   -1,  325,   -1,   -1,   -1,   -1,  330,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  420,  339,  340,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  349,  350,
   -1,  352,   -1,  269,   -1,   -1,   -1,  358,  359,  360,
   -1,   -1,  363,  364,  365,  366,   -1,  368,   -1,  285,
   -1,   -1,  288,  289,   -1,  376,  377,   -1,   -1,   -1,
  463,  297,  298,  299,   -1,  386,  387,   -1,  389,   -1,
  306,   -1,  308,   -1,   -1,  311,   -1,  313,   -1,   -1,
   -1,  317,  318,  404,   -1,   -1,   -1,   -1,   -1,  325,
   -1,   -1,   -1,   -1,  330,   -1,   -1,   -1,   -1,  420,
   -1,   -1,   -1,  339,  340,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  349,  350,  269,  352,   -1,   -1,   -1,
   -1,   -1,  358,  359,  360,   -1,   -1,  363,  364,  365,
  366,  285,  368,   -1,  288,  289,   -1,   -1,   -1,   -1,
  376,  377,  463,  297,  298,  299,   -1,   -1,   -1,   -1,
  386,  387,  306,  389,  308,   -1,   -1,  311,   -1,  313,
   -1,   -1,   -1,  317,  318,   -1,   -1,   -1,  404,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  330,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  420,  339,  340,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  349,  350,   -1,  352,   -1,
  269,   -1,   -1,   -1,  358,  359,  360,   -1,   -1,  363,
  364,  365,  366,   -1,  368,   -1,  285,   -1,   -1,  288,
  289,   -1,  376,  377,   -1,   -1,   -1,  463,  297,  298,
  299,   -1,  386,  387,   -1,  389,   -1,  306,   -1,  308,
   -1,   -1,  311,   -1,  313,   -1,   -1,   -1,  317,  318,
  404,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  330,   -1,   -1,   -1,   -1,  420,   -1,   -1,   -1,
  339,  340,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  349,  350,  269,  352,   -1,   -1,   -1,   -1,   -1,  358,
  359,  360,   -1,   -1,  363,  364,  365,  366,  285,  368,
   -1,  288,  289,   -1,   -1,   -1,   -1,  376,  377,  463,
  297,  298,  299,   -1,   -1,   -1,   -1,  386,  387,  306,
  389,  308,  309,   -1,  311,   -1,  313,   -1,   -1,   -1,
  317,  318,   -1,   -1,   -1,  404,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  330,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  420,  339,  340,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  349,  350,   -1,  352,   -1,   -1,   -1,   -1,
   -1,  358,  359,  360,   -1,   -1,  363,   -1,  365,  366,
   -1,  368,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  376,
  377,   -1,  269,   -1,  463,   -1,   -1,   -1,   -1,  386,
  387,   -1,  389,   -1,   -1,   -1,   -1,   -1,  285,   -1,
   -1,  288,  289,   -1,   -1,   -1,   -1,  404,   -1,   -1,
  297,  298,  299,   -1,   -1,   -1,   -1,   -1,  415,  306,
   -1,  308,  309,   -1,  311,   -1,  313,   -1,   -1,   -1,
  317,  318,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  330,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  339,  340,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  349,  350,  269,  352,  463,   -1,   -1,   -1,
   -1,  358,  359,  360,   -1,   -1,  363,   -1,  365,  366,
  285,  368,   -1,  288,  289,   -1,   -1,   -1,   -1,  376,
  377,   -1,  297,  298,  299,   -1,   -1,   -1,   -1,  386,
  387,  306,  389,  308,   -1,   -1,  311,   -1,  313,   -1,
   -1,   -1,  317,  318,   -1,   -1,   -1,  404,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  330,   -1,   -1,  415,   -1,
   -1,   -1,   -1,   -1,  339,  340,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  349,  350,   -1,  352,   -1,  269,
   -1,   -1,   -1,  358,  359,  360,   -1,   -1,  363,  364,
  365,  366,   -1,  368,   -1,  285,   -1,   -1,  288,  289,
   -1,  376,  377,   -1,   -1,   -1,  463,  297,  298,  299,
   -1,  386,  387,   -1,  389,   -1,  306,   -1,  308,   -1,
   -1,  311,   -1,  313,   -1,   -1,   -1,  317,  318,  404,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  330,   -1,   -1,   -1,   -1,  420,   -1,   -1,   -1,  339,
  340,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  349,
  350,  269,  352,   -1,   -1,   -1,   -1,   -1,  358,  359,
  360,   -1,   -1,  363,  364,  365,  366,  285,  368,   -1,
  288,  289,   -1,   -1,   -1,   -1,  376,  377,  463,  297,
  298,  299,   -1,   -1,   -1,   -1,  386,  387,  306,  389,
  308,   -1,   -1,  311,   -1,  313,   -1,   -1,   -1,  317,
  318,   -1,   -1,   -1,  404,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  330,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  420,  339,  340,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  349,  350,   -1,  352,   -1,  269,   -1,   -1,   -1,
  358,  359,  360,   -1,   -1,  363,  364,  365,  366,   -1,
  368,   -1,  285,   -1,   -1,  288,  289,   -1,  376,  377,
   -1,   -1,   -1,  463,  297,  298,  299,   -1,  386,  387,
   -1,  389,   -1,  306,   -1,  308,   -1,   -1,  311,   -1,
  313,   -1,   -1,   -1,  317,  318,  404,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  330,   -1,   -1,
   -1,   -1,  420,   -1,   -1,   -1,  339,  340,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  349,  350,  269,  352,
   -1,   -1,   -1,   -1,   -1,  358,  359,  360,   -1,   -1,
  363,  364,  365,  366,  285,  368,   -1,  288,  289,   -1,
   -1,   -1,   -1,  376,  377,  463,  297,  298,  299,   -1,
   -1,   -1,   -1,  386,  387,  306,  389,  308,   -1,   -1,
  311,   -1,  313,   -1,   -1,   -1,  317,  318,   -1,   -1,
   -1,  404,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  330,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  420,  339,  340,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  349,  350,
   -1,  352,   -1,  269,   -1,   -1,   -1,  358,  359,  360,
   -1,   -1,  363,  364,  365,  366,   -1,  368,   -1,  285,
   -1,   -1,  288,  289,   -1,  376,  377,   -1,   -1,   -1,
  463,  297,  298,  299,   -1,  386,  387,   -1,  389,   -1,
  306,   -1,  308,   -1,   -1,  311,   -1,  313,   -1,   -1,
   -1,  317,  318,  404,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  330,   -1,   -1,   -1,   -1,  420,
   -1,   -1,   -1,  339,  340,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  349,  350,  269,  352,   -1,   -1,   -1,
   -1,   -1,  358,  359,  360,   -1,   -1,  363,  364,  365,
  366,  285,  368,   -1,  288,  289,   -1,   -1,   -1,   -1,
  376,  377,  463,  297,  298,  299,   -1,   -1,   -1,   -1,
  386,  387,  306,  389,  308,   -1,   -1,  311,   -1,  313,
   -1,   -1,   -1,  317,  318,   -1,   -1,   -1,  404,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  330,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  420,  339,  340,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  349,  350,   -1,  352,   -1,
  269,   -1,   -1,   -1,  358,  359,  360,   -1,   -1,  363,
  364,  365,  366,   -1,  368,   -1,  285,   -1,   -1,  288,
  289,   -1,  376,  377,   -1,   -1,   -1,  463,  297,  298,
  299,   -1,  386,  387,   -1,  389,   -1,  306,   -1,  308,
   -1,   -1,  311,   -1,  313,   -1,   -1,   -1,  317,  318,
  404,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  330,   -1,   -1,   -1,   -1,  420,   -1,   -1,   -1,
  339,  340,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  349,  350,  269,  352,   -1,   -1,   -1,   -1,   -1,  358,
  359,  360,   -1,   -1,  363,  364,  365,  366,  285,  368,
   -1,  288,  289,   -1,   -1,   -1,   -1,  376,  377,  463,
  297,  298,  299,   -1,   -1,   -1,   -1,  386,  387,  306,
  389,  308,   -1,   -1,  311,   -1,  313,   -1,   -1,   -1,
  317,  318,   -1,   -1,   -1,  404,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  330,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  420,  339,  340,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  349,  350,   -1,  352,   -1,  269,   -1,   -1,
   -1,  358,  359,  360,   -1,   -1,  363,  364,  365,  366,
   -1,  368,   -1,  285,   -1,   -1,  288,  289,   -1,  376,
  377,   -1,   -1,   -1,  463,  297,  298,  299,   -1,  386,
  387,   -1,  389,   -1,  306,   -1,  308,   -1,   -1,  311,
   -1,  313,   -1,   -1,   -1,  317,  318,  404,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  330,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  339,  340,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  349,  350,  269,
  352,   -1,   -1,   -1,   -1,   -1,  358,  359,  360,   -1,
   -1,  363,  364,  365,  366,  285,  368,   -1,  288,  289,
   -1,   -1,   -1,   -1,  376,  377,  463,  297,  298,  299,
   -1,   -1,   -1,   -1,  386,  387,  306,  389,  308,  309,
   -1,  311,   -1,  313,   -1,   -1,   -1,  317,  318,   -1,
   -1,   -1,  404,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  330,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  339,
  340,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  349,
  350,   -1,  352,   -1,  269,   -1,   -1,   -1,  358,  359,
  360,   -1,   -1,  363,   -1,  365,  366,   -1,  368,   -1,
  285,   -1,   -1,  288,  289,   -1,  376,  377,   -1,   -1,
   -1,  463,  297,  298,  299,   -1,  386,  387,   -1,  389,
   -1,  306,   -1,  308,   -1,   -1,  311,   -1,  313,   -1,
   -1,   -1,  317,  318,  404,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  330,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  339,  340,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  349,  350,  269,  352,   -1,   -1,
   -1,   -1,   -1,  358,  359,  360,   -1,   -1,  363,  364,
  365,  366,  285,  368,   -1,  288,  289,   -1,   -1,   -1,
   -1,  376,  377,  463,  297,  298,  299,   -1,   -1,   -1,
   -1,  386,  387,  306,  389,  308,   -1,   -1,  311,   -1,
  313,   -1,   -1,   -1,  317,  318,   -1,   -1,   -1,  404,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  330,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  339,  340,   -1,   -1,
   -1,   -1,   -1,  263,  264,   -1,  349,  350,   -1,  352,
   -1,  269,   -1,   -1,   -1,  358,  359,  360,   -1,   -1,
  363,  364,  365,  366,   -1,  368,   -1,  285,   -1,   -1,
  288,  289,   -1,  376,  377,   -1,   -1,   -1,  463,  297,
  298,  299,   -1,  386,  387,   -1,  389,   -1,  306,  309,
  308,   -1,   -1,  311,   -1,  313,   -1,   -1,   -1,  317,
  318,  404,   -1,   -1,   -1,  325,   -1,   -1,   -1,   -1,
   -1,  331,  330,   -1,   -1,   -1,  263,  264,  338,   -1,
   -1,  339,  340,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  349,  350,   -1,  352,   -1,  356,  357,   -1,   -1,
  358,  359,  360,   -1,   -1,  363,   -1,  365,  366,   -1,
  368,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  376,  377,
  463,   -1,  309,   -1,   -1,   -1,  263,  264,  386,  387,
  390,  389,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  331,  405,  404,   -1,   -1,   -1,
  410,  338,  412,  413,   -1,  415,  416,  417,  418,  419,
  420,  421,  422,  423,  424,   -1,  426,  427,   -1,  356,
  357,   -1,  309,   -1,   -1,   -1,   -1,  437,   -1,  439,
   -1,  441,   -1,   -1,   -1,  445,   -1,  447,   -1,   -1,
   -1,   -1,   -1,   -1,  331,   -1,   -1,   -1,   -1,  263,
  264,  338,   -1,  390,   -1,  463,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  405,  356,
  357,   -1,   -1,  410,   -1,  412,  413,  414,  415,  416,
  417,  418,  419,  420,  421,  422,  423,  424,   -1,  426,
  427,   -1,   -1,   -1,   -1,  309,   -1,   -1,   -1,   -1,
  437,   -1,  439,  390,  441,   -1,   -1,   -1,  445,   -1,
  447,   -1,   -1,   -1,   -1,   -1,   -1,  331,  405,   -1,
   -1,   -1,   -1,  410,  338,  412,  413,  414,  415,  416,
  417,  418,  419,  420,  421,  422,  423,  424,   -1,  426,
  427,   -1,  356,  357,   -1,   -1,  263,  264,   -1,   -1,
  437,   -1,  439,   -1,  441,   -1,   -1,   -1,  445,   -1,
  447,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  390,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  263,  264,   -1,   -1,   -1,   -1,
   -1,  405,  309,   -1,   -1,   -1,  410,   -1,  412,  413,
   -1,  415,  416,  417,  418,  419,  420,  421,  422,  423,
  424,   -1,  426,  427,  331,   -1,   -1,   -1,   -1,   -1,
   -1,  338,   -1,  437,   -1,  439,   -1,  441,   -1,   -1,
  309,  445,   -1,  447,   -1,   -1,   -1,   -1,   -1,  356,
  357,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  331,   -1,   -1,   -1,   -1,   -1,   -1,  338,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  390,   -1,   -1,   -1,  356,  357,   -1,
   -1,  263,  264,   -1,   -1,   -1,   -1,   -1,  405,   -1,
   -1,   -1,   -1,   -1,   -1,  412,  413,  414,  415,  416,
  417,  418,  419,  420,  421,  422,  423,  424,   -1,  426,
  427,  390,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  437,   -1,  439,   -1,  441,   -1,  405,  309,  445,   -1,
  447,  263,  264,  412,  413,   -1,  415,  416,  417,  418,
  419,  420,  421,  422,  423,  424,   -1,  426,  427,  331,
   -1,   -1,   -1,   -1,   -1,   -1,  338,   -1,  437,   -1,
  439,   -1,  441,   -1,   -1,   -1,  445,   -1,  447,   -1,
   -1,   -1,   -1,   -1,  356,  357,   -1,  309,  263,  264,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  331,
   -1,   -1,   -1,   -1,   -1,   -1,  338,   -1,  390,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  405,  356,  357,   -1,   -1,  263,  264,
  412,  413,   -1,  415,  416,  417,  418,  419,  420,  421,
  422,  423,  424,   -1,  426,  427,  331,   -1,   -1,   -1,
   -1,   -1,   -1,  338,   -1,  437,   -1,  439,  390,  441,
   -1,   -1,   -1,  445,   -1,  447,   -1,   -1,   -1,   -1,
   -1,  356,  357,  405,  309,   -1,   -1,   -1,  263,  264,
  412,  413,   -1,  415,  416,  417,  418,  419,  420,  421,
  422,  423,   -1,   -1,  426,  427,  331,   -1,   -1,   -1,
   -1,   -1,   -1,  338,   -1,  437,   -1,  439,   -1,  441,
   -1,   -1,   -1,  445,   -1,  447,   -1,   -1,   -1,   -1,
  405,  356,  357,   -1,  309,  410,   -1,   -1,  413,  414,
  415,   -1,  417,  418,  419,  420,  421,  422,  423,  424,
   -1,  426,  427,   -1,   -1,   -1,  331,   -1,   -1,   -1,
  263,  264,  437,  338,  439,  390,  441,   -1,   -1,   -1,
  445,   -1,  447,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  405,  356,  357,   -1,   -1,   -1,   -1,  412,  413,   -1,
  415,  416,  417,  418,  419,  420,  421,   -1,   -1,   -1,
   -1,  426,  427,  263,  264,   -1,  309,   -1,   -1,   -1,
   -1,   -1,  437,   -1,  439,  390,  441,   -1,   -1,   -1,
  445,   -1,  447,   -1,   -1,   -1,   -1,   -1,  331,   -1,
  405,   -1,   -1,   -1,   -1,  338,   -1,  412,  413,   -1,
  415,  416,  417,  418,  419,  420,  421,   -1,   -1,  309,
   -1,  426,  427,  356,  357,  263,  264,   -1,   -1,   -1,
   -1,   -1,  437,   -1,  439,   -1,  441,   -1,   -1,   -1,
  445,  331,  447,   -1,   -1,   -1,   -1,   -1,  338,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  390,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  356,  357,   -1,   -1,
   -1,  309,  405,   -1,   -1,  263,  264,   -1,   -1,  412,
  413,   -1,  415,  416,  417,  418,  419,  420,  421,   -1,
   -1,   -1,   -1,  331,  427,   -1,   -1,   -1,   -1,   -1,
  390,   -1,   -1,   -1,  437,   -1,  439,   -1,  441,   -1,
   -1,   -1,  445,   -1,  447,  405,   -1,   -1,  356,  357,
   -1,  309,  412,  413,   -1,  415,  416,  417,  418,  419,
  420,  421,   -1,   -1,   -1,   -1,   -1,  427,   -1,   -1,
   -1,   -1,   -1,  331,  263,  264,   -1,  437,   -1,  439,
   -1,  441,  390,   -1,   -1,  445,   -1,  447,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  405,  356,  357,
   -1,   -1,   -1,   -1,  412,  413,   -1,  415,  416,  417,
  418,  419,  420,  421,   -1,   -1,   -1,   -1,   -1,  427,
  309,   -1,   -1,   -1,  263,  264,   -1,   -1,   -1,  437,
   -1,  439,  390,  441,   -1,   -1,   -1,  445,   -1,  447,
   -1,   -1,  331,   -1,   -1,   -1,   -1,  405,   -1,   -1,
   -1,   -1,   -1,   -1,  412,  413,   -1,  415,  416,  417,
  418,  419,  420,  421,   -1,   -1,   -1,  356,  357,  427,
  309,   -1,   -1,   -1,  263,  264,   -1,   -1,   -1,  437,
   -1,  439,   -1,  441,   -1,   -1,   -1,  445,   -1,  447,
   -1,   -1,  331,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  390,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  405,  356,  357,   -1,
  309,   -1,   -1,  412,  413,   -1,  415,  416,  417,  418,
  419,  420,  421,   -1,   -1,   -1,   -1,   -1,  427,   -1,
   -1,   -1,  331,  263,  264,   -1,   -1,   -1,  437,   -1,
  439,  390,  441,   -1,   -1,   -1,  445,   -1,  447,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  405,  356,  357,   -1,
   -1,   -1,   -1,  412,  413,   -1,  415,  416,   -1,   -1,
  419,  420,  421,   -1,   -1,   -1,   -1,   -1,  427,  309,
   -1,   -1,   -1,  263,  264,   -1,   -1,   -1,  437,   -1,
  439,  390,  441,   -1,   -1,   -1,  445,   -1,  447,   -1,
   -1,  331,   -1,   -1,   -1,   -1,  405,   -1,   -1,   -1,
   -1,   -1,   -1,  412,  413,   -1,  415,  416,   -1,   -1,
  419,  420,  421,   -1,   -1,   -1,  356,  357,  427,  309,
   -1,   -1,   -1,  263,  264,   -1,   -1,   -1,  437,   -1,
  439,   -1,  441,   -1,   -1,   -1,  445,   -1,  447,   -1,
   -1,  331,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  390,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  263,  264,   -1,   -1,   -1,  405,  356,  357,   -1,  309,
   -1,   -1,  412,  413,   -1,  415,  416,   -1,   -1,  419,
  420,  421,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  331,   -1,   -1,   -1,  263,  264,  437,   -1,  439,
  390,  441,   -1,   -1,   -1,  445,  309,  447,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  405,  356,  357,   -1,   -1,
   -1,   -1,  412,  413,   -1,  415,  416,   -1,  331,  419,
  420,  421,  263,  264,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  309,   -1,   -1,   -1,   -1,   -1,  437,   -1,  439,
  390,  441,   -1,  356,  357,  445,   -1,  447,   -1,   -1,
   -1,   -1,   -1,  331,   -1,  405,   -1,   -1,   -1,   -1,
   -1,   -1,  412,  413,   -1,  415,  416,   -1,  309,  419,
  420,  421,  263,  264,   -1,   -1,   -1,  390,  356,  357,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  437,   -1,  439,
  331,  441,  405,   -1,   -1,  445,   -1,  447,   -1,  412,
  413,   -1,  415,  416,   -1,   -1,  419,  420,  421,  263,
  264,   -1,  390,   -1,   -1,  356,  357,   -1,  309,   -1,
   -1,   -1,  263,  264,  437,   -1,  439,  405,  441,   -1,
   -1,   -1,   -1,   -1,  412,  413,   -1,  415,  416,   -1,
  331,  419,  420,  421,   -1,   -1,   -1,   -1,   -1,  390,
   -1,   -1,   -1,   -1,   -1,  309,   -1,   -1,   -1,  437,
   -1,  439,   -1,  441,  405,  356,  357,   -1,  309,   -1,
   -1,  412,  413,   -1,  415,  416,   -1,  331,  419,  420,
  421,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  263,  264,
  331,   -1,   -1,   -1,   -1,   -1,  437,   -1,  439,  390,
  441,   -1,  356,  357,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  405,  356,  357,   -1,   -1,   -1,
   -1,  412,  413,   -1,  415,  416,   -1,   -1,  419,  420,
  421,  263,  264,   -1,  309,   -1,  390,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  437,   -1,  439,  390,
  441,  405,   -1,   -1,   -1,   -1,  331,   -1,  412,  413,
   -1,  415,  416,   -1,  405,  419,  420,  421,   -1,   -1,
   -1,  412,  413,   -1,  415,  416,   -1,  309,  419,  420,
  421,  356,  357,  437,   -1,  439,   -1,  441,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  437,   -1,  439,  331,
  441,  263,  264,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  390,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  356,  357,   -1,   -1,   -1,   -1,
  405,   -1,   -1,   -1,   -1,   -1,   -1,  412,  413,   -1,
  415,  416,   -1,   -1,  419,  420,  421,  309,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  390,   -1,
   -1,   -1,  437,   -1,  439,   -1,  441,   -1,   -1,  331,
   -1,   -1,   -1,  405,   -1,   -1,   -1,   -1,   -1,   -1,
  412,  413,   -1,  415,  416,   -1,   -1,  419,  420,  421,
   -1,   -1,   -1,   -1,  356,  357,   -1,   -1,   -1,   -1,
   -1,  257,   -1,   -1,   -1,  437,   -1,  439,   -1,  441,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  390,  285,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  297,  298,  405,   -1,   -1,   -1,   -1,   -1,   -1,
  412,  413,  308,  415,  416,   -1,   -1,  419,  420,  421,
   -1,  317,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  326,   -1,   -1,  257,  330,  437,   -1,  439,   -1,  441,
   -1,   -1,   -1,  339,  340,   -1,   -1,   -1,  344,   -1,
   -1,   -1,   -1,  349,  350,   -1,   -1,   -1,  354,   -1,
   -1,  285,  358,  359,  360,   -1,   -1,  363,   -1,  365,
  366,   -1,  368,  297,  298,   -1,   -1,   -1,   -1,   -1,
  376,  377,   -1,   -1,  308,   -1,   -1,   -1,   -1,   -1,
  386,   -1,   -1,  317,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  326,   -1,   -1,  257,  330,   -1,  404,   -1,
   -1,   -1,   -1,   -1,   -1,  339,  340,   -1,   -1,   -1,
  344,   -1,   -1,   -1,  420,  349,  350,   -1,   -1,   -1,
  354,   -1,   -1,  285,  358,  359,  360,   -1,   -1,  363,
   -1,  365,  366,   -1,  368,  297,  298,   -1,   -1,   -1,
   -1,   -1,  376,  377,   -1,   -1,  308,   -1,   -1,   -1,
   -1,   -1,  386,   -1,   -1,  317,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  326,   -1,   -1,  257,  330,   -1,
  404,   -1,   -1,   -1,   -1,   -1,   -1,  339,  340,   -1,
   -1,   -1,  344,   -1,   -1,   -1,  420,  349,  350,   -1,
   -1,   -1,  354,   -1,   -1,  285,  358,  359,  360,   -1,
   -1,  363,   -1,  365,  366,   -1,  368,  297,  298,   -1,
   -1,   -1,   -1,   -1,  376,  377,   -1,   -1,  308,   -1,
   -1,   -1,   -1,   -1,  386,   -1,   -1,  317,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  326,   -1,   -1,  257,
  330,   -1,  404,   -1,   -1,   -1,   -1,   -1,   -1,  339,
  340,   -1,   -1,   -1,  344,   -1,   -1,   -1,  420,  349,
  350,   -1,   -1,   -1,   -1,   -1,   -1,  285,  358,  359,
  360,   -1,   -1,  363,   -1,  365,  366,   -1,  368,  297,
  298,   -1,   -1,   -1,   -1,   -1,  376,  377,   -1,   -1,
  308,   -1,   -1,   -1,   -1,   -1,  386,   -1,   -1,  317,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  326,   -1,
   -1,  257,  330,   -1,  404,   -1,   -1,   -1,   -1,   -1,
   -1,  339,  340,   -1,   -1,   -1,  344,   -1,   -1,   -1,
  420,  349,  350,   -1,   -1,   -1,   -1,   -1,   -1,  285,
  358,  359,  360,   -1,   -1,  363,   -1,  365,  366,   -1,
  368,  297,  298,   -1,   -1,   -1,   -1,   -1,  376,  377,
   -1,   -1,  308,   -1,   -1,   -1,   -1,   -1,  386,   -1,
   -1,  317,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  326,   -1,   -1,  257,  330,   -1,  404,   -1,   -1,   -1,
   -1,   -1,   -1,  339,  340,   -1,   -1,   -1,  344,   -1,
   -1,   -1,  420,  349,  350,   -1,   -1,   -1,   -1,   -1,
   -1,  285,  358,  359,  360,   -1,   -1,  363,   -1,  365,
  366,   -1,  368,  297,  298,   -1,   -1,   -1,   -1,   -1,
  376,  377,  306,   -1,  308,   -1,   -1,   -1,   -1,   -1,
  386,   -1,   -1,  317,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  257,  330,   -1,  404,   -1,
   -1,   -1,   -1,   -1,   -1,  339,  340,   -1,   -1,   -1,
  344,   -1,   -1,   -1,  420,  349,  350,   -1,   -1,   -1,
   -1,   -1,   -1,  285,  358,  359,  360,   -1,   -1,  363,
   -1,  365,  366,   -1,  368,  297,  298,   -1,   -1,   -1,
   -1,   -1,  376,  377,   -1,   -1,  308,   -1,   -1,   -1,
   -1,   -1,  386,   -1,   -1,  317,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  257,  330,   -1,
  404,   -1,   -1,   -1,   -1,   -1,   -1,  339,  340,   -1,
   -1,   -1,  344,   -1,   -1,   -1,  420,  349,  350,   -1,
   -1,   -1,   -1,   -1,   -1,  285,  358,  359,  360,   -1,
   -1,  363,   -1,  365,  366,   -1,  368,  297,  298,   -1,
   -1,   -1,   -1,   -1,  376,  377,   -1,   -1,  308,   -1,
   -1,   -1,   -1,   -1,  386,   -1,   -1,  317,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  330,   -1,  404,   -1,   -1,   -1,   -1,   -1,   -1,  339,
  340,   -1,   -1,   -1,  344,   -1,   -1,   -1,  420,  349,
  350,   -1,   -1,   -1,   -1,   -1,   -1,  285,  358,  359,
  360,   -1,   -1,  363,   -1,  365,  366,   -1,  368,  297,
  298,   -1,   -1,   -1,   -1,   -1,  376,  377,  306,   -1,
  308,   -1,   -1,   -1,   -1,   -1,  386,   -1,   -1,  317,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  297,   -1,
   -1,   -1,  330,   -1,  404,   -1,   -1,  306,   -1,   -1,
   -1,  339,  340,   -1,   -1,   -1,  344,   -1,  317,  318,
  420,  349,  350,   -1,   -1,   -1,   -1,   -1,   -1,  328,
  358,  359,  360,   -1,   -1,  363,   -1,  365,  366,   -1,
  368,  340,   -1,   -1,   -1,   -1,   -1,   -1,  376,  377,
  349,  350,   -1,   -1,   -1,   -1,   -1,   -1,  386,  358,
  359,  360,   -1,  297,  363,  364,  365,  366,   -1,  368,
   -1,   -1,  306,   -1,   -1,   -1,  404,  376,  377,   -1,
   -1,   -1,   -1,  317,  318,   -1,   -1,   -1,  387,   -1,
   -1,   -1,  420,  297,  328,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,  306,   -1,   -1,  404,  340,   -1,   -1,   -1,
   -1,   -1,   -1,  317,  318,  349,  350,   -1,   -1,   -1,
   -1,  420,   -1,   -1,  358,  359,  360,   -1,  297,  363,
  364,  365,  366,   -1,  368,   -1,  340,  306,   -1,   -1,
   -1,   -1,  376,  377,   -1,  349,  350,   -1,  317,  318,
   -1,   -1,   -1,  387,  358,  359,  360,   -1,  297,  363,
  364,  365,  366,   -1,  368,   -1,   -1,  306,   -1,   -1,
  404,  340,  376,  377,   -1,   -1,   -1,   -1,  317,  318,
  349,  350,   -1,  387,   -1,   -1,  420,   -1,   -1,  358,
  359,  360,   -1,  297,  363,  364,  365,  366,   -1,  368,
  404,  340,  306,   -1,   -1,   -1,   -1,  376,  377,   -1,
  349,  350,   -1,  317,  318,   -1,  420,   -1,  387,  358,
  359,  360,   -1,  297,  363,  364,  365,  366,   -1,  368,
   -1,   -1,  306,   -1,   -1,  404,  340,  376,  377,   -1,
   -1,   -1,   -1,  317,  318,  349,  350,   -1,  387,   -1,
   -1,  420,   -1,   -1,  358,  359,  360,   -1,   -1,  363,
  364,  365,  366,   -1,  368,  404,  340,   -1,   -1,   -1,
   -1,   -1,  376,  377,   -1,  349,  350,   -1,   -1,   -1,
   -1,  420,   -1,  387,  358,  359,  360,   -1,   -1,  363,
  364,  365,  366,   -1,  368,   -1,  285,   -1,   -1,   -1,
  404,   -1,  376,  377,   -1,   -1,   -1,   -1,  297,  298,
   -1,   -1,   -1,  387,   -1,   -1,  420,  306,   -1,  308,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  317,   -1,
  404,   -1,   -1,   -1,   -1,   -1,  285,   -1,   -1,   -1,
   -1,  330,   -1,   -1,   -1,   -1,  420,   -1,  297,  298,
  339,  340,   -1,   -1,   -1,  344,   -1,  306,   -1,  308,
  349,  350,   -1,   -1,   -1,   -1,   -1,   -1,  317,  358,
  359,  360,   -1,   -1,  363,   -1,  365,  366,   -1,  368,
   -1,  330,   -1,   -1,   -1,   -1,   -1,  376,  377,   -1,
  339,  340,   -1,   -1,   -1,  344,   -1,  386,   -1,   -1,
  349,  350,   -1,   -1,   -1,   -1,   -1,   -1,  285,  358,
  359,  360,   -1,   -1,  363,  404,  365,  366,   -1,  368,
  297,  298,   -1,   -1,   -1,   -1,   -1,  376,  377,   -1,
   -1,  308,   -1,   -1,   -1,   -1,   -1,  386,   -1,   -1,
  317,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  330,   -1,  404,   -1,   -1,   -1,   -1,
   -1,   -1,  339,  340,   -1,   -1,   -1,  344,   -1,   -1,
   -1,   -1,  349,  350,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  358,  359,  360,   -1,   -1,  363,   -1,  365,  366,
   -1,  368,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  376,
  377,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  386,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  404,
  };

#line 4902 "mb-parser.jay"


Tokenizer lexer;

public Tokenizer Lexer {
	get {
		return lexer;
	}
}		   

public static Expression DecomposeQI (string name, Location loc)
{
	Expression o;

	if (name.IndexOf ('.') == -1){
		return new SimpleName (name, loc);
	} else {
		int pos = name.LastIndexOf (".");
		string left = name.Substring (0, pos);
		string right = name.Substring (pos + 1);

		o = DecomposeQI (left, loc);

		return new MemberAccess (o, right, loc);
	}
}

Block declare_local_variables (Expression dummy_type, ArrayList variable_declarators, Location loc)
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
	
	VariableDeclaration.FixupTypes (variable_declarators);
	// FIXME: Should VariableDeclaration.FixupArrayTypes be called here

	if (current_block.Used)
		implicit_block = new Block (current_block, Block.Flags.Implicit, loc, Location.Null);
	else
		implicit_block = current_block;


	foreach (VariableDeclaration decl in variable_declarators){
		Expression type = decl.type;
		if (implicit_block.AddVariable (type, decl.identifier, current_local_parameters, decl.Location) != null) {
			if (decl.expression_or_array_initializer != null){
				if (inits == null)
					inits = new ArrayList ();
				inits.Add (decl);
			}
		}
	}

	if (inits == null)
		return implicit_block;

	foreach (VariableDeclaration decl in inits){
		Assign assign;
		Expression expr;
		Expression type = decl.type;
		
		if ((decl.expression_or_array_initializer is Expression) || 
		    (decl.expression_or_array_initializer is New)) {
			expr = (Expression) decl.expression_or_array_initializer;
		} else {
			ArrayList init = (ArrayList) decl.expression_or_array_initializer;
			
			expr = new ArrayCreation (type, "", init, decl.Location);
		}

		LocalVariableReference var;
		var = new LocalVariableReference (implicit_block, decl.identifier, loc);

		assign = new Assign (var, expr, decl.Location);

		implicit_block.AddStatement (new StatementExpression (assign, lexer.Location));
	}
	
	return implicit_block;
}

Block declare_local_constant (Expression dummy_type, ArrayList variable_declarators)
{
	Block implicit_block;
	VariableDeclaration.FixupTypes (variable_declarators);

	if (current_block.Used)
		implicit_block = new Block (current_block, Block.Flags.Implicit);
	else
		implicit_block = current_block;

	foreach (VariableDeclaration decl in variable_declarators){
		Expression type = decl.type;
		implicit_block.AddConstant (type, decl.identifier, (Expression) decl.expression_or_array_initializer,
					  current_local_parameters, decl.Location);
	}
	
	return implicit_block;
}



struct VarName {
                public object Name;
                public object Type;
		public object Rank;
                                                                                
                public VarName (object n, object t, object r)
                {
                        Name = n;
                        Type = t;
			Rank = r;
                }
        }


// <summary>
//   A class used to pass around variable declarations and constants
// </summary>
public class VariableDeclaration {
	public string identifier;
	public object expression_or_array_initializer;
	public Location Location;
	public Attributes OptAttributes;
	public string DocComment;
	public Expression type;
	public ArrayList dims;
		
	public VariableDeclaration (string id, Expression t, object eoai, Location l, Attributes opt_attrs)
	{
		this.identifier = id;
		this.expression_or_array_initializer = eoai;
		this.Location = l;
		this.OptAttributes = opt_attrs;
		this.type = t;
		this.dims = null;
	}	

	public VariableDeclaration (string id, object eoai, Location l) : this (id, eoai, l, null)
	{
	}
	
	public VariableDeclaration (string id, Expression t, Location l) : this (id, t, null, l, null)
	{
	}	
	
	public VariableDeclaration (string id, object eoai, Location l, Attributes opt_attrs) :	this 
					(id, TypeManager.system_object_expr, eoai, l, opt_attrs)
	{
	}	
	
	public static ArrayCreation BuildArrayCreator (Expression vartype, ArrayList a_dims, ArrayList varinit, Location l)
	{	
		// FIXME : This is broken: only the first rank is parsed
		return new ArrayCreation (vartype, (ArrayList) a_dims[0], "", varinit, l);
	}
	
	public static void FixupTypes (ArrayList vars)
	{
		int varcount = 	vars.Count;
		VariableDeclaration last_var = (VariableDeclaration) vars[varcount - 1];
			
		if (last_var.type == null)
			last_var.type = TypeManager.system_object_expr;
			
		Expression cur_type = last_var.type;
		int n = varcount - 1;
		
		while (n >= 0) {
			VariableDeclaration var = (VariableDeclaration) vars[n--];
			if (var.type == null)
				var.type = cur_type;
			else
				cur_type = var.type;
		}
	}
	
	public static bool IndexesSpecifiedInRank (ArrayList IndexList)
	{
		bool res = false;
		
		if (IndexList != null) {
			foreach (Expression e in IndexList)
				if (!(e is EmptyExpression)) {
					res = true;
					break;
				}	
		}
		return (res);
	}	
	
	
	public static bool IndexesSpecified (ArrayList ranks)
	{
		bool res = false;
		
		if (ranks != null) {
			foreach (ArrayList IndexList in ranks) {
				if (IndexesSpecifiedInRank (IndexList)) {
					res = true;
					break;
				}	
			}	
		}
		return (res);
	}
	
	public static string StripDims (string varname, ref string d)
	{
		string res = varname;
		string dres = "";
		
		if (varname.IndexOf("[") >= 0) {
			dres = varname.Substring(varname.IndexOf("["), (varname.LastIndexOf("]") - varname.IndexOf("["))+1);
			res = varname.Substring(0, varname.IndexOf("["));
		}
		d = dres;
		return (res);
	}	
	
	public static string StripDims (string varname)
	{
		string dres = "";
		
		return (StripDims(varname, ref dres));
	}	
	
	public static string StripIndexesFromDims (string dims)
	{
		StringBuilder sb = new StringBuilder();

		foreach (char c in dims) 
			if (c == ',' || c == ']' || c == '[')
				sb.Append (c);
				
		return sb.ToString();				
	}
	
	public static string BuildRank (ArrayList rank)
	{
		bool allEmpty;
		return BuildRank(rank, out allEmpty);
	}
            
	public static string BuildRank (ArrayList rank, out bool allEmpty)
	{
		string res = "";

		res += "[";
		allEmpty = true;
		bool first = true;
		foreach (object e in rank) {
			if (!(e is EmptyExpression))
				allEmpty = false;
			if (!first)
				res += ",";
			first = false;
		}
			
		res += "]";
		return res;
	}
		
	public static string BuildRanks (ArrayList rank_specifiers, bool mustBeEmpty, Location loc)
	{
		string res = "";

		bool allEmpty = true;
		foreach (ArrayList rank in rank_specifiers) {
			bool tmp;
			res = BuildRank (rank, out tmp) + res;
			if (!tmp)
				allEmpty = false;
		}
		if (!allEmpty && mustBeEmpty)
			Report.Error (30638, loc, "Array bounds cannot appear in type specifiers.");	

		return res;
	}	
	
	public static void VBFixIndexList (ref ArrayList IndexList)
	{
		if (IndexList != null) {
			for (int x = 0; x < IndexList.Count; x++) {
				Expression e = (Expression) IndexList[x];
				if (!(e is EmptyExpression)) {
					IndexList[x] = new Binary (Binary.Operator.Addition, e, new IntLiteral(1), Location.Null);
				}
			}
		}
	}		
	
// 	public static bool IsArrayDecl (Parser t)
// 	{
// 		// return (varname.IndexOf("[") >= 0);
// 		return (t.current_rank_specifiers != null);
// 	}			
	
	public static void VBFixIndexLists (ref ArrayList ranks)
	{	
		if (ranks != null) {
			for (int x = 0; x < ranks.Count; x++) {
				ArrayList IndexList = (ArrayList) ranks[x];
				VBFixIndexList (ref IndexList);
			}	
		}	
	}
		
	public static void FixupArrayTypes (ArrayList vars)
	{
		int varcount = 	vars.Count;
		string dims;
		
		foreach (VariableDeclaration var in vars) {
		  	if (var.identifier.EndsWith(",")) {
		  		dims = "[" + var.identifier.Substring(var.identifier.IndexOf (","), 
		  						var.identifier.LastIndexOf(",")) + "]";
				var.identifier = var.identifier.Substring (0, var.identifier.IndexOf (","));
				var.type = new ComposedCast (var.type, (string) dims, var.Location);
		  	}
		}
	}				
}


// public Property BuildSimpleProperty (Expression p_type, string name, 
// 					Field p_fld, int mod_flags,
// 			 		Attributes attrs, Location loc) 
// {
// 	Property p;
// 	Block get_block, set_block;
// 	Accessor acc_set, acc_get;
// 	StatementExpression a_set;
// 	Statement a_get;
// 	Parameter [] args;
	
// 	// Build SET Block
// 	Parameter implicit_value_parameter = new Parameter (p_type, "value", Parameter.Modifier.NONE, null);	
// 	args  = new Parameter [1];
// 	args [0] = implicit_value_parameter;
		
// 	Parameters set_params = new Parameters (args, null, loc);
// 	a_set = new StatementExpression ((ExpressionStatement) new Assign ((Expression) DecomposeQI(p_fld.Name, loc), 
// 			    (Expression) new SimpleName("value", loc), loc), loc);
			    
// 	set_block = new Block (current_block, set_params, loc, Location.Null);
// 	set_block.AddStatement ((Statement) a_set);					    
// 	acc_set = new Accessor (set_block, attrs);
	
// 	// Build GET Block
// 	a_get = (Statement) new Return ((Expression) DecomposeQI(p_fld.Name, loc), loc);
// 	get_block = new Block (current_block, null, loc, Location.Null);
// 	get_block.AddStatement ((Statement) a_get);					    
// 	acc_get = new Accessor (get_block, attrs);
		
// 	p = new Property (p_type, name, mod_flags, (Accessor) acc_get, (Accessor) acc_set, attrs, loc);
	
// 	return (p);
// }
	
void start_block () 
{
	  if (current_block == null){
		  current_block = new ToplevelBlock ((ToplevelBlock) top_current_block, 
					     current_local_parameters, lexer.Location);
		  top_current_block = current_block;
	  } else {
		  current_block = new Block (current_block, current_local_parameters,
						 lexer.Location, Location.Null);
	  }
} 


Block end_block ()
{ 
	Block res;

	while (current_block.Implicit)
		current_block = current_block.Parent;
	res = current_block;
	current_block.SetEndLocation (lexer.Location);
	current_block = current_block.Parent;
	if (current_block == null)
		top_current_block = null;

	return res;
}

// private void AddHandler (Expression evt_definition, Expression handler_exp)
// {
// 	AddHandler (current_block, evt_definition, handler_exp);
// }

void CheckAttributeTarget (string a)
{
	switch (a) {

	case "assembly" : case "field" : case "method" : case "param" : case "property" : case "type" :
		return;
		
	default :
		Location l = lexer.Location;
		Report.Error (658, l, "`" + a + "' is an invalid attribute target");
		break;
	}
}

// private void AddHandler (Block b, Expression evt_id, Expression handles_exp)
// {
// 	Expression evt_target;
// 	Location loc = lexer.Location;
	
// 	Statement addhnd = (Statement) new AddHandler (evt_id, 
// 													handles_exp, 
// 													loc);													
													
// 	b.AddStatement (addhnd);
// }

// private void RaiseEvent (string evt_name, ArrayList args)
// {
// 	Location loc = lexer.Location;
	
// 	Invocation evt_call = new Invocation (DecomposeQI(evt_name, loc), args, lexer.Location);
//    	Statement s = (Statement)(new StatementExpression ((ExpressionStatement) evt_call, loc)); 
//    	current_block.AddStatement (s);	
// }

// private void RemoveHandler (Block b, Expression evt_definition, Expression handler_exp)
// {
// 	Expression evt_target;
// 	Location loc = lexer.Location;
	
// 	Statement rmhnd = (Statement) new RemoveHandler (evt_definition, 
// 													handler_exp, 
// 													loc);
// 	b.AddStatement (rmhnd);
// }

// <summary>
//  This method is used to get at the complete string representation of
//  a fully-qualified type name, hiding inside a MemberAccess ;-)
//  This is necessary because local_variable_type admits primary_expression
//  as the type of the variable. So we do some extra checking
// </summary>
string GetQualifiedIdentifier (Expression expr)
{
	if (expr is SimpleName)
		return ((SimpleName)expr).Name;
	else if (expr is MemberAccess)
		return GetQualifiedIdentifier (((MemberAccess)expr).Expr) + "." + ((MemberAccess) expr).Identifier;
	else 
		throw new Exception ("Expr has to be either SimpleName or MemberAccess! (" + expr + ")");
	
}

// private void RemoveHandler (Expression evt_definition, Expression handler_exp)
// {
// 	RemoveHandler (current_block, evt_definition, handler_exp);
// }

// FIXME: This needs to be fixed for This and Base access because the way the name of the
// mbas' constructor is changed from "New" to current_container.Basename

private ConstructorInitializer CheckConstructorInitializer (ref ArrayList s)
{
	ConstructorInitializer ci = null;
	
	if (s.Count > 0) {
		if (s[0] is StatementExpression && ((StatementExpression) s[0]).expr is Invocation) {
			Invocation i = (Invocation) ((StatementExpression) s[0]).expr;
			
			if (i.expr is BaseAccess) {
				BaseAccess ba = (BaseAccess) i.expr;
				if (ba.member == "New" || ba.member == ".ctor") {
					ci = new ConstructorBaseInitializer (i.Arguments, current_local_parameters, lexer.Location);
					s.RemoveAt(0);
				}
			}
			if (i.expr.ToString() == "Mono.MonoBASIC.This..ctor") {
				ci = new ConstructorThisInitializer (i.Arguments, current_local_parameters, lexer.Location); 
				s.RemoveAt(0);
			}
		}
	}
	return ci;
}

void Error_ExpectingTypeName (Location l, Expression expr)
{
	if (expr is Invocation){
		Report.Error (1002, l, "; expected");
	} else {
		Report.Error (-1, l, "Invalid Type definition");
	}
}

static bool AlwaysAccept (MemberInfo m, object filterCriteria) {
	return true;
}

private void ReportError9998()
{
	Report.Error (29998, lexer.Location, "This construct is only available in MonoBASIC extended syntax.");
}

public CSharpParser (SeekableStreamReader reader, SourceFile file, ArrayList defines)
{
	current_namespace = new NamespaceEntry (null, file, null, Location.Null);
	this.name = file.Name;
	this.file = file;
	current_container = RootContext.Tree.Types;
	current_container.NamespaceEntry = current_namespace;
	oob_stack = new Stack ();
	switch_stack = new Stack ();

	lexer = new Tokenizer (reader, file, defines);

	ifElseStateMachine = new IfElseStateMachine();
	tokenizerController = new TokenizerController(lexer);
}

public void parse ()
{
	try {
		if (yacc_verbose_flag > 1)
			yyparse (lexer, new yydebug.yyDebugSimple ());
		else
			yyparse (lexer);
		Tokenizer tokenizer = lexer as Tokenizer;
		tokenizer.cleanup ();		
	} catch (Exception e){
		// 
		// Removed for production use, use parser verbose to get the output.
		//
		// Console.WriteLine (e);
		Report.Error (-25, lexer.Location, "Parsing error");
		if (yacc_verbose_flag > 0)
			Console.WriteLine (e);
	}
}


// protected override int parse ()
// {
// 	RootContext.InitializeImports(ImportsList);
// 	current_namespace = new Namespace (null, RootContext.RootNamespace);
// 	current_container = RootContext.Tree.Types;
// 	current_container.Namespace = current_namespace;
// 	oob_stack = new Stack ();
// 	switch_stack = new Stack ();
// 	expr_stack = new Stack ();	
// 	tmp_blocks = new Stack(); 
// 	with_stack = new Stack();
// 	statement_stack = new Stack(); 	

// 	allow_global_attribs = true;
// 	expecting_global_attribs = false;
// 	expecting_local_attribs = false;
// 	local_attrib_section_added = false;

// 	UseExtendedSyntax = name.EndsWith(".mbs");
// 	OptionExplicit = InitialOptionExplicit || UseExtendedSyntax;
// 	OptionStrict = InitialOptionStrict || UseExtendedSyntax;
// 	OptionCompareBinary = InitialOptionCompareBinary;

// 	lexer = new Tokenizer (input, name, defines);
	
// 	ifElseStateMachine = new IfElseStateMachine();
// 	tokenizerController = new TokenizerController(lexer);
	
// 	StringBuilder value = new StringBuilder ();
// 	try {
// 		if (yacc_verbose_flag > 0)
// 			yyparse (lexer, new yydebug.yyDebugSimple ());
// 		else {
// 			yyparse (lexer);
// 			cleanup();
// 		}
// 	} 
// 	catch(MBASException e) {
// 		Report.Error(e.code, e.loc, e.Message);
// 	}
// 	catch (Exception e) {
// 		if (Report.Stacktrace)
// 			Console.WriteLine(e);
// 		Report.Error (29999, lexer.Location, "Parsing error");
// 	}

// 	RootContext.VerifyImports();

// 	return Report.Errors;
// }

// void cleanup()
// {
// 	try {
// 		ifElseStateMachine.HandleToken(IfElseStateMachine.Token.EOF);
// 	}
// 	catch(ApplicationException) {
// 		throw new MBASException(ifElseStateMachine.Error, lexer.Location, ifElseStateMachine.ErrString);
// 	}

// 	if(in_external_source) 
// 		Report.Error (30579, lexer.Location, "'#ExternalSource' directives must end with matching '#End ExternalSource'");

// 	if(in_marked_region > 0)
// 		Report.Error (30205, lexer.Location, "'#Region' directive must be followed by a matching '#End Region'");
// }

void HandleConditionalDirective(IfElseStateMachine.Token tok, BoolLiteral expr)
{
	try {
		tokenizerController.PositionTokenizerCursor(tok, expr);
	}
	catch(ApplicationException) {
		tok = IfElseStateMachine.Token.EOF;
		try {
			ifElseStateMachine.HandleToken(tok);
		}
		catch(ApplicationException) {
			throw new MBASException(ifElseStateMachine.Error, lexer.Location, ifElseStateMachine.ErrString);
		}
	}
}
/* end end end */

// <summary>
//   Given the @class_name name, it creates a fully qualified name
//   based on the containing declaration space
// </summary>
MemberName
MakeName (MemberName class_name)
{
	string ns = current_namespace.FullName;

	if (current_container.Name == ""){
		if (ns != "")
			return new MemberName (new MemberName (ns), class_name);
		else
			return class_name;
	} else {
		return new MemberName (current_container.MemberName, class_name);
	}
}

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
  public const int BINARY = 269;
  public const int BOOLEAN = 270;
  public const int BYREF = 271;
  public const int BYTE = 272;
  public const int BYVAL = 273;
  public const int CALL = 274;
  public const int CASE = 275;
  public const int CATCH = 276;
  public const int CBOOL = 277;
  public const int CBYTE = 278;
  public const int CCHAR = 279;
  public const int CDATE = 280;
  public const int CDEC = 281;
  public const int CDBL = 282;
  public const int CHAR = 283;
  public const int CINT = 284;
  public const int CLASS = 285;
  public const int CLNG = 286;
  public const int COBJ = 287;
  public const int COMPARE = 288;
  public const int CONST = 289;
  public const int CSHORT = 290;
  public const int CSNG = 291;
  public const int CSTR = 292;
  public const int CTYPE = 293;
  public const int DATE = 294;
  public const int DECIMAL = 295;
  public const int DECLARE = 296;
  public const int DEFAULT = 297;
  public const int DELEGATE = 298;
  public const int DIM = 299;
  public const int DIRECTCAST = 300;
  public const int DO = 301;
  public const int DOUBLE = 302;
  public const int EACH = 303;
  public const int ELSE = 304;
  public const int ELSEIF = 305;
  public const int END = 306;
  public const int ENDIF = 307;
  public const int ENUM = 308;
  public const int EOL = 309;
  public const int ERASE = 310;
  public const int EVENT = 311;
  public const int EXIT = 312;
  public const int EXPLICIT = 313;
  public const int FALSE = 314;
  public const int FINALLY = 315;
  public const int FOR = 316;
  public const int FRIEND = 317;
  public const int FUNCTION = 318;
  public const int GET = 319;
  public const int GETTYPE = 320;
  public const int GOSUB = 321;
  public const int GOTO = 322;
  public const int HANDLES = 323;
  public const int IF = 324;
  public const int IMPLEMENTS = 325;
  public const int IMPORTS = 326;
  public const int IN = 327;
  public const int INHERITS = 328;
  public const int INTEGER = 329;
  public const int INTERFACE = 330;
  public const int IS = 331;
  public const int LET = 332;
  public const int LIB = 333;
  public const int LIKE = 334;
  public const int LONG = 335;
  public const int LOOP = 336;
  public const int ME = 337;
  public const int MOD = 338;
  public const int MODULE = 339;
  public const int MUSTINHERIT = 340;
  public const int MUSTOVERRIDE = 341;
  public const int MYBASE = 342;
  public const int MYCLASS = 343;
  public const int NAMESPACE = 344;
  public const int NEW = 345;
  public const int NEXT = 346;
  public const int NOT = 347;
  public const int NOTHING = 348;
  public const int NOTINHERITABLE = 349;
  public const int NOTOVERRIDABLE = 350;
  public const int OBJECT = 351;
  public const int OFF = 352;
  public const int ON = 353;
  public const int OPTION = 354;
  public const int OPTIONAL = 355;
  public const int OR = 356;
  public const int ORELSE = 357;
  public const int OVERLOADS = 358;
  public const int OVERRIDABLE = 359;
  public const int OVERRIDES = 360;
  public const int PARAM_ARRAY = 361;
  public const int PRESERVE = 362;
  public const int PRIVATE = 363;
  public const int PROPERTY = 364;
  public const int PROTECTED = 365;
  public const int PUBLIC = 366;
  public const int RAISEEVENT = 367;
  public const int READONLY = 368;
  public const int REDIM = 369;
  public const int REM = 370;
  public const int REMOVEHANDLER = 371;
  public const int RESUME = 372;
  public const int RETURN = 373;
  public const int SELECT = 374;
  public const int SET = 375;
  public const int SHADOWS = 376;
  public const int SHARED = 377;
  public const int SHORT = 378;
  public const int SINGLE = 379;
  public const int SIZEOF = 380;
  public const int STATIC = 381;
  public const int STEP = 382;
  public const int STOP = 383;
  public const int STRICT = 384;
  public const int STRING = 385;
  public const int STRUCTURE = 386;
  public const int SUB = 387;
  public const int SYNCLOCK = 388;
  public const int TEXT = 389;
  public const int THEN = 390;
  public const int THROW = 391;
  public const int TO = 392;
  public const int TRUE = 393;
  public const int TRY = 394;
  public const int TYPEOF = 395;
  public const int UNICODE = 396;
  public const int UNTIL = 397;
  public const int VARIANT = 398;
  public const int WEND = 399;
  public const int WHEN = 400;
  public const int WHILE = 401;
  public const int WITH = 402;
  public const int WITHEVENTS = 403;
  public const int WRITEONLY = 404;
  public const int XOR = 405;
  public const int YIELD = 406;
  public const int HASH = 407;
  public const int OPEN_BRACKET = 408;
  public const int CLOSE_BRACKET = 409;
  public const int OPEN_PARENS = 410;
  public const int OPEN_BRACE = 411;
  public const int CLOSE_BRACE = 412;
  public const int CLOSE_PARENS = 413;
  public const int DOT = 414;
  public const int COMMA = 415;
  public const int COLON = 416;
  public const int PLUS = 417;
  public const int MINUS = 418;
  public const int ASSIGN = 419;
  public const int OP_LT = 420;
  public const int OP_GT = 421;
  public const int STAR = 422;
  public const int DIV = 423;
  public const int OP_EXP = 424;
  public const int INTERR = 425;
  public const int OP_IDIV = 426;
  public const int OP_CONCAT = 427;
  public const int EXCLAMATION = 428;
  public const int PERCENT = 429;
  public const int LONGTYPECHAR = 430;
  public const int AT_SIGN = 431;
  public const int SINGLETYPECHAR = 432;
  public const int NUMBER_SIGN = 433;
  public const int DOLAR_SIGN = 434;
  public const int ATTR_ASSIGN = 435;
  public const int OP_LE = 437;
  public const int OP_GE = 439;
  public const int OP_NE = 441;
  public const int OP_XOR = 443;
  public const int xor = 444;
  public const int OP_SHIFT_LEFT = 445;
  public const int OP_SHIFT_RIGHT = 447;
  public const int LITERAL_INTEGER = 449;
  public const int LITERAL_SINGLE = 451;
  public const int LITERAL_DOUBLE = 453;
  public const int LITERAL_DECIMAL = 455;
  public const int LITERAL_CHARACTER = 457;
  public const int LITERAL_STRING = 459;
  public const int LITERAL_DATE = 461;
  public const int IDENTIFIER = 463;
  public const int LOWPREC = 464;
  public const int OP_OR = 465;
  public const int OP_AND = 466;
  public const int BITWISE_OR = 467;
  public const int BITWISE_AND = 468;
  public const int BITWISE_NOT = 469;
  public const int CARRET = 470;
  public const int UMINUS = 471;
  public const int OP_INC = 472;
  public const int OP_DEC = 473;
  public const int HIGHPREC = 474;
  public const int label_name = 475;
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
