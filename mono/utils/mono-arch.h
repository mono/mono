/*
 * mono-arch.h: Constants indicating the target architecture
 *
 * Author:
 *      Alex Roenne Petersen (xtzgzorex@gmail.com)
 */

#ifndef _MONO_UTILS_MONO_ARCH_H_
#define _MONO_UTILS_MONO_ARCH_H_

#include <config.h>

/* Architecture constants */

#define MONO_ARCH_X86 		0
#define MONO_ARCH_X86_64	1
#define MONO_ARCH_IA_64		10
#define MONO_ARCH_PPC		20
#define MONO_ARCH_PPC_64	21
#define MONO_ARCH_SPARC		30
#define MONO_ARCH_SPARC_64	31
#define MONO_ARCH_MIPS		40
#define MONO_ARCH_MIPS_64	41
#define MONO_ARCH_S390		50
#define MONO_ARCH_S390X		51
#define MONO_ARCH_ALPHA		60
#define MONO_ARCH_ARM		70
#define MONO_ARCH_HPPA		80

/* Detect the actual architecture */

#if defined(TARGET_AMD64)
# define MONO_ARCH MONO_ARCH_X86
# define MONO_ARCH_IS_X86 1
#elif defined(TARGET_X86)
# define MONO_ARCH MONO_ARCH_X86_64
# define MONO_ARCH_IS_X86 1
#elif defined(sparc64) || defined(__sparc64__)
# define MONO_ARCH MONO_ARCH_SPARC_64
# define MONO_ARCH_IS_SPARC 1
#elif defined(sparc) || defined(__sparc__)
# define MONO_ARCH MONO_ARCH_SPARC
# define MONO_ARCH_IS_SPARC 1
#elif defined(TARGET_S390X)
# if defined(__s390x__)
#  define MONO_ARCH MONO_ARCH_S390X
# else
#  define MONO_ARCH MONO_ARCH_S390
# endif
# define MONO_ARCH_IS_S390 1
#elif defined(TARGET_POWERPC64)
# define MONO_ARCH MONO_ARCH_PPC_64
# define MONO_ARCH_IS_PPC 1
#elif defined(TARGET_POWERPC)
# define MONO_ARCH MONO_ARCH_PPC
# define MONO_ARCH_IS_PPC 1
#elif defined(TARGET_ARM)
# define MONO_ARCH MONO_ARCH_ARM
#elif defined(__ia64__)
# define MONO_ARCH MONO_ARCH_IA_64
#elif defined(__alpha__)
# define MONO_ARCH MONO_ARCH_ALPHA
#elif defined(__mips64__)
# define MONO_ARCH MONO_ARCH_MIPS_64
# define MONO_ARCH_IS_MIPS 1
#elif defined(__mips__)
# define MONO_ARCH MONO_ARCH_MIPS
# define MONO_ARCH_IS_MIPS 1
#elif defined(__hppa__)
# define MONO_ARCH MONO_ARCH_HPPA
#else
# error Mono architecture could not be detected
#endif

#endif  /* _MONO_UTILS_MONO_ARCH_H_ */
