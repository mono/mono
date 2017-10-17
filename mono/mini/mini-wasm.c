#include "mini.h"
#include "ir-emit.h"
#include "cpu-wasm.h"

#include "binaryen-c.h"

void mono_wasm_create_module (void);
void mono_wasm_emit_aot_data (const char *symbol, guint8 *data, int data_len);
void mono_wasm_emit_aot_file_info (MonoAotFileInfo *info, gboolean has_jitted_code);
void mono_wasm_emit_module (const char *filename);
void mono_wasm_code_gen (MonoCompile *cfg);



gpointer
mono_arch_get_this_arg_from_call (mgreg_t *regs, guint8 *code)
{
	g_error ("mono_arch_get_this_arg_from_call");
}

gpointer
mono_arch_get_delegate_virtual_invoke_impl (MonoMethodSignature *sig, MonoMethod *method, int offset, gboolean load_imt_reg)
{
	g_error ("mono_arch_get_delegate_virtual_invoke_impl");
}


void
mono_arch_cpu_init (void)
{
	// printf ("mono_arch_cpu_init\n");
}

void
mono_arch_finish_init (void)
{
	// printf ("mono_arch_finish_init\n");
}

void
mono_arch_init (void)
{
	// printf ("mono_arch_init\n");
}

void
mono_arch_cleanup (void)
{
}

void
mono_arch_register_lowlevel_calls (void)
{
}

void
mono_arch_flush_register_windows (void)
{
}

void
mono_arch_free_jit_tls_data (MonoJitTlsData *tls)
{
}


MonoMethod*
mono_arch_find_imt_method (mgreg_t *regs, guint8 *code)
{
	g_error ("mono_arch_find_static_call_vtable");
	return (MonoMethod*) regs [MONO_ARCH_IMT_REG];
}

MonoVTable*
mono_arch_find_static_call_vtable (mgreg_t *regs, guint8 *code)
{
	g_error ("mono_arch_find_static_call_vtable");
	return (MonoVTable*) regs [MONO_ARCH_RGCTX_REG];
}

gpointer
mono_arch_build_imt_trampoline (MonoVTable *vtable, MonoDomain *domain, MonoIMTCheckItem **imt_entries, int count, gpointer fail_tramp)
{
	g_error ("mono_arch_build_imt_trampoline");
}

guint32
mono_arch_cpu_enumerate_simd_versions (void)
{
	return 0;
}

guint32
mono_arch_cpu_optimizations (guint32 *exclude_mask)
{
	return 0;
}

GSList*
mono_arch_get_delegate_invoke_impls (void)
{
	g_error ("mono_arch_get_delegate_invoke_impls");
	return NULL;
}

gpointer
mono_arch_get_delegate_invoke_impl (MonoMethodSignature *sig, gboolean has_target)
{
	g_error ("mono_arch_get_delegate_invoke_impl");
	return NULL;
}

mgreg_t
mono_arch_context_get_int_reg (MonoContext *ctx, int reg)
{
	g_error ("mono_arch_context_get_int_reg");
	return 0;
}

int
mono_arch_get_argument_info (MonoMethodSignature *csig, int param_count, MonoJitArgumentInfo *arg_info)
{
	g_error ("mono_arch_get_argument_info");
	return 0;

}

void
mono_arch_init_lmf_ext (MonoLMFExt *ext, gpointer prev_lmf)
{
	ext->lmf.previous_lmf = prev_lmf;
	/* Mark that this is a MonoLMFExt */
	ext->lmf.previous_lmf = (gpointer)(((gssize)ext->lmf.previous_lmf) | 2);
}

#if defined (HOST_WASM)


/*
The following functions don't belong here, but are due to laziness.
*/

//w32file-wasm.c
gboolean
mono_w32file_get_volume_information (const gunichar2 *path, gunichar2 *volumename, gint volumesize, gint *outserial, gint *maxcomp, gint *fsflags, gunichar2 *fsbuffer, gint fsbuffersize)
{
	g_error ("mono_w32file_get_volume_information");
}


//misc runtime funcs
gboolean
MONO_SIG_HANDLER_SIGNATURE (mono_chain_signal)
{
	g_error ("mono_chain_signal");
	
	return FALSE;
}

gboolean
mono_thread_state_init_from_handle (MonoThreadUnwindState *tctx, MonoThreadInfo *info)
{
	g_error ("WASM systems don't support mono_thread_state_init_from_handle");
	return FALSE;
}

void
mono_runtime_setup_stat_profiler (void)
{
	g_error ("mono_runtime_setup_stat_profiler");
}


void
mono_runtime_shutdown_stat_profiler (void)
{
	g_error ("mono_runtime_shutdown_stat_profiler");
}


void
mono_runtime_install_handlers (void)
{
}

void
mono_runtime_cleanup_handlers (void)
{
}

//libc / libpthread missing bits from musl or shit we didn't detect :facepalm:
int pthread_getschedparam (pthread_t thread, int *policy, struct sched_param *param)
{
	g_error ("pthread_getschedparam");
	return 0;
}

int
pthread_attr_getstacksize (const pthread_attr_t *restrict attr, size_t *restrict stacksize)
{
	return 65536; //wasm page size
}

int
pthread_sigmask (int how, const sigset_t * restrict set, sigset_t * restrict oset)
{
	return 0;
}


int
sigsuspend(const sigset_t *sigmask)
{
	g_error ("sigsuspend");
	return 0;
}

int
getdtablesize (void)
{
	return 256; //random constant that is the fd limit
}

void *
getgrnam (const char *name)
{
	return NULL;
}

void *
getgrgid (gid_t gid)
{
	return NULL;
}

int
inotify_init (void)
{
	g_error ("inotify_init");
}

int
inotify_rm_watch (int fd, int wd)
{
	g_error ("inotify_rm_watch");
	return 0;
}

int
inotify_add_watch (int fd, const char *pathname, uint32_t mask)
{
	g_error ("inotify_add_watch");
	return 0;
}

int
sem_timedwait (sem_t *sem, const struct timespec *abs_timeout)
{
	g_error ("sem_timedwait");
	return 0;
	
}
#endif

gboolean
mono_arch_have_fast_tls (void)
{
	//We got the fastest TLS on earth, it's called no threads
	return TRUE;
}

#ifndef DISABLE_JIT

gboolean 
mono_arch_is_inst_imm (gint64 imm)
{
	//Encoding is not our problem!
	return TRUE;
}

/*
 * Return whether @opcode is suppported on the current target.
 */
gboolean
mono_arch_opcode_supported (int opcode)
{
	//XXX we got no armv7 nonsense to deal with
	return TRUE;
}

/*
 * mono_arch_lowering_pass:
 *
 *  Converts complex opcodes into simpler ones so that each IR instruction
 * corresponds to one machine instruction.
 */
void
mono_arch_lowering_pass (MonoCompile *cfg, MonoBasicBlock *bb)
{
	//XXX right now we got nothing to decompose
}

void
mono_arch_allocate_vars (MonoCompile *cfg)
{
	g_error ("convert func args into BP loads");
}

void
mono_arch_create_vars (MonoCompile *cfg)
{
	if (cfg->method->save_lmf)
		cfg->create_lmf_var = TRUE;

	if (cfg->method->save_lmf) {
		cfg->lmf_ir = TRUE;
	}

	//XXX what else?
}

void
mono_arch_emit_call (MonoCompile *cfg, MonoCallInst *call)
{
	g_error ("Emit calls");
}

void
mono_arch_emit_epilog (MonoCompile *cfg)
{
	g_error ("mono_arch_emit_epilog");
	//XXX do we do anything here?
}

guint8 *
mono_arch_emit_prolog (MonoCompile *cfg)
{
	g_error ("mono_arch_emit_prolog");
	//XXX shall we do something here?
}

void*
mono_arch_instrument_epilog_full (MonoCompile *cfg, void *func, void *p, gboolean enable_arguments, gboolean preserve_argument_registers)
{
	g_error ("mono_arch_instrument_epilog_full");
	//XXX rather not
}

void
mono_arch_output_basic_block (MonoCompile *cfg, MonoBasicBlock *bb)
{
	g_error ("mono_arch_output_basic_block");
	//The big code gen switch
}

void
mono_arch_patch_code_new (MonoCompile *cfg, MonoDomain *domain, guint8 *code, MonoJumpInfo *ji, gpointer target)
{
	g_error ("mono_arch_patch_code_new");
	//figure out patching
}

/* Return the name of a register */
const char*
mono_arch_regname (int reg)
{
	return "WASM HAS NO REGISTERS!";
}

/* Return the name of a float register */

const char*
mono_arch_fregname (int reg)
{
	return "WASM HAS NO FP REGS!";
}

/* WTF this do? */
guint32
mono_arch_get_patch_offset (guint8 *code)
{
	g_error ("mono_arch_get_patch_offset");
	return 1;
}

/*
 * mono_arch_get_plt_info_offset:
 *
 *   Return the PLT info offset belonging to the plt entry PLT_ENTRY.
 */
guint32
mono_arch_get_plt_info_offset (guint8 *plt_entry, mgreg_t *regs, guint8 *code)
{
	g_error ("mono_arch_get_plt_info_offset");
	return 0;
}

/*
 * mono_arch_peephole_pass_1:
 *
 *   Perform peephole opts which should/can be performed before local regalloc
 */
void
mono_arch_peephole_pass_1 (MonoCompile *cfg, MonoBasicBlock *bb)
{
}

/*
 * mono_arch_peephole_pass_2:
 *
 *   Perform peephole opts that is not ?mono_arch_peephole_pass_1?
 */

void
mono_arch_peephole_pass_2 (MonoCompile *cfg, MonoBasicBlock *bb)
{
}


/*
 * mono_arch_regalloc_cost:
 *
 *  Return the cost, in number of memory references, of the action of 
 * allocating the variable VMV into a register during global register
 * allocation.
 */
guint32
mono_arch_regalloc_cost (MonoCompile *cfg, MonoMethodVar *vmv)
{
	return 0;
	//it's regalloc, for real
}


void
mono_arch_emit_exceptions (MonoCompile *cfg)
{
	g_error ("mono_arch_emit_exceptions");
	//XXX what is this?
}

MonoInst*
mono_arch_emit_inst_for_method (MonoCompile *cfg, MonoMethod *cmethod, MonoMethodSignature *fsig, MonoInst **args)
{
	//This is for arch intrinsics!
	return NULL;
}

void
mono_arch_emit_outarg_vt (MonoCompile *cfg, MonoInst *ins, MonoInst *src)
{
	g_error ("mono_arch_emit_outarg_vt");
	//XXX what is this?
}


void
mono_arch_emit_setret (MonoCompile *cfg, MonoMethod *method, MonoInst *val)
{
	//XXX no clue on this puppy
	// g_error ("mono_arch_emit_setret");
	MonoType *ret = mini_get_underlying_type (mono_method_signature (method)->ret);

	if (ret->type == MONO_TYPE_R4) {
		MONO_EMIT_NEW_UNALU (cfg, OP_RMOVE, cfg->ret->dreg, val->dreg);
	} else if (ret->type == MONO_TYPE_R8) {
		MONO_EMIT_NEW_UNALU (cfg, OP_FMOVE, cfg->ret->dreg, val->dreg);
	} else {
		MONO_EMIT_NEW_UNALU (cfg, OP_MOVE, cfg->ret->dreg, val->dreg);
	}
}

void
mono_arch_flush_icache (guint8 *code, gint size)
{
	// LOLWUT?
}

GList *
mono_arch_get_allocatable_int_vars (MonoCompile *cfg)
{
	g_error ("mono_arch_get_allocatable_int_vars");
	//XXX there's no regalloc in wasmlandia
	return NULL;
}

GList *
mono_arch_get_global_int_regs (MonoCompile *cfg)
{
	g_error ("mono_arch_get_global_int_regs");
	//XXX how cute, trying to regalloc
	return NULL;	
}



//AOT support

static BinaryenModuleRef aot_module;
static GPtrArray *all_funcs;
typedef struct {
	GHashTable *vreg_to_wasm;
	GPtrArray *vars;
	int verbose_level;
} WasmCodeGen;

static BinaryenType
mono_type_to_wasm_type (MonoType *type)
{
	switch (type->type) {
	case MONO_TYPE_I1:
	case MONO_TYPE_U1:
	case MONO_TYPE_BOOLEAN:
	case MONO_TYPE_I2:
	case MONO_TYPE_U2:
	case MONO_TYPE_I4:
	case MONO_TYPE_U4:
		return BinaryenInt32 ();
	default:
		printf ("no clue on how to handle: %s\n", mono_type_full_name (type));
		g_assert_not_reached ();
	}
}

static gboolean
op_is_op_imm (int op)
{
	if (op >= OP_IADD_IMM && op <= OP_ISHR_UN_IMM)
		return TRUE;
	if (op >= OP_ADD_IMM && op <= OP_SHR_UN_IMM)
		return TRUE;
	if (op >= OP_STORE_MEMBASE_IMM && op <= OP_STOREI8_MEMBASE_IMM)
		return TRUE;
	if (op >= OP_LADD_IMM && op <= OP_LREM_UN_IMM)
		return TRUE;
	switch (op) {
	case OP_COMPARE_IMM:
	case OP_ICOMPARE_IMM:
	case OP_LCOMPARE_IMM:
	case OP_LOCALLOC_IMM:
	case OP_STORE_MEM_IMM:
	case OP_IADC_IMM:
	case OP_ISBB_IMM:
	case OP_ADC_IMM:
	case OP_SBB_IMM:
	case OP_ADDCC_IMM:
	case OP_SUBCC_IMM:
		return TRUE;
	}
	return FALSE;
}

static void
cg_init (WasmCodeGen *cg, MonoCompile *cfg)
{
	cg->vreg_to_wasm = g_hash_table_new (g_direct_hash, g_direct_equal);
	cg->vars = g_ptr_array_new ();
	cg->verbose_level = cfg->verbose_level;
}

static BinaryenIndex
get_var (WasmCodeGen *cg, int vreg, MonoType *type)
{
	gpointer res = g_hash_table_lookup (cg->vreg_to_wasm, GINT_TO_POINTER (vreg));
	if (res)
		return GPOINTER_TO_INT (res) - 1;

	int var_idx = cg->vars->len;
	if (cg->verbose_level > 2)
		printf ("VREG %d -> LOCAL %d\n", vreg, var_idx);
	g_hash_table_insert (cg->vreg_to_wasm, GINT_TO_POINTER (vreg), GINT_TO_POINTER (var_idx + 1));
	g_ptr_array_add (cg->vars, GINT_TO_POINTER (mono_type_to_wasm_type (type)));
	return var_idx;
}

static BinaryenIndex
get_ivar (WasmCodeGen *cg, int vreg)
{
	return get_var (cg, vreg, &mono_defaults.int32_class->byval_arg);
}

static BinaryenExpressionRef
get_iloc (WasmCodeGen *cg, int vreg)
{
	return BinaryenGetLocal (aot_module, get_ivar (cg, vreg), BinaryenInt32 ());
}

static BinaryenIndex
add_wasm_var (WasmCodeGen *cg, BinaryenType type)
{
	int var_idx = cg->vars->len;
	g_ptr_array_add (cg->vars, GINT_TO_POINTER (type));
	return var_idx;
}

static BinaryenExpressionRef
emit_conv_u (WasmCodeGen *cg, MonoInst *ins, int width)
{
	int mask = (1 << width * 8) - 1;

	//conv_u1 is (v & 0xFF)
	BinaryenExpressionRef val = BinaryenBinary (
		aot_module,
		BinaryenAndInt32 (),
		get_iloc (cg, ins->sreg1),
		BinaryenConst (aot_module, BinaryenLiteralInt32 (mask)));

	return BinaryenSetLocal (aot_module, get_ivar (cg, ins->dreg), val);
}

static BinaryenExpressionRef
emit_conv_i (WasmCodeGen *cg, MonoInst *ins, int width)
{
	int shift = 32 - (width * 8);

	//conv_i1 is ((v << 24) >>> 24)
	BinaryenExpressionRef left_shift = BinaryenBinary (
		aot_module,
		BinaryenShlInt32 (),
		get_iloc (cg, ins->sreg1),
		BinaryenConst (aot_module, BinaryenLiteralInt32 (shift)));

	BinaryenExpressionRef right_shift = BinaryenBinary (
		aot_module,
		BinaryenShrSInt32 (),
		left_shift,
		BinaryenConst (aot_module, BinaryenLiteralInt32 (shift)));

	return BinaryenSetLocal (aot_module, get_ivar (cg, ins->dreg), right_shift);
}

static BinaryenExpressionRef
emit_unary_op (WasmCodeGen *cg, MonoInst *ins, BinaryenOp op)
{
	BinaryenExpressionRef val = BinaryenUnary (
		aot_module,
		op,
		get_iloc (cg, ins->sreg1));

	return BinaryenSetLocal (aot_module, get_ivar (cg, ins->dreg), val);
}

static BinaryenExpressionRef
emit_binary_op (WasmCodeGen *cg, MonoInst *ins, BinaryenOp op)
{
	BinaryenExpressionRef val = BinaryenBinary (
		aot_module,
		op,
		get_iloc (cg, ins->sreg1),
		op_is_op_imm (ins->opcode) ? BinaryenConst (aot_module, BinaryenLiteralInt32 (ins->inst_imm)) : get_iloc (cg, ins->sreg2));
	return BinaryenSetLocal (aot_module, get_ivar (cg, ins->dreg), val);
}

void
mono_wasm_code_gen (MonoCompile *cfg)
{
	if (cfg->verbose_level > 2) {
		printf ("CODE GEN %s\n", cfg->method->name);
		mono_print_code (cfg, "WASM-CODEGEN");
	}

	int i, ret_var_idx = -1;
	MonoMethod *method = cfg->method;
	MonoMethodSignature *sig = mono_method_signature (method);
	BinaryenType *params = g_new (BinaryenType, sig->param_count + sig->hasthis);

	WasmCodeGen cg;
	cg_init (&cg, cfg);
	
	if (sig->hasthis) {
		get_var (&cg, cfg->args [0]->dreg, &method->klass->this_arg);
		params [0] = mono_type_to_wasm_type (&method->klass->this_arg);
	}

	for (i = 0; i < sig->param_count; ++i) {
		get_var (&cg, cfg->args [i + sig->hasthis]->dreg, sig->params [i]);
		params [i + sig->hasthis] = mono_type_to_wasm_type (sig->params [i]);
	}

	if (sig->ret->type != MONO_TYPE_VOID) {
		ret_var_idx = get_var (&cg, cfg->ret->dreg, sig->ret);
	}
	
	MonoBasicBlock *bb;
	RelooperRef relooper = RelooperCreate ();
	RelooperBlockRef entry_block;
	
	for (bb = cfg->bb_entry; bb; bb = bb->next_bb) {
		GPtrArray *body = g_ptr_array_new ();
		MonoInst *ins;
		BinaryenExpressionRef active_branch = NULL;

		for (ins = bb->code; ins; ins = ins->next) {
			switch (ins->opcode) {
			case OP_IADD_IMM:
			case OP_IADD:
				g_ptr_array_add (body, emit_binary_op (&cg, ins, BinaryenAddInt32 ()));
				break;

			case OP_ISUB:
			case OP_ISUB_IMM:
				g_ptr_array_add (body, emit_binary_op (&cg, ins, BinaryenSubInt32 ()));
				break;

			case OP_ISHL:
			case OP_ISHL_IMM:
			case OP_SHL_IMM:
				g_ptr_array_add (body, emit_binary_op (&cg, ins, BinaryenShlInt32 ()));
				break;

			case OP_ISHR:
			case OP_ISHR_IMM:
				g_ptr_array_add (body, emit_binary_op (&cg, ins, BinaryenShrSInt32 ()));
				break;

			case OP_INEG: {
				//emit 0 - $val
				BinaryenExpressionRef val = BinaryenBinary (
					aot_module,
					BinaryenSubInt32 (),
					BinaryenConst (aot_module, BinaryenLiteralInt32 (0)),
					get_iloc (&cg, ins->sreg1));

				BinaryenExpressionRef store = BinaryenSetLocal (aot_module, get_ivar (&cg, ins->dreg), val);
				g_ptr_array_add (body, store);
				break;
			}
			case OP_IOR:
			case OP_IOR_IMM:
				g_ptr_array_add (body, emit_binary_op (&cg, ins, BinaryenOrInt32 ()));
				break;

			case OP_IAND:
			case OP_IAND_IMM:
			case OP_AND_IMM:
				g_ptr_array_add (body, emit_binary_op (&cg, ins, BinaryenAndInt32 ()));
				break;

			case OP_ISHR_UN:
			case OP_ISHR_UN_IMM:
				g_ptr_array_add (body, emit_binary_op (&cg, ins, BinaryenShrUInt32 ()));
				break;

			case OP_ICONST:
				g_ptr_array_add (body,  BinaryenSetLocal (aot_module, get_ivar (&cg, ins->dreg), BinaryenConst (aot_module, BinaryenLiteralInt32 (ins->inst_c0))));
				break;

			case OP_ICONV_TO_U1:
				g_ptr_array_add (body, emit_conv_u (&cg, ins, 1));
				break;
			case OP_ICONV_TO_U2:
				g_ptr_array_add (body, emit_conv_u (&cg, ins, 2));
				break;
			case OP_ICONV_TO_I1:
				g_ptr_array_add (body, emit_conv_i (&cg, ins, 1));
				break;
			case OP_ICONV_TO_I2:
				g_ptr_array_add (body, emit_conv_i (&cg, ins, 2));
				break;

				case OP_ICOMPARE:
				case OP_ICOMPARE_IMM: {
				BinaryenExpressionRef left = get_iloc (&cg, ins->sreg1);
				BinaryenExpressionRef right = ins->opcode == OP_ICOMPARE ? get_iloc (&cg, ins->sreg2) : BinaryenConst (aot_module, BinaryenLiteralInt32 (ins->inst_imm));
				BinaryenExpressionRef cmp = NULL;
				g_assert (ins->next);
				switch (ins->next->opcode) {
				case OP_IBEQ:
					cmp = BinaryenBinary (aot_module, BinaryenEqInt32 (), left, right);
					break;
				case OP_IBNE_UN:
					cmp = BinaryenBinary (aot_module, BinaryenNeInt32 (), left, right);
					break;
				case OP_IBLE:
					cmp = BinaryenBinary (aot_module, BinaryenLeSInt32 (), left, right);
					break;
				case OP_IBLT:
					cmp = BinaryenBinary (aot_module, BinaryenLtSInt32 (), left, right);
					break;
				case OP_IBGE:
					cmp = BinaryenBinary (aot_module, BinaryenGeSInt32 (), left, right);
					break;
				case OP_IBGT:
					cmp = BinaryenBinary (aot_module, BinaryenGtSInt32 (), left, right);
					break;
				case OP_ICEQ:
					//don't set cmp as this is a value
					g_ptr_array_add (body,  BinaryenSetLocal (aot_module, get_ivar (&cg, ins->next->dreg), BinaryenBinary (aot_module, BinaryenEqInt32 (), left, right)));
					break;
				case OP_ICGT:
					//don't set cmp as this is a value
					g_ptr_array_add (body,  BinaryenSetLocal (aot_module, get_ivar (&cg, ins->next->dreg), BinaryenBinary (aot_module, BinaryenGtSInt32 (), left, right)));
					break;
				case OP_ICGT_UN:
					//don't set cmp as this is a value
					g_ptr_array_add (body,  BinaryenSetLocal (aot_module, get_ivar (&cg, ins->next->dreg), BinaryenBinary (aot_module, BinaryenGtUInt32 (), left, right)));
					break;
				case OP_ICLT_UN:
					//don't set cmp as this is a value
					g_ptr_array_add (body,  BinaryenSetLocal (aot_module, get_ivar (&cg, ins->next->dreg), BinaryenBinary (aot_module, BinaryenLtUInt32 (), left, right)));
					break;

				default:
					printf ("wasm backend can't translate branch: ");
					mono_print_ins (ins->next);
					g_assert_not_reached ();
				}
				g_assert (!active_branch);
				active_branch = cmp;
				ins = ins->next;
				break;
			}

			case OP_BR: {
				//we ignore branches, they will be inserted by the relooper step
				break;
			}

			case OP_MOVE: {
				BinaryenExpressionRef store = BinaryenSetLocal (aot_module, get_ivar (&cg, ins->dreg), get_iloc (&cg, ins->sreg1));
				g_ptr_array_add (body, store);
				break;
			}

			default:
				printf ("wasm backend can't translate: ");
				mono_print_ins (ins);
				g_assert_not_reached ();
			}
		}
		//last BB generate 
		if (!bb->next_bb && ret_var_idx != -1)
			g_ptr_array_add (body, BinaryenReturn (aot_module, BinaryenGetLocal (aot_module, ret_var_idx, mono_type_to_wasm_type (sig->ret))));

		//FIXME should the type be the ret type in case of the leaf block?		
		BinaryenExpressionRef block = BinaryenBlock (aot_module, g_strdup_printf ("BB_%d", bb->block_num), (BinaryenExpressionRef*) body->pdata, body->len, BinaryenNone ());
		//how do we connect fallthrough blocks?
		RelooperBlockRef ref = RelooperAddBlock (relooper, block);
		bb->backend_data = ref;
		bb->backend_branch_data = active_branch;
		if (bb == cfg->bb_entry)
			entry_block = ref;
	}

	for (bb = cfg->bb_entry; bb; bb = bb->next_bb) {
		if (!bb->out_count)
			continue;
		if (bb->out_count == 1) {
			if (cfg->verbose_level > 2)
				printf ("connecting BB_%d to BB_%d\n", bb->block_num, bb->out_bb [0]->block_num);
			RelooperAddBranch ((RelooperBlockRef)bb->backend_data, (RelooperBlockRef)bb->out_bb [0]->backend_data, NULL, NULL);
		} else if (bb->out_count == 2) {
			if (cfg->verbose_level > 2)
				printf ("connecting BB_%d to BB_%d and BB_%d\n", bb->block_num, bb->out_bb [0]->block_num, bb->out_bb [1]->block_num);
			RelooperAddBranch ((RelooperBlockRef)bb->backend_data, (RelooperBlockRef)bb->out_bb [0]->backend_data, bb->backend_branch_data, NULL);
			RelooperAddBranch ((RelooperBlockRef)bb->backend_data, (RelooperBlockRef)bb->out_bb [1]->backend_data, NULL, NULL);
		} else {
			g_error ("Cannot handle branching factor of %d\n", bb->out_count);
		}
	}

	BinaryenExpressionRef func_body = RelooperRenderAndDispose (relooper, entry_block, add_wasm_var (&cg, BinaryenInt32 ()), aot_module);

	//TODO we must cannonicalize this (or use binaryen's support fo rthat)
	BinaryenFunctionTypeRef func_sig = BinaryenAddFunctionType (aot_module, NULL, mono_type_to_wasm_type (sig->ret), params, sig->param_count + sig->hasthis);
	BinaryenFunctionRef func = BinaryenAddFunction (aot_module, method->name, func_sig, (BinaryenType *)cg.vars->pdata, cg.vars->len, func_body);
	BinaryenAddExport(aot_module, method->name, method->name);
	g_ptr_array_add (all_funcs, func);

	if (cfg->verbose_level > 2) {
		printf ("RAW WASM OUTPUT:\n");
		BinaryenExpressionPrint (func_body);
	}
}

void
mono_wasm_create_module (void)
{
	printf ("mono_wasm_create_module\n");

	// BinaryenSetAPITracing (1);
	aot_module = BinaryenModuleCreate ();
	all_funcs = g_ptr_array_new ();
}

void
mono_wasm_emit_aot_data (const char *symbol, guint8 *data, int data_len)
{
	printf ("emit '%s' with %d bytes\n", symbol, data_len);
}

void
mono_wasm_emit_aot_file_info (MonoAotFileInfo *info, gboolean has_jitted_code)
{
	printf ("mono_wasm_emit_aot_file_info\n");
}

void
mono_wasm_emit_module (const char *filename)
{
	BinaryenSetFunctionTable (aot_module, (BinaryenFunctionRef*)all_funcs->pdata, all_funcs->len);

	// BinaryenModulePrint (aot_module);

	printf ("------OPTIMIZE-------\n");
	BinaryenModuleOptimize(aot_module);
	BinaryenModulePrint (aot_module);
	BinaryenModuleWriteFile (aot_module, filename);

}

#endif

