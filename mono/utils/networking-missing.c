/*
 * networking-missing.c: Implements missing standard socket functions.
 *
 * Author:
 *	Rodrigo Kumpera (kumpera@gmail.com)
 *
 * (C) 2015 Xamarin
 */

#include <mono/utils/networking.h>
#include <glib.h>

#ifdef HAVE_NETDB_H
#include <netdb.h>
#endif

#ifndef HAVE_INET_PTON

int
inet_pton (int family, const char *address, void *inaddrp)
{
	if (family == AF_INET) {
#ifdef HAVE_INET_ATON
		struct in_addr inaddr;
		
		if (!inet_aton (address, &inaddr))
			return 0;
		
		memcpy (inaddrp, &inaddr, sizeof (struct in_addr));
		return 1;
#else
		/* assume the system has inet_addr(), if it doesn't
		   have that we're pretty much screwed... */
		guint32 inaddr;
		
		if (!strcmp (address, "255.255.255.255")) {
			/* special-case hack */
			inaddr = 0xffffffff;
		} else {
			inaddr = inet_addr (address);
#ifndef INADDR_NONE
#define INADDR_NONE ((in_addr_t) -1)
#endif
			if (inaddr == INADDR_NONE)
				return 0;
		}
		
		memcpy (inaddrp, &inaddr, sizeof (guint32));
		return 1;
#endif /* HAVE_INET_ATON */
	}
	
	return -1;
}

#else /* !HAVE_INET_PTON */

#ifdef _MSC_VER
// Quiet Visual Studio linker warning, LNK4221, in cases when this source file intentional ends up empty.
void __mono_win32_networking_missing_lnk4221(void) {}
#endif
#endif /* !HAVE_INET_PTON */
