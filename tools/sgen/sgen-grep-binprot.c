#include <stdio.h>
#include <stdlib.h>
#include <assert.h>
#include <glib.h>

#define SGEN_BINARY_PROTOCOL
#define MONO_INTERNAL

#include <mono/metadata/sgen-protocol.h>

#define SGEN_PROTOCOL_EOF	255

#define TYPE(t)		((t) & 0x7f)
#define WORKER(t)	((t) & 0x80)

static int
read_entry (FILE *in, void **data)
{
	unsigned char type;
	int size;

	if (fread (&type, 1, 1, in) != 1)
		return SGEN_PROTOCOL_EOF;
	switch (TYPE (type)) {
	case SGEN_PROTOCOL_COLLECTION_FORCE: size = sizeof (SGenProtocolCollectionForce); break;
	case SGEN_PROTOCOL_COLLECTION_BEGIN: size = sizeof (SGenProtocolCollection); break;
	case SGEN_PROTOCOL_COLLECTION_END: size = sizeof (SGenProtocolCollection); break;
	case SGEN_PROTOCOL_CONCURRENT_START: size = 0; break;
	case SGEN_PROTOCOL_CONCURRENT_UPDATE_FINISH: size = 0; break;
	case SGEN_PROTOCOL_WORLD_STOPPING: size = sizeof (SGenProtocolWorldStopping); break;
	case SGEN_PROTOCOL_WORLD_STOPPED: size = sizeof (SGenProtocolWorldStopped); break;
	case SGEN_PROTOCOL_WORLD_RESTARTING: size = sizeof (SGenProtocolWorldRestarting); break;
	case SGEN_PROTOCOL_WORLD_RESTARTED: size = sizeof (SGenProtocolWorldRestarted); break;
	case SGEN_PROTOCOL_ALLOC: size = sizeof (SGenProtocolAlloc); break;
	case SGEN_PROTOCOL_ALLOC_PINNED: size = sizeof (SGenProtocolAlloc); break;
	case SGEN_PROTOCOL_ALLOC_DEGRADED: size = sizeof (SGenProtocolAlloc); break;
	case SGEN_PROTOCOL_COPY: size = sizeof (SGenProtocolCopy); break;
	case SGEN_PROTOCOL_PIN: size = sizeof (SGenProtocolPin); break;
	case SGEN_PROTOCOL_MARK: size = sizeof (SGenProtocolMark); break;
	case SGEN_PROTOCOL_SCAN_BEGIN: size = sizeof (SGenProtocolScanBegin); break;
	case SGEN_PROTOCOL_SCAN_VTYPE_BEGIN: size = sizeof (SGenProtocolScanVTypeBegin); break;
	case SGEN_PROTOCOL_WBARRIER: size = sizeof (SGenProtocolWBarrier); break;
	case SGEN_PROTOCOL_GLOBAL_REMSET: size = sizeof (SGenProtocolGlobalRemset); break;
	case SGEN_PROTOCOL_PTR_UPDATE: size = sizeof (SGenProtocolPtrUpdate); break;
	case SGEN_PROTOCOL_CLEANUP: size = sizeof (SGenProtocolCleanup); break;
	case SGEN_PROTOCOL_EMPTY: size = sizeof (SGenProtocolEmpty); break;
	case SGEN_PROTOCOL_THREAD_SUSPEND: size = sizeof (SGenProtocolThreadSuspend); break;
	case SGEN_PROTOCOL_THREAD_RESTART: size = sizeof (SGenProtocolThreadRestart); break;
	case SGEN_PROTOCOL_THREAD_REGISTER: size = sizeof (SGenProtocolThreadRegister); break;
	case SGEN_PROTOCOL_THREAD_UNREGISTER: size = sizeof (SGenProtocolThreadUnregister); break;
	case SGEN_PROTOCOL_MISSING_REMSET: size = sizeof (SGenProtocolMissingRemset); break;
	case SGEN_PROTOCOL_CARD_SCAN: size = sizeof (SGenProtocolCardScan); break;
	case SGEN_PROTOCOL_CEMENT: size = sizeof (SGenProtocolCement); break;
	case SGEN_PROTOCOL_CEMENT_RESET: size = 0; break;
	case SGEN_PROTOCOL_DISLINK_UPDATE: size = sizeof (SGenProtocolDislinkUpdate); break;
	case SGEN_PROTOCOL_DISLINK_UPDATE_STAGED: size = sizeof (SGenProtocolDislinkUpdateStaged); break;
	case SGEN_PROTOCOL_DISLINK_PROCESS_STAGED: size = sizeof (SGenProtocolDislinkProcessStaged); break;
	case SGEN_PROTOCOL_DOMAIN_UNLOAD_BEGIN: size = sizeof (SGenProtocolDomainUnload); break;
	case SGEN_PROTOCOL_DOMAIN_UNLOAD_END: size = sizeof (SGenProtocolDomainUnload); break;
	default: assert (0);
	}

	if (size) {
		*data = malloc (size);
		if (fread (*data, size, 1, in) != 1)
			assert (0);
	} else {
		*data = NULL;
	}

	return (int)type;
}

#define WORKER_PREFIX(t)	(WORKER ((t)) ? "w" : " ")

static void
print_entry (int type, void *data)
{
	switch (TYPE (type)) {
	case SGEN_PROTOCOL_COLLECTION_FORCE: {
		SGenProtocolCollectionForce *entry = data;
		printf ("%s collection force generation %d\n", WORKER_PREFIX (type), entry->generation);
		break;
	}
	case SGEN_PROTOCOL_COLLECTION_BEGIN: {
		SGenProtocolCollection *entry = data;
		printf ("%s collection begin %d generation %d\n", WORKER_PREFIX (type), entry->index, entry->generation);
		break;
	}
	case SGEN_PROTOCOL_COLLECTION_END: {
		SGenProtocolCollection *entry = data;
		printf ("%s collection end %d generation %d\n", WORKER_PREFIX (type), entry->index, entry->generation);
		break;
	}
	case SGEN_PROTOCOL_CONCURRENT_START: {
		printf ("%s concurrent start\n", WORKER_PREFIX (type));
		break;
	}
	case SGEN_PROTOCOL_CONCURRENT_UPDATE_FINISH: {
		printf ("%s concurrent update or finish\n", WORKER_PREFIX (type));
		break;
	}
	case SGEN_PROTOCOL_WORLD_STOPPING: {
		SGenProtocolWorldStopping *entry = data;
		printf ("%s world stopping timestamp %lld\n", WORKER_PREFIX (type), entry->timestamp);
		break;
	}
	case SGEN_PROTOCOL_WORLD_STOPPED: {
		SGenProtocolWorldStopped *entry = data;
		long long total = entry->total_major_cards + entry->total_los_cards;
		long long marked = entry->marked_major_cards + entry->marked_los_cards;
		printf ("%s world stopped timestamp %lld total %lld marked %lld %0.2f%%\n", WORKER_PREFIX (type), entry->timestamp, total, marked, 100.0 * (double) marked / (double) total);
		break;
	}
	case SGEN_PROTOCOL_WORLD_RESTARTING: {
		SGenProtocolWorldRestarting *entry = data;
		long long total = entry->total_major_cards + entry->total_los_cards;
		long long marked = entry->marked_major_cards + entry->marked_los_cards;
		printf ("%s world restarting generation %d timestamp %lld total %lld marked %lld %0.2f%%\n", WORKER_PREFIX (type), entry->generation, entry->timestamp, total, marked, 100.0 * (double) marked / (double) total);
		break;
	}
	case SGEN_PROTOCOL_WORLD_RESTARTED: {
		SGenProtocolWorldRestarted *entry = data;
		printf ("%s world restarted generation %d timestamp %lld\n", WORKER_PREFIX (type), entry->generation, entry->timestamp);
		break;
	}
	case SGEN_PROTOCOL_ALLOC: {
		SGenProtocolAlloc *entry = data;
		printf ("%s alloc obj %p vtable %p size %d\n", WORKER_PREFIX (type), entry->obj, entry->vtable, entry->size);
		break;
	}
	case SGEN_PROTOCOL_ALLOC_PINNED: {
		SGenProtocolAlloc *entry = data;
		printf ("%s alloc pinned obj %p vtable %p size %d\n", WORKER_PREFIX (type), entry->obj, entry->vtable, entry->size);
		break;
	}
	case SGEN_PROTOCOL_ALLOC_DEGRADED: {
		SGenProtocolAlloc *entry = data;
		printf ("%s alloc degraded obj %p vtable %p size %d\n", WORKER_PREFIX (type), entry->obj, entry->vtable, entry->size);
		break;
	}
	case SGEN_PROTOCOL_COPY: {
		SGenProtocolCopy *entry = data;
		printf ("%s copy from %p to %p vtable %p size %d\n", WORKER_PREFIX (type), entry->from, entry->to, entry->vtable, entry->size);
		break;
	}
	case SGEN_PROTOCOL_PIN: {
		SGenProtocolPin *entry = data;
		printf ("%s pin obj %p vtable %p size %d\n", WORKER_PREFIX (type), entry->obj, entry->vtable, entry->size);
		break;
	}
	case SGEN_PROTOCOL_MARK: {
		SGenProtocolMark *entry = data;
		printf ("%s mark obj %p vtable %p size %d\n", WORKER_PREFIX (type), entry->obj, entry->vtable, entry->size);
		break;
	}
	case SGEN_PROTOCOL_SCAN_BEGIN: {
		SGenProtocolScanBegin *entry = data;
		printf ("%s scan_begin obj %p vtable %p size %d\n", WORKER_PREFIX (type), entry->obj, entry->vtable, entry->size);
		break;
	}
	case SGEN_PROTOCOL_SCAN_VTYPE_BEGIN: {
		SGenProtocolScanVTypeBegin *entry = data;
		printf ("%s scan_vtype_begin obj %p size %d\n", WORKER_PREFIX (type), entry->obj, entry->size);
		break;
	}
	case SGEN_PROTOCOL_WBARRIER: {
		SGenProtocolWBarrier *entry = data;
		printf ("%s wbarrier ptr %p value %p value_vtable %p\n", WORKER_PREFIX (type), entry->ptr, entry->value, entry->value_vtable);
		break;
	}
	case SGEN_PROTOCOL_GLOBAL_REMSET: {
		SGenProtocolGlobalRemset *entry = data;
		printf ("%s global_remset ptr %p value %p value_vtable %p\n", WORKER_PREFIX (type), entry->ptr, entry->value, entry->value_vtable);
		break;
	}
	case SGEN_PROTOCOL_PTR_UPDATE: {
		SGenProtocolPtrUpdate *entry = data;
		printf ("%s ptr_update ptr %p old_value %p new_value %p vtable %p size %d\n", WORKER_PREFIX (type),
				entry->ptr, entry->old_value, entry->new_value, entry->vtable, entry->size);
		break;
	}
	case SGEN_PROTOCOL_CLEANUP: {
		SGenProtocolCleanup *entry = data;
		printf ("%s cleanup ptr %p vtable %p size %d\n", WORKER_PREFIX (type), entry->ptr, entry->vtable, entry->size);
		break;
	}
	case SGEN_PROTOCOL_EMPTY: {
		SGenProtocolEmpty *entry = data;
		printf ("%s empty start %p size %d\n", WORKER_PREFIX (type), entry->start, entry->size);
		break;
	}
	case SGEN_PROTOCOL_THREAD_SUSPEND: {
		SGenProtocolThreadSuspend *entry = data;
		printf ("%s thread_suspend thread %p ip %p\n", WORKER_PREFIX (type), entry->thread, entry->stopped_ip);
		break;
	}
	case SGEN_PROTOCOL_THREAD_RESTART: {
		SGenProtocolThreadRestart *entry = data;
		printf ("%s thread_restart thread %p\n", WORKER_PREFIX (type), entry->thread);
		break;
	}
	case SGEN_PROTOCOL_THREAD_REGISTER: {
		SGenProtocolThreadRegister *entry = data;
		printf ("%s thread_register thread %p\n", WORKER_PREFIX (type), entry->thread);
		break;
	}
	case SGEN_PROTOCOL_THREAD_UNREGISTER: {
		SGenProtocolThreadUnregister *entry = data;
		printf ("%s thread_unregister thread %p\n", WORKER_PREFIX (type), entry->thread);
		break;
	}
	case SGEN_PROTOCOL_MISSING_REMSET: {
		SGenProtocolMissingRemset *entry = data;
		printf ("%s missing_remset obj %p obj_vtable %p offset %d value %p value_vtable %p value_pinned %d\n", WORKER_PREFIX (type),
				entry->obj, entry->obj_vtable, entry->offset, entry->value, entry->value_vtable, entry->value_pinned);
		break;
	}
	case SGEN_PROTOCOL_CARD_SCAN: {
		SGenProtocolCardScan *entry = data;
		printf ("%s card_scan start %p size %d\n", WORKER_PREFIX (type), entry->start, entry->size);
		break;
	}
	case SGEN_PROTOCOL_CEMENT: {
		SGenProtocolCement *entry = data;
		printf ("%s cement obj %p vtable %p size %d\n", WORKER_PREFIX (type), entry->obj, entry->vtable, entry->size);
		break;
	}
	case SGEN_PROTOCOL_CEMENT_RESET: {
		printf ("%s cement_reset\n", WORKER_PREFIX (type));
		break;
	}
	case SGEN_PROTOCOL_DISLINK_UPDATE: {
		SGenProtocolDislinkUpdate *entry = data;
		printf ("%s dislink_update link %p obj %p staged %d", WORKER_PREFIX (type), entry->link, entry->obj, entry->staged);
		if (entry->obj)
			printf (" track %d\n", entry->track);
		else
			printf ("\n");
		break;
	}
	case SGEN_PROTOCOL_DISLINK_UPDATE_STAGED: {
		SGenProtocolDislinkUpdateStaged *entry = data;
		printf ("%s dislink_update_staged link %p obj %p index %d", WORKER_PREFIX (type), entry->link, entry->obj, entry->index);
		if (entry->obj)
			printf (" track %d\n", entry->track);
		else
			printf ("\n");
		break;
	}
	case SGEN_PROTOCOL_DISLINK_PROCESS_STAGED: {
		SGenProtocolDislinkProcessStaged *entry = data;
		printf ("%s dislink_process_staged link %p obj %p index %d\n", WORKER_PREFIX (type), entry->link, entry->obj, entry->index);
		break;
	}
	case SGEN_PROTOCOL_DOMAIN_UNLOAD_BEGIN: {
		SGenProtocolDomainUnload *entry = data;
		printf ("%s dislink_unload_begin domain %p\n", WORKER_PREFIX (type), entry->domain);
		break;
	}
	case SGEN_PROTOCOL_DOMAIN_UNLOAD_END: {
		SGenProtocolDomainUnload *entry = data;
		printf ("%s dislink_unload_end domain %p\n", WORKER_PREFIX (type), entry->domain);
		break;
	}
	default:
		assert (0);
	}
}

static gboolean
matches_interval (gpointer ptr, gpointer start, int size)
{
	return ptr >= start && (char*)ptr < (char*)start + size;
}

static gboolean
is_match (gpointer ptr, int type, void *data)
{
	switch (TYPE (type)) {
	case SGEN_PROTOCOL_COLLECTION_FORCE:
	case SGEN_PROTOCOL_COLLECTION_BEGIN:
	case SGEN_PROTOCOL_COLLECTION_END:
	case SGEN_PROTOCOL_CONCURRENT_START:
	case SGEN_PROTOCOL_CONCURRENT_UPDATE_FINISH:
	case SGEN_PROTOCOL_WORLD_STOPPING:
	case SGEN_PROTOCOL_WORLD_STOPPED:
	case SGEN_PROTOCOL_WORLD_RESTARTING:
	case SGEN_PROTOCOL_WORLD_RESTARTED:
	case SGEN_PROTOCOL_THREAD_SUSPEND:
	case SGEN_PROTOCOL_THREAD_RESTART:
	case SGEN_PROTOCOL_THREAD_REGISTER:
	case SGEN_PROTOCOL_THREAD_UNREGISTER:
	case SGEN_PROTOCOL_CEMENT_RESET:
	case SGEN_PROTOCOL_DOMAIN_UNLOAD_BEGIN:
	case SGEN_PROTOCOL_DOMAIN_UNLOAD_END:
		return TRUE;
	case SGEN_PROTOCOL_ALLOC:
	case SGEN_PROTOCOL_ALLOC_PINNED:
	case SGEN_PROTOCOL_ALLOC_DEGRADED: {
		SGenProtocolAlloc *entry = data;
		return matches_interval (ptr, entry->obj, entry->size);
	}
	case SGEN_PROTOCOL_COPY: {
		SGenProtocolCopy *entry = data;
		return matches_interval (ptr, entry->from, entry->size) || matches_interval (ptr, entry->to, entry->size);
	}
	case SGEN_PROTOCOL_PIN: {
		SGenProtocolPin *entry = data;
		return matches_interval (ptr, entry->obj, entry->size);
	}
	case SGEN_PROTOCOL_MARK: {
		SGenProtocolMark *entry = data;
		return matches_interval (ptr, entry->obj, entry->size);
	}
	case SGEN_PROTOCOL_SCAN_BEGIN: {
		SGenProtocolScanBegin *entry = data;
		return matches_interval (ptr, entry->obj, entry->size);
	}
	case SGEN_PROTOCOL_SCAN_VTYPE_BEGIN: {
		SGenProtocolScanVTypeBegin *entry = data;
		return matches_interval (ptr, entry->obj, entry->size);
	}
	case SGEN_PROTOCOL_WBARRIER: {
		SGenProtocolWBarrier *entry = data;
		return ptr == entry->ptr || ptr == entry->value;
	}
	case SGEN_PROTOCOL_GLOBAL_REMSET: {
		SGenProtocolGlobalRemset *entry = data;
		return ptr == entry->ptr || ptr == entry->value;
	}
	case SGEN_PROTOCOL_PTR_UPDATE: {
		SGenProtocolPtrUpdate *entry = data;
		return ptr == entry->ptr ||
			matches_interval (ptr, entry->old_value, entry->size) ||
			matches_interval (ptr, entry->new_value, entry->size);
	}
	case SGEN_PROTOCOL_CLEANUP: {
		SGenProtocolCleanup *entry = data;
		return matches_interval (ptr, entry->ptr, entry->size);
	}
	case SGEN_PROTOCOL_EMPTY: {
		SGenProtocolEmpty *entry = data;
		return matches_interval (ptr, entry->start, entry->size);
	}
	case SGEN_PROTOCOL_MISSING_REMSET: {
		SGenProtocolMissingRemset *entry = data;
		return ptr == entry->obj || ptr == entry->value || ptr == (char*)entry->obj + entry->offset;
	}
	case SGEN_PROTOCOL_CARD_SCAN: {
		SGenProtocolCardScan *entry = data;
		return matches_interval (ptr, entry->start, entry->size);
	}
	case SGEN_PROTOCOL_CEMENT: {
		SGenProtocolCement *entry = data;
		return matches_interval (ptr, entry->obj, entry->size);
	}
	case SGEN_PROTOCOL_DISLINK_UPDATE: {
		SGenProtocolDislinkUpdate *entry = data;
		return ptr == entry->obj || ptr == entry->link;
	}
	case SGEN_PROTOCOL_DISLINK_UPDATE_STAGED: {
		SGenProtocolDislinkUpdateStaged *entry = data;
		return ptr == entry->obj || ptr == entry->link;
	}
	case SGEN_PROTOCOL_DISLINK_PROCESS_STAGED: {
		SGenProtocolDislinkProcessStaged *entry = data;
		return ptr == entry->obj || ptr == entry->link;
	}
	default:
		assert (0);
	}
}

static gboolean
is_vtable_match (gpointer ptr, int type, void *data)
{
	switch (TYPE (type)) {
	case SGEN_PROTOCOL_ALLOC:
	case SGEN_PROTOCOL_ALLOC_PINNED:
	case SGEN_PROTOCOL_ALLOC_DEGRADED: {
		SGenProtocolAlloc *entry = data;
		return ptr == entry->vtable;
	}
	case SGEN_PROTOCOL_COPY: {
		SGenProtocolCopy *entry = data;
		return ptr == entry->vtable;
	}
	case SGEN_PROTOCOL_PIN: {
		SGenProtocolPin *entry = data;
		return ptr == entry->vtable;
	}
	case SGEN_PROTOCOL_SCAN_BEGIN: {
		SGenProtocolScanBegin *entry = data;
		return ptr == entry->vtable;
	}
	case SGEN_PROTOCOL_WBARRIER: {
		SGenProtocolWBarrier *entry = data;
		return ptr == entry->value_vtable;
	}
	case SGEN_PROTOCOL_GLOBAL_REMSET: {
		SGenProtocolGlobalRemset *entry = data;
		return ptr == entry->value_vtable;
	}
	case SGEN_PROTOCOL_PTR_UPDATE: {
		SGenProtocolPtrUpdate *entry = data;
		return ptr == entry->vtable;
	}
	case SGEN_PROTOCOL_CLEANUP: {
		SGenProtocolCleanup *entry = data;
		return ptr == entry->vtable;
	}
	case SGEN_PROTOCOL_MISSING_REMSET: {
		SGenProtocolMissingRemset *entry = data;
		return ptr == entry->obj_vtable || ptr == entry->value_vtable;
	}
	case SGEN_PROTOCOL_CEMENT: {
		SGenProtocolCement *entry = data;
		return ptr == entry->vtable;
	}
	default:
		return FALSE;
	}
}

int
main (int argc, char *argv[])
{
	int type;
	void *data;
	int num_args = argc - 1;
	int num_nums = 0;
	int num_vtables = 0;
	int i;
	long nums [num_args];
	long vtables [num_args];
	gboolean dump_all = FALSE;
	gboolean pause_times = FALSE;
	gboolean pause_times_stopped = FALSE;
	gboolean pause_times_concurrent = FALSE;
	long long pause_times_ts = 0;

	for (i = 0; i < num_args; ++i) {
		char *arg = argv [i + 1];
		char *next_arg = argv [i + 2];
		if (!strcmp (arg, "--all")) {
			dump_all = TRUE;
		} else if (!strcmp (arg, "--pause-times")) {
			pause_times = TRUE;
		} else if (!strcmp (arg, "-v") || !strcmp (arg, "--vtable")) {
			vtables [num_vtables++] = strtoul (next_arg, NULL, 16);
			++i;
		} else {
			nums [num_nums++] = strtoul (arg, NULL, 16);
		}
	}

	if (dump_all)
		assert (!pause_times);
	if (pause_times)
		assert (!dump_all);

	while ((type = read_entry (stdin, &data)) != SGEN_PROTOCOL_EOF) {
		if (pause_times) {
			switch (type) {
			case SGEN_PROTOCOL_WORLD_STOPPING: {
				SGenProtocolWorldStopping *entry = data;
				assert (!pause_times_stopped);
				pause_times_concurrent = FALSE;
				pause_times_ts = entry->timestamp;
				pause_times_stopped = TRUE;
				break;
			}
			case SGEN_PROTOCOL_CONCURRENT_START:
			case SGEN_PROTOCOL_CONCURRENT_UPDATE_FINISH:
				pause_times_concurrent = TRUE;
				break;
			case SGEN_PROTOCOL_WORLD_RESTARTED: {
				SGenProtocolWorldRestarted *entry = data;
				assert (pause_times_stopped);
				printf ("pause-time %d %d %lld %lld\n",
						entry->generation,
						pause_times_concurrent,
						entry->timestamp - pause_times_ts,
						pause_times_ts);
				pause_times_stopped = FALSE;
				break;
			}
			}
		} else {
			gboolean match = num_nums == 0 ? is_match (NULL, type, data) : FALSE;
			for (i = 0; i < num_nums; ++i) {
				if (is_match ((gpointer) nums [i], type, data)) {
					match = TRUE;
					break;
				}
			}
			if (!match) {
				for (i = 0; i < num_vtables; ++i) {
					if (is_vtable_match ((gpointer) vtables [i], type, data)) {
						match = TRUE;
						break;
					}
				}
			}
			if (dump_all)
				printf (match ? "* " : "  ");
			if (match || dump_all)
				print_entry (type, data);
		}
		free (data);
	}

	return 0;
}
