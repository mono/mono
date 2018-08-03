/**
 * \file
 * ObjectiveC hacks to improve our changes with thread shutdown
 *
 * This is separate from mono-threads.h so mono-threads-mach-helper.c
 * can avoid io-layer internal types which conflict with Objective C.
 *
 * Author:
 *	Rodrigo Kumpera (kumpera@gmail.com)
 *	Jay Krell (jaykrell@microsoft.com)
 *
 * (C) 2014 Xamarin Inc
 */

#ifndef __MONO_THREADS_MACH_HELPER_H__
#define __MONO_THREADS_MACH_HELPER_H__

#ifdef __MACH__

void mono_threads_init_dead_letter (void);
void mono_threads_install_dead_letter (void);

// This is MONO_API elsewhere so at least extern "C".
// MONO_BEGIN_DECLS and G_BEGIN_DECLS are not available here.
#ifdef __cplusplus
extern "C"
#endif
void mono_thread_info_detach (void);

#endif // __MACH__
#endif // __MONO_THREADS_MACH_HELPER_H__
