/*
 * cfold.c: Constant folding support
 *
 * Author:
 *   Paolo Molaro (lupus@ximian.com)
 *   Dietmar Maurer (dietmar@ximian.com)
 *
 * (C) 2003 Ximian, Inc.  http://www.ximian.com
 */
#include "mini.h"

int
mono_is_power_of_two (guint32 val)
{
	int i, j, k;

	for (i = 0, j = 1, k = 0xfffffffe; i < 32; ++i, j = j << 1, k = k << 1) {
		if (val & j)
			break;
	}
	if (i == 32 || val & k)
		return -1;
	return i;
}

#define FOLD_BINOP(name,op)	\
	case name:	\
		if (inst->inst_i0->opcode != OP_ICONST)	\
			return;	\
		if (inst->inst_i1->opcode == OP_ICONST) {	\
			inst->opcode = OP_ICONST;	\
			inst->inst_c0 = inst->inst_i0->inst_c0 op inst->inst_i1->inst_c0;	\
		} \
                return;

/*
 * We try to put constants on the left side of a commutative operation
 * because it reduces register pressure and it matches the usual cpu 
 * instructions with immediates.
 */
#define FOLD_BINOPCOMM(name,op)	\
	case name:	\
		if (inst->inst_i0->opcode == OP_ICONST)	{\
			if (inst->inst_i1->opcode == OP_ICONST) {	\
				inst->opcode = OP_ICONST;	\
				inst->inst_c0 = inst->inst_i0->inst_c0 op inst->inst_i1->inst_c0;	\
                                return; \
			} else { \
				MonoInst *tmp = inst->inst_i0;	\
				inst->inst_i0 = inst->inst_i1;	\
				inst->inst_i1 = tmp;	\
                       } \
		} \
		if (inst->inst_i1->opcode == OP_ICONST && inst->opcode == CEE_ADD) {	\
			if (inst->inst_i1->inst_c0 == 0) { \
				*inst = *(inst->inst_i0); \
				return;	\
			} \
		} \
		if (inst->inst_i1->opcode == OP_ICONST && inst->opcode == CEE_MUL) {	\
 		        int power2;	\
			if (inst->inst_i1->inst_c0 == 1) {	\
				*inst = *(inst->inst_i0);	\
				return;	\
			} else if (inst->inst_i1->inst_c0 == -1) {	\
				inst->opcode = CEE_NEG;	\
				return;	\
			}	\
 		        power2 = mono_is_power_of_two (inst->inst_i1->inst_c0);	\
		     	if (power2 < 0) return;	\
			inst->opcode = CEE_SHL;	\
			inst->inst_i1->inst_c0 = power2;	\
		} \
                return;

#ifndef G_MININT32
#define MYGINT32_MAX 2147483647
#define G_MININT32 (-MYGINT32_MAX -1)
#endif

/* 
 * We can't let this cause a division by zero exception since the division 
 * might not be executed during runtime.
 */
#define FOLD_BINOPZ(name,op,cast)	\
	case name:	\
		if (inst->inst_i1->opcode == OP_ICONST && inst->opcode == CEE_REM_UN && inst->inst_i1->inst_c0 == 2) {	\
			inst->opcode = CEE_AND;	\
			inst->inst_i1->inst_c0 = 1;	\
			return;	\
		}	\
		if (inst->inst_i1->opcode == OP_ICONST) {	\
			if (!inst->inst_i1->inst_c0) return;	\
			if (inst->inst_i0->opcode == OP_ICONST) {	\
                if ((inst->inst_i0->inst_c0 == G_MININT32) && (inst->inst_i1->inst_c0 == -1)) \
                    return; \
				inst->inst_c0 = (cast)inst->inst_i0->inst_c0 op (cast)inst->inst_i1->inst_c0;	\
				inst->opcode = OP_ICONST;	\
			} else {	\
				int power2 = mono_is_power_of_two (inst->inst_i1->inst_c0);	\
				if (power2 < 0) return;	\
				if (inst->opcode == CEE_REM_UN) {	\
					inst->opcode = CEE_AND;	\
					inst->inst_i1->inst_c0 = (1 << power2) - 1;	\
				} else if (inst->opcode == CEE_DIV_UN) {	\
					inst->opcode = CEE_SHR_UN;	\
					inst->inst_i1->inst_c0 = power2;	\
				}	\
			}	\
		} \
                return;
	
#define FOLD_CXX(name,op,cast)	\
	case name:	\
		if (inst->inst_i0->opcode != OP_COMPARE)	\
			return;	\
		if (inst->inst_i0->inst_i0->opcode != OP_ICONST)	\
			return;	\
		if (inst->inst_i0->inst_i1->opcode == OP_ICONST) {	\
			inst->opcode = OP_ICONST;	\
			inst->inst_c0 = (cast)inst->inst_i0->inst_i0->inst_c0 op (cast)inst->inst_i0->inst_i1->inst_c0;	\
		} \
                return;

#define FOLD_UNOP(name,op)	\
	case name:	\
		if (inst->inst_i0->opcode == OP_ICONST) {	\
 		        inst->opcode = OP_ICONST;	\
		        inst->inst_c0 = op inst->inst_i0->inst_c0; \
                } else if (inst->inst_i0->opcode == OP_I8CONST) { \
 		        inst->opcode = OP_I8CONST;	\
		        inst->inst_l = op inst->inst_i0->inst_l; \
                } return;

#define FOLD_BRBINOP(name,op,cast)	\
	case name:	\
		if (inst->inst_i0->opcode != OP_COMPARE)	\
			return;	\
		if (inst->inst_i0->inst_i0->opcode != OP_ICONST)	\
			return;	\
		if (inst->inst_i0->inst_i1->opcode == OP_ICONST) {	\
			if ((cast)inst->inst_i0->inst_i0->inst_c0 op (cast)inst->inst_i0->inst_i1->inst_c0)	\
				inst->opcode = CEE_BR;	\
			else	\
				inst->opcode = CEE_NOP;	\
		} \
                return;

/*
 * Helper function to do constant expression evaluation.
 * We do constant folding of integers only, FP stuff is much more tricky,
 * int64 probably not worth it.
 */
void
mono_constant_fold_inst (MonoInst *inst, gpointer data)
{
	switch (inst->opcode) {

	/* FIXME: the CEE_B* don't contain operands, need to use the OP_COMPARE instruction */
	/*FOLD_BRBINOP (CEE_BEQ,==,gint32)
	FOLD_BRBINOP (CEE_BGE,>=,gint32)
	FOLD_BRBINOP (CEE_BGT,>,gint32)
	FOLD_BRBINOP (CEE_BLE,<=,gint32)
	FOLD_BRBINOP (CEE_BLT,<,gint32)
	FOLD_BRBINOP (CEE_BNE_UN,!=,guint32)
	FOLD_BRBINOP (CEE_BGE_UN,>=,guint32)
	FOLD_BRBINOP (CEE_BGT_UN,>,guint32)
	FOLD_BRBINOP (CEE_BLE_UN,<=,guint32)
	FOLD_BRBINOP (CEE_BLT_UN,<,guint32)*/

	FOLD_BINOPCOMM (CEE_MUL,*)

	FOLD_BINOPCOMM (CEE_ADD,+)
	FOLD_BINOP (CEE_SUB,-)
	FOLD_BINOPZ (CEE_DIV,/,gint32)
	FOLD_BINOPZ (CEE_DIV_UN,/,guint32)
	FOLD_BINOPZ (CEE_REM,%,gint32)
	FOLD_BINOPZ (CEE_REM_UN,%,guint32)
	FOLD_BINOPCOMM (CEE_AND,&)
	FOLD_BINOPCOMM (CEE_OR,|)
	FOLD_BINOPCOMM (CEE_XOR,^)
	FOLD_BINOP (CEE_SHL,<<)
	FOLD_BINOP (CEE_SHR,>>)
	case CEE_SHR_UN:
		if (inst->inst_i0->opcode != OP_ICONST)
			return;
		if (inst->inst_i1->opcode == OP_ICONST) {
			inst->opcode = OP_ICONST;
			inst->inst_c0 = (guint32)inst->inst_i0->inst_c0 >> (guint32)inst->inst_i1->inst_c0;
		}
		return;
	FOLD_UNOP (CEE_NEG,-)
	FOLD_UNOP (CEE_NOT,~)
	FOLD_CXX (OP_CEQ,==,gint32)
	FOLD_CXX (OP_CGT,>,gint32)
	FOLD_CXX (OP_CGT_UN,>,guint32)
	FOLD_CXX (OP_CLT,<,gint32)
	FOLD_CXX (OP_CLT_UN,<,guint32)
	case CEE_CONV_I8:
		if (inst->inst_i0->opcode == OP_ICONST) {
			inst->opcode = OP_I8CONST;
			inst->inst_l = inst->inst_i0->inst_c0;
		}
		return;
	case CEE_CONV_I:
	case CEE_CONV_U:
		if (inst->inst_i0->opcode == OP_ICONST) {
			inst->opcode = OP_ICONST;
			inst->inst_c0 = inst->inst_i0->inst_c0;
		} else if (inst->inst_i0->opcode == CEE_LDIND_I) {
			*inst = *inst->inst_i0;
		}
		return;
	/* we should be able to handle isinst and castclass as well */
	case CEE_ISINST:
	case CEE_CASTCLASS:
	/*
	 * TODO: 
	 * 	conv.* opcodes.
	 * 	*ovf* opcodes? I'ts slow and hard to do in C.
	 *      switch can be replaced by a simple jump 
	 */
#if SIZEOF_VOID_P == 4
	case CEE_CONV_I4:
		if ((inst->inst_left->type == STACK_I4) || (inst->inst_left->type == STACK_PTR)) {
			*inst = *inst->inst_left;
		}
		break;	
#endif
	default:
		return;
	}
}

void
mono_constant_fold (MonoCompile *cfg)
{
	MonoBasicBlock *bb;
	
	for (bb = cfg->bb_entry; bb; bb = bb->next_bb) {
		MonoInst *ins;
		for (ins = bb->code; ins; ins = ins->next)	
			mono_inst_foreach (ins, mono_constant_fold_inst, NULL);
	}
}

/*
 * If the arguments to the cond branch are constants, eval and
 * return BRANCH_NOT_TAKEN for not taken, BRANCH_TAKEN for taken,
 * BRANCH_UNDEF otherwise.
 * If this code is changed to handle also non-const values, make sure
 * side effects are handled in optimize_branches() in mini.c, by
 * inserting pop instructions.
 */
int
mono_eval_cond_branch (MonoInst *ins)
{
	MonoInst *left, *right;
	/* FIXME: handle also 64 bit ints */
	left = ins->inst_left->inst_left;
	if (left->opcode != OP_ICONST && left->opcode != OP_PCONST)
		return BRANCH_UNDEF;
	right = ins->inst_left->inst_right;
	if (right->opcode != OP_ICONST && right->opcode != OP_PCONST)
		return BRANCH_UNDEF;
	switch (ins->opcode) {
	case CEE_BEQ:
		if (left->inst_c0 == right->inst_c0)
			return BRANCH_TAKEN;
		return BRANCH_NOT_TAKEN;
	case CEE_BGE:
		if (left->inst_c0 >= right->inst_c0)
			return BRANCH_TAKEN;
		return BRANCH_NOT_TAKEN;
	case CEE_BGT:
		if (left->inst_c0 > right->inst_c0)
			return BRANCH_TAKEN;
		return BRANCH_NOT_TAKEN;
	case CEE_BLE:
		if (left->inst_c0 <= right->inst_c0)
			return BRANCH_TAKEN;
		return BRANCH_NOT_TAKEN;
	case CEE_BLT:
		if (left->inst_c0 < right->inst_c0)
			return BRANCH_TAKEN;
		return BRANCH_NOT_TAKEN;
	case CEE_BNE_UN:
		if ((gsize)left->inst_c0 != (gsize)right->inst_c0)
			return BRANCH_TAKEN;
		return BRANCH_NOT_TAKEN;
	case CEE_BGE_UN:
		if ((gsize)left->inst_c0 >= (gsize)right->inst_c0)
			return BRANCH_TAKEN;
		return BRANCH_NOT_TAKEN;
	case CEE_BGT_UN:
		if ((gsize)left->inst_c0 > (gsize)right->inst_c0)
			return BRANCH_TAKEN;
		return BRANCH_NOT_TAKEN;
	case CEE_BLE_UN:
		if ((gsize)left->inst_c0 <= (gsize)right->inst_c0)
			return BRANCH_TAKEN;
		return BRANCH_NOT_TAKEN;
	case CEE_BLT_UN:
		if ((gsize)left->inst_c0 < (gsize)right->inst_c0)
			return BRANCH_TAKEN;
		return BRANCH_NOT_TAKEN;
	}
	return BRANCH_UNDEF;
}

#ifndef G_MININT32
#define MYGINT32_MAX 2147483647
#define G_MININT32 (-MYGINT32_MAX -1)
#endif

#define FOLD_UNOP2(name,op)	\
	case name:	\
	    ins->inst_c0 = op arg1->inst_c0; \
        break;

#define FOLD_BINOP2(name, op) \
	case name:	\
	    ins->inst_c0 = arg1->inst_c0 op arg2->inst_c0;	\
        break;

#define FOLD_BINOPC2(name,op,cast)	\
	case name:	\
	    ins->inst_c0 = (cast)arg1->inst_c0 op (cast)arg2->inst_c0;	\
        break;

#define FOLD_BINOP2_IMM(name, op) \
	case name:	\
	    ins->inst_c0 = arg1->inst_c0 op ins->inst_imm;	\
        break;

#define FOLD_BINOPC2_IMM(name, op, cast) \
	case name:	\
	    ins->inst_c0 = (cast)arg1->inst_c0 op (cast)ins->inst_imm;	\
        break;

#define FOLD_BINOPCXX2(name,op,cast)	\
	case name:	\
	    res = (cast)arg1->inst_c0 op (cast)arg2->inst_c0;	\
        break; \

void
mono_constant_fold_ins2 (MonoInst *ins, MonoInst *arg1, MonoInst *arg2)
{
	switch (ins->opcode) {
	case OP_IMUL:
	case OP_IADD:
	case OP_IAND:
	case OP_IOR:
	case OP_IXOR:
		if (arg2->opcode == OP_ICONST) {
			if (arg1->opcode == OP_ICONST) {
				switch (ins->opcode) {
					FOLD_BINOP2 (OP_IMUL, *);
					FOLD_BINOP2 (OP_IADD, +);
					FOLD_BINOP2 (OP_IAND, &);
					FOLD_BINOP2 (OP_IOR, |);
					FOLD_BINOP2 (OP_IXOR, ^);
				}
				ins->opcode = OP_ICONST;
				ins->sreg1 = ins->sreg2 = -1;
			}
			else {
				/* 
				 * This is commutative so swap the arguments, allowing the _imm variant
				 * to be used.
				 */
				ins->opcode = mono_op_to_op_imm (ins->opcode);
				ins->sreg1 = ins->sreg2;
				ins->sreg2 = -1;
				ins->inst_c0 = arg2->inst_c0;
			}
		}
		break;
	case OP_IMUL_IMM:
	case OP_IADD_IMM:
	case OP_IAND_IMM:
	case OP_IOR_IMM:
	case OP_IXOR_IMM:
	case OP_ISUB_IMM:
	case OP_ISHL_IMM:
	case OP_ISHR_IMM:
	case OP_ISHR_UN_IMM:
		if (arg1->opcode == OP_ICONST) {
			switch (ins->opcode) {
				FOLD_BINOP2_IMM (OP_IMUL_IMM, *);
				FOLD_BINOP2_IMM (OP_IADD_IMM, +);
				FOLD_BINOP2_IMM (OP_IAND_IMM, &);
				FOLD_BINOP2_IMM (OP_IOR_IMM, |);
				FOLD_BINOP2_IMM (OP_IXOR_IMM, ^);
				FOLD_BINOP2_IMM (OP_ISUB_IMM, -);
				FOLD_BINOP2_IMM (OP_ISHL_IMM, <<);
				FOLD_BINOP2_IMM (OP_ISHR_IMM, >>);
				FOLD_BINOPC2_IMM (OP_ISHR_UN_IMM, >>, guint32);
			}
			ins->opcode = OP_ICONST;
			ins->sreg1 = ins->sreg2 = -1;
		}
		break;
	case OP_ISUB:
	case OP_ISHL:
	case OP_ISHR:
	case OP_ISHR_UN:
		if ((arg1->opcode == OP_ICONST) && (arg2->opcode == OP_ICONST)) {
			switch (ins->opcode) {
				FOLD_BINOP2 (OP_ISUB, -);
				FOLD_BINOP2 (OP_ISHL, <<);
				FOLD_BINOP2 (OP_ISHR, >>);
				FOLD_BINOPC2 (OP_ISHR_UN, >>, guint32);
			}
			ins->opcode = OP_ICONST;
			ins->sreg1 = ins->sreg2 = -1;
		}
		break;
	case OP_IDIV:
	case OP_IDIV_UN:
	case OP_IREM:
	case OP_IREM_UN:
		if ((arg1->opcode == OP_ICONST) && (arg2->opcode == OP_ICONST)) {
			if ((arg2->inst_c0 == 0) || ((arg1->inst_c0 == G_MININT32) && (arg2->inst_c0 == -1)))
				return;
			switch (ins->opcode) {
				FOLD_BINOPC2 (OP_IDIV, /, gint32);
				FOLD_BINOPC2 (OP_IDIV_UN, /, guint32);
				FOLD_BINOPC2 (OP_IREM, %, gint32);
				FOLD_BINOPC2 (OP_IREM_UN, %, guint32);
			}
			ins->opcode = OP_ICONST;
			ins->sreg1 = ins->sreg2 = -1;
		}
		break;
		/* case OP_INEG: */
	case OP_INOT:
	case OP_INEG:
		if (arg1->opcode == OP_ICONST) {
			/* INEG sets cflags on x86, and the LNEG decomposition depends on that */
			if ((ins->opcode == OP_INEG) && ins->next && (ins->next->opcode == OP_ADC_IMM))
				return;
			switch (ins->opcode) {
				FOLD_UNOP2 (OP_INEG,-);
				FOLD_UNOP2 (OP_INOT,~);
			}
			ins->opcode = OP_ICONST;
			ins->sreg1 = ins->sreg2 = -1;
		}
		break;

	case OP_COMPARE:
	case OP_ICOMPARE:
	case OP_COMPARE_IMM:
	case OP_ICOMPARE_IMM: {
		MonoInst dummy_arg2;
		if (ins->sreg2 == -1) {
			arg2 = &dummy_arg2;
			arg2->opcode = OP_ICONST;
			arg2->inst_c0 = ins->inst_imm;
		}

		if ((arg1->opcode == OP_ICONST) && (arg2->opcode == OP_ICONST)) {
			MonoInst *next = ins->next;
			gboolean res;

			switch (next->opcode) {
			case OP_CEQ:
			case OP_ICEQ:
			case OP_CGT:
			case OP_ICGT:
			case OP_CGT_UN:
			case OP_ICGT_UN:
			case OP_CLT:
			case OP_ICLT:
			case OP_CLT_UN:
			case OP_ICLT_UN:
				switch (next->opcode) {
					FOLD_BINOPCXX2 (OP_CEQ,==,gint32);
					FOLD_BINOPCXX2 (OP_ICEQ,==,gint32);
					FOLD_BINOPCXX2 (OP_CGT,>,gint32);
					FOLD_BINOPCXX2 (OP_ICGT,>,gint32);
					FOLD_BINOPCXX2 (OP_CGT_UN,>,guint32);
					FOLD_BINOPCXX2 (OP_ICGT_UN,>,guint32);
					FOLD_BINOPCXX2 (OP_CLT,<,gint32);
					FOLD_BINOPCXX2 (OP_ICLT,<,gint32);
					FOLD_BINOPCXX2 (OP_CLT_UN,<,guint32);
					FOLD_BINOPCXX2 (OP_ICLT_UN,<,guint32);
				}

				ins->opcode = OP_NOP;
				ins->sreg1 = ins->sreg2 = -1;
				next->opcode = OP_ICONST;
				next->inst_c0 = res;
				next->sreg1 = next->sreg2 = -1;
				break;
			case OP_IBEQ:
			case OP_IBNE_UN:
			case OP_IBGT:
			case OP_IBGT_UN:
			case OP_IBGE:
			case OP_IBGE_UN:
			case OP_IBLT:
			case OP_IBLT_UN:
			case OP_IBLE:
			case OP_IBLE_UN:
				switch (next->opcode) {
					FOLD_BINOPCXX2 (OP_IBEQ,==,gint32);
					FOLD_BINOPCXX2 (OP_IBNE_UN,!=,guint32);
					FOLD_BINOPCXX2 (OP_IBGT,>,gint32);
					FOLD_BINOPCXX2 (OP_IBGT_UN,>,guint32);
					FOLD_BINOPCXX2 (OP_IBGE,>=,gint32);
					FOLD_BINOPCXX2 (OP_IBGE_UN,>=,guint32);
					FOLD_BINOPCXX2 (OP_IBLT,<,gint32);
					FOLD_BINOPCXX2 (OP_IBLT_UN,<,guint32);
					FOLD_BINOPCXX2 (OP_IBLE,<=,gint32);
					FOLD_BINOPCXX2 (OP_IBLE_UN,<=,guint32);
				}

				/* 
				 * Can't nullify OP_COMPARE here since the decompose long branch 
				 * opcodes depend on it being executed. Also, the branch might not
				 * be eliminated after all if loop opts is disabled, for example.
				 */
				if (res)
					next->flags |= MONO_INST_CFOLD_TAKEN;
				else
					next->flags |= MONO_INST_CFOLD_NOT_TAKEN;
				break;
			case OP_NOP:
				/* This happens when a conditional branch is eliminated */
				ins->opcode = OP_NOP;
				ins->sreg1 = ins->sreg2 = -1;
				break;
			default:
				return;
			}
		}
		break;
	}

		/*
		 * TODO: 
		 * 	conv.* opcodes.
		 * 	*ovf* opcodes? I'ts slow and hard to do in C.
		 *      switch can be replaced by a simple jump 
		 */
	default:
		return;
	}
}	
