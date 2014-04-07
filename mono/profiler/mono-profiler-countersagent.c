/*
 * mono-profiler-countersagent.c: Counters agent profiler for Mono.
 *
 * Authors:
 *   Ludovic Henry <ludovic.henry@xamarin.com>
 *
 * Note: this profiler is based on utils/mono-counters-agent and it's
 * sole purpose is to expose this agent as a profiler for easier use.
 */
#include <mono/metadata/profiler.h>
#include <mono/utils/mono-counters-agent.h>
#include <glib.h>

struct _MonoProfiler {
	char* desc;
};

void on_runtime_initialized (MonoProfiler *prof);
void on_runtime_shutdown (MonoProfiler *prof);
void mono_profiler_startup (const char *desc);

void
on_runtime_initialized (MonoProfiler *prof)
{
	char **split = g_strsplit (prof->desc, ":", 2);

	mono_counters_agent_start (split [1]);

	g_strfreev (split);
}

void
on_runtime_shutdown (MonoProfiler *prof)
{
	mono_counters_agent_stop ();
}

void
mono_profiler_startup (const char *desc)
{
	MonoProfiler *prof = g_new0 (MonoProfiler, 1);

	prof->desc = g_strdup (desc);

	mono_profiler_install (prof, on_runtime_shutdown);
	mono_profiler_install_runtime_initialized (on_runtime_initialized);
}
