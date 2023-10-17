#ifndef __UNITY_MONO_LIVENESS_H
#define __UNITY_MONO_LIVENESS_H

#include <mono/metadata/class-internals.h>

/* number of sub elements of an array to process before recursing
 * we take a depth first approach to use stack space rather than re-allocating
 * processing array which requires restarting world to ensure allocator lock is not held
*/
const int kArrayElementsPerChunk = 256;

/* how far we recurse processing array elements before we stop. Prevents stack overflow */
const int kMaxTraverseRecursionDepth = 128;

typedef struct _LivenessState LivenessState;

typedef void(*register_object_callback) (gpointer *arr, int size, void *callback_userdata);
typedef void(*WorldStateChanged) ();
typedef void *(*ReallocateArray) (void *ptr, int size, void *callback_userdata);
typedef void* (*unity_aligned_malloc_func)(size_t size, size_t alignment);
typedef void (*unity_aligned_free_func)(void *ptr);

/* Liveness calculation */
MONO_API LivenessState * mono_unity_liveness_allocate_struct(MonoClass *filter, guint max_count, register_object_callback callback, void *callback_userdata, WorldStateChanged onWorldStart);
MONO_API void mono_unity_liveness_stop_gc_world(void);
MONO_API void mono_unity_liveness_finalize(LivenessState *state);
MONO_API void mono_unity_liveness_start_gc_world(void);
MONO_API void mono_unity_liveness_free_struct(LivenessState *state);

MONO_API void mono_unity_liveness_calculation_from_root(MonoObject *root, LivenessState *state);
MONO_API void mono_unity_liveness_calculation_from_statics(LivenessState *state);

MONO_API LivenessState * mono_unity_liveness_calculation_begin(MonoClass *filter, guint max_count, register_object_callback callback, void *callback_userdata, WorldStateChanged onWorldStarted, WorldStateChanged onWorldStopped);

MONO_API void mono_unity_liveness_calculation_end(LivenessState *state);
MONO_API void mono_unity_liveness_set_memory_callback(unity_aligned_malloc_func aligned_alloc_callback, unity_aligned_free_func aligned_free_callback);
#endif