#ifndef __MONO_RUNTIME_STATS_H__
#define __MONO_RUNTIME_STATS_H__

#include <mono/utils/mono-publib.h>

MONO_BEGIN_DECLS

typedef struct _MonoRuntimeStats
{
    int64_t new_object_count;
    int64_t initialized_class_count;
    // uint64_t generic_vtable_count;
    // uint64_t used_class_count;
    int64_t method_count;
    // uint64_t class_vtable_size;
    int64_t class_static_data_size;
    int64_t generic_instance_count;
    int64_t generic_class_count;
    int64_t inflated_method_count;
    int64_t inflated_type_count;
    // uint64_t delegate_creations;
    // uint64_t minor_gc_count;
    // uint64_t major_gc_count;
    // uint64_t minor_gc_time_usecs;
    // uint64_t major_gc_time_usecs;
    int32_t enabled;
} MonoRuntimeStats;

extern MonoRuntimeStats mono_runtime_stats;

MONO_API
MonoRuntimeStats *
get_mono_runtime_stats();

typedef void *cbPtr;
typedef void (*CBFunc)(cbPtr data, cbPtr user_data);
MONO_API void
rg_gc_heap_foreach(CBFunc callback, cbPtr user_data);

// IL2CPP_ENABLE_WRITE_BARRIER_VALIDATION
typedef void (*rg_mono_func_GC_dirty_inner)(void **ptr);
typedef void (*rg_mono_func_GC_free)(void *ptr);
typedef void* (*rg_mono_func_GC_malloc)(size_t size);
typedef void* (*rg_mono_func_GC_gcj_malloc)(size_t size, void * ptr_to_struct_containing_descr);
typedef void* (*rg_mono_func_GC_malloc_uncollectable)(size_t size);
typedef void* (*rg_mono_func_GC_malloc_atomic)(size_t size);

MONO_API void rg_mono_set_GC_dirty_inner(rg_mono_func_GC_dirty_inner func);
MONO_API void rg_mono_set_GC_free(rg_mono_func_GC_free func);
MONO_API void rg_mono_set_GC_malloc(rg_mono_func_GC_malloc func);
MONO_API void rg_mono_set_GC_gcj_malloc(rg_mono_func_GC_gcj_malloc func);
MONO_API void rg_mono_set_GC_malloc_uncollectable(rg_mono_func_GC_malloc_uncollectable func);
MONO_API void rg_mono_set_GC_malloc_atomic(rg_mono_func_GC_malloc_atomic func);

MONO_API void *rg_mono_GC_malloc_kind(size_t /* lb */, int /* k */);

MONO_END_DECLS

#endif /* __MONO_RUNTIME_STATS_H__ */