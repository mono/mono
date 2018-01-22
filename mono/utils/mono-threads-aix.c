/**
 * \file
 */

#include <config.h>

#if defined(_AIX)

#include <mono/utils/mono-threads.h>
#include <pthread.h>

void
mono_threads_platform_get_stack_bounds (guint8 **staddr, size_t *stsize)
{
	/* see GC_push_all_stacks in libgc/aix_irix_threads.c 
           for why we do this; pthread_getattr_np exists only
           on some versions of AIX and not on PASE, so use a
           legacy way to get the stack information */
	struct __pthrdsinfo pi;
	pthread_t pt;
	int res, rbv, ps;
	int rb[64];

	pt = pthread_self();
	ps = sizeof(pi);
	rbv = sizeof(rb);

	*staddr = NULL;
	*stsize = (size_t)-1;

	res = pthread_getthrds_np(&pt, PTHRDSINFO_QUERY_ALL, &pi, ps, rb, &rbv);
	/* FIXME: are these the right values? */
	*staddr = (void*)(pi.__pi_ustk - 288);
	*stsize = pi.__pi_stacksize;
}

#endif
