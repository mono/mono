/*
 * Copyright 1988, 1989 Hans-J. Boehm, Alan J. Demers
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

#include "private/gc_priv.h"

#if !defined(PLATFORM_MACH_DEP) && !defined(SN_TARGET_PSP2)

#include <stdio.h>

#ifdef AMIGA
# ifndef __GNUC__
#   include <dos.h>
# else
#   include <machine/reg.h>
# endif
#endif

#if defined(MACOS) && defined(__MWERKS__)

#if defined(POWERPC)

# define NONVOLATILE_GPR_COUNT 19
  struct ppc_registers {
        unsigned long gprs[NONVOLATILE_GPR_COUNT];      /* R13-R31 */
  };
  typedef struct ppc_registers ppc_registers;

# if defined(CPPCHECK)
    void getRegisters(ppc_registers* regs);
# else
    asm static void getRegisters(register ppc_registers* regs)
    {
        stmw    r13,regs->gprs                          /* save R13-R31 */
        blr
    }
# endif

  static void PushMacRegisters(void)
  {
        ppc_registers regs;
        int i;
        getRegisters(&regs);
        for (i = 0; i < NONVOLATILE_GPR_COUNT; i++)
                GC_push_one(regs.gprs[i]);
  }

#else /* M68K */

  asm static void PushMacRegisters(void)
  {
    sub.w   #4,sp                   /* reserve space for one parameter */
    move.l  a2,(sp)
    jsr         GC_push_one
    move.l  a3,(sp)
    jsr         GC_push_one
    move.l  a4,(sp)
    jsr         GC_push_one
#   if !__option(a6frames)
      /* <pcb> perhaps a6 should be pushed if stack frames are not being used */
        move.l  a6,(sp)
        jsr             GC_push_one
#   endif
        /* skip a5 (globals), a6 (frame pointer), and a7 (stack pointer) */
    move.l  d2,(sp)
    jsr         GC_push_one
    move.l  d3,(sp)
    jsr         GC_push_one
    move.l  d4,(sp)
    jsr         GC_push_one
    move.l  d5,(sp)
    jsr         GC_push_one
    move.l  d6,(sp)
    jsr         GC_push_one
    move.l  d7,(sp)
    jsr         GC_push_one
    add.w   #4,sp                   /* fix stack */
    rts
  }

#endif /* M68K */

#endif /* MACOS && __MWERKS__ */

# if defined(SPARC) || defined(IA64)
    /* Value returned from register flushing routine; either sp (SPARC) */
    /* or ar.bsp (IA64).                                                */
    GC_INNER ptr_t GC_save_regs_ret_val = NULL;
# endif

/* Routine to mark from registers that are preserved by the C compiler. */
/* This must be ported to every new architecture.  It is not optional,  */
/* and should not be used on platforms that are either UNIX-like, or    */
/* require thread support.                                              */

#undef HAVE_PUSH_REGS

#if defined(USE_ASM_PUSH_REGS)
# define HAVE_PUSH_REGS
#else  /* No asm implementation */

# ifdef STACK_NOT_SCANNED
    void GC_push_regs(void)
    {
      /* empty */
    }
#   define HAVE_PUSH_REGS

# elif defined(M68K) && defined(AMIGA)
    /* This function is not static because it could also be             */
    /* erroneously defined in .S file, so this error would be caught    */
    /* by the linker.                                                   */
    void GC_push_regs(void)
    {
         /*  AMIGA - could be replaced by generic code                  */
         /* a0, a1, d0 and d1 are caller save */

#       ifdef __GNUC__
          asm("subq.w &0x4,%sp");       /* allocate word on top of stack */

          asm("mov.l %a2,(%sp)"); asm("jsr _GC_push_one");
          asm("mov.l %a3,(%sp)"); asm("jsr _GC_push_one");
          asm("mov.l %a4,(%sp)"); asm("jsr _GC_push_one");
          asm("mov.l %a5,(%sp)"); asm("jsr _GC_push_one");
          asm("mov.l %a6,(%sp)"); asm("jsr _GC_push_one");
          /* Skip frame pointer and stack pointer */
          asm("mov.l %d2,(%sp)"); asm("jsr _GC_push_one");
          asm("mov.l %d3,(%sp)"); asm("jsr _GC_push_one");
          asm("mov.l %d4,(%sp)"); asm("jsr _GC_push_one");
          asm("mov.l %d5,(%sp)"); asm("jsr _GC_push_one");
          asm("mov.l %d6,(%sp)"); asm("jsr _GC_push_one");
          asm("mov.l %d7,(%sp)"); asm("jsr _GC_push_one");

          asm("addq.w &0x4,%sp");       /* put stack back where it was  */
#       else /* !__GNUC__ */
          GC_push_one(getreg(REG_A2));
          GC_push_one(getreg(REG_A3));
#         ifndef __SASC
            /* Can probably be changed to #if 0 -Kjetil M. (a4=globals) */
            GC_push_one(getreg(REG_A4));
#         endif
          GC_push_one(getreg(REG_A5));
          GC_push_one(getreg(REG_A6));
          /* Skip stack pointer */
          GC_push_one(getreg(REG_D2));
          GC_push_one(getreg(REG_D3));
          GC_push_one(getreg(REG_D4));
          GC_push_one(getreg(REG_D5));
          GC_push_one(getreg(REG_D6));
          GC_push_one(getreg(REG_D7));
#       endif /* !__GNUC__ */
    }
#   define HAVE_PUSH_REGS

# elif defined(MACOS)

#   if defined(M68K) && defined(THINK_C) && !defined(CPPCHECK)
#     define PushMacReg(reg) \
              move.l  reg,(sp) \
              jsr             GC_push_one
      void GC_push_regs(void)
      {
          asm {
              sub.w   #4,sp          ; reserve space for one parameter.
              PushMacReg(a2);
              PushMacReg(a3);
              PushMacReg(a4);
              ; skip a5 (globals), a6 (frame pointer), and a7 (stack pointer)
              PushMacReg(d2);
              PushMacReg(d3);
              PushMacReg(d4);
              PushMacReg(d5);
              PushMacReg(d6);
              PushMacReg(d7);
              add.w   #4,sp          ; fix stack.
          }
      }
#     define HAVE_PUSH_REGS
#     undef PushMacReg
#   elif defined(__MWERKS__)
      void GC_push_regs(void)
      {
          PushMacRegisters();
      }
#     define HAVE_PUSH_REGS
#   endif /* __MWERKS__ */
# endif /* MACOS */

#endif /* !USE_ASM_PUSH_REGS */

#if defined(HAVE_PUSH_REGS) && defined(THREADS)
# error GC_push_regs cannot be used with threads
 /* Would fail for GC_do_blocking.  There are probably other safety     */
 /* issues.                                                             */
# undef HAVE_PUSH_REGS
#endif

#if !defined(HAVE_PUSH_REGS) && defined(UNIX_LIKE)
# include <signal.h>
# ifndef NO_GETCONTEXT
#   if defined(DARWIN) \
       && (MAC_OS_X_VERSION_MAX_ALLOWED >= 1060 /*MAC_OS_X_VERSION_10_6*/)
#     include <sys/ucontext.h>
#   else
#     include <ucontext.h>
#   endif /* !DARWIN */
#   ifdef GETCONTEXT_FPU_EXCMASK_BUG
#     include <fenv.h>
#   endif
# endif
#endif /* !HAVE_PUSH_REGS */

/* Ensure that either registers are pushed, or callee-save registers    */
/* are somewhere on the stack, and then call fn(arg, ctxt).             */
/* ctxt is either a pointer to a ucontext_t we generated, or NULL.      */
GC_ATTR_NO_SANITIZE_ADDR
GC_INNER void GC_with_callee_saves_pushed(void (*fn)(ptr_t, void *),
                                          volatile ptr_t arg)
{
  volatile int dummy;
  void * volatile context = 0;

# if defined(HAVE_PUSH_REGS)
    GC_push_regs();
# else
#   if defined(UNIX_LIKE) && !defined(NO_GETCONTEXT)
      /* Older versions of Darwin seem to lack getcontext().    */
      /* ARM and MIPS Linux often doesn't support a real        */
      /* getcontext().                                          */
      static signed char getcontext_works = 0; /* (-1) - broken, 1 - works */
      ucontext_t ctxt;
#     ifdef GETCONTEXT_FPU_EXCMASK_BUG
        /* Workaround a bug (clearing the FPU exception mask) in        */
        /* getcontext on Linux/x86_64.                                  */
#       ifdef X86_64
          /* We manipulate FPU control word here just not to force the  */
          /* client application to use -lm linker option.               */
          unsigned short old_fcw;

#         if defined(CPPCHECK)
            GC_noop1((word)&old_fcw);
#         endif
          __asm__ __volatile__ ("fstcw %0" : "=m" (*&old_fcw));
#       else
          int except_mask = fegetexcept();
#       endif
#     endif

      if (getcontext_works >= 0) {
        if (getcontext(&ctxt) < 0) {
          WARN("getcontext failed:"
               " using another register retrieval method...\n", 0);
          /* getcontext() is broken, do not try again.          */
          /* E.g., to workaround a bug in Docker ubuntu_32bit.  */
        } else {
          context = &ctxt;
        }
        if (EXPECT(0 == getcontext_works, FALSE))
          getcontext_works = context != NULL ? 1 : -1;
      }
#     ifdef GETCONTEXT_FPU_EXCMASK_BUG
#       ifdef X86_64
          __asm__ __volatile__ ("fldcw %0" : : "m" (*&old_fcw));
          {
            unsigned mxcsr;
            /* And now correct the exception mask in SSE MXCSR. */
            __asm__ __volatile__ ("stmxcsr %0" : "=m" (*&mxcsr));
            mxcsr = (mxcsr & ~(FE_ALL_EXCEPT << 7)) |
                        ((old_fcw & FE_ALL_EXCEPT) << 7);
            __asm__ __volatile__ ("ldmxcsr %0" : : "m" (*&mxcsr));
          }
#       else /* !X86_64 */
          if (feenableexcept(except_mask) < 0)
            ABORT("feenableexcept failed");
#       endif
#     endif /* GETCONTEXT_FPU_EXCMASK_BUG */
#     if defined(SPARC) || defined(IA64)
        /* On a register window machine, we need to save register       */
        /* contents on the stack for this to work.  This may already be */
        /* subsumed by the getcontext() call.                           */
        GC_save_regs_ret_val = GC_save_regs_in_stack();
#     endif
      if (NULL == context) /* getcontext failed */
#   endif /* !NO_GETCONTEXT */
    {
#     if defined(HAVE_BUILTIN_UNWIND_INIT)
        /* This was suggested by Richard Henderson as the way to        */
        /* force callee-save registers and register windows onto        */
        /* the stack.                                                   */
        __builtin_unwind_init();
#     elif defined(NO_CRT) && defined(MSWIN32)
        CONTEXT ctx;
        RtlCaptureContext(&ctx);
#     else
        /* Generic code                          */
        /* The idea is due to Parag Patel at HP. */
        /* We're not sure whether he would like  */
        /* to be acknowledged for it or not.     */
        jmp_buf regs;
        word * i = (word *)&regs;
        ptr_t lim = (ptr_t)(&regs) + sizeof(regs);

        /* Setjmp doesn't always clear all of the buffer.               */
        /* That tends to preserve garbage.  Clear it.                   */
        for (; (word)i < (word)lim; i++) {
            *i = 0;
        }
#       if defined(MSWIN32) || defined(MSWINCE) || defined(UTS4) \
           || defined(OS2) || defined(CX_UX) || defined(__CC_ARM) \
           || defined(LINUX) || defined(EWS4800) || defined(RTEMS)
          (void) setjmp(regs);
#       else
          (void) _setjmp(regs);
          /* We don't want to mess with signals. According to   */
          /* SUSV3, setjmp() may or may not save signal mask.   */
          /* _setjmp won't, but is less portable.               */
#       endif
#     endif /* !HAVE_BUILTIN_UNWIND_INIT */
    }
# endif /* !HAVE_PUSH_REGS */
  /* FIXME: context here is sometimes just zero.  At the moment the     */
  /* callees don't really need it.                                      */
  fn(arg, context);
  /* Strongly discourage the compiler from treating the above   */
  /* as a tail-call, since that would pop the register          */
  /* contents before we get a chance to look at them.           */
  GC_noop1((word)(&dummy));
}

#endif /* !SN_TARGET_ORBIS && !SN_TARGET_PSP2 */
