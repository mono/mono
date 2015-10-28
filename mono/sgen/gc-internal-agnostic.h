/*
 * gc-internal-agnostic.h: Mono-agnostic GC interface.
 *
 * Copyright (C) 2015 Xamarin Inc
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

#ifndef __MONO_METADATA_GCINTERNALAGNOSTIC_H__
#define __MONO_METADATA_GCINTERNALAGNOSTIC_H__

#include <config.h>
#include <glib.h>
#include <stdio.h>

#include "mono/utils/mono-compiler.h"
#include "mono/utils/parse.h"
#include "mono/utils/memfuncs.h"
#ifdef HAVE_SGEN_GC
#include "mono/sgen/sgen-conf.h"
#endif

/* h indicates whether to hide or just tag.
 * (-!!h ^ p) is used instead of (h ? ~p : p) to avoid multiple mentions of p.
 */
#define MONO_GC_HIDE_POINTER(p,t,h) ((gpointer)(((-(size_t)!!(h) ^ (size_t)(p)) & ~3UL) | (t & 3UL)))
#define MONO_GC_REVEAL_POINTER(p,h) ((gpointer)((-(size_t)!!(h) ^ (size_t)(p)) & ~3UL))

#define MONO_GC_POINTER_TAG(p) ((size_t)(p) & 3UL)

#define MONO_GC_HANDLE_OCCUPIED_MASK (1)
#define MONO_GC_HANDLE_VALID_MASK (2)
#define MONO_GC_HANDLE_TAG_MASK (MONO_GC_HANDLE_OCCUPIED_MASK | MONO_GC_HANDLE_VALID_MASK)

#define MONO_GC_HANDLE_METADATA_POINTER(p,h) (MONO_GC_HIDE_POINTER ((p), MONO_GC_HANDLE_OCCUPIED_MASK, (h)))
#define MONO_GC_HANDLE_OBJECT_POINTER(p,h) (MONO_GC_HIDE_POINTER ((p), MONO_GC_HANDLE_OCCUPIED_MASK | MONO_GC_HANDLE_VALID_MASK, (h)))

#define MONO_GC_HANDLE_OCCUPIED(slot) ((size_t)(slot) & MONO_GC_HANDLE_OCCUPIED_MASK)
#define MONO_GC_HANDLE_VALID(slot) ((size_t)(slot) & MONO_GC_HANDLE_VALID_MASK)

#define MONO_GC_HANDLE_TAG(slot) ((size_t)(slot) & MONO_GC_HANDLE_TAG_MASK)

#define MONO_GC_HANDLE_IS_OBJECT_POINTER(slot) (MONO_GC_HANDLE_TAG (slot) == (MONO_GC_HANDLE_OCCUPIED_MASK | MONO_GC_HANDLE_VALID_MASK))
#define MONO_GC_HANDLE_IS_METADATA_POINTER(slot) (MONO_GC_HANDLE_TAG (slot) == MONO_GC_HANDLE_OCCUPIED_MASK)

typedef enum {
	HANDLE_TYPE_MIN = 0,
	HANDLE_WEAK = HANDLE_TYPE_MIN,
	HANDLE_WEAK_TRACK,
	HANDLE_NORMAL,
	HANDLE_PINNED,
	HANDLE_TYPE_MAX
} GCHandleType;

#define GC_HANDLE_TYPE_IS_WEAK(x) ((x) <= HANDLE_WEAK_TRACK)

#define MONO_GC_HANDLE_TYPE_SHIFT (3)
#define MONO_GC_HANDLE_TYPE_MASK ((1 << MONO_GC_HANDLE_TYPE_SHIFT) - 1)
#define MONO_GC_HANDLE_TYPE(x) (((x) & MONO_GC_HANDLE_TYPE_MASK) - 1)
#define MONO_GC_HANDLE_SLOT(x) ((x) >> MONO_GC_HANDLE_TYPE_SHIFT)
#define MONO_GC_HANDLE_TYPE_IS_WEAK(x) ((x) <= HANDLE_WEAK_TRACK)
#define MONO_GC_HANDLE(slot, type) (((slot) << MONO_GC_HANDLE_TYPE_SHIFT) | (((type) & MONO_GC_HANDLE_TYPE_MASK) + 1))

typedef struct {
	guint minor_gc_count;
	guint major_gc_count;
	guint64 minor_gc_time;
	guint64 major_gc_time;
	guint64 major_gc_time_concurrent;
} GCStats;

extern GCStats gc_stats;

#ifdef HAVE_SGEN_GC
typedef SgenDescriptor MonoGCDescriptor;
#define MONO_GC_DESCRIPTOR_NULL	SGEN_DESCRIPTOR_NULL
#else
typedef void* MonoGCDescriptor;
#define MONO_GC_DESCRIPTOR_NULL NULL
#endif

/*
 * Try to register a foreign thread with the GC, if we fail or the backend
 * can't cope with this concept - we return FALSE.
 */
extern gboolean mono_gc_register_thread (void *baseptr);

gboolean mono_gc_parse_environment_string_extract_number (const char *str, size_t *out);

MonoGCDescriptor mono_gc_make_descr_for_object (gsize *bitmap, int numbits, size_t obj_size);
MonoGCDescriptor mono_gc_make_descr_for_array (int vector, gsize *elem_bitmap, int numbits, size_t elem_size);

/* simple interface for data structures needed in the runtime */
MonoGCDescriptor mono_gc_make_descr_from_bitmap (gsize *bitmap, int numbits);

/* Return a root descriptor for a root with all refs */
MonoGCDescriptor mono_gc_make_root_descr_all_refs (int numbits);

/* Return the bitmap encoded by a descriptor */
gsize* mono_gc_get_bitmap_for_descr (MonoGCDescriptor descr, int *numbits);

/*
These functions must be used when it's possible that either destination is not
word aligned or size is not a multiple of word size.
*/
void mono_gc_bzero_atomic (void *dest, size_t size);
void mono_gc_bzero_aligned (void *dest, size_t size);
void mono_gc_memmove_atomic (void *dest, const void *src, size_t size);
void mono_gc_memmove_aligned (void *dest, const void *src, size_t size);

FILE *mono_gc_get_logfile (void);

/*
 * This causes the compile to extend the liveness of 'v' till the call to dummy_use
 */
static inline void
mono_gc_dummy_use (gpointer v) {
#if defined(__GNUC__)
	__asm__ volatile ("" : "=r"(v) : "r"(v));
#elif defined(_MSC_VER)
	static volatile gpointer ptr;
	ptr = v;
#else
#error "Implement mono_gc_dummy_use for your compiler"
#endif
}

#endif
