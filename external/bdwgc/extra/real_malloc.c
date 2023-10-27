/*
 * Copyright 1988, 1989 Hans-J. Boehm, Alan J. Demers
 * Copyright (c) 1991-1994 by Xerox Corporation.  All rights reserved.
 *
 * THIS MATERIAL IS PROVIDED AS IS, WITH ABSOLUTELY NO WARRANTY EXPRESSED
 * OR IMPLIED.  ANY USE IS AT YOUR OWN RISK.
 *
 * Permission is hereby granted to use or copy this program
 * for any purpose,  provided the above notices are retained on all copies.
 * Permission to modify the code and to distribute modified code is granted,
 * provided the above notices are retained, and a notice that the code was
 * modified is included with the above copyright notice.
 */

# ifdef HAVE_CONFIG_H
#   include "config.h"
# endif

# ifdef PCR
/*
 * This definition should go in its own file that includes no other
 * header files.  Otherwise, we risk not getting the underlying system
 * malloc.
 */
# define PCR_NO_RENAME
# include <stdlib.h>

void * real_malloc(size_t size)
{
    return(malloc(size));
}

# else

extern int GC_quiet;
        /* ANSI C doesn't allow translation units to be empty.  */
        /* So we guarantee this one is nonempty.                */

#endif /* PCR */
