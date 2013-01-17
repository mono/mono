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

#include "map.h"
#include "mph.h"
#include <glib.h>

G_BEGIN_DECLS

int
Mono_Posix_Syscall_openlog (void* ident, int option, int facility)
{
	openlog ((const char*) ident, option, facility);
	return 0;
}

int
Mono_Posix_Syscall_closelog (void)
{
	closelog ();
	return 0;
}

#ifdef __GNUC__
	#pragma GCC diagnostic push
	#pragma GCC diagnostic ignored "-Wformat-security"
#endif
int
Mono_Posix_Syscall_syslog (int priority, const char* message)
{
	syslog (priority, message);
	return 0;
}

#ifdef __GNUC__
	#pragma GCC diagnostic pop
#endif

/* vararg version of syslog(3). */
gint32
Mono_Posix_Syscall_syslog2 (int priority, const char *format, ...)
{
	va_list ap;

	va_start (ap, format);
	vsyslog (priority, format, ap);
	va_end (ap);

	return 0;
}


G_END_DECLS

/*
 * vim: noexpandtab
 */
