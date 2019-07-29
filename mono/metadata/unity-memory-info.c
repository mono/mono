#include <config.h>
#include <mono/utils/mono-publib.h>
#include "unity-memory-info.h"
#include <mono/metadata/assembly.h>
#include <mono/metadata/class.h>
#include <mono/metadata/class-internals.h>
#include <mono/metadata/image.h>
#include <mono/metadata/metadata-internals.h>
#include <mono/metadata/object-internals.h>
#include <mono/metadata/tabledefs.h>
#include <mono/metadata/tokentype.h>
#include <mono/metadata/gc-internals.h>
#include <glib.h>
#include <stdlib.h>

#if HAVE_BDWGC_GC

#include "external/bdwgc/include/gc.h"

typedef struct CollectMetadataContext
{
	GHashTable *allTypes;
	int currentIndex;
	MonoMetadataSnapshot* metadata;
} CollectMetadataContext;

static void
ContextRecurseClassData (CollectMetadataContext *context, MonoClass *klass)
{
	gpointer orig_key, value;
	gpointer iter = NULL;
	MonoClassField *field = NULL;
	int fieldCount;

	/* use g_hash_table_lookup_extended as it returns boolean to indicate if value was found.
	* If we use g_hash_table_lookup it returns the value which we were comparing to NULL. The problem is
	* that 0 is a valid class index and was confusing our logic.
	*/
	if (klass->inited && !g_hash_table_lookup_extended (context->allTypes, klass, &orig_key, &value)) {
		g_hash_table_insert (context->allTypes, klass, GINT_TO_POINTER (context->currentIndex++));

		fieldCount = mono_class_num_fields (klass);
		
		if (fieldCount > 0) {
			while ((field = mono_class_get_fields (klass, &iter))) {
				MonoClass *fieldKlass = mono_class_from_mono_type (field->type);

				if (fieldKlass != klass)
					ContextRecurseClassData (context, fieldKlass);
			}
		}
	}
}

static void
CollectHashMapClass (gpointer key, gpointer value, gpointer user_data)
{
	CollectMetadataContext *context = (CollectMetadataContext *)user_data;
	MonoClass *klass = (MonoClass *)value;
	ContextRecurseClassData (context, klass);
}

static void
CollectHashMapListClasses (gpointer key, gpointer value, gpointer user_data)
{
	CollectMetadataContext *context = (CollectMetadataContext *)user_data;
	GSList *list = (GSList *)value;

	while (list != NULL) {
		MonoClass *klass = (MonoClass *)list->data;
		ContextRecurseClassData (context, klass);

		list = g_slist_next (list);
	}
}

static void
CollectGenericClass (MonoGenericClass *genericClass, gpointer user_data)
{
	CollectMetadataContext *context = (CollectMetadataContext *)user_data;

	if (genericClass->cached_class != NULL)
		ContextRecurseClassData (context, genericClass->cached_class);
}

static void
CollectImageMetaData (MonoImage *image, gpointer value, CollectMetadataContext *context)
{
	int i;
	MonoTableInfo *tdef = &image->tables[MONO_TABLE_TYPEDEF];
	GSList *list;

	if (image->dynamic) {
		GHashTableIter iter;
		gpointer key;
		MonoDynamicImage *dynamicImage = (MonoDynamicImage *)image;
		g_hash_table_iter_init (&iter, dynamicImage->typeref);

		while (g_hash_table_iter_next (&iter, &key, NULL)) {
			MonoType *monoType = (MonoType *)key;
			MonoClass *klass = mono_class_from_mono_type (monoType);

			if (klass)
				ContextRecurseClassData (context, klass);
		}
	}

	/* Some classes are only in this list. 
	   They are added in reflection_setup_internal_class_internal.
	*/
	list = image->reflection_info_unregister_classes;

	while (list) {
		MonoClass *klass = (MonoClass *)list->data;

		if (klass)
			ContextRecurseClassData (context, klass);

		list = list->next;
	}

	for (i = 1; i < tdef->rows; ++i) {
		MonoClass *klass;
		MonoError error;

		guint32 token = (i + 1) | MONO_TOKEN_TYPE_DEF;

		klass = mono_class_get_checked (image, token, &error);

		if (klass)
			ContextRecurseClassData (context, klass);
	}

	if (image->array_cache)
		g_hash_table_foreach (image->array_cache, CollectHashMapListClasses, context);

	if (image->szarray_cache)
		g_hash_table_foreach (image->szarray_cache, CollectHashMapClass, context);

	if (image->ptr_cache)
		g_hash_table_foreach (image->ptr_cache, CollectHashMapClass, context);
}

static int
FindClassIndex (GHashTable *hashTable, MonoClass *klass)
{
	gpointer orig_key, value;

	if (!g_hash_table_lookup_extended (hashTable, klass, &orig_key, &value))
		return -1;

	return GPOINTER_TO_INT (value);
}

static void
AddMetadataType (gpointer key, gpointer value, gpointer user_data)
{
	MonoClass *klass = (MonoClass *)key;

	int index = GPOINTER_TO_INT (value);
	CollectMetadataContext *context = (CollectMetadataContext *)user_data;
	MonoMetadataSnapshot *metadata = context->metadata;
	MonoMetadataType *type = &metadata->types[index];

	if (klass->rank > 0) {
		type->flags = (MonoMetadataTypeFlags) (kArray | (kArrayRankMask & (klass->rank << 16)));
		type->baseOrElementTypeIndex = FindClassIndex (context->allTypes, mono_class_get_element_class (klass));
	} else {
		gpointer iter = NULL;
		int fieldCount = 0;
		MonoClassField *field;
		MonoClass *baseClass;
		MonoVTable *vtable;
		void *statics_data;

		type->flags = (klass->valuetype || klass->byval_arg.type == MONO_TYPE_PTR) ? kValueType : kNone;
		type->fieldCount = 0;
		fieldCount = mono_class_num_fields (klass);
		if (fieldCount > 0) {
			type->fields = g_new (MonoMetadataField, fieldCount);

			while ((field = mono_class_get_fields (klass, &iter))) {
				MonoMetadataField *metaField = &type->fields[type->fieldCount];
				MonoClass *typeKlass = mono_class_from_mono_type (field->type);

				metaField->typeIndex = FindClassIndex (context->allTypes, typeKlass);

				// This will happen if fields type is not initialized
				// It's OK to skip it, because it means the field is guaranteed to be null on any object
				if (metaField->typeIndex == -1) {
					continue;
				}

				// literals have no actual storage, and are not relevant in this context.
				if ((field->type->attrs & FIELD_ATTRIBUTE_LITERAL) != 0)
					continue;

				metaField->isStatic = (field->type->attrs & FIELD_ATTRIBUTE_STATIC) != 0;

				metaField->offset = field->offset;
				metaField->name = field->name;
				type->fieldCount++;
			}
		}

		vtable = mono_class_try_get_vtable (mono_domain_get (), klass);
		statics_data = vtable ? mono_vtable_get_static_field_data (vtable) : NULL;

		type->staticsSize = statics_data ? mono_class_data_size (klass) : 0;
		type->statics = NULL;

		if (type->staticsSize > 0) {
			type->statics = g_new0 (uint8_t, type->staticsSize);
			memcpy (type->statics, statics_data, type->staticsSize);
		}

		baseClass = mono_class_get_parent (klass);
		type->baseOrElementTypeIndex = baseClass ? FindClassIndex (context->allTypes, baseClass) : -1;
	}

	type->assemblyName = mono_class_get_image (klass)->assembly->aname.name;
	type->name = mono_type_get_name_full (&klass->byval_arg, MONO_TYPE_NAME_FORMAT_IL);
	type->typeInfoAddress = (uint64_t)klass;
	type->size = (klass->valuetype) != 0 ? (mono_class_instance_size (klass) - sizeof (MonoObject)) : mono_class_instance_size (klass);
}

static void CollectMetadata(MonoMetadataSnapshot* metadata, GHashTable* monoImages)
{
	CollectMetadataContext context;

	context.allTypes = g_hash_table_new(NULL, NULL);
	context.currentIndex = 0;
	context.metadata = metadata;

	g_hash_table_foreach(monoImages, (GHFunc)CollectImageMetaData, &context);

	mono_metadata_generic_class_foreach(CollectGenericClass, &context);

	metadata->typeCount = g_hash_table_size(context.allTypes);
	metadata->types = g_new0(MonoMetadataType, metadata->typeCount);

	g_hash_table_foreach(context.allTypes, AddMetadataType, &context);

	g_hash_table_destroy(context.allTypes);
}

static void MonoMemPoolNumChunksCallback(void* start, void* end, void* user_data)
{
	int* count = (int*)user_data;
	(*count)++;
}

static int MonoMemPoolNumChunks(MonoMemPool* pool)
{
	int count = 0;
	mono_mempool_foreach_block(pool, MonoMemPoolNumChunksCallback, &count);
	return count;
}

typedef struct SectionIterationContext
{
	MonoManagedMemorySection* currentSection;
} SectionIterationContext;

static void AllocateMemoryForSection(void* context, void* sectionStart, void* sectionEnd)
{
	ptrdiff_t sectionSize;

	SectionIterationContext* ctx = (SectionIterationContext*)context;
	MonoManagedMemorySection* section = ctx->currentSection;

	section->sectionStartAddress = (uint64_t)sectionStart;
	sectionSize = (uint8_t*)(sectionEnd)-(uint8_t*)(sectionStart);

	section->sectionSize = (uint32_t)(sectionSize);
	section->sectionBytes = g_new(uint8_t, section->sectionSize);

	ctx->currentSection++;
}

static void AllocateMemoryForMemPoolChunk(void* chunkStart, void* chunkEnd, void* context)
{
	AllocateMemoryForSection(context, chunkStart, chunkEnd);
}

static void CopyHeapSection(void* context, void* sectionStart, void* sectionEnd)
{
	SectionIterationContext* ctx = (SectionIterationContext*)(context);
	MonoManagedMemorySection* section = ctx->currentSection;

	g_assert(section->sectionStartAddress == (uint64_t)(sectionStart));
	g_assert(section->sectionSize == (uint8_t*)(sectionEnd)-(uint8_t*)(sectionStart));
	memcpy(section->sectionBytes, sectionStart, section->sectionSize);

	ctx->currentSection++;
}

static void CopyMemPoolChunk(void* chunkStart, void* chunkEnd, void* context)
{
	CopyHeapSection(context, chunkStart, chunkEnd);
}

static void IncrementCountForImageMemPoolNumChunks(MonoImage *image, gpointer* value, void *user_data)
{
	int* count = (int*)user_data;
	(*count) += MonoMemPoolNumChunks(image->mempool);
}

static int MonoImagesMemPoolNumChunks(GHashTable* monoImages)
{
	int count = 0;

	g_hash_table_foreach(monoImages, (GHFunc)IncrementCountForImageMemPoolNumChunks, &count);
	return count;
}

static void AllocateMemoryForMemPool(MonoMemPool* pool, void *user_data)
{
	mono_mempool_foreach_block(pool, AllocateMemoryForMemPoolChunk, user_data);
}

static void AllocateMemoryForImageMemPool(MonoImage *image, gpointer value, void *user_data)
{
	AllocateMemoryForMemPool(image->mempool, user_data);
}

static void CopyMemPool(MonoMemPool *pool, SectionIterationContext *context)
{
	mono_mempool_foreach_block(pool, CopyMemPoolChunk, context);
}

static void CopyImageMemPool(MonoImage *image, gpointer value, SectionIterationContext *context)
{
	CopyMemPool(image->mempool, context);
}

static void AllocateMemoryForImageClassCache(MonoImage *image, gpointer *value, void *user_data)
{
	AllocateMemoryForSection(user_data, image->class_cache.table, ((uint8_t*)image->class_cache.table) + image->class_cache.size);
}

static void CopyImageClassCache(MonoImage *image, gpointer value, SectionIterationContext *context)
{
	CopyHeapSection(context, image->class_cache.table, ((uint8_t*)image->class_cache.table) + image->class_cache.size);
}

static void IncrementCountForImageSetMemPoolNumChunks(MonoImageSet *imageSet, void *user_data)
{
	int* count = (int*)user_data;
	(*count) += MonoMemPoolNumChunks(imageSet->mempool);
}

static int MonoImageSetsMemPoolNumChunks()
{
	int count = 0;	
	mono_metadata_image_set_foreach(IncrementCountForImageSetMemPoolNumChunks, &count);
	return count;
}

static void AllocateMemoryForImageSetMemPool(MonoImageSet* imageSet, void *user_data)
{
	AllocateMemoryForMemPool(imageSet->mempool, user_data);
}

static void CopyImageSetMemPool(MonoImageSet* imageSet, void *user_data)
{
	CopyMemPool(imageSet->mempool, user_data);
}

typedef struct
{
	MonoManagedHeap* heap;
	GHashTable* monoImages;

} CaptureHeapInfoData;

static void* CaptureHeapInfo(void* user)
{
	CaptureHeapInfoData* data = (CaptureHeapInfoData*)user;
	MonoManagedHeap* heap = data->heap;
	GHashTable* monoImages = data->monoImages;

	MonoDomain* domain = mono_domain_get();
	MonoDomain* rootDomain = mono_get_root_domain();
	SectionIterationContext iterationContext;

	// Increment count for each heap section
	heap->sectionCount = GC_get_heap_section_count();
	// Increment count for the domain mem pool chunk
	heap->sectionCount += MonoMemPoolNumChunks(rootDomain->mp);
	heap->sectionCount += MonoMemPoolNumChunks(domain->mp);
	// Increment count for each image mem pool chunk
	heap->sectionCount += MonoImagesMemPoolNumChunks(monoImages);
	// Increment count for each image->class_cache hash table.
	heap->sectionCount += g_hash_table_size(monoImages);
	// Increment count for each image set mem pool chunk
	heap->sectionCount += MonoImageSetsMemPoolNumChunks();

	heap->sections = g_new0(MonoManagedMemorySection, heap->sectionCount);

	iterationContext.currentSection = heap->sections;

	// Allocate memory for each heap section
	GC_foreach_heap_section(&iterationContext, AllocateMemoryForSection);
	// Allocate memory for the domain mem pool chunk
	mono_domain_lock(rootDomain);
	mono_mempool_foreach_block(rootDomain->mp, AllocateMemoryForMemPoolChunk, &iterationContext);
	mono_domain_unlock(rootDomain);
	mono_domain_lock(domain);
	mono_mempool_foreach_block(domain->mp, AllocateMemoryForMemPoolChunk, &iterationContext);
	mono_domain_unlock(domain);
	// Allocate memory for each image mem pool chunk
	g_hash_table_foreach(monoImages, (GHFunc)AllocateMemoryForImageMemPool, &iterationContext);
	// Allocate memory for each image->class_cache hash table.
	g_hash_table_foreach(monoImages, (GHFunc)AllocateMemoryForImageClassCache, &iterationContext);
	// Allocate memory for each image->class_cache hash table.
	mono_metadata_image_set_foreach(AllocateMemoryForImageSetMemPool, &iterationContext);

	return NULL;
}

static void FreeMonoManagedHeap(MonoManagedHeap* heap)
{
	uint32_t i;

	for (i = 0; i < heap->sectionCount; i++)
	{
		g_free(heap->sections[i].sectionBytes);
	}

	g_free(heap->sections);
}

typedef struct VerifyHeapSectionStillValidIterationContext
{
	MonoManagedMemorySection* currentSection;
	gboolean wasValid;
} VerifyHeapSectionStillValidIterationContext;

static void VerifyHeapSectionIsStillValid(void* context, void* sectionStart, void* sectionEnd)
{
	VerifyHeapSectionStillValidIterationContext* iterationContext = (VerifyHeapSectionStillValidIterationContext*)context;
	if (iterationContext->currentSection->sectionSize != (uint8_t*)(sectionEnd)-(uint8_t*)(sectionStart))
		iterationContext->wasValid = FALSE;
	else if (iterationContext->currentSection->sectionStartAddress != (uint64_t)(sectionStart))
		iterationContext->wasValid = FALSE;

	iterationContext->currentSection++;
}

static gboolean MonoManagedHeapStillValid(MonoManagedHeap* heap, GHashTable* monoImages)
{
	MonoDomain* rootDomain = mono_get_root_domain();
	MonoDomain* domain = mono_domain_get();

	VerifyHeapSectionStillValidIterationContext iterationContext;
	int currentSectionCount;

	currentSectionCount = GC_get_heap_section_count();
	currentSectionCount += MonoMemPoolNumChunks(rootDomain->mp);
	currentSectionCount += MonoMemPoolNumChunks(domain->mp);
	currentSectionCount += MonoImagesMemPoolNumChunks(monoImages);
	currentSectionCount += g_hash_table_size(monoImages); // image->class_cache hash table.
	currentSectionCount += MonoImageSetsMemPoolNumChunks();

	if (heap->sectionCount != currentSectionCount)
		return FALSE;

	iterationContext.currentSection = heap->sections;
	iterationContext.wasValid = TRUE;

	GC_foreach_heap_section(&iterationContext, VerifyHeapSectionIsStillValid);

	return iterationContext.wasValid;
}

// The difficulty in capturing the managed snapshot is that we need to do quite some work with the world stopped,
// to make sure that our snapshot is "valid", and didn't change as we were copying it. However, stopping the world,
// makes it so you cannot take any lock or allocations. We deal with it like this:
//
// 1) We take note of the amount of heap sections and their sizes, and we allocate memory to copy them into.
// 2) We stop the world.
// 3) We check if the amount of heapsections and their sizes didn't change in the mean time. If they did, try again.
// 4) Now, with the world still stopped, we memcpy() the memory from the real heapsections, into the memory that we
//    allocated for their copies.
// 5) Start the world again.

static void CaptureManagedHeap(MonoManagedHeap* heap, GHashTable* monoImages)
{
	MonoDomain* rootDomain = mono_get_root_domain();
	MonoDomain* domain = mono_domain_get();
	SectionIterationContext iterationContext;

	CaptureHeapInfoData data;

	data.heap = heap;
	data.monoImages = monoImages;

	CaptureHeapInfo(&data);

	iterationContext.currentSection = heap->sections;

	GC_foreach_heap_section(&iterationContext, CopyHeapSection);
	mono_mempool_foreach_block(rootDomain->mp, CopyMemPoolChunk, &iterationContext);
	mono_mempool_foreach_block(domain->mp, CopyMemPoolChunk, &iterationContext);
	g_hash_table_foreach(monoImages, (GHFunc)CopyImageMemPool, &iterationContext);
	g_hash_table_foreach(monoImages, (GHFunc)CopyImageClassCache, &iterationContext);
	mono_metadata_image_set_foreach(CopyImageSetMemPool, &iterationContext);
}

static void GCHandleIterationCallback(MonoObject* managedObject, GList** managedObjects)
{
	*managedObjects = g_list_append(*managedObjects, managedObject);
}

static inline void CaptureGCHandleTargets(MonoGCHandles* gcHandles)
{
	uint32_t i;
	GList* trackedObjects, *trackedObject;

	trackedObjects = NULL;

	mono_gc_strong_handle_foreach((GFunc)GCHandleIterationCallback, &trackedObjects);

	gcHandles->trackedObjectCount = (uint32_t)g_list_length(trackedObjects);
	gcHandles->pointersToObjects = (uint64_t*)g_new0(uint64_t, gcHandles->trackedObjectCount);

	trackedObject = trackedObjects;

	for (i = 0; i < gcHandles->trackedObjectCount; i++)
	{
		gcHandles->pointersToObjects[i] = (uint64_t)trackedObject->data;
		trackedObject = g_list_next(trackedObject);
	}

	g_list_free(trackedObjects);
}

static void FillRuntimeInformation(MonoRuntimeInformation* runtimeInfo)
{
	runtimeInfo->pointerSize = (uint32_t)(sizeof(void*));
	runtimeInfo->objectHeaderSize = (uint32_t)(sizeof(MonoObject));
	runtimeInfo->arrayHeaderSize = offsetof(MonoArray, vector);
	runtimeInfo->arraySizeOffsetInHeader = offsetof(MonoArray, max_length);
	runtimeInfo->arrayBoundsOffsetInHeader = offsetof(MonoArray, bounds);
	runtimeInfo->allocationGranularity = (uint32_t)(2 * sizeof(void*));
}

static gboolean ManagedHeapContainsAddress(MonoManagedHeap* heap, uint64_t address)
{
	uint32_t i;

	for (i = 0; i < heap->sectionCount; ++i)
	{
		MonoManagedMemorySection* section = &heap->sections[i];
		uint64_t sectionBegin = section->sectionStartAddress;
		uint64_t sectionEnd = sectionBegin + section->sectionSize;

		if (address >= sectionBegin && address <= sectionEnd)
			return TRUE;
	}

	return FALSE;
}

static void VerifySnapshot(MonoManagedMemorySnapshot* snapshot, GHashTable* monoImages)
{
	uint32_t i;
	FILE* file;
	GHashTableIter iter;
	gpointer key;
	MonoMetadataSnapshot* meta = &snapshot->metadata;

	file = fopen("MonoMemorySnapshotLog.txt", "w");

	g_hash_table_iter_init(&iter, monoImages);

	while (g_hash_table_iter_next(&iter, &key, NULL))
	{
		MonoImage* image = (MonoImage*)key;
		fprintf(file, "MonoImage [0x%016llX] dynamic: %i name: '%s'\n", (uint64_t)image, image->dynamic, image->name);
	}

	/* Verify that we have collected memory sections for all types */
	for (i = 0; i < meta->typeCount; ++i)
	{
		MonoMetadataType* type = &meta->types[i];

		if (!ManagedHeapContainsAddress(&snapshot->heap, type->typeInfoAddress))
		{
			fprintf(file, "The memory for type '%s' @ 0x%016llX is not the part the snapshot.\n", type->name, type->typeInfoAddress);
		}
	}

	fclose(file);
}

static void CollectMonoImage(MonoImage* image, GHashTable* monoImages)
{
	if (g_hash_table_lookup(monoImages, image) != NULL)
		return;

	g_hash_table_insert(monoImages, image, image);

	if (image->assembly->image != NULL &&
		image != image->assembly->image)
	{
		CollectMonoImage(image->assembly->image, monoImages);
	}

	if (image->module_count > 0)
	{
		int i;

		for (i = 0; i < image->module_count; ++i)
		{
			MonoImage* moduleImage = image->modules[i];

			if (moduleImage)
				CollectMonoImage(moduleImage, monoImages);
		}
	}
}

static void CollectMonoImageFromAssembly(MonoAssembly *assembly, void *user_data)
{
	GHashTable* monoImages = (GHashTable*)user_data;
	CollectMonoImage(assembly->image, monoImages);
}



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
	MonoImage *image = mono_assembly_get_image(assembly);
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
			MonoClass *klass = mono_class_from_mono_type(monoType);

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

	if (genericClass->cached_class != NULL)
		context->callback(genericClass->cached_class, context->user_data);
}

void
mono_unity_class_for_each(ClassReportFunc callback, void *user_data)
{
	ClassReportContext reportContext;
	reportContext.callback = callback;
	reportContext.user_data = user_data;
	mono_domain_assembly_foreach(mono_domain_get(), ReportClassesFromAssembly, &reportContext);
	mono_metadata_generic_class_foreach(ReportGenericClasses, &reportContext);
}

MonoManagedMemorySnapshot* mono_unity_capture_memory_snapshot()
{
	GC_disable();
	GC_stop_world_external();

	MonoManagedMemorySnapshot* snapshot;
	snapshot = g_new0(MonoManagedMemorySnapshot, 1);

	GHashTable* monoImages = g_hash_table_new(NULL, NULL);

	mono_domain_assembly_foreach(mono_domain_get(), CollectMonoImageFromAssembly, monoImages);

	CollectMetadata(&snapshot->metadata, monoImages);
	CaptureManagedHeap(&snapshot->heap, monoImages);
	CaptureGCHandleTargets(&snapshot->gcHandles);
	FillRuntimeInformation(&snapshot->runtimeInformation);

#if _DEBUG
//	VerifySnapshot(snapshot, monoImages);
#endif

	g_hash_table_destroy(monoImages);

	GC_start_world_external();
	GC_enable();

	return snapshot;
}

void mono_unity_free_captured_memory_snapshot(MonoManagedMemorySnapshot* snapshot)
{
	uint32_t i;
	MonoMetadataSnapshot* metadata = &snapshot->metadata;

	FreeMonoManagedHeap(&snapshot->heap);

	g_free(snapshot->gcHandles.pointersToObjects);

	for (i = 0; i < metadata->typeCount; i++)
	{
		if ((metadata->types[i].flags & kArray) == 0)
		{
			g_free(metadata->types[i].fields);
			g_free(metadata->types[i].statics);
		}

		g_free(metadata->types[i].name);
	}

	g_free(metadata->types);
	g_free(snapshot);
}

#else

MonoManagedMemorySnapshot* mono_unity_capture_memory_snapshot()
{
	MonoManagedMemorySnapshot* snapshot;
	snapshot = g_new0(MonoManagedMemorySnapshot, 1);

	return snapshot;
}

void mono_unity_free_captured_memory_snapshot(MonoManagedMemorySnapshot* snapshot)
{
	g_free(snapshot);
}

#endif