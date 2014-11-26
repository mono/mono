/*
 * sgen-copy-object.h: This is where objects are copied.
 *
 * Copyright 2001-2003 Ximian, Inc
 * Copyright 2003-2010 Novell, Inc.
 * Copyright (C) 2012 Xamarin Inc
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Library General Public
 * License 2.0 as published by the Free Software Foundation;
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Library General Public License for more details.
 *
 * You should have received a copy of the GNU Library General Public
 * License 2.0 along with this library; if not, write to the Free
 * Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 */

#include "mono/utils/mono-compiler.h"

extern guint64 stat_copy_object_called_nursery;
extern guint64 stat_objects_copied_nursery;

extern guint64 stat_nursery_copy_object_failed_from_space;
extern guint64 stat_nursery_copy_object_failed_forwarded;
extern guint64 stat_nursery_copy_object_failed_pinned;

extern guint64 stat_slots_allocated_in_vain;

/*
 * Copies an object and enqueues it if a queue is given.
 *
 * This function can be used even if the vtable of obj is not valid
 * anymore, which is the case in the parallel collector.
 */
static MONO_ALWAYS_INLINE void
par_copy_object_no_checks (char *destination, MonoVTable *vt, void *obj, mword objsize, SgenGrayQueue *queue)
{
	SGEN_ASSERT (9, vt->klass->inited, "vtable %p for class %s:%s was not initialized", vt, vt->klass->name_space, vt->klass->name);
	SGEN_LOG (9, " (to %p, %s size: %lu)", destination, ((MonoObject*)obj)->vtable->klass->name, (unsigned long)objsize);
	binary_protocol_copy (obj, destination, vt, objsize);

#ifdef ENABLE_DTRACE
	if (G_UNLIKELY (MONO_GC_OBJ_MOVED_ENABLED ())) {
		int dest_gen = sgen_ptr_in_nursery (destination) ? GENERATION_NURSERY : GENERATION_OLD;
		int src_gen = sgen_ptr_in_nursery (obj) ? GENERATION_NURSERY : GENERATION_OLD;
		MONO_GC_OBJ_MOVED ((mword)destination, (mword)obj, dest_gen, src_gen, objsize, vt->klass->name_space, vt->klass->name);
	}
#endif

	memcpy (destination + sizeof (mword), (char*)obj + sizeof (mword), objsize - sizeof (mword));

	/* adjust array->bounds */
	SGEN_ASSERT (9, vt->gc_descr, "vtable %p for class %s:%s has no gc descriptor", vt, vt->klass->name_space, vt->klass->name);

	if (G_UNLIKELY (vt->rank && ((MonoArray*)obj)->bounds)) {
		MonoArray *array = (MonoArray*)destination;
		array->bounds = (MonoArrayBounds*)((char*)destination + ((char*)((MonoArray*)obj)->bounds - (char*)obj));
		SGEN_LOG (9, "Array instance %p: size: %lu, rank: %d, length: %lu", array, (unsigned long)objsize, vt->rank, (unsigned long)mono_array_length (array));
	}
	if (G_UNLIKELY (mono_profiler_events & MONO_PROFILE_GC_MOVES))
		sgen_register_moved_object (obj, destination);
	obj = destination;
	if (queue) {
		SGEN_LOG (9, "Enqueuing gray object %p (%s)", obj, sgen_safe_name (obj));
		GRAY_OBJECT_ENQUEUE (queue, obj, sgen_vtable_get_descriptor (vt));
	}
}

/*
 * This can return OBJ itself on OOM.
 */
static MONO_NEVER_INLINE void*
copy_object_no_checks (void *obj, SgenGrayQueue *queue)
{
	MonoVTable *vt = ((MonoObject*)obj)->vtable;
	gboolean has_references = SGEN_VTABLE_HAS_REFERENCES (vt);
	mword objsize = SGEN_ALIGN_UP (sgen_par_object_get_size (vt, (MonoObject*)obj));
	/* FIXME: Does this not mark the newly allocated object? */
	char *destination = COLLECTOR_SERIAL_ALLOC_FOR_PROMOTION (vt, obj, objsize, has_references);

	if (G_UNLIKELY (!destination)) {
		/* FIXME: Is this path ever tested? */
		collector_pin_object (obj, queue);
		sgen_set_pinned_from_failed_allocation (objsize);
		return obj;
	}

	if (!has_references)
		queue = NULL;

	par_copy_object_no_checks (destination, vt, obj, objsize, queue);
	/* FIXME: mark mod union cards if necessary */

	/* set the forwarding pointer */
	SGEN_FORWARD_OBJECT (obj, destination);

	return destination;
}
