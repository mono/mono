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
/* Boehm, November 17, 1995 12:13 pm PST */
# include "private/gc_priv.h"
# include <stdio.h>
# include <setjmp.h>
# if defined(OS2) || defined(CX_UX)
#   define _setjmp(b) setjmp(b)
#   define _longjmp(b,v) longjmp(b,v)
# endif
# ifdef AMIGA
#   ifndef __GNUC__
#     include <dos.h>
#   else
#     include <machine/reg.h>
#   endif
# endif

#if defined(__MWERKS__) && !defined(POWERPC)

asm static void PushMacRegisters()
{
    sub.w   #4,sp                   // reserve space for one parameter.
    move.l  a2,(sp)
    jsr		GC_push_one
    move.l  a3,(sp)
    jsr		GC_push_one
    move.l  a4,(sp)
    jsr		GC_push_one
#   if !__option(a6frames)
	// <pcb> perhaps a6 should be pushed if stack frames are not being used.    
  	move.l	a6,(sp)
  	jsr		GC_push_one
#   endif
	// skip a5 (globals), a6 (frame pointer), and a7 (stack pointer)
    move.l  d2,(sp)
    jsr		GC_push_one
    move.l  d3,(sp)
    jsr		GC_push_one
    move.l  d4,(sp)
    jsr		GC_push_one
    move.l  d5,(sp)
    jsr		GC_push_one
    move.l  d6,(sp)
    jsr		GC_push_one
    move.l  d7,(sp)
    jsr		GC_push_one
    add.w   #4,sp                   // fix stack.
    rts
}

#endif /* __MWERKS__ */

# if defined(SPARC) || defined(IA64)
    /* Value returned from register flushing routine; either sp (SPARC) */
    /* or ar.bsp (IA64)							*/
    word GC_save_regs_ret_val;
# endif

/* Routine to mark from registers that are preserved by the C compiler. */
/* This must be ported to every new architecture.  There is a generic   */
/* version at the end, that is likely, but not guaranteed to work       */
/* on your architecture.  Run the test_setjmp program to see whether    */
/* there is any chance it will work.                                    */

#if !defined(USE_GENERIC_PUSH_REGS) && !defined(USE_ASM_PUSH_REGS)
#undef HAVE_PUSH_REGS
void GC_push_regs()
{
#       ifdef RT
	  register long TMP_SP; /* must be bound to r11 */
#       endif

#       ifdef VAX
	  /* VAX - generic code below does not work under 4.2 */
	  /* r1 through r5 are caller save, and therefore     */
	  /* on the stack or dead.                            */
	  asm("pushl r11");     asm("calls $1,_GC_push_one");
	  asm("pushl r10"); 	asm("calls $1,_GC_push_one");
	  asm("pushl r9");	asm("calls $1,_GC_push_one");
	  asm("pushl r8");	asm("calls $1,_GC_push_one");
	  asm("pushl r7");	asm("calls $1,_GC_push_one");
	  asm("pushl r6");	asm("calls $1,_GC_push_one");
#	  define HAVE_PUSH_REGS
#       endif
#       if defined(M68K) && (defined(SUNOS4) || defined(NEXT))
	/*  M68K SUNOS - could be replaced by generic code */
	  /* a0, a1 and d1 are caller save          */
	  /*  and therefore are on stack or dead.   */
	
	  asm("subqw #0x4,sp");		/* allocate word on top of stack */

	  asm("movl a2,sp@");	asm("jbsr _GC_push_one");
	  asm("movl a3,sp@");	asm("jbsr _GC_push_one");
	  asm("movl a4,sp@");	asm("jbsr _GC_push_one");
	  asm("movl a5,sp@");	asm("jbsr _GC_push_one");
	  /* Skip frame pointer and stack pointer */
	  asm("movl d1,sp@");	asm("jbsr _GC_push_one");
	  asm("movl d2,sp@");	asm("jbsr _GC_push_one");
	  asm("movl d3,sp@");	asm("jbsr _GC_push_one");
	  asm("movl d4,sp@");	asm("jbsr _GC_push_one");
	  asm("movl d5,sp@");	asm("jbsr _GC_push_one");
	  asm("movl d6,sp@");	asm("jbsr _GC_push_one");
	  asm("movl d7,sp@");	asm("jbsr _GC_push_one");

	  asm("addqw #0x4,sp");		/* put stack back where it was	*/
#	  define HAVE_PUSH_REGS
#       endif

#       if defined(M68K) && defined(HP)
	/*  M68K HP - could be replaced by generic code */
	  /* a0, a1 and d1 are caller save.  */
	
	  asm("subq.w &0x4,%sp");	/* allocate word on top of stack */

	  asm("mov.l %a2,(%sp)"); asm("jsr _GC_push_one");
	  asm("mov.l %a3,(%sp)"); asm("jsr _GC_push_one");
	  asm("mov.l %a4,(%sp)"); asm("jsr _GC_push_one");
	  asm("mov.l %a5,(%sp)"); asm("jsr _GC_push_one");
	  /* Skip frame pointer and stack pointer */
	  asm("mov.l %d1,(%sp)"); asm("jsr _GC_push_one");
	  asm("mov.l %d2,(%sp)"); asm("jsr _GC_push_one");
	  asm("mov.l %d3,(%sp)"); asm("jsr _GC_push_one");
	  asm("mov.l %d4,(%sp)"); asm("jsr _GC_push_one");
	  asm("mov.l %d5,(%sp)"); asm("jsr _GC_push_one");
	  asm("mov.l %d6,(%sp)"); asm("jsr _GC_push_one");
	  asm("mov.l %d7,(%sp)"); asm("jsr _GC_push_one");

	  asm("addq.w &0x4,%sp");	/* put stack back where it was	*/
#	  define HAVE_PUSH_REGS
#       endif /* M68K HP */

#	if defined(M68K) && defined(AMIGA)
 	 /*  AMIGA - could be replaced by generic code 			*/
 	 /* a0, a1, d0 and d1 are caller save */

#        ifdef __GNUC__
	  asm("subq.w &0x4,%sp");	/* allocate word on top of stack */

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

	  asm("addq.w &0x4,%sp");	/* put stack back where it was	*/
#	  define HAVE_PUSH_REGS
#        else /* !__GNUC__ */
	  GC_push_one(getreg(REG_A2));
	  GC_push_one(getreg(REG_A3));
#         ifndef __SASC
	      /* Can probably be changed to #if 0 -Kjetil M. (a4=globals)*/
	    GC_push_one(getreg(REG_A4));
#	  endif
	  GC_push_one(getreg(REG_A5));
	  GC_push_one(getreg(REG_A6));
	  /* Skip stack pointer */
	  GC_push_one(getreg(REG_D2));
	  GC_push_one(getreg(REG_D3));
	  GC_push_one(getreg(REG_D4));
	  GC_push_one(getreg(REG_D5));
	  GC_push_one(getreg(REG_D6));
	  GC_push_one(getreg(REG_D7));
#	  define HAVE_PUSH_REGS
#	 endif /* !__GNUC__ */
#       endif /* AMIGA */

#	if defined(M68K) && defined(MACOS)
#	if defined(THINK_C)
#         define PushMacReg(reg) \
              move.l  reg,(sp) \
              jsr             GC_push_one
	  asm {
              sub.w   #4,sp                   ; reserve space for one parameter.
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
              add.w   #4,sp                   ; fix stack.
	  }
#	  define HAVE_PUSH_REGS
#	  undef PushMacReg
#	endif /* THINK_C */
#	if defined(__MWERKS__)
	  PushMacRegisters();
#	  define HAVE_PUSH_REGS
#	endif	/* __MWERKS__ */
#   endif	/* MACOS */

#       if defined(I386) &&!defined(OS2) &&!defined(SVR4) \
	&& (defined(__MINGW32__) || !defined(MSWIN32)) \
	&& !defined(SCO) && !defined(SCO_ELF) \
 	&& !(defined(LINUX) && defined(__ELF__)) \
	&& !(defined(FREEBSD) && defined(__ELF__)) \
	&& !(defined(NETBSD) && defined(__ELF__)) \
	&& !(defined(OPENBSD) && defined(__ELF__)) \
	&& !(defined(BEOS) && defined(__ELF__)) \
	&& !defined(DOS4GW) && !defined(HURD)
	/* I386 code, generic code does not appear to work */
	/* It does appear to work under OS2, and asms dont */
	/* This is used for some 38g UNIX variants and for CYGWIN32 */
	  asm("pushl %eax");  asm("call _GC_push_one"); asm("addl $4,%esp");
	  asm("pushl %ecx");  asm("call _GC_push_one"); asm("addl $4,%esp");
	  asm("pushl %edx");  asm("call _GC_push_one"); asm("addl $4,%esp");
	  asm("pushl %ebp");  asm("call _GC_push_one"); asm("addl $4,%esp");
	  asm("pushl %esi");  asm("call _GC_push_one"); asm("addl $4,%esp");
	  asm("pushl %edi");  asm("call _GC_push_one"); asm("addl $4,%esp");
	  asm("pushl %ebx");  asm("call _GC_push_one"); asm("addl $4,%esp");
#	  define HAVE_PUSH_REGS
#       endif

#	if ( defined(I386) && defined(LINUX) && defined(__ELF__) ) \
	|| ( defined(I386) && defined(FREEBSD) && defined(__ELF__) ) \
	|| ( defined(I386) && defined(NETBSD) && defined(__ELF__) ) \
	|| ( defined(I386) && defined(OPENBSD) && defined(__ELF__) ) \
	|| ( defined(I386) && defined(HURD) && defined(__ELF__) ) \
	|| ( defined(I386) && defined(DGUX) )

	/* This is modified for Linux with ELF (Note: _ELF_ only) */
	/* This section handles FreeBSD with ELF. */
	/* Eax is caller-save and dead here.  Other caller-save 	*/
	/* registers could also be skipped.  We assume there are no	*/
	/* pointers in MMX registers, etc.				*/
	/* We combine instructions in a single asm to prevent gcc from 	*/
	/* inserting code in the middle.				*/
	  asm("pushl %ecx; call GC_push_one; addl $4,%esp");
	  asm("pushl %edx; call GC_push_one; addl $4,%esp");
	  asm("pushl %ebp; call GC_push_one; addl $4,%esp");
	  asm("pushl %esi; call GC_push_one; addl $4,%esp");
	  asm("pushl %edi; call GC_push_one; addl $4,%esp");
	  asm("pushl %ebx; call GC_push_one; addl $4,%esp");
#	  define HAVE_PUSH_REGS
#	endif

#	if ( defined(I386) && defined(BEOS) && defined(__ELF__) )
	/* As far as I can understand from				*/
	/* http://www.beunited.org/articles/jbq/nasm.shtml,		*/
	/* only ebp, esi, edi and ebx are not scratch. How MMX 		*/
	/* etc. registers should be treated, I have no idea. 		*/
	  asm("pushl %ebp; call GC_push_one; addl $4,%esp");
	  asm("pushl %esi; call GC_push_one; addl $4,%esp");
	  asm("pushl %edi; call GC_push_one; addl $4,%esp");
	  asm("pushl %ebx; call GC_push_one; addl $4,%esp");
#	  define HAVE_PUSH_REGS
#       endif

#       if defined(I386) && defined(MSWIN32) && !defined(__MINGW32__) \
	   && !defined(USE_GENERIC)
	/* I386 code, Microsoft variant		*/
	  __asm  push eax
	  __asm  call GC_push_one
	  __asm  add esp,4
	  __asm  push ebx
	  __asm  call GC_push_one
	  __asm  add esp,4
	  __asm  push ecx
	  __asm  call GC_push_one
	  __asm  add esp,4
	  __asm  push edx
	  __asm  call GC_push_one
	  __asm  add esp,4
	  __asm  push ebp
	  __asm  call GC_push_one
	  __asm  add esp,4
	  __asm  push esi
	  __asm  call GC_push_one
	  __asm  add esp,4
	  __asm  push edi
	  __asm  call GC_push_one
	  __asm  add esp,4
#	  define HAVE_PUSH_REGS
#       endif

#       if defined(I386) && (defined(SVR4) || defined(SCO) || defined(SCO_ELF))
	/* I386 code, SVR4 variant, generic code does not appear to work */
	  asm("pushl %eax");  asm("call GC_push_one"); asm("addl $4,%esp");
	  asm("pushl %ebx");  asm("call GC_push_one"); asm("addl $4,%esp");
	  asm("pushl %ecx");  asm("call GC_push_one"); asm("addl $4,%esp");
	  asm("pushl %edx");  asm("call GC_push_one"); asm("addl $4,%esp");
	  asm("pushl %ebp");  asm("call GC_push_one"); asm("addl $4,%esp");
	  asm("pushl %esi");  asm("call GC_push_one"); asm("addl $4,%esp");
	  asm("pushl %edi");  asm("call GC_push_one"); asm("addl $4,%esp");
#	  define HAVE_PUSH_REGS
#       endif

#       ifdef NS32K
	  asm ("movd r3, tos"); asm ("bsr ?_GC_push_one"); asm ("adjspb $-4");
	  asm ("movd r4, tos"); asm ("bsr ?_GC_push_one"); asm ("adjspb $-4");
	  asm ("movd r5, tos"); asm ("bsr ?_GC_push_one"); asm ("adjspb $-4");
	  asm ("movd r6, tos"); asm ("bsr ?_GC_push_one"); asm ("adjspb $-4");
	  asm ("movd r7, tos"); asm ("bsr ?_GC_push_one"); asm ("adjspb $-4");
#	  define HAVE_PUSH_REGS
#       endif

#       if defined(SPARC)
	  GC_save_regs_ret_val = GC_save_regs_in_stack();
#	  define HAVE_PUSH_REGS
#       endif

#	ifdef RT
	    GC_push_one(TMP_SP);    /* GC_push_one from r11 */

	    asm("cas r11, r6, r0"); GC_push_one(TMP_SP);	/* r6 */
	    asm("cas r11, r7, r0"); GC_push_one(TMP_SP);	/* through */
	    asm("cas r11, r8, r0"); GC_push_one(TMP_SP);	/* r10 */
	    asm("cas r11, r9, r0"); GC_push_one(TMP_SP);
	    asm("cas r11, r10, r0"); GC_push_one(TMP_SP);

	    asm("cas r11, r12, r0"); GC_push_one(TMP_SP); /* r12 */
	    asm("cas r11, r13, r0"); GC_push_one(TMP_SP); /* through */
	    asm("cas r11, r14, r0"); GC_push_one(TMP_SP); /* r15 */
	    asm("cas r11, r15, r0"); GC_push_one(TMP_SP);
#	    define HAVE_PUSH_REGS
#       endif

#       if defined(M68K) && defined(SYSV)
  	/*  Once again similar to SUN and HP, though setjmp appears to work.
  		--Parag
  	 */
#        ifdef __GNUC__
  	  asm("subqw #0x4,%sp");	/* allocate word on top of stack */
  
  	  asm("movl %a2,%sp@");	asm("jbsr GC_push_one");
  	  asm("movl %a3,%sp@");	asm("jbsr GC_push_one");
  	  asm("movl %a4,%sp@");	asm("jbsr GC_push_one");
  	  asm("movl %a5,%sp@");	asm("jbsr GC_push_one");
  	  /* Skip frame pointer and stack pointer */
  	  asm("movl %d1,%sp@");	asm("jbsr GC_push_one");
  	  asm("movl %d2,%sp@");	asm("jbsr GC_push_one");
  	  asm("movl %d3,%sp@");	asm("jbsr GC_push_one");
  	  asm("movl %d4,%sp@");	asm("jbsr GC_push_one");
  	  asm("movl %d5,%sp@");	asm("jbsr GC_push_one");
  	  asm("movl %d6,%sp@");	asm("jbsr GC_push_one");
  	  asm("movl %d7,%sp@");	asm("jbsr GC_push_one");
  
  	  asm("addqw #0x4,%sp");	/* put stack back where it was	*/
#	  define HAVE_PUSH_REGS
#        else /* !__GNUC__*/
  	  asm("subq.w &0x4,%sp");	/* allocate word on top of stack */
  
  	  asm("mov.l %a2,(%sp)"); asm("jsr GC_push_one");
  	  asm("mov.l %a3,(%sp)"); asm("jsr GC_push_one");
  	  asm("mov.l %a4,(%sp)"); asm("jsr GC_push_one");
  	  asm("mov.l %a5,(%sp)"); asm("jsr GC_push_one");
  	  /* Skip frame pointer and stack pointer */
  	  asm("mov.l %d1,(%sp)"); asm("jsr GC_push_one");
  	  asm("mov.l %d2,(%sp)"); asm("jsr GC_push_one");
  	  asm("mov.l %d3,(%sp)"); asm("jsr GC_push_one");
  	  asm("mov.l %d4,(%sp)"); asm("jsr GC_push_one");
  	  asm("mov.l %d5,(%sp)"); asm("jsr GC_push_one");
   	  asm("mov.l %d6,(%sp)"); asm("jsr GC_push_one");
  	  asm("mov.l %d7,(%sp)"); asm("jsr GC_push_one");
  
  	  asm("addq.w &0x4,%sp");	/* put stack back where it was	*/
#	  define HAVE_PUSH_REGS
#        endif /* !__GNUC__ */
#       endif /* M68K/SYSV */

#     if defined(PJ)
	{
	    register int * sp asm ("optop");
	    extern int *__libc_stack_end;

	    GC_push_all_stack (sp, __libc_stack_end);
#	    define HAVE_PUSH_REGS
	    /* Isn't this redundant with the code to push the stack? */
        }
#     endif

      /* other machines... */
#       if !defined(HAVE_PUSH_REGS)
	    --> We just generated an empty GC_push_regs, which
	    --> is almost certainly broken.  Try defining
	    --> USE_GENERIC_PUSH_REGS instead.
#       endif
}
#endif /* !USE_GENERIC_PUSH_REGS && !USE_ASM_PUSH_REGS */

#if defined(USE_GENERIC_PUSH_REGS)
void GC_generic_push_regs(cold_gc_frame)
ptr_t cold_gc_frame;
{
	{
	    word dummy;

#	    ifdef HAVE_BUILTIN_UNWIND_INIT
	      /* This was suggested by Richard Henderson as the way to	*/
	      /* force callee-save registers and register windows onto	*/
	      /* the stack.						*/
	      __builtin_unwind_init();
#	    else /* !HAVE_BUILTIN_UNWIND_INIT */
	      /* Generic code                          */
	      /* The idea is due to Parag Patel at HP. */
	      /* We're not sure whether he would like  */
	      /* to be he acknowledged for it or not.  */
	      jmp_buf regs;
	      register word * i = (word *) regs;
	      register ptr_t lim = (ptr_t)(regs) + (sizeof regs);

	      /* Setjmp doesn't always clear all of the buffer.		*/
	      /* That tends to preserve garbage.  Clear it.   		*/
		for (; (char *)i < lim; i++) {
		    *i = 0;
		}
#	      if defined(POWERPC) || defined(MSWIN32) || defined(MSWINCE) \
                || defined(UTS4) || defined(LINUX) || defined(EWS4800)
		  (void) setjmp(regs);
#	      else
	          (void) _setjmp(regs);
		  /* We don't want to mess with signals. According to	*/
		  /* SUSV3, setjmp() may or may not save signal mask.	*/
		  /* _setjmp won't, but is less portable.		*/
#	      endif
#	    endif /* !HAVE_BUILTIN_UNWIND_INIT */
#           if (defined(SPARC) && !defined(HAVE_BUILTIN_UNWIND_INIT)) \
		|| defined(IA64)
	      /* On a register window machine, we need to save register	*/
	      /* contents on the stack for this to work.  The setjmp	*/
	      /* is probably not needed on SPARC, since pointers are	*/
	      /* only stored in windowed or scratch registers.  It is	*/
	      /* needed on IA64, since some non-windowed registers are	*/
	      /* preserved.						*/
	      {
	        GC_save_regs_ret_val = GC_save_regs_in_stack();
		/* On IA64 gcc, could use __builtin_ia64_flushrs() and	*/
		/* __builtin_ia64_flushrs().  The latter will be done	*/
		/* implicitly by __builtin_unwind_init() for gcc3.0.1	*/
		/* and later.						*/
	      }
#           endif
	    GC_push_current_stack(cold_gc_frame);
	    /* Strongly discourage the compiler from treating the above	*/
	    /* as a tail-call, since that would pop the register 	*/
	    /* contents before we get a chance to look at them.		*/
	    GC_noop1((word)(&dummy));
	}
}
#endif /* USE_GENERIC_PUSH_REGS */

/* On register window machines, we need a way to force registers into 	*/
/* the stack.	Return sp.						*/
# ifdef SPARC
    asm("	.seg 	\"text\"");
#   if defined(SVR4) || defined(NETBSD)
      asm("	.globl	GC_save_regs_in_stack");
      asm("GC_save_regs_in_stack:");
      asm("	.type GC_save_regs_in_stack,#function");
#   else
      asm("	.globl	_GC_save_regs_in_stack");
      asm("_GC_save_regs_in_stack:");
#   endif
#   if defined(__arch64__) || defined(__sparcv9)
      asm("	save	%sp,-128,%sp");
      asm("	flushw");
      asm("	ret");
      asm("	restore %sp,2047+128,%o0");
#   else
      asm("	ta	0x3   ! ST_FLUSH_WINDOWS");
      asm("	retl");
      asm("	mov	%sp,%o0");
#   endif
#   ifdef SVR4
      asm("	.GC_save_regs_in_stack_end:");
      asm("	.size GC_save_regs_in_stack,.GC_save_regs_in_stack_end-GC_save_regs_in_stack");
#   endif
#   ifdef LINT
	word GC_save_regs_in_stack() { return(0 /* sp really */);}
#   endif
# endif

/* On IA64, we also need to flush register windows.  But they end	*/
/* up on the other side of the stack segment.				*/
/* Returns the backing store pointer for the register stack.		*/
/* We now implement this as a separate assembly file, since inline	*/
/* assembly code here doesn't work with either the Intel or HP 		*/
/* compilers.								*/
# if 0
#   ifdef LINUX
	asm("        .text");
	asm("        .psr abi64");
	asm("        .psr lsb");
	asm("        .lsb");
	asm("");
	asm("        .text");
	asm("        .align 16");
	asm("        .global GC_save_regs_in_stack");
	asm("        .proc GC_save_regs_in_stack");
	asm("GC_save_regs_in_stack:");
	asm("        .body");
	asm("        flushrs");
	asm("        ;;");
	asm("        mov r8=ar.bsp");
	asm("        br.ret.sptk.few rp");
	asm("        .endp GC_save_regs_in_stack");
#   endif /* LINUX */
#   if 0 /* Other alternatives that don't work on HP/UX */
	word GC_save_regs_in_stack() {
#	  if USE_BUILTINS
	    __builtin_ia64_flushrs();
	    return __builtin_ia64_bsp();
#	  else
#	    ifdef HPUX
	      _asm("        flushrs");
	      _asm("        ;;");
	      _asm("        mov r8=ar.bsp");
	      _asm("        br.ret.sptk.few rp");
#	    else
	      asm("        flushrs");
	      asm("        ;;");
	      asm("        mov r8=ar.bsp");
	      asm("        br.ret.sptk.few rp");
#	    endif
#	  endif
	}
#   endif
# endif

/* GC_clear_stack_inner(arg, limit) clears stack area up to limit and	*/
/* returns arg.  Stack clearing is crucial on SPARC, so we supply	*/
/* an assembly version that's more careful.  Assumes limit is hotter	*/
/* than sp, and limit is 8 byte aligned.				*/
#if defined(ASM_CLEAR_CODE)
#ifndef SPARC
	--> fix it
#endif
# ifdef SUNOS4
    asm(".globl _GC_clear_stack_inner");
    asm("_GC_clear_stack_inner:");
# else
    asm(".globl GC_clear_stack_inner");
    asm("GC_clear_stack_inner:");
    asm(".type GC_save_regs_in_stack,#function");
# endif
#if defined(__arch64__) || defined(__sparcv9)
  asm("mov %sp,%o2");		/* Save sp			*/
  asm("add %sp,2047-8,%o3");	/* p = sp+bias-8		*/
  asm("add %o1,-2047-192,%sp");	/* Move sp out of the way,	*/
  				/* so that traps still work.	*/
  				/* Includes some extra words	*/
  				/* so we can be sloppy below.	*/
  asm("loop:");
  asm("stx %g0,[%o3]");		/* *(long *)p = 0		*/
  asm("cmp %o3,%o1");
  asm("bgu,pt %xcc, loop");	/* if (p > limit) goto loop	*/
    asm("add %o3,-8,%o3");	/* p -= 8 (delay slot) */
  asm("retl");
    asm("mov %o2,%sp");		/* Restore sp., delay slot	*/
#else
  asm("mov %sp,%o2");		/* Save sp	*/
  asm("add %sp,-8,%o3");	/* p = sp-8	*/
  asm("clr %g1");		/* [g0,g1] = 0	*/
  asm("add %o1,-0x60,%sp");	/* Move sp out of the way,	*/
  				/* so that traps still work.	*/
  				/* Includes some extra words	*/
  				/* so we can be sloppy below.	*/
  asm("loop:");
  asm("std %g0,[%o3]");		/* *(long long *)p = 0	*/
  asm("cmp %o3,%o1");
  asm("bgu loop	");		/* if (p > limit) goto loop	*/
    asm("add %o3,-8,%o3");	/* p -= 8 (delay slot) */
  asm("retl");
    asm("mov %o2,%sp");		/* Restore sp., delay slot	*/
#endif /* old SPARC */
  /* First argument = %o0 = return value */
#   ifdef SVR4
      asm("	.GC_clear_stack_inner_end:");
      asm("	.size GC_clear_stack_inner,.GC_clear_stack_inner_end-GC_clear_stack_inner");
#   endif
  
# ifdef LINT
    /*ARGSUSED*/
    ptr_t GC_clear_stack_inner(arg, limit)
    ptr_t arg; word limit;
    { return(arg); }
# endif
#endif  
