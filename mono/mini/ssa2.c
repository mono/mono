/*
 * ssa.c: Static single assign form support for the JIT compiler.
 *
 * Author:
 *    Dietmar Maurer (dietmar@ximian.com)
 *
 * (C) 2003 Ximian, Inc.
 */
#include <string.h>
#include <mono/metadata/debug-helpers.h>

#include "mini.h"

extern guint8 mono_burg_arity [];

#define USE_ORIGINAL_VARS
#define CREATE_PRUNED_SSA

//#define DEBUG_SSA 1

#define NEW_PHI(cfg,dest,val) do {	\
		(dest) = mono_mempool_alloc0 ((cfg)->mempool, sizeof (MonoInst));	\
		(dest)->opcode = OP_PHI;	\
		(dest)->inst_c0 = (val);	\
        (dest)->dreg = (dest)->sreg1 = (dest)->sreg2 = -1; \
	} while (0)

#define NEW_ICONST(cfg,dest,val) do {	\
		(dest) = mono_mempool_alloc0 ((cfg)->mempool, sizeof (MonoInst));	\
		(dest)->opcode = OP_ICONST;	\
		(dest)->inst_c0 = (val);	\
		(dest)->type = STACK_I4;	\
        (dest)->dreg = (dest)->sreg1 = (dest)->sreg2 = -1; \
	} while (0)


static GList*
g_list_prepend_mempool (GList* l, MonoMemPool* mp, gpointer datum)
{
	GList* n = mono_mempool_alloc (mp, sizeof (GList));
	n->next = l;
	n->prev = NULL;
	n->data = datum;
	return n;
}

static void 
unlink_target (MonoBasicBlock *bb, MonoBasicBlock *target)
{
	int i;

	for (i = 0; i < bb->out_count; i++) {
		if (bb->out_bb [i] == target) {
			bb->out_bb [i] = bb->out_bb [--bb->out_count];
			break;
		}
	}
	for (i = 0; i < target->in_count; i++) {
		if (target->in_bb [i] == bb) {
			target->in_bb [i] = target->in_bb [--target->in_count];
			break;
			
		}
	}
}

static void
unlink_unused_bblocks (MonoCompile *cfg) 
{
	int i, j;
	MonoBasicBlock *bb;

	g_assert (cfg->comp_done & MONO_COMP_REACHABILITY);

	for (bb = cfg->bb_entry; bb && bb->next_bb;) {
		if (!(bb->next_bb->flags & BB_REACHABLE)) {
			bb->next_bb = bb->next_bb->next_bb;
		} else 
			bb = bb->next_bb;
	}

	for (i = 1; i < cfg->num_bblocks; i++) {
		bb = cfg->bblocks [i];
	       
		if (!(bb->flags & BB_REACHABLE)) {
			for (j = 0; j < bb->in_count; j++) {
				unlink_target (bb->in_bb [j], bb);	
			}
			for (j = 0; j < bb->out_count; j++) {
				unlink_target (bb, bb->out_bb [j]);	
			}
		}
 
	}
}

static void
replace_usage (MonoCompile *cfg, MonoBasicBlock *bb, MonoInst *inst, MonoInst **stack)
{
	int arity;

	if (!inst)
		return;

	arity = mono_burg_arity [inst->opcode];

	if ((inst->ssa_op == MONO_SSA_LOAD || inst->ssa_op == MONO_SSA_ADDRESS_TAKEN) && 
	    (inst->inst_i0->opcode == OP_LOCAL || inst->inst_i0->opcode == OP_ARG)) {
		MonoInst *new_var;
		int idx = inst->inst_i0->inst_c0;
			
		if (stack [idx]) {
			new_var = stack [idx];
		} else {
			new_var = cfg->varinfo [idx];

			if ((new_var->opcode != OP_ARG) && (new_var->opcode != OP_LOCAL)) {
				/* uninitialized variable ? */
				g_warning ("using uninitialized variables %d in BB%d (%s)", idx, bb->block_num,
					   mono_method_full_name (cfg->method, TRUE));
				//g_assert_not_reached ();
			}
		}
#ifdef DEBUG_SSA
		printf ("REPLACE BB%d %d %d\n", bb->block_num, idx, new_var->inst_c0);
#endif
		inst->inst_i0 = new_var;
	} else {

		if (arity) {
			if (inst->ssa_op != MONO_SSA_STORE)
				replace_usage (cfg, bb, inst->inst_left, stack);
			if (arity > 1)
				replace_usage (cfg, bb, inst->inst_right, stack);
		}
	}
}

static int
extends_live (MonoInst *inst)
{
	int arity;

	if (!inst)
		return 0;

	arity = mono_burg_arity [inst->opcode];

	if (inst->ssa_op == MONO_SSA_LOAD && 
	    (inst->inst_i0->opcode == OP_LOCAL || inst->inst_i0->opcode == OP_ARG)) {
		return 1;
	} else {
		if (arity) {
			if (inst->ssa_op != MONO_SSA_STORE)
				if (extends_live (inst->inst_left))
					return 1;
			if (arity > 1)
				if (extends_live (inst->inst_right))
					return 1;
		}
	}

	return 0;
}

static int
replace_usage_new (MonoCompile *cfg, MonoInst *inst, int varnum, MonoInst *rep)
{
	int arity;

	if (!inst)
		return 0;

	arity = mono_burg_arity [inst->opcode];

	if ((inst->ssa_op == MONO_SSA_LOAD) && 
	    (inst->inst_i0->opcode == OP_LOCAL || inst->inst_i0->opcode == OP_ARG) &&
	    inst->inst_i0->inst_c0 == varnum && rep->type == inst->type) {
		*inst = *rep;
		return 1;
	} else {
		if (arity) {
			if (inst->ssa_op != MONO_SSA_STORE)
				if (replace_usage_new (cfg, inst->inst_left, varnum, rep))
					return 1;
			if (arity > 1)
				if (replace_usage_new (cfg, inst->inst_right, varnum, rep))
					return 1;
		}
	}

	return 0;
}

static void
mono_ssa_rename_vars2 (MonoCompile *cfg, int max_vars, MonoBasicBlock *bb, gboolean *originals_used, MonoInst **stack) 
{
	MonoInst *ins, *new_var;
	int i, j, idx;
	GList *tmp;
	MonoInst **new_stack;

	/* FIXME: Need to rename local vregs as well */

	if (cfg->verbose_level >= 4)
		printf ("\nRENAME VARS BLOCK %d:\n", bb->block_num);

	/* First pass: Create new vars */
	for (ins = bb->code; ins; ins = ins->next) {
		const char *spec = ins_info [ins->opcode - OP_START - 1];

#ifdef DEBUG_SSA
		printf ("\tProcessing "); mono_print_ins (ins);
#endif
		if (ins->opcode == OP_NOP)
			continue;

		/* SREG1 */
		if (spec [MONO_INST_SRC1] == 'i') {
			MonoInst *var = get_vreg_to_inst (cfg, ins->sreg1);
			if (var && !(var->flags & (MONO_INST_VOLATILE|MONO_INST_INDIRECT))) {
				int idx = var->inst_c0;
				if (stack [idx]) {
					if (var->opcode != OP_ARG)
						g_assert (stack [idx]);
					ins->sreg1 = stack [idx]->dreg;
				}
			}
		}					

		/* SREG2 */
		if (spec [MONO_INST_SRC2] == 'i') {
			MonoInst *var = get_vreg_to_inst (cfg, ins->sreg2);
			if (var && !(var->flags & (MONO_INST_VOLATILE|MONO_INST_INDIRECT))) {
				int idx = var->inst_c0;
				if (stack [idx]) {
					if (var->opcode != OP_ARG)
						g_assert (stack [idx]);

					ins->sreg2 = stack [idx]->dreg;
				}
			}
		}

		if (MONO_IS_STORE_MEMBASE (ins)) {
			MonoInst *var = get_vreg_to_inst (cfg, ins->dreg);
			if (var && !(var->flags & (MONO_INST_VOLATILE|MONO_INST_INDIRECT))) {
				int idx = var->inst_c0;
				if (stack [idx]) {
					if (var->opcode != OP_ARG)
						g_assert (stack [idx]);
					ins->dreg = stack [idx]->dreg;
				}
			}
		}

		/* DREG */
		if ((spec [MONO_INST_DEST] == 'i') && !MONO_IS_STORE_MEMBASE (ins)) {
			MonoInst *var = get_vreg_to_inst (cfg, ins->dreg);
			if (var && !(var->flags & (MONO_INST_VOLATILE|MONO_INST_INDIRECT))) {
				idx = var->inst_c0;
				g_assert (idx < max_vars);

				if (var->opcode == OP_ARG)
					originals_used [idx] = TRUE;

				if (originals_used [idx]) {
					new_var = mono_compile_create_var (cfg, var->inst_vtype,  var->opcode);
					new_var->flags = var->flags;

					if (cfg->verbose_level >= 4)
						printf ("  R%d -> R%d\n", var->dreg, new_var->dreg);

					stack [idx] = new_var;

					ins->dreg = new_var->dreg;
				}
				else {
					/* FIXME: This actually leads to worse final code */
					stack [idx] = var;
					originals_used [idx] = TRUE;
				}
			}
		}

#ifdef DEBUG_SSA
		printf ("\tAfter processing "); mono_print_ins (ins);
#endif

	}

	/* Rename PHI arguments in succeeding bblocks */
	for (i = 0; i < bb->out_count; i++) {
		MonoBasicBlock *n = bb->out_bb [i];

		for (j = 0; j < n->in_count; j++)
			if (n->in_bb [j] == bb)
				break;
		
		for (ins = n->code; ins; ins = ins->next) {
			if (ins->opcode == OP_PHI) {
				idx = ins->inst_c0;
				if (stack [idx])
					new_var = stack [idx];
				else
					new_var = cfg->varinfo [idx];
#ifdef DEBUG_SSA
				printf ("FOUND PHI %d (%d, %d)\n", idx, j, new_var->inst_c0);
#endif
				ins->inst_phi_args [j + 1] = new_var->dreg;
				
				if (G_UNLIKELY (cfg->verbose_level >= 4))
					printf ("\tAdd PHI R%d <- R%d to BB%d\n", ins->dreg, new_var->dreg, n->block_num);

			}
		}
	}

	if (bb->dominated) {
		new_stack = g_new (MonoInst*, max_vars);
		for (tmp = bb->dominated; tmp; tmp = tmp->next) {
			memcpy (new_stack, stack, sizeof (MonoInst *) * max_vars); 
			mono_ssa_rename_vars2 (cfg, max_vars, (MonoBasicBlock *)tmp->data, originals_used, new_stack);
		}
		g_free (new_stack);
	}

}

void
mono_ssa_compute2 (MonoCompile *cfg)
{
	int i, j, idx, bitsize;
	MonoBitSet *set;
	MonoMethodVar *vinfo = g_new0 (MonoMethodVar, cfg->num_varinfo);
	MonoInst *ins, *store, **stack;
	guint8 *buf, *buf_start;

	g_assert (!(cfg->comp_done & MONO_COMP_SSA));

	/* we dont support methods containing exception clauses */
	g_assert (mono_method_get_header (cfg->method)->num_clauses == 0);
	g_assert (!cfg->disable_ssa);

	if (cfg->verbose_level >= 4)
		printf ("\nCOMPUTE SSA %s %d (R%d-)\n\n", mono_method_full_name (cfg->method, TRUE), cfg->num_varinfo, cfg->next_vireg);

#ifdef CREATE_PRUNED_SSA
	/* we need liveness for pruned SSA */
	if (!cfg->new_ir && !(cfg->comp_done & MONO_COMP_LIVENESS))
		mono_analyze_liveness (cfg);
#endif

	mono_compile_dominator_info (cfg, MONO_COMP_DOM | MONO_COMP_IDOM | MONO_COMP_DFRONTIER);

	bitsize = mono_bitset_alloc_size (cfg->num_bblocks, 0);
	buf = buf_start = g_malloc0 (mono_bitset_alloc_size (cfg->num_bblocks, 0) * cfg->num_varinfo);

	for (i = 0; i < cfg->num_varinfo; ++i) {
		vinfo [i].def_in = mono_bitset_mem_new (buf, cfg->num_bblocks, 0);
		buf += bitsize;
		vinfo [i].idx = i;
		/* implicit reference at start */
		mono_bitset_set_fast (vinfo [i].def_in, 0);
	}

	for (i = 0; i < cfg->num_bblocks; ++i) {
		for (ins = cfg->bblocks [i]->code; ins; ins = ins->next) {
			const char *spec = ins_info [ins->opcode - OP_START - 1];

			if (ins->opcode == OP_NOP)
				continue;

			/* FIXME: Handle OP_LDADDR */
			/* FIXME: Handle non-ints as well */
			if ((spec [MONO_INST_DEST] == 'i') && !MONO_IS_STORE_MEMBASE (ins) && get_vreg_to_inst (cfg, ins->dreg)) {
				mono_bitset_set_fast (vinfo [get_vreg_to_inst (cfg, ins->dreg)->inst_c0].def_in, i);
			}
		}
	}

	/* insert phi functions */
	for (i = 0; i < cfg->num_varinfo; ++i) {
		MonoInst *var = cfg->varinfo [i];

#if SIZEOF_VOID_P == 8
		if ((var->type != STACK_I4) && (var->type != STACK_PTR) && (var->type != STACK_OBJ) && (var->type != STACK_MP) && (var->type != STACK_I8))
			continue;
#else
		if ((var->type != STACK_I4) && (var->type != STACK_PTR) && (var->type != STACK_OBJ) && (var->type != STACK_MP))
			continue;
#endif

		if (var->flags & (MONO_INST_VOLATILE|MONO_INST_INDIRECT))
			continue;

		/* Most variables have only one definition */
		if (mono_bitset_count (vinfo [i].def_in) <= 1)
			continue;

		set = mono_compile_iterated_dfrontier (cfg, vinfo [i].def_in);
		vinfo [i].dfrontier = set;

		if (cfg->verbose_level >= 4) {
			if (mono_bitset_count (set) > 0) {
				printf ("\tR%d needs PHI functions in ", var->dreg);
				mono_blockset_print (cfg, set, "", -1);
			}
		}
			
		mono_bitset_foreach_bit (set, idx, cfg->num_bblocks) {
			MonoBasicBlock *bb = cfg->bblocks [idx];

			/* fixme: create pruned SSA? we would need liveness information for that */

			if (bb == cfg->bb_exit)
				continue;

			if ((cfg->comp_done & MONO_COMP_LIVENESS) && !mono_bitset_test_fast (bb->live_in_set, i)) {
				//printf ("%d is not live in BB%d %s\n", i, bb->block_num, mono_method_full_name (cfg->method, TRUE));
				continue;
			}

			/* FIXME: Might need type specific variants */
			NEW_PHI (cfg, ins, i);

			ins->inst_phi_args =  mono_mempool_alloc0 (cfg->mempool, sizeof (int) * (cfg->bblocks [idx]->in_count + 1));
			ins->inst_phi_args [0] = cfg->bblocks [idx]->in_count;

			/* For debugging */
			for (j = 0; j < cfg->bblocks [idx]->in_count; ++j)
				ins->inst_phi_args [j + 1] = -1;

			if (cfg->new_ir) {
				ins->dreg = cfg->varinfo [i]->dreg;

				ins->next = bb->code;
				bb->code = ins;
				if (!bb->last_ins)
					bb->last_ins = bb->code;
			} else {
				store = mono_mempool_alloc0 ((cfg)->mempool, sizeof (MonoInst));
				if (!cfg->varinfo [i]->inst_vtype->type)
					g_assert_not_reached ();
				store->opcode = mono_type_to_stind (cfg->varinfo [i]->inst_vtype);
				store->ssa_op = MONO_SSA_STORE;
				store->inst_i0 = cfg->varinfo [i];
				store->inst_i1 = ins;
				store->klass = store->inst_i0->klass;
	     
				store->next = bb->code;
				bb->code = store;
				if (!bb->last_ins)
					bb->last_ins = bb->code;
			}

#ifdef DEBUG_SSA
			printf ("ADD PHI BB%d %s\n", cfg->bblocks [idx]->block_num, mono_method_full_name (cfg->method, TRUE));
#endif
		}
	}

	/* free the stuff */
	g_free (vinfo);
	g_free (buf_start);

	stack = alloca (sizeof (MonoInst *) * cfg->num_varinfo);
		
	for (i = 0; i < cfg->num_varinfo; i++)
		stack [i] = NULL;

	{
		gboolean *originals = g_new0 (gboolean, cfg->num_varinfo);
		mono_ssa_rename_vars2 (cfg, cfg->num_varinfo, cfg->bb_entry, originals, stack);
		g_free (originals);
	}

	if (cfg->verbose_level >= 4)
		printf ("\nEND COMPUTE SSA.\n\n");

	cfg->comp_done |= MONO_COMP_SSA;
}

void
mono_ssa_remove2 (MonoCompile *cfg)
{
	MonoInst *ins, *var, *move;
	int i, j;

	g_assert (cfg->comp_done & MONO_COMP_SSA);

	for (i = 0; i < cfg->num_bblocks; ++i) {
		MonoBasicBlock *bb = cfg->bblocks [i];

		if (cfg->verbose_level >= 4)
			printf ("\nREMOVE SSA %d:\n", bb->block_num);

		for (ins = bb->code; ins; ins = ins->next) {
			if (ins->opcode == OP_PHI) {
				g_assert (ins->inst_phi_args [0] == bb->in_count);
				var = get_vreg_to_inst (cfg, ins->dreg);

				for (j = 0; j < bb->in_count; j++) {
					MonoBasicBlock *pred = bb->in_bb [j];
					int sreg = ins->inst_phi_args [j + 1];

					/* FIXME: Add back optimizations */
					if (cfg->verbose_level >= 4)
						printf ("\tADD R%d <- R%d in BB%d\n", var->dreg, sreg, pred->block_num);
						MONO_INST_NEW (cfg, move, OP_MOVE);
						move->dreg = var->dreg;
						move->sreg1 = sreg;
						mono_add_ins_to_end (pred, move);
				}

				/* remove the phi functions */
				ins->opcode = OP_NOP;
				ins->dreg = -1;
			}
		}

		if (cfg->verbose_level >= 4)
			mono_print_bb (bb, "AFTER REMOVE SSA:");
	}
}

#ifndef USE_ORIGINAL_VARS
static GPtrArray *
mono_ssa_get_allocatable_vars (MonoCompile *cfg)
{
	GHashTable *type_hash;
	GPtrArray *varlist_array = g_ptr_array_new ();
	int tidx, i;

	g_assert (cfg->comp_done & MONO_COMP_LIVENESS);

	type_hash = g_hash_table_new (NULL, NULL);

	for (i = 0; i < cfg->num_varinfo; i++) {
		MonoInst *ins = cfg->varinfo [i];
		MonoMethodVar *vmv = MONO_VARINFO (cfg, i);

		/* unused vars */
		if (vmv->range.first_use.abs_pos > vmv->range.last_use.abs_pos)
			continue;

		if (ins->flags & ((MONO_INST_VOLATILE|MONO_INST_INDIRECT)|MONO_INST_INDIRECT) || 
		    (ins->opcode != OP_LOCAL && ins->opcode != OP_ARG) || vmv->reg != -1)
			continue;

		g_assert (ins->inst_vtype);
		g_assert (vmv->reg == -1);
		g_assert (i == vmv->idx);

		if (!(tidx = (int)g_hash_table_lookup (type_hash, ins->inst_vtype))) {
			GList *vars = g_list_append (NULL, vmv);
			g_ptr_array_add (varlist_array, vars);
			g_hash_table_insert (type_hash, ins->inst_vtype, (gpointer)varlist_array->len);
		} else {
			tidx--;
			g_ptr_array_index (varlist_array, tidx) =
				mono_varlist_insert_sorted (cfg, g_ptr_array_index (varlist_array, tidx), vmv, FALSE);
		}
	}

	g_hash_table_destroy (type_hash);

	return varlist_array;
}
#endif

static void
mono_ssa_replace_copies (MonoCompile *cfg, MonoBasicBlock *bb, MonoInst *inst, char *is_live)
{
	int arity;

	if (!inst)
		return;

	arity = mono_burg_arity [inst->opcode];

	if ((inst->ssa_op == MONO_SSA_LOAD || inst->ssa_op == MONO_SSA_ADDRESS_TAKEN || inst->ssa_op == MONO_SSA_STORE) && 
	    (inst->inst_i0->opcode == OP_LOCAL || inst->inst_i0->opcode == OP_ARG)) {
		MonoInst *new_var;
		int idx = inst->inst_i0->inst_c0;
		MonoMethodVar *mv = cfg->vars [idx];

		if (mv->reg != -1 && mv->reg != mv->idx) {
		       
			is_live [mv->reg] = 1;

			new_var = cfg->varinfo [mv->reg];

#if 0
			printf ("REPLACE COPY BB%d %d %d\n", bb->block_num, idx, new_var->inst_c0);
			g_assert (cfg->varinfo [mv->reg]->inst_vtype == cfg->varinfo [idx]->inst_vtype);
#endif
			inst->inst_i0 = new_var;
		} else {
			is_live [mv->idx] = 1;
		}
	}


	if (arity) {
		mono_ssa_replace_copies (cfg, bb, inst->inst_left, is_live);
		if (arity > 1)
			mono_ssa_replace_copies (cfg, bb, inst->inst_right, is_live);
	}

	if (inst->ssa_op == MONO_SSA_STORE && inst->inst_i1->ssa_op == MONO_SSA_LOAD &&
	    inst->inst_i0->inst_c0 == inst->inst_i1->inst_i0->inst_c0) {
		inst->ssa_op = MONO_SSA_NOP;
		inst->opcode = CEE_NOP;
	}

}

#define IS_CALL(op) (op == CEE_CALLI || op == CEE_CALL || op == CEE_CALLVIRT || (op >= OP_VOIDCALL && op <= OP_CALL_MEMBASE))

typedef struct {
	MonoBasicBlock *bb;
	MonoInst *inst;
} MonoVarUsageInfo;

static void
analyze_dev_use (MonoCompile *cfg, MonoBasicBlock *bb, MonoInst *root, MonoInst *inst)
{
	MonoMethodVar *info;
	int i, idx, arity;

	if (!inst)
		return;

	arity = mono_burg_arity [inst->opcode];

	if ((inst->ssa_op == MONO_SSA_STORE) && 
	    (inst->inst_i0->opcode == OP_LOCAL /*|| inst->inst_i0->opcode == OP_ARG */)) {
		idx = inst->inst_i0->inst_c0;
		info = cfg->vars [idx];
		//printf ("%d defined in BB%d %p\n", idx, bb->block_num, root);
		if (info->def) {
			g_warning ("more than one definition of variable %d in %s", idx,
				   mono_method_full_name (cfg->method, TRUE));
			g_assert_not_reached ();
		}
		if (!IS_CALL (inst->inst_i1->opcode) /* && inst->inst_i1->opcode == OP_ICONST */) {
			g_assert (inst == root);
			info->def = root;
			info->def_bb = bb;
		}

		if (inst->inst_i1->opcode == OP_PHI) {
			for (i = inst->inst_i1->inst_phi_args [0]; i > 0; i--) {
				MonoVarUsageInfo *ui = mono_mempool_alloc (cfg->mempool, sizeof (MonoVarUsageInfo));
				idx = inst->inst_i1->inst_phi_args [i];	
				info = cfg->vars [idx];
				//printf ("FOUND %d\n", idx);
				ui->bb = bb;
				ui->inst = root;
				info->uses = g_list_prepend_mempool (info->uses, cfg->mempool, ui);
			}
		}
	}

	if ((inst->ssa_op == MONO_SSA_LOAD || inst->ssa_op == MONO_SSA_ADDRESS_TAKEN) && 
	    (inst->inst_i0->opcode == OP_LOCAL || inst->inst_i0->opcode == OP_ARG)) {
		MonoVarUsageInfo *ui = mono_mempool_alloc (cfg->mempool, sizeof (MonoVarUsageInfo));
		idx = inst->inst_i0->inst_c0;	
		info = cfg->vars [idx];
		//printf ("FOUND %d\n", idx);
		ui->bb = bb;
		ui->inst = root;
		info->uses = g_list_prepend_mempool (info->uses, cfg->mempool, ui);
	} else {
		if (arity) {
			//if (inst->ssa_op != MONO_SSA_STORE)
			analyze_dev_use (cfg, bb, root, inst->inst_left);
			if (arity > 1)
				analyze_dev_use (cfg, bb, root, inst->inst_right);
		}
	}
}

static inline void
record_use (MonoCompile *cfg, MonoInst *var, MonoBasicBlock *bb, MonoInst *ins)
{
	MonoMethodVar *info;
	MonoVarUsageInfo *ui = mono_mempool_alloc (cfg->mempool, sizeof (MonoVarUsageInfo));

	info = cfg->vars [var->inst_c0];
	
	ui->bb = bb;
	ui->inst = ins;
	info->uses = g_list_prepend_mempool (info->uses, cfg->mempool, ui);
}	

static void
mono_ssa_create_def_use (MonoCompile *cfg) 
{
	MonoBasicBlock *bb;
	MonoInst *ins;
	int i;

	g_assert (!(cfg->comp_done & MONO_COMP_SSA_DEF_USE));

	/* FIXME: Merge this into compute_ssa */

	for (bb = cfg->bb_entry; bb; bb = bb->next_bb) {
		for (ins = bb->code; ins; ins = ins->next) {
			const char *spec = ins_info [ins->opcode - OP_START - 1];
			MonoMethodVar *info;

			if (ins->opcode == OP_NOP)
				continue;

			/* SREG1 */
			if (spec [MONO_INST_SRC1] == 'i') {
				MonoInst *var = get_vreg_to_inst (cfg, ins->sreg1);
				if (var && !(var->flags & (MONO_INST_VOLATILE|MONO_INST_INDIRECT)))
					record_use (cfg, var, bb, ins);
			}

			/* SREG2 */
			if (spec [MONO_INST_SRC2] == 'i') {
				MonoInst *var = get_vreg_to_inst (cfg, ins->sreg2);
				if (var && !(var->flags & (MONO_INST_VOLATILE|MONO_INST_INDIRECT)))
					record_use (cfg, var, bb, ins);
			}
				
			if (MONO_IS_STORE_MEMBASE (ins)) {
				MonoInst *var = get_vreg_to_inst (cfg, ins->dreg);
				if (var && !(var->flags & (MONO_INST_VOLATILE|MONO_INST_INDIRECT)))
					record_use (cfg, var, bb, ins);
			}

			if (ins->opcode == OP_PHI) {
				for (i = ins->inst_phi_args [0]; i > 0; i--) {
					g_assert (ins->inst_phi_args [i] != -1);
					record_use (cfg,  get_vreg_to_inst (cfg, ins->inst_phi_args [i]), bb, ins);
				}
			}

			/* DREG */
			if ((spec [MONO_INST_DEST] == 'i') && !MONO_IS_STORE_MEMBASE (ins)) {
				MonoInst *var = get_vreg_to_inst (cfg, ins->dreg);

				if (var && !(var->flags & (MONO_INST_VOLATILE|MONO_INST_INDIRECT))) {
					info = cfg->vars [var->inst_c0];
					info->def = ins;
					info->def_bb = bb;
				}
			}
		}
	}

	cfg->comp_done |= MONO_COMP_SSA_DEF_USE;
}

static void
mono_ssa_copyprop (MonoCompile *cfg)
{
	int i, index;
	GList *l;

	g_assert ((cfg->comp_done & MONO_COMP_SSA_DEF_USE));

	for (index = 0; index < cfg->num_varinfo; ++index) {
		MonoInst *var = cfg->varinfo [index];
		MonoMethodVar *info = cfg->vars [index];

		if (info->def && (info->def->opcode == OP_MOVE)) {
			MonoInst *var2 = get_vreg_to_inst (cfg, info->def->sreg1);

			if (var2 && !(var2->flags & (MONO_INST_VOLATILE|MONO_INST_INDIRECT)) && cfg->vars [var2->inst_c0]->def && (cfg->vars [var2->inst_c0]->def->opcode != OP_PHI)) {
				/* Rewrite all uses of var to be uses of var2 */
				int dreg = var->dreg;
				int sreg1 = var2->dreg;
				const char *spec;

				l = info->uses;
				while (l) {
					MonoVarUsageInfo *u = (MonoVarUsageInfo*)l->data;
					MonoInst *ins = u->inst;
					GList *next = l->next;

					spec = ins_info [ins->opcode - OP_START - 1];

					if (spec [MONO_INST_SRC1] == 'i' && ins->sreg1 == dreg) {
						ins->sreg1 = sreg1;
					} else if (spec [MONO_INST_SRC2] == 'i' && ins->sreg2 == dreg) {
						ins->sreg2 = sreg1;
					} else if (MONO_IS_STORE_MEMBASE (ins) && ins->dreg == dreg) {
						ins->dreg = sreg1;
					} else if (ins->opcode == OP_PHI) {
						for (i = ins->inst_phi_args [0]; i > 0; i--) {
							int sreg = ins->inst_phi_args [i];
							if (sreg == var->dreg)
								break;
						}
						g_assert (i > 0);
						ins->inst_phi_args [i] = sreg1;
					}
					else
						g_assert_not_reached ();

					record_use (cfg, var2, u->bb, ins);

					l = next;
				}

				info->uses = NULL;
			}
		}
	}

	if (cfg->verbose_level >= 4) {
		MonoBasicBlock *bb;

		for (bb = cfg->bb_entry; bb; bb = bb->next_bb)
			mono_print_bb (bb, "AFTER SSA COPYPROP");
	}
}

static int
simulate_compare (int opcode, int a, int b)
{
	switch (opcode) {
	case CEE_BEQ:
		return a == b;
	case CEE_BGE:
		return a >= b;
	case CEE_BGT:
		return a > b;
	case CEE_BLE:
		return a <= b;
	case CEE_BLT:
		return a < b;
	case CEE_BNE_UN:
		return a != b;
	case CEE_BGE_UN:
		return (unsigned)a >= (unsigned)b;
	case CEE_BGT_UN:
		return (unsigned)a > (unsigned)b;
	case CEE_BLE_UN:
		return (unsigned)a <= (unsigned)b;
	case CEE_BLT_UN:
		return (unsigned)a < (unsigned)b;
	default:
		g_assert_not_reached ();
	}

	return 0;
}

static int
simulate_long_compare (int opcode, gint64 a, gint64 b)
{
	switch (opcode) {
	case CEE_BEQ:
		return a == b;
	case CEE_BGE:
		return a >= b;
	case CEE_BGT:
		return a > b;
	case CEE_BLE:
		return a <= b;
	case CEE_BLT:
		return a < b;
	case CEE_BNE_UN:
		return a != b;
	case CEE_BGE_UN:
		return (guint64)a >= (guint64)b;
	case CEE_BGT_UN:
		return (guint64)a > (guint64)b;
	case CEE_BLE_UN:
		return (guint64)a <= (guint64)b;
	case CEE_BLT_UN:
		return (guint64)a < (guint64)b;
	default:
		g_assert_not_reached ();
	}

	return 0;
}

#define EVAL_CXX(name,op,cast)	\
	case name:	\
		if ((inst->inst_i0->opcode == OP_COMPARE) || (inst->inst_i0->opcode == OP_LCOMPARE)) { \
			r1 = evaluate_const_tree (cfg, inst->inst_i0->inst_i0, &a, carray); \
			r2 = evaluate_const_tree (cfg, inst->inst_i0->inst_i1, &b, carray); \
			if (r1 == 1 && r2 == 1) { \
				*res = ((cast)a op (cast)b); \
				return 1; \
			} else { \
				return MAX (r1, r2); \
			} \
		} \
		break;

#define EVAL_BINOP(name,op)	\
	case name:	\
		r1 = evaluate_const_tree (cfg, inst->inst_i0, &a, carray); \
		r2 = evaluate_const_tree (cfg, inst->inst_i1, &b, carray); \
		if (r1 == 1 && r2 == 1) { \
			*res = (a op b); \
			return 1; \
		} else { \
			return MAX (r1, r2); \
		} \
		break;


/* fixme: this only works for interger constants, but not for other types (long, float) */
static int
evaluate_const_tree (MonoCompile *cfg, MonoInst *inst, int *res, MonoInst **carray)
{
	MonoInst *c0;
	int a, b, r1, r2;

	if (!inst)
		return 0;

	if (inst->ssa_op == MONO_SSA_LOAD && 
	    (inst->inst_i0->opcode == OP_LOCAL || inst->inst_i0->opcode == OP_ARG) &&
	    (c0 = carray [inst->inst_i0->inst_c0])) {
		*res = c0->inst_c0;
		return 1;
	}

	switch (inst->opcode) {
	case OP_ICONST:
		*res = inst->inst_c0;
		return 1;

	EVAL_CXX (OP_CEQ,==,gint32)
	EVAL_CXX (OP_CGT,>,gint32)
	EVAL_CXX (OP_CGT_UN,>,guint32)
	EVAL_CXX (OP_CLT,<,gint32)
	EVAL_CXX (OP_CLT_UN,<,guint32)

	EVAL_BINOP (CEE_ADD,+)
	EVAL_BINOP (CEE_SUB,-)
	EVAL_BINOP (CEE_MUL,*)
	EVAL_BINOP (CEE_AND,&)
	EVAL_BINOP (CEE_OR,|)
	EVAL_BINOP (CEE_XOR,^)
	EVAL_BINOP (CEE_SHL,<<)
	EVAL_BINOP (CEE_SHR,>>)

	default:
		return 2;
	}

	return 2;
}

static void
fold_tree (MonoCompile *cfg, MonoBasicBlock *bb, MonoInst *inst, MonoInst **carray)
{
	MonoInst *c0;
	int arity, a, b;

	if (!inst)
		return;

	arity = mono_burg_arity [inst->opcode];

	if (inst->ssa_op == MONO_SSA_STORE && 
	    (inst->inst_i0->opcode == OP_LOCAL || inst->inst_i0->opcode == OP_ARG) &&
	    inst->inst_i1->opcode == OP_PHI && (c0 = carray [inst->inst_i0->inst_c0])) {
		//{static int cn = 0; printf ("PHICONST %d %d %s\n", cn++, c0->inst_c0, mono_method_full_name (cfg->method, TRUE));}
		*inst->inst_i1 = *c0;		
	} else if (inst->ssa_op == MONO_SSA_LOAD && 
	    (inst->inst_i0->opcode == OP_LOCAL || inst->inst_i0->opcode == OP_ARG) &&
	    (c0 = carray [inst->inst_i0->inst_c0])) {
		//{static int cn = 0; printf ("YCCOPY %d %d %s\n", cn++, c0->inst_c0, mono_method_full_name (cfg->method, TRUE));}
		*inst = *c0;
	} else {

		if (arity) {
			fold_tree (cfg, bb, inst->inst_left, carray);
			if (arity > 1)
				fold_tree (cfg, bb, inst->inst_right, carray);
			mono_constant_fold_inst (inst, NULL); 
		}
	}

	if ((inst->opcode >= CEE_BEQ && inst->opcode <= CEE_BLT_UN) &&
	    ((inst->inst_i0->opcode == OP_COMPARE) || (inst->inst_i0->opcode == OP_LCOMPARE))) {
		MonoInst *v0 = inst->inst_i0->inst_i0;
		MonoInst *v1 = inst->inst_i0->inst_i1;
		MonoBasicBlock *target = NULL;

		/* hack for longs to optimize the simply cases */
		if (v0->opcode == OP_I8CONST && v1->opcode == OP_I8CONST) {
			if (simulate_long_compare (inst->opcode, v0->inst_l, v1->inst_l)) {
				//unlink_target (bb, inst->inst_false_bb);
				target = inst->inst_true_bb;
			} else {
				//unlink_target (bb, inst->inst_true_bb);
				target = inst->inst_false_bb;
			}			
		} else if (evaluate_const_tree (cfg, v0, &a, carray) == 1 &&
			   evaluate_const_tree (cfg, v1, &b, carray) == 1) {				
			if (simulate_compare (inst->opcode, a, b)) {
				//unlink_target (bb, inst->inst_false_bb);
				target = inst->inst_true_bb;
			} else {
				//unlink_target (bb, inst->inst_true_bb);
				target = inst->inst_false_bb;
			}
		}

		if (target) {
			bb->out_bb [0] = target;
			bb->out_count = 1;
			inst->opcode = CEE_BR;
			inst->inst_target_bb = target;
		}
	} else if (inst->opcode == CEE_SWITCH && (evaluate_const_tree (cfg, inst->inst_left, &a, carray) == 1) && (a >= 0) && (a < GPOINTER_TO_INT (inst->klass))) {
		bb->out_bb [0] = inst->inst_many_bb [a];
		bb->out_count = 1;
		inst->inst_target_bb = bb->out_bb [0];
		inst->opcode = CEE_BR;
	}

}

static void
change_varstate (MonoCompile *cfg, GList **cvars, MonoMethodVar *info, int state, MonoInst *c0, MonoInst **carray)
{
	if (info->cpstate >= state)
		return;

	info->cpstate = state;

	//printf ("SETSTATE %d to %d\n", info->idx, info->cpstate);

	if (state == 1)
		carray [info->idx] = c0;
	else
		carray [info->idx] = NULL;

	if (!g_list_find (*cvars, info)) {
		*cvars = g_list_prepend (*cvars, info);
	}
}

static void
visit_inst (MonoCompile *cfg, MonoBasicBlock *bb, MonoInst *inst, GList **cvars, GList **bblist, MonoInst **carray)
{
	g_assert (inst);

	if (inst->opcode == CEE_SWITCH) {
		int r1, i, a;
		int cases = GPOINTER_TO_INT (inst->klass);

		r1 = evaluate_const_tree (cfg, inst->inst_left, &a, carray);
		if ((r1 == 1) && ((a < 0) || (a >= cases)))
			r1 = 2;
		if (r1 == 1) {
			MonoBasicBlock *tb = inst->inst_many_bb [a];
			if (!(tb->flags &  BB_REACHABLE)) {
				tb->flags |= BB_REACHABLE;
				*bblist = g_list_prepend (*bblist, tb);
			}
		} else if (r1 == 2) {
			for (i = GPOINTER_TO_INT (inst->klass); i >= 0; i--) {
				MonoBasicBlock *tb = inst->inst_many_bb [i];
				if (!(tb->flags &  BB_REACHABLE)) {
					tb->flags |= BB_REACHABLE;
					*bblist = g_list_prepend (*bblist, tb);
				}
			}
		}
	} else if ((inst->opcode >= CEE_BEQ && inst->opcode <= CEE_BLT_UN) &&
	    ((inst->inst_i0->opcode == OP_COMPARE) || (inst->inst_i0->opcode == OP_LCOMPARE))) {
		int a, b, r1, r2;
		MonoInst *v0 = inst->inst_i0->inst_i0;
		MonoInst *v1 = inst->inst_i0->inst_i1;

		r1 = evaluate_const_tree (cfg, v0, &a, carray);
		r2 = evaluate_const_tree (cfg, v1, &b, carray);

		if (r1 == 1 && r2 == 1) {
			MonoBasicBlock *target;
				
			if (simulate_compare (inst->opcode, a, b)) {
				target = inst->inst_true_bb;
			} else {
				target = inst->inst_false_bb;
			}
			if (!(target->flags &  BB_REACHABLE)) {
				target->flags |= BB_REACHABLE;
				*bblist = g_list_prepend (*bblist, target);
			}
		} else if (r1 == 2 || r2 == 2) {
			if (!(inst->inst_true_bb->flags &  BB_REACHABLE)) {
				inst->inst_true_bb->flags |= BB_REACHABLE;
				*bblist = g_list_prepend (*bblist, inst->inst_true_bb);
			}
			if (!(inst->inst_false_bb->flags &  BB_REACHABLE)) {
				inst->inst_false_bb->flags |= BB_REACHABLE;
				*bblist = g_list_prepend (*bblist, inst->inst_false_bb);
			}
		}	
	} else if (inst->ssa_op == MONO_SSA_STORE && 
		   (inst->inst_i0->opcode == OP_LOCAL || inst->inst_i0->opcode == OP_ARG)) {
		MonoMethodVar *info = cfg->vars [inst->inst_i0->inst_c0];
		MonoInst *i1 = inst->inst_i1;
		int res;
		
		if (info->cpstate < 2) {
			if (i1->opcode == OP_ICONST) { 
				change_varstate (cfg, cvars, info, 1, i1, carray);
			} else if (i1->opcode == OP_PHI) {
				MonoInst *c0 = NULL;
				int j;

				for (j = 1; j <= i1->inst_phi_args [0]; j++) {
					MonoMethodVar *mv = cfg->vars [i1->inst_phi_args [j]];
					MonoInst *src = mv->def;

					if (mv->def_bb && !(mv->def_bb->flags & BB_REACHABLE)) {
						continue;
					}

					if (!mv->def || !src || src->ssa_op != MONO_SSA_STORE ||
					    !(src->inst_i0->opcode == OP_LOCAL || src->inst_i0->opcode == OP_ARG) ||
					    mv->cpstate == 2) {
						change_varstate (cfg, cvars, info, 2, NULL, carray);
						break;
					}
					
					if (mv->cpstate == 0)
						continue;

					//g_assert (src->inst_i1->opcode == OP_ICONST);
					g_assert (carray [mv->idx]);

					if (!c0) {
						c0 = carray [mv->idx];
					}
					
					if (carray [mv->idx]->inst_c0 != c0->inst_c0) {
						change_varstate (cfg, cvars, info, 2, NULL, carray);
						break;
					}
				}
				
				if (c0 && info->cpstate < 1) {
					change_varstate (cfg, cvars, info, 1, c0, carray);
				}
			} else {
				int state = evaluate_const_tree (cfg, i1, &res, carray);
				if (state == 1) {
					NEW_ICONST (cfg, i1, res);
					change_varstate (cfg, cvars, info, 1, i1, carray);
				} else {
					change_varstate (cfg, cvars, info, 2, NULL, carray);
				}
			}
		}
	}
}

void
mono_ssa_cprop2 (MonoCompile *cfg) 
{
	MonoInst **carray;
	MonoBasicBlock *bb;
	GList *bblock_list, *cvars;
	GList *tmp;
	int i;
	//printf ("SIMPLE OPTS BB%d %s\n", bb->block_num, mono_method_full_name (cfg->method, TRUE));

	carray = g_new0 (MonoInst*, cfg->num_varinfo);

	if (!(cfg->comp_done & MONO_COMP_SSA_DEF_USE))
		mono_ssa_create_def_use (cfg);

	bblock_list = g_list_prepend (NULL, cfg->bb_entry);
	cfg->bb_entry->flags |= BB_REACHABLE;

	memset (carray, 0, sizeof (MonoInst *) * cfg->num_varinfo);

	for (i = 0; i < cfg->num_varinfo; i++) {
		MonoMethodVar *info = cfg->vars [i];
		if (!info->def)
			info->cpstate = 2;
	}

	cvars = NULL;

	while (bblock_list) {
		MonoInst *inst;

		bb = (MonoBasicBlock *)bblock_list->data;

		bblock_list = g_list_delete_link (bblock_list, bblock_list);

		g_assert (bb->flags &  BB_REACHABLE);

		if (bb->out_count == 1) {
			if (!(bb->out_bb [0]->flags &  BB_REACHABLE)) {
				bb->out_bb [0]->flags |= BB_REACHABLE;
				bblock_list = g_list_prepend (bblock_list, bb->out_bb [0]);
			}
		}

		for (inst = bb->code; inst; inst = inst->next) {
			visit_inst (cfg, bb, inst, &cvars, &bblock_list, carray);
		}

		while (cvars) {
			MonoMethodVar *info = (MonoMethodVar *)cvars->data;			
			cvars = g_list_delete_link (cvars, cvars);

			for (tmp = info->uses; tmp; tmp = tmp->next) {
				MonoVarUsageInfo *ui = (MonoVarUsageInfo *)tmp->data;
				if (!(ui->bb->flags & BB_REACHABLE))
					continue;
				visit_inst (cfg, ui->bb, ui->inst, &cvars, &bblock_list, carray);
			}
		}
	}

	for (bb = cfg->bb_entry->next_bb; bb; bb = bb->next_bb) {
		MonoInst *inst;
		for (inst = bb->code; inst; inst = inst->next) {
			fold_tree (cfg, bb, inst, carray);
		}
	}

	g_free (carray);

	cfg->comp_done |= MONO_COMP_REACHABILITY;

	/* fixme: we should update usage infos during cprop, instead of computing it again */
	cfg->comp_done &=  ~MONO_COMP_SSA_DEF_USE;
	for (i = 0; i < cfg->num_varinfo; i++) {
		MonoMethodVar *info = cfg->vars [i];
		info->def = NULL;
		info->uses = NULL;
	}
}

static void
add_to_dce_worklist (MonoCompile *cfg, MonoMethodVar *var, MonoMethodVar *use, GList **wl)
{
	GList *tmp;

	*wl = g_list_prepend (*wl, use);

	for (tmp = use->uses; tmp; tmp = tmp->next) {
		MonoVarUsageInfo *ui = (MonoVarUsageInfo *)tmp->data;
		if (ui->inst == var->def) {
			/* from the mempool */
			use->uses = g_list_remove_link (use->uses, tmp);
			break;
		}
	}	
}

void
mono_ssa_deadce2 (MonoCompile *cfg) 
{
	int i;
	GList *work_list;

	g_assert (cfg->comp_done & MONO_COMP_SSA);

	//printf ("DEADCE %s\n", mono_method_full_name (cfg->method, TRUE));

	if (!(cfg->comp_done & MONO_COMP_SSA_DEF_USE))
		mono_ssa_create_def_use (cfg);

	mono_ssa_copyprop (cfg);

	work_list = NULL;
	for (i = 0; i < cfg->num_varinfo; i++) {
		MonoMethodVar *info = cfg->vars [i];
		work_list = g_list_prepend (work_list, info);
	}

	while (work_list) {
		MonoMethodVar *info = (MonoMethodVar *)work_list->data;
		work_list = g_list_delete_link (work_list, work_list);

		if (!info->uses && info->def) {
			MonoInst *def = info->def;

			/* FIXME: Add more opcodes */
			if (def->opcode == OP_MOVE) {
				MonoInst *src_var = get_vreg_to_inst (cfg, def->sreg1);
				if (src_var && !(src_var->flags & (MONO_INST_VOLATILE|MONO_INST_INDIRECT)))
					add_to_dce_worklist (cfg, info, cfg->vars [src_var->inst_c0], &work_list);
				def->opcode = OP_NOP;
				def->dreg = def->sreg1 = def->sreg2 = -1;
			} else if ((def->opcode == OP_ICONST) || (def->opcode == OP_I8CONST)) {
				def->opcode = OP_NOP;
				def->dreg = def->sreg1 = def->sreg2 = -1;
			} else if (def->opcode == OP_PHI) {
				int j;
				for (j = def->inst_phi_args [0]; j > 0; j--) {
					MonoMethodVar *u = cfg->vars [get_vreg_to_inst (cfg, def->inst_phi_args [j])->inst_c0];
					add_to_dce_worklist (cfg, info, u, &work_list);
				}
				def->opcode = OP_NOP;
				def->dreg = def->sreg1 = def->sreg2 = -1;
			}
			else if (def->opcode == OP_NOP) {
			}
			//else
			//mono_print_ins (def);
		}

	}
}

#if 0
void
mono_ssa_strength_reduction (MonoCompile *cfg)
{
	MonoBasicBlock *bb;
	int i;

	g_assert (cfg->comp_done & MONO_COMP_SSA);
	g_assert (cfg->comp_done & MONO_COMP_LOOPS);
	g_assert (cfg->comp_done & MONO_COMP_SSA_DEF_USE);

	for (bb = cfg->bb_entry->next_bb; bb; bb = bb->next_bb) {
		GList *lp = bb->loop_blocks;

		if (lp) {
			MonoBasicBlock *h = (MonoBasicBlock *)lp->data;

			/* we only consider loops with 2 in bblocks */
			if (!h->in_count == 2)
				continue;

			for (i = 0; i < cfg->num_varinfo; i++) {
				MonoMethodVar *info = cfg->vars [i];
			
				if (info->def && info->def->ssa_op == MONO_SSA_STORE &&
				    info->def->inst_i0->opcode == OP_LOCAL && g_list_find (lp, info->def_bb)) {
					MonoInst *v = info->def->inst_i1;


					printf ("FOUND %d in %s\n", info->idx, mono_method_full_name (cfg->method, TRUE));
				}
			}
		}
	}
}
#endif
