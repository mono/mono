/*
 * <syslog.h> wrapper functions.
 *
 * Authors:
 *   Jonathan Pryor (jonpryor@vt.edu)
 *
 * Copyright (C) 2005 Jonathan Pryor
 */

#include <stdarg.h>
#ifndef __QNXNTO__
#include <syslog.h>
#endif
#include <errno.h>

#include "map.h"
#include "mph.h"
#include <glib.h>

G_BEGIN_DECLS

int
Mono_Posix_Syscall_openlog (void* ident, int option, int facility)
{
#ifndef __QNXNTO__
	openlog ((const char*) ident, option, facility);
#endif
	return 0;
}

int
Mono_Posix_Syscall_closelog (void)
{
#ifndef __QNXNTO__
	closelog ();
#endif
	return 0;
}

int
Mono_Posix_Syscall_syslog (int priority, const char* message)
{
#ifndef __QNXNTO__
	syslog (priority, message);
#endif
	return 0;
}

/* vararg version of syslog(3). */
gint32
Mono_Posix_Syscall_syslog2 (int priority, const char *format, ...)
{
#ifndef __QNXNTO__
	va_list ap;

	va_start (ap, format);
	vsyslog (priority, format, ap);
	va_end (ap);
#endif

	return 0;
}


G_END_DECLS

/*
 * vim: noexpandtab
 */
