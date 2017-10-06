#if defined(RUNTIME_IL2CPP)

#include "il2cpp-config.h"
#include <il2cpp-class-internals.h>
#include "gc/GCHandle.h"
#include "gc/GarbageCollector.h"
#include "vm/Assembly.h"
#include "vm/AssemblyName.h"
#include "vm/Class.h"
#include "vm/Domain.h"
#include "vm/Field.h"
#include "vm/Image.h"
#include "vm/Method.h"
#include "vm/Object.h"
#include "vm/Profiler.h"
#include "vm/Runtime.h"
#include "vm/String.h"
#include "vm/Thread.h"
#include "vm/ThreadPoolMs.h"
#include "vm/Type.h"
#include "vm-utils/Debugger.h"

extern "C" {

#include <glib.h>
#include <mono/utils/mono-coop-mutex.h>
#include <mono/metadata/handle.h>
#include <mono/metadata/object-internals.h>
#include <mono/metadata/appdomain.h>
#include <mono/metadata/mono-debug.h>
#include <mono/metadata/debug-mono-symfile.h>
#include <mono/metadata/profiler.h>
#include <mono/sgen/sgen-conf.h>
#include <mono/mini/mini.h>
#include <mono/metadata/seq-points-data.h>
#include "il2cpp-c-types.h"
#include <mono/metadata/il2cpp-compat-metadata.h>

static MonoGHashTable *method_signatures;

Il2CppMonoMethod* il2cpp_mono_image_get_entry_point (Il2CppMonoImage *image)
{
	return (Il2CppMonoMethod*)il2cpp::vm::Image::GetEntryPoint((Il2CppImage*)image);
}

const char* il2cpp_mono_image_get_filename (Il2CppMonoImage *image)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

const char*  il2cpp_mono_image_get_guid (Il2CppMonoImage *image)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

Il2CppMonoClass* il2cpp_mono_type_get_class (Il2CppMonoType *type)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

mono_bool il2cpp_mono_type_is_struct (Il2CppMonoType *type)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return 0;
}

mono_bool il2cpp_mono_type_is_reference (Il2CppMonoType *type)
{
	return il2cpp::vm::Type::IsReference((Il2CppType*)type);
}

void il2cpp_mono_metadata_free_mh (Il2CppMonoMethodHeader *mh)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
}

Il2CppMonoMethodSignature* il2cpp_mono_method_signature (Il2CppMonoMethod *m)
{
	MethodInfo* method = (MethodInfo*)m;

	if (method_signatures == NULL)
		method_signatures = mono_g_hash_table_new_type(NULL, NULL, MONO_HASH_KEY_GC, MONO_ROOT_SOURCE_DEBUGGER, "method-to-signature for il2cpp table");

	Il2CppMonoMethodSignature* existing_signature = (Il2CppMonoMethodSignature*)mono_g_hash_table_lookup(method_signatures, method);
	if (existing_signature != NULL)
		return existing_signature;

	Il2CppMonoMethodSignature* sig = g_new(Il2CppMonoMethodSignature, 1);

	sig->call_convention = MONO_CALL_DEFAULT;
	sig->hasthis = il2cpp::vm::Method::IsInstance(method);
	sig->ret = (Il2CppMonoType*)il2cpp::vm::Method::GetReturnType(method);
	sig->generic_param_count = il2cpp::vm::Method::GetGenericParamCount(method);
	sig->param_count = il2cpp::vm::Method::GetParamCount(method);
	sig->params = g_new(Il2CppMonoType*, sig->param_count);
	for (int i = 0; i < sig->param_count; ++i)
		sig->params[i] = (Il2CppMonoType*)il2cpp::vm::Method::GetParam(method, i);

	mono_g_hash_table_insert(method_signatures, method, sig);

	return sig;
}

static void il2cpp_mono_free_method_signature(gpointer unused1, gpointer value, gpointer unused2)
{
	Il2CppMonoMethodSignature* sig = (Il2CppMonoMethodSignature*)value;
	g_free(sig->params);
	g_free(sig);
}

void il2cpp_mono_free_method_signatures()
{
	if (method_signatures != NULL)
	{
		mono_g_hash_table_foreach(method_signatures, il2cpp_mono_free_method_signature, NULL);
		mono_g_hash_table_destroy(method_signatures);
	}
}

void il2cpp_mono_method_get_param_names (Il2CppMonoMethod *m, const char **names)
{
	MethodInfo* method = (MethodInfo*)m;
	uint32_t numberOfParameters = il2cpp::vm::Method::GetParamCount(method);
	for (int i = 0; i < numberOfParameters; ++i)
		names[i] = il2cpp::vm::Method::GetParamName(method, i);
}

mono_bool il2cpp_mono_type_generic_inst_is_valuetype (Il2CppMonoType *monoType)
{
	static const int kBitIsValueType = 1;
	Il2CppType *type = (Il2CppType*)monoType;
	const Il2CppTypeDefinition *typeDef = il2cpp::vm::MetadataCache::GetTypeDefinitionFromIndex(type->data.generic_class->typeDefinitionIndex);
	return (typeDef->bitfield >> (kBitIsValueType - 1)) & 0x1;
}

Il2CppMonoMethodHeader* il2cpp_mono_method_get_header_checked (Il2CppMonoMethod *method, MonoError *error)
{
	return NULL;
}

gboolean il2cpp_mono_class_init (Il2CppMonoClass *klass)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return 0;
}

Il2CppMonoVTable* il2cpp_mono_class_vtable (Il2CppMonoDomain *domain, Il2CppMonoClass *klass)
{
	return (Il2CppMonoVTable*)((Il2CppClass*)klass)->vtable;
}

Il2CppMonoClassField* il2cpp_mono_class_get_field_from_name (Il2CppMonoClass *klass, const char *name)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

int32_t il2cpp_mono_array_element_size (Il2CppMonoClass *ac)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return 0;
}

int32_t il2cpp_mono_class_instance_size (Il2CppMonoClass *klass)
{
	return il2cpp::vm::Class::GetInstanceSize((Il2CppClass*)klass);
}

int32_t il2cpp_mono_class_value_size (Il2CppMonoClass *klass, uint32_t *align)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return 0;
}

gboolean il2cpp_mono_class_is_assignable_from (Il2CppMonoClass *klass, Il2CppMonoClass *oklass)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return 0;
}

Il2CppMonoClass* il2cpp_mono_class_from_mono_type (Il2CppMonoType *type)
{
	return (Il2CppMonoClass*)il2cpp::vm::Class::FromIl2CppType((Il2CppType*)type);
}

int il2cpp_mono_class_num_fields (Il2CppMonoClass *klass)
{
	return il2cpp::vm::Class::GetNumFields((Il2CppClass*)klass);
}

int il2cpp_mono_class_num_methods (Il2CppMonoClass *klass)
{
	return il2cpp::vm::Class::GetNumMethods((Il2CppClass*)klass);
}

int il2cpp_mono_class_num_properties (Il2CppMonoClass *klass)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return 0;
}

Il2CppMonoClassField* il2cpp_mono_class_get_fields (Il2CppMonoClass* klass, gpointer *iter)
{
	return (Il2CppMonoClassField*)il2cpp::vm::Class::GetFields((Il2CppClass*)klass, iter);
}

Il2CppMonoMethod* il2cpp_mono_class_get_methods (Il2CppMonoClass* klass, gpointer *iter)
{
	return (Il2CppMonoMethod*)il2cpp::vm::Class::GetMethods((Il2CppClass*)klass, iter);
}

Il2CppMonoProperty* il2cpp_mono_class_get_properties (Il2CppMonoClass* klass, gpointer *iter)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

const char* il2cpp_mono_field_get_name (Il2CppMonoClassField *field)
{
	return il2cpp::vm::Field::GetName((FieldInfo*)field);
}

Il2CppMonoMethod* il2cpp_mono_class_get_method_from_name (Il2CppMonoClass *klass, const char *name, int param_count)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

mono_unichar2* il2cpp_mono_string_chars (Il2CppMonoString *monoStr)
{
	Il2CppString *str = (Il2CppString*)monoStr;
	return (mono_unichar2*)str->chars;
}

gboolean il2cpp_mono_class_is_abstract (Il2CppMonoClass *klass)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return 0;
}

int il2cpp_mono_string_length (Il2CppMonoString *monoStr)
{
	Il2CppString *str = (Il2CppString*)monoStr;
	return str->length;
}

char* il2cpp_mono_array_addr_with_size (Il2CppMonoArray *array, int size, uintptr_t idx)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

uintptr_t il2cpp_mono_array_length (Il2CppMonoArray *array)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return 0;
}

Il2CppMonoString* il2cpp_mono_string_new (Il2CppMonoDomain *domain, const char *text)
{
	return (Il2CppMonoString*)il2cpp::vm::String::New(text);
}

char* il2cpp_mono_string_to_utf8_checked (Il2CppMonoString *string_obj, MonoError *error)
{
	mono_error_init(error);
	Il2CppString *str = (Il2CppString*)string_obj;
	std::string s = il2cpp::utils::StringUtils::Utf16ToUtf8(str->chars, str->length);
	return g_strdup(s.c_str());
}

int il2cpp_mono_object_hash (Il2CppMonoObject* obj)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return 0;
}

void* il2cpp_mono_object_unbox (Il2CppMonoObject *monoObj)
{
	Il2CppObject *obj = (Il2CppObject*)monoObj;
	return il2cpp::vm::Object::Unbox(obj);
}

Il2CppMonoMethod* il2cpp_mono_object_get_virtual_method (Il2CppMonoObject *obj_raw, Il2CppMonoMethod *method)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

void il2cpp_mono_field_set_value (Il2CppMonoObject *obj, Il2CppMonoClassField *field, void *value)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
}

void il2cpp_mono_field_static_set_value (Il2CppMonoVTable *vt, Il2CppMonoClassField *field, void *value)
{
	il2cpp::vm::Field::StaticSetValue((FieldInfo*)field, value);
}

uint32_t il2cpp_mono_gchandle_new_weakref (Il2CppMonoObject *obj, mono_bool track_resurrection)
{
	return il2cpp::gc::GCHandle::NewWeakref((Il2CppObject*)obj, track_resurrection == 0 ? false : true);
}

Il2CppMonoObject*  il2cpp_mono_gchandle_get_target  (uint32_t gchandle)
{
	return (Il2CppMonoObject*)il2cpp::gc::GCHandle::GetTarget(gchandle);
}

void il2cpp_mono_gchandle_free (uint32_t gchandle)
{
	il2cpp::gc::GCHandle::Free(gchandle);
}

void il2cpp_mono_gc_wbarrier_generic_store (void* ptr, Il2CppMonoObject* value)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
}

int il2cpp_mono_reflection_parse_type (char *name, MonoTypeNameParse *info)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return 0;
}

void il2cpp_mono_reflection_free_type_info (MonoTypeNameParse *info)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
}

mono_bool il2cpp_mono_custom_attrs_has_attr (Il2CppMonoCustomAttrInfo *ainfo, Il2CppMonoClass *attr_klass)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return 0;
}

void il2cpp_mono_custom_attrs_free (Il2CppMonoCustomAttrInfo *ainfo)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
}

Il2CppMonoDomain* il2cpp_mono_get_root_domain (void)
{
	return (Il2CppMonoDomain*)il2cpp::vm::Domain::GetCurrent();
}

void il2cpp_mono_runtime_quit (void)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
}

gboolean il2cpp_mono_runtime_is_shutting_down (void)
{
	return il2cpp::vm::Runtime::IsShuttingDown() ? TRUE : FALSE;
}

Il2CppMonoDomain* il2cpp_mono_domain_get (void)
{
	return il2cpp_mono_get_root_domain();
}

gboolean il2cpp_mono_domain_set (Il2CppMonoDomain *domain, gboolean force)
{
	IL2CPP_ASSERT(domain == il2cpp_mono_get_root_domain());
	return TRUE;
}

void il2cpp_mono_domain_foreach(MonoDomainFunc func, gpointer user_data)
{
	func((MonoDomain*)il2cpp_mono_get_root_domain(), user_data);
}

MonoJitInfo* il2cpp_mono_jit_info_table_find(Il2CppMonoDomain* domain, char* addr)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

Il2CppMonoMethod* il2cpp_mono_jit_info_get_method(MonoJitInfo* ji)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

MonoDebugMethodInfo* il2cpp_mono_debug_lookup_method(Il2CppMonoMethod* method)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

MonoDebugMethodJitInfo* il2cpp_mono_debug_find_method(Il2CppMonoMethod* method, Il2CppMonoDomain* domain)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

void il2cpp_mono_debug_free_method_jit_info(MonoDebugMethodJitInfo* jit)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
}

MonoDebugLocalsInfo* il2cpp_mono_debug_lookup_locals(Il2CppMonoMethod* method)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

MonoDebugMethodAsyncInfo* il2cpp_mono_debug_lookup_method_async_debug_info(Il2CppMonoMethod* method)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

MonoDebugSourceLocation* il2cpp_mono_debug_method_lookup_location(MonoDebugMethodInfo* minfo, int il_offset)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

gint32 il2cpp_mono_debug_il_offset_from_address(Il2CppMonoMethod* method, Il2CppMonoDomain* domain, guint32 native_offset)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return 0;
}

void il2cpp_mono_debug_free_source_location(MonoDebugSourceLocation* location)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
}

void il2cpp_mono_set_is_debugger_attached(gboolean attached)
{
	il2cpp::utils::Debugger::SetIsDebuggerAttached(attached == TRUE);
}

char* il2cpp_mono_type_full_name(Il2CppMonoType* type)
{
	std::string name = il2cpp::vm::Type::GetName((Il2CppType*)type, IL2CPP_TYPE_NAME_FORMAT_FULL_NAME);
	return g_strdup(name.c_str());
}

char* il2cpp_mono_method_full_name(Il2CppMonoMethod* method, gboolean signature)
{
	return g_strdup(((MethodInfo*)method)->name);
}

void il2cpp_mono_debug_get_seq_points(MonoDebugMethodInfo* minfo, char** source_file, GPtrArray** source_file_list, int** source_files, MonoSymSeqPoint** seq_points, int* n_seq_points)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
}

void il2cpp_mono_debug_free_locals(MonoDebugLocalsInfo* info)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
}

void il2cpp_mono_debug_free_method_async_debug_info(MonoDebugMethodAsyncInfo* info)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
}

Il2CppMonoThread* il2cpp_mono_thread_current()
{
	return (Il2CppMonoThread*)il2cpp::vm::Thread::Current();
}

Il2CppMonoThread* il2cpp_mono_thread_get_main()
{
	return (Il2CppMonoThread*)il2cpp::vm::Thread::Main();
}

Il2CppMonoThread* il2cpp_mono_thread_attach(Il2CppMonoDomain* domain)
{
	return (Il2CppMonoThread*)il2cpp::vm::Thread::Attach((Il2CppDomain*)domain);
}

void il2cpp_mono_domain_lock(Il2CppMonoDomain* domain)
{
}

void il2cpp_mono_domain_unlock(Il2CppMonoDomain* domain)
{
}

MonoJitInfo* il2cpp_mono_jit_info_table_find_internal(Il2CppMonoDomain* domain, char* addr, gboolean try_aot, gboolean allow_trampolines)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

guint il2cpp_mono_aligned_addr_hash(gconstpointer ptr)
{
	return GPOINTER_TO_UINT(ptr) >> 3;
}

Il2CppMonoGenericInst* il2cpp_mono_metadata_get_generic_inst(int type_argc, Il2CppMonoType** type_argv)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

Il2CppMonoMethod* il2cpp_mono_get_method_checked(Il2CppMonoImage* image, guint32 token, Il2CppMonoClass* klass, Il2CppMonoGenericContext* context, MonoError* error)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

SgenDescriptor il2cpp_mono_gc_make_root_descr_all_refs(int numbits)
{
	return NULL;
}

int il2cpp_mono_class_interface_offset_with_variance(Il2CppMonoClass* klass, Il2CppMonoClass* itf, gboolean* non_exact_match)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return 0;
}

void il2cpp_mono_class_setup_supertypes(Il2CppMonoClass* klass)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
}

void il2cpp_mono_class_setup_vtable(Il2CppMonoClass* klass)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
}

void il2cpp_mono_class_setup_methods(Il2CppMonoClass* klass)
{
	il2cpp::vm::Class::SetupMethods((Il2CppClass*)klass);
}

gboolean il2cpp_mono_class_field_is_special_static(Il2CppMonoClassField* field)
{
	return il2cpp::vm::Field::IsNormalStatic((FieldInfo*)field) ? FALSE : TRUE;
}

guint32 il2cpp_mono_class_field_get_special_static_type(Il2CppMonoClassField* field)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return 0;
}

Il2CppMonoGenericContext* il2cpp_mono_class_get_context(Il2CppMonoClass* klass)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

Il2CppMonoGenericContext* il2cpp_mono_method_get_context(Il2CppMonoMethod* method)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

Il2CppMonoGenericContainer* il2cpp_mono_method_get_generic_container(Il2CppMonoMethod* method)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

Il2CppMonoMethod* il2cpp_mono_class_inflate_generic_method_full_checked(Il2CppMonoMethod* method, Il2CppMonoClass* klass_hint, Il2CppMonoGenericContext* context, MonoError* error)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

Il2CppMonoMethod* il2cpp_mono_class_inflate_generic_method_checked(Il2CppMonoMethod* method, Il2CppMonoGenericContext* context, MonoError* error)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

void il2cpp_mono_loader_lock()
{
}

void il2cpp_mono_loader_unlock()
{
}

void il2cpp_mono_loader_lock_track_ownership(gboolean track)
{
}

gboolean il2cpp_mono_loader_lock_is_owned_by_self()
{
	return FALSE;
}

gpointer il2cpp_mono_method_get_wrapper_data(Il2CppMonoMethod* method, guint32 id)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return 0;
}

char* il2cpp_mono_type_get_name_full(Il2CppMonoType* type, MonoTypeNameFormat format)
{
	std::string name = il2cpp::vm::Type::GetName((Il2CppType*)type, (Il2CppTypeNameFormat)format);
	return g_strdup(name.c_str());
}

gboolean il2cpp_mono_class_is_nullable(Il2CppMonoClass* klass)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return 0;
}

Il2CppMonoGenericContainer* il2cpp_mono_class_get_generic_container(Il2CppMonoClass* klass)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

void il2cpp_mono_class_setup_interfaces(Il2CppMonoClass* klass, MonoError* error)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
}

GPtrArray* il2cpp_mono_class_get_methods_by_name(Il2CppMonoClass* klass, const char* name, guint32 bflags, gboolean ignore_case, gboolean allow_ctors, MonoError* error)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

gpointer il2cpp_mono_ldtoken_checked(Il2CppMonoImage* image, guint32 token, Il2CppMonoClass** handle_class, Il2CppMonoGenericContext* context, MonoError* error)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return 0;
}

Il2CppMonoClass* il2cpp_mono_class_from_generic_parameter_internal(Il2CppMonoGenericParam* param)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

Il2CppMonoClass* il2cpp_mono_class_load_from_name(Il2CppMonoImage* image, const char* name_space, const char* name)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

Il2CppMonoClass* il2cpp_mono_class_try_load_from_name(Il2CppMonoImage* image, const char* name_space, const char* name)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

Il2CppMonoGenericClass* il2cpp_mono_class_get_generic_class(Il2CppMonoClass* monoClass)
{
	Il2CppClass *klass = (Il2CppClass*)monoClass;
	return (Il2CppMonoGenericClass*)klass->generic_class;
}

Il2CppMonoInternalThread* il2cpp_mono_thread_internal_current()
{
	return (Il2CppMonoInternalThread*)il2cpp_mono_thread_current();
}

void il2cpp_mono_thread_internal_abort(Il2CppMonoInternalThread* thread)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
}

void il2cpp_mono_thread_internal_reset_abort(Il2CppMonoInternalThread* thread)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
}

gunichar2* il2cpp_mono_thread_get_name(Il2CppMonoInternalThread* this_obj, guint32* name_len)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

void il2cpp_mono_thread_set_name_internal(Il2CppMonoInternalThread* this_obj, Il2CppMonoString* name, gboolean permanent, gboolean reset, MonoError* error)
{
	il2cpp::vm::Thread::SetName((Il2CppThread*)this_obj, (Il2CppString*)name);
	mono_error_init(error);
}

void il2cpp_mono_thread_suspend_all_other_threads()
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
}

void il2cpp_mono_stack_mark_record_size(MonoThreadInfo* info, HandleStackMark* stackmark, const char* func_name)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
}

Il2CppMonoRuntimeExceptionHandlingCallbacks* il2cpp_mono_get_eh_callbacks()
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

void il2cpp_mono_reflection_create_custom_attr_data_args(Il2CppMonoImage* image, Il2CppMonoMethod* method, const guchar* data, guint32 len, Il2CppMonoArray** typed_args, Il2CppMonoArray** named_args, CattrNamedArg** named_arg_info, MonoError* error)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
}

void il2cpp_mono_nullable_init(guint8* buf, Il2CppMonoObject* value, Il2CppMonoClass* klass)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
}

Il2CppMonoObject* il2cpp_mono_value_box_checked(Il2CppMonoDomain* domain, Il2CppMonoClass* klass, gpointer value, MonoError* error)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

void il2cpp_mono_field_static_get_value_checked(Il2CppMonoVTable* vt, Il2CppMonoClassField* field, void* value, MonoError* error)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
}

void il2cpp_mono_field_static_get_value_for_thread(Il2CppMonoInternalThread* thread, Il2CppMonoVTable* vt, Il2CppMonoClassField* field, void* value, MonoError* error)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
}

Il2CppMonoObject* il2cpp_mono_field_get_value_object_checked(Il2CppMonoDomain* domain, Il2CppMonoClassField* field, Il2CppMonoObject* obj, MonoError* error)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

Il2CppMonoObject* il2cpp_mono_object_new_checked(Il2CppMonoDomain* domain, Il2CppMonoClass* klass, MonoError* error)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

Il2CppMonoString* il2cpp_mono_ldstr_checked(Il2CppMonoDomain* domain, Il2CppMonoImage* image, guint32 idx, MonoError* error)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

Il2CppMonoObject* il2cpp_mono_runtime_try_invoke(Il2CppMonoMethod* method, void* obj, void** params, Il2CppMonoObject** exc, MonoError* error)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

Il2CppMonoObject* il2cpp_mono_runtime_invoke_checked(Il2CppMonoMethod* method, void* obj, void** params, MonoError* error)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

void il2cpp_mono_gc_base_init()
{
}

int il2cpp_mono_gc_register_root(char* start, size_t size, MonoGCDescriptor descr, MonoGCRootSource source, const char* msg)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return 0;
}

void il2cpp_mono_gc_deregister_root(char* addr)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
}

#ifndef HOST_WIN32
int il2cpp_mono_gc_pthread_create (pthread_t *new_thread, const pthread_attr_t *attr, void *(*start_routine)(void *), void *arg)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return 0;
}
#endif

gboolean il2cpp_mono_gc_is_moving()
{
	return FALSE;
}

gint32 il2cpp_mono_environment_exitcode_get()
{
	return il2cpp::vm::Runtime::GetExitCode();
}

void il2cpp_mono_environment_exitcode_set(gint32 value)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
}

void il2cpp_mono_threadpool_suspend()
{
#if NET_4_0
	il2cpp::vm::ThreadPoolMs::Suspend();
#endif // NET_4_0
}

void il2cpp_mono_threadpool_resume()
{
#if NET_4_0
	il2cpp::vm::ThreadPoolMs::Resume();
#endif // NET_4_0
}

Il2CppMonoImage* il2cpp_mono_assembly_get_image(Il2CppMonoAssembly* assembly)
{
	return (Il2CppMonoImage*)il2cpp::vm::Assembly::GetImage((Il2CppAssembly*)assembly);
}

gboolean il2cpp_mono_runtime_try_shutdown()
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return 0;
}

gboolean il2cpp_mono_verifier_is_method_valid_generic_instantiation(Il2CppMonoMethod* method)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return 0;
}

Il2CppMonoType* il2cpp_mono_reflection_get_type_checked(Il2CppMonoImage* rootimage, Il2CppMonoImage* image, MonoTypeNameParse* info, gboolean ignorecase, gboolean* type_resolve, MonoError* error)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

Il2CppMonoCustomAttrInfo* il2cpp_mono_custom_attrs_from_method_checked(Il2CppMonoMethod* method, MonoError* error)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

Il2CppMonoCustomAttrInfo* il2cpp_mono_custom_attrs_from_class_checked(Il2CppMonoClass* klass, MonoError* error)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

Il2CppMonoCustomAttrInfo* il2cpp_mono_custom_attrs_from_property_checked(Il2CppMonoClass* klass, Il2CppMonoProperty* property, MonoError* error)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

Il2CppMonoCustomAttrInfo* il2cpp_mono_custom_attrs_from_field_checked(Il2CppMonoClass* klass, Il2CppMonoClassField* field, MonoError* error)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

Il2CppMonoReflectionAssemblyHandle il2cpp_mono_assembly_get_object_handle(Il2CppMonoDomain* domain, Il2CppMonoAssembly* assembly, MonoError* error)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return 0;
}

Il2CppMonoReflectionType* il2cpp_mono_type_get_object_checked(Il2CppMonoDomain* domain, Il2CppMonoType* type, MonoError* error)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

void il2cpp_mono_network_init()
{
}

gint il2cpp_mono_w32socket_set_blocking(void* sock, gboolean blocking)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return 0;
}

void il2cpp_mono_profiler_install(Il2CppMonoProfiler* prof, Il2CppMonoProfileFunc callback)
{
	il2cpp::vm::Profiler::Install((Il2CppProfiler*)prof, (Il2CppProfileFunc)callback);
}

void il2cpp_mono_profiler_set_events(MonoProfileFlags events)
{
	il2cpp::vm::Profiler::SetEvents((Il2CppProfileFlags)events);
}

void il2cpp_mono_profiler_install_appdomain(Il2CppMonoProfileAppDomainFunc start_load, Il2CppMonoProfileAppDomainResult end_load, Il2CppMonoProfileAppDomainFunc start_unload, Il2CppMonoProfileAppDomainFunc end_unload)
{
}

void il2cpp_mono_profiler_install_assembly(Il2CppMonoProfileAssemblyFunc start_load, Il2CppMonoProfileAssemblyResult end_load, Il2CppMonoProfileAssemblyFunc start_unload, Il2CppMonoProfileAssemblyFunc end_unload)
{
}

void il2cpp_mono_profiler_install_jit_end(MonoProfileJitResult end)
{
}

void il2cpp_mono_profiler_install_thread(MonoProfileThreadFunc start, MonoProfileThreadFunc end)
{
	il2cpp::utils::Debugger::RegisterThreadCallbacks((ThreadCallback)start, (ThreadCallback)end);
}

void il2cpp_mono_profiler_install_thread_fast_attach_detach(MonoProfileThreadFunc fast_attach, MonoProfileThreadFunc fast_detach)
{
}

void il2cpp_mono_profiler_install_runtime_initialized(Il2CppMonoProfileFunc runtime_initialized_callback)
{
}

char* il2cpp_mono_get_runtime_build_info()
{
	return g_strdup_printf ("%s (%s)", "0.0", "IL2CPP");
}

Il2CppMonoMethod* il2cpp_mono_marshal_method_from_wrapper(Il2CppMonoMethod* wrapper)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

MonoDebugOptions* il2cpp_mini_get_debug_options()
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

gpointer il2cpp_mono_jit_find_compiled_method_with_jit_info(Il2CppMonoDomain* domain, Il2CppMonoMethod* method, MonoJitInfo** ji)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return 0;
}

MonoLMF** il2cpp_mono_get_lmf_addr()
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

void il2cpp_mono_set_lmf(MonoLMF* lmf)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
}

gpointer il2cpp_mono_aot_get_method_checked(Il2CppMonoDomain* domain, Il2CppMonoMethod* method, MonoError* error)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return 0;
}

void il2cpp_mono_arch_setup_resume_sighandler_ctx(MonoContext* ctx, gpointer func)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
}

void il2cpp_mono_arch_set_breakpoint(MonoJitInfo* ji, guint8* ip)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
}

void il2cpp_mono_arch_clear_breakpoint(MonoJitInfo* ji, guint8* ip)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
}

void il2cpp_mono_arch_start_single_stepping()
{
	//IL2CPP_ASSERT(0 && "This method is not yet implemented");
}

void il2cpp_mono_arch_stop_single_stepping()
{
	//IL2CPP_ASSERT(0 && "This method is not yet implemented");
}

void il2cpp_mono_arch_skip_breakpoint(MonoContext* ctx, MonoJitInfo* ji)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
}

void il2cpp_mono_arch_skip_single_step(MonoContext* ctx)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
}

void il2cpp_mono_arch_init_lmf_ext(MonoLMFExt* ext, gpointer prev_lmf)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
}

mgreg_t il2cpp_mono_arch_context_get_int_reg(MonoContext* ctx, int reg)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return 0;
}

void il2cpp_mono_arch_context_set_int_reg(MonoContext* ctx, int reg, mgreg_t val)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
}

void il2cpp_mono_walk_stack_with_ctx(Il2CppMonoJitStackWalk func, MonoContext* start_ctx, MonoUnwindOptions unwind_options, void* user_data)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
}

void il2cpp_mono_walk_stack_with_state(Il2CppMonoJitStackWalk func, MonoThreadUnwindState* state, MonoUnwindOptions unwind_options, void* user_data)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
}

gboolean il2cpp_mono_thread_state_init_from_current(MonoThreadUnwindState* ctx)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return 0;
}

gboolean il2cpp_mono_thread_state_init_from_monoctx(MonoThreadUnwindState* ctx, MonoContext* mctx)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return 0;
}

MonoJitInfo* il2cpp_mini_jit_info_table_find(Il2CppMonoDomain* domain, char* addr, Il2CppMonoDomain** out_domain)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

void il2cpp_mono_restore_context(MonoContext* ctx)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
}

gboolean il2cpp_mono_find_jit_info_ext(Il2CppMonoDomain* domain, MonoJitTlsData* jit_tls, MonoJitInfo* prev_ji, MonoContext* ctx, MonoContext* new_ctx, char** trace, MonoLMF** lmf, mgreg_t** save_locations, StackFrameInfo* frame)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return 0;
}

Il2CppMonoMethod* il2cpp_mono_method_get_declaring_generic_method(Il2CppMonoMethod* method)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

gboolean il2cpp_mono_error_ok (MonoError *error)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return 0;
}

Il2CppMonoMethod* il2cpp_jinfo_get_method (MonoJitInfo *ji)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

gboolean il2cpp_mono_find_prev_seq_point_for_native_offset (Il2CppMonoDomain *domain, Il2CppMonoMethod *method, gint32 native_offset, MonoSeqPointInfo **info, SeqPoint* seq_point)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return 0;
}

void il2cpp_mono_error_cleanup (MonoError *oerror)
{
}

void il2cpp_mono_error_init (MonoError *error)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
}

void* il2cpp_mono_w32socket_accept_internal (void* s, struct sockaddr *addr, void *addrlen, gboolean blocking)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return 0;
}

gboolean il2cpp_mono_find_next_seq_point_for_native_offset (Il2CppMonoDomain *domain, Il2CppMonoMethod *method, gint32 native_offset, MonoSeqPointInfo **info, SeqPoint* seq_point)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return 0;
}

gboolean il2cpp_mono_class_has_parent (Il2CppMonoClass *klass, Il2CppMonoClass *parent)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return 0;
}

Il2CppMonoGenericParam* il2cpp_mono_generic_container_get_param (Il2CppMonoGenericContainer *gc, int i)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

gboolean il2cpp_mono_find_seq_point (Il2CppMonoDomain *domain, Il2CppMonoMethod *method, gint32 il_offset, MonoSeqPointInfo **info, SeqPoint *seq_point)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return 0;
}

void il2cpp_mono_seq_point_iterator_init (SeqPointIterator* it, MonoSeqPointInfo* info)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
}

gboolean il2cpp_mono_seq_point_iterator_next (SeqPointIterator* it)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return 0;
}

void il2cpp_mono_seq_point_init_next (MonoSeqPointInfo* info, SeqPoint sp, SeqPoint* next)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
}

MonoSeqPointInfo* il2cpp_mono_get_seq_points (Il2CppMonoDomain *domain, Il2CppMonoMethod *method)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

void IL2CPP_G_BREAKPOINT()
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
}

void il2cpp_mono_thread_info_safe_suspend_and_run (MonoNativeThreadId id, gboolean interrupt_kernel, MonoSuspendThreadCallback callback, gpointer user_data)
{
	callback(NULL, user_data);
}

Il2CppMonoException* il2cpp_mono_error_convert_to_exception (MonoError *target_error)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

const char* il2cpp_mono_error_get_message (MonoError *oerror)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

void il2cpp_mono_error_assert_ok_pos (MonoError *error, const char* filename, int lineno)
{
	if (error->error_code == MONO_ERROR_NONE)
		return;

	g_error ("%s:%d\n", filename, lineno);
}

void* il2cpp_mono_gc_alloc_fixed (size_t size, void* descr, MonoGCRootSource source, const char *msg)
{
	return il2cpp_gc_alloc_fixed(size);
}

typedef void* (*Il2CppMonoGCLockedCallbackFunc) (void *data);
void* il2cpp_mono_gc_invoke_with_gc_lock (Il2CppMonoGCLockedCallbackFunc func, void *data)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

int il2cpp_mono_profiler_get_events (void)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return 0;
}
void il2cpp_mono_profiler_iomap (char *report, const char *pathname, const char *new_pathname)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
}

// These functions expose the IL2CPP VM C++ API to C

void il2cpp_set_thread_state_background(Il2CppMonoThread* thread)
{
	il2cpp::vm::Thread::SetState((Il2CppThread*)thread, il2cpp::vm::ThreadState::kThreadStateBackground);
}

void* il2cpp_domain_get_agent_info(Il2CppMonoAppDomain* domain)
{
	return ((Il2CppDomain*)domain)->agent_info;
}

void il2cpp_domain_set_agent_info(Il2CppMonoAppDomain* domain, void* agentInfo)
{
	((Il2CppDomain*)domain)->agent_info = agentInfo;
}

void il2cpp_send_assemblies_for_domain (Il2CppMonoAppDomain *domain, void *user_data, emit_assembly_load_callback callback)
{
	il2cpp::vm::AssemblyVector* assemblies = il2cpp::vm::Assembly::GetAllAssemblies();
	for (il2cpp::vm::AssemblyVector::iterator it = assemblies->begin(); it != assemblies->end(); it++)
	{
		callback((void*)*it, NULL);
	}
}

void il2cpp_start_debugger_thread()
{
	il2cpp::utils::Debugger::StartDebuggerThread();
}

uintptr_t il2cpp_get_thread_id(Il2CppMonoThread* thread)
{
	return il2cpp::vm::Thread::GetId((Il2CppThread*)thread);
}

void* il2cpp_gc_alloc_fixed(size_t size)
{
	return il2cpp::gc::GarbageCollector::AllocateFixed(size, NULL);
}

void il2cpp_gc_free_fixed(void* address)
{
	il2cpp::gc::GarbageCollector::FreeFixed(address);
}

char* il2cpp_assembly_get_name(Il2CppMonoAssembly* assembly)
{
	std::string name = il2cpp::vm::AssemblyName::AssemblyNameToString(((Il2CppAssembly*)assembly)->aname);
	return g_strdup(name.c_str());
}

const char* il2cpp_domain_get_name(Il2CppMonoDomain* domain)
{
	return ((Il2CppDomain*)domain)->friendly_name;
}

int il2cpp_mono_type_get_attrs(Il2CppMonoType* type)
{
	return ((Il2CppType*)type)->attrs;
}

Il2CppSequencePointC* il2cpp_get_sequence_points(void* *iter)
{
	return (Il2CppSequencePointC*)il2cpp::utils::Debugger::GetSequencePoints(iter);
}

gboolean il2cpp_mono_methods_match(Il2CppMonoMethod* left, Il2CppMonoMethod* right)
{
	MethodInfo* leftMethod = (MethodInfo*)left;
	MethodInfo* rightMethod = (MethodInfo*)right;

	if (rightMethod == leftMethod)
		return TRUE;
	if (rightMethod == NULL || leftMethod == NULL)
		return FALSE;
	if (rightMethod->is_inflated && rightMethod->genericMethod->methodDefinition == leftMethod)
		return TRUE;
	if (leftMethod->is_inflated && leftMethod->genericMethod->methodDefinition == rightMethod)
		return TRUE;

	return FALSE;
}

Il2CppMonoGenericInst* il2cpp_generic_class_get_inst(Il2CppMonoGenericClass *monoGenClass)
{
	Il2CppGenericClass *klass = (Il2CppGenericClass*) monoGenClass;
	return (Il2CppMonoGenericInst*)klass->context.class_inst;
}

guint il2cpp_generic_inst_type_argc(Il2CppMonoGenericInst *monoInst)
{
	Il2CppGenericInst *inst = (Il2CppGenericInst*)monoInst;
	return inst->type_argc;
}

Il2CppMonoType* il2cpp_generic_inst_type_arg(Il2CppMonoGenericInst *monoInst, int i)
{
	Il2CppGenericInst *inst = (Il2CppGenericInst*)monoInst;
	return (Il2CppMonoType*)inst->type_argv[i];
}

Il2CppMonoType* il2cpp_class_this_arg(Il2CppMonoClass *monoClass)
{
	Il2CppClass *klass = (Il2CppClass*)monoClass;
	return (Il2CppMonoType*)klass->this_arg;
}

Il2CppClass* il2cpp_class_get_nested_types_accepts_generic(Il2CppClass *klass, void* *iter)
{
	if (klass->generic_class)
		return NULL;

	return il2cpp::vm::Class::GetNestedTypes(klass, iter);
}

Il2CppMonoClass* il2cpp_defaults_object_class()
{
	return (Il2CppMonoClass*)il2cpp_defaults.object_class;
}

}

#endif // RUNTIME_IL2CPP
