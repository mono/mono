#ifndef __UNITY_MONO_UTILS_H
#define __UNITY_MONO_UTILS_H

#include <glib.h>
#include <stdio.h>
#include <mono/metadata/object.h>
#include <mono/metadata/marshal.h>

typedef void(*vprintf_func)(const char* msg, va_list args);
typedef struct {
	void* (*malloc_func)(size_t size);
	void(*free_func)(void *ptr);
	void* (*calloc_func)(size_t nmemb, size_t size);
	void* (*realloc_func)(void *ptr, size_t size);
} MonoMemoryCallbacks;

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

gboolean
unity_mono_method_is_inflated (MonoMethod* method);

MONO_API gboolean
unity_mono_method_is_generic (MonoMethod* method);

typedef const char*(*UnityFindPluginCallback)(const char*);

MONO_API void
mono_set_find_plugin_callback(UnityFindPluginCallback find);

MONO_API UnityFindPluginCallback
mono_get_find_plugin_callback();

MonoAssembly* mono_unity_mscorlib();
MonoImage* mono_unity_mscorlib_image();
const char* mono_unity_image_name_for(MonoClass* klass);
void* mono_unity_get_field_address(MonoObject *obj, MonoVTable *vt, MonoClassField *field);
MonoObject* mono_unity_compare_exchange(MonoObject **location, MonoObject *value, MonoObject *comparand);
MonoObject* mono_unity_exchange(MonoObject **location, MonoObject *value);
void mono_unity_init_obj(void* obj, MonoClass* klass);
MonoObject* mono_unity_isinst_sealed(MonoObject* obj, MonoClass* targetType);
MonoClass* mono_unity_class_get_generic_definition(MonoClass* klass);
MonoMethod* mono_unity_method_get_generic_definition(MonoMethod* method);
MonoClass* mono_unity_get_class_for_generic_parameter(MonoGenericContainer* generic_container, gint index);
MonoClass* mono_unity_class_inflate_generic_class(MonoClass *gklass, MonoGenericContext *context);
MonoVTable* mono_unity_class_get_vtable(MonoClass* klass);
gboolean mono_unity_class_has_parent_unsafe(MonoClass *klass, MonoClass *parent);
guint64 mono_unity_get_method_hash(MonoMethod *method);
void mono_unity_install_finalize_runtime_invoke(MonoDomain* domain, RuntimeInvokeFunction callback);
void mono_unity_install_capture_context_runtime_invoke(MonoDomain* domain, RuntimeInvokeFunction callback);
void mono_unity_install_capture_context_method(MonoDomain* domain, gpointer callback);
MonoString* mono_unity_append_assembly_name_if_necessary(MonoString* typeName, const char* assemblyName);
void mono_unity_memory_barrier();
void mono_unity_object_unbox_nullable(MonoObject* obj, MonoClass* nullableArgumentClass, void* storage);
MonoReflectionMethod* mono_unity_method_get_object(MonoMethod *method);
MonoAssembly* mono_unity_assembly_from_class(MonoClass *klass);
MonoMethod* mono_unity_aot_get_array_helper_from_wrapper(MonoMethod *method);
gboolean mono_unity_class_is_array(MonoClass *klass);
int mono_unity_get_array_element_size(MonoArray *arr);
MonoException* mono_unity_thread_check_exception();
MonoClass* mono_unity_class_from_array(MonoArray *arr);
MonoClass* mono_unity_element_class_from_class(MonoClass *klass);
MonoClass* mono_unity_class_from_object(MonoObject *obj);
gboolean mono_unity_type_is_generic_instance(MonoType *type);
MonoGenericClass* mono_unity_type_get_generic_class(MonoType *type);
MonoGenericContext mono_unity_generic_class_get_context(MonoGenericClass *klass);
MonoClass* mono_unity_generic_class_get_container_class(MonoGenericClass *klass);
gboolean mono_unity_check_box_cast(MonoObject *obj, MonoClass *klass);
mono_array_size_t mono_unity_get_array_max_length(MonoArray *arr);
gboolean mono_unity_class_is_delegate(MonoClass *klass);
MonoObject* mono_unity_delegate_get_target(MonoDelegate *delegate);
MonoObject* mono_unity_convert_return_type_if_needed(MonoMethod *method, void *value);
MonoClass* mono_unity_class_for_method_param(MonoMethodSignature *sig, int index);
int mono_unity_num_method_parameters(MonoMethodSignature *sig);
int mono_unity_class_instance_size(MonoClass *klass);
gboolean mono_unity_method_param_is_byref(MonoMethodSignature *sig, int index);
MonoClass* mono_unity_int_class_get();
MonoClass* mono_unity_stack_frame_class_get();
MonoClass* mono_unity_class_get_castclass(MonoClass *klass);
gchar* mono_unity_get_runtime_build_info(const char *date, const char *time);
gboolean mono_unity_type_is_enum_type(MonoType *type);
MonoClass* mono_unity_int32_class_get();
MonoClass* mono_unity_char_class_get();
MonoClass* mono_unity_delegate_class_get();
MonoBoolean mono_unity_is_class(MonoClass* klass);
guint32 mono_unity_native_size(MonoClass* klass);
MonoBoolean mono_unity_class_is_string(MonoClass* klass);
MonoException* mono_unity_get_exception_marshal_directive(const char* msg);
MonoMethod* mono_unity_method_alloc0(MonoClass* declaring_type);

//only safe to call when the type has a defined klass data element
MonoClass* mono_unity_element_class_from_type(MonoType *type);

gboolean mono_unity_type_is_boolean(MonoType *type);
MonoClass* mono_unity_byte_class_get();

#endif
