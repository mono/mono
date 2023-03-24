#ifndef __UNITY_MONO_UTILS_H
#define __UNITY_MONO_UTILS_H

#include <glib.h>
#include <stdio.h>
#include <mono/metadata/object.h>
#include <mono/metadata/marshal.h>

typedef int (*vprintf_func)(const char* msg, va_list args);
typedef struct {
	void* (*malloc_func)(size_t size);
	void(*free_func)(void *ptr);
	void* (*calloc_func)(size_t nmemb, size_t size);
	void* (*realloc_func)(void *ptr, size_t size);
} MonoMemoryCallbacks;

// This structure must remain in sync with the one in Burst (MonoBreakpointHandling.cs)
typedef struct
{
	// For Burst 
	void (*BurstFinishSetup)();

	// For MonoDebugInternal From Burst
	int (*BurstIsFileBursted)(int ignoreCase, const char* baseName);
	void (*BurstBreakpointNotify)(gboolean set, void* method, long ilOffset, void* bp);
	void (*BurstFetchMethods)(void* buf, void* domain, guint32 id, gboolean protocol_version_set, int major_version, int minor_version);
	void (*BurstFetchFiles)(void* buf, guint32 fullPath, gboolean protocol_version_set, int major_version, int minor_version);
	void (*BurstFetchDebugInfo)(void* buf, void* method, gboolean protocol_version_set, int major_version, int minor_version);
	void (*BurstFetchMethodInfo)(void* buf, void* method, gboolean protocol_version_set, int major_version, int minor_version);
	void (*BurstFetchParamInfo)(void* buf, void* method, gboolean protocol_version_set, int major_version, int minor_version);
	void (*BurstFetchMethodName)(void* buf, void* method, gboolean protocol_version_set, int major_version, int minor_version);

	// For Burst From MonoDebugInternal
	void (*buffer_add_byte)(void* buf, guint8 iVal);
	void (*buffer_add_int)(void* buf, guint32 iVal);
	void (*buffer_add_id)(void* buf, int iVal);
	void (*buffer_add_string)(void* buf, const char* str);
	void (*buffer_add_ptr_id)(void* buf, void* domain, int id, gpointer ptr);
	void (*mono_burst_shutdown)();

	// For Burst From Unity (set via mono to avoid requiring another copy of the definition of this structure in unity)
	void (*burst_unity_domain_init)(const char* asmPath, const char* typeNamespace,const char* typeName);

} BurstMonoDebuggerCallbacks;

/**
 *	Custom exit function, called instead of system exit()
 */
void unity_mono_exit( int code );

/**
 *	Closes redirected output files.
 */
void unity_mono_close_output(void);

extern MonoString* mono_unity_get_embeddinghostname(void);

#ifdef WIN32
FILE* unity_fopen( const char *name, const char *mode );
#endif

extern gboolean mono_unity_socket_security_enabled_get (void);
MONO_API extern void mono_unity_socket_security_enabled_set (gboolean enabled);
MONO_API void mono_unity_set_vprintf_func(vprintf_func func);


void unity_mono_install_memory_callbacks(MonoMemoryCallbacks* callbacks);

MONO_API extern void burst_mono_simulate_burst_debug_domain_reload();
MONO_API extern void burst_mono_install_hooks(BurstMonoDebuggerCallbacks* callbacks, void* extra);
MONO_API extern void burst_mono_update_tracking_pointers(MonoDomain* domain,MonoClass* klass);

MONO_API gboolean
unity_mono_method_is_generic (MonoMethod* method);

typedef const char*(*UnityFindPluginCallback)(const char*);

MONO_API void
mono_set_find_plugin_callback(UnityFindPluginCallback find);

MONO_API UnityFindPluginCallback
mono_get_find_plugin_callback();

//object
void mono_unity_object_init(void* obj, MonoClass* klass);
MonoObject* mono_unity_object_isinst_sealed(MonoObject* obj, MonoClass* targetType);
void mono_unity_object_unbox_nullable(MonoObject* obj, MonoClass* nullableArgumentClass, void* storage);
MonoClass* mono_unity_object_get_class(MonoObject *obj);
MonoObject* mono_unity_object_compare_exchange(MonoObject **location, MonoObject *value, MonoObject *comparand);
MonoObject* mono_unity_object_exchange(MonoObject **location, MonoObject *value);
gboolean mono_unity_object_check_box_cast(MonoObject *obj, MonoClass *klass);

//class 
const char* mono_unity_class_get_image_name(MonoClass* klass);
MonoClass* mono_unity_class_get_generic_definition(MonoClass* klass);
MonoClass* mono_unity_class_inflate_generic_class(MonoClass *gklass, MonoGenericContext *context);
gboolean mono_unity_class_has_parent_unsafe(MonoClass *klass, MonoClass *parent);
MonoAssembly* mono_unity_class_get_assembly(MonoClass *klass);
gboolean mono_unity_class_is_array(MonoClass *klass);
MonoClass* mono_unity_class_get_element_class(MonoClass *klass);
gboolean mono_unity_class_is_delegate(MonoClass *klass);
int mono_unity_class_get_instance_size(MonoClass *klass);
MonoClass* mono_unity_class_get_castclass(MonoClass *klass);
guint32 mono_unity_class_get_native_size(MonoClass* klass);
MonoBoolean mono_unity_class_is_string(MonoClass* klass);
MonoBoolean mono_unity_class_is_class_type(MonoClass* klass);
MONO_API gboolean mono_unity_class_is_inited(MonoClass* klass);
MONO_API gboolean mono_class_is_generic(MonoClass *klass);
MONO_API gboolean mono_class_is_blittable(MonoClass *klass);
MONO_API gboolean mono_class_is_inflated(MonoClass *klass);
gboolean mono_unity_class_has_cctor(MonoClass *klass);

//method 
MonoMethod* mono_unity_method_get_generic_definition(MonoMethod* method);
MonoReflectionMethod* mono_unity_method_get_object(MonoMethod *method);
MonoMethod* mono_unity_method_alloc0(MonoClass* klass);
MonoMethod* mono_unity_method_delegate_invoke_wrapper(MonoClass* klass);
gboolean mono_unity_method_is_static(MonoMethod *method);
MonoClass* mono_unity_method_get_class(const MonoMethod *method);

#ifdef IL2CPP_ON_MONO
void* mono_unity_method_get_method_pointer(MonoMethod* method);
void mono_unity_method_set_method_pointer(MonoMethod* method, void *p);
void* mono_unity_method_get_invoke_pointer(MonoMethod* method);
void mono_unity_method_set_invoke_pointer(MonoMethod* method, void *p);
#endif

const char* mono_unity_method_get_name(const MonoMethod *method);
guint64 mono_unity_method_get_hash(MonoMethod *method, gboolean inflate);
MonoMethod* mono_unity_method_get_aot_array_helper_from_wrapper(MonoMethod *method);
MonoObject* mono_unity_method_convert_return_type_if_needed(MonoMethod *method, void *value);
MONO_API gboolean unity_mono_method_is_inflated(MonoMethod* method);
MONO_API guint32 mono_unity_method_get_token(MonoMethod *method);

//domain
void mono_unity_domain_install_finalize_runtime_invoke(MonoDomain* domain, RuntimeInvokeFunction callback);
void mono_unity_domain_install_capture_context_runtime_invoke(MonoDomain* domain, RuntimeInvokeFunction callback);
void mono_unity_domain_install_capture_context_method(MonoDomain* domain, void* callback);
MONO_API void mono_unity_domain_unload (MonoDomain *domain, MonoUnityExceptionFunc callback);

//array
int mono_unity_array_get_element_size(MonoArray *arr);
MonoClass* mono_unity_array_get_class(MonoArray *arr);
mono_array_size_t mono_unity_array_get_max_length(MonoArray *arr);

//type
gboolean mono_unity_type_is_generic_instance(MonoType *type);
MonoGenericClass* mono_unity_type_get_generic_class(MonoType *type);
gboolean mono_unity_type_is_enum_type(MonoType *type);
gboolean mono_unity_type_is_boolean(MonoType *type);
MonoClass* mono_unity_type_get_element_class(MonoType *type); //only safe to call when the type has a defined klass data element
guint64 mono_unity_type_get_hash(MonoType *type, gboolean inflate);

//generic class
MonoGenericContext mono_unity_generic_class_get_context(MonoGenericClass *klass);
MonoClass* mono_unity_generic_class_get_container_class(MonoGenericClass *klass);

//method signature
MonoClass* mono_unity_signature_get_class_for_param(MonoMethodSignature *sig, int index);
int mono_unity_signature_num_parameters(MonoMethodSignature *sig);
gboolean mono_unity_signature_param_is_byref(MonoMethodSignature *sig, int index);

//generic inst
guint mono_unity_generic_inst_get_type_argc(MonoGenericInst *inst);
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
MonoClass* mono_unity_defaults_get_int_class();
MonoClass* mono_unity_defaults_get_stack_frame_class();
MonoClass* mono_unity_defaults_get_int32_class();
MonoClass* mono_unity_defaults_get_char_class();
MonoClass* mono_unity_defaults_get_delegate_class();
MonoClass* mono_unity_defaults_get_byte_class();

//unitytls
typedef struct unitytls_interface_struct unitytls_interface_struct;
MONO_API unitytls_interface_struct* mono_unity_get_unitytls_interface();
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
MONO_API void mono_unity_gc_enable();
// Deprecated. Remove when Unity has switched to mono_unity_gc_set_mode
MONO_API void mono_unity_gc_disable();
// Deprecated. Remove when Unity has switched to mono_unity_gc_set_mode
MONO_API int mono_unity_gc_is_disabled();

// logging
typedef void (*UnityLogErrorCallback) (const char *message);
MONO_API void mono_unity_set_editor_logging_callback(UnityLogErrorCallback callback);
gboolean mono_unity_log_error_to_editor(const char *message);

//misc
MonoAssembly* mono_unity_assembly_get_mscorlib();
MonoImage* mono_unity_image_get_mscorlib();
MonoClass* mono_unity_generic_container_get_parameter_class(MonoGenericContainer* generic_container, gint index);
MonoString* mono_unity_string_append_assembly_name_if_necessary(MonoString* typeName, const char* assemblyName);
void mono_unity_memory_barrier();
MonoException* mono_unity_thread_check_exception();
MonoObject* mono_unity_delegate_get_target(MonoDelegate *delegate);
gchar* mono_unity_get_runtime_build_info(const char *date, const char *time);
void* mono_unity_get_field_address(MonoObject *obj, MonoVTable *vt, MonoClassField *field);
MONO_API MonoClassField* mono_unity_field_from_token_checked(MonoImage *image, guint32 token, MonoClass **retklass, MonoGenericContext *context, MonoError *error);
gboolean mono_unity_thread_state_init_from_handle(MonoThreadUnwindState *tctx, MonoThreadInfo *info, void* fixme);
void mono_unity_stackframe_set_method(MonoStackFrame *sf, MonoMethod *method);
MonoType* mono_unity_reflection_type_get_type(MonoReflectionType *type);
MONO_API void mono_unity_set_data_dir(const char* dir);
MONO_API char* mono_unity_get_data_dir();
MONO_API MonoClass* mono_unity_class_get(MonoImage* image, guint32 type_token);
MONO_API gpointer mono_unity_alloc(gsize size);
MONO_API void mono_unity_g_free (void *ptr);

MONO_API MonoClass* mono_custom_attrs_get_attrs (MonoCustomAttrInfo *ainfo, gpointer *iter);
MONO_API MonoArray* mono_unity_custom_attrs_construct (MonoCustomAttrInfo *cinfo, MonoError *error);

typedef size_t (*RemapPathFunction)(const char* path, char* buffer, size_t buffer_len);
MONO_API void mono_unity_register_path_remapper (RemapPathFunction func);

const char*
mono_unity_remap_path (const char* path);

const gunichar2*
mono_unity_remap_path_utf16 (const gunichar2* path);

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

MONO_API gboolean mono_unity_class_is_open_constructed_type (MonoClass *klass);

MONO_API gboolean mono_unity_class_has_failure (const MonoClass* klass);

#ifdef ANDROID
typedef uint8_t (*android_network_up_state)(const char* ifName, uint8_t* is_up);

MONO_API void
mono_unity_set_android_network_up_state_func(android_network_up_state func);

MonoBoolean
ves_icall_Unity_Android_Network_Interface_Up_State (MonoString *ifName, MonoBoolean* is_up);
#endif

#endif
