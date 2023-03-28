#include <config.h>
#include <mono/utils/mono-publib.h>
#include "unity-memory-info.h"
#include <mono/metadata/assembly-internals.h>
#include <mono/metadata/class-internals.h>
#include <mono/metadata/image.h>
#include <mono/metadata/metadata-internals.h>
#include <mono/metadata/object-internals.h>
#include <mono/metadata/tokentype.h>
#include <mono/metadata/gc-internals.h>
#include <glib.h>
#include <stdlib.h>

typedef struct ClassReportContext {
	ClassReportFunc callback;
	void *user_data;
} ClassReportContext;

static void
ReportHashMapClasses(gpointer key, gpointer value, gpointer user_data)
{
	ClassReportContext *context = (ClassReportContext *)user_data;
	MonoClass *klass = (MonoClass *)value;
	if (klass->inited)
		context->callback(klass, context->user_data);
}

static void
ReportHashMapListClasses(gpointer key, gpointer value, gpointer user_data)
{
	ClassReportContext *context = (ClassReportContext *)user_data;
	GSList *list = (GSList *)value;

	while (list != NULL) {
		MonoClass *klass = (MonoClass *)list->data;

		if (klass->inited)
			context->callback(klass, context->user_data);

		list = g_slist_next(list);
	}
}

static void
ReportClassesFromAssembly(MonoAssembly *assembly, void *user_data)
{
	MonoImage *image = mono_assembly_get_image_internal(assembly);
	int i;
	MonoTableInfo *tdef = &image->tables[MONO_TABLE_TYPEDEF];
	GSList *list;
	ClassReportContext *context = (ClassReportContext*)user_data;

	if (image->dynamic) {
		GHashTableIter iter;
		gpointer key;
		MonoDynamicImage *dynamicImage = (MonoDynamicImage *)image;
		g_hash_table_iter_init(&iter, dynamicImage->typeref);

		while (g_hash_table_iter_next(&iter, &key, NULL)) {
			MonoType *monoType = (MonoType *)key;
			MonoClass *klass = mono_class_from_mono_type_internal(monoType);

			if (klass && klass->inited)
				context->callback(klass, context->user_data);
		}
	}

	/* Some classes are only in this list.
	   They are added in reflection_setup_internal_class_internal.
	*/
	list = image->reflection_info_unregister_classes;

	while (list) {
		MonoClass *klass = (MonoClass *)list->data;

		if (klass && klass->inited)
			context->callback(klass, context->user_data);

		list = list->next;
	}

	// Iterate all image classes
	for (i = 1; i < tdef->rows; ++i) {
		MonoClass *klass;
		MonoError error;

		guint32 token = (i + 1) | MONO_TOKEN_TYPE_DEF;

		klass = mono_class_get_checked(image, token, &error);

		if (klass && klass->inited)
			context->callback(klass, context->user_data);
	}

	if (image->array_cache)
		g_hash_table_foreach(image->array_cache, ReportHashMapListClasses, user_data);

	if (image->szarray_cache)
		g_hash_table_foreach(image->szarray_cache, ReportHashMapClasses, user_data);

	if (image->ptr_cache)
		g_hash_table_foreach(image->ptr_cache, ReportHashMapClasses, user_data);
}

static void
ReportGenericClasses(MonoGenericClass *genericClass, gpointer user_data)
{
	ClassReportContext *context = (ClassReportContext *)user_data;

	if (genericClass->cached_class != NULL && genericClass->cached_class->inited)
		context->callback(genericClass->cached_class, context->user_data);
}

static void
ReportImageSetClasses(MonoImageSet *imageSet, void* user_data)
{
	if (imageSet->array_cache)
		g_hash_table_foreach(imageSet->array_cache, ReportHashMapListClasses, user_data);

	if (imageSet->szarray_cache)
		g_hash_table_foreach(imageSet->szarray_cache, ReportHashMapClasses, user_data);

	if (imageSet->ptr_cache)
		g_hash_table_foreach(imageSet->ptr_cache, ReportHashMapClasses, user_data);
}

MONO_API void
mono_unity_class_for_each(ClassReportFunc callback, void *user_data)
{
	ClassReportContext reportContext;
	reportContext.callback = callback;
	reportContext.user_data = user_data;
	// Report all assembly classes and assembly specific arrays
	mono_domain_assembly_foreach(mono_domain_get(), ReportClassesFromAssembly, &reportContext);
	// Report all generic classes
	mono_metadata_generic_class_foreach(ReportGenericClasses, &reportContext);
	// Report all image set arrays
	mono_metadata_image_set_foreach(ReportImageSetClasses, &reportContext);
}

MONO_API MonoManagedMemorySnapshot* 
mono_unity_capture_memory_snapshot()
{
	return NULL;
}

MONO_API void
mono_unity_free_captured_memory_snapshot(MonoManagedMemorySnapshot* snapshot)
{
}
