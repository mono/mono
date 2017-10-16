#pragma once

#ifdef IL2CPP_MONO_DEBUGGER

#include <mono/sgen/sgen-conf.h>
#include <mono/metadata/mono-gc.h>

#if defined(_POSIX_VERSION)
#include <pthread.h>
#endif

#define mono_gc_make_root_descr_all_refs il2cpp_mono_gc_make_root_descr_all_refs
#define mono_gc_alloc_fixed il2cpp_mono_gc_alloc_fixed
#define mono_gc_free_fixed il2cpp_gc_free_fixed
#define mono_gc_is_moving il2cpp_mono_gc_is_moving
#define mono_gc_invoke_with_gc_lock il2cpp_mono_gc_invoke_with_gc_lock
#define mono_gc_pthread_create il2cpp_mono_gc_pthread_create
#define mono_profiler_get_events il2cpp_mono_profiler_get_events
#define mono_profiler_iomap il2cpp_mono_profiler_iomap

SgenDescriptor il2cpp_mono_gc_make_root_descr_all_refs(int numbits);
void* il2cpp_mono_gc_alloc_fixed (size_t size, void* descr, MonoGCRootSource source, const char *msg);
gboolean il2cpp_mono_gc_is_moving();

typedef void* (*MonoGCLockedCallbackFunc) (void *data);
void* il2cpp_mono_gc_invoke_with_gc_lock (MonoGCLockedCallbackFunc func, void *data);

#ifndef HOST_WIN32
int il2cpp_mono_gc_pthread_create (pthread_t *new_thread, const pthread_attr_t *attr, void *(*start_routine)(void *), void *arg);
#endif

int il2cpp_mono_profiler_get_events (void);
void il2cpp_mono_profiler_iomap (char *report, const char *pathname, const char *new_pathname);

#endif