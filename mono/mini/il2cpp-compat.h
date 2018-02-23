#pragma once

#include <mono/metadata/il2cpp-compat-metadata.h>
#if defined(RUNTIME_IL2CPP)
#include "il2cpp-c-types.h"
#include "il2cpp-api.h"
#else
#include <mono/mini/mini.h>
#include <mono/sgen/sgen-conf.h>
#include <mono/metadata/profiler.h>
#endif //RUNTIME_IL2CPP

#ifdef RUNTIME_IL2CPP

#define THREAD_STATIC_FIELD_OFFSET -1

#define VM_DOMAIN_GET_AGENT_INFO(domain) il2cpp_domain_get_agent_info(domain)
#define VM_DOMAIN_SET_AGENT_INFO(domain, value) il2cpp_domain_set_agent_info(domain, value)
#define VM_METHOD_IS_STRING_CTOR(method) il2cpp_method_is_string_ctor(method)
#define VM_INFLATED_METHOD_GET_DECLARING(imethod) il2cpp_method_get_generic_definition(imethod)
#define VM_INFLATED_METHOD_GET_CLASS_INST(imethod) il2cpp_method_get_generic_class_inst(imethod)
#define VM_OBJECT_GET_DOMAIN(object) il2cpp_mono_domain_get()
#define VM_OBJECT_GET_TYPE(object) il2cpp_mono_object_get_type(object)
#define VM_GENERIC_CLASS_GET_CONTAINER_CLASS(gklass) il2cpp_generic_class_get_container_class(gklass)
#define VM_DEFAULTS_OBJECT_CLASS il2cpp_defaults_object_class()
#define VM_DEFAULTS_EXCEPTION_CLASS il2cpp_defaults_exception_class()
#define VM_DEFAULTS_CORLIB_IMAGE il2cpp_defaults_corlib_image()
#define VM_DEFAULTS_VOID_CLASS il2cpp_defaults_void_class()
//Fixme module name as image name seems bad
#define VM_IMAGE_GET_MODULE_NAME(image) il2cpp_image_name(image)
#else
#define VM_DOMAIN_GET_AGENT_INFO(domain) domain_jit_info (domain)->agent_info
#define VM_DOMAIN_SET_AGENT_INFO(domain, value) domain_jit_info (domain)->agent_info = value
#define VM_METHOD_IS_STRING_CTOR(method) method->string_ctor
#define VM_INFLATED_METHOD_GET_DECLARING(imethod) (imethod)->declaring
#define VM_INFLATED_METHOD_GET_CLASS_INST(imethod) (imethod)->context.class_inst
#define VM_OBJECT_GET_DOMAIN(object) ((MonoObject*)object)->vtable->domain
#define VM_OBJECT_GET_TYPE(object) ((MonoReflectionType*)object->vtable->type)->type
#define VM_GENERIC_CLASS_GET_CONTAINER_CLASS(gklass) (gklass)->container_class
#define VM_DEFAULTS_OBJECT_CLASS mono_defaults.object_class
#define VM_DEFAULTS_EXCEPTION_CLASS mono_defaults.exception_class
#define VM_DEFAULTS_CORLIB_IMAGE mono_defaults.corlib
#define VM_DEFAULTS_VOID_CLASS mono_defaults.void_class
#define VM_IMAGE_GET_MODULE_NAME(image) (image)->module_name
#endif

#if defined(RUNTIME_IL2CPP)

#define MonoMethodSignature Il2CppMonoMethodSignature
#define MonoRuntimeExceptionHandlingCallbacks Il2CppMonoRuntimeExceptionHandlingCallbacks
#define debug_options il2cpp_mono_debug_options
#define MonoTypeNameParse Il2CppMonoTypeNameParse
#define MonoDebugOptions Il2CppMonoDebugOptions
#define MonoLMF Il2CppMonoLMF

#define mono_image_get_entry_point il2cpp_mono_image_get_entry_point
#define mono_image_get_filename il2cpp_mono_image_get_filename
#define mono_image_get_guid il2cpp_mono_image_get_guid
#define mono_type_get_class il2cpp_mono_type_get_class
#define mono_type_is_struct il2cpp_mono_type_is_struct
#define mono_type_is_reference il2cpp_mono_type_is_reference
#define mono_metadata_free_mh il2cpp_mono_metadata_free_mh
#define mono_method_signature il2cpp_mono_method_signature
#define mono_method_get_param_names il2cpp_mono_method_get_param_names
#define mono_type_generic_inst_is_valuetype il2cpp_mono_type_generic_inst_is_valuetype
#define mono_method_get_header_checked il2cpp_mono_method_get_header_checked
#define mono_class_init il2cpp_mono_class_init
#define mono_class_vtable il2cpp_mono_class_vtable
#define mono_class_get_field_from_name il2cpp_mono_class_get_field_from_name
#define mono_array_element_size il2cpp_mono_array_element_size
#define mono_class_instance_size il2cpp_mono_class_instance_size
#define mono_class_value_size il2cpp_mono_class_value_size
#define mono_class_is_assignable_from il2cpp_mono_class_is_assignable_from
#define mono_class_from_mono_type il2cpp_mono_class_from_mono_type
#define mono_class_get_flags il2cpp_class_get_flags
#define mono_class_num_fields il2cpp_mono_class_num_fields
#define mono_class_num_methods il2cpp_mono_class_num_methods
#define mono_class_num_properties il2cpp_mono_class_num_properties
#define mono_class_get_fields il2cpp_mono_class_get_fields
#define mono_class_get_methods il2cpp_mono_class_get_methods
#define mono_class_get_properties il2cpp_mono_class_get_properties
#define mono_class_get_nested_types il2cpp_class_get_nested_types_accepts_generic
#define mono_field_get_name il2cpp_mono_field_get_name
#define mono_class_get_method_from_name il2cpp_class_get_method_from_name
#define mono_string_chars il2cpp_mono_string_chars
#define mono_class_is_abstract il2cpp_class_is_abstract
#define mono_string_length il2cpp_mono_string_length
#define mono_array_addr_with_size il2cpp_mono_array_addr_with_size
#define mono_array_length il2cpp_mono_array_length
#define mono_string_new il2cpp_mono_string_new
#define mono_string_new_checked il2cpp_mono_string_new_checked
#define mono_string_to_utf8_checked il2cpp_mono_string_to_utf8_checked
#define mono_object_hash il2cpp_mono_object_hash
#define mono_object_unbox il2cpp_mono_object_unbox
#define mono_object_get_virtual_method il2cpp_object_get_virtual_method
#define mono_field_set_value il2cpp_mono_field_set_value
#define mono_field_static_set_value il2cpp_mono_field_static_set_value
#define mono_gchandle_new_weakref il2cpp_mono_gchandle_new_weakref
#define mono_gchandle_get_target il2cpp_mono_gchandle_get_target
#define mono_gchandle_free il2cpp_mono_gchandle_free
#define mono_gc_wbarrier_generic_store il2cpp_mono_gc_wbarrier_generic_store
#define mono_reflection_parse_type_checked il2cpp_mono_reflection_parse_type_checked
#define mono_reflection_free_type_info il2cpp_mono_reflection_free_type_info
#define mono_get_root_domain il2cpp_mono_get_root_domain
#define mono_runtime_quit il2cpp_mono_runtime_quit
#define mono_runtime_is_shutting_down il2cpp_mono_runtime_is_shutting_down
#define mono_domain_get il2cpp_mono_domain_get
#define mono_domain_set il2cpp_mono_domain_set
#define mono_domain_foreach il2cpp_mono_domain_foreach
#define mono_jit_info_table_find il2cpp_mono_jit_info_table_find
#define mono_jit_info_get_method il2cpp_mono_jit_info_get_method
#define mono_debug_find_method il2cpp_mono_debug_find_method
#define mono_debug_free_method_jit_info il2cpp_mono_debug_free_method_jit_info
#define mono_debug_il_offset_from_address il2cpp_mono_debug_il_offset_from_address
#define mono_set_is_debugger_attached il2cpp_mono_set_is_debugger_attached
#define mono_type_full_name il2cpp_mono_type_full_name
#define mono_method_full_name il2cpp_mono_method_full_name
#define mono_thread_current il2cpp_mono_thread_current
#define mono_thread_get_main il2cpp_mono_thread_get_main
#define mono_thread_attach il2cpp_mono_thread_attach
#define mono_thread_detach il2cpp_mono_thread_detach
#define mono_domain_lock il2cpp_mono_domain_lock
#define mono_domain_unlock il2cpp_mono_domain_unlock
#define mono_jit_info_table_find_internal il2cpp_mono_jit_info_table_find_internal
#define mono_aligned_addr_hash il2cpp_mono_aligned_addr_hash
#define mono_metadata_get_generic_inst il2cpp_mono_metadata_get_generic_inst
#define mono_get_method_checked il2cpp_mono_get_method_checked
#define mono_class_interface_offset_with_variance il2cpp_mono_class_interface_offset_with_variance
#define mono_class_setup_supertypes il2cpp_mono_class_setup_supertypes
#define mono_class_setup_vtable il2cpp_mono_class_setup_vtable
#define mono_class_setup_methods il2cpp_mono_class_setup_methods
#define mono_class_field_is_special_static il2cpp_mono_class_field_is_special_static
#define mono_class_field_get_special_static_type il2cpp_mono_class_field_get_special_static_type
#define mono_class_get_context il2cpp_mono_class_get_context
#define mono_method_get_context il2cpp_mono_method_get_context
#define mono_method_get_generic_container il2cpp_mono_method_get_generic_container
#define mono_class_inflate_generic_method_full_checked il2cpp_mono_class_inflate_generic_method_full_checked
#define mono_class_inflate_generic_method_checked il2cpp_mono_class_inflate_generic_method_checked
#define mono_loader_lock il2cpp_mono_loader_lock
#define mono_loader_unlock il2cpp_mono_loader_unlock
#define mono_loader_lock_track_ownership il2cpp_mono_loader_lock_track_ownership
#define mono_loader_lock_is_owned_by_self il2cpp_mono_loader_lock_is_owned_by_self
#define mono_method_get_wrapper_data il2cpp_mono_method_get_wrapper_data
#define mono_type_get_name_full il2cpp_mono_type_get_name_full
#define mono_class_is_nullable il2cpp_mono_class_is_nullable
#define mono_class_get_generic_container il2cpp_mono_class_get_generic_container
#define mono_class_setup_interfaces il2cpp_mono_class_setup_interfaces
#define mono_class_get_methods_by_name il2cpp_mono_class_get_methods_by_name
#define mono_ldtoken_checked il2cpp_mono_ldtoken_checked
#define mono_class_from_generic_parameter_internal il2cpp_mono_class_from_generic_parameter_internal
#define mono_class_load_from_name il2cpp_mono_class_load_from_name
#define mono_class_try_load_from_name il2cpp_class_from_name
#define mono_class_get_generic_class il2cpp_mono_class_get_generic_class
#define mono_thread_internal_current il2cpp_mono_thread_internal_current
#define mono_thread_internal_is_current il2cpp_mono_thread_internal_is_current
#define mono_thread_internal_abort il2cpp_mono_thread_internal_abort
#define mono_thread_internal_reset_abort il2cpp_mono_thread_internal_reset_abort
#define mono_thread_get_name il2cpp_mono_thread_get_name
#define mono_thread_set_name_internal il2cpp_mono_thread_set_name_internal
#define mono_thread_suspend_all_other_threads il2cpp_mono_thread_suspend_all_other_threads
#define mono_stack_mark_record_size il2cpp_mono_stack_mark_record_size
#define mono_get_eh_callbacks il2cpp_mono_get_eh_callbacks
#define mono_nullable_init il2cpp_mono_nullable_init
#define mono_value_box_checked il2cpp_mono_value_box_checked
#define mono_field_static_get_value_checked il2cpp_mono_field_static_get_value_checked
#define mono_field_static_get_value_for_thread il2cpp_mono_field_static_get_value_for_thread
#define mono_field_get_value_object_checked il2cpp_mono_field_get_value_object_checked
#define mono_object_new_checked il2cpp_mono_object_new_checked
#define mono_ldstr_checked il2cpp_mono_ldstr_checked
#define mono_runtime_try_invoke il2cpp_mono_runtime_try_invoke
#define mono_runtime_invoke_checked il2cpp_mono_runtime_invoke_checked
#define mono_gc_base_init il2cpp_mono_gc_base_init
#define mono_gc_register_root il2cpp_mono_gc_register_root
#define mono_gc_deregister_root il2cpp_mono_gc_deregister_root
#define mono_environment_exitcode_get il2cpp_mono_environment_exitcode_get
#define mono_environment_exitcode_set il2cpp_mono_environment_exitcode_set
#define mono_threadpool_suspend il2cpp_mono_threadpool_suspend
#define mono_threadpool_resume il2cpp_mono_threadpool_resume
#define mono_assembly_get_image il2cpp_mono_assembly_get_image
#define mono_runtime_try_shutdown il2cpp_mono_runtime_try_shutdown
#define mono_verifier_is_method_valid_generic_instantiation il2cpp_mono_verifier_is_method_valid_generic_instantiation
#define mono_reflection_get_type_checked il2cpp_mono_reflection_get_type_checked
#define mono_assembly_get_object_handle il2cpp_mono_assembly_get_object_handle
#define mono_type_get_object_checked il2cpp_mono_type_get_object_checked
#define mono_network_init il2cpp_mono_network_init
#define mono_w32socket_set_blocking il2cpp_mono_w32socket_set_blocking

#define mono_get_runtime_build_info il2cpp_mono_get_runtime_build_info
#define mono_marshal_method_from_wrapper il2cpp_mono_marshal_method_from_wrapper
#define mini_get_debug_options il2cpp_mini_get_debug_options
#define mono_jit_find_compiled_method_with_jit_info il2cpp_mono_jit_find_compiled_method_with_jit_info
#define mono_get_lmf_addr il2cpp_mono_get_lmf_addr
#define mono_set_lmf il2cpp_mono_set_lmf
#define mono_aot_get_method_checked il2cpp_mono_aot_get_method_checked
#define mono_arch_setup_resume_sighandler_ctx il2cpp_mono_arch_setup_resume_sighandler_ctx
#define mono_arch_set_breakpoint il2cpp_mono_arch_set_breakpoint
#define mono_arch_clear_breakpoint il2cpp_mono_arch_clear_breakpoint
#define mono_arch_start_single_stepping il2cpp_mono_arch_start_single_stepping
#define mono_arch_stop_single_stepping il2cpp_mono_arch_stop_single_stepping
#define mono_arch_skip_breakpoint il2cpp_mono_arch_skip_breakpoint
#define mono_arch_skip_single_step il2cpp_mono_arch_skip_single_step
#define mono_arch_context_get_int_reg il2cpp_mono_arch_context_get_int_reg
#define mono_arch_context_set_int_reg il2cpp_mono_arch_context_set_int_reg
#define mono_walk_stack_with_ctx il2cpp_mono_walk_stack_with_ctx
#define mono_walk_stack_with_state il2cpp_mono_walk_stack_with_state
#define mono_thread_state_init_from_current il2cpp_mono_thread_state_init_from_current
#define mono_thread_state_init_from_monoctx il2cpp_mono_thread_state_init_from_monoctx
#define mini_jit_info_table_find il2cpp_mini_jit_info_table_find
#define mono_restore_context il2cpp_mono_restore_context
#define mono_method_get_declaring_generic_method il2cpp_mono_method_get_declaring_generic_method
#define jinfo_get_method il2cpp_jinfo_get_method
#define mono_defaults il2cpp_mono_defaults
#define mono_find_prev_seq_point_for_native_offset il2cpp_mono_find_prev_seq_point_for_native_offset
#define mono_w32socket_accept_internal il2cpp_mono_w32socket_accept_internal
#define mono_find_next_seq_point_for_native_offset il2cpp_mono_find_next_seq_point_for_native_offset
#define mono_class_has_parent il2cpp_mono_class_has_parent
#define mono_class_is_gtd il2cpp_class_is_generic
#define mono_class_is_ginst il2cpp_class_is_inflated
#define mono_generic_container_get_param il2cpp_mono_generic_container_get_param
#define mono_find_seq_point il2cpp_mono_find_seq_point
#define mono_seq_point_iterator_init il2cpp_mono_seq_point_iterator_init
#define mono_seq_point_iterator_next il2cpp_mono_seq_point_iterator_next
#define mono_seq_point_init_next il2cpp_mono_seq_point_init_next
#define mono_get_seq_points il2cpp_mono_get_seq_points
#define G_BREAKPOINT IL2CPP_G_BREAKPOINT
#define mono_thread_info_safe_suspend_and_run il2cpp_mono_thread_info_safe_suspend_and_run
#define mono_error_cleanup il2cpp_mono_error_cleanup
#define mono_error_convert_to_exception il2cpp_mono_error_convert_to_exception
#define mono_error_get_message il2cpp_mono_error_get_message
#define mono_error_assert_ok_pos il2cpp_mono_error_assert_ok_pos
#define mono_class_get_namespace il2cpp_class_get_namespace
#define mono_class_get_name il2cpp_class_get_name
#define mono_object_get_class il2cpp_object_get_class
#define mono_field_get_parent il2cpp_field_get_parent
#define mono_class_get_parent il2cpp_class_get_parent
#define mono_field_get_type il2cpp_field_get_type
#define mono_method_get_name il2cpp_method_get_name
#define mono_class_get_type il2cpp_class_get_type
#define mono_method_get_class il2cpp_method_get_class
#define mono_class_get_image il2cpp_class_get_image
#define mono_class_get_interfaces il2cpp_class_get_interfaces
#undef MONO_CLASS_IS_INTERFACE
#define MONO_CLASS_IS_INTERFACE il2cpp_class_is_interface
#define mono_image_get_assembly il2cpp_image_get_assembly
#define mono_image_get_name il2cpp_image_get_name
#define mono_type_get_type il2cpp_type_get_type
#define mono_class_get_rank il2cpp_class_get_rank
#define mono_class_get_element_class il2cpp_class_get_element_class
#define mono_class_get_type_token il2cpp_class_get_type_token
#define mono_type_is_byref il2cpp_type_is_byref
#define mono_class_is_enum il2cpp_class_is_enum
#define mono_method_get_flags il2cpp_method_get_flags
#define mono_method_get_token il2cpp_method_get_token
#define mono_method_is_generic il2cpp_method_is_generic
#define mono_method_is_inflated il2cpp_method_is_inflated
#undef mono_field_is_deleted
#define mono_field_is_deleted il2cpp_field_is_deleted
#define mono_domain_get_assemblies_iter il2cpp_domain_get_assemblies_iter

#undef mono_domain_assemblies_lock
#define mono_domain_assemblies_lock
#undef mono_domain_assemblies_unlock
#define mono_domain_assemblies_unlock

#define mono_get_string_class il2cpp_mono_get_string_class

#define MONO_MAX_IREGS 1
#define NOT_IMPLEMENTED do { g_assert_not_reached (); } while (0)

MonoMethod* il2cpp_mono_image_get_entry_point (MonoImage *image);
const char* il2cpp_mono_image_get_filename (MonoImage *image);
const char*  il2cpp_mono_image_get_guid (MonoImage *image);
MonoClass* il2cpp_mono_type_get_class (MonoType *type);
mono_bool il2cpp_mono_type_is_struct (MonoType *type);
mono_bool il2cpp_mono_type_is_reference (MonoType *type);
void il2cpp_mono_metadata_free_mh (MonoMethodHeader *mh);
Il2CppMonoMethodSignature* il2cpp_mono_method_signature (MonoMethod *m);
void il2cpp_mono_method_get_param_names (MonoMethod *method, const char **names);
mono_bool il2cpp_mono_type_generic_inst_is_valuetype (MonoType *type);
MonoMethodHeader* il2cpp_mono_method_get_header_checked (MonoMethod *method, MonoError *error);
gboolean il2cpp_mono_class_init (MonoClass *klass);
MonoVTable* il2cpp_mono_class_vtable (MonoDomain *domain, MonoClass *klass);
MonoClassField* il2cpp_mono_class_get_field_from_name (MonoClass *klass, const char *name);
int32_t il2cpp_mono_array_element_size (MonoClass *ac);
int32_t il2cpp_mono_class_instance_size (MonoClass *klass);
int32_t il2cpp_mono_class_value_size (MonoClass *klass, uint32_t *align);
gboolean il2cpp_mono_class_is_assignable_from (MonoClass *klass, MonoClass *oklass);
MonoClass* il2cpp_mono_class_from_mono_type (MonoType *type);
int il2cpp_mono_class_num_fields (MonoClass *klass);
int il2cpp_mono_class_num_methods (MonoClass *klass);
int il2cpp_mono_class_num_properties (MonoClass *klass);
MonoClassField* il2cpp_mono_class_get_fields (MonoClass* klass, gpointer *iter);
MonoMethod* il2cpp_mono_class_get_methods (MonoClass* klass, gpointer *iter);
MonoProperty* il2cpp_mono_class_get_properties (MonoClass* klass, gpointer *iter);
const char* il2cpp_mono_field_get_name (MonoClassField *field);
mono_unichar2* il2cpp_mono_string_chars (MonoString *s);
int il2cpp_mono_string_length (MonoString *s);
char* il2cpp_mono_array_addr_with_size (MonoArray *array, int size, uintptr_t idx);
uintptr_t il2cpp_mono_array_length (MonoArray *array);
MonoString* il2cpp_mono_string_new (MonoDomain *domain, const char *text);
MonoString* il2cpp_mono_string_new (MonoDomain *domain, const char *text);
MonoString* il2cpp_mono_string_new_checked (MonoDomain *domain, const char *text, MonoError *merror);
char* il2cpp_mono_string_to_utf8_checked (MonoString *string_obj, MonoError *error);
int il2cpp_mono_object_hash (MonoObject* obj);
void* il2cpp_mono_object_unbox (MonoObject *obj);
void il2cpp_mono_field_set_value (MonoObject *obj, MonoClassField *field, void *value);
void il2cpp_mono_field_static_set_value (MonoVTable *vt, MonoClassField *field, void *value);
uint32_t il2cpp_mono_gchandle_new_weakref (MonoObject *obj, mono_bool track_resurrection);
MonoObject*  il2cpp_mono_gchandle_get_target (uint32_t gchandle);
void il2cpp_mono_gchandle_free (uint32_t gchandle);
void il2cpp_mono_gc_wbarrier_generic_store (void* ptr, MonoObject* value);
int il2cpp_mono_reflection_parse_type_checked (char *name, Il2CppMonoTypeNameParse *info, MonoError *error);
void il2cpp_mono_reflection_free_type_info (Il2CppMonoTypeNameParse *info);
MonoDomain* il2cpp_mono_get_root_domain (void);
void il2cpp_mono_runtime_quit (void);
gboolean il2cpp_mono_runtime_is_shutting_down (void);
MonoDomain* il2cpp_mono_domain_get (void);
gboolean il2cpp_mono_domain_set (MonoDomain *domain, gboolean force);
void il2cpp_mono_domain_foreach(MonoDomainFunc func, gpointer user_data);
MonoJitInfo* il2cpp_mono_jit_info_table_find(MonoDomain* domain, char* addr);
MonoMethod* il2cpp_mono_jit_info_get_method(MonoJitInfo* ji);
gint32 il2cpp_mono_debug_il_offset_from_address(MonoMethod* method, MonoDomain* domain, guint32 native_offset);
void il2cpp_mono_set_is_debugger_attached(gboolean attached);
char* il2cpp_mono_type_full_name(MonoType* type);
char* il2cpp_mono_method_full_name(MonoMethod* method, gboolean signature);
MonoThread* il2cpp_mono_thread_current();
MonoThread* il2cpp_mono_thread_get_main();
MonoThread* il2cpp_mono_thread_attach(MonoDomain* domain);
void il2cpp_mono_domain_lock(MonoDomain* domain);
void il2cpp_mono_domain_unlock(MonoDomain* domain);
MonoJitInfo* il2cpp_mono_jit_info_table_find_internal(MonoDomain* domain, char* addr, gboolean try_aot, gboolean allow_trampolines);
guint il2cpp_mono_aligned_addr_hash(gconstpointer ptr);
MonoGenericInst* il2cpp_mono_metadata_get_generic_inst(int type_argc, MonoType** type_argv);
MonoMethod* il2cpp_mono_get_method_checked(MonoImage* image, guint32 token, MonoClass* klass, MonoGenericContext* context, MonoError* error);
int il2cpp_mono_class_interface_offset_with_variance(MonoClass* klass, MonoClass* itf, gboolean* non_exact_match);
void il2cpp_mono_class_setup_supertypes(MonoClass* klass);
void il2cpp_mono_class_setup_vtable(MonoClass* klass);
void il2cpp_mono_class_setup_methods(MonoClass* klass);
gboolean il2cpp_mono_class_field_is_special_static(MonoClassField* field);
guint32 il2cpp_mono_class_field_get_special_static_type(MonoClassField* field);
MonoGenericContext* il2cpp_mono_class_get_context(MonoClass* klass);
MonoGenericContext* il2cpp_mono_method_get_context(MonoMethod* method);
MonoGenericContainer* il2cpp_mono_method_get_generic_container(MonoMethod* method);
MonoMethod* il2cpp_mono_class_inflate_generic_method_full_checked(MonoMethod* method, MonoClass* klass_hint, MonoGenericContext* context, MonoError* error);
MonoMethod* il2cpp_mono_class_inflate_generic_method_checked(MonoMethod* method, MonoGenericContext* context, MonoError* error);
void il2cpp_mono_loader_lock();
void il2cpp_mono_loader_unlock();
void il2cpp_mono_loader_lock_track_ownership(gboolean track);
gboolean il2cpp_mono_loader_lock_is_owned_by_self();
gpointer il2cpp_mono_method_get_wrapper_data(MonoMethod* method, guint32 id);
char* il2cpp_mono_type_get_name_full(MonoType* type, MonoTypeNameFormat format);
gboolean il2cpp_mono_class_is_nullable(MonoClass* klass);
MonoGenericContainer* il2cpp_mono_class_get_generic_container(MonoClass* klass);
void il2cpp_mono_class_setup_interfaces(MonoClass* klass, MonoError* error);
GPtrArray* il2cpp_mono_class_get_methods_by_name(MonoClass* klass, const char* name, guint32 bflags, gboolean ignore_case, gboolean allow_ctors, MonoError* error);
gpointer il2cpp_mono_ldtoken_checked(MonoImage* image, guint32 token, MonoClass** handle_class, MonoGenericContext* context, MonoError* error);
MonoClass* il2cpp_mono_class_from_generic_parameter_internal(MonoGenericParam* param);
MonoClass* il2cpp_mono_class_load_from_name(MonoImage* image, const char* name_space, const char* name);
MonoGenericClass* il2cpp_mono_class_get_generic_class(MonoClass* klass);
MonoInternalThread* il2cpp_mono_thread_internal_current();
gboolean il2cpp_mono_thread_internal_is_current(MonoInternalThread* thread);
void il2cpp_mono_thread_internal_abort(MonoInternalThread* thread, gboolean appdomain_unload);
void il2cpp_mono_thread_internal_reset_abort(MonoInternalThread* thread);
gunichar2* il2cpp_mono_thread_get_name(MonoInternalThread* this_obj, guint32* name_len);
void il2cpp_mono_thread_set_name_internal(MonoInternalThread* this_obj, MonoString* name, gboolean permanent, gboolean reset, MonoError* error);
void il2cpp_mono_thread_suspend_all_other_threads();
void il2cpp_mono_stack_mark_record_size(MonoThreadInfo* info, HandleStackMark* stackmark, const char* func_name);
Il2CppMonoRuntimeExceptionHandlingCallbacks* il2cpp_mono_get_eh_callbacks();
void il2cpp_mono_nullable_init(guint8* buf, MonoObject* value, MonoClass* klass);
MonoObject* il2cpp_mono_value_box_checked(MonoDomain* domain, MonoClass* klass, gpointer value, MonoError* error);
void il2cpp_mono_field_static_get_value_checked(MonoVTable* vt, MonoClassField* field, void* value, MonoError* error);
void il2cpp_mono_field_static_get_value_for_thread(MonoInternalThread* thread, MonoVTable* vt, MonoClassField* field, void* value, MonoError* error);
MonoObject* il2cpp_mono_field_get_value_object_checked(MonoDomain* domain, MonoClassField* field, MonoObject* obj, MonoError* error);
MonoObject* il2cpp_mono_object_new_checked(MonoDomain* domain, MonoClass* klass, MonoError* error);
MonoString* il2cpp_mono_ldstr_checked(MonoDomain* domain, MonoImage* image, guint32 idx, MonoError* error);
MonoObject* il2cpp_mono_runtime_try_invoke(MonoMethod* method, void* obj, void** params, MonoObject** exc, MonoError* error);
MonoObject* il2cpp_mono_runtime_invoke_checked(MonoMethod* method, void* obj, void** params, MonoError* error);
void il2cpp_mono_gc_base_init();
int il2cpp_mono_gc_register_root(char* start, size_t size, MonoGCDescriptor descr, MonoGCRootSource source, void *key, const char* msg);
void il2cpp_mono_gc_deregister_root(char* addr);
gint32 il2cpp_mono_environment_exitcode_get();
void il2cpp_mono_environment_exitcode_set(gint32 value);
void il2cpp_mono_threadpool_suspend();
void il2cpp_mono_threadpool_resume();
gboolean il2cpp_mono_runtime_try_shutdown();
gboolean il2cpp_mono_verifier_is_method_valid_generic_instantiation(MonoMethod* method);
MonoType* il2cpp_mono_reflection_get_type_checked(MonoImage* rootimage, MonoImage* image, Il2CppMonoTypeNameParse* info, gboolean ignorecase, gboolean* type_resolve, MonoError* error);
MonoReflectionAssemblyHandle il2cpp_mono_assembly_get_object_handle(MonoDomain* domain, MonoAssembly* assembly, MonoError* error);
MonoReflectionType* il2cpp_mono_type_get_object_checked(MonoDomain* domain, MonoType* type, MonoError* error);
void il2cpp_mono_network_init();
gint il2cpp_mono_w32socket_set_blocking(SOCKET sock, gboolean blocking);

char* il2cpp_mono_get_runtime_build_info();
MonoMethod* il2cpp_mono_marshal_method_from_wrapper(MonoMethod* wrapper);
MonoDebugOptions* il2cpp_mini_get_debug_options();
gpointer il2cpp_mono_jit_find_compiled_method_with_jit_info(MonoDomain* domain, MonoMethod* method, MonoJitInfo** ji);
MonoLMF** il2cpp_mono_get_lmf_addr();
void il2cpp_mono_set_lmf(MonoLMF* lmf);
gpointer il2cpp_mono_aot_get_method_checked(MonoDomain* domain, MonoMethod* method, MonoError* error);
void il2cpp_mono_arch_setup_resume_sighandler_ctx(MonoContext* ctx, gpointer func);
void il2cpp_mono_arch_set_breakpoint(MonoJitInfo* ji, guint8* ip);
void il2cpp_mono_arch_clear_breakpoint(MonoJitInfo* ji, guint8* ip);
void il2cpp_mono_arch_start_single_stepping();
void il2cpp_mono_arch_stop_single_stepping();
void il2cpp_mono_arch_skip_breakpoint(MonoContext* ctx, MonoJitInfo* ji);
void il2cpp_mono_arch_skip_single_step(MonoContext* ctx);
mgreg_t il2cpp_mono_arch_context_get_int_reg(MonoContext* ctx, int reg);
void il2cpp_mono_arch_context_set_int_reg(MonoContext* ctx, int reg, mgreg_t val);
void il2cpp_mono_walk_stack_with_ctx(Il2CppMonoJitStackWalk func, MonoContext* start_ctx, MonoUnwindOptions unwind_options, void* user_data);
void il2cpp_mono_walk_stack_with_state(Il2CppMonoJitStackWalk func, MonoThreadUnwindState* state, MonoUnwindOptions unwind_options, void* user_data);
gboolean il2cpp_mono_thread_state_init_from_current(MonoThreadUnwindState* ctx);
gboolean il2cpp_mono_thread_state_init_from_monoctx(MonoThreadUnwindState* ctx, MonoContext* mctx);
MonoJitInfo* il2cpp_mini_jit_info_table_find(MonoDomain* domain, char* addr, MonoDomain** out_domain);
void il2cpp_mono_restore_context(MonoContext* ctx);
MonoMethod* il2cpp_mono_method_get_declaring_generic_method(MonoMethod* method);
MonoMethod* il2cpp_jinfo_get_method (MonoJitInfo *ji);
gboolean il2cpp_mono_find_prev_seq_point_for_native_offset (MonoDomain *domain, MonoMethod *method, gint32 native_offset, MonoSeqPointInfo **info, SeqPoint* seq_point);
SOCKET il2cpp_mono_w32socket_accept_internal (SOCKET s, struct sockaddr *addr, socklen_t *addrlen, gboolean blocking);
gboolean il2cpp_mono_find_next_seq_point_for_native_offset (MonoDomain *domain, MonoMethod *method, gint32 native_offset, MonoSeqPointInfo **info, SeqPoint* seq_point);
gboolean il2cpp_mono_class_has_parent (MonoClass *klass, MonoClass *parent);
MonoGenericParam* il2cpp_mono_generic_container_get_param (MonoGenericContainer *gc, int i);
gboolean il2cpp_mono_find_seq_point (MonoDomain *domain, MonoMethod *method, gint32 il_offset, MonoSeqPointInfo **info, SeqPoint *seq_point);
void il2cpp_mono_seq_point_iterator_init (SeqPointIterator* it, MonoSeqPointInfo* info);
gboolean il2cpp_mono_seq_point_iterator_next (SeqPointIterator* it);
void il2cpp_mono_seq_point_init_next (MonoSeqPointInfo* info, SeqPoint sp, SeqPoint* next);
MonoSeqPointInfo* il2cpp_mono_get_seq_points (MonoDomain *domain, MonoMethod *method);
void IL2CPP_G_BREAKPOINT();
void il2cpp_mono_thread_info_safe_suspend_and_run (MonoNativeThreadId id, gboolean interrupt_kernel, MonoSuspendThreadCallback callback, gpointer user_data);
void il2cpp_mono_error_cleanup (MonoError *oerror);
MonoException* il2cpp_mono_error_convert_to_exception (MonoError *target_error);
const char* il2cpp_mono_error_get_message (MonoError *oerror);
void il2cpp_mono_error_assert_ok_pos (MonoError *error, const char* filename, int lineno);
Il2CppSequencePoint* il2cpp_get_sequence_points(void* *iter);
Il2CppSequencePoint* il2cpp_get_method_sequence_points(MonoMethod* method, void* *iter);
MonoClass* il2cpp_class_get_nested_types_accepts_generic(MonoClass *monoClass, void* *iter);
MonoClass* il2cpp_defaults_object_class();
guint8 il2cpp_array_rank(MonoArray *monoArr);
const char* il2cpp_image_name(MonoImage *monoImage);
guint8* il2cpp_field_get_address(MonoObject *obj, MonoClassField *monoField);
MonoType* il2cpp_mono_object_get_type(MonoObject* object);
MonoClass* il2cpp_defaults_exception_class();
MonoImage* il2cpp_defaults_corlib_image();
bool il2cpp_method_is_string_ctor (const MonoMethod * method);
MonoClass* il2cpp_defaults_void_class();
void il2cpp_set_var(guint8* newValue, void *value, MonoType *localVariableTypeMono);
MonoMethod* il2cpp_get_interface_method(MonoClass* klass, MonoClass* itf, int slot);
gboolean il2cpp_field_is_deleted(MonoClassField *field);
MonoClass* il2cpp_iterate_loaded_classes(void* *iter);
MonoAssembly* il2cpp_domain_get_assemblies_iter(MonoAppDomain *domain, void* *iter);
const char** il2cpp_get_source_files_for_type(MonoClass *klass, int *count);
MonoMethod* il2cpp_method_get_generic_definition(MonoMethodInflated *imethod);
MonoGenericInst* il2cpp_method_get_generic_class_inst(MonoMethodInflated *imethod);
MonoClass* il2cpp_generic_class_get_container_class(MonoGenericClass *gclass);
void il2cpp_mono_thread_detach(MonoThread* thread);
MonoClass* il2cpp_mono_get_string_class (void);
Il2CppSequencePoint* il2cpp_get_sequence_point(int id);
char* il2cpp_assembly_get_full_name(MonoAssembly *assembly);
const MonoMethod* il2cpp_get_seq_point_method(Il2CppSequencePoint *seqPoint);
const MonoClass* il2cpp_get_class_from_index(int index);
const MonoType* il2cpp_get_type_from_index(int index);

#endif // RUNTIME_IL2CPP
