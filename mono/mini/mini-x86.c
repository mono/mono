/*
 * mini-x86.c: x86 backend for the Mono code generator
 *
 * Authors:
 *   Paolo Molaro (lupus@ximian.com)
 *   Dietmar Maurer (dietmar@ximian.com)
 *   Patrik Torstensson
 *
 * (C) 2003 Ximian, Inc.
 */
#include "mini.h"
#include <string.h>
#include <math.h>
#ifdef HAVE_UNISTD_H
#include <unistd.h>
#endif

#include <mono/metadata/appdomain.h>
#include <mono/metadata/debug-helpers.h>
#include <mono/metadata/threads.h>
#include <mono/metadata/profiler-private.h>
#include <mono/metadata/mono-debug.h>
#include <mono/utils/mono-math.h>
#include <mono/utils/mono-counters.h>

#include "trace.h"
#include "mini-x86.h"
#include "cpu-x86.h"
#include "ir-emit.h"

/* On windows, these hold the key returned by TlsAlloc () */
static gint lmf_tls_offset = -1;
static gint lmf_addr_tls_offset = -1;
static gint appdomain_tls_offset = -1;
static gint thread_tls_offset = -1;

#ifdef MONO_XEN_OPT
static gboolean optimize_for_xen = TRUE;
#else
#define optimize_for_xen 0
#endif

#ifdef PLATFORM_WIN32
static gboolean is_win32 = TRUE;
#else
static gboolean is_win32 = FALSE;
#endif

/* This mutex protects architecture specific caches */
#define mono_mini_arch_lock() EnterCriticalSection (&mini_arch_mutex)
#define mono_mini_arch_unlock() LeaveCriticalSection (&mini_arch_mutex)
static CRITICAL_SECTION mini_arch_mutex;

#define ALIGN_TO(val,align) ((((guint64)val) + ((align) - 1)) & ~((align) - 1))

#define ARGS_OFFSET 8

#ifdef PLATFORM_WIN32
/* Under windows, the default pinvoke calling convention is stdcall */
#define CALLCONV_IS_STDCALL(sig) ((((sig)->call_convention) == MONO_CALL_STDCALL) || ((sig)->pinvoke && ((sig)->call_convention) == MONO_CALL_DEFAULT))
#else
#define CALLCONV_IS_STDCALL(sig) (((sig)->call_convention) == MONO_CALL_STDCALL)
#endif

MonoBreakpointInfo
mono_breakpoint_info [MONO_BREAKPOINT_ARRAY_SIZE];

const char*
mono_arch_regname (int reg)
{
	switch (reg) {
	case X86_EAX: return "%eax";
	case X86_EBX: return "%ebx";
	case X86_ECX: return "%ecx";
	case X86_EDX: return "%edx";
	case X86_ESP: return "%esp";	
	case X86_EBP: return "%ebp";
	case X86_EDI: return "%edi";
	case X86_ESI: return "%esi";
	}
	return "unknown";
}

const char*
mono_arch_fregname (int reg)
{
	switch (reg) {
	case 0:
		return "%fr0";
	case 1:
		return "%fr1";
	case 2:
		return "%fr2";
	case 3:
		return "%fr3";
	case 4:
		return "%fr4";
	case 5:
		return "%fr5";
	case 6:
		return "%fr6";
	case 7:
		return "%fr7";
	default:
		return "unknown";
	}
}

const char *
mono_arch_xregname (int reg)
{
	switch (reg) {
	case 0:
		return "%xmm0";
	case 1:
		return "%xmm1";
	case 2:
		return "%xmm2";
	case 3:
		return "%xmm3";
	case 4:
		return "%xmm4";
	case 5:
		return "%xmm5";
	case 6:
		return "%xmm6";
	case 7:
		return "%xmm7";
	default:
		return "unknown";
	}
}


typedef enum {
	ArgInIReg,
	ArgInFloatSSEReg,
	ArgInDoubleSSEReg,
	ArgOnStack,
	ArgValuetypeInReg,
	ArgOnFloatFpStack,
	ArgOnDoubleFpStack,
	ArgNone
} ArgStorage;

typedef struct {
	gint16 offset;
	gint8  reg;
	ArgStorage storage;

	/* Only if storage == ArgValuetypeInReg */
	ArgStorage pair_storage [2];
	gint8 pair_regs [2];
} ArgInfo;

typedef struct {
	int nargs;
	guint32 stack_usage;
	guint32 reg_usage;
	guint32 freg_usage;
	gboolean need_stack_align;
	guint32 stack_align_amount;
	ArgInfo ret;
	ArgInfo sig_cookie;
	ArgInfo args [1];
} CallInfo;

#define PARAM_REGS 0

#define FLOAT_PARAM_REGS 0

static X86_Reg_No param_regs [] = { 0 };

#if defined(PLATFORM_WIN32) || defined(__APPLE__) || defined(__FreeBSD__)
#define SMALL_STRUCTS_IN_REGS
static X86_Reg_No return_regs [] = { X86_EAX, X86_EDX };
#endif

static void inline
add_general (guint32 *gr, guint32 *stack_size, ArgInfo *ainfo)
{
    ainfo->offset = *stack_size;

    if (*gr >= PARAM_REGS) {
		ainfo->storage = ArgOnStack;
		(*stack_size) += sizeof (gpointer);
    }
    else {
		ainfo->storage = ArgInIReg;
		ainfo->reg = param_regs [*gr];
		(*gr) ++;
    }
}

static void inline
add_general_pair (guint32 *gr, guint32 *stack_size, ArgInfo *ainfo)
{
	ainfo->offset = *stack_size;

	g_assert (PARAM_REGS == 0);
	
	ainfo->storage = ArgOnStack;
	(*stack_size) += sizeof (gpointer) * 2;
}

static void inline
add_float (guint32 *gr, guint32 *stack_size, ArgInfo *ainfo, gboolean is_double)
{
    ainfo->offset = *stack_size;

    if (*gr >= FLOAT_PARAM_REGS) {
		ainfo->storage = ArgOnStack;
		(*stack_size) += is_double ? 8 : 4;
    }
    else {
		/* A double register */
		if (is_double)
			ainfo->storage = ArgInDoubleSSEReg;
		else
			ainfo->storage = ArgInFloatSSEReg;
		ainfo->reg = *gr;
		(*gr) += 1;
    }
}


static void
add_valuetype (MonoGenericSharingContext *gsctx, MonoMethodSignature *sig, ArgInfo *ainfo, MonoType *type,
	       gboolean is_return,
	       guint32 *gr, guint32 *fr, guint32 *stack_size)
{
	guint32 size;
	MonoClass *klass;

	klass = mono_class_from_mono_type (type);
	size = mini_type_stack_size_full (gsctx, &klass->byval_arg, NULL, sig->pinvoke);

#ifdef SMALL_STRUCTS_IN_REGS
	if (sig->pinvoke && is_return) {
		MonoMarshalType *info;

		/*
		 * the exact rules are not very well documented, the code below seems to work with the 
		 * code generated by gcc 3.3.3 -mno-cygwin.
		 */
		info = mono_marshal_load_type_info (klass);
		g_assert (info);

		ainfo->pair_storage [0] = ainfo->pair_storage [1] = ArgNone;

		/* Special case structs with only a float member */
		if ((info->native_size == 8) && (info->num_fields == 1) && (info->fields [0].field->type->type == MONO_TYPE_R8)) {
			ainfo->storage = ArgValuetypeInReg;
			ainfo->pair_storage [0] = ArgOnDoubleFpStack;
			return;
		}
		if ((info->native_size == 4) && (info->num_fields == 1) && (info->fields [0].field->type->type == MONO_TYPE_R4)) {
			ainfo->storage = ArgValuetypeInReg;
			ainfo->pair_storage [0] = ArgOnFloatFpStack;
			return;
		}		
		if ((info->native_size == 1) || (info->native_size == 2) || (info->native_size == 4) || (info->native_size == 8)) {
			ainfo->storage = ArgValuetypeInReg;
			ainfo->pair_storage [0] = ArgInIReg;
			ainfo->pair_regs [0] = return_regs [0];
			if (info->native_size > 4) {
				ainfo->pair_storage [1] = ArgInIReg;
				ainfo->pair_regs [1] = return_regs [1];
			}
			return;
		}
	}
#endif

	ainfo->offset = *stack_size;
	ainfo->storage = ArgOnStack;
	*stack_size += ALIGN_TO (size, sizeof (gpointer));
}

/*
 * get_call_info:
 *
 *  Obtain information about a call according to the calling convention.
 * For x86 ELF, see the "System V Application Binary Interface Intel386 
 * Architecture Processor Supplment, Fourth Edition" document for more
 * information.
 * For x86 win32, see ???.
 */
static CallInfo*
get_call_info (MonoGenericSharingContext *gsctx, MonoMemPool *mp, MonoMethodSignature *sig, gboolean is_pinvoke)
{
	guint32 i, gr, fr;
	MonoType *ret_type;
	int n = sig->hasthis + sig->param_count;
	guint32 stack_size = 0;
	CallInfo *cinfo;

	if (mp)
		cinfo = mono_mempool_alloc0 (mp, sizeof (CallInfo) + (sizeof (ArgInfo) * n));
	else
		cinfo = g_malloc0 (sizeof (CallInfo) + (sizeof (ArgInfo) * n));

	gr = 0;
	fr = 0;

	/* return value */
	{
		ret_type = mini_type_get_underlying_type (gsctx, sig->ret);
		switch (ret_type->type) {
		case MONO_TYPE_BOOLEAN:
		case MONO_TYPE_I1:
		case MONO_TYPE_U1:
		case MONO_TYPE_I2:
		case MONO_TYPE_U2:
		case MONO_TYPE_CHAR:
		case MONO_TYPE_I4:
		case MONO_TYPE_U4:
		case MONO_TYPE_I:
		case MONO_TYPE_U:
		case MONO_TYPE_PTR:
		case MONO_TYPE_FNPTR:
		case MONO_TYPE_CLASS:
		case MONO_TYPE_OBJECT:
		case MONO_TYPE_SZARRAY:
		case MONO_TYPE_ARRAY:
		case MONO_TYPE_STRING:
			cinfo->ret.storage = ArgInIReg;
			cinfo->ret.reg = X86_EAX;
			break;
		case MONO_TYPE_U8:
		case MONO_TYPE_I8:
			cinfo->ret.storage = ArgInIReg;
			cinfo->ret.reg = X86_EAX;
			break;
		case MONO_TYPE_R4:
			cinfo->ret.storage = ArgOnFloatFpStack;
			break;
		case MONO_TYPE_R8:
			cinfo->ret.storage = ArgOnDoubleFpStack;
			break;
		case MONO_TYPE_GENERICINST:
			if (!mono_type_generic_inst_is_valuetype (sig->ret)) {
				cinfo->ret.storage = ArgInIReg;
				cinfo->ret.reg = X86_EAX;
				break;
			}
			/* Fall through */
		case MONO_TYPE_VALUETYPE: {
			guint32 tmp_gr = 0, tmp_fr = 0, tmp_stacksize = 0;

			add_valuetype (gsctx, sig, &cinfo->ret, sig->ret, TRUE, &tmp_gr, &tmp_fr, &tmp_stacksize);
			if (cinfo->ret.storage == ArgOnStack)
				/* The caller passes the address where the value is stored */
				add_general (&gr, &stack_size, &cinfo->ret);
			break;
		}
		case MONO_TYPE_TYPEDBYREF:
			/* Same as a valuetype with size 24 */
			add_general (&gr, &stack_size, &cinfo->ret);
			;
			break;
		case MONO_TYPE_VOID:
			cinfo->ret.storage = ArgNone;
			break;
		default:
			g_error ("Can't handle as return value 0x%x", sig->ret->type);
		}
	}

	/* this */
	if (sig->hasthis)
		add_general (&gr, &stack_size, cinfo->args + 0);

	if (!sig->pinvoke && (sig->call_convention == MONO_CALL_VARARG) && (n == 0)) {
		gr = PARAM_REGS;
		fr = FLOAT_PARAM_REGS;
		
		/* Emit the signature cookie just before the implicit arguments */
		add_general (&gr, &stack_size, &cinfo->sig_cookie);
	}

	for (i = 0; i < sig->param_count; ++i) {
		ArgInfo *ainfo = &cinfo->args [sig->hasthis + i];
		MonoType *ptype;

		if (!sig->pinvoke && (sig->call_convention == MONO_CALL_VARARG) && (i == sig->sentinelpos)) {
			/* We allways pass the sig cookie on the stack for simplicity */
			/* 
			 * Prevent implicit arguments + the sig cookie from being passed 
			 * in registers.
			 */
			gr = PARAM_REGS;
			fr = FLOAT_PARAM_REGS;

			/* Emit the signature cookie just before the implicit arguments */
			add_general (&gr, &stack_size, &cinfo->sig_cookie);
		}

		if (sig->params [i]->byref) {
			add_general (&gr, &stack_size, ainfo);
			continue;
		}
		ptype = mini_type_get_underlying_type (gsctx, sig->params [i]);
		switch (ptype->type) {
		case MONO_TYPE_BOOLEAN:
		case MONO_TYPE_I1:
		case MONO_TYPE_U1:
			add_general (&gr, &stack_size, ainfo);
			break;
		case MONO_TYPE_I2:
		case MONO_TYPE_U2:
		case MONO_TYPE_CHAR:
			add_general (&gr, &stack_size, ainfo);
			break;
		case MONO_TYPE_I4:
		case MONO_TYPE_U4:
			add_general (&gr, &stack_size, ainfo);
			break;
		case MONO_TYPE_I:
		case MONO_TYPE_U:
		case MONO_TYPE_PTR:
		case MONO_TYPE_FNPTR:
		case MONO_TYPE_CLASS:
		case MONO_TYPE_OBJECT:
		case MONO_TYPE_STRING:
		case MONO_TYPE_SZARRAY:
		case MONO_TYPE_ARRAY:
			add_general (&gr, &stack_size, ainfo);
			break;
		case MONO_TYPE_GENERICINST:
			if (!mono_type_generic_inst_is_valuetype (sig->params [i])) {
				add_general (&gr, &stack_size, ainfo);
				break;
			}
			/* Fall through */
		case MONO_TYPE_VALUETYPE:
			add_valuetype (gsctx, sig, ainfo, sig->params [i], FALSE, &gr, &fr, &stack_size);
			break;
		case MONO_TYPE_TYPEDBYREF:
			stack_size += sizeof (MonoTypedRef);
			ainfo->storage = ArgOnStack;
			break;
		case MONO_TYPE_U8:
		case MONO_TYPE_I8:
			add_general_pair (&gr, &stack_size, ainfo);
			break;
		case MONO_TYPE_R4:
			add_float (&fr, &stack_size, ainfo, FALSE);
			break;
		case MONO_TYPE_R8:
			add_float (&fr, &stack_size, ainfo, TRUE);
			break;
		default:
			g_error ("unexpected type 0x%x", ptype->type);
			g_assert_not_reached ();
		}
	}

	if (!sig->pinvoke && (sig->call_convention == MONO_CALL_VARARG) && (n > 0) && (sig->sentinelpos == sig->param_count)) {
		gr = PARAM_REGS;
		fr = FLOAT_PARAM_REGS;
		
		/* Emit the signature cookie just before the implicit arguments */
		add_general (&gr, &stack_size, &cinfo->sig_cookie);
	}

	if (mono_do_x86_stack_align && (stack_size % MONO_ARCH_FRAME_ALIGNMENT) != 0) {
		cinfo->need_stack_align = TRUE;
		cinfo->stack_align_amount = MONO_ARCH_FRAME_ALIGNMENT - (stack_size % MONO_ARCH_FRAME_ALIGNMENT);
		stack_size += cinfo->stack_align_amount;
	}

	cinfo->stack_usage = stack_size;
	cinfo->reg_usage = gr;
	cinfo->freg_usage = fr;
	return cinfo;
}

/*
 * mono_arch_get_argument_info:
 * @csig:  a method signature
 * @param_count: the number of parameters to consider
 * @arg_info: an array to store the result infos
 *
 * Gathers information on parameters such as size, alignment and
 * padding. arg_info should be large enought to hold param_count + 1 entries. 
 *
 * Returns the size of the argument area on the stack.
 */
int
mono_arch_get_argument_info (MonoMethodSignature *csig, int param_count, MonoJitArgumentInfo *arg_info)
{
	int k, args_size = 0;
	int size, pad;
	guint32 align;
	int offset = 8;
	CallInfo *cinfo;

	cinfo = get_call_info (NULL, NULL, csig, FALSE);

	if (MONO_TYPE_ISSTRUCT (csig->ret) && (cinfo->ret.storage == ArgOnStack)) {
		args_size += sizeof (gpointer);
		offset += 4;
	}

	arg_info [0].offset = offset;

	if (csig->hasthis) {
		args_size += sizeof (gpointer);
		offset += 4;
	}

	arg_info [0].size = args_size;

	for (k = 0; k < param_count; k++) {
		size = mini_type_stack_size_full (NULL, csig->params [k], &align, csig->pinvoke);

		/* ignore alignment for now */
		align = 1;

		args_size += pad = (align - (args_size & (align - 1))) & (align - 1);	
		arg_info [k].pad = pad;
		args_size += size;
		arg_info [k + 1].pad = 0;
		arg_info [k + 1].size = size;
		offset += pad;
		arg_info [k + 1].offset = offset;
		offset += size;
	}

	if (mono_do_x86_stack_align && !CALLCONV_IS_STDCALL (csig))
		align = MONO_ARCH_FRAME_ALIGNMENT;
	else
		align = 4;
	args_size += pad = (align - (args_size & (align - 1))) & (align - 1);
	arg_info [k].pad = pad;

	g_free (cinfo);

	return args_size;
}

static const guchar cpuid_impl [] = {
	0x55,                   	/* push   %ebp */
	0x89, 0xe5,                	/* mov    %esp,%ebp */
	0x53,                   	/* push   %ebx */
	0x8b, 0x45, 0x08,             	/* mov    0x8(%ebp),%eax */
	0x0f, 0xa2,                	/* cpuid   */
	0x50,                   	/* push   %eax */
	0x8b, 0x45, 0x10,             	/* mov    0x10(%ebp),%eax */
	0x89, 0x18,                	/* mov    %ebx,(%eax) */
	0x8b, 0x45, 0x14,             	/* mov    0x14(%ebp),%eax */
	0x89, 0x08,                	/* mov    %ecx,(%eax) */
	0x8b, 0x45, 0x18,             	/* mov    0x18(%ebp),%eax */
	0x89, 0x10,                	/* mov    %edx,(%eax) */
	0x58,                   	/* pop    %eax */
	0x8b, 0x55, 0x0c,             	/* mov    0xc(%ebp),%edx */
	0x89, 0x02,                	/* mov    %eax,(%edx) */
	0x5b,                   	/* pop    %ebx */
	0xc9,                   	/* leave   */
	0xc3,                   	/* ret     */
};

typedef void (*CpuidFunc) (int id, int* p_eax, int* p_ebx, int* p_ecx, int* p_edx);

static int 
cpuid (int id, int* p_eax, int* p_ebx, int* p_ecx, int* p_edx)
{
	int have_cpuid = 0;
#ifndef _MSC_VER
	__asm__  __volatile__ (
		"pushfl\n"
		"popl %%eax\n"
		"movl %%eax, %%edx\n"
		"xorl $0x200000, %%eax\n"
		"pushl %%eax\n"
		"popfl\n"
		"pushfl\n"
		"popl %%eax\n"
		"xorl %%edx, %%eax\n"
		"andl $0x200000, %%eax\n"
		"movl %%eax, %0"
		: "=r" (have_cpuid)
		:
		: "%eax", "%edx"
	);
#else
	__asm {
		pushfd
		pop eax
		mov edx, eax
		xor eax, 0x200000
		push eax
		popfd
		pushfd
		pop eax
		xor eax, edx
		and eax, 0x200000
		mov have_cpuid, eax
	}
#endif
	if (have_cpuid) {
		/* Have to use the code manager to get around WinXP DEP */
		static CpuidFunc func = NULL;
		void *ptr;
		if (!func) {
			ptr = mono_global_codeman_reserve (sizeof (cpuid_impl));
			memcpy (ptr, cpuid_impl, sizeof (cpuid_impl));
			func = (CpuidFunc)ptr;
		}
		func (id, p_eax, p_ebx, p_ecx, p_edx);

		/*
		 * We use this approach because of issues with gcc and pic code, see:
		 * http://gcc.gnu.org/cgi-bin/gnatsweb.pl?cmd=view%20audit-trail&database=gcc&pr=7329
		__asm__ __volatile__ ("cpuid"
			: "=a" (*p_eax), "=b" (*p_ebx), "=c" (*p_ecx), "=d" (*p_edx)
			: "a" (id));
		*/
		return 1;
	}
	return 0;
}

/*
 * Initialize the cpu to execute managed code.
 */
void
mono_arch_cpu_init (void)
{
	/* spec compliance requires running with double precision */
#ifndef _MSC_VER
	guint16 fpcw;

	__asm__  __volatile__ ("fnstcw %0\n": "=m" (fpcw));
	fpcw &= ~X86_FPCW_PRECC_MASK;
	fpcw |= X86_FPCW_PREC_DOUBLE;
	__asm__  __volatile__ ("fldcw %0\n": : "m" (fpcw));
	__asm__  __volatile__ ("fnstcw %0\n": "=m" (fpcw));
#else
	_control87 (_PC_53, MCW_PC);
#endif
}

/*
 * Initialize architecture specific code.
 */
void
mono_arch_init (void)
{
	InitializeCriticalSection (&mini_arch_mutex);
}

/*
 * Cleanup architecture specific code.
 */
void
mono_arch_cleanup (void)
{
	DeleteCriticalSection (&mini_arch_mutex);
}

/*
 * This function returns the optimizations supported on this cpu.
 */
guint32
mono_arch_cpu_optimizazions (guint32 *exclude_mask)
{
	int eax, ebx, ecx, edx;
	guint32 opts = 0;
	
	*exclude_mask = 0;
	/* Feature Flags function, flags returned in EDX. */
	if (cpuid (1, &eax, &ebx, &ecx, &edx)) {
		if (edx & (1 << 15)) {
			opts |= MONO_OPT_CMOV;
			if (edx & 1)
				opts |= MONO_OPT_FCMOV;
			else
				*exclude_mask |= MONO_OPT_FCMOV;
		} else
			*exclude_mask |= MONO_OPT_CMOV;
		if (edx & (1 << 26))
			opts |= MONO_OPT_SSE2;
		else
			*exclude_mask |= MONO_OPT_SSE2;

#ifdef MONO_ARCH_SIMD_INTRINSICS
		/*SIMD intrinsics require at least SSE2.*/
		if (!(opts & MONO_OPT_SSE2))
			*exclude_mask |= MONO_OPT_SIMD;
#endif
	}
	return opts;
}

/*
 * This function test for all SSE functions supported.
 *
 * Returns a bitmask corresponding to all supported versions.
 * 
 * TODO detect other versions like SSE4a.
 */
guint32
mono_arch_cpu_enumerate_simd_versions (void)
{
	int eax, ebx, ecx, edx;
	guint32 sse_opts = 0;

	if (cpuid (1, &eax, &ebx, &ecx, &edx)) {
		if (edx & (1 << 25))
			sse_opts |= 1 << SIMD_VERSION_SSE1;
		if (edx & (1 << 26))
			sse_opts |= 1 << SIMD_VERSION_SSE2;
		if (ecx & (1 << 0))
			sse_opts |= 1 << SIMD_VERSION_SSE3;
		if (ecx & (1 << 9))
			sse_opts |= 1 << SIMD_VERSION_SSSE3;
		if (ecx & (1 << 19))
			sse_opts |= 1 << SIMD_VERSION_SSE41;
		if (ecx & (1 << 20))
			sse_opts |= 1 << SIMD_VERSION_SSE42;
	}
	return sse_opts;	
}

/*
 * Determine whenever the trap whose info is in SIGINFO is caused by
 * integer overflow.
 */
gboolean
mono_arch_is_int_overflow (void *sigctx, void *info)
{
	MonoContext ctx;
	guint8* ip;

	mono_arch_sigctx_to_monoctx (sigctx, &ctx);

	ip = (guint8*)ctx.eip;

	if ((ip [0] == 0xf7) && (x86_modrm_mod (ip [1]) == 0x3) && (x86_modrm_reg (ip [1]) == 0x7)) {
		gint32 reg;

		/* idiv REG */
		switch (x86_modrm_rm (ip [1])) {
		case X86_EAX:
			reg = ctx.eax;
			break;
		case X86_ECX:
			reg = ctx.ecx;
			break;
		case X86_EDX:
			reg = ctx.edx;
			break;
		case X86_EBX:
			reg = ctx.ebx;
			break;
		case X86_ESI:
			reg = ctx.esi;
			break;
		case X86_EDI:
			reg = ctx.edi;
			break;
		default:
			g_assert_not_reached ();
			reg = -1;
		}

		if (reg == -1)
			return TRUE;
	}
			
	return FALSE;
}

GList *
mono_arch_get_allocatable_int_vars (MonoCompile *cfg)
{
	GList *vars = NULL;
	int i;

	for (i = 0; i < cfg->num_varinfo; i++) {
		MonoInst *ins = cfg->varinfo [i];
		MonoMethodVar *vmv = MONO_VARINFO (cfg, i);

		/* unused vars */
		if (vmv->range.first_use.abs_pos >= vmv->range.last_use.abs_pos)
			continue;

		if ((ins->flags & (MONO_INST_IS_DEAD|MONO_INST_VOLATILE|MONO_INST_INDIRECT)) || 
		    (ins->opcode != OP_LOCAL && ins->opcode != OP_ARG))
			continue;

		/* we dont allocate I1 to registers because there is no simply way to sign extend 
		 * 8bit quantities in caller saved registers on x86 */
		if (mono_is_regsize_var (ins->inst_vtype) && (ins->inst_vtype->type != MONO_TYPE_I1)) {
			g_assert (MONO_VARINFO (cfg, i)->reg == -1);
			g_assert (i == vmv->idx);
			vars = g_list_prepend (vars, vmv);
		}
	}

	vars = mono_varlist_sort (cfg, vars, 0);

	return vars;
}

GList *
mono_arch_get_global_int_regs (MonoCompile *cfg)
{
	GList *regs = NULL;

	/* we can use 3 registers for global allocation */
	regs = g_list_prepend (regs, (gpointer)X86_EBX);
	regs = g_list_prepend (regs, (gpointer)X86_ESI);
	regs = g_list_prepend (regs, (gpointer)X86_EDI);

	return regs;
}

/*
 * mono_arch_regalloc_cost:
 *
 *  Return the cost, in number of memory references, of the action of 
 * allocating the variable VMV into a register during global register
 * allocation.
 */
guint32
mono_arch_regalloc_cost (MonoCompile *cfg, MonoMethodVar *vmv)
{
	MonoInst *ins = cfg->varinfo [vmv->idx];

	if (cfg->method->save_lmf)
		/* The register is already saved */
		return (ins->opcode == OP_ARG) ? 1 : 0;
	else
		/* push+pop+possible load if it is an argument */
		return (ins->opcode == OP_ARG) ? 3 : 2;
}
 
/*
 * Set var information according to the calling convention. X86 version.
 * The locals var stuff should most likely be split in another method.
 */
void
mono_arch_allocate_vars (MonoCompile *cfg)
{
	MonoMethodSignature *sig;
	MonoMethodHeader *header;
	MonoInst *inst;
	guint32 locals_stack_size, locals_stack_align;
	int i, offset;
	gint32 *offsets;
	CallInfo *cinfo;

	header = mono_method_get_header (cfg->method);
	sig = mono_method_signature (cfg->method);

	cinfo = get_call_info (cfg->generic_sharing_context, cfg->mempool, sig, FALSE);

	cfg->frame_reg = X86_EBP;
	offset = 0;

	/* Reserve space to save LMF and caller saved registers */

	if (cfg->method->save_lmf) {
		offset += sizeof (MonoLMF);
	} else {
		if (cfg->used_int_regs & (1 << X86_EBX)) {
			offset += 4;
		}

		if (cfg->used_int_regs & (1 << X86_EDI)) {
			offset += 4;
		}

		if (cfg->used_int_regs & (1 << X86_ESI)) {
			offset += 4;
		}
	}

	switch (cinfo->ret.storage) {
	case ArgValuetypeInReg:
		/* Allocate a local to hold the result, the epilog will copy it to the correct place */
		offset += 8;
		cfg->ret->opcode = OP_REGOFFSET;
		cfg->ret->inst_basereg = X86_EBP;
		cfg->ret->inst_offset = - offset;
		break;
	default:
		break;
	}

	/* Allocate locals */
	offsets = mono_allocate_stack_slots (cfg, &locals_stack_size, &locals_stack_align);
	if (locals_stack_align) {
		offset += (locals_stack_align - 1);
		offset &= ~(locals_stack_align - 1);
	}
	/*
	 * EBP is at alignment 8 % MONO_ARCH_FRAME_ALIGNMENT, so if we
	 * have locals larger than 8 bytes we need to make sure that
	 * they have the appropriate offset.
	 */
	if (MONO_ARCH_FRAME_ALIGNMENT > 8 && locals_stack_align > 8)
		offset += MONO_ARCH_FRAME_ALIGNMENT - sizeof (gpointer) * 2;
	for (i = cfg->locals_start; i < cfg->num_varinfo; i++) {
		if (offsets [i] != -1) {
			MonoInst *inst = cfg->varinfo [i];
			inst->opcode = OP_REGOFFSET;
			inst->inst_basereg = X86_EBP;
			inst->inst_offset = - (offset + offsets [i]);
			//printf ("allocated local %d to ", i); mono_print_tree_nl (inst);
		}
	}
	offset += locals_stack_size;


	/*
	 * Allocate arguments+return value
	 */

	switch (cinfo->ret.storage) {
	case ArgOnStack:
		if (MONO_TYPE_ISSTRUCT (sig->ret)) {
			/* 
			 * In the new IR, the cfg->vret_addr variable represents the
			 * vtype return value.
			 */
			cfg->vret_addr->opcode = OP_REGOFFSET;
			cfg->vret_addr->inst_basereg = cfg->frame_reg;
			cfg->vret_addr->inst_offset = cinfo->ret.offset + ARGS_OFFSET;
			if (G_UNLIKELY (cfg->verbose_level > 1)) {
				printf ("vret_addr =");
				mono_print_ins (cfg->vret_addr);
			}
		} else {
			cfg->ret->opcode = OP_REGOFFSET;
			cfg->ret->inst_basereg = X86_EBP;
			cfg->ret->inst_offset = cinfo->ret.offset + ARGS_OFFSET;
		}
		break;
	case ArgValuetypeInReg:
		break;
	case ArgInIReg:
		cfg->ret->opcode = OP_REGVAR;
		cfg->ret->inst_c0 = cinfo->ret.reg;
		cfg->ret->dreg = cinfo->ret.reg;
		break;
	case ArgNone:
	case ArgOnFloatFpStack:
	case ArgOnDoubleFpStack:
		break;
	default:
		g_assert_not_reached ();
	}

	if (sig->call_convention == MONO_CALL_VARARG) {
		g_assert (cinfo->sig_cookie.storage == ArgOnStack);
		cfg->sig_cookie = cinfo->sig_cookie.offset + ARGS_OFFSET;
	}

	for (i = 0; i < sig->param_count + sig->hasthis; ++i) {
		ArgInfo *ainfo = &cinfo->args [i];
		inst = cfg->args [i];
		if (inst->opcode != OP_REGVAR) {
			inst->opcode = OP_REGOFFSET;
			inst->inst_basereg = X86_EBP;
		}
		inst->inst_offset = ainfo->offset + ARGS_OFFSET;
	}

	offset += (MONO_ARCH_FRAME_ALIGNMENT - 1);
	offset &= ~(MONO_ARCH_FRAME_ALIGNMENT - 1);

	cfg->stack_offset = offset;
}

void
mono_arch_create_vars (MonoCompile *cfg)
{
	MonoMethodSignature *sig;
	CallInfo *cinfo;

	sig = mono_method_signature (cfg->method);

	cinfo = get_call_info (cfg->generic_sharing_context, cfg->mempool, sig, FALSE);

	if (cinfo->ret.storage == ArgValuetypeInReg)
		cfg->ret_var_is_local = TRUE;
	if ((cinfo->ret.storage != ArgValuetypeInReg) && MONO_TYPE_ISSTRUCT (sig->ret)) {
		cfg->vret_addr = mono_compile_create_var (cfg, &mono_defaults.int_class->byval_arg, OP_ARG);
	}
}

/*
 * It is expensive to adjust esp for each individual fp argument pushed on the stack
 * so we try to do it just once when we have multiple fp arguments in a row.
 * We don't use this mechanism generally because for int arguments the generated code
 * is slightly bigger and new generation cpus optimize away the dependency chains
 * created by push instructions on the esp value.
 * fp_arg_setup is the first argument in the execution sequence where the esp register
 * is modified.
 */
static G_GNUC_UNUSED int
collect_fp_stack_space (MonoMethodSignature *sig, int start_arg, int *fp_arg_setup)
{
	int fp_space = 0;
	MonoType *t;

	for (; start_arg < sig->param_count; ++start_arg) {
		t = mini_type_get_underlying_type (NULL, sig->params [start_arg]);
		if (!t->byref && t->type == MONO_TYPE_R8) {
			fp_space += sizeof (double);
			*fp_arg_setup = start_arg;
		} else {
			break;
		}
	}
	return fp_space;
}

static void
emit_sig_cookie (MonoCompile *cfg, MonoCallInst *call, CallInfo *cinfo)
{
	MonoMethodSignature *tmp_sig;

	/* FIXME: Add support for signature tokens to AOT */
	cfg->disable_aot = TRUE;

	/*
	 * mono_ArgIterator_Setup assumes the signature cookie is 
	 * passed first and all the arguments which were before it are
	 * passed on the stack after the signature. So compensate by 
	 * passing a different signature.
	 */
	tmp_sig = mono_metadata_signature_dup (call->signature);
	tmp_sig->param_count -= call->signature->sentinelpos;
	tmp_sig->sentinelpos = 0;
	memcpy (tmp_sig->params, call->signature->params + call->signature->sentinelpos, tmp_sig->param_count * sizeof (MonoType*));

	MONO_EMIT_NEW_BIALU_IMM (cfg, OP_X86_PUSH_IMM, -1, -1, tmp_sig);
}

void
mono_arch_emit_call (MonoCompile *cfg, MonoCallInst *call)
{
	MonoInst *arg, *in;
	MonoMethodSignature *sig;
	int i, n;
	CallInfo *cinfo;
	int sentinelpos = 0;

	sig = call->signature;
	n = sig->param_count + sig->hasthis;

	cinfo = get_call_info (cfg->generic_sharing_context, cfg->mempool, sig, FALSE);

	if (!sig->pinvoke && (sig->call_convention == MONO_CALL_VARARG))
		sentinelpos = sig->sentinelpos + (sig->hasthis ? 1 : 0);

	if (cinfo->need_stack_align) {
		MONO_INST_NEW (cfg, arg, OP_SUB_IMM);
		arg->dreg = X86_ESP;
		arg->sreg1 = X86_ESP;
		arg->inst_imm = cinfo->stack_align_amount;
		MONO_ADD_INS (cfg->cbb, arg);
	}

	if (sig->ret && MONO_TYPE_ISSTRUCT (sig->ret)) {
		if (cinfo->ret.storage == ArgValuetypeInReg) {
			/*
			 * Tell the JIT to use a more efficient calling convention: call using
			 * OP_CALL, compute the result location after the call, and save the 
			 * result there.
			 */
			call->vret_in_reg = TRUE;
			if (call->vret_var)
				NULLIFY_INS (call->vret_var);
		}
	}

	/* Handle the case where there are no implicit arguments */
	if (!sig->pinvoke && (sig->call_convention == MONO_CALL_VARARG) && (n == sentinelpos)) {
		emit_sig_cookie (cfg, call, cinfo);
	}

	/* Arguments are pushed in the reverse order */
	for (i = n - 1; i >= 0; i --) {
		ArgInfo *ainfo = cinfo->args + i;
		MonoType *t;

		if (i >= sig->hasthis)
			t = sig->params [i - sig->hasthis];
		else
			t = &mono_defaults.int_class->byval_arg;
		t = mini_type_get_underlying_type (cfg->generic_sharing_context, t);

		MONO_INST_NEW (cfg, arg, OP_X86_PUSH);

		in = call->args [i];
		arg->cil_code = in->cil_code;
		arg->sreg1 = in->dreg;
		arg->type = in->type;

		g_assert (in->dreg != -1);

		if ((i >= sig->hasthis) && (MONO_TYPE_ISSTRUCT(t))) {
			guint32 align;
			guint32 size;

			g_assert (in->klass);

			if (t->type == MONO_TYPE_TYPEDBYREF) {
				size = sizeof (MonoTypedRef);
				align = sizeof (gpointer);
			}
			else {
				size = mini_type_stack_size_full (cfg->generic_sharing_context, &in->klass->byval_arg, &align, sig->pinvoke);
			}

			if (size > 0) {
				arg->opcode = OP_OUTARG_VT;
				arg->sreg1 = in->dreg;
				arg->klass = in->klass;
				arg->backend.size = size;

				MONO_ADD_INS (cfg->cbb, arg);
			}
		}
		else {
			switch (ainfo->storage) {
			case ArgOnStack:
				arg->opcode = OP_X86_PUSH;
				if (!t->byref) {
					if (t->type == MONO_TYPE_R4) {
						MONO_EMIT_NEW_BIALU_IMM (cfg, OP_SUB_IMM, X86_ESP, X86_ESP, 4);
						arg->opcode = OP_STORER4_MEMBASE_REG;
						arg->inst_destbasereg = X86_ESP;
						arg->inst_offset = 0;
					} else if (t->type == MONO_TYPE_R8) {
						MONO_EMIT_NEW_BIALU_IMM (cfg, OP_SUB_IMM, X86_ESP, X86_ESP, 8);
						arg->opcode = OP_STORER8_MEMBASE_REG;
						arg->inst_destbasereg = X86_ESP;
						arg->inst_offset = 0;
					} else if (t->type == MONO_TYPE_I8 || t->type == MONO_TYPE_U8) {
						arg->sreg1 ++;
						MONO_EMIT_NEW_UNALU (cfg, OP_X86_PUSH, -1, in->dreg + 2);
					}
				}
				break;
			default:
				g_assert_not_reached ();
			}
			
			MONO_ADD_INS (cfg->cbb, arg);
		}

		if (!sig->pinvoke && (sig->call_convention == MONO_CALL_VARARG) && (i == sentinelpos)) {
			/* Emit the signature cookie just before the implicit arguments */
			emit_sig_cookie (cfg, call, cinfo);
		}
	}

	if (sig->ret && MONO_TYPE_ISSTRUCT (sig->ret)) {
		MonoInst *vtarg;

		if (cinfo->ret.storage == ArgValuetypeInReg) {
			/* Already done */
		}
		else if (cinfo->ret.storage == ArgInIReg) {
			NOT_IMPLEMENTED;
			/* The return address is passed in a register */
			MONO_INST_NEW (cfg, vtarg, OP_MOVE);
			vtarg->sreg1 = call->inst.dreg;
			vtarg->dreg = mono_alloc_ireg (cfg);
			MONO_ADD_INS (cfg->cbb, vtarg);
				
			mono_call_inst_add_outarg_reg (cfg, call, vtarg->dreg, cinfo->ret.reg, FALSE);
		} else {
			MonoInst *vtarg;
			MONO_INST_NEW (cfg, vtarg, OP_X86_PUSH);
			vtarg->type = STACK_MP;
			vtarg->sreg1 = call->vret_var->dreg;
			MONO_ADD_INS (cfg->cbb, vtarg);
		}

		/* if the function returns a struct, the called method already does a ret $0x4 */
		cinfo->stack_usage -= 4;
	}

	call->stack_usage = cinfo->stack_usage;
}

void
mono_arch_emit_outarg_vt (MonoCompile *cfg, MonoInst *ins, MonoInst *src)
{
	MonoInst *arg;
	int size = ins->backend.size;

	if (size <= 4) {
		MONO_INST_NEW (cfg, arg, OP_X86_PUSH_MEMBASE);
		arg->sreg1 = src->dreg;

		MONO_ADD_INS (cfg->cbb, arg);
	} else if (size <= 20) {	
		MONO_EMIT_NEW_BIALU_IMM (cfg, OP_SUB_IMM, X86_ESP, X86_ESP, ALIGN_TO (size, 4));
		mini_emit_memcpy (cfg, X86_ESP, 0, src->dreg, 0, size, 4);
	} else {
		MONO_INST_NEW (cfg, arg, OP_X86_PUSH_OBJ);
		arg->inst_basereg = src->dreg;
		arg->inst_offset = 0;
		arg->inst_imm = size;
					
		MONO_ADD_INS (cfg->cbb, arg);
	}
}

void
mono_arch_emit_setret (MonoCompile *cfg, MonoMethod *method, MonoInst *val)
{
	MonoType *ret = mini_type_get_underlying_type (cfg->generic_sharing_context, mono_method_signature (method)->ret);

	if (!ret->byref) {
		if (ret->type == MONO_TYPE_R4) {
			/* Nothing to do */
			return;
		} else if (ret->type == MONO_TYPE_R8) {
			/* Nothing to do */
			return;
		} else if (ret->type == MONO_TYPE_I8 || ret->type == MONO_TYPE_U8) {
			MONO_EMIT_NEW_UNALU (cfg, OP_MOVE, X86_EAX, val->dreg + 1);
			MONO_EMIT_NEW_UNALU (cfg, OP_MOVE, X86_EDX, val->dreg + 2);
			return;
		}
	}
			
	MONO_EMIT_NEW_UNALU (cfg, OP_MOVE, cfg->ret->dreg, val->dreg);
}

/*
 * Allow tracing to work with this interface (with an optional argument)
 */
void*
mono_arch_instrument_prolog (MonoCompile *cfg, void *func, void *p, gboolean enable_arguments)
{
	guchar *code = p;

	g_assert (MONO_ARCH_FRAME_ALIGNMENT >= 8);
	x86_alu_reg_imm (code, X86_SUB, X86_ESP, MONO_ARCH_FRAME_ALIGNMENT - 8);

	/* if some args are passed in registers, we need to save them here */
	x86_push_reg (code, X86_EBP);

	if (cfg->compile_aot) {
		x86_push_imm (code, cfg->method);
		x86_mov_reg_imm (code, X86_EAX, func);
		x86_call_reg (code, X86_EAX);
	} else {
		mono_add_patch_info (cfg, code-cfg->native_code, MONO_PATCH_INFO_METHODCONST, cfg->method);
		x86_push_imm (code, cfg->method);
		mono_add_patch_info (cfg, code-cfg->native_code, MONO_PATCH_INFO_ABS, func);
		x86_call_code (code, 0);
	}
	x86_alu_reg_imm (code, X86_ADD, X86_ESP, MONO_ARCH_FRAME_ALIGNMENT);

	return code;
}

enum {
	SAVE_NONE,
	SAVE_STRUCT,
	SAVE_EAX,
	SAVE_EAX_EDX,
	SAVE_FP
};

void*
mono_arch_instrument_epilog (MonoCompile *cfg, void *func, void *p, gboolean enable_arguments)
{
	guchar *code = p;
	int arg_size = 0, save_mode = SAVE_NONE;
	MonoMethod *method = cfg->method;
	
	switch (mini_type_get_underlying_type (cfg->generic_sharing_context, mono_method_signature (method)->ret)->type) {
	case MONO_TYPE_VOID:
		/* special case string .ctor icall */
		if (strcmp (".ctor", method->name) && method->klass == mono_defaults.string_class)
			save_mode = SAVE_EAX;
		else
			save_mode = SAVE_NONE;
		break;
	case MONO_TYPE_I8:
	case MONO_TYPE_U8:
		save_mode = SAVE_EAX_EDX;
		break;
	case MONO_TYPE_R4:
	case MONO_TYPE_R8:
		save_mode = SAVE_FP;
		break;
	case MONO_TYPE_GENERICINST:
		if (!mono_type_generic_inst_is_valuetype (mono_method_signature (method)->ret)) {
			save_mode = SAVE_EAX;
			break;
		}
		/* Fall through */
	case MONO_TYPE_VALUETYPE:
		save_mode = SAVE_STRUCT;
		break;
	default:
		save_mode = SAVE_EAX;
		break;
	}

	switch (save_mode) {
	case SAVE_EAX_EDX:
		x86_push_reg (code, X86_EDX);
		x86_push_reg (code, X86_EAX);
		if (enable_arguments) {
			x86_push_reg (code, X86_EDX);
			x86_push_reg (code, X86_EAX);
			arg_size = 8;
		}
		break;
	case SAVE_EAX:
		x86_push_reg (code, X86_EAX);
		if (enable_arguments) {
			x86_push_reg (code, X86_EAX);
			arg_size = 4;
		}
		break;
	case SAVE_FP:
		x86_alu_reg_imm (code, X86_SUB, X86_ESP, 8);
		x86_fst_membase (code, X86_ESP, 0, TRUE, TRUE);
		if (enable_arguments) {
			x86_alu_reg_imm (code, X86_SUB, X86_ESP, 8);
			x86_fst_membase (code, X86_ESP, 0, TRUE, TRUE);
			arg_size = 8;
		}
		break;
	case SAVE_STRUCT:
		if (enable_arguments) {
			x86_push_membase (code, X86_EBP, 8);
			arg_size = 4;
		}
		break;
	case SAVE_NONE:
	default:
		break;
	}

	if (cfg->compile_aot) {
		x86_push_imm (code, method);
		x86_mov_reg_imm (code, X86_EAX, func);
		x86_call_reg (code, X86_EAX);
	} else {
		mono_add_patch_info (cfg, code-cfg->native_code, MONO_PATCH_INFO_METHODCONST, method);
		x86_push_imm (code, method);
		mono_add_patch_info (cfg, code-cfg->native_code, MONO_PATCH_INFO_ABS, func);
		x86_call_code (code, 0);
	}
	x86_alu_reg_imm (code, X86_ADD, X86_ESP, arg_size + 4);

	switch (save_mode) {
	case SAVE_EAX_EDX:
		x86_pop_reg (code, X86_EAX);
		x86_pop_reg (code, X86_EDX);
		break;
	case SAVE_EAX:
		x86_pop_reg (code, X86_EAX);
		break;
	case SAVE_FP:
		x86_fld_membase (code, X86_ESP, 0, TRUE);
		x86_alu_reg_imm (code, X86_ADD, X86_ESP, 8);
		break;
	case SAVE_NONE:
	default:
		break;
	}

	return code;
}

#define EMIT_COND_BRANCH(ins,cond,sign) \
if (ins->flags & MONO_INST_BRLABEL) { \
        if (ins->inst_i0->inst_c0) { \
	        x86_branch (code, cond, cfg->native_code + ins->inst_i0->inst_c0, sign); \
        } else { \
	        mono_add_patch_info (cfg, code - cfg->native_code, MONO_PATCH_INFO_LABEL, ins->inst_i0); \
	        if ((cfg->opt & MONO_OPT_BRANCH) && \
                    x86_is_imm8 (ins->inst_i0->inst_c1 - cpos)) \
		        x86_branch8 (code, cond, 0, sign); \
                else \
	                x86_branch32 (code, cond, 0, sign); \
        } \
} else { \
        if (ins->inst_true_bb->native_offset) { \
	        x86_branch (code, cond, cfg->native_code + ins->inst_true_bb->native_offset, sign); \
        } else { \
	        mono_add_patch_info (cfg, code - cfg->native_code, MONO_PATCH_INFO_BB, ins->inst_true_bb); \
	        if ((cfg->opt & MONO_OPT_BRANCH) && \
                    x86_is_imm8 (ins->inst_true_bb->max_offset - cpos)) \
		        x86_branch8 (code, cond, 0, sign); \
                else \
	                x86_branch32 (code, cond, 0, sign); \
        } \
}

/*  
 *	Emit an exception if condition is fail and
 *  if possible do a directly branch to target 
 */
#define EMIT_COND_SYSTEM_EXCEPTION(cond,signed,exc_name)            \
	do {                                                        \
		MonoInst *tins = mono_branch_optimize_exception_target (cfg, bb, exc_name); \
		if (tins == NULL) {										\
			mono_add_patch_info (cfg, code - cfg->native_code,   \
					MONO_PATCH_INFO_EXC, exc_name);  \
			x86_branch32 (code, cond, 0, signed);               \
		} else {	\
			EMIT_COND_BRANCH (tins, cond, signed);	\
		}			\
	} while (0); 

#define EMIT_FPCOMPARE(code) do { \
	x86_fcompp (code); \
	x86_fnstsw (code); \
} while (0); 


static guint8*
emit_call (MonoCompile *cfg, guint8 *code, guint32 patch_type, gconstpointer data)
{
	mono_add_patch_info (cfg, code - cfg->native_code, patch_type, data);
	x86_call_code (code, 0);

	return code;
}

#define INST_IGNORES_CFLAGS(opcode) (!(((opcode) == OP_ADC) || ((opcode) == OP_IADC) || ((opcode) == OP_ADC_IMM) || ((opcode) == OP_IADC_IMM) || ((opcode) == OP_SBB) || ((opcode) == OP_ISBB) || ((opcode) == OP_SBB_IMM) || ((opcode) == OP_ISBB_IMM)))

/*
 * mono_peephole_pass_1:
 *
 *   Perform peephole opts which should/can be performed before local regalloc
 */
void
mono_arch_peephole_pass_1 (MonoCompile *cfg, MonoBasicBlock *bb)
{
	MonoInst *ins, *n;

	MONO_BB_FOR_EACH_INS_SAFE (bb, n, ins) {
		MonoInst *last_ins = ins->prev;

		switch (ins->opcode) {
		case OP_IADD_IMM:
		case OP_ADD_IMM:
			if ((ins->sreg1 < MONO_MAX_IREGS) && (ins->dreg >= MONO_MAX_IREGS)) {
				/* 
				 * X86_LEA is like ADD, but doesn't have the
				 * sreg1==dreg restriction.
				 */
				ins->opcode = OP_X86_LEA_MEMBASE;
				ins->inst_basereg = ins->sreg1;
			} else if ((ins->inst_imm == 1) && (ins->dreg == ins->sreg1))
				ins->opcode = OP_X86_INC_REG;
			break;
		case OP_SUB_IMM:
		case OP_ISUB_IMM:
			if ((ins->sreg1 < MONO_MAX_IREGS) && (ins->dreg >= MONO_MAX_IREGS)) {
				ins->opcode = OP_X86_LEA_MEMBASE;
				ins->inst_basereg = ins->sreg1;
				ins->inst_imm = -ins->inst_imm;
			} else if ((ins->inst_imm == 1) && (ins->dreg == ins->sreg1))
				ins->opcode = OP_X86_DEC_REG;
			break;
		case OP_COMPARE_IMM:
		case OP_ICOMPARE_IMM:
			/* OP_COMPARE_IMM (reg, 0) 
			 * --> 
			 * OP_X86_TEST_NULL (reg) 
			 */
			if (!ins->inst_imm)
				ins->opcode = OP_X86_TEST_NULL;
			break;
		case OP_X86_COMPARE_MEMBASE_IMM:
			/* 
			 * OP_STORE_MEMBASE_REG reg, offset(basereg)
			 * OP_X86_COMPARE_MEMBASE_IMM offset(basereg), imm
			 * -->
			 * OP_STORE_MEMBASE_REG reg, offset(basereg)
			 * OP_COMPARE_IMM reg, imm
			 *
			 * Note: if imm = 0 then OP_COMPARE_IMM replaced with OP_X86_TEST_NULL
			 */
			if (last_ins && (last_ins->opcode == OP_STOREI4_MEMBASE_REG) &&
			    ins->inst_basereg == last_ins->inst_destbasereg &&
			    ins->inst_offset == last_ins->inst_offset) {
					ins->opcode = OP_COMPARE_IMM;
					ins->sreg1 = last_ins->sreg1;

					/* check if we can remove cmp reg,0 with test null */
					if (!ins->inst_imm)
						ins->opcode = OP_X86_TEST_NULL;
				}

			break;			
		case OP_X86_PUSH_MEMBASE:
			if (last_ins && (last_ins->opcode == OP_STOREI4_MEMBASE_REG ||
				         last_ins->opcode == OP_STORE_MEMBASE_REG) &&
			    ins->inst_basereg == last_ins->inst_destbasereg &&
			    ins->inst_offset == last_ins->inst_offset) {
				    ins->opcode = OP_X86_PUSH;
				    ins->sreg1 = last_ins->sreg1;
			}
			break;
		}

		mono_peephole_ins (bb, ins);
	}
}

void
mono_arch_peephole_pass_2 (MonoCompile *cfg, MonoBasicBlock *bb)
{
	MonoInst *ins, *n;

	MONO_BB_FOR_EACH_INS_SAFE (bb, n, ins) {
		switch (ins->opcode) {
		case OP_ICONST:
			/* reg = 0 -> XOR (reg, reg) */
			/* XOR sets cflags on x86, so we cant do it always */
			if (ins->inst_c0 == 0 && (!ins->next || (ins->next && INST_IGNORES_CFLAGS (ins->next->opcode)))) {
				MonoInst *ins2;

				ins->opcode = OP_IXOR;
				ins->sreg1 = ins->dreg;
				ins->sreg2 = ins->dreg;

				/* 
				 * Convert succeeding STORE_MEMBASE_IMM 0 ins to STORE_MEMBASE_REG 
				 * since it takes 3 bytes instead of 7.
				 */
				for (ins2 = ins->next; ins2; ins2 = ins2->next) {
					if ((ins2->opcode == OP_STORE_MEMBASE_IMM) && (ins2->inst_imm == 0)) {
						ins2->opcode = OP_STORE_MEMBASE_REG;
						ins2->sreg1 = ins->dreg;
					}
					else if ((ins2->opcode == OP_STOREI4_MEMBASE_IMM) && (ins2->inst_imm == 0)) {
						ins2->opcode = OP_STOREI4_MEMBASE_REG;
						ins2->sreg1 = ins->dreg;
					}
					else if ((ins2->opcode == OP_STOREI1_MEMBASE_IMM) || (ins2->opcode == OP_STOREI2_MEMBASE_IMM)) {
						/* Continue iteration */
					}
					else
						break;
				}
			}
			break;
		case OP_IADD_IMM:
		case OP_ADD_IMM:
			if ((ins->inst_imm == 1) && (ins->dreg == ins->sreg1))
				ins->opcode = OP_X86_INC_REG;
			break;
		case OP_ISUB_IMM:
		case OP_SUB_IMM:
			if ((ins->inst_imm == 1) && (ins->dreg == ins->sreg1))
				ins->opcode = OP_X86_DEC_REG;
			break;
		}

		mono_peephole_ins (bb, ins);
	}
}

/*
 * mono_arch_lowering_pass:
 *
 *  Converts complex opcodes into simpler ones so that each IR instruction
 * corresponds to one machine instruction.
 */
void
mono_arch_lowering_pass (MonoCompile *cfg, MonoBasicBlock *bb)
{
	MonoInst *ins, *next;

	/*
	 * FIXME: Need to add more instructions, but the current machine 
	 * description can't model some parts of the composite instructions like
	 * cdq.
	 */
	MONO_BB_FOR_EACH_INS_SAFE (bb, next, ins) {
		switch (ins->opcode) {
		case OP_IREM_IMM:
		case OP_IDIV_IMM:
		case OP_IDIV_UN_IMM:
		case OP_IREM_UN_IMM:
			/* 
			 * Keep the cases where we could generated optimized code, otherwise convert
			 * to the non-imm variant.
			 */
			if ((ins->opcode == OP_IREM_IMM) && mono_is_power_of_two (ins->inst_imm) >= 0)
				break;
			mono_decompose_op_imm (cfg, bb, ins);
			break;
		default:
			break;
		}
	}

	bb->max_vreg = cfg->next_vreg;
}

static const int 
branch_cc_table [] = {
	X86_CC_EQ, X86_CC_GE, X86_CC_GT, X86_CC_LE, X86_CC_LT,
	X86_CC_NE, X86_CC_GE, X86_CC_GT, X86_CC_LE, X86_CC_LT,
	X86_CC_O, X86_CC_NO, X86_CC_C, X86_CC_NC
};

/* Maps CMP_... constants to X86_CC_... constants */
static const int
cc_table [] = {
	X86_CC_EQ, X86_CC_NE, X86_CC_LE, X86_CC_GE, X86_CC_LT, X86_CC_GT,
	X86_CC_LE, X86_CC_GE, X86_CC_LT, X86_CC_GT
};

static const int
cc_signed_table [] = {
	TRUE, TRUE, TRUE, TRUE, TRUE, TRUE,
	FALSE, FALSE, FALSE, FALSE
};

static unsigned char*
emit_float_to_int (MonoCompile *cfg, guchar *code, int dreg, int size, gboolean is_signed)
{
#define XMM_TEMP_REG 0
	/*This SSE2 optimization must not be done which OPT_SIMD in place as it clobbers xmm0.*/
	/*The xmm pass decomposes OP_FCONV_ ops anyway anyway.*/
	if (cfg->opt & MONO_OPT_SSE2 && size < 8 && !(cfg->opt & MONO_OPT_SIMD)) {
		/* optimize by assigning a local var for this use so we avoid
		 * the stack manipulations */
		x86_alu_reg_imm (code, X86_SUB, X86_ESP, 8);
		x86_fst_membase (code, X86_ESP, 0, TRUE, TRUE);
		x86_movsd_reg_membase (code, XMM_TEMP_REG, X86_ESP, 0);
		x86_cvttsd2si (code, dreg, XMM_TEMP_REG);
		x86_alu_reg_imm (code, X86_ADD, X86_ESP, 8);
		if (size == 1)
			x86_widen_reg (code, dreg, dreg, is_signed, FALSE);
		else if (size == 2)
			x86_widen_reg (code, dreg, dreg, is_signed, TRUE);
		return code;
	}
	x86_alu_reg_imm (code, X86_SUB, X86_ESP, 4);
	x86_fnstcw_membase(code, X86_ESP, 0);
	x86_mov_reg_membase (code, dreg, X86_ESP, 0, 2);
	x86_alu_reg_imm (code, X86_OR, dreg, 0xc00);
	x86_mov_membase_reg (code, X86_ESP, 2, dreg, 2);
	x86_fldcw_membase (code, X86_ESP, 2);
	if (size == 8) {
		x86_alu_reg_imm (code, X86_SUB, X86_ESP, 8);
		x86_fist_pop_membase (code, X86_ESP, 0, TRUE);
		x86_pop_reg (code, dreg);
		/* FIXME: need the high register 
		 * x86_pop_reg (code, dreg_high);
		 */
	} else {
		x86_push_reg (code, X86_EAX); // SP = SP - 4
		x86_fist_pop_membase (code, X86_ESP, 0, FALSE);
		x86_pop_reg (code, dreg);
	}
	x86_fldcw_membase (code, X86_ESP, 0);
	x86_alu_reg_imm (code, X86_ADD, X86_ESP, 4);

	if (size == 1)
		x86_widen_reg (code, dreg, dreg, is_signed, FALSE);
	else if (size == 2)
		x86_widen_reg (code, dreg, dreg, is_signed, TRUE);
	return code;
}

static unsigned char*
mono_emit_stack_alloc (guchar *code, MonoInst* tree)
{
	int sreg = tree->sreg1;
	int need_touch = FALSE;

#if defined(PLATFORM_WIN32) || defined(MONO_ARCH_SIGSEGV_ON_ALTSTACK)
	need_touch = TRUE;
#endif

	if (need_touch) {
		guint8* br[5];

		/*
		 * Under Windows:
		 * If requested stack size is larger than one page,
		 * perform stack-touch operation
		 */
		/*
		 * Generate stack probe code.
		 * Under Windows, it is necessary to allocate one page at a time,
		 * "touching" stack after each successful sub-allocation. This is
		 * because of the way stack growth is implemented - there is a
		 * guard page before the lowest stack page that is currently commited.
		 * Stack normally grows sequentially so OS traps access to the
		 * guard page and commits more pages when needed.
		 */
		x86_test_reg_imm (code, sreg, ~0xFFF);
		br[0] = code; x86_branch8 (code, X86_CC_Z, 0, FALSE);

		br[2] = code; /* loop */
		x86_alu_reg_imm (code, X86_SUB, X86_ESP, 0x1000);
		x86_test_membase_reg (code, X86_ESP, 0, X86_ESP);

		/* 
		 * By the end of the loop, sreg2 is smaller than 0x1000, so the init routine
		 * that follows only initializes the last part of the area.
		 */
		/* Same as the init code below with size==0x1000 */
		if (tree->flags & MONO_INST_INIT) {
			x86_push_reg (code, X86_EAX);
			x86_push_reg (code, X86_ECX);
			x86_push_reg (code, X86_EDI);
			x86_mov_reg_imm (code, X86_ECX, (0x1000 >> 2));
			x86_alu_reg_reg (code, X86_XOR, X86_EAX, X86_EAX);				
			x86_lea_membase (code, X86_EDI, X86_ESP, 12);
			x86_cld (code);
			x86_prefix (code, X86_REP_PREFIX);
			x86_stosl (code);
			x86_pop_reg (code, X86_EDI);
			x86_pop_reg (code, X86_ECX);
			x86_pop_reg (code, X86_EAX);
		}

		x86_alu_reg_imm (code, X86_SUB, sreg, 0x1000);
		x86_alu_reg_imm (code, X86_CMP, sreg, 0x1000);
		br[3] = code; x86_branch8 (code, X86_CC_AE, 0, FALSE);
		x86_patch (br[3], br[2]);
		x86_test_reg_reg (code, sreg, sreg);
		br[4] = code; x86_branch8 (code, X86_CC_Z, 0, FALSE);
		x86_alu_reg_reg (code, X86_SUB, X86_ESP, sreg);

		br[1] = code; x86_jump8 (code, 0);

		x86_patch (br[0], code);
		x86_alu_reg_reg (code, X86_SUB, X86_ESP, sreg);
		x86_patch (br[1], code);
		x86_patch (br[4], code);
	}
	else
		x86_alu_reg_reg (code, X86_SUB, X86_ESP, tree->sreg1);

	if (tree->flags & MONO_INST_INIT) {
		int offset = 0;
		if (tree->dreg != X86_EAX && sreg != X86_EAX) {
			x86_push_reg (code, X86_EAX);
			offset += 4;
		}
		if (tree->dreg != X86_ECX && sreg != X86_ECX) {
			x86_push_reg (code, X86_ECX);
			offset += 4;
		}
		if (tree->dreg != X86_EDI && sreg != X86_EDI) {
			x86_push_reg (code, X86_EDI);
			offset += 4;
		}
		
		x86_shift_reg_imm (code, X86_SHR, sreg, 2);
		if (sreg != X86_ECX)
			x86_mov_reg_reg (code, X86_ECX, sreg, 4);
		x86_alu_reg_reg (code, X86_XOR, X86_EAX, X86_EAX);
				
		x86_lea_membase (code, X86_EDI, X86_ESP, offset);
		x86_cld (code);
		x86_prefix (code, X86_REP_PREFIX);
		x86_stosl (code);
		
		if (tree->dreg != X86_EDI && sreg != X86_EDI)
			x86_pop_reg (code, X86_EDI);
		if (tree->dreg != X86_ECX && sreg != X86_ECX)
			x86_pop_reg (code, X86_ECX);
		if (tree->dreg != X86_EAX && sreg != X86_EAX)
			x86_pop_reg (code, X86_EAX);
	}
	return code;
}


static guint8*
emit_move_return_value (MonoCompile *cfg, MonoInst *ins, guint8 *code)
{
	/* Move return value to the target register */
	switch (ins->opcode) {
	case OP_CALL:
	case OP_CALL_REG:
	case OP_CALL_MEMBASE:
		if (ins->dreg != X86_EAX)
			x86_mov_reg_reg (code, ins->dreg, X86_EAX, 4);
		break;
	case OP_FCALL: {
		MonoCallInst *call = (MonoCallInst*)ins;
		if (call->method && !mono_method_signature (call->method)->ret->byref && mono_method_signature (call->method)->ret->type == MONO_TYPE_R4) {
			/* Avoid some precision issues by saving/reloading the return value */
			x86_alu_reg_imm (code, X86_SUB, X86_ESP, 8);
			x86_fst_membase (code, X86_ESP, 0, FALSE, TRUE);
			x86_fld_membase (code, X86_ESP, 0, FALSE);
			x86_alu_reg_imm (code, X86_ADD, X86_ESP, 8);
		}
		break;
	}
	default:
		break;
	}

	return code;
}

/*
 * mono_x86_emit_tls_get:
 * @code: buffer to store code to
 * @dreg: hard register where to place the result
 * @tls_offset: offset info
 *
 * mono_x86_emit_tls_get emits in @code the native code that puts in
 * the dreg register the item in the thread local storage identified
 * by tls_offset.
 *
 * Returns: a pointer to the end of the stored code
 */
guint8*
mono_x86_emit_tls_get (guint8* code, int dreg, int tls_offset)
{
#ifdef PLATFORM_WIN32
	/* 
	 * See the Under the Hood article in the May 1996 issue of Microsoft Systems 
	 * Journal and/or a disassembly of the TlsGet () function.
	 */
	g_assert (tls_offset < 64);
	x86_prefix (code, X86_FS_PREFIX);
	x86_mov_reg_mem (code, dreg, 0x18, 4);
	/* Dunno what this does but TlsGetValue () contains it */
	x86_alu_membase_imm (code, X86_AND, dreg, 0x34, 0);
	x86_mov_reg_membase (code, dreg, dreg, 3600 + (tls_offset * 4), 4);
#else
	if (optimize_for_xen) {
		x86_prefix (code, X86_GS_PREFIX);
		x86_mov_reg_mem (code, dreg, 0, 4);
		x86_mov_reg_membase (code, dreg, dreg, tls_offset, 4);
	} else {
		x86_prefix (code, X86_GS_PREFIX);
		x86_mov_reg_mem (code, dreg, tls_offset, 4);
	}
#endif
	return code;
}

/*
 * emit_load_volatile_arguments:
 *
 *  Load volatile arguments from the stack to the original input registers.
 * Required before a tail call.
 */
static guint8*
emit_load_volatile_arguments (MonoCompile *cfg, guint8 *code)
{
	MonoMethod *method = cfg->method;
	MonoMethodSignature *sig;
	MonoInst *inst;
	CallInfo *cinfo;
	guint32 i;

	/* FIXME: Generate intermediate code instead */

	sig = mono_method_signature (method);

	cinfo = get_call_info (cfg->generic_sharing_context, cfg->mempool, sig, FALSE);
	
	/* This is the opposite of the code in emit_prolog */

	for (i = 0; i < sig->param_count + sig->hasthis; ++i) {
		ArgInfo *ainfo = cinfo->args + i;
		MonoType *arg_type;
		inst = cfg->args [i];

		if (sig->hasthis && (i == 0))
			arg_type = &mono_defaults.object_class->byval_arg;
		else
			arg_type = sig->params [i - sig->hasthis];

		/*
		 * On x86, the arguments are either in their original stack locations, or in
		 * global regs.
		 */
		if (inst->opcode == OP_REGVAR) {
			g_assert (ainfo->storage == ArgOnStack);
			
			x86_mov_membase_reg (code, X86_EBP, inst->inst_offset, inst->dreg, 4);
		}
	}

	return code;
}

#define REAL_PRINT_REG(text,reg) \
mono_assert (reg >= 0); \
x86_push_reg (code, X86_EAX); \
x86_push_reg (code, X86_EDX); \
x86_push_reg (code, X86_ECX); \
x86_push_reg (code, reg); \
x86_push_imm (code, reg); \
x86_push_imm (code, text " %d %p\n"); \
x86_mov_reg_imm (code, X86_EAX, printf); \
x86_call_reg (code, X86_EAX); \
x86_alu_reg_imm (code, X86_ADD, X86_ESP, 3*4); \
x86_pop_reg (code, X86_ECX); \
x86_pop_reg (code, X86_EDX); \
x86_pop_reg (code, X86_EAX);

/* benchmark and set based on cpu */
#define LOOP_ALIGNMENT 8
#define bb_is_loop_start(bb) ((bb)->loop_body_start && (bb)->nesting)

void
mono_arch_output_basic_block (MonoCompile *cfg, MonoBasicBlock *bb)
{
	MonoInst *ins;
	MonoCallInst *call;
	guint offset;
	guint8 *code = cfg->native_code + cfg->code_len;
	int max_len, cpos;

	if (cfg->opt & MONO_OPT_LOOP) {
		int pad, align = LOOP_ALIGNMENT;
		/* set alignment depending on cpu */
		if (bb_is_loop_start (bb) && (pad = (cfg->code_len & (align - 1)))) {
			pad = align - pad;
			/*g_print ("adding %d pad at %x to loop in %s\n", pad, cfg->code_len, cfg->method->name);*/
			x86_padding (code, pad);
			cfg->code_len += pad;
			bb->native_offset = cfg->code_len;
		}
	}

	if (cfg->verbose_level > 2)
		g_print ("Basic block %d starting at offset 0x%x\n", bb->block_num, bb->native_offset);

	cpos = bb->max_offset;

	if (cfg->prof_options & MONO_PROFILE_COVERAGE) {
		MonoProfileCoverageInfo *cov = cfg->coverage_info;
		g_assert (!cfg->compile_aot);
		cpos += 6;

		cov->data [bb->dfn].cil_code = bb->cil_code;
		/* this is not thread save, but good enough */
		x86_inc_mem (code, &cov->data [bb->dfn].count); 
	}

	offset = code - cfg->native_code;

	mono_debug_open_block (cfg, bb, offset);

	MONO_BB_FOR_EACH_INS (bb, ins) {
		offset = code - cfg->native_code;

		max_len = ((guint8 *)ins_get_spec (ins->opcode))[MONO_INST_LEN];

		if (G_UNLIKELY (offset > (cfg->code_size - max_len - 16))) {
			cfg->code_size *= 2;
			cfg->native_code = g_realloc (cfg->native_code, cfg->code_size);
			code = cfg->native_code + offset;
			mono_jit_stats.code_reallocs++;
		}

		if (cfg->debug_info)
			mono_debug_record_line_number (cfg, ins, offset);

		switch (ins->opcode) {
		case OP_BIGMUL:
			x86_mul_reg (code, ins->sreg2, TRUE);
			break;
		case OP_BIGMUL_UN:
			x86_mul_reg (code, ins->sreg2, FALSE);
			break;
		case OP_X86_SETEQ_MEMBASE:
		case OP_X86_SETNE_MEMBASE:
			x86_set_membase (code, ins->opcode == OP_X86_SETEQ_MEMBASE ? X86_CC_EQ : X86_CC_NE,
		                         ins->inst_basereg, ins->inst_offset, TRUE);
			break;
		case OP_STOREI1_MEMBASE_IMM:
			x86_mov_membase_imm (code, ins->inst_destbasereg, ins->inst_offset, ins->inst_imm, 1);
			break;
		case OP_STOREI2_MEMBASE_IMM:
			x86_mov_membase_imm (code, ins->inst_destbasereg, ins->inst_offset, ins->inst_imm, 2);
			break;
		case OP_STORE_MEMBASE_IMM:
		case OP_STOREI4_MEMBASE_IMM:
			x86_mov_membase_imm (code, ins->inst_destbasereg, ins->inst_offset, ins->inst_imm, 4);
			break;
		case OP_STOREI1_MEMBASE_REG:
			x86_mov_membase_reg (code, ins->inst_destbasereg, ins->inst_offset, ins->sreg1, 1);
			break;
		case OP_STOREI2_MEMBASE_REG:
			x86_mov_membase_reg (code, ins->inst_destbasereg, ins->inst_offset, ins->sreg1, 2);
			break;
		case OP_STORE_MEMBASE_REG:
		case OP_STOREI4_MEMBASE_REG:
			x86_mov_membase_reg (code, ins->inst_destbasereg, ins->inst_offset, ins->sreg1, 4);
			break;
		case OP_STORE_MEM_IMM:
			x86_mov_mem_imm (code, ins->inst_p0, ins->inst_c0, 4);
			break;
		case OP_LOADU4_MEM:
			x86_mov_reg_mem (code, ins->dreg, ins->inst_imm, 4);
			break;
		case OP_LOAD_MEM:
		case OP_LOADI4_MEM:
			/* These are created by the cprop pass so they use inst_imm as the source */
			x86_mov_reg_mem (code, ins->dreg, ins->inst_imm, 4);
			break;
		case OP_LOADU1_MEM:
			x86_widen_mem (code, ins->dreg, ins->inst_imm, FALSE, FALSE);
			break;
		case OP_LOADU2_MEM:
			x86_widen_mem (code, ins->dreg, ins->inst_imm, FALSE, TRUE);
			break;
		case OP_LOAD_MEMBASE:
		case OP_LOADI4_MEMBASE:
		case OP_LOADU4_MEMBASE:
			x86_mov_reg_membase (code, ins->dreg, ins->inst_basereg, ins->inst_offset, 4);
			break;
		case OP_LOADU1_MEMBASE:
			x86_widen_membase (code, ins->dreg, ins->inst_basereg, ins->inst_offset, FALSE, FALSE);
			break;
		case OP_LOADI1_MEMBASE:
			x86_widen_membase (code, ins->dreg, ins->inst_basereg, ins->inst_offset, TRUE, FALSE);
			break;
		case OP_LOADU2_MEMBASE:
			x86_widen_membase (code, ins->dreg, ins->inst_basereg, ins->inst_offset, FALSE, TRUE);
			break;
		case OP_LOADI2_MEMBASE:
			x86_widen_membase (code, ins->dreg, ins->inst_basereg, ins->inst_offset, TRUE, TRUE);
			break;
		case OP_ICONV_TO_I1:
		case OP_SEXT_I1:
			x86_widen_reg (code, ins->dreg, ins->sreg1, TRUE, FALSE);
			break;
		case OP_ICONV_TO_I2:
		case OP_SEXT_I2:
			x86_widen_reg (code, ins->dreg, ins->sreg1, TRUE, TRUE);
			break;
		case OP_ICONV_TO_U1:
			x86_widen_reg (code, ins->dreg, ins->sreg1, FALSE, FALSE);
			break;
		case OP_ICONV_TO_U2:
			x86_widen_reg (code, ins->dreg, ins->sreg1, FALSE, TRUE);
			break;
		case OP_COMPARE:
		case OP_ICOMPARE:
			x86_alu_reg_reg (code, X86_CMP, ins->sreg1, ins->sreg2);
			break;
		case OP_COMPARE_IMM:
		case OP_ICOMPARE_IMM:
			x86_alu_reg_imm (code, X86_CMP, ins->sreg1, ins->inst_imm);
			break;
		case OP_X86_COMPARE_MEMBASE_REG:
			x86_alu_membase_reg (code, X86_CMP, ins->inst_basereg, ins->inst_offset, ins->sreg2);
			break;
		case OP_X86_COMPARE_MEMBASE_IMM:
			x86_alu_membase_imm (code, X86_CMP, ins->inst_basereg, ins->inst_offset, ins->inst_imm);
			break;
		case OP_X86_COMPARE_MEMBASE8_IMM:
			x86_alu_membase8_imm (code, X86_CMP, ins->inst_basereg, ins->inst_offset, ins->inst_imm);
			break;
		case OP_X86_COMPARE_REG_MEMBASE:
			x86_alu_reg_membase (code, X86_CMP, ins->sreg1, ins->sreg2, ins->inst_offset);
			break;
		case OP_X86_COMPARE_MEM_IMM:
			x86_alu_mem_imm (code, X86_CMP, ins->inst_offset, ins->inst_imm);
			break;
		case OP_X86_TEST_NULL:
			x86_test_reg_reg (code, ins->sreg1, ins->sreg1);
			break;
		case OP_X86_ADD_MEMBASE_IMM:
			x86_alu_membase_imm (code, X86_ADD, ins->inst_basereg, ins->inst_offset, ins->inst_imm);
			break;
		case OP_X86_ADD_REG_MEMBASE:
			x86_alu_reg_membase (code, X86_ADD, ins->sreg1, ins->sreg2, ins->inst_offset);
			break;
		case OP_X86_SUB_MEMBASE_IMM:
			x86_alu_membase_imm (code, X86_SUB, ins->inst_basereg, ins->inst_offset, ins->inst_imm);
			break;
		case OP_X86_SUB_REG_MEMBASE:
			x86_alu_reg_membase (code, X86_SUB, ins->sreg1, ins->sreg2, ins->inst_offset);
			break;
		case OP_X86_AND_MEMBASE_IMM:
			x86_alu_membase_imm (code, X86_AND, ins->inst_basereg, ins->inst_offset, ins->inst_imm);
			break;
		case OP_X86_OR_MEMBASE_IMM:
			x86_alu_membase_imm (code, X86_OR, ins->inst_basereg, ins->inst_offset, ins->inst_imm);
			break;
		case OP_X86_XOR_MEMBASE_IMM:
			x86_alu_membase_imm (code, X86_XOR, ins->inst_basereg, ins->inst_offset, ins->inst_imm);
			break;
		case OP_X86_ADD_MEMBASE_REG:
			x86_alu_membase_reg (code, X86_ADD, ins->inst_basereg, ins->inst_offset, ins->sreg2);
			break;
		case OP_X86_SUB_MEMBASE_REG:
			x86_alu_membase_reg (code, X86_SUB, ins->inst_basereg, ins->inst_offset, ins->sreg2);
			break;
		case OP_X86_AND_MEMBASE_REG:
			x86_alu_membase_reg (code, X86_AND, ins->inst_basereg, ins->inst_offset, ins->sreg2);
			break;
		case OP_X86_OR_MEMBASE_REG:
			x86_alu_membase_reg (code, X86_OR, ins->inst_basereg, ins->inst_offset, ins->sreg2);
			break;
		case OP_X86_XOR_MEMBASE_REG:
			x86_alu_membase_reg (code, X86_XOR, ins->inst_basereg, ins->inst_offset, ins->sreg2);
			break;
		case OP_X86_INC_MEMBASE:
			x86_inc_membase (code, ins->inst_basereg, ins->inst_offset);
			break;
		case OP_X86_INC_REG:
			x86_inc_reg (code, ins->dreg);
			break;
		case OP_X86_DEC_MEMBASE:
			x86_dec_membase (code, ins->inst_basereg, ins->inst_offset);
			break;
		case OP_X86_DEC_REG:
			x86_dec_reg (code, ins->dreg);
			break;
		case OP_X86_MUL_REG_MEMBASE:
			x86_imul_reg_membase (code, ins->sreg1, ins->sreg2, ins->inst_offset);
			break;
		case OP_X86_AND_REG_MEMBASE:
			x86_alu_reg_membase (code, X86_AND, ins->sreg1, ins->sreg2, ins->inst_offset);
			break;
		case OP_X86_OR_REG_MEMBASE:
			x86_alu_reg_membase (code, X86_OR, ins->sreg1, ins->sreg2, ins->inst_offset);
			break;
		case OP_X86_XOR_REG_MEMBASE:
			x86_alu_reg_membase (code, X86_XOR, ins->sreg1, ins->sreg2, ins->inst_offset);
			break;
		case OP_BREAK:
			x86_breakpoint (code);
			break;
 		case OP_RELAXED_NOP:
			x86_prefix (code, X86_REP_PREFIX);
			x86_nop (code);
			break;
 		case OP_HARD_NOP:
			x86_nop (code);
			break;
 		case OP_NOP:
 		case OP_DUMMY_USE:
 		case OP_DUMMY_STORE:
 		case OP_NOT_REACHED:
 		case OP_NOT_NULL:
 			break;
		case OP_ADDCC:
		case OP_IADDCC:
		case OP_IADD:
			x86_alu_reg_reg (code, X86_ADD, ins->sreg1, ins->sreg2);
			break;
		case OP_ADC:
		case OP_IADC:
			x86_alu_reg_reg (code, X86_ADC, ins->sreg1, ins->sreg2);
			break;
		case OP_ADDCC_IMM:
		case OP_ADD_IMM:
		case OP_IADD_IMM:
			x86_alu_reg_imm (code, X86_ADD, ins->dreg, ins->inst_imm);
			break;
		case OP_ADC_IMM:
		case OP_IADC_IMM:
			x86_alu_reg_imm (code, X86_ADC, ins->dreg, ins->inst_imm);
			break;
		case OP_SUBCC:
		case OP_ISUBCC:
		case OP_ISUB:
			x86_alu_reg_reg (code, X86_SUB, ins->sreg1, ins->sreg2);
			break;
		case OP_SBB:
		case OP_ISBB:
			x86_alu_reg_reg (code, X86_SBB, ins->sreg1, ins->sreg2);
			break;
		case OP_SUBCC_IMM:
		case OP_SUB_IMM:
		case OP_ISUB_IMM:
			x86_alu_reg_imm (code, X86_SUB, ins->dreg, ins->inst_imm);
			break;
		case OP_SBB_IMM:
		case OP_ISBB_IMM:
			x86_alu_reg_imm (code, X86_SBB, ins->dreg, ins->inst_imm);
			break;
		case OP_IAND:
			x86_alu_reg_reg (code, X86_AND, ins->sreg1, ins->sreg2);
			break;
		case OP_AND_IMM:
		case OP_IAND_IMM:
			x86_alu_reg_imm (code, X86_AND, ins->sreg1, ins->inst_imm);
			break;
		case OP_IDIV:
		case OP_IREM:
			/* 
			 * The code is the same for div/rem, the allocator will allocate dreg
			 * to RAX/RDX as appropriate.
			 */
			if (ins->sreg2 == X86_EDX) {
				/* cdq clobbers this */
				x86_push_reg (code, ins->sreg2);
				x86_cdq (code);
				x86_div_membase (code, X86_ESP, 0, TRUE);
				x86_alu_reg_imm (code, X86_ADD, X86_ESP, 4);				
			} else {
				x86_cdq (code);
				x86_div_reg (code, ins->sreg2, TRUE);
			}
			break;
		case OP_IDIV_UN:
		case OP_IREM_UN:
			if (ins->sreg2 == X86_EDX) {
				x86_push_reg (code, ins->sreg2);
				x86_alu_reg_reg (code, X86_XOR, X86_EDX, X86_EDX);
				x86_div_membase (code, X86_ESP, 0, FALSE);
				x86_alu_reg_imm (code, X86_ADD, X86_ESP, 4);				
			} else {
				x86_alu_reg_reg (code, X86_XOR, X86_EDX, X86_EDX);
				x86_div_reg (code, ins->sreg2, FALSE);
			}
			break;
		case OP_DIV_IMM:
			x86_mov_reg_imm (code, ins->sreg2, ins->inst_imm);
			x86_cdq (code);
			x86_div_reg (code, ins->sreg2, TRUE);
			break;
		case OP_IREM_IMM: {
			int power = mono_is_power_of_two (ins->inst_imm);

			g_assert (ins->sreg1 == X86_EAX);
			g_assert (ins->dreg == X86_EAX);
			g_assert (power >= 0);

			if (power == 1) {
				/* Based on http://compilers.iecc.com/comparch/article/93-04-079 */
				x86_cdq (code);
				x86_alu_reg_imm (code, X86_AND, X86_EAX, 1);
				/* 
				 * If the divident is >= 0, this does not nothing. If it is positive, it
				 * it transforms %eax=0 into %eax=0, and %eax=1 into %eax=-1.
				 */
				x86_alu_reg_reg (code, X86_XOR, X86_EAX, X86_EDX);
				x86_alu_reg_reg (code, X86_SUB, X86_EAX, X86_EDX);
			} else if (power == 0) {
				x86_alu_reg_reg (code, X86_XOR, ins->dreg, ins->dreg);
			} else {
				/* Based on gcc code */

				/* Add compensation for negative dividents */
				x86_cdq (code);
				x86_shift_reg_imm (code, X86_SHR, X86_EDX, 32 - power);
				x86_alu_reg_reg (code, X86_ADD, X86_EAX, X86_EDX);
				/* Compute remainder */
				x86_alu_reg_imm (code, X86_AND, X86_EAX, (1 << power) - 1);
				/* Remove compensation */
				x86_alu_reg_reg (code, X86_SUB, X86_EAX, X86_EDX);
			}
			break;
		}
		case OP_IOR:
			x86_alu_reg_reg (code, X86_OR, ins->sreg1, ins->sreg2);
			break;
		case OP_OR_IMM:
		case OP_IOR_IMM:
			x86_alu_reg_imm (code, X86_OR, ins->sreg1, ins->inst_imm);
			break;
		case OP_IXOR:
			x86_alu_reg_reg (code, X86_XOR, ins->sreg1, ins->sreg2);
			break;
		case OP_XOR_IMM:
		case OP_IXOR_IMM:
			x86_alu_reg_imm (code, X86_XOR, ins->sreg1, ins->inst_imm);
			break;
		case OP_ISHL:
			g_assert (ins->sreg2 == X86_ECX);
			x86_shift_reg (code, X86_SHL, ins->dreg);
			break;
		case OP_ISHR:
			g_assert (ins->sreg2 == X86_ECX);
			x86_shift_reg (code, X86_SAR, ins->dreg);
			break;
		case OP_SHR_IMM:
		case OP_ISHR_IMM:
			x86_shift_reg_imm (code, X86_SAR, ins->dreg, ins->inst_imm);
			break;
		case OP_SHR_UN_IMM:
		case OP_ISHR_UN_IMM:
			x86_shift_reg_imm (code, X86_SHR, ins->dreg, ins->inst_imm);
			break;
		case OP_ISHR_UN:
			g_assert (ins->sreg2 == X86_ECX);
			x86_shift_reg (code, X86_SHR, ins->dreg);
			break;
		case OP_SHL_IMM:
		case OP_ISHL_IMM:
			x86_shift_reg_imm (code, X86_SHL, ins->dreg, ins->inst_imm);
			break;
		case OP_LSHL: {
			guint8 *jump_to_end;

			/* handle shifts below 32 bits */
			x86_shld_reg (code, ins->backend.reg3, ins->sreg1);
			x86_shift_reg (code, X86_SHL, ins->sreg1);

			x86_test_reg_imm (code, X86_ECX, 32);
			jump_to_end = code; x86_branch8 (code, X86_CC_EQ, 0, TRUE);

			/* handle shift over 32 bit */
			x86_mov_reg_reg (code, ins->backend.reg3, ins->sreg1, 4);
			x86_clear_reg (code, ins->sreg1);
			
			x86_patch (jump_to_end, code);
			}
			break;
		case OP_LSHR: {
			guint8 *jump_to_end;

			/* handle shifts below 32 bits */
			x86_shrd_reg (code, ins->sreg1, ins->backend.reg3);
			x86_shift_reg (code, X86_SAR, ins->backend.reg3);

			x86_test_reg_imm (code, X86_ECX, 32);
			jump_to_end = code; x86_branch8 (code, X86_CC_EQ, 0, FALSE);

			/* handle shifts over 31 bits */
			x86_mov_reg_reg (code, ins->sreg1, ins->backend.reg3, 4);
			x86_shift_reg_imm (code, X86_SAR, ins->backend.reg3, 31);
			
			x86_patch (jump_to_end, code);
			}
			break;
		case OP_LSHR_UN: {
			guint8 *jump_to_end;

			/* handle shifts below 32 bits */
			x86_shrd_reg (code, ins->sreg1, ins->backend.reg3);
			x86_shift_reg (code, X86_SHR, ins->backend.reg3);

			x86_test_reg_imm (code, X86_ECX, 32);
			jump_to_end = code; x86_branch8 (code, X86_CC_EQ, 0, FALSE);

			/* handle shifts over 31 bits */
			x86_mov_reg_reg (code, ins->sreg1, ins->backend.reg3, 4);
			x86_clear_reg (code, ins->backend.reg3);
			
			x86_patch (jump_to_end, code);
			}
			break;
		case OP_LSHL_IMM:
			if (ins->inst_imm >= 32) {
				x86_mov_reg_reg (code, ins->backend.reg3, ins->sreg1, 4);
				x86_clear_reg (code, ins->sreg1);
				x86_shift_reg_imm (code, X86_SHL, ins->backend.reg3, ins->inst_imm - 32);
			} else {
				x86_shld_reg_imm (code, ins->backend.reg3, ins->sreg1, ins->inst_imm);
				x86_shift_reg_imm (code, X86_SHL, ins->sreg1, ins->inst_imm);
			}
			break;
		case OP_LSHR_IMM:
			if (ins->inst_imm >= 32) {
				x86_mov_reg_reg (code, ins->sreg1, ins->backend.reg3,  4);
				x86_shift_reg_imm (code, X86_SAR, ins->backend.reg3, 0x1f);
				x86_shift_reg_imm (code, X86_SAR, ins->sreg1, ins->inst_imm - 32);
			} else {
				x86_shrd_reg_imm (code, ins->sreg1, ins->backend.reg3, ins->inst_imm);
				x86_shift_reg_imm (code, X86_SAR, ins->backend.reg3, ins->inst_imm);
			}
			break;
		case OP_LSHR_UN_IMM:
			if (ins->inst_imm >= 32) {
				x86_mov_reg_reg (code, ins->sreg1, ins->backend.reg3, 4);
				x86_clear_reg (code, ins->backend.reg3);
				x86_shift_reg_imm (code, X86_SHR, ins->sreg1, ins->inst_imm - 32);
			} else {
				x86_shrd_reg_imm (code, ins->sreg1, ins->backend.reg3, ins->inst_imm);
				x86_shift_reg_imm (code, X86_SHR, ins->backend.reg3, ins->inst_imm);
			}
			break;
		case OP_INOT:
			x86_not_reg (code, ins->sreg1);
			break;
		case OP_INEG:
			x86_neg_reg (code, ins->sreg1);
			break;

		case OP_IMUL:
			x86_imul_reg_reg (code, ins->sreg1, ins->sreg2);
			break;
		case OP_MUL_IMM:
		case OP_IMUL_IMM:
			switch (ins->inst_imm) {
			case 2:
				/* MOV r1, r2 */
				/* ADD r1, r1 */
				if (ins->dreg != ins->sreg1)
					x86_mov_reg_reg (code, ins->dreg, ins->sreg1, 4);
				x86_alu_reg_reg (code, X86_ADD, ins->dreg, ins->dreg);
				break;
			case 3:
				/* LEA r1, [r2 + r2*2] */
				x86_lea_memindex (code, ins->dreg, ins->sreg1, 0, ins->sreg1, 1);
				break;
			case 5:
				/* LEA r1, [r2 + r2*4] */
				x86_lea_memindex (code, ins->dreg, ins->sreg1, 0, ins->sreg1, 2);
				break;
			case 6:
				/* LEA r1, [r2 + r2*2] */
				/* ADD r1, r1          */
				x86_lea_memindex (code, ins->dreg, ins->sreg1, 0, ins->sreg1, 1);
				x86_alu_reg_reg (code, X86_ADD, ins->dreg, ins->dreg);
				break;
			case 9:
				/* LEA r1, [r2 + r2*8] */
				x86_lea_memindex (code, ins->dreg, ins->sreg1, 0, ins->sreg1, 3);
				break;
			case 10:
				/* LEA r1, [r2 + r2*4] */
				/* ADD r1, r1          */
				x86_lea_memindex (code, ins->dreg, ins->sreg1, 0, ins->sreg1, 2);
				x86_alu_reg_reg (code, X86_ADD, ins->dreg, ins->dreg);
				break;
			case 12:
				/* LEA r1, [r2 + r2*2] */
				/* SHL r1, 2           */
				x86_lea_memindex (code, ins->dreg, ins->sreg1, 0, ins->sreg1, 1);
				x86_shift_reg_imm (code, X86_SHL, ins->dreg, 2);
				break;
			case 25:
				/* LEA r1, [r2 + r2*4] */
				/* LEA r1, [r1 + r1*4] */
				x86_lea_memindex (code, ins->dreg, ins->sreg1, 0, ins->sreg1, 2);
				x86_lea_memindex (code, ins->dreg, ins->dreg, 0, ins->dreg, 2);
				break;
			case 100:
				/* LEA r1, [r2 + r2*4] */
				/* SHL r1, 2           */
				/* LEA r1, [r1 + r1*4] */
				x86_lea_memindex (code, ins->dreg, ins->sreg1, 0, ins->sreg1, 2);
				x86_shift_reg_imm (code, X86_SHL, ins->dreg, 2);
				x86_lea_memindex (code, ins->dreg, ins->dreg, 0, ins->dreg, 2);
				break;
			default:
				x86_imul_reg_reg_imm (code, ins->dreg, ins->sreg1, ins->inst_imm);
				break;
			}
			break;
		case OP_IMUL_OVF:
			x86_imul_reg_reg (code, ins->sreg1, ins->sreg2);
			EMIT_COND_SYSTEM_EXCEPTION (X86_CC_O, FALSE, "OverflowException");
			break;
		case OP_IMUL_OVF_UN: {
			/* the mul operation and the exception check should most likely be split */
			int non_eax_reg, saved_eax = FALSE, saved_edx = FALSE;
			/*g_assert (ins->sreg2 == X86_EAX);
			g_assert (ins->dreg == X86_EAX);*/
			if (ins->sreg2 == X86_EAX) {
				non_eax_reg = ins->sreg1;
			} else if (ins->sreg1 == X86_EAX) {
				non_eax_reg = ins->sreg2;
			} else {
				/* no need to save since we're going to store to it anyway */
				if (ins->dreg != X86_EAX) {
					saved_eax = TRUE;
					x86_push_reg (code, X86_EAX);
				}
				x86_mov_reg_reg (code, X86_EAX, ins->sreg1, 4);
				non_eax_reg = ins->sreg2;
			}
			if (ins->dreg == X86_EDX) {
				if (!saved_eax) {
					saved_eax = TRUE;
					x86_push_reg (code, X86_EAX);
				}
			} else if (ins->dreg != X86_EAX) {
				saved_edx = TRUE;
				x86_push_reg (code, X86_EDX);
			}
			x86_mul_reg (code, non_eax_reg, FALSE);
			/* save before the check since pop and mov don't change the flags */
			if (ins->dreg != X86_EAX)
				x86_mov_reg_reg (code, ins->dreg, X86_EAX, 4);
			if (saved_edx)
				x86_pop_reg (code, X86_EDX);
			if (saved_eax)
				x86_pop_reg (code, X86_EAX);
			EMIT_COND_SYSTEM_EXCEPTION (X86_CC_O, FALSE, "OverflowException");
			break;
		}
		case OP_ICONST:
			x86_mov_reg_imm (code, ins->dreg, ins->inst_c0);
			break;
		case OP_AOTCONST:
			g_assert_not_reached ();
			mono_add_patch_info (cfg, offset, (MonoJumpInfoType)ins->inst_i1, ins->inst_p0);
			x86_mov_reg_imm (code, ins->dreg, 0);
			break;
		case OP_JUMP_TABLE:
			mono_add_patch_info (cfg, offset, (MonoJumpInfoType)ins->inst_i1, ins->inst_p0);
			x86_mov_reg_imm (code, ins->dreg, 0);
			break;
		case OP_LOAD_GOTADDR:
			x86_call_imm (code, 0);
			/* 
			 * The patch needs to point to the pop, since the GOT offset needs 
			 * to be added to that address.
			 */
			mono_add_patch_info (cfg, code - cfg->native_code, MONO_PATCH_INFO_GOT_OFFSET, NULL);
			x86_pop_reg (code, ins->dreg);
			x86_alu_reg_imm (code, X86_ADD, ins->dreg, 0xf0f0f0f0);
			break;
		case OP_GOT_ENTRY:
			mono_add_patch_info (cfg, offset, (MonoJumpInfoType)ins->inst_right->inst_i1, ins->inst_right->inst_p0);
			x86_mov_reg_membase (code, ins->dreg, ins->inst_basereg, 0xf0f0f0f0, 4);
			break;
		case OP_X86_PUSH_GOT_ENTRY:
			mono_add_patch_info (cfg, offset, (MonoJumpInfoType)ins->inst_right->inst_i1, ins->inst_right->inst_p0);
			x86_push_membase (code, ins->inst_basereg, 0xf0f0f0f0);
			break;
		case OP_MOVE:
			x86_mov_reg_reg (code, ins->dreg, ins->sreg1, 4);
			break;
		case OP_JMP: {
			/*
			 * Note: this 'frame destruction' logic is useful for tail calls, too.
			 * Keep in sync with the code in emit_epilog.
			 */
			int pos = 0;

			/* FIXME: no tracing support... */
			if (cfg->prof_options & MONO_PROFILE_ENTER_LEAVE)
				code = mono_arch_instrument_epilog (cfg, mono_profiler_method_leave, code, FALSE);
			/* reset offset to make max_len work */
			offset = code - cfg->native_code;

			g_assert (!cfg->method->save_lmf);

			code = emit_load_volatile_arguments (cfg, code);

			if (cfg->used_int_regs & (1 << X86_EBX))
				pos -= 4;
			if (cfg->used_int_regs & (1 << X86_EDI))
				pos -= 4;
			if (cfg->used_int_regs & (1 << X86_ESI))
				pos -= 4;
			if (pos)
				x86_lea_membase (code, X86_ESP, X86_EBP, pos);
	
			if (cfg->used_int_regs & (1 << X86_ESI))
				x86_pop_reg (code, X86_ESI);
			if (cfg->used_int_regs & (1 << X86_EDI))
				x86_pop_reg (code, X86_EDI);
			if (cfg->used_int_regs & (1 << X86_EBX))
				x86_pop_reg (code, X86_EBX);
	
			/* restore ESP/EBP */
			x86_leave (code);
			offset = code - cfg->native_code;
			mono_add_patch_info (cfg, offset, MONO_PATCH_INFO_METHOD_JUMP, ins->inst_p0);
			x86_jump32 (code, 0);

			cfg->disable_aot = TRUE;
			break;
		}
		case OP_CHECK_THIS:
			/* ensure ins->sreg1 is not NULL
			 * note that cmp DWORD PTR [eax], eax is one byte shorter than
			 * cmp DWORD PTR [eax], 0
		         */
			x86_alu_membase_reg (code, X86_CMP, ins->sreg1, 0, ins->sreg1);
			break;
		case OP_ARGLIST: {
			int hreg = ins->sreg1 == X86_EAX? X86_ECX: X86_EAX;
			x86_push_reg (code, hreg);
			x86_lea_membase (code, hreg, X86_EBP, cfg->sig_cookie);
			x86_mov_membase_reg (code, ins->sreg1, 0, hreg, 4);
			x86_pop_reg (code, hreg);
			break;
		}
		case OP_FCALL:
		case OP_LCALL:
		case OP_VCALL:
		case OP_VCALL2:
		case OP_VOIDCALL:
		case OP_CALL:
			call = (MonoCallInst*)ins;
			if (ins->flags & MONO_INST_HAS_METHOD)
				code = emit_call (cfg, code, MONO_PATCH_INFO_METHOD, call->method);
			else
				code = emit_call (cfg, code, MONO_PATCH_INFO_ABS, call->fptr);
			if (call->stack_usage && !CALLCONV_IS_STDCALL (call->signature)) {
				/* a pop is one byte, while an add reg, imm is 3. So if there are 4 or 8
				 * bytes to pop, we want to use pops. GCC does this (note it won't happen
				 * for P4 or i686 because gcc will avoid using pop push at all. But we aren't
				 * smart enough to do that optimization yet
				 *
				 * It turns out that on my P4, doing two pops for 8 bytes on the stack makes
				 * mcs botstrap slow down. However, doing 1 pop for 4 bytes creates a small,
				 * (most likely from locality benefits). People with other processors should
				 * check on theirs to see what happens.
				 */
				if (call->stack_usage == 4) {
					/* we want to use registers that won't get used soon, so use
					 * ecx, as eax will get allocated first. edx is used by long calls,
					 * so we can't use that.
					 */
					
					x86_pop_reg (code, X86_ECX);
				} else {
					x86_alu_reg_imm (code, X86_ADD, X86_ESP, call->stack_usage);
				}
			}
			code = emit_move_return_value (cfg, ins, code);
			break;
		case OP_FCALL_REG:
		case OP_LCALL_REG:
		case OP_VCALL_REG:
		case OP_VCALL2_REG:
		case OP_VOIDCALL_REG:
		case OP_CALL_REG:
			call = (MonoCallInst*)ins;
			x86_call_reg (code, ins->sreg1);
			if (call->stack_usage && !CALLCONV_IS_STDCALL (call->signature)) {
				if (call->stack_usage == 4)
					x86_pop_reg (code, X86_ECX);
				else
					x86_alu_reg_imm (code, X86_ADD, X86_ESP, call->stack_usage);
			}
			code = emit_move_return_value (cfg, ins, code);
			break;
		case OP_FCALL_MEMBASE:
		case OP_LCALL_MEMBASE:
		case OP_VCALL_MEMBASE:
		case OP_VCALL2_MEMBASE:
		case OP_VOIDCALL_MEMBASE:
		case OP_CALL_MEMBASE:
			call = (MonoCallInst*)ins;

			/* 
			 * Emit a few nops to simplify get_vcall_slot ().
			 */
			x86_nop (code);
			x86_nop (code);
			x86_nop (code);

			x86_call_membase (code, ins->sreg1, ins->inst_offset);
			if (call->stack_usage && !CALLCONV_IS_STDCALL (call->signature)) {
				if (call->stack_usage == 4)
					x86_pop_reg (code, X86_ECX);
				else
					x86_alu_reg_imm (code, X86_ADD, X86_ESP, call->stack_usage);
			}
			code = emit_move_return_value (cfg, ins, code);
			break;
		case OP_X86_PUSH:
			x86_push_reg (code, ins->sreg1);
			break;
		case OP_X86_PUSH_IMM:
			x86_push_imm (code, ins->inst_imm);
			break;
		case OP_X86_PUSH_MEMBASE:
			x86_push_membase (code, ins->inst_basereg, ins->inst_offset);
			break;
		case OP_X86_PUSH_OBJ: 
			x86_alu_reg_imm (code, X86_SUB, X86_ESP, ins->inst_imm);
			x86_push_reg (code, X86_EDI);
			x86_push_reg (code, X86_ESI);
			x86_push_reg (code, X86_ECX);
			if (ins->inst_offset)
				x86_lea_membase (code, X86_ESI, ins->inst_basereg, ins->inst_offset);
			else
				x86_mov_reg_reg (code, X86_ESI, ins->inst_basereg, 4);
			x86_lea_membase (code, X86_EDI, X86_ESP, 12);
			x86_mov_reg_imm (code, X86_ECX, (ins->inst_imm >> 2));
			x86_cld (code);
			x86_prefix (code, X86_REP_PREFIX);
			x86_movsd (code);
			x86_pop_reg (code, X86_ECX);
			x86_pop_reg (code, X86_ESI);
			x86_pop_reg (code, X86_EDI);
			break;
		case OP_X86_LEA:
			x86_lea_memindex (code, ins->dreg, ins->sreg1, ins->inst_imm, ins->sreg2, ins->backend.shift_amount);
			break;
		case OP_X86_LEA_MEMBASE:
			x86_lea_membase (code, ins->dreg, ins->sreg1, ins->inst_imm);
			break;
		case OP_X86_XCHG:
			x86_xchg_reg_reg (code, ins->sreg1, ins->sreg2, 4);
			break;
		case OP_LOCALLOC:
			/* keep alignment */
			x86_alu_reg_imm (code, X86_ADD, ins->sreg1, MONO_ARCH_LOCALLOC_ALIGNMENT - 1);
			x86_alu_reg_imm (code, X86_AND, ins->sreg1, ~(MONO_ARCH_LOCALLOC_ALIGNMENT - 1));
			code = mono_emit_stack_alloc (code, ins);
			x86_mov_reg_reg (code, ins->dreg, X86_ESP, 4);
			break;
		case OP_LOCALLOC_IMM: {
			guint32 size = ins->inst_imm;
			size = (size + (MONO_ARCH_FRAME_ALIGNMENT - 1)) & ~ (MONO_ARCH_FRAME_ALIGNMENT - 1);

			if (ins->flags & MONO_INST_INIT) {
				/* FIXME: Optimize this */
				x86_mov_reg_imm (code, ins->dreg, size);
				ins->sreg1 = ins->dreg;

				code = mono_emit_stack_alloc (code, ins);
				x86_mov_reg_reg (code, ins->dreg, X86_ESP, 4);
			} else {
				x86_alu_reg_imm (code, X86_SUB, X86_ESP, size);
				x86_mov_reg_reg (code, ins->dreg, X86_ESP, 4);
			}
			break;
		}
		case OP_THROW: {
			x86_push_reg (code, ins->sreg1);
			code = emit_call (cfg, code, MONO_PATCH_INFO_INTERNAL_METHOD, 
							  (gpointer)"mono_arch_throw_exception");
			break;
		}
		case OP_RETHROW: {
			x86_push_reg (code, ins->sreg1);
			code = emit_call (cfg, code, MONO_PATCH_INFO_INTERNAL_METHOD, 
							  (gpointer)"mono_arch_rethrow_exception");
			break;
		}
		case OP_CALL_HANDLER:
			x86_alu_reg_imm (code, X86_SUB, X86_ESP, MONO_ARCH_FRAME_ALIGNMENT - 4);
			mono_add_patch_info (cfg, code - cfg->native_code, MONO_PATCH_INFO_BB, ins->inst_target_bb);
			x86_call_imm (code, 0);
			x86_alu_reg_imm (code, X86_ADD, X86_ESP, MONO_ARCH_FRAME_ALIGNMENT - 4);
			break;
		case OP_START_HANDLER: {
			MonoInst *spvar = mono_find_spvar_for_region (cfg, bb->region);
			x86_mov_membase_reg (code, spvar->inst_basereg, spvar->inst_offset, X86_ESP, 4);
			break;
		}
		case OP_ENDFINALLY: {
			MonoInst *spvar = mono_find_spvar_for_region (cfg, bb->region);
			x86_mov_reg_membase (code, X86_ESP, spvar->inst_basereg, spvar->inst_offset, 4);
			x86_ret (code);
			break;
		}
		case OP_ENDFILTER: {
			MonoInst *spvar = mono_find_spvar_for_region (cfg, bb->region);
			x86_mov_reg_membase (code, X86_ESP, spvar->inst_basereg, spvar->inst_offset, 4);
			/* The local allocator will put the result into EAX */
			x86_ret (code);
			break;
		}

		case OP_LABEL:
			ins->inst_c0 = code - cfg->native_code;
			break;
		case OP_BR:
			if (ins->flags & MONO_INST_BRLABEL) {
				if (ins->inst_i0->inst_c0) {
					x86_jump_code (code, cfg->native_code + ins->inst_i0->inst_c0);
				} else {
					mono_add_patch_info (cfg, offset, MONO_PATCH_INFO_LABEL, ins->inst_i0);
					if ((cfg->opt & MONO_OPT_BRANCH) &&
					    x86_is_imm8 (ins->inst_i0->inst_c1 - cpos))
						x86_jump8 (code, 0);
					else 
						x86_jump32 (code, 0);
				}
			} else {
				if (ins->inst_target_bb->native_offset) {
					x86_jump_code (code, cfg->native_code + ins->inst_target_bb->native_offset); 
				} else {
					mono_add_patch_info (cfg, offset, MONO_PATCH_INFO_BB, ins->inst_target_bb);
					if ((cfg->opt & MONO_OPT_BRANCH) &&
					    x86_is_imm8 (ins->inst_target_bb->max_offset - cpos))
						x86_jump8 (code, 0);
					else 
						x86_jump32 (code, 0);
				} 
			}
			break;
		case OP_BR_REG:
			x86_jump_reg (code, ins->sreg1);
			break;
		case OP_CEQ:
		case OP_CLT:
		case OP_CLT_UN:
		case OP_CGT:
		case OP_CGT_UN:
		case OP_CNE:
		case OP_ICEQ:
		case OP_ICLT:
		case OP_ICLT_UN:
		case OP_ICGT:
		case OP_ICGT_UN:
			x86_set_reg (code, cc_table [mono_opcode_to_cond (ins->opcode)], ins->dreg, cc_signed_table [mono_opcode_to_cond (ins->opcode)]);
			x86_widen_reg (code, ins->dreg, ins->dreg, FALSE, FALSE);
			break;
		case OP_COND_EXC_EQ:
		case OP_COND_EXC_NE_UN:
		case OP_COND_EXC_LT:
		case OP_COND_EXC_LT_UN:
		case OP_COND_EXC_GT:
		case OP_COND_EXC_GT_UN:
		case OP_COND_EXC_GE:
		case OP_COND_EXC_GE_UN:
		case OP_COND_EXC_LE:
		case OP_COND_EXC_LE_UN:
		case OP_COND_EXC_IEQ:
		case OP_COND_EXC_INE_UN:
		case OP_COND_EXC_ILT:
		case OP_COND_EXC_ILT_UN:
		case OP_COND_EXC_IGT:
		case OP_COND_EXC_IGT_UN:
		case OP_COND_EXC_IGE:
		case OP_COND_EXC_IGE_UN:
		case OP_COND_EXC_ILE:
		case OP_COND_EXC_ILE_UN:
			EMIT_COND_SYSTEM_EXCEPTION (cc_table [mono_opcode_to_cond (ins->opcode)], cc_signed_table [mono_opcode_to_cond (ins->opcode)], ins->inst_p1);
			break;
		case OP_COND_EXC_OV:
		case OP_COND_EXC_NO:
		case OP_COND_EXC_C:
		case OP_COND_EXC_NC:
			EMIT_COND_SYSTEM_EXCEPTION (branch_cc_table [ins->opcode - OP_COND_EXC_EQ], (ins->opcode < OP_COND_EXC_NE_UN), ins->inst_p1);
			break;
		case OP_COND_EXC_IOV:
		case OP_COND_EXC_INO:
		case OP_COND_EXC_IC:
		case OP_COND_EXC_INC:
			EMIT_COND_SYSTEM_EXCEPTION (branch_cc_table [ins->opcode - OP_COND_EXC_IEQ], (ins->opcode < OP_COND_EXC_INE_UN), ins->inst_p1);
			break;
		case OP_IBEQ:
		case OP_IBNE_UN:
		case OP_IBLT:
		case OP_IBLT_UN:
		case OP_IBGT:
		case OP_IBGT_UN:
		case OP_IBGE:
		case OP_IBGE_UN:
		case OP_IBLE:
		case OP_IBLE_UN:
			EMIT_COND_BRANCH (ins, cc_table [mono_opcode_to_cond (ins->opcode)], cc_signed_table [mono_opcode_to_cond (ins->opcode)]);
			break;

		case OP_CMOV_IEQ:
		case OP_CMOV_IGE:
		case OP_CMOV_IGT:
		case OP_CMOV_ILE:
		case OP_CMOV_ILT:
		case OP_CMOV_INE_UN:
		case OP_CMOV_IGE_UN:
		case OP_CMOV_IGT_UN:
		case OP_CMOV_ILE_UN:
		case OP_CMOV_ILT_UN:
			g_assert (ins->dreg == ins->sreg1);
			x86_cmov_reg (code, cc_table [mono_opcode_to_cond (ins->opcode)], cc_signed_table [mono_opcode_to_cond (ins->opcode)], ins->dreg, ins->sreg2);
			break;

		/* floating point opcodes */
		case OP_R8CONST: {
			double d = *(double *)ins->inst_p0;

			if ((d == 0.0) && (mono_signbit (d) == 0)) {
				x86_fldz (code);
			} else if (d == 1.0) {
				x86_fld1 (code);
			} else {
				if (cfg->compile_aot) {
					guint32 *val = (guint32*)&d;
					x86_push_imm (code, val [1]);
					x86_push_imm (code, val [0]);
					x86_fld_membase (code, X86_ESP, 0, TRUE);
					x86_alu_reg_imm (code, X86_ADD, X86_ESP, 8);
				}
				else {
					mono_add_patch_info (cfg, code - cfg->native_code, MONO_PATCH_INFO_R8, ins->inst_p0);
					x86_fld (code, NULL, TRUE);
				}
			}
			break;
		}
		case OP_R4CONST: {
			float f = *(float *)ins->inst_p0;

			if ((f == 0.0) && (mono_signbit (f) == 0)) {
				x86_fldz (code);
			} else if (f == 1.0) {
				x86_fld1 (code);
			} else {
				if (cfg->compile_aot) {
					guint32 val = *(guint32*)&f;
					x86_push_imm (code, val);
					x86_fld_membase (code, X86_ESP, 0, FALSE);
					x86_alu_reg_imm (code, X86_ADD, X86_ESP, 4);
				}
				else {
					mono_add_patch_info (cfg, code - cfg->native_code, MONO_PATCH_INFO_R4, ins->inst_p0);
					x86_fld (code, NULL, FALSE);
				}
			}
			break;
		}
		case OP_STORER8_MEMBASE_REG:
			x86_fst_membase (code, ins->inst_destbasereg, ins->inst_offset, TRUE, TRUE);
			break;
		case OP_LOADR8_SPILL_MEMBASE:
			x86_fld_membase (code, ins->inst_basereg, ins->inst_offset, TRUE);
			x86_fxch (code, 1);
			break;
		case OP_LOADR8_MEMBASE:
			x86_fld_membase (code, ins->inst_basereg, ins->inst_offset, TRUE);
			break;
		case OP_STORER4_MEMBASE_REG:
			x86_fst_membase (code, ins->inst_destbasereg, ins->inst_offset, FALSE, TRUE);
			break;
		case OP_LOADR4_MEMBASE:
			x86_fld_membase (code, ins->inst_basereg, ins->inst_offset, FALSE);
			break;
		case OP_ICONV_TO_R4: /* FIXME: change precision */
		case OP_ICONV_TO_R8:
			x86_push_reg (code, ins->sreg1);
			x86_fild_membase (code, X86_ESP, 0, FALSE);
			x86_alu_reg_imm (code, X86_ADD, X86_ESP, 4);
			break;
		case OP_ICONV_TO_R_UN:
			x86_push_imm (code, 0);
			x86_push_reg (code, ins->sreg1);
			x86_fild_membase (code, X86_ESP, 0, TRUE);
			x86_alu_reg_imm (code, X86_ADD, X86_ESP, 8);
			break;
		case OP_X86_FP_LOAD_I8:
			x86_fild_membase (code, ins->inst_basereg, ins->inst_offset, TRUE);
			break;
		case OP_X86_FP_LOAD_I4:
			x86_fild_membase (code, ins->inst_basereg, ins->inst_offset, FALSE);
			break;
		case OP_FCONV_TO_R4:
			/* FIXME: nothing to do ?? */
			break;
		case OP_FCONV_TO_I1:
			code = emit_float_to_int (cfg, code, ins->dreg, 1, TRUE);
			break;
		case OP_FCONV_TO_U1:
			code = emit_float_to_int (cfg, code, ins->dreg, 1, FALSE);
			break;
		case OP_FCONV_TO_I2:
			code = emit_float_to_int (cfg, code, ins->dreg, 2, TRUE);
			break;
		case OP_FCONV_TO_U2:
			code = emit_float_to_int (cfg, code, ins->dreg, 2, FALSE);
			break;
		case OP_FCONV_TO_I4:
		case OP_FCONV_TO_I:
			code = emit_float_to_int (cfg, code, ins->dreg, 4, TRUE);
			break;
		case OP_FCONV_TO_I8:
			x86_alu_reg_imm (code, X86_SUB, X86_ESP, 4);
			x86_fnstcw_membase(code, X86_ESP, 0);
			x86_mov_reg_membase (code, ins->dreg, X86_ESP, 0, 2);
			x86_alu_reg_imm (code, X86_OR, ins->dreg, 0xc00);
			x86_mov_membase_reg (code, X86_ESP, 2, ins->dreg, 2);
			x86_fldcw_membase (code, X86_ESP, 2);
			x86_alu_reg_imm (code, X86_SUB, X86_ESP, 8);
			x86_fist_pop_membase (code, X86_ESP, 0, TRUE);
			x86_pop_reg (code, ins->dreg);
			x86_pop_reg (code, ins->backend.reg3);
			x86_fldcw_membase (code, X86_ESP, 0);
			x86_alu_reg_imm (code, X86_ADD, X86_ESP, 4);
			break;
		case OP_LCONV_TO_R8_2:
			x86_push_reg (code, ins->sreg2);
			x86_push_reg (code, ins->sreg1);
			x86_fild_membase (code, X86_ESP, 0, TRUE);
			x86_alu_reg_imm (code, X86_ADD, X86_ESP, 8);
			break;
		case OP_LCONV_TO_R4_2:
			x86_push_reg (code, ins->sreg2);
			x86_push_reg (code, ins->sreg1);
			x86_fild_membase (code, X86_ESP, 0, TRUE);
			/* Change precision */
			x86_fst_membase (code, X86_ESP, 0, FALSE, TRUE);
			x86_fld_membase (code, X86_ESP, 0, FALSE);
			x86_alu_reg_imm (code, X86_ADD, X86_ESP, 8);
			break;
		case OP_LCONV_TO_R_UN:
		case OP_LCONV_TO_R_UN_2: { 
			static guint8 mn[] = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x3f, 0x40 };
			guint8 *br;

			/* load 64bit integer to FP stack */
			x86_push_imm (code, 0);
			x86_push_reg (code, ins->sreg2);
			x86_push_reg (code, ins->sreg1);
			x86_fild_membase (code, X86_ESP, 0, TRUE);
			/* store as 80bit FP value */
			x86_fst80_membase (code, X86_ESP, 0);
			
			/* test if lreg is negative */
			x86_test_reg_reg (code, ins->sreg2, ins->sreg2);
			br = code; x86_branch8 (code, X86_CC_GEZ, 0, TRUE);
	
			/* add correction constant mn */
			x86_fld80_mem (code, mn);
			x86_fld80_membase (code, X86_ESP, 0);
			x86_fp_op_reg (code, X86_FADD, 1, TRUE);
			x86_fst80_membase (code, X86_ESP, 0);

			x86_patch (br, code);

			x86_fld80_membase (code, X86_ESP, 0);
			x86_alu_reg_imm (code, X86_ADD, X86_ESP, 12);

			break;
		}
		case OP_LCONV_TO_OVF_I:
		case OP_LCONV_TO_OVF_I4_2: {
			guint8 *br [3], *label [1];
			MonoInst *tins;

			/* 
			 * Valid ints: 0xffffffff:8000000 to 00000000:0x7f000000
			 */
			x86_test_reg_reg (code, ins->sreg1, ins->sreg1);

			/* If the low word top bit is set, see if we are negative */
			br [0] = code; x86_branch8 (code, X86_CC_LT, 0, TRUE);
			/* We are not negative (no top bit set, check for our top word to be zero */
			x86_test_reg_reg (code, ins->sreg2, ins->sreg2);
			br [1] = code; x86_branch8 (code, X86_CC_EQ, 0, TRUE);
			label [0] = code;

			/* throw exception */
			tins = mono_branch_optimize_exception_target (cfg, bb, "OverflowException");
			if (tins) {
				mono_add_patch_info (cfg, code - cfg->native_code, MONO_PATCH_INFO_BB, tins->inst_true_bb);
				if ((cfg->opt & MONO_OPT_BRANCH) && x86_is_imm8 (tins->inst_true_bb->max_offset - cpos))
					x86_jump8 (code, 0);
				else
					x86_jump32 (code, 0);
			} else {
				mono_add_patch_info (cfg, code - cfg->native_code, MONO_PATCH_INFO_EXC, "OverflowException");
				x86_jump32 (code, 0);
			}
	
	
			x86_patch (br [0], code);
			/* our top bit is set, check that top word is 0xfffffff */
			x86_alu_reg_imm (code, X86_CMP, ins->sreg2, 0xffffffff);
		
			x86_patch (br [1], code);
			/* nope, emit exception */
			br [2] = code; x86_branch8 (code, X86_CC_NE, 0, TRUE);
			x86_patch (br [2], label [0]);

			if (ins->dreg != ins->sreg1)
				x86_mov_reg_reg (code, ins->dreg, ins->sreg1, 4);
			break;
		}
		case OP_FMOVE:
			/* Not needed on the fp stack */
			break;
		case OP_FADD:
			x86_fp_op_reg (code, X86_FADD, 1, TRUE);
			break;
		case OP_FSUB:
			x86_fp_op_reg (code, X86_FSUB, 1, TRUE);
			break;		
		case OP_FMUL:
			x86_fp_op_reg (code, X86_FMUL, 1, TRUE);
			break;		
		case OP_FDIV:
			x86_fp_op_reg (code, X86_FDIV, 1, TRUE);
			break;		
		case OP_FNEG:
			x86_fchs (code);
			break;		
		case OP_SIN:
			x86_fsin (code);
			x86_fldz (code);
			x86_fp_op_reg (code, X86_FADD, 1, TRUE);
			break;		
		case OP_COS:
			x86_fcos (code);
			x86_fldz (code);
			x86_fp_op_reg (code, X86_FADD, 1, TRUE);
			break;		
		case OP_ABS:
			x86_fabs (code);
			break;		
		case OP_TAN: {
			/* 
			 * it really doesn't make sense to inline all this code,
			 * it's here just to show that things may not be as simple 
			 * as they appear.
			 */
			guchar *check_pos, *end_tan, *pop_jump;
			x86_push_reg (code, X86_EAX);
			x86_fptan (code);
			x86_fnstsw (code);
			x86_test_reg_imm (code, X86_EAX, X86_FP_C2);
			check_pos = code;
			x86_branch8 (code, X86_CC_NE, 0, FALSE);
			x86_fstp (code, 0); /* pop the 1.0 */
			end_tan = code;
			x86_jump8 (code, 0);
			x86_fldpi (code);
			x86_fp_op (code, X86_FADD, 0);
			x86_fxch (code, 1);
			x86_fprem1 (code);
			x86_fstsw (code);
			x86_test_reg_imm (code, X86_EAX, X86_FP_C2);
			pop_jump = code;
			x86_branch8 (code, X86_CC_NE, 0, FALSE);
			x86_fstp (code, 1);
			x86_fptan (code);
			x86_patch (pop_jump, code);
			x86_fstp (code, 0); /* pop the 1.0 */
			x86_patch (check_pos, code);
			x86_patch (end_tan, code);
			x86_fldz (code);
			x86_fp_op_reg (code, X86_FADD, 1, TRUE);
			x86_pop_reg (code, X86_EAX);
			break;
		}
		case OP_ATAN:
			x86_fld1 (code);
			x86_fpatan (code);
			x86_fldz (code);
			x86_fp_op_reg (code, X86_FADD, 1, TRUE);
			break;		
		case OP_SQRT:
			x86_fsqrt (code);
			break;
		case OP_ROUND:
			x86_frndint (code);
			break;
		case OP_IMIN:
			g_assert (cfg->opt & MONO_OPT_CMOV);
			g_assert (ins->dreg == ins->sreg1);
			x86_alu_reg_reg (code, X86_CMP, ins->sreg1, ins->sreg2);
			x86_cmov_reg (code, X86_CC_GT, TRUE, ins->dreg, ins->sreg2);
			break;
		case OP_IMIN_UN:
			g_assert (cfg->opt & MONO_OPT_CMOV);
			g_assert (ins->dreg == ins->sreg1);
			x86_alu_reg_reg (code, X86_CMP, ins->sreg1, ins->sreg2);
			x86_cmov_reg (code, X86_CC_GT, FALSE, ins->dreg, ins->sreg2);
			break;
		case OP_IMAX:
			g_assert (cfg->opt & MONO_OPT_CMOV);
			g_assert (ins->dreg == ins->sreg1);
			x86_alu_reg_reg (code, X86_CMP, ins->sreg1, ins->sreg2);
			x86_cmov_reg (code, X86_CC_LT, TRUE, ins->dreg, ins->sreg2);
			break;
		case OP_IMAX_UN:
			g_assert (cfg->opt & MONO_OPT_CMOV);
			g_assert (ins->dreg == ins->sreg1);
			x86_alu_reg_reg (code, X86_CMP, ins->sreg1, ins->sreg2);
			x86_cmov_reg (code, X86_CC_LT, FALSE, ins->dreg, ins->sreg2);
			break;
		case OP_X86_FPOP:
			x86_fstp (code, 0);
			break;
		case OP_X86_FXCH:
			x86_fxch (code, ins->inst_imm);
			break;
		case OP_FREM: {
			guint8 *l1, *l2;

			x86_push_reg (code, X86_EAX);
			/* we need to exchange ST(0) with ST(1) */
			x86_fxch (code, 1);

			/* this requires a loop, because fprem somtimes 
			 * returns a partial remainder */
			l1 = code;
			/* looks like MS is using fprem instead of the IEEE compatible fprem1 */
			/* x86_fprem1 (code); */
			x86_fprem (code);
			x86_fnstsw (code);
			x86_alu_reg_imm (code, X86_AND, X86_EAX, X86_FP_C2);
			l2 = code + 2;
			x86_branch8 (code, X86_CC_NE, l1 - l2, FALSE);

			/* pop result */
			x86_fstp (code, 1);

			x86_pop_reg (code, X86_EAX);
			break;
		}
		case OP_FCOMPARE:
			if (cfg->opt & MONO_OPT_FCMOV) {
				x86_fcomip (code, 1);
				x86_fstp (code, 0);
				break;
			}
			/* this overwrites EAX */
			EMIT_FPCOMPARE(code);
			x86_alu_reg_imm (code, X86_AND, X86_EAX, X86_FP_CC_MASK);
			break;
		case OP_FCEQ:
			if (cfg->opt & MONO_OPT_FCMOV) {
				/* zeroing the register at the start results in 
				 * shorter and faster code (we can also remove the widening op)
				 */
				guchar *unordered_check;
				x86_alu_reg_reg (code, X86_XOR, ins->dreg, ins->dreg);
				x86_fcomip (code, 1);
				x86_fstp (code, 0);
				unordered_check = code;
				x86_branch8 (code, X86_CC_P, 0, FALSE);
				x86_set_reg (code, X86_CC_EQ, ins->dreg, FALSE);
				x86_patch (unordered_check, code);
				break;
			}
			if (ins->dreg != X86_EAX) 
				x86_push_reg (code, X86_EAX);

			EMIT_FPCOMPARE(code);
			x86_alu_reg_imm (code, X86_AND, X86_EAX, X86_FP_CC_MASK);
			x86_alu_reg_imm (code, X86_CMP, X86_EAX, 0x4000);
			x86_set_reg (code, X86_CC_EQ, ins->dreg, TRUE);
			x86_widen_reg (code, ins->dreg, ins->dreg, FALSE, FALSE);

			if (ins->dreg != X86_EAX) 
				x86_pop_reg (code, X86_EAX);
			break;
		case OP_FCLT:
		case OP_FCLT_UN:
			if (cfg->opt & MONO_OPT_FCMOV) {
				/* zeroing the register at the start results in 
				 * shorter and faster code (we can also remove the widening op)
				 */
				x86_alu_reg_reg (code, X86_XOR, ins->dreg, ins->dreg);
				x86_fcomip (code, 1);
				x86_fstp (code, 0);
				if (ins->opcode == OP_FCLT_UN) {
					guchar *unordered_check = code;
					guchar *jump_to_end;
					x86_branch8 (code, X86_CC_P, 0, FALSE);
					x86_set_reg (code, X86_CC_GT, ins->dreg, FALSE);
					jump_to_end = code;
					x86_jump8 (code, 0);
					x86_patch (unordered_check, code);
					x86_inc_reg (code, ins->dreg);
					x86_patch (jump_to_end, code);
				} else {
					x86_set_reg (code, X86_CC_GT, ins->dreg, FALSE);
				}
				break;
			}
			if (ins->dreg != X86_EAX) 
				x86_push_reg (code, X86_EAX);

			EMIT_FPCOMPARE(code);
			x86_alu_reg_imm (code, X86_AND, X86_EAX, X86_FP_CC_MASK);
			if (ins->opcode == OP_FCLT_UN) {
				guchar *is_not_zero_check, *end_jump;
				is_not_zero_check = code;
				x86_branch8 (code, X86_CC_NZ, 0, TRUE);
				end_jump = code;
				x86_jump8 (code, 0);
				x86_patch (is_not_zero_check, code);
				x86_alu_reg_imm (code, X86_CMP, X86_EAX, X86_FP_CC_MASK);

				x86_patch (end_jump, code);
			}
			x86_set_reg (code, X86_CC_EQ, ins->dreg, TRUE);
			x86_widen_reg (code, ins->dreg, ins->dreg, FALSE, FALSE);

			if (ins->dreg != X86_EAX) 
				x86_pop_reg (code, X86_EAX);
			break;
		case OP_FCGT:
		case OP_FCGT_UN:
			if (cfg->opt & MONO_OPT_FCMOV) {
				/* zeroing the register at the start results in 
				 * shorter and faster code (we can also remove the widening op)
				 */
				guchar *unordered_check;
				x86_alu_reg_reg (code, X86_XOR, ins->dreg, ins->dreg);
				x86_fcomip (code, 1);
				x86_fstp (code, 0);
				if (ins->opcode == OP_FCGT) {
					unordered_check = code;
					x86_branch8 (code, X86_CC_P, 0, FALSE);
					x86_set_reg (code, X86_CC_LT, ins->dreg, FALSE);
					x86_patch (unordered_check, code);
				} else {
					x86_set_reg (code, X86_CC_LT, ins->dreg, FALSE);
				}
				break;
			}
			if (ins->dreg != X86_EAX) 
				x86_push_reg (code, X86_EAX);

			EMIT_FPCOMPARE(code);
			x86_alu_reg_imm (code, X86_AND, X86_EAX, X86_FP_CC_MASK);
			x86_alu_reg_imm (code, X86_CMP, X86_EAX, X86_FP_C0);
			if (ins->opcode == OP_FCGT_UN) {
				guchar *is_not_zero_check, *end_jump;
				is_not_zero_check = code;
				x86_branch8 (code, X86_CC_NZ, 0, TRUE);
				end_jump = code;
				x86_jump8 (code, 0);
				x86_patch (is_not_zero_check, code);
				x86_alu_reg_imm (code, X86_CMP, X86_EAX, X86_FP_CC_MASK);
	
				x86_patch (end_jump, code);
			}
			x86_set_reg (code, X86_CC_EQ, ins->dreg, TRUE);
			x86_widen_reg (code, ins->dreg, ins->dreg, FALSE, FALSE);

			if (ins->dreg != X86_EAX) 
				x86_pop_reg (code, X86_EAX);
			break;
		case OP_FBEQ:
			if (cfg->opt & MONO_OPT_FCMOV) {
				guchar *jump = code;
				x86_branch8 (code, X86_CC_P, 0, TRUE);
				EMIT_COND_BRANCH (ins, X86_CC_EQ, FALSE);
				x86_patch (jump, code);
				break;
			}
			x86_alu_reg_imm (code, X86_CMP, X86_EAX, 0x4000);
			EMIT_COND_BRANCH (ins, X86_CC_EQ, TRUE);
			break;
		case OP_FBNE_UN:
			/* Branch if C013 != 100 */
			if (cfg->opt & MONO_OPT_FCMOV) {
				/* branch if !ZF or (PF|CF) */
				EMIT_COND_BRANCH (ins, X86_CC_NE, FALSE);
				EMIT_COND_BRANCH (ins, X86_CC_P, FALSE);
				EMIT_COND_BRANCH (ins, X86_CC_B, FALSE);
				break;
			}
			x86_alu_reg_imm (code, X86_CMP, X86_EAX, X86_FP_C3);
			EMIT_COND_BRANCH (ins, X86_CC_NE, FALSE);
			break;
		case OP_FBLT:
			if (cfg->opt & MONO_OPT_FCMOV) {
				EMIT_COND_BRANCH (ins, X86_CC_GT, FALSE);
				break;
			}
			EMIT_COND_BRANCH (ins, X86_CC_EQ, FALSE);
			break;
		case OP_FBLT_UN:
			if (cfg->opt & MONO_OPT_FCMOV) {
				EMIT_COND_BRANCH (ins, X86_CC_P, FALSE);
				EMIT_COND_BRANCH (ins, X86_CC_GT, FALSE);
				break;
			}
			if (ins->opcode == OP_FBLT_UN) {
				guchar *is_not_zero_check, *end_jump;
				is_not_zero_check = code;
				x86_branch8 (code, X86_CC_NZ, 0, TRUE);
				end_jump = code;
				x86_jump8 (code, 0);
				x86_patch (is_not_zero_check, code);
				x86_alu_reg_imm (code, X86_CMP, X86_EAX, X86_FP_CC_MASK);

				x86_patch (end_jump, code);
			}
			EMIT_COND_BRANCH (ins, X86_CC_EQ, FALSE);
			break;
		case OP_FBGT:
		case OP_FBGT_UN:
			if (cfg->opt & MONO_OPT_FCMOV) {
				if (ins->opcode == OP_FBGT) {
					guchar *br1;

					/* skip branch if C1=1 */
					br1 = code;
					x86_branch8 (code, X86_CC_P, 0, FALSE);
					/* branch if (C0 | C3) = 1 */
					EMIT_COND_BRANCH (ins, X86_CC_LT, FALSE);
					x86_patch (br1, code);
				} else {
					EMIT_COND_BRANCH (ins, X86_CC_LT, FALSE);
				}
				break;
			}
			x86_alu_reg_imm (code, X86_CMP, X86_EAX, X86_FP_C0);
			if (ins->opcode == OP_FBGT_UN) {
				guchar *is_not_zero_check, *end_jump;
				is_not_zero_check = code;
				x86_branch8 (code, X86_CC_NZ, 0, TRUE);
				end_jump = code;
				x86_jump8 (code, 0);
				x86_patch (is_not_zero_check, code);
				x86_alu_reg_imm (code, X86_CMP, X86_EAX, X86_FP_CC_MASK);

				x86_patch (end_jump, code);
			}
			EMIT_COND_BRANCH (ins, X86_CC_EQ, FALSE);
			break;
		case OP_FBGE:
			/* Branch if C013 == 100 or 001 */
			if (cfg->opt & MONO_OPT_FCMOV) {
				guchar *br1;

				/* skip branch if C1=1 */
				br1 = code;
				x86_branch8 (code, X86_CC_P, 0, FALSE);
				/* branch if (C0 | C3) = 1 */
				EMIT_COND_BRANCH (ins, X86_CC_BE, FALSE);
				x86_patch (br1, code);
				break;
			}
			x86_alu_reg_imm (code, X86_CMP, X86_EAX, X86_FP_C0);
			EMIT_COND_BRANCH (ins, X86_CC_EQ, FALSE);
			x86_alu_reg_imm (code, X86_CMP, X86_EAX, X86_FP_C3);
			EMIT_COND_BRANCH (ins, X86_CC_EQ, FALSE);
			break;
		case OP_FBGE_UN:
			/* Branch if C013 == 000 */
			if (cfg->opt & MONO_OPT_FCMOV) {
				EMIT_COND_BRANCH (ins, X86_CC_LE, FALSE);
				break;
			}
			EMIT_COND_BRANCH (ins, X86_CC_NE, FALSE);
			break;
		case OP_FBLE:
			/* Branch if C013=000 or 100 */
			if (cfg->opt & MONO_OPT_FCMOV) {
				guchar *br1;

				/* skip branch if C1=1 */
				br1 = code;
				x86_branch8 (code, X86_CC_P, 0, FALSE);
				/* branch if C0=0 */
				EMIT_COND_BRANCH (ins, X86_CC_NB, FALSE);
				x86_patch (br1, code);
				break;
			}
			x86_alu_reg_imm (code, X86_AND, X86_EAX, (X86_FP_C0|X86_FP_C1));
			x86_alu_reg_imm (code, X86_CMP, X86_EAX, 0);
			EMIT_COND_BRANCH (ins, X86_CC_EQ, FALSE);
			break;
		case OP_FBLE_UN:
			/* Branch if C013 != 001 */
			if (cfg->opt & MONO_OPT_FCMOV) {
				EMIT_COND_BRANCH (ins, X86_CC_P, FALSE);
				EMIT_COND_BRANCH (ins, X86_CC_GE, FALSE);
				break;
			}
			x86_alu_reg_imm (code, X86_CMP, X86_EAX, X86_FP_C0);
			EMIT_COND_BRANCH (ins, X86_CC_NE, FALSE);
			break;
		case OP_CKFINITE: {
			guchar *br1;
			x86_push_reg (code, X86_EAX);
			x86_fxam (code);
			x86_fnstsw (code);
			x86_alu_reg_imm (code, X86_AND, X86_EAX, 0x4100);
			x86_alu_reg_imm (code, X86_CMP, X86_EAX, X86_FP_C0);
			x86_pop_reg (code, X86_EAX);

			/* Have to clean up the fp stack before throwing the exception */
			br1 = code;
			x86_branch8 (code, X86_CC_NE, 0, FALSE);

			x86_fstp (code, 0);			
			EMIT_COND_SYSTEM_EXCEPTION (X86_CC_EQ, FALSE, "ArithmeticException");

			x86_patch (br1, code);
			break;
		}
		case OP_TLS_GET: {
			code = mono_x86_emit_tls_get (code, ins->dreg, ins->inst_offset);
			break;
		}
		case OP_MEMORY_BARRIER: {
			/* Not needed on x86 */
			break;
		}
		case OP_ATOMIC_ADD_I4: {
			int dreg = ins->dreg;

			if (dreg == ins->inst_basereg) {
				x86_push_reg (code, ins->sreg2);
				dreg = ins->sreg2;
			} 
			
			if (dreg != ins->sreg2)
				x86_mov_reg_reg (code, ins->dreg, ins->sreg2, 4);

			x86_prefix (code, X86_LOCK_PREFIX);
			x86_xadd_membase_reg (code, ins->inst_basereg, ins->inst_offset, dreg, 4);

			if (dreg != ins->dreg) {
				x86_mov_reg_reg (code, ins->dreg, dreg, 4);
				x86_pop_reg (code, dreg);
			}

			break;
		}
		case OP_ATOMIC_ADD_NEW_I4: {
			int dreg = ins->dreg;

			/* hack: limit in regalloc, dreg != sreg1 && dreg != sreg2 */
			if (ins->sreg2 == dreg) {
				if (dreg == X86_EBX) {
					dreg = X86_EDI;
					if (ins->inst_basereg == X86_EDI)
						dreg = X86_ESI;
				} else {
					dreg = X86_EBX;
					if (ins->inst_basereg == X86_EBX)
						dreg = X86_EDI;
				}
			} else if (ins->inst_basereg == dreg) {
				if (dreg == X86_EBX) {
					dreg = X86_EDI;
					if (ins->sreg2 == X86_EDI)
						dreg = X86_ESI;
				} else {
					dreg = X86_EBX;
					if (ins->sreg2 == X86_EBX)
						dreg = X86_EDI;
				}
			}

			if (dreg != ins->dreg) {
				x86_push_reg (code, dreg);
			}

			x86_mov_reg_reg (code, dreg, ins->sreg2, 4);
			x86_prefix (code, X86_LOCK_PREFIX);
			x86_xadd_membase_reg (code, ins->inst_basereg, ins->inst_offset, dreg, 4);
			/* dreg contains the old value, add with sreg2 value */
			x86_alu_reg_reg (code, X86_ADD, dreg, ins->sreg2);
			
			if (ins->dreg != dreg) {
				x86_mov_reg_reg (code, ins->dreg, dreg, 4);
				x86_pop_reg (code, dreg);
			}

			break;
		}
		case OP_ATOMIC_EXCHANGE_I4:
		case OP_ATOMIC_CAS_IMM_I4: {
			guchar *br[2];
			int sreg2 = ins->sreg2;
			int breg = ins->inst_basereg;

			/* cmpxchg uses eax as comperand, need to make sure we can use it
			 * hack to overcome limits in x86 reg allocator 
			 * (req: dreg == eax and sreg2 != eax and breg != eax) 
			 */
			g_assert (ins->dreg == X86_EAX);
			
			/* We need the EAX reg for the cmpxchg */
			if (ins->sreg2 == X86_EAX) {
				sreg2 = (breg == X86_EDX) ? X86_EBX : X86_EDX;
				x86_push_reg (code, sreg2);
				x86_mov_reg_reg (code, sreg2, X86_EAX, 4);
			}

			if (breg == X86_EAX) {
				breg = (sreg2 == X86_ESI) ? X86_EDI : X86_ESI;
				x86_push_reg (code, breg);
				x86_mov_reg_reg (code, breg, X86_EAX, 4);
			}

			if (ins->opcode == OP_ATOMIC_CAS_IMM_I4) {
				x86_mov_reg_imm (code, X86_EAX, ins->backend.data);

				x86_prefix (code, X86_LOCK_PREFIX);
				x86_cmpxchg_membase_reg (code, breg, ins->inst_offset, sreg2);
			} else {
				x86_mov_reg_membase (code, X86_EAX, breg, ins->inst_offset, 4);

				br [0] = code; x86_prefix (code, X86_LOCK_PREFIX);
				x86_cmpxchg_membase_reg (code, breg, ins->inst_offset, sreg2);
				br [1] = code; x86_branch8 (code, X86_CC_NE, -1, FALSE);
				x86_patch (br [1], br [0]);
			}

			if (breg != ins->inst_basereg)
				x86_pop_reg (code, breg);

			if (ins->sreg2 != sreg2)
				x86_pop_reg (code, sreg2);

			break;
		}
#ifdef MONO_ARCH_SIMD_INTRINSICS
		case OP_ADDPS:
			x86_sse_alu_ps_reg_reg (code, X86_SSE_ADD, ins->sreg1, ins->sreg2);
			break;
		case OP_DIVPS:
			x86_sse_alu_ps_reg_reg (code, X86_SSE_DIV, ins->sreg1, ins->sreg2);
			break;
		case OP_MULPS:
			x86_sse_alu_ps_reg_reg (code, X86_SSE_MUL, ins->sreg1, ins->sreg2);
			break;
		case OP_SUBPS:
			x86_sse_alu_ps_reg_reg (code, X86_SSE_SUB, ins->sreg1, ins->sreg2);
			break;
		case OP_MAXPS:
			x86_sse_alu_ps_reg_reg (code, X86_SSE_MAX, ins->sreg1, ins->sreg2);
			break;
		case OP_MINPS:
			x86_sse_alu_ps_reg_reg (code, X86_SSE_MIN, ins->sreg1, ins->sreg2);
			break;
		case OP_COMPPS:
			g_assert (ins->inst_c0 >= 0 && ins->inst_c0 <= 7);
			x86_sse_alu_ps_reg_reg_imm (code, X86_SSE_COMP, ins->sreg1, ins->sreg2, ins->inst_c0);
			break;
		case OP_ANDPS:
			x86_sse_alu_ps_reg_reg (code, X86_SSE_AND, ins->sreg1, ins->sreg2);
			break;
		case OP_ANDNPS:
			x86_sse_alu_ps_reg_reg (code, X86_SSE_ANDN, ins->sreg1, ins->sreg2);
			break;
		case OP_ORPS:
			x86_sse_alu_ps_reg_reg (code, X86_SSE_OR, ins->sreg1, ins->sreg2);
			break;
		case OP_XORPS:
			x86_sse_alu_ps_reg_reg (code, X86_SSE_XOR, ins->sreg1, ins->sreg2);
			break;
		case OP_SQRTPS:
			x86_sse_alu_ps_reg_reg (code, X86_SSE_SQRT, ins->dreg, ins->sreg1);
			break;
		case OP_RSQRTPS:
			x86_sse_alu_ps_reg_reg (code, X86_SSE_RSQRT, ins->dreg, ins->sreg1);
			break;
		case OP_RCPPS:
			x86_sse_alu_ps_reg_reg (code, X86_SSE_RCP, ins->dreg, ins->sreg1);
			break;
		case OP_ADDSUBPS:
			x86_sse_alu_sd_reg_reg (code, X86_SSE_ADDSUB, ins->sreg1, ins->sreg2);
			break;
		case OP_HADDPS:
			x86_sse_alu_sd_reg_reg (code, X86_SSE_HADD, ins->sreg1, ins->sreg2);
			break;
		case OP_HSUBPS:
			x86_sse_alu_sd_reg_reg (code, X86_SSE_HSUB, ins->sreg1, ins->sreg2);
			break;
		case OP_DUPPS_HIGH:
			x86_sse_alu_ss_reg_reg (code, X86_SSE_MOVSHDUP, ins->dreg, ins->sreg1);
			break;
		case OP_DUPPS_LOW:
			x86_sse_alu_ss_reg_reg (code, X86_SSE_MOVSLDUP, ins->dreg, ins->sreg1);
			break;

		case OP_PSHUFLEW_HIGH:
			g_assert (ins->inst_c0 >= 0 && ins->inst_c0 <= 0xFF);
			x86_pshufw_reg_reg (code, ins->dreg, ins->sreg1, ins->inst_c0, 1);
			break;
		case OP_PSHUFLEW_LOW:
			g_assert (ins->inst_c0 >= 0 && ins->inst_c0 <= 0xFF);
			x86_pshufw_reg_reg (code, ins->dreg, ins->sreg1, ins->inst_c0, 0);
			break;
		case OP_PSHUFLED:
			g_assert (ins->inst_c0 >= 0 && ins->inst_c0 <= 0xFF);
			x86_sse_shift_reg_imm (code, X86_SSE_PSHUFD, ins->dreg, ins->sreg1, ins->inst_c0);
			break;

		case OP_ADDPD:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_ADD, ins->sreg1, ins->sreg2);
			break;
		case OP_DIVPD:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_DIV, ins->sreg1, ins->sreg2);
			break;
		case OP_MULPD:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_MUL, ins->sreg1, ins->sreg2);
			break;
		case OP_SUBPD:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_SUB, ins->sreg1, ins->sreg2);
			break;
		case OP_MAXPD:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_MAX, ins->sreg1, ins->sreg2);
			break;
		case OP_MINPD:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_MIN, ins->sreg1, ins->sreg2);
			break;
		case OP_COMPPD:
			g_assert (ins->inst_c0 >= 0 && ins->inst_c0 <= 7);
			x86_sse_alu_pd_reg_reg_imm (code, X86_SSE_COMP, ins->sreg1, ins->sreg2, ins->inst_c0);
			break;
		case OP_ANDPD:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_AND, ins->sreg1, ins->sreg2);
			break;
		case OP_ANDNPD:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_ANDN, ins->sreg1, ins->sreg2);
			break;
		case OP_ORPD:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_OR, ins->sreg1, ins->sreg2);
			break;
		case OP_XORPD:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_XOR, ins->sreg1, ins->sreg2);
			break;
		case OP_ADDSUBPD:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_ADDSUB, ins->sreg1, ins->sreg2);
			break;
		case OP_HADDPD:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_HADD, ins->sreg1, ins->sreg2);
			break;
		case OP_HSUBPD:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_HSUB, ins->sreg1, ins->sreg2);
			break;
		case OP_DUPPD:
			x86_sse_alu_sd_reg_reg (code, X86_SSE_MOVDDUP, ins->dreg, ins->sreg1);
			break;
			
		case OP_EXTRACT_MASK:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_PMOVMSKB, ins->dreg, ins->sreg1);
			break;
	
		case OP_PAND:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_PAND, ins->sreg1, ins->sreg2);
			break;
		case OP_POR:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_POR, ins->sreg1, ins->sreg2);
			break;
		case OP_PXOR:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_PXOR, ins->sreg1, ins->sreg2);
			break;

		case OP_PADDB:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_PADDB, ins->sreg1, ins->sreg2);
			break;
		case OP_PADDW:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_PADDW, ins->sreg1, ins->sreg2);
			break;
		case OP_PADDD:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_PADDD, ins->sreg1, ins->sreg2);
			break;
		case OP_PADDQ:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_PADDQ, ins->sreg1, ins->sreg2);
			break;

		case OP_PSUBB:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_PSUBB, ins->sreg1, ins->sreg2);
			break;
		case OP_PSUBW:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_PSUBW, ins->sreg1, ins->sreg2);
			break;
		case OP_PSUBD:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_PSUBD, ins->sreg1, ins->sreg2);
			break;
		case OP_PSUBQ:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_PSUBQ, ins->sreg1, ins->sreg2);
			break;

		case OP_PMAXB_UN:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_PMAXUB, ins->sreg1, ins->sreg2);
			break;
		case OP_PMAXW_UN:
			x86_sse_alu_sse41_reg_reg (code, X86_SSE_PMAXUW, ins->sreg1, ins->sreg2);
			break;
		case OP_PMAXD_UN:
			x86_sse_alu_sse41_reg_reg (code, X86_SSE_PMAXUD, ins->sreg1, ins->sreg2);
			break;
		
		case OP_PMAXB:
			x86_sse_alu_sse41_reg_reg (code, X86_SSE_PMAXSB, ins->sreg1, ins->sreg2);
			break;
		case OP_PMAXW:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_PMAXSW, ins->sreg1, ins->sreg2);
			break;
		case OP_PMAXD:
			x86_sse_alu_sse41_reg_reg (code, X86_SSE_PMAXSD, ins->sreg1, ins->sreg2);
			break;

		case OP_PAVGB_UN:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_PAVGB, ins->sreg1, ins->sreg2);
			break;
		case OP_PAVGW_UN:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_PAVGW, ins->sreg1, ins->sreg2);
			break;

		case OP_PMINB_UN:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_PMINUB, ins->sreg1, ins->sreg2);
			break;
		case OP_PMINW_UN:
			x86_sse_alu_sse41_reg_reg (code, X86_SSE_PMINUW, ins->sreg1, ins->sreg2);
			break;
		case OP_PMIND_UN:
			x86_sse_alu_sse41_reg_reg (code, X86_SSE_PMINUD, ins->sreg1, ins->sreg2);
			break;

		case OP_PMINB:
			x86_sse_alu_sse41_reg_reg (code, X86_SSE_PMINSB, ins->sreg1, ins->sreg2);
			break;
		case OP_PMINW:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_PMINSW, ins->sreg1, ins->sreg2);
			break;
		case OP_PMIND:
			x86_sse_alu_sse41_reg_reg (code, X86_SSE_PMINSD, ins->sreg1, ins->sreg2);
			break;

		case OP_PCMPEQB:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_PCMPEQB, ins->sreg1, ins->sreg2);
			break;
		case OP_PCMPEQW:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_PCMPEQW, ins->sreg1, ins->sreg2);
			break;
		case OP_PCMPEQD:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_PCMPEQD, ins->sreg1, ins->sreg2);
			break;
		case OP_PCMPEQQ:
			x86_sse_alu_sse41_reg_reg (code, X86_SSE_PCMPEQQ, ins->sreg1, ins->sreg2);
			break;

		case OP_PCMPGTB:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_PCMPGTB, ins->sreg1, ins->sreg2);
			break;
		case OP_PCMPGTW:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_PCMPGTW, ins->sreg1, ins->sreg2);
			break;
		case OP_PCMPGTD:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_PCMPGTD, ins->sreg1, ins->sreg2);
			break;
		case OP_PCMPGTQ:
			x86_sse_alu_sse41_reg_reg (code, X86_SSE_PCMPGTQ, ins->sreg1, ins->sreg2);
			break;

		case OP_PSUM_ABS_DIFF:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_PSADBW, ins->sreg1, ins->sreg2);
			break;

		case OP_UNPACK_LOWB:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_PUNPCKLBW, ins->sreg1, ins->sreg2);
			break;
		case OP_UNPACK_LOWW:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_PUNPCKLWD, ins->sreg1, ins->sreg2);
			break;
		case OP_UNPACK_LOWD:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_PUNPCKLDQ, ins->sreg1, ins->sreg2);
			break;
		case OP_UNPACK_LOWQ:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_PUNPCKLQDQ, ins->sreg1, ins->sreg2);
			break;
		case OP_UNPACK_LOWPS:
			x86_sse_alu_ps_reg_reg (code, X86_SSE_UNPCKL, ins->sreg1, ins->sreg2);
			break;
		case OP_UNPACK_LOWPD:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_UNPCKL, ins->sreg1, ins->sreg2);
			break;

		case OP_UNPACK_HIGHB:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_PUNPCKHBW, ins->sreg1, ins->sreg2);
			break;
		case OP_UNPACK_HIGHW:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_PUNPCKHWD, ins->sreg1, ins->sreg2);
			break;
		case OP_UNPACK_HIGHD:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_PUNPCKHDQ, ins->sreg1, ins->sreg2);
			break;
		case OP_UNPACK_HIGHQ:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_PUNPCKHQDQ, ins->sreg1, ins->sreg2);
			break;
		case OP_UNPACK_HIGHPS:
			x86_sse_alu_ps_reg_reg (code, X86_SSE_UNPCKH, ins->sreg1, ins->sreg2);
			break;
		case OP_UNPACK_HIGHPD:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_UNPCKH, ins->sreg1, ins->sreg2);
			break;

		case OP_PACKW:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_PACKSSWB, ins->sreg1, ins->sreg2);
			break;
		case OP_PACKD:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_PACKSSDW, ins->sreg1, ins->sreg2);
			break;
		case OP_PACKW_UN:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_PACKUSWB, ins->sreg1, ins->sreg2);
			break;
		case OP_PACKD_UN:
			x86_sse_alu_sse41_reg_reg (code, X86_SSE_PACKUSDW, ins->sreg1, ins->sreg2);
			break;

		case OP_PADDB_SAT_UN:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_PADDUSB, ins->sreg1, ins->sreg2);
			break;
		case OP_PSUBB_SAT_UN:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_PSUBUSB, ins->sreg1, ins->sreg2);
			break;
		case OP_PADDW_SAT_UN:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_PADDUSW, ins->sreg1, ins->sreg2);
			break;
		case OP_PSUBW_SAT_UN:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_PSUBUSW, ins->sreg1, ins->sreg2);
			break;

		case OP_PADDB_SAT:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_PADDSB, ins->sreg1, ins->sreg2);
			break;
		case OP_PSUBB_SAT:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_PSUBSB, ins->sreg1, ins->sreg2);
			break;
		case OP_PADDW_SAT:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_PADDSW, ins->sreg1, ins->sreg2);
			break;
		case OP_PSUBW_SAT:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_PSUBSW, ins->sreg1, ins->sreg2);
			break;
			
		case OP_PMULW:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_PMULLW, ins->sreg1, ins->sreg2);
			break;
		case OP_PMULD:
			x86_sse_alu_sse41_reg_reg (code, X86_SSE_PMULLD, ins->sreg1, ins->sreg2);
			break;
		case OP_PMULQ:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_PMULUDQ, ins->sreg1, ins->sreg2);
			break;
		case OP_PMULW_HIGH_UN:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_PMULHUW, ins->sreg1, ins->sreg2);
			break;
		case OP_PMULW_HIGH:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_PMULHW, ins->sreg1, ins->sreg2);
			break;

		case OP_PSHRW:
			x86_sse_shift_reg_imm (code, X86_SSE_PSHIFTW, X86_SSE_SHR, ins->dreg, ins->inst_imm);
			break;
		case OP_PSHRW_REG:
			x86_sse_shift_reg_reg (code, X86_SSE_PSRLW_REG, ins->dreg, ins->sreg2);
			break;

		case OP_PSARW:
			x86_sse_shift_reg_imm (code, X86_SSE_PSHIFTW, X86_SSE_SAR, ins->dreg, ins->inst_imm);
			break;
		case OP_PSARW_REG:
			x86_sse_shift_reg_reg (code, X86_SSE_PSRAW_REG, ins->dreg, ins->sreg2);
			break;

		case OP_PSHLW:
			x86_sse_shift_reg_imm (code, X86_SSE_PSHIFTW, X86_SSE_SHL, ins->dreg, ins->inst_imm);
			break;
		case OP_PSHLW_REG:
			x86_sse_shift_reg_reg (code, X86_SSE_PSLLW_REG, ins->dreg, ins->sreg2);
			break;

		case OP_PSHRD:
			x86_sse_shift_reg_imm (code, X86_SSE_PSHIFTD, X86_SSE_SHR, ins->dreg, ins->inst_imm);
			break;
		case OP_PSHRD_REG:
			x86_sse_shift_reg_reg (code, X86_SSE_PSRLD_REG, ins->dreg, ins->sreg2);
			break;

		case OP_PSARD:
			x86_sse_shift_reg_imm (code, X86_SSE_PSHIFTD, X86_SSE_SAR, ins->dreg, ins->inst_imm);
			break;
		case OP_PSARD_REG:
			x86_sse_shift_reg_reg (code, X86_SSE_PSRAD_REG, ins->dreg, ins->sreg2);
			break;

		case OP_PSHLD:
			x86_sse_shift_reg_imm (code, X86_SSE_PSHIFTD, X86_SSE_SHL, ins->dreg, ins->inst_imm);
			break;
		case OP_PSHLD_REG:
			x86_sse_shift_reg_reg (code, X86_SSE_PSLLD_REG, ins->dreg, ins->sreg2);
			break;

		case OP_PSHRQ:
			x86_sse_shift_reg_imm (code, X86_SSE_PSHIFTQ, X86_SSE_SHR, ins->dreg, ins->inst_imm);
			break;
		case OP_PSHRQ_REG:
			x86_sse_shift_reg_reg (code, X86_SSE_PSRLQ_REG, ins->dreg, ins->sreg2);
			break;

		case OP_PSHLQ:
			x86_sse_shift_reg_imm (code, X86_SSE_PSHIFTQ, X86_SSE_SHL, ins->dreg, ins->inst_imm);
			break;
		case OP_PSHLQ_REG:
			x86_sse_shift_reg_reg (code, X86_SSE_PSLLQ_REG, ins->dreg, ins->sreg2);
			break;		
			
		case OP_ICONV_TO_X:
			x86_movd_xreg_reg (code, ins->dreg, ins->sreg1);
			break;
		case OP_EXTRACT_I4:
			x86_movd_reg_xreg (code, ins->dreg, ins->sreg1);
			break;
		case OP_EXTRACT_I1:
		case OP_EXTRACT_U1:
			x86_movd_reg_xreg (code, ins->dreg, ins->sreg1);
			if (ins->inst_c0)
				x86_shift_reg_imm (code, X86_SHR, ins->dreg, ins->inst_c0 * 8);
			x86_widen_reg (code, ins->dreg, ins->dreg, ins->opcode == OP_EXTRACT_I1, FALSE);
			break;
		case OP_EXTRACT_I2:
		case OP_EXTRACT_U2:
			x86_movd_reg_xreg (code, ins->dreg, ins->sreg1);
			if (ins->inst_c0)
				x86_shift_reg_imm (code, X86_SHR, ins->dreg, 16);
			x86_widen_reg (code, ins->dreg, ins->dreg, ins->opcode == OP_EXTRACT_I2, TRUE);
			break;
		case OP_EXTRACT_R8:
			if (ins->inst_c0)
				x86_sse_alu_pd_membase_reg (code, X86_SSE_MOVHPD_MEMBASE_REG, ins->backend.spill_var->inst_basereg, ins->backend.spill_var->inst_offset, ins->sreg1);
			else
				x86_sse_alu_sd_membase_reg (code, X86_SSE_MOVSD_MEMBASE_REG, ins->backend.spill_var->inst_basereg, ins->backend.spill_var->inst_offset, ins->sreg1);
			x86_fld_membase (code, ins->backend.spill_var->inst_basereg, ins->backend.spill_var->inst_offset, TRUE);
			break;

		case OP_INSERT_I2:
			x86_sse_alu_pd_reg_reg_imm (code, X86_SSE_PINSRW, ins->sreg1, ins->sreg2, ins->inst_c0);
			break;
		case OP_EXTRACTX_U2:
			x86_sse_alu_pd_reg_reg_imm (code, X86_SSE_PEXTRW, ins->dreg, ins->sreg1, ins->inst_c0);
			break;
		case OP_INSERTX_U1_SLOW:
			/*sreg1 is the extracted ireg (scratch)
			/sreg2 is the to be inserted ireg (scratch)
			/dreg is the xreg to receive the value*/

			/*clear the bits from the extracted word*/
			x86_alu_reg_imm (code, X86_AND, ins->sreg1, ins->inst_c0 & 1 ? 0x00FF : 0xFF00);
			/*shift the value to insert if needed*/
			if (ins->inst_c0 & 1)
				x86_shift_reg_imm (code, X86_SHL, ins->sreg2, 8);
			/*join them together*/
			x86_alu_reg_reg (code, X86_OR, ins->sreg1, ins->sreg2);
			x86_sse_alu_pd_reg_reg_imm (code, X86_SSE_PINSRW, ins->dreg, ins->sreg1, ins->inst_c0 / 2);
			break;
		case OP_INSERTX_I4_SLOW:
			x86_sse_alu_pd_reg_reg_imm (code, X86_SSE_PINSRW, ins->dreg, ins->sreg2, ins->inst_c0 * 2);
			x86_shift_reg_imm (code, X86_SHR, ins->sreg2, 16);
			x86_sse_alu_pd_reg_reg_imm (code, X86_SSE_PINSRW, ins->dreg, ins->sreg2, ins->inst_c0 * 2 + 1);
			break;

		case OP_INSERTX_R4_SLOW:
			x86_fst_membase (code, ins->backend.spill_var->inst_basereg, ins->backend.spill_var->inst_offset, FALSE, TRUE);
			/*TODO if inst_c0 == 0 use movss*/
			x86_sse_alu_pd_reg_membase_imm (code, X86_SSE_PINSRW, ins->dreg, ins->backend.spill_var->inst_basereg, ins->backend.spill_var->inst_offset + 0, ins->inst_c0 * 2);
			x86_sse_alu_pd_reg_membase_imm (code, X86_SSE_PINSRW, ins->dreg, ins->backend.spill_var->inst_basereg, ins->backend.spill_var->inst_offset + 2, ins->inst_c0 * 2 + 1);
			break;
		case OP_INSERTX_R8_SLOW:
			x86_fst_membase (code, ins->backend.spill_var->inst_basereg, ins->backend.spill_var->inst_offset, TRUE, TRUE);
			if (ins->inst_c0)
				x86_sse_alu_pd_reg_membase (code, X86_SSE_MOVHPD_REG_MEMBASE, ins->dreg, ins->backend.spill_var->inst_basereg, ins->backend.spill_var->inst_offset);
			else
				x86_sse_alu_pd_reg_membase (code, X86_SSE_MOVSD_REG_MEMBASE, ins->dreg, ins->backend.spill_var->inst_basereg, ins->backend.spill_var->inst_offset);
			break;

		case OP_STOREX_MEMBASE_REG:
		case OP_STOREX_MEMBASE:
			x86_movups_membase_reg (code, ins->dreg, ins->inst_offset, ins->sreg1);
			break;
		case OP_LOADX_MEMBASE:
			x86_movups_reg_membase (code, ins->dreg, ins->sreg1, ins->inst_offset);
			break;
		case OP_LOADX_ALIGNED_MEMBASE:
			x86_movaps_reg_membase (code, ins->dreg, ins->sreg1, ins->inst_offset);
			break;
		case OP_STOREX_ALIGNED_MEMBASE_REG:
			x86_movaps_membase_reg (code, ins->dreg, ins->inst_offset, ins->sreg1);
			break;
		case OP_STOREX_NTA_MEMBASE_REG:
			x86_sse_alu_reg_membase (code, X86_SSE_MOVNTPS, ins->dreg, ins->sreg1, ins->inst_offset);
			break;
		case OP_PREFETCH_MEMBASE:
			x86_sse_alu_reg_membase (code, X86_SSE_PREFETCH, ins->backend.arg_info, ins->sreg1, ins->inst_offset);

			break;
		case OP_XMOVE:
			/*FIXME the peephole pass should have killed this*/
			if (ins->dreg != ins->sreg1)
				x86_movaps_reg_reg (code, ins->dreg, ins->sreg1);
			break;		
		case OP_XZERO:
			x86_sse_alu_pd_reg_reg (code, X86_SSE_PXOR, ins->dreg, ins->dreg);
			break;
		case OP_ICONV_TO_R8_RAW:
			x86_mov_membase_reg (code, ins->backend.spill_var->inst_basereg, ins->backend.spill_var->inst_offset, ins->sreg1, 4);
			x86_fld_membase (code, ins->backend.spill_var->inst_basereg, ins->backend.spill_var->inst_offset, FALSE);
			break;

		case OP_FCONV_TO_R8_X:
			x86_fst_membase (code, ins->backend.spill_var->inst_basereg, ins->backend.spill_var->inst_offset, TRUE, TRUE);
			x86_movsd_reg_membase (code, ins->dreg, ins->backend.spill_var->inst_basereg, ins->backend.spill_var->inst_offset);
			break;

		case OP_XCONV_R8_TO_I4:
			x86_cvttsd2si (code, ins->dreg, ins->sreg1);
			switch (ins->backend.source_opcode) {
			case OP_FCONV_TO_I1:
				x86_widen_reg (code, ins->dreg, ins->dreg, TRUE, FALSE);
				break;
			case OP_FCONV_TO_U1:
				x86_widen_reg (code, ins->dreg, ins->dreg, FALSE, FALSE);
				break;
			case OP_FCONV_TO_I2:
				x86_widen_reg (code, ins->dreg, ins->dreg, TRUE, TRUE);
				break;
			case OP_FCONV_TO_U2:
				x86_widen_reg (code, ins->dreg, ins->dreg, FALSE, TRUE);
				break;
			}			
			break;

		case OP_EXPAND_I1:
			/*FIXME this causes a partial register stall, maybe it would not be that bad to use shift + mask + or*/
			/*The +4 is to get a mov ?h, ?l over the same reg.*/
			x86_mov_reg_reg (code, ins->dreg + 4, ins->dreg, 1);
			x86_sse_alu_pd_reg_reg_imm (code, X86_SSE_PINSRW, ins->dreg, ins->sreg1, 0);
			x86_sse_alu_pd_reg_reg_imm (code, X86_SSE_PINSRW, ins->dreg, ins->sreg1, 1);
			x86_sse_shift_reg_imm (code, X86_SSE_PSHUFD, ins->dreg, ins->dreg, 0);
			break;
		case OP_EXPAND_I2:
			x86_sse_alu_pd_reg_reg_imm (code, X86_SSE_PINSRW, ins->dreg, ins->sreg1, 0);
			x86_sse_alu_pd_reg_reg_imm (code, X86_SSE_PINSRW, ins->dreg, ins->sreg1, 1);
			x86_sse_shift_reg_imm (code, X86_SSE_PSHUFD, ins->dreg, ins->dreg, 0);
			break;
		case OP_EXPAND_I4:
			x86_movd_xreg_reg (code, ins->dreg, ins->sreg1);
			x86_sse_shift_reg_imm (code, X86_SSE_PSHUFD, ins->dreg, ins->dreg, 0);
			break;
		case OP_EXPAND_R4:
			x86_fst_membase (code, ins->backend.spill_var->inst_basereg, ins->backend.spill_var->inst_offset, FALSE, TRUE);
			x86_movd_xreg_membase (code, ins->dreg, ins->backend.spill_var->inst_basereg, ins->backend.spill_var->inst_offset);
			x86_sse_shift_reg_imm (code, X86_SSE_PSHUFD, ins->dreg, ins->dreg, 0);
			break;
		case OP_EXPAND_R8:
			x86_fst_membase (code, ins->backend.spill_var->inst_basereg, ins->backend.spill_var->inst_offset, TRUE, TRUE);
			x86_movsd_reg_membase (code, ins->dreg, ins->backend.spill_var->inst_basereg, ins->backend.spill_var->inst_offset);
			x86_sse_shift_reg_imm (code, X86_SSE_PSHUFD, ins->dreg, ins->dreg, 0x44);
			break;
#endif
		default:
			g_warning ("unknown opcode %s\n", mono_inst_name (ins->opcode));
			g_assert_not_reached ();
		}

		if (G_UNLIKELY ((code - cfg->native_code - offset) > max_len)) {
			g_warning ("wrong maximal instruction length of instruction %s (expected %d, got %d)",
				   mono_inst_name (ins->opcode), max_len, code - cfg->native_code - offset);
			g_assert_not_reached ();
		}
	       
		cpos += max_len;
	}

	cfg->code_len = code - cfg->native_code;
}

void
mono_arch_register_lowlevel_calls (void)
{
}

void
mono_arch_patch_code (MonoMethod *method, MonoDomain *domain, guint8 *code, MonoJumpInfo *ji, gboolean run_cctors)
{
	MonoJumpInfo *patch_info;
	gboolean compile_aot = !run_cctors;

	for (patch_info = ji; patch_info; patch_info = patch_info->next) {
		unsigned char *ip = patch_info->ip.i + code;
		const unsigned char *target;

		target = mono_resolve_patch_target (method, domain, code, patch_info, run_cctors);

		if (compile_aot) {
			switch (patch_info->type) {
			case MONO_PATCH_INFO_BB:
			case MONO_PATCH_INFO_LABEL:
				break;
			default:
				/* No need to patch these */
				continue;
			}
		}

		switch (patch_info->type) {
		case MONO_PATCH_INFO_IP:
			*((gconstpointer *)(ip)) = target;
			break;
		case MONO_PATCH_INFO_CLASS_INIT: {
			guint8 *code = ip;
			/* Might already been changed to a nop */
			x86_call_code (code, 0);
			x86_patch (ip, target);
			break;
		}
		case MONO_PATCH_INFO_ABS:
		case MONO_PATCH_INFO_METHOD:
		case MONO_PATCH_INFO_METHOD_JUMP:
		case MONO_PATCH_INFO_INTERNAL_METHOD:
		case MONO_PATCH_INFO_BB:
		case MONO_PATCH_INFO_LABEL:
		case MONO_PATCH_INFO_RGCTX_FETCH:
		case MONO_PATCH_INFO_GENERIC_CLASS_INIT:
		case MONO_PATCH_INFO_MONITOR_ENTER:
		case MONO_PATCH_INFO_MONITOR_EXIT:
			x86_patch (ip, target);
			break;
		case MONO_PATCH_INFO_NONE:
			break;
		default: {
			guint32 offset = mono_arch_get_patch_offset (ip);
			*((gconstpointer *)(ip + offset)) = target;
			break;
		}
		}
	}
}

guint8 *
mono_arch_emit_prolog (MonoCompile *cfg)
{
	MonoMethod *method = cfg->method;
	MonoBasicBlock *bb;
	MonoMethodSignature *sig;
	MonoInst *inst;
	int alloc_size, pos, max_offset, i;
	guint8 *code;

	cfg->code_size =  MAX (mono_method_get_header (method)->code_size * 4, 10240);

	if (cfg->prof_options & MONO_PROFILE_ENTER_LEAVE)
		cfg->code_size += 512;

	code = cfg->native_code = g_malloc (cfg->code_size);

	x86_push_reg (code, X86_EBP);
	x86_mov_reg_reg (code, X86_EBP, X86_ESP, 4);

	alloc_size = cfg->stack_offset;
	pos = 0;

	if (method->wrapper_type == MONO_WRAPPER_NATIVE_TO_MANAGED) {
		/* Might need to attach the thread to the JIT  or change the domain for the callback */
		if (appdomain_tls_offset != -1 && lmf_tls_offset != -1) {
			guint8 *buf, *no_domain_branch;

			code = mono_x86_emit_tls_get (code, X86_EAX, appdomain_tls_offset);
			x86_alu_reg_imm (code, X86_CMP, X86_EAX, GPOINTER_TO_UINT (cfg->domain));
			no_domain_branch = code;
			x86_branch8 (code, X86_CC_NE, 0, 0);
			code = mono_x86_emit_tls_get ( code, X86_EAX, lmf_tls_offset);
			x86_test_reg_reg (code, X86_EAX, X86_EAX);
			buf = code;
			x86_branch8 (code, X86_CC_NE, 0, 0);
			x86_patch (no_domain_branch, code);
			x86_push_imm (code, cfg->domain);
			code = emit_call (cfg, code, MONO_PATCH_INFO_INTERNAL_METHOD, (gpointer)"mono_jit_thread_attach");
			x86_alu_reg_imm (code, X86_ADD, X86_ESP, 4);
			x86_patch (buf, code);
#ifdef PLATFORM_WIN32
			/* The TLS key actually contains a pointer to the MonoJitTlsData structure */
			/* FIXME: Add a separate key for LMF to avoid this */
			x86_alu_reg_imm (code, X86_ADD, X86_EAX, G_STRUCT_OFFSET (MonoJitTlsData, lmf));
#endif
		}
		else {
			g_assert (!cfg->compile_aot);
			x86_push_imm (code, cfg->domain);
			code = emit_call (cfg, code, MONO_PATCH_INFO_INTERNAL_METHOD, (gpointer)"mono_jit_thread_attach");
			x86_alu_reg_imm (code, X86_ADD, X86_ESP, 4);
		}
	}

	if (method->save_lmf) {
		pos += sizeof (MonoLMF);

		/* save the current IP */
		mono_add_patch_info (cfg, code + 1 - cfg->native_code, MONO_PATCH_INFO_IP, NULL);
		x86_push_imm_template (code);

		/* save all caller saved regs */
		x86_push_reg (code, X86_EBP);
		x86_push_reg (code, X86_ESI);
		x86_push_reg (code, X86_EDI);
		x86_push_reg (code, X86_EBX);

		if ((lmf_tls_offset != -1) && !is_win32 && !optimize_for_xen) {
			/*
			 * Optimized version which uses the mono_lmf TLS variable instead of indirection
			 * through the mono_lmf_addr TLS variable.
			 */
			/* %eax = previous_lmf */
			x86_prefix (code, X86_GS_PREFIX);
			x86_mov_reg_mem (code, X86_EAX, lmf_tls_offset, 4);
			/* skip esp + method_info + lmf */
			x86_alu_reg_imm (code, X86_SUB, X86_ESP, 12);
			/* push previous_lmf */
			x86_push_reg (code, X86_EAX);
			/* new lmf = ESP */
			x86_prefix (code, X86_GS_PREFIX);
			x86_mov_mem_reg (code, lmf_tls_offset, X86_ESP, 4);
		} else {
			/* get the address of lmf for the current thread */
			/* 
			 * This is performance critical so we try to use some tricks to make
			 * it fast.
			 */									   

			if (lmf_addr_tls_offset != -1) {
				/* Load lmf quicky using the GS register */
				code = mono_x86_emit_tls_get (code, X86_EAX, lmf_addr_tls_offset);
#ifdef PLATFORM_WIN32
				/* The TLS key actually contains a pointer to the MonoJitTlsData structure */
				/* FIXME: Add a separate key for LMF to avoid this */
				x86_alu_reg_imm (code, X86_ADD, X86_EAX, G_STRUCT_OFFSET (MonoJitTlsData, lmf));
#endif
			} else {
				code = emit_call (cfg, code, MONO_PATCH_INFO_INTERNAL_METHOD, (gpointer)"mono_get_lmf_addr");
			}

			/* Skip esp + method info */
			x86_alu_reg_imm (code, X86_SUB, X86_ESP, 8);

			/* push lmf */
			x86_push_reg (code, X86_EAX); 
			/* push *lfm (previous_lmf) */
			x86_push_membase (code, X86_EAX, 0);
			/* *(lmf) = ESP */
			x86_mov_membase_reg (code, X86_EAX, 0, X86_ESP, 4);
		}
	} else {

		if (cfg->used_int_regs & (1 << X86_EBX)) {
			x86_push_reg (code, X86_EBX);
			pos += 4;
		}

		if (cfg->used_int_regs & (1 << X86_EDI)) {
			x86_push_reg (code, X86_EDI);
			pos += 4;
		}

		if (cfg->used_int_regs & (1 << X86_ESI)) {
			x86_push_reg (code, X86_ESI);
			pos += 4;
		}
	}

	alloc_size -= pos;

	/* the original alloc_size is already aligned: there is %ebp and retip pushed, so realign */
	if (mono_do_x86_stack_align) {
		int tot = alloc_size + pos + 4 + 4; /* ret ip + ebp */
		tot &= MONO_ARCH_FRAME_ALIGNMENT - 1;
		alloc_size += MONO_ARCH_FRAME_ALIGNMENT - tot;
	}

	if (alloc_size) {
		/* See mono_emit_stack_alloc */
#if defined(PLATFORM_WIN32) || defined(MONO_ARCH_SIGSEGV_ON_ALTSTACK)
		guint32 remaining_size = alloc_size;
		while (remaining_size >= 0x1000) {
			x86_alu_reg_imm (code, X86_SUB, X86_ESP, 0x1000);
			x86_test_membase_reg (code, X86_ESP, 0, X86_ESP);
			remaining_size -= 0x1000;
		}
		if (remaining_size)
			x86_alu_reg_imm (code, X86_SUB, X86_ESP, remaining_size);
#else
		x86_alu_reg_imm (code, X86_SUB, X86_ESP, alloc_size);
#endif
	}

	if (cfg->method->wrapper_type == MONO_WRAPPER_NATIVE_TO_MANAGED ||
			cfg->method->wrapper_type == MONO_WRAPPER_RUNTIME_INVOKE) {
		x86_alu_reg_imm (code, X86_AND, X86_ESP, -MONO_ARCH_FRAME_ALIGNMENT);
	}

#if DEBUG_STACK_ALIGNMENT
	/* check the stack is aligned */
	if (method->wrapper_type == MONO_WRAPPER_NONE) {
		x86_mov_reg_reg (code, X86_ECX, X86_ESP, 4);
		x86_alu_reg_imm (code, X86_AND, X86_ECX, MONO_ARCH_FRAME_ALIGNMENT - 1);
		x86_alu_reg_imm (code, X86_CMP, X86_ECX, 0);
		x86_branch_disp (code, X86_CC_EQ, 3, FALSE);
		x86_breakpoint (code);
	}
#endif

        /* compute max_offset in order to use short forward jumps */
	max_offset = 0;
	if (cfg->opt & MONO_OPT_BRANCH) {
		for (bb = cfg->bb_entry; bb; bb = bb->next_bb) {
			MonoInst *ins;
			bb->max_offset = max_offset;

			if (cfg->prof_options & MONO_PROFILE_COVERAGE)
				max_offset += 6;
			/* max alignment for loops */
			if ((cfg->opt & MONO_OPT_LOOP) && bb_is_loop_start (bb))
				max_offset += LOOP_ALIGNMENT;

			MONO_BB_FOR_EACH_INS (bb, ins) {
				if (ins->opcode == OP_LABEL)
					ins->inst_c1 = max_offset;
				
				max_offset += ((guint8 *)ins_get_spec (ins->opcode))[MONO_INST_LEN];
			}
		}
	}

	/* store runtime generic context */
	if (cfg->rgctx_var) {
		g_assert (cfg->rgctx_var->opcode == OP_REGOFFSET && cfg->rgctx_var->inst_basereg == X86_EBP);

		x86_mov_membase_reg (code, X86_EBP, cfg->rgctx_var->inst_offset, MONO_ARCH_RGCTX_REG, 4);
	}

	if (mono_jit_trace_calls != NULL && mono_trace_eval (method))
		code = mono_arch_instrument_prolog (cfg, mono_trace_enter_method, code, TRUE);

	/* load arguments allocated to register from the stack */
	sig = mono_method_signature (method);
	pos = 0;

	for (i = 0; i < sig->param_count + sig->hasthis; ++i) {
		inst = cfg->args [pos];
		if (inst->opcode == OP_REGVAR) {
			x86_mov_reg_membase (code, inst->dreg, X86_EBP, inst->inst_offset, 4);
			if (cfg->verbose_level > 2)
				g_print ("Argument %d assigned to register %s\n", pos, mono_arch_regname (inst->dreg));
		}
		pos++;
	}

	cfg->code_len = code - cfg->native_code;

	g_assert (cfg->code_len < cfg->code_size);

	return code;
}

void
mono_arch_emit_epilog (MonoCompile *cfg)
{
	MonoMethod *method = cfg->method;
	MonoMethodSignature *sig = mono_method_signature (method);
	int quad, pos;
	guint32 stack_to_pop;
	guint8 *code;
	int max_epilog_size = 16;
	CallInfo *cinfo;
	
	if (cfg->method->save_lmf)
		max_epilog_size += 128;

	while (cfg->code_len + max_epilog_size > (cfg->code_size - 16)) {
		cfg->code_size *= 2;
		cfg->native_code = g_realloc (cfg->native_code, cfg->code_size);
		mono_jit_stats.code_reallocs++;
	}

	code = cfg->native_code + cfg->code_len;

	if (mono_jit_trace_calls != NULL && mono_trace_eval (method))
		code = mono_arch_instrument_epilog (cfg, mono_trace_leave_method, code, TRUE);

	/* the code restoring the registers must be kept in sync with OP_JMP */
	pos = 0;
	
	if (method->save_lmf) {
		gint32 prev_lmf_reg;
		gint32 lmf_offset = -sizeof (MonoLMF);

		/* check if we need to restore protection of the stack after a stack overflow */
		if (mono_get_jit_tls_offset () != -1) {
			guint8 *patch;
			code = mono_x86_emit_tls_get (code, X86_ECX, mono_get_jit_tls_offset ());
			/* we load the value in a separate instruction: this mechanism may be
			 * used later as a safer way to do thread interruption
			 */
			x86_mov_reg_membase (code, X86_ECX, X86_ECX, G_STRUCT_OFFSET (MonoJitTlsData, restore_stack_prot), 4);
			x86_alu_reg_imm (code, X86_CMP, X86_ECX, 0);
			patch = code;
		        x86_branch8 (code, X86_CC_Z, 0, FALSE);
			/* note that the call trampoline will preserve eax/edx */
			x86_call_reg (code, X86_ECX);
			x86_patch (patch, code);
		} else {
			/* FIXME: maybe save the jit tls in the prolog */
		}
		if ((lmf_tls_offset != -1) && !is_win32 && !optimize_for_xen) {
			/*
			 * Optimized version which uses the mono_lmf TLS variable instead of indirection
			 * through the mono_lmf_addr TLS variable.
			 */
			/* reg = previous_lmf */
			x86_mov_reg_membase (code, X86_ECX, X86_EBP, lmf_offset + G_STRUCT_OFFSET (MonoLMF, previous_lmf), 4);

			/* lmf = previous_lmf */
			x86_prefix (code, X86_GS_PREFIX);
			x86_mov_mem_reg (code, lmf_tls_offset, X86_ECX, 4);
		} else {
			/* Find a spare register */
			switch (mini_type_get_underlying_type (cfg->generic_sharing_context, sig->ret)->type) {
			case MONO_TYPE_I8:
			case MONO_TYPE_U8:
				prev_lmf_reg = X86_EDI;
				cfg->used_int_regs |= (1 << X86_EDI);
				break;
			default:
				prev_lmf_reg = X86_EDX;
				break;
			}

			/* reg = previous_lmf */
			x86_mov_reg_membase (code, prev_lmf_reg, X86_EBP, lmf_offset + G_STRUCT_OFFSET (MonoLMF, previous_lmf), 4);

			/* ecx = lmf */
			x86_mov_reg_membase (code, X86_ECX, X86_EBP, lmf_offset + G_STRUCT_OFFSET (MonoLMF, lmf_addr), 4);

			/* *(lmf) = previous_lmf */
			x86_mov_membase_reg (code, X86_ECX, 0, prev_lmf_reg, 4);
		}

		/* restore caller saved regs */
		if (cfg->used_int_regs & (1 << X86_EBX)) {
			x86_mov_reg_membase (code, X86_EBX, X86_EBP, lmf_offset + G_STRUCT_OFFSET (MonoLMF, ebx), 4);
		}

		if (cfg->used_int_regs & (1 << X86_EDI)) {
			x86_mov_reg_membase (code, X86_EDI, X86_EBP, lmf_offset + G_STRUCT_OFFSET (MonoLMF, edi), 4);
		}
		if (cfg->used_int_regs & (1 << X86_ESI)) {
			x86_mov_reg_membase (code, X86_ESI, X86_EBP, lmf_offset + G_STRUCT_OFFSET (MonoLMF, esi), 4);
		}

		/* EBP is restored by LEAVE */
	} else {
		if (cfg->used_int_regs & (1 << X86_EBX)) {
			pos -= 4;
		}
		if (cfg->used_int_regs & (1 << X86_EDI)) {
			pos -= 4;
		}
		if (cfg->used_int_regs & (1 << X86_ESI)) {
			pos -= 4;
		}

		if (pos)
			x86_lea_membase (code, X86_ESP, X86_EBP, pos);

		if (cfg->used_int_regs & (1 << X86_ESI)) {
			x86_pop_reg (code, X86_ESI);
		}
		if (cfg->used_int_regs & (1 << X86_EDI)) {
			x86_pop_reg (code, X86_EDI);
		}
		if (cfg->used_int_regs & (1 << X86_EBX)) {
			x86_pop_reg (code, X86_EBX);
		}
	}

	/* Load returned vtypes into registers if needed */
	cinfo = get_call_info (cfg->generic_sharing_context, cfg->mempool, sig, FALSE);
	if (cinfo->ret.storage == ArgValuetypeInReg) {
		for (quad = 0; quad < 2; quad ++) {
			switch (cinfo->ret.pair_storage [quad]) {
			case ArgInIReg:
				x86_mov_reg_membase (code, cinfo->ret.pair_regs [quad], cfg->ret->inst_basereg, cfg->ret->inst_offset + (quad * sizeof (gpointer)), 4);
				break;
			case ArgOnFloatFpStack:
				x86_fld_membase (code, cfg->ret->inst_basereg, cfg->ret->inst_offset + (quad * sizeof (gpointer)), FALSE);
				break;
			case ArgOnDoubleFpStack:
				x86_fld_membase (code, cfg->ret->inst_basereg, cfg->ret->inst_offset + (quad * sizeof (gpointer)), TRUE);
				break;
			case ArgNone:
				break;
			default:
				g_assert_not_reached ();
			}
		}
	}

	x86_leave (code);

	if (CALLCONV_IS_STDCALL (sig)) {
		MonoJitArgumentInfo *arg_info = alloca (sizeof (MonoJitArgumentInfo) * (sig->param_count + 1));

		stack_to_pop = mono_arch_get_argument_info (sig, sig->param_count, arg_info);
	} else if (MONO_TYPE_ISSTRUCT (mono_method_signature (cfg->method)->ret) && (cinfo->ret.storage == ArgOnStack))
		stack_to_pop = 4;
	else
		stack_to_pop = 0;

	if (stack_to_pop)
		x86_ret_imm (code, stack_to_pop);
	else
		x86_ret (code);

	cfg->code_len = code - cfg->native_code;

	g_assert (cfg->code_len < cfg->code_size);
}

void
mono_arch_emit_exceptions (MonoCompile *cfg)
{
	MonoJumpInfo *patch_info;
	int nthrows, i;
	guint8 *code;
	MonoClass *exc_classes [16];
	guint8 *exc_throw_start [16], *exc_throw_end [16];
	guint32 code_size;
	int exc_count = 0;

	/* Compute needed space */
	for (patch_info = cfg->patch_info; patch_info; patch_info = patch_info->next) {
		if (patch_info->type == MONO_PATCH_INFO_EXC)
			exc_count++;
	}

	/* 
	 * make sure we have enough space for exceptions
	 * 16 is the size of two push_imm instructions and a call
	 */
	if (cfg->compile_aot)
		code_size = exc_count * 32;
	else
		code_size = exc_count * 16;

	while (cfg->code_len + code_size > (cfg->code_size - 16)) {
		cfg->code_size *= 2;
		cfg->native_code = g_realloc (cfg->native_code, cfg->code_size);
		mono_jit_stats.code_reallocs++;
	}

	code = cfg->native_code + cfg->code_len;

	nthrows = 0;
	for (patch_info = cfg->patch_info; patch_info; patch_info = patch_info->next) {
		switch (patch_info->type) {
		case MONO_PATCH_INFO_EXC: {
			MonoClass *exc_class;
			guint8 *buf, *buf2;
			guint32 throw_ip;

			x86_patch (patch_info->ip.i + cfg->native_code, code);

			exc_class = mono_class_from_name (mono_defaults.corlib, "System", patch_info->data.name);
			g_assert (exc_class);
			throw_ip = patch_info->ip.i;

			/* Find a throw sequence for the same exception class */
			for (i = 0; i < nthrows; ++i)
				if (exc_classes [i] == exc_class)
					break;
			if (i < nthrows) {
				x86_push_imm (code, (exc_throw_end [i] - cfg->native_code) - throw_ip);
				x86_jump_code (code, exc_throw_start [i]);
				patch_info->type = MONO_PATCH_INFO_NONE;
			}
			else {
				guint32 size;

				/* Compute size of code following the push <OFFSET> */
				size = 5 + 5;

				if ((code - cfg->native_code) - throw_ip < 126 - size) {
					/* Use the shorter form */
					buf = buf2 = code;
					x86_push_imm (code, 0);
				}
				else {
					buf = code;
					x86_push_imm (code, 0xf0f0f0f0);
					buf2 = code;
				}

				if (nthrows < 16) {
					exc_classes [nthrows] = exc_class;
					exc_throw_start [nthrows] = code;
				}

				x86_push_imm (code, exc_class->type_token - MONO_TOKEN_TYPE_DEF);
				patch_info->data.name = "mono_arch_throw_corlib_exception";
				patch_info->type = MONO_PATCH_INFO_INTERNAL_METHOD;
				patch_info->ip.i = code - cfg->native_code;
				x86_call_code (code, 0);
				x86_push_imm (buf, (code - cfg->native_code) - throw_ip);
				while (buf < buf2)
					x86_nop (buf);

				if (nthrows < 16) {
					exc_throw_end [nthrows] = code;
					nthrows ++;
				}
			}
			break;
		}
		default:
			/* do nothing */
			break;
		}
	}

	cfg->code_len = code - cfg->native_code;

	g_assert (cfg->code_len < cfg->code_size);
}

void
mono_arch_flush_icache (guint8 *code, gint size)
{
	/* not needed */
}

void
mono_arch_flush_register_windows (void)
{
}

gboolean 
mono_arch_is_inst_imm (gint64 imm)
{
	return TRUE;
}

/*
 * Support for fast access to the thread-local lmf structure using the GS
 * segment register on NPTL + kernel 2.6.x.
 */

static gboolean tls_offset_inited = FALSE;

void
mono_arch_setup_jit_tls_data (MonoJitTlsData *tls)
{
	if (!tls_offset_inited) {
		if (!getenv ("MONO_NO_TLS")) {
#ifdef PLATFORM_WIN32
			/* 
			 * We need to init this multiple times, since when we are first called, the key might not
			 * be initialized yet.
			 */
			appdomain_tls_offset = mono_domain_get_tls_key ();
			lmf_tls_offset = mono_get_jit_tls_key ();
			thread_tls_offset = mono_thread_get_tls_key ();

			/* Only 64 tls entries can be accessed using inline code */
			if (appdomain_tls_offset >= 64)
				appdomain_tls_offset = -1;
			if (lmf_tls_offset >= 64)
				lmf_tls_offset = -1;
			if (thread_tls_offset >= 64)
				thread_tls_offset = -1;
#else
#if MONO_XEN_OPT
			optimize_for_xen = access ("/proc/xen", F_OK) == 0;
#endif
			tls_offset_inited = TRUE;
			appdomain_tls_offset = mono_domain_get_tls_offset ();
			lmf_tls_offset = mono_get_lmf_tls_offset ();
			lmf_addr_tls_offset = mono_get_lmf_addr_tls_offset ();
			thread_tls_offset = mono_thread_get_tls_offset ();
#endif
		}
	}		
}

void
mono_arch_free_jit_tls_data (MonoJitTlsData *tls)
{
}

#ifdef MONO_ARCH_HAVE_IMT

// Linear handler, the bsearch head compare is shorter
//[2 + 4] x86_alu_reg_imm (code, X86_CMP, ins->sreg1, ins->inst_imm);
//[1 + 1] x86_branch8(inst,cond,imm,is_signed)
//        x86_patch(ins,target)
//[1 + 5] x86_jump_mem(inst,mem)

#define CMP_SIZE 6
#define BR_SMALL_SIZE 2
#define BR_LARGE_SIZE 5
#define JUMP_IMM_SIZE 6
#define ENABLE_WRONG_METHOD_CHECK 0

static int
imt_branch_distance (MonoIMTCheckItem **imt_entries, int start, int target)
{
	int i, distance = 0;
	for (i = start; i < target; ++i)
		distance += imt_entries [i]->chunk_size;
	return distance;
}

/*
 * LOCKING: called with the domain lock held
 */
gpointer
mono_arch_build_imt_thunk (MonoVTable *vtable, MonoDomain *domain, MonoIMTCheckItem **imt_entries, int count,
	gpointer fail_tramp)
{
	int i;
	int size = 0;
	guint8 *code, *start;

	for (i = 0; i < count; ++i) {
		MonoIMTCheckItem *item = imt_entries [i];
		if (item->is_equals) {
			if (item->check_target_idx) {
				if (!item->compare_done)
					item->chunk_size += CMP_SIZE;
				item->chunk_size += BR_SMALL_SIZE + JUMP_IMM_SIZE;
			} else {
				if (fail_tramp) {
					item->chunk_size += CMP_SIZE + BR_SMALL_SIZE + JUMP_IMM_SIZE * 2;
				} else {
					item->chunk_size += JUMP_IMM_SIZE;
#if ENABLE_WRONG_METHOD_CHECK
					item->chunk_size += CMP_SIZE + BR_SMALL_SIZE + 1;
#endif
				}
			}
		} else {
			item->chunk_size += CMP_SIZE + BR_LARGE_SIZE;
			imt_entries [item->check_target_idx]->compare_done = TRUE;
		}
		size += item->chunk_size;
	}
	if (fail_tramp)
		code = mono_method_alloc_generic_virtual_thunk (domain, size);
	else
		code = mono_code_manager_reserve (domain->code_mp, size);
	start = code;
	for (i = 0; i < count; ++i) {
		MonoIMTCheckItem *item = imt_entries [i];
		item->code_target = code;
		if (item->is_equals) {
			if (item->check_target_idx) {
				if (!item->compare_done)
					x86_alu_reg_imm (code, X86_CMP, MONO_ARCH_IMT_REG, (guint32)item->key);
				item->jmp_code = code;
				x86_branch8 (code, X86_CC_NE, 0, FALSE);
				if (fail_tramp)
					x86_jump_code (code, item->value.target_code);
				else
					x86_jump_mem (code, & (vtable->vtable [item->value.vtable_slot]));
			} else {
				if (fail_tramp) {
					x86_alu_reg_imm (code, X86_CMP, MONO_ARCH_IMT_REG, (guint32)item->key);
					item->jmp_code = code;
					x86_branch8 (code, X86_CC_NE, 0, FALSE);
					x86_jump_code (code, item->value.target_code);
					x86_patch (item->jmp_code, code);
					x86_jump_code (code, fail_tramp);
					item->jmp_code = NULL;
				} else {
					/* enable the commented code to assert on wrong method */
#if ENABLE_WRONG_METHOD_CHECK
					x86_alu_reg_imm (code, X86_CMP, MONO_ARCH_IMT_REG, (guint32)item->key);
					item->jmp_code = code;
					x86_branch8 (code, X86_CC_NE, 0, FALSE);
#endif
					x86_jump_mem (code, & (vtable->vtable [item->value.vtable_slot]));
#if ENABLE_WRONG_METHOD_CHECK
					x86_patch (item->jmp_code, code);
					x86_breakpoint (code);
					item->jmp_code = NULL;
#endif
				}
			}
		} else {
			x86_alu_reg_imm (code, X86_CMP, MONO_ARCH_IMT_REG, (guint32)item->key);
			item->jmp_code = code;
			if (x86_is_imm8 (imt_branch_distance (imt_entries, i, item->check_target_idx)))
				x86_branch8 (code, X86_CC_GE, 0, FALSE);
			else
				x86_branch32 (code, X86_CC_GE, 0, FALSE);
		}
	}
	/* patch the branches to get to the target items */
	for (i = 0; i < count; ++i) {
		MonoIMTCheckItem *item = imt_entries [i];
		if (item->jmp_code) {
			if (item->check_target_idx) {
				x86_patch (item->jmp_code, imt_entries [item->check_target_idx]->code_target);
			}
		}
	}

	if (!fail_tramp)
		mono_stats.imt_thunks_size += code - start;
	g_assert (code - start <= size);
	return start;
}

MonoMethod*
mono_arch_find_imt_method (gpointer *regs, guint8 *code)
{
	return (MonoMethod*) regs [MONO_ARCH_IMT_REG];
}

MonoObject*
mono_arch_find_this_argument (gpointer *regs, MonoMethod *method, MonoGenericSharingContext *gsctx)
{
	MonoMethodSignature *sig = mono_method_signature (method);
	CallInfo *cinfo = get_call_info (gsctx, NULL, sig, FALSE);
	int this_argument_offset;
	MonoObject *this_argument;

	/* 
	 * this is the offset of the this arg from esp as saved at the start of 
	 * mono_arch_create_trampoline_code () in tramp-x86.c.
	 */
	this_argument_offset = 5;
	if (MONO_TYPE_ISSTRUCT (sig->ret) && (cinfo->ret.storage == ArgOnStack))
		this_argument_offset++;

	this_argument = * (MonoObject**) (((guint8*) regs [X86_ESP]) + this_argument_offset * sizeof (gpointer));

	g_free (cinfo);
	return this_argument;
}
#endif

MonoVTable*
mono_arch_find_static_call_vtable (gpointer *regs, guint8 *code)
{
	return (MonoVTable*) regs [MONO_ARCH_RGCTX_REG];
}

MonoInst*
mono_arch_emit_inst_for_method (MonoCompile *cfg, MonoMethod *cmethod, MonoMethodSignature *fsig, MonoInst **args)
{
	MonoInst *ins = NULL;
	int opcode = 0;

	if (cmethod->klass == mono_defaults.math_class) {
		if (strcmp (cmethod->name, "Sin") == 0) {
			opcode = OP_SIN;
		} else if (strcmp (cmethod->name, "Cos") == 0) {
			opcode = OP_COS;
		} else if (strcmp (cmethod->name, "Tan") == 0) {
			opcode = OP_TAN;
		} else if (strcmp (cmethod->name, "Atan") == 0) {
			opcode = OP_ATAN;
		} else if (strcmp (cmethod->name, "Sqrt") == 0) {
			opcode = OP_SQRT;
		} else if (strcmp (cmethod->name, "Abs") == 0 && fsig->params [0]->type == MONO_TYPE_R8) {
			opcode = OP_ABS;
		} else if (strcmp (cmethod->name, "Round") == 0 && fsig->param_count == 1 && fsig->params [0]->type == MONO_TYPE_R8) {
			opcode = OP_ROUND;
		}
		
		if (opcode) {
			MONO_INST_NEW (cfg, ins, opcode);
			ins->type = STACK_R8;
			ins->dreg = mono_alloc_freg (cfg);
			ins->sreg1 = args [0]->dreg;
			MONO_ADD_INS (cfg->cbb, ins);
		}

		if (cfg->opt & MONO_OPT_CMOV) {
			int opcode = 0;

			if (strcmp (cmethod->name, "Min") == 0) {
				if (fsig->params [0]->type == MONO_TYPE_I4)
					opcode = OP_IMIN;
			} else if (strcmp (cmethod->name, "Max") == 0) {
				if (fsig->params [0]->type == MONO_TYPE_I4)
					opcode = OP_IMAX;
			}		

			if (opcode) {
				MONO_INST_NEW (cfg, ins, opcode);
				ins->type = STACK_I4;
				ins->dreg = mono_alloc_ireg (cfg);
				ins->sreg1 = args [0]->dreg;
				ins->sreg2 = args [1]->dreg;
				MONO_ADD_INS (cfg->cbb, ins);
			}
		}

#if 0
		/* OP_FREM is not IEEE compatible */
		else if (strcmp (cmethod->name, "IEEERemainder") == 0) {
			MONO_INST_NEW (cfg, ins, OP_FREM);
			ins->inst_i0 = args [0];
			ins->inst_i1 = args [1];
		}
#endif
	}

	return ins;
}

gboolean
mono_arch_print_tree (MonoInst *tree, int arity)
{
	return 0;
}

MonoInst* mono_arch_get_domain_intrinsic (MonoCompile* cfg)
{
	MonoInst* ins;

	return NULL;

	if (appdomain_tls_offset == -1)
		return NULL;

	MONO_INST_NEW (cfg, ins, OP_TLS_GET);
	ins->inst_offset = appdomain_tls_offset;
	return ins;
}

MonoInst* mono_arch_get_thread_intrinsic (MonoCompile* cfg)
{
	MonoInst* ins;

	if (thread_tls_offset == -1)
		return NULL;

	MONO_INST_NEW (cfg, ins, OP_TLS_GET);
	ins->inst_offset = thread_tls_offset;
	return ins;
}

guint32
mono_arch_get_patch_offset (guint8 *code)
{
	if ((code [0] == 0x8b) && (x86_modrm_mod (code [1]) == 0x2))
		return 2;
	else if ((code [0] == 0xba))
		return 1;
	else if ((code [0] == 0x68))
		/* push IMM */
		return 1;
	else if ((code [0] == 0xff) && (x86_modrm_reg (code [1]) == 0x6))
		/* push <OFFSET>(<REG>) */
		return 2;
	else if ((code [0] == 0xff) && (x86_modrm_reg (code [1]) == 0x2))
		/* call *<OFFSET>(<REG>) */
		return 2;
	else if ((code [0] == 0xdd) || (code [0] == 0xd9))
		/* fldl <ADDR> */
		return 2;
	else if ((code [0] == 0x58) && (code [1] == 0x05))
		/* pop %eax; add <OFFSET>, %eax */
		return 2;
	else if ((code [0] >= 0x58) && (code [0] <= 0x58 + X86_NREG) && (code [1] == 0x81))
		/* pop <REG>; add <OFFSET>, <REG> */
		return 3;
	else if ((code [0] >= 0xb8) && (code [0] < 0xb8 + 8))
		/* mov <REG>, imm */
		return 1;
	else {
		g_assert_not_reached ();
		return -1;
	}
}

/**
 * mono_breakpoint_clean_code:
 *
 * Copy @size bytes from @code - @offset to the buffer @buf. If the debugger inserted software
 * breakpoints in the original code, they are removed in the copy.
 *
 * Returns TRUE if no sw breakpoint was present.
 */
gboolean
mono_breakpoint_clean_code (guint8 *method_start, guint8 *code, int offset, guint8 *buf, int size)
{
	int i;
	gboolean can_write = TRUE;
	/*
	 * If method_start is non-NULL we need to perform bound checks, since we access memory
	 * at code - offset we could go before the start of the method and end up in a different
	 * page of memory that is not mapped or read incorrect data anyway. We zero-fill the bytes
	 * instead.
	 */
	if (!method_start || code - offset >= method_start) {
		memcpy (buf, code - offset, size);
	} else {
		int diff = code - method_start;
		memset (buf, 0, size);
		memcpy (buf + offset - diff, method_start, diff + size - offset);
	}
	code -= offset;
	for (i = 0; i < MONO_BREAKPOINT_ARRAY_SIZE; ++i) {
		int idx = mono_breakpoint_info_index [i];
		guint8 *ptr;
		if (idx < 1)
			continue;
		ptr = mono_breakpoint_info [idx].address;
		if (ptr >= code && ptr < code + size) {
			guint8 saved_byte = mono_breakpoint_info [idx].saved_byte;
			can_write = FALSE;
			/*g_print ("patching %p with 0x%02x (was: 0x%02x)\n", ptr, saved_byte, buf [ptr - code]);*/
			buf [ptr - code] = saved_byte;
		}
	}
	return can_write;
}

gpointer
mono_arch_get_vcall_slot (guint8 *code, gpointer *regs, int *displacement)
{
	guint8 buf [8];
	guint8 reg = 0;
	gint32 disp = 0;

	mono_breakpoint_clean_code (NULL, code, 8, buf, sizeof (buf));
	code = buf + 8;

	*displacement = 0;

	code -= 6;

	/* 
	 * A given byte sequence can match more than case here, so we have to be
	 * really careful about the ordering of the cases. Longer sequences
	 * come first.
	 * There are two types of calls:
	 * - direct calls: 0xff address_byte 8/32 bits displacement
	 * - indirect calls: nop nop nop <call>
	 * The nops make sure we don't confuse the instruction preceeding an indirect
	 * call with a direct call.
	 */
	if ((code [1] != 0xe8) && (code [3] == 0xff) && ((code [4] & 0x18) == 0x10) && ((code [4] >> 6) == 1)) {
		reg = code [4] & 0x07;
		disp = (signed char)code [5];
	} else if ((code [0] == 0xff) && ((code [1] & 0x18) == 0x10) && ((code [1] >> 6) == 2)) {
		reg = code [1] & 0x07;
		disp = *((gint32*)(code + 2));
	} else if ((code [1] == 0xe8)) {
			return NULL;
	} else if ((code [4] == 0xff) && (((code [5] >> 6) & 0x3) == 0) && (((code [5] >> 3) & 0x7) == 2)) {
		/*
		 * This is a interface call
		 * 8b 40 30   mov    0x30(%eax),%eax
		 * ff 10      call   *(%eax)
		 */
		disp = 0;
		reg = code [5] & 0x07;
	}
	else
		return NULL;

	*displacement = disp;
	return regs [reg];
}

gpointer*
mono_arch_get_vcall_slot_addr (guint8 *code, gpointer *regs)
{
	gpointer vt;
	int displacement;
	vt = mono_arch_get_vcall_slot (code, regs, &displacement);
	if (!vt)
		return NULL;
	return (gpointer*)((char*)vt + displacement);
}

gpointer
mono_arch_get_this_arg_from_call (MonoGenericSharingContext *gsctx, MonoMethodSignature *sig,
		gssize *regs, guint8 *code)
{
	guint32 esp = regs [X86_ESP];
	CallInfo *cinfo = NULL;
	gpointer res;
	int offset;

	/* 
	 * Avoid expensive calls to get_generic_context_from_code () + get_call_info 
	 * if possible.
	 */
	if (MONO_TYPE_ISSTRUCT (sig->ret)) {
		if (!gsctx && code)
			gsctx = mono_get_generic_context_from_code (code);
		cinfo = get_call_info (gsctx, NULL, sig, FALSE);

		offset = cinfo->args [0].offset;
	} else {
		offset = 0;
	}

	/*
	 * The stack looks like:
	 * <other args>
	 * <this=delegate>
	 * <possible vtype return address>
	 * <return addr>
	 * <4 pointers pushed by mono_arch_create_trampoline_code ()>
	 */
	res = (((MonoObject**)esp) [5 + (offset / 4)]);
	if (cinfo)
		g_free (cinfo);
	return res;
}

#define MAX_ARCH_DELEGATE_PARAMS 10

gpointer
mono_arch_get_delegate_invoke_impl (MonoMethodSignature *sig, gboolean has_target)
{
	guint8 *code, *start;

	if (sig->param_count > MAX_ARCH_DELEGATE_PARAMS)
		return NULL;

	/* FIXME: Support more cases */
	if (MONO_TYPE_ISSTRUCT (sig->ret))
		return NULL;

	/*
	 * The stack contains:
	 * <delegate>
	 * <return addr>
	 */

	if (has_target) {
		static guint8* cached = NULL;
		if (cached)
			return cached;
		
		start = code = mono_global_codeman_reserve (64);

		/* Replace the this argument with the target */
		x86_mov_reg_membase (code, X86_EAX, X86_ESP, 4, 4);
		x86_mov_reg_membase (code, X86_ECX, X86_EAX, G_STRUCT_OFFSET (MonoDelegate, target), 4);
		x86_mov_membase_reg (code, X86_ESP, 4, X86_ECX, 4);
		x86_jump_membase (code, X86_EAX, G_STRUCT_OFFSET (MonoDelegate, method_ptr));

		g_assert ((code - start) < 64);

		mono_debug_add_delegate_trampoline (start, code - start);

		mono_memory_barrier ();

		cached = start;
	} else {
		static guint8* cache [MAX_ARCH_DELEGATE_PARAMS + 1] = {NULL};
		int i = 0;
		/* 8 for mov_reg and jump, plus 8 for each parameter */
		int code_reserve = 8 + (sig->param_count * 8);

		for (i = 0; i < sig->param_count; ++i)
			if (!mono_is_regsize_var (sig->params [i]))
				return NULL;

		code = cache [sig->param_count];
		if (code)
			return code;

		/*
		 * The stack contains:
		 * <args in reverse order>
		 * <delegate>
		 * <return addr>
		 *
		 * and we need:
		 * <args in reverse order>
		 * <return addr>
		 * 
		 * without unbalancing the stack.
		 * So move each arg up a spot in the stack (overwriting un-needed 'this' arg)
		 * and leaving original spot of first arg as placeholder in stack so
		 * when callee pops stack everything works.
		 */

		start = code = mono_global_codeman_reserve (code_reserve);

		/* store delegate for access to method_ptr */
		x86_mov_reg_membase (code, X86_ECX, X86_ESP, 4, 4);

		/* move args up */
		for (i = 0; i < sig->param_count; ++i) {
			x86_mov_reg_membase (code, X86_EAX, X86_ESP, (i+2)*4, 4);
			x86_mov_membase_reg (code, X86_ESP, (i+1)*4, X86_EAX, 4);
		}

		x86_jump_membase (code, X86_ECX, G_STRUCT_OFFSET (MonoDelegate, method_ptr));

		g_assert ((code - start) < code_reserve);

		mono_debug_add_delegate_trampoline (start, code - start);

		mono_memory_barrier ();

		cache [sig->param_count] = start;
	}

	return start;
}

gpointer
mono_arch_context_get_int_reg (MonoContext *ctx, int reg)
{
	switch (reg) {
	case X86_EAX: return (gpointer)ctx->eax;
	case X86_EBX: return (gpointer)ctx->ebx;
	case X86_ECX: return (gpointer)ctx->ecx;
	case X86_EDX: return (gpointer)ctx->edx;
	case X86_ESP: return (gpointer)ctx->esp;
	case X86_EBP: return (gpointer)ctx->ebp;
	case X86_ESI: return (gpointer)ctx->esi;
	case X86_EDI: return (gpointer)ctx->edi;
	default: g_assert_not_reached ();
	}
}

#ifdef MONO_ARCH_SIMD_INTRINSICS

static MonoInst*
get_float_to_x_spill_area (MonoCompile *cfg)
{
	if (!cfg->fconv_to_r8_x_var) {
		cfg->fconv_to_r8_x_var = mono_compile_create_var (cfg, &mono_defaults.double_class->byval_arg, OP_LOCAL);
		cfg->fconv_to_r8_x_var->flags |= MONO_INST_VOLATILE; /*FIXME, use the don't regalloc flag*/
	}	
	return cfg->fconv_to_r8_x_var;
}

/*
 * Convert all fconv opts that MONO_OPT_SSE2 would get wrong. 
 */
void
mono_arch_decompose_opts (MonoCompile *cfg, MonoInst *ins)
{
	MonoInst *fconv;
	int dreg, src_opcode;

	if (!(cfg->opt & MONO_OPT_SSE2) || !(cfg->opt & MONO_OPT_SIMD))
		return;

	switch (src_opcode = ins->opcode) {
	case OP_FCONV_TO_I1:
	case OP_FCONV_TO_U1:
	case OP_FCONV_TO_I2:
	case OP_FCONV_TO_U2:
	case OP_FCONV_TO_I4:
	case OP_FCONV_TO_I:
		break;
	default:
		return;
	}

	/* dreg is the IREG and sreg1 is the FREG */
	MONO_INST_NEW (cfg, fconv, OP_FCONV_TO_R8_X);
	fconv->klass = NULL; /*FIXME, what can I use here as the Mono.Simd lib might not be loaded yet*/
	fconv->sreg1 = ins->sreg1;
	fconv->dreg = mono_alloc_ireg (cfg);
	fconv->type = STACK_VTYPE;
	fconv->backend.spill_var = get_float_to_x_spill_area (cfg);

	mono_bblock_insert_before_ins (cfg->cbb, ins, fconv);

	dreg = ins->dreg;
	NULLIFY_INS (ins);
	ins->opcode = OP_XCONV_R8_TO_I4;

	ins->klass = mono_defaults.int32_class;
	ins->sreg1 = fconv->dreg;
	ins->dreg = dreg;
	ins->type = STACK_I4;
	ins->backend.source_opcode = src_opcode;
}

void
mono_arch_decompose_long_opts (MonoCompile *cfg, MonoInst *long_ins)
{
	MonoInst *ins;
	int vreg;
	if (!(cfg->opt & MONO_OPT_SIMD))
		return;
	
	/*TODO move this to simd-intrinsic.c once we support sse 4.1 dword extractors since we need the runtime caps info */ 
	switch (long_ins->opcode) {
	case OP_EXTRACT_I8:
		vreg = long_ins->sreg1;
	
		if (long_ins->inst_c0) {
			MONO_INST_NEW (cfg, ins, OP_PSHUFLED);
			ins->klass = long_ins->klass;
			ins->sreg1 = long_ins->sreg1;
			ins->inst_c0 = 2;
			ins->type = STACK_VTYPE;
			ins->dreg = vreg = alloc_ireg (cfg);
			MONO_ADD_INS (cfg->cbb, ins);
		}
	
		MONO_INST_NEW (cfg, ins, OP_EXTRACT_I4);
		ins->klass = mono_defaults.int32_class;
		ins->sreg1 = vreg;
		ins->type = STACK_I4;
		ins->dreg = long_ins->dreg + 1;
		MONO_ADD_INS (cfg->cbb, ins);
	
		MONO_INST_NEW (cfg, ins, OP_PSHUFLED);
		ins->klass = long_ins->klass;
		ins->sreg1 = long_ins->sreg1;
		ins->inst_c0 = long_ins->inst_c0 ? 3 : 1;
		ins->type = STACK_VTYPE;
		ins->dreg = vreg = alloc_ireg (cfg);
		MONO_ADD_INS (cfg->cbb, ins);
	
		MONO_INST_NEW (cfg, ins, OP_EXTRACT_I4);
		ins->klass = mono_defaults.int32_class;
		ins->sreg1 = vreg;
		ins->type = STACK_I4;
		ins->dreg = long_ins->dreg + 2;
		MONO_ADD_INS (cfg->cbb, ins);
	
		long_ins->opcode = OP_NOP;
		break;
	case OP_INSERTX_I8_SLOW:
		MONO_INST_NEW (cfg, ins, OP_INSERTX_I4_SLOW);
		ins->dreg = long_ins->dreg;
		ins->sreg1 = long_ins->dreg;
		ins->sreg2 = long_ins->sreg2 + 1;
		ins->inst_c0 = long_ins->inst_c0 * 2;
		MONO_ADD_INS (cfg->cbb, ins);

		MONO_INST_NEW (cfg, ins, OP_INSERTX_I4_SLOW);
		ins->dreg = long_ins->dreg;
		ins->sreg1 = long_ins->dreg;
		ins->sreg2 = long_ins->sreg2 + 2;
		ins->inst_c0 = long_ins->inst_c0 * 2 + 1;
		MONO_ADD_INS (cfg->cbb, ins);

		long_ins->opcode = OP_NOP;
		break;
	case OP_EXPAND_I8:
		MONO_INST_NEW (cfg, ins, OP_ICONV_TO_X);
		ins->dreg = long_ins->dreg;
		ins->sreg1 = long_ins->sreg1 + 1;
		ins->klass = long_ins->klass;
		ins->type = STACK_VTYPE;
		MONO_ADD_INS (cfg->cbb, ins);

		MONO_INST_NEW (cfg, ins, OP_INSERTX_I4_SLOW);
		ins->dreg = long_ins->dreg;
		ins->sreg1 = long_ins->dreg;
		ins->sreg2 = long_ins->sreg1 + 2;
		ins->inst_c0 = 1;
		ins->klass = long_ins->klass;
		ins->type = STACK_VTYPE;
		MONO_ADD_INS (cfg->cbb, ins);

		MONO_INST_NEW (cfg, ins, OP_PSHUFLED);
		ins->dreg = long_ins->dreg;
		ins->sreg1 = long_ins->dreg;;
		ins->inst_c0 = 0x44; /*Magic number for swizzling (X,Y,X,Y)*/
		ins->klass = long_ins->klass;
		ins->type = STACK_VTYPE;
		MONO_ADD_INS (cfg->cbb, ins);

		long_ins->opcode = OP_NOP;
		break;
	}
}
#endif

