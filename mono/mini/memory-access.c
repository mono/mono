/**
 * memory-access.c: Functions to emit memory accesses in the front-end.
 */

#include <config.h>
#include <mono/utils/mono-compiler.h>

#ifndef DISABLE_JIT

#include <mono/metadata/gc-internals.h>
#include <mono/utils/mono-memory-model.h>

#include "mini.h"
#include "ir-emit.h"
#include "jit-icalls.h"

#define MAX_INLINE_COPIES 10

//STUFF TO FIX NAMING
MonoInst* emit_get_gsharedvt_info_klass (MonoCompile *cfg, MonoClass *klass, MonoRgctxInfoType rgctx_type);
MonoInst* mono_emit_calli (MonoCompile *cfg, MonoMethodSignature *sig, MonoInst **args, MonoInst *addr, MonoInst *imt_arg, MonoInst *rgctx_arg);
MonoMethod* get_memcpy_method (void);
MonoInst* emit_memory_barrier (MonoCompile *cfg, int kind);
MonoInst* emit_runtime_constant (MonoCompile *cfg, MonoJumpInfoType patch_type, gpointer data);
int mini_class_check_context_used (MonoCompile *cfg, MonoClass *klass);
gboolean mono_emit_wb_aware_memcpy (MonoCompile *cfg, MonoClass *klass, MonoInst *iargs[4], int size, int align);



//new stuff

/* Can only copy ref-free memory */
static void 
mini_emit_unrolled_memcpy (MonoCompile *cfg, int destreg, int doffset, int srcreg, int soffset, int size, int align)
{
	int cur_reg;

	/*FIXME arbitrary hack to avoid unbound code expansion.*/
	g_assert (size < 10000);
	g_assert (align > 0);

	if (align < SIZEOF_VOID_P) {
		if (align == 4)
			goto copy_4;
		if (align == 2)
			goto copy_2;
		goto copy_1;
	}

	if (SIZEOF_VOID_P == 8) {
		while (size >= 8) {
			cur_reg = alloc_preg (cfg);
			MONO_EMIT_NEW_LOAD_MEMBASE_OP (cfg, OP_LOADI8_MEMBASE, cur_reg, srcreg, soffset);
			MONO_EMIT_NEW_STORE_MEMBASE (cfg, OP_STOREI8_MEMBASE_REG, destreg, doffset, cur_reg);
			doffset += 8;
			soffset += 8;
			size -= 8;
		}
	}

copy_4:
	while (size >= 4) {
		cur_reg = alloc_preg (cfg);
		MONO_EMIT_NEW_LOAD_MEMBASE_OP (cfg, OP_LOADI4_MEMBASE, cur_reg, srcreg, soffset);
		MONO_EMIT_NEW_STORE_MEMBASE (cfg, OP_STOREI4_MEMBASE_REG, destreg, doffset, cur_reg);
		doffset += 4;
		soffset += 4;
		size -= 4;
	}

copy_2:
	while (size >= 2) {
		cur_reg = alloc_preg (cfg);
		MONO_EMIT_NEW_LOAD_MEMBASE_OP (cfg, OP_LOADI2_MEMBASE, cur_reg, srcreg, soffset);
		MONO_EMIT_NEW_STORE_MEMBASE (cfg, OP_STOREI2_MEMBASE_REG, destreg, doffset, cur_reg);
		doffset += 2;
		soffset += 2;
		size -= 2;
	}

copy_1:
	while (size >= 1) {
		cur_reg = alloc_preg (cfg);
		MONO_EMIT_NEW_LOAD_MEMBASE_OP (cfg, OP_LOADI1_MEMBASE, cur_reg, srcreg, soffset);
		MONO_EMIT_NEW_STORE_MEMBASE (cfg, OP_STOREI1_MEMBASE_REG, destreg, doffset, cur_reg);
		doffset += 1;
		soffset += 1;
		size -= 1;
	}
}



static void 
mini_emit_memcpy_internal (MonoCompile *cfg, MonoInst *dest, MonoInst *src, int size, int align)
{
	/* FIXME: Optimize the case when src/dest is OP_LDADDR */
	
	/*
	We can't do copies at a smaller granule than the provided alignment
	*/
	if ((size / align > MAX_INLINE_COPIES) && !(cfg->opt & MONO_OPT_INTRINS)) {
		MonoInst *iargs [3];
		iargs [0] = dest;
		iargs [1] = src;

		EMIT_NEW_ICONST (cfg, iargs [2], size);
		mono_emit_method_call (cfg, get_memcpy_method (), iargs, NULL);
	} else {
		mini_emit_unrolled_memcpy (cfg, dest->dreg, 0, src->dreg, 0, size, align);
	}
}


//XXXX HACK HACK
static gboolean
mono_arch_can_do_unaligned_access (int size)
{
	return FALSE;
}


static void
mini_emit_memory_copy_internal (MonoCompile *cfg, MonoInst *dest, MonoInst *src, MonoClass *klass, gboolean native, int explicit_align)
{
	MonoInst *iargs [4];
	int size;
	guint32 align = 0;
	MonoMethod *memcpy_method;
	MonoInst *size_ins = NULL;
	MonoInst *memcpy_ins = NULL;

	g_assert (klass);
	g_assert (!(native && klass->has_references));

	if (cfg->gshared)
		klass = mono_class_from_mono_type (mini_get_underlying_type (&klass->byval_arg));

	/*
	 * This check breaks with spilled vars... need to handle it during verification anyway.
	 * g_assert (klass && klass == src->klass && klass == dest->klass);
	 */

	if (mini_is_gsharedvt_klass (klass)) {
		g_assert (!native);
		size_ins = emit_get_gsharedvt_info_klass (cfg, klass, MONO_RGCTX_INFO_VALUE_SIZE);
		memcpy_ins = emit_get_gsharedvt_info_klass (cfg, klass, MONO_RGCTX_INFO_MEMCPY);
	}

	if (native)
		size = mono_class_native_size (klass, &align);
	else
		size = mono_class_value_size (klass, &align);

	if (!align)
		align = SIZEOF_VOID_P;
	if (explicit_align)
		align = explicit_align;

	/* if native is true there should be no references in the struct */
	if (cfg->gen_write_barriers && (klass->has_references || size_ins) && !native) {
		/* Avoid barriers when storing to the stack */
		if (!((dest->opcode == OP_ADD_IMM && dest->sreg1 == cfg->frame_reg) ||
			  (dest->opcode == OP_LDADDR))) {
			int context_used;

			iargs [0] = dest;
			iargs [1] = src;

			context_used = mini_class_check_context_used (cfg, klass);

			/* It's ok to intrinsify under gsharing since shared code types are layout stable. */
			if (!size_ins && (cfg->opt & MONO_OPT_INTRINS) && mono_emit_wb_aware_memcpy (cfg, klass, iargs, size, align)) {
				return;
			} else if (size_ins || align < SIZEOF_VOID_P) {
				if (context_used) {
					iargs [2] = mini_emit_get_rgctx_klass (cfg, context_used, klass, MONO_RGCTX_INFO_KLASS);
				}  else {
					iargs [2] = emit_runtime_constant (cfg, MONO_PATCH_INFO_CLASS, klass);
					if (!cfg->compile_aot)
						mono_class_compute_gc_descriptor (klass);
				}
				if (size_ins)
					mono_emit_jit_icall (cfg, mono_gsharedvt_value_copy, iargs);
				else
					mono_emit_jit_icall (cfg, mono_value_copy, iargs);
			} else {
				/* We don't unroll more than 5 stores to avoid code bloat. */
				/*This is harmless and simplify mono_gc_get_range_copy_func */
				size += (SIZEOF_VOID_P - 1);
				size &= ~(SIZEOF_VOID_P - 1);

				EMIT_NEW_ICONST (cfg, iargs [2], size);
				mono_emit_jit_icall (cfg, mono_gc_get_range_copy_func (), iargs);
			}
		}
	}

	if (size_ins) {
		iargs [0] = dest;
		iargs [1] = src;
		iargs [2] = size_ins;
		mono_emit_calli (cfg, mono_method_signature (memcpy_method), iargs, memcpy_ins, NULL, NULL);		
	} else {
		mini_emit_memcpy_internal (cfg, dest, src, size, align);
	}
}


void
mini_emit_memory_copy (MonoCompile *cfg, MonoInst *dest, MonoInst *src, MonoClass *klass, int ins_flag)
{
	if (cfg->verbose_level > 3)
		printf ("EMITING MEMORY COPY FROM %d to %d flags %x\n", src->dreg, dest->dreg, ins_flag);
	
	int explicit_align = 0;
	if (ins_flag & MONO_INST_UNALIGNED)
		explicit_align = 1;

	mini_emit_memory_copy_internal (cfg, dest, src, klass, FALSE, explicit_align);
	
	if (ins_flag & MONO_INST_VOLATILE) {
		/* Volatile loads have acquire semantics, see 12.6.7 in Ecma 335 */
		emit_memory_barrier (cfg, MONO_MEMORY_BARRIER_ACQ);
	}
}




MonoInst*
mini_emit_memory_load (MonoCompile *cfg, MonoClass *klass, MonoInst *src_address, int ins_flag)
{
	MonoInst *ins;
	if (cfg->verbose_level > 3)
		printf ("EMITING MEMORY LOAD FROM %d flags %x\n", src_address->dreg, ins_flag);

	if (ins_flag & MONO_INST_UNALIGNED) {
		int align;
		int size = mono_type_size (&klass->byval_arg, &align);
		if (mono_arch_can_do_unaligned_access (size)) {
			EMIT_NEW_LOAD_MEMBASE_TYPE (cfg, ins, &klass->byval_arg, src_address->dreg, 0);
		} else {
			MonoInst *addr;
			ins = mono_compile_create_var (cfg, &klass->byval_arg, OP_LOCAL);
			EMIT_NEW_VARLOADA (cfg, addr, ins, ins->inst_vtype);
			mini_emit_memcpy_internal (cfg, addr, src_address, size, 1);
		}
	} else {
		EMIT_NEW_LOAD_MEMBASE_TYPE (cfg, ins, &klass->byval_arg, src_address->dreg, 0);
	}
	ins->flags |= ins_flag;

	if (ins_flag & MONO_INST_VOLATILE) {
		/* Volatile loads have acquire semantics, see 12.6.7 in Ecma 335 */
		emit_memory_barrier (cfg, MONO_MEMORY_BARRIER_ACQ);
	}
	return ins;
}

#endif

