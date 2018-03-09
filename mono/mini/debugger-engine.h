/*
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */
#ifndef __MONO_DEBUGGER_ENGINE_H__
#define __MONO_DEBUGGER_ENGINE_H__

typedef struct {
	int log_level;
	char *log_file;
} MonoDebuggerEngineOptions;

void mono_de_init (MonoDebuggerEngineOptions *options);

//Logging functions

void mono_de_log (int level, const char *format, ...);
int mono_de_get_log_level (void);

#endif