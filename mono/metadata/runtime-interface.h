#ifndef __RUNTIME_INTERFACE_H__
#define __RUNTIME_INTERFACE_H__

#include <mono/utils/mono-threads.h>



/*
The follow macros help with functions that either are called by managed code or
make calls to non-runtime functions.

Skeleton for an icall that returns no value:

void
do_something (MonoObject *this_raw, int flags)
{
	MonoError error;
	//Order matters
	//XXX it would be nice to colapse those two calls
	ICALL_ENTRY ();
	MONO_HANDLE_ARENA_PUSH ();

	//The convetion is that HANDLE_DCL wraps a variable with equal name with a _raw suffix. 
	MONO_HANDLE_DCL (this_obj);

	...
	//All icalls MUST have a single exit
	//XXX colapse those two calls into a single one
	//XXX force all epilogues to have a done label if we can avoid the warning
done:
	mono_error_set_pending_exception (&error);
	MONO_HANDLE_ARENA_POP ();
	ICALL_EXIT ();
}

Skeleton for an icall that returns a reference value

MonoObject*
do_something (void)
{
	MonoObject *ret_raw = NULL;
	MonoError error;
	ICALL_ENTRY ();
	LOCAL_HANDLE_PUSH_FRAME ();
	MonoObjectHandle ret;

	ret = mono_array_new_handle (...);

	//XXX this is pretty ugly, it would be nice to wrap this into something nicer
done:
	mono_error_set_pending_exception (&error);
	MONO_HANDLE_ARENA_POP_RETURN_UNSAFE (ret, ret_raw);
	ICALL_EXIT ();
	return ret_raw;
}


TODO

- Skeleton for non-icall functions
- Guidelines on how to handle'ize the existing API
- Skeleton + Guideless for fast (frame-less) icalls.

*/

/*
managed -> native helpers.

The following macros help working with icalls.

TODO:
	All those macros lack checked build asserts for entry/exit states and missuse.
*/


/*
The following macros must happen at the beginning of every single icall

their function is to transition the thread from managed into native code and ensure
the thread can be suspended if needed.

ICALL_ENTRY does push all registers and update the stack pointer. We need this because the managed
caller might have managed pointers in callee safe registers and we need to ensure the GC will see them.

ICALL_ENTRY_FAST doesn't update the stack mark, it must only be used with functions that ever safepoint.

ICALL_EXIT(_FAST) cleanup the work done by the matching ICALL_ENTRY function. It must be the last thing
to happen in the function and it must match the first one whether is __FAST or not.

TODO:
	1) optimize the case we control code gen and can ensure no callee saved registers will hold managed pointers,
	This needs to be coordinated with the effort to remove icall wrappers.

NOTES:
	Maybe we have functions that have a no-safepoints fast-path and we'll have to offer a split variant of ICALL_ENTRY
*/

#define ICALL_ENTRY() \
	__builtin_unwind_init ();	\
	MonoThreadInfo *__current_thread = mono_thread_info_current ();	\
	void *__previous_stack_mark = mono_thread_info_push_stack_mark (__current_thread, &__current_thread);	\

#define ICALL_ENTRY_FAST() \
	//Nothing to do here

#define ICALL_EXIT() \
	mono_thread_info_pop_stack_mark (__current_thread, __previous_stack_mark);

#define ICALL_EXIT_FAST() \
	//Nothing to do here


void* mono_thread_info_push_stack_mark (MonoThreadInfo *, void *);
void mono_thread_info_pop_stack_mark (MonoThreadInfo *, void *);

#endif


