/**
 * \file
 * Copyright 2019 Microsoft
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

#include <config.h>
#include <glib.h>
#include "mono/metadata/exception-internals.h"
#include "register-icall-def.h"

// FIXME move to jit-icalls.c
MonoJitICallInfos mono_jit_icall_info = { 0 };


