#ifndef __MONO_MINI_WASM_H__
#define __MONO_MINI_WASM_H__

#include <mono/utils/mono-sigcontext.h>
#include <mono/utils/mono-context.h>

#define MONO_ARCH_CPU_SPEC mono_wasm_desc

#define MONO_ARCH_HAVE_INIT_LMF_EXT 1

#define MONO_MAX_IREGS 1
#define MONO_MAX_FREGS 1

#define WASM_REG_0 0


struct MonoLMF {
	/* 
	 * If the second lowest bit is set to 1, then this is a MonoLMFExt structure, and
	 * the other fields are not valid.
	 */
	gpointer previous_lmf;
	gpointer lmf_addr;

	/* This is only set in trampoline LMF frames */
	MonoMethod *method;

	gboolean top_entry;
};

typedef struct {
	int dummy;
} MonoCompileArch;

#define MONO_ARCH_INIT_TOP_LMF_ENTRY(lmf) do { (lmf)->top_entry = TRUE; } while (0)

#define MONO_CONTEXT_SET_LLVM_EXC_REG(ctx, exc) do { (ctx)->llvm_exc_reg = (gsize)exc; } while (0)

#define MONO_INIT_CONTEXT_FROM_FUNC(ctx,start_func) do {	\
		MONO_CONTEXT_SET_IP ((ctx), (start_func));	\
		MONO_CONTEXT_SET_BP ((ctx), (0));	\
		MONO_CONTEXT_SET_SP ((ctx), (0));	\
	} while (0)


#define MONO_ARCH_VTABLE_REG WASM_REG_0
#define MONO_ARCH_IMT_REG WASM_REG_0
#define MONO_ARCH_RGCTX_REG WASM_REG_0
#define MONO_ARCH_USE_FPSTACK FALSE
#define MONO_ARCH_HAVE_PATCH_CODE_NEW TRUE


//bucket of defines to make mini-codegen.c happy. They are not used since we don't use the regalloc
//FIXME take the non-regalloc bits from there and simply not compile it
#define MONO_ARCH_CALLEE_REGS 0
#define MONO_ARCH_CALLEE_FREGS 0
#define MONO_ARCH_CALLEE_SAVED_FREGS 1
#define MONO_ARCH_CALLEE_SAVED_REGS 1

#define MONO_ARCH_INST_IS_REGPAIR(desc) (desc == 'l' || desc == 'L')
#define MONO_ARCH_INST_FIXED_REG(desc) 0

#define MONO_ARCH_INST_REGPAIR_REG2(desc,hreg1) 0
#define MONO_ARCH_INST_SREG2_MASK(ins) 0
#endif /* __MONO_MINI_WASM_H__ */  
