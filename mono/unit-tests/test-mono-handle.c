/*
 * test-mono-handle: tests for MonoHandle and MonoHandleArena
 *
 * Authors:
 *   Aleksey Kliger <aleksey@xamarin.com>
 *
 * Copyright 2015 Xamarin, Inc. (www.xamarin.com)
 */

#include <config.h>
#include <glib.h>
#include <mono/metadata/handle.h>

static void
test2_arena_push_pop ()
{
	MonoHandleArena *top = NULL;

	MonoHandleArena *new_arena1 = g_malloc0 (mono_handle_arena_size ());
	mono_handle_arena_stack_push (&top, new_arena1);

	MonoHandleArena *new_arena2 = g_malloc0 (mono_handle_arena_size ());

	mono_handle_arena_stack_push (&top, new_arena2);

	g_assert (top == new_arena2);

	mono_handle_arena_stack_pop (&top, new_arena2);

	g_free (new_arena2);

	g_assert (top == new_arena1);

	mono_handle_arena_stack_pop (&top, new_arena1);

	g_assert (top == NULL);
	
	g_free (new_arena1);
}

static void
test3_arena_mark_unwind ()
{
	MonoHandleArena *top = NULL;

	// check that these don't crash when the stack is empty.
	mono_handle_arena_set_unwind_mark (top);
	mono_handle_arena_clear_unwind_mark (top);
	g_assert (top == NULL);

	mono_handle_arena_stack_unwind_to_mark_and_clear (&top);
	g_assert (top == NULL);

	MonoHandleArena *new_arena1 = g_malloc0 (mono_handle_arena_size ());
	mono_handle_arena_stack_push (&top, new_arena1);

	mono_handle_arena_set_unwind_mark (new_arena1);

	g_assert (mono_handle_arena_unwind_mark_is_set (new_arena1));

	//top: [arena1 marked]NULL
	mono_handle_arena_stack_unwind_to_mark_and_clear (&top);
	//top: [arena1]NULL

	g_assert (top == new_arena1);
	g_assert (!mono_handle_arena_unwind_mark_is_set (new_arena1));

	MonoHandleArena *new_arena2 = g_malloc0 (mono_handle_arena_size ());
	mono_handle_arena_stack_push (&top, new_arena2);
	mono_handle_arena_set_unwind_mark (new_arena2);
	MonoHandleArena *new_arena3 = g_malloc0 (mono_handle_arena_size ());
	mono_handle_arena_stack_push (&top, new_arena3);
	mono_handle_arena_set_unwind_mark (new_arena3);
	MonoHandleArena *new_arena4 = g_malloc0 (mono_handle_arena_size ());
	mono_handle_arena_stack_push (&top, new_arena4);

	// top: [arena4][arena3 marked][arena2 marked][arena1]NULL
	mono_handle_arena_stack_unwind_to_mark_and_clear (&top);
	// top: [arena3][arena2 marked][arena1]NULL
	g_assert (top == new_arena3);
	g_assert (!mono_handle_arena_unwind_mark_is_set (new_arena3));
	g_assert (mono_handle_arena_unwind_mark_is_set (new_arena2));

	mono_handle_arena_stack_unwind_to_mark_and_clear (&top);
	// top: [arena2][arena1]NULL
	g_assert (top == new_arena2);
	g_assert (!mono_handle_arena_unwind_mark_is_set (new_arena2));

	mono_handle_arena_stack_unwind_to_mark_and_clear (&top);
	g_assert (top == NULL);
}


int
main (int argc, const char* argv[])
{
	test2_arena_push_pop ();
	test3_arena_mark_unwind ();

	return 0;
}
