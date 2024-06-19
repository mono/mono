using System.Globalization;
using System.Resources;
using System.Threading;

namespace System.Configuration
{

  // Resource class for sys.config if you're building against refsrc
  internal sealed class SR
  {
    internal const string Parameter_Invalid = "Parameter_Invalid";
    internal const string Parameter_NullOrEmpty = "Parameter_NullOrEmpty";
    internal const string Property_NullOrEmpty = "Property_NullOrEmpty";
    internal const string Property_Invalid = "Property_Invalid";
    internal const string Unexpected_Error = "Unexpected_Error";
    internal const string Wrapped_exception_message = "Wrapped_exception_message";
    internal const string Config_error_loading_XML_file = "Config_error_loading_XML_file";
    internal const string Config_exception_creating_section_handler = "Config_exception_creating_section_handler";
    internal const string Config_exception_creating_section = "Config_exception_creating_section";
    internal const string Config_tag_name_invalid = "Config_tag_name_invalid";
    internal const string Argument_AddingDuplicate = "Argument_AddingDuplicate";
    internal const string Config_add_configurationsection_already_added = "Config_add_configurationsection_already_added";
    internal const string Config_add_configurationsection_already_exists = "Config_add_configurationsection_already_exists";
    internal const string Config_add_configurationsection_in_location_config = "Config_add_configurationsection_in_location_config";
    internal const string Config_add_configurationsectiongroup_already_added = "Config_add_configurationsectiongroup_already_added";
    internal const string Config_add_configurationsectiongroup_already_exists = "Config_add_configurationsectiongroup_already_exists";
    internal const string Config_add_configurationsectiongroup_in_location_config = "Config_add_configurationsectiongroup_in_location_config";
    internal const string Config_allow_exedefinition_error_application = "Config_allow_exedefinition_error_application";
    internal const string Config_allow_exedefinition_error_machine = "Config_allow_exedefinition_error_machine";
    internal const string Config_allow_exedefinition_error_roaminguser = "Config_allow_exedefinition_error_roaminguser";
    internal const string Config_appsettings_declaration_invalid = "Config_appsettings_declaration_invalid";
    internal const string Config_base_attribute_locked = "Config_base_attribute_locked";
    internal const string Config_base_collection_item_locked_cannot_clear = "Config_base_collection_item_locked_cannot_clear";
    internal const string Config_base_collection_item_locked = "Config_base_collection_item_locked";
    internal const string Config_base_cannot_add_items_above_inherited_items = "Config_base_cannot_add_items_above_inherited_items";
    internal const string Config_base_cannot_add_items_below_inherited_items = "Config_base_cannot_add_items_below_inherited_items";
    internal const string Config_base_cannot_remove_inherited_items = "Config_base_cannot_remove_inherited_items";
    internal const string Config_base_collection_elements_may_not_be_removed = "Config_base_collection_elements_may_not_be_removed";
    internal const string Config_base_collection_entry_already_exists = "Config_base_collection_entry_already_exists";
    internal const string Config_base_collection_entry_already_removed = "Config_base_collection_entry_already_removed";
    internal const string Config_base_collection_entry_not_found = "Config_base_collection_entry_not_found";
    internal const string Config_base_element_cannot_have_multiple_child_elements = "Config_base_element_cannot_have_multiple_child_elements";
    internal const string Config_base_element_default_collection_cannot_be_locked = "Config_base_element_default_collection_cannot_be_locked";
    internal const string Config_base_element_locked = "Config_base_element_locked";
    internal const string Config_base_expected_enum = "Config_base_expected_enum";
    internal const string Config_base_expected_to_find_element = "Config_base_expected_to_find_element";
    internal const string Config_base_invalid_attribute_to_lock = "Config_base_invalid_attribute_to_lock";
    internal const string Config_base_invalid_attribute_to_lock_by_add = "Config_base_invalid_attribute_to_lock_by_add";
    internal const string Config_base_invalid_element_key = "Config_base_invalid_element_key";
    internal const string Config_base_invalid_element_to_lock = "Config_base_invalid_element_to_lock";
    internal const string Config_base_invalid_element_to_lock_by_add = "Config_base_invalid_element_to_lock_by_add";
    internal const string Config_base_property_is_not_a_configuration_element = "Config_base_property_is_not_a_configuration_element";
    internal const string Config_base_read_only = "Config_base_read_only";
    internal const string Config_base_required_attribute_locked = "Config_base_required_attribute_locked";
    internal const string Config_base_required_attribute_lock_attempt = "Config_base_required_attribute_lock_attempt";
    internal const string Config_base_required_attribute_missing = "Config_base_required_attribute_missing";
    internal const string Config_base_section_cannot_contain_cdata = "Config_base_section_cannot_contain_cdata";
    internal const string Config_base_section_invalid_content = "Config_base_section_invalid_content";
    internal const string Config_base_unrecognized_attribute = "Config_base_unrecognized_attribute";
    internal const string Config_base_unrecognized_element = "Config_base_unrecognized_element";
    internal const string Config_base_unrecognized_element_name = "Config_base_unrecognized_element_name";
    internal const string Config_base_value_cannot_contain = "Config_base_value_cannot_contain";
    internal const string Config_cannot_edit_configurationsection_in_location_config = "Config_cannot_edit_configurationsection_in_location_config";
    internal const string Config_cannot_edit_configurationsection_parentsection = "Config_cannot_edit_configurationsection_parentsection";
    internal const string Config_cannot_edit_configurationsection_when_location_locked = "Config_cannot_edit_configurationsection_when_location_locked";
    internal const string Config_cannot_edit_configurationsection_when_locked = "Config_cannot_edit_configurationsection_when_locked";
    internal const string Config_cannot_edit_configurationsection_when_not_attached = "Config_cannot_edit_configurationsection_when_not_attached";
    internal const string Config_cannot_edit_configurationsection_when_it_is_implicit = "Config_cannot_edit_configurationsection_when_it_is_implicit";
    internal const string Config_cannot_edit_configurationsection_when_it_is_undeclared = "Config_cannot_edit_configurationsection_when_it_is_undeclared";
    internal const string Config_cannot_edit_configurationsectiongroup_in_location_config = "Config_cannot_edit_configurationsectiongroup_in_location_config";
    internal const string Config_cannot_edit_configurationsectiongroup_when_not_attached = "Config_cannot_edit_configurationsectiongroup_when_not_attached";
    internal const string Config_cannot_edit_locationattriubtes = "Config_cannot_edit_locationattriubtes";
    internal const string Config_cannot_open_config_source = "Config_cannot_open_config_source";
    internal const string Config_client_config_init_error = "Config_client_config_init_error";
    internal const string Config_client_config_init_security = "Config_client_config_init_security";
    internal const string Config_client_config_too_many_configsections_elements = "Config_client_config_too_many_configsections_elements";
    internal const string Config_configmanager_open_noexe = "Config_configmanager_open_noexe";
    internal const string Config_configsection_parentnotvalid = "Config_configsection_parentnotvalid";
    internal const string Config_connectionstrings_declaration_invalid = "Config_connectionstrings_declaration_invalid";
    internal const string Config_data_read_count_mismatch = "Config_data_read_count_mismatch";
    internal const string Config_element_no_context = "Config_element_no_context";
    internal const string Config_empty_lock_attributes_except = "Config_empty_lock_attributes_except";
    internal const string Config_empty_lock_attributes_except_effective = "Config_empty_lock_attributes_except_effective";
    internal const string Config_empty_lock_element_except = "Config_empty_lock_element_except";
    internal const string Config_exception_in_config_section_handler = "Config_exception_in_config_section_handler";
    internal const string Config_file_doesnt_have_root_configuration = "Config_file_doesnt_have_root_configuration";
    internal const string Config_file_has_changed = "Config_file_has_changed";
    internal const string Config_getparentconfigurationsection_first_instance = "Config_getparentconfigurationsection_first_instance";
    internal const string Config_inconsistent_location_attributes = "Config_inconsistent_location_attributes";
    internal const string Config_invalid_attributes_for_write = "Config_invalid_attributes_for_write";
    internal const string Config_invalid_boolean_attribute = "Config_invalid_boolean_attribute";
    internal const string Config_invalid_configurationsection_constructor = "Config_invalid_configurationsection_constructor";
    internal const string Config_invalid_node_type = "Config_invalid_node_type";
    internal const string Config_location_location_not_allowed = "Config_location_location_not_allowed";
    internal const string Config_location_path_invalid_character = "Config_location_path_invalid_character";
    internal const string Config_location_path_invalid_first_character = "Config_location_path_invalid_first_character";
    internal const string Config_location_path_invalid_last_character = "Config_location_path_invalid_last_character";
    internal const string Config_missing_required_attribute = "Config_missing_required_attribute";
    internal const string Config_more_data_than_expected = "Config_more_data_than_expected";
    internal const string Config_name_value_file_section_file_invalid_root = "Config_name_value_file_section_file_invalid_root";
    internal const string Config_namespace_invalid = "Config_namespace_invalid";
    internal const string Config_no_stream_to_write = "Config_no_stream_to_write";
    internal const string Config_not_allowed_to_encrypt_this_section = "Config_not_allowed_to_encrypt_this_section";
    internal const string Config_object_is_null = "Config_object_is_null";
    internal const string Config_operation_not_runtime = "Config_operation_not_runtime";
    internal const string Config_properties_may_not_be_derived_from_configuration_section = "Config_properties_may_not_be_derived_from_configuration_section";
    internal const string Config_protection_section_not_found = "Config_protection_section_not_found";
    internal const string Config_provider_must_implement_type = "Config_provider_must_implement_type";
    internal const string Config_root_section_group_cannot_be_edited = "Config_root_section_group_cannot_be_edited";
    internal const string Config_section_allow_definition_attribute_invalid = "Config_section_allow_definition_attribute_invalid";
    internal const string Config_section_allow_exe_definition_attribute_invalid = "Config_section_allow_exe_definition_attribute_invalid";
    internal const string Config_section_cannot_be_used_in_location = "Config_section_cannot_be_used_in_location";
    internal const string Config_section_group_missing_public_constructor = "Config_section_group_missing_public_constructor";
    internal const string Config_section_locked = "Config_section_locked";
    internal const string Config_sections_must_be_unique = "Config_sections_must_be_unique";
    internal const string Config_source_cannot_be_shared = "Config_source_cannot_be_shared";
    internal const string Config_source_parent_conflict = "Config_source_parent_conflict";
    internal const string Config_source_file_format = "Config_source_file_format";
    internal const string Config_source_invalid_format = "Config_source_invalid_format";
    internal const string Config_source_invalid_chars = "Config_source_invalid_chars";
    internal const string Config_source_requires_file = "Config_source_requires_file";
    internal const string Config_source_syntax_error = "Config_source_syntax_error";
    internal const string Config_system_already_set = "Config_system_already_set";
    internal const string Config_tag_name_already_defined = "Config_tag_name_already_defined";
    internal const string Config_tag_name_already_defined_at_this_level = "Config_tag_name_already_defined_at_this_level";
    internal const string Config_tag_name_cannot_be_location = "Config_tag_name_cannot_be_location";
    internal const string Config_tag_name_cannot_begin_with_config = "Config_tag_name_cannot_begin_with_config";
    internal const string Config_type_doesnt_inherit_from_type = "Config_type_doesnt_inherit_from_type";
    internal const string Config_unexpected_element_end = "Config_unexpected_element_end";
    internal const string Config_unexpected_element_name = "Config_unexpected_element_name";
    internal const string Config_unexpected_node_type = "Config_unexpected_node_type";
    internal const string Config_unrecognized_configuration_section = "Config_unrecognized_configuration_section";
    internal const string Config_write_failed = "Config_write_failed";
    internal const string Converter_timespan_not_in_second = "Converter_timespan_not_in_second";
    internal const string Converter_unsupported_value_type = "Converter_unsupported_value_type";
    internal const string Decryption_failed = "Decryption_failed";
    internal const string Default_value_conversion_error_from_string = "Default_value_conversion_error_from_string";
    internal const string Default_value_wrong_type = "Default_value_wrong_type";
    internal const string DPAPI_bad_data = "DPAPI_bad_data";
    internal const string Empty_attribute = "Empty_attribute";
    internal const string EncryptedNode_not_found = "EncryptedNode_not_found";
    internal const string EncryptedNode_is_in_invalid_format = "EncryptedNode_is_in_invalid_format";
    internal const string Encryption_failed = "Encryption_failed";
    internal const string Expect_bool_value_for_DoNotShowUI = "Expect_bool_value_for_DoNotShowUI";
    internal const string Expect_bool_value_for_useMachineProtection = "Expect_bool_value_for_useMachineProtection";
    internal const string IndexOutOfRange = "IndexOutOfRange";
    internal const string Invalid_enum_value = "Invalid_enum_value";
    internal const string Key_container_doesnt_exist_or_access_denied = "Key_container_doesnt_exist_or_access_denied";
    internal const string Must_add_to_config_before_protecting_it = "Must_add_to_config_before_protecting_it";
    internal const string No_converter = "No_converter";
    internal const string No_exception_information_available = "No_exception_information_available";
    internal const string Property_name_reserved = "Property_name_reserved";
    internal const string Item_name_reserved = "Item_name_reserved";
    internal const string Basicmap_item_name_reserved = "Basicmap_item_name_reserved";
    internal const string ProtectedConfigurationProvider_not_found = "ProtectedConfigurationProvider_not_found";
    internal const string Regex_validator_error = "Regex_validator_error";
    internal const string String_null_or_empty = "String_null_or_empty";
    internal const string Subclass_validator_error = "Subclass_validator_error";
    internal const string Top_level_conversion_error_from_string = "Top_level_conversion_error_from_string";
    internal const string Top_level_conversion_error_to_string = "Top_level_conversion_error_to_string";
    internal const string Top_level_validation_error = "Top_level_validation_error";
    internal const string Type_cannot_be_resolved = "Type_cannot_be_resolved";
    internal const string TypeNotPublic = "TypeNotPublic";
    internal const string Unrecognized_initialization_value = "Unrecognized_initialization_value";
    internal const string UseMachineContainer_must_be_bool = "UseMachineContainer_must_be_bool";
    internal const string UseOAEP_must_be_bool = "UseOAEP_must_be_bool";
    internal const string Validation_scalar_range_violation_not_different = "Validation_scalar_range_violation_not_different";
    internal const string Validation_scalar_range_violation_not_equal = "Validation_scalar_range_violation_not_equal";
    internal const string Validation_scalar_range_violation_not_in_range = "Validation_scalar_range_violation_not_in_range";
    internal const string Validation_scalar_range_violation_not_outside_range = "Validation_scalar_range_violation_not_outside_range";
    internal const string Validator_Attribute_param_not_validator = "Validator_Attribute_param_not_validator";
    internal const string Validator_does_not_support_elem_type = "Validator_does_not_support_elem_type";
    internal const string Validator_does_not_support_prop_type = "Validator_does_not_support_prop_type";
    internal const string Validator_element_not_valid = "Validator_element_not_valid";
    internal const string Validator_method_not_found = "Validator_method_not_found";
    internal const string Validator_min_greater_than_max = "Validator_min_greater_than_max";
    internal const string Validator_scalar_resolution_violation = "Validator_scalar_resolution_violation";
    internal const string Validator_string_invalid_chars = "Validator_string_invalid_chars";
    internal const string Validator_string_max_length = "Validator_string_max_length";
    internal const string Validator_string_min_length = "Validator_string_min_length";
    internal const string Validator_value_type_invalid = "Validator_value_type_invalid";
    internal const string Validator_multiple_validator_attributes = "Validator_multiple_validator_attributes";
    internal const string Validator_timespan_value_must_be_positive = "Validator_timespan_value_must_be_positive";
    internal const string WrongType_of_Protected_provider = "WrongType_of_Protected_provider";
    internal const string Type_from_untrusted_assembly = "Type_from_untrusted_assembly";
    internal const string Config_element_locking_not_supported = "Config_element_locking_not_supported";
    internal const string Config_element_null_instance = "Config_element_null_instance";
    internal const string ConfigurationPermissionBadXml = "ConfigurationPermissionBadXml";
    internal const string ConfigurationPermission_Denied = "ConfigurationPermission_Denied";
    internal const string Section_from_untrusted_assembly = "Section_from_untrusted_assembly";
    internal const string Protection_provider_syntax_error = "Protection_provider_syntax_error";
    internal const string Protection_provider_invalid_format = "Protection_provider_invalid_format";
    internal const string Cannot_declare_or_remove_implicit_section = "Cannot_declare_or_remove_implicit_section";
    internal const string Config_reserved_attribute = "Config_reserved_attribute";
    internal const string Filename_in_SaveAs_is_used_already = "Filename_in_SaveAs_is_used_already";
    internal const string Provider_Already_Initialized = "Provider_Already_Initialized";
    internal const string Config_provider_name_null_or_empty = "Config_provider_name_null_or_empty";
    internal const string CollectionReadOnly = "CollectionReadOnly";
    internal const string Config_source_not_under_config_dir = "Config_source_not_under_config_dir";
    internal const string Config_source_invalid = "Config_source_invalid";
    internal const string Location_invalid_inheritInChildApplications_in_machine_or_root_web_config = "Location_invalid_inheritInChildApplications_in_machine_or_root_web_config";
    internal const string Cannot_change_both_AllowOverride_and_OverrideMode = "Cannot_change_both_AllowOverride_and_OverrideMode";
    internal const string Config_section_override_mode_attribute_invalid = "Config_section_override_mode_attribute_invalid";
    internal const string Invalid_override_mode_declaration = "Invalid_override_mode_declaration";
    internal const string Config_cannot_edit_locked_configurationsection_when_mode_is_not_allow = "Config_cannot_edit_locked_configurationsection_when_mode_is_not_allow";
    internal const string Machine_config_file_not_found = "Machine_config_file_not_found";
    internal const string Config_builder_not_found = "Config_builder_not_found";
    internal const string WrongType_of_config_builder = "WrongType_of_config_builder";
    internal const string Config_builder_invalid_format = "Config_builder_invalid_format";
    internal const string ConfigBuilder_processXml_error = "ConfigBuilder_processXml_error";
    private static SR loader;
    private ResourceManager resources;

    internal SR()
    {
      this.resources = new ResourceManager("System.Configuration", this.GetType().Assembly);
    }

    private static SR GetLoader()
    {
      if (SR.loader == null)
      {
        SR sr = new SR();
        Interlocked.CompareExchange<SR>(ref SR.loader, sr, (SR) null);
      }
      return SR.loader;
    }

    private static CultureInfo Culture
    {
      get
      {
        return (CultureInfo) null;
      }
    }

    public static ResourceManager Resources
    {
      get
      {
        return SR.GetLoader().resources;
      }
    }

    public static string GetString(string name, params object[] args)
    {
      SR loader = SR.GetLoader();
      if (loader == null)
        return (string) null;
      string format = loader.resources.GetString(name, SR.Culture);
      if (args == null || args.Length == 0)
        return format;
      for (int index = 0; index < args.Length; ++index)
      {
        string str = args[index] as string;
        if (str != null && str.Length > 1024)
          args[index] = (object) (str.Substring(0, 1021) + "...");
      }
      return string.Format((IFormatProvider) CultureInfo.CurrentCulture, format, args);
    }

    public static string GetString(string name)
    {
      SR loader = SR.GetLoader();
      if (loader == null)
        return (string) null;
      return loader.resources.GetString(name, SR.Culture);
    }

    public static string GetString(string name, out bool usedFallback)
    {
      usedFallback = false;
      return SR.GetString(name);
    }

    public static object GetObject(string name)
    {
      SR loader = SR.GetLoader();
      if (loader == null)
        return (object) null;
      return loader.resources.GetObject(name, SR.Culture);
    }
  }
}