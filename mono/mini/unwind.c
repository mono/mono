/*
 * unwind.c: Stack Unwinding Interface
 *
 * Authors:
 *   Zoltan Varga (vargaz@gmail.com)
 *
 * (C) 2008 Novell, Inc.
 */

#include "mini.h"
#include "mini-unwind.h"

#include <mono/utils/mono-counters.h>
#include <mono/utils/freebsd-dwarf.h>
#include <mono/utils/hazard-pointer.h>
#include <mono/metadata/threads-types.h>
#include <mono/metadata/mono-endian.h>

typedef enum {
	LOC_SAME,
	LOC_OFFSET
} LocType;

typedef struct {
	LocType loc_type;
	int offset;
} Loc;

typedef struct {
	guint32 len;
	guint8 info [MONO_ZERO_LEN_ARRAY];
} MonoUnwindInfo;

#define ALIGN_TO(val,align) ((((size_t)val) + ((align) - 1)) & ~((align) - 1))

static mono_mutex_t unwind_mutex;

static MonoUnwindInfo **cached_info;
static int cached_info_next, cached_info_size;
static GSList *cached_info_list;
/* Statistics */
static int unwind_info_size;

#define unwind_lock() mono_mutex_lock (&unwind_mutex)
#define unwind_unlock() mono_mutex_unlock (&unwind_mutex)

#ifdef TARGET_AMD64
static int map_hw_reg_to_dwarf_reg [] = { 0, 2, 1, 3, 7, 6, 4, 5, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
#define NUM_REGS AMD64_NREG
#define DWARF_DATA_ALIGN (-8)
#define DWARF_PC_REG (mono_hw_reg_to_dwarf_reg (AMD64_RIP))
#elif defined(TARGET_ARM)
// http://infocenter.arm.com/help/topic/com.arm.doc.ihi0040a/IHI0040A_aadwarf.pdf
/* Assign d8..d15 to hregs 16..24 */
static int map_hw_reg_to_dwarf_reg [] = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 264, 265, 266, 267, 268, 269, 270, 271 };
#define NUM_REGS 272
#define DWARF_DATA_ALIGN (-4)
#define DWARF_PC_REG (mono_hw_reg_to_dwarf_reg (ARMREG_LR))
#elif defined(TARGET_ARM64)
#define NUM_REGS 96
#define DWARF_DATA_ALIGN (-8)
/* LR */
#define DWARF_PC_REG 30
static int map_hw_reg_to_dwarf_reg [] = {
	0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15,
	16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31,
	/* v8..v15 */
	72, 73, 74, 75, 76, 77, 78, 79,
};
#elif defined (TARGET_X86)
#ifdef __APPLE__
/*
 * LLVM seems to generate unwind info where esp is encoded as 5, and ebp as 4, ie see this line:
 *   def ESP : RegisterWithSubRegs<"esp", [SP]>, DwarfRegNum<[-2, 5, 4]>;
 * in lib/Target/X86/X86RegisterInfo.td in the llvm sources.
 */
static int map_hw_reg_to_dwarf_reg [] = { 0, 1, 2, 3, 5, 4, 6, 7, 8 };
#else
static int map_hw_reg_to_dwarf_reg [] = { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
#endif
/* + 1 is for IP */
#define NUM_REGS X86_NREG + 1
#define DWARF_DATA_ALIGN (-4)
#define DWARF_PC_REG (mono_hw_reg_to_dwarf_reg (X86_NREG))
#elif defined (TARGET_POWERPC)
// http://refspecs.linuxfoundation.org/ELF/ppc64/PPC-elf64abi-1.9.html
static int map_hw_reg_to_dwarf_reg [] = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 
										  9, 10, 11, 12, 13, 14, 15, 16,
										  17, 18, 19, 20, 21, 22, 23, 24,
										  25, 26, 27, 28, 29, 30, 31 };
#define NUM_REGS 110
#define DWARF_DATA_ALIGN (-(gint32)sizeof (mgreg_t))
#define DWARF_PC_REG 108
#elif defined (TARGET_S390X)
static int map_hw_reg_to_dwarf_reg [] = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
#define NUM_REGS 16
#define DWARF_DATA_ALIGN (-8)
#define DWARF_PC_REG (mono_hw_reg_to_dwarf_reg (14))
#elif defined (TARGET_MIPS)
/* FIXME: */
static int map_hw_reg_to_dwarf_reg [32] = {
	0, 1, 2, 3, 4, 5, 6, 7,
	8, 9, 10, 11, 12, 13, 14, 15,
	16, 17, 18, 19, 20, 21, 22, 23,
	24, 25, 26, 27, 28, 29, 30, 31
};
#define NUM_REGS 32
#define DWARF_DATA_ALIGN (-(gint32)sizeof (mgreg_t))
#define DWARF_PC_REG (mono_hw_reg_to_dwarf_reg (mips_ra))
#else
static int map_hw_reg_to_dwarf_reg [16];
#define NUM_REGS 16
#define DWARF_DATA_ALIGN 0
#define DWARF_PC_REG -1
#endif

static gboolean dwarf_reg_to_hw_reg_inited;

static int map_dwarf_reg_to_hw_reg [NUM_REGS];

/*
 * mono_hw_reg_to_dwarf_reg:
 *
 *   Map the hardware register number REG to the register number used by DWARF.
 */
int
mono_hw_reg_to_dwarf_reg (int reg)
{
#ifdef TARGET_POWERPC
	if (reg == ppc_lr)
		return 108;
	else
		g_assert (reg < NUM_REGS);
#endif

	if (NUM_REGS == 0) {
		g_assert_not_reached ();
		return -1;
	} else {
		return map_hw_reg_to_dwarf_reg [reg];
	}
}

static void
init_reg_map (void)
{
	int i;

	g_assert (NUM_REGS > 0);
	for (i = 0; i < sizeof (map_hw_reg_to_dwarf_reg) / sizeof (int); ++i) {
		map_dwarf_reg_to_hw_reg [mono_hw_reg_to_dwarf_reg (i)] = i;
	}

#ifdef TARGET_POWERPC
	map_dwarf_reg_to_hw_reg [DWARF_PC_REG] = ppc_lr;
#endif

	mono_memory_barrier ();
	dwarf_reg_to_hw_reg_inited = TRUE;
}

int
mono_dwarf_reg_to_hw_reg (int reg)
{
	if (!dwarf_reg_to_hw_reg_inited)
		init_reg_map ();

	return map_dwarf_reg_to_hw_reg [reg];
}

static G_GNUC_UNUSED void
encode_uleb128 (guint32 value, guint8 *buf, guint8 **endbuf)
{
	guint8 *p = buf;

	do {
		guint8 b = value & 0x7f;
		value >>= 7;
		if (value != 0) /* more bytes to come */
			b |= 0x80;
		*p ++ = b;
	} while (value);

	*endbuf = p;
}

static G_GNUC_UNUSED void
encode_sleb128 (gint32 value, guint8 *buf, guint8 **endbuf)
{
	gboolean more = 1;
	gboolean negative = (value < 0);
	guint32 size = 32;
	guint8 byte;
	guint8 *p = buf;

	while (more) {
		byte = value & 0x7f;
		value >>= 7;
		/* the following is unnecessary if the
		 * implementation of >>= uses an arithmetic rather
		 * than logical shift for a signed left operand
		 */
		if (negative)
			/* sign extend */
			value |= - (1 <<(size - 7));
		/* sign bit of byte is second high order bit (0x40) */
		if ((value == 0 && !(byte & 0x40)) ||
			(value == -1 && (byte & 0x40)))
			more = 0;
		else
			byte |= 0x80;
		*p ++= byte;
	}

	*endbuf = p;
}

static inline guint32
decode_uleb128 (guint8 *buf, guint8 **endbuf)
{
	guint8 *p = buf;
	guint32 res = 0;
	int shift = 0;

	while (TRUE) {
		guint8 b = *p;
		p ++;

		res = res | (((int)(b & 0x7f)) << shift);
		if (!(b & 0x80))
			break;
		shift += 7;
	}

	*endbuf = p;

	return res;
}

static inline gint32
decode_sleb128 (guint8 *buf, guint8 **endbuf)
{
	guint8 *p = buf;
	gint32 res = 0;
	int shift = 0;

	while (TRUE) {
		guint8 b = *p;
		p ++;

		res = res | (((int)(b & 0x7f)) << shift);
		shift += 7;
		if (!(b & 0x80)) {
			if (shift < 32 && (b & 0x40))
				res |= - (1 << shift);
			break;
		}
	}

	*endbuf = p;

	return res;
}

void
mono_print_unwind_info (guint8 *unwind_info, int unwind_info_len)
{
	guint8 *p;
	int pos, reg, offset, cfa_reg, cfa_offset;

	p = unwind_info;
	pos = 0;
	while (p < unwind_info + unwind_info_len) {
		int op = *p & 0xc0;

		switch (op) {
		case DW_CFA_advance_loc:
			pos += *p & 0x3f;
			p ++;
			break;
		case DW_CFA_offset:
			reg = *p & 0x3f;
			p ++;
			offset = decode_uleb128 (p, &p) * DWARF_DATA_ALIGN;
			if (reg == DWARF_PC_REG)
				printf ("CFA: [%x] offset: %s at cfa-0x%x\n", pos, "pc", -offset);
			else
				printf ("CFA: [%x] offset: %s at cfa-0x%x\n", pos, mono_arch_regname (mono_dwarf_reg_to_hw_reg (reg)), -offset);
			break;
		case 0: {
			int ext_op = *p;
			p ++;
			switch (ext_op) {
			case DW_CFA_def_cfa:
				cfa_reg = decode_uleb128 (p, &p);
				cfa_offset = decode_uleb128 (p, &p);
				printf ("CFA: [%x] def_cfa: %s+0x%x\n", pos, mono_arch_regname (mono_dwarf_reg_to_hw_reg (cfa_reg)), cfa_offset);
				break;
			case DW_CFA_def_cfa_offset:
				cfa_offset = decode_uleb128 (p, &p);
				printf ("CFA: [%x] def_cfa_offset: 0x%x\n", pos, cfa_offset);
				break;
			case DW_CFA_def_cfa_register:
				cfa_reg = decode_uleb128 (p, &p);
				printf ("CFA: [%x] def_cfa_reg: %s\n", pos, mono_arch_regname (mono_dwarf_reg_to_hw_reg (cfa_reg)));
				break;
			case DW_CFA_offset_extended_sf:
				reg = decode_uleb128 (p, &p);
				offset = decode_sleb128 (p, &p) * DWARF_DATA_ALIGN;
				printf ("CFA: [%x] offset_extended_sf: %s at cfa-0x%x\n", pos, mono_arch_regname (mono_dwarf_reg_to_hw_reg (reg)), -offset);
				break;
			case DW_CFA_same_value:
				reg = decode_uleb128 (p, &p);
				printf ("CFA: [%x] same_value: %s\n", pos, mono_arch_regname (mono_dwarf_reg_to_hw_reg (reg)));
				break;
			case DW_CFA_advance_loc4:
				pos += read32 (p);
				p += 4;
				break;
			case DW_CFA_remember_state:
				printf ("CFA: [%x] remember_state\n", pos);
				break;
			case DW_CFA_restore_state:
				printf ("CFA: [%x] restore_state\n", pos);
				break;
			case DW_CFA_mono_advance_loc:
				printf ("CFA: [%x] mono_advance_loc\n", pos);
				break;
			default:
				g_assert_not_reached ();
			}
			break;
		}
		default:
			g_assert_not_reached ();
		}
	}
}

/*
 * mono_unwind_ops_encode:
 *
 *   Encode the unwind ops in UNWIND_OPS into the compact DWARF encoding.
 * Return a pointer to malloc'ed memory.
 */
guint8*
mono_unwind_ops_encode (GSList *unwind_ops, guint32 *out_len)
{
	GSList *l;
	MonoUnwindOp *op;
	int loc;
	guint8 *buf, *p, *res;

	p = buf = g_malloc0 (4096);

	loc = 0;
	l = unwind_ops;
	for (; l; l = l->next) {
		int reg;

		op = l->data;

		/* Convert the register from the hw encoding to the dwarf encoding */
		reg = mono_hw_reg_to_dwarf_reg (op->reg);

		if (op->op == DW_CFA_mono_advance_loc) {
			/* This advances loc to its location */
			loc = op->when;
		}

		/* Emit an advance_loc if neccesary */
		while (op->when > loc) {
			if (op->when - loc > 65536) {
				*p ++ = DW_CFA_advance_loc4;
				*(guint32*)p = (guint32)(op->when - loc);
				g_assert (read32 (p) == (guint32)(op->when - loc));
				p += 4;
				loc = op->when;
			} else if (op->when - loc > 256) {
				*p ++ = DW_CFA_advance_loc2;
				*(guint16*)p = (guint16)(op->when - loc);
				g_assert (read16 (p) == (guint32)(op->when - loc));
				p += 2;
				loc = op->when;
			} else if (op->when - loc >= 32) {
				*p ++ = DW_CFA_advance_loc1;
				*(guint8*)p = (guint8)(op->when - loc);
				p += 1;
				loc = op->when;
			} else if (op->when - loc < 32) {
				*p ++ = DW_CFA_advance_loc | (op->when - loc);
				loc = op->when;
			} else {
				*p ++ = DW_CFA_advance_loc | (30);
				loc += 30;
			}
		}			

		switch (op->op) {
		case DW_CFA_def_cfa:
			*p ++ = op->op;
			encode_uleb128 (reg, p, &p);
			encode_uleb128 (op->val, p, &p);
			break;
		case DW_CFA_def_cfa_offset:
			*p ++ = op->op;
			encode_uleb128 (op->val, p, &p);
			break;
		case DW_CFA_def_cfa_register:
			*p ++ = op->op;
			encode_uleb128 (reg, p, &p);
			break;
		case DW_CFA_same_value:
			*p ++ = op->op;
			encode_uleb128 (reg, p, &p);
			break;
		case DW_CFA_offset:
			if (reg > 63) {
				*p ++ = DW_CFA_offset_extended_sf;
				encode_uleb128 (reg, p, &p);
				encode_sleb128 (op->val / DWARF_DATA_ALIGN, p, &p);
			} else {
				*p ++ = DW_CFA_offset | reg;
				encode_uleb128 (op->val / DWARF_DATA_ALIGN, p, &p);
			}
			break;
		case DW_CFA_remember_state:
		case DW_CFA_restore_state:
			*p ++ = op->op;
			break;
		case DW_CFA_mono_advance_loc:
			/* Only one location is supported */
			g_assert (op->val == 0);
			*p ++ = op->op;
			break;
		default:
			g_assert_not_reached ();
			break;
		}
	}
	
	g_assert (p - buf < 4096);
	*out_len = p - buf;
	res = g_malloc (p - buf);
	memcpy (res, buf, p - buf);
	g_free (buf);
	return res;
}

#if 0
#define UNW_DEBUG(stmt) do { stmt; } while (0)
#else
#define UNW_DEBUG(stmt) do { } while (0)
#endif

static G_GNUC_UNUSED void
print_dwarf_state (int cfa_reg, int cfa_offset, int ip, int nregs, Loc *locations, guint8 *reg_saved)
{
	int i;

	printf ("\t%x: cfa=r%d+%d ", ip, cfa_reg, cfa_offset);

	for (i = 0; i < nregs; ++i)
		if (reg_saved [i] && locations [i].loc_type == LOC_OFFSET)
			printf ("r%d@%d(cfa) ", i, locations [i].offset);
	printf ("\n");
}

typedef struct {
	Loc locations [NUM_REGS];
	guint8 reg_saved [NUM_REGS];
	int cfa_reg, cfa_offset;
} UnwindState;

/*
 * Given the state of the current frame as stored in REGS, execute the unwind 
 * operations in unwind_info until the location counter reaches POS. The result is 
 * stored back into REGS. OUT_CFA will receive the value of the CFA.
 * If SAVE_LOCATIONS is non-NULL, it should point to an array of size SAVE_LOCATIONS_LEN.
 * On return, the nth entry will point to the address of the stack slot where register
 * N was saved, or NULL, if it was not saved by this frame.
 * MARK_LOCATIONS should contain the locations marked by mono_emit_unwind_op_mark_loc (), if any.
 * This function is signal safe.
 */
void
mono_unwind_frame (guint8 *unwind_info, guint32 unwind_info_len, 
				   guint8 *start_ip, guint8 *end_ip, guint8 *ip, guint8 **mark_locations,
				   mgreg_t *regs, int nregs,
				   mgreg_t **save_locations, int save_locations_len,
				   guint8 **out_cfa)
{
	Loc locations [NUM_REGS];
	guint8 reg_saved [NUM_REGS];
	int i, pos, reg, cfa_reg, cfa_offset, offset;
	guint8 *p;
	guint8 *cfa_val;
	UnwindState state_stack [1];
	int state_stack_pos;

	memset (reg_saved, 0, sizeof (reg_saved));

	p = unwind_info;
	pos = 0;
	cfa_reg = -1;
	cfa_offset = -1;
	state_stack_pos = 0;
	while (pos <= ip - start_ip && p < unwind_info + unwind_info_len) {
		int op = *p & 0xc0;

		switch (op) {
		case DW_CFA_advance_loc:
			UNW_DEBUG (print_dwarf_state (cfa_reg, cfa_offset, pos, nregs, locations));
			pos += *p & 0x3f;
			p ++;
			break;
		case DW_CFA_offset:
			reg = *p & 0x3f;
			p ++;
			reg_saved [reg] = TRUE;
			locations [reg].loc_type = LOC_OFFSET;
			locations [reg].offset = decode_uleb128 (p, &p) * DWARF_DATA_ALIGN;
			break;
		case 0: {
			int ext_op = *p;
			p ++;
			switch (ext_op) {
			case DW_CFA_def_cfa:
				cfa_reg = decode_uleb128 (p, &p);
				cfa_offset = decode_uleb128 (p, &p);
				break;
			case DW_CFA_def_cfa_offset:
				cfa_offset = decode_uleb128 (p, &p);
				break;
			case DW_CFA_def_cfa_register:
				cfa_reg = decode_uleb128 (p, &p);
				break;
			case DW_CFA_offset_extended_sf:
				reg = decode_uleb128 (p, &p);
				offset = decode_sleb128 (p, &p);
				g_assert (reg < NUM_REGS);
				reg_saved [reg] = TRUE;
				locations [reg].loc_type = LOC_OFFSET;
				locations [reg].offset = offset * DWARF_DATA_ALIGN;
				break;
			case DW_CFA_offset_extended:
				reg = decode_uleb128 (p, &p);
				offset = decode_uleb128 (p, &p);
				g_assert (reg < NUM_REGS);
				reg_saved [reg] = TRUE;
				locations [reg].loc_type = LOC_OFFSET;
				locations [reg].offset = offset * DWARF_DATA_ALIGN;
				break;
			case DW_CFA_same_value:
				reg = decode_uleb128 (p, &p);
				locations [reg].loc_type = LOC_SAME;
				break;
			case DW_CFA_advance_loc1:
				pos += *p;
				p += 1;
				break;
			case DW_CFA_advance_loc2:
				pos += read16 (p);
				p += 2;
				break;
			case DW_CFA_advance_loc4:
				pos += read32 (p);
				p += 4;
				break;
			case DW_CFA_remember_state:
				g_assert (state_stack_pos == 0);
				memcpy (&state_stack [0].locations, &locations, sizeof (locations));
				memcpy (&state_stack [0].reg_saved, &reg_saved, sizeof (reg_saved));
				state_stack [0].cfa_reg = cfa_reg;
				state_stack [0].cfa_offset = cfa_offset;
				state_stack_pos ++;
				break;
			case DW_CFA_restore_state:
				g_assert (state_stack_pos == 1);
				state_stack_pos --;
				memcpy (&locations, &state_stack [0].locations, sizeof (locations));
				memcpy (&reg_saved, &state_stack [0].reg_saved, sizeof (reg_saved));
				cfa_reg = state_stack [0].cfa_reg;
				cfa_offset = state_stack [0].cfa_offset;
				break;
			case DW_CFA_mono_advance_loc:
				g_assert (mark_locations [0]);
				pos = mark_locations [0] - start_ip;
				break;
			default:
				g_assert_not_reached ();
			}
			break;
		}
		default:
			g_assert_not_reached ();
		}
	}

	if (save_locations)
		memset (save_locations, 0, save_locations_len * sizeof (mgreg_t*));

	cfa_val = (guint8*)regs [mono_dwarf_reg_to_hw_reg (cfa_reg)] + cfa_offset;
	for (i = 0; i < NUM_REGS; ++i) {
		if (reg_saved [i] && locations [i].loc_type == LOC_OFFSET) {
			int hreg = mono_dwarf_reg_to_hw_reg (i);
			g_assert (hreg < nregs);
			regs [hreg] = *(mgreg_t*)(cfa_val + locations [i].offset);
			if (save_locations && hreg < save_locations_len)
				save_locations [hreg] = (mgreg_t*)(cfa_val + locations [i].offset);
		}
	}

	*out_cfa = cfa_val;
}

void
mono_unwind_init (void)
{
	mono_mutex_init_recursive (&unwind_mutex);

	mono_counters_register ("Unwind info size", MONO_COUNTER_JIT | MONO_COUNTER_INT, &unwind_info_size);
}

void
mono_unwind_cleanup (void)
{
	int i;

	mono_mutex_destroy (&unwind_mutex);

	if (!cached_info)
		return;

	for (i = 0; i < cached_info_next; ++i) {
		MonoUnwindInfo *cached = cached_info [i];

		g_free (cached);
	}

	g_free (cached_info);
}

/*
 * mono_cache_unwind_info
 *
 *   Save UNWIND_INFO in the unwind info cache and return an id which can be passed
 * to mono_get_cached_unwind_info to get a cached copy of the info.
 * A copy is made of the unwind info.
 * This function is useful for two reasons:
 * - many methods have the same unwind info
 * - MonoJitInfo->unwind_info is an int so it can't store the pointer to the unwind info
 */
guint32
mono_cache_unwind_info (guint8 *unwind_info, guint32 unwind_info_len)
{
	int i;
	MonoUnwindInfo *info;

	unwind_lock ();

	if (cached_info == NULL) {
		cached_info_size = 16;
		cached_info = g_new0 (MonoUnwindInfo*, cached_info_size);
	}

	for (i = 0; i < cached_info_next; ++i) {
		MonoUnwindInfo *cached = cached_info [i];

		if (cached->len == unwind_info_len && memcmp (cached->info, unwind_info, unwind_info_len) == 0) {
			unwind_unlock ();
			return i;
		}
	}

	info = g_malloc (sizeof (MonoUnwindInfo) + unwind_info_len);
	info->len = unwind_info_len;
	memcpy (&info->info, unwind_info, unwind_info_len);

	i = cached_info_next;
	
	if (cached_info_next >= cached_info_size) {
		MonoUnwindInfo **old_table, **new_table;

		/*
		 * Avoid freeing the old table so mono_get_cached_unwind_info ()
		 * doesn't need locks/hazard pointers.
		 */

		old_table = cached_info;
		new_table = g_new0 (MonoUnwindInfo*, cached_info_size * 2);

		memcpy (new_table, cached_info, cached_info_size * sizeof (MonoUnwindInfo*));

		mono_memory_barrier ();

		cached_info = new_table;

		cached_info_list = g_slist_prepend (cached_info_list, cached_info);

		cached_info_size *= 2;
	}

	cached_info [cached_info_next ++] = info;

	unwind_info_size += sizeof (MonoUnwindInfo) + unwind_info_len;

	unwind_unlock ();
	return i;
}

/*
 * This function is signal safe.
 */
guint8*
mono_get_cached_unwind_info (guint32 index, guint32 *unwind_info_len)
{
	MonoUnwindInfo **table;
	MonoUnwindInfo *info;
	guint8 *data;

	/*
	 * This doesn't need any locks/hazard pointers,
	 * since new tables are copies of the old ones.
	 */
	table = cached_info;

	info = table [index];

	*unwind_info_len = info->len;
	data = info->info;

	return data;
}

/*
 * mono_unwind_get_dwarf_data_align:
 *
 *   Return the data alignment used by the encoded unwind information.
 */
int
mono_unwind_get_dwarf_data_align (void)
{
	return DWARF_DATA_ALIGN;
}

/*
 * mono_unwind_get_dwarf_pc_reg:
 *
 *   Return the dwarf register number of the register holding the ip of the
 * previous frame.
 */
int
mono_unwind_get_dwarf_pc_reg (void)
{
	return DWARF_PC_REG;
}

static void
decode_cie_op (guint8 *p, guint8 **endp)
{
	int op = *p & 0xc0;

	switch (op) {
	case DW_CFA_advance_loc:
		p ++;
		break;
	case DW_CFA_offset:
		p ++;
		decode_uleb128 (p, &p);
		break;
	case 0: {
		int ext_op = *p;
		p ++;
		switch (ext_op) {
		case DW_CFA_def_cfa:
			decode_uleb128 (p, &p);
			decode_uleb128 (p, &p);
			break;
		case DW_CFA_def_cfa_offset:
			decode_uleb128 (p, &p);
			break;
		case DW_CFA_def_cfa_register:
			decode_uleb128 (p, &p);
			break;
		case DW_CFA_advance_loc4:
			p += 4;
			break;
		case DW_CFA_offset_extended_sf:
			decode_uleb128 (p, &p);
			decode_uleb128 (p, &p);
			break;
		default:
			g_assert_not_reached ();
		}
		break;
	}
	default:
		g_assert_not_reached ();
	}

	*endp = p;
}

static gint64
read_encoded_val (guint32 encoding, guint8 *p, guint8 **endp)
{
	gint64 res;

	switch (encoding & 0xf) {
	case DW_EH_PE_sdata8:
		res = *(gint64*)p;
		p += 8;
		break;
	case DW_EH_PE_sdata4:
		res = *(gint32*)p;
		p += 4;
		break;
	default:
		g_assert_not_reached ();
	}

	*endp = p;
	return res;
}

/*
 * decode_lsda:
 *
 *   Decode the Mono specific Language Specific Data Area generated by LLVM.
 */
static void
decode_lsda (guint8 *lsda, guint8 *code, MonoJitExceptionInfo **ex_info, guint32 *ex_info_len, gpointer **type_info, int *this_reg, int *this_offset)
{
	guint8 *p;
	int i, ncall_sites, this_encoding;
	guint32 mono_magic, version;

	p = lsda;

	/* This is the modified LSDA generated by the LLVM mono branch */
	mono_magic = decode_uleb128 (p, &p);
	g_assert (mono_magic == 0x4d4fef4f);
	version = decode_uleb128 (p, &p);
	g_assert (version == 1);
	this_encoding = *p;
	p ++;
	if (this_encoding == DW_EH_PE_udata4) {
		gint32 op, reg, offset;

		/* 'this' location */
		op = *p;
		g_assert (op == DW_OP_bregx);
		p ++;
		reg = decode_uleb128 (p, &p);
		offset = decode_sleb128 (p, &p);

		*this_reg = mono_dwarf_reg_to_hw_reg (reg);
		*this_offset = offset;
	} else {
		g_assert (this_encoding == DW_EH_PE_omit);

		*this_reg = -1;
		*this_offset = -1;
	}
	ncall_sites = decode_uleb128 (p, &p);
	p = (guint8*)ALIGN_TO ((mgreg_t)p, 4);

	if (ex_info) {
		*ex_info = g_malloc0 (ncall_sites * sizeof (MonoJitExceptionInfo));
		*ex_info_len = ncall_sites;
	}
	if (type_info)
		*type_info = g_malloc0 (ncall_sites * sizeof (gpointer));

	for (i = 0; i < ncall_sites; ++i) {
		int block_start_offset, block_size, landing_pad;
		guint8 *tinfo;

		block_start_offset = read32 (p);
		p += sizeof (gint32);
		block_size = read32 (p);
		p += sizeof (gint32);
		landing_pad = read32 (p);
		p += sizeof (gint32);
		tinfo = p;
		p += sizeof (gint32);

		g_assert (landing_pad);
		g_assert (((size_t)tinfo % 4) == 0);
		//printf ("X: %p %d\n", landing_pad, *(int*)tinfo);

		if (ex_info) {
			if (*type_info)
				(*type_info) [i] = tinfo;
			(*ex_info)[i].try_start = code + block_start_offset;
			(*ex_info)[i].try_end = code + block_start_offset + block_size;
			(*ex_info)[i].handler_start = code + landing_pad;
		}
	}
}

/*
 * mono_unwind_decode_fde:
 *
 *   Decode a DWARF FDE entry, returning the unwind opcodes.
 * If not NULL, EX_INFO is set to a malloc-ed array of MonoJitExceptionInfo structures,
 * only try_start, try_end and handler_start is set.
 * If not NULL, TYPE_INFO is set to a malloc-ed array containing the ttype table from the
 * LSDA.
 */
guint8*
mono_unwind_decode_fde (guint8 *fde, guint32 *out_len, guint32 *code_len, MonoJitExceptionInfo **ex_info, guint32 *ex_info_len, gpointer **type_info, int *this_reg, int *this_offset)
{
	guint8 *p, *cie, *fde_current, *fde_aug = NULL, *code, *fde_cfi, *cie_cfi;
	gint32 fde_len, cie_offset, pc_begin, pc_range, aug_len, fde_data_len;
	gint32 cie_len, cie_id, cie_version, code_align, data_align, return_reg;
	gint32 i, cie_aug_len, buf_len;
	char *cie_aug_str;
	guint8 *buf;
	gboolean has_fde_augmentation = FALSE;

	/* 
	 * http://refspecs.freestandards.org/LSB_3.0.0/LSB-Core-generic/LSB-Core-generic/ehframechpt.html
	 */

	*type_info = NULL;
	*this_reg = -1;
	*this_offset = -1;

	/* Decode FDE */

	p = fde;
	// FIXME: Endianess ?
	fde_len = *(guint32*)p;
	g_assert (fde_len != 0xffffffff && fde_len != 0);
	p += 4;
	cie_offset = *(guint32*)p;
	cie = p - cie_offset;
	p += 4;
	fde_current = p;

	/* Decode CIE */
	p = cie;
	cie_len = *(guint32*)p;
	p += 4;
	cie_id = *(guint32*)p;
	g_assert (cie_id == 0);
	p += 4;
	cie_version = *p;
	g_assert (cie_version == 1);
	p += 1;
	cie_aug_str = (char*)p;
	p += strlen (cie_aug_str) + 1;
	code_align = decode_uleb128 (p, &p);
	data_align = decode_sleb128 (p, &p);
	return_reg = decode_uleb128 (p, &p);
	if (strstr (cie_aug_str, "z")) {
		guint8 *cie_aug;
		guint32 p_encoding;

		cie_aug_len = decode_uleb128 (p, &p);

		has_fde_augmentation = TRUE;

		cie_aug = p;
		for (i = 0; cie_aug_str [i] != '\0'; ++i) {
			switch (cie_aug_str [i]) {
			case 'z':
				break;
			case 'P':
				p_encoding = *p;
				p ++;
				read_encoded_val (p_encoding, p, &p);
				break;
			case 'L':
				g_assert ((*p == (DW_EH_PE_sdata4|DW_EH_PE_pcrel)) || (*p == (DW_EH_PE_sdata8|DW_EH_PE_pcrel)));
				p ++;
				break;
			case 'R':
				g_assert (*p == (DW_EH_PE_sdata4|DW_EH_PE_pcrel));
				p ++;
				break;
			default:
				g_assert_not_reached ();
				break;
			}
		}
			
		p = cie_aug;
		p += cie_aug_len;
	}
	cie_cfi = p;

	/* Continue decoding FDE */
	p = fde_current;
	/* DW_EH_PE_sdata4|DW_EH_PE_pcrel encoding */
	pc_begin = *(gint32*)p;
	code = p + pc_begin;
	p += 4;
	pc_range = *(guint32*)p;
	p += 4;
	if (has_fde_augmentation) {
		aug_len = decode_uleb128 (p, &p);
		fde_aug = p;
		p += aug_len;
	} else {
		aug_len = 0;
	}
	fde_cfi = p;
	fde_data_len = fde + 4 + fde_len - p;

	if (code_len)
		*code_len = pc_range;

	if (ex_info) {
		*ex_info = NULL;
		*ex_info_len = 0;
	}

	/* Decode FDE augmention */
	if (aug_len) {
		gint32 lsda_offset;
		guint8 *lsda;

		/* sdata|pcrel encoding */
		if (aug_len == 4)
			lsda_offset = read32 (fde_aug);
		else if (aug_len == 8)
			lsda_offset = *(gint64*)fde_aug;
		else
			g_assert_not_reached ();
		if (lsda_offset != 0) {
			lsda = fde_aug + lsda_offset;

			decode_lsda (lsda, code, ex_info, ex_info_len, type_info, this_reg, this_offset);
		}
	}

	/* Make sure the FDE uses the same constants as we do */
	g_assert (code_align == 1);
	g_assert (data_align == DWARF_DATA_ALIGN);
	g_assert (return_reg == DWARF_PC_REG);

	buf_len = (cie + cie_len + 4 - cie_cfi) + (fde + fde_len + 4 - fde_cfi);
	buf = g_malloc0 (buf_len);

	i = 0;
	p = cie_cfi;
	while (p < cie + cie_len + 4) {
		if (*p == DW_CFA_nop)
			break;
		else
			decode_cie_op (p, &p);
	}
	memcpy (buf + i, cie_cfi, p - cie_cfi);
	i += p - cie_cfi;

	p = fde_cfi;
	while (p < fde + fde_len + 4) {
		if (*p == DW_CFA_nop)
			break;
		else
			decode_cie_op (p, &p);
	}
	memcpy (buf + i, fde_cfi, p - fde_cfi);
	i += p - fde_cfi;
	g_assert (i <= buf_len);

	*out_len = i;

	return g_realloc (buf, i);
}

/*
 * mono_unwind_decode_mono_fde:
 *
 *   Decode an FDE entry in the LLVM emitted mono EH frame.
 * info->ex_info is set to a malloc-ed array of MonoJitExceptionInfo structures,
 * only try_start, try_end and handler_start is set.
 * info->type_info is set to a malloc-ed array containing the ttype table from the
 * LSDA.
 */
void
mono_unwind_decode_llvm_mono_fde (guint8 *fde, int fde_len, guint8 *cie, guint8 *code, MonoLLVMFDEInfo *res)
{
	guint8 *p, *fde_aug, *cie_cfi, *fde_cfi, *buf;
	int has_aug, aug_len, cie_cfi_len, fde_cfi_len;
	gint32 code_align, data_align, return_reg, pers_encoding;

	memset (res, 0, sizeof (*res));
	res->this_reg = -1;
	res->this_offset = -1;

	/* fde points to data emitted by LLVM in DwarfException::EmitMonoEHFrame () */
	p = fde;
	has_aug = *p;
	p ++;
	if (has_aug) {
		aug_len = read32 (p);
		p += 4;
	} else {
		aug_len = 0;
	}
	fde_aug = p;
	p += aug_len;
	fde_cfi = p;

	if (has_aug) {
		guint8 *lsda;

		/* The LSDA is embedded directly into the FDE */
		lsda = fde_aug;

		decode_lsda (lsda, code, &res->ex_info, &res->ex_info_len, &res->type_info, &res->this_reg, &res->this_offset);
	}

	/* Decode CIE */
	p = cie;
	code_align = decode_uleb128 (p, &p);
	data_align = decode_sleb128 (p, &p);
	return_reg = decode_uleb128 (p, &p);
	pers_encoding = *p;
	p ++;
	if (pers_encoding != DW_EH_PE_omit)
		read_encoded_val (pers_encoding, p, &p);

	cie_cfi = p;

	/* Make sure the FDE uses the same constants as we do */
	g_assert (code_align == 1);
	g_assert (data_align == DWARF_DATA_ALIGN);
	g_assert (return_reg == DWARF_PC_REG);

	/* Compute size of CIE unwind info it is DW_CFA_nop terminated */
	p = cie_cfi;
	while (TRUE) {
		if (*p == DW_CFA_nop)
			break;
		else
			decode_cie_op (p, &p);
	}
	cie_cfi_len = p - cie_cfi;
	fde_cfi_len = (fde + fde_len - fde_cfi);

	buf = g_malloc0 (cie_cfi_len + fde_cfi_len);
	memcpy (buf, cie_cfi, cie_cfi_len);
	memcpy (buf + cie_cfi_len, fde_cfi, fde_cfi_len);

	res->unw_info_len = cie_cfi_len + fde_cfi_len;
	res->unw_info = buf;
}

/*
 * mono_unwind_get_cie_program:
 *
 *   Get the unwind bytecode for the DWARF CIE.
 */
GSList*
mono_unwind_get_cie_program (void)
{
#if defined(TARGET_AMD64) || defined(TARGET_X86) || defined(TARGET_POWERPC)
	return mono_arch_get_cie_program ();
#else
	return NULL;
#endif
}
