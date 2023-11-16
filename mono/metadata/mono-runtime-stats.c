#include <mono/metadata/mono-runtime-stats.h>
#if HAVE_BOEHM_GC
#include <mono/utils/gc_wrapper.h>
#endif
#include <mono/metadata/gc-internals.h>

#include <glib.h>

MonoRuntimeStats mono_runtime_stats = {{0}};

MonoRuntimeStats *
get_mono_runtime_stats()
{
	return &mono_runtime_stats;
}

MONO_API void
rg_gc_heap_foreach(RG_GC_heap_section_proc callback, void* user_data)
{
#if HAVE_BOEHM_GC
	GC_foreach_heap_section(user_data, callback);
#else
	g_assert_not_reached();
#endif
}

MONO_API void rg_mono_set_GC_dirty_inner(rg_mono_func_GC_dirty_inner func)
{
#if HAVE_BOEHM_GC
	rg_set_GC_dirty_inner(func);
#endif
}

MONO_API void rg_mono_set_GC_free(rg_mono_func_GC_free func)
{
#if HAVE_BOEHM_GC
	rg_set_GC_free(func);
#endif
}

MONO_API void rg_mono_set_GC_malloc(rg_mono_func_GC_malloc func)
{
#if HAVE_BOEHM_GC
	rg_set_GC_malloc(func);
#endif
}

MONO_API void rg_mono_set_GC_gcj_malloc(rg_mono_func_GC_gcj_malloc func)
{
#if HAVE_BOEHM_GC
	rg_set_GC_gcj_malloc(func);
#endif
}

MONO_API void rg_mono_set_GC_malloc_uncollectable(rg_mono_func_GC_malloc_uncollectable func)
{
#if HAVE_BOEHM_GC
	rg_set_GC_malloc_uncollectable(func);
#endif
}

MONO_API void rg_mono_set_GC_malloc_atomic(rg_mono_func_GC_malloc_atomic func)
{
#if HAVE_BOEHM_GC
	rg_set_GC_malloc_atomic(func);
#endif
}

void* GC_malloc_kind(size_t size, int k);

MONO_API void *rg_mono_GC_malloc_kind(size_t lb, int k)
{
	return GC_malloc_kind(lb, k);
}

MONO_API void rg_mono_GC_set_time_limit(unsigned long slice)
{
#if HAVE_BOEHM_GC
	GC_set_time_limit(slice);
#endif
}