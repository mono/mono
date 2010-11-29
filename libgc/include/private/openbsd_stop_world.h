#ifndef GC_OPENBSD_STOP_WORLD_H
#define GC_OPENBSD_STOP_WORLD_H

#if !defined(GC_OPENBSD_THREADS)
#error openbsd_stop_world.h included without GC_OPENBSD_THREADS defined
#endif

struct thread_stop_info {
    ptr_t stack_ptr;  		/* Valid only when stopped.      	*/
};
    
#endif
