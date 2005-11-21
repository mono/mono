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

#include "mini.h"
#include <string.h>
#include <ctype.h>
#include "inssel.h"
#include "trace.h"

#include "jit-icalls.c"

#include "aliasing.h"

/* 
 * this is used to determine when some branch optimizations are possible: we exclude FP compares
 * because they have weird semantics with NaNs.
 */
#define MONO_IS_COND_BRANCH_OP(ins) (((ins)->opcode >= CEE_BEQ && (ins)->opcode <= CEE_BLT_UN) || ((ins)->opcode >= OP_LBEQ && (ins)->opcode <= OP_LBLT_UN) || ((ins)->opcode >= OP_FBEQ && (ins)->opcode <= OP_FBLT_UN) || ((ins)->opcode >= OP_IBEQ && (ins)->opcode <= OP_IBLT_UN))
#define MONO_IS_COND_BRANCH_NOFP(ins) (MONO_IS_COND_BRANCH_OP(ins) && (ins)->inst_left->inst_left->type != STACK_R8)

#define MONO_CHECK_THIS(ins) (mono_method_signature (cfg->method)->hasthis && (ins)->ssa_op == MONO_SSA_LOAD && (ins)->inst_left->inst_c0 == 0)

static void setup_stat_profiler (void);
gboolean  mono_arch_print_tree(MonoInst *tree, int arity);
static gpointer mono_jit_compile_method_with_opt (MonoMethod *method, guint32 opt);
static gpointer mono_jit_compile_method (MonoMethod *method);
static gpointer mono_jit_find_compiled_method (MonoDomain *domain, MonoMethod *method);

static void handle_stobj (MonoCompile *cfg, MonoBasicBlock *bblock, MonoInst *dest, MonoInst *src, 
			  const unsigned char *ip, MonoClass *klass, gboolean to_end, gboolean native);

static void dec_foreach (MonoInst *tree, MonoCompile *cfg);

int mono_method_to_ir2 (MonoCompile *cfg, MonoMethod *method, MonoBasicBlock *start_bblock, MonoBasicBlock *end_bblock, 
		   int locals_offset, MonoInst *return_var, GList *dont_inline, MonoInst **inline_args, 
		   guint inline_offset, gboolean is_virtual_call);

extern guint8 mono_burg_arity [];
/* helper methods signature */
static MonoMethodSignature *helper_sig_class_init_trampoline = NULL;
static MonoMethodSignature *helper_sig_domain_get = NULL;

static guint32 default_opt = 0;

guint32 mono_jit_tls_id = -1;
MonoTraceSpec *mono_jit_trace_calls = NULL;
gboolean mono_break_on_exc = FALSE;
#ifndef DISABLE_AOT
gboolean mono_compile_aot = FALSE;
#endif
gboolean mono_use_security_manager = FALSE;

static int mini_verbose = 0;

#define mono_jit_lock() EnterCriticalSection (&jit_mutex)
#define mono_jit_unlock() LeaveCriticalSection (&jit_mutex)
static CRITICAL_SECTION jit_mutex;

static GHashTable *class_init_hash_addr = NULL;

static MonoCodeManager *global_codeman = NULL;

static GHashTable *jit_icall_name_hash = NULL;

static MonoDebugOptions debug_options;

MonoJumpInfoToken *
mono_jump_info_token_new (MonoMemPool *mp, MonoImage *image, guint32 token)
{
	MonoJumpInfoToken *res = mono_mempool_alloc0 (mp, sizeof (MonoJumpInfoToken));
	res->image = image;
	res->token = token;

	return res;
}

#define MONO_INIT_VARINFO(vi,id) do { \
	(vi)->range.first_use.pos.bid = 0xffff; \
	(vi)->reg = -1; \
        (vi)->idx = (id); \
} while (0)

static inline
alloc_ireg (MonoCompile *cfg)
{
	return cfg->max_ivreg ++;
}

static inline
alloc_freg (MonoCompile *cfg)
{
	return cfg->max_fvreg ++;
}

static inline
alloc_dreg (MonoCompile *cfg, MonoStackType stack_type)
{
	switch (stack_type) {
	case STACK_I4:
	case STACK_PTR:
		return alloc_ireg (cfg);
	default:
		g_assert_not_reached ();
	}
}

/*
 * Basic blocks have two numeric identifiers:
 * dfn: Depth First Number
 * block_num: unique ID assigned at bblock creation
 */
#define NEW_BBLOCK(cfg) (mono_mempool_alloc0 ((cfg)->mempool, sizeof (MonoBasicBlock)))
#define ADD_BBLOCK(cfg,bbhash,b) do {	\
		g_hash_table_insert (bbhash, (b)->cil_code, (b));	\
		(b)->block_num = cfg->num_bblocks++;	\
		(b)->real_offset = real_offset;	\
	} while (0)

#define GET_BBLOCK(cfg,bbhash,tblock,ip) do {	\
		(tblock) = g_hash_table_lookup (bbhash, (ip));	\
		if (!(tblock)) {	\
			if ((ip) >= end || (ip) < header->code) goto unverified; \
			(tblock) = NEW_BBLOCK (cfg);	\
			(tblock)->cil_code = (ip);	\
			ADD_BBLOCK (cfg, (bbhash), (tblock));	\
		} \
	} while (0)

#define CHECK_BBLOCK(target,ip,tblock) do {	\
		if ((target) < (ip) && !(tblock)->code)	{	\
			bb_recheck = g_list_prepend (bb_recheck, (tblock));	\
			if (cfg->verbose_level > 2) g_print ("queued block %d for check at IL%04x from IL%04x\n", (tblock)->block_num, (int)((target) - header->code), (int)((ip) - header->code));	\
		}	\
	} while (0)

#define NOT_IMPLEMENTED g_assert_not_reached ()

#define NEW_ICONST(cfg,dest,val) do {	\
		(dest) = mono_mempool_alloc0 ((cfg)->mempool, sizeof (MonoInst));	\
		(dest)->opcode = OP_ICONST;	\
		(dest)->inst_c0 = (val);	\
		(dest)->type = STACK_I4;	\
		(dest)->dreg = alloc_dreg ((cfg), STACK_I4);	\
		mono_bblock_add_inst ((cfg)->cbb, (dest)); \
	} while (0)

#define NEW_PCONST(cfg,dest,val) do {	\
		(dest) = mono_mempool_alloc0 ((cfg)->mempool, sizeof (MonoInst));	\
		(dest)->opcode = OP_PCONST;	\
		(dest)->inst_p0 = (val);	\
		(dest)->type = STACK_PTR;	\
		(dest)->dreg = alloc_dreg ((cfg), STACK_PTR);	\
		mono_bblock_add_inst ((cfg)->cbb, (dest)); \
	} while (0)


#ifdef MONO_ARCH_NEED_GOT_VAR

#define NEW_PATCH_INFO(cfg,dest,el1,el2) do {	\
		(dest) = mono_mempool_alloc0 ((cfg)->mempool, sizeof (MonoInst));	\
		(dest)->opcode = OP_PATCH_INFO;	\
		(dest)->inst_left = (gpointer)(el1);	\
		(dest)->inst_right = (gpointer)(el2);	\
	} while (0)

#define NEW_AOTCONST(cfg,dest,patch_type,cons) do {			\
		(dest) = mono_mempool_alloc0 ((cfg)->mempool, sizeof (MonoInst)); \
		(dest)->opcode = cfg->compile_aot ? OP_GOT_ENTRY : OP_PCONST; \
		if (cfg->compile_aot) {					\
			MonoInst *group, *got_var, *got_loc;		\
			got_loc = mono_get_got_var (cfg);		\
			NEW_TEMPLOAD ((cfg), got_var, got_loc->inst_c0); \
			NEW_PATCH_INFO ((cfg), group, cons, patch_type); \
			(dest)->inst_p0 = got_var;			\
			(dest)->inst_p1 = group;			\
		} else {						\
			(dest)->inst_p0 = (cons);			\
			(dest)->inst_i1 = (gpointer)(patch_type);	\
		}							\
		(dest)->type = STACK_PTR;				\
		(dest)->dreg = alloc_dreg ((cfg), STACK_PTR);	\
		mono_bblock_add_inst ((cfg)->cbb, (dest)); \
	} while (0)

#define NEW_AOTCONST_TOKEN(cfg,dest,patch_type,image,token,stack_type) do { \
		MonoInst *group, *got_var, *got_loc;			\
		(dest) = mono_mempool_alloc0 ((cfg)->mempool, sizeof (MonoInst)); \
		(dest)->opcode = OP_GOT_ENTRY;				\
		got_loc = mono_get_got_var (cfg);			\
		NEW_TEMPLOAD ((cfg), got_var, got_loc->inst_c0);	\
		NEW_PATCH_INFO ((cfg), group, NULL, patch_type);	\
		group->inst_p0 = mono_jump_info_token_new ((cfg)->mempool, (image), (token)); \
		(dest)->inst_p0 = got_var;				\
		(dest)->inst_p1 = group;				\
		(dest)->type = (stack_type);				\
		(dest)->dreg = alloc_dreg ((cfg), (stack_type));	\
		mono_bblock_add_inst ((cfg)->cbb, (dest)); \
	} while (0)

#else

#define NEW_AOTCONST(cfg,dest,patch_type,cons) do {    \
		(dest) = mono_mempool_alloc0 ((cfg)->mempool, sizeof (MonoInst));	\
		(dest)->opcode = cfg->compile_aot ? OP_AOTCONST : OP_PCONST;	\
		(dest)->inst_p0 = (cons);	\
		(dest)->inst_i1 = (gpointer)(patch_type); \
		(dest)->type = STACK_PTR;	\
		(dest)->dreg = alloc_dreg ((cfg), STACK_PTR);	\
		mono_bblock_add_inst ((cfg)->cbb, (dest)); \
    } while (0)

#define NEW_AOTCONST_TOKEN(cfg,dest,patch_type,image,token,stack_type) do {    \
		(dest) = mono_mempool_alloc0 ((cfg)->mempool, sizeof (MonoInst));	\
		(dest)->opcode = OP_AOTCONST;	\
		(dest)->inst_p0 = mono_jump_info_token_new ((cfg)->mempool, (image), (token));	\
		(dest)->inst_p1 = (gpointer)(patch_type); \
		(dest)->type = (stack_type);	\
		(dest)->dreg = alloc_dreg ((cfg), (stack_type));	\
		mono_bblock_add_inst ((cfg)->cbb, (dest)); \
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
            NOT_IMPLEMENTED; \
			/* avoid depending on undefined C behavior in sequence points */ \
			MonoInst* __domain_var = mono_get_domainvar (cfg); \
			NEW_TEMPLOAD (cfg, dest, __domain_var->inst_c0); \
		} else { \
			NEW_PCONST (cfg, dest, (cfg)->domain); \
		} \
	} while (0)

#define GET_VARINFO_INST(cfg,num) ((cfg)->varinfo [(num)]->inst)

#define NEW_ARGLOAD(cfg,dest,num) do {	\
                if (arg_array [(num)]->opcode == OP_ICONST) (dest) = arg_array [(num)]; else { \
		(dest) = mono_mempool_alloc0 ((cfg)->mempool, sizeof (MonoInst));	\
		(dest)->ssa_op = MONO_SSA_LOAD;	\
		(dest)->inst_i0 = arg_array [(num)];	\
		(dest)->opcode = mono_type_to_ldind ((dest)->inst_i0->inst_vtype);	\
		type_to_eval_stack_type (param_types [(num)], (dest));	\
		(dest)->klass = (dest)->inst_i0->klass;	\
	}} while (0)

#define NEW_LOCLOAD(cfg,dest,num) do {	\
		(dest) = mono_mempool_alloc0 ((cfg)->mempool, sizeof (MonoInst));	\
		(dest)->ssa_op = MONO_SSA_LOAD;	\
		(dest)->inst_i0 = (cfg)->varinfo [locals_offset + (num)];	\
		(dest)->opcode = mono_type_to_ldind ((dest)->inst_i0->inst_vtype);	\
		type_to_eval_stack_type (header->locals [(num)], (dest));	\
		(dest)->klass = (dest)->inst_i0->klass;	\
	} while (0)

#define NEW_LOCLOADA(cfg,dest,num) do {	\
		(dest) = mono_mempool_alloc0 ((cfg)->mempool, sizeof (MonoInst));	\
		(dest)->ssa_op = MONO_SSA_ADDRESS_TAKEN;	\
		(dest)->inst_i0 = (cfg)->varinfo [locals_offset + (num)];	\
		(dest)->inst_i0->flags |= MONO_INST_INDIRECT;	\
		(dest)->opcode = OP_LDADDR;	\
		(dest)->type = STACK_MP;	\
		(dest)->klass = (dest)->inst_i0->klass;	\
        if (!MONO_TYPE_ISSTRUCT (header->locals [(num)])) \
           (cfg)->disable_ssa = TRUE; \
	} while (0)

#define NEW_RETLOADA(cfg,dest) do {	\
		(dest) = mono_mempool_alloc0 ((cfg)->mempool, sizeof (MonoInst));	\
		(dest)->ssa_op = MONO_SSA_ADDRESS_TAKEN;	\
		(dest)->inst_i0 = (cfg)->ret;	\
		(dest)->inst_i0->flags |= MONO_INST_INDIRECT;	\
		(dest)->opcode = cfg->ret_var_is_local ? OP_LDADDR : CEE_LDIND_I;	\
		(dest)->type = STACK_MP;	\
		(dest)->klass = (dest)->inst_i0->klass;	\
                (cfg)->disable_ssa = TRUE; \
	} while (0)

#define NEW_ARGLOADA(cfg,dest,num) do {	\
                if (arg_array [(num)]->opcode == OP_ICONST) goto inline_failure; \
		(dest) = mono_mempool_alloc0 ((cfg)->mempool, sizeof (MonoInst));	\
		(dest)->ssa_op = MONO_SSA_ADDRESS_TAKEN;	\
		(dest)->inst_i0 = arg_array [(num)];	\
		(dest)->inst_i0->flags |= MONO_INST_INDIRECT;	\
		(dest)->opcode = OP_LDADDR;	\
		(dest)->type = STACK_MP;	\
		(dest)->klass = (dest)->inst_i0->klass;	\
                (cfg)->disable_ssa = TRUE; \
	} while (0)

#define NEW_TEMPLOAD(cfg,dest,num) do {	\
		(dest) = mono_mempool_alloc0 ((cfg)->mempool, sizeof (MonoInst));	\
		(dest)->ssa_op = MONO_SSA_LOAD;	\
		(dest)->inst_i0 = (cfg)->varinfo [(num)];	\
		(dest)->opcode = mono_type_to_ldind ((dest)->inst_i0->inst_vtype);	\
		type_to_eval_stack_type ((dest)->inst_i0->inst_vtype, (dest));	\
		(dest)->klass = (dest)->inst_i0->klass;	\
	} while (0)

#define NEW_TEMPLOADA(cfg,dest,num) do {	\
		(dest) = mono_mempool_alloc0 ((cfg)->mempool, sizeof (MonoInst));	\
		(dest)->ssa_op = MONO_SSA_ADDRESS_TAKEN;	\
		(dest)->inst_i0 = (cfg)->varinfo [(num)];	\
		(dest)->inst_i0->flags |= MONO_INST_INDIRECT;	\
		(dest)->opcode = OP_LDADDR;	\
		(dest)->type = STACK_MP;	\
		(dest)->klass = (dest)->inst_i0->klass;	\
        if (!MONO_TYPE_ISSTRUCT (cfg->varinfo [(num)]->inst_vtype)) \
           (cfg)->disable_ssa = TRUE; \
	} while (0)


#define NEW_INDLOAD(cfg,dest,addr,vtype) do {	\
		(dest) = mono_mempool_alloc0 ((cfg)->mempool, sizeof (MonoInst));	\
		(dest)->inst_left = addr;	\
		(dest)->opcode = mono_type_to_ldind (vtype);	\
		type_to_eval_stack_type (vtype, (dest));	\
		/* FIXME: (dest)->klass = (dest)->inst_i0->klass;*/	\
	} while (0)

#define NEW_INDSTORE(cfg,dest,addr,value,vtype) do {	\
		(dest) = mono_mempool_alloc0 ((cfg)->mempool, sizeof (MonoInst));	\
		(dest)->inst_i0 = addr;	\
		(dest)->opcode = mono_type_to_stind (vtype);	\
		(dest)->inst_i1 = (value);	\
		/* FIXME: (dest)->klass = (dest)->inst_i0->klass;*/	\
	} while (0)

#define NEW_TEMPSTORE(cfg,dest,num,inst) do {	\
		(dest) = mono_mempool_alloc0 ((cfg)->mempool, sizeof (MonoInst));	\
		(dest)->ssa_op = MONO_SSA_STORE;	\
		(dest)->inst_i0 = (cfg)->varinfo [(num)];	\
		(dest)->opcode = mono_type_to_stind ((dest)->inst_i0->inst_vtype);	\
		(dest)->inst_i1 = (inst);	\
		(dest)->klass = (dest)->inst_i0->klass;	\
	} while (0)

#define NEW_LOCSTORE(cfg,dest,num,inst) do {	\
		(dest) = mono_mempool_alloc0 ((cfg)->mempool, sizeof (MonoInst));	\
		(dest)->opcode = mono_type_to_stind (header->locals [(num)]);	\
		(dest)->ssa_op = MONO_SSA_STORE;	\
		(dest)->inst_i0 = (cfg)->varinfo [locals_offset + (num)];	\
		(dest)->inst_i1 = (inst);	\
		(dest)->klass = (dest)->inst_i0->klass;	\
	} while (0)

#define NEW_ARGSTORE(cfg,dest,num,inst) do {	\
                if (arg_array [(num)]->opcode == OP_ICONST) goto inline_failure; \
		(dest) = mono_mempool_alloc0 ((cfg)->mempool, sizeof (MonoInst));	\
		(dest)->opcode = mono_type_to_stind (param_types [(num)]);	\
		(dest)->ssa_op = MONO_SSA_STORE;	\
		(dest)->inst_i0 = arg_array [(num)];	\
		(dest)->inst_i1 = (inst);	\
		(dest)->klass = (dest)->inst_i0->klass;	\
	} while (0)

#define NEW_DUMMY_USE(cfg,dest,load) do { \
		(dest) = mono_mempool_alloc0 ((cfg)->mempool, sizeof (MonoInst));	\
		(dest)->opcode = OP_DUMMY_USE; \
		(dest)->inst_left = (load); \
    } while (0)

#define NEW_DUMMY_STORE(cfg,dest,num) do { \
		(dest) = mono_mempool_alloc0 ((cfg)->mempool, sizeof (MonoInst));	\
		(dest)->inst_i0 = (cfg)->varinfo [(num)];	\
		(dest)->opcode = OP_DUMMY_STORE; \
		(dest)->klass = (dest)->inst_i0->klass;	\
	} while (0)

#define ADD_BINOP(op) do {	\
		MONO_INST_NEW (cfg, ins, (op));	\
		ins->cil_code = ip;	\
		sp -= 2;	\
		ins->inst_i0 = sp [0];	\
		ins->inst_i1 = sp [1];	\
		*sp++ = ins;	\
		type_from_op (ins);	\
		CHECK_TYPE (ins);	\
	} while (0)

#define ADD_UNOP(op) do {	\
		MONO_INST_NEW (cfg, ins, (op));	\
		ins->cil_code = ip;	\
		sp--;	\
		ins->inst_i0 = sp [0];	\
		*sp++ = ins;	\
		type_from_op (ins);	\
		CHECK_TYPE (ins);	\
	} while (0)

#define ADD_BINCOND(next_block) do {	\
		MonoInst *cmp;	\
		sp -= 2;		\
		MONO_INST_NEW(cfg, cmp, OP_COMPARE);	\
		cmp->inst_i0 = sp [0];	\
		cmp->inst_i1 = sp [1];	\
		cmp->cil_code = ins->cil_code;	\
		type_from_op (cmp);	\
		CHECK_TYPE (cmp);	\
		ins->inst_i0 = cmp;	\
		MONO_ADD_INS (bblock, ins);	\
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
	} while (0)

/* FIXME: handle float, long ... */
#define ADD_UNCOND(istrue) do {	\
		MonoInst *cmp;	\
		sp--;		\
		MONO_INST_NEW(cfg, cmp, OP_COMPARE);	\
		cmp->inst_i0 = sp [0];	\
                switch (cmp->inst_i0->type) { \
		case STACK_I8: \
			cmp->inst_i1 = zero_int64; break; \
		case STACK_R8: \
			cmp->inst_i1 = zero_r8; break; \
		case STACK_PTR: \
		case STACK_MP: \
			cmp->inst_i1 = zero_ptr; break;	\
		case STACK_OBJ: \
			cmp->inst_i1 = zero_obj; break;	\
		default: \
			cmp->inst_i1 = zero_int32;  \
		}  \
		cmp->cil_code = ins->cil_code;	\
		type_from_op (cmp);	\
		CHECK_TYPE (cmp);	\
		ins->inst_i0 = cmp;	\
		ins->opcode = (istrue)? CEE_BNE_UN: CEE_BEQ;	\
		MONO_ADD_INS (bblock, ins);	\
		ins->inst_many_bb = mono_mempool_alloc (cfg->mempool, sizeof(gpointer)*2);	\
		GET_BBLOCK (cfg, bbhash, tblock, target);		\
		link_bblock (cfg, bblock, tblock);	\
		ins->inst_true_bb = tblock;	\
		CHECK_BBLOCK (target, ip, tblock);	\
		GET_BBLOCK (cfg, bbhash, tblock, ip);		\
		link_bblock (cfg, bblock, tblock);	\
		ins->inst_false_bb = tblock;	\
		start_new_bblock = 2;	\
	} while (0)

#define NEW_LDELEMA(cfg,dest,sp,k) do {	\
		(dest) = mono_mempool_alloc0 ((cfg)->mempool, sizeof (MonoInst));	\
		(dest)->opcode = CEE_LDELEMA;	\
		(dest)->inst_left = (sp) [0];	\
		(dest)->inst_right = (sp) [1];	\
		(dest)->type = STACK_MP;	\
		(dest)->klass = (k);	\
		(cfg)->flags |= MONO_CFG_HAS_LDELEMA; \
	} while (0)

#define NEW_GROUP(cfg,dest,el1,el2) do {	\
		(dest) = mono_mempool_alloc0 ((cfg)->mempool, sizeof (MonoInst));	\
		(dest)->opcode = OP_GROUP;	\
		(dest)->inst_left = (el1);	\
		(dest)->inst_right = (el2);	\
	} while (0)

#if 0
static gint
compare_bblock (gconstpointer a, gconstpointer b)
{
	const MonoBasicBlock *b1 = a;
	const MonoBasicBlock *b2 = b;

	return b2->cil_code - b1->cil_code;
}
#endif

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
			g_print ("edge from IL%04x to IL_%04x\n", from->cil_code - cfg->cil_code, to->cil_code - cfg->cil_code);
		else
			g_print ("edge from IL%04x to exit\n", from->cil_code - cfg->cil_code);
	} else {
		if (to->cil_code)
			g_print ("edge from entry to IL_%04x\n", to->cil_code - cfg->cil_code);
		else
			g_print ("edge from entry to exit\n");
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

MonoInst *
mono_find_spvar_for_region (MonoCompile *cfg, int region)
{
	return g_hash_table_lookup (cfg->spvars, GINT_TO_POINTER (region));
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
	/*g_print ("visit %d at %p (BB%ld)\n", *dfn, start->cil_code, start->block_num);*/
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

	/*g_print ("start search at %p for %p\n", first->cil_code, second->cil_code);*/
	for (inst = first->code; inst && inst->next; inst = inst->next) {
		/*char *code = mono_disasm_code_one (NULL, cfg->method, inst->next->cil_code, NULL);
		g_print ("found %p: %s", inst->next->cil_code, code);
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

static guint32
reverse_branch_op (guint32 opcode)
{
	static const int reverse_map [] = {
		CEE_BNE_UN, CEE_BLT, CEE_BLE, CEE_BGT, CEE_BGE,
		CEE_BEQ, CEE_BLT_UN, CEE_BLE_UN, CEE_BGT_UN, CEE_BGE_UN
	};
	static const int reverse_fmap [] = {
		OP_FBNE_UN, OP_FBLT, OP_FBLE, OP_FBGT, OP_FBGE,
		OP_FBEQ, OP_FBLT_UN, OP_FBLE_UN, OP_FBGT_UN, OP_FBGE_UN
	};
	static const int reverse_lmap [] = {
		OP_LBNE_UN, OP_LBLT, OP_LBLE, OP_LBGT, OP_LBGE,
		OP_LBEQ, OP_LBLT_UN, OP_LBLE_UN, OP_LBGT_UN, OP_LBGE_UN
	};
	static const int reverse_imap [] = {
		OP_IBNE_UN, OP_IBLT, OP_IBLE, OP_IBGT, OP_IBGE,
		OP_IBEQ, OP_IBLT_UN, OP_IBLE_UN, OP_IBGT_UN, OP_IBGE_UN
	};
				
	if (opcode >= CEE_BEQ && opcode <= CEE_BLT_UN) {
		opcode = reverse_map [opcode - CEE_BEQ];
	} else if (opcode >= OP_FBEQ && opcode <= OP_FBLT_UN) {
		opcode = reverse_fmap [opcode - OP_FBEQ];
	} else if (opcode >= OP_LBEQ && opcode <= OP_LBLT_UN) {
		opcode = reverse_lmap [opcode - OP_LBEQ];
	} else if (opcode >= OP_IBEQ && opcode <= OP_IBLT_UN) {
		opcode = reverse_imap [opcode - OP_IBEQ];
	} else
		g_assert_not_reached ();

	return opcode;
}

guint
mono_type_to_ldind (MonoType *type)
{
	if (type->byref)
		return CEE_LDIND_I;

handle_enum:
	switch (type->type) {
	case MONO_TYPE_I1:
		return CEE_LDIND_I1;
	case MONO_TYPE_U1:
	case MONO_TYPE_BOOLEAN:
		return CEE_LDIND_U1;
	case MONO_TYPE_I2:
		return CEE_LDIND_I2;
	case MONO_TYPE_U2:
	case MONO_TYPE_CHAR:
		return CEE_LDIND_U2;
	case MONO_TYPE_I4:
		return CEE_LDIND_I4;
	case MONO_TYPE_U4:
		return CEE_LDIND_U4;
	case MONO_TYPE_I:
	case MONO_TYPE_U:
	case MONO_TYPE_PTR:
	case MONO_TYPE_FNPTR:
		return CEE_LDIND_I;
	case MONO_TYPE_CLASS:
	case MONO_TYPE_STRING:
	case MONO_TYPE_OBJECT:
	case MONO_TYPE_SZARRAY:
	case MONO_TYPE_ARRAY:    
		return CEE_LDIND_REF;
	case MONO_TYPE_I8:
	case MONO_TYPE_U8:
		return CEE_LDIND_I8;
	case MONO_TYPE_R4:
		return CEE_LDIND_R4;
	case MONO_TYPE_R8:
		return CEE_LDIND_R8;
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
		g_error ("unknown type 0x%02x in type_to_ldind", type->type);
	}
	return -1;
}

guint
mono_type_to_stind (MonoType *type)
{
	if (type->byref)
		return CEE_STIND_I;

handle_enum:
	switch (type->type) {
	case MONO_TYPE_I1:
	case MONO_TYPE_U1:
	case MONO_TYPE_BOOLEAN:
		return CEE_STIND_I1;
	case MONO_TYPE_I2:
	case MONO_TYPE_U2:
	case MONO_TYPE_CHAR:
		return CEE_STIND_I2;
	case MONO_TYPE_I4:
	case MONO_TYPE_U4:
		return CEE_STIND_I4;
	case MONO_TYPE_I:
	case MONO_TYPE_U:
	case MONO_TYPE_PTR:
	case MONO_TYPE_FNPTR:
		return CEE_STIND_I;
	case MONO_TYPE_CLASS:
	case MONO_TYPE_STRING:
	case MONO_TYPE_OBJECT:
	case MONO_TYPE_SZARRAY:
	case MONO_TYPE_ARRAY:    
		return CEE_STIND_REF;
	case MONO_TYPE_I8:
	case MONO_TYPE_U8:
		return CEE_STIND_I8;
	case MONO_TYPE_R4:
		return CEE_STIND_R4;
	case MONO_TYPE_R8:
		return CEE_STIND_R8;
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
		g_error ("unknown type 0x%02x in type_to_stind", type->type);
	}
	return -1;
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
	0, 0, OP_LADD-CEE_ADD, OP_PADD-CEE_ADD, OP_FADD-CEE_ADD, OP_PADD-CEE_ADD
};

/* handles from CEE_NEG to CEE_CONV_U8 */
static const guint16
unops_op_map [STACK_MAX] = {
	0, 0, OP_LNEG-CEE_NEG, OP_PNEG-CEE_NEG, OP_FNEG-CEE_NEG, OP_PNEG-CEE_NEG
};

/* handles from CEE_CONV_U2 to CEE_SUB_OVF_UN */
static const guint16
ovfops_op_map [STACK_MAX] = {
	0, 0, OP_LCONV_TO_U2-CEE_CONV_U2, OP_PCONV_TO_U2-CEE_CONV_U2, OP_FCONV_TO_U2-CEE_CONV_U2, OP_PCONV_TO_U2-CEE_CONV_U2, OP_PCONV_TO_U2-CEE_CONV_U2
};

/* handles from CEE_CONV_OVF_I1_UN to CEE_CONV_OVF_U_UN */
static const guint16
ovf2ops_op_map [STACK_MAX] = {
	0, 0, OP_LCONV_TO_OVF_I1_UN-CEE_CONV_OVF_I1_UN, OP_PCONV_TO_OVF_I1_UN-CEE_CONV_OVF_I1_UN, OP_FCONV_TO_OVF_I1_UN-CEE_CONV_OVF_I1_UN, OP_PCONV_TO_OVF_I1_UN-CEE_CONV_OVF_I1_UN
};

/* handles from CEE_CONV_OVF_I1 to CEE_CONV_OVF_U8 */
static const guint16
ovf3ops_op_map [STACK_MAX] = {
	0, 0, OP_LCONV_TO_OVF_I1-CEE_CONV_OVF_I1, OP_PCONV_TO_OVF_I1-CEE_CONV_OVF_I1, OP_FCONV_TO_OVF_I1-CEE_CONV_OVF_I1, OP_PCONV_TO_OVF_I1-CEE_CONV_OVF_I1
};

/* handles from CEE_CEQ to CEE_CLT_UN */
static const guint16
ceqops_op_map [STACK_MAX] = {
	0, 0, OP_LCEQ-CEE_CEQ, OP_PCEQ-CEE_CEQ, OP_FCEQ-CEE_CEQ, OP_LCEQ-CEE_CEQ
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
type_from_op (MonoInst *ins) {
	switch (ins->opcode) {
	/* binops */
	case CEE_ADD:
	case CEE_SUB:
	case CEE_MUL:
	case CEE_DIV:
	case CEE_REM:
		/* FIXME: check unverifiable args for STACK_MP */
		ins->type = bin_num_table [ins->inst_i0->type] [ins->inst_i1->type];
		ins->opcode += binops_op_map [ins->type];
		return;
	case CEE_DIV_UN:
	case CEE_REM_UN:
	case CEE_AND:
	case CEE_OR:
	case CEE_XOR:
		ins->type = bin_int_table [ins->inst_i0->type] [ins->inst_i1->type];
		ins->opcode += binops_op_map [ins->type];
		return;
	case CEE_SHL:
	case CEE_SHR:
	case CEE_SHR_UN:
		ins->type = shift_table [ins->inst_i0->type] [ins->inst_i1->type];
		ins->opcode += binops_op_map [ins->type];
		return;
	case OP_COMPARE:
	case OP_LCOMPARE:
		/* FIXME: handle some specifics with ins->next->type */
		ins->type = bin_comp_table [ins->inst_i0->type] [ins->inst_i1->type] ? STACK_I4: STACK_INV;
		if ((ins->inst_i0->type == STACK_I8) || ((sizeof (gpointer) == 8) && ((ins->inst_i0->type == STACK_PTR) || (ins->inst_i0->type == STACK_OBJ) || (ins->inst_i0->type == STACK_MP))))
			ins->opcode = OP_LCOMPARE;
		return;
	case OP_CEQ:
	case OP_CGT:
	case OP_CGT_UN:
	case OP_CLT:
	case OP_CLT_UN:
		ins->type = bin_comp_table [ins->inst_i0->type] [ins->inst_i1->type] ? STACK_I4: STACK_INV;
		ins->opcode += ceqops_op_map [ins->inst_i0->type];
		return;
	/* unops */
	case CEE_NEG:
		ins->type = neg_table [ins->inst_i0->type];
		ins->opcode += unops_op_map [ins->type];
		return;
	case CEE_NOT:
		if (ins->inst_i0->type >= STACK_I4 && ins->inst_i0->type <= STACK_PTR)
			ins->type = ins->inst_i0->type;
		else
			ins->type = STACK_INV;
		ins->opcode += unops_op_map [ins->type];
		return;
	case CEE_CONV_I1:
	case CEE_CONV_I2:
	case CEE_CONV_I4:
	case CEE_CONV_U4:
		ins->type = STACK_I4;
		ins->opcode += unops_op_map [ins->inst_i0->type];
		return;
	case CEE_CONV_R_UN:
		ins->type = STACK_R8;
		switch (ins->inst_i0->type) {
		case STACK_I4:
		case STACK_PTR:
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
		ins->opcode += ovf3ops_op_map [ins->inst_i0->type];
		return;
	case CEE_CONV_OVF_I_UN:
	case CEE_CONV_OVF_U_UN:
		ins->type = STACK_PTR;
		ins->opcode += ovf2ops_op_map [ins->inst_i0->type];
		return;
	case CEE_CONV_OVF_I1_UN:
	case CEE_CONV_OVF_I2_UN:
	case CEE_CONV_OVF_I4_UN:
	case CEE_CONV_OVF_U1_UN:
	case CEE_CONV_OVF_U2_UN:
	case CEE_CONV_OVF_U4_UN:
		ins->type = STACK_I4;
		ins->opcode += ovf2ops_op_map [ins->inst_i0->type];
		return;
	case CEE_CONV_U:
		ins->type = STACK_PTR;
		switch (ins->inst_i0->type) {
		case STACK_I4:
		case STACK_PTR:
		case STACK_MP:
			break;
		case STACK_I8:
			ins->opcode = OP_LCONV_TO_U;
			break;
		case STACK_R8:
			ins->opcode = OP_FCONV_TO_U;
			break;
		}
		return;
	case CEE_CONV_I8:
	case CEE_CONV_U8:
		ins->type = STACK_I8;
		ins->opcode += unops_op_map [ins->inst_i0->type];
		return;
	case CEE_CONV_OVF_I8:
	case CEE_CONV_OVF_U8:
		ins->type = STACK_I8;
		ins->opcode += ovf3ops_op_map [ins->inst_i0->type];
		return;
	case CEE_CONV_OVF_U8_UN:
	case CEE_CONV_OVF_I8_UN:
		ins->type = STACK_I8;
		ins->opcode += ovf2ops_op_map [ins->inst_i0->type];
		return;
	case CEE_CONV_R4:
	case CEE_CONV_R8:
		ins->type = STACK_R8;
		ins->opcode += unops_op_map [ins->inst_i0->type];
		return;
	case CEE_CKFINITE:
		ins->type = STACK_R8;		
		return;
	case CEE_CONV_U2:
	case CEE_CONV_U1:
		ins->type = STACK_I4;
		ins->opcode += ovfops_op_map [ins->inst_i0->type];
		break;
	case CEE_CONV_I:
	case CEE_CONV_OVF_I:
	case CEE_CONV_OVF_U:
		ins->type = STACK_PTR;
		ins->opcode += ovfops_op_map [ins->inst_i0->type];
		return;
	case CEE_ADD_OVF:
	case CEE_ADD_OVF_UN:
	case CEE_MUL_OVF:
	case CEE_MUL_OVF_UN:
	case CEE_SUB_OVF:
	case CEE_SUB_OVF_UN:
		ins->type = bin_num_table [ins->inst_i0->type] [ins->inst_i1->type];
		ins->opcode += ovfops_op_map [ins->inst_i0->type];
		return;
	default:
		g_error ("opcode 0x%04x not handled in type from op", ins->opcode);
		break;
	}
}

static const char 
ldind_type [] = {
	STACK_I4, STACK_I4, STACK_I4, STACK_I4, STACK_I4, STACK_I4, STACK_I8, STACK_MP, STACK_R8, STACK_R8, STACK_OBJ
};

/* map ldelem.x to the matching ldind.x opcode */
static const guchar
ldelem_to_ldind [] = {
	CEE_LDIND_I1,
	CEE_LDIND_U1,
	CEE_LDIND_I2,
	CEE_LDIND_U2,
	CEE_LDIND_I4,
	CEE_LDIND_U4,
	CEE_LDIND_I8,
	CEE_LDIND_I,
	CEE_LDIND_R4,
	CEE_LDIND_R8,
	CEE_LDIND_REF
};

/* map stelem.x to the matching stind.x opcode */
static const guchar
stelem_to_stind [] = {
	CEE_STIND_I,
	CEE_STIND_I1,
	CEE_STIND_I2,
	CEE_STIND_I4,
	CEE_STIND_I8,
	CEE_STIND_R4,
	CEE_STIND_R8,
	CEE_STIND_REF
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

MonoInst*
mono_compile_create_var (MonoCompile *cfg, MonoType *type, int opcode)
{
	MonoInst *inst;
	int num = cfg->num_varinfo;

	if ((num + 1) >= cfg->varinfo_count) {
		cfg->varinfo_count = (cfg->varinfo_count + 2) * 2;
		cfg->varinfo = (MonoInst **)g_realloc (cfg->varinfo, sizeof (MonoInst*) * cfg->varinfo_count);
		cfg->vars = (MonoMethodVar **)g_realloc (cfg->vars, sizeof (MonoMethodVar*) * cfg->varinfo_count);      
	}

	/*g_print ("created temp %d of type 0x%x\n", num, type->type);*/
	mono_jit_stats.allocate_var++;

	MONO_INST_NEW (cfg, inst, opcode);
	inst->inst_c0 = num;
	inst->inst_vtype = type;
	inst->klass = mono_class_from_mono_type (type);
	/* if set to 1 the variable is native */
	inst->unused = 0;

	cfg->varinfo [num] = inst;

	cfg->vars [num] = mono_mempool_alloc0 (cfg->mempool, sizeof (MonoMethodVar));
	MONO_INIT_VARINFO (cfg->vars [num], num);

	cfg->num_varinfo++;
	//g_print ("created temp %d of type %s\n", num, mono_type_get_name (type));
	return inst;
}

/*
 * Transform a MonoInst into a load from the variable of index var_index.
 */
void
mono_compile_make_var_load (MonoCompile *cfg, MonoInst *dest, gssize var_index) {
	memset (dest, 0, sizeof (MonoInst));
	dest->ssa_op = MONO_SSA_LOAD;
	dest->inst_i0 = cfg->varinfo [var_index];
	dest->opcode = mono_type_to_ldind (dest->inst_i0->inst_vtype);
	type_to_eval_stack_type (dest->inst_i0->inst_vtype, dest);
	dest->klass = dest->inst_i0->klass;
}

/*
 * Create a MonoInst that is a load from the variable of index var_index.
 */
MonoInst*
mono_compile_create_var_load (MonoCompile *cfg, gssize var_index) {
	MonoInst *dest;
	NEW_TEMPLOAD (cfg,dest,var_index);
	return dest;
}

/*
 * Create a MonoInst that is a store of the given value into the variable of index var_index.
 */
MonoInst*
mono_compile_create_var_store (MonoCompile *cfg, gssize var_index, MonoInst *value) {
	MonoInst *dest;
	NEW_TEMPSTORE (cfg, dest, var_index, value);
	return dest;
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

MonoType*
mono_type_from_stack_type (MonoInst *ins) {
	return type_from_stack_type (ins);
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

void
mono_add_ins_to_end (MonoBasicBlock *bb, MonoInst *inst)
{
	MonoInst *prev;
	if (!bb->code) {
		MONO_ADD_INS (bb, inst);
		return;
	}
	switch (bb->last_ins->opcode) {
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
	case CEE_BR:
	case CEE_SWITCH:
		prev = bb->code;
		while (prev->next && prev->next != bb->last_ins)
			prev = prev->next;
		if (prev == bb->code) {
			if (bb->last_ins == bb->code) {
				inst->next = bb->code;
				bb->code = inst;
			} else {
				inst->next = prev->next;
				prev->next = inst;
			}
		} else {
			inst->next = bb->last_ins;
			prev->next = inst;
		}
		break;
	//	g_warning ("handle conditional jump in add_ins_to_end ()\n");
	default:
		MONO_ADD_INS (bb, inst);
		break;
	}
}

void
mono_add_varcopy_to_end (MonoCompile *cfg, MonoBasicBlock *bb, int src, int dest)
{
	MonoInst *inst, *load;

	NEW_TEMPLOAD (cfg, load, src);

	NEW_TEMPSTORE (cfg, inst, dest, load);
	if (inst->opcode == CEE_STOBJ) {
		NEW_TEMPLOADA (cfg, inst, dest);
		handle_stobj (cfg, bb, inst, load, NULL, inst->klass, TRUE, FALSE);
	} else {
		inst->cil_code = NULL;
		mono_add_ins_to_end (bb, inst);
	}
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
 * It is also used to handle items on the stack in store opcodes, since it is
 * possible that the variable to be stored into is already on the stack, in
 * which case its old value should be used.
 * A single joint point will use the same variables (stored in the array bb->out_stack or
 * bb->in_stack, if the basic block is before or after the joint point).
 */
static int
handle_stack_args (MonoCompile *cfg, MonoBasicBlock *bb, MonoInst **sp, int count) {
	int i, bindex;
	MonoBasicBlock *outb;
	MonoInst *inst, **locals;
	gboolean found;

	if (!count)
		return 0;
	if (cfg->verbose_level > 3)
		g_print ("%d item(s) on exit from B%d\n", count, bb->block_num);
	if (!bb->out_scount) {
		bb->out_scount = count;
		//g_print ("bblock %d has out:", bb->block_num);
		found = FALSE;
		for (i = 0; i < bb->out_count; ++i) {
			outb = bb->out_bb [i];
			/* exception handlers are linked, but they should not be considered for stack args */
			if (outb->flags & BB_EXCEPTION_HANDLER)
				continue;
			//g_print (" %d", outb->block_num);
			if (outb->in_stack) {
				found = TRUE;
				bb->out_stack = outb->in_stack;
				break;
			}
		}
		//g_print ("\n");
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
	for (i = 0; i < count; ++i) {
		/* add store ops at the end of the bb, before the branch */
		NEW_TEMPSTORE (cfg, inst, locals [i]->inst_c0, sp [i]);
		if (inst->opcode == CEE_STOBJ) {
			NEW_TEMPLOADA (cfg, inst, locals [i]->inst_c0);
			handle_stobj (cfg, bb, inst, sp [i], sp [i]->cil_code, inst->klass, TRUE, FALSE);
		} else {
			inst->cil_code = sp [i]->cil_code;
			mono_add_ins_to_end (bb, inst);
		}
		if (cfg->verbose_level > 3)
			g_print ("storing %d to temp %d\n", i, (int)locals [i]->inst_c0);
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
				/* 
				 * Instead of storing sp [i] to locals [i], we need to store
				 * locals [i] to <new locals>[i], since the sp [i] tree can't
				 * be shared between trees.
				 */
				for (i = 0; i < count; ++i)
					mono_add_varcopy_to_end (cfg, bb, locals [i]->inst_c0, outb->in_stack [i]->inst_c0);
				locals = outb->in_stack;
				found = TRUE;
				break;
			}
			bindex ++;
		}
	}
	
	return 0;
}

static int
ret_type_to_call_opcode (MonoType *type, int calli, int virt)
{
	if (type->byref)
		return calli? OP_CALL_REG: virt? CEE_CALLVIRT: CEE_CALL;

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
		return calli? OP_CALL_REG: virt? CEE_CALLVIRT: CEE_CALL;
	case MONO_TYPE_I:
	case MONO_TYPE_U:
	case MONO_TYPE_PTR:
	case MONO_TYPE_FNPTR:
		return calli? OP_CALL_REG: virt? CEE_CALLVIRT: CEE_CALL;
	case MONO_TYPE_CLASS:
	case MONO_TYPE_STRING:
	case MONO_TYPE_OBJECT:
	case MONO_TYPE_SZARRAY:
	case MONO_TYPE_ARRAY:    
		return calli? OP_CALL_REG: virt? CEE_CALLVIRT: CEE_CALL;
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

void
mono_create_jump_table (MonoCompile *cfg, MonoInst *label, MonoBasicBlock **bbs, int num_blocks)
{
	MonoJumpInfo *ji = mono_mempool_alloc (cfg->mempool, sizeof (MonoJumpInfo));
	MonoJumpInfoBBTable *table;

	table = mono_mempool_alloc (cfg->mempool, sizeof (MonoJumpInfoBBTable));
	table->table = bbs;
	table->table_size = num_blocks;
	
	ji->ip.label = label;
	ji->type = MONO_PATCH_INFO_SWITCH;
	ji->data.table = table;
	ji->next = cfg->patch_info;
	cfg->patch_info = ji;
}

/*
 * When we add a tree of instructions, we need to ensure the instructions currently
 * on the stack are executed before (like, if we load a value from a local).
 * We ensure this by saving the currently loaded values to temps and rewriting the
 * instructions to load the values.
 * This is not done for opcodes that terminate a basic block (because it's handled already
 * by handle_stack_args ()) and for opcodes that can't change values, like POP.
 */
static void
handle_loaded_temps (MonoCompile *cfg, MonoBasicBlock *bblock, MonoInst **stack, MonoInst **sp)
{
	MonoInst *load, *store, *temp, *ins;

	while (stack < sp) {
		ins = *stack;
		/* handle also other constants */
		if ((ins->opcode != OP_ICONST) &&
		    /* temps never get written to again, so we can safely avoid duplicating them */
		    !(ins->ssa_op == MONO_SSA_LOAD && ins->inst_i0->opcode == OP_LOCAL && ins->inst_i0->flags & MONO_INST_IS_TEMP)) {
			temp = mono_compile_create_var (cfg, type_from_stack_type (ins), OP_LOCAL);
			temp->flags |= MONO_INST_IS_TEMP;
			NEW_TEMPSTORE (cfg, store, temp->inst_c0, ins);
			store->cil_code = ins->cil_code;
			if (store->opcode == CEE_STOBJ) {
				NEW_TEMPLOADA (cfg, store, temp->inst_c0);
				handle_stobj (cfg, bblock, store, ins, ins->cil_code, temp->klass, FALSE, FALSE);
			} else
				MONO_ADD_INS (bblock, store);
			NEW_TEMPLOAD (cfg, load, temp->inst_c0);
			load->cil_code = ins->cil_code;
			*stack = load;
		}
		stack++;
	}
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

inline static int
mono_spill_call (MonoCompile *cfg, MonoBasicBlock *bblock, MonoCallInst *call, MonoMethodSignature *sig, gboolean ret_object, 
		 const guint8 *ip, gboolean to_end)
{
	MonoInst *temp, *store, *ins = (MonoInst*)call;
	MonoType *ret = sig->ret;

	if (!MONO_TYPE_IS_VOID (ret) || ret_object) {
		if (ret_object) {
			call->inst.type = STACK_OBJ;
			call->inst.opcode = CEE_CALL;
			temp = mono_compile_create_var (cfg, &mono_defaults.string_class->byval_arg, OP_LOCAL);
		} else {
			type_to_eval_stack_type (ret, ins);
			temp = mono_compile_create_var (cfg, ret, OP_LOCAL);
		}
		
		temp->flags |= MONO_INST_IS_TEMP;

		if (MONO_TYPE_ISSTRUCT (ret)) {
			MonoInst *loada, *dummy_store;

			/* 
			 * Emit a dummy store to the local holding the result so the
			 * liveness info remains correct.
			 */
			NEW_DUMMY_STORE (cfg, dummy_store, temp->inst_c0);
			if (to_end)
				mono_add_ins_to_end (bblock, dummy_store);
			else
				MONO_ADD_INS (bblock, dummy_store);

			/* we use this to allocate native sized structs */
			temp->unused = sig->pinvoke;

			NEW_TEMPLOADA (cfg, loada, temp->inst_c0);
			if (call->inst.opcode == OP_VCALL)
				ins->inst_left = loada;
			else
				ins->inst_right = loada; /* a virtual or indirect call */

			if (to_end)
				mono_add_ins_to_end (bblock, ins);
			else
				MONO_ADD_INS (bblock, ins);
		} else {
			NEW_TEMPSTORE (cfg, store, temp->inst_c0, ins);
			store->cil_code = ip;
			if (to_end)
				mono_add_ins_to_end (bblock, store);
			else
				MONO_ADD_INS (bblock, store);
		}
		return temp->inst_c0;
	} else {
		if (to_end)
			mono_add_ins_to_end (bblock, ins);
		else
			MONO_ADD_INS (bblock, ins);
		return -1;
	}
}

inline static MonoCallInst *
mono_emit_call_args (MonoCompile *cfg, MonoBasicBlock *bblock, MonoMethodSignature *sig, 
		     MonoInst **args, int calli, int virtual, const guint8 *ip, gboolean to_end)
{
	MonoCallInst *call;
	MonoInst *arg;

	MONO_INST_NEW_CALL (cfg, call, ret_type_to_call_opcode (sig->ret, calli, virtual));
	
	call->inst.cil_code = ip;
	call->args = args;
	call->signature = sig;
	call = mono_arch_call_opcode (cfg, bblock, call, virtual);
	type_to_eval_stack_type (sig->ret, &call->inst);
	
	for (arg = call->out_args; arg;) {
		MonoInst *narg = arg->next;
		arg->next = NULL;
		if (!arg->cil_code)
			arg->cil_code = ip;
		if (to_end)
			mono_add_ins_to_end (bblock, arg);
		else
			MONO_ADD_INS (bblock, arg);
		arg = narg;
	}
	return call;
}

inline static int
mono_emit_calli (MonoCompile *cfg, MonoBasicBlock *bblock, MonoMethodSignature *sig, 
		 MonoInst **args, MonoInst *addr, const guint8 *ip)
{
	MonoCallInst *call = mono_emit_call_args (cfg, bblock, sig, args, TRUE, FALSE, ip, FALSE);

	call->inst.inst_i0 = addr;

	return mono_spill_call (cfg, bblock, call, sig, FALSE, ip, FALSE);
}

static MonoCallInst*
mono_emit_method_call_full (MonoCompile *cfg, MonoBasicBlock *bblock, MonoMethod *method, MonoMethodSignature *sig,
		       MonoInst **args, const guint8 *ip, MonoInst *this, gboolean to_end)
{
	gboolean virtual = this != NULL;
	MonoCallInst *call;

	call = mono_emit_call_args (cfg, bblock, sig, args, FALSE, virtual, ip, to_end);

	if (this && sig->hasthis && 
	    (method->klass->marshalbyref || method->klass == mono_defaults.object_class) && 
	    !(method->flags & METHOD_ATTRIBUTE_VIRTUAL) && !MONO_CHECK_THIS (this)) {
		call->method = mono_marshal_get_remoting_invoke_with_check (method);
	} else {
		call->method = method;
	}
	call->inst.flags |= MONO_INST_HAS_METHOD;
	call->inst.inst_left = this;

	if (!virtual)
		mono_get_got_var (cfg);
	else if (call->method->klass->flags & TYPE_ATTRIBUTE_INTERFACE)
		/* Needed by the code generated in inssel.brg */
		mono_get_got_var (cfg);

	return call;
}

static MonoCallInst*
mono_emit_method_call (MonoCompile *cfg, MonoBasicBlock *bblock, MonoMethod *method, MonoMethodSignature *sig,
		       MonoInst **args, const guint8 *ip, MonoInst *this)
{
	return mono_emit_method_call_full (cfg, bblock, method, sig, args, ip, this, FALSE);
}

inline static int
mono_emit_method_call_spilled (MonoCompile *cfg, MonoBasicBlock *bblock, MonoMethod *method,  
		       MonoMethodSignature *signature, MonoInst **args, const guint8 *ip, MonoInst *this)
{
	MonoCallInst *call = mono_emit_method_call (cfg, bblock, method, signature, args, ip, this);

	return mono_spill_call (cfg, bblock, call, signature, method->string_ctor, ip, FALSE);
}

inline static int
mono_emit_method_call_spilled_full (MonoCompile *cfg, MonoBasicBlock *bblock, MonoMethod *method,  
		       MonoMethodSignature *signature, MonoInst **args, const guint8 *ip, MonoInst *this,
		       gboolean ret_object, gboolean to_end)
{
	MonoCallInst *call = mono_emit_method_call_full (cfg, bblock, method, signature, args, ip, this, to_end);

	return mono_spill_call (cfg, bblock, call, signature, ret_object, ip, to_end);
}

inline static int
mono_emit_native_call (MonoCompile *cfg, MonoBasicBlock *bblock, gconstpointer func, MonoMethodSignature *sig,
		       MonoInst **args, const guint8 *ip, gboolean ret_object, gboolean to_end)
{
	MonoCallInst *call;

	g_assert (sig);

	call = mono_emit_call_args (cfg, bblock, sig, args, FALSE, FALSE, ip, to_end);
	call->fptr = func;

	mono_get_got_var (cfg);

	return mono_spill_call (cfg, bblock, call, sig, ret_object, ip, to_end);
}

inline static int
mono_emit_jit_icall (MonoCompile *cfg, MonoBasicBlock *bblock, gconstpointer func, MonoInst **args, const guint8 *ip)
{
	MonoJitICallInfo *info = mono_find_jit_icall_by_addr (func);
	
	if (!info) {
		g_warning ("unregistered JIT ICall");
		g_assert_not_reached ();
	}

	mono_get_got_var (cfg);
	return mono_emit_native_call (cfg, bblock, mono_icall_get_wrapper (info), info->sig, args, ip, FALSE, FALSE);
}

static void
mono_emulate_opcode (MonoCompile *cfg, MonoInst *tree, MonoInst **iargs, MonoJitICallInfo *info)
{
	MonoInst *ins, *temp = NULL, *store, *load, *begin;
	MonoInst *last_arg = NULL;
	int nargs;
	MonoCallInst *call;

	//g_print ("emulating: ");
	//mono_print_tree_nl (tree);
	MONO_INST_NEW_CALL (cfg, call, ret_type_to_call_opcode (info->sig->ret, FALSE, FALSE));
	ins = (MonoInst*)call;
	
	call->inst.cil_code = tree->cil_code;
	call->args = iargs;
	call->signature = info->sig;

	call = mono_arch_call_opcode (cfg, cfg->cbb, call, FALSE);

	mono_get_got_var (cfg);

	if (!MONO_TYPE_IS_VOID (info->sig->ret)) {
		temp = mono_compile_create_var (cfg, info->sig->ret, OP_LOCAL);
		temp->flags |= MONO_INST_IS_TEMP;
		NEW_TEMPSTORE (cfg, store, temp->inst_c0, ins);
		store->cil_code = tree->cil_code;
	} else {
		store = ins;
	}

	nargs = info->sig->param_count + info->sig->hasthis;

	for (last_arg = call->out_args; last_arg && last_arg->next; last_arg = last_arg->next) ;

	if (nargs)
		last_arg->next = store;

	if (nargs)
		begin = call->out_args;
	else
		begin = store;

	if (cfg->prev_ins) {
		/* 
		 * This assumes that that in a tree, emulate_opcode is called for a
		 * node before it is called for its children. dec_foreach needs to
		 * take this into account.
		 */
		store->next = cfg->prev_ins->next;
		cfg->prev_ins->next = begin;
	} else {
		store->next = cfg->cbb->code;
		cfg->cbb->code = begin;
	}

	call->fptr = mono_icall_get_wrapper (info);

	if (!MONO_TYPE_IS_VOID (info->sig->ret)) {
		NEW_TEMPLOAD (cfg, load, temp->inst_c0);
		*tree = *load;
	}
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

	res->ret = &mono_defaults.int_class->byval_arg;

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

static void
handle_stobj (MonoCompile *cfg, MonoBasicBlock *bblock, MonoInst *dest, MonoInst *src, const unsigned char *ip, MonoClass *klass, gboolean to_end, gboolean native) {
	MonoInst *iargs [3];
	int n;
	int align = 0;
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

	if ((cfg->opt & MONO_OPT_INTRINS) && !to_end && n <= sizeof (gpointer) * 5) {
		MonoInst *inst;
		if (dest->opcode == OP_LDADDR) {
			/* Keep liveness info correct */
			NEW_DUMMY_STORE (cfg, inst, dest->inst_i0->inst_c0);
			MONO_ADD_INS (bblock, inst);
		}
		MONO_INST_NEW (cfg, inst, OP_MEMCPY);
		inst->inst_left = dest;
		inst->inst_right = src;
		inst->cil_code = ip;
		inst->unused = n;
		MONO_ADD_INS (bblock, inst);
		return;
	}
	iargs [0] = dest;
	iargs [1] = src;
	NEW_ICONST (cfg, iargs [2], n);

	memcpy_method = get_memcpy_method ();
	mono_emit_method_call_spilled_full (cfg, bblock, memcpy_method, memcpy_method->signature, iargs, ip, NULL, FALSE, to_end);
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
handle_initobj (MonoCompile *cfg, MonoBasicBlock *bblock, MonoInst *dest, const guchar *ip, MonoClass *klass, MonoInst **stack_start, MonoInst **sp)
{
	MonoInst *iargs [3];
	MonoInst *ins, *zero_int32;
	int n;
	MonoMethod *memset_method;

	NEW_ICONST (cfg, zero_int32, 0);

	mono_class_init (klass);
	n = mono_class_value_size (klass, NULL);
	MONO_INST_NEW (cfg, ins, 0);
	ins->cil_code = ip;
	ins->inst_left = dest;
	ins->inst_right = zero_int32;
	switch (n) {
	case 1:
		ins->opcode = CEE_STIND_I1;
		MONO_ADD_INS (bblock, ins);
		break;
	case 2:
		ins->opcode = CEE_STIND_I2;
		MONO_ADD_INS (bblock, ins);
		break;
	case 4:
		ins->opcode = CEE_STIND_I4;
		MONO_ADD_INS (bblock, ins);
		break;
	default:
		if (n <= sizeof (gpointer) * 5) {
			ins->opcode = OP_MEMSET;
			ins->inst_imm = 0;
			ins->unused = n;
			MONO_ADD_INS (bblock, ins);
			break;
		}
		memset_method = get_memset_method ();
		handle_loaded_temps (cfg, bblock, stack_start, sp);
		iargs [0] = dest;
		NEW_ICONST (cfg, iargs [1], 0);
		NEW_ICONST (cfg, iargs [2], n);
		mono_emit_method_call_spilled (cfg, bblock, memset_method, memset_method->signature, iargs, ip, NULL);
		break;
	}
}

static int
handle_alloc (MonoCompile *cfg, MonoBasicBlock *bblock, MonoClass *klass, gboolean for_box, const guchar *ip)
{
	MonoInst *iargs [2];
	void *alloc_ftn;

	if (cfg->opt & MONO_OPT_SHARED) {
		NEW_DOMAINCONST (cfg, iargs [0]);
		NEW_CLASSCONST (cfg, iargs [1], klass);

		alloc_ftn = mono_object_new;
	} else {
		MonoVTable *vtable = mono_class_vtable (cfg->domain, klass);
		gboolean pass_lw;
		
		alloc_ftn = mono_class_get_allocation_ftn (vtable, for_box, &pass_lw);
		if (pass_lw) {
			guint32 lw = vtable->klass->instance_size;
			lw = ((lw + (sizeof (gpointer) - 1)) & ~(sizeof (gpointer) - 1)) / sizeof (gpointer);
			NEW_ICONST (cfg, iargs [0], lw);
			NEW_VTABLECONST (cfg, iargs [1], vtable);
		}
		else
			NEW_VTABLECONST (cfg, iargs [0], vtable);
	}

	return mono_emit_jit_icall (cfg, bblock, alloc_ftn, iargs, ip);
}
	
static MonoInst *
handle_box (MonoCompile *cfg, MonoBasicBlock *bblock, MonoInst *val, const guchar *ip, MonoClass *klass)
{
	MonoInst *dest, *vtoffset, *add, *vstore;
	int temp;

	temp = handle_alloc (cfg, bblock, klass, TRUE, ip);
	NEW_TEMPLOAD (cfg, dest, temp);
	NEW_ICONST (cfg, vtoffset, sizeof (MonoObject));
	MONO_INST_NEW (cfg, add, OP_PADD);
	add->inst_left = dest;
	add->inst_right = vtoffset;
	add->cil_code = ip;
	add->klass = klass;
	MONO_INST_NEW (cfg, vstore, CEE_STIND_I);
	vstore->opcode = mono_type_to_stind (&klass->byval_arg);
	vstore->cil_code = ip;
	vstore->inst_left = add;
	vstore->inst_right = val;

	if (vstore->opcode == CEE_STOBJ) {
		handle_stobj (cfg, bblock, add, val, ip, klass, FALSE, FALSE);
	} else
		MONO_ADD_INS (bblock, vstore);

	NEW_TEMPLOAD (cfg, dest, temp);
	return dest;
}

static int
handle_array_new (MonoCompile *cfg, MonoBasicBlock *bblock, int rank, MonoInst **sp, unsigned char *ip)
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

	return mono_emit_native_call (cfg, bblock, mono_icall_get_wrapper (info), info->sig, sp, ip, TRUE, FALSE);
}

static void
mono_emit_load_got_addr (MonoCompile *cfg)
{
	MonoInst *load, *store, *dummy_use;
	MonoInst *get_got;

	if (!cfg->got_var || cfg->got_var_allocated)
		return;

	MONO_INST_NEW (cfg, get_got, OP_LOAD_GOTADDR);
	NEW_TEMPSTORE (cfg, store, cfg->got_var->inst_c0, get_got);

	/* Add it to the start of the first bblock */
	if (cfg->bb_entry->code) {
		store->next = cfg->bb_entry->code;
		cfg->bb_entry->code = store;
	}
	else
		MONO_ADD_INS (cfg->bb_entry, store);

	cfg->got_var_allocated = TRUE;

	/* 
	 * Add a dummy use to keep the got_var alive, since real uses might
	 * only be generated in the decompose or instruction selection phases.
	 * Add it to end_bblock, so the variable's lifetime covers the whole
	 * method.
	 */
	NEW_TEMPLOAD (cfg, load, cfg->got_var->inst_c0);
	NEW_DUMMY_USE (cfg, dummy_use, load);
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
mini_get_ldelema_ins (MonoCompile *cfg, MonoBasicBlock *bblock, MonoMethod *cmethod, MonoInst **sp, unsigned char *ip, gboolean is_set)
{
	int temp, rank;
	MonoInst *addr;
	MonoMethodSignature *esig;
	char icall_name [256];
	char *name;
	MonoJitICallInfo *info;

	rank = mono_method_signature (cmethod)->param_count - (is_set? 1: 0);

	if (rank == 2 && (cfg->opt & MONO_OPT_INTRINS)) {
#ifdef MONO_ARCH_EMULATE_MUL_DIV
		/* OP_LDELEMA2D depends on OP_LMUL */
#else
		MonoInst *indexes;
		NEW_GROUP (cfg, indexes, sp [1], sp [2]);
		MONO_INST_NEW (cfg, addr, OP_LDELEMA2D);
		addr->inst_left = sp [0];
		addr->inst_right = indexes;
		addr->cil_code = ip;
		addr->type = STACK_MP;
		addr->klass = cmethod->klass;
		return addr;
#endif
	}

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

	temp = mono_emit_native_call (cfg, bblock, mono_icall_get_wrapper (info), info->sig, sp, ip, FALSE, FALSE);
	cfg->flags |= MONO_CFG_HAS_VARARGS;

	NEW_TEMPLOAD (cfg, addr, temp);
	return addr;
}

static MonoJitICallInfo **emul_opcode_map = NULL;

static inline MonoJitICallInfo *
mono_find_jit_opcode_emulation (int opcode)
{
	if  (emul_opcode_map)
		return emul_opcode_map [opcode];
	else
		return NULL;
}

static MonoException*
mini_loader_error_to_exception (MonoLoaderError *error)
{
	MonoException *ex = NULL;

	switch (error->kind) {
	case MONO_LOADER_ERROR_TYPE: {
		MonoString *class_name = mono_string_new (mono_domain_get (), error->class_name);
		
		ex = mono_get_exception_type_load (class_name, error->assembly_name);
		break;
	}
	case MONO_LOADER_ERROR_METHOD:
	case MONO_LOADER_ERROR_FIELD: {
		char *class_name;
		
		class_name = g_strdup_printf ("%s%s%s", error->klass->name_space, *error->klass->name_space ? "." : "", error->klass->name);

		if (error->kind == MONO_LOADER_ERROR_METHOD)
			ex = mono_get_exception_missing_method (class_name, error->member_name);
		else
			ex = mono_get_exception_missing_field (class_name, error->member_name);
		g_free (class_name);
		break;
	}
	default:
		g_assert_not_reached ();
	}

	return ex;
}

static MonoInst*
mini_get_inst_for_method (MonoCompile *cfg, MonoMethod *cmethod, MonoMethodSignature *fsig, MonoInst **args)
{
	MonoInst *ins = NULL;
	
	static MonoClass *runtime_helpers_class = NULL;
	if (! runtime_helpers_class)
		runtime_helpers_class = mono_class_from_name (mono_defaults.corlib,
			"System.Runtime.CompilerServices", "RuntimeHelpers");

	if (cmethod->klass == mono_defaults.string_class) {
 		if (cmethod->name [0] != 'g')
 			return NULL;
 
		if (strcmp (cmethod->name, "get_Chars") == 0) {
 			MONO_INST_NEW (cfg, ins, OP_GETCHR);
			ins->inst_i0 = args [0];
			ins->inst_i1 = args [1];
			return ins;
		} else if (strcmp (cmethod->name, "get_Length") == 0) {
 			MONO_INST_NEW (cfg, ins, OP_STRLEN);
			ins->inst_i0 = args [0];
			return ins;
		} else 
			return NULL;
	} else if (cmethod->klass == mono_defaults.object_class) {
		if (strcmp (cmethod->name, "GetType") == 0) {
 			MONO_INST_NEW (cfg, ins, OP_GETTYPE);
			ins->inst_i0 = args [0];
			return ins;
		} else if (strcmp (cmethod->name, "InternalGetHashCode") == 0) {
#ifdef MONO_ARCH_EMULATE_MUL_DIV
		/* The OP_GETHASHCODE rule depends on OP_MUL */
#else
 			MONO_INST_NEW (cfg, ins, OP_GETHASHCODE);
			ins->inst_i0 = args [0];
			return ins;
#endif
		} else if (strcmp (cmethod->name, ".ctor") == 0) {
 			MONO_INST_NEW (cfg, ins, CEE_NOP);
			return ins;
		} else
			return NULL;
	} else if (mini_class_is_system_array (cmethod->klass)) {
 		if (cmethod->name [0] != 'g')
 			return NULL;

		if (strcmp (cmethod->name, "get_Rank") == 0) {
 			MONO_INST_NEW (cfg, ins, OP_ARRAY_RANK);
			ins->inst_i0 = args [0];
			return ins;
		} else if (strcmp (cmethod->name, "get_Length") == 0) {
 			MONO_INST_NEW (cfg, ins, CEE_LDLEN);
			ins->inst_i0 = args [0];
			return ins;
		} else
			return NULL;
	} else if (cmethod->klass == runtime_helpers_class) {
		if (strcmp (cmethod->name, "get_OffsetToStringData") == 0) {
			NEW_ICONST (cfg, ins, G_STRUCT_OFFSET (MonoString, chars));
			return ins;
		} else
			return NULL;
	} else if (cmethod->klass == mono_defaults.thread_class) {
		if (strcmp (cmethod->name, "get_CurrentThread") == 0 && (ins = mono_arch_get_thread_intrinsic (cfg)))
			return ins;
	}

	return mono_arch_get_inst_for_method (cfg, cmethod, fsig, args);
}

static void
mono_save_args (MonoCompile *cfg, MonoBasicBlock *bblock, MonoMethodSignature *sig, MonoInst **sp, MonoInst **args)
{
	MonoInst *store, *temp;
	int i;

	g_assert (!MONO_TYPE_ISSTRUCT (sig->ret));

	if (!sig->hasthis && sig->param_count == 0) 
		return;

	if (sig->hasthis) {
		if (sp [0]->opcode == OP_ICONST) {
			*args++ = sp [0];
		} else {
			temp = mono_compile_create_var (cfg, type_from_stack_type (*sp), OP_LOCAL);
			*args++ = temp;
			NEW_TEMPSTORE (cfg, store, temp->inst_c0, *sp);
			store->cil_code = sp [0]->cil_code;
			MONO_ADD_INS (bblock, store);
		}
		sp++;
	}

	for (i = 0; i < sig->param_count; ++i) {
		if (sp [0]->opcode == OP_ICONST) {
			*args++ = sp [0];
		} else {
			temp = mono_compile_create_var (cfg, sig->params [i], OP_LOCAL);
			*args++ = temp;
			NEW_TEMPSTORE (cfg, store, temp->inst_c0, *sp);
			store->cil_code = sp [0]->cil_code;
			if (store->opcode == CEE_STOBJ) {
				NEW_TEMPLOADA (cfg, store, temp->inst_c0);
				handle_stobj (cfg, bblock, store, *sp, sp [0]->cil_code, temp->klass, FALSE, FALSE);
			} else {
				MONO_ADD_INS (bblock, store);
			} 
		}
		sp++;
	}
}

static int
inline_method (MonoCompile *cfg, MonoMethod *cmethod, MonoMethodSignature *fsig, MonoBasicBlock *bblock, MonoInst **sp,
		guchar *ip, guint real_offset, GList *dont_inline, MonoBasicBlock **last_b, gboolean inline_allways)
{
	MonoInst *ins, *rvar = NULL;
	MonoMethodHeader *cheader;
	MonoBasicBlock *ebblock, *sbblock;
	int i, costs, new_locals_offset;
	MonoMethod *prev_inlined_method;

	if (cfg->verbose_level > 2)
		g_print ("INLINE START %p %s -> %s\n", cmethod,  mono_method_full_name (cfg->method, TRUE), mono_method_full_name (cmethod, TRUE));

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
	
	/* allocate starte and end blocks */
	sbblock = NEW_BBLOCK (cfg);
	sbblock->block_num = cfg->num_bblocks++;
	sbblock->real_offset = real_offset;

	ebblock = NEW_BBLOCK (cfg);
	ebblock->block_num = cfg->num_bblocks++;
	ebblock->real_offset = real_offset;

	prev_inlined_method = cfg->inlined_method;
	cfg->inlined_method = cmethod;

	costs = mono_method_to_ir (cfg, cmethod, sbblock, ebblock, new_locals_offset, rvar, dont_inline, sp, real_offset, *ip == CEE_CALLVIRT);

	cfg->inlined_method = prev_inlined_method;

	if ((costs >= 0 && costs < 60) || inline_allways) {
		if (cfg->verbose_level > 2)
			g_print ("INLINE END %s -> %s\n", mono_method_full_name (cfg->method, TRUE), mono_method_full_name (cmethod, TRUE));
		
		mono_jit_stats.inlined_methods++;

		/* always add some code to avoid block split failures */
		MONO_INST_NEW (cfg, ins, CEE_NOP);
		MONO_ADD_INS (bblock, ins);
		ins->cil_code = ip;

		bblock->next_bb = sbblock;
		link_bblock (cfg, bblock, sbblock);

		if (rvar) {
			NEW_TEMPLOAD (cfg, ins, rvar->inst_c0);
			*sp++ = ins;
		}
		*last_b = ebblock;
		return costs + 1;
	} else {
		if (cfg->verbose_level > 2)
			g_print ("INLINE ABORTED %s\n", mono_method_full_name (cmethod, TRUE));
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

static MonoInst*
emit_tree (MonoCompile *cfg, MonoBasicBlock *bblock, MonoInst *ins, const guint8* ip_next)
{
	MonoInst *store, *temp, *load;
	
	if (ip_in_bb (cfg, bblock, ip_next) &&
		(CODE_IS_STLOC (ip_next) || *ip_next == CEE_RET))
			return ins;
	
	temp = mono_compile_create_var (cfg, type_from_stack_type (ins), OP_LOCAL);
	temp->flags |= MONO_INST_IS_TEMP;
	NEW_TEMPSTORE (cfg, store, temp->inst_c0, ins);
	store->cil_code = ins->cil_code;
	MONO_ADD_INS (bblock, store);
	NEW_TEMPLOAD (cfg, load, temp->inst_c0);
	load->cil_code = ins->cil_code;
	return load;
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
void check_linkdemand (MonoCompile *cfg, MonoMethod *caller, MonoMethod *callee, MonoBasicBlock *bblock, unsigned char *ip)
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
		mono_emit_method_call_spilled (cfg, bblock, secman->linkdemandsecurityexception, mono_method_signature (secman->linkdemandsecurityexception), args, ip, NULL);
	} else if (cfg->exception_type == MONO_EXCEPTION_NONE) {
		 /* don't hide previous results */
		cfg->exception_type = MONO_EXCEPTION_SECURITY_LINKDEMAND;
		cfg->exception_data = result;
	}
}


/*
 * mono_method_to_ir: translates IL into basic blocks containing trees
 */
int
mono_method_to_ir2 (MonoCompile *cfg, MonoMethod *method, MonoBasicBlock *start_bblock, MonoBasicBlock *end_bblock, 
		   int locals_offset, MonoInst *return_var, GList *dont_inline, MonoInst **inline_args, 
		   guint inline_offset, gboolean is_virtual_call)
{
	MonoInst *zero_int32, *zero_int64, *zero_ptr, *zero_obj, *zero_r8;
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
	int i, n, start_new_bblock, align;
	int num_calls = 0, inline_costs = 0;
	int breakpoint_id = 0;
	guint real_offset, num_args;
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
		real_offset = 0;
		bbhash = cfg->bb_hash;
	} else {
		real_offset = inline_offset;
		bbhash = g_hash_table_new (g_direct_hash, NULL);
	}

	if (cfg->verbose_level > 2)
		g_print ("method to IR %s\n", mono_method_full_name (method, TRUE));

	dont_inline = g_list_prepend (dont_inline, method);
	if (cfg->method == method) {

		if (cfg->method->save_lmf)
			/* Needed by the prolog code */
			mono_get_got_var (cfg);

		if (cfg->prof_options & MONO_PROFILE_INS_COVERAGE)
			cfg->coverage_info = mono_profiler_coverage_alloc (cfg->method, header->code_size);

		/* ENTRY BLOCK */
		cfg->bb_entry = start_bblock = NEW_BBLOCK (cfg);
		start_bblock->cil_code = NULL;
		start_bblock->cil_length = 0;
		start_bblock->block_num = cfg->num_bblocks++;

		/* EXIT BLOCK */
		cfg->bb_exit = end_bblock = NEW_BBLOCK (cfg);
		end_bblock->cil_code = NULL;
		end_bblock->cil_length = 0;
		end_bblock->block_num = cfg->num_bblocks++;
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


			/*g_print ("clause try IL_%04x to IL_%04x handler %d at IL_%04x to IL_%04x\n", clause->try_offset, clause->try_offset + clause->try_len, clause->flags, clause->handler_offset, clause->handler_offset + clause->handler_len);
			  while (p < end) {
			  g_print ("%s", mono_disasm_code_one (NULL, method, p, &p));
			  }*/
			/* catch and filter blocks get the exception object on the stack */
			if (clause->flags == MONO_EXCEPTION_CLAUSE_NONE ||
			    clause->flags == MONO_EXCEPTION_CLAUSE_FILTER) {
				MonoInst *load, *dummy_use;

				/* mostly like handle_stack_args (), but just sets the input args */
				/* g_print ("handling clause at IL_%04x\n", clause->handler_offset); */
				tblock->in_scount = 1;
				tblock->in_stack = mono_mempool_alloc (cfg->mempool, sizeof (MonoInst*));
				tblock->in_stack [0] = mono_create_exvar_for_offset (cfg, clause->handler_offset);

				/* 
				 * Add a dummy use for the exvar so its liveness info will be
				 * correct.
				 */
				NEW_TEMPLOAD (cfg, load, tblock->in_stack [0]->inst_c0);
				NEW_DUMMY_USE (cfg, dummy_use, load);
				MONO_ADD_INS (tblock, dummy_use);
				
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
		mono_save_args (cfg, start_bblock, sig, inline_args, arg_array);
	}

	/* FIRST CODE BLOCK */
	bblock = NEW_BBLOCK (cfg);
	bblock->cil_code = ip;

	ADD_BBLOCK (cfg, bbhash, bblock);

	if (cfg->method == method) {
		breakpoint_id = mono_debugger_method_has_breakpoint (method);
		if (breakpoint_id && (mono_debug_format != MONO_DEBUG_FORMAT_DEBUGGER)) {
			MONO_INST_NEW (cfg, ins, CEE_BREAK);
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
		cfg->bb_init = init_localsbb = NEW_BBLOCK (cfg);
		init_localsbb->real_offset = real_offset;
		start_bblock->next_bb = init_localsbb;
		init_localsbb->next_bb = bblock;
		link_bblock (cfg, start_bblock, init_localsbb);
		link_bblock (cfg, init_localsbb, bblock);
		init_localsbb->block_num = cfg->num_bblocks++;
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
			mono_emit_method_call_spilled (cfg, init_localsbb, secman->demand, mono_method_signature (secman->demand), args, ip, NULL);
		}
		if (actions.noncasdemand.blob) {
			/* CLR 1.x uses a .noncasdemand (but 2.x doesn't) */
			/* For Mono we re-route non-CAS Demand to Demand (as the managed code must deal with it anyway) */
			NEW_DECLSECCONST (cfg, args[0], image, actions.noncasdemand);
			NEW_ICONST (cfg, args [1], actions.noncasdemand.size);
			/* Calls static void SecurityManager.InternalDemand (byte* permissions, int size); */
			mono_emit_method_call_spilled (cfg, init_localsbb, secman->demand, mono_method_signature (secman->demand), args, ip, NULL);
		}
		if (actions.demandchoice.blob) {
			/* New in 2.0, Demand must succeed for one of the permissions (i.e. not all) */
			NEW_DECLSECCONST (cfg, args[0], image, actions.demandchoice);
			NEW_ICONST (cfg, args [1], actions.demandchoice.size);
			/* Calls static void SecurityManager.InternalDemandChoice (byte* permissions, int size); */
			mono_emit_method_call_spilled (cfg, init_localsbb, secman->demandchoice, mono_method_signature (secman->demandchoice), args, ip, NULL);
		}
	}

	/* we must Demand SecurityPermission.Unmanaged before p/invoking */
	if (pinvoke) {
		mono_emit_method_call_spilled (cfg, init_localsbb, secman->demandunmanaged, mono_method_signature (secman->demandunmanaged), NULL, ip, NULL);
	}

	if (get_basic_blocks (cfg, bbhash, header, real_offset, ip, end, &err_pos)) {
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

	/* do this somewhere outside - not here */
	NEW_ICONST (cfg, zero_int32, 0);
	NEW_ICONST (cfg, zero_int64, 0);
	zero_int64->type = STACK_I8;
	NEW_PCONST (cfg, zero_ptr, 0);
	NEW_PCONST (cfg, zero_obj, 0);
	zero_obj->type = STACK_OBJ;

	MONO_INST_NEW (cfg, zero_r8, OP_R8CONST);
	zero_r8->type = STACK_R8;
	zero_r8->inst_p0 = &r8_0;

	/* add a check for this != NULL to inlined methods */
	if (is_virtual_call) {
		MONO_INST_NEW (cfg, ins, OP_CHECK_THIS);
		NEW_ARGLOAD (cfg, ins->inst_left, 0);
		ins->cil_code = ip;
		MONO_ADD_INS (bblock, ins);
	}

	/* we use a spare stack slot in SWITCH and NEWOBJ and others */
	stack_start = sp = mono_mempool_alloc0 (cfg->mempool, sizeof (MonoInst*) * (header->max_stack + 1));

	ins_flag = 0;
	start_new_bblock = 0;
	while (ip < end) {

		if (cfg->method == method)
			real_offset = ip - header->code;
		else
			real_offset = inline_offset;

		if (start_new_bblock) {
			bblock->cil_length = ip - bblock->cil_code;
			if (start_new_bblock == 2) {
				g_assert (ip == tblock->cil_code);
			} else {
				GET_BBLOCK (cfg, bbhash, tblock, ip);
			}
			bblock->next_bb = tblock;
			bblock = tblock;
			start_new_bblock = 0;
			for (i = 0; i < bblock->in_scount; ++i) {
				if (cfg->verbose_level > 3)
					g_print ("loading %d from temp %d\n", i, (int)bblock->in_stack [i]->inst_c0);						
				NEW_TEMPLOAD (cfg, ins, bblock->in_stack [i]->inst_c0);
				*sp++ = ins;
			}
			g_slist_free (class_inits);
			class_inits = NULL;
		} else {
			if ((tblock = g_hash_table_lookup (bbhash, ip)) && (tblock != bblock)) {
				link_bblock (cfg, bblock, tblock);
				if (sp != stack_start) {
					handle_stack_args (cfg, bblock, stack_start, sp - stack_start);
					sp = stack_start;
				}
				bblock->next_bb = tblock;
				bblock = tblock;
				for (i = 0; i < bblock->in_scount; ++i) {
					if (cfg->verbose_level > 3)
						g_print ("loading %d from temp %d\n", i, (int)bblock->in_stack [i]->inst_c0);						
					NEW_TEMPLOAD (cfg, ins, bblock->in_stack [i]->inst_c0);
					*sp++ = ins;
				}
				g_slist_free (class_inits);
				class_inits = NULL;
			}
		}

		bblock->real_offset = real_offset;

		if ((cfg->method == method) && cfg->coverage_info) {
			MonoInst *store, *one;
			guint32 cil_offset = ip - header->code;
			cfg->coverage_info->data [cil_offset].cil_code = ip;

			/* TODO: Use an increment here */
			NEW_ICONST (cfg, one, 1);
			one->cil_code = ip;

			NEW_PCONST (cfg, ins, &(cfg->coverage_info->data [cil_offset].count));
			ins->cil_code = ip;

			MONO_INST_NEW (cfg, store, CEE_STIND_I);
			store->cil_code = ip;
			store->inst_left = ins;
			store->inst_right = one;

			MONO_ADD_INS (bblock, store);
		}

		if (cfg->verbose_level > 3)
			g_print ("converting (in B%d: stack: %d) %s", bblock->block_num, (int)(sp - stack_start), mono_disasm_code_one (NULL, method, ip, NULL));

		switch (*ip) {
		case CEE_NOP:
		case CEE_BREAK:
			MONO_INST_NEW (cfg, ins, *ip);
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
			*sp++ = ins;
			break;
		case CEE_STLOC_0:
		case CEE_STLOC_1:
		case CEE_STLOC_2:
		case CEE_STLOC_3:
			CHECK_STACK (1);
			n = (*ip)-CEE_STLOC_0;
			CHECK_LOCAL (n);
			--sp;
			handle_loaded_temps (cfg, bblock, stack_start, sp);
			NEW_LOCSTORE (cfg, ins, n, *sp);
			ins->cil_code = ip;
			if (ins->opcode == CEE_STOBJ) {
				NEW_LOCLOADA (cfg, ins, n);
				handle_stobj (cfg, bblock, ins, *sp, ip, ins->klass, FALSE, FALSE);
			} else
				MONO_ADD_INS (bblock, ins);
			++ip;
			inline_costs += 1;
			break;
		case CEE_LDARG_S:
			CHECK_OPSIZE (2);
			CHECK_STACK_OVF (1);
			CHECK_ARG (ip [1]);
			NEW_ARGLOAD (cfg, ins, ip [1]);
			ins->cil_code = ip;
			*sp++ = ins;
			ip += 2;
			break;
		case CEE_LDARGA_S:
			CHECK_OPSIZE (2);
			CHECK_STACK_OVF (1);
			CHECK_ARG (ip [1]);
			NEW_ARGLOADA (cfg, ins, ip [1]);
			ins->cil_code = ip;
			*sp++ = ins;
			ip += 2;
			break;
		case CEE_STARG_S:
			CHECK_OPSIZE (2);
			CHECK_STACK (1);
			--sp;
			CHECK_ARG (ip [1]);
			NEW_ARGSTORE (cfg, ins, ip [1], *sp);
			handle_loaded_temps (cfg, bblock, stack_start, sp);
			ins->cil_code = ip;
			if (ins->opcode == CEE_STOBJ) {
				NEW_ARGLOADA (cfg, ins, ip [1]);
				handle_stobj (cfg, bblock, ins, *sp, ip, ins->klass, FALSE, FALSE);
			} else
				MONO_ADD_INS (bblock, ins);
			ip += 2;
			break;
		case CEE_LDLOC_S:
			CHECK_OPSIZE (2);
			CHECK_STACK_OVF (1);
			CHECK_LOCAL (ip [1]);
			NEW_LOCLOAD (cfg, ins, ip [1]);
			ins->cil_code = ip;
			*sp++ = ins;
			ip += 2;
			break;
		case CEE_LDLOCA_S:
			CHECK_OPSIZE (2);
			CHECK_STACK_OVF (1);
			CHECK_LOCAL (ip [1]);
			NEW_LOCLOADA (cfg, ins, ip [1]);
			ins->cil_code = ip;
			*sp++ = ins;
			ip += 2;
			break;
		case CEE_STLOC_S:
			CHECK_OPSIZE (2);
			CHECK_STACK (1);
			--sp;
			handle_loaded_temps (cfg, bblock, stack_start, sp);
			CHECK_LOCAL (ip [1]);
			NEW_LOCSTORE (cfg, ins, ip [1], *sp);
			ins->cil_code = ip;
			if (ins->opcode == CEE_STOBJ) {
				NEW_LOCLOADA (cfg, ins, ip [1]);
				handle_stobj (cfg, bblock, ins, *sp, ip, ins->klass, FALSE, FALSE);
			} else
				MONO_ADD_INS (bblock, ins);
			ip += 2;
			inline_costs += 1;
			break;
		case CEE_LDNULL:
			CHECK_STACK_OVF (1);
			NEW_PCONST (cfg, ins, NULL);
			ins->cil_code = ip;
			ins->type = STACK_OBJ;
			++ip;
			*sp++ = ins;
			break;
		case CEE_LDC_I4_M1:
			CHECK_STACK_OVF (1);
			NEW_ICONST (cfg, ins, -1);
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
			NEW_ICONST (cfg, ins, (*ip) - CEE_LDC_I4_0);
			ins->cil_code = ip;
			++ip;
			*sp++ = ins;
			break;
		case CEE_LDC_I4_S:
			CHECK_OPSIZE (2);
			CHECK_STACK_OVF (1);
			++ip;
			NEW_ICONST (cfg, ins, *((signed char*)ip));
			ins->cil_code = ip;
			++ip;
			*sp++ = ins;
			break;
		case CEE_LDC_I4:
			CHECK_OPSIZE (5);
			CHECK_STACK_OVF (1);
			NEW_ICONST (cfg, ins, (gint32)read32 (ip + 1));
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
			++ip;
			ins->inst_l = (gint64)read64 (ip);
			ip += 8;
			*sp++ = ins;
			break;
		case CEE_LDC_R4: {
			float *f;
			/* we should really allocate this only late in the compilation process */
			mono_domain_lock (cfg->domain);
			f = mono_mempool_alloc (cfg->domain->mp, sizeof (float));
			mono_domain_unlock (cfg->domain);
			CHECK_OPSIZE (5);
			CHECK_STACK_OVF (1);
			MONO_INST_NEW (cfg, ins, OP_R4CONST);
			ins->type = STACK_R8;
			++ip;
			readr4 (ip, f);
			ins->inst_p0 = f;

			ip += 4;
			*sp++ = ins;			
			break;
		}
		case CEE_LDC_R8: {
			double *d;
			mono_domain_lock (cfg->domain);
			d = mono_mempool_alloc (cfg->domain->mp, sizeof (double));
			mono_domain_unlock (cfg->domain);
			CHECK_OPSIZE (9);
			CHECK_STACK_OVF (1);
			MONO_INST_NEW (cfg, ins, OP_R8CONST);
			ins->type = STACK_R8;
			++ip;
			readr8 (ip, d);
			ins->inst_p0 = d;

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
		
			/* 
			 * small optimization: if the loaded value was from a local already,
			 * just load it twice.
			 */
			if (ins->ssa_op == MONO_SSA_LOAD && 
			    (ins->inst_i0->opcode == OP_LOCAL || ins->inst_i0->opcode == OP_ARG)) {
				sp++;
				MONO_INST_NEW (cfg, temp, 0);
				*temp = *ins;
				temp->cil_code = ip;
				*sp++ = temp;
			} else {
				temp = mono_compile_create_var (cfg, type_from_stack_type (ins), OP_LOCAL);
				temp->flags |= MONO_INST_IS_TEMP;
				temp->cil_code = ip;
				NEW_TEMPSTORE (cfg, store, temp->inst_c0, ins);
				store->cil_code = ip;
				if (store->opcode == CEE_STOBJ) {
					NEW_TEMPLOADA (cfg, store, temp->inst_c0);
					handle_stobj (cfg, bblock, store, sp [0], sp [0]->cil_code, store->klass, TRUE, FALSE);
				} else {
					MONO_ADD_INS (bblock, store);
				}
				NEW_TEMPLOAD (cfg, ins, temp->inst_c0);
				*sp++ = ins;
				ins->cil_code = ip;
				NEW_TEMPLOAD (cfg, ins, temp->inst_c0);
				*sp++ = ins;
				ins->cil_code = ip;
			}
			++ip;
			inline_costs += 2;
			break;
		}
		case CEE_POP:
			CHECK_STACK (1);
			MONO_INST_NEW (cfg, ins, CEE_POP);
			MONO_ADD_INS (bblock, ins);
			ins->cil_code = ip++;
			--sp;
			ins->inst_i0 = *sp;
			break;
		case CEE_JMP:
			CHECK_OPSIZE (5);
			if (stack_start != sp)
				goto unverified;
			MONO_INST_NEW (cfg, ins, CEE_JMP);
			token = read32 (ip + 1);
			/* FIXME: check the signature matches */
			cmethod = mini_get_method (method, token, NULL, generic_context);

			if (!cmethod)
				goto load_error;

			if (mono_use_security_manager) {
				check_linkdemand (cfg, method, cmethod, bblock, ip);
			}

			ins->inst_p0 = cmethod;
			MONO_ADD_INS (bblock, ins);
			ip += 5;
			start_new_bblock = 1;
			break;
		case CEE_CALLI:
		case CEE_CALL:
		case CEE_CALLVIRT: {
			MonoInst *addr = NULL;
			MonoMethodSignature *fsig = NULL;
			int temp, array_rank = 0;
			int virtual = *ip == CEE_CALLVIRT;

			CHECK_OPSIZE (5);
			token = read32 (ip + 1);

			if (*ip == CEE_CALLI) {
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
					check_linkdemand (cfg, method, cmethod, bblock, ip);
				}

				if (cmethod->iflags & METHOD_IMPL_ATTRIBUTE_INTERNAL_CALL &&
				    mini_class_is_system_array (cmethod->klass)) {
					array_rank = cmethod->klass->rank;
				}

				if (cmethod->string_ctor)
					g_assert_not_reached ();

			}

			if (!virtual) {
				mono_get_got_var (cfg);
			} else {
				/* code in inssel.brg might transform a virtual call to a normal call */
				if (!(cmethod->flags & METHOD_ATTRIBUTE_VIRTUAL) || 
					((cmethod->flags & METHOD_ATTRIBUTE_FINAL) && 
					 cmethod->wrapper_type != MONO_WRAPPER_REMOTING_INVOKE_WITH_CHECK))
					mono_get_got_var (cfg);
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
					MonoInst *load;
					/*
					 * The type parameter is instantiated as a valuetype,
					 * but that type doesn't override the method we're
					 * calling, so we need to box `this'.
					 * sp [0] is a pointer to the data: we need the value
					 * in handle_box (), so load it here.
					 */
					MONO_INST_NEW (cfg, load, mono_type_to_ldind (&constrained_call->byval_arg));
					type_to_eval_stack_type (&constrained_call->byval_arg, load);
					load->cil_code = ip;
					load->inst_left = sp [0];
					sp [0] = handle_box (cfg, bblock, load, ip, constrained_call);
				} else if (!constrained_call->valuetype) {
					MonoInst *ins;

					/*
					 * The type parameter is instantiated as a reference
					 * type.  We have a managed pointer on the stack, so
					 * we need to dereference it here.
					 */

					MONO_INST_NEW (cfg, ins, CEE_LDIND_REF);
					ins->cil_code = ip;
					ins->inst_i0 = sp [0];
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
				this_temp->cil_code = ip;
				NEW_TEMPSTORE (cfg, store, this_temp->inst_c0, sp [0]);

				store->cil_code = ip;
				MONO_ADD_INS (bblock, store);

				NEW_TEMPLOAD (cfg, iargs [0], this_temp->inst_c0);
				NEW_PCONST (cfg, iargs [1], cmethod);
				NEW_PCONST (cfg, iargs [2], ((MonoMethodInflated *) cmethod)->context);
				temp = mono_emit_jit_icall (cfg, bblock, helper_compile_generic_method, iargs, ip);

				NEW_TEMPLOAD (cfg, addr, temp);
				NEW_TEMPLOAD (cfg, sp [0], this_temp->inst_c0);

				if ((temp = mono_emit_calli (cfg, bblock, fsig, sp, addr, ip)) != -1) {
					NEW_TEMPLOAD (cfg, *sp, temp);
					sp++;
				}

				ip += 5;
				break;
			}

			if ((ins_flag & MONO_INST_TAILCALL) && cmethod && (*ip == CEE_CALL) &&
				 (mono_metadata_signature_equal (mono_method_signature (method), mono_method_signature (cmethod)))) {
				int i;
				/* FIXME: This assumes the two methods has the same number and type of arguments */
				for (i = 0; i < n; ++i) {
					/* Check if argument is the same */
					NEW_ARGLOAD (cfg, ins, i);
					if ((ins->opcode == sp [i]->opcode) && (ins->inst_i0 == sp [i]->inst_i0))
						continue;

					/* Prevent argument from being register allocated */
					arg_array [i]->flags |= MONO_INST_VOLATILE;
					NEW_ARGSTORE (cfg, ins, i, sp [i]);
					ins->cil_code = ip;
					if (ins->opcode == CEE_STOBJ) {
						NEW_ARGLOADA (cfg, ins, i);
						handle_stobj (cfg, bblock, ins, sp [i], sp [i]->cil_code, ins->klass, FALSE, FALSE);
					}
					else
						MONO_ADD_INS (bblock, ins);
				}
				MONO_INST_NEW (cfg, ins, CEE_JMP);
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
			if (cmethod && (cfg->opt & MONO_OPT_INTRINS) && (ins = mini_get_inst_for_method (cfg, cmethod, fsig, sp))) {
				ins->cil_code = ip;

				if (MONO_TYPE_IS_VOID (fsig->ret)) {
					MONO_ADD_INS (bblock, ins);
				} else {
					type_to_eval_stack_type (fsig->ret, ins);
					*sp = ins;
					sp++;
				}

				ip += 5;
				break;
			}

			handle_loaded_temps (cfg, bblock, stack_start, sp);

			if ((cfg->opt & MONO_OPT_INLINE) && cmethod &&
			    (!virtual || !(cmethod->flags & METHOD_ATTRIBUTE_VIRTUAL) || (cmethod->flags & METHOD_ATTRIBUTE_FINAL)) && 
			    mono_method_check_inlining (cfg, cmethod) &&
				 !g_list_find (dont_inline, cmethod)) {
				int costs;
				MonoBasicBlock *ebblock;
				gboolean allways = FALSE;

				if ((cmethod->iflags & METHOD_IMPL_ATTRIBUTE_INTERNAL_CALL) ||
					(cmethod->flags & METHOD_ATTRIBUTE_PINVOKE_IMPL)) {
					cmethod = mono_marshal_get_native_wrapper (cmethod);
					allways = TRUE;
				}

 				if ((costs = inline_method (cfg, cmethod, fsig, bblock, sp, ip, real_offset, dont_inline, &ebblock, allways))) {
					ip += 5;
					real_offset += 5;

					GET_BBLOCK (cfg, bbhash, bblock, ip);
					ebblock->next_bb = bblock;
					link_bblock (cfg, ebblock, bblock);

 					if (!MONO_TYPE_IS_VOID (fsig->ret))
 						sp++;

					/* indicates start of a new block, and triggers a load of all 
					   stack arguments at bb boundarie */
					bblock = ebblock;

					inline_costs += costs;
					break;
				}
			}
			
			inline_costs += 10 * num_calls++;

			/* tail recursion elimination */
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
					MONO_INST_NEW (cfg, ins, CEE_BR);
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

				if ((temp = mono_emit_calli (cfg, bblock, fsig, sp, addr, ip)) != -1) {
					NEW_TEMPLOAD (cfg, *sp, temp);
					sp++;
				}
	      				
			} else if (array_rank) {
				MonoInst *addr;

				if (strcmp (cmethod->name, "Set") == 0) { /* array Set */ 
					if (sp [fsig->param_count]->type == STACK_OBJ) {
						MonoInst *iargs [2];
						MonoInst *array, *to_store, *store;

						handle_loaded_temps (cfg, bblock, stack_start, sp);
						
						array = mono_compile_create_var (cfg, type_from_stack_type (sp [0]), OP_LOCAL);
						NEW_TEMPSTORE (cfg, store, array->inst_c0, sp [0]);
						store->cil_code = ip;
						MONO_ADD_INS (bblock, store);
						NEW_TEMPLOAD (cfg, iargs [0], array->inst_c0);

						to_store = mono_compile_create_var (cfg, type_from_stack_type (sp [fsig->param_count]), OP_LOCAL);
						NEW_TEMPSTORE (cfg, store, to_store->inst_c0, sp [fsig->param_count]);
						store->cil_code = ip;
						MONO_ADD_INS (bblock, store);
						NEW_TEMPLOAD (cfg, iargs [1], to_store->inst_c0);

						/*
						 * We first save the args for the call so that the args are copied to the stack
						 * and a new instruction tree for them is created. If we don't do this,
						 * the same MonoInst is added to two different trees and this is not 
						 * allowed by burg.
						 */
						mono_emit_jit_icall (cfg, bblock, helper_stelem_ref_check, iargs, ip);

						NEW_TEMPLOAD (cfg, sp [0], array->inst_c0);
						NEW_TEMPLOAD (cfg, sp [fsig->param_count], to_store->inst_c0);
					}

					addr = mini_get_ldelema_ins (cfg, bblock, cmethod, sp, ip, TRUE);
					NEW_INDSTORE (cfg, ins, addr, sp [fsig->param_count], fsig->params [fsig->param_count - 1]);
					ins->cil_code = ip;
					if (ins->opcode == CEE_STOBJ) {
						handle_stobj (cfg, bblock, addr, sp [fsig->param_count], ip, mono_class_from_mono_type (fsig->params [fsig->param_count-1]), FALSE, FALSE);
					} else {
						MONO_ADD_INS (bblock, ins);
					}

				} else if (strcmp (cmethod->name, "Get") == 0) { /* array Get */
					addr = mini_get_ldelema_ins (cfg, bblock, cmethod, sp, ip, FALSE);
					NEW_INDLOAD (cfg, ins, addr, fsig->ret);
					ins->cil_code = ip;

					*sp++ = ins;
				} else if (strcmp (cmethod->name, "Address") == 0) { /* array Address */
					addr = mini_get_ldelema_ins (cfg, bblock, cmethod, sp, ip, FALSE);
					*sp++ = addr;
				} else {
					g_assert_not_reached ();
				}

			} else {
				if (ip_in_bb (cfg, bblock, ip + 5) 
				    && (!MONO_TYPE_ISSTRUCT (fsig->ret))
				    && (!MONO_TYPE_IS_VOID (fsig->ret) || cmethod->string_ctor)
				    && (CODE_IS_STLOC (ip + 5) || ip [5] == CEE_POP || ip [5] == CEE_RET)) {
					/* no need to spill */
					ins = (MonoInst*)mono_emit_method_call (cfg, bblock, cmethod, fsig, sp, ip, virtual ? sp [0] : NULL);
					*sp++ = ins;
				} else {
					if ((temp = mono_emit_method_call_spilled (cfg, bblock, cmethod, fsig, sp, ip, virtual ? sp [0] : NULL)) != -1) {
						NEW_TEMPLOAD (cfg, *sp, temp);
						sp++;
					}
				}
			}

			ip += 5;
			break;
		}
		case CEE_RET:
			if (cfg->method != method) {
				/* return from inlined methode */
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
						handle_stobj (cfg, bblock, store, *sp, sp [0]->cil_code, return_var->klass, FALSE, FALSE);
					} else
						MONO_ADD_INS (bblock, store);
				} 
			} else {
				if (cfg->ret) {
					g_assert (!return_var);
					CHECK_STACK (1);
					--sp;
					MONO_INST_NEW (cfg, ins, CEE_NOP);
					ins->opcode = mono_type_to_stind (mono_method_signature (method)->ret);
					if (ins->opcode == CEE_STOBJ) {
						NEW_RETLOADA (cfg, ins);
						handle_stobj (cfg, bblock, ins, *sp, ip, ins->klass, FALSE, FALSE);
					} else {
						ins->opcode = OP_SETRET;
						ins->cil_code = ip;
						ins->inst_i0 = *sp;;
						ins->inst_i1 = NULL;
						MONO_ADD_INS (bblock, ins);
					}
				}
			}
			if (sp != stack_start)
				goto unverified;
			MONO_INST_NEW (cfg, ins, CEE_BR);
			ins->cil_code = ip++;
			ins->inst_target_bb = end_bblock;
			MONO_ADD_INS (bblock, ins);
			link_bblock (cfg, bblock, end_bblock);
			start_new_bblock = 1;
			break;
		case CEE_BR_S:
			CHECK_OPSIZE (2);
			MONO_INST_NEW (cfg, ins, CEE_BR);
			ins->cil_code = ip++;
			MONO_ADD_INS (bblock, ins);
			target = ip + 1 + (signed char)(*ip);
			++ip;
			GET_BBLOCK (cfg, bbhash, tblock, target);
			link_bblock (cfg, bblock, tblock);
			CHECK_BBLOCK (target, ip, tblock);
			ins->inst_target_bb = tblock;
			if (sp != stack_start) {
				handle_stack_args (cfg, bblock, stack_start, sp - stack_start);
				sp = stack_start;
			}
			start_new_bblock = 1;
			inline_costs += 10;
			break;
		case CEE_BRFALSE_S:
		case CEE_BRTRUE_S:
			CHECK_OPSIZE (2);
			CHECK_STACK (1);
			MONO_INST_NEW (cfg, ins, *ip + BIG_BRANCH_OFFSET);
			ins->cil_code = ip++;
			target = ip + 1 + *(signed char*)ip;
			ip++;
			ADD_UNCOND (ins->opcode == CEE_BRTRUE);
			if (sp != stack_start) {
				handle_stack_args (cfg, bblock, stack_start, sp - stack_start);
				sp = stack_start;
			}
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
			if (sp != stack_start) {
				handle_stack_args (cfg, bblock, stack_start, sp - stack_start);
				sp = stack_start;
			}
			inline_costs += 10;
			break;
		case CEE_BR:
			CHECK_OPSIZE (5);
			MONO_INST_NEW (cfg, ins, CEE_BR);
			ins->cil_code = ip++;
			MONO_ADD_INS (bblock, ins);
			target = ip + 4 + (gint32)read32(ip);
			ip += 4;
			GET_BBLOCK (cfg, bbhash, tblock, target);
			link_bblock (cfg, bblock, tblock);
			CHECK_BBLOCK (target, ip, tblock);
			ins->inst_target_bb = tblock;
			if (sp != stack_start) {
				handle_stack_args (cfg, bblock, stack_start, sp - stack_start);
				sp = stack_start;
			}
			start_new_bblock = 1;
			inline_costs += 10;
			break;
		case CEE_BRFALSE:
		case CEE_BRTRUE:
			CHECK_OPSIZE (5);
			CHECK_STACK (1);
			MONO_INST_NEW (cfg, ins, *ip);
			ins->cil_code = ip++;
			target = ip + 4 + (gint32)read32(ip);
			ip += 4;
			ADD_UNCOND(ins->opcode == CEE_BRTRUE);
			if (sp != stack_start) {
				handle_stack_args (cfg, bblock, stack_start, sp - stack_start);
				sp = stack_start;
			}
			inline_costs += 10;
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
			CHECK_OPSIZE (5);
			CHECK_STACK (2);
			MONO_INST_NEW (cfg, ins, *ip);
			ins->cil_code = ip++;
			target = ip + 4 + (gint32)read32(ip);
			ip += 4;
			ADD_BINCOND(NULL);
			if (sp != stack_start) {
				handle_stack_args (cfg, bblock, stack_start, sp - stack_start);
				sp = stack_start;
			}
			inline_costs += 10;
			break;
		case CEE_SWITCH:
			CHECK_OPSIZE (5);
			CHECK_STACK (1);
			n = read32 (ip + 1);
			MONO_INST_NEW (cfg, ins, *ip);
			--sp;
			ins->inst_left = *sp;
			if ((ins->inst_left->type != STACK_I4) && (ins->inst_left->type != STACK_PTR)) 
				goto unverified;
			ins->cil_code = ip;
			ip += 5;
			CHECK_OPSIZE (n * sizeof (guint32));
			target = ip + n * sizeof (guint32);
			MONO_ADD_INS (bblock, ins);
			GET_BBLOCK (cfg, bbhash, tblock, target);
			link_bblock (cfg, bblock, tblock);
			ins->klass = GUINT_TO_POINTER (n);
			ins->inst_many_bb = mono_mempool_alloc (cfg->mempool, sizeof (MonoBasicBlock*) * (n + 1));
			ins->inst_many_bb [n] = tblock;

			for (i = 0; i < n; ++i) {
				GET_BBLOCK (cfg, bbhash, tblock, target + (gint32)read32(ip));
				link_bblock (cfg, bblock, tblock);
				ins->inst_many_bb [i] = tblock;
				ip += 4;
			}
			if (sp != stack_start) {
				handle_stack_args (cfg, bblock, stack_start, sp - stack_start);
				sp = stack_start;
			}
			/* Needed by the code generated in inssel.brg */
			mono_get_got_var (cfg);
			inline_costs += 20;
			break;
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
			MONO_INST_NEW (cfg, ins, *ip);
			ins->cil_code = ip;
			--sp;
			ins->inst_i0 = *sp;
			*sp++ = ins;
			ins->type = ldind_type [*ip - CEE_LDIND_I1];
			ins->flags |= ins_flag;
			ins_flag = 0;
			++ip;
			break;
		case CEE_STIND_REF:
		case CEE_STIND_I1:
		case CEE_STIND_I2:
		case CEE_STIND_I4:
		case CEE_STIND_I8:
		case CEE_STIND_R4:
		case CEE_STIND_R8:
			CHECK_STACK (2);
			MONO_INST_NEW (cfg, ins, *ip);
			ins->cil_code = ip++;
			sp -= 2;
			handle_loaded_temps (cfg, bblock, stack_start, sp);
			MONO_ADD_INS (bblock, ins);
			ins->inst_i0 = sp [0];
			ins->inst_i1 = sp [1];
			ins->flags |= ins_flag;
			ins_flag = 0;
			inline_costs += 1;
			break;
		case CEE_MUL:
			CHECK_STACK (2);
			ADD_BINOP (*ip);

#ifdef MONO_ARCH_NO_EMULATE_MUL_IMM
			/* FIXME: This breaks with ssapre (mono -O=ssapre loader.exe) */
			if ((ins->inst_right->opcode == OP_ICONST) && !(cfg->opt & MONO_OPT_SSAPRE)) {
				switch (ins->opcode) {
				case CEE_MUL:
					ins->opcode = OP_IMUL_IMM;
					ins->inst_imm = ins->inst_right->inst_c0;
					break;
				case OP_LMUL:
					ins->opcode = OP_LMUL_IMM;
					ins->inst_imm = ins->inst_right->inst_c0;
					break;
				default:
					g_assert_not_reached ();
				}
			}
#endif

			if (mono_find_jit_opcode_emulation (ins->opcode)) {
				--sp;
				*sp++ = emit_tree (cfg, bblock, ins, ip + 1);
				mono_get_got_var (cfg);
			}
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
			ADD_BINOP (*ip);
			/* special case that gives a nice speedup and happens to workaorund a ppc jit but (for the release)
			 * later apply the speedup to the left shift as well
			 * See BUG# 57957.
			 */
			if ((ins->opcode == OP_LSHR_UN) && (ins->type == STACK_I8) 
					&& (ins->inst_right->opcode == OP_ICONST) && (ins->inst_right->inst_c0 == 32)) {
				ins->opcode = OP_LONG_SHRUN_32;
				/*g_print ("applied long shr speedup to %s\n", cfg->method->name);*/
				ip++;
				break;
			}
			if (mono_find_jit_opcode_emulation (ins->opcode)) {
				--sp;
				*sp++ = emit_tree (cfg, bblock, ins, ip + 1);
				mono_get_got_var (cfg);
			}
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
			ADD_UNOP (*ip);
			if (mono_find_jit_opcode_emulation (ins->opcode)) {
				--sp;
				*sp++ = emit_tree (cfg, bblock, ins, ip + 1);
				mono_get_got_var (cfg);
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
			CHECK_STACK (1);
			ADD_UNOP (*ip);
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
				MONO_INST_NEW (cfg, load, CEE_LDIND_REF);
				load->cil_code = ip;
				load->inst_i0 = sp [1];
				load->type = STACK_OBJ;
				load->flags |= ins_flag;
				MONO_INST_NEW (cfg, store, CEE_STIND_REF);
				store->cil_code = ip;
				handle_loaded_temps (cfg, bblock, stack_start, sp);
				MONO_ADD_INS (bblock, store);
				store->inst_i0 = sp [0];
				store->inst_i1 = load;
				store->flags |= ins_flag;
			} else {
				n = mono_class_value_size (klass, NULL);
				if ((cfg->opt & MONO_OPT_INTRINS) && n <= sizeof (gpointer) * 5) {
					MonoInst *copy;
					MONO_INST_NEW (cfg, copy, OP_MEMCPY);
					copy->inst_left = sp [0];
					copy->inst_right = sp [1];
					copy->cil_code = ip;
					copy->unused = n;
					MONO_ADD_INS (bblock, copy);
				} else {
					MonoMethod *memcpy_method = get_memcpy_method ();
					MonoInst *iargs [3];
					iargs [0] = sp [0];
					iargs [1] = sp [1];
					NEW_ICONST (cfg, iargs [2], n);
					iargs [2]->cil_code = ip;

					mono_emit_method_call_spilled (cfg, bblock, memcpy_method, memcpy_method->signature, iargs, ip, NULL);
				}
			}
			ins_flag = 0;
			ip += 5;
			break;
		case CEE_LDOBJ: {
			MonoInst *iargs [3];
			int loc_index = -1;
			int stloc_len = 0;
			CHECK_OPSIZE (5);
			CHECK_STACK (1);
			--sp;
			token = read32 (ip + 1);
			klass = mini_get_class (method, token, generic_context);
			if (!klass)
				goto load_error;
			if (MONO_TYPE_IS_REFERENCE (&klass->byval_arg)) {
				MONO_INST_NEW (cfg, ins, CEE_LDIND_REF);
				ins->cil_code = ip;
				ins->inst_i0 = sp [0];
				ins->type = STACK_OBJ;
				ins->flags |= ins_flag;
				ins_flag = 0;
				*sp++ = ins;
				ip += 5;
				break;
			}

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
				NEW_LOCSTORE (cfg, ins, loc_index, *sp);

				if (ins->opcode == CEE_STOBJ) {
					handle_loaded_temps (cfg, bblock, stack_start, sp);
					ins->cil_code = ip;
					g_assert (ins->opcode == CEE_STOBJ);
					NEW_LOCLOADA (cfg, ins, loc_index);
					handle_stobj (cfg, bblock, ins, *sp, ip, ins->klass, FALSE, FALSE);
					ip += 5;
					ip += stloc_len;
					break;
				}
			}

			n = mono_class_value_size (klass, NULL);
			ins = mono_compile_create_var (cfg, &klass->byval_arg, OP_LOCAL);
			NEW_TEMPLOADA (cfg, iargs [0], ins->inst_c0);
			if ((cfg->opt & MONO_OPT_INTRINS) && n <= sizeof (gpointer) * 5) {
				MonoInst *copy;
				MONO_INST_NEW (cfg, copy, OP_MEMCPY);
				copy->inst_left = iargs [0];
				copy->inst_right = *sp;
				copy->cil_code = ip;
				copy->unused = n;
				MONO_ADD_INS (bblock, copy);
			} else {
				MonoMethod *memcpy_method = get_memcpy_method ();
				iargs [1] = *sp;
				NEW_ICONST (cfg, iargs [2], n);
				iargs [2]->cil_code = ip;

				mono_emit_method_call_spilled (cfg, bblock, memcpy_method, memcpy_method->signature, iargs, ip, NULL);
			}
			NEW_TEMPLOAD (cfg, *sp, ins->inst_c0);
			++sp;
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
				NEW_PCONST (cfg, ins, mono_method_get_wrapper_data (method, n));
				ins->cil_code = ip;
				ins->type = STACK_OBJ;
				*sp = ins;
			}
			else if (method->wrapper_type != MONO_WRAPPER_NONE) {
				int temp;
				MonoInst *iargs [1];

				NEW_PCONST (cfg, iargs [0], mono_method_get_wrapper_data (method, n));				
				temp = mono_emit_jit_icall (cfg, bblock, mono_string_new_wrapper, iargs, ip);
				NEW_TEMPLOAD (cfg, *sp, temp);

			} else {

				if (cfg->opt & MONO_OPT_SHARED) {
					int temp;
					MonoInst *iargs [3];
					MonoInst* domain_var;
					
					if (cfg->compile_aot) {
						cfg->ldstr_list = g_list_prepend (cfg->ldstr_list, GINT_TO_POINTER (n));
					}
					/* avoid depending on undefined C behavior in sequence points */
					domain_var = mono_get_domainvar (cfg);
					NEW_TEMPLOAD (cfg, iargs [0], domain_var->inst_c0);
					NEW_IMAGECONST (cfg, iargs [1], image);
					NEW_ICONST (cfg, iargs [2], mono_metadata_token_index (n));
					temp = mono_emit_jit_icall (cfg, bblock, mono_ldstr, iargs, ip);
					NEW_TEMPLOAD (cfg, *sp, temp);
					mono_ldstr (cfg->domain, image, mono_metadata_token_index (n));
				} else {
					if (bblock->out_of_line) {
						MonoInst *iargs [2];
						int temp;

						/* Avoid creating the string object */
						NEW_IMAGECONST (cfg, iargs [0], image);
						NEW_ICONST (cfg, iargs [1], mono_metadata_token_index (n));
						temp = mono_emit_jit_icall (cfg, bblock, helper_ldstr, iargs, ip);
						NEW_TEMPLOAD (cfg, *sp, temp);
					} 
					else
					if (cfg->compile_aot) {
						NEW_LDSTRCONST (cfg, ins, image, n);
						*sp = ins;
					} 
					else {
						NEW_PCONST (cfg, ins, NULL);
						ins->cil_code = ip;
						ins->type = STACK_OBJ;
						ins->inst_p0 = mono_ldstr (cfg->domain, image, mono_metadata_token_index (n));
						*sp = ins;
					}
				}
			}

			sp++;
			ip += 5;
			break;
		case CEE_NEWOBJ: {
			MonoInst *iargs [2];
			MonoMethodSignature *fsig;
			int temp;
			
			CHECK_OPSIZE (5);
			token = read32 (ip + 1);
			cmethod = mini_get_method (method, token, NULL, generic_context);
			if (!cmethod)
				goto load_error;
			fsig = mono_method_get_signature (cmethod, image, token);

			mono_class_init (cmethod->klass);

			if (mono_use_security_manager) {
				check_linkdemand (cfg, method, cmethod, bblock, ip);
			}

			n = fsig->param_count;
			CHECK_STACK (n);

			/* move the args to allow room for 'this' in the first position */
			while (n--) {
				--sp;
				sp [1] = sp [0];
			}

			handle_loaded_temps (cfg, bblock, stack_start, sp);

			if (mini_class_is_system_array (cmethod->klass)) {
				NEW_METHODCONST (cfg, *sp, cmethod);
				temp = handle_array_new (cfg, bblock, fsig->param_count, sp, ip);
			} else if (cmethod->string_ctor) {
				/* we simply pass a null pointer */
				NEW_PCONST (cfg, *sp, NULL); 
				/* now call the string ctor */
				temp = mono_emit_method_call_spilled (cfg, bblock, cmethod, fsig, sp, ip, NULL);
			} else {
				MonoInst* callvirt_this_arg = NULL;
				
				if (cmethod->klass->valuetype) {
					iargs [0] = mono_compile_create_var (cfg, &cmethod->klass->byval_arg, OP_LOCAL);
					temp = iargs [0]->inst_c0;

					NEW_TEMPLOADA (cfg, *sp, temp);

					handle_initobj (cfg, bblock, *sp, NULL, cmethod->klass, stack_start, sp);

					NEW_TEMPLOADA (cfg, *sp, temp);

					/* 
					 * The code generated by mini_emit_virtual_call () expects
					 * iargs [0] to be a boxed instance, but luckily the vcall
					 * will be transformed into a normal call there. The AOT
					 * case needs an already allocate got_var.
					 */
					mono_get_got_var (cfg);
				} else {
					temp = handle_alloc (cfg, bblock, cmethod->klass, FALSE, ip);
					NEW_TEMPLOAD (cfg, *sp, temp);
				}

				/* Avoid virtual calls to ctors if possible */
				if (cmethod->klass->marshalbyref)
					callvirt_this_arg = sp [0];
				
				if ((cfg->opt & MONO_OPT_INLINE) && cmethod &&
				    mono_method_check_inlining (cfg, cmethod) &&
				    !mono_class_is_subclass_of (cmethod->klass, mono_defaults.exception_class, FALSE) &&
				    !g_list_find (dont_inline, cmethod)) {
					int costs;
					MonoBasicBlock *ebblock;
					if ((costs = inline_method (cfg, cmethod, fsig, bblock, sp, ip, real_offset, dont_inline, &ebblock, FALSE))) {

						ip += 5;
						real_offset += 5;
						
						GET_BBLOCK (cfg, bbhash, bblock, ip);
						ebblock->next_bb = bblock;
						link_bblock (cfg, ebblock, bblock);

						NEW_TEMPLOAD (cfg, *sp, temp);
						sp++;

						/* indicates start of a new block, and triggers a load 
						   of all stack arguments at bb boundarie */
						bblock = ebblock;

						inline_costs += costs;
						break;
						
					} else {
						mono_emit_method_call_spilled (cfg, bblock, cmethod, fsig, sp, ip, callvirt_this_arg);
					}
				} else {
					/* now call the actual ctor */
					mono_emit_method_call_spilled (cfg, bblock, cmethod, fsig, sp, ip, callvirt_this_arg);
				}
			}

			NEW_TEMPLOAD (cfg, *sp, temp);
			sp++;
			
			ip += 5;
			inline_costs += 5;
			break;
		}
		case CEE_ISINST:
			CHECK_STACK (1);
			--sp;
			CHECK_OPSIZE (5);
			token = read32 (ip + 1);
			klass = mini_get_class (method, token, generic_context);
			if (!klass)
				goto load_error;

			/* Needed by the code generated in inssel.brg */
			mono_get_got_var (cfg);

			if (klass->marshalbyref || klass->flags & TYPE_ATTRIBUTE_INTERFACE) {
			
				MonoMethod *mono_isinst;
				MonoInst *iargs [1];
				MonoBasicBlock *ebblock;
				int costs;
				int temp;
				
				mono_isinst = mono_marshal_get_isinst (klass); 
				iargs [0] = sp [0];
				
				costs = inline_method (cfg, mono_isinst, mono_method_signature (mono_isinst), bblock, 
							   iargs, ip, real_offset, dont_inline, &ebblock, TRUE);
			
				g_assert (costs > 0);
				
				ip += 5;
				real_offset += 5;
			
				GET_BBLOCK (cfg, bbhash, bblock, ip);
				ebblock->next_bb = bblock;
				link_bblock (cfg, ebblock, bblock);

				temp = iargs [0]->inst_i0->inst_c0;
				NEW_TEMPLOAD (cfg, *sp, temp);
				
 				sp++;
				bblock = ebblock;
				inline_costs += costs;

			}
			else {
				MONO_INST_NEW (cfg, ins, *ip);
				ins->type = STACK_OBJ;
				ins->inst_left = *sp;
				ins->inst_newa_class = klass;
				ins->cil_code = ip;
				*sp++ = emit_tree (cfg, bblock, ins, ip + 5);
				ip += 5;
			}
			break;
		case CEE_UNBOX_ANY: {
			MonoInst *add, *vtoffset;
			MonoInst *iargs [3];

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
					MonoBasicBlock *ebblock;
					int costs;
					int temp;
					
					mono_castclass = mono_marshal_get_castclass (klass); 
					iargs [0] = sp [0];
					
					costs = inline_method (cfg, mono_castclass, mono_method_signature (mono_castclass), bblock, 
								   iargs, ip, real_offset, dont_inline, &ebblock, TRUE);
				
					g_assert (costs > 0);
					
					ip += 5;
					real_offset += 5;
				
					GET_BBLOCK (cfg, bbhash, bblock, ip);
					ebblock->next_bb = bblock;
					link_bblock (cfg, ebblock, bblock);
	
					temp = iargs [0]->inst_i0->inst_c0;
					NEW_TEMPLOAD (cfg, *sp, temp);
					
					sp++;
					bblock = ebblock;
					inline_costs += costs;				
				}
				else {
					MONO_INST_NEW (cfg, ins, CEE_CASTCLASS);
					ins->type = STACK_OBJ;
					ins->inst_left = *sp;
					ins->klass = klass;
					ins->inst_newa_class = klass;
					ins->cil_code = ip;
					*sp++ = ins;
					ip += 5;
				}
				break;
			}

			MONO_INST_NEW (cfg, ins, OP_UNBOXCAST);
			ins->type = STACK_OBJ;
			ins->inst_left = *sp;
			ins->klass = klass;
			ins->inst_newa_class = klass;
			ins->cil_code = ip;

			MONO_INST_NEW (cfg, add, OP_PADD);
			NEW_ICONST (cfg, vtoffset, sizeof (MonoObject));
			add->inst_left = ins;
			add->inst_right = vtoffset;
			add->type = STACK_MP;
			*sp = add;
			ip += 5;
			/* LDOBJ impl */
			n = mono_class_value_size (klass, NULL);
			ins = mono_compile_create_var (cfg, &klass->byval_arg, OP_LOCAL);
			NEW_TEMPLOADA (cfg, iargs [0], ins->inst_c0);
			if ((cfg->opt & MONO_OPT_INTRINS) && n <= sizeof (gpointer) * 5) {
				MonoInst *copy;
				MONO_INST_NEW (cfg, copy, OP_MEMCPY);
				copy->inst_left = iargs [0];
				copy->inst_right = *sp;
				copy->cil_code = ip;
				copy->unused = n;
				MONO_ADD_INS (bblock, copy);
			} else {
				MonoMethod *memcpy_method = get_memcpy_method ();
				iargs [1] = *sp;
				NEW_ICONST (cfg, iargs [2], n);
				iargs [2]->cil_code = ip;

				mono_emit_method_call_spilled (cfg, bblock, memcpy_method, memcpy_method->signature, iargs, ip, NULL);
			}
			NEW_TEMPLOAD (cfg, *sp, ins->inst_c0);
			++sp;
			inline_costs += 2;
			break;
		}
		case CEE_UNBOX: {
			MonoInst *add, *vtoffset;

			CHECK_STACK (1);
			--sp;
			CHECK_OPSIZE (5);
			token = read32 (ip + 1);
			klass = mini_get_class (method, token, generic_context);
			if (!klass)
				goto load_error;

			/* Needed by the code generated in inssel.brg */
			mono_get_got_var (cfg);

			MONO_INST_NEW (cfg, ins, OP_UNBOXCAST);
			ins->type = STACK_OBJ;
			ins->inst_left = *sp;
			ins->klass = klass;
			ins->inst_newa_class = klass;
			ins->cil_code = ip;

			MONO_INST_NEW (cfg, add, OP_PADD);
			NEW_ICONST (cfg, vtoffset, sizeof (MonoObject));
			add->inst_left = ins;
			add->inst_right = vtoffset;
			add->type = STACK_MP;
			*sp++ = add;
			ip += 5;
			inline_costs += 2;
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

			/* Needed by the code generated in inssel.brg */
			mono_get_got_var (cfg);
		
			if (klass->marshalbyref || klass->flags & TYPE_ATTRIBUTE_INTERFACE) {
				
				MonoMethod *mono_castclass;
				MonoInst *iargs [1];
				MonoBasicBlock *ebblock;
				int costs;
				int temp;
				
				mono_castclass = mono_marshal_get_castclass (klass); 
				iargs [0] = sp [0];
				
				costs = inline_method (cfg, mono_castclass, mono_method_signature (mono_castclass), bblock, 
							   iargs, ip, real_offset, dont_inline, &ebblock, TRUE);
			
				g_assert (costs > 0);
				
				ip += 5;
				real_offset += 5;
			
				GET_BBLOCK (cfg, bbhash, bblock, ip);
				ebblock->next_bb = bblock;
				link_bblock (cfg, ebblock, bblock);

				temp = iargs [0]->inst_i0->inst_c0;
				NEW_TEMPLOAD (cfg, *sp, temp);
				
 				sp++;
				bblock = ebblock;
				inline_costs += costs;
			}
			else {
				MONO_INST_NEW (cfg, ins, *ip);
				ins->type = STACK_OBJ;
				ins->inst_left = *sp;
				ins->klass = klass;
				ins->inst_newa_class = klass;
				ins->cil_code = ip;
				*sp++ = emit_tree (cfg, bblock, ins, ip + 5);
				ip += 5;
			}
			break;
		case CEE_THROW:
			CHECK_STACK (1);
			MONO_INST_NEW (cfg, ins, *ip);
			--sp;
			ins->inst_left = *sp;
			ins->cil_code = ip++;
			bblock->out_of_line = TRUE;
			MONO_ADD_INS (bblock, ins);
			MONO_INST_NEW (cfg, ins, OP_NOT_REACHED);
			ins->cil_code = ip - 1;
			MONO_ADD_INS (bblock, ins);
			sp = stack_start;
			
			link_bblock (cfg, bblock, end_bblock);
			start_new_bblock = 1;
			mono_get_got_var (cfg);
			break;
		case CEE_LDFLD:
		case CEE_LDFLDA:
		case CEE_STFLD: {
			MonoInst *offset_ins;
			MonoClassField *field;
			MonoBasicBlock *ebblock;
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
					NEW_CLASSCONST (cfg, iargs [1], klass);
					NEW_FIELDCONST (cfg, iargs [2], field);
					NEW_ICONST (cfg, iargs [3], klass->valuetype ? field->offset - sizeof (MonoObject) : 
						    field->offset);
					iargs [4] = sp [1];

					if (cfg->opt & MONO_OPT_INLINE) {
						costs = inline_method (cfg, stfld_wrapper, mono_method_signature (stfld_wrapper), bblock, 
								       iargs, ip, real_offset, dont_inline, &ebblock, TRUE);
						g_assert (costs > 0);
						      
						ip += 5;
						real_offset += 5;

						GET_BBLOCK (cfg, bbhash, bblock, ip);
						ebblock->next_bb = bblock;
						link_bblock (cfg, ebblock, bblock);

						/* indicates start of a new block, and triggers a load 
						   of all stack arguments at bb boundarie */
						bblock = ebblock;

						inline_costs += costs;
						break;
					} else {
						mono_emit_method_call_spilled (cfg, bblock, stfld_wrapper, mono_method_signature (stfld_wrapper), iargs, ip, NULL);
					}
				} else {
					MonoInst *store;
					NEW_ICONST (cfg, offset_ins, foffset);
					MONO_INST_NEW (cfg, ins, OP_PADD);
					ins->cil_code = ip;
					ins->inst_left = *sp;
					ins->inst_right = offset_ins;
					ins->type = STACK_MP;

					MONO_INST_NEW (cfg, store, mono_type_to_stind (field->type));
					store->cil_code = ip;
					store->inst_left = ins;
					store->inst_right = sp [1];
					handle_loaded_temps (cfg, bblock, stack_start, sp);
					store->flags |= ins_flag;
					ins_flag = 0;
					if (store->opcode == CEE_STOBJ) {
						handle_stobj (cfg, bblock, ins, sp [1], ip, 
							      mono_class_from_mono_type (field->type), FALSE, FALSE);
					} else
						MONO_ADD_INS (bblock, store);
				}
			} else {
				if ((klass->marshalbyref && !MONO_CHECK_THIS (sp [0])) || klass->contextbound || klass == mono_defaults.marshalbyrefobject_class) {
					MonoMethod *wrapper = (*ip == CEE_LDFLDA) ? mono_marshal_get_ldflda_wrapper (field->type) : mono_marshal_get_ldfld_wrapper (field->type); 
					MonoInst *iargs [4];
					int temp;
					
					iargs [0] = sp [0];
					NEW_CLASSCONST (cfg, iargs [1], klass);
					NEW_FIELDCONST (cfg, iargs [2], field);
					NEW_ICONST (cfg, iargs [3], klass->valuetype ? field->offset - sizeof (MonoObject) : field->offset);
					if ((cfg->opt & MONO_OPT_INLINE) && !MONO_TYPE_ISSTRUCT (mono_method_signature (wrapper)->ret)) {
						costs = inline_method (cfg, wrapper, mono_method_signature (wrapper), bblock, 
								       iargs, ip, real_offset, dont_inline, &ebblock, TRUE);
						g_assert (costs > 0);
						      
						ip += 5;
						real_offset += 5;

						GET_BBLOCK (cfg, bbhash, bblock, ip);
						ebblock->next_bb = bblock;
						link_bblock (cfg, ebblock, bblock);

						temp = iargs [0]->inst_i0->inst_c0;

						NEW_TEMPLOAD (cfg, *sp, temp);
						sp++;

						/* indicates start of a new block, and triggers a load of
						   all stack arguments at bb boundarie */
						bblock = ebblock;
						
						inline_costs += costs;
						break;
					} else {
						temp = mono_emit_method_call_spilled (cfg, bblock, wrapper, mono_method_signature (wrapper), iargs, ip, NULL);
						NEW_TEMPLOAD (cfg, *sp, temp);
						sp++;
					}
				} else {
					NEW_ICONST (cfg, offset_ins, foffset);
					MONO_INST_NEW (cfg, ins, OP_PADD);
					ins->cil_code = ip;
					ins->inst_left = *sp;
					ins->inst_right = offset_ins;
					ins->type = STACK_MP;

					if (*ip == CEE_LDFLDA) {
						*sp++ = ins;
					} else {
						MonoInst *load;
						MONO_INST_NEW (cfg, load, mono_type_to_ldind (field->type));
						type_to_eval_stack_type (field->type, load);
						load->cil_code = ip;
						load->inst_left = ins;
						load->flags |= ins_flag;
						ins_flag = 0;
						*sp++ = load;
					}
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

			field = mono_field_from_token (image, token, &klass, generic_context);
			if (!field)
				goto load_error;
			mono_class_init (klass);

			g_assert (!(field->type->attrs & FIELD_ATTRIBUTE_LITERAL));

			if ((*ip) == CEE_STSFLD)
				handle_loaded_temps (cfg, bblock, stack_start, sp);

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
				int temp;
				MonoInst *iargs [2];
				MonoInst *domain_var;
				
				g_assert (field->parent);
				/* avoid depending on undefined C behavior in sequence points */
				domain_var = mono_get_domainvar (cfg);
				NEW_TEMPLOAD (cfg, iargs [0], domain_var->inst_c0);
				NEW_FIELDCONST (cfg, iargs [1], field);
				temp = mono_emit_jit_icall (cfg, bblock, mono_class_static_field_address, iargs, ip);
				NEW_TEMPLOAD (cfg, ins, temp);
			} else {
				MonoVTable *vtable;
				vtable = mono_class_vtable (cfg->domain, klass);
				if (!addr) {
					if (mini_field_access_needs_cctor_run (cfg, method, vtable) && !(g_slist_find (class_inits, vtable))) {
						guint8 *tramp = mono_create_class_init_trampoline (vtable);
						mono_emit_native_call (cfg, bblock, tramp, 
											   helper_sig_class_init_trampoline,
											   NULL, ip, FALSE, FALSE);
						if (cfg->verbose_level > 2)
							g_print ("class %s.%s needs init call for %s\n", klass->name_space, klass->name, field->name);
						class_inits = g_slist_prepend (class_inits, vtable);
					} else {
						if (cfg->run_cctors)
							mono_runtime_class_init (vtable);
					}
					addr = (char*)vtable->data + field->offset;

					if (cfg->compile_aot)
						NEW_SFLDACONST (cfg, ins, field);
					else
						NEW_PCONST (cfg, ins, addr);
					ins->cil_code = ip;
				} else {
					/* 
					 * insert call to mono_threads_get_static_data (GPOINTER_TO_UINT (addr)) 
					 * This could be later optimized to do just a couple of
					 * memory dereferences with constant offsets.
					 */
					int temp;
					MonoInst *iargs [1];
					NEW_ICONST (cfg, iargs [0], GPOINTER_TO_UINT (addr));
					temp = mono_emit_jit_icall (cfg, bblock, mono_get_special_static_data, iargs, ip);
					NEW_TEMPLOAD (cfg, ins, temp);
				}
			}

			/* FIXME: mark instructions for use in SSA */
			if (*ip == CEE_LDSFLDA) {
				*sp++ = ins;
			} else if (*ip == CEE_STSFLD) {
				MonoInst *store;
				CHECK_STACK (1);
				sp--;
				MONO_INST_NEW (cfg, store, mono_type_to_stind (field->type));
				store->cil_code = ip;
				store->inst_left = ins;
				store->inst_right = sp [0];
				store->flags |= ins_flag;
				ins_flag = 0;

				if (store->opcode == CEE_STOBJ) {
					handle_stobj (cfg, bblock, ins, sp [0], ip, mono_class_from_mono_type (field->type), FALSE, FALSE);
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
					/* g_print ("RO-FIELD %s.%s:%s\n", klass->name_space, klass->name, field->name);*/
					is_const = TRUE;
					switch (ro_type) {
					case MONO_TYPE_BOOLEAN:
					case MONO_TYPE_U1:
						NEW_ICONST (cfg, *sp, *((guint8 *)addr));
						sp++;
						break;
					case MONO_TYPE_I1:
						NEW_ICONST (cfg, *sp, *((gint8 *)addr));
						sp++;
						break;						
					case MONO_TYPE_CHAR:
					case MONO_TYPE_U2:
						NEW_ICONST (cfg, *sp, *((guint16 *)addr));
						sp++;
						break;
					case MONO_TYPE_I2:
						NEW_ICONST (cfg, *sp, *((gint16 *)addr));
						sp++;
						break;
						break;
					case MONO_TYPE_I4:
						NEW_ICONST (cfg, *sp, *((gint32 *)addr));
						sp++;
						break;						
					case MONO_TYPE_U4:
						NEW_ICONST (cfg, *sp, *((guint32 *)addr));
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
						NEW_PCONST (cfg, *sp, *((gpointer *)addr));
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
					CHECK_STACK_OVF (1);
					MONO_INST_NEW (cfg, load, mono_type_to_ldind (field->type));
					type_to_eval_stack_type (field->type, load);
					load->cil_code = ip;
					load->inst_left = ins;
					*sp++ = load;
					load->flags |= ins_flag;
					ins_flag = 0;
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
			n = mono_type_to_stind (&klass->byval_arg);
			if (n == CEE_STOBJ) {
				handle_stobj (cfg, bblock, sp [0], sp [1], ip, klass, FALSE, FALSE);
			} else {
				/* FIXME: should check item at sp [1] is compatible with the type of the store. */
				MonoInst *store;
				MONO_INST_NEW (cfg, store, n);
				store->cil_code = ip;
				store->inst_left = sp [0];
				store->inst_right = sp [1];
				store->flags |= ins_flag;
				MONO_ADD_INS (bblock, store);
			}
			ins_flag = 0;
			ip += 5;
			inline_costs += 1;
			break;
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
			*sp++ = handle_box (cfg, bblock, val, ip, klass);
			ip += 5;
			inline_costs += 1;
			break;
		}
		case CEE_NEWARR:
			CHECK_STACK (1);
			MONO_INST_NEW (cfg, ins, *ip);
			ins->cil_code = ip;
			--sp;

			CHECK_OPSIZE (5);
			token = read32 (ip + 1);

			/* allocate the domainvar - becaus this is used in decompose_foreach */
			if (cfg->opt & MONO_OPT_SHARED) {
				mono_get_domainvar (cfg);
				/* LAME-IR: Mark it as used since otherwise it will be optimized away */
				cfg->domainvar->flags |= MONO_INST_VOLATILE;
			}

			/* Ditto */
			mono_get_got_var (cfg);

			klass = mini_get_class (method, token, generic_context);
			if (!klass)
				goto load_error;
			ins->inst_newa_class = klass;
			ins->inst_newa_len = *sp;
			ins->type = STACK_OBJ;
			ip += 5;
			*sp++ = ins;
			/* 
			 * we store the object so calls to create the array are not interleaved
			 * with the arguments of other calls.
			 */
			if (1) {
				MonoInst *store, *temp, *load;
				--sp;
				temp = mono_compile_create_var (cfg, type_from_stack_type (ins), OP_LOCAL);
				NEW_TEMPSTORE (cfg, store, temp->inst_c0, ins);
				store->cil_code = ins->cil_code;
				MONO_ADD_INS (bblock, store);
				NEW_TEMPLOAD (cfg, load, temp->inst_c0);
				load->cil_code = ins->cil_code;
				*sp++ = load;
			}
			inline_costs += 1;
			break;
		case CEE_LDLEN:
			CHECK_STACK (1);
			MONO_INST_NEW (cfg, ins, *ip);
			ins->cil_code = ip++;
			--sp;
			ins->inst_left = *sp;
			ins->type = STACK_PTR;
			*sp++ = ins;
			break;
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
				MonoInst* check;
				MONO_INST_NEW (cfg, check, OP_CHECK_ARRAY_TYPE);
				check->cil_code = ip;
				check->klass = klass;
				check->inst_left = sp [0];
				check->type = STACK_OBJ;
				sp [0] = check;
			}
			
			mono_class_init (klass);
			NEW_LDELEMA (cfg, ins, sp, klass);
			ins->cil_code = ip;
			*sp++ = ins;
			ip += 5;
			break;
		case CEE_LDELEM_ANY: {
			MonoInst *load;
			CHECK_STACK (2);
			sp -= 2;
			CHECK_OPSIZE (5);
			token = read32 (ip + 1);
			klass = mono_class_get_full (image, token, generic_context);
			if (!klass)
				goto load_error;
			mono_class_init (klass);
			NEW_LDELEMA (cfg, load, sp, klass);
			load->cil_code = ip;
			MONO_INST_NEW (cfg, ins, mono_type_to_ldind (&klass->byval_arg));
			ins->cil_code = ip;
			ins->inst_left = load;
			*sp++ = ins;
			type_to_eval_stack_type (&klass->byval_arg, ins);
			ip += 5;
			break;
		}
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
			MonoInst *load;
			/*
			 * translate to:
			 * ldind.x (ldelema (array, index))
			 * ldelema does the bounds check
			 */
			CHECK_STACK (2);
			sp -= 2;
			klass = array_access_to_klass (*ip);
			NEW_LDELEMA (cfg, load, sp, klass);
			load->cil_code = ip;
			MONO_INST_NEW (cfg, ins, ldelem_to_ldind [*ip - CEE_LDELEM_I1]);
			ins->cil_code = ip;
			ins->inst_left = load;
			*sp++ = ins;
			ins->type = ldind_type [ins->opcode - CEE_LDIND_I1];
			++ip;
			break;
		}
		case CEE_STELEM_I:
		case CEE_STELEM_I1:
		case CEE_STELEM_I2:
		case CEE_STELEM_I4:
		case CEE_STELEM_I8:
		case CEE_STELEM_R4:
		case CEE_STELEM_R8: {
			MonoInst *load;
			/*
			 * translate to:
			 * stind.x (ldelema (array, index), val)
			 * ldelema does the bounds check
			 */
			CHECK_STACK (3);
			sp -= 3;
			klass = array_access_to_klass (*ip);
			NEW_LDELEMA (cfg, load, sp, klass);
			load->cil_code = ip;
			MONO_INST_NEW (cfg, ins, stelem_to_stind [*ip - CEE_STELEM_I]);
			ins->cil_code = ip;
			ins->inst_left = load;
			ins->inst_right = sp [2];
			++ip;
			handle_loaded_temps (cfg, bblock, stack_start, sp);
			MONO_ADD_INS (bblock, ins);
			inline_costs += 1;
			break;
		}
		case CEE_STELEM_ANY: {
			MonoInst *load;
			/*
			 * translate to:
			 * stind.x (ldelema (array, index), val)
			 * ldelema does the bounds check
			 */
			CHECK_STACK (3);
			sp -= 3;
			CHECK_OPSIZE (5);
			token = read32 (ip + 1);
			klass = mono_class_get_full (image, token, generic_context);
			if (!klass)
				goto load_error;
			mono_class_init (klass);
			if (MONO_TYPE_IS_REFERENCE (&klass->byval_arg)) {
				MonoMethod* helper = mono_marshal_get_stelemref ();
				MonoInst *iargs [3];
				handle_loaded_temps (cfg, bblock, stack_start, sp);

				iargs [2] = sp [2];
				iargs [1] = sp [1];
				iargs [0] = sp [0];
				
				mono_emit_method_call_spilled (cfg, bblock, helper, mono_method_signature (helper), iargs, ip, NULL);
			} else {
				NEW_LDELEMA (cfg, load, sp, klass);
				load->cil_code = ip;

				n = mono_type_to_stind (&klass->byval_arg);
				if (n == CEE_STOBJ)
					handle_stobj (cfg, bblock, load, sp [2], ip, klass, FALSE, FALSE);
				else {
					MONO_INST_NEW (cfg, ins, n);
					ins->cil_code = ip;
					ins->inst_left = load;
					ins->inst_right = sp [2];
					handle_loaded_temps (cfg, bblock, stack_start, sp);
					MONO_ADD_INS (bblock, ins);
				}
			}
			ip += 5;
			inline_costs += 1;
			break;
		}
		case CEE_STELEM_REF: {
			MonoInst *iargs [3];
			MonoMethod* helper = mono_marshal_get_stelemref ();

			CHECK_STACK (3);
			sp -= 3;

			handle_loaded_temps (cfg, bblock, stack_start, sp);

			iargs [2] = sp [2];
			iargs [1] = sp [1];
			iargs [0] = sp [0];
			
			mono_emit_method_call_spilled (cfg, bblock, helper, mono_method_signature (helper), iargs, ip, NULL);

			/*
			MonoInst *group;
			NEW_GROUP (cfg, group, sp [0], sp [1]);
			MONO_INST_NEW (cfg, ins, CEE_STELEM_REF);
			ins->cil_code = ip;
			ins->inst_left = group;
			ins->inst_right = sp [2];
			MONO_ADD_INS (bblock, ins);
			*/

			++ip;
			inline_costs += 1;
			break;
		}
		case CEE_CKFINITE: {
			MonoInst *store, *temp;
			CHECK_STACK (1);

			/* this instr. can throw exceptions as side effect,
			 * so we cant eliminate dead code which contains CKFINITE opdodes.
			 * Spilling to memory makes sure that we always perform
			 * this check */

			
			MONO_INST_NEW (cfg, ins, CEE_CKFINITE);
			ins->cil_code = ip;
			ins->inst_left = sp [-1];
			temp = mono_compile_create_var (cfg, &mono_defaults.double_class->byval_arg, OP_LOCAL);

			NEW_TEMPSTORE (cfg, store, temp->inst_c0, ins);
			store->cil_code = ip;
			MONO_ADD_INS (bblock, store);

			NEW_TEMPLOAD (cfg, sp [-1], temp->inst_c0);
		       
			++ip;
			break;
		}
		case CEE_REFANYVAL:
			CHECK_STACK (1);
			MONO_INST_NEW (cfg, ins, *ip);
			--sp;
			CHECK_OPSIZE (5);
			klass = mono_class_get_full (image, read32 (ip + 1), generic_context);
			if (!klass)
				goto load_error;
			mono_class_init (klass);
			ins->type = STACK_MP;
			ins->inst_left = *sp;
			ins->klass = klass;
			ins->inst_newa_class = klass;
			ins->cil_code = ip;
			ip += 5;
			*sp++ = ins;
			break;
		case CEE_MKREFANY: {
			MonoInst *loc, *klassconst;

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
			NEW_TEMPLOADA (cfg, ins->inst_right, loc->inst_c0);

			NEW_PCONST (cfg, klassconst, klass);
			NEW_GROUP (cfg, ins->inst_left, *sp, klassconst);
			
			MONO_ADD_INS (bblock, ins);

			NEW_TEMPLOAD (cfg, *sp, loc->inst_c0);
			++sp;
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
			}
			else {
				handle = mono_ldtoken (image, n, &handle_class, generic_context);
			}
			if (!handle)
				goto load_error;
			mono_class_init (handle_class);

			if (cfg->opt & MONO_OPT_SHARED) {
				int temp;
				MonoInst *res, *store, *addr, *vtvar, *iargs [3];

				vtvar = mono_compile_create_var (cfg, &handle_class->byval_arg, OP_LOCAL); 

				NEW_IMAGECONST (cfg, iargs [0], image);
				NEW_ICONST (cfg, iargs [1], n);
				NEW_PCONST (cfg, iargs [2], generic_context);
				temp = mono_emit_jit_icall (cfg, bblock, mono_ldtoken_wrapper, iargs, ip);
				NEW_TEMPLOAD (cfg, res, temp);
				NEW_TEMPLOADA (cfg, addr, vtvar->inst_c0);
				NEW_INDSTORE (cfg, store, addr, res, &mono_defaults.int_class->byval_arg);
				MONO_ADD_INS (bblock, store);
				NEW_TEMPLOAD (cfg, ins, vtvar->inst_c0);
			} else {
				if ((ip [5] == CEE_CALL) && (cmethod = mini_get_method (method, read32 (ip + 6), NULL, generic_context)) &&
						(cmethod->klass == mono_defaults.monotype_class->parent) &&
						(strcmp (cmethod->name, "GetTypeFromHandle") == 0) && ip_in_bb (cfg, bblock, ip + 5)) {
					MonoClass *tclass = mono_class_from_mono_type (handle);
					mono_class_init (tclass);
					if (cfg->compile_aot)
						NEW_TYPE_FROM_HANDLE_CONST (cfg, ins, image, n);
					else
						NEW_PCONST (cfg, ins, mono_type_get_object (cfg->domain, handle));
					ins->type = STACK_OBJ;
					ins->klass = cmethod->klass;
					ip += 5;
				} else {
					MonoInst *store, *addr, *vtvar;

					if (cfg->compile_aot)
						NEW_LDTOKENCONST (cfg, ins, image, n);
					else
						NEW_PCONST (cfg, ins, handle);
					vtvar = mono_compile_create_var (cfg, &handle_class->byval_arg, OP_LOCAL);
					NEW_TEMPLOADA (cfg, addr, vtvar->inst_c0);
					NEW_INDSTORE (cfg, store, addr, ins, &mono_defaults.int_class->byval_arg);
					MONO_ADD_INS (bblock, store);
					NEW_TEMPLOAD (cfg, ins, vtvar->inst_c0);
				}
			}

			*sp++ = ins;
			ip += 5;
			break;
		}
		case CEE_CONV_U2:
		case CEE_CONV_U1:
		case CEE_CONV_I:
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
			if (mono_find_jit_opcode_emulation (ins->opcode)) {
				--sp;
				*sp++ = emit_tree (cfg, bblock, ins, ip + 1);
				mono_get_got_var (cfg);
			}
			ip++;
			break;
		case CEE_ENDFINALLY:
			MONO_INST_NEW (cfg, ins, *ip);
			MONO_ADD_INS (bblock, ins);
			ins->cil_code = ip++;
			start_new_bblock = 1;

			/*
			 * Control will leave the method so empty the stack, otherwise
			 * the next basic block will start with a nonempty stack.
			 */
			while (sp != stack_start) {
				MONO_INST_NEW (cfg, ins, CEE_POP);
				ins->cil_code = ip;
				sp--;
				ins->inst_i0 = *sp;
				MONO_ADD_INS (bblock, ins);
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
				MONO_INST_NEW (cfg, ins, CEE_POP);
				ins->cil_code = ip;
				sp--;
				ins->inst_i0 = *sp;
				MONO_ADD_INS (bblock, ins);
			}

			/* 
			 * If this leave statement is in a catch block, check for a
			 * pending exception, and rethrow it if necessary.
			 */
			for (i = 0; i < header->num_clauses; ++i) {
				MonoExceptionClause *clause = &header->clauses [i];

				if (MONO_OFFSET_IN_HANDLER (clause, ip - header->code) && (clause->flags == MONO_EXCEPTION_CLAUSE_NONE) && (ip - header->code + ((*ip == CEE_LEAVE) ? 5 : 2)) == (clause->handler_offset + clause->handler_len)) {
					int temp;
					MonoInst *load;

					NEW_TEMPLOAD (cfg, load, mono_find_exvar_for_offset (cfg, clause->handler_offset)->inst_c0);
					load->cil_code = ip;

					temp = mono_emit_jit_icall (cfg, bblock, mono_thread_get_pending_exception, NULL, ip);
					NEW_TEMPLOAD (cfg, *sp, temp);
				
					MONO_INST_NEW (cfg, ins, OP_THROW_OR_NULL);
					ins->inst_left = *sp;
					ins->inst_right = load;
					ins->cil_code = ip;
					MONO_ADD_INS (bblock, ins);
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

			MONO_INST_NEW (cfg, ins, CEE_BR);
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
		case CEE_STIND_I:
			CHECK_STACK (2);
			MONO_INST_NEW (cfg, ins, *ip);
			sp -= 2;
			handle_loaded_temps (cfg, bblock, stack_start, sp);
			MONO_ADD_INS (bblock, ins);
			ins->cil_code = ip++;
			ins->inst_i0 = sp [0];
			ins->inst_i1 = sp [1];
			inline_costs += 1;
			break;
		case CEE_CONV_U:
			CHECK_STACK (1);
			ADD_UNOP (*ip);
			ip++;
			break;
		/* trampoline mono specific opcodes */
		case MONO_CUSTOM_PREFIX: {

			g_assert (method->wrapper_type != MONO_WRAPPER_NONE);

			CHECK_OPSIZE (2);
			switch (ip [1]) {

			case CEE_MONO_ICALL: {
				int temp;
				gpointer func;
				MonoJitICallInfo *info;

				token = read32 (ip + 2);
				func = mono_method_get_wrapper_data (method, token);
				info = mono_find_jit_icall_by_addr (func);
				g_assert (info);

				CHECK_STACK (info->sig->param_count);
				sp -= info->sig->param_count;

				temp = mono_emit_jit_icall (cfg, bblock, info->func, sp, ip);
				if (!MONO_TYPE_IS_VOID (info->sig->ret)) {
					NEW_TEMPLOAD (cfg, *sp, temp);
					sp++;
				}

				ip += 6;
				inline_costs += 10 * num_calls++;

				break;
			}
			case CEE_MONO_LDPTR:
				CHECK_STACK_OVF (1);
				CHECK_OPSIZE (6);
				token = read32 (ip + 2);
				NEW_PCONST (cfg, ins, mono_method_get_wrapper_data (method, token));
				ins->cil_code = ip;
				*sp++ = ins;
				ip += 6;
				inline_costs += 10 * num_calls++;
				/* Can't embed random pointers into AOT code */
				cfg->disable_aot = 1;
				break;
			case CEE_MONO_VTADDR:
				CHECK_STACK (1);
				--sp;
				MONO_INST_NEW (cfg, ins, OP_VTADDR);
				ins->cil_code = ip;
				ins->type = STACK_MP;
				ins->inst_left = *sp;
				*sp++ = ins;
				ip += 2;
				break;
			case CEE_MONO_NEWOBJ: {
				MonoInst *iargs [2];
				int temp;
				CHECK_STACK_OVF (1);
				CHECK_OPSIZE (6);
				token = read32 (ip + 2);
				klass = (MonoClass *)mono_method_get_wrapper_data (method, token);
				mono_class_init (klass);
				NEW_DOMAINCONST (cfg, iargs [0]);
				NEW_CLASSCONST (cfg, iargs [1], klass);
				temp = mono_emit_jit_icall (cfg, bblock, mono_object_new, iargs, ip);
				NEW_TEMPLOAD (cfg, *sp, temp);
				sp++;
				ip += 6;
				inline_costs += 10 * num_calls++;
				break;
			}
			case CEE_MONO_OBJADDR:
				CHECK_STACK (1);
				--sp;
				MONO_INST_NEW (cfg, ins, OP_OBJADDR);
				ins->cil_code = ip;
				ins->type = STACK_MP;
				ins->inst_left = *sp;
				*sp++ = ins;
				ip += 2;
				break;
			case CEE_MONO_LDNATIVEOBJ:
				CHECK_STACK (1);
				CHECK_OPSIZE (6);
				token = read32 (ip + 2);
				klass = mono_method_get_wrapper_data (method, token);
				g_assert (klass->valuetype);
				mono_class_init (klass);
				NEW_INDLOAD (cfg, ins, sp [-1], &klass->byval_arg);
				sp [-1] = ins;
				ip += 6;
				break;
			case CEE_MONO_RETOBJ:
				g_assert (cfg->ret);
				g_assert (mono_method_signature (method)->pinvoke); 
				CHECK_STACK (1);
				--sp;
				
				CHECK_OPSIZE (6);
				token = read32 (ip + 2);    
				klass = (MonoClass *)mono_method_get_wrapper_data (method, token);

				NEW_RETLOADA (cfg, ins);
				handle_stobj (cfg, bblock, ins, *sp, ip, klass, FALSE, TRUE);
				
				if (sp != stack_start)
					goto unverified;
				
				MONO_INST_NEW (cfg, ins, CEE_BR);
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
				MONO_INST_NEW (cfg, ins, (ip [1] == CEE_MONO_CISINST) ? OP_CISINST : OP_CCASTCLASS);
				ins->type = STACK_I4;
				ins->inst_left = *sp;
				ins->inst_newa_class = klass;
				ins->cil_code = ip;
				*sp++ = emit_tree (cfg, bblock, ins, ip + 6);
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
				NEW_CLASSCONST (cfg, ins, mono_method_get_wrapper_data (method, token));
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

				NEW_TEMPLOADA (cfg, addr, vtvar->inst_c0);
				addr->cil_code = ip;
				MONO_INST_NEW (cfg, ins, OP_ARGLIST);
				ins->cil_code = ip;
				ins->inst_left = addr;
				MONO_ADD_INS (bblock, ins);
				NEW_TEMPLOAD (cfg, ins, vtvar->inst_c0);
				ins->cil_code = ip;
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
				MONO_INST_NEW (cfg, cmp, 256 + ip [1]);
				
				MONO_INST_NEW (cfg, ins, cmp->opcode);
				sp -= 2;
				cmp->inst_i0 = sp [0];
				cmp->inst_i1 = sp [1];
				cmp->cil_code = ip;
				type_from_op (cmp);
				CHECK_TYPE (cmp);
				if ((sp [0]->type == STACK_I8) || ((sizeof (gpointer) == 8) && ((sp [0]->type == STACK_PTR) || (sp [0]->type == STACK_OBJ) || (sp [0]->type == STACK_MP))))
					cmp->opcode = OP_LCOMPARE;
				else
					cmp->opcode = OP_COMPARE;
				ins->cil_code = ip;
				ins->type = STACK_I4;
				ins->inst_i0 = cmp;
				*sp++ = ins;
				/* spill it to reduce the expression complexity
				 * and workaround bug 54209 
				 */
				if (cmp->inst_left->type == STACK_I8) {
					--sp;
					*sp++ = emit_tree (cfg, bblock, ins, ip + 2);
				}
				ip += 2;
				break;
			}
			case CEE_LDFTN: {
				MonoInst *argconst;
				int temp;

				CHECK_STACK_OVF (1);
				CHECK_OPSIZE (6);
				n = read32 (ip + 2);
				cmethod = mini_get_method (method, n, NULL, generic_context);
				if (!cmethod)
					goto load_error;
				mono_class_init (cmethod->klass);

				if (mono_use_security_manager) {
					check_linkdemand (cfg, method, cmethod, bblock, ip);
				}

				handle_loaded_temps (cfg, bblock, stack_start, sp);

				NEW_METHODCONST (cfg, argconst, cmethod);
				if (method->wrapper_type != MONO_WRAPPER_SYNCHRONIZED)
					temp = mono_emit_jit_icall (cfg, bblock, mono_ldftn, &argconst, ip);
				else
					temp = mono_emit_jit_icall (cfg, bblock, mono_ldftn_nosync, &argconst, ip);
				NEW_TEMPLOAD (cfg, *sp, temp);
				sp ++;
				
				ip += 6;
				inline_costs += 10 * num_calls++;
				break;
			}
			case CEE_LDVIRTFTN: {
				MonoInst *args [2];
				int temp;

				CHECK_STACK (1);
				CHECK_OPSIZE (6);
				n = read32 (ip + 2);
				cmethod = mini_get_method (method, n, NULL, generic_context);
				if (!cmethod)
					goto load_error;
				mono_class_init (cmethod->klass);

				if (mono_use_security_manager) {
					check_linkdemand (cfg, method, cmethod, bblock, ip);
				}

				handle_loaded_temps (cfg, bblock, stack_start, sp);

				--sp;
				args [0] = *sp;
				NEW_METHODCONST (cfg, args [1], cmethod);
				temp = mono_emit_jit_icall (cfg, bblock, mono_ldvirtfn, args, ip);
				NEW_TEMPLOAD (cfg, *sp, temp);
				sp ++;

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
				*sp++ = ins;
				ip += 4;
				break;
			case CEE_STARG:
				CHECK_STACK (1);
				--sp;
				handle_loaded_temps (cfg, bblock, stack_start, sp);
				CHECK_OPSIZE (4);
				n = read16 (ip + 2);
				CHECK_ARG (n);
				NEW_ARGSTORE (cfg, ins, n, *sp);
				ins->cil_code = ip;
				if (ins->opcode == CEE_STOBJ) {
					NEW_ARGLOADA (cfg, ins, n);
					handle_stobj (cfg, bblock, ins, *sp, ip, ins->klass, FALSE, FALSE);
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
				ins->cil_code = ip;
				*sp++ = ins;
				ip += 4;
				break;
			case CEE_LDLOCA:
				CHECK_STACK_OVF (1);
				CHECK_OPSIZE (4);
				n = read16 (ip + 2);
				CHECK_LOCAL (n);
				NEW_LOCLOADA (cfg, ins, n);
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
				handle_loaded_temps (cfg, bblock, stack_start, sp);
				NEW_LOCSTORE (cfg, ins, n, *sp);
				ins->cil_code = ip;
				if (ins->opcode == CEE_STOBJ) {
					NEW_LOCLOADA (cfg, ins, n);
					handle_stobj (cfg, bblock, ins, *sp, ip, ins->klass, FALSE, FALSE);
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
				MONO_INST_NEW (cfg, ins, OP_LOCALLOC);
				ins->inst_left = *sp;
				ins->cil_code = ip;
				ins->type = STACK_MP;

				cfg->flags |= MONO_CFG_HAS_ALLOCA;
				if (header->init_locals)
					ins->flags |= MONO_INST_INIT;

				*sp++ = ins;
				ip += 2;
				/* FIXME: set init flag if locals init is set in this method */
				break;
			case CEE_ENDFILTER: {
				MonoExceptionClause *clause, *nearest;
				int cc, nearest_num;

				CHECK_STACK (1);
				--sp;
				if ((sp != stack_start) || (sp [0]->type != STACK_I4)) 
					goto unverified;
				MONO_INST_NEW (cfg, ins, OP_ENDFILTER);
				ins->inst_left = *sp;
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
					MonoInst *store, *load;
					NEW_PCONST (cfg, load, NULL);
					load->cil_code = ip;
					load->type = STACK_OBJ;
					MONO_INST_NEW (cfg, store, CEE_STIND_REF);
					store->cil_code = ip;
					handle_loaded_temps (cfg, bblock, stack_start, sp);
					MONO_ADD_INS (bblock, store);
					store->inst_i0 = sp [0];
					store->inst_i1 = load;
				} else {
					handle_initobj (cfg, bblock, *sp, NULL, klass, stack_start, sp);
				}
				ip += 6;
				inline_costs += 1;
				break;
			case CEE_CONSTRAINED_:
				/* FIXME: implement */
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
				if ((cfg->opt & MONO_OPT_INTRINS) && (ip [1] == CEE_CPBLK) && (sp [2]->opcode == OP_ICONST) && ((n = sp [2]->inst_c0) <= sizeof (gpointer) * 5)) {
					MonoInst *copy;
					MONO_INST_NEW (cfg, copy, OP_MEMCPY);
					copy->inst_left = sp [0];
					copy->inst_right = sp [1];
					copy->cil_code = ip;
					copy->unused = n;
					MONO_ADD_INS (bblock, copy);
					ip += 2;
					break;
				}
				iargs [0] = sp [0];
				iargs [1] = sp [1];
				iargs [2] = sp [2];
				handle_loaded_temps (cfg, bblock, stack_start, sp);
				if (ip [1] == CEE_CPBLK) {
					MonoMethod *memcpy_method = get_memcpy_method ();
					mono_emit_method_call_spilled (cfg, bblock, memcpy_method, memcpy_method->signature, iargs, ip, NULL);
				} else {
					MonoMethod *memset_method = get_memset_method ();
					mono_emit_method_call_spilled (cfg, bblock, memset_method, memset_method->signature, iargs, ip, NULL);
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

				NEW_TEMPLOAD (cfg, load, mono_find_exvar_for_offset (cfg, handler_offset)->inst_c0);
				load->cil_code = ip;
				MONO_INST_NEW (cfg, ins, OP_RETHROW);
				ins->inst_left = load;
				ins->cil_code = ip;
				MONO_ADD_INS (bblock, ins);
				sp = stack_start;
				link_bblock (cfg, bblock, end_bblock);
				start_new_bblock = 1;
				ip += 2;
				mono_get_got_var (cfg);
				break;
			}
			case CEE_SIZEOF:
				CHECK_STACK_OVF (1);
				CHECK_OPSIZE (6);
				token = read32 (ip + 2);
				/* FIXXME: handle generics. */
				if (mono_metadata_token_table (token) == MONO_TABLE_TYPESPEC) {
					MonoType *type = mono_type_create_from_typespec (image, token);
					token = mono_type_size (type, &align);
				} else {
					MonoClass *klass = mono_class_get_full (image, token, generic_context);
					if (!klass)
						goto load_error;
					mono_class_init (klass);
					token = mono_class_value_size (klass, &align);
				}
				NEW_ICONST (cfg, ins, token);
				ins->cil_code = ip;
				*sp++= ins;
				ip += 6;
				break;
			case CEE_REFANYTYPE:
				CHECK_STACK (1);
				MONO_INST_NEW (cfg, ins, OP_REFANYTYPE);
				--sp;
				ins->type = STACK_MP;
				ins->inst_left = *sp;
				ins->type = STACK_VTYPE;
				ins->klass = mono_defaults.typehandle_class;
				ins->cil_code = ip;
				ip += 2;
				*sp++ = ins;
				break;
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
		
		if (! (get_domain = mono_arch_get_domain_intrinsic (cfg))) {
			MonoCallInst *call;
			
			MONO_INST_NEW_CALL (cfg, call, CEE_CALL);
			call->signature = helper_sig_domain_get;
			call->inst.type = STACK_PTR;
			call->fptr = mono_domain_get;
			get_domain = (MonoInst*)call;
		}
		
		NEW_TEMPSTORE (cfg, store, cfg->domainvar->inst_c0, get_domain);
		MONO_ADD_INS (init_localsbb, store);
	}

	if (cfg->method == method && cfg->got_var)
		mono_emit_load_got_addr (cfg);

	if (header->init_locals) {
		MonoInst *store;
		for (i = 0; i < header->num_locals; ++i) {
			MonoType *ptype = header->locals [i];
			int t = ptype->type;
			if (t == MONO_TYPE_VALUETYPE && ptype->data.klass->enumtype)
				t = ptype->data.klass->enum_basetype->type;
			if (ptype->byref) {
				NEW_PCONST (cfg, ins, NULL);
				NEW_LOCSTORE (cfg, store, i, ins);
				MONO_ADD_INS (init_localsbb, store);
			} else if (t >= MONO_TYPE_BOOLEAN && t <= MONO_TYPE_U4) {
				NEW_ICONST (cfg, ins, 0);
				NEW_LOCSTORE (cfg, store, i, ins);
				MONO_ADD_INS (init_localsbb, store);
			} else if (t == MONO_TYPE_I8 || t == MONO_TYPE_U8) {
				MONO_INST_NEW (cfg, ins, OP_I8CONST);
				ins->type = STACK_I8;
				ins->inst_l = 0;
				NEW_LOCSTORE (cfg, store, i, ins);
				MONO_ADD_INS (init_localsbb, store);
			} else if (t == MONO_TYPE_R4 || t == MONO_TYPE_R8) {
				MONO_INST_NEW (cfg, ins, OP_R8CONST);
				ins->type = STACK_R8;
				ins->inst_p0 = (void*)&r8_0;
				NEW_LOCSTORE (cfg, store, i, ins);
				MONO_ADD_INS (init_localsbb, store);
			} else if ((t == MONO_TYPE_VALUETYPE) || (t == MONO_TYPE_TYPEDBYREF) ||
				   ((t == MONO_TYPE_GENERICINST) && mono_metadata_generic_class_is_valuetype (ptype->data.generic_class))) {
				NEW_LOCLOADA (cfg, ins, i);
				handle_initobj (cfg, init_localsbb, ins, NULL, mono_class_from_mono_type (ptype), NULL, NULL);
			} else {
				NEW_PCONST (cfg, ins, NULL);
				NEW_LOCSTORE (cfg, store, i, ins);
				MONO_ADD_INS (init_localsbb, store);
			}
		}
	}

	/* resolve backward branches in the middle of an existing basic block */
	for (tmp = bb_recheck; tmp; tmp = tmp->next) {
		bblock = tmp->data;
		/*g_print ("need recheck in %s at IL_%04x\n", method->name, bblock->cil_code - header->code);*/
		tblock = find_previous (bbhash, start_bblock, bblock->cil_code);
		if (tblock != start_bblock) {
			int l;
			split_bblock (cfg, tblock, bblock);
			l = bblock->cil_code - header->code;
			bblock->cil_length = tblock->cil_length - l;
			tblock->cil_length = l;
		} else {
			g_print ("recheck failed.\n");
		}
	}

	if (cfg->method == method) {
		MonoBasicBlock *bb;
		for (bb = cfg->bb_entry; bb; bb = bb->next_bb) {
			bb->region = mono_find_block_region (cfg, bb->real_offset);
			if (cfg->spvars)
				mono_create_spvar_for_region (cfg, bb->region);
			if (cfg->verbose_level > 2)
				g_print ("REGION BB%d IL_%04x ID_%08X\n", bb->block_num, bb->real_offset, bb->region);
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

void
mono_print_tree (MonoInst *tree) {
	int arity;

	if (!tree)
		return;

	arity = mono_burg_arity [tree->opcode];

	printf (" %s%s", arity?"(":"",  mono_inst_name (tree->opcode));

	switch (tree->opcode) {
	case OP_ICONST:
		printf ("[%d]", (int)tree->inst_c0);
		break;
	case OP_I8CONST:
		printf ("[%lld]", (long long)tree->inst_l);
		break;
	case OP_R8CONST:
		printf ("[%f]", *(double*)tree->inst_p0);
		break;
	case OP_R4CONST:
		printf ("[%f]", *(float*)tree->inst_p0);
		break;
	case OP_ARG:
	case OP_LOCAL:
		printf ("[%d]", (int)tree->inst_c0);
		break;
	case OP_REGOFFSET:
		if (tree->inst_offset < 0)
			printf ("[-0x%x(%s)]", (int)(-tree->inst_offset), mono_arch_regname (tree->inst_basereg));
		else
			printf ("[0x%x(%s)]", (int)(tree->inst_offset), mono_arch_regname (tree->inst_basereg));
		break;
	case OP_REGVAR:
		printf ("[%s]", mono_arch_regname (tree->dreg));
		break;
	case CEE_NEWARR:
		printf ("[%s]",  tree->inst_newa_class->name);
		mono_print_tree (tree->inst_newa_len);
		break;
	case CEE_CALL:
	case CEE_CALLVIRT:
	case OP_FCALL:
	case OP_FCALLVIRT:
	case OP_LCALL:
	case OP_LCALLVIRT:
	case OP_VCALL:
	case OP_VCALLVIRT:
	case OP_VOIDCALL:
	case OP_VOIDCALLVIRT: {
		MonoCallInst *call = (MonoCallInst*)tree;
		if (call->method)
			printf ("[%s]", call->method->name);
		else if (call->fptr) {
			MonoJitICallInfo *info = mono_find_jit_icall_by_addr (call->fptr);
			if (info)
				printf ("[%s]", info->name);
		}
		break;
	}
	case OP_PHI: {
		int i;
		printf ("[%d (", (int)tree->inst_c0);
		for (i = 0; i < tree->inst_phi_args [0]; i++) {
			if (i)
				printf (", ");
			printf ("%d", tree->inst_phi_args [i + 1]);
		}
		printf (")]");
		break;
	}
	case OP_RENAME:
	case OP_RETARG:
	case CEE_NOP:
	case CEE_JMP:
	case CEE_BREAK:
		break;
	case OP_LOAD_MEMBASE:
	case OP_LOADI4_MEMBASE:
	case OP_LOADU4_MEMBASE:
	case OP_LOADU1_MEMBASE:
	case OP_LOADI1_MEMBASE:
	case OP_LOADU2_MEMBASE:
	case OP_LOADI2_MEMBASE:
		printf ("[%s] <- [%s + 0x%x]", mono_arch_regname (tree->dreg), mono_arch_regname (tree->inst_basereg), (int)tree->inst_offset);
		break;
	case CEE_BR:
	case OP_CALL_HANDLER:
		printf ("[B%d]", tree->inst_target_bb->block_num);
		break;
	case CEE_SWITCH:
	case CEE_ISINST:
	case CEE_CASTCLASS:
	case OP_OUTARG:
	case OP_CALL_REG:
	case OP_FCALL_REG:
	case OP_LCALL_REG:
	case OP_VCALL_REG:
	case OP_VOIDCALL_REG:
		mono_print_tree (tree->inst_left);
		break;
	case CEE_BNE_UN:
	case CEE_BEQ:
	case CEE_BLT:
	case CEE_BLT_UN:
	case CEE_BGT:
	case CEE_BGT_UN:
	case CEE_BGE:
	case CEE_BGE_UN:
	case CEE_BLE:
	case CEE_BLE_UN:
		printf ("[B%dB%d]", tree->inst_true_bb->block_num, tree->inst_false_bb->block_num);
		mono_print_tree (tree->inst_left);
		break;
	default:
		if (!mono_arch_print_tree(tree, arity)) {
			if (arity) {
				mono_print_tree (tree->inst_left);
				if (arity > 1)
					mono_print_tree (tree->inst_right);
			}
		}
		break;
	}

	if (arity)
		printf (")");
}
