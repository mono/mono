/*
 * processes.c:  Process handles
 *
 * Author:
 *	Dick Porter (dick@ximian.com)
 *
 * (C) 2002-2011 Novell, Inc.
 * Copyright 2011 Xamarin Inc
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

#include <config.h>
#include <glib.h>
#include <stdio.h>
#include <string.h>
#include <pthread.h>
#include <sched.h>
#include <sys/time.h>
#include <errno.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <unistd.h>
#ifdef HAVE_SIGNAL_H
#include <signal.h>
#endif
#include <sys/time.h>
#include <fcntl.h>
#ifdef HAVE_SYS_PARAM_H
#include <sys/param.h>
#endif
#include <ctype.h>

#ifdef HAVE_SYS_WAIT_H
#include <sys/wait.h>
#endif
#ifdef HAVE_SYS_RESOURCE_H
#include <sys/resource.h>
#endif

#ifdef HAVE_SYS_MKDEV_H
#include <sys/mkdev.h>
#endif

#ifdef HAVE_UTIME_H
#include <utime.h>
#endif

/* sys/resource.h (for rusage) is required when using osx 10.3 (but not 10.4) */
#ifdef __APPLE__
#include <TargetConditionals.h>
#include <sys/resource.h>
#ifdef HAVE_LIBPROC_H
/* proc_name */
#include <libproc.h>
#endif
#endif

#if defined(PLATFORM_MACOSX)
#define USE_OSX_LOADER
#endif

#if ( defined(__OpenBSD__) || defined(__FreeBSD__) ) && defined(HAVE_LINK_H)
#define USE_BSD_LOADER
#endif

#if defined(__HAIKU__)
#define USE_HAIKU_LOADER
#endif

#if defined(USE_OSX_LOADER) || defined(USE_BSD_LOADER)
#include <sys/proc.h>
#include <sys/sysctl.h>
#  if !defined(__OpenBSD__)
#    include <sys/utsname.h>
#  endif
#  if defined(__FreeBSD__)
#    include <sys/user.h>  /* struct kinfo_proc */
#  endif
#endif

#ifdef PLATFORM_SOLARIS
/* procfs.h cannot be included if this define is set, but it seems to work fine if it is undefined */
#if _FILE_OFFSET_BITS == 64
#undef _FILE_OFFSET_BITS
#include <procfs.h>
#define _FILE_OFFSET_BITS 64
#else
#include <procfs.h>
#endif
#endif

#ifdef __HAIKU__
#include <KernelKit.h>
#endif

#include <mono/io-layer/wapi.h>
#include <mono/io-layer/wapi-private.h>
#include <mono/io-layer/process-private.h>
#include <mono/io-layer/io-trace.h>
#include <mono/utils/strenc.h>
#include <mono/utils/mono-path.h>
#include <mono/io-layer/timefuncs.h>
#include <mono/utils/mono-time.h>
#include <mono/utils/mono-membar.h>
#include <mono/utils/mono-os-mutex.h>
#include <mono/utils/mono-signal-handler.h>
#include <mono/utils/mono-proclib.h>
#include <mono/utils/mono-once.h>
#include <mono/utils/mono-logger-internals.h>
#include <mono/metadata/w32handle.h>

static guint32 process_wait (gpointer handle, guint32 timeout, gboolean *alerted);
static void process_close (gpointer handle, gpointer data);
static void process_details (gpointer data);
static const gchar* process_typename (void);
static gsize process_typesize (void);

static MonoW32HandleOps _wapi_process_ops = {
	process_close,		/* close_shared */
	NULL,				/* signal */
	NULL,				/* own */
	NULL,				/* is_owned */
	process_wait,			/* special_wait */
	NULL,				/* prewait */
	process_details,	/* details */
	process_typename,	/* typename */
	process_typesize,	/* typesize */
};

/* The signal-safe logic to use mono_processes goes like this:
 * - The list must be safe to traverse for the signal handler at all times.
 *   It's safe to: prepend an entry (which is a single store to 'mono_processes'),
 *   unlink an entry (assuming the unlinked entry isn't freed and doesn't 
 *   change its 'next' pointer so that it can still be traversed).
 * When cleaning up we first unlink an entry, then we verify that
 * the read lock isn't locked. Then we can free the entry, since
 * we know that nobody is using the old version of the list (including
 * the unlinked entry).
 * We also need to lock when adding and cleaning up so that those two
 * operations don't mess with eachother. (This lock is not used in the
 * signal handler)
 */
static MonoProcess *mono_processes = NULL;
static volatile gint32 mono_processes_cleaning_up = 0;
static mono_mutex_t mono_processes_mutex;
static void mono_processes_cleanup (void);

static gpointer current_process;

static WapiHandle_process *
lookup_process_handle (gpointer handle)
{
	WapiHandle_process *process_data;
	gboolean ret;

	ret = mono_w32handle_lookup (handle, MONO_W32HANDLE_PROCESS,
							   (gpointer *)&process_data);
	if (!ret)
		return NULL;
	return process_data;
}

static void
process_set_defaults (WapiHandle_process *process_handle)
{
	/* These seem to be the defaults on w2k */
	process_handle->min_working_set = 204800;
	process_handle->max_working_set = 1413120;
	
	_wapi_time_t_to_filetime (time (NULL), &process_handle->create_time);
}

static int
len16 (const gunichar2 *str)
{
	int len = 0;
	
	while (*str++ != 0)
		len++;

	return len;
}

static gunichar2 *
utf16_concat (const gunichar2 *first, ...)
{
	va_list args;
	int total = 0, i;
	const gunichar2 *s;
	gunichar2 *ret;

	va_start (args, first);
	total += len16 (first);
        for (s = va_arg (args, gunichar2 *); s != NULL; s = va_arg(args, gunichar2 *)){
		total += len16 (s);
        }
	va_end (args);

	ret = g_new (gunichar2, total + 1);
	if (ret == NULL)
		return NULL;

	ret [total] = 0;
	i = 0;
	for (s = first; *s != 0; s++)
		ret [i++] = *s;
	va_start (args, first);
	for (s = va_arg (args, gunichar2 *); s != NULL; s = va_arg (args, gunichar2 *)){
		const gunichar2 *p;
		
		for (p = s; *p != 0; p++)
			ret [i++] = *p;
	}
	va_end (args);
	
	return ret;
}

#ifdef DEBUG_ENABLED
/* Useful in gdb */
void
print_utf16 (gunichar2 *str)
{
	char *res;

	res = g_utf16_to_utf8 (str, -1, NULL, NULL, NULL);
	g_print ("%s\n", res);
	g_free (res);
}
#endif

static void
process_set_name (WapiHandle_process *process_handle)
{
	char *progname, *utf8_progname, *slash;
	
	progname = g_get_prgname ();
	utf8_progname = mono_utf8_from_external (progname);

	MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: using [%s] as prog name", __func__, progname);

	if (utf8_progname) {
		slash = strrchr (utf8_progname, '/');
		if (slash)
			process_handle->proc_name = g_strdup (slash+1);
		else
			process_handle->proc_name = g_strdup (utf8_progname);
		g_free (utf8_progname);
	}
}

void
_wapi_processes_init (void)
{
	WapiHandle_process process_handle;

	mono_w32handle_register_ops (MONO_W32HANDLE_PROCESS, &_wapi_process_ops);

	mono_w32handle_register_capabilities (MONO_W32HANDLE_PROCESS,
		(MonoW32HandleCapability)(MONO_W32HANDLE_CAP_WAIT | MONO_W32HANDLE_CAP_SPECIAL_WAIT));

	memset (&process_handle, 0, sizeof (process_handle));
	process_handle.id = wapi_getpid ();
	process_set_defaults (&process_handle);
	process_set_name (&process_handle);

	current_process = mono_w32handle_new (MONO_W32HANDLE_PROCESS, &process_handle);
	g_assert (current_process);

	mono_os_mutex_init (&mono_processes_mutex);
}

gpointer
_wapi_process_duplicate (void)
{
	mono_w32handle_ref (current_process);
	
	return current_process;
}

static char *
get_process_name_from_proc (pid_t pid)
{
#if defined(USE_BSD_LOADER)
	int mib [6];
	size_t size;
	struct kinfo_proc *pi;
#elif defined(USE_OSX_LOADER)
#if !(!defined (__mono_ppc__) && defined (TARGET_OSX))
	size_t size;
	struct kinfo_proc *pi;
	int mib[] = { CTL_KERN, KERN_PROC, KERN_PROC_PID, pid };
#endif
#else
	FILE *fp;
	char *filename = NULL;
#endif
	char buf[256];
	char *ret = NULL;

#if defined(PLATFORM_SOLARIS)
	filename = g_strdup_printf ("/proc/%d/psinfo", pid);
	if ((fp = fopen (filename, "r")) != NULL) {
		struct psinfo info;
		int nread;

		nread = fread (&info, sizeof (info), 1, fp);
		if (nread == 1) {
			ret = g_strdup (info.pr_fname);
		}

		fclose (fp);
	}
	g_free (filename);
#elif defined(USE_OSX_LOADER)
#if !defined (__mono_ppc__) && defined (TARGET_OSX)
	/* No proc name on OSX < 10.5 nor ppc nor iOS */
	memset (buf, '\0', sizeof(buf));
	proc_name (pid, buf, sizeof(buf));

	// Fixes proc_name triming values to 15 characters #32539
	if (strlen (buf) >= MAXCOMLEN - 1) {
		char path_buf [PROC_PIDPATHINFO_MAXSIZE];
		char *name_buf;
		int path_len;

		memset (path_buf, '\0', sizeof(path_buf));
		path_len = proc_pidpath (pid, path_buf, sizeof(path_buf));

		if (path_len > 0 && path_len < sizeof(path_buf)) {
			name_buf = path_buf + path_len;
			for(;name_buf > path_buf; name_buf--) {
				if (name_buf [0] == '/') {
					name_buf++;
					break;
				}
			}

			if (memcmp (buf, name_buf, MAXCOMLEN - 1) == 0)
				ret = g_strdup (name_buf);
		}
	}

	if (ret == NULL && strlen (buf) > 0)
		ret = g_strdup (buf);
#else
	if (sysctl(mib, 4, NULL, &size, NULL, 0) < 0)
		return(ret);

	if ((pi = g_malloc (size)) == NULL)
		return(ret);

	if (sysctl (mib, 4, pi, &size, NULL, 0) < 0) {
		if (errno == ENOMEM) {
			g_free (pi);
			MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: Didn't allocate enough memory for kproc info", __func__);
		}
		return(ret);
	}

	if (strlen (pi->kp_proc.p_comm) > 0)
		ret = g_strdup (pi->kp_proc.p_comm);

	g_free (pi);
#endif
#elif defined(USE_BSD_LOADER)
#if defined(__FreeBSD__)
	mib [0] = CTL_KERN;
	mib [1] = KERN_PROC;
	mib [2] = KERN_PROC_PID;
	mib [3] = pid;
	if (sysctl(mib, 4, NULL, &size, NULL, 0) < 0) {
		MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: sysctl() failed: %d", __func__, errno);
		return(ret);
	}

	if ((pi = g_malloc (size)) == NULL)
		return(ret);

	if (sysctl (mib, 4, pi, &size, NULL, 0) < 0) {
		if (errno == ENOMEM) {
			g_free (pi);
			MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: Didn't allocate enough memory for kproc info", __func__);
		}
		return(ret);
	}

	if (strlen (pi->ki_comm) > 0)
		ret = g_strdup (pi->ki_comm);
	g_free (pi);
#elif defined(__OpenBSD__)
	mib [0] = CTL_KERN;
	mib [1] = KERN_PROC;
	mib [2] = KERN_PROC_PID;
	mib [3] = pid;
	mib [4] = sizeof(struct kinfo_proc);
	mib [5] = 0;

retry:
	if (sysctl(mib, 6, NULL, &size, NULL, 0) < 0) {
		MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: sysctl() failed: %d", __func__, errno);
		return(ret);
	}

	if ((pi = g_malloc (size)) == NULL)
		return(ret);

	mib[5] = (int)(size / sizeof(struct kinfo_proc));

	if ((sysctl (mib, 6, pi, &size, NULL, 0) < 0) ||
		(size != sizeof (struct kinfo_proc))) {
		if (errno == ENOMEM) {
			g_free (pi);
			goto retry;
		}
		return(ret);
	}

	if (strlen (pi->p_comm) > 0)
		ret = g_strdup (pi->p_comm);

	g_free (pi);
#endif
#elif defined(USE_HAIKU_LOADER)
	image_info imageInfo;
	int32 cookie = 0;

	if (get_next_image_info ((team_id)pid, &cookie, &imageInfo) == B_OK) {
		ret = g_strdup (imageInfo.name);
	}
#else
	memset (buf, '\0', sizeof(buf));
	filename = g_strdup_printf ("/proc/%d/exe", pid);
	if (readlink (filename, buf, 255) > 0) {
		ret = g_strdup (buf);
	}
	g_free (filename);

	if (ret != NULL) {
		return(ret);
	}

	filename = g_strdup_printf ("/proc/%d/cmdline", pid);
	if ((fp = fopen (filename, "r")) != NULL) {
		if (fgets (buf, 256, fp) != NULL) {
			ret = g_strdup (buf);
		}
		
		fclose (fp);
	}
	g_free (filename);

	if (ret != NULL) {
		return(ret);
	}
	
	filename = g_strdup_printf ("/proc/%d/stat", pid);
	if ((fp = fopen (filename, "r")) != NULL) {
		if (fgets (buf, 256, fp) != NULL) {
			char *start, *end;
			
			start = strchr (buf, '(');
			if (start != NULL) {
				end = strchr (start + 1, ')');
				
				if (end != NULL) {
					ret = g_strndup (start + 1,
							 end - start - 1);
				}
			}
		}
		
		fclose (fp);
	}
	g_free (filename);
#endif

	return ret;
}

/*
 * wapi_process_get_path:
 *
 *   Return the full path of the executable of the process PID, or NULL if it cannot be determined.
 * Returns malloc-ed memory.
 */
char*
wapi_process_get_path (pid_t pid)
{
#if defined(PLATFORM_MACOSX) && !defined(__mono_ppc__) && defined(TARGET_OSX)
	char buf [PROC_PIDPATHINFO_MAXSIZE];
	int res;

	res = proc_pidpath (pid, buf, sizeof (buf));
	if (res <= 0)
		return NULL;
	if (buf [0] == '\0')
		return NULL;
	return g_strdup (buf);
#else
	return get_process_name_from_proc (pid);
#endif
}

static void
mono_processes_cleanup (void)
{
	MonoProcess *mp;
	MonoProcess *prev = NULL;
	GSList *finished = NULL;
	GSList *l;
	gpointer unref_handle;

	MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s", __func__);

	/* Ensure we're not in here in multiple threads at once, nor recursive. */
	if (InterlockedCompareExchange (&mono_processes_cleaning_up, 1, 0) != 0)
		return;

	for (mp = mono_processes; mp; mp = mp->next) {
		if (mp->pid == 0 && mp->handle) {
			/* This process has exited and we need to remove the artifical ref
			 * on the handle */
			mono_os_mutex_lock (&mono_processes_mutex);
			unref_handle = mp->handle;
			mp->handle = NULL;
			mono_os_mutex_unlock (&mono_processes_mutex);
			if (unref_handle)
				mono_w32handle_unref (unref_handle);
		}
	}

	/*
	 * Remove processes which exited from the mono_processes list.
	 * We need to synchronize with the sigchld handler here, which runs
	 * asynchronously. The handler requires that the mono_processes list
	 * remain valid.
	 */
	mono_os_mutex_lock (&mono_processes_mutex);

	mp = mono_processes;
	while (mp) {
		if (mp->handle_count == 0 && mp->freeable) {
			/*
			 * Unlink the entry.
			 * This code can run parallel with the sigchld handler, but the
			 * modifications it makes are safe.
			 */
			if (mp == mono_processes)
				mono_processes = mp->next;
			else
				prev->next = mp->next;
			finished = g_slist_prepend (finished, mp);

			mp = mp->next;
		} else {
			prev = mp;
			mp = mp->next;
		}
	}

	mono_memory_barrier ();

	for (l = finished; l; l = l->next) {
		/*
		 * All the entries in the finished list are unlinked from mono_processes, and
		 * they have the 'finished' flag set, which means the sigchld handler is done
		 * accessing them.
		 */
		mp = (MonoProcess *)l->data;
		mono_os_sem_destroy (&mp->exit_sem);
		g_free (mp);
	}
	g_slist_free (finished);

	mono_os_mutex_unlock (&mono_processes_mutex);

	MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s done", __func__);

	InterlockedDecrement (&mono_processes_cleaning_up);
}

static void
process_close (gpointer handle, gpointer data)
{
	WapiHandle_process *process_handle;

	MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s", __func__);

	process_handle = (WapiHandle_process *) data;
	g_free (process_handle->proc_name);
	process_handle->proc_name = NULL;
	if (process_handle->mono_process)
		InterlockedDecrement (&process_handle->mono_process->handle_count);
	mono_processes_cleanup ();
}

static void process_details (gpointer data)
{
	WapiHandle_process *process_handle = (WapiHandle_process *) data;
	g_print ("id: %d, exited: %s, exitstatus: %d",
		process_handle->id, process_handle->exited ? "true" : "false", process_handle->exitstatus);
}

static const gchar* process_typename (void)
{
	return "Process";
}

static gsize process_typesize (void)
{
	return sizeof (WapiHandle_process);
}

static guint32
process_wait (gpointer handle, guint32 timeout, gboolean *alerted)
{
	WapiHandle_process *process_handle;
	pid_t pid G_GNUC_UNUSED, ret;
	int status;
	gint64 start, now;
	MonoProcess *mp;

	/* FIXME: We can now easily wait on processes that aren't our own children,
	 * but WaitFor*Object won't call us for pseudo handles. */
	g_assert ((GPOINTER_TO_UINT (handle) & _WAPI_PROCESS_UNHANDLED) != _WAPI_PROCESS_UNHANDLED);

	MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s (%p, %u)", __func__, handle, timeout);

	if (alerted)
		*alerted = FALSE;

	process_handle = lookup_process_handle (handle);
	if (!process_handle) {
		g_warning ("%s: error looking up process handle %p", __func__, handle);
		return WAIT_FAILED;
	}

	if (process_handle->exited) {
		/* We've already done this one */
		MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s (%p, %u): Process already exited", __func__, handle, timeout);
		return WAIT_OBJECT_0;
	}

	pid = process_handle->id;

	MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s (%p, %u): PID: %d", __func__, handle, timeout, pid);

	/* We don't need to lock mono_processes here, the entry
	 * has a handle_count > 0 which means it will not be freed. */
	mp = process_handle->mono_process;
	if (!mp) {
		pid_t res;

		if (pid == mono_process_current_pid ()) {
			MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s (%p, %u): waiting on current process", __func__, handle, timeout);
			return WAIT_TIMEOUT;
		}

		/* This path is used when calling Process.HasExited, so
		 * it is only used to poll the state of the process, not
		 * to actually wait on it to exit */
		g_assert (timeout == 0);

		MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s (%p, %u): waiting on non-child process", __func__, handle, timeout);

		res = waitpid (pid, &status, WNOHANG);
		if (res == 0) {
			MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s (%p, %u): non-child process WAIT_TIMEOUT", __func__, handle, timeout);
			return WAIT_TIMEOUT;
		}
		if (res > 0) {
			MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s (%p, %u): non-child process waited successfully", __func__, handle, timeout);
			return WAIT_OBJECT_0;
		}

		MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s (%p, %u): non-child process WAIT_FAILED, error : %s (%d))", __func__, handle, timeout, g_strerror (errno), errno);
		return WAIT_FAILED;
	}

	start = mono_msec_ticks ();
	now = start;

	while (1) {
		if (timeout != INFINITE) {
			MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s (%p, %u): waiting on semaphore for %li ms...", 
				    __func__, handle, timeout, (long)(timeout - (now - start)));
			ret = mono_os_sem_timedwait (&mp->exit_sem, (timeout - (now - start)), alerted ? MONO_SEM_FLAGS_ALERTABLE : MONO_SEM_FLAGS_NONE);
		} else {
			MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s (%p, %u): waiting on semaphore forever...", 
				   __func__, handle, timeout);
			ret = mono_os_sem_wait (&mp->exit_sem, alerted ? MONO_SEM_FLAGS_ALERTABLE : MONO_SEM_FLAGS_NONE);
		}

		if (ret == MONO_SEM_TIMEDWAIT_RET_SUCCESS) {
			/* Success, process has exited */
			mono_os_sem_post (&mp->exit_sem);
			break;
		}

		if (ret == MONO_SEM_TIMEDWAIT_RET_TIMEDOUT) {
			MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s (%p, %u): WAIT_TIMEOUT (timeout = 0)", __func__, handle, timeout);
			return WAIT_TIMEOUT;
		}

		now = mono_msec_ticks ();
		if (now - start >= timeout) {
			MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s (%p, %u): WAIT_TIMEOUT", __func__, handle, timeout);
			return WAIT_TIMEOUT;
		}
		
		if (alerted && ret == MONO_SEM_TIMEDWAIT_RET_ALERTED) {
			MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s (%p, %u): WAIT_IO_COMPLETION", __func__, handle, timeout);
			*alerted = TRUE;
			return WAIT_IO_COMPLETION;
		}
	}

	/* Process must have exited */
	MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s (%p, %u): Waited successfully", __func__, handle, timeout);

	status = mp ? mp->status : 0;
	if (WIFSIGNALED (status))
		process_handle->exitstatus = 128 + WTERMSIG (status);
	else
		process_handle->exitstatus = WEXITSTATUS (status);
	_wapi_time_t_to_filetime (time (NULL), &process_handle->exit_time);

	process_handle->exited = TRUE;

	MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s (%p, %u): Setting pid %d signalled, exit status %d",
		   __func__, handle, timeout, process_handle->id, process_handle->exitstatus);

	mono_w32handle_set_signal_state (handle, TRUE, TRUE);

	return WAIT_OBJECT_0;
}
