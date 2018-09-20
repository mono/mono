//
//  openssl-error.c
//  MonoOpenSSL
//
//  Created by Martin Baulig on 6/19/16.
//  Copyright © 2016 Xamarin. All rights reserved.
//

#include "openssl-error.h"
#include "openssl-util.h"
#include <openssl/err.h>
#include <assert.h>

MONO_API int
mono_tls_error_peek_error (void)
{
	return ERR_peek_error ();
}

MONO_API int
mono_tls_error_get_error (void)
{
	return ERR_get_error ();
}

MONO_API int
mono_tls_error_peek_error_line (const char **file, int *line)
{
	return ERR_peek_error_line (file, line);
}

MONO_API int
mono_tls_error_get_error_line (const char **file, int *line)
{
	return ERR_get_error_line (file, line);
}

MONO_API void
mono_tls_error_clear_error (void)
{
	ERR_clear_error ();
}

MONO_API void
mono_tls_error_get_error_string_n (int error, char *buf, int len)
{
	ERR_error_string_n (error, buf, len);
}
 
MONO_API int
mono_tls_error_get_reason (int error)
{
    const uint32_t lib = ERR_GET_LIB (error);
    const uint32_t reason = ERR_GET_REASON (error);

    if (lib == ERR_LIB_SYS)
        return -1;

    switch (reason) {
        case SSL_R_NO_RENEGOTIATION:
            return 100;
        default:
            return 0;
    }

    return reason;
}
