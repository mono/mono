/**
 * \file
 * Copyright 2018 Microsoft
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

#include <config.h>
#include <glib.h>
#include "mono/metadata/exception-internals.h"

#define MONO_REGISTER_JIT_ICALL(x) MonoJitICallInfo x ## _icall_info = { 0 };
#include "register-icall-def.h"
