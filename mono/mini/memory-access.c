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
MonoMethod* get_memset_method (void);
MonoInst* emit_memory_barrier (MonoCompile *cfg, int kind);
MonoInst* emit_runtime_constant (MonoCompile *cfg, MonoJumpInfoType patch_type, gpointer data);
int mini_class_check_context_used (MonoCompile *cfg, MonoClass *klass);
gboolean mono_emit_wb_aware_memcpy (MonoCompile *cfg, MonoClass *klass, MonoInst *iargs[4], int size, int align);
void emit_write_barrier (MonoCompile *cfg, MonoInst *ptr, MonoInst *value);


//new funcs to go to mini.h later
void mini_emit_memory_copy_bytes (MonoCompile *cfg, MonoInst *dest, MonoInst *src, MonoInst *size, int ins_flag);
void mini_emit_memory_copy (MonoCompile *cfg, MonoInst *dest, MonoInst *src, MonoClass *klass, int ins_flag);
void mini_emit_memory_store (MonoCompile *cfg, MonoClass *klass, MonoInst *dest_address, MonoInst *src, int ins_flag);
MonoInst* mini_emit_memory_load (MonoCompile *cfg, MonoClass *klass, MonoInst *src_address, int ins_flag);
void mini_emit_memory_init_bytes (MonoCompile *cfg, MonoInst *dest, MonoInst *value, MonoInst *size, int ins_flag);


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

	//Unaligned offsets don't naturaly happen in the runtime, so it's ok to be conservative in how we copy
	//On input src and dest must be aligned to `align` so offset just worsen it
	int offsets_mask = (doffset | soffset) & 0x7; //we only care about the misalignment part
	if (offsets_mask) {
		if (offsets_mask % 2 == 1)
			goto copy_1;
		if (offsets_mask % 4 == 2)
			goto copy_2;
		if (SIZEOF_VOID_P == 8 && offsets_mask % 8 == 4)
			goto copy_4;
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
mini_emit_unrolled_memset (MonoCompile *cfg, int destreg, int size, int val, int align)
{
	/* FIXME support value != 0 using patterning */
	int val_reg;
	int offset = 0;

	g_assert (val == 0);
	g_assert (align > 0);

	if ((size <= SIZEOF_REGISTER) && (size <= align)) {
		switch (size) {
		case 1:
			MONO_EMIT_NEW_STORE_MEMBASE_IMM (cfg, OP_STOREI1_MEMBASE_IMM, destreg, offset, val);
			return;
		case 2:
			MONO_EMIT_NEW_STORE_MEMBASE_IMM (cfg, OP_STOREI2_MEMBASE_IMM, destreg, offset, val);
			return;
		case 4:
			MONO_EMIT_NEW_STORE_MEMBASE_IMM (cfg, OP_STOREI4_MEMBASE_IMM, destreg, offset, val);
			return;
#if SIZEOF_REGISTER == 8
		case 8:
			MONO_EMIT_NEW_STORE_MEMBASE_IMM (cfg, OP_STOREI8_MEMBASE_IMM, destreg, offset, val);
			return;
#endif
		}
	}

	val_reg = alloc_preg (cfg);

	if (SIZEOF_REGISTER == 8)
		MONO_EMIT_NEW_I8CONST (cfg, val_reg, val);
	else
		MONO_EMIT_NEW_ICONST (cfg, val_reg, val);

	if (align < SIZEOF_VOID_P) {
		if (align == 4)
			goto set_4;
		if (align == 2)
			goto set_2;
		goto set_1;
	}

	if (SIZEOF_REGISTER == 8) {
		while (size >= 8) {
			MONO_EMIT_NEW_STORE_MEMBASE (cfg, OP_STOREI8_MEMBASE_REG, destreg, offset, val_reg);
			offset += 8;
			size -= 8;
		}
	}

set_4:
	while (size >= 4) {
		MONO_EMIT_NEW_STORE_MEMBASE (cfg, OP_STOREI4_MEMBASE_REG, destreg, offset, val_reg);
		offset += 4;
		size -= 4;
	}

set_2:
	while (size >= 2) {
		MONO_EMIT_NEW_STORE_MEMBASE (cfg, OP_STOREI2_MEMBASE_REG, destreg, offset, val_reg);
		offset += 2;
		size -= 2;
	}

set_1:
	while (size >= 1) {
		MONO_EMIT_NEW_STORE_MEMBASE (cfg, OP_STOREI1_MEMBASE_REG, destreg, offset, val_reg);
		offset += 1;
		size -= 1;
	}
}



static void 
mini_emit_memcpy_internal (MonoCompile *cfg, MonoInst *dest, MonoInst *src, MonoInst *size_ins, int size, int align)
{
	/* FIXME: Optimize the case when src/dest is OP_LDADDR */
	if (cfg->verbose_level)
		printf ("\tEMITING memcpy [%d] <= [%d] size_ins %d size %d align %d\n", dest->dreg, src->dreg, size_ins ? size_ins->dreg : -1, size, align);
	/*
	We can't do copies at a smaller granule than the provided alignment
	*/
	if (size_ins || ((size / align > MAX_INLINE_COPIES) && !(cfg->opt & MONO_OPT_INTRINS))) {
		MonoInst *iargs [3];
		iargs [0] = dest;
		iargs [1] = src;

		if (!size_ins)
			EMIT_NEW_ICONST (cfg, size_ins, size);
		iargs [2] = size_ins;
		mono_emit_method_call (cfg, get_memcpy_method (), iargs, NULL);
	} else {
		mini_emit_unrolled_memcpy (cfg, dest->dreg, 0, src->dreg, 0, size, align);
	}
}
static void 
mini_emit_memset_internal (MonoCompile *cfg, MonoInst *dest, MonoInst *value_ins, int value, MonoInst *size_ins, int size, int align)
{
	/* FIXME: Optimize the case when dest is OP_LDADDR */

	if (cfg->verbose_level)
		printf ("\tEMITING memset [%d] value R%d (v %d) size R%d (v %d) align %d\n", dest->dreg, value_ins ? value_ins->dreg : -1, value, size_ins ? size_ins->dreg : -1, size, align);
	/*
	We can't do copies at a smaller granule than the provided alignment
	*/
	if (value_ins || size_ins || value != 0 || ((size / align > MAX_INLINE_COPIES) && !(cfg->opt & MONO_OPT_INTRINS))) {
		MonoInst *iargs [3];
		iargs [0] = dest;

		if (!value_ins)
			EMIT_NEW_ICONST (cfg, value_ins, value);
		iargs [1] = value_ins;

		if (!size_ins)
			EMIT_NEW_ICONST (cfg, size_ins, size);
		iargs [2] = size_ins;

		mono_emit_method_call (cfg, get_memset_method (), iargs, NULL);
	} else {
		mini_emit_unrolled_memset (cfg, dest->dreg, size, value, align);
	}
}

static void
mini_emit_memcpy_const_size (MonoCompile *cfg, MonoInst *dest, MonoInst *src, int size, int align)
{
	mini_emit_memcpy_internal (cfg, dest, src, NULL, size, align);
}


static void
mini_emit_memset_const_size (MonoCompile *cfg, MonoInst *dest, int value, int size, int align)
{
	mini_emit_memset_internal (cfg, dest, NULL, value, NULL, size, align);
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
		mono_emit_calli (cfg, mono_method_signature (get_memcpy_method ()), iargs, memcpy_ins, NULL, NULL);
	} else {
		mini_emit_memcpy_const_size (cfg, dest, src, size, align);
	}
}


void
mini_emit_memory_copy_bytes (MonoCompile *cfg, MonoInst *dest, MonoInst *src, MonoInst *size, int ins_flag)
{
	if (cfg->verbose_level > 3)
		printf ("EMITING MEMORY COPY BYTES FROM %d to %d size-var %d flags %x\n", src->dreg, dest->dreg, size->dreg, ins_flag);

	int align = (ins_flag & MONO_INST_UNALIGNED) ? 1 : SIZEOF_VOID_P;


	/* FIXME: See mini_emit_memory_copy caveat */
	if (ins_flag & MONO_INST_VOLATILE) {
		/* Volatile loads have acquire semantics, see 12.6.7 in Ecma 335 */
		emit_memory_barrier (cfg, MONO_MEMORY_BARRIER_SEQ);
	}

	if ((cfg->opt & MONO_OPT_INTRINS) && (size->opcode == OP_ICONST)) {
		if (cfg->verbose_level > 3)
			printf ("EMITING CONST COPY for %lld bytes\n", size->inst_c0);
		mini_emit_memcpy_const_size (cfg, dest, src, size->inst_c0, align);
	} else {
		if (cfg->verbose_level > 3)
			printf ("EMITING REGULAR COPY\n");
		mini_emit_memcpy_internal (cfg, dest, src, size, 0, align);
	}

	if (ins_flag & MONO_INST_VOLATILE) {
		/* Volatile loads have acquire semantics, see 12.6.7 in Ecma 335 */
		emit_memory_barrier (cfg, MONO_MEMORY_BARRIER_SEQ);
	}
}

void
mini_emit_memory_init_bytes (MonoCompile *cfg, MonoInst *dest, MonoInst *value, MonoInst *size, int ins_flag)
{
	if (cfg->verbose_level > 3)
		printf ("EMITING MEMORY INIT BYTES TO R%d size-ins R%d flags %x\n", dest->dreg, size ? size->dreg : 1, ins_flag);

	int align = (ins_flag & MONO_INST_UNALIGNED) ? 1 : SIZEOF_VOID_P;

	if (ins_flag & MONO_INST_VOLATILE) {
		/* Volatile stores have release semantics, see 12.6.7 in Ecma 335 */
		emit_memory_barrier (cfg, MONO_MEMORY_BARRIER_REL);
	}

	//FIXME unrolled memset only supports zeroing
	if ((cfg->opt & MONO_OPT_INTRINS) && (size->opcode == OP_ICONST) && (value->opcode == OP_ICONST) && (value->inst_c0 == 0)) {
		if (cfg->verbose_level > 3)
			printf ("EMITING CONST INIT for %lld bytes with %lld value\n", size->inst_c0, size->inst_c0);
		mini_emit_memset_const_size (cfg, dest, value->inst_c0, size->inst_c0, align);
	} else {
		if (cfg->verbose_level > 3)
			printf ("EMITING REGULAR MEMSET\n");
		mini_emit_memset_internal (cfg, dest, value, 0, size, 0, align);
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

	/*
	 * FIXME: It's unclear whether we should be emitting both the acquire
	 * and release barriers for cpblk. It is technically both a load and
	 * store operation, so it seems like that's the sensible thing to do.
	 *
	 * FIXME: We emit full barriers on both sides of the operation for
	 * simplicity. We should have a separate atomic memcpy method instead.
	 */
	if (ins_flag & MONO_INST_VOLATILE) {
		/* Volatile loads have acquire semantics, see 12.6.7 in Ecma 335 */
		emit_memory_barrier (cfg, MONO_MEMORY_BARRIER_SEQ);
	}

	mini_emit_memory_copy_internal (cfg, dest, src, klass, FALSE, explicit_align);
	
	if (ins_flag & MONO_INST_VOLATILE) {
		/* Volatile loads have acquire semantics, see 12.6.7 in Ecma 335 */
		emit_memory_barrier (cfg, MONO_MEMORY_BARRIER_SEQ);
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
			mini_emit_memcpy_const_size (cfg, addr, src_address, size, 1);
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

void
mini_emit_memory_store (MonoCompile *cfg, MonoClass *klass, MonoInst *dest_address, MonoInst *src, int ins_flag)
{
	MonoInst *ins;

	if (ins_flag & MONO_INST_VOLATILE) {
		/* Volatile stores have release semantics, see 12.6.7 in Ecma 335 */
		emit_memory_barrier (cfg, MONO_MEMORY_BARRIER_REL);
	}
	if (ins_flag & MONO_INST_UNALIGNED) {
		int align;
		int size = mono_type_size (&klass->byval_arg, &align);
		if (!mono_arch_can_do_unaligned_access (size)) {
			MonoInst *addr, *mov, *tmp_var;
			tmp_var = mono_compile_create_var (cfg, &klass->byval_arg, OP_LOCAL);
			EMIT_NEW_TEMPSTORE (cfg, mov, tmp_var->inst_c0, src);
			EMIT_NEW_VARLOADA (cfg, addr, tmp_var, tmp_var->inst_vtype);
			mini_emit_memory_copy_internal (cfg, dest_address, addr, klass, FALSE, 1);
			return;
		}
	}

	EMIT_NEW_STORE_MEMBASE_TYPE (cfg, ins, &klass->byval_arg, dest_address->dreg, 0, src->dreg);
	ins->flags |= ins_flag;
	if (cfg->gen_write_barriers && cfg->method->wrapper_type != MONO_WRAPPER_WRITE_BARRIER &&
		mini_type_is_reference (&klass->byval_arg) && !MONO_INS_IS_PCONST_NULL (src)) {
		/* insert call to write barrier */
		emit_write_barrier (cfg, dest_address, src);
	}
}


#endif

