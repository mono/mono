/**
 * \file
 */

#ifndef _MONO_CLI_STRING_ICALLS_H_
#define _MONO_CLI_STRING_ICALLS_H_

/*
 * string-icalls.h: String internal calls for the corlib
 *
 * Author:
 *   Patrik Torstensson (patrik.torstensson@labs2.com)
 *
 * (C) 2001 Ximian, Inc.
 */

#include <glib.h>
#include <mono/metadata/class.h>
#include <mono/metadata/object.h>
#include "mono/utils/mono-compiler.h"

ICALL_EXPORT
void
ves_icall_System_String_ctor_RedirectToCreateString (void);

ICALL_EXPORT
MonoString *
ves_icall_System_String_InternalAllocateStr (gint32 length);

ICALL_EXPORT
MonoString  *
ves_icall_System_String_InternalIntern (MonoString *str);

ICALL_EXPORT
MonoString * 
ves_icall_System_String_InternalIsInterned (MonoString *str);

ICALL_EXPORT
int
ves_icall_System_String_GetLOSLimit (void);

#endif /* _MONO_CLI_STRING_ICALLS_H_ */
