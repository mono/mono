//
// monogc.c
//
// Author:
// implementation started by John Sohn (jsohn@columbus.rr.com)
// Alexandre Pigolkine (pigolkine@gmx.de)
// Jonathan Gilbert
//
// (C) Ximian, Inc., 2002/2003
//
#include <windows.h>
#include <stdio.h>
#include <gc/gc.h>
#include <gc/gc_typed.h>
#include <pthread.h>

int GC_finalize_on_demand = -1;
void (* GC_finalizer_notifier)() = (void (*) GC_PROTO((void)))0;
HMODULE moduleGC;

typedef GC_PTR (*gc_malloc_ptrt)(size_t lb);
typedef int (*gc_push_all_stack_ptrt)(GC_PTR b, GC_PTR t);
typedef int (*gc_init_gcj_malloc_ptrt)(int mp_index, void * mp);
typedef void* (* gc_gcj_malloc_ptrt)(size_t lb, void * ptr_to_struct_containing_descr);
typedef int (*gc_make_descriptor_ptrt)(GC_bitmap b, size_t t);
typedef int (*gc_should_invoke_finalizers_ptrt)(void);
typedef void (*gc_register_finalizer_ptrt)(GC_PTR obj, GC_finalization_proc fn,
        		                   GC_PTR cd, GC_finalization_proc *ofn, GC_PTR *ocd);
typedef GC_PTR (*gc_malloc_atomic_ptrt)(size_t lb);
typedef GC_PTR (*gc_realloc_ptrt)(GC_PTR old_object, size_t new_size_in_bytes);
typedef GC_PTR (*gc_base_ptrt)(GC_PTR displaced_pointer);
typedef GC_PTR (*gc_free_ptrt)(GC_PTR object_addr);
typedef GC_PTR (*gc_gcollect_ptrt)();
typedef size_t (*gc_get_heap_size_ptrt)();
typedef int (*gc_invoke_finalizers_ptrt)();

gc_malloc_ptrt             gc_malloc;
gc_push_all_stack_ptrt     gc_push_all_stack;
gc_init_gcj_malloc_ptrt    gc_init_gcj_malloc;
gc_gcj_malloc_ptrt         gc_gcj_malloc;
gc_make_descriptor_ptrt    gc_make_descriptor;
gc_should_invoke_finalizers_ptrt  gc_should_invoke_finalizers;
gc_register_finalizer_ptrt gc_register_finalizer;
gc_malloc_atomic_ptrt      gc_malloc_atomic;
gc_realloc_ptrt            gc_realloc;
gc_base_ptrt               gc_base;
gc_free_ptrt               gc_free;
gc_gcollect_ptrt           gc_gcollect;
gc_get_heap_size_ptrt      gc_get_heap_size;
gc_invoke_finalizers_ptrt  gc_invoke_finalizers;

static int printf_stub(const char *format,...)
{
	return 0;
}

#define TRACE printf_stub

void InitGC ()
{
	printf_stub(0); // suppress warning
	if(moduleGC == 0){
		printf ("Initializing Boehm GC library...\n");
		moduleGC = LoadLibraryA ("gc.dll");

		if (moduleGC == NULL) {
			printf ("Boehm GC library cannot be loaded");
			exit (1);
		}

		gc_malloc = (gc_malloc_ptrt)GetProcAddress (moduleGC, "GC_malloc");
		gc_push_all_stack = (gc_push_all_stack_ptrt) GetProcAddress (moduleGC, "GC_push_all_stack");
		gc_init_gcj_malloc = (gc_init_gcj_malloc_ptrt) GetProcAddress (moduleGC, "GC_init_gcj_malloc");
		gc_gcj_malloc = (gc_gcj_malloc_ptrt) GetProcAddress (moduleGC, "GC_gcj_malloc");
		gc_make_descriptor = (gc_make_descriptor_ptrt) GetProcAddress (moduleGC, "GC_make_descriptor");
		gc_should_invoke_finalizers = (gc_should_invoke_finalizers_ptrt) 
					GetProcAddress (moduleGC, "GC_should_invoke_finalizers");
		gc_register_finalizer =
			(gc_register_finalizer_ptrt)GetProcAddress (moduleGC, "GC_register_finalizer");
		gc_malloc_atomic = (gc_malloc_atomic_ptrt)GetProcAddress (moduleGC, "GC_malloc_atomic");
		gc_realloc = (gc_realloc_ptrt)GetProcAddress (moduleGC, "GC_realloc");
		gc_base = (gc_base_ptrt)GetProcAddress (moduleGC, "GC_base");
		gc_free = (gc_free_ptrt) GetProcAddress (moduleGC, "GC_free");
		gc_gcollect = (gc_gcollect_ptrt) GetProcAddress (moduleGC, "GC_gcollect");
		gc_get_heap_size = (gc_get_heap_size_ptrt) GetProcAddress (moduleGC, "GC_get_heap_size");
		gc_invoke_finalizers =
			(gc_invoke_finalizers_ptrt) GetProcAddress (moduleGC, "GC_invoke_finalizers");

		printf("done!\n");
	}
}

GC_PTR GC_malloc (size_t lb)
{
	GC_PTR status = NULL;
	InitGC ();

	TRACE ("GC_malloc start\n");
	status = gc_malloc (lb);
	TRACE ("GC_malloc end\n");
	return status;
}

void GC_push_all_stack (GC_PTR b, GC_PTR t)
{
	InitGC ();
	TRACE ("GC_push_all_stack\n");
	gc_push_all_stack (b,t);
	TRACE ("GC_push_all_stack end\n");
}

void GC_init_gcj_malloc(int mp_index, void * /* really GC_mark_proc */mp)
{
	InitGC ();
	TRACE ("GC_init_gcj_malloc\n");
	gc_init_gcj_malloc (mp_index,mp);
	TRACE ("GC_init_gcj_malloc end\n");
}

void * GC_gcj_malloc(size_t lb, void * ptr_to_struct_containing_descr)
{
	void *result = 0;
	InitGC ();
	TRACE ("gc_gcj_malloc bytes %d\n", lb);
	result = gc_gcj_malloc (lb,ptr_to_struct_containing_descr);
	TRACE ("gc_gcj_malloc end %p\n", result);
	return result;
}

GC_descr GC_make_descriptor(GC_bitmap bm, size_t len)
{
	GC_descr result = 0;
	InitGC ();
	TRACE ("GC_make_descriptor\n");
	result = gc_make_descriptor (bm,len);
	TRACE ("GC_make_descriptor end\n");
	return result;
}

int GC_should_invoke_finalizers(void)
{
	int result = 0;
	InitGC ();
	TRACE ("GC_should_invoke_finalizers\n");
	result = gc_should_invoke_finalizers ();
	TRACE ("GC_should_invoke_finalizers end\n");
	return result;
}

void GC_register_finalizer (GC_PTR obj, GC_finalization_proc fn, GC_PTR cd,
		GC_finalization_proc *ofn, GC_PTR *ocd)
{
	InitGC ();
	TRACE ("GC_register_finalizer\n");
	gc_register_finalizer (obj, fn, cd, ofn, ocd);
	TRACE ("GC_register_finalizer end\n");
}

void GC_register_finalizer_no_order (GC_PTR obj, GC_finalization_proc fn,
				     GC_PTR cd, GC_finalization_proc *ofn,
				     GC_PTR *ocd)
{
	InitGC ();
	TRACE ("GC_register_finalizer_no_order\n");
	gc_register_finalizer (obj, fn, cd, ofn, ocd);
	TRACE ("GC_register_finalizer_no_order end\n");
}


GC_PTR GC_debug_malloc (size_t size_in_bytes, GC_EXTRA_PARAMS)
{
	return GC_malloc (size_in_bytes);
}

GC_PTR GC_malloc_atomic (size_t size_in_bytes)
{
	GC_PTR status = NULL;
	InitGC ();
	TRACE ("GC_malloc_atomic start\n");
	status = gc_malloc_atomic (size_in_bytes);
	TRACE ("GC_malloc_atomic end\n");
	return status;
}

GC_PTR GC_realloc (GC_PTR old_object, size_t new_size_in_bytes)
{
	GC_PTR status = NULL;
	InitGC ();
	TRACE ("GC_realloc start\n");
	status = gc_realloc (old_object, new_size_in_bytes);
	TRACE ("GC_realloc end\n");
	return status;
}

GC_PTR GC_base (GC_PTR displaced_pointer)
{
	GC_PTR status = NULL;
	InitGC ();

	TRACE ("GC_base start\n");
	status = gc_base (displaced_pointer);
	TRACE ("GC_base end\n");
	return status;
}

void GC_free (GC_PTR object_addr)
{
	InitGC ();
	TRACE ("GC_free start\n");
	gc_free (object_addr);
	TRACE ("GC_free end\n");
}

void GC_gcollect ()
{
	InitGC ();
	TRACE ("GC_gcollect start\n");
	gc_gcollect ();
	TRACE ("GC_gcollect end\n");
}

size_t GC_get_heap_size ()
{
	size_t status = 0;
	InitGC ();

	TRACE ("GC_get_heap_size start\n");
	status = gc_get_heap_size ();
	TRACE ("GC_get_heap_size end\n");
	return status;
}

int GC_invoke_finalizers ()
{
	int status = 0;
	InitGC ();

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
