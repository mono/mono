#ifndef __MONO_MINI_X86_H__
#define __MONO_MINI_X86_H__

#include <mono/arch/x86/x86-codegen.h>
#include <mono/utils/mono-sigcontext.h>
#ifdef HOST_WIN32
#include <windows.h>
/* use SIG* defines if possible */
#ifdef HAVE_SIGNAL_H
#include <signal.h>
#endif

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

typedef void (* MonoW32ExceptionHandler) (int _dummy, EXCEPTION_RECORD *info, void *context);
void win32_seh_init(void);
void win32_seh_cleanup(void);
void win32_seh_set_handler(int type, MonoW32ExceptionHandler handler);

#ifndef SIGFPE
#define SIGFPE 4
#endif

#ifndef SIGILL
#define SIGILL 8
#endif

#ifndef	SIGSEGV
#define	SIGSEGV 11
#endif

LONG CALLBACK seh_handler(EXCEPTION_POINTERS* ep);

#endif /* HOST_WIN32 */

#ifdef __HAIKU__
struct sigcontext {
	vregs regs;
};
#endif /* __HAIKU__ */

#if defined( __linux__) || defined(__sun) || defined(__APPLE__) || defined(__NetBSD__) || \
       defined(__FreeBSD__) || defined(__OpenBSD__)
#define MONO_ARCH_USE_SIGACTION
#endif

#if defined(__native_client__)
#undef MONO_ARCH_USE_SIGACTION
#endif

#if defined(__native_client_codegen__) || defined(__native_client__)
#define NACL_SIZE(a, b) (b)
#else
#define NACL_SIZE(a, b) (a)
#endif

#ifndef HOST_WIN32

#ifdef HAVE_WORKING_SIGALTSTACK
/* 
 * solaris doesn't have pthread_getattr_np () needed by the sigaltstack setup
 * code.
 */
#ifndef __sun
#define MONO_ARCH_SIGSEGV_ON_ALTSTACK
#endif
/* Haiku doesn't have SA_SIGINFO */
#ifndef __HAIKU__
#define MONO_ARCH_USE_SIGACTION
#endif /* __HAIKU__ */

#endif /* HAVE_WORKING_SIGALTSTACK */
#endif /* !HOST_WIN32 */

#define MONO_ARCH_SUPPORT_SIMD_INTRINSICS 1
#define MONO_ARCH_SUPPORT_TASKLETS 1

#ifndef DISABLE_SIMD
#define MONO_ARCH_SIMD_INTRINSICS 1
#define MONO_ARCH_NEED_SIMD_BANK 1
#endif

/* we should lower this size and make sure we don't call heavy stack users in the segv handler */
#define MONO_ARCH_SIGNAL_STACK_SIZE (16 * 1024)
#define MONO_ARCH_HAVE_RESTORE_STACK_SUPPORT 1

#define MONO_ARCH_CPU_SPEC x86_desc

#define MONO_MAX_IREGS 8
#define MONO_MAX_FREGS 8
#define MONO_MAX_XREGS 8

/* Parameters used by the register allocator */
#define MONO_ARCH_CALLEE_REGS X86_CALLEE_REGS
#define MONO_ARCH_CALLEE_SAVED_REGS X86_CALLER_REGS

#define MONO_ARCH_CALLEE_FREGS (0xff & ~(regmask (MONO_ARCH_FPSTACK_SIZE)))
#define MONO_ARCH_CALLEE_SAVED_FREGS 0

/* All registers are clobered by a call */
#define MONO_ARCH_CALLEE_XREGS (0xff & ~(regmask (MONO_MAX_XREGS)))
#define MONO_ARCH_CALLEE_SAVED_XREGS 0

#define MONO_ARCH_USE_FPSTACK TRUE
#define MONO_ARCH_FPSTACK_SIZE 6

#define MONO_ARCH_INST_FIXED_REG(desc) (((desc == ' ') || (desc == 'i')) ? -1 : ((desc == 's') ? X86_ECX : ((desc == 'a') ? X86_EAX : ((desc == 'd') ? X86_EDX : ((desc == 'l') ? X86_EAX : -1)))))

#define MONO_ARCH_INST_FIXED_MASK(desc) ((desc == 'y') ? (X86_BYTE_REGS) : 0)

/* RDX is clobbered by the opcode implementation before accessing sreg2 */
/* 
 * Originally this contained X86_EDX for div/rem opcodes, but that led to unsolvable 
 * situations since there are only 3 usable registers for local register allocation.
 * Instead, we handle the sreg2==edx case in the opcodes.
 */
#define MONO_ARCH_INST_SREG2_MASK(ins) 0

/*
 * L is a generic register pair, while l means eax:rdx
 */
#define MONO_ARCH_INST_IS_REGPAIR(desc) (desc == 'l' || desc == 'L')
#define MONO_ARCH_INST_REGPAIR_REG2(desc,hreg1) (desc == 'l' ? X86_EDX : -1)

/* must be at a power of 2 and >= 8 */
#define MONO_ARCH_FRAME_ALIGNMENT 16

/* fixme: align to 16byte instead of 32byte (we align to 32byte to get 
 * reproduceable results for benchmarks */
#define MONO_ARCH_CODE_ALIGNMENT 32

#define MONO_ARCH_RETREG1 X86_EAX
#define MONO_ARCH_RETREG2 X86_EDX

/*This is the max size of the locals area of a given frame. I think 1MB is a safe default for now*/
#define MONO_ARCH_MAX_FRAME_SIZE 0x100000

struct MonoLMF {
	/* 
	 * If the lowest bit is set to 1, then this is a trampoline LMF frame.
	 * If the second lowest bit is set to 1, then this is a MonoLMFExt structure, and
	 * the other fields are not valid.
	 */
	guint32    previous_lmf;
	gpointer    lmf_addr;
	/* Only set in trampoline LMF frames */
	MonoMethod *method;
	/* Only set in trampoline LMF frames */
	guint32     esp;
	guint32     ebx;
	guint32     edi;
	guint32     esi;
	guint32     ebp;
	guint32     eip;
};

typedef struct {
	gboolean need_stack_frame_inited;
	gboolean need_stack_frame;
} MonoCompileArch;

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
	guint32 eax;
	guint32 ebx;
	guint32 ecx;
	guint32 edx;
	guint32 ebp;
	guint32 esp;
    guint32 esi;
	guint32 edi;
	guint32 eip;
} MonoContext;

#define MONO_CONTEXT_SET_IP(ctx,ip) do { (ctx)->eip = (long)(ip); } while (0); 
#define MONO_CONTEXT_SET_BP(ctx,bp) do { (ctx)->ebp = (long)(bp); } while (0); 
#define MONO_CONTEXT_SET_SP(ctx,sp) do { (ctx)->esp = (long)(sp); } while (0); 

#define MONO_CONTEXT_GET_IP(ctx) ((gpointer)((ctx)->eip))
#define MONO_CONTEXT_GET_BP(ctx) ((gpointer)((ctx)->ebp))
#define MONO_CONTEXT_GET_SP(ctx) ((gpointer)((ctx)->esp))

#define MONO_CONTEXT_SET_LLVM_EXC_REG(ctx, exc) do { (ctx)->eax = (gsize)exc; } while (0)

#ifdef _MSC_VER

#define MONO_INIT_CONTEXT_FROM_FUNC(ctx, start_func) do { \
    unsigned int stackptr; \
	mono_arch_flush_register_windows (); \
    { \
	   __asm mov stackptr, ebp \
    } \
	MONO_CONTEXT_SET_IP ((ctx), (start_func)); \
	MONO_CONTEXT_SET_BP ((ctx), stackptr); \
	MONO_CONTEXT_SET_SP ((ctx), stackptr); \
} while (0)

#else

#define MONO_INIT_CONTEXT_FROM_FUNC(ctx,start_func) do {	\
		mono_arch_flush_register_windows ();	\
		MONO_CONTEXT_SET_IP ((ctx), (start_func));	\
		MONO_CONTEXT_SET_BP ((ctx), __builtin_frame_address (0));	\
		MONO_CONTEXT_SET_SP ((ctx), __builtin_frame_address (0));	\
	} while (0)

#endif

/*
 * This structure is an extension of MonoLMF and contains extra information.
 */
typedef struct {
	struct MonoLMF lmf;
	gboolean debugger_invoke;
	MonoContext ctx; /* if debugger_invoke is TRUE */
} MonoLMFExt;

/* Enables OP_LSHL, OP_LSHL_IMM, OP_LSHR, OP_LSHR_IMM, OP_LSHR_UN, OP_LSHR_UN_IMM */
#define MONO_ARCH_NO_EMULATE_LONG_SHIFT_OPS

#define MONO_ARCH_BIGMUL_INTRINS 1
#define MONO_ARCH_NEED_DIV_CHECK 1
#define MONO_ARCH_HAVE_IS_INT_OVERFLOW 1
#define MONO_ARCH_HAVE_INVALIDATE_METHOD 1
#define MONO_ARCH_NEED_GOT_VAR 1
#define MONO_ARCH_ENABLE_MONO_LMF_VAR 1
#define MONO_ARCH_HAVE_CREATE_DELEGATE_TRAMPOLINE 1
#define MONO_ARCH_HAVE_ATOMIC_ADD 1
#define MONO_ARCH_HAVE_ATOMIC_EXCHANGE 1
#define MONO_ARCH_HAVE_ATOMIC_CAS 1
#define MONO_ARCH_HAVE_IMT 1
#define MONO_ARCH_HAVE_TLS_GET (mono_x86_have_tls_get ())
#define MONO_ARCH_IMT_REG X86_EDX
#define MONO_ARCH_VTABLE_REG X86_EDX
#define MONO_ARCH_RGCTX_REG X86_EDX
#define MONO_ARCH_HAVE_GENERALIZED_IMT_THUNK 1
#define MONO_ARCH_HAVE_LIVERANGE_OPS 1
#define MONO_ARCH_HAVE_XP_UNWIND 1
#define MONO_ARCH_HAVE_SIGCTX_TO_MONOCTX 1
#if defined(__linux__)
#define MONO_ARCH_MONITOR_OBJECT_REG X86_EAX
#endif
#define MONO_ARCH_HAVE_STATIC_RGCTX_TRAMPOLINE 1
#define MONO_ARCH_HAVE_FULL_AOT_TRAMPOLINES 1
#define MONO_ARCH_GOT_REG X86_EBX
#define MONO_ARCH_HAVE_GET_TRAMPOLINES 1

#define MONO_ARCH_HAVE_CMOV_OPS 1

#ifdef MONO_ARCH_SIMD_INTRINSICS
#define MONO_ARCH_HAVE_DECOMPOSE_OPTS 1
#endif

#define MONO_ARCH_HAVE_DECOMPOSE_LONG_OPTS 1

#if !defined(__APPLE__) || defined(__native_client_codegen__)
#define MONO_ARCH_AOT_SUPPORTED 1
#endif

#if defined(__linux__) || defined(__sun)
#define MONO_ARCH_ENABLE_MONITOR_IL_FASTPATH 1
#endif

#define MONO_ARCH_GSHARED_SUPPORTED 1
#define MONO_ARCH_HAVE_LLVM_IMT_TRAMPOLINE 1
#define MONO_ARCH_LLVM_SUPPORTED 1
#define MONO_ARCH_THIS_AS_FIRST_ARG 1

#if defined(MONO_ARCH_USE_SIGACTION) || defined(TARGET_WIN32)
#define MONO_ARCH_SOFT_DEBUG_SUPPORTED 1
#endif

#define MONO_ARCH_HAVE_EXCEPTIONS_INIT 1
#define MONO_ARCH_HAVE_HANDLER_BLOCK_GUARD 1

#define MONO_ARCH_HAVE_CARD_TABLE_WBARRIER 1

/* Used for optimization, not complete */
#define MONO_ARCH_IS_OP_MEMBASE(opcode) ((opcode) == OP_X86_PUSH_MEMBASE)

#define MONO_ARCH_EMIT_BOUNDS_CHECK(cfg, array_reg, offset, index_reg) do { \
            MonoInst *inst; \
            MONO_INST_NEW ((cfg), inst, OP_X86_COMPARE_MEMBASE_REG); \
            inst->inst_basereg = array_reg; \
            inst->inst_offset = offset; \
            inst->sreg2 = index_reg; \
            MONO_ADD_INS ((cfg)->cbb, inst); \
			MONO_EMIT_NEW_COND_EXC (cfg, LE_UN, "IndexOutOfRangeException"); \
	} while (0)

typedef struct {
	guint8 *address;
	guint8 saved_byte;
} MonoBreakpointInfo;

extern MonoBreakpointInfo mono_breakpoint_info [MONO_BREAKPOINT_ARRAY_SIZE];

guint8*
mono_x86_emit_tls_get (guint8* code, int dreg, int tls_offset) MONO_INTERNAL;

guint32
mono_x86_get_this_arg_offset (MonoGenericSharingContext *gsctx, MonoMethodSignature *sig) MONO_INTERNAL;

gboolean
mono_x86_have_tls_get (void) MONO_INTERNAL;

void
mono_x86_throw_exception (mgreg_t *regs, MonoObject *exc, 
						  mgreg_t eip, gboolean rethrow) MONO_INTERNAL;

void
mono_x86_throw_corlib_exception (mgreg_t *regs, guint32 ex_token_index, 
								 mgreg_t eip, gint32 pc_offset) MONO_INTERNAL;

void 
mono_x86_patch (unsigned char* code, gpointer target) MONO_INTERNAL;

#endif /* __MONO_MINI_X86_H__ */  

