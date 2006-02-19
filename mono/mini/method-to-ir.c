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
#include <unistd.h>
#include <math.h>
#include <sys/time.h>

#ifdef sun    // Solaris x86
#include <sys/types.h>
#include <sys/ucontext.h>
#endif

#ifdef HAVE_VALGRIND_MEMCHECK_H
#include <valgrind/memcheck.h>
#endif

#include <mono/metadata/assembly.h>
#include <mono/metadata/loader.h>
#include <mono/metadata/cil-coff.h>
#include <mono/metadata/tabledefs.h>
#include <mono/metadata/class.h>
#include <mono/metadata/object.h>
#include <mono/metadata/exception.h>
#include <mono/metadata/opcodes.h>
#include <mono/metadata/mono-endian.h>
#include <mono/metadata/tokentype.h>
#include <mono/metadata/tabledefs.h>
#include <mono/metadata/threads.h>
#include <mono/metadata/marshal.h>
#include <mono/metadata/socket-io.h>
#include <mono/metadata/appdomain.h>
#include <mono/metadata/debug-helpers.h>
#include <mono/io-layer/io-layer.h>
#include "mono/metadata/profiler.h"
#include <mono/metadata/profiler-private.h>
#include <mono/metadata/mono-config.h>
#include <mono/metadata/environment.h>
#include <mono/metadata/mono-debug.h>
#include <mono/metadata/mono-debug-debugger.h>
#include <mono/metadata/monitor.h>
#include <mono/metadata/security-manager.h>
#include <mono/metadata/threads-types.h>
#include <mono/metadata/rawbuffer.h>
#include <mono/utils/mono-math.h>
#include <mono/utils/mono-compiler.h>
#include <mono/os/gc_wrapper.h>

#define NEW_IR
#include "mini.h"
#include <string.h>
#include <ctype.h>
#include "trace.h"

#include "jit-icalls.h"

#include "aliasing.h"

/* 
 * this is used to determine when some branch optimizations are possible: we exclude FP compares
 * because they have weird semantics with NaNs.
 */
#define MONO_IS_COND_BRANCH_OP(ins) (((ins)->opcode >= CEE_BEQ && (ins)->opcode <= CEE_BLT_UN) || ((ins)->opcode >= OP_LBEQ && (ins)->opcode <= OP_LBLT_UN) || ((ins)->opcode >= OP_FBEQ && (ins)->opcode <= OP_FBLT_UN) || ((ins)->opcode >= OP_IBEQ && (ins)->opcode <= OP_IBLT_UN))
#define MONO_IS_COND_BRANCH_NOFP(ins) (MONO_IS_COND_BRANCH_OP(ins) && (ins)->inst_left->inst_left->type != STACK_R8)

/* FIXME: What is this ? */
#define MONO_CHECK_THIS(ins) FALSE
//#define MONO_CHECK_THIS(ins) (mono_method_signature (cfg->method)->hasthis && (ins)->ssa_op == MONO_SSA_LOAD && (ins)->inst_left->inst_c0 == 0)

#define NOT_IMPLEMENTED g_assert_not_reached ()

static int ldind_to_load_membase (int opcode);
static int stind_to_store_membase (int opcode);

int mono_op_to_op_imm (int opcode);

gboolean  mono_arch_print_tree(MonoInst *tree, int arity);

static void emit_stobj (MonoCompile *cfg, MonoInst *dest, MonoInst *src, 
			  const unsigned char *ip, MonoClass *klass, gboolean native);

int mono_method_to_ir2 (MonoCompile *cfg, MonoMethod *method, MonoBasicBlock *start_bblock, MonoBasicBlock *end_bblock, 
		   int locals_offset, MonoInst *return_var, GList *dont_inline, MonoInst **inline_args, 
		   guint inline_offset, gboolean is_virtual_call);

extern guint8 mono_burg_arity [];
/* helper methods signature */
MonoMethodSignature *helper_sig_class_init_trampoline;
MonoMethodSignature *helper_sig_domain_get;

#define mono_jit_lock() EnterCriticalSection (&jit_mutex)
#define mono_jit_unlock() LeaveCriticalSection (&jit_mutex)
static CRITICAL_SECTION jit_mutex;

/*
 * Instruction metadata
 */
#ifdef MINI_OP
#undef MINI_OP
#endif
#define MINI_OP(a,b,dest,src1,src2) dest src1 src2,
#define NONE " "
#define IREG "i"
#define FREG "f"
#if SIZEOF_VOID_P == 8
#define LREG IREG
#else
#define LREG "l"
#endif
/* keep in sync with the enum in mini.h */
const char* const
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

static inline guint32
alloc_ireg (MonoCompile *cfg)
{
	return cfg->next_vireg ++;
}

static inline guint32
alloc_preg (MonoCompile *cfg)
{
	return alloc_ireg (cfg);
}

static inline guint32
alloc_freg (MonoCompile *cfg)
{
	/* Allocate these from the same pool as the int regs */
#if 1
	cfg->next_vfreg = cfg->next_vireg + 1;
	return cfg->next_vireg ++;
#else
	return cfg->next_vfreg ++;
#endif
}

static inline guint32
alloc_lreg (MonoCompile *cfg)
{
#if SIZEOF_VOID_P == 8
	return cfg->next_vireg ++;
#else
	/* Use a pair of consecutive vregs */
	guint32 res = cfg->next_vireg;

	cfg->next_vireg += 3;

	return res;
#endif
}

static inline guint32
alloc_dreg (MonoCompile *cfg, MonoStackType stack_type)
{
	switch (stack_type) {
	case STACK_I4:
	case STACK_PTR:
	case STACK_MP:
	case STACK_OBJ:
		return alloc_ireg (cfg);
	case STACK_R8:
		return alloc_freg (cfg);
	case STACK_I8:
		return alloc_lreg (cfg);
	case STACK_VTYPE:
		return -1;
	default:
		g_assert_not_reached ();
	}
}

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

static guint
mono_type_to_regstore (MonoType *type)
{
	if (type->byref)
		return OP_MOVE;

	/* FIXME: Handle truncation */
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
#if SIZEOF_VOID_P == 8
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
		return CEE_STOBJ;
	case MONO_TYPE_TYPEDBYREF:
		return CEE_STOBJ;
	case MONO_TYPE_GENERICINST:
		type = &type->data.generic_class->container_class->byval_arg;
		goto handle_enum;
	default:
		g_error ("unknown type 0x%02x in type_to_regstore", type->type);
	}
	return -1;
}

static guint
mono_type_to_regload (MonoType *type)
{
	if (type->byref)
		return OP_MOVE;

	/* FIXME: Handle widening */
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
#if SIZEOF_VOID_P == 8
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
		return CEE_LDOBJ;
	case MONO_TYPE_TYPEDBYREF:
		return CEE_LDOBJ;
	case MONO_TYPE_GENERICINST:
		type = &type->data.generic_class->container_class->byval_arg;
		goto handle_enum;
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

	printf ("\n%s %d: [", msg, bb->block_num);
	for (i = 0; i < bb->out_count; ++i)
		printf (" BB%d %d", bb->out_bb [i]->block_num, bb->out_bb [i]->dfn);
	printf (" ]\n");
	for (tree = bb->code; tree; tree = tree->next)
		mono_print_ins_index (-1, tree);
}

/*
 * Basic blocks have two numeric identifiers:
 * dfn: Depth First Number
 * block_num: unique ID assigned at bblock creation
 */
#define NEW_BBLOCK(cfg,bblock) do { \
	(bblock) = mono_mempool_alloc0 ((cfg)->mempool, sizeof (MonoBasicBlock)); \
	(bblock)->block_num = cfg->num_bblocks++; \
    } while (0)

#define ADD_BBLOCK(cfg,bbhash,b) do {	\
		g_hash_table_insert (bbhash, (b)->cil_code, (b));	\
		(b)->real_offset = cfg->real_offset;	\
	} while (0)

#define GET_BBLOCK(cfg,bbhash,tblock,ip) do {	\
		(tblock) = g_hash_table_lookup (bbhash, (ip));	\
		if (!(tblock)) {	\
			if ((ip) >= end || (ip) < header->code) goto unverified; \
            NEW_BBLOCK (cfg, (tblock)); \
			(tblock)->cil_code = (ip);	\
			ADD_BBLOCK (cfg, (bbhash), (tblock));	\
		} \
	} while (0)

#define CHECK_BBLOCK(target,ip,tblock) do {	\
		if ((target) < (ip) && !(tblock)->code)	{	\
			bb_recheck = g_list_prepend (bb_recheck, (tblock));	\
			if (cfg->verbose_level > 2) printf ("queued block %d for check at IL%04x from IL%04x\n", (tblock)->block_num, (int)((target) - header->code), (int)((ip) - header->code));	\
		}	\
	} while (0)

/*
 * IR Emission Macros
 */

#undef MONO_INST_NEW
/* 
 * FIXME: zeroing out some fields is not needed with the new IR, but the old 
 * JIT code still uses the left and right fields, so it has to stay.
 */
#define MONO_INST_NEW(cfg,dest,op) do {	\
		(dest) = mono_mempool_alloc ((cfg)->mempool, sizeof (MonoInst));	\
        (dest)->inst_p0 = (dest)->inst_p1 = (dest)->next = NULL; \
		(dest)->opcode = (op);	\
        (dest)->flags = 0; \
        (dest)->dreg = (dest)->sreg1 = (dest)->sreg2 = -1;  \
        (dest)->next = NULL; \
	} while (0)

/*
 * Variants which take a dest argument and do not do an emit
 */
#define NEW_ICONST(cfg,dest,val) do {	\
        MONO_INST_NEW ((cfg), (dest), OP_ICONST); \
		(dest)->inst_c0 = (val);	\
		(dest)->type = STACK_I4;	\
		(dest)->dreg = alloc_dreg ((cfg), STACK_I4);	\
	} while (0)

#define NEW_PCONST(cfg,dest,val) do {	\
        MONO_INST_NEW ((cfg), (dest), OP_PCONST); \
		(dest)->inst_p0 = (val);	\
		(dest)->type = STACK_PTR;	\
		(dest)->dreg = alloc_dreg ((cfg), STACK_PTR);	\
	} while (0)

#define NEW_STORE_MEMBASE(cfg,dest,op,base,offset,sr) do { \
        MONO_INST_NEW ((cfg), (dest), (op)); \
        (dest)->sreg1 = sr; \
        (dest)->inst_destbasereg = base; \
        (dest)->inst_offset = offset; \
	} while (0)

#define NEW_LOAD_MEMBASE(cfg,dest,op,dr,base,offset) do { \
        MONO_INST_NEW ((cfg), (dest), (op)); \
        (dest)->dreg = (dr); \
        (dest)->inst_basereg = (base); \
        (dest)->inst_offset = (offset); \
        (dest)->type = STACK_I4; \
	} while (0)

#define NEW_LOAD_MEM(cfg,dest,op,dr,mem) do { \
        MONO_INST_NEW ((cfg), (dest), (op)); \
        (dest)->dreg = (dr); \
        (dest)->inst_p0 = (gpointer)(gssize)(mem); \
        (dest)->type = STACK_I4; \
	} while (0)

#define NEW_BIALU_IMM(cfg,dest,op,dr,sr,imm) do { \
        MONO_INST_NEW ((cfg), (dest), (op)); \
        (dest)->dreg = dr; \
        (dest)->sreg1 = sr; \
        (dest)->inst_p1 = (gpointer)(gssize)(imm); \
	} while (0)

#ifdef MONO_ARCH_NEED_GOT_VAR

#define NEW_PATCH_INFO(cfg,dest,el1,el2) do {	\
        MONO_INST_NEW ((cfg), (dest), OP_PATCH_INFO); \
		(dest)->inst_left = (gpointer)(el1);	\
		(dest)->inst_right = (gpointer)(el2);	\
	} while (0)

/* FIXME: Add the PUSH_GOT_ENTRY optimizations */
#define NEW_AOTCONST(cfg,dest,patch_type,cons) do {			\
        MONO_INST_NEW ((cfg), (dest), cfg->compile_aot ? OP_GOT_ENTRY : OP_PCONST); \
		if (cfg->compile_aot) {					\
			MonoInst *group, *got_loc;		\
			got_loc = mono_get_got_var (cfg);		\
			NEW_PATCH_INFO ((cfg), group, cons, patch_type); \
			(dest)->inst_basereg = got_loc->dreg;			\
			(dest)->inst_p1 = group;			\
		} else {						\
			(dest)->inst_p0 = (cons);			\
			(dest)->inst_i1 = (gpointer)(patch_type);	\
		}							\
		(dest)->type = STACK_PTR;				\
		(dest)->dreg = alloc_dreg ((cfg), STACK_PTR);	\
	} while (0)

#define NEW_AOTCONST_TOKEN(cfg,dest,patch_type,image,token,stack_type) do { \
		MonoInst *group, *got_loc;			\
        MONO_INST_NEW ((cfg), (dest), OP_GOT_ENTRY); \
		got_loc = mono_get_got_var (cfg);			\
		NEW_PATCH_INFO ((cfg), group, NULL, patch_type);	\
		group->inst_p0 = mono_jump_info_token_new ((cfg)->mempool, (image), (token)); \
		(dest)->inst_basereg = got_loc->dreg;				\
		(dest)->inst_p1 = group;				\
		(dest)->type = (stack_type);				\
		(dest)->dreg = alloc_dreg ((cfg), (stack_type));	\
	} while (0)

#else

#define NEW_AOTCONST(cfg,dest,patch_type,cons) do {    \
        MONO_INST_NEW ((cfg), (dest), cfg->compile_aot ? OP_AOTCONST : OP_PCONST); \
		(dest)->inst_p0 = (cons);	\
		(dest)->inst_i1 = (gpointer)(patch_type); \
		(dest)->type = STACK_PTR;	\
		(dest)->dreg = alloc_dreg ((cfg), STACK_PTR);	\
    } while (0)

#define NEW_AOTCONST_TOKEN(cfg,dest,patch_type,image,token,stack_type) do {   \
        MONO_INST_NEW ((cfg), (dest), OP_AOTCONST); \
		(dest)->inst_p0 = mono_jump_info_token_new ((cfg)->mempool, (image), (token));	\
		(dest)->inst_p1 = (gpointer)(patch_type); \
		(dest)->type = (stack_type);	\
		(dest)->dreg = alloc_dreg ((cfg), (stack_type));	\
    } while (0)

#endif

#define NEW_CLASSCONST(cfg,dest,val) NEW_AOTCONST ((cfg), (dest), MONO_PATCH_INFO_CLASS, (val))

#define NEW_IMAGECONST(cfg,dest,val) NEW_AOTCONST ((cfg), (dest), MONO_PATCH_INFO_IMAGE, (val))

#define NEW_FIELDCONST(cfg,dest,val) NEW_AOTCONST ((cfg), (dest), MONO_PATCH_INFO_FIELD, (val))

#define NEW_METHODCONST(cfg,dest,val) NEW_AOTCONST ((cfg), (dest), MONO_PATCH_INFO_METHODCONST, (val))

#define NEW_VTABLECONST(cfg,dest,vtable) NEW_AOTCONST ((cfg), (dest), MONO_PATCH_INFO_VTABLE, cfg->compile_aot ? (gpointer)((vtable)->klass) : (vtable))

#define NEW_SFLDACONST(cfg,dest,val) NEW_AOTCONST ((cfg), (dest), MONO_PATCH_INFO_SFLDA, (val))

#define NEW_LDSTRCONST(cfg,dest,image,token) NEW_AOTCONST_TOKEN ((cfg), (dest), MONO_PATCH_INFO_LDSTR, (image), (token), STACK_OBJ)

#define NEW_TYPE_FROM_HANDLE_CONST(cfg,dest,image,token) NEW_AOTCONST_TOKEN ((cfg), (dest), MONO_PATCH_INFO_TYPE_FROM_HANDLE, (image), (token), STACK_OBJ)

#define NEW_LDTOKENCONST(cfg,dest,image,token) NEW_AOTCONST_TOKEN ((cfg), (dest), MONO_PATCH_INFO_LDTOKEN, (image), (token), STACK_PTR)

#define NEW_DECLSECCONST(cfg,dest,image,entry) do { \
		if (cfg->compile_aot) { \
			NEW_AOTCONST_TOKEN (cfg, dest, MONO_PATCH_INFO_DECLSEC, image, (entry).index, STACK_OBJ); \
		} else { \
			NEW_PCONST (cfg, args [0], (entry).blob); \
		} \
	} while (0)

#define NEW_DOMAINCONST(cfg,dest) do { \
		if (cfg->opt & MONO_OPT_SHARED) { \
			/* avoid depending on undefined C behavior in sequence points */ \
			MonoInst* __domain_var = mono_get_domainvar (cfg); \
			NEW_TEMPLOAD (cfg, dest, __domain_var->inst_c0); \
		} else { \
			NEW_PCONST (cfg, dest, (cfg)->domain); \
		} \
	} while (0)

#define GET_VARINFO_INST(cfg,num) ((cfg)->varinfo [(num)]->inst)

#define NEW_VARLOAD(cfg,dest,var,vartype) do { \
        MONO_INST_NEW ((cfg), (dest), OP_MOVE); \
		(dest)->opcode = mono_type_to_regload ((vartype));  \
		(dest)->ssa_op = MONO_SSA_LOAD;	\
		type_to_eval_stack_type ((vartype), (dest));	\
		(dest)->klass = var->klass;	\
		(dest)->sreg1 = var->dreg;   \
        (dest)->dreg = alloc_dreg ((cfg), (dest)->type); \
	} while (0)

#define NEW_VARLOADA(cfg,dest,var,vartype) do {	\
        MONO_INST_NEW ((cfg), (dest), OP_LDADDR); \
		(dest)->ssa_op = MONO_SSA_ADDRESS_TAKEN;	\
		(dest)->inst_p0 = (var); \
		(var)->flags |= MONO_INST_INDIRECT;	\
		(dest)->type = STACK_MP;	\
		(dest)->klass = (var)->klass;	\
        (dest)->dreg = alloc_dreg ((cfg), STACK_MP); \
        if (!MONO_TYPE_ISSTRUCT (vartype)) \
           (cfg)->disable_ssa = TRUE; \
	} while (0)

#define NEW_VARSTORE(cfg,dest,var,vartype,inst) do {	\
        MONO_INST_NEW ((cfg), (dest), OP_MOVE); \
		(dest)->opcode = mono_type_to_regstore ((vartype));    \
		(dest)->ssa_op = MONO_SSA_STORE;	\
		(dest)->klass = (var)->klass;	\
        (dest)->sreg1 = (inst)->dreg; \
		(dest)->dreg = (var)->dreg;   \
	} while (0)

#define NEW_TEMPLOAD(cfg,dest,num) NEW_VARLOAD ((cfg), (dest), (cfg)->varinfo [(num)], (cfg)->varinfo [(num)]->inst_vtype)

#define NEW_TEMPLOADA(cfg,dest,num) NEW_VARLOADA ((cfg), (dest), cfg->varinfo [(num)], cfg->varinfo [(num)]->inst_vtype)

#define NEW_TEMPSTORE(cfg,dest,num,inst) NEW_VARSTORE ((cfg), (dest), (cfg)->varinfo [(num)], (cfg)->varinfo [(num)]->inst_vtype, (inst))

#define NEW_LOCSTORE(cfg,dest,num,inst) NEW_TEMPSTORE ((cfg), (dest), locals_offset + (num), (inst))

#define NEW_ARGLOAD(cfg,dest,num) NEW_VARLOAD ((cfg), (dest), arg_array [(num)], param_types [(num)])

#define NEW_LOCLOAD(cfg,dest,num) NEW_VARLOAD ((cfg), (dest), cfg->varinfo [locals_offset + (num)], header->locals [(num)])

#define NEW_LOCLOADA(cfg,dest,num) NEW_VARLOADA ((cfg), (dest), (cfg)->varinfo [locals_offset + (num)], (cfg)->varinfo [locals_offset + (num)]->inst_vtype)

#define NEW_RETLOADA(cfg,dest) do {	\
        if (cfg->ret_var_is_local) { \
			NEW_VARLOADA ((cfg), (dest), (cfg)->ret, (cfg)->ret->inst_vtype); \
        } else { \
            MONO_INST_NEW ((cfg), (dest), OP_MOVE); \
		    (dest)->ssa_op = MONO_SSA_LOAD;	\
            (dest)->type = STACK_MP; \
		    (dest)->klass = cfg->ret->klass;	\
		    (dest)->sreg1 = cfg->ret->dreg;   \
            (dest)->dreg = alloc_dreg ((cfg), (dest)->type); \
        } \
	} while (0)

#define NEW_ARGLOADA(cfg,dest,num) NEW_VARLOADA ((cfg), (dest), arg_array [(num)], param_types [(num)])

#define NEW_ARGSTORE(cfg,dest,num,inst) NEW_VARSTORE ((cfg), (dest), arg_array [(num)], param_types [(num)], (inst))

#define NEW_DUMMY_USE(cfg,dest,var) do { \
        MONO_INST_NEW ((cfg), (dest), OP_DUMMY_USE); \
		(dest)->sreg1 = var->dreg; \
    } while (0)

#define NEW_DUMMY_STORE(cfg,dest,num) do { \
        MONO_INST_NEW ((cfg), (dest), OP_DUMMY_STORE); \
		(dest)->inst_i0 = (cfg)->varinfo [(num)];	\
		(dest)->klass = (dest)->inst_i0->klass;	\
	} while (0)

/*
 * Variants which do an emit as well.
 */
#define EMIT_NEW_ICONST(cfg,dest,val) do { \
        NEW_ICONST ((cfg), (dest), (val)); \
        MONO_ADD_INS ((cfg)->cbb, (dest)); \
    } while (0)

#define EMIT_NEW_PCONST(cfg,dest,val) do {	\
        NEW_PCONST ((cfg), (dest), (val)); \
        MONO_ADD_INS ((cfg)->cbb, (dest)); \
    } while (0)

#define EMIT_NEW_AOTCONST(cfg,dest,patch_type,cons) do { \
        NEW_AOTCONST ((cfg), (dest), (patch_type), (cons)); \
        MONO_ADD_INS ((cfg)->cbb, (dest)); \
    } while (0)

#define EMIT_NEW_AOTCONST_TOKEN(cfg,dest,patch_type,image,token,stack_type) do { \
        NEW_AOTCONST_TOKEN ((cfg), (dest), (patch_type), (image), (token), (stack_type)); \
        MONO_ADD_INS ((cfg)->cbb, (dest)); \
    } while (0)

#define EMIT_NEW_CLASSCONST(cfg,dest,val) EMIT_NEW_AOTCONST ((cfg), (dest), MONO_PATCH_INFO_CLASS, (val))

#define EMIT_NEW_IMAGECONST(cfg,dest,val) EMIT_NEW_AOTCONST ((cfg), (dest), MONO_PATCH_INFO_IMAGE, (val))

#define EMIT_NEW_FIELDCONST(cfg,dest,val) EMIT_NEW_AOTCONST ((cfg), (dest), MONO_PATCH_INFO_FIELD, (val))

#define EMIT_NEW_METHODCONST(cfg,dest,val) EMIT_NEW_AOTCONST ((cfg), (dest), MONO_PATCH_INFO_METHODCONST, (val))

#define EMIT_NEW_VTABLECONST(cfg,dest,vtable) EMIT_NEW_AOTCONST ((cfg), (dest), MONO_PATCH_INFO_VTABLE, cfg->compile_aot ? (gpointer)((vtable)->klass) : (vtable))

#define EMIT_NEW_SFLDACONST(cfg,dest,val) EMIT_NEW_AOTCONST ((cfg), (dest), MONO_PATCH_INFO_SFLDA, (val))

#define EMIT_NEW_LDSTRCONST(cfg,dest,image,token) EMIT_NEW_AOTCONST_TOKEN ((cfg), (dest), MONO_PATCH_INFO_LDSTR, (image), (token), STACK_OBJ)

#define EMIT_NEW_TYPE_FROM_HANDLE_CONST(cfg,dest,image,token) EMIT_NEW_AOTCONST_TOKEN ((cfg), (dest), MONO_PATCH_INFO_TYPE_FROM_HANDLE, (image), (token), STACK_OBJ)

#define EMIT_NEW_LDTOKENCONST(cfg,dest,image,token) EMIT_NEW_AOTCONST_TOKEN ((cfg), (dest), MONO_PATCH_INFO_LDTOKEN, (image), (token), STACK_PTR)

#define EMIT_NEW_DOMAINCONST(cfg,dest) do { NEW_DOMAINCONST ((cfg), (dest)); MONO_ADD_INS ((cfg)->cbb, (dest)); } while (0)

#define EMIT_NEW_TEMPLOAD(cfg,dest,num) do { NEW_TEMPLOAD ((cfg), (dest), (num)); MONO_ADD_INS ((cfg)->cbb, (dest)); } while (0)

#define EMIT_NEW_TEMPLOADA(cfg,dest,num) do { NEW_TEMPLOADA ((cfg), (dest), (num)); MONO_ADD_INS ((cfg)->cbb, (dest)); } while (0)

#define EMIT_NEW_LOCLOADA(cfg,dest,num) do { NEW_LOCLOADA ((cfg), (dest), (num)); MONO_ADD_INS ((cfg)->cbb, (dest)); } while (0)

#define EMIT_NEW_ARGLOADA(cfg,dest,num) do { NEW_ARGLOADA ((cfg), (dest), (num)); MONO_ADD_INS ((cfg)->cbb, (dest)); } while (0)

#define EMIT_NEW_RETLOADA(cfg,dest) do { NEW_RETLOADA ((cfg), (dest)); MONO_ADD_INS ((cfg)->cbb, (dest)); } while (0)

#define EMIT_NEW_DUMMY_USE(cfg,dest,var) do { NEW_DUMMY_USE ((cfg), (dest), (var)); MONO_ADD_INS ((cfg)->cbb, (dest)); } while (0)

#define EMIT_NEW_UNALU(cfg,dest,op,dr,sr1) do { \
        MONO_INST_NEW ((cfg), (dest), (op)); \
        (dest)->opcode = op; \
        (dest)->dreg = dr; \
        (dest)->sreg1 = sr1; \
	    MONO_ADD_INS (cfg->cbb, (dest)); \
    } while (0)        

#define EMIT_NEW_BIALU_IMM(cfg,dest,op,dr,sr,imm) do { \
        NEW_BIALU_IMM ((cfg), (dest), (op), (dr), (sr), (imm)); \
        MONO_ADD_INS ((cfg)->cbb, (dest)); \
	} while (0)

#define EMIT_NEW_LOAD_MEMBASE(cfg,dest,op,dr,base,offset) do { \
        MONO_INST_NEW ((cfg), (dest), (op)); \
        (dest)->dreg = (dr); \
        (dest)->inst_basereg = (base); \
        (dest)->inst_offset = (offset); \
        (dest)->type = STACK_I4; \
        MONO_ADD_INS (cfg->cbb, (dest)); \
	} while (0)

/*
 * Variants which do not take an dest argument, but take a dreg argument.
 */
#define	MONO_EMIT_NEW_ICONST(cfg,dr,imm) do { \
        MonoInst *inst; \
        MONO_INST_NEW ((cfg), (inst), OP_ICONST); \
        inst->dreg = dr; \
        inst->inst_c0 = imm; \
	    MONO_ADD_INS ((cfg)->cbb, inst); \
	} while (0)

#define MONO_EMIT_NEW_PCONST(cfg,dr,val) do {	\
        MonoInst *inst; \
        MONO_INST_NEW ((cfg), (inst), OP_PCONST); \
        inst->dreg = dr; \
		(inst)->inst_p0 = (val);	\
		(inst)->type = STACK_PTR;	\
	    MONO_ADD_INS ((cfg)->cbb, inst); \
	} while (0)

#define	MONO_EMIT_NEW_I8CONST(cfg,dr,imm) do { \
        MonoInst *inst; \
        MONO_INST_NEW ((cfg), (inst), OP_I8CONST); \
        inst->dreg = dr; \
        inst->inst_l = imm; \
	    MONO_ADD_INS ((cfg)->cbb, inst); \
	} while (0)

#ifdef MONO_ARCH_NEED_GOT_VAR

#define MONO_EMIT_NEW_AOTCONST(cfg,dr,cons,patch_type) do { \
        MonoInst *inst; \
        NEW_AOTCONST ((cfg), (inst), (patch_type), (cons)); \
        inst->dreg = (dr); \
        MONO_ADD_INS ((cfg)->cbb, inst); \
    } while (0)

#else

#define	MONO_EMIT_NEW_AOTCONST(cfg,dr,imm,type) do { \
        MonoInst *inst; \
        MONO_INST_NEW ((cfg), (inst), OP_AOTCONST); \
        inst->dreg = dr; \
        inst->inst_p0 = imm; \
        inst->inst_c1 = type; \
		MONO_ADD_INS ((cfg)->cbb, inst); \
	} while (0)

#endif

#define	MONO_EMIT_NEW_CLASSCONST(cfg,dr,imm) MONO_EMIT_NEW_AOTCONST(cfg,dr,imm,MONO_PATCH_INFO_CLASS)
#define MONO_EMIT_NEW_VTABLECONST(cfg,dest,vtable) MONO_EMIT_NEW_AOTCONST ((cfg), (dest), (cfg)->compile_aot ? (gpointer)((vtable)->klass) : (vtable), MONO_PATCH_INFO_VTABLE)

#define MONO_EMIT_NEW_UNALU(cfg,op,dr,sr1) do { \
        MonoInst *inst; \
        EMIT_NEW_UNALU ((cfg), (inst), (op), (dr), (sr1)); \
	} while (0)

#define MONO_EMIT_NEW_BIALU(cfg,op,dr,sr1,sr2) do { \
        MonoInst *inst; \
        MONO_INST_NEW ((cfg), (inst), (op)); \
        inst->dreg = dr; \
        inst->sreg1 = sr1; \
        inst->sreg2 = sr2; \
	    MONO_ADD_INS (cfg->cbb, inst); \
	} while (0)

#define MONO_EMIT_NEW_BIALU_IMM(cfg,op,dr,sr,imm) do { \
        MonoInst *inst; \
        MONO_INST_NEW ((cfg), (inst), (op)); \
        inst->dreg = dr; \
        inst->sreg1 = sr; \
        inst->inst_p1 = (gpointer)(gssize)(imm); \
	    MONO_ADD_INS (cfg->cbb, inst); \
	} while (0)

#define	MONO_EMIT_NEW_COMPARE_IMM(cfg,sr1,imm) do { \
        MonoInst *inst; \
        MONO_INST_NEW ((cfg), (inst), (OP_COMPARE_IMM)); \
        inst->sreg1 = sr1; \
        inst->inst_p1 = (gpointer)imm; \
	    MONO_ADD_INS ((cfg)->cbb, inst); \
	} while (0)

#define	MONO_EMIT_NEW_ICOMPARE_IMM(cfg,sr1,imm) do { \
        MonoInst *inst; \
        MONO_INST_NEW ((cfg), (inst), sizeof (void*) == 8 ? OP_ICOMPARE_IMM : OP_COMPARE_IMM); \
        inst->sreg1 = sr1; \
        inst->inst_p1 = (gpointer)imm; \
	    MONO_ADD_INS ((cfg)->cbb, inst); \
	} while (0)

#define MONO_EMIT_NEW_LOAD_MEMBASE_OP(cfg,op,dr,base,offset) do { \
        MonoInst *inst; \
        MONO_INST_NEW ((cfg), (inst), (op)); \
        inst->dreg = dr; \
        inst->inst_basereg = base; \
        inst->inst_offset = offset; \
	    MONO_ADD_INS (cfg->cbb, inst); \
    } while (0)

#define MONO_EMIT_NEW_LOAD_MEMBASE(cfg,dr,base,offset) MONO_EMIT_NEW_LOAD_MEMBASE_OP ((cfg), (OP_LOAD_MEMBASE), (dr), (base), (offset))

#define MONO_EMIT_NEW_STORE_MEMBASE(cfg,op,base,offset,sr) do { \
        MonoInst *inst; \
        MONO_INST_NEW ((cfg), (inst), (op)); \
        (inst)->sreg1 = sr; \
        (inst)->inst_destbasereg = base; \
        (inst)->inst_offset = offset; \
	    MONO_ADD_INS (cfg->cbb, inst); \
	} while (0)

#define MONO_EMIT_NEW_STORE_MEMBASE_IMM(cfg,op,base,offset,imm) do { \
        MonoInst *inst; \
        MONO_INST_NEW ((cfg), (inst), (op)); \
        inst->inst_destbasereg = base; \
        inst->inst_offset = offset; \
        inst->inst_p1 = (gpointer)(gssize)imm; \
        MONO_ADD_INS ((cfg)->cbb, inst); \
	} while (0)

#define	MONO_EMIT_NEW_COND_EXC(cfg,cond,name) do { \
        MonoInst *inst; \
        MONO_INST_NEW ((cfg), (inst), (OP_COND_EXC_##cond)); \
        inst->inst_p1 = (char*)name; \
	    MONO_ADD_INS ((cfg)->cbb, inst); \
	} while (0)

#define MONO_NEW_LABEL(cfg,inst) do { \
        MONO_INST_NEW ((cfg), (inst), OP_LABEL); \
	} while (0)

/* Emit a one-way conditional branch and start a new basic block */
#define	MONO_EMIT_NEW_BRANCH_BLOCK(cfg,op,truebb) do { \
        MonoInst *ins; \
        MonoBasicBlock *falsebb; \
	    NEW_BBLOCK ((cfg), falsebb); \
        MONO_INST_NEW ((cfg), (ins), (op)); \
        if ((op) == OP_BR) { \
            ins->inst_target_bb = (truebb); \
        } else { \
		    ins->inst_many_bb = mono_mempool_alloc (cfg->mempool, sizeof(gpointer)*2);	\
            ins->inst_true_bb = (truebb); \
            ins->inst_false_bb = (falsebb); \
        } \
        link_bblock ((cfg), (cfg)->cbb, (truebb)); \
        link_bblock ((cfg), (cfg)->cbb, (falsebb)); \
        MONO_ADD_INS ((cfg)->cbb, ins); \
        MONO_START_BB ((cfg), falsebb); \
	} while (0)

/* Emit a two-way conditional branch */
#define	MONO_EMIT_NEW_BRANCH_BLOCK2(cfg,op,truebb,falsebb) do { \
        MonoInst *ins; \
        MONO_INST_NEW ((cfg), (ins), (op)); \
		ins->inst_many_bb = mono_mempool_alloc (cfg->mempool, sizeof(gpointer)*2);	\
        ins->inst_true_bb = (truebb); \
        ins->inst_false_bb = (falsebb); \
        link_bblock ((cfg), (cfg)->cbb, (truebb)); \
        link_bblock ((cfg), (cfg)->cbb, (falsebb)); \
        MONO_ADD_INS ((cfg)->cbb, ins); \
	} while (0)

#define MONO_START_BB(cfg, bblock) do { \
        ADD_BBLOCK ((cfg), (cfg)->cbb_hash, (bblock)); \
        link_bblock ((cfg), (cfg)->cbb, (bblock)); \
	    (cfg)->cbb->next_bb = (bblock); \
	    (cfg)->cbb = (bblock); \
    } while (0)

#define MONO_EMIT_BOUNDS_CHECK(cfg, array_reg, array_type, array_length_field, index_reg) do { \
			int _length_reg = alloc_ireg (cfg); \
			MONO_EMIT_NEW_LOAD_MEMBASE_OP (cfg, OP_LOADI4_MEMBASE, _length_reg, array_reg, G_STRUCT_OFFSET (array_type, array_length_field)); \
			MONO_EMIT_NEW_BIALU (cfg, OP_COMPARE, -1, _length_reg, index_reg); \
			MONO_EMIT_NEW_COND_EXC (cfg, LE_UN, "IndexOutOfRangeException"); \
	} while (0)

#define ADD_BINOP(op) do {	\
		MONO_INST_NEW (cfg, ins, (op));	\
		ins->cil_code = ip;	\
		sp -= 2;	\
		ins->sreg1 = sp [0]->dreg;	\
		ins->sreg2 = sp [1]->dreg;	\
		type_from_op (ins, sp [0], sp [1]);	\
		CHECK_TYPE (ins);	\
        ins->dreg = alloc_dreg ((cfg), (ins)->type); \
        MONO_ADD_INS ((cfg)->cbb, (ins)); \
		*sp++ = ins;	\
        decompose_opcode ((cfg), (ins)); \
	} while (0)

#define ADD_UNOP(op) do {	\
		MONO_INST_NEW (cfg, ins, (op));	\
		ins->cil_code = ip;	\
		sp--;	\
		ins->sreg1 = sp [0]->dreg;	\
		type_from_op (ins, sp [0], NULL);	\
		CHECK_TYPE (ins);	\
        (ins)->dreg = alloc_dreg ((cfg), (ins)->type); \
        MONO_ADD_INS ((cfg)->cbb, (ins)); \
		*sp++ = ins;	\
		decompose_opcode (cfg, ins); \
	} while (0)

#define ADD_BINCOND(next_block) do {	\
		MonoInst *cmp;	\
		sp -= 2; \
		MONO_INST_NEW(cfg, cmp, OP_COMPARE);	\
		cmp->sreg1 = sp [0]->dreg;	\
		cmp->sreg2 = sp [1]->dreg;	\
		cmp->cil_code = ins->cil_code;	\
		type_from_op (cmp, sp [0], sp [1]);	\
		CHECK_TYPE (cmp);	\
		type_from_op (ins, sp [0], sp [1]);	\
		ins->inst_many_bb = mono_mempool_alloc (cfg->mempool, sizeof(gpointer)*2);	\
		GET_BBLOCK (cfg, bbhash, tblock, target);		\
		link_bblock (cfg, bblock, tblock);	\
		ins->inst_true_bb = tblock;	\
		CHECK_BBLOCK (target, ip, tblock);	\
		if ((next_block)) {	\
			link_bblock (cfg, bblock, (next_block));	\
			ins->inst_false_bb = (next_block);	\
			start_new_bblock = 1;	\
		} else {	\
			GET_BBLOCK (cfg, bbhash, tblock, ip);		\
			link_bblock (cfg, bblock, tblock);	\
			ins->inst_false_bb = tblock;	\
			start_new_bblock = 2;	\
		}	\
		if (sp != stack_start) \
		    handle_stack_args (cfg, stack_start, sp - stack_start); \
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
			if (clause->flags & MONO_EXCEPTION_CLAUSE_FINALLY)
				return ((i + 1) << 8) | MONO_REGION_FINALLY | clause->flags;
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
				handler = g_hash_table_lookup (cfg->bb_hash, header->code + clause->handler_offset);
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

static void
df_visit (MonoBasicBlock *start, int *dfn, MonoBasicBlock **array)
{
	int i;

	array [*dfn] = start;
	/*printf ("visit %d at %p (BB%ld)\n", *dfn, start->cil_code, start->block_num);*/
	for (i = 0; i < start->out_count; ++i) {
		if (start->out_bb [i]->dfn)
			continue;
		(*dfn)++;
		start->out_bb [i]->dfn = *dfn;
		start->out_bb [i]->df_parent = start;
		array [*dfn] = start->out_bb [i];
		df_visit (start->out_bb [i], dfn, array);
	}
}

typedef struct {
	const guchar *code;
	MonoBasicBlock *best;
} PrevStruct;

static void
previous_foreach (gconstpointer key, gpointer val, gpointer data)
{
	PrevStruct *p = data;
	MonoBasicBlock *bb = val;
	//printf ("FIDPREV %d %p  %p %p %p %p %d %d %d\n", bb->block_num, p->code, bb, p->best, bb->cil_code, p->best->cil_code,
	//bb->method == p->best->method, bb->cil_code < p->code, bb->cil_code > p->best->cil_code);

	if (bb->cil_code && bb->cil_code < p->code && bb->cil_code > p->best->cil_code)
		p->best = bb;
}

static MonoBasicBlock*
find_previous (GHashTable *bb_hash, MonoBasicBlock *start, const guchar *code) {
	PrevStruct p;

	p.code = code;
	p.best = start;

	g_hash_table_foreach (bb_hash, (GHFunc)previous_foreach, &p);
	return p.best;
}

static void
split_bblock (MonoCompile *cfg, MonoBasicBlock *first, MonoBasicBlock *second) {
	int i, j;
	MonoInst *inst;
	MonoBasicBlock *bb;

	if (second->code)
		return;
	
	/* 
	 * FIXME: take into account all the details:
	 * second may have been the target of more than one bblock
	 */
	second->out_count = first->out_count;
	second->out_bb = first->out_bb;

	for (i = 0; i < first->out_count; ++i) {
		bb = first->out_bb [i];
		for (j = 0; j < bb->in_count; ++j) {
			if (bb->in_bb [j] == first)
				bb->in_bb [j] = second;
		}
	}

	first->out_count = 0;
	first->out_bb = NULL;
	link_bblock (cfg, first, second);

	second->last_ins = first->last_ins;

	/*printf ("start search at %p for %p\n", first->cil_code, second->cil_code);*/
	for (inst = first->code; inst && inst->next; inst = inst->next) {
		/*char *code = mono_disasm_code_one (NULL, cfg->method, inst->next->cil_code, NULL);
		printf ("found %p: %s", inst->next->cil_code, code);
		g_free (code);*/
		if (inst->cil_code < second->cil_code && inst->next->cil_code >= second->cil_code) {
			second->code = inst->next;
			inst->next = NULL;
			first->last_ins = inst;
			second->next_bb = first->next_bb;
			first->next_bb = second;
			return;
		}
	}
	if (!second->code) {
		g_warning ("bblock split failed in %s::%s\n", cfg->method->klass->name, cfg->method->name);
		//G_BREAKPOINT ();
	}
}

/*
 * Returns the type used in the eval stack when @type is loaded.
 * FIXME: return a MonoType/MonoClass for the byref and VALUETYPE cases.
 */
static void
type_to_eval_stack_type (MonoType *type, MonoInst *inst)
{
	MonoClass *klass;

	if (type->byref) {
		inst->type = STACK_MP;
		return;
	}

	klass = mono_class_from_mono_type (type);

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
	{0},
	{0, 1, 0, 1, 0, 0, 4, 0},
	{0, 0, 1, 0, 0, 0, 0, 0},
	{0, 1, 0, 1, 0, 2, 4, 0},
	{0, 0, 0, 0, 1, 0, 0, 0},
	{0, 0, 0, 2, 0, 1, 0, 0},
	{0, 4, 0, 4, 0, 0, 3, 0},
	{0, 0, 0, 0, 0, 0, 0, 0},
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
		return;
	case CEE_DIV_UN:
	case CEE_REM_UN:
	case CEE_AND:
	case CEE_OR:
	case CEE_XOR:
		ins->type = bin_int_table [src1->type] [src2->type];
		ins->opcode += binops_op_map [ins->type];
		return;
	case CEE_SHL:
	case CEE_SHR:
	case CEE_SHR_UN:
		ins->type = shift_table [src1->type] [src2->type];
		ins->opcode += binops_op_map [ins->type];
		return;
	case OP_COMPARE:
	case OP_LCOMPARE:
	case OP_ICOMPARE:
		ins->type = bin_comp_table [src1->type] [src2->type] ? STACK_I4: STACK_INV;
		if ((src1->type == STACK_I8) || ((sizeof (gpointer) == 8) && ((src1->type == STACK_PTR) || (src1->type == STACK_OBJ) || (src1->type == STACK_MP))))
			ins->opcode = OP_LCOMPARE;
		else if (src1->type == STACK_R8)
			ins->opcode = OP_FCOMPARE;
		else
			ins->opcode = OP_ICOMPARE;
		return;
	case OP_ICOMPARE_IMM:
		ins->type = bin_comp_table [src1->type] [src1->type] ? STACK_I4 : STACK_INV;
		if ((src1->type == STACK_I8) || ((sizeof (gpointer) == 8) && ((src1->type == STACK_PTR) || (src1->type == STACK_OBJ) || (src1->type == STACK_MP))))
			ins->opcode = OP_LCOMPARE_IMM;		
		return;
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
		return;
		break;
	case OP_CEQ:
	case OP_CGT:
	case OP_CGT_UN:
	case OP_CLT:
	case OP_CLT_UN:
		ins->type = bin_comp_table [src1->type] [src2->type] ? STACK_I4: STACK_INV;
		ins->opcode += ceqops_op_map [src1->type];
		return;
	/* unops */
	case CEE_NEG:
		ins->type = neg_table [src1->type];
		ins->opcode += unops_op_map [ins->type];
		return;
	case CEE_NOT:
		if (src1->type >= STACK_I4 && src1->type <= STACK_PTR)
			ins->type = src1->type;
		else
			ins->type = STACK_INV;
		ins->opcode += unops_op_map [ins->type];
		return;
	case CEE_CONV_I1:
	case CEE_CONV_I2:
	case CEE_CONV_I4:
	case CEE_CONV_U4:
		ins->type = STACK_I4;
		ins->opcode += unops_op_map [src1->type];
		return;
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
		return;
	case CEE_CONV_OVF_I1:
	case CEE_CONV_OVF_U1:
	case CEE_CONV_OVF_I2:
	case CEE_CONV_OVF_U2:
	case CEE_CONV_OVF_I4:
	case CEE_CONV_OVF_U4:
		ins->type = STACK_I4;
		ins->opcode += ovf3ops_op_map [src1->type];
		return;
	case CEE_CONV_OVF_I_UN:
	case CEE_CONV_OVF_U_UN:
		ins->type = STACK_PTR;
		ins->opcode += ovf2ops_op_map [src1->type];
		return;
	case CEE_CONV_OVF_I1_UN:
	case CEE_CONV_OVF_I2_UN:
	case CEE_CONV_OVF_I4_UN:
	case CEE_CONV_OVF_U1_UN:
	case CEE_CONV_OVF_U2_UN:
	case CEE_CONV_OVF_U4_UN:
		ins->type = STACK_I4;
		ins->opcode += ovf2ops_op_map [src1->type];
		return;
	case CEE_CONV_I8:
	case CEE_CONV_U8:
		ins->type = STACK_I8;
		ins->opcode += unops_op_map [src1->type];
		return;
	case CEE_CONV_OVF_I8:
	case CEE_CONV_OVF_U8:
		ins->type = STACK_I8;
		ins->opcode += ovf3ops_op_map [src1->type];
		return;
	case CEE_CONV_OVF_U8_UN:
	case CEE_CONV_OVF_I8_UN:
		ins->type = STACK_I8;
		ins->opcode += ovf2ops_op_map [src1->type];
		return;
	case CEE_CONV_R4:
	case CEE_CONV_R8:
		ins->type = STACK_R8;
		ins->opcode += unops_op_map [src1->type];
		return;
	case CEE_CKFINITE:
		ins->type = STACK_R8;		
		return;
	case CEE_CONV_U2:
	case CEE_CONV_U1:
		ins->type = STACK_I4;
		ins->opcode += ovfops_op_map [src1->type];
		break;
	case CEE_CONV_I:
	case CEE_CONV_U:
	case CEE_CONV_OVF_I:
	case CEE_CONV_OVF_U:
		ins->type = STACK_PTR;
		ins->opcode += ovfops_op_map [src1->type];
		return;
	case CEE_ADD_OVF:
	case CEE_ADD_OVF_UN:
	case CEE_MUL_OVF:
	case CEE_MUL_OVF_UN:
	case CEE_SUB_OVF:
	case CEE_SUB_OVF_UN:
		ins->type = bin_num_table [src1->type] [src2->type];
		ins->opcode += ovfops_op_map [src1->type];
		return;
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
}

static const char 
ldind_type [] = {
	STACK_I4, STACK_I4, STACK_I4, STACK_I4, STACK_I4, STACK_I4, STACK_I8, STACK_MP, STACK_R8, STACK_R8, STACK_OBJ
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

static MonoType*
type_from_stack_type (MonoInst *ins) {
	switch (ins->type) {
	case STACK_I4: return &mono_defaults.int32_class->byval_arg;
	case STACK_I8: return &mono_defaults.int64_class->byval_arg;
	case STACK_PTR: return &mono_defaults.int_class->byval_arg;
	case STACK_R8: return &mono_defaults.double_class->byval_arg;
	case STACK_MP: return &mono_defaults.int_class->byval_arg;
	case STACK_OBJ: return &mono_defaults.object_class->byval_arg;
	case STACK_VTYPE: return &ins->klass->byval_arg;
	default:
		g_error ("stack type %d to montype not handled\n", ins->type);
	}
	return NULL;
}

static int
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
 */
static int
handle_stack_args (MonoCompile *cfg, MonoInst **sp, int count) {
	int i, bindex;
	MonoBasicBlock *bb = cfg->cbb;
	MonoBasicBlock *outb;
	MonoInst *inst, **locals;
	gboolean found;

	if (!count)
		return 0;
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
			if (outb->in_scount != bb->out_scount)
				G_BREAKPOINT ();
			continue; /* check they are the same locals */
		}
		outb->in_scount = count;
		outb->in_stack = bb->out_stack;
	}

	locals = bb->out_stack;
	cfg->cbb = bb;
	for (i = 0; i < count; ++i) {
		NEW_TEMPSTORE (cfg, inst, locals [i]->inst_c0, sp [i]);
		if (inst->opcode == CEE_STOBJ) {
			EMIT_NEW_TEMPLOADA (cfg, inst, locals [i]->inst_c0);
			emit_stobj (cfg, inst, sp [i], sp [i]->cil_code, inst->klass, FALSE);
			sp [i] = inst;
		} else {
			inst->cil_code = sp [i]->cil_code;
			MONO_ADD_INS (cfg->cbb, inst);
			sp [i] = locals [i];
		}
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
					NEW_TEMPSTORE (cfg, inst, outb->in_stack [i]->inst_c0, sp [i]);
					if (inst->opcode == CEE_STOBJ) {
						EMIT_NEW_TEMPLOADA (cfg, inst, outb->in_stack [i]->inst_c0);
						emit_stobj (cfg, inst, sp [i], sp [i]->cil_code, inst->klass, FALSE);
						sp [i] = inst;
					} else {
						inst->cil_code = sp [i]->cil_code;
						MONO_ADD_INS (cfg->cbb, inst);
						sp [i] = locals [i];
					}
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
	
	return 0;
}

static void
mini_emit_load_intf_reg (MonoCompile *cfg, int intf_reg, int ioffset_reg, MonoClass *klass)
{
	if (cfg->compile_aot) {
		int iid_reg = alloc_preg (cfg);
		MONO_EMIT_NEW_AOTCONST (cfg, iid_reg, klass, MONO_PATCH_INFO_IID);
#if SIZEOF_VOID_P == 8
		MONO_EMIT_NEW_BIALU_IMM (cfg, OP_SHL_IMM, iid_reg, iid_reg, 3);
#else
		MONO_EMIT_NEW_BIALU_IMM (cfg, OP_SHL_IMM, iid_reg, iid_reg, 2);
#endif
		MONO_EMIT_NEW_BIALU (cfg, OP_PADD, ioffset_reg, ioffset_reg, iid_reg);
		MONO_EMIT_NEW_LOAD_MEMBASE (cfg, intf_reg, ioffset_reg, 0);
	}
	else
		MONO_EMIT_NEW_LOAD_MEMBASE (cfg, intf_reg, ioffset_reg, klass->interface_id * SIZEOF_VOID_P);
}

/* Emit code which loads <vtable_reg>->interface_offsets [klass->interface_id] */
static void
mini_emit_load_intf_reg_vtable (MonoCompile *cfg, int intf_reg, int vtable_reg, MonoClass *klass)
{
	int ioffset_reg = alloc_preg (cfg);

	MONO_EMIT_NEW_LOAD_MEMBASE (cfg, ioffset_reg, vtable_reg, G_STRUCT_OFFSET (MonoVTable, interface_offsets));
	mini_emit_load_intf_reg (cfg, intf_reg, ioffset_reg, klass);
}

/* Emit code which loads <klass_reg>->interface_offsets [klass->interface_id] */
static void
mini_emit_load_intf_reg_class (MonoCompile *cfg, int intf_reg, int klass_reg, MonoClass *klass)
{
	int ioffset_reg = alloc_preg (cfg);

	MONO_EMIT_NEW_LOAD_MEMBASE (cfg, ioffset_reg, klass_reg, G_STRUCT_OFFSET (MonoClass, interface_offsets));

	if (cfg->compile_aot) {
		int iid_reg = alloc_preg (cfg);
		MONO_EMIT_NEW_AOTCONST (cfg, iid_reg, klass, MONO_PATCH_INFO_IID);
		MONO_EMIT_NEW_BIALU_IMM (cfg, OP_SHL_IMM, iid_reg, iid_reg, 2);
		MONO_EMIT_NEW_BIALU (cfg, OP_PADD, ioffset_reg, ioffset_reg, iid_reg);
		MONO_EMIT_NEW_LOAD_MEMBASE_OP (cfg, OP_LOADI4_MEMBASE, intf_reg, ioffset_reg, 0);
	}
	else
		MONO_EMIT_NEW_LOAD_MEMBASE_OP (cfg, OP_LOADI4_MEMBASE, intf_reg, ioffset_reg, klass->interface_id * sizeof (int));
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
	mini_emit_load_intf_reg_vtable (cfg, intf_reg, vtable_reg, klass);
	MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, intf_reg, 0);
	if (true_target)
		MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_PBNE_UN, true_target);
	else
		MONO_EMIT_NEW_COND_EXC (cfg, EQ, "InvalidCastException");		
}

/*
 * Variant of the above that takes a register to the class, not the vtable.
 * Note that inside interfaces_offsets the empty value is -1, not NULL, in this case.
 */
static void 
mini_emit_iface_class_cast (MonoCompile *cfg, int klass_reg, MonoClass *klass, MonoBasicBlock *false_target, MonoBasicBlock *true_target)
{
	int intf_reg = alloc_preg (cfg);

	mini_emit_max_iid_check_class (cfg, klass_reg, klass, false_target);
	mini_emit_load_intf_reg_class (cfg, intf_reg, klass_reg, klass);
	MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, intf_reg, -1);
	if (true_target)
		MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_PBGE, true_target);
	else
		MONO_EMIT_NEW_COND_EXC (cfg, EQ, "InvalidCastException");
}

static void 
mini_emit_castclass (MonoCompile *cfg, int klass_reg, MonoClass *klass)
{
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
	if (cfg->compile_aot) {
		int const_reg = alloc_preg (cfg);
		MONO_EMIT_NEW_CLASSCONST (cfg, const_reg, klass);
		MONO_EMIT_NEW_BIALU (cfg, OP_COMPARE, -1, stype, const_reg);
	} else {
		MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, stype, klass);
	}
	MONO_EMIT_NEW_COND_EXC (cfg, NE_UN, "InvalidCastException");
}

static void 
mini_emit_memset (MonoCompile *cfg, int destreg, int offset, int size, int val, int align)
{
	int val_reg;

	g_assert (val == 0);

	if (size <= 4) {
		switch (size) {
		case 1:
			MONO_EMIT_NEW_STORE_MEMBASE_IMM (cfg, OP_STOREI1_MEMBASE_IMM, destreg, offset, val);
			break;
		case 2:
			MONO_EMIT_NEW_STORE_MEMBASE_IMM (cfg, OP_STOREI2_MEMBASE_IMM, destreg, offset, val);
			break;
		case 4:
			MONO_EMIT_NEW_STORE_MEMBASE_IMM (cfg, OP_STOREI4_MEMBASE_IMM, destreg, offset, val);
			break;
#if SIZEOF_VOID_P == 8
		case 8:
			MONO_EMIT_NEW_STORE_MEMBASE_IMM (cfg, OP_STOREI8_MEMBASE_IMM, destreg, offset, val);
#endif
		}
		return;
	}

	val_reg = alloc_preg (cfg);

	if (sizeof (gpointer) == 8)
		MONO_EMIT_NEW_I8CONST (cfg, val_reg, val);
	else
		MONO_EMIT_NEW_ICONST (cfg, val_reg, val);

#if !NO_UNALIGNED_ACCESS
	if (sizeof (gpointer) == 8) {
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

void 
mini_emit_memcpy2 (MonoCompile *cfg, int destreg, int doffset, int srcreg, int soffset, int size, int align)
{
	int cur_reg;

#if !NO_UNALIGNED_ACCESS
	if (sizeof (gpointer) == 8) {
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

static int
ret_type_to_call_opcode (MonoType *type, int calli, int virt)
{
	if (type->byref)
		return calli? OP_CALL_REG: virt? OP_CALLVIRT: OP_CALL;

handle_enum:
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
 * Prepare arguments for passing to a function call.
 * Return a non-zero value if the arguments can't be passed to the given
 * signature.
 * The type checks are not yet complete and some conversions may need
 * casts on 32 or 64 bit architectures.
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

inline static MonoCallInst *
mono_emit_call_args (MonoCompile *cfg, MonoMethodSignature *sig, 
		     MonoInst **args, int calli, int virtual, const guint8 *ip)
{
	MonoCallInst *call;

	MONO_INST_NEW_CALL (cfg, call, ret_type_to_call_opcode (sig->ret, calli, virtual));

	call->inst.cil_code = ip;
	call->args = args;
	call->signature = sig;

	type_to_eval_stack_type (sig->ret, &call->inst);

	if (MONO_TYPE_ISSTRUCT (sig->ret)) {
		MonoInst *temp = mono_compile_create_var (cfg, sig->ret, OP_LOCAL);
		MonoInst *loada, *dummy_store;

		temp->flags |= MONO_INST_IS_TEMP;

		/* 
		 * Emit a dummy store to the local holding the result so the
		 * liveness info remains correct.
		 */
		NEW_DUMMY_STORE (cfg, dummy_store, temp->inst_c0);
		MONO_ADD_INS (cfg->cbb, dummy_store);

		/* FIXME: What is this ? */
		/* we use this to allocate native sized structs */
		temp->unused = sig->pinvoke;

		EMIT_NEW_TEMPLOADA (cfg, loada, temp->inst_c0);

		call->vret_var = loada;
	} else if (!MONO_TYPE_IS_VOID (sig->ret))
		call->inst.dreg = alloc_dreg (cfg, call->inst.type);

	call = mono_arch_call_opcode2 (cfg, call, virtual);
	
	return call;
}

/**
 * mono_emit_call_result:
 *
 *   Return an ins representing the result of a call.
 */
static MonoInst*
mono_emit_call_result (MonoCompile *cfg, MonoCallInst *call)
{
	if (MONO_TYPE_ISSTRUCT (call->signature->ret)) {
		MonoInst *loada = call->vret_var;

		loada->type = call->inst.type;
		loada->klass = call->inst.klass;

		return loada;
	}
	else
		return (MonoInst*)call;
}

inline static MonoInst*
mono_emit_calli (MonoCompile *cfg, MonoMethodSignature *sig, MonoInst **args, MonoInst *addr, const guint8 *ip)
{
	MonoCallInst *call = mono_emit_call_args (cfg, sig, args, TRUE, FALSE, ip);

	call->inst.sreg1 = addr->dreg;

	MONO_ADD_INS (cfg->cbb, (MonoInst*)call);

	return mono_emit_call_result (cfg, call);
}

static MonoInst*
mono_emit_method_call_full (MonoCompile *cfg, MonoMethod *method, MonoMethodSignature *sig,
		       MonoInst **args, const guint8 *ip, MonoInst *this)
{
	gboolean virtual = this != NULL;
	gboolean enable_for_aot = TRUE;
	MonoCallInst *call;

	if (method->string_ctor) {
		/* Create the real signature */
		/* FIXME: Cache these */
		MonoMethodSignature *ctor_sig = mono_metadata_signature_dup (sig);
		ctor_sig->ret = &mono_defaults.string_class->byval_arg;

		sig = ctor_sig;
	}

	call = mono_emit_call_args (cfg, sig, args, FALSE, virtual, ip);

	if (this && sig->hasthis && 
	    (method->klass->marshalbyref || method->klass == mono_defaults.object_class) && 
	    !(method->flags & METHOD_ATTRIBUTE_VIRTUAL) && !MONO_CHECK_THIS (this)) {
		call->method = mono_marshal_get_remoting_invoke_with_check (method);
	} else {
		call->method = method;
	}
	call->inst.flags |= MONO_INST_HAS_METHOD;
	call->inst.inst_left = this;

	if (virtual) {
		int vtable_reg, slot_reg, this_reg;

		this_reg = this->dreg;

		if ((!cfg->compile_aot || enable_for_aot) && 
			(!(method->flags & METHOD_ATTRIBUTE_VIRTUAL) || 
			 ((method->flags & METHOD_ATTRIBUTE_FINAL) && 
			  method->wrapper_type != MONO_WRAPPER_REMOTING_INVOKE_WITH_CHECK))) {
			/* 
			 * the method is not virtual, we just need to ensure this is not null
			 * and then we can call the method directly.
			 */
			if (method->klass->marshalbyref || method->klass == mono_defaults.object_class) {
				method = call->method = mono_marshal_get_remoting_invoke_with_check (method);
			}

			/* The backends will need the got var */
			mono_get_got_var (cfg);

			if (!method->string_ctor)
				MONO_EMIT_NEW_UNALU (cfg, OP_CHECK_THIS, -1, this_reg);

			call->inst.opcode = callvirt_to_call (call->inst.opcode);

			MONO_ADD_INS (cfg->cbb, (MonoInst*)call);

			return mono_emit_call_result (cfg, call);
		}

		call->inst.opcode = callvirt_to_call_membase (call->inst.opcode);

		/* Initialize method->slot */
		mono_class_setup_vtable (method->klass);

		vtable_reg = alloc_preg (cfg);
		MONO_EMIT_NEW_LOAD_MEMBASE (cfg, vtable_reg, this_reg, G_STRUCT_OFFSET (MonoObject, vtable));
		if (method->klass->flags & TYPE_ATTRIBUTE_INTERFACE) {
			slot_reg = alloc_preg (cfg);
			mini_emit_load_intf_reg_vtable (cfg, slot_reg, vtable_reg, method->klass);
			call->inst.inst_offset = method->slot * SIZEOF_VOID_P;
		} else {
			slot_reg = vtable_reg;
			call->inst.inst_offset = G_STRUCT_OFFSET (MonoVTable, vtable) + (method->slot * SIZEOF_VOID_P);
		}

		call->inst.sreg1 = slot_reg;
		call->virtual = TRUE;
	}

	if (!virtual)
		/* The backends will need the got var */
		mono_get_got_var (cfg);

	MONO_ADD_INS (cfg->cbb, (MonoInst*)call);

	return mono_emit_call_result (cfg, call);
}

static MonoInst*
mono_emit_method_call (MonoCompile *cfg, MonoMethod *method, MonoMethodSignature *sig,
		       MonoInst **args, const guint8 *ip, MonoInst *this)
{
	return (MonoInst*)mono_emit_method_call_full (cfg, method, sig, args, ip, this);
}

inline static MonoInst*
mono_emit_native_call (MonoCompile *cfg, gconstpointer func, MonoMethodSignature *sig,
		       MonoInst **args, const guint8 *ip)
{
	MonoCallInst *call;

	g_assert (sig);

	call = mono_emit_call_args (cfg, sig, args, FALSE, FALSE, ip);
	call->fptr = func;

	MONO_ADD_INS (cfg->cbb, (MonoInst*)call);

	/* The backends will need the got var */
	mono_get_got_var (cfg);

	return mono_emit_call_result (cfg, call);
}

inline static MonoInst*
mono_emit_jit_icall (MonoCompile *cfg, gconstpointer func, MonoInst **args, const guint8 *ip)
{
	MonoJitICallInfo *info = mono_find_jit_icall_by_addr (func);

	g_assert (info);

	return mono_emit_native_call (cfg, mono_icall_get_wrapper (info), info->sig, args, ip);
}

static MonoMethodSignature *
mono_get_element_address_signature (int arity)
{
	static GHashTable *sighash = NULL;
	MonoMethodSignature *res;
	int i;

	mono_jit_lock ();
	if (!sighash) {
		sighash = g_hash_table_new (NULL, NULL);
	}
	else if ((res = g_hash_table_lookup (sighash, GINT_TO_POINTER (arity)))) {
		LeaveCriticalSection (&jit_mutex);
		return res;
	}

	res = mono_metadata_signature_alloc (mono_defaults.corlib, arity + 1);

	res->pinvoke = 1;
#ifdef MONO_ARCH_VARARG_ICALLS
	/* Only set this only some archs since not all backends can handle varargs+pinvoke */
	res->call_convention = MONO_CALL_VARARG;
#endif
	res->params [0] = &mono_defaults.array_class->byval_arg; 
	
	for (i = 1; i <= arity; i++)
		res->params [i] = &mono_defaults.int_class->byval_arg;

	res->ret = &mono_defaults.int_class->byval_arg;

	g_hash_table_insert (sighash, GINT_TO_POINTER (arity), res);
	mono_jit_unlock ();

	return res;
}

static MonoMethodSignature *
mono_get_array_new_va_signature (int arity)
{
	static GHashTable *sighash = NULL;
	MonoMethodSignature *res;
	int i;

	mono_jit_lock ();
	if (!sighash) {
		sighash = g_hash_table_new (NULL, NULL);
	}
	else if ((res = g_hash_table_lookup (sighash, GINT_TO_POINTER (arity)))) {
		mono_jit_unlock ();
		return res;
	}

	res = mono_metadata_signature_alloc (mono_defaults.corlib, arity + 1);

	res->pinvoke = 1;
#ifdef MONO_ARCH_VARARG_ICALLS
	/* Only set this only some archs since not all backends can handle varargs+pinvoke */
	res->call_convention = MONO_CALL_VARARG;
#endif

	res->params [0] = &mono_defaults.int_class->byval_arg;	
	for (i = 0; i < arity; i++)
		res->params [i + 1] = &mono_defaults.int_class->byval_arg;

	res->ret = &mono_defaults.object_class->byval_arg;

	g_hash_table_insert (sighash, GINT_TO_POINTER (arity), res);
	mono_jit_unlock ();

	return res;
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
static void
emit_stobj (MonoCompile *cfg, MonoInst *dest, MonoInst *src, const unsigned char *ip, MonoClass *klass, gboolean native) {
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

	if ((cfg->opt & MONO_OPT_INTRINS) && n <= sizeof (gpointer) * 5) {
		/* FIXME: Optimize the case when src/dest is OP_LDADDR */
		mini_emit_memcpy2 (cfg, dest->dreg, 0, src->dreg, 0, n, 0);
	} else {
		iargs [0] = dest;
		iargs [1] = src;
		EMIT_NEW_ICONST (cfg, iargs [2], n);
		
		memcpy_method = get_memcpy_method ();
		mono_emit_method_call_full (cfg, memcpy_method, memcpy_method->signature, iargs, ip, NULL);
	}
}

/*
 * Emit code to load a valuetype to the evaluation stack and return a MonoInst
 * which can be pushed to the stack.
 */
static MonoInst*
emit_ldobj (MonoCompile *cfg, MonoInst *src, const unsigned char *ip, MonoClass *klass)
{
	MonoInst *dest, *temp;

	temp = mono_compile_create_var (cfg, &klass->byval_arg, OP_LOCAL);
	EMIT_NEW_TEMPLOADA (cfg, dest, temp->inst_c0);
	emit_stobj (cfg, dest, src, ip, klass, FALSE);

	EMIT_NEW_TEMPLOADA (cfg, dest, temp->inst_c0);
	dest->type = STACK_VTYPE;
	dest->klass = klass;
	return dest;
}

static MonoInst*
emit_ldobj_var (MonoCompile *cfg, MonoInst *var, const unsigned char*ip)
{
	MonoInst *src;

	NEW_VARLOADA (cfg, src, var, var->inst_vtype);
	MONO_ADD_INS (cfg->cbb, src);

	return emit_ldobj (cfg, src, ip, var->klass);
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

static void
handle_initobj (MonoCompile *cfg, MonoInst *dest, const guchar *ip, MonoClass *klass, MonoInst **stack_start, MonoInst **sp)
{
	MonoInst *iargs [3];
	int n;
	MonoMethod *memset_method;

	/* FIXME: Optimize this for the case when dest is an LDADDR */

	mono_class_init (klass);
	n = mono_class_value_size (klass, NULL);

	if (n <= sizeof (gpointer) * 5) {
		mini_emit_memset (cfg, dest->dreg, 0, n, 0, 0);			
	}
	else {
		memset_method = get_memset_method ();
		iargs [0] = dest;
		EMIT_NEW_ICONST (cfg, iargs [1], 0);
		EMIT_NEW_ICONST (cfg, iargs [2], n);
		mono_emit_method_call (cfg, memset_method, memset_method->signature, iargs, ip, NULL);
	}
}

/**
 * Handles unbox of a Nullable<T>
 */
static MonoInst*
handle_unbox_nullable (MonoCompile* cfg, MonoInst* val, const guchar *ip, MonoClass* klass)
{
	MonoMethod* method = mono_class_get_method_from_name (klass, "Unbox", 1);
	return mono_emit_method_call (cfg, method, mono_method_signature (method), &val, ip, NULL);
}

static MonoInst*
handle_unbox (MonoCompile *cfg, MonoClass *klass, MonoInst **sp, const guchar *ip)
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
			
	if (cfg->compile_aot) {
		int const_reg = alloc_preg (cfg);
		MONO_EMIT_NEW_CLASSCONST (cfg, const_reg, klass->element_class);
		MONO_EMIT_NEW_BIALU (cfg, OP_COMPARE, -1, eclass_reg, const_reg);
	} else {
		MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, eclass_reg, klass->element_class);
	}
	
	MONO_EMIT_NEW_COND_EXC (cfg, NE_UN, "InvalidCastException");

	NEW_BIALU_IMM (cfg, add, OP_ADD_IMM, alloc_dreg (cfg, STACK_PTR), obj_reg, sizeof (MonoObject));
	MONO_ADD_INS (cfg->cbb, add);
	add->type = STACK_PTR;

	return add;
}

static MonoInst*
handle_alloc (MonoCompile *cfg, MonoClass *klass, gboolean for_box, const guchar *ip)
{
	MonoInst *iargs [2];
	void *alloc_ftn;

	if (cfg->opt & MONO_OPT_SHARED) {
		EMIT_NEW_DOMAINCONST (cfg, iargs [0]);
		EMIT_NEW_CLASSCONST (cfg, iargs [1], klass);

		alloc_ftn = mono_object_new;
	} else {
		MonoVTable *vtable = mono_class_vtable (cfg->domain, klass);
		gboolean pass_lw;
		
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

	return mono_emit_jit_icall (cfg, alloc_ftn, iargs, ip);
}
	
static MonoInst*
handle_box (MonoCompile *cfg, MonoInst *val, const guchar *ip, MonoClass *klass)
{
	MonoInst *alloc, *dest, *vstore;

	if (mono_class_is_nullable (klass)) {
		MonoMethod* method = mono_class_get_method_from_name (klass, "Box", 1);
		return mono_emit_method_call (cfg, method, mono_method_signature (method), &val, ip, NULL);
	}

	alloc = handle_alloc (cfg, klass, TRUE, ip);

	NEW_STORE_MEMBASE (cfg, vstore, mono_type_to_store_membase (&klass->byval_arg), alloc->dreg, sizeof (MonoObject), val->dreg);

	if (vstore->opcode == CEE_STOBJ) {
		int dreg = alloc_preg (cfg);
		NEW_BIALU_IMM (cfg, dest, OP_ADD_IMM, dreg, alloc->dreg, sizeof (MonoObject));
		MONO_ADD_INS (cfg->cbb, dest);
		emit_stobj (cfg, dest, val, NULL, klass, FALSE);
	} else {
		MONO_ADD_INS (cfg->cbb, vstore);
	}

	return alloc;
}

static MonoInst*
handle_castclass (MonoCompile *cfg, MonoClass *klass, MonoInst *src, unsigned char *ip)
{
	MonoBasicBlock *is_null_bb;
	int obj_reg = src->dreg;
	int vtable_reg = alloc_preg (cfg);

	NEW_BBLOCK (cfg, is_null_bb);

	MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, obj_reg, 0);
	MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_PBEQ, is_null_bb);

	if (klass->flags & TYPE_ATTRIBUTE_INTERFACE) {
		MONO_EMIT_NEW_LOAD_MEMBASE (cfg, vtable_reg, obj_reg, G_STRUCT_OFFSET (MonoObject, vtable));
		mini_emit_iface_cast (cfg, vtable_reg, klass, NULL, NULL);
	} else {
		int klass_reg = alloc_preg (cfg);

		MONO_EMIT_NEW_LOAD_MEMBASE (cfg, vtable_reg, obj_reg, G_STRUCT_OFFSET (MonoObject, vtable));

		if (klass->rank) {
			int rank_reg = alloc_preg (cfg);
			int eclass_reg = alloc_preg (cfg);

			MONO_EMIT_NEW_LOAD_MEMBASE_OP (cfg, OP_LOADU1_MEMBASE, rank_reg, vtable_reg, G_STRUCT_OFFSET (MonoVTable, rank));
			MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, rank_reg, klass->rank);
			MONO_EMIT_NEW_COND_EXC (cfg, NE_UN, "InvalidCastException");
			MONO_EMIT_NEW_LOAD_MEMBASE (cfg, klass_reg, vtable_reg, G_STRUCT_OFFSET (MonoVTable, klass));
			MONO_EMIT_NEW_LOAD_MEMBASE (cfg, eclass_reg, klass_reg, G_STRUCT_OFFSET (MonoClass, cast_class));
			if (klass->cast_class == mono_defaults.object_class) {
				int parent_reg = alloc_preg (cfg);
				int const_reg = -1;
				MONO_EMIT_NEW_LOAD_MEMBASE (cfg, parent_reg, eclass_reg, G_STRUCT_OFFSET (MonoClass, parent));
				if (cfg->compile_aot) {
					const_reg = alloc_preg (cfg);
					MONO_EMIT_NEW_CLASSCONST (cfg, const_reg, mono_defaults.enum_class->parent);
					MONO_EMIT_NEW_BIALU (cfg, OP_COMPARE, -1, parent_reg, const_reg);
				} else {
					MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, parent_reg, mono_defaults.enum_class->parent);
				}
				MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_PBNE_UN, is_null_bb);
				if (cfg->compile_aot) {
					MONO_EMIT_NEW_CLASSCONST (cfg, const_reg, mono_defaults.enum_class);
					MONO_EMIT_NEW_BIALU (cfg, OP_COMPARE, -1, eclass_reg, const_reg);
				} else {
					MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, eclass_reg, mono_defaults.enum_class);
				}
				MONO_EMIT_NEW_COND_EXC (cfg, NE_UN, "InvalidCastException");
			} else if (klass->cast_class == mono_defaults.enum_class->parent) {
				int const_reg = alloc_preg (cfg);
				if (cfg->compile_aot) {
					MONO_EMIT_NEW_CLASSCONST (cfg, const_reg, mono_defaults.enum_class->parent);
					MONO_EMIT_NEW_BIALU (cfg, OP_COMPARE, -1, eclass_reg, const_reg);
				} else {
					MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, eclass_reg, mono_defaults.enum_class->parent);
				}
				MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_PBEQ, is_null_bb);
				if (cfg->compile_aot) {
					MONO_EMIT_NEW_CLASSCONST (cfg, const_reg, mono_defaults.enum_class);
					MONO_EMIT_NEW_BIALU (cfg, OP_COMPARE, -1, eclass_reg, const_reg);
				} else {
					MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, eclass_reg, mono_defaults.enum_class);
				}
				MONO_EMIT_NEW_COND_EXC (cfg, NE_UN, "InvalidCastException");
			} else if (klass->cast_class == mono_defaults.enum_class) {
				if (cfg->compile_aot) {
					int const_reg = alloc_preg (cfg);
					MONO_EMIT_NEW_CLASSCONST (cfg, const_reg, mono_defaults.enum_class);
					MONO_EMIT_NEW_BIALU (cfg, OP_COMPARE, -1, eclass_reg, const_reg);
				} else {
					MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, eclass_reg, mono_defaults.enum_class);
				}
				MONO_EMIT_NEW_COND_EXC (cfg, NE_UN, "InvalidCastException");
			} else if (klass->cast_class->flags & TYPE_ATTRIBUTE_INTERFACE) {
				mini_emit_iface_class_cast (cfg, eclass_reg, klass->cast_class, NULL, NULL);
			} else {
				mini_emit_castclass (cfg, eclass_reg, klass->cast_class);
			}

			if ((klass->rank == 1) && (klass->byval_arg.type == MONO_TYPE_SZARRAY)) {
				/* Check that the object is a vector too */
				int bounds_reg = alloc_preg (cfg);
				MONO_EMIT_NEW_LOAD_MEMBASE (cfg, bounds_reg, obj_reg, G_STRUCT_OFFSET (MonoArray, bounds));
				MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, bounds_reg, 0);
				MONO_EMIT_NEW_COND_EXC (cfg, NE_UN, "InvalidCastException");
			}
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
				MONO_EMIT_NEW_COND_EXC (cfg, NE_UN, "InvalidCastException");
			} else {
				MONO_EMIT_NEW_LOAD_MEMBASE (cfg, klass_reg, vtable_reg, G_STRUCT_OFFSET (MonoVTable, klass));
				mini_emit_castclass (cfg, klass_reg, klass);
			}
		}
	}

	MONO_START_BB (cfg, is_null_bb);

	return src;
}

static MonoInst*
handle_isinst (MonoCompile *cfg, MonoClass *klass, MonoInst *src, unsigned char *ip)
{
	MonoInst *ins;
	MonoBasicBlock *is_null_bb, *false_bb, *end_bb;
	int obj_reg = src->dreg;
	int vtable_reg = alloc_preg (cfg);
	int res_reg = alloc_preg (cfg);

	NEW_BBLOCK (cfg, is_null_bb);
	NEW_BBLOCK (cfg, false_bb);
	NEW_BBLOCK (cfg, end_bb);

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
				int const_reg = -1;
				MONO_EMIT_NEW_LOAD_MEMBASE (cfg, parent_reg, eclass_reg, G_STRUCT_OFFSET (MonoClass, parent));
				if (cfg->compile_aot) {
					const_reg = alloc_preg (cfg);
					MONO_EMIT_NEW_CLASSCONST (cfg, const_reg, mono_defaults.enum_class->parent);
					MONO_EMIT_NEW_BIALU (cfg, OP_COMPARE, -1, parent_reg, const_reg);
				} else {
					MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, parent_reg, mono_defaults.enum_class->parent);
				}
				MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_PBNE_UN, is_null_bb);
				if (cfg->compile_aot) {
					MONO_EMIT_NEW_CLASSCONST (cfg, const_reg, mono_defaults.enum_class);
					MONO_EMIT_NEW_BIALU (cfg, OP_COMPARE, -1, eclass_reg, const_reg);
				} else {
					MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, eclass_reg, mono_defaults.enum_class);
				}
				MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_PBEQ, is_null_bb);
				MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_BR, false_bb);
			} else if (klass->cast_class == mono_defaults.enum_class->parent) {
				int const_reg = -1;
				
				if (cfg->compile_aot) {
					const_reg = alloc_preg (cfg);
					MONO_EMIT_NEW_CLASSCONST (cfg, const_reg, mono_defaults.enum_class->parent);
					MONO_EMIT_NEW_BIALU (cfg, OP_COMPARE, -1, eclass_reg, const_reg);
				} else {
					MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, eclass_reg, mono_defaults.enum_class->parent);
				}
				MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_PBEQ, is_null_bb);
				if (cfg->compile_aot) {
					MONO_EMIT_NEW_CLASSCONST (cfg, const_reg, mono_defaults.enum_class);
					MONO_EMIT_NEW_BIALU (cfg, OP_COMPARE, -1, eclass_reg, const_reg);
				} else {
					MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, eclass_reg, mono_defaults.enum_class);
				}
				MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_PBEQ, is_null_bb);
				MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_BR, false_bb);
			} else if (klass->cast_class == mono_defaults.enum_class) {
				if (cfg->compile_aot) {
					int const_reg = alloc_preg (cfg);
					MONO_EMIT_NEW_CLASSCONST (cfg, const_reg, mono_defaults.enum_class);
					MONO_EMIT_NEW_BIALU (cfg, OP_COMPARE, -1, eclass_reg, const_reg);
				} else {
					MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, eclass_reg, mono_defaults.enum_class);
				}
				MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_PBEQ, is_null_bb);
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

	MONO_EMIT_NEW_ICONST (cfg, res_reg, 0);
	MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_BR, end_bb);

	MONO_START_BB (cfg, is_null_bb);

	EMIT_NEW_UNALU (cfg, ins, OP_MOVE, res_reg, obj_reg);
	ins->type = STACK_OBJ;

	MONO_START_BB (cfg, end_bb);

	return ins;
}

static MonoInst*
handle_cisinst (MonoCompile *cfg, MonoClass *klass, MonoInst *src, unsigned char *ip)
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
		
		if (cfg->compile_aot) {
			int tproxy_reg = alloc_preg (cfg);
			MONO_EMIT_NEW_CLASSCONST (cfg, tproxy_reg, mono_defaults.transparent_proxy_class);
			MONO_EMIT_NEW_BIALU (cfg, OP_COMPARE, -1, klass_reg, tproxy_reg);
		} else {
			MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, klass_reg, mono_defaults.transparent_proxy_class);
		}
		MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_PBNE_UN, false_bb);

		tmp_reg = alloc_preg (cfg);
		MONO_EMIT_NEW_LOAD_MEMBASE (cfg, tmp_reg, obj_reg, G_STRUCT_OFFSET (MonoTransparentProxy, custom_type_info));
		MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, tmp_reg, 0);
		MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_PBNE_UN, false2_bb);
		
	} else {
		tmp_reg = alloc_preg (cfg);
		MONO_EMIT_NEW_LOAD_MEMBASE (cfg, tmp_reg, obj_reg, G_STRUCT_OFFSET (MonoObject, vtable));
		MONO_EMIT_NEW_LOAD_MEMBASE (cfg, klass_reg, tmp_reg, G_STRUCT_OFFSET (MonoVTable, klass));
		
		if (cfg->compile_aot) {
			int tproxy_reg = alloc_preg (cfg);
			MONO_EMIT_NEW_CLASSCONST (cfg, tproxy_reg, mono_defaults.transparent_proxy_class);
			MONO_EMIT_NEW_BIALU (cfg, OP_COMPARE, -1, klass_reg, tproxy_reg);
		} else {
			MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, klass_reg, mono_defaults.transparent_proxy_class);
		}
		MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_PBNE_UN, no_proxy_bb);
		tmp_reg = alloc_preg (cfg);
		MONO_EMIT_NEW_LOAD_MEMBASE (cfg, tmp_reg, obj_reg, G_STRUCT_OFFSET (MonoTransparentProxy, remote_class));
		MONO_EMIT_NEW_LOAD_MEMBASE (cfg, klass_reg, tmp_reg, G_STRUCT_OFFSET (MonoRemoteClass, proxy_class));

		tmp_reg = alloc_preg (cfg);		
		MONO_EMIT_NEW_LOAD_MEMBASE (cfg, tmp_reg, obj_reg, G_STRUCT_OFFSET (MonoTransparentProxy, custom_type_info));
		MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, tmp_reg, 0);
		MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_PBEQ, no_proxy_bb);
		
		mini_emit_isninst_cast (cfg, klass_reg, klass, NULL, true_bb);
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
handle_ccastclass (MonoCompile *cfg, MonoClass *klass, MonoInst *src, unsigned char *ip)
{
	/* This opcode takes as input an object reference and a class, and returns:
	0) if the object is an instance of the class,
	1) if the object is a proxy whose type cannot be determined
	an InvalidCastException exception is thrown otherwhise*/
	
	MonoInst *ins;
	MonoBasicBlock *end_bb, *ok_result_bb, *no_proxy_bb, *interface_fail_bb;
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

		if (cfg->compile_aot) {
			int tproxy_reg = alloc_preg (cfg);
			MONO_EMIT_NEW_CLASSCONST (cfg, tproxy_reg, mono_defaults.transparent_proxy_class);
			MONO_EMIT_NEW_BIALU (cfg, OP_COMPARE, -1, klass_reg, tproxy_reg);
		} else {
			MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, klass_reg, mono_defaults.transparent_proxy_class);
		}
		
		MONO_EMIT_NEW_COND_EXC (cfg, NE_UN, "InvalidCastException");

		tmp_reg = alloc_preg (cfg);
		MONO_EMIT_NEW_LOAD_MEMBASE (cfg, tmp_reg, obj_reg, G_STRUCT_OFFSET (MonoTransparentProxy, remote_class));
		MONO_EMIT_NEW_LOAD_MEMBASE (cfg, klass_reg, tmp_reg, G_STRUCT_OFFSET (MonoRemoteClass, proxy_class));

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

		if (cfg->compile_aot) {
			int tproxy_reg = alloc_preg (cfg);
			MONO_EMIT_NEW_CLASSCONST (cfg, tproxy_reg, mono_defaults.transparent_proxy_class);
			MONO_EMIT_NEW_BIALU (cfg, OP_COMPARE, -1, klass_reg, tproxy_reg);
		} else {
			MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, klass_reg, mono_defaults.transparent_proxy_class);
		}
		MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_PBNE_UN, no_proxy_bb);

		tmp_reg = alloc_preg (cfg);
		MONO_EMIT_NEW_LOAD_MEMBASE (cfg, tmp_reg, obj_reg, G_STRUCT_OFFSET (MonoTransparentProxy, remote_class));
		MONO_EMIT_NEW_LOAD_MEMBASE (cfg, klass_reg, tmp_reg, G_STRUCT_OFFSET (MonoRemoteClass, proxy_class));

		tmp_reg = alloc_preg (cfg);
		MONO_EMIT_NEW_LOAD_MEMBASE (cfg, tmp_reg, obj_reg, G_STRUCT_OFFSET (MonoTransparentProxy, custom_type_info));
		MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, tmp_reg, 0);
		MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_PBEQ, no_proxy_bb);
		
		mini_emit_isninst_cast (cfg, klass_reg, klass, NULL, ok_result_bb);

		MONO_EMIT_NEW_ICONST (cfg, dreg, 1);
		MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_BR, end_bb);

		MONO_START_BB (cfg, no_proxy_bb);

		mini_emit_castclass (cfg, klass_reg, klass);
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

static MonoInst*
handle_array_new (MonoCompile *cfg, int rank, MonoInst **sp, unsigned char *ip)
{
	MonoMethodSignature *esig;
	char icall_name [256];
	char *name;
	MonoJitICallInfo *info;

	/* Need to register the icall so it gets an icall wrapper */
	sprintf (icall_name, "ves_array_new_va_%d", rank);

	info = mono_find_jit_icall_by_name (icall_name);
	if (info == NULL) {
		esig = mono_get_array_new_va_signature (rank);
		name = g_strdup (icall_name);
		info = mono_register_jit_icall (mono_array_new_va, name, esig, FALSE);

		mono_jit_lock ();
		g_hash_table_insert (jit_icall_name_hash, name, name);
		mono_jit_unlock ();
	}

	cfg->flags |= MONO_CFG_HAS_VARARGS;

	return mono_emit_native_call (cfg, mono_icall_get_wrapper (info), info->sig, sp, ip);
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

#define CODE_IS_STLOC(ip) (((ip) [0] >= CEE_STLOC_0 && (ip) [0] <= CEE_STLOC_3) || ((ip) [0] == CEE_STLOC_S))

static gboolean
mini_class_is_system_array (MonoClass *klass)
{
	if (klass->parent == mono_defaults.array_class)
		return TRUE;
	else if (mono_defaults.generic_array_class && klass->parent && klass->parent->generic_class)
		return klass->parent->generic_class->container_class == mono_defaults.generic_array_class;
	else
		return FALSE;
}

static gboolean
mono_method_check_inlining (MonoCompile *cfg, MonoMethod *method)
{
	MonoMethodHeader *header = mono_method_get_header (method);
	MonoMethodSignature *signature = mono_method_signature (method);
	MonoVTable *vtable;
	int i;

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
	    !header || header->num_clauses ||
	    /* fixme: why cant we inline valuetype returns? */
	    MONO_TYPE_ISSTRUCT (signature->ret))
		return FALSE;

	/* its not worth to inline methods with valuetype arguments?? */
	for (i = 0; i < signature->param_count; i++) {
		if (MONO_TYPE_ISSTRUCT (signature->params [i])) {
			return FALSE;
		}
	}

	/*
	 * if we can initialize the class of the method right away, we do,
	 * otherwise we don't allow inlining if the class needs initialization,
	 * since it would mean inserting a call to mono_runtime_class_init()
	 * inside the inlined code
	 */
	if (!(cfg->opt & MONO_OPT_SHARED)) {
		vtable = mono_class_vtable (cfg->domain, method->klass);
		if (method->klass->flags & TYPE_ATTRIBUTE_BEFORE_FIELD_INIT) {
			if (cfg->run_cctors)
				mono_runtime_class_init (vtable);
		}
		else if (!vtable->initialized && mono_class_needs_cctor_run (method->klass, NULL))
			return FALSE;
	} else {
		/* 
		 * If we're compiling for shared code
		 * the cctor will need to be run at aot method load time, for example,
		 * or at the end of the compilation of the inlining method.
		 */
		if (mono_class_needs_cctor_run (method->klass, NULL) && !((method->klass->flags & TYPE_ATTRIBUTE_BEFORE_FIELD_INIT)))
			return FALSE;
	}
	//if (!MONO_TYPE_IS_VOID (signature->ret)) return FALSE;

	/*
	 * CAS - do not inline methods with declarative security
	 * Note: this has to be before any possible return TRUE;
	 */
	if (mono_method_has_declsec (method))
		return FALSE;

	/* also consider num_locals? */
	if (getenv ("MONO_INLINELIMIT")) {
		if (header->code_size < atoi (getenv ("MONO_INLINELIMIT"))) {
			return TRUE;
		}
	} else if (header->code_size < 20)
		return TRUE;

	return FALSE;
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
	int mult_reg, add_reg, array_reg, index_reg;

	mono_class_init (klass);
	size = mono_class_array_element_size (klass);

	mult_reg = alloc_preg (cfg);
	add_reg = alloc_preg (cfg);
	array_reg = arr->dreg;
	index_reg = index->dreg;

	MONO_EMIT_BOUNDS_CHECK (cfg, array_reg, MonoArray, max_length, index_reg);

#ifdef __i386__
	if (size == 1 || size == 2 || size == 4 || size == 8) {
		static const int fast_log2 [] = { 1, 0, 1, -1, 2, -1, -1, -1, 3 };
		MONO_INST_NEW (cfg, ins, OP_X86_LEA);
		ins->dreg = add_reg;
		ins->sreg1 = array_reg;
		ins->sreg2 = index_reg;
		ins->inst_imm = G_STRUCT_OFFSET (MonoArray, vector);
		ins->unused = fast_log2 [size];
		ins->type = STACK_PTR;
		MONO_ADD_INS (cfg->cbb, ins);

		return ins;
	}
#endif		

	MONO_EMIT_NEW_BIALU_IMM (cfg, OP_MUL_IMM, mult_reg, index_reg, size);
	MONO_EMIT_NEW_BIALU (cfg, OP_PADD, add_reg, array_reg, mult_reg);
	NEW_BIALU_IMM (cfg, ins, OP_PADD_IMM, add_reg, add_reg, G_STRUCT_OFFSET (MonoArray, vector));
	ins->type = STACK_PTR;
	MONO_ADD_INS (cfg->cbb, ins);

	return ins;
}

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

	ins->type = STACK_PTR;
	MONO_ADD_INS (cfg->cbb, ins);

	return ins;
}

static MonoInst*
mini_emit_ldelema_ins (MonoCompile *cfg, MonoMethod *cmethod, MonoInst **sp, unsigned char *ip, gboolean is_set)
{
	int rank;
	MonoInst *addr;
	MonoMethodSignature *esig;
	char icall_name [256];
	char *name;
	MonoJitICallInfo *info;

	rank = mono_method_signature (cmethod)->param_count - (is_set? 1: 0);

	if (rank == 1)
		return mini_emit_ldelema_1_ins (cfg, cmethod->klass->element_class, sp [0], sp [1]);

#ifndef MONO_ARCH_EMULATE_MUL_DIV
	/* emit_ldelema_2 depends on OP_LMUL */
	if (rank == 2 && (cfg->opt & MONO_OPT_INTRINS)) {
		return mini_emit_ldelema_2_ins (cfg, cmethod->klass->element_class, sp [0], sp [1], sp [2]);
	}
#endif

	/* Need to register the icall so it gets an icall wrapper */
	sprintf (icall_name, "ves_array_element_address_%d", rank);

	info = mono_find_jit_icall_by_name (icall_name);
	if (info == NULL) {
		esig = mono_get_element_address_signature (rank);
		name = g_strdup (icall_name);
		info = mono_register_jit_icall (ves_array_element_address, name, esig, FALSE);

		mono_jit_lock ();
		g_hash_table_insert (jit_icall_name_hash, name, name);
		mono_jit_unlock ();
	}

	cfg->flags |= MONO_CFG_HAS_VARARGS;
	addr = mono_emit_native_call (cfg, mono_icall_get_wrapper (info), info->sig, sp, ip);

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
			int mult_reg = alloc_preg (cfg);
			int add_reg = alloc_preg (cfg);
	
			MONO_EMIT_BOUNDS_CHECK (cfg, args [0]->dreg, MonoString, length, args [1]->dreg);
			MONO_EMIT_NEW_BIALU_IMM (cfg, OP_ISHL_IMM, mult_reg, args [1]->dreg, 1);
			MONO_EMIT_NEW_BIALU (cfg, OP_PADD, add_reg, mult_reg, args [0]->dreg);
			EMIT_NEW_LOAD_MEMBASE (cfg, ins, OP_LOADU2_MEMBASE, dreg, 
								   add_reg, G_STRUCT_OFFSET (MonoString, chars));
			type_from_op (ins, NULL, NULL);
			return ins;
		} else if (strcmp (cmethod->name, "get_Length") == 0) {
			int dreg = alloc_ireg (cfg);
			EMIT_NEW_LOAD_MEMBASE (cfg, ins, OP_LOADI4_MEMBASE, dreg, 
								   args [0]->dreg, G_STRUCT_OFFSET (MonoString, length));
			type_from_op (ins, NULL, NULL);

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
		} else if (strcmp (cmethod->name, "InternalGetHashCode") == 0) {
#ifndef MONO_ARCH_EMULATE_MUL_DIV
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
		}
	}

	return mono_arch_emit_inst_for_method (cfg, cmethod, fsig, args);
}

static void
mono_save_args (MonoCompile *cfg, MonoMethodSignature *sig, MonoInst **sp, MonoInst **args)
{
	MonoInst *store, *temp;
	int i;

	g_assert (!MONO_TYPE_ISSTRUCT (sig->ret));

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
		*args++ = temp;
		NEW_TEMPSTORE (cfg, store, temp->inst_c0, *sp);
		store->cil_code = sp [0]->cil_code;
		if (store->opcode == CEE_STOBJ) {
			EMIT_NEW_TEMPLOADA (cfg, store, temp->inst_c0);
			emit_stobj (cfg, store, *sp, sp [0]->cil_code, temp->klass, FALSE);
		} else {
			MONO_ADD_INS (cfg->cbb, store);
		}
		sp++;
	}
}

static int
inline_method (MonoCompile *cfg, MonoMethod *cmethod, MonoMethodSignature *fsig, MonoInst **sp,
		guchar *ip, guint real_offset, GList *dont_inline, gboolean inline_allways)
{
	MonoInst *ins, *rvar = NULL;
	MonoMethodHeader *cheader;
	MonoBasicBlock *ebblock, *sbblock;
	int i, costs, new_locals_offset;
	MonoMethod *prev_inlined_method;
	guint prev_real_offset;
	GHashTable *prev_cbb_hash;
	MonoBasicBlock *prev_cbb;

	if (cfg->verbose_level > 2)
		printf ("INLINE START %p %s -> %s\n", cmethod,  mono_method_full_name (cfg->method, TRUE), mono_method_full_name (cmethod, TRUE));

	if (!cmethod->inline_info) {
		mono_jit_stats.inlineable_methods++;
		cmethod->inline_info = 1;
	}
	/* allocate space to store the return value */
	if (!MONO_TYPE_IS_VOID (fsig->ret)) {
		rvar =  mono_compile_create_var (cfg, fsig->ret, OP_LOCAL);
	}

	/* allocate local variables */
	cheader = mono_method_get_header (cmethod);
	new_locals_offset = cfg->num_varinfo;
	for (i = 0; i < cheader->num_locals; ++i)
		mono_compile_create_var (cfg, cheader->locals [i], OP_LOCAL);
	
	/* allocate start and end blocks */
	/* This is needed so if the inline is aborted, we can clean up */
	NEW_BBLOCK (cfg, sbblock);
	sbblock->real_offset = real_offset;

	NEW_BBLOCK (cfg, ebblock);
	ebblock->block_num = cfg->num_bblocks++;
	ebblock->real_offset = real_offset;

	prev_inlined_method = cfg->inlined_method;
	cfg->inlined_method = cmethod;
	prev_real_offset = cfg->real_offset;
	prev_cbb_hash = cfg->cbb_hash;
	prev_cbb = cfg->cbb;

	costs = mono_method_to_ir2 (cfg, cmethod, sbblock, ebblock, new_locals_offset, rvar, dont_inline, sp, real_offset, *ip == CEE_CALLVIRT);

	cfg->inlined_method = prev_inlined_method;
	cfg->real_offset = prev_real_offset;
	cfg->cbb_hash = prev_cbb_hash;

	if ((costs >= 0 && costs < 60) || inline_allways) {
		if (cfg->verbose_level > 2)
			printf ("INLINE END %s -> %s\n", mono_method_full_name (cfg->method, TRUE), mono_method_full_name (cmethod, TRUE));
		
		mono_jit_stats.inlined_methods++;

		/* always add some code to avoid block split failures */
		MONO_INST_NEW (cfg, ins, OP_NOP);
		MONO_ADD_INS (prev_cbb, ins);
		ins->cil_code = ip;

		prev_cbb->next_bb = sbblock;
		link_bblock (cfg, prev_cbb, sbblock);

		cfg->cbb = ebblock;

		if (rvar) {
			NEW_TEMPLOAD (cfg, ins, rvar->inst_c0);
			if (ins->opcode == CEE_LDOBJ)
				g_assert_not_reached ();
			else
				MONO_ADD_INS (cfg->cbb, ins);
			*sp++ = ins;
		}
		return costs + 1;
	} else {
		if (cfg->verbose_level > 2)
			printf ("INLINE ABORTED %s\n", mono_method_full_name (cmethod, TRUE));

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
 *
 * TODO:
 * * consider using an array instead of an hash table (bb_hash)
 */

#define CHECK_TYPE(ins) if (!(ins)->type) goto unverified
#define CHECK_STACK(num) if ((sp - stack_start) < (num)) goto unverified
#define CHECK_STACK_OVF(num) if (((sp - stack_start) + (num)) > header->max_stack) goto unverified
#define CHECK_ARG(num) if ((unsigned)(num) >= (unsigned)num_args) goto unverified
#define CHECK_LOCAL(num) if ((unsigned)(num) >= (unsigned)header->num_locals) goto unverified
#define CHECK_OPSIZE(size) if (ip + size > end) goto unverified


/* offset from br.s -> br like opcodes */
#define BIG_BRANCH_OFFSET 13

static gboolean
ip_in_bb (MonoCompile *cfg, MonoBasicBlock *bb, const guint8* ip)
{
	MonoBasicBlock *b = g_hash_table_lookup (cfg->bb_hash, ip);
	
	return b == NULL || b == bb;
}

static int
get_basic_blocks (MonoCompile *cfg, GHashTable *bbhash, MonoMethodHeader* header, guint real_offset, unsigned char *start, unsigned char *end, unsigned char **pos)
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
			goto unverified;
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
			GET_BBLOCK (cfg, bbhash, bblock, target);
			ip += 2;
			if (ip < end)
				GET_BBLOCK (cfg, bbhash, bblock, ip);
			break;
		case MonoInlineBrTarget:
			target = start + cli_addr + 5 + (gint32)read32 (ip + 1);
			GET_BBLOCK (cfg, bbhash, bblock, target);
			ip += 5;
			if (ip < end)
				GET_BBLOCK (cfg, bbhash, bblock, ip);
			break;
		case MonoInlineSwitch: {
			guint32 n = read32 (ip + 1);
			guint32 j;
			ip += 5;
			cli_addr += 5 + 4 * n;
			target = start + cli_addr;
			GET_BBLOCK (cfg, bbhash, bblock, target);
			
			for (j = 0; j < n; ++j) {
				target = start + cli_addr + (gint32)read32 (ip);
				GET_BBLOCK (cfg, bbhash, bblock, target);
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
			while ((bb_start > start) && !bblock) {
				bblock = g_hash_table_lookup (bbhash, (bb_start));
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
mini_get_method (MonoMethod *m, guint32 token, MonoClass *klass, MonoGenericContext *context)
{
	MonoMethod *method;

	if (m->wrapper_type != MONO_WRAPPER_NONE)
		return mono_method_get_wrapper_data (m, token);

	method = mono_get_method_full (m->klass->image, token, klass, context);

	if (method && method->is_inflated)
		method = mono_get_inflated_method (method);

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

static
void check_linkdemand (MonoCompile *cfg, MonoMethod *caller, MonoMethod *callee, unsigned char *ip)
{
	guint32 result = mono_declsec_linkdemand (cfg->domain, caller, callee);
	if (result == MONO_JIT_SECURITY_OK)
		return;

	if (result == MONO_JIT_LINKDEMAND_ECMA) {
		/* Generate code to throw a SecurityException before the actual call/link */
		MonoAssembly *assembly = mono_image_get_assembly (caller->klass->image);
		MonoReflectionAssembly *refass = (MonoReflectionAssembly*) mono_assembly_get_object (cfg->domain, assembly);
		MonoReflectionMethod *refmet = mono_method_get_object (cfg->domain, caller, NULL);
		MonoSecurityManager *secman = mono_security_manager_get_methods ();
		MonoInst *args [3];

		NEW_ICONST (cfg, args [0], 4);
		NEW_PCONST (cfg, args [1], refass);
		NEW_PCONST (cfg, args [2], refmet);
		mono_emit_method_call (cfg, secman->linkdemandsecurityexception, mono_method_signature (secman->linkdemandsecurityexception), args, ip, NULL);
	} else if (cfg->exception_type == MONO_EXCEPTION_NONE) {
		 /* don't hide previous results */
		cfg->exception_type = MONO_EXCEPTION_SECURITY_LINKDEMAND;
		cfg->exception_data = result;
	}
}

/*
 * decompose_opcode:
 *
 *   Decompose complex opcodes into ones closer to opcodes supported by
 * the given architecture.
 */
static void
decompose_opcode (MonoCompile *cfg, MonoInst *ins)
{
	/* FIXME: Instead of = NOP, don't emit the original ins at all */

	/*
	 * The code below assumes that we are called immediately after emitting 
	 * ins. This means we can emit code using the normal code generation
	 * macros.
	 */
	switch (ins->opcode) {
	case OP_IADD_OVF:
		ins->opcode = OP_IADDCC;
		MONO_EMIT_NEW_COND_EXC (cfg, IOV, "OverflowException");
		break;
	case OP_IADD_OVF_UN:
		ins->opcode = OP_IADDCC;
		MONO_EMIT_NEW_COND_EXC (cfg, IC, "OverflowException");
		break;
	case OP_ISUB_OVF:
		ins->opcode = OP_ISUBCC;
		MONO_EMIT_NEW_COND_EXC (cfg, IOV, "OverflowException");
		break;
	case OP_ISUB_OVF_UN:
		ins->opcode = OP_ISUBCC;
		MONO_EMIT_NEW_COND_EXC (cfg, IC, "OverflowException");
		break;
	case OP_ICONV_TO_OVF_I1:
		MONO_EMIT_NEW_ICOMPARE_IMM (cfg, ins->sreg1, 127);
		MONO_EMIT_NEW_COND_EXC (cfg, IGT, "OverflowException");
		MONO_EMIT_NEW_ICOMPARE_IMM (cfg, ins->sreg1, -128);
		MONO_EMIT_NEW_COND_EXC (cfg, ILT, "OverflowException");
		MONO_EMIT_NEW_UNALU (cfg, OP_ICONV_TO_I1, ins->dreg, ins->sreg1);
		ins->opcode = OP_NOP;
		break;
	case OP_ICONV_TO_OVF_I1_UN:
		/* probe values between 0 to 127 */
		MONO_EMIT_NEW_ICOMPARE_IMM (cfg, ins->sreg1, 127);
		MONO_EMIT_NEW_COND_EXC (cfg, IGT_UN, "OverflowException");
		MONO_EMIT_NEW_UNALU (cfg, OP_ICONV_TO_I1, ins->dreg, ins->sreg1);
		ins->opcode = OP_NOP;
		break;
	case OP_ICONV_TO_OVF_U1:
	case OP_ICONV_TO_OVF_U1_UN:
		/* probe value to be within 0 to 255 */
		MONO_EMIT_NEW_COMPARE_IMM (cfg, ins->sreg1, 255);
		MONO_EMIT_NEW_COND_EXC (cfg, IGT_UN, "OverflowException");
		MONO_EMIT_NEW_BIALU_IMM (cfg, OP_IAND_IMM, ins->dreg, ins->sreg1, 0xff);
		ins->opcode = OP_NOP;
		break;
	case OP_ICONV_TO_OVF_I2:
		/* Probe value to be within -32768 and 32767 */
		MONO_EMIT_NEW_ICOMPARE_IMM (cfg, ins->sreg1, 32767);
		MONO_EMIT_NEW_COND_EXC (cfg, IGT, "OverflowException");
		MONO_EMIT_NEW_ICOMPARE_IMM (cfg, ins->sreg1, -32768);
		MONO_EMIT_NEW_COND_EXC (cfg, ILT, "OverflowException");
		MONO_EMIT_NEW_UNALU (cfg, OP_ICONV_TO_I2, ins->dreg, ins->sreg1);
		ins->opcode = OP_NOP;
		break;
	case OP_ICONV_TO_OVF_I2_UN:
		/* Convert uint value into short, value within 0 and 32767 */
		MONO_EMIT_NEW_ICOMPARE_IMM (cfg, ins->sreg1, 32767);
		MONO_EMIT_NEW_COND_EXC (cfg, IGT_UN, "OverflowException");
		MONO_EMIT_NEW_UNALU (cfg, OP_ICONV_TO_I2, ins->dreg, ins->sreg1);
		ins->opcode = OP_NOP;
		break;
	case OP_ICONV_TO_OVF_U2:
	case OP_ICONV_TO_OVF_U2_UN:
		/* Probe value to be within 0 and 65535 */
		MONO_EMIT_NEW_ICOMPARE_IMM (cfg, ins->sreg1, 0xffff);
		MONO_EMIT_NEW_COND_EXC (cfg, IGT_UN, "OverflowException");
		MONO_EMIT_NEW_BIALU_IMM (cfg, OP_IAND_IMM, ins->dreg, ins->sreg1, 0xffff);
		ins->opcode = OP_NOP;
		break;
	case OP_ICONV_TO_OVF_U4:
	case OP_ICONV_TO_OVF_I4_UN:
		MONO_EMIT_NEW_ICOMPARE_IMM (cfg, ins->sreg1, 0);
		MONO_EMIT_NEW_COND_EXC (cfg, ILT, "OverflowException");
		MONO_EMIT_NEW_UNALU (cfg, OP_MOVE, ins->dreg, ins->sreg1);
		ins->opcode = OP_NOP;
		break;
	case OP_ICONV_TO_I4:
	case OP_ICONV_TO_U4:
	case OP_ICONV_TO_I:
	case OP_ICONV_TO_U:
		ins->opcode = OP_MOVE;
		break;

		/* Long opcodes on 64 bit machines */
#if SIZEOF_VOID_P == 8
	case OP_LCONV_TO_I4:
		MONO_EMIT_NEW_BIALU_IMM (cfg, OP_LSHR_IMM, ins->dreg, ins->sreg1, 0);
		ins->opcode = OP_NOP;
		break;
	case OP_LCONV_TO_I8:
	case OP_LCONV_TO_I:
		ins->opcode = OP_MOVE;
		break;
	case OP_ICONV_TO_I8:
		ins->opcode = OP_MOVE;
		break;
	case OP_LCONV_TO_U4:
		/* Clean out the upper word */
		MONO_EMIT_NEW_BIALU_IMM (cfg, OP_ISHR_UN_IMM, ins->dreg, ins->sreg1, 0);
		ins->opcode = OP_NOP;
		break;
	case OP_LADD_OVF:
		MONO_EMIT_NEW_BIALU (cfg, OP_ADDCC, ins->dreg, ins->sreg1, ins->sreg2);
		MONO_EMIT_NEW_COND_EXC (cfg, OV, "OverflowException");
		ins->opcode = OP_NOP;
		break;
	case OP_LADD_OVF_UN:
		MONO_EMIT_NEW_BIALU (cfg, OP_ADDCC, ins->dreg, ins->sreg1, ins->sreg2);
		MONO_EMIT_NEW_COND_EXC (cfg, C, "OverflowException");
		ins->opcode = OP_NOP;
		break;
	case OP_LSUB_OVF:
		MONO_EMIT_NEW_BIALU (cfg, OP_SUBCC, ins->dreg, ins->sreg1, ins->sreg2);
		MONO_EMIT_NEW_COND_EXC (cfg, OV, "OverflowException");
		ins->opcode = OP_NOP;
		break;
	case OP_LSUB_OVF_UN:
		MONO_EMIT_NEW_BIALU (cfg, OP_SUBCC, ins->dreg, ins->sreg1, ins->sreg2);
		MONO_EMIT_NEW_COND_EXC (cfg, C, "OverflowException");
		ins->opcode = OP_NOP;
		break;
		
	case OP_ICONV_TO_OVF_I8:
		/* Sign extend the value in the lower word into the upper word */
		MONO_EMIT_NEW_BIALU_IMM (cfg, OP_LSHR_IMM, ins->dreg, ins->sreg1, 0);
		ins->opcode = OP_NOP;
		break;
	case OP_ICONV_TO_OVF_U8:
		MONO_EMIT_NEW_COMPARE_IMM (cfg,ins->sreg1, 0);
		MONO_EMIT_NEW_COND_EXC (cfg, LT, "OverflowException");
		MONO_EMIT_NEW_UNALU (cfg, OP_MOVE, ins->dreg, ins->sreg1);
		ins->opcode = OP_NOP;
		break;
	case OP_ICONV_TO_OVF_I8_UN:
	case OP_ICONV_TO_OVF_U8_UN:
		/* an unsigned 32 bit num always fits in an (un)signed 64 bit one */
		/* Clean out the upper word */
		MONO_EMIT_NEW_BIALU_IMM (cfg, OP_ISHR_UN_IMM, ins->dreg, ins->sreg1, 0);
		ins->opcode = OP_NOP;
		break;
	case OP_LCONV_TO_OVF_I1:
		MONO_EMIT_NEW_COMPARE_IMM (cfg, ins->sreg1, 127);
		MONO_EMIT_NEW_COND_EXC (cfg, GT, "OverflowException");
		MONO_EMIT_NEW_COMPARE_IMM (cfg, ins->sreg1, -128);
		MONO_EMIT_NEW_COND_EXC (cfg, LT, "OverflowException");
		MONO_EMIT_NEW_UNALU (cfg, OP_LCONV_TO_I1, ins->dreg, ins->sreg1);
		ins->opcode = OP_NOP;
		break;
	case OP_LCONV_TO_OVF_I1_UN:
		MONO_EMIT_NEW_COMPARE_IMM (cfg, ins->sreg1, 127);
		MONO_EMIT_NEW_COND_EXC (cfg, GT_UN, "OverflowException");
		MONO_EMIT_NEW_UNALU (cfg, OP_LCONV_TO_I1, ins->dreg, ins->sreg1);
		ins->opcode = OP_NOP;
		break;
	case OP_LCONV_TO_OVF_U1:
		/* probe value to be within 0 to 255 */
		MONO_EMIT_NEW_COMPARE_IMM (cfg, ins->sreg1, 255);
		MONO_EMIT_NEW_COND_EXC (cfg, GT_UN, "OverflowException");
		MONO_EMIT_NEW_BIALU_IMM (cfg, OP_AND_IMM, ins->dreg, ins->sreg1, 0xff);
		ins->opcode = OP_NOP;
		break;
	case OP_LCONV_TO_OVF_U1_UN:
		/* probe value to be within 0 to 255 */
		MONO_EMIT_NEW_COMPARE_IMM (cfg, ins->sreg1, 255);
		MONO_EMIT_NEW_COND_EXC (cfg, GT_UN, "OverflowException");
		MONO_EMIT_NEW_BIALU_IMM (cfg, OP_AND_IMM, ins->dreg, ins->sreg1, 0xff);
		ins->opcode = OP_NOP;
		break;
	case OP_LCONV_TO_OVF_I2:
		/* Probe value to be within -32768 and 32767 */
		MONO_EMIT_NEW_COMPARE_IMM (cfg, ins->sreg1, 32767);
		MONO_EMIT_NEW_COND_EXC (cfg, GT, "OverflowException");
		MONO_EMIT_NEW_COMPARE_IMM (cfg, ins->sreg1, -32768);
		MONO_EMIT_NEW_COND_EXC (cfg, LT, "OverflowException");
		MONO_EMIT_NEW_UNALU (cfg, OP_LCONV_TO_I2, ins->dreg, ins->sreg1);
		ins->opcode = OP_NOP;
		break;
	case OP_LCONV_TO_OVF_I2_UN:
		/* Probe value to be within 0 and 32767 */
		MONO_EMIT_NEW_COMPARE_IMM (cfg, ins->sreg1, 32767);
		MONO_EMIT_NEW_COND_EXC (cfg, GT_UN, "OverflowException");
		MONO_EMIT_NEW_UNALU (cfg, OP_LCONV_TO_I2, ins->dreg, ins->sreg1);
		ins->opcode = OP_NOP;
		break;
	case OP_LCONV_TO_OVF_U2:
		/* Probe value to be within 0 and 65535 */
		MONO_EMIT_NEW_COMPARE_IMM (cfg, ins->sreg1, 0xffff);
		MONO_EMIT_NEW_COND_EXC (cfg, GT_UN, "OverflowException");
		MONO_EMIT_NEW_BIALU_IMM (cfg, OP_AND_IMM, ins->dreg, ins->sreg1, 0xffff);
		ins->opcode = OP_NOP;
		break;
	case OP_LCONV_TO_OVF_U2_UN:
		/* Probe value to be within 0 and 65535 */
		MONO_EMIT_NEW_COMPARE_IMM (cfg, ins->sreg1, 0xffff);
		MONO_EMIT_NEW_COND_EXC (cfg, GT_UN, "OverflowException");
		MONO_EMIT_NEW_BIALU_IMM (cfg, OP_AND_IMM, ins->dreg, ins->sreg1, 0xffff);
		ins->opcode = OP_NOP;
		break;
	case OP_LCONV_TO_OVF_I4:
		MONO_EMIT_NEW_COMPARE_IMM (cfg, ins->sreg1, 0x7fffffff);
		MONO_EMIT_NEW_COND_EXC (cfg, GT, "OverflowException");
		MONO_EMIT_NEW_COMPARE_IMM (cfg, ins->sreg1, -2147483648);
		MONO_EMIT_NEW_COND_EXC (cfg, LT, "OverflowException");
		MONO_EMIT_NEW_UNALU (cfg, OP_MOVE, ins->dreg, ins->sreg1);
		ins->opcode = OP_NOP;
		break;
	case OP_LCONV_TO_OVF_I4_UN:
		MONO_EMIT_NEW_COMPARE_IMM (cfg, ins->sreg1, 0x7fffffff);
		MONO_EMIT_NEW_COND_EXC (cfg, GT, "OverflowException");
		MONO_EMIT_NEW_UNALU (cfg, OP_MOVE, ins->dreg, ins->sreg1);
		ins->opcode = OP_NOP;
		break;
	case OP_LCONV_TO_OVF_U4:
		MONO_EMIT_NEW_COMPARE_IMM (cfg, ins->sreg1, 0xffffffffUL);
		MONO_EMIT_NEW_COND_EXC (cfg, GT, "OverflowException");
		MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, ins->sreg1, 0);
		MONO_EMIT_NEW_COND_EXC (cfg, LT, "OverflowException");
		MONO_EMIT_NEW_UNALU (cfg, OP_MOVE, ins->dreg, ins->sreg1);
		ins->opcode = OP_NOP;
		break;
	case OP_LCONV_TO_OVF_U4_UN:
		MONO_EMIT_NEW_COMPARE_IMM (cfg, ins->sreg1, 0xffffffff);
		MONO_EMIT_NEW_COND_EXC (cfg, GT_UN, "OverflowException");
		MONO_EMIT_NEW_UNALU (cfg, OP_MOVE, ins->dreg, ins->sreg1);
		ins->opcode = OP_NOP;
		break;
	case OP_LCONV_TO_OVF_I:
		ins->opcode = OP_MOVE;
		break;
	case OP_LCONV_TO_OVF_I_UN:
	case OP_LCONV_TO_OVF_I8_UN:
		MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, ins->sreg1, 0);
		MONO_EMIT_NEW_COND_EXC (cfg, LT, "OverflowException");
		MONO_EMIT_NEW_UNALU (cfg, OP_MOVE, ins->dreg, ins->sreg1);
		ins->opcode = OP_NOP;
		break;
	case OP_LCONV_TO_OVF_U8:
		MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, ins->sreg1, 0);
		MONO_EMIT_NEW_COND_EXC (cfg, LT, "OverflowException");

		MONO_EMIT_NEW_UNALU (cfg, OP_MOVE, ins->dreg, ins->sreg1);
		ins->opcode = OP_NOP;
		break;
#endif

	default: {
		MonoJitICallInfo *info;

		info = mono_find_jit_opcode_emulation (ins->opcode);
		if (info) {
			MonoInst **args;
			MonoInst *call;

			/* Create dummy MonoInst's for the arguments */
			g_assert (!info->sig->hasthis);
			g_assert (info->sig->param_count <= 2);

			args = mono_mempool_alloc0 (cfg->mempool, sizeof (MonoInst*) * info->sig->param_count);
			if (info->sig->param_count > 0) {
				MONO_INST_NEW (cfg, args [0], OP_ARG);
				args [0]->dreg = ins->sreg1;
			}
			if (info->sig->param_count > 1) {
				MONO_INST_NEW (cfg, args [1], OP_ARG);
				args [1]->dreg = ins->sreg2;
			}

			call = mono_emit_native_call (cfg, mono_icall_get_wrapper (info), info->sig, args, ins->cil_code);
			call->dreg = ins->dreg;

			ins->opcode = OP_NOP;
		}
		break;
	}
	}
}

/**
 * decompose_long_opts:
 *
 *  Decompose 64bit opcodes into 32bit opcodes on 32 bit platforms.
 */
void
mono_decompose_long_opts (MonoCompile *cfg)
{
#if SIZEOF_VOID_P == 4
	MonoBasicBlock *bb, *first_bb;

	/*
	 * Some opcodes, like lcall can't be decomposed so the rest of the JIT
	 * needs to be able to handle long vregs.
	 */

	/**
	 * Create a dummy bblock and emit code into it so we can use the normal 
	 * code generation macros.
	 */
	cfg->cbb = mono_mempool_alloc0 ((cfg)->mempool, sizeof (MonoBasicBlock));
	first_bb = cfg->cbb;

	for (bb = cfg->bb_entry; bb; bb = bb->next_bb) {
		MonoInst *tree = bb->code;	
		MonoInst *prev = NULL;

		   /*
		printf ("BEFORE LOWER_LONG_OPTS: %d:\n", bb->block_num);
		mono_print_bb_code_new (bb);
		*/

		tree = bb->code;
		cfg->cbb->code = cfg->cbb->last_ins = NULL;

		for (; tree; tree = tree->next) {
			switch (tree->opcode) {
			case OP_I8CONST:
				MONO_EMIT_NEW_ICONST (cfg, tree->dreg + 1, tree->inst_ls_word);
				MONO_EMIT_NEW_ICONST (cfg, tree->dreg + 2, tree->inst_ms_word);
				break;
			case OP_LMOVE:
			case OP_LCONV_TO_U8:
			case OP_LCONV_TO_I8:
				MONO_EMIT_NEW_UNALU (cfg, OP_MOVE, tree->dreg + 1, tree->sreg1 + 1);
				MONO_EMIT_NEW_UNALU (cfg, OP_MOVE, tree->dreg + 2, tree->sreg1 + 2);
				break;
			case OP_STOREI8_MEMBASE_REG:
				MONO_EMIT_NEW_STORE_MEMBASE (cfg, OP_STOREI4_MEMBASE_REG, tree->inst_destbasereg, tree->inst_offset + MINI_MS_WORD_OFFSET, tree->sreg1 + 2);
				MONO_EMIT_NEW_STORE_MEMBASE (cfg, OP_STOREI4_MEMBASE_REG, tree->inst_destbasereg, tree->inst_offset + MINI_LS_WORD_OFFSET, tree->sreg1 + 1);
				break;
			case OP_LOADI8_MEMBASE:
				MONO_EMIT_NEW_LOAD_MEMBASE_OP (cfg, OP_LOADI4_MEMBASE, tree->dreg + 2, tree->inst_basereg, tree->inst_offset + MINI_MS_WORD_OFFSET);
				MONO_EMIT_NEW_LOAD_MEMBASE_OP (cfg, OP_LOADI4_MEMBASE, tree->dreg + 1, tree->inst_basereg, tree->inst_offset + MINI_LS_WORD_OFFSET);
				break;

			case OP_ICONV_TO_I8: {
				guint32 tmpreg = alloc_ireg (cfg);

				/* branchless code:
				 * low = reg;
				 * tmp = low > -1 ? 1: 0;
				 * high = tmp - 1; if low is zero or pos high becomes 0, else -1
				 */
				MONO_EMIT_NEW_UNALU (cfg, OP_MOVE, tree->dreg + 1, tree->sreg1);
				MONO_EMIT_NEW_BIALU_IMM (cfg, OP_ICOMPARE_IMM, -1, tree->dreg + 1, -1);
				MONO_EMIT_NEW_BIALU (cfg, OP_ICGT, tmpreg, -1, -1);
				MONO_EMIT_NEW_BIALU_IMM (cfg, OP_ISUB_IMM, tree->dreg + 2, tmpreg, 1);
				break;
			}
			case OP_ICONV_TO_U8:
				MONO_EMIT_NEW_UNALU (cfg, OP_MOVE, tree->dreg + 1, tree->sreg1);
				MONO_EMIT_NEW_ICONST (cfg, tree->dreg + 2, 0);
				break;
			case OP_ICONV_TO_OVF_I8:
				/* a signed 32 bit num always fits in a signed 64 bit one */
				MONO_EMIT_NEW_BIALU_IMM (cfg, OP_SHR_IMM, tree->dreg + 2, tree->sreg1, 31);
				MONO_EMIT_NEW_UNALU (cfg, OP_MOVE, tree->dreg + 1, tree->sreg1);
				break;
			case OP_ICONV_TO_OVF_U8:
				MONO_EMIT_NEW_COMPARE_IMM (cfg, tree->sreg1, 0);
				MONO_EMIT_NEW_COND_EXC (cfg, LT, "OverflowException");
				MONO_EMIT_NEW_ICONST (cfg, tree->dreg + 2, 0);
				MONO_EMIT_NEW_UNALU (cfg, OP_MOVE, tree->dreg + 1, tree->sreg1);
				break;
			case OP_ICONV_TO_OVF_U8_UN:
				MONO_EMIT_NEW_ICONST (cfg, tree->dreg + 2, 0);
				MONO_EMIT_NEW_UNALU (cfg, OP_MOVE, tree->dreg + 1, tree->sreg1);
				break;
			case OP_LCONV_TO_I1:
				MONO_EMIT_NEW_UNALU (cfg, OP_ICONV_TO_I1, tree->dreg, tree->sreg1 + 1);
				break;
			case OP_LCONV_TO_U1:
				MONO_EMIT_NEW_UNALU (cfg, OP_ICONV_TO_U1, tree->dreg, tree->sreg1 + 1);
				break;
			case OP_LCONV_TO_I2:
				MONO_EMIT_NEW_UNALU (cfg, OP_ICONV_TO_I2, tree->dreg, tree->sreg1 + 1);
				break;
			case OP_LCONV_TO_U2:
				MONO_EMIT_NEW_UNALU (cfg, OP_ICONV_TO_U2, tree->dreg, tree->sreg1 + 1);
				break;
			case OP_LCONV_TO_I4:
			case OP_LCONV_TO_U4:
			case OP_LCONV_TO_I:
				MONO_EMIT_NEW_UNALU (cfg, OP_MOVE, tree->dreg, tree->sreg1 + 1);
				break;
			case OP_LCONV_TO_R8:
				MONO_EMIT_NEW_BIALU (cfg, OP_LCONV_TO_R8_2, tree->dreg, tree->sreg1 + 1, tree->sreg1 + 2);
				break;
			case OP_LCONV_TO_R4:
				MONO_EMIT_NEW_BIALU (cfg, OP_LCONV_TO_R4_2, tree->dreg, tree->sreg1 + 1, tree->sreg1 + 2);
				break;
			case OP_LCONV_TO_R_UN:
				MONO_EMIT_NEW_BIALU (cfg, OP_LCONV_TO_R_UN_2, tree->dreg, tree->sreg1 + 1, tree->sreg1 + 2);
				break;
			case OP_LCONV_TO_OVF_I1: {
				MonoBasicBlock *is_negative, *end_label;

				NEW_BBLOCK (cfg, is_negative);
				NEW_BBLOCK (cfg, end_label);

				MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, tree->sreg1 + 2, 0);
				MONO_EMIT_NEW_COND_EXC (cfg, GT, "OverflowException");
				MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, tree->sreg1 + 2, -1);
				MONO_EMIT_NEW_COND_EXC (cfg, LT, "OverflowException");

				MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, tree->sreg1 + 2, 0);
				MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_IBLT, is_negative);

				/* Positive */
				MONO_EMIT_NEW_COMPARE_IMM (cfg, tree->sreg1 + 1, 127);
				MONO_EMIT_NEW_COND_EXC (cfg, GT_UN, "OverflowException");
				MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_BR, end_label);

				/* Negative */
				MONO_START_BB (cfg, is_negative);
				MONO_EMIT_NEW_COMPARE_IMM (cfg, tree->sreg1 + 1, -128);
				MONO_EMIT_NEW_COND_EXC (cfg, LT_UN, "OverflowException");

				MONO_START_BB (cfg, end_label);

				MONO_EMIT_NEW_UNALU (cfg, OP_ICONV_TO_I1, tree->dreg, tree->sreg1 + 1);
				break;
			}
			case OP_LCONV_TO_OVF_I1_UN:
				MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, tree->sreg1 + 2, 0);
				MONO_EMIT_NEW_COND_EXC (cfg, NE_UN, "OverflowException");

				MONO_EMIT_NEW_COMPARE_IMM (cfg, tree->sreg1 + 1, 127);
				MONO_EMIT_NEW_COND_EXC (cfg, GT, "OverflowException");
				MONO_EMIT_NEW_COMPARE_IMM (cfg, tree->sreg1 + 1, -128);
				MONO_EMIT_NEW_COND_EXC (cfg, LT, "OverflowException");
				MONO_EMIT_NEW_UNALU (cfg, OP_ICONV_TO_I1, tree->dreg, tree->sreg1 + 1);
				break;
			case OP_LCONV_TO_OVF_U1:
			case OP_LCONV_TO_OVF_U1_UN:
				MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, tree->sreg1 + 2, 0);
				MONO_EMIT_NEW_COND_EXC (cfg, NE_UN, "OverflowException");

				/* probe value to be within 0 to 255 */
				MONO_EMIT_NEW_COMPARE_IMM (cfg, tree->sreg1 + 1, 255);
				MONO_EMIT_NEW_COND_EXC (cfg, GT_UN, "OverflowException");
				MONO_EMIT_NEW_BIALU_IMM (cfg, OP_AND_IMM, tree->dreg, tree->sreg1 + 1, 0xff);
				break;
			case OP_LCONV_TO_OVF_I2: {
				MonoBasicBlock *is_negative, *end_label;

				NEW_BBLOCK (cfg, is_negative);
				NEW_BBLOCK (cfg, end_label);

				MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, tree->sreg1 + 2, 0);
				MONO_EMIT_NEW_COND_EXC (cfg, GT, "OverflowException");
				MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, tree->sreg1 + 2, -1);
				MONO_EMIT_NEW_COND_EXC (cfg, LT, "OverflowException");

				MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, tree->sreg1 + 2, 0);
				MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_IBLT, is_negative);

				/* Positive */
				MONO_EMIT_NEW_COMPARE_IMM (cfg, tree->sreg1 + 1, 32767);
				MONO_EMIT_NEW_COND_EXC (cfg, GT_UN, "OverflowException");
				MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_BR, end_label);

				/* Negative */
				MONO_START_BB (cfg, is_negative);
				MONO_EMIT_NEW_COMPARE_IMM (cfg, tree->sreg1 + 1, -32768);
				MONO_EMIT_NEW_COND_EXC (cfg, LT_UN, "OverflowException");
				MONO_START_BB (cfg, end_label);

				MONO_EMIT_NEW_UNALU (cfg, OP_ICONV_TO_I2, tree->dreg, tree->sreg1 + 1);
				break;
			}
			case OP_LCONV_TO_OVF_I2_UN:
				MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, tree->sreg1 + 2, 0);
				MONO_EMIT_NEW_COND_EXC (cfg, NE_UN, "OverflowException");

				/* Probe value to be within -32768 and 32767 */
				MONO_EMIT_NEW_COMPARE_IMM (cfg, tree->sreg1 + 1, 32767);
				MONO_EMIT_NEW_COND_EXC (cfg, GT, "OverflowException");
				MONO_EMIT_NEW_COMPARE_IMM (cfg, tree->sreg1 + 1, -32768);
				MONO_EMIT_NEW_COND_EXC (cfg, LT, "OverflowException");
				MONO_EMIT_NEW_UNALU (cfg, OP_ICONV_TO_I2, tree->dreg, tree->sreg1 + 1);
				break;
			case OP_LCONV_TO_OVF_U2:
			case OP_LCONV_TO_OVF_U2_UN:
				MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, tree->sreg1 + 2, 0);
				MONO_EMIT_NEW_COND_EXC (cfg, NE_UN, "OverflowException");

				/* Probe value to be within 0 and 65535 */
				MONO_EMIT_NEW_COMPARE_IMM (cfg, tree->sreg1 + 1, 0xffff);
				MONO_EMIT_NEW_COND_EXC (cfg, GT_UN, "OverflowException");
				MONO_EMIT_NEW_BIALU_IMM (cfg, OP_AND_IMM, tree->dreg, tree->sreg1 + 1, 0xffff);
				break;
			case OP_LCONV_TO_OVF_I4:
			case OP_LCONV_TO_OVF_I:
				MONO_EMIT_NEW_BIALU (cfg, OP_LCONV_TO_OVF_I4_2, tree->dreg, tree->sreg1 + 1, tree->sreg1 + 2);
				break;
			case OP_LCONV_TO_OVF_U4:
			case OP_LCONV_TO_OVF_U4_UN:
				MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, tree->sreg1 + 2, 0);
				MONO_EMIT_NEW_COND_EXC (cfg, NE_UN, "OverflowException");
				MONO_EMIT_NEW_UNALU (cfg, OP_MOVE, tree->dreg, tree->sreg1 + 1);
				break;
			case OP_LCONV_TO_OVF_I_UN:
			case OP_LCONV_TO_OVF_I4_UN:
				MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, tree->sreg1 + 2, 0);
				MONO_EMIT_NEW_COND_EXC (cfg, NE_UN, "OverflowException");
				MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, tree->sreg1 + 1, 0);
				MONO_EMIT_NEW_COND_EXC (cfg, LT, "OverflowException");
				MONO_EMIT_NEW_UNALU (cfg, OP_MOVE, tree->dreg, tree->sreg1 + 1);
				break;
			case OP_LCONV_TO_OVF_U8:
				MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, tree->sreg1 + 2, 0);
				MONO_EMIT_NEW_COND_EXC (cfg, LT, "OverflowException");

				MONO_EMIT_NEW_UNALU (cfg, OP_MOVE, tree->dreg + 1, tree->sreg1 + 1);
				MONO_EMIT_NEW_UNALU (cfg, OP_MOVE, tree->dreg + 2, tree->sreg1 + 2);
				break;
			case OP_LCONV_TO_OVF_I8_UN:
				MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, tree->sreg1 + 2, 0);
				MONO_EMIT_NEW_COND_EXC (cfg, LT, "OverflowException");

				MONO_EMIT_NEW_UNALU (cfg, OP_MOVE, tree->dreg + 1, tree->sreg1 + 1);
				MONO_EMIT_NEW_UNALU (cfg, OP_MOVE, tree->dreg + 2, tree->sreg1 + 2);
				break;

			case OP_LADD:
				MONO_EMIT_NEW_BIALU (cfg, OP_IADDCC, tree->dreg + 1, tree->sreg1 + 1, tree->sreg2 + 1);
				MONO_EMIT_NEW_BIALU (cfg, OP_IADC, tree->dreg + 2, tree->sreg1 + 2, tree->sreg2 + 2);
				break;
			case OP_LSUB:
				MONO_EMIT_NEW_BIALU (cfg, OP_ISUBCC, tree->dreg + 1, tree->sreg1 + 1, tree->sreg2 + 1);
				MONO_EMIT_NEW_BIALU (cfg, OP_ISBB, tree->dreg + 2, tree->sreg1 + 2, tree->sreg2 + 2);
				break;
			case OP_LADD_OVF:
				/* ADC sets the condition code */
				MONO_EMIT_NEW_BIALU (cfg, OP_IADDCC, tree->dreg + 1, tree->sreg1 + 1, tree->sreg2 + 1);
				MONO_EMIT_NEW_BIALU (cfg, OP_IADC, tree->dreg + 2, tree->sreg1 + 2, tree->sreg2 + 2);
				MONO_EMIT_NEW_COND_EXC (cfg, OV, "OverflowException");
				break;
			case OP_LADD_OVF_UN:
				/* ADC sets the condition code */
				MONO_EMIT_NEW_BIALU (cfg, OP_IADDCC, tree->dreg + 1, tree->sreg1 + 1, tree->sreg2 + 1);
				MONO_EMIT_NEW_BIALU (cfg, OP_IADC, tree->dreg + 2, tree->sreg1 + 2, tree->sreg2 + 2);
				MONO_EMIT_NEW_COND_EXC (cfg, C, "OverflowException");
				break;
			case OP_LSUB_OVF:
				/* SBB sets the condition code */
				MONO_EMIT_NEW_BIALU (cfg, OP_ISUBCC, tree->dreg + 1, tree->sreg1 + 1, tree->sreg2 + 1);
				MONO_EMIT_NEW_BIALU (cfg, OP_ISBB, tree->dreg + 2, tree->sreg1 + 2, tree->sreg2 + 2);
				MONO_EMIT_NEW_COND_EXC (cfg, OV, "OverflowException");
				break;
			case OP_LSUB_OVF_UN:
				/* SBB sets the condition code */
				MONO_EMIT_NEW_BIALU (cfg, OP_ISUBCC, tree->dreg + 1, tree->sreg1 + 1, tree->sreg2 + 1);
				MONO_EMIT_NEW_BIALU (cfg, OP_ISBB, tree->dreg + 2, tree->sreg1 + 2, tree->sreg2 + 2);
				MONO_EMIT_NEW_COND_EXC (cfg, C, "OverflowException");
				break;
			case OP_LAND:
				MONO_EMIT_NEW_BIALU (cfg, OP_IAND, tree->dreg + 1, tree->sreg1 + 1, tree->sreg2 + 1);
				MONO_EMIT_NEW_BIALU (cfg, OP_IAND, tree->dreg + 2, tree->sreg1 + 2, tree->sreg2 + 2);
				break;
			case OP_LOR:
				MONO_EMIT_NEW_BIALU (cfg, OP_IOR, tree->dreg + 1, tree->sreg1 + 1, tree->sreg2 + 1);
				MONO_EMIT_NEW_BIALU (cfg, OP_IOR, tree->dreg + 2, tree->sreg1 + 2, tree->sreg2 + 2);
				break;
			case OP_LXOR:
				MONO_EMIT_NEW_BIALU (cfg, OP_IXOR, tree->dreg + 1, tree->sreg1 + 1, tree->sreg2 + 1);
				MONO_EMIT_NEW_BIALU (cfg, OP_IXOR, tree->dreg + 2, tree->sreg1 + 2, tree->sreg2 + 2);
				break;
			case OP_LNOT:
				MONO_EMIT_NEW_UNALU (cfg, OP_INOT, tree->dreg + 1, tree->sreg1 + 1);
				MONO_EMIT_NEW_UNALU (cfg, OP_INOT, tree->dreg + 2, tree->sreg1 + 2);
				break;
			case OP_LNEG:
				/* 
				 * FIXME: The original version in inssel-long32.brg does not work
				 * on x86, and the x86 version might not work on other archs ?
				 */
#ifndef __i386__
				NOT_IMPLEMENTED;
#endif
				MONO_EMIT_NEW_UNALU (cfg, OP_INEG, tree->dreg + 1, tree->sreg1 + 1);
				MONO_EMIT_NEW_BIALU_IMM (cfg, OP_ADC_IMM, tree->dreg + 2, tree->sreg1 + 2, 0);
				MONO_EMIT_NEW_UNALU (cfg, OP_INEG, tree->dreg + 2, tree->dreg + 2);
				break;
			case OP_LMUL:
				/* Emulated */
				/* FIXME: Add OP_BIGMUL optimization */
				break;

			case OP_LADD_IMM:
				MONO_EMIT_NEW_BIALU_IMM (cfg, OP_ADDCC_IMM, tree->dreg + 1, tree->sreg1 + 1, tree->inst_ls_word);
				MONO_EMIT_NEW_BIALU_IMM (cfg, OP_ADC_IMM, tree->dreg + 2, tree->sreg1 + 2, tree->inst_ms_word);
				break;
			case OP_LSUB_IMM:
				MONO_EMIT_NEW_BIALU_IMM (cfg, OP_SUBCC_IMM, tree->dreg + 1, tree->sreg1 + 1, tree->inst_ls_word);
				MONO_EMIT_NEW_BIALU_IMM (cfg, OP_SBB_IMM, tree->dreg + 2, tree->sreg1 + 2, tree->inst_ms_word);
				break;
			case OP_LAND_IMM:
				MONO_EMIT_NEW_BIALU_IMM (cfg, OP_AND_IMM, tree->dreg + 1, tree->sreg1 + 1, tree->inst_ls_word);
				MONO_EMIT_NEW_BIALU_IMM (cfg, OP_AND_IMM, tree->dreg + 2, tree->sreg1 + 2, tree->inst_ms_word);
				break;
			case OP_LOR_IMM:
				MONO_EMIT_NEW_BIALU_IMM (cfg, OP_OR_IMM, tree->dreg + 1, tree->sreg1 + 1, tree->inst_ls_word);
				MONO_EMIT_NEW_BIALU_IMM (cfg, OP_OR_IMM, tree->dreg + 2, tree->sreg1 + 2, tree->inst_ms_word);
				break;
			case OP_LXOR_IMM:
				MONO_EMIT_NEW_BIALU_IMM (cfg, OP_XOR_IMM, tree->dreg + 1, tree->sreg1 + 1, tree->inst_ls_word);
				MONO_EMIT_NEW_BIALU_IMM (cfg, OP_XOR_IMM, tree->dreg + 2, tree->sreg1 + 2, tree->inst_ms_word);
				break;
			case OP_LSHR_UN_IMM:
				if (tree->inst_c1 == 32) {

					/* The original code had this comment: */
					/* special case that gives a nice speedup and happens to workaorund a ppc jit but (for the release)
					 * later apply the speedup to the left shift as well
					 * See BUG# 57957.
					 */
					/* FIXME: Move this to the strenght reduction pass */
					/* just move the upper half to the lower and zero the high word */
					MONO_EMIT_NEW_UNALU (cfg, OP_MOVE, tree->dreg + 1, tree->sreg1 + 2);
					MONO_EMIT_NEW_ICONST (cfg, tree->dreg + 2, 0);
				}
				break;
			case OP_LSHL_IMM:
				if (tree->inst_c1 == 32) {
					/* just move the lower half to the upper and zero the lower word */
					MONO_EMIT_NEW_UNALU (cfg, OP_MOVE, tree->dreg + 2, tree->sreg1 + 1);
					MONO_EMIT_NEW_ICONST (cfg, tree->dreg + 1, 0);
				}
				break;

			case OP_LCOMPARE: {
				MonoInst *next = tree->next;

				g_assert (next);

				switch (next->opcode) {
				case OP_LBEQ:
					MONO_EMIT_NEW_BIALU (cfg, OP_COMPARE, -1, tree->sreg1 + 1, tree->sreg2 + 1);
					MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_IBNE_UN, next->inst_false_bb);
					MONO_EMIT_NEW_BIALU (cfg, OP_COMPARE, -1, tree->sreg1 + 2, tree->sreg2 + 2);
					MONO_EMIT_NEW_BRANCH_BLOCK2 (cfg, OP_IBEQ, next->inst_true_bb, next->inst_false_bb);
					next->opcode = OP_NOP;
					break;
				case OP_LBNE_UN:
					MONO_EMIT_NEW_BIALU (cfg, OP_COMPARE, -1, tree->sreg1 + 1, tree->sreg2 + 1);
					MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_IBNE_UN, next->inst_true_bb);
					MONO_EMIT_NEW_BIALU (cfg, OP_COMPARE, -1, tree->sreg1 + 2, tree->sreg2 + 2);
					MONO_EMIT_NEW_BRANCH_BLOCK2 (cfg, OP_IBNE_UN, next->inst_true_bb, next->inst_false_bb);
					next->opcode = OP_NOP;
					break;
				case OP_LBLT:
					MONO_EMIT_NEW_BIALU (cfg, OP_COMPARE, -1, tree->sreg1 + 2, tree->sreg2 + 2);
					MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_IBLT, next->inst_true_bb);
					MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_IBNE_UN, next->inst_false_bb);
					MONO_EMIT_NEW_BIALU (cfg, OP_COMPARE, -1, tree->sreg1 + 1, tree->sreg2 + 1);
					MONO_EMIT_NEW_BRANCH_BLOCK2 (cfg, OP_IBLT_UN, next->inst_true_bb, next->inst_false_bb);
					next->opcode = OP_NOP;
					break;
				case OP_LBGT:
					MONO_EMIT_NEW_BIALU (cfg, OP_COMPARE, -1, tree->sreg1 + 2, tree->sreg2 + 2);
					MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_IBGT, next->inst_true_bb);
					MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_IBNE_UN, next->inst_false_bb);
					MONO_EMIT_NEW_BIALU (cfg, OP_COMPARE, -1, tree->sreg1 + 1, tree->sreg2 + 1);
					MONO_EMIT_NEW_BRANCH_BLOCK2 (cfg, OP_IBGT_UN, next->inst_true_bb, next->inst_false_bb);
					next->opcode = OP_NOP;
					break;
				case OP_LBGE:
					MONO_EMIT_NEW_BIALU (cfg, OP_COMPARE, -1, tree->sreg1 + 2, tree->sreg2 + 2);
					MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_IBGT, next->inst_true_bb);
					MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_IBNE_UN, next->inst_false_bb);
					MONO_EMIT_NEW_BIALU (cfg, OP_COMPARE, -1, tree->sreg1 + 1, tree->sreg2 + 1);
					MONO_EMIT_NEW_BRANCH_BLOCK2 (cfg, OP_IBGE_UN, next->inst_true_bb, next->inst_false_bb);
					next->opcode = OP_NOP;
					break;
				case OP_LBLE:
					MONO_EMIT_NEW_BIALU (cfg, OP_COMPARE, -1, tree->sreg1 + 2, tree->sreg2 + 2);
					MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_IBLT, next->inst_true_bb);
					MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_IBNE_UN, next->inst_false_bb);
					MONO_EMIT_NEW_BIALU (cfg, OP_COMPARE, -1, tree->sreg1 + 1, tree->sreg2 + 1);
					MONO_EMIT_NEW_BRANCH_BLOCK2 (cfg, OP_IBLE_UN, next->inst_true_bb, next->inst_false_bb);
					next->opcode = OP_NOP;
					break;
				case OP_LBLT_UN:
					MONO_EMIT_NEW_BIALU (cfg, OP_COMPARE, -1, tree->sreg1 + 2, tree->sreg2 + 2);
					MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_IBLT_UN, next->inst_true_bb);
					MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_IBNE_UN, next->inst_false_bb);
					MONO_EMIT_NEW_BIALU (cfg, OP_COMPARE, -1, tree->sreg1 + 1, tree->sreg2 + 1);
					MONO_EMIT_NEW_BRANCH_BLOCK2 (cfg, OP_IBLT_UN, next->inst_true_bb, next->inst_false_bb);
					next->opcode = OP_NOP;
					break;
				case OP_LBGT_UN:
					MONO_EMIT_NEW_BIALU (cfg, OP_COMPARE, -1, tree->sreg1 + 2, tree->sreg2 + 2);
					MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_IBGT_UN, next->inst_true_bb);
					MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_IBNE_UN, next->inst_false_bb);
					MONO_EMIT_NEW_BIALU (cfg, OP_COMPARE, -1, tree->sreg1 + 1, tree->sreg2 + 1);
					MONO_EMIT_NEW_BRANCH_BLOCK2 (cfg, OP_IBGT_UN, next->inst_true_bb, next->inst_false_bb);
					next->opcode = OP_NOP;
					break;
				case OP_LBGE_UN:
					MONO_EMIT_NEW_BIALU (cfg, OP_COMPARE, -1, tree->sreg1 + 2, tree->sreg2 + 2);
					MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_IBGT_UN, next->inst_true_bb);
					MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_IBNE_UN, next->inst_false_bb);
					MONO_EMIT_NEW_BIALU (cfg, OP_COMPARE, -1, tree->sreg1 + 1, tree->sreg2 + 1);
					MONO_EMIT_NEW_BRANCH_BLOCK2 (cfg, OP_IBGE_UN, next->inst_true_bb, next->inst_false_bb);
					next->opcode = OP_NOP;
					break;
				case OP_LBLE_UN:
					MONO_EMIT_NEW_BIALU (cfg, OP_COMPARE, -1, tree->sreg1 + 2, tree->sreg2 + 2);
					MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_IBLT_UN, next->inst_true_bb);
					MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_IBNE_UN, next->inst_false_bb);
					MONO_EMIT_NEW_BIALU (cfg, OP_COMPARE, -1, tree->sreg1 + 1, tree->sreg2 + 1);
					MONO_EMIT_NEW_BRANCH_BLOCK2 (cfg, OP_IBLE_UN, next->inst_true_bb, next->inst_false_bb);
					next->opcode = OP_NOP;
					break;
				case OP_LCEQ: {
					MonoBasicBlock *word_differs;
	
					NEW_BBLOCK (cfg, word_differs);

					MONO_EMIT_NEW_ICONST (cfg, next->dreg, 0);
					MONO_EMIT_NEW_BIALU (cfg, OP_COMPARE, -1, tree->sreg1 + 2, tree->sreg2 + 2);
					MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_IBNE_UN, word_differs);
					MONO_EMIT_NEW_BIALU (cfg, OP_COMPARE, -1, tree->sreg1 + 1, tree->sreg2 + 1);
					MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_IBNE_UN, word_differs);
					MONO_EMIT_NEW_ICONST (cfg, next->dreg, 1);
					MONO_START_BB (cfg, word_differs);
					next->opcode = OP_NOP;
					break;
				}
				case OP_LCLT: {
					MonoBasicBlock *set_to_0, *set_to_1;
	
					NEW_BBLOCK (cfg, set_to_0);
					NEW_BBLOCK (cfg, set_to_1);

					MONO_EMIT_NEW_ICONST (cfg, next->dreg, 0);
					MONO_EMIT_NEW_BIALU (cfg, OP_COMPARE, -1, tree->sreg1 + 2, tree->sreg2 + 2);
					MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_IBGT, set_to_0);
					MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_IBNE_UN, set_to_1);
					MONO_EMIT_NEW_BIALU (cfg, OP_COMPARE, -1, tree->sreg1 + 1, tree->sreg2 + 1);
					MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_IBGE_UN, set_to_0);
					MONO_START_BB (cfg, set_to_1);
					MONO_EMIT_NEW_ICONST (cfg, next->dreg, 1);
					MONO_START_BB (cfg, set_to_0);
					next->opcode = OP_NOP;
					break;	
				}
				case OP_LCLT_UN: {
					MonoBasicBlock *set_to_0, *set_to_1;
	
					NEW_BBLOCK (cfg, set_to_0);
					NEW_BBLOCK (cfg, set_to_1);

					MONO_EMIT_NEW_ICONST (cfg, next->dreg, 0);
					MONO_EMIT_NEW_BIALU (cfg, OP_COMPARE, -1, tree->sreg1 + 2, tree->sreg2 + 2);
					MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_IBGT_UN, set_to_0);
					MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_IBNE_UN, set_to_1);
					MONO_EMIT_NEW_BIALU (cfg, OP_COMPARE, -1, tree->sreg1 + 1, tree->sreg2 + 1);
					MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_IBGE_UN, set_to_0);
					MONO_START_BB (cfg, set_to_1);
					MONO_EMIT_NEW_ICONST (cfg, next->dreg, 1);
					MONO_START_BB (cfg, set_to_0);
					next->opcode = OP_NOP;
					break;	
				}
				case OP_LCGT: {
					MonoBasicBlock *set_to_0, *set_to_1;
	
				    NEW_BBLOCK (cfg, set_to_0);
					NEW_BBLOCK (cfg, set_to_1);

					MONO_EMIT_NEW_ICONST (cfg, next->dreg, 0);
					MONO_EMIT_NEW_BIALU (cfg, OP_COMPARE, -1, tree->sreg2 + 2, tree->sreg1 + 2);
					MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_IBGT, set_to_0);
					MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_IBNE_UN, set_to_1);
					MONO_EMIT_NEW_BIALU (cfg, OP_COMPARE, -1, tree->sreg2 + 1, tree->sreg1 + 1);
					MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_IBGE_UN, set_to_0);
					MONO_START_BB (cfg, set_to_1);
					MONO_EMIT_NEW_ICONST (cfg, next->dreg, 1);
					MONO_START_BB (cfg, set_to_0);
					next->opcode = OP_NOP;
					break;
				}
				case OP_LCGT_UN: {
					MonoBasicBlock *set_to_0, *set_to_1;
	
					NEW_BBLOCK (cfg, set_to_0);
					NEW_BBLOCK (cfg, set_to_1);

					MONO_EMIT_NEW_ICONST (cfg, next->dreg, 0);
					MONO_EMIT_NEW_BIALU (cfg, OP_COMPARE, -1, tree->sreg2 + 2, tree->sreg1 + 2);
					MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_IBGT_UN, set_to_0);
					MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_IBNE_UN, set_to_1);
					MONO_EMIT_NEW_BIALU (cfg, OP_COMPARE, -1, tree->sreg2 + 1, tree->sreg1 + 1);
					MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_IBGE_UN, set_to_0);
					MONO_START_BB (cfg, set_to_1);
					MONO_EMIT_NEW_ICONST (cfg, next->dreg, 1);
					MONO_START_BB (cfg, set_to_0);
					next->opcode = OP_NOP;
					break;
				}
				default:
					g_assert_not_reached ();
				}
			}
			default:
				break;
			}

			if (cfg->cbb->code || (cfg->cbb != first_bb)) {
				/* Replace the original instruction with the new code sequence */

				if (cfg->cbb == first_bb) {
					/* 
					 * Only one replacement bb, merge the code into
					 * the current bb.
					 */

					/* Head */
					if (prev)
						prev->next = first_bb->code;
					else
						bb->code = first_bb->code;

					/* Tail */
					cfg->cbb->last_ins->next = tree->next;
					if (tree->next == NULL)
						bb->last_ins = cfg->cbb->last_ins;
					prev = cfg->cbb->last_ins;
				}
				else {
					int i;
					MonoInst *next = tree->next;

					if (next && next->opcode == OP_NOP)
						/* Avoid NOPs following branches */
						next = next->next;

					/* Multiple BBs */

					/* Split the original bb */
					tree->next = NULL;
					bb->last_ins = tree;

					/* Merge the second part of the original bb into the last bb */
					if (cfg->cbb->last_ins)
						cfg->cbb->last_ins->next = next;
					else {
						MonoInst *last;

						cfg->cbb->code = next;

						if (next) {
							for (last = next; last->next != NULL; last = last->next)
								;
							cfg->cbb->last_ins = last;
						}
					}

					for (i = 0; i < bb->out_count; ++i)
						link_bblock (cfg, cfg->cbb, bb->out_bb [i]);

					/* Merge the first (dummy) bb to the original bb */
					if (prev)
						prev->next = first_bb->code;
					else
						bb->code = first_bb->code;
					bb->last_ins = first_bb->last_ins;
					for (i = 0; i < first_bb->out_count; ++i)
						link_bblock (cfg, bb, first_bb->out_bb [i]);

					cfg->cbb->next_bb = bb->next_bb;
					bb->next_bb = first_bb->next_bb;

					prev = NULL;
				}

				first_bb->code = first_bb->last_ins = NULL;
				first_bb->in_count = first_bb->out_count = 0;
				cfg->cbb = first_bb;
			}
			else
				prev = tree;
		}
	}
#endif

	/*
	for (bb = cfg->bb_entry; bb; bb = bb->next_bb)
		mono_print_bb (bb, "AFTER LOWER-LONG-OPTS");
	*/
}

/*
 * mono_method_to_ir: translates IL into basic blocks containing trees
 */
int
mono_method_to_ir2 (MonoCompile *cfg, MonoMethod *method, MonoBasicBlock *start_bblock, MonoBasicBlock *end_bblock, 
		   int locals_offset, MonoInst *return_var, GList *dont_inline, MonoInst **inline_args, 
		   guint inline_offset, gboolean is_virtual_call)
{
	MonoInst *ins, **sp, **stack_start;
	MonoBasicBlock *bblock, *tblock = NULL, *init_localsbb = NULL;
	GHashTable *bbhash;
	MonoMethod *cmethod;
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
	GList *bb_recheck = NULL, *tmp;
	int i, n, start_new_bblock, dreg;
	int num_calls = 0, inline_costs = 0;
	int breakpoint_id = 0;
	guint num_args;
	MonoBoolean security, pinvoke;
	MonoSecurityManager* secman = NULL;
	MonoDeclSecurityActions actions;
	GSList *class_inits = NULL;

	image = method->klass->image;
	header = mono_method_get_header (method);
	generic_container = method->generic_container;
	sig = mono_method_signature (method);
	num_args = sig->hasthis + sig->param_count;
	ip = (unsigned char*)header->code;
	end = ip + header->code_size;
	mono_jit_stats.cil_code_size += header->code_size;

	if (sig->is_inflated)
		generic_context = ((MonoMethodInflated *) method)->context;
	else if (generic_container)
		generic_context = &generic_container->context;

	g_assert (!sig->has_type_parameters);

	if (cfg->method == method) {
		cfg->real_offset = 0;
		bbhash = cfg->bb_hash;
	} else {
		cfg->real_offset = inline_offset;
		bbhash = g_hash_table_new (g_direct_hash, NULL);
	}

	cfg->cbb_hash = bbhash;

	if (cfg->verbose_level > 2)
		printf ("method to IR %s\n", mono_method_full_name (method, TRUE));

	dont_inline = g_list_prepend (dont_inline, method);
	if (cfg->method == method) {

		if (cfg->method->save_lmf)
			/* Needed by the prolog code */
			mono_get_got_var (cfg);

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

		arg_array = alloca (sizeof (MonoInst *) * num_args);
		for (i = num_args - 1; i >= 0; i--)
			arg_array [i] = cfg->varinfo [i];

		if (header->num_clauses) {
			cfg->spvars = g_hash_table_new (NULL, NULL);
			cfg->exvars = g_hash_table_new (NULL, NULL);
		}
		/* handle exception clauses */
		for (i = 0; i < header->num_clauses; ++i) {
			MonoBasicBlock *try_bb;
			MonoExceptionClause *clause = &header->clauses [i];
			GET_BBLOCK (cfg, bbhash, try_bb, ip + clause->try_offset);
			try_bb->real_offset = clause->try_offset;
			GET_BBLOCK (cfg, bbhash, tblock, ip + clause->handler_offset);
			tblock->real_offset = clause->handler_offset;
			tblock->flags |= BB_EXCEPTION_HANDLER;

			link_bblock (cfg, try_bb, tblock);

			if (*(ip + clause->handler_offset) == CEE_POP)
				tblock->flags |= BB_EXCEPTION_DEAD_OBJ;

			if (clause->flags == MONO_EXCEPTION_CLAUSE_FINALLY ||
			    clause->flags == MONO_EXCEPTION_CLAUSE_FILTER) {
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
					GET_BBLOCK (cfg, bbhash, tblock, ip + clause->data.filter_offset);
					tblock->real_offset = clause->data.filter_offset;
					tblock->in_scount = 1;
					tblock->in_stack = mono_mempool_alloc (cfg->mempool, sizeof (MonoInst*));
					/* The filter block shares the exvar with the handler block */
					tblock->in_stack [0] = mono_create_exvar_for_offset (cfg, clause->handler_offset);
					MONO_INST_NEW (cfg, ins, OP_START_HANDLER);
					MONO_ADD_INS (tblock, ins);
				}
			}
		}
	} else {
		arg_array = alloca (sizeof (MonoInst *) * num_args);
		cfg->cbb = start_bblock;
		mono_save_args (cfg, sig, inline_args, arg_array);
	}

	/* FIRST CODE BLOCK */
	NEW_BBLOCK (cfg, bblock);
	bblock->cil_code = ip;
	cfg->cbb = bblock;

	ADD_BBLOCK (cfg, bbhash, bblock);

	if (cfg->method == method) {
		breakpoint_id = mono_debugger_method_has_breakpoint (method);
		if (breakpoint_id && (mono_debug_format != MONO_DEBUG_FORMAT_DEBUGGER)) {
			MONO_INST_NEW (cfg, ins, OP_BREAK);
			MONO_ADD_INS (bblock, ins);
		}
	}

	if (mono_use_security_manager)
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

			if (pinvoke) {
				custom = mono_custom_attrs_from_class (wrapped->klass);
				if (custom && mono_custom_attrs_has_attr (custom, secman->suppressunmanagedcodesecurity)) {
					pinvoke = FALSE;
				}
			}
		} else {
			/* not a P/Invoke after all */
			pinvoke = FALSE;
		}
	}
	
	if ((header->init_locals || (cfg->method == method && (cfg->opt & MONO_OPT_SHARED))) || mono_compile_aot || security || pinvoke) {
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
			NEW_DECLSECCONST (cfg, args[0], image, actions.demand);
			NEW_ICONST (cfg, args [1], actions.demand.size);
			/* Calls static void SecurityManager.InternalDemand (byte* permissions, int size); */
			mono_emit_method_call (cfg, secman->demand, mono_method_signature (secman->demand), args, ip, NULL);
		}
		if (actions.noncasdemand.blob) {
			/* CLR 1.x uses a .noncasdemand (but 2.x doesn't) */
			/* For Mono we re-route non-CAS Demand to Demand (as the managed code must deal with it anyway) */
			NEW_DECLSECCONST (cfg, args[0], image, actions.noncasdemand);
			NEW_ICONST (cfg, args [1], actions.noncasdemand.size);
			/* Calls static void SecurityManager.InternalDemand (byte* permissions, int size); */
			mono_emit_method_call (cfg, secman->demand, mono_method_signature (secman->demand), args, ip, NULL);
		}
		if (actions.demandchoice.blob) {
			/* New in 2.0, Demand must succeed for one of the permissions (i.e. not all) */
			NEW_DECLSECCONST (cfg, args[0], image, actions.demandchoice);
			NEW_ICONST (cfg, args [1], actions.demandchoice.size);
			/* Calls static void SecurityManager.InternalDemandChoice (byte* permissions, int size); */
			mono_emit_method_call (cfg, secman->demandchoice, mono_method_signature (secman->demandchoice), args, ip, NULL);
		}
	}

	/* we must Demand SecurityPermission.Unmanaged before p/invoking */
	if (pinvoke) {
		mono_emit_method_call (cfg, secman->demandunmanaged, mono_method_signature (secman->demandunmanaged), NULL, ip, NULL);
	}

	if (get_basic_blocks (cfg, bbhash, header, cfg->real_offset, ip, end, &err_pos)) {
		ip = err_pos;
		goto unverified;
	}

	if (cfg->method == method)
		mono_debug_init_method (cfg, bblock, breakpoint_id);

	param_types = mono_mempool_alloc (cfg->mempool, sizeof (MonoType*) * num_args);
	if (sig->hasthis)
		param_types [0] = method->klass->valuetype?&method->klass->this_arg:&method->klass->byval_arg;
	for (n = 0; n < sig->param_count; ++n)
		param_types [n + sig->hasthis] = sig->params [n];
	class_inits = NULL;

	/* add a check for this != NULL to inlined methods */
	if (is_virtual_call) {
		MonoInst *arg_ins;

		NEW_ARGLOAD (cfg, arg_ins, 0);
		MONO_ADD_INS (cfg->cbb, arg_ins);
		MONO_EMIT_NEW_UNALU (cfg, OP_CHECK_THIS, -1, arg_ins->dreg);
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

		if (start_new_bblock) {
			bblock->cil_length = ip - bblock->cil_code;
			if (start_new_bblock == 2) {
				g_assert (ip == tblock->cil_code);
			} else {
				GET_BBLOCK (cfg, bbhash, tblock, ip);
			}
			bblock->next_bb = tblock;
			bblock = tblock;
			cfg->cbb = bblock;
			start_new_bblock = 0;
			for (i = 0; i < bblock->in_scount; ++i) {
				if (cfg->verbose_level > 3)
					printf ("loading %d from temp %d\n", i, (int)bblock->in_stack [i]->inst_c0);						
				NEW_TEMPLOAD (cfg, ins, bblock->in_stack [i]->inst_c0);
				if (ins->opcode == CEE_LDOBJ)
					ins = emit_ldobj_var (cfg, bblock->in_stack [i], ip);
				else
					MONO_ADD_INS (cfg->cbb, ins);
				*sp++ = ins;
			}
			g_slist_free (class_inits);
			class_inits = NULL;
		} else {
			if ((tblock = g_hash_table_lookup (bbhash, ip)) && (tblock != bblock)) {
				link_bblock (cfg, bblock, tblock);
				if (sp != stack_start) {
					handle_stack_args (cfg, stack_start, sp - stack_start);
					sp = stack_start;
				}
				bblock->next_bb = tblock;
				bblock = tblock;
				cfg->cbb = bblock;
				for (i = 0; i < bblock->in_scount; ++i) {
					if (cfg->verbose_level > 3)
						printf ("loading %d from temp %d\n", i, (int)bblock->in_stack [i]->inst_c0);						
					NEW_TEMPLOAD (cfg, ins, bblock->in_stack [i]->inst_c0);
					if (ins->opcode == CEE_LDOBJ)
						ins = emit_ldobj_var (cfg, bblock->in_stack [i], ip);
					else
						MONO_ADD_INS (cfg->cbb, ins);
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
#if defined(__i386__)
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
		case CEE_BREAK:
			MONO_INST_NEW (cfg, ins, (*ip) == CEE_NOP ? OP_NOP : OP_BREAK);
			ins->cil_code = ip++;
			MONO_ADD_INS (bblock, ins);
			break;
		case CEE_LDARG_0:
		case CEE_LDARG_1:
		case CEE_LDARG_2:
		case CEE_LDARG_3:
			CHECK_STACK_OVF (1);
			n = (*ip)-CEE_LDARG_0;
			CHECK_ARG (n);
			NEW_ARGLOAD (cfg, ins, n);
			ins->cil_code = ip++;
			if (ins->opcode == CEE_LDOBJ) {
				MonoInst *src;

				EMIT_NEW_ARGLOADA (cfg, src, n);
				ins = emit_ldobj (cfg, src, ip, ins->klass);
			}
			else
				MONO_ADD_INS (cfg->cbb, ins);
			*sp++ = ins;
			break;
		case CEE_LDLOC_0:
		case CEE_LDLOC_1:
		case CEE_LDLOC_2:
		case CEE_LDLOC_3:
			CHECK_STACK_OVF (1);
			n = (*ip)-CEE_LDLOC_0;
			CHECK_LOCAL (n);
			NEW_LOCLOAD (cfg, ins, n);
			ins->cil_code = ip++;

			if (ins->opcode == CEE_LDOBJ) {
				MonoInst *src;

				EMIT_NEW_LOCLOADA (cfg, src, n);
				*sp++ = emit_ldobj (cfg, src, ip, ins->klass);
			} else {
				MONO_ADD_INS (bblock, ins);
				*sp++ = ins;
			}
			break;
		case CEE_STLOC_0:
		case CEE_STLOC_1:
		case CEE_STLOC_2:
		case CEE_STLOC_3: {
			guint32 opcode;

			CHECK_STACK (1);
			n = (*ip)-CEE_STLOC_0;
			CHECK_LOCAL (n);
			--sp;

			opcode = mono_type_to_regstore (header->locals [n]);
			if ((opcode == OP_MOVE) && ((sp [0]->opcode == OP_ICONST) || (sp [0]->opcode == OP_I8CONST))) {
				/* Optimize reg-reg moves away */
				/* 
				 * Can't optimize other opcodes, since sp[0] might point to
				 * the last ins of a decomposed opcode.
				 */
				sp [0]->dreg = (cfg)->varinfo [locals_offset + n]->dreg;
			} else {
				NEW_LOCSTORE (cfg, ins, n, *sp);
				ins->cil_code = ip;

				if (ins->opcode == CEE_STOBJ) {
					EMIT_NEW_LOCLOADA (cfg, ins, n);
					emit_stobj (cfg, ins, *sp, ip, ins->klass, FALSE);
				} else
					MONO_ADD_INS (bblock, ins);
			}
			++ip;
			inline_costs += 1;
			break;
			}
		case CEE_LDARG_S:
			CHECK_OPSIZE (2);
			CHECK_STACK_OVF (1);
			n = ip [1];
			CHECK_ARG (n);
			NEW_ARGLOAD (cfg, ins, n);
			ins->cil_code = ip;
			if (ins->opcode == CEE_LDOBJ) {
				MonoInst *src;

				EMIT_NEW_ARGLOADA (cfg, src, n);
				ins = emit_ldobj (cfg, src, ip, ins->klass);
			}
			else
				MONO_ADD_INS (cfg->cbb, ins);
			*sp++ = ins;
			ip += 2;
			break;
		case CEE_LDARGA_S:
			CHECK_OPSIZE (2);
			CHECK_STACK_OVF (1);
			n = ip [1];
			CHECK_ARG (n);
			NEW_ARGLOADA (cfg, ins, n);
			ins->cil_code = ip;
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
			NEW_ARGSTORE (cfg, ins, n, *sp);
			ins->cil_code = ip;
			if (ins->opcode == CEE_STOBJ) {
				EMIT_NEW_ARGLOADA (cfg, ins, n);
				emit_stobj (cfg, ins, *sp, ip, ins->klass, FALSE);
			} else
				MONO_ADD_INS (cfg->cbb, ins);
			ip += 2;
			break;
		case CEE_LDLOC_S:
			CHECK_OPSIZE (2);
			CHECK_STACK_OVF (1);
			n = ip [1];
			CHECK_LOCAL (n);
			NEW_LOCLOAD (cfg, ins, n);
			ins->cil_code = ip;

			if (ins->opcode == CEE_LDOBJ) {
				MonoInst *src;

				EMIT_NEW_LOCLOADA (cfg, src, n);
				*sp++ = emit_ldobj (cfg, src, ip, ins->klass);
			} else {
				MONO_ADD_INS (cfg->cbb, ins);
				*sp++ = ins;
			}
			ip += 2;
			break;
		case CEE_LDLOCA_S:
			CHECK_OPSIZE (2);
			CHECK_STACK_OVF (1);
			CHECK_LOCAL (ip [1]);
			EMIT_NEW_LOCLOADA (cfg, ins, ip [1]);
			ins->cil_code = ip;
			*sp++ = ins;
			ip += 2;
			break;
		case CEE_STLOC_S:
			CHECK_OPSIZE (2);
			CHECK_STACK (1);
			--sp;
			CHECK_LOCAL (ip [1]);
			NEW_LOCSTORE (cfg, ins, ip [1], *sp);
			ins->cil_code = ip;
			if (ins->opcode == CEE_STOBJ) {
				EMIT_NEW_LOCLOADA (cfg, ins, ip [1]);
				emit_stobj (cfg, ins, *sp, ip, ins->klass, FALSE);
			} else
				MONO_ADD_INS (bblock, ins);
			ip += 2;
			inline_costs += 1;
			break;
		case CEE_LDNULL:
			CHECK_STACK_OVF (1);
			EMIT_NEW_PCONST (cfg, ins, NULL);
			ins->cil_code = ip;
			ins->type = STACK_OBJ;
			++ip;
			*sp++ = ins;
			break;
		case CEE_LDC_I4_M1:
			CHECK_STACK_OVF (1);
			EMIT_NEW_ICONST (cfg, ins, -1);
			ins->cil_code = ip;
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
			ins->cil_code = ip;
			++ip;
			*sp++ = ins;
			break;
		case CEE_LDC_I4_S:
			CHECK_OPSIZE (2);
			CHECK_STACK_OVF (1);
			++ip;
			EMIT_NEW_ICONST (cfg, ins, *((signed char*)ip));
			ins->cil_code = ip;
			++ip;
			*sp++ = ins;
			break;
		case CEE_LDC_I4:
			CHECK_OPSIZE (5);
			CHECK_STACK_OVF (1);
			EMIT_NEW_ICONST (cfg, ins, (gint32)read32 (ip + 1));
			ins->cil_code = ip;
			ip += 5;
			*sp++ = ins;
			break;
		case CEE_LDC_I8:
			CHECK_OPSIZE (9);
			CHECK_STACK_OVF (1);
			MONO_INST_NEW (cfg, ins, OP_I8CONST);
			ins->cil_code = ip;
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
			f = mono_mempool_alloc (cfg->domain->mp, sizeof (float));
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
			d = mono_mempool_alloc (cfg->domain->mp, sizeof (double));
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
			temp->flags |= MONO_INST_IS_TEMP;
			temp->cil_code = ip;
			NEW_TEMPSTORE (cfg, store, temp->inst_c0, ins);
			store->cil_code = ip;
			if (store->opcode == CEE_STOBJ) {
				EMIT_NEW_TEMPLOADA (cfg, store, temp->inst_c0);
				emit_stobj (cfg, store, sp [0], sp [0]->cil_code, store->klass, FALSE);
			} else {
				MONO_ADD_INS (bblock, store);
			}

			NEW_TEMPLOAD (cfg, ins, temp->inst_c0);
			ins->cil_code = ip;
			if (ins->opcode == CEE_LDOBJ) {
				MonoInst *src;

				EMIT_NEW_TEMPLOADA (cfg, src, temp->inst_c0);
				*sp++ = emit_ldobj (cfg, src, ip, ins->klass);
			}
			else {
				MONO_ADD_INS (bblock, ins);
				*sp++ = ins;
			}

			NEW_TEMPLOAD (cfg, ins, temp->inst_c0);
			ins->cil_code = ip;
			if (ins->opcode == CEE_LDOBJ) {
				MonoInst *src;

				EMIT_NEW_TEMPLOADA (cfg, src, temp->inst_c0);
				*sp++ = emit_ldobj (cfg, src, ip, ins->klass);
			}
			else {
				MONO_ADD_INS (bblock, ins);
				*sp++ = ins;
			}

			++ip;
			inline_costs += 2;
			break;
		}
		case CEE_POP:
			CHECK_STACK (1);
			ip++;
			--sp;

#ifdef __i386__
			if (sp [0]->type == STACK_R8)
				/* we need to pop the value from the x86 FP stack */
				MONO_EMIT_NEW_UNALU (cfg, OP_X86_FPOP, -1, sp [0]->dreg);
#endif
			break;
		case CEE_JMP:
			CHECK_OPSIZE (5);
			if (stack_start != sp)
				goto unverified;
			MONO_INST_NEW (cfg, ins, OP_JMP);
			token = read32 (ip + 1);
			/* FIXME: check the signature matches */
			cmethod = mini_get_method (method, token, NULL, generic_context);

			if (!cmethod)
				goto load_error;

			if (mono_use_security_manager) {
				check_linkdemand (cfg, method, cmethod, ip);
			}

			ins->inst_p0 = cmethod;
			MONO_ADD_INS (bblock, ins);
			ip += 5;
			start_new_bblock = 1;

			/* FIXME: */
			cfg->disable_aot = 1;
			break;
		case CEE_CALLI:
		case CEE_CALL:
		case CEE_CALLVIRT: {
			MonoInst *addr = NULL;
			MonoMethodSignature *fsig = NULL;
			int array_rank = 0;
			int virtual = *ip == CEE_CALLVIRT;
			int calli = *ip == CEE_CALLI;

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
				if (method->wrapper_type != MONO_WRAPPER_NONE) {
					cmethod =  (MonoMethod *)mono_method_get_wrapper_data (method, token);
				} else if (constrained_call) {
					cmethod = mono_get_method_constrained (image, token, constrained_call, generic_context);
					cmethod = mono_get_inflated_method (cmethod);
				} else {
					cmethod = mini_get_method (method, token, NULL, generic_context);
				}

				if (!cmethod)
					goto load_error;

				if (!virtual && (cmethod->flags & METHOD_ATTRIBUTE_ABSTRACT))
					/* MS.NET seems to silently convert this to a callvirt */
					virtual = 1;

				if (!cmethod->klass->inited)
					mono_class_init (cmethod->klass);

				if (mono_method_signature (cmethod)->pinvoke) {
					MonoMethod *wrapper = mono_marshal_get_native_wrapper (cmethod);
					fsig = mono_method_signature (wrapper);
				} else if (constrained_call) {
					fsig = mono_method_signature (cmethod);
				} else {
					fsig = mono_method_get_signature_full (cmethod, image, token, generic_context);
				}

				n = fsig->param_count + fsig->hasthis;

				if (mono_use_security_manager) {
					check_linkdemand (cfg, method, cmethod, ip);
				}

				if (cmethod->iflags & METHOD_IMPL_ATTRIBUTE_INTERNAL_CALL &&
				    mini_class_is_system_array (cmethod->klass)) {
					array_rank = cmethod->klass->rank;
				}

				if (cmethod->string_ctor)
					g_assert_not_reached ();

			}

			if (cmethod && cmethod->klass->generic_container) {
				// G_BREAKPOINT ();
				goto unverified;
			}

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
					sp [0] = handle_box (cfg, sp [0], ip, constrained_call);
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

			if (*ip != CEE_CALLI && check_call_signature (cfg, fsig, sp)) {
				// G_BREAKPOINT ();
				goto unverified;
			}

			if (cmethod && virtual && 
			    (cmethod->flags & METHOD_ATTRIBUTE_VIRTUAL) && 
		 	    !((cmethod->flags & METHOD_ATTRIBUTE_FINAL) && 
			      cmethod->wrapper_type != MONO_WRAPPER_REMOTING_INVOKE_WITH_CHECK) &&
			    mono_method_signature (cmethod)->generic_param_count) {
				MonoInst *this_temp, *store;
				MonoInst *iargs [3];

				g_assert (mono_method_signature (cmethod)->is_inflated);

				this_temp = mono_compile_create_var (cfg, type_from_stack_type (sp [0]), OP_LOCAL);
				NEW_TEMPSTORE (cfg, store, this_temp->inst_c0, sp [0]);
				MONO_ADD_INS (bblock, store);

				EMIT_NEW_TEMPLOAD (cfg, iargs [0], this_temp->inst_c0);
				EMIT_NEW_PCONST (cfg, iargs [1], cmethod);
				EMIT_NEW_PCONST (cfg, iargs [2], ((MonoMethodInflated *) cmethod)->context);
				addr = mono_emit_jit_icall (cfg, helper_compile_generic_method, iargs, ip);
				EMIT_NEW_TEMPLOAD (cfg, sp [0], this_temp->inst_c0);

				ins = (MonoInst*)mono_emit_calli (cfg, fsig, sp, addr, ip);
				if (!MONO_TYPE_IS_VOID (fsig->ret))
					*sp++ = ins;

				ip += 5;
				break;
			}

			/* Tail prefix */
			if ((ins_flag & MONO_INST_TAILCALL) && cmethod && (*ip == CEE_CALL) &&
				 (mono_metadata_signature_equal (mono_method_signature (method), mono_method_signature (cmethod)))) {
				int i;

				for (i = 0; i < n; ++i) {
					/* Prevent argument from being register allocated */
					arg_array [i]->flags |= MONO_INST_VOLATILE;
					NEW_ARGSTORE (cfg, ins, i, sp [i]);
					ins->cil_code = ip;
					if (ins->opcode == CEE_STOBJ) {
						EMIT_NEW_ARGLOADA (cfg, ins, i);
						emit_stobj (cfg, ins, sp [i], sp [i]->cil_code, ins->klass, FALSE);
					}
					else
						MONO_ADD_INS (bblock, ins);
				}

				/* FIXME: */
				cfg->disable_aot = 1;

				MONO_INST_NEW (cfg, ins, OP_JMP);
				ins->cil_code = ip;
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
				ins->cil_code = ip;

				if (!MONO_TYPE_IS_VOID (fsig->ret)) {
					type_to_eval_stack_type (fsig->ret, ins);
					*sp = ins;
					sp++;
				}

				ip += 5;
				break;
			}

			/* Inlining */
			if ((cfg->opt & MONO_OPT_INLINE) && cmethod &&
			    (!virtual || !(cmethod->flags & METHOD_ATTRIBUTE_VIRTUAL) || (cmethod->flags & METHOD_ATTRIBUTE_FINAL)) && 
			    mono_method_check_inlining (cfg, cmethod) &&
				 !g_list_find (dont_inline, cmethod)) {
				int costs;
				gboolean allways = FALSE;

				if ((cmethod->iflags & METHOD_IMPL_ATTRIBUTE_INTERNAL_CALL) ||
					(cmethod->flags & METHOD_ATTRIBUTE_PINVOKE_IMPL)) {
					cmethod = mono_marshal_get_native_wrapper (cmethod);
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
					break;
				}
			}
			
			inline_costs += 10 * num_calls++;

			/* Tail recursion elimination */
			if ((cfg->opt & MONO_OPT_TAILC) && *ip == CEE_CALL && cmethod == method && ip [5] == CEE_RET) {
				gboolean has_vtargs = FALSE;
				int i;

				/* keep it simple */
				for (i =  fsig->param_count - 1; i >= 0; i--) {
					if (MONO_TYPE_ISSTRUCT (mono_method_signature (cmethod)->params [i])) 
						has_vtargs = TRUE;
				}

				if (!has_vtargs) {
					for (i = 0; i < n; ++i) {
						NEW_ARGSTORE (cfg, ins, i, sp [i]);
						ins->cil_code = ip;
						MONO_ADD_INS (bblock, ins);
					}
					MONO_INST_NEW (cfg, ins, OP_BR);
					ins->cil_code = ip;
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

					break;
				}
			}

			if (*ip == CEE_CALLI) {
				ins = (MonoInst*)mono_emit_calli (cfg, fsig, sp, addr, ip);
				if (!MONO_TYPE_IS_VOID (fsig->ret))
					*sp++ = ins;

				ip += 5;
				break;
			}
	      				
			if (array_rank) {
				MonoInst *addr;
				int stack_type;

				if (strcmp (cmethod->name, "Set") == 0) { /* array Set */ 
					if (sp [fsig->param_count]->type == STACK_OBJ) {
						MonoInst *iargs [2];

						iargs [0] = sp [0];
						iargs [1] = sp [fsig->param_count];
						
						mono_emit_jit_icall (cfg, helper_stelem_ref_check, iargs, ip);
					}
					
					addr = mini_emit_ldelema_ins (cfg, cmethod, sp, ip, TRUE);
					NEW_STORE_MEMBASE (cfg, ins, mono_type_to_store_membase (fsig->params [fsig->param_count - 1]), addr->dreg, 0, sp [fsig->param_count]->dreg);
					if (ins->opcode == CEE_STOBJ) {
						emit_stobj (cfg, addr, sp [fsig->param_count], ip, mono_class_from_mono_type (fsig->params [fsig->param_count-1]), FALSE);
					} else {
						MONO_ADD_INS (bblock, ins);
					}

				} else if (strcmp (cmethod->name, "Get") == 0) { /* array Get */
					addr = mini_emit_ldelema_ins (cfg, cmethod, sp, ip, FALSE);

					stack_type = type_to_stack_type (fsig->ret);
					dreg = alloc_dreg (cfg, stack_type);
					NEW_LOAD_MEMBASE (cfg, ins, mono_type_to_load_membase (fsig->ret), dreg, addr->dreg, 0);
					ins->type = stack_type;
					ins->klass = mono_class_from_mono_type (fsig->ret);

					if (ins->opcode == CEE_LDOBJ)
						ins = emit_ldobj (cfg, addr, ip, mono_class_from_mono_type (fsig->ret));
					else
						MONO_ADD_INS (cfg->cbb, ins);

					*sp++ = ins;
				} else if (strcmp (cmethod->name, "Address") == 0) { /* array Address */
					addr = mini_emit_ldelema_ins (cfg, cmethod, sp, ip, FALSE);
					*sp++ = addr;
				} else {
					g_assert_not_reached ();
				}

				ip += 5;
				break;
			}

			/* Common call */
			ins = (MonoInst*)mono_emit_method_call (cfg, cmethod, fsig, sp, ip, virtual ? sp [0] : NULL);

			if (!MONO_TYPE_IS_VOID (fsig->ret))
				*sp++ = ins;

			ip += 5;
			break;
		}
		case CEE_RET:
			if (cfg->method != method) {
				/* return from inlined method */
				if (return_var) {
					MonoInst *store;
					CHECK_STACK (1);
					--sp;
					//g_assert (returnvar != -1);
					NEW_TEMPSTORE (cfg, store, return_var->inst_c0, *sp);
					store->cil_code = sp [0]->cil_code;
					if (store->opcode == CEE_STOBJ) {
						g_assert_not_reached ();
						NEW_TEMPLOADA (cfg, store, return_var->inst_c0);
						emit_stobj (cfg, store, *sp, sp [0]->cil_code, return_var->klass, FALSE);
					} else
						MONO_ADD_INS (bblock, store);
				} 
			} else {
				if (cfg->ret) {
					g_assert (!return_var);
					CHECK_STACK (1);
					--sp;
					MONO_INST_NEW (cfg, ins, OP_NOP);
					ins->opcode = mono_type_to_stind (mono_method_signature (method)->ret);
					if (ins->opcode == CEE_STOBJ) {
						EMIT_NEW_RETLOADA (cfg, ins);
						/* 
						 * cfg->ret is a scalar variable, so can't use its 
						 * class field.
						 */
						emit_stobj (cfg, ins, *sp, ip, mono_class_from_mono_type (mono_method_signature (method)->ret), FALSE);
					} else {
						mono_arch_emit_setret (cfg, method, *sp);
					}
				}
			}
			if (sp != stack_start)
				goto unverified;
			MONO_INST_NEW (cfg, ins, OP_BR);
			ins->cil_code = ip++;
			ins->inst_target_bb = end_bblock;
			MONO_ADD_INS (bblock, ins);
			link_bblock (cfg, bblock, end_bblock);
			start_new_bblock = 1;
			break;
		case CEE_BR_S:
			CHECK_OPSIZE (2);
			MONO_INST_NEW (cfg, ins, OP_BR);
			ins->cil_code = ip++;
			target = ip + 1 + (signed char)(*ip);
			++ip;
			GET_BBLOCK (cfg, bbhash, tblock, target);
			link_bblock (cfg, bblock, tblock);
			CHECK_BBLOCK (target, ip, tblock);
			ins->inst_target_bb = tblock;
			if (sp != stack_start) {
				handle_stack_args (cfg, stack_start, sp - stack_start);
				sp = stack_start;
			}
			MONO_ADD_INS (bblock, ins);
			start_new_bblock = 1;
			inline_costs += 10;
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
			ins->cil_code = ip++;
			target = ip + 1 + *(signed char*)ip;
			ip++;

			ADD_BINCOND (NULL);

			sp = stack_start;
			inline_costs += 10;
			break;
		case CEE_BR:
			CHECK_OPSIZE (5);
			MONO_INST_NEW (cfg, ins, OP_BR);
			ins->cil_code = ip++;

			target = ip + 4 + (gint32)read32(ip);
			ip += 4;
			GET_BBLOCK (cfg, bbhash, tblock, target);
			link_bblock (cfg, bblock, tblock);
			CHECK_BBLOCK (target, ip, tblock);
			ins->inst_target_bb = tblock;
			if (sp != stack_start) {
				handle_stack_args (cfg, stack_start, sp - stack_start);
				sp = stack_start;
			}

			MONO_ADD_INS (bblock, ins);

			start_new_bblock = 1;
			inline_costs += 10;
			break;
		case CEE_BRFALSE_S:
		case CEE_BRTRUE_S:
		case CEE_BRFALSE:
		case CEE_BRTRUE: {
			MonoInst *cmp;
			gboolean is_short = ((*ip) == CEE_BRFALSE_S) || ((*ip) == CEE_BRTRUE_S);
			gboolean is_true = ((*ip) == CEE_BRTRUE_S) || ((*ip) == CEE_BRTRUE);
			guint32 opsize = is_short ? 1 : 4;
			guint8 *orig_ip = ip;

			CHECK_OPSIZE (opsize);
			CHECK_STACK (1);
			ip ++;
			target = ip + opsize + (is_short ? *(signed char*)ip : (gint32)read32(ip));
			ip += opsize;

			sp--;

			GET_BBLOCK (cfg, bbhash, tblock, target);
			link_bblock (cfg, bblock, tblock);
			CHECK_BBLOCK (target, ip, tblock);
			GET_BBLOCK (cfg, bbhash, tblock, ip);
			link_bblock (cfg, bblock, tblock);

			if (sp != stack_start)
				handle_stack_args (cfg, stack_start, sp - stack_start);

			MONO_INST_NEW(cfg, cmp, OP_ICOMPARE_IMM);
			cmp->sreg1 = sp [0]->dreg;
			cmp->cil_code = orig_ip;
			type_from_op (cmp, sp [0], NULL);
			CHECK_TYPE (cmp);

#if SIZEOF_VOID_P == 4
			if (cmp->opcode == OP_LCOMPARE_IMM) {
				/* Convert it to OP_LCOMPARE */
				MONO_INST_NEW (cfg, ins, OP_I8CONST);
				ins->cil_code = ip;
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
			ins->cil_code = orig_ip;
			type_from_op (ins, sp [0], NULL);
			MONO_ADD_INS (bblock, ins);
			ins->inst_many_bb = mono_mempool_alloc (cfg->mempool, sizeof(gpointer)*2);
			GET_BBLOCK (cfg, bbhash, tblock, target);
			ins->inst_true_bb = tblock;
			GET_BBLOCK (cfg, bbhash, tblock, ip);
			ins->inst_false_bb = tblock;
			start_new_bblock = 2;

			sp = stack_start;
			inline_costs += 10;
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
			ins->cil_code = ip++;
			target = ip + 4 + (gint32)read32(ip);
			ip += 4;

			ADD_BINCOND (NULL);

			sp = stack_start;
			inline_costs += 10;
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

			CHECK_OPSIZE (5);
			CHECK_STACK (1);
			n = read32 (ip + 1);
			--sp;
			src1 = sp [0];
			if ((src1->type != STACK_I4) && (src1->type != STACK_PTR)) 
				goto unverified;

			ip += 5;
			CHECK_OPSIZE (n * sizeof (guint32));
			target = ip + n * sizeof (guint32);

			GET_BBLOCK (cfg, bbhash, default_bblock, target);

			targets = mono_mempool_alloc (cfg->mempool, sizeof (MonoBasicBlock*) * n);
			for (i = 0; i < n; ++i) {
				GET_BBLOCK (cfg, bbhash, tblock, target + (gint32)read32(ip));
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
			}

			MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, src1->dreg, n);
			MONO_EMIT_NEW_BRANCH_BLOCK (cfg, OP_PBGE_UN, default_bblock);
			bblock = cfg->cbb;

			for (i = 0; i < n; ++i)
				link_bblock (cfg, bblock, targets [i]);

			if (sizeof (gpointer) == 8)
				MONO_EMIT_NEW_BIALU_IMM (cfg, OP_SHL_IMM, offset_reg, src1->dreg, 3);
			else
				MONO_EMIT_NEW_BIALU_IMM (cfg, OP_SHL_IMM, offset_reg, src1->dreg, 2);

			table = mono_mempool_alloc (cfg->mempool, sizeof (MonoJumpInfoBBTable));
			table->table = targets;
			table->table_size = n;

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
			inline_costs += 20;
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
			ins->cil_code = ip;
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
			ins->cil_code = ip;
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

			decompose_opcode (cfg, ins);
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
			ins->cil_code = ip;
			sp -= 2;
			ins->sreg1 = sp [0]->dreg;
			ins->sreg2 = sp [1]->dreg;
			type_from_op (ins, sp [0], sp [1]);
			CHECK_TYPE (ins);
			ins->dreg = alloc_dreg ((cfg), (ins)->type);

			/* FIXME: Pass opcode to is_inst_imm */

			/* Use the immediate opcodes if possible */
			if (((sp [1]->opcode == OP_ICONST) || (sp [1]->opcode == OP_I8CONST)) && mono_arch_is_inst_imm (sp [1]->opcode == OP_ICONST ? sp [1]->inst_c0 : sp [1]->inst_l)) {
				int imm_opcode = mono_op_to_op_imm (ins->opcode);
				if (imm_opcode != -1) {
					ins->opcode = imm_opcode;
					if (sp [1]->opcode == OP_I8CONST) {
#if SIZEOF_VOID_P == 8
						ins->inst_imm = sp [1]->inst_l;
#else
						ins->inst_ls_word = sp [1]->inst_ls_word;
						ins->inst_ms_word = sp [1]->inst_ms_word;
#endif
					}
					else
						ins->inst_p1 = (gpointer)(gssize)(sp [1]->inst_c0);
					ins->sreg2 = -1;

					sp [1]->opcode = OP_NOP;
				}
			}
			MONO_ADD_INS ((cfg)->cbb, (ins));
			*sp++ = ins;

			decompose_opcode (cfg, ins);
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
#if SIZEOF_VOID_P == 8
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
			if (!klass)
				goto load_error;
			sp -= 2;
			if (MONO_TYPE_IS_REFERENCE (&klass->byval_arg)) {
				MonoInst *store, *load;
				int dreg = alloc_preg (cfg);

				NEW_LOAD_MEMBASE (cfg, load, OP_LOAD_MEMBASE, dreg, sp [1]->dreg, 0);
				load->flags |= ins_flag;
				MONO_ADD_INS (cfg->cbb, load);

				NEW_STORE_MEMBASE (cfg, store, OP_STORE_MEMBASE_REG, sp [0]->dreg, 0, dreg);
				store->flags |= ins_flag;
				MONO_ADD_INS (cfg->cbb, store);
			} else {
				emit_stobj (cfg, sp [0], sp [1], ip, klass, FALSE);
			}
			ins_flag = 0;
			ip += 5;
			break;
		case CEE_LDOBJ: {
			int loc_index = -1;
			int stloc_len = 0;
			int stack_type;
			int dreg;

			CHECK_OPSIZE (5);
			CHECK_STACK (1);
			--sp;
			token = read32 (ip + 1);
			klass = mini_get_class (method, token, generic_context);
			if (!klass)
				goto load_error;

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

				EMIT_NEW_LOCLOADA (cfg, ins, loc_index);
				emit_stobj (cfg, ins, *sp, ip, klass, FALSE);
				ip += 5;
				ip += stloc_len;
				break;
			}

			stack_type = type_to_stack_type (&klass->byval_arg);
			dreg = alloc_dreg (cfg, stack_type);
			NEW_LOAD_MEMBASE (cfg, ins, mono_type_to_load_membase (&klass->byval_arg), dreg, sp[0]->dreg, 0);
			if (ins->opcode == CEE_LDOBJ)
				ins = emit_ldobj (cfg, sp [0], ip, klass);
			else
				MONO_ADD_INS (cfg->cbb, ins);
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
				ins->cil_code = ip;
				ins->type = STACK_OBJ;
				*sp = ins;
			}
			else if (method->wrapper_type != MONO_WRAPPER_NONE) {
				MonoInst *iargs [1];

				EMIT_NEW_PCONST (cfg, iargs [0], mono_method_get_wrapper_data (method, n));				
				*sp = mono_emit_jit_icall (cfg, mono_string_new_wrapper, iargs, ip);
			} else {
				if (cfg->opt & MONO_OPT_SHARED) {
					MonoInst *iargs [3];

					if (cfg->compile_aot) {
						cfg->ldstr_list = g_list_prepend (cfg->ldstr_list, GINT_TO_POINTER (n));
					}
					EMIT_NEW_DOMAINCONST (cfg, iargs [0]);
					EMIT_NEW_IMAGECONST (cfg, iargs [1], image);
					EMIT_NEW_ICONST (cfg, iargs [2], mono_metadata_token_index (n));
					*sp = mono_emit_jit_icall (cfg, mono_ldstr, iargs, ip);
					mono_ldstr (cfg->domain, image, mono_metadata_token_index (n));
				} else {
					if (bblock->out_of_line) {
						MonoInst *iargs [2];

						/* Avoid creating the string object */
						EMIT_NEW_IMAGECONST (cfg, iargs [0], image);
						EMIT_NEW_ICONST (cfg, iargs [1], mono_metadata_token_index (n));
						*sp = mono_emit_jit_icall (cfg, helper_ldstr, iargs, ip);
					} 
					else
					if (cfg->compile_aot) {
						NEW_LDSTRCONST (cfg, ins, image, n);
						*sp = ins;
						MONO_ADD_INS (bblock, ins);
					} 
					else {
						NEW_PCONST (cfg, ins, NULL);
						ins->cil_code = ip;
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
			MonoInst *alloc;
			int temp;
			
			CHECK_OPSIZE (5);
			token = read32 (ip + 1);
			cmethod = mini_get_method (method, token, NULL, generic_context);
			if (!cmethod)
				goto load_error;
			fsig = mono_method_get_signature (cmethod, image, token);

			mono_class_init (cmethod->klass);

			if (mono_use_security_manager) {
				check_linkdemand (cfg, method, cmethod, ip);
			}

			n = fsig->param_count;
			CHECK_STACK (n);

			/* move the args to allow room for 'this' in the first position */
			while (n--) {
				--sp;
				sp [1] = sp [0];
			}

			iargs [0] = NULL;

			if (mini_class_is_system_array (cmethod->klass)) {
				EMIT_NEW_METHODCONST (cfg, *sp, cmethod);
				alloc = handle_array_new (cfg, fsig->param_count, sp, ip);
			} else if (cmethod->string_ctor) {
				/* we simply pass a null pointer */
				EMIT_NEW_PCONST (cfg, *sp, NULL); 
				/* now call the string ctor */
				alloc = mono_emit_method_call (cfg, cmethod, fsig, sp, ip, NULL);
			} else {
				MonoInst* callvirt_this_arg = NULL;
				
				if (cmethod->klass->valuetype) {
					iargs [0] = mono_compile_create_var (cfg, &cmethod->klass->byval_arg, OP_LOCAL);
					temp = iargs [0]->inst_c0;

					EMIT_NEW_TEMPLOADA (cfg, *sp, temp);

					handle_initobj (cfg, *sp, NULL, cmethod->klass, stack_start, sp);

					EMIT_NEW_TEMPLOADA (cfg, *sp, temp);

					alloc = NULL;

					/* 
					 * The code generated by mini_emit_virtual_call () expects
					 * iargs [0] to be a boxed instance, but luckily the vcall
					 * will be transformed into a normal call there.
					 */
				} else {
					alloc = handle_alloc (cfg, cmethod->klass, FALSE, ip);
					*sp = alloc;
				}

				/* Avoid virtual calls to ctors if possible */
				if (cmethod->klass->marshalbyref)
					callvirt_this_arg = sp [0];
				
				if ((cfg->opt & MONO_OPT_INLINE) && cmethod &&
				    mono_method_check_inlining (cfg, cmethod) &&
				    !mono_class_is_subclass_of (cmethod->klass, mono_defaults.exception_class, FALSE) &&
				    !g_list_find (dont_inline, cmethod)) {
					int costs;

					if ((costs = inline_method (cfg, cmethod, fsig, sp, ip, cfg->real_offset, dont_inline, FALSE))) {
						cfg->real_offset += 5;
						bblock = cfg->cbb;

						inline_costs += costs - 5;
					} else {
						mono_emit_method_call (cfg, cmethod, fsig, sp, ip, callvirt_this_arg);
					}
				} else {
					/* now call the actual ctor */
					mono_emit_method_call (cfg, cmethod, fsig, sp, ip, callvirt_this_arg);
				}
			}

			if (alloc == NULL) {
				/* Valuetype */
				EMIT_NEW_TEMPLOADA (cfg, ins, iargs [0]->inst_c0);
				ins->type = STACK_VTYPE;
				ins->klass = ins->klass;
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
			if (!klass)
				goto load_error;

			if (klass->marshalbyref || klass->flags & TYPE_ATTRIBUTE_INTERFACE) {
				
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
				ins = handle_castclass (cfg, klass, *sp, ip);
				bblock = cfg->cbb;
				*sp ++ = ins;
				ip += 5;
			}
			break;
		case CEE_ISINST:
			CHECK_STACK (1);
			--sp;
			CHECK_OPSIZE (5);
			token = read32 (ip + 1);
			klass = mini_get_class (method, token, generic_context);
			if (!klass)
				goto load_error;

			if (klass->marshalbyref || klass->flags & TYPE_ATTRIBUTE_INTERFACE) {
			
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
				ins = handle_isinst (cfg, klass, *sp, ip);
				bblock = cfg->cbb;
				*sp ++ = ins;
				ip += 5;
			}
			break;
		case CEE_UNBOX_ANY: {
			CHECK_STACK (1);
			--sp;
			CHECK_OPSIZE (5);
			token = read32 (ip + 1);
			klass = mini_get_class (method, token, generic_context);
			if (!klass)
				goto load_error;

			if (MONO_TYPE_IS_REFERENCE (&klass->byval_arg)) {
				/* CASTCLASS */
				if (klass->marshalbyref || klass->flags & TYPE_ATTRIBUTE_INTERFACE) {
				
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
					ins = handle_castclass (cfg, klass, *sp, ip);
					bblock = cfg->cbb;
					*sp ++ = ins;
					ip += 5;
				}
				break;
			}			

			if (mono_class_is_nullable (klass)) {
				ins = handle_unbox_nullable (cfg, *sp, ip, klass);
				*sp++= ins;
				ip += 5;
				break;
			}

			/* UNBOX */
			ins = handle_unbox (cfg, klass, sp, ip);
			*sp = ins;

			ip += 5;

			/* LDOBJ */
			{
				MonoInst *dest;
				int stack_type;
				int dreg;

				stack_type = type_to_stack_type (&klass->byval_arg);
				dreg = alloc_dreg (cfg, stack_type);
				NEW_LOAD_MEMBASE (cfg, dest, mono_type_to_load_membase (&klass->byval_arg), dreg, sp[0]->dreg, 0);
				if (dest->opcode == CEE_LDOBJ)
					dest = emit_ldobj (cfg, sp [0], ip, klass);
				else
					MONO_ADD_INS (cfg->cbb, dest);
				*sp++ = dest;
			}

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
			if (!klass)
				goto load_error;

			if (MONO_TYPE_IS_REFERENCE (&klass->byval_arg)) {
				*sp++ = val;
				ip += 5;
				break;
			}

			if (ip + 5 < end && ip_in_bb (cfg, bblock, ip + 5) && (ip [5] == CEE_BRTRUE || ip [5] == CEE_BRTRUE_S)) {
				/*printf ("box-brtrue opt at 0x%04x in %s\n", real_offset, method->name);*/
				ip += 5;
				MONO_INST_NEW (cfg, ins, OP_BR);
				ins->cil_code = ip;
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
				GET_BBLOCK (cfg, bbhash, tblock, target);
				link_bblock (cfg, bblock, tblock);
				CHECK_BBLOCK (target, ip, tblock);
				ins->inst_target_bb = tblock;
				if (sp != stack_start) {
					handle_stack_args (cfg, stack_start, sp - stack_start);
					sp = stack_start;
				}
				MONO_ADD_INS (bblock, ins);
				start_new_bblock = 1;
				break;
			}

			*sp++ = handle_box (cfg, val, ip, klass);
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
			if (!klass)
				goto load_error;

			if (mono_class_is_nullable (klass)) {
				ins = handle_unbox_nullable (cfg, *sp, ip, klass);
				*sp++= ins;
			} else {
				ins = handle_unbox (cfg, klass, sp, ip);
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
			// FIXME: enable this test later.
			//if (sp [0]->type != STACK_OBJ && sp [0]->type != STACK_MP)
			//	goto unverified;
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

			foffset = klass->valuetype? field->offset - sizeof (MonoObject): field->offset;
			/* FIXME: mark instructions for use in SSA */
			if (*ip == CEE_STFLD) {
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
						      
						ip += 5;
						cfg->real_offset += 5;
						bblock = cfg->cbb;

						inline_costs += costs;
						break;
					} else {
						mono_emit_method_call (cfg, stfld_wrapper, mono_method_signature (stfld_wrapper), iargs, ip, NULL);
					}
				} else {
					MonoInst *store;

					NEW_STORE_MEMBASE (cfg, store, mono_type_to_store_membase (field->type), sp [0]->dreg, foffset, sp [1]->dreg);
					store->flags |= ins_flag;
					ins_flag = 0;
					if (store->opcode == CEE_STOBJ) {
						MonoInst *dest;
						int dreg = alloc_preg (cfg);

						EMIT_NEW_BIALU_IMM (cfg, dest, OP_PADD_IMM, dreg, sp [0]->dreg, foffset);
						emit_stobj (cfg, dest, sp [1], ip, 
							      mono_class_from_mono_type (field->type), FALSE);
					} else
						MONO_ADD_INS (bblock, store);
				}
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
						      
					ip += 5;
					cfg->real_offset += 5;

					*sp++ = iargs [0];

					inline_costs += costs;
					break;
				} else {
					ins = mono_emit_method_call (cfg, wrapper, mono_method_signature (wrapper), iargs, ip, NULL);
					*sp++ = ins;
				}
			} else {
				if (*ip == CEE_LDFLDA) {
					dreg = alloc_preg (cfg);

					EMIT_NEW_BIALU_IMM (cfg, ins, OP_PADD_IMM, dreg, sp [0]->dreg, foffset);
					ins->type = STACK_MP;
					*sp++ = ins;
				} else {
					MonoInst *load;

					NEW_LOAD_MEMBASE (cfg, load, mono_type_to_load_membase (field->type), 0, sp [0]->dreg, foffset);
					type_to_eval_stack_type (field->type, load);
					load->dreg = alloc_dreg (cfg, load->type);
					load->cil_code = ip;
					load->flags |= ins_flag;
					ins_flag = 0;

					if (load->opcode == CEE_LDOBJ) {
						MonoInst *src;

						dreg = alloc_preg (cfg);
						EMIT_NEW_BIALU_IMM (cfg, src, OP_PADD_IMM, dreg, sp [0]->dreg, foffset);
						load = emit_ldobj (cfg, src, ip, mono_class_from_mono_type (field->type));
					}
					else
						MONO_ADD_INS (bblock, load);
					*sp++ = load;
				}
			}
			ip += 5;
			break;
		}
		case CEE_LDSFLD:
		case CEE_LDSFLDA:
		case CEE_STSFLD: {
			MonoClassField *field;
			gpointer addr = NULL;

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

			g_assert (!(field->type->attrs & FIELD_ATTRIBUTE_LITERAL));

			/* The special_static_fields field is init'd in mono_class_vtable, so it needs
			 * to be called here.
			 */
			if (!(cfg->opt & MONO_OPT_SHARED))
				mono_class_vtable (cfg->domain, klass);
			mono_domain_lock (cfg->domain);
			if (cfg->domain->special_static_fields)
				addr = g_hash_table_lookup (cfg->domain->special_static_fields, field);
			mono_domain_unlock (cfg->domain);

			if ((cfg->opt & MONO_OPT_SHARED) || (cfg->compile_aot && addr)) {
				MonoInst *iargs [2];

				g_assert (field->parent);
				EMIT_NEW_DOMAINCONST (cfg, iargs [0]);
				EMIT_NEW_FIELDCONST (cfg, iargs [1], field);
				ins = mono_emit_jit_icall (cfg, mono_class_static_field_address, iargs, ip);
			} else {
				MonoVTable *vtable;
				vtable = mono_class_vtable (cfg->domain, klass);
				if (!addr) {
					if (mini_field_access_needs_cctor_run (cfg, method, vtable) && !(g_slist_find (class_inits, vtable))) {
						guint8 *tramp = mono_create_class_init_trampoline (vtable);
						mono_emit_native_call (cfg, tramp, 
											   helper_sig_class_init_trampoline,
											   NULL, ip);
						if (cfg->verbose_level > 2)
							printf ("class %s.%s needs init call for %s\n", klass->name_space, klass->name, field->name);
						class_inits = g_slist_prepend (class_inits, vtable);
					} else {
						if (cfg->run_cctors)
							mono_runtime_class_init (vtable);
					}
					addr = (char*)vtable->data + field->offset;

					if (cfg->compile_aot)
						EMIT_NEW_SFLDACONST (cfg, ins, field);
					else
						EMIT_NEW_PCONST (cfg, ins, addr);
					ins->cil_code = ip;
				} else {
					/* 
					 * insert call to mono_threads_get_static_data (GPOINTER_TO_UINT (addr)) 
					 * This could be later optimized to do just a couple of
					 * memory dereferences with constant offsets.
					 */
					MonoInst *iargs [1];
					EMIT_NEW_ICONST (cfg, iargs [0], GPOINTER_TO_UINT (addr));
					ins = mono_emit_jit_icall (cfg, mono_get_special_static_data, iargs, ip);
				}
			}

			/* FIXME: mark instructions for use in SSA */
			if (*ip == CEE_LDSFLDA) {
				*sp++ = ins;
			} else if (*ip == CEE_STSFLD) {
				MonoInst *store;
				CHECK_STACK (1);
				sp--;

				NEW_STORE_MEMBASE (cfg, store, mono_type_to_store_membase (field->type), ins->dreg, 0, sp [0]->dreg);
				store->flags |= ins_flag;
				ins_flag = 0;

				if (store->opcode == CEE_STOBJ) {
					emit_stobj (cfg, ins, sp [0], ip, mono_class_from_mono_type (field->type), FALSE);
				} else
					MONO_ADD_INS (bblock, store);
			} else {
				gboolean is_const = FALSE;
				MonoVTable *vtable = mono_class_vtable (cfg->domain, klass);
				if (!((cfg->opt & MONO_OPT_SHARED) || cfg->compile_aot) && 
				    vtable->initialized && (field->type->attrs & FIELD_ATTRIBUTE_INIT_ONLY)) {
					gpointer addr = (char*)vtable->data + field->offset;
					int ro_type = field->type->type;
					if (ro_type == MONO_TYPE_VALUETYPE && field->type->data.klass->enumtype) {
						ro_type = field->type->data.klass->enum_basetype->type;
					}
					/* printf ("RO-FIELD %s.%s:%s\n", klass->name_space, klass->name, field->name);*/
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
						type_to_eval_stack_type (field->type, *sp);
						sp++;
						break;
					case MONO_TYPE_I8:
					case MONO_TYPE_U8:
						MONO_INST_NEW (cfg, *sp, OP_I8CONST);
						sp [0]->type = STACK_I8;
						sp [0]->inst_l = *((gint64 *)addr);
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
					int dreg;

					CHECK_STACK_OVF (1);

					dreg = alloc_dreg (cfg, type_to_stack_type (field->type));
					NEW_LOAD_MEMBASE (cfg, load, mono_type_to_load_membase (field->type), dreg, ins->dreg, 0);
					if (load->opcode == CEE_LDOBJ) {
						load = emit_ldobj (cfg, ins, ip, mono_class_from_mono_type (field->type));
					} else {
						MONO_ADD_INS (cfg->cbb, load);
						load->type = type_to_stack_type (field->type);
					}
					load->flags |= ins_flag;
					ins_flag = 0;
					*sp++ = load;

					/* fixme: dont see the problem why this does not work */
					//cfg->disable_aot = TRUE;
				}
			}
			ip += 5;
			break;
		}
		case CEE_STOBJ:
			CHECK_STACK (2);
			sp -= 2;
			CHECK_OPSIZE (5);
			token = read32 (ip + 1);
			klass = mini_get_class (method, token, generic_context);
			if (!klass)
				goto load_error;
			/* FIXME: should check item at sp [1] is compatible with the type of the store. */
			NEW_STORE_MEMBASE (cfg, ins, mono_type_to_store_membase (&klass->byval_arg), sp [0]->dreg, 0, sp [1]->dreg);
			if (ins->opcode == CEE_STOBJ)
				emit_stobj (cfg, sp [0], sp [1], ip, klass, FALSE);
			else
				MONO_ADD_INS (cfg->cbb, ins);
			ins_flag = 0;
			ip += 5;
			inline_costs += 1;
			break;

			/*
			 * Array opcodes
			 */
		case CEE_NEWARR: {
			MonoInst *iargs [3];

			CHECK_STACK (1);
			--sp;

			CHECK_OPSIZE (5);
			token = read32 (ip + 1);

			klass = mini_get_class (method, token, generic_context);
			if (!klass)
				goto load_error;

			if (cfg->opt & MONO_OPT_SHARED) {
				EMIT_NEW_DOMAINCONST (cfg, iargs [0]);
				EMIT_NEW_CLASSCONST (cfg, iargs [1], klass);
				iargs [2] = sp [0];

				*sp++ = mono_emit_jit_icall (cfg, mono_array_new, iargs, ip);
			} else {
				MonoVTable *vtable = mono_class_vtable (cfg->domain, mono_array_class_get (klass, 1));

				NEW_VTABLECONST (cfg, iargs [0], vtable);
				MONO_ADD_INS (cfg->cbb, iargs [0]);
				iargs [1] = sp [0];

				*sp++ = mono_emit_jit_icall (cfg, mono_array_new_specific, iargs, ip);
			}

			ip += 5;
			inline_costs += 1;
			break;
		}
		case CEE_LDLEN: {
			int dreg;

			CHECK_STACK (1);
			--sp;

			dreg = alloc_preg (cfg);
			NEW_LOAD_MEMBASE (cfg, ins, OP_LOADI4_MEMBASE, dreg, sp [0]->dreg,
							  G_STRUCT_OFFSET (MonoArray, max_length));
			MONO_ADD_INS (cfg->cbb, ins);
			ip ++;
			*sp++ = ins;
			break;
		}
		case CEE_LDELEMA:
			CHECK_STACK (2);
			sp -= 2;
			CHECK_OPSIZE (5);

			klass = mini_get_class (method, read32 (ip + 1), generic_context);
			if (!klass)
				goto load_error;			
			/* we need to make sure that this array is exactly the type it needs
			 * to be for correctness. the wrappers are lax with their usage
			 * so we need to ignore them here
			 */
			if (!klass->valuetype && method->wrapper_type == MONO_WRAPPER_NONE) {
				MonoClass* array_class = mono_array_class_get (klass, 1);
				int vtable_reg = alloc_preg (cfg);

				MONO_EMIT_NEW_LOAD_MEMBASE_OP (cfg, OP_LOAD_MEMBASE, vtable_reg, 
											   sp [0]->dreg, G_STRUCT_OFFSET (MonoObject, vtable));
				       
				if (cfg->opt & MONO_OPT_SHARED) {
					int class_reg = alloc_preg (cfg);
					MONO_EMIT_NEW_LOAD_MEMBASE_OP (cfg, OP_LOAD_MEMBASE, class_reg, 
												   vtable_reg, G_STRUCT_OFFSET (MonoVTable, klass));
					if (cfg->compile_aot) {
						int klass_reg = alloc_preg (cfg);
						MONO_EMIT_NEW_CLASSCONST (cfg, klass_reg, array_class);
						MONO_EMIT_NEW_BIALU (cfg, OP_COMPARE, -1, class_reg, klass_reg);
					} else {
						MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, class_reg, array_class);
					}
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
			guint32 size, stack_type;
			int mult_reg, add_reg, array_reg, index_reg, dreg;

			/* FIXME: Add back the LDELEMA(reg,OP_ICONST) optimization */
			/* FIXME: Add arch specific optimizations */

			CHECK_STACK (2);
			sp -= 2;

			if (*ip == CEE_LDELEM_ANY) {
				CHECK_OPSIZE (5);
				token = read32 (ip + 1);
				klass = mono_class_get_full (image, token, generic_context);
				if (!klass)
					goto load_error;
				mono_class_init (klass);
			}
			else
				klass = array_access_to_klass (*ip);

			size = mono_class_array_element_size (klass);

			mult_reg = alloc_preg (cfg);
			add_reg = alloc_preg (cfg);
			array_reg = sp [0]->dreg;
			index_reg = sp [1]->dreg;

			MONO_EMIT_BOUNDS_CHECK (cfg, array_reg, MonoArray, max_length, index_reg);
			MONO_EMIT_NEW_BIALU_IMM (cfg, OP_MUL_IMM, mult_reg, index_reg, size);
			MONO_EMIT_NEW_BIALU (cfg, OP_PADD, add_reg, array_reg, mult_reg);

			stack_type = type_to_stack_type (&klass->byval_arg);
			dreg = alloc_dreg (cfg, stack_type);
			NEW_LOAD_MEMBASE (cfg, ins, mono_type_to_load_membase (&klass->byval_arg), dreg, add_reg, G_STRUCT_OFFSET (MonoArray, vector));
			ins->type = stack_type;
			if (ins->opcode == CEE_LDOBJ) {
				int addr_reg = alloc_preg (cfg);
				
				EMIT_NEW_BIALU_IMM (cfg, ins, OP_PADD_IMM, addr_reg, add_reg, G_STRUCT_OFFSET (MonoArray, vector));
				ins = emit_ldobj (cfg, ins, ip, klass);
			}
			else
				MONO_ADD_INS (cfg->cbb, ins);

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
			guint32 size;
			int mult_reg, add_reg, array_reg, index_reg, val_reg;

			CHECK_STACK (3);
			sp -= 3;

			if (*ip == CEE_STELEM_ANY) {
				CHECK_OPSIZE (5);
				token = read32 (ip + 1);
				klass = mono_class_get_full (image, token, generic_context);
				if (!klass)
					goto load_error;
				mono_class_init (klass);
			}
			else
				klass = array_access_to_klass (*ip);

			if (MONO_TYPE_IS_REFERENCE (&klass->byval_arg)) {
				MonoMethod* helper = mono_marshal_get_stelemref ();
				MonoInst *iargs [3];

				iargs [2] = sp [2];
				iargs [1] = sp [1];
				iargs [0] = sp [0];
				
				mono_emit_method_call (cfg, helper, mono_method_signature (helper), iargs, ip, NULL);
			} else {
				size = mono_class_array_element_size (klass);

				/* FIXME: Add back the LDELEMA(reg,OP_ICONST) optimization */
				/* FIXME: Add arch specific optimizations */

				mult_reg = alloc_preg (cfg);
				add_reg = alloc_preg (cfg);
				array_reg = sp [0]->dreg;
				index_reg = sp [1]->dreg;
				val_reg = sp [2]->dreg;

				MONO_EMIT_BOUNDS_CHECK (cfg, array_reg, MonoArray, max_length, index_reg);
				MONO_EMIT_NEW_BIALU_IMM (cfg, OP_MUL_IMM, mult_reg, index_reg, size);
				MONO_EMIT_NEW_BIALU (cfg, OP_PADD, add_reg, array_reg, mult_reg);

				NEW_STORE_MEMBASE (cfg, ins, mono_type_to_store_membase (&klass->byval_arg), add_reg, G_STRUCT_OFFSET (MonoArray, vector), val_reg);
				if (ins->opcode == CEE_STOBJ) {
					int addr_reg = alloc_preg (cfg);
				
					EMIT_NEW_BIALU_IMM (cfg, ins, OP_PADD_IMM, addr_reg, add_reg, G_STRUCT_OFFSET (MonoArray, vector));
					emit_stobj (cfg, ins, sp [2], ip, klass, FALSE);
				}
				else
					MONO_ADD_INS (cfg->cbb, ins);
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
			ins->cil_code = ip;
			ins->sreg1 = sp [0]->dreg;
			ins->dreg = alloc_freg (cfg);
			ins->type = STACK_R8;
			MONO_ADD_INS (bblock, ins);
			*sp++ = ins;

			++ip;
			break;
		}
		case CEE_REFANYVAL: {
			int klass_reg = alloc_preg (cfg);
			int dreg = alloc_preg (cfg);

			CHECK_STACK (1);
			MONO_INST_NEW (cfg, ins, *ip);
			--sp;
			CHECK_OPSIZE (5);
			klass = mono_class_get_full (image, read32 (ip + 1), generic_context);
			if (!klass)
				goto load_error;
			mono_class_init (klass);

			MONO_EMIT_NEW_LOAD_MEMBASE (cfg, klass_reg, sp [0]->dreg, G_STRUCT_OFFSET (MonoTypedRef, klass));
			if (cfg->compile_aot) {
				int const_reg = alloc_preg (cfg);
				MONO_EMIT_NEW_CLASSCONST (cfg, const_reg, klass);
				MONO_EMIT_NEW_BIALU (cfg, OP_COMPARE, -1, klass_reg, const_reg);
			} else {
				MONO_EMIT_NEW_BIALU_IMM (cfg, OP_COMPARE_IMM, -1, klass_reg, klass);
			}
			MONO_EMIT_NEW_COND_EXC (cfg, NE_UN, "InvalidCastException");
			EMIT_NEW_LOAD_MEMBASE (cfg, ins, OP_LOAD_MEMBASE, dreg, sp [0]->dreg, G_STRUCT_OFFSET (MonoTypedRef, value));
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
			if (!klass)
				goto load_error;
			mono_class_init (klass);
			ins->cil_code = ip;

			loc = mono_compile_create_var (cfg, &mono_defaults.typed_reference_class->byval_arg, OP_LOCAL);
			EMIT_NEW_TEMPLOADA (cfg, addr, loc->inst_c0);

			if (cfg->compile_aot) {
				int const_reg = alloc_preg (cfg);
				int type_reg = alloc_preg (cfg);

				MONO_EMIT_NEW_CLASSCONST (cfg, const_reg, klass);
				MONO_EMIT_NEW_STORE_MEMBASE (cfg, OP_STOREP_MEMBASE_REG, addr->dreg, G_STRUCT_OFFSET (MonoTypedRef, klass), const_reg);
				MONO_EMIT_NEW_BIALU_IMM (cfg, OP_ADD_IMM, type_reg, const_reg, G_STRUCT_OFFSET (MonoClass, byval_arg));
				MONO_EMIT_NEW_STORE_MEMBASE (cfg, OP_STOREP_MEMBASE_REG, addr->dreg, G_STRUCT_OFFSET (MonoTypedRef, type), type_reg);
			}
			else {
				MONO_EMIT_NEW_STORE_MEMBASE_IMM (cfg, OP_STOREP_MEMBASE_IMM, addr->dreg, G_STRUCT_OFFSET (MonoTypedRef, type), &klass->byval_arg);
				MONO_EMIT_NEW_STORE_MEMBASE_IMM (cfg, OP_STOREP_MEMBASE_IMM, addr->dreg, G_STRUCT_OFFSET (MonoTypedRef, klass), klass);
			}
			MONO_EMIT_NEW_STORE_MEMBASE (cfg, OP_STOREP_MEMBASE_REG, addr->dreg, G_STRUCT_OFFSET (MonoTypedRef, value), sp [0]->dreg);
			addr->type = STACK_MP;
			*sp++ = addr;
			ip += 5;
			break;
		}
		case CEE_LDTOKEN: {
			gpointer handle;
			MonoClass *handle_class;

			CHECK_STACK_OVF (1);

			CHECK_OPSIZE (5);
			n = read32 (ip + 1);

			if (method->wrapper_type == MONO_WRAPPER_DYNAMIC_METHOD) {
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

			if (cfg->opt & MONO_OPT_SHARED) {
				MonoInst *addr, *vtvar, *iargs [3];

				vtvar = mono_compile_create_var (cfg, &handle_class->byval_arg, OP_LOCAL); 

				EMIT_NEW_IMAGECONST (cfg, iargs [0], image);
				EMIT_NEW_ICONST (cfg, iargs [1], n);
				EMIT_NEW_PCONST (cfg, iargs [2], generic_context);
				ins = mono_emit_jit_icall (cfg, mono_ldtoken_wrapper, iargs, ip);
				EMIT_NEW_TEMPLOADA (cfg, addr, vtvar->inst_c0);

				MONO_EMIT_NEW_STORE_MEMBASE (cfg, OP_STORE_MEMBASE_REG, addr->dreg, 0, ins->dreg);

				EMIT_NEW_TEMPLOADA (cfg, ins, vtvar->inst_c0);
				ins->type = STACK_VTYPE;
				ins->klass = handle_class;
			} else {
				if ((ip [5] == CEE_CALL) && (cmethod = mini_get_method (method, read32 (ip + 6), NULL, generic_context)) &&
						(cmethod->klass == mono_defaults.monotype_class->parent) &&
						(strcmp (cmethod->name, "GetTypeFromHandle") == 0) && ip_in_bb (cfg, bblock, ip + 5)) {
					MonoClass *tclass = mono_class_from_mono_type (handle);
					mono_class_init (tclass);
					if (cfg->compile_aot)
						EMIT_NEW_TYPE_FROM_HANDLE_CONST (cfg, ins, image, n);
					else
						EMIT_NEW_PCONST (cfg, ins, mono_type_get_object (cfg->domain, handle));
					ins->type = STACK_OBJ;
					ins->klass = cmethod->klass;
					ip += 5;
				} else {
					MonoInst *addr, *vtvar;

					vtvar = mono_compile_create_var (cfg, &handle_class->byval_arg, OP_LOCAL);
					if (cfg->compile_aot)
						EMIT_NEW_LDTOKENCONST (cfg, ins, image, n);
					else
						EMIT_NEW_PCONST (cfg, ins, handle);
					EMIT_NEW_TEMPLOADA (cfg, addr, vtvar->inst_c0);
					MONO_EMIT_NEW_STORE_MEMBASE (cfg, OP_STORE_MEMBASE_REG, addr->dreg, 0, ins->dreg);
					EMIT_NEW_TEMPLOADA (cfg, ins, vtvar->inst_c0);
					ins->type = STACK_VTYPE;
					ins->klass = handle_class;
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
			ins->cil_code = ip++;
			bblock->out_of_line = TRUE;
			MONO_ADD_INS (bblock, ins);
			MONO_INST_NEW (cfg, ins, OP_NOT_REACHED);
			ins->cil_code = ip - 1;
			MONO_ADD_INS (bblock, ins);
			sp = stack_start;
			
			link_bblock (cfg, bblock, end_bblock);
			start_new_bblock = 1;

			/* Needed by the backend */
			mono_get_got_var (cfg);
			break;
		case CEE_ENDFINALLY:
			MONO_INST_NEW (cfg, ins, OP_ENDFINALLY);
			MONO_ADD_INS (bblock, ins);
			ins->cil_code = ip++;
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

				if (MONO_OFFSET_IN_HANDLER (clause, ip - header->code) && (clause->flags == MONO_EXCEPTION_CLAUSE_NONE) && (ip - header->code + ((*ip == CEE_LEAVE) ? 5 : 2)) == (clause->handler_offset + clause->handler_len)) {
					MonoInst *exc_ins;
					MonoBasicBlock *dont_throw;

					/*
					  MonoInst *load;

					  NEW_TEMPLOAD (cfg, load, mono_find_exvar_for_offset (cfg, clause->handler_offset)->inst_c0);
					*/

					exc_ins = mono_emit_jit_icall (cfg, mono_thread_get_pending_exception, NULL, ip);

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

			/* fixme: call fault handler ? */

			if ((handlers = mono_find_final_block (cfg, ip, target, MONO_EXCEPTION_CLAUSE_FINALLY))) {
				GList *tmp;
				for (tmp = handlers; tmp; tmp = tmp->next) {
					tblock = tmp->data;
					link_bblock (cfg, bblock, tblock);
					MONO_INST_NEW (cfg, ins, OP_CALL_HANDLER);
					ins->cil_code = ip;
					ins->inst_target_bb = tblock;
					MONO_ADD_INS (bblock, ins);
				}
				g_list_free (handlers);
			} 

			MONO_INST_NEW (cfg, ins, OP_BR);
			ins->cil_code = ip;
			MONO_ADD_INS (bblock, ins);
			GET_BBLOCK (cfg, bbhash, tblock, target);
			link_bblock (cfg, bblock, tblock);
			CHECK_BBLOCK (target, ip, tblock);
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

				ins = mono_emit_jit_icall (cfg, info->func, sp, ip);
				if (!MONO_TYPE_IS_VOID (info->sig->ret))
					*sp++ = ins;

				ip += 6;
				inline_costs += 10 * num_calls++;

				break;
			}
			case CEE_MONO_LDPTR:
				CHECK_STACK_OVF (1);
				CHECK_OPSIZE (6);
				token = read32 (ip + 2);
				EMIT_NEW_PCONST (cfg, ins, mono_method_get_wrapper_data (method, token));
				ins->cil_code = ip;
				*sp++ = ins;
				ip += 6;
				inline_costs += 10 * num_calls++;
				/* Can't embed random pointers into AOT code */
				cfg->disable_aot = 1;
				break;
			case CEE_MONO_VTADDR: {
				int dreg = alloc_preg (cfg);

				CHECK_STACK (1);
				--sp;
				EMIT_NEW_UNALU (cfg, ins, OP_MOVE, dreg, sp [0]->dreg);
				ins->type = STACK_MP;
				*sp++ = ins;
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
				*sp++ = mono_emit_jit_icall (cfg, mono_object_new, iargs, ip);
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
				ins->cil_code = ip;
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

				/* 
				 * In theory, we should load the value to the stack, but that
				 * would involve creating a variable whose type is the native
				 * vtype, and this is a bit hard to do. So instead we 
				 * simulate the load by loading the original address.
				 */
				ins = sp [0];
				ins->type = STACK_VTYPE;
				ins->klass = klass;
				*sp++ = ins;
				ip += 6;
				break;
			case CEE_MONO_RETOBJ:
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

				EMIT_NEW_RETLOADA (cfg, ins);
				emit_stobj (cfg, ins, *sp, ip, klass, TRUE);
				
				if (sp != stack_start)
					goto unverified;
				
				MONO_INST_NEW (cfg, ins, OP_BR);
				ins->cil_code = ip;
				ins->inst_target_bb = end_bblock;
				MONO_ADD_INS (bblock, ins);
				link_bblock (cfg, bblock, end_bblock);
				start_new_bblock = 1;
				ip += 6;
				break;
			case CEE_MONO_CISINST:
			case CEE_MONO_CCASTCLASS: {
				int token;
				CHECK_STACK (1);
				--sp;
				CHECK_OPSIZE (6);
				token = read32 (ip + 2);
				klass = (MonoClass *)mono_method_get_wrapper_data (method, token);
				if (ip [1] == CEE_MONO_CISINST)
					ins = handle_cisinst (cfg, klass, sp [0], ip);
				else
					ins = handle_ccastclass (cfg, klass, sp [0], ip);
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
				ins->cil_code = ip;
				*sp++ = ins;
				ip += 6;
				inline_costs += 10 * num_calls++;
				break;
			case CEE_MONO_NOT_TAKEN:
				bblock->out_of_line = TRUE;
				ip += 2;
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
				addr->type = STACK_VTYPE;
				addr->klass = mono_defaults.argumenthandle_class;
				*sp++ = addr;
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
				MONO_INST_NEW (cfg, cmp, 256 + ip [1]);
				
				MONO_INST_NEW (cfg, ins, cmp->opcode);
				sp -= 2;
				cmp->sreg1 = sp [0]->dreg;
				cmp->sreg2 = sp [1]->dreg;
				cmp->cil_code = ip;
				type_from_op (cmp, sp [0], sp [1]);
				CHECK_TYPE (cmp);
				if ((sp [0]->type == STACK_I8) || ((sizeof (gpointer) == 8) && ((sp [0]->type == STACK_PTR) || (sp [0]->type == STACK_OBJ) || (sp [0]->type == STACK_MP))))
					cmp->opcode = OP_LCOMPARE;
				else if (sp [0]->type == STACK_R8)
					cmp->opcode = OP_FCOMPARE;
				else
					cmp->opcode = OP_ICOMPARE;
				MONO_ADD_INS (bblock, cmp);
				ins->cil_code = ip;
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

				CHECK_STACK_OVF (1);
				CHECK_OPSIZE (6);
				n = read32 (ip + 2);
				cmethod = mini_get_method (method, n, NULL, generic_context);
				if (!cmethod)
					goto load_error;
				mono_class_init (cmethod->klass);

				if (mono_use_security_manager) {
					check_linkdemand (cfg, method, cmethod, ip);
				}

				EMIT_NEW_METHODCONST (cfg, argconst, cmethod);
				if (method->wrapper_type != MONO_WRAPPER_SYNCHRONIZED)
					ins = mono_emit_jit_icall (cfg, mono_ldftn, &argconst, ip);
				else
					ins = mono_emit_jit_icall (cfg, mono_ldftn_nosync, &argconst, ip);
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
				cmethod = mini_get_method (method, n, NULL, generic_context);
				if (!cmethod)
					goto load_error;
				mono_class_init (cmethod->klass);

				if (mono_use_security_manager) {
					check_linkdemand (cfg, method, cmethod, ip);
				}

				--sp;
				args [0] = *sp;
				EMIT_NEW_METHODCONST (cfg, args [1], cmethod);
				*sp++ = mono_emit_jit_icall (cfg, mono_ldvirtfn, args, ip);

				ip += 6;
				inline_costs += 10 * num_calls++;
				break;
			}
			case CEE_LDARG:
				CHECK_STACK_OVF (1);
				CHECK_OPSIZE (4);
				n = read16 (ip + 2);
				CHECK_ARG (n);
				NEW_ARGLOAD (cfg, ins, n);
				ins->cil_code = ip;
				if (ins->opcode == CEE_LDOBJ) {
					MonoInst *src;

					EMIT_NEW_ARGLOADA (cfg, src, n);
					ins = emit_ldobj (cfg, src, ip, ins->klass);
				}
				else
					MONO_ADD_INS (cfg->cbb, ins);
				*sp++ = ins;
				ip += 4;
				break;
			case CEE_LDARGA:
				CHECK_STACK_OVF (1);
				CHECK_OPSIZE (4);
				n = read16 (ip + 2);
				CHECK_ARG (n);
				NEW_ARGLOADA (cfg, ins, n);
				ins->cil_code = ip;
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
				NEW_ARGSTORE (cfg, ins, n, *sp);
				ins->cil_code = ip;
				if (ins->opcode == CEE_STOBJ) {
					EMIT_NEW_ARGLOADA (cfg, ins, n);
					emit_stobj (cfg, ins, *sp, ip, ins->klass, FALSE);
				} else
					MONO_ADD_INS (bblock, ins);
				ip += 4;
				break;
			case CEE_LDLOC:
				CHECK_STACK_OVF (1);
				CHECK_OPSIZE (4);
				n = read16 (ip + 2);
				CHECK_LOCAL (n);
				NEW_LOCLOAD (cfg, ins, n);
				if (ins->opcode == CEE_LDOBJ) {
					MonoInst *src;

					EMIT_NEW_LOCLOADA (cfg, src, n);

					*sp++ = emit_ldobj (cfg, src, ip, ins->klass);
				} else {
					MONO_ADD_INS (bblock, ins);
					*sp++ = ins;
				}
				ip += 4;
				break;
			case CEE_LDLOCA:
				CHECK_STACK_OVF (1);
				CHECK_OPSIZE (4);
				n = read16 (ip + 2);
				CHECK_LOCAL (n);
				EMIT_NEW_LOCLOADA (cfg, ins, n);
				ins->cil_code = ip;
				*sp++ = ins;
				ip += 4;
				break;
			case CEE_STLOC:
				CHECK_STACK (1);
				--sp;
				CHECK_OPSIZE (4);
				n = read16 (ip + 2);
				CHECK_LOCAL (n);
				NEW_LOCSTORE (cfg, ins, n, *sp);
				ins->cil_code = ip;
				if (ins->opcode == CEE_STOBJ) {
					EMIT_NEW_LOCLOADA (cfg, ins, n);
					emit_stobj (cfg, ins, *sp, ip, ins->klass, FALSE);
				} else
					MONO_ADD_INS (bblock, ins);
				ip += 4;
				inline_costs += 1;
				break;
			case CEE_LOCALLOC:
				CHECK_STACK (1);
				--sp;
				if (sp != stack_start) 
					goto unverified;
				if (cfg->method != method) 
					/* 
					 * Inlining this into a loop in a parent could lead to 
					 * stack overflows which is different behavior than the
					 * non-inlined case, thus disable inlining in this case.
					 */
					goto inline_failure;

				if (sp [0]->opcode == OP_ICONST) {
					MONO_INST_NEW (cfg, ins, OP_LOCALLOC_IMM);
					ins->dreg = alloc_preg (cfg);
					ins->inst_imm = sp [0]->inst_c0;
				} else {
					MONO_INST_NEW (cfg, ins, OP_LOCALLOC);
					ins->dreg = alloc_preg (cfg);
					ins->sreg1 = sp [0]->dreg;
				}
				ins->cil_code = ip;
				ins->type = STACK_MP;
				MONO_ADD_INS (cfg->cbb, ins);

				cfg->flags |= MONO_CFG_HAS_ALLOCA;
				if (header->init_locals)
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
					goto unverified;
				MONO_INST_NEW (cfg, ins, OP_ENDFILTER);
				ins->sreg1 = (*sp)->dreg;
				ins->cil_code = ip;
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
					goto unverified;

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
				if (!klass)
					goto load_error;
				if (MONO_TYPE_IS_REFERENCE (&klass->byval_arg)) {
					MONO_EMIT_NEW_STORE_MEMBASE_IMM (cfg, OP_STORE_MEMBASE_IMM, sp [0]->dreg, 0, 0);
				} else {
					handle_initobj (cfg, *sp, NULL, klass, stack_start, sp);
				}
				ip += 6;
				inline_costs += 1;
				break;
			case CEE_CONSTRAINED_:
				CHECK_OPSIZE (6);
				token = read32 (ip + 2);
				constrained_call = mono_class_get_full (image, token, generic_context);
				if (!constrained_call)
					goto load_error;
				ip += 6;
				break;
			case CEE_CPBLK:
			case CEE_INITBLK: {
				MonoInst *iargs [3];
				CHECK_STACK (3);
				sp -= 3;

				if ((ip [1] == CEE_CPBLK) && (cfg->opt & MONO_OPT_INTRINS) && (sp [2]->opcode == OP_ICONST) && ((n = sp [2]->inst_c0) <= sizeof (gpointer) * 5)) {
					mini_emit_memcpy2 (cfg, sp [0]->dreg, 0, sp [1]->dreg, 0, sp [2]->inst_c0, 0);
				} else if ((ip [1] == CEE_INITBLK) && (cfg->opt & MONO_OPT_INTRINS) && (sp [2]->opcode == OP_ICONST) && ((n = sp [2]->inst_c0) <= sizeof (gpointer) * 5) && (sp [1]->opcode == OP_ICONST) && (sp [1]->inst_c0 == 0)) {
					/* emit_memset only works when val == 0 */
					mini_emit_memset (cfg, sp [0]->dreg, 0, sp [2]->inst_c0, sp [1]->inst_c0, 0);
				} else {
					iargs [0] = sp [0];
					iargs [1] = sp [1];
					iargs [2] = sp [2];
					if (ip [1] == CEE_CPBLK) {
						MonoMethod *memcpy_method = get_memcpy_method ();
						mono_emit_method_call (cfg, memcpy_method, memcpy_method->signature, iargs, ip, NULL);
					} else {
						MonoMethod *memset_method = get_memset_method ();
						mono_emit_method_call (cfg, memset_method, memset_method->signature, iargs, ip, NULL);
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
					if (MONO_OFFSET_IN_HANDLER (clause, ip - header->code) && !(clause->flags & MONO_EXCEPTION_CLAUSE_FINALLY))
						handler_offset = clause->handler_offset;
				}

				bblock->flags |= BB_EXCEPTION_UNSAFE;

				g_assert (handler_offset != -1);

				EMIT_NEW_TEMPLOAD (cfg, load, mono_find_exvar_for_offset (cfg, handler_offset)->inst_c0);
				load->cil_code = ip;
				MONO_INST_NEW (cfg, ins, OP_RETHROW);
				ins->sreg1 = load->dreg;
				ins->cil_code = ip;
				MONO_ADD_INS (bblock, ins);
				sp = stack_start;
				link_bblock (cfg, bblock, end_bblock);
				start_new_bblock = 1;
				ip += 2;

				/* Needed by the backend */
				mono_get_got_var (cfg);
				break;
			}
			case CEE_SIZEOF: {
				guint32 align;
				int ialign;

				CHECK_STACK_OVF (1);
				CHECK_OPSIZE (6);
				token = read32 (ip + 2);
				/* FIXXME: handle generics. */
				if (mono_metadata_token_table (token) == MONO_TABLE_TYPESPEC) {
					MonoType *type = mono_type_create_from_typespec (image, token);
					token = mono_type_size (type, &ialign);
				} else {
					MonoClass *klass = mono_class_get_full (image, token, generic_context);
					if (!klass)
						goto load_error;
					mono_class_init (klass);
					token = mono_class_value_size (klass, &align);
				}
				EMIT_NEW_ICONST (cfg, ins, token);
				ins->cil_code = ip;
				*sp++= ins;
				ip += 6;
				break;
			}
			case CEE_REFANYTYPE: {
				int dreg = alloc_preg (cfg);

				CHECK_STACK (1);
				--sp;

				EMIT_NEW_BIALU_IMM (cfg, ins, OP_PADD_IMM, dreg, sp [0]->dreg, G_STRUCT_OFFSET (MonoTypedRef, type));
				ins->type = STACK_VTYPE;
				ins->klass = mono_defaults.typehandle_class;
				*sp++ = ins;
				ip += 2;
				break;
			}
			case CEE_READONLY_:
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
		goto unverified;

	bblock->cil_length = ip - bblock->cil_code;
	bblock->next_bb = end_bblock;

	if (cfg->method == method && cfg->domainvar) {
		MonoInst *store;
		MonoInst *get_domain;

		cfg->cbb = init_localsbb;

		if (! (get_domain = mono_arch_get_domain_intrinsic (cfg))) {
			get_domain = mono_emit_jit_icall (cfg, mono_domain_get, NULL, ip);
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

	if (header->init_locals) {
		MonoInst *store;

		cfg->cbb = init_localsbb;
		for (i = 0; i < header->num_locals; ++i) {
			MonoType *ptype = header->locals [i];
			int t = ptype->type;
			dreg = cfg->varinfo [locals_offset + i]->dreg;

			if (t == MONO_TYPE_VALUETYPE && ptype->data.klass->enumtype)
				t = ptype->data.klass->enum_basetype->type;
			if (ptype->byref) {
				MONO_EMIT_NEW_PCONST (cfg, dreg, NULL);
			} else if (t >= MONO_TYPE_BOOLEAN && t <= MONO_TYPE_U4) {
				MONO_EMIT_NEW_ICONST (cfg, cfg->varinfo [locals_offset + i]->dreg, 0);
			} else if (t == MONO_TYPE_I8 || t == MONO_TYPE_U8) {
				MONO_EMIT_NEW_I8CONST (cfg, cfg->varinfo [locals_offset + i]->dreg, 0);
			} else if (t == MONO_TYPE_R4 || t == MONO_TYPE_R8) {
				MONO_INST_NEW (cfg, ins, OP_R8CONST);
				ins->type = STACK_R8;
				ins->inst_p0 = (void*)&r8_0;
				ins->dreg = alloc_dreg (cfg, STACK_R8);
				MONO_ADD_INS (init_localsbb, ins);
				NEW_LOCSTORE (cfg, store, i, ins);
				MONO_ADD_INS (init_localsbb, store);
			} else if ((t == MONO_TYPE_VALUETYPE) || (t == MONO_TYPE_TYPEDBYREF) ||
				   ((t == MONO_TYPE_GENERICINST) && mono_metadata_generic_class_is_valuetype (ptype->data.generic_class))) {
				EMIT_NEW_LOCLOADA (cfg, ins, i);
				handle_initobj (cfg, ins, NULL, mono_class_from_mono_type (ptype), NULL, NULL);
			} else {
				MONO_EMIT_NEW_PCONST (cfg, dreg, NULL);
			}
		}
	}

	/* resolve backward branches in the middle of an existing basic block */
	for (tmp = bb_recheck; tmp; tmp = tmp->next) {
		bblock = tmp->data;
		/*printf ("need recheck in %s at IL_%04x\n", method->name, bblock->cil_code - header->code);*/
		tblock = find_previous (bbhash, start_bblock, bblock->cil_code);
		if (tblock != start_bblock) {
			int l;
			split_bblock (cfg, tblock, bblock);
			l = bblock->cil_code - header->code;
			bblock->cil_length = tblock->cil_length - l;
			tblock->cil_length = l;
		} else {
			printf ("recheck failed.\n");
		}
	}

	if (cfg->method == method) {
		MonoBasicBlock *bb;
		for (bb = cfg->bb_entry; bb; bb = bb->next_bb) {
			bb->region = mono_find_block_region (cfg, bb->real_offset);
			if (cfg->spvars)
				mono_create_spvar_for_region (cfg, bb->region);
			if (cfg->verbose_level > 2)
				printf ("REGION BB%d IL_%04x ID_%08X\n", bb->block_num, bb->real_offset, bb->region);
		}
	} else {
		g_hash_table_destroy (bbhash);
	}

	g_slist_free (class_inits);
	dont_inline = g_list_remove (dont_inline, method);
	return inline_costs;

 inline_failure:
	if (cfg->method != method) 
		g_hash_table_destroy (bbhash);
	g_slist_free (class_inits);
	dont_inline = g_list_remove (dont_inline, method);
	return -1;

 load_error:
	if (cfg->method != method)
		g_hash_table_destroy (bbhash);
	g_slist_free (class_inits);
	dont_inline = g_list_remove (dont_inline, method);
	return -1;

 unverified:
	if (cfg->method != method) 
		g_hash_table_destroy (bbhash);
	g_slist_free (class_inits);
	g_error ("Invalid IL code at IL%04x in %s: %s\n", (int)(ip - header->code), 
		 mono_method_full_name (method, TRUE), mono_disasm_code_one (NULL, method, ip, NULL));
	dont_inline = g_list_remove (dont_inline, method);
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

int
mono_op_to_op_imm (int opcode)
{
	switch (opcode) {
	case OP_IADD:
		return OP_IADD_IMM;
	case OP_ISUB:
		return OP_ISUB_IMM;
		/* FIXME: On x86, there are no such instructions
	case OP_IDIV:
		return OP_IDIV_IMM;
	case OP_IDIV_UN:
		return OP_IDIV_UN_IMM;
	case OP_IREM:
		return OP_IREM_IMM;
	case OP_IREM_UN:
		return OP_IREM_UN_IMM;
		*/
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

	case OP_STORE_MEMBASE_REG:
		return OP_STORE_MEMBASE_IMM;
	case OP_STOREI1_MEMBASE_REG:
		return OP_STOREI1_MEMBASE_IMM;
	case OP_STOREI2_MEMBASE_REG:
		return OP_STOREI2_MEMBASE_IMM;
	case OP_STOREI4_MEMBASE_REG:
		return OP_STOREI4_MEMBASE_IMM;
	case OP_X86_PUSH:
		return OP_X86_PUSH_IMM;

	case OP_VOIDCALL_REG:
		return OP_VOIDCALL;
	case OP_CALL_REG:
		return OP_CALL;
	case OP_LCALL_REG:
		return OP_LCALL;
	case OP_FCALL_REG:
		return OP_FCALL;
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
#if SIZEOF_VOID_P == 8
	case OP_LOADI8_MEMBASE:
		return OP_LOADI8_MEM;
#endif
	}

	return -1;
}

static inline int
op_to_op_dest_membase (int opcode)
{
#ifdef __i386__
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
	}
#endif

	return -1;
}

static inline int
op_to_op_src1_membase (int load_opcode, int opcode)
{
#ifdef __i386__
	if ((opcode == OP_ICOMPARE_IMM) && (load_opcode == OP_LOADU1_MEMBASE))
		return OP_X86_COMPARE_MEMBASE8_IMM;

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

	return -1;
}

static inline int
op_to_op_src2_membase (int load_opcode, int opcode)
{
#ifdef __i386__
	if (!((load_opcode == OP_LOAD_MEMBASE) || (load_opcode == OP_LOADI4_MEMBASE) || (load_opcode == OP_LOADU4_MEMBASE)))
		return -1;
	
	switch (opcode) {
	case OP_COMPARE:
	case OP_ICOMPARE:
		return OP_X86_COMPARE_REG_MEMBASE;
	}
#endif

	return -1;
}

/**
 * mono_handle_global_vregs:
 *
 *   Make vregs used in more than one bblock 'global', i.e. allocate a variable
 * for them.
 */
void
mono_handle_global_vregs (MonoCompile *cfg)
{
	MonoBasicBlock ***vreg_to_bb;
	MonoBasicBlock *bb;
	int i;

	vreg_to_bb = g_new0 (MonoBasicBlock**, 256);
	vreg_to_bb ['i'] = g_new0 (MonoBasicBlock*, cfg->next_vireg);
	vreg_to_bb ['l'] = g_new0 (MonoBasicBlock*, cfg->next_vireg);
	/* The vfregs are allocated from the same pool as the viregs */
	vreg_to_bb ['f'] = vreg_to_bb ['i'];

	/* Find local vregs used in more than one bb */
	/* FIXME: Optimize this */
	for (bb = cfg->bb_entry; bb; bb = bb->next_bb) {
		MonoInst *ins = bb->code;	
		MonoInst *prev = NULL;

		if (cfg->verbose_level > 1)
			printf ("\nHANDLE-GLOBAL-VREGS BLOCK %d:\n", bb->block_num);

		cfg->cbb = bb;
		for (; ins; ins = ins->next) {
			const char *spec = ins_info [ins->opcode - OP_START - 1];
			int regtype, regindex;

			if (cfg->verbose_level > 1)
				mono_print_ins (ins);

			if (ins->opcode == OP_NOP) {
				prev = ins;
				continue;
			}

			if (ins->opcode < MONO_CEE_LAST)
				spec = "   ";

			for (regindex = 0; regindex < 3; regindex ++) {
				int vreg;

				if (regindex == 0) {
					vreg = ins->dreg;
					regtype = spec [MONO_INST_DEST];
				} else if (regindex == 1) {
					vreg = ins->sreg1;
					regtype = spec [MONO_INST_SRC1];
				} else {
					vreg = ins->sreg2;
					regtype = spec [MONO_INST_SRC2];
				}

				if (((regtype == 'i' && (vreg < MONO_MAX_IREGS))) || (regtype == 'f' && (vreg < MONO_MAX_FREGS)))
					continue;

				if ((regtype == 'l') && (vreg != -1)) {
					/*
					 * Since some instructions reference the original long vreg,
					 * and some references the two component vregs, it is quite hard
					 * to determine when it needs to be global. So be conservative.
					 */
					if (!get_vreg_to_inst (cfg, vreg)) {
						mono_compile_create_var_for_vreg (cfg, &mono_defaults.int64_class->byval_arg, OP_LOCAL, 'l', vreg);

						if (cfg->verbose_level > 1)
							printf ("LONG VREG R%d made global.\n", vreg);
					}
				}

				if (vreg != -1) {
					if (vreg_to_bb [regtype][vreg] == NULL) {
						vreg_to_bb [regtype][vreg] = bb;
					} else if (vreg_to_bb [regtype][vreg] != bb) {
						if (!get_vreg_to_inst (cfg, vreg)) {
							if (cfg->verbose_level > 1)
								printf ("VREG R%d used in BB%d and BB%d made global.\n", vreg, vreg_to_bb [regtype][vreg]->block_num, bb->block_num);

							switch (regtype) {
							case 'i':
								mono_compile_create_var_for_vreg (cfg, &mono_defaults.int_class->byval_arg, OP_LOCAL, 'i', vreg);
								break;
							case 'f':
								mono_compile_create_var_for_vreg (cfg, &mono_defaults.double_class->byval_arg, OP_LOCAL, 'f', vreg);
								break;
							default:
								g_assert_not_reached ();
							}
						}

						/* Flag as having been used in more than one bb */
						if (vreg_to_bb [regtype][vreg] != (gpointer)(gssize)-1)
							vreg_to_bb [regtype][vreg] = (gpointer)(gssize)-1;
					}
				}
			}
		}
	}

	/* If a variable is used in only one bblock, convert it into a local vreg */
	for (i = 0; i < cfg->num_varinfo; i++) {
		MonoInst *var = cfg->varinfo [i];
		MonoMethodVar *vmv = MONO_VARINFO (cfg, i);

		/* Generalize to other types */
		switch (var->type) {
		case STACK_I4:
		case STACK_OBJ:
		case STACK_PTR:
		case STACK_MP:
#if SIZEOF_VOID_P == 8
		case STACK_I8:
#endif
			/* Arguments are implicitly global */
			if ((var->opcode != OP_ARG) && (var != cfg->ret) && !(var->flags & (MONO_INST_VOLATILE|MONO_INST_INDIRECT)) && (vreg_to_bb ['i'][var->dreg] != (gpointer)(gssize)-1)) {
				if (cfg->verbose_level > 2)
					printf ("CONVERTED R%d(%d) TO VREG.\n", var->dreg, vmv->idx);
				var->flags |= MONO_INST_IS_DEAD;
				cfg->vreg_to_inst [var->dreg] = NULL;
			}
			break;
		}
	}

	g_free (vreg_to_bb ['i']);
	g_free (vreg_to_bb ['l']);
	g_free (vreg_to_bb);
}

static void
insert_before_ins (MonoBasicBlock *bb, MonoInst *ins, MonoInst *ins_to_insert, MonoInst **prev)
{
	if (*prev)
		(*prev)->next = ins_to_insert;
	else
		bb->code = ins_to_insert;
	
	ins_to_insert->next = ins;
	*prev = ins_to_insert;
}

static void
insert_after_ins (MonoBasicBlock *bb, MonoInst *ins, MonoInst *ins_to_insert)
{
   ins_to_insert->next = ins->next;
   ins->next = ins_to_insert;
   if (bb->last_ins == ins)
	   bb->last_ins = ins_to_insert;
}

/**
 * mono_spill_global_vars:
 *
 *   Generate spill code for variables which are not allocated to registers, 
 * and replace vregs with their allocated hregs.
 */
void
mono_spill_global_vars (MonoCompile *cfg)
{
	guint32 *stacktypes;
	MonoBasicBlock *bb;
	char spec2 [16];
#if SIZEOF_VOID_P == 4
	int i;
#endif

	memset (spec2, 0, sizeof (spec2));

	/* FIXME: Move this function to mini.c */
	stacktypes = g_new0 (guint32, 128);
	stacktypes ['i'] = STACK_PTR;
	stacktypes ['l'] = STACK_I8;
	stacktypes ['f'] = STACK_R8;

#if SIZEOF_VOID_P == 4
	/* Create MonoInsts for longs */
	for (i = 0; i < cfg->num_varinfo; i++) {
		MonoInst *ins = cfg->varinfo [i];

		if (ins->opcode != OP_REGVAR) {
			switch (ins->type) {
			case STACK_I8: {
				MonoInst *tree;

				g_assert (ins->opcode == OP_REGOFFSET);

				tree = get_vreg_to_inst (cfg, ins->dreg + 1);
				g_assert (tree);
				tree->opcode = OP_REGOFFSET;
				tree->inst_basereg = ins->inst_basereg;
				tree->inst_offset = ins->inst_offset;

				tree = get_vreg_to_inst (cfg, ins->dreg + 2);
				g_assert (tree);
				tree->opcode = OP_REGOFFSET;
				tree->inst_basereg = ins->inst_basereg;
				tree->inst_offset = ins->inst_offset + 4;
				break;
			}
			default:
				break;
			}
		}
	}
#endif

	/* FIXME: widening and truncation */

	/* Add spill loads/stores */
	for (bb = cfg->bb_entry; bb; bb = bb->next_bb) {
		MonoInst *ins = bb->code;	
		MonoInst *prev = NULL;

		if (cfg->verbose_level > 1)
			printf ("\nSPILL BLOCK %d:\n", bb->block_num);

		cfg->cbb = bb;
		for (; ins; ins = ins->next) {
			const char *spec = ins_info [ins->opcode - OP_START - 1];
			int regtype, srcindex, sreg, tmp_reg;
			gboolean store;

			if (cfg->verbose_level > 1)
				mono_print_ins (ins);

			if (ins->opcode == OP_NOP) {
				prev = ins;
				continue;
			}

			/* 
			 * We handle LDADDR here as well, since it can only be decomposed
			 * when variable addresses are known.
			 */
			if (ins->opcode == OP_LDADDR) {
				MonoInst *var = ins->inst_p0;

				g_assert (var->opcode == OP_REGOFFSET);

				ins->opcode = OP_ADD_IMM;
				ins->sreg1 = var->inst_basereg;
				ins->inst_imm = var->inst_offset;

				spec = ins_info [ins->opcode - OP_START - 1];
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

			if (cfg->verbose_level > 1)
				printf ("\t %s %d %d %d\n", spec, ins->dreg, ins->sreg1, ins->sreg2);

			/* DREG */
			regtype = spec [MONO_INST_DEST];
			g_assert (((ins->dreg == -1) && (regtype == ' ')) || ((ins->dreg != -1) && (regtype != ' ')));

			if ((ins->dreg != -1) && get_vreg_to_inst (cfg, ins->dreg)) {
				MonoInst *var = get_vreg_to_inst (cfg, ins->dreg);
				MonoInst *store_ins;
				int store_opcode;

				if (var->opcode == OP_REGVAR) {
					ins->dreg = var->dreg;
				} else if ((ins->dreg == ins->sreg1) && (spec [MONO_INST_DEST] == 'i') && (spec [MONO_INST_SRC1] == 'i') && (op_to_op_dest_membase (ins->opcode) != -1)) {
					/* 
					 * Instead of emitting a load+store, use a _membase opcode.
					 */
					g_assert (var->opcode == OP_REGOFFSET);
					ins->opcode = op_to_op_dest_membase (ins->opcode);
					ins->inst_basereg = var->inst_basereg;
					ins->inst_offset = var->inst_offset;
					ins->dreg = -1;
					spec = ins_info [ins->opcode - OP_START - 1];
				} else {
					g_assert (var->opcode == OP_REGOFFSET);

					ins->dreg = alloc_dreg (cfg, stacktypes [regtype]);

					if (regtype == 'l') {
						NEW_STORE_MEMBASE (cfg, store_ins, OP_STOREI4_MEMBASE_REG, var->inst_basereg, var->inst_offset + MINI_LS_WORD_OFFSET, ins->dreg + 1);
						insert_after_ins (bb, ins, store_ins);
						NEW_STORE_MEMBASE (cfg, store_ins, OP_STOREI4_MEMBASE_REG, var->inst_basereg, var->inst_offset + MINI_MS_WORD_OFFSET, ins->dreg + 2);
						insert_after_ins (bb, ins, store_ins);
					}
					else {
						store_opcode = mono_type_to_store_membase (var->inst_vtype);
						/* Try to fuse the store into the instruction itself */
						/* FIXME: Add more instructions */
						if ((ins->opcode == OP_ICONST) || ((ins->opcode == OP_I8CONST) && (ins->inst_c0 == 0))) {
							ins->opcode = store_membase_reg_to_store_membase_imm (store_opcode);
							ins->inst_imm = ins->inst_c0;
							ins->inst_destbasereg = var->inst_basereg;
							ins->inst_offset = var->inst_offset;
						} else if ((ins->opcode == OP_MOVE) || (ins->opcode == OP_FMOVE) || (ins->opcode == OP_LMOVE)) {
							ins->opcode = store_opcode;
							ins->inst_destbasereg = var->inst_basereg;
							ins->inst_offset = var->inst_offset;
						} else {
							/* printf ("INS: "); mono_print_ins (ins); */
							/* Create a store instruction */
							NEW_STORE_MEMBASE (cfg, store_ins, store_opcode, var->inst_basereg, var->inst_offset, ins->dreg);

							/* Insert it after the instruction */
							insert_after_ins (bb, ins, store_ins);
						}
					}
				}
			}

			/* SREGS */
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
					} else {
						g_assert (var->opcode == OP_REGOFFSET);

						load_opcode = mono_type_to_load_membase (var->inst_vtype);

						/* Try to fuse the load into the instruction */
						/* FIXME: Generalize this to 64 bit */
						if ((srcindex == 0) && (op_to_op_src1_membase (load_opcode, ins->opcode) != -1)) {
							ins->opcode = op_to_op_src1_membase (load_opcode, ins->opcode);
							ins->inst_basereg = var->inst_basereg;
							ins->inst_offset = var->inst_offset;
						} else if ((srcindex == 1) && (op_to_op_src2_membase (load_opcode, ins->opcode) != -1)) {
							ins->opcode = op_to_op_src2_membase (load_opcode, ins->opcode);
							ins->sreg2 = var->inst_basereg;
							ins->inst_offset = var->inst_offset;
						} else {
							if (ins->opcode == OP_MOVE) {
								ins->opcode = OP_NOP;
								sreg = ins->dreg;
							} else {
								sreg = alloc_dreg (cfg, stacktypes [regtype]);
							}

							if (srcindex == 0)
								ins->sreg1 = sreg;
							else
								ins->sreg2 = sreg;

							if (regtype == 'l') {
								NEW_LOAD_MEMBASE (cfg, load_ins, OP_LOADI4_MEMBASE, sreg + 2, var->inst_basereg, var->inst_offset + MINI_MS_WORD_OFFSET);
								insert_before_ins (bb, ins, load_ins, &prev);
								NEW_LOAD_MEMBASE (cfg, load_ins, OP_LOADI4_MEMBASE, sreg + 1, var->inst_basereg, var->inst_offset + MINI_LS_WORD_OFFSET);
								insert_before_ins (bb, ins, load_ins, &prev);
							}
							else {
								NEW_LOAD_MEMBASE (cfg, load_ins, load_opcode, sreg, var->inst_basereg, var->inst_offset);
								insert_before_ins (bb, ins, load_ins, &prev);
							}
						}
					}
				}
			}

			if (store) {
				tmp_reg = ins->dreg;
				ins->dreg = ins->sreg2;
				ins->sreg2 = tmp_reg;
			}

			if (cfg->verbose_level > 1)
				mono_print_ins (ins);

			prev = ins;
		}
	}

	g_free (stacktypes);
}

/**
 * FIXME:
 * - use NEW/EMIT macros correctly
 * - use 'iadd' instead of 'int_add'
 * - handling ovf opcodes: decompose in method_to_ir.
 * - unify to_regstore/to_regload as to_regmove
 * - unify iregs/fregs
 *   -> partly done, the missing parts are:
 *   - a more complete unification would involve unifying the hregs as well, so
 *     code wouldn't need if (fp) all over the place. but that would mean the hregs
 *     would no longer map to the machine hregs, so the code generators would need to
 *     be modified. Also, on ia64 for example, niregs + nfregs > 256 -> bitmasks
 *     wouldn't work any more.
 * - use sext/zext opcodes instead of shifts
 * - make mono_print_ins use the arch-independent opcode metadata
 * - simplify the emitting of calls
 * - add OP_ICALL
 * - get rid of TEMPLOADs if possible and use vregs instead
 * - clean up/automatize setting of ins->cil_code
 * - clean up usage of OP_P/OP_ opcodes
 * - cleanup calls. especially the spill stuff
 * - handle the emit ins + need it/its dreg pattern
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
 * - integrate handle_stack_args into inline_method
 * - use sreg2 for destbasereg since it is not really a dreg
 * - cleanup creation of branches+bblocks in opcode decomposition
 * - get rid of the empty bblocks created by MONO_EMIT_NEW_BRACH_BLOCK2
 * - Things to backport to the old JIT:
 *   - op_atomic_exchange fix for amd64
 *   - localloc fix for amd64
 *   - x86 type_token change
 *   - lconv fixes
 * - handle long shift opts on 32 bit platforms somehow: they require 
 *   3 sregs (2 for arg1 and 1 for arg2)
 * - make byref a 'normal' type.
 * - use vregs for bb->out_stacks if possible, handle_global_vreg will make them a
 *   variable if needed.
 * - do not start a new IL level bblock when cfg->cbb is changed by a function call
 *   like inline_method.
 * - remove inlining restrictions
 * - remove mono_save_args.
 * - spill_global_vars does not play nicely with the fp stack (loads are inserted at
 *   the wrong place).
 * - get rid of redundant loads and stores inserted by spill_global_vars.
 * - add 'introduce a new optimization to simplify some range checks'
 * - fix LNEG and enable cfold of INEG
 * - generalize x86 optimizations like ldelema as a peephole optimization
 * - port the x86 optimizations to amd64 as well
 * - add store_mem_imm for amd64
 * - optimize the loading of the interruption flag in the managed->native wrappers
 * - avoid special handling of OP_NOP in passes
 * - move code inserting instructions into one function/macro.
 * - some tests no longer work with COUNT=0
 * - LAST MERGE: 56617.
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
  - When a vtype is pushed on the stack, a new tempotary is created, an 
    instruction computing its address (LDADDR) is emitted and pushed on
    the stack. Need to optimize cases when the vtype is used immediately as in
    argument passing, stloc etc.
- Instead of the to_end stuff in the old JIT, simply call the function handling
  the values on the stack before emitting the last instruction of the bb.
*/
