/*
 * mono-log-common.c: Platform-independent interface to the logger
 *
 * This module contains the POSIX syslog logger interface
 *
 * Author:
 *    Neale Ferguson <neale@sinenomine.net>
 *
 */
#include <config.h>

#ifdef HAVE_UNISTD_H
#include <unistd.h>
#endif

#include <stdlib.h>
#include <stdio.h>
#include <ctype.h>
#include <string.h>
#include <glib.h>
#include <errno.h>
#include <time.h>
#ifndef HOST_WIN32
#include <sys/time.h>
#else
#include <process.h>
#endif
#include "mono-logger-internals.h"

static FILE *logFile = NULL;
static void *logUserData = NULL;

/**
 * mapSyslogLevel:
 * 	
 * 	@level - GLogLevelFlags value
 * 	@returns The equivalent character identifier
 */
static inline char 
mapLogFileLevel(GLogLevelFlags level) 
{
	if (level & G_LOG_LEVEL_ERROR)
		return ('E');
	if (level & G_LOG_LEVEL_CRITICAL)
		return ('C');
	if (level & G_LOG_LEVEL_WARNING)
		return ('W');
	if (level & G_LOG_LEVEL_MESSAGE)
		return ('N');
	if (level & G_LOG_LEVEL_INFO)
		return ('I');
	if (level & G_LOG_LEVEL_DEBUG)
		return ('D');
	return ('I');
}

/**
 * mono_log_open_logfile
 * 	
 *	Open the logfile. If the path is not specified default to stdout. If the
 *	open fails issue a warning and use stdout as the log file destination.
 *
 * 	@path - Path for log file
 * 	@userData - Not used
 */
void
mono_log_open_logfile(const char *path, void *userData)
{
	if (path == NULL) {
		logFile = stdout;
	} else {
#ifndef HOST_WIN32
		logFile = fopen(path, "w");
#else
		gunichar2 *wPath = g_utf8_to_utf16(path, -1, 0, 0, 0);
		if (wPath != NULL) {
			logFile = _wfopen((wchar_t *) wPath, L"w");
			g_free (wPath);
		}
#endif
		if (logFile == NULL) {
			g_warning("opening of log file %s failed with %s - defaulting to stdout", 
				  path, strerror(errno));
			logFile = stdout;
		}
	}
	logUserData = userData;
}

/**
 * mono_log_write_logfile
 * 	
 * 	Write data to the log file.
 *
 * 	@domain - Identifier string
 * 	@level - Logging level flags
 * 	@format - Printf format string
 * 	@vargs - Variable argument list
 */
void
mono_log_write_logfile (const char *log_domain, GLogLevelFlags level, mono_bool hdr, const char *message)
{
	time_t t;

	if (logFile == NULL)
		logFile = stdout;

	if (hdr) {
		pid_t pid;
		char logTime [80];

#ifndef HOST_WIN32
		struct tm tod;
		time(&t);
		localtime_r(&t, &tod);
		pid = getpid();
		strftime(logTime, sizeof(logTime), "%Y-%m-%d %H:%M:%S", &tod);
#else
		struct tm *tod;
		time(&t);
		tod = localtime(&t);
		pid = _getpid();
		strftime(logTime, sizeof(logTime), "%F %T", tod);
#endif
		fprintf (logFile, "%s level[%c] mono[%d]: %s\n", logTime, mapLogFileLevel (level), pid, message);
	} else {
		fprintf (logFile, "%s%s%s\n",
			log_domain != NULL ? log_domain : "",
			log_domain != NULL ? ": " : "",
			message);
	}

	fflush(logFile);

	if (level & G_LOG_LEVEL_ERROR)
		abort();
}

/**
 * mono_log_close_logfile
 *
 * 	Close the log file
 */
void
mono_log_close_logfile()
{
	if (logFile) {
		if (logFile != stdout)
			fclose(logFile);
		logFile = NULL;
	}
}
