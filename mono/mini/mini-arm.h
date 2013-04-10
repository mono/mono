/*
 * Copyright 2011 Xamarin Inc
 */

#ifndef __MONO_MINI_ARM_H__
#define __MONO_MINI_ARM_H__

#include <mono/arch/arm/arm-codegen.h>
#include <mono/utils/mono-context.h>
#include <glib.h>

#ifdef __native_client_codegen__
#define kNaClAlignmentARM 16
#define kNaClAlignmentMaskARM (kNaClAlignmentARM - 1)
#define kNaClLengthOfCallImm 12

#define ARM_NACL_MASK_REG(code, reg) ARM_BIC_REG_IMM_COND (code, reg, reg, 0xc0, 8, ARMCOND_AL)
#define ARM_NACL_MASK_CODE(code, reg) ARM_BIC_REG_IMM_COND (code, reg, reg, 0xfc, 4, ARMCOND_AL)

#define ARM_NACL_MASK_REG_ALIGN(code, reg) do {   \
      code = mono_arm_nacl_ensure_bundle(code, 4); \
      ARM_NACL_MASK_REG (code, reg); \
   } while (0);

#define ARM_NACL_MASK_CODE_ALIGN(code, reg) do {   \
      code = mono_arm_nacl_ensure_bundle(code, 4); \
      ARM_NACL_MASK_CODE (code, reg); \
   } while (0);

guint8* mono_arm_nacl_ensure_bundle (guint8 *code, int space);
guint8* mono_arm_nacl_ensure_at_position (guint8 *code, int position);
#else
#define NACL_BUNDLE_ALIGN_UP(p) (p)
#endif

#if defined(ARM_FPU_NONE) || (defined(__ARM_EABI__) && !defined(ARM_FPU_VFP) && !defined(ARM_FPU_VFP_HARD))
#define MONO_ARCH_SOFT_FLOAT 1
#endif

#ifdef ARM_FPU_VFP_HARD
#error "hardfp-abi not yet supported."
#endif

#if defined(__ARM_EABI__)
#if G_BYTE_ORDER == G_LITTLE_ENDIAN
#define ARM_ARCHITECTURE "armel"
#else
#define ARM_ARCHITECTURE "armeb"
#endif
#else
#define ARM_ARCHITECTURE "arm"
#endif

#if defined(ARM_FPU_FPA)
#define ARM_FP_MODEL "fpa"
#elif defined(ARM_FPU_VFP)
#define ARM_FP_MODEL "vfp"
#elif defined(ARM_FPU_NONE)
#define ARM_FP_MODEL "soft-float"
#elif defined(ARM_FPU_VFP_HARD)
#define ARM_FP_MODEL "vfp(hardfp-abi)"
#else
#error "At least one of ARM_FPU_NONE, ARM_FPU_FPA, ARM_FPU_VFP or ARM_FPU_VFP_HARD must be defined."
#endif

#define MONO_ARCH_ARCHITECTURE ARM_ARCHITECTURE "," ARM_FP_MODEL

#define MONO_ARCH_CPU_SPEC arm_cpu_desc

#if G_BYTE_ORDER == G_LITTLE_ENDIAN
#define ARM_LSW_REG ARMREG_R0
#define ARM_MSW_REG ARMREG_R1
#else
#define ARM_LSW_REG ARMREG_R1
#define ARM_MSW_REG ARMREG_R0
#endif

#define MONO_MAX_IREGS 16
#define MONO_MAX_FREGS 16

#ifdef __native_client__
/* NaCl cannot access R9. */
#define MONO_SAVED_GREGS 9 /* r4-r8, r10, r11, ip, lr */
#else
#define MONO_SAVED_GREGS 10 /* r4-r11, ip, lr */
#endif
#define MONO_SAVED_FREGS 8

#define MONO_ARM_NUM_SAVED_REGS 10

#ifdef __native_client_codegen__
/* NaCl cannot access R9, so use two masks. */
/* r4 - r8 */
#define MONO_ARM_REGSAVE_MASK1 (0x1f0)
/* r8, r10, r11, r12, lr=r14 */
#define MONO_ARM_REGSAVE_MASK2 ((1 << ARMREG_R8) | 0x5c00)
/* r0 - r8 */
#define MONO_ARM_LMF_REGSAVE_MASK1 (MONO_ARM_REGSAVE_MASK1 | 0xf)
#define MONO_ARM_LMF_REGSAVE_MASK2  MONO_ARM_REGSAVE_MASK2
#else
/* r4 - r12, lr */
#define MONO_ARM_REGSAVE_MASK 0x5ff0
#define MONO_ARM_LMF_REGSAVE_MASK (MONO_ARM_REGSAVE_MASK | 0xf)
#endif

#define MONO_ARM_FIRST_SAVED_REG ARMREG_R4
#define MONO_ARM_LMF_REGSAVE_SIZE 14
#define MONO_ARM_LMF_REGSAVE_LR_INDEX (MONO_ARM_LMF_REGSAVE_SIZE-1)

/* Parameters used by the register allocator */

#define MONO_ARCH_CALLEE_REGS ((1<<ARMREG_R0) | (1<<ARMREG_R1) | (1<<ARMREG_R2) | (1<<ARMREG_R3) | (1<<ARMREG_IP))
#define MONO_ARCH_CALLEE_SAVED_REGS ((1<<ARMREG_V1) | (1<<ARMREG_V2) | (1<<ARMREG_V3) | (1<<ARMREG_V4) | (1<<ARMREG_V5) | (1<<ARMREG_V6) | (1<<ARMREG_V7))

#if defined(ARM_FPU_VFP) || defined(ARM_FPU_VFP_HARD)
/* Every double precision vfp register, d0/d1 is reserved for a scratch reg */
#define MONO_ARCH_CALLEE_FREGS 0x55555550
#else
#define MONO_ARCH_CALLEE_FREGS 0xf
#endif
#define MONO_ARCH_CALLEE_SAVED_FREGS 0

#define MONO_ARCH_USE_FPSTACK FALSE
#define MONO_ARCH_FPSTACK_SIZE 0

#define MONO_ARCH_INST_SREG2_MASK(ins) (0)

#ifdef MONO_ARCH_SOFT_FLOAT
#define MONO_ARCH_INST_FIXED_REG(desc) (((desc) == 'l' || (desc == 'f') || (desc == 'g')) ? ARM_LSW_REG: (((desc) == 'a') ? ARMREG_R0 : -1))
#define MONO_ARCH_INST_IS_REGPAIR(desc) ((desc) == 'l' || (desc) == 'L' || (desc) == 'f' || (desc) == 'g')
#define MONO_ARCH_INST_IS_FLOAT(desc) (FALSE)
#else
#define MONO_ARCH_INST_FIXED_REG(desc) (((desc) == 'l')? ARM_LSW_REG: (((desc) == 'a') ? ARMREG_R0 : -1))
#define MONO_ARCH_INST_IS_REGPAIR(desc) (desc == 'l' || desc == 'L')
#define MONO_ARCH_INST_IS_FLOAT(desc) ((desc == 'f') || (desc == 'g'))
#endif
#define MONO_ARCH_INST_REGPAIR_REG2(desc,hreg1) (desc == 'l'  || (desc == 'f') || (desc == 'g')? ARM_MSW_REG : -1)

#define MONO_ARCH_FRAME_ALIGNMENT 8

/* fixme: align to 16byte instead of 32byte (we align to 32byte to get 
 * reproduceable results for benchmarks */
#define MONO_ARCH_CODE_ALIGNMENT 32


/* Return value marshalling for calls between gsharedvt and normal code */
typedef enum {
	GSHAREDVT_RET_NONE = 0,
	GSHAREDVT_RET_IREGS = 1,
	GSHAREDVT_RET_I1 = 5,
	GSHAREDVT_RET_U1 = 6,
	GSHAREDVT_RET_I2 = 7,
	GSHAREDVT_RET_U2 = 8
} GSharedVtRetMarshal;

typedef struct {
	/* Method address to call */
	gpointer addr;
	/* The trampoline reads this, so keep the size explicit */
	int ret_marshal;
	/* If ret_marshal != NONE, this is the reg of the vret arg, else -1 */
	int vret_arg_reg;
	/* The stack slot where the return value will be stored */
	int vret_slot;
	int stack_usage, map_count;
	/* If not -1, then make a virtual call using this vtable offset */
	int vcall_offset;
	/* If 1, make an indirect call to the address in the rgctx reg */
	int calli;
	/* Whenever this is a in or an out call */
	int gsharedvt_in;
	/* Maps stack slots/registers in the caller to the stack slots/registers in the callee */
	/* A negative value means a register, i.e. -1=r0, -2=r1 etc. */
	int map [MONO_ZERO_LEN_ARRAY];
} GSharedVtCallInfo;

void arm_patch (guchar *code, const guchar *target);
guint8* mono_arm_emit_bx_cond (guint8 *code, ARMReg dreg, int need_return, int cond);
guint8* mono_arm_emit_bx (guint8 *code, ARMReg dreg, int need_return);
guint8* mono_arm_emit_call_reg (guint8 *code, ARMReg dreg);
guint8* mono_arm_emit_load_imm (guint8 *code, ARMReg dreg, guint32 val);
gint8   mono_arm_ret_pc_offset (guint8 *code);

guint8* mono_arm_emit_ldr_imm12 (guint8* code, ARMReg dreg, ARMReg basereg, int offset);
guint8* mono_arm_emit_ldrh_imm8 (guint8* code, ARMReg dreg, ARMReg basereg, int offset);
guint8* mono_arm_emit_ldrsh_imm8 (guint8* code, ARMReg dreg, ARMReg basereg, int offset);
guint8* mono_arm_emit_ldrb_imm8(guint8* code, ARMReg dreg, ARMReg basereg, int offset);
guint8* mono_arm_emit_ldrsb_imm8 (guint8* code, ARMReg dreg, ARMReg basereg, int offset);
guint8* mono_arm_emit_flds_imm8 (guint8* code, int dreg, ARMReg basereg, int offset);
guint8* mono_arm_emit_fldd_imm8 (guint8* code, int dreg, ARMReg basereg, int offset);
guint8* mono_arm_emit_ldr_imm (guint8* code, ARMReg dreg, ARMReg basereg, int offset, ARMReg scratch_reg);
guint8* mono_arm_emit_ldrh_imm (guint8* code, ARMReg dreg, ARMReg basereg, int offset, ARMReg scratch_reg);
guint8* mono_arm_emit_ldsh_imm (guint8* code, ARMReg dreg, ARMReg basereg, int offset, ARMReg scratch_reg);
guint8* mono_arm_emit_ldrb_imm (guint8* code, ARMReg dreg, ARMReg basereg, int offset, ARMReg scratch_reg);
guint8* mono_arm_emit_ldrsb_imm (guint8* code, ARMReg dreg, ARMReg basereg, int offset, ARMReg scratch_reg);
guint8* mono_arm_emit_flds_imm (guint8* code, int dreg, ARMReg basereg, int offset, ARMReg scratch_reg);
guint8* mono_arm_emit_fldd_imm (guint8* code, int dreg, ARMReg basereg, int offset, ARMReg scratch_reg);

guint8* mono_arm_emit_ldr_reg (guint8* code, ARMReg dreg, ARMReg basereg, ARMReg offreg);
guint8* mono_arm_emit_ldrh_reg (guint8* code, ARMReg dreg, ARMReg basereg, ARMReg offreg);
guint8* mono_arm_emit_ldrsh_reg (guint8* code, ARMReg dreg, ARMReg basereg, ARMReg offreg);
guint8* mono_arm_emit_ldrb_reg (guint8* code, ARMReg dreg, ARMReg basereg, ARMReg offreg);
guint8* mono_arm_emit_ldrsb_reg (guint8* code, ARMReg dreg, ARMReg basereg, ARMReg offreg);

guint8* mono_arm_emit_str_imm12 (guint8* code, ARMReg dreg, ARMReg basereg, int offset);
guint8* mono_arm_emit_strh_imm8 (guint8* code, ARMReg dreg, ARMReg basereg, int offset);
guint8* mono_arm_emit_strb_imm8 (guint8* code, ARMReg dreg, ARMReg basereg, int offset);
guint8* mono_arm_emit_fsts_imm8 (guint8* code, int dreg, ARMReg basereg, int offset);
guint8* mono_arm_emit_fstd_imm8 (guint8* code, int dreg, ARMReg basereg, int offset);
guint8* mono_arm_emit_str_imm (guint8* code, ARMReg dreg, ARMReg basereg, int offset, ARMReg scratch_reg);
guint8* mono_arm_emit_strh_imm (guint8* code, ARMReg dreg, ARMReg basereg, int offset, ARMReg scratch_reg);
guint8* mono_arm_emit_strb_imm (guint8* code, ARMReg dreg, ARMReg basereg, int offset, ARMReg scratch_reg);
guint8* mono_arm_emit_fsts_imm (guint8* code, int dreg, ARMReg basereg, int offset, ARMReg scratch_reg);
guint8* mono_arm_emit_fstd_imm (guint8* code, int dreg, ARMReg basereg, int offset, ARMReg scratch_reg);
guint8* mono_arm_emit_str_reg (guint8* code, ARMReg dreg, ARMReg basereg, ARMReg offreg);
guint8* mono_arm_emit_strh_reg (guint8* code, ARMReg dreg, ARMReg basereg, ARMReg offreg);
guint8* mono_arm_emit_strb_reg (guint8* code, ARMReg dreg, ARMReg basereg, ARMReg offreg);

guint8* mono_arm_adjust_stack_imm (guint8* code, int offset);
guint8* mono_arm_adjust_stack_reg (guint8* code, ARMReg reg, int add);
guint8* mono_arm_adjust_stack_reg_imm (guint8* code, ARMReg reg, int offset);

int mono_arm_is_rotated_imm8 (guint32 val, gint *rot_amount);

void
mono_arm_throw_exception_by_token (guint32 type_token, mgreg_t pc, mgreg_t sp, mgreg_t *int_regs, gdouble *fp_regs);

gpointer
mono_arm_start_gsharedvt_call (GSharedVtCallInfo *info, gpointer *caller, gpointer *callee, gpointer *caller_regs, gpointer *callee_regs, gpointer mrgctx_reg) MONO_INTERNAL;

typedef enum {
	MONO_ARM_FPU_NONE = 0,
	MONO_ARM_FPU_FPA = 1,
	MONO_ARM_FPU_VFP = 2,
	MONO_ARM_FPU_VFP_HARD = 3
} MonoArmFPU;

/* keep the size of the structure a multiple of 8 */
struct MonoLMF {
	/* 
	 * If the second lowest bit is set to 1, then this is a MonoLMFExt structure, and
	 * the other fields are not valid.
	 */
	gpointer    previous_lmf;
	gpointer    lmf_addr;
	/* This is only set in trampoline LMF frames */
	MonoMethod *method;
	mgreg_t    sp;
	mgreg_t    ip;
	mgreg_t    fp;
	/* all but sp and pc: matches the PUSH instruction layout in the trampolines
	 * 0-4 should be considered undefined (execpt in the magic tramp)
	 * sp is saved at IP.
	 */
	mgreg_t    iregs [MONO_ARM_LMF_REGSAVE_SIZE];
};

typedef struct MonoCompileArch {
	gpointer seq_point_info_var, ss_trigger_page_var;
	gpointer seq_point_read_var, seq_point_ss_method_var;
	gpointer seq_point_bp_method_var;
	gboolean omit_fp, omit_fp_computed;
	gpointer cinfo;
} MonoCompileArch;

#define MONO_ARCH_EMULATE_FCONV_TO_I8 1
#define MONO_ARCH_EMULATE_LCONV_TO_R8 1
#define MONO_ARCH_EMULATE_LCONV_TO_R4 1
#define MONO_ARCH_EMULATE_LCONV_TO_R8_UN 1
#define MONO_ARCH_EMULATE_FREM 1
#define MONO_ARCH_EMULATE_DIV 1
#define MONO_ARCH_EMULATE_CONV_R8_UN 1
#define MONO_ARCH_EMULATE_MUL_OVF 1
//#define MONO_ARCH_BIGMUL_INTRINS 1

#define ARM_FIRST_ARG_REG 0
#define ARM_LAST_ARG_REG 3

#define MONO_ARCH_USE_SIGACTION 1

#if defined(__native_client__)
#undef MONO_ARCH_USE_SIGACTION
#endif

#define MONO_ARCH_NEED_DIV_CHECK 1

#define MONO_ARCH_HAVE_CREATE_DELEGATE_TRAMPOLINE
#define MONO_ARCH_HAVE_XP_UNWIND 1
#define MONO_ARCH_HAVE_GENERALIZED_IMT_THUNK 1

#define ARM_NUM_REG_ARGS (ARM_LAST_ARG_REG-ARM_FIRST_ARG_REG+1)
#define ARM_NUM_REG_FPARGS 0

#define MONO_ARCH_HAVE_FULL_AOT_TRAMPOLINES 1
#define MONO_ARCH_HAVE_IMT 1
#define MONO_ARCH_HAVE_DECOMPOSE_LONG_OPTS 1

#define MONO_ARCH_AOT_SUPPORTED 1
#define MONO_ARCH_LLVM_SUPPORTED 1
#define MONO_ARCH_THIS_AS_FIRST_ARG 1

#define MONO_ARCH_GSHARED_SUPPORTED 1
#define MONO_ARCH_DYN_CALL_SUPPORTED 1
#define MONO_ARCH_DYN_CALL_PARAM_AREA 24

#define MONO_ARCH_SOFT_DEBUG_SUPPORTED 1
#define MONO_ARCH_HAVE_EXCEPTIONS_INIT 1
#define MONO_ARCH_HAVE_GET_TRAMPOLINES 1
#define MONO_ARCH_HAVE_CONTEXT_SET_INT_REG 1
#define MONO_ARCH_HAVE_SIGCTX_TO_MONOCTX 1
#define MONO_ARCH_GC_MAPS_SUPPORTED 1
#define MONO_ARCH_HAVE_SETUP_ASYNC_CALLBACK 1
#define MONO_ARCH_HAVE_CONTEXT_SET_INT_REG 1
#define MONO_ARCH_HAVE_SETUP_RESUME_FROM_SIGNAL_HANDLER_CTX 1
#define MONO_ARCH_GSHAREDVT_SUPPORTED 1
#define MONO_ARCH_HAVE_GENERAL_RGCTX_LAZY_FETCH_TRAMPOLINE 1

#if defined(__native_client__)
#undef MONO_ARCH_SOFT_DEBUG_SUPPORTED
#undef MONO_ARCH_HAVE_SIGCTX_TO_MONOCTX
#undef MONO_ARCH_HAVE_CONTEXT_SET_INT_REG
#endif

/* Matches the HAVE_AEABI_READ_TP define in mini-arm.c */
#if defined(__ARM_EABI__) && defined(__linux__) && !defined(TARGET_ANDROID) && !defined(__native_client__)
#define MONO_ARCH_HAVE_TLS_GET 1
#endif

/* ARM doesn't have too many registers, so we have to use a callee saved one */
#define MONO_ARCH_RGCTX_REG ARMREG_V5
/* First argument reg */
#define MONO_ARCH_VTABLE_REG ARMREG_R0

#define MONO_CONTEXT_SET_LLVM_EXC_REG(ctx, exc) do { (ctx)->regs [0] = (gsize)exc; } while (0)

#define MONO_INIT_CONTEXT_FROM_FUNC(ctx,func) do {	\
		MONO_CONTEXT_SET_BP ((ctx), __builtin_frame_address (0));	\
		MONO_CONTEXT_SET_SP ((ctx), __builtin_frame_address (0));	\
		MONO_CONTEXT_SET_IP ((ctx), (func));	\
	} while (0)

#define MONO_ARCH_INIT_TOP_LMF_ENTRY(lmf)

void
mono_arm_throw_exception (MonoObject *exc, mgreg_t pc, mgreg_t sp, mgreg_t *int_regs, gdouble *fp_regs);

void
mono_arm_throw_exception_by_token (guint32 type_token, mgreg_t pc, mgreg_t sp, mgreg_t *int_regs, gdouble *fp_regs);

void
mono_arm_resume_unwind (guint32 dummy1, mgreg_t pc, mgreg_t sp, mgreg_t *int_regs, gdouble *fp_regs);

gboolean
mono_arm_thumb_supported (void);

GSList*
mono_arm_get_exception_trampolines (gboolean aot) MONO_INTERNAL;

guint8*
mono_arm_get_thumb_plt_entry (guint8 *code) MONO_INTERNAL;

guint8*
mono_arm_patchable_b (guint8 *code, int cond);

guint8*
mono_arm_patchable_bl (guint8 *code, int cond);

#ifdef USE_JUMP_TABLES
guint8*
mono_arm_load_jumptable_entry_addr (guint8 *code, gpointer *jte, ARMReg reg) MONO_INTERNAL;

guint8*
mono_arm_load_jumptable_entry (guint8 *code, gpointer *jte, ARMReg reg) MONO_INTERNAL;
#endif

#endif /* __MONO_MINI_ARM_H__ */
