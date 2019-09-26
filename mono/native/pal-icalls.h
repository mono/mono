/**
 * \file
 * System.Native PAL internal calls
 * Adapter code between the Mono runtime and the CoreFX Platform Abstraction Layer (PAL)
 * Copyright 2018 Microsoft
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
*/

#ifndef __MONO_METADATA_PAL_ICALLS_H__
#define __MONO_METADATA_PAL_ICALLS_H__

#include "mono/metadata/metadata.h"
#include "mono/metadata/class-internals.h"
#include "pal_runtimeshutdown.h"
#include "pal_networking.h"
#include "pal_io.h"

MONO_API void mono_pal_init (void);
MONO_API void mono_pal_init_sockets (void);

#if defined(__APPLE__)
extern void mono_thread_info_install_interrupt (void (*callback) (gpointer data), gpointer data, gboolean *interrupted);
extern void mono_thread_info_uninstall_interrupt (gboolean *interrupted);
void ves_icall_Interop_RunLoop_CFRunLoopRun (void);
#endif

#endif
