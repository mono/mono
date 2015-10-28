/*
 * mini.c: The new Mono code generator.
 *
 * Authors:
 *   Paolo Molaro (lupus@ximian.com)
 *   Dietmar Maurer (dietmar@ximian.com)
 *
 * Copyright 2002-2003 Ximian, Inc.
 * Copyright 2003-2010 Novell, Inc.
 * Copyright 2011 Xamarin, Inc (http://www.xamarin.com)
 */

#include <config.h>
#ifdef HAVE_ALLOCA_H
#include <alloca.h>
#endif
#ifdef HAVE_UNISTD_H
#include <unistd.h>
#endif
#include <math.h>
#ifdef HAVE_SYS_TIME_H
#include <sys/time.h>
#endif

#include <mono/utils/memcheck.h>

#include <mono/metadata/assembly.h>
#include <mono/metadata/loader.h>
#include <mono/metadata/tabledefs.h>
#include <mono/metadata/class.h>
#include <mono/metadata/object.h>
#include <mono/metadata/tokentype.h>
#include <mono/metadata/tabledefs.h>
#include <mono/metadata/threads.h>
#include <mono/metadata/appdomain.h>
#include <mono/metadata/debug-helpers.h>
#include "mono/metadata/profiler.h"
#include <mono/metadata/profiler-private.h>
#include <mono/metadata/mono-config.h>
#include <mono/metadata/environment.h>
#include <mono/metadata/mono-debug.h>
#include <mono/metadata/gc-internal.h>
#include <mono/metadata/threads-types.h>
#include <mono/metadata/verify.h>
#include <mono/metadata/verify-internals.h>
#include <mono/metadata/mempool-internals.h>
#include <mono/metadata/attach.h>
#include <mono/metadata/runtime.h>
#include <mono/utils/mono-math.h>
#include <mono/utils/mono-compiler.h>
#include <mono/utils/mono-counters.h>
#include <mono/utils/mono-error-internals.h>
#include <mono/utils/mono-logger-internal.h>
#include <mono/utils/mono-mmap.h>
#include <mono/utils/mono-path.h>
#include <mono/utils/mono-tls.h>
#include <mono/utils/mono-hwcap.h>
#include <mono/utils/dtrace.h>
#include <mono/utils/mono-threads.h>
#include <mono/io-layer/io-layer.h>

#include "mini.h"
#include "seq-points.h"
#include "tasklets.h"
#include <string.h>
#include <ctype.h>
#include "trace.h"
#include "version.h"
#include "ir-emit.h"

#include "jit-icalls.h"

#include "mini-gc.h"
#include "debugger-agent.h"

MonoTraceSpec *mono_jit_trace_calls;
MonoMethodDesc *mono_inject_async_exc_method;
int mono_inject_async_exc_pos;
MonoMethodDesc *mono_break_at_bb_method;
int mono_break_at_bb_bb_num;
gboolean mono_do_x86_stack_align = TRUE;
gboolean mono_using_xdebug;

#define mono_jit_lock() mono_mutex_lock (&jit_mutex)
#define mono_jit_unlock() mono_mutex_unlock (&jit_mutex)
static mono_mutex_t jit_mutex;

#ifndef DISABLE_JIT

gpointer
mono_realloc_native_code (MonoCompile *cfg)
{
#if defined(__default_codegen__)
	return g_realloc (cfg->native_code, cfg->code_size);
#elif defined(__native_client_codegen__)
	guint old_padding;
	gpointer native_code;
	guint alignment_check;

	/* Save the old alignment offset so we can re-align after the realloc. */
	old_padding = (guint)(cfg->native_code - cfg->native_code_alloc);
	cfg->code_size = NACL_BUNDLE_ALIGN_UP (cfg->code_size);

	cfg->native_code_alloc = g_realloc ( cfg->native_code_alloc,
										 cfg->code_size + kNaClAlignment );

	/* Align native_code to next nearest kNaClAlignment byte. */
	native_code = (guint)cfg->native_code_alloc + kNaClAlignment;
	native_code = (guint)native_code & ~kNaClAlignmentMask;

	/* Shift the data to be 32-byte aligned again. */
	memmove (native_code, cfg->native_code_alloc + old_padding, cfg->code_size);

	alignment_check = (guint)native_code & kNaClAlignmentMask;
	g_assert (alignment_check == 0);
	return native_code;
#else
	g_assert_not_reached ();
	return cfg->native_code;
#endif
}

#ifdef __native_client_codegen__

/* Prevent instructions from straddling a 32-byte alignment boundary.   */
/* Instructions longer than 32 bytes must be aligned internally.        */
/* IN: pcode, instlen                                                   */
/* OUT: pcode                                                           */
void mono_nacl_align_inst(guint8 **pcode, int instlen) {
  int space_in_block;

  space_in_block = kNaClAlignment - ((uintptr_t)(*pcode) & kNaClAlignmentMask);

  if (G_UNLIKELY (instlen >= kNaClAlignment)) {
    g_assert_not_reached();
  } else if (instlen > space_in_block) {
    *pcode = mono_arch_nacl_pad(*pcode, space_in_block);
  }
}

/* Move emitted call sequence to the end of a kNaClAlignment-byte block.  */
/* IN: start    pointer to start of call sequence                         */
/* IN: pcode    pointer to end of call sequence (current "IP")            */
/* OUT: start   pointer to the start of the call sequence after padding   */
/* OUT: pcode   pointer to the end of the call sequence after padding     */
void mono_nacl_align_call(guint8 **start, guint8 **pcode) {
  const size_t MAX_NACL_CALL_LENGTH = kNaClAlignment;
  guint8 copy_of_call[MAX_NACL_CALL_LENGTH];
  guint8 *temp;

  const size_t length = (size_t)((*pcode)-(*start));
  g_assert(length < MAX_NACL_CALL_LENGTH);

  memcpy(copy_of_call, *start, length);
  temp = mono_nacl_pad_call(*start, (guint8)length);
  memcpy(temp, copy_of_call, length);
  (*start) = temp;
  (*pcode) = temp + length;
}

/* mono_nacl_pad_call(): Insert padding for Native Client call instructions */
/*    code     pointer to buffer for emitting code                          */
/*    ilength  length of call instruction                                   */
guint8 *mono_nacl_pad_call(guint8 *code, guint8 ilength) {
  int freeSpaceInBlock = kNaClAlignment - ((uintptr_t)code & kNaClAlignmentMask);
  int padding = freeSpaceInBlock - ilength;

  if (padding < 0) {
    /* There isn't enough space in this block for the instruction. */
    /* Fill this block and start a new one.                        */
    code = mono_arch_nacl_pad(code, freeSpaceInBlock);
    freeSpaceInBlock = kNaClAlignment;
    padding = freeSpaceInBlock - ilength;
  }
  g_assert(ilength > 0);
  g_assert(padding >= 0);
  g_assert(padding < kNaClAlignment);
  if (0 == padding) return code;
  return mono_arch_nacl_pad(code, padding);
}

guint8 *mono_nacl_align(guint8 *code) {
  int padding = kNaClAlignment - ((uintptr_t)code & kNaClAlignmentMask);
  if (padding != kNaClAlignment) code = mono_arch_nacl_pad(code, padding);
  return code;
}

void mono_nacl_fix_patches(const guint8 *code, MonoJumpInfo *ji)
{
#ifndef USE_JUMP_TABLES
  MonoJumpInfo *patch_info;
  for (patch_info = ji; patch_info; patch_info = patch_info->next) {
    unsigned char *ip = patch_info->ip.i + code;
    ip = mono_arch_nacl_skip_nops(ip);
    patch_info->ip.i = ip - code;
  }
#endif
}
#endif  /* __native_client_codegen__ */

#ifdef USE_JUMP_TABLES

#define DEFAULT_JUMPTABLE_CHUNK_ELEMENTS 128

typedef struct MonoJumpTableChunk {
	guint32 total;
	guint32 active;
	struct MonoJumpTableChunk *previous;
	/* gpointer entries[total]; */
} MonoJumpTableChunk;

static MonoJumpTableChunk* g_jumptable;
#define mono_jumptable_lock() mono_mutex_lock (&jumptable_mutex)
#define mono_jumptable_unlock() mono_mutex_unlock (&jumptable_mutex)
static mono_mutex_t jumptable_mutex;

static  MonoJumpTableChunk*
mono_create_jumptable_chunk (guint32 max_entries)
{
	guint32 size = sizeof (MonoJumpTableChunk) + max_entries * sizeof(gpointer);
	MonoJumpTableChunk *chunk = (MonoJumpTableChunk*) g_new0 (guchar, size);
	chunk->total = max_entries;
	return chunk;
}

void
mono_jumptable_init (void)
{
	if (g_jumptable == NULL) {
		mono_mutex_init_recursive (&jumptable_mutex);
		g_jumptable = mono_create_jumptable_chunk (DEFAULT_JUMPTABLE_CHUNK_ELEMENTS);
	}
}

gpointer*
mono_jumptable_add_entry (void)
{
	return mono_jumptable_add_entries (1);
}

gpointer*
mono_jumptable_add_entries (guint32 entries)
{
	guint32 index;
	gpointer *result;

	mono_jumptable_init ();
	mono_jumptable_lock ();
	index = g_jumptable->active;
	if (index + entries >= g_jumptable->total) {
		/*
		 * Grow jumptable, by adding one more chunk.
		 * We cannot realloc jumptable, as there could be pointers
		 * to existing jump table entries in the code, so instead
		 * we just add one more chunk.
		 */
		guint32 max_entries = entries;
		MonoJumpTableChunk *new_chunk;

		if (max_entries < DEFAULT_JUMPTABLE_CHUNK_ELEMENTS)
			max_entries = DEFAULT_JUMPTABLE_CHUNK_ELEMENTS;
		new_chunk = mono_create_jumptable_chunk (max_entries);
		/* Link old jumptable, so that we could free it up later. */
		new_chunk->previous = g_jumptable;
		g_jumptable = new_chunk;
		index = 0;
	}
	g_jumptable->active = index + entries;
	result = (gpointer*)((guchar*)g_jumptable + sizeof(MonoJumpTableChunk)) + index;
	mono_jumptable_unlock();

	return result;
}

void
mono_jumptable_cleanup (void)
{
	if (g_jumptable) {
		MonoJumpTableChunk *current = g_jumptable, *prev;
		while (current != NULL) {
			prev = current->previous;
			g_free (current);
			current = prev;
		}
		g_jumptable = NULL;
		mono_mutex_destroy (&jumptable_mutex);
	}
}

gpointer*
mono_jumptable_get_entry (guint8 *code_ptr)
{
	return mono_arch_jumptable_entry_from_code (code_ptr);
}

#endif /* USE_JUMP_TABLES */

typedef struct {
	MonoExceptionClause *clause;
	MonoBasicBlock *basic_block;
	int start_offset;
} TryBlockHole;

/**
 * mono_emit_unwind_op:
 *
 *   Add an unwind op with the given parameters for the list of unwind ops stored in
 * cfg->unwind_ops.
 */
void
mono_emit_unwind_op (MonoCompile *cfg, int when, int tag, int reg, int val)
{
	MonoUnwindOp *op = mono_mempool_alloc0 (cfg->mempool, sizeof (MonoUnwindOp));

	op->op = tag;
	op->reg = reg;
	op->val = val;
	op->when = when;
	
	cfg->unwind_ops = g_slist_append_mempool (cfg->mempool, cfg->unwind_ops, op);
	if (cfg->verbose_level > 1) {
		switch (tag) {
		case DW_CFA_def_cfa:
			printf ("CFA: [%x] def_cfa: %s+0x%x\n", when, mono_arch_regname (reg), val);
			break;
		case DW_CFA_def_cfa_register:
			printf ("CFA: [%x] def_cfa_reg: %s\n", when, mono_arch_regname (reg));
			break;
		case DW_CFA_def_cfa_offset:
			printf ("CFA: [%x] def_cfa_offset: 0x%x\n", when, val);
			break;
		case DW_CFA_offset:
			printf ("CFA: [%x] offset: %s at cfa-0x%x\n", when, mono_arch_regname (reg), -val);
			break;
		}
	}
}

#define MONO_INIT_VARINFO(vi,id) do { \
	(vi)->range.first_use.pos.bid = 0xffff; \
	(vi)->reg = -1; \
        (vi)->idx = (id); \
} while (0)

/**
 * mono_unlink_bblock:
 *
 *   Unlink two basic blocks.
 */
void
mono_unlink_bblock (MonoCompile *cfg, MonoBasicBlock *from, MonoBasicBlock* to)
{
	int i, pos;
	gboolean found;

	found = FALSE;
	for (i = 0; i < from->out_count; ++i) {
		if (to == from->out_bb [i]) {
			found = TRUE;
			break;
		}
	}
	if (found) {
		pos = 0;
		for (i = 0; i < from->out_count; ++i) {
			if (from->out_bb [i] != to)
				from->out_bb [pos ++] = from->out_bb [i];
		}
		g_assert (pos == from->out_count - 1);
		from->out_count--;
	}

	found = FALSE;
	for (i = 0; i < to->in_count; ++i) {
		if (from == to->in_bb [i]) {
			found = TRUE;
			break;
		}
	}
	if (found) {
		pos = 0;
		for (i = 0; i < to->in_count; ++i) {
			if (to->in_bb [i] != from)
				to->in_bb [pos ++] = to->in_bb [i];
		}
		g_assert (pos == to->in_count - 1);
		to->in_count--;
	}
}

/*
 * mono_bblocks_linked:
 *
 *   Return whenever BB1 and BB2 are linked in the CFG.
 */
gboolean
mono_bblocks_linked (MonoBasicBlock *bb1, MonoBasicBlock *bb2)
{
	int i;

	for (i = 0; i < bb1->out_count; ++i) {
		if (bb1->out_bb [i] == bb2)
			return TRUE;
	}

	return FALSE;
}

static int
mono_find_block_region_notry (MonoCompile *cfg, int offset)
{
	MonoMethodHeader *header = cfg->header;
	MonoExceptionClause *clause;
	int i;

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

	return -1;
}

/*
 * mono_get_block_region_notry:
 *
 *   Return the region corresponding to REGION, ignoring try clauses nested inside
 * finally clauses.
 */
int
mono_get_block_region_notry (MonoCompile *cfg, int region)
{
	if ((region & (0xf << 4)) == MONO_REGION_TRY) {
		MonoMethodHeader *header = cfg->header;
		
		/*
		 * This can happen if a try clause is nested inside a finally clause.
		 */
		int clause_index = (region >> 8) - 1;
		g_assert (clause_index >= 0 && clause_index < header->num_clauses);
		
		region = mono_find_block_region_notry (cfg, header->clauses [clause_index].try_offset);
	}

	return region;
}

MonoInst *
mono_find_spvar_for_region (MonoCompile *cfg, int region)
{
	region = mono_get_block_region_notry (cfg, region);

	return g_hash_table_lookup (cfg->spvars, GINT_TO_POINTER (region));
}

static void
df_visit (MonoBasicBlock *start, int *dfn, MonoBasicBlock **array)
{
	int i;

	array [*dfn] = start;
	/* g_print ("visit %d at %p (BB%ld)\n", *dfn, start->cil_code, start->block_num); */
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

guint32
mono_reverse_branch_op (guint32 opcode)
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
mono_type_to_store_membase (MonoCompile *cfg, MonoType *type)
{
	type = mini_get_underlying_type (type);

handle_enum:
	switch (type->type) {
	case MONO_TYPE_I1:
	case MONO_TYPE_U1:
		return OP_STOREI1_MEMBASE_REG;
	case MONO_TYPE_I2:
	case MONO_TYPE_U2:
		return OP_STOREI2_MEMBASE_REG;
	case MONO_TYPE_I4:
	case MONO_TYPE_U4:
		return OP_STOREI4_MEMBASE_REG;
	case MONO_TYPE_I:
	case MONO_TYPE_U:
	case MONO_TYPE_PTR:
	case MONO_TYPE_FNPTR:
		return OP_STORE_MEMBASE_REG;
	case MONO_TYPE_CLASS:
	case MONO_TYPE_STRING:
	case MONO_TYPE_OBJECT:
	case MONO_TYPE_SZARRAY:
	case MONO_TYPE_ARRAY:    
		return OP_STORE_MEMBASE_REG;
	case MONO_TYPE_I8:
	case MONO_TYPE_U8:
		return OP_STOREI8_MEMBASE_REG;
	case MONO_TYPE_R4:
		return OP_STORER4_MEMBASE_REG;
	case MONO_TYPE_R8:
		return OP_STORER8_MEMBASE_REG;
	case MONO_TYPE_VALUETYPE:
		if (type->data.klass->enumtype) {
			type = mono_class_enum_basetype (type->data.klass);
			goto handle_enum;
		}
		if (MONO_CLASS_IS_SIMD (cfg, mono_class_from_mono_type (type)))
			return OP_STOREX_MEMBASE;
		return OP_STOREV_MEMBASE;
	case MONO_TYPE_TYPEDBYREF:
		return OP_STOREV_MEMBASE;
	case MONO_TYPE_GENERICINST:
		type = &type->data.generic_class->container_class->byval_arg;
		goto handle_enum;
	case MONO_TYPE_VAR:
	case MONO_TYPE_MVAR:
		g_assert (mini_type_var_is_vt (type));
		return OP_STOREV_MEMBASE;
	default:
		g_error ("unknown type 0x%02x in type_to_store_membase", type->type);
	}
	return -1;
}

guint
mono_type_to_load_membase (MonoCompile *cfg, MonoType *type)
{
	type = mini_get_underlying_type (type);

	switch (type->type) {
	case MONO_TYPE_I1:
		return OP_LOADI1_MEMBASE;
	case MONO_TYPE_U1:
		return OP_LOADU1_MEMBASE;
	case MONO_TYPE_I2:
		return OP_LOADI2_MEMBASE;
	case MONO_TYPE_U2:
		return OP_LOADU2_MEMBASE;
	case MONO_TYPE_I4:
		return OP_LOADI4_MEMBASE;
	case MONO_TYPE_U4:
		return OP_LOADU4_MEMBASE;
	case MONO_TYPE_I:
	case MONO_TYPE_U:
	case MONO_TYPE_PTR:
	case MONO_TYPE_FNPTR:
		return OP_LOAD_MEMBASE;
	case MONO_TYPE_CLASS:
	case MONO_TYPE_STRING:
	case MONO_TYPE_OBJECT:
	case MONO_TYPE_SZARRAY:
	case MONO_TYPE_ARRAY:    
		return OP_LOAD_MEMBASE;
	case MONO_TYPE_I8:
	case MONO_TYPE_U8:
		return OP_LOADI8_MEMBASE;
	case MONO_TYPE_R4:
		return OP_LOADR4_MEMBASE;
	case MONO_TYPE_R8:
		return OP_LOADR8_MEMBASE;
	case MONO_TYPE_VALUETYPE:
		if (MONO_CLASS_IS_SIMD (cfg, mono_class_from_mono_type (type)))
			return OP_LOADX_MEMBASE;
	case MONO_TYPE_TYPEDBYREF:
		return OP_LOADV_MEMBASE;
	case MONO_TYPE_GENERICINST:
		if (mono_type_generic_inst_is_valuetype (type))
			return OP_LOADV_MEMBASE;
		else
			return OP_LOAD_MEMBASE;
		break;
	case MONO_TYPE_VAR:
	case MONO_TYPE_MVAR:
		g_assert (cfg->gshared);
		g_assert (mini_type_var_is_vt (type));
		return OP_LOADV_MEMBASE;
	default:
		g_error ("unknown type 0x%02x in type_to_load_membase", type->type);
	}
	return -1;
}

guint
mini_type_to_stind (MonoCompile* cfg, MonoType *type)
{
	type = mini_get_underlying_type (type);
	if (cfg->gshared && !type->byref && (type->type == MONO_TYPE_VAR || type->type == MONO_TYPE_MVAR)) {
		g_assert (mini_type_var_is_vt (type));
		return CEE_STOBJ;
	}
	return mono_type_to_stind (type);
}

int
mono_op_imm_to_op (int opcode)
{
	switch (opcode) {
	case OP_ADD_IMM:
#if SIZEOF_REGISTER == 4
		return OP_IADD;
#else
		return OP_LADD;
#endif
	case OP_IADD_IMM:
		return OP_IADD;
	case OP_LADD_IMM:
		return OP_LADD;
	case OP_ISUB_IMM:
		return OP_ISUB;
	case OP_LSUB_IMM:
		return OP_LSUB;
	case OP_IMUL_IMM:
		return OP_IMUL;
	case OP_AND_IMM:
#if SIZEOF_REGISTER == 4
		return OP_IAND;
#else
		return OP_LAND;
#endif
	case OP_OR_IMM:
#if SIZEOF_REGISTER == 4
		return OP_IOR;
#else
		return OP_LOR;
#endif
	case OP_XOR_IMM:
#if SIZEOF_REGISTER == 4
		return OP_IXOR;
#else
		return OP_LXOR;
#endif
	case OP_IAND_IMM:
		return OP_IAND;
	case OP_LAND_IMM:
		return OP_LAND;
	case OP_IOR_IMM:
		return OP_IOR;
	case OP_LOR_IMM:
		return OP_LOR;
	case OP_IXOR_IMM:
		return OP_IXOR;
	case OP_LXOR_IMM:
		return OP_LXOR;
	case OP_ISHL_IMM:
		return OP_ISHL;
	case OP_LSHL_IMM:
		return OP_LSHL;
	case OP_ISHR_IMM:
		return OP_ISHR;
	case OP_LSHR_IMM:
		return OP_LSHR;
	case OP_ISHR_UN_IMM:
		return OP_ISHR_UN;
	case OP_LSHR_UN_IMM:
		return OP_LSHR_UN;
	case OP_IDIV_IMM:
		return OP_IDIV;
	case OP_IDIV_UN_IMM:
		return OP_IDIV_UN;
	case OP_IREM_UN_IMM:
		return OP_IREM_UN;
	case OP_IREM_IMM:
		return OP_IREM;
	case OP_LREM_IMM:
		return OP_LREM;
	case OP_DIV_IMM:
#if SIZEOF_REGISTER == 4
		return OP_IDIV;
#else
		return OP_LDIV;
#endif
	case OP_REM_IMM:
#if SIZEOF_REGISTER == 4
		return OP_IREM;
#else
		return OP_LREM;
#endif
	case OP_ADDCC_IMM:
		return OP_ADDCC;
	case OP_ADC_IMM:
		return OP_ADC;
	case OP_SUBCC_IMM:
		return OP_SUBCC;
	case OP_SBB_IMM:
		return OP_SBB;
	case OP_IADC_IMM:
		return OP_IADC;
	case OP_ISBB_IMM:
		return OP_ISBB;
	case OP_COMPARE_IMM:
		return OP_COMPARE;
	case OP_ICOMPARE_IMM:
		return OP_ICOMPARE;
	case OP_LOCALLOC_IMM:
		return OP_LOCALLOC;
	default:
		printf ("%s\n", mono_inst_name (opcode));
		g_assert_not_reached ();
		return -1;
	}
}

/*
 * mono_decompose_op_imm:
 *
 *   Replace the OP_.._IMM INS with its non IMM variant.
 */
void
mono_decompose_op_imm (MonoCompile *cfg, MonoBasicBlock *bb, MonoInst *ins)
{
	MonoInst *temp;

	MONO_INST_NEW (cfg, temp, OP_ICONST);
	temp->inst_c0 = ins->inst_imm;
	temp->dreg = mono_alloc_ireg (cfg);
	mono_bblock_insert_before_ins (bb, ins, temp);
	ins->opcode = mono_op_imm_to_op (ins->opcode);
	if (ins->opcode == OP_LOCALLOC)
		ins->sreg1 = temp->dreg;
	else
		ins->sreg2 = temp->dreg;

	bb->max_vreg = MAX (bb->max_vreg, cfg->next_vreg);
}

static void
set_vreg_to_inst (MonoCompile *cfg, int vreg, MonoInst *inst)
{
	if (vreg >= cfg->vreg_to_inst_len) {
		MonoInst **tmp = cfg->vreg_to_inst;
		int size = cfg->vreg_to_inst_len;

		while (vreg >= cfg->vreg_to_inst_len)
			cfg->vreg_to_inst_len = cfg->vreg_to_inst_len ? cfg->vreg_to_inst_len * 2 : 32;
		cfg->vreg_to_inst = mono_mempool_alloc0 (cfg->mempool, sizeof (MonoInst*) * cfg->vreg_to_inst_len);
		if (size)
			memcpy (cfg->vreg_to_inst, tmp, size * sizeof (MonoInst*));
	}
	cfg->vreg_to_inst [vreg] = inst;
}

#define mono_type_is_long(type) (!(type)->byref && ((mono_type_get_underlying_type (type)->type == MONO_TYPE_I8) || (mono_type_get_underlying_type (type)->type == MONO_TYPE_U8)))
#define mono_type_is_float(type) (!(type)->byref && (((type)->type == MONO_TYPE_R8) || ((type)->type == MONO_TYPE_R4)))

MonoInst*
mono_compile_create_var_for_vreg (MonoCompile *cfg, MonoType *type, int opcode, int vreg)
{
	MonoInst *inst;
	int num = cfg->num_varinfo;
	gboolean regpair;

	type = mini_get_underlying_type (type);

	if ((num + 1) >= cfg->varinfo_count) {
		int orig_count = cfg->varinfo_count;
		cfg->varinfo_count = cfg->varinfo_count ? (cfg->varinfo_count * 2) : 32;
		cfg->varinfo = (MonoInst **)g_realloc (cfg->varinfo, sizeof (MonoInst*) * cfg->varinfo_count);
		cfg->vars = (MonoMethodVar *)g_realloc (cfg->vars, sizeof (MonoMethodVar) * cfg->varinfo_count);
		memset (&cfg->vars [orig_count], 0, (cfg->varinfo_count - orig_count) * sizeof (MonoMethodVar));
	}

	cfg->stat_allocate_var++;

	MONO_INST_NEW (cfg, inst, opcode);
	inst->inst_c0 = num;
	inst->inst_vtype = type;
	inst->klass = mono_class_from_mono_type (type);
	type_to_eval_stack_type (cfg, type, inst);
	/* if set to 1 the variable is native */
	inst->backend.is_pinvoke = 0;
	inst->dreg = vreg;

	if (inst->klass->exception_type)
		mono_cfg_set_exception (cfg, MONO_EXCEPTION_TYPE_LOAD);

	if (cfg->compute_gc_maps) {
		if (type->byref) {
			mono_mark_vreg_as_mp (cfg, vreg);
		} else {
			if ((MONO_TYPE_ISSTRUCT (type) && inst->klass->has_references) || mini_type_is_reference (type)) {
				inst->flags |= MONO_INST_GC_TRACK;
				mono_mark_vreg_as_ref (cfg, vreg);
			}
		}
	}
	
	cfg->varinfo [num] = inst;

	MONO_INIT_VARINFO (&cfg->vars [num], num);
	MONO_VARINFO (cfg, num)->vreg = vreg;

	if (vreg != -1)
		set_vreg_to_inst (cfg, vreg, inst);

#if SIZEOF_REGISTER == 4
	if (mono_arch_is_soft_float ()) {
		regpair = mono_type_is_long (type) || mono_type_is_float (type);
	} else {
		regpair = mono_type_is_long (type);
	}
#else
	regpair = FALSE;
#endif

	if (regpair) {
		MonoInst *tree;

		/* 
		 * These two cannot be allocated using create_var_for_vreg since that would
		 * put it into the cfg->varinfo array, confusing many parts of the JIT.
		 */

		/* 
		 * Set flags to VOLATILE so SSA skips it.
		 */

		if (cfg->verbose_level >= 4) {
			printf ("  Create LVAR R%d (R%d, R%d)\n", inst->dreg, inst->dreg + 1, inst->dreg + 2);
		}

		if (mono_arch_is_soft_float () && cfg->opt & MONO_OPT_SSA) {
			if (mono_type_is_float (type))
				inst->flags = MONO_INST_VOLATILE;
		}

		/* Allocate a dummy MonoInst for the first vreg */
		MONO_INST_NEW (cfg, tree, OP_LOCAL);
		tree->dreg = inst->dreg + 1;
		if (cfg->opt & MONO_OPT_SSA)
			tree->flags = MONO_INST_VOLATILE;
		tree->inst_c0 = num;
		tree->type = STACK_I4;
		tree->inst_vtype = &mono_defaults.int32_class->byval_arg;
		tree->klass = mono_class_from_mono_type (tree->inst_vtype);

		set_vreg_to_inst (cfg, inst->dreg + 1, tree);

		/* Allocate a dummy MonoInst for the second vreg */
		MONO_INST_NEW (cfg, tree, OP_LOCAL);
		tree->dreg = inst->dreg + 2;
		if (cfg->opt & MONO_OPT_SSA)
			tree->flags = MONO_INST_VOLATILE;
		tree->inst_c0 = num;
		tree->type = STACK_I4;
		tree->inst_vtype = &mono_defaults.int32_class->byval_arg;
		tree->klass = mono_class_from_mono_type (tree->inst_vtype);

		set_vreg_to_inst (cfg, inst->dreg + 2, tree);
	}

	cfg->num_varinfo++;
	if (cfg->verbose_level > 2)
		g_print ("created temp %d (R%d) of type %s\n", num, vreg, mono_type_get_name (type));
	return inst;
}

MonoInst*
mono_compile_create_var (MonoCompile *cfg, MonoType *type, int opcode)
{
	int dreg;
	type = mini_get_underlying_type (type);

	if (mono_type_is_long (type))
		dreg = mono_alloc_dreg (cfg, STACK_I8);
	else if (mono_arch_is_soft_float () && mono_type_is_float (type))
		dreg = mono_alloc_dreg (cfg, STACK_R8);
	else
		/* All the others are unified */
		dreg = mono_alloc_preg (cfg);

	return mono_compile_create_var_for_vreg (cfg, type, opcode, dreg);
}

MonoInst*
mini_get_int_to_float_spill_area (MonoCompile *cfg)
{
#ifdef TARGET_X86
	if (!cfg->iconv_raw_var) {
		cfg->iconv_raw_var = mono_compile_create_var (cfg, &mono_defaults.int32_class->byval_arg, OP_LOCAL);
		cfg->iconv_raw_var->flags |= MONO_INST_VOLATILE; /*FIXME, use the don't regalloc flag*/
	}
	return cfg->iconv_raw_var;
#else
	return NULL;
#endif
}

void
mono_mark_vreg_as_ref (MonoCompile *cfg, int vreg)
{
	if (vreg >= cfg->vreg_is_ref_len) {
		gboolean *tmp = cfg->vreg_is_ref;
		int size = cfg->vreg_is_ref_len;

		while (vreg >= cfg->vreg_is_ref_len)
			cfg->vreg_is_ref_len = cfg->vreg_is_ref_len ? cfg->vreg_is_ref_len * 2 : 32;
		cfg->vreg_is_ref = mono_mempool_alloc0 (cfg->mempool, sizeof (gboolean) * cfg->vreg_is_ref_len);
		if (size)
			memcpy (cfg->vreg_is_ref, tmp, size * sizeof (gboolean));
	}
	cfg->vreg_is_ref [vreg] = TRUE;
}	

void
mono_mark_vreg_as_mp (MonoCompile *cfg, int vreg)
{
	if (vreg >= cfg->vreg_is_mp_len) {
		gboolean *tmp = cfg->vreg_is_mp;
		int size = cfg->vreg_is_mp_len;

		while (vreg >= cfg->vreg_is_mp_len)
			cfg->vreg_is_mp_len = cfg->vreg_is_mp_len ? cfg->vreg_is_mp_len * 2 : 32;
		cfg->vreg_is_mp = mono_mempool_alloc0 (cfg->mempool, sizeof (gboolean) * cfg->vreg_is_mp_len);
		if (size)
			memcpy (cfg->vreg_is_mp, tmp, size * sizeof (gboolean));
	}
	cfg->vreg_is_mp [vreg] = TRUE;
}	

static MonoType*
type_from_stack_type (MonoInst *ins)
{
	switch (ins->type) {
	case STACK_I4: return &mono_defaults.int32_class->byval_arg;
	case STACK_I8: return &mono_defaults.int64_class->byval_arg;
	case STACK_PTR: return &mono_defaults.int_class->byval_arg;
	case STACK_R8: return &mono_defaults.double_class->byval_arg;
	case STACK_MP:
		/* 
		 * this if used to be commented without any specific reason, but
		 * it breaks #80235 when commented
		 */
		if (ins->klass)
			return &ins->klass->this_arg;
		else
			return &mono_defaults.object_class->this_arg;
	case STACK_OBJ:
		/* ins->klass may not be set for ldnull.
		 * Also, if we have a boxed valuetype, we want an object lass,
		 * not the valuetype class
		 */
		if (ins->klass && !ins->klass->valuetype)
			return &ins->klass->byval_arg;
		return &mono_defaults.object_class->byval_arg;
	case STACK_VTYPE: return &ins->klass->byval_arg;
	default:
		g_error ("stack type %d to montype not handled\n", ins->type);
	}
	return NULL;
}

MonoType*
mono_type_from_stack_type (MonoInst *ins)
{
	return type_from_stack_type (ins);
}

/*
 * mono_add_ins_to_end:
 *
 *   Same as MONO_ADD_INS, but add INST before any branches at the end of BB.
 */
void
mono_add_ins_to_end (MonoBasicBlock *bb, MonoInst *inst)
{
	int opcode;

	if (!bb->code) {
		MONO_ADD_INS (bb, inst);
		return;
	}

	switch (bb->last_ins->opcode) {
	case OP_BR:
	case OP_BR_REG:
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
	case OP_SWITCH:
		mono_bblock_insert_before_ins (bb, bb->last_ins, inst);
		break;
	default:
		if (MONO_IS_COND_BRANCH_OP (bb->last_ins)) {
			/* Need to insert the ins before the compare */
			if (bb->code == bb->last_ins) {
				mono_bblock_insert_before_ins (bb, bb->last_ins, inst);
				return;
			}

			if (bb->code->next == bb->last_ins) {
				/* Only two instructions */
				opcode = bb->code->opcode;

				if ((opcode == OP_COMPARE) || (opcode == OP_COMPARE_IMM) || (opcode == OP_ICOMPARE) || (opcode == OP_ICOMPARE_IMM) || (opcode == OP_FCOMPARE) || (opcode == OP_LCOMPARE) || (opcode == OP_LCOMPARE_IMM) || (opcode == OP_RCOMPARE)) {
					/* NEW IR */
					mono_bblock_insert_before_ins (bb, bb->code, inst);
				} else {
					mono_bblock_insert_before_ins (bb, bb->last_ins, inst);
				}
			} else {
				opcode = bb->last_ins->prev->opcode;

				if ((opcode == OP_COMPARE) || (opcode == OP_COMPARE_IMM) || (opcode == OP_ICOMPARE) || (opcode == OP_ICOMPARE_IMM) || (opcode == OP_FCOMPARE) || (opcode == OP_LCOMPARE) || (opcode == OP_LCOMPARE_IMM) || (opcode == OP_RCOMPARE)) {
					/* NEW IR */
					mono_bblock_insert_before_ins (bb, bb->last_ins->prev, inst);
				} else {
					mono_bblock_insert_before_ins (bb, bb->last_ins, inst);
				}					
			}
		}
		else
			MONO_ADD_INS (bb, inst);
		break;
	}
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

static MonoMethodSignature *
mono_get_array_new_va_signature (int arity)
{
	static GHashTable *sighash;
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
	if (ARCH_VARARG_ICALLS)
		/* Only set this only some archs since not all backends can handle varargs+pinvoke */
		res->call_convention = MONO_CALL_VARARG;

#ifdef TARGET_WIN32
	res->call_convention = MONO_CALL_C;
#endif

	res->params [0] = &mono_defaults.int_class->byval_arg;	
	for (i = 0; i < arity; i++)
		res->params [i + 1] = &mono_defaults.int_class->byval_arg;

	res->ret = &mono_defaults.object_class->byval_arg;

	g_hash_table_insert (sighash, GINT_TO_POINTER (arity), res);
	mono_jit_unlock ();

	return res;
}

MonoJitICallInfo *
mono_get_array_new_va_icall (int rank)
{
	MonoMethodSignature *esig;
	char icall_name [256];
	char *name;
	MonoJitICallInfo *info;

	/* Need to register the icall so it gets an icall wrapper */
	sprintf (icall_name, "ves_array_new_va_%d", rank);

	mono_jit_lock ();
	info = mono_find_jit_icall_by_name (icall_name);
	if (info == NULL) {
		esig = mono_get_array_new_va_signature (rank);
		name = g_strdup (icall_name);
		info = mono_register_jit_icall (mono_array_new_va, name, esig, FALSE);
	}
	mono_jit_unlock ();

	return info;
}

gboolean
mini_class_is_system_array (MonoClass *klass)
{
	if (klass->parent == mono_defaults.array_class)
		return TRUE;
	else
		return FALSE;
}

gboolean
mini_assembly_can_skip_verification (MonoDomain *domain, MonoMethod *method)
{
	MonoAssembly *assembly = method->klass->image->assembly;
	if (method->wrapper_type != MONO_WRAPPER_NONE && method->wrapper_type != MONO_WRAPPER_DYNAMIC_METHOD)
		return FALSE;
	if (assembly->in_gac || assembly->image == mono_defaults.corlib)
		return FALSE;
	return mono_assembly_has_skip_verification (assembly);
}

/*
 * mini_method_verify:
 * 
 * Verify the method using the new verfier.
 * 
 * Returns true if the method is invalid. 
 */
static gboolean
mini_method_verify (MonoCompile *cfg, MonoMethod *method, gboolean fail_compile)
{
	GSList *tmp, *res;
	gboolean is_fulltrust;
	MonoLoaderError *error;

	if (method->verification_success)
		return FALSE;

	if (!mono_verifier_is_enabled_for_method (method))
		return FALSE;

	/*skip verification implies the assembly must be */
	is_fulltrust = mono_verifier_is_method_full_trust (method) ||  mini_assembly_can_skip_verification (cfg->domain, method);

	res = mono_method_verify_with_current_settings (method, cfg->skip_visibility, is_fulltrust);

	if ((error = mono_loader_get_last_error ())) {
		if (fail_compile)
			cfg->exception_type = error->exception_type;
		else
			mono_loader_clear_error ();
		if (res)
			mono_free_verify_list (res);
		return TRUE;
	}

	if (res) { 
		for (tmp = res; tmp; tmp = tmp->next) {
			MonoVerifyInfoExtended *info = (MonoVerifyInfoExtended *)tmp->data;
			if (info->info.status == MONO_VERIFY_ERROR) {
				if (fail_compile) {
				char *method_name = mono_method_full_name (method, TRUE);
					cfg->exception_type = info->exception_type;
					cfg->exception_message = g_strdup_printf ("Error verifying %s: %s", method_name, info->info.message);
					g_free (method_name);
				}
				mono_free_verify_list (res);
				return TRUE;
			}
			if (info->info.status == MONO_VERIFY_NOT_VERIFIABLE && (!is_fulltrust || info->exception_type == MONO_EXCEPTION_METHOD_ACCESS || info->exception_type == MONO_EXCEPTION_FIELD_ACCESS)) {
				if (fail_compile) {
					char *method_name = mono_method_full_name (method, TRUE);
					cfg->exception_type = info->exception_type;
					cfg->exception_message = g_strdup_printf ("Error verifying %s: %s", method_name, info->info.message);
					g_free (method_name);
				}
				mono_free_verify_list (res);
				return TRUE;
			}
		}
		mono_free_verify_list (res);
	}
	method->verification_success = 1;
	return FALSE;
}

/*Returns true if something went wrong*/
gboolean
mono_compile_is_broken (MonoCompile *cfg, MonoMethod *method, gboolean fail_compile)
{
	MonoMethod *method_definition = method;
	gboolean dont_verify = method->klass->image->assembly->corlib_internal;

	while (method_definition->is_inflated) {
		MonoMethodInflated *imethod = (MonoMethodInflated *) method_definition;
		method_definition = imethod->declaring;
	}

	return !dont_verify && mini_method_verify (cfg, method_definition, fail_compile);
}

static void
mono_dynamic_code_hash_insert (MonoDomain *domain, MonoMethod *method, MonoJitDynamicMethodInfo *ji)
{
	if (!domain_jit_info (domain)->dynamic_code_hash)
		domain_jit_info (domain)->dynamic_code_hash = g_hash_table_new (NULL, NULL);
	g_hash_table_insert (domain_jit_info (domain)->dynamic_code_hash, method, ji);
}

static MonoJitDynamicMethodInfo*
mono_dynamic_code_hash_lookup (MonoDomain *domain, MonoMethod *method)
{
	MonoJitDynamicMethodInfo *res;

	if (domain_jit_info (domain)->dynamic_code_hash)
		res = g_hash_table_lookup (domain_jit_info (domain)->dynamic_code_hash, method);
	else
		res = NULL;
	return res;
}

typedef struct {
	MonoClass *vtype;
	GList *active, *inactive;
	GSList *slots;
} StackSlotInfo;

static gint 
compare_by_interval_start_pos_func (gconstpointer a, gconstpointer b)
{
	MonoMethodVar *v1 = (MonoMethodVar*)a;
	MonoMethodVar *v2 = (MonoMethodVar*)b;

	if (v1 == v2)
		return 0;
	else if (v1->interval->range && v2->interval->range)
		return v1->interval->range->from - v2->interval->range->from;
	else if (v1->interval->range)
		return -1;
	else
		return 1;
}

#if 0
#define LSCAN_DEBUG(a) do { a; } while (0)
#else
#define LSCAN_DEBUG(a)
#endif

static gint32*
mono_allocate_stack_slots2 (MonoCompile *cfg, gboolean backward, guint32 *stack_size, guint32 *stack_align)
{
	int i, slot, offset, size;
	guint32 align;
	MonoMethodVar *vmv;
	MonoInst *inst;
	gint32 *offsets;
	GList *vars = NULL, *l, *unhandled;
	StackSlotInfo *scalar_stack_slots, *vtype_stack_slots, *slot_info;
	MonoType *t;
	int nvtypes;
	gboolean reuse_slot;

	LSCAN_DEBUG (printf ("Allocate Stack Slots 2 for %s:\n", mono_method_full_name (cfg->method, TRUE)));

	scalar_stack_slots = mono_mempool_alloc0 (cfg->mempool, sizeof (StackSlotInfo) * MONO_TYPE_PINNED);
	vtype_stack_slots = NULL;
	nvtypes = 0;

	offsets = mono_mempool_alloc (cfg->mempool, sizeof (gint32) * cfg->num_varinfo);
	for (i = 0; i < cfg->num_varinfo; ++i)
		offsets [i] = -1;

	for (i = cfg->locals_start; i < cfg->num_varinfo; i++) {
		inst = cfg->varinfo [i];
		vmv = MONO_VARINFO (cfg, i);

		if ((inst->flags & MONO_INST_IS_DEAD) || inst->opcode == OP_REGVAR || inst->opcode == OP_REGOFFSET)
			continue;

		vars = g_list_prepend (vars, vmv);
	}

	vars = g_list_sort (g_list_copy (vars), compare_by_interval_start_pos_func);

	/* Sanity check */
	/*
	i = 0;
	for (unhandled = vars; unhandled; unhandled = unhandled->next) {
		MonoMethodVar *current = unhandled->data;

		if (current->interval->range) {
			g_assert (current->interval->range->from >= i);
			i = current->interval->range->from;
		}
	}
	*/

	offset = 0;
	*stack_align = 0;
	for (unhandled = vars; unhandled; unhandled = unhandled->next) {
		MonoMethodVar *current = unhandled->data;

		vmv = current;
		inst = cfg->varinfo [vmv->idx];

		t = mono_type_get_underlying_type (inst->inst_vtype);
		if (cfg->gsharedvt && mini_is_gsharedvt_variable_type (t))
			continue;

		/* inst->backend.is_pinvoke indicates native sized value types, this is used by the
		* pinvoke wrappers when they call functions returning structures */
		if (inst->backend.is_pinvoke && MONO_TYPE_ISSTRUCT (t) && t->type != MONO_TYPE_TYPEDBYREF) {
			size = mono_class_native_size (mono_class_from_mono_type (t), &align);
		}
		else {
			int ialign;

			size = mini_type_stack_size (t, &ialign);
			align = ialign;

			if (MONO_CLASS_IS_SIMD (cfg, mono_class_from_mono_type (t)))
				align = 16;
		}

		reuse_slot = TRUE;
		if (cfg->disable_reuse_stack_slots)
			reuse_slot = FALSE;

		t = mini_get_underlying_type (t);
		switch (t->type) {
		case MONO_TYPE_GENERICINST:
			if (!mono_type_generic_inst_is_valuetype (t)) {
				slot_info = &scalar_stack_slots [t->type];
				break;
			}
			/* Fall through */
		case MONO_TYPE_VALUETYPE:
			if (!vtype_stack_slots)
				vtype_stack_slots = mono_mempool_alloc0 (cfg->mempool, sizeof (StackSlotInfo) * 256);
			for (i = 0; i < nvtypes; ++i)
				if (t->data.klass == vtype_stack_slots [i].vtype)
					break;
			if (i < nvtypes)
				slot_info = &vtype_stack_slots [i];
			else {
				g_assert (nvtypes < 256);
				vtype_stack_slots [nvtypes].vtype = t->data.klass;
				slot_info = &vtype_stack_slots [nvtypes];
				nvtypes ++;
			}
			if (cfg->disable_reuse_ref_stack_slots)
				reuse_slot = FALSE;
			break;

		case MONO_TYPE_PTR:
		case MONO_TYPE_I:
		case MONO_TYPE_U:
#if SIZEOF_VOID_P == 4
		case MONO_TYPE_I4:
#else
		case MONO_TYPE_I8:
#endif
			if (cfg->disable_ref_noref_stack_slot_share) {
				slot_info = &scalar_stack_slots [MONO_TYPE_I];
				break;
			}
			/* Fall through */

		case MONO_TYPE_CLASS:
		case MONO_TYPE_OBJECT:
		case MONO_TYPE_ARRAY:
		case MONO_TYPE_SZARRAY:
		case MONO_TYPE_STRING:
			/* Share non-float stack slots of the same size */
			slot_info = &scalar_stack_slots [MONO_TYPE_CLASS];
			if (cfg->disable_reuse_ref_stack_slots)
				reuse_slot = FALSE;
			break;

		default:
			slot_info = &scalar_stack_slots [t->type];
		}

		slot = 0xffffff;
		if (cfg->comp_done & MONO_COMP_LIVENESS) {
			int pos;
			gboolean changed;

			//printf ("START  %2d %08x %08x\n",  vmv->idx, vmv->range.first_use.abs_pos, vmv->range.last_use.abs_pos);

			if (!current->interval->range) {
				if (inst->flags & (MONO_INST_VOLATILE|MONO_INST_INDIRECT))
					pos = ~0;
				else {
					/* Dead */
					inst->flags |= MONO_INST_IS_DEAD;
					continue;
				}
			}
			else
				pos = current->interval->range->from;

			LSCAN_DEBUG (printf ("process R%d ", inst->dreg));
			if (current->interval->range)
				LSCAN_DEBUG (mono_linterval_print (current->interval));
			LSCAN_DEBUG (printf ("\n"));

			/* Check for intervals in active which expired or inactive */
			changed = TRUE;
			/* FIXME: Optimize this */
			while (changed) {
				changed = FALSE;
				for (l = slot_info->active; l != NULL; l = l->next) {
					MonoMethodVar *v = (MonoMethodVar*)l->data;

					if (v->interval->last_range->to < pos) {
						slot_info->active = g_list_delete_link (slot_info->active, l);
						slot_info->slots = g_slist_prepend_mempool (cfg->mempool, slot_info->slots, GINT_TO_POINTER (offsets [v->idx]));
						LSCAN_DEBUG (printf ("Interval R%d has expired, adding 0x%x to slots\n", cfg->varinfo [v->idx]->dreg, offsets [v->idx]));
						changed = TRUE;
						break;
					}
					else if (!mono_linterval_covers (v->interval, pos)) {
						slot_info->inactive = g_list_append (slot_info->inactive, v);
						slot_info->active = g_list_delete_link (slot_info->active, l);
						LSCAN_DEBUG (printf ("Interval R%d became inactive\n", cfg->varinfo [v->idx]->dreg));
						changed = TRUE;
						break;
					}
				}
			}

			/* Check for intervals in inactive which expired or active */
			changed = TRUE;
			/* FIXME: Optimize this */
			while (changed) {
				changed = FALSE;
				for (l = slot_info->inactive; l != NULL; l = l->next) {
					MonoMethodVar *v = (MonoMethodVar*)l->data;

					if (v->interval->last_range->to < pos) {
						slot_info->inactive = g_list_delete_link (slot_info->inactive, l);
						// FIXME: Enabling this seems to cause impossible to debug crashes
						//slot_info->slots = g_slist_prepend_mempool (cfg->mempool, slot_info->slots, GINT_TO_POINTER (offsets [v->idx]));
						LSCAN_DEBUG (printf ("Interval R%d has expired, adding 0x%x to slots\n", cfg->varinfo [v->idx]->dreg, offsets [v->idx]));
						changed = TRUE;
						break;
					}
					else if (mono_linterval_covers (v->interval, pos)) {
						slot_info->active = g_list_append (slot_info->active, v);
						slot_info->inactive = g_list_delete_link (slot_info->inactive, l);
						LSCAN_DEBUG (printf ("\tInterval R%d became active\n", cfg->varinfo [v->idx]->dreg));
						changed = TRUE;
						break;
					}
				}
			}

			/* 
			 * This also handles the case when the variable is used in an
			 * exception region, as liveness info is not computed there.
			 */
			/* 
			 * FIXME: All valuetypes are marked as INDIRECT because of LDADDR
			 * opcodes.
			 */
			if (! (inst->flags & (MONO_INST_VOLATILE|MONO_INST_INDIRECT))) {
				if (slot_info->slots) {
					slot = GPOINTER_TO_INT (slot_info->slots->data);

					slot_info->slots = slot_info->slots->next;
				}

				/* FIXME: We might want to consider the inactive intervals as well if slot_info->slots is empty */

				slot_info->active = mono_varlist_insert_sorted (cfg, slot_info->active, vmv, TRUE);
			}
		}

#if 0
		{
			static int count = 0;
			count ++;

			if (count == atoi (g_getenv ("COUNT3")))
				printf ("LAST: %s\n", mono_method_full_name (cfg->method, TRUE));
			if (count > atoi (g_getenv ("COUNT3")))
				slot = 0xffffff;
			else {
				mono_print_ins (inst);
				}
		}
#endif

		LSCAN_DEBUG (printf ("R%d %s -> 0x%x\n", inst->dreg, mono_type_full_name (t), slot));

		if (inst->flags & MONO_INST_LMF) {
			size = sizeof (MonoLMF);
			align = sizeof (mgreg_t);
			reuse_slot = FALSE;
		}

		if (!reuse_slot)
			slot = 0xffffff;

		if (slot == 0xffffff) {
			/*
			 * Allways allocate valuetypes to sizeof (gpointer) to allow more
			 * efficient copying (and to work around the fact that OP_MEMCPY
			 * and OP_MEMSET ignores alignment).
			 */
			if (MONO_TYPE_ISSTRUCT (t)) {
				align = MAX (align, sizeof (gpointer));
				align = MAX (align, mono_class_min_align (mono_class_from_mono_type (t)));
			}

			if (backward) {
				offset += size;
				offset += align - 1;
				offset &= ~(align - 1);
				slot = offset;
			}
			else {
				offset += align - 1;
				offset &= ~(align - 1);
				slot = offset;
				offset += size;
			}

			if (*stack_align == 0)
				*stack_align = align;
		}

		offsets [vmv->idx] = slot;
	}
	g_list_free (vars);
	for (i = 0; i < MONO_TYPE_PINNED; ++i) {
		if (scalar_stack_slots [i].active)
			g_list_free (scalar_stack_slots [i].active);
	}
	for (i = 0; i < nvtypes; ++i) {
		if (vtype_stack_slots [i].active)
			g_list_free (vtype_stack_slots [i].active);
	}

	cfg->stat_locals_stack_size += offset;

	*stack_size = offset;
	return offsets;
}

/*
 *  mono_allocate_stack_slots:
 *
 *  Allocate stack slots for all non register allocated variables using a
 * linear scan algorithm.
 * Returns: an array of stack offsets.
 * STACK_SIZE is set to the amount of stack space needed.
 * STACK_ALIGN is set to the alignment needed by the locals area.
 */
gint32*
mono_allocate_stack_slots (MonoCompile *cfg, gboolean backward, guint32 *stack_size, guint32 *stack_align)
{
	int i, slot, offset, size;
	guint32 align;
	MonoMethodVar *vmv;
	MonoInst *inst;
	gint32 *offsets;
	GList *vars = NULL, *l;
	StackSlotInfo *scalar_stack_slots, *vtype_stack_slots, *slot_info;
	MonoType *t;
	int nvtypes;
	gboolean reuse_slot;

	if ((cfg->num_varinfo > 0) && MONO_VARINFO (cfg, 0)->interval)
		return mono_allocate_stack_slots2 (cfg, backward, stack_size, stack_align);

	scalar_stack_slots = mono_mempool_alloc0 (cfg->mempool, sizeof (StackSlotInfo) * MONO_TYPE_PINNED);
	vtype_stack_slots = NULL;
	nvtypes = 0;

	offsets = mono_mempool_alloc (cfg->mempool, sizeof (gint32) * cfg->num_varinfo);
	for (i = 0; i < cfg->num_varinfo; ++i)
		offsets [i] = -1;

	for (i = cfg->locals_start; i < cfg->num_varinfo; i++) {
		inst = cfg->varinfo [i];
		vmv = MONO_VARINFO (cfg, i);

		if ((inst->flags & MONO_INST_IS_DEAD) || inst->opcode == OP_REGVAR || inst->opcode == OP_REGOFFSET)
			continue;

		vars = g_list_prepend (vars, vmv);
	}

	vars = mono_varlist_sort (cfg, vars, 0);
	offset = 0;
	*stack_align = sizeof(mgreg_t);
	for (l = vars; l; l = l->next) {
		vmv = l->data;
		inst = cfg->varinfo [vmv->idx];

		t = mono_type_get_underlying_type (inst->inst_vtype);
		if (cfg->gsharedvt && mini_is_gsharedvt_variable_type (t))
			continue;

		/* inst->backend.is_pinvoke indicates native sized value types, this is used by the
		* pinvoke wrappers when they call functions returning structures */
		if (inst->backend.is_pinvoke && MONO_TYPE_ISSTRUCT (t) && t->type != MONO_TYPE_TYPEDBYREF) {
			size = mono_class_native_size (mono_class_from_mono_type (t), &align);
		} else {
			int ialign;

			size = mini_type_stack_size (t, &ialign);
			align = ialign;

			if (mono_class_from_mono_type (t)->exception_type)
				mono_cfg_set_exception (cfg, MONO_EXCEPTION_TYPE_LOAD);

			if (MONO_CLASS_IS_SIMD (cfg, mono_class_from_mono_type (t)))
				align = 16;
		}

		reuse_slot = TRUE;
		if (cfg->disable_reuse_stack_slots)
			reuse_slot = FALSE;

		t = mini_get_underlying_type (t);
		switch (t->type) {
		case MONO_TYPE_GENERICINST:
			if (!mono_type_generic_inst_is_valuetype (t)) {
				slot_info = &scalar_stack_slots [t->type];
				break;
			}
			/* Fall through */
		case MONO_TYPE_VALUETYPE:
			if (!vtype_stack_slots)
				vtype_stack_slots = mono_mempool_alloc0 (cfg->mempool, sizeof (StackSlotInfo) * 256);
			for (i = 0; i < nvtypes; ++i)
				if (t->data.klass == vtype_stack_slots [i].vtype)
					break;
			if (i < nvtypes)
				slot_info = &vtype_stack_slots [i];
			else {
				g_assert (nvtypes < 256);
				vtype_stack_slots [nvtypes].vtype = t->data.klass;
				slot_info = &vtype_stack_slots [nvtypes];
				nvtypes ++;
			}
			if (cfg->disable_reuse_ref_stack_slots)
				reuse_slot = FALSE;
			break;

		case MONO_TYPE_PTR:
		case MONO_TYPE_I:
		case MONO_TYPE_U:
#if SIZEOF_VOID_P == 4
		case MONO_TYPE_I4:
#else
		case MONO_TYPE_I8:
#endif
			if (cfg->disable_ref_noref_stack_slot_share) {
				slot_info = &scalar_stack_slots [MONO_TYPE_I];
				break;
			}
			/* Fall through */

		case MONO_TYPE_CLASS:
		case MONO_TYPE_OBJECT:
		case MONO_TYPE_ARRAY:
		case MONO_TYPE_SZARRAY:
		case MONO_TYPE_STRING:
			/* Share non-float stack slots of the same size */
			slot_info = &scalar_stack_slots [MONO_TYPE_CLASS];
			if (cfg->disable_reuse_ref_stack_slots)
				reuse_slot = FALSE;
			break;
		case MONO_TYPE_VAR:
		case MONO_TYPE_MVAR:
			slot_info = &scalar_stack_slots [t->type];
			break;
		default:
			slot_info = &scalar_stack_slots [t->type];
			break;
		}

		slot = 0xffffff;
		if (cfg->comp_done & MONO_COMP_LIVENESS) {
			//printf ("START  %2d %08x %08x\n",  vmv->idx, vmv->range.first_use.abs_pos, vmv->range.last_use.abs_pos);
			
			/* expire old intervals in active */
			while (slot_info->active) {
				MonoMethodVar *amv = (MonoMethodVar *)slot_info->active->data;

				if (amv->range.last_use.abs_pos > vmv->range.first_use.abs_pos)
					break;

				//printf ("EXPIR  %2d %08x %08x C%d R%d\n", amv->idx, amv->range.first_use.abs_pos, amv->range.last_use.abs_pos, amv->spill_costs, amv->reg);

				slot_info->active = g_list_delete_link (slot_info->active, slot_info->active);
				slot_info->slots = g_slist_prepend_mempool (cfg->mempool, slot_info->slots, GINT_TO_POINTER (offsets [amv->idx]));
			}

			/* 
			 * This also handles the case when the variable is used in an
			 * exception region, as liveness info is not computed there.
			 */
			/* 
			 * FIXME: All valuetypes are marked as INDIRECT because of LDADDR
			 * opcodes.
			 */
			if (! (inst->flags & (MONO_INST_VOLATILE|MONO_INST_INDIRECT))) {
				if (slot_info->slots) {
					slot = GPOINTER_TO_INT (slot_info->slots->data);

					slot_info->slots = slot_info->slots->next;
				}

				slot_info->active = mono_varlist_insert_sorted (cfg, slot_info->active, vmv, TRUE);
			}
		}

		{
			static int count = 0;
			count ++;

			/*
			if (count == atoi (g_getenv ("COUNT")))
				printf ("LAST: %s\n", mono_method_full_name (cfg->method, TRUE));
			if (count > atoi (g_getenv ("COUNT")))
				slot = 0xffffff;
			else {
				mono_print_ins (inst);
				}
			*/
		}

		if (inst->flags & MONO_INST_LMF) {
			/*
			 * This variable represents a MonoLMF structure, which has no corresponding
			 * CLR type, so hard-code its size/alignment.
			 */
			size = sizeof (MonoLMF);
			align = sizeof (mgreg_t);
			reuse_slot = FALSE;
		}

		if (!reuse_slot)
			slot = 0xffffff;

		if (slot == 0xffffff) {
			/*
			 * Allways allocate valuetypes to sizeof (gpointer) to allow more
			 * efficient copying (and to work around the fact that OP_MEMCPY
			 * and OP_MEMSET ignores alignment).
			 */
			if (MONO_TYPE_ISSTRUCT (t)) {
				align = MAX (align, sizeof (gpointer));
				align = MAX (align, mono_class_min_align (mono_class_from_mono_type (t)));
				/* 
				 * Align the size too so the code generated for passing vtypes in
				 * registers doesn't overwrite random locals.
				 */
				size = (size + (align - 1)) & ~(align -1);
			}

			if (backward) {
				offset += size;
				offset += align - 1;
				offset &= ~(align - 1);
				slot = offset;
			}
			else {
				offset += align - 1;
				offset &= ~(align - 1);
				slot = offset;
				offset += size;
			}

			*stack_align = MAX (*stack_align, align);
		}

		offsets [vmv->idx] = slot;
	}
	g_list_free (vars);
	for (i = 0; i < MONO_TYPE_PINNED; ++i) {
		if (scalar_stack_slots [i].active)
			g_list_free (scalar_stack_slots [i].active);
	}
	for (i = 0; i < nvtypes; ++i) {
		if (vtype_stack_slots [i].active)
			g_list_free (vtype_stack_slots [i].active);
	}

	cfg->stat_locals_stack_size += offset;

	*stack_size = offset;
	return offsets;
}

#define EMUL_HIT_SHIFT 3
#define EMUL_HIT_MASK ((1 << EMUL_HIT_SHIFT) - 1)
/* small hit bitmap cache */
static mono_byte emul_opcode_hit_cache [(OP_LAST>>EMUL_HIT_SHIFT) + 1] = {0};
static short emul_opcode_num = 0;
static short emul_opcode_alloced = 0;
static short *emul_opcode_opcodes;
static MonoJitICallInfo **emul_opcode_map;

MonoJitICallInfo *
mono_find_jit_opcode_emulation (int opcode)
{
	g_assert (opcode >= 0 && opcode <= OP_LAST);
	if (emul_opcode_hit_cache [opcode >> (EMUL_HIT_SHIFT + 3)] & (1 << (opcode & EMUL_HIT_MASK))) {
		int i;
		for (i = 0; i < emul_opcode_num; ++i) {
			if (emul_opcode_opcodes [i] == opcode)
				return emul_opcode_map [i];
		}
	}
	return NULL;
}

void
mini_register_opcode_emulation (int opcode, const char *name, const char *sigstr, gpointer func, const char *symbol, gboolean no_throw)
{
	MonoJitICallInfo *info;
	MonoMethodSignature *sig = mono_create_icall_signature (sigstr);

	g_assert (!sig->hasthis);
	g_assert (sig->param_count < 3);

	/* Opcode emulation functions are assumed to don't call mono_raise_exception () */
	info = mono_register_jit_icall_full (func, name, sig, no_throw, TRUE, symbol);

	if (emul_opcode_num >= emul_opcode_alloced) {
		int incr = emul_opcode_alloced? emul_opcode_alloced/2: 16;
		emul_opcode_alloced += incr;
		emul_opcode_map = g_realloc (emul_opcode_map, sizeof (emul_opcode_map [0]) * emul_opcode_alloced);
		emul_opcode_opcodes = g_realloc (emul_opcode_opcodes, sizeof (emul_opcode_opcodes [0]) * emul_opcode_alloced);
	}
	emul_opcode_map [emul_opcode_num] = info;
	emul_opcode_opcodes [emul_opcode_num] = opcode;
	emul_opcode_num++;
	emul_opcode_hit_cache [opcode >> (EMUL_HIT_SHIFT + 3)] |= (1 << (opcode & EMUL_HIT_MASK));
}

static void
print_dfn (MonoCompile *cfg)
{
	int i, j;
	char *code;
	MonoBasicBlock *bb;
	MonoInst *c;

	{
		char *method_name = mono_method_full_name (cfg->method, TRUE);
		g_print ("IR code for method %s\n", method_name);
		g_free (method_name);
	}

	for (i = 0; i < cfg->num_bblocks; ++i) {
		bb = cfg->bblocks [i];
		/*if (bb->cil_code) {
			char* code1, *code2;
			code1 = mono_disasm_code_one (NULL, cfg->method, bb->cil_code, NULL);
			if (bb->last_ins->cil_code)
				code2 = mono_disasm_code_one (NULL, cfg->method, bb->last_ins->cil_code, NULL);
			else
				code2 = g_strdup ("");

			code1 [strlen (code1) - 1] = 0;
			code = g_strdup_printf ("%s -> %s", code1, code2);
			g_free (code1);
			g_free (code2);
		} else*/
			code = g_strdup ("\n");
		g_print ("\nBB%d (%d) (len: %d): %s", bb->block_num, i, bb->cil_length, code);
		MONO_BB_FOR_EACH_INS (bb, c) {
			mono_print_ins_index (-1, c);
		}

		g_print ("\tprev:");
		for (j = 0; j < bb->in_count; ++j) {
			g_print (" BB%d", bb->in_bb [j]->block_num);
		}
		g_print ("\t\tsucc:");
		for (j = 0; j < bb->out_count; ++j) {
			g_print (" BB%d", bb->out_bb [j]->block_num);
		}
		g_print ("\n\tidom: BB%d\n", bb->idom? bb->idom->block_num: -1);

		if (bb->idom)
			g_assert (mono_bitset_test_fast (bb->dominators, bb->idom->dfn));

		if (bb->dominators)
			mono_blockset_print (cfg, bb->dominators, "\tdominators", bb->idom? bb->idom->dfn: -1);
		if (bb->dfrontier)
			mono_blockset_print (cfg, bb->dfrontier, "\tdfrontier", -1);
		g_free (code);
	}

	g_print ("\n");
}

void
mono_bblock_add_inst (MonoBasicBlock *bb, MonoInst *inst)
{
	MONO_ADD_INS (bb, inst);
}

void
mono_bblock_insert_after_ins (MonoBasicBlock *bb, MonoInst *ins, MonoInst *ins_to_insert)
{
	if (ins == NULL) {
		ins = bb->code;
		bb->code = ins_to_insert;

		/* Link with next */
		ins_to_insert->next = ins;
		if (ins)
			ins->prev = ins_to_insert;

		if (bb->last_ins == NULL)
			bb->last_ins = ins_to_insert;
	} else {
		/* Link with next */
		ins_to_insert->next = ins->next;
		if (ins->next)
			ins->next->prev = ins_to_insert;

		/* Link with previous */
		ins->next = ins_to_insert;
		ins_to_insert->prev = ins;

		if (bb->last_ins == ins)
			bb->last_ins = ins_to_insert;
	}
}

void
mono_bblock_insert_before_ins (MonoBasicBlock *bb, MonoInst *ins, MonoInst *ins_to_insert)
{
	if (ins == NULL) {
		ins = bb->code;
		if (ins)
			ins->prev = ins_to_insert;
		bb->code = ins_to_insert;
		ins_to_insert->next = ins;
		if (bb->last_ins == NULL)
			bb->last_ins = ins_to_insert;
	} else {
		/* Link with previous */
		if (ins->prev)
			ins->prev->next = ins_to_insert;
		ins_to_insert->prev = ins->prev;

		/* Link with next */
		ins->prev = ins_to_insert;
		ins_to_insert->next = ins;

		if (bb->code == ins)
			bb->code = ins_to_insert;
	}
}

/*
 * mono_verify_bblock:
 *
 *   Verify that the next and prev pointers are consistent inside the instructions in BB.
 */
void
mono_verify_bblock (MonoBasicBlock *bb)
{
	MonoInst *ins, *prev;

	prev = NULL;
	for (ins = bb->code; ins; ins = ins->next) {
		g_assert (ins->prev == prev);
		prev = ins;
	}
	if (bb->last_ins)
		g_assert (!bb->last_ins->next);
}

/*
 * mono_verify_cfg:
 *
 *   Perform consistency checks on the JIT data structures and the IR
 */
void
mono_verify_cfg (MonoCompile *cfg)
{
	MonoBasicBlock *bb;

	for (bb = cfg->bb_entry; bb; bb = bb->next_bb)
		mono_verify_bblock (bb);
}

void
mono_destroy_compile (MonoCompile *cfg)
{
	GSList *l;

	if (cfg->header)
		mono_metadata_free_mh (cfg->header);
	//mono_mempool_stats (cfg->mempool);
	mono_free_loop_info (cfg);
	if (cfg->rs)
		mono_regstate_free (cfg->rs);
	if (cfg->spvars)
		g_hash_table_destroy (cfg->spvars);
	if (cfg->exvars)
		g_hash_table_destroy (cfg->exvars);
	for (l = cfg->headers_to_free; l; l = l->next)
		mono_metadata_free_mh (l->data);
	g_list_free (cfg->ldstr_list);
	g_hash_table_destroy (cfg->token_info_hash);
	if (cfg->abs_patches)
		g_hash_table_destroy (cfg->abs_patches);
	mono_mempool_destroy (cfg->mempool);

	mono_debug_free_method (cfg);

	g_free (cfg->varinfo);
	g_free (cfg->vars);
	g_free (cfg->exception_message);
	g_free (cfg);
}

static MonoInst*
mono_create_tls_get_offset (MonoCompile *cfg, int offset)
{
	MonoInst* ins;

	if (!cfg->have_tls_get)
		return NULL;

	if (offset == -1)
		return NULL;

	MONO_INST_NEW (cfg, ins, OP_TLS_GET);
	ins->dreg = mono_alloc_preg (cfg);
	ins->inst_offset = offset;
	return ins;
}

gboolean
mini_tls_get_supported (MonoCompile *cfg, MonoTlsKey key)
{
	if (!cfg->have_tls_get)
		return FALSE;

	if (cfg->compile_aot)
		return cfg->have_tls_get_reg;
	else
		return mini_get_tls_offset (key) != -1;
}

MonoInst*
mono_create_tls_get (MonoCompile *cfg, MonoTlsKey key)
{
	if (!cfg->have_tls_get)
		return NULL;

	/*
	 * TLS offsets might be different at AOT time, so load them from a GOT slot and
	 * use a different opcode.
	 */
	if (cfg->compile_aot) {
		if (cfg->have_tls_get_reg) {
			MonoInst *ins, *c;

			EMIT_NEW_TLS_OFFSETCONST (cfg, c, key);
			MONO_INST_NEW (cfg, ins, OP_TLS_GET_REG);
			ins->dreg = mono_alloc_preg (cfg);
			ins->sreg1 = c->dreg;
			return ins;
		} else {
			return NULL;
		}
	}

	return mono_create_tls_get_offset (cfg, mini_get_tls_offset (key));
}

MonoInst*
mono_get_jit_tls_intrinsic (MonoCompile *cfg)
{
	return mono_create_tls_get (cfg, TLS_KEY_JIT_TLS);
}

MonoInst*
mono_get_domain_intrinsic (MonoCompile* cfg)
{
	return mono_create_tls_get (cfg, TLS_KEY_DOMAIN);
}

MonoInst*
mono_get_thread_intrinsic (MonoCompile* cfg)
{
	return mono_create_tls_get (cfg, TLS_KEY_THREAD);
}

MonoInst*
mono_get_lmf_intrinsic (MonoCompile* cfg)
{
	return mono_create_tls_get (cfg, TLS_KEY_LMF);
}

MonoInst*
mono_get_lmf_addr_intrinsic (MonoCompile* cfg)
{
	return mono_create_tls_get (cfg, TLS_KEY_LMF_ADDR);
}

void
mono_add_patch_info (MonoCompile *cfg, int ip, MonoJumpInfoType type, gconstpointer target)
{
	MonoJumpInfo *ji = mono_mempool_alloc0 (cfg->mempool, sizeof (MonoJumpInfo));

	ji->ip.i = ip;
	ji->type = type;
	ji->data.target = target;
	ji->next = cfg->patch_info;

	cfg->patch_info = ji;
}

void
mono_add_patch_info_rel (MonoCompile *cfg, int ip, MonoJumpInfoType type, gconstpointer target, int relocation)
{
	MonoJumpInfo *ji = mono_mempool_alloc0 (cfg->mempool, sizeof (MonoJumpInfo));

	ji->ip.i = ip;
	ji->type = type;
	ji->relocation = relocation;
	ji->data.target = target;
	ji->next = cfg->patch_info;

	cfg->patch_info = ji;
}

void
mono_remove_patch_info (MonoCompile *cfg, int ip)
{
	MonoJumpInfo **ji = &cfg->patch_info;

	while (*ji) {
		if ((*ji)->ip.i == ip)
			*ji = (*ji)->next;
		else
			ji = &((*ji)->next);
	}
}

void
mono_add_seq_point (MonoCompile *cfg, MonoBasicBlock *bb, MonoInst *ins, int native_offset)
{
	ins->inst_offset = native_offset;
	g_ptr_array_add (cfg->seq_points, ins);
	if (bb) {
		bb->seq_points = g_slist_prepend_mempool (cfg->mempool, bb->seq_points, ins);
		bb->last_seq_point = ins;
	}
}

void
mono_add_var_location (MonoCompile *cfg, MonoInst *var, gboolean is_reg, int reg, int offset, int from, int to)
{
	MonoDwarfLocListEntry *entry = mono_mempool_alloc0 (cfg->mempool, sizeof (MonoDwarfLocListEntry));

	if (is_reg)
		g_assert (offset == 0);

	entry->is_reg = is_reg;
	entry->reg = reg;
	entry->offset = offset;
	entry->from = from;
	entry->to = to;

	if (var == cfg->args [0])
		cfg->this_loclist = g_slist_append_mempool (cfg->mempool, cfg->this_loclist, entry);
	else if (var == cfg->rgctx_var)
		cfg->rgctx_loclist = g_slist_append_mempool (cfg->mempool, cfg->rgctx_loclist, entry);
}

static void
mono_compile_create_vars (MonoCompile *cfg)
{
	MonoMethodSignature *sig;
	MonoMethodHeader *header;
	int i;

	header = cfg->header;

	sig = mono_method_signature (cfg->method);
	
	if (!MONO_TYPE_IS_VOID (sig->ret)) {
		cfg->ret = mono_compile_create_var (cfg, sig->ret, OP_ARG);
		/* Inhibit optimizations */
		cfg->ret->flags |= MONO_INST_VOLATILE;
	}
	if (cfg->verbose_level > 2)
		g_print ("creating vars\n");

	cfg->args = mono_mempool_alloc0 (cfg->mempool, (sig->param_count + sig->hasthis) * sizeof (MonoInst*));

	if (sig->hasthis)
		cfg->args [0] = mono_compile_create_var (cfg, &cfg->method->klass->this_arg, OP_ARG);

	for (i = 0; i < sig->param_count; ++i) {
		cfg->args [i + sig->hasthis] = mono_compile_create_var (cfg, sig->params [i], OP_ARG);
	}

	if (cfg->verbose_level > 2) {
		if (cfg->ret) {
			printf ("\treturn : ");
			mono_print_ins (cfg->ret);
		}

		if (sig->hasthis) {
			printf ("\tthis: ");
			mono_print_ins (cfg->args [0]);
		}

		for (i = 0; i < sig->param_count; ++i) {
			printf ("\targ [%d]: ", i);
			mono_print_ins (cfg->args [i + sig->hasthis]);
		}
	}

	cfg->locals_start = cfg->num_varinfo;
	cfg->locals = mono_mempool_alloc0 (cfg->mempool, header->num_locals * sizeof (MonoInst*));

	if (cfg->verbose_level > 2)
		g_print ("creating locals\n");

	for (i = 0; i < header->num_locals; ++i)
		cfg->locals [i] = mono_compile_create_var (cfg, header->locals [i], OP_LOCAL);

	if (cfg->verbose_level > 2)
		g_print ("locals done\n");

	mono_arch_create_vars (cfg);

	if (cfg->method->save_lmf && cfg->create_lmf_var) {
		MonoInst *lmf_var = mono_compile_create_var (cfg, &mono_defaults.int_class->byval_arg, OP_LOCAL);
		lmf_var->flags |= MONO_INST_VOLATILE;
		lmf_var->flags |= MONO_INST_LMF;
		cfg->lmf_var = lmf_var;
	}
}

void
mono_print_code (MonoCompile *cfg, const char* msg)
{
	MonoBasicBlock *bb;
	
	for (bb = cfg->bb_entry; bb; bb = bb->next_bb)
		mono_print_bb (bb, msg);
}

static void
mono_postprocess_patches (MonoCompile *cfg)
{
	MonoJumpInfo *patch_info;
	int i;

	for (patch_info = cfg->patch_info; patch_info; patch_info = patch_info->next) {
		switch (patch_info->type) {
		case MONO_PATCH_INFO_ABS: {
			MonoJitICallInfo *info = mono_find_jit_icall_by_addr (patch_info->data.target);

			/*
			 * Change patches of type MONO_PATCH_INFO_ABS into patches describing the 
			 * absolute address.
			 */
			if (info) {
				//printf ("TEST %s %p\n", info->name, patch_info->data.target);
				/* for these array methods we currently register the same function pointer
				 * since it's a vararg function. But this means that mono_find_jit_icall_by_addr ()
				 * will return the incorrect one depending on the order they are registered.
				 * See tests/test-arr.cs
				 */
				if (strstr (info->name, "ves_array_new_va_") == NULL && strstr (info->name, "ves_array_element_address_") == NULL) {
					patch_info->type = MONO_PATCH_INFO_INTERNAL_METHOD;
					patch_info->data.name = info->name;
				}
			}

			if (patch_info->type == MONO_PATCH_INFO_ABS) {
				if (cfg->abs_patches) {
					MonoJumpInfo *abs_ji = g_hash_table_lookup (cfg->abs_patches, patch_info->data.target);
					if (abs_ji) {
						patch_info->type = abs_ji->type;
						patch_info->data.target = abs_ji->data.target;
					}
				}
			}

			break;
		}
		case MONO_PATCH_INFO_SWITCH: {
			gpointer *table;
#if defined(__native_client__) && defined(__native_client_codegen__)
			/* This memory will leak.  */
			/* TODO: can we free this when  */
			/* making the final jump table? */
			table = g_malloc0 (sizeof(gpointer) * patch_info->data.table->table_size);
#else
			if (cfg->method->dynamic) {
				table = mono_code_manager_reserve (cfg->dynamic_info->code_mp, sizeof (gpointer) * patch_info->data.table->table_size);
			} else {
				table = mono_domain_code_reserve (cfg->domain, sizeof (gpointer) * patch_info->data.table->table_size);
			}
#endif

			for (i = 0; i < patch_info->data.table->table_size; i++) {
				/* Might be NULL if the switch is eliminated */
				if (patch_info->data.table->table [i]) {
					g_assert (patch_info->data.table->table [i]->native_offset);
					table [i] = GINT_TO_POINTER (patch_info->data.table->table [i]->native_offset);
				} else {
					table [i] = NULL;
				}
			}
			patch_info->data.table->table = (MonoBasicBlock**)table;
			break;
		}
		case MONO_PATCH_INFO_METHOD_JUMP: {
			MonoJumpList *jlist;
			MonoDomain *domain = cfg->domain;
			unsigned char *ip = cfg->native_code + patch_info->ip.i;
#if defined(__native_client__) && defined(__native_client_codegen__)
			/* When this jump target gets evaluated, the method */
			/* will be installed in the dynamic code section,   */
			/* not at the location of cfg->native_code.         */
			ip = nacl_inverse_modify_patch_target (cfg->native_code) + patch_info->ip.i;
#endif

			mono_domain_lock (domain);
			jlist = g_hash_table_lookup (domain_jit_info (domain)->jump_target_hash, patch_info->data.method);
			if (!jlist) {
				jlist = mono_domain_alloc0 (domain, sizeof (MonoJumpList));
				g_hash_table_insert (domain_jit_info (domain)->jump_target_hash, patch_info->data.method, jlist);
			}
			jlist->list = g_slist_prepend (jlist->list, ip);
			mono_domain_unlock (domain);
			break;
		}
		default:
			/* do nothing */
			break;
		}
	}
}

void
mono_codegen (MonoCompile *cfg)
{
	MonoBasicBlock *bb;
	int max_epilog_size;
	guint8 *code;
	MonoDomain *code_domain;
	guint unwindlen = 0;

	if (mono_using_xdebug)
		/*
		 * Recent gdb versions have trouble processing symbol files containing
		 * overlapping address ranges, so allocate all code from the code manager
		 * of the root domain. (#666152).
		 */
		code_domain = mono_get_root_domain ();
	else
		code_domain = cfg->domain;

#if defined(__native_client_codegen__) && defined(__native_client__)
	void *code_dest;

	/* This keeps patch targets from being transformed during
	 * ordinary method compilation, for local branches and jumps.
	 */
	nacl_allow_target_modification (FALSE);
#endif

	for (bb = cfg->bb_entry; bb; bb = bb->next_bb) {
		cfg->spill_count = 0;
		/* we reuse dfn here */
		/* bb->dfn = bb_count++; */

		mono_arch_lowering_pass (cfg, bb);

		if (cfg->opt & MONO_OPT_PEEPHOLE)
			mono_arch_peephole_pass_1 (cfg, bb);

		mono_local_regalloc (cfg, bb);

		if (cfg->opt & MONO_OPT_PEEPHOLE)
			mono_arch_peephole_pass_2 (cfg, bb);

		if (cfg->gen_seq_points && !cfg->gen_sdb_seq_points)
			mono_bb_deduplicate_op_il_seq_points (cfg, bb);
	}

	if (cfg->prof_options & MONO_PROFILE_COVERAGE)
		cfg->coverage_info = mono_profiler_coverage_alloc (cfg->method, cfg->num_bblocks);

	code = mono_arch_emit_prolog (cfg);

	cfg->code_len = code - cfg->native_code;
	cfg->prolog_end = cfg->code_len;
	cfg->cfa_reg = cfg->cur_cfa_reg;
	cfg->cfa_offset = cfg->cur_cfa_offset;

	mono_debug_open_method (cfg);

	/* emit code all basic blocks */
	for (bb = cfg->bb_entry; bb; bb = bb->next_bb) {
		bb->native_offset = cfg->code_len;
		bb->real_native_offset = cfg->code_len;
		//if ((bb == cfg->bb_entry) || !(bb->region == -1 && !bb->dfn))
			mono_arch_output_basic_block (cfg, bb);
		bb->native_length = cfg->code_len - bb->native_offset;

		if (bb == cfg->bb_exit) {
			cfg->epilog_begin = cfg->code_len;
			mono_arch_emit_epilog (cfg);
			cfg->epilog_end = cfg->code_len;
		}
	}

#ifdef __native_client_codegen__
	mono_nacl_fix_patches (cfg->native_code, cfg->patch_info);
#endif
	mono_arch_emit_exceptions (cfg);

	max_epilog_size = 0;

	/* we always allocate code in cfg->domain->code_mp to increase locality */
	cfg->code_size = cfg->code_len + max_epilog_size;
#ifdef __native_client_codegen__
	cfg->code_size = NACL_BUNDLE_ALIGN_UP (cfg->code_size);
#endif
	/* fixme: align to MONO_ARCH_CODE_ALIGNMENT */

#ifdef MONO_ARCH_HAVE_UNWIND_TABLE
	unwindlen = mono_arch_unwindinfo_get_size (cfg->arch.unwindinfo);
#endif

	if (cfg->method->dynamic) {
		/* Allocate the code into a separate memory pool so it can be freed */
		cfg->dynamic_info = g_new0 (MonoJitDynamicMethodInfo, 1);
		cfg->dynamic_info->code_mp = mono_code_manager_new_dynamic ();
		mono_domain_lock (cfg->domain);
		mono_dynamic_code_hash_insert (cfg->domain, cfg->method, cfg->dynamic_info);
		mono_domain_unlock (cfg->domain);

		if (mono_using_xdebug)
			/* See the comment for cfg->code_domain */
			code = mono_domain_code_reserve (code_domain, cfg->code_size + cfg->thunk_area + unwindlen);
		else
			code = mono_code_manager_reserve (cfg->dynamic_info->code_mp, cfg->code_size + cfg->thunk_area + unwindlen);
	} else {
		code = mono_domain_code_reserve (code_domain, cfg->code_size + cfg->thunk_area + unwindlen);
	}
#if defined(__native_client_codegen__) && defined(__native_client__)
	nacl_allow_target_modification (TRUE);
#endif
	if (cfg->thunk_area) {
		cfg->thunks_offset = cfg->code_size + unwindlen;
		cfg->thunks = code + cfg->thunks_offset;
		memset (cfg->thunks, 0, cfg->thunk_area);
	}

	g_assert (code);
	memcpy (code, cfg->native_code, cfg->code_len);
#if defined(__default_codegen__)
	g_free (cfg->native_code);
#elif defined(__native_client_codegen__)
	if (cfg->native_code_alloc) {
		g_free (cfg->native_code_alloc);
		cfg->native_code_alloc = 0;
	}
	else if (cfg->native_code) {
		g_free (cfg->native_code);
	}
#endif /* __native_client_codegen__ */
	cfg->native_code = code;
	code = cfg->native_code + cfg->code_len;
  
	/* g_assert (((int)cfg->native_code & (MONO_ARCH_CODE_ALIGNMENT - 1)) == 0); */
	mono_postprocess_patches (cfg);

#ifdef VALGRIND_JIT_REGISTER_MAP
	if (valgrind_register){
		char* nm = mono_method_full_name (cfg->method, TRUE);
		VALGRIND_JIT_REGISTER_MAP (nm, cfg->native_code, cfg->native_code + cfg->code_len);
		g_free (nm);
	}
#endif
 
	if (cfg->verbose_level > 0) {
		char* nm = mono_method_full_name (cfg->method, TRUE);
		g_print ("Method %s emitted at %p to %p (code length %d) [%s]\n", 
				 nm, 
				 cfg->native_code, cfg->native_code + cfg->code_len, cfg->code_len, cfg->domain->friendly_name);
		g_free (nm);
	}

	{
		gboolean is_generic = FALSE;

		if (cfg->method->is_inflated || mono_method_get_generic_container (cfg->method) ||
				cfg->method->klass->generic_container || cfg->method->klass->generic_class) {
			is_generic = TRUE;
		}

		if (cfg->gshared)
			g_assert (is_generic);
	}

#ifdef MONO_ARCH_HAVE_SAVE_UNWIND_INFO
	mono_arch_save_unwind_info (cfg);
#endif

#if defined(__native_client_codegen__) && defined(__native_client__)
	if (!cfg->compile_aot) {
		if (cfg->method->dynamic) {
			code_dest = nacl_code_manager_get_code_dest(cfg->dynamic_info->code_mp, cfg->native_code);
		} else {
			code_dest = nacl_domain_get_code_dest(cfg->domain, cfg->native_code);
		}
	}
#endif

#if defined(__native_client_codegen__)
	mono_nacl_fix_patches (cfg->native_code, cfg->patch_info);
#endif

#ifdef MONO_ARCH_HAVE_PATCH_CODE_NEW
	{
		MonoJumpInfo *ji;
		gpointer target;

		for (ji = cfg->patch_info; ji; ji = ji->next) {
			if (cfg->compile_aot) {
				switch (ji->type) {
				case MONO_PATCH_INFO_BB:
				case MONO_PATCH_INFO_LABEL:
					break;
				default:
					/* No need to patch these */
					continue;
				}
			}

			if (ji->type == MONO_PATCH_INFO_NONE)
				continue;

			target = mono_resolve_patch_target (cfg->method, cfg->domain, cfg->native_code, ji, cfg->run_cctors);
			mono_arch_patch_code_new (cfg, cfg->domain, cfg->native_code, ji, target);
		}
	}
#else
	mono_arch_patch_code (cfg, cfg->method, cfg->domain, cfg->native_code, cfg->patch_info, cfg->run_cctors);
#endif

	if (cfg->method->dynamic) {
		if (mono_using_xdebug)
			mono_domain_code_commit (code_domain, cfg->native_code, cfg->code_size, cfg->code_len);
		else
			mono_code_manager_commit (cfg->dynamic_info->code_mp, cfg->native_code, cfg->code_size, cfg->code_len);
	} else {
		mono_domain_code_commit (code_domain, cfg->native_code, cfg->code_size, cfg->code_len);
	}
#if defined(__native_client_codegen__) && defined(__native_client__)
	cfg->native_code = code_dest;
#endif
	mono_profiler_code_buffer_new (cfg->native_code, cfg->code_len, MONO_PROFILER_CODE_BUFFER_METHOD, cfg->method);
	
	mono_arch_flush_icache (cfg->native_code, cfg->code_len);

	mono_debug_close_method (cfg);

#ifdef MONO_ARCH_HAVE_UNWIND_TABLE
	mono_arch_unwindinfo_install_unwind_info (&cfg->arch.unwindinfo, cfg->native_code, cfg->code_len);
#endif
}

static void
compute_reachable (MonoBasicBlock *bb)
{
	int i;

	if (!(bb->flags & BB_VISITED)) {
		bb->flags |= BB_VISITED;
		for (i = 0; i < bb->out_count; ++i)
			compute_reachable (bb->out_bb [i]);
	}
}

static void
mono_handle_out_of_line_bblock (MonoCompile *cfg)
{
	MonoBasicBlock *bb;
	for (bb = cfg->bb_entry; bb; bb = bb->next_bb) {
		if (bb->next_bb && bb->next_bb->out_of_line && bb->last_ins && !MONO_IS_BRANCH_OP (bb->last_ins)) {
			MonoInst *ins;
			MONO_INST_NEW (cfg, ins, OP_BR);
			MONO_ADD_INS (bb, ins);
			ins->inst_target_bb = bb->next_bb;
		}
	}
}

static MonoJitInfo*
create_jit_info (MonoCompile *cfg, MonoMethod *method_to_compile)
{
	GSList *tmp;
	MonoMethodHeader *header;
	MonoJitInfo *jinfo;
	MonoJitInfoFlags flags = JIT_INFO_NONE;
	int num_clauses, num_holes = 0;
	guint32 stack_size = 0;

	g_assert (method_to_compile == cfg->method);
	header = cfg->header;

	if (cfg->gshared)
		flags |= JIT_INFO_HAS_GENERIC_JIT_INFO;

	if (cfg->arch_eh_jit_info) {
		MonoJitArgumentInfo *arg_info;
		MonoMethodSignature *sig = mono_method_signature (cfg->method_to_register);

		/*
		 * This cannot be computed during stack walking, as
		 * mono_arch_get_argument_info () is not signal safe.
		 */
		arg_info = g_newa (MonoJitArgumentInfo, sig->param_count + 1);
		stack_size = mono_arch_get_argument_info (sig, sig->param_count, arg_info);

		if (stack_size)
			flags |= JIT_INFO_HAS_ARCH_EH_INFO;
	}

	if (cfg->has_unwind_info_for_epilog && !(flags & JIT_INFO_HAS_ARCH_EH_INFO))
		flags |= JIT_INFO_HAS_ARCH_EH_INFO;

	if (cfg->thunk_area)
		flags |= JIT_INFO_HAS_THUNK_INFO;

	if (cfg->try_block_holes) {
		for (tmp = cfg->try_block_holes; tmp; tmp = tmp->next) {
			TryBlockHole *hole = tmp->data;
			MonoExceptionClause *ec = hole->clause;
			int hole_end = hole->basic_block->native_offset + hole->basic_block->native_length;
			MonoBasicBlock *clause_last_bb = cfg->cil_offset_to_bb [ec->try_offset + ec->try_len];
			g_assert (clause_last_bb);

			/* Holes at the end of a try region can be represented by simply reducing the size of the block itself.*/
			if (clause_last_bb->native_offset != hole_end)
				++num_holes;
		}
		if (num_holes)
			flags |= JIT_INFO_HAS_TRY_BLOCK_HOLES;
		if (G_UNLIKELY (cfg->verbose_level >= 4))
			printf ("Number of try block holes %d\n", num_holes);
	}

	if (COMPILE_LLVM (cfg))
		num_clauses = cfg->llvm_ex_info_len;
	else
		num_clauses = header->num_clauses;

	if (cfg->method->dynamic)
		jinfo = g_malloc0 (mono_jit_info_size (flags, num_clauses, num_holes));
	else
		jinfo = mono_domain_alloc0 (cfg->domain, mono_jit_info_size (flags, num_clauses, num_holes));
	mono_jit_info_init (jinfo, cfg->method_to_register, cfg->native_code, cfg->code_len, flags, num_clauses, num_holes);
	jinfo->domain_neutral = (cfg->opt & MONO_OPT_SHARED) != 0;

	if (COMPILE_LLVM (cfg))
		jinfo->from_llvm = TRUE;

	if (cfg->gshared) {
		MonoInst *inst;
		MonoGenericJitInfo *gi;
		GSList *loclist = NULL;

		gi = mono_jit_info_get_generic_jit_info (jinfo);
		g_assert (gi);

		if (cfg->method->dynamic)
			gi->generic_sharing_context = g_new0 (MonoGenericSharingContext, 1);
		else
			gi->generic_sharing_context = mono_domain_alloc0 (cfg->domain, sizeof (MonoGenericSharingContext));
		mini_init_gsctx (cfg->method->dynamic ? NULL : cfg->domain, NULL, cfg->gsctx_context, gi->generic_sharing_context);

		if ((method_to_compile->flags & METHOD_ATTRIBUTE_STATIC) ||
				mini_method_get_context (method_to_compile)->method_inst ||
				method_to_compile->klass->valuetype) {
			g_assert (cfg->rgctx_var);
		}

		gi->has_this = 1;

		if ((method_to_compile->flags & METHOD_ATTRIBUTE_STATIC) ||
				mini_method_get_context (method_to_compile)->method_inst ||
				method_to_compile->klass->valuetype) {
			inst = cfg->rgctx_var;
			if (!COMPILE_LLVM (cfg))
				g_assert (inst->opcode == OP_REGOFFSET);
			loclist = cfg->rgctx_loclist;
		} else {
			inst = cfg->args [0];
			loclist = cfg->this_loclist;
		}

		if (loclist) {
			/* Needed to handle async exceptions */
			GSList *l;
			int i;

			gi->nlocs = g_slist_length (loclist);
			if (cfg->method->dynamic)
				gi->locations = g_malloc0 (gi->nlocs * sizeof (MonoDwarfLocListEntry));
			else
				gi->locations = mono_domain_alloc0 (cfg->domain, gi->nlocs * sizeof (MonoDwarfLocListEntry));
			i = 0;
			for (l = loclist; l; l = l->next) {
				memcpy (&(gi->locations [i]), l->data, sizeof (MonoDwarfLocListEntry));
				i ++;
			}
		}

		if (COMPILE_LLVM (cfg)) {
			g_assert (cfg->llvm_this_reg != -1);
			gi->this_in_reg = 0;
			gi->this_reg = cfg->llvm_this_reg;
			gi->this_offset = cfg->llvm_this_offset;
		} else if (inst->opcode == OP_REGVAR) {
			gi->this_in_reg = 1;
			gi->this_reg = inst->dreg;
		} else {
			g_assert (inst->opcode == OP_REGOFFSET);
#ifdef TARGET_X86
			g_assert (inst->inst_basereg == X86_EBP);
#elif defined(TARGET_AMD64)
			g_assert (inst->inst_basereg == X86_EBP || inst->inst_basereg == X86_ESP);
#endif
			g_assert (inst->inst_offset >= G_MININT32 && inst->inst_offset <= G_MAXINT32);

			gi->this_in_reg = 0;
			gi->this_reg = inst->inst_basereg;
			gi->this_offset = inst->inst_offset;
		}
	}

	if (num_holes) {
		MonoTryBlockHoleTableJitInfo *table;
		int i;

		table = mono_jit_info_get_try_block_hole_table_info (jinfo);
		table->num_holes = (guint16)num_holes;
		i = 0;
		for (tmp = cfg->try_block_holes; tmp; tmp = tmp->next) {
			guint32 start_bb_offset;
			MonoTryBlockHoleJitInfo *hole;
			TryBlockHole *hole_data = tmp->data;
			MonoExceptionClause *ec = hole_data->clause;
			int hole_end = hole_data->basic_block->native_offset + hole_data->basic_block->native_length;
			MonoBasicBlock *clause_last_bb = cfg->cil_offset_to_bb [ec->try_offset + ec->try_len];
			g_assert (clause_last_bb);

			/* Holes at the end of a try region can be represented by simply reducing the size of the block itself.*/
			if (clause_last_bb->native_offset == hole_end)
				continue;

			start_bb_offset = hole_data->start_offset - hole_data->basic_block->native_offset;
			hole = &table->holes [i++];
			hole->clause = hole_data->clause - &header->clauses [0];
			hole->offset = (guint32)hole_data->start_offset;
			hole->length = (guint16)(hole_data->basic_block->native_length - start_bb_offset);

			if (G_UNLIKELY (cfg->verbose_level >= 4))
				printf ("\tTry block hole at eh clause %d offset %x length %x\n", hole->clause, hole->offset, hole->length);
		}
		g_assert (i == num_holes);
	}

	if (jinfo->has_arch_eh_info) {
		MonoArchEHJitInfo *info;

		info = mono_jit_info_get_arch_eh_info (jinfo);

		info->stack_size = stack_size;
	}

	if (cfg->thunk_area) {
		MonoThunkJitInfo *info;

		info = mono_jit_info_get_thunk_info (jinfo);
		info->thunks_offset = cfg->thunks_offset;
		info->thunks_size = cfg->thunk_area;
	}

	if (COMPILE_LLVM (cfg)) {
		if (num_clauses)
			memcpy (&jinfo->clauses [0], &cfg->llvm_ex_info [0], num_clauses * sizeof (MonoJitExceptionInfo));
	} else if (header->num_clauses) {
		int i;

		for (i = 0; i < header->num_clauses; i++) {
			MonoExceptionClause *ec = &header->clauses [i];
			MonoJitExceptionInfo *ei = &jinfo->clauses [i];
			MonoBasicBlock *tblock;
			MonoInst *exvar, *spvar;

			ei->flags = ec->flags;

			if (G_UNLIKELY (cfg->verbose_level >= 4))
				printf ("IL clause: try 0x%x-0x%x handler 0x%x-0x%x filter 0x%x\n", ec->try_offset, ec->try_offset + ec->try_len, ec->handler_offset, ec->handler_offset + ec->handler_len, ec->flags == MONO_EXCEPTION_CLAUSE_FILTER ? ec->data.filter_offset : 0);

			/*
			 * The spvars are needed by mono_arch_install_handler_block_guard ().
			 */
			if (ei->flags == MONO_EXCEPTION_CLAUSE_FINALLY) {
				int region;

				region = ((i + 1) << 8) | MONO_REGION_FINALLY | ec->flags;
				spvar = mono_find_spvar_for_region (cfg, region);
				g_assert (spvar);
				ei->exvar_offset = spvar->inst_offset;
			} else {
				exvar = mono_find_exvar_for_offset (cfg, ec->handler_offset);
				ei->exvar_offset = exvar ? exvar->inst_offset : 0;
			}

			if (ei->flags == MONO_EXCEPTION_CLAUSE_FILTER) {
				tblock = cfg->cil_offset_to_bb [ec->data.filter_offset];
				g_assert (tblock);
				ei->data.filter = cfg->native_code + tblock->native_offset;
			} else {
				ei->data.catch_class = ec->data.catch_class;
			}

			tblock = cfg->cil_offset_to_bb [ec->try_offset];
			g_assert (tblock);
			g_assert (tblock->native_offset);
			ei->try_start = cfg->native_code + tblock->native_offset;
			if (tblock->extend_try_block) {
				/*
				 * Extend the try block backwards to include parts of the previous call
				 * instruction.
				 */
				ei->try_start = (guint8*)ei->try_start - cfg->monitor_enter_adjustment;
			}
			tblock = cfg->cil_offset_to_bb [ec->try_offset + ec->try_len];
			g_assert (tblock);
			if (!tblock->native_offset) {
				int j, end;
				for (j = ec->try_offset + ec->try_len, end = ec->try_offset; j >= end; --j) {
					MonoBasicBlock *bb = cfg->cil_offset_to_bb [j];
					if (bb && bb->native_offset) {
						tblock = bb;
						break;
					}
				}
			}
			ei->try_end = cfg->native_code + tblock->native_offset;
			g_assert (tblock->native_offset);
			tblock = cfg->cil_offset_to_bb [ec->handler_offset];
			g_assert (tblock);
			ei->handler_start = cfg->native_code + tblock->native_offset;

			for (tmp = cfg->try_block_holes; tmp; tmp = tmp->next) {
				TryBlockHole *hole = tmp->data;
				gpointer hole_end = cfg->native_code + (hole->basic_block->native_offset + hole->basic_block->native_length);
				if (hole->clause == ec && hole_end == ei->try_end) {
					if (G_UNLIKELY (cfg->verbose_level >= 4))
						printf ("\tShortening try block %d from %x to %x\n", i, (int)((guint8*)ei->try_end - cfg->native_code), hole->start_offset);

					ei->try_end = cfg->native_code + hole->start_offset;
					break;
				}
			}

			if (ec->flags == MONO_EXCEPTION_CLAUSE_FINALLY) {
				int end_offset;
				if (ec->handler_offset + ec->handler_len < header->code_size) {
					tblock = cfg->cil_offset_to_bb [ec->handler_offset + ec->handler_len];
					if (tblock->native_offset) {
						end_offset = tblock->native_offset;
					} else {
						int j, end;

						for (j = ec->handler_offset + ec->handler_len, end = ec->handler_offset; j >= end; --j) {
							MonoBasicBlock *bb = cfg->cil_offset_to_bb [j];
							if (bb && bb->native_offset) {
								tblock = bb;
								break;
							}
						}
						end_offset = tblock->native_offset +  tblock->native_length;
					}
				} else {
					end_offset = cfg->epilog_begin;
				}
				ei->data.handler_end = cfg->native_code + end_offset;
			}
		}
	}

	if (G_UNLIKELY (cfg->verbose_level >= 4)) {
		int i;
		for (i = 0; i < jinfo->num_clauses; i++) {
			MonoJitExceptionInfo *ei = &jinfo->clauses [i];
			int start = (guint8*)ei->try_start - cfg->native_code;
			int end = (guint8*)ei->try_end - cfg->native_code;
			int handler = (guint8*)ei->handler_start - cfg->native_code;
			int handler_end = (guint8*)ei->data.handler_end - cfg->native_code;

			printf ("JitInfo EH clause %d flags %x try %x-%x handler %x-%x\n", i, ei->flags, start, end, handler, handler_end);
		}
	}

	if (cfg->encoded_unwind_ops) {
		/* Generated by LLVM */
		jinfo->unwind_info = mono_cache_unwind_info (cfg->encoded_unwind_ops, cfg->encoded_unwind_ops_len);
		g_free (cfg->encoded_unwind_ops);
	} else if (cfg->unwind_ops) {
		guint32 info_len;
		guint8 *unwind_info = mono_unwind_ops_encode (cfg->unwind_ops, &info_len);
		guint32 unwind_desc;

		unwind_desc = mono_cache_unwind_info (unwind_info, info_len);

		if (cfg->has_unwind_info_for_epilog) {
			MonoArchEHJitInfo *info;

			info = mono_jit_info_get_arch_eh_info (jinfo);
			g_assert (info);
			info->epilog_size = cfg->code_len - cfg->epilog_begin;
		}
		jinfo->unwind_info = unwind_desc;
		g_free (unwind_info);
	} else {
		jinfo->unwind_info = cfg->used_int_regs;
	}

	return jinfo;
}

/* Return whenever METHOD is a gsharedvt method */
static gboolean
is_gsharedvt_method (MonoMethod *method)
{
	MonoGenericContext *context;
	MonoGenericInst *inst;
	int i;

	if (!method->is_inflated)
		return FALSE;
	context = mono_method_get_context (method);
	inst = context->class_inst;
	if (inst) {
		for (i = 0; i < inst->type_argc; ++i)
			if (mini_is_gsharedvt_gparam (inst->type_argv [i]))
				return TRUE;
	}
	inst = context->method_inst;
	if (inst) {
		for (i = 0; i < inst->type_argc; ++i)
			if (mini_is_gsharedvt_gparam (inst->type_argv [i]))
				return TRUE;
	}
	return FALSE;
}

static gboolean
is_open_method (MonoMethod *method)
{
	MonoGenericContext *context;

	if (!method->is_inflated)
		return FALSE;
	context = mono_method_get_context (method);
	if (context->class_inst && context->class_inst->is_open)
		return TRUE;
	if (context->method_inst && context->method_inst->is_open)
		return TRUE;
	return FALSE;
}

#if defined(__native_client_codegen__) || USE_COOP_GC

static void
mono_create_gc_safepoint (MonoCompile *cfg, MonoBasicBlock *bblock)
{
	MonoInst *poll_addr, *ins;
	if (cfg->verbose_level > 1)
		printf ("ADDING SAFE POINT TO BB %d\n", bblock->block_num);

#if defined(__native_client_codegen__)
	NEW_AOTCONST (cfg, poll_addr, MONO_PATCH_INFO_GC_SAFE_POINT_FLAG, (gpointer)&__nacl_thread_suspension_needed);
#else
	NEW_AOTCONST (cfg, poll_addr, MONO_PATCH_INFO_GC_SAFE_POINT_FLAG, (gpointer)&mono_polling_required);
#endif

	MONO_INST_NEW (cfg, ins, OP_GC_SAFE_POINT);
	ins->sreg1 = poll_addr->dreg;

	 if (bblock->flags & BB_EXCEPTION_HANDLER) {
		MonoInst *eh_op = bblock->code;

		if (eh_op && eh_op->opcode != OP_START_HANDLER && eh_op->opcode != OP_GET_EX_OBJ) {
			eh_op = NULL;
		} else {
			MonoInst *next_eh_op = eh_op ? eh_op->next : NULL;
			// skip all EH relateds ops
			while (next_eh_op && (next_eh_op->opcode == OP_START_HANDLER || next_eh_op->opcode == OP_GET_EX_OBJ)) {
				eh_op = next_eh_op;
				next_eh_op = eh_op->next;
			}
		}

		mono_bblock_insert_after_ins (bblock, eh_op, poll_addr);
		mono_bblock_insert_after_ins (bblock, poll_addr, ins);
	} else if (bblock == cfg->bb_entry) {
		mono_bblock_insert_after_ins (bblock, bblock->last_ins, poll_addr);
		mono_bblock_insert_after_ins (bblock, poll_addr, ins);

	} else {
		mono_bblock_insert_before_ins (bblock, NULL, poll_addr);
		mono_bblock_insert_after_ins (bblock, poll_addr, ins);
	}
}

/*
This code inserts safepoints into managed code at important code paths.
Those are:

-the first basic block
-landing BB for exception handlers
-loop body starts.

*/
static void
mono_insert_safepoints (MonoCompile *cfg)
{
	MonoBasicBlock *bb;
	if (cfg->method->wrapper_type == MONO_WRAPPER_MANAGED_TO_NATIVE) {
		WrapperInfo *info = mono_marshal_get_wrapper_info (cfg->method);
#if defined(__native_client__) || defined(__native_client_codegen__)
		gpointer poll_func = &mono_nacl_gc;
#elif defined(USE_COOP_GC)
		gpointer poll_func = &mono_threads_state_poll;
#else
		gpointer poll_func = NULL;
#endif

		if (info && info->subtype == WRAPPER_SUBTYPE_ICALL_WRAPPER && info->d.icall.func == poll_func) {
			if (cfg->verbose_level > 1)
				printf ("SKIPPING SAFEPOINTS for the polling function icall\n");
			return;
		}
	}

	if (cfg->method->wrapper_type == MONO_WRAPPER_NATIVE_TO_MANAGED) {
		if (cfg->verbose_level > 1)
			printf ("SKIPPING SAFEPOINTS for native-to-managed wrappers.\n");
		return;
	}

	if (cfg->method->wrapper_type == MONO_WRAPPER_MANAGED_TO_NATIVE) {
		WrapperInfo *info = mono_marshal_get_wrapper_info (cfg->method);

		if (info && info->subtype == WRAPPER_SUBTYPE_ICALL_WRAPPER &&
			(info->d.icall.func == mono_thread_interruption_checkpoint ||
			info->d.icall.func == mono_threads_finish_blocking ||
			info->d.icall.func == mono_threads_reset_blocking_start)) {
			/* These wrappers are called from the wrapper for the polling function, leading to potential stack overflow */
			if (cfg->verbose_level > 1)
				printf ("SKIPPING SAFEPOINTS for wrapper %s\n", cfg->method->name);
			return;
		}
	}

	if (cfg->verbose_level > 1)
		printf ("INSERTING SAFEPOINTS\n");
	if (cfg->verbose_level > 2)
		mono_print_code (cfg, "BEFORE SAFEPOINTS");

	for (bb = cfg->bb_entry; bb; bb = bb->next_bb) {
		if (bb->loop_body_start || bb == cfg->bb_entry || bb->flags & BB_EXCEPTION_HANDLER)
			mono_create_gc_safepoint (cfg, bb);
	}

	if (cfg->verbose_level > 2)
		mono_print_code (cfg, "AFTER SAFEPOINTS");

}

#else

static void
mono_insert_safepoints (MonoCompile *cfg)
{
}

#endif

static void
init_compile (MonoCompile *cfg)
{
#ifdef MONO_ARCH_NEED_GOT_VAR
	if (cfg->compile_aot)
		cfg->need_got_var = 1;
#endif
#ifdef MONO_ARCH_HAVE_CARD_TABLE_WBARRIER
	cfg->have_card_table_wb = 1;
#endif
#ifdef MONO_ARCH_HAVE_OP_GENERIC_CLASS_INIT
	cfg->have_op_generic_class_init = 1;
#endif
#ifdef MONO_ARCH_EMULATE_MUL_DIV
	cfg->emulate_mul_div = 1;
#endif
#ifdef MONO_ARCH_EMULATE_DIV
	cfg->emulate_div = 1;
#endif
#if !defined(MONO_ARCH_NO_EMULATE_LONG_SHIFT_OPS)
	cfg->emulate_long_shift_opts = 1;
#endif
#ifdef MONO_ARCH_HAVE_OBJC_GET_SELECTOR
	cfg->have_objc_get_selector = 1;
#endif
#ifdef MONO_ARCH_HAVE_GENERALIZED_IMT_THUNK
	cfg->have_generalized_imt_thunk = 1;
#endif
#ifdef MONO_ARCH_GSHARED_SUPPORTED
	cfg->gshared_supported = 1;
#endif
	if (MONO_ARCH_HAVE_TLS_GET)
		cfg->have_tls_get = 1;
#ifdef MONO_ARCH_HAVE_TLS_GET_REG
		cfg->have_tls_get_reg = 1;
#endif
	if (MONO_ARCH_USE_FPSTACK)
		cfg->use_fpstack = 1;
#ifdef MONO_ARCH_HAVE_LIVERANGE_OPS
	cfg->have_liverange_ops = 1;
#endif
#ifdef MONO_ARCH_HAVE_OP_TAIL_CALL
	cfg->have_op_tail_call = 1;
#endif
#ifndef MONO_ARCH_MONITOR_ENTER_ADJUSTMENT
	cfg->monitor_enter_adjustment = 1;
#else
	cfg->monitor_enter_adjustment = MONO_ARCH_MONITOR_ENTER_ADJUSTMENT;
#endif
#if defined(__mono_ilp32__)
	cfg->ilp32 = 1;
#endif
#ifdef MONO_ARCH_HAVE_DUMMY_INIT
	cfg->have_dummy_init = 1;
#endif
#ifdef MONO_ARCH_DYN_CALL_PARAM_AREA
	cfg->dyn_call_param_area = MONO_ARCH_DYN_CALL_PARAM_AREA;
#endif
}

/*
 * mini_method_compile:
 * @method: the method to compile
 * @opts: the optimization flags to use
 * @domain: the domain where the method will be compiled in
 * @flags: compilation flags
 * @parts: debug flag
 *
 * Returns: a MonoCompile* pointer. Caller must check the exception_type
 * field in the returned struct to see if compilation succeded.
 */
MonoCompile*
mini_method_compile (MonoMethod *method, guint32 opts, MonoDomain *domain, JitFlags flags, int parts, int aot_method_index)
{
	MonoMethodHeader *header;
	MonoMethodSignature *sig;
	MonoError err;
	MonoCompile *cfg;
	int dfn, i, code_size_ratio;
	gboolean try_generic_shared, try_llvm = FALSE;
	MonoMethod *method_to_compile, *method_to_register;
	gboolean method_is_gshared = FALSE;
	gboolean run_cctors = (flags & JIT_FLAG_RUN_CCTORS) ? 1 : 0;
	gboolean compile_aot = (flags & JIT_FLAG_AOT) ? 1 : 0;
	gboolean full_aot = (flags & JIT_FLAG_FULL_AOT) ? 1 : 0;
	gboolean disable_direct_icalls = (flags & JIT_FLAG_NO_DIRECT_ICALLS) ? 1 : 0;
	gboolean gsharedvt_method = FALSE;
#ifdef ENABLE_LLVM
	gboolean llvm = (flags & JIT_FLAG_LLVM) ? 1 : 0;
#endif
	static gboolean verbose_method_inited;
	static const char *verbose_method_name;

	InterlockedIncrement (&mono_jit_stats.methods_compiled);
	if (mono_profiler_get_events () & MONO_PROFILE_JIT_COMPILATION)
		mono_profiler_method_jit (method);
	if (MONO_METHOD_COMPILE_BEGIN_ENABLED ())
		MONO_PROBE_METHOD_COMPILE_BEGIN (method);

	gsharedvt_method = is_gsharedvt_method (method);

	/*
	 * In AOT mode, method can be the following:
	 * - a gsharedvt method.
	 * - a method inflated with type parameters. This is for ref/partial sharing.
	 * - a method inflated with concrete types.
	 */
	if (compile_aot) {
		if (is_open_method (method)) {
			try_generic_shared = TRUE;
			method_is_gshared = TRUE;
		} else {
			try_generic_shared = FALSE;
		}
		g_assert (opts & MONO_OPT_GSHARED);
	} else {
		try_generic_shared = mono_class_generic_sharing_enabled (method->klass) &&
			(opts & MONO_OPT_GSHARED) && mono_method_is_generic_sharable (method, FALSE);
		if (mini_is_gsharedvt_sharable_method (method)) {
			if (!mono_debug_count ())
				try_generic_shared = FALSE;
		}
	}

	/*
	if (try_generic_shared && !mono_debug_count ())
		try_generic_shared = FALSE;
	*/

	if (opts & MONO_OPT_GSHARED) {
		if (try_generic_shared)
			mono_stats.generics_sharable_methods++;
		else if (mono_method_is_generic_impl (method))
			mono_stats.generics_unsharable_methods++;
	}

#ifdef ENABLE_LLVM
	try_llvm = mono_use_llvm || llvm;
#endif

 restart_compile:
	if (method_is_gshared) {
		method_to_compile = method;
	} else {
		if (try_generic_shared) {
			method_to_compile = mini_get_shared_method (method);
			g_assert (method_to_compile);
		} else {
			method_to_compile = method;
		}
	}

	cfg = g_new0 (MonoCompile, 1);
	cfg->method = method_to_compile;
	cfg->header = mono_method_get_header (cfg->method);
	cfg->mempool = mono_mempool_new ();
	cfg->opt = opts;
	cfg->prof_options = mono_profiler_get_events ();
	cfg->run_cctors = run_cctors;
	cfg->domain = domain;
	cfg->verbose_level = mini_verbose;
	cfg->compile_aot = compile_aot;
	cfg->full_aot = full_aot;
	cfg->skip_visibility = method->skip_visibility;
	cfg->orig_method = method;
	cfg->gen_seq_points = debug_options.gen_seq_points_compact_data || debug_options.gen_sdb_seq_points;
	cfg->gen_sdb_seq_points = debug_options.gen_sdb_seq_points;
	cfg->llvm_only = (flags & JIT_FLAG_LLVM_ONLY) != 0;

#ifdef PLATFORM_ANDROID
	if (cfg->method->wrapper_type != MONO_WRAPPER_NONE) {
		/* FIXME: Why is this needed */
		cfg->gen_seq_points = FALSE;
		cfg->gen_sdb_seq_points = FALSE;
	}
#endif
	/* coop / nacl requires loop detection to happen */
#if defined(__native_client_codegen__) || defined(USE_COOP_GC)
	cfg->opt |= MONO_OPT_LOOP;
#endif
	cfg->explicit_null_checks = debug_options.explicit_null_checks || (flags & JIT_FLAG_EXPLICIT_NULL_CHECKS);
	cfg->soft_breakpoints = debug_options.soft_breakpoints;
	cfg->check_pinvoke_callconv = debug_options.check_pinvoke_callconv;
	cfg->disable_direct_icalls = disable_direct_icalls;
	if (try_generic_shared)
		cfg->gshared = TRUE;
	cfg->compile_llvm = try_llvm;
	cfg->token_info_hash = g_hash_table_new (NULL, NULL);
	if (cfg->compile_aot)
		cfg->method_index = aot_method_index;

	/*
	if (!mono_debug_count ())
		cfg->opt &= ~MONO_OPT_FLOAT32;
	*/
	if (cfg->llvm_only)
		cfg->opt &= ~MONO_OPT_SIMD;
	cfg->r4fp = (cfg->opt & MONO_OPT_FLOAT32) ? 1 : 0;
	cfg->r4_stack_type = cfg->r4fp ? STACK_R4 : STACK_R8;

	init_compile (cfg);

	if (cfg->gen_seq_points)
		cfg->seq_points = g_ptr_array_new ();
	mono_error_init (&cfg->error);

	if (cfg->compile_aot && !try_generic_shared && (method->is_generic || method->klass->generic_container || method_is_gshared)) {
		cfg->exception_type = MONO_EXCEPTION_GENERIC_SHARING_FAILED;
		return cfg;
	}

	if (cfg->gshared && (gsharedvt_method || mini_is_gsharedvt_sharable_method (method))) {
		MonoMethodInflated *inflated;
		MonoGenericContext *context;

		if (gsharedvt_method) {
			g_assert (method->is_inflated);
			inflated = (MonoMethodInflated*)method;
			context = &inflated->context;

			/* We are compiling a gsharedvt method directly */
			g_assert (compile_aot);
		} else {
			g_assert (method_to_compile->is_inflated);
			inflated = (MonoMethodInflated*)method_to_compile;
			context = &inflated->context;
		}

		mini_init_gsctx (NULL, cfg->mempool, context, &cfg->gsctx);
		cfg->gsctx_context = context;

		cfg->gsharedvt = TRUE;
		// FIXME:
		cfg->disable_llvm = TRUE;
		cfg->exception_message = g_strdup ("gsharedvt");
	}

	if (cfg->gshared) {
		method_to_register = method_to_compile;
	} else {
		g_assert (method == method_to_compile);
		method_to_register = method;
	}
	cfg->method_to_register = method_to_register;

	mono_error_init (&err);
	sig = mono_method_signature_checked (cfg->method, &err);	
	if (!sig) {
		cfg->exception_type = MONO_EXCEPTION_TYPE_LOAD;
		cfg->exception_message = g_strdup (mono_error_get_message (&err));
		mono_error_cleanup (&err);
		if (MONO_METHOD_COMPILE_END_ENABLED ())
			MONO_PROBE_METHOD_COMPILE_END (method, FALSE);
		return cfg;
	}

	header = cfg->header;
	if (!header) {
		MonoLoaderError *error;

		if ((error = mono_loader_get_last_error ())) {
			cfg->exception_type = error->exception_type;
		} else {
			cfg->exception_type = MONO_EXCEPTION_INVALID_PROGRAM;
			cfg->exception_message = g_strdup_printf ("Missing or incorrect header for method %s", cfg->method->name);
		}
		if (MONO_METHOD_COMPILE_END_ENABLED ())
			MONO_PROBE_METHOD_COMPILE_END (method, FALSE);
		return cfg;
	}

#ifdef ENABLE_LLVM
	{
		static gboolean inited;

		if (!inited)
			inited = TRUE;

		/* 
		 * Check for methods which cannot be compiled by LLVM early, to avoid
		 * the extra compilation pass.
		 */
		if (COMPILE_LLVM (cfg)) {
			mono_llvm_check_method_supported (cfg);
			if (cfg->disable_llvm) {
				if (cfg->verbose_level >= cfg->llvm_only ? 0 : 1) {
					//nm = mono_method_full_name (cfg->method, TRUE);
					printf ("LLVM failed for '%s': %s\n", method->name, cfg->exception_message);
					//g_free (nm);
				}
				if (cfg->llvm_only) {
					cfg->disable_aot = TRUE;
					return cfg;
				}
				mono_destroy_compile (cfg);
				try_llvm = FALSE;
				goto restart_compile;
			}
		}
	}
#endif

	/* The debugger has no liveness information, so avoid sharing registers/stack slots */
	if (debug_options.mdb_optimizations) {
		cfg->disable_reuse_registers = TRUE;
		cfg->disable_reuse_stack_slots = TRUE;
		/* 
		 * This decreases the change the debugger will read registers/stack slots which are
		 * not yet initialized.
		 */
		cfg->disable_initlocals_opt = TRUE;

		cfg->extend_live_ranges = TRUE;

		/* Temporarily disable this when running in the debugger until we have support
		 * for this in the debugger. */
		/* This is no longer needed with sdb */
		//cfg->disable_omit_fp = TRUE;

		/* The debugger needs all locals to be on the stack or in a global register */
		cfg->disable_vreg_to_lvreg = TRUE;

		/* Don't remove unused variables when running inside the debugger since the user
		 * may still want to view them. */
		cfg->disable_deadce_vars = TRUE;

		// cfg->opt |= MONO_OPT_SHARED;
		cfg->opt &= ~MONO_OPT_DEADCE;
		cfg->opt &= ~MONO_OPT_INLINE;
		cfg->opt &= ~MONO_OPT_COPYPROP;
		cfg->opt &= ~MONO_OPT_CONSPROP;
		/* This is no longer needed with sdb */
		//cfg->opt &= ~MONO_OPT_GSHARED;

		/* This is needed for the soft debugger, which doesn't like code after the epilog */
		cfg->disable_out_of_line_bblocks = TRUE;
	}

	if (mono_using_xdebug) {
		/* 
		 * Make each variable use its own register/stack slot and extend 
		 * their liveness to cover the whole method, making them displayable
		 * in gdb even after they are dead.
		 */
		cfg->disable_reuse_registers = TRUE;
		cfg->disable_reuse_stack_slots = TRUE;
		cfg->extend_live_ranges = TRUE;
		cfg->compute_precise_live_ranges = TRUE;
	}

	mini_gc_init_cfg (cfg);

	if (COMPILE_LLVM (cfg)) {
		cfg->opt |= MONO_OPT_ABCREM;
	}

	if (!verbose_method_inited) {
		verbose_method_name = g_getenv ("MONO_VERBOSE_METHOD");
		verbose_method_inited = TRUE;
	}
	if (verbose_method_name) {
		const char *name = verbose_method_name;

		if ((strchr (name, '.') > name) || strchr (name, ':')) {
			MonoMethodDesc *desc;
			
			desc = mono_method_desc_new (name, TRUE);
			if (mono_method_desc_full_match (desc, cfg->method)) {
				cfg->verbose_level = 4;
			}
			mono_method_desc_free (desc);
		} else {
			if (strcmp (cfg->method->name, name) == 0)
				cfg->verbose_level = 4;
		}
	}

	cfg->intvars = mono_mempool_alloc0 (cfg->mempool, sizeof (guint16) * STACK_MAX * header->max_stack);

	if (cfg->verbose_level > 0) {
		char *method_name;

		method_name = mono_method_full_name (method, TRUE);
		g_print ("converting %s%s%smethod %s\n", COMPILE_LLVM (cfg) ? "llvm " : "", cfg->gsharedvt ? "gsharedvt " : "", (cfg->gshared && !cfg->gsharedvt) ? "gshared " : "", method_name);
		/*
		if (COMPILE_LLVM (cfg))
			g_print ("converting llvm method %s\n", method_name = mono_method_full_name (method, TRUE));
		else if (cfg->gsharedvt)
			g_print ("converting gsharedvt method %s\n", method_name = mono_method_full_name (method_to_compile, TRUE));
		else if (cfg->gshared)
			g_print ("converting shared method %s\n", method_name = mono_method_full_name (method_to_compile, TRUE));
		else
			g_print ("converting method %s\n", method_name = mono_method_full_name (method, TRUE));
		*/
		g_free (method_name);
	}

	if (cfg->opt & MONO_OPT_ABCREM)
		cfg->opt |= MONO_OPT_SSA;

	cfg->rs = mono_regstate_new ();
	cfg->next_vreg = cfg->rs->next_vreg;

	/* FIXME: Fix SSA to handle branches inside bblocks */
	if (cfg->opt & MONO_OPT_SSA)
		cfg->enable_extended_bblocks = FALSE;

	/*
	 * FIXME: This confuses liveness analysis because variables which are assigned after
	 * a branch inside a bblock become part of the kill set, even though the assignment
	 * might not get executed. This causes the optimize_initlocals pass to delete some
	 * assignments which are needed.
	 * Also, the mono_if_conversion pass needs to be modified to recognize the code
	 * created by this.
	 */
	//cfg->enable_extended_bblocks = TRUE;

	/*We must verify the method before doing any IR generation as mono_compile_create_vars can assert.*/
	if (mono_compile_is_broken (cfg, cfg->method, TRUE)) {
		if (mini_get_debug_options ()->break_on_unverified)
			G_BREAKPOINT ();
		return cfg;
	}

	/*
	 * create MonoInst* which represents arguments and local variables
	 */
	mono_compile_create_vars (cfg);

	i = mono_method_to_ir (cfg, method_to_compile, NULL, NULL, NULL, NULL, 0, FALSE);

	if (i < 0) {
		if (try_generic_shared && cfg->exception_type == MONO_EXCEPTION_GENERIC_SHARING_FAILED) {
			if (compile_aot) {
				if (MONO_METHOD_COMPILE_END_ENABLED ())
					MONO_PROBE_METHOD_COMPILE_END (method, FALSE);
				return cfg;
			}
			mono_destroy_compile (cfg);
			try_generic_shared = FALSE;
			goto restart_compile;
		}
		g_assert (cfg->exception_type != MONO_EXCEPTION_GENERIC_SHARING_FAILED);

		if (MONO_METHOD_COMPILE_END_ENABLED ())
			MONO_PROBE_METHOD_COMPILE_END (method, FALSE);
		/* cfg contains the details of the failure, so let the caller cleanup */
		return cfg;
	}

	cfg->stat_basic_blocks += cfg->num_bblocks;

	if (COMPILE_LLVM (cfg)) {
		MonoInst *ins;

		/* The IR has to be in SSA form for LLVM */
		cfg->opt |= MONO_OPT_SSA;

		// FIXME:
		if (cfg->ret) {
			// Allow SSA on the result value
			cfg->ret->flags &= ~MONO_INST_VOLATILE;

			// Add an explicit return instruction referencing the return value
			MONO_INST_NEW (cfg, ins, OP_SETRET);
			ins->sreg1 = cfg->ret->dreg;

			MONO_ADD_INS (cfg->bb_exit, ins);
		}

		cfg->opt &= ~MONO_OPT_LINEARS;

		/* FIXME: */
		cfg->opt &= ~MONO_OPT_BRANCH;
	}

	/* todo: remove code when we have verified that the liveness for try/catch blocks
	 * works perfectly 
	 */
	/* 
	 * Currently, this can't be commented out since exception blocks are not
	 * processed during liveness analysis.
	 * It is also needed, because otherwise the local optimization passes would
	 * delete assignments in cases like this:
	 * r1 <- 1
	 * <something which throws>
	 * r1 <- 2
	 * This also allows SSA to be run on methods containing exception clauses, since
	 * SSA will ignore variables marked VOLATILE.
	 */
	mono_liveness_handle_exception_clauses (cfg);

	mono_handle_out_of_line_bblock (cfg);

	/*g_print ("numblocks = %d\n", cfg->num_bblocks);*/

	if (!COMPILE_LLVM (cfg))
		mono_decompose_long_opts (cfg);

	/* Should be done before branch opts */
	if (cfg->opt & (MONO_OPT_CONSPROP | MONO_OPT_COPYPROP))
		mono_local_cprop (cfg);

	if (cfg->opt & MONO_OPT_BRANCH)
		mono_optimize_branches (cfg);

	/* This must be done _before_ global reg alloc and _after_ decompose */
	mono_handle_global_vregs (cfg);
	if (cfg->opt & MONO_OPT_DEADCE)
		mono_local_deadce (cfg);
	if (cfg->opt & MONO_OPT_ALIAS_ANALYSIS)
		mono_local_alias_analysis (cfg);
	/* Disable this for LLVM to make the IR easier to handle */
	if (!COMPILE_LLVM (cfg))
		mono_if_conversion (cfg);

	MONO_SUSPEND_CHECK ();

	/* Depth-first ordering on basic blocks */
	cfg->bblocks = mono_mempool_alloc (cfg->mempool, sizeof (MonoBasicBlock*) * (cfg->num_bblocks + 1));

	cfg->max_block_num = cfg->num_bblocks;

	dfn = 0;
	df_visit (cfg->bb_entry, &dfn, cfg->bblocks);
	if (cfg->num_bblocks != dfn + 1) {
		MonoBasicBlock *bb;

		cfg->num_bblocks = dfn + 1;

		/* remove unreachable code, because the code in them may be 
		 * inconsistent  (access to dead variables for example) */
		for (bb = cfg->bb_entry; bb; bb = bb->next_bb)
			bb->flags &= ~BB_VISITED;
		compute_reachable (cfg->bb_entry);
		for (bb = cfg->bb_entry; bb; bb = bb->next_bb)
			if (bb->flags & BB_EXCEPTION_HANDLER)
				compute_reachable (bb);
		for (bb = cfg->bb_entry; bb; bb = bb->next_bb) {
			if (!(bb->flags & BB_VISITED)) {
				if (cfg->verbose_level > 1)
					g_print ("found unreachable code in BB%d\n", bb->block_num);
				bb->code = bb->last_ins = NULL;
				while (bb->out_count)
					mono_unlink_bblock (cfg, bb, bb->out_bb [0]);
			}
		}
		for (bb = cfg->bb_entry; bb; bb = bb->next_bb)
			bb->flags &= ~BB_VISITED;
	}

	if (((cfg->num_varinfo > 2000) || (cfg->num_bblocks > 1000)) && !cfg->compile_aot) {
		/* 
		 * we disable some optimizations if there are too many variables
		 * because JIT time may become too expensive. The actual number needs 
		 * to be tweaked and eventually the non-linear algorithms should be fixed.
		 */
		cfg->opt &= ~ (MONO_OPT_LINEARS | MONO_OPT_COPYPROP | MONO_OPT_CONSPROP);
		cfg->disable_ssa = TRUE;
	}

	if (cfg->opt & MONO_OPT_LOOP) {
		mono_compile_dominator_info (cfg, MONO_COMP_DOM | MONO_COMP_IDOM);
		mono_compute_natural_loops (cfg);
	}

	mono_insert_safepoints (cfg);

	/* after method_to_ir */
	if (parts == 1) {
		if (MONO_METHOD_COMPILE_END_ENABLED ())
			MONO_PROBE_METHOD_COMPILE_END (method, TRUE);
		return cfg;
	}

	/*
	  if (header->num_clauses)
	  cfg->disable_ssa = TRUE;
	*/

//#define DEBUGSSA "logic_run"
//#define DEBUGSSA_CLASS "Tests"
#ifdef DEBUGSSA

	if (!cfg->disable_ssa) {
		mono_local_cprop (cfg);

#ifndef DISABLE_SSA
		mono_ssa_compute (cfg);
#endif
	}
#else 
	if (cfg->opt & MONO_OPT_SSA) {
		if (!(cfg->comp_done & MONO_COMP_SSA) && !cfg->disable_ssa) {
#ifndef DISABLE_SSA
			mono_ssa_compute (cfg);
#endif

			if (cfg->verbose_level >= 2) {
				print_dfn (cfg);
			}
		}
	}
#endif

	/* after SSA translation */
	if (parts == 2) {
		if (MONO_METHOD_COMPILE_END_ENABLED ())
			MONO_PROBE_METHOD_COMPILE_END (method, TRUE);
		return cfg;
	}

	if ((cfg->opt & MONO_OPT_CONSPROP) || (cfg->opt & MONO_OPT_COPYPROP)) {
		if (cfg->comp_done & MONO_COMP_SSA && !COMPILE_LLVM (cfg)) {
#ifndef DISABLE_SSA
			mono_ssa_cprop (cfg);
#endif
		}
	}

#ifndef DISABLE_SSA
	if (cfg->comp_done & MONO_COMP_SSA && !COMPILE_LLVM (cfg)) {
		//mono_ssa_strength_reduction (cfg);

		if (cfg->opt & MONO_OPT_DEADCE)
			mono_ssa_deadce (cfg);

		if ((cfg->flags & (MONO_CFG_HAS_LDELEMA|MONO_CFG_HAS_CHECK_THIS)) && (cfg->opt & MONO_OPT_ABCREM))
			mono_perform_abc_removal (cfg);

		mono_ssa_remove (cfg);
		mono_local_cprop (cfg);
		mono_handle_global_vregs (cfg);
		if (cfg->opt & MONO_OPT_DEADCE)
			mono_local_deadce (cfg);

		if (cfg->opt & MONO_OPT_BRANCH)
			mono_optimize_branches (cfg);
	}
#endif

	if (cfg->comp_done & MONO_COMP_SSA && COMPILE_LLVM (cfg)) {
		mono_ssa_loop_invariant_code_motion (cfg);
		/* This removes MONO_INST_FAULT flags too so perform it unconditionally */
		if (cfg->opt & MONO_OPT_ABCREM)
			mono_perform_abc_removal (cfg);
	}

	/* after SSA removal */
	if (parts == 3) {
		if (MONO_METHOD_COMPILE_END_ENABLED ())
			MONO_PROBE_METHOD_COMPILE_END (method, TRUE);
		return cfg;
	}

#ifdef MONO_ARCH_SOFT_FLOAT_FALLBACK
	if (COMPILE_SOFT_FLOAT (cfg))
		mono_decompose_soft_float (cfg);
#endif
	if (COMPILE_LLVM (cfg))
		mono_decompose_vtype_opts_llvm (cfg);
	else
		mono_decompose_vtype_opts (cfg);
	if (cfg->flags & MONO_CFG_HAS_ARRAY_ACCESS)
		mono_decompose_array_access_opts (cfg);

	if (cfg->got_var) {
#ifndef MONO_ARCH_GOT_REG
		GList *regs;
#endif
		int got_reg;

		g_assert (cfg->got_var_allocated);

		/* 
		 * Allways allocate the GOT var to a register, because keeping it
		 * in memory will increase the number of live temporaries in some
		 * code created by inssel.brg, leading to the well known spills+
		 * branches problem. Testcase: mcs crash in 
		 * System.MonoCustomAttrs:GetCustomAttributes.
		 */
#ifdef MONO_ARCH_GOT_REG
		got_reg = MONO_ARCH_GOT_REG;
#else
		regs = mono_arch_get_global_int_regs (cfg);
		g_assert (regs);
		got_reg = GPOINTER_TO_INT (regs->data);
		g_list_free (regs);
#endif
		cfg->got_var->opcode = OP_REGVAR;
		cfg->got_var->dreg = got_reg;
		cfg->used_int_regs |= 1LL << cfg->got_var->dreg;
	}

	/*
	 * Have to call this again to process variables added since the first call.
	 */
	mono_liveness_handle_exception_clauses (cfg);

	if (cfg->opt & MONO_OPT_LINEARS) {
		GList *vars, *regs, *l;
		
		/* fixme: maybe we can avoid to compute livenesss here if already computed ? */
		cfg->comp_done &= ~MONO_COMP_LIVENESS;
		if (!(cfg->comp_done & MONO_COMP_LIVENESS))
			mono_analyze_liveness (cfg);

		if ((vars = mono_arch_get_allocatable_int_vars (cfg))) {
			regs = mono_arch_get_global_int_regs (cfg);
			/* Remove the reg reserved for holding the GOT address */
			if (cfg->got_var) {
				for (l = regs; l; l = l->next) {
					if (GPOINTER_TO_UINT (l->data) == cfg->got_var->dreg) {
						regs = g_list_delete_link (regs, l);
						break;
					}
				}
			}
			mono_linear_scan (cfg, vars, regs, &cfg->used_int_regs);
		}
	}

	//mono_print_code (cfg, "");

    //print_dfn (cfg);
	
	/* variables are allocated after decompose, since decompose could create temps */
	if (!COMPILE_LLVM (cfg)) {
		mono_arch_allocate_vars (cfg);
		if (cfg->exception_type)
			return cfg;
	}

	{
		MonoBasicBlock *bb;
		gboolean need_local_opts;

		if (!COMPILE_LLVM (cfg)) {
			mono_spill_global_vars (cfg, &need_local_opts);

			if (need_local_opts || cfg->compile_aot) {
				/* To optimize code created by spill_global_vars */
				mono_local_cprop (cfg);
				if (cfg->opt & MONO_OPT_DEADCE)
					mono_local_deadce (cfg);
			}
		}

		/* Add branches between non-consecutive bblocks */
		for (bb = cfg->bb_entry; bb; bb = bb->next_bb) {
			if (bb->last_ins && MONO_IS_COND_BRANCH_OP (bb->last_ins) &&
				bb->last_ins->inst_false_bb && bb->next_bb != bb->last_ins->inst_false_bb) {
				/* we are careful when inverting, since bugs like #59580
				 * could show up when dealing with NaNs.
				 */
				if (MONO_IS_COND_BRANCH_NOFP(bb->last_ins) && bb->next_bb == bb->last_ins->inst_true_bb) {
					MonoBasicBlock *tmp =  bb->last_ins->inst_true_bb;
					bb->last_ins->inst_true_bb = bb->last_ins->inst_false_bb;
					bb->last_ins->inst_false_bb = tmp;

					bb->last_ins->opcode = mono_reverse_branch_op (bb->last_ins->opcode);
				} else {			
					MonoInst *inst = mono_mempool_alloc0 (cfg->mempool, sizeof (MonoInst));
					inst->opcode = OP_BR;
					inst->inst_target_bb = bb->last_ins->inst_false_bb;
					mono_bblock_add_inst (bb, inst);
				}
			}
		}

		if (cfg->verbose_level >= 4) {
			for (bb = cfg->bb_entry; bb; bb = bb->next_bb) {
				MonoInst *tree = bb->code;	
				g_print ("DUMP BLOCK %d:\n", bb->block_num);
				if (!tree)
					continue;
				for (; tree; tree = tree->next) {
					mono_print_ins_index (-1, tree);
				}
			}
		}

		/* FIXME: */
		for (bb = cfg->bb_entry; bb; bb = bb->next_bb) {
			bb->max_vreg = cfg->next_vreg;
		}
	}

	if (COMPILE_LLVM (cfg)) {
#ifdef ENABLE_LLVM
		char *nm;

		/* The IR has to be in SSA form for LLVM */
		if (!(cfg->comp_done & MONO_COMP_SSA)) {
			cfg->exception_message = g_strdup ("SSA disabled.");
			cfg->disable_llvm = TRUE;
		}

		if (cfg->flags & MONO_CFG_HAS_ARRAY_ACCESS)
			mono_decompose_array_access_opts (cfg);

		if (!cfg->disable_llvm)
			mono_llvm_emit_method (cfg);
		if (cfg->disable_llvm) {
			if (cfg->verbose_level >= cfg->llvm_only ? 0 : 1) {
				//nm = mono_method_full_name (cfg->method, TRUE);
				printf ("LLVM failed for '%s': %s\n", method->name, cfg->exception_message);
				//g_free (nm);
			}
			if (cfg->llvm_only) {
				cfg->disable_aot = TRUE;
				return cfg;
			}
			mono_destroy_compile (cfg);
			try_llvm = FALSE;
			goto restart_compile;
		}

		if (cfg->verbose_level > 0 && !cfg->compile_aot) {
			nm = mono_method_full_name (cfg->method, TRUE);
			g_print ("LLVM Method %s emitted at %p to %p (code length %d) [%s]\n", 
					 nm, 
					 cfg->native_code, cfg->native_code + cfg->code_len, cfg->code_len, cfg->domain->friendly_name);
			g_free (nm);
		}
#endif
	} else {
		mono_codegen (cfg);
	}

	if (COMPILE_LLVM (cfg))
		InterlockedIncrement (&mono_jit_stats.methods_with_llvm);
	else
		InterlockedIncrement (&mono_jit_stats.methods_without_llvm);

	cfg->jit_info = create_jit_info (cfg, method_to_compile);

#ifdef MONO_ARCH_HAVE_LIVERANGE_OPS
	if (cfg->extend_live_ranges) {
		/* Extend live ranges to cover the whole method */
		for (i = 0; i < cfg->num_varinfo; ++i)
			MONO_VARINFO (cfg, i)->live_range_end = cfg->code_len;
	}
#endif

	if (!cfg->compile_aot)
		mono_save_xdebug_info (cfg);

	mini_gc_create_gc_map (cfg);
 
	mono_save_seq_point_info (cfg);

	if (cfg->verbose_level >= 2) {
		char *id =  mono_method_full_name (cfg->method, FALSE);
		mono_disassemble_code (cfg, cfg->native_code, cfg->code_len, id + 3);
		g_free (id);
	}

	if (!cfg->compile_aot) {
		mono_domain_lock (cfg->domain);
		mono_jit_info_table_add (cfg->domain, cfg->jit_info);

		if (cfg->method->dynamic)
			mono_dynamic_code_hash_lookup (cfg->domain, cfg->method)->ji = cfg->jit_info;
		mono_domain_unlock (cfg->domain);
	}

#if 0
	if (cfg->gsharedvt)
		printf ("GSHAREDVT: %s\n", mono_method_full_name (cfg->method, TRUE));
#endif

	/* collect statistics */
#ifndef DISABLE_PERFCOUNTERS
	mono_perfcounters->jit_methods++;
	mono_perfcounters->jit_bytes += header->code_size;
#endif
	mono_jit_stats.allocated_code_size += cfg->code_len;
	code_size_ratio = cfg->code_len;
	if (code_size_ratio > mono_jit_stats.biggest_method_size && mono_jit_stats.enabled) {
		mono_jit_stats.biggest_method_size = code_size_ratio;
		g_free (mono_jit_stats.biggest_method);
		mono_jit_stats.biggest_method = g_strdup_printf ("%s::%s)", method->klass->name, method->name);
	}
	code_size_ratio = (code_size_ratio * 100) / header->code_size;
	if (code_size_ratio > mono_jit_stats.max_code_size_ratio && mono_jit_stats.enabled) {
		mono_jit_stats.max_code_size_ratio = code_size_ratio;
		g_free (mono_jit_stats.max_ratio_method);
		mono_jit_stats.max_ratio_method = g_strdup_printf ("%s::%s)", method->klass->name, method->name);
	}
	mono_jit_stats.native_code_size += cfg->code_len;

	if (MONO_METHOD_COMPILE_END_ENABLED ())
		MONO_PROBE_METHOD_COMPILE_END (method, TRUE);

	return cfg;
}

void*
mono_arch_instrument_epilog (MonoCompile *cfg, void *func, void *p, gboolean enable_arguments)
{
	return mono_arch_instrument_epilog_full (cfg, func, p, enable_arguments, FALSE);
}

void
mono_cfg_add_try_hole (MonoCompile *cfg, MonoExceptionClause *clause, guint8 *start, MonoBasicBlock *bb)
{
	TryBlockHole *hole = mono_mempool_alloc (cfg->mempool, sizeof (TryBlockHole));
	hole->clause = clause;
	hole->start_offset = start - cfg->native_code;
	hole->basic_block = bb;

	cfg->try_block_holes = g_slist_append_mempool (cfg->mempool, cfg->try_block_holes, hole);
}

void
mono_cfg_set_exception (MonoCompile *cfg, int type)
{
	cfg->exception_type = type;
}

#endif /* DISABLE_JIT */

static MonoJitInfo*
create_jit_info_for_trampoline (MonoMethod *wrapper, MonoTrampInfo *info)
{
	MonoDomain *domain = mono_get_root_domain ();
	MonoJitInfo *jinfo;
	guint8 *uw_info;
	guint32 info_len;

	if (info->uw_info) {
		uw_info = info->uw_info;
		info_len = info->uw_info_len;
	} else {
		uw_info = mono_unwind_ops_encode (info->unwind_ops, &info_len);
	}

	jinfo = mono_domain_alloc0 (domain, MONO_SIZEOF_JIT_INFO);
	jinfo->d.method = wrapper;
	jinfo->code_start = info->code;
	jinfo->code_size = info->code_size;
	jinfo->unwind_info = mono_cache_unwind_info (uw_info, info_len);

	if (!info->uw_info)
		g_free (uw_info);

	return jinfo;
}

/*
 * mono_jit_compile_method_inner:
 *
 *   Main entry point for the JIT.
 */
gpointer
mono_jit_compile_method_inner (MonoMethod *method, MonoDomain *target_domain, int opt, MonoException **jit_ex)
{
	MonoCompile *cfg;
	gpointer code = NULL;
	MonoJitInfo *jinfo, *info;
	MonoVTable *vtable;
	MonoException *ex = NULL;
	guint32 prof_options;
	GTimer *jit_timer;
	MonoMethod *prof_method, *shared;

	if ((method->iflags & METHOD_IMPL_ATTRIBUTE_INTERNAL_CALL) ||
	    (method->flags & METHOD_ATTRIBUTE_PINVOKE_IMPL)) {
		MonoMethod *nm;
		MonoMethodPInvoke* piinfo = (MonoMethodPInvoke *) method;

		if (!piinfo->addr) {
			if (method->iflags & METHOD_IMPL_ATTRIBUTE_INTERNAL_CALL)
				piinfo->addr = mono_lookup_internal_call (method);
			else if (method->iflags & METHOD_IMPL_ATTRIBUTE_NATIVE)
#ifdef HOST_WIN32
				g_warning ("Method '%s' in assembly '%s' contains native code that cannot be executed by Mono in modules loaded from byte arrays. The assembly was probably created using C++/CLI.\n", mono_method_full_name (method, TRUE), method->klass->image->name);
#else
				g_warning ("Method '%s' in assembly '%s' contains native code that cannot be executed by Mono on this platform. The assembly was probably created using C++/CLI.\n", mono_method_full_name (method, TRUE), method->klass->image->name);
#endif
			else
				mono_lookup_pinvoke_call (method, NULL, NULL);
		}
		nm = mono_marshal_get_native_wrapper (method, TRUE, mono_aot_only);
		code = mono_get_addr_from_ftnptr (mono_compile_method (nm));
		jinfo = mono_jit_info_table_find (target_domain, code);
		if (!jinfo)
			jinfo = mono_jit_info_table_find (mono_domain_get (), code);
		if (jinfo)
			mono_profiler_method_end_jit (method, jinfo, MONO_PROFILE_OK);
		return code;
	} else if ((method->iflags & METHOD_IMPL_ATTRIBUTE_RUNTIME)) {
		const char *name = method->name;
		char *full_name, *msg;
		MonoMethod *nm;

		if (method->klass->parent == mono_defaults.multicastdelegate_class) {
			if (*name == '.' && (strcmp (name, ".ctor") == 0)) {
				MonoJitICallInfo *mi = mono_find_jit_icall_by_name ("mono_delegate_ctor");
				g_assert (mi);
				/*
				 * We need to make sure this wrapper
				 * is compiled because it might end up
				 * in an (M)RGCTX if generic sharing
				 * is enabled, and would be called
				 * indirectly.  If it were a
				 * trampoline we'd try to patch that
				 * indirect call, which is not
				 * possible.
				 */
				return mono_get_addr_from_ftnptr ((gpointer)mono_icall_get_wrapper_full (mi, TRUE));
			} else if (*name == 'I' && (strcmp (name, "Invoke") == 0)) {
				if (mono_llvm_only) {
					nm = mono_marshal_get_delegate_invoke (method, NULL);
					return mono_get_addr_from_ftnptr (mono_compile_method (nm));
				}
				return mono_create_delegate_trampoline (target_domain, method->klass);
			} else if (*name == 'B' && (strcmp (name, "BeginInvoke") == 0)) {
				nm = mono_marshal_get_delegate_begin_invoke (method);
				return mono_get_addr_from_ftnptr (mono_compile_method (nm));
			} else if (*name == 'E' && (strcmp (name, "EndInvoke") == 0)) {
				nm = mono_marshal_get_delegate_end_invoke (method);
				return mono_get_addr_from_ftnptr (mono_compile_method (nm));
			}
		}

		full_name = mono_method_full_name (method, TRUE);
		msg = g_strdup_printf ("Unrecognizable runtime implemented method '%s'", full_name);
		*jit_ex = mono_exception_from_name_msg (mono_defaults.corlib, "System", "InvalidProgramException", msg);
		g_free (full_name);
		g_free (msg);
		return NULL;
	}

	if (method->wrapper_type == MONO_WRAPPER_UNKNOWN) {
		WrapperInfo *info = mono_marshal_get_wrapper_info (method);

		if (info->subtype == WRAPPER_SUBTYPE_GSHAREDVT_IN || info->subtype == WRAPPER_SUBTYPE_GSHAREDVT_OUT) {
			static MonoTrampInfo *in_tinfo, *out_tinfo;
			MonoTrampInfo *tinfo;
			MonoJitInfo *jinfo;
			gboolean is_in = info->subtype == WRAPPER_SUBTYPE_GSHAREDVT_IN;

			if (is_in && in_tinfo)
				return in_tinfo->code;
			else if (!is_in && out_tinfo)
				return out_tinfo->code;

			/*
			 * This is a special wrapper whose body is implemented in assembly, like a trampoline. We use a wrapper so EH
			 * works.
			 * FIXME: The caller signature doesn't match the callee, which might cause problems on some platforms
			 */
			if (mono_aot_only)
				mono_aot_get_trampoline_full (is_in ? "gsharedvt_trampoline" : "gsharedvt_out_trampoline", &tinfo);
			else
				mono_arch_get_gsharedvt_trampoline (&tinfo, FALSE);
			jinfo = create_jit_info_for_trampoline (method, tinfo);
			mono_jit_info_table_add (mono_get_root_domain (), jinfo);
			if (is_in)
				in_tinfo = tinfo;
			else
				out_tinfo = tinfo;
			return tinfo->code;
		}
	}

	if (mono_aot_only) {
		char *fullname = mono_method_full_name (method, TRUE);
		char *msg = g_strdup_printf ("Attempting to JIT compile method '%s' while running with --aot-only. See http://docs.xamarin.com/ios/about/limitations for more information.\n", fullname);

		*jit_ex = mono_get_exception_execution_engine (msg);
		g_free (fullname);
		g_free (msg);
		
		return NULL;
	}

	jit_timer = g_timer_new ();

	cfg = mini_method_compile (method, opt, target_domain, JIT_FLAG_RUN_CCTORS, 0, -1);
	prof_method = cfg->method;

	g_timer_stop (jit_timer);
	mono_jit_stats.jit_time += g_timer_elapsed (jit_timer, NULL);
	g_timer_destroy (jit_timer);

	switch (cfg->exception_type) {
	case MONO_EXCEPTION_NONE:
		break;
	case MONO_EXCEPTION_TYPE_LOAD:
	case MONO_EXCEPTION_MISSING_FIELD:
	case MONO_EXCEPTION_MISSING_METHOD:
	case MONO_EXCEPTION_FILE_NOT_FOUND:
	case MONO_EXCEPTION_BAD_IMAGE: {
		/* Throw a type load exception if needed */
		MonoLoaderError *error = mono_loader_get_last_error ();

		if (error) {
			ex = mono_loader_error_prepare_exception (error);
		} else {
			if (cfg->exception_ptr) {
				ex = mono_class_get_exception_for_failure (cfg->exception_ptr);
			} else {
				if (cfg->exception_type == MONO_EXCEPTION_MISSING_FIELD)
					ex = mono_exception_from_name_msg (mono_defaults.corlib, "System", "MissingFieldException", cfg->exception_message);
				else if (cfg->exception_type == MONO_EXCEPTION_MISSING_METHOD)
					ex = mono_exception_from_name_msg (mono_defaults.corlib, "System", "MissingMethodException", cfg->exception_message);
				else if (cfg->exception_type == MONO_EXCEPTION_TYPE_LOAD)
					ex = mono_exception_from_name_msg (mono_defaults.corlib, "System", "TypeLoadException", cfg->exception_message);
				else if (cfg->exception_type == MONO_EXCEPTION_FILE_NOT_FOUND)
					ex = mono_exception_from_name_msg (mono_defaults.corlib, "System.IO", "FileNotFoundException", cfg->exception_message);
				else if (cfg->exception_type == MONO_EXCEPTION_BAD_IMAGE)
					ex = mono_get_exception_bad_image_format (cfg->exception_message);
				else
					g_assert_not_reached ();
			}
		}
		break;
	}
	case MONO_EXCEPTION_INVALID_PROGRAM:
		ex = mono_exception_from_name_msg (mono_defaults.corlib, "System", "InvalidProgramException", cfg->exception_message);
		break;
	case MONO_EXCEPTION_UNVERIFIABLE_IL:
		ex = mono_exception_from_name_msg (mono_defaults.corlib, "System.Security", "VerificationException", cfg->exception_message);
		break;
	case MONO_EXCEPTION_METHOD_ACCESS:
		ex = mono_exception_from_name_msg (mono_defaults.corlib, "System", "MethodAccessException", cfg->exception_message);
		break;
	case MONO_EXCEPTION_FIELD_ACCESS:
		ex = mono_exception_from_name_msg (mono_defaults.corlib, "System", "FieldAccessException", cfg->exception_message);
		break;
	case MONO_EXCEPTION_OBJECT_SUPPLIED: {
		MonoException *exp = cfg->exception_ptr;
		MONO_GC_UNREGISTER_ROOT (cfg->exception_ptr);

		ex = exp;
		break;
	}
	case MONO_EXCEPTION_OUT_OF_MEMORY:
		ex = mono_domain_get ()->out_of_memory_ex;
		break;
	case MONO_EXCEPTION_MONO_ERROR:
		g_assert (!mono_error_ok (&cfg->error));
		ex = mono_error_convert_to_exception (&cfg->error);
		break;
	default:
		g_assert_not_reached ();
	}

	if (ex) {
		if (cfg->prof_options & MONO_PROFILE_JIT_COMPILATION)
			mono_profiler_method_end_jit (method, NULL, MONO_PROFILE_FAILED);

		mono_destroy_compile (cfg);
		*jit_ex = ex;

		return NULL;
	}

	if (mono_method_is_generic_sharable (method, FALSE))
		shared = mini_get_shared_method (method);
	else
		shared = NULL;

	mono_domain_lock (target_domain);

	/* Check if some other thread already did the job. In this case, we can
       discard the code this thread generated. */

	info = mini_lookup_method (target_domain, method, shared);
	if (info) {
		/* We can't use a domain specific method in another domain */
		if ((target_domain == mono_domain_get ()) || info->domain_neutral) {
			code = info->code_start;
//			printf("Discarding code for method %s\n", method->name);
		}
	}
	if (code == NULL) {
		/* The lookup + insert is atomic since this is done inside the domain lock */
		mono_domain_jit_code_hash_lock (target_domain);
		mono_internal_hash_table_insert (&target_domain->jit_code_hash, cfg->jit_info->d.method, cfg->jit_info);
		mono_domain_jit_code_hash_unlock (target_domain);

		code = cfg->native_code;

		if (cfg->gshared && mono_method_is_generic_sharable (method, FALSE))
			mono_stats.generics_shared_methods++;
		if (cfg->gsharedvt)
			mono_stats.gsharedvt_methods++;
	}

	jinfo = cfg->jit_info;

	prof_options = cfg->prof_options;

	/*
	 * Update global stats while holding a lock, instead of doing many
	 * InterlockedIncrement operations during JITting.
	 */
	mono_jit_stats.allocate_var += cfg->stat_allocate_var;
	mono_jit_stats.locals_stack_size += cfg->stat_locals_stack_size;
	mono_jit_stats.basic_blocks += cfg->stat_basic_blocks;
	mono_jit_stats.max_basic_blocks = MAX (cfg->stat_basic_blocks, mono_jit_stats.max_basic_blocks);
	mono_jit_stats.cil_code_size += cfg->stat_cil_code_size;
	mono_jit_stats.regvars += cfg->stat_n_regvars;
	mono_jit_stats.inlineable_methods += cfg->stat_inlineable_methods;
	mono_jit_stats.inlined_methods += cfg->stat_inlined_methods;
	mono_jit_stats.code_reallocs += cfg->stat_code_reallocs;

	mono_destroy_compile (cfg);

#ifndef DISABLE_JIT
	if (domain_jit_info (target_domain)->jump_target_hash) {
		MonoJumpInfo patch_info;
		MonoJumpList *jlist;
		GSList *tmp;
		jlist = g_hash_table_lookup (domain_jit_info (target_domain)->jump_target_hash, method);
		if (jlist) {
			patch_info.next = NULL;
			patch_info.ip.i = 0;
			patch_info.type = MONO_PATCH_INFO_METHOD_JUMP;
			patch_info.data.method = method;
			g_hash_table_remove (domain_jit_info (target_domain)->jump_target_hash, method);

#if defined(__native_client_codegen__) && defined(__native_client__)
			/* These patches are applied after a method has been installed, no target munging is needed. */
			nacl_allow_target_modification (FALSE);
#endif
#ifdef MONO_ARCH_HAVE_PATCH_CODE_NEW
			for (tmp = jlist->list; tmp; tmp = tmp->next) {
				gpointer target = mono_resolve_patch_target (NULL, target_domain, tmp->data, &patch_info, TRUE);
				mono_arch_patch_code_new (NULL, target_domain, tmp->data, &patch_info, target);
			}
#else
			for (tmp = jlist->list; tmp; tmp = tmp->next)
				mono_arch_patch_code (NULL, NULL, target_domain, tmp->data, &patch_info, TRUE);
#endif
#if defined(__native_client_codegen__) && defined(__native_client__)
			nacl_allow_target_modification (TRUE);
#endif
		}
	}

	mono_emit_jit_map (jinfo);
#endif
	mono_domain_unlock (target_domain);

	vtable = mono_class_vtable (target_domain, method->klass);
	if (!vtable) {
		ex = mono_class_get_exception_for_failure (method->klass);
		g_assert (ex);
		*jit_ex = ex;
		return NULL;
	}

	if (prof_options & MONO_PROFILE_JIT_COMPILATION) {
		if (method->wrapper_type == MONO_WRAPPER_MANAGED_TO_NATIVE) {
			if (mono_marshal_method_from_wrapper (method)) {
				/* Native func wrappers have no method */
				/* The profiler doesn't know about wrappers, so pass the original icall method */
				mono_profiler_method_end_jit (mono_marshal_method_from_wrapper (method), jinfo, MONO_PROFILE_OK);
			}
		}
		mono_profiler_method_end_jit (method, jinfo, MONO_PROFILE_OK);
		if (prof_method != method) {
			mono_profiler_method_end_jit (prof_method, jinfo, MONO_PROFILE_OK);
		}
	}

	ex = mono_runtime_class_init_full (vtable, FALSE);
	if (ex) {
		*jit_ex = ex;
		return NULL;
	}
	return code;
}

/*
 * mini_get_underlying_type:
 *
 *   Return the type the JIT will use during compilation.
 * Handles: byref, enums, native types, generic sharing.
 * For gsharedvt types, it will return the original VAR/MVAR.
 */
MonoType*
mini_get_underlying_type (MonoType *type)
{
	return mini_type_get_underlying_type (type);
}

void
mini_jit_init (void)
{
	mono_mutex_init_recursive (&jit_mutex);
}

void
mini_jit_cleanup (void)
{
#ifndef DISABLE_JIT
	g_free (emul_opcode_map);
	g_free (emul_opcode_opcodes);
#endif
}

#ifndef ENABLE_LLVM
void
mono_llvm_emit_aot_file_info (MonoAotFileInfo *info, gboolean has_jitted_code)
{
	g_assert_not_reached ();
}

void mono_llvm_emit_aot_data (const char *symbol, guint8 *data, int data_len)
{
	g_assert_not_reached ();
}

void
mono_llvm_throw_corlib_exception (guint32 ex_token_index)
{
	g_assert_not_reached ();
}

#endif

#ifdef DISABLE_JIT

MonoCompile*
mini_method_compile (MonoMethod *method, guint32 opts, MonoDomain *domain, JitFlags flags, int parts, int aot_method_index)
{
	g_assert_not_reached ();
	return NULL;
}

void
mono_destroy_compile (MonoCompile *cfg)
{
	g_assert_not_reached ();
}

void
mono_add_patch_info (MonoCompile *cfg, int ip, MonoJumpInfoType type, gconstpointer target)
{
	g_assert_not_reached ();
}

#endif /* DISABLE_JIT */
