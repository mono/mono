#ifndef __MONO_DEBUGGER_AGENT_H__
#define __MONO_DEBUGGER_AGENT_H__

#include "mini.h"

MONO_API void
mono_debugger_agent_parse_options (char *options);

void
mono_debugger_agent_init (void);

#ifdef IL2CPP_MONO_DEBUGGER
void
mono_debugger_run_debugger_thread_func(void* arg);
#endif // IL2CPP_MONO_DEBUGGER

void
mono_debugger_agent_breakpoint_hit (void *sigctx);

void
mono_debugger_agent_single_step_event (void *sigctx);

void
#ifndef IL2CPP_MONO_DEBUGGER
debugger_agent_single_step_from_context (MonoContext *ctx);
#else
debugger_agent_single_step_from_context (MonoContext *ctx, uint64_t sequencePointId);
#endif

void
debugger_agent_breakpoint_from_context (MonoContext *ctx);

void
mono_debugger_agent_free_domain_info (MonoDomain *domain);

#if defined(PLATFORM_ANDROID) || defined(TARGET_ANDROID)
void
mono_debugger_agent_unhandled_exception (MonoException *exc);
#endif

void
mono_debugger_agent_handle_exception (MonoException *ext, MonoContext *throw_ctx, MonoContext *catch_ctx);

void
mono_debugger_agent_begin_exception_filter (MonoException *exc, MonoContext *ctx, MonoContext *orig_ctx);

void
mono_debugger_agent_end_exception_filter (MonoException *exc, MonoContext *ctx, MonoContext *orig_ctx);

void
mono_debugger_agent_user_break (void);

void
mono_debugger_agent_debug_log (int level, MonoString *category, MonoString *message);

gboolean
mono_debugger_agent_debug_log_is_enabled (void);

MONO_API gboolean
mono_debugger_agent_transport_handshake (void);

#endif
