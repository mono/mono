/**
 * \file
 * Remoting support
 *
 * (C) 2014 Xamarin, Inc.  http://www.xamarin.com
 *
 */

#ifndef __MONO_REMOTING_H__
#define __MONO_REMOTING_H__

void mono_remoting_init (void);

#ifndef DISABLE_REMOTING

#include "config.h"
#include <mono/metadata/class.h>
#include <mono/metadata/object-internals.h>
#include <mono/metadata/class-internals.h>

MonoMethod *
mono_marshal_get_remoting_invoke (MonoMethod *method, MonoError *error);

MonoMethod *
mono_marshal_get_xappdomain_invoke (MonoMethod *method, MonoError *error);

MonoMethod *
mono_marshal_get_remoting_invoke_for_target (MonoMethod *method, MonoRemotingTarget target_type, MonoError *error);

MonoMethod *
mono_marshal_get_remoting_invoke_with_check (MonoMethod *method, MonoError *error);

MonoMethod *
mono_marshal_get_stfld_wrapper (MonoType *type);

MonoMethod *
mono_marshal_get_ldfld_wrapper (MonoType *type);

MonoMethod *
mono_marshal_get_ldflda_wrapper (MonoType *type);

MonoMethod *
mono_marshal_get_proxy_cancast (MonoClass *klass);

#endif
#endif
