#include <windows.h>
#include <stdio.h>
#include <gc/gc.h>
#include <pthread.h>
#include <wine/library.h>

int GC_finalize_on_demand = -1;
void (* GC_finalizer_notifier)() = (void (*) GC_PROTO((void)))0;
HMODULE moduleGC;
void * dmHandle;
char error[256];

int printf_stub(const char *format,...)
{
	return 0;
}

#define TRACE printf_stub

void InitGC ()
{
/*
	if(moduleGC == 0){
		printf ("Initializing Boehm GC library...\n");
		moduleGC = LoadLibraryExA ("gc-wine.dll", 0, 0);
		printf("Module GC %p\n", moduleGC);
		if (moduleGC == NULL) {
			exit (1);
		}
	}
*/
	if(dmHandle == 0) {
		dmHandle = wine_dlopen("/usr/local/lib/wine/gc-wine.dll.so", 1, error, sizeof(error));
		printf("Module GC.so %p, %s\n", dmHandle, error);
	}
}

GC_PTR GC_malloc (size_t lb)
{
	GC_PTR (*gc_malloc)(size_t lb);
	GC_PTR status = NULL;
	InitGC ();
/*
	printf ("GetProcAddress of %p\n", moduleGC);
	gc_malloc = GetProcAddress (moduleGC, "GC_malloc");
	printf ("GC_malloc start %p, %d\n", gc_malloc, GetLastError());
*/
	gc_malloc = wine_dlsym(dmHandle,"GC_malloc", error, sizeof(error));
	TRACE ("GC_malloc start %p, %s\n", gc_malloc, error);
	status = gc_malloc (lb);
	TRACE ("GC_malloc end\n");
	return status;
}

void GC_register_finalizer (GC_PTR obj, GC_finalization_proc fn, GC_PTR cd,
		GC_finalization_proc *ofn, GC_PTR *ocd)
{
	void (*gc_register_finalizer)(GC_PTR obj, GC_finalization_proc fn,
			GC_PTR cd, GC_finalization_proc *ofn, GC_PTR *ocd);
	InitGC ();
/*
	gc_register_finalizer =
		GetProcAddress (moduleGC, "GC_register_finalizer");
*/
	gc_register_finalizer = wine_dlsym(dmHandle,"GC_register_finalizer", error, sizeof(error));
	TRACE ("GC_register_finalizer %p, %s\n", gc_register_finalizer, error);
	gc_register_finalizer (obj, fn, cd, ofn, ocd);
	TRACE ("GC_register_finalizer end\n");
}

void GC_register_finalizer_no_order (GC_PTR obj, GC_finalization_proc fn,
				     GC_PTR cd, GC_finalization_proc *ofn,
				     GC_PTR *ocd)
{
	void (*gc_register_finalizer_no_order)(GC_PTR obj,
			GC_finalization_proc fn, GC_PTR cd,
			GC_finalization_proc *ofn, GC_PTR *ocd);
	InitGC ();
/*
	gc_register_finalizer_no_order =
		GetProcAddress (moduleGC, "GC_register_finalizer");
*/
	gc_register_finalizer_no_order = wine_dlsym(dmHandle,"GC_register_finalizer", error, sizeof(error));
	TRACE ("GC_register_finalizer_no_order %p, %s\n", gc_register_finalizer_no_order, error);
	gc_register_finalizer_no_order (obj, fn, cd, ofn, ocd);
	TRACE ("GC_register_finalizer_no_order end\n");
}


GC_PTR GC_debug_malloc (size_t size_in_bytes, GC_EXTRA_PARAMS)
{
	return GC_malloc (size_in_bytes);
}

GC_PTR GC_malloc_atomic (size_t size_in_bytes)
{
	GC_PTR (*gc_malloc_atomic)(size_t lb);
	GC_PTR status = NULL;
	InitGC ();
	//gc_malloc_atomic = GetProcAddress (moduleGC, "GC_malloc_atomic");

	gc_malloc_atomic = wine_dlsym(dmHandle,"GC_malloc_atomic", error, sizeof(error));
	TRACE ("GC_malloc_atomic %p, %s\n", gc_malloc_atomic, error);
	status = gc_malloc_atomic (size_in_bytes);
	TRACE ("GC_malloc_atomic end\n");
	return status;
}

GC_PTR GC_realloc (GC_PTR old_object, size_t new_size_in_bytes)
{
	GC_PTR (*gc_realloc)(GC_PTR old_object, size_t new_size_in_bytes);
	GC_PTR status = NULL;
	InitGC ();
	//gc_realloc = GetProcAddress (moduleGC, "GC_realloc");

	gc_realloc = wine_dlsym(dmHandle,"GC_realloc", error, sizeof(error));
	TRACE ("GC_realloc %p, %s\n", gc_realloc, error);
	status = gc_realloc (old_object, new_size_in_bytes);
	TRACE ("GC_realloc end\n");
	return status;
}

GC_PTR GC_base (GC_PTR displaced_pointer)
{
	GC_PTR (*gc_base)(GC_PTR displaced_pointer);
	GC_PTR status = NULL;

	InitGC ();
	//gc_base = GetProcAddress (moduleGC, "GC_base");
	gc_base = wine_dlsym(dmHandle,"GC_base", error, sizeof(error));
	TRACE ("GC_base %p, %s\n", gc_base, error);
	status = gc_base (displaced_pointer);
	TRACE ("GC_base end\n");
	return status;
}

void GC_free (GC_PTR object_addr)
{
	GC_PTR (*gc_free)(GC_PTR object_addr);
	InitGC ();
	//gc_free = GetProcAddress (moduleGC, "GC_free");
	gc_free = wine_dlsym(dmHandle,"GC_free", error, sizeof(error));
	TRACE ("GC_free %p, %s\n", gc_free, error);
	gc_free (object_addr);
	TRACE ("GC_free end\n");
}

void GC_gcollect ()
{
	GC_PTR (*gc_gcollect)();
	InitGC ();
	//printf ("GC_gcollect start\n");
	//gc_gcollect = GetProcAddress (moduleGC, "GC_gcollect");
	gc_gcollect = wine_dlsym(dmHandle,"GC_gcollect", error, sizeof(error));
	TRACE ("GC_gcollect %p, %s\n", gc_gcollect, error);
	gc_gcollect ();
	TRACE ("GC_gcollect end\n");
}

size_t GC_get_heap_size ()
{
	size_t (*gc_get_heap_size)();
	size_t status = 0;

	InitGC ();
	//printf ("in GC_get_heap_size\n");
	//gc_get_heap_size = GetProcAddress (moduleGC, "GC_get_heap_size");
	//printf ("GC_get_heap_size start\n");
	gc_get_heap_size = wine_dlsym(dmHandle,"GC_get_heap_size", error, sizeof(error));
	TRACE ("GC_get_heap_size %p, %s\n", gc_get_heap_size, error);
	status = gc_get_heap_size ();
	TRACE ("GC_get_heap_size end\n");
	return status;
}

int GC_invoke_finalizers ()
{
	int (*gc_invoke_finalizers)();
	int status = 0;

	InitGC ();
	//gc_invoke_finalizers = GetProcAddress (moduleGC, "GC_invoke_finalizers");
	//printf ("GC_invoke_finalizers start\n");
	gc_invoke_finalizers = wine_dlsym(dmHandle,"GC_invoke_finalizers", error, sizeof(error));
	TRACE ("GC_invoke_finalizers %p, %s\n", gc_invoke_finalizers, error);
	status = gc_invoke_finalizers ();
	TRACE ("GC_invoke_finalizers end\n");
	return status;
}

int GC_unregister_disappearing_link (GC_PTR *link)
{
	printf ("GC_unregister_disappearing_link (not implenented)\n");
	return 0;
}

int GC_general_register_disappearing_link (GC_PTR *link, GC_PTR obj)
{
	printf ("GC_general_register_disappearing_link (not implemented)\n");
	return 0;
}

// GC pthread wrapper
int GC_pthread_create (pthread_t *new_thread,
		       const pthread_attr_t *attr,
		       void *(*start_routine)(void *), void *arg)
{
	TRACE ("GC_pthread_create\n");
	return pthread_create (new_thread, attr, start_routine, arg);
}

int GC_pthread_sigmask (int how, const sigset_t *set, sigset_t *oset)
{
	TRACE ("GC_pthread_sigmask\n");
	pthread_sigmask (how, set, oset);
}

int GC_pthread_join (pthread_t thread, void **retval)
{
	TRACE ("GC_pthread_join\n");
	return pthread_join (thread, retval);
}

int GC_pthread_detach (pthread_t thread)
{
	TRACE ("GC_pthread_detach\n");
	return pthread_detach (thread);
}


