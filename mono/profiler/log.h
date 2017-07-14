#ifndef __MONO_PROFLOG_H__
#define __MONO_PROFLOG_H__

#include <glib.h>
#define MONO_PROFILER_UNSTABLE_GC_ROOTS
#include <mono/metadata/profiler.h>

#define BUF_ID 0x4D504C01
#define LOG_HEADER_ID 0x4D505A01
#define LOG_VERSION_MAJOR 2
#define LOG_VERSION_MINOR 0
#define LOG_DATA_VERSION 14

/*
 * Changes in major/minor versions:
 * version 1.0: removed sysid field from header
 *              added args, arch, os fields to header
 *
 * Changes in data versions:
 * version 2: added offsets in heap walk
 * version 3: added GC roots
 * version 4: added sample/statistical profiling
 * version 5: added counters sampling
 * version 6: added optional backtrace in sampling info
 * version 8: added TYPE_RUNTIME and JIT helpers/trampolines
 * version 9: added MONO_PROFILER_CODE_BUFFER_EXCEPTION_HANDLING
 * version 10: added TYPE_COVERAGE
 * version 11: added thread ID to TYPE_SAMPLE_HIT
               added more load/unload events
                   unload for class
                   unload for image
                   load/unload for appdomain
                   load/unload for contexts
                   load/unload/name for assemblies
               removed TYPE_LOAD_ERR flag (profiler never generated it, now removed from the format itself)
               added TYPE_GC_HANDLE_{CREATED,DESTROYED}_BT
               TYPE_JIT events are no longer guaranteed to have code start/size info (can be zero)
 * version 12: added MONO_COUNTER_PROFILER
 * version 13: added MONO_GC_EVENT_{PRE_STOP_WORLD_LOCKED,POST_START_WORLD_UNLOCKED}
               added TYPE_META + TYPE_SYNC_POINT
               removed il and native offset in TYPE_SAMPLE_HIT
               methods in backtraces are now encoded as proper method pointers
               removed flags in backtrace format
               removed flags in metadata events
               changed the following fields to a single byte rather than leb128
                   TYPE_GC_EVENT: event_type, generation
                   TYPE_HEAP_ROOT: root_type
                   TYPE_JITHELPER: type
                   TYPE_SAMPLE_HIT: sample_type
                   TYPE_CLAUSE: clause_type
                   TYPE_SAMPLE_COUNTERS_DESC: type, unit, variance
                   TYPE_SAMPLE_COUNTERS: type
               added time fields to all events that were missing one
                   TYPE_HEAP_OBJECT
                   TYPE_HEAP_ROOT
                   TYPE_SAMPLE_USYM
                   TYPE_SAMPLE_COUNTERS_DESC
                   TYPE_COVERAGE_METHOD
                   TYPE_COVERAGE_STATEMENT
                   TYPE_COVERAGE_CLASS
                   TYPE_COVERAGE_ASSEMBLY
               moved the time field in TYPE_SAMPLE_HIT to right after the event byte, now encoded as a regular time field
               changed the time field in TYPE_SAMPLE_COUNTERS to be encoded as a regular time field (in nanoseconds)
               added TYPE_GC_FINALIZE_{START,END,OBJECT_START,OBJECT_END}
 * version 14: added event field to TYPE_MONITOR instead of encoding it in the extended info
               all TYPE_MONITOR events can now contain backtraces
               changed address field in TYPE_SAMPLE_UBIN to be based on ptr_base
               added an image pointer field to assembly load events
               added an exception object field to TYPE_CLAUSE
               class unload events no longer exist (they were never emitted)
               removed type field from TYPE_SAMPLE_HIT
 */

enum {
	TYPE_ALLOC,
	TYPE_GC,
	TYPE_METADATA,
	TYPE_METHOD,
	TYPE_EXCEPTION,
	TYPE_MONITOR,
	TYPE_HEAP,
	TYPE_SAMPLE,
	TYPE_RUNTIME,
	TYPE_COVERAGE,
	TYPE_META,
	/* extended type for TYPE_HEAP */
	TYPE_HEAP_START  = 0 << 4,
	TYPE_HEAP_END    = 1 << 4,
	TYPE_HEAP_OBJECT = 2 << 4,
	TYPE_HEAP_ROOT   = 3 << 4,
	/* extended type for TYPE_METADATA */
	TYPE_END_LOAD     = 2 << 4,
	TYPE_END_UNLOAD   = 4 << 4,
	/* extended type for TYPE_GC */
	TYPE_GC_EVENT  = 1 << 4,
	TYPE_GC_RESIZE = 2 << 4,
	TYPE_GC_MOVE   = 3 << 4,
	TYPE_GC_HANDLE_CREATED      = 4 << 4,
	TYPE_GC_HANDLE_DESTROYED    = 5 << 4,
	TYPE_GC_HANDLE_CREATED_BT   = 6 << 4,
	TYPE_GC_HANDLE_DESTROYED_BT = 7 << 4,
	TYPE_GC_FINALIZE_START = 8 << 4,
	TYPE_GC_FINALIZE_END = 9 << 4,
	TYPE_GC_FINALIZE_OBJECT_START = 10 << 4,
	TYPE_GC_FINALIZE_OBJECT_END = 11 << 4,
	/* extended type for TYPE_METHOD */
	TYPE_LEAVE     = 1 << 4,
	TYPE_ENTER     = 2 << 4,
	TYPE_EXC_LEAVE = 3 << 4,
	TYPE_JIT       = 4 << 4,
	/* extended type for TYPE_EXCEPTION */
	TYPE_THROW_NO_BT = 0 << 7,
	TYPE_THROW_BT    = 1 << 7,
	TYPE_CLAUSE      = 1 << 4,
	/* extended type for TYPE_ALLOC */
	TYPE_ALLOC_NO_BT  = 0 << 4,
	TYPE_ALLOC_BT     = 1 << 4,
	/* extended type for TYPE_MONITOR */
	TYPE_MONITOR_NO_BT  = 0 << 7,
	TYPE_MONITOR_BT     = 1 << 7,
	/* extended type for TYPE_SAMPLE */
	TYPE_SAMPLE_HIT           = 0 << 4,
	TYPE_SAMPLE_USYM          = 1 << 4,
	TYPE_SAMPLE_UBIN          = 2 << 4,
	TYPE_SAMPLE_COUNTERS_DESC = 3 << 4,
	TYPE_SAMPLE_COUNTERS      = 4 << 4,
	/* extended type for TYPE_RUNTIME */
	TYPE_JITHELPER = 1 << 4,
	/* extended type for TYPE_COVERAGE */
	TYPE_COVERAGE_ASSEMBLY = 0 << 4,
	TYPE_COVERAGE_METHOD   = 1 << 4,
	TYPE_COVERAGE_STATEMENT = 2 << 4,
	TYPE_COVERAGE_CLASS = 3 << 4,
	/* extended type for TYPE_META */
	TYPE_SYNC_POINT = 0 << 4,
};

enum {
	/* metadata type byte for TYPE_METADATA */
	TYPE_CLASS    = 1,
	TYPE_IMAGE    = 2,
	TYPE_ASSEMBLY = 3,
	TYPE_DOMAIN   = 4,
	TYPE_THREAD   = 5,
	TYPE_CONTEXT  = 6,
};

typedef enum {
	SYNC_POINT_PERIODIC = 0,
	SYNC_POINT_WORLD_STOP = 1,
	SYNC_POINT_WORLD_START = 2,
} MonoProfilerSyncPointType;

typedef enum {
	MONO_PROFILER_MONITOR_CONTENTION = 1,
	MONO_PROFILER_MONITOR_DONE = 2,
	MONO_PROFILER_MONITOR_FAIL = 3,
} MonoProfilerMonitorEvent;

enum {
	MONO_PROFILER_GC_HANDLE_CREATED = 0,
	MONO_PROFILER_GC_HANDLE_DESTROYED = 1,
};

typedef enum {
	MONO_PROFILER_HEAPSHOT_NONE = 0,
	MONO_PROFILER_HEAPSHOT_MAJOR = 1,
	MONO_PROFILER_HEAPSHOT_ON_DEMAND = 2,
	MONO_PROFILER_HEAPSHOT_X_GC = 3,
	MONO_PROFILER_HEAPSHOT_X_MS = 4,
} MonoProfilerHeapshotMode;

// If you alter MAX_FRAMES, you may need to alter SAMPLE_BLOCK_SIZE too.
#define MAX_FRAMES 32

//The following flags control emitting individual events
#define PROFLOG_EXCEPTION_EVENTS (1 << 0)
#define PROFLOG_MONITOR_EVENTS (1 << 1)
#define PROFLOG_GC_EVENTS (1 << 2)
#define PROFLOG_GC_ALLOCATION_EVENTS (1 << 3)
#define PROFLOG_GC_MOVE_EVENTS (1 << 4)
#define PROFLOG_GC_ROOT_EVENTS (1 << 5)
#define PROFLOG_GC_HANDLE_EVENTS (1 << 6)
#define PROFLOG_FINALIZATION_EVENTS (1 << 7)
#define PROFLOG_COUNTER_EVENTS (1 << 8)
#define PROFLOG_SAMPLE_EVENTS (1 << 9)
#define PROFLOG_JIT_EVENTS (1 << 10)

#define PROFLOG_ALLOC_ALIAS (PROFLOG_GC_EVENTS | PROFLOG_GC_ALLOCATION_EVENTS | PROFLOG_GC_MOVE_EVENTS)
#define PROFLOG_HEAPSHOT_ALIAS (PROFLOG_GC_EVENTS | PROFLOG_GC_ROOT_EVENTS)
#define PROFLOG_LEGACY_ALIAS (PROFLOG_EXCEPTION_EVENTS | PROFLOG_MONITOR_EVENTS | PROFLOG_GC_EVENTS | PROFLOG_GC_MOVE_EVENTS | PROFLOG_GC_ROOT_EVENTS | PROFLOG_GC_HANDLE_EVENTS | PROFLOG_FINALIZATION_EVENTS | PROFLOG_COUNTER_EVENTS)

typedef struct {
	//Events explicitly enabled
	int enable_mask;

	//Events explicitly disabled
	int disable_mask;

	// Actual mask the profiler should use. Can be changed at runtime.
	int effective_mask;

	// Whether to do method prologue/epilogue instrumentation. Only used at startup.
	gboolean enter_leave;

	// Whether to collect code coverage by instrumenting basic blocks.
	gboolean collect_coverage;

	//Emit a report at the end of execution
	gboolean do_report;

	//Enable profiler internal debugging
	gboolean do_debug;

	//Where to compress the output file
	gboolean use_zip;

	// Heapshot mode (every major, on demand, XXgc, XXms). Can be changed at runtime.
	MonoProfilerHeapshotMode hs_mode;

	// Heapshot frequency in milliseconds (for MONO_HEAPSHOT_X_MS). Can be changed at runtime.
	unsigned int hs_freq_ms;

	// Heapshot frequency in number of collections (for MONO_HEAPSHOT_X_GC). Can be changed at runtime.
	unsigned int hs_freq_gc;

	// Sample frequency in Hertz. Only used at startup.
	int sample_freq;

	// Maximum number of frames to collect. Can be changed at runtime.
	int num_frames;

	// Max depth to record enter/leave events. Can be changed at runtime.
	int max_call_depth;

	//Name of the generated mlpd file
	const char *output_filename;

	//Filter files used by the code coverage mode
	GPtrArray *cov_filter_files;

	// Port to listen for profiling commands (e.g. "heapshot" for on-demand heapshot).
	int command_port;

	// Maximum number of SampleHit structures. We'll drop samples if this number is not sufficient.
	int max_allocated_sample_hits;

	// Sample mode. Only used at startup.
	MonoProfilerSampleMode sampling_mode;
} ProfilerConfig;

void proflog_parse_args (ProfilerConfig *config, const char *desc);

#endif /* __MONO_PROFLOG_H__ */
