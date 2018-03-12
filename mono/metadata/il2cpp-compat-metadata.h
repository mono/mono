#pragma once

#ifdef RUNTIME_IL2CPP

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
#define mono_gc_register_root_wbarrier il2cpp_mono_gc_register_root_wbarrier
#define mono_gc_wbarrier_generic_store il2cpp_mono_gc_wbarrier_generic_store
#define mono_gc_make_vector_descr il2cpp_mono_gc_make_vector_descr
#define mono_gc_deregister_root il2cpp_mono_gc_deregister_root

int il2cpp_mono_gc_register_root_wbarrier (char *start, size_t size, MonoGCDescriptor descr, MonoGCRootSource source, void *key, const char *msg);
SgenDescriptor il2cpp_mono_gc_make_root_descr_all_refs(int numbits);
MonoGCDescriptor il2cpp_mono_gc_make_vector_descr (void);
void* il2cpp_mono_gc_alloc_fixed (size_t size, void* descr, MonoGCRootSource source, void *key, const char *msg);
gboolean il2cpp_mono_gc_is_moving();

typedef void* (*MonoGCLockedCallbackFunc) (void *data);
void* il2cpp_mono_gc_invoke_with_gc_lock (MonoGCLockedCallbackFunc func, void *data);

#ifndef HOST_WIN32
int il2cpp_mono_gc_pthread_create (pthread_t *new_thread, const pthread_attr_t *attr, void *(*start_routine)(void *), void *arg);
#endif

#endif
