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
	g_free (rs->iassign);
	if (rs->iassign != rs->fassign)
		g_free (rs->fassign);
	g_free (rs);
}

void
mono_regstate_reset (MonoRegState *rs) {
	rs->next_vireg = MONO_MAX_IREGS;
	rs->next_vfreg = MONO_MAX_FREGS;
}

void
mono_regstate_assign (MonoRegState *rs) {
	int i;
	rs->max_ireg = -1;

	if (rs->next_vireg != rs->iassign_size) {
		g_free (rs->iassign);
		rs->iassign = g_malloc (MAX (MONO_MAX_IREGS, rs->next_vireg) * sizeof (int));
		rs->iassign_size = rs->next_vireg;
	}

	for (i = 0; i < MONO_MAX_IREGS; ++i) {
		rs->iassign [i] = i;
		rs->isymbolic [i] = 0;
	}

	/* iassign can be very large so it needs to be initialized by the caller */
	memset (rs->iassign, -1, MONO_MAX_IREGS);

	if (rs->next_vfreg != rs->fassign_size) {
		g_free (rs->fassign);
		rs->fassign = g_malloc (MAX (MONO_MAX_FREGS, rs->next_vfreg) * sizeof (int));
		rs->fassign_size = rs->next_vfreg;
	}

	for (i = 0; i < MONO_MAX_FREGS; ++i) {
		rs->fassign [i] = i;
		rs->fsymbolic [i] = 0;
	}

	if (rs->next_vfreg > MONO_MAX_FREGS)
		memset (rs->fassign, -1, sizeof (rs->fassign [0]) * rs->next_vfreg);
}

int
mono_regstate_alloc_int (MonoRegState *rs, regmask_t allow)
{
	int i;
	regmask_t mask = allow & rs->ifree_mask;
	for (i = 0; i < MONO_MAX_IREGS; ++i) {
		if (mask & ((regmask_t)1 << i)) {
			rs->ifree_mask &= ~ ((regmask_t)1 << i);
			rs->max_ireg = MAX (rs->max_ireg, i);
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
	int rval = rs->next_vireg;

	rs->next_vireg += 2;

	return rval;
}

