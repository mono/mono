#include <windows.h>
#include <stdio.h>
#include <gc/gc.h>
#include <pthread.h>

int GC_finalize_on_demand = -1;
void (* GC_finalizer_notifier)() = (void (*) GC_PROTO((void)))0;
HMODULE moduleGC;

static int printf_stub(const char *format,...)
{
	return 0;
}

#define TRACE printf_stub

void InitGC ()
{
	if(moduleGC == 0){
		printf ("Initializing Boehm GC library...\n");
		moduleGC = LoadLibraryA ("gc.dll");

		if (moduleGC == NULL) {
			exit (1);
		}
	}

}

GC_PTR GC_malloc (size_t lb)
{
	GC_PTR (*gc_malloc)(size_t lb);
	GC_PTR status = NULL;
	InitGC ();
	gc_malloc = GetProcAddress (moduleGC, "GC_malloc");

	TRACE ("GC_malloc start\n");
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
	TRACE ("GC_register_finalizer\n");
	gc_register_finalizer =
		GetProcAddress (moduleGC, "GC_register_finalizer");
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
	gc_register_finalizer_no_order =
		GetProcAddress (moduleGC, "GC_register_finalizer");
	TRACE ("GC_register_finalizer_no_order\n");
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
	gc_malloc_atomic = GetProcAddress (moduleGC, "GC_malloc_atomic");

	TRACE ("GC_malloc_atomic start\n");
	status = gc_malloc_atomic (size_in_bytes);
	TRACE ("GC_malloc_atomic end\n");
	return status;
}

GC_PTR GC_realloc (GC_PTR old_object, size_t new_size_in_bytes)
{
	GC_PTR (*gc_realloc)(GC_PTR old_object, size_t new_size_in_bytes);
	GC_PTR status = NULL;
	InitGC ();
	gc_realloc = GetProcAddress (moduleGC, "GC_realloc");

	TRACE ("GC_realloc start\n");
	status = gc_realloc (old_object, new_size_in_bytes);
	TRACE ("GC_realloc end\n");
	return status;
}

GC_PTR GC_base (GC_PTR displaced_pointer)
{
	GC_PTR (*gc_base)(GC_PTR displaced_pointer);
	GC_PTR status = NULL;
	InitGC ();

	TRACE ("in GC_base\n");
	gc_base = GetProcAddress (moduleGC, "GC_base");
	TRACE ("GC_base start\n");
	status = gc_base (displaced_pointer);
	TRACE ("GC_base end\n");
	return status;
}

void GC_free (GC_PTR object_addr)
{
	GC_PTR (*gc_free)(GC_PTR object_addr);
	InitGC ();
	TRACE ("GC_free start\n");
	gc_free = GetProcAddress (moduleGC, "GC_free");
	gc_free (object_addr);
	TRACE ("GC_free end\n");
}

void GC_gcollect ()
{
	GC_PTR (*gc_gcollect)();
	InitGC ();
	TRACE ("GC_gcollect start\n");
	gc_gcollect = GetProcAddress (moduleGC, "GC_gcollect");
	gc_gcollect ();
	TRACE ("GC_gcollect end\n");
}

size_t GC_get_heap_size ()
{
	size_t (*gc_get_heap_size)();
	size_t status = 0;
	InitGC ();

	TRACE ("in GC_get_heap_size\n");
	gc_get_heap_size = GetProcAddress (moduleGC, "GC_get_heap_size");
	TRACE ("GC_get_heap_size start\n");
	status = gc_get_heap_size ();
	TRACE ("GC_get_heap_size end\n");
	return status;
}

int GC_invoke_finalizers ()
{
	int (*gc_invoke_finalizers)();
	int status = 0;
	InitGC ();

	gc_invoke_finalizers = GetProcAddress (moduleGC, "GC_invoke_finalizers");
	TRACE ("GC_invoke_finalizers start\n");
	status = gc_invoke_finalizers ();
	TRACE ("GC_invoke_finalizers end\n");
	return status;
}

int GC_unregister_disappearing_link (GC_PTR *link)
{
	TRACE ("GC_unregister_disappearing_link (not implenented)\n");
	return 0;
}

int GC_general_register_disappearing_link (GC_PTR *link, GC_PTR obj)
{
	TRACE ("GC_general_register_disappearing_link (not implemented)\n");
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


