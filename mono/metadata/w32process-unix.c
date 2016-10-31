/*
 * process.c: System.Diagnostics.Process support
 *
 * Author:
 *	Dick Porter (dick@ximian.com)
 *
 * Copyright 2002 Ximian, Inc.
 * Copyright 2002-2006 Novell, Inc.
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
#if !defined(__OpenBSD__)
#include <sys/utsname.h>
#endif
#if defined(__FreeBSD__)
#include <sys/user.h> /* struct kinfo_proc */
#endif
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

#if defined(USE_HAIKU_LOADER)
#include <KernelKit.h>
#endif

#include <mono/metadata/w32process.h>
#include <mono/metadata/class.h>
#include <mono/metadata/class-internals.h>
#include <mono/metadata/object.h>
#include <mono/metadata/object-internals.h>
#include <mono/metadata/metadata.h>
#include <mono/metadata/metadata-internals.h>
#include <mono/metadata/exception.h>
#include <mono/io-layer/io-layer.h>
#include <mono/metadata/w32handle.h>
#include <mono/utils/mono-membar.h>
#include <mono/utils/mono-logger-internals.h>
#include <mono/utils/strenc.h>
#include <mono/utils/mono-proclib.h>
#include <mono/utils/mono-path.h>
#include <mono/utils/mono-lazy-init.h>
#include <mono/utils/mono-signal-handler.h>
#include <mono/utils/mono-time.h>

#ifndef MAXPATHLEN
#define MAXPATHLEN 242
#endif

#define STILL_ACTIVE ((int) 0x00000103)

#define LOGDEBUG(...)
/* define LOGDEBUG(...) g_message(__VA_ARGS__)  */

/* The process' environment strings */
#if defined(__APPLE__)
#if defined (TARGET_OSX)
/* Apple defines this in crt_externs.h but doesn't provide that header for 
 * arm-apple-darwin9.  We'll manually define the symbol on Apple as it does
 * in fact exist on all implementations (so far) 
 */
gchar ***_NSGetEnviron(void);
#define environ (*_NSGetEnviron())
#else
static char *mono_environ[1] = { NULL };
#define environ mono_environ
#endif /* defined (TARGET_OSX) */
#else
extern char **environ;
#endif

/*
 * Handles > _WAPI_PROCESS_UNHANDLED are pseudo handles which represent processes
 * not started by the runtime.
 */
/* This marks a system process that we don't have a handle on */
/* FIXME: Cope with PIDs > sizeof guint */
#define _WAPI_PROCESS_UNHANDLED (1 << (8*sizeof(pid_t)-1))

#define WAPI_IS_PSEUDO_PROCESS_HANDLE(handle) ((GPOINTER_TO_UINT(handle) & _WAPI_PROCESS_UNHANDLED) == _WAPI_PROCESS_UNHANDLED)
#define WAPI_PID_TO_HANDLE(pid) GINT_TO_POINTER (_WAPI_PROCESS_UNHANDLED + (pid))
#define WAPI_HANDLE_TO_PID(handle) (GPOINTER_TO_UINT ((handle)) - _WAPI_PROCESS_UNHANDLED)

typedef enum {
	STARTF_USESHOWWINDOW=0x001,
	STARTF_USESIZE=0x002,
	STARTF_USEPOSITION=0x004,
	STARTF_USECOUNTCHARS=0x008,
	STARTF_USEFILLATTRIBUTE=0x010,
	STARTF_RUNFULLSCREEN=0x020,
	STARTF_FORCEONFEEDBACK=0x040,
	STARTF_FORCEOFFFEEDBACK=0x080,
	STARTF_USESTDHANDLES=0x100
} StartupFlags;

typedef struct {
	gpointer input;
	gpointer output;
	gpointer error;
} StartupHandles;

typedef struct {
#if G_BYTE_ORDER == G_BIG_ENDIAN
	guint32 highDateTime;
	guint32 lowDateTime;
#else
	guint32 lowDateTime;
	guint32 highDateTime;
#endif
} ProcessTime;

/*
 * MonoProcess describes processes we create.
 * It contains a semaphore that can be waited on in order to wait
 * for process termination. It's accessed in our SIGCHLD handler,
 * when status is updated (and pid cleared, to not clash with
 * subsequent processes that may get executed).
 */
typedef struct _MonoProcess MonoProcess;
struct _MonoProcess {
	pid_t pid; /* the pid of the process. This value is only valid until the process has exited. */
	MonoSemType exit_sem; /* this semaphore will be released when the process exits */
	int status; /* the exit status */
	gint32 handle_count; /* the number of handles to this mono_process instance */
	/* we keep a ref to the creating _WapiHandle_process handle until
	 * the process has exited, so that the information there isn't lost.
	 */
	gpointer handle;
	gboolean freeable;
	MonoProcess *next;
};

/* MonoW32HandleProcess is a structure containing all the required information for process handling. */
typedef struct {
	pid_t id;
	guint32 exitstatus;
	gpointer main_thread;
	WapiFileTime create_time;
	WapiFileTime exit_time;
	char *proc_name;
	size_t min_working_set;
	size_t max_working_set;
	gboolean exited;
	MonoProcess *mono_process;
} MonoW32HandleProcess;

#if HAVE_SIGACTION
static mono_lazy_init_t process_sig_chld_once = MONO_LAZY_INIT_STATUS_NOT_INITIALIZED;
#endif

static gchar *cli_launcher;

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
 * signal handler) */
static MonoProcess *mono_processes;
static mono_mutex_t mono_processes_mutex;
static volatile gint32 mono_processes_cleaning_up;

static gpointer current_process;

static const gunichar2 utf16_space_bytes [2] = { 0x20, 0 };
static const gunichar2 *utf16_space = utf16_space_bytes;
static const gunichar2 utf16_quote_bytes [2] = { 0x22, 0 };
static const gunichar2 *utf16_quote = utf16_quote_bytes;

static void
process_details (gpointer data)
{
	MonoW32HandleProcess *process_handle = (MonoW32HandleProcess *) data;
	g_print ("id: %d, exited: %s, exitstatus: %d",
		process_handle->id, process_handle->exited ? "true" : "false", process_handle->exitstatus);
}

static const gchar*
process_typename (void)
{
	return "Process";
}

static gsize
process_typesize (void)
{
	return sizeof (MonoW32HandleProcess);
}

static guint32
process_wait (gpointer handle, guint32 timeout, gboolean *alerted)
{
	MonoW32HandleProcess *process_handle;
	pid_t pid G_GNUC_UNUSED, ret;
	int status;
	gint64 start, now;
	MonoProcess *mp;
	gboolean res;

	/* FIXME: We can now easily wait on processes that aren't our own children,
	 * but WaitFor*Object won't call us for pseudo handles. */
	g_assert ((GPOINTER_TO_UINT (handle) & _WAPI_PROCESS_UNHANDLED) != _WAPI_PROCESS_UNHANDLED);

	mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s (%p, %u)", __func__, handle, timeout);

	if (alerted)
		*alerted = FALSE;

	res = mono_w32handle_lookup (handle, MONO_W32HANDLE_PROCESS, (gpointer*) &process_handle);
	if (!res) {
		g_warning ("%s: error looking up process handle %p", __func__, handle);
		return WAIT_FAILED;
	}

	if (process_handle->exited) {
		/* We've already done this one */
		mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s (%p, %u): Process already exited", __func__, handle, timeout);
		return WAIT_OBJECT_0;
	}

	pid = process_handle->id;

	mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s (%p, %u): PID: %d", __func__, handle, timeout, pid);

	/* We don't need to lock mono_processes here, the entry
	 * has a handle_count > 0 which means it will not be freed. */
	mp = process_handle->mono_process;
	if (!mp) {
		pid_t res;

		if (pid == mono_process_current_pid ()) {
			mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s (%p, %u): waiting on current process", __func__, handle, timeout);
			return WAIT_TIMEOUT;
		}

		/* This path is used when calling Process.HasExited, so
		 * it is only used to poll the state of the process, not
		 * to actually wait on it to exit */
		g_assert (timeout == 0);

		mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s (%p, %u): waiting on non-child process", __func__, handle, timeout);

		res = waitpid (pid, &status, WNOHANG);
		if (res == 0) {
			mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s (%p, %u): non-child process WAIT_TIMEOUT", __func__, handle, timeout);
			return WAIT_TIMEOUT;
		}
		if (res > 0) {
			mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s (%p, %u): non-child process waited successfully", __func__, handle, timeout);
			return WAIT_OBJECT_0;
		}

		mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s (%p, %u): non-child process WAIT_FAILED, error : %s (%d))", __func__, handle, timeout, g_strerror (errno), errno);
		return WAIT_FAILED;
	}

	start = mono_msec_ticks ();
	now = start;

	while (1) {
		if (timeout != INFINITE) {
			mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s (%p, %u): waiting on semaphore for %li ms...",
				__func__, handle, timeout, (long)(timeout - (now - start)));
			ret = mono_os_sem_timedwait (&mp->exit_sem, (timeout - (now - start)), alerted ? MONO_SEM_FLAGS_ALERTABLE : MONO_SEM_FLAGS_NONE);
		} else {
			mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s (%p, %u): waiting on semaphore forever...",
				__func__, handle, timeout);
			ret = mono_os_sem_wait (&mp->exit_sem, alerted ? MONO_SEM_FLAGS_ALERTABLE : MONO_SEM_FLAGS_NONE);
		}

		if (ret == MONO_SEM_TIMEDWAIT_RET_SUCCESS) {
			/* Success, process has exited */
			mono_os_sem_post (&mp->exit_sem);
			break;
		}

		if (ret == MONO_SEM_TIMEDWAIT_RET_TIMEDOUT) {
			mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s (%p, %u): WAIT_TIMEOUT (timeout = 0)", __func__, handle, timeout);
			return WAIT_TIMEOUT;
		}

		now = mono_msec_ticks ();
		if (now - start >= timeout) {
			mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s (%p, %u): WAIT_TIMEOUT", __func__, handle, timeout);
			return WAIT_TIMEOUT;
		}

		if (alerted && ret == MONO_SEM_TIMEDWAIT_RET_ALERTED) {
			mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s (%p, %u): WAIT_IO_COMPLETION", __func__, handle, timeout);
			*alerted = TRUE;
			return WAIT_IO_COMPLETION;
		}
	}

	/* Process must have exited */
	mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s (%p, %u): Waited successfully", __func__, handle, timeout);

	status = mp ? mp->status : 0;
	if (WIFSIGNALED (status))
		process_handle->exitstatus = 128 + WTERMSIG (status);
	else
		process_handle->exitstatus = WEXITSTATUS (status);
	_wapi_time_t_to_filetime (time (NULL), &process_handle->exit_time);

	process_handle->exited = TRUE;

	mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s (%p, %u): Setting pid %d signalled, exit status %d",
		   __func__, handle, timeout, process_handle->id, process_handle->exitstatus);

	mono_w32handle_set_signal_state (handle, TRUE, TRUE);

	return WAIT_OBJECT_0;
}

static void
processes_cleanup (void)
{
	MonoProcess *mp;
	MonoProcess *prev = NULL;
	GSList *finished = NULL;
	GSList *l;
	gpointer unref_handle;

	mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s", __func__);

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

	mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s done", __func__);

	InterlockedDecrement (&mono_processes_cleaning_up);
}

static void
process_close (gpointer handle, gpointer data)
{
	MonoW32HandleProcess *process_handle;

	mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s", __func__);

	process_handle = (MonoW32HandleProcess *) data;
	g_free (process_handle->proc_name);
	process_handle->proc_name = NULL;
	if (process_handle->mono_process)
		InterlockedDecrement (&process_handle->mono_process->handle_count);
	processes_cleanup ();
}

static MonoW32HandleOps process_ops = {
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

static void
process_set_defaults (MonoW32HandleProcess *process_handle)
{
	/* These seem to be the defaults on w2k */
	process_handle->min_working_set = 204800;
	process_handle->max_working_set = 1413120;

	_wapi_time_t_to_filetime (time (NULL), &process_handle->create_time);
}

static void
process_set_name (MonoW32HandleProcess *process_handle)
{
	char *progname, *utf8_progname, *slash;

	progname = g_get_prgname ();
	utf8_progname = mono_utf8_from_external (progname);

	mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: using [%s] as prog name", __func__, progname);

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
mono_w32process_init (void)
{
	MonoW32HandleProcess process_handle;

	mono_w32handle_register_ops (MONO_W32HANDLE_PROCESS, &process_ops);

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

void
mono_w32process_cleanup (void)
{
	g_free (cli_launcher);
}

/* Check if a pid is valid - i.e. if a process exists with this pid. */
static gboolean
is_pid_valid (pid_t pid)
{
	gboolean result = FALSE;

#if defined(HOST_WATCHOS)
	result = TRUE; // TODO: Rewrite using sysctl
#elif defined(PLATFORM_MACOSX) || defined(__OpenBSD__) || defined(__FreeBSD__)
	if (((kill(pid, 0) == 0) || (errno == EPERM)) && pid != 0)
		result = TRUE;
#elif defined(__HAIKU__)
	team_info teamInfo;
	if (get_team_info ((team_id)pid, &teamInfo) == B_OK)
		result = TRUE;
#else
	char *dir = g_strdup_printf ("/proc/%d", pid);
	if (!access (dir, F_OK))
		result = TRUE;
	g_free (dir);
#endif

	return result;
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
	const gunichar2 *p;
	gunichar2 *ret;

	va_start (args, first);
	total += len16 (first);
	for (s = va_arg (args, gunichar2 *); s != NULL; s = va_arg(args, gunichar2 *))
		total += len16 (s);
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
		for (p = s; *p != 0; p++)
			ret [i++] = *p;
	}
	va_end (args);

	return ret;
}

static guint32
process_get_pid (gpointer handle)
{
	MonoW32HandleProcess *process_handle;
	gboolean res;

	if (WAPI_IS_PSEUDO_PROCESS_HANDLE (handle)) {
		/* This is a pseudo handle */
		return WAPI_HANDLE_TO_PID (handle);
	}

	res = mono_w32handle_lookup (handle, MONO_W32HANDLE_PROCESS, (gpointer*) &process_handle);
	if (!res) {
		SetLastError (ERROR_INVALID_HANDLE);
		return 0;
	}

	return process_handle->id;
}

static gboolean
process_open_compare (gpointer handle, gpointer user_data)
{
	gboolean res;
	MonoW32HandleProcess *process_handle;
	pid_t wanted_pid, checking_pid;

	g_assert (!WAPI_IS_PSEUDO_PROCESS_HANDLE (handle));

	res = mono_w32handle_lookup (handle, MONO_W32HANDLE_PROCESS, (gpointer*) &process_handle);
	if (!res)
		g_error ("%s: unknown process handle %p", __func__, handle);

	mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: looking at process %d", __func__, process_handle->id);

	checking_pid = process_handle->id;
	if (checking_pid == 0)
		return FALSE;

	wanted_pid = GPOINTER_TO_UINT (user_data);

	/* It's possible to have more than one process handle with the
	 * same pid, but only the one running process can be
	 * unsignalled.
	 * If the handle is blown away in the window between
	 * returning TRUE here and mono_w32handle_search pinging
	 * the timestamp, the search will continue. */
	return checking_pid == wanted_pid && !mono_w32handle_issignalled (handle);
}

HANDLE
ves_icall_System_Diagnostics_Process_GetProcess_internal (guint32 pid)
{
	gpointer handle;

	mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: looking for process %d", __func__, pid);

	handle = mono_w32handle_search (MONO_W32HANDLE_PROCESS, process_open_compare, GUINT_TO_POINTER (pid), NULL, TRUE);
	if (handle) {
		/* mono_w32handle_search () already added a ref */
		return handle;
	}

	if (is_pid_valid (pid)) {
		/* Return a pseudo handle for processes we don't have handles for */
		return WAPI_PID_TO_HANDLE (pid);
	} else {
		mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: Can't find pid %d", __func__, pid);

		SetLastError (ERROR_PROC_NOT_FOUND);
		return NULL;
	}
}

static gboolean
match_procname_to_modulename (char *procname, char *modulename)
{
	char* lastsep = NULL;
	char* lastsep2 = NULL;
	char* pname = NULL;
	char* mname = NULL;
	gboolean result = FALSE;

	if (procname == NULL || modulename == NULL)
		return (FALSE);

	mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: procname=\"%s\", modulename=\"%s\"", __func__, procname, modulename);
	pname = mono_path_resolve_symlinks (procname);
	mname = mono_path_resolve_symlinks (modulename);

	if (!strcmp (pname, mname))
		result = TRUE;

	if (!result) {
		lastsep = strrchr (mname, '/');
		if (lastsep)
			if (!strcmp (lastsep+1, pname))
				result = TRUE;
		if (!result) {
			lastsep2 = strrchr (pname, '/');
			if (lastsep2){
				if (lastsep) {
					if (!strcmp (lastsep+1, lastsep2+1))
						result = TRUE;
				} else {
					if (!strcmp (mname, lastsep2+1))
						result = TRUE;
				}
			}
		}
	}

	g_free (pname);
	g_free (mname);

	mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: result is %d", __func__, result);
	return result;
}

static MonoImage *system_assembly;

static void
stash_system_assembly (MonoObject *obj)
{
	if (!system_assembly)
		system_assembly = obj->vtable->klass->image;
}

//Hand coded version that loads from system
static MonoClass*
mono_class_get_file_version_info_class (void)
{
	static MonoClass *tmp_class;
	MonoClass *klass = tmp_class;
	if (!klass) {
		klass = mono_class_load_from_name (system_assembly, "System.Diagnostics", "FileVersionInfo");
		mono_memory_barrier ();
		tmp_class = klass;
	}
	return klass;
}

static MonoClass*
mono_class_get_process_module_class (void)
{
	static MonoClass *tmp_class;
	MonoClass *klass = tmp_class;
	if (!klass) {
		klass = mono_class_load_from_name (system_assembly, "System.Diagnostics", "ProcessModule");
		mono_memory_barrier ();
		tmp_class = klass;
	}
	return klass;
}

static guint32
unicode_chars (const gunichar2 *str)
{
	guint32 len;

	for (len = 0; str [len] != '\0'; ++len)
		;
	return len;
}

static void
process_set_field_object (MonoObject *obj, const gchar *fieldname,
						  MonoObject *data)
{
	MonoClassField *field;

	LOGDEBUG (g_message ("%s: Setting field %s to object at %p", __func__, fieldname, data));

	field = mono_class_get_field_from_name (mono_object_class (obj),
											fieldname);
	mono_gc_wbarrier_generic_store (((char *)obj) + field->offset, data);
}

static void
process_set_field_string (MonoObject *obj, const gchar *fieldname,
						  const gunichar2 *val, guint32 len, MonoError *error)
{
	MonoClassField *field;
	MonoString *string;

	mono_error_init (error);

	LOGDEBUG (g_message ("%s: Setting field %s to [%s]", __func__, fieldname, g_utf16_to_utf8 (val, len, NULL, NULL, NULL)));

	string = mono_string_new_utf16_checked (mono_object_domain (obj), val, len, error);
	
	field = mono_class_get_field_from_name (mono_object_class (obj),
											fieldname);
	mono_gc_wbarrier_generic_store (((char *)obj) + field->offset, (MonoObject*)string);
}

static void
process_set_field_string_char (MonoObject *obj, const gchar *fieldname,
							   const gchar *val)
{
	MonoClassField *field;
	MonoString *string;

	LOGDEBUG (g_message ("%s: Setting field %s to [%s]", __func__, fieldname, val));

	string = mono_string_new (mono_object_domain (obj), val);
	
	field = mono_class_get_field_from_name (mono_object_class (obj), fieldname);
	mono_gc_wbarrier_generic_store (((char *)obj) + field->offset, (MonoObject*)string);
}

static void
process_set_field_int (MonoObject *obj, const gchar *fieldname,
					   guint32 val)
{
	MonoClassField *field;

	LOGDEBUG (g_message ("%s: Setting field %s to %d", __func__,fieldname, val));
	
	field = mono_class_get_field_from_name (mono_object_class (obj),
					      fieldname);
	*(guint32 *)(((char *)obj) + field->offset)=val;
}

static void
process_set_field_intptr (MonoObject *obj, const gchar *fieldname,
						  gpointer val)
{
	MonoClassField *field;

	LOGDEBUG (g_message ("%s: Setting field %s to %p", __func__, fieldname, val));
	
	field = mono_class_get_field_from_name (mono_object_class (obj),
											fieldname);
	*(gpointer *)(((char *)obj) + field->offset) = val;
}

static void
process_set_field_bool (MonoObject *obj, const gchar *fieldname,
						gboolean val)
{
	MonoClassField *field;

	LOGDEBUG (g_message ("%s: Setting field %s to %s", __func__, fieldname, val ? "TRUE":"FALSE"));
	
	field = mono_class_get_field_from_name (mono_object_class (obj),
											fieldname);
	*(guint8 *)(((char *)obj) + field->offset) = val;
}

#define SFI_COMMENTS		"\\StringFileInfo\\%02X%02X%02X%02X\\Comments"
#define SFI_COMPANYNAME		"\\StringFileInfo\\%02X%02X%02X%02X\\CompanyName"
#define SFI_FILEDESCRIPTION	"\\StringFileInfo\\%02X%02X%02X%02X\\FileDescription"
#define SFI_FILEVERSION		"\\StringFileInfo\\%02X%02X%02X%02X\\FileVersion"
#define SFI_INTERNALNAME	"\\StringFileInfo\\%02X%02X%02X%02X\\InternalName"
#define SFI_LEGALCOPYRIGHT	"\\StringFileInfo\\%02X%02X%02X%02X\\LegalCopyright"
#define SFI_LEGALTRADEMARKS	"\\StringFileInfo\\%02X%02X%02X%02X\\LegalTrademarks"
#define SFI_ORIGINALFILENAME	"\\StringFileInfo\\%02X%02X%02X%02X\\OriginalFilename"
#define SFI_PRIVATEBUILD	"\\StringFileInfo\\%02X%02X%02X%02X\\PrivateBuild"
#define SFI_PRODUCTNAME		"\\StringFileInfo\\%02X%02X%02X%02X\\ProductName"
#define SFI_PRODUCTVERSION	"\\StringFileInfo\\%02X%02X%02X%02X\\ProductVersion"
#define SFI_SPECIALBUILD	"\\StringFileInfo\\%02X%02X%02X%02X\\SpecialBuild"
#define EMPTY_STRING		(gunichar2*)"\000\000"

static void
process_module_string_read (MonoObject *filever, gpointer data,
			    const gchar *fieldname, guchar lang_hi, guchar lang_lo,
			    const gchar *key, MonoError *error)
{
	gchar *lang_key_utf8;
	gunichar2 *lang_key, *buffer;
	UINT chars;

	mono_error_init (error);

	lang_key_utf8 = g_strdup_printf (key, lang_lo, lang_hi, 0x04, 0xb0);

	LOGDEBUG (g_message ("%s: asking for [%s]", __func__, lang_key_utf8));

	lang_key = g_utf8_to_utf16 (lang_key_utf8, -1, NULL, NULL, NULL);

	if (VerQueryValue (data, lang_key, (gpointer *)&buffer, &chars) && chars > 0) {
		LOGDEBUG (g_message ("%s: found %d chars of [%s]", __func__, chars, g_utf16_to_utf8 (buffer, chars, NULL, NULL, NULL)));
		/* chars includes trailing null */
		process_set_field_string (filever, fieldname, buffer, chars - 1, error);
	} else {
		process_set_field_string (filever, fieldname, EMPTY_STRING, 0, error);
	}

	g_free (lang_key);
	g_free (lang_key_utf8);
}

typedef struct {
	const char *name;
	const char *id;
} StringTableEntry;

static StringTableEntry stringtable_entries [] = {
	{ "comments", SFI_COMMENTS },
	{ "companyname", SFI_COMPANYNAME },
	{ "filedescription", SFI_FILEDESCRIPTION },
	{ "fileversion", SFI_FILEVERSION },
	{ "internalname", SFI_INTERNALNAME },
	{ "legalcopyright", SFI_LEGALCOPYRIGHT },
	{ "legaltrademarks", SFI_LEGALTRADEMARKS },
	{ "originalfilename", SFI_ORIGINALFILENAME },
	{ "privatebuild", SFI_PRIVATEBUILD },
	{ "productname", SFI_PRODUCTNAME },
	{ "productversion", SFI_PRODUCTVERSION },
	{ "specialbuild", SFI_SPECIALBUILD }
};

static void
process_module_stringtable (MonoObject *filever, gpointer data,
							guchar lang_hi, guchar lang_lo, MonoError *error)
{
	int i;

	for (i = 0; i < G_N_ELEMENTS (stringtable_entries); ++i) {
		process_module_string_read (filever, data, stringtable_entries [i].name, lang_hi, lang_lo,
									stringtable_entries [i].id, error);
		return_if_nok (error);
	}
}

static void
mono_process_get_fileversion (MonoObject *filever, gunichar2 *filename, MonoError *error)
{
	DWORD verinfohandle;
	VS_FIXEDFILEINFO *ffi;
	gpointer data;
	DWORD datalen;
	guchar *trans_data;
	gunichar2 *query;
	UINT ffi_size, trans_size;
	BOOL ok;
	gunichar2 lang_buf[128];
	guint32 lang, lang_count;

	mono_error_init (error);

	datalen = GetFileVersionInfoSize (filename, &verinfohandle);
	if (datalen) {
		data = g_malloc0 (datalen);
		ok = GetFileVersionInfo (filename, verinfohandle, datalen,
					 data);
		if (ok) {
			query = g_utf8_to_utf16 ("\\", -1, NULL, NULL, NULL);
			if (query == NULL) {
				g_free (data);
				return;
			}
			
			if (VerQueryValue (data, query, (gpointer *)&ffi,
			    &ffi_size)) {
				LOGDEBUG (g_message ("%s: recording assembly: FileName [%s] FileVersionInfo [%d.%d.%d.%d]", __func__, g_utf16_to_utf8 (filename, -1, NULL, NULL, NULL), HIWORD (ffi->dwFileVersionMS), LOWORD (ffi->dwFileVersionMS), HIWORD (ffi->dwFileVersionLS), LOWORD (ffi->dwFileVersionLS)));
	
				process_set_field_int (filever, "filemajorpart", HIWORD (ffi->dwFileVersionMS));
				process_set_field_int (filever, "fileminorpart", LOWORD (ffi->dwFileVersionMS));
				process_set_field_int (filever, "filebuildpart", HIWORD (ffi->dwFileVersionLS));
				process_set_field_int (filever, "fileprivatepart", LOWORD (ffi->dwFileVersionLS));

				process_set_field_int (filever, "productmajorpart", HIWORD (ffi->dwProductVersionMS));
				process_set_field_int (filever, "productminorpart", LOWORD (ffi->dwProductVersionMS));
				process_set_field_int (filever, "productbuildpart", HIWORD (ffi->dwProductVersionLS));
				process_set_field_int (filever, "productprivatepart", LOWORD (ffi->dwProductVersionLS));

				process_set_field_bool (filever, "isdebug", ((ffi->dwFileFlags & ffi->dwFileFlagsMask) & VS_FF_DEBUG) != 0);
				process_set_field_bool (filever, "isprerelease", ((ffi->dwFileFlags & ffi->dwFileFlagsMask) & VS_FF_PRERELEASE) != 0);
				process_set_field_bool (filever, "ispatched", ((ffi->dwFileFlags & ffi->dwFileFlagsMask) & VS_FF_PATCHED) != 0);
				process_set_field_bool (filever, "isprivatebuild", ((ffi->dwFileFlags & ffi->dwFileFlagsMask) & VS_FF_PRIVATEBUILD) != 0);
				process_set_field_bool (filever, "isspecialbuild", ((ffi->dwFileFlags & ffi->dwFileFlagsMask) & VS_FF_SPECIALBUILD) != 0);
			}
			g_free (query);

			query = g_utf8_to_utf16 ("\\VarFileInfo\\Translation", -1, NULL, NULL, NULL);
			if (query == NULL) {
				g_free (data);
				return;
			}
			
			if (VerQueryValue (data, query,
					   (gpointer *)&trans_data,
					   &trans_size)) {
				/* use the first language ID we see
				 */
				if (trans_size >= 4) {
		 			LOGDEBUG (g_message("%s: %s has 0x%0x 0x%0x 0x%0x 0x%0x", __func__, g_utf16_to_utf8 (filename, -1, NULL, NULL, NULL), trans_data[0], trans_data[1], trans_data[2], trans_data[3]));
					lang = (trans_data[0]) |
						(trans_data[1] << 8) |
						(trans_data[2] << 16) |
						(trans_data[3] << 24);
					/* Only give the lower 16 bits
					 * to VerLanguageName, as
					 * Windows gets confused
					 * otherwise
					 */
					lang_count = VerLanguageName (lang & 0xFFFF, lang_buf, 128);
					if (lang_count) {
						process_set_field_string (filever, "language", lang_buf, lang_count, error);
						return_if_nok (error);
					}
					process_module_stringtable (filever, data, trans_data[0], trans_data[1], error);
					return_if_nok (error);
				}
			} else {
				int i;

				for (i = 0; i < G_N_ELEMENTS (stringtable_entries); ++i) {
					/* No strings, so set every field to
					 * the empty string
					 */
					process_set_field_string (filever,
											  stringtable_entries [i].name,
											  EMPTY_STRING, 0, error);
					return_if_nok (error);
				}

				/* And language seems to be set to
				 * en_US according to bug 374600
				 */
				lang_count = VerLanguageName (0x0409, lang_buf, 128);
				if (lang_count) {
					process_set_field_string (filever, "language", lang_buf, lang_count, error);
					return_if_nok (error);
				}
			}
			
			g_free (query);
		}
		g_free (data);
	}
}

static void
process_get_assembly_fileversion (MonoObject *filever, MonoAssembly *assembly)
{
	process_set_field_int (filever, "filemajorpart", assembly->aname.major);
	process_set_field_int (filever, "fileminorpart", assembly->aname.minor);
	process_set_field_int (filever, "filebuildpart", assembly->aname.build);
}

static MonoObject*
get_process_module (MonoAssembly *assembly, MonoClass *proc_class, MonoError *error)
{
	MonoObject *item, *filever;
	MonoDomain *domain = mono_domain_get ();
	char *filename;
	const char *modulename = assembly->aname.name;

	mono_error_init (error);

	/* Build a System.Diagnostics.ProcessModule with the data.
	 */
	item = mono_object_new_checked (domain, proc_class, error);
	return_val_if_nok (error, NULL);
	filever = mono_object_new_checked (domain, mono_class_get_file_version_info_class (), error);
	return_val_if_nok (error, NULL);

	filename = g_strdup_printf ("[In Memory] %s", modulename);

	process_get_assembly_fileversion (filever, assembly);
	process_set_field_string_char (filever, "filename", filename);
	process_set_field_object (item, "version_info", filever);

	process_set_field_intptr (item, "baseaddr", assembly->image->raw_data);
	process_set_field_int (item, "memory_size", assembly->image->raw_data_len);
	process_set_field_string_char (item, "filename", filename);
	process_set_field_string_char (item, "modulename", modulename);

	g_free (filename);

	return item;
}

static gboolean
get_module_information (gpointer process, gpointer module, WapiModuleInfo *modinfo, guint32 size);

static MonoObject*
process_add_module (HANDLE process, HMODULE mod, gunichar2 *filename, gunichar2 *modulename, MonoClass *proc_class, MonoError *error)
{
	MonoObject *item, *filever;
	MonoDomain *domain = mono_domain_get ();
	MODULEINFO modinfo;
	BOOL ok;

	mono_error_init (error);

	/* Build a System.Diagnostics.ProcessModule with the data.
	 */
	item = mono_object_new_checked (domain, proc_class, error);
	return_val_if_nok (error, NULL);
	filever = mono_object_new_checked (domain, mono_class_get_file_version_info_class (), error);
	return_val_if_nok (error, NULL);

	mono_process_get_fileversion (filever, filename, error);
	return_val_if_nok (error, NULL);

	process_set_field_string (filever, "filename", filename,
							  unicode_chars (filename), error);
	return_val_if_nok (error, NULL);
	ok = get_module_information (process, mod, &modinfo, sizeof(MODULEINFO));
	if (ok) {
		process_set_field_intptr (item, "baseaddr",
					  modinfo.lpBaseOfDll);
		process_set_field_intptr (item, "entryaddr",
					  modinfo.EntryPoint);
		process_set_field_int (item, "memory_size",
				       modinfo.SizeOfImage);
	}
	process_set_field_string (item, "filename", filename,
							  unicode_chars (filename), error);
	return_val_if_nok (error, NULL);
	process_set_field_string (item, "modulename", modulename,
							  unicode_chars (modulename), error);
	return_val_if_nok (error, NULL);
	process_set_field_object (item, "version_info", filever);

	return item;
}

static GPtrArray*
get_domain_assemblies (MonoDomain *domain)
{
	GSList *tmp;
	GPtrArray *assemblies;

	/* 
	 * Make a copy of the list of assemblies because we can't hold the assemblies
	 * lock while creating objects etc.
	 */
	assemblies = g_ptr_array_new ();
	mono_domain_assemblies_lock (domain);
	for (tmp = domain->domain_assemblies; tmp; tmp = tmp->next) {
		MonoAssembly *ass = (MonoAssembly *)tmp->data;
		if (ass->image->fileio_used)
			continue;
		g_ptr_array_add (assemblies, ass);
	}
	mono_domain_assemblies_unlock (domain);

	return assemblies;
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
			mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: Didn't allocate enough memory for kproc info", __func__);
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
		mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: sysctl() failed: %d", __func__, errno);
		return(ret);
	}

	if ((pi = g_malloc (size)) == NULL)
		return(ret);

	if (sysctl (mib, 4, pi, &size, NULL, 0) < 0) {
		if (errno == ENOMEM) {
			g_free (pi);
			mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: Didn't allocate enough memory for kproc info", __func__);
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
		mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: sysctl() failed: %d", __func__, errno);
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

typedef struct
{
	gpointer address_start;
	gpointer address_end;
	char *perms;
	gpointer address_offset;
	guint64 device;
	guint64 inode;
	char *filename;
} WapiProcModule;

static void free_procmodule (WapiProcModule *mod)
{
	if (mod->perms != NULL) {
		g_free (mod->perms);
	}
	if (mod->filename != NULL) {
		g_free (mod->filename);
	}
	g_free (mod);
}

static gint find_procmodule (gconstpointer a, gconstpointer b)
{
	WapiProcModule *want = (WapiProcModule *)a;
	WapiProcModule *compare = (WapiProcModule *)b;
	return want->device == compare->device && want->inode == compare->inode ? 0 : 1;
}

#if defined(USE_OSX_LOADER)
#include <mach-o/dyld.h>
#include <mach-o/getsect.h>

static GSList*
load_modules (void)
{
	GSList *ret = NULL;
	WapiProcModule *mod;
	uint32_t count = _dyld_image_count ();
	int i = 0;

	for (i = 0; i < count; i++) {
#if SIZEOF_VOID_P == 8
		const struct mach_header_64 *hdr;
		const struct section_64 *sec;
#else
		const struct mach_header *hdr;
		const struct section *sec;
#endif
		const char *name;

		name = _dyld_get_image_name (i);
#if SIZEOF_VOID_P == 8
		hdr = (const struct mach_header_64*)_dyld_get_image_header (i);
		sec = getsectbynamefromheader_64 (hdr, SEG_DATA, SECT_DATA);
#else
		hdr = _dyld_get_image_header (i);
		sec = getsectbynamefromheader (hdr, SEG_DATA, SECT_DATA);
#endif

		/* Some dynlibs do not have data sections on osx (#533893) */
		if (sec == 0) {
			continue;
		}

		mod = g_new0 (WapiProcModule, 1);
		mod->address_start = GINT_TO_POINTER (sec->addr);
		mod->address_end = GINT_TO_POINTER (sec->addr+sec->size);
		mod->perms = g_strdup ("r--p");
		mod->address_offset = 0;
		mod->device = makedev (0, 0);
		mod->inode = i;
		mod->filename = g_strdup (name);

		if (g_slist_find_custom (ret, mod, find_procmodule) == NULL) {
			ret = g_slist_prepend (ret, mod);
		} else {
			free_procmodule (mod);
		}
	}

	ret = g_slist_reverse (ret);

	return(ret);
}
#elif defined(USE_BSD_LOADER)
#include <link.h>

static int
load_modules_callback (struct dl_phdr_info *info, size_t size, void *ptr)
{
	if (size < offsetof (struct dl_phdr_info, dlpi_phnum) + sizeof (info->dlpi_phnum))
		return (-1);

	struct dl_phdr_info *cpy = g_calloc (1, sizeof(struct dl_phdr_info));
	if (!cpy)
		return (-1);

	memcpy(cpy, info, sizeof(*info));

	g_ptr_array_add ((GPtrArray *)ptr, cpy);

	return (0);
}

static GSList*
load_modules (void)
{
	GSList *ret = NULL;
	WapiProcModule *mod;
	GPtrArray *dlarray = g_ptr_array_new();
	int i;

	if (dl_iterate_phdr(load_modules_callback, dlarray) < 0)
		return (ret);

	for (i = 0; i < dlarray->len; i++) {
		struct dl_phdr_info *info = g_ptr_array_index (dlarray, i);

		mod = g_new0 (WapiProcModule, 1);
		mod->address_start = (gpointer)(info->dlpi_addr + info->dlpi_phdr[0].p_vaddr);
		mod->address_end = (gpointer)(info->dlpi_addr + info->dlpi_phdr[info->dlpi_phnum - 1].p_vaddr);
		mod->perms = g_strdup ("r--p");
		mod->address_offset = 0;
		mod->inode = i;
		mod->filename = g_strdup (info->dlpi_name);

		mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: inode=%d, filename=%s, address_start=%p, address_end=%p",
			__func__, mod->inode, mod->filename, mod->address_start, mod->address_end);

		g_free (info);

		if (g_slist_find_custom (ret, mod, find_procmodule) == NULL) {
			ret = g_slist_prepend (ret, mod);
		} else {
			free_procmodule (mod);
		}
	}

	g_ptr_array_free (dlarray, TRUE);

	ret = g_slist_reverse (ret);

	return(ret);
}
#elif defined(USE_HAIKU_LOADER)
static GSList*
load_modules (void)
{
	GSList *ret = NULL;
	WapiProcModule *mod;
	int32 cookie = 0;
	image_info imageInfo;

	while (get_next_image_info (B_CURRENT_TEAM, &cookie, &imageInfo) == B_OK) {
		mod = g_new0 (WapiProcModule, 1);
		mod->device = imageInfo.device;
		mod->inode = imageInfo.node;
		mod->filename = g_strdup (imageInfo.name);
		mod->address_start = MIN (imageInfo.text, imageInfo.data);
		mod->address_end = MAX ((uint8_t*)imageInfo.text + imageInfo.text_size,
			(uint8_t*)imageInfo.data + imageInfo.data_size);
		mod->perms = g_strdup ("r--p");
		mod->address_offset = 0;

		if (g_slist_find_custom (ret, mod, find_procmodule) == NULL) {
			ret = g_slist_prepend (ret, mod);
		} else {
			free_procmodule (mod);
		}
	}

	ret = g_slist_reverse (ret);

	return ret;
}
#else
static GSList*
load_modules (FILE *fp)
{
	GSList *ret = NULL;
	WapiProcModule *mod;
	char buf[MAXPATHLEN + 1], *p, *endp;
	char *start_start, *end_start, *prot_start, *offset_start;
	char *maj_dev_start, *min_dev_start, *inode_start, prot_buf[5];
	gpointer address_start, address_end, address_offset;
	guint32 maj_dev, min_dev;
	guint64 inode;
	guint64 device;

	while (fgets (buf, sizeof(buf), fp)) {
		p = buf;
		while (g_ascii_isspace (*p)) ++p;
		start_start = p;
		if (!g_ascii_isxdigit (*start_start)) {
			continue;
		}
		address_start = (gpointer)strtoul (start_start, &endp, 16);
		p = endp;
		if (*p != '-') {
			continue;
		}

		++p;
		end_start = p;
		if (!g_ascii_isxdigit (*end_start)) {
			continue;
		}
		address_end = (gpointer)strtoul (end_start, &endp, 16);
		p = endp;
		if (!g_ascii_isspace (*p)) {
			continue;
		}

		while (g_ascii_isspace (*p)) ++p;
		prot_start = p;
		if (*prot_start != 'r' && *prot_start != '-') {
			continue;
		}
		memcpy (prot_buf, prot_start, 4);
		prot_buf[4] = '\0';
		while (!g_ascii_isspace (*p)) ++p;

		while (g_ascii_isspace (*p)) ++p;
		offset_start = p;
		if (!g_ascii_isxdigit (*offset_start)) {
			continue;
		}
		address_offset = (gpointer)strtoul (offset_start, &endp, 16);
		p = endp;
		if (!g_ascii_isspace (*p)) {
			continue;
		}

		while(g_ascii_isspace (*p)) ++p;
		maj_dev_start = p;
		if (!g_ascii_isxdigit (*maj_dev_start)) {
			continue;
		}
		maj_dev = strtoul (maj_dev_start, &endp, 16);
		p = endp;
		if (*p != ':') {
			continue;
		}

		++p;
		min_dev_start = p;
		if (!g_ascii_isxdigit (*min_dev_start)) {
			continue;
		}
		min_dev = strtoul (min_dev_start, &endp, 16);
		p = endp;
		if (!g_ascii_isspace (*p)) {
			continue;
		}

		while (g_ascii_isspace (*p)) ++p;
		inode_start = p;
		if (!g_ascii_isxdigit (*inode_start)) {
			continue;
		}
		inode = (guint64)strtol (inode_start, &endp, 10);
		p = endp;
		if (!g_ascii_isspace (*p)) {
			continue;
		}

		device = makedev ((int)maj_dev, (int)min_dev);
		if ((device == 0) &&
		    (inode == 0)) {
			continue;
		}

		while(g_ascii_isspace (*p)) ++p;
		/* p now points to the filename */

		mod = g_new0 (WapiProcModule, 1);
		mod->address_start = address_start;
		mod->address_end = address_end;
		mod->perms = g_strdup (prot_buf);
		mod->address_offset = address_offset;
		mod->device = device;
		mod->inode = inode;
		mod->filename = g_strdup (g_strstrip (p));

		if (g_slist_find_custom (ret, mod, find_procmodule) == NULL) {
			ret = g_slist_prepend (ret, mod);
		} else {
			free_procmodule (mod);
		}
	}

	ret = g_slist_reverse (ret);

	return(ret);
}
#endif

static gboolean
get_process_modules (gpointer process, gpointer *modules, guint32 size, guint32 *needed)
{
	MonoW32HandleProcess *process_handle;
#if !defined(USE_OSX_LOADER) && !defined(USE_BSD_LOADER)
	FILE *fp;
#endif
	GSList *mods = NULL;
	WapiProcModule *module;
	guint32 count, avail = size / sizeof(gpointer);
	int i;
	pid_t pid;
	char *proc_name = NULL;
	gboolean res;

	/* Store modules in an array of pointers (main module as
	 * modules[0]), using the load address for each module as a
	 * token.  (Use 'NULL' as an alternative for the main module
	 * so that the simple implementation can just return one item
	 * for now.)  Get the info from /proc/<pid>/maps on linux,
	 * /proc/<pid>/map on FreeBSD, other systems will have to
	 * implement /dev/kmem reading or whatever other horrid
	 * technique is needed.
	 */
	if (size < sizeof(gpointer))
		return FALSE;

	if (WAPI_IS_PSEUDO_PROCESS_HANDLE (process)) {
		pid = WAPI_HANDLE_TO_PID (process);
		proc_name = get_process_name_from_proc (pid);
	} else {
		res = mono_w32handle_lookup (process, MONO_W32HANDLE_PROCESS, (gpointer*) &process_handle);
		if (!res) {
			mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: Can't find process %p", __func__, process);
			return FALSE;
		}

		pid = process_handle->id;
		proc_name = g_strdup (process_handle->proc_name);
	}

#if defined(USE_OSX_LOADER) || defined(USE_BSD_LOADER) || defined(USE_HAIKU_LOADER)
	mods = load_modules ();
	if (!proc_name) {
		modules[0] = NULL;
		*needed = sizeof(gpointer);
		return TRUE;
	}
#else
	fp = open_process_map (pid, "r");
	if (!fp) {
		/* No /proc/<pid>/maps so just return the main module
		 * shortcut for now
		 */
		modules[0] = NULL;
		*needed = sizeof(gpointer);
		g_free (proc_name);
		return TRUE;
	}
	mods = load_modules (fp);
	fclose (fp);
#endif
	count = g_slist_length (mods);

	/* count + 1 to leave slot 0 for the main module */
	*needed = sizeof(gpointer) * (count + 1);

	/*
	 * Use the NULL shortcut, as the first line in
	 * /proc/<pid>/maps isn't the executable, and we need
	 * that first in the returned list. Check the module name
	 * to see if it ends with the proc name and substitute
	 * the first entry with it.  FIXME if this turns out to
	 * be a problem.
	 */
	modules[0] = NULL;
	for (i = 0; i < (avail - 1) && i < count; i++) {
		module = (WapiProcModule *)g_slist_nth_data (mods, i);
		if (modules[0] != NULL)
			modules[i] = module->address_start;
		else if (match_procname_to_modulename (proc_name, module->filename))
			modules[0] = module->address_start;
		else
			modules[i + 1] = module->address_start;
	}

	for (i = 0; i < count; i++) {
		free_procmodule ((WapiProcModule *)g_slist_nth_data (mods, i));
	}
	g_slist_free (mods);
	g_free (proc_name);

	return TRUE;
}

static guint32
get_module_filename (gpointer process, gpointer module,
					 gunichar2 *basename, guint32 size)
{
	gint pid, len;
	gsize bytes;
	gchar *path;
	gunichar2 *proc_path;

	size *= sizeof (gunichar2); /* adjust for unicode characters */

	if (basename == NULL || size == 0)
		return 0;

	pid = process_get_pid (process);

	path = mono_w32process_get_path (pid);
	if (path == NULL)
		return 0;

	proc_path = mono_unicode_from_external (path, &bytes);
	g_free (path);

	if (proc_path == NULL)
		return 0;

	len = (bytes / 2);

	/* Add the terminator */
	bytes += 2;

	if (size < bytes) {
		mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: Size %d smaller than needed (%ld); truncating", __func__, size, bytes);
		memcpy (basename, proc_path, size);
	} else {
		mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: Size %d larger than needed (%ld)", __func__, size, bytes);
		memcpy (basename, proc_path, bytes);
	}

	g_free (proc_path);

	return len;
}

static guint32
get_module_name (gpointer process, gpointer module, gunichar2 *basename, guint32 size, gboolean base)
{
	MonoW32HandleProcess *process_handle;
	pid_t pid;
	gunichar2 *procname;
	char *procname_ext = NULL;
	glong len;
	gsize bytes;
#if !defined(USE_OSX_LOADER) && !defined(USE_BSD_LOADER)
	FILE *fp;
#endif
	GSList *mods = NULL;
	WapiProcModule *found_module;
	guint32 count;
	int i;
	char *proc_name = NULL;
	gboolean res;

	mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: Getting module base name, process handle %p module %p",
		   __func__, process, module);

	size = size * sizeof (gunichar2); /* adjust for unicode characters */

	if (basename == NULL || size == 0)
		return 0;

	if (WAPI_IS_PSEUDO_PROCESS_HANDLE (process)) {
		/* This is a pseudo handle */
		pid = (pid_t)WAPI_HANDLE_TO_PID (process);
		proc_name = get_process_name_from_proc (pid);
	} else {
		res = mono_w32handle_lookup (process, MONO_W32HANDLE_PROCESS, (gpointer*) &process_handle);
		if (!res) {
			mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: Can't find process %p", __func__, process);
			return 0;
		}

		pid = process_handle->id;
		proc_name = g_strdup (process_handle->proc_name);
	}

	/* Look up the address in /proc/<pid>/maps */
#if defined(USE_OSX_LOADER) || defined(USE_BSD_LOADER) || defined(USE_HAIKU_LOADER)
	mods = load_modules ();
#else
	fp = open_process_map (pid, "r");
	if (fp == NULL) {
		if (errno == EACCES && module == NULL && base == TRUE) {
			procname_ext = get_process_name_from_proc (pid);
		} else {
			/* No /proc/<pid>/maps, so just return failure
			 * for now
			 */
			g_free (proc_name);
			return 0;
		}
	} else {
		mods = load_modules (fp);
		fclose (fp);
	}
#endif
	count = g_slist_length (mods);

	/* If module != NULL compare the address.
	 * If module == NULL we are looking for the main module.
	 * The best we can do for now check it the module name end with the process name.
	 */
	for (i = 0; i < count; i++) {
		found_module = (WapiProcModule *)g_slist_nth_data (mods, i);
		if (procname_ext == NULL &&
			((module == NULL && match_procname_to_modulename (proc_name, found_module->filename)) ||
			 (module != NULL && found_module->address_start == module))) {
			if (base)
				procname_ext = g_path_get_basename (found_module->filename);
			else
				procname_ext = g_strdup (found_module->filename);
		}

		free_procmodule (found_module);
	}

	if (procname_ext == NULL) {
		/* If it's *still* null, we might have hit the
		 * case where reading /proc/$pid/maps gives an
		 * empty file for this user.
		 */
		procname_ext = get_process_name_from_proc (pid);
	}

	g_slist_free (mods);
	g_free (proc_name);

	if (procname_ext) {
		mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: Process name is [%s]", __func__,
			   procname_ext);

		procname = mono_unicode_from_external (procname_ext, &bytes);
		if (procname == NULL) {
			/* bugger */
			g_free (procname_ext);
			return 0;
		}

		len = (bytes / 2);

		/* Add the terminator */
		bytes += 2;

		if (size < bytes) {
			mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: Size %d smaller than needed (%ld); truncating", __func__, size, bytes);

			memcpy (basename, procname, size);
		} else {
			mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: Size %d larger than needed (%ld)",
				   __func__, size, bytes);

			memcpy (basename, procname, bytes);
		}

		g_free (procname);
		g_free (procname_ext);

		return len;
	}

	return 0;
}

static gboolean
get_module_information (gpointer process, gpointer module, WapiModuleInfo *modinfo, guint32 size)
{
	MonoW32HandleProcess *process_handle;
	pid_t pid;
#if !defined(USE_OSX_LOADER) && !defined(USE_BSD_LOADER)
	FILE *fp;
#endif
	GSList *mods = NULL;
	WapiProcModule *found_module;
	guint32 count;
	int i;
	gboolean ret = FALSE;
	char *proc_name = NULL;
	gboolean res;

	mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: Getting module info, process handle %p module %p",
		   __func__, process, module);

	if (modinfo == NULL || size < sizeof (WapiModuleInfo))
		return FALSE;

	if (WAPI_IS_PSEUDO_PROCESS_HANDLE (process)) {
		pid = (pid_t)WAPI_HANDLE_TO_PID (process);
		proc_name = get_process_name_from_proc (pid);
	} else {
		res = mono_w32handle_lookup (process, MONO_W32HANDLE_PROCESS, (gpointer*) &process_handle);
		if (!res) {
			mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: Can't find process %p", __func__, process);
			return FALSE;
		}

		pid = process_handle->id;
		proc_name = g_strdup (process_handle->proc_name);
	}

#if defined(USE_OSX_LOADER) || defined(USE_BSD_LOADER) || defined(USE_HAIKU_LOADER)
	mods = load_modules ();
#else
	/* Look up the address in /proc/<pid>/maps */
	if ((fp = open_process_map (pid, "r")) == NULL) {
		/* No /proc/<pid>/maps, so just return failure
		 * for now
		 */
		g_free (proc_name);
		return FALSE;
	}
	mods = load_modules (fp);
	fclose (fp);
#endif
	count = g_slist_length (mods);

	/* If module != NULL compare the address.
	 * If module == NULL we are looking for the main module.
	 * The best we can do for now check it the module name end with the process name.
	 */
	for (i = 0; i < count; i++) {
			found_module = (WapiProcModule *)g_slist_nth_data (mods, i);
			if (ret == FALSE &&
				((module == NULL && match_procname_to_modulename (proc_name, found_module->filename)) ||
				 (module != NULL && found_module->address_start == module))) {
				modinfo->lpBaseOfDll = found_module->address_start;
				modinfo->SizeOfImage = (gsize)(found_module->address_end) - (gsize)(found_module->address_start);
				modinfo->EntryPoint = found_module->address_offset;
				ret = TRUE;
			}

			free_procmodule (found_module);
	}

	g_slist_free (mods);
	g_free (proc_name);

	return ret;
}

/* Returns an array of System.Diagnostics.ProcessModule */
MonoArray *
ves_icall_System_Diagnostics_Process_GetModules_internal (MonoObject *this_obj, HANDLE process)
{
	MonoError error;
	MonoArray *temp_arr = NULL;
	MonoArray *arr;
	HMODULE mods[1024];
	gunichar2 filename[MAX_PATH];
	gunichar2 modname[MAX_PATH];
	DWORD needed;
	guint32 count = 0, module_count = 0, assembly_count = 0;
	guint32 i, num_added = 0;
	GPtrArray *assemblies = NULL;

	stash_system_assembly (this_obj);

	if (process_get_pid (process) == mono_process_current_pid ()) {
		assemblies = get_domain_assemblies (mono_domain_get ());
		assembly_count = assemblies->len;
	}

	if (get_process_modules (process, mods, sizeof(mods), &needed)) {
		module_count += needed / sizeof(HMODULE);
	}

	count = module_count + assembly_count; 
	temp_arr = mono_array_new_checked (mono_domain_get (), mono_class_get_process_module_class (), count, &error);
	if (mono_error_set_pending_exception (&error))
		return NULL;

	for (i = 0; i < module_count; i++) {
		if (get_module_name (process, mods[i], modname, MAX_PATH, TRUE) &&
				get_module_filename (process, mods[i], filename, MAX_PATH)) {
			MonoObject *module = process_add_module (process, mods[i],
													 filename, modname, mono_class_get_process_module_class (), &error);
			if (!mono_error_ok (&error)) {
				mono_error_set_pending_exception (&error);
				return NULL;
			}
			mono_array_setref (temp_arr, num_added++, module);
		}
	}

	if (assemblies) {
		for (i = 0; i < assembly_count; i++) {
			MonoAssembly *ass = (MonoAssembly *)g_ptr_array_index (assemblies, i);
			MonoObject *module = get_process_module (ass, mono_class_get_process_module_class (), &error);
			if (!mono_error_ok (&error)) {
				mono_error_set_pending_exception (&error);
				return NULL;
			}
			mono_array_setref (temp_arr, num_added++, module);
		}
		g_ptr_array_free (assemblies, TRUE);
	}

	if (count == num_added) {
		arr = temp_arr;
	} else {
		/* shorter version of the array */
		arr = mono_array_new_checked (mono_domain_get (), mono_class_get_process_module_class (), num_added, &error);
		if (mono_error_set_pending_exception (&error))
			return NULL;

		for (i = 0; i < num_added; i++)
			mono_array_setref (arr, i, mono_array_get (temp_arr, MonoObject*, i));
	}

	return arr;
}

void
ves_icall_System_Diagnostics_FileVersionInfo_GetVersionInfo_internal (MonoObject *this_obj, MonoString *filename)
{
	MonoError error;

	stash_system_assembly (this_obj);
	
	mono_process_get_fileversion (this_obj, mono_string_chars (filename), &error);
	if (!mono_error_ok (&error)) {
		mono_error_set_pending_exception (&error);
		return;
	}
	process_set_field_string (this_obj, "filename",
							  mono_string_chars (filename),
							  mono_string_length (filename), &error);
	if (!mono_error_ok (&error)) {
		mono_error_set_pending_exception (&error);
		return;
	}
}

static void
switch_dir_separators (char *path)
{
	size_t i, pathLength = strlen(path);
	
	/* Turn all the slashes round the right way, except for \' */
	/* There are probably other characters that need to be excluded as well. */
	for (i = 0; i < pathLength; i++) {
		if (path[i] == '\\' && i < pathLength - 1 && path[i+1] != '\'' )
			path[i] = '/';
	}
}

#if HAVE_SIGACTION

MONO_SIGNAL_HANDLER_FUNC (static, mono_sigchld_signal_handler, (int _dummy, siginfo_t *info, void *context))
{
	int status;
	int pid;
	MonoProcess *p;

	do {
		do {
			pid = waitpid (-1, &status, WNOHANG);
		} while (pid == -1 && errno == EINTR);

		if (pid <= 0)
			break;

		/*
		 * This can run concurrently with the code in the rest of this module.
		 */
		for (p = mono_processes; p; p = p->next) {
			if (p->pid == pid) {
				break;
			}
		}
		if (p) {
			p->pid = 0; /* this pid doesn't exist anymore, clear it */
			p->status = status;
			mono_os_sem_post (&p->exit_sem);
			mono_memory_barrier ();
			/* Mark this as freeable, the pointer becomes invalid afterwards */
			p->freeable = TRUE;
		}
	} while (1);
}

static void
process_add_sigchld_handler (void)
{
	struct sigaction sa;

	sa.sa_sigaction = mono_sigchld_signal_handler;
	sigemptyset (&sa.sa_mask);
	sa.sa_flags = SA_NOCLDSTOP | SA_SIGINFO;
	g_assert (sigaction (SIGCHLD, &sa, NULL) != -1);
	mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "Added SIGCHLD handler");
}

#endif

static gboolean
is_readable_or_executable (const char *prog)
{
	struct stat buf;
	int a = access (prog, R_OK);
	int b = access (prog, X_OK);
	if (a != 0 && b != 0)
		return FALSE;
	if (stat (prog, &buf))
		return FALSE;
	if (S_ISREG (buf.st_mode))
		return TRUE;
	return FALSE;
}

static gboolean
is_executable (const char *prog)
{
	struct stat buf;
	if (access (prog, X_OK) != 0)
		return FALSE;
	if (stat (prog, &buf))
		return FALSE;
	if (S_ISREG (buf.st_mode))
		return TRUE;
	return FALSE;
}

static gboolean
is_managed_binary (const char *filename)
{
	int original_errno = errno;
#if defined(HAVE_LARGE_FILE_SUPPORT) && defined(O_LARGEFILE)
	int file = open (filename, O_RDONLY | O_LARGEFILE);
#else
	int file = open (filename, O_RDONLY);
#endif
	off_t new_offset;
	unsigned char buffer[8];
	off_t file_size, optional_header_offset;
	off_t pe_header_offset, clr_header_offset;
	gboolean managed = FALSE;
	int num_read;
	guint32 first_word, second_word, magic_number;
	
	/* If we are unable to open the file, then we definitely
	 * can't say that it is managed. The child mono process
	 * probably wouldn't be able to open it anyway.
	 */
	if (file < 0) {
		errno = original_errno;
		return FALSE;
	}

	/* Retrieve the length of the file for future sanity checks. */
	file_size = lseek (file, 0, SEEK_END);
	lseek (file, 0, SEEK_SET);

	/* We know we need to read a header field at offset 60. */
	if (file_size < 64)
		goto leave;

	num_read = read (file, buffer, 2);

	if ((num_read != 2) || (buffer[0] != 'M') || (buffer[1] != 'Z'))
		goto leave;

	new_offset = lseek (file, 60, SEEK_SET);

	if (new_offset != 60)
		goto leave;
	
	num_read = read (file, buffer, 4);

	if (num_read != 4)
		goto leave;
	pe_header_offset =  buffer[0]
		| (buffer[1] <<  8)
		| (buffer[2] << 16)
		| (buffer[3] << 24);
	
	if (pe_header_offset + 24 > file_size)
		goto leave;

	new_offset = lseek (file, pe_header_offset, SEEK_SET);

	if (new_offset != pe_header_offset)
		goto leave;

	num_read = read (file, buffer, 4);

	if ((num_read != 4) || (buffer[0] != 'P') || (buffer[1] != 'E') || (buffer[2] != 0) || (buffer[3] != 0))
		goto leave;

	/*
	 * Verify that the header we want in the optional header data
	 * is present in this binary.
	 */
	new_offset = lseek (file, pe_header_offset + 20, SEEK_SET);

	if (new_offset != pe_header_offset + 20)
		goto leave;

	num_read = read (file, buffer, 2);

	if ((num_read != 2) || ((buffer[0] | (buffer[1] << 8)) < 216))
		goto leave;

	optional_header_offset = pe_header_offset + 24;

	/* Read the PE magic number */
	new_offset = lseek (file, optional_header_offset, SEEK_SET);
	
	if (new_offset != optional_header_offset)
		goto leave;

	num_read = read (file, buffer, 2);

	if (num_read != 2)
		goto leave;

	magic_number = (buffer[0] | (buffer[1] << 8));
	
	if (magic_number == 0x10B)  // PE32
		clr_header_offset = 208;
	else if (magic_number == 0x20B)  // PE32+
		clr_header_offset = 224;
	else
		goto leave;

	/* Read the CLR header address and size fields. These will be
	 * zero if the binary is not managed.
	 */
	new_offset = lseek (file, optional_header_offset + clr_header_offset, SEEK_SET);

	if (new_offset != optional_header_offset + clr_header_offset)
		goto leave;

	num_read = read (file, buffer, 8);
	
	/* We are not concerned with endianness, only with
	 * whether it is zero or not.
	 */
	first_word = *(guint32 *)&buffer[0];
	second_word = *(guint32 *)&buffer[4];
	
	if ((num_read != 8) || (first_word == 0) || (second_word == 0))
		goto leave;
	
	managed = TRUE;

leave:
	close (file);
	errno = original_errno;
	return managed;
}

static gboolean
process_create (const gunichar2 *appname, const gunichar2 *cmdline, gpointer new_environ,
	const gunichar2 *cwd, StartupHandles *startup_handles, MonoW32ProcessInfo *process_info)
{
#if defined (HAVE_FORK) && defined (HAVE_EXECVE)
	char *cmd = NULL, *prog = NULL, *full_prog = NULL, *args = NULL, *args_after_prog = NULL;
	char *dir = NULL, **env_strings = NULL, **argv = NULL;
	guint32 i, env_count = 0;
	gboolean ret = FALSE;
	gpointer handle = NULL;
	MonoW32HandleProcess process_handle = {0}, *process_handle_data;
	GError *gerr = NULL;
	int in_fd, out_fd, err_fd;
	pid_t pid = 0;
	int startup_pipe [2] = {-1, -1};
	int dummy;
	MonoProcess *mono_process;
	gboolean fork_failed = FALSE;
	gboolean res;

#if HAVE_SIGACTION
	mono_lazy_initialize (&process_sig_chld_once, process_add_sigchld_handler);
#endif

	/* appname and cmdline specify the executable and its args:
	 *
	 * If appname is not NULL, it is the name of the executable.
	 * Otherwise the executable is the first token in cmdline.
	 *
	 * Executable searching:
	 *
	 * If appname is not NULL, it can specify the full path and
	 * file name, or else a partial name and the current directory
	 * will be used.  There is no additional searching.
	 *
	 * If appname is NULL, the first whitespace-delimited token in
	 * cmdline is used.  If the name does not contain a full
	 * directory path, the search sequence is:
	 *
	 * 1) The directory containing the current process
	 * 2) The current working directory
	 * 3) The windows system directory  (Ignored)
	 * 4) The windows directory (Ignored)
	 * 5) $PATH
	 *
	 * Just to make things more interesting, tokens can contain
	 * white space if they are surrounded by quotation marks.  I'm
	 * beginning to understand just why windows apps are generally
	 * so crap, with an API like this :-(
	 */
	if (appname != NULL) {
		cmd = mono_unicode_to_external (appname);
		if (cmd == NULL) {
			mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: unicode conversion returned NULL",
				   __func__);

			SetLastError (ERROR_PATH_NOT_FOUND);
			goto free_strings;
		}

		switch_dir_separators(cmd);
	}

	if (cmdline != NULL) {
		args = mono_unicode_to_external (cmdline);
		if (args == NULL) {
			mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: unicode conversion returned NULL", __func__);

			SetLastError (ERROR_PATH_NOT_FOUND);
			goto free_strings;
		}
	}

	if (cwd != NULL) {
		dir = mono_unicode_to_external (cwd);
		if (dir == NULL) {
			mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: unicode conversion returned NULL", __func__);

			SetLastError (ERROR_PATH_NOT_FOUND);
			goto free_strings;
		}

		/* Turn all the slashes round the right way */
		switch_dir_separators(dir);
	}


	/* We can't put off locating the executable any longer :-( */
	if (cmd != NULL) {
		char *unquoted;
		if (g_ascii_isalpha (cmd[0]) && (cmd[1] == ':')) {
			/* Strip off the drive letter.  I can't
			 * believe that CP/M holdover is still
			 * visible...
			 */
			g_memmove (cmd, cmd+2, strlen (cmd)-2);
			cmd[strlen (cmd)-2] = '\0';
		}

		unquoted = g_shell_unquote (cmd, NULL);
		if (unquoted[0] == '/') {
			/* Assume full path given */
			prog = g_strdup (unquoted);

			/* Executable existing ? */
			if (!is_readable_or_executable (prog)) {
				mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: Couldn't find executable %s",
					   __func__, prog);
				g_free (unquoted);
				SetLastError (ERROR_FILE_NOT_FOUND);
				goto free_strings;
			}
		} else {
			/* Search for file named by cmd in the current
			 * directory
			 */
			char *curdir = g_get_current_dir ();

			prog = g_strdup_printf ("%s/%s", curdir, unquoted);
			g_free (curdir);

			/* And make sure it's readable */
			if (!is_readable_or_executable (prog)) {
				mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: Couldn't find executable %s",
					   __func__, prog);
				g_free (unquoted);
				SetLastError (ERROR_FILE_NOT_FOUND);
				goto free_strings;
			}
		}
		g_free (unquoted);

		args_after_prog = args;
	} else {
		char *token = NULL;
		char quote;

		/* Dig out the first token from args, taking quotation
		 * marks into account
		 */

		/* First, strip off all leading whitespace */
		args = g_strchug (args);

		/* args_after_prog points to the contents of args
		 * after token has been set (otherwise argv[0] is
		 * duplicated)
		 */
		args_after_prog = args;

		/* Assume the opening quote will always be the first
		 * character
		 */
		if (args[0] == '\"' || args [0] == '\'') {
			quote = args [0];
			for (i = 1; args[i] != '\0' && args[i] != quote; i++);
			if (args [i + 1] == '\0' || g_ascii_isspace (args[i+1])) {
				/* We found the first token */
				token = g_strndup (args+1, i-1);
				args_after_prog = g_strchug (args + i + 1);
			} else {
				/* Quotation mark appeared in the
				 * middle of the token.  Just give the
				 * whole first token, quotes and all,
				 * to exec.
				 */
			}
		}

		if (token == NULL) {
			/* No quote mark, or malformed */
			for (i = 0; args[i] != '\0'; i++) {
				if (g_ascii_isspace (args[i])) {
					token = g_strndup (args, i);
					args_after_prog = args + i + 1;
					break;
				}
			}
		}

		if (token == NULL && args[0] != '\0') {
			/* Must be just one token in the string */
			token = g_strdup (args);
			args_after_prog = NULL;
		}

		if (token == NULL) {
			/* Give up */
			mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: Couldn't find what to exec", __func__);

			SetLastError (ERROR_PATH_NOT_FOUND);
			goto free_strings;
		}

		/* Turn all the slashes round the right way. Only for
		 * the prg. name
		 */
		switch_dir_separators(token);

		if (g_ascii_isalpha (token[0]) && (token[1] == ':')) {
			/* Strip off the drive letter.  I can't
			 * believe that CP/M holdover is still
			 * visible...
			 */
			g_memmove (token, token+2, strlen (token)-2);
			token[strlen (token)-2] = '\0';
		}

		if (token[0] == '/') {
			/* Assume full path given */
			prog = g_strdup (token);

			/* Executable existing ? */
			if (!is_readable_or_executable (prog)) {
				mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: Couldn't find executable %s",
					   __func__, token);
				g_free (token);
				SetLastError (ERROR_FILE_NOT_FOUND);
				goto free_strings;
			}
		} else {
			char *curdir = g_get_current_dir ();

			/* FIXME: Need to record the directory
			 * containing the current process, and check
			 * that for the new executable as the first
			 * place to look
			 */

			prog = g_strdup_printf ("%s/%s", curdir, token);
			g_free (curdir);

			/* I assume X_OK is the criterion to use,
			 * rather than F_OK
			 *
			 * X_OK is too strict *if* the target is a CLR binary
			 */
			if (!is_readable_or_executable (prog)) {
				g_free (prog);
				prog = g_find_program_in_path (token);
				if (prog == NULL) {
					mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: Couldn't find executable %s", __func__, token);

					g_free (token);
					SetLastError (ERROR_FILE_NOT_FOUND);
					goto free_strings;
				}
			}
		}

		g_free (token);
	}

	mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: Exec prog [%s] args [%s]",
		__func__, prog, args_after_prog);

	/* Check for CLR binaries; if found, we will try to invoke
	 * them using the same mono binary that started us.
	 */
	if (is_managed_binary (prog)) {
		gunichar2 *newapp, *newcmd;
		gsize bytes_ignored;

		newapp = mono_unicode_from_external (cli_launcher ? cli_launcher : "mono", &bytes_ignored);
		if (newapp) {
			if (appname)
				newcmd = utf16_concat (utf16_quote, newapp, utf16_quote, utf16_space, appname, utf16_space, cmdline, NULL);
			else
				newcmd = utf16_concat (utf16_quote, newapp, utf16_quote, utf16_space, cmdline, NULL);

			g_free (newapp);

			if (newcmd) {
				ret = process_create (NULL, newcmd, new_environ, cwd, startup_handles, process_info);

				g_free (newcmd);

				goto free_strings;
			}
		}
	} else {
		if (!is_executable (prog)) {
			mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: Executable permisson not set on %s", __func__, prog);
			SetLastError (ERROR_ACCESS_DENIED);
			goto free_strings;
		}
	}

	if (args_after_prog != NULL && *args_after_prog) {
		char *qprog;

		qprog = g_shell_quote (prog);
		full_prog = g_strconcat (qprog, " ", args_after_prog, NULL);
		g_free (qprog);
	} else {
		full_prog = g_shell_quote (prog);
	}

	ret = g_shell_parse_argv (full_prog, NULL, &argv, &gerr);
	if (ret == FALSE) {
		g_message ("process_create: %s\n", gerr->message);
		g_error_free (gerr);
		gerr = NULL;
		goto free_strings;
	}

	if (startup_handles) {
		in_fd = GPOINTER_TO_UINT (startup_handles->input);
		out_fd = GPOINTER_TO_UINT (startup_handles->output);
		err_fd = GPOINTER_TO_UINT (startup_handles->error);
	} else {
		in_fd = GPOINTER_TO_UINT (GetStdHandle (STD_INPUT_HANDLE));
		out_fd = GPOINTER_TO_UINT (GetStdHandle (STD_OUTPUT_HANDLE));
		err_fd = GPOINTER_TO_UINT (GetStdHandle (STD_ERROR_HANDLE));
	}

	process_handle.proc_name = g_strdup (prog);

	process_set_defaults (&process_handle);

	handle = mono_w32handle_new (MONO_W32HANDLE_PROCESS, &process_handle);
	if (handle == INVALID_HANDLE_VALUE) {
		g_warning ("%s: error creating process handle", __func__);

		ret = FALSE;
		SetLastError (ERROR_OUTOFMEMORY);
		goto free_strings;
	}

	/* new_environ is a block of NULL-terminated strings, which
	 * is itself NULL-terminated. Of course, passing an array of
	 * string pointers would have made things too easy :-(
	 *
	 * If new_environ is not NULL it specifies the entire set of
	 * environment variables in the new process.  Otherwise the
	 * new process inherits the same environment.
	 */
	if (new_environ) {
		gunichar2 *new_environp;

		/* Count the number of strings */
		for (new_environp = (gunichar2 *)new_environ; *new_environp; new_environp++) {
			env_count++;
			while (*new_environp)
				new_environp++;
		}

		/* +2: one for the process handle value, and the last
		 * one is NULL
		 */
		env_strings = g_new0 (char *, env_count + 2);

		/* Copy each environ string into 'strings' turning it
		 * into utf8 (or the requested encoding) at the same
		 * time
		 */
		env_count = 0;
		for (new_environp = (gunichar2 *)new_environ; *new_environp; new_environp++) {
			env_strings[env_count] = mono_unicode_to_external (new_environp);
			env_count++;
			while (*new_environp) {
				new_environp++;
			}
		}
	} else {
		for (i = 0; environ[i] != NULL; i++)
			env_count++;

		/* +2: one for the process handle value, and the last
		 * one is NULL
		 */
		env_strings = g_new0 (char *, env_count + 2);

		/* Copy each environ string into 'strings' turning it
		 * into utf8 (or the requested encoding) at the same
		 * time
		 */
		env_count = 0;
		for (i = 0; environ[i] != NULL; i++) {
			env_strings[env_count] = g_strdup (environ[i]);
			env_count++;
		}
	}

	/* Create a pipe to make sure the child doesn't exit before
	 * we can add the process to the linked list of mono_processes */
	if (pipe (startup_pipe) == -1) {
		/* Could not create the pipe to synchroniz process startup. We'll just not synchronize.
		 * This is just for a very hard to hit race condition in the first place */
		startup_pipe [0] = startup_pipe [1] = -1;
		mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: new process startup not synchronized. We may not notice if the newly created process exits immediately.", __func__);
	}

	switch (pid = fork ()) {
	case -1: /* Error */ {
		SetLastError (ERROR_OUTOFMEMORY);
		ret = FALSE;
		fork_failed = TRUE;
		break;
	}
	case 0: /* Child */ {
		if (startup_pipe [0] != -1) {
			/* Wait until the parent has updated it's internal data */
			ssize_t _i G_GNUC_UNUSED = read (startup_pipe [0], &dummy, 1);
			mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: child: parent has completed its setup", __func__);
			close (startup_pipe [0]);
			close (startup_pipe [1]);
		}

		/* should we detach from the process group? */

		/* Connect stdin, stdout and stderr */
		dup2 (in_fd, 0);
		dup2 (out_fd, 1);
		dup2 (err_fd, 2);

		/* Close all file descriptors */
		for (i = mono_w32handle_fd_reserve - 1; i > 2; i--)
			close (i);

#ifdef DEBUG_ENABLED
		mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: exec()ing [%s] in dir [%s]", __func__, cmd,
			   dir == NULL?".":dir);
		for (i = 0; argv[i] != NULL; i++)
			g_message ("arg %d: [%s]", i, argv[i]);

		for (i = 0; env_strings[i] != NULL; i++)
			g_message ("env %d: [%s]", i, env_strings[i]);
#endif

		/* set cwd */
		if (dir != NULL && chdir (dir) == -1) {
			/* set error */
			_exit (-1);
		}

		/* exec */
		execve (argv[0], argv, env_strings);

		/* set error */
		_exit (-1);

		break;
	}
	default: /* Parent */ {
		res = mono_w32handle_lookup (handle, MONO_W32HANDLE_PROCESS, (gpointer*) &process_handle_data);
		if (!res) {
			g_warning ("%s: error looking up process handle %p", __func__, handle);
			mono_w32handle_unref (handle);
		} else {
			process_handle_data->id = pid;

			/* Add our mono_process into the linked list of mono_processes */
			mono_process = (MonoProcess *) g_malloc0 (sizeof (MonoProcess));
			mono_process->pid = pid;
			mono_process->handle_count = 1;
			mono_os_sem_init (&mono_process->exit_sem, 0);

			/* Keep the process handle artificially alive until the process
			 * exits so that the information in the handle isn't lost. */
			mono_w32handle_ref (handle);
			mono_process->handle = handle;

			process_handle_data->mono_process = mono_process;

			mono_os_mutex_lock (&mono_processes_mutex);
			mono_process->next = mono_processes;
			mono_processes = mono_process;
			mono_os_mutex_unlock (&mono_processes_mutex);

			if (process_info != NULL) {
				process_info->process_handle = handle;
				process_info->pid = pid;

				/* FIXME: we might need to handle the thread info some day */
				process_info->thread_handle = INVALID_HANDLE_VALUE;
				process_info->tid = 0;
			}
		}

		break;
	}
	}

	if (fork_failed)
		mono_w32handle_unref (handle);

	if (startup_pipe [1] != -1) {
		/* Write 1 byte, doesn't matter what */
		ssize_t _i G_GNUC_UNUSED = write (startup_pipe [1], startup_pipe, 1);
		close (startup_pipe [0]);
		close (startup_pipe [1]);
	}

free_strings:
	if (cmd)
		g_free (cmd);
	if (full_prog)
		g_free (full_prog);
	if (prog)
		g_free (prog);
	if (args)
		g_free (args);
	if (dir)
		g_free (dir);
	if (env_strings)
		g_strfreev (env_strings);
	if (argv)
		g_strfreev (argv);

	mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: returning handle %p for pid %d", __func__, handle, pid);

	/* Check if something needs to be cleaned up. */
	processes_cleanup ();

	return ret;
#else
	SetLastError (ERROR_NOT_SUPPORTED);
	return FALSE;
#endif // defined (HAVE_FORK) && defined (HAVE_EXECVE)
}

MonoBoolean
ves_icall_System_Diagnostics_Process_ShellExecuteEx_internal (MonoW32ProcessStartInfo *proc_start_info, MonoW32ProcessInfo *process_info)
{
	const gunichar2 *lpFile;
	const gunichar2 *lpParameters;
	const gunichar2 *lpDirectory;
	gunichar2 *args;
	gboolean ret;

	if (!proc_start_info->filename) {
		/* w2k returns TRUE for this, for some reason. */
		ret = TRUE;
		goto done;
	}

	lpFile = proc_start_info->filename ? mono_string_chars (proc_start_info->filename) : NULL;
	lpParameters = proc_start_info->arguments ? mono_string_chars (proc_start_info->arguments) : NULL;
	lpDirectory = proc_start_info->working_directory && mono_string_length (proc_start_info->working_directory) != 0 ?
		mono_string_chars (proc_start_info->working_directory) : NULL;

	/* Put both executable and parameters into the second argument
	 * to process_create (), so it searches $PATH.  The conversion
	 * into and back out of utf8 is because there is no
	 * g_strdup_printf () equivalent for gunichar2 :-(
	 */
	args = utf16_concat (utf16_quote, lpFile, utf16_quote, lpParameters == NULL ? NULL : utf16_space, lpParameters, NULL);
	if (args == NULL) {
		SetLastError (ERROR_INVALID_DATA);
		ret = FALSE;
		goto done;
	}
	ret = process_create (NULL, args, NULL, lpDirectory, NULL, process_info);
	g_free (args);

	if (!ret && GetLastError () == ERROR_OUTOFMEMORY)
		goto done;

	if (!ret) {
		static char *handler;
		static gunichar2 *handler_utf16;

		if (handler_utf16 == (gunichar2 *)-1) {
			ret = FALSE;
			goto done;
		}

#ifdef PLATFORM_MACOSX
		handler = g_strdup ("/usr/bin/open");
#else
		/*
		 * On Linux, try: xdg-open, the FreeDesktop standard way of doing it,
		 * if that fails, try to use gnome-open, then kfmclient
		 */
		handler = g_find_program_in_path ("xdg-open");
		if (handler == NULL){
			handler = g_find_program_in_path ("gnome-open");
			if (handler == NULL){
				handler = g_find_program_in_path ("kfmclient");
				if (handler == NULL){
					handler_utf16 = (gunichar2 *) -1;
					ret = FALSE;
					goto done;
				} else {
					/* kfmclient needs exec argument */
					char *old = handler;
					handler = g_strconcat (old, " exec",
							       NULL);
					g_free (old);
				}
			}
		}
#endif
		handler_utf16 = g_utf8_to_utf16 (handler, -1, NULL, NULL, NULL);
		g_free (handler);

		/* Put quotes around the filename, in case it's a url
		 * that contains #'s (process_create() calls
		 * g_shell_parse_argv(), which deliberately throws
		 * away anything after an unquoted #).  Fixes bug
		 * 371567.
		 */
		args = utf16_concat (handler_utf16, utf16_space, utf16_quote, lpFile, utf16_quote,
			lpParameters == NULL ? NULL : utf16_space, lpParameters, NULL);
		if (args == NULL) {
			SetLastError (ERROR_INVALID_DATA);
			ret = FALSE;
			goto done;
		}
		ret = process_create (NULL, args, NULL, lpDirectory, NULL, process_info);
		g_free (args);
		if (!ret) {
			if (GetLastError () != ERROR_OUTOFMEMORY)
				SetLastError (ERROR_INVALID_DATA);
			ret = FALSE;
			goto done;
		}
		/* Shell exec should not return a process handle when it spawned a GUI thing, like a browser. */
		CloseHandle (process_info->process_handle);
		process_info->process_handle = NULL;
	}

done:
	if (ret == FALSE) {
		process_info->pid = -GetLastError ();
	} else {
		process_info->thread_handle = NULL;
#if !defined(MONO_CROSS_COMPILE)
		process_info->pid = process_get_pid (process_info->process_handle);
#else
		process_info->pid = 0;
#endif
		process_info->tid = 0;
	}

	return ret;
}

/* Only used when UseShellExecute is false */
static gboolean
mono_process_complete_path (const gunichar2 *appname, gchar **completed)
{
	gchar *utf8app;
	gchar *found;

	utf8app = g_utf16_to_utf8 (appname, -1, NULL, NULL, NULL);

	if (g_path_is_absolute (utf8app)) {
		*completed = g_shell_quote (utf8app);
		g_free (utf8app);
		return TRUE;
	}

	if (g_file_test (utf8app, G_FILE_TEST_IS_EXECUTABLE) && !g_file_test (utf8app, G_FILE_TEST_IS_DIR)) {
		*completed = g_shell_quote (utf8app);
		g_free (utf8app);
		return TRUE;
	}
	
	found = g_find_program_in_path (utf8app);
	if (found == NULL) {
		*completed = NULL;
		g_free (utf8app);
		return FALSE;
	}

	*completed = g_shell_quote (found);
	g_free (found);
	g_free (utf8app);
	return TRUE;
}

static gboolean
mono_process_get_shell_arguments (MonoW32ProcessStartInfo *proc_start_info, gunichar2 **shell_path, MonoString **cmd)
{
	gchar *spath = NULL;

	*shell_path = NULL;
	*cmd = proc_start_info->arguments;

	mono_process_complete_path (mono_string_chars (proc_start_info->filename), &spath);
	if (spath != NULL) {
		*shell_path = g_utf8_to_utf16 (spath, -1, NULL, NULL, NULL);
		g_free (spath);
	}

	return (*shell_path != NULL) ? TRUE : FALSE;
}

MonoBoolean
ves_icall_System_Diagnostics_Process_CreateProcess_internal (MonoW32ProcessStartInfo *proc_start_info,
	HANDLE stdin_handle, HANDLE stdout_handle, HANDLE stderr_handle, MonoW32ProcessInfo *process_info)
{
	gboolean ret;
	gunichar2 *dir;
	StartupHandles startup_handles;
	gunichar2 *shell_path = NULL;
	gchar *env_vars = NULL;
	MonoString *cmd = NULL;

	memset (&startup_handles, 0, sizeof (startup_handles));
	startup_handles.input = stdin_handle;
	startup_handles.output = stdout_handle;
	startup_handles.error = stderr_handle;

	if (mono_process_get_shell_arguments (proc_start_info, &shell_path, &cmd) == FALSE) {
		process_info->pid = -ERROR_FILE_NOT_FOUND;
		return FALSE;
	}

	if (process_info->env_keys) {
		gint i, len; 
		MonoString *ms;
		MonoString *key, *value;
		gunichar2 *str, *ptr;
		gunichar2 *equals16;

		for (len = 0, i = 0; i < mono_array_length (process_info->env_keys); i++) {
			ms = mono_array_get (process_info->env_values, MonoString *, i);
			if (ms == NULL)
				continue;

			len += mono_string_length (ms) * sizeof (gunichar2);
			ms = mono_array_get (process_info->env_keys, MonoString *, i);
			len += mono_string_length (ms) * sizeof (gunichar2);
			len += 2 * sizeof (gunichar2);
		}

		equals16 = g_utf8_to_utf16 ("=", 1, NULL, NULL, NULL);
		ptr = str = g_new0 (gunichar2, len + 1);
		for (i = 0; i < mono_array_length (process_info->env_keys); i++) {
			value = mono_array_get (process_info->env_values, MonoString *, i);
			if (value == NULL)
				continue;

			key = mono_array_get (process_info->env_keys, MonoString *, i);
			memcpy (ptr, mono_string_chars (key), mono_string_length (key) * sizeof (gunichar2));
			ptr += mono_string_length (key);

			memcpy (ptr, equals16, sizeof (gunichar2));
			ptr++;

			memcpy (ptr, mono_string_chars (value), mono_string_length (value) * sizeof (gunichar2));
			ptr += mono_string_length (value);
			ptr++;
		}

		g_free (equals16);
		env_vars = (gchar *) str;
	}
	
	/* The default dir name is "".  Turn that into NULL to mean "current directory" */
	dir = proc_start_info->working_directory && mono_string_length (proc_start_info->working_directory) > 0 ?
			mono_string_chars (proc_start_info->working_directory) : NULL;

	ret = process_create (shell_path, cmd ? mono_string_chars (cmd): NULL, env_vars, dir, &startup_handles, process_info);

	g_free (env_vars);
	if (shell_path != NULL)
		g_free (shell_path);

	if (!ret)
		process_info->pid = -GetLastError ();

	return ret;
}

MonoString *
ves_icall_System_Diagnostics_Process_ProcessName_internal (HANDLE process)
{
	MonoError error;
	MonoString *string;
	gunichar2 name[MAX_PATH];
	guint32 len;
	gboolean ok;
	HMODULE mod;
	DWORD needed;

	ok = get_process_modules (process, &mod, sizeof(mod), &needed);
	if (!ok)
		return NULL;

	len = get_module_name (process, mod, name, MAX_PATH, TRUE);

	if (len == 0)
		return NULL;
	
	LOGDEBUG (g_message ("%s: process name is [%s]", __func__, g_utf16_to_utf8 (name, -1, NULL, NULL, NULL)));
	
	string = mono_string_new_utf16_checked (mono_domain_get (), name, len, &error);
	if (!mono_error_ok (&error))
		mono_error_set_pending_exception (&error);
	
	return string;
}

/* Returns an array of pids */
MonoArray *
ves_icall_System_Diagnostics_Process_GetProcesses_internal (void)
{
	MonoError error;
	MonoArray *procs;
	gpointer *pidarray;
	int i, count;

	pidarray = mono_process_list (&count);
	if (!pidarray) {
		mono_set_pending_exception (mono_get_exception_not_supported ("This system does not support EnumProcesses"));
		return NULL;
	}
	procs = mono_array_new_checked (mono_domain_get (), mono_get_int32_class (), count, &error);
	if (mono_error_set_pending_exception (&error)) {
		g_free (pidarray);
		return NULL;
	}
	if (sizeof (guint32) == sizeof (gpointer)) {
		memcpy (mono_array_addr (procs, guint32, 0), pidarray, count * sizeof (gint32));
	} else {
		for (i = 0; i < count; ++i)
			*(mono_array_addr (procs, guint32, i)) = GPOINTER_TO_UINT (pidarray [i]);
	}
	g_free (pidarray);

	return procs;
}

gint64
ves_icall_System_Diagnostics_Process_GetProcessData (int pid, gint32 data_type, gint32 *error)
{
	MonoProcessError perror;
	guint64 res;

	res = mono_process_get_data_with_error (GINT_TO_POINTER (pid), (MonoProcessData)data_type, &perror);
	if (error)
		*error = perror;
	return res;
}

void
mono_w32process_set_cli_launcher (gchar *path)
{
	g_free (cli_launcher);
	cli_launcher = g_strdup (path);
}

gchar*
mono_w32process_get_path (pid_t pid)
{
#if defined(PLATFORM_MACOSX) && !defined(__mono_ppc__) && defined(TARGET_OSX)
	gchar buf [PROC_PIDPATHINFO_MAXSIZE];
	gint res;

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

gpointer
ves_icall_Microsoft_Win32_NativeMethods_GetCurrentProcess (void)
{
	mono_w32handle_ref (current_process);
	return current_process;
}

MonoBoolean
ves_icall_Microsoft_Win32_NativeMethods_GetExitCodeProcess (gpointer handle, gint32 *exitcode)
{
	MonoW32HandleProcess *process_handle;
	guint32 pid;
	gboolean res;

	if (!exitcode)
		return FALSE;

	if (WAPI_IS_PSEUDO_PROCESS_HANDLE (handle)) {
		pid = WAPI_HANDLE_TO_PID (handle);
		/* This is a pseudo handle, so we don't know what the exit
		 * code was, but we can check whether it's alive or not */
		if (is_pid_valid (pid)) {
			*exitcode = STILL_ACTIVE;
			return TRUE;
		} else {
			*exitcode = -1;
			return TRUE;
		}
	}

	res = mono_w32handle_lookup (handle, MONO_W32HANDLE_PROCESS, (gpointer*) &process_handle);
	if (!res) {
		mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: Can't find process %p", __func__, handle);
		return FALSE;
	}

	if (process_handle->id == wapi_getpid ()) {
		*exitcode = STILL_ACTIVE;
		return TRUE;
	}

	/* A process handle is only signalled if the process has exited
	 * and has been waited for. Make sure any process exit has been
	 * noticed before checking if the process is signalled.
	 * Fixes bug 325463. */
	mono_w32handle_wait_one (handle, 0, TRUE);

	*exitcode = mono_w32handle_issignalled (handle) ? process_handle->exitstatus : STILL_ACTIVE;
	return TRUE;
}

MonoBoolean
ves_icall_Microsoft_Win32_NativeMethods_CloseProcess (gpointer handle)
{
	if (WAPI_IS_PSEUDO_PROCESS_HANDLE (handle))
		return TRUE;
	return CloseHandle (handle);
}

MonoBoolean
ves_icall_Microsoft_Win32_NativeMethods_TerminateProcess (gpointer handle, gint32 exitcode)
{
#ifdef HAVE_KILL
	MonoW32HandleProcess *process_handle;
	int ret;
	pid_t pid;

	if (WAPI_IS_PSEUDO_PROCESS_HANDLE (handle)) {
		/* This is a pseudo handle */
		pid = (pid_t)WAPI_HANDLE_TO_PID (handle);
	} else {
		gboolean res;

		res = mono_w32handle_lookup (handle, MONO_W32HANDLE_PROCESS, (gpointer*) &process_handle);
		if (!res) {
			mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: Can't find process %p", __func__, handle);
			SetLastError (ERROR_INVALID_HANDLE);
			return FALSE;
		}

		pid = process_handle->id;
	}

	ret = kill (pid, exitcode == -1 ? SIGKILL : SIGTERM);
	if (ret == 0)
		return TRUE;

	switch (errno) {
	case EINVAL: SetLastError (ERROR_INVALID_PARAMETER); break;
	case EPERM:  SetLastError (ERROR_ACCESS_DENIED);     break;
	case ESRCH:  SetLastError (ERROR_PROC_NOT_FOUND);    break;
	default:     SetLastError (ERROR_GEN_FAILURE);       break;
	}

	return FALSE;
#else
	g_error ("kill() is not supported by this platform");
#endif
}

MonoBoolean
ves_icall_Microsoft_Win32_NativeMethods_GetProcessWorkingSetSize (gpointer handle, gsize *min, gsize *max)
{
	MonoW32HandleProcess *process_handle;
	gboolean res;

	if (!min || !max)
		return FALSE;

	if (WAPI_IS_PSEUDO_PROCESS_HANDLE (handle))
		return FALSE;

	res = mono_w32handle_lookup (handle, MONO_W32HANDLE_PROCESS, (gpointer*) &process_handle);
	if (!res) {
		mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: Can't find process %p", __func__, handle);
		return FALSE;
	}

	*min = process_handle->min_working_set;
	*max = process_handle->max_working_set;
	return TRUE;
}

MonoBoolean
ves_icall_Microsoft_Win32_NativeMethods_SetProcessWorkingSetSize (gpointer handle, gsize min, gsize max)
{
	MonoW32HandleProcess *process_handle;
	gboolean res;

	if (WAPI_IS_PSEUDO_PROCESS_HANDLE (handle))
		return FALSE;

	res = mono_w32handle_lookup (handle, MONO_W32HANDLE_PROCESS, (gpointer*) &process_handle);
	if (!res) {
		mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: Can't find process %p", __func__, handle);
		return FALSE;
	}

	process_handle->min_working_set = min;
	process_handle->max_working_set = max;
	return TRUE;
}

gint32
ves_icall_Microsoft_Win32_NativeMethods_GetPriorityClass (gpointer handle)
{
#ifdef HAVE_GETPRIORITY
	MonoW32HandleProcess *process_handle;
	gint ret;
	pid_t pid;

	if (WAPI_IS_PSEUDO_PROCESS_HANDLE (handle)) {
		/* This is a pseudo handle */
		pid = (pid_t)WAPI_HANDLE_TO_PID (handle);
	} else {
		gboolean res;

		res = mono_w32handle_lookup (handle, MONO_W32HANDLE_PROCESS, (gpointer*) &process_handle);
		if (!res) {
			SetLastError (ERROR_INVALID_HANDLE);
			return 0;
		}

		pid = process_handle->id;
	}

	errno = 0;
	ret = getpriority (PRIO_PROCESS, pid);
	if (ret == -1 && errno != 0) {
		switch (errno) {
		case EPERM:
		case EACCES:
			SetLastError (ERROR_ACCESS_DENIED);
			break;
		case ESRCH:
			SetLastError (ERROR_PROC_NOT_FOUND);
			break;
		default:
			SetLastError (ERROR_GEN_FAILURE);
		}
		return 0;
	}

	if (ret == 0)
		return MONO_W32PROCESS_PRIORITY_CLASS_NORMAL;
	else if (ret < -15)
		return MONO_W32PROCESS_PRIORITY_CLASS_REALTIME;
	else if (ret < -10)
		return MONO_W32PROCESS_PRIORITY_CLASS_HIGH;
	else if (ret < 0)
		return MONO_W32PROCESS_PRIORITY_CLASS_ABOVE_NORMAL;
	else if (ret > 10)
		return MONO_W32PROCESS_PRIORITY_CLASS_IDLE;
	else if (ret > 0)
		return MONO_W32PROCESS_PRIORITY_CLASS_BELOW_NORMAL;

	return MONO_W32PROCESS_PRIORITY_CLASS_NORMAL;
#else
	SetLastError (ERROR_NOT_SUPPORTED);
	return 0;
#endif
}

MonoBoolean
ves_icall_Microsoft_Win32_NativeMethods_SetPriorityClass (gpointer handle, gint32 priorityClass)
{
#ifdef HAVE_SETPRIORITY
	MonoW32HandleProcess *process_handle;
	int ret;
	int prio;
	pid_t pid;

	if (WAPI_IS_PSEUDO_PROCESS_HANDLE (handle)) {
		/* This is a pseudo handle */
		pid = (pid_t)WAPI_HANDLE_TO_PID (handle);
	} else {
		gboolean res;

		res = mono_w32handle_lookup (handle, MONO_W32HANDLE_PROCESS, (gpointer*) &process_handle);
		if (!res) {
			SetLastError (ERROR_INVALID_HANDLE);
			return FALSE;
		}

		pid = process_handle->id;
	}

	switch (priorityClass) {
	case MONO_W32PROCESS_PRIORITY_CLASS_IDLE:
		prio = 19;
		break;
	case MONO_W32PROCESS_PRIORITY_CLASS_BELOW_NORMAL:
		prio = 10;
		break;
	case MONO_W32PROCESS_PRIORITY_CLASS_NORMAL:
		prio = 0;
		break;
	case MONO_W32PROCESS_PRIORITY_CLASS_ABOVE_NORMAL:
		prio = -5;
		break;
	case MONO_W32PROCESS_PRIORITY_CLASS_HIGH:
		prio = -11;
		break;
	case MONO_W32PROCESS_PRIORITY_CLASS_REALTIME:
		prio = -20;
		break;
	default:
		SetLastError (ERROR_INVALID_PARAMETER);
		return FALSE;
	}

	ret = setpriority (PRIO_PROCESS, pid, prio);
	if (ret == -1) {
		switch (errno) {
		case EPERM:
		case EACCES:
			SetLastError (ERROR_ACCESS_DENIED);
			break;
		case ESRCH:
			SetLastError (ERROR_PROC_NOT_FOUND);
			break;
		default:
			SetLastError (ERROR_GEN_FAILURE);
		}
	}

	return ret == 0;
#else
	SetLastError (ERROR_NOT_SUPPORTED);
	return FALSE;
#endif
}

static void
ticks_to_processtime (guint64 ticks, ProcessTime *processtime)
{
	processtime->lowDateTime = ticks & 0xFFFFFFFF;
	processtime->highDateTime = ticks >> 32;
}

static void
wapifiletime_to_processtime (WapiFileTime wapi_filetime, ProcessTime *processtime)
{
	processtime->lowDateTime = wapi_filetime.dwLowDateTime;
	processtime->highDateTime = wapi_filetime.dwHighDateTime;
}

MonoBoolean
ves_icall_Microsoft_Win32_NativeMethods_GetProcessTimes (gpointer handle, gint64 *creation_time, gint64 *exit_time, gint64 *kernel_time, gint64 *user_time)
{
	MonoW32HandleProcess *process_handle;
	ProcessTime *creation_processtime, *exit_processtime, *kernel_processtime, *user_processtime;
	gboolean res;

	if (!creation_time || !exit_time || !kernel_time || !user_time) {
		/* Not sure if w32 allows NULLs here or not */
		return FALSE;
	}

	creation_processtime = (ProcessTime*) creation_time;
	exit_processtime = (ProcessTime*) exit_time;
	kernel_processtime = (ProcessTime*) kernel_time;
	user_processtime = (ProcessTime*) user_time;

	memset (creation_processtime, 0, sizeof (ProcessTime));
	memset (exit_processtime, 0, sizeof (ProcessTime));
	memset (kernel_processtime, 0, sizeof (ProcessTime));
	memset (user_processtime, 0, sizeof (ProcessTime));

	if (WAPI_IS_PSEUDO_PROCESS_HANDLE (handle)) {
		gint64 start_ticks, user_ticks, kernel_ticks;

		mono_process_get_times (GINT_TO_POINTER (WAPI_HANDLE_TO_PID (handle)),
			&start_ticks, &user_ticks, &kernel_ticks);

		ticks_to_processtime (start_ticks, creation_processtime);
		ticks_to_processtime (user_ticks, kernel_processtime);
		ticks_to_processtime (kernel_ticks, user_processtime);
		return TRUE;
	}

	res = mono_w32handle_lookup (handle, MONO_W32HANDLE_PROCESS, (gpointer*) &process_handle);
	if (!res) {
		mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: Can't find process %p", __func__, handle);
		return FALSE;
	}

	wapifiletime_to_processtime (process_handle->create_time, creation_processtime);

	/* A process handle is only signalled if the process has
	 * exited, otherwise exit_processtime isn't set */
	if (mono_w32handle_issignalled (handle))
		wapifiletime_to_processtime (process_handle->exit_time, exit_processtime);

#ifdef HAVE_GETRUSAGE
	if (process_handle->id == getpid ()) {
		struct rusage time_data;
		if (getrusage (RUSAGE_SELF, &time_data) == 0) {
			ticks_to_processtime ((guint64)time_data.ru_utime.tv_sec * 10000000 + (guint64)time_data.ru_utime.tv_usec * 10, user_processtime);
			ticks_to_processtime ((guint64)time_data.ru_stime.tv_sec * 10000000 + (guint64)time_data.ru_stime.tv_usec * 10, kernel_processtime);
		}
	}
#endif

	return TRUE;
}
