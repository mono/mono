/*
 * checked-build.c: Expensive asserts used when mono is built with --with-checked-build=yes
 *
 * Author:
 *	Rodrigo Kumpera (kumpera@gmail.com)
 *
 * (C) 2015 Xamarin
 */
#include <config.h>
#ifdef CHECKED_BUILD

#include <mono/utils/checked-build.h>
#include <mono/utils/mono-threads.h>
#include <mono/utils/mono-tls.h>
#include <mono/metadata/mempool.h>
#include <mono/metadata/metadata-internals.h>
#include <mono/metadata/image-internals.h>
#include <mono/metadata/class-internals.h>
#include <glib.h>

#define MAX_NATIVE_BT 6
#define MAX_NATIVE_BT_PROBE (MAX_NATIVE_BT + 5)
#define MAX_TRANSITIONS 3


#ifdef HAVE_BACKTRACE_SYMBOLS
#include <execinfo.h>

//XXX We should collect just the IPs and lazily symbolificate them.
static int
collect_backtrace (gpointer out_data[])
{
	return backtrace (out_data, MAX_NATIVE_BT_PROBE);
}

static char*
translate_backtrace (gpointer native_trace[], int size)
{
	char **names = backtrace_symbols (native_trace, size);
	GString* bt = g_string_sized_new (100);

	int i, j = -1;

	//Figure out the cut point of useless backtraces
	//We'll skip up to the caller of checked_build_thread_transition
	for (i = 0; i < size; ++i) {
		if (strstr (names [i], "checked_build_thread_transition")) {
			j = i + 1;
			break;
		}
	}

	if (j == -1)
		j = 0;
	for (i = j; i < size; ++i) {
		if (i - j <= MAX_NATIVE_BT)
			g_string_append_printf (bt, "\tat %s\n", names [i]);
	}

	free (names);
	return g_string_free (bt, FALSE);
}

#else

static int
collect_backtrace (gpointer out_data[])
{
	return 0;
}

static char*
translate_backtrace (gpointer native_trace[], int size)
{
	return g_strdup ("\tno backtrace available\n");
}

#endif


typedef struct {
	GPtrArray *transitions;
} CheckState;

typedef struct {
	const char *name;
	int from_state, next_state, suspend_count, suspend_count_delta, size;
	gpointer backtrace [MAX_NATIVE_BT_PROBE];
} ThreadTransition;

static MonoNativeTlsKey thread_status;

void
checked_build_init (void)
{
	mono_native_tls_alloc (&thread_status, NULL);
}

static CheckState*
get_state (void)
{
	CheckState *state = mono_native_tls_get_value (thread_status);
	if (!state) {
		state = g_new0 (CheckState, 1);
		state->transitions = g_ptr_array_new ();
		mono_native_tls_set_value (thread_status, state);
	}

	return state;
}

static void
free_transition (ThreadTransition *t)
{
	g_free (t);
}

void
checked_build_thread_transition (const char *transition, void *info, int from_state, int suspend_count, int next_state, int suspend_count_delta)
{
	MonoThreadInfo *cur = mono_thread_info_current_unchecked ();
	CheckState *state = get_state ();
	/* We currently don't record external changes as those are hard to reason about. */
	if (cur != info)
		return;

	if (state->transitions->len >= MAX_TRANSITIONS)
		free_transition (g_ptr_array_remove_index (state->transitions, 0));

	ThreadTransition *t = g_new0 (ThreadTransition, 1);
	t->name = transition;
	t->from_state = from_state;
	t->next_state = next_state;
	t->suspend_count = suspend_count;
	t->suspend_count_delta = suspend_count_delta;
	t->size = collect_backtrace (t->backtrace);
	g_ptr_array_add (state->transitions, t);
}

static void
assertion_fail (const char *msg, ...)
{
	int i;
	GString* err = g_string_sized_new (100);
	CheckState *state = get_state ();

	g_string_append_printf (err, "Assertion failure in thread %p due to: ", mono_native_thread_id_get ());

	va_list args;
	va_start (args, msg);
	g_string_append_vprintf (err, msg, args);
	va_end (args);

	g_string_append_printf (err, "\nLast %d state transitions: (most recent first)\n", state->transitions->len);

	for (i = state->transitions->len - 1; i >= 0; --i) {
		ThreadTransition *t = state->transitions->pdata [i];
		char *bt = translate_backtrace (t->backtrace, t->size);
		g_string_append_printf (err, "[%s] %s -> %s (%d) %s%d at:\n%s",
			t->name,
			mono_thread_state_name (t->from_state),
			mono_thread_state_name (t->next_state),
			t->suspend_count,
			t->suspend_count_delta > 0 ? "+" : "", //I'd like to see this sort of values: -1, 0, +1
			t->suspend_count_delta,
			bt);
		g_free (bt);
	}

	g_error (err->str);
	g_string_free (err, TRUE);
}

void
assert_gc_safe_mode (void)
{
	MonoThreadInfo *cur = mono_thread_info_current ();
	int state;

	if (!cur)
		assertion_fail ("Expected GC Safe mode but thread is not attached");

	switch (state = mono_thread_info_current_state (cur)) {
	case STATE_BLOCKING:
	case STATE_BLOCKING_AND_SUSPENDED:
		break;
	default:
		assertion_fail ("Expected GC Safe mode but was in %s state", mono_thread_state_name (state));
	}
}

void
assert_gc_unsafe_mode (void)
{
	MonoThreadInfo *cur = mono_thread_info_current ();
	int state;

	if (!cur)
		assertion_fail ("Expected GC Unsafe mode but thread is not attached");

	switch (state = mono_thread_info_current_state (cur)) {
	case STATE_RUNNING:
	case STATE_ASYNC_SUSPEND_REQUESTED:
	case STATE_SELF_SUSPEND_REQUESTED:
		break;
	default:
		assertion_fail ("Expected GC Unsafe mode but was in %s state", mono_thread_state_name (state));
	}
}

void
assert_gc_neutral_mode (void)
{
	MonoThreadInfo *cur = mono_thread_info_current ();
	int state;

	if (!cur)
		assertion_fail ("Expected GC Neutral mode but thread is not attached");

	switch (state = mono_thread_info_current_state (cur)) {
	case STATE_RUNNING:
	case STATE_ASYNC_SUSPEND_REQUESTED:
	case STATE_SELF_SUSPEND_REQUESTED:
	case STATE_BLOCKING:
	case STATE_BLOCKING_AND_SUSPENDED:
		break;
	default:
		assertion_fail ("Expected GC Neutral mode but was in %s state", mono_thread_state_name (state));
	}
}

// check_metadata_store et al: The goal of these functions is to verify that if there is a pointer from one mempool into
// another, that the pointed-to memory is protected by the reference count mechanism for MonoImages.
//
// Note: The code below catches only some kinds of failures. Failures outside its scope notably incode:
// * Code below absolutely assumes that no mempool is ever held as "mempool" member by more than one Image or ImageSet at once
// * Code below assumes reference counts never underflow (ie: if we have a pointer to something, it won't be deallocated while we're looking at it)
// Locking strategy is a little slapdash overall.

#define check_mempool_assert_message(...) \
	g_assertion_message("Mempool reference violation: " __VA_ARGS__)

// Say image X "references" image Y if X either contains Y in its modules field, or X’s "references" field contains an
// assembly whose image is Y.
// Say image X transitively references image Y if there is any chain of images-referencing-images which leads from X to Y.
// Once the mempools for two pointers have been looked up, there are four possibilities:

// Case 1. Image FROM points to Image TO: Legal if FROM transitively references TO

// We'll do a simple BFS graph search on images. For each image we visit:
static void
check_image_search (GHashTable *visited, GPtrArray *next, MonoImage *candidate, MonoImage *goal, gboolean *success)
{
	// Image hasn't even been loaded-- ignore it
	if (!candidate)
		return;

	// Image has already been visited-- ignore it
	if (g_hash_table_lookup_extended (visited, candidate, NULL, NULL))
		return;

	// Image is the target-- mark success
	if (candidate == goal)
	{
		*success = TRUE;
		return;
	}

	// Unvisited image, queue it to have its children visited
	g_hash_table_insert (visited, candidate, NULL);
	g_ptr_array_add (next, candidate);
	return;
}

static gboolean
check_image_may_reference_image(MonoImage *from, MonoImage *to)
{
	if (to == from) // Shortcut
		return TRUE;

	// Corlib is never unloaded, and all images implicitly reference it.
	// Some images avoid explicitly referencing it as an optimization, so special-case it here.
	if (to == mono_defaults.corlib)
		return TRUE;

	gboolean success = FALSE;

	// Images to inspect on this pass, images to inspect on the next pass
	GPtrArray *current = g_ptr_array_sized_new (1), *next = g_ptr_array_new ();

	// Because in practice the image graph contains cycles, we must track which images we've visited
	GHashTable *visited = g_hash_table_new (NULL, NULL);

	#define CHECK_IMAGE_VISIT(i) check_image_search (visited, next, (i), to, &success)

	CHECK_IMAGE_VISIT (from); // Initially "next" contains only from node

	// For each pass exhaust the "to check" queue while filling up the "check next" queue
	while (!success && next->len > 0) // Halt on success or when out of nodes to process
	{
		// Swap "current" and "next" and clear next
		GPtrArray *temp = current;
		current = next;
		next = temp;
		g_ptr_array_set_size (next, 0);

		int current_idx;
		for(current_idx = 0; current_idx < current->len; current_idx++)
		{
			MonoImage *checking = g_ptr_array_index (current, current_idx); // CAST?

			mono_image_lock (checking);

			// For each queued image visit all directly referenced images
			int inner_idx;

			for (inner_idx = 0; !success && inner_idx < checking->module_count; inner_idx++)
			{
				CHECK_IMAGE_VISIT (checking->modules[inner_idx]);
			}

			for (inner_idx = 0; !success && inner_idx < checking->nreferences; inner_idx++)
			{
				// References are lazy-loaded and thus allowed to be NULL.
				// If they are NULL, we don't care about them for this search, because they haven't impacted ref_count yet.
				if (checking->references[inner_idx])
				{
					CHECK_IMAGE_VISIT (checking->references[inner_idx]->image);
				}
			}

			mono_image_unlock (checking);
		}
	}

	g_ptr_array_free (current, TRUE); g_ptr_array_free (next, TRUE); g_hash_table_destroy (visited);

	return success;
}

// Case 2. ImageSet FROM points to Image TO: One of FROM's "images" either is, or transitively references, TO.
static gboolean
check_image_set_may_reference_image (MonoImageSet *from, MonoImage *to)
{
	// See above-- All images implicitly reference corlib
	if (to == mono_defaults.corlib)
		return TRUE;

	int idx;
	gboolean success = FALSE;
	mono_image_set_lock (from);
	for (idx = 0; !success && idx < from->nimages; idx++)
	{
		if (!check_image_may_reference_image (from->images[idx], to))
			success = TRUE;
	}
	mono_image_set_unlock (from);

	return success; // No satisfying image found in from->images
}

// Case 3. ImageSet FROM points to ImageSet TO: The images in TO are a strict subset of FROM (no transitive relationship is important here)
static gboolean
check_image_set_may_reference_image_set (MonoImageSet *from, MonoImageSet *to)
{
	if (to == from)
		return TRUE;

	gboolean valid = TRUE; // Until proven otherwise

	mono_image_set_lock (from); mono_image_set_lock (to);

	int to_idx, from_idx;
	for (to_idx = 0; valid && to_idx < to->nimages; to_idx++)
	{
		gboolean seen = FALSE;

		// For each item in to->images, scan over from->images looking for it.
		for (from_idx = 0; !seen && from_idx < from->nimages; from_idx++)
		{
			if (to->images[to_idx] == from->images[from_idx])
				seen = TRUE;
		}

		// If the to->images item is not found in from->images, the subset check has failed
		if (!seen)
			valid = FALSE;
	}

	mono_image_set_unlock (from); mono_image_set_unlock (to);

	return valid; // All items in "to" were found in "from"
}

// Case 4. Image FROM points to ImageSet TO: FROM transitively references *ALL* of the “images” listed in TO
static gboolean
check_image_may_reference_image_set (MonoImage *from, MonoImageSet *to)
{
	if (to->nimages == 0) // Malformed image_set
		return FALSE;

	gboolean valid = TRUE;

	mono_image_set_lock (to);
	int idx;
	for (idx = 0; valid && idx < to->nimages; idx++)
	{
		if (!check_image_may_reference_image (from, to->images[idx]))
			valid = FALSE;
	}
	mono_image_set_unlock (to);

	return valid; // All images in to->images checked out
}

// Small helper-- get a descriptive string for a MonoMemPoolOwner
static const char *
check_mempool_owner_name (MonoMemPoolOwner owner)
{
	if (owner.image)
		return owner.image->name;
	if (owner.image_set) // TODO: Construct a string containing all included images
		return "(Imageset)";
	return "(Non-image memory)";
}

static void
check_mempool_may_reference_mempool (void *from_ptr, void *to_ptr, gboolean require_local)
{
	// Null pointers are OK
	if (!to_ptr)
		return;

	MonoMemPoolOwner from = mono_find_mempool_owner (from_ptr), to = mono_find_mempool_owner (to_ptr);

	if (require_local)
	{
		if (!check_mempool_owner_eq (from,to))
			check_mempool_assert_message ("Pointer in image %s should have been internal, but instead pointed to image %s", check_mempool_owner_name(from), check_mempool_owner_name(to));
	}

	// Writing into unknown mempool
	else if (check_mempool_owner_eq (from, mono_mempool_no_owner))
	{
		check_mempool_assert_message ("Non-image memory attempting to write pointer to image %s", check_mempool_owner_name(to));
	}

	// Reading from unknown mempool
	else if (check_mempool_owner_eq (to, mono_mempool_no_owner))
	{
		check_mempool_assert_message ("Attempting to write pointer from image %s to non-image memory", check_mempool_owner_name(from));
	}

	// Split out the four cases described above:
	else if (from.image && to.image)
	{
		if (!check_image_may_reference_image (from.image, to.image))
			check_mempool_assert_message ("Image %s tried to point to image %s, but does not retain a reference", check_mempool_owner_name(from), check_mempool_owner_name(to));
	}

	else if (from.image && to.image_set)
	{
		if (!check_image_may_reference_image_set (from.image, to.image_set))
			check_mempool_assert_message ("Image %s tried to point to image set, but does not retain a reference", check_mempool_owner_name(from));
	}

	else if (from.image_set && to.image_set)
	{
		if (!check_image_set_may_reference_image_set (from.image_set, to.image_set))
			check_mempool_assert_message ("Image set tried to point to image set, but does not retain a reference");
	}

	else if (from.image_set && to.image)
	{
		if (!check_image_set_may_reference_image (from.image_set, to.image))
			check_mempool_assert_message ("Image set tried to point to image %s, but does not retain a reference", check_mempool_owner_name(to));
	}

	else
	{
		check_mempool_assert_message ("Internal logic error: Unreachable code");
	}
}

void
check_metadata_store (void *from, void *to)
{
    check_mempool_may_reference_mempool (from, to, FALSE);
}

void
check_metadata_store_local (void *from, void *to)
{
    check_mempool_may_reference_mempool (from, to, TRUE);
}

#endif /* CHECKED_BUILD */
