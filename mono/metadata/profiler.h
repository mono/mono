#ifndef __MONO_PROFILER_H__
#define __MONO_PROFILER_H__

#include <mono/metadata/object.h>
#include <mono/metadata/appdomain.h>

MONO_BEGIN_DECLS

typedef struct _MonoProfiler MonoProfiler;
typedef struct _MonoProfilerDesc MonoProfilerDesc;

#define MONO_PROFILER_MAX_STAT_CALL_CHAIN_DEPTH 128

typedef enum {
	MONO_PROFILE_NONE = 0,
	MONO_PROFILE_APPDOMAIN_EVENTS = 1 << 0,
	MONO_PROFILE_ASSEMBLY_EVENTS  = 1 << 1,
	MONO_PROFILE_MODULE_EVENTS    = 1 << 2,
	MONO_PROFILE_CLASS_EVENTS     = 1 << 3,
	MONO_PROFILE_JIT_COMPILATION  = 1 << 4,
	MONO_PROFILE_INLINING         = 1 << 5, // Currently does nothing.
	MONO_PROFILE_EXCEPTIONS       = 1 << 6,
	MONO_PROFILE_ALLOCATIONS      = 1 << 7,
	MONO_PROFILE_GC               = 1 << 8,
	MONO_PROFILE_THREADS          = 1 << 9,
	MONO_PROFILE_REMOTING         = 1 << 10,
	MONO_PROFILE_TRANSITIONS      = 1 << 11, // Currently does nothing.
	/*
	 * NOTE: While all event flags here can be toggled dynamically at runtime,
	 * the following flags will not have retroactive effect.
	 *
	 * MONO_PROFILE_ENTER_LEAVE
	 * MONO_PROFILE_COVERAGE
	 * MONO_PROFILE_INS_COVERAGE
	 *
	 * Methods which have already been JIT'd with or without instrumentation
	 * won't be re-JIT'd. Toggling these events only affects subsequent
	 * JIT'ing.
	 */
	MONO_PROFILE_ENTER_LEAVE      = 1 << 12,
	/*
	 * NOTE: The runtime currently does not support using the two coverage
	 * modes at the same time, whether it is from a single profiler or from two
	 * separate profilers. You are free to switch between one or the other at
	 * runtime, but both cannot be set at the same time.
	 *
	 * It is generally recommended to use MONO_PROFILE_INS_COVERAGE as it
	 * provides more accurate data and is available on all platforms.
	 */
	MONO_PROFILE_COVERAGE         = 1 << 13, // Coverage on a basic block basis.
	MONO_PROFILE_INS_COVERAGE     = 1 << 14, // Coverage on an instruction basis.
	MONO_PROFILE_STATISTICAL      = 1 << 15,
	MONO_PROFILE_METHOD_EVENTS    = 1 << 16,
	MONO_PROFILE_MONITOR_EVENTS   = 1 << 17,
	MONO_PROFILE_IOMAP_EVENTS     = 1 << 18,
	MONO_PROFILE_GC_MOVES         = 1 << 19,
	MONO_PROFILE_GC_ROOTS         = 1 << 20,
	MONO_PROFILE_CONTEXT_EVENTS   = 1 << 21,
	MONO_PROFILE_GC_FINALIZATION  = 1 << 22,
	MONO_PROFILE_GC_HANDLES       = 1 << 23
} MonoProfileFlags;

typedef enum {
	MONO_PROFILE_OK,
	MONO_PROFILE_FAILED
} MonoProfileResult;

// Keep somewhat in sync with libgc/include/gc.h:enum GC_EventType
typedef enum {
	MONO_GC_EVENT_START,
	MONO_GC_EVENT_MARK_START,
	MONO_GC_EVENT_MARK_END,
	MONO_GC_EVENT_RECLAIM_START,
	MONO_GC_EVENT_RECLAIM_END,
	MONO_GC_EVENT_END,
	/*
	 * This is the actual arrival order of the following events:
	 *
	 * MONO_GC_EVENT_PRE_STOP_WORLD
	 * MONO_GC_EVENT_PRE_STOP_WORLD_LOCKED
	 * MONO_GC_EVENT_POST_STOP_WORLD
	 * MONO_GC_EVENT_PRE_START_WORLD
	 * MONO_GC_EVENT_POST_START_WORLD_UNLOCKED
	 * MONO_GC_EVENT_POST_START_WORLD
	 *
	 * The LOCKED and UNLOCKED events guarantee that, by the time they arrive,
	 * the GC and suspend locks will both have been acquired and released,
	 * respectively.
	 */
	MONO_GC_EVENT_PRE_STOP_WORLD,
	MONO_GC_EVENT_POST_STOP_WORLD,
	MONO_GC_EVENT_PRE_START_WORLD,
	MONO_GC_EVENT_POST_START_WORLD,
	MONO_GC_EVENT_PRE_STOP_WORLD_LOCKED,
	MONO_GC_EVENT_POST_START_WORLD_UNLOCKED
} MonoGCEvent;

/* coverage info */
typedef struct {
	MonoMethod *method;
	int iloffset;
	int counter;
	const char *filename;
	int line;
	int col;
} MonoProfileCoverageEntry;

/* executable code buffer info */
typedef enum {
	MONO_PROFILER_CODE_BUFFER_UNKNOWN,
	MONO_PROFILER_CODE_BUFFER_METHOD,
	MONO_PROFILER_CODE_BUFFER_METHOD_TRAMPOLINE,
	MONO_PROFILER_CODE_BUFFER_UNBOX_TRAMPOLINE,
	MONO_PROFILER_CODE_BUFFER_IMT_TRAMPOLINE,
	MONO_PROFILER_CODE_BUFFER_GENERICS_TRAMPOLINE,
	MONO_PROFILER_CODE_BUFFER_SPECIFIC_TRAMPOLINE,
	MONO_PROFILER_CODE_BUFFER_HELPER,
	MONO_PROFILER_CODE_BUFFER_MONITOR,
	MONO_PROFILER_CODE_BUFFER_DELEGATE_INVOKE,
	MONO_PROFILER_CODE_BUFFER_EXCEPTION_HANDLING,
	MONO_PROFILER_CODE_BUFFER_LAST
} MonoProfilerCodeBufferType;

typedef enum {
	MONO_PROFILER_MONITOR_CONTENTION = 1,
	MONO_PROFILER_MONITOR_DONE = 2,
	MONO_PROFILER_MONITOR_FAIL = 3
} MonoProfilerMonitorEvent;

typedef enum {
	MONO_PROFILER_CALL_CHAIN_NONE = 0,
	MONO_PROFILER_CALL_CHAIN_NATIVE = 1,
	MONO_PROFILER_CALL_CHAIN_GLIBC = 2,
	MONO_PROFILER_CALL_CHAIN_MANAGED = 3,
	MONO_PROFILER_CALL_CHAIN_INVALID = 4
} MonoProfilerCallChainStrategy;

typedef enum {
	MONO_PROFILER_GC_HANDLE_CREATED,
	MONO_PROFILER_GC_HANDLE_DESTROYED
} MonoProfileGCHandleEvent;

typedef enum {
	MONO_PROFILE_GC_ROOT_PINNING  = 1 << 8,
	MONO_PROFILE_GC_ROOT_WEAKREF  = 2 << 8,
	MONO_PROFILE_GC_ROOT_INTERIOR = 4 << 8,
	/* the above are flags, the type is in the low 2 bytes */
	MONO_PROFILE_GC_ROOT_STACK = 0,
	MONO_PROFILE_GC_ROOT_FINALIZER = 1,
	MONO_PROFILE_GC_ROOT_HANDLE = 2,
	MONO_PROFILE_GC_ROOT_OTHER = 3,
	MONO_PROFILE_GC_ROOT_MISC = 4, /* could be stack, handle, etc. */
	MONO_PROFILE_GC_ROOT_TYPEMASK = 0xff
} MonoProfileGCRootType;

typedef enum {
	/* Elapsed time is tracked by user+kernel time of the process - this is the default*/
	MONO_PROFILER_STAT_MODE_PROCESS = 0,
	/* Elapsed time is tracked by wallclock time */
	MONO_PROFILER_STAT_MODE_REAL = 1,
} MonoProfileSamplingMode;

/*
 * Functions that the runtime will call on the profiler.
 */

typedef void (*MonoProfileFunc) (MonoProfiler *prof);

typedef void (*MonoProfileAppDomainFunc) (MonoProfiler *prof, MonoDomain   *domain);
typedef void (*MonoProfileContextFunc)   (MonoProfiler *prof, MonoAppContext *context);
typedef void (*MonoProfileMethodFunc)   (MonoProfiler *prof, MonoMethod   *method);
typedef void (*MonoProfileClassFunc)    (MonoProfiler *prof, MonoClass    *klass);
typedef void (*MonoProfileModuleFunc)   (MonoProfiler *prof, MonoImage    *module);
typedef void (*MonoProfileAssemblyFunc) (MonoProfiler *prof, MonoAssembly *assembly);
typedef void (*MonoProfileMonitorFunc)  (MonoProfiler *prof, MonoObject *obj, MonoProfilerMonitorEvent event);

typedef void (*MonoProfileExceptionFunc) (MonoProfiler *prof, MonoObject *object);
typedef void (*MonoProfileExceptionClauseFunc) (MonoProfiler *prof, MonoMethod *method, int clause_type, int clause_num);

typedef void (*MonoProfileAppDomainResult)(MonoProfiler *prof, MonoDomain   *domain,   int result);
typedef void (*MonoProfileAppDomainFriendlyNameFunc) (MonoProfiler *prof, MonoDomain *domain, const char *name);
typedef void (*MonoProfileMethodResult)   (MonoProfiler *prof, MonoMethod   *method,   int result);
typedef void (*MonoProfileJitResult)      (MonoProfiler *prof, MonoMethod   *method,   MonoJitInfo* jinfo,   int result);
typedef void (*MonoProfileClassResult)    (MonoProfiler *prof, MonoClass    *klass,    int result);
typedef void (*MonoProfileModuleResult)   (MonoProfiler *prof, MonoImage    *module,   int result);
typedef void (*MonoProfileAssemblyResult) (MonoProfiler *prof, MonoAssembly *assembly, int result);

typedef void (*MonoProfileMethodInline)   (MonoProfiler *prof, MonoMethod   *parent, MonoMethod *child, int *ok);

typedef void (*MonoProfileThreadFunc)     (MonoProfiler *prof, uintptr_t tid);
typedef void (*MonoProfileThreadNameFunc) (MonoProfiler *prof, uintptr_t tid, const char *name);
typedef void (*MonoProfileAllocFunc)      (MonoProfiler *prof, MonoObject *obj, MonoClass *klass);
typedef void (*MonoProfileStatFunc)       (MonoProfiler *prof, mono_byte *ip, void *context);
typedef void (*MonoProfileStatCallChainFunc) (MonoProfiler *prof, int call_chain_depth, mono_byte **ip, void *context);
typedef void (*MonoProfileGCFunc)         (MonoProfiler *prof, MonoGCEvent event, int generation);
typedef void (*MonoProfileGCMoveFunc)     (MonoProfiler *prof, void **objects, int num);
typedef void (*MonoProfileGCResizeFunc)   (MonoProfiler *prof, int64_t new_size);
typedef void (*MonoProfileGCHandleFunc)   (MonoProfiler *prof, int op, int type, uintptr_t handle, MonoObject *obj);
typedef void (*MonoProfileGCRootFunc)     (MonoProfiler *prof, int num_roots, void **objects, int *root_types, uintptr_t *extra_info);

typedef void (*MonoProfileGCFinalizeFunc)  (MonoProfiler *prof);
typedef void (*MonoProfileGCFinalizeObjectFunc) (MonoProfiler *prof, MonoObject *obj);

typedef void (*MonoProfileIomapFunc) (MonoProfiler *prof, const char *report, const char *pathname, const char *new_pathname);

typedef mono_bool (*MonoProfileCoverageFilterFunc)   (MonoProfiler *prof, MonoMethod *method);

typedef void (*MonoProfileCoverageFunc)   (MonoProfiler *prof, const MonoProfileCoverageEntry *entry);

typedef void (*MonoProfilerCodeChunkNew) (MonoProfiler *prof, void* chunk, int size);
typedef void (*MonoProfilerCodeChunkDestroy) (MonoProfiler *prof, void* chunk);
typedef void (*MonoProfilerCodeBufferNew) (MonoProfiler *prof, void* buffer, int size, MonoProfilerCodeBufferType type, void *data);

MONO_API void mono_profiler_load (const char *desc);

/*
 * Functions the profiler module may call.
 */

MONO_API MonoProfilerDesc *mono_profiler_new (MonoProfiler *prof, MonoProfileFunc shutdown_callback);

MONO_API MONO_RT_EXTERNAL_ONLY MonoProfileFlags mono_profiler_get_events (void);
MONO_API MonoProfileFlags mono_profiler_get_event_flags (MonoProfilerDesc *desc);
MONO_API void mono_profiler_set_event_flags (MonoProfilerDesc *desc, MonoProfileFlags events);

MONO_API void mono_profiler_set_appdomain_cb (MonoProfilerDesc *desc, MonoProfileAppDomainFunc start_load, MonoProfileAppDomainResult end_load,
                                              MonoProfileAppDomainFunc start_unload, MonoProfileAppDomainFunc end_unload);
MONO_API void mono_profiler_set_appdomain_name_cb (MonoProfilerDesc *desc, MonoProfileAppDomainFriendlyNameFunc domain_name_cb);
MONO_API void mono_profiler_set_context_cb (MonoProfilerDesc *desc, MonoProfileContextFunc load, MonoProfileContextFunc unload);
MONO_API void mono_profiler_set_assembly_cb (MonoProfilerDesc *desc, MonoProfileAssemblyFunc start_load, MonoProfileAssemblyResult end_load,
                                             MonoProfileAssemblyFunc start_unload, MonoProfileAssemblyFunc end_unload);
MONO_API void mono_profiler_set_module_cb (MonoProfilerDesc *desc, MonoProfileModuleFunc start_load, MonoProfileModuleResult end_load,
                                           MonoProfileModuleFunc start_unload, MonoProfileModuleFunc end_unload);
MONO_API void mono_profiler_set_class_cb (MonoProfilerDesc *desc, MonoProfileClassFunc start_load, MonoProfileClassResult end_load,
                                          MonoProfileClassFunc start_unload, MonoProfileClassFunc end_unload);
MONO_API void mono_profiler_set_jit_compile_cb (MonoProfilerDesc *desc, MonoProfileMethodFunc start, MonoProfileMethodResult end);
MONO_API void mono_profiler_set_jit_end_cb (MonoProfilerDesc *desc, MonoProfileJitResult end);
MONO_API void mono_profiler_set_method_free_cb (MonoProfilerDesc *desc, MonoProfileMethodFunc callback);
MONO_API void mono_profiler_set_method_invoke_cb (MonoProfilerDesc *desc, MonoProfileMethodFunc start, MonoProfileMethodFunc end);
MONO_API void mono_profiler_set_enter_leave_cb (MonoProfilerDesc *desc, MonoProfileMethodFunc enter, MonoProfileMethodFunc fleave);
MONO_API void mono_profiler_set_thread_cb (MonoProfilerDesc *desc, MonoProfileThreadFunc start, MonoProfileThreadFunc end);
MONO_API void mono_profiler_set_thread_name_cb (MonoProfilerDesc *desc, MonoProfileThreadNameFunc thread_name_cb);
MONO_API void mono_profiler_set_transition_cb (MonoProfilerDesc *desc, MonoProfileMethodResult callback);
MONO_API void mono_profiler_set_allocation_cb (MonoProfilerDesc *desc, MonoProfileAllocFunc callback);
MONO_API void mono_profiler_set_monitor_cb (MonoProfilerDesc *desc, MonoProfileMonitorFunc callback);
MONO_API void mono_profiler_set_statistical_cb (MonoProfilerDesc *desc, MonoProfileStatFunc callback);
MONO_API void mono_profiler_set_exception_cb (MonoProfilerDesc *desc, MonoProfileExceptionFunc throw_callback, MonoProfileMethodFunc exc_method_leave,
                                              MonoProfileExceptionClauseFunc clause_callback);
MONO_API void mono_profiler_set_coverage_filter_cb (MonoProfilerDesc *desc, MonoProfileCoverageFilterFunc callback);
MONO_API void mono_profiler_set_gc_cb (MonoProfilerDesc *desc, MonoProfileGCFunc callback, MonoProfileGCResizeFunc heap_resize_callback);
MONO_API void mono_profiler_set_gc_moves_cb (MonoProfilerDesc *desc, MonoProfileGCMoveFunc callback);
MONO_API void mono_profiler_set_gc_roots_cb (MonoProfilerDesc *desc, MonoProfileGCHandleFunc handle_callback, MonoProfileGCRootFunc roots_callback);
MONO_API void mono_profiler_set_gc_finalize_cb (MonoProfilerDesc *desc, MonoProfileGCFinalizeFunc begin, MonoProfileGCFinalizeObjectFunc begin_obj,
                                                MonoProfileGCFinalizeObjectFunc end_obj, MonoProfileGCFinalizeFunc end);
MONO_API void mono_profiler_set_runtime_initialized_cb (MonoProfilerDesc *desc, MonoProfileFunc runtime_initialized_callback);
MONO_API void mono_profiler_set_code_chunk_new_cb (MonoProfilerDesc *desc, MonoProfilerCodeChunkNew callback);
MONO_API void mono_profiler_set_code_chunk_destroy_cb (MonoProfilerDesc *desc, MonoProfilerCodeChunkDestroy callback);
MONO_API void mono_profiler_set_code_buffer_new_cb (MonoProfilerDesc *desc, MonoProfilerCodeBufferNew callback);
MONO_API void mono_profiler_set_iomap_cb (MonoProfilerDesc *desc, MonoProfileIomapFunc callback);

MONO_API void mono_profiler_coverage_get (MonoProfiler *prof, MonoMethod *method, MonoProfileCoverageFunc func);

MONO_API void mono_profiler_set_statistical_mode (MonoProfileSamplingMode mode, int64_t sampling_frequency_hz);

/*
 * These APIs have been deprecated in favor of the new ones that require a
 * specific MonoProfilerDesc pointer as an argument.
 */

MONO_DEPRECATED void mono_profiler_install (MonoProfiler *prof, MonoProfileFunc shutdown_callback);
MONO_DEPRECATED void mono_profiler_set_events (MonoProfileFlags events);
MONO_DEPRECATED void mono_profiler_install_appdomain (MonoProfileAppDomainFunc start_load, MonoProfileAppDomainResult end_load,
                                                      MonoProfileAppDomainFunc start_unload, MonoProfileAppDomainFunc end_unload);
MONO_DEPRECATED void mono_profiler_install_appdomain_name (MonoProfileAppDomainFriendlyNameFunc domain_name_cb);
MONO_DEPRECATED void mono_profiler_install_context (MonoProfileContextFunc load, MonoProfileContextFunc unload);
MONO_DEPRECATED void mono_profiler_install_assembly (MonoProfileAssemblyFunc start_load, MonoProfileAssemblyResult end_load,
                                                     MonoProfileAssemblyFunc start_unload, MonoProfileAssemblyFunc end_unload);
MONO_DEPRECATED void mono_profiler_install_module (MonoProfileModuleFunc start_load, MonoProfileModuleResult end_load,
                                                   MonoProfileModuleFunc start_unload, MonoProfileModuleFunc end_unload);
MONO_DEPRECATED void mono_profiler_install_class (MonoProfileClassFunc start_load, MonoProfileClassResult end_load,
                                                  MonoProfileClassFunc start_unload, MonoProfileClassFunc end_unload);
MONO_DEPRECATED void mono_profiler_install_jit_compile (MonoProfileMethodFunc start, MonoProfileMethodResult end);
MONO_DEPRECATED void mono_profiler_install_jit_end (MonoProfileJitResult end);
MONO_DEPRECATED void mono_profiler_install_method_free (MonoProfileMethodFunc callback);
MONO_DEPRECATED void mono_profiler_install_method_invoke (MonoProfileMethodFunc start, MonoProfileMethodFunc end);
MONO_DEPRECATED void mono_profiler_install_enter_leave (MonoProfileMethodFunc enter, MonoProfileMethodFunc fleave);
MONO_DEPRECATED void mono_profiler_install_thread (MonoProfileThreadFunc start, MonoProfileThreadFunc end);
MONO_DEPRECATED void mono_profiler_install_thread_name (MonoProfileThreadNameFunc thread_name_cb);
MONO_DEPRECATED void mono_profiler_install_transition (MonoProfileMethodResult callback);
MONO_DEPRECATED void mono_profiler_install_allocation (MonoProfileAllocFunc callback);
MONO_DEPRECATED void mono_profiler_install_monitor (MonoProfileMonitorFunc callback);
MONO_DEPRECATED void mono_profiler_install_statistical (MonoProfileStatFunc callback);
MONO_DEPRECATED void mono_profiler_install_statistical_call_chain (MonoProfileStatCallChainFunc callback, int call_chain_depth,
                                                                   MonoProfilerCallChainStrategy call_chain_strategy);
MONO_DEPRECATED void mono_profiler_install_exception (MonoProfileExceptionFunc throw_callback, MonoProfileMethodFunc exc_method_leave,
                                                      MonoProfileExceptionClauseFunc clause_callback);
MONO_DEPRECATED void mono_profiler_install_coverage_filter (MonoProfileCoverageFilterFunc callback);
MONO_DEPRECATED void mono_profiler_install_gc (MonoProfileGCFunc callback, MonoProfileGCResizeFunc heap_resize_callback);
MONO_DEPRECATED void mono_profiler_install_gc_moves (MonoProfileGCMoveFunc callback);
MONO_DEPRECATED void mono_profiler_install_gc_roots (MonoProfileGCHandleFunc handle_callback, MonoProfileGCRootFunc roots_callback);
MONO_DEPRECATED void mono_profiler_install_gc_finalize (MonoProfileGCFinalizeFunc begin, MonoProfileGCFinalizeObjectFunc begin_obj,
                                                        MonoProfileGCFinalizeObjectFunc end_obj, MonoProfileGCFinalizeFunc end);
MONO_DEPRECATED void mono_profiler_install_runtime_initialized (MonoProfileFunc runtime_initialized_callback);
MONO_DEPRECATED void mono_profiler_install_code_chunk_new (MonoProfilerCodeChunkNew callback);
MONO_DEPRECATED void mono_profiler_install_code_chunk_destroy (MonoProfilerCodeChunkDestroy callback);
MONO_DEPRECATED void mono_profiler_install_code_buffer_new (MonoProfilerCodeBufferNew callback);
MONO_DEPRECATED void mono_profiler_install_iomap (MonoProfileIomapFunc callback);

MONO_END_DECLS

#endif /* __MONO_PROFILER_H__ */

