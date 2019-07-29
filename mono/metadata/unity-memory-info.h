#ifndef __UNITY_MONO_MEMORY_INFO_H
#define __UNITY_MONO_MEMORY_INFO_H

#include <glib.h>

typedef struct MonoMetadataField
{
	uint32_t offset;
	uint32_t typeIndex;
	const char* name;
	gboolean isStatic;
} MonoMetadataField;

typedef enum MonoMetadataTypeFlags
{
	kNone = 0,
	kValueType = 1 << 0,
	kArray = 1 << 1,
	kArrayRankMask = 0xFFFF0000
} MonoMetadataTypeFlags;

typedef struct MonoMetadataType
{
	MonoMetadataTypeFlags flags;  // If it's an array, rank is encoded in the upper 2 bytes
	MonoMetadataField* fields;
	uint32_t fieldCount;
	uint32_t staticsSize;
	uint8_t* statics;
	uint32_t baseOrElementTypeIndex;
	char* name;
	const char* assemblyName;
	uint64_t typeInfoAddress;
	uint32_t size;
} MonoMetadataType;

typedef struct MonoMetadataSnapshot
{
	uint32_t typeCount;
	MonoMetadataType* types;
} MonoMetadataSnapshot;

typedef struct MonoManagedMemorySection
{
	uint64_t sectionStartAddress;
	uint32_t sectionSize;
	uint8_t* sectionBytes;
} MonoManagedMemorySection;

typedef struct MonoManagedHeap
{
	uint32_t sectionCount;
	MonoManagedMemorySection* sections;
} MonoManagedHeap;

typedef struct MonoStacks
{
	uint32_t stackCount;
	MonoManagedMemorySection* stacks;
} MonoStacks;

typedef struct NativeObject
{
	uint32_t gcHandleIndex;
	uint32_t size;
	uint32_t instanceId;
	uint32_t classId;
	uint32_t referencedNativeObjectIndicesCount;
	uint32_t* referencedNativeObjectIndices;
} NativeObject;

typedef struct MonoGCHandles
{
	uint32_t trackedObjectCount;
	uint64_t* pointersToObjects;
} MonoGCHandles;

typedef struct MonoRuntimeInformation
{
	uint32_t pointerSize;
	uint32_t objectHeaderSize;
	uint32_t arrayHeaderSize;
	uint32_t arrayBoundsOffsetInHeader;
	uint32_t arraySizeOffsetInHeader;
	uint32_t allocationGranularity;
} MonoRuntimeInformation;

typedef struct MonoManagedMemorySnapshot
{
	MonoManagedHeap heap;
	MonoStacks stacks;
	MonoMetadataSnapshot metadata;
	MonoGCHandles gcHandles;
	MonoRuntimeInformation runtimeInformation;
	void* additionalUserInformation;
} MonoManagedMemorySnapshot;

typedef struct _MonoClass MonoClass;

typedef void(*ClassReportFunc) (MonoClass* klass, void *user_data);

MONO_API void
mono_unity_class_for_each(ClassReportFunc callback, void* user_data);

MONO_API MonoManagedMemorySnapshot* mono_unity_capture_memory_snapshot();
MONO_API void mono_unity_free_captured_memory_snapshot(MonoManagedMemorySnapshot* snapshot);

#endif
