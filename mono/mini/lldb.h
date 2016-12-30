#ifndef __MONO_XDEBUG_LLDB_H__
#define __MONO_XDEBUG_LLDB_H__

#include "config.h"
#include "mini.h"

void mono_lldb_init (const char *options);

void mono_lldb_save_method_info (MonoCompile *cfg);

void mono_lldb_save_trampoline_info (MonoTrampInfo *info);

#endif
