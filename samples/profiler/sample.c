#include <mono/metadata/profiler.h>

/*
 * Bare bones profiler. Compile with:
 *
 * linux : gcc -fPIC -shared -o libmono-profiler-sample.so sample.c `pkg-config --cflags --libs mono-2`
 * mac : gcc -o mono-profiler-sample.dylib sample.c -lz `pkg-config --cflags mono-2` -undefined suppress -flat_namespace
 * linux with a custom prefix (e.g. --prefix=/opt/my-mono-build):
 *	gcc -fPIC -shared -o libmono-profiler-sample.so sample.c `PKG_CONFIG_PATH=/opt/my-mono-build/lib/pkgconfig/ pkg-config --cflags --libs mono-2`
 *
 * Install the binary where the dynamic loader can find it. eg /usr/lib etc.
 * For a custom prefix build, <prefix>/lib would also work.
 * Then run mono with:
 * mono --profile=sample your_application.exe
 *
 * Note if you name a profiler with more than 8 characters (eg sample6789) appears to not work
 */

struct _MonoProfiler {
	unsigned long long ncalls;
};

static MonoProfiler prof_instance;

/* called at the end of the program */
static void
sample_shutdown (MonoProfiler *prof)
{
	printf("total number of calls: %llu\n", prof->ncalls);
}

static void
sample_method_enter (MonoProfiler *prof, MonoMethod *method, MonoProfilerCallContext *ctx)
{
	prof->ncalls++;
}

static MonoProfilerCallInstrumentationFlags
sample_instrumentation_filter (MonoProfiler *prof, MonoMethod *method)
{
	return MONO_PROFILER_CALL_INSTRUMENTATION_ENTER;
}

/* the entry point */
void
mono_profiler_init_sample (const char *desc)
{
	MonoProfiler *prof = &prof_instance;

	MonoProfilerHandle handle = mono_profiler_create (prof);
	mono_profiler_set_runtime_shutdown_end_callback (handle, sample_shutdown);
	mono_profiler_set_call_instrumentation_filter_callback (handle, sample_instrumentation_filter);
	mono_profiler_set_method_enter_callback (handle, sample_method_enter);
}


