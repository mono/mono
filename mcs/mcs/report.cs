//
// report.cs: report errors and warnings.
//
// Author: Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc. (http://www.ximian.com)
//

//
// FIXME: currently our class library does not support custom number format strings
//
using System;
using System.Text;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Reflection;

namespace Mono.CSharp {

	public enum Message: int {
		CS_24_The_Microsoft_Runtime_cannot_set_this_marshal_info = -24,
		CS_28_The_Microsoft_NET_Runtime_does_not_permit_setting_custom_attributes_on_the_return_type = -28,
		CS0028_The_wrong_signature_to_be_an_entry_point = 28,
		CS0067_The_event_is_never_used = 67,
		CS0072_Event_can_override_only_event = 72,
		CS0078_The_l_suffix_is_easily_confused_with_the_digit_1 = 78,
		CS0105_The_using_directive_for_appeared_previously_in_this_namespace = 105,
		CS0108_The_keyword_new_is_required = 108,
		CS0109_The_member_does_not_hide_an_inherited_member_new_keyword_is_not_required = 109,
		CS0111_Type_already_defines_member_with_the_same_parameter_types = 111,
		CS0114_Hides_inherited_member = 114,
		CS0115_No_suitable_methods_found_to_override = 115,
		CS0122_is_inaccessible_due_to_its_protection_level = 122,
		CS0134_Cannot_use_qualified_namespace_names_in_nested_namespace_declarations = 134,
		CS0145_A_const_field_requires_a_value_to_be_provided = 145,
		CS0160_A_previous_catch_clause_already_catches_all_exceptions_of_this_or_a_super_type = 160,
		CS0162_Unreachable_code_detected = 162,
		CS0168_The_variable_is_declared_but_never_used = 168,
		CS0169_The_private_field_is_never_used = 169,
		CS0183_The_given_expression_is_always_of_the_provided_type = 183,
		CS0184_The_given_expression_is_never_of_the_provided_type = 184,
		CS0210_You_must_provide_an_initializer_in_a_fixed_or_using_statement_declaration = 210,
		CS0219_The_variable_is_assigned_but_its_value_is_never_used = 219,
		CS0243_Conditional_not_valid_on_because_it_is_an_override_method = 243,
		CS0247_Cannot_use_a_negative_size_with_stackalloc = 247,
		CS0415_The_IndexerName_attribute_is_valid_only_on_an_indexer_that_is_not_an_explicit_interface_member_declaration = 415,
		CS0502_cannot_be_both_abstract_and_sealed = 502,
		CS0553_user_defined_conversion_to_from_base_class = 553,
		CS0554_user_defined_conversion_to_from_derived_class = 554,
		CS0577_Conditional_not_valid_on_because_it_is_a_destructor,_operator,_or_explicit_interface_implementation = 557,
		CS0578_Conditional_not_valid_on_because_its_return_type_is_not_void = 578,
		CS0582_Conditional_not_valid_on_interface_members = 582,
		CS0592_Attribute_is_not_valid_on_this_declaration_type = 592,
		CS0601_The_DllImport_attribute_must_be_specified_on_a_method_marked_static_and_extern = 601,
		CS0609_Cannot_set_the_IndexerName_attribute_on_an_indexer_marked_override = 609,
		CS0610_Field_or_property_cannot_be_of_type = 610,
		CS0612_is_obsolete = 612,
		CS0619_error_is_obsolete = 619,
		CS0618_warning_is_obsolete = 618,
		CS0626_Method_operator_or_accessor_is_marked_external_and_has_no_attributes_on_it = 626,
		CS0628_New_protected_member_declared_in_sealed_class = 628,
		CS0629_Conditional_member_cannot_implement_interface_member = 629,
		CS0633_The_argument_to_the_IndexerName_attribute_must_be_a_valid_identifier = 633,
		CS0642_Possible_mistaken_empty_statement = 642,
		CS0649_Field_is_never_assigned_to_and_will_always_have_its_default_value = 649,
		CS0657_is_not_a_valid_attribute_location_for_this_declaration = 657,
		CS0659_overrides_Equals_but_does_not_override_GetHashCode = 659,
		CS0660_defines_operator_but_does_not_override_Equals = 660,
		CS0661_defines_operator_but_does_not_override_GetHashCode = 661,
		CS0668_Two_indexers_have_different_names = 668,
		CS0672_Member_overrides_obsolete_member = 672,
		CS1030_warning = 1030,
		CS1555_Could_not_find_specified_for_Main_method = 1555,
		CS1556_specified_for_Main_method_must_be_a_valid_class_or_struct = 1556, 
		CS1616_Option_overrides_options_given_in_source = 1616,
		CS1618_Cannot_create_delegate_with_because_it_has_a_Conditional_attribute = 1618,
		CS1667_is_not_valid_on_property_or_event_accessors = 1667,
		CS1669__arglist_is_not_valid_in_this_context = 1669,
		CS2002_Source_file_specified_multiple_times = 2002,
		CS3000_Methods_with_variable_arguments_are_not_CLS_compliant = 3000,
		CS3001_Argument_type_is_not_CLS_compliant = 3001, 
		CS3002_Return_type_of_is_not_CLS_compliant = 3002,
		CS3003_Type_is_not_CLS_compliant = 3003, 
		CS3005_Identifier_differing_only_in_case_is_not_CLS_compliant = 3005,
		CS3006_Overloaded_method_differing_only_in_ref_or_out_or_in_array_rank_is_not_CLS_compliant = 3006, 
		CS3008_Identifier_is_not_CLS_compliant = 3008,
		CS3009_base_type_is_not_CLS_compliant = 3009,
		CS3010_CLS_compliant_interfaces_must_have_only_CLScompliant_members = 3010, 
		CS3011_only_CLS_compliant_members_can_be_abstract = 3011,
		CS3012_You_must_specify_the_CLSCompliant_attribute_on_the_assembly_not_the_module_to_enable_CLS_compliance_checking = 3012,
		CS3013_Added_modules_must_be_marked_with_the_CLSCompliant_attribute_to_match_the_assembly = 3013,
		CS3014_cannot_be_marked_as_CLS_compliant_because_the_assembly_does_not_have_a_CLSCompliant_attribute = 3014,
		CS3015_has_no_accessible_constructors_which_use_only_CLS_compliant_types = 3015,
		CS3016_Arrays_as_attribute_arguments_are_not_CLS_compliant = 3016,
		CS3019_CLS_compliance_checking_will_not_be_performed_on_because_it_is_private_or_internal = 3019
	}

	/// <summary>
	///   This class is used to report errors and warnings t te user.
	/// </summary>
	public class Report {
		/// <summary>  
		///   Errors encountered so far
		/// </summary>
		static public int Errors;

		/// <summary>  
		///   Warnings encountered so far
		/// </summary>
		static public int Warnings;

		/// <summary>  
		///   Whether errors should be throw an exception
		/// </summary>
		static public bool Fatal;
		
		/// <summary>  
		///   Whether warnings should be considered errors
		/// </summary>
		static public bool WarningsAreErrors;

		/// <summary>  
		///   Whether to dump a stack trace on errors. 
		/// </summary>
		static public bool Stacktrace;
		
		//
		// If the 'expected' error code is reported then the
                // compilation succeeds.
		//
		// Used for the test suite to excercise the error codes
		//
		static int expected_error = 0;

		//
		// Keeps track of the warnings that we are ignoring
		//
		static Hashtable warning_ignore_table;

		/// <summary>
		/// List of symbols related to reported error/warning. You have to fill it before error/warning is reported.
		/// </summary>
		static StringCollection related_symbols = new StringCollection ();

		abstract class MessageData {
			protected readonly string Message;

			public MessageData (string text)
			{
				Message = text;
			}

			static void Check (int code)
			{
				if (code == expected_error) {
					Environment.Exit (0);
				}
			}

			string Format (params object[] args)
			{
				return String.Format (Message, args);
			}

			public abstract string MessageType { get; }

			public virtual void Print (Message msg_id, string location, params object[] args)
			{
				int code = (int)msg_id;

				if (code < 0)
					code = 8000-code;

				string text = Format (args);
				string msg = String.Format ("{0} {1} CS{2:0000}: {3}", location, MessageType, code, text);
				Console.WriteLine (msg);

				foreach (string s in related_symbols) {
					Console.WriteLine (s + MessageType + ')');
				}
				related_symbols.Clear ();

				if (Stacktrace)
					Console.WriteLine (FriendlyStackTrace (new StackTrace (true)));

				if (Fatal)
					throw new Exception (text);

				Check (code);
			}
		}

		class WarningData: MessageData {
			public WarningData (int level, string text):
				base (text) {
				Level = level;
			}

			bool IsEnabled (int code)
			{
				if (RootContext.WarningLevel < Level)
					return false;

				if (warning_ignore_table != null) {
					if (warning_ignore_table.Contains (code)) {
						return false;
					}
				}
				return true;
			}

			public override void Print(Message msg_id, string location, params object[] args)
			{
				int code = (int)msg_id;
				if (!IsEnabled (code)) {
					related_symbols.Clear ();
					return;
				}

				if (WarningsAreErrors) {
					new ErrorData (Message).Print (msg_id, location, args);
					return;
				}

				Warnings++;
				base.Print (msg_id, location, args);
			}

			public override string MessageType {
				get {
					return "warning";
				}
			}

			readonly int Level;
		}

		class ErrorData: MessageData {
			public ErrorData (string text):
				base (text)
			{
			}

			public override void Print(Message msg, string location, params object[] args)
			{
				Errors++;
				base.Print (msg, location, args);
			}

			public override string MessageType {
				get {
					return "error";
				}
			}

		}

		static MessageData GetErrorMsg (Message msg)
		{
			switch ((int)msg) {
				case -24: return new WarningData (1, "The Microsoft Runtime cannot set this marshal info. Please use the Mono runtime instead.");
				case -28: return new WarningData (1, "The Microsoft .NET Runtime 1.x does not permit setting custom attributes on the return type");
				case 0028: return new WarningData (4, "'{0}' has the wrong signature to be an entry point");
				case 0067: return new WarningData (3, "The event '{0}' is never used");
				case 0072: return new ErrorData ("Event '{0}' can override only event");
				case 0078: return new WarningData (4, "The 'l' suffix is easily confused with the digit '1' (use 'L' for clarity)");
				case 0105: return new WarningData (3, "The using directive for '{0}' appeared previously in this namespace");
				case 0108: return new WarningData (1, "The keyword new is required on '{0}' because it hides inherited member");
				case 0109: return new WarningData (4, "The member '{0}' does not hide an inherited member. The new keyword is not required");
				case 0111: return new ErrorData ("Type '{0}' already defines a member called '{1}' with the same parameter types");
				case 0114: return new WarningData (2, "'{0}' hides inherited member '{1}'. To make the current member override that implementation, add the override keyword. Otherwise add the new keyword");
				case 0115: return new ErrorData ("'{0}': no suitable methods found to override");
 				case 0122: return new ErrorData ("'{0}' is inaccessible due to its protection level");
 				case 0134: return new ErrorData ("Cannot use qualified namespace names in nested namespace declarations");
				case 0145: return new ErrorData ("A const field requires a value to be provided");
				case 0160: return new ErrorData ("A previous catch clause already catches all exceptions of this or a super type '{0}'");
				case 0162: return new WarningData (2, "Unreachable code detected");
				case 0168: return new WarningData (3, "The variable '{0}' is declared but never used");
				case 0169: return new WarningData (3, "The private field '{0}' is never used");
				case 0183: return new WarningData (1, "The given expression is always of the provided ('{0}') type");
				case 0184: return new WarningData (1, "The given expression is never of the provided ('{0}') type");
				case 0210: return new ErrorData ("You must provide an initializer in a fixed or using statement declaration");
				case 0219: return new WarningData (3, "The variable '{0}' is assigned but its value is never used");
 				case 0243: return new ErrorData ("Conditional not valid on '{0}' because it is an override method");
				case 0247: return new ErrorData ("Cannot use a negative size with stackalloc");
				case 0415: return new ErrorData ("The 'IndexerName' attribute is valid only on an indexer that is not an explicit interface member declaration");
				case 0502: return new ErrorData ("'{0}' cannot be both abstract and sealed");
 				case 0553: return new ErrorData ("'{0}' : user defined conversion to/from base class");
 				case 0554: return new ErrorData ("'{0}' : user defined conversion to/from derived class");
 				case 0577: return new ErrorData ("Conditional not valid on '{0}' because it is a destructor, operator, or explicit interface implementation");
 				case 0578: return new ErrorData ("Conditional not valid on '{0}' because its return new ErrorData ( type is not void");
 				case 0582: return new ErrorData ("Conditional not valid on interface members");
				case 0592: return new ErrorData ("Attribute '{0}' is not valid on this declaration type. It is valid on {1} declarations only.");
				case 0601: return new ErrorData ("The DllImport attribute must be specified on a method marked `static' and `extern'");
				case 0609: return new ErrorData ("Cannot set the 'IndexerName' attribute on an indexer marked override");
				case 0610: return new ErrorData ("Field or property cannot be of type '{0}'");
				case 0612: return new WarningData (1, "'{0}' is obsolete");
				case 0618: return new WarningData (2, "'{0}' is obsolete: '{1}'");
				case 0619: return new ErrorData ("'{0}' is obsolete: '{1}'");
				case 0626: return new ErrorData ("Method, operator, or accessor '{0}' is marked external and has no attributes on it. Consider adding a DllImport attribute to specify the external implementation");
				case 0628: return new WarningData (4, "'{0}': new protected member declared in sealed class");
 				case 0629: return new ErrorData ("Conditional member '{0}' cannot implement interface member");
				case 0633: return new ErrorData ("The argument to the 'IndexerName' attribute must be a valid identifier");
				case 0642: return new WarningData (3, "Possible mistaken empty statement");
				case 0649: return new WarningData (4, "Field '{0}' is never assigned to, and will always have its default value '{1}'");
				case 0657: return new ErrorData ("'{0}' is not a valid attribute location for this declaration. Valid attribute locations for this declaration are '{1}'");
				case 0659: return new WarningData (3, "'{0}' overrides Object.Equals(object) but does not override Object.GetHashCode()");
				case 0660: return new WarningData (3, "'{0}' defines operator == or operator != but does not override Object.Equals(object o)");
				case 0661: return new WarningData (3, "'{0}' defines operator == or operator != but does not override Object.GetHashCode()");
				case 0668: return new ErrorData ("Two indexers have different names; the IndexerName attribute must be used with the same name on every indexer within a type");
				case 0672: return new WarningData (1, "Member '{0}' overrides obsolete member. Add the Obsolete attribute to '{0}'");
				case 1030: return new WarningData (1, "#warning: '{0}'");
				case 1555: return new ErrorData ("Could not find '{0}' specified for Main method");
				case 1556: return new ErrorData ("'{0}' specified for Main method must be a valid class or struct");                                    
				case 1616: return new WarningData (1, "Compiler option '{0}' overrides '{1}' given in source");
 				case 1618: return new ErrorData ("Cannot create delegate with '{0}' because it has a Conditional attribute");
				case 1667: return new ErrorData ("'{0}' is not valid on property or event accessors. It is valid on '{1}' declarations only");
				case 1669: return new ErrorData ("__arglist is not valid in this context");
				case 2002: return new WarningData (1, "Source file '{0}' specified multiple times");
				case 3000: return new ErrorData ("Methods with variable arguments are not CLS-compliant");
				case 3001: return new ErrorData ("Argument type '{0}' is not CLS-compliant");
				case 3002: return new ErrorData ("return new ErrorData ( type of '{0}' is not CLS-compliant");
				case 3003: return new ErrorData ("Type of '{0}' is not CLS-compliant");
				case 3005: return new ErrorData ("Identifier '{0}' differing only in case is not CLS-compliant");
				case 3006: return new ErrorData ("Overloaded method '{0}' differing only in ref or out, or in array rank, is not CLS-compliant");
				case 3008: return new ErrorData ("Identifier '{0}' is not CLS-compliant");
				case 3009: return new ErrorData ("'{0}': base type '{1}' is not CLS-compliant");
				case 3010: return new ErrorData ("'{0}': CLS-compliant interfaces must have only CLS-compliant members");
				case 3011: return new ErrorData ("'{0}': only CLS-compliant members can be abstract");
				case 3012: return new WarningData (1, "You must specify the CLSCompliant attribute on the assembly, not the module, to enable CLS compliance checking");
				case 3013: return new ErrorData ("Added modules must be marked with the CLSCompliant attribute to match the assembly");
				case 3014: return new ErrorData ("'{0}' cannot be marked as CLS-compliant because the assembly does not have a CLSCompliant attribute");
				case 3015: return new ErrorData ("'{0}' has no accessible constructors which use only CLS-compliant types");
				case 3016: return new ErrorData ("Arrays as attribute arguments are not CLS-compliant");
				case 3019: return new WarningData (2, "CLS compliance checking will not be performed on '{0}' because it is private or internal");
			}
			throw new InternalErrorException (String.Format ("Missing message '{0}' text", msg));
		}		

		static void Check (int code)
		{
			if (code == expected_error){
				if (Fatal)
					throw new Exception ();
				
				Environment.Exit (0);
			}
		}
		
		public static string FriendlyStackTrace (Exception e)
		{
			return FriendlyStackTrace (new StackTrace (e, true));
		}
		
		static string FriendlyStackTrace (StackTrace t)
		{		
			StringBuilder sb = new StringBuilder ();
			
			bool foundUserCode = false;
			
			for (int i = 0; i < t.FrameCount; i++) {
				StackFrame f = t.GetFrame (i);
				MethodBase mb = f.GetMethod ();
				
				if (!foundUserCode && mb.ReflectedType == typeof (Report))
					continue;
				
				foundUserCode = true;
				
				sb.Append ("\tin ");
				
				if (f.GetFileLineNumber () > 0)
					sb.AppendFormat ("(at {0}:{1}) ", f.GetFileName (), f.GetFileLineNumber ());
				
				sb.AppendFormat ("{0}.{1} (", mb.ReflectedType.Name, mb.Name);
				
				bool first = true;
				foreach (ParameterInfo pi in mb.GetParameters ()) {
					if (!first)
						sb.Append (", ");
					first = false;
					
					sb.Append (TypeManager.CSharpName (pi.ParameterType));
				}
				sb.Append (")\n");
			}
	
			return sb.ToString ();
		}
		
		static public void LocationOfPreviousError (Location loc)
		{
			Console.WriteLine (String.Format ("{0}({1}) (Location of symbol related to previous error)", loc.Name, loc.Row));
		}                

		/// <summary>
		/// In most error cases is very useful to have information about symbol that caused the error.
		/// Call this method before you call Report.Error when it makes sense.
		/// </summary>
		static public void SymbolRelatedToPreviousError (Location loc, string symbol)
		{
			SymbolRelatedToPreviousError (String.Format ("{0}({1})", loc.Name, loc.Row), symbol);
		}

		static public void SymbolRelatedToPreviousError (MemberInfo mi)
		{
			DeclSpace temp_ds = TypeManager.LookupDeclSpace (mi.DeclaringType);
			if (temp_ds == null) {
				SymbolRelatedToPreviousError (mi.DeclaringType.Assembly.Location, TypeManager.GetFullNameSignature (mi));
			} else {
				string name = String.Concat (temp_ds.Name, ".", mi.Name);
				MemberCore mc = temp_ds.GetDefinition (name) as MemberCore;
				SymbolRelatedToPreviousError (mc.Location, mc.GetSignatureForError ());
			}
		}

		static public void SymbolRelatedToPreviousError (MemberCore mc)
		{
			Report.SymbolRelatedToPreviousError (mc.Location, mc.GetSignatureForError ());
		}

		static public void SymbolRelatedToPreviousError (Type type)
		{
			SymbolRelatedToPreviousError (type.Assembly.Location, TypeManager.CSharpName (type));
		}

		static void SymbolRelatedToPreviousError (string loc, string symbol)
		{
			related_symbols.Add (String.Format ("{0}: ('{1}' name of symbol related to previous ", loc, symbol));
		}

		static public void RealError (string msg)
		{
			Errors++;
			Console.WriteLine (msg);

			foreach (string s in related_symbols)
				Console.WriteLine (s);
			related_symbols.Clear ();

			if (Stacktrace)
				Console.WriteLine (FriendlyStackTrace (new StackTrace (true)));
			
			if (Fatal)
				throw new Exception (msg);
		}


		/// <summary>
		/// Method reports warning message. Only one reason why exist Warning and Report methods is beter code readability.
		/// </summary>
		static public void Warning (Message msg, Location loc, params object[] args)
		{
			MessageData md = GetErrorMsg (msg);
			md.Print (msg, String.Format ("{0}({1})", loc.Name, loc.Row), args);
		}

		static public void Warning (Message msg, params object[] args)
		{
			MessageData md = GetErrorMsg (msg);
			md.Print (msg, "", args);
		}

		/// <summary>
		/// Reports error message.
		/// </summary>
		static public void Error_T (Message msg, Location loc, params object[] args)
		{
			Error_T (msg, String.Format ("{0}({1})", loc.Name, loc.Row), args);
		}

		static public void Error_T (Message msg, string location, params object[] args)
		{
			MessageData md = GetErrorMsg (msg);
			md.Print (msg, location, args);
		}

		//[Obsolete ("Use Error_T")]
		static public void Error (int code, Location l, string text)
		{
			if (code < 0)
				code = 8000-code;
			
			string msg = String.Format (
				"{0}({1}) error CS{2:0000}: {3}", l.Name, l.Row, code, text);
//				"{0}({1}) error CS{2}: {3}", l.Name, l.Row, code, text);
			
			RealError (msg);
			Check (code);
		}

//		[Obsolete ("Use Warning with Message parameter")]
		static public void Warning (int code, Location l, string text)
		{
			if (code < 0)
				code = 8000-code;
			
			if (warning_ignore_table != null){
				if (warning_ignore_table.Contains (code)) {
					related_symbols.Clear ();
					return;
				}
			}
			
			if (WarningsAreErrors)
				Error (code, l, text);
			else {
				string row;
				
				if (Location.IsNull (l))
					row = "";
				else
					row = l.Row.ToString ();
				
				Console.WriteLine (String.Format (
					"{0}({1}) warning CS{2:0000}: {3}",
//					"{0}({1}) warning CS{2}: {3}",
					l.Name,  row, code, text));
				Warnings++;

				foreach (string s in related_symbols)
					Console.WriteLine (s);
				related_symbols.Clear ();

				Check (code);

				if (Stacktrace)
					Console.WriteLine (new StackTrace ().ToString ());
			}
		}
		
//		[Obsolete ("Use Warning with Message parameter")]
		static public void Warning (int code, string text)
		{
			Warning (code, Location.Null, text);
		}

		//[Obsolete ("Use Error_T")]
		static public void Error (int code, string text)
		{
			if (code < 0)
				code = 8000-code;
			
			string msg = String.Format ("error CS{0:0000}: {1}", code, text);
//			string msg = String.Format ("error CS{0}: {1}", code, text);
			
			RealError (msg);
			Check (code);
		}

		//[Obsolete ("Use Error_T")]
		static public void Error (int code, Location loc, string format, params object[] args)
		{
			Error (code, loc, String.Format (format, args));
		}

		static public void SetIgnoreWarning (int code)
		{
			if (warning_ignore_table == null)
				warning_ignore_table = new Hashtable ();

			warning_ignore_table [code] = true;
		}
		
		static public int ExpectedError {
			set {
				expected_error = value;
			}
			get {
				return expected_error;
			}
		}

		public static int DebugFlags = 0;

		[Conditional ("MCS_DEBUG")]
		static public void Debug (string message, params object[] args)
		{
			Debug (4, message, args);
		}
			
		[Conditional ("MCS_DEBUG")]
		static public void Debug (int category, string message, params object[] args)
		{
			if ((category & DebugFlags) == 0)
				return;

			StringBuilder sb = new StringBuilder (message);

			if ((args != null) && (args.Length > 0)) {
				sb.Append (": ");

				bool first = true;
				foreach (object arg in args) {
					if (first)
						first = false;
					else
						sb.Append (", ");
					if (arg == null)
						sb.Append ("null");
					else if (arg is ICollection)
						sb.Append (PrintCollection ((ICollection) arg));
					else
						sb.Append (arg);
				}
			}

			Console.WriteLine (sb.ToString ());
		}

		static public string PrintCollection (ICollection collection)
		{
			StringBuilder sb = new StringBuilder ();

			sb.Append (collection.GetType ());
			sb.Append ("(");

			bool first = true;
			foreach (object o in collection) {
				if (first)
					first = false;
				else
					sb.Append (", ");
				sb.Append (o);
			}

			sb.Append (")");
			return sb.ToString ();
		}
	}

	public enum TimerType {
		FindMembers	= 0,
		TcFindMembers	= 1,
		MemberLookup	= 2,
		CachedLookup	= 3,
		CacheInit	= 4,
		MiscTimer	= 5,
		CountTimers	= 6
	}

	public enum CounterType {
		FindMembers	= 0,
		MemberCache	= 1,
		MiscCounter	= 2,
		CountCounters	= 3
	}

	public class Timer
	{
		static DateTime[] timer_start;
		static TimeSpan[] timers;
		static long[] timer_counters;
		static long[] counters;

		static Timer ()
		{
			timer_start = new DateTime [(int) TimerType.CountTimers];
			timers = new TimeSpan [(int) TimerType.CountTimers];
			timer_counters = new long [(int) TimerType.CountTimers];
			counters = new long [(int) CounterType.CountCounters];

			for (int i = 0; i < (int) TimerType.CountTimers; i++) {
				timer_start [i] = DateTime.Now;
				timers [i] = TimeSpan.Zero;
			}
		}

		[Conditional("TIMER")]
		static public void IncrementCounter (CounterType which)
		{
			++counters [(int) which];
		}

		[Conditional("TIMER")]
		static public void StartTimer (TimerType which)
		{
			timer_start [(int) which] = DateTime.Now;
		}

		[Conditional("TIMER")]
		static public void StopTimer (TimerType which)
		{
			timers [(int) which] += DateTime.Now - timer_start [(int) which];
			++timer_counters [(int) which];
		}

		[Conditional("TIMER")]
		static public void ShowTimers ()
		{
			ShowTimer (TimerType.FindMembers, "- FindMembers timer");
			ShowTimer (TimerType.TcFindMembers, "- TypeContainer.FindMembers timer");
			ShowTimer (TimerType.MemberLookup, "- MemberLookup timer");
			ShowTimer (TimerType.CachedLookup, "- CachedLookup timer");
			ShowTimer (TimerType.CacheInit, "- Cache init");
			ShowTimer (TimerType.MiscTimer, "- Misc timer");

			ShowCounter (CounterType.FindMembers, "- Find members");
			ShowCounter (CounterType.MemberCache, "- Member cache");
			ShowCounter (CounterType.MiscCounter, "- Misc counter");
		}

		static public void ShowCounter (CounterType which, string msg)
		{
			Console.WriteLine ("{0} {1}", counters [(int) which], msg);
		}

		static public void ShowTimer (TimerType which, string msg)
		{
			Console.WriteLine (
				"[{0:00}:{1:000}] {2} (used {3} times)",
				(int) timers [(int) which].TotalSeconds,
				timers [(int) which].Milliseconds, msg,
				timer_counters [(int) which]);
		}
	}

	public class InternalErrorException : Exception {
		public InternalErrorException ()
			: base ("Internal error")
		{
		}

		public InternalErrorException (string message)
			: base (message)
		{
		}
	}
}
