#include <mono/metadata/mono-runtime-stats.h>
#include <mono/utils/gc_wrapper.h>
#include <glib.h>
#include <mono/utils/gc_wrapper.h>

MonoRuntimeStats mono_runtime_stats = {{0}};

MonoRuntimeStats *
get_mono_runtime_stats()
{
	return &mono_runtime_stats;
}

typedef struct
{
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

void rg_set_GC_dirty_inner(func_GC_dirty_inner func);
void rg_set_GC_free(func_GC_free func);
void rg_set_GC_malloc(func_GC_malloc func);
void rg_set_GC_gcj_malloc(func_GC_gcj_malloc func);
void rg_set_GC_malloc_uncollectable(func_GC_malloc_uncollectable func);
void rg_set_GC_malloc_atomic(func_GC_malloc_atomic func);

MONO_API void rg_mono_set_GC_dirty_inner(rg_mono_func_GC_dirty_inner func)
{
	rg_set_GC_dirty_inner(func);
}

MONO_API void rg_mono_set_GC_free(rg_mono_func_GC_free func)
{
	rg_set_GC_free(func);
}

MONO_API void rg_mono_set_GC_malloc(rg_mono_func_GC_malloc func)
{
	rg_set_GC_malloc(func);
}

MONO_API void rg_mono_set_GC_gcj_malloc(rg_mono_func_GC_gcj_malloc func)
{
	rg_set_GC_gcj_malloc(func);
}

MONO_API void rg_mono_set_GC_malloc_uncollectable(rg_mono_func_GC_malloc_uncollectable func)
{
	rg_set_GC_malloc_uncollectable(func);
}

MONO_API void rg_mono_set_GC_malloc_atomic(rg_mono_func_GC_malloc_atomic func)
{
	rg_set_GC_malloc_atomic(func);
}

void* GC_malloc_kind(size_t size, int k);

MONO_API void *rg_mono_GC_malloc_kind(size_t lb, int k)
{
	return GC_malloc_kind(lb, k);
}