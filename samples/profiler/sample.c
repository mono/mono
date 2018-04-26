/*
 * Bare bones profiler showing how a profiler module should be structured.
 *
 * Compilation:
 * - Linux: gcc -fPIC -shared -o libmono-profiler-sample.so sample.c `pkg-config --cflags mono-2`
 * - OS X: clang -undefined suppress -flat_namespace -o mono-profiler-sample.dylib sample.c `pkg-config --cflags mono-2`
 *
 * If you're using a custom prefix for your Mono installation (e.g. /opt/mono),
 * pkg-config must be invoked like this: PKG_CONFIG_PATH=/opt/mono pkg-config --cflags mono-2
 *
 * Install the resulting shared library where the dynamic loader can find it,
 * e.g. /usr/local/lib or /opt/mono (custom prefix).
 *
 * To use the module: mono --profile=sample hello.exe
 */

#include <mono/metadata/profiler.h>

/*
 * Defining a type called _MonoProfiler will complete the opaque MonoProfiler
 * type, which is used throughout the profiler API.
 */
struct _MonoProfiler {
	/* Handle obtained from mono_profiler_create (). */
	MonoProfilerHandle handle;

	/* Counts the number of calls observed. */
	unsigned long long ncalls;
};

/*
 * Use static storage for the profiler structure for simplicity. The structure
 * can be allocated dynamically as well, if needed.
 */
static MonoProfiler profiler;

/*
 * Callback invoked after the runtime finishes shutting down. Managed code can
 * no longer run and most runtime services are unavailable.
 */
static void
sample_shutdown_end (MonoProfiler *prof)
{
	printf ("Total number of calls: %llu\n", prof->ncalls);
}

/*
 * Method enter callback invoked on entry to all instrumented methods.
 */
static void
sample_method_enter (MonoProfiler *prof, MonoMethod *method, MonoProfilerCallContext *ctx)
{
	prof->ncalls++;
}

/*
 * Filter callback that decides which methods to instrument and how.
 */
static MonoProfilerCallInstrumentationFlags
sample_call_instrumentation_filter (MonoProfiler *prof, MonoMethod *method)
{
	return MONO_PROFILER_CALL_INSTRUMENTATION_ENTER;
}

/*
 * The entry point function invoked by the Mono runtime.
 */
void
mono_profiler_init_sample (const char *desc)
{
	profiler.handle = mono_profiler_create (&profiler);

	mono_profiler_set_runtime_shutdown_end_callback (profiler.handle, sample_shutdown_end);
	mono_profiler_set_call_instrumentation_filter_callback (profiler.handle, sample_call_instrumentation_filter);
	mono_profiler_set_method_enter_callback (profiler.handle, sample_method_enter);
}
