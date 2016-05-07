/*
 * sgen-archdep.h: Architecture dependent parts of SGen.
 *
 * Copyright 2001-2003 Ximian, Inc
 * Copyright 2003-2010 Novell, Inc.
 * Copyright (C) 2012 Xamarin Inc
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */
#ifndef __MONO_SGENARCHDEP_H__
#define __MONO_SGENARCHDEP_H__

#include <mono/utils/mono-context.h>

#if defined(MONO_CROSS_COMPILE)

#define REDZONE_SIZE	0

#elif defined(TARGET_X86)

#define REDZONE_SIZE	0

#ifndef MONO_ARCH_HAS_MONO_CONTEXT
#error 0
#endif

#elif defined(TARGET_AMD64)

#define REDZONE_SIZE	128

#elif defined(TARGET_POWERPC)

#define REDZONE_SIZE	224

/* MS_BLOCK_SIZE must be a multiple of the system pagesize, which for some
   architectures is 64k.  */
#if defined(TARGET_POWERPC) || defined(TARGET_POWERPC64)
#define ARCH_MIN_MS_BLOCK_SIZE	(64*1024)
#define ARCH_MIN_MS_BLOCK_SIZE_SHIFT	16
#endif

#elif defined(TARGET_ARM)

#define REDZONE_SIZE	0

#elif defined(TARGET_ARM64)

#ifdef __linux__
#define REDZONE_SIZE    0
#elif defined(__APPLE__)
#define REDZONE_SIZE	128
#else
#error "Not implemented."
#endif

#elif defined(__mips__)

#define REDZONE_SIZE	0

#elif defined(__s390x__)

#define REDZONE_SIZE	0

#elif defined(__sparc__)

#define REDZONE_SIZE	0

#endif

#endif /* __MONO_SGENARCHDEP_H__ */
