/*
 * Common/shared macros and routines.
 *
 * This file contains macros of the form
 *
 *   mph_return_if_TYPE_overflow(val);
 *
 * Which tests `val' for a TYPE underflow/overflow (that is, is `val' within
 * the range for TYPE?).  If `val' can't fit in TYPE, errno is set to
 * EOVERFLOW, and `return -1' is executed (which is why it's a macro).
 *
 * Assumptions:
 *
 * I'm working from GLibc, so that's the basis for my assumptions.  They may
 * not be completely portable, in which case I'll need to fix my assumptions.
 * :-(
 *
 * See the typedefs for type size assumptions.  These typedefs *must* be kept
 * in sync with the types used in Mono.Posix.dll.
 */

#ifndef INC_mph_H
#define INC_mph_H

#include <stdint.h>  /* for SIZE_MAX */
#include <limits.h>
#include <glib/gtypes.h>

#ifdef _LARGEFILE64_SOURCE
#define MPH_USE_64_API
#endif

#if __APPLE__ || __BSD__
#define MPH_ON_BSD
#endif

typedef    gint64 mph_blkcnt_t;
typedef    gint64 mph_blksize_t;
typedef   guint64 mph_dev_t;
typedef   guint64 mph_ino_t;
typedef   guint64 mph_nlink_t;
typedef    gint64 mph_off_t;
typedef   guint64 mph_size_t;
typedef    gint64 mph_ssize_t;
typedef    gint32 mph_pid_t;
typedef   guint32 mph_gid_t;
typedef   guint32 mph_uid_t;
typedef    gint64 mph_time_t;
typedef    gint64 mph_clock_t;

#define mph_have_long_overflow(var) ((var) > LONG_MAX || (var) < LONG_MIN)

#define mph_return_val_if_long_overflow(var, ret) G_STMT_START{ \
	if (mph_have_long_overflow(var)) { \
		errno = EOVERFLOW; \
		return ret; \
	}}G_STMT_END

#define mph_return_if_long_overflow(var) mph_return_val_if_long_overflow(var, -1)

#define mph_have_ulong_overflow(var) ((var) > ULONG_MAX)
#define mph_return_val_if_ulong_overflow(var, ret) G_STMT_START{ \
	if (mph_have_ulong_overflow(var)) { \
		errno = EOVERFLOW; \
		return ret; \
	}}G_STMT_END

#define mph_return_if_ulong_overflow(var) mph_return_val_if_ulong_overflow(var, -1)

#ifdef SIZE_MAX
#define mph_have_size_t_overflow(var) ((var) > SIZE_MAX)
#define mph_return_val_if_size_t_overflow(var, ret) G_STMT_START{ \
	if (mph_have_size_t_overflow(var)) { \
		errno = EOVERFLOW; \
		return ret; \
	}}G_STMT_END
#define mph_return_if_size_t_overflow(var) mph_return_val_if_size_t_overflow(var, -1)
#else
#define mph_have_size_t_overflow(var) mph_have_ulong_overflow(var)
#define mph_return_if_size_t_overflow(var) mph_return_if_ulong_overflow(var)
#define mph_return_val_if_size_t_overflow(var, ret) mph_return_if_ulong_overflow(var, ret)
#endif

#define mph_return_if_off_t_overflow(var) mph_return_if_long_overflow(var)
#define mph_return_if_ssize_t_overflow(var) mph_return_if_long_overflow(var)
#define mph_return_if_time_t_overflow(var) mph_return_if_long_overflow(var)

#endif /* ndef INC_mph_H */

/*
 * vim: noexpandtab
 */
