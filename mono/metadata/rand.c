/*
 * rand.c: System.Security.Cryptography.RNGCryptoServiceProvider support
 *
 * Authors:
 *      Mark Crichton (crichton@gimp.org)
 *      Patrik Torstensson (p@rxc.se)
 *	Sebastien Pouliot (sebastien@ximian.com)
 *
 * Copyright 2001-2003 Ximian, Inc (http://www.ximian.com)
 * Copyright 2004-2009 Novell, Inc (http://www.novell.com)
 */

#include <glib.h>

#include "object.h"
#include "rand.h"
#include "utils/mono-rand.h"

MonoBoolean
ves_icall_System_Security_Cryptography_RNGCryptoServiceProvider_RngOpen (void)
{
	return (MonoBoolean) mono_rand_open ();
}

gpointer
ves_icall_System_Security_Cryptography_RNGCryptoServiceProvider_RngInitialize (MonoArray *seed)
{
	
	return mono_rand_init (seed ? mono_array_addr (seed, guchar, 0) : NULL, seed ? mono_array_length (seed) : 0);
}

gpointer
ves_icall_System_Security_Cryptography_RNGCryptoServiceProvider_RngGetBytes (gpointer handle, MonoArray *arry)
{
	g_assert (arry);
	mono_rand_try_get_bytes (&handle, mono_array_addr (arry, guchar, 0), mono_array_length (arry));
	return handle;
}

void
ves_icall_System_Security_Cryptography_RNGCryptoServiceProvider_RngClose (gpointer handle)
{
	mono_rand_close (handle);
}
