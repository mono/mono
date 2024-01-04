/**
 * \file
 */

#include <config.h>

#if (defined(__linux__) && !defined(HOST_ANDROID)) || defined(__FreeBSD_kernel__)

#include <mono/utils/mono-threads.h>
#include <pthread.h>
#include <sys/syscall.h>

#if defined(UNITY)
// store the stack bounds and address in thread local storage.
//
// This is a performance improvement over using pthread functions to query these values.  This change is big
// enough to create a ~2.5 order of magnitude improvement in scripting_thread_has_sufficient_execution_stack
// dominated performance tests (i.e. things that invoke CloneObject a lot).

static MONO_KEYWORD_THREAD size_t s_stack_bounds = 0;
static MONO_KEYWORD_THREAD guint8 *s_stack_addr = NULL;
#endif

void
mono_threads_platform_get_stack_bounds (guint8 **staddr, size_t *stsize)
{
#if defined(UNITY)
	// unity specific change - cache the values we retrieve in thread local storage (TLS)
	if (s_stack_bounds)
	{
		*stsize = s_stack_bounds;
		*staddr = s_stack_addr;
	}
	else
	{
		pthread_attr_t attr;
		gint res;
 
		*staddr = NULL;
		*stsize = (size_t)-1;
 
		res = pthread_attr_init (&attr);
		if (G_UNLIKELY (res != 0))
			g_error ("%s: pthread_attr_init failed with \"%s\" (%d)", __func__, g_strerror (res), res);
 
		res = pthread_getattr_np (pthread_self (), &attr);
		if (G_UNLIKELY (res != 0))
			g_error ("%s: pthread_getattr_np failed with \"%s\" (%d)", __func__, g_strerror (res), res);
 
		res = pthread_attr_getstack (&attr, (void**)staddr, stsize);
		if (G_UNLIKELY (res != 0))
			g_error ("%s: pthread_attr_getstack failed with \"%s\" (%d)", __func__, g_strerror (res), res);
 
		res = pthread_attr_destroy (&attr);
		if (G_UNLIKELY (res != 0))
			g_error ("%s: pthread_attr_destroy failed with \"%s\" (%d)", __func__, g_strerror (res), res);
 
		s_stack_bounds = *stsize;
		s_stack_addr = *staddr;
	}
#else
	pthread_attr_t attr;
	gint res;

	*staddr = NULL;
	*stsize = (size_t)-1;

	res = pthread_attr_init (&attr);
	if (G_UNLIKELY (res != 0))
		g_error ("%s: pthread_attr_init failed with \"%s\" (%d)", __func__, g_strerror (res), res);

	res = pthread_getattr_np (pthread_self (), &attr);
	if (G_UNLIKELY (res != 0))
		g_error ("%s: pthread_getattr_np failed with \"%s\" (%d)", __func__, g_strerror (res), res);

	res = pthread_attr_getstack (&attr, (void**)staddr, stsize);
	if (G_UNLIKELY (res != 0))
		g_error ("%s: pthread_attr_getstack failed with \"%s\" (%d)", __func__, g_strerror (res), res);

	res = pthread_attr_destroy (&attr);
	if (G_UNLIKELY (res != 0))
		g_error ("%s: pthread_attr_destroy failed with \"%s\" (%d)", __func__, g_strerror (res), res);
#endif // defined(UNITY)
}

guint64
mono_native_thread_os_id_get (void)
{
	return (guint64)syscall (SYS_gettid);
}

#else

#include <mono/utils/mono-compiler.h>

MONO_EMPTY_SOURCE_FILE (mono_threads_linux);

#endif
