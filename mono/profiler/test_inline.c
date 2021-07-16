/*
 * test_inline.c: A test profiler for Mono.
 *
 * Copyright 2017 vFunction, Inc.
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full
 * license information.
 */

#include <glib.h>
#include <mono/metadata/loader.h>
#include <mono/metadata/profiler.h>

struct _MonoProfiler {
	char **parsed_params;
	char *method_name;
	int expected_ncalls;
	uint64_t ncalls;
};

/* called at the end of the program */
static void testinline_shutdown(MonoProfiler *prof)
{
	if (prof->ncalls != (uint64_t)prof->expected_ncalls) {
		g_printf("Expected %d calls, got %" PRIu64 "\n",
			 prof->expected_ncalls, prof->ncalls);
		exit(1);
	}
}

static void testinline_method_enter(MonoProfiler *prof,
				    MonoMethod *method,
				    MonoProfilerCallContext *ctx)
{
	prof->ncalls++;
}

static MonoProfilerCallInstrumentationFlags
testinline_instrumentation_filter(MonoProfiler *prof, MonoMethod *method)
{
	if (strcmp(mono_method_get_name(method), prof->method_name) == 0)
		return MONO_PROFILER_CALL_INSTRUMENTATION_ENTER;
	return 0;
}

/* the entry point */
void mono_profiler_init_testinline(const char *desc);

void mono_profiler_init_testinline(const char *desc)
{
	MonoProfiler *prof = g_new0(MonoProfiler, 1);
	prof->parsed_params = g_strsplit(desc, ":", -1);
	if (prof->parsed_params[0] == NULL || prof->parsed_params[1] == NULL ||
	    prof->parsed_params[2] == NULL) {
		g_printf("usage: "
			 "--profile=testinline:<method-to-profile>:<expected-"
			 "ncalls>\n");
		exit(1);
	}
	prof->method_name = prof->parsed_params[1];
	prof->expected_ncalls = atoi(prof->parsed_params[2]);

	MonoProfilerHandle handle = mono_profiler_create(prof);
	mono_profiler_set_runtime_shutdown_end_callback(handle,
							testinline_shutdown);
	mono_profiler_set_call_instrumentation_filter_callback(
	    handle, testinline_instrumentation_filter);
	mono_profiler_set_method_enter_callback(handle,
						testinline_method_enter);
}
