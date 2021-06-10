#include <config.h>


#include <mono/utils/mono-publib.h>

/* allow Unity to use deprecated functions for now */
#ifdef MONO_RT_EXTERNAL_ONLY
#undef MONO_RT_EXTERNAL_ONLY
#define MONO_RT_EXTERNAL_ONLY
#endif
#include <mono/metadata/unity-utils.h>
#include <stdio.h>
#include <stdlib.h>
#ifdef WIN32
#include <fcntl.h>
#endif
#include <mono/metadata/exception.h>
#include <mono/metadata/object.h>
#include <mono/metadata/metadata.h>
#include <mono/metadata/tabledefs.h>
#include <mono/metadata/class-internals.h>
#include <mono/metadata/class-init.h>
#include <mono/metadata/object-internals.h>
#include <mono/metadata/marshal.h>
#include <mono/metadata/metadata-internals.h>
#include <mono/metadata/profiler-private.h>
#include <mono/metadata/profiler.h>
#include <mono/metadata/reflection-internals.h>
#include <mono/metadata/threads.h>
#include <mono/metadata/threadpool.h>
#include <mono/metadata/tokentype.h>
#include <mono/utils/mono-string.h>

#if HAVE_BOEHM_GC
#include <mono/utils/gc_wrapper.h>
#include <mono/metadata/gc-internals.h>
#endif

#include <glib.h>

#ifdef WIN32
#define UTF8_2_WIDE(src,dst) MultiByteToWideChar( CP_UTF8, 0, src, -1, dst, MAX_PATH )
#endif

#undef exit

void unity_mono_exit( int code )
{
	//fprintf( stderr, "mono: exit called, code %d\n", code );
	exit( code );
}


GString* gEmbeddingHostName = 0;


MONO_API void mono_unity_set_embeddinghostname(const char* name)
{
	gEmbeddingHostName = g_string_new(name);
}



MonoString* mono_unity_get_embeddinghostname()
{
	if (gEmbeddingHostName == 0)
		mono_unity_set_embeddinghostname("mono");
	return mono_string_new_wrapper(gEmbeddingHostName->str);
}

static gboolean socket_security_enabled = FALSE;

gboolean
mono_unity_socket_security_enabled_get ()
{
	return socket_security_enabled;
}

void
mono_unity_socket_security_enabled_set (gboolean enabled)
{
	socket_security_enabled = enabled;
}

void mono_unity_set_vprintf_func (vprintf_func func)
{
	set_vprintf_func (func);
}

MONO_API gboolean
mono_unity_class_is_interface (MonoClass* klass)
{
	return MONO_CLASS_IS_INTERFACE(klass);
}

MONO_API gboolean
mono_unity_class_is_abstract (MonoClass* klass)
{
	return (mono_class_get_flags (klass) & TYPE_ATTRIBUTE_ABSTRACT);
}

// classes_ref is a preallocated array of *length_ref MonoClass*
// returned classes are stored in classes_ref, number of stored classes is stored in length_ref
// return value is number of classes found (which may be greater than number of classes stored)
unsigned mono_unity_get_all_classes_with_name_case (MonoImage *image, const char *name, MonoClass **classes_ref, unsigned *length_ref)
{
	MonoClass *klass;
	MonoTableInfo *tdef = &image->tables [MONO_TABLE_TYPEDEF];
	int i, count;
	guint32 attrs, visibility;
	unsigned length = 0;

	/* (yoinked from icall.c) we start the count from 1 because we skip the special type <Module> */
	for (i = 1; i < tdef->rows; ++i)
	{
		klass = mono_class_get (image, (i + 1) | MONO_TOKEN_TYPE_DEF);
		if (klass && klass->name && 0 == mono_utf8_strcasecmp (klass->name, name))
		{
			if (length < *length_ref)
				classes_ref[length] = klass;
			++length;
		}
	}

	if (length < *length_ref)
		*length_ref = length;
	return length;
}

gboolean
unity_mono_method_is_generic (MonoMethod* method)
{
	return method->is_generic;
}

MONO_API MonoMethod*
unity_mono_reflection_method_get_method(MonoReflectionMethod* mrf)
{
	if(!mrf)
		return NULL;

	return mrf->method;
}

MONO_API void
mono_unity_g_free(void *ptr)
{
	g_free (ptr);
}


MONO_API MonoClass*
mono_custom_attrs_get_attrs (MonoCustomAttrInfo *ainfo, gpointer *iter)
{
	int index = -1;
	if (!iter)
		return NULL;
	if (!*iter)
	{
		*iter = GINT_TO_POINTER (1);
		return ainfo->attrs[0].ctor->klass;
	}

	index = GPOINTER_TO_INT (*iter);
	if (index >= ainfo->num_attrs)
		return NULL;
	*iter = GINT_TO_POINTER (index + 1);
	return ainfo->attrs[index].ctor->klass;
}

MONO_API MonoArray*
mono_unity_custom_attrs_construct (MonoCustomAttrInfo *cinfo, MonoError *error)
{
	HANDLE_FUNCTION_ENTER ();
	MonoArrayHandle result = mono_custom_attrs_construct_by_type (cinfo, NULL, error);
	HANDLE_FUNCTION_RETURN_OBJ (result);
}

MONO_API gboolean
mono_class_is_inflated (MonoClass *klass)
{
	g_assert(klass);
	return (klass->class_kind == MONO_CLASS_GINST);
}

MONO_API void
mono_thread_pool_cleanup (void)
{
	// TODO_UNITY : I am not sure we need to call this anymore
	mono_threadpool_cleanup ();
}

MONO_API void*
mono_class_get_userdata (MonoClass* klass)
{
	return klass->unity_user_data;
}

MONO_API void
mono_class_set_userdata(MonoClass* klass, void* userdata)
{
	klass->unity_user_data = userdata;
}

MONO_API int
mono_class_get_userdata_offset()
{
	return offsetof(struct _MonoClass, unity_user_data);
}


static UnityFindPluginCallback unity_find_plugin_callback;

MONO_API void
mono_set_find_plugin_callback (UnityFindPluginCallback find)
{
	unity_find_plugin_callback = find;
}

MONO_API UnityFindPluginCallback
mono_get_find_plugin_callback ()
{
	return unity_find_plugin_callback;
}

//object

void mono_unity_object_init(void* obj, MonoClass* klass)
{
	if (klass->valuetype)
		memset(obj, 0, klass->instance_size - sizeof(MonoObject));
	else
		*(MonoObject**)obj = NULL;
}

MonoObject* mono_unity_object_isinst_sealed(MonoObject* obj, MonoClass* targetType)
{
	return obj->vtable->klass == targetType ? obj : NULL;
}

void mono_unity_object_unbox_nullable(MonoObject* obj, MonoClass* nullableArgumentClass, void* storage)
{
	uint32_t valueSize = nullableArgumentClass->instance_size - sizeof(MonoObject);

	if (obj == NULL)
	{
		*((mono_byte*)(storage)+valueSize) = 0;
	}
	else if (obj->vtable->klass != nullableArgumentClass)
	{
		mono_raise_exception_deprecated (mono_get_exception_invalid_cast());
	}
	else
	{
		memcpy(storage, mono_object_unbox(obj), valueSize);
		*((mono_byte*)(storage)+valueSize) = 1;
	}
}

MonoClass* mono_unity_object_get_class(MonoObject *obj)
{
	return obj->vtable->klass;
}

// MonoObject* mono_unity_object_compare_exchange(MonoObject **location, MonoObject *value, MonoObject *comparand)
// {
// 	return ves_icall_System_Threading_Interlocked_CompareExchange_T(location, value, comparand);
// }

// MonoObject* mono_unity_object_exchange(MonoObject **location, MonoObject *value)
// {
// 	return ves_icall_System_Threading_Interlocked_Exchange_T(location, value);
// }

gboolean mono_unity_object_check_box_cast(MonoObject *obj, MonoClass *klass)
{
	return (obj->vtable->klass->element_class == klass->element_class);
}

//class

const char* mono_unity_class_get_image_name(MonoClass* klass)
{
	return klass->image->assembly_name;
}

MonoClass* mono_unity_class_get_generic_definition(MonoClass* klass)
{
	MonoGenericClass* generic_class = mono_class_try_get_generic_class (klass);
	if (generic_class)
		return generic_class->container_class;

	return NULL;
}

MonoClass* mono_unity_class_inflate_generic_class(MonoClass *gklass, MonoGenericContext *context)
{
	MonoError error;
	MonoClass* klass;
	klass = mono_class_inflate_generic_class_checked(gklass, context, &error);
	mono_error_cleanup (&error);
	return klass;
}

gboolean mono_unity_class_has_parent_unsafe(MonoClass *klass, MonoClass *parent)
{
	return mono_class_has_parent_fast(klass, parent);
}

MonoAssembly* mono_unity_class_get_assembly(MonoClass *klass)
{
	return klass->image->assembly;
}

gboolean mono_unity_class_is_array(MonoClass *klass)
{
	return klass->rank > 0;
}

MonoClass* mono_unity_class_get_element_class(MonoClass *klass)
{
	return klass->element_class;
}

gboolean mono_unity_class_is_delegate(MonoClass *klass)
{
	return klass->delegate;
}

int mono_unity_class_get_instance_size(MonoClass *klass)
{
	return klass->instance_size;
}

MonoClass* mono_unity_class_get_castclass(MonoClass *klass)
{
	return klass->cast_class;
}

guint32 mono_unity_class_get_native_size(MonoClass* klass)
{
	MonoMarshalType* info = mono_marshal_load_type_info(klass);
	return info->native_size;
}

MonoBoolean mono_unity_class_is_string(MonoClass* klass)
{
	if (mono_class_get_type(klass)->type == MONO_TYPE_STRING)
		return TRUE;
	return FALSE;
}

MonoBoolean mono_unity_class_is_class_type(MonoClass* klass)
{
	if (mono_class_get_type(klass)->type == MONO_TYPE_CLASS)
		return TRUE;
	return FALSE;
}

MONO_API gboolean
mono_class_is_generic(MonoClass *klass)
{
	g_assert(klass);
	return (klass->class_kind == MONO_CLASS_GTD);
}

MONO_API gboolean
mono_class_is_blittable(MonoClass *klass)
{
	g_assert(klass);
	return klass->blittable;
}

gboolean mono_unity_class_has_cctor(MonoClass *klass)
{
    return klass->has_cctor ? TRUE : FALSE;
}

//method

MonoMethod* mono_unity_method_get_generic_definition(MonoMethod* method)
{
	if (method->is_inflated)
		return ((MonoMethodInflated*)method)->declaring;

	return NULL;
}

MonoReflectionMethod* mono_unity_method_get_object(MonoMethod *method)
{
	MonoError unused;
	return mono_method_get_object_checked(mono_domain_get(), method, NULL, &unused);
}

MonoMethod* mono_unity_method_alloc0(MonoClass* klass)
{
	return mono_image_alloc0(klass->image, sizeof(MonoMethod));
}

MonoMethod* mono_unity_method_delegate_invoke_wrapper(MonoClass* klass)
{
	MonoMethod* method = (MonoMethod*)mono_image_alloc0(klass->image, sizeof(MonoMethod));
	MonoMethod *invoke = mono_get_delegate_invoke (klass);
	method->signature = mono_metadata_signature_dup_full (klass->image, mono_method_signature (invoke));
	return method;
}

gboolean mono_unity_method_is_static(MonoMethod *method)
{
	return method->flags & METHOD_ATTRIBUTE_STATIC;
}

MonoClass* mono_unity_method_get_class(const MonoMethod *method)
{
	return method->klass;
}

#ifdef IL2CPP_ON_MONO

void* mono_unity_method_get_method_pointer(MonoMethod* method)
{
	return method->method_pointer;
}

void mono_unity_method_set_method_pointer(MonoMethod* method, void *p)
{
	method->method_pointer = p;
}

void* mono_unity_method_get_invoke_pointer(MonoMethod* method)
{
	return method->invoke_pointer;
}

void mono_unity_method_set_invoke_pointer(MonoMethod* method, void *p)
{
	method->invoke_pointer = p;
}

#endif

const char* mono_unity_method_get_name(const MonoMethod *method)
{
	return method->name;
}


//must match the hash in il2cpp code generation
static guint32 hash_string_djb2(guchar *str)
{
	guint32 hash = 5381;
	int c;

	while (c = *str++)
		hash = ((hash << 5) + hash) + c; /* hash * 33 + c */

	return hash;
}

static guint32 get_array_structure_hash(MonoArrayType *atype)
{
	char buffer[100];
	char *ptr = buffer;

	*ptr++ = '[';

	char numbuffer[10];

	for (int i = 0; i < atype->rank; ++i)
	{
		if (atype->numlobounds > 0 && atype->lobounds[i] != 0)
		{
			snprintf(numbuffer, 10, "%d", atype->lobounds[i]);
			char *ptrnum = numbuffer;
			while (*ptrnum)
				*ptr++ = *ptrnum++;

			*ptr++ = ':';
		}

		if (atype->numsizes > 0 && atype->sizes[i] != 0)
		{
			snprintf(numbuffer, 10, "%d", atype->sizes[i]);
			char *ptrnum = numbuffer;
			while (*ptrnum)
				*ptr++ = *ptrnum++;
		}

		if (i < atype->rank - 1)
			*ptr++ = ',';
	}

	*ptr++ = ']';
	*ptr++ = 0;

	return hash_string_djb2(buffer);
}

/* Begin: Hash computation helper functions */

static void get_type_hashes(MonoType *type, GList *hashes, gboolean inflate);
static void get_type_hashes_generic_inst(MonoGenericInst *inst, GList *hashes, gboolean inflate);


static void get_type_hashes_generic_inst(MonoGenericInst *inst, GList *hashes, gboolean inflate)
{
	for (int i = 0; i < inst->type_argc; ++i)
	{
		MonoType *type = inst->type_argv[i];
		get_type_hashes(type, hashes, inflate);
	}
}

static void get_type_hashes(MonoType *type, GList *hashes, gboolean inflate)
{
	if (type->type != MONO_TYPE_GENERICINST)
	{
		MonoClass *klass = NULL;

		switch (type->type)
		{
		case MONO_TYPE_ARRAY:
		{
			MonoArrayType *atype = type->data.array;
			g_list_append(hashes, GUINT_TO_POINTER(MONO_TOKEN_TYPE_SPEC));
			g_list_append(hashes, GUINT_TO_POINTER(get_array_structure_hash(atype)));
			get_type_hashes(&(atype->eklass->this_arg), hashes, inflate);
			break;
		}
		case MONO_TYPE_CLASS:
		case MONO_TYPE_VALUETYPE:
			klass = type->data.klass;
			break;
		case MONO_TYPE_BOOLEAN:
			klass = mono_defaults.boolean_class;
			break;
		case MONO_TYPE_CHAR:
			klass = mono_defaults.char_class;
			break;
		case MONO_TYPE_I:
			klass = mono_defaults.int_class;
			break;
		case MONO_TYPE_U:
			klass = mono_defaults.uint_class;
			break;
		case MONO_TYPE_I1:
			klass = mono_defaults.sbyte_class;
			break;
		case MONO_TYPE_U1:
			klass = mono_defaults.byte_class;
			break;
		case MONO_TYPE_I2:
			klass = mono_defaults.int16_class;
			break;
		case MONO_TYPE_U2:
			klass = mono_defaults.uint16_class;
			break;
		case MONO_TYPE_I4:
			klass = mono_defaults.int32_class;
			break;
		case MONO_TYPE_U4:
			klass = mono_defaults.uint32_class;
			break;
		case MONO_TYPE_I8:
			klass = mono_defaults.int64_class;
			break;
		case MONO_TYPE_U8:
			klass = mono_defaults.uint64_class;
			break;
		case MONO_TYPE_R4:
			klass = mono_defaults.single_class;
			break;
		case MONO_TYPE_R8:
			klass = mono_defaults.double_class;
			break;
		case MONO_TYPE_STRING:
			klass = mono_defaults.string_class;
			break;
		case MONO_TYPE_OBJECT:
			klass = mono_defaults.object_class;
			break;
		}

		if (klass)
		{
			g_list_append(hashes, GUINT_TO_POINTER(klass->type_token));
			g_list_append(hashes, GUINT_TO_POINTER(hash_string_djb2(klass->image->module_name)));
		}

		return;
	}
	else
	{
		g_list_append(hashes, GUINT_TO_POINTER(type->data.generic_class->container_class->type_token));
		g_list_append(hashes, GUINT_TO_POINTER(hash_string_djb2(type->data.generic_class->container_class->image->module_name)));

        if (inflate)
		    get_type_hashes_generic_inst(type->data.generic_class->context.class_inst, hashes, inflate);
	}

}

static GList* get_type_hashes_method(MonoMethod *method, gboolean inflate)
{
	GList *hashes = monoeg_g_list_alloc();

	hashes->data = GUINT_TO_POINTER(method->token);
	g_list_append(hashes, GUINT_TO_POINTER(hash_string_djb2(method->klass->image->module_name)));

	if (inflate && method->klass->class_kind == MONO_CLASS_GINST)
	{
		g_list_append(hashes, GUINT_TO_POINTER(method->klass->type_token));
		get_type_hashes_generic_inst(mono_class_get_generic_class (method->klass)->context.class_inst, hashes, inflate);
	}

	if (inflate && method->is_inflated)
	{
		MonoGenericContext* methodGenericContext = mono_method_get_context(method);
		if (methodGenericContext->method_inst != NULL)
			get_type_hashes_generic_inst(methodGenericContext->method_inst, hashes, inflate);
	}

	return hashes;
}

//hash combination function must match the one used in IL2CPP codegen
static guint64 combine_hashes(guint64 hash1, guint64 hash2)
{
	const guint64 seed = 486187739;
	return hash1 * seed + hash2;
}

static void combine_all_hashes(gpointer data, gpointer user_data)
{
	guint64 *hash = (guint64*)user_data;
	if (*hash == 0)
		*hash = (guint64)data;
	else
		*hash = combine_hashes(*hash, (guint64)(uintptr_t)data);
}

/* End: Hash computation helper functions */

guint64 mono_unity_method_get_hash(MonoMethod *method, gboolean inflate)
{
	GList *hashes = get_type_hashes_method(method, inflate);

	guint64 hash = 0;

	g_list_first(hashes);
	g_list_foreach(hashes, combine_all_hashes, &hash);
	g_list_free(hashes);

	return hash;
}

guint64 mono_unity_type_get_hash(MonoType *type, gboolean inflate)
{
    GList *hashes = monoeg_g_list_alloc();

    get_type_hashes(type, hashes, inflate);
    
    guint64 hash = 0;

    g_list_first(hashes);
    g_list_foreach(hashes, combine_all_hashes, &hash);
    g_list_free(hashes);

    return hash;
}


MonoMethod* mono_unity_method_get_aot_array_helper_from_wrapper(MonoMethod *method)
{
	MonoMethod *m;
	const char *prefix;
	MonoGenericContext ctx;
	MonoType *args[16];
	char *mname, *iname, *s, *s2, *helper_name = NULL;

	prefix = "System.Collections.Generic";
	s = g_strdup_printf("%s", method->name + strlen(prefix) + 1);
	s2 = strstr(s, "`1.");
	g_assert(s2);
	s2[0] = '\0';
	iname = s;
	mname = s2 + 3;

	//printf ("X: %s %s\n", iname, mname);

	if (!strcmp(iname, "IList"))
		helper_name = g_strdup_printf("InternalArray__%s", mname);
	else
		helper_name = g_strdup_printf("InternalArray__%s_%s", iname, mname);
	m = mono_class_get_method_from_name(mono_defaults.array_class, helper_name, mono_method_signature(method)->param_count);
	g_assert(m);
	g_free(helper_name);
	g_free(s);

	if (m->is_generic) {
		MonoError error;
		memset(&ctx, 0, sizeof(ctx));
		args[0] = &method->klass->element_class->_byval_arg;
		ctx.method_inst = mono_metadata_get_generic_inst(1, args);
		m = mono_class_inflate_generic_method_checked(m, &ctx, &error);
		g_assert(is_ok(&error)); /* FIXME don't swallow the error */
	}

	return m;
}

MonoObject* mono_unity_method_convert_return_type_if_needed(MonoMethod *method, void *value)
{
	if (method->signature && method->signature->ret->type == MONO_TYPE_PTR)
	{
		MonoError unused;
		return mono_value_box_checked(mono_domain_get(), mono_defaults.int_class, &value, &unused);
	}

	return (MonoObject*)value;
}

MONO_API gboolean
unity_mono_method_is_inflated(MonoMethod* method)
{
	return method->is_inflated;
}

guint32 mono_unity_method_get_token(MonoMethod *method)
{
	return method->token;
}

//domain


void mono_unity_domain_install_finalize_runtime_invoke(MonoDomain* domain, RuntimeInvokeFunction callback)
{
	domain->finalize_runtime_invoke = callback;
}

void mono_unity_domain_install_capture_context_runtime_invoke(MonoDomain* domain, RuntimeInvokeFunction callback)
{
	domain->capture_context_runtime_invoke = callback;
}

void mono_unity_domain_install_capture_context_method(MonoDomain* domain, gpointer callback)
{
	domain->capture_context_method = callback;
}


void mono_unity_domain_unload (MonoDomain* domain, MonoUnityExceptionFunc callback)
{
	MonoObject *exc = NULL;
	mono_domain_try_unload (domain, &exc, callback);
}

//array

int mono_unity_array_get_element_size(MonoArray *arr)
{
	return arr->obj.vtable->klass->sizes.element_size;
}

MonoClass* mono_unity_array_get_class(MonoArray *arr)
{
	return arr->obj.vtable->klass;
}

mono_array_size_t mono_unity_array_get_max_length(MonoArray *arr)
{
	return arr->max_length;
}

//type

gboolean mono_unity_type_is_generic_instance(MonoType *type)
{
	return type->type == MONO_TYPE_GENERICINST;
}

MonoGenericClass* mono_unity_type_get_generic_class(MonoType *type)
{
	if (type->type != MONO_TYPE_GENERICINST)
		return NULL;

	return type->data.generic_class;
}

gboolean mono_unity_type_is_enum_type(MonoType *type)
{
	if (type->type == MONO_TYPE_VALUETYPE && type->data.klass->enumtype)
		return TRUE;
	if (type->type == MONO_TYPE_GENERICINST && type->data.generic_class->container_class->enumtype)
		return TRUE;
	return FALSE;
}

gboolean mono_unity_type_is_boolean(MonoType *type)
{
	return type->type == MONO_TYPE_BOOLEAN;
}

MonoClass* mono_unity_type_get_element_class(MonoType *type)
{
	return type->data.klass->element_class;
}

//generic class

MonoGenericContext mono_unity_generic_class_get_context(MonoGenericClass *klass)
{
	return klass->context;
}

MonoClass* mono_unity_generic_class_get_container_class(MonoGenericClass *klass)
{
	return klass->container_class;
}

//method signature

MonoClass* mono_unity_signature_get_class_for_param(MonoMethodSignature *sig, int index)
{
	MonoType *type = sig->params[index];
	return mono_class_from_mono_type(type);
}

int mono_unity_signature_num_parameters(MonoMethodSignature *sig)
{
	return sig->param_count;
}

gboolean mono_unity_signature_param_is_byref(MonoMethodSignature *sig, int index)
{
	return sig->params[index]->byref;
}

//generic inst

guint mono_unity_generic_inst_get_type_argc(MonoGenericInst *inst)
{
	return inst->type_argc;
}

MonoType* mono_unity_generic_inst_get_type_argument(MonoGenericInst *inst, int index)
{
	return inst->type_argv[index];
}

//exception

MonoString* mono_unity_exception_get_message(MonoException *exc)
{
	return exc->message;
}

MonoString* mono_unity_exception_get_stack_trace(MonoException *exc)
{
	return exc->stack_trace;
}

MonoObject* mono_unity_exception_get_inner_exception(MonoException *exc)
{
	return exc->inner_ex;
}

MonoArray* mono_unity_exception_get_trace_ips(MonoException *exc)
{
	return exc->trace_ips;
}

void mono_unity_exception_set_trace_ips(MonoException *exc, MonoArray *ips)
{
	g_assert(sizeof((exc)->trace_ips) == sizeof(void**));
	mono_gc_wbarrier_set_field((MonoObject*)(exc), &((exc)->trace_ips), (MonoObject*)ips);
}

MonoException* mono_unity_exception_get_marshal_directive(const char* msg)
{
	return mono_exception_from_name_msg(mono_get_corlib(), "System.Runtime.InteropServices", "MarshalDirectiveException", msg);
}

MonoException* mono_unity_error_convert_to_exception (MonoError *error)
{
	return mono_error_convert_to_exception (error);
}

//defaults

MonoClass* mono_unity_defaults_get_int_class()
{
	return mono_defaults.int_class;
}

MonoClass* mono_unity_defaults_get_stack_frame_class()
{
	return mono_defaults.stack_frame_class;
}

MonoClass* mono_unity_defaults_get_int32_class()
{
	return mono_defaults.int32_class;
}

MonoClass* mono_unity_defaults_get_char_class()
{
	return mono_defaults.char_class;
}

MonoClass* mono_unity_defaults_get_delegate_class()
{
	return mono_defaults.delegate_class;
}

MonoClass* mono_unity_defaults_get_byte_class()
{
	return mono_defaults.byte_class;
}

//unitytls

static unitytls_interface_struct* gUnitytlsInterface = NULL;

MONO_API unitytls_interface_struct* 
mono_unity_get_unitytls_interface()
{
	return gUnitytlsInterface;
}

// gc
MONO_API void mono_unity_gc_set_mode(MonoGCMode mode)
{
	switch (mode)
	{
#if HAVE_BOEHM_GC
		case MONO_GC_MODE_ENABLED:
			if (GC_is_disabled())
				GC_enable();
			GC_set_disable_automatic_collection(FALSE);
			break;

		case MONO_GC_MODE_DISABLED:
			if (!GC_is_disabled())
				GC_disable();
			break;

		case MONO_GC_MODE_MANUAL:
			if (GC_is_disabled())
				GC_enable();
			GC_set_disable_automatic_collection(TRUE);
			break;
#else
		g_assert_not_reached();
#endif
	}
}

// Deprecated. Remove when Unity has switched to mono_unity_gc_set_mode
MONO_API void mono_unity_gc_enable()
{
#if HAVE_BOEHM_GC
	GC_enable();
#else
	g_assert_not_reached ();
#endif
}

// Deprecated. Remove when Unity has switched to mono_unity_gc_set_mode
MONO_API void mono_unity_gc_disable()
{
#if HAVE_BOEHM_GC
	GC_disable();
#else
	g_assert_not_reached ();
#endif
}

// Deprecated. Remove when Unity has switched to mono_unity_gc_set_mode
MONO_API int mono_unity_gc_is_disabled()
{
#if HAVE_BOEHM_GC
	return GC_is_disabled ();
#else
	g_assert_not_reached ();
	return 0;
#endif	
}

// Logging
static UnityLogErrorCallback editorLoggingCallback;
MONO_API void mono_unity_set_editor_logging_callback(UnityLogErrorCallback callback)
{
	editorLoggingCallback = callback;
}

gboolean mono_unity_log_error_to_editor(const char *message)
{
	if (editorLoggingCallback)
	{
		editorLoggingCallback(message);
		return TRUE;
	}
	return FALSE;
}

MONO_API void 
mono_unity_install_unitytls_interface(unitytls_interface_struct* callbacks)
{
	gUnitytlsInterface = callbacks;
}

//misc

MonoAssembly* mono_unity_assembly_get_mscorlib()
{
	return mono_defaults.corlib->assembly;
}

MonoImage* mono_unity_image_get_mscorlib()
{
	return mono_defaults.corlib->assembly->image;
}

MonoClass* mono_unity_generic_container_get_parameter_class(MonoGenericContainer* generic_container, gint index)
{
	MonoGenericParam *param = mono_generic_container_get_param(generic_container, index);
	return mono_class_create_generic_parameter(param);
}

MonoString* mono_unity_string_append_assembly_name_if_necessary(MonoString* typeName, const char* assemblyName)
{
	if (typeName != NULL)
	{
		MonoTypeNameParse info;

		// The mono_reflection_parse_type function will mangle the name, so don't use this copy later.
		MonoError unused;
		char* nameForParsing = mono_string_to_utf8_checked(typeName, &unused);
		if (mono_reflection_parse_type(nameForParsing, &info))
		{
			if (!info.assembly.name)
			{
				GString* assemblyQualifiedName = g_string_new(0);
				char* name = mono_string_to_utf8_checked(typeName, &unused);
				g_string_append_printf(assemblyQualifiedName, "%s, %s", name, assemblyName);

				typeName = mono_string_new(mono_domain_get(), assemblyQualifiedName->str);

				g_string_free(assemblyQualifiedName, FALSE);
				mono_free(name);
			}
		}

		mono_free(nameForParsing);
	}

	return typeName;
}

void mono_unity_memory_barrier()
{
	mono_memory_barrier();
}

MonoObject* mono_unity_delegate_get_target(MonoDelegate *delegate)
{
	return delegate->target;
}

gchar* mono_unity_get_runtime_build_info(const char *date, const char *time)
{
	return g_strdup_printf("Unity IL2CPP(%s %s)", date, time);
}

void* mono_unity_get_field_address(MonoObject *obj, MonoVTable *vt, MonoClassField *field)
{
	// This is a copy of mono_field_get_addr - we need to consider how to expose that on the public API.
	MONO_REQ_GC_UNSAFE_MODE;

	guint8 *src;

	if (field->type->attrs & FIELD_ATTRIBUTE_STATIC) {
		if (field->offset == -1) {
			/* Special static */
			gpointer addr;

			mono_domain_lock(vt->domain);
			addr = g_hash_table_lookup(vt->domain->special_static_fields, field);
			mono_domain_unlock(vt->domain);
			src = (guint8 *)mono_get_special_static_data(GPOINTER_TO_UINT(addr));
		}
		else {
			src = (guint8*)mono_vtable_get_static_field_data(vt) + field->offset;
		}
	}
	else {
		src = (guint8*)obj + field->offset;
	}

	return src;
}

MONO_API MonoClassField* mono_unity_field_from_token_checked(MonoImage *image, guint32 token, MonoClass **retklass, MonoGenericContext *context, MonoError *error)
{
	return mono_field_from_token_checked(image, token, retklass, context, error);
}

gboolean mono_unity_thread_state_init_from_handle(MonoThreadUnwindState *tctx, MonoThreadInfo *info, void* fixme)
{
	tctx->valid = TRUE;
	tctx->unwind_data[MONO_UNWIND_DATA_DOMAIN] = mono_domain_get();
	tctx->unwind_data[MONO_UNWIND_DATA_LMF] = NULL;
	tctx->unwind_data[MONO_UNWIND_DATA_JIT_TLS] = NULL;

	return TRUE;
}

void mono_unity_stackframe_set_method(MonoStackFrame *sf, MonoMethod *method)
{
	g_assert(sizeof(sf->method) == sizeof(void**));
	MonoError unused;
	mono_gc_wbarrier_set_field((MonoObject*)(sf), &(sf->method), (MonoObject*)mono_method_get_object_checked(mono_domain_get(), method, NULL, &unused));
}

MonoType* mono_unity_reflection_type_get_type(MonoReflectionType *type)
{
	return type->type;
}

// layer to proxy differences between old and new Mono versions

MONO_API void
mono_unity_runtime_set_main_args (int argc, const char* argv[])
{
	mono_runtime_set_main_args (argc, argv);
}

MONO_API MonoString*
mono_unity_string_empty_wrapper ()
{
	return mono_string_empty (mono_domain_get ());
}

MONO_API MonoArray*
mono_unity_array_new_2d (MonoDomain *domain, MonoClass *eklass, size_t size0, size_t size1)
{
	MonoError error;
	uintptr_t sizes[] = { (uintptr_t)size0, (uintptr_t)size1 };
	MonoClass* ac = mono_array_class_get (eklass, 2);

	MonoArray* array = mono_array_new_full_checked (domain, ac, sizes, NULL, &error);
	mono_error_cleanup (&error);

	return array;
}

MONO_API MonoArray*
mono_unity_array_new_3d (MonoDomain *domain, MonoClass *eklass, size_t size0, size_t size1, size_t size2)
{
	MonoError error;
	uintptr_t sizes[] = { (uintptr_t)size0, (uintptr_t)size1, (uintptr_t)size2 };
	MonoClass* ac = mono_array_class_get (eklass, 3);

	MonoArray* array =  mono_array_new_full_checked (domain, ac, sizes, NULL, &error);
	mono_error_cleanup (&error);

	return array;
}

MONO_API void
mono_unity_domain_set_config (MonoDomain *domain, const char *base_dir, const char *config_file_name)
{
	mono_domain_set_config (domain, base_dir, config_file_name);
}

MONO_API MonoException*
mono_unity_loader_get_last_error_and_error_prepare_exception ()
{
	return NULL;
}

MONO_API void
mono_unity_install_memory_callbacks (MonoAllocatorVTable* callbacks)
{
	mono_set_allocator_vtable (callbacks);
}

static char* data_dir = NULL;
MONO_API void
mono_unity_set_data_dir(const char* dir)
{
    if (data_dir)
        g_free(data_dir);

    data_dir = g_new(char, strlen(dir) + 1);
    strcpy(data_dir, dir);
}

MONO_API char*
mono_unity_get_data_dir()
{
    return data_dir;
}

MONO_API MonoClass*
mono_unity_class_get_generic_type_definition (MonoClass* klass)
{
	MonoGenericClass* generic_class = mono_class_try_get_generic_class (klass);
	return generic_class ? generic_class->container_class : NULL;
}

MONO_API MonoClass*
mono_unity_class_get_generic_parameter_at (MonoClass* klass, guint32 index)
{
	MonoGenericContainer* generic_container = mono_class_try_get_generic_container (klass);
	if (!generic_container || index >= generic_container->type_argc)
		return NULL;

	return mono_class_create_generic_parameter (mono_generic_container_get_param (generic_container, index));
}

MONO_API guint32
mono_unity_class_get_generic_parameter_count (MonoClass* klass)
{
	MonoGenericContainer* generic_container = mono_class_try_get_generic_container (klass);

	if (!generic_container)
		return 0;

	return generic_container->type_argc;
}

MONO_API MonoClass*
mono_unity_class_get_generic_argument_at (MonoClass* klass, guint32 index)
{
	if (!mono_class_is_ginst (klass))
		return NULL;

	MonoGenericClass* generic_class = mono_class_get_generic_class (klass);

	if (index >= generic_class->context.class_inst->type_argc)
		return NULL;

	return mono_class_from_mono_type (generic_class->context.class_inst->type_argv[index]);
}

MONO_API guint32
mono_unity_class_get_generic_argument_count (MonoClass* klass)
{
	if (!mono_class_is_ginst (klass))
		return 0;

	MonoGenericClass* generic_class = mono_class_get_generic_class (klass);

	return generic_class->context.class_inst->type_argc;
}

MONO_API MonoClass*
mono_unity_class_get(MonoImage* image, guint32 type_token)
{
	// Unity expects to try to get classes that don't exist, and
	// have a value of NULL returned. So eat the error message.
	MonoError unused;
	MonoClass* klass= mono_class_get_checked(image, type_token, &unused);
	mono_error_cleanup(&unused);
	return klass;
}

MONO_API gpointer
mono_unity_alloc(gsize size)
{
	return g_malloc(size);
}


MONO_API void
mono_unity_thread_fast_attach (MonoDomain *domain)
{
	MonoInternalThread *thread;

	g_assert (domain);
	g_assert (domain != mono_get_root_domain ());

	thread = mono_thread_internal_current ();
	g_assert (thread);

	mono_thread_push_appdomain_ref (domain);
	g_assert (mono_domain_set (domain, FALSE));

	//mono_profiler_thread_fast_attach (thread->tid);
}

MONO_API void
mono_unity_thread_fast_detach ()
{
	MonoInternalThread *thread;
	MonoDomain *current_domain;

	thread = mono_thread_internal_current ();
	g_assert (thread);

	current_domain = mono_domain_get ();

	g_assert (current_domain);
	g_assert (current_domain != mono_get_root_domain ());

	//mono_profiler_thread_fast_detach (thread->tid);

	// Migrating to the root domain and popping the domain reference allows
	// the thread to stay alive and keep running while the domain can be unloaded
	g_assert (mono_domain_set (mono_get_root_domain (), FALSE));
	mono_thread_pop_appdomain_ref ();
}

// hack, FIXME jon

/*
size_t RemapPathFunction (const char* path, char* buffer, size_t buffer_len)
	path         = original path
	buffer       = provided buffer to fill out
	buffer_len   = byte size of buffer (above)
	return value = buffer size needed, incl. terminating 0
	* may be called with buffer = null / buffer_len = 0, or a shorter-than-necessary-buffer.
	* return value is always the size _needed_; not the size written.
	* terminating zero should always be written.
	* if buffer_len is less than needed, buffer content is undefined
	* if return value is 0 no remapping is needed / available
*/
static RemapPathFunction g_RemapPathFunc = NULL;

void
mono_unity_register_path_remapper (RemapPathFunction func)
{
	g_RemapPathFunc = func;
}

/* calls remapper function if registered; allocates memory if remapping is available */
static inline size_t
call_remapper(const char* path, char** buf)
{
	size_t len;

	if (!g_RemapPathFunc)
		return 0;

	*buf = NULL;
	len = g_RemapPathFunc(path, *buf, 0);

	if (len == 0)
		return 0;

	*buf = g_new (char, len);
	g_RemapPathFunc(path, *buf, len);

	return len;
}

MonoBoolean
ves_icall_System_IO_MonoIO_RemapPath  (MonoString *path, MonoString **new_path)
{
	MonoError error;
	const gunichar2* path_remapped;

	if (!g_RemapPathFunc)
		return 0;

	path_remapped = mono_unity_remap_path_utf16 (mono_string_chars (path));

	if (!path_remapped)
		return FALSE;

	mono_gc_wbarrier_generic_store (new_path, (MonoObject*)mono_string_from_utf16_checked (path_remapped, &error));

	g_free (path_remapped);

	mono_error_set_pending_exception (&error);

	return TRUE;
}

const char*
mono_unity_remap_path (const char* path)
{
	const char* path_remap = NULL;
	call_remapper (path, &path_remap);

	return path_remap;
}

const gunichar2*
mono_unity_remap_path_utf16 (const gunichar2* path)
{
	const gunichar2* path_remap = NULL;
	char * utf8_path;
	char * buf;
	char * path_end;
	size_t len;

	if (!g_RemapPathFunc)
		return path_remap;

	utf8_path = g_utf16_to_utf8 (path, -1, NULL, NULL, NULL);
	len = call_remapper (utf8_path, &buf);
	if (len == 0)
	{
		g_free (utf8_path);
		return path_remap;
	}

	path_end = memchr (buf, '\0', len);
	len = path_end ? (size_t)(path_end - buf) : len;

	path_remap = g_utf8_to_utf16 (buf, len, NULL, NULL, NULL);

	g_free (utf8_path);
	g_free (buf);

	return path_remap;
}

MonoMethod*
mono_method_get_method_definition (MonoMethod *method)
{
	while (method->is_inflated)
		method = ((MonoMethodInflated*)method)->declaring;
	return method;
}

gboolean mono_allow_gc_aware_layout = TRUE;

void
mono_class_set_allow_gc_aware_layout(mono_bool allow)
{
	mono_allow_gc_aware_layout = allow;
}

static mono_bool enable_handler_block_guards = TRUE;

void
mono_unity_set_enable_handler_block_guards (mono_bool allow)
{
	enable_handler_block_guards = allow;
}

mono_bool
mono_unity_get_enable_handler_block_guards (void)
{
	return enable_handler_block_guards;
}

//helper structures for VM information extraction functions
typedef struct {
	GFunc callback;
	gpointer user_data;
} execution_ctx;

typedef struct
{
	gpointer start;
	size_t size;
} mono_heap_chunk;

// class metadata memory
static void
handle_mem_pool_chunk(gpointer chunkStart, gpointer chunkEnd, gpointer userData)
{
	mono_heap_chunk chunk;
	chunk.start = chunkStart;
	chunk.size = (uint8_t *)chunkEnd - (uint8_t *)chunkStart;

	execution_ctx *ctx = (execution_ctx *)userData;
	ctx->callback(&chunk, ctx->user_data);
}

static void
handle_image_set_mem_pool(MonoImageSet *imageSet, gpointer user_data)
{
	mono_mempool_foreach_block(imageSet->mempool, handle_mem_pool_chunk, user_data);
}

MONO_API void
mono_unity_image_set_mempool_chunk_foreach(GFunc callback, gpointer user_data)
{
	execution_ctx ctx;
	ctx.callback = callback;
	ctx.user_data = user_data;

	mono_metadata_image_set_foreach(handle_image_set_mem_pool, &ctx);
}

MONO_API void
mono_unity_domain_mempool_chunk_foreach(MonoDomain *domain, GFunc callback, gpointer user_data)
{
	MonoMemoryManager* memory_manager = mono_domain_ambient_memory_manager(domain);

	mono_mem_manager_lock(memory_manager);
	execution_ctx ctx;
	ctx.callback = callback;
	ctx.user_data = user_data;
	mono_mempool_foreach_block(memory_manager->mp, handle_mem_pool_chunk, &ctx);

	mono_mem_manager_unlock(memory_manager);
}

MONO_API void
mono_unity_root_domain_mempool_chunk_foreach(GFunc callback, gpointer user_data)
{
	MonoMemoryManager* memory_manager = mono_domain_ambient_memory_manager(mono_get_root_domain());
	mono_mem_manager_lock(memory_manager);

	execution_ctx ctx;
	ctx.callback = callback;
	ctx.user_data = user_data;
	mono_mempool_foreach_block(memory_manager->mp, handle_mem_pool_chunk, &ctx);

	mono_mem_manager_unlock(memory_manager);
}

MONO_API void
mono_unity_assembly_mempool_chunk_foreach(MonoAssembly *assembly, GFunc callback, gpointer user_data)
{
	MonoImage *image = assembly->image;
	mono_image_lock(image);

	execution_ctx ctx;
	ctx.callback = callback;
	ctx.user_data = user_data;
	mono_mempool_foreach_block(image->mempool, handle_mem_pool_chunk, &ctx);

	if (image->module_count > 0) {
		guint32 i;

		for (i = 0; i < image->module_count; ++i) {
			MonoImage *moduleImage = image->modules[i];

			if (moduleImage) {
				mono_mempool_foreach_block(moduleImage->mempool, handle_mem_pool_chunk, &ctx);
			}
		}
	}
	mono_image_unlock(image);
}

// class metadata

static char *
mono_identifier_escape_type_append(char *bufferPtr, const char *identifier)
{
	for (const char *s = identifier; *s != 0; ++s) {
		switch (*s) {
		case ',':
		case '+':
		case '&':
		case '*':
		case '[':
		case ']':
		case '\\':
			*bufferPtr++ = '\\';
			*bufferPtr++ = *s;

			return bufferPtr;
		default:
			*bufferPtr++ = *s;

			return bufferPtr;
		}
	}

	return bufferPtr;
}

enum {
	//max digits on uint16 is 5(used to convert the number of generic args) + max 3 other slots taken;
	kNameChunkBufferSize = 8
};

static inline char *
flush_name_buffer(char *buffer, GFunc callback, void *userData)
{
	callback(buffer, userData);
	memset(buffer, 0x00, kNameChunkBufferSize);

	return buffer;
}

static void
mono_unity_type_get_name_foreach_name_chunk_recurse(MonoType *type, gboolean is_recursed, MonoTypeNameFormat format, GFunc nameChunkReport, void *userData)
{
	MonoClass *klass = NULL;
	char buffer[kNameChunkBufferSize + 1]; //null terminate the buffer
	memset(buffer, 0x00, kNameChunkBufferSize + 1);
	char *bufferPtr = buffer;
	char *bufferIter = buffer;

	switch (type->type) {
	case MONO_TYPE_ARRAY: {
		int i, rank = type->data.array->rank;
		MonoTypeNameFormat nested_format;

		nested_format = format == MONO_TYPE_NAME_FORMAT_ASSEMBLY_QUALIFIED ? MONO_TYPE_NAME_FORMAT_FULL_NAME : format;

		mono_unity_type_get_name_foreach_name_chunk_recurse(
			&type->data.array->eklass->_byval_arg, FALSE, nested_format, nameChunkReport, userData);

		*bufferIter++ = '[';

		if (rank == 1) {
			*bufferIter++ = '*';
		}

		for (i = 1; i < rank; i++) {

			*bufferIter++ = ',';

			if (kNameChunkBufferSize - (bufferIter - bufferPtr) < 2) {
				bufferIter = flush_name_buffer(bufferPtr, nameChunkReport, userData);
			}
		}

		*bufferIter++ = ']';

		if (type->byref) {
			*bufferIter++ = '&';
		}

		nameChunkReport(bufferPtr, userData);

		if (format == MONO_TYPE_NAME_FORMAT_ASSEMBLY_QUALIFIED) {
			MonoClass *klass = mono_class_from_mono_type(type);
			MonoImage *klassImg = mono_class_get_image(klass);
			char *imgName = mono_image_get_name(klassImg);
			nameChunkReport(imgName, userData);
		}
		break;
	}
	case MONO_TYPE_SZARRAY: {
		MonoTypeNameFormat nested_format;

		nested_format = format == MONO_TYPE_NAME_FORMAT_ASSEMBLY_QUALIFIED ? MONO_TYPE_NAME_FORMAT_FULL_NAME : format;

		mono_unity_type_get_name_foreach_name_chunk_recurse(&type->data.klass->_byval_arg, FALSE, nested_format, nameChunkReport, userData);

		*bufferIter++ = '[';
		*bufferIter++ = ']';

		if (type->byref)
			*bufferIter++ = '&';

		nameChunkReport(bufferPtr, userData);

		if (format == MONO_TYPE_NAME_FORMAT_ASSEMBLY_QUALIFIED) {
			MonoClass *klass = mono_class_from_mono_type(type);
			MonoImage *klassImg = mono_class_get_image(klass);
			char *imgName = mono_image_get_name(klassImg);
			nameChunkReport(imgName, userData);
		}
		break;
	}
	case MONO_TYPE_PTR: {
		MonoTypeNameFormat nested_format;

		nested_format = format == MONO_TYPE_NAME_FORMAT_ASSEMBLY_QUALIFIED ? MONO_TYPE_NAME_FORMAT_FULL_NAME : format;

		mono_unity_type_get_name_foreach_name_chunk_recurse(type->data.type, FALSE, nested_format, nameChunkReport, userData);
		*bufferIter++ = '*';

		if (type->byref)
			*bufferIter++ = '&';

		nameChunkReport(bufferPtr, userData);

		if (format == MONO_TYPE_NAME_FORMAT_ASSEMBLY_QUALIFIED) {
			MonoClass *klass = mono_class_from_mono_type(type);
			MonoImage *klassImg = mono_class_get_image(klass);
			char *imgName = mono_image_get_name(klassImg);
			nameChunkReport(imgName, userData);
		}
		break;
	}
	case MONO_TYPE_VAR:
	case MONO_TYPE_MVAR:
		if (!mono_generic_param_name(type->data.generic_param)) {

			if (type->type == MONO_TYPE_VAR) {
				*bufferIter++ = '!';
			}
			else {
				*bufferIter++ = '!';
				*bufferIter++ = '!';
			}
			sprintf(bufferIter, "%d", type->data.generic_param->num);
		}
		else
			nameChunkReport(mono_generic_param_name(type->data.generic_param), userData);

		if (type->byref)
			*bufferIter++ = '&';

		nameChunkReport(bufferPtr, userData);
		break;
	default:
		klass = mono_class_from_mono_type(type);
		if (klass->nested_in) {
			mono_unity_type_get_name_foreach_name_chunk_recurse(&klass->nested_in->_byval_arg, TRUE, format, nameChunkReport, userData);
			if (format == MONO_TYPE_NAME_FORMAT_IL)
				*bufferIter++ = '.';
			else
				*bufferIter++ = '+';
		}
		else if (*klass->name_space) {
			if (format == MONO_TYPE_NAME_FORMAT_IL)
				nameChunkReport(klass->name_space, userData);
			else
				bufferIter = mono_identifier_escape_type_append(bufferIter, klass->name_space);

			*bufferIter++ = '.';
		}

		if (format == MONO_TYPE_NAME_FORMAT_IL) {
			char *s = strchr(klass->name, '`');
			int len = s ? s - klass->name : strlen(klass->name);

			for (int i = 0; i < len; ++i) {

				*bufferIter++ = *(klass->name + i);
				if (kNameChunkBufferSize - (bufferIter - bufferPtr) == 0) {
					bufferIter = flush_name_buffer(bufferPtr, nameChunkReport, userData);
				}
			}

			if (bufferPtr != bufferIter) {
				bufferIter = flush_name_buffer(bufferPtr, nameChunkReport, userData);
			}

		}
		else {
			bufferIter = mono_identifier_escape_type_append(bufferIter, klass->name);
		}

		if (!is_recursed) {
			if (bufferIter != bufferPtr)
				bufferIter = flush_name_buffer(bufferPtr, nameChunkReport, userData);

			if (mono_class_is_ginst(klass)) {
				MonoGenericClass *gclass = mono_class_get_generic_class(klass);
				MonoGenericInst *inst = gclass->context.class_inst;
				MonoTypeNameFormat nested_format;
				int i;

				nested_format = format == MONO_TYPE_NAME_FORMAT_FULL_NAME ? MONO_TYPE_NAME_FORMAT_ASSEMBLY_QUALIFIED : format;

				if (format == MONO_TYPE_NAME_FORMAT_IL)
					*bufferIter++ = '<';
				else
					*bufferIter++ = '[';

				for (i = 0; i < inst->type_argc; i++) {
					MonoType *t = inst->type_argv[i];

					if (i)
						*bufferIter++ = ',';
					if ((nested_format == MONO_TYPE_NAME_FORMAT_ASSEMBLY_QUALIFIED) &&
						(t->type != MONO_TYPE_VAR) && (type->type != MONO_TYPE_MVAR))
						*bufferIter++ = '[';

					//flush the buffer before recursing
					bufferIter = flush_name_buffer(bufferPtr, nameChunkReport, userData);
					mono_unity_type_get_name_foreach_name_chunk_recurse(inst->type_argv[i], FALSE, nested_format, nameChunkReport, userData);

					if ((nested_format == MONO_TYPE_NAME_FORMAT_ASSEMBLY_QUALIFIED) &&
						(t->type != MONO_TYPE_VAR) && (type->type != MONO_TYPE_MVAR))
						*bufferIter++ = ']';
				}
				if (format == MONO_TYPE_NAME_FORMAT_IL)
					*bufferIter++ = '>';
				else
					*bufferIter++ = ']';
			}
			else if (mono_class_is_gtd(klass) &&
				(format != MONO_TYPE_NAME_FORMAT_FULL_NAME) &&
				(format != MONO_TYPE_NAME_FORMAT_ASSEMBLY_QUALIFIED)) {
				int i;

				if (format == MONO_TYPE_NAME_FORMAT_IL)
					*bufferIter++ = '<';
				else
					*bufferIter++ = '[';

				bufferIter = flush_name_buffer(bufferPtr, nameChunkReport, userData);

				for (i = 0; i < mono_class_get_generic_container(klass)->type_argc; i++) {
					if (i)
						nameChunkReport(",", userData);
					nameChunkReport(mono_generic_container_get_param_info(mono_class_get_generic_container(klass), i)->name, userData);
				}
				if (format == MONO_TYPE_NAME_FORMAT_IL)
					*bufferIter++ = '>';
				else
					*bufferIter++ = ']';
			}

			if (type->byref)
				*bufferIter++ = '&';
		}

		if (bufferPtr != bufferIter)
			nameChunkReport(bufferPtr, userData);

		if ((format == MONO_TYPE_NAME_FORMAT_ASSEMBLY_QUALIFIED) &&
			(type->type != MONO_TYPE_VAR) && (type->type != MONO_TYPE_MVAR)) {
			MonoImage *klassImg = mono_class_get_image(klass);
			char *imgName = mono_image_get_name(klassImg);
			nameChunkReport(imgName, userData);
		}
		break;
	}
}

/**
 * mono_unity_type_get_name_full_chunked:
 * \param type a type
 * \reports chunks null terminated of a type's name via a callback.
 */

MONO_API void
mono_unity_type_get_name_full_chunked(MonoType *type, GFunc chunkReportFunc, gpointer userData)
{
	mono_unity_type_get_name_foreach_name_chunk_recurse(type, FALSE, MONO_TYPE_NAME_FORMAT_IL, chunkReportFunc, userData);
}

MONO_API gboolean
mono_unity_type_is_pointer_type(MonoType *type)
{
	return type->type == MONO_TYPE_PTR;
}

MONO_API gboolean
mono_unity_type_is_static(MonoType *type)
{
	return (type->attrs & FIELD_ATTRIBUTE_STATIC) != 0;
}

MONO_API MonoVTable *
mono_unity_class_try_get_vtable(MonoDomain *domain, MonoClass *klass)
{
	return mono_class_try_get_vtable(domain, klass);
}

MONO_API uint32_t
mono_unity_class_get_data_size(MonoClass *klass)
{
	return mono_class_data_size(klass);
}

MONO_API void *
mono_unity_vtable_get_static_field_data(MonoVTable *vTable)
{
	return mono_vtable_get_static_field_data(vTable);
}

MONO_API gboolean
mono_unity_class_field_is_literal(MonoClassField *field)
{
	return (field->type->attrs & FIELD_ATTRIBUTE_LITERAL) != 0;
}

// GC world control
MONO_API void
mono_unity_stop_gc_world()
{
#if HAVE_BOEHM_GC
	GC_stop_world_external();
#else
	g_assert_not_reached();
#endif
}

MONO_API void
mono_unity_start_gc_world()
{
#if HAVE_BOEHM_GC
	GC_start_world_external();
#else
	g_assert_not_reached();
#endif
}


//GC memory
static void
handle_gc_heap_chunk(void *userdata, gpointer chunk_start, gpointer chunk_end)
{
	execution_ctx *ctx = (execution_ctx *)userdata;
	mono_heap_chunk chunk;
	chunk.start = chunk_start;
	chunk.size = (uint8_t *)chunk_end - (uint8_t *)chunk_start;
	ctx->callback(&chunk, ctx->user_data);
}

MONO_API void
mono_unity_gc_heap_foreach(GFunc callback, gpointer user_data)
{
#if HAVE_BOEHM_GC
	execution_ctx ctx;
	ctx.callback = callback;
	ctx.user_data = user_data;

	GC_foreach_heap_section(&ctx, handle_gc_heap_chunk);
#else
	g_assert_not_reached();
#endif
}

//GC handles
static void
handle_gc_handle(gpointer handle_target, gpointer handle_report_callback)
{
	execution_ctx *ctx = (execution_ctx *)handle_report_callback;
	ctx->callback(handle_target, ctx->user_data);
}

MONO_API void
mono_unity_gc_handles_foreach_get_target(GFunc callback, gpointer user_data)
{
#if HAVE_BOEHM_GC
	execution_ctx ctx;
	ctx.callback = callback;
	ctx.user_data = user_data;
	mono_gc_strong_handle_foreach(handle_gc_handle, &ctx);
#else
	g_assert_not_reached();
#endif
}

// VM runtime info
MONO_API uint32_t
mono_unity_object_header_size()
{
	return (uint32_t)(sizeof(MonoObject));
}

MONO_API uint32_t
mono_unity_array_object_header_size()
{
	return offsetof(MonoArray, vector);
}

MONO_API uint32_t
mono_unity_offset_of_array_length_in_array_object_header()
{
	return offsetof(MonoArray, max_length);
}

MONO_API uint32_t
mono_unity_offset_of_array_bounds_in_array_object_header()
{
	return offsetof(MonoArray, bounds);
}

MONO_API uint32_t
mono_unity_allocation_granularity()
{
	return (uint32_t)(2 * sizeof(void *));
}

MONO_API gboolean
mono_unity_class_is_open_constructed_type (MonoClass *klass)
{
	return mono_class_is_open_constructed_type (m_class_get_byval_arg(klass));
}

MONO_API gboolean
mono_unity_class_has_failure(const MonoClass* klass)
{
	return mono_class_has_failure(klass);
}
