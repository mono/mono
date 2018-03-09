#include <config.h>

#include "mini.h"
#include "debugger-engine.h"

#if (!defined (MONO_ARCH_SOFT_DEBUG_SUPPORTED) || defined (DISABLE_SOFT_DEBUG)) && !defined(MONO_ARCH_WASM_DEBUG_SUPPORTED)
#define DISABLE_DEBUGGER_ENGINE 1
#endif

#ifndef DISABLE_DEBUGGER_ENGINE

static int log_level;
static FILE* log_file;


void
mono_de_init (MonoDebuggerEngineOptions *options)
{
	log_level = options->log_level;

	if (options->log_file) {
		log_file = fopen (options->log_file, "w+");
		if (!log_file) {
			fprintf (stderr, "Unable to create log file '%s': %s.\n", options->log_file, strerror (errno));
			exit (1);
		}
	} else {
		log_file = stdout;
	}
}

void
mono_de_log (int level, const char *format, ...)
{
	va_list args;
	va_start (args, format);

#ifdef HOST_ANDROID
	char *msg = NULL;
	if (g_vasprintf (&msg, format, args) >= 0)
		g_print ("%s", msg);
	g_free (msg);
#else
	vfprintf (log_file, format, args);
	fflush (log_file);
#endif

	va_end (args);
}

int
mono_de_get_log_level (void)
{
	return log_level;
}

#else

void
mono_de_init (MonoDebuggerEngineOptions *options)
{
	g_error ("Debugger engine disabled");
}

void
mono_de_log (int level, const char *format, ...)
{
	g_error ("Debugger engine disabled");
}
int
mono_de_get_log_level (void)
{
	return 0;
}

#endif

