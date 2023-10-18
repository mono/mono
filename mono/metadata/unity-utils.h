#ifndef __UNITY_MONO_UTILS_H
#define __UNITY_MONO_UTILS_H

#include <stdio.h>
#include <mono/metadata/object.h>
#include <mono/metadata/reflection.h>

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
void mono_unity_object_init(void* obj, MonoClass* klass);
MonoObject* mono_unity_object_isinst_sealed(MonoObject* obj, MonoClass* targetType);
void mono_unity_object_unbox_nullable(MonoObject* obj, MonoClass* nullableArgumentClass, void* storage);
MonoClass* mono_unity_object_get_class(MonoObject *obj);
MonoObject* mono_unity_object_compare_exchange(MonoObject **location, MonoObject *value, MonoObject *comparand);
MonoObject* mono_unity_object_exchange(MonoObject **location, MonoObject *value);
mono_bool mono_unity_object_check_box_cast(MonoObject *obj, MonoClass *klass);

//class 
const char* mono_unity_class_get_image_name(MonoClass* klass);
MonoClass* mono_unity_class_get_generic_definition(MonoClass* klass);
MonoClass* mono_unity_class_inflate_generic_class(MonoClass *gklass, MonoGenericContext *context);
mono_bool mono_unity_class_has_parent_unsafe(MonoClass *klass, MonoClass *parent);
MonoAssembly* mono_unity_class_get_assembly(MonoClass *klass);
mono_bool mono_unity_class_is_array(MonoClass *klass);
MonoClass* mono_unity_class_get_element_class(MonoClass *klass);
mono_bool mono_unity_class_is_delegate(MonoClass *klass);
int mono_unity_class_get_instance_size(MonoClass *klass);
MonoClass* mono_unity_class_get_castclass(MonoClass *klass);
uint32_t mono_unity_class_get_native_size(MonoClass* klass);
MonoBoolean mono_unity_class_is_string(MonoClass* klass);
MonoBoolean mono_unity_class_is_class_type(MonoClass* klass);
MONO_API mono_bool mono_class_is_generic(MonoClass *klass);
MONO_API mono_bool mono_class_is_blittable(MonoClass *klass);
MONO_API mono_bool mono_class_is_inflated(MonoClass *klass);
mono_bool mono_unity_class_has_cctor(MonoClass *klass);
mono_bool mono_unity_class_has_reference(MonoClass* kclass);
MONO_API mono_bool mono_unity_class_is_abstract (MonoClass* klass);
MONO_API void* mono_unity_vtable_get_static_field_data(MonoVTable *vTable);
MONO_API MonoVTable* mono_unity_class_try_get_vtable(MonoDomain *domain, MonoClass *klass);
MONO_API mono_bool mono_unity_class_field_is_literal(MonoClassField *field);

//method 
MonoMethod* mono_unity_method_get_generic_definition(MonoMethod* method);
MonoReflectionMethod* mono_unity_method_get_object(MonoMethod *method);
MonoMethod* mono_unity_method_alloc0(MonoClass* klass);
MonoMethod* mono_unity_method_delegate_invoke_wrapper(MonoClass* klass);
mono_bool mono_unity_method_is_static(MonoMethod *method);
MonoClass* mono_unity_method_get_class(const MonoMethod *method);
MONO_API MonoMethod* mono_unity_reflection_method_get_method(MonoReflectionMethod* mrf);

#ifdef IL2CPP_ON_MONO
void* mono_unity_method_get_method_pointer(MonoMethod* method);
void mono_unity_method_set_method_pointer(MonoMethod* method, void *p);
void* mono_unity_method_get_invoke_pointer(MonoMethod* method);
void mono_unity_method_set_invoke_pointer(MonoMethod* method, void *p);
#endif

const char* mono_unity_method_get_name(const MonoMethod *method);
uint64_t mono_unity_method_get_hash(MonoMethod *method, mono_bool inflate);
MonoMethod* mono_unity_method_get_aot_array_helper_from_wrapper(MonoMethod *method);
MonoObject* mono_unity_method_convert_return_type_if_needed(MonoMethod *method, void *value);
MONO_API mono_bool mono_unity_method_is_inflated(MonoMethod* method);
MONO_API uint32_t mono_unity_method_get_token(MonoMethod *method);
MONO_API const char* mono_unity_method_get_param_name(MonoMethod *method, uint32_t index);

//domain
void mono_unity_domain_install_finalize_runtime_invoke(MonoDomain* domain, UnityRuntimeInvokeFunction callback);
void mono_unity_domain_install_capture_context_runtime_invoke(MonoDomain* domain, UnityRuntimeInvokeFunction callback);
void mono_unity_domain_install_capture_context_method(MonoDomain* domain, void* callback);
MONO_API void mono_unity_domain_unload (MonoDomain *domain, MonoUnityExceptionFunc callback);

//array
int mono_unity_array_get_element_size(MonoClass *arr_class);
MonoClass* mono_unity_array_get_class(MonoArray *arr);
uint32_t mono_unity_array_get_max_length(MonoArray *arr);
int mono_unity_array_get_byte_length (MonoArray* array);
MonoArray* mono_unity_array_new_specific(MonoClass* arrayClass, uintptr_t size);

//type
mono_bool mono_unity_type_is_generic_instance(MonoType *type);
MonoGenericClass* mono_unity_type_get_generic_class(MonoType *type);
mono_bool mono_unity_type_is_enum_type(MonoType *type);
mono_bool mono_unity_type_is_boolean(MonoType *type);
MonoClass* mono_unity_type_get_element_class(MonoType *type); //only safe to call when the type has a defined klass data element
uint64_t mono_unity_type_get_hash(MonoType *type, mono_bool inflate);
MONO_API mono_bool mono_unity_type_is_static(MonoType *type);

//generic class
MonoGenericContext mono_unity_generic_class_get_context(MonoGenericClass *klass);
MonoClass* mono_unity_generic_class_get_container_class(MonoGenericClass *klass);

//method signature
MonoClass* mono_unity_signature_get_class_for_param(MonoMethodSignature *sig, int index);
int mono_unity_signature_num_parameters(MonoMethodSignature *sig);
mono_bool mono_unity_signature_param_is_byref(MonoMethodSignature *sig, int index);
MonoType* mono_unity_signature_get_type_for_param(MonoMethodSignature *sig, int index);


//generic inst
uint32_t mono_unity_generic_inst_get_type_argc(MonoGenericInst *inst);
MonoType* mono_unity_generic_inst_get_type_argument(MonoGenericInst *inst, int index);

//exception
MonoString* mono_unity_exception_get_message(MonoException *exc);
MonoString* mono_unity_exception_get_stack_trace(MonoException *exc);
MonoObject* mono_unity_exception_get_inner_exception(MonoException *exc);
MonoArray* mono_unity_exception_get_trace_ips(MonoException *exc);
void mono_unity_exception_set_trace_ips(MonoException *exc, MonoArray *ips);
MonoException* mono_unity_exception_get_marshal_directive(const char* msg);
MONO_API MonoException* mono_unity_error_convert_to_exception(MonoError *error);

//defaults
MonoClass* mono_unity_defaults_get_int_class(void);
MonoClass* mono_unity_defaults_get_stack_frame_class(void);
MonoClass* mono_unity_defaults_get_int32_class(void);
MonoClass* mono_unity_defaults_get_char_class(void);
MonoClass* mono_unity_defaults_get_delegate_class(void);
MonoClass* mono_unity_defaults_get_byte_class(void);

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
mono_bool mono_unity_log_error_to_editor(const char *message);

//misc
MonoAssembly* mono_unity_assembly_get_mscorlib(void);
MonoImage* mono_unity_image_get_mscorlib(void);
MonoClass* mono_unity_generic_container_get_parameter_class(MonoGenericContainer* generic_container, int32_t index);
MonoString* mono_unity_string_append_assembly_name_if_necessary(MonoString* typeName, const char* assemblyName);
void mono_unity_memory_barrier(void);
MonoException* mono_unity_thread_check_exception(void);
MonoObject* mono_unity_delegate_get_target(MonoDelegate *delegate);
char* mono_unity_get_runtime_build_info(const char *date, const char *time);
void* mono_unity_get_field_address(MonoObject *obj, MonoVTable *vt, MonoClassField *field);
MONO_API MonoClassField* mono_unity_field_from_token_checked(MonoImage *image, guint32 token, MonoClass **retklass, MonoGenericContext *context, MonoError *error);
mono_bool mono_unity_thread_state_init_from_handle(MonoThreadUnwindState *tctx, MonoThreadInfo *info, void* fixme);
void mono_unity_stackframe_set_method(MonoStackFrame *sf, MonoMethod *method);
MonoType* mono_unity_reflection_type_get_type(MonoReflectionType *type);
MONO_API void mono_unity_set_data_dir(const char* dir);
MONO_API char* mono_unity_get_data_dir(void);
MONO_API MonoClass* mono_unity_class_get(MonoImage* image, uint32_t type_token);
MONO_API void* mono_unity_alloc(size_t size);
MONO_API void mono_unity_g_free (void *ptr);
MonoImage* mono_unity_assembly_get_image(MonoAssembly* assembly);
MonoClass* mono_unity_field_get_class(MonoClassField* field);

MONO_API MonoClass* mono_custom_attrs_get_attrs (MonoCustomAttrInfo *ainfo, void* *iter);
MONO_API MonoArray* mono_unity_custom_attrs_construct (MonoCustomAttrInfo *cinfo, MonoError *error);

typedef size_t (*RemapPathFunction)(const char* path, char* buffer, size_t buffer_len);
MONO_API void mono_unity_register_path_remapper (RemapPathFunction func);

const char*
mono_unity_remap_path (const char* path);

const mono_unichar2*
mono_unity_remap_path_utf16 (const mono_unichar2* path);

MonoBoolean
ves_icall_System_IO_MonoIO_RemapPath  (MonoString *path, MonoString **new_path);

MonoMethod*
mono_method_get_method_definition(MonoMethod *method);

void
mono_class_set_allow_gc_aware_layout(mono_bool allow);

MONO_API void
mono_unity_set_enable_handler_block_guards (mono_bool allow);

mono_bool
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
