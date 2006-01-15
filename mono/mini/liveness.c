/*
 * liveness.c: liveness analysis
 *
 * Author:
 *   Dietmar Maurer (dietmar@ximian.com)
 *
 * (C) 2002 Ximian, Inc.
 */

#include "mini.h"
#include "inssel.h"
#include "aliasing.h"

//#define DEBUG_LIVENESS

extern guint8 mono_burg_arity [];

/* mono_bitset_mp_new:
 * 
 * allocates a MonoBitSet inside a memory pool
 */
static MonoBitSet* 
mono_bitset_mp_new (MonoMemPool *mp,  guint32 max_size)
{
	int size = mono_bitset_alloc_size (max_size, 0);
	gpointer mem;

	mem = mono_mempool_alloc0 (mp, size);
	return mono_bitset_mem_new (mem, max_size, MONO_BITSET_DONT_FREE);
}

G_GNUC_UNUSED static void
mono_bitset_print (MonoBitSet *set)
{
	int i;

	printf ("{");
	for (i = 0; i < mono_bitset_size (set); i++) {

		if (mono_bitset_test (set, i))
			printf ("%d, ", i);

	}
	printf ("}\n");
}

static inline void
update_live_range (MonoCompile *cfg, int idx, int block_dfn, int tree_pos)
{
	MonoLiveRange *range = &MONO_VARINFO (cfg, idx)->range;
	guint32 abs_pos = (block_dfn << 16) | tree_pos;

	if (range->first_use.abs_pos > abs_pos)
		range->first_use.abs_pos = abs_pos;

	if (range->last_use.abs_pos < abs_pos)
		range->last_use.abs_pos = abs_pos;
}

static void
update_gen_kill_set (MonoCompile *cfg, MonoBasicBlock *bb, MonoInst *inst, int inst_num)
{
	int arity;
	int max_vars = cfg->num_varinfo;

	if (cfg->new_ir) {
		MonoInst *ins = inst;
		const char *spec = ins_info [ins->opcode - OP_START - 1];
		int regtype, srcindex, sreg;
		gboolean store;

#ifdef DEBUG_LIVENESS
			printf ("\t"); mono_print_ins (ins);
#endif

		if (ins->opcode < MONO_CEE_LAST)
			return;

		switch (ins->opcode) {
		case OP_STORE:       
		case OP_STORE_MEMBASE_REG:
		case OP_STOREI1_MEMBASE_REG: 
		case OP_STOREI2_MEMBASE_REG: 
		case OP_STOREI4_MEMBASE_REG: 
		case OP_STOREI8_MEMBASE_REG:
		case OP_STORER4_MEMBASE_REG: 
		case OP_STORER8_MEMBASE_REG:
		case OP_STORE_MEMBASE_IMM:
		case OP_STOREI1_MEMBASE_IMM: 
		case OP_STOREI2_MEMBASE_IMM: 
		case OP_STOREI4_MEMBASE_IMM: 
		case OP_STOREI8_MEMBASE_IMM: 
			store = TRUE;
			break;
		case OP_STORE_MEMINDEX:
		case OP_STOREI1_MEMINDEX:
		case OP_STOREI2_MEMINDEX:
		case OP_STOREI4_MEMINDEX:
		case OP_STOREI8_MEMINDEX:
		case OP_STORER4_MEMINDEX:
		case OP_STORER8_MEMINDEX:
			g_assert_not_reached ();
		default:
			store = FALSE;
		}

		/* SREGS */
		/* These must come first, so MOVE r <- r is handled correctly */
		for (srcindex = 0; srcindex < 2; ++srcindex) {
			regtype = spec [(srcindex == 0) ? MONO_INST_SRC1 : MONO_INST_SRC2];
			sreg = srcindex == 0 ? ins->sreg1 : ins->sreg2;

			g_assert (((sreg == -1) && (regtype == ' ')) || ((sreg != -1) && (regtype != ' ')));
			if ((sreg != -1) && get_vreg_to_inst (cfg, regtype, sreg)) {
				MonoInst *var = cfg->vreg_to_inst [regtype][sreg];
				int idx = var->inst_c0;
				MonoMethodVar *vi = MONO_VARINFO (cfg, idx);

#ifdef DEBUG_LIVENESS
				printf ("\tGEN: R%d(%d)\n", sreg, idx);
#endif
				update_live_range (cfg, idx, bb->dfn, inst_num); 
				if (!mono_bitset_test (bb->kill_set, idx))
					mono_bitset_set (bb->gen_set, idx);
				vi->spill_costs += 1 + (bb->nesting * 2);
			}
		}

		/* DREG */
		regtype = spec [MONO_INST_DEST];
		g_assert (((ins->dreg == -1) && (regtype == ' ')) || ((ins->dreg != -1) && (regtype != ' ')));
				
		if ((ins->dreg != -1) && get_vreg_to_inst (cfg, regtype, ins->dreg)) {
			MonoInst *var = cfg->vreg_to_inst [regtype][ins->dreg];
			int idx = var->inst_c0;
			MonoMethodVar *vi = MONO_VARINFO (cfg, idx);

			if (store) {
				update_live_range (cfg, idx, bb->dfn, inst_num); 
				if (!mono_bitset_test (bb->kill_set, idx))
					mono_bitset_set (bb->gen_set, idx);
				vi->spill_costs += 1 + (bb->nesting * 2);
			} else {
#ifdef DEBUG_LIVENESS
				printf ("\tKILL: R%d(%d)\n", ins->dreg, idx);
#endif
				update_live_range (cfg, idx, bb->dfn, inst_num); 
				mono_bitset_set (bb->kill_set, idx);
				vi->spill_costs += 1 + (bb->nesting * 2);
			}
		}

		return;
	}

	arity = mono_burg_arity [inst->opcode];
	if (arity)
		update_gen_kill_set (cfg, bb, inst->inst_i0, inst_num);

	if (arity > 1)
		update_gen_kill_set (cfg, bb, inst->inst_i1, inst_num);

	if ((inst->ssa_op & MONO_SSA_LOAD_STORE) || (inst->opcode == OP_DUMMY_STORE)) {
		MonoLocalVariableList* affected_variables;
		MonoLocalVariableList local_affected_variable;
		
		if (cfg->aliasing_info == NULL) {
			if ((inst->ssa_op == MONO_SSA_LOAD) || (inst->ssa_op == MONO_SSA_STORE) || (inst->opcode == OP_DUMMY_STORE)) {
				local_affected_variable.variable_index = inst->inst_i0->inst_c0;
				local_affected_variable.next = NULL;
				affected_variables = &local_affected_variable;
			} else {
				affected_variables = NULL;
			}
		} else {
			affected_variables = mono_aliasing_get_affected_variables_for_inst_traversing_code (cfg->aliasing_info, inst);
		}
		
		if (inst->ssa_op & MONO_SSA_LOAD) {
			MonoLocalVariableList* affected_variable = affected_variables;
			while (affected_variable != NULL) {
				int idx = affected_variable->variable_index;
				MonoMethodVar *vi = MONO_VARINFO (cfg, idx);
				g_assert (idx < max_vars);
				if ((bb->region != -1) && !MONO_BBLOCK_IS_IN_REGION (bb, MONO_REGION_TRY)) {
					/*
					 * Variables used in exception regions can't be allocated to 
					 * registers.
					 */
					cfg->varinfo [vi->idx]->flags |= MONO_INST_VOLATILE;
				}
				update_live_range (cfg, idx, bb->dfn, inst_num); 
				if (!mono_bitset_test (bb->kill_set, idx))
					mono_bitset_set (bb->gen_set, idx);
				if (inst->ssa_op == MONO_SSA_LOAD)
					vi->spill_costs += 1 + (bb->nesting * 2);
				
				affected_variable = affected_variable->next;
			}
		} else if ((inst->ssa_op == MONO_SSA_STORE) || (inst->opcode == OP_DUMMY_STORE)) {
			MonoLocalVariableList* affected_variable = affected_variables;
			while (affected_variable != NULL) {
				int idx = affected_variable->variable_index;
				MonoMethodVar *vi = MONO_VARINFO (cfg, idx);
				g_assert (idx < max_vars);
				//if (arity > 0)
					//g_assert (inst->inst_i1->opcode != OP_PHI);
				if ((bb->region != -1) && !MONO_BBLOCK_IS_IN_REGION (bb, MONO_REGION_TRY)) {
					/*
					 * Variables used in exception regions can't be allocated to 
					 * registers.
					 */
					cfg->varinfo [vi->idx]->flags |= MONO_INST_VOLATILE;
				}
				update_live_range (cfg, idx, bb->dfn, inst_num); 
				mono_bitset_set (bb->kill_set, idx);
				if (inst->ssa_op == MONO_SSA_STORE)
					vi->spill_costs += 1 + (bb->nesting * 2);
				
				affected_variable = affected_variable->next;
			}
		}
	} else if (inst->opcode == CEE_JMP) {
		/* Keep arguments live! */
		int i;
		for (i = 0; i < cfg->num_varinfo; i++) {
			if (cfg->varinfo [i]->opcode == OP_ARG) {
				if (!mono_bitset_test (bb->kill_set, i))
					mono_bitset_set (bb->gen_set, i);
			}
		}
	}
} 

static void
update_volatile (MonoCompile *cfg, MonoBasicBlock *bb, MonoInst *inst)
{
	int arity = mono_burg_arity [inst->opcode];
	int max_vars = cfg->num_varinfo;

	if (arity)
		update_volatile (cfg, bb, inst->inst_i0);

	if (arity > 1)
		update_volatile (cfg, bb, inst->inst_i1);

	if (inst->ssa_op & MONO_SSA_LOAD_STORE) {
		MonoLocalVariableList* affected_variables;
		MonoLocalVariableList local_affected_variable;
		
		if (cfg->aliasing_info == NULL) {
			if ((inst->ssa_op == MONO_SSA_LOAD) || (inst->ssa_op == MONO_SSA_STORE)) {
				local_affected_variable.variable_index = inst->inst_i0->inst_c0;
				local_affected_variable.next = NULL;
				affected_variables = &local_affected_variable;
			} else {
				affected_variables = NULL;
			}
		} else {
			affected_variables = mono_aliasing_get_affected_variables_for_inst_traversing_code (cfg->aliasing_info, inst);
		}
		
		while (affected_variables != NULL) {
			int idx = affected_variables->variable_index;
			MonoMethodVar *vi = MONO_VARINFO (cfg, idx);
			g_assert (idx < max_vars);
			cfg->varinfo [vi->idx]->flags |= MONO_INST_VOLATILE;
			
			affected_variables = affected_variables->next;
		}
	}
} 

static void
visit_bb (MonoCompile *cfg, MonoBasicBlock *bb, GSList **visited)
{
	int i;
	MonoInst *ins;

	if (g_slist_find (*visited, bb))
		return;

	if (cfg->new_ir) {
		/* FIXME: Rewrite this using the aliasing framework */
		for (ins = bb->code; ins; ins = ins->next) {
			const char *spec = ins_info [ins->opcode - OP_START - 1];
			int regtype, srcindex, sreg;

			if (ins->opcode < MONO_CEE_LAST)
				return;

			/* DREG */
			regtype = spec [MONO_INST_DEST];
			g_assert (((ins->dreg == -1) && (regtype == ' ')) || ((ins->dreg != -1) && (regtype != ' ')));
				
			if ((ins->dreg != -1) && get_vreg_to_inst (cfg, regtype, ins->dreg)) {
				MonoInst *var = cfg->vreg_to_inst [regtype][ins->dreg];
				int idx = var->inst_c0;
				MonoMethodVar *vi = MONO_VARINFO (cfg, idx);

				cfg->varinfo [vi->idx]->flags |= MONO_INST_VOLATILE;
			}
			
			/* SREGS */
			for (srcindex = 0; srcindex < 2; ++srcindex) {
				regtype = spec [(srcindex == 0) ? MONO_INST_SRC1 : MONO_INST_SRC2];
				sreg = srcindex == 0 ? ins->sreg1 : ins->sreg2;

				g_assert (((sreg == -1) && (regtype == ' ')) || ((sreg != -1) && (regtype != ' ')));
				if ((sreg != -1) && get_vreg_to_inst (cfg, regtype, sreg)) {
					MonoInst *var = cfg->vreg_to_inst [regtype][sreg];
					int idx = var->inst_c0;
					MonoMethodVar *vi = MONO_VARINFO (cfg, idx);

					cfg->varinfo [vi->idx]->flags |= MONO_INST_VOLATILE;
				}
			}
		}

		return;
	}

	if (cfg->aliasing_info != NULL)
		mono_aliasing_initialize_code_traversal (cfg->aliasing_info, bb);

	for (ins = bb->code; ins; ins = ins->next) {
		update_volatile (cfg, bb, ins);
	}

	*visited = g_slist_append (*visited, bb);

	/* 
	 * Need to visit all bblocks reachable from this one since they can be
	 * reached during exception handling.
	 */
	for (i = 0; i < bb->out_count; ++i) {
		visit_bb (cfg, bb->out_bb [i], visited);
	}
}

static void
handle_exception_clauses (MonoCompile *cfg)
{
	MonoBasicBlock *bb;
	GSList *visited = NULL;

	/*
	 * Variables in exception handler register cannot be allocated to registers
	 * so make them volatile. See bug #42136. This will not be neccessary when
	 * the back ends could guarantee that the variables will be in the
	 * correct registers when a handler is called.
	 * This includes try blocks too, since a variable in a try block might be
	 * accessed after an exception handler has been run.
	 */
	for (bb = cfg->bb_entry; bb; bb = bb->next_bb) {

		if (bb->region == -1 || MONO_BBLOCK_IS_IN_REGION (bb, MONO_REGION_TRY))
			continue;

		visit_bb (cfg, bb, &visited);
	}
	g_slist_free (visited);
}

/* generic liveness analysis code. CFG specific parts are 
 * in update_gen_kill_set()
 */
void
mono_analyze_liveness (MonoCompile *cfg)
{
	MonoBitSet *old_live_in_set, *old_live_out_set, *tmp_in_set;
	gboolean changes;
	int i, j, max_vars = cfg->num_varinfo;
	int iterations, out_iter, in_iter;
	gboolean *changed_in, *changed_out, *new_changed_in, *in_worklist;
	MonoBasicBlock **worklist;
	guint32 l_begin, l_end;
	static int count = 0;

#ifdef DEBUG_LIVENESS
	printf ("LIVENESS %s\n", mono_method_full_name (cfg->method, TRUE));
#endif

	g_assert (!(cfg->comp_done & MONO_COMP_LIVENESS));

	cfg->comp_done |= MONO_COMP_LIVENESS;
	
	if (max_vars == 0)
		return;

	for (i = 0; i < cfg->num_bblocks; ++i) {
		MonoBasicBlock *bb = cfg->bblocks [i];

		bb->gen_set = mono_bitset_mp_new (cfg->mempool, max_vars);
		bb->kill_set = mono_bitset_mp_new (cfg->mempool, max_vars);
		bb->live_in_set = mono_bitset_mp_new (cfg->mempool, max_vars);
		bb->live_out_set = mono_bitset_mp_new (cfg->mempool, max_vars);
	}
	for (i = 0; i < max_vars; i ++) {
		MONO_VARINFO (cfg, i)->range.first_use.abs_pos = ~ 0;
		MONO_VARINFO (cfg, i)->range.last_use .abs_pos =   0;
		MONO_VARINFO (cfg, i)->spill_costs = 0;
	}

	for (i = 0; i < cfg->num_bblocks; ++i) {
		MonoBasicBlock *bb = cfg->bblocks [i];
		MonoInst *inst;
		int tree_num;

		if (cfg->aliasing_info != NULL)
			mono_aliasing_initialize_code_traversal (cfg->aliasing_info, bb);
		
		for (tree_num = 0, inst = bb->code; inst; inst = inst->next, tree_num++) {
#ifdef DEBUG_LIVENESS
			mono_print_tree (inst); printf ("\n");
#endif
			update_gen_kill_set (cfg, bb, inst, tree_num);
		}

#ifdef DEBUG_LIVENESS
		printf ("BLOCK BB%d (", bb->block_num);
		for (j = 0; j < bb->out_count; j++) 
			printf ("BB%d, ", bb->out_bb [j]->block_num);
		
		printf (")\n");
		printf ("GEN  BB%d: ", bb->block_num); mono_bitset_print (bb->gen_set);
		printf ("KILL BB%d: ", bb->block_num); mono_bitset_print (bb->kill_set);
#endif
	}

	old_live_in_set = mono_bitset_new (max_vars, 0);
	old_live_out_set = mono_bitset_new (max_vars, 0);
	tmp_in_set = mono_bitset_new (max_vars, 0);
	changed_in = g_new0 (gboolean, cfg->num_bblocks + 1);
	changed_out = g_new0 (gboolean, cfg->num_bblocks + 1);
	in_worklist = g_new0 (gboolean, cfg->num_bblocks + 1);
	new_changed_in = g_new0 (gboolean, cfg->num_bblocks + 1);

	for (i = 0; i < cfg->num_bblocks + 1; ++i) {
		changed_in [i] = TRUE;
		changed_out [i] = TRUE;
	}

	count ++;

	worklist = g_new0 (MonoBasicBlock *, cfg->num_bblocks + 1);
	l_begin = 0;
	l_end = 0;

	for (i = cfg->num_bblocks - 1; i >= 0; i--) {
		MonoBasicBlock *bb = cfg->bblocks [i];

		worklist [l_end ++] = bb;
		in_worklist [bb->dfn] = TRUE;
	}

	iterations = 0;
	out_iter = 0;
	in_iter = 0;
	do {
		changes = FALSE;
		iterations ++;

		while (l_begin != l_end) {
			MonoBasicBlock *bb = worklist [l_begin ++];

			in_worklist [bb->dfn] = FALSE;

			if (l_begin == cfg->num_bblocks + 1)
				l_begin = 0;

			if (bb->out_count > 0) {
				out_iter ++;
				mono_bitset_copyto (bb->live_out_set, old_live_out_set);

				for (j = 0; j < bb->out_count; j++) {
					MonoBasicBlock *out_bb = bb->out_bb [j];

					mono_bitset_copyto (out_bb->live_out_set, tmp_in_set);
					mono_bitset_sub (tmp_in_set, out_bb->kill_set);
					mono_bitset_union (tmp_in_set, out_bb->gen_set);

					mono_bitset_union (bb->live_out_set, tmp_in_set);

				}
				
				changed_out [bb->dfn] = !mono_bitset_equal (old_live_out_set, bb->live_out_set);
				if (changed_out [bb->dfn]) {
					for (j = 0; j < bb->in_count; j++) {
						MonoBasicBlock *in_bb = bb->in_bb [j];
						/* 
						 * Some basic blocks do not seem to be in the 
						 * cfg->bblocks array...
						 */
						if (in_bb->live_in_set)
							if (!in_worklist [in_bb->dfn]) {
								worklist [l_end ++] = in_bb;
								if (l_end == cfg->num_bblocks + 1)
									l_end = 0;
								in_worklist [in_bb->dfn] = TRUE;
							}
					}

					changes = TRUE;
				}
			}
		}
	} while (changes);

	//printf ("IT: %d %d %d.\n", iterations, in_iter, out_iter);

	mono_bitset_free (old_live_in_set);
	mono_bitset_free (old_live_out_set);
	mono_bitset_free (tmp_in_set);

	g_free (changed_in);
	g_free (changed_out);
	g_free (new_changed_in);
	g_free (worklist);
	g_free (in_worklist);

	for (i = cfg->num_bblocks - 1; i >= 0; i--) {
		MonoBasicBlock *bb = cfg->bblocks [i];

		mono_bitset_copyto (bb->live_out_set, bb->live_in_set);
		mono_bitset_sub (bb->live_in_set, bb->kill_set);
		mono_bitset_union (bb->live_in_set, bb->gen_set);
	}

	/*
	 * This code can be slow for large methods so inline the calls to
	 * mono_bitset_test.
	 */
	for (i = 0; i < cfg->num_bblocks; ++i) {
		MonoBasicBlock *bb = cfg->bblocks [i];
		guint32 rem;

		rem = max_vars % 32;
		for (j = 0; j < (max_vars / 32) + 1; ++j) {
			guint32 bits_in;
			guint32 bits_out;
			int k, nbits;

			if (j > (max_vars / 32))
				break;
			else
				if (j == (max_vars / 32))
					nbits = rem;
				else
					nbits = 32;

			bits_in = mono_bitset_test_bulk (bb->live_in_set, j * 32);
			bits_out = mono_bitset_test_bulk (bb->live_out_set, j * 32);
			for (k = 0; k < nbits; ++k) {
				if (bits_in & (1 << k))
					update_live_range (cfg, (j * 32) + k, bb->dfn, 0);
				if (bits_out & (1 << k))
					update_live_range (cfg, (j * 32) + k, bb->dfn, 0xffff);
			}
		}
	}

	/* todo: remove code when we have verified that the liveness for try/catch blocks
	 * works perfectly 
	 */
	/* 
	 * Currently, this can't be commented out since exception blocks are not
	 * processed during liveness analysis.
	 */
	handle_exception_clauses (cfg);

	/*
	 * Arguments need to have their live ranges extended to the beginning of
	 * the method to account for the arg reg/memory -> global register copies
	 * in the prolog (bug #74992).
	 */

	for (i = 0; i < max_vars; i ++) {
		MonoMethodVar *vi = MONO_VARINFO (cfg, i);
		if (cfg->varinfo [vi->idx]->opcode == OP_ARG)
			vi->range.first_use.abs_pos = 0;
	}

#ifdef DEBUG_LIVENESS
	for (i = cfg->num_bblocks - 1; i >= 0; i--) {
		MonoBasicBlock *bb = cfg->bblocks [i];
		
		printf ("LIVE IN  BB%d: ", bb->block_num); 
		mono_bitset_print (bb->live_in_set); 
		printf ("LIVE OUT BB%d: ", bb->block_num); 
		mono_bitset_print (bb->live_out_set); 
	}
#endif
}
