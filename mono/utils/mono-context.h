/*
 * mono-context.h: plat independent machine state definitions
 *
 *
 * Copyright (c) 2011 Novell, Inc (http://www.novell.com)
 */


#ifndef __MONO_MONO_CONTEXT_H__
#define __MONO_MONO_CONTEXT_H__

#include "mono-compiler.h"
#include "mono-sigcontext.h"
#include "mono-machine.h"

#ifdef HAVE_SIGNAL_H
#include <signal.h>
#endif

/*
 * General notes about mono-context.
 * Each arch defines a MonoContext struct with all GPR regs + IP/PC.
 * IP/PC should be the last element of the struct (this is a mild sgen constraint we could drop if needed)
 * Macros to get/set BP, SP and IP are defined too.
 * MONO_CONTEXT_GET_CURRENT captures the current context as close as possible. One reg might be clobbered
 *  to hold the address of the target MonoContext. It will be a caller save one, so should not be a problem.
 */
#if (defined(__i386__) && !defined(MONO_CROSS_COMPILE)) || (defined(TARGET_X86))

/*HACK, move this to an eventual mono-signal.c*/
#if defined( __linux__) || defined(__sun) || defined(__APPLE__) || defined(__NetBSD__) || \
       defined(__FreeBSD__) || defined(__OpenBSD__)
#ifdef HAVE_SIGACTION
#define MONO_SIGNAL_USE_SIGACTION 1
#endif
#endif

#if defined(__native_client__)
#undef MONO_SIGNAL_USE_SIGACTION
#endif

#ifdef HOST_WIN32
/* sigcontext surrogate */
struct sigcontext {
	unsigned int eax;
	unsigned int ebx;
	unsigned int ecx;
	unsigned int edx;
	unsigned int ebp;
	unsigned int esp;
	unsigned int esi;
	unsigned int edi;
	unsigned int eip;
};
#endif

#if defined(__FreeBSD__) || defined(__NetBSD__) || defined(__OpenBSD__) || defined(__APPLE__)
# define SC_EAX sc_eax
# define SC_EBX sc_ebx
# define SC_ECX sc_ecx
# define SC_EDX sc_edx
# define SC_EBP sc_ebp
# define SC_EIP sc_eip
# define SC_ESP sc_esp
# define SC_EDI sc_edi
# define SC_ESI sc_esi
#elif defined(__HAIKU__)
# define SC_EAX regs.eax
# define SC_EBX regs._reserved_2[2]
# define SC_ECX regs.ecx
# define SC_EDX regs.edx
# define SC_EBP regs.ebp
# define SC_EIP regs.eip
# define SC_ESP regs.esp
# define SC_EDI regs._reserved_2[0]
# define SC_ESI regs._reserved_2[1]
#else
# define SC_EAX eax
# define SC_EBX ebx
# define SC_ECX ecx
# define SC_EDX edx
# define SC_EBP ebp
# define SC_EIP eip
# define SC_ESP esp
# define SC_EDI edi
# define SC_ESI esi
#endif

typedef struct {
	mgreg_t eax;
	mgreg_t ebx;
	mgreg_t ecx;
	mgreg_t edx;
	mgreg_t ebp;
	mgreg_t esp;
	mgreg_t esi;
	mgreg_t edi;
	mgreg_t eip;
} MonoContext;

#define MONO_CONTEXT_SET_IP(ctx,ip) do { (ctx)->eip = (mgreg_t)(ip); } while (0); 
#define MONO_CONTEXT_SET_BP(ctx,bp) do { (ctx)->ebp = (mgreg_t)(bp); } while (0); 
#define MONO_CONTEXT_SET_SP(ctx,sp) do { (ctx)->esp = (mgreg_t)(sp); } while (0); 

#define MONO_CONTEXT_GET_IP(ctx) ((gpointer)((ctx)->eip))
#define MONO_CONTEXT_GET_BP(ctx) ((gpointer)((ctx)->ebp))
#define MONO_CONTEXT_GET_SP(ctx) ((gpointer)((ctx)->esp))

/*We set EAX to zero since we are clobering it anyway*/
#ifdef _MSC_VER
#define MONO_CONTEXT_GET_CURRENT(ctx) do { \
	void *_ptr = &(ctx);												\
	__asm {																\
	 __asm mov eax, _ptr												\
	 __asm mov [eax+0x00], eax											\
	 __asm mov [eax+0x04], ebx											\
	 __asm mov [eax+0x08], ecx											\
	 __asm mov [eax+0x0c], edx											\
	 __asm mov [eax+0x10], ebp											\
	 __asm mov [eax+0x14], esp											\
	 __asm mov [eax+0x18], esi											\
	 __asm mov [eax+0x1c], edi											\
	 __asm call __mono_context_get_ip									\
	 __asm __mono_context_get_ip:										\
	 __asm pop dword ptr [eax+0x20]										\
		 }																\
	} while (0)
#else
#define MONO_CONTEXT_GET_CURRENT(ctx) \
	__asm__ __volatile__(   \
	"movl $0x0, 0x00(%0)\n" \
	"mov %%ebx, 0x04(%0)\n" \
	"mov %%ecx, 0x08(%0)\n" \
	"mov %%edx, 0x0c(%0)\n" \
	"mov %%ebp, 0x10(%0)\n" \
	"mov %%esp, 0x14(%0)\n" \
	"mov %%esi, 0x18(%0)\n" \
	"mov %%edi, 0x1c(%0)\n" \
	"call 1f\n"     \
	"1: pop 0x20(%0)\n"     \
	:	\
	: "a" (&(ctx))  \
	: "memory")
#endif

#define MONO_ARCH_HAS_MONO_CONTEXT 1

#elif (defined(__x86_64__) && !defined(MONO_CROSS_COMPILE)) || (defined(TARGET_AMD64)) /* defined(__i386__) */

#include <mono/arch/amd64/amd64-codegen.h>

#if !defined( HOST_WIN32 ) && !defined(__native_client__) && !defined(__native_client_codegen__)

#ifdef HAVE_SIGACTION
#define MONO_SIGNAL_USE_SIGACTION 1
#endif

#endif

typedef struct {
	mgreg_t gregs [AMD64_NREG];
	double fregs [AMD64_XMM_NREG];
} MonoContext;

#define MONO_CONTEXT_SET_IP(ctx,ip) do { (ctx)->gregs [AMD64_RIP] = (mgreg_t)(ip); } while (0);
#define MONO_CONTEXT_SET_BP(ctx,bp) do { (ctx)->gregs [AMD64_RBP] = (mgreg_t)(bp); } while (0);
#define MONO_CONTEXT_SET_SP(ctx,esp) do { (ctx)->gregs [AMD64_RSP] = (mgreg_t)(esp); } while (0);

#define MONO_CONTEXT_GET_IP(ctx) ((gpointer)((ctx)->gregs [AMD64_RIP]))
#define MONO_CONTEXT_GET_BP(ctx) ((gpointer)((ctx)->gregs [AMD64_RBP]))
#define MONO_CONTEXT_GET_SP(ctx) ((gpointer)((ctx)->gregs [AMD64_RSP]))

#if defined (HOST_WIN32) && !defined(__GNUC__)
/* msvc doesn't support inline assembly, so have to use a separate .asm file */
extern void mono_context_get_current (void *);
#define MONO_CONTEXT_GET_CURRENT(ctx) do { mono_context_get_current((void*)&(ctx)); } while (0)

#elif defined(__native_client__)
#define MONO_CONTEXT_GET_CURRENT(ctx)	\
	__asm__ __volatile__(	\
		"movq $0x0,  %%nacl:0x00(%%r15, %0, 1)\n"	\
		"movq %%rbx, %%nacl:0x08(%%r15, %0, 1)\n"	\
		"movq %%rcx, %%nacl:0x10(%%r15, %0, 1)\n"	\
		"movq %%rdx, %%nacl:0x18(%%r15, %0, 1)\n"	\
		"movq %%rbp, %%nacl:0x20(%%r15, %0, 1)\n"	\
		"movq %%rsp, %%nacl:0x28(%%r15, %0, 1)\n"	\
		"movq %%rsi, %%nacl:0x30(%%r15, %0, 1)\n"	\
		"movq %%rdi, %%nacl:0x38(%%r15, %0, 1)\n"	\
		"movq %%r8,  %%nacl:0x40(%%r15, %0, 1)\n"	\
		"movq %%r9,  %%nacl:0x48(%%r15, %0, 1)\n"	\
		"movq %%r10, %%nacl:0x50(%%r15, %0, 1)\n"	\
		"movq %%r11, %%nacl:0x58(%%r15, %0, 1)\n"	\
		"movq %%r12, %%nacl:0x60(%%r15, %0, 1)\n"	\
		"movq %%r13, %%nacl:0x68(%%r15, %0, 1)\n"	\
		"movq %%r14, %%nacl:0x70(%%r15, %0, 1)\n"	\
		"movq %%r15, %%nacl:0x78(%%r15, %0, 1)\n"	\
		"leaq (%%rip), %%rdx\n"	\
		"movq %%rdx, %%nacl:0x80(%%r15, %0, 1)\n"	\
		: 	\
		: "a" ((int64_t)&(ctx))	\
		: "rdx", "memory")
#else
#define MONO_CONTEXT_GET_CURRENT(ctx)	\
	__asm__ __volatile__(	\
		"movq $0x0,  0x00(%0)\n"	\
		"movq %%rbx, 0x08(%0)\n"	\
		"movq %%rcx, 0x10(%0)\n"	\
		"movq %%rdx, 0x18(%0)\n"	\
		"movq %%rbp, 0x20(%0)\n"	\
		"movq %%rsp, 0x28(%0)\n"	\
		"movq %%rsi, 0x30(%0)\n"	\
		"movq %%rdi, 0x38(%0)\n"	\
		"movq %%r8,  0x40(%0)\n"	\
		"movq %%r9,  0x48(%0)\n"	\
		"movq %%r10, 0x50(%0)\n"	\
		"movq %%r11, 0x58(%0)\n"	\
		"movq %%r12, 0x60(%0)\n"	\
		"movq %%r13, 0x68(%0)\n"	\
		"movq %%r14, 0x70(%0)\n"	\
		"movq %%r15, 0x78(%0)\n"	\
		/* "leaq (%%rip), %%rdx\n" is not understood by icc */	\
		".byte 0x48, 0x8d, 0x15, 0x00, 0x00, 0x00, 0x00\n" \
		"movq %%rdx, 0x80(%0)\n"	\
		: 	\
		: "a" (&(ctx))	\
		: "rdx", "memory")
#endif

#define MONO_ARCH_HAS_MONO_CONTEXT 1

#elif (defined(__arm__) && !defined(MONO_CROSS_COMPILE)) || (defined(TARGET_ARM)) /* defined(__x86_64__) */

#include <mono/arch/arm/arm-codegen.h>

typedef struct {
	mgreg_t pc;
	mgreg_t regs [16];
	double fregs [16];
	mgreg_t cpsr;
} MonoContext;

/* we have the stack pointer, not the base pointer in sigcontext */
#define MONO_CONTEXT_SET_IP(ctx,ip) do { (ctx)->pc = (mgreg_t)ip; } while (0); 
#define MONO_CONTEXT_SET_BP(ctx,bp) do { (ctx)->regs [ARMREG_FP] = (mgreg_t)bp; } while (0); 
#define MONO_CONTEXT_SET_SP(ctx,bp) do { (ctx)->regs [ARMREG_SP] = (mgreg_t)bp; } while (0); 

#define MONO_CONTEXT_GET_IP(ctx) ((gpointer)((ctx)->pc))
#define MONO_CONTEXT_GET_BP(ctx) ((gpointer)((ctx)->regs [ARMREG_FP]))
#define MONO_CONTEXT_GET_SP(ctx) ((gpointer)((ctx)->regs [ARMREG_SP]))

#define MONO_CONTEXT_GET_CURRENT(ctx)	do { 	\
	__asm__ __volatile__(			\
		"push {r0}\n"				\
		"push {r1}\n"				\
		"mov r0, %0\n"				\
		"ldr r1, [sp, #4]\n"			\
		"str r1, [r0]!\n"			\
		"ldr r1, [sp, #0]\n"			\
		"str r1, [r0]!\n"			\
		"stmia r0!, {r2-r12}\n"		\
		"str sp, [r0]!\n"			\
		"str lr, [r0]!\n"			\
		"mov r1, pc\n"				\
		"str r1, [r0]!\n"			\
		"pop {r1}\n"				\
		"pop {r0}\n"				\
		:							\
		: "r" (&ctx.regs)			\
		: "memory"					\
	);								\
	ctx.pc = ctx.regs [15];			\
} while (0)

#define MONO_ARCH_HAS_MONO_CONTEXT 1

#elif (defined(__aarch64__) && !defined(MONO_CROSS_COMPILE)) || (defined(TARGET_ARM64))

#include <mono/arch/arm64/arm64-codegen.h>

typedef struct {
	mgreg_t regs [32];
	double fregs [32];
	mgreg_t pc;
} MonoContext;

#define MONO_CONTEXT_SET_IP(ctx,ip) do { (ctx)->pc = (mgreg_t)ip; } while (0)
#define MONO_CONTEXT_SET_BP(ctx,bp) do { (ctx)->regs [ARMREG_FP] = (mgreg_t)bp; } while (0);
#define MONO_CONTEXT_SET_SP(ctx,bp) do { (ctx)->regs [ARMREG_SP] = (mgreg_t)bp; } while (0);

#define MONO_CONTEXT_GET_IP(ctx) (gpointer)((ctx)->pc)
#define MONO_CONTEXT_GET_BP(ctx) (gpointer)((ctx)->regs [ARMREG_FP])
#define MONO_CONTEXT_GET_SP(ctx) (gpointer)((ctx)->regs [ARMREG_SP])

#define MONO_CONTEXT_GET_CURRENT(ctx)	do { 	\
	__asm__ __volatile__(			\
		"mov x16, %0\n" \
		"stp x0, x1, [x16], #16\n"	\
		"stp x2, x3, [x16], #16\n"	\
		"stp x4, x5, [x16], #16\n"	\
		"stp x6, x7, [x16], #16\n"	\
		"stp x8, x9, [x16], #16\n"	\
		"stp x10, x11, [x16], #16\n"	\
		"stp x12, x13, [x16], #16\n"	\
		"stp x14, x15, [x16], #16\n"	\
		"stp xzr, x17, [x16], #16\n"	\
		"stp x18, x19, [x16], #16\n"	\
		"stp x20, x21, [x16], #16\n"	\
		"stp x22, x23, [x16], #16\n"	\
		"stp x24, x25, [x16], #16\n"	\
		"stp x26, x27, [x16], #16\n"	\
		"stp x28, x29, [x16], #16\n"	\
		"stp x30, xzr, [x16]\n"	\
		"mov x30, sp\n"				\
		"str x30, [x16, #8]\n"		\
		:							\
		: "r" (&ctx.regs)			\
		: "x30", "memory"			\
	);								\
	__asm__ __volatile__( \
		"adr %0, L0\n" \
		"L0:\n"	\
		: "=r" (ctx.pc)		\
		:					\
		: "memory"			 \
	); \
} while (0)

#define MONO_ARCH_HAS_MONO_CONTEXT 1

#elif defined(__mono_ppc__) /* defined(__arm__) */

/* we define our own structure and we'll copy the data
 * from sigcontext/ucontext/mach when we need it.
 * This also makes us save stack space and time when copying
 * We might also want to add an additional field to propagate
 * the original context from the signal handler.
 */
typedef struct {
	gulong sc_ir;          // pc 
	gulong sc_sp;          // r1
	mgreg_t regs [19]; /*FIXME, this must be changed to 32 for sgen*/
	double fregs [18];
} MonoContext;

/* we have the stack pointer, not the base pointer in sigcontext */
#define MONO_CONTEXT_SET_IP(ctx,ip) do { (ctx)->sc_ir = (gulong)ip; } while (0);
/* FIXME: should be called SET_SP */
#define MONO_CONTEXT_SET_BP(ctx,bp) do { (ctx)->sc_sp = (gulong)bp; } while (0);
#define MONO_CONTEXT_SET_SP(ctx,sp) do { (ctx)->sc_sp = (gulong)sp; } while (0);

#define MONO_CONTEXT_GET_IP(ctx) ((gpointer)((ctx)->sc_ir))
#define MONO_CONTEXT_GET_BP(ctx) ((gpointer)((ctx)->regs [ppc_r31-13]))
#define MONO_CONTEXT_GET_SP(ctx) ((gpointer)((ctx)->sc_sp))

#elif defined(__sparc__) || defined(sparc) /* defined(__mono_ppc__) */

typedef struct MonoContext {
	guint8 *ip;
	gpointer *sp;
	gpointer *fp;
} MonoContext;

#define MONO_CONTEXT_SET_IP(ctx,eip) do { (ctx)->ip = (gpointer)(eip); } while (0); 
#define MONO_CONTEXT_SET_BP(ctx,ebp) do { (ctx)->fp = (gpointer*)(ebp); } while (0); 
#define MONO_CONTEXT_SET_SP(ctx,esp) do { (ctx)->sp = (gpointer*)(esp); } while (0); 

#define MONO_CONTEXT_GET_IP(ctx) ((gpointer)((ctx)->ip))
#define MONO_CONTEXT_GET_BP(ctx) ((gpointer)((ctx)->fp))
#define MONO_CONTEXT_GET_SP(ctx) ((gpointer)((ctx)->sp))

#elif defined(__ia64__) /*defined(__sparc__) || defined(sparc) */

#ifndef UNW_LOCAL_ONLY

#define UNW_LOCAL_ONLY
#include <libunwind.h>

#endif

typedef struct MonoContext {
	unw_cursor_t cursor;
	/* Whenever the ip in 'cursor' points to the ip where the exception happened */
	/* This is true for the initial context for exceptions thrown from signal handlers */
	gboolean precise_ip;
} MonoContext;

/*XXX SET_BP is missing*/
#define MONO_CONTEXT_SET_IP(ctx,eip) do { int err = unw_set_reg (&(ctx)->cursor, UNW_IA64_IP, (unw_word_t)(eip)); g_assert (err == 0); } while (0)
#define MONO_CONTEXT_SET_SP(ctx,esp) do { int err = unw_set_reg (&(ctx)->cursor, UNW_IA64_SP, (unw_word_t)(esp)); g_assert (err == 0); } while (0)

#define MONO_CONTEXT_GET_IP(ctx) ((gpointer)(mono_ia64_context_get_ip ((ctx))))
#define MONO_CONTEXT_GET_BP(ctx) ((gpointer)(mono_ia64_context_get_fp ((ctx))))
#define MONO_CONTEXT_GET_SP(ctx) ((gpointer)(mono_ia64_context_get_sp ((ctx))))

static inline unw_word_t
mono_ia64_context_get_ip (MonoContext *ctx)
{
	unw_word_t ip;
	int err;

	err = unw_get_reg (&ctx->cursor, UNW_IA64_IP, &ip);
	g_assert (err == 0);

	if (ctx->precise_ip) {
		return ip;
	} else {
		/* Subtrack 1 so ip points into the actual instruction */
		return ip - 1;
	}
}

static inline unw_word_t
mono_ia64_context_get_sp (MonoContext *ctx)
{
	unw_word_t sp;
	int err;

	err = unw_get_reg (&ctx->cursor, UNW_IA64_SP, &sp);
	g_assert (err == 0);

	return sp;
}

static inline unw_word_t
mono_ia64_context_get_fp (MonoContext *ctx)
{
	unw_cursor_t new_cursor;
	unw_word_t fp;
	int err;

	{
		unw_word_t ip, sp;

		err = unw_get_reg (&ctx->cursor, UNW_IA64_SP, &sp);
		g_assert (err == 0);

		err = unw_get_reg (&ctx->cursor, UNW_IA64_IP, &ip);
		g_assert (err == 0);
	}

	/* fp is the SP of the parent frame */
	new_cursor = ctx->cursor;

	err = unw_step (&new_cursor);
	g_assert (err >= 0);

	err = unw_get_reg (&new_cursor, UNW_IA64_SP, &fp);
	g_assert (err == 0);

	return fp;
}

#elif ((defined(__mips__) && !defined(MONO_CROSS_COMPILE)) || (defined(TARGET_MIPS))) && SIZEOF_REGISTER == 4 /* defined(__ia64__) */

#include <mono/arch/mips/mips-codegen.h>

typedef struct {
	mgreg_t	    sc_pc;
	mgreg_t		sc_regs [32];
	gfloat		sc_fpregs [32];
} MonoContext;

#define MONO_CONTEXT_SET_IP(ctx,ip) do { (ctx)->sc_pc = (mgreg_t)(ip); } while (0);
#define MONO_CONTEXT_SET_BP(ctx,bp) do { (ctx)->sc_regs[mips_fp] = (mgreg_t)(bp); } while (0);
#define MONO_CONTEXT_SET_SP(ctx,sp) do { (ctx)->sc_regs[mips_sp] = (mgreg_t)(sp); } while (0);

#define MONO_CONTEXT_GET_IP(ctx) ((gpointer)((ctx)->sc_pc))
#define MONO_CONTEXT_GET_BP(ctx) ((gpointer)((ctx)->sc_regs[mips_fp]))
#define MONO_CONTEXT_GET_SP(ctx) ((gpointer)((ctx)->sc_regs[mips_sp]))

#define MONO_CONTEXT_GET_CURRENT(ctx)	\
	__asm__ __volatile__(	\
		"sw $0,0(%0)\n\t"	\
		"sw $1,4(%0)\n\t"	\
		"sw $2,8(%0)\n\t"	\
		"sw $3,12(%0)\n\t"	\
		"sw $4,16(%0)\n\t"	\
		"sw $5,20(%0)\n\t"	\
		"sw $6,24(%0)\n\t"	\
		"sw $7,28(%0)\n\t"	\
		"sw $8,32(%0)\n\t"	\
		"sw $9,36(%0)\n\t"	\
		"sw $10,40(%0)\n\t"	\
		"sw $11,44(%0)\n\t"	\
		"sw $12,48(%0)\n\t"	\
		"sw $13,52(%0)\n\t"	\
		"sw $14,56(%0)\n\t"	\
		"sw $15,60(%0)\n\t"	\
		"sw $16,64(%0)\n\t"	\
		"sw $17,68(%0)\n\t"	\
		"sw $18,72(%0)\n\t"	\
		"sw $19,76(%0)\n\t"	\
		"sw $20,80(%0)\n\t"	\
		"sw $21,84(%0)\n\t"	\
		"sw $22,88(%0)\n\t"	\
		"sw $23,92(%0)\n\t"	\
		"sw $24,96(%0)\n\t"	\
		"sw $25,100(%0)\n\t"	\
		"sw $26,104(%0)\n\t"	\
		"sw $27,108(%0)\n\t"	\
		"sw $28,112(%0)\n\t"	\
		"sw $29,116(%0)\n\t"	\
		"sw $30,120(%0)\n\t"	\
		"sw $31,124(%0)\n\t"	\
		: : "r" (&(ctx).sc_regs [0])	\
		: "memory"			\
	)

#elif defined(__s390x__)

#define MONO_ARCH_HAS_MONO_CONTEXT 1

typedef struct ucontext MonoContext;

#define MONO_CONTEXT_SET_IP(ctx,ip) 					\
	do {								\
		(ctx)->uc_mcontext.gregs[14] = (unsigned long)ip;	\
		(ctx)->uc_mcontext.psw.addr = (unsigned long)ip;	\
	} while (0); 

#define MONO_CONTEXT_SET_SP(ctx,bp) MONO_CONTEXT_SET_BP((ctx),(bp))
#define MONO_CONTEXT_SET_BP(ctx,bp) 					\
	do {		 						\
		(ctx)->uc_mcontext.gregs[15] = (unsigned long)bp;	\
		(ctx)->uc_stack.ss_sp	     = (void*)bp;		\
	} while (0) 

#define MONO_CONTEXT_GET_IP(ctx) (gpointer) (ctx)->uc_mcontext.psw.addr
#define MONO_CONTEXT_GET_SP(ctx) ((gpointer)((ctx)->uc_mcontext.gregs[15]))
#define MONO_CONTEXT_GET_BP(ctx) ((gpointer)((ctx)->uc_mcontext.gregs[11]))

#define MONO_CONTEXT_GET_CURRENT(ctx)	\
	__asm__ __volatile__(	\
		"stmg	%%r0,%%r15,0(%0)\n"	\
		: : "r" (&(ctx).uc_mcontext.gregs[0])	\
		: "memory"			\
	)

#else

#error "Implement mono-context for the current arch"

#endif

/*
 * The naming is misleading, the SIGCTX argument should be the platform's context
 * structure (ucontext_c on posix, CONTEXT on windows).
 */
void mono_sigctx_to_monoctx (void *sigctx, MonoContext *mctx);

/*
 * This will not completely initialize SIGCTX since MonoContext contains less
 * information that the system context. The caller should obtain a SIGCTX from
 * the system, and use this function to override the parts of it which are
 * also in MonoContext.
 */
void mono_monoctx_to_sigctx (MonoContext *mctx, void *sigctx);

#endif /* __MONO_MONO_CONTEXT_H__ */
