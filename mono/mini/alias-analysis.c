/**
 * \file
 * Implement simple alias analysis for local variables.
 *
 * Author:
 *   Rodrigo Kumpera (kumpera@gmail.com)
 *
 * (C) 2013 Xamarin
 */

#include <config.h>
#include <stdio.h>

#include "mini.h"
#include "ir-emit.h"
#include "glib.h"
#include <mono/utils/mono-compiler.h>

#ifndef DISABLE_JIT

static gboolean
is_int_stack_size (int type)
{
#if SIZEOF_VOID_P == 4
	return type == STACK_I4 || type == STACK_MP;
#else
	return type == STACK_I4;
#endif
}

static gboolean
is_long_stack_size (int type)
{
#if SIZEOF_VOID_P == 8
	return type == STACK_I8 || type == STACK_MP;
#else
	return type == STACK_I8;
#endif
}


static gboolean
lower_load (MonoCompile *cfg, MonoInst *load, MonoInst *ldaddr)
{
	MonoInst *var = (MonoInst *)ldaddr->inst_p0;
	MonoType *type = m_class_get_byval_arg (var->klass);
	int replaced_op = mono_type_to_load_membase (cfg, type);

	if (load->opcode == OP_LOADV_MEMBASE && load->klass != var->klass) {
		if (cfg->verbose_level > 2)
			printf ("Incompatible load_vtype classes %s x %s\n", m_class_get_name (load->klass), m_class_get_name (var->klass));
		return FALSE;
	}

	if (replaced_op != load->opcode) {
		if (cfg->verbose_level > 2) 
			printf ("Incompatible load type: expected %s but got %s\n", 
				mono_inst_name (replaced_op),
				mono_inst_name (load->opcode));
		return FALSE;
	} else {
		if (cfg->verbose_level > 2) { printf ("mem2reg replacing: "); mono_print_ins (load); }
	}

	load->opcode = mono_type_to_regmove (cfg, type);
	mini_type_to_eval_stack_type (cfg, type, load);
	load->sreg1 = var->dreg;
	mono_atomic_inc_i32 (&mono_jit_stats.loads_eliminated);
	return TRUE;
}

static gboolean
lower_store (MonoCompile *cfg, MonoInst *store, MonoInst *ldaddr)
{
	MonoInst *var = (MonoInst *)ldaddr->inst_p0;
	MonoType *type = m_class_get_byval_arg (var->klass);
	int replaced_op = mono_type_to_store_membase (cfg, type);

	if (store->opcode == OP_STOREV_MEMBASE && store->klass != var->klass) {
		if (cfg->verbose_level > 2)
			printf ("Incompatible store_vtype classes %s x %s\n", m_class_get_name (store->klass), m_class_get_name (store->klass));
		return FALSE;
	}


	if (replaced_op != store->opcode) {
		if (cfg->verbose_level > 2) 
			printf ("Incompatible store_reg type: expected %s but got %s\n", 
				mono_inst_name (replaced_op),
				mono_inst_name (store->opcode));
		return FALSE;
	} else {
		if (cfg->verbose_level > 2) { printf ("mem2reg replacing: "); mono_print_ins (store); }
	}

	int coerce_op = mono_type_to_stloc_coerce (type);
	if (coerce_op)
		store->opcode = coerce_op;
	else
		store->opcode = mono_type_to_regmove (cfg, type);
	mini_type_to_eval_stack_type (cfg, type, store);
	store->dreg = var->dreg;
	mono_atomic_inc_i32 (&mono_jit_stats.stores_eliminated);
	return TRUE;
}

static gboolean
lower_store_imm (MonoCompile *cfg, MonoInst *store, MonoInst *ldaddr)
{
	MonoInst *var = (MonoInst *)ldaddr->inst_p0;
	MonoType *type = m_class_get_byval_arg (var->klass);
	int store_op = mono_type_to_store_membase (cfg, type);
	if (store_op == OP_STOREV_MEMBASE || store_op == OP_STOREX_MEMBASE)
		return FALSE;

	switch (store->opcode) {
#if SIZEOF_VOID_P == 4
	case OP_STORE_MEMBASE_IMM:
#endif
	case OP_STOREI4_MEMBASE_IMM:
		if (!is_int_stack_size (var->type)) {
			if (cfg->verbose_level > 2) printf ("Incompatible variable of size != 4\n");
			return FALSE;
		}
		if (cfg->verbose_level > 2) { printf ("mem2reg replacing: "); mono_print_ins (store); }
		store->opcode = OP_ICONST;
		store->type = STACK_I4;
		store->dreg = var->dreg;
		store->inst_c0 = store->inst_imm;
		break;

#if SIZEOF_VOID_P == 8
	case OP_STORE_MEMBASE_IMM:
#endif    
	case OP_STOREI8_MEMBASE_IMM:
	 	if (!is_long_stack_size (var->type)) {
			if (cfg->verbose_level > 2) printf ("Incompatible variable of size != 8\n");
			return FALSE;
		}
		if (cfg->verbose_level > 2) { printf ("mem2reg replacing: "); mono_print_ins (store); }
		store->opcode = OP_I8CONST;
		store->type = STACK_I8;
		store->dreg = var->dreg;
		store->inst_l = store->inst_imm;
		break;
	default:
		return FALSE;
	}
	mono_atomic_inc_i32 (&mono_jit_stats.stores_eliminated);
	return TRUE;
}

/*
 * compute_defs:
 *
 * Find all vregs whose only definition is an ldaddr opcode.
 */
static G_GNUC_UNUSED void
compute_defs (MonoCompile *cfg, MonoInst **defs)
{
	MonoBasicBlock *bb;
	MonoInst *ins;

	for (bb = cfg->bb_entry; bb; bb = bb->next_bb) {
		for (ins = bb->code; ins; ins = ins->next) {
			if (ins->dreg != -1) {
				if (defs [ins->dreg])
					/* Mark as having multiple defs */
					defs [ins->dreg] = GINT_TO_POINTER (-1);
				else
					defs [ins->dreg] = ins;
			}
		}
	}

	for (int i = 0; i < cfg->next_vreg; ++i) {
		if (defs [i]) {
			if (defs [i] == GINT_TO_POINTER (-1) || defs [i]->opcode != OP_LDADDR)
				defs [i] = NULL;
		}
	}

	for (int i = 0; i < cfg->next_vreg; ++i) {
		if (defs [i]) {
			ins = defs [i];
			MonoInst *var = (MonoInst*)ins->inst_p0;
			if (var->flags & MONO_INST_VOLATILE) {
				if (cfg->verbose_level > 2) { printf ("Found address to volatile var, can't take it: "); mono_print_ins (ins); }
				defs [i] = NULL;
			} else {
				if (cfg->verbose_level > 2) { printf ("New global address: "); mono_print_ins (ins); printf ("%s\n", mono_type_full_name (var->inst_vtype)); }
			}
		}
	}
}

static gboolean
lower_memory_access (MonoCompile *cfg)
{
	MonoBasicBlock *bb;
	MonoInst *ins, *tmp;
	gboolean needs_dce = FALSE;
	GHashTable *addr_loads = g_hash_table_new (NULL, NULL);
	MonoInst **defs;

	defs = g_malloc0 (sizeof (MonoInst*) * cfg->next_vreg);
	if (COMPILE_LLVM (cfg))
		compute_defs (cfg, defs);

	for (bb = cfg->bb_entry; bb; bb = bb->next_bb) {
		g_hash_table_remove_all (addr_loads);

		for (ins = bb->code; ins; ins = ins->next) {
handle_instruction:
			switch (ins->opcode) {
			case OP_LDADDR: {
				MonoInst *var = (MonoInst*)ins->inst_p0;
				if (var->flags & MONO_INST_VOLATILE) {
					if (cfg->verbose_level > 2) { printf ("Found address to volatile var, can't take it: "); mono_print_ins (ins); }
				} else {
					g_hash_table_insert (addr_loads, GINT_TO_POINTER (ins->dreg), ins);
					if (cfg->verbose_level > 2) { printf ("New address: "); mono_print_ins (ins); }
				}
				break;
			}

			case OP_MOVE:
				tmp = (MonoInst*)g_hash_table_lookup (addr_loads, GINT_TO_POINTER (ins->sreg1));
				/*
				Forward propagate known aliases
				ldaddr R10 <- R8
				mov R11 <- R10
				*/
				if (tmp) {
					g_hash_table_insert (addr_loads, GINT_TO_POINTER (ins->dreg), tmp);
					if (cfg->verbose_level > 2) { printf ("New alias: "); mono_print_ins (ins); }
				} else {
					/*
					Source value is not a know address, kill the variable.
					*/
					if (g_hash_table_remove (addr_loads, GINT_TO_POINTER (ins->dreg))) {
						if (cfg->verbose_level > 2) { printf ("Killed alias: "); mono_print_ins (ins); }
					}
				}
				break;

			case OP_LOADV_MEMBASE:
			case OP_LOAD_MEMBASE:
			case OP_LOADU1_MEMBASE:
			case OP_LOADI2_MEMBASE:
			case OP_LOADU2_MEMBASE:
			case OP_LOADI4_MEMBASE:
			case OP_LOADU4_MEMBASE:
			case OP_LOADI1_MEMBASE:
			case OP_LOADI8_MEMBASE:
#ifndef MONO_ARCH_SOFT_FLOAT_FALLBACK
			case OP_LOADR4_MEMBASE:
#endif
			case OP_LOADR8_MEMBASE:
				// FIXME: Unify the two kinds of lookups
				tmp = defs [ins->sreg1];
				if (tmp) {
					MonoInst *var = (MonoInst*)tmp->inst_p0;
					if (MONO_TYPE_ISSTRUCT (var->inst_vtype) && ins->opcode != OP_LOADV_MEMBASE && mini_is_scalar_repl_class (var->klass)) {
						/* Loading a field of a vtype */
						if (ins->opcode == OP_LOADI4_MEMBASE || ins->opcode == OP_LOAD_MEMBASE) {
							if (cfg->verbose_level > 2) { printf ("Converted to extract:"); mono_print_ins (ins); }
							MonoInst *var = (MonoInst*)tmp->inst_p0;
							ins->opcode = mono_load_membase_to_extract (ins->opcode);
							ins->sreg1 = var->dreg;
							ins->inst_imm = mini_field_offset_to_field_index (var->klass, ins->inst_offset);
							ins->klass = var->klass;
							needs_dce = TRUE;
						} else {
							if (cfg->verbose_level > 2) { printf ("Skipped:"); mono_print_ins (ins); }
							continue;
						}
						break;
					}
				}
				tmp = (MonoInst *)g_hash_table_lookup (addr_loads, GINT_TO_POINTER (ins->sreg1));
				if (!tmp)
					break;
				if (ins->inst_offset != 0)
					break;
				if (cfg->verbose_level > 2) { printf ("Found candidate load:"); mono_print_ins (ins); }
				if (lower_load (cfg, ins, tmp)) {
					needs_dce = TRUE;
					/* Try to propagate known aliases if an OP_MOVE was inserted */
					goto handle_instruction;
				}
				break;

			case OP_STORE_MEMBASE_REG:
			case OP_STOREI1_MEMBASE_REG:
			case OP_STOREI2_MEMBASE_REG:
			case OP_STOREI4_MEMBASE_REG:
			case OP_STOREI8_MEMBASE_REG:
#ifndef MONO_ARCH_SOFT_FLOAT_FALLBACK
			case OP_STORER4_MEMBASE_REG:
#endif
			case OP_STORER8_MEMBASE_REG:
			case OP_STOREV_MEMBASE:
				if (ins->inst_offset != 0)
					continue;
				tmp = (MonoInst *)g_hash_table_lookup (addr_loads, GINT_TO_POINTER (ins->dreg));
				if (tmp) {
					if (cfg->verbose_level > 2) { printf ("Found candidate store:"); mono_print_ins (ins); }
					if (lower_store (cfg, ins, tmp)) {
						needs_dce = TRUE;
						/* Try to propagate known aliases if an OP_MOVE was inserted */
						goto handle_instruction;
					}
				}
				break;
			//FIXME missing storei1_membase_imm and storei2_membase_imm
			case OP_STORE_MEMBASE_IMM:
			case OP_STOREI4_MEMBASE_IMM:
			case OP_STOREI8_MEMBASE_IMM:
				if (ins->inst_offset != 0)
					continue;
				tmp = (MonoInst *)g_hash_table_lookup (addr_loads, GINT_TO_POINTER (ins->dreg));
				if (tmp) {
					if (cfg->verbose_level > 2) { printf ("Found candidate store-imm:"); mono_print_ins (ins); }
					needs_dce |= lower_store_imm (cfg, ins, tmp);
				}
				break;
			case OP_CHECK_THIS:
			case OP_NOT_NULL:
				tmp = (MonoInst *)g_hash_table_lookup (addr_loads, GINT_TO_POINTER (ins->sreg1));
				if (tmp) {
					if (cfg->verbose_level > 2) { printf ("Found null check over local: "); mono_print_ins (ins); }
					NULLIFY_INS (ins);
					needs_dce = TRUE;
				}
				break;
			}
		}
	}
	g_hash_table_destroy (addr_loads);
	g_free (defs);

	return needs_dce;
}

/*
 * mini_is_scalar_repl_class:
 *
 *   Return whenever scalar replacement of aggregates is enabled for KLASS.
 */
gboolean
mini_is_scalar_repl_class (MonoClass *klass)
{
	if (!MONO_TYPE_ISSTRUCT (m_class_get_byval_arg (klass)))
		return FALSE;

	if (!(klass->image == mono_get_corlib () && (!strcmp (klass->name, "Span`1") || !strcmp (klass->name, "ReadOnlySpan`1"))))
		return FALSE;
	if (mono_class_get_field_count (klass) != 3)
		/* Fast span */
		return FALSE;
	if (mini_is_gsharedvt_klass (klass))
		return FALSE;
	return TRUE;
}

static gboolean
recompute_aliased_variables (MonoCompile *cfg, int *restored_vars)
{
	int i;
	MonoBasicBlock *bb;
	MonoInst *ins;
	int kills = 0;
	int adds = 0;
	*restored_vars = 0;

	for (i = 0; i < cfg->num_varinfo; i++) {
		MonoInst *var = cfg->varinfo [i];
		if (var->flags & MONO_INST_INDIRECT) {
			if (cfg->verbose_level > 2) {
				printf ("Killing :"); mono_print_ins (var);
			}
			++kills;
		}
		var->flags &= ~MONO_INST_INDIRECT;
	}

	if (!kills)
		return FALSE;

	for (bb = cfg->bb_entry; bb; bb = bb->next_bb) {
		for (ins = bb->code; ins; ins = ins->next) {
			if (ins->opcode == OP_LDADDR) {
				MonoInst *var;

				if (cfg->verbose_level > 2) { printf ("Found op :"); mono_print_ins (ins); }

				var = (MonoInst*)ins->inst_p0;
				if (!(var->flags & MONO_INST_INDIRECT)) {
					if (cfg->verbose_level > 1) { printf ("Restoring :"); mono_print_ins (var); }
					++adds;
				}
				var->flags |= MONO_INST_INDIRECT;
			}
		}
	}
	*restored_vars = adds;

	mono_atomic_fetch_add_i32 (&mono_jit_stats.alias_found, kills);
	mono_atomic_fetch_add_i32 (&mono_jit_stats.alias_removed, kills - adds);
	if (kills > adds) {
		if (cfg->verbose_level > 2) {
			printf ("Method: %s\n", mono_method_full_name (cfg->method, 1));
			printf ("Kills %d Adds %d\n", kills, adds);
		}
		return TRUE;
	}
	return FALSE;
}

/*
FIXME:
	Don't DCE on the whole CFG, only the BBs that have changed.

TODO:
	SRVT of small types can fix cases of mismatch for fields of a different type than the component.
	Handle aliasing of byrefs in call conventions.
*/
void
mono_local_alias_analysis (MonoCompile *cfg)
{
	int i, restored_vars = 1;
	if (!cfg->has_indirection)
		return;

	if (cfg->verbose_level > 2)
		mono_print_code (cfg, "BEFORE ALIAS_ANALYSIS");

	/*
	Remove indirection and memory access of known variables.
	*/
	if (!lower_memory_access (cfg))
		goto done;

	/*
	By replacing indirect access with direct operations, some LDADDR ops become dead. Kill them.
	*/
	if (cfg->opt & MONO_OPT_DEADCE)
		mono_local_deadce (cfg);

	/*
	Some variables no longer need to be flagged as indirect, find them.
	Since indirect vars are converted into global vregs, each pass eliminates only one level of indirection.
	Most cases only need one pass and some 2.
	*/
	for (i = 0; i < 3 && restored_vars > 0 && recompute_aliased_variables (cfg, &restored_vars); ++i) {
		/*
		A lot of simplification just took place, we recompute local variables and do DCE to
		really profit from the previous gains
		*/
		mono_handle_global_vregs (cfg);
		if (cfg->opt & MONO_OPT_DEADCE)
			mono_local_deadce (cfg);
	}

done:
	if (cfg->verbose_level > 2)
		mono_print_code (cfg, "AFTER ALIAS_ANALYSIS");
}

typedef struct {
	MonoInst *var;
	MonoClass *klass;
	MonoClassField *fields;
	int fcount;
	MonoInst **scalars;
} ScalarReplVarInfo;

static ScalarReplVarInfo*
get_scalar_repl_var (MonoCompile *cfg, GHashTable *vars, int vreg)
{
	MonoInst *var = get_vreg_to_inst (cfg, vreg);
	if (!var)
		return NULL;
	if ((var->flags & MONO_INST_VOLATILE) || (var->flags & MONO_INST_INDIRECT))
		return NULL;
	if (var->opcode != OP_LOCAL)
		return NULL;
	if (!MONO_TYPE_ISSTRUCT (var->inst_vtype))
		return NULL;
	MonoClass *klass = mono_class_from_mono_type (var->inst_vtype);
	if (!mini_is_scalar_repl_class (klass))
		return NULL;

	ScalarReplVarInfo *info = g_hash_table_lookup (vars, GINT_TO_POINTER (vreg));
	if (!info) {
		info = mono_mempool_alloc0 (cfg->mempool, sizeof (ScalarReplVarInfo));
		info->var = var;
		info->klass = klass;
		info->fields = m_class_get_fields (klass);
		info->fcount = mono_class_get_field_count (klass);
		info->scalars = mono_mempool_alloc0 (cfg->mempool, sizeof (MonoInst*) * info->fcount);

		for (int i = 0; i < info->fcount; ++i) {
			MonoClassField *field = &info->fields [i];
			MonoType *t = field->type;
			info->scalars [i] = mono_compile_create_var (cfg, t, OP_LOCAL);
			if (cfg->verbose_level > 2) {
				printf ("Map R%d %s(0x%0x) to: ", var->dreg, field->name, (int)(field->offset - sizeof (MonoObject)));
				mono_print_ins (info->scalars [i]);
			}
		}

		g_hash_table_insert (vars, GINT_TO_POINTER (vreg), info);
	}
	return info;
}

static void
emit_vtype_to_scalar (MonoCompile *cfg, ScalarReplVarInfo *info, int sreg)
{
	for (int i = 0; i < info->fcount; ++i) {
		MonoInst *extract;

		MONO_INST_NEW (cfg, extract, mono_load_membase_to_extract (mono_type_to_load_membase (cfg, info->scalars [i]->inst_vtype)));
		extract->sreg1 = sreg;
		extract->inst_offset = info->fields [i].offset - sizeof (MonoObject);
		extract->dreg = info->scalars [i]->dreg;
		extract->klass = info->var->klass;
		extract->inst_imm = i;
		MONO_ADD_INS (cfg->cbb, extract);
		if (cfg->verbose_level > 2) {
			printf ("\tAdd: ");
			mono_print_ins (extract);
		}
	}
}

static void
emit_scalar_to_vtype (MonoCompile *cfg, ScalarReplVarInfo *info, int dreg)
{
	MONO_EMIT_NEW_VZERO (cfg, dreg, info->var->klass);
	/* Transform into a series of INSERT ops */
	for (int i = 0; i < info->fcount; ++i) {
		MonoInst *tmp;

		MONO_INST_NEW (cfg, tmp, mono_load_membase_to_insert (mono_type_to_load_membase (cfg, info->scalars [i]->inst_vtype)));
		tmp->dreg = dreg;
		tmp->sreg1 = dreg;
		tmp->sreg2 = info->scalars [i]->dreg;
		tmp->inst_offset = info->fields [i].offset - sizeof (MonoObject);
		tmp->klass = info->var->klass;
		tmp->inst_imm = i;
		MONO_ADD_INS (cfg->cbb, tmp);
		if (cfg->verbose_level > 2) {
			printf ("\tAdd: ");
			mono_print_ins (tmp);
		}
	}
}

/*
 * mono_scalar_repl:
 *
 *   Scalar replacement of aggregates.
 */
void
mono_scalar_repl (MonoCompile *cfg)
{
	MonoBasicBlock *bb, *first_bb;
	MonoInst *ins;
	ScalarReplVarInfo *info, *src_info;
	GHashTable *vars;
	int i;

	if (!COMPILE_LLVM (cfg))
		/* Keep this llvm only for now */
		return;

	vars = g_hash_table_new (NULL, NULL);

	/**
	 * Create a dummy bblock and emit code into it so we can use the normal 
	 * code generation macros.
	 */
	cfg->cbb = (MonoBasicBlock *)mono_mempool_alloc0 ((cfg)->mempool, sizeof (MonoBasicBlock));
	first_bb = cfg->cbb;

	for (bb = cfg->bb_entry; bb; bb = bb->next_bb) {
		MonoInst *prev = NULL, *n = NULL;

		MONO_BB_FOR_EACH_INS_SAFE (bb, n, ins) {
			switch (ins->opcode) {
			case OP_EXTRACTI4:
			case OP_EXTRACTI: {
				info = get_scalar_repl_var (cfg, vars, ins->sreg1);
				if (!info)
					break;
				if (cfg->verbose_level > 2) {
					printf ("Processing: ");
					mono_print_ins (ins);
				}
				int findex = ins->inst_imm;
				g_assert (findex != -1);
				// FIXME: Check the load size matches
				ins->opcode = OP_MOVE;
				ins->sreg1 = info->scalars [findex]->dreg;
				if (cfg->verbose_level > 2) {
					printf ("\tTransform to: ");
					mono_print_ins (ins);
				}
				break;
			}
			case OP_VZERO: {
				info = get_scalar_repl_var (cfg, vars, ins->dreg);
				if (!info)
					break;
				for (i = 0; i < info->fcount; ++i)
					mini_emit_init_rvar (cfg, info->scalars [i]->dreg, info->scalars [i]->inst_vtype);
				break;
			}
			case OP_VMOVE: {
				ScalarReplVarInfo *dst_info = get_scalar_repl_var (cfg, vars, ins->dreg);
				src_info = get_scalar_repl_var (cfg, vars, ins->sreg1);
				if (!dst_info && !src_info)
					break;
				if (cfg->verbose_level > 2) {
					printf ("Processing: ");
					mono_print_ins (ins);
				}
				if (dst_info && !src_info) {
					/* Transform into a series of EXTRACT ops */
					emit_vtype_to_scalar (cfg, dst_info, ins->sreg1);
				} else if (src_info && !dst_info) {
					emit_scalar_to_vtype (cfg, src_info, ins->dreg);
				} else {
					info = src_info;
					/* Transform into moves */
					for (i = 0; i < info->fcount; ++i) {
						MonoInst *tmp;

						MONO_INST_NEW (cfg, tmp, OP_MOVE);
						tmp->dreg = dst_info->scalars [i]->dreg;
						tmp->sreg1 = src_info->scalars [i]->dreg;
						MONO_ADD_INS (cfg->cbb, tmp);
						if (cfg->verbose_level > 2) {
							printf ("\tAdd: ");
							mono_print_ins (tmp);
						}
					}
				}
				break;
			}
			case OP_VCALL: {
				info = get_scalar_repl_var (cfg, vars, ins->dreg);
				if (!info)
					break;

				if (cfg->verbose_level > 2) {
					printf ("Processing: ");
					mono_print_ins (ins);
				}

				/*
				 * We don't have a version of VCALL which can handle scalarrepl so make the
				 * call use a new temp, and decompose the temp after the call.
				 * This is slower, but calls are not perf sensitive with Span<T>.
				 */
				MonoInst *tmp_var = mono_compile_create_var (cfg, info->var->inst_vtype, OP_LOCAL);
				ins->dreg = tmp_var->dreg;

				if (cfg->verbose_level > 2) {
					printf ("Changed to: ");
					mono_print_ins (ins);
				}

				/*
				 * Keep the original ins by making a dummy copy, and setting ins to it, so
				 * the mono_replace_ins () call operates on it below.
				 */
				MonoInst *dummy_ins;
				MONO_INST_NEW (cfg, dummy_ins, OP_NOP);
				memcpy (dummy_ins, ins, sizeof (MonoInst));
				MONO_ADD_INS (cfg->cbb, ins);
				ins = dummy_ins;

				/* Decompose from the temp */
				emit_vtype_to_scalar (cfg, info, tmp_var->dreg);
				break;
			}
			case OP_OUTARG_VT:
				info = get_scalar_repl_var (cfg, vars, ins->sreg1);
				if (!info)
					break;

				if (cfg->verbose_level > 2) {
					printf ("Processing: ");
					mono_print_ins (ins);
				}

				/*
				 * Make a temp, store the scalars into it, and pass the temp to the call.
				 */
				MonoInst *tmp_var = mono_compile_create_var (cfg, info->var->inst_vtype, OP_LOCAL);

				emit_scalar_to_vtype (cfg, info, tmp_var->dreg);

				MonoInst *tmp_ins;
				MONO_INST_NEW (cfg, tmp_ins, OP_OUTARG_VT);
				memcpy (tmp_ins, ins, sizeof (MonoInst));
				tmp_ins->sreg1 = tmp_var->dreg;
				MONO_ADD_INS (cfg->cbb, tmp_ins);
				if (cfg->verbose_level > 2) {
					printf ("\tAdd: ");
					mono_print_ins (tmp_ins);
				}
				break;
			default:
				break;
			}

			g_assert (cfg->cbb == first_bb);

			if (cfg->cbb->code || (cfg->cbb != first_bb)) {
				/* Replace the original instruction with the new code sequence */

				mono_replace_ins (cfg, bb, ins, &prev, first_bb, cfg->cbb);
				first_bb->code = first_bb->last_ins = NULL;
				first_bb->in_count = first_bb->out_count = 0;
				cfg->cbb = first_bb;
			} else {
				prev = ins;
			}
		}
	}

	g_free (vars);
}

#else /* !DISABLE_JIT */

MONO_EMPTY_SOURCE_FILE (alias_analysis);

#endif /* !DISABLE_JIT */
