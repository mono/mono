#ifndef __MONO_PMIP_MY_CALLSTACK_H__
#define __MONO_PMIP_MY_CALLSTACK_H__

#include "config.h"
#include "mini.h"

void mono_pmip_my_callstack_init (const char *options);

void mono_pmip_my_callstack_save_method_info (MonoCompile *cfg);

void mono_pmip_my_callstack_save_trampoline_info (MonoTrampInfo *info);

void mono_pmip_my_callstack_remove_method (MonoDomain *domain, MonoMethod *method, MonoJitDynamicMethodInfo *info);

void mono_pmip_my_callstack_save_specific_trampoline_info (gpointer arg1, MonoTrampolineType tramp_type, MonoDomain *domain, gpointer code, guint32 code_len);

#endif
