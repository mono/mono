/*
 * reflection.c: System.Type icalls and related reflection queries.
 * 
 * Author:
 *   Paolo Molaro (lupus@ximian.com)
 *
 * Copyright 2001-2003 Ximian, Inc (http://www.ximian.com)
 * Copyright 2004-2009 Novell, Inc (http://www.novell.com)
 * Copyright 2011 Rodrigo Kumpera
 * Copyright 2016 Microsoft
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */
#include <config.h>
#include "mono/utils/mono-membar.h"
#include "mono/metadata/reflection-internals.h"
#include "mono/metadata/tabledefs.h"
#include "mono/metadata/metadata-internals.h"
#include <mono/metadata/profiler-private.h>
#include "mono/metadata/class-internals.h"
#include "mono/metadata/gc-internals.h"
#include "mono/metadata/domain-internals.h"
#include "mono/metadata/opcodes.h"
#include "mono/metadata/assembly.h"
#include "mono/metadata/object-internals.h"
#include <mono/metadata/exception.h>
#include <mono/metadata/marshal.h>
#include <mono/metadata/security-manager.h>
#include <mono/metadata/reflection-cache.h>
#include <mono/metadata/sre-internals.h>
#include <stdio.h>
#include <glib.h>
#include <errno.h>
#include <time.h>
#include <string.h>
#include <ctype.h>
#include "image.h"
#include "cil-coff.h"
#include "mono-endian.h"
#include <mono/metadata/gc-internals.h>
#include <mono/metadata/mempool-internals.h>
#include <mono/metadata/security-core-clr.h>
#include <mono/metadata/debug-helpers.h>
#include <mono/metadata/verify-internals.h>
#include <mono/metadata/mono-ptr-array.h>
#include <mono/utils/mono-string.h>
#include <mono/utils/mono-error-internals.h>
#include <mono/utils/checked-build.h>
#include <mono/utils/mono-counters.h>

static void get_default_param_value_blobs (MonoMethod *method, char **blobs, guint32 *types);
static MonoType* mono_reflection_get_type_with_rootimage (MonoImage *rootimage, MonoImage* image, MonoTypeNameParse *info, gboolean ignorecase, gboolean *type_resolve, MonoError *error);

/* Class lazy loading functions */
static GENERATE_GET_CLASS_WITH_CACHE (mono_assembly, System.Reflection, MonoAssembly)
static GENERATE_GET_CLASS_WITH_CACHE (mono_module, System.Reflection, MonoModule)
static GENERATE_GET_CLASS_WITH_CACHE (mono_method, System.Reflection, MonoMethod);
static GENERATE_GET_CLASS_WITH_CACHE (mono_cmethod, System.Reflection, MonoCMethod);
static GENERATE_GET_CLASS_WITH_CACHE (mono_field, System.Reflection, MonoField);
static GENERATE_GET_CLASS_WITH_CACHE (mono_event, System.Reflection, MonoEvent);
static GENERATE_GET_CLASS_WITH_CACHE (mono_property, System.Reflection, MonoProperty);
static GENERATE_GET_CLASS_WITH_CACHE (mono_parameter_info, System.Reflection, MonoParameterInfo);
static GENERATE_GET_CLASS_WITH_CACHE (missing, System.Reflection, Missing);
static GENERATE_GET_CLASS_WITH_CACHE (method_body, System.Reflection, MethodBody);
static GENERATE_GET_CLASS_WITH_CACHE (local_variable_info, System.Reflection, LocalVariableInfo);
static GENERATE_GET_CLASS_WITH_CACHE (exception_handling_clause, System.Reflection, ExceptionHandlingClause);
static GENERATE_GET_CLASS_WITH_CACHE (type_builder, System.Reflection.Emit, TypeBuilder);
static GENERATE_GET_CLASS_WITH_CACHE (dbnull, System, DBNull);


static int class_ref_info_handle_count;

void
mono_reflection_init (void)
{
	mono_reflection_emit_init ();

	mono_counters_register ("MonoClass::ref_info_handle count",
							MONO_COUNTER_METADATA | MONO_COUNTER_INT, &class_ref_info_handle_count);

}

/*
 * mono_class_get_ref_info:
 *
 *   Return the type builder/generic param builder corresponding to KLASS, if it exists.
 */
gpointer
mono_class_get_ref_info (MonoClass *klass)
{
	MONO_REQ_GC_UNSAFE_MODE;
	guint32 ref_info_handle = mono_class_get_ref_info_handle (klass);

	if (ref_info_handle == 0)
		return NULL;
	else
		return mono_gchandle_get_target (ref_info_handle);
}

void
mono_class_set_ref_info (MonoClass *klass, gpointer obj)
{
	MONO_REQ_GC_UNSAFE_MODE;

	guint32 candidate = mono_gchandle_new ((MonoObject*)obj, FALSE);
	guint32 handle = mono_class_set_ref_info_handle (klass, candidate);
	++class_ref_info_handle_count;

	if (handle != candidate)
		mono_gchandle_free (candidate);
}

void
mono_class_free_ref_info (MonoClass *klass)
{
	MONO_REQ_GC_NEUTRAL_MODE;
	guint32 handle = mono_class_get_ref_info_handle (klass);

	if (handle) {
		mono_gchandle_free (handle);
		mono_class_set_ref_info_handle (klass, 0);
	}
}


void
mono_custom_attrs_free (MonoCustomAttrInfo *ainfo)
{
	MONO_REQ_GC_NEUTRAL_MODE;

	if (ainfo && !ainfo->cached)
		g_free (ainfo);
}


gboolean
reflected_equal (gconstpointer a, gconstpointer b)
{
	const ReflectedEntry *ea = (const ReflectedEntry *)a;
	const ReflectedEntry *eb = (const ReflectedEntry *)b;

	return (ea->item == eb->item) && (ea->refclass == eb->refclass);
}

guint
reflected_hash (gconstpointer a) {
	const ReflectedEntry *ea = (const ReflectedEntry *)a;
	return mono_aligned_addr_hash (ea->item);
}


static void
clear_cached_object (MonoDomain *domain, gpointer o, MonoClass *klass)
{
	mono_domain_lock (domain);
	if (domain->refobject_hash) {
        ReflectedEntry pe;
		gpointer orig_pe, orig_value;

		pe.item = o;
		pe.refclass = klass;
		if (mono_g_hash_table_lookup_extended (domain->refobject_hash, &pe, &orig_pe, &orig_value)) {
			mono_g_hash_table_remove (domain->refobject_hash, &pe);
			FREE_REFENTRY (orig_pe);
		}
	}
	mono_domain_unlock (domain);
}

#ifdef REFENTRY_REQUIRES_CLEANUP
static void
cleanup_refobject_hash (gpointer key, gpointer value, gpointer user_data)
{
	FREE_REFENTRY (key);
}
#endif

void
mono_reflection_cleanup_domain (MonoDomain *domain)
{
	if (domain->refobject_hash) {
/*let's avoid scanning the whole hashtable if not needed*/
#ifdef REFENTRY_REQUIRES_CLEANUP
		mono_g_hash_table_foreach (domain->refobject_hash, cleanup_refobject_hash, NULL);
#endif
		mono_g_hash_table_destroy (domain->refobject_hash);
		domain->refobject_hash = NULL;
	}
}


/*
 * mono_assembly_get_object:
 * @domain: an app domain
 * @assembly: an assembly
 *
 * Return an System.Reflection.Assembly object representing the MonoAssembly @assembly.
 */
MonoReflectionAssembly*
mono_assembly_get_object (MonoDomain *domain, MonoAssembly *assembly)
{
	HANDLE_FUNCTION_ENTER ();
	MonoError error;
	MonoReflectionAssemblyHandle result = mono_assembly_get_object_handle (domain, assembly, &error);
	mono_error_cleanup (&error); /* FIXME new API that doesn't swallow the error */
	HANDLE_FUNCTION_RETURN_OBJ (result);
}

static MonoReflectionAssemblyHandle
assembly_object_construct (MonoDomain *domain, MonoClass *unused_klass, MonoAssembly *assembly, gpointer user_data, MonoError *error)
{
	mono_error_init (error);
	MonoReflectionAssemblyHandle res = MONO_HANDLE_NEW (MonoReflectionAssembly, mono_object_new_checked (domain, mono_class_get_mono_assembly_class (), error));
	return_val_if_nok (error, MONO_HANDLE_CAST (MonoReflectionAssembly, NULL_HANDLE));
	MONO_HANDLE_SETVAL (res, assembly, MonoAssembly*, assembly);
	return res;
}

/*
 * mono_assembly_get_object_handle:
 * @domain: an app domain
 * @assembly: an assembly
 *
 * Return an System.Reflection.Assembly object representing the MonoAssembly @assembly.
 */
MonoReflectionAssemblyHandle
mono_assembly_get_object_handle (MonoDomain *domain, MonoAssembly *assembly, MonoError *error)
{
	mono_error_init (error);
	return CHECK_OR_CONSTRUCT_HANDLE (MonoReflectionAssemblyHandle, assembly, NULL, assembly_object_construct, NULL);
}

MonoReflectionModule*   
mono_module_get_object   (MonoDomain *domain, MonoImage *image)
{
	HANDLE_FUNCTION_ENTER ();
	MonoError error;
	MonoReflectionModuleHandle result = mono_module_get_object_handle (domain, image, &error);
	mono_error_cleanup (&error);
	HANDLE_FUNCTION_RETURN_OBJ (result);
}

static MonoReflectionModuleHandle
module_object_construct (MonoDomain *domain, MonoClass *unused_klass, MonoImage *image, gpointer user_data, MonoError *error)
{
	char* basename;
	
	mono_error_init (error);
	MonoReflectionModuleHandle res = MONO_HANDLE_NEW (MonoReflectionModule, mono_object_new_checked (domain, mono_class_get_mono_module_class (), error));
	if (!is_ok (error))
		goto fail;

	MONO_HANDLE_SETVAL (res, image, MonoImage *, image);
	MonoReflectionAssemblyHandle assm_obj = mono_assembly_get_object_handle (domain, image->assembly, error);
	if (!is_ok (error))
		goto fail;
	MONO_HANDLE_SET (res, assembly, assm_obj);

	MONO_HANDLE_SET (res, fqname, mono_string_new_handle (domain, image->name, error));
	if (!is_ok (error))
		goto fail;
	basename = g_path_get_basename (image->name);
	MONO_HANDLE_SET (res, name, mono_string_new_handle (domain, basename, error));
	if (!is_ok (error))
		goto fail;
	MONO_HANDLE_SET (res, scopename, mono_string_new_handle (domain, image->module_name, error));
	if (!is_ok (error))
		goto fail;

	g_free (basename);

	guint32 token = 0;
	if (image->assembly->image == image) {
		token  = mono_metadata_make_token (MONO_TABLE_MODULE, 1);
	} else {
		int i;
		if (image->assembly->image->modules) {
			for (i = 0; i < image->assembly->image->module_count; i++) {
				if (image->assembly->image->modules [i] == image)
					token = mono_metadata_make_token (MONO_TABLE_MODULEREF, i + 1);
			}
			g_assert (token != 0);
		}
	}
	MONO_HANDLE_SETVAL (res, token, guint32, token);

	return res;
fail:
	return MONO_HANDLE_CAST (MonoReflectionModule, NULL_HANDLE);
}

MonoReflectionModuleHandle
mono_module_get_object_handle (MonoDomain *domain, MonoImage *image, MonoError *error)
{
	mono_error_init (error);
	return CHECK_OR_CONSTRUCT_HANDLE (MonoReflectionModuleHandle, image, NULL, module_object_construct, NULL);
}

MonoReflectionModule*
mono_module_file_get_object (MonoDomain *domain, MonoImage *image, int table_index)
{
	HANDLE_FUNCTION_ENTER ();
	MonoError error;
	MonoReflectionModuleHandle result = mono_module_file_get_object_handle (domain, image, table_index, &error);
	mono_error_cleanup (&error);
	HANDLE_FUNCTION_RETURN_OBJ (result);
}

MonoReflectionModuleHandle
mono_module_file_get_object_handle (MonoDomain *domain, MonoImage *image, int table_index, MonoError *error)
{
	MonoTableInfo *table;
	guint32 cols [MONO_FILE_SIZE];
	const char *name;
	guint32 i, name_idx;
	const char *val;
	
	mono_error_init (error);

	MonoReflectionModuleHandle res = MONO_HANDLE_NEW (MonoReflectionModule, mono_object_new_checked (domain, mono_class_get_mono_module_class (), error));
	if (!is_ok (error))
		goto fail;

	table = &image->tables [MONO_TABLE_FILE];
	g_assert (table_index < table->rows);
	mono_metadata_decode_row (table, table_index, cols, MONO_FILE_SIZE);

	MONO_HANDLE_SETVAL (res, image, MonoImage*, NULL);
	MonoReflectionAssemblyHandle assm_obj = mono_assembly_get_object_handle (domain, image->assembly, error);
	if (!is_ok (error))
		goto fail;
	MONO_HANDLE_SET (res, assembly, assm_obj);
	name = mono_metadata_string_heap (image, cols [MONO_FILE_NAME]);

	/* Check whenever the row has a corresponding row in the moduleref table */
	table = &image->tables [MONO_TABLE_MODULEREF];
	for (i = 0; i < table->rows; ++i) {
		name_idx = mono_metadata_decode_row_col (table, i, MONO_MODULEREF_NAME);
		val = mono_metadata_string_heap (image, name_idx);
		if (strcmp (val, name) == 0)
			MONO_HANDLE_SETVAL (res, image, MonoImage*, image->modules [i]);
	}

	MONO_HANDLE_SET (res, fqname, mono_string_new_handle (domain, name, error));
	if (!is_ok (error))
		goto fail;
	MONO_HANDLE_SET (res, name, mono_string_new_handle (domain, name, error));
	if (!is_ok (error))
		goto fail;
	MONO_HANDLE_SET (res, scopename, mono_string_new_handle (domain, name, error));
	if (!is_ok (error))
		goto fail;
	MONO_HANDLE_SETVAL (res, is_resource, MonoBoolean, cols [MONO_FILE_FLAGS] & FILE_CONTAINS_NO_METADATA);
	MONO_HANDLE_SETVAL (res, token, guint32, mono_metadata_make_token (MONO_TABLE_FILE, table_index + 1));

	return res;
fail:
	return MONO_HANDLE_CAST (MonoReflectionModule, NULL_HANDLE);
}

static MonoType*
mono_type_normalize (MonoType *type)
{
	int i;
	MonoGenericClass *gclass;
	MonoGenericInst *ginst;
	MonoClass *gtd;
	MonoGenericContainer *gcontainer;
	MonoType **argv = NULL;
	gboolean is_denorm_gtd = TRUE, requires_rebind = FALSE;

	if (type->type != MONO_TYPE_GENERICINST)
		return type;

	gclass = type->data.generic_class;
	ginst = gclass->context.class_inst;
	if (!ginst->is_open)
		return type;

	gtd = gclass->container_class;
	gcontainer = mono_class_get_generic_container (gtd);
	argv = g_newa (MonoType*, ginst->type_argc);

	for (i = 0; i < ginst->type_argc; ++i) {
		MonoType *t = ginst->type_argv [i], *norm;
		if (t->type != MONO_TYPE_VAR || t->data.generic_param->num != i || t->data.generic_param->owner != gcontainer)
			is_denorm_gtd = FALSE;
		norm = mono_type_normalize (t);
		argv [i] = norm;
		if (norm != t)
			requires_rebind = TRUE;
	}

	if (is_denorm_gtd)
		return type->byref == gtd->byval_arg.byref ? &gtd->byval_arg : &gtd->this_arg;

	if (requires_rebind) {
		MonoClass *klass = mono_class_bind_generic_parameters (gtd, ginst->type_argc, argv, gclass->is_dynamic);
		return type->byref == klass->byval_arg.byref ? &klass->byval_arg : &klass->this_arg;
	}

	return type;
}
/*
 * mono_type_get_object:
 * @domain: an app domain
 * @type: a type
 *
 * Return an System.MonoType object representing the type @type.
 */
MonoReflectionType*
mono_type_get_object (MonoDomain *domain, MonoType *type)
{
	MonoError error;
	MonoReflectionType *ret = mono_type_get_object_checked (domain, type, &error);
	mono_error_cleanup (&error);

	return ret;
}

MonoReflectionType*
mono_type_get_object_checked (MonoDomain *domain, MonoType *type, MonoError *error)
{
	MonoType *norm_type;
	MonoReflectionType *res;
	MonoClass *klass;

	mono_error_init (error);

	g_assert (type != NULL);
	klass = mono_class_from_mono_type (type);

	/*we must avoid using @type as it might have come
	 * from a mono_metadata_type_dup and the caller
	 * expects that is can be freed.
	 * Using the right type from 
	 */
	type = klass->byval_arg.byref == type->byref ? &klass->byval_arg : &klass->this_arg;

	/* void is very common */
	if (type->type == MONO_TYPE_VOID && domain->typeof_void)
		return (MonoReflectionType*)domain->typeof_void;

	/*
	 * If the vtable of the given class was already created, we can use
	 * the MonoType from there and avoid all locking and hash table lookups.
	 * 
	 * We cannot do this for TypeBuilders as mono_reflection_create_runtime_class expects
	 * that the resulting object is different.   
	 */
	if (type == &klass->byval_arg && !image_is_dynamic (klass->image)) {
		MonoVTable *vtable = mono_class_try_get_vtable (domain, klass);
		if (vtable && vtable->type)
			return (MonoReflectionType *)vtable->type;
	}

	mono_loader_lock (); /*FIXME mono_class_init and mono_class_vtable acquire it*/
	mono_domain_lock (domain);
	if (!domain->type_hash)
		domain->type_hash = mono_g_hash_table_new_type ((GHashFunc)mono_metadata_type_hash, 
				(GCompareFunc)mono_metadata_type_equal, MONO_HASH_VALUE_GC, MONO_ROOT_SOURCE_DOMAIN, "domain reflection types table");
	if ((res = (MonoReflectionType *)mono_g_hash_table_lookup (domain->type_hash, type))) {
		mono_domain_unlock (domain);
		mono_loader_unlock ();
		return res;
	}

	/*Types must be normalized so a generic instance of the GTD get's the same inner type.
	 * For example in: Foo<A,B>; Bar<A> : Foo<A, Bar<A>>
	 * The second Bar will be encoded a generic instance of Bar with <A> as parameter.
	 * On all other places, Bar<A> will be encoded as the GTD itself. This is an implementation
	 * artifact of how generics are encoded and should be transparent to managed code so we
	 * need to weed out this diference when retrieving managed System.Type objects.
	 */
	norm_type = mono_type_normalize (type);
	if (norm_type != type) {
		res = mono_type_get_object_checked (domain, norm_type, error);
		if (!mono_error_ok (error))
			return NULL;
		mono_g_hash_table_insert (domain->type_hash, type, res);
		mono_domain_unlock (domain);
		mono_loader_unlock ();
		return res;
	}

	if ((type->type == MONO_TYPE_GENERICINST) && type->data.generic_class->is_dynamic && !type->data.generic_class->container_class->wastypebuilder) {
		/* This can happen if a TypeBuilder for a generic class K<T,U>
		 * had reflection_create_generic_class) called on it, but not
		 * ves_icall_TypeBuilder_create_runtime_class.  This can happen
		 * if the K`2 is refernced from a generic instantiation
		 * (e.g. K<int,string>) that appears as type argument
		 * (e.g. Dict<string,K<int,string>>), field (e.g. K<int,string>
		 * Foo) or method signature, parent class or any of the above
		 * in a nested class of some other TypeBuilder.  Such an
		 * occurrence caused mono_reflection_type_get_handle to be
		 * called on the sre generic instance (K<int,string>) which
		 * required the container_class for the generic class K`2 to be
		 * set up, but the remainder of class construction for K`2 has
		 * not been done. */
		char * full_name = mono_type_get_full_name (klass);
		/* I would have expected ReflectionTypeLoadException, but evidently .NET throws TLE in this case. */
		mono_error_set_type_load_class (error, klass, "TypeBuilder.CreateType() not called for generic class %s", full_name);
		g_free (full_name);
		mono_domain_unlock (domain);
		mono_loader_unlock ();
		return NULL;
	}

	if (mono_class_get_ref_info (klass) && !klass->wastypebuilder && !type->byref) {
		mono_domain_unlock (domain);
		mono_loader_unlock ();
		return (MonoReflectionType *)mono_class_get_ref_info (klass);
	}
	/* This is stored in vtables/JITted code so it has to be pinned */
	res = (MonoReflectionType *)mono_object_new_pinned (domain, mono_defaults.runtimetype_class, error);
	if (!mono_error_ok (error))
		return NULL;

	res->type = type;
	mono_g_hash_table_insert (domain->type_hash, type, res);

	if (type->type == MONO_TYPE_VOID)
		domain->typeof_void = (MonoObject*)res;

	mono_domain_unlock (domain);
	mono_loader_unlock ();
	return res;
}

MonoReflectionTypeHandle
mono_type_get_object_handle (MonoDomain *domain, MonoType *type, MonoError *error)
{
	/* NOTE: We happen to know that mono_type_get_object_checked returns
	 * pinned objects, so we can just wrap its return value in a handle for
	 * uniformity.  If it ever starts returning unpinned, objects, this
	 * implementation would need to change!
	 */
	return MONO_HANDLE_NEW (MonoReflectionType, mono_type_get_object_checked (domain, type, error));
}

/*
 * mono_method_get_object:
 * @domain: an app domain
 * @method: a method
 * @refclass: the reflected type (can be NULL)
 *
 * Return an System.Reflection.MonoMethod object representing the method @method.
 */
MonoReflectionMethod*
mono_method_get_object (MonoDomain *domain, MonoMethod *method, MonoClass *refclass)
{
	HANDLE_FUNCTION_ENTER ();
	MonoError error;
	MonoReflectionMethodHandle ret = mono_method_get_object_handle (domain, method, refclass, &error);
	mono_error_cleanup (&error);
	HANDLE_FUNCTION_RETURN_OBJ (ret);
}

static MonoReflectionMethodHandle
method_object_construct (MonoDomain *domain, MonoClass *refclass, MonoMethod *method, gpointer user_data, MonoError *error)
{
	mono_error_init (error);
	g_assert (refclass != NULL);
	/*
	 * We use the same C representation for methods and constructors, but the type 
	 * name in C# is different.
	 */
	MonoClass *klass;

	mono_error_init (error);

	if (*method->name == '.' && (strcmp (method->name, ".ctor") == 0 || strcmp (method->name, ".cctor") == 0)) {
		klass = mono_class_get_mono_cmethod_class ();
	}
	else {
		klass = mono_class_get_mono_method_class ();
	}
	MonoReflectionMethodHandle ret = MONO_HANDLE_NEW (MonoReflectionMethod, mono_object_new_checked (domain, klass, error));
	if (!is_ok (error))
		goto fail;
	MONO_HANDLE_SETVAL (ret, method, MonoMethod*, method);

	MonoReflectionTypeHandle rt = mono_type_get_object_handle (domain, &refclass->byval_arg, error);
	if (!is_ok (error))
		goto fail;

	MONO_HANDLE_SET (ret, reftype, rt);

	return ret;

fail:
	return MONO_HANDLE_CAST (MonoReflectionMethod, NULL_HANDLE);
}

/*
 * mono_method_get_object_handle:
 * @domain: an app domain
 * @method: a method
 * @refclass: the reflected type (can be NULL)
 * @error: set on error.
 *
 * Return an System.Reflection.MonoMethod object representing the method @method.
 * Returns NULL and sets @error on error.
 */
MonoReflectionMethodHandle
mono_method_get_object_handle (MonoDomain *domain, MonoMethod *method, MonoClass *refclass, MonoError *error)
{
	mono_error_init (error);
	if (!refclass)
		refclass = method->klass;

	return CHECK_OR_CONSTRUCT_HANDLE (MonoReflectionMethodHandle, method, refclass, method_object_construct, NULL);
}
/*
 * mono_method_get_object_checked:
 * @domain: an app domain
 * @method: a method
 * @refclass: the reflected type (can be NULL)
 * @error: set on error.
 *
 * Return an System.Reflection.MonoMethod object representing the method @method.
 * Returns NULL and sets @error on error.
 */
MonoReflectionMethod*
mono_method_get_object_checked (MonoDomain *domain, MonoMethod *method, MonoClass *refclass, MonoError *error)
{
	HANDLE_FUNCTION_ENTER ();
	MonoReflectionMethodHandle result = mono_method_get_object_handle (domain, method, refclass, error);
	HANDLE_FUNCTION_RETURN_OBJ (result);
}

/*
 * mono_method_clear_object:
 *
 *   Clear the cached reflection objects for the dynamic method METHOD.
 */
void
mono_method_clear_object (MonoDomain *domain, MonoMethod *method)
{
	MonoClass *klass;
	g_assert (method_is_dynamic (method));

	klass = method->klass;
	while (klass) {
		clear_cached_object (domain, method, klass);
		klass = klass->parent;
	}
	/* Added by mono_param_get_objects () */
	clear_cached_object (domain, &(method->signature), NULL);
	klass = method->klass;
	while (klass) {
		clear_cached_object (domain, &(method->signature), klass);
		klass = klass->parent;
	}
}

/*
 * mono_field_get_object:
 * @domain: an app domain
 * @klass: a type
 * @field: a field
 *
 * Return an System.Reflection.MonoField object representing the field @field
 * in class @klass.
 */
MonoReflectionField*
mono_field_get_object (MonoDomain *domain, MonoClass *klass, MonoClassField *field)
{
	HANDLE_FUNCTION_ENTER ();
	MonoError error;
	MonoReflectionFieldHandle result = mono_field_get_object_handle (domain, klass, field, &error);
	mono_error_cleanup (&error);
	HANDLE_FUNCTION_RETURN_OBJ (result);
}

static MonoReflectionFieldHandle
field_object_construct (MonoDomain *domain, MonoClass *klass, MonoClassField *field, gpointer user_data, MonoError *error)
{
	mono_error_init (error);

	MonoReflectionFieldHandle res = MONO_HANDLE_NEW (MonoReflectionField, mono_object_new_checked (domain, mono_class_get_mono_field_class (), error));
	if (!is_ok (error))
		goto fail;
	MONO_HANDLE_SETVAL (res, klass, MonoClass *, klass);
	MONO_HANDLE_SETVAL (res, field, MonoClassField *, field);
	MonoStringHandle name = mono_string_new_handle (domain, mono_field_get_name (field), error);
	if (!is_ok (error))
		goto fail;
	MONO_HANDLE_SET (res, name, name);

	if (field->type) {
		MonoReflectionTypeHandle rt = mono_type_get_object_handle (domain, field->type, error);
		if (!is_ok (error))
			goto fail;

		MONO_HANDLE_SET (res, type, rt);
	}
	MONO_HANDLE_SETVAL (res, attrs, guint32, mono_field_get_flags (field));
	return res;
fail:
	return MONO_HANDLE_CAST (MonoReflectionField, NULL_HANDLE);
}

/*
 * mono_field_get_object_handle:
 * @domain: an app domain
 * @klass: a type
 * @field: a field
 * @error: set on error
 *
 * Return an System.Reflection.MonoField object representing the field @field
 * in class @klass. On error, returns NULL and sets @error.
 */
MonoReflectionFieldHandle
mono_field_get_object_handle (MonoDomain *domain, MonoClass *klass, MonoClassField *field, MonoError *error)
{
	mono_error_init (error);
	return CHECK_OR_CONSTRUCT_HANDLE (MonoReflectionFieldHandle, field, klass, field_object_construct, NULL);
}


/*
 * mono_field_get_object_checked:
 * @domain: an app domain
 * @klass: a type
 * @field: a field
 * @error: set on error
 *
 * Return an System.Reflection.MonoField object representing the field @field
 * in class @klass. On error, returns NULL and sets @error.
 */
MonoReflectionField*
mono_field_get_object_checked (MonoDomain *domain, MonoClass *klass, MonoClassField *field, MonoError *error)
{
	HANDLE_FUNCTION_ENTER ();
	MonoReflectionFieldHandle result = mono_field_get_object_handle (domain, klass, field, error);
	HANDLE_FUNCTION_RETURN_OBJ (result);
}

/*
 * mono_property_get_object:
 * @domain: an app domain
 * @klass: a type
 * @property: a property
 *
 * Return an System.Reflection.MonoProperty object representing the property @property
 * in class @klass.
 */
MonoReflectionProperty*
mono_property_get_object (MonoDomain *domain, MonoClass *klass, MonoProperty *property)
{
	HANDLE_FUNCTION_ENTER ();
	MonoError error;
	MonoReflectionPropertyHandle result = mono_property_get_object_handle (domain, klass, property, &error);
	mono_error_cleanup (&error);
	HANDLE_FUNCTION_RETURN_OBJ (result);
}

static MonoReflectionPropertyHandle
property_object_construct (MonoDomain *domain, MonoClass *klass, MonoProperty *property, gpointer user_data, MonoError *error)
{
	mono_error_init (error);

	MonoReflectionPropertyHandle res = MONO_HANDLE_NEW (MonoReflectionProperty, mono_object_new_checked (domain, mono_class_get_mono_property_class (), error));
	if (!is_ok (error))
		goto fail;
	MONO_HANDLE_SETVAL (res, klass, MonoClass *, klass);
	MONO_HANDLE_SETVAL (res, property, MonoProperty *, property);
	return res;
fail:
	return MONO_HANDLE_CAST (MonoReflectionProperty, NULL_HANDLE);
}

/**
 * mono_property_get_object_handle:
 * @domain: an app domain
 * @klass: a type
 * @property: a property
 * @error: set on error
 *
 * Return an System.Reflection.MonoProperty object representing the property @property
 * in class @klass.  On error returns NULL and sets @error.
 */
MonoReflectionPropertyHandle
mono_property_get_object_handle (MonoDomain *domain, MonoClass *klass, MonoProperty *property, MonoError *error)
{
	return CHECK_OR_CONSTRUCT_HANDLE (MonoReflectionPropertyHandle, property, klass, property_object_construct, NULL);
}

/**
 * mono_property_get_object:
 * @domain: an app domain
 * @klass: a type
 * @property: a property
 * @error: set on error
 *
 * Return an System.Reflection.MonoProperty object representing the property @property
 * in class @klass.  On error returns NULL and sets @error.
 */
MonoReflectionProperty*
mono_property_get_object_checked (MonoDomain *domain, MonoClass *klass, MonoProperty *property, MonoError *error)
{
	HANDLE_FUNCTION_ENTER ();
	MonoReflectionPropertyHandle res = mono_property_get_object_handle (domain, klass, property, error);
	HANDLE_FUNCTION_RETURN_OBJ (res);
}

/*
 * mono_event_get_object:
 * @domain: an app domain
 * @klass: a type
 * @event: a event
 *
 * Return an System.Reflection.MonoEvent object representing the event @event
 * in class @klass.
 */
MonoReflectionEvent*
mono_event_get_object (MonoDomain *domain, MonoClass *klass, MonoEvent *event)
{
	HANDLE_FUNCTION_ENTER ();
	MonoError error;
	MonoReflectionEventHandle result = mono_event_get_object_handle (domain, klass, event, &error);
	mono_error_cleanup (&error);
	HANDLE_FUNCTION_RETURN_OBJ (result);
}

static MonoReflectionEventHandle
event_object_construct (MonoDomain *domain, MonoClass *klass, MonoEvent *event, gpointer user_data, MonoError *error)
{

	mono_error_init (error);
	MonoReflectionMonoEventHandle mono_event = MONO_HANDLE_NEW (MonoReflectionMonoEvent, mono_object_new_checked (domain, mono_class_get_mono_event_class (), error));
	if (!is_ok (error))
		return MONO_HANDLE_CAST (MonoReflectionEvent, NULL_HANDLE);
	MONO_HANDLE_SETVAL (mono_event, klass, MonoClass* , klass);
	MONO_HANDLE_SETVAL (mono_event, event, MonoEvent* , event);
	return MONO_HANDLE_CAST (MonoReflectionEvent, mono_event);
}

/**
 * mono_event_get_object_handle:
 * @domain: an app domain
 * @klass: a type
 * @event: a event
 * @error: set on error
 *
 * Return an System.Reflection.MonoEvent object representing the event @event
 * in class @klass. On failure sets @error and returns NULL
 */
MonoReflectionEventHandle
mono_event_get_object_handle (MonoDomain *domain, MonoClass *klass, MonoEvent *event, MonoError *error)
{
	mono_error_init (error);
	return CHECK_OR_CONSTRUCT_HANDLE (MonoReflectionEventHandle, event, klass, event_object_construct, NULL);
}


/**
 * mono_get_reflection_missing_object:
 * @domain: Domain where the object lives
 *
 * Returns the System.Reflection.Missing.Value singleton object
 * (of type System.Reflection.Missing).
 *
 * Used as the value for ParameterInfo.DefaultValue when Optional
 * is present
 */
static MonoObjectHandle
mono_get_reflection_missing_object (MonoDomain *domain)
{
	MonoError error;
	static MonoClassField *missing_value_field = NULL;
	
	if (!missing_value_field) {
		MonoClass *missing_klass;
		missing_klass = mono_class_get_missing_class ();
		mono_class_init (missing_klass);
		missing_value_field = mono_class_get_field_from_name (missing_klass, "Value");
		g_assert (missing_value_field);
	}
	/* FIXME change mono_field_get_value_object_checked to return a handle */
	MonoObjectHandle obj = MONO_HANDLE_NEW (MonoObject, mono_field_get_value_object_checked (domain, missing_value_field, NULL, &error));
	mono_error_assert_ok (&error);
	return obj;
}

static MonoObjectHandle
get_dbnull_object (MonoDomain *domain, MonoError *error)
{
	static MonoClassField *dbnull_value_field = NULL;

	mono_error_init (error);

	if (!dbnull_value_field) {
		MonoClass *dbnull_klass;
		dbnull_klass = mono_class_get_dbnull_class ();
		dbnull_value_field = mono_class_get_field_from_name (dbnull_klass, "Value");
		g_assert (dbnull_value_field);
	}
	/* FIXME change mono_field_get_value_object_checked to return a handle */
	MonoObjectHandle obj = MONO_HANDLE_NEW (MonoObject, mono_field_get_value_object_checked (domain, dbnull_value_field, NULL, error));
	return obj;
}

static MonoObjectHandle
get_dbnull (MonoDomain *domain, MonoObjectHandle dbnull, MonoError *error)
{
	mono_error_init (error);
	if (MONO_HANDLE_IS_NULL (dbnull))
		MONO_HANDLE_ASSIGN (dbnull, get_dbnull_object (domain, error));
	return dbnull;
}

static MonoObjectHandle
get_reflection_missing (MonoDomain *domain, MonoObjectHandleOut reflection_missing)
{
	if (MONO_HANDLE_IS_NULL (reflection_missing))
		MONO_HANDLE_ASSIGN (reflection_missing, mono_get_reflection_missing_object (domain));
	return reflection_missing;
}

static gboolean
add_parameter_object_to_array (MonoDomain *domain, MonoMethod *method, MonoObjectHandle member, int idx, const char *name, MonoType *sig_param, guint32 blob_type_enum, const char *blob, MonoMarshalSpec *mspec, MonoObjectHandle missing, MonoObjectHandle dbnull, MonoArrayHandle dest,  MonoError *error)
{
	HANDLE_FUNCTION_ENTER ();
	mono_error_init (error);
	MonoReflectionParameterHandle param = MONO_HANDLE_NEW (MonoReflectionParameter, mono_object_new_checked (domain, mono_class_get_mono_parameter_info_class (), error));
	if (!is_ok (error))
		goto leave;

	MonoReflectionTypeHandle rt = mono_type_get_object_handle (domain, sig_param, error);
	if (!is_ok (error))
		goto leave;

	MONO_HANDLE_SET (param, ClassImpl, rt);

	MONO_HANDLE_SET (param, MemberImpl, member);

	MonoStringHandle name_str = mono_string_new_handle (domain, name, error);
	if (!is_ok (error))
		goto leave;

	MONO_HANDLE_SET (param, NameImpl, name_str);

	MONO_HANDLE_SETVAL (param, PositionImpl, gint32, idx);

	MONO_HANDLE_SETVAL (param, AttrsImpl, guint32, sig_param->attrs);

	if (!(sig_param->attrs & PARAM_ATTRIBUTE_HAS_DEFAULT)) {
		if (sig_param->attrs & PARAM_ATTRIBUTE_OPTIONAL)
			MONO_HANDLE_SET (param, DefaultValueImpl, get_reflection_missing (domain, missing));
		else
			MONO_HANDLE_SET (param, DefaultValueImpl, get_dbnull (domain, dbnull, error));
		if (!is_ok (error))
			goto leave;
	} else {

		MonoType blob_type;

		blob_type.type = (MonoTypeEnum)blob_type_enum;
		blob_type.data.klass = NULL;
		if (blob_type_enum == MONO_TYPE_CLASS)
			blob_type.data.klass = mono_defaults.object_class;
		else if ((sig_param->type == MONO_TYPE_VALUETYPE) && sig_param->data.klass->enumtype) {
			/* For enums, types [i] contains the base type */

			blob_type.type = MONO_TYPE_VALUETYPE;
			blob_type.data.klass = mono_class_from_mono_type (sig_param);
		} else
			blob_type.data.klass = mono_class_from_mono_type (&blob_type);

		MonoObjectHandle default_val_obj = MONO_HANDLE_NEW (MonoObject, mono_get_object_from_blob (domain, &blob_type, blob, error)); /* FIXME make mono_get_object_from_blob return a handle */
		if (!is_ok (error))
			goto leave;
		MONO_HANDLE_SET (param, DefaultValueImpl, default_val_obj);

		/* Type in the Constant table is MONO_TYPE_CLASS for nulls */
		if (blob_type_enum != MONO_TYPE_CLASS && MONO_HANDLE_IS_NULL(default_val_obj)) {
			if (sig_param->attrs & PARAM_ATTRIBUTE_OPTIONAL)
				MONO_HANDLE_SET (param, DefaultValueImpl, get_reflection_missing (domain, missing));
			else
				MONO_HANDLE_SET (param, DefaultValueImpl, get_dbnull (domain, dbnull, error));
			if (!is_ok (error))
				goto leave;
		}
	}

	if (mspec) {
		MonoReflectionMarshalAsAttributeHandle mobj = mono_reflection_marshal_as_attribute_from_marshal_spec (domain, method->klass, mspec, error);
		if (!is_ok (error))
			goto leave;
		MONO_HANDLE_SET (param, MarshalAsImpl, mobj);
	}

	MONO_HANDLE_ARRAY_SETREF (dest, idx, param);

leave:
	HANDLE_FUNCTION_RETURN_VAL (is_ok (error));
}

static MonoArrayHandle
param_objects_construct (MonoDomain *domain, MonoClass *refclass, MonoMethodSignature **addr_of_sig, gpointer user_data, MonoError *error)
{
	MonoMethod *method = (MonoMethod*)user_data;
	MonoMethodSignature *sig = *addr_of_sig; /* see note in mono_param_get_objects_internal */

	MonoArrayHandle res = MONO_HANDLE_NEW (MonoArray, NULL);
	char **names = NULL, **blobs = NULL;
	guint32 *types = NULL;
	MonoMarshalSpec **mspecs = NULL;
	int i;

	mono_error_init (error);
	
	MonoReflectionMethodHandle member = mono_method_get_object_handle (domain, method, refclass, error);
	if (!is_ok (error))
		goto leave;
	names = g_new (char *, sig->param_count);
	mono_method_get_param_names (method, (const char **) names);

	mspecs = g_new (MonoMarshalSpec*, sig->param_count + 1);
	mono_method_get_marshal_info (method, mspecs);

	res = mono_array_new_handle (domain, mono_class_get_mono_parameter_info_class (), sig->param_count, error);
	if (!res)
		goto leave;

	gboolean any_default_value = FALSE;
	for (i = 0; i < sig->param_count; ++i) {
		if ((sig->params [i]->attrs & PARAM_ATTRIBUTE_HAS_DEFAULT) != 0) {
			any_default_value = TRUE;
			break;
		}
	}
	if (any_default_value) {
		blobs = g_new0 (char *, sig->param_count);
		types = g_new0 (guint32, sig->param_count);
		get_default_param_value_blobs (method, blobs, types);
	}

	/* Handles missing and dbnull are assigned in add_parameter_object_to_array when needed */
	MonoObjectHandle missing = MONO_HANDLE_NEW (MonoObject, NULL);
	MonoObjectHandle dbnull = MONO_HANDLE_NEW (MonoObject, NULL);
	for (i = 0; i < sig->param_count; ++i) {
		if (!add_parameter_object_to_array (domain, method, MONO_HANDLE_CAST(MonoObject, member), i, names[i], sig->params[i], types ? types[i] : 0, blobs ? blobs[i] : NULL, mspecs [i + 1], missing, dbnull, res, error))
			goto leave;
	}

leave:
	g_free (names);
	g_free (blobs);
	g_free (types);

	if (sig && mspecs) {
		for (i = sig->param_count; i >= 0; i--) {
			if (mspecs [i])
				mono_metadata_free_marshal_spec (mspecs [i]);
		}
	}
	g_free (mspecs);

	if (!is_ok (error))
		return NULL;
	
	return res;
}

/*
 * mono_param_get_objects:
 * @domain: an app domain
 * @method: a method
 *
 * Return an System.Reflection.ParameterInfo array object representing the parameters
 * in the method @method.
 */
MonoArrayHandle
mono_param_get_objects_internal (MonoDomain *domain, MonoMethod *method, MonoClass *refclass, MonoError *error)
{
	mono_error_init (error);

	/* side-effect: sets method->signature non-NULL on success */
	MonoMethodSignature *sig = mono_method_signature_checked (method, error);
	if (!is_ok (error))
		goto fail;

	if (!sig->param_count) {
		MonoArrayHandle res = mono_array_new_handle (domain, mono_class_get_mono_parameter_info_class (), 0, error);
		if (!is_ok (error))
			goto fail;

		return res;
	}

	/* Note: the cache is based on the address of the signature into the method
	 * since we already cache MethodInfos with the method as keys.
	 */
	return CHECK_OR_CONSTRUCT_HANDLE (MonoArrayHandle, &method->signature, refclass, param_objects_construct, method);
fail:
	return MONO_HANDLE_NEW (MonoArray, NULL_HANDLE);
}

MonoArray*
mono_param_get_objects (MonoDomain *domain, MonoMethod *method)
{
	HANDLE_FUNCTION_ENTER ();
	MonoError error;
	MonoArrayHandle result = mono_param_get_objects_internal (domain, method, NULL, &error);
	mono_error_assert_ok (&error);
	HANDLE_FUNCTION_RETURN_OBJ (result);
}

static gboolean
add_local_var_info_to_array (MonoDomain *domain, MonoMethodHeader *header, int idx, MonoArrayHandle dest, MonoError *error)
{
	HANDLE_FUNCTION_ENTER ();
	mono_error_init (error);
	MonoReflectionLocalVariableInfoHandle info = MONO_HANDLE_NEW (MonoReflectionLocalVariableInfo, mono_object_new_checked (domain, mono_class_get_local_variable_info_class (), error));
	if (!is_ok (error))
		goto leave;

	MonoReflectionTypeHandle rt = mono_type_get_object_handle (domain, header->locals [idx], error);
	if (!is_ok (error))
		goto leave;

	MONO_HANDLE_SET (info, local_type, rt);

	MONO_HANDLE_SETVAL (info, is_pinned, MonoBoolean, header->locals [idx]->pinned);
	MONO_HANDLE_SETVAL (info, local_index, guint16, idx);

	MONO_HANDLE_ARRAY_SETREF (dest, idx, info);

leave:
	HANDLE_FUNCTION_RETURN_VAL (is_ok (error));
}

static gboolean
add_exception_handling_clause_to_array (MonoDomain *domain, MonoMethodHeader *header, int idx, MonoArrayHandle dest, MonoError *error)
{
	HANDLE_FUNCTION_ENTER ();
	mono_error_init (error);
	MonoReflectionExceptionHandlingClauseHandle info = MONO_HANDLE_NEW (MonoReflectionExceptionHandlingClause, mono_object_new_checked (domain, mono_class_get_exception_handling_clause_class (), error));
	if (!is_ok (error))
		goto leave;
	MonoExceptionClause *clause = &header->clauses [idx];

	MONO_HANDLE_SETVAL (info, flags, gint32, clause->flags);
	MONO_HANDLE_SETVAL (info, try_offset, gint32, clause->try_offset);
	MONO_HANDLE_SETVAL (info, try_length, gint32, clause->try_len);
	MONO_HANDLE_SETVAL (info, handler_offset, gint32, clause->handler_offset);
	MONO_HANDLE_SETVAL (info, handler_length, gint32, clause->handler_len);
	if (clause->flags == MONO_EXCEPTION_CLAUSE_FILTER)
		MONO_HANDLE_SETVAL (info, filter_offset, gint32, clause->data.filter_offset);
	else if (clause->data.catch_class) {
		MonoReflectionTypeHandle rt = mono_type_get_object_handle (mono_domain_get (), &clause->data.catch_class->byval_arg, error);
		if (!is_ok (error))
			goto leave;

		MONO_HANDLE_SET (info, catch_type, rt);
	}

	MONO_HANDLE_ARRAY_SETREF (dest, idx, info);
leave:
	HANDLE_FUNCTION_RETURN_VAL (is_ok (error));
}

/*
 * mono_method_body_get_object:
 * @domain: an app domain
 * @method: a method
 *
 * Return an System.Reflection.MethodBody object representing the method @method.
 */
MonoReflectionMethodBody*
mono_method_body_get_object (MonoDomain *domain, MonoMethod *method)
{
	HANDLE_FUNCTION_ENTER ();
	MonoError error;
	MonoReflectionMethodBodyHandle result = mono_method_body_get_object_handle (domain, method, &error);
	mono_error_cleanup (&error);
	HANDLE_FUNCTION_RETURN_OBJ (result);
}

static MonoReflectionMethodBodyHandle
method_body_object_construct (MonoDomain *domain, MonoClass *unused_class, MonoMethod *method, gpointer user_data, MonoError *error)
{
	MonoMethodHeader *header = NULL;
	MonoImage *image;
	guint32 method_rva, local_var_sig_token;
	char *ptr;
	unsigned char format, flags;
	int i;

	mono_error_init (error);

	/* for compatibility with .net */
	if (method_is_dynamic (method)) {
		mono_error_set_generic_error (error, "System", "InvalidOperationException", "");
		goto fail;
	}

	if ((method->flags & METHOD_ATTRIBUTE_PINVOKE_IMPL) ||
		(method->flags & METHOD_ATTRIBUTE_ABSTRACT) ||
	    (method->iflags & METHOD_IMPL_ATTRIBUTE_INTERNAL_CALL) ||
		(method->klass->image->raw_data && method->klass->image->raw_data [1] != 'Z') ||
	    (method->iflags & METHOD_IMPL_ATTRIBUTE_RUNTIME))
		return MONO_HANDLE_CAST (MonoReflectionMethodBody, NULL_HANDLE);

	image = method->klass->image;
	header = mono_method_get_header_checked (method, error);
	if (!is_ok (error))
		goto fail;

	if (!image_is_dynamic (image)) {
		/* Obtain local vars signature token */
		method_rva = mono_metadata_decode_row_col (&image->tables [MONO_TABLE_METHOD], mono_metadata_token_index (method->token) - 1, MONO_METHOD_RVA);
		ptr = mono_image_rva_map (image, method_rva);
		flags = *(const unsigned char *) ptr;
		format = flags & METHOD_HEADER_FORMAT_MASK;
		switch (format){
		case METHOD_HEADER_TINY_FORMAT:
			local_var_sig_token = 0;
			break;
		case METHOD_HEADER_FAT_FORMAT:
			ptr += 2;
			ptr += 2;
			ptr += 4;
			local_var_sig_token = read32 (ptr);
			break;
		default:
			g_assert_not_reached ();
		}
	} else
		local_var_sig_token = 0; //FIXME

	MonoReflectionMethodBodyHandle ret = MONO_HANDLE_NEW (MonoReflectionMethodBody, mono_object_new_checked (domain, mono_class_get_method_body_class (), error));
	if (!is_ok (error))
		goto fail;

	MONO_HANDLE_SETVAL (ret, init_locals, MonoBoolean, header->init_locals);
	MONO_HANDLE_SETVAL (ret, max_stack, guint32, header->max_stack);
	MONO_HANDLE_SETVAL (ret, local_var_sig_token, guint32, local_var_sig_token);
	MonoArrayHandle il_arr = mono_array_new_handle (domain, mono_defaults.byte_class, header->code_size, error);
	if (!is_ok (error))
		goto fail;
	MONO_HANDLE_SET (ret, il, il_arr);
	uint32_t il_gchandle;
	guint8* il_data = MONO_ARRAY_HANDLE_PIN (il_arr, guint8, 0, &il_gchandle);
	memcpy (il_data, header->code, header->code_size);
	mono_gchandle_free (il_gchandle);

	/* Locals */
	MonoArrayHandle locals_arr = mono_array_new_handle (domain, mono_class_get_local_variable_info_class (), header->num_locals, error);
	if (!is_ok (error))
		goto fail;
	MONO_HANDLE_SET (ret, locals, locals_arr);
	for (i = 0; i < header->num_locals; ++i) {
		if (!add_local_var_info_to_array (domain, header, i, locals_arr, error))
			goto fail;
	}

	/* Exceptions */
	MonoArrayHandle exn_clauses = mono_array_new_handle (domain, mono_class_get_exception_handling_clause_class (), header->num_clauses, error);
	if (!is_ok (error))
		goto fail;
	MONO_HANDLE_SET (ret, clauses, exn_clauses);
	for (i = 0; i < header->num_clauses; ++i) {
		if (!add_exception_handling_clause_to_array (domain, header, i, exn_clauses, error))
			goto fail;
	}

	mono_metadata_free_mh (header);
	return ret;
fail:
	if (header)
		mono_metadata_free_mh (header);
	return NULL;
}

/**
 * mono_method_body_get_object_handle:
 * @domain: an app domain
 * @method: a method
 * @error: set on error
 *
 * Return an System.Reflection.MethodBody object representing the
 * method @method.  On failure, returns NULL and sets @error.
 */
MonoReflectionMethodBodyHandle
mono_method_body_get_object_handle (MonoDomain *domain, MonoMethod *method, MonoError *error)
{
	mono_error_init (error);
	return CHECK_OR_CONSTRUCT_HANDLE (MonoReflectionMethodBodyHandle, method, NULL, method_body_object_construct, NULL);
}


/**
 * mono_get_dbnull_object:
 * @domain: Domain where the object lives
 *
 * Returns the System.DBNull.Value singleton object
 *
 * Used as the value for ParameterInfo.DefaultValue 
 */
MonoObject *
mono_get_dbnull_object (MonoDomain *domain)
{
	HANDLE_FUNCTION_ENTER ();
	MonoError error;
	MonoObjectHandle obj = get_dbnull_object (domain, &error);
	mono_error_assert_ok (&error);
	HANDLE_FUNCTION_RETURN_OBJ (obj);
}

static void
get_default_param_value_blobs (MonoMethod *method, char **blobs, guint32 *types)
{
	guint32 param_index, i, lastp, crow = 0;
	guint32 param_cols [MONO_PARAM_SIZE], const_cols [MONO_CONSTANT_SIZE];
	gint32 idx;

	MonoClass *klass = method->klass;
	MonoImage *image = klass->image;
	MonoMethodSignature *methodsig = mono_method_signature (method);

	MonoTableInfo *constt;
	MonoTableInfo *methodt;
	MonoTableInfo *paramt;

	if (!methodsig->param_count)
		return;

	mono_class_init (klass);

	if (image_is_dynamic (klass->image)) {
		MonoReflectionMethodAux *aux;
		if (method->is_inflated)
			method = ((MonoMethodInflated*)method)->declaring;
		aux = (MonoReflectionMethodAux *)g_hash_table_lookup (((MonoDynamicImage*)method->klass->image)->method_aux_hash, method);
		if (aux && aux->param_defaults) {
			memcpy (blobs, &(aux->param_defaults [1]), methodsig->param_count * sizeof (char*));
			memcpy (types, &(aux->param_default_types [1]), methodsig->param_count * sizeof (guint32));
		}
		return;
	}

	methodt = &klass->image->tables [MONO_TABLE_METHOD];
	paramt = &klass->image->tables [MONO_TABLE_PARAM];
	constt = &image->tables [MONO_TABLE_CONSTANT];

	idx = mono_method_get_index (method) - 1;
	g_assert (idx != -1);

	param_index = mono_metadata_decode_row_col (methodt, idx, MONO_METHOD_PARAMLIST);
	if (idx + 1 < methodt->rows)
		lastp = mono_metadata_decode_row_col (methodt, idx + 1, MONO_METHOD_PARAMLIST);
	else
		lastp = paramt->rows + 1;

	for (i = param_index; i < lastp; ++i) {
		guint32 paramseq;

		mono_metadata_decode_row (paramt, i - 1, param_cols, MONO_PARAM_SIZE);
		paramseq = param_cols [MONO_PARAM_SEQUENCE];

		if (!(param_cols [MONO_PARAM_FLAGS] & PARAM_ATTRIBUTE_HAS_DEFAULT))
			continue;

		crow = mono_metadata_get_constant_index (image, MONO_TOKEN_PARAM_DEF | i, crow + 1);
		if (!crow) {
			continue;
		}
	
		mono_metadata_decode_row (constt, crow - 1, const_cols, MONO_CONSTANT_SIZE);
		blobs [paramseq - 1] = (char *)mono_metadata_blob_heap (image, const_cols [MONO_CONSTANT_VALUE]);
		types [paramseq - 1] = const_cols [MONO_CONSTANT_TYPE];
	}

	return;
}

MonoObject *
mono_get_object_from_blob (MonoDomain *domain, MonoType *type, const char *blob, MonoError *error)
{
	void *retval;
	MonoClass *klass;
	MonoObject *object;
	MonoType *basetype = type;

	mono_error_init (error);

	if (!blob)
		return NULL;
	
	klass = mono_class_from_mono_type (type);
	if (klass->valuetype) {
		object = mono_object_new_checked (domain, klass, error);
		return_val_if_nok (error, NULL);
		retval = ((gchar *) object + sizeof (MonoObject));
		if (klass->enumtype)
			basetype = mono_class_enum_basetype (klass);
	} else {
		retval = &object;
	}
			
	if (!mono_get_constant_value_from_blob (domain, basetype->type,  blob, retval, error))
		return object;
	else
		return NULL;
}

static int
assembly_name_to_aname (MonoAssemblyName *assembly, char *p) {
	int found_sep;
	char *s;
	gboolean quoted = FALSE;

	memset (assembly, 0, sizeof (MonoAssemblyName));
	assembly->culture = "";
	memset (assembly->public_key_token, 0, MONO_PUBLIC_KEY_TOKEN_LENGTH);

	if (*p == '"') {
		quoted = TRUE;
		p++;
	}
	assembly->name = p;
	while (*p && (isalnum (*p) || *p == '.' || *p == '-' || *p == '_' || *p == '$' || *p == '@' || g_ascii_isspace (*p)))
		p++;
	if (quoted) {
		if (*p != '"')
			return 1;
		*p = 0;
		p++;
	}
	if (*p != ',')
		return 1;
	*p = 0;
	/* Remove trailing whitespace */
	s = p - 1;
	while (*s && g_ascii_isspace (*s))
		*s-- = 0;
	p ++;
	while (g_ascii_isspace (*p))
		p++;
	while (*p) {
		if (*p == 'V' && g_ascii_strncasecmp (p, "Version=", 8) == 0) {
			p += 8;
			assembly->major = strtoul (p, &s, 10);
			if (s == p || *s != '.')
				return 1;
			p = ++s;
			assembly->minor = strtoul (p, &s, 10);
			if (s == p || *s != '.')
				return 1;
			p = ++s;
			assembly->build = strtoul (p, &s, 10);
			if (s == p || *s != '.')
				return 1;
			p = ++s;
			assembly->revision = strtoul (p, &s, 10);
			if (s == p)
				return 1;
			p = s;
		} else if (*p == 'C' && g_ascii_strncasecmp (p, "Culture=", 8) == 0) {
			p += 8;
			if (g_ascii_strncasecmp (p, "neutral", 7) == 0) {
				assembly->culture = "";
				p += 7;
			} else {
				assembly->culture = p;
				while (*p && *p != ',') {
					p++;
				}
			}
		} else if (*p == 'P' && g_ascii_strncasecmp (p, "PublicKeyToken=", 15) == 0) {
			p += 15;
			if (strncmp (p, "null", 4) == 0) {
				p += 4;
			} else {
				int len;
				gchar *start = p;
				while (*p && *p != ',') {
					p++;
				}
				len = (p - start + 1);
				if (len > MONO_PUBLIC_KEY_TOKEN_LENGTH)
					len = MONO_PUBLIC_KEY_TOKEN_LENGTH;
				g_strlcpy ((char*)assembly->public_key_token, start, len);
			}
		} else {
			while (*p && *p != ',')
				p++;
		}
		found_sep = 0;
		while (g_ascii_isspace (*p) || *p == ',') {
			*p++ = 0;
			found_sep = 1;
			continue;
		}
		/* failed */
		if (!found_sep)
			return 1;
	}

	return 0;
}

/*
 * mono_reflection_parse_type:
 * @name: type name
 *
 * Parse a type name as accepted by the GetType () method and output the info
 * extracted in the info structure.
 * the name param will be mangled, so, make a copy before passing it to this function.
 * The fields in info will be valid until the memory pointed to by name is valid.
 *
 * See also mono_type_get_name () below.
 *
 * Returns: 0 on parse error.
 */
static int
_mono_reflection_parse_type (char *name, char **endptr, gboolean is_recursed,
			     MonoTypeNameParse *info)
{
	char *start, *p, *w, *last_point, *startn;
	int in_modifiers = 0;
	int isbyref = 0, rank = 0, isptr = 0;

	start = p = w = name;

	//FIXME could we just zero the whole struct? memset (&info, 0, sizeof (MonoTypeNameParse))
	memset (&info->assembly, 0, sizeof (MonoAssemblyName));
	info->name = info->name_space = NULL;
	info->nested = NULL;
	info->modifiers = NULL;
	info->type_arguments = NULL;

	/* last_point separates the namespace from the name */
	last_point = NULL;
	/* Skips spaces */
	while (*p == ' ') p++, start++, w++, name++;

	while (*p) {
		switch (*p) {
		case '+':
			*p = 0; /* NULL terminate the name */
			startn = p + 1;
			info->nested = g_list_append (info->nested, startn);
			/* we have parsed the nesting namespace + name */
			if (info->name)
				break;
			if (last_point) {
				info->name_space = start;
				*last_point = 0;
				info->name = last_point + 1;
			} else {
				info->name_space = (char *)"";
				info->name = start;
			}
			break;
		case '.':
			last_point = p;
			break;
		case '\\':
			++p;
			break;
		case '&':
		case '*':
		case '[':
		case ',':
		case ']':
			in_modifiers = 1;
			break;
		default:
			break;
		}
		if (in_modifiers)
			break;
		// *w++ = *p++;
		p++;
	}
	
	if (!info->name) {
		if (last_point) {
			info->name_space = start;
			*last_point = 0;
			info->name = last_point + 1;
		} else {
			info->name_space = (char *)"";
			info->name = start;
		}
	}
	while (*p) {
		switch (*p) {
		case '&':
			if (isbyref) /* only one level allowed by the spec */
				return 0;
			isbyref = 1;
			isptr = 0;
			info->modifiers = g_list_append (info->modifiers, GUINT_TO_POINTER (0));
			*p++ = 0;
			break;
		case '*':
			if (isbyref) /* pointer to ref not okay */
				return 0;
			info->modifiers = g_list_append (info->modifiers, GUINT_TO_POINTER (-1));
			isptr = 1;
			*p++ = 0;
			break;
		case '[':
			if (isbyref) /* array of ref and generic ref are not okay */
				return 0;
			//Decide if it's an array of a generic argument list
			*p++ = 0;

			if (!*p) //XXX test
				return 0;
			if (*p  == ',' || *p == '*' || *p == ']') { //array
				isptr = 0;
				rank = 1;
				while (*p) {
					if (*p == ']')
						break;
					if (*p == ',')
						rank++;
					else if (*p == '*') /* '*' means unknown lower bound */
						info->modifiers = g_list_append (info->modifiers, GUINT_TO_POINTER (-2));
					else
						return 0;
					++p;
				}
				if (*p++ != ']')
					return 0;
				info->modifiers = g_list_append (info->modifiers, GUINT_TO_POINTER (rank));
			} else {
				if (rank || isptr) /* generic args after array spec or ptr*/ //XXX test
					return 0;
				isptr = 0;
				info->type_arguments = g_ptr_array_new ();
				while (*p) {
					MonoTypeNameParse *subinfo = g_new0 (MonoTypeNameParse, 1);
					gboolean fqname = FALSE;

					g_ptr_array_add (info->type_arguments, subinfo);

					while (*p == ' ') p++;
					if (*p == '[') {
						p++;
						fqname = TRUE;
					}

					if (!_mono_reflection_parse_type (p, &p, TRUE, subinfo))
						return 0;

					/*MS is lenient on [] delimited parameters that aren't fqn - and F# uses them.*/
					if (fqname && (*p != ']')) {
						char *aname;

						if (*p != ',')
							return 0;
						*p++ = 0;

						aname = p;
						while (*p && (*p != ']'))
							p++;

						if (*p != ']')
							return 0;

						*p++ = 0;
						while (*aname) {
							if (g_ascii_isspace (*aname)) {
								++aname;
								continue;
							}
							break;
						}
						if (!*aname ||
						    !assembly_name_to_aname (&subinfo->assembly, aname))
							return 0;
					} else if (fqname && (*p == ']')) {
						*p++ = 0;
					}
					if (*p == ']') {
						*p++ = 0;
						break;
					} else if (!*p) {
						return 0;
					}
					*p++ = 0;
				}
			}
			break;
		case ']':
			if (is_recursed)
				goto end;
			return 0;
		case ',':
			if (is_recursed)
				goto end;
			*p++ = 0;
			while (*p) {
				if (g_ascii_isspace (*p)) {
					++p;
					continue;
				}
				break;
			}
			if (!*p)
				return 0; /* missing assembly name */
			if (!assembly_name_to_aname (&info->assembly, p))
				return 0;
			break;
		default:
			return 0;
		}
		if (info->assembly.name)
			break;
	}
	// *w = 0; /* terminate class name */
 end:
	if (!info->name || !*info->name)
		return 0;
	if (endptr)
		*endptr = p;
	/* add other consistency checks */
	return 1;
}


/**
 * mono_identifier_unescape_type_name_chars:
 * @identifier: the display name of a mono type
 *
 * Returns:
 *  The name in internal form, that is without escaping backslashes.
 *
 *  The string is modified in place!
 */
char*
mono_identifier_unescape_type_name_chars(char* identifier)
{
	char *w, *r;
	if (!identifier)
		return NULL;
	for (w = r = identifier; *r != 0; r++)
	{
		char c = *r;
		if (c == '\\') {
			r++;
			if (*r == 0)
				break;
			c = *r;
		}
		*w = c;
		w++;
	}
	if (w != r)
		*w = 0;
	return identifier;
}

void
mono_identifier_unescape_info (MonoTypeNameParse* info);

static void
unescape_each_type_argument(void* data, void* user_data)
{
	MonoTypeNameParse* info = (MonoTypeNameParse*)data;
	mono_identifier_unescape_info (info);
}

static void
unescape_each_nested_name (void* data, void* user_data)
{
	char* nested_name = (char*) data;
	mono_identifier_unescape_type_name_chars(nested_name);
}

/**
 * mono_identifier_unescape_info:
 *
 * @info: a parsed display form of an (optionally assembly qualified) full type name.
 *
 * Returns: nothing.
 *
 * Destructively updates the info by unescaping the identifiers that
 * comprise the type namespace, name, nested types (if any) and
 * generic type arguments (if any).
 *
 * The resulting info has the names in internal form.
 *
 */
void
mono_identifier_unescape_info (MonoTypeNameParse *info)
{
	if (!info)
		return;
	mono_identifier_unescape_type_name_chars(info->name_space);
	mono_identifier_unescape_type_name_chars(info->name);
	// but don't escape info->assembly
	if (info->type_arguments)
		g_ptr_array_foreach(info->type_arguments, &unescape_each_type_argument, NULL);
	if (info->nested)
		g_list_foreach(info->nested, &unescape_each_nested_name, NULL);
}

int
mono_reflection_parse_type (char *name, MonoTypeNameParse *info)
{
	int ok = _mono_reflection_parse_type (name, NULL, FALSE, info);
	if (ok) {
		mono_identifier_unescape_info (info);
	}
	return ok;
}

static MonoType*
_mono_reflection_get_type_from_info (MonoTypeNameParse *info, MonoImage *image, gboolean ignorecase, MonoError *error)
{
	gboolean type_resolve = FALSE;
	MonoType *type;
	MonoImage *rootimage = image;

	mono_error_init (error);

	if (info->assembly.name) {
		MonoAssembly *assembly = mono_assembly_loaded (&info->assembly);
		if (!assembly && image && image->assembly && mono_assembly_names_equal (&info->assembly, &image->assembly->aname))
			/* 
			 * This could happen in the AOT compiler case when the search hook is not
			 * installed.
			 */
			assembly = image->assembly;
		if (!assembly) {
			/* then we must load the assembly ourselve - see #60439 */
			assembly = mono_assembly_load (&info->assembly, image->assembly->basedir, NULL);
			if (!assembly)
				return NULL;
		}
		image = assembly->image;
	} else if (!image) {
		image = mono_defaults.corlib;
	}

	type = mono_reflection_get_type_with_rootimage (rootimage, image, info, ignorecase, &type_resolve, error);
	if (type == NULL && !info->assembly.name && image != mono_defaults.corlib) {
		/* ignore the error and try again */
		mono_error_cleanup (error);
		mono_error_init (error);
		image = mono_defaults.corlib;
		type = mono_reflection_get_type_with_rootimage (rootimage, image, info, ignorecase, &type_resolve, error);
	}

	return type;
}

/**
 * mono_reflection_get_type_internal:
 *
 * Returns: may return NULL on success, sets error on failure.
 */
static MonoType*
mono_reflection_get_type_internal (MonoImage *rootimage, MonoImage* image, MonoTypeNameParse *info, gboolean ignorecase, MonoError *error)
{
	MonoClass *klass;
	GList *mod;
	int modval;
	gboolean bounded = FALSE;
	
	mono_error_init (error);
	if (!image)
		image = mono_defaults.corlib;

	if (!rootimage)
		rootimage = mono_defaults.corlib;

	if (ignorecase)
		klass = mono_class_from_name_case_checked (image, info->name_space, info->name, error);
	else
		klass = mono_class_from_name_checked (image, info->name_space, info->name, error);

	if (!klass)
		return NULL;

	for (mod = info->nested; mod; mod = mod->next) {
		gpointer iter = NULL;
		MonoClass *parent;

		parent = klass;
		mono_class_init (parent);

		while ((klass = mono_class_get_nested_types (parent, &iter))) {
			char *lastp;
			char *nested_name, *nested_nspace;
			gboolean match = TRUE;

			lastp = strrchr ((const char *)mod->data, '.');
			if (lastp) {
				/* Nested classes can have namespaces */
				int nspace_len;

				nested_name = g_strdup (lastp + 1);
				nspace_len = lastp - (char*)mod->data;
				nested_nspace = (char *)g_malloc (nspace_len + 1);
				memcpy (nested_nspace, mod->data, nspace_len);
				nested_nspace [nspace_len] = '\0';

			} else {
				nested_name = (char *)mod->data;
				nested_nspace = NULL;
			}

			if (nested_nspace) {
				if (ignorecase) {
					if (!(klass->name_space && mono_utf8_strcasecmp (klass->name_space, nested_nspace) == 0))
						match = FALSE;
				} else {
					if (!(klass->name_space && strcmp (klass->name_space, nested_nspace) == 0))
						match = FALSE;
				}
			}
			if (match) {
				if (ignorecase) {
					if (mono_utf8_strcasecmp (klass->name, nested_name) != 0)
						match = FALSE;
				} else {
					if (strcmp (klass->name, nested_name) != 0)
						match = FALSE;
				}
			}
			if (lastp) {
				g_free (nested_name);
				g_free (nested_nspace);
			}
			if (match)
				break;
		}

		if (!klass)
			break;
	}
	if (!klass)
		return NULL;

	if (info->type_arguments) {
		MonoType **type_args = g_new0 (MonoType *, info->type_arguments->len);
		MonoReflectionType *the_type;
		MonoType *instance;
		int i;

		for (i = 0; i < info->type_arguments->len; i++) {
			MonoTypeNameParse *subinfo = (MonoTypeNameParse *)g_ptr_array_index (info->type_arguments, i);

			type_args [i] = _mono_reflection_get_type_from_info (subinfo, rootimage, ignorecase, error);
			if (!type_args [i]) {
				g_free (type_args);
				return NULL;
			}
		}

		the_type = mono_type_get_object_checked (mono_domain_get (), &klass->byval_arg, error);
		if (!the_type)
			return NULL;

		instance = mono_reflection_bind_generic_parameters (
			the_type, info->type_arguments->len, type_args, error);

		g_free (type_args);
		if (!instance)
			return NULL;

		klass = mono_class_from_mono_type (instance);
	}

	for (mod = info->modifiers; mod; mod = mod->next) {
		modval = GPOINTER_TO_UINT (mod->data);
		if (!modval) { /* byref: must be last modifier */
			return &klass->this_arg;
		} else if (modval == -1) {
			klass = mono_ptr_class_get (&klass->byval_arg);
		} else if (modval == -2) {
			bounded = TRUE;
		} else { /* array rank */
			klass = mono_bounded_array_class_get (klass, modval, bounded);
		}
	}

	return &klass->byval_arg;
}

/*
 * mono_reflection_get_type:
 * @image: a metadata context
 * @info: type description structure
 * @ignorecase: flag for case-insensitive string compares
 * @type_resolve: whenever type resolve was already tried
 *
 * Build a MonoType from the type description in @info.
 * 
 */

MonoType*
mono_reflection_get_type (MonoImage* image, MonoTypeNameParse *info, gboolean ignorecase, gboolean *type_resolve) {
	MonoError error;
	MonoType *result = mono_reflection_get_type_with_rootimage (image, image, info, ignorecase, type_resolve, &error);
	mono_error_cleanup (&error);
	return result;
}

/**
 * mono_reflection_get_type_checked:
 * @rootimage: the image of the currently active managed caller
 * @image: a metadata context
 * @info: type description structure
 * @ignorecase: flag for case-insensitive string compares
 * @type_resolve: whenever type resolve was already tried
 * @error: set on error.
 *
 * Build a MonoType from the type description in @info. On failure returns NULL and sets @error.
 *
 */
MonoType*
mono_reflection_get_type_checked (MonoImage *rootimage, MonoImage* image, MonoTypeNameParse *info, gboolean ignorecase, gboolean *type_resolve, MonoError *error) {
	mono_error_init (error);
	return mono_reflection_get_type_with_rootimage (rootimage, image, info, ignorecase, type_resolve, error);
}


static MonoType*
module_builder_array_get_type (MonoArrayHandle module_builders, int i, MonoImage *rootimage, MonoTypeNameParse *info, gboolean ignorecase, MonoError *error)
{
	HANDLE_FUNCTION_ENTER ();
	mono_error_init (error);
	MonoType *type = NULL;
	MonoReflectionModuleBuilderHandle mb = MONO_HANDLE_NEW (MonoReflectionModuleBuilder, NULL);
	MONO_HANDLE_ARRAY_GETREF (mb, module_builders, i);
	MonoDynamicImage *dynamic_image = MONO_HANDLE_GETVAL (mb, dynamic_image);
	type = mono_reflection_get_type_internal (rootimage, &dynamic_image->image, info, ignorecase, error);
	HANDLE_FUNCTION_RETURN_VAL (type);
}

static MonoType*
module_array_get_type (MonoArrayHandle modules, int i, MonoImage *rootimage, MonoTypeNameParse *info, gboolean ignorecase, MonoError *error)
{
	HANDLE_FUNCTION_ENTER ();
	mono_error_init (error);
	MonoType *type = NULL;
	MonoReflectionModuleHandle mod = MONO_HANDLE_NEW (MonoReflectionModule, NULL);
	MONO_HANDLE_ARRAY_GETREF (mod, modules, i);
	MonoImage *image = MONO_HANDLE_GETVAL (mod, image);
	type = mono_reflection_get_type_internal (rootimage, image, info, ignorecase, error);
	HANDLE_FUNCTION_RETURN_VAL (type);
}

static MonoType*
mono_reflection_get_type_internal_dynamic (MonoImage *rootimage, MonoAssembly *assembly, MonoTypeNameParse *info, gboolean ignorecase, MonoError *error)
{
	HANDLE_FUNCTION_ENTER ();
	MonoType *type = NULL;
	int i;

	mono_error_init (error);
	g_assert (assembly_is_dynamic (assembly));
	MonoReflectionAssemblyBuilderHandle abuilder = MONO_HANDLE_CAST (MonoReflectionAssemblyBuilder, mono_assembly_get_object_handle (((MonoDynamicAssembly*)assembly)->domain, assembly, error));
	if (!is_ok (error))
		goto leave;

	/* Enumerate all modules */

	MonoArrayHandle modules = MONO_HANDLE_NEW (MonoArray, NULL);
	MONO_HANDLE_GET (modules, abuilder, modules);
	if (!MONO_HANDLE_IS_NULL (modules)) {
		int n = mono_array_handle_length (modules);
		for (i = 0; i < n; ++i) {
			type = module_builder_array_get_type (modules, i, rootimage, info, ignorecase, error);
			if (type)
				break;
			if (!is_ok (error))
				goto leave;
		}
	}

	MonoArrayHandle loaded_modules = MONO_HANDLE_NEW (MonoArray, NULL);
	MONO_HANDLE_GET (loaded_modules, abuilder, loaded_modules);
	if (!type && !MONO_HANDLE_IS_NULL(loaded_modules)) {
		int n = mono_array_handle_length (loaded_modules);
		for (i = 0; i < n; ++i) {
			type = module_array_get_type (loaded_modules, i, rootimage, info, ignorecase, error);
			if (type)
				break;
			if (!is_ok (error))
				goto leave;
		}
	}

leave:
	HANDLE_FUNCTION_RETURN_VAL (type);
}
	
MonoType*
mono_reflection_get_type_with_rootimage (MonoImage *rootimage, MonoImage* image, MonoTypeNameParse *info, gboolean ignorecase, gboolean *type_resolve, MonoError *error)
{
	MonoType *type;
	MonoReflectionAssembly *assembly;
	GString *fullName;
	GList *mod;

	mono_error_init (error);

	if (image && image_is_dynamic (image))
		type = mono_reflection_get_type_internal_dynamic (rootimage, image->assembly, info, ignorecase, error);
	else {
		type = mono_reflection_get_type_internal (rootimage, image, info, ignorecase, error);
	}
	return_val_if_nok (error, NULL);

	if (type)
		return type;
	if (!mono_domain_has_type_resolve (mono_domain_get ()))
		return NULL;

	if (type_resolve) {
		if (*type_resolve) 
			return NULL;
		else
			*type_resolve = TRUE;
	}
	
	/* Reconstruct the type name */
	fullName = g_string_new ("");
	if (info->name_space && (info->name_space [0] != '\0'))
		g_string_printf (fullName, "%s.%s", info->name_space, info->name);
	else
		g_string_printf (fullName, "%s", info->name);
	for (mod = info->nested; mod; mod = mod->next)
		g_string_append_printf (fullName, "+%s", (char*)mod->data);

	assembly = mono_domain_try_type_resolve_checked ( mono_domain_get (), fullName->str, NULL, error);
	if (!is_ok (error)) {
		g_string_free (fullName, TRUE);
		return NULL;
	}

	if (assembly) {
		if (assembly_is_dynamic (assembly->assembly))
			type = mono_reflection_get_type_internal_dynamic (rootimage, assembly->assembly,
									  info, ignorecase, error);
		else
			type = mono_reflection_get_type_internal (rootimage, assembly->assembly->image, 
								  info, ignorecase, error);
	}
	g_string_free (fullName, TRUE);
	return_val_if_nok (error, NULL);
	return type;
}

void
mono_reflection_free_type_info (MonoTypeNameParse *info)
{
	g_list_free (info->modifiers);
	g_list_free (info->nested);

	if (info->type_arguments) {
		int i;

		for (i = 0; i < info->type_arguments->len; i++) {
			MonoTypeNameParse *subinfo = (MonoTypeNameParse *)g_ptr_array_index (info->type_arguments, i);

			mono_reflection_free_type_info (subinfo);
			/*We free the subinfo since it is allocated by _mono_reflection_parse_type*/
			g_free (subinfo);
		}

		g_ptr_array_free (info->type_arguments, TRUE);
	}
}

/*
 * mono_reflection_type_from_name:
 * @name: type name.
 * @image: a metadata context (can be NULL).
 *
 * Retrieves a MonoType from its @name. If the name is not fully qualified,
 * it defaults to get the type from @image or, if @image is NULL or loading
 * from it fails, uses corlib.
 * 
 */
MonoType*
mono_reflection_type_from_name (char *name, MonoImage *image)
{
	MonoError error;
	MonoType  *result = mono_reflection_type_from_name_checked (name, image, &error);
	mono_error_cleanup (&error);
	return result;
}

/**
 * mono_reflection_type_from_name_checked:
 * @name: type name.
 * @image: a metadata context (can be NULL).
 * @error: set on errror.
 *
 * Retrieves a MonoType from its @name. If the name is not fully qualified,
 * it defaults to get the type from @image or, if @image is NULL or loading
 * from it fails, uses corlib.  On failure returns NULL and sets @error.
 * 
 */
MonoType*
mono_reflection_type_from_name_checked (char *name, MonoImage *image, MonoError *error)
{
	MonoType *type = NULL;
	MonoTypeNameParse info;
	char *tmp;

	mono_error_init (error);
	/* Make a copy since parse_type modifies its argument */
	tmp = g_strdup (name);
	
	/*g_print ("requested type %s\n", str);*/
	if (mono_reflection_parse_type (tmp, &info)) {
		type = _mono_reflection_get_type_from_info (&info, image, FALSE, error);
		if (!is_ok (error)) {
			g_free (tmp);
			mono_reflection_free_type_info (&info);
			return NULL;
		}
	}

	g_free (tmp);
	mono_reflection_free_type_info (&info);
	return type;
}

/*
 * mono_reflection_get_token:
 *
 *   Return the metadata token of OBJ which should be an object
 * representing a metadata element.
 */
guint32
mono_reflection_get_token (MonoObject *obj)
{
	MonoError error;
	guint32 result = mono_reflection_get_token_checked (obj, &error);
	mono_error_assert_ok (&error);
	return result;
}

/**
 * mono_reflection_get_token_checked:
 * @obj: the object
 * @error: set on error
 *
 *   Return the metadata token of @obj which should be an object
 * representing a metadata element.  On failure sets @error.
 */
guint32
mono_reflection_get_token_checked (MonoObject *obj, MonoError *error)
{
	MonoClass *klass;
	guint32 token = 0;

	mono_error_init (error);

	klass = obj->vtable->klass;

	if (strcmp (klass->name, "MethodBuilder") == 0) {
		MonoReflectionMethodBuilder *mb = (MonoReflectionMethodBuilder *)obj;

		token = mb->table_idx | MONO_TOKEN_METHOD_DEF;
	} else if (strcmp (klass->name, "ConstructorBuilder") == 0) {
		MonoReflectionCtorBuilder *mb = (MonoReflectionCtorBuilder *)obj;

		token = mb->table_idx | MONO_TOKEN_METHOD_DEF;
	} else if (strcmp (klass->name, "FieldBuilder") == 0) {
		MonoReflectionFieldBuilder *fb = (MonoReflectionFieldBuilder *)obj;

		token = fb->table_idx | MONO_TOKEN_FIELD_DEF;
	} else if (strcmp (klass->name, "TypeBuilder") == 0) {
		MonoReflectionTypeBuilder *tb = (MonoReflectionTypeBuilder *)obj;
		token = tb->table_idx | MONO_TOKEN_TYPE_DEF;
	} else if (strcmp (klass->name, "RuntimeType") == 0) {
		MonoType *type = mono_reflection_type_get_handle ((MonoReflectionType*)obj, error);
		return_val_if_nok (error, 0);
		MonoClass *mc = mono_class_from_mono_type (type);
		if (!mono_class_init (mc)) {
			mono_error_set_for_class_failure (error, mc);
			return 0;
		}

		token = mc->type_token;
	} else if (strcmp (klass->name, "MonoCMethod") == 0 ||
			   strcmp (klass->name, "MonoMethod") == 0) {
		MonoReflectionMethod *m = (MonoReflectionMethod *)obj;
		if (m->method->is_inflated) {
			MonoMethodInflated *inflated = (MonoMethodInflated *) m->method;
			return inflated->declaring->token;
		} else {
			token = m->method->token;
		}
	} else if (strcmp (klass->name, "MonoField") == 0) {
		MonoReflectionField *f = (MonoReflectionField*)obj;

		token = mono_class_get_field_token (f->field);
	} else if (strcmp (klass->name, "MonoProperty") == 0) {
		MonoReflectionProperty *p = (MonoReflectionProperty*)obj;

		token = mono_class_get_property_token (p->property);
	} else if (strcmp (klass->name, "MonoEvent") == 0) {
		MonoReflectionMonoEvent *p = (MonoReflectionMonoEvent*)obj;

		token = mono_class_get_event_token (p->event);
	} else if (strcmp (klass->name, "ParameterInfo") == 0 || strcmp (klass->name, "MonoParameterInfo") == 0) {
		MonoReflectionParameter *p = (MonoReflectionParameter*)obj;
		MonoClass *member_class = mono_object_class (p->MemberImpl);
		g_assert (mono_class_is_reflection_method_or_constructor (member_class));

		token = mono_method_get_param_token (((MonoReflectionMethod*)p->MemberImpl)->method, p->PositionImpl);
	} else if (strcmp (klass->name, "Module") == 0 || strcmp (klass->name, "MonoModule") == 0) {
		MonoReflectionModule *m = (MonoReflectionModule*)obj;

		token = m->token;
	} else if (strcmp (klass->name, "Assembly") == 0 || strcmp (klass->name, "MonoAssembly") == 0) {
		token = mono_metadata_make_token (MONO_TABLE_ASSEMBLY, 1);
	} else {
		mono_error_set_not_implemented (error, "MetadataToken is not supported for type '%s.%s'",
						klass->name_space, klass->name);
		return 0;
	}

	return token;
}


gboolean
mono_reflection_is_usertype (MonoReflectionType *ref)
{
	MonoClass *klass = mono_object_class (ref);
	return klass->image != mono_defaults.corlib || strcmp ("TypeDelegator", klass->name) == 0;
}

/**
 * mono_reflection_bind_generic_parameters:
 * @type: a managed type object (which should be some kind of generic (instance? definition?))
 * @type_args: the number of type arguments to bind
 * @types: array of type arguments
 * @error: set on error
 *
 * Given a managed type object for a generic type instance, binds each of its arguments to the specified types.
 * Returns the MonoType* for the resulting type instantiation.  On failure returns NULL and sets @error.
 */
MonoType*
mono_reflection_bind_generic_parameters (MonoReflectionType *type, int type_argc, MonoType **types, MonoError *error)
{
	MonoClass *klass;
	gboolean is_dynamic = FALSE;
	MonoClass *geninst;

	mono_error_init (error);
	
	mono_loader_lock ();

	if (mono_is_sre_type_builder (mono_object_class (type))) {
		is_dynamic = TRUE;
	} else if (mono_is_sre_generic_instance (mono_object_class (type))) {
		/* Does this ever make sense?  what does instantiating a generic instance even mean? */
		g_assert_not_reached ();
		MonoReflectionGenericClass *rgi = (MonoReflectionGenericClass *) type;
		MonoReflectionType *gtd = rgi->generic_type;

		if (mono_is_sre_type_builder (mono_object_class (gtd)))
			is_dynamic = TRUE;
	}

	MonoType *t = mono_reflection_type_get_handle (type, error);
	if (!is_ok (error)) {
		mono_loader_unlock ();
		return NULL;
	}

	klass = mono_class_from_mono_type (t);
	if (!mono_class_is_gtd (klass)) {
		mono_loader_unlock ();
		mono_error_set_type_load_class (error, klass, "Cannot bind generic parameters of a non-generic type");
		return NULL;
	}

	guint gtd_type_argc = mono_class_get_generic_container (klass)->type_argc;
	if (gtd_type_argc != type_argc) {
		mono_loader_unlock ();
		mono_error_set_argument (error, "types", "The generic type definition needs %d type arguments, but was instantiated with %d ", gtd_type_argc, type_argc);
		return NULL;
	}


	if (klass->wastypebuilder)
		is_dynamic = TRUE;

	mono_loader_unlock ();

	geninst = mono_class_bind_generic_parameters (klass, type_argc, types, is_dynamic);

	return &geninst->byval_arg;
}

MonoClass*
mono_class_bind_generic_parameters (MonoClass *klass, int type_argc, MonoType **types, gboolean is_dynamic)
{
	MonoGenericClass *gclass;
	MonoGenericInst *inst;

	g_assert (mono_class_is_gtd (klass));

	inst = mono_metadata_get_generic_inst (type_argc, types);
	gclass = mono_metadata_lookup_generic_class (klass, inst, is_dynamic);

	return mono_generic_class_get_class (gclass);
}

static MonoReflectionMethod*
reflection_bind_generic_method_parameters (MonoReflectionMethod *rmethod, MonoArray *types, MonoError *error)
{
	MonoClass *klass;
	MonoMethod *method, *inflated;
	MonoMethodInflated *imethod;
	MonoGenericContext tmp_context;
	MonoGenericInst *ginst;
	MonoType **type_argv;
	int count, i;

	mono_error_init (error);

	g_assert (strcmp (rmethod->object.vtable->klass->name, "MethodBuilder"));

	method = rmethod->method;

	klass = method->klass;

	if (method->is_inflated)
		method = ((MonoMethodInflated *) method)->declaring;

	count = mono_method_signature (method)->generic_param_count;
	if (count != mono_array_length (types))
		return NULL;

	type_argv = g_new0 (MonoType *, count);
	for (i = 0; i < count; i++) {
		MonoReflectionType *garg = (MonoReflectionType *)mono_array_get (types, gpointer, i);
		type_argv [i] = mono_reflection_type_get_handle (garg, error);
		if (!is_ok (error)) {
			g_free (type_argv);
			return NULL;
		}
	}
	ginst = mono_metadata_get_generic_inst (count, type_argv);
	g_free (type_argv);

	tmp_context.class_inst = mono_class_is_ginst (klass) ? mono_class_get_generic_class (klass)->context.class_inst : NULL;
	tmp_context.method_inst = ginst;

	inflated = mono_class_inflate_generic_method_checked (method, &tmp_context, error);
	mono_error_assert_ok (error);
	imethod = (MonoMethodInflated *) inflated;

	/*FIXME but I think this is no longer necessary*/
	if (image_is_dynamic (method->klass->image)) {
		MonoDynamicImage *image = (MonoDynamicImage*)method->klass->image;
		/*
		 * This table maps metadata structures representing inflated methods/fields
		 * to the reflection objects representing their generic definitions.
		 */
		mono_image_lock ((MonoImage*)image);
		mono_g_hash_table_insert (image->generic_def_objects, imethod, rmethod);
		mono_image_unlock ((MonoImage*)image);
	}

	if (!mono_verifier_is_method_valid_generic_instantiation (inflated)) {
		mono_error_set_argument (error, "typeArguments", "Invalid generic arguments");
		return NULL;
	}
	
	return mono_method_get_object_checked (mono_object_domain (rmethod), inflated, NULL, error);
}

MonoReflectionMethod*
ves_icall_MonoMethod_MakeGenericMethod_impl (MonoReflectionMethod *rmethod, MonoArray *types)
{
	MonoError error;
	MonoReflectionMethod *result = reflection_bind_generic_method_parameters (rmethod, types, &error);
	mono_error_set_pending_exception (&error);
	return result;
}


/* SECURITY_ACTION_* are defined in mono/metadata/tabledefs.h */
const static guint32 declsec_flags_map[] = {
	0x00000000,					/* empty */
	MONO_DECLSEC_FLAG_REQUEST,			/* SECURITY_ACTION_REQUEST			(x01) */
	MONO_DECLSEC_FLAG_DEMAND,			/* SECURITY_ACTION_DEMAND			(x02) */
	MONO_DECLSEC_FLAG_ASSERT,			/* SECURITY_ACTION_ASSERT			(x03) */
	MONO_DECLSEC_FLAG_DENY,				/* SECURITY_ACTION_DENY				(x04) */
	MONO_DECLSEC_FLAG_PERMITONLY,			/* SECURITY_ACTION_PERMITONLY			(x05) */
	MONO_DECLSEC_FLAG_LINKDEMAND,			/* SECURITY_ACTION_LINKDEMAND			(x06) */
	MONO_DECLSEC_FLAG_INHERITANCEDEMAND,		/* SECURITY_ACTION_INHERITANCEDEMAND		(x07) */
	MONO_DECLSEC_FLAG_REQUEST_MINIMUM,		/* SECURITY_ACTION_REQUEST_MINIMUM		(x08) */
	MONO_DECLSEC_FLAG_REQUEST_OPTIONAL,		/* SECURITY_ACTION_REQUEST_OPTIONAL		(x09) */
	MONO_DECLSEC_FLAG_REQUEST_REFUSE,		/* SECURITY_ACTION_REQUEST_REFUSE		(x0A) */
	MONO_DECLSEC_FLAG_PREJIT_GRANT,			/* SECURITY_ACTION_PREJIT_GRANT			(x0B) */
	MONO_DECLSEC_FLAG_PREJIT_DENY,			/* SECURITY_ACTION_PREJIT_DENY			(x0C) */
	MONO_DECLSEC_FLAG_NONCAS_DEMAND,		/* SECURITY_ACTION_NONCAS_DEMAND		(x0D) */
	MONO_DECLSEC_FLAG_NONCAS_LINKDEMAND,		/* SECURITY_ACTION_NONCAS_LINKDEMAND		(x0E) */
	MONO_DECLSEC_FLAG_NONCAS_INHERITANCEDEMAND,	/* SECURITY_ACTION_NONCAS_INHERITANCEDEMAND	(x0F) */
	MONO_DECLSEC_FLAG_LINKDEMAND_CHOICE,		/* SECURITY_ACTION_LINKDEMAND_CHOICE		(x10) */
	MONO_DECLSEC_FLAG_INHERITANCEDEMAND_CHOICE,	/* SECURITY_ACTION_INHERITANCEDEMAND_CHOICE	(x11) */
	MONO_DECLSEC_FLAG_DEMAND_CHOICE,		/* SECURITY_ACTION_DEMAND_CHOICE		(x12) */
};

/*
 * Returns flags that includes all available security action associated to the handle.
 * @token: metadata token (either for a class or a method)
 * @image: image where resides the metadata.
 */
static guint32
mono_declsec_get_flags (MonoImage *image, guint32 token)
{
	int index = mono_metadata_declsec_from_index (image, token);
	MonoTableInfo *t = &image->tables [MONO_TABLE_DECLSECURITY];
	guint32 result = 0;
	guint32 action;
	int i;

	/* HasSecurity can be present for other, not specially encoded, attributes,
	   e.g. SuppressUnmanagedCodeSecurityAttribute */
	if (index < 0)
		return 0;

	for (i = index; i < t->rows; i++) {
		guint32 cols [MONO_DECL_SECURITY_SIZE];

		mono_metadata_decode_row (t, i, cols, MONO_DECL_SECURITY_SIZE);
		if (cols [MONO_DECL_SECURITY_PARENT] != token)
			break;

		action = cols [MONO_DECL_SECURITY_ACTION];
		if ((action >= MONO_DECLSEC_ACTION_MIN) && (action <= MONO_DECLSEC_ACTION_MAX)) {
			result |= declsec_flags_map [action];
		} else {
			g_assert_not_reached ();
		}
	}
	return result;
}

/*
 * Get the security actions (in the form of flags) associated with the specified method.
 *
 * @method: The method for which we want the declarative security flags.
 * Return the declarative security flags for the method (only).
 *
 * Note: To keep MonoMethod size down we do not cache the declarative security flags
 *       (except for the stack modifiers which are kept in the MonoJitInfo structure)
 */
guint32
mono_declsec_flags_from_method (MonoMethod *method)
{
	if (method->flags & METHOD_ATTRIBUTE_HAS_SECURITY) {
		/* FIXME: No cache (for the moment) */
		guint32 idx = mono_method_get_index (method);
		idx <<= MONO_HAS_DECL_SECURITY_BITS;
		idx |= MONO_HAS_DECL_SECURITY_METHODDEF;
		return mono_declsec_get_flags (method->klass->image, idx);
	}
	return 0;
}

/*
 * Get the security actions (in the form of flags) associated with the specified class.
 *
 * @klass: The class for which we want the declarative security flags.
 * Return the declarative security flags for the class.
 *
 * Note: We cache the flags inside the MonoClass structure as this will get 
 *       called very often (at least for each method).
 */
guint32
mono_declsec_flags_from_class (MonoClass *klass)
{
	if (mono_class_get_flags (klass) & TYPE_ATTRIBUTE_HAS_SECURITY) {
		guint32 flags = mono_class_get_declsec_flags (klass);

		if (!flags) {
			guint32 idx;

			idx = mono_metadata_token_index (klass->type_token);
			idx <<= MONO_HAS_DECL_SECURITY_BITS;
			idx |= MONO_HAS_DECL_SECURITY_TYPEDEF;
			flags = mono_declsec_get_flags (klass->image, idx);
			/* we cache the flags on classes */
			mono_class_set_declsec_flags (klass, flags);
		}
		return flags;
	}
	return 0;
}

/*
 * Get the security actions (in the form of flags) associated with the specified assembly.
 *
 * @assembly: The assembly for which we want the declarative security flags.
 * Return the declarative security flags for the assembly.
 */
guint32
mono_declsec_flags_from_assembly (MonoAssembly *assembly)
{
	guint32 idx = 1; /* there is only one assembly */
	idx <<= MONO_HAS_DECL_SECURITY_BITS;
	idx |= MONO_HAS_DECL_SECURITY_ASSEMBLY;
	return mono_declsec_get_flags (assembly->image, idx);
}


/*
 * Fill actions for the specific index (which may either be an encoded class token or
 * an encoded method token) from the metadata image.
 * Returns TRUE if some actions requiring code generation are present, FALSE otherwise.
 */
static MonoBoolean
fill_actions_from_index (MonoImage *image, guint32 token, MonoDeclSecurityActions* actions,
	guint32 id_std, guint32 id_noncas, guint32 id_choice)
{
	MonoBoolean result = FALSE;
	MonoTableInfo *t;
	guint32 cols [MONO_DECL_SECURITY_SIZE];
	int index = mono_metadata_declsec_from_index (image, token);
	int i;

	t  = &image->tables [MONO_TABLE_DECLSECURITY];
	for (i = index; i < t->rows; i++) {
		mono_metadata_decode_row (t, i, cols, MONO_DECL_SECURITY_SIZE);

		if (cols [MONO_DECL_SECURITY_PARENT] != token)
			return result;

		/* if present only replace (class) permissions with method permissions */
		/* if empty accept either class or method permissions */
		if (cols [MONO_DECL_SECURITY_ACTION] == id_std) {
			if (!actions->demand.blob) {
				const char *blob = mono_metadata_blob_heap (image, cols [MONO_DECL_SECURITY_PERMISSIONSET]);
				actions->demand.index = cols [MONO_DECL_SECURITY_PERMISSIONSET];
				actions->demand.blob = (char*) (blob + 2);
				actions->demand.size = mono_metadata_decode_blob_size (blob, &blob);
				result = TRUE;
			}
		} else if (cols [MONO_DECL_SECURITY_ACTION] == id_noncas) {
			if (!actions->noncasdemand.blob) {
				const char *blob = mono_metadata_blob_heap (image, cols [MONO_DECL_SECURITY_PERMISSIONSET]);
				actions->noncasdemand.index = cols [MONO_DECL_SECURITY_PERMISSIONSET];
				actions->noncasdemand.blob = (char*) (blob + 2);
				actions->noncasdemand.size = mono_metadata_decode_blob_size (blob, &blob);
				result = TRUE;
			}
		} else if (cols [MONO_DECL_SECURITY_ACTION] == id_choice) {
			if (!actions->demandchoice.blob) {
				const char *blob = mono_metadata_blob_heap (image, cols [MONO_DECL_SECURITY_PERMISSIONSET]);
				actions->demandchoice.index = cols [MONO_DECL_SECURITY_PERMISSIONSET];
				actions->demandchoice.blob = (char*) (blob + 2);
				actions->demandchoice.size = mono_metadata_decode_blob_size (blob, &blob);
				result = TRUE;
			}
		}
	}

	return result;
}

static MonoBoolean
mono_declsec_get_class_demands_params (MonoClass *klass, MonoDeclSecurityActions* demands, 
	guint32 id_std, guint32 id_noncas, guint32 id_choice)
{
	guint32 idx = mono_metadata_token_index (klass->type_token);
	idx <<= MONO_HAS_DECL_SECURITY_BITS;
	idx |= MONO_HAS_DECL_SECURITY_TYPEDEF;
	return fill_actions_from_index (klass->image, idx, demands, id_std, id_noncas, id_choice);
}

static MonoBoolean
mono_declsec_get_method_demands_params (MonoMethod *method, MonoDeclSecurityActions* demands, 
	guint32 id_std, guint32 id_noncas, guint32 id_choice)
{
	guint32 idx = mono_method_get_index (method);
	idx <<= MONO_HAS_DECL_SECURITY_BITS;
	idx |= MONO_HAS_DECL_SECURITY_METHODDEF;
	return fill_actions_from_index (method->klass->image, idx, demands, id_std, id_noncas, id_choice);
}

/*
 * Collect all actions (that requires to generate code in mini) assigned for
 * the specified method.
 * Note: Don't use the content of actions if the function return FALSE.
 */
MonoBoolean
mono_declsec_get_demands (MonoMethod *method, MonoDeclSecurityActions* demands)
{
	guint32 mask = MONO_DECLSEC_FLAG_DEMAND | MONO_DECLSEC_FLAG_NONCAS_DEMAND | 
		MONO_DECLSEC_FLAG_DEMAND_CHOICE;
	MonoBoolean result = FALSE;
	guint32 flags;

	/* quick exit if no declarative security is present in the metadata */
	if (!method->klass->image->tables [MONO_TABLE_DECLSECURITY].rows)
		return FALSE;

	/* we want the original as the wrapper is "free" of the security informations */
	if (method->wrapper_type == MONO_WRAPPER_MANAGED_TO_NATIVE || method->wrapper_type == MONO_WRAPPER_MANAGED_TO_MANAGED) {
		method = mono_marshal_method_from_wrapper (method);
		if (!method)
			return FALSE;
	}

	/* First we look for method-level attributes */
	if (method->flags & METHOD_ATTRIBUTE_HAS_SECURITY) {
		mono_class_init (method->klass);
		memset (demands, 0, sizeof (MonoDeclSecurityActions));

		result = mono_declsec_get_method_demands_params (method, demands, 
			SECURITY_ACTION_DEMAND, SECURITY_ACTION_NONCASDEMAND, SECURITY_ACTION_DEMANDCHOICE);
	}

	/* Here we use (or create) the class declarative cache to look for demands */
	flags = mono_declsec_flags_from_class (method->klass);
	if (flags & mask) {
		if (!result) {
			mono_class_init (method->klass);
			memset (demands, 0, sizeof (MonoDeclSecurityActions));
		}
		result |= mono_declsec_get_class_demands_params (method->klass, demands, 
			SECURITY_ACTION_DEMAND, SECURITY_ACTION_NONCASDEMAND, SECURITY_ACTION_DEMANDCHOICE);
	}

	/* The boolean return value is used as a shortcut in case nothing needs to
	   be generated (e.g. LinkDemand[Choice] and InheritanceDemand[Choice]) */
	return result;
}


/*
 * Collect all Link actions: LinkDemand, NonCasLinkDemand and LinkDemandChoice (2.0).
 *
 * Note: Don't use the content of actions if the function return FALSE.
 */
MonoBoolean
mono_declsec_get_linkdemands (MonoMethod *method, MonoDeclSecurityActions* klass, MonoDeclSecurityActions *cmethod)
{
	MonoBoolean result = FALSE;
	guint32 flags;

	/* quick exit if no declarative security is present in the metadata */
	if (!method->klass->image->tables [MONO_TABLE_DECLSECURITY].rows)
		return FALSE;

	/* we want the original as the wrapper is "free" of the security informations */
	if (method->wrapper_type == MONO_WRAPPER_MANAGED_TO_NATIVE || method->wrapper_type == MONO_WRAPPER_MANAGED_TO_MANAGED) {
		method = mono_marshal_method_from_wrapper (method);
		if (!method)
			return FALSE;
	}

	/* results are independant - zeroize both */
	memset (cmethod, 0, sizeof (MonoDeclSecurityActions));
	memset (klass, 0, sizeof (MonoDeclSecurityActions));

	/* First we look for method-level attributes */
	if (method->flags & METHOD_ATTRIBUTE_HAS_SECURITY) {
		mono_class_init (method->klass);

		result = mono_declsec_get_method_demands_params (method, cmethod, 
			SECURITY_ACTION_LINKDEMAND, SECURITY_ACTION_NONCASLINKDEMAND, SECURITY_ACTION_LINKDEMANDCHOICE);
	}

	/* Here we use (or create) the class declarative cache to look for demands */
	flags = mono_declsec_flags_from_class (method->klass);
	if (flags & (MONO_DECLSEC_FLAG_LINKDEMAND | MONO_DECLSEC_FLAG_NONCAS_LINKDEMAND | MONO_DECLSEC_FLAG_LINKDEMAND_CHOICE)) {
		mono_class_init (method->klass);

		result |= mono_declsec_get_class_demands_params (method->klass, klass, 
			SECURITY_ACTION_LINKDEMAND, SECURITY_ACTION_NONCASLINKDEMAND, SECURITY_ACTION_LINKDEMANDCHOICE);
	}

	return result;
}

/*
 * Collect all Inherit actions: InheritanceDemand, NonCasInheritanceDemand and InheritanceDemandChoice (2.0).
 *
 * @klass	The inherited class - this is the class that provides the security check (attributes)
 * @demans	
 * return TRUE if inheritance demands (any kind) are present, FALSE otherwise.
 * 
 * Note: Don't use the content of actions if the function return FALSE.
 */
MonoBoolean
mono_declsec_get_inheritdemands_class (MonoClass *klass, MonoDeclSecurityActions* demands)
{
	MonoBoolean result = FALSE;
	guint32 flags;

	/* quick exit if no declarative security is present in the metadata */
	if (!klass->image->tables [MONO_TABLE_DECLSECURITY].rows)
		return FALSE;

	/* Here we use (or create) the class declarative cache to look for demands */
	flags = mono_declsec_flags_from_class (klass);
	if (flags & (MONO_DECLSEC_FLAG_INHERITANCEDEMAND | MONO_DECLSEC_FLAG_NONCAS_INHERITANCEDEMAND | MONO_DECLSEC_FLAG_INHERITANCEDEMAND_CHOICE)) {
		mono_class_init (klass);
		memset (demands, 0, sizeof (MonoDeclSecurityActions));

		result |= mono_declsec_get_class_demands_params (klass, demands, 
			SECURITY_ACTION_INHERITDEMAND, SECURITY_ACTION_NONCASINHERITANCE, SECURITY_ACTION_INHERITDEMANDCHOICE);
	}

	return result;
}

/*
 * Collect all Inherit actions: InheritanceDemand, NonCasInheritanceDemand and InheritanceDemandChoice (2.0).
 *
 * Note: Don't use the content of actions if the function return FALSE.
 */
MonoBoolean
mono_declsec_get_inheritdemands_method (MonoMethod *method, MonoDeclSecurityActions* demands)
{
	/* quick exit if no declarative security is present in the metadata */
	if (!method->klass->image->tables [MONO_TABLE_DECLSECURITY].rows)
		return FALSE;

	/* we want the original as the wrapper is "free" of the security informations */
	if (method->wrapper_type == MONO_WRAPPER_MANAGED_TO_NATIVE || method->wrapper_type == MONO_WRAPPER_MANAGED_TO_MANAGED) {
		method = mono_marshal_method_from_wrapper (method);
		if (!method)
			return FALSE;
	}

	if (method->flags & METHOD_ATTRIBUTE_HAS_SECURITY) {
		mono_class_init (method->klass);
		memset (demands, 0, sizeof (MonoDeclSecurityActions));

		return mono_declsec_get_method_demands_params (method, demands, 
			SECURITY_ACTION_INHERITDEMAND, SECURITY_ACTION_NONCASINHERITANCE, SECURITY_ACTION_INHERITDEMANDCHOICE);
	}
	return FALSE;
}


static MonoBoolean
get_declsec_action (MonoImage *image, guint32 token, guint32 action, MonoDeclSecurityEntry *entry)
{
	guint32 cols [MONO_DECL_SECURITY_SIZE];
	MonoTableInfo *t;
	int i;

	int index = mono_metadata_declsec_from_index (image, token);
	if (index == -1)
		return FALSE;

	t =  &image->tables [MONO_TABLE_DECLSECURITY];
	for (i = index; i < t->rows; i++) {
		mono_metadata_decode_row (t, i, cols, MONO_DECL_SECURITY_SIZE);

		/* shortcut - index are ordered */
		if (token != cols [MONO_DECL_SECURITY_PARENT])
			return FALSE;

		if (cols [MONO_DECL_SECURITY_ACTION] == action) {
			const char *metadata = mono_metadata_blob_heap (image, cols [MONO_DECL_SECURITY_PERMISSIONSET]);
			entry->blob = (char*) (metadata + 2);
			entry->size = mono_metadata_decode_blob_size (metadata, &metadata);
			return TRUE;
		}
	}

	return FALSE;
}

MonoBoolean
mono_declsec_get_method_action (MonoMethod *method, guint32 action, MonoDeclSecurityEntry *entry)
{
	if (method->flags & METHOD_ATTRIBUTE_HAS_SECURITY) {
		guint32 idx = mono_method_get_index (method);
		idx <<= MONO_HAS_DECL_SECURITY_BITS;
		idx |= MONO_HAS_DECL_SECURITY_METHODDEF;
		return get_declsec_action (method->klass->image, idx, action, entry);
	}
	return FALSE;
}

MonoBoolean
mono_declsec_get_class_action (MonoClass *klass, guint32 action, MonoDeclSecurityEntry *entry)
{
	/* use cache */
	guint32 flags = mono_declsec_flags_from_class (klass);
	if (declsec_flags_map [action] & flags) {
		guint32 idx = mono_metadata_token_index (klass->type_token);
		idx <<= MONO_HAS_DECL_SECURITY_BITS;
		idx |= MONO_HAS_DECL_SECURITY_TYPEDEF;
		return get_declsec_action (klass->image, idx, action, entry);
	}
	return FALSE;
}

MonoBoolean
mono_declsec_get_assembly_action (MonoAssembly *assembly, guint32 action, MonoDeclSecurityEntry *entry)
{
	guint32 idx = 1; /* there is only one assembly */
	idx <<= MONO_HAS_DECL_SECURITY_BITS;
	idx |= MONO_HAS_DECL_SECURITY_ASSEMBLY;

	return get_declsec_action (assembly->image, idx, action, entry);
}

gboolean
mono_reflection_call_is_assignable_to (MonoClass *klass, MonoClass *oklass, MonoError *error)
{
	MonoObject *res, *exc;
	void *params [1];
	static MonoMethod *method = NULL;

	mono_error_init (error);

	if (method == NULL) {
		method = mono_class_get_method_from_name (mono_class_get_type_builder_class (), "IsAssignableTo", 1);
		g_assert (method);
	}

	/* 
	 * The result of mono_type_get_object_checked () might be a System.MonoType but we
	 * need a TypeBuilder so use mono_class_get_ref_info (klass).
	 */
	g_assert (mono_class_get_ref_info (klass));
	g_assert (!strcmp (((MonoObject*)(mono_class_get_ref_info (klass)))->vtable->klass->name, "TypeBuilder"));

	params [0] = mono_type_get_object_checked (mono_domain_get (), &oklass->byval_arg, error);
	return_val_if_nok (error, FALSE);

	MonoError inner_error;
	res = mono_runtime_try_invoke (method, (MonoObject*)(mono_class_get_ref_info (klass)), params, &exc, &inner_error);

	if (exc || !is_ok (&inner_error)) {
		mono_error_cleanup (&inner_error);
		return FALSE;
	} else
		return *(MonoBoolean*)mono_object_unbox (res);
}

/**
 * mono_reflection_type_get_type:
 * @reftype: the System.Type object
 *
 * Returns the MonoType* associated with the C# System.Type object @reftype.
 */
MonoType*
mono_reflection_type_get_type (MonoReflectionType *reftype)
{
	g_assert (reftype);

	MonoError error;
	MonoType *result = mono_reflection_type_get_handle (reftype, &error);
	mono_error_assert_ok (&error);
	return result;
}

/**
 * mono_reflection_assembly_get_assembly:
 * @refassembly: the System.Reflection.Assembly object
 *
 * Returns the MonoAssembly* associated with the C# System.Reflection.Assembly object @refassembly.
 */
MonoAssembly*
mono_reflection_assembly_get_assembly (MonoReflectionAssembly *refassembly)
{
	g_assert (refassembly);

	return refassembly->assembly;
}

/**
 * mono_class_from_mono_type_handle:
 * @reftype: the System.Type handle
 *
 * Returns the MonoClass* corresponding to the given type.
 */
MonoClass*
mono_class_from_mono_type_handle (MonoReflectionTypeHandle reftype)
{
	return mono_class_from_mono_type (MONO_HANDLE_RAW (reftype)->type);
}
