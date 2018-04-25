#ifndef SYS_UIO_H
#define SYS_UIO_H

#include <glib.h>

#include <sys/uio.h>

#include "map.h"
#include "mph.h"

G_BEGIN_DECLS

MPH_INTERNAL struct iovec*
_mph_from_iovec_array (struct Mono_Posix_Iovec *iov, gint32 iovcnt);

G_END_DECLS

#endif /* SYS_UIO_H */
