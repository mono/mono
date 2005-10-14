/*
 * <syslog.h> wrapper functions.
 *
 * Authors:
 *   Jonathan Pryor (jonpryor@vt.edu)
 *
 * Copyright (C) 2005 Jonathan Pryor
 */

#include <stdarg.h>
#include <syslog.h>
#include <errno.h>

#include "mph.h"
#include <glib/gtypes.h>

G_BEGIN_DECLS

int
Mono_Posix_Syscall_openlog (void* ident, int option, int facility)
{
	errno = 0;
	openlog ((const char*) ident, option, facility);
	return errno == 0 ? 0 : -1;
}

int
Mono_Posix_Syscall_closelog (void)
{
	errno = 0;
	closelog ();
	return errno == 0 ? 0 : -1;
}

int
Mono_Posix_Syscall_syslog (int priority, const char* message)
{
	errno = 0;
	syslog (priority, message);
	return errno == 0 ? 0 : -1;
}

/* vararg version of syslog(3). */
gint32
Mono_Posix_Syscall_syslog2 (int priority, const char *format, ...)
{
	va_list ap;

	errno = 0;

	va_start (ap, format);
	vsyslog (priority, format, ap);
	va_end (ap);

	if (errno != 0)
		return -1;
	return 0;
}


G_END_DECLS

/*
 * vim: noexpandtab
 */
