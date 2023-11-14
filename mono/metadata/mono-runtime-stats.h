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

MONO_END_DECLS

#endif /* __MONO_RUNTIME_STATS_H__ */