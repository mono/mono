#ifndef LIBGC_MONO_DEBUGGER_H
#define LIBGC_MONO_DEBUGGER_H

#if defined(_IN_LIBGC_GC_H) || defined(IN_MONO_DEBUGGER)

typedef struct
{
	void (* initialize) (void);

	void (* stop_world) (void);
	void (* push_all_stacks) (void);
	void (* start_world) (void);
} GCThreadFunctions;

extern GCThreadFunctions *gc_thread_vtable;

#else
#error "This header is only intended to be used by the Mono Debugger"
#endif

#endif

