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

typedef struct CollectMetadataContext
{
	GHashTable *allTypes;
	int currentIndex;
	MonoMetadataSnapshot* metadata;
} CollectMetadataContext;

static void ContextInsertClass(CollectMetadataContext* context, MonoClass* klass)
{
	if (klass->inited && g_hash_table_lookup(context->allTypes, klass) == NULL)
		g_hash_table_insert(context->allTypes, klass, (gpointer)(context->currentIndex++));
}

static void CollectHashMapClass(gpointer key, gpointer value, gpointer user_data)
{
	CollectMetadataContext* context = (CollectMetadataContext*)user_data;
	MonoClass* klass = (MonoClass*)value;
	ContextInsertClass(context, klass);
}

static void CollectHashMapListClasses(gpointer key, gpointer value, gpointer user_data)
{
	CollectMetadataContext* context = (CollectMetadataContext*)user_data;
	GSList* list = (GSList*)value;

	while (list != NULL)
	{
		MonoClass* klass = (MonoClass*)list->data;
		ContextInsertClass(context, klass);

		list = g_slist_next(list);
	}
}

static void CollectGenericClass(gpointer value, gpointer user_data)
{
	CollectMetadataContext* context = (CollectMetadataContext*)user_data;
	MonoGenericClass* genericClass = (MonoGenericClass*)value;

	if (genericClass->cached_class != NULL)
		ContextInsertClass(context, genericClass->cached_class);
}

static void CollectAssemblyMetaData(MonoAssembly *assembly, void *user_data)
{
	int i;
	CollectMetadataContext* context = (CollectMetadataContext*)user_data;
	MonoImage* image = mono_assembly_get_image(assembly);
	MonoTableInfo *tdef = &image->tables[MONO_TABLE_TYPEDEF];

	for (i = 0; i < tdef->rows - 1; ++i)
	{
		MonoClass* klass = mono_class_get(image, (i + 2) | MONO_TOKEN_TYPE_DEF);
		ContextInsertClass(context, klass);
	}

	if (image->array_cache)
		g_hash_table_foreach(image->array_cache, CollectHashMapListClasses, user_data);

	if (image->szarray_cache)
		g_hash_table_foreach(image->szarray_cache, CollectHashMapClass, user_data);

	if (image->ptr_cache)
		g_hash_table_foreach(image->ptr_cache, CollectHashMapClass, user_data);
}

static int FindClassIndex(GHashTable* hashTable, MonoClass* klass)
{
	gpointer value = g_hash_table_lookup(hashTable, klass);

	if (!value)
		return -1;

	return (int)value;
}

static void AddMetadataType(gpointer key, gpointer value, gpointer user_data)
{
	MonoClass* klass = (MonoClass*)key;
	int index = (int)value;
	CollectMetadataContext* context = (CollectMetadataContext*)user_data;
	MonoMetadataSnapshot* metadata = context->metadata;
	MonoMetadataType* type = &metadata->types[index];

	if (klass->rank > 0)
	{
		type->flags = (MonoMetadataTypeFlags)(kArray | (kArrayRankMask & (klass->rank << 16)));
		type->baseOrElementTypeIndex = FindClassIndex(context->allTypes, mono_class_get_element_class(klass));
	}
	else
	{
		gpointer iter = NULL;
		int fieldCount = 0;
		MonoClassField* field;
		MonoClass* baseClass;
		MonoVTable* vtable;
		void* statics_data;

		type->flags = (klass->valuetype || klass->byval_arg.type == MONO_TYPE_PTR) ? kValueType : kNone;
		type->fieldCount = 0;

		if (mono_class_num_fields(klass) > 0)
		{
			type->fields = g_new(MonoMetadataField, mono_class_num_fields(klass));

			while ((field = mono_class_get_fields(klass, &iter)))
			{
				MonoMetadataField* metaField = &type->fields[type->fieldCount];
				metaField->typeIndex = FindClassIndex(context->allTypes, mono_class_from_mono_type(field->type));

				// This will happen if fields type is not initialized
				// It's OK to skip it, because it means the field is guaranteed to be null on any object
				if (metaField->typeIndex == -1)
					continue;

				// literals have no actual storage, and are not relevant in this context.
				if ((field->type->attrs & FIELD_ATTRIBUTE_LITERAL) != 0)
					continue;

				metaField->isStatic = (field->type->attrs & FIELD_ATTRIBUTE_STATIC) != 0;
				metaField->offset = field->offset;
				metaField->name = field->name;
				type->fieldCount++;
			}
		}

		vtable = mono_class_try_get_vtable(mono_domain_get(), klass);
		statics_data = vtable ? mono_vtable_get_static_field_data(vtable) : NULL;

		type->staticsSize = statics_data ? mono_class_data_size(klass) : 0;
		type->statics = NULL;

		if (type->staticsSize > 0)
		{
			type->statics = g_new0(uint8_t, type->staticsSize);
			memcpy(type->statics, statics_data, type->staticsSize);
		}

		baseClass = mono_class_get_parent(klass);
		type->baseOrElementTypeIndex = baseClass ? FindClassIndex(context->allTypes, baseClass) : -1;
	}

	type->assemblyName = mono_class_get_image(klass)->assembly->aname.name;
	type->name = mono_type_get_name_full(&klass->byval_arg, MONO_TYPE_NAME_FORMAT_IL);
	type->typeInfoAddress = (uint64_t)klass;
	type->size = (klass->valuetype) != 0 ? (mono_class_instance_size(klass) - sizeof(MonoObject)) : mono_class_instance_size(klass);
}

static void CollectMetadata(MonoMetadataSnapshot* metadata)
{
	CollectMetadataContext context;

	context.allTypes = g_hash_table_new(NULL, NULL);
	context.currentIndex = 0;
	context.metadata = metadata;

	mono_assembly_foreach((GFunc)CollectAssemblyMetaData, &context);

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

static void AddImageMemoryPoolChunkCount(MonoAssembly *assembly, MonoManagedHeap* heap)
{
	heap->sectionCount += MonoMemPoolNumChunks(assembly->image->mempool);
}

static void IncrementCountForImageMemPoolNumChunks(MonoAssembly *assembly, void *user_data)
{
	int* count = (int*)user_data;
	(*count) += MonoMemPoolNumChunks(assembly->image->mempool);
}

static int MonoImagesMemPoolNumChunks()
{
	int count = 0;
	mono_assembly_foreach((GFunc)IncrementCountForImageMemPoolNumChunks, &count);
	return count;
}

static void AllocateMemoryForMemPool(MonoMemPool* pool, void *user_data)
{
	mono_mempool_foreach_block(pool, AllocateMemoryForMemPoolChunk, user_data);
}

static void AllocateMemoryForImageMemPool(MonoAssembly *assembly, void *user_data)
{
	MonoImage* image = assembly->image;
	mono_image_lock(image);
	AllocateMemoryForMemPool(image->mempool, user_data);
	mono_image_unlock(image);
}

static void CopyMemPool(MonoMemPool *pool, SectionIterationContext *context)
{
	mono_mempool_foreach_block(pool, CopyMemPoolChunk, context);
}

static void CopyImageMemPool(MonoAssembly *assembly, SectionIterationContext *context)
{
	MonoImage* image = assembly->image;

	mono_image_lock(image);
	CopyMemPool(image->mempool, context);
	mono_image_unlock(image);
}

static void MonoImagesCountCallback(MonoAssembly *assembly, void *user_data)
{
	int* count = (int*)user_data;
	(*count)++;
}

static int MonoImagesCount()
{
	int count = 0;
	mono_assembly_foreach((GFunc)MonoImagesCountCallback, &count);
	return count;
}

static void AllocateMemoryForImageClassCache(MonoAssembly *assembly, void *user_data)
{
	MonoImage* image = assembly->image;
	mono_image_lock(image);

	AllocateMemoryForSection(user_data, image->class_cache.table, ((uint8_t*)image->class_cache.table) + image->class_cache.size);
	mono_image_unlock(image);
}

static void CopyImageClassCache(MonoAssembly *assembly, SectionIterationContext *context)
{
	MonoImage* image = assembly->image;
	mono_image_lock(image);

	CopyHeapSection(context, image->class_cache.table, ((uint8_t*)image->class_cache.table) + image->class_cache.size);

	mono_image_unlock(image);
}

static void IncrementCountForImageSetMemPoolNumChunks(MonoImageSet *imageSet, void *user_data)
{
	int* count = (int*)user_data;
	(*count) += MonoMemPoolNumChunks(imageSet->mempool);
}

static int MonoImageSetsMemPoolNumChunks()
{
	int count = 0;	
	mono_metadata_image_set_foreach((GFunc)IncrementCountForImageSetMemPoolNumChunks, &count);
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

static void* CaptureHeapInfo(void* monoManagedHeap)
{
	MonoManagedHeap* heap = (MonoManagedHeap*)monoManagedHeap;
	MonoDomain* domain = mono_domain_get();
	MonoDomain* rootDomain = mono_get_root_domain();
	SectionIterationContext iterationContext;

	// Increment count for each heap section
	heap->sectionCount = GC_get_heap_section_count();
	// Increment count for the domain mem pool chunk
	heap->sectionCount += MonoMemPoolNumChunks(rootDomain->mp);
	heap->sectionCount += MonoMemPoolNumChunks(domain->mp);
	// Increment count for each image mem pool chunk
	heap->sectionCount += MonoImagesMemPoolNumChunks();
	// Increment count for each image->class_cache hash table.
	heap->sectionCount += MonoImagesCount();
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
	mono_assembly_foreach((GFunc)AllocateMemoryForImageMemPool, &iterationContext);
	// Allocate memory for each image->class_cache hash table.
	mono_assembly_foreach((GFunc)AllocateMemoryForImageClassCache, &iterationContext);
	// Allocate memory for each image->class_cache hash table.
	mono_metadata_image_set_foreach((GFunc)AllocateMemoryForImageSetMemPool, &iterationContext);

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

static gboolean MonoManagedHeapStillValid(MonoManagedHeap* heap)
{
	MonoDomain* rootDomain = mono_get_root_domain();
	MonoDomain* domain = mono_domain_get();

	VerifyHeapSectionStillValidIterationContext iterationContext;
	int currentSectionCount;

	currentSectionCount = GC_get_heap_section_count();
	currentSectionCount += MonoMemPoolNumChunks(rootDomain->mp);
	currentSectionCount += MonoMemPoolNumChunks(domain->mp);
	currentSectionCount += MonoImagesMemPoolNumChunks();
	currentSectionCount += MonoImagesCount(); // image->class_cache hash table.
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

static void CaptureManagedHeap(MonoManagedHeap* heap)
{
	MonoDomain* rootDomain = mono_get_root_domain();
	MonoDomain* domain = mono_domain_get();
	SectionIterationContext iterationContext;

	while (TRUE)
	{
		GC_call_with_alloc_lock(CaptureHeapInfo, heap);
		GC_stop_world_external();

		if (MonoManagedHeapStillValid(heap))
			break;

		GC_start_world_external();

		FreeMonoManagedHeap(heap);
	}

	iterationContext.currentSection = heap->sections;

	GC_foreach_heap_section(&iterationContext, CopyHeapSection);
	mono_mempool_foreach_block(rootDomain->mp, CopyMemPoolChunk, &iterationContext);
	mono_mempool_foreach_block(domain->mp, CopyMemPoolChunk, &iterationContext);
	mono_assembly_foreach((GFunc)CopyImageMemPool, &iterationContext);
	mono_assembly_foreach((GFunc)CopyImageClassCache, &iterationContext);
	mono_metadata_image_set_foreach((GFunc)CopyImageSetMemPool, &iterationContext);

	GC_start_world_external();
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

static void VerifySnapshot(MonoManagedMemorySnapshot* snapshot)
{
	uint32_t i;
	FILE* file;
	MonoMetadataSnapshot* meta = &snapshot->metadata;

	file = fopen("MonoMemorySnapshotLog.txt", "w");

	/* Verify that we have collected memory sections for all types */
	for (i = 0; i < meta->typeCount; ++i)
	{
		MonoMetadataType* type = &meta->types[i];

		if (!ManagedHeapContainsAddress(&snapshot->heap, type->typeInfoAddress))
		{
			fprintf(file, "The memory for type '%s' @ 0x%016X is not the part the snapshot.\n", type->name, type->typeInfoAddress);
		}
	}

	fclose(file);
}

MonoManagedMemorySnapshot* mono_unity_capture_memory_snapshot()
{
	MonoManagedMemorySnapshot* snapshot;
	snapshot = g_new0(MonoManagedMemorySnapshot, 1);

	CollectMetadata(&snapshot->metadata);
	CaptureManagedHeap(&snapshot->heap);
	CaptureGCHandleTargets(&snapshot->gcHandles);
	FillRuntimeInformation(&snapshot->runtimeInformation);

#if _DEBUG
	VerifySnapshot(snapshot);
#endif

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