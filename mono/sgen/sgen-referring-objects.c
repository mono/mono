
#include "mono/sgen/sgen-gc.h"
#include "mono/sgen/sgen-referring-objects.h"

LOCK_DECLARE (global_search_state_lock);
static RootTraversalState *global_search_state = NULL;


static void print_reference (ReferringObjectTuple *ref)
{
	switch (ref->kind) {
	case REFERENCE_KIND_ROOT:
		g_print ("found ref to %p in root record %p\n", *ref->ptr_location, ref->referring.root);
		break;
	case REFERENCE_KIND_OBJECT_FIELD: {
		GCVTable *vtable = SGEN_LOAD_VTABLE (ref->referring.obj);\
		size_t offset = (size_t)*ref->ptr_location - (size_t)ref->referring.obj;

		if (global_search_state->precise) {
			g_print ("found ref to %p in object %p (%s.%s) at offset %zd\n", \
					*ref->ptr_location, ref->referring.obj, sgen_client_vtable_get_namespace (vtable), sgen_client_vtable_get_name (vtable), offset); \
		} else {
			g_print ("found possible ref to %p in object %p (%s.%s) at offset %zd\n",
					*ref->ptr_location, ref->referring.obj, sgen_client_vtable_get_namespace (vtable), sgen_client_vtable_get_name (vtable), offset);
		}
		break;
	}
	case REFERENCE_KIND_THREAD_STACK:
		SGEN_LOG (0, "Object %p referenced in thread %p (id %p) at %p, stack: %p-%p", *ref->ptr_location, ref->referring.thread.info,
			(gpointer)mono_thread_info_get_tid (ref->referring.thread.info), ref->referring.thread.stack_addr,
			ref->referring.thread.info->client_info.stack_start, ref->referring.thread.info->client_info.stack_end);
		break;
	case REFERENCE_KIND_THREAD_REG:
		SGEN_LOG (0, "Object %p referenced in saved reg %d of thread %p (id %p)", *ref->ptr_location, ref->referring.thread.reg, ref->referring.thread.info,
			(gpointer)mono_thread_info_get_tid (ref->referring.thread.info));
		break;
	default:
		g_assert_not_reached ();
	}
}

static void
map_over_pinning_refs_from_threads (char *obj, size_t size)
{
#ifndef SGEN_WITHOUT_MONO
	SgenThreadInfo *info;
	char *endobj = obj + size;

	g_assert (global_search_state && global_search_state->callback);

	ReferringObjectTuple ref;

	// With the thread root types, 
	// We don't use ptr_location in any useful way beyond
	// communicating which object we're checking on so this
	// lets us use that.
	ref.ptr_location = (void **)&obj;

	FOREACH_THREAD (info) {
		SgenReferringThreadInfo *tinfo = &ref.referring.thread;
		tinfo->info = info;

		ref.kind = REFERENCE_KIND_THREAD_STACK;
		tinfo->stack_addr = (char**)info->client_info.stack_start;
		if (info->client_info.skip || info->client_info.gc_disabled)
			continue;
		while (tinfo->stack_addr < (char**)info->client_info.stack_end) {
			if (*tinfo->stack_addr >= obj && *tinfo->stack_addr < endobj)
				global_search_state->callback (&ref);
			tinfo->stack_addr++;
		}

		ref.kind = REFERENCE_KIND_THREAD_REG;
		for (tinfo->reg = 0; tinfo->reg < ARCH_NUM_REGS; tinfo->reg++) {
#ifdef USE_MONO_CTX
			mword w = ((mword*)&info->client_info.ctx) [tinfo->reg];
#else
			mword w = (mword)&info->client_info.regs [tinfo->reg];
#endif

			if (w >= (mword)obj && w < (mword)obj + size)
				global_search_state->callback (&ref);
		} END_FOREACH_THREAD
	}
#endif
}

// We assume this is called without registering a mapping function, and
// we want only to print out the info.
void 
find_pinning_ref_from_thread (char *obj, size_t size)
{
	RootTraversalState search_state;

	// print the references
	search_state.callback = print_reference;

	mono_mutex_lock (&global_search_state_lock);
	global_search_state = &search_state;

	map_over_pinning_refs_from_threads (obj, size);

	global_search_state = NULL;
	mono_mutex_unlock (&global_search_state_lock);
}

#undef HANDLE_PTR
#define HANDLE_PTR(ptr,obj_ptr) do {					\
		if ((GCObject*)*(ptr) == key && global_search_state) { \
			ReferringObjectTuple ref; \
			ref.kind = REFERENCE_KIND_OBJECT_FIELD; \
			ref.ptr_location = (ptr);\
			ref.referring.obj = (GCObject *)(obj_ptr);\
			global_search_state->callback (&ref); \
		} \
	} while (0)

static void
scan_object_for_specific_ref (char *start, GCObject *key)
{
	char *forwarded;

	if ((forwarded = SGEN_OBJECT_IS_FORWARDED (start)))
		start = forwarded;

	if (global_search_state && global_search_state->precise) {
		mword desc = sgen_obj_get_descriptor_safe (start);
		#include "sgen-scan-object.h"
	} else {
		mword *words = (mword*)start;
		size_t size = sgen_safe_object_get_size ((GCObject*)start);
		int i;
		for (i = 0; i < size / sizeof (mword); ++i) {
			if (words [i] == (mword)key) {
				if(global_search_state) {
					ReferringObjectTuple ref;
					ref.kind = REFERENCE_KIND_OBJECT_FIELD;
					ref.ptr_location = (void **) &(words [i]);
					ref.referring.obj = (MonoObject *)start;
					global_search_state->callback (&ref);
				}
			}
		}
	}
}

static void
scan_object_for_specific_ref_callback (char *obj, size_t size, GCObject *key)
{
	scan_object_for_specific_ref (obj, key);
}

static void
check_root_obj_specific_ref (RootRecord *root, GCObject *key, GCObject *obj)
{
	if (key != obj)
		return;
	if (global_search_state->callback) {
		ReferringObjectTuple ref;
		ref.kind = REFERENCE_KIND_ROOT;
		ref.ptr_location = (void **)obj;
		ref.referring.root = root;
		global_search_state->callback (&ref);
	}
}

static void
check_root_obj_specific_ref_from_marker (void **obj, void *gc_data)
{
	check_root_obj_specific_ref (global_search_state->check_root,
		global_search_state->check_key, *obj);
}

static void
scan_roots_for_specific_ref (GCObject *key, int root_type)
{
	void **start_root;
	RootRecord *root;
	global_search_state->check_key = key;

	SGEN_HASH_TABLE_FOREACH (&roots_hash [root_type], start_root, root) {
		mword desc = root->root_desc;

		global_search_state->check_root = root;

		switch (desc & ROOT_DESC_TYPE_MASK) {
		case ROOT_DESC_BITMAP:
			desc >>= ROOT_DESC_TYPE_SHIFT;
			while (desc) {
				if (desc & 1)
					check_root_obj_specific_ref (root, key, *start_root);
				desc >>= 1;
				start_root++;
			}
			return;
		case ROOT_DESC_COMPLEX: {
			gsize *bitmap_data = sgen_get_complex_descriptor_bitmap (desc);
			int bwords = (int) ((*bitmap_data) - 1);
			void **start_run = start_root;
			bitmap_data++;
			while (bwords-- > 0) {
				gsize bmap = *bitmap_data++;
				void **objptr = start_run;
				while (bmap) {
					if (bmap & 1)
						check_root_obj_specific_ref (root, key, *objptr);
					bmap >>= 1;
					++objptr;
				}
				start_run += GC_BITS_PER_WORD;
			}
			break;
		}
		case ROOT_DESC_USER: {
			SgenUserRootMarkFunc marker = sgen_get_user_descriptor_func (desc);
			marker (start_root, check_root_obj_specific_ref_from_marker, NULL);
			break;
		}
		case ROOT_DESC_RUN_LEN:
			g_assert_not_reached ();
		default:
			g_assert_not_reached ();
		}
	} SGEN_HASH_TABLE_FOREACH_END;

	global_search_state->check_key = NULL;
	global_search_state->check_root = NULL;
}

static void
mono_gc_foreach_referring_ref (GCObject *key, gboolean precise, RootProcessor fn, void *state)
{
	void **ptr;
	RootRecord *root;

	RootTraversalState search_state;
	search_state.check_key = NULL;
	search_state.check_root = NULL;
	search_state.precise = precise;
	search_state.callback = fn;

	search_state.state = state;

	mono_mutex_lock (&global_search_state_lock);
	global_search_state = &search_state;

	sgen_stop_world (0);
	sgen_clear_nursery_fragments ();

	sgen_scan_area_with_callback (nursery_section->data, nursery_section->end_data,
	(IterateObjectCallbackFunc)scan_object_for_specific_ref_callback, key, TRUE);

	major_collector.iterate_objects (ITERATE_OBJECTS_SWEEP_ALL, (IterateObjectCallbackFunc)scan_object_for_specific_ref_callback, key);

	sgen_los_iterate_objects ((IterateObjectCallbackFunc)scan_object_for_specific_ref_callback, key);

	scan_roots_for_specific_ref (key, ROOT_TYPE_NORMAL);
	scan_roots_for_specific_ref (key, ROOT_TYPE_WBARRIER);

	SGEN_HASH_TABLE_FOREACH (&roots_hash [ROOT_TYPE_PINNED], ptr, root) {
		while (ptr < (void**)root->end_root) {
			check_root_obj_specific_ref (root, *ptr, key);
			++ptr;
		}
	} SGEN_HASH_TABLE_FOREACH_END;

	if (sgen_is_world_stopped ())
		map_over_pinning_refs_from_threads ((char*)key, sizeof (GCObject));

	sgen_restart_world (0, NULL);

	global_search_state = NULL;
	mono_mutex_unlock (&global_search_state_lock);
}

static void 
accumulate_reference (ReferringObjectTuple *ref)
{
	g_assert (ref->kind != REFERENCE_KIND_BROKEN);
	
	// FIXME: Serialize the root data in some form that's useful to return
	if (ref->kind != REFERENCE_KIND_OBJECT_FIELD)
		return;

	ReferringObjectTuple *this = sgen_alloc_internal (INTERNAL_MEM_OBJECT_REFERENCES);
	memcpy (this, ref, sizeof (ReferringObjectTuple));

	SGEN_PIN_OBJECT (ref->referring.obj);

	sgen_pointer_queue_add ((ReferringObjects *) global_search_state->state, this);
}

void
sgen_free_incoming_references (ReferringObjects *refs)
{
	while (!sgen_pointer_queue_is_empty (refs)) {
		ReferringObjectTuple *curr = (ReferringObjectTuple *) sgen_pointer_queue_pop (refs);
		g_assert (curr->kind != REFERENCE_KIND_BROKEN);
		SGEN_UNPIN_OBJECT (curr->referring.obj);
		sgen_free_internal (curr, INTERNAL_MEM_OBJECT_REFERENCES);
	}
	sgen_pointer_queue_free (refs);
}

void
sgen_get_incoming_references (GCObject *key, gboolean precise, ReferringObjects *pointers)
{
	sgen_pointer_queue_init (pointers, INTERNAL_MEM_OBJECT_REFERENCES);
	mono_gc_foreach_referring_ref (key, precise, (RootProcessor)accumulate_reference, pointers);
	sgen_pointer_queue_remove_nulls (pointers);
}

void
mono_gc_scan_for_specific_ref (GCObject *key, gboolean precise)
{
	mono_gc_foreach_referring_ref (key, precise, (RootProcessor)print_reference, NULL);
}


void
sgen_init_referring_objects (void)
{
	sgen_register_fixed_internal_mem_type (INTERNAL_MEM_OBJECT_REFERENCES, sizeof (ReferringObjectTuple));
	LOCK_INIT (global_search_state_lock);
}

