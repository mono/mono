/*
 * filewatcher.c: File System Watcher internal calls
 *
 * Authors:
 *	Gonzalo Paniagua Javier (gonzalo@ximian.com)
 *
 * Copyright 2004-2009 Novell, Inc (http://www.novell.com)
 */

#ifdef HAVE_CONFIG_H
#include <config.h>
#endif

#include <mono/metadata/appdomain.h>
#include <mono/metadata/exception.h>
#include <mono/metadata/filewatcher.h>
#include <mono/metadata/marshal.h>
#include <mono/utils/mono-dl.h>
#include <mono/utils/mono-io-portability.h>
#ifdef HOST_WIN32

/*
 * TODO:
 * We use the managed watcher on windows, so the code inside this #if is never used
 */
gint
ves_icall_System_IO_FSW_SupportsFSW (void)
{
	return 1;
}

gboolean
ves_icall_System_IO_FAMW_InternalFAMNextEvent (gpointer conn,
					       MonoString **filename,
					       gint *code,
					       gint *reqnum)
{
	return FALSE;
}

#else

static int (*FAMNextEvent) (gpointer, gpointer);

gint
ves_icall_System_IO_FSW_SupportsFSW (void)
{
#if HAVE_KQUEUE
	return 3;
#else
	MonoDl *fam_module;
	int lib_used = 4; /* gamin */
	int inotify_instance;
	char *err;

	inotify_instance = ves_icall_System_IO_InotifyWatcher_GetInotifyInstance ();
	if (inotify_instance != -1) {
		close (inotify_instance);
		return 5; /* inotify */
	}

	fam_module = mono_dl_open ("libgamin-1.so", MONO_DL_LAZY, NULL);
	if (fam_module == NULL) {
		lib_used = 2; /* FAM */
		fam_module = mono_dl_open ("libfam.so", MONO_DL_LAZY, NULL);
	}

	if (fam_module == NULL)
		return 0;

	err = mono_dl_symbol (fam_module, "FAMNextEvent", (gpointer *) &FAMNextEvent);
	g_free (err);
	if (FAMNextEvent == NULL)
		return 0;

	return lib_used;
#endif
}

/* Almost copied from fam.h. Weird, I know */
typedef struct {
	gint reqnum;
} FAMRequest;

typedef struct FAMEvent {
	gpointer fc;
	FAMRequest fr;
	gchar *hostname;
	gchar filename [PATH_MAX];
	gpointer userdata;
	gint code;
} FAMEvent;

gboolean
ves_icall_System_IO_FAMW_InternalFAMNextEvent (gpointer conn,
					       MonoString **filename,
					       gint *code,
					       gint *reqnum)
{
	FAMEvent ev;

	if (FAMNextEvent (conn, &ev) == 1) {
		*filename = mono_string_new (mono_domain_get (), ev.filename);
		*code = ev.code;
		*reqnum = ev.fr.reqnum;
		return TRUE;
	}

	return FALSE;
}
#endif

#ifndef HAVE_SYS_INOTIFY_H
int ves_icall_System_IO_InotifyWatcher_GetInotifyInstance ()
{
	return -1;
}

int ves_icall_System_IO_InotifyWatcher_AddWatch (int fd, MonoString *directory, gint32 mask)
{
	return -1;
}

int ves_icall_System_IO_InotifyWatcher_RemoveWatch (int fd, gint32 watch_descriptor)
{
	return -1;
}
#else
#include <sys/inotify.h>
#include <errno.h>

int
ves_icall_System_IO_InotifyWatcher_GetInotifyInstance ()
{
	return inotify_init ();
}

int
ves_icall_System_IO_InotifyWatcher_AddWatch (int fd, MonoString *name, gint32 mask)
{
	char *str, *path;
	int retval;

	if (name == NULL)
		return -1;

	str = mono_string_to_utf8 (name);
	path = mono_portability_find_file (str, TRUE);
	if (!path)
		path = str;

	retval = inotify_add_watch (fd, path, mask);
	if (retval < 0) {
		switch (errno) {
		case EACCES:
			errno = ERROR_ACCESS_DENIED;
			break;
		case EBADF:
			errno = ERROR_INVALID_HANDLE;
			break;
		case EFAULT:
			errno = ERROR_INVALID_ACCESS;
			break;
		case EINVAL:
			errno = ERROR_INVALID_DATA;
			break;
		case ENOMEM:
			errno = ERROR_NOT_ENOUGH_MEMORY;
			break;
		case ENOSPC:
			errno = ERROR_TOO_MANY_OPEN_FILES;
			break;
		default:
			errno = ERROR_GEN_FAILURE;
			break;
		}
		mono_marshal_set_last_error ();
	}
	if (path != str)
		g_free (path);
	g_free (str);
	return retval;
}

int
ves_icall_System_IO_InotifyWatcher_RemoveWatch (int fd, gint32 watch_descriptor)
{
	return inotify_rm_watch (fd, watch_descriptor);
}
#endif

