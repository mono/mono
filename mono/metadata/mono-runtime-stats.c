#include <mono/metadata/mono-runtime-stats.h>
#include <mono/utils/gc_wrapper.h>
#include <glib.h>

MonoRuntimeStats mono_runtime_stats = {{ 0 }};

MonoRuntimeStats *
get_mono_runtime_stats ()
{
	return &mono_runtime_stats;
}

typedef struct {
	CBFunc callback;
	cbPtr user_data;
} rg_execution_ctx;

MONO_API void
rg_gc_heap_foreach(CBFunc callback, cbPtr user_data)
{
#if HAVE_BOEHM_GC
	GC_foreach_heap_section(user_data, callback);
#else
	g_assert_not_reached();
#endif
}