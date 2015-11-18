
#include "icall-wrapper.h"

#include "gc-internals.h"

#ifdef HAVE_SGEN_GC

struct _MonoIcallWrapperData {
	MonoIcallWrapperData *prev;
	gsize stackdata_size;
	gchar stackdata [MONO_ZERO_LEN_ARRAY];
};

static MonoGCDescriptor icall_wrapper_data_desc = MONO_GC_DESCRIPTOR_NULL;

static inline MonoIcallWrapperData*
icall_wrapper_data_alloc (gsize stackdata_size)
{
	return g_malloc0 (sizeof (MonoIcallWrapperData) + sizeof (gchar) * (stackdata_size - MONO_ZERO_LEN_ARRAY));
}

static inline void
icall_wrapper_data_free (MonoIcallWrapperData* data)
{
	return g_free (data);
}

static void
icall_wrapper_data_mark (void *addr, MonoGCMarkFunc mark_func, void *gc_data)
{
	MonoIcallWrapperData *current;

	printf ("icall_wrapper_data_mark\n");

	g_assert (addr);

	for (current = *(MonoIcallWrapperData**) addr; current; current = current->prev)
		mono_gc_conservatively_scan_area (&current->stackdata [0], &current->stackdata [current->stackdata_size - 1]);
}

static gsize*
frame_address ()
{
#ifdef _MSC_VER
	g_assert_not_reached ();
#else
	return __builtin_frame_address (0);
#endif
}

MonoIcallWrapperData*
mono_icall_wrapper_start (gsize *stackdata)
{
	gsize stackdata_size, *stackdata_end;
	MonoInternalThread *thread;
	MonoIcallWrapperData *data;

#ifdef _MSC_VER
	// FIXME
	g_assert_not_reached ();
#else
	__builtin_unwind_init ();
#endif

	stackdata_end = frame_address ();

	if (((gsize) stackdata & (SIZEOF_VOID_P - 1)) != 0)
		g_error ("stackdata (%p) must be %d-byte aligned", stackdata, SIZEOF_VOID_P);
	if (((gsize) stackdata_end & (SIZEOF_VOID_P - 1)) != 0)
		g_error ("stackdata_end (%p) must be %d-byte aligned", stackdata_end, SIZEOF_VOID_P);

	stackdata_size = (char*)stackdata - (char*)stackdata_end;

	if (stackdata_size <= 0)
		g_error ("stackdata_size = %d, but must be > 0, stackdata = %p, stackdata_end = %p", stackdata_size, stackdata, stackdata_end);

	printf ("stackdata_size = %"G_GSIZE_FORMAT"\n", stackdata_size);

	thread = mono_thread_internal_current ();
	g_assert (thread);

	data = icall_wrapper_data_alloc (stackdata_size);
	g_assert (data);

	data->stackdata_size = stackdata_size;
	memcpy (&data->stackdata [0], stackdata_end, data->stackdata_size);

	if (thread->icall_wrapper_data) {
		data->prev = thread->icall_wrapper_data;
	} else {
		if (!icall_wrapper_data_desc)
			icall_wrapper_data_desc = mono_gc_make_root_descr_user (icall_wrapper_data_mark);
		mono_gc_register_root ((char*) &thread->icall_wrapper_data, sizeof (MonoIcallWrapperData*), icall_wrapper_data_desc, MONO_ROOT_SOURCE_ICALL_WRAPPER_DATA, "runtime icall wrapper data");
	}

	return thread->icall_wrapper_data = data;
}

void
mono_icall_wrapper_end (MonoIcallWrapperData *data, gsize *stackdata)
{
	MonoInternalThread *thread;

	g_assert (data);

	thread = mono_thread_internal_current ();
	g_assert (thread);

	if (!data->prev)
		mono_gc_deregister_root ((char*) &thread->icall_wrapper_data);

	thread->icall_wrapper_data = data->prev;

	icall_wrapper_data_free (data);
}

#else

MonoIcallWrapperData*
mono_icall_wrapper_start (gsize *stackdata)
{
	return NULL;
}

void
mono_icall_wrapper_end (MonoIcallWrapperData *data, gsize *stackdata)
{
}

#endif
