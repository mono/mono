#ifndef __MONO_SGEN_REFERRING_OBJECTS_INLINES_H__
#define __MONO_SGEN_REFERRING_OBJECTS_INLINES_H__

#include <glib.h>
#include <sgen/sgen-pointer-queue.h>

typedef struct _ReferringObjectTuple ReferringObjectTuple;

typedef void (*RootProcessor) (ReferringObjectTuple *ref);

typedef struct {
	GCObject *check_key;
	RootRecord *check_root;
	gboolean precise;
	RootProcessor callback;
	gpointer *state;
} RootTraversalState;

typedef enum {
	REFERENCE_KIND_BROKEN = 0,
	REFERENCE_KIND_ROOT = 1,
	REFERENCE_KIND_OBJECT_FIELD = 2,
	REFERENCE_KIND_THREAD_STACK = 3,
	REFERENCE_KIND_THREAD_REG = 4
} ReferenceKind;

typedef struct {
	SgenThreadInfo *info;
	union {
		char **stack_addr;
		int reg;
	};
} SgenReferringThreadInfo;

struct _ReferringObjectTuple {
	ReferenceKind kind;
	gpointer *ptr_location;
	union {
		RootRecord *root;
		GCObject *obj;
		SgenReferringThreadInfo thread;
	} referring;
};

typedef SgenPointerQueue ReferringObjects;

// Initialize shared static state
void
sgen_init_referring_objects (void);

// Print out all incoming refs
void
mono_gc_scan_for_specific_ref (GCObject *key, gboolean precise);

// Return all incoming refs
void
sgen_get_incoming_references (GCObject *key, gboolean precise, ReferringObjects *pointers);

// Cleanup
void
sgen_free_incoming_references (ReferringObjects *refs);

/*
 * Print all of the pinning references
 * Note, do not use inside any function that is already mapping over roots.
 * For that use map_over_pinning_refs_from_threads directly
*/
void 
find_pinning_ref_from_thread (char *obj, size_t size);

#endif // #ifndef __MONO_SGEN_REFERRING_OBJECTS_INLINES_H__
