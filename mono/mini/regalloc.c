/*
 * regalloc.c: register state class
 *
 * Authors:
 *    Paolo Molaro (lupus@ximian.com)
 *
 * (C) 2003 Ximian, Inc.
 */
#include "mini.h"

MonoRegState*
mono_regstate_new (void)
{
	MonoRegState* rs = g_new0 (MonoRegState, 1);

	mono_regstate_reset (rs);

	return rs;
}

void
mono_regstate_free (MonoRegState *rs) {
	g_free (rs->vassign);
	g_free (rs);
}

void
mono_regstate_reset (MonoRegState *rs) {
	rs->next_vreg = MAX (MONO_MAX_IREGS, MONO_MAX_FREGS);
}

void
mono_regstate_assign (MonoRegState *rs) {
	int i;
	rs->max_vreg = -1;

	if (rs->next_vreg != rs->vassign_size) {
		g_free (rs->vassign);
		rs->vassign = g_malloc (MAX (MONO_MAX_IREGS, rs->next_vreg) * sizeof (int));
		rs->vassign_size = rs->next_vreg;
	}

	for (i = 0; i < MAX (MONO_MAX_IREGS, MONO_MAX_FREGS); ++i)
		rs->vassign [i] = i;
	for (i = 0; i < MONO_MAX_IREGS; ++i)
		rs->isymbolic [i] = 0;
	for (i = 0; i < MONO_MAX_FREGS; ++i)
		rs->fsymbolic [i] = 0;

	/* vassign can be very large so it needs to be initialized by the caller */
}

int
mono_regstate_alloc_int (MonoRegState *rs, regmask_t allow)
{
	int i;
	regmask_t mask = allow & rs->ifree_mask;
	for (i = 0; i < MONO_MAX_IREGS; ++i) {
		if (mask & ((regmask_t)1 << i)) {
			rs->ifree_mask &= ~ ((regmask_t)1 << i);
			rs->max_vreg = MAX (rs->max_vreg, i);
			return i;
		}
	}
	return -1;
}

void
mono_regstate_free_int (MonoRegState *rs, int reg)
{
	if (reg >= 0) {
		rs->ifree_mask |= (regmask_t)1 << reg;
		rs->isymbolic [reg] = 0;
	}
}

int
mono_regstate_alloc_float (MonoRegState *rs, regmask_t allow)
{
	int i;
	regmask_t mask = allow & rs->ffree_mask;
	for (i = 0; i < MONO_MAX_FREGS; ++i) {
		if (mask & ((regmask_t)1 << i)) {
			rs->ffree_mask &= ~ ((regmask_t)1 << i);
			return i;
		}
	}
	return -1;
}

void
mono_regstate_free_float (MonoRegState *rs, int reg)
{
	if (reg >= 0) {
		rs->ffree_mask |= (regmask_t)1 << reg;
		rs->fsymbolic [reg] = 0;
	}
}

inline int
mono_regstate_next_long (MonoRegState *rs)
{
	int rval = rs->next_vreg;

	rs->next_vreg += 2;

	return rval;
}

