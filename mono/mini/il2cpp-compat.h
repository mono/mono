#pragma once

#include <mono/metadata/il2cpp-compat-metadata.h>
#if defined(RUNTIME_IL2CPP)
#include "il2cpp-c-types.h"
#include "il2cpp-api.h"
#endif // RUNTIME_IL2CPP
#include <mono/mini/mini.h>
#include <mono/sgen/sgen-conf.h>
#include <mono/metadata/profiler.h>

#ifdef IL2CPP_MONO_DEBUGGER

#define THREAD_STATIC_FIELD_OFFSET -1

#define VM_THREAD_GET_INTERNAL(thread) il2cpp_mono_thread_get_internal(thread)
#define VM_INTERNAL_THREAD_SET_STATE_BACKGROUND(internal_thread) il2cpp_internal_thread_set_state_background(internal_thread)
#define VM_INTERNAL_THREAD_SET_FLAG_DONT_MANAGE(thread)
#define VM_INTERNAL_THREAD_GET_ID(internal_thread) il2cpp_internal_thread_get_thread_id(internal_thread)
#define VM_INTERNAL_THREAD_GET_STATE(internal_thread) il2cpp_internal_thread_get_state(internal_thread)
#define VM_INTERNAL_THREAD_GET_THREADPOOL_THREAD(internal_thread) il2cpp_internal_thread_get_threadpool_thread(internal_thread)
#define VM_DOMAIN_GET_AGENT_INFO(domain) il2cpp_domain_get_agent_info(domain)
#define VM_DOMAIN_SET_AGENT_INFO(domain, value) il2cpp_domain_set_agent_info(domain, value)
#define VM_DOMAIN_GET_NAME(domain) il2cpp_domain_get_name(domain)
#define VM_DOMAIN_GET_CORLIB(domain) il2cpp_image_get_assembly(il2cpp_get_corlib())
#define VM_DOMAIN_GET_ASSEMBLIES(domain, iter) il2cpp_domain_get_assemblies_iter(domain, iter)
#define VM_ASSEMBLY_GET_NAME(assembly) il2cpp_assembly_get_name(assembly)
#define VM_ASSEMBLY_FREE_NAME(name) g_free(name)
#define VM_ASSEMBLY_IS_DYNAMIC(assembly) FALSE
#define VM_ASSEMBLY_GET_IMAGE(assembly) il2cpp_mono_assembly_get_image(assembly)
#define VM_ASSEMBLY_NAME_GET_NAME(assembly) il2cpp_assembly_name_name(assembly)
#define VM_ASSEMBLY_NAME_GET_MAJOR(assembly) il2cpp_assembly_name_major(assembly)
#define VM_ASSEMBLY_NAME_GET_MINOR(assembly) il2cpp_assembly_name_minor(assembly)
#define VM_ASSEMBLY_NAME_GET_BUILD(assembly) il2cpp_assembly_name_build(assembly)
#define VM_ASSEMBLY_NAME_GET_REVISION(assembly) il2cpp_assembly_name_revision(assembly)
#define VM_ASSEMBLY_NAME_GET_CULTURE(assembly) il2cpp_assembly_name_culture(assembly)
#define VM_ASSEMBLY_NAME_GET_PUBLIC_KEY_TOKEN(assembly, i) il2cpp_assembly_name_public_key_token(assembly, i)
#define VM_ASSEMBLY_NAME_GET_PUBLIC_KEY_TOKEN_STRING(assembly) il2cpp_assembly_name_public_key_token_string(assembly)
#define VM_ASSEMBLY_NAME_GET_FLAGS(assembly) il2cpp_assembly_name_flags(assembly)
#define VM_CLASS_GET_TYPE(klass) il2cpp_class_get_type(klass)
#define VM_CLASS_GET_THIS_ARG(klass) il2cpp_class_this_arg(klass)
#define VM_CLASS_GET_ELEMENT_CLASS(klass) il2cpp_class_get_element_class(klass)
#define VM_CLASS_GET_PARENT(klass) il2cpp_class_get_parent(klass)
#define VM_CLASS_GET_IMAGE(klass) il2cpp_class_get_image(klass)
#define VM_CLASS_IS_VALUETYPE(klass) il2cpp_class_is_valuetype(klass)
#define VM_CLASS_IS_INTERFACE(klass) il2cpp_class_is_interface(klass)
#define VM_CLASS_GET_NAME(klass) il2cpp_class_get_name(klass)
#define VM_CLASS_GET_INTERFACES(klass, iter) il2cpp_class_get_interfaces(klass, iter)
#define VM_CLASS_GET_ENUMTYPE(klass) il2cpp_class_get_enumtype(klass)
#define VM_METHOD_GET_WRAPPER_TYPE(method) FALSE
#define VM_METHOD_GET_DECLARING_TYPE(method) il2cpp_method_get_declaring_type(method)
#define VM_METHOD_GET_FLAGS(method) il2cpp_method_get_flags_no_iflags(method)
#define VM_METHOD_GET_NAME(method) il2cpp_method_get_name(method)
#define VM_METHOD_IS_GENERIC(method) il2cpp_method_is_generic(method)
#define VM_METHOD_IS_INFLATED(method) il2cpp_method_is_inflated(method)
#define VM_METHOD_IS_STRING_CTOR(method) il2cpp_method_is_string_ctor(method)
#define VM_INFLATED_METHOD_GET_DECLARING(imethod) il2cpp_method_get_generic_definition(imethod)
#define VM_INFLATED_METHOD_GET_CLASS_INST(imethod) il2cpp_method_get_generic_class_inst(imethod)
#define VM_FIELD_GET_NAME(field) il2cpp_mono_field_get_name(field)
#define VM_FIELD_GET_PARENT(field) il2cpp_field_get_parent(field)
#define VM_FIELD_GET_TYPE(field) il2cpp_field_get_type(field)
#define VM_FIELD_GET_ADDRESS(obj, field) il2cpp_field_get_address(obj, field)
#define VM_FIELD_IS_DELETED(field) il2cpp_field_is_deleted(field)
#define VM_FIELD_GET_OFFSET(field) il2cpp_field_get_offset(field)
#define VM_TYPE_GET_ATTRS(type) il2cpp_mono_type_get_attrs(type)
#define VM_TYPE_GET_TYPE(type) il2cpp_type_get_type(type)
#define VM_TYPE_IS_BYREF(type) il2cpp_type_is_byref(type)
#define VM_TYPE_GET_GENERIC_CLASS(type) il2cpp_type_get_generic_class(type)
#define VM_OBJECT_GET_DOMAIN(object) il2cpp_mono_domain_get()
#define VM_OBJECT_GET_CLASS(object) il2cpp_object_get_class(object)
#define VM_OBJECT_GET_TYPE(object) il2cpp_mono_object_get_type(object)
#define VM_GENERIC_CLASS_GET_INST(gklass) il2cpp_generic_class_get_inst(gklass)
#define VM_GENERIC_CLASS_GET_CONTAINER_CLASS(gklass) il2cpp_generic_class_get_container_class(gklass)
#define VM_GENERIC_CONTAINER_GET_TYPE_ARGC(container) il2cpp_generic_container_get_type_argc(container)
#define VM_GENERIC_INST_TYPE_ARGC(inst) il2cpp_generic_inst_type_argc(inst)
#define VM_GENERIC_INST_TYPE_ARG(inst, i) il2cpp_generic_inst_type_arg(inst, i)
#define VM_DEFAULTS_OBJECT_CLASS il2cpp_defaults_object_class()
#define VM_DEFAULTS_EXCEPTION_CLASS il2cpp_defaults_exception_class()
#define VM_DEFAULTS_CORLIB_IMAGE il2cpp_defaults_corlib_image()
#define VM_DEFAULTS_VOID_CLASS il2cpp_defaults_void_class()
#define VM_ARRAY_GET_RANK(arr) il2cpp_array_rank(arr)
#define VM_ARRAY_BOUND_LENGTH(arr, i) il2cpp_array_bound_length(arr, i)
#define VM_ARRAY_BOUND_LOWER_BOUND(arr, i) il2cpp_array_bound_lower_bound(arr, i)
#define VM_IMAGE_GET_NAME(image) il2cpp_image_name(image)
#define VM_IMAGE_GET_MODULE_NAME(image) il2cpp_image_name(image)
#define VM_IMAGE_GET_ASSEMBLY(image) il2cpp_image_assembly(image)
#define VM_PROPERTY_GET_NAME(prop) il2cpp_property_get_name(prop)
#define VM_PROPERTY_GET_GET_METHOD(prop) il2cpp_property_get_get_method(prop)
#define VM_PROPERTY_GET_SET_METHOD(prop) il2cpp_property_get_set_method(prop)
#define VM_PROPERTY_GET_ATTRS(prop) il2cpp_property_get_flags(prop)
#else
#define VM_THREAD_GET_INTERNAL(thread) thread->internal_thread
#define VM_INTERNAL_THREAD_SET_STATE_BACKGROUND(internal_thread) internal_thread->state |= ThreadState_Background
#define VM_INTERNAL_THREAD_SET_FLAG_DONT_MANAGE(internal_thread) internal_thread->flags |= MONO_THREAD_FLAG_DONT_MANAGE
#define VM_INTERNAL_THREAD_GET_ID(internal_thread) internal_thread->tid
#define VM_INTERNAL_THREAD_GET_STATE(internal_thread) internal_thread->state
#define VM_INTERNAL_THREAD_GET_THREADPOOL_THREAD(internal_thread) internal_thread->threadpool_thread
#define VM_DOMAIN_GET_AGENT_INFO(domain) domain_jit_info (domain)->agent_info
#define VM_DOMAIN_SET_AGENT_INFO(domain, value) domain_jit_info (domain)->agent_info = value
#define VM_DOMAIN_GET_NAME(domain) domain->friendly_name
#define VM_DOMAIN_GET_CORLIB(domain) domain->domain->mbr.obj.vtable->klass->image->assembly
#define VM_DOMAIN_GET_ASSEMBLIES(domain, iter) mono_domain_get_assemblies_iter(domain, iter)
#define VM_ASSEMBLY_GET_NAME(assembly) assembly->aname.name
#define VM_ASSEMBLY_FREE_NAME(name)
#define VM_ASSEMBLY_IS_DYNAMIC(assembly) assembly->image->dynamic
#define VM_ASSEMBLY_GET_IMAGE(assembly) assembly->image
#define VM_ASSEMBLY_NAME_GET_NAME(assembly) (assembly)->aname.name
#define VM_ASSEMBLY_NAME_GET_MAJOR(assembly) (assembly)->aname.major
#define VM_ASSEMBLY_NAME_GET_MINOR(assembly) (assembly)->aname.minor
#define VM_ASSEMBLY_NAME_GET_BUILD(assembly) (assembly)->aname.build
#define VM_ASSEMBLY_NAME_GET_REVISION(assembly) (assembly)->aname.revision
#define VM_ASSEMBLY_NAME_GET_CULTURE(assembly) (assembly)->aname.culture
#define VM_ASSEMBLY_NAME_GET_PUBLIC_KEY_TOKEN(assembly, i) (assembly)->aname.public_key_token[i]
#define VM_ASSEMBLY_NAME_GET_PUBLIC_KEY_TOKEN_STRING(assembly) (char*)(assembly)->aname.public_key_token
#define VM_ASSEMBLY_NAME_GET_FLAGS(assembly) (assembly)->aname.flags
#define VM_CLASS_GET_TYPE(klass) &(klass)->byval_arg
#define VM_CLASS_GET_THIS_ARG(klass) &(klass)->this_arg
#define VM_CLASS_GET_PARENT(klass) (klass)->parent
#define VM_CLASS_GET_IMAGE(klass) (klass)->image
#define VM_CLASS_IS_VALUETYPE(klass) klass->valuetype
#define VM_CLASS_IS_INTERFACE(klass) MONO_CLASS_IS_INTERFACE(klass)
#define VM_CLASS_GET_NAME(klass) (klass)->name
#define VM_CLASS_GET_INTERFACES(klass, iter) mono_class_get_interfaces(klass, iter)
#define VM_CLASS_GET_ENUMTYPE(klass) (klass)->enumtype
#define VM_METHOD_GET_WRAPPER_TYPE(method) method->wrapper_type
#define VM_METHOD_GET_DECLARING_TYPE(method) (method)->klass
#define VM_METHOD_GET_FLAGS(method) (method)->flags
#define VM_METHOD_GET_NAME(method) (method)->name
#define VM_METHOD_IS_GENERIC(method) method->is_generic
#define VM_METHOD_IS_INFLATED(method) method->is_inflated
#define VM_METHOD_IS_STRING_CTOR(method) method->string_ctor
#define VM_INFLATED_METHOD_GET_DECLARING(imethod) (imethod)->declaring
#define VM_INFLATED_METHOD_GET_CLASS_INST(imethod) (imethod)->context.class_inst
#define VM_FIELD_GET_NAME(field) field->name
#define VM_FIELD_GET_PARENT(field) (field)->parent
#define VM_FIELD_GET_TYPE(field) (field)->type
#define VM_FIELD_GET_ADDRESS(obj, field) (guint8*)(obj) + (f)->offset
#define VM_FIELD_IS_DELETED(field) mono_field_is_deleted(field)
#define VM_FIELD_GET_OFFSET(field) (field)->offset
#define VM_TYPE_GET_ATTRS(type) type->attrs
#define VM_TYPE_GET_TYPE(typeparam) (typeparam)->type
#define VM_TYPE_IS_BYREF(type) (type)->byref
#define VM_TYPE_GET_GENERIC_CLASS(type) (type)->data.generic_class
#define VM_OBJECT_GET_DOMAIN(object) ((MonoObject*)object)->vtable->domain
#define VM_OBJECT_GET_CLASS(object) ((MonoObject*)object)->vtable->klass
#define VM_OBJECT_GET_TYPE(object) ((MonoReflectionType*)object->vtable->type)->type
#define VM_GENERIC_CONTAINER_GET_TYPE_ARGC(container) container->type_argc
#define VM_GENERIC_CLASS_GET_INST(gklass) (gklass)->context.class_inst
#define VM_GENERIC_CLASS_GET_CONTAINER_CLASS(gklass) (gklass)->container_class
#define VM_GENERIC_INST_TYPE_ARGC(inst) (inst)->type_argc
#define VM_GENERIC_INST_TYPE_ARG(inst, i) (inst)->type_argv[i]
#define VM_DEFAULTS_OBJECT_CLASS mono_defaults.object_class
#define VM_DEFAULTS_EXCEPTION_CLASS mono_defaults.exception_class
#define VM_DEFAULTS_CORLIB_IMAGE mono_defaults.corlib
#define VM_DEFAULTS_VOID_CLASS mono_defaults.void_class
#define VM_ARRAY_GET_RANK(arr) (arr)->obj.vtable->klass->rank
#define VM_CLASS_GET_ELEMENT_CLASS(klass) (klass)->element_class
#define VM_ARRAY_BOUND_LENGTH(arr, i) arr->bounds[i].length
#define VM_ARRAY_BOUND_LOWER_BOUND(arr, i) arr->bounds[i].lower_bound
#define VM_IMAGE_GET_NAME(image) (image)->name
#define VM_IMAGE_GET_MODULE_NAME(image) (image)->module_name
#define VM_IMAGE_GET_ASSEMBLY(image)  (image)->assembly
#define VM_PROPERTY_GET_NAME(prop) (prop)->name
#define VM_PROPERTY_GET_GET_METHOD(prop) (prop)->get
#define VM_PROPERTY_GET_SET_METHOD(prop) (prop)->set
#define VM_PROPERTY_GET_ATTRS(prop) (prop)->attrs
#endif

#if defined(RUNTIME_IL2CPP)

#define MonoType Il2CppMonoType
#define MonoClass Il2CppMonoClass
#define MonoMethodHeader Il2CppMonoMethodHeader
#define MonoVTable Il2CppMonoVTable
#define MonoAssembly Il2CppMonoAssembly
#define MonoProperty Il2CppMonoProperty
#define MonoString Il2CppMonoString
#define MonoAppDomain Il2CppMonoAppDomain
#define MonoDomain Il2CppMonoDomain
#define MonoImage Il2CppMonoImage
#define MonoMethodSignature Il2CppMonoMethodSignature
#define MonoMethod Il2CppMonoMethod
#define MonoClassField Il2CppMonoClassField
#define MonoArrayType Il2CppMonoArrayType
#define MonoGenericParam Il2CppMonoGenericParam
#define MonoGenericInst Il2CppMonoGenericInst
#define MonoGenericContext Il2CppMonoGenericContext
#define MonoGenericClass Il2CppMonoGenericClass
#define MonoAssemblyName Il2CppMonoAssemblyNameReplacement
#define MonoMarshalByRefObject Il2CppMonoMarshalByRefObject
#define MonoObject Il2CppMonoObject
#define MonoArray Il2CppMonoArray
#define MonoCustomAttrInfo Il2CppMonoCustomAttrInfo
#define MonoThread Il2CppMonoThread
#define MonoInternalThread Il2CppMonoInternalThread
#define MonoGHashTable Il2CppMonoGHashTable
#define MonoGenericContainer Il2CppMonoGenericContainer
#define MonoReflectionAssemblyHandle Il2CppMonoReflectionAssemblyHandle
#define MonoReflectionType Il2CppMonoReflectionType
#define MonoProfiler Il2CppMonoProfiler
#define MonoJitTlsData Il2CppMonoJitTlsData
#define MonoRuntimeExceptionHandlingCallbacks Il2CppMonoRuntimeExceptionHandlingCallbacks
#define MonoCustomAttrEntry Il2CppMonoCustomAttrEntry
#define StackFrameInfo Il2CppMonoStackFrameInfo
#define MonoMethodInflated Il2CppMonoMethodInflated
#define MonoException Il2CppMonoException
#define CattrNamedArg Il2CppCattrNamedArg
#define MonoExceptionClause Il2CppMonoExceptionClause
#define debug_options il2cpp_mono_debug_options
#define MonoTypeNameParse Il2CppMonoTypeNameParse

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
#define mono_custom_attrs_has_attr il2cpp_mono_custom_attrs_has_attr
#define mono_custom_attrs_free il2cpp_mono_custom_attrs_free
#define mono_get_root_domain il2cpp_mono_get_root_domain
#define mono_runtime_quit il2cpp_mono_runtime_quit
#define mono_runtime_is_shutting_down il2cpp_mono_runtime_is_shutting_down
#define mono_domain_get il2cpp_mono_domain_get
#define mono_domain_set il2cpp_mono_domain_set
#define mono_domain_foreach il2cpp_mono_domain_foreach
#define mono_jit_info_table_find il2cpp_mono_jit_info_table_find
#define mono_jit_info_get_method il2cpp_mono_jit_info_get_method
#define mono_debug_lookup_method il2cpp_mono_debug_lookup_method
#define mono_debug_find_method il2cpp_mono_debug_find_method
#define mono_debug_free_method_jit_info il2cpp_mono_debug_free_method_jit_info
#define mono_debug_lookup_locals il2cpp_mono_debug_lookup_locals
#define mono_debug_lookup_method_async_debug_info il2cpp_mono_debug_lookup_method_async_debug_info
#define mono_debug_method_lookup_location il2cpp_mono_debug_method_lookup_location
#define mono_debug_il_offset_from_address il2cpp_mono_debug_il_offset_from_address
#define mono_debug_free_source_location il2cpp_mono_debug_free_source_location
#define mono_set_is_debugger_attached il2cpp_mono_set_is_debugger_attached
#define mono_type_full_name il2cpp_mono_type_full_name
#define mono_method_full_name il2cpp_mono_method_full_name
#define mono_debug_get_seq_points il2cpp_mono_debug_get_seq_points
#define mono_debug_free_locals il2cpp_mono_debug_free_locals
#define mono_debug_free_method_async_debug_info il2cpp_mono_debug_free_method_async_debug_info
#define mono_thread_current il2cpp_mono_thread_current
#define mono_thread_get_main il2cpp_mono_thread_get_main
#define mono_thread_attach il2cpp_mono_thread_attach
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
#define mono_reflection_create_custom_attr_data_args il2cpp_mono_reflection_create_custom_attr_data_args
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
#define mono_custom_attrs_from_method_checked il2cpp_mono_custom_attrs_from_method_checked
#define mono_custom_attrs_from_class_checked il2cpp_mono_custom_attrs_from_class_checked
#define mono_custom_attrs_from_property_checked il2cpp_mono_custom_attrs_from_property_checked
#define mono_custom_attrs_from_field_checked il2cpp_mono_custom_attrs_from_field_checked
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
#define mono_arch_init_lmf_ext il2cpp_mono_arch_init_lmf_ext
#define mono_arch_context_get_int_reg il2cpp_mono_arch_context_get_int_reg
#define mono_arch_context_set_int_reg il2cpp_mono_arch_context_set_int_reg
#define mono_walk_stack_with_ctx il2cpp_mono_walk_stack_with_ctx
#define mono_walk_stack_with_state il2cpp_mono_walk_stack_with_state
#define mono_thread_state_init_from_current il2cpp_mono_thread_state_init_from_current
#define mono_thread_state_init_from_monoctx il2cpp_mono_thread_state_init_from_monoctx
#define mini_jit_info_table_find il2cpp_mini_jit_info_table_find
#define mono_restore_context il2cpp_mono_restore_context
#define mono_find_jit_info_ext il2cpp_mono_find_jit_info_ext
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
#define mono_class_get_type il2cpp_class_get_type
#define mono_class_get_image il2cpp_class_get_image
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

#define mono_domain_assemblies_lock
#define mono_domain_assemblies_unlock

#define mono_get_string_class il2cpp_mono_get_string_class

Il2CppMonoMethod* il2cpp_mono_image_get_entry_point (Il2CppMonoImage *image);
const char* il2cpp_mono_image_get_filename (Il2CppMonoImage *image);
const char*  il2cpp_mono_image_get_guid (Il2CppMonoImage *image);
Il2CppMonoClass* il2cpp_mono_type_get_class (Il2CppMonoType *type);
mono_bool il2cpp_mono_type_is_struct (Il2CppMonoType *type);
mono_bool il2cpp_mono_type_is_reference (Il2CppMonoType *type);
void il2cpp_mono_metadata_free_mh (Il2CppMonoMethodHeader *mh);
Il2CppMonoMethodSignature* il2cpp_mono_method_signature (Il2CppMonoMethod *m);
void il2cpp_mono_method_get_param_names (Il2CppMonoMethod *method, const char **names);
mono_bool il2cpp_mono_type_generic_inst_is_valuetype (Il2CppMonoType *type);
Il2CppMonoMethodHeader* il2cpp_mono_method_get_header_checked (Il2CppMonoMethod *method, MonoError *error);
gboolean il2cpp_mono_class_init (Il2CppMonoClass *klass);
Il2CppMonoVTable* il2cpp_mono_class_vtable (Il2CppMonoDomain *domain, Il2CppMonoClass *klass);
Il2CppMonoClassField* il2cpp_mono_class_get_field_from_name (Il2CppMonoClass *klass, const char *name);
int32_t il2cpp_mono_array_element_size (Il2CppMonoClass *ac);
int32_t il2cpp_mono_class_instance_size (Il2CppMonoClass *klass);
int32_t il2cpp_mono_class_value_size (Il2CppMonoClass *klass, uint32_t *align);
gboolean il2cpp_mono_class_is_assignable_from (Il2CppMonoClass *klass, Il2CppMonoClass *oklass);
Il2CppMonoClass* il2cpp_mono_class_from_mono_type (Il2CppMonoType *type);
int il2cpp_mono_class_num_fields (Il2CppMonoClass *klass);
int il2cpp_mono_class_num_methods (Il2CppMonoClass *klass);
int il2cpp_mono_class_num_properties (Il2CppMonoClass *klass);
Il2CppMonoClassField* il2cpp_mono_class_get_fields (Il2CppMonoClass* klass, gpointer *iter);
Il2CppMonoMethod* il2cpp_mono_class_get_methods (Il2CppMonoClass* klass, gpointer *iter);
Il2CppMonoProperty* il2cpp_mono_class_get_properties (Il2CppMonoClass* klass, gpointer *iter);
const char* il2cpp_mono_field_get_name (Il2CppMonoClassField *field);
mono_unichar2* il2cpp_mono_string_chars (Il2CppMonoString *s);
int il2cpp_mono_string_length (Il2CppMonoString *s);
char* il2cpp_mono_array_addr_with_size (Il2CppMonoArray *array, int size, uintptr_t idx);
uintptr_t il2cpp_mono_array_length (Il2CppMonoArray *array);
Il2CppMonoString* il2cpp_mono_string_new (Il2CppMonoDomain *domain, const char *text);
Il2CppMonoString* il2cpp_mono_string_new (Il2CppMonoDomain *domain, const char *text);
Il2CppMonoString* il2cpp_mono_string_new_checked (Il2CppMonoDomain *domain, const char *text, MonoError *merror);
char* il2cpp_mono_string_to_utf8_checked (Il2CppMonoString *string_obj, MonoError *error);
int il2cpp_mono_object_hash (Il2CppMonoObject* obj);
void* il2cpp_mono_object_unbox (Il2CppMonoObject *obj);
void il2cpp_mono_field_set_value (Il2CppMonoObject *obj, Il2CppMonoClassField *field, void *value);
void il2cpp_mono_field_static_set_value (Il2CppMonoVTable *vt, Il2CppMonoClassField *field, void *value);
uint32_t il2cpp_mono_gchandle_new_weakref (Il2CppMonoObject *obj, mono_bool track_resurrection);
Il2CppMonoObject*  il2cpp_mono_gchandle_get_target (uint32_t gchandle);
void il2cpp_mono_gchandle_free (uint32_t gchandle);
void il2cpp_mono_gc_wbarrier_generic_store (void* ptr, Il2CppMonoObject* value);
int il2cpp_mono_reflection_parse_type_checked (char *name, Il2CppMonoTypeNameParse *info, MonoError *error);
void il2cpp_mono_reflection_free_type_info (Il2CppMonoTypeNameParse *info);
mono_bool il2cpp_mono_custom_attrs_has_attr (Il2CppMonoCustomAttrInfo *ainfo, Il2CppMonoClass *attr_klass);
void il2cpp_mono_custom_attrs_free (Il2CppMonoCustomAttrInfo *ainfo);
Il2CppMonoDomain* il2cpp_mono_get_root_domain (void);
void il2cpp_mono_runtime_quit (void);
gboolean il2cpp_mono_runtime_is_shutting_down (void);
Il2CppMonoDomain* il2cpp_mono_domain_get (void);
gboolean il2cpp_mono_domain_set (Il2CppMonoDomain *domain, gboolean force);
void il2cpp_mono_domain_foreach(Il2CppMonoDomainFunc func, gpointer user_data);
MonoJitInfo* il2cpp_mono_jit_info_table_find(Il2CppMonoDomain* domain, char* addr);
Il2CppMonoMethod* il2cpp_mono_jit_info_get_method(MonoJitInfo* ji);
MonoDebugMethodInfo* il2cpp_mono_debug_lookup_method(Il2CppMonoMethod* method);
MonoDebugMethodJitInfo* il2cpp_mono_debug_find_method(Il2CppMonoMethod* method, Il2CppMonoDomain* domain);
void il2cpp_mono_debug_free_method_jit_info(MonoDebugMethodJitInfo* jit);
MonoDebugLocalsInfo* il2cpp_mono_debug_lookup_locals(Il2CppMonoMethod* method);
MonoDebugMethodAsyncInfo* il2cpp_mono_debug_lookup_method_async_debug_info(Il2CppMonoMethod* method);
MonoDebugSourceLocation* il2cpp_mono_debug_method_lookup_location(MonoDebugMethodInfo* minfo, int il_offset);
gint32 il2cpp_mono_debug_il_offset_from_address(Il2CppMonoMethod* method, Il2CppMonoDomain* domain, guint32 native_offset);
void il2cpp_mono_debug_free_source_location(MonoDebugSourceLocation* location);
void il2cpp_mono_set_is_debugger_attached(gboolean attached);
char* il2cpp_mono_type_full_name(Il2CppMonoType* type);
char* il2cpp_mono_method_full_name(Il2CppMonoMethod* method, gboolean signature);
void il2cpp_mono_debug_get_seq_points(MonoDebugMethodInfo* minfo, char** source_file, GPtrArray** source_file_list, int** source_files, MonoSymSeqPoint** seq_points, int* n_seq_points);
void il2cpp_mono_debug_free_locals(MonoDebugLocalsInfo* info);
void il2cpp_mono_debug_free_method_async_debug_info(MonoDebugMethodAsyncInfo* info);
Il2CppMonoThread* il2cpp_mono_thread_current();
Il2CppMonoThread* il2cpp_mono_thread_get_main();
Il2CppMonoThread* il2cpp_mono_thread_attach(Il2CppMonoDomain* domain);
void il2cpp_mono_domain_lock(Il2CppMonoDomain* domain);
void il2cpp_mono_domain_unlock(Il2CppMonoDomain* domain);
MonoJitInfo* il2cpp_mono_jit_info_table_find_internal(Il2CppMonoDomain* domain, char* addr, gboolean try_aot, gboolean allow_trampolines);
guint il2cpp_mono_aligned_addr_hash(gconstpointer ptr);
Il2CppMonoGenericInst* il2cpp_mono_metadata_get_generic_inst(int type_argc, Il2CppMonoType** type_argv);
Il2CppMonoMethod* il2cpp_mono_get_method_checked(Il2CppMonoImage* image, guint32 token, Il2CppMonoClass* klass, Il2CppMonoGenericContext* context, MonoError* error);
int il2cpp_mono_class_interface_offset_with_variance(Il2CppMonoClass* klass, Il2CppMonoClass* itf, gboolean* non_exact_match);
void il2cpp_mono_class_setup_supertypes(Il2CppMonoClass* klass);
void il2cpp_mono_class_setup_vtable(Il2CppMonoClass* klass);
void il2cpp_mono_class_setup_methods(Il2CppMonoClass* klass);
gboolean il2cpp_mono_class_field_is_special_static(Il2CppMonoClassField* field);
guint32 il2cpp_mono_class_field_get_special_static_type(Il2CppMonoClassField* field);
Il2CppMonoGenericContext* il2cpp_mono_class_get_context(Il2CppMonoClass* klass);
Il2CppMonoGenericContext* il2cpp_mono_method_get_context(Il2CppMonoMethod* method);
Il2CppMonoGenericContainer* il2cpp_mono_method_get_generic_container(Il2CppMonoMethod* method);
Il2CppMonoMethod* il2cpp_mono_class_inflate_generic_method_full_checked(Il2CppMonoMethod* method, Il2CppMonoClass* klass_hint, Il2CppMonoGenericContext* context, MonoError* error);
Il2CppMonoMethod* il2cpp_mono_class_inflate_generic_method_checked(Il2CppMonoMethod* method, Il2CppMonoGenericContext* context, MonoError* error);
void il2cpp_mono_loader_lock();
void il2cpp_mono_loader_unlock();
void il2cpp_mono_loader_lock_track_ownership(gboolean track);
gboolean il2cpp_mono_loader_lock_is_owned_by_self();
gpointer il2cpp_mono_method_get_wrapper_data(Il2CppMonoMethod* method, guint32 id);
char* il2cpp_mono_type_get_name_full(Il2CppMonoType* type, MonoTypeNameFormat format);
gboolean il2cpp_mono_class_is_nullable(Il2CppMonoClass* klass);
Il2CppMonoGenericContainer* il2cpp_mono_class_get_generic_container(Il2CppMonoClass* klass);
void il2cpp_mono_class_setup_interfaces(Il2CppMonoClass* klass, MonoError* error);
GPtrArray* il2cpp_mono_class_get_methods_by_name(Il2CppMonoClass* klass, const char* name, guint32 bflags, gboolean ignore_case, gboolean allow_ctors, MonoError* error);
gpointer il2cpp_mono_ldtoken_checked(Il2CppMonoImage* image, guint32 token, Il2CppMonoClass** handle_class, Il2CppMonoGenericContext* context, MonoError* error);
Il2CppMonoClass* il2cpp_mono_class_from_generic_parameter_internal(Il2CppMonoGenericParam* param);
Il2CppMonoClass* il2cpp_mono_class_load_from_name(Il2CppMonoImage* image, const char* name_space, const char* name);
Il2CppMonoGenericClass* il2cpp_mono_class_get_generic_class(Il2CppMonoClass* klass);
Il2CppMonoInternalThread* il2cpp_mono_thread_internal_current();
gboolean il2cpp_mono_thread_internal_is_current(Il2CppMonoInternalThread* thread);
void il2cpp_mono_thread_internal_abort(Il2CppMonoInternalThread* thread, gboolean appdomain_unload);
void il2cpp_mono_thread_internal_reset_abort(Il2CppMonoInternalThread* thread);
gunichar2* il2cpp_mono_thread_get_name(Il2CppMonoInternalThread* this_obj, guint32* name_len);
void il2cpp_mono_thread_set_name_internal(Il2CppMonoInternalThread* this_obj, Il2CppMonoString* name, gboolean permanent, gboolean reset, MonoError* error);
void il2cpp_mono_thread_suspend_all_other_threads();
void il2cpp_mono_stack_mark_record_size(MonoThreadInfo* info, HandleStackMark* stackmark, const char* func_name);
Il2CppMonoRuntimeExceptionHandlingCallbacks* il2cpp_mono_get_eh_callbacks();
void il2cpp_mono_reflection_create_custom_attr_data_args(Il2CppMonoImage* image, Il2CppMonoMethod* method, const guchar* data, guint32 len, Il2CppMonoArray** typed_args, Il2CppMonoArray** named_args, CattrNamedArg** named_arg_info, MonoError* error);
void il2cpp_mono_nullable_init(guint8* buf, Il2CppMonoObject* value, Il2CppMonoClass* klass);
Il2CppMonoObject* il2cpp_mono_value_box_checked(Il2CppMonoDomain* domain, Il2CppMonoClass* klass, gpointer value, MonoError* error);
void il2cpp_mono_field_static_get_value_checked(Il2CppMonoVTable* vt, Il2CppMonoClassField* field, void* value, MonoError* error);
void il2cpp_mono_field_static_get_value_for_thread(Il2CppMonoInternalThread* thread, Il2CppMonoVTable* vt, Il2CppMonoClassField* field, void* value, MonoError* error);
Il2CppMonoObject* il2cpp_mono_field_get_value_object_checked(Il2CppMonoDomain* domain, Il2CppMonoClassField* field, Il2CppMonoObject* obj, MonoError* error);
Il2CppMonoObject* il2cpp_mono_object_new_checked(Il2CppMonoDomain* domain, Il2CppMonoClass* klass, MonoError* error);
Il2CppMonoString* il2cpp_mono_ldstr_checked(Il2CppMonoDomain* domain, Il2CppMonoImage* image, guint32 idx, MonoError* error);
Il2CppMonoObject* il2cpp_mono_runtime_try_invoke(Il2CppMonoMethod* method, void* obj, void** params, Il2CppMonoObject** exc, MonoError* error);
Il2CppMonoObject* il2cpp_mono_runtime_invoke_checked(Il2CppMonoMethod* method, void* obj, void** params, MonoError* error);
void il2cpp_mono_gc_base_init();
int il2cpp_mono_gc_register_root(char* start, size_t size, MonoGCDescriptor descr, MonoGCRootSource source, const char* msg);
void il2cpp_mono_gc_deregister_root(char* addr);
gint32 il2cpp_mono_environment_exitcode_get();
void il2cpp_mono_environment_exitcode_set(gint32 value);
void il2cpp_mono_threadpool_suspend();
void il2cpp_mono_threadpool_resume();
Il2CppMonoImage* il2cpp_mono_assembly_get_image(Il2CppMonoAssembly* assembly);
gboolean il2cpp_mono_runtime_try_shutdown();
gboolean il2cpp_mono_verifier_is_method_valid_generic_instantiation(Il2CppMonoMethod* method);
Il2CppMonoType* il2cpp_mono_reflection_get_type_checked(Il2CppMonoImage* rootimage, Il2CppMonoImage* image, Il2CppMonoTypeNameParse* info, gboolean ignorecase, gboolean* type_resolve, MonoError* error);
Il2CppMonoCustomAttrInfo* il2cpp_mono_custom_attrs_from_method_checked(Il2CppMonoMethod* method, MonoError* error);
Il2CppMonoCustomAttrInfo* il2cpp_mono_custom_attrs_from_class_checked(Il2CppMonoClass* klass, MonoError* error);
Il2CppMonoCustomAttrInfo* il2cpp_mono_custom_attrs_from_property_checked(Il2CppMonoClass* klass, Il2CppMonoProperty* property, MonoError* error);
Il2CppMonoCustomAttrInfo* il2cpp_mono_custom_attrs_from_field_checked(Il2CppMonoClass* klass, Il2CppMonoClassField* field, MonoError* error);
Il2CppMonoReflectionAssemblyHandle il2cpp_mono_assembly_get_object_handle(Il2CppMonoDomain* domain, Il2CppMonoAssembly* assembly, MonoError* error);
Il2CppMonoReflectionType* il2cpp_mono_type_get_object_checked(Il2CppMonoDomain* domain, Il2CppMonoType* type, MonoError* error);
void il2cpp_mono_network_init();
gint il2cpp_mono_w32socket_set_blocking(SOCKET sock, gboolean blocking);

char* il2cpp_mono_get_runtime_build_info();
Il2CppMonoMethod* il2cpp_mono_marshal_method_from_wrapper(Il2CppMonoMethod* wrapper);
MonoDebugOptions* il2cpp_mini_get_debug_options();
gpointer il2cpp_mono_jit_find_compiled_method_with_jit_info(Il2CppMonoDomain* domain, Il2CppMonoMethod* method, MonoJitInfo** ji);
MonoLMF** il2cpp_mono_get_lmf_addr();
void il2cpp_mono_set_lmf(MonoLMF* lmf);
gpointer il2cpp_mono_aot_get_method_checked(Il2CppMonoDomain* domain, Il2CppMonoMethod* method, MonoError* error);
void il2cpp_mono_arch_setup_resume_sighandler_ctx(MonoContext* ctx, gpointer func);
void il2cpp_mono_arch_set_breakpoint(MonoJitInfo* ji, guint8* ip);
void il2cpp_mono_arch_clear_breakpoint(MonoJitInfo* ji, guint8* ip);
void il2cpp_mono_arch_start_single_stepping();
void il2cpp_mono_arch_stop_single_stepping();
void il2cpp_mono_arch_skip_breakpoint(MonoContext* ctx, MonoJitInfo* ji);
void il2cpp_mono_arch_skip_single_step(MonoContext* ctx);
void il2cpp_mono_arch_init_lmf_ext(MonoLMFExt* ext, gpointer prev_lmf);
mgreg_t il2cpp_mono_arch_context_get_int_reg(MonoContext* ctx, int reg);
void il2cpp_mono_arch_context_set_int_reg(MonoContext* ctx, int reg, mgreg_t val);
void il2cpp_mono_walk_stack_with_ctx(Il2CppMonoJitStackWalk func, MonoContext* start_ctx, MonoUnwindOptions unwind_options, void* user_data);
void il2cpp_mono_walk_stack_with_state(Il2CppMonoJitStackWalk func, MonoThreadUnwindState* state, MonoUnwindOptions unwind_options, void* user_data);
gboolean il2cpp_mono_thread_state_init_from_current(MonoThreadUnwindState* ctx);
gboolean il2cpp_mono_thread_state_init_from_monoctx(MonoThreadUnwindState* ctx, MonoContext* mctx);
MonoJitInfo* il2cpp_mini_jit_info_table_find(Il2CppMonoDomain* domain, char* addr, Il2CppMonoDomain** out_domain);
void il2cpp_mono_restore_context(MonoContext* ctx);
gboolean il2cpp_mono_find_jit_info_ext(Il2CppMonoDomain* domain, Il2CppMonoJitTlsData* jit_tls, MonoJitInfo* prev_ji, MonoContext* ctx, MonoContext* new_ctx, char** trace, MonoLMF** lmf, mgreg_t** save_locations, StackFrameInfo* frame);
Il2CppMonoMethod* il2cpp_mono_method_get_declaring_generic_method(Il2CppMonoMethod* method);
Il2CppMonoMethod* il2cpp_jinfo_get_method (MonoJitInfo *ji);
gboolean il2cpp_mono_find_prev_seq_point_for_native_offset (Il2CppMonoDomain *domain, Il2CppMonoMethod *method, gint32 native_offset, MonoSeqPointInfo **info, SeqPoint* seq_point);
SOCKET il2cpp_mono_w32socket_accept_internal (SOCKET s, struct sockaddr *addr, socklen_t *addrlen, gboolean blocking);
gboolean il2cpp_mono_find_next_seq_point_for_native_offset (Il2CppMonoDomain *domain, Il2CppMonoMethod *method, gint32 native_offset, MonoSeqPointInfo **info, SeqPoint* seq_point);
gboolean il2cpp_mono_class_has_parent (Il2CppMonoClass *klass, Il2CppMonoClass *parent);
Il2CppMonoGenericParam* il2cpp_mono_generic_container_get_param (Il2CppMonoGenericContainer *gc, int i);
gboolean il2cpp_mono_find_seq_point (Il2CppMonoDomain *domain, Il2CppMonoMethod *method, gint32 il_offset, MonoSeqPointInfo **info, SeqPoint *seq_point);
void il2cpp_mono_seq_point_iterator_init (SeqPointIterator* it, MonoSeqPointInfo* info);
gboolean il2cpp_mono_seq_point_iterator_next (SeqPointIterator* it);
void il2cpp_mono_seq_point_init_next (MonoSeqPointInfo* info, SeqPoint sp, SeqPoint* next);
MonoSeqPointInfo* il2cpp_mono_get_seq_points (Il2CppMonoDomain *domain, Il2CppMonoMethod *method);
void IL2CPP_G_BREAKPOINT();
void il2cpp_mono_thread_info_safe_suspend_and_run (MonoNativeThreadId id, gboolean interrupt_kernel, MonoSuspendThreadCallback callback, gpointer user_data);
void il2cpp_mono_error_cleanup (MonoError *oerror);
Il2CppMonoException* il2cpp_mono_error_convert_to_exception (MonoError *target_error);
const char* il2cpp_mono_error_get_message (MonoError *oerror);
void il2cpp_mono_error_assert_ok_pos (MonoError *error, const char* filename, int lineno);
Il2CppSequencePointC* il2cpp_get_sequence_points(void* *iter);
Il2CppSequencePointC* il2cpp_get_method_sequence_points(Il2CppMonoMethod* method, void* *iter);
Il2CppMonoGenericInst* il2cpp_generic_class_get_inst(Il2CppMonoGenericClass *monoGenClass);
guint il2cpp_generic_inst_type_argc(Il2CppMonoGenericInst *monoInst);
Il2CppMonoType* il2cpp_generic_inst_type_arg(Il2CppMonoGenericInst *monoInst, int i);
Il2CppMonoType* il2cpp_class_this_arg(Il2CppMonoClass *monoClass);
Il2CppMonoClass* il2cpp_class_get_nested_types_accepts_generic(Il2CppMonoClass *monoClass, void* *iter);
Il2CppMonoClass* il2cpp_defaults_object_class();
guint8 il2cpp_array_rank(Il2CppMonoArray *monoArr);
mono_array_size_t il2cpp_array_bound_length(Il2CppMonoArray *monoArr, int i);
mono_array_lower_bound_t il2cpp_array_bound_lower_bound(Il2CppMonoArray *monoArr, int i);
const char* il2cpp_assembly_name_name(Il2CppMonoAssembly *monoAssembly);
uint16_t il2cpp_assembly_name_major(Il2CppMonoAssembly *monoAssembly);
uint16_t il2cpp_assembly_name_minor(Il2CppMonoAssembly *monoAssembly);
uint16_t il2cpp_assembly_name_build(Il2CppMonoAssembly *monoAssembly);
uint16_t il2cpp_assembly_name_revision(Il2CppMonoAssembly *monoAssembly);
const char* il2cpp_assembly_name_culture(Il2CppMonoAssembly *monoAssembly);
mono_byte il2cpp_assembly_name_public_key_token(Il2CppMonoAssembly *monoAssembly, int i);
const char* il2cpp_assembly_name_public_key_token_string(Il2CppMonoAssembly *monoAssembly);
uint32_t il2cpp_assembly_name_flags(Il2CppMonoAssembly *monoAssembly);
const char* il2cpp_image_name(Il2CppMonoImage *monoImage);
Il2CppMonoAssembly* il2cpp_image_assembly(Il2CppMonoImage *monoImage);
guint8* il2cpp_field_get_address(Il2CppMonoObject *obj, Il2CppMonoClassField *monoField);
Il2CppMonoType* il2cpp_mono_object_get_type(Il2CppMonoObject* object);
Il2CppMonoClass* il2cpp_defaults_exception_class();
Il2CppMonoImage* il2cpp_defaults_corlib_image();
int il2cpp_generic_container_get_type_argc(Il2CppMonoGenericClass* container);
uint32_t il2cpp_method_get_flags_no_iflags (const Il2CppMonoMethod * method);
bool il2cpp_method_is_string_ctor (const Il2CppMonoMethod * method);
Il2CppMonoClass* il2cpp_defaults_void_class();
void il2cpp_set_var(guint8* newValue, void *value, Il2CppMonoType *localVariableTypeMono);
Il2CppMonoMethod* il2cpp_get_interface_method(Il2CppMonoClass* klass, Il2CppMonoClass* itf, int slot);
gboolean il2cpp_field_is_deleted(Il2CppMonoClassField *field);
Il2CppMonoGenericClass* il2cpp_type_get_generic_class(Il2CppMonoType *type);
gboolean il2cpp_class_get_enumtype(Il2CppMonoClass *klass);
Il2CppMonoClass* il2cpp_iterate_loaded_classes(void* *iter);
Il2CppMonoAssembly* il2cpp_domain_get_assemblies_iter(Il2CppMonoAppDomain *domain, void* *iter);
const char** il2cpp_get_source_files_for_type(Il2CppMonoClass *klass, int *count);
Il2CppMonoInternalThread* il2cpp_mono_thread_get_internal(Il2CppMonoThread* thread);
uint32_t il2cpp_internal_thread_get_state(Il2CppMonoInternalThread* thread);
il2cpp_internal_thread_get_threadpool_thread(Il2CppMonoInternalThread* thread);
Il2CppMonoMethod* il2cpp_method_get_generic_definition(Il2CppMonoMethodInflated *imethod);
Il2CppMonoGenericInst* il2cpp_method_get_generic_class_inst(Il2CppMonoMethodInflated *imethod);
Il2CppMonoClass* il2cpp_generic_class_get_container_class(Il2CppMonoGenericClass *gclass);

Il2CppMonoClass* il2cpp_mono_get_string_class (void);

#endif // RUNTIME_IL2CPP
