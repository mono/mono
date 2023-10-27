/*
 * Copyright (c) 1991-1994 by Xerox Corporation.  All rights reserved.
 *
 * THIS MATERIAL IS PROVIDED AS IS, WITH ABSOLUTELY NO WARRANTY EXPRESSED
 * OR IMPLIED.  ANY USE IS AT YOUR OWN RISK.
 *
 * Permission is hereby granted to use or copy this program
 * for any purpose,  provided the above notices are retained on all copies.
 * Permission to modify the code and to distribute modified code is granted,
 * provided the above notices are retained, and a notice that the code was
 * modified is included with the above copyright notice.
 */

/* Check whether setjmp actually saves registers in jmp_buf. */
/* If it doesn't, the generic mark_regs code won't work.     */
/* Compilers vary as to whether they will put x in a         */
/* (callee-save) register without -O.  The code is           */
/* contrived such that any decent compiler should put x in   */
/* a callee-save register with -O.  Thus it is               */
/* recommended that this be run optimized.  (If the machine  */
/* has no callee-save registers, then the generic code is    */
/* safe, but this will not be noticed by this piece of       */
/* code.)  This test appears to be far from perfect.         */
#include <stdio.h>
#include <setjmp.h>
#include <string.h>
#include "private/gc_priv.h"

#ifdef OS2
/* GETPAGESIZE() is set to getpagesize() by default, but that   */
/* doesn't really exist, and the collector doesn't need it.     */
#define INCL_DOSFILEMGR
#define INCL_DOSMISC
#define INCL_DOSERRORS
#include <os2.h>

int getpagesize(void)
{
    ULONG result[1];

    if (DosQuerySysInfo(QSV_PAGE_SIZE, QSV_PAGE_SIZE,
                        (void *)result, sizeof(ULONG)) != NO_ERROR) {
        fprintf(stderr, "DosQuerySysInfo failed\n");
        result[0] = 4096;
    }
    return((int)(result[0]));
}
#elif defined(MSWIN32) || defined(MSWINCE) || defined(CYGWIN32)
# include <windows.h>
  int getpagesize(void)
  {
    SYSTEM_INFO sysinfo;
    GetSystemInfo(&sysinfo);
    return sysinfo.dwPageSize;
  }
#endif

struct a_s {
  char a_a;
  char * a_b;
} a;

word nested_sp(void)
{
# if GC_GNUC_PREREQ(4, 0)
    return (word)__builtin_frame_address(0);
# else
    volatile word sp;
    sp = (word)(&sp);
    return sp;
# endif
}

/* To prevent nested_sp inlining. */
word (*volatile nested_sp_fn)(void) = nested_sp;

int g(int x);

int main(void)
{
    volatile word sp;
    unsigned ps = GETPAGESIZE();
    JMP_BUF b;
    register int x = (int)strlen("a");  /* 1, slightly disguised */
    static volatile int y = 0;

    sp = (word)(&sp);
    printf("This appears to be a %s running %s\n", MACH_TYPE, OS_TYPE);
#   if defined(CPPCHECK)
      (void)nested_sp(); /* to workaround a bug in cppcheck */
#   endif
    if (nested_sp_fn() < sp) {
      printf("Stack appears to grow down, which is the default.\n");
      printf("A good guess for STACKBOTTOM on this machine is 0x%lx.\n",
             ((unsigned long)sp + ps) & ~(ps-1));
    } else {
      printf("Stack appears to grow up.\n");
      printf("Define STACK_GROWS_UP in gc_private.h\n");
      printf("A good guess for STACKBOTTOM on this machine is 0x%lx.\n",
             ((unsigned long)sp + ps) & ~(ps-1));
    }
    printf("Note that this may vary between machines of ostensibly\n");
    printf("the same architecture (e.g. Sun 3/50s and 3/80s).\n");
    printf("On many machines the value is not fixed.\n");
    printf("A good guess for ALIGNMENT on this machine is %lu.\n",
           (unsigned long)((word)(&(a.a_b)) - (word)(&a)));

    printf("The following is a very dubious test of one root marking"
           " strategy.\n");
    printf("Results may not be accurate/useful:\n");
    /* Encourage the compiler to keep x in a callee-save register */
    x = 2*x-1;
    printf("\n");
    x = 2*x-1;
    (void)SETJMP(b);
    if (y == 1) {
      if (x == 2) {
        printf("Setjmp-based generic mark_regs code probably won't work.\n");
        printf("But we rarely try that anymore.  If you have getcontect()\n");
        printf("this probably doesn't matter.\n");
      } else if (x == 1) {
          printf("Setjmp-based register marking code may work.\n");
      } else {
          printf("Very strange setjmp implementation.\n");
      }
    }
    y++;
    x = 2;
    if (y == 1) LONGJMP(b, 1);
    printf("Some GC internal configuration stuff: \n");
    printf("\tWORDSZ = %lu, ALIGNMENT = %d, GC_GRANULE_BYTES = %d\n",
           (unsigned long)WORDSZ, ALIGNMENT, GC_GRANULE_BYTES);
    printf("\tUsing one mark ");
#   if defined(USE_MARK_BYTES)
      printf("byte");
#   else
      printf("bit");
#   endif
    printf(" per ");
#   if defined(MARK_BIT_PER_OBJ)
      printf("object.\n");
#   elif defined(MARK_BIT_PER_GRANULE)
      printf("granule.\n");
#   endif
#   ifdef THREAD_LOCAL_ALLOC
      printf("Thread local allocation enabled.\n");
#   endif
#   ifdef PARALLEL_MARK
      printf("Parallel marking enabled.\n");
#   endif
    (void)g(x);
    return(0);
}

int g(int x)
{
    return(x);
}
