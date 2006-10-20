/*
 * <sys/wait.h> wrapper functions.
 *
 * Authors:
 *   Jonathan Pryor (jonpryor@vt.edu)
 *
 * Copyright (C) 2004 Jonathan Pryor
 */

#include <sys/types.h>
#include <sys/wait.h>

#include <glib.h>

#include "mph.h"
#include "map.h"

G_BEGIN_DECLS

gint32
Mono_Posix_Syscall_WIFEXITED (gint32 status)
{
	return WIFEXITED (status);
}

gint32
Mono_Posix_Syscall_WEXITSTATUS (gint32 status)
{
	return WEXITSTATUS (status);
}

gint32
Mono_Posix_Syscall_WIFSIGNALED (gint32 status)
{
	return WIFSIGNALED (status);
}

gint32
Mono_Posix_Syscall_WTERMSIG (gint32 status)
{
	return WTERMSIG (status);
}

gint32
Mono_Posix_Syscall_WIFSTOPPED (gint32 status)
{
	return WIFSTOPPED (status);
}

gint32
Mono_Posix_Syscall_WSTOPSIG (gint32 status)
{
	return WSTOPSIG (status);
}

G_END_DECLS

/*
 * vim: noexpandtab
 */
