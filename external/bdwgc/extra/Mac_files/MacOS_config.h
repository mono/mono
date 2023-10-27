/*
        MacOS_config.h

        Configuration flags for Macintosh development systems.

        <Revision History>

        11/16/95  pcb  Updated compilation flags to reflect latest 4.6 Makefile.

        by Patrick C. Beard.
 */
/* Boehm, November 17, 1995 12:10 pm PST */

#ifdef __MWERKS__
/* for CodeWarrior Pro with Metrowerks Standard Library (MSL). */
/* #define MSL_USE_PRECOMPILED_HEADERS 0 */
#include <ansi_prefix.mac.h>
#endif /* __MWERKS__ */

/* these are defined again in gc_priv.h. */
#undef TRUE
#undef FALSE

#define ALL_INTERIOR_POINTERS   /* follows interior pointers. */
/* #define DONT_ADD_BYTE_AT_END */    /* no padding. */
/* #define SMALL_CONFIG */           /* whether to use a smaller heap. */
#define USE_TEMPORARY_MEMORY    /* use Macintosh temporary memory. */
