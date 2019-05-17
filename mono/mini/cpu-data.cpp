/**
 * \file
 *   Data describing a CPU.
 *
 * Author:
 *   Jay Krell (jaykrell@microsoft.com)
 *
 * Copyright 2019 Microsoft
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

// FIXME all architectures or at least the configured one

#include "mini.h"

#undef x86_inc_reg
#undef x86_dec_reg
#undef x86_push_membase
#undef x86_lea_membase
#undef amd64_lea_membase
#undef x86_inc_membase
#undef x86_push_imm
#undef x86_dec_membase

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wreorder"

#define break break_
#define throw throw_
#define switch switch_
#define abs abs_

#define A 'A'
#define a 'a'
#define b 'b'
#define c 'c'
#define f 'f'
#define i 'i'
#define m 'm'
#define x 'x'
#define y 'y'

constexpr MonoInstSpecs::MonoInstSpecs() :
#include "cpu-amd64.md"
{ }

#pragma GCC diagnostic pop

extern "C"
{
extern constexpr MonoInstSpecs mono_amd64_desc {};
}

// for debugging
typedef MonoInstSpec::src1_t src1;
typedef MonoInstSpec::src2_t src2;
typedef MonoInstSpec::src3_t src3;
typedef MonoInstSpec::dest_t dest;
typedef MonoInstSpec::clob_t clob;
typedef MonoInstSpec::len_t len;
MonoInstSpec
#include "cpu-amd64.md"
;
