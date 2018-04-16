#if defined(RUNTIME_IL2CPP)

#include "il2cpp-config.h"
#include <il2cpp-class-internals.h>
#include "gc/GCHandle.h"
#include "gc/GarbageCollector.h"
#include "gc/WriteBarrier.h"
#include "gc/gc_wrapper.h"
#include "metadata/FieldLayout.h"
#include "vm/Assembly.h"
#include "vm/AssemblyName.h"
#include "vm/Class.h"
#include "vm/Domain.h"
#include "vm/Field.h"
#include "vm/GenericContainer.h"
#include "vm/GenericClass.h"
#include "vm/Image.h"
#include "vm/Method.h"
#include "vm/Object.h"
#include "vm/Profiler.h"
#include "vm/Reflection.h"
#include "vm/Runtime.h"
#include "vm/String.h"
#include "vm/Thread.h"
#include "vm/ThreadPoolMs.h"
#include "vm/Type.h"
#include "vm-utils/Debugger.h"
#include "metadata/GenericMetadata.h"

extern "C" {

#include <glib.h>
#include <mono/utils/mono-coop-mutex.h>
#include <mono/utils/mono-string.h>
#include <mono/metadata/handle.h>
#include <mono/metadata/object-internals.h>
#include <mono/metadata/appdomain.h>
#include <mono/metadata/mono-debug.h>
#include <mono/metadata/debug-mono-symfile.h>
#include <mono/metadata/profiler-private.h>
#include <mono/metadata/profiler.h>
#include <mono/sgen/sgen-conf.h>
#include <mono/metadata/seq-points-data.h>
#include "il2cpp-c-types.h"

Il2CppMonoDefaults il2cpp_mono_defaults;
Il2CppMonoDebugOptions il2cpp_mono_debug_options;

#include <mono/metadata/il2cpp-compat-metadata.h>

static MonoGHashTable *method_signatures;

static il2cpp::os::Mutex s_il2cpp_mono_loader_lock(false);
static uint64_t s_il2cpp_mono_loader_lock_tid = 0;

MonoMethod* il2cpp_mono_image_get_entry_point (MonoImage *image)
{
	return (MonoMethod*)il2cpp::vm::Image::GetEntryPoint((Il2CppImage*)image);
}

const char* il2cpp_mono_image_get_filename (MonoImage *monoImage)
{
	Il2CppImage *image = (Il2CppImage*)monoImage;
	return image->name;
}

const char*  il2cpp_mono_image_get_guid (MonoImage *image)
{
	return "00000000-0000-0000-0000-000000000000"; //IL2CPP doesn't have image GUIDs
}

MonoClass* il2cpp_mono_type_get_class (MonoType *type)
{
	return (MonoClass*) il2cpp::vm::Type::GetClass((Il2CppType*)type);
}

mono_bool il2cpp_mono_type_is_struct (MonoType *type)
{
	return il2cpp::vm::Type::IsStruct((Il2CppType*)type);
}

mono_bool il2cpp_mono_type_is_reference (MonoType *type)
{
	return il2cpp::vm::Type::IsReference((Il2CppType*)type);
}

void il2cpp_mono_metadata_free_mh (MonoMethodHeader *mh)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
}

Il2CppMonoMethodSignature* il2cpp_mono_method_signature (MonoMethod *m)
{
	MethodInfo* method = (MethodInfo*)m;

	if (method_signatures == NULL)
		method_signatures = mono_g_hash_table_new_type(NULL, NULL, MONO_HASH_KEY_GC, MONO_ROOT_SOURCE_DEBUGGER, NULL, "method-to-signature for il2cpp table");

	Il2CppMonoMethodSignature* existing_signature = (Il2CppMonoMethodSignature*)mono_g_hash_table_lookup(method_signatures, method);
	if (existing_signature != NULL)
		return existing_signature;

	Il2CppMonoMethodSignature* sig = g_new(Il2CppMonoMethodSignature, 1);

	sig->call_convention = MONO_CALL_DEFAULT;
	sig->hasthis = il2cpp::vm::Method::IsInstance(method);
	sig->ret = (MonoType*)il2cpp::vm::Method::GetReturnType(method);

	sig->generic_param_count = 0;

	if (method->is_generic)
	{
		sig->generic_param_count = il2cpp::vm::Method::GetGenericParamCount(method);
	}
	else if (method->is_inflated)
	{
		if (method->genericMethod->context.method_inst)
			sig->generic_param_count += method->genericMethod->context.method_inst->type_argc;

		if (method->genericMethod->context.class_inst)
			sig->generic_param_count += method->genericMethod->context.class_inst->type_argc;
	}

	sig->param_count = il2cpp::vm::Method::GetParamCount(method);
	sig->params = g_new(MonoType*, sig->param_count);
	for (int i = 0; i < sig->param_count; ++i)
		sig->params[i] = (MonoType*)il2cpp::vm::Method::GetParam(method, i);

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
        method_signatures = NULL;
	}
}

void il2cpp_mono_method_get_param_names (MonoMethod *m, const char **names)
{
	MethodInfo* method = (MethodInfo*)m;
	uint32_t numberOfParameters = il2cpp::vm::Method::GetParamCount(method);
	for (int i = 0; i < numberOfParameters; ++i)
		names[i] = il2cpp::vm::Method::GetParamName(method, i);
}

mono_bool il2cpp_mono_type_generic_inst_is_valuetype (MonoType *monoType)
{
	static const int kBitIsValueType = 1;
	Il2CppType *type = (Il2CppType*)monoType;
	const Il2CppTypeDefinition *typeDef = il2cpp::vm::MetadataCache::GetTypeDefinitionFromIndex(type->data.generic_class->typeDefinitionIndex);
	return (typeDef->bitfield >> (kBitIsValueType - 1)) & 0x1;
}

MonoMethodHeader* il2cpp_mono_method_get_header_checked (MonoMethod *method, MonoError *error)
{
	return NULL;
}

gboolean il2cpp_mono_class_init (MonoClass *klass)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return 0;
}

MonoVTable* il2cpp_mono_class_vtable (MonoDomain *domain, MonoClass *klass)
{
	return (MonoVTable*)((Il2CppClass*)klass)->vtable;
}

MonoClassField* il2cpp_mono_class_get_field_from_name (MonoClass *klass, const char *name)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

int32_t il2cpp_mono_array_element_size (MonoClass *monoClass)
{
	Il2CppClass *klass = (Il2CppClass*)monoClass;
	return klass->element_size;
}

int32_t il2cpp_mono_class_instance_size (MonoClass *klass)
{
	return il2cpp::vm::Class::GetInstanceSize((Il2CppClass*)klass);
}

int32_t il2cpp_mono_class_value_size (MonoClass *klass, uint32_t *align)
{
	return il2cpp::vm::Class::GetValueSize((Il2CppClass*)klass, align);
}

gboolean il2cpp_mono_class_is_assignable_from (MonoClass *klass, MonoClass *oklass)
{
	return il2cpp::vm::Class::IsAssignableFrom((Il2CppClass*)klass, (Il2CppClass*)oklass);
}

MonoClass* il2cpp_mono_class_from_mono_type (MonoType *type)
{
	return (MonoClass*)il2cpp::vm::Class::FromIl2CppType((Il2CppType*)type);
}

int il2cpp_mono_class_num_fields (MonoClass *klass)
{
	return il2cpp::vm::Class::GetNumFields((Il2CppClass*)klass);
}

int il2cpp_mono_class_num_methods (MonoClass *klass)
{
	return il2cpp::vm::Class::GetNumMethods((Il2CppClass*)klass);
}

int il2cpp_mono_class_num_properties (MonoClass *klass)
{
	return il2cpp::vm::Class::GetNumProperties((Il2CppClass*)klass);
}

MonoClassField* il2cpp_mono_class_get_fields (MonoClass* klass, gpointer *iter)
{
	return (MonoClassField*)il2cpp::vm::Class::GetFields((Il2CppClass*)klass, iter);
}

MonoMethod* il2cpp_mono_class_get_methods (MonoClass* klass, gpointer *iter)
{
	return (MonoMethod*)il2cpp::vm::Class::GetMethods((Il2CppClass*)klass, iter);
}

MonoProperty* il2cpp_mono_class_get_properties (MonoClass* klass, gpointer *iter)
{
	return (MonoProperty*)il2cpp::vm::Class::GetProperties((Il2CppClass*)klass, iter);
}

const char* il2cpp_mono_field_get_name (MonoClassField *field)
{
	return il2cpp::vm::Field::GetName((FieldInfo*)field);
}

mono_unichar2* il2cpp_mono_string_chars (MonoString *monoStr)
{
	Il2CppString *str = (Il2CppString*)monoStr;
	return (mono_unichar2*)str->chars;
}

int il2cpp_mono_string_length (MonoString *monoStr)
{
	Il2CppString *str = (Il2CppString*)monoStr;
	return str->length;
}

char* il2cpp_mono_array_addr_with_size (MonoArray *array, int size, uintptr_t idx)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

uintptr_t il2cpp_mono_array_length (MonoArray *array)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return 0;
}

MonoString* il2cpp_mono_string_new (MonoDomain *domain, const char *text)
{
	return (MonoString*)il2cpp::vm::String::New(text);
}


MonoString* il2cpp_mono_string_new_checked (MonoDomain *domain, const char *text, MonoError *merror)
{
	error_init(merror);
	return il2cpp_mono_string_new (domain, text);
}

char* il2cpp_mono_string_to_utf8_checked (MonoString *string_obj, MonoError *error)
{
	error_init(error);
	Il2CppString *str = (Il2CppString*)string_obj;
	std::string s = il2cpp::utils::StringUtils::Utf16ToUtf8(str->chars, str->length);
	return g_strdup(s.c_str());
}

int il2cpp_mono_object_hash (MonoObject* obj)
{
	return (int)((intptr_t)obj >> 3);
}

void* il2cpp_mono_object_unbox (MonoObject *monoObj)
{
	Il2CppObject *obj = (Il2CppObject*)monoObj;
	return il2cpp::vm::Object::Unbox(obj);
}

void il2cpp_mono_field_set_value (MonoObject *obj, MonoClassField *field, void *value)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
}

void il2cpp_mono_field_static_set_value (MonoVTable *vt, MonoClassField *field, void *value)
{
	il2cpp::vm::Field::StaticSetValue((FieldInfo*)field, value);
}

uint32_t il2cpp_mono_gchandle_new_weakref (MonoObject *obj, mono_bool track_resurrection)
{
	return il2cpp::gc::GCHandle::NewWeakref((Il2CppObject*)obj, track_resurrection == 0 ? false : true);
}

MonoObject*  il2cpp_mono_gchandle_get_target  (uint32_t gchandle)
{
	return (MonoObject*)il2cpp::gc::GCHandle::GetTarget(gchandle);
}

void il2cpp_mono_gchandle_free (uint32_t gchandle)
{
	il2cpp::gc::GCHandle::Free(gchandle);
}

void il2cpp_mono_gc_wbarrier_generic_store (void* ptr, MonoObject* value)
{
	il2cpp::gc::WriteBarrier::GenericStore(ptr, (Il2CppObject*)value);
}

int il2cpp_mono_reflection_parse_type_checked (char *name, Il2CppMonoTypeNameParse *monoInfo, MonoError *error)
{
	error_init(error);
	il2cpp::vm::TypeNameParseInfo *pInfo = new il2cpp::vm::TypeNameParseInfo();
	std::string nameStr = name;
	std::replace(nameStr.begin(), nameStr.end(), '/', '+');
	il2cpp::vm::TypeNameParser parser(nameStr, *pInfo, false);
	monoInfo->assembly.name = NULL;
	monoInfo->il2cppTypeNameParseInfo = pInfo;
	return parser.Parse();
}

void il2cpp_mono_reflection_free_type_info (Il2CppMonoTypeNameParse *info)
{
	delete (il2cpp::vm::TypeNameParseInfo*)info->il2cppTypeNameParseInfo;
}

MonoDomain* il2cpp_mono_get_root_domain (void)
{
	return (MonoDomain*)il2cpp::vm::Domain::GetCurrent();
}

void il2cpp_mono_runtime_quit (void)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
}

gboolean il2cpp_mono_runtime_is_shutting_down (void)
{
	return il2cpp::vm::Runtime::IsShuttingDown() ? TRUE : FALSE;
}

MonoDomain* il2cpp_mono_domain_get (void)
{
	return il2cpp_mono_get_root_domain();
}

gboolean il2cpp_mono_domain_set (MonoDomain *domain, gboolean force)
{
	IL2CPP_ASSERT(domain == il2cpp_mono_get_root_domain());
	return TRUE;
}

void il2cpp_mono_domain_foreach(MonoDomainFunc func, gpointer user_data)
{
	func((MonoDomain*)il2cpp_mono_get_root_domain(), user_data);
}

MonoJitInfo* il2cpp_mono_jit_info_table_find(MonoDomain* domain, char* addr)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

MonoMethod* il2cpp_mono_jit_info_get_method(MonoJitInfo* ji)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

gint32 il2cpp_mono_debug_il_offset_from_address(MonoMethod* method, MonoDomain* domain, guint32 native_offset)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return 0;
}

void il2cpp_mono_set_is_debugger_attached(gboolean attached)
{
#if IL2CPP_MONO_DEBUGGER
	il2cpp::utils::Debugger::SetIsDebuggerAttached(attached == TRUE);
#endif
}

char* il2cpp_mono_type_full_name(MonoType* type)
{
	std::string name = il2cpp::vm::Type::GetName((Il2CppType*)type, IL2CPP_TYPE_NAME_FORMAT_FULL_NAME);
	return g_strdup(name.c_str());
}

char* il2cpp_mono_method_full_name(MonoMethod* method, gboolean signature)
{
	return g_strdup(((MethodInfo*)method)->name);
}

MonoThread* il2cpp_mono_thread_current()
{
	return (MonoThread*)il2cpp::vm::Thread::Current();
}

MonoThread* il2cpp_mono_thread_get_main()
{
	return (MonoThread*)il2cpp::vm::Thread::Main();
}

MonoThread* il2cpp_mono_thread_attach(MonoDomain* domain)
{
	return (MonoThread*)il2cpp::vm::Thread::Attach((Il2CppDomain*)domain);
}

void il2cpp_mono_thread_detach(MonoThread* thread)
{
	il2cpp::vm::Thread::Detach((Il2CppThread*)thread);
}

void il2cpp_mono_domain_lock(MonoDomain* domain)
{
}

void il2cpp_mono_domain_unlock(MonoDomain* domain)
{
}

MonoJitInfo* il2cpp_mono_jit_info_table_find_internal(MonoDomain* domain, char* addr, gboolean try_aot, gboolean allow_trampolines)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

guint il2cpp_mono_aligned_addr_hash(gconstpointer ptr)
{
	return GPOINTER_TO_UINT(ptr) >> 3;
}

MonoGenericInst* il2cpp_mono_metadata_get_generic_inst(int type_argc, MonoType** type_argv)
{
	return (MonoGenericInst*)il2cpp::vm::MetadataCache::GetGenericInst((Il2CppType**)type_argv, type_argc);
}

MonoMethod* il2cpp_mono_get_method_checked(MonoImage* image, guint32 token, MonoClass* klass, MonoGenericContext* context, MonoError* error)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

SgenDescriptor il2cpp_mono_gc_make_root_descr_all_refs(int numbits)
{
	return NULL;
}

int il2cpp_mono_gc_register_root_wbarrier (char *start, size_t size, MonoGCDescriptor descr, MonoGCRootSource source, void *key, const char *msg)
{
	il2cpp::gc::GarbageCollector::RegisterRoot(start, size);
	return 1;
}

MonoGCDescriptor il2cpp_mono_gc_make_vector_descr (void)
{
	return 0;
}

int il2cpp_mono_class_interface_offset_with_variance(MonoClass* klass, MonoClass* itf, gboolean* non_exact_match)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return 0;
}

void il2cpp_mono_class_setup_supertypes(MonoClass* klass)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
}

void il2cpp_mono_class_setup_vtable(MonoClass* klass)
{
	il2cpp::vm::Class::Init((Il2CppClass*)klass);
}

void il2cpp_mono_class_setup_methods(MonoClass* klass)
{
	il2cpp::vm::Class::SetupMethods((Il2CppClass*)klass);
}

gboolean il2cpp_mono_class_field_is_special_static(MonoClassField* field)
{
	return il2cpp::vm::Field::IsNormalStatic((FieldInfo*)field) ? FALSE : TRUE;
}

guint32 il2cpp_mono_class_field_get_special_static_type(MonoClassField* field)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return 0;
}

MonoGenericContext* il2cpp_mono_class_get_context(MonoClass* klass)
{
	return (MonoGenericContext*)&((Il2CppClass*)klass)->generic_class->context;
}

MonoGenericContext* il2cpp_mono_method_get_context(MonoMethod* monoMethod)
{
	MethodInfo * method = (MethodInfo*)monoMethod;

	if (!method->is_inflated || method->is_generic)
		return NULL;

	return (MonoGenericContext*) &((MethodInfo*)method)->genericMethod->context;
}

MonoGenericContainer* il2cpp_mono_method_get_generic_container(MonoMethod* monoMethod)
{
	MethodInfo * method = (MethodInfo*)monoMethod;

	if (method->is_inflated || !method->is_generic)
		return NULL;

	return (MonoGenericContainer*) method->genericContainer;
}

MonoMethod* il2cpp_mono_class_inflate_generic_method_full_checked(MonoMethod* method, MonoClass* klass_hint, MonoGenericContext* context, MonoError* error)
{
	error_init(error);
	return (MonoMethod*) il2cpp::metadata::GenericMetadata::Inflate((MethodInfo*)method, (Il2CppClass*)klass_hint, (Il2CppGenericContext*)context);
}

MonoMethod* il2cpp_mono_class_inflate_generic_method_checked(MonoMethod* method, MonoGenericContext* context, MonoError* error)
{
	error_init(error);
	return (MonoMethod*)il2cpp::metadata::GenericMetadata::Inflate((MethodInfo*)method, NULL, (Il2CppGenericContext*)context);
}

void il2cpp_mono_loader_lock()
{
	s_il2cpp_mono_loader_lock.Lock();
	s_il2cpp_mono_loader_lock_tid = il2cpp::os::Thread::CurrentThreadId();
}

void il2cpp_mono_loader_unlock()
{
	s_il2cpp_mono_loader_lock_tid = 0;
	s_il2cpp_mono_loader_lock.Unlock();
}

void il2cpp_mono_loader_lock_track_ownership(gboolean track)
{
}

gboolean il2cpp_mono_loader_lock_is_owned_by_self()
{
	return s_il2cpp_mono_loader_lock_tid == il2cpp::os::Thread::CurrentThreadId();
}

gpointer il2cpp_mono_method_get_wrapper_data(MonoMethod* method, guint32 id)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return 0;
}

char* il2cpp_mono_type_get_name_full(MonoType* type, MonoTypeNameFormat format)
{
	std::string name = il2cpp::vm::Type::GetName((Il2CppType*)type, (Il2CppTypeNameFormat)format);
	return g_strdup(name.c_str());
}

gboolean il2cpp_mono_class_is_nullable(MonoClass* klass)
{
	return il2cpp::vm::Class::IsNullable((Il2CppClass*)klass);
}

MonoGenericContainer* il2cpp_mono_class_get_generic_container(MonoClass* klass)
{
    return (MonoGenericContainer*)il2cpp::vm::Class::GetGenericContainer((Il2CppClass*)klass);
}

void il2cpp_mono_class_setup_interfaces(MonoClass* klass, MonoError* error)
{
	error_init(error);
	il2cpp::vm::Class::SetupInterfaces((Il2CppClass*)klass);
}

enum {
	BFLAGS_IgnoreCase = 1,
	BFLAGS_DeclaredOnly = 2,
	BFLAGS_Instance = 4,
	BFLAGS_Static = 8,
	BFLAGS_Public = 0x10,
	BFLAGS_NonPublic = 0x20,
	BFLAGS_FlattenHierarchy = 0x40,
	BFLAGS_InvokeMethod = 0x100,
	BFLAGS_CreateInstance = 0x200,
	BFLAGS_GetField = 0x400,
	BFLAGS_SetField = 0x800,
	BFLAGS_GetProperty = 0x1000,
	BFLAGS_SetProperty = 0x2000,
	BFLAGS_ExactBinding = 0x10000,
	BFLAGS_SuppressChangeType = 0x20000,
	BFLAGS_OptionalParamBinding = 0x40000
};

static gboolean
method_nonpublic (MethodInfo* method, gboolean start_klass)
{
	switch (method->flags & METHOD_ATTRIBUTE_MEMBER_ACCESS_MASK) {
		case METHOD_ATTRIBUTE_ASSEM:
			return (start_klass || il2cpp_defaults.generic_ilist_class);
		case METHOD_ATTRIBUTE_PRIVATE:
			return start_klass;
		case METHOD_ATTRIBUTE_PUBLIC:
			return FALSE;
		default:
			return TRUE;
	}
}

GPtrArray* il2cpp_mono_class_get_methods_by_name(MonoClass* il2cppMonoKlass, const char* name, guint32 bflags, gboolean ignore_case, gboolean allow_ctors, MonoError* error)
{
	GPtrArray *array;
	Il2CppClass *klass = (Il2CppClass*)il2cppMonoKlass;
	Il2CppClass *startklass;
	MethodInfo *method;
	gpointer iter;
	int match, nslots;
	/*FIXME, use MonoBitSet*/
	/*guint32 method_slots_default [8];
	guint32 *method_slots = NULL;*/
	int (*compare_func) (const char *s1, const char *s2) = NULL;

	array = g_ptr_array_new ();
	startklass = klass;
	error_init (error);

	if (name != NULL)
		compare_func = (ignore_case) ? mono_utf8_strcasecmp : strcmp;

	/*il2cpp_mono_class_setup_methods (klass);
	il2cpp_mono_class_setup_vtable (klass);

	if (is_generic_parameter (&klass->byval_arg))
		nslots = mono_class_get_vtable_size (klass->parent);
	else
		nslots = MONO_CLASS_IS_INTERFACE (klass) ? mono_class_num_methods (klass) : mono_class_get_vtable_size (klass);
	if (nslots >= sizeof (method_slots_default) * 8) {
		method_slots = g_new0 (guint32, nslots / 32 + 1);
	} else {
		method_slots = method_slots_default;
		memset (method_slots, 0, sizeof (method_slots_default));
	}*/
handle_parent:
	il2cpp_mono_class_setup_methods ((MonoClass*)klass);
	il2cpp_mono_class_setup_vtable ((MonoClass*)klass);

	iter = NULL;
	while ((method = (MethodInfo*)il2cpp_mono_class_get_methods ((MonoClass*)klass, &iter))) {
		match = 0;
		/*if (method->slot != -1) {
			g_assert (method->slot < nslots);
			if (method_slots [method->slot >> 5] & (1 << (method->slot & 0x1f)))
				continue;
			if (!(method->flags & METHOD_ATTRIBUTE_NEW_SLOT))
				method_slots [method->slot >> 5] |= 1 << (method->slot & 0x1f);
		}*/

		if (!allow_ctors && method->name [0] == '.' && (strcmp (method->name, ".ctor") == 0 || strcmp (method->name, ".cctor") == 0))
			continue;
		if ((method->flags & METHOD_ATTRIBUTE_MEMBER_ACCESS_MASK) == METHOD_ATTRIBUTE_PUBLIC) {
			if (bflags & BFLAGS_Public)
				match++;
		} else if ((bflags & BFLAGS_NonPublic) && method_nonpublic (method, (klass == startklass))) {
				match++;
		}
		if (!match)
			continue;
		match = 0;
		if (method->flags & METHOD_ATTRIBUTE_STATIC) {
			if (bflags & BFLAGS_Static)
				if ((bflags & BFLAGS_FlattenHierarchy) || (klass == startklass))
					match++;
		} else {
			if (bflags & BFLAGS_Instance)
				match++;
		}

		if (!match)
			continue;

		if (name != NULL) {
			if (compare_func (name, method->name))
				continue;
		}

		match = 0;
		g_ptr_array_add (array, method);
	}
	if (!(bflags & BFLAGS_DeclaredOnly) && (klass = klass->parent))
		goto handle_parent;
	/*if (method_slots != method_slots_default)
		g_free (method_slots);*/

	return array;
}

gpointer il2cpp_mono_ldtoken_checked(MonoImage* image, guint32 token, MonoClass** handle_class, MonoGenericContext* context, MonoError* error)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return 0;
}

MonoClass* il2cpp_mono_class_from_generic_parameter_internal(MonoGenericParam* param)
{
	return (MonoClass*)il2cpp::vm::Class::FromGenericParameter((Il2CppGenericParameter*)param);
}

MonoClass* il2cpp_mono_class_load_from_name(MonoImage* image, const char* name_space, const char* name)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

MonoGenericClass* il2cpp_mono_class_get_generic_class(MonoClass* monoClass)
{
	Il2CppClass *klass = (Il2CppClass*)monoClass;
	return (MonoGenericClass*)klass->generic_class;
}

MonoInternalThread* il2cpp_mono_thread_internal_current()
{
	return (MonoInternalThread*)(((Il2CppThread*)il2cpp_mono_thread_current())->internal_thread);
}

gboolean il2cpp_mono_thread_internal_is_current(MonoInternalThread* thread)
{
	return il2cpp_mono_thread_internal_current () == thread;
}

void il2cpp_mono_thread_internal_abort(MonoInternalThread* thread, gboolean appdomain_unload)
{
	il2cpp::vm::Thread::RequestAbort((Il2CppInternalThread*)thread);
}

void il2cpp_mono_thread_internal_reset_abort(MonoInternalThread* thread)
{
	il2cpp::vm::Thread::ResetAbort((Il2CppInternalThread*)thread);
}

gunichar2* il2cpp_mono_thread_get_name(MonoInternalThread* this_obj, guint32* name_len)
{
	std::string name = il2cpp::vm::Thread::GetName((Il2CppInternalThread*)this_obj);

	if (name_len != NULL)
		*name_len = name.size();

	if (name.empty())
		return NULL;
	return g_utf8_to_utf16(name.c_str(), name.size(), NULL, NULL, NULL);
}

void il2cpp_mono_thread_set_name_internal(MonoInternalThread* this_obj, MonoString* name, gboolean permanent, gboolean reset, MonoError* error)
{
	il2cpp::vm::Thread::SetName((Il2CppInternalThread*)this_obj, (Il2CppString*)name);
	error_init(error);
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

void il2cpp_mono_nullable_init(guint8* buf, MonoObject* value, MonoClass* klass)
{
	il2cpp::vm::Object::NullableInit(buf, (Il2CppObject*)value, (Il2CppClass*)klass);
}

MonoObject* il2cpp_mono_value_box_checked(MonoDomain* domain, MonoClass* klass, gpointer value, MonoError* error)
{
	error_init(error);
	return (MonoObject*)il2cpp::vm::Object::Box((Il2CppClass*)klass, value);
}

void il2cpp_mono_field_static_get_value_checked(MonoVTable* vt, MonoClassField* field, void* value, MonoError* error)
{
	error_init(error);
	il2cpp::vm::Field::StaticGetValue((FieldInfo*)field, value);
}

void il2cpp_mono_field_static_get_value_for_thread(MonoInternalThread* thread, MonoVTable* vt, MonoClassField* field, void* value, MonoError* error)
{
	error_init(error);
	il2cpp::vm::Field::StaticGetValueForThread((FieldInfo*)field, value, (Il2CppInternalThread*)thread);
}

MonoObject* il2cpp_mono_field_get_value_object_checked(MonoDomain* domain, MonoClassField* field, MonoObject* obj, MonoError* error)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

MonoObject* il2cpp_mono_object_new_checked(MonoDomain* domain, MonoClass* klass, MonoError* error)
{
	error_init(error);
	return (MonoObject*)il2cpp::vm::Object::New((Il2CppClass*)klass);
}

MonoString* il2cpp_mono_ldstr_checked(MonoDomain* domain, MonoImage* image, guint32 idx, MonoError* error)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

MonoObject* il2cpp_mono_runtime_try_invoke(MonoMethod* method, void* obj, void** params, MonoObject** exc, MonoError* error)
{
	error_init(error);

	if (((MethodInfo*)method)->klass->valuetype)
		obj = static_cast<Il2CppObject*>(obj) - 1;

	return (MonoObject*)il2cpp::vm::Runtime::Invoke((MethodInfo*)method, obj, params, (Il2CppException**)exc);
}

MonoObject* il2cpp_mono_runtime_invoke_checked(MonoMethod* method, void* obj, void** params, MonoError* error)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

void il2cpp_mono_gc_base_init()
{
}

static il2cpp::os::Mutex s_il2cpp_gc_root_lock(false);
int il2cpp_mono_gc_register_root(char* start, size_t size, MonoGCDescriptor descr, MonoGCRootSource source, const char* msg)
{
	il2cpp::gc::GarbageCollector::RegisterRoot(start, size);
	return 1;
}

void il2cpp_mono_gc_deregister_root(char* addr)
{
	il2cpp::gc::GarbageCollector::UnregisterRoot(addr);
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

MonoImage* il2cpp_mono_assembly_get_image(MonoAssembly* assembly)
{
	return (MonoImage*)il2cpp::vm::Assembly::GetImage((Il2CppAssembly*)assembly);
}

gboolean il2cpp_mono_runtime_try_shutdown()
{
	il2cpp::vm::Runtime::Shutdown();
	return TRUE;
}

gboolean il2cpp_mono_verifier_is_method_valid_generic_instantiation(MonoMethod* method)
{
	if (!method)
		return FALSE;

	if (!((MethodInfo*)method)->is_generic && ((MethodInfo*)method)->is_inflated && ((MethodInfo*)method)->methodPointer)
		return TRUE;

	return FALSE;
}

MonoType* il2cpp_mono_reflection_get_type_checked(MonoImage* rootimage, MonoImage* image, Il2CppMonoTypeNameParse* info, gboolean ignorecase, gboolean* type_resolve, MonoError* error)
{
	error_init(error);

	Il2CppClass *klass = il2cpp::vm::Image::FromTypeNameParseInfo((Il2CppImage*)image, *((il2cpp::vm::TypeNameParseInfo*)info->il2cppTypeNameParseInfo), ignorecase);
	if (!klass)
		return NULL;

	return (MonoType*)il2cpp::vm::Class::GetType(klass);
}

MonoReflectionAssemblyHandle il2cpp_mono_assembly_get_object_handle(MonoDomain* domain, MonoAssembly* assembly, MonoError* error)
{
	return (MonoReflectionAssemblyHandle)il2cpp::vm::Reflection::GetAssemblyObject((const Il2CppAssembly *)assembly);
}

MonoReflectionType* il2cpp_mono_type_get_object_checked(MonoDomain* domain, MonoType* type, MonoError* error)
{
	error_init(error);
	return (MonoReflectionType*)il2cpp::vm::Reflection::GetTypeObject((const Il2CppType*)type);
}

void il2cpp_mono_network_init()
{
}

gint il2cpp_mono_w32socket_set_blocking(void* sock, gboolean blocking)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return 0;
}

char* il2cpp_mono_get_runtime_build_info()
{
	return g_strdup_printf ("%s (%s)", "0.0", "IL2CPP");
}

MonoMethod* il2cpp_mono_marshal_method_from_wrapper(MonoMethod* wrapper)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

Il2CppMonoDebugOptions* il2cpp_mini_get_debug_options()
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

gpointer il2cpp_mono_jit_find_compiled_method_with_jit_info(MonoDomain* domain, MonoMethod* method, MonoJitInfo** ji)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return 0;
}

Il2CppMonoLMF** il2cpp_mono_get_lmf_addr()
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

void il2cpp_mono_set_lmf(Il2CppMonoLMF* lmf)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
}

gpointer il2cpp_mono_aot_get_method_checked(MonoDomain* domain, MonoMethod* method, MonoError* error)
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

MonoJitInfo* il2cpp_mini_jit_info_table_find(MonoDomain* domain, char* addr, MonoDomain** out_domain)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

void il2cpp_mono_restore_context(MonoContext* ctx)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
}

MonoMethod* il2cpp_mono_method_get_declaring_generic_method(MonoMethod* method)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

gboolean il2cpp_mono_error_ok (MonoError *error)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return 0;
}

MonoMethod* il2cpp_jinfo_get_method (MonoJitInfo *ji)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return NULL;
}

gboolean il2cpp_mono_find_prev_seq_point_for_native_offset (MonoDomain *domain, MonoMethod *method, gint32 native_offset, MonoSeqPointInfo **info, SeqPoint* seq_point)
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

gboolean il2cpp_mono_find_next_seq_point_for_native_offset (MonoDomain *domain, MonoMethod *method, gint32 native_offset, MonoSeqPointInfo **info, SeqPoint* seq_point)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return 0;
}

gboolean il2cpp_mono_class_has_parent (MonoClass *klass, MonoClass *parent)
{
	IL2CPP_ASSERT(0 && "This method is not yet implemented");
	return 0;
}

MonoGenericParam* il2cpp_mono_generic_container_get_param (MonoGenericContainer *gc, int i)
{
	return (MonoGenericParam*)il2cpp::vm::GenericContainer::GetGenericParameter((Il2CppGenericContainer*)gc, i);
}

gboolean il2cpp_mono_find_seq_point (MonoDomain *domain, MonoMethod *method, gint32 il_offset, MonoSeqPointInfo **info, SeqPoint *seq_point)
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

MonoSeqPointInfo* il2cpp_mono_get_seq_points (MonoDomain *domain, MonoMethod *method)
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

MonoException* il2cpp_mono_error_convert_to_exception (MonoError *target_error)
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

void* il2cpp_mono_gc_alloc_fixed (size_t size, void* descr, MonoGCRootSource source, void *key, const char *msg)
{
	return il2cpp_gc_alloc_fixed(size);
}

typedef void* (*Il2CppMonoGCLockedCallbackFunc) (void *data);
void* il2cpp_mono_gc_invoke_with_gc_lock (Il2CppMonoGCLockedCallbackFunc func, void *data)
{
	return il2cpp::gc::GarbageCollector::CallWithAllocLockHeld (func, data);
}

// These functions expose the IL2CPP VM C++ API to C

void* il2cpp_domain_get_agent_info(MonoAppDomain* domain)
{
	return ((Il2CppDomain*)domain)->agent_info;
}

void il2cpp_domain_set_agent_info(MonoAppDomain* domain, void* agentInfo)
{
	((Il2CppDomain*)domain)->agent_info = agentInfo;
}

MonoAssembly* il2cpp_domain_get_assemblies_iter(MonoAppDomain *domain, void* *iter)
{
	if (!iter)
		return NULL;

	il2cpp::vm::AssemblyVector* assemblies = il2cpp::vm::Assembly::GetAllAssemblies();

	if (!*iter)
	{
		il2cpp::vm::AssemblyVector::iterator *pIter = new il2cpp::vm::AssemblyVector::iterator();
		*pIter = assemblies->begin();
		*iter = pIter;
		return (MonoAssembly*)**pIter;
	}

	il2cpp::vm::AssemblyVector::iterator *pIter = (il2cpp::vm::AssemblyVector::iterator*)*iter;
	(*pIter)++;
	if (*pIter != assemblies->end())
	{
		return (MonoAssembly*)(**pIter);
	}
	else
	{
		delete pIter;
		*iter = NULL;
	}

	return NULL;
}

void il2cpp_start_debugger_thread()
{
#if IL2CPP_MONO_DEBUGGER
	il2cpp::utils::Debugger::StartDebuggerThread();
#endif
}

void* il2cpp_gc_alloc_fixed(size_t size)
{
	return il2cpp::gc::GarbageCollector::AllocateFixed(size, NULL);
}

void il2cpp_gc_free_fixed(void* address)
{
	il2cpp::gc::GarbageCollector::FreeFixed(address);
}

const char* il2cpp_domain_get_name(MonoDomain* domain)
{
	return ((Il2CppDomain*)domain)->friendly_name;
}

Il2CppSequencePoint* il2cpp_get_sequence_points(void* *iter)
{
#if IL2CPP_MONO_DEBUGGER
	return (Il2CppSequencePoint*)il2cpp::utils::Debugger::GetSequencePoints(iter);
#else
    return NULL;
#endif
}

Il2CppSequencePoint* il2cpp_get_method_sequence_points(MonoMethod* method, void* *iter)
{
#if IL2CPP_MONO_DEBUGGER
	if (!method)
		return (Il2CppSequencePoint*)il2cpp::utils::Debugger::GetSequencePoints(iter);
	else
		return (Il2CppSequencePoint*)il2cpp::utils::Debugger::GetSequencePoints((const MethodInfo*)method, iter);
#else
    return NULL;
#endif
}

gboolean il2cpp_mono_methods_match(MonoMethod* left, MonoMethod* right)
{
	MethodInfo* leftMethod = (MethodInfo*)left;
	MethodInfo* rightMethod = (MethodInfo*)right;

	if (rightMethod == leftMethod)
		return TRUE;
	if (rightMethod == NULL || leftMethod == NULL)
		return FALSE;
	if (rightMethod->is_inflated && !rightMethod->is_generic && rightMethod->genericMethod->methodDefinition == leftMethod)
		return TRUE;
	if (leftMethod->is_inflated && !leftMethod->is_generic && leftMethod->genericMethod->methodDefinition == rightMethod)
		return TRUE;
    if (leftMethod->is_generic && rightMethod->is_inflated && rightMethod->methodPointer &&
        leftMethod->klass == rightMethod->klass &&
        strcmp(leftMethod->name, rightMethod->name) == 0)
    {
        if (leftMethod->parameters_count != rightMethod->parameters_count)
            return FALSE;

        for(int i = 0;i < leftMethod->parameters_count;++i)
        {
			if ((leftMethod->parameters[i].parameter_type->type != IL2CPP_TYPE_MVAR) && (leftMethod->parameters[i].parameter_type->type != IL2CPP_TYPE_VAR) && (leftMethod->parameters[i].parameter_type != rightMethod->parameters[i].parameter_type))
				return FALSE;
        }

        return TRUE;
    }

	return FALSE;
}

MonoClass* il2cpp_class_get_nested_types_accepts_generic(MonoClass *monoClass, void* *iter)
{
	Il2CppClass *klass = (Il2CppClass*)monoClass;
	if (klass->generic_class)
		return NULL;

	return (MonoClass*)il2cpp::vm::Class::GetNestedTypes(klass, iter);
}

MonoClass* il2cpp_defaults_object_class()
{
	return (MonoClass*)il2cpp_defaults.object_class;
}

guint8 il2cpp_array_rank(MonoArray *monoArr)
{
	Il2CppArray *arr = (Il2CppArray*)monoArr;
	return arr->klass->rank;
}

const char* il2cpp_image_name(MonoImage *monoImage)
{
	Il2CppImage *image = (Il2CppImage*)monoImage;
	return image->name;
}

guint8* il2cpp_field_get_address(MonoObject *obj, MonoClassField *monoField)
{
	FieldInfo *field = (FieldInfo*)monoField;
	return (guint8*)obj + field->offset;
}

MonoType* il2cpp_mono_object_get_type(MonoObject* object)
{
    return (MonoType*)&(((Il2CppObject*)object)->klass->byval_arg);
}

MonoClass* il2cpp_defaults_exception_class()
{
	return (MonoClass*)il2cpp_defaults.exception_class;
}

MonoImage* il2cpp_defaults_corlib_image()
{
	return (MonoImage*)il2cpp_defaults.corlib;
}

bool il2cpp_method_is_string_ctor(const MonoMethod * method)
{
	MethodInfo* methodInfo = (MethodInfo*)method;
	return methodInfo->klass == il2cpp_defaults.string_class && !strcmp (methodInfo->name, ".ctor");
}

MonoClass* il2cpp_defaults_void_class()
{
	return (MonoClass*)il2cpp_defaults.void_class;
}

void il2cpp_set_var(guint8* newValue, void *value, MonoType *localVariableTypeMono)
{
	il2cpp::metadata::SizeAndAlignment sa = il2cpp::metadata::FieldLayout::GetTypeSizeAndAlignment((const Il2CppType*)localVariableTypeMono);
	if (((Il2CppType*)localVariableTypeMono)->byref)
		memcpy(*(void**)value, newValue, sa.size);
	else
		memcpy(value, newValue, sa.size);
}

MonoMethod* il2cpp_get_interface_method(MonoClass* klass, MonoClass* itf, int slot)
{
	const VirtualInvokeData* data = il2cpp::vm::Class::GetInterfaceInvokeDataFromVTable((Il2CppClass*)klass, (Il2CppClass*)itf, slot);
	if (!data)
		return NULL;

	return (MonoMethod*)data->method;
}

gboolean il2cpp_field_is_deleted(MonoClassField *field)
{
	return il2cpp::vm::Field::IsDeleted((FieldInfo*)field);
}

struct TypeIterState
{
	il2cpp::vm::AssemblyVector* assemblies;
	il2cpp::vm::AssemblyVector::iterator assembly;
	Il2CppImage* image;
	il2cpp::vm::TypeVector types;
	il2cpp::vm::TypeVector::iterator type;
};

MonoClass* il2cpp_iterate_loaded_classes(void* *iter)
{
	if (!iter)
		return NULL;

	if (!*iter)
	{
		TypeIterState *state = new TypeIterState();
		state->assemblies = il2cpp::vm::Assembly::GetAllAssemblies();
		state->assembly = state->assemblies->begin();
		state->image = il2cpp::vm::Assembly::GetImage(*state->assembly);
		il2cpp::vm::Image::GetTypes(state->image, true, &state->types);
		state->type = state->types.begin();
		*iter = state;	
		return (MonoClass*)*state->type;
	}

	TypeIterState *state = (TypeIterState*)*iter;
	
	state->type++;
	if (state->type == state->types.end())
	{
		state->assembly++;
		if (state->assembly == state->assemblies->end())
		{
			delete state;
			*iter = NULL;
			return NULL;
		}

		state->image = il2cpp::vm::Assembly::GetImage(*state->assembly);
		il2cpp::vm::Image::GetTypes(state->image, true, &state->types);
		state->type = state->types.begin();
	}

	return (MonoClass*)*state->type;
}

const char** il2cpp_get_source_files_for_type(MonoClass *klass, int *count)
{
#if IL2CPP_MONO_DEBUGGER
	return il2cpp::utils::Debugger::GetTypeSourceFiles((Il2CppClass*)klass, *count);
#else
    return NULL;
#endif
}

MonoMethod* il2cpp_method_get_generic_definition(MonoMethodInflated *imethod)
{
	MethodInfo *method = (MethodInfo*)imethod;

	if (!method->is_inflated || method->is_generic)
		return NULL;

	return (MonoMethod*)((MethodInfo*)imethod)->genericMethod->methodDefinition;
}


MonoGenericInst* il2cpp_method_get_generic_class_inst(MonoMethodInflated *imethod)
{
	MethodInfo *method = (MethodInfo*)imethod;

	if (!method->is_inflated || method->is_generic)
		return NULL;

	return (MonoGenericInst*)method->genericMethod->context.class_inst;
}

MonoClass* il2cpp_generic_class_get_container_class(MonoGenericClass *gclass)
{
	return (MonoClass*)il2cpp::vm::GenericClass::GetTypeDefinition((Il2CppGenericClass*)gclass);
}


MonoClass* il2cpp_mono_get_string_class (void)
{
	return (MonoClass*)il2cpp_defaults.string_class;
}

Il2CppSequencePoint* il2cpp_get_sequence_point(int id)
{
#if IL2CPP_MONO_DEBUGGER
    return il2cpp::utils::Debugger::GetSequencePoint(id);
#else
    return NULL;
#endif
}
    
char* il2cpp_assembly_get_full_name(MonoAssembly *assembly)
{
    std::string s = il2cpp::vm::AssemblyName::AssemblyNameToString(assembly->aname);
    return g_strdup(s.c_str());
}

const MonoMethod* il2cpp_get_seq_point_method(Il2CppSequencePoint *seqPoint)
{
#if IL2CPP_MONO_DEBUGGER
    return il2cpp::utils::Debugger::GetSequencePointMethod(seqPoint);
#else
    return NULL;
#endif
}

const MonoClass* il2cpp_get_class_from_index(int index)
{
    if (index < 0)
        return NULL;

    return il2cpp::vm::MetadataCache::GetTypeInfoFromTypeIndex(index);
}

const MonoType* il2cpp_get_type_from_index(int index)
{
    return il2cpp::vm::MetadataCache::GetIl2CppTypeFromIndex(index);
}

}
#endif // RUNTIME_IL2CPP
