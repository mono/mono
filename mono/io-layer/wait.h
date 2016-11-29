/*
 * wait.h:  wait for handles to become signalled
 *
 * Author:
 *	Dick Porter (dick@ximian.com)
 *
 * (C) 2002 Ximian, Inc.
 */

#ifndef _WAPI_WAIT_H_
#define _WAPI_WAIT_H_

#include "mono/io-layer/status.h"
#include "mono/metadata/w32handle.h"

G_BEGIN_DECLS

#define WAIT_FAILED		0xFFFFFFFF
#define WAIT_OBJECT_0		((STATUS_WAIT_0) +0)
#define WAIT_ABANDONED		((STATUS_ABANDONED_WAIT_0) +0)
#define WAIT_ABANDONED_0	((STATUS_ABANDONED_WAIT_0) +0)

/* WAIT_TIMEOUT is also defined in error.h. Luckily it's the same value */
#define WAIT_TIMEOUT		STATUS_TIMEOUT
#define WAIT_IO_COMPLETION	STATUS_USER_APC

G_END_DECLS
#endif /* _WAPI_WAIT_H_ */
