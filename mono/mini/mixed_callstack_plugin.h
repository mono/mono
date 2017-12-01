#ifndef __MIXED_CALLSTACK_PLUGIN_H__
#define __MIXED_CALLSTACK_PLUGIN_H__

#include "config.h"
#include "mini.h"

void mixed_callstack_plugin_init (const char *options);

void mixed_callstack_plugin_save_method_info (MonoCompile *cfg);

void mixed_callstack_plugin_save_trampoline_info (MonoTrampInfo *info);

void mixed_callstack_plugin_remove_method (MonoDomain *domain, MonoMethod *method, MonoJitDynamicMethodInfo *info);

void mixed_callstack_plugin_save_specific_trampoline_info (gpointer arg1, MonoTrampolineType tramp_type, MonoDomain *domain, gpointer code, guint32 code_len);

void mixed_callstack_plugin_on_domain_unload_end();
#endif
