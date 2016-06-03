#include <config.h>

#include <mono/metadata/runtime-interface.h>

void*
mono_thread_info_push_stack_mark (MonoThreadInfo *info, void *new_stack_bottom)
{
	return NULL;
}

void
mono_thread_info_pop_stack_mark (MonoThreadInfo *info, void *old_stack_bottom)
{
}
