#ifndef __UNITY_MONO_UTILS_H
#define __UNITY_MONO_UTILS_H

#include <stdio.h>
#include <mono/metadata/object.h>
#include <mono/metadata/reflection.h>
#include <mono/metadata/appdomain.h>

typedef int (*unity_vprintf_func)(const char* msg, va_list args);
typedef MonoObject *(*UnityRuntimeInvokeFunction) (MonoObject *this_obj, void **params, MonoObject **exc, void* compiled_method);

typedef void (*assembly_foreach_func)(MonoAssembly* assembly);
typedef struct {
	void* (*malloc_func)(size_t size);
	void(*free_func)(void *ptr);
	void* (*calloc_func)(size_t nmemb, size_t size);
	void* (*realloc_func)(void *ptr, size_t size);
} MonoMemoryCallbacks;

struct MonoThreadUnwindState;
struct MonoThreadInfo;
struct MonoStackFrame;


/**
 *	Custom exit function, called instead of system exit()
 */
void unity_mono_exit( int code );

/**
 *	Closes redirected output files.
 */
void unity_mono_close_output(void);

MONO_API MonoString* mono_unity_get_embeddinghostname(void);

#ifdef WIN32
FILE* unity_fopen( const char *name, const char *mode );
#endif

MONO_API mono_bool mono_unity_socket_security_enabled_get (void);
MONO_API void mono_unity_socket_security_enabled_set (mono_bool enabled);
MONO_API void mono_unity_set_vprintf_func(unity_vprintf_func func);


// void mono_unity_install_memory_callbacks(MonoMemoryCallbacks* callbacks);

MONO_API mono_bool
mono_unity_method_is_generic (MonoMethod* method);

typedef const char*(*UnityFindPluginCallback)(const char*);

MONO_API void
mono_set_find_plugin_callback(UnityFindPluginCallback find);

MONO_API UnityFindPluginCallback
mono_get_find_plugin_callback(void);

//object
MONO_API void mono_unity_object_init(void* obj, MonoClass* klass);
MONO_API MonoObject* mono_unity_object_isinst_sealed(MonoObject* obj, MonoClass* targetType);
MONO_API void mono_unity_object_unbox_nullable(MonoObject* obj, MonoClass* nullableArgumentClass, void* storage);
MONO_API MonoClass* mono_unity_object_get_class(MonoObject *obj);
MONO_API MonoObject* mono_unity_object_compare_exchange(MonoObject **location, MonoObject *value, MonoObject *comparand);
MONO_API MonoObject* mono_unity_object_exchange(MonoObject **location, MonoObject *value);
MONO_API mono_bool mono_unity_object_check_box_cast(MonoObject *obj, MonoClass *klass);

//class 
MONO_API const char* mono_unity_class_get_image_name(MonoClass* klass);
MONO_API MonoClass* mono_unity_class_get_generic_definition(MonoClass* klass);
MONO_API MonoClass* mono_unity_class_inflate_generic_class(MonoClass *gklass, MonoGenericContext *context);
MONO_API mono_bool mono_unity_class_has_parent_unsafe(MonoClass *klass, MonoClass *parent);
MONO_API MonoAssembly* mono_unity_class_get_assembly(MonoClass *klass);
MONO_API mono_bool mono_unity_class_is_array(MonoClass *klass);
MONO_API MonoClass* mono_unity_class_get_element_class(MonoClass *klass);
MONO_API mono_bool mono_unity_class_is_delegate(MonoClass *klass);
MONO_API int mono_unity_class_get_instance_size(MonoClass *klass);
MONO_API MonoClass* mono_unity_class_get_castclass(MonoClass *klass);
MONO_API uint32_t mono_unity_class_get_native_size(MonoClass* klass);
MONO_API MonoBoolean mono_unity_class_is_string(MonoClass* klass);
MONO_API MonoBoolean mono_unity_class_is_class_type(MonoClass* klass);
MONO_API mono_bool mono_class_is_generic(MonoClass *klass);
MONO_API mono_bool mono_class_is_blittable(MonoClass *klass);
MONO_API mono_bool mono_class_is_inflated(MonoClass *klass);
MONO_API mono_bool mono_unity_class_has_cctor(MonoClass *klass);
MONO_API mono_bool mono_unity_class_has_reference(MonoClass* kclass);
MONO_API mono_bool mono_unity_class_is_abstract (MonoClass* klass);
MONO_API void* mono_unity_vtable_get_static_field_data(MonoVTable *vTable);
MONO_API MonoVTable* mono_unity_class_try_get_vtable(MonoDomain *domain, MonoClass *klass);
MONO_API mono_bool mono_unity_class_field_is_literal(MonoClassField *field);

//method 
MONO_API MonoMethod* mono_unity_method_get_generic_definition(MonoMethod* method);
MONO_API MonoReflectionMethod* mono_unity_method_get_object(MonoMethod *method);
MONO_API MonoMethod* mono_unity_method_alloc0(MonoClass* klass);
MONO_API MonoMethod* mono_unity_method_delegate_invoke_wrapper(MonoClass* klass);
MONO_API mono_bool mono_unity_method_is_static(MonoMethod *method);
MONO_API MonoClass* mono_unity_method_get_class(const MonoMethod *method);
MONO_API MonoMethod* mono_unity_reflection_method_get_method(MonoReflectionMethod* mrf);

#ifdef IL2CPP_ON_MONO
void* mono_unity_method_get_method_pointer(MonoMethod* method);
void mono_unity_method_set_method_pointer(MonoMethod* method, void *p);
void* mono_unity_method_get_invoke_pointer(MonoMethod* method);
void mono_unity_method_set_invoke_pointer(MonoMethod* method, void *p);
#endif

MONO_API const char* mono_unity_method_get_name(const MonoMethod *method);
MONO_API uint64_t mono_unity_method_get_hash(MonoMethod *method, mono_bool inflate);
MONO_API MonoMethod* mono_unity_method_get_aot_array_helper_from_wrapper(MonoMethod *method);
MONO_API MonoObject* mono_unity_method_convert_return_type_if_needed(MonoMethod *method, void *value);
MONO_API mono_bool mono_unity_method_is_inflated(MonoMethod* method);
MONO_API uint32_t mono_unity_method_get_token(MonoMethod *method);
MONO_API const char* mono_unity_method_get_param_name(MonoMethod *method, uint32_t index);

//domain
MONO_API void mono_unity_domain_install_finalize_runtime_invoke(MonoDomain* domain, UnityRuntimeInvokeFunction callback);
MONO_API void mono_unity_domain_install_capture_context_runtime_invoke(MonoDomain* domain, UnityRuntimeInvokeFunction callback);
MONO_API void mono_unity_domain_install_capture_context_method(MonoDomain* domain, void* callback);
MONO_API void mono_unity_domain_unload (MonoDomain *domain, MonoUnityExceptionFunc callback);

//array
MONO_API int mono_unity_array_get_element_size(MonoClass *arr_class);
MONO_API MonoClass* mono_unity_array_get_class(MonoArray *arr);
MONO_API uint32_t mono_unity_array_get_max_length(MonoArray *arr);
MONO_API int mono_unity_array_get_byte_length (MonoArray* array);
MONO_API MonoArray* mono_unity_array_new_specific(MonoClass* arrayClass, uintptr_t size);

//type
MONO_API mono_bool mono_unity_type_is_generic_instance(MonoType *type);
MONO_API MonoGenericClass* mono_unity_type_get_generic_class(MonoType *type);
MONO_API mono_bool mono_unity_type_is_enum_type(MonoType *type);
MONO_API mono_bool mono_unity_type_is_boolean(MonoType *type);
MONO_API MonoClass* mono_unity_type_get_element_class(MonoType *type); //only safe to call when the type has a defined klass data element
MONO_API uint64_t mono_unity_type_get_hash(MonoType *type, mono_bool inflate);
MONO_API mono_bool mono_unity_type_is_static(MonoType *type);

//generic class
MONO_API MonoGenericContext mono_unity_generic_class_get_context(MonoGenericClass *klass);
MONO_API MonoClass* mono_unity_generic_class_get_container_class(MonoGenericClass *klass);

//method signature
MONO_API MonoClass* mono_unity_signature_get_class_for_param(MonoMethodSignature *sig, int index);
MONO_API int mono_unity_signature_num_parameters(MonoMethodSignature *sig);
MONO_API mono_bool mono_unity_signature_param_is_byref(MonoMethodSignature *sig, int index);
MONO_API MonoType* mono_unity_signature_get_type_for_param(MonoMethodSignature *sig, int index);


//generic inst
MONO_API uint32_t mono_unity_generic_inst_get_type_argc(MonoGenericInst *inst);
MONO_API MonoType* mono_unity_generic_inst_get_type_argument(MonoGenericInst *inst, int index);

//exception
MONO_API MonoString* mono_unity_exception_get_message(MonoException *exc);
MONO_API MonoString* mono_unity_exception_get_stack_trace(MonoException *exc);
MONO_API MonoObject* mono_unity_exception_get_inner_exception(MonoException *exc);
MONO_API MonoArray* mono_unity_exception_get_trace_ips(MonoException *exc);
MONO_API void mono_unity_exception_set_trace_ips(MonoException *exc, MonoArray *ips);
MONO_API MonoException* mono_unity_exception_get_marshal_directive(const char* msg);
MONO_API MonoException* mono_unity_error_convert_to_exception(MonoError *error);

//defaults
MONO_API MonoClass* mono_unity_defaults_get_int_class(void);
MONO_API MonoClass* mono_unity_defaults_get_stack_frame_class(void);
MONO_API MonoClass* mono_unity_defaults_get_int32_class(void);
MONO_API MonoClass* mono_unity_defaults_get_char_class(void);
MONO_API MonoClass* mono_unity_defaults_get_delegate_class(void);
MONO_API MonoClass* mono_unity_defaults_get_byte_class(void);

//unitytls
typedef struct unitytls_interface_struct unitytls_interface_struct;
MONO_API unitytls_interface_struct* mono_unity_get_unitytls_interface(void);
MONO_API void mono_unity_install_unitytls_interface(unitytls_interface_struct* callbacks);

// gc
typedef enum
{
	MONO_GC_MODE_DISABLED = 0,
	MONO_GC_MODE_ENABLED = 1,
	MONO_GC_MODE_MANUAL = 2
}  MonoGCMode;

MONO_API void mono_unity_gc_set_mode(MonoGCMode mode);

// Deprecated. Remove when Unity has switched to mono_unity_gc_set_mode
MONO_API void mono_unity_gc_enable(void);
// Deprecated. Remove when Unity has switched to mono_unity_gc_set_mode
MONO_API void mono_unity_gc_disable(void);
// Deprecated. Remove when Unity has switched to mono_unity_gc_set_mode
MONO_API int mono_unity_gc_is_disabled(void);
MONO_API void mono_unity_gc_handles_foreach_get_target(MonoFunc callback, void* user_data);

// logging
typedef void (*UnityLogErrorCallback) (const char *message);
MONO_API void mono_unity_set_editor_logging_callback(UnityLogErrorCallback callback);
MONO_API mono_bool mono_unity_log_error_to_editor(const char *message);

//misc
MONO_API MonoAssembly* mono_unity_assembly_get_mscorlib(void);
MONO_API MonoImage* mono_unity_image_get_mscorlib(void);
MONO_API MonoClass* mono_unity_generic_container_get_parameter_class(MonoGenericContainer* generic_container, int32_t index);
MONO_API MonoString* mono_unity_string_append_assembly_name_if_necessary(MonoString* typeName, const char* assemblyName);
MONO_API void mono_unity_memory_barrier(void);
MONO_API MonoException* mono_unity_thread_check_exception(void);
MONO_API MonoObject* mono_unity_delegate_get_target(MonoDelegate *delegate);
MONO_API char* mono_unity_get_runtime_build_info(const char *date, const char *time);
MONO_API void* mono_unity_get_field_address(MonoObject *obj, MonoVTable *vt, MonoClassField *field);
MONO_API MonoClassField* mono_unity_field_from_token_checked(MonoImage *image, guint32 token, MonoClass **retklass, MonoGenericContext *context, MonoError *error);
MONO_API mono_bool mono_unity_thread_state_init_from_handle(MonoThreadUnwindState *tctx, MonoThreadInfo *info, void* fixme);
MONO_API void mono_unity_stackframe_set_method(MonoStackFrame *sf, MonoMethod *method);
MONO_API MonoType* mono_unity_reflection_type_get_type(MonoReflectionType *type);
MONO_API void mono_unity_set_data_dir(const char* dir);
MONO_API char* mono_unity_get_data_dir(void);
MONO_API MonoClass* mono_unity_class_get(MonoImage* image, uint32_t type_token);
MONO_API void* mono_unity_alloc(size_t size);
MONO_API void mono_unity_g_free (void *ptr);
MONO_API MonoImage* mono_unity_assembly_get_image(MonoAssembly* assembly);
MONO_API MonoClass* mono_unity_field_get_class(MonoClassField* field);

MONO_API MonoClass* mono_custom_attrs_get_attrs (MonoCustomAttrInfo *ainfo, void* *iter);
MONO_API MonoArray* mono_unity_custom_attrs_construct (MonoCustomAttrInfo *cinfo, MonoError *error);

typedef size_t (*RemapPathFunction)(const char* path, char* buffer, size_t buffer_len);
MONO_API void mono_unity_register_path_remapper (RemapPathFunction func);

MONO_API const char*
mono_unity_remap_path (const char* path);

MONO_API const mono_unichar2*
mono_unity_remap_path_utf16 (const mono_unichar2* path);

MONO_API MonoBoolean
ves_icall_System_IO_MonoIO_RemapPath  (MonoString *path, MonoString **new_path);

MONO_API MonoMethod*
mono_method_get_method_definition(MonoMethod *method);

MONO_API void
mono_class_set_allow_gc_aware_layout(mono_bool allow);

MONO_API MONO_API void
mono_unity_set_enable_handler_block_guards (mono_bool allow);

MONO_API mono_bool
mono_unity_get_enable_handler_block_guards (void);

MONO_API mono_bool mono_unity_class_is_open_constructed_type (MonoClass *klass);

MONO_API gboolean mono_unity_class_has_failure (const MonoClass* klass);

#ifdef ANDROID
typedef uint8_t (*android_network_up_state)(const char* ifName, uint8_t* is_up);

MONO_API void
mono_unity_set_android_network_up_state_func(android_network_up_state func);
#endif

MonoBoolean
ves_icall_Unity_Android_Network_Interface_Up_State (MonoString *ifName, MonoBoolean* is_up);
MONO_API
mono_unichar2*
mono_unity_string_chars (MonoString *s);

MONO_API
uint32_t mono_unity_get_all_classes_size (MonoImage *image);

MONO_API
MonoClass* mono_unity_get_class_by_index(MonoImage *image, uint32_t index);

MONO_API uint32_t
mono_unity_object_header_size(void);

MONO_API uint32_t
mono_unity_array_object_header_size(void);

MONO_API uint32_t
mono_unity_offset_of_array_length_in_array_object_header(void);

MONO_API uint32_t
mono_unity_offset_of_array_bounds_in_array_object_header(void);

MONO_API uint32_t
mono_unity_allocation_granularity(void);

MONO_API
void mono_monitor_pulse_external(MonoObject *obj, const char *func, mono_bool all);

MONO_API
MonoBoolean
mono_monitor_wait_external (MonoObject* obj, uint32_t ms);

MONO_API void
mono_unity_type_get_name_full_chunked(MonoType *type, MonoFunc chunkReportFunc, void* userData);

MONO_API void
mono_class_set_userdata(MonoClass* klass, void* userdata);

MONO_API int
mono_class_get_userdata_offset(void);

#endif
