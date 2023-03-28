#ifndef __UNITY_MONO_MEMORY_INFO_H
#define __UNITY_MONO_MEMORY_INFO_H

typedef struct _MonoClass MonoClass;
typedef void(*ClassReportFunc) (MonoClass* klass, void *user_data);

MONO_API void mono_unity_class_for_each(ClassReportFunc callback, void* user_data);

typedef struct MonoManagedMemorySnapshot MonoManagedMemorySnapshot;
MONO_API MonoManagedMemorySnapshot* mono_unity_capture_memory_snapshot();
MONO_API void mono_unity_free_captured_memory_snapshot(MonoManagedMemorySnapshot* snapshot);

#endif
