/*
 * <stdio.h> wrapper functions.
 *
 * Authors:
 *   Jonathan Pryor (jonpryor@vt.edu)
 *
 * Copyright (C) 2004 Jonathan Pryor
 */

#include <stdio.h>

#include "mph.h"

G_BEGIN_DECLS

gint32
Mono_Posix_Syscall_L_ctermid (void)
{
	return L_ctermid;
}

gint32
Mono_Posix_Syscall_L_cuserid (void)
{
	return L_cuserid;
}

G_END_DECLS

/*
 * vim: noexpandtab
 */
