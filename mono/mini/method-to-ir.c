/*
 * method-to-ir.c: Convert CIL to the JIT internal representation
 *
 * Author:
 *   Paolo Molaro (lupus@ximian.com)
 *   Dietmar Maurer (dietmar@ximian.com)
 *
 * (C) 2002 Ximian, Inc.
 */

#include <config.h>
#include <signal.h>

#ifdef HAVE_UNISTD_H
#include <unistd.h>
#endif

#include <math.h>
#include <string.h>
#include <ctype.h>

#ifdef HAVE_SYS_TIME_H
#include <sys/time.h>
#endif

#ifdef HAVE_VALGRIND_MEMCHECK_H
#include <valgrind/memcheck.h>
#endif

#include <mono/metadata/assembly.h>
#include <mono/metadata/loader.h>
#include <mono/metadata/tabledefs.h>
#include <mono/metadata/class.h>
#include <mono/metadata/object.h>
#include <mono/metadata/exception.h>
#include <mono/metadata/opcodes.h>
#include <mono/metadata/mono-endian.h>
#include <mono/metadata/tokentype.h>
#include <mono/metadata/tabledefs.h>
#include <mono/metadata/marshal.h>
#include <mono/metadata/debug-helpers.h>
#include <mono/metadata/mono-debug.h>
#include <mono/metadata/gc-internal.h>
#include <mono/metadata/security-manager.h>
#include <mono/metadata/threads-types.h>
#include <mono/metadata/security-core-clr.h>
#include <mono/metadata/monitor.h>
#include <mono/utils/mono-compiler.h>

#include "mini.h"
#include "trace.h"

#include "ir-emit.h"

#include "jit-icalls.h"

#define BRANCH_COST 100
#define INLINE_LENGTH_LIMIT 20
#define INLINE_FAILURE do {\
		if ((cfg->method != method) && (method->wrapper_type == MONO_WRAPPER_NONE))\
			goto inline_failure;\
	} while (0)
#define CHECK_CFG_EXCEPTION do {\
		if (cfg->exception_type != MONO_EXCEPTION_NONE)\
			goto exception_exit;\
	} while (0)
#define METHOD_ACCESS_FAILURE do {	\
		char *method_fname = mono_method_full_name (method, TRUE);	\
		char *cil_method_fname = mono_method_full_name (cil_method, TRUE);	\
		cfg->exception_type = MONO_EXCEPTION_METHOD_ACCESS;	\
		cfg->exception_message = g_strdup_printf ("Method `%s' is inaccessible from method `%s'\n", cil_method_fname, method_fname);	\
		g_free (method_fname);	\
		g_free (cil_method_fname);	\
		goto exception_exit;	\
	} while (0)
#define FIELD_ACCESS_FAILURE do {	\
		char *method_fname = mono_method_full_name (method, TRUE);	\
		char *field_fname = mono_field_full_name (field);	\
		cfg->exception_type = MONO_EXCEPTION_FIELD_ACCESS;	\
		cfg->exception_message = g_strdup_printf ("Field `%s' is inaccessible from method `%s'\n", field_fname, method_fname);	\
		g_free (method_fname);	\
		g_free (field_fname);	\
		goto exception_exit;	\
	} while (0)
#define GENERIC_SHARING_FAILURE(opcode) do {		\
		if (cfg->generic_sharing_context) {	\
            if (cfg->verbose_level > 2) \
			    printf ("sharing failed for method %s.%s.%s/%d opcode %s line %d\n", method->klass->name_space, method->klass->name, method->name, method->signature->param_count, mono_opcode_name ((opcode)), __LINE__); \
			cfg->exception_type = MONO_EXCEPTION_GENERIC_SHARING_FAILED;	\
			goto exception_exit;	\
		}			\
	} while (0)

/* Determine whenever 'ins' represents a load of the 'this' argument */
#define MONO_CHECK_THIS(ins) (mono_method_signature (cfg->method)->hasthis && ((ins)->opcode == OP_MOVE) && ((ins)->sreg1 == cfg->args [0]->dreg))

static int ldind_to_load_membase (int opcode);
static int stind_to_store_membase (int opcode);

int mono_op_to_op_imm (int opcode);
int mono_op_to_op_imm_noemul (int opcode);

MonoInst* mono_emit_native_call (MonoCompile *cfg, gconstpointer func, MonoMethodSignature *sig, MonoInst **args);
void mini_emit_stobj (MonoCompile *cfg, MonoInst *dest, MonoInst *src, MonoClass *klass, gboolean native);
void mini_emit_initobj (MonoCompile *cfg, MonoInst *dest, const guchar *ip, MonoClass *klass);

/* helper methods signature */
extern MonoMethodSignature *helper_sig_class_init_trampoline;
extern MonoMethodSignature *helper_sig_domain_get;
extern MonoMethodSignature *helper_sig_generic_class_init_trampoline;
extern MonoMethodSignature *helper_sig_rgctx_lazy_fetch_trampoline;
extern MonoMethodSignature *helper_sig_monitor_enter_exit_trampoline;

/*
 * Instruction metadata
 */
#ifdef MINI_OP
#undef MINI_OP
#endif
#define MINI_OP(a,b,dest,src1,src2) dest, src1, src2,
#define NONE ' '
#define IREG 'i'
#define FREG 'f'
#define VREG 'v'
#define XREG 'x'
#if SIZEOF_REGISTER == 8
#define LREG IREG
#else
#define LREG 'l'
#endif
/* keep in sync with the enum in mini.h */
const char
ins_info[] = {
#include "mini-ops.h"
};
#undef MINI_OP

extern GHashTable *jit_icall_name_hash;

#define MONO_INIT_VARINFO(vi,id) do { \
	(vi)->range.first_use.pos.bid = 0xffff; \
	(vi)->reg = -1; \
	(vi)->idx = (id); \
} while (0)

guint32
mono_alloc_ireg (MonoCompile *cfg)
{
	return alloc_ireg (cfg);
}

guint32
mono_alloc_freg (MonoCompile *cfg)
{
	return alloc_freg (cfg);
}

guint32
mono_alloc_preg (MonoCompile *cfg)
{
	return alloc_preg (cfg);
}

guint32
mono_alloc_dreg (MonoCompile *cfg, MonoStackType stack_type)
{
	return alloc_dreg (cfg, stack_type);
}

guint
mono_type_to_regmove (MonoCompile *cfg, MonoType *type)
{
	if (type->byref)
		return OP_MOVE;

handle_enum:
	switch (type->type) {
	case MONO_TYPE_I1:
	case MONO_TYPE_U1:
	case MONO_TYPE_BOOLEAN:
		return OP_MOVE;
	case MONO_TYPE_I2:
	case MONO_TYPE_U2:
	case MONO_TYPE_CHAR:
		return OP_MOVE;
	case MONO_TYPE_I4:
	case MONO_TYPE_U4:
		return OP_MOVE;
	case MONO_TYPE_I:
	case MONO_TYPE_U:
	case MONO_TYPE_PTR:
	case MONO_TYPE_FNPTR:
		return OP_MOVE;
	case MONO_TYPE_CLASS:
	case MONO_TYPE_STRING:
	case MONO_TYPE_OBJECT:
	case MONO_TYPE_SZARRAY:
	case MONO_TYPE_ARRAY:    
		return OP_MOVE;
	case MONO_TYPE_I8:
	case MONO_TYPE_U8:
#if SIZEOF_REGISTER == 8
		return OP_MOVE;
#else
		return OP_LMOVE;
#endif
	case MONO_TYPE_R4:
		return OP_FMOVE;
	case MONO_TYPE_R8:
		return OP_FMOVE;
	case MONO_TYPE_VALUETYPE:
		if (type->data.klass->enumtype) {
			type = type->data.klass->enum_basetype;
			goto handle_enum;
		}
		if (MONO_CLASS_IS_SIMD (cfg, mono_class_from_mono_type (type)))
			return OP_XMOVE;
		return OP_VMOVE;
	case MONO_TYPE_TYPEDBYREF:
		return OP_VMOVE;
	case MONO_TYPE_GENERICINST:
		type = &type->data.generic_class->container_class->byval_arg;
		goto handle_enum;
	case MONO_TYPE_VAR:
	case MONO_TYPE_MVAR:
		g_assert (cfg->generic_sharing_context);
		return OP_MOVE;
	default:
		g_error ("unknown type 0x%02x in type_to_regstore", type->type);
	}
	return -1;
}

void
mono_print_bb (MonoBasicBlock *bb, const char *msg)
{
	int i;
	MonoInst *tree;

	printf ("\n%s %d: [IN: ", msg, bb->block_num);
	for (i = 0; i < bb->in_count; ++i)
		printf (" BB%d(%d)", bb->in_bb [i]->block_num, bb->in_bb [i]->dfn);
	printf (", OUT: ");
	for (i = 0; i < bb->out_count; ++i)
		printf (" BB%d(%d)", bb->out_bb [i]->block_num, bb->out_bb [i]->dfn);
	printf (" ]\n");
	for (tree = bb->code; tree; tree = tree->next)
		mono_print_ins_index (-1, tree);
}

/* 
 * Can't put this at the beginning, since other files reference stuff from this
 * file.
 */
#ifndef DISABLE_JIT

#define UNVERIFIED do { if (mini_get_debug_options ()->break_on_unverified) G_BREAKPOINT (); else goto unverified; } while (0)

#define GET_BBLOCK(cfg,tblock,ip) do {	\
		(tblock) = cfg->cil_offset_to_bb [(ip) - cfg->cil_start]; \
		if (!(tblock)) {	\
			if ((ip) >= end || (ip) < header->code) UNVERIFIED; \
            NEW_BBLOCK (cfg, (tblock)); \
			(tblock)->cil_code = (ip);	\
			ADD_BBLOCK (cfg, (tblock));	\
		} \
	} while (0)

#if defined(TARGET_X86) || defined(TARGET_AMD64)
#define EMIT_NEW_X86_LEA(cfg,dest,sr1,sr2,shift,imm) do { \
		MONO_INST_NEW (cfg, dest, OP_X86_LEA); \
		(dest)->dreg = alloc_preg ((cfg)); \
		(dest)->sreg1 = (sr1); \
		(dest)->sreg2 = (sr2); \
		(dest)->inst_imm = (imm); \
		(dest)->backend.shift_amount = (shift); \
		MONO_ADD_INS ((cfg)->cbb, (dest)); \
	} while (0)
#endif

#if SIZEOF_REGISTER == 8
#define ADD_WIDEN_OP(ins, arg1, arg2) do { \
		/* FIXME: Need to add many more cases */ \
		if ((arg1)->type == STACK_PTR && (arg2)->type == STACK_I4) {	\
			MonoInst *widen; \
			int dr = alloc_preg (cfg); \
			EMIT_NEW_UNALU (cfg, widen, OP_SEXT_I4, dr, (arg2)->dreg); \
			(ins)->sreg2 = widen->dreg; \
		} \
	} while (0)
#else
#define ADD_WIDEN_OP(ins, arg1, arg2)
#endif

#define ADD_BINOP(op) do {	\
		MONO_INST_NEW (cfg, ins, (op));	\
		sp -= 2;	\
		ins->sreg1 = sp [0]->dreg;	\
		ins->sreg2 = sp [1]->dreg;	\
		type_from_op (ins, sp [0], sp [1]);	\
		CHECK_TYPE (ins);	\
		/* Have to insert a widening op */		 \
        ADD_WIDEN_OP (ins, sp [0], sp [1]); \
        ins->dreg = alloc_dreg ((cfg), (ins)->type); \
        MONO_ADD_INS ((cfg)->cbb, (ins)); \
		*sp++ = ins;	\
        mono_decompose_opcode ((cfg), (ins)); \
	} while (0)

#define ADD_UNOP(op) do {	\
		MONO_INST_NEW (cfg, ins, (op));	\
		sp--;	\
		ins->sreg1 = sp [0]->dreg;	\
		type_from_op (ins, sp [0], NULL);	\
		CHECK_TYPE (ins);	\
        (ins)->dreg = alloc_dreg ((cfg), (ins)->type); \
        MONO_ADD_INS ((cfg)->cbb, (ins)); \
		*sp++ = ins;	\
		mono_decompose_opcode (cfg, ins); \
	} while (0)

#define ADD_BINCOND(next_block) do {	\
		MonoInst *cmp;	\
		sp -= 2; \
		MONO_INST_NEW(cfg, cmp, OP_COMPARE);	\
		cmp->sreg1 = sp [0]->dreg;	\
		cmp->sreg2 = sp [1]->dreg;	\
		type_from_op (cmp, sp [0], sp [1]);	\
		CHECK_TYPE (cmp);	\
		type_from_op (ins, sp [0], sp [1]);	\
		ins->inst_many_bb = mono_mempool_alloc (cfg->mempool, sizeof(gpointer)*2);	\
		GET_BBLOCK (cfg, tblock, target);		\
		link_bblock (cfg, bblock, tblock);	\
		ins->inst_true_bb = tblock;	\
		if ((next_block)) {	\
			link_bblock (cfg, bblock, (next_block));	\
			ins->inst_false_bb = (next_block);	\
			start_new_bblock = 1;	\
		} else {	\
			GET_BBLOCK (cfg, tblock, ip);		\
			link_bblock (cfg, bblock, tblock);	\
			ins->inst_false_bb = tblock;	\
			start_new_bblock = 2;	\
		}	\
		if (sp != stack_start) {									\
		    handle_stack_args (cfg, stack_start, sp - stack_start); \
			CHECK_UNVERIFIABLE (cfg); \
		} \
        MONO_ADD_INS (bblock, cmp); \
		MONO_ADD_INS (bblock, ins);	\
	} while (0)

/* *
 * link_bblock: Links two basic blocks
 *
 * links two basic blocks in the control flow graph, the 'from'
 * argument is the starting block and the 'to' argument is the block
 * the control flow ends to after 'from'.
 */
static void
link_bblock (MonoCompile *cfg, MonoBasicBlock *from, MonoBasicBlock* to)
{
	MonoBasicBlock **newa;
	int i, found;

#if 0
	if (from->cil_code) {
		if (to->cil_code)
			printf ("edge from IL%04x to IL_%04x\n", from->cil_code - cfg->cil_code, to->cil_code - cfg->cil_code);
		else
			printf ("edge from IL%04x to exit\n", from->cil_code - cfg->cil_code);
	} else {
		if (to->cil_code)
			printf ("edge from entry to IL_%04x\n", to->cil_code - cfg->cil_code);
		else
			printf ("edge from entry to exit\n");
	}
#endif

	found = FALSE;
	for (i = 0; i < from->out_count; ++i) {
		if (to == from->out_bb [i]) {
			found = TRUE;
			break;
		}
	}
	if (!found) {
		newa = mono_mempool_alloc (cfg->mempool, sizeof (gpointer) * (from->out_count + 1));
		for (i = 0; i < from->out_count; ++i) {
			newa [i] = from->out_bb [i];
		}
		newa [i] = to;
		from->out_count++;
		from->out_bb = newa;
	}

	found = FALSE;
	for (i = 0; i < to->in_count; ++i) {
		if (from == to->in_bb [i]) {
			found = TRUE;
			break;
		}
	}
	if (!found) {
		newa = mono_mempool_alloc (cfg->mempool, sizeof (gpointer) * (to->in_count + 1));
		for (i = 0; i < to->in_count; ++i) {
			newa [i] = to->in_bb [i];
		}
		newa [i] = from;
		to->in_count++;
		to->in_bb = newa;
	}
}

void
mono_link_bblock (MonoCompile *cfg, MonoBasicBlock *from, MonoBasicBlock* to)
{
	link_bblock (cfg, from, to);
}

/**
 * mono_find_block_region:
 *
 *   We mark each basic block with a region ID. We use that to avoid BB
 *   optimizations when blocks are in different regions.
 *
 * Returns:
 *   A region token that encodes where this region is, and information
 *   about the clause owner for this block.
 *
 *   The region encodes the try/catch/filter clause that owns this block
 *   as well as the type.  -1 is a special value that represents a block
 *   that is in none of try/catch/filter.
 */
static int
mono_find_block_region (MonoCompile *cfg, int offset)
{
	MonoMethod *method = cfg->method;
	MonoMethodHeader *header = mono_method_get_header (method);
	MonoExceptionClause *clause;
	int i;

	/* first search for handlers and filters */
	for (i = 0; i < header->num_clauses; ++i) {
		clause = &header->clauses [i];
		if ((clause->flags == MONO_EXCEPTION_CLAUSE_FILTER) && (offset >= clause->data.filter_offset) &&
		    (offset < (clause->handler_offset)))
			return ((i + 1) << 8) | MONO_REGION_FILTER | clause->flags;
			   
		if (MONO_OFFSET_IN_HANDLER (clause, offset)) {
			if (clause->flags == MONO_EXCEPTION_CLAUSE_FINALLY)
				return ((i + 1) << 8) | MONO_REGION_FINALLY | clause->flags;
			else if (clause->flags == MONO_EXCEPTION_CLAUSE_FAULT)
				return ((i + 1) << 8) | MONO_REGION_FAULT | clause->flags;
			else
				return ((i + 1) << 8) | MONO_REGION_CATCH | clause->flags;
		}
	}

	/* search the try blocks */
	for (i = 0; i < header->num_clauses; ++i) {
		clause = &header->clauses [i];
		if (MONO_OFFSET_IN_CLAUSE (clause, offset))
			return ((i + 1) << 8) | clause->flags;
	}

	return -1;
}

static GList*
mono_find_final_block (MonoCompile *cfg, unsigned char *ip, unsigned char *target, int type)
{
	MonoMethod *method = cfg->method;
	MonoMethodHeader *header = mono_method_get_header (method);
	MonoExceptionClause *clause;
	MonoBasicBlock *handler;
	int i;
	GList *res = NULL;

	for (i = 0; i < header->num_clauses; ++i) {
		clause = &header->clauses [i];
		if (MONO_OFFSET_IN_CLAUSE (clause, (ip - header->code)) && 
		    (!MONO_OFFSET_IN_CLAUSE (clause, (target - header->code)))) {
			if (clause->flags == type) {
				handler = cfg->cil_offset_to_bb [clause->handler_offset];
				g_assert (handler);
				res = g_list_append (res, handler);
			}
		}
	}
	return res;
}

static void
mono_create_spvar_for_region (MonoCompile *cfg, int region)
{
	MonoInst *var;

	var = g_hash_table_lookup (cfg->spvars, GINT_TO_POINTER (region));
	if (var)
		return;

	var = mono_compile_create_var (cfg, &mono_defaults.int_class->byval_arg, OP_LOCAL);
	/* prevent it from being register allocated */
	var->flags |= MONO_INST_INDIRECT;

	g_hash_table_insert (cfg->spvars, GINT_TO_POINTER (region), var);
}

static MonoInst *
mono_find_exvar_for_offset (MonoCompile *cfg, int offset)
{
	return g_hash_table_lookup (cfg->exvars, GINT_TO_POINTER (offset));
}

static MonoInst*
mono_create_exvar_for_offset (MonoCompile *cfg, int offset)
{
	MonoInst *var;

	var = g_hash_table_lookup (cfg->exvars, GINT_TO_POINTER (offset));
	if (var)
		return var;

	var = mono_compile_create_var (cfg, &mono_defaults.object_class->byval_arg, OP_LOCAL);
	/* prevent it from being register allocated */
	var->flags |= MONO_INST_INDIRECT;

	g_hash_table_insert (cfg->exvars, GINT_TO_POINTER (offset), var);

	return var;
}

/*
 * Returns the type used in the eval stack when @type is loaded.
 * FIXME: return a MonoType/MonoClass for the byref and VALUETYPE cases.
 */
void
type_to_eval_stack_type (MonoCompile *cfg, MonoType *type, MonoInst *inst)
{
	MonoClass *klass;

	inst->klass = klass = mono_class_from_mono_type (type);
	if (type->byref) {
		inst->type = STACK_MP;
		return;
	}

handle_enum:
	switch (type->type) {
	case MONO_TYPE_VOID:
		inst->type = STACK_INV;
		return;
	case MONO_TYPE_I1:
	case MONO_TYPE_U1:
	case MONO_TYPE_BOOLEAN:
	case MONO_TYPE_I2:
	case MONO_TYPE_U2:
	case MONO_TYPE_CHAR:
	case MONO_TYPE_I4:
	case MONO_TYPE_U4:
		inst->type = STACK_I4;
		return;
	case MONO_TYPE_I:
	case MONO_TYPE_U:
	case MONO_TYPE_PTR:
	case MONO_TYPE_FNPTR:
		inst->type = STACK_PTR;
		return;
	case MONO_TYPE_CLASS:
	case MONO_TYPE_STRING:
	case MONO_TYPE_OBJECT:
	case MONO_TYPE_SZARRAY:
	case MONO_TYPE_ARRAY:    
		inst->type = STACK_OBJ;
		return;
	case MONO_TYPE_I8:
	case MONO_TYPE_U8:
		inst->type = STACK_I8;
		return;
	case MONO_TYPE_R4:
	case MONO_TYPE_R8:
		inst->type = STACK_R8;
		return;
	case MONO_TYPE_VALUETYPE:
		if (type->data.klass->enumtype) {
			type = type->data.klass->enum_basetype;
			goto handle_enum;
		} else {
			inst->klass = klass;
			inst->type = STACK_VTYPE;
			return;
		}
	case MONO_TYPE_TYPEDBYREF:
		inst->klass = mono_defaults.typed_reference_class;
		inst->type = STACK_VTYPE;
		return;
	case MONO_TYPE_GENERICINST:
		type = &type->data.generic_class->container_class->byval_arg;
		goto handle_enum;
	case MONO_TYPE_VAR :
	case MONO_TYPE_MVAR :
		/* FIXME: all the arguments must be references for now,
		 * later look inside cfg and see if the arg num is
		 * really a reference
		 */
		g_assert (cfg->generic_sharing_context);
		inst->type = STACK_OBJ;
		return;
	default:
		g_error ("unknown type 0x%02x in eval stack type", type->type);
	}
}

/*
 * The following tables are used to quickly validate the IL code in type_from_op ().
 */
static const char
bin_num_table [STACK_MAX] [STACK_MAX] = {
	{STACK_INV, STACK_INV, STACK_INV, STACK_INV, STACK_INV, STACK_INV, STACK_INV, STACK_INV},
	{STACK_INV, STACK_I4,  STACK_INV, STACK_PTR, STACK_INV, STACK_MP,  STACK_INV, STACK_INV},
	{STACK_INV, STACK_INV, STACK_I8,  STACK_INV, STACK_INV, STACK_INV, STACK_INV, STACK_INV},
	{STACK_INV, STACK_PTR, STACK_INV, STACK_PTR, STACK_INV, STACK_MP,  STACK_INV, STACK_INV},
	{STACK_INV, STACK_INV, STACK_INV, STACK_INV, STACK_R8,  STACK_INV, STACK_INV, STACK_INV},
	{STACK_INV, STACK_MP,  STACK_INV, STACK_MP,  STACK_INV, STACK_PTR, STACK_INV, STACK_INV},
	{STACK_INV, STACK_INV, STACK_INV, STACK_INV, STACK_INV, STACK_INV, STACK_INV, STACK_INV},
	{STACK_INV, STACK_INV, STACK_INV, STACK_INV, STACK_INV, STACK_INV, STACK_INV, STACK_INV}
};

static const char 
neg_table [] = {
	STACK_INV, STACK_I4, STACK_I8, STACK_PTR, STACK_R8, STACK_INV, STACK_INV, STACK_INV
};

/* reduce the size of this table */
static const char
bin_int_table [STACK_MAX] [STACK_MAX] = {
	{STACK_INV, STACK_INV, STACK_INV, STACK_INV, STACK_INV, STACK_INV, STACK_INV, STACK_INV},
	{STACK_INV, STACK_I4,  STACK_INV, STACK_PTR, STACK_INV, STACK_INV, STACK_INV, STACK_INV},
	{STACK_INV, STACK_INV, STACK_I8,  STACK_INV, STACK_INV, STACK_INV, STACK_INV, STACK_INV},
	{STACK_INV, STACK_PTR, STACK_INV, STACK_PTR, STACK_INV, STACK_INV, STACK_INV, STACK_INV},
	{STACK_INV, STACK_INV, STACK_INV, STACK_INV, STACK_INV, STACK_INV, STACK_INV, STACK_INV},
	{STACK_INV, STACK_INV, STACK_INV, STACK_INV, STACK_INV, STACK_INV, STACK_INV, STACK_INV},
	{STACK_INV, STACK_INV, STACK_INV, STACK_INV, STACK_INV, STACK_INV, STACK_INV, STACK_INV},
	{STACK_INV, STACK_INV, STACK_INV, STACK_INV, STACK_INV, STACK_INV, STACK_INV, STACK_INV}
};

static const char
bin_comp_table [STACK_MAX] [STACK_MAX] = {
/*	Inv i  L  p  F  &  O  vt */
	{0},
	{0, 1, 0, 1, 0, 0, 0, 0}, /* i, int32 */
	{0, 0, 1, 0, 0, 0, 0, 0}, /* L, int64 */
	{0, 1, 0, 1, 0, 2, 4, 0}, /* p, ptr */
	{0, 0, 0, 0, 1, 0, 0, 0}, /* F, R8 */
	{0, 0, 0, 2, 0, 1, 0, 0}, /* &, managed pointer */
	{0, 0, 0, 4, 0, 0, 3, 0}, /* O, reference */
	{0, 0, 0, 0, 0, 0, 0, 0}, /* vt value type */
};

/* reduce the size of this table */
static const char
shift_table [STACK_MAX] [STACK_MAX] = {
	{STACK_INV, STACK_INV, STACK_INV, STACK_INV, STACK_INV, STACK_INV, STACK_INV, STACK_INV},
	{STACK_INV, STACK_I4,  STACK_INV, STACK_I4,  STACK_INV, STACK_INV, STACK_INV, STACK_INV},
	{STACK_INV, STACK_I8,  STACK_INV, STACK_I8,  STACK_INV, STACK_INV, STACK_INV, STACK_INV},
	{STACK_INV, STACK_PTR, STACK_INV, STACK_PTR, STACK_INV, STACK_INV, STACK_INV, STACK_INV},
	{STACK_INV, STACK_INV, STACK_INV, STACK_INV, STACK_INV, STACK_INV, STACK_INV, STACK_INV},
	{STACK_INV, STACK_INV, STACK_INV, STACK_INV, STACK_INV, STACK_INV, STACK_INV, STACK_INV},
	{STACK_INV, STACK_INV, STACK_INV, STACK_INV, STACK_INV, STACK_INV, STACK_INV, STACK_INV},
	{STACK_INV, STACK_INV, STACK_INV, STACK_INV, STACK_INV, STACK_INV, STACK_INV, STACK_INV}
};

/*
 * Tables to map from the non-specific opcode to the matching
 * type-specific opcode.
 */
/* handles from CEE_ADD to CEE_SHR_UN (CEE_REM_UN for floats) */
static const guint16
binops_op_map [STACK_MAX] = {
	0, OP_IADD-CEE_ADD, OP_LADD-CEE_ADD, OP_PADD-CEE_ADD, OP_FADD-CEE_ADD, OP_PADD-CEE_ADD
};

/* handles from CEE_NEG to CEE_CONV_U8 */
static const guint16
unops_op_map [STACK_MAX] = {
	0, OP_INEG-CEE_NEG, OP_LNEG-CEE_NEG, OP_PNEG-CEE_NEG, OP_FNEG-CEE_NEG, OP_PNEG-CEE_NEG
};

/* handles from CEE_CONV_U2 to CEE_SUB_OVF_UN */
static const guint16
ovfops_op_map [STACK_MAX] = {
	0, OP_ICONV_TO_U2-CEE_CONV_U2, OP_LCONV_TO_U2-CEE_CONV_U2, OP_PCONV_TO_U2-CEE_CONV_U2, OP_FCONV_TO_U2-CEE_CONV_U2, OP_PCONV_TO_U2-CEE_CONV_U2, OP_PCONV_TO_U2-CEE_CONV_U2
};

/* handles from CEE_CONV_OVF_I1_UN to CEE_CONV_OVF_U_UN */
static const guint16
ovf2ops_op_map [STACK_MAX] = {
	0, OP_ICONV_TO_OVF_I1_UN-CEE_CONV_OVF_I1_UN, OP_LCONV_TO_OVF_I1_UN-CEE_CONV_OVF_I1_UN, OP_PCONV_TO_OVF_I1_UN-CEE_CONV_OVF_I1_UN, OP_FCONV_TO_OVF_I1_UN-CEE_CONV_OVF_I1_UN, OP_PCONV_TO_OVF_I1_UN-CEE_CONV_OVF_I1_UN
};

/* handles from CEE_CONV_OVF_I1 to CEE_CONV_OVF_U8 */
static const guint16
ovf3ops_op_map [STACK_MAX] = {
	0, OP_ICONV_TO_OVF_I1-CEE_CONV_OVF_I1, OP_LCONV_TO_OVF_I1-CEE_CONV_OVF_I1, OP_PCONV_TO_OVF_I1-CEE_CONV_OVF_I1, OP_FCONV_TO_OVF_I1-CEE_CONV_OVF_I1, OP_PCONV_TO_OVF_I1-CEE_CONV_OVF_I1
};

/* handles from CEE_BEQ to CEE_BLT_UN */
static const guint16
beqops_op_map [STACK_MAX] = {
	0, OP_IBEQ-CEE_BEQ, OP_LBEQ-CEE_BEQ, OP_PBEQ-CEE_BEQ, OP_FBEQ-CEE_BEQ, OP_PBEQ-CEE_BEQ, OP_PBEQ-CEE_BEQ
};

/* handles from CEE_CEQ to CEE_CLT_UN */
static const guint16
ceqops_op_map [STACK_MAX] = {
	0, OP_ICEQ-OP_CEQ, OP_LCEQ-OP_CEQ, OP_PCEQ-OP_CEQ, OP_FCEQ-OP_CEQ, OP_PCEQ-OP_CEQ, OP_PCEQ-OP_CEQ
};

/*
 * Sets ins->type (the type on the eval stack) according to the
 * type of the opcode and the arguments to it.
 * Invalid IL code is marked by setting ins->type to the invalid value STACK_INV.
 *
 * FIXME: this function sets ins->type unconditionally in some cases, but
 * it should set it to invalid for some types (a conv.x on an object)
 */
static void
type_from_op (MonoInst *ins, MonoInst *src1, MonoInst *src2) {

	switch (ins->opcode) {
	/* binops */
	case CEE_ADD:
	case CEE_SUB:
	case CEE_MUL:
	case CEE_DIV:
	case CEE_REM:
		/* FIXME: check unverifiable args for STACK_MP */
		ins->type = bin_num_table [src1->type] [src2->type];
		ins->opcode += binops_op_map [ins->type];
		break;
	case CEE_DIV_UN:
	case CEE_REM_UN:
	case CEE_AND:
	case CEE_OR:
	case CEE_XOR:
		ins->type = bin_int_table [src1->type] [src2->type];
		ins->opcode += binops_op_map [ins->type];
		break;
	case CEE_SHL:
	case CEE_SHR:
	case CEE_SHR_UN:
		ins->type = shift_table [src1->type] [src2->type];
		ins->opcode += binops_op_map [ins->type];
		break;
	case OP_COMPARE:
	case OP_LCOMPARE:
	case OP_ICOMPARE:
		ins->type = bin_comp_table [src1->type] [src2->type] ? STACK_I4: STACK_INV;
		if ((src1->type == STACK_I8) || ((SIZEOF_REGISTER == 8) && ((src1->type == STACK_PTR) || (src1->type == STACK_OBJ) || (src1->type == STACK_MP))))
			ins->opcode = OP_LCOMPARE;
		else if (src1->type == STACK_R8)
			ins->opcode = OP_FCOMPARE;
		else
			ins->opcode = OP_ICOMPARE;
		break;
	case OP_ICOMPARE_IMM:
		ins->type = bin_comp_table [src1->type] [src1->type] ? STACK_I4 : STACK_INV;
		if ((src1->type == STACK_I8) || ((SIZEOF_REGISTER == 8) && ((src1->type == STACK_PTR) || (src1->type == STACK_OBJ) || (src1->type == STACK_MP))))
			ins->opcode = OP_LCOMPARE_IMM;		
		break;
	case CEE_BEQ:
	case CEE_BGE:
	case CEE_BGT:
	case CEE_BLE:
	case CEE_BLT:
	case CEE_BNE_UN:
	case CEE_BGE_UN:
	case CEE_BGT_UN:
	case CEE_BLE_UN:
	case CEE_BLT_UN:
		ins->opcode += beqops_op_map [src1->type];
		break;
	case OP_CEQ:
		ins->type = bin_comp_table [src1->type] [src2->type] ? STACK_I4: STACK_INV;
		ins->opcode += ceqops_op_map [src1->type];
		break;
	case OP_CGT:
	case OP_CGT_UN:
	case OP_CLT:
	case OP_CLT_UN:
		ins->type = (bin_comp_table [src1->type] [src2->type] & 1) ? STACK_I4: STACK_INV;
		ins->opcode += ceqops_op_map [src1->type];
		break;
	/* unops */
	case CEE_NEG:
		ins->type = neg_table [src1->type];
		ins->opcode += unops_op_map [ins->type];
		break;
	case CEE_NOT:
		if (src1->type >= STACK_I4 && src1->type <= STACK_PTR)
			ins->type = src1->type;
		else
			ins->type = STACK_INV;
		ins->opcode += unops_op_map [ins->type];
		break;
	case CEE_CONV_I1:
	case CEE_CONV_I2:
	case CEE_CONV_I4:
	case CEE_CONV_U4:
		ins->type = STACK_I4;
		ins->opcode += unops_op_map [src1->type];
		break;
	case CEE_CONV_R_UN:
		ins->type = STACK_R8;
		switch (src1->type) {
		case STACK_I4:
		case STACK_PTR:
			ins->opcode = OP_ICONV_TO_R_UN;
			break;
		case STACK_I8:
			ins->opcode = OP_LCONV_TO_R_UN; 
			break;
		}
		break;
	case CEE_CONV_OVF_I1:
	case CEE_CONV_OVF_U1:
	case CEE_CONV_OVF_I2:
	case CEE_CONV_OVF_U2:
	case CEE_CONV_OVF_I4:
	case CEE_CONV_OVF_U4:
		ins->type = STACK_I4;
		ins->opcode += ovf3ops_op_map [src1->type];
		break;
	case CEE_CONV_OVF_I_UN:
	case CEE_CONV_OVF_U_UN:
		ins->type = STACK_PTR;
		ins->opcode += ovf2ops_op_map [src1->type];
		break;
	case CEE_CONV_OVF_I1_UN:
	case CEE_CONV_OVF_I2_UN:
	case CEE_CONV_OVF_I4_UN:
	case CEE_CONV_OVF_U1_UN:
	case CEE_CONV_OVF_U2_UN:
	case CEE_CONV_OVF_U4_UN:
		ins->type = STACK_I4;
		ins->opcode += ovf2ops_op_map [src1->type];
		break;
	case CEE_CONV_U:
		ins->type = STACK_PTR;
		switch (src1->type) {
		case STACK_I4:
			ins->opcode = OP_ICONV_TO_U;
			break;
		case STACK_PTR:
		case STACK_MP:
#if SIZEOF_REGISTER == 8
			ins->opcode = OP_LCONV_TO_U;
#else
			ins->opcode = OP_MOVE;
#endif
			break;
		case STACK_I8:
			ins->opcode = OP_LCONV_TO_U;
			break;
		case STACK_R8:
			ins->opcode = OP_FCONV_TO_U;
			break;
		}
		break;
	case CEE_CONV_I8:
	case CEE_CONV_U8:
		ins->type = STACK_I8;
		ins->opcode += unops_op_map [src1->type];
		break;
	case CEE_CONV_OVF_I8:
	case CEE_CONV_OVF_U8:
		ins->type = STACK_I8;
		ins->opcode += ovf3ops_op_map [src1->type];
		break;
	case CEE_CONV_OVF_U8_UN:
	case CEE_CONV_OVF_I8_UN:
		ins->type = STACK_I8;
		ins->opcode += ovf2ops_op_map [src1->type];
		break;
	case CEE_CONV_R4:
	case CEE_CONV_R8:
		ins->type = STACK_R8;
		ins->opcode += unops_op_map [src1->type];
		break;
	case OP_CKFINITE:
		ins->type = STACK_R8;		
		break;
	case CEE_CONV_U2:
	case CEE_CONV_U1:
		ins->type = STACK_I4;
		ins->opcode += ovfops_op_map [src1->type];
		break;
	case CEE_CONV_I:
	case CEE_CONV_OVF_I:
	case CEE_CONV_OVF_U:
		ins->type = STACK_PTR;
		ins->opcode += ovfops_op_map [src1->type];
		break;
	case CEE_ADD_OVF:
	case CEE_ADD_OVF_UN:
	case CEE_MUL_OVF:
	case CEE_MUL_OVF_UN:
	case CEE_SUB_OVF:
	case CEE_SUB_OVF_UN:
		ins->type = bin_num_table [src1->type] [src2->type];
		ins->opcode += ovfops_op_map [src1->type];
		if (ins->type == STACK_R8)
			ins->type = STACK_INV;
		break;
	case OP_LOAD_MEMBASE:
		ins->type = STACK_PTR;
		break;
	case OP_LOADI1_MEMBASE:
	case OP_LOADU1_MEMBASE:
	case OP_LOADI2_MEMBASE:
	case OP_LOADU2_MEMBASE:
	case OP_LOADI4_MEMBASE:
	case OP_LOADU4_MEMBASE:
		ins->type = STACK_PTR;
		break;
	case OP_LOADI8_MEMBASE:
		ins->type = STACK_I8;
		break;
	case OP_LOADR4_MEMBASE:
	case OP_LOADR8_MEMBASE:
		ins->type = STACK_R8;
		break;
	default:
		g_error ("opcode 0x%04x not handled in type from op", ins->opcode);
		break;
	}

	if (ins->type == STACK_MP)
		ins->klass = mono_defaults.object_class;
}

static const char 
ldind_type [] = {
	STACK_I4, STACK_I4, STACK_I4, STACK_I4, STACK_I4, STACK_I4, STACK_I8, STACK_PTR, STACK_R8, STACK_R8, STACK_OBJ
};

#if 0

static const char
param_table [STACK_MAX] [STACK_MAX] = {
	{0},
};

static int
check_values_to_signature (MonoInst *args, MonoType *this, MonoMethodSignature *sig) {
	int i;

	if (sig->hasthis) {
		switch (args->type) {
		case STACK_I4:
		case STACK_I8:
		case STACK_R8:
		case STACK_VTYPE:
		case STACK_INV:
			return 0;
		}
		args++;
	}
	for (i = 0; i < sig->param_count; ++i) {
		switch (args [i].type) {
		case STACK_INV:
			return 0;
		case STACK_MP:
			if (!sig->params [i]->byref)
				return 0;
			continue;
		case STACK_OBJ:
			if (sig->params [i]->byref)
				return 0;
			switch (sig->params [i]->type) {
			case MONO_TYPE_CLASS:
			case MONO_TYPE_STRING:
			case MONO_TYPE_OBJECT:
			case MONO_TYPE_SZARRAY:
			case MONO_TYPE_ARRAY:
				break;
			default:
				return 0;
			}
			continue;
		case STACK_R8:
			if (sig->params [i]->byref)
				return 0;
			if (sig->params [i]->type != MONO_TYPE_R4 && sig->params [i]->type != MONO_TYPE_R8)
				return 0;
			continue;
		case STACK_PTR:
		case STACK_I4:
		case STACK_I8:
		case STACK_VTYPE:
			break;
		}
		/*if (!param_table [args [i].type] [sig->params [i]->type])
			return 0;*/
	}
	return 1;
}
#endif

/*
 * When we need a pointer to the current domain many times in a method, we
 * call mono_domain_get() once and we store the result in a local variable.
 * This function returns the variable that represents the MonoDomain*.
 */
inline static MonoInst *
mono_get_domainvar (MonoCompile *cfg)
{
	if (!cfg->domainvar)
		cfg->domainvar = mono_compile_create_var (cfg, &mono_defaults.int_class->byval_arg, OP_LOCAL);
	return cfg->domainvar;
}

/*
 * The got_var contains the address of the Global Offset Table when AOT 
 * compiling.
 */
inline static MonoInst *
mono_get_got_var (MonoCompile *cfg)
{
#ifdef MONO_ARCH_NEED_GOT_VAR
	if (!cfg->compile_aot)
		return NULL;
	if (!cfg->got_var) {
		cfg->got_var = mono_compile_create_var (cfg, &mono_defaults.int_class->byval_arg, OP_LOCAL);
	}
	return cfg->got_var;
#else
	return NULL;
#endif
}

static MonoInst *
mono_get_vtable_var (MonoCompile *cfg)
{
	g_assert (cfg->generic_sharing_context);

	if (!cfg->rgctx_var) {
		cfg->rgctx_var = mono_compile_create_var (cfg, &mono_defaults.int_class->byval_arg, OP_LOCAL);
		/* force the var to be stack allocated */
		cfg->rgctx_var->flags |= MONO_INST_INDIRECT;
	}

	return cfg->rgctx_var;
}

static MonoType*
type_from_stack_type (MonoInst *ins) {
	switch (ins->type) {
	case STACK_I4: return &mono_defaults.int32_class->byval_arg;
	case STACK_I8: return &mono_defaults.int64_class->byval_arg;
	case STACK_PTR: return &mono_defaults.int_class->byval_arg;
	case STACK_R8: return &mono_defaults.double_class->byval_arg;
	case STACK_MP:
		return &ins->klass->this_arg;
	case STACK_OBJ: return &mono_defaults.object_class->byval_arg;
	case STACK_VTYPE: return &ins->klass->byval_arg;
	default:
		g_error ("stack type %d to monotype not handled\n", ins->type);
	}
	return NULL;
}

static G_GNUC_UNUSED int
type_to_stack_type (MonoType *t)
{
	switch (mono_type_get_underlying_type (t)->type) {
	case MONO_TYPE_I1:
	case MONO_TYPE_U1:
	case MONO_TYPE_BOOLEAN:
	case MONO_TYPE_I2:
	case MONO_TYPE_U2:
	case MONO_TYPE_CHAR:
	case MONO_TYPE_I4:
	case MONO_TYPE_U4:
		return STACK_I4;
	case MONO_TYPE_I:
	case MONO_TYPE_U:
	case MONO_TYPE_PTR:
	case MONO_TYPE_FNPTR:
		return STACK_PTR;
	case MONO_TYPE_CLASS:
	case MONO_TYPE_STRING:
	case MONO_TYPE_OBJECT:
	case MONO_TYPE_SZARRAY:
	case MONO_TYPE_ARRAY:    
		return STACK_OBJ;
	case MONO_TYPE_I8:
	case MONO_TYPE_U8:
		return STACK_I8;
	case MONO_TYPE_R4:
	case MONO_TYPE_R8:
		return STACK_R8;
	case MONO_TYPE_VALUETYPE:
	case MONO_TYPE_TYPEDBYREF:
		return STACK_VTYPE;
	case MONO_TYPE_GENERICINST:
		if (mono_type_generic_inst_is_valuetype (t))
			return STACK_VTYPE;
		else
			return STACK_OBJ;
		break;
	default:
		g_assert_not_reached ();
	}

	return -1;
}

static MonoClass*
array_access_to_klass (int opcode)
{
	switch (opcode) {
	case CEE_LDELEM_U1:
		return mono_defaults.byte_class;
	case CEE_LDELEM_U2:
		return mono_defaults.uint16_class;
	case CEE_LDELEM_I:
	case CEE_STELEM_I:
		return mono_defaults.int_class;
	case CEE_LDELEM_I1:
	case CEE_STELEM_I1:
		return mono_defaults.sbyte_class;
	case CEE_LDELEM_I2:
	case CEE_STELEM_I2:
		return mono_defaults.int16_class;
	case CEE_LDELEM_I4:
	case CEE_STELEM_I4:
		return mono_defaults.int32_class;
	case CEE_LDELEM_U4:
		return mono_defaults.uint32_class;
	case CEE_LDELEM_I8:
	case CEE_STELEM_I8:
		return mono_defaults.int64_class;
	case CEE_LDELEM_R4:
	case CEE_STELEM_R4:
		return mono_defaults.single_class;
	case CEE_LDELEM_R8:
	case CEE_STELEM_R8:
		return mono_defaults.double_class;
	case CEE_LDELEM_REF:
	case CEE_STELEM_REF:
		return mono_defaults.object_class;
	default:
		g_assert_not_reached ();
	}
	return NULL;
}

/*
 * We try to share variables when possible
 */
static MonoInst *
mono_compile_get_interface_var (MonoCompile *cfg, int slot, MonoInst *ins)
{
	MonoInst *res;
	int pos, vnum;

	/* inlining can result in deeper stacks */ 
	if (slot >= mono_method_get_header (cfg->method)->max_stack)
		return mono_compile_create_var (cfg, type_from_stack_type (ins), OP_LOCAL);

	pos = ins->type - 1 + slot * STACK_MAX;

	switch (ins->type) {
	case STACK_I4:
	case STACK_I8:
	case STACK_R8:
	case STACK_PTR:
	case STACK_MP:
	case STACK_OBJ:
		if ((vnum = cfg->intvars [pos]))
			return cfg->varinfo [vnum];
		res = mono_compile_create_var (cfg, type_from_stack_type (ins), OP_LOCAL);
		cfg->intvars [pos] = res->inst_c0;
		break;
	default:
		res = mono_compile_create_var (cfg, type_from_stack_type (ins), OP_LOCAL);
	}
	return res;
}

static void
mono_save_token_info (MonoCompile *cfg, MonoImage *image, guint32 token, gpointer key)
{
	/* 
	 * Don't use this if a generic_context is set, since that means AOT can't
	 * look up the method using just the image+token.
	 * table == 0 means this is a reference made from a wrapper.
	 */
	if (cfg->compile_aot && !cfg->generic_context && (mono_metadata_token_table (token) > 0)) {
		MonoJumpInfoToken *jump_info_token = mono_mempool_alloc0 (cfg->mempool, sizeof (MonoJumpInfoToken));
		jump_info_token->image = image;
		jump_info_token->token = token;
		g_hash_table_insert (cfg->token_info_hash, key, jump_info_token);
	}
}

/*
 * This function is called to handle items that are left on the evaluation stack
 * at basic block boundaries. What happens is that we save the values to local variables
 * and we reload them later when first entering the target basic block (with the
 * handle_loaded_temps () function).
 * A single joint point will use the same variables (stored in the array bb->out_stack or
 * bb->in_stack, if the basic block is before or after the joint point).
 *
 * This function needs to be called _before_ emitting the last instruction of
 * the bb (i.e. before emitting a branch).
 * If the stack merge fails at a join point, cfg->unverifiable is set.
 */
static void
handle_stack_args (MonoCompile *cfg, MonoInst **sp, int count)
{
	int i, bindex;
	MonoBasicBlock *bb = cfg->cbb;
	MonoBasicBlock *outb;
	MonoInst *inst, **locals;
	gboolean found;

	if (!count)
		return;
	if (cfg->verbose_level > 3)
		printf ("%d item(s) on exit from B%d\n", count, bb->block_num);
	if (!bb->out_scount) {
		bb->out_scount = count;
		//printf ("bblock %d has out:", bb->block_num);
		found = FALSE;
		for (i = 0; i < bb->out_count; ++i) {
			outb = bb->out_bb [i];
			/* exception handlers are linked, but they should not be considered for stack args */
			if (outb->flags & BB_EXCEPTION_HANDLER)
				continue;
			//printf (" %d", outb->block_num);
			if (outb->in_stack) {
				found = TRUE;
				bb->out_stack = outb->in_stack;
				break;
			}
		}
		//printf ("\n");
		if (!found) {
			bb->out_stack = mono_mempool_alloc (cfg->mempool, sizeof (MonoInst*) * count);
			for (i = 0; i < count; ++i) {
				/* 
				 * try to reuse temps already allocated for this purpouse, if they occupy the same
				 * stack slot and if they are of the same type.
				 * This won't cause conflicts since if 'local' is used to 
				 * store one of the values in the in_stack of a bblock, then
				 * the same variable will be used for the same outgoing stack 
				 * slot as well. 
				 * This doesn't work when inlining methods, since the bblocks
				 * in the inlined methods do not inherit their in_stack from
				 * the bblock they are inlined to. See bug #58863 for an
				 * example.
				 */
				if (cfg->inlined_method)
					bb->out_stack [i] = mono_compile_create_var (cfg, type_from_stack_type (sp [i]), OP_LOCAL);
				else
					bb->out_stack [i] = mono_compile_get_interface_var (cfg, i, sp [i]);
			}
		}
	}

	for (i = 0; i < bb->out_count; ++i) {
		outb = bb->out_bb [i];
		/* exception handlers are linked, but they should not be considered for stack args */
		if (outb->flags & BB_EXCEPTION_HANDLER)
			continue;
		if (outb->in_scount) {
			if (outb->in_scount != bb->out_scount) {
				cfg->unverifiable = TRUE;
				return;
			}
			continue; /* check they are the same locals */
		}
		outb->in_scount = count;
		outb->in_stack = bb->out_stack;
	}

	locals = bb->out_stack;
	cfg->cbb = bb;
	for (i = 0; i < count; ++i) {
		EMIT_NEW_TEMPSTORE (cfg, inst, locals [i]->inst_c0, sp [i]);
		inst->cil_code = sp [i]->cil_code;
		sp [i] = locals [i];
		if (cfg->verbose_level > 3)
			printf ("storing %d to temp %d\n", i, (int)locals [i]->inst_c0);
	}

	/*
	 * It is possible that the out bblocks already have in_stack assigned, and
	 * the in_stacks differ. In this case, we will store to all the different 
	 * in_stacks.
	 */

	found = TRUE;
	bindex = 0;
	while (found) {
		/* Find a bblock which has a different in_stack */
		found = FALSE;
		while (bindex < bb->out_count) {
			outb = bb->out_bb [bindex];
			/* exception handlers are linked, but they should not be considered for stack args */
			if (outb->flags & BB_EXCEPTION_HANDLER) {
				bindex++;
				continue;
			}
			if (outb->in_stack != locals) {
				for (i = 0; i < count; ++i) {
					EMIT_NEW_TEMPSTORE (cfg, inst, outb->in_stack [i]->inst_c0, sp [i]);
					inst->cil_code = sp [i]->cil_code;
					sp [i] = locals [i];
					if (cfg->verbose_level > 3)
						printf ("storing %d to temp %d\n", i, (int)outb->in_stack [i]->inst_c0);
				}
				locals = outb->in_stack;
				found = TRUE;
				break;
			}
			bindex ++;
		}
	}
}

/* Emit code which loads interface_offsets [klass->interface_id]
 * The array is stored in memory before vtable.
*/
static void
mini_emit_load_intf_reg_vtable (MonoCompile *cfg, int intf_reg, int vtable_reg, MonoClass *klass)
{
	if (cfg->compile_aot) {
		int ioffset_reg = alloc_preg (cfg);
		int iid_reg = alloc_preg (cfg);

		MONO_EMIT_NEW_AOTCONST (cfg, iid_reg, klass, MONO_PATCH_INFO_ADJUSTED_IID);
		MONO_EMIT_NEW_BIALU (cfg, OP_PADD, ioffset_reg, iid_reg, vtable_reg);
		MONO_EMIT_NEW_LOAD_MEMBASE (cfg, intf_reg, ioffset_reg, 0);
	}
	else {
		MONO_EMIT_NEW_LOAD_MEMBASE (cfg, intf_reg, vtable_reg, -((klass->interface_id + 1) * SIZEOF_VOID_P));
	}
}

/* 
 * Emit code which loads into "intf_bit_reg" a nonzero value if the MonoClass
 * stored in "klass_reg" implements the interface "klass".
 */
static void
mini_emit_load_intf_bit_reg_class (MonoCompile *cfg, int intf_bit_reg, int klass_reg, MonoClass *klass)
{
	int ibitmap_reg = alloc_preg (cfg);
	int ibitmap_byte_reg = alloc_preg (cfg);

	MONO_EMIT_NEW_LOAD_MEMBASE (cfg, ibitmap_reg, klass_reg, G_STRUCT_OFFSET (MonoClass, interface_bitmap));

	if (cfg->compile_aot) {
		int iid_reg = alloc_preg (cfg);
		int shifted_iid_reg = alloc_preg (cfg);
		int ibitmap_byte_address_reg = alloc_preg (cfg);
		int masked_iid_reg = alloc_preg (cfg);
		int iid_one_bit_reg = alloc_preg (cfg);
		int iid_bit_reg = alloc_preg (cfg);
		MONO_EMIT_NEW_AOTCONST (cfg, iid_reg, klass, MONO_PATCH_INFO_IID);
		MONO_EMIT_NEW_BIALU_IMM (cfg, OP_SHR_IMM, shifted_iid_reg, iid_reg, 3);
		MONO_EMIT_NEW_BIALU (cfg, OP_PADD, ibitmap_byte_address_reg, ibitmap_reg, shifted_iid_reg);
		MONO_EMIT_NEW_LOAD_MEMBASE_OP (cfg, OP_LOADU1_MEMBASE, ibitmap_byte_reg, ibitmap_byte_address_reg, 0);
		MONO_EMIT_NEW_BIALU_IMM (cfg, OP_IAND_IMM, masked_iid_reg, iid_reg, 7);
		MONO_EMIT_NEW_ICONST (cfg, iid_one_bit_reg, 1);
		MONO_EMIT_NEW_BIALU (cfg, OP_ISHL, iid_bit_reg, iid_one_bit_reg, masked_iid_reg);
		MONO_EMIT_NEW_BIALU (cfg, OP_IAND, intf_bit_reg, ibitmap_byte_reg, iid_bit_reg);
	} else {
		MONO_EMIT_NEW_LOAD_MEMBASE_OP (cfg, OP_LOADI1_MEMBASE, ibitmap_byte_reg, ibitmap_reg, klass->interface_id >> 3);
		MONO_EMIT_NEW_BIALU_IMM (cfg, OP_AND_IMM, intf_bit_reg, ibitmap_byte_reg, 1 << (klass->interface_id & 7));
	}
}

/* 
 * Emit code which loads into "intf_bit_reg" a nonzero value if the MonoVTable
 * stored in "vtable_reg" implements the interface "klass".
 */
static void
mini_emit_load_intf_bit_reg_vtable (MonoCompile *cfg, int intf_bit_reg, int vtable_reg, MonoClass *klass)
{
	int ibitmap_reg = alloc_preg (cfg);
	int ibitmap_byte_reg = alloc_preg (cfg);
 
	MONO_EMIT_NEW_LOAD_MEMBASE (cfg, ibitmap_reg, vtable_reg, G_STRUCT_OFFSET (MonoVTable, interface_bitmap));

	if (cfg->compile_aot) {
		int iid_reg = alloc_preg (cfg);
		int shifted_iid_reg = alloc_preg (cfg);
		int ibitmap_byte_address_reg = alloc_preg (cfg);
		int masked_iid_reg = alloc_preg (cfg);
		int iid_one_bit_reg = alloc_preg (cfg);
		int iid_bit_reg = alloc_preg (cfg);
		MONO_EMIT_NEW_AOTCONST (cfg, iid_reg, klass, MONO_PATCH_INFO_IID);
		MONO_EMIT_NEW_BIALU_IMM (cfg, OP_ISHR_IMM, shifted_iid_reg, iid_reg, 3);
		MONO_EMIT_NEW_BIALU (cfg, OP_PADD, ibitmap_byte_address_reg, ibitmap_reg, shifted_iid_reg);
		MONO_EMIT_NEW_LOAD_MEMBASE_OP (cfg, OP_LOADU1_MEMBASE, ibitmap_byte_reg, ibitmap_byte_address_reg, 0);
		MONO_EMIT_NEW_BIALU_IMM (cfg, OP_AND_IMM, masked_iid_reg, iid_reg, 7);
		MONO_EMIT_NEW_ICONST (cfg, iid_one_bit_reg, 1);
		MONO_EMIT_NEW_BIALU (cfg, OP_ISHL, iid_bit_reg, iid_one_bit_reg, masked_iid_reg);
		MONO_EMIT_NEW_BIALU (cfg, OP_IAND, intf_bit_reg, ibitmap_byte_reg, iid_bit_reg);
	} else {
		MONO_EMIT_NEW_LOAD_MEMBASE_OP (cfg, OP_LOADI1_MEMBASE, ibitmap_byte_reg, ibitmap_reg, klass->interface_id >> 3);
		MONO_EMIT_NEW_BIALU_IMM (cfg, OP_IAND_IMM, intf_bit_reg, ibitmap_byte_reg, 1 << (klass->interface_id & 7));
	}
}

/* 
 * Emit code which checks whenever the interface id of @klass is smaller than
 * than the value given by max_iid_reg.
*/
static void
mini_emit_max_iid_check (MonoCompile *cfg, int max_iid_reg, MonoClass *klass,
						 MonoBasicBlock *false_target)
{
	if (cfg->compile_aot) {
		int iid_reg = alloc_preg (cfg);
		MONO_EMIT_NEW_AOTCONST (cfg, iid_reg, klass, MONO_PATCH_INFO_IID);
		MONO_EMIT_NEW_BIALU (cfg, OP_COMPARE, -1, max_iid_reg, iid_reg);
	}
	else
		MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, max_iid_reg, klass->interface_id);
	if (false_target)
		MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_PBLT_UN, false_target);
	else
		MONO_EMIT_NEW_COND_EXC (cfg, LT_UN, "InvalidCastException");
}

/* Same as above, but obtains max_iid from a vtable */
static void
mini_emit_max_iid_check_vtable (MonoCompile *cfg, int vtable_reg, MonoClass *klass,
								 MonoBasicBlock *false_target)
{
	int max_iid_reg = alloc_preg (cfg);
		
	MONO_EMIT_NEW_LOAD_MEMBASE_OP (cfg, OP_LOADU2_MEMBASE, max_iid_reg, vtable_reg, G_STRUCT_OFFSET (MonoVTable, max_interface_id));
	mini_emit_max_iid_check (cfg, max_iid_reg, klass, false_target);
}

/* Same as above, but obtains max_iid from a klass */
static void
mini_emit_max_iid_check_class (MonoCompile *cfg, int klass_reg, MonoClass *klass,
								 MonoBasicBlock *false_target)
{
	int max_iid_reg = alloc_preg (cfg);

 	MONO_EMIT_NEW_LOAD_MEMBASE_OP (cfg, OP_LOADU2_MEMBASE, max_iid_reg, klass_reg, G_STRUCT_OFFSET (MonoClass, max_interface_id));		
	mini_emit_max_iid_check (cfg, max_iid_reg, klass, false_target);
}

static void 
mini_emit_isninst_cast (MonoCompile *cfg, int klass_reg, MonoClass *klass, MonoBasicBlock *false_target, MonoBasicBlock *true_target)
{
	int idepth_reg = alloc_preg (cfg);
	int stypes_reg = alloc_preg (cfg);
	int stype = alloc_preg (cfg);

	if (klass->idepth > MONO_DEFAULT_SUPERTABLE_SIZE) {
		MONO_EMIT_NEW_LOAD_MEMBASE_OP (cfg, OP_LOADU2_MEMBASE, idepth_reg, klass_reg, G_STRUCT_OFFSET (MonoClass, idepth));
		MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, idepth_reg, klass->idepth);
		MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_PBLT_UN, false_target);
	}
	MONO_EMIT_NEW_LOAD_MEMBASE (cfg, stypes_reg, klass_reg, G_STRUCT_OFFSET (MonoClass, supertypes));
	MONO_EMIT_NEW_LOAD_MEMBASE (cfg, stype, stypes_reg, ((klass->idepth - 1) * SIZEOF_VOID_P));
	if (cfg->compile_aot) {
		int const_reg = alloc_preg (cfg);
		MONO_EMIT_NEW_CLASSCONST (cfg, const_reg, klass);
		MONO_EMIT_NEW_BIALU (cfg, OP_COMPARE, -1, stype, const_reg);
	} else {
		MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, stype, klass);
	}
	MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_PBEQ, true_target);
}

static void 
mini_emit_iface_cast (MonoCompile *cfg, int vtable_reg, MonoClass *klass, MonoBasicBlock *false_target, MonoBasicBlock *true_target)
{
	int intf_reg = alloc_preg (cfg);

	mini_emit_max_iid_check_vtable (cfg, vtable_reg, klass, false_target);
	mini_emit_load_intf_bit_reg_vtable (cfg, intf_reg, vtable_reg, klass);
	MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, intf_reg, 0);
	if (true_target)
		MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_PBNE_UN, true_target);
	else
		MONO_EMIT_NEW_COND_EXC (cfg, EQ, "InvalidCastException");		
}

/*
 * Variant of the above that takes a register to the class, not the vtable.
 */
static void 
mini_emit_iface_class_cast (MonoCompile *cfg, int klass_reg, MonoClass *klass, MonoBasicBlock *false_target, MonoBasicBlock *true_target)
{
	int intf_bit_reg = alloc_preg (cfg);

	mini_emit_max_iid_check_class (cfg, klass_reg, klass, false_target);
	mini_emit_load_intf_bit_reg_class (cfg, intf_bit_reg, klass_reg, klass);
	MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, intf_bit_reg, 0);
	if (true_target)
		MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_PBNE_UN, true_target);
	else
		MONO_EMIT_NEW_COND_EXC (cfg, EQ, "InvalidCastException");
}

static inline void
mini_emit_class_check (MonoCompile *cfg, int klass_reg, MonoClass *klass)
{
	if (cfg->compile_aot) {
		int const_reg = alloc_preg (cfg);
		MONO_EMIT_NEW_CLASSCONST (cfg, const_reg, klass);
		MONO_EMIT_NEW_BIALU (cfg, OP_COMPARE, -1, klass_reg, const_reg);
	} else {
		MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, klass_reg, klass);
	}
	MONO_EMIT_NEW_COND_EXC (cfg, NE_UN, "InvalidCastException");
}

static inline void
mini_emit_class_check_branch (MonoCompile *cfg, int klass_reg, MonoClass *klass, int branch_op, MonoBasicBlock *target)
{
	if (cfg->compile_aot) {
		int const_reg = alloc_preg (cfg);
		MONO_EMIT_NEW_CLASSCONST (cfg, const_reg, klass);
		MONO_EMIT_NEW_BIALU (cfg, OP_COMPARE, -1, klass_reg, const_reg);
	} else {
		MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, klass_reg, klass);
	}
	MONO_EMIT_NEW_BRANCH_BLOCK (cfg, branch_op, target);
}
	
static void 
mini_emit_castclass (MonoCompile *cfg, int obj_reg, int klass_reg, MonoClass *klass, MonoBasicBlock *object_is_null)
{
	if (klass->rank) {
		int rank_reg = alloc_preg (cfg);
		int eclass_reg = alloc_preg (cfg);

		MONO_EMIT_NEW_LOAD_MEMBASE_OP (cfg, OP_LOADU1_MEMBASE, rank_reg, klass_reg, G_STRUCT_OFFSET (MonoClass, rank));
		MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, rank_reg, klass->rank);
		MONO_EMIT_NEW_COND_EXC (cfg, NE_UN, "InvalidCastException");
		//		MONO_EMIT_NEW_LOAD_MEMBASE (cfg, klass_reg, vtable_reg, G_STRUCT_OFFSET (MonoVTable, klass));
		MONO_EMIT_NEW_LOAD_MEMBASE (cfg, eclass_reg, klass_reg, G_STRUCT_OFFSET (MonoClass, cast_class));
		if (klass->cast_class == mono_defaults.object_class) {
			int parent_reg = alloc_preg (cfg);
			MONO_EMIT_NEW_LOAD_MEMBASE (cfg, parent_reg, eclass_reg, G_STRUCT_OFFSET (MonoClass, parent));
			mini_emit_class_check_branch (cfg, parent_reg, mono_defaults.enum_class->parent, OP_PBNE_UN, object_is_null);
			mini_emit_class_check (cfg, eclass_reg, mono_defaults.enum_class);
		} else if (klass->cast_class == mono_defaults.enum_class->parent) {
			mini_emit_class_check_branch (cfg, eclass_reg, mono_defaults.enum_class->parent, OP_PBEQ, object_is_null);
			mini_emit_class_check (cfg, eclass_reg, mono_defaults.enum_class);
		} else if (klass->cast_class == mono_defaults.enum_class) {
			mini_emit_class_check (cfg, eclass_reg, mono_defaults.enum_class);
		} else if (klass->cast_class->flags & TYPE_ATTRIBUTE_INTERFACE) {
			mini_emit_iface_class_cast (cfg, eclass_reg, klass->cast_class, NULL, NULL);
		} else {
			// Pass -1 as obj_reg to skip the check below for arrays of arrays
			mini_emit_castclass (cfg, -1, eclass_reg, klass->cast_class, object_is_null);
		}

		if ((klass->rank == 1) && (klass->byval_arg.type == MONO_TYPE_SZARRAY) && (obj_reg != -1)) {
			/* Check that the object is a vector too */
			int bounds_reg = alloc_preg (cfg);
			MONO_EMIT_NEW_LOAD_MEMBASE (cfg, bounds_reg, obj_reg, G_STRUCT_OFFSET (MonoArray, bounds));
			MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, bounds_reg, 0);
			MONO_EMIT_NEW_COND_EXC (cfg, NE_UN, "InvalidCastException");
		}
	} else {
		int idepth_reg = alloc_preg (cfg);
		int stypes_reg = alloc_preg (cfg);
		int stype = alloc_preg (cfg);

		if (klass->idepth > MONO_DEFAULT_SUPERTABLE_SIZE) {
			MONO_EMIT_NEW_LOAD_MEMBASE_OP (cfg, OP_LOADU2_MEMBASE, idepth_reg, klass_reg, G_STRUCT_OFFSET (MonoClass, idepth));
			MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, idepth_reg, klass->idepth);
			MONO_EMIT_NEW_COND_EXC (cfg, LT_UN, "InvalidCastException");
		}
		MONO_EMIT_NEW_LOAD_MEMBASE (cfg, stypes_reg, klass_reg, G_STRUCT_OFFSET (MonoClass, supertypes));
		MONO_EMIT_NEW_LOAD_MEMBASE (cfg, stype, stypes_reg, ((klass->idepth - 1) * SIZEOF_VOID_P));
		mini_emit_class_check (cfg, stype, klass);
	}
}

static void 
mini_emit_memset (MonoCompile *cfg, int destreg, int offset, int size, int val, int align)
{
	int val_reg;

	g_assert (val == 0);

	if (align == 0)
		align = 4;

	if ((size <= 4) && (size <= align)) {
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

	if (align < 4) {
		/* This could be optimized further if neccesary */
		while (size >= 1) {
			MONO_EMIT_NEW_STORE_MEMBASE (cfg, OP_STOREI1_MEMBASE_REG, destreg, offset, val_reg);
			offset += 1;
			size -= 1;
		}
		return;
	}	

#if !NO_UNALIGNED_ACCESS
	if (SIZEOF_REGISTER == 8) {
		if (offset % 8) {
			MONO_EMIT_NEW_STORE_MEMBASE (cfg, OP_STOREI4_MEMBASE_REG, destreg, offset, val_reg);
			offset += 4;
			size -= 4;
		}
		while (size >= 8) {
			MONO_EMIT_NEW_STORE_MEMBASE (cfg, OP_STOREI8_MEMBASE_REG, destreg, offset, val_reg);
			offset += 8;
			size -= 8;
		}
	}	
#endif

	while (size >= 4) {
		MONO_EMIT_NEW_STORE_MEMBASE (cfg, OP_STOREI4_MEMBASE_REG, destreg, offset, val_reg);
		offset += 4;
		size -= 4;
	}
	while (size >= 2) {
		MONO_EMIT_NEW_STORE_MEMBASE (cfg, OP_STOREI2_MEMBASE_REG, destreg, offset, val_reg);
		offset += 2;
		size -= 2;
	}
	while (size >= 1) {
		MONO_EMIT_NEW_STORE_MEMBASE (cfg, OP_STOREI1_MEMBASE_REG, destreg, offset, val_reg);
		offset += 1;
		size -= 1;
	}
}

#endif /* DISABLE_JIT */

void 
mini_emit_memcpy (MonoCompile *cfg, int destreg, int doffset, int srcreg, int soffset, int size, int align)
{
	int cur_reg;

	if (align == 0)
		align = 4;

	if (align < 4) {
		/* This could be optimized further if neccesary */
		while (size >= 1) {
			cur_reg = alloc_preg (cfg);
			MONO_EMIT_NEW_LOAD_MEMBASE_OP (cfg, OP_LOADI1_MEMBASE, cur_reg, srcreg, soffset);
			MONO_EMIT_NEW_STORE_MEMBASE (cfg, OP_STOREI1_MEMBASE_REG, destreg, doffset, cur_reg);
			doffset += 1;
			soffset += 1;
			size -= 1;
		}
	}

#if !NO_UNALIGNED_ACCESS
	if (SIZEOF_REGISTER == 8) {
		while (size >= 8) {
			cur_reg = alloc_preg (cfg);
			MONO_EMIT_NEW_LOAD_MEMBASE_OP (cfg, OP_LOADI8_MEMBASE, cur_reg, srcreg, soffset);
			MONO_EMIT_NEW_STORE_MEMBASE (cfg, OP_STOREI8_MEMBASE_REG, destreg, doffset, cur_reg);
			doffset += 8;
			soffset += 8;
			size -= 8;
		}
	}	
#endif

	while (size >= 4) {
		cur_reg = alloc_preg (cfg);
		MONO_EMIT_NEW_LOAD_MEMBASE_OP (cfg, OP_LOADI4_MEMBASE, cur_reg, srcreg, soffset);
		MONO_EMIT_NEW_STORE_MEMBASE (cfg, OP_STOREI4_MEMBASE_REG, destreg, doffset, cur_reg);
		doffset += 4;
		soffset += 4;
		size -= 4;
	}
	while (size >= 2) {
		cur_reg = alloc_preg (cfg);
		MONO_EMIT_NEW_LOAD_MEMBASE_OP (cfg, OP_LOADI2_MEMBASE, cur_reg, srcreg, soffset);
		MONO_EMIT_NEW_STORE_MEMBASE (cfg, OP_STOREI2_MEMBASE_REG, destreg, doffset, cur_reg);
		doffset += 2;
		soffset += 2;
		size -= 2;
	}
	while (size >= 1) {
		cur_reg = alloc_preg (cfg);
		MONO_EMIT_NEW_LOAD_MEMBASE_OP (cfg, OP_LOADI1_MEMBASE, cur_reg, srcreg, soffset);
		MONO_EMIT_NEW_STORE_MEMBASE (cfg, OP_STOREI1_MEMBASE_REG, destreg, doffset, cur_reg);
		doffset += 1;
		soffset += 1;
		size -= 1;
	}
}

#ifndef DISABLE_JIT

static int
ret_type_to_call_opcode (MonoType *type, int calli, int virt, MonoGenericSharingContext *gsctx)
{
	if (type->byref)
		return calli? OP_CALL_REG: virt? OP_CALLVIRT: OP_CALL;

handle_enum:
	type = mini_get_basic_type_from_generic (gsctx, type);
	switch (type->type) {
	case MONO_TYPE_VOID:
		return calli? OP_VOIDCALL_REG: virt? OP_VOIDCALLVIRT: OP_VOIDCALL;
	case MONO_TYPE_I1:
	case MONO_TYPE_U1:
	case MONO_TYPE_BOOLEAN:
	case MONO_TYPE_I2:
	case MONO_TYPE_U2:
	case MONO_TYPE_CHAR:
	case MONO_TYPE_I4:
	case MONO_TYPE_U4:
		return calli? OP_CALL_REG: virt? OP_CALLVIRT: OP_CALL;
	case MONO_TYPE_I:
	case MONO_TYPE_U:
	case MONO_TYPE_PTR:
	case MONO_TYPE_FNPTR:
		return calli? OP_CALL_REG: virt? OP_CALLVIRT: OP_CALL;
	case MONO_TYPE_CLASS:
	case MONO_TYPE_STRING:
	case MONO_TYPE_OBJECT:
	case MONO_TYPE_SZARRAY:
	case MONO_TYPE_ARRAY:    
		return calli? OP_CALL_REG: virt? OP_CALLVIRT: OP_CALL;
	case MONO_TYPE_I8:
	case MONO_TYPE_U8:
		return calli? OP_LCALL_REG: virt? OP_LCALLVIRT: OP_LCALL;
	case MONO_TYPE_R4:
	case MONO_TYPE_R8:
		return calli? OP_FCALL_REG: virt? OP_FCALLVIRT: OP_FCALL;
	case MONO_TYPE_VALUETYPE:
		if (type->data.klass->enumtype) {
			type = type->data.klass->enum_basetype;
			goto handle_enum;
		} else
			return calli? OP_VCALL_REG: virt? OP_VCALLVIRT: OP_VCALL;
	case MONO_TYPE_TYPEDBYREF:
		return calli? OP_VCALL_REG: virt? OP_VCALLVIRT: OP_VCALL;
	case MONO_TYPE_GENERICINST:
		type = &type->data.generic_class->container_class->byval_arg;
		goto handle_enum;
	default:
		g_error ("unknown type 0x%02x in ret_type_to_call_opcode", type->type);
	}
	return -1;
}

/*
 * target_type_is_incompatible:
 * @cfg: MonoCompile context
 *
 * Check that the item @arg on the evaluation stack can be stored
 * in the target type (can be a local, or field, etc).
 * The cfg arg can be used to check if we need verification or just
 * validity checks.
 *
 * Returns: non-0 value if arg can't be stored on a target.
 */
static int
target_type_is_incompatible (MonoCompile *cfg, MonoType *target, MonoInst *arg)
{
	MonoType *simple_type;
	MonoClass *klass;

	if (target->byref) {
		/* FIXME: check that the pointed to types match */
		if (arg->type == STACK_MP)
			return arg->klass != mono_class_from_mono_type (target);
		if (arg->type == STACK_PTR)
			return 0;
		return 1;
	}

	simple_type = mono_type_get_underlying_type (target);
	switch (simple_type->type) {
	case MONO_TYPE_VOID:
		return 1;
	case MONO_TYPE_I1:
	case MONO_TYPE_U1:
	case MONO_TYPE_BOOLEAN:
	case MONO_TYPE_I2:
	case MONO_TYPE_U2:
	case MONO_TYPE_CHAR:
	case MONO_TYPE_I4:
	case MONO_TYPE_U4:
		if (arg->type != STACK_I4 && arg->type != STACK_PTR)
			return 1;
		return 0;
	case MONO_TYPE_PTR:
		/* STACK_MP is needed when setting pinned locals */
		if (arg->type != STACK_I4 && arg->type != STACK_PTR && arg->type != STACK_MP)
			return 1;
		return 0;
	case MONO_TYPE_I:
	case MONO_TYPE_U:
	case MONO_TYPE_FNPTR:
		if (arg->type != STACK_I4 && arg->type != STACK_PTR)
			return 1;
		return 0;
	case MONO_TYPE_CLASS:
	case MONO_TYPE_STRING:
	case MONO_TYPE_OBJECT:
	case MONO_TYPE_SZARRAY:
	case MONO_TYPE_ARRAY:    
		if (arg->type != STACK_OBJ)
			return 1;
		/* FIXME: check type compatibility */
		return 0;
	case MONO_TYPE_I8:
	case MONO_TYPE_U8:
		if (arg->type != STACK_I8)
			return 1;
		return 0;
	case MONO_TYPE_R4:
	case MONO_TYPE_R8:
		if (arg->type != STACK_R8)
			return 1;
		return 0;
	case MONO_TYPE_VALUETYPE:
		if (arg->type != STACK_VTYPE)
			return 1;
		klass = mono_class_from_mono_type (simple_type);
		if (klass != arg->klass)
			return 1;
		return 0;
	case MONO_TYPE_TYPEDBYREF:
		if (arg->type != STACK_VTYPE)
			return 1;
		klass = mono_class_from_mono_type (simple_type);
		if (klass != arg->klass)
			return 1;
		return 0;
	case MONO_TYPE_GENERICINST:
		if (mono_type_generic_inst_is_valuetype (simple_type)) {
			if (arg->type != STACK_VTYPE)
				return 1;
			klass = mono_class_from_mono_type (simple_type);
			if (klass != arg->klass)
				return 1;
			return 0;
		} else {
			if (arg->type != STACK_OBJ)
				return 1;
			/* FIXME: check type compatibility */
			return 0;
		}
	case MONO_TYPE_VAR:
	case MONO_TYPE_MVAR:
		/* FIXME: all the arguments must be references for now,
		 * later look inside cfg and see if the arg num is
		 * really a reference
		 */
		g_assert (cfg->generic_sharing_context);
		if (arg->type != STACK_OBJ)
			return 1;
		return 0;
	default:
		g_error ("unknown type 0x%02x in target_type_is_incompatible", simple_type->type);
	}
	return 1;
}

/*
 * Prepare arguments for passing to a function call.
 * Return a non-zero value if the arguments can't be passed to the given
 * signature.
 * The type checks are not yet complete and some conversions may need
 * casts on 32 or 64 bit architectures.
 *
 * FIXME: implement this using target_type_is_incompatible ()
 */
static int
check_call_signature (MonoCompile *cfg, MonoMethodSignature *sig, MonoInst **args)
{
	MonoType *simple_type;
	int i;

	if (sig->hasthis) {
		if (args [0]->type != STACK_OBJ && args [0]->type != STACK_MP && args [0]->type != STACK_PTR)
			return 1;
		args++;
	}
	for (i = 0; i < sig->param_count; ++i) {
		if (sig->params [i]->byref) {
			if (args [i]->type != STACK_MP && args [i]->type != STACK_PTR)
				return 1;
			continue;
		}
		simple_type = sig->params [i];
		simple_type = mini_get_basic_type_from_generic (cfg->generic_sharing_context, simple_type);
handle_enum:
		switch (simple_type->type) {
		case MONO_TYPE_VOID:
			return 1;
			continue;
		case MONO_TYPE_I1:
		case MONO_TYPE_U1:
		case MONO_TYPE_BOOLEAN:
		case MONO_TYPE_I2:
		case MONO_TYPE_U2:
		case MONO_TYPE_CHAR:
		case MONO_TYPE_I4:
		case MONO_TYPE_U4:
			if (args [i]->type != STACK_I4 && args [i]->type != STACK_PTR)
				return 1;
			continue;
		case MONO_TYPE_I:
		case MONO_TYPE_U:
		case MONO_TYPE_PTR:
		case MONO_TYPE_FNPTR:
			if (args [i]->type != STACK_I4 && args [i]->type != STACK_PTR && args [i]->type != STACK_MP && args [i]->type != STACK_OBJ)
				return 1;
			continue;
		case MONO_TYPE_CLASS:
		case MONO_TYPE_STRING:
		case MONO_TYPE_OBJECT:
		case MONO_TYPE_SZARRAY:
		case MONO_TYPE_ARRAY:    
			if (args [i]->type != STACK_OBJ)
				return 1;
			continue;
		case MONO_TYPE_I8:
		case MONO_TYPE_U8:
			if (args [i]->type != STACK_I8)
				return 1;
			continue;
		case MONO_TYPE_R4:
		case MONO_TYPE_R8:
			if (args [i]->type != STACK_R8)
				return 1;
			continue;
		case MONO_TYPE_VALUETYPE:
			if (simple_type->data.klass->enumtype) {
				simple_type = simple_type->data.klass->enum_basetype;
				goto handle_enum;
			}
			if (args [i]->type != STACK_VTYPE)
				return 1;
			continue;
		case MONO_TYPE_TYPEDBYREF:
			if (args [i]->type != STACK_VTYPE)
				return 1;
			continue;
		case MONO_TYPE_GENERICINST:
			simple_type = &simple_type->data.generic_class->container_class->byval_arg;
			goto handle_enum;

		default:
			g_error ("unknown type 0x%02x in check_call_signature",
				 simple_type->type);
		}
	}
	return 0;
}

static int
callvirt_to_call (int opcode)
{
	switch (opcode) {
	case OP_CALLVIRT:
		return OP_CALL;
	case OP_VOIDCALLVIRT:
		return OP_VOIDCALL;
	case OP_FCALLVIRT:
		return OP_FCALL;
	case OP_VCALLVIRT:
		return OP_VCALL;
	case OP_LCALLVIRT:
		return OP_LCALL;
	default:
		g_assert_not_reached ();
	}

	return -1;
}

static int
callvirt_to_call_membase (int opcode)
{
	switch (opcode) {
	case OP_CALLVIRT:
		return OP_CALL_MEMBASE;
	case OP_VOIDCALLVIRT:
		return OP_VOIDCALL_MEMBASE;
	case OP_FCALLVIRT:
		return OP_FCALL_MEMBASE;
	case OP_LCALLVIRT:
		return OP_LCALL_MEMBASE;
	case OP_VCALLVIRT:
		return OP_VCALL_MEMBASE;
	default:
		g_assert_not_reached ();
	}

	return -1;
}

#ifdef MONO_ARCH_HAVE_IMT
static void
emit_imt_argument (MonoCompile *cfg, MonoCallInst *call, MonoInst *imt_arg)
{
#ifdef MONO_ARCH_IMT_REG
	int method_reg = alloc_preg (cfg);

	if (imt_arg) {
		MONO_EMIT_NEW_UNALU (cfg, OP_MOVE, method_reg, imt_arg->dreg);
	} else if (cfg->compile_aot) {
		MONO_EMIT_NEW_AOTCONST (cfg, method_reg, call->method, MONO_PATCH_INFO_METHODCONST);
	} else {
		MonoInst *ins;
		MONO_INST_NEW (cfg, ins, OP_PCONST);
		ins->inst_p0 = call->method;
		ins->dreg = method_reg;
		MONO_ADD_INS (cfg->cbb, ins);
	}

	mono_call_inst_add_outarg_reg (cfg, call, method_reg, MONO_ARCH_IMT_REG, FALSE);
#else
	mono_arch_emit_imt_argument (cfg, call, imt_arg);
#endif
}
#endif

static MonoJumpInfo *
mono_patch_info_new (MonoMemPool *mp, int ip, MonoJumpInfoType type, gconstpointer target)
{
	MonoJumpInfo *ji = mono_mempool_alloc (mp, sizeof (MonoJumpInfo));

	ji->ip.i = ip;
	ji->type = type;
	ji->data.target = target;

	return ji;
}

inline static MonoInst*
mono_emit_jit_icall (MonoCompile *cfg, gconstpointer func, MonoInst **args);

inline static MonoCallInst *
mono_emit_call_args (MonoCompile *cfg, MonoMethodSignature *sig, 
		     MonoInst **args, int calli, int virtual)
{
	MonoCallInst *call;
#ifdef MONO_ARCH_SOFT_FLOAT
	int i;
#endif

	MONO_INST_NEW_CALL (cfg, call, ret_type_to_call_opcode (sig->ret, calli, virtual, cfg->generic_sharing_context));

	call->args = args;
	call->signature = sig;

	type_to_eval_stack_type ((cfg), sig->ret, &call->inst);

	if (MONO_TYPE_ISSTRUCT (sig->ret)) {
		MonoInst *temp = mono_compile_create_var (cfg, sig->ret, OP_LOCAL);
		MonoInst *loada;

		temp->backend.is_pinvoke = sig->pinvoke;

		/*
		 * We use a new opcode OP_OUTARG_VTRETADDR instead of LDADDR for emitting the
		 * address of return value to increase optimization opportunities.
		 * Before vtype decomposition, the dreg of the call ins itself represents the
		 * fact the call modifies the return value. After decomposition, the call will
		 * be transformed into one of the OP_VOIDCALL opcodes, and the VTRETADDR opcode
		 * will be transformed into an LDADDR.
		 */
		MONO_INST_NEW (cfg, loada, OP_OUTARG_VTRETADDR);
		loada->dreg = alloc_preg (cfg);
		loada->inst_p0 = temp;
		/* We reference the call too since call->dreg could change during optimization */
		loada->inst_p1 = call;
		MONO_ADD_INS (cfg->cbb, loada);

		call->inst.dreg = temp->dreg;

		call->vret_var = loada;
	} else if (!MONO_TYPE_IS_VOID (sig->ret))
		call->inst.dreg = alloc_dreg (cfg, call->inst.type);

#ifdef MONO_ARCH_SOFT_FLOAT
	/* 
	 * If the call has a float argument, we would need to do an r8->r4 conversion using 
	 * an icall, but that cannot be done during the call sequence since it would clobber
	 * the call registers + the stack. So we do it before emitting the call.
	 */
	for (i = 0; i < sig->param_count + sig->hasthis; ++i) {
		MonoType *t;
		MonoInst *in = call->args [i];

		if (i >= sig->hasthis)
			t = sig->params [i - sig->hasthis];
		else
			t = &mono_defaults.int_class->byval_arg;
		t = mono_type_get_underlying_type (t);

		if (!t->byref && t->type == MONO_TYPE_R4) {
			MonoInst *iargs [1];
			MonoInst *conv;

			iargs [0] = in;
			conv = mono_emit_jit_icall (cfg, mono_fload_r4_arg, iargs);

			/* The result will be in an int vreg */
			call->args [i] = conv;
		}
	}
#endif

	mono_arch_emit_call (cfg, call);

	cfg->param_area = MAX (cfg->param_area, call->stack_usage);
	cfg->flags |= MONO_CFG_HAS_CALLS;
	
	return call;
}

inline static MonoInst*
mono_emit_calli (MonoCompile *cfg, MonoMethodSignature *sig, MonoInst **args, MonoInst *addr)
{
	MonoCallInst *call = mono_emit_call_args (cfg, sig, args, TRUE, FALSE);

	call->inst.sreg1 = addr->dreg;

	MONO_ADD_INS (cfg->cbb, (MonoInst*)call);

	return (MonoInst*)call;
}

inline static MonoInst*
mono_emit_rgctx_calli (MonoCompile *cfg, MonoMethodSignature *sig, MonoInst **args, MonoInst *addr, MonoInst *rgctx_arg)
{
#ifdef MONO_ARCH_RGCTX_REG
	MonoCallInst *call;
	int rgctx_reg = -1;

	if (rgctx_arg) {
		rgctx_reg = mono_alloc_preg (cfg);
		MONO_EMIT_NEW_UNALU (cfg, OP_MOVE, rgctx_reg, rgctx_arg->dreg);
	}
	call = (MonoCallInst*)mono_emit_calli (cfg, sig, args, addr);
	if (rgctx_arg) {
		mono_call_inst_add_outarg_reg (cfg, call, rgctx_reg, MONO_ARCH_RGCTX_REG, FALSE);
		cfg->uses_rgctx_reg = TRUE;
	}
	return (MonoInst*)call;
#else
	g_assert_not_reached ();
	return NULL;
#endif
}

static MonoInst*
emit_get_rgctx_method (MonoCompile *cfg, int context_used, MonoMethod *cmethod, int rgctx_type);

static MonoInst*
mono_emit_method_call_full (MonoCompile *cfg, MonoMethod *method, MonoMethodSignature *sig,
							MonoInst **args, MonoInst *this, MonoInst *imt_arg)
{
	gboolean might_be_remote;
	gboolean virtual = this != NULL;
	gboolean enable_for_aot = TRUE;
	int context_used;
	MonoCallInst *call;

	if (method->string_ctor) {
		/* Create the real signature */
		/* FIXME: Cache these */
		MonoMethodSignature *ctor_sig = mono_metadata_signature_dup_full (cfg->mempool, sig);
		ctor_sig->ret = &mono_defaults.string_class->byval_arg;

		sig = ctor_sig;
	}

	might_be_remote = this && sig->hasthis &&
		(method->klass->marshalbyref || method->klass == mono_defaults.object_class) &&
		!(method->flags & METHOD_ATTRIBUTE_VIRTUAL) && !MONO_CHECK_THIS (this);

	context_used = mono_method_check_context_used (method);
	if (might_be_remote && context_used) {
		MonoInst *addr;

		g_assert (cfg->generic_sharing_context);

		addr = emit_get_rgctx_method (cfg, context_used, method, MONO_RGCTX_INFO_REMOTING_INVOKE_WITH_CHECK);

		return mono_emit_calli (cfg, sig, args, addr);
	}

	call = mono_emit_call_args (cfg, sig, args, FALSE, virtual);

	if (might_be_remote) {
		if (mono_method_check_context_used (method)) {
			g_assert (cfg->generic_sharing_context);
			return NULL;
		}
		call->method = mono_marshal_get_remoting_invoke_with_check (method);
	} else {
		call->method = method;
	}
	call->inst.flags |= MONO_INST_HAS_METHOD;
	call->inst.inst_left = this;

	if (virtual) {
		int vtable_reg, slot_reg, this_reg;

		this_reg = this->dreg;

#ifdef MONO_ARCH_HAVE_CREATE_DELEGATE_TRAMPOLINE
		if ((method->klass->parent == mono_defaults.multicastdelegate_class) && (!strcmp (method->name, "Invoke"))) {
			/* Make a call to delegate->invoke_impl */
			call->inst.opcode = callvirt_to_call_membase (call->inst.opcode);
			call->inst.inst_basereg = this_reg;
			call->inst.inst_offset = G_STRUCT_OFFSET (MonoDelegate, invoke_impl);
			MONO_ADD_INS (cfg->cbb, (MonoInst*)call);

			return (MonoInst*)call;
		}
#endif

		if ((!cfg->compile_aot || enable_for_aot) && 
			(!(method->flags & METHOD_ATTRIBUTE_VIRTUAL) || 
			 (MONO_METHOD_IS_FINAL (method) &&
			  method->wrapper_type != MONO_WRAPPER_REMOTING_INVOKE_WITH_CHECK))) {
			/* 
			 * the method is not virtual, we just need to ensure this is not null
			 * and then we can call the method directly.
			 */
			if (method->klass->marshalbyref || method->klass == mono_defaults.object_class) {
				method = call->method = mono_marshal_get_remoting_invoke_with_check (method);
			}

			if (!method->string_ctor) {
				cfg->flags |= MONO_CFG_HAS_CHECK_THIS;
				MONO_EMIT_NEW_UNALU (cfg, OP_CHECK_THIS, -1, this_reg);
				MONO_EMIT_NEW_UNALU (cfg, OP_NOT_NULL, -1, this_reg);
			}

			call->inst.opcode = callvirt_to_call (call->inst.opcode);

			MONO_ADD_INS (cfg->cbb, (MonoInst*)call);

			return (MonoInst*)call;
		}

		if ((method->flags & METHOD_ATTRIBUTE_VIRTUAL) && MONO_METHOD_IS_FINAL (method)) {
			/*
			 * the method is virtual, but we can statically dispatch since either
			 * it's class or the method itself are sealed.
			 * But first we need to ensure it's not a null reference.
			 */
			cfg->flags |= MONO_CFG_HAS_CHECK_THIS;
			MONO_EMIT_NEW_UNALU (cfg, OP_CHECK_THIS, -1, this_reg);
			MONO_EMIT_NEW_UNALU (cfg, OP_NOT_NULL, -1, this_reg);

			call->inst.opcode = callvirt_to_call (call->inst.opcode);
			MONO_ADD_INS (cfg->cbb, (MonoInst*)call);

			return (MonoInst*)call;
		}

		call->inst.opcode = callvirt_to_call_membase (call->inst.opcode);

		vtable_reg = alloc_preg (cfg);
		MONO_EMIT_NEW_LOAD_MEMBASE (cfg, vtable_reg, this_reg, G_STRUCT_OFFSET (MonoObject, vtable));
		if (method->klass->flags & TYPE_ATTRIBUTE_INTERFACE) {
			slot_reg = -1;
#ifdef MONO_ARCH_HAVE_IMT
			if (mono_use_imt) {
				guint32 imt_slot = mono_method_get_imt_slot (method);
				emit_imt_argument (cfg, call, imt_arg);
				slot_reg = vtable_reg;
				call->inst.inst_offset = ((gint32)imt_slot - MONO_IMT_SIZE) * SIZEOF_VOID_P;
			}
#endif
			if (slot_reg == -1) {
				slot_reg = alloc_preg (cfg);
				mini_emit_load_intf_reg_vtable (cfg, slot_reg, vtable_reg, method->klass);
				call->inst.inst_offset = mono_method_get_vtable_index (method) * SIZEOF_VOID_P;
			}
		} else {
			slot_reg = vtable_reg;
			call->inst.inst_offset = G_STRUCT_OFFSET (MonoVTable, vtable) +
				(mono_method_get_vtable_index (method) * SIZEOF_VOID_P);
#ifdef MONO_ARCH_HAVE_IMT
			if (imt_arg) {
				g_assert (mono_method_signature (method)->generic_param_count);
				emit_imt_argument (cfg, call, imt_arg);
			}
#endif
		}

		call->inst.sreg1 = slot_reg;
		call->virtual = TRUE;
	}

	MONO_ADD_INS (cfg->cbb, (MonoInst*)call);

	return (MonoInst*)call;
}

static MonoInst*
mono_emit_rgctx_method_call_full (MonoCompile *cfg, MonoMethod *method, MonoMethodSignature *sig,
		MonoInst **args, MonoInst *this, MonoInst *imt_arg, MonoInst *vtable_arg)
{
	int rgctx_reg;
	MonoInst *ins;
	MonoCallInst *call;

	if (vtable_arg) {
#ifdef MONO_ARCH_RGCTX_REG
		rgctx_reg = mono_alloc_preg (cfg);
		MONO_EMIT_NEW_UNALU (cfg, OP_MOVE, rgctx_reg, vtable_arg->dreg);
#else
		NOT_IMPLEMENTED;
#endif
	}
	ins = mono_emit_method_call_full (cfg, method, sig, args, this, imt_arg);
	if (!ins)
		return NULL;

	call = (MonoCallInst*)ins;
	if (vtable_arg) {
#ifdef MONO_ARCH_RGCTX_REG
		mono_call_inst_add_outarg_reg (cfg, call, rgctx_reg, MONO_ARCH_RGCTX_REG, FALSE);
		cfg->uses_rgctx_reg = TRUE;
#else
		NOT_IMPLEMENTED;
#endif
	}

	return ins;
}

static inline MonoInst*
mono_emit_method_call (MonoCompile *cfg, MonoMethod *method, MonoInst **args, MonoInst *this)
{
	return mono_emit_method_call_full (cfg, method, mono_method_signature (method), args, this, NULL);
}

MonoInst*
mono_emit_native_call (MonoCompile *cfg, gconstpointer func, MonoMethodSignature *sig,
					   MonoInst **args)
{
	MonoCallInst *call;

	g_assert (sig);

	call = mono_emit_call_args (cfg, sig, args, FALSE, FALSE);
	call->fptr = func;

	MONO_ADD_INS (cfg->cbb, (MonoInst*)call);

	return (MonoInst*)call;
}

inline static MonoInst*
mono_emit_jit_icall (MonoCompile *cfg, gconstpointer func, MonoInst **args)
{
	MonoJitICallInfo *info = mono_find_jit_icall_by_addr (func);

	g_assert (info);

	return mono_emit_native_call (cfg, mono_icall_get_wrapper (info), info->sig, args);
}

/*
 * mono_emit_abs_call:
 *
 *   Emit a call to the runtime function described by PATCH_TYPE and DATA.
 */
inline static MonoInst*
mono_emit_abs_call (MonoCompile *cfg, MonoJumpInfoType patch_type, gconstpointer data, 
					MonoMethodSignature *sig, MonoInst **args)
{
	MonoJumpInfo *ji = mono_patch_info_new (cfg->mempool, 0, patch_type, data);
	MonoInst *ins;

	/* 
	 * We pass ji as the call address, the PATCH_INFO_ABS resolving code will
	 * handle it.
	 */
	if (cfg->abs_patches == NULL)
		cfg->abs_patches = g_hash_table_new (NULL, NULL);
	g_hash_table_insert (cfg->abs_patches, ji, ji);
	ins = mono_emit_native_call (cfg, ji, sig, args);
	((MonoCallInst*)ins)->fptr_is_patch = TRUE;
	return ins;
}

static MonoMethod*
get_memcpy_method (void)
{
	static MonoMethod *memcpy_method = NULL;
	if (!memcpy_method) {
		memcpy_method = mono_class_get_method_from_name (mono_defaults.string_class, "memcpy", 3);
		if (!memcpy_method)
			g_error ("Old corlib found. Install a new one");
	}
	return memcpy_method;
}

/*
 * Emit code to copy a valuetype of type @klass whose address is stored in
 * @src->dreg to memory whose address is stored at @dest->dreg.
 */
void
mini_emit_stobj (MonoCompile *cfg, MonoInst *dest, MonoInst *src, MonoClass *klass, gboolean native)
{
	MonoInst *iargs [3];
	int n;
	guint32 align = 0;
	MonoMethod *memcpy_method;

	g_assert (klass);
	/*
	 * This check breaks with spilled vars... need to handle it during verification anyway.
	 * g_assert (klass && klass == src->klass && klass == dest->klass);
	 */

	if (native)
		n = mono_class_native_size (klass, &align);
	else
		n = mono_class_value_size (klass, &align);

#if HAVE_WRITE_BARRIERS
	/* if native is true there should be no references in the struct */
	if (klass->has_references && !native) {
		/* Avoid barriers when storing to the stack */
		if (!((dest->opcode == OP_ADD_IMM && dest->sreg1 == cfg->frame_reg) ||
			  (dest->opcode == OP_LDADDR))) {
			iargs [0] = dest;
			iargs [1] = src;
			EMIT_NEW_PCONST (cfg, iargs [2], klass);

			mono_emit_jit_icall (cfg, mono_value_copy, iargs);
		}
	}
#endif

	if ((cfg->opt & MONO_OPT_INTRINS) && n <= sizeof (gpointer) * 5) {
		/* FIXME: Optimize the case when src/dest is OP_LDADDR */
		mini_emit_memcpy (cfg, dest->dreg, 0, src->dreg, 0, n, align);
	} else {
		iargs [0] = dest;
		iargs [1] = src;
		EMIT_NEW_ICONST (cfg, iargs [2], n);
		
		memcpy_method = get_memcpy_method ();
		mono_emit_method_call (cfg, memcpy_method, iargs, NULL);
	}
}

static MonoMethod*
get_memset_method (void)
{
	static MonoMethod *memset_method = NULL;
	if (!memset_method) {
		memset_method = mono_class_get_method_from_name (mono_defaults.string_class, "memset", 3);
		if (!memset_method)
			g_error ("Old corlib found. Install a new one");
	}
	return memset_method;
}

void
mini_emit_initobj (MonoCompile *cfg, MonoInst *dest, const guchar *ip, MonoClass *klass)
{
	MonoInst *iargs [3];
	int n;
	guint32 align;
	MonoMethod *memset_method;

	/* FIXME: Optimize this for the case when dest is an LDADDR */

	mono_class_init (klass);
	n = mono_class_value_size (klass, &align);

	if (n <= sizeof (gpointer) * 5) {
		mini_emit_memset (cfg, dest->dreg, 0, n, 0, align);
	}
	else {
		memset_method = get_memset_method ();
		iargs [0] = dest;
		EMIT_NEW_ICONST (cfg, iargs [1], 0);
		EMIT_NEW_ICONST (cfg, iargs [2], n);
		mono_emit_method_call (cfg, memset_method, iargs, NULL);
	}
}

static MonoInst*
emit_get_rgctx (MonoCompile *cfg, MonoMethod *method, int context_used)
{
	MonoInst *this = NULL;

	g_assert (cfg->generic_sharing_context);

	if (!(method->flags & METHOD_ATTRIBUTE_STATIC) &&
			!(context_used & MONO_GENERIC_CONTEXT_USED_METHOD) &&
			!method->klass->valuetype)
		EMIT_NEW_ARGLOAD (cfg, this, 0);

	if (context_used & MONO_GENERIC_CONTEXT_USED_METHOD) {
		MonoInst *mrgctx_loc, *mrgctx_var;

		g_assert (!this);
		g_assert (method->is_inflated && mono_method_get_context (method)->method_inst);

		mrgctx_loc = mono_get_vtable_var (cfg);
		EMIT_NEW_TEMPLOAD (cfg, mrgctx_var, mrgctx_loc->inst_c0);

		return mrgctx_var;
	} else if (method->flags & METHOD_ATTRIBUTE_STATIC || method->klass->valuetype) {
		MonoInst *vtable_loc, *vtable_var;

		g_assert (!this);

		vtable_loc = mono_get_vtable_var (cfg);
		EMIT_NEW_TEMPLOAD (cfg, vtable_var, vtable_loc->inst_c0);

		if (method->is_inflated && mono_method_get_context (method)->method_inst) {
			MonoInst *mrgctx_var = vtable_var;
			int vtable_reg;

			vtable_reg = alloc_preg (cfg);
			EMIT_NEW_LOAD_MEMBASE (cfg, vtable_var, OP_LOAD_MEMBASE, vtable_reg, mrgctx_var->dreg, G_STRUCT_OFFSET (MonoMethodRuntimeGenericContext, class_vtable));
			vtable_var->type = STACK_PTR;
		}

		return vtable_var;
	} else {
		MonoInst *ins;
		int vtable_reg, res_reg;
	
		vtable_reg = alloc_preg (cfg);
		res_reg = alloc_preg (cfg);
		EMIT_NEW_LOAD_MEMBASE (cfg, ins, OP_LOAD_MEMBASE, vtable_reg, this->dreg, G_STRUCT_OFFSET (MonoObject, vtable));
		return ins;
	}
}

static MonoJumpInfoRgctxEntry *
mono_patch_info_rgctx_entry_new (MonoMemPool *mp, MonoMethod *method, gboolean in_mrgctx, MonoJumpInfoType patch_type, gconstpointer patch_data, int info_type)
{
	MonoJumpInfoRgctxEntry *res = mono_mempool_alloc0 (mp, sizeof (MonoJumpInfoRgctxEntry));
	res->method = method;
	res->in_mrgctx = in_mrgctx;
	res->data = mono_mempool_alloc0 (mp, sizeof (MonoJumpInfo));
	res->data->type = patch_type;
	res->data->data.target = patch_data;
	res->info_type = info_type;

	return res;
}

static inline MonoInst*
emit_rgctx_fetch (MonoCompile *cfg, MonoInst *rgctx, MonoJumpInfoRgctxEntry *entry)
{
	return mono_emit_abs_call (cfg, MONO_PATCH_INFO_RGCTX_FETCH, entry, helper_sig_rgctx_lazy_fetch_trampoline, &rgctx);
}

static MonoInst*
emit_get_rgctx_klass (MonoCompile *cfg, int context_used,
					  MonoClass *klass, int rgctx_type)
{
	MonoJumpInfoRgctxEntry *entry = mono_patch_info_rgctx_entry_new (cfg->mempool, cfg->current_method, context_used & MONO_GENERIC_CONTEXT_USED_METHOD, MONO_PATCH_INFO_CLASS, klass, rgctx_type);
	MonoInst *rgctx = emit_get_rgctx (cfg, cfg->current_method, context_used);

	return emit_rgctx_fetch (cfg, rgctx, entry);
}

static MonoInst*
emit_get_rgctx_method (MonoCompile *cfg, int context_used,
					   MonoMethod *cmethod, int rgctx_type)
{
	MonoJumpInfoRgctxEntry *entry = mono_patch_info_rgctx_entry_new (cfg->mempool, cfg->current_method, context_used & MONO_GENERIC_CONTEXT_USED_METHOD, MONO_PATCH_INFO_METHODCONST, cmethod, rgctx_type);
	MonoInst *rgctx = emit_get_rgctx (cfg, cfg->current_method, context_used);

	return emit_rgctx_fetch (cfg, rgctx, entry);
}

static MonoInst*
emit_get_rgctx_field (MonoCompile *cfg, int context_used,
					  MonoClassField *field, int rgctx_type)
{
	MonoJumpInfoRgctxEntry *entry = mono_patch_info_rgctx_entry_new (cfg->mempool, cfg->current_method, context_used & MONO_GENERIC_CONTEXT_USED_METHOD, MONO_PATCH_INFO_FIELD, field, rgctx_type);
	MonoInst *rgctx = emit_get_rgctx (cfg, cfg->current_method, context_used);

	return emit_rgctx_fetch (cfg, rgctx, entry);
}

static void
emit_generic_class_init (MonoCompile *cfg, MonoClass *klass)
{
	MonoInst *vtable_arg;
	MonoCallInst *call;
	int context_used = 0;

	if (cfg->generic_sharing_context)
		context_used = mono_class_check_context_used (klass);

	if (context_used) {
		vtable_arg = emit_get_rgctx_klass (cfg, context_used,
										   klass, MONO_RGCTX_INFO_VTABLE);
	} else {
		MonoVTable *vtable = mono_class_vtable (cfg->domain, klass);

		if (!vtable)
			return;
		EMIT_NEW_VTABLECONST (cfg, vtable_arg, vtable);
	}

	call = (MonoCallInst*)mono_emit_abs_call (cfg, MONO_PATCH_INFO_GENERIC_CLASS_INIT, NULL, helper_sig_generic_class_init_trampoline, &vtable_arg);
#ifdef MONO_ARCH_VTABLE_REG
	mono_call_inst_add_outarg_reg (cfg, call, vtable_arg->dreg, MONO_ARCH_VTABLE_REG, FALSE);
	cfg->uses_vtable_reg = TRUE;
#else
	NOT_IMPLEMENTED;
#endif
}

static void
mini_emit_check_array_type (MonoCompile *cfg, MonoInst *obj, MonoClass *array_class)
{
	int vtable_reg = alloc_preg (cfg);
	int context_used = 0;

	if (cfg->generic_sharing_context)
		context_used = mono_class_check_context_used (array_class);

	MONO_EMIT_NEW_LOAD_MEMBASE (cfg, vtable_reg, obj->dreg, G_STRUCT_OFFSET (MonoObject, vtable));
				       
	if (cfg->opt & MONO_OPT_SHARED) {
		int class_reg = alloc_preg (cfg);
		MONO_EMIT_NEW_LOAD_MEMBASE (cfg, class_reg, vtable_reg, G_STRUCT_OFFSET (MonoVTable, klass));
		if (cfg->compile_aot) {
			int klass_reg = alloc_preg (cfg);
			MONO_EMIT_NEW_CLASSCONST (cfg, klass_reg, array_class);
			MONO_EMIT_NEW_BIALU (cfg, OP_COMPARE, -1, class_reg, klass_reg);
		} else {
			MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, class_reg, array_class);
		}
	} else if (context_used) {
		MonoInst *vtable_ins;

		vtable_ins = emit_get_rgctx_klass (cfg, context_used, array_class, MONO_RGCTX_INFO_VTABLE);
		MONO_EMIT_NEW_BIALU (cfg, OP_COMPARE, -1, vtable_reg, vtable_ins->dreg);
	} else {
		if (cfg->compile_aot) {
			int vt_reg = alloc_preg (cfg);
			MONO_EMIT_NEW_VTABLECONST (cfg, vt_reg, mono_class_vtable (cfg->domain, array_class));
			MONO_EMIT_NEW_BIALU (cfg, OP_COMPARE, -1, vtable_reg, vt_reg);
		} else {
			MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, vtable_reg, mono_class_vtable (cfg->domain, array_class));
		}
	}
	
	MONO_EMIT_NEW_COND_EXC (cfg, NE_UN, "ArrayTypeMismatchException");
}

static void
save_cast_details (MonoCompile *cfg, MonoClass *klass, int obj_reg)
{
	if (mini_get_debug_options ()->better_cast_details) {
		int to_klass_reg = alloc_preg (cfg);
		int vtable_reg = alloc_preg (cfg);
		int klass_reg = alloc_preg (cfg);
		MonoInst *tls_get = mono_get_jit_tls_intrinsic (cfg);

		if (!tls_get) {
			fprintf (stderr, "error: --debug=casts not supported on this platform.\n.");
			exit (1);
		}

		MONO_ADD_INS (cfg->cbb, tls_get);
		MONO_EMIT_NEW_LOAD_MEMBASE (cfg, vtable_reg, obj_reg, G_STRUCT_OFFSET (MonoObject, vtable));
		MONO_EMIT_NEW_LOAD_MEMBASE (cfg, klass_reg, vtable_reg, G_STRUCT_OFFSET (MonoVTable, klass));

		MONO_EMIT_NEW_STORE_MEMBASE (cfg, OP_STORE_MEMBASE_REG, tls_get->dreg, G_STRUCT_OFFSET (MonoJitTlsData, class_cast_from), klass_reg);
		MONO_EMIT_NEW_PCONST (cfg, to_klass_reg, klass);
		MONO_EMIT_NEW_STORE_MEMBASE (cfg, OP_STORE_MEMBASE_REG, tls_get->dreg, G_STRUCT_OFFSET (MonoJitTlsData, class_cast_to), to_klass_reg);
	}
}

static void
reset_cast_details (MonoCompile *cfg)
{
	/* Reset the variables holding the cast details */
	if (mini_get_debug_options ()->better_cast_details) {
		MonoInst *tls_get = mono_get_jit_tls_intrinsic (cfg);

		MONO_ADD_INS (cfg->cbb, tls_get);
		/* It is enough to reset the from field */
		MONO_EMIT_NEW_STORE_MEMBASE_IMM (cfg, OP_STORE_MEMBASE_IMM, tls_get->dreg, G_STRUCT_OFFSET (MonoJitTlsData, class_cast_from), 0);
	}
}

/**
 * Handles unbox of a Nullable<T>. If context_used is non zero, then shared 
 * generic code is generated.
 */
static MonoInst*
handle_unbox_nullable (MonoCompile* cfg, MonoInst* val, MonoClass* klass, int context_used)
{
	MonoMethod* method = mono_class_get_method_from_name (klass, "Unbox", 1);

	if (context_used) {
		MonoInst *rgctx, *addr;

		/* FIXME: What if the class is shared?  We might not
		   have to get the address of the method from the
		   RGCTX. */
		addr = emit_get_rgctx_method (cfg, context_used, method,
									  MONO_RGCTX_INFO_GENERIC_METHOD_CODE);

		rgctx = emit_get_rgctx (cfg, method, context_used);

		return mono_emit_rgctx_calli (cfg, mono_method_signature (method), &val, addr, rgctx);
	} else {
		return mono_emit_method_call (cfg, method, &val, NULL);
	}
}

static MonoInst*
handle_unbox (MonoCompile *cfg, MonoClass *klass, MonoInst **sp, int context_used)
{
	MonoInst *add;
	int obj_reg;
	int vtable_reg = alloc_dreg (cfg ,STACK_PTR);
	int klass_reg = alloc_dreg (cfg ,STACK_PTR);
	int eclass_reg = alloc_dreg (cfg ,STACK_PTR);
	int rank_reg = alloc_dreg (cfg ,STACK_I4);

	obj_reg = sp [0]->dreg;
	MONO_EMIT_NEW_LOAD_MEMBASE (cfg, vtable_reg, obj_reg, G_STRUCT_OFFSET (MonoObject, vtable));
	MONO_EMIT_NEW_LOAD_MEMBASE_OP (cfg, OP_LOADU1_MEMBASE, rank_reg, vtable_reg, G_STRUCT_OFFSET (MonoVTable, rank));

	/* FIXME: generics */
	g_assert (klass->rank == 0);
			
	// Check rank == 0
	MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, rank_reg, 0);
	MONO_EMIT_NEW_COND_EXC (cfg, NE_UN, "InvalidCastException");

	MONO_EMIT_NEW_LOAD_MEMBASE (cfg, klass_reg, vtable_reg, G_STRUCT_OFFSET (MonoVTable, klass));
	MONO_EMIT_NEW_LOAD_MEMBASE (cfg, eclass_reg, klass_reg, G_STRUCT_OFFSET (MonoClass, element_class));

	if (context_used) {
		MonoInst *element_class;

		/* This assertion is from the unboxcast insn */
		g_assert (klass->rank == 0);

		element_class = emit_get_rgctx_klass (cfg, context_used,
				klass->element_class, MONO_RGCTX_INFO_KLASS);

		MONO_EMIT_NEW_BIALU (cfg, OP_COMPARE, -1, eclass_reg, element_class->dreg);
		MONO_EMIT_NEW_COND_EXC (cfg, NE_UN, "InvalidCastException");
	} else {
		save_cast_details (cfg, klass->element_class, obj_reg);
		mini_emit_class_check (cfg, eclass_reg, klass->element_class);
		reset_cast_details (cfg);
	}

	NEW_BIALU_IMM (cfg, add, OP_ADD_IMM, alloc_dreg (cfg, STACK_PTR), obj_reg, sizeof (MonoObject));
	MONO_ADD_INS (cfg->cbb, add);
	add->type = STACK_MP;
	add->klass = klass;

	return add;
}

static MonoInst*
handle_alloc (MonoCompile *cfg, MonoClass *klass, gboolean for_box)
{
	MonoInst *iargs [2];
	void *alloc_ftn;

	if (cfg->opt & MONO_OPT_SHARED) {
		EMIT_NEW_DOMAINCONST (cfg, iargs [0]);
		EMIT_NEW_CLASSCONST (cfg, iargs [1], klass);

		alloc_ftn = mono_object_new;
	} else if (cfg->compile_aot && cfg->cbb->out_of_line && klass->type_token && klass->image == mono_defaults.corlib && !klass->generic_class) {
		/* This happens often in argument checking code, eg. throw new FooException... */
		/* Avoid relocations and save some space by calling a helper function specialized to mscorlib */
		EMIT_NEW_ICONST (cfg, iargs [0], mono_metadata_token_index (klass->type_token));
		return mono_emit_jit_icall (cfg, mono_helper_newobj_mscorlib, iargs);
	} else {
		MonoVTable *vtable = mono_class_vtable (cfg->domain, klass);
		MonoMethod *managed_alloc = mono_gc_get_managed_allocator (vtable, for_box);
		gboolean pass_lw;

		if (managed_alloc) {
			EMIT_NEW_VTABLECONST (cfg, iargs [0], vtable);
			return mono_emit_method_call (cfg, managed_alloc, iargs, NULL);
		}
		alloc_ftn = mono_class_get_allocation_ftn (vtable, for_box, &pass_lw);
		if (pass_lw) {
			guint32 lw = vtable->klass->instance_size;
			lw = ((lw + (sizeof (gpointer) - 1)) & ~(sizeof (gpointer) - 1)) / sizeof (gpointer);
			EMIT_NEW_ICONST (cfg, iargs [0], lw);
			EMIT_NEW_VTABLECONST (cfg, iargs [1], vtable);
		}
		else {
			EMIT_NEW_VTABLECONST (cfg, iargs [0], vtable);
		}
	}

	return mono_emit_jit_icall (cfg, alloc_ftn, iargs);
}

static MonoInst*
handle_alloc_from_inst (MonoCompile *cfg, MonoClass *klass, MonoInst *data_inst,
						gboolean for_box)
{
	MonoInst *iargs [2];
	MonoMethod *managed_alloc = NULL;
	void *alloc_ftn;

	/*
	  FIXME: we cannot get managed_alloc here because we can't get
	  the class's vtable (because it's not a closed class)

	MonoVTable *vtable = mono_class_vtable (cfg->domain, klass);
	MonoMethod *managed_alloc = mono_gc_get_managed_allocator (vtable, for_box);
	*/

	if (cfg->opt & MONO_OPT_SHARED) {
		EMIT_NEW_DOMAINCONST (cfg, iargs [0]);
		iargs [1] = data_inst;
		alloc_ftn = mono_object_new;
	} else {
		if (managed_alloc) {
			iargs [0] = data_inst;
			return mono_emit_method_call (cfg, managed_alloc, iargs, NULL);
		}

		iargs [0] = data_inst;
		alloc_ftn = mono_object_new_specific;
	}

	return mono_emit_jit_icall (cfg, alloc_ftn, iargs);
}
	
static MonoInst*
handle_box (MonoCompile *cfg, MonoInst *val, MonoClass *klass)
{
	MonoInst *alloc, *ins;

	if (mono_class_is_nullable (klass)) {
		MonoMethod* method = mono_class_get_method_from_name (klass, "Box", 1);
		return mono_emit_method_call (cfg, method, &val, NULL);
	}

	alloc = handle_alloc (cfg, klass, TRUE);

	EMIT_NEW_STORE_MEMBASE_TYPE (cfg, ins, &klass->byval_arg, alloc->dreg, sizeof (MonoObject), val->dreg);

	return alloc;
}

static MonoInst *
handle_box_from_inst (MonoCompile *cfg, MonoInst *val, MonoClass *klass, int context_used, MonoInst *data_inst)
{
	MonoInst *alloc, *ins;

	if (mono_class_is_nullable (klass)) {
		MonoMethod* method = mono_class_get_method_from_name (klass, "Box", 1);
		/* FIXME: What if the class is shared?  We might not
		   have to get the method address from the RGCTX. */
		MonoInst *addr = emit_get_rgctx_method (cfg, context_used, method,
			MONO_RGCTX_INFO_GENERIC_METHOD_CODE);
		MonoInst *rgctx = emit_get_rgctx (cfg, cfg->current_method, context_used);

		return mono_emit_rgctx_calli (cfg, mono_method_signature (method), &val, addr, rgctx);
	} else {
		alloc = handle_alloc_from_inst (cfg, klass, data_inst, TRUE);

		EMIT_NEW_STORE_MEMBASE_TYPE (cfg, ins, &klass->byval_arg, alloc->dreg, sizeof (MonoObject), val->dreg);

		return alloc;
	}
}

static MonoInst*
handle_castclass (MonoCompile *cfg, MonoClass *klass, MonoInst *src)
{
	MonoBasicBlock *is_null_bb;
	int obj_reg = src->dreg;
	int vtable_reg = alloc_preg (cfg);

	NEW_BBLOCK (cfg, is_null_bb);

	MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, obj_reg, 0);
	MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_PBEQ, is_null_bb);

	save_cast_details (cfg, klass, obj_reg);

	if (klass->flags & TYPE_ATTRIBUTE_INTERFACE) {
		MONO_EMIT_NEW_LOAD_MEMBASE (cfg, vtable_reg, obj_reg, G_STRUCT_OFFSET (MonoObject, vtable));
		mini_emit_iface_cast (cfg, vtable_reg, klass, NULL, NULL);
	} else {
		int klass_reg = alloc_preg (cfg);

		MONO_EMIT_NEW_LOAD_MEMBASE (cfg, vtable_reg, obj_reg, G_STRUCT_OFFSET (MonoObject, vtable));

		if (!klass->rank && !cfg->compile_aot && !(cfg->opt & MONO_OPT_SHARED) && (klass->flags & TYPE_ATTRIBUTE_SEALED)) {
			/* the remoting code is broken, access the class for now */
			if (0) {
				MonoVTable *vt = mono_class_vtable (cfg->domain, klass);
				MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, vtable_reg, vt);
			} else {
				MONO_EMIT_NEW_LOAD_MEMBASE (cfg, klass_reg, vtable_reg, G_STRUCT_OFFSET (MonoVTable, klass));
				MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, klass_reg, klass);
			}
			MONO_EMIT_NEW_COND_EXC (cfg, NE_UN, "InvalidCastException");
		} else {
			MONO_EMIT_NEW_LOAD_MEMBASE (cfg, klass_reg, vtable_reg, G_STRUCT_OFFSET (MonoVTable, klass));
			mini_emit_castclass (cfg, obj_reg, klass_reg, klass, is_null_bb);
		}
	}

	MONO_START_BB (cfg, is_null_bb);

	reset_cast_details (cfg);

	return src;
}

static MonoInst*
handle_isinst (MonoCompile *cfg, MonoClass *klass, MonoInst *src)
{
	MonoInst *ins;
	MonoBasicBlock *is_null_bb, *false_bb, *end_bb;
	int obj_reg = src->dreg;
	int vtable_reg = alloc_preg (cfg);
	int res_reg = alloc_preg (cfg);

	NEW_BBLOCK (cfg, is_null_bb);
	NEW_BBLOCK (cfg, false_bb);
	NEW_BBLOCK (cfg, end_bb);

	/* Do the assignment at the beginning, so the other assignment can be if converted */
	EMIT_NEW_UNALU (cfg, ins, OP_MOVE, res_reg, obj_reg);
	ins->type = STACK_OBJ;
	ins->klass = klass;

	MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, obj_reg, 0);
	MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_IBEQ, is_null_bb);

	if (klass->flags & TYPE_ATTRIBUTE_INTERFACE) {
		MONO_EMIT_NEW_LOAD_MEMBASE (cfg, vtable_reg, obj_reg, G_STRUCT_OFFSET (MonoObject, vtable));
		/* the is_null_bb target simply copies the input register to the output */
		mini_emit_iface_cast (cfg, vtable_reg, klass, false_bb, is_null_bb);
	} else {
		int klass_reg = alloc_preg (cfg);

		MONO_EMIT_NEW_LOAD_MEMBASE (cfg, vtable_reg, obj_reg, G_STRUCT_OFFSET (MonoObject, vtable));

		if (klass->rank) {
			int rank_reg = alloc_preg (cfg);
			int eclass_reg = alloc_preg (cfg);

			MONO_EMIT_NEW_LOAD_MEMBASE_OP (cfg, OP_LOADU1_MEMBASE, rank_reg, vtable_reg, G_STRUCT_OFFSET (MonoVTable, rank));
			MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, rank_reg, klass->rank);
			MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_PBNE_UN, false_bb);
			MONO_EMIT_NEW_LOAD_MEMBASE (cfg, klass_reg, vtable_reg, G_STRUCT_OFFSET (MonoVTable, klass));
			MONO_EMIT_NEW_LOAD_MEMBASE (cfg, eclass_reg, klass_reg, G_STRUCT_OFFSET (MonoClass, cast_class));
			if (klass->cast_class == mono_defaults.object_class) {
				int parent_reg = alloc_preg (cfg);
				MONO_EMIT_NEW_LOAD_MEMBASE (cfg, parent_reg, eclass_reg, G_STRUCT_OFFSET (MonoClass, parent));
				mini_emit_class_check_branch (cfg, parent_reg, mono_defaults.enum_class->parent, OP_PBNE_UN, is_null_bb);
				mini_emit_class_check_branch (cfg, eclass_reg, mono_defaults.enum_class, OP_PBEQ, is_null_bb);
				MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_BR, false_bb);
			} else if (klass->cast_class == mono_defaults.enum_class->parent) {
				mini_emit_class_check_branch (cfg, eclass_reg, mono_defaults.enum_class->parent, OP_PBEQ, is_null_bb);
				mini_emit_class_check_branch (cfg, eclass_reg, mono_defaults.enum_class, OP_PBEQ, is_null_bb);				
				MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_BR, false_bb);
			} else if (klass->cast_class == mono_defaults.enum_class) {
				mini_emit_class_check_branch (cfg, eclass_reg, mono_defaults.enum_class, OP_PBEQ, is_null_bb);
				MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_BR, false_bb);
			} else if (klass->cast_class->flags & TYPE_ATTRIBUTE_INTERFACE) {
				mini_emit_iface_class_cast (cfg, eclass_reg, klass->cast_class, false_bb, is_null_bb);
			} else {
				if ((klass->rank == 1) && (klass->byval_arg.type == MONO_TYPE_SZARRAY)) {
					/* Check that the object is a vector too */
					int bounds_reg = alloc_preg (cfg);
					MONO_EMIT_NEW_LOAD_MEMBASE (cfg, bounds_reg, obj_reg, G_STRUCT_OFFSET (MonoArray, bounds));
					MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, bounds_reg, 0);
					MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_PBNE_UN, false_bb);
				}

				/* the is_null_bb target simply copies the input register to the output */
				mini_emit_isninst_cast (cfg, eclass_reg, klass->cast_class, false_bb, is_null_bb);
			}
		} else if (mono_class_is_nullable (klass)) {
			MONO_EMIT_NEW_LOAD_MEMBASE (cfg, klass_reg, vtable_reg, G_STRUCT_OFFSET (MonoVTable, klass));
			/* the is_null_bb target simply copies the input register to the output */
			mini_emit_isninst_cast (cfg, klass_reg, klass->cast_class, false_bb, is_null_bb);
		} else {
			if (!cfg->compile_aot && !(cfg->opt & MONO_OPT_SHARED) && (klass->flags & TYPE_ATTRIBUTE_SEALED)) {
				/* the remoting code is broken, access the class for now */
				if (0) {
					MonoVTable *vt = mono_class_vtable (cfg->domain, klass);
					MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, vtable_reg, vt);
				} else {
					MONO_EMIT_NEW_LOAD_MEMBASE (cfg, klass_reg, vtable_reg, G_STRUCT_OFFSET (MonoVTable, klass));
					MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, klass_reg, klass);
				}
				MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_PBNE_UN, false_bb);
				MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_BR, is_null_bb);
			} else {
				MONO_EMIT_NEW_LOAD_MEMBASE (cfg, klass_reg, vtable_reg, G_STRUCT_OFFSET (MonoVTable, klass));
				/* the is_null_bb target simply copies the input register to the output */
				mini_emit_isninst_cast (cfg, klass_reg, klass, false_bb, is_null_bb);
			}
		}
	}

	MONO_START_BB (cfg, false_bb);

	MONO_EMIT_NEW_PCONST (cfg, res_reg, 0);
	MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_BR, end_bb);

	MONO_START_BB (cfg, is_null_bb);

	MONO_START_BB (cfg, end_bb);

	return ins;
}

static MonoInst*
handle_cisinst (MonoCompile *cfg, MonoClass *klass, MonoInst *src)
{
	/* This opcode takes as input an object reference and a class, and returns:
	0) if the object is an instance of the class,
	1) if the object is not instance of the class,
	2) if the object is a proxy whose type cannot be determined */

	MonoInst *ins;
	MonoBasicBlock *true_bb, *false_bb, *false2_bb, *end_bb, *no_proxy_bb, *interface_fail_bb;
	int obj_reg = src->dreg;
	int dreg = alloc_ireg (cfg);
	int tmp_reg;
	int klass_reg = alloc_preg (cfg);

	NEW_BBLOCK (cfg, true_bb);
	NEW_BBLOCK (cfg, false_bb);
	NEW_BBLOCK (cfg, false2_bb);
	NEW_BBLOCK (cfg, end_bb);
	NEW_BBLOCK (cfg, no_proxy_bb);

	MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, obj_reg, 0);
	MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_PBEQ, false_bb);

	if (klass->flags & TYPE_ATTRIBUTE_INTERFACE) {
		NEW_BBLOCK (cfg, interface_fail_bb);

		tmp_reg = alloc_preg (cfg);
		MONO_EMIT_NEW_LOAD_MEMBASE (cfg, tmp_reg, obj_reg, G_STRUCT_OFFSET (MonoObject, vtable));
		mini_emit_iface_cast (cfg, tmp_reg, klass, interface_fail_bb, true_bb);
		MONO_START_BB (cfg, interface_fail_bb);
		MONO_EMIT_NEW_LOAD_MEMBASE (cfg, klass_reg, tmp_reg, G_STRUCT_OFFSET (MonoVTable, klass));
		
		mini_emit_class_check_branch (cfg, klass_reg, mono_defaults.transparent_proxy_class, OP_PBNE_UN, false_bb);

		tmp_reg = alloc_preg (cfg);
		MONO_EMIT_NEW_LOAD_MEMBASE (cfg, tmp_reg, obj_reg, G_STRUCT_OFFSET (MonoTransparentProxy, custom_type_info));
		MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, tmp_reg, 0);
		MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_PBNE_UN, false2_bb);		
	} else {
		tmp_reg = alloc_preg (cfg);
		MONO_EMIT_NEW_LOAD_MEMBASE (cfg, tmp_reg, obj_reg, G_STRUCT_OFFSET (MonoObject, vtable));
		MONO_EMIT_NEW_LOAD_MEMBASE (cfg, klass_reg, tmp_reg, G_STRUCT_OFFSET (MonoVTable, klass));

		mini_emit_class_check_branch (cfg, klass_reg, mono_defaults.transparent_proxy_class, OP_PBNE_UN, no_proxy_bb);		
		tmp_reg = alloc_preg (cfg);
		MONO_EMIT_NEW_LOAD_MEMBASE (cfg, tmp_reg, obj_reg, G_STRUCT_OFFSET (MonoTransparentProxy, remote_class));
		MONO_EMIT_NEW_LOAD_MEMBASE (cfg, klass_reg, tmp_reg, G_STRUCT_OFFSET (MonoRemoteClass, proxy_class));

		tmp_reg = alloc_preg (cfg);		
		MONO_EMIT_NEW_LOAD_MEMBASE (cfg, tmp_reg, obj_reg, G_STRUCT_OFFSET (MonoTransparentProxy, custom_type_info));
		MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, tmp_reg, 0);
		MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_PBEQ, no_proxy_bb);
		
		mini_emit_isninst_cast (cfg, klass_reg, klass, false2_bb, true_bb);
		MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_BR, false2_bb);

		MONO_START_BB (cfg, no_proxy_bb);

		mini_emit_isninst_cast (cfg, klass_reg, klass, false_bb, true_bb);
	}

	MONO_START_BB (cfg, false_bb);

	MONO_EMIT_NEW_ICONST (cfg, dreg, 1);
	MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_BR, end_bb);

	MONO_START_BB (cfg, false2_bb);

	MONO_EMIT_NEW_ICONST (cfg, dreg, 2);
	MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_BR, end_bb);

	MONO_START_BB (cfg, true_bb);

	MONO_EMIT_NEW_ICONST (cfg, dreg, 0);

	MONO_START_BB (cfg, end_bb);

	/* FIXME: */
	MONO_INST_NEW (cfg, ins, OP_ICONST);
	ins->dreg = dreg;
	ins->type = STACK_I4;

	return ins;
}

static MonoInst*
handle_ccastclass (MonoCompile *cfg, MonoClass *klass, MonoInst *src)
{
	/* This opcode takes as input an object reference and a class, and returns:
	0) if the object is an instance of the class,
	1) if the object is a proxy whose type cannot be determined
	an InvalidCastException exception is thrown otherwhise*/
	
	MonoInst *ins;
	MonoBasicBlock *end_bb, *ok_result_bb, *no_proxy_bb, *interface_fail_bb, *fail_1_bb;
	int obj_reg = src->dreg;
	int dreg = alloc_ireg (cfg);
	int tmp_reg = alloc_preg (cfg);
	int klass_reg = alloc_preg (cfg);

	NEW_BBLOCK (cfg, end_bb);
	NEW_BBLOCK (cfg, ok_result_bb);

	MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, obj_reg, 0);
	MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_PBEQ, ok_result_bb);

	if (klass->flags & TYPE_ATTRIBUTE_INTERFACE) {
		NEW_BBLOCK (cfg, interface_fail_bb);
	
		MONO_EMIT_NEW_LOAD_MEMBASE (cfg, tmp_reg, obj_reg, G_STRUCT_OFFSET (MonoObject, vtable));
		mini_emit_iface_cast (cfg, tmp_reg, klass, interface_fail_bb, ok_result_bb);
		MONO_START_BB (cfg, interface_fail_bb);
		MONO_EMIT_NEW_LOAD_MEMBASE (cfg, klass_reg, tmp_reg, G_STRUCT_OFFSET (MonoVTable, klass));

		mini_emit_class_check (cfg, klass_reg, mono_defaults.transparent_proxy_class);

		tmp_reg = alloc_preg (cfg);		
		MONO_EMIT_NEW_LOAD_MEMBASE (cfg, tmp_reg, obj_reg, G_STRUCT_OFFSET (MonoTransparentProxy, custom_type_info));
		MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, tmp_reg, 0);
		MONO_EMIT_NEW_COND_EXC (cfg, EQ, "InvalidCastException");
		
		MONO_EMIT_NEW_ICONST (cfg, dreg, 1);
		MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_BR, end_bb);
		
	} else {
		NEW_BBLOCK (cfg, no_proxy_bb);

		MONO_EMIT_NEW_LOAD_MEMBASE (cfg, tmp_reg, obj_reg, G_STRUCT_OFFSET (MonoObject, vtable));
		MONO_EMIT_NEW_LOAD_MEMBASE (cfg, klass_reg, tmp_reg, G_STRUCT_OFFSET (MonoVTable, klass));
		mini_emit_class_check_branch (cfg, klass_reg, mono_defaults.transparent_proxy_class, OP_PBNE_UN, no_proxy_bb);		

		tmp_reg = alloc_preg (cfg);
		MONO_EMIT_NEW_LOAD_MEMBASE (cfg, tmp_reg, obj_reg, G_STRUCT_OFFSET (MonoTransparentProxy, remote_class));
		MONO_EMIT_NEW_LOAD_MEMBASE (cfg, klass_reg, tmp_reg, G_STRUCT_OFFSET (MonoRemoteClass, proxy_class));

		tmp_reg = alloc_preg (cfg);
		MONO_EMIT_NEW_LOAD_MEMBASE (cfg, tmp_reg, obj_reg, G_STRUCT_OFFSET (MonoTransparentProxy, custom_type_info));
		MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, tmp_reg, 0);
		MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_PBEQ, no_proxy_bb);

		NEW_BBLOCK (cfg, fail_1_bb);
		
		mini_emit_isninst_cast (cfg, klass_reg, klass, fail_1_bb, ok_result_bb);

		MONO_START_BB (cfg, fail_1_bb);

		MONO_EMIT_NEW_ICONST (cfg, dreg, 1);
		MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_BR, end_bb);

		MONO_START_BB (cfg, no_proxy_bb);

		mini_emit_castclass (cfg, obj_reg, klass_reg, klass, ok_result_bb);
	}

	MONO_START_BB (cfg, ok_result_bb);

	MONO_EMIT_NEW_ICONST (cfg, dreg, 0);

	MONO_START_BB (cfg, end_bb);

	/* FIXME: */
	MONO_INST_NEW (cfg, ins, OP_ICONST);
	ins->dreg = dreg;
	ins->type = STACK_I4;

	return ins;
}

static G_GNUC_UNUSED MonoInst*
handle_delegate_ctor (MonoCompile *cfg, MonoClass *klass, MonoInst *target, MonoMethod *method)
{
	gpointer *trampoline;
	MonoInst *obj, *method_ins, *tramp_ins;
	MonoDomain *domain;
	guint8 **code_slot;

	obj = handle_alloc (cfg, klass, FALSE);

	/* Inline the contents of mono_delegate_ctor */

	/* Set target field */
	/* Optimize away setting of NULL target */
	if (!(target->opcode == OP_PCONST && target->inst_p0 == 0))
		MONO_EMIT_NEW_STORE_MEMBASE (cfg, OP_STORE_MEMBASE_REG, obj->dreg, G_STRUCT_OFFSET (MonoDelegate, target), target->dreg);

	/* Set method field */
	EMIT_NEW_METHODCONST (cfg, method_ins, method);
	MONO_EMIT_NEW_STORE_MEMBASE (cfg, OP_STORE_MEMBASE_REG, obj->dreg, G_STRUCT_OFFSET (MonoDelegate, method), method_ins->dreg);

	/* 
	 * To avoid looking up the compiled code belonging to the target method
	 * in mono_delegate_trampoline (), we allocate a per-domain memory slot to
	 * store it, and we fill it after the method has been compiled.
	 */
	if (!cfg->compile_aot && !method->dynamic) {
		MonoInst *code_slot_ins;

		domain = mono_domain_get ();
		mono_domain_lock (domain);
		if (!domain_jit_info (domain)->method_code_hash)
			domain_jit_info (domain)->method_code_hash = g_hash_table_new (NULL, NULL);
		code_slot = g_hash_table_lookup (domain_jit_info (domain)->method_code_hash, method);
		if (!code_slot) {
			code_slot = mono_domain_alloc0 (domain, sizeof (gpointer));
			g_hash_table_insert (domain_jit_info (domain)->method_code_hash, method, code_slot);
		}
		mono_domain_unlock (domain);

		EMIT_NEW_PCONST (cfg, code_slot_ins, code_slot);
		MONO_EMIT_NEW_STORE_MEMBASE (cfg, OP_STORE_MEMBASE_REG, obj->dreg, G_STRUCT_OFFSET (MonoDelegate, method_code), code_slot_ins->dreg);		
	}

	/* Set invoke_impl field */
	if (cfg->compile_aot) {
		EMIT_NEW_AOTCONST (cfg, tramp_ins, MONO_PATCH_INFO_DELEGATE_TRAMPOLINE, klass);
	} else {
		trampoline = mono_create_delegate_trampoline (klass);
		EMIT_NEW_PCONST (cfg, tramp_ins, trampoline);
	}
	MONO_EMIT_NEW_STORE_MEMBASE (cfg, OP_STORE_MEMBASE_REG, obj->dreg, G_STRUCT_OFFSET (MonoDelegate, invoke_impl), tramp_ins->dreg);

	/* All the checks which are in mono_delegate_ctor () are done by the delegate trampoline */

	return obj;
}

static MonoInst*
handle_array_new (MonoCompile *cfg, int rank, MonoInst **sp, unsigned char *ip)
{
	MonoJitICallInfo *info;

	/* Need to register the icall so it gets an icall wrapper */
	info = mono_get_array_new_va_icall (rank);

	cfg->flags |= MONO_CFG_HAS_VARARGS;

	/* FIXME: This uses info->sig, but it should use the signature of the wrapper */
	return mono_emit_native_call (cfg, mono_icall_get_wrapper (info), info->sig, sp);
}

static void
mono_emit_load_got_addr (MonoCompile *cfg)
{
	MonoInst *getaddr, *dummy_use;

	if (!cfg->got_var || cfg->got_var_allocated)
		return;

	MONO_INST_NEW (cfg, getaddr, OP_LOAD_GOTADDR);
	getaddr->dreg = cfg->got_var->dreg;

	/* Add it to the start of the first bblock */
	if (cfg->bb_entry->code) {
		getaddr->next = cfg->bb_entry->code;
		cfg->bb_entry->code = getaddr;
	}
	else
		MONO_ADD_INS (cfg->bb_entry, getaddr);

	cfg->got_var_allocated = TRUE;

	/* 
	 * Add a dummy use to keep the got_var alive, since real uses might
	 * only be generated by the back ends.
	 * Add it to end_bblock, so the variable's lifetime covers the whole
	 * method.
	 * It would be better to make the usage of the got var explicit in all
	 * cases when the backend needs it (i.e. calls, throw etc.), so this
	 * wouldn't be needed.
	 */
	NEW_DUMMY_USE (cfg, dummy_use, cfg->got_var);
	MONO_ADD_INS (cfg->bb_exit, dummy_use);
}

static int inline_limit;
static gboolean inline_limit_inited;

static gboolean
mono_method_check_inlining (MonoCompile *cfg, MonoMethod *method)
{
	MonoMethodHeader *header = mono_method_get_header (method);
	MonoVTable *vtable;
#ifdef MONO_ARCH_SOFT_FLOAT
	MonoMethodSignature *sig = mono_method_signature (method);
	int i;
#endif

	if (cfg->generic_sharing_context)
		return FALSE;

#ifdef MONO_ARCH_HAVE_LMF_OPS
	if (((method->iflags & METHOD_IMPL_ATTRIBUTE_INTERNAL_CALL) ||
		 (method->flags & METHOD_ATTRIBUTE_PINVOKE_IMPL)) &&
	    !MONO_TYPE_ISSTRUCT (signature->ret) && !mini_class_is_system_array (method->klass))
		return TRUE;
#endif

	if ((method->iflags & METHOD_IMPL_ATTRIBUTE_RUNTIME) ||
	    (method->iflags & METHOD_IMPL_ATTRIBUTE_INTERNAL_CALL) ||
	    (method->iflags & METHOD_IMPL_ATTRIBUTE_NOINLINING) ||
	    (method->iflags & METHOD_IMPL_ATTRIBUTE_SYNCHRONIZED) ||
	    (method->flags & METHOD_ATTRIBUTE_PINVOKE_IMPL) ||
	    (method->klass->marshalbyref) ||
	    !header || header->num_clauses)
		return FALSE;

	/* also consider num_locals? */
	/* Do the size check early to avoid creating vtables */
	if (!inline_limit_inited) {
		if (getenv ("MONO_INLINELIMIT"))
			inline_limit = atoi (getenv ("MONO_INLINELIMIT"));
		else
			inline_limit = INLINE_LENGTH_LIMIT;
		inline_limit_inited = TRUE;
	}
	if (header->code_size >= inline_limit)
		return FALSE;

	/*
	 * if we can initialize the class of the method right away, we do,
	 * otherwise we don't allow inlining if the class needs initialization,
	 * since it would mean inserting a call to mono_runtime_class_init()
	 * inside the inlined code
	 */
	if (!(cfg->opt & MONO_OPT_SHARED)) {
		if (method->klass->flags & TYPE_ATTRIBUTE_BEFORE_FIELD_INIT) {
			if (cfg->run_cctors && method->klass->has_cctor) {
				if (!method->klass->runtime_info)
					/* No vtable created yet */
					return FALSE;
				vtable = mono_class_vtable (cfg->domain, method->klass);
				if (!vtable)
					return FALSE;
				/* This makes so that inline cannot trigger */
				/* .cctors: too many apps depend on them */
				/* running with a specific order... */
				if (! vtable->initialized)
					return FALSE;
				mono_runtime_class_init (vtable);
			}
		} else if (mono_class_needs_cctor_run (method->klass, NULL)) {
			if (!method->klass->runtime_info)
				/* No vtable created yet */
				return FALSE;
			vtable = mono_class_vtable (cfg->domain, method->klass);
			if (!vtable)
				return FALSE;
			if (!vtable->initialized)
				return FALSE;
		}
	} else {
		/* 
		 * If we're compiling for shared code
		 * the cctor will need to be run at aot method load time, for example,
		 * or at the end of the compilation of the inlining method.
		 */
		if (mono_class_needs_cctor_run (method->klass, NULL) && !((method->klass->flags & TYPE_ATTRIBUTE_BEFORE_FIELD_INIT)))
			return FALSE;
	}

	/*
	 * CAS - do not inline methods with declarative security
	 * Note: this has to be before any possible return TRUE;
	 */
	if (mono_method_has_declsec (method))
		return FALSE;

#ifdef MONO_ARCH_SOFT_FLOAT
	/* FIXME: */
	if (sig->ret && sig->ret->type == MONO_TYPE_R4)
		return FALSE;
	for (i = 0; i < sig->param_count; ++i)
		if (!sig->params [i]->byref && sig->params [i]->type == MONO_TYPE_R4)
			return FALSE;
#endif

	return TRUE;
}

static gboolean
mini_field_access_needs_cctor_run (MonoCompile *cfg, MonoMethod *method, MonoVTable *vtable)
{
	if (vtable->initialized && !cfg->compile_aot)
		return FALSE;

	if (vtable->klass->flags & TYPE_ATTRIBUTE_BEFORE_FIELD_INIT)
		return FALSE;

	if (!mono_class_needs_cctor_run (vtable->klass, method))
		return FALSE;

	if (! (method->flags & METHOD_ATTRIBUTE_STATIC) && (vtable->klass == method->klass))
		/* The initialization is already done before the method is called */
		return FALSE;

	return TRUE;
}

static MonoInst*
mini_emit_ldelema_1_ins (MonoCompile *cfg, MonoClass *klass, MonoInst *arr, MonoInst *index)
{
	MonoInst *ins;
	guint32 size;
	int mult_reg, add_reg, array_reg, index_reg, index2_reg;

	mono_class_init (klass);
	size = mono_class_array_element_size (klass);

	mult_reg = alloc_preg (cfg);
	array_reg = arr->dreg;
	index_reg = index->dreg;

#if SIZEOF_REGISTER == 8
	/* The array reg is 64 bits but the index reg is only 32 */
	index2_reg = alloc_preg (cfg);
	MONO_EMIT_NEW_UNALU (cfg, OP_SEXT_I4, index2_reg, index_reg);
#else
	if (index->type == STACK_I8) {
		index2_reg = alloc_preg (cfg);
		MONO_EMIT_NEW_UNALU (cfg, OP_LCONV_TO_I4, index2_reg, index_reg);
	} else {
		index2_reg = index_reg;
	}
#endif

	MONO_EMIT_BOUNDS_CHECK (cfg, array_reg, MonoArray, max_length, index2_reg);

#if defined(TARGET_X86) || defined(TARGET_AMD64)
	if (size == 1 || size == 2 || size == 4 || size == 8) {
		static const int fast_log2 [] = { 1, 0, 1, -1, 2, -1, -1, -1, 3 };

		EMIT_NEW_X86_LEA (cfg, ins, array_reg, index2_reg, fast_log2 [size], G_STRUCT_OFFSET (MonoArray, vector));
		ins->type = STACK_PTR;

		return ins;
	}
#endif		

	add_reg = alloc_preg (cfg);

	MONO_EMIT_NEW_BIALU_IMM (cfg, OP_MUL_IMM, mult_reg, index2_reg, size);
	MONO_EMIT_NEW_BIALU (cfg, OP_PADD, add_reg, array_reg, mult_reg);
	NEW_BIALU_IMM (cfg, ins, OP_PADD_IMM, add_reg, add_reg, G_STRUCT_OFFSET (MonoArray, vector));
	ins->type = STACK_PTR;
	MONO_ADD_INS (cfg->cbb, ins);

	return ins;
}

#ifndef MONO_ARCH_EMULATE_MUL_DIV
static MonoInst*
mini_emit_ldelema_2_ins (MonoCompile *cfg, MonoClass *klass, MonoInst *arr, MonoInst *index_ins1, MonoInst *index_ins2)
{
	int bounds_reg = alloc_preg (cfg);
	int add_reg = alloc_preg (cfg);
	int mult_reg = alloc_preg (cfg);
	int mult2_reg = alloc_preg (cfg);
	int low1_reg = alloc_preg (cfg);
	int low2_reg = alloc_preg (cfg);
	int high1_reg = alloc_preg (cfg);
	int high2_reg = alloc_preg (cfg);
	int realidx1_reg = alloc_preg (cfg);
	int realidx2_reg = alloc_preg (cfg);
	int sum_reg = alloc_preg (cfg);
	int index1, index2;
	MonoInst *ins;
	guint32 size;

	mono_class_init (klass);
	size = mono_class_array_element_size (klass);

	index1 = index_ins1->dreg;
	index2 = index_ins2->dreg;

	/* range checking */
	MONO_EMIT_NEW_LOAD_MEMBASE (cfg, bounds_reg, 
				       arr->dreg, G_STRUCT_OFFSET (MonoArray, bounds));

	MONO_EMIT_NEW_LOAD_MEMBASE_OP (cfg, OP_LOADI4_MEMBASE, low1_reg, 
				       bounds_reg, G_STRUCT_OFFSET (MonoArrayBounds, lower_bound));
	MONO_EMIT_NEW_BIALU (cfg, OP_PSUB, realidx1_reg, index1, low1_reg);
	MONO_EMIT_NEW_LOAD_MEMBASE_OP (cfg, OP_LOADI4_MEMBASE, high1_reg, 
				       bounds_reg, G_STRUCT_OFFSET (MonoArrayBounds, length));
	MONO_EMIT_NEW_BIALU (cfg, OP_COMPARE, -1, high1_reg, realidx1_reg);
	MONO_EMIT_NEW_COND_EXC (cfg, LE_UN, "IndexOutOfRangeException");

	MONO_EMIT_NEW_LOAD_MEMBASE_OP (cfg, OP_LOADI4_MEMBASE, low2_reg, 
				       bounds_reg, sizeof (MonoArrayBounds) + G_STRUCT_OFFSET (MonoArrayBounds, lower_bound));
	MONO_EMIT_NEW_BIALU (cfg, OP_PSUB, realidx2_reg, index2, low2_reg);
	MONO_EMIT_NEW_LOAD_MEMBASE_OP (cfg, OP_LOADI4_MEMBASE, high2_reg, 
				       bounds_reg, sizeof (MonoArrayBounds) + G_STRUCT_OFFSET (MonoArrayBounds, length));
	MONO_EMIT_NEW_BIALU (cfg, OP_COMPARE, -1, high2_reg, realidx2_reg);
	MONO_EMIT_NEW_COND_EXC (cfg, LE_UN, "IndexOutOfRangeException");

	MONO_EMIT_NEW_BIALU (cfg, OP_PMUL, mult_reg, high2_reg, realidx1_reg);
	MONO_EMIT_NEW_BIALU (cfg, OP_PADD, sum_reg, mult_reg, realidx2_reg);
	MONO_EMIT_NEW_BIALU_IMM (cfg, OP_PMUL_IMM, mult2_reg, sum_reg, size);
	MONO_EMIT_NEW_BIALU (cfg, OP_PADD, add_reg, mult2_reg, arr->dreg);
	NEW_BIALU_IMM (cfg, ins, OP_PADD_IMM, add_reg, add_reg, G_STRUCT_OFFSET (MonoArray, vector));

	ins->type = STACK_MP;
	ins->klass = klass;
	MONO_ADD_INS (cfg->cbb, ins);

	return ins;
}
#endif

static MonoInst*
mini_emit_ldelema_ins (MonoCompile *cfg, MonoMethod *cmethod, MonoInst **sp, unsigned char *ip, gboolean is_set)
{
	int rank;
	MonoInst *addr;
	MonoMethod *addr_method;
	int element_size;

	rank = mono_method_signature (cmethod)->param_count - (is_set? 1: 0);

	if (rank == 1)
		return mini_emit_ldelema_1_ins (cfg, cmethod->klass->element_class, sp [0], sp [1]);

#ifndef MONO_ARCH_EMULATE_MUL_DIV
	/* emit_ldelema_2 depends on OP_LMUL */
	if (rank == 2 && (cfg->opt & MONO_OPT_INTRINS)) {
		return mini_emit_ldelema_2_ins (cfg, cmethod->klass->element_class, sp [0], sp [1], sp [2]);
	}
#endif

	element_size = mono_class_array_element_size (cmethod->klass->element_class);
	addr_method = mono_marshal_get_array_address (rank, element_size);
	addr = mono_emit_method_call (cfg, addr_method, sp, NULL);

	return addr;
}

static MonoInst*
mini_emit_inst_for_method (MonoCompile *cfg, MonoMethod *cmethod, MonoMethodSignature *fsig, MonoInst **args)
{
	MonoInst *ins = NULL;
	
	static MonoClass *runtime_helpers_class = NULL;
	if (! runtime_helpers_class)
		runtime_helpers_class = mono_class_from_name (mono_defaults.corlib,
			"System.Runtime.CompilerServices", "RuntimeHelpers");

	if (cmethod->klass == mono_defaults.string_class) {
		if (strcmp (cmethod->name, "get_Chars") == 0) {
			int dreg = alloc_ireg (cfg);
			int index_reg = alloc_preg (cfg);
			int mult_reg = alloc_preg (cfg);
			int add_reg = alloc_preg (cfg);

#if SIZEOF_REGISTER == 8
			/* The array reg is 64 bits but the index reg is only 32 */
			MONO_EMIT_NEW_UNALU (cfg, OP_SEXT_I4, index_reg, args [1]->dreg);
#else
			index_reg = args [1]->dreg;
#endif	
			MONO_EMIT_BOUNDS_CHECK (cfg, args [0]->dreg, MonoString, length, index_reg);

#if defined(TARGET_X86) || defined(TARGET_AMD64)
			EMIT_NEW_X86_LEA (cfg, ins, args [0]->dreg, index_reg, 1, G_STRUCT_OFFSET (MonoString, chars));
			add_reg = ins->dreg;
			/* Avoid a warning */
			mult_reg = 0;
			EMIT_NEW_LOAD_MEMBASE (cfg, ins, OP_LOADU2_MEMBASE, dreg, 
								   add_reg, 0);
#else
			MONO_EMIT_NEW_BIALU_IMM (cfg, OP_SHL_IMM, mult_reg, index_reg, 1);
			MONO_EMIT_NEW_BIALU (cfg, OP_PADD, add_reg, mult_reg, args [0]->dreg);
			EMIT_NEW_LOAD_MEMBASE (cfg, ins, OP_LOADU2_MEMBASE, dreg, 
								   add_reg, G_STRUCT_OFFSET (MonoString, chars));
#endif
			type_from_op (ins, NULL, NULL);
			return ins;
		} else if (strcmp (cmethod->name, "get_Length") == 0) {
			int dreg = alloc_ireg (cfg);
			/* Decompose later to allow more optimizations */
			EMIT_NEW_UNALU (cfg, ins, OP_STRLEN, dreg, args [0]->dreg);
			ins->type = STACK_I4;
			cfg->cbb->has_array_access = TRUE;
			cfg->flags |= MONO_CFG_HAS_ARRAY_ACCESS;

			return ins;
		} else if (strcmp (cmethod->name, "InternalSetChar") == 0) {
			int mult_reg = alloc_preg (cfg);
			int add_reg = alloc_preg (cfg);

			/* The corlib functions check for oob already. */
			MONO_EMIT_NEW_BIALU_IMM (cfg, OP_SHL_IMM, mult_reg, args [1]->dreg, 1);
			MONO_EMIT_NEW_BIALU (cfg, OP_PADD, add_reg, mult_reg, args [0]->dreg);
			MONO_EMIT_NEW_STORE_MEMBASE (cfg, OP_STOREI2_MEMBASE_REG, add_reg, G_STRUCT_OFFSET (MonoString, chars), args [2]->dreg);
		} else 
			return NULL;
	} else if (cmethod->klass == mono_defaults.object_class) {

		if (strcmp (cmethod->name, "GetType") == 0) {
			int dreg = alloc_preg (cfg);
			int vt_reg = alloc_preg (cfg);
			MONO_EMIT_NEW_LOAD_MEMBASE (cfg, vt_reg, args [0]->dreg, G_STRUCT_OFFSET (MonoObject, vtable));
			EMIT_NEW_LOAD_MEMBASE (cfg, ins, OP_LOAD_MEMBASE, dreg, vt_reg, G_STRUCT_OFFSET (MonoVTable, type));
			type_from_op (ins, NULL, NULL);

			return ins;
#if !defined(MONO_ARCH_EMULATE_MUL_DIV) && !defined(HAVE_MOVING_COLLECTOR)
		} else if (strcmp (cmethod->name, "InternalGetHashCode") == 0) {
			int dreg = alloc_ireg (cfg);
			int t1 = alloc_ireg (cfg);
	
			MONO_EMIT_NEW_BIALU_IMM (cfg, OP_SHL_IMM, t1, args [0]->dreg, 3);
			EMIT_NEW_BIALU_IMM (cfg, ins, OP_MUL_IMM, dreg, t1, 2654435761u);
			ins->type = STACK_I4;

			return ins;
#endif
		} else if (strcmp (cmethod->name, ".ctor") == 0) {
 			MONO_INST_NEW (cfg, ins, OP_NOP);
			MONO_ADD_INS (cfg->cbb, ins);
			return ins;
		} else
			return NULL;
	} else if (cmethod->klass == mono_defaults.array_class) {
 		if (cmethod->name [0] != 'g')
 			return NULL;

		if (strcmp (cmethod->name, "get_Rank") == 0) {
			int dreg = alloc_ireg (cfg);
			int vtable_reg = alloc_preg (cfg);
			MONO_EMIT_NEW_LOAD_MEMBASE_OP (cfg, OP_LOAD_MEMBASE, vtable_reg, 
										   args [0]->dreg, G_STRUCT_OFFSET (MonoObject, vtable));
			EMIT_NEW_LOAD_MEMBASE (cfg, ins, OP_LOADU1_MEMBASE, dreg,
								   vtable_reg, G_STRUCT_OFFSET (MonoVTable, rank));
			type_from_op (ins, NULL, NULL);

			return ins;
		} else if (strcmp (cmethod->name, "get_Length") == 0) {
			int dreg = alloc_ireg (cfg);

			EMIT_NEW_LOAD_MEMBASE (cfg, ins, OP_LOADI4_MEMBASE, dreg, 
								   args [0]->dreg, G_STRUCT_OFFSET (MonoArray, max_length));
			type_from_op (ins, NULL, NULL);

			return ins;
		} else
			return NULL;
	} else if (cmethod->klass == runtime_helpers_class) {

		if (strcmp (cmethod->name, "get_OffsetToStringData") == 0) {
			EMIT_NEW_ICONST (cfg, ins, G_STRUCT_OFFSET (MonoString, chars));
			return ins;
		} else
			return NULL;
	} else if (cmethod->klass == mono_defaults.thread_class) {
		if (strcmp (cmethod->name, "get_CurrentThread") == 0 && (ins = mono_arch_get_thread_intrinsic (cfg))) {
			ins->dreg = alloc_preg (cfg);
			ins->type = STACK_OBJ;
			MONO_ADD_INS (cfg->cbb, ins);
			return ins;
		} else if (strcmp (cmethod->name, "SpinWait_nop") == 0) {
			MONO_INST_NEW (cfg, ins, OP_RELAXED_NOP);
			MONO_ADD_INS (cfg->cbb, ins);
			return ins;
		} else if (strcmp (cmethod->name, "MemoryBarrier") == 0) {
			MONO_INST_NEW (cfg, ins, OP_MEMORY_BARRIER);
			MONO_ADD_INS (cfg->cbb, ins);
			return ins;
		}
	} else if (cmethod->klass == mono_defaults.monitor_class) {
#if defined(MONO_ARCH_MONITOR_OBJECT_REG)
		if (strcmp (cmethod->name, "Enter") == 0) {
			MonoCallInst *call;

			call = (MonoCallInst*)mono_emit_abs_call (cfg, MONO_PATCH_INFO_MONITOR_ENTER,
					NULL, helper_sig_monitor_enter_exit_trampoline, NULL);
			mono_call_inst_add_outarg_reg (cfg, call, args [0]->dreg,
					MONO_ARCH_MONITOR_OBJECT_REG, FALSE);

			return (MonoInst*)call;
		} else if (strcmp (cmethod->name, "Exit") == 0) {
			MonoCallInst *call;

			call = (MonoCallInst*)mono_emit_abs_call (cfg, MONO_PATCH_INFO_MONITOR_EXIT,
					NULL, helper_sig_monitor_enter_exit_trampoline, NULL);
			mono_call_inst_add_outarg_reg (cfg, call, args [0]->dreg,
					MONO_ARCH_MONITOR_OBJECT_REG, FALSE);

			return (MonoInst*)call;
		}
#elif defined(MONO_ARCH_ENABLE_MONITOR_IL_FASTPATH)
		MonoMethod *fast_method = NULL;

		/* Avoid infinite recursion */
		if (cfg->method->wrapper_type == MONO_WRAPPER_UNKNOWN &&
				(strcmp (cfg->method->name, "FastMonitorEnter") == 0 ||
				 strcmp (cfg->method->name, "FastMonitorExit") == 0))
			return NULL;

		if (strcmp (cmethod->name, "Enter") == 0 ||
				strcmp (cmethod->name, "Exit") == 0)
			fast_method = mono_monitor_get_fast_path (cmethod);
		if (!fast_method)
			return NULL;

		return (MonoInst*)mono_emit_method_call (cfg, fast_method, args, NULL);
#endif
	} else if (mini_class_is_system_array (cmethod->klass) &&
			strcmp (cmethod->name, "GetGenericValueImpl") == 0) {
		MonoInst *addr, *store, *load;
		MonoClass *eklass = mono_class_from_mono_type (fsig->params [1]);

		addr = mini_emit_ldelema_1_ins (cfg, eklass, args [0], args [1]);
		EMIT_NEW_LOAD_MEMBASE_TYPE (cfg, load, &eklass->byval_arg, addr->dreg, 0);
		EMIT_NEW_STORE_MEMBASE_TYPE (cfg, store, &eklass->byval_arg, args [2]->dreg, 0, load->dreg);
		return store;
	} else if (cmethod->klass->image == mono_defaults.corlib &&
			   (strcmp (cmethod->klass->name_space, "System.Threading") == 0) &&
			   (strcmp (cmethod->klass->name, "Interlocked") == 0)) {
		ins = NULL;

#if SIZEOF_REGISTER == 8
		if (strcmp (cmethod->name, "Read") == 0 && (fsig->params [0]->type == MONO_TYPE_I8)) {
			/* 64 bit reads are already atomic */
			MONO_INST_NEW (cfg, ins, OP_LOADI8_MEMBASE);
			ins->dreg = mono_alloc_preg (cfg);
			ins->inst_basereg = args [0]->dreg;
			ins->inst_offset = 0;
			MONO_ADD_INS (cfg->cbb, ins);
		}
#endif

#ifdef MONO_ARCH_HAVE_ATOMIC_ADD
		if (strcmp (cmethod->name, "Increment") == 0) {
			MonoInst *ins_iconst;
			guint32 opcode = 0;

			if (fsig->params [0]->type == MONO_TYPE_I4)
				opcode = OP_ATOMIC_ADD_NEW_I4;
#if SIZEOF_REGISTER == 8
			else if (fsig->params [0]->type == MONO_TYPE_I8)
				opcode = OP_ATOMIC_ADD_NEW_I8;
#endif
			if (opcode) {
				MONO_INST_NEW (cfg, ins_iconst, OP_ICONST);
				ins_iconst->inst_c0 = 1;
				ins_iconst->dreg = mono_alloc_ireg (cfg);
				MONO_ADD_INS (cfg->cbb, ins_iconst);

				MONO_INST_NEW (cfg, ins, opcode);
				ins->dreg = mono_alloc_ireg (cfg);
				ins->inst_basereg = args [0]->dreg;
				ins->inst_offset = 0;
				ins->sreg2 = ins_iconst->dreg;
				ins->type = (opcode == OP_ATOMIC_ADD_NEW_I4) ? STACK_I4 : STACK_I8;
				MONO_ADD_INS (cfg->cbb, ins);
			}
		} else if (strcmp (cmethod->name, "Decrement") == 0) {
			MonoInst *ins_iconst;
			guint32 opcode = 0;

			if (fsig->params [0]->type == MONO_TYPE_I4)
				opcode = OP_ATOMIC_ADD_NEW_I4;
#if SIZEOF_REGISTER == 8
			else if (fsig->params [0]->type == MONO_TYPE_I8)
				opcode = OP_ATOMIC_ADD_NEW_I8;
#endif
			if (opcode) {
				MONO_INST_NEW (cfg, ins_iconst, OP_ICONST);
				ins_iconst->inst_c0 = -1;
				ins_iconst->dreg = mono_alloc_ireg (cfg);
				MONO_ADD_INS (cfg->cbb, ins_iconst);

				MONO_INST_NEW (cfg, ins, opcode);
				ins->dreg = mono_alloc_ireg (cfg);
				ins->inst_basereg = args [0]->dreg;
				ins->inst_offset = 0;
				ins->sreg2 = ins_iconst->dreg;
				ins->type = (opcode == OP_ATOMIC_ADD_NEW_I4) ? STACK_I4 : STACK_I8;
				MONO_ADD_INS (cfg->cbb, ins);
			}
		} else if (strcmp (cmethod->name, "Add") == 0) {
			guint32 opcode = 0;

			if (fsig->params [0]->type == MONO_TYPE_I4)
				opcode = OP_ATOMIC_ADD_NEW_I4;
#if SIZEOF_REGISTER == 8
			else if (fsig->params [0]->type == MONO_TYPE_I8)
				opcode = OP_ATOMIC_ADD_NEW_I8;
#endif

			if (opcode) {
				MONO_INST_NEW (cfg, ins, opcode);
				ins->dreg = mono_alloc_ireg (cfg);
				ins->inst_basereg = args [0]->dreg;
				ins->inst_offset = 0;
				ins->sreg2 = args [1]->dreg;
				ins->type = (opcode == OP_ATOMIC_ADD_NEW_I4) ? STACK_I4 : STACK_I8;
				MONO_ADD_INS (cfg->cbb, ins);
			}
		}
#endif /* MONO_ARCH_HAVE_ATOMIC_ADD */

#ifdef MONO_ARCH_HAVE_ATOMIC_EXCHANGE
		if (strcmp (cmethod->name, "Exchange") == 0) {
			guint32 opcode;

			if (fsig->params [0]->type == MONO_TYPE_I4)
				opcode = OP_ATOMIC_EXCHANGE_I4;
#if SIZEOF_REGISTER == 8
			else if ((fsig->params [0]->type == MONO_TYPE_I8) ||
					 (fsig->params [0]->type == MONO_TYPE_I) ||
					 (fsig->params [0]->type == MONO_TYPE_OBJECT))
				opcode = OP_ATOMIC_EXCHANGE_I8;
#else
			else if ((fsig->params [0]->type == MONO_TYPE_I) ||
					 (fsig->params [0]->type == MONO_TYPE_OBJECT))
				opcode = OP_ATOMIC_EXCHANGE_I4;
#endif
			else
				return NULL;

			MONO_INST_NEW (cfg, ins, opcode);
			ins->dreg = mono_alloc_ireg (cfg);
			ins->inst_basereg = args [0]->dreg;
			ins->inst_offset = 0;
			ins->sreg2 = args [1]->dreg;
			MONO_ADD_INS (cfg->cbb, ins);

			switch (fsig->params [0]->type) {
			case MONO_TYPE_I4:
				ins->type = STACK_I4;
				break;
			case MONO_TYPE_I8:
			case MONO_TYPE_I:
				ins->type = STACK_I8;
				break;
			case MONO_TYPE_OBJECT:
				ins->type = STACK_OBJ;
				break;
			default:
				g_assert_not_reached ();
			}
		}
#endif /* MONO_ARCH_HAVE_ATOMIC_EXCHANGE */
 
#ifdef MONO_ARCH_HAVE_ATOMIC_CAS_IMM
		/* 
		 * Can't implement CompareExchange methods this way since they have
		 * three arguments. We can implement one of the common cases, where the new
		 * value is a constant.
		 */
		if ((strcmp (cmethod->name, "CompareExchange") == 0)) {
			if ((fsig->params [1]->type == MONO_TYPE_I4 ||
						(sizeof (gpointer) == 4 && fsig->params [1]->type == MONO_TYPE_I))
					&& args [2]->opcode == OP_ICONST) {
				MONO_INST_NEW (cfg, ins, OP_ATOMIC_CAS_IMM_I4);
				ins->dreg = alloc_ireg (cfg);
				ins->sreg1 = args [0]->dreg;
				ins->sreg2 = args [1]->dreg;
				ins->backend.data = GINT_TO_POINTER (args [2]->inst_c0);
				ins->type = STACK_I4;
				MONO_ADD_INS (cfg->cbb, ins);
			}
			/* The I8 case is hard to detect, since the arg might be a conv.i8 (iconst) tree */
		}
#endif /* MONO_ARCH_HAVE_ATOMIC_CAS_IMM */

		if (ins)
			return ins;
	} else if (cmethod->klass->image == mono_defaults.corlib) {
		if (cmethod->name [0] == 'B' && strcmp (cmethod->name, "Break") == 0
				&& strcmp (cmethod->klass->name, "Debugger") == 0) {
			MONO_INST_NEW (cfg, ins, OP_BREAK);
			MONO_ADD_INS (cfg->cbb, ins);
			return ins;
		}
		if (cmethod->name [0] == 'g' && strcmp (cmethod->name, "get_IsRunningOnWindows") == 0
				&& strcmp (cmethod->klass->name, "Environment") == 0) {
#ifdef PLATFORM_WIN32
	                EMIT_NEW_ICONST (cfg, ins, 1);
#else
	                EMIT_NEW_ICONST (cfg, ins, 0);
#endif
			return ins;
		}
	} else if (cmethod->klass == mono_defaults.math_class) {
		/* 
		 * There is general branches code for Min/Max, but it does not work for 
		 * all inputs:
		 * http://everything2.com/?node_id=1051618
		 */
	}

#ifdef MONO_ARCH_SIMD_INTRINSICS
	if (cfg->opt & MONO_OPT_SIMD) {
		ins = mono_emit_simd_intrinsics (cfg, cmethod, fsig, args);
		if (ins)
			return ins;
	}
#endif

	return mono_arch_emit_inst_for_method (cfg, cmethod, fsig, args);
}

/*
 * This entry point could be used later for arbitrary method
 * redirection.
 */
inline static MonoInst*
mini_redirect_call (MonoCompile *cfg, MonoMethod *method,  
					MonoMethodSignature *signature, MonoInst **args, MonoInst *this)
{
	if (method->klass == mono_defaults.string_class) {
		/* managed string allocation support */
		if (strcmp (method->name, "InternalAllocateStr") == 0) {
			MonoInst *iargs [2];
			MonoVTable *vtable = mono_class_vtable (cfg->domain, method->klass);
			MonoMethod *managed_alloc = mono_gc_get_managed_allocator (vtable, FALSE);
			if (!managed_alloc)
				return NULL;
			EMIT_NEW_VTABLECONST (cfg, iargs [0], vtable);
			iargs [1] = args [0];
			return mono_emit_method_call (cfg, managed_alloc, iargs, this);
		}
	}
	return NULL;
}

static void
mono_save_args (MonoCompile *cfg, MonoMethodSignature *sig, MonoInst **sp)
{
	MonoInst *store, *temp;
	int i;

	for (i = 0; i < sig->param_count + sig->hasthis; ++i) {
		MonoType *argtype = (sig->hasthis && (i == 0)) ? type_from_stack_type (*sp) : sig->params [i - sig->hasthis];

		/*
		 * FIXME: We should use *args++ = sp [0], but that would mean the arg
		 * would be different than the MonoInst's used to represent arguments, and
		 * the ldelema implementation can't deal with that.
		 * Solution: When ldelema is used on an inline argument, create a var for 
		 * it, emit ldelema on that var, and emit the saving code below in
		 * inline_method () if needed.
		 */
		temp = mono_compile_create_var (cfg, argtype, OP_LOCAL);
		cfg->args [i] = temp;
		/* This uses cfg->args [i] which is set by the preceeding line */
		EMIT_NEW_ARGSTORE (cfg, store, i, *sp);
		store->cil_code = sp [0]->cil_code;
		sp++;
	}
}

#define MONO_INLINE_CALLED_LIMITED_METHODS 1
#define MONO_INLINE_CALLER_LIMITED_METHODS 1

#if (MONO_INLINE_CALLED_LIMITED_METHODS)
static gboolean
check_inline_called_method_name_limit (MonoMethod *called_method)
{
	int strncmp_result;
	static char *limit = NULL;
	
	if (limit == NULL) {
		char *limit_string = getenv ("MONO_INLINE_CALLED_METHOD_NAME_LIMIT");

		if (limit_string != NULL)
			limit = limit_string;
		else
			limit = (char *) "";
	}

	if (limit [0] != '\0') {
		char *called_method_name = mono_method_full_name (called_method, TRUE);

		strncmp_result = strncmp (called_method_name, limit, strlen (limit));
		g_free (called_method_name);
	
		//return (strncmp_result <= 0);
		return (strncmp_result == 0);
	} else {
		return TRUE;
	}
}
#endif

#if (MONO_INLINE_CALLER_LIMITED_METHODS)
static gboolean
check_inline_caller_method_name_limit (MonoMethod *caller_method)
{
	int strncmp_result;
	static char *limit = NULL;
	
	if (limit == NULL) {
		char *limit_string = getenv ("MONO_INLINE_CALLER_METHOD_NAME_LIMIT");
		if (limit_string != NULL) {
			limit = limit_string;
		} else {
			limit = (char *) "";
		}
	}

	if (limit [0] != '\0') {
		char *caller_method_name = mono_method_full_name (caller_method, TRUE);

		strncmp_result = strncmp (caller_method_name, limit, strlen (limit));
		g_free (caller_method_name);
	
		//return (strncmp_result <= 0);
		return (strncmp_result == 0);
	} else {
		return TRUE;
	}
}
#endif

static int
inline_method (MonoCompile *cfg, MonoMethod *cmethod, MonoMethodSignature *fsig, MonoInst **sp,
		guchar *ip, guint real_offset, GList *dont_inline, gboolean inline_allways)
{
	MonoInst *ins, *rvar = NULL;
	MonoMethodHeader *cheader;
	MonoBasicBlock *ebblock, *sbblock;
	int i, costs;
	MonoMethod *prev_inlined_method;
	MonoInst **prev_locals, **prev_args;
	MonoType **prev_arg_types;
	guint prev_real_offset;
	GHashTable *prev_cbb_hash;
	MonoBasicBlock **prev_cil_offset_to_bb;
	MonoBasicBlock *prev_cbb;
	unsigned char* prev_cil_start;
	guint32 prev_cil_offset_to_bb_len;
	MonoMethod *prev_current_method;
	MonoGenericContext *prev_generic_context;
	gboolean ret_var_set, prev_ret_var_set;

	g_assert (cfg->exception_type == MONO_EXCEPTION_NONE);

#if (MONO_INLINE_CALLED_LIMITED_METHODS)
	if ((! inline_allways) && ! check_inline_called_method_name_limit (cmethod))
		return 0;
#endif
#if (MONO_INLINE_CALLER_LIMITED_METHODS)
	if ((! inline_allways) && ! check_inline_caller_method_name_limit (cfg->method))
		return 0;
#endif

	if (cfg->verbose_level > 2)
		printf ("INLINE START %p %s -> %s\n", cmethod,  mono_method_full_name (cfg->method, TRUE), mono_method_full_name (cmethod, TRUE));

	if (!cmethod->inline_info) {
		mono_jit_stats.inlineable_methods++;
		cmethod->inline_info = 1;
	}
	/* allocate space to store the return value */
	if (!MONO_TYPE_IS_VOID (fsig->ret)) {
		rvar = mono_compile_create_var (cfg, fsig->ret, OP_LOCAL);
	}

	/* allocate local variables */
	cheader = mono_method_get_header (cmethod);
	prev_locals = cfg->locals;
	cfg->locals = mono_mempool_alloc0 (cfg->mempool, cheader->num_locals * sizeof (MonoInst*));	
	for (i = 0; i < cheader->num_locals; ++i)
		cfg->locals [i] = mono_compile_create_var (cfg, cheader->locals [i], OP_LOCAL);

	/* allocate start and end blocks */
	/* This is needed so if the inline is aborted, we can clean up */
	NEW_BBLOCK (cfg, sbblock);
	sbblock->real_offset = real_offset;

	NEW_BBLOCK (cfg, ebblock);
	ebblock->block_num = cfg->num_bblocks++;
	ebblock->real_offset = real_offset;

	prev_args = cfg->args;
	prev_arg_types = cfg->arg_types;
	prev_inlined_method = cfg->inlined_method;
	cfg->inlined_method = cmethod;
	cfg->ret_var_set = FALSE;
	prev_real_offset = cfg->real_offset;
	prev_cbb_hash = cfg->cbb_hash;
	prev_cil_offset_to_bb = cfg->cil_offset_to_bb;
	prev_cil_offset_to_bb_len = cfg->cil_offset_to_bb_len;
	prev_cil_start = cfg->cil_start;
	prev_cbb = cfg->cbb;
	prev_current_method = cfg->current_method;
	prev_generic_context = cfg->generic_context;
	prev_ret_var_set = cfg->ret_var_set;

	costs = mono_method_to_ir (cfg, cmethod, sbblock, ebblock, rvar, dont_inline, sp, real_offset, *ip == CEE_CALLVIRT);

	ret_var_set = cfg->ret_var_set;

	cfg->inlined_method = prev_inlined_method;
	cfg->real_offset = prev_real_offset;
	cfg->cbb_hash = prev_cbb_hash;
	cfg->cil_offset_to_bb = prev_cil_offset_to_bb;
	cfg->cil_offset_to_bb_len = prev_cil_offset_to_bb_len;
	cfg->cil_start = prev_cil_start;
	cfg->locals = prev_locals;
	cfg->args = prev_args;
	cfg->arg_types = prev_arg_types;
	cfg->current_method = prev_current_method;
	cfg->generic_context = prev_generic_context;
	cfg->ret_var_set = prev_ret_var_set;

	if ((costs >= 0 && costs < 60) || inline_allways) {
		if (cfg->verbose_level > 2)
			printf ("INLINE END %s -> %s\n", mono_method_full_name (cfg->method, TRUE), mono_method_full_name (cmethod, TRUE));
		
		mono_jit_stats.inlined_methods++;

		/* always add some code to avoid block split failures */
		MONO_INST_NEW (cfg, ins, OP_NOP);
		MONO_ADD_INS (prev_cbb, ins);

		prev_cbb->next_bb = sbblock;
		link_bblock (cfg, prev_cbb, sbblock);

		/* 
		 * Get rid of the begin and end bblocks if possible to aid local
		 * optimizations.
		 */
		mono_merge_basic_blocks (cfg, prev_cbb, sbblock);

		if ((prev_cbb->out_count == 1) && (prev_cbb->out_bb [0]->in_count == 1) && (prev_cbb->out_bb [0] != ebblock))
			mono_merge_basic_blocks (cfg, prev_cbb, prev_cbb->out_bb [0]);

		if ((ebblock->in_count == 1) && ebblock->in_bb [0]->out_count == 1) {
			MonoBasicBlock *prev = ebblock->in_bb [0];
			mono_merge_basic_blocks (cfg, prev, ebblock);
			cfg->cbb = prev;
			if ((prev_cbb->out_count == 1) && (prev_cbb->out_bb [0]->in_count == 1) && (prev_cbb->out_bb [0] == prev)) {
				mono_merge_basic_blocks (cfg, prev_cbb, prev);
				cfg->cbb = prev_cbb;
			}
		} else {
			cfg->cbb = ebblock;
		}

		if (rvar) {
			/*
			 * If the inlined method contains only a throw, then the ret var is not 
			 * set, so set it to a dummy value.
			 */
			if (!ret_var_set) {
				static double r8_0 = 0.0;

				switch (rvar->type) {
				case STACK_I4:
					MONO_EMIT_NEW_ICONST (cfg, rvar->dreg, 0);
					break;
				case STACK_I8:
					MONO_EMIT_NEW_I8CONST (cfg, rvar->dreg, 0);
					break;
				case STACK_PTR:
				case STACK_MP:
				case STACK_OBJ:
					MONO_EMIT_NEW_PCONST (cfg, rvar->dreg, 0);
					break;
				case STACK_R8:
					MONO_INST_NEW (cfg, ins, OP_R8CONST);
					ins->type = STACK_R8;
					ins->inst_p0 = (void*)&r8_0;
					ins->dreg = rvar->dreg;
					MONO_ADD_INS (cfg->cbb, ins);
					break;
				case STACK_VTYPE:
					MONO_EMIT_NEW_VZERO (cfg, rvar->dreg, mono_class_from_mono_type (fsig->ret));
					break;
				default:
					g_assert_not_reached ();
				}
			}

			EMIT_NEW_TEMPLOAD (cfg, ins, rvar->inst_c0);
			*sp++ = ins;
		}
		return costs + 1;
	} else {
		if (cfg->verbose_level > 2)
			printf ("INLINE ABORTED %s\n", mono_method_full_name (cmethod, TRUE));
		cfg->exception_type = MONO_EXCEPTION_NONE;
		mono_loader_clear_error ();

		/* This gets rid of the newly added bblocks */
		cfg->cbb = prev_cbb;
	}
	return 0;
}

/*
 * Some of these comments may well be out-of-date.
 * Design decisions: we do a single pass over the IL code (and we do bblock 
 * splitting/merging in the few cases when it's required: a back jump to an IL
 * address that was not already seen as bblock starting point).
 * Code is validated as we go (full verification is still better left to metadata/verify.c).
 * Complex operations are decomposed in simpler ones right away. We need to let the 
 * arch-specific code peek and poke inside this process somehow (except when the 
 * optimizations can take advantage of the full semantic info of coarse opcodes).
 * All the opcodes of the form opcode.s are 'normalized' to opcode.
 * MonoInst->opcode initially is the IL opcode or some simplification of that 
 * (OP_LOAD, OP_STORE). The arch-specific code may rearrange it to an arch-specific 
 * opcode with value bigger than OP_LAST.
 * At this point the IR can be handed over to an interpreter, a dumb code generator
 * or to the optimizing code generator that will translate it to SSA form.
 *
 * Profiling directed optimizations.
 * We may compile by default with few or no optimizations and instrument the code
 * or the user may indicate what methods to optimize the most either in a config file
 * or through repeated runs where the compiler applies offline the optimizations to 
 * each method and then decides if it was worth it.
 */

#define CHECK_TYPE(ins) if (!(ins)->type) UNVERIFIED
#define CHECK_STACK(num) if ((sp - stack_start) < (num)) UNVERIFIED
#define CHECK_STACK_OVF(num) if (((sp - stack_start) + (num)) > header->max_stack) UNVERIFIED
#define CHECK_ARG(num) if ((unsigned)(num) >= (unsigned)num_args) UNVERIFIED
#define CHECK_LOCAL(num) if ((unsigned)(num) >= (unsigned)header->num_locals) UNVERIFIED
#define CHECK_OPSIZE(size) if (ip + size > end) UNVERIFIED
#define CHECK_UNVERIFIABLE(cfg) if (cfg->unverifiable) UNVERIFIED
#define CHECK_TYPELOAD(klass) if (!(klass) || (klass)->exception_type) {cfg->exception_ptr = klass; goto load_error;}

/* offset from br.s -> br like opcodes */
#define BIG_BRANCH_OFFSET 13

static gboolean
ip_in_bb (MonoCompile *cfg, MonoBasicBlock *bb, const guint8* ip)
{
	MonoBasicBlock *b = cfg->cil_offset_to_bb [ip - cfg->cil_start];

	return b == NULL || b == bb;
}

static int
get_basic_blocks (MonoCompile *cfg, MonoMethodHeader* header, guint real_offset, unsigned char *start, unsigned char *end, unsigned char **pos)
{
	unsigned char *ip = start;
	unsigned char *target;
	int i;
	guint cli_addr;
	MonoBasicBlock *bblock;
	const MonoOpcode *opcode;

	while (ip < end) {
		cli_addr = ip - start;
		i = mono_opcode_value ((const guint8 **)&ip, end);
		if (i < 0)
			UNVERIFIED;
		opcode = &mono_opcodes [i];
		switch (opcode->argument) {
		case MonoInlineNone:
			ip++; 
			break;
		case MonoInlineString:
		case MonoInlineType:
		case MonoInlineField:
		case MonoInlineMethod:
		case MonoInlineTok:
		case MonoInlineSig:
		case MonoShortInlineR:
		case MonoInlineI:
			ip += 5;
			break;
		case MonoInlineVar:
			ip += 3;
			break;
		case MonoShortInlineVar:
		case MonoShortInlineI:
			ip += 2;
			break;
		case MonoShortInlineBrTarget:
			target = start + cli_addr + 2 + (signed char)ip [1];
			GET_BBLOCK (cfg, bblock, target);
			ip += 2;
			if (ip < end)
				GET_BBLOCK (cfg, bblock, ip);
			break;
		case MonoInlineBrTarget:
			target = start + cli_addr + 5 + (gint32)read32 (ip + 1);
			GET_BBLOCK (cfg, bblock, target);
			ip += 5;
			if (ip < end)
				GET_BBLOCK (cfg, bblock, ip);
			break;
		case MonoInlineSwitch: {
			guint32 n = read32 (ip + 1);
			guint32 j;
			ip += 5;
			cli_addr += 5 + 4 * n;
			target = start + cli_addr;
			GET_BBLOCK (cfg, bblock, target);
			
			for (j = 0; j < n; ++j) {
				target = start + cli_addr + (gint32)read32 (ip);
				GET_BBLOCK (cfg, bblock, target);
				ip += 4;
			}
			break;
		}
		case MonoInlineR:
		case MonoInlineI8:
			ip += 9;
			break;
		default:
			g_assert_not_reached ();
		}

		if (i == CEE_THROW) {
			unsigned char *bb_start = ip - 1;
			
			/* Find the start of the bblock containing the throw */
			bblock = NULL;
			while ((bb_start >= start) && !bblock) {
				bblock = cfg->cil_offset_to_bb [(bb_start) - start];
				bb_start --;
			}
			if (bblock)
				bblock->out_of_line = 1;
		}
	}
	return 0;
unverified:
	*pos = ip;
	return 1;
}

static inline MonoMethod *
mini_get_method_allow_open (MonoMethod *m, guint32 token, MonoClass *klass, MonoGenericContext *context)
{
	MonoMethod *method;

	if (m->wrapper_type != MONO_WRAPPER_NONE)
		return mono_method_get_wrapper_data (m, token);

	method = mono_get_method_full (m->klass->image, token, klass, context);

	return method;
}

static inline MonoMethod *
mini_get_method (MonoCompile *cfg, MonoMethod *m, guint32 token, MonoClass *klass, MonoGenericContext *context)
{
	MonoMethod *method = mini_get_method_allow_open (m, token, klass, context);

	if (method && cfg && !cfg->generic_sharing_context && mono_class_is_open_constructed_type (&method->klass->byval_arg))
		return NULL;

	return method;
}

static inline MonoClass*
mini_get_class (MonoMethod *method, guint32 token, MonoGenericContext *context)
{
	MonoClass *klass;

	if (method->wrapper_type != MONO_WRAPPER_NONE)
		klass = mono_method_get_wrapper_data (method, token);
	else
		klass = mono_class_get_full (method->klass->image, token, context);
	if (klass)
		mono_class_init (klass);
	return klass;
}

/*
 * Returns TRUE if the JIT should abort inlining because "callee"
 * is influenced by security attributes.
 */
static
gboolean check_linkdemand (MonoCompile *cfg, MonoMethod *caller, MonoMethod *callee)
{
	guint32 result;
	
	if ((cfg->method != caller) && mono_method_has_declsec (callee)) {
		return TRUE;
	}
	
	result = mono_declsec_linkdemand (cfg->domain, caller, callee);
	if (result == MONO_JIT_SECURITY_OK)
		return FALSE;

	if (result == MONO_JIT_LINKDEMAND_ECMA) {
		/* Generate code to throw a SecurityException before the actual call/link */
		MonoSecurityManager *secman = mono_security_manager_get_methods ();
		MonoInst *args [2];

		NEW_ICONST (cfg, args [0], 4);
		NEW_METHODCONST (cfg, args [1], caller);
		mono_emit_method_call (cfg, secman->linkdemandsecurityexception, args, NULL);
	} else if (cfg->exception_type == MONO_EXCEPTION_NONE) {
		 /* don't hide previous results */
		cfg->exception_type = MONO_EXCEPTION_SECURITY_LINKDEMAND;
		cfg->exception_data = result;
		return TRUE;
	}
	
	return FALSE;
}

static MonoMethod*
method_access_exception (void)
{
	static MonoMethod *method = NULL;

	if (!method) {
		MonoSecurityManager *secman = mono_security_manager_get_methods ();
		method = mono_class_get_method_from_name (secman->securitymanager,
							  "MethodAccessException", 2);
	}
	g_assert (method);
	return method;
}

static void
emit_throw_method_access_exception (MonoCompile *cfg, MonoMethod *caller, MonoMethod *callee,
				    MonoBasicBlock *bblock, unsigned char *ip)
{
	MonoMethod *thrower = method_access_exception ();
	MonoInst *args [2];

	EMIT_NEW_METHODCONST (cfg, args [0], caller);
	EMIT_NEW_METHODCONST (cfg, args [1], callee);
	mono_emit_method_call (cfg, thrower, args, NULL);
}

static MonoMethod*
verification_exception (void)
{
	static MonoMethod *method = NULL;

	if (!method) {
		MonoSecurityManager *secman = mono_security_manager_get_methods ();
		method = mono_class_get_method_from_name (secman->securitymanager,
							  "VerificationException", 0);
	}
	g_assert (method);
	return method;
}

static void
emit_throw_verification_exception (MonoCompile *cfg, MonoBasicBlock *bblock, unsigned char *ip)
{
	MonoMethod *thrower = verification_exception ();

	mono_emit_method_call (cfg, thrower, NULL, NULL);
}

static void
ensure_method_is_allowed_to_call_method (MonoCompile *cfg, MonoMethod *caller, MonoMethod *callee,
					 MonoBasicBlock *bblock, unsigned char *ip)
{
	MonoSecurityCoreCLRLevel caller_level = mono_security_core_clr_method_level (caller, TRUE);
	MonoSecurityCoreCLRLevel callee_level = mono_security_core_clr_method_level (callee, TRUE);
	gboolean is_safe = TRUE;

	if (!(caller_level >= callee_level ||
			caller_level == MONO_SECURITY_CORE_CLR_SAFE_CRITICAL ||
			callee_level == MONO_SECURITY_CORE_CLR_SAFE_CRITICAL)) {
		is_safe = FALSE;
	}

	if (!is_safe)
		emit_throw_method_access_exception (cfg, caller, callee, bblock, ip);
}

static gboolean
method_is_safe (MonoMethod *method)
{
	/*
	if (strcmp (method->name, "unsafeMethod") == 0)
		return FALSE;
	*/
	return TRUE;
}

/*
 * Check that the IL instructions at ip are the array initialization
 * sequence and return the pointer to the data and the size.
 */
static const char*
initialize_array_data (MonoMethod *method, gboolean aot, unsigned char *ip, MonoClass *klass, guint32 len, int *out_size, guint32 *out_field_token)
{
	/*
	 * newarr[System.Int32]
	 * dup
	 * ldtoken field valuetype ...
	 * call void class [mscorlib]System.Runtime.CompilerServices.RuntimeHelpers::InitializeArray(class [mscorlib]System.Array, valuetype [mscorlib]System.RuntimeFieldHandle)
	 */
	if (ip [0] == CEE_DUP && ip [1] == CEE_LDTOKEN && ip [5] == 0x4 && ip [6] == CEE_CALL) {
		guint32 token = read32 (ip + 7);
		guint32 field_token = read32 (ip + 2);
		guint32 field_index = field_token & 0xffffff;
		guint32 rva;
		const char *data_ptr;
		int size = 0;
		MonoMethod *cmethod;
		MonoClass *dummy_class;
		MonoClassField *field = mono_field_from_token (method->klass->image, field_token, &dummy_class, NULL);
		int dummy_align;

		if (!field)
			return NULL;

		*out_field_token = field_token;

		cmethod = mini_get_method (NULL, method, token, NULL, NULL);
		if (!cmethod)
			return NULL;
		if (strcmp (cmethod->name, "InitializeArray") || strcmp (cmethod->klass->name, "RuntimeHelpers") || cmethod->klass->image != mono_defaults.corlib)
			return NULL;
		switch (mono_type_get_underlying_type (&klass->byval_arg)->type) {
		case MONO_TYPE_BOOLEAN:
		case MONO_TYPE_I1:
		case MONO_TYPE_U1:
			size = 1; break;
		/* we need to swap on big endian, so punt. Should we handle R4 and R8 as well? */
#if G_BYTE_ORDER == G_LITTLE_ENDIAN
		case MONO_TYPE_CHAR:
		case MONO_TYPE_I2:
		case MONO_TYPE_U2:
			size = 2; break;
		case MONO_TYPE_I4:
		case MONO_TYPE_U4:
		case MONO_TYPE_R4:
			size = 4; break;
		case MONO_TYPE_R8:
#ifdef ARM_FPU_FPA
			return NULL; /* stupid ARM FP swapped format */
#endif
		case MONO_TYPE_I8:
		case MONO_TYPE_U8:
			size = 8; break;
#endif
		default:
			return NULL;
		}
		size *= len;
		if (size > mono_type_size (field->type, &dummy_align))
		    return NULL;
		*out_size = size;
		/*g_print ("optimized in %s: size: %d, numelems: %d\n", method->name, size, newarr->inst_newa_len->inst_c0);*/
		if (!method->klass->image->dynamic) {
			field_index = read32 (ip + 2) & 0xffffff;
			mono_metadata_field_info (method->klass->image, field_index - 1, NULL, &rva, NULL);
			data_ptr = mono_image_rva_map (method->klass->image, rva);
			/*g_print ("field: 0x%08x, rva: %d, rva_ptr: %p\n", read32 (ip + 2), rva, data_ptr);*/
			/* for aot code we do the lookup on load */
			if (aot && data_ptr)
				return GUINT_TO_POINTER (rva);
		} else {
			/*FIXME is it possible to AOT a SRE assembly not meant to be saved? */ 
			g_assert (!aot);
			data_ptr = mono_field_get_data (field);
		}
		return data_ptr;
	}
	return NULL;
}

static void
set_exception_type_from_invalid_il (MonoCompile *cfg, MonoMethod *method, unsigned char *ip)
{
	char *method_fname = mono_method_full_name (method, TRUE);
	char *method_code;

	if (mono_method_get_header (method)->code_size == 0)
		method_code = g_strdup ("method body is empty.");
	else
		method_code = mono_disasm_code_one (NULL, method, ip, NULL);
 	cfg->exception_type = MONO_EXCEPTION_INVALID_PROGRAM;
 	cfg->exception_message = g_strdup_printf ("Invalid IL code in %s: %s\n", method_fname, method_code);
 	g_free (method_fname);
 	g_free (method_code);
}

static void
set_exception_object (MonoCompile *cfg, MonoException *exception)
{
	cfg->exception_type = MONO_EXCEPTION_OBJECT_SUPPLIED;
	MONO_GC_REGISTER_ROOT (cfg->exception_ptr);
	cfg->exception_ptr = exception;
}

static gboolean
generic_class_is_reference_type (MonoCompile *cfg, MonoClass *klass)
{
	MonoType *type;

	if (cfg->generic_sharing_context)
		type = mini_get_basic_type_from_generic (cfg->generic_sharing_context, &klass->byval_arg);
	else
		type = &klass->byval_arg;
	return MONO_TYPE_IS_REFERENCE (type);
}

/**
 * mono_decompose_array_access_opts:
 *
 *  Decompose array access opcodes.
 * This should be in decompose.c, but it emits calls so it has to stay here until
 * the old JIT is gone.
 */
void
mono_decompose_array_access_opts (MonoCompile *cfg)
{
	MonoBasicBlock *bb, *first_bb;

	/*
	 * Unlike decompose_long_opts, this pass does not alter the CFG of the method so it 
	 * can be executed anytime. It should be run before decompose_long
	 */

	/**
	 * Create a dummy bblock and emit code into it so we can use the normal 
	 * code generation macros.
	 */
	cfg->cbb = mono_mempool_alloc0 ((cfg)->mempool, sizeof (MonoBasicBlock));
	first_bb = cfg->cbb;

	for (bb = cfg->bb_entry; bb; bb = bb->next_bb) {
		MonoInst *ins;
		MonoInst *prev = NULL;
		MonoInst *dest;
		MonoInst *iargs [3];
		gboolean restart;

		if (!bb->has_array_access)
			continue;

		if (cfg->verbose_level > 3) mono_print_bb (bb, "BEFORE DECOMPOSE-ARRAY-ACCESS-OPTS ");

		cfg->cbb->code = cfg->cbb->last_ins = NULL;
		restart = TRUE;

		while (restart) {
			restart = FALSE;

			for (ins = bb->code; ins; ins = ins->next) {
				switch (ins->opcode) {
				case OP_LDLEN:
					NEW_LOAD_MEMBASE (cfg, dest, OP_LOADI4_MEMBASE, ins->dreg, ins->sreg1,
									  G_STRUCT_OFFSET (MonoArray, max_length));
					MONO_ADD_INS (cfg->cbb, dest);
					break;
				case OP_BOUNDS_CHECK:
					MONO_ARCH_EMIT_BOUNDS_CHECK (cfg, ins->sreg1, ins->inst_imm, ins->sreg2);
					break;
				case OP_NEWARR:
					if (cfg->opt & MONO_OPT_SHARED) {
						EMIT_NEW_DOMAINCONST (cfg, iargs [0]);
						EMIT_NEW_CLASSCONST (cfg, iargs [1], ins->inst_newa_class);
						MONO_INST_NEW (cfg, iargs [2], OP_MOVE);
						iargs [2]->dreg = ins->sreg1;

						dest = mono_emit_jit_icall (cfg, mono_array_new, iargs);
						dest->dreg = ins->dreg;
					} else {
						MonoVTable *vtable = mono_class_vtable (cfg->domain, mono_array_class_get (ins->inst_newa_class, 1));

						g_assert (vtable);
						NEW_VTABLECONST (cfg, iargs [0], vtable);
						MONO_ADD_INS (cfg->cbb, iargs [0]);
						MONO_INST_NEW (cfg, iargs [1], OP_MOVE);
						iargs [1]->dreg = ins->sreg1;

						dest = mono_emit_jit_icall (cfg, mono_array_new_specific, iargs);
						dest->dreg = ins->dreg;
					}
					break;
				case OP_STRLEN:
					NEW_LOAD_MEMBASE (cfg, dest, OP_LOADI4_MEMBASE, ins->dreg,
									  ins->sreg1, G_STRUCT_OFFSET (MonoString, length));
					MONO_ADD_INS (cfg->cbb, dest);
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
				}
				else
					prev = ins;
			}
		}

		if (cfg->verbose_level > 3) mono_print_bb (bb, "AFTER DECOMPOSE-ARRAY-ACCESS-OPTS ");
	}
}

typedef union {
	guint32 vali [2];
	gint64 vall;
	double vald;
} DVal;

#ifdef MONO_ARCH_SOFT_FLOAT

/**
 * mono_decompose_soft_float:
 *
 *  Soft float support on ARM. We store each double value in a pair of integer vregs,
 * similar to long support on 32 bit platforms. 32 bit float values require special
 * handling when used as locals, arguments, and in calls.
 * One big problem with soft-float is that there are few r4 test cases in our test suite.
 */
void
mono_decompose_soft_float (MonoCompile *cfg)
{
	MonoBasicBlock *bb, *first_bb;

	/*
	 * This pass creates long opcodes, so it should be run before decompose_long_opts ().
	 */

	/**
	 * Create a dummy bblock and emit code into it so we can use the normal 
	 * code generation macros.
	 */
	cfg->cbb = mono_mempool_alloc0 ((cfg)->mempool, sizeof (MonoBasicBlock));
	first_bb = cfg->cbb;

	for (bb = cfg->bb_entry; bb; bb = bb->next_bb) {
		MonoInst *ins;
		MonoInst *prev = NULL;
		gboolean restart;

		if (cfg->verbose_level > 3) mono_print_bb (bb, "BEFORE HANDLE-SOFT-FLOAT ");

		cfg->cbb->code = cfg->cbb->last_ins = NULL;
		restart = TRUE;

		while (restart) {
			restart = FALSE;

			for (ins = bb->code; ins; ins = ins->next) {
				const char *spec = INS_INFO (ins->opcode);

				/* Most fp operations are handled automatically by opcode emulation */

				switch (ins->opcode) {
				case OP_R8CONST: {
					DVal d;
					d.vald = *(double*)ins->inst_p0;
					MONO_EMIT_NEW_I8CONST (cfg, ins->dreg, d.vall);
					break;
				}
				case OP_R4CONST: {
					DVal d;
					/* We load the r8 value */
					d.vald = *(float*)ins->inst_p0;
					MONO_EMIT_NEW_I8CONST (cfg, ins->dreg, d.vall);
					break;
				}
				case OP_FMOVE:
					ins->opcode = OP_LMOVE;
					break;
				case OP_FGETLOW32:
					ins->opcode = OP_MOVE;
					ins->sreg1 = ins->sreg1 + 1;
					break;
				case OP_FGETHIGH32:
					ins->opcode = OP_MOVE;
					ins->sreg1 = ins->sreg1 + 2;
					break;
				case OP_SETFRET: {
					int reg = ins->sreg1;

					ins->opcode = OP_SETLRET;
					ins->dreg = -1;
					ins->sreg1 = reg + 1;
					ins->sreg2 = reg + 2;
					break;
				}
				case OP_LOADR8_MEMBASE:
					ins->opcode = OP_LOADI8_MEMBASE;
					break;
				case OP_STORER8_MEMBASE_REG:
					ins->opcode = OP_STOREI8_MEMBASE_REG;
					break;
				case OP_STORER4_MEMBASE_REG: {
					MonoInst *iargs [2];
					int addr_reg;

					/* Arg 1 is the double value */
					MONO_INST_NEW (cfg, iargs [0], OP_ARG);
					iargs [0]->dreg = ins->sreg1;

					/* Arg 2 is the address to store to */
					addr_reg = mono_alloc_preg (cfg);
					EMIT_NEW_BIALU_IMM (cfg, iargs [1], OP_PADD_IMM, addr_reg, ins->inst_destbasereg, ins->inst_offset);
					mono_emit_jit_icall (cfg, mono_fstore_r4, iargs);
					restart = TRUE;
					break;
				}
				case OP_LOADR4_MEMBASE: {
					MonoInst *iargs [1];
					MonoInst *conv;
					int addr_reg;

					addr_reg = mono_alloc_preg (cfg);
					EMIT_NEW_BIALU_IMM (cfg, iargs [0], OP_PADD_IMM, addr_reg, ins->inst_basereg, ins->inst_offset);
					conv = mono_emit_jit_icall (cfg, mono_fload_r4, iargs);
					conv->dreg = ins->dreg;
					break;
				}					
				case OP_FCALL:
				case OP_FCALL_REG:
				case OP_FCALL_MEMBASE: {
					MonoCallInst *call = (MonoCallInst*)ins;
					if (call->signature->ret->type == MONO_TYPE_R4) {
						MonoCallInst *call2;
						MonoInst *iargs [1];
						MonoInst *conv;

						/* Convert the call into a call returning an int */
						MONO_INST_NEW_CALL (cfg, call2, OP_CALL);
						memcpy (call2, call, sizeof (MonoCallInst));
						switch (ins->opcode) {
						case OP_FCALL:
							call2->inst.opcode = OP_CALL;
							break;
						case OP_FCALL_REG:
							call2->inst.opcode = OP_CALL_REG;
							break;
						case OP_FCALL_MEMBASE:
							call2->inst.opcode = OP_CALL_MEMBASE;
							break;
						default:
							g_assert_not_reached ();
						}
						call2->inst.dreg = mono_alloc_ireg (cfg);
						MONO_ADD_INS (cfg->cbb, (MonoInst*)call2);

						/* FIXME: Optimize this */

						/* Emit an r4->r8 conversion */
						EMIT_NEW_VARLOADA_VREG (cfg, iargs [0], call2->inst.dreg, &mono_defaults.int32_class->byval_arg);
						conv = mono_emit_jit_icall (cfg, mono_fload_r4, iargs);
						conv->dreg = ins->dreg;
					} else {
						switch (ins->opcode) {
						case OP_FCALL:
							ins->opcode = OP_LCALL;
							break;
						case OP_FCALL_REG:
							ins->opcode = OP_LCALL_REG;
							break;
						case OP_FCALL_MEMBASE:
							ins->opcode = OP_LCALL_MEMBASE;
							break;
						default:
							g_assert_not_reached ();
						}
					}
					break;
				}
				case OP_FCOMPARE: {
					MonoJitICallInfo *info;
					MonoInst *iargs [2];
					MonoInst *call, *cmp, *br;

					/* Convert fcompare+fbcc to icall+icompare+beq */

					info = mono_find_jit_opcode_emulation (ins->next->opcode);
					g_assert (info);

					/* Create dummy MonoInst's for the arguments */
					MONO_INST_NEW (cfg, iargs [0], OP_ARG);
					iargs [0]->dreg = ins->sreg1;
					MONO_INST_NEW (cfg, iargs [1], OP_ARG);
					iargs [1]->dreg = ins->sreg2;

					call = mono_emit_native_call (cfg, mono_icall_get_wrapper (info), info->sig, iargs);

					MONO_INST_NEW (cfg, cmp, OP_ICOMPARE_IMM);
					cmp->sreg1 = call->dreg;
					cmp->inst_imm = 0;
					MONO_ADD_INS (cfg->cbb, cmp);
					
					MONO_INST_NEW (cfg, br, OP_IBNE_UN);
					br->inst_many_bb = mono_mempool_alloc (cfg->mempool, sizeof (gpointer) * 2);
					br->inst_true_bb = ins->next->inst_true_bb;
					br->inst_false_bb = ins->next->inst_false_bb;
					MONO_ADD_INS (cfg->cbb, br);

					/* The call sequence might include fp ins */
					restart = TRUE;

					/* Skip fbcc or fccc */
					NULLIFY_INS (ins->next);
					break;
				}
				case OP_FCEQ:
				case OP_FCGT:
				case OP_FCGT_UN:
				case OP_FCLT:
				case OP_FCLT_UN: {
					MonoJitICallInfo *info;
					MonoInst *iargs [2];
					MonoInst *call;

					/* Convert fccc to icall+icompare+iceq */

					info = mono_find_jit_opcode_emulation (ins->opcode);
					g_assert (info);

					/* Create dummy MonoInst's for the arguments */
					MONO_INST_NEW (cfg, iargs [0], OP_ARG);
					iargs [0]->dreg = ins->sreg1;
					MONO_INST_NEW (cfg, iargs [1], OP_ARG);
					iargs [1]->dreg = ins->sreg2;

					call = mono_emit_native_call (cfg, mono_icall_get_wrapper (info), info->sig, iargs);

					MONO_EMIT_NEW_BIALU_IMM (cfg, OP_ICOMPARE_IMM, -1, call->dreg, 1);
					MONO_EMIT_NEW_UNALU (cfg, OP_ICEQ, ins->dreg, -1);

					/* The call sequence might include fp ins */
					restart = TRUE;
					break;
				}
				default:
					if (spec [MONO_INST_SRC1] == 'f' || spec [MONO_INST_SRC2] == 'f' || spec [MONO_INST_DEST] == 'f') {
						mono_print_ins (ins);
						g_assert_not_reached ();
					}
					break;
				}

				g_assert (cfg->cbb == first_bb);

				if (cfg->cbb->code || (cfg->cbb != first_bb)) {
					/* Replace the original instruction with the new code sequence */

					mono_replace_ins (cfg, bb, ins, &prev, first_bb, cfg->cbb);
					first_bb->code = first_bb->last_ins = NULL;
					first_bb->in_count = first_bb->out_count = 0;
					cfg->cbb = first_bb;
				}
				else
					prev = ins;
			}
		}

		if (cfg->verbose_level > 3) mono_print_bb (bb, "AFTER HANDLE-SOFT-FLOAT ");
	}

	mono_decompose_long_opts (cfg);
}

#endif

static void
emit_stloc_ir (MonoCompile *cfg, MonoInst **sp, MonoMethodHeader *header, int n)
{
	MonoInst *ins;
	guint32 opcode = mono_type_to_regmove (cfg, header->locals [n]);
	if ((opcode == OP_MOVE) && cfg->cbb->last_ins == sp [0]  &&
			((sp [0]->opcode == OP_ICONST) || (sp [0]->opcode == OP_I8CONST))) {
		/* Optimize reg-reg moves away */
		/* 
		 * Can't optimize other opcodes, since sp[0] might point to
		 * the last ins of a decomposed opcode.
		 */
		sp [0]->dreg = (cfg)->locals [n]->dreg;
	} else {
		EMIT_NEW_LOCSTORE (cfg, ins, n, *sp);
	}
}

/*
 * ldloca inhibits many optimizations so try to get rid of it in common
 * cases.
 */
static inline unsigned char *
emit_optimized_ldloca_ir (MonoCompile *cfg, unsigned char *ip, unsigned char *end, int size)
{
	int local, token;
	MonoClass *klass;

	if (size == 1) {
		local = ip [1];
		ip += 2;
	} else {
		local = read16 (ip + 2);
		ip += 4;
	}
	
	if (ip + 6 < end && (ip [0] == CEE_PREFIX1) && (ip [1] == CEE_INITOBJ) && ip_in_bb (cfg, cfg->cbb, ip + 1)) {
		gboolean skip = FALSE;

		/* From the INITOBJ case */
		token = read32 (ip + 2);
		klass = mini_get_class (cfg->current_method, token, cfg->generic_context);
		CHECK_TYPELOAD (klass);
		if (generic_class_is_reference_type (cfg, klass)) {
			MONO_EMIT_NEW_PCONST (cfg, cfg->locals [local]->dreg, NULL);
		} else if (MONO_TYPE_IS_REFERENCE (&klass->byval_arg)) {
			MONO_EMIT_NEW_PCONST (cfg, cfg->locals [local]->dreg, NULL);
		} else if (MONO_TYPE_ISSTRUCT (&klass->byval_arg)) {
			MONO_EMIT_NEW_VZERO (cfg, cfg->locals [local]->dreg, klass);
		} else {
			skip = TRUE;
		}
			
		if (!skip)
			return ip + 6;
	}
load_error:
	return NULL;
}

static gboolean
is_exception_class (MonoClass *class)
{
	while (class) {
		if (class == mono_defaults.exception_class)
			return TRUE;
		class = class->parent;
	}
	return FALSE;
}

/*
 * mono_method_to_ir:
 *
 *   Translate the .net IL into linear IR.
 */
int
mono_method_to_ir (MonoCompile *cfg, MonoMethod *method, MonoBasicBlock *start_bblock, MonoBasicBlock *end_bblock, 
		   MonoInst *return_var, GList *dont_inline, MonoInst **inline_args, 
		   guint inline_offset, gboolean is_virtual_call)
{
	MonoInst *ins, **sp, **stack_start;
	MonoBasicBlock *bblock, *tblock = NULL, *init_localsbb = NULL;
	MonoMethod *cmethod, *method_definition;
	MonoInst **arg_array;
	MonoMethodHeader *header;
	MonoImage *image;
	guint32 token, ins_flag;
	MonoClass *klass;
	MonoClass *constrained_call = NULL;
	unsigned char *ip, *end, *target, *err_pos;
	static double r8_0 = 0.0;
	MonoMethodSignature *sig;
	MonoGenericContext *generic_context = NULL;
	MonoGenericContainer *generic_container = NULL;
	MonoType **param_types;
	int i, n, start_new_bblock, dreg;
	int num_calls = 0, inline_costs = 0;
	int breakpoint_id = 0;
	guint num_args;
	MonoBoolean security, pinvoke;
	MonoSecurityManager* secman = NULL;
	MonoDeclSecurityActions actions;
	GSList *class_inits = NULL;
	gboolean dont_verify, dont_verify_stloc, readonly = FALSE;
	int context_used;
	gboolean init_locals;

	/* serialization and xdomain stuff may need access to private fields and methods */
	dont_verify = method->klass->image->assembly->corlib_internal? TRUE: FALSE;
	dont_verify |= method->wrapper_type == MONO_WRAPPER_XDOMAIN_INVOKE;
	dont_verify |= method->wrapper_type == MONO_WRAPPER_XDOMAIN_DISPATCH;
 	dont_verify |= method->wrapper_type == MONO_WRAPPER_MANAGED_TO_NATIVE; /* bug #77896 */
	dont_verify |= method->wrapper_type == MONO_WRAPPER_COMINTEROP;
	dont_verify |= method->wrapper_type == MONO_WRAPPER_COMINTEROP_INVOKE;

	dont_verify |= mono_security_get_mode () == MONO_SECURITY_MODE_SMCS_HACK;

	/* still some type unsafety issues in marshal wrappers... (unknown is PtrToStructure) */
	dont_verify_stloc = method->wrapper_type == MONO_WRAPPER_MANAGED_TO_NATIVE;
	dont_verify_stloc |= method->wrapper_type == MONO_WRAPPER_UNKNOWN;
	dont_verify_stloc |= method->wrapper_type == MONO_WRAPPER_NATIVE_TO_MANAGED;

	image = method->klass->image;
	header = mono_method_get_header (method);
	generic_container = mono_method_get_generic_container (method);
	sig = mono_method_signature (method);
	num_args = sig->hasthis + sig->param_count;
	ip = (unsigned char*)header->code;
	cfg->cil_start = ip;
	end = ip + header->code_size;
	mono_jit_stats.cil_code_size += header->code_size;
	init_locals = header->init_locals;

	/* 
	 * Methods without init_locals set could cause asserts in various passes
	 * (#497220).
	 */
	init_locals = TRUE;

	method_definition = method;
	while (method_definition->is_inflated) {
		MonoMethodInflated *imethod = (MonoMethodInflated *) method_definition;
		method_definition = imethod->declaring;
	}

	/* SkipVerification is not allowed if core-clr is enabled */
	if (!dont_verify && mini_assembly_can_skip_verification (cfg->domain, method)) {
		dont_verify = TRUE;
		dont_verify_stloc = TRUE;
	}

	if (!dont_verify && mini_method_verify (cfg, method_definition))
		goto exception_exit;

	if (mono_debug_using_mono_debugger ())
		cfg->keep_cil_nops = TRUE;

	if (sig->is_inflated)
		generic_context = mono_method_get_context (method);
	else if (generic_container)
		generic_context = &generic_container->context;
	cfg->generic_context = generic_context;

	if (!cfg->generic_sharing_context)
		g_assert (!sig->has_type_parameters);

	if (sig->generic_param_count && method->wrapper_type == MONO_WRAPPER_NONE) {
		g_assert (method->is_inflated);
		g_assert (mono_method_get_context (method)->method_inst);
	}
	if (method->is_inflated && mono_method_get_context (method)->method_inst)
		g_assert (sig->generic_param_count);

	if (cfg->method == method) {
		cfg->real_offset = 0;
	} else {
		cfg->real_offset = inline_offset;
	}

	cfg->cil_offset_to_bb = mono_mempool_alloc0 (cfg->mempool, sizeof (MonoBasicBlock*) * header->code_size);
	cfg->cil_offset_to_bb_len = header->code_size;

	cfg->current_method = method;

	if (cfg->verbose_level > 2)
		printf ("method to IR %s\n", mono_method_full_name (method, TRUE));

	param_types = mono_mempool_alloc (cfg->mempool, sizeof (MonoType*) * num_args);
	if (sig->hasthis)
		param_types [0] = method->klass->valuetype?&method->klass->this_arg:&method->klass->byval_arg;
	for (n = 0; n < sig->param_count; ++n)
		param_types [n + sig->hasthis] = sig->params [n];
	cfg->arg_types = param_types;

	dont_inline = g_list_prepend (dont_inline, method);
	if (cfg->method == method) {

		if (cfg->prof_options & MONO_PROFILE_INS_COVERAGE)
			cfg->coverage_info = mono_profiler_coverage_alloc (cfg->method, header->code_size);

		/* ENTRY BLOCK */
		NEW_BBLOCK (cfg, start_bblock);
		cfg->bb_entry = start_bblock;
		start_bblock->cil_code = NULL;
		start_bblock->cil_length = 0;

		/* EXIT BLOCK */
		NEW_BBLOCK (cfg, end_bblock);
		cfg->bb_exit = end_bblock;
		end_bblock->cil_code = NULL;
		end_bblock->cil_length = 0;
		g_assert (cfg->num_bblocks == 2);

		arg_array = cfg->args;

		if (header->num_clauses) {
			cfg->spvars = g_hash_table_new (NULL, NULL);
			cfg->exvars = g_hash_table_new (NULL, NULL);
		}
		/* handle exception clauses */
		for (i = 0; i < header->num_clauses; ++i) {
			MonoBasicBlock *try_bb;
			MonoExceptionClause *clause = &header->clauses [i];
			GET_BBLOCK (cfg, try_bb, ip + clause->try_offset);
			try_bb->real_offset = clause->try_offset;
			GET_BBLOCK (cfg, tblock, ip + clause->handler_offset);
			tblock->real_offset = clause->handler_offset;
			tblock->flags |= BB_EXCEPTION_HANDLER;

			link_bblock (cfg, try_bb, tblock);

			if (*(ip + clause->handler_offset) == CEE_POP)
				tblock->flags |= BB_EXCEPTION_DEAD_OBJ;

			if (clause->flags == MONO_EXCEPTION_CLAUSE_FINALLY ||
			    clause->flags == MONO_EXCEPTION_CLAUSE_FILTER ||
			    clause->flags == MONO_EXCEPTION_CLAUSE_FAULT) {
				MONO_INST_NEW (cfg, ins, OP_START_HANDLER);
				MONO_ADD_INS (tblock, ins);

				/* todo: is a fault block unsafe to optimize? */
				if (clause->flags == MONO_EXCEPTION_CLAUSE_FAULT)
					tblock->flags |= BB_EXCEPTION_UNSAFE;
			}


			/*printf ("clause try IL_%04x to IL_%04x handler %d at IL_%04x to IL_%04x\n", clause->try_offset, clause->try_offset + clause->try_len, clause->flags, clause->handler_offset, clause->handler_offset + clause->handler_len);
			  while (p < end) {
			  printf ("%s", mono_disasm_code_one (NULL, method, p, &p));
			  }*/
			/* catch and filter blocks get the exception object on the stack */
			if (clause->flags == MONO_EXCEPTION_CLAUSE_NONE ||
			    clause->flags == MONO_EXCEPTION_CLAUSE_FILTER) {
				MonoInst *dummy_use;

				/* mostly like handle_stack_args (), but just sets the input args */
				/* printf ("handling clause at IL_%04x\n", clause->handler_offset); */
				tblock->in_scount = 1;
				tblock->in_stack = mono_mempool_alloc (cfg->mempool, sizeof (MonoInst*));
				tblock->in_stack [0] = mono_create_exvar_for_offset (cfg, clause->handler_offset);

				/* 
				 * Add a dummy use for the exvar so its liveness info will be
				 * correct.
				 */
				cfg->cbb = tblock;
				EMIT_NEW_DUMMY_USE (cfg, dummy_use, tblock->in_stack [0]);
				
				if (clause->flags == MONO_EXCEPTION_CLAUSE_FILTER) {
					GET_BBLOCK (cfg, tblock, ip + clause->data.filter_offset);
					tblock->flags |= BB_EXCEPTION_HANDLER;
					tblock->real_offset = clause->data.filter_offset;
					tblock->in_scount = 1;
					tblock->in_stack = mono_mempool_alloc (cfg->mempool, sizeof (MonoInst*));
					/* The filter block shares the exvar with the handler block */
					tblock->in_stack [0] = mono_create_exvar_for_offset (cfg, clause->handler_offset);
					MONO_INST_NEW (cfg, ins, OP_START_HANDLER);
					MONO_ADD_INS (tblock, ins);
				}
			}

			if (clause->flags != MONO_EXCEPTION_CLAUSE_FILTER &&
					clause->data.catch_class &&
					cfg->generic_sharing_context &&
					mono_class_check_context_used (clause->data.catch_class)) {
				/*
				 * In shared generic code with catch
				 * clauses containing type variables
				 * the exception handling code has to
				 * be able to get to the rgctx.
				 * Therefore we have to make sure that
				 * the vtable/mrgctx argument (for
				 * static or generic methods) or the
				 * "this" argument (for non-static
				 * methods) are live.
				 */
				if ((method->flags & METHOD_ATTRIBUTE_STATIC) ||
						mini_method_get_context (method)->method_inst ||
						method->klass->valuetype) {
					mono_get_vtable_var (cfg);
				} else {
					MonoInst *dummy_use;

					EMIT_NEW_DUMMY_USE (cfg, dummy_use, arg_array [0]);
				}
			}
		}
	} else {
		arg_array = alloca (sizeof (MonoInst *) * num_args);
		cfg->cbb = start_bblock;
		cfg->args = arg_array;
		mono_save_args (cfg, sig, inline_args);
	}

	/* FIRST CODE BLOCK */
	NEW_BBLOCK (cfg, bblock);
	bblock->cil_code = ip;
	cfg->cbb = bblock;
	cfg->ip = ip;

	ADD_BBLOCK (cfg, bblock);

	if (cfg->method == method) {
		breakpoint_id = mono_debugger_method_has_breakpoint (method);
		if (breakpoint_id && (mono_debug_format != MONO_DEBUG_FORMAT_DEBUGGER)) {
			MONO_INST_NEW (cfg, ins, OP_BREAK);
			MONO_ADD_INS (bblock, ins);
		}
	}

	if (mono_security_get_mode () == MONO_SECURITY_MODE_CAS)
		secman = mono_security_manager_get_methods ();

	security = (secman && mono_method_has_declsec (method));
	/* at this point having security doesn't mean we have any code to generate */
	if (security && (cfg->method == method)) {
		/* Only Demand, NonCasDemand and DemandChoice requires code generation.
		 * And we do not want to enter the next section (with allocation) if we
		 * have nothing to generate */
		security = mono_declsec_get_demands (method, &actions);
	}

	/* we must Demand SecurityPermission.Unmanaged before P/Invoking */
	pinvoke = (secman && (method->wrapper_type == MONO_WRAPPER_MANAGED_TO_NATIVE));
	if (pinvoke) {
		MonoMethod *wrapped = mono_marshal_method_from_wrapper (method);
		if (wrapped && (wrapped->flags & METHOD_ATTRIBUTE_PINVOKE_IMPL)) {
			MonoCustomAttrInfo* custom = mono_custom_attrs_from_method (wrapped);

			/* unless the method or it's class has the [SuppressUnmanagedCodeSecurity] attribute */
			if (custom && mono_custom_attrs_has_attr (custom, secman->suppressunmanagedcodesecurity)) {
				pinvoke = FALSE;
			}
			if (custom)
				mono_custom_attrs_free (custom);

			if (pinvoke) {
				custom = mono_custom_attrs_from_class (wrapped->klass);
				if (custom && mono_custom_attrs_has_attr (custom, secman->suppressunmanagedcodesecurity)) {
					pinvoke = FALSE;
				}
				if (custom)
					mono_custom_attrs_free (custom);
			}
		} else {
			/* not a P/Invoke after all */
			pinvoke = FALSE;
		}
	}
	
	if ((init_locals || (cfg->method == method && (cfg->opt & MONO_OPT_SHARED))) || cfg->compile_aot || security || pinvoke) {
		/* we use a separate basic block for the initialization code */
		NEW_BBLOCK (cfg, init_localsbb);
		cfg->bb_init = init_localsbb;
		init_localsbb->real_offset = cfg->real_offset;
		start_bblock->next_bb = init_localsbb;
		init_localsbb->next_bb = bblock;
		link_bblock (cfg, start_bblock, init_localsbb);
		link_bblock (cfg, init_localsbb, bblock);
		
		cfg->cbb = init_localsbb;
	} else {
		start_bblock->next_bb = bblock;
		link_bblock (cfg, start_bblock, bblock);
	}

	/* at this point we know, if security is TRUE, that some code needs to be generated */
	if (security && (cfg->method == method)) {
		MonoInst *args [2];

		mono_jit_stats.cas_demand_generation++;

		if (actions.demand.blob) {
			/* Add code for SecurityAction.Demand */
			EMIT_NEW_DECLSECCONST (cfg, args[0], image, actions.demand);
			EMIT_NEW_ICONST (cfg, args [1], actions.demand.size);
			/* Calls static void SecurityManager.InternalDemand (byte* permissions, int size); */
			mono_emit_method_call (cfg, secman->demand, args, NULL);
		}
		if (actions.noncasdemand.blob) {
			/* CLR 1.x uses a .noncasdemand (but 2.x doesn't) */
			/* For Mono we re-route non-CAS Demand to Demand (as the managed code must deal with it anyway) */
			EMIT_NEW_DECLSECCONST (cfg, args[0], image, actions.noncasdemand);
			EMIT_NEW_ICONST (cfg, args [1], actions.noncasdemand.size);
			/* Calls static void SecurityManager.InternalDemand (byte* permissions, int size); */
			mono_emit_method_call (cfg, secman->demand, args, NULL);
		}
		if (actions.demandchoice.blob) {
			/* New in 2.0, Demand must succeed for one of the permissions (i.e. not all) */
			EMIT_NEW_DECLSECCONST (cfg, args[0], image, actions.demandchoice);
			EMIT_NEW_ICONST (cfg, args [1], actions.demandchoice.size);
			/* Calls static void SecurityManager.InternalDemandChoice (byte* permissions, int size); */
			mono_emit_method_call (cfg, secman->demandchoice, args, NULL);
		}
	}

	/* we must Demand SecurityPermission.Unmanaged before p/invoking */
	if (pinvoke) {
		mono_emit_method_call (cfg, secman->demandunmanaged, NULL, NULL);
	}

	if (mono_security_get_mode () == MONO_SECURITY_MODE_CORE_CLR) {
		if (method->wrapper_type == MONO_WRAPPER_MANAGED_TO_NATIVE) {
			MonoMethod *wrapped = mono_marshal_method_from_wrapper (method);
			if (wrapped && (wrapped->flags & METHOD_ATTRIBUTE_PINVOKE_IMPL)) {
				if (!(method->klass && method->klass->image &&
						mono_security_core_clr_is_platform_image (method->klass->image))) {
					emit_throw_method_access_exception (cfg, method, wrapped, bblock, ip);
				}
			}
		}
		if (!method_is_safe (method))
			emit_throw_verification_exception (cfg, bblock, ip);
	}

	if (header->code_size == 0)
		UNVERIFIED;

	if (get_basic_blocks (cfg, header, cfg->real_offset, ip, end, &err_pos)) {
		ip = err_pos;
		UNVERIFIED;
	}

	if (cfg->method == method)
		mono_debug_init_method (cfg, bblock, breakpoint_id);

	for (n = 0; n < header->num_locals; ++n) {
		if (header->locals [n]->type == MONO_TYPE_VOID && !header->locals [n]->byref)
			UNVERIFIED;
	}
	class_inits = NULL;

	/* We force the vtable variable here for all shared methods
	   for the possibility that they might show up in a stack
	   trace where their exact instantiation is needed. */
	if (cfg->generic_sharing_context && method == cfg->method) {
		if ((method->flags & METHOD_ATTRIBUTE_STATIC) ||
				mini_method_get_context (method)->method_inst ||
				method->klass->valuetype) {
			mono_get_vtable_var (cfg);
		} else {
			/* FIXME: Is there a better way to do this?
			   We need the variable live for the duration
			   of the whole method. */
			cfg->args [0]->flags |= MONO_INST_INDIRECT;
		}
	}

	/* add a check for this != NULL to inlined methods */
	if (is_virtual_call) {
		MonoInst *arg_ins;

		NEW_ARGLOAD (cfg, arg_ins, 0);
		MONO_ADD_INS (cfg->cbb, arg_ins);
		cfg->flags |= MONO_CFG_HAS_CHECK_THIS;
		MONO_EMIT_NEW_UNALU (cfg, OP_CHECK_THIS, -1, arg_ins->dreg);
		MONO_EMIT_NEW_UNALU (cfg, OP_NOT_NULL, -1, arg_ins->dreg);
	}

	/* we use a spare stack slot in SWITCH and NEWOBJ and others */
	stack_start = sp = mono_mempool_alloc0 (cfg->mempool, sizeof (MonoInst*) * (header->max_stack + 1));

	ins_flag = 0;
	start_new_bblock = 0;
	cfg->cbb = bblock;
	while (ip < end) {

		if (cfg->method == method)
			cfg->real_offset = ip - header->code;
		else
			cfg->real_offset = inline_offset;
		cfg->ip = ip;

		context_used = 0;
		
		if (start_new_bblock) {
			bblock->cil_length = ip - bblock->cil_code;
			if (start_new_bblock == 2) {
				g_assert (ip == tblock->cil_code);
			} else {
				GET_BBLOCK (cfg, tblock, ip);
			}
			bblock->next_bb = tblock;
			bblock = tblock;
			cfg->cbb = bblock;
			start_new_bblock = 0;
			for (i = 0; i < bblock->in_scount; ++i) {
				if (cfg->verbose_level > 3)
					printf ("loading %d from temp %d\n", i, (int)bblock->in_stack [i]->inst_c0);						
				EMIT_NEW_TEMPLOAD (cfg, ins, bblock->in_stack [i]->inst_c0);
				*sp++ = ins;
			}
			if (class_inits)
				g_slist_free (class_inits);
			class_inits = NULL;
		} else {
			if ((tblock = cfg->cil_offset_to_bb [ip - cfg->cil_start]) && (tblock != bblock)) {
				link_bblock (cfg, bblock, tblock);
				if (sp != stack_start) {
					handle_stack_args (cfg, stack_start, sp - stack_start);
					sp = stack_start;
					CHECK_UNVERIFIABLE (cfg);
				}
				bblock->next_bb = tblock;
				bblock = tblock;
				cfg->cbb = bblock;
				for (i = 0; i < bblock->in_scount; ++i) {
					if (cfg->verbose_level > 3)
						printf ("loading %d from temp %d\n", i, (int)bblock->in_stack [i]->inst_c0);						
					EMIT_NEW_TEMPLOAD (cfg, ins, bblock->in_stack [i]->inst_c0);
					*sp++ = ins;
				}
				g_slist_free (class_inits);
				class_inits = NULL;
			}
		}

		bblock->real_offset = cfg->real_offset;

		if ((cfg->method == method) && cfg->coverage_info) {
			guint32 cil_offset = ip - header->code;
			cfg->coverage_info->data [cil_offset].cil_code = ip;

			/* TODO: Use an increment here */
#if defined(TARGET_X86)
			MONO_INST_NEW (cfg, ins, OP_STORE_MEM_IMM);
			ins->inst_p0 = &(cfg->coverage_info->data [cil_offset].count);
			ins->inst_imm = 1;
			MONO_ADD_INS (cfg->cbb, ins);
#else
			EMIT_NEW_PCONST (cfg, ins, &(cfg->coverage_info->data [cil_offset].count));
			MONO_EMIT_NEW_STORE_MEMBASE_IMM (cfg, OP_STORE_MEMBASE_IMM, ins->dreg, 0, 1);
#endif
		}

		if (cfg->verbose_level > 3)
			printf ("converting (in B%d: stack: %d) %s", bblock->block_num, (int)(sp - stack_start), mono_disasm_code_one (NULL, method, ip, NULL));

		switch (*ip) {
		case CEE_NOP:
			if (cfg->keep_cil_nops)
				MONO_INST_NEW (cfg, ins, OP_HARD_NOP);
			else
				MONO_INST_NEW (cfg, ins, OP_NOP);
			ip++;
			MONO_ADD_INS (bblock, ins);
			break;
		case CEE_BREAK:
			MONO_INST_NEW (cfg, ins, OP_BREAK);
			ip++;
			MONO_ADD_INS (bblock, ins);
			break;
		case CEE_LDARG_0:
		case CEE_LDARG_1:
		case CEE_LDARG_2:
		case CEE_LDARG_3:
			CHECK_STACK_OVF (1);
			n = (*ip)-CEE_LDARG_0;
			CHECK_ARG (n);
			EMIT_NEW_ARGLOAD (cfg, ins, n);
			ip++;
			*sp++ = ins;
			break;
		case CEE_LDLOC_0:
		case CEE_LDLOC_1:
		case CEE_LDLOC_2:
		case CEE_LDLOC_3:
			CHECK_STACK_OVF (1);
			n = (*ip)-CEE_LDLOC_0;
			CHECK_LOCAL (n);
			EMIT_NEW_LOCLOAD (cfg, ins, n);
			ip++;
			*sp++ = ins;
			break;
		case CEE_STLOC_0:
		case CEE_STLOC_1:
		case CEE_STLOC_2:
		case CEE_STLOC_3: {
			CHECK_STACK (1);
			n = (*ip)-CEE_STLOC_0;
			CHECK_LOCAL (n);
			--sp;
			if (!dont_verify_stloc && target_type_is_incompatible (cfg, header->locals [n], *sp))
				UNVERIFIED;
			emit_stloc_ir (cfg, sp, header, n);
			++ip;
			inline_costs += 1;
			break;
			}
		case CEE_LDARG_S:
			CHECK_OPSIZE (2);
			CHECK_STACK_OVF (1);
			n = ip [1];
			CHECK_ARG (n);
			EMIT_NEW_ARGLOAD (cfg, ins, n);
			*sp++ = ins;
			ip += 2;
			break;
		case CEE_LDARGA_S:
			CHECK_OPSIZE (2);
			CHECK_STACK_OVF (1);
			n = ip [1];
			CHECK_ARG (n);
			NEW_ARGLOADA (cfg, ins, n);
			MONO_ADD_INS (cfg->cbb, ins);
			*sp++ = ins;
			ip += 2;
			break;
		case CEE_STARG_S:
			CHECK_OPSIZE (2);
			CHECK_STACK (1);
			--sp;
			n = ip [1];
			CHECK_ARG (n);
			if (!dont_verify_stloc && target_type_is_incompatible (cfg, param_types [ip [1]], *sp))
				UNVERIFIED;
			EMIT_NEW_ARGSTORE (cfg, ins, n, *sp);
			ip += 2;
			break;
		case CEE_LDLOC_S:
			CHECK_OPSIZE (2);
			CHECK_STACK_OVF (1);
			n = ip [1];
			CHECK_LOCAL (n);
			EMIT_NEW_LOCLOAD (cfg, ins, n);
			*sp++ = ins;
			ip += 2;
			break;
		case CEE_LDLOCA_S: {
			unsigned char *tmp_ip;
			CHECK_OPSIZE (2);
			CHECK_STACK_OVF (1);
			CHECK_LOCAL (ip [1]);

			if ((tmp_ip = emit_optimized_ldloca_ir (cfg, ip, end, 1))) {
				ip = tmp_ip;
				inline_costs += 1;
				break;
			}

			EMIT_NEW_LOCLOADA (cfg, ins, ip [1]);
			*sp++ = ins;
			ip += 2;
			break;
		}
		case CEE_STLOC_S:
			CHECK_OPSIZE (2);
			CHECK_STACK (1);
			--sp;
			CHECK_LOCAL (ip [1]);
			if (!dont_verify_stloc && target_type_is_incompatible (cfg, header->locals [ip [1]], *sp))
				UNVERIFIED;
			emit_stloc_ir (cfg, sp, header, ip [1]);
			ip += 2;
			inline_costs += 1;
			break;
		case CEE_LDNULL:
			CHECK_STACK_OVF (1);
			EMIT_NEW_PCONST (cfg, ins, NULL);
			ins->type = STACK_OBJ;
			++ip;
			*sp++ = ins;
			break;
		case CEE_LDC_I4_M1:
			CHECK_STACK_OVF (1);
			EMIT_NEW_ICONST (cfg, ins, -1);
			++ip;
			*sp++ = ins;
			break;
		case CEE_LDC_I4_0:
		case CEE_LDC_I4_1:
		case CEE_LDC_I4_2:
		case CEE_LDC_I4_3:
		case CEE_LDC_I4_4:
		case CEE_LDC_I4_5:
		case CEE_LDC_I4_6:
		case CEE_LDC_I4_7:
		case CEE_LDC_I4_8:
			CHECK_STACK_OVF (1);
			EMIT_NEW_ICONST (cfg, ins, (*ip) - CEE_LDC_I4_0);
			++ip;
			*sp++ = ins;
			break;
		case CEE_LDC_I4_S:
			CHECK_OPSIZE (2);
			CHECK_STACK_OVF (1);
			++ip;
			EMIT_NEW_ICONST (cfg, ins, *((signed char*)ip));
			++ip;
			*sp++ = ins;
			break;
		case CEE_LDC_I4:
			CHECK_OPSIZE (5);
			CHECK_STACK_OVF (1);
			EMIT_NEW_ICONST (cfg, ins, (gint32)read32 (ip + 1));
			ip += 5;
			*sp++ = ins;
			break;
		case CEE_LDC_I8:
			CHECK_OPSIZE (9);
			CHECK_STACK_OVF (1);
			MONO_INST_NEW (cfg, ins, OP_I8CONST);
			ins->type = STACK_I8;
			ins->dreg = alloc_dreg (cfg, STACK_I8);
			++ip;
			ins->inst_l = (gint64)read64 (ip);
			MONO_ADD_INS (bblock, ins);
			ip += 8;
			*sp++ = ins;
			break;
		case CEE_LDC_R4: {
			float *f;
			/* FIXME: we should really allocate this only late in the compilation process */
			mono_domain_lock (cfg->domain);
			f = mono_domain_alloc (cfg->domain, sizeof (float));
			mono_domain_unlock (cfg->domain);
			CHECK_OPSIZE (5);
			CHECK_STACK_OVF (1);
			MONO_INST_NEW (cfg, ins, OP_R4CONST);
			ins->type = STACK_R8;
			ins->dreg = alloc_dreg (cfg, STACK_R8);
			++ip;
			readr4 (ip, f);
			ins->inst_p0 = f;
			MONO_ADD_INS (bblock, ins);
			
			ip += 4;
			*sp++ = ins;			
			break;
		}
		case CEE_LDC_R8: {
			double *d;
			/* FIXME: we should really allocate this only late in the compilation process */
			mono_domain_lock (cfg->domain);
			d = mono_domain_alloc (cfg->domain, sizeof (double));
			mono_domain_unlock (cfg->domain);
			CHECK_OPSIZE (9);
			CHECK_STACK_OVF (1);
			MONO_INST_NEW (cfg, ins, OP_R8CONST);
			ins->type = STACK_R8;
			ins->dreg = alloc_dreg (cfg, STACK_R8);
			++ip;
			readr8 (ip, d);
			ins->inst_p0 = d;
			MONO_ADD_INS (bblock, ins);

			ip += 8;
			*sp++ = ins;			
			break;
		}
		case CEE_DUP: {
			MonoInst *temp, *store;
			CHECK_STACK (1);
			CHECK_STACK_OVF (1);
			sp--;
			ins = *sp;

			temp = mono_compile_create_var (cfg, type_from_stack_type (ins), OP_LOCAL);
			EMIT_NEW_TEMPSTORE (cfg, store, temp->inst_c0, ins);

			EMIT_NEW_TEMPLOAD (cfg, ins, temp->inst_c0);
			*sp++ = ins;

			EMIT_NEW_TEMPLOAD (cfg, ins, temp->inst_c0);
			*sp++ = ins;

			++ip;
			inline_costs += 2;
			break;
		}
		case CEE_POP:
			CHECK_STACK (1);
			ip++;
			--sp;

#ifdef TARGET_X86
			if (sp [0]->type == STACK_R8)
				/* we need to pop the value from the x86 FP stack */
				MONO_EMIT_NEW_UNALU (cfg, OP_X86_FPOP, -1, sp [0]->dreg);
#endif
			break;
		case CEE_JMP: {
			MonoCallInst *call;

			INLINE_FAILURE;

			CHECK_OPSIZE (5);
			if (stack_start != sp)
				UNVERIFIED;
			token = read32 (ip + 1);
			/* FIXME: check the signature matches */
			cmethod = mini_get_method (cfg, method, token, NULL, generic_context);

			if (!cmethod)
				goto load_error;
 
			if (cfg->generic_sharing_context && mono_method_check_context_used (cmethod))
				GENERIC_SHARING_FAILURE (CEE_JMP);

			if (mono_security_get_mode () == MONO_SECURITY_MODE_CAS)
				CHECK_CFG_EXCEPTION;

#ifdef TARGET_AMD64
			{
				MonoMethodSignature *fsig = mono_method_signature (cmethod);
				int i, n;

				/* Handle tail calls similarly to calls */
				n = fsig->param_count + fsig->hasthis;

				MONO_INST_NEW_CALL (cfg, call, OP_TAILCALL);
				call->method = cmethod;
				call->tail_call = TRUE;
				call->signature = mono_method_signature (cmethod);
				call->args = mono_mempool_alloc (cfg->mempool, sizeof (MonoInst*) * n);
				call->inst.inst_p0 = cmethod;
				for (i = 0; i < n; ++i)
					EMIT_NEW_ARGLOAD (cfg, call->args [i], i);

				mono_arch_emit_call (cfg, call);
				MONO_ADD_INS (bblock, (MonoInst*)call);
			}
#else
			for (i = 0; i < num_args; ++i)
				/* Prevent arguments from being optimized away */
				arg_array [i]->flags |= MONO_INST_VOLATILE;

			MONO_INST_NEW_CALL (cfg, call, OP_JMP);
			ins = (MonoInst*)call;
			ins->inst_p0 = cmethod;
			MONO_ADD_INS (bblock, ins);
#endif

			ip += 5;
			start_new_bblock = 1;
			break;
		}
		case CEE_CALLI:
		case CEE_CALL:
		case CEE_CALLVIRT: {
			MonoInst *addr = NULL;
			MonoMethodSignature *fsig = NULL;
			int array_rank = 0;
			int virtual = *ip == CEE_CALLVIRT;
			int calli = *ip == CEE_CALLI;
			gboolean pass_imt_from_rgctx = FALSE;
			MonoInst *imt_arg = NULL;
			gboolean pass_vtable = FALSE;
			gboolean pass_mrgctx = FALSE;
			MonoInst *vtable_arg = NULL;
			gboolean check_this = FALSE;

			CHECK_OPSIZE (5);
			token = read32 (ip + 1);

			if (calli) {
				cmethod = NULL;
				CHECK_STACK (1);
				--sp;
				addr = *sp;
				if (method->wrapper_type != MONO_WRAPPER_NONE)
					fsig = (MonoMethodSignature *)mono_method_get_wrapper_data (method, token);
				else
					fsig = mono_metadata_parse_signature (image, token);

				n = fsig->param_count + fsig->hasthis;
			} else {
				MonoMethod *cil_method;
				
				if (method->wrapper_type != MONO_WRAPPER_NONE) {
					cmethod =  (MonoMethod *)mono_method_get_wrapper_data (method, token);
					cil_method = cmethod;
				} else if (constrained_call) {
					if ((constrained_call->byval_arg.type == MONO_TYPE_VAR || constrained_call->byval_arg.type == MONO_TYPE_MVAR) && cfg->generic_sharing_context) {
						/* 
						 * This is needed since get_method_constrained can't find 
						 * the method in klass representing a type var.
						 * The type var is guaranteed to be a reference type in this
						 * case.
						 */
						cmethod = mini_get_method (cfg, method, token, NULL, generic_context);
						cil_method = cmethod;
						g_assert (!cmethod->klass->valuetype);
					} else {
						cmethod = mono_get_method_constrained (image, token, constrained_call, generic_context, &cil_method);
					}
				} else {
					cmethod = mini_get_method (cfg, method, token, NULL, generic_context);
					cil_method = cmethod;
				}

				if (!cmethod)
					goto load_error;
				if (!dont_verify && !cfg->skip_visibility) {
					MonoMethod *target_method = cil_method;
					if (method->is_inflated) {
						target_method = mini_get_method_allow_open (method, token, NULL, &(mono_method_get_generic_container (method_definition)->context));
					}
					if (!mono_method_can_access_method (method_definition, target_method) &&
						!mono_method_can_access_method (method, cil_method))
						METHOD_ACCESS_FAILURE;
				}

				if (mono_security_get_mode () == MONO_SECURITY_MODE_CORE_CLR)
					ensure_method_is_allowed_to_call_method (cfg, method, cil_method, bblock, ip);

				if (!virtual && (cmethod->flags & METHOD_ATTRIBUTE_ABSTRACT))
					/* MS.NET seems to silently convert this to a callvirt */
					virtual = 1;

				if (!cmethod->klass->inited)
					if (!mono_class_init (cmethod->klass))
						goto load_error;

				if (cmethod->iflags & METHOD_IMPL_ATTRIBUTE_INTERNAL_CALL &&
				    mini_class_is_system_array (cmethod->klass)) {
					array_rank = cmethod->klass->rank;
					fsig = mono_method_signature (cmethod);
				} else {
					if (mono_method_signature (cmethod)->pinvoke) {
						MonoMethod *wrapper = mono_marshal_get_native_wrapper (cmethod,
							check_for_pending_exc, FALSE);
						fsig = mono_method_signature (wrapper);
					} else if (constrained_call) {
						fsig = mono_method_signature (cmethod);
					} else {
						fsig = mono_method_get_signature_full (cmethod, image, token, generic_context);
					}
				}

				mono_save_token_info (cfg, image, token, cil_method);

				n = fsig->param_count + fsig->hasthis;

				if (mono_security_get_mode () == MONO_SECURITY_MODE_CAS) {
					if (check_linkdemand (cfg, method, cmethod))
						INLINE_FAILURE;
					CHECK_CFG_EXCEPTION;
				}

				if (cmethod->string_ctor && method->wrapper_type != MONO_WRAPPER_RUNTIME_INVOKE)
					g_assert_not_reached ();
			}

			if (!cfg->generic_sharing_context && cmethod && cmethod->klass->generic_container)
				UNVERIFIED;

			if (!cfg->generic_sharing_context && cmethod)
				g_assert (!mono_method_check_context_used (cmethod));

			CHECK_STACK (n);

			//g_assert (!virtual || fsig->hasthis);

			sp -= n;

			if (constrained_call) {
				/*
				 * We have the `constrained.' prefix opcode.
				 */
				if (constrained_call->valuetype && !cmethod->klass->valuetype) {
					/*
					 * The type parameter is instantiated as a valuetype,
					 * but that type doesn't override the method we're
					 * calling, so we need to box `this'.
					 */
					EMIT_NEW_LOAD_MEMBASE_TYPE (cfg, ins, &constrained_call->byval_arg, sp [0]->dreg, 0);
					ins->klass = constrained_call;
					sp [0] = handle_box (cfg, ins, constrained_call);
				} else if (!constrained_call->valuetype) {
					int dreg = alloc_preg (cfg);

					/*
					 * The type parameter is instantiated as a reference
					 * type.  We have a managed pointer on the stack, so
					 * we need to dereference it here.
					 */
					EMIT_NEW_LOAD_MEMBASE (cfg, ins, OP_LOAD_MEMBASE, dreg, sp [0]->dreg, 0);
					ins->type = STACK_OBJ;
					sp [0] = ins;
				} else if (cmethod->klass->valuetype)
					virtual = 0;
				constrained_call = NULL;
			}

			if (*ip != CEE_CALLI && check_call_signature (cfg, fsig, sp))
				UNVERIFIED;

			/* 
			 * If the callee is a shared method, then its static cctor
			 * might not get called after the call was patched.
			 */
			if (cfg->generic_sharing_context && cmethod && cmethod->klass != method->klass && cmethod->klass->generic_class && mono_method_is_generic_sharable_impl (cmethod, TRUE) && mono_class_needs_cctor_run (cmethod->klass, method)) {
				emit_generic_class_init (cfg, cmethod->klass);
			}

			if (cmethod && ((cmethod->flags & METHOD_ATTRIBUTE_STATIC) || cmethod->klass->valuetype) &&
					(cmethod->klass->generic_class || cmethod->klass->generic_container)) {
				gboolean sharing_enabled = mono_class_generic_sharing_enabled (cmethod->klass);
				MonoGenericContext *context = mini_class_get_context (cmethod->klass);
				gboolean context_sharable = mono_generic_context_is_sharable (context, TRUE);

				/*
				 * Pass vtable iff target method might
				 * be shared, which means that sharing
				 * is enabled for its class and its
				 * context is sharable (and it's not a
				 * generic method).
				 */
				if (sharing_enabled && context_sharable &&
					!(mini_method_get_context (cmethod) && mini_method_get_context (cmethod)->method_inst))
					pass_vtable = TRUE;
			}

			if (cmethod && mini_method_get_context (cmethod) &&
					mini_method_get_context (cmethod)->method_inst) {
				gboolean sharing_enabled = mono_class_generic_sharing_enabled (cmethod->klass);
				MonoGenericContext *context = mini_method_get_context (cmethod);
				gboolean context_sharable = mono_generic_context_is_sharable (context, TRUE);

				g_assert (!pass_vtable);

				if (sharing_enabled && context_sharable)
					pass_mrgctx = TRUE;
			}

			if (cfg->generic_sharing_context && cmethod) {
				MonoGenericContext *cmethod_context = mono_method_get_context (cmethod);

				context_used = mono_method_check_context_used (cmethod);

				if (context_used && (cmethod->klass->flags & TYPE_ATTRIBUTE_INTERFACE)) {
					/* Generic method interface
					   calls are resolved via a
					   helper function and don't
					   need an imt. */
					if (!cmethod_context || !cmethod_context->method_inst)
						pass_imt_from_rgctx = TRUE;
				}

				/*
				 * If a shared method calls another
				 * shared method then the caller must
				 * have a generic sharing context
				 * because the magic trampoline
				 * requires it.  FIXME: We shouldn't
				 * have to force the vtable/mrgctx
				 * variable here.  Instead there
				 * should be a flag in the cfg to
				 * request a generic sharing context.
				 */
				if (context_used &&
						((method->flags & METHOD_ATTRIBUTE_STATIC) || method->klass->valuetype))
					mono_get_vtable_var (cfg);
			}

			if (pass_vtable) {
				if (context_used) {
					vtable_arg = emit_get_rgctx_klass (cfg, context_used, cmethod->klass, MONO_RGCTX_INFO_VTABLE);
				} else {
					MonoVTable *vtable = mono_class_vtable (cfg->domain, cmethod->klass);

					CHECK_TYPELOAD (cmethod->klass);
					EMIT_NEW_VTABLECONST (cfg, vtable_arg, vtable);
				}
			}

			if (pass_mrgctx) {
				g_assert (!vtable_arg);

				if (context_used) {
					vtable_arg = emit_get_rgctx_method (cfg, context_used, cmethod, MONO_RGCTX_INFO_METHOD_RGCTX);
				} else {
					EMIT_NEW_METHOD_RGCTX_CONST (cfg, vtable_arg, cmethod);
				}

				if (!(cmethod->flags & METHOD_ATTRIBUTE_VIRTUAL) ||
						MONO_METHOD_IS_FINAL (cmethod)) {
					if (virtual)
						check_this = TRUE;
					virtual = 0;
				}
			}

			if (pass_imt_from_rgctx) {
				g_assert (!pass_vtable);
				g_assert (cmethod);

				imt_arg = emit_get_rgctx_method (cfg, context_used,
					cmethod, MONO_RGCTX_INFO_METHOD);
			}

			if (check_this) {
				MonoInst *check;

				MONO_INST_NEW (cfg, check, OP_CHECK_THIS);
				check->sreg1 = sp [0]->dreg;
				MONO_ADD_INS (cfg->cbb, check);
			}

			/* Calling virtual generic methods */
			if (cmethod && virtual && 
			    (cmethod->flags & METHOD_ATTRIBUTE_VIRTUAL) && 
		 	    !(MONO_METHOD_IS_FINAL (cmethod) && 
			      cmethod->wrapper_type != MONO_WRAPPER_REMOTING_INVOKE_WITH_CHECK) &&
			    mono_method_signature (cmethod)->generic_param_count) {
				MonoInst *this_temp, *this_arg_temp, *store;
				MonoInst *iargs [4];

				g_assert (mono_method_signature (cmethod)->is_inflated);

				/* Prevent inlining of methods that contain indirect calls */
				INLINE_FAILURE;

#if MONO_ARCH_HAVE_GENERALIZED_IMT_THUNK
				if (!(cmethod->klass->flags & TYPE_ATTRIBUTE_INTERFACE) &&
						cmethod->wrapper_type == MONO_WRAPPER_NONE) {
					g_assert (!imt_arg);
					if (context_used) {
						imt_arg = emit_get_rgctx_method (cfg, context_used,
							cmethod, MONO_RGCTX_INFO_METHOD_CONTEXT);

					} else {
						// FIXME:
						cfg->disable_aot = TRUE;
						g_assert (cmethod->is_inflated);
						EMIT_NEW_PCONST (cfg, imt_arg,
							((MonoMethodInflated*)cmethod)->context.method_inst);
					}
					ins = mono_emit_method_call_full (cfg, cmethod, fsig, sp, sp [0], imt_arg);
					if (!ins)
						GENERIC_SHARING_FAILURE (*ip);
				} else
#endif
				{
					this_temp = mono_compile_create_var (cfg, type_from_stack_type (sp [0]), OP_LOCAL);
					NEW_TEMPSTORE (cfg, store, this_temp->inst_c0, sp [0]);
					MONO_ADD_INS (bblock, store);

					/* FIXME: This should be a managed pointer */
					this_arg_temp = mono_compile_create_var (cfg, &mono_defaults.int_class->byval_arg, OP_LOCAL);

					EMIT_NEW_TEMPLOAD (cfg, iargs [0], this_temp->inst_c0);
					if (context_used) {
						iargs [1] = emit_get_rgctx_method (cfg, context_used,
							cmethod, MONO_RGCTX_INFO_METHOD);
						EMIT_NEW_TEMPLOADA (cfg, iargs [2], this_arg_temp->inst_c0);
						addr = mono_emit_jit_icall (cfg,
								mono_helper_compile_generic_method, iargs);
					} else {
						EMIT_NEW_METHODCONST (cfg, iargs [1], cmethod);
						EMIT_NEW_TEMPLOADA (cfg, iargs [2], this_arg_temp->inst_c0);
						addr = mono_emit_jit_icall (cfg, mono_helper_compile_generic_method, iargs);
					}

					EMIT_NEW_TEMPLOAD (cfg, sp [0], this_arg_temp->inst_c0);

					ins = (MonoInst*)mono_emit_calli (cfg, fsig, sp, addr);
				}

				if (!MONO_TYPE_IS_VOID (fsig->ret))
					*sp++ = ins;

				ip += 5;
				ins_flag = 0;
				break;
			}

			/* Tail prefix */
			/* FIXME: runtime generic context pointer for jumps? */
			/* FIXME: handle this for generic sharing eventually */
			if ((ins_flag & MONO_INST_TAILCALL) && !cfg->generic_sharing_context && !vtable_arg && cmethod && (*ip == CEE_CALL) &&
				(mono_metadata_signature_equal (mono_method_signature (method), mono_method_signature (cmethod))) && !MONO_TYPE_ISSTRUCT (mono_method_signature (cmethod)->ret)) {
				MonoCallInst *call;

				/* Prevent inlining of methods with tail calls (the call stack would be altered) */
				INLINE_FAILURE;

				MONO_INST_NEW_CALL (cfg, call, OP_JMP);
				call->tail_call = TRUE;
				call->method = cmethod;
				call->signature = mono_method_signature (cmethod);

#ifdef TARGET_AMD64
				/* Handle tail calls similarly to calls */
				call->inst.opcode = OP_TAILCALL;
				call->args = sp;
				mono_arch_emit_call (cfg, call);
#else
				/*
				 * We implement tail calls by storing the actual arguments into the 
				 * argument variables, then emitting a CEE_JMP.
				 */
				for (i = 0; i < n; ++i) {
					/* Prevent argument from being register allocated */
					arg_array [i]->flags |= MONO_INST_VOLATILE;
					EMIT_NEW_ARGSTORE (cfg, ins, i, sp [i]);
				}
#endif

				ins = (MonoInst*)call;
				ins->inst_p0 = cmethod;
				ins->inst_p1 = arg_array [0];
				MONO_ADD_INS (bblock, ins);
				link_bblock (cfg, bblock, end_bblock);			
				start_new_bblock = 1;
				/* skip CEE_RET as well */
				ip += 6;
				ins_flag = 0;
				break;
			}

			/* Conversion to a JIT intrinsic */
			if (cmethod && (cfg->opt & MONO_OPT_INTRINS) && (ins = mini_emit_inst_for_method (cfg, cmethod, fsig, sp))) {
				if (!MONO_TYPE_IS_VOID (fsig->ret)) {
					type_to_eval_stack_type ((cfg), fsig->ret, ins);
					*sp = ins;
					sp++;
				}

				ip += 5;
				ins_flag = 0;
				break;
			}

			/* Inlining */
			if ((cfg->opt & MONO_OPT_INLINE) && cmethod &&
				(!virtual || !(cmethod->flags & METHOD_ATTRIBUTE_VIRTUAL) || MONO_METHOD_IS_FINAL (cmethod)) &&
			    mono_method_check_inlining (cfg, cmethod) &&
				 !g_list_find (dont_inline, cmethod)) {
				int costs;
				gboolean allways = FALSE;

				if ((cmethod->iflags & METHOD_IMPL_ATTRIBUTE_INTERNAL_CALL) ||
					(cmethod->flags & METHOD_ATTRIBUTE_PINVOKE_IMPL)) {
					/* Prevent inlining of methods that call wrappers */
					INLINE_FAILURE;
					cmethod = mono_marshal_get_native_wrapper (cmethod, check_for_pending_exc, FALSE);
					allways = TRUE;
				}

 				if ((costs = inline_method (cfg, cmethod, fsig, sp, ip, cfg->real_offset, dont_inline, allways))) {
					ip += 5;
					cfg->real_offset += 5;
					bblock = cfg->cbb;

 					if (!MONO_TYPE_IS_VOID (fsig->ret))
						/* *sp is already set by inline_method */
 						sp++;

					inline_costs += costs;
					ins_flag = 0;
					break;
				}
			}
			
			inline_costs += 10 * num_calls++;

			/* Tail recursion elimination */
			if ((cfg->opt & MONO_OPT_TAILC) && *ip == CEE_CALL && cmethod == method && ip [5] == CEE_RET && !vtable_arg) {
				gboolean has_vtargs = FALSE;
				int i;

				/* Prevent inlining of methods with tail calls (the call stack would be altered) */
				INLINE_FAILURE;

				/* keep it simple */
				for (i =  fsig->param_count - 1; i >= 0; i--) {
					if (MONO_TYPE_ISSTRUCT (mono_method_signature (cmethod)->params [i])) 
						has_vtargs = TRUE;
				}

				if (!has_vtargs) {
					for (i = 0; i < n; ++i)
						EMIT_NEW_ARGSTORE (cfg, ins, i, sp [i]);
					MONO_INST_NEW (cfg, ins, OP_BR);
					MONO_ADD_INS (bblock, ins);
					tblock = start_bblock->out_bb [0];
					link_bblock (cfg, bblock, tblock);
					ins->inst_target_bb = tblock;
					start_new_bblock = 1;

					/* skip the CEE_RET, too */
					if (ip_in_bb (cfg, bblock, ip + 5))
						ip += 6;
					else
						ip += 5;

					ins_flag = 0;
					break;
				}
			}

			/* Generic sharing */
			/* FIXME: only do this for generic methods if
			   they are not shared! */
			if (context_used && !imt_arg && !array_rank &&
					(!mono_method_is_generic_sharable_impl (cmethod, TRUE) ||
						!mono_class_generic_sharing_enabled (cmethod->klass)) &&
					(!virtual || MONO_METHOD_IS_FINAL (cmethod) ||
						!(cmethod->flags & METHOD_ATTRIBUTE_VIRTUAL))) {
				INLINE_FAILURE;

				g_assert (cfg->generic_sharing_context && cmethod);
				g_assert (!addr);

				/*
				 * We are compiling a call to a
				 * generic method from shared code,
				 * which means that we have to look up
				 * the method in the rgctx and do an
				 * indirect call.
				 */
				addr = emit_get_rgctx_method (cfg, context_used, cmethod, MONO_RGCTX_INFO_GENERIC_METHOD_CODE);
			}

			/* Indirect calls */
			if (addr) {
				g_assert (!imt_arg);

				if (*ip == CEE_CALL)
					g_assert (context_used);
				else if (*ip == CEE_CALLI)
					g_assert (!vtable_arg);
				else
					/* FIXME: what the hell is this??? */
					g_assert (cmethod->flags & METHOD_ATTRIBUTE_FINAL ||
							!(cmethod->flags & METHOD_ATTRIBUTE_FINAL));

				/* Prevent inlining of methods with indirect calls */
				INLINE_FAILURE;

				if (vtable_arg) {
#ifdef MONO_ARCH_RGCTX_REG
					MonoCallInst *call;
					int rgctx_reg = mono_alloc_preg (cfg);

					MONO_EMIT_NEW_UNALU (cfg, OP_MOVE, rgctx_reg, vtable_arg->dreg);
					ins = (MonoInst*)mono_emit_calli (cfg, fsig, sp, addr);
					call = (MonoCallInst*)ins;
					mono_call_inst_add_outarg_reg (cfg, call, rgctx_reg, MONO_ARCH_RGCTX_REG, FALSE);
					cfg->uses_rgctx_reg = TRUE;
#else
					NOT_IMPLEMENTED;
#endif
				} else {
					if (addr->opcode == OP_AOTCONST && addr->inst_c1 == MONO_PATCH_INFO_ICALL_ADDR) {
						/* 
						 * Instead of emitting an indirect call, emit a direct call
						 * with the contents of the aotconst as the patch info.
						 */
						ins = (MonoInst*)mono_emit_abs_call (cfg, MONO_PATCH_INFO_ICALL_ADDR, addr->inst_p0, fsig, sp);
						NULLIFY_INS (addr);
					} else {
						ins = (MonoInst*)mono_emit_calli (cfg, fsig, sp, addr);
					}
				}
				if (!MONO_TYPE_IS_VOID (fsig->ret)) {
					if (fsig->pinvoke && !fsig->ret->byref) {
						int widen_op = -1;

						/* 
						 * Native code might return non register sized integers 
						 * without initializing the upper bits.
						 */
						switch (mono_type_to_load_membase (cfg, fsig->ret)) {
						case OP_LOADI1_MEMBASE:
							widen_op = OP_ICONV_TO_I1;
							break;
						case OP_LOADU1_MEMBASE:
							widen_op = OP_ICONV_TO_U1;
							break;
						case OP_LOADI2_MEMBASE:
							widen_op = OP_ICONV_TO_I2;
							break;
						case OP_LOADU2_MEMBASE:
							widen_op = OP_ICONV_TO_U2;
							break;
						default:
							break;
						}

						if (widen_op != -1) {
							int dreg = alloc_preg (cfg);
							MonoInst *widen;

							EMIT_NEW_UNALU (cfg, widen, widen_op, dreg, ins->dreg);
							widen->type = ins->type;
							ins = widen;
						}
					}

					*sp++ = ins;
				}

				ip += 5;
				ins_flag = 0;
				break;
			}
	      				
			/* Array methods */
			if (array_rank) {
				MonoInst *addr;

				if (strcmp (cmethod->name, "Set") == 0) { /* array Set */ 
					if (sp [fsig->param_count]->type == STACK_OBJ) {
						MonoInst *iargs [2];

						iargs [0] = sp [0];
						iargs [1] = sp [fsig->param_count];
						
						mono_emit_jit_icall (cfg, mono_helper_stelem_ref_check, iargs);
					}
					
					addr = mini_emit_ldelema_ins (cfg, cmethod, sp, ip, TRUE);
					EMIT_NEW_STORE_MEMBASE_TYPE (cfg, ins, fsig->params [fsig->param_count - 1], addr->dreg, 0, sp [fsig->param_count]->dreg);
				} else if (strcmp (cmethod->name, "Get") == 0) { /* array Get */
					addr = mini_emit_ldelema_ins (cfg, cmethod, sp, ip, FALSE);

					EMIT_NEW_LOAD_MEMBASE_TYPE (cfg, ins, fsig->ret, addr->dreg, 0);

					*sp++ = ins;
				} else if (strcmp (cmethod->name, "Address") == 0) { /* array Address */
					if (!cmethod->klass->element_class->valuetype && !readonly)
						mini_emit_check_array_type (cfg, sp [0], cmethod->klass);
					
					readonly = FALSE;
					addr = mini_emit_ldelema_ins (cfg, cmethod, sp, ip, FALSE);
					*sp++ = addr;
				} else {
					g_assert_not_reached ();
				}

				ip += 5;
				ins_flag = 0;
				break;
			}

			ins = mini_redirect_call (cfg, cmethod, fsig, sp, virtual ? sp [0] : NULL);
			if (ins) {
				if (!MONO_TYPE_IS_VOID (fsig->ret))
					*sp++ = ins;

				ip += 5;
				ins_flag = 0;
				break;
			}

			/* Common call */
			INLINE_FAILURE;
			if (vtable_arg) {
				ins = mono_emit_rgctx_method_call_full (cfg, cmethod, fsig, sp, virtual ? sp [0] : NULL,
					NULL, vtable_arg);
			} else if (imt_arg) {
				ins = (MonoInst*)mono_emit_method_call_full (cfg, cmethod, fsig, sp, virtual ? sp [0] : NULL, imt_arg);
			} else {
				ins = (MonoInst*)mono_emit_method_call_full (cfg, cmethod, fsig, sp, virtual ? sp [0] : NULL, NULL);
			}
			if (!ins)
				GENERIC_SHARING_FAILURE (*ip);

			if (!MONO_TYPE_IS_VOID (fsig->ret))
				*sp++ = ins;

			ip += 5;
			ins_flag = 0;
			break;
		}
		case CEE_RET:
			if (cfg->method != method) {
				/* return from inlined method */
				/* 
				 * If in_count == 0, that means the ret is unreachable due to
				 * being preceeded by a throw. In that case, inline_method () will
				 * handle setting the return value 
				 * (test case: test_0_inline_throw ()).
				 */
				if (return_var && cfg->cbb->in_count) {
					MonoInst *store;
					CHECK_STACK (1);
					--sp;
					//g_assert (returnvar != -1);
					EMIT_NEW_TEMPSTORE (cfg, store, return_var->inst_c0, *sp);
					cfg->ret_var_set = TRUE;
				} 
			} else {
				if (cfg->ret) {
					MonoType *ret_type = mono_method_signature (method)->ret;

					g_assert (!return_var);
					CHECK_STACK (1);
					--sp;
					if (mini_type_to_stind (cfg, ret_type) == CEE_STOBJ) {
						MonoInst *ret_addr;

						if (!cfg->vret_addr) {
							MonoInst *ins;

							EMIT_NEW_VARSTORE (cfg, ins, cfg->ret, ret_type, (*sp));
						} else {
							EMIT_NEW_RETLOADA (cfg, ret_addr);

							EMIT_NEW_STORE_MEMBASE (cfg, ins, OP_STOREV_MEMBASE, ret_addr->dreg, 0, (*sp)->dreg);
							ins->klass = mono_class_from_mono_type (ret_type);
						}
					} else {
#ifdef MONO_ARCH_SOFT_FLOAT
						if (!ret_type->byref && ret_type->type == MONO_TYPE_R4) {
							MonoInst *iargs [1];
							MonoInst *conv;

							iargs [0] = *sp;
							conv = mono_emit_jit_icall (cfg, mono_fload_r4_arg, iargs);
							mono_arch_emit_setret (cfg, method, conv);
						} else {
							mono_arch_emit_setret (cfg, method, *sp);
						}
#else
						mono_arch_emit_setret (cfg, method, *sp);
#endif
					}
				}
			}
			if (sp != stack_start)
				UNVERIFIED;
			MONO_INST_NEW (cfg, ins, OP_BR);
			ip++;
			ins->inst_target_bb = end_bblock;
			MONO_ADD_INS (bblock, ins);
			link_bblock (cfg, bblock, end_bblock);
			start_new_bblock = 1;
			break;
		case CEE_BR_S:
			CHECK_OPSIZE (2);
			MONO_INST_NEW (cfg, ins, OP_BR);
			ip++;
			target = ip + 1 + (signed char)(*ip);
			++ip;
			GET_BBLOCK (cfg, tblock, target);
			link_bblock (cfg, bblock, tblock);
			ins->inst_target_bb = tblock;
			if (sp != stack_start) {
				handle_stack_args (cfg, stack_start, sp - stack_start);
				sp = stack_start;
				CHECK_UNVERIFIABLE (cfg);
			}
			MONO_ADD_INS (bblock, ins);
			start_new_bblock = 1;
			inline_costs += BRANCH_COST;
			break;
		case CEE_BEQ_S:
		case CEE_BGE_S:
		case CEE_BGT_S:
		case CEE_BLE_S:
		case CEE_BLT_S:
		case CEE_BNE_UN_S:
		case CEE_BGE_UN_S:
		case CEE_BGT_UN_S:
		case CEE_BLE_UN_S:
		case CEE_BLT_UN_S:
			CHECK_OPSIZE (2);
			CHECK_STACK (2);
			MONO_INST_NEW (cfg, ins, *ip + BIG_BRANCH_OFFSET);
			ip++;
			target = ip + 1 + *(signed char*)ip;
			ip++;

			ADD_BINCOND (NULL);

			sp = stack_start;
			inline_costs += BRANCH_COST;
			break;
		case CEE_BR:
			CHECK_OPSIZE (5);
			MONO_INST_NEW (cfg, ins, OP_BR);
			ip++;

			target = ip + 4 + (gint32)read32(ip);
			ip += 4;
			GET_BBLOCK (cfg, tblock, target);
			link_bblock (cfg, bblock, tblock);
			ins->inst_target_bb = tblock;
			if (sp != stack_start) {
				handle_stack_args (cfg, stack_start, sp - stack_start);
				sp = stack_start;
				CHECK_UNVERIFIABLE (cfg);
			}

			MONO_ADD_INS (bblock, ins);

			start_new_bblock = 1;
			inline_costs += BRANCH_COST;
			break;
		case CEE_BRFALSE_S:
		case CEE_BRTRUE_S:
		case CEE_BRFALSE:
		case CEE_BRTRUE: {
			MonoInst *cmp;
			gboolean is_short = ((*ip) == CEE_BRFALSE_S) || ((*ip) == CEE_BRTRUE_S);
			gboolean is_true = ((*ip) == CEE_BRTRUE_S) || ((*ip) == CEE_BRTRUE);
			guint32 opsize = is_short ? 1 : 4;

			CHECK_OPSIZE (opsize);
			CHECK_STACK (1);
			if (sp [-1]->type == STACK_VTYPE || sp [-1]->type == STACK_R8)
				UNVERIFIED;
			ip ++;
			target = ip + opsize + (is_short ? *(signed char*)ip : (gint32)read32(ip));
			ip += opsize;

			sp--;

			GET_BBLOCK (cfg, tblock, target);
			link_bblock (cfg, bblock, tblock);
			GET_BBLOCK (cfg, tblock, ip);
			link_bblock (cfg, bblock, tblock);

			if (sp != stack_start) {
				handle_stack_args (cfg, stack_start, sp - stack_start);
				CHECK_UNVERIFIABLE (cfg);
			}

			MONO_INST_NEW(cfg, cmp, OP_ICOMPARE_IMM);
			cmp->sreg1 = sp [0]->dreg;
			type_from_op (cmp, sp [0], NULL);
			CHECK_TYPE (cmp);

#if SIZEOF_REGISTER == 4
			if (cmp->opcode == OP_LCOMPARE_IMM) {
				/* Convert it to OP_LCOMPARE */
				MONO_INST_NEW (cfg, ins, OP_I8CONST);
				ins->type = STACK_I8;
				ins->dreg = alloc_dreg (cfg, STACK_I8);
				ins->inst_l = 0;
				MONO_ADD_INS (bblock, ins);
				cmp->opcode = OP_LCOMPARE;
				cmp->sreg2 = ins->dreg;
			}
#endif
			MONO_ADD_INS (bblock, cmp);

			MONO_INST_NEW (cfg, ins, is_true ? CEE_BNE_UN : CEE_BEQ);
			type_from_op (ins, sp [0], NULL);
			MONO_ADD_INS (bblock, ins);
			ins->inst_many_bb = mono_mempool_alloc (cfg->mempool, sizeof(gpointer)*2);
			GET_BBLOCK (cfg, tblock, target);
			ins->inst_true_bb = tblock;
			GET_BBLOCK (cfg, tblock, ip);
			ins->inst_false_bb = tblock;
			start_new_bblock = 2;

			sp = stack_start;
			inline_costs += BRANCH_COST;
			break;
		}
		case CEE_BEQ:
		case CEE_BGE:
		case CEE_BGT:
		case CEE_BLE:
		case CEE_BLT:
		case CEE_BNE_UN:
		case CEE_BGE_UN:
		case CEE_BGT_UN:
		case CEE_BLE_UN:
		case CEE_BLT_UN:
			CHECK_OPSIZE (5);
			CHECK_STACK (2);
			MONO_INST_NEW (cfg, ins, *ip);
			ip++;
			target = ip + 4 + (gint32)read32(ip);
			ip += 4;

			ADD_BINCOND (NULL);

			sp = stack_start;
			inline_costs += BRANCH_COST;
			break;
		case CEE_SWITCH: {
			MonoInst *src1;
			MonoBasicBlock **targets;
			MonoBasicBlock *default_bblock;
			MonoJumpInfoBBTable *table;
			int offset_reg = alloc_preg (cfg);
			int target_reg = alloc_preg (cfg);
			int table_reg = alloc_preg (cfg);
			int sum_reg = alloc_preg (cfg);
			gboolean use_op_switch;

			CHECK_OPSIZE (5);
			CHECK_STACK (1);
			n = read32 (ip + 1);
			--sp;
			src1 = sp [0];
			if ((src1->type != STACK_I4) && (src1->type != STACK_PTR)) 
				UNVERIFIED;

			ip += 5;
			CHECK_OPSIZE (n * sizeof (guint32));
			target = ip + n * sizeof (guint32);

			GET_BBLOCK (cfg, default_bblock, target);

			targets = mono_mempool_alloc (cfg->mempool, sizeof (MonoBasicBlock*) * n);
			for (i = 0; i < n; ++i) {
				GET_BBLOCK (cfg, tblock, target + (gint32)read32(ip));
				targets [i] = tblock;
				ip += 4;
			}

			if (sp != stack_start) {
				/* 
				 * Link the current bb with the targets as well, so handle_stack_args
				 * will set their in_stack correctly.
				 */
				link_bblock (cfg, bblock, default_bblock);
				for (i = 0; i < n; ++i)
					link_bblock (cfg, bblock, targets [i]);

				handle_stack_args (cfg, stack_start, sp - stack_start);
				sp = stack_start;
				CHECK_UNVERIFIABLE (cfg);
			}

			MONO_EMIT_NEW_BIALU_IMM (cfg, OP_ICOMPARE_IMM, -1, src1->dreg, n);
			MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_IBGE_UN, default_bblock);
			bblock = cfg->cbb;

			for (i = 0; i < n; ++i)
				link_bblock (cfg, bblock, targets [i]);

			table = mono_mempool_alloc (cfg->mempool, sizeof (MonoJumpInfoBBTable));
			table->table = targets;
			table->table_size = n;

			use_op_switch = FALSE;
#ifdef TARGET_ARM
			/* ARM implements SWITCH statements differently */
			/* FIXME: Make it use the generic implementation */
			if (!cfg->compile_aot)
				use_op_switch = TRUE;
#endif
			
			if (use_op_switch) {
				MONO_INST_NEW (cfg, ins, OP_SWITCH);
				ins->sreg1 = src1->dreg;
				ins->inst_p0 = table;
				ins->inst_many_bb = targets;
				ins->klass = GUINT_TO_POINTER (n);
				MONO_ADD_INS (cfg->cbb, ins);
			} else {
				if (sizeof (gpointer) == 8)
					MONO_EMIT_NEW_BIALU_IMM (cfg, OP_SHL_IMM, offset_reg, src1->dreg, 3);
				else
					MONO_EMIT_NEW_BIALU_IMM (cfg, OP_SHL_IMM, offset_reg, src1->dreg, 2);

#if SIZEOF_REGISTER == 8
				/* The upper word might not be zero, and we add it to a 64 bit address later */
				MONO_EMIT_NEW_UNALU (cfg, OP_ZEXT_I4, offset_reg, offset_reg);
#endif

				if (cfg->compile_aot) {
					MONO_EMIT_NEW_AOTCONST (cfg, table_reg, table, MONO_PATCH_INFO_SWITCH);
				} else {
					MONO_INST_NEW (cfg, ins, OP_JUMP_TABLE);
					ins->inst_c1 = MONO_PATCH_INFO_SWITCH;
					ins->inst_p0 = table;
					ins->dreg = table_reg;
					MONO_ADD_INS (cfg->cbb, ins);
				}

				/* FIXME: Use load_memindex */
				MONO_EMIT_NEW_BIALU (cfg, OP_PADD, sum_reg, table_reg, offset_reg);
				MONO_EMIT_NEW_LOAD_MEMBASE (cfg, target_reg, sum_reg, 0);
				MONO_EMIT_NEW_UNALU (cfg, OP_BR_REG, -1, target_reg);
			}
			start_new_bblock = 1;
			inline_costs += (BRANCH_COST * 2);
			break;
		}
		case CEE_LDIND_I1:
		case CEE_LDIND_U1:
		case CEE_LDIND_I2:
		case CEE_LDIND_U2:
		case CEE_LDIND_I4:
		case CEE_LDIND_U4:
		case CEE_LDIND_I8:
		case CEE_LDIND_I:
		case CEE_LDIND_R4:
		case CEE_LDIND_R8:
		case CEE_LDIND_REF:
			CHECK_STACK (1);
			--sp;

			switch (*ip) {
			case CEE_LDIND_R4:
			case CEE_LDIND_R8:
				dreg = alloc_freg (cfg);
				break;
			case CEE_LDIND_I8:
				dreg = alloc_lreg (cfg);
				break;
			default:
				dreg = alloc_preg (cfg);
			}

			NEW_LOAD_MEMBASE (cfg, ins, ldind_to_load_membase (*ip), dreg, sp [0]->dreg, 0);
			ins->type = ldind_type [*ip - CEE_LDIND_I1];
			ins->flags |= ins_flag;
			ins_flag = 0;
			MONO_ADD_INS (bblock, ins);
			*sp++ = ins;
			++ip;
			break;
		case CEE_STIND_REF:
		case CEE_STIND_I1:
		case CEE_STIND_I2:
		case CEE_STIND_I4:
		case CEE_STIND_I8:
		case CEE_STIND_R4:
		case CEE_STIND_R8:
		case CEE_STIND_I:
			CHECK_STACK (2);
			sp -= 2;

#if HAVE_WRITE_BARRIERS
			if (*ip == CEE_STIND_REF && method->wrapper_type != MONO_WRAPPER_WRITE_BARRIER && !((sp [1]->opcode == OP_PCONST) && (sp [1]->inst_p0 == 0))) {
				/* insert call to write barrier */
				MonoMethod *write_barrier = mono_gc_get_write_barrier ();
				mono_emit_method_call (cfg, write_barrier, sp, NULL);
				ins_flag = 0;
				ip++;
				break;
			}
#endif

			NEW_STORE_MEMBASE (cfg, ins, stind_to_store_membase (*ip), sp [0]->dreg, 0, sp [1]->dreg);
			ins->flags |= ins_flag;
			ins_flag = 0;
			MONO_ADD_INS (bblock, ins);
			inline_costs += 1;
			++ip;
			break;

		case CEE_MUL:
			CHECK_STACK (2);

			MONO_INST_NEW (cfg, ins, (*ip));
			sp -= 2;
			ins->sreg1 = sp [0]->dreg;
			ins->sreg2 = sp [1]->dreg;
			type_from_op (ins, sp [0], sp [1]);
			CHECK_TYPE (ins);
			ins->dreg = alloc_dreg ((cfg), (ins)->type);

			/* Use the immediate opcodes if possible */
			if ((sp [1]->opcode == OP_ICONST) && mono_arch_is_inst_imm (sp [1]->inst_c0)) {
				int imm_opcode = mono_op_to_op_imm (ins->opcode);
				if (imm_opcode != -1) {
					ins->opcode = imm_opcode;
					ins->inst_p1 = (gpointer)(gssize)(sp [1]->inst_c0);
					ins->sreg2 = -1;

					sp [1]->opcode = OP_NOP;
				}
			}

			MONO_ADD_INS ((cfg)->cbb, (ins));
			*sp++ = ins;

			mono_decompose_opcode (cfg, ins);
			ip++;
			break;
		case CEE_ADD:
		case CEE_SUB:
		case CEE_DIV:
		case CEE_DIV_UN:
		case CEE_REM:
		case CEE_REM_UN:
		case CEE_AND:
		case CEE_OR:
		case CEE_XOR:
		case CEE_SHL:
		case CEE_SHR:
		case CEE_SHR_UN:
			CHECK_STACK (2);

			MONO_INST_NEW (cfg, ins, (*ip));
			sp -= 2;
			ins->sreg1 = sp [0]->dreg;
			ins->sreg2 = sp [1]->dreg;
			type_from_op (ins, sp [0], sp [1]);
			CHECK_TYPE (ins);
			ADD_WIDEN_OP (ins, sp [0], sp [1]);
			ins->dreg = alloc_dreg ((cfg), (ins)->type);

			/* FIXME: Pass opcode to is_inst_imm */

			/* Use the immediate opcodes if possible */
			if (((sp [1]->opcode == OP_ICONST) || (sp [1]->opcode == OP_I8CONST)) && mono_arch_is_inst_imm (sp [1]->opcode == OP_ICONST ? sp [1]->inst_c0 : sp [1]->inst_l)) {
				int imm_opcode;

				imm_opcode = mono_op_to_op_imm_noemul (ins->opcode);
				if (imm_opcode != -1) {
					ins->opcode = imm_opcode;
					if (sp [1]->opcode == OP_I8CONST) {
#if SIZEOF_REGISTER == 8
						ins->inst_imm = sp [1]->inst_l;
#else
						ins->inst_ls_word = sp [1]->inst_ls_word;
						ins->inst_ms_word = sp [1]->inst_ms_word;
#endif
					}
					else
						ins->inst_p1 = (gpointer)(gssize)(sp [1]->inst_c0);
					ins->sreg2 = -1;

					/* Might be followed by an instruction added by ADD_WIDEN_OP */
					if (sp [1]->next == NULL)
						sp [1]->opcode = OP_NOP;
				}
			}
			MONO_ADD_INS ((cfg)->cbb, (ins));
			*sp++ = ins;

			mono_decompose_opcode (cfg, ins);
			ip++;
			break;
		case CEE_NEG:
		case CEE_NOT:
		case CEE_CONV_I1:
		case CEE_CONV_I2:
		case CEE_CONV_I4:
		case CEE_CONV_R4:
		case CEE_CONV_R8:
		case CEE_CONV_U4:
		case CEE_CONV_I8:
		case CEE_CONV_U8:
		case CEE_CONV_OVF_I8:
		case CEE_CONV_OVF_U8:
		case CEE_CONV_R_UN:
			CHECK_STACK (1);

			/* Special case this earlier so we have long constants in the IR */
			if ((((*ip) == CEE_CONV_I8) || ((*ip) == CEE_CONV_U8)) && (sp [-1]->opcode == OP_ICONST)) {
				int data = sp [-1]->inst_c0;
				sp [-1]->opcode = OP_I8CONST;
				sp [-1]->type = STACK_I8;
#if SIZEOF_REGISTER == 8
				if ((*ip) == CEE_CONV_U8)
					sp [-1]->inst_c0 = (guint32)data;
				else
					sp [-1]->inst_c0 = data;
#else
				sp [-1]->inst_ls_word = data;
				if ((*ip) == CEE_CONV_U8)
					sp [-1]->inst_ms_word = 0;
				else
					sp [-1]->inst_ms_word = (data < 0) ? -1 : 0;
#endif
				sp [-1]->dreg = alloc_dreg (cfg, STACK_I8);
			}
			else {
				ADD_UNOP (*ip);
			}
			ip++;
			break;
		case CEE_CONV_OVF_I4:
		case CEE_CONV_OVF_I1:
		case CEE_CONV_OVF_I2:
		case CEE_CONV_OVF_I:
		case CEE_CONV_OVF_U:
			CHECK_STACK (1);

			if (sp [-1]->type == STACK_R8) {
				ADD_UNOP (CEE_CONV_OVF_I8);
				ADD_UNOP (*ip);
			} else {
				ADD_UNOP (*ip);
			}
			ip++;
			break;
		case CEE_CONV_OVF_U1:
		case CEE_CONV_OVF_U2:
		case CEE_CONV_OVF_U4:
			CHECK_STACK (1);

			if (sp [-1]->type == STACK_R8) {
				ADD_UNOP (CEE_CONV_OVF_U8);
				ADD_UNOP (*ip);
			} else {
				ADD_UNOP (*ip);
			}
			ip++;
			break;
		case CEE_CONV_OVF_I1_UN:
		case CEE_CONV_OVF_I2_UN:
		case CEE_CONV_OVF_I4_UN:
		case CEE_CONV_OVF_I8_UN:
		case CEE_CONV_OVF_U1_UN:
		case CEE_CONV_OVF_U2_UN:
		case CEE_CONV_OVF_U4_UN:
		case CEE_CONV_OVF_U8_UN:
		case CEE_CONV_OVF_I_UN:
		case CEE_CONV_OVF_U_UN:
		case CEE_CONV_U2:
		case CEE_CONV_U1:
		case CEE_CONV_I:
		case CEE_CONV_U:
			CHECK_STACK (1);
			ADD_UNOP (*ip);
			ip++;
			break;
		case CEE_ADD_OVF:
		case CEE_ADD_OVF_UN:
		case CEE_MUL_OVF:
		case CEE_MUL_OVF_UN:
		case CEE_SUB_OVF:
		case CEE_SUB_OVF_UN:
			CHECK_STACK (2);
			ADD_BINOP (*ip);
			ip++;
			break;
		case CEE_CPOBJ:
			CHECK_OPSIZE (5);
			CHECK_STACK (2);
			token = read32 (ip + 1);
			klass = mini_get_class (method, token, generic_context);
			CHECK_TYPELOAD (klass);
			sp -= 2;
			if (generic_class_is_reference_type (cfg, klass)) {
				MonoInst *store, *load;
				int dreg = alloc_preg (cfg);

				NEW_LOAD_MEMBASE (cfg, load, OP_LOAD_MEMBASE, dreg, sp [1]->dreg, 0);
				load->flags |= ins_flag;
				MONO_ADD_INS (cfg->cbb, load);

				NEW_STORE_MEMBASE (cfg, store, OP_STORE_MEMBASE_REG, sp [0]->dreg, 0, dreg);
				store->flags |= ins_flag;
				MONO_ADD_INS (cfg->cbb, store);
			} else {
				mini_emit_stobj (cfg, sp [0], sp [1], klass, FALSE);
			}
			ins_flag = 0;
			ip += 5;
			break;
		case CEE_LDOBJ: {
			int loc_index = -1;
			int stloc_len = 0;

			CHECK_OPSIZE (5);
			CHECK_STACK (1);
			--sp;
			token = read32 (ip + 1);
			klass = mini_get_class (method, token, generic_context);
			CHECK_TYPELOAD (klass);

			/* Optimize the common ldobj+stloc combination */
			switch (ip [5]) {
			case CEE_STLOC_S:
				loc_index = ip [6];
				stloc_len = 2;
				break;
			case CEE_STLOC_0:
			case CEE_STLOC_1:
			case CEE_STLOC_2:
			case CEE_STLOC_3:
				loc_index = ip [5] - CEE_STLOC_0;
				stloc_len = 1;
				break;
			default:
				break;
			}

			if ((loc_index != -1) && ip_in_bb (cfg, bblock, ip + 5)) {
				CHECK_LOCAL (loc_index);

				EMIT_NEW_LOAD_MEMBASE_TYPE (cfg, ins, &klass->byval_arg, sp [0]->dreg, 0);
				ins->dreg = cfg->locals [loc_index]->dreg;
				ip += 5;
				ip += stloc_len;
				break;
			}

			/* Optimize the ldobj+stobj combination */
			/* The reference case ends up being a load+store anyway */
			if (((ip [5] == CEE_STOBJ) && ip_in_bb (cfg, bblock, ip + 5) && read32 (ip + 6) == token) && !generic_class_is_reference_type (cfg, klass)) {
				CHECK_STACK (1);

				sp --;

				mini_emit_stobj (cfg, sp [0], sp [1], klass, FALSE);

				ip += 5 + 5;
				ins_flag = 0;
				break;
			}

			EMIT_NEW_LOAD_MEMBASE_TYPE (cfg, ins, &klass->byval_arg, sp [0]->dreg, 0);
			*sp++ = ins;

			ip += 5;
			ins_flag = 0;
			inline_costs += 1;
			break;
		}
		case CEE_LDSTR:
			CHECK_STACK_OVF (1);
			CHECK_OPSIZE (5);
			n = read32 (ip + 1);

			if (method->wrapper_type == MONO_WRAPPER_DYNAMIC_METHOD) {
				EMIT_NEW_PCONST (cfg, ins, mono_method_get_wrapper_data (method, n));
				ins->type = STACK_OBJ;
				*sp = ins;
			}
			else if (method->wrapper_type != MONO_WRAPPER_NONE) {
				MonoInst *iargs [1];

				EMIT_NEW_PCONST (cfg, iargs [0], mono_method_get_wrapper_data (method, n));				
				*sp = mono_emit_jit_icall (cfg, mono_string_new_wrapper, iargs);
			} else {
				if (cfg->opt & MONO_OPT_SHARED) {
					MonoInst *iargs [3];

					if (cfg->compile_aot) {
						cfg->ldstr_list = g_list_prepend (cfg->ldstr_list, GINT_TO_POINTER (n));
					}
					EMIT_NEW_DOMAINCONST (cfg, iargs [0]);
					EMIT_NEW_IMAGECONST (cfg, iargs [1], image);
					EMIT_NEW_ICONST (cfg, iargs [2], mono_metadata_token_index (n));
					*sp = mono_emit_jit_icall (cfg, mono_ldstr, iargs);
					mono_ldstr (cfg->domain, image, mono_metadata_token_index (n));
				} else {
					if (bblock->out_of_line) {
						MonoInst *iargs [2];

						if (image == mono_defaults.corlib) {
							/* 
							 * Avoid relocations in AOT and save some space by using a 
							 * version of helper_ldstr specialized to mscorlib.
							 */
							EMIT_NEW_ICONST (cfg, iargs [0], mono_metadata_token_index (n));
							*sp = mono_emit_jit_icall (cfg, mono_helper_ldstr_mscorlib, iargs);
						} else {
							/* Avoid creating the string object */
							EMIT_NEW_IMAGECONST (cfg, iargs [0], image);
							EMIT_NEW_ICONST (cfg, iargs [1], mono_metadata_token_index (n));
							*sp = mono_emit_jit_icall (cfg, mono_helper_ldstr, iargs);
						}
					} 
					else
					if (cfg->compile_aot) {
						NEW_LDSTRCONST (cfg, ins, image, n);
						*sp = ins;
						MONO_ADD_INS (bblock, ins);
					} 
					else {
						NEW_PCONST (cfg, ins, NULL);
						ins->type = STACK_OBJ;
						ins->inst_p0 = mono_ldstr (cfg->domain, image, mono_metadata_token_index (n));
						*sp = ins;
						MONO_ADD_INS (bblock, ins);
					}
				}
			}

			sp++;
			ip += 5;
			break;
		case CEE_NEWOBJ: {
			MonoInst *iargs [2];
			MonoMethodSignature *fsig;
			MonoInst this_ins;
			MonoInst *alloc;
			MonoInst *vtable_arg = NULL;

			CHECK_OPSIZE (5);
			token = read32 (ip + 1);
			cmethod = mini_get_method (cfg, method, token, NULL, generic_context);
			if (!cmethod)
				goto load_error;
			fsig = mono_method_get_signature (cmethod, image, token);

			mono_save_token_info (cfg, image, token, cmethod);

			if (!mono_class_init (cmethod->klass))
				goto load_error;

			if (cfg->generic_sharing_context)
				context_used = mono_method_check_context_used (cmethod);

			if (mono_security_get_mode () == MONO_SECURITY_MODE_CAS) {
				if (check_linkdemand (cfg, method, cmethod))
					INLINE_FAILURE;
				CHECK_CFG_EXCEPTION;
			} else if (mono_security_get_mode () == MONO_SECURITY_MODE_CORE_CLR) {
				ensure_method_is_allowed_to_call_method (cfg, method, cmethod, bblock, ip);
 			}

			if (cmethod->klass->valuetype && mono_class_generic_sharing_enabled (cmethod->klass) &&
					mono_method_is_generic_sharable_impl (cmethod, TRUE)) {
				if (cmethod->is_inflated && mono_method_get_context (cmethod)->method_inst) {
					if (context_used) {
						vtable_arg = emit_get_rgctx_method (cfg, context_used,
							cmethod, MONO_RGCTX_INFO_METHOD_RGCTX);
					} else {
						EMIT_NEW_METHOD_RGCTX_CONST (cfg, vtable_arg, cmethod);
					}
				} else {
					if (context_used) {
						vtable_arg = emit_get_rgctx_klass (cfg, context_used,
							cmethod->klass, MONO_RGCTX_INFO_VTABLE);
					} else {
						MonoVTable *vtable = mono_class_vtable (cfg->domain, cmethod->klass);

						CHECK_TYPELOAD (cmethod->klass);
						EMIT_NEW_VTABLECONST (cfg, vtable_arg, vtable);
					}
				}
			}

			n = fsig->param_count;
			CHECK_STACK (n);

			/* 
			 * Generate smaller code for the common newobj <exception> instruction in
			 * argument checking code.
			 */
			if (bblock->out_of_line && cmethod->klass->image == mono_defaults.corlib &&
				is_exception_class (cmethod->klass) && n <= 2 &&
				((n < 1) || (!fsig->params [0]->byref && fsig->params [0]->type == MONO_TYPE_STRING)) && 
				((n < 2) || (!fsig->params [1]->byref && fsig->params [1]->type == MONO_TYPE_STRING))) {
				MonoInst *iargs [3];

				g_assert (!vtable_arg);

				sp -= n;

				EMIT_NEW_ICONST (cfg, iargs [0], cmethod->klass->type_token);
				switch (n) {
				case 0:
					*sp ++ = mono_emit_jit_icall (cfg, mono_create_corlib_exception_0, iargs);
					break;
				case 1:
					iargs [1] = sp [0];
					*sp ++ = mono_emit_jit_icall (cfg, mono_create_corlib_exception_1, iargs);
					break;
				case 2:
					iargs [1] = sp [0];
					iargs [2] = sp [1];
					*sp ++ = mono_emit_jit_icall (cfg, mono_create_corlib_exception_2, iargs);
					break;
				default:
					g_assert_not_reached ();
				}

				ip += 5;
				inline_costs += 5;
				break;
			}

			/* move the args to allow room for 'this' in the first position */
			while (n--) {
				--sp;
				sp [1] = sp [0];
			}

			/* check_call_signature () requires sp[0] to be set */
			this_ins.type = STACK_OBJ;
			sp [0] = &this_ins;
			if (check_call_signature (cfg, fsig, sp))
				UNVERIFIED;

			iargs [0] = NULL;

			if (mini_class_is_system_array (cmethod->klass)) {
				if (context_used)
					GENERIC_SHARING_FAILURE (*ip);
				g_assert (!context_used);
				g_assert (!vtable_arg);
				EMIT_NEW_METHODCONST (cfg, *sp, cmethod);

				/* Avoid varargs in the common case */
				if (fsig->param_count == 1)
					alloc = mono_emit_jit_icall (cfg, mono_array_new_1, sp);
				else if (fsig->param_count == 2)
					alloc = mono_emit_jit_icall (cfg, mono_array_new_2, sp);
				else
					alloc = handle_array_new (cfg, fsig->param_count, sp, ip);
			} else if (cmethod->string_ctor) {
				g_assert (!context_used);
				g_assert (!vtable_arg);
				/* we simply pass a null pointer */
				EMIT_NEW_PCONST (cfg, *sp, NULL); 
				/* now call the string ctor */
				alloc = mono_emit_method_call_full (cfg, cmethod, fsig, sp, NULL, NULL);
			} else {
				MonoInst* callvirt_this_arg = NULL;
				
				if (cmethod->klass->valuetype) {
					iargs [0] = mono_compile_create_var (cfg, &cmethod->klass->byval_arg, OP_LOCAL);
					MONO_EMIT_NEW_VZERO (cfg, iargs [0]->dreg, cmethod->klass);
					EMIT_NEW_TEMPLOADA (cfg, *sp, iargs [0]->inst_c0);

					alloc = NULL;

					/* 
					 * The code generated by mini_emit_virtual_call () expects
					 * iargs [0] to be a boxed instance, but luckily the vcall
					 * will be transformed into a normal call there.
					 */
				} else if (context_used) {
					MonoInst *data;
					int rgctx_info;

					if (cfg->opt & MONO_OPT_SHARED)
						rgctx_info = MONO_RGCTX_INFO_KLASS;
					else
						rgctx_info = MONO_RGCTX_INFO_VTABLE;
					data = emit_get_rgctx_klass (cfg, context_used, cmethod->klass, rgctx_info);

					alloc = handle_alloc_from_inst (cfg, cmethod->klass, data, FALSE);
					*sp = alloc;
				} else {
					MonoVTable *vtable = mono_class_vtable (cfg->domain, cmethod->klass);

					CHECK_TYPELOAD (cmethod->klass);

					/*
					 * TypeInitializationExceptions thrown from the mono_runtime_class_init
					 * call in mono_jit_runtime_invoke () can abort the finalizer thread.
					 * As a workaround, we call class cctors before allocating objects.
					 */
					if (mini_field_access_needs_cctor_run (cfg, method, vtable) && !(g_slist_find (class_inits, vtable))) {
						mono_emit_abs_call (cfg, MONO_PATCH_INFO_CLASS_INIT, vtable->klass, helper_sig_class_init_trampoline, NULL);
						if (cfg->verbose_level > 2)
							printf ("class %s.%s needs init call for ctor\n", cmethod->klass->name_space, cmethod->klass->name);
						class_inits = g_slist_prepend (class_inits, vtable);
					}

					alloc = handle_alloc (cfg, cmethod->klass, FALSE);
					*sp = alloc;
				}

				if (alloc)
					MONO_EMIT_NEW_UNALU (cfg, OP_NOT_NULL, -1, alloc->dreg);

				/* Now call the actual ctor */
				/* Avoid virtual calls to ctors if possible */
				if (cmethod->klass->marshalbyref)
					callvirt_this_arg = sp [0];

				if ((cfg->opt & MONO_OPT_INLINE) && cmethod && !context_used && !vtable_arg &&
				    mono_method_check_inlining (cfg, cmethod) &&
				    !mono_class_is_subclass_of (cmethod->klass, mono_defaults.exception_class, FALSE) &&
				    !g_list_find (dont_inline, cmethod)) {
					int costs;

					if ((costs = inline_method (cfg, cmethod, fsig, sp, ip, cfg->real_offset, dont_inline, FALSE))) {
						cfg->real_offset += 5;
						bblock = cfg->cbb;

						inline_costs += costs - 5;
					} else {
						INLINE_FAILURE;
						if (!mono_emit_method_call_full (cfg, cmethod, fsig, sp, callvirt_this_arg, NULL))
							GENERIC_SHARING_FAILURE (*ip);
					}
				} else if (context_used &&
						(!mono_method_is_generic_sharable_impl (cmethod, TRUE) ||
							!mono_class_generic_sharing_enabled (cmethod->klass))) {
					MonoInst *cmethod_addr;

					cmethod_addr = emit_get_rgctx_method (cfg, context_used,
						cmethod, MONO_RGCTX_INFO_GENERIC_METHOD_CODE);

					mono_emit_rgctx_calli (cfg, fsig, sp, cmethod_addr, vtable_arg);
				} else {
					INLINE_FAILURE;
					mono_emit_rgctx_method_call_full (cfg, cmethod, fsig, sp,
							callvirt_this_arg, NULL, vtable_arg);
				}
			}

			if (alloc == NULL) {
				/* Valuetype */
				EMIT_NEW_TEMPLOAD (cfg, ins, iargs [0]->inst_c0);
				type_to_eval_stack_type (cfg, &ins->klass->byval_arg, ins);
				*sp++= ins;
			}
			else
				*sp++ = alloc;
			
			ip += 5;
			inline_costs += 5;
			break;
		}
		case CEE_CASTCLASS:
			CHECK_STACK (1);
			--sp;
			CHECK_OPSIZE (5);
			token = read32 (ip + 1);
			klass = mini_get_class (method, token, generic_context);
			CHECK_TYPELOAD (klass);
			if (sp [0]->type != STACK_OBJ)
				UNVERIFIED;

			if (cfg->generic_sharing_context)
				context_used = mono_class_check_context_used (klass);

			if (context_used) {
				MonoInst *args [2];

				/* obj */
				args [0] = *sp;

				/* klass */
				args [1] = emit_get_rgctx_klass (cfg, context_used,
					klass, MONO_RGCTX_INFO_KLASS);

				ins = mono_emit_jit_icall (cfg, mono_object_castclass, args);
				*sp ++ = ins;
				ip += 5;
				inline_costs += 2;
			} else if (klass->marshalbyref || klass->flags & TYPE_ATTRIBUTE_INTERFACE) {
				MonoMethod *mono_castclass;
				MonoInst *iargs [1];
				int costs;

				mono_castclass = mono_marshal_get_castclass (klass); 
				iargs [0] = sp [0];
				
				costs = inline_method (cfg, mono_castclass, mono_method_signature (mono_castclass), 
							   iargs, ip, cfg->real_offset, dont_inline, TRUE);			
				g_assert (costs > 0);
				
				ip += 5;
				cfg->real_offset += 5;
				bblock = cfg->cbb;

 				*sp++ = iargs [0];

				inline_costs += costs;
			}
			else {
				ins = handle_castclass (cfg, klass, *sp);
				bblock = cfg->cbb;
				*sp ++ = ins;
				ip += 5;
			}
			break;
		case CEE_ISINST: {
			CHECK_STACK (1);
			--sp;
			CHECK_OPSIZE (5);
			token = read32 (ip + 1);
			klass = mini_get_class (method, token, generic_context);
			CHECK_TYPELOAD (klass);
			if (sp [0]->type != STACK_OBJ)
				UNVERIFIED;
 
			if (cfg->generic_sharing_context)
				context_used = mono_class_check_context_used (klass);

			if (context_used) {
				MonoInst *args [2];

				/* obj */
				args [0] = *sp;

				/* klass */
				args [1] = emit_get_rgctx_klass (cfg, context_used, klass, MONO_RGCTX_INFO_KLASS);

				*sp = mono_emit_jit_icall (cfg, mono_object_isinst, args);
				sp++;
				ip += 5;
				inline_costs += 2;
			} else if (klass->marshalbyref || klass->flags & TYPE_ATTRIBUTE_INTERFACE) {			
				MonoMethod *mono_isinst;
				MonoInst *iargs [1];
				int costs;

				mono_isinst = mono_marshal_get_isinst (klass); 
				iargs [0] = sp [0];

				costs = inline_method (cfg, mono_isinst, mono_method_signature (mono_isinst), 
							   iargs, ip, cfg->real_offset, dont_inline, TRUE);			
				g_assert (costs > 0);
				
				ip += 5;
				cfg->real_offset += 5;
				bblock = cfg->cbb;

				*sp++= iargs [0];

				inline_costs += costs;
			}
			else {
				ins = handle_isinst (cfg, klass, *sp);
				bblock = cfg->cbb;
				*sp ++ = ins;
				ip += 5;
			}
			break;
		}
		case CEE_UNBOX_ANY: {
			CHECK_STACK (1);
			--sp;
			CHECK_OPSIZE (5);
			token = read32 (ip + 1);
			klass = mini_get_class (method, token, generic_context);
			CHECK_TYPELOAD (klass);
 
			mono_save_token_info (cfg, image, token, klass);

			if (cfg->generic_sharing_context)
				context_used = mono_class_check_context_used (klass);

			if (generic_class_is_reference_type (cfg, klass)) {
				/* CASTCLASS */
				if (context_used) {
					MonoInst *iargs [2];

					/* obj */
					iargs [0] = *sp;
					/* klass */
					iargs [1] = emit_get_rgctx_klass (cfg, context_used, klass, MONO_RGCTX_INFO_KLASS);
					ins = mono_emit_jit_icall (cfg, mono_object_castclass, iargs);
					*sp ++ = ins;
					ip += 5;
					inline_costs += 2;
				} else if (klass->marshalbyref || klass->flags & TYPE_ATTRIBUTE_INTERFACE) {				
					MonoMethod *mono_castclass;
					MonoInst *iargs [1];
					int costs;

					mono_castclass = mono_marshal_get_castclass (klass); 
					iargs [0] = sp [0];

					costs = inline_method (cfg, mono_castclass, mono_method_signature (mono_castclass), 
										   iargs, ip, cfg->real_offset, dont_inline, TRUE);
			
					g_assert (costs > 0);
				
					ip += 5;
					cfg->real_offset += 5;
					bblock = cfg->cbb;

					*sp++ = iargs [0];
					inline_costs += costs;
				} else {
					ins = handle_castclass (cfg, klass, *sp);
					bblock = cfg->cbb;
					*sp ++ = ins;
					ip += 5;
				}
				break;
			}

			if (mono_class_is_nullable (klass)) {
				ins = handle_unbox_nullable (cfg, *sp, klass, context_used);
				*sp++= ins;
				ip += 5;
				break;
			}

			/* UNBOX */
			ins = handle_unbox (cfg, klass, sp, context_used);
			*sp = ins;

			ip += 5;

			/* LDOBJ */
			EMIT_NEW_LOAD_MEMBASE_TYPE (cfg, ins, &klass->byval_arg, sp [0]->dreg, 0);
			*sp++ = ins;

			inline_costs += 2;
			break;
		}
		case CEE_BOX: {
			MonoInst *val;

			CHECK_STACK (1);
			--sp;
			val = *sp;
			CHECK_OPSIZE (5);
			token = read32 (ip + 1);
			klass = mini_get_class (method, token, generic_context);
			CHECK_TYPELOAD (klass);

			mono_save_token_info (cfg, image, token, klass);

			if (cfg->generic_sharing_context)
				context_used = mono_class_check_context_used (klass);

			if (generic_class_is_reference_type (cfg, klass)) {
				*sp++ = val;
				ip += 5;
				break;
			}

			if (klass == mono_defaults.void_class)
				UNVERIFIED;
			if (target_type_is_incompatible (cfg, &klass->byval_arg, *sp))
				UNVERIFIED;
			/* frequent check in generic code: box (struct), brtrue */
			if (!mono_class_is_nullable (klass) &&
				ip + 5 < end && ip_in_bb (cfg, bblock, ip + 5) && (ip [5] == CEE_BRTRUE || ip [5] == CEE_BRTRUE_S)) {
				/*printf ("box-brtrue opt at 0x%04x in %s\n", real_offset, method->name);*/
				ip += 5;
				MONO_INST_NEW (cfg, ins, OP_BR);
				if (*ip == CEE_BRTRUE_S) {
					CHECK_OPSIZE (2);
					ip++;
					target = ip + 1 + (signed char)(*ip);
					ip++;
				} else {
					CHECK_OPSIZE (5);
					ip++;
					target = ip + 4 + (gint)(read32 (ip));
					ip += 4;
				}
				GET_BBLOCK (cfg, tblock, target);
				link_bblock (cfg, bblock, tblock);
				ins->inst_target_bb = tblock;
				GET_BBLOCK (cfg, tblock, ip);
				/* 
				 * This leads to some inconsistency, since the two bblocks are 
				 * not really connected, but it is needed for handling stack 
				 * arguments correctly (See test_0_box_brtrue_opt_regress_81102).
				 * FIXME: This should only be needed if sp != stack_start, but that
				 * doesn't work for some reason (test failure in mcs/tests on x86).
				 */
				link_bblock (cfg, bblock, tblock);
				if (sp != stack_start) {
					handle_stack_args (cfg, stack_start, sp - stack_start);
					sp = stack_start;
					CHECK_UNVERIFIABLE (cfg);
				}
				MONO_ADD_INS (bblock, ins);
				start_new_bblock = 1;
				break;
			}

			if (context_used) {
				MonoInst *data;
				int rgctx_info;

				if (cfg->opt & MONO_OPT_SHARED)
					rgctx_info = MONO_RGCTX_INFO_KLASS;
				else
					rgctx_info = MONO_RGCTX_INFO_VTABLE;
				data = emit_get_rgctx_klass (cfg, context_used, klass, rgctx_info);
				*sp++ = handle_box_from_inst (cfg, val, klass, context_used, data);
			} else {
				*sp++ = handle_box (cfg, val, klass);
			}

			ip += 5;
			inline_costs += 1;
			break;
		}
		case CEE_UNBOX: {
			CHECK_STACK (1);
			--sp;
			CHECK_OPSIZE (5);
			token = read32 (ip + 1);
			klass = mini_get_class (method, token, generic_context);
			CHECK_TYPELOAD (klass);

			mono_save_token_info (cfg, image, token, klass);

			if (cfg->generic_sharing_context)
				context_used = mono_class_check_context_used (klass);

			if (mono_class_is_nullable (klass)) {
				MonoInst *val;

				val = handle_unbox_nullable (cfg, *sp, klass, context_used);
				EMIT_NEW_VARLOADA (cfg, ins, get_vreg_to_inst (cfg, val->dreg), &val->klass->byval_arg);

				*sp++= ins;
			} else {
				ins = handle_unbox (cfg, klass, sp, context_used);
				*sp++ = ins;
			}
			ip += 5;
			inline_costs += 2;
			break;
		}
		case CEE_LDFLD:
		case CEE_LDFLDA:
		case CEE_STFLD: {
			MonoClassField *field;
			int costs;
			guint foffset;

			if (*ip == CEE_STFLD) {
				CHECK_STACK (2);
				sp -= 2;
			} else {
				CHECK_STACK (1);
				--sp;
			}
			if (sp [0]->type == STACK_I4 || sp [0]->type == STACK_I8 || sp [0]->type == STACK_R8)
				UNVERIFIED;
			if (*ip != CEE_LDFLD && sp [0]->type == STACK_VTYPE)
				UNVERIFIED;
			CHECK_OPSIZE (5);
			token = read32 (ip + 1);
			if (method->wrapper_type != MONO_WRAPPER_NONE) {
				field = mono_method_get_wrapper_data (method, token);
				klass = field->parent;
			}
			else {
				field = mono_field_from_token (image, token, &klass, generic_context);
			}
			if (!field)
				goto load_error;
			if (!dont_verify && !cfg->skip_visibility && !mono_method_can_access_field (method, field))
				FIELD_ACCESS_FAILURE;
			mono_class_init (klass);

			foffset = klass->valuetype? field->offset - sizeof (MonoObject): field->offset;
			if (*ip == CEE_STFLD) {
				if (target_type_is_incompatible (cfg, field->type, sp [1]))
					UNVERIFIED;
				if ((klass->marshalbyref && !MONO_CHECK_THIS (sp [0])) || klass->contextbound || klass == mono_defaults.marshalbyrefobject_class) {
					MonoMethod *stfld_wrapper = mono_marshal_get_stfld_wrapper (field->type); 
					MonoInst *iargs [5];

					iargs [0] = sp [0];
					EMIT_NEW_CLASSCONST (cfg, iargs [1], klass);
					EMIT_NEW_FIELDCONST (cfg, iargs [2], field);
					EMIT_NEW_ICONST (cfg, iargs [3], klass->valuetype ? field->offset - sizeof (MonoObject) : 
						    field->offset);
					iargs [4] = sp [1];

					if (cfg->opt & MONO_OPT_INLINE) {
						costs = inline_method (cfg, stfld_wrapper, mono_method_signature (stfld_wrapper), 
								       iargs, ip, cfg->real_offset, dont_inline, TRUE);
						g_assert (costs > 0);
						      
						cfg->real_offset += 5;
						bblock = cfg->cbb;

						inline_costs += costs;
					} else {
						mono_emit_method_call (cfg, stfld_wrapper, iargs, NULL);
					}
				} else {
					MonoInst *store;

#if HAVE_WRITE_BARRIERS
				if (mini_type_to_stind (cfg, field->type) == CEE_STIND_REF && !(sp [1]->opcode == OP_PCONST && sp [1]->inst_c0 == 0)) {
					/* insert call to write barrier */
					MonoMethod *write_barrier = mono_gc_get_write_barrier ();
					MonoInst *iargs [2];
					int dreg;

					dreg = alloc_preg (cfg);
					EMIT_NEW_BIALU_IMM (cfg, iargs [0], OP_PADD_IMM, dreg, sp [0]->dreg, foffset);
					iargs [1] = sp [1];
					mono_emit_method_call (cfg, write_barrier, iargs, NULL);
				}
#endif

					EMIT_NEW_STORE_MEMBASE_TYPE (cfg, store, field->type, sp [0]->dreg, foffset, sp [1]->dreg);
						
					store->flags |= ins_flag;
				}
				ins_flag = 0;
				ip += 5;
				break;
			}

			if ((klass->marshalbyref && !MONO_CHECK_THIS (sp [0])) || klass->contextbound || klass == mono_defaults.marshalbyrefobject_class) {
				MonoMethod *wrapper = (*ip == CEE_LDFLDA) ? mono_marshal_get_ldflda_wrapper (field->type) : mono_marshal_get_ldfld_wrapper (field->type); 
				MonoInst *iargs [4];

				iargs [0] = sp [0];
				EMIT_NEW_CLASSCONST (cfg, iargs [1], klass);
				EMIT_NEW_FIELDCONST (cfg, iargs [2], field);
				EMIT_NEW_ICONST (cfg, iargs [3], klass->valuetype ? field->offset - sizeof (MonoObject) : field->offset);
				if ((cfg->opt & MONO_OPT_INLINE) && !MONO_TYPE_ISSTRUCT (mono_method_signature (wrapper)->ret)) {
					costs = inline_method (cfg, wrapper, mono_method_signature (wrapper), 
										   iargs, ip, cfg->real_offset, dont_inline, TRUE);
					bblock = cfg->cbb;
					g_assert (costs > 0);
						      
					cfg->real_offset += 5;

					*sp++ = iargs [0];

					inline_costs += costs;
				} else {
					ins = mono_emit_method_call (cfg, wrapper, iargs, NULL);
					*sp++ = ins;
				}
			} else {
				if (sp [0]->type == STACK_VTYPE) {
					MonoInst *var;

					/* Have to compute the address of the variable */

					var = get_vreg_to_inst (cfg, sp [0]->dreg);
					if (!var)
						var = mono_compile_create_var_for_vreg (cfg, &klass->byval_arg, OP_LOCAL, sp [0]->dreg);
					else
						g_assert (var->klass == klass);
					
					EMIT_NEW_VARLOADA (cfg, ins, var, &var->klass->byval_arg);
					sp [0] = ins;
				}

				if (*ip == CEE_LDFLDA) {
					dreg = alloc_preg (cfg);

					EMIT_NEW_BIALU_IMM (cfg, ins, OP_PADD_IMM, dreg, sp [0]->dreg, foffset);
					ins->klass = mono_class_from_mono_type (field->type);
					ins->type = STACK_MP;
					*sp++ = ins;
				} else {
					MonoInst *load;

					EMIT_NEW_LOAD_MEMBASE_TYPE (cfg, load, field->type, sp [0]->dreg, foffset);
					load->flags |= ins_flag;
					*sp++ = load;
				}
			}
			ins_flag = 0;
			ip += 5;
			break;
		}
		case CEE_LDSFLD:
		case CEE_LDSFLDA:
		case CEE_STSFLD: {
			MonoClassField *field;
			gpointer addr = NULL;
			gboolean is_special_static;

			CHECK_OPSIZE (5);
			token = read32 (ip + 1);

			if (method->wrapper_type != MONO_WRAPPER_NONE) {
				field = mono_method_get_wrapper_data (method, token);
				klass = field->parent;
			}
			else
				field = mono_field_from_token (image, token, &klass, generic_context);
			if (!field)
				goto load_error;
			mono_class_init (klass);
			if (!dont_verify && !cfg->skip_visibility && !mono_method_can_access_field (method, field))
				FIELD_ACCESS_FAILURE;

			/*
			 * We can only support shared generic static
			 * field access on architectures where the
			 * trampoline code has been extended to handle
			 * the generic class init.
			 */
#ifndef MONO_ARCH_VTABLE_REG
			GENERIC_SHARING_FAILURE (*ip);
#endif

			if (cfg->generic_sharing_context)
				context_used = mono_class_check_context_used (klass);

			g_assert (!(field->type->attrs & FIELD_ATTRIBUTE_LITERAL));

			/* The special_static_fields field is init'd in mono_class_vtable, so it needs
			 * to be called here.
			 */
			if (!context_used && !(cfg->opt & MONO_OPT_SHARED)) {
				mono_class_vtable (cfg->domain, klass);
				CHECK_TYPELOAD (klass);
			}
			mono_domain_lock (cfg->domain);
			if (cfg->domain->special_static_fields)
				addr = g_hash_table_lookup (cfg->domain->special_static_fields, field);
			mono_domain_unlock (cfg->domain);

			is_special_static = mono_class_field_is_special_static (field);

			/* Generate IR to compute the field address */

			if ((cfg->opt & MONO_OPT_SHARED) ||
					(cfg->compile_aot && is_special_static) ||
					(context_used && is_special_static)) {
				MonoInst *iargs [2];

				g_assert (field->parent);
				EMIT_NEW_DOMAINCONST (cfg, iargs [0]);
				if (context_used) {
					iargs [1] = emit_get_rgctx_field (cfg, context_used,
						field, MONO_RGCTX_INFO_CLASS_FIELD);
				} else {
					EMIT_NEW_FIELDCONST (cfg, iargs [1], field);
				}
				ins = mono_emit_jit_icall (cfg, mono_class_static_field_address, iargs);
			} else if (context_used) {
				MonoInst *static_data;

				/*
				g_print ("sharing static field access in %s.%s.%s - depth %d offset %d\n",
					method->klass->name_space, method->klass->name, method->name,
					depth, field->offset);
				*/

				if (mono_class_needs_cctor_run (klass, method)) {
					MonoCallInst *call;
					MonoInst *vtable;

					vtable = emit_get_rgctx_klass (cfg, context_used,
						klass, MONO_RGCTX_INFO_VTABLE);

					// FIXME: This doesn't work since it tries to pass the argument
					// in the normal way, instead of using MONO_ARCH_VTABLE_REG
					/* 
					 * The vtable pointer is always passed in a register regardless of
					 * the calling convention, so assign it manually, and make a call
					 * using a signature without parameters.
					 */					
					call = (MonoCallInst*)mono_emit_abs_call (cfg, MONO_PATCH_INFO_GENERIC_CLASS_INIT, NULL, helper_sig_generic_class_init_trampoline, &vtable);
#ifdef MONO_ARCH_VTABLE_REG
					mono_call_inst_add_outarg_reg (cfg, call, vtable->dreg, MONO_ARCH_VTABLE_REG, FALSE);
					cfg->uses_vtable_reg = TRUE;
#else
					NOT_IMPLEMENTED;
#endif
				}

				/*
				 * The pointer we're computing here is
				 *
				 *   super_info.static_data + field->offset
				 */
				static_data = emit_get_rgctx_klass (cfg, context_used,
					klass, MONO_RGCTX_INFO_STATIC_DATA);

				if (field->offset == 0) {
					ins = static_data;
				} else {
					int addr_reg = mono_alloc_preg (cfg);
					EMIT_NEW_BIALU_IMM (cfg, ins, OP_PADD_IMM, addr_reg, static_data->dreg, field->offset);
				}
				} else if ((cfg->opt & MONO_OPT_SHARED) || (cfg->compile_aot && addr)) {
				MonoInst *iargs [2];

				g_assert (field->parent);
				EMIT_NEW_DOMAINCONST (cfg, iargs [0]);
				EMIT_NEW_FIELDCONST (cfg, iargs [1], field);
				ins = mono_emit_jit_icall (cfg, mono_class_static_field_address, iargs);
			} else {
				MonoVTable *vtable = mono_class_vtable (cfg->domain, klass);

				CHECK_TYPELOAD (klass);
				if (!addr) {
					if (mini_field_access_needs_cctor_run (cfg, method, vtable) && !(g_slist_find (class_inits, vtable))) {
						mono_emit_abs_call (cfg, MONO_PATCH_INFO_CLASS_INIT, vtable->klass, helper_sig_class_init_trampoline, NULL);
						if (cfg->verbose_level > 2)
							printf ("class %s.%s needs init call for %s\n", klass->name_space, klass->name, mono_field_get_name (field));
						class_inits = g_slist_prepend (class_inits, vtable);
					} else {
						if (cfg->run_cctors) {
							MonoException *ex;
							/* This makes so that inline cannot trigger */
							/* .cctors: too many apps depend on them */
							/* running with a specific order... */
							if (! vtable->initialized)
								INLINE_FAILURE;
							ex = mono_runtime_class_init_full (vtable, FALSE);
							if (ex) {
								set_exception_object (cfg, ex);
								goto exception_exit;
							}
						}
					}
					addr = (char*)vtable->data + field->offset;

					if (cfg->compile_aot)
						EMIT_NEW_SFLDACONST (cfg, ins, field);
					else
						EMIT_NEW_PCONST (cfg, ins, addr);
				} else {
					/* 
					 * insert call to mono_threads_get_static_data (GPOINTER_TO_UINT (addr)) 
					 * This could be later optimized to do just a couple of
					 * memory dereferences with constant offsets.
					 */
					MonoInst *iargs [1];
					EMIT_NEW_ICONST (cfg, iargs [0], GPOINTER_TO_UINT (addr));
					ins = mono_emit_jit_icall (cfg, mono_get_special_static_data, iargs);
				}
			}

			/* Generate IR to do the actual load/store operation */

			if (*ip == CEE_LDSFLDA) {
				ins->klass = mono_class_from_mono_type (field->type);
				ins->type = STACK_PTR;
				*sp++ = ins;
			} else if (*ip == CEE_STSFLD) {
				MonoInst *store;
				CHECK_STACK (1);
				sp--;

				EMIT_NEW_STORE_MEMBASE_TYPE (cfg, store, field->type, ins->dreg, 0, sp [0]->dreg);
				store->flags |= ins_flag;
			} else {
				gboolean is_const = FALSE;
				MonoVTable *vtable = NULL;

				if (!context_used) {
					vtable = mono_class_vtable (cfg->domain, klass);
					CHECK_TYPELOAD (klass);
				}
				if (!context_used && !((cfg->opt & MONO_OPT_SHARED) || cfg->compile_aot) && 
				    vtable->initialized && (field->type->attrs & FIELD_ATTRIBUTE_INIT_ONLY)) {
					gpointer addr = (char*)vtable->data + field->offset;
					int ro_type = field->type->type;
					if (ro_type == MONO_TYPE_VALUETYPE && field->type->data.klass->enumtype) {
						ro_type = field->type->data.klass->enum_basetype->type;
					}
					/* printf ("RO-FIELD %s.%s:%s\n", klass->name_space, klass->name, mono_field_get_name (field));*/
					is_const = TRUE;
					switch (ro_type) {
					case MONO_TYPE_BOOLEAN:
					case MONO_TYPE_U1:
						EMIT_NEW_ICONST (cfg, *sp, *((guint8 *)addr));
						sp++;
						break;
					case MONO_TYPE_I1:
						EMIT_NEW_ICONST (cfg, *sp, *((gint8 *)addr));
						sp++;
						break;						
					case MONO_TYPE_CHAR:
					case MONO_TYPE_U2:
						EMIT_NEW_ICONST (cfg, *sp, *((guint16 *)addr));
						sp++;
						break;
					case MONO_TYPE_I2:
						EMIT_NEW_ICONST (cfg, *sp, *((gint16 *)addr));
						sp++;
						break;
						break;
					case MONO_TYPE_I4:
						EMIT_NEW_ICONST (cfg, *sp, *((gint32 *)addr));
						sp++;
						break;						
					case MONO_TYPE_U4:
						EMIT_NEW_ICONST (cfg, *sp, *((guint32 *)addr));
						sp++;
						break;
#ifndef HAVE_MOVING_COLLECTOR
					case MONO_TYPE_I:
					case MONO_TYPE_U:
					case MONO_TYPE_STRING:
					case MONO_TYPE_OBJECT:
					case MONO_TYPE_CLASS:
					case MONO_TYPE_SZARRAY:
					case MONO_TYPE_PTR:
					case MONO_TYPE_FNPTR:
					case MONO_TYPE_ARRAY:
						EMIT_NEW_PCONST (cfg, *sp, *((gpointer *)addr));
						type_to_eval_stack_type ((cfg), field->type, *sp);
						sp++;
						break;
#endif
					case MONO_TYPE_I8:
					case MONO_TYPE_U8:
						EMIT_NEW_I8CONST (cfg, *sp, *((gint64 *)addr));
						sp++;
						break;
					case MONO_TYPE_R4:
					case MONO_TYPE_R8:
					case MONO_TYPE_VALUETYPE:
					default:
						is_const = FALSE;
						break;
					}
				}

				if (!is_const) {
					MonoInst *load;

					CHECK_STACK_OVF (1);

					EMIT_NEW_LOAD_MEMBASE_TYPE (cfg, load, field->type, ins->dreg, 0);
					load->flags |= ins_flag;
					ins_flag = 0;
					*sp++ = load;
				}
			}
			ins_flag = 0;
			ip += 5;
			break;
		}
		case CEE_STOBJ:
			CHECK_STACK (2);
			sp -= 2;
			CHECK_OPSIZE (5);
			token = read32 (ip + 1);
			klass = mini_get_class (method, token, generic_context);
			CHECK_TYPELOAD (klass);
			/* FIXME: should check item at sp [1] is compatible with the type of the store. */
			EMIT_NEW_STORE_MEMBASE_TYPE (cfg, ins, &klass->byval_arg, sp [0]->dreg, 0, sp [1]->dreg);
			ins_flag = 0;
			ip += 5;
			inline_costs += 1;
			break;

			/*
			 * Array opcodes
			 */
		case CEE_NEWARR: {
			MonoInst *len_ins;
			const char *data_ptr;
			int data_size = 0;
			guint32 field_token;

			CHECK_STACK (1);
			--sp;

			CHECK_OPSIZE (5);
			token = read32 (ip + 1);

			klass = mini_get_class (method, token, generic_context);
			CHECK_TYPELOAD (klass);

			if (cfg->generic_sharing_context)
				context_used = mono_class_check_context_used (klass);

			if (sp [0]->type == STACK_I8 || (SIZEOF_VOID_P == 8 && sp [0]->type == STACK_PTR)) {
				MONO_INST_NEW (cfg, ins, OP_LCONV_TO_I4);
				ins->sreg1 = sp [0]->dreg;
				ins->type = STACK_I4;
				ins->dreg = alloc_ireg (cfg);
				MONO_ADD_INS (cfg->cbb, ins);
				mono_decompose_opcode (cfg, ins);
				*sp = ins;
			}

			if (context_used) {
				MonoInst *args [2];

				/* FIXME: Decompose later to help abcrem */

				/* vtable */
				args [0] = emit_get_rgctx_klass (cfg, context_used,
					mono_array_class_get (klass, 1), MONO_RGCTX_INFO_VTABLE);

				/* array len */
				args [1] = sp [0];

				ins = mono_emit_jit_icall (cfg, mono_array_new_specific, args);
			} else {
				if (cfg->opt & MONO_OPT_SHARED) {
					/* Decompose now to avoid problems with references to the domainvar */
					MonoInst *iargs [3];

					EMIT_NEW_DOMAINCONST (cfg, iargs [0]);
					EMIT_NEW_CLASSCONST (cfg, iargs [1], klass);
					iargs [2] = sp [0];

					ins = mono_emit_jit_icall (cfg, mono_array_new, iargs);
				} else {
					/* Decompose later since it is needed by abcrem */
					MONO_INST_NEW (cfg, ins, OP_NEWARR);
					ins->dreg = alloc_preg (cfg);
					ins->sreg1 = sp [0]->dreg;
					ins->inst_newa_class = klass;
					ins->type = STACK_OBJ;
					ins->klass = klass;
					MONO_ADD_INS (cfg->cbb, ins);
					cfg->flags |= MONO_CFG_HAS_ARRAY_ACCESS;
					cfg->cbb->has_array_access = TRUE;

					/* Needed so mono_emit_load_get_addr () gets called */
					mono_get_got_var (cfg);
				}
			}

			len_ins = sp [0];
			ip += 5;
			*sp++ = ins;
			inline_costs += 1;

			/* 
			 * we inline/optimize the initialization sequence if possible.
			 * we should also allocate the array as not cleared, since we spend as much time clearing to 0 as initializing
			 * for small sizes open code the memcpy
			 * ensure the rva field is big enough
			 */
			if ((cfg->opt & MONO_OPT_INTRINS) && ip + 6 < end && ip_in_bb (cfg, bblock, ip + 6) && (len_ins->opcode == OP_ICONST) && (data_ptr = initialize_array_data (method, cfg->compile_aot, ip, klass, len_ins->inst_c0, &data_size, &field_token))) {
				MonoMethod *memcpy_method = get_memcpy_method ();
				MonoInst *iargs [3];
				int add_reg = alloc_preg (cfg);

				EMIT_NEW_BIALU_IMM (cfg, iargs [0], OP_PADD_IMM, add_reg, ins->dreg, G_STRUCT_OFFSET (MonoArray, vector));
				if (cfg->compile_aot) {
					EMIT_NEW_AOTCONST_TOKEN (cfg, iargs [1], MONO_PATCH_INFO_RVA, method->klass->image, GPOINTER_TO_UINT(field_token), STACK_PTR, NULL);
				} else {
					EMIT_NEW_PCONST (cfg, iargs [1], (char*)data_ptr);
				}
				EMIT_NEW_ICONST (cfg, iargs [2], data_size);
				mono_emit_method_call (cfg, memcpy_method, iargs, NULL);
				ip += 11;
			}

			break;
		}
		case CEE_LDLEN:
			CHECK_STACK (1);
			--sp;
			if (sp [0]->type != STACK_OBJ)
				UNVERIFIED;

			dreg = alloc_preg (cfg);
			MONO_INST_NEW (cfg, ins, OP_LDLEN);
			ins->dreg = alloc_preg (cfg);
			ins->sreg1 = sp [0]->dreg;
			ins->type = STACK_I4;
			MONO_ADD_INS (cfg->cbb, ins);
			cfg->flags |= MONO_CFG_HAS_ARRAY_ACCESS;
			cfg->cbb->has_array_access = TRUE;
			ip ++;
			*sp++ = ins;
			break;
		case CEE_LDELEMA:
			CHECK_STACK (2);
			sp -= 2;
			CHECK_OPSIZE (5);
			if (sp [0]->type != STACK_OBJ)
				UNVERIFIED;

			cfg->flags |= MONO_CFG_HAS_LDELEMA;

			klass = mini_get_class (method, read32 (ip + 1), generic_context);
			CHECK_TYPELOAD (klass);
			/* we need to make sure that this array is exactly the type it needs
			 * to be for correctness. the wrappers are lax with their usage
			 * so we need to ignore them here
			 */
			if (!klass->valuetype && method->wrapper_type == MONO_WRAPPER_NONE && !readonly)
				mini_emit_check_array_type (cfg, sp [0], mono_array_class_get (klass, 1));

			readonly = FALSE;
			ins = mini_emit_ldelema_1_ins (cfg, klass, sp [0], sp [1]);
			*sp++ = ins;
			ip += 5;
			break;
		case CEE_LDELEM_ANY:
		case CEE_LDELEM_I1:
		case CEE_LDELEM_U1:
		case CEE_LDELEM_I2:
		case CEE_LDELEM_U2:
		case CEE_LDELEM_I4:
		case CEE_LDELEM_U4:
		case CEE_LDELEM_I8:
		case CEE_LDELEM_I:
		case CEE_LDELEM_R4:
		case CEE_LDELEM_R8:
		case CEE_LDELEM_REF: {
			MonoInst *addr;

			CHECK_STACK (2);
			sp -= 2;

			if (*ip == CEE_LDELEM_ANY) {
				CHECK_OPSIZE (5);
				token = read32 (ip + 1);
				klass = mini_get_class (method, token, generic_context);
				CHECK_TYPELOAD (klass);
				mono_class_init (klass);
			}
			else
				klass = array_access_to_klass (*ip);

			if (sp [0]->type != STACK_OBJ)
				UNVERIFIED;

			cfg->flags |= MONO_CFG_HAS_LDELEMA;

			if (sp [1]->opcode == OP_ICONST) {
				int array_reg = sp [0]->dreg;
				int index_reg = sp [1]->dreg;
				int offset = (mono_class_array_element_size (klass) * sp [1]->inst_c0) + G_STRUCT_OFFSET (MonoArray, vector);

				MONO_EMIT_BOUNDS_CHECK (cfg, array_reg, MonoArray, max_length, index_reg);
				EMIT_NEW_LOAD_MEMBASE_TYPE (cfg, ins, &klass->byval_arg, array_reg, offset);
			} else {
				addr = mini_emit_ldelema_1_ins (cfg, klass, sp [0], sp [1]);
				EMIT_NEW_LOAD_MEMBASE_TYPE (cfg, ins, &klass->byval_arg, addr->dreg, 0);
			}
			*sp++ = ins;
			if (*ip == CEE_LDELEM_ANY)
				ip += 5;
			else
				++ip;
			break;
		}
		case CEE_STELEM_I:
		case CEE_STELEM_I1:
		case CEE_STELEM_I2:
		case CEE_STELEM_I4:
		case CEE_STELEM_I8:
		case CEE_STELEM_R4:
		case CEE_STELEM_R8:
		case CEE_STELEM_REF:
		case CEE_STELEM_ANY: {
			MonoInst *addr;

			CHECK_STACK (3);
			sp -= 3;

			cfg->flags |= MONO_CFG_HAS_LDELEMA;

			if (*ip == CEE_STELEM_ANY) {
				CHECK_OPSIZE (5);
				token = read32 (ip + 1);
				klass = mini_get_class (method, token, generic_context);
				CHECK_TYPELOAD (klass);
				mono_class_init (klass);
			}
			else
				klass = array_access_to_klass (*ip);

			if (sp [0]->type != STACK_OBJ)
				UNVERIFIED;

			/* storing a NULL doesn't need any of the complex checks in stelemref */
			if (generic_class_is_reference_type (cfg, klass) &&
				!(sp [2]->opcode == OP_PCONST && sp [2]->inst_p0 == NULL)) {
				MonoMethod* helper = mono_marshal_get_stelemref ();
				MonoInst *iargs [3];

				if (sp [0]->type != STACK_OBJ)
					UNVERIFIED;
				if (sp [2]->type != STACK_OBJ)
					UNVERIFIED;

				iargs [2] = sp [2];
				iargs [1] = sp [1];
				iargs [0] = sp [0];
				
				mono_emit_method_call (cfg, helper, iargs, NULL);
			} else {
				if (sp [1]->opcode == OP_ICONST) {
					int array_reg = sp [0]->dreg;
					int index_reg = sp [1]->dreg;
					int offset = (mono_class_array_element_size (klass) * sp [1]->inst_c0) + G_STRUCT_OFFSET (MonoArray, vector);

					MONO_EMIT_BOUNDS_CHECK (cfg, array_reg, MonoArray, max_length, index_reg);
					EMIT_NEW_STORE_MEMBASE_TYPE (cfg, ins, &klass->byval_arg, array_reg, offset, sp [2]->dreg);
				} else {
					addr = mini_emit_ldelema_1_ins (cfg, klass, sp [0], sp [1]);
					EMIT_NEW_STORE_MEMBASE_TYPE (cfg, ins, &klass->byval_arg, addr->dreg, 0, sp [2]->dreg);
				}
			}

			if (*ip == CEE_STELEM_ANY)
				ip += 5;
			else
				++ip;
			inline_costs += 1;
			break;
		}
		case CEE_CKFINITE: {
			CHECK_STACK (1);
			--sp;

			MONO_INST_NEW (cfg, ins, OP_CKFINITE);
			ins->sreg1 = sp [0]->dreg;
			ins->dreg = alloc_freg (cfg);
			ins->type = STACK_R8;
			MONO_ADD_INS (bblock, ins);
			*sp++ = ins;

			mono_decompose_opcode (cfg, ins);

			++ip;
			break;
		}
		case CEE_REFANYVAL: {
			MonoInst *src_var, *src;

			int klass_reg = alloc_preg (cfg);
			int dreg = alloc_preg (cfg);

			CHECK_STACK (1);
			MONO_INST_NEW (cfg, ins, *ip);
			--sp;
			CHECK_OPSIZE (5);
			klass = mono_class_get_full (image, read32 (ip + 1), generic_context);
			CHECK_TYPELOAD (klass);
			mono_class_init (klass);

			if (cfg->generic_sharing_context)
				context_used = mono_class_check_context_used (klass);

			// FIXME:
			src_var = get_vreg_to_inst (cfg, sp [0]->dreg);
			if (!src_var)
				src_var = mono_compile_create_var_for_vreg (cfg, &mono_defaults.typed_reference_class->byval_arg, OP_LOCAL, sp [0]->dreg);
			EMIT_NEW_VARLOADA (cfg, src, src_var, src_var->inst_vtype);
			MONO_EMIT_NEW_LOAD_MEMBASE (cfg, klass_reg, src->dreg, G_STRUCT_OFFSET (MonoTypedRef, klass));

			if (context_used) {
				MonoInst *klass_ins;

				klass_ins = emit_get_rgctx_klass (cfg, context_used,
						klass, MONO_RGCTX_INFO_KLASS);

				// FIXME:
				MONO_EMIT_NEW_BIALU (cfg, OP_COMPARE, -1, klass_reg, klass_ins->dreg);
				MONO_EMIT_NEW_COND_EXC (cfg, NE_UN, "InvalidCastException");
			} else {
				mini_emit_class_check (cfg, klass_reg, klass);
			}
			EMIT_NEW_LOAD_MEMBASE (cfg, ins, OP_LOAD_MEMBASE, dreg, src->dreg, G_STRUCT_OFFSET (MonoTypedRef, value));
			ins->type = STACK_MP;
			*sp++ = ins;
			ip += 5;
			break;
		}
		case CEE_MKREFANY: {
			MonoInst *loc, *addr;

			CHECK_STACK (1);
			MONO_INST_NEW (cfg, ins, *ip);
			--sp;
			CHECK_OPSIZE (5);
			klass = mono_class_get_full (image, read32 (ip + 1), generic_context);
			CHECK_TYPELOAD (klass);
			mono_class_init (klass);

			if (cfg->generic_sharing_context)
				context_used = mono_class_check_context_used (klass);

			loc = mono_compile_create_var (cfg, &mono_defaults.typed_reference_class->byval_arg, OP_LOCAL);
			EMIT_NEW_TEMPLOADA (cfg, addr, loc->inst_c0);

			if (context_used) {
				MonoInst *const_ins;
				int type_reg = alloc_preg (cfg);

				const_ins = emit_get_rgctx_klass (cfg, context_used, klass, MONO_RGCTX_INFO_KLASS);
				MONO_EMIT_NEW_STORE_MEMBASE (cfg, OP_STOREP_MEMBASE_REG, addr->dreg, G_STRUCT_OFFSET (MonoTypedRef, klass), const_ins->dreg);
				MONO_EMIT_NEW_BIALU_IMM (cfg, OP_ADD_IMM, type_reg, const_ins->dreg, G_STRUCT_OFFSET (MonoClass, byval_arg));
				MONO_EMIT_NEW_STORE_MEMBASE (cfg, OP_STOREP_MEMBASE_REG, addr->dreg, G_STRUCT_OFFSET (MonoTypedRef, type), type_reg);
			} else if (cfg->compile_aot) {
				int const_reg = alloc_preg (cfg);
				int type_reg = alloc_preg (cfg);

				MONO_EMIT_NEW_CLASSCONST (cfg, const_reg, klass);
				MONO_EMIT_NEW_STORE_MEMBASE (cfg, OP_STOREP_MEMBASE_REG, addr->dreg, G_STRUCT_OFFSET (MonoTypedRef, klass), const_reg);
				MONO_EMIT_NEW_BIALU_IMM (cfg, OP_ADD_IMM, type_reg, const_reg, G_STRUCT_OFFSET (MonoClass, byval_arg));
				MONO_EMIT_NEW_STORE_MEMBASE (cfg, OP_STOREP_MEMBASE_REG, addr->dreg, G_STRUCT_OFFSET (MonoTypedRef, type), type_reg);
			} else {
				MONO_EMIT_NEW_STORE_MEMBASE_IMM (cfg, OP_STOREP_MEMBASE_IMM, addr->dreg, G_STRUCT_OFFSET (MonoTypedRef, type), &klass->byval_arg);
				MONO_EMIT_NEW_STORE_MEMBASE_IMM (cfg, OP_STOREP_MEMBASE_IMM, addr->dreg, G_STRUCT_OFFSET (MonoTypedRef, klass), klass);
			}
			MONO_EMIT_NEW_STORE_MEMBASE (cfg, OP_STOREP_MEMBASE_REG, addr->dreg, G_STRUCT_OFFSET (MonoTypedRef, value), sp [0]->dreg);

			EMIT_NEW_TEMPLOAD (cfg, ins, loc->inst_c0);
			ins->type = STACK_VTYPE;
			ins->klass = mono_defaults.typed_reference_class;
			*sp++ = ins;
			ip += 5;
			break;
		}
		case CEE_LDTOKEN: {
			gpointer handle;
			MonoClass *handle_class;

			CHECK_STACK_OVF (1);

			CHECK_OPSIZE (5);
			n = read32 (ip + 1);

			if (method->wrapper_type == MONO_WRAPPER_DYNAMIC_METHOD ||
					method->wrapper_type == MONO_WRAPPER_SYNCHRONIZED) {
				handle = mono_method_get_wrapper_data (method, n);
				handle_class = mono_method_get_wrapper_data (method, n + 1);
				if (handle_class == mono_defaults.typehandle_class)
					handle = &((MonoClass*)handle)->byval_arg;
			}
			else {
				handle = mono_ldtoken (image, n, &handle_class, generic_context);
			}
			if (!handle)
				goto load_error;
			mono_class_init (handle_class);
			if (cfg->generic_sharing_context) {
				if (mono_metadata_token_table (n) == MONO_TABLE_TYPEDEF ||
						mono_metadata_token_table (n) == MONO_TABLE_TYPEREF) {
					/* This case handles ldtoken
					   of an open type, like for
					   typeof(Gen<>). */
					context_used = 0;
				} else if (handle_class == mono_defaults.typehandle_class) {
					/* If we get a MONO_TYPE_CLASS
					   then we need to provide the
					   open type, not an
					   instantiation of it. */
					if (mono_type_get_type (handle) == MONO_TYPE_CLASS)
						context_used = 0;
					else
						context_used = mono_class_check_context_used (mono_class_from_mono_type (handle));
				} else if (handle_class == mono_defaults.fieldhandle_class)
					context_used = mono_class_check_context_used (((MonoClassField*)handle)->parent);
				else if (handle_class == mono_defaults.methodhandle_class)
					context_used = mono_method_check_context_used (handle);
				else
					g_assert_not_reached ();
			}

			if ((cfg->opt & MONO_OPT_SHARED) &&
					method->wrapper_type != MONO_WRAPPER_DYNAMIC_METHOD &&
					method->wrapper_type != MONO_WRAPPER_SYNCHRONIZED) {
				MonoInst *addr, *vtvar, *iargs [3];
				int method_context_used;

				if (cfg->generic_sharing_context)
					method_context_used = mono_method_check_context_used (method);
				else
					method_context_used = 0;

				vtvar = mono_compile_create_var (cfg, &handle_class->byval_arg, OP_LOCAL); 

				EMIT_NEW_IMAGECONST (cfg, iargs [0], image);
				EMIT_NEW_ICONST (cfg, iargs [1], n);
				if (method_context_used) {
					iargs [2] = emit_get_rgctx_method (cfg, method_context_used,
						method, MONO_RGCTX_INFO_METHOD);
					ins = mono_emit_jit_icall (cfg, mono_ldtoken_wrapper_generic_shared, iargs);
				} else {
					EMIT_NEW_PCONST (cfg, iargs [2], generic_context);
					ins = mono_emit_jit_icall (cfg, mono_ldtoken_wrapper, iargs);
				}
				EMIT_NEW_TEMPLOADA (cfg, addr, vtvar->inst_c0);

				MONO_EMIT_NEW_STORE_MEMBASE (cfg, OP_STORE_MEMBASE_REG, addr->dreg, 0, ins->dreg);

				EMIT_NEW_TEMPLOAD (cfg, ins, vtvar->inst_c0);
			} else {
				if ((ip + 5 < end) && ip_in_bb (cfg, bblock, ip + 5) && 
					((ip [5] == CEE_CALL) || (ip [5] == CEE_CALLVIRT)) && 
					(cmethod = mini_get_method (cfg, method, read32 (ip + 6), NULL, generic_context)) &&
					(cmethod->klass == mono_defaults.monotype_class->parent) &&
					(strcmp (cmethod->name, "GetTypeFromHandle") == 0)) {
					MonoClass *tclass = mono_class_from_mono_type (handle);

					mono_class_init (tclass);
					if (context_used) {
						ins = emit_get_rgctx_klass (cfg, context_used,
							tclass, MONO_RGCTX_INFO_REFLECTION_TYPE);
					} else if (cfg->compile_aot) {
						EMIT_NEW_TYPE_FROM_HANDLE_CONST (cfg, ins, image, n, generic_context);
					} else {
						EMIT_NEW_PCONST (cfg, ins, mono_type_get_object (cfg->domain, handle));
					}
					ins->type = STACK_OBJ;
					ins->klass = cmethod->klass;
					ip += 5;
				} else {
					MonoInst *addr, *vtvar;

					vtvar = mono_compile_create_var (cfg, &handle_class->byval_arg, OP_LOCAL);

					if (context_used) {
						if (handle_class == mono_defaults.typehandle_class) {
							ins = emit_get_rgctx_klass (cfg, context_used,
									mono_class_from_mono_type (handle),
									MONO_RGCTX_INFO_TYPE);
						} else if (handle_class == mono_defaults.methodhandle_class) {
							ins = emit_get_rgctx_method (cfg, context_used,
									handle, MONO_RGCTX_INFO_METHOD);
						} else if (handle_class == mono_defaults.fieldhandle_class) {
							ins = emit_get_rgctx_field (cfg, context_used,
									handle, MONO_RGCTX_INFO_CLASS_FIELD);
						} else {
							g_assert_not_reached ();
						}
					} else if (cfg->compile_aot) {
						EMIT_NEW_LDTOKENCONST (cfg, ins, image, n);
					} else {
						EMIT_NEW_PCONST (cfg, ins, handle);
					}
					EMIT_NEW_TEMPLOADA (cfg, addr, vtvar->inst_c0);
					MONO_EMIT_NEW_STORE_MEMBASE (cfg, OP_STORE_MEMBASE_REG, addr->dreg, 0, ins->dreg);
					EMIT_NEW_TEMPLOAD (cfg, ins, vtvar->inst_c0);
				}
			}

			*sp++ = ins;
			ip += 5;
			break;
		}
		case CEE_THROW:
			CHECK_STACK (1);
			MONO_INST_NEW (cfg, ins, OP_THROW);
			--sp;
			ins->sreg1 = sp [0]->dreg;
			ip++;
			bblock->out_of_line = TRUE;
			MONO_ADD_INS (bblock, ins);
			MONO_INST_NEW (cfg, ins, OP_NOT_REACHED);
			MONO_ADD_INS (bblock, ins);
			sp = stack_start;
			
			link_bblock (cfg, bblock, end_bblock);
			start_new_bblock = 1;
			break;
		case CEE_ENDFINALLY:
			MONO_INST_NEW (cfg, ins, OP_ENDFINALLY);
			MONO_ADD_INS (bblock, ins);
			ip++;
			start_new_bblock = 1;

			/*
			 * Control will leave the method so empty the stack, otherwise
			 * the next basic block will start with a nonempty stack.
			 */
			while (sp != stack_start) {
				sp--;
			}
			break;
		case CEE_LEAVE:
		case CEE_LEAVE_S: {
			GList *handlers;

			if (*ip == CEE_LEAVE) {
				CHECK_OPSIZE (5);
				target = ip + 5 + (gint32)read32(ip + 1);
			} else {
				CHECK_OPSIZE (2);
				target = ip + 2 + (signed char)(ip [1]);
			}

			/* empty the stack */
			while (sp != stack_start) {
				sp--;
			}

			/* 
			 * If this leave statement is in a catch block, check for a
			 * pending exception, and rethrow it if necessary.
			 */
			for (i = 0; i < header->num_clauses; ++i) {
				MonoExceptionClause *clause = &header->clauses [i];

				/* 
				 * Use <= in the final comparison to handle clauses with multiple
				 * leave statements, like in bug #78024.
				 * The ordering of the exception clauses guarantees that we find the
				 * innermost clause.
				 */
				if (MONO_OFFSET_IN_HANDLER (clause, ip - header->code) && (clause->flags == MONO_EXCEPTION_CLAUSE_NONE) && (ip - header->code + ((*ip == CEE_LEAVE) ? 5 : 2)) <= (clause->handler_offset + clause->handler_len)) {
					MonoInst *exc_ins;
					MonoBasicBlock *dont_throw;

					/*
					  MonoInst *load;

					  NEW_TEMPLOAD (cfg, load, mono_find_exvar_for_offset (cfg, clause->handler_offset)->inst_c0);
					*/

					exc_ins = mono_emit_jit_icall (cfg, mono_thread_get_undeniable_exception, NULL);

					NEW_BBLOCK (cfg, dont_throw);

					/*
					 * Currently, we allways rethrow the abort exception, despite the 
					 * fact that this is not correct. See thread6.cs for an example. 
					 * But propagating the abort exception is more important than 
					 * getting the sematics right.
					 */
					MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, exc_ins->dreg, 0);
					MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_PBEQ, dont_throw);
					MONO_EMIT_NEW_UNALU (cfg, OP_THROW, -1, exc_ins->dreg);

					MONO_START_BB (cfg, dont_throw);
					bblock = cfg->cbb;
				}
			}

			if ((handlers = mono_find_final_block (cfg, ip, target, MONO_EXCEPTION_CLAUSE_FINALLY))) {
				GList *tmp;
				for (tmp = handlers; tmp; tmp = tmp->next) {
					tblock = tmp->data;
					link_bblock (cfg, bblock, tblock);
					MONO_INST_NEW (cfg, ins, OP_CALL_HANDLER);
					ins->inst_target_bb = tblock;
					MONO_ADD_INS (bblock, ins);
				}
				g_list_free (handlers);
			} 

			MONO_INST_NEW (cfg, ins, OP_BR);
			MONO_ADD_INS (bblock, ins);
			GET_BBLOCK (cfg, tblock, target);
			link_bblock (cfg, bblock, tblock);
			ins->inst_target_bb = tblock;
			start_new_bblock = 1;

			if (*ip == CEE_LEAVE)
				ip += 5;
			else
				ip += 2;

			break;
		}

			/*
			 * Mono specific opcodes
			 */
		case MONO_CUSTOM_PREFIX: {

			g_assert (method->wrapper_type != MONO_WRAPPER_NONE);

			CHECK_OPSIZE (2);
			switch (ip [1]) {
			case CEE_MONO_ICALL: {
				gpointer func;
				MonoJitICallInfo *info;

				token = read32 (ip + 2);
				func = mono_method_get_wrapper_data (method, token);
				info = mono_find_jit_icall_by_addr (func);
				g_assert (info);

				CHECK_STACK (info->sig->param_count);
				sp -= info->sig->param_count;

				ins = mono_emit_jit_icall (cfg, info->func, sp);
				if (!MONO_TYPE_IS_VOID (info->sig->ret))
					*sp++ = ins;

				ip += 6;
				inline_costs += 10 * num_calls++;

				break;
			}
			case CEE_MONO_LDPTR: {
				gpointer ptr;

				CHECK_STACK_OVF (1);
				CHECK_OPSIZE (6);
				token = read32 (ip + 2);

				ptr = mono_method_get_wrapper_data (method, token);
				if (cfg->compile_aot && (method->wrapper_type == MONO_WRAPPER_MANAGED_TO_NATIVE) && (strstr (method->name, "__icall_wrapper_") == method->name)) {
					MonoJitICallInfo *callinfo;
					const char *icall_name;

					icall_name = method->name + strlen ("__icall_wrapper_");
					g_assert (icall_name);
					callinfo = mono_find_jit_icall_by_name (icall_name);
					g_assert (callinfo);
						
					if (ptr == callinfo->func) {
						/* Will be transformed into an AOTCONST later */
						EMIT_NEW_PCONST (cfg, ins, ptr);
						*sp++ = ins;
						ip += 6;
						break;
					}
				}
				/* FIXME: Generalize this */
				if (cfg->compile_aot && ptr == mono_thread_interruption_request_flag ()) {
					EMIT_NEW_AOTCONST (cfg, ins, MONO_PATCH_INFO_INTERRUPTION_REQUEST_FLAG, NULL);
					*sp++ = ins;
					ip += 6;
					break;
				}
				EMIT_NEW_PCONST (cfg, ins, ptr);
				*sp++ = ins;
				ip += 6;
				inline_costs += 10 * num_calls++;
				/* Can't embed random pointers into AOT code */
				cfg->disable_aot = 1;
				break;
			}
			case CEE_MONO_ICALL_ADDR: {
				MonoMethod *cmethod;
				gpointer ptr;

				CHECK_STACK_OVF (1);
				CHECK_OPSIZE (6);
				token = read32 (ip + 2);

				cmethod = mono_method_get_wrapper_data (method, token);

				if (cfg->compile_aot) {
					EMIT_NEW_AOTCONST (cfg, ins, MONO_PATCH_INFO_ICALL_ADDR, cmethod);
				} else {
					ptr = mono_lookup_internal_call (cmethod);
					g_assert (ptr);
					EMIT_NEW_PCONST (cfg, ins, ptr);
				}
				*sp++ = ins;
				ip += 6;
				break;
			}
			case CEE_MONO_VTADDR: {
				MonoInst *src_var, *src;

				CHECK_STACK (1);
				--sp;

				// FIXME:
				src_var = get_vreg_to_inst (cfg, sp [0]->dreg);
				EMIT_NEW_VARLOADA ((cfg), (src), src_var, src_var->inst_vtype);
				*sp++ = src;
				ip += 2;
				break;
			}
			case CEE_MONO_NEWOBJ: {
				MonoInst *iargs [2];

				CHECK_STACK_OVF (1);
				CHECK_OPSIZE (6);
				token = read32 (ip + 2);
				klass = (MonoClass *)mono_method_get_wrapper_data (method, token);
				mono_class_init (klass);
				NEW_DOMAINCONST (cfg, iargs [0]);
				MONO_ADD_INS (cfg->cbb, iargs [0]);
				NEW_CLASSCONST (cfg, iargs [1], klass);
				MONO_ADD_INS (cfg->cbb, iargs [1]);
				*sp++ = mono_emit_jit_icall (cfg, mono_object_new, iargs);
				ip += 6;
				inline_costs += 10 * num_calls++;
				break;
			}
			case CEE_MONO_OBJADDR:
				CHECK_STACK (1);
				--sp;
				MONO_INST_NEW (cfg, ins, OP_MOVE);
				ins->dreg = alloc_preg (cfg);
				ins->sreg1 = sp [0]->dreg;
				ins->type = STACK_MP;
				MONO_ADD_INS (cfg->cbb, ins);
				*sp++ = ins;
				ip += 2;
				break;
			case CEE_MONO_LDNATIVEOBJ:
				/*
				 * Similar to LDOBJ, but instead load the unmanaged 
				 * representation of the vtype to the stack.
				 */
				CHECK_STACK (1);
				CHECK_OPSIZE (6);
				--sp;
				token = read32 (ip + 2);
				klass = mono_method_get_wrapper_data (method, token);
				g_assert (klass->valuetype);
				mono_class_init (klass);

				{
					MonoInst *src, *dest, *temp;

					src = sp [0];
					temp = mono_compile_create_var (cfg, &klass->byval_arg, OP_LOCAL);
					temp->backend.is_pinvoke = 1;
					EMIT_NEW_TEMPLOADA (cfg, dest, temp->inst_c0);
					mini_emit_stobj (cfg, dest, src, klass, TRUE);

					EMIT_NEW_TEMPLOAD (cfg, dest, temp->inst_c0);
					dest->type = STACK_VTYPE;
					dest->klass = klass;

					*sp ++ = dest;
					ip += 6;
				}
				break;
			case CEE_MONO_RETOBJ: {
				/*
				 * Same as RET, but return the native representation of a vtype
				 * to the caller.
				 */
				g_assert (cfg->ret);
				g_assert (mono_method_signature (method)->pinvoke); 
				CHECK_STACK (1);
				--sp;
				
				CHECK_OPSIZE (6);
				token = read32 (ip + 2);    
				klass = (MonoClass *)mono_method_get_wrapper_data (method, token);

				if (!cfg->vret_addr) {
					g_assert (cfg->ret_var_is_local);

					EMIT_NEW_VARLOADA (cfg, ins, cfg->ret, cfg->ret->inst_vtype);
				} else {
					EMIT_NEW_RETLOADA (cfg, ins);
				}
				mini_emit_stobj (cfg, ins, sp [0], klass, TRUE);
				
				if (sp != stack_start)
					UNVERIFIED;
				
				MONO_INST_NEW (cfg, ins, OP_BR);
				ins->inst_target_bb = end_bblock;
				MONO_ADD_INS (bblock, ins);
				link_bblock (cfg, bblock, end_bblock);
				start_new_bblock = 1;
				ip += 6;
				break;
			}
			case CEE_MONO_CISINST:
			case CEE_MONO_CCASTCLASS: {
				int token;
				CHECK_STACK (1);
				--sp;
				CHECK_OPSIZE (6);
				token = read32 (ip + 2);
				klass = (MonoClass *)mono_method_get_wrapper_data (method, token);
				if (ip [1] == CEE_MONO_CISINST)
					ins = handle_cisinst (cfg, klass, sp [0]);
				else
					ins = handle_ccastclass (cfg, klass, sp [0]);
				bblock = cfg->cbb;
				*sp++ = ins;
				ip += 6;
				break;
			}
			case CEE_MONO_SAVE_LMF:
			case CEE_MONO_RESTORE_LMF:
#ifdef MONO_ARCH_HAVE_LMF_OPS
				MONO_INST_NEW (cfg, ins, (ip [1] == CEE_MONO_SAVE_LMF) ? OP_SAVE_LMF : OP_RESTORE_LMF);
				MONO_ADD_INS (bblock, ins);
				cfg->need_lmf_area = TRUE;
#endif
				ip += 2;
				break;
			case CEE_MONO_CLASSCONST:
				CHECK_STACK_OVF (1);
				CHECK_OPSIZE (6);
				token = read32 (ip + 2);
				EMIT_NEW_CLASSCONST (cfg, ins, mono_method_get_wrapper_data (method, token));
				*sp++ = ins;
				ip += 6;
				inline_costs += 10 * num_calls++;
				break;
			case CEE_MONO_NOT_TAKEN:
				bblock->out_of_line = TRUE;
				ip += 2;
				break;
			case CEE_MONO_TLS:
				CHECK_STACK_OVF (1);
				CHECK_OPSIZE (6);
				MONO_INST_NEW (cfg, ins, OP_TLS_GET);
				ins->dreg = alloc_preg (cfg);
				ins->inst_offset = (gint32)read32 (ip + 2);
				ins->type = STACK_PTR;
				MONO_ADD_INS (bblock, ins);
				*sp++ = ins;
				ip += 6;
				break;
			default:
				g_error ("opcode 0x%02x 0x%02x not handled", MONO_CUSTOM_PREFIX, ip [1]);
				break;
			}
			break;
		}

		case CEE_PREFIX1: {
			CHECK_OPSIZE (2);
			switch (ip [1]) {
			case CEE_ARGLIST: {
				/* somewhat similar to LDTOKEN */
				MonoInst *addr, *vtvar;
				CHECK_STACK_OVF (1);
				vtvar = mono_compile_create_var (cfg, &mono_defaults.argumenthandle_class->byval_arg, OP_LOCAL); 

				EMIT_NEW_TEMPLOADA (cfg, addr, vtvar->inst_c0);
				EMIT_NEW_UNALU (cfg, ins, OP_ARGLIST, -1, addr->dreg);

				EMIT_NEW_TEMPLOAD (cfg, ins, vtvar->inst_c0);
				ins->type = STACK_VTYPE;
				ins->klass = mono_defaults.argumenthandle_class;
				*sp++ = ins;
				ip += 2;
				break;
			}
			case CEE_CEQ:
			case CEE_CGT:
			case CEE_CGT_UN:
			case CEE_CLT:
			case CEE_CLT_UN: {
				MonoInst *cmp;
				CHECK_STACK (2);
				/*
				 * The following transforms:
				 *    CEE_CEQ    into OP_CEQ
				 *    CEE_CGT    into OP_CGT
				 *    CEE_CGT_UN into OP_CGT_UN
				 *    CEE_CLT    into OP_CLT
				 *    CEE_CLT_UN into OP_CLT_UN
				 */
				MONO_INST_NEW (cfg, cmp, (OP_CEQ - CEE_CEQ) + ip [1]);
				
				MONO_INST_NEW (cfg, ins, cmp->opcode);
				sp -= 2;
				cmp->sreg1 = sp [0]->dreg;
				cmp->sreg2 = sp [1]->dreg;
				type_from_op (cmp, sp [0], sp [1]);
				CHECK_TYPE (cmp);
				if ((sp [0]->type == STACK_I8) || ((SIZEOF_REGISTER == 8) && ((sp [0]->type == STACK_PTR) || (sp [0]->type == STACK_OBJ) || (sp [0]->type == STACK_MP))))
					cmp->opcode = OP_LCOMPARE;
				else if (sp [0]->type == STACK_R8)
					cmp->opcode = OP_FCOMPARE;
				else
					cmp->opcode = OP_ICOMPARE;
				MONO_ADD_INS (bblock, cmp);
				ins->type = STACK_I4;
				ins->dreg = alloc_dreg (cfg, ins->type);
				type_from_op (ins, sp [0], sp [1]);

				if (cmp->opcode == OP_FCOMPARE) {
					/*
					 * The backends expect the fceq opcodes to do the
					 * comparison too.
					 */
					cmp->opcode = OP_NOP;
					ins->sreg1 = cmp->sreg1;
					ins->sreg2 = cmp->sreg2;
				}
				MONO_ADD_INS (bblock, ins);
				*sp++ = ins;
				ip += 2;
				break;
			}
			case CEE_LDFTN: {
				MonoInst *argconst;
				MonoMethod *cil_method;
				gboolean needs_static_rgctx_invoke;

				CHECK_STACK_OVF (1);
				CHECK_OPSIZE (6);
				n = read32 (ip + 2);
				cmethod = mini_get_method (cfg, method, n, NULL, generic_context);
				if (!cmethod)
					goto load_error;
				mono_class_init (cmethod->klass);

				mono_save_token_info (cfg, image, n, cmethod);

				if (cfg->generic_sharing_context)
					context_used = mono_method_check_context_used (cmethod);

				needs_static_rgctx_invoke = mono_method_needs_static_rgctx_invoke (cmethod, TRUE);
 
				cil_method = cmethod;
				if (!dont_verify && !cfg->skip_visibility && !mono_method_can_access_method (method, cmethod))
					METHOD_ACCESS_FAILURE;

				if (mono_security_get_mode () == MONO_SECURITY_MODE_CAS) {
					if (check_linkdemand (cfg, method, cmethod))
						INLINE_FAILURE;
					CHECK_CFG_EXCEPTION;
				} else if (mono_security_get_mode () == MONO_SECURITY_MODE_CORE_CLR) {
					ensure_method_is_allowed_to_call_method (cfg, method, cmethod, bblock, ip);
 				}

				/* 
				 * Optimize the common case of ldftn+delegate creation
				 */
#if defined(MONO_ARCH_HAVE_CREATE_DELEGATE_TRAMPOLINE) && !defined(HAVE_WRITE_BARRIERS)
				/* FIXME: SGEN support */
				/* FIXME: handle shared static generic methods */
				/* FIXME: handle this in shared code */
				if (!needs_static_rgctx_invoke && !context_used && (sp > stack_start) && (ip + 6 + 5 < end) && ip_in_bb (cfg, bblock, ip + 6) && (ip [6] == CEE_NEWOBJ)) {
					MonoMethod *ctor_method = mini_get_method (cfg, method, read32 (ip + 7), NULL, generic_context);
					if (ctor_method && (ctor_method->klass->parent == mono_defaults.multicastdelegate_class)) {
						MonoInst *target_ins;

						ip += 6;
						if (cfg->verbose_level > 3)
							g_print ("converting (in B%d: stack: %d) %s", bblock->block_num, (int)(sp - stack_start), mono_disasm_code_one (NULL, method, ip, NULL));
						target_ins = sp [-1];
						sp --;
						*sp = handle_delegate_ctor (cfg, ctor_method->klass, target_ins, cmethod);
						ip += 5;			
						sp ++;
						break;
					}
				}
#endif

				if (context_used) {
					if (needs_static_rgctx_invoke)
						cmethod = mono_marshal_get_static_rgctx_invoke (cmethod);

					argconst = emit_get_rgctx_method (cfg, context_used, cmethod, MONO_RGCTX_INFO_METHOD);
				} else if (needs_static_rgctx_invoke) {
					EMIT_NEW_METHODCONST (cfg, argconst, mono_marshal_get_static_rgctx_invoke (cmethod));
				} else {
					EMIT_NEW_METHODCONST (cfg, argconst, cmethod);
				}
				ins = mono_emit_jit_icall (cfg, mono_ldftn, &argconst);
				*sp++ = ins;
				
				ip += 6;
				inline_costs += 10 * num_calls++;
				break;
			}
			case CEE_LDVIRTFTN: {
				MonoInst *args [2];

				CHECK_STACK (1);
				CHECK_OPSIZE (6);
				n = read32 (ip + 2);
				cmethod = mini_get_method (cfg, method, n, NULL, generic_context);
				if (!cmethod)
					goto load_error;
				mono_class_init (cmethod->klass);
 
				if (cfg->generic_sharing_context)
					context_used = mono_method_check_context_used (cmethod);

				if (mono_security_get_mode () == MONO_SECURITY_MODE_CAS) {
					if (check_linkdemand (cfg, method, cmethod))
						INLINE_FAILURE;
					CHECK_CFG_EXCEPTION;
				} else if (mono_security_get_mode () == MONO_SECURITY_MODE_CORE_CLR) {
					ensure_method_is_allowed_to_call_method (cfg, method, cmethod, bblock, ip);
				}

				--sp;
				args [0] = *sp;

				if (context_used) {
					args [1] = emit_get_rgctx_method (cfg, context_used,
						cmethod, MONO_RGCTX_INFO_METHOD);
					*sp++ = mono_emit_jit_icall (cfg, mono_ldvirtfn_gshared, args);
				} else {
					EMIT_NEW_METHODCONST (cfg, args [1], cmethod);
					*sp++ = mono_emit_jit_icall (cfg, mono_ldvirtfn, args);
				}

				ip += 6;
				inline_costs += 10 * num_calls++;
				break;
			}
			case CEE_LDARG:
				CHECK_STACK_OVF (1);
				CHECK_OPSIZE (4);
				n = read16 (ip + 2);
				CHECK_ARG (n);
				EMIT_NEW_ARGLOAD (cfg, ins, n);
				*sp++ = ins;
				ip += 4;
				break;
			case CEE_LDARGA:
				CHECK_STACK_OVF (1);
				CHECK_OPSIZE (4);
				n = read16 (ip + 2);
				CHECK_ARG (n);
				NEW_ARGLOADA (cfg, ins, n);
				MONO_ADD_INS (cfg->cbb, ins);
				*sp++ = ins;
				ip += 4;
				break;
			case CEE_STARG:
				CHECK_STACK (1);
				--sp;
				CHECK_OPSIZE (4);
				n = read16 (ip + 2);
				CHECK_ARG (n);
				if (!dont_verify_stloc && target_type_is_incompatible (cfg, param_types [n], *sp))
					UNVERIFIED;
				EMIT_NEW_ARGSTORE (cfg, ins, n, *sp);
				ip += 4;
				break;
			case CEE_LDLOC:
				CHECK_STACK_OVF (1);
				CHECK_OPSIZE (4);
				n = read16 (ip + 2);
				CHECK_LOCAL (n);
				EMIT_NEW_LOCLOAD (cfg, ins, n);
				*sp++ = ins;
				ip += 4;
				break;
			case CEE_LDLOCA: {
				unsigned char *tmp_ip;
				CHECK_STACK_OVF (1);
				CHECK_OPSIZE (4);
				n = read16 (ip + 2);
				CHECK_LOCAL (n);

				if ((tmp_ip = emit_optimized_ldloca_ir (cfg, ip, end, 2))) {
					ip = tmp_ip;
					inline_costs += 1;
					break;
				}			
				
				EMIT_NEW_LOCLOADA (cfg, ins, n);
				*sp++ = ins;
				ip += 4;
				break;
			}
			case CEE_STLOC:
				CHECK_STACK (1);
				--sp;
				CHECK_OPSIZE (4);
				n = read16 (ip + 2);
				CHECK_LOCAL (n);
				if (!dont_verify_stloc && target_type_is_incompatible (cfg, header->locals [n], *sp))
					UNVERIFIED;
				emit_stloc_ir (cfg, sp, header, n);
				ip += 4;
				inline_costs += 1;
				break;
			case CEE_LOCALLOC:
				CHECK_STACK (1);
				--sp;
				if (sp != stack_start) 
					UNVERIFIED;
				if (cfg->method != method) 
					/* 
					 * Inlining this into a loop in a parent could lead to 
					 * stack overflows which is different behavior than the
					 * non-inlined case, thus disable inlining in this case.
					 */
					goto inline_failure;

				MONO_INST_NEW (cfg, ins, OP_LOCALLOC);
				ins->dreg = alloc_preg (cfg);
				ins->sreg1 = sp [0]->dreg;
				ins->type = STACK_PTR;
				MONO_ADD_INS (cfg->cbb, ins);

				cfg->flags |= MONO_CFG_HAS_ALLOCA;
				if (init_locals)
					ins->flags |= MONO_INST_INIT;

				*sp++ = ins;
				ip += 2;
				break;
			case CEE_ENDFILTER: {
				MonoExceptionClause *clause, *nearest;
				int cc, nearest_num;

				CHECK_STACK (1);
				--sp;
				if ((sp != stack_start) || (sp [0]->type != STACK_I4)) 
					UNVERIFIED;
				MONO_INST_NEW (cfg, ins, OP_ENDFILTER);
				ins->sreg1 = (*sp)->dreg;
				MONO_ADD_INS (bblock, ins);
				start_new_bblock = 1;
				ip += 2;

				nearest = NULL;
				nearest_num = 0;
				for (cc = 0; cc < header->num_clauses; ++cc) {
					clause = &header->clauses [cc];
					if ((clause->flags & MONO_EXCEPTION_CLAUSE_FILTER) &&
						((ip - header->code) > clause->data.filter_offset && (ip - header->code) <= clause->handler_offset) &&
					    (!nearest || (clause->data.filter_offset < nearest->data.filter_offset))) {
						nearest = clause;
						nearest_num = cc;
					}
				}
				g_assert (nearest);
				if ((ip - header->code) != nearest->handler_offset)
					UNVERIFIED;

				break;
			}
			case CEE_UNALIGNED_:
				ins_flag |= MONO_INST_UNALIGNED;
				/* FIXME: record alignment? we can assume 1 for now */
				CHECK_OPSIZE (3);
				ip += 3;
				break;
			case CEE_VOLATILE_:
				ins_flag |= MONO_INST_VOLATILE;
				ip += 2;
				break;
			case CEE_TAIL_:
				ins_flag   |= MONO_INST_TAILCALL;
				cfg->flags |= MONO_CFG_HAS_TAIL;
				/* Can't inline tail calls at this time */
				inline_costs += 100000;
				ip += 2;
				break;
			case CEE_INITOBJ:
				CHECK_STACK (1);
				--sp;
				CHECK_OPSIZE (6);
				token = read32 (ip + 2);
				klass = mini_get_class (method, token, generic_context);
				CHECK_TYPELOAD (klass);
				if (generic_class_is_reference_type (cfg, klass))
					MONO_EMIT_NEW_STORE_MEMBASE_IMM (cfg, OP_STORE_MEMBASE_IMM, sp [0]->dreg, 0, 0);
				else
					mini_emit_initobj (cfg, *sp, NULL, klass);
				ip += 6;
				inline_costs += 1;
				break;
			case CEE_CONSTRAINED_:
				CHECK_OPSIZE (6);
				token = read32 (ip + 2);
				constrained_call = mono_class_get_full (image, token, generic_context);
				CHECK_TYPELOAD (constrained_call);
				ip += 6;
				break;
			case CEE_CPBLK:
			case CEE_INITBLK: {
				MonoInst *iargs [3];
				CHECK_STACK (3);
				sp -= 3;

				if ((ip [1] == CEE_CPBLK) && (cfg->opt & MONO_OPT_INTRINS) && (sp [2]->opcode == OP_ICONST) && ((n = sp [2]->inst_c0) <= sizeof (gpointer) * 5)) {
					mini_emit_memcpy (cfg, sp [0]->dreg, 0, sp [1]->dreg, 0, sp [2]->inst_c0, 0);
				} else if ((ip [1] == CEE_INITBLK) && (cfg->opt & MONO_OPT_INTRINS) && (sp [2]->opcode == OP_ICONST) && ((n = sp [2]->inst_c0) <= sizeof (gpointer) * 5) && (sp [1]->opcode == OP_ICONST) && (sp [1]->inst_c0 == 0)) {
					/* emit_memset only works when val == 0 */
					mini_emit_memset (cfg, sp [0]->dreg, 0, sp [2]->inst_c0, sp [1]->inst_c0, 0);
				} else {
					iargs [0] = sp [0];
					iargs [1] = sp [1];
					iargs [2] = sp [2];
					if (ip [1] == CEE_CPBLK) {
						MonoMethod *memcpy_method = get_memcpy_method ();
						mono_emit_method_call (cfg, memcpy_method, iargs, NULL);
					} else {
						MonoMethod *memset_method = get_memset_method ();
						mono_emit_method_call (cfg, memset_method, iargs, NULL);
					}
				}
				ip += 2;
				inline_costs += 1;
				break;
			}
			case CEE_NO_:
				CHECK_OPSIZE (3);
				if (ip [2] & 0x1)
					ins_flag |= MONO_INST_NOTYPECHECK;
				if (ip [2] & 0x2)
					ins_flag |= MONO_INST_NORANGECHECK;
				/* we ignore the no-nullcheck for now since we
				 * really do it explicitly only when doing callvirt->call
				 */
				ip += 3;
				break;
			case CEE_RETHROW: {
				MonoInst *load;
				int handler_offset = -1;

				for (i = 0; i < header->num_clauses; ++i) {
					MonoExceptionClause *clause = &header->clauses [i];
					if (MONO_OFFSET_IN_HANDLER (clause, ip - header->code) && !(clause->flags & MONO_EXCEPTION_CLAUSE_FINALLY)) {
						handler_offset = clause->handler_offset;
						break;
					}
				}

				bblock->flags |= BB_EXCEPTION_UNSAFE;

				g_assert (handler_offset != -1);

				EMIT_NEW_TEMPLOAD (cfg, load, mono_find_exvar_for_offset (cfg, handler_offset)->inst_c0);
				MONO_INST_NEW (cfg, ins, OP_RETHROW);
				ins->sreg1 = load->dreg;
				MONO_ADD_INS (bblock, ins);
				sp = stack_start;
				link_bblock (cfg, bblock, end_bblock);
				start_new_bblock = 1;
				ip += 2;
				break;
			}
			case CEE_SIZEOF: {
				guint32 align;
				int ialign;

				CHECK_STACK_OVF (1);
				CHECK_OPSIZE (6);
				token = read32 (ip + 2);
				if (mono_metadata_token_table (token) == MONO_TABLE_TYPESPEC) {
					MonoType *type = mono_type_create_from_typespec (image, token);
					token = mono_type_size (type, &ialign);
				} else {
					MonoClass *klass = mono_class_get_full (image, token, generic_context);
					CHECK_TYPELOAD (klass);
					mono_class_init (klass);
					token = mono_class_value_size (klass, &align);
				}
				EMIT_NEW_ICONST (cfg, ins, token);
				*sp++= ins;
				ip += 6;
				break;
			}
			case CEE_REFANYTYPE: {
				MonoInst *src_var, *src;

				CHECK_STACK (1);
				--sp;

				// FIXME:
				src_var = get_vreg_to_inst (cfg, sp [0]->dreg);
				if (!src_var)
					src_var = mono_compile_create_var_for_vreg (cfg, &mono_defaults.typed_reference_class->byval_arg, OP_LOCAL, sp [0]->dreg);
				EMIT_NEW_VARLOADA (cfg, src, src_var, src_var->inst_vtype);
				EMIT_NEW_LOAD_MEMBASE_TYPE (cfg, ins, &mono_defaults.typehandle_class->byval_arg, src->dreg, G_STRUCT_OFFSET (MonoTypedRef, type));
				*sp++ = ins;
				ip += 2;
				break;
			}
			case CEE_READONLY_:
				readonly = TRUE;
				ip += 2;
				break;
			default:
				g_error ("opcode 0xfe 0x%02x not handled", ip [1]);
			}
			break;
		}
		default:
			g_error ("opcode 0x%02x not handled", *ip);
		}
	}
	if (start_new_bblock != 1)
		UNVERIFIED;

	bblock->cil_length = ip - bblock->cil_code;
	bblock->next_bb = end_bblock;

	if (cfg->method == method && cfg->domainvar) {
		MonoInst *store;
		MonoInst *get_domain;

		cfg->cbb = init_localsbb;

		if (! (get_domain = mono_arch_get_domain_intrinsic (cfg))) {
			get_domain = mono_emit_jit_icall (cfg, mono_domain_get, NULL);
		}
		else {
			get_domain->dreg = alloc_preg (cfg);
			MONO_ADD_INS (cfg->cbb, get_domain);
		}		
		NEW_TEMPSTORE (cfg, store, cfg->domainvar->inst_c0, get_domain);
		MONO_ADD_INS (cfg->cbb, store);
	}

	if (cfg->method == method && cfg->got_var)
		mono_emit_load_got_addr (cfg);

	if (init_locals) {
		MonoInst *store;

		cfg->cbb = init_localsbb;
		cfg->ip = NULL;
		for (i = 0; i < header->num_locals; ++i) {
			MonoType *ptype = header->locals [i];
			int t = ptype->type;
			dreg = cfg->locals [i]->dreg;

			if (t == MONO_TYPE_VALUETYPE && ptype->data.klass->enumtype)
				t = ptype->data.klass->enum_basetype->type;
			if (ptype->byref) {
				MONO_EMIT_NEW_PCONST (cfg, dreg, NULL);
			} else if (t >= MONO_TYPE_BOOLEAN && t <= MONO_TYPE_U4) {
				MONO_EMIT_NEW_ICONST (cfg, cfg->locals [i]->dreg, 0);
			} else if (t == MONO_TYPE_I8 || t == MONO_TYPE_U8) {
				MONO_EMIT_NEW_I8CONST (cfg, cfg->locals [i]->dreg, 0);
			} else if (t == MONO_TYPE_R4 || t == MONO_TYPE_R8) {
				MONO_INST_NEW (cfg, ins, OP_R8CONST);
				ins->type = STACK_R8;
				ins->inst_p0 = (void*)&r8_0;
				ins->dreg = alloc_dreg (cfg, STACK_R8);
				MONO_ADD_INS (init_localsbb, ins);
				EMIT_NEW_LOCSTORE (cfg, store, i, ins);
			} else if ((t == MONO_TYPE_VALUETYPE) || (t == MONO_TYPE_TYPEDBYREF) ||
				   ((t == MONO_TYPE_GENERICINST) && mono_type_generic_inst_is_valuetype (ptype))) {
				MONO_EMIT_NEW_VZERO (cfg, dreg, mono_class_from_mono_type (ptype));
			} else {
				MONO_EMIT_NEW_PCONST (cfg, dreg, NULL);
			}
		}
	}

	cfg->ip = NULL;

	if (cfg->method == method) {
		MonoBasicBlock *bb;
		for (bb = cfg->bb_entry; bb; bb = bb->next_bb) {
			bb->region = mono_find_block_region (cfg, bb->real_offset);
			if (cfg->spvars)
				mono_create_spvar_for_region (cfg, bb->region);
			if (cfg->verbose_level > 2)
				printf ("REGION BB%d IL_%04x ID_%08X\n", bb->block_num, bb->real_offset, bb->region);
		}
	}

	g_slist_free (class_inits);
	dont_inline = g_list_remove (dont_inline, method);

	if (inline_costs < 0) {
		char *mname;

		/* Method is too large */
		mname = mono_method_full_name (method, TRUE);
		cfg->exception_type = MONO_EXCEPTION_INVALID_PROGRAM;
		cfg->exception_message = g_strdup_printf ("Method %s is too complex.", mname);
		g_free (mname);
		return -1;
	}

	if ((cfg->verbose_level > 2) && (cfg->method == method)) 
		mono_print_code (cfg, "AFTER METHOD-TO-IR");

	return inline_costs;
 
 exception_exit:
	g_assert (cfg->exception_type != MONO_EXCEPTION_NONE);
	g_slist_free (class_inits);
	dont_inline = g_list_remove (dont_inline, method);
	return -1;

 inline_failure:
	g_slist_free (class_inits);
	dont_inline = g_list_remove (dont_inline, method);
	return -1;

 load_error:
	g_slist_free (class_inits);
	dont_inline = g_list_remove (dont_inline, method);
	cfg->exception_type = MONO_EXCEPTION_TYPE_LOAD;
	return -1;

 unverified:
	g_slist_free (class_inits);
	dont_inline = g_list_remove (dont_inline, method);
	set_exception_type_from_invalid_il (cfg, method, ip);
	return -1;
}

static int
store_membase_reg_to_store_membase_imm (int opcode)
{
	switch (opcode) {
	case OP_STORE_MEMBASE_REG:
		return OP_STORE_MEMBASE_IMM;
	case OP_STOREI1_MEMBASE_REG:
		return OP_STOREI1_MEMBASE_IMM;
	case OP_STOREI2_MEMBASE_REG:
		return OP_STOREI2_MEMBASE_IMM;
	case OP_STOREI4_MEMBASE_REG:
		return OP_STOREI4_MEMBASE_IMM;
	case OP_STOREI8_MEMBASE_REG:
		return OP_STOREI8_MEMBASE_IMM;
	default:
		g_assert_not_reached ();
	}

	return -1;
}		

#endif /* DISABLE_JIT */

int
mono_op_to_op_imm (int opcode)
{
	switch (opcode) {
	case OP_IADD:
		return OP_IADD_IMM;
	case OP_ISUB:
		return OP_ISUB_IMM;
	case OP_IDIV:
		return OP_IDIV_IMM;
	case OP_IDIV_UN:
		return OP_IDIV_UN_IMM;
	case OP_IREM:
		return OP_IREM_IMM;
	case OP_IREM_UN:
		return OP_IREM_UN_IMM;
	case OP_IMUL:
		return OP_IMUL_IMM;
	case OP_IAND:
		return OP_IAND_IMM;
	case OP_IOR:
		return OP_IOR_IMM;
	case OP_IXOR:
		return OP_IXOR_IMM;
	case OP_ISHL:
		return OP_ISHL_IMM;
	case OP_ISHR:
		return OP_ISHR_IMM;
	case OP_ISHR_UN:
		return OP_ISHR_UN_IMM;

	case OP_LADD:
		return OP_LADD_IMM;
	case OP_LSUB:
		return OP_LSUB_IMM;
	case OP_LAND:
		return OP_LAND_IMM;
	case OP_LOR:
		return OP_LOR_IMM;
	case OP_LXOR:
		return OP_LXOR_IMM;
	case OP_LSHL:
		return OP_LSHL_IMM;
	case OP_LSHR:
		return OP_LSHR_IMM;
	case OP_LSHR_UN:
		return OP_LSHR_UN_IMM;		

	case OP_COMPARE:
		return OP_COMPARE_IMM;
	case OP_ICOMPARE:
		return OP_ICOMPARE_IMM;
	case OP_LCOMPARE:
		return OP_LCOMPARE_IMM;

	case OP_STORE_MEMBASE_REG:
		return OP_STORE_MEMBASE_IMM;
	case OP_STOREI1_MEMBASE_REG:
		return OP_STOREI1_MEMBASE_IMM;
	case OP_STOREI2_MEMBASE_REG:
		return OP_STOREI2_MEMBASE_IMM;
	case OP_STOREI4_MEMBASE_REG:
		return OP_STOREI4_MEMBASE_IMM;

#if defined(TARGET_X86) || defined (TARGET_AMD64)
	case OP_X86_PUSH:
		return OP_X86_PUSH_IMM;
	case OP_X86_COMPARE_MEMBASE_REG:
		return OP_X86_COMPARE_MEMBASE_IMM;
#endif
#if defined(TARGET_AMD64)
	case OP_AMD64_ICOMPARE_MEMBASE_REG:
		return OP_AMD64_ICOMPARE_MEMBASE_IMM;
#endif
	case OP_VOIDCALL_REG:
		return OP_VOIDCALL;
	case OP_CALL_REG:
		return OP_CALL;
	case OP_LCALL_REG:
		return OP_LCALL;
	case OP_FCALL_REG:
		return OP_FCALL;
	case OP_LOCALLOC:
		return OP_LOCALLOC_IMM;
	}

	return -1;
}

static int
ldind_to_load_membase (int opcode)
{
	switch (opcode) {
	case CEE_LDIND_I1:
		return OP_LOADI1_MEMBASE;
	case CEE_LDIND_U1:
		return OP_LOADU1_MEMBASE;
	case CEE_LDIND_I2:
		return OP_LOADI2_MEMBASE;
	case CEE_LDIND_U2:
		return OP_LOADU2_MEMBASE;
	case CEE_LDIND_I4:
		return OP_LOADI4_MEMBASE;
	case CEE_LDIND_U4:
		return OP_LOADU4_MEMBASE;
	case CEE_LDIND_I:
		return OP_LOAD_MEMBASE;
	case CEE_LDIND_REF:
		return OP_LOAD_MEMBASE;
	case CEE_LDIND_I8:
		return OP_LOADI8_MEMBASE;
	case CEE_LDIND_R4:
		return OP_LOADR4_MEMBASE;
	case CEE_LDIND_R8:
		return OP_LOADR8_MEMBASE;
	default:
		g_assert_not_reached ();
	}

	return -1;
}

static int
stind_to_store_membase (int opcode)
{
	switch (opcode) {
	case CEE_STIND_I1:
		return OP_STOREI1_MEMBASE_REG;
	case CEE_STIND_I2:
		return OP_STOREI2_MEMBASE_REG;
	case CEE_STIND_I4:
		return OP_STOREI4_MEMBASE_REG;
	case CEE_STIND_I:
	case CEE_STIND_REF:
		return OP_STORE_MEMBASE_REG;
	case CEE_STIND_I8:
		return OP_STOREI8_MEMBASE_REG;
	case CEE_STIND_R4:
		return OP_STORER4_MEMBASE_REG;
	case CEE_STIND_R8:
		return OP_STORER8_MEMBASE_REG;
	default:
		g_assert_not_reached ();
	}

	return -1;
}

int
mono_load_membase_to_load_mem (int opcode)
{
	// FIXME: Add a MONO_ARCH_HAVE_LOAD_MEM macro
#if defined(TARGET_X86) || defined(TARGET_AMD64)
	switch (opcode) {
	case OP_LOAD_MEMBASE:
		return OP_LOAD_MEM;
	case OP_LOADU1_MEMBASE:
		return OP_LOADU1_MEM;
	case OP_LOADU2_MEMBASE:
		return OP_LOADU2_MEM;
	case OP_LOADI4_MEMBASE:
		return OP_LOADI4_MEM;
	case OP_LOADU4_MEMBASE:
		return OP_LOADU4_MEM;
#if SIZEOF_REGISTER == 8
	case OP_LOADI8_MEMBASE:
		return OP_LOADI8_MEM;
#endif
	}
#endif

	return -1;
}

static inline int
op_to_op_dest_membase (int store_opcode, int opcode)
{
#if defined(TARGET_X86)
	if (!((store_opcode == OP_STORE_MEMBASE_REG) || (store_opcode == OP_STOREI4_MEMBASE_REG)))
		return -1;

	switch (opcode) {
	case OP_IADD:
		return OP_X86_ADD_MEMBASE_REG;
	case OP_ISUB:
		return OP_X86_SUB_MEMBASE_REG;
	case OP_IAND:
		return OP_X86_AND_MEMBASE_REG;
	case OP_IOR:
		return OP_X86_OR_MEMBASE_REG;
	case OP_IXOR:
		return OP_X86_XOR_MEMBASE_REG;
	case OP_ADD_IMM:
	case OP_IADD_IMM:
		return OP_X86_ADD_MEMBASE_IMM;
	case OP_SUB_IMM:
	case OP_ISUB_IMM:
		return OP_X86_SUB_MEMBASE_IMM;
	case OP_AND_IMM:
	case OP_IAND_IMM:
		return OP_X86_AND_MEMBASE_IMM;
	case OP_OR_IMM:
	case OP_IOR_IMM:
		return OP_X86_OR_MEMBASE_IMM;
	case OP_XOR_IMM:
	case OP_IXOR_IMM:
		return OP_X86_XOR_MEMBASE_IMM;
	case OP_MOVE:
		return OP_NOP;
	}
#endif

#if defined(TARGET_AMD64)
	if (!((store_opcode == OP_STORE_MEMBASE_REG) || (store_opcode == OP_STOREI4_MEMBASE_REG) || (store_opcode == OP_STOREI8_MEMBASE_REG)))
		return -1;

	switch (opcode) {
	case OP_IADD:
		return OP_X86_ADD_MEMBASE_REG;
	case OP_ISUB:
		return OP_X86_SUB_MEMBASE_REG;
	case OP_IAND:
		return OP_X86_AND_MEMBASE_REG;
	case OP_IOR:
		return OP_X86_OR_MEMBASE_REG;
	case OP_IXOR:
		return OP_X86_XOR_MEMBASE_REG;
	case OP_IADD_IMM:
		return OP_X86_ADD_MEMBASE_IMM;
	case OP_ISUB_IMM:
		return OP_X86_SUB_MEMBASE_IMM;
	case OP_IAND_IMM:
		return OP_X86_AND_MEMBASE_IMM;
	case OP_IOR_IMM:
		return OP_X86_OR_MEMBASE_IMM;
	case OP_IXOR_IMM:
		return OP_X86_XOR_MEMBASE_IMM;
	case OP_LADD:
		return OP_AMD64_ADD_MEMBASE_REG;
	case OP_LSUB:
		return OP_AMD64_SUB_MEMBASE_REG;
	case OP_LAND:
		return OP_AMD64_AND_MEMBASE_REG;
	case OP_LOR:
		return OP_AMD64_OR_MEMBASE_REG;
	case OP_LXOR:
		return OP_AMD64_XOR_MEMBASE_REG;
	case OP_ADD_IMM:
	case OP_LADD_IMM:
		return OP_AMD64_ADD_MEMBASE_IMM;
	case OP_SUB_IMM:
	case OP_LSUB_IMM:
		return OP_AMD64_SUB_MEMBASE_IMM;
	case OP_AND_IMM:
	case OP_LAND_IMM:
		return OP_AMD64_AND_MEMBASE_IMM;
	case OP_OR_IMM:
	case OP_LOR_IMM:
		return OP_AMD64_OR_MEMBASE_IMM;
	case OP_XOR_IMM:
	case OP_LXOR_IMM:
		return OP_AMD64_XOR_MEMBASE_IMM;
	case OP_MOVE:
		return OP_NOP;
	}
#endif

	return -1;
}

static inline int
op_to_op_store_membase (int store_opcode, int opcode)
{
#if defined(TARGET_X86) || defined(TARGET_AMD64)
	switch (opcode) {
	case OP_ICEQ:
		if (store_opcode == OP_STOREI1_MEMBASE_REG)
			return OP_X86_SETEQ_MEMBASE;
	case OP_CNE:
		if (store_opcode == OP_STOREI1_MEMBASE_REG)
			return OP_X86_SETNE_MEMBASE;
	}
#endif

	return -1;
}

static inline int
op_to_op_src1_membase (int load_opcode, int opcode)
{
#ifdef TARGET_X86
	/* FIXME: This has sign extension issues */
	/*
	if ((opcode == OP_ICOMPARE_IMM) && (load_opcode == OP_LOADU1_MEMBASE))
		return OP_X86_COMPARE_MEMBASE8_IMM;
	*/

	if (!((load_opcode == OP_LOAD_MEMBASE) || (load_opcode == OP_LOADI4_MEMBASE) || (load_opcode == OP_LOADU4_MEMBASE)))
		return -1;

	switch (opcode) {
	case OP_X86_PUSH:
		return OP_X86_PUSH_MEMBASE;
	case OP_COMPARE_IMM:
	case OP_ICOMPARE_IMM:
		return OP_X86_COMPARE_MEMBASE_IMM;
	case OP_COMPARE:
	case OP_ICOMPARE:
		return OP_X86_COMPARE_MEMBASE_REG;
	}
#endif

#ifdef TARGET_AMD64
	/* FIXME: This has sign extension issues */
	/*
	if ((opcode == OP_ICOMPARE_IMM) && (load_opcode == OP_LOADU1_MEMBASE))
		return OP_X86_COMPARE_MEMBASE8_IMM;
	*/

	switch (opcode) {
	case OP_X86_PUSH:
		if ((load_opcode == OP_LOAD_MEMBASE) || (load_opcode == OP_LOADI8_MEMBASE))
			return OP_X86_PUSH_MEMBASE;
		break;
		/* FIXME: This only works for 32 bit immediates
	case OP_COMPARE_IMM:
	case OP_LCOMPARE_IMM:
		if ((load_opcode == OP_LOAD_MEMBASE) || (load_opcode == OP_LOADI8_MEMBASE))
			return OP_AMD64_COMPARE_MEMBASE_IMM;
		*/
	case OP_ICOMPARE_IMM:
		if ((load_opcode == OP_LOADI4_MEMBASE) || (load_opcode == OP_LOADU4_MEMBASE))
			return OP_AMD64_ICOMPARE_MEMBASE_IMM;
		break;
	case OP_COMPARE:
	case OP_LCOMPARE:
		if ((load_opcode == OP_LOAD_MEMBASE) || (load_opcode == OP_LOADI8_MEMBASE))
			return OP_AMD64_COMPARE_MEMBASE_REG;
		break;
	case OP_ICOMPARE:
		if ((load_opcode == OP_LOADI4_MEMBASE) || (load_opcode == OP_LOADU4_MEMBASE))
			return OP_AMD64_ICOMPARE_MEMBASE_REG;
		break;
	}
#endif

	return -1;
}

static inline int
op_to_op_src2_membase (int load_opcode, int opcode)
{
#ifdef TARGET_X86
	if (!((load_opcode == OP_LOAD_MEMBASE) || (load_opcode == OP_LOADI4_MEMBASE) || (load_opcode == OP_LOADU4_MEMBASE)))
		return -1;
	
	switch (opcode) {
	case OP_COMPARE:
	case OP_ICOMPARE:
		return OP_X86_COMPARE_REG_MEMBASE;
	case OP_IADD:
		return OP_X86_ADD_REG_MEMBASE;
	case OP_ISUB:
		return OP_X86_SUB_REG_MEMBASE;
	case OP_IAND:
		return OP_X86_AND_REG_MEMBASE;
	case OP_IOR:
		return OP_X86_OR_REG_MEMBASE;
	case OP_IXOR:
		return OP_X86_XOR_REG_MEMBASE;
	}
#endif

#ifdef TARGET_AMD64
	switch (opcode) {
	case OP_ICOMPARE:
		if ((load_opcode == OP_LOADI4_MEMBASE) || (load_opcode == OP_LOADU4_MEMBASE))
			return OP_AMD64_ICOMPARE_REG_MEMBASE;
		break;
	case OP_COMPARE:
	case OP_LCOMPARE:
		if ((load_opcode == OP_LOADI8_MEMBASE) || (load_opcode == OP_LOAD_MEMBASE))
			return OP_AMD64_COMPARE_REG_MEMBASE;
		break;
	case OP_IADD:
		if ((load_opcode == OP_LOADI4_MEMBASE) || (load_opcode == OP_LOADU4_MEMBASE))
			return OP_X86_ADD_REG_MEMBASE;
	case OP_ISUB:
		if ((load_opcode == OP_LOADI4_MEMBASE) || (load_opcode == OP_LOADU4_MEMBASE))
			return OP_X86_SUB_REG_MEMBASE;
	case OP_IAND:
		if ((load_opcode == OP_LOADI4_MEMBASE) || (load_opcode == OP_LOADU4_MEMBASE))
			return OP_X86_AND_REG_MEMBASE;
	case OP_IOR:
		if ((load_opcode == OP_LOADI4_MEMBASE) || (load_opcode == OP_LOADU4_MEMBASE))
			return OP_X86_OR_REG_MEMBASE;
	case OP_IXOR:
		if ((load_opcode == OP_LOADI4_MEMBASE) || (load_opcode == OP_LOADU4_MEMBASE))
			return OP_X86_XOR_REG_MEMBASE;
	case OP_LADD:
		if ((load_opcode == OP_LOADI8_MEMBASE) || (load_opcode == OP_LOAD_MEMBASE))
			return OP_AMD64_ADD_REG_MEMBASE;
	case OP_LSUB:
		if ((load_opcode == OP_LOADI8_MEMBASE) || (load_opcode == OP_LOAD_MEMBASE))
			return OP_AMD64_SUB_REG_MEMBASE;
	case OP_LAND:
		if ((load_opcode == OP_LOADI8_MEMBASE) || (load_opcode == OP_LOAD_MEMBASE))
			return OP_AMD64_AND_REG_MEMBASE;
	case OP_LOR:
		if ((load_opcode == OP_LOADI8_MEMBASE) || (load_opcode == OP_LOAD_MEMBASE))
			return OP_AMD64_OR_REG_MEMBASE;
	case OP_LXOR:
		if ((load_opcode == OP_LOADI8_MEMBASE) || (load_opcode == OP_LOAD_MEMBASE))
			return OP_AMD64_XOR_REG_MEMBASE;
	}
#endif

	return -1;
}

int
mono_op_to_op_imm_noemul (int opcode)
{
	switch (opcode) {
#if SIZEOF_REGISTER == 4 && !defined(MONO_ARCH_NO_EMULATE_LONG_SHIFT_OPS)
	case OP_LSHR:
	case OP_LSHL:
	case OP_LSHR_UN:
#endif
#if defined(MONO_ARCH_EMULATE_MUL_DIV) || defined(MONO_ARCH_EMULATE_DIV)
	case OP_IDIV:
	case OP_IDIV_UN:
	case OP_IREM:
	case OP_IREM_UN:
#endif
		return -1;
	default:
		return mono_op_to_op_imm (opcode);
	}
}

#ifndef DISABLE_JIT

/**
 * mono_handle_global_vregs:
 *
 *   Make vregs used in more than one bblock 'global', i.e. allocate a variable
 * for them.
 */
void
mono_handle_global_vregs (MonoCompile *cfg)
{
	gint32 *vreg_to_bb;
	MonoBasicBlock *bb;
	int i, pos;

	vreg_to_bb = mono_mempool_alloc0 (cfg->mempool, sizeof (gint32*) * cfg->next_vreg + 1);

#ifdef MONO_ARCH_SIMD_INTRINSICS
	if (cfg->uses_simd_intrinsics)
		mono_simd_simplify_indirection (cfg);
#endif

	/* Find local vregs used in more than one bb */
	for (bb = cfg->bb_entry; bb; bb = bb->next_bb) {
		MonoInst *ins = bb->code;	
		int block_num = bb->block_num;

		if (cfg->verbose_level > 2)
			printf ("\nHANDLE-GLOBAL-VREGS BLOCK %d:\n", bb->block_num);

		cfg->cbb = bb;
		for (; ins; ins = ins->next) {
			const char *spec = INS_INFO (ins->opcode);
			int regtype, regindex;
			gint32 prev_bb;

			if (G_UNLIKELY (cfg->verbose_level > 2))
				mono_print_ins (ins);

			g_assert (ins->opcode >= MONO_CEE_LAST);

			for (regindex = 0; regindex < 3; regindex ++) {
				int vreg;

				if (regindex == 0) {
					regtype = spec [MONO_INST_DEST];
					if (regtype == ' ')
						continue;
					vreg = ins->dreg;
				} else if (regindex == 1) {
					regtype = spec [MONO_INST_SRC1];
					if (regtype == ' ')
						continue;
					vreg = ins->sreg1;
				} else {
					regtype = spec [MONO_INST_SRC2];
					if (regtype == ' ')
						continue;
					vreg = ins->sreg2;
				}

#if SIZEOF_REGISTER == 4
				if (regtype == 'l') {
					/*
					 * Since some instructions reference the original long vreg,
					 * and some reference the two component vregs, it is quite hard
					 * to determine when it needs to be global. So be conservative.
					 */
					if (!get_vreg_to_inst (cfg, vreg)) {
						mono_compile_create_var_for_vreg (cfg, &mono_defaults.int64_class->byval_arg, OP_LOCAL, vreg);

						if (cfg->verbose_level > 2)
							printf ("LONG VREG R%d made global.\n", vreg);
					}

					/*
					 * Make the component vregs volatile since the optimizations can
					 * get confused otherwise.
					 */
					get_vreg_to_inst (cfg, vreg + 1)->flags |= MONO_INST_VOLATILE;
					get_vreg_to_inst (cfg, vreg + 2)->flags |= MONO_INST_VOLATILE;
				}
#endif

				g_assert (vreg != -1);

				prev_bb = vreg_to_bb [vreg];
				if (prev_bb == 0) {
					/* 0 is a valid block num */
					vreg_to_bb [vreg] = block_num + 1;
				} else if ((prev_bb != block_num + 1) && (prev_bb != -1)) {
					if (((regtype == 'i' && (vreg < MONO_MAX_IREGS))) || (regtype == 'f' && (vreg < MONO_MAX_FREGS)))
						continue;

					if (!get_vreg_to_inst (cfg, vreg)) {
						if (G_UNLIKELY (cfg->verbose_level > 2))
							printf ("VREG R%d used in BB%d and BB%d made global.\n", vreg, vreg_to_bb [vreg], block_num);

						switch (regtype) {
						case 'i':
							mono_compile_create_var_for_vreg (cfg, &mono_defaults.int_class->byval_arg, OP_LOCAL, vreg);
							break;
						case 'f':
							mono_compile_create_var_for_vreg (cfg, &mono_defaults.double_class->byval_arg, OP_LOCAL, vreg);
							break;
						case 'v':
							mono_compile_create_var_for_vreg (cfg, &ins->klass->byval_arg, OP_LOCAL, vreg);
							break;
						default:
							g_assert_not_reached ();
						}
					}

					/* Flag as having been used in more than one bb */
					vreg_to_bb [vreg] = -1;
				}
			}
		}
	}

	/* If a variable is used in only one bblock, convert it into a local vreg */
	for (i = 0; i < cfg->num_varinfo; i++) {
		MonoInst *var = cfg->varinfo [i];
		MonoMethodVar *vmv = MONO_VARINFO (cfg, i);

		switch (var->type) {
		case STACK_I4:
		case STACK_OBJ:
		case STACK_PTR:
		case STACK_MP:
		case STACK_VTYPE:
#if SIZEOF_REGISTER == 8
		case STACK_I8:
#endif
#if !defined(TARGET_X86) && !defined(MONO_ARCH_SOFT_FLOAT)
		/* Enabling this screws up the fp stack on x86 */
		case STACK_R8:
#endif
			/* Arguments are implicitly global */
			/* Putting R4 vars into registers doesn't work currently */
			if ((var->opcode != OP_ARG) && (var != cfg->ret) && !(var->flags & (MONO_INST_VOLATILE|MONO_INST_INDIRECT)) && (vreg_to_bb [var->dreg] != -1) && (var->klass->byval_arg.type != MONO_TYPE_R4) && !cfg->disable_vreg_to_lvreg) {
				/* 
				 * Make that the variable's liveness interval doesn't contain a call, since
				 * that would cause the lvreg to be spilled, making the whole optimization
				 * useless.
				 */
				/* This is too slow for JIT compilation */
#if 0
				if (cfg->compile_aot && vreg_to_bb [var->dreg]) {
					MonoInst *ins;
					int def_index, call_index, ins_index;
					gboolean spilled = FALSE;

					def_index = -1;
					call_index = -1;
					ins_index = 0;
					for (ins = vreg_to_bb [var->dreg]->code; ins; ins = ins->next) {
						const char *spec = INS_INFO (ins->opcode);

						if ((spec [MONO_INST_DEST] != ' ') && (ins->dreg == var->dreg))
							def_index = ins_index;

						if (((spec [MONO_INST_SRC1] != ' ') && (ins->sreg1 == var->dreg)) ||
							((spec [MONO_INST_SRC1] != ' ') && (ins->sreg1 == var->dreg))) {
							if (call_index > def_index) {
								spilled = TRUE;
								break;
							}
						}

						if (MONO_IS_CALL (ins))
							call_index = ins_index;

						ins_index ++;
					}

					if (spilled)
						break;
				}
#endif

				if (G_UNLIKELY (cfg->verbose_level > 2))
					printf ("CONVERTED R%d(%d) TO VREG.\n", var->dreg, vmv->idx);
				var->flags |= MONO_INST_IS_DEAD;
				cfg->vreg_to_inst [var->dreg] = NULL;
			}
			break;
		}
	}

	/* 
	 * Compress the varinfo and vars tables so the liveness computation is faster and
	 * takes up less space.
	 */
	pos = 0;
	for (i = 0; i < cfg->num_varinfo; ++i) {
		MonoInst *var = cfg->varinfo [i];
		if (pos < i && cfg->locals_start == i)
			cfg->locals_start = pos;
		if (!(var->flags & MONO_INST_IS_DEAD)) {
			if (pos < i) {
				cfg->varinfo [pos] = cfg->varinfo [i];
				cfg->varinfo [pos]->inst_c0 = pos;
				memcpy (&cfg->vars [pos], &cfg->vars [i], sizeof (MonoMethodVar));
				cfg->vars [pos].idx = pos;
#if SIZEOF_REGISTER == 4
				if (cfg->varinfo [pos]->type == STACK_I8) {
					/* Modify the two component vars too */
					MonoInst *var1;

					var1 = get_vreg_to_inst (cfg, cfg->varinfo [pos]->dreg + 1);
					var1->inst_c0 = pos;
					var1 = get_vreg_to_inst (cfg, cfg->varinfo [pos]->dreg + 2);
					var1->inst_c0 = pos;
				}
#endif
			}
			pos ++;
		}
	}
	cfg->num_varinfo = pos;
	if (cfg->locals_start > cfg->num_varinfo)
		cfg->locals_start = cfg->num_varinfo;
}

/**
 * mono_spill_global_vars:
 *
 *   Generate spill code for variables which are not allocated to registers, 
 * and replace vregs with their allocated hregs. *need_local_opts is set to TRUE if
 * code is generated which could be optimized by the local optimization passes.
 */
void
mono_spill_global_vars (MonoCompile *cfg, gboolean *need_local_opts)
{
	MonoBasicBlock *bb;
	char spec2 [16];
	int orig_next_vreg;
	guint32 *vreg_to_lvreg;
	guint32 *lvregs;
	guint32 i, lvregs_len;
	gboolean dest_has_lvreg = FALSE;
	guint32 stacktypes [128];

	*need_local_opts = FALSE;

	memset (spec2, 0, sizeof (spec2));

	/* FIXME: Move this function to mini.c */
	stacktypes ['i'] = STACK_PTR;
	stacktypes ['l'] = STACK_I8;
	stacktypes ['f'] = STACK_R8;
#ifdef MONO_ARCH_SIMD_INTRINSICS
	stacktypes ['x'] = STACK_VTYPE;
#endif

#if SIZEOF_REGISTER == 4
	/* Create MonoInsts for longs */
	for (i = 0; i < cfg->num_varinfo; i++) {
		MonoInst *ins = cfg->varinfo [i];

		if ((ins->opcode != OP_REGVAR) && !(ins->flags & MONO_INST_IS_DEAD)) {
			switch (ins->type) {
#ifdef MONO_ARCH_SOFT_FLOAT
			case STACK_R8:
#endif
			case STACK_I8: {
				MonoInst *tree;

				g_assert (ins->opcode == OP_REGOFFSET);

				tree = get_vreg_to_inst (cfg, ins->dreg + 1);
				g_assert (tree);
				tree->opcode = OP_REGOFFSET;
				tree->inst_basereg = ins->inst_basereg;
				tree->inst_offset = ins->inst_offset + MINI_LS_WORD_OFFSET;

				tree = get_vreg_to_inst (cfg, ins->dreg + 2);
				g_assert (tree);
				tree->opcode = OP_REGOFFSET;
				tree->inst_basereg = ins->inst_basereg;
				tree->inst_offset = ins->inst_offset + MINI_MS_WORD_OFFSET;
				break;
			}
			default:
				break;
			}
		}
	}
#endif

	/* FIXME: widening and truncation */

	/*
	 * As an optimization, when a variable allocated to the stack is first loaded into 
	 * an lvreg, we will remember the lvreg and use it the next time instead of loading
	 * the variable again.
	 */
	orig_next_vreg = cfg->next_vreg;
	vreg_to_lvreg = mono_mempool_alloc0 (cfg->mempool, sizeof (guint32) * cfg->next_vreg);
	lvregs = mono_mempool_alloc (cfg->mempool, sizeof (guint32) * 1024);
	lvregs_len = 0;
	
	/* Add spill loads/stores */
	for (bb = cfg->bb_entry; bb; bb = bb->next_bb) {
		MonoInst *ins;

		if (cfg->verbose_level > 2)
			printf ("\nSPILL BLOCK %d:\n", bb->block_num);

		/* Clear vreg_to_lvreg array */
		for (i = 0; i < lvregs_len; i++)
			vreg_to_lvreg [lvregs [i]] = 0;
		lvregs_len = 0;

		cfg->cbb = bb;
		MONO_BB_FOR_EACH_INS (bb, ins) {
			const char *spec = INS_INFO (ins->opcode);
			int regtype, srcindex, sreg, tmp_reg, prev_dreg;
			gboolean store, no_lvreg;

			if (G_UNLIKELY (cfg->verbose_level > 2))
				mono_print_ins (ins);

			if (ins->opcode == OP_NOP)
				continue;

			/* 
			 * We handle LDADDR here as well, since it can only be decomposed
			 * when variable addresses are known.
			 */
			if (ins->opcode == OP_LDADDR) {
				MonoInst *var = ins->inst_p0;

				if (var->opcode == OP_VTARG_ADDR) {
					/* Happens on SPARC/S390 where vtypes are passed by reference */
					MonoInst *vtaddr = var->inst_left;
					if (vtaddr->opcode == OP_REGVAR) {
						ins->opcode = OP_MOVE;
						ins->sreg1 = vtaddr->dreg;
					}
					else if (var->inst_left->opcode == OP_REGOFFSET) {
						ins->opcode = OP_LOAD_MEMBASE;
						ins->inst_basereg = vtaddr->inst_basereg;
						ins->inst_offset = vtaddr->inst_offset;
					} else
						NOT_IMPLEMENTED;
				} else {
					g_assert (var->opcode == OP_REGOFFSET);

					ins->opcode = OP_ADD_IMM;
					ins->sreg1 = var->inst_basereg;
					ins->inst_imm = var->inst_offset;
				}

				*need_local_opts = TRUE;
				spec = INS_INFO (ins->opcode);
			}

			if (ins->opcode < MONO_CEE_LAST) {
				mono_print_ins (ins);
				g_assert_not_reached ();
			}

			/*
			 * Store opcodes have destbasereg in the dreg, but in reality, it is an
			 * src register.
			 * FIXME:
			 */
			if (MONO_IS_STORE_MEMBASE (ins)) {
				tmp_reg = ins->dreg;
				ins->dreg = ins->sreg2;
				ins->sreg2 = tmp_reg;
				store = TRUE;

				spec2 [MONO_INST_DEST] = ' ';
				spec2 [MONO_INST_SRC1] = spec [MONO_INST_SRC1];
				spec2 [MONO_INST_SRC2] = spec [MONO_INST_DEST];
				spec = spec2;
			} else if (MONO_IS_STORE_MEMINDEX (ins))
				g_assert_not_reached ();
			else
				store = FALSE;
			no_lvreg = FALSE;

			if (G_UNLIKELY (cfg->verbose_level > 2))
				printf ("\t %.3s %d %d %d\n", spec, ins->dreg, ins->sreg1, ins->sreg2);

			/***************/
			/*    DREG     */
			/***************/
			regtype = spec [MONO_INST_DEST];
			g_assert (((ins->dreg == -1) && (regtype == ' ')) || ((ins->dreg != -1) && (regtype != ' ')));
			prev_dreg = -1;

			if ((ins->dreg != -1) && get_vreg_to_inst (cfg, ins->dreg)) {
				MonoInst *var = get_vreg_to_inst (cfg, ins->dreg);
				MonoInst *store_ins;
				int store_opcode;

				store_opcode = mono_type_to_store_membase (cfg, var->inst_vtype);

				if (var->opcode == OP_REGVAR) {
					ins->dreg = var->dreg;
				} else if ((ins->dreg == ins->sreg1) && (spec [MONO_INST_DEST] == 'i') && (spec [MONO_INST_SRC1] == 'i') && !vreg_to_lvreg [ins->dreg] && (op_to_op_dest_membase (store_opcode, ins->opcode) != -1)) {
					/* 
					 * Instead of emitting a load+store, use a _membase opcode.
					 */
					g_assert (var->opcode == OP_REGOFFSET);
					if (ins->opcode == OP_MOVE) {
						NULLIFY_INS (ins);
					} else {
						ins->opcode = op_to_op_dest_membase (store_opcode, ins->opcode);
						ins->inst_basereg = var->inst_basereg;
						ins->inst_offset = var->inst_offset;
						ins->dreg = -1;
					}
					spec = INS_INFO (ins->opcode);
				} else {
					guint32 lvreg;

					g_assert (var->opcode == OP_REGOFFSET);

					prev_dreg = ins->dreg;

					/* Invalidate any previous lvreg for this vreg */
					vreg_to_lvreg [ins->dreg] = 0;

					lvreg = 0;

#ifdef MONO_ARCH_SOFT_FLOAT
					if (store_opcode == OP_STORER8_MEMBASE_REG) {
						regtype = 'l';
						store_opcode = OP_STOREI8_MEMBASE_REG;
					}
#endif

					ins->dreg = alloc_dreg (cfg, stacktypes [regtype]);

					if (regtype == 'l') {
						NEW_STORE_MEMBASE (cfg, store_ins, OP_STOREI4_MEMBASE_REG, var->inst_basereg, var->inst_offset + MINI_LS_WORD_OFFSET, ins->dreg + 1);
						mono_bblock_insert_after_ins (bb, ins, store_ins);
						NEW_STORE_MEMBASE (cfg, store_ins, OP_STOREI4_MEMBASE_REG, var->inst_basereg, var->inst_offset + MINI_MS_WORD_OFFSET, ins->dreg + 2);
						mono_bblock_insert_after_ins (bb, ins, store_ins);
					}
					else {
						g_assert (store_opcode != OP_STOREV_MEMBASE);

						/* Try to fuse the store into the instruction itself */
						/* FIXME: Add more instructions */
						if (!lvreg && ((ins->opcode == OP_ICONST) || ((ins->opcode == OP_I8CONST) && (ins->inst_c0 == 0)))) {
							ins->opcode = store_membase_reg_to_store_membase_imm (store_opcode);
							ins->inst_imm = ins->inst_c0;
							ins->inst_destbasereg = var->inst_basereg;
							ins->inst_offset = var->inst_offset;
						} else if (!lvreg && ((ins->opcode == OP_MOVE) || (ins->opcode == OP_FMOVE) || (ins->opcode == OP_LMOVE))) {
							ins->opcode = store_opcode;
							ins->inst_destbasereg = var->inst_basereg;
							ins->inst_offset = var->inst_offset;

							no_lvreg = TRUE;

							tmp_reg = ins->dreg;
							ins->dreg = ins->sreg2;
							ins->sreg2 = tmp_reg;
							store = TRUE;

							spec2 [MONO_INST_DEST] = ' ';
							spec2 [MONO_INST_SRC1] = spec [MONO_INST_SRC1];
							spec2 [MONO_INST_SRC2] = spec [MONO_INST_DEST];
							spec = spec2;
						} else if (!lvreg && (op_to_op_store_membase (store_opcode, ins->opcode) != -1)) {
							// FIXME: The backends expect the base reg to be in inst_basereg
							ins->opcode = op_to_op_store_membase (store_opcode, ins->opcode);
							ins->dreg = -1;
							ins->inst_basereg = var->inst_basereg;
							ins->inst_offset = var->inst_offset;
							spec = INS_INFO (ins->opcode);
						} else {
							/* printf ("INS: "); mono_print_ins (ins); */
							/* Create a store instruction */
							NEW_STORE_MEMBASE (cfg, store_ins, store_opcode, var->inst_basereg, var->inst_offset, ins->dreg);

							/* Insert it after the instruction */
							mono_bblock_insert_after_ins (bb, ins, store_ins);

							/* 
							 * We can't assign ins->dreg to var->dreg here, since the
							 * sregs could use it. So set a flag, and do it after
							 * the sregs.
							 */
							if ((!MONO_ARCH_USE_FPSTACK || ((store_opcode != OP_STORER8_MEMBASE_REG) && (store_opcode != OP_STORER4_MEMBASE_REG))) && !((var)->flags & (MONO_INST_VOLATILE|MONO_INST_INDIRECT)))
								dest_has_lvreg = TRUE;
						}
					}
				}
			}

			/************/
			/*  SREGS   */
			/************/
			for (srcindex = 0; srcindex < 2; ++srcindex) {
				regtype = spec [(srcindex == 0) ? MONO_INST_SRC1 : MONO_INST_SRC2];
				sreg = srcindex == 0 ? ins->sreg1 : ins->sreg2;

				g_assert (((sreg == -1) && (regtype == ' ')) || ((sreg != -1) && (regtype != ' ')));
				if ((sreg != -1) && get_vreg_to_inst (cfg, sreg)) {
					MonoInst *var = get_vreg_to_inst (cfg, sreg);
					MonoInst *load_ins;
					guint32 load_opcode;

					if (var->opcode == OP_REGVAR) {
						if (srcindex == 0)
							ins->sreg1 = var->dreg;
						else
							ins->sreg2 = var->dreg;
						continue;
					}

					g_assert (var->opcode == OP_REGOFFSET);
						
					load_opcode = mono_type_to_load_membase (cfg, var->inst_vtype);

					g_assert (load_opcode != OP_LOADV_MEMBASE);

					if (vreg_to_lvreg [sreg]) {
						/* The variable is already loaded to an lvreg */
						if (G_UNLIKELY (cfg->verbose_level > 2))
							printf ("\t\tUse lvreg R%d for R%d.\n", vreg_to_lvreg [sreg], sreg);
						if (srcindex == 0)
							ins->sreg1 = vreg_to_lvreg [sreg];
						else
							ins->sreg2 = vreg_to_lvreg [sreg];
						continue;
					}

					/* Try to fuse the load into the instruction */
					if ((srcindex == 0) && (op_to_op_src1_membase (load_opcode, ins->opcode) != -1)) {
						ins->opcode = op_to_op_src1_membase (load_opcode, ins->opcode);
						ins->inst_basereg = var->inst_basereg;
						ins->inst_offset = var->inst_offset;
					} else if ((srcindex == 1) && (op_to_op_src2_membase (load_opcode, ins->opcode) != -1)) {
						ins->opcode = op_to_op_src2_membase (load_opcode, ins->opcode);
						ins->sreg2 = var->inst_basereg;
						ins->inst_offset = var->inst_offset;
					} else {
						if (MONO_IS_REAL_MOVE (ins)) {
							ins->opcode = OP_NOP;
							sreg = ins->dreg;
						} else {
							//printf ("%d ", srcindex); mono_print_ins (ins);

							sreg = alloc_dreg (cfg, stacktypes [regtype]);

							if ((!MONO_ARCH_USE_FPSTACK || ((load_opcode != OP_LOADR8_MEMBASE) && (load_opcode != OP_LOADR4_MEMBASE))) && !((var)->flags & (MONO_INST_VOLATILE|MONO_INST_INDIRECT)) && !no_lvreg) {
								if (var->dreg == prev_dreg) {
									/*
									 * sreg refers to the value loaded by the load
									 * emitted below, but we need to use ins->dreg
									 * since it refers to the store emitted earlier.
									 */
									sreg = ins->dreg;
								}
								vreg_to_lvreg [var->dreg] = sreg;
								g_assert (lvregs_len < 1024);
								lvregs [lvregs_len ++] = var->dreg;
							}
						}

						if (srcindex == 0)
							ins->sreg1 = sreg;
						else
							ins->sreg2 = sreg;

						if (regtype == 'l') {
							NEW_LOAD_MEMBASE (cfg, load_ins, OP_LOADI4_MEMBASE, sreg + 2, var->inst_basereg, var->inst_offset + MINI_MS_WORD_OFFSET);
							mono_bblock_insert_before_ins (bb, ins, load_ins);
							NEW_LOAD_MEMBASE (cfg, load_ins, OP_LOADI4_MEMBASE, sreg + 1, var->inst_basereg, var->inst_offset + MINI_LS_WORD_OFFSET);
							mono_bblock_insert_before_ins (bb, ins, load_ins);
						}
						else {
#if SIZEOF_REGISTER == 4
							g_assert (load_opcode != OP_LOADI8_MEMBASE);
#endif
							NEW_LOAD_MEMBASE (cfg, load_ins, load_opcode, sreg, var->inst_basereg, var->inst_offset);
							mono_bblock_insert_before_ins (bb, ins, load_ins);
						}
					}
				}
			}

			if (dest_has_lvreg) {
				vreg_to_lvreg [prev_dreg] = ins->dreg;
				g_assert (lvregs_len < 1024);
				lvregs [lvregs_len ++] = prev_dreg;
				dest_has_lvreg = FALSE;
			}

			if (store) {
				tmp_reg = ins->dreg;
				ins->dreg = ins->sreg2;
				ins->sreg2 = tmp_reg;
			}

			if (MONO_IS_CALL (ins)) {
				/* Clear vreg_to_lvreg array */
				for (i = 0; i < lvregs_len; i++)
					vreg_to_lvreg [lvregs [i]] = 0;
				lvregs_len = 0;
			}

			if (cfg->verbose_level > 2)
				mono_print_ins_index (1, ins);
		}
	}
}

/**
 * FIXME:
 * - use 'iadd' instead of 'int_add'
 * - handling ovf opcodes: decompose in method_to_ir.
 * - unify iregs/fregs
 *   -> partly done, the missing parts are:
 *   - a more complete unification would involve unifying the hregs as well, so
 *     code wouldn't need if (fp) all over the place. but that would mean the hregs
 *     would no longer map to the machine hregs, so the code generators would need to
 *     be modified. Also, on ia64 for example, niregs + nfregs > 256 -> bitmasks
 *     wouldn't work any more. Duplicating the code in mono_local_regalloc () into
 *     fp/non-fp branches speeds it up by about 15%.
 * - use sext/zext opcodes instead of shifts
 * - add OP_ICALL
 * - get rid of TEMPLOADs if possible and use vregs instead
 * - clean up usage of OP_P/OP_ opcodes
 * - cleanup usage of DUMMY_USE
 * - cleanup the setting of ins->type for MonoInst's which are pushed on the 
 *   stack
 * - set the stack type and allocate a dreg in the EMIT_NEW macros
 * - get rid of all the <foo>2 stuff when the new JIT is ready.
 * - make sure handle_stack_args () is called before the branch is emitted
 * - when the new IR is done, get rid of all unused stuff
 * - COMPARE/BEQ as separate instructions or unify them ?
 *   - keeping them separate allows specialized compare instructions like
 *     compare_imm, compare_membase
 *   - most back ends unify fp compare+branch, fp compare+ceq
 * - integrate mono_save_args into inline_method
 * - get rid of the empty bblocks created by MONO_EMIT_NEW_BRACH_BLOCK2
 * - handle long shift opts on 32 bit platforms somehow: they require 
 *   3 sregs (2 for arg1 and 1 for arg2)
 * - make byref a 'normal' type.
 * - use vregs for bb->out_stacks if possible, handle_global_vreg will make them a
 *   variable if needed.
 * - do not start a new IL level bblock when cfg->cbb is changed by a function call
 *   like inline_method.
 * - remove inlining restrictions
 * - fix LNEG and enable cfold of INEG
 * - generalize x86 optimizations like ldelema as a peephole optimization
 * - add store_mem_imm for amd64
 * - optimize the loading of the interruption flag in the managed->native wrappers
 * - avoid special handling of OP_NOP in passes
 * - move code inserting instructions into one function/macro.
 * - try a coalescing phase after liveness analysis
 * - add float -> vreg conversion + local optimizations on !x86
 * - figure out how to handle decomposed branches during optimizations, ie.
 *   compare+branch, op_jump_table+op_br etc.
 * - promote RuntimeXHandles to vregs
 * - vtype cleanups:
 *   - add a NEW_VARLOADA_VREG macro
 * - the vtype optimizations are blocked by the LDADDR opcodes generated for 
 *   accessing vtype fields.
 * - get rid of I8CONST on 64 bit platforms
 * - dealing with the increase in code size due to branches created during opcode
 *   decomposition:
 *   - use extended basic blocks
 *     - all parts of the JIT
 *     - handle_global_vregs () && local regalloc
 *   - avoid introducing global vregs during decomposition, like 'vtable' in isinst
 * - sources of increase in code size:
 *   - vtypes
 *   - long compares
 *   - isinst and castclass
 *   - lvregs not allocated to global registers even if used multiple times
 * - call cctors outside the JIT, to make -v output more readable and JIT timings more
 *   meaningful.
 * - check for fp stack leakage in other opcodes too. (-> 'exceptions' optimization)
 * - add all micro optimizations from the old JIT
 * - put tree optimizations into the deadce pass
 * - decompose op_start_handler/op_endfilter/op_endfinally earlier using an arch
 *   specific function.
 * - unify the float comparison opcodes with the other comparison opcodes, i.e.
 *   fcompare + branchCC.
 * - create a helper function for allocating a stack slot, taking into account 
 *   MONO_CFG_HAS_SPILLUP.
 * - merge r68207.
 * - merge the ia64 switch changes.
 * - optimize mono_regstate2_alloc_int/float.
 * - fix the pessimistic handling of variables accessed in exception handler blocks.
 * - need to write a tree optimization pass, but the creation of trees is difficult, i.e.
 *   parts of the tree could be separated by other instructions, killing the tree
 *   arguments, or stores killing loads etc. Also, should we fold loads into other
 *   instructions if the result of the load is used multiple times ?
 * - make the REM_IMM optimization in mini-x86.c arch-independent.
 * - LAST MERGE: 108395.
 * - when returning vtypes in registers, generate IR and append it to the end of the
 *   last bb instead of doing it in the epilog.
 * - change the store opcodes so they use sreg1 instead of dreg to store the base register.
 */

/*

NOTES
-----

- When to decompose opcodes:
  - earlier: this makes some optimizations hard to implement, since the low level IR
  no longer contains the neccessary information. But it is easier to do.
  - later: harder to implement, enables more optimizations.
- Branches inside bblocks:
  - created when decomposing complex opcodes. 
    - branches to another bblock: harmless, but not tracked by the branch 
      optimizations, so need to branch to a label at the start of the bblock.
    - branches to inside the same bblock: very problematic, trips up the local
      reg allocator. Can be fixed by spitting the current bblock, but that is a
      complex operation, since some local vregs can become global vregs etc.
- Local/global vregs:
  - local vregs: temporary vregs used inside one bblock. Assigned to hregs by the
    local register allocator.
  - global vregs: used in more than one bblock. Have an associated MonoMethodVar
    structure, created by mono_create_var (). Assigned to hregs or the stack by
    the global register allocator.
- When to do optimizations like alu->alu_imm:
  - earlier -> saves work later on since the IR will be smaller/simpler
  - later -> can work on more instructions
- Handling of valuetypes:
  - When a vtype is pushed on the stack, a new temporary is created, an 
    instruction computing its address (LDADDR) is emitted and pushed on
    the stack. Need to optimize cases when the vtype is used immediately as in
    argument passing, stloc etc.
- Instead of the to_end stuff in the old JIT, simply call the function handling
  the values on the stack before emitting the last instruction of the bb.
*/

#endif /* DISABLE_JIT */
