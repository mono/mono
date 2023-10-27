/*
 * Copyright (c) 1994 by Xerox Corporation.  All rights reserved.
 * Copyright (c) 1996 by Silicon Graphics.  All rights reserved.
 * Copyright (c) 1998 by Fergus Henderson.  All rights reserved.
 * Copyright (c) 2000-2009 by Hewlett-Packard Development Company.
 * All rights reserved.
 *
 * THIS MATERIAL IS PROVIDED AS IS, WITH ABSOLUTELY NO WARRANTY EXPRESSED
 * OR IMPLIED.  ANY USE IS AT YOUR OWN RISK.
 *
 * Permission is hereby granted to use or copy this program
 * for any purpose,  provided the above notices are retained on all copies.
 * Permission to modify the code and to distribute modified code is granted,
 * provided the above notices are retained, and a notice that the code was
 * modified is included with the above copyright notice.
 */

#ifndef GC_DARWIN_STOP_WORLD_H
#define GC_DARWIN_STOP_WORLD_H

#if !defined(GC_DARWIN_THREADS)
# error darwin_stop_world.h included without GC_DARWIN_THREADS defined
#endif

#include <mach/mach.h>
#include <mach/thread_act.h>

EXTERN_C_BEGIN

struct thread_stop_info {
  mach_port_t mach_thread;
  ptr_t stack_ptr; /* Valid only when thread is in a "blocked" state.   */
};

#ifndef DARWIN_DONT_PARSE_STACK
  GC_INNER ptr_t GC_FindTopOfStack(unsigned long);
#endif

#ifdef MPROTECT_VDB
  GC_INNER void GC_mprotect_stop(void);
  GC_INNER void GC_mprotect_resume(void);
# ifndef GC_NO_THREADS_DISCOVERY
    GC_INNER void GC_darwin_register_mach_handler_thread(mach_port_t thread);
# endif
#endif

#if defined(PARALLEL_MARK) && !defined(GC_NO_THREADS_DISCOVERY)
  GC_INNER GC_bool GC_is_mach_marker(thread_act_t);
#endif

EXTERN_C_END

#endif
